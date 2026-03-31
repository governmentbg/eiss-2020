using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.IndexService;
using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class ElasticIndexService : BaseMQService, IElasticIndexService
    {
        private readonly IElasticService elasticService;

        public ElasticIndexService(
                IRepository _repo,
                IElasticService _elasticService,
                ILogger<ElasticIndexService> _logger)
        {
            repo = _repo;
            elasticService = _elasticService;
            logger = _logger;
            this.IntegrationTypeId = NomenclatureConstants.IntegrationTypes.ElasticService;
        }

        //protected override IEnumerable<MQEpep> FetchHighPriorityItems(int fetchCount)
        //{
        //    return null;
        //}

        protected override  Task<bool> InitChanel()
        {
            return  Task.FromResult(true);
        }

        protected override async Task CloseChanel()
        {
            await Task.Yield();
        }

        protected override async Task SendMQ(MQEpep mq)
        {
            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                case EpepConstants.Methods.Update:
                    await Add(mq);
                    break;
                case EpepConstants.Methods.Delete:
                    await Delete(mq);
                    break;
                case EpepConstants.Methods.Manage:
                    await Manage(mq);
                    break;
                case "select":
                    await Select(mq);
                    break;
                default:
                    break;
            }
        }

        private async Task Select(MQEpep mq)
        {
            var filter = JsonConvert.DeserializeObject<SearchFilterModel>(System.Text.Encoding.UTF8.GetString(mq.Content));
            filter.Skip = 0;
            filter.Take = 1;
            var result = (await elasticService.Search(filter))?.Items?.FirstOrDefault();
            if (result != null)
            {
                mq.ErrorDescription = $"id={result.ActId} {result.ActNumber}/{result.ActDeclaredDate:dd.MM.yyyy} {result.Highlights.FirstOrDefault()}";
            }
            else
            {
                mq.ErrorDescription = "No results";
            }
            UpdateMQ(mq, true);
        }

        private async Task Add(MQEpep mq)
        {
            var actKey = getKey(mq.SourceType, mq.SourceId);
            bool newAct = true;

            if (!string.IsNullOrEmpty(actKey))
            {
                newAct = false;
                var delResponse = await elasticService.ManageIndex(new IndexRequestModel()
                {
                    Method = EpepConstants.Methods.Delete,
                    ActId = (int)mq.SourceId,
                    IntegrationActKey = actKey
                });

                if (!delResponse.SendOk)
                {
                    SetErrorToMQ(mq, EpepConstants.IntegrationStates.DataContentError, delResponse.ErrorMessage);
                    return;
                }
            }
            else
            {
                actKey = Guid.NewGuid().ToString();
            }
            var addResponse = await elasticService.ManageIndex(new IndexRequestModel()
            {
                Method = EpepConstants.Methods.Add,
                ActId = (int)mq.SourceId,
                IntegrationActKey = actKey
            });
            if (addResponse.SendOk)
            {
                if (newAct)
                {
                    AddIntegrationKey(mq, Guid.Parse(actKey), false);
                }
                else
                {
                    UpdateMQ(mq, true);
                }
            }
            else
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.TransferError, addResponse.ErrorMessage);
            }
        }

        private async Task Delete(MQEpep mq)
        {
            var actKey = getKey(mq.SourceType, mq.SourceId);
            var delResponse = await elasticService.ManageIndex(new IndexRequestModel()
            {
                Method = EpepConstants.Methods.Delete,
                ActId = (int)mq.SourceId,
                IntegrationActKey = actKey
            });

            if (delResponse.SendOk)
            {
                UpdateMQ(mq, true);
                RemoveIntegrationKeys(mq);
            }
            else
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.DataContentError, delResponse.ErrorMessage);
                return;
            }
        }

        private async Task Manage(MQEpep mq)
        {
            var manageResponse = await elasticService.ManageIndex(new IndexRequestModel()
            {
                Method = EpepConstants.Methods.Manage
            });

            if (manageResponse.SendOk)
            {
                mq.ErrorDescription = manageResponse.ErrorMessage;
                UpdateMQ(mq, true);
            }
            else
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.DataContentError, manageResponse.ErrorMessage);
                return;
            }
        }
    }

}



