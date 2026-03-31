// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.Integrations.EISS;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class EissProcessService : BaseMQService, IEissProcessService
    {
        private readonly ICaseLifecycleService lifecycleService;
        private readonly ICaseDeadlineService caseDeadlineService;
        private readonly IWorkNotificationService workNotificationService;
        private readonly ICaseLoadIndexService caseLoadIndexService;
        private readonly ICaseService caseService;
        public EissProcessService(
             IRepository _repo,
             ILogger<EissProcessService> _logger,
             ICaseLifecycleService _lifecycleService,
             ICaseDeadlineService _caseDeadlineService,
             IWorkNotificationService _workNotificationService,
             ICaseLoadIndexService _caseLoadIndexService,
             ICaseService _caseService
            )
        {
            this.repo = _repo;
            this.logger = _logger;

            lifecycleService = _lifecycleService;
            caseDeadlineService = _caseDeadlineService;
            workNotificationService = _workNotificationService;
            caseLoadIndexService = _caseLoadIndexService;
            caseService = _caseService;

            IntegrationTypeId = NomenclatureConstants.IntegrationTypes.EissProcess;
        }

        protected override Task<bool> InitChanel() => Task.FromResult(true);

        protected override async Task SendMQ(MQEpep mq)
        {
            this.startTime = DateTime.Now;
            try
            {
                switch (mq.TargetClassName)
                {
                    case NomenclatureConstants.EissProcessTypes.CaseSave:
                        await ProcessCaseSave(mq);
                        break;
                    case NomenclatureConstants.EissProcessTypes.ActDeclared:
                        await ProcessActDeclared(mq);
                        break;
                }
            }
            catch (Exception ex)
            {
                mq.ErrorDescription = ex.Message;
                UpdateMQ(mq, false);
            }
        }

        async Task ProcessCaseSave(MQEpep mq)
        {
            int caseId = (int)mq.SourceId;
            using (var transaction = caseDeadlineService.BeginTransaction())
            {
                var caseModel = await repo.AllReadonly<Case>()
                                          .Where(x => x.Id == caseId)
                                          .Select(x => new
                                          {
                                              x.RegDate,
                                              IsFastProcess = x.IsFastProcess ?? false,
                                              IsReadSimilarCases = x.IsReadSimilarCases ?? false
                                          })
                                          .FirstOrDefaultAsync();

                await workNotificationService.TurnOffNotificationsForFilingClaimFastProcess(caseId);
                await workNotificationService.SaveNotificationsForNewCaseHigherInstanceWith0604_1_2FastProcess(caseId);
                await workNotificationService.SaveNotificationsForNewCaseHigherInstanceWithout0604_1_2FastProcess(caseId);
                await workNotificationService.TurnOffNotificationsForLackSubmittedObjectionFastProcess(caseId);

                if (caseModel.IsFastProcess && !caseModel.IsReadSimilarCases)
                {
                    await caseService.GetSimilarCase(caseId, caseModel.RegDate);
                }

                transaction.Commit();
            }

            UpdateMQ(mq, true);
        }

        async Task ProcessActDeclared(MQEpep mq)
        {
            using (var transaction = caseDeadlineService.BeginTransaction())
            {
                var actModel = await repo.All<CaseSessionAct>().Where(x => x.Id == (int)mq.SourceId).FirstOrDefaultAsync();
                var contextModel = JsonConvert.DeserializeObject<EissProcessActDeclaredVM>(Encoding.UTF8.GetString(mq.Content));
                var caseCase = await repo.GetByIdAsync<Case>(actModel.CaseId);
                
                caseDeadlineService.SetImpersonatedUser(contextModel.LastUserIdCompleted);
                caseDeadlineService.SetImpersonatedCourt(actModel.CourtId ?? 0);
                caseDeadlineService.DeadLineMotive(actModel);
                caseDeadlineService.DeadLineCompleteOnSessionAct(actModel);

                if (caseCase.IsFastProcess ?? false)
                {
                    await caseDeadlineService.CompleteExpiredCaseDeadlineFastProcess(actModel.CaseId ?? 0, actModel.CaseId ?? 0, SourceTypeSelectVM.Case, NomenclatureConstants.DeadlineType.TakingActionFastProcess);
                    await caseDeadlineService.CompleteExpiredCaseDeadlineFastProcess(actModel.CaseId ?? 0, null, SourceTypeSelectVM.Document, NomenclatureConstants.DeadlineType.MissingActForCompliantDocumentFastProcess);
                    await caseDeadlineService.CompleteExpiredMissingActForCompliantDocumentFastProcess(actModel.Id, false);
                    await workNotificationService.TurnOfCompliantDocumentCaseFastProcess(actModel.Id, false);
                }

                // Автоматизиране на статус - решено
                if ((caseCase.CaseStateId == NomenclatureConstants.CaseState.AnnouncedForResolution) && (actModel.ActTypeId == NomenclatureConstants.ActType.Answer))
                {
                    caseCase.CaseStateId = NomenclatureConstants.CaseState.Resolution;
                    caseCase.DateWrt = DateTime.Now;
                    caseCase.UserId = contextModel.LastUserIdCompleted;
                    //repo.Update(caseCase);
                    caseDeadlineService.DeadLineOnCase(caseCase);
                }

                await repo.SaveChangesAsync();

                if (caseCase.IsFastProcess ?? false)
                {
                    await workNotificationService.SaveNotificationsForN3(actModel.Id);
                    await workNotificationService.SaveNotificationsForDecreeRecusalSelfRecusalFastProcessByActId(actModel.Id);
                    await workNotificationService.SaveNotificationsForActionTakenCourtOfficerDeclatActFastProcess(actModel.Id);
                    await workNotificationService.TurnOffForN1(actModel.Id);
                    await workNotificationService.SaveNotificationsForN23(actModel.Id);
                }

                await workNotificationService.SaveNotificationsForN11(actModel.Id);
                await workNotificationService.SaveNotificationsForActInforcedAnotherInstanceFastProcess(actModel.Id);

                //caseLoadIndexService.CaseLoadIndexAutomationElementGroupe_SRA_SaveData(actModel.CaseSessionId);

                transaction.Commit();
            }

            UpdateMQ(mq, true);
        }
    }
}
