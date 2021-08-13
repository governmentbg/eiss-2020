using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.IndexService;
using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class ElasticIndexService : BaseMQService, IElasticIndexService
    {
        private readonly IElasticService elasticService;

        public ElasticIndexService(
                IRepository _repo,
                IElasticService _elasticService,
                ILogger<CubipsaService> _logger)
        {
            repo = _repo;
            elasticService = _elasticService;
            logger = _logger;
            this.IntegrationTypeId = NomenclatureConstants.IntegrationTypes.ElasticService;
        }
        protected override async Task<bool> InitChanel()
        {
            //var ttt = elasticService.Search(45, "повик");
            return true;
        }

        protected override async Task CloseChanel()
        {
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
                default:
                    break;
            }
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
    }

}



