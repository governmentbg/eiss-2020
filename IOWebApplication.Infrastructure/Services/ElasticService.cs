// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.IndexService;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Services
{
    public class ElasticService : IElasticService
    {

        private readonly IRepository repo;
        private readonly ICdnService cdnService;
        private readonly IConfiguration config;
        private ElasticClient elClient;

        private readonly string elasticSearchUri;
        private readonly string elasticSearchIndex;

        public ElasticService(IRepository _repo, ICdnService _cdnService, IConfiguration _config)
        {
            repo = _repo;
            cdnService = _cdnService;
            config = _config;

            elasticSearchUri = config.GetValue<string>("ElasticSearch:URI");
            elasticSearchIndex = config.GetValue<string>("ElasticSearch:Index");
        }


        public async Task<IndexResponseModel> ManageIndex(IndexRequestModel model)
        {
            switch (model.Method)
            {
                case EpepConstants.Methods.Add:
                    return await AppendToIndex(model);
                case EpepConstants.Methods.Delete:
                    return DeleteFromIndex(model);
                default:
                    return new IndexResponseModel()
                    {
                        SendOk = false
                    };
            }
        }

        private async Task<IndexResponseModel> AppendToIndex(IndexRequestModel model)
        {
            var fileModel = cdnService.Select(SourceTypeSelectVM.CaseSessionActDepersonalizedBlank, model.ActId.ToString()).FirstOrDefault();
            if (fileModel == null)
            {
                return new IndexResponseModel()
                {
                    SendOk = false,
                    ErrorMessage = "Няма намерен файл."
                };
            }
            CdnDownloadResult fileContent = null;
            var actModel = repo.GetById<CaseSessionAct>(model.ActId);
            fileContent = await cdnService.MongoCdn_Download(fileModel.MongoFileId);

            if (actModel == null)
            {
                return new IndexResponseModel()
                {
                    SendOk = false,
                    ErrorMessage = "Ненамерен акт."
                };
            }

            IndexDocumentModel docModel = new IndexDocumentModel()
            {
                Id = model.IntegrationActKey,
                CourtId = actModel.CourtId.Value,
                CaseId = actModel.CaseId.Value,
                DateUploaded = fileModel.DateUploaded.ToString("dd.MM.yyyy HH:mm:ss"),
                CaseSessionActId = model.ActId,
                FileName = fileModel.FileName,
                Content = fileContent.FileContentBase64
            };


            return SendToElasticSearch(docModel);
        }


        private IndexResponseModel DeleteFromIndex(IndexRequestModel model)
        {
            var client = initClient();

            try
            {
                var responce = client.Delete<IndexDocumentModel>(model.IntegrationActKey);
                if (responce != null)
                {
                    return new IndexResponseModel()
                    {
                        SendOk = responce.Result == Result.Deleted,
                        Id = responce.Id
                    };
                }
                else
                {
                    return new IndexResponseModel()
                    {
                        SendOk = false,
                        ErrorMessage = "Проблем при изтриване"
                    };
                }
            }
            catch (Exception ex)
            {
                return new IndexResponseModel()
                {
                    SendOk = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private IndexResponseModel SendToElasticSearch(IndexDocumentModel model)
        {
            var client = initClient();

            string pipelineName = "attachments";

            var plResponse = client.Ingest.PutPipeline(pipelineName, p => p
               .Description("Document attachment pipeline")
               .Processors(pr => pr
                   .Attachment<IndexDocumentModel>(a => a
                       .Field(f => f.Content)
                       .TargetField(f => f.Attachment)
                   )
                   .Remove<IndexDocumentModel>(r => r
                       .Field(f => f.Field(d => d.Content))
                   )
               )
            );

            try
            {
                var responce = client.Index(model, i => i.Pipeline(pipelineName));
                if (responce != null)
                {
                    return new IndexResponseModel()
                    {
                        SendOk = responce.Result == Result.Created,
                        Id = responce.Id
                    };
                }
                else
                {
                    return new IndexResponseModel()
                    {
                        SendOk = false,
                        ErrorMessage = "Not sent."
                    };
                }
            }
            catch (Exception ex)
            {
                return new IndexResponseModel()
                {
                    SendOk = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private ElasticClient initClient()
        {
            if (elClient == null)
            {
                var settings = new ConnectionSettings(new Uri(elasticSearchUri))
                  .DefaultIndex(elasticSearchIndex);
                elClient = new ElasticClient(settings);
            }
            return elClient;
        }


        public ICollection<IndexDataModel> Search(int courtId, string query)
        {

            var client = initClient();

            var bytes = System.Text.Encoding.UTF8.GetBytes(query);
            var encoded = System.Text.Encoding.UTF8.GetString(bytes);

            //var eee = client.CreateIndex(elasticSearchIndex);

            var searchResult = client
                 .Search<IndexDocumentModel>(s =>
                     s.Query(q =>
                         q.Match(c => c
                             .Field(p => p.Attachment.Content)
                             .Analyzer("standard")
                             //.CutoffFrequency(0.001)
                             .Query(encoded)
                             .Fuzziness(Fuzziness.AutoLength(2, 6))
                             .Lenient()
                             .FuzzyTranspositions()
                             //.MinimumShouldMatch(2)
                             .Operator(Operator.And)
                             .FuzzyRewrite(MultiTermQueryRewrite.TopTermsBlendedFreqs(10))
                             .Name("match_document_content_query")
                          )
                     )
                     .PostFilter(q => q.Term(p => p.CourtId, courtId))                     
                     .Take(200)
                 .Highlight(h => h
                     .PreTags("<mark>")
                     .PostTags("</mark>")
                     .Encoder(HighlighterEncoder.Html)
                     .Fields(
                         fs => fs
                             .Field(p => p.Attachment.Content)
                             .Type("plain")
                             .ForceSource()
                             .FragmentSize(150)
                             .Fragmenter(HighlighterFragmenter.Span)
                             .NumberOfFragments(3)
                             .NoMatchSize(150)
                     )
                 )
                 );

            return searchResult.Documents.Select(x => new IndexDataModel
            {
                Id = x.Id,
                CaseId = x.CaseId,
                DateUploaded = x.DateUploaded,
                FileName = x.FileName,
                SourceId = x.SourceId,
                SourceType = x.SourceType
                //Content = x.Content
            }).ToList();

        }


        public void Dispose()
        {
            if (elClient != null)
            {
                elClient = null;
            }
        }
    }
}

