using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Services
{
    public partial class MqRecoverService : BaseMQService, IMqRecoverService
    {
        private readonly IMQEpepService mqEpepService;

        public MqRecoverService(
            IRepository _repo,
            ILogger<MqRecoverService> _logger,
            IMQEpepService _mqEpepService,
            IConfiguration configuration)
        {
            repo = _repo;
            logger = _logger;
            IntegrationTypeId = NomenclatureConstants.IntegrationTypes.MqRecover;
            batchSave = false;
            mqEpepService = _mqEpepService;
        }

        protected override Task<bool> InitChanel()
        {
            return Task.FromResult(true);
        }

        protected override async Task CloseChanel()
        {
            await Task.Yield();
        }

        protected override async Task Reconnect()
        {
            if (!await InitChanel())
            {
                return;
            }
        }

        protected override async Task<IEnumerable<MQEpep>> FetchHighPriorityItems(int fetchCount)
        {
            return await Task.FromResult(new List<MQEpep>());
        }


        protected override async Task SendMQ(MQEpep mq)
        {
            DateTime lastDate = DateTime.Now;
            this.currentMqId = mq.Id;

            this.startTime = DateTime.Now;
            await Recover_Data(mq);
        }

        private async Task Recover_Data(MQEpep mq)
        {
            var idList = await repo.All<ID_List>()
                                    .Where(x => x.Remark == null)
                                    .ToListAsync();

            mqEpepService.Set_MqID($"RID:{mq.Id}");

            foreach (var item in idList)
            {
                bool mqResult = true;
                switch (mq.SourceType)
                {
                    case SourceTypeSelectVM.CasePerson:
                        mqResult = await Recover_CasePerson((int)item.Id, mq.MethodName);
                        break;
                    case SourceTypeSelectVM.CaseSessionAct:
                        mqResult = await Recover_CaseSessionAct((int)item.Id, mq.MethodName);
                        break;
                    case SourceTypeSelectVM.CaseSessionActPdf:
                        mqResult = await Recover_CaseSessionActPrivate((int)item.Id, mq.MethodName);
                        break;
                    case SourceTypeSelectVM.CaseSessionActDepersonalized:
                        mqResult = await Recover_CaseSessionActPublic((int)item.Id, mq.MethodName);
                        break;
                    default:
                        break;
                }

                if (mqResult)
                {
                    item.Remark = $"OK; {DateTime.Now}";
                }
                else
                {
                    item.Remark = $"Failed; {DateTime.Now}";
                }
                await repo.SaveChangesAsync();
            }

            mq.ErrorDescription = $"OK:{idList.Where(x => x.Remark.Contains("OK")).Count()} от {idList.Count}";

            UpdateMQ(mq, true);

        }

        private async Task<bool> Recover_CasePerson(int casePersonId, string methodName)
        {
            CasePerson model = await repo.AllReadonly<CasePerson>().Where(x => x.Id == casePersonId).FirstOrDefaultAsync();
            return await mqEpepService.AppendCasePerson(model, EpepConstants.Methods.GetMethod(methodName));
        }

        private async Task<bool> Recover_CaseSessionAct(int caseSessionActId, string methodName)
        {
            CaseSessionAct model = await repo.AllReadonly<CaseSessionAct>().Where(x => x.Id == caseSessionActId).FirstOrDefaultAsync();
            return await mqEpepService.AppendCaseSessionAct(model, EpepConstants.Methods.GetMethod(methodName));
        }

        private async Task<bool> Recover_CaseSessionActPrivate(int caseSessionActId, string methodName)
        {
            DateTime? actDeclaredDate = await repo.GetPropByIdAsync<CaseSessionAct, DateTime?>(x => x.Id == caseSessionActId, x => x.ActDeclaredDate);
            if (actDeclaredDate == null && methodName != EpepConstants.Methods.Delete)
            {
                return false;
            }
            return await mqEpepService.AppendCaseSessionAct_Private(caseSessionActId, EpepConstants.Methods.GetMethod(methodName));
        }
        private async Task<bool> Recover_CaseSessionActPublic(int caseSessionActId, string methodName)
        {
            DateTime? actDepersonalizedDate = await repo.GetPropByIdAsync<CaseSessionAct, DateTime?>(x => x.Id == caseSessionActId, x => x.DepersonalizeEndDate);
            if (actDepersonalizedDate == null && methodName != EpepConstants.Methods.Delete)
            {
                return false;
            }
            return await mqEpepService.AppendCaseSessionAct_Public(caseSessionActId, EpepConstants.Methods.GetMethod(methodName));
        }
    }
}



