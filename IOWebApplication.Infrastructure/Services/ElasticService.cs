// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Elasticsearch.Net;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.IndexService;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.EntityFrameworkCore;
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
        private ElasticClient elClient;

        private readonly string elasticSearchUri;
        private readonly string elasticSearchIndex;

        public ElasticService(IRepository _repo, ICdnService _cdnService, IConfiguration _config)
        {
            repo = _repo;
            cdnService = _cdnService;

            elasticSearchUri = _config.GetValue<string>("ElasticSearch:URI");
            elasticSearchIndex = _config.GetValue<string>("ElasticSearch:Index");
        }


        public async Task<IndexResponseModel> ManageIndex(IndexRequestModel model)
        {
            switch (model.Method)
            {
                case EpepConstants.Methods.Add:
                    return await AppendToIndex(model);
                case EpepConstants.Methods.Delete:
                    return DeleteFromIndex(model);
                case EpepConstants.Methods.Manage:
                    return new IndexResponseModel()
                    {
                        SendOk = true,
                        ErrorMessage = await CreateIndex()
                    };
                default:
                    return new IndexResponseModel()
                    {
                        SendOk = false
                    };
            }
        }

        private async Task<IndexResponseModel> AppendToIndex(IndexRequestModel model)
        {
            var fileModel = cdnService.Select(SourceTypeSelectVM.CaseSessionActDepersonalized, model.ActId.ToString()).FirstOrDefault();
            if (fileModel == null)
            {
                return new IndexResponseModel()
                {
                    SendOk = false,
                    ErrorMessage = "Няма намерен файл."
                };
            }
            //CdnDownloadResult fileContent = null;
            var actModel = repo.AllReadonly<CaseSessionAct>()
                                   .Include(x => x.Case)
                                   .ThenInclude(x => x.CaseLawUnits)
                                   .Where(x => x.Id == model.ActId)
                                   .Select(x => new
                                   {
                                       x.Case.CourtId,
                                       CaseId = x.Case.Id,
                                       x.Case.RegNumber,
                                       x.Case.CaseGroupId,
                                       x.Case.CaseTypeId,
                                       ActNumber = x.RegNumber,
                                       x.ActDeclaredDate,
                                       x.ActTypeId,
                                       x.Case.OtdelenieId,
                                       x.Case.JudicalCompositionId,
                                       x.IsFinalDoc,
                                       JudgeReporterId = x.Case.CaseLawUnits.Where(r => r.CaseSessionId == x.CaseSessionId
                                                                        && r.DateTo == null
                                                                        && r.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                                        .Select(r => r.LawUnitId).FirstOrDefault()
                                   }).FirstOrDefault();

            var fileContent = await cdnService.MongoCdn_Download(fileModel.MongoFileId);

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
                CourtId = actModel.CourtId,
                CaseId = actModel.CaseId,
                CaseNumber = actModel.RegNumber,
                CaseGroupId = actModel.CaseGroupId,
                CaseTypeId = actModel.CaseTypeId,
                OtdelenieId = actModel.OtdelenieId,
                JudicalCompositionId = actModel.JudicalCompositionId,
                ActTypeId = actModel.ActTypeId,
                ActNumber = actModel.ActNumber,
                IsFinalDoc = actModel.IsFinalDoc,
                JudgeReporterId = actModel.JudgeReporterId,
                ActDeclaredDate = actModel.ActDeclaredDate.Value,
                DateUploaded = fileModel.DateUploaded,
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
                        ErrorMessage = getErrorMessageFromResponse(responce),
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
                       .IndexedCharacters(-1)
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
                    if (responce.IsValid)
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
                            Id = null,
                            ErrorMessage = getErrorMessageFromResponse(responce)
                        };
                    }
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

        private string getErrorMessageFromResponse(ResponseBase response)
        {
            if (response.IsValid)
            {
                return string.Empty;
            }
            string result = "";
            if (response.OriginalException != null)
            {
                result = $"{response.OriginalException.Message} ;";
            }
            result += response.DebugInformation;

            if (result.Length > 250)
            {
                result = result.Substring(0, result.IndexOf("# OriginalException"));
            }
            return result;
        }

        private ElasticClient initClient()
        {
            if (elClient == null)
            {
                var strURIs = elasticSearchUri.Split(',', StringSplitOptions.RemoveEmptyEntries);
                var URIs = strURIs.Select(x => new Uri(x)).ToArray();
                ConnectionSettings settings;
                if (URIs.Length == 1)
                {
                    settings = new ConnectionSettings(URIs[0])
                        .DefaultIndex(elasticSearchIndex);
                    //.DisableDirectStreaming(true);
                }
                else
                {
                    var pool = new StaticConnectionPool(URIs);
                    settings = new ConnectionSettings(pool)
                        .DefaultIndex(elasticSearchIndex);
                }


                elClient = new ElasticClient(settings);
            }
            return elClient;
        }


        public async Task<SearchResponseModel> Search(SearchFilterModel filter)
        {
            int maxResults = 10000;
            var client = initClient();

            var bytes = System.Text.Encoding.UTF8.GetBytes(filter.Query?.ToLower());
            var encodedQuery = System.Text.Encoding.UTF8.GetString(bytes);

            var dateFrom = filter.ActDateFrom ?? new DateTime(1900, 1, 1);
            var dateTo = (filter.ActDateTo ?? new DateTime(2100, 1, 1)).MakeEndDate();
            var mustFilterList = new List<Func<QueryContainerDescriptor<IndexDocumentModel>, QueryContainer>>();
            mustFilterList.Add(x => x.Term(p => p.CourtId, filter.CourtId));
            foreach (var item in encodedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                mustFilterList.Add(w => w.QueryString(wc => wc
                    .DefaultField(p => p.Attachment.Content)
                    .Query(item)
                    .Rewrite(MultiTermQueryRewrite.TopTermsBlendedFreqs(10))
                    .AnalyzeWildcard()
                    .DefaultOperator(Operator.And)
                    ));
            }

            if (!string.IsNullOrEmpty(filter.CaseGroupIds))
            {
                var _intArr = filter.CaseGroupIds.ToIntArray();
                mustFilterList.Add(x => x.Terms(p => p.Field(f => f.CaseGroupId).Terms(_intArr)));
            }
            if (!string.IsNullOrEmpty(filter.CaseTypeIds))
            {
                var _intArr = filter.CaseTypeIds.ToIntArray();
                mustFilterList.Add(x => x.Terms(p => p.Field(f => f.CaseTypeId).Terms(_intArr)));
            }
            if (!string.IsNullOrEmpty(filter.ActTypeIds))
            {
                var _intArr = filter.ActTypeIds.ToIntArray();
                mustFilterList.Add(x => x.Terms(p => p.Field(f => f.ActTypeId).Terms(_intArr)));
            }
            if (filter.JudgeReporterId > 0)
            {
                mustFilterList.Add(x => x.Term(p => p.Field(f => f.JudgeReporterId).Value(filter.JudgeReporterId)));
            }
            if (filter.OtdelenieId > 0)
            {
                mustFilterList.Add(x => x.Term(p => p.Field(f => f.OtdelenieId).Value(filter.OtdelenieId)));
            }
            if (filter.JudicalCompositionId > 0)
            {
                mustFilterList.Add(x => x.Term(p => p.Field(f => f.JudicalCompositionId).Value(filter.JudicalCompositionId)));
            }

            if (filter.IsFinalDoc == true)
            {
                mustFilterList.Add(x => x.Term(p => p.Field(f => f.IsFinalDoc).Value(true)));
            }
            mustFilterList.Add(dr => dr.DateRange(d =>
                            d
                             .Field(f => f.ActDeclaredDate)
                             .GreaterThanOrEquals(dateFrom)
                             .LessThanOrEquals(dateTo)
                             ));


            //var rawQuery = Encoding.UTF8.GetString(alResult.ApiCall.RequestBodyInBytes);

            var searchResult = await client
                 .SearchAsync<IndexDocumentModel>(s => s
                     .Query(q =>
                        q.Bool(b =>
                         b.Must(mustFilterList.AsEnumerable()
                     ))
                        )
                     .TrackTotalHits()
                     .Skip(filter.Skip)
                     .Take((filter.Take < 0) ? maxResults : filter.Take)
                     .Highlight(h => h
                     .PreTags("<mark>")
                     .PostTags("</mark>")
                     .Encoder(HighlighterEncoder.Html)
                     .Fields(
                     fs => fs
                         .Field(p => p.Attachment.Content)
                         .Type(HighlighterType.Plain)
                         .ForceSource()
                         .FragmentSize(100)
                         .Fragmenter(HighlighterFragmenter.Span)
                         .NumberOfFragments(5)
                         .NoMatchSize(100)
                 )
                 )
                 );

            //var countResponse = await client
            //     .SearchAsync<IndexDocumentModel>(s => s
            //         .Query(q =>
            //            q.Bool(b =>
            //                    b.Must(mustFilterList.AsEnumerable())
            //                  )
            //                )
            //         .Skip(0)
            //         .Take(maxResults)
            //         .Fields(f => f.Field(ff => ff.CaseSessionActId))

            //     );


            var data = searchResult.Documents.Select(x => new IndexDataModel
            {
                Id = x.Id,
                CaseId = x.CaseId,
                CaseGroupId = x.CaseGroupId,
                CaseTypeId = x.CaseTypeId,
                CaseNumber = x.CaseNumber,
                JudgeReporterId = x.JudgeReporterId,
                ActTypeId = x.ActTypeId,
                ActNumber = x.ActNumber,
                IsFinalDoc = x.IsFinalDoc,
                ActDeclaredDate = x.ActDeclaredDate,
                DateUploaded = x.DateUploaded,
                CaseSessionActId = x.CaseSessionActId,
                FileName = x.FileName,
                SourceId = x.SourceId,
                SourceType = x.SourceType
                //,
                //Highlights = searchResult.Hits.Where(h => h.Id == x.Id).SelectMany(h => h.Highlight.Values.AsEnumerable())).ToArray()

            }).ToList();




            var result = new List<SearchResultModel>();
            foreach (var dItem in data)
            {
                var _item = new SearchResultModel()
                {
                    ActId = dItem.CaseSessionActId.Value,
                    CaseId = dItem.CaseId.Value,
                    ActDeclaredDate = dItem.ActDeclaredDate,
                    CaseNumber = dItem.CaseNumber,
                    ActNumber = dItem.ActNumber,
                    IsFinalDoc = dItem.IsFinalDoc
                };

                _item.Highlights = searchResult.Hits.Where(x => x.Id == dItem.Id)
                                    .SelectMany(x => x.Highlight.Values)
                                    .FirstOrDefault()?.Select(x => x.Replace("\n", " ").Replace("\t", " ")).ToArray();

                _item.CaseTypeName = repo.GetPropById<CaseType, string>(x => x.Id == dItem.CaseTypeId, x => x.Code);
                _item.ActTypeName = repo.GetPropById<ActType, string>(x => x.Id == dItem.ActTypeId, x => x.Label);
                if (dItem.JudgeReporterId > 0)
                {
                    _item.JudgeReporterName = repo.GetPropById<LawUnit, string>(x => x.Id == dItem.JudgeReporterId, x => x.FullName);
                }
                result.Add(_item);
            }

            return new SearchResponseModel()
            {
                TotalCount = (int)searchResult.Total,
                Items = result.AsQueryable()
            };
        }


        public void Dispose()
        {
            if (elClient != null)
            {
                elClient = null;
            }
        }

        //http://10.240.145.49:5601/app/management/data/index_management/indices

        public async Task<string> CreateIndex()
        {
            var client = initClient();

            var existsResponse = await client.Indices.ExistsAsync(elasticSearchIndex);
            if (existsResponse.Exists && existsResponse.IsValid)
            {
                return $"Index {elasticSearchIndex} allready exists.";
            }

            var createIndexResponse = await client.Indices.CreateAsync(elasticSearchIndex, c => c
                .Map<IndexDocumentModel>(m => m
                    .AutoMap()
                    .Properties(ps => ps
                        .Date(n => n
                            .Name(nn => nn.ActDeclaredDate)
                            .Name(nn => nn.DateUploaded)
                        )
                    )
                )
            .Settings(s => s
                .NumberOfReplicas(2)
                .NumberOfShards(10)
                )
            );


            return $"Index created : {createIndexResponse.Index}";
        }
    }
}

