using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Extensions.HTML;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess;
using IOWebApplication.Infrastructure.Models.Integrations.RNFL;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using Mapster;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class CaseSessionActService : BaseService, ICaseSessionActService
    {
        private readonly ICounterService counterService;
        private readonly ICaseSessionActCoordinationService coordinationService;
        private readonly IWorkTaskService taskService;
        private readonly ICasePersonService casePersonService;
        private readonly ICasePersonLinkService casePersonLinkService;
        private readonly ICaseFastProcessService caseFastProcessService;
        private readonly IMQEpepService mqEpepService;
        private readonly ICaseDeadlineService caseDeadlineService;
        private readonly IWorkNotificationService workNotificationService;
        private readonly ICaseLifecycleService caseLifecycleService;
        private readonly ICaseLoadIndexService caseLoadIndexService;
        private readonly IDocumentRequestService documentRequestService;

        public CaseSessionActService(ILogger<CaseSessionActService> _logger,
                                     IRepository _repo,
                                     IUserContext _userContext,
                                     IWorkTaskService _taskService,
                                     ICaseSessionActCoordinationService _coordinationService,
                                     ICasePersonService _casePersonService,
                                     ICounterService _counterService,
                                     ICaseFastProcessService _caseFastProcessService,
                                     IMQEpepService _mqEpepService,
                                     ICaseDeadlineService _caseDeadlineService,
                                     IWorkNotificationService _workNotificationService,
                                     ICaseLifecycleService _caseLifecycleService,
                                     ICaseLoadIndexService _caseLoadIndexService,
                                     ICasePersonLinkService _casePersonLinkService,
                                     IDocumentRequestService _documentRequestService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            taskService = _taskService;
            counterService = _counterService;
            coordinationService = _coordinationService;
            casePersonService = _casePersonService;
            caseFastProcessService = _caseFastProcessService;
            mqEpepService = _mqEpepService;
            caseDeadlineService = _caseDeadlineService;
            caseLifecycleService = _caseLifecycleService;
            caseLoadIndexService = _caseLoadIndexService;
            casePersonLinkService = _casePersonLinkService;
            documentRequestService = _documentRequestService;
            workNotificationService = _workNotificationService;
        }

        /// <summary>
        /// Извличане на данни за съдебни актове
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <param name="caseId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="showExpired"></param>
        /// <param name="year"></param>
        /// <param name="caseRegNumber"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionActVM> CaseSessionAct_Select(int caseSessionId, int? caseId, DateTime? DateFrom,
             DateTime? DateTo, int? year, string caseRegNumber, bool showExpired = false)
        {

            Expression<Func<CaseSessionAct, bool>> yearSearch = x => true;
            if ((year ?? 0) > 0)
                yearSearch = x => x.Case.RegDate.Year == year;

            Expression<Func<CaseSessionAct, bool>> caseRegnumberSearch = x => true;
            if (!string.IsNullOrEmpty(caseRegNumber))
                caseRegnumberSearch = x => EF.Functions.ILike(x.Case.RegNumber, caseRegNumber.ToCasePaternSearch());

            return repo.AllReadonly<CaseSessionAct>()

                .Where(this.FilterExpireInfo<CaseSessionAct>(showExpired))
                .Where(x => ((caseSessionId > 0) ? (x.CaseSessionId == caseSessionId) : true) &&
                            ((x.RegDate == null) ? true : ((DateFrom != null) ? ((x.RegDate.Value.Date >= (DateFrom ?? DateTime.Now).Date) && (x.RegDate.Value.Date <= (DateTo ?? DateTime.Now).Date)) : true)) &&
                            (((caseId ?? 0) > 0) ? (x.CaseSession.CaseId == caseId) : true) &&
                            ((caseSessionId < 1 && (caseId ?? 0) < 1) ? x.Case.CourtId == userContext.CourtId : true))
                .Where(yearSearch)
                .Where(caseRegnumberSearch)
                //.Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                .Select(x => new CaseSessionActVM()
                {
                    Id = x.Id,
                    CaseSessionId = x.CaseSessionId,
                    CaseId = x.CaseSession.CaseId,
                    CaseSessionLabel = (x.CaseSession != null) ? x.CaseSession.SessionType.Label + "/" + x.CaseSession.DateFrom.ToString("dd.MM.yyyy HH:mm") : string.Empty,
                    CaseLabel = x.Case.RegNumber,
                    ActTypeLabel = (x.ActType != null) ? x.ActType.Label : string.Empty,
                    ActTypeId = x.ActTypeId,
                    //ActResultLabel = (x.ActResult != null) ? x.ActResult.Label : string.Empty,
                    ActStateLabel = (x.ActState != null) ? x.ActState.Label + (x.ActCoordination.Any(c => c.CoordinationDeclaredDate != null) ? " (ОМ)" : string.Empty) : string.Empty,
                    RegNumber = x.RegNumber,
                    RegNumberNew = x.RegNumber,
                    RegDate = x.RegDate.Value,
                    IsFinalDoc = x.IsFinalDoc,
                    DateWrt = x.DateWrt,
                    EcliCode = x.EcliCode,
                    Description = x.Description,
                    ActDeclaredDate = x.ActDeclaredDate,
                    ActInforcedDate = x.ActInforcedDate,
                    ActMotivesDeclaredDate = x.ActMotivesDeclaredDate,
                    ActCoordinationLabel = x.ActCoordination.Any(c => c.CoordinationDeclaredDate != null) ? "ОМ" : string.Empty,
                    HasCorrectedAct = x.CorrectedActId != null
                })
                .AsQueryable();
        }

        /// <summary>
        /// Попълване на данни за нотификации
        /// </summary>
        /// <param name="act">Акт</param>
        /// <returns></returns>
        private async Task FillNotificationPeriodData(CaseSessionActEditVM act)
        {
            List<CaseSessionActPeriodNotification> periods = await repo.AllReadonly<CaseSessionActPeriodNotification>()
                                                                       .Where(x => x.CaseSessionActId == act.Id)
                                                                       .ToListAsync();

            if (periods.Count < 1)
                return;

            foreach (var period in periods)
            {
                switch (period.WorkNotificationTypeId)
                {
                    case NomenclatureConstants.WorkNotificationType.N23:
                        {
                            act.ActNotificationOn = period.NotificationOn;
                            act.ActNotificationDays = period.NotificationDays;
                            act.ActNotificationMonts = period.NotificationMonts;
                            act.ActNotificationWeeks = period.NotificationWeeks;
                            act.ActNotificationDescription = period.Description;
                        }
                        break;
                }
            }
        }

        public async Task<CaseSessionActEditVM> ReadActById(int id)
        {
            var entity = await ReadByIdAsync<CaseSessionAct>(id);
            if (entity == null)
            {
                return null;
            }
            var result = entity.Adapt<CaseSessionActEditVM>();
            result.GenerateExecProcess = entity.GenerateExecProcess ?? false;


            if (string.IsNullOrEmpty(entity.SecretaryUserId))
            {
                var savedLawunitUserIds = await repo.All<CaseSessionActLawunit>()
                                                       .Where(x => x.CaseSessionActId == id && x.JudgeRoleId == NomenclatureConstants.JudgeRole.Secretary)
                                                       .Select(x => x.LawUnitUserId)
                                                       .ToListAsync();
                result.SecretaryUserId_list = string.Join(',', savedLawunitUserIds);
            }
            else
            {
                result.SecretaryUserId_list = entity.SecretaryUserId;
            }
            result.SecretaryUserId = result.SecretaryUserId_list;

            if (!string.IsNullOrEmpty(result.SecretaryUserId_list))
            {
                var userIds = result.SecretaryUserId_list.Split(',', StringSplitOptions.RemoveEmptyEntries);
                var secretaryNames = await repo.AllReadonly<ApplicationUser>()
                                                .Where(x => userIds.Contains(x.Id))
                                                .Select(x => x.LawUnit.FullName)
                                                .ToArrayAsync();

                result.SecretaryUserNamesList = string.Join(", ", secretaryNames);

            }
            result.CorrectedActsIds = await repo.AllReadonly<CaseSessionActCorrection>()
                                                .Where(x => x.CaseSessionActId == id)
                                                .Select(x => x.CorrectedActId.ToString())
                                                .ToArrayAsync();
            result.HasCorrectedAct = result.CorrectedActsIds.Length > 0;
            await FillNotificationPeriodData(result);
            return result;
        }

        /// <summary>
        /// Запис на съдебни актове
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public async Task<SaveResultVM> CaseSessionAct_SaveData(CaseSessionActEditVM viewModel)
        {
            try
            {
                if (!viewModel.HasCorrectedAct)
                {
                    viewModel.CorrectedActsIds = new string[] { };
                }

                //Задължително при промяна на полета да се опишат и в CaseSessionActEditVM !!!!
                var model = viewModel.Adapt<CaseSessionAct>();

                bool isFastProcess = await repo.GetPropByIdAsync<Case, bool>(x => x.Id == model.CaseId, x => x.IsFastProcess ?? false);

                var secretaryList = await makeSecretaryList(model.Id, viewModel.SecretaryUserId_list);

                bool isDifFinishDoc = false;
                if (secretaryList.Count == 1)
                {
                    model.SecretaryUserId = secretaryList.Select(x => x.LawUnitUserId).First();
                }
                else
                {
                    model.SecretaryUserId = null;
                }
                model.ActKindId = model.ActKindId.EmptyToNull();
                model.ActResultId = model.ActResultId.EmptyToNull();
                model.ActComplainResultId = model.ActComplainResultId.NumberEmptyToNull();
                model.ActComplainIndexId = model.ActComplainIndexId.NumberEmptyToNull();
                model.ActISPNReasonId = model.ActISPNReasonId.NumberEmptyToNull();
                model.ActISPNDebtorStateId = model.ActISPNDebtorStateId.NumberEmptyToNull();
                model.RelatedActId = model.RelatedActId.NumberEmptyToNull();
                model.SignJudgeLawUnitId = model.SignJudgeLawUnitId.EmptyToNull().EmptyToNull(0);
                model.CorrectedActId = model.CorrectedActId.NumberEmptyToNull();
                if (!NomenclatureConstants.ActType.HasSignJudge.Contains(model.ActTypeId))
                {
                    model.SignJudgeLawUnitId = null;
                }

                List<int> correctedActs = new List<int>();
                if (viewModel.CorrectedActsIds != null)
                {
                    correctedActs = viewModel.CorrectedActsIds.Select(c => int.Parse(c)).ToList();
                }


                if (model.Id > 0)
                {
                    //Update
                    var saved = await this.ReadByIdAsync<CaseSessionAct>(model.Id);

                    if (saved.DateWrt > model.DateWrt.AddSeconds(1))
                    {
                        return new SaveResultVM()
                        {
                            Result = false,
                            ReloadNeeded = true
                        };
                    }

                    var savedLawunits = await repo.All<CaseSessionActLawunit>()
                                                    .Where(x => x.CaseSessionActId == model.Id && x.JudgeRoleId == NomenclatureConstants.JudgeRole.Secretary)
                                                    .ToListAsync();
                    if (secretaryList.Count <= 1)
                    {
                        repo.DeleteRange(savedLawunits);
                    }
                    else
                    {
                        repo.DeleteRange(savedLawunits.Where(sl => !secretaryList.Any(s => s.LawUnitUserId == sl.LawUnitUserId)));
                        foreach (var newSecretary in secretaryList.Where(s => !savedLawunits.Any(sl => sl.LawUnitUserId == s.LawUnitUserId)))
                        {
                            repo.Add(newSecretary);
                        }
                    }

                    var savedCorrectedActs = await repo.All<CaseSessionActCorrection>().Where(x => x.CaseSessionActId == model.Id).ToListAsync();
                    repo.DeleteRange(savedCorrectedActs);
                    saved.ActsToCorrect = correctedActs.Select(c => new CaseSessionActCorrection
                    {
                        CaseSessionActId = saved.Id,
                        CorrectedActId = c
                    }).ToList();

                    isDifFinishDoc = saved.IsFinalDoc != model.IsFinalDoc;
                    //След регистриране на акта тези данни не се променят
                    if (string.IsNullOrEmpty(model.RegNumber))
                    {
                        saved.CaseId = model.CaseId;
                        saved.CourtId = model.CourtId;
                        saved.CaseSessionId = model.CaseSessionId;
                        saved.ActTypeId = model.ActTypeId;
                        saved.ActKindId = model.ActKindId;
                        saved.ActDirectionId = model.ActDirectionId;
                        saved.RelatedActId = model.RelatedActId;
                        saved.SignJudgeLawUnitId = model.SignJudgeLawUnitId;
                        saved.IsFinalDoc = model.IsFinalDoc;
                        saved.CorrectedActId = model.CorrectedActId;
                        saved.GenerateExecProcess = model.GenerateExecProcess;
                    }
                    else
                    {
                        //След регистрация, ако един път е записан като финализиращ - повече да не може да се премахне
                        if (!saved.IsFinalDoc && model.IsFinalDoc)
                        {
                            saved.IsFinalDoc = model.IsFinalDoc;
                        }
                    }
                    if (saved.ActDeclaredDate == null)
                    {
                        saved.SecretaryUserId = model.SecretaryUserId;
                    }
                    saved.ActResultId = model.ActResultId;
                    saved.ActStateId = model.ActStateId;

                    var appealNotificationSrokChange = saved.AppealNotificationDaysFastProcess != model.AppealNotificationDaysFastProcess.NumberEmptyToNull() ||
                                                       saved.AppealNotificationWeeksFastProcess != model.AppealNotificationWeeksFastProcess.NumberEmptyToNull() ||
                                                       saved.AppealNotificationMontsFastProcess != model.AppealNotificationMontsFastProcess.NumberEmptyToNull();

                    saved.IsReadyForPublish = model.IsReadyForPublish;
                    saved.CanAppeal = model.CanAppeal;
                    saved.AppealNotificationStartFastProcess = model.AppealNotificationStartFastProcess;
                    saved.AppealNotificationDaysFastProcess = model.AppealNotificationDaysFastProcess.NumberEmptyToNull();
                    saved.AppealNotificationWeeksFastProcess = model.AppealNotificationWeeksFastProcess.NumberEmptyToNull();
                    saved.AppealNotificationMontsFastProcess = model.AppealNotificationMontsFastProcess.NumberEmptyToNull();
                    saved.NotificationDays = model.NotificationDays.NumberEmptyToNull();
                    saved.NotificationWeeks = model.NotificationWeeks.NumberEmptyToNull();
                    saved.NotificationMonts = model.NotificationMonts.NumberEmptyToNull();
                    saved.NotificationOn = model.NotificationOn;
                    saved.ActInforcedDate = model.ActInforcedDate;
                    //saved.ActMotivesDeclaredDate = model.ActMotivesDeclaredDate;

                    saved.ActComplainResultId = (model.IsFinalDoc) ? model.ActComplainResultId : null;
                    saved.ActComplainIndexId = model.ActComplainIndexId;
                    saved.ActISPNReasonId = model.ActISPNReasonId;
                    saved.RnflEffectiveImmediately = model.RnflEffectiveImmediately;
                    saved.ActISPNDebtorStateId = model.ActISPNDebtorStateId;
                    saved.TDActForRegistration = model.TDActForRegistration;
                    saved.ActTerm = model.ActTerm;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;

                    if (saved.ActStateId != NomenclatureConstants.SessionActState.ComingIntoForce)
                        saved.ActInforcedDate = null;



                    var lastHistory = await CreateHistoryAsync<CaseSessionAct, CaseSessionActH>(saved, "CaseSessionAct_SaveData.Edit");

                    if (string.IsNullOrEmpty(saved.RegNumber) && !string.IsNullOrEmpty(lastHistory?.RegNumber))
                    {
                        logger.LogError($"ACT_REGNUMBER_CHANGE! ActId={saved.Id}");
                        return new SaveResultVM(false, "Непозволена промяна на данни за акт.");
                    }

                    repo.DeleteRange<CaseSessionActPeriodNotification>(x => x.CaseSessionActId == saved.Id &&
                                                                            x.WorkNotificationTypeId == NomenclatureConstants.WorkNotificationType.N23);
                    if (viewModel.ActNotificationOn ?? false)
                    {
                        repo.Add<CaseSessionActPeriodNotification>(new CaseSessionActPeriodNotification()
                        {
                            CaseSessionActId = saved.Id,
                            WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.N23,
                            NotificationOn = viewModel.ActNotificationOn,
                            NotificationDays = viewModel.ActNotificationDays,
                            NotificationWeeks = viewModel.ActNotificationWeeks,
                            NotificationMonts = viewModel.ActNotificationMonts,
                            Description = viewModel.ActNotificationDescription
                        });
                    }

                    caseDeadlineService.DeadLineMotive(saved);
                    if (appealNotificationSrokChange && isFastProcess)
                    {
                        await workNotificationService.SaveNotificationsForAppealActFastProcessOnActSave(saved, false);
                    }
                    //repo.Update(saved);
                    await repo.SaveChangesAsync();

                    //След регистриране, до постановяване на финализиращ акт може да се вземе ЕКЛИ номер
                    if (!string.IsNullOrEmpty(saved.RegNumber) && saved.ActDate != null && saved.IsFinalDoc && string.IsNullOrEmpty(saved.EcliCode))
                    {
                        if (await GenerateActEcliNumber(saved))
                        {
                            await repo.SaveChangesAsync();
                        }
                    }

                    if (saved.ActDeclaredDate != null)
                    {
                        await mqEpepService.AppendCaseSessionAct(saved, EpepConstants.ServiceMethod.Update);

                        if (isFastProcess)
                        {
                            await workNotificationService.SaveUpdateNotificationsForNoProceduralActionTakenFastProcess(model.Id);
                            await workNotificationService.SaveUpdateNotificationsForN23(model.Id);
                        }
                    }

                    if (model.IsFinalDoc)
                        await caseLifecycleService.CaseLifecycle_CloseInterval(model.CaseId ?? 0, model.Id, model.ActDeclaredDate ?? DateTime.Now);
                    else
                        caseLifecycleService.CaseLifecycle_UndoCloseInterval(model.CaseId ?? 0, model.Id);

                    caseLoadIndexService.EditActAndRecalcCase(model.CaseId ?? 0, model.Id);
                }
                else
                {
                    //Insert
                    if (secretaryList.Count > 1)
                    {

                        foreach (var newSecretary in secretaryList)
                        {
                            model.CaseSessionActLawunits.Add(newSecretary);
                        }
                    }

                    model.ActsToCorrect = correctedActs.Select(c => new CaseSessionActCorrection
                    {
                        CorrectedActId = c
                    }).ToList();


                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;

                    if (viewModel.ActNotificationOn ?? false)
                    {
                        model.PeriodNotifications.Add(new CaseSessionActPeriodNotification()
                        {
                            WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.N23,
                            NotificationOn = viewModel.ActNotificationOn,
                            NotificationDays = viewModel.ActNotificationDays,
                            NotificationWeeks = viewModel.ActNotificationWeeks,
                            NotificationMonts = viewModel.ActNotificationMonts,
                            Description = viewModel.ActNotificationDescription
                        });
                    }

                    await CreateHistoryAsync<CaseSessionAct, CaseSessionActH>(model, "CaseSessionAct_SaveData.Add");
                    repo.Add<CaseSessionAct>(model);
                    await repo.SaveChangesAsync();

                    caseDeadlineService.DeadLineMotive(model);
                    await repo.SaveChangesAsync();
                    viewModel.Id = model.Id;
                }

                caseLoadIndexService.CaseLoadIndexAutomationElementGroupe_SRA_SaveData(model.CaseSessionId);

                if (model.ActInforcedDate != null && isFastProcess)
                {
                    await workNotificationService.SaveNotificationsForActInforcedAnotherInstanceFastProcess(model.Id);
                }

                return new SaveResultVM(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на акт Id={viewModel.Id}");
                return new SaveResultVM(false);
            }
        }

        private async Task<List<CaseSessionActLawunit>> makeSecretaryList(int actId, string idList)
        {
            if (string.IsNullOrEmpty(idList))
            {
                return new List<CaseSessionActLawunit>();
            }
            var items = idList.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var result = new List<CaseSessionActLawunit>();
            foreach (var userId in items)
            {
                if (userId == "" || userId == "0" || userId == "-1")
                {
                    continue;
                }
                var newItem = new CaseSessionActLawunit()
                {
                    CaseSessionActId = actId,
                    LawUnitUserId = userId,
                    LawUnitId = await repo.GetPropByIdAsync<ApplicationUser, int>(x => x.Id == userId, x => x.LawUnitId),
                    JudgeRoleId = NomenclatureConstants.JudgeRole.Secretary
                };
                result.Add(newItem);
            }

            return result;
        }

        /// <summary>
        /// Запис на диспозитив
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dispositiv"></param>
        /// <returns></returns>
        public async Task<bool> CaseSessionAct_SaveDispositiv(int id, string dispositiv, string actBlank)
        {
            try
            {
                var saved = await this.ReadByIdAsync<CaseSessionAct>(id);

                saved.Description = dispositiv;
                saved.DateWrt = DateTime.Now;
                saved.UserId = userContext.UserId;
                //Ако акта няма създател и акта все още не е издаден 
                if (!string.IsNullOrWhiteSpace(actBlank) && string.IsNullOrEmpty(saved.ActCreatorUserId) && string.IsNullOrEmpty(saved.RegNumber))
                {
                    saved.ActCreatorUserId = userContext.UserId;
                }
                await CreateHistoryAsync<CaseSessionAct, CaseSessionActH>(saved, "CaseSessionAct_SaveDispositiv");

                await repo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"CaseSessionAct_SaveDispositiv;ID={id}");
                return false;
            }
        }

        public bool CaseSessionAct_SaveMotiveCreator(int id)
        {
            try
            {
                var saved = this.ReadById<CaseSessionAct>(id);

                saved.DateWrt = DateTime.Now;
                saved.UserId = userContext.UserId;
                //Ако акта няма създател и акта все още не е издаден 
                if (string.IsNullOrEmpty(saved.MotiveCreatorUserId))
                {
                    saved.MotiveCreatorUserId = userContext.UserId;
                }
                CreateHistory<CaseSessionAct, CaseSessionActH>(saved, "CaseSessionAct_SaveMotiveCreator");

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"CaseSessionAct_SaveDispositiv;ID={id}");
                return false;
            }
        }

        /// <summary>
        /// Проверка за достъп до акт
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(bool canAccess, string lawunitName)> CheckActAccess(int id, CaseSessionAct model = null)
        {
            var act = model ?? await this.GetReadonlyAsync<CaseSessionAct>(id);

            if (act.CourtId != userContext.CourtId)
            {

                return (false, string.Empty);
            }
            //Ако акта е постановен - има достъп
            if (act.ActDeclaredDate != null)
            {
                return (true, string.Empty);
            }
            return await checkBlankAccess(act, act.ActCreatorUserId);
        }

        public async Task<(bool canAccess, string lawunitName)> CheckActBlankAccess(int actId, NomenclatureConstants.ActAccessMode mode)
        {
            return await CheckActBlankAccess(await this.GetReadonlyAsync<CaseSessionAct>(actId), mode);
        }
        public async Task<(bool canAccess, string lawunitName)> CheckActBlankAccess(CaseSessionAct act, NomenclatureConstants.ActAccessMode mode)
        {
            if (act == null)
            {
                return (false, string.Empty);
            }
            if (act.CourtId != userContext.CourtId)
            {
                return (false, string.Empty);
            }

            var canCorrectAfterDeclare = userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.CaseSessionActCorrection);

            switch (mode)
            {
                case NomenclatureConstants.ActAccessMode.ActBlank:
                    //Бланка на акт се достъпва ако не е обезличен акта и потребителя е изготвящия акта, председателя или СД на делото
                    if (act.ActDeclaredDate != null && !canCorrectAfterDeclare)
                    {
                        return (false, string.Empty);
                    }
                    return await checkBlankAccess(act, act.ActCreatorUserId);
                case NomenclatureConstants.ActAccessMode.ActDefaceBlank:
                    //Обезличаване на акта - постановен акт и потребител с пълен достъп до делото
                    {
                        var caseContext = await GetCurrentContextAsync(SourceTypeSelectVM.Case, act.CaseId, AuditConstants.Operations.Update);
                        return (act.ActDeclaredDate != null && caseContext.CanAccess, string.Empty);
                    }
                case NomenclatureConstants.ActAccessMode.MotiveBlank:
                    if (act.ActMotivesDeclaredDate != null && !canCorrectAfterDeclare)
                    {
                        return (false, string.Empty);
                    }
                    return await checkBlankAccess(act, act.MotiveCreatorUserId);
                case NomenclatureConstants.ActAccessMode.MotiveDefaceBlank:
                    //Обезличаване на мотиви - постановени мотиви и потребител с пълен достъп до делото
                    {
                        var caseContext = await GetCurrentContextAsync(SourceTypeSelectVM.Case, act.CaseId, AuditConstants.Operations.Update);
                        return (act.ActMotivesDeclaredDate != null && caseContext.CanAccess, string.Empty);
                    }
            }

            return (false, string.Empty);

        }



        public async Task<bool> CheckActPrivateFileAccess(int id, CaseSessionAct model = null)
        {
            var act = model ?? await this.ReadByIdAsync<CaseSessionAct>(id);

            if (act.CourtId != userContext.CourtId)
            {
                var caseContext = await GetCurrentContextAsync(SourceTypeSelectVM.Case, act.CaseId, AuditConstants.Operations.View);

                return caseContext.CanAccess;
            }
            //Ако акта е постановен - има достъп
            if (act.ActDeclaredDate != null)
            {
                return true;
            }
            return (await checkBlankAccess(act, act.ActCreatorUserId)).canAccess;
        }


        private async Task<(bool canAccess, string lawunitName)> checkBlankAccess(CaseSessionAct model, string userCreatorId)
        {
            (bool canAccess, string lawunitName) result = (false, string.Empty);
            //Достъп имат изготвилия бланката или всеки от делото, ако все още не е въведен текст
            if (string.IsNullOrEmpty(userCreatorId) || userCreatorId == userContext.UserId)
            {
                result.canAccess = true;
                return result;
            }

            var sessionReporterPredsedatel = await repo.AllReadonly<CaseLawUnit>()
                                               .Where(x => x.CaseId == model.CaseId && x.CaseSessionId == model.CaseSessionId)
                                               .Where(x => (x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                        || (x.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel))
                                               .Select(x => x.LawUnitId)
                                               .ToArrayAsync();

            result.canAccess = sessionReporterPredsedatel.Contains(userContext.LawUnitId);

            if (!result.canAccess)
            {
                result.lawunitName = await repo.AllReadonly<ApplicationUser>()
                                                .Where(x => x.Id == userCreatorId)
                                                .Select(x => (x.LawUnit != null) ? x.LawUnit.FullName : "")
                                                .FirstOrDefaultAsync();
            }
            return result;
        }

        /// <summary>
        /// Регистриране на съдебни актове
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<SaveResultVM> CaseSessionAct_RegisterAct(CaseSessionAct model, string historyType = null)
        {
            if (!string.IsNullOrEmpty(model.RegNumber))
            {
                return new SaveResultVM(true);
            }
            var _caseInfo = await repo.AllReadonly<CaseSession>()
                                .Where(x => x.Id == model.CaseSessionId)
                                .Select(x => new
                                {
                                    x.Case.CaseGroupId,
                                    x.Case.CourtId
                                })
                                .FirstOrDefaultAsync();

            if (counterService.Counter_GetActCounter(model, _caseInfo.CaseGroupId, _caseInfo.CourtId))
            {
                await GenerateActEcliNumber(model);
                model.DateWrt = DateTime.Now;
                model.ActStateId = NomenclatureConstants.SessionActState.Registered;
                await CreateHistoryAsync<CaseSessionAct, CaseSessionActH>(model, historyType ?? "CaseSessionAct_RegisterAct");
                //repo.Update(model);
                await repo.SaveChangesAsync();
                return new SaveResultVM(true, null, NomenclatureConstants.CounterResults.Register);
            }
            return new SaveResultVM(false, "Проблем при регистриране на акт");
        }

        /// <summary>
        /// Изпращане за съгласуване
        /// </summary>
        /// <param name="caseSessionActId"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public async Task<bool> SendForCoordination_Init(int caseSessionActId, long taskId, int coordinationType)
        {
            int coordinateTaskType = WorkTaskConstants.Types.CaseSessionAct_Coordinate;
            if (coordinationType == NomenclatureConstants.CoordinationTypes.Motive)
            {
                coordinateTaskType = WorkTaskConstants.Types.CaseSessionAct_MotiveCoordinate;
            }


            var act = repo.AllReadonly<CaseSessionAct>()
                               .Include(x => x.CaseSession)
                               .ThenInclude(x => x.SessionType)
                               .Where(x => x.Id == caseSessionActId)
                               .FirstOrDefault();

            Expression<Func<CaseLawUnit, bool>> judgeFilter = x => true;
            if (act.ActTypeId == NomenclatureConstants.ActType.Protokol && act.CaseSession.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession)
            {
                //Когато акта е от вид протокол в ОСЗ се подписва само от председателя на състава
                judgeFilter = x => x.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel;
            }

            var model = (await GetCaseLawUnitsByAct(caseSessionActId)).AsQueryable()
                            .Where(judgeFilter)
                            .OrderByDescending(x => x.JudgeRoleId)
                            .ToList();

            bool hasCoordinationForAct = await repo.AllReadonly<CaseSessionActCoordination>()
                                                    .AnyAsync(x => x.CaseSessionActId == caseSessionActId && x.CoordinationType == coordinationType);
            if (hasCoordinationForAct)
            {
                var prevCoordicationTasks = await repo.All<WorkTask>()
                                                    .Where(x => x.SourceType == SourceTypeSelectVM.CaseSessionAct && x.SourceId == caseSessionActId)
                                                    .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))

                                                    .ToListAsync();
                foreach (var item in prevCoordicationTasks)
                {
                    item.TaskStateId = WorkTaskConstants.States.Deleted;
                    await repo.SaveChangesAsync();
                }
            }
            foreach (var caseLawUnit in model)
            {
                //Ако вече има създадени записи за особени мнения към акта - не се създават нови
                if (!hasCoordinationForAct)
                {
                    var coordination = new CaseSessionActCoordination()
                    {
                        CaseId = act.CaseId,
                        CourtId = act.CourtId,
                        CaseSessionActId = act.Id,
                        CaseLawUnitId = caseLawUnit.Id,
                        ActCoordinationTypeId = NomenclatureConstants.ActCoordinationTypes.New,
                        CoordinationType = coordinationType,
                        UserId = userContext.UserId,
                        DateWrt = DateTime.Now

                    };
                    repo.Add(coordination);
                    await repo.SaveChangesAsync();
                }

                var userId = GetUserIdByLawUnitId(caseLawUnit.LawUnitId);
                if (!string.IsNullOrEmpty(userId))
                {
                    var newTask = new WorkTaskEditVM()
                    {
                        ParentTaskId = taskId,
                        SourceType = SourceTypeSelectVM.CaseSessionAct,
                        SourceId = caseSessionActId,
                        TaskTypeId = coordinateTaskType,
                        TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser,
                        UserId = userId,
                    };
                    await taskService.CreateTask(newTask);
                }
            }

            return model.Count > 0;
        }

        /// <summary>
        /// Извличане на състав от акт
        /// </summary>
        /// <param name="caseSessionActId"></param>
        /// <param name="caseSessionId"></param>
        /// <param name="forMotives"></param>
        /// <returns></returns>
        public async Task<ICollection<CaseLawUnit>> GetCaseLawUnitsByAct(int caseSessionActId, int caseSessionId = 0, bool forMotives = false, bool forActPrint = false)
        {
            int[] judgeRoles = NomenclatureConstants.JudgeRole.JudgeAndJuryRolesListMain;


            if (forMotives == true)
            {
                //Когато няма особено мнение - всички приемат акта, мотивите не се подписват и от заседателите
                bool hasCoordinationDisagree = await repo.AllReadonly<CaseSessionActCoordination>()
                                                        .Where(x => x.CaseSessionActId == caseSessionActId)
                                                        .Where(x => x.ActCoordinationTypeId != NomenclatureConstants.ActCoordinationTypes.Accept)
                                                        .AnyAsync();

                if (!hasCoordinationDisagree)
                {
                    judgeRoles = NomenclatureConstants.JudgeRole.JudgeRolesListMain;
                }
            }
            if (forActPrint)
            {
                judgeRoles = NomenclatureConstants.JudgeRole.JudgeAndJuryRolesListMainForActPrint;
            }


            var result = new List<CaseLawUnit>();
            int courtId = 0;
            if (caseSessionActId > 0)
            {
                var actInfo = await repo.AllReadonly<CaseSessionAct>()
                                    .Where(x => x.Id == caseSessionActId)
                                    //Състава и задачите за акта се вземат към началото на заседанието CaseSession.DateFrom, 05.10.2021
                                    //.Select(x => new { RegDate = (x.RegDate ?? DateTime.Now), x.CaseSessionId, x.CaseId }).FirstOrDefault();
                                    .Select(x => new { SessionDate = x.CaseSession.DateFrom, x.CaseSessionId, x.CaseId, x.CourtId }).FirstOrDefaultAsync();

                courtId = actInfo.CourtId ?? 0;
                result.AddRange(await repo.AllReadonly<CaseLawUnit>()
                                .Include(x => x.LawUnit)
                                .Where(x => x.CaseId == actInfo.CaseId && x.CaseSessionId == actInfo.CaseSessionId)
                                .Where(x => judgeRoles.Contains(x.JudgeRoleId))
                                .Where(x => x.DateFrom <= actInfo.SessionDate && (x.DateTo ?? DateTime.MaxValue) > actInfo.SessionDate)
                                .OrderByDescending(x => x.JudgeRoleId)
                                .ToListAsync());
            }
            else
            {
                var sessionInfo = await repo.AllReadonly<CaseSession>().Where(x => x.Id == caseSessionId)
                                                    .Select(x => new { RegDate = x.DateFrom, CaseSessionId = x.Id, x.CaseId, x.CourtId }).FirstOrDefaultAsync();

                courtId = sessionInfo.CourtId ?? 0;
                result.AddRange(await repo.AllReadonly<CaseLawUnit>()
                                .Include(x => x.LawUnit)
                                .Where(x => x.CaseId == sessionInfo.CaseId && x.CaseSessionId == sessionInfo.CaseSessionId)
                                .Where(x => judgeRoles.Contains(x.JudgeRoleId))
                                .Where(x => x.DateFrom.Date <= sessionInfo.RegDate.Date && (x.DateTo ?? DateTime.MaxValue) > sessionInfo.RegDate)
                                .OrderByDescending(x => x.JudgeRoleId)
                                .ToListAsync());
            }

            int[] lawUnitsIds = result.Select(c => c.LawUnitId).ToArray();
            var lawUnitsOrder = await repo.AllReadonly<CourtLawUnitOrder>()
                                        .Where(x => x.CourtId == courtId)
                                        .Where(x => lawUnitsIds.Contains(x.LawUnitId))
                                        .ToListAsync();

            foreach (var caseLaw in result)
            {
                if (caseLaw.SavedOrderBy != 0)
                {
                    continue;
                }

                if (caseLaw.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel)
                {
                    //Председателите излизат първи
                    caseLaw.OrderBy = -1;
                    continue;
                }

                //Останалите лица в състава се редят по реда на старшинство
                var _order = lawUnitsOrder.FirstOrDefault(x => x.LawUnitId == caseLaw.LawUnitId);
                if (_order != null)
                {
                    caseLaw.OrderBy = _order.OrderNumber;
                }
                else
                {
                    //Съдиите, които не фигурират в списъка на старшинството се редят по реда им на добавяне в делото
                    caseLaw.OrderBy = 10000000 + caseLaw.Id;
                }
            }

            return result.OrderBy(x => x.SavedOrderBy).ThenBy(x => x.OrderBy).ToList();

        }

        public async Task<SaveResultVM> LawUnit_SaveOrderBy(int caseSessionActId, int caseSessionId = 0)
        {
            var lawunits = await GetCaseLawUnitsByAct(caseSessionActId, caseSessionId);
            if (lawunits.Any(l => l.SavedOrderBy < -1))
            {
                return new SaveResultVM(true);
            }
            //Записания ред на старшинство е с отрицателен знак за да не се бърка с текущ прочетения от CourtLawunitOrder
            int judgeNumber = -100;
            foreach (var lawunit in lawunits)
            {
                repo.Attach(lawunit);
                lawunit.SavedOrderBy = judgeNumber;
                await repo.SaveChangesAsync();
                judgeNumber++;
            }

            return new SaveResultVM(true);
        }

        /// <summary>
        /// Пращане за подписване
        /// </summary>
        /// <param name="caseSessionActId"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public async Task<SaveResultVM> SendForSign_Init(int caseSessionActId, long taskId)
        {
            SaveResultVM result = new SaveResultVM();
            var act = await repo.AllReadonly<CaseSessionAct>()
                                .Include(x => x.CaseSession)
                                .ThenInclude(x => x.SessionType)
                                .Where(x => x.Id == caseSessionActId)
                                .FirstOrDefaultAsync();

            Expression<Func<CaseLawUnit, bool>> judgeFilter = x => true;
            if (act.ActTypeId == NomenclatureConstants.ActType.Protokol && act.CaseSession.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession)
            {
                //Когато акта е от вид протокол в ОСЗ се подписва само от председателя на състава
                judgeFilter = x => x.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel;
            }
            var model = (await GetCaseLawUnitsByAct(caseSessionActId, 0, false, true)).AsQueryable()
                            .Where(judgeFilter)
                            .OrderByDescending(x => x.JudgeRoleId)
                            .ToList();

            if (act.SignJudgeLawUnitId > 0 && NomenclatureConstants.ActType.HasSignJudge.Contains(act.ActTypeId))
            {
                model = new List<CaseLawUnit>()
                {
                    new CaseLawUnit()
                    {
                        LawUnitId = act.SignJudgeLawUnitId.Value
                    }
                };
            }

            foreach (var caseLawUnit in model)
            {
                caseLawUnit.LawUnitUserId = GetUserIdByLawUnitId(caseLawUnit.LawUnitId);
            }

            if (model.Any(x => string.IsNullOrEmpty(x.LawUnitUserId)))
            {
                result.Result = false;
                result.ErrorMessage = "Съществуват лица с неактивен/липсващ потребител!";
                return result;
            }

            foreach (var caseLawUnit in model)
            {
                var newTask = new WorkTaskEditVM()
                {
                    ParentTaskId = taskId,
                    SourceType = SourceTypeSelectVM.CaseSessionAct,
                    SourceId = caseSessionActId,
                    TaskTypeId = WorkTaskConstants.Types.CaseSessionAct_Sign,
                    TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser,
                    UserId = caseLawUnit.LawUnitUserId,
                };
                await taskService.CreateTask(newTask);
            }
            if (model.Any())
            {
                //Протоколи и протоколни определения се подписват и от секретаря по акта, ако има такъв
                if (NomenclatureConstants.ActType.SecretarySign.Contains(act.ActTypeId))
                {
                    List<string> secretaryUsersIds = new List<string>();
                    if (!string.IsNullOrEmpty(act.SecretaryUserId))
                    {
                        secretaryUsersIds.Add(act.SecretaryUserId);
                    }
                    else
                    {
                        secretaryUsersIds = await repo.AllReadonly<CaseSessionActLawunit>()
                                                    .Where(x => x.CaseSessionActId == act.Id)
                                                    .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.Secretary)
                                                    .Select(x => x.LawUnitUserId)
                                                    .ToListAsync();
                    }
                    foreach (var secretaryUserId in secretaryUsersIds)
                    {
                        {
                            var newTask = new WorkTaskEditVM()
                            {
                                ParentTaskId = taskId,
                                SourceType = SourceTypeSelectVM.CaseSessionAct,
                                SourceId = caseSessionActId,
                                TaskTypeId = WorkTaskConstants.Types.CaseSessionAct_Sign,
                                TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser,
                                UserId = secretaryUserId,
                            };
                            await taskService.CreateTask(newTask);
                        }
                    }
                }
            }

            if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.Request1_2024))
            {
                var coordinations = await coordinationService.CaseSessionActCoordination_Select(caseSessionActId)
                                            .Where(x => (x.ActCoordinationTypeId == NomenclatureConstants.ActCoordinationTypes.AcceptWithOpinion) || (x.ActCoordinationTypeId == NomenclatureConstants.ActCoordinationTypes.DontAccept))
                                            .ToListAsync();
                foreach (var coordination in coordinations)
                {

                    var userId = GetUserIdByLawUnitId(coordination.LawUnitId);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var newTask = new WorkTaskEditVM()
                        {
                            ParentTaskId = taskId,
                            SourceType = SourceTypeSelectVM.CaseSessionAct,
                            SourceId = caseSessionActId,
                            SubSourceId = coordination.Id,
                            TaskTypeId = WorkTaskConstants.Types.CaseSessionActCoordination_Sign,
                            TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser,
                            UserId = userId,
                        };
                        await taskService.CreateTask(newTask);
                    }
                }
            }


            result.Result = model.Count > 0;
            if (!result.Result)
            {
                result.ErrorMessage = "Няма съдебен състав по заседанието";
            }

            return result;
        }



        /// <summary>
        /// Изпращане за подписване на мотив
        /// </summary>
        /// <param name="caseSessionActId"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public async Task<bool> SendForSignMotives_Init(int caseSessionActId, long taskId)
        {
            var model = await GetCaseLawUnitsByAct(caseSessionActId, 0, true);

            foreach (var caseLawUnit in model)
            {

                var userId = GetUserIdByLawUnitId(caseLawUnit.LawUnitId);
                if (!string.IsNullOrEmpty(userId))
                {
                    var newTask = new WorkTaskEditVM()
                    {
                        ParentTaskId = taskId,
                        SourceType = SourceTypeSelectVM.CaseSessionAct,
                        SourceId = caseSessionActId,
                        TaskTypeId = WorkTaskConstants.Types.CaseSessionActMotives_Sign,
                        TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser,
                        UserId = userId,
                    };
                    await taskService.CreateTask(newTask);
                }
            }


            return model.Count > 0;
        }

        /// <summary>
        /// Зареждане в комбо на актовете от заседания по ID на дело
        /// </summary>
        /// <param name="caseId">ID на акт</param>
        /// <param name="IsFinal">Финален документ. Ако е null не се взема предвид</param>
        /// <param name="IsDecreed">Постановен. Ако е null не се взема предвид</param>
        /// <param name="IsReadyForPublish">Готов за публикуване. Ако е null не се взема предвид</param>
        /// <param name="IsActInforced">Влязъл в сила. Ако е null не се взема предвид</param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownList(int caseId, bool? IsFinal = null, bool? IsDecreed = null, bool? IsReadyForPublish = null, bool? IsActInforced = null, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = repo.AllReadonly<CaseSessionAct>()
                             .Where(x => (x.CaseSession.CaseId == caseId) &&
                                         (IsFinal != null ? x.IsFinalDoc : true) &&
                                         (IsDecreed != null ? x.ActDate != null : true) &&
                                         (IsReadyForPublish != null ? x.IsReadyForPublish : true) &&
                                         (IsActInforced != null ? x.ActInforcedDate != null : true))
                             .Select(x => new SelectListItem()
                             {
                                 Text = x.ActType.Label + " " + x.ActState.Label + " " + (x.RegNumber ?? string.Empty) + ((x.RegDate != null) ? "/" + (x.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty),
                                 Value = x.Id.ToString()
                             })
                             .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        /// <summary>
        /// Зареждане в комбо на актовете от заседания по ID на дело
        /// </summary>
        /// <param name="caseId">ID на акт</param>
        /// <param name="IsFinal">Финален документ. Ако е null не се взема предвид</param>
        /// <param name="IsDecreed">Постановен. Ако е null не се взема предвид</param>
        /// <param name="IsReadyForPublish">Готов за публикуване. Ако е null не се взема предвид</param>
        /// <param name="IsActInforced">Влязъл в сила. Ако е null не се взема предвид</param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDropDownListAsync(int caseId, bool? IsFinal = null, bool? IsDecreed = null, bool? IsReadyForPublish = null, bool? IsActInforced = null, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = await repo.AllReadonly<CaseSessionAct>()
                                   .Where(x => (x.CaseSession.CaseId == caseId) &&
                                               (IsFinal != null ? x.IsFinalDoc : true) &&
                                               (IsDecreed != null ? x.ActDate != null : true) &&
                                               (IsReadyForPublish != null ? x.IsReadyForPublish : true) &&
                                               (IsActInforced != null ? x.ActInforcedDate != null : true))
                                   .Select(x => new SelectListItem()
                                   {
                                       Text = x.ActType.Label + " " + x.ActState.Label + " " + (x.RegNumber ?? string.Empty) + ((x.RegDate != null) ? "/" + (x.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty),
                                       Value = x.Id.ToString()
                                   })
                                   .ToListAsync() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        /// Зареждане в комбо на актовете от заседания по постановени с 
        /// С определение за отвод/С разпореждане за отвод
        public List<SelectListItem> GetDropDownListForDismisal(int caseId)
        {

            var session_result = repo.AllReadonly<CaseSessionResult>();
            var result = repo.AllReadonly<CaseSessionAct>()
                             .Where(x => (x.CaseSession.CaseId == caseId) &&
                                         ((x.ActStateId == NomenclatureConstants.SessionActState.Enforced) || (x.ActStateId == NomenclatureConstants.SessionActState.ComingIntoForce)) &&
                                         (!string.IsNullOrEmpty(x.RegNumber)) &&
                                         session_result.Where(g => g.CaseSessionId == x.CaseSessionId).Any(g => NomenclatureConstants.CaseSessionResult.ActZaOtvod.Contains(g.SessionResultId))

                                         //(IsActInforced != null ? x.ActInforcedDate != null : true)
                                         )
                .Select(x => new SelectListItem()
                {
                    Text = x.ActType.Label + " " + x.ActState.Label + " " + (x.RegNumber ?? string.Empty) + ((x.RegDate != null) ? "/" + (x.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty),
                    Value = x.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();



            return result;
        }

        /// <summary>
        /// Зареждане в комбо на актовете от заседания по постановени с определение за отвод/С разпореждане за отвод
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDropDownListForDismisalAsync(int caseId)
        {
            var session_result = repo.AllReadonly<CaseSessionResult>();

            var result = await repo.AllReadonly<CaseSessionAct>()
                                   .Where(x => x.CaseSession.CaseId == caseId)
                                   .Where(x => x.ActStateId == NomenclatureConstants.SessionActState.Enforced || x.ActStateId == NomenclatureConstants.SessionActState.ComingIntoForce)
                                   .Where(x => !string.IsNullOrEmpty(x.RegNumber))
                                   .Where(x => session_result.Where(g => g.CaseSessionId == x.CaseSessionId)
                                                             .Any(g => NomenclatureConstants.CaseSessionResult
                                                                                            .ActZaOtvod
                                                                                            .Contains(g.SessionResultId)))
                                   .OrderByDescending(x => x.RegDate)
                                   .Select(x => new SelectListItem()
                                   {
                                       Text = x.ActType.Label + " " + x.ActState.Label + " " + (x.RegNumber ?? string.Empty) + ((x.RegDate != null) ? "/" + (x.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty),
                                       Value = x.Id.ToString()
                                   })
                                   .ToListAsync() ?? new List<SelectListItem>();

            return result;
        }

        public List<SelectListItem> GetDropDownListForDismisalRequest(int caseId)
        {
            var result = repo.AllReadonly<CaseSessionAct>()
                             .Where(x => (x.CaseId == caseId) &&
                                         ((x.ActStateId == NomenclatureConstants.SessionActState.Enforced) || (x.ActStateId == NomenclatureConstants.SessionActState.ComingIntoForce)) &&
                                         (x.ActDeclaredDate != null) &&
                                         x.ActType.ActFormatType == NomenclatureConstants.ActFormatType.Protokol
                                         )
                .Select(x => new SelectListItem()
                {
                    Text = x.ActType.Label + " " + x.ActState.Label + " " + (x.RegNumber ?? string.Empty) + ((x.RegDate != null) ? "/" + (x.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty),
                    Value = x.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();



            return result;
        }

        public async Task<List<SelectListItem>> GetDropDownListForDismisalRequestAsync(int caseId)
        {
            var result = await repo.AllReadonly<CaseSessionAct>()
                                   .Where(x => x.CaseId == caseId)
                                   .Where(x => x.ActStateId == NomenclatureConstants.SessionActState.Enforced || x.ActStateId == NomenclatureConstants.SessionActState.ComingIntoForce)
                                   .Where(x => x.ActDeclaredDate != null)
                                   .Where(x => x.ActType.ActFormatType == NomenclatureConstants.ActFormatType.Protokol)
                                   .OrderByDescending(x => x.RegDate)
                                   .Select(x => new SelectListItem()
                                   {
                                       Text = x.ActType.Label + " " + x.ActState.Label + " " + (x.RegNumber ?? string.Empty) + ((x.RegDate != null) ? "/" + (x.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty),
                                       Value = x.Id.ToString()
                                   })
                                   .ToListAsync() ?? new List<SelectListItem>();

            return result;
        }

        /// <summary>
        /// Извличане на съдебни актове по сесия за комбобокс
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownListBySessionId(int caseSessionId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = repo.AllReadonly<CaseSessionAct>()
                             .Where(x => x.CaseSessionId == caseSessionId)
                .Select(x => new SelectListItem()
                {
                    Text = x.ActType.Label + " " + x.ActState.Label + " " + (x.RegNumber ?? string.Empty) + ((x.RegDate != null) ? "/" + (x.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty),
                    Value = x.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        /// <summary>
        /// Извличане на данни за съдебни актове за страните (лява/дясна)
        /// </summary>
        /// <param name="models"></param>
        /// <param name="caseSessionActPrint"></param>
        private void FillLeftRightSide_CasePersons(List<CasePersonListVM> models, CaseSessionActPrintVM caseSessionActPrint)
        {
            string result = string.Empty;

            caseSessionActPrint.LeftSide = new List<string>();
            var LeftSideName = new List<string>();
            var LeftSideOnlyName = new List<string>();
            var LeftSideWithAddress = new List<string>();
            foreach (var casePerson in models.Where(x => x.RoleKindId == NomenclatureConstants.PersonKinds.LeftSide && (x.DateTo ?? DateTime.MaxValue) > DateTime.Now))
            {
                var person = casePerson.PersonRoleShortForumLabel.ToLower() + " " + casePerson.FullName + (!string.IsNullOrEmpty(casePerson.Uic) ? " с " + casePerson.UicTypeLabel + ": " + casePerson.Uic + " " : " ") + (!string.IsNullOrEmpty(casePerson.AllAddressString) ? " с адрес: " + casePerson.AllAddressString : string.Empty);
                var linkListVM = casePersonLinkService.GetLinkForPerson(casePerson.Id, false, 0, null);
                if (linkListVM != null)
                {
                    person = person + " " + string.Join(", ", linkListVM.Select(x => x.LabelWithoutFirstPerson));
                }
                caseSessionActPrint.LeftSide.Add(person);
                var personName = casePerson.FullName + (!string.IsNullOrEmpty(casePerson.Uic) ? " " + casePerson.UicTypeLabel + ": " + casePerson.Uic : string.Empty) + (string.IsNullOrEmpty(casePerson.AllAddressString) ? string.Empty : ", " + casePerson.AllAddressString);
                LeftSideName.Add(personName);
                LeftSideOnlyName.Add(casePerson.FullName);
                LeftSideWithAddress.Add($"{personName}");
            }

            caseSessionActPrint.LeftSideWithOutRole_410_417 = string.Join(" и ", LeftSideName.Select(x => "кредитора " + x));
            caseSessionActPrint.LeftSideName = string.Join(" и ", LeftSideName.Select(x => x));
            caseSessionActPrint.LeftSideOnlyName = LeftSideOnlyName.ToArray();
            caseSessionActPrint.LeftSidesWithAddress = string.Join(" и ", LeftSideWithAddress.Select(x => x));
            caseSessionActPrint.LeftSideCurrentAddress = string.Join(", ", models.Where(x => x.RoleKindId == NomenclatureConstants.PersonKinds.LeftSide && !string.IsNullOrEmpty(x.AllAddressString)).Select(x => x.AllAddressString));
            caseSessionActPrint.LeftSideWorkAddress = string.Join(", ", models.Where(x => x.RoleKindId == NomenclatureConstants.PersonKinds.LeftSide && !string.IsNullOrEmpty(x.AllAddressString)).Select(x => x.AllAddressString));

            caseSessionActPrint.RightSide = new List<string>();
            var RightSideName = new List<string>();
            var RightSideWithAddress = new List<string>();
            var RightSideOnlyName = new List<string>();
            foreach (var casePerson in models.Where(x => x.RoleKindId == NomenclatureConstants.PersonKinds.RightSide))
            {
                var person = ((RightSideName.Count >= 1) ? casePerson.PersonRoleBigForumLabel.ToLower() : casePerson.PersonRoleBigForumLabel) + " " + casePerson.FullName + " с " + casePerson.UicTypeLabel + ": " + casePerson.Uic + " " + (!string.IsNullOrEmpty(casePerson.AllAddressString) ? " с адрес: " + casePerson.AllAddressString : string.Empty);
                var linkListVM = casePersonLinkService.GetLinkForPerson(casePerson.Id, false, 0, null);
                if (linkListVM != null)
                {
                    person = person + " " + string.Join(", ", linkListVM.Select(x => x.LabelWithoutFirstPerson));
                }
                caseSessionActPrint.RightSide.Add(person);
                var personName = casePerson.FullName + (!string.IsNullOrEmpty(casePerson.Uic) ? " " + casePerson.UicTypeLabel + ": " + casePerson.Uic : string.Empty);
                RightSideName.Add(personName);
                if (!string.IsNullOrEmpty(casePerson.AllAddressString))
                {
                    RightSideWithAddress.Add($"{personName}, {casePerson.AllAddressString}");
                }
                else
                {
                    RightSideWithAddress.Add($"{personName}");
                }
                RightSideOnlyName.Add(casePerson.FullName);
            }
            caseSessionActPrint.RightSideName = string.Join(" и ", RightSideName.Select(x => x));
            caseSessionActPrint.RightSidesWithAddress = string.Join(" и ", RightSideWithAddress.Select(x => x));
            caseSessionActPrint.RightSideCurrentAddress = string.Join(", ", models.Where(x => x.RoleKindId == NomenclatureConstants.PersonKinds.RightSide && !string.IsNullOrEmpty(x.AllAddressString)).Select(x => x.AllAddressString));
            caseSessionActPrint.RightSideWorkAddress = string.Join(", ", models.Where(x => x.RoleKindId == NomenclatureConstants.PersonKinds.RightSide && !string.IsNullOrEmpty(x.AllAddressString)).Select(x => x.AllAddressString));
            caseSessionActPrint.RightSidesOnlyName = RightSideOnlyName.ToArray();

            caseSessionActPrint.LeftSide_410_417 = string.Join(" и ", caseSessionActPrint.LeftSide);
            caseSessionActPrint.RightSide_410_417 = string.Join(" и ", caseSessionActPrint.RightSide);
            caseSessionActPrint.LeftSide_410_417_Count = caseSessionActPrint.LeftSide.Count;
            caseSessionActPrint.RightSide_410_417_Count = caseSessionActPrint.RightSide.Count;


            //Смяна на страните при обърнати бланки
            if (!NomenclatureConstants.ActBlankNames.ActDirectionAlter.Contains(caseSessionActPrint.ActKindBlankName))
            {
                return;
            }

            if (caseSessionActPrint.ActDirection == NomenclatureConstants.ActBlankDirection.RightToLeft)
            {
                var tmp = string.Join(',', caseSessionActPrint.LeftSide);
                caseSessionActPrint.LeftSide = caseSessionActPrint.RightSide;
                caseSessionActPrint.RightSide = tmp.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

                tmp = caseSessionActPrint.LeftSideName;
                caseSessionActPrint.LeftSideName = caseSessionActPrint.RightSideName;
                caseSessionActPrint.RightSideName = tmp;

                tmp = caseSessionActPrint.LeftSidesWithAddress;
                caseSessionActPrint.LeftSidesWithAddress = caseSessionActPrint.RightSidesWithAddress;
                caseSessionActPrint.RightSidesWithAddress = tmp;

                tmp = caseSessionActPrint.LeftSideCurrentAddress;
                caseSessionActPrint.LeftSideCurrentAddress = caseSessionActPrint.RightSideCurrentAddress;
                caseSessionActPrint.RightSideCurrentAddress = tmp;

                tmp = caseSessionActPrint.LeftSideWorkAddress;
                caseSessionActPrint.LeftSideWorkAddress = caseSessionActPrint.RightSideWorkAddress;
                caseSessionActPrint.RightSideWorkAddress = tmp;

                tmp = caseSessionActPrint.LeftSide_410_417;
                caseSessionActPrint.LeftSide_410_417 = caseSessionActPrint.RightSide_410_417;
                caseSessionActPrint.RightSide_410_417 = tmp;

                int tmpI = caseSessionActPrint.LeftSide_410_417_Count;
                caseSessionActPrint.LeftSide_410_417_Count = caseSessionActPrint.RightSide_410_417_Count;
                caseSessionActPrint.RightSide_410_417_Count = tmpI;
            }
        }

        /// <summary>
        /// Принтиране на съдебни актове
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CaseSessionActPrintVM> CaseSessionAct_GetForPrint(int id)
        {
            CaseSessionActPrintVM result = new CaseSessionActPrintVM();

            DateTime dateNow = DateTime.Now;

            var act = await repo.AllReadonly<CaseSessionAct>()
                                .Where(x => x.Id == id)
                                .Select(x => new
                                {
                                    x.Id,
                                    x.CaseSessionId,
                                    CaseSessionSessionTypeGroup = x.CaseSession.SessionType.SessionTypeGroup,
                                    x.RegDate,
                                    x.RegNumber,
                                    x.ActTypeId,
                                    x.CaseId,
                                    ActTypeActFormatType = x.ActType.ActFormatType,
                                    ActTypeCode = x.ActType.Code,
                                    ActTypeLabel = x.ActType.Label,
                                    x.ActDirectionId,
                                    ActTypeBlankHeaderText = x.ActType.BlankHeaderText,
                                    ActTypeBlankLabel = x.ActType.BlankLabel,
                                    ActTypeBlankDecisionText = x.ActType.BlankDecisionText,
                                    ActKindBlankName = (x.ActKind != null) ? x.ActKind.BlankName : "",
                                    ActKindDescription = (x.ActKind != null) ? x.ActKind.Description : "",
                                    x.Description,
                                    x.ActDeclaredDate,
                                    x.RelatedActId,
                                    x.SignJudgeLawUnitId,
                                    x.ActDate,
                                    x.SecretaryUserId,
                                    x.ActTerm,
                                    x.ActKindId,
                                    x.GenerateExecProcess,
                                    RelatedActText = x.RelatedActId != null ? x.RelatedAct.ActType.Label.ToLower() + " №" + x.RelatedAct.RegNumber + "/" + (x.RelatedAct.RegDate ?? dateNow).ToString("dd.MM.yyyy") + "г." : string.Empty
                                })
                                .FirstOrDefaultAsync();

            var caseSession = await repo.AllReadonly<CaseSession>()
                                        .Where(x => x.Id == act.CaseSessionId)
                                        .Select(x => new
                                        {
                                            x.CaseId,
                                            CaseTypeLabel = x.Case.CaseType.Label,
                                            x.Case.ShortNumber,
                                            x.Case.RegNumber,
                                            x.Case.RegDate,
                                            x.Case.CourtId,
                                            CourtLabel = x.Case.Court.Label,
                                            CityName = x.Case.Court.CityName,
                                            CourtRegionParentId = x.Case.Court.CourtRegion.ParentId,
                                            x.Case.Court.CourtLogo,
                                            SessionTypeLabel = x.SessionType.Label,
                                            SessionTypeSessionTypeGroup = x.SessionType.SessionTypeGroup,
                                            SessionTypeSessionActLabel = x.SessionType.SessionActLabel,
                                            x.SessionStateId,
                                            x.DateFrom,
                                            CaseByDocumentRequest = x.Case.Document.DocumentRequestTypeId != null
                                        })
                                        .FirstOrDefaultAsync();

            //var act = repo.AllReadonly<CaseSessionAct>()
            //                         .Include(x => x.CaseSession)
            //                         .ThenInclude(x => x.SessionType)
            //                         .Include(x => x.CaseSession)
            //                         .ThenInclude(x => x.Case)
            //                         .ThenInclude(x => x.Court)
            //                         .ThenInclude(x => x.CourtRegion)
            //                         .Include(x => x.CaseSession)
            //                         .ThenInclude(x => x.Case)
            //                         .ThenInclude(x => x.CaseType)
            //                         .Include(x => x.ActType)
            //                         .Include(x => x.ActKind)
            //                         .Include(x => x.RelatedAct)
            //                         .ThenInclude(x => x.ActType)
            //                         .FirstOrDefault(x => x.Id == id);

            if (act.ActTypeId == NomenclatureConstants.ActType.Protokol && act.CaseSessionSessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession)
            {
                result.ChairmanSignOnly = true;
            }

            DateTime actDate = act.RegDate ?? DateTime.Now;

            result.Id = id;
            result.CaseId = act.CaseId ?? 0;
            result.ActFormatType = act.ActTypeActFormatType;
            result.ActTypeId = act.ActTypeId;
            result.ActTypeCode = act.ActTypeCode;
            result.ActTypeName = act.ActTypeLabel;
            result.ActDirection = act.ActDirectionId ?? NomenclatureConstants.ActBlankDirection.LeftToRight;
            result.BlankHeaderText = act.ActTypeBlankHeaderText;
            result.BlankActTypeName = act.ActTypeBlankLabel;
            result.ActKindBlankName = act.ActKindBlankName;
            result.ActKindDescription = act.ActKindDescription;
            result.ActRegNumber = act.RegNumber;
            result.Dispositiv = act.Description;
            result.ActDeclaredDate = act.ActDeclaredDate;
            result.ActRegDate = (act.RegDate != null) ? act.RegDate.Value.ToString("dd.MM.yyyy") : "";
            result.ActRegYear = (act.RegDate != null) ? act.RegDate.Value.Year.ToString() : "";
            result.BlankDecisionText = act.ActTypeBlankDecisionText;
            result.CourtId = caseSession.CourtId;
            result.CourtCity = caseSession.CityName;
            result.CourtName = caseSession.CourtLabel;
            result.CourtLogo = caseSession.CourtLogo;
            result.CaseByDocumentRequest = caseSession.CaseByDocumentRequest;
            result.RelatedActId = act.RelatedActId;
            result.RelatedActText = act.RelatedActText;
            result.GenerateExecProcess = act.GenerateExecProcess ?? false;
            if (act.RelatedActId > 0)
            {
                var relatedAct = await repo.AllReadonly<CaseSessionAct>()
                                                .Where(x => x.Id == act.RelatedActId.Value)
                                                .Select(x => new
                                                {
                                                    ActTypeLabel = x.ActType.Label,
                                                    x.RegNumber,
                                                    RegDate = (x.RegDate ?? DateTime.Now),
                                                    x.Description
                                                }).FirstOrDefaultAsync();
                result.RelatedActTypeName = relatedAct.ActTypeLabel;
                result.RelatedActNumber = relatedAct.RegNumber;
                result.RelatedActDate = relatedAct.RegDate.ToString("dd.MM.yyyy");
                result.RelatedActYear = relatedAct.RegDate.Year.ToString();
                result.RelatedActDispositive = relatedAct.Description;
            }

            result.CourtParent = string.Empty;

            if (caseSession.CourtRegionParentId > 0)
            {
                result.CourtParent = await repo.GetPropByIdAsync<CourtRegion, string>(x => x.Id == caseSession.CourtRegionParentId, x => x.Label);
            }

            if (result.CourtId == NomenclatureConstants.VKScourtId)
            {
                result.IsMixedJuryVKS_VAS = await repo.AllReadonly<CaseLawUnitCount>()
                                                    .Where(x => x.CaseId == result.CaseId)
                                                    .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeVAS)
                                                    .AnyAsync();
            }

            //result.CourtParent = (act.CaseSession.Case.Court.CourtRegion != null) ? repo.GetById<CourtRegion>(act.CaseSession.Case.Court.CourtRegion.ParentId).Label : string.Empty;
            result.CaseSessionId = act.CaseSessionId;
            result.SessionTypeName = caseSession.SessionTypeLabel;
            result.SessionActLabel = caseSession.SessionTypeSessionActLabel;
            result.SessionStateId = caseSession.SessionStateId;
            result.SessionDate = caseSession.DateFrom;
            result.SessionIdOpen = caseSession.SessionTypeSessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession;

            var firstSessionMeeting = await repo.AllReadonly<CaseSessionMeeting>()
                                        .Where(x => x.CaseSessionId == act.CaseSessionId)
                                        .Where(FilterExpireInfo<CaseSessionMeeting>(false))
                                        .OrderBy(x => x.DateFrom)
                                        .FirstOrDefaultAsync();

            //Началния час на заседанието се взема от първата сесия на заседанието, 15.07.2020 КБорисов
            if (firstSessionMeeting != null)
            {
                result.SessionDate = firstSessionMeeting.DateFrom;
            }

            result.CaseId = caseSession.CaseId;
            result.CaseTypeName = caseSession.CaseTypeLabel;
            result.CaseRegShortNumber = caseSession.ShortNumber;
            result.CaseRegNumber = caseSession.RegNumber;
            result.CaseRegYear = caseSession.RegDate.Year;
            result.ActTerm = act.ActTerm;
            result.AnswerActRegNumber = string.Empty;
            if (!string.IsNullOrEmpty(act.SecretaryUserId))
            {
                var secretaryName = (await repo.AllReadonly<ApplicationUser>()
                                        .Where(x => x.Id == act.SecretaryUserId)
                                        .Select(x => x.LawUnit)
                                        .FirstOrDefaultAsync())?.FullName_MiddleNameInitials;
                result.SecretaryName = secretaryName;
            }
            if (string.IsNullOrEmpty(result.SecretaryName))
            {
                //Ако има избран секретар в акта, няма да има записани данни в CaseSessionActLawunit
                result.SecretaryList = await repo.AllReadonly<CaseSessionActLawunit>()
                                                 .Where(x => x.CaseSessionActId == act.Id && x.JudgeRoleId == NomenclatureConstants.JudgeRole.Secretary)
                                                 .Select(x => x.LawUnit.FullName)
                                                 .ToListAsync();
            }

            if (act.ActTypeId == NomenclatureConstants.ActType.CommandmentProtection || act.ActTypeId == NomenclatureConstants.ActType.CommandmentimmediatelyProtection)
            {
                var actOther = repo.AllReadonly<CaseSessionAct>()
                                   .Where(x => x.CaseSessionId == act.CaseSessionId &&
                                               (x.ActTypeId == NomenclatureConstants.ActType.Answer || x.ActTypeId == NomenclatureConstants.ActType.Definition))
                                   .FirstOrDefault();

                result.AnswerActRegNumber = (actOther != null) ? (actOther.RegNumber + "/" + (actOther.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy")) : string.Empty;
            }
            var caseJuryCount = repo.AllReadonly<CaseLawUnitCount>()
                                    .Where(x => x.CaseId == act.CaseId)
                                    .Where(x => NomenclatureConstants.JudgeRole.JuriRolesListMain.Contains(x.JudgeRoleId))
                                    .Select(x => x.PersonCount).Sum();

            //var lawUnits = caseLawUnitService.CaseLawUnit_Select(act.CaseSession.CaseId, act.CaseSessionId).ToList();
            var lawUnits = (await GetCaseLawUnitsByAct(act.Id, act.CaseSessionId, false, true)).ToList();
            if (act.SignJudgeLawUnitId > 0 && NomenclatureConstants.ActType.HasSignJudge.Contains(act.ActTypeId))
            {
                var signJudge = repo.GetById<LawUnit>(act.SignJudgeLawUnitId.Value);
                lawUnits = new List<CaseLawUnit>()
                {
                    new CaseLawUnit()
                    {
                        LawUnitId = act.SignJudgeLawUnitId.Value,
                        LawUnit = signJudge,

                        //LawUnitNameShort = signJudge.FullName_MiddleNameInitials,
                        //LawUnitNameInitials = signJudge.FullName_Initials,
                        JudgeRoleId = NomenclatureConstants.JudgeRole.JudgeReporter,
                        JudgeDepartmentRoleId = NomenclatureConstants.JudgeDepartmentRole.Predsedatel
                    }
                };

            }
            bool appendReserveJury = caseJuryCount > lawUnits.Where(x => NomenclatureConstants.JudgeRole.JuriRolesListMain.Contains(x.JudgeRoleId)).Count();

            //var persons = casePersonService.CasePerson_Select(act.CaseSession.CaseId, act.CaseSessionId);
            foreach (var item in lawUnits)
            {
                var lawUnitNameShort = item.LawUnit.FullName_MiddleNameInitials;
                var lawUnitName = item.LawUnit.FullName;
                switch (item.JudgeRoleId)
                {
                    case NomenclatureConstants.JudgeRole.JudgeReporter:
                    case NomenclatureConstants.JudgeRole.Judge:
                    case NomenclatureConstants.JudgeRole.ExtJudge:
                    case NomenclatureConstants.JudgeRole.JudgeVAS:
                        {


                            var newItem = new LabelValueVM()
                            {
                                Value = lawUnitNameShort
                            };
                            newItem.Label = repo.AllReadonly<CourtLawUnit>()
                                                .Where(x => x.LawUnitId == item.LawUnitId && x.DateExpired == null)
                                                .Where(x => x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Appoint)
                                                .Where(x => x.DateFrom <= act.ActDate && (x.DateTo ?? DateTime.MaxValue) >= act.ActDate)
                                                .Select(x => x.Court.Label)
                                                .FirstOrDefault();

                            if (item.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                            {
                                result.JudgeReporter = lawUnitNameShort;
                            }
                            else
                            {
                                result.JudgeList.Add(newItem);
                            }

                            if (item.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel)
                            {
                                result.JudgeChairman = lawUnitNameShort;
                            }
                            else
                            {
                                result.AllJudgeList.Add(newItem);
                            }
                        }
                        break;
                    case NomenclatureConstants.JudgeRole.Jury:
                    case NomenclatureConstants.JudgeRole.ExtJury:
                        result.JuryList.Add(lawUnitNameShort);
                        break;
                    case NomenclatureConstants.JudgeRole.ReserveJury:
                        if (appendReserveJury)
                        {
                            result.JuryList.Add(lawUnitNameShort);
                        }
                        break;
                }
            }
            var depInfo = repo.AllReadonly<CaseLawUnit>()
                                    .Where(x => x.CaseSessionId == null)
                                    .Where(x => x.CourtDepartmentId > 0)
                                    .Where(x => x.CaseId == result.CaseId)
                                    .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                    .Where(x => x.DateFrom <= actDate && (x.DateTo ?? DateTime.MaxValue) > actDate)
                                    .Where(x => !NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId))
                                    .Select(x => new
                                    {
                                        DepartmentName = x.CourtDepartment.Label,
                                        CompartmentType = (x.CourtDepartment.ParentDepartment != null) ? x.CourtDepartment.ParentDepartment.DepartmentType.Label : "",
                                        CompartmentName = (x.CourtDepartment.ParentDepartment != null) ? x.CourtDepartment.ParentDepartment.Label : ""
                                    })
                                    .FirstOrDefault();
            if (depInfo != null)
            {
                result.DepartmentName = depInfo.DepartmentName;
                result.CompartmentType = depInfo.CompartmentType;
                result.CompartmentName = depInfo.CompartmentName;
            }

            List<CasePersonListVM> casePersonList = null;

            if (act.ActKindId > 0)
            {
                //Зареждане на всички данни, заради попълването на полета в бланките
                casePersonList = casePersonService.CasePerson_Select(caseSession.CaseId, act.CaseSessionId, true, false, false).ToList();

                FillLeftRightSide_CasePersons(casePersonList, result);
            }
            else
            {
                DateTime dateEnd = DateTime.Now.AddYears(100);
                Expression<Func<CasePerson, bool>> sessionCheck = x => x.CaseSessionId == act.CaseSessionId &&
                                        ((x.DateTo ?? dateEnd) >= x.CaseSession.DateFrom);


                //Бързо зареждане на основни данни за лицата
                casePersonList = await repo.AllReadonly<CasePerson>()
                                            .Where(x => x.CaseId == caseSession.CaseId && x.CaseSessionId == act.CaseSessionId)
                                            .Where(sessionCheck)
                                            .Where(FilterExpireInfo<CasePerson>(false))
                                            .Select(x => new CasePersonListVM
                                            {
                                                FullName = x.FullName,
                                                FirstName = x.FirstName,
                                                MiddleName = x.MiddleName,
                                                FamilyName = x.FamilyName,
                                                Family2Name = x.Family2Name,
                                                PersonRoleId = x.PersonRole.Id
                                            }).ToListAsync();
                //casePersonList = casePersonService.CasePersonFast_SelectForCasePreview(caseSession.CaseId, act.CaseSessionId).ToList();
            }

            foreach (var casePerson in casePersonList)
            {
                if (casePerson.PersonRoleId == NomenclatureConstants.PersonRole.Prokuror)
                {
                    result.ProsecutorList.Add(casePerson.FullName_MiddleNameInitials);
                }
            }

            result.SDorFirstJudge = result.JudgeReporter;
            if (string.IsNullOrEmpty(result.JudgeReporter))
            {
                result.SDorFirstJudge = result.JudgeList.Select(x => x.Value).FirstOrDefault();
            }

            result.F_NUM_ACT_Z = await Get_F_NUM_ACT_Z(caseSession.CaseId);
            result.F_DEBTOR_410_417_DELIVERY_DATA = string.Join(", ", await Get_F_DEBTOR_410_417_DELIVERY_DATA(caseSession.CaseId));
            result.F_AssignmentDocument_Num_V = string.Join(", ", await Get_F_AssignmentDocument_Num_V(caseSession.CaseId));
            result.F_AssignmentDocument_Num_V_414a = string.Join(", ", await Get_F_AssignmentDocument_Num_V_414a(caseSession.CaseId));

            return result;
        }

        /// <summary>
        /// Извличане на заповед за изпълнение номер и дата на постановяване от дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<string> Get_F_NUM_ACT_Z(int caseId)
        {
            return await repo.AllReadonly<CaseSessionAct>()
                             .Where(x => x.CaseId == caseId)
                             .Where(x => x.DateExpired == null)
                             .Where(x => x.ActTypeId == NomenclatureConstants.ActType.CommandmentForExec)
                             .Where(x => x.ActDeclaredDate != null)
                             .Select(x => x.RegNumber + "/" + (x.ActDeclaredDate ?? DateTime.Now).ToString("dd.MM.yyyy"))
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Връща всички имена на длъжници с доставени призовки
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<string>> Get_F_DEBTOR_410_417_DELIVERY_DATA(int caseId)
        {
            return await repo.AllReadonly<CaseNotification>()
                             .Where(x => x.CaseId == caseId)
                             .Where(x => x.DateExpired == null)
                             .Where(x => x.CaseNotificationActs
                                          .Any(a => a.CaseSessionAct.DateExpired == null &&
                                                    a.CaseSessionAct.ActTypeId == NomenclatureConstants.ActType.CommandmentForExec) ||
                                         (x.CaseSessionActId != null && x.CaseSessionAct.ActTypeId == NomenclatureConstants.ActType.CommandmentForExec))
                             .Where(x => x.CasePerson.PersonRoleId == NomenclatureConstants.PersonRole.Debtor)
                             .Where(x => x.NotificationStateId == NomenclatureConstants.NotificationState.Delivered)
                             .Select(x => x.CasePerson.FullName + " на " + (x.DeliveryDate ?? DateTime.Now).ToString("dd.MM.yyyy"))
                             .ToListAsync();
        }

        /// <summary>
        /// Извлича номерата на съпровождащи документи към дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<string>> Get_F_AssignmentDocument_Num_V(int caseId)
        {
            return await repo.AllReadonly<DocumentCaseInfo>()
                             .Where(x => x.Document.DocumentGroupId == 20)
                             .Where(x => NomenclatureConstants.DocumentType.BlankaRazporejdane.Contains(x.Document.DocumentTypeId))
                             .Where(x => x.Document.DateExpired == null)
                             .Where(x => x.CaseId == caseId)
                             .Select(x => x.Document.DocumentNumber + "/" + x.Document.DocumentDate.ToString("dd.MM.yyyy"))
                             .ToListAsync();
        }

        /// <summary>
        /// Извлича номерата на съпровождащ документ от точен тип възражение  от дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<string>> Get_F_AssignmentDocument_Num_V_414a(int caseId)
        {
            return await repo.AllReadonly<DocumentCaseInfo>()
                             .Where(x => x.Document.DocumentGroupId == 20)
                             .Where(x => x.Document.DocumentTypeId == 365)
                             .Where(x => x.Document.DateExpired == null)
                             .Where(x => x.CaseId == caseId)
                             .Select(x => x.Document.DocumentNumber + "/" + x.Document.DocumentDate.ToString("dd.MM.yyyy"))
                             .ToListAsync();
        }

        /// <summary>
        /// Извличане на тип акт по дело
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <param name="addDefaultElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetActTypesByCase(int caseSessionId, bool addDefaultElement = true)
        {
            var caseInfo = repo.AllReadonly<CaseSession>()
                               .Where(x => x.Id == caseSessionId)
                               .Select(x => new
                               {
                                   CourtTypeId = x.Case.Court.CourtTypeId,
                                   CaseInstanceId = x.Case.CaseType.CaseInstanceId,
                                   CaseGroupId = x.Case.CaseGroupId,
                                   SessionTypeGroup = x.SessionType.SessionTypeGroup
                               }).FirstOrDefault();

            var actTypes = repo.AllReadonly<ActTypeCourtLink>()
                               .Where(x => x.CaseGroupId == caseInfo.CaseGroupId &&
                                           x.CourtTypeId == caseInfo.CourtTypeId &&
                                           x.CaseInstanceId == caseInfo.CaseInstanceId)
                               .Select(x => x.ActTypeId)
                               .ToArray();

            var selectListItems = repo.AllReadonly<ActTypeSessionTypeGroup>()
                                      .Where(x => actTypes.Contains(x.ActTypeId) &&
                                                  x.SessionTypeGroup == caseInfo.SessionTypeGroup)
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.ActType.Label,
                                          Value = x.ActType.Id.ToString()
                                      })
                                      .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetActTypesFromCaseByCase(int caseId, int SessionTypeId, bool addDefaultElement = true)
        {
            var caseInfo = repo.AllReadonly<Case>()
                               .Where(x => x.Id == caseId)
                               .Select(x => new
                               {
                                   CourtTypeId = x.Court.CourtTypeId,
                                   CaseInstanceId = x.CaseType.CaseInstanceId,
                                   CaseGroupId = x.CaseGroupId
                               }).FirstOrDefault();

            var actTypes = repo.AllReadonly<ActTypeCourtLink>()
                               .Where(x => x.CaseGroupId == caseInfo.CaseGroupId &&
                                           x.CourtTypeId == caseInfo.CourtTypeId &&
                                           x.CaseInstanceId == caseInfo.CaseInstanceId)
                               .Select(x => x.ActTypeId)
                               .ToArray();

            var sessionType = repo.GetById<SessionType>(SessionTypeId) ?? new SessionType();

            var selectListItems = repo.AllReadonly<ActTypeSessionTypeGroup>()
                                      .Where(x => actTypes.Contains(x.ActTypeId) &&
                                                  x.SessionTypeGroup == sessionType.SessionTypeGroup)
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.ActType.Label,
                                          Value = x.ActType.Id.ToString()
                                      })
                                      .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Метод връщащ суми по хора, ако не е солидарно разпределено
        /// </summary>
        /// <param name="caseSessionAct">Данни за делото</param>
        /// <returns></returns>
        private string GetSumJointDistribution(CaseSessionActCommandVM caseSessionAct)
        {
            string row = string.Empty;

            if (caseSessionAct.FastProcessRequest == null)
                return row;

            if (caseSessionAct.FastProcessRequest.MoneyClaims == null)
                return row;

            foreach (var claim in caseSessionAct.FastProcessRequest.MoneyClaims)
            {
                if (string.IsNullOrEmpty(row))
                    row = caseSessionAct.FastProcessRequest.MoneyClaims.Count() > 1 ? "сумите: " : "сумата: ";
                else
                    row += ", ";

                //row += (caseSessionAct.IsInEuro ? "<b>" + (claim.TotalAmountEUR.ToString("### ### ##0.00") + NomenclatureConstants.CurrencyCode.EUR + " (" + Extensions.MoneyExtensions.MoneyToString(claim.TotalAmountEUR, NomenclatureConstants.Currency.EUR) + ")</b>") :
                //                                  "<b>" + (claim.TotalAmountBGN.ToString("### ### ##0.00") + NomenclatureConstants.CurrencyCode.BGN + " (" + Extensions.MoneyExtensions.MoneyToString(claim.TotalAmountBGN, NomenclatureConstants.Currency.BGN) + ")</b>"));

                row += "<b>" + (claim.Amount.ToString("### ### ##0.00") + " " + claim.CurrencyCode + " (" + Extensions.MoneyExtensions.MoneyToString(claim.Amount, claim.CurrencyCode) + ")</b>");

                row += ", представляваща " + caseSessionAct.getNomenclature(NomenclatureConstants.FPaliases.FP_MoneyClaimTypes, claim.MoneyClaimTypeCode).ToLower();

                //row += " по " + caseSessionAct.getNomenclature(NomenclatureConstants.FPaliases.FP_ClaimCircumstances, caseSessionAct.FastProcessRequest.ClaimCircumstances.ClaimCircumstancesCode).ToLower() +
                //       " № " + caseSessionAct.FastProcessRequest.ClaimCircumstances.Number + " от дата " + caseSessionAct.formatDate(caseSessionAct.FastProcessRequest.ClaimCircumstances.Date);

                if (!string.IsNullOrEmpty(claim.Description))
                    row += " - " + claim.Description;

                if (claim.DateFrom != null && claim.DateTo != null)
                    row += " за периода от " + caseSessionAct.formatDate(claim.DateFrom) + " до " + caseSessionAct.formatDate(claim.DateTo);

                if (claim.HasStatutoryinterest)
                    row += $", ведно със законната лихва за период от {caseSessionAct.formatDate(claim.StatutoryinterestDate)} до окончателното изплащане на вземането";
                else
                    row += "";
            }

            return row;
        }

        /// <summary>
        /// Метод връщащ името на елемент
        /// </summary>
        /// <param name="claimCode">Код на елемента</param>
        /// <param name="caseSessionAct">Данни за делото</param>
        /// <returns></returns>
        private FastProcessSelectItemVM GetNameElement(string claimCode, CaseSessionActCommandVM caseSessionAct)
        {
            string _type = claimCode[..1];

            switch (_type)
            {
                case "1":
                    {
                        FastProcessMoneyClaimVM _claim = caseSessionAct.FastProcessRequest.MoneyClaims.Where(c => "1|" + c.Gid == claimCode).FirstOrDefault();
                        return new()
                        {
                            Name = caseSessionAct.getNomenclature(NomenclatureConstants.FPaliases.FP_MoneyClaimTypes, _claim.MoneyClaimTypeCode).ToLower(),
                            DateFrom = _claim.DateFrom,
                            DateTo = _claim.DateTo,
                            Description = _claim.Description,
                            HasStatutoryinterest = _claim.HasStatutoryinterest,
                            StatutoryinterestDate = _claim.StatutoryinterestDate
                        };
                    }
                case "2":
                    {
                        FastProcessItemSubstitutionClaimVM _itemSubstitutionClaims = caseSessionAct.FastProcessRequest.ItemSubstitutionClaims.Where(s => "2|" + s.Gid == claimCode).FirstOrDefault();
                        return new()
                        {
                            Name = _itemSubstitutionClaims.TypeName.ToLower(),
                            Description = _itemSubstitutionClaims.QuantityText,
                            HasStatutoryinterest = false
                        };
                    }
                case "3":
                    {
                        return new()
                        {
                            Name = caseSessionAct.FastProcessRequest.ItemClaim.Description.ToLower(),
                            HasStatutoryinterest = false
                        };
                    }
                case "4":
                    {
                        FastProcessExpenseVM _expenses = caseSessionAct.FastProcessRequest.Expenses.Where(e => "4|" + e.Gid == claimCode).FirstOrDefault();
                        return new()
                        {
                            Name = caseSessionAct.getNomenclature(NomenclatureConstants.FPaliases.FP_ExpenseTypes, _expenses.ExpenseTypeCode).ToLower(),
                            Description = _expenses.Description,
                            HasStatutoryinterest = false
                        };
                    }
                default: return new();
            }
        }

        /// <summary>
        /// Метод връщащ суми по хора, ако не е солидарно разпределено
        /// </summary>
        /// <param name="caseSessionAct">Данни за делото</param>
        /// <returns></returns>
        private string[] GetSumIsNotJointDistribution(CaseSessionActCommandVM caseSessionAct)
        {
            List<string> result = [];

            string personName = string.Empty;
            string row = string.Empty;

            if (caseSessionAct.FastProcessRequest == null)
                return result.ToArray();

            if (caseSessionAct.FastProcessRequest.DebtDistributions == null)
                return result.ToArray();

            foreach (var debt in caseSessionAct.FastProcessRequest.DebtDistributions.OrderBy(x => x.PersonCode).ThenBy(x => x.Index))
            {
                BaseRequestPersonInfoVM _person = caseSessionAct.FastProcessRequest.RightSide.Where(p => p.PersonGid.ToLower() == debt.PersonCode.ToLower()).FirstOrDefault();
                FastProcessSelectItemVM _selectItem = GetNameElement(debt.ClaimCode, caseSessionAct);

                if (_person.FullName != personName)
                {
                    if (!string.IsNullOrEmpty(row))
                    {
                        result.Add(row + ".");
                    }

                    personName = _person.FullName;
                    string _personIden = !string.IsNullOrEmpty(_person.Identifier) ? " с ЕГН: " + _person.Identifier : string.Empty;
                    string _personAddress = _person.Addresses.Select(x => " с адрес: " + x.FullAddress).FirstOrDefault();
                    row = $"Длъжникът {_person.FullName} {_personIden}{_personAddress} да заплати на кредитора {string.Join(", ", caseSessionAct.CaseSessionActPrint.LeftSideOnlyName)} следните суми: {_selectItem.Name} в размер на ";
                }
                else
                {
                    row += ", " + _selectItem.Name + " в размер на ";
                }

                //row += (caseSessionAct.IsInEuro ? "<b>" + (debt.TotalAmountEUR.ToString("### ### ##0.00") + NomenclatureConstants.CurrencyCode.EUR + " (" + Extensions.MoneyExtensions.MoneyToString(debt.TotalAmountEUR, NomenclatureConstants.Currency.EUR) + ")</b>") :
                //                                  "<b>" + (debt.TotalAmountBGN.ToString("### ### ##0.00") + NomenclatureConstants.CurrencyCode.BGN + " (" + Extensions.MoneyExtensions.MoneyToString(debt.TotalAmountBGN, NomenclatureConstants.Currency.BGN) + ")</b>"));

                row += "<b>" + (debt.Amount.ToString("### ### ##0.00") + " " + debt.CurrencyCode + " (" + Extensions.MoneyExtensions.MoneyToString(debt.Amount, debt.CurrencyCode) + ")</b>");

                if (_selectItem.DateFrom != null && _selectItem.DateTo != null)
                    row += " за периода от " + caseSessionAct.formatDate(_selectItem.DateFrom) + " до " + caseSessionAct.formatDate(_selectItem.DateTo);

                //row += " по " + caseSessionAct.getNomenclature(NomenclatureConstants.FPaliases.FP_ClaimCircumstances, caseSessionAct.FastProcessRequest.ClaimCircumstances.ClaimCircumstancesCode).ToLower() +
                //       " № " + caseSessionAct.FastProcessRequest.ClaimCircumstances.Number + " от дата " + caseSessionAct.formatDate(caseSessionAct.FastProcessRequest.ClaimCircumstances.Date);

                if (!string.IsNullOrEmpty(_selectItem.Description))
                    row += " - " + _selectItem.Description;

                if (_selectItem.HasStatutoryinterest)
                    row += $", ведно със законна лихва за период от {caseSessionAct.formatDate(_selectItem.StatutoryinterestDate)} до изплащане на вземането";
                else
                    row += "";
            }

            if (!string.IsNullOrEmpty(row))
            {
                result.Add(row + ".");
            }

            return result.ToArray();
        }

        /// <summary>
        /// Принтиране на заповеди
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CaseSessionActCommandVM> CaseSessionActCommand_GetForPrint(int id)
        {
            CaseSessionActCommandVM result = new CaseSessionActCommandVM();
            result.CaseSessionActPrint = await CaseSessionAct_GetForPrint(id);

            // TO-DO това да се размаркира и да се изтрие долното зареждане
            //if (!result.CaseSessionActPrint.CaseByDocumentRequest)
            //    result.CaseFastProcessView = caseFastProcessService.Select(result.CaseSessionActPrint.CaseId);
            //else
            //{
            //    result.Nomenclatures = await documentRequestService.LoadAliasNomenclatures(NomenclatureConstants.FPaliases.FastProcessNomenclatures);
            //    result.IsInEuro = userContext.IsPeriodEuro;
            //    result.FastProcessRequest = (FastProcessRequestVM)await documentRequestService.GetDocumentRequestById(0, result.CaseSessionActPrint.CaseId);
            //}

            result.GenerateExecProcess = result.CaseSessionActPrint.GenerateExecProcess;
            result.CaseFastProcessView = caseFastProcessService.Select(result.CaseSessionActPrint.CaseId);
            result.IsInEuro = userContext.IsPeriodEuro;
            result.FastProcessRequest = (FastProcessRequestVM)await documentRequestService.GetDocumentRequestById(0, result.CaseSessionActPrint.CaseId, true);
            result.Nomenclatures = await documentRequestService.LoadAliasNomenclatures(NomenclatureConstants.FPaliases.FastProcessNomenclatures, result.FastProcessRequest);

            if (!result.CaseSessionActPrint.CaseByDocumentRequest)
            {
                result.CaseFastProcessView.JointDistribution = true;
                var caseMoneyClaim = result.CaseFastProcessView.CaseMoneyClaims.FirstOrDefault();
                if (caseMoneyClaim != null)
                {
                    var caseMoneyCollection = caseMoneyClaim.CaseMoneyCollections.FirstOrDefault();
                    if (caseMoneyCollection != null)
                        result.CaseFastProcessView.JointDistribution = caseMoneyCollection.JointDistributionBool;
                }

                result.CaseSessionActPrint.LeftRightSide_410_417 = result.CaseSessionActPrint.RightSide_410_417 +
                                                                   ((result.CaseSessionActPrint.RightSide_410_417_Count > 1) ? " да заплатят" : " да заплати") +
                                                                   ((result.CaseSessionActPrint.LeftSide_410_417_Count > 1 ? (result.CaseFastProcessView.JointDistribution ? " солидарно на " : " разделено на ") : " на ")) +
                                                                   result.CaseSessionActPrint.LeftSide_410_417;
            }
            else
            {
                if (!result.JointDistribution)
                    result.SumIsNotJointDistribution = GetSumIsNotJointDistribution(result);
                else
                {
                    result.SumJointDistributionWithoutPoint = GetSumJointDistribution(result);
                    result.SumJointDistribution = result.SumJointDistributionWithoutPoint + ".";
                }

                if (result.FastProcessRequest != null)
                {
                    result.FastProcessRequestExpenses = result.FastProcessRequest.Expenses.Any() ? string.Join(", ", result.FastProcessRequest.Expenses.Select(x => $"{(result.getNomenclature(NomenclatureConstants.FPaliases.FP_ExpenseTypes, x.ExpenseTypeCode)).ToLower()} в размер на: {"<b>" + (result.IsInEuro ? x.TotalAmountEUR.ToString("### ### ##0.00") + " EUR" + " (" + Extensions.MoneyExtensions.MoneyToString(x.TotalAmountEUR, "EUR") + ")" : x.TotalAmountBGN.ToString("### ### ##0.00") + " BGN" + " (" + Extensions.MoneyExtensions.MoneyToString(x.TotalAmountBGN, "BGN") + ")") + "</b>"}")) : string.Empty;
                }
                result.CaseSessionActPrint.LeftRightSide_410_417 = result.CaseSessionActPrint.RightSide_410_417 +
                                                                   ((result.CaseSessionActPrint.RightSide_410_417_Count > 1) ? " да заплатят" : " да заплати") +
                                                                   ((result.CaseSessionActPrint.RightSide_410_417_Count > 1 ? (result.JointDistribution ? " при условията на солидарност на " : " разделено на ") : " на ")) +
                                                                   result.CaseSessionActPrint.LeftSide_410_417 +
                                                                   (result.CaseSessionActPrint.LeftSide_410_417_Count > 1 ? ", при условията на активна солидарност." : string.Empty);

                result.CaseSessionActPrint.LeftWithOutRoleRightSide_410_417 = ((result.CaseSessionActPrint.RightSide_410_417_Count > 1) ? (NomenclatureConstants.ActKindBlankName.execlist.Contains(result.CaseSessionActPrint.ActKindBlankName) ? " " : "Длъжниците ") : (NomenclatureConstants.ActKindBlankName.execlist.Contains(result.CaseSessionActPrint.ActKindBlankName) ? " " : "Длъжникът ")) +
                                                                              result.CaseSessionActPrint.RightSidesWithAddress +
                                                                              ((result.CaseSessionActPrint.RightSide_410_417_Count > 1) ? ", да заплатят" : ", да заплати") +
                                                                              ((result.CaseSessionActPrint.RightSide_410_417_Count > 1 ? (result.JointDistribution ? " при условията на солидарност на " : " разделено на ") : " на ")) +
                                                                              result.CaseSessionActPrint.LeftSideWithOutRole_410_417 +
                                                                              (result.CaseSessionActPrint.LeftSide_410_417_Count > 1 ? ", при условията на активна солидарност." : string.Empty);

                result.CaseSessionActPrint.LeftRightSide_410_417_Expenses = result.CaseSessionActPrint.RightSidesWithAddress +
                                                                            ((result.CaseSessionActPrint.RightSide_410_417_Count > 1) ? ", да заплатят следните разноски на " : ", да заплати следните разноски на ") +
                                                                            result.CaseSessionActPrint.LeftSide_410_417;

            }

            return result;
        }

        /// <summary>
        /// Извличане на вид акт по тип
        /// </summary>
        /// <param name="actTypeId"></param>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public List<SelectListItem> GetActKindsByActType(int actTypeId, int? caseId)
        {
            Expression<Func<ActKind, bool>> isFastProcessWhere = x => true;
            if (caseId != null)
            {
                bool isFastProcess = repo.AllReadonly<Case>()
                                         .Where(x => x.Id == caseId)
                                         .Select(x => x.IsFastProcess)
                                         .FirstOrDefault() ?? false;

                isFastProcessWhere = x => (x.IsFastProcess == isFastProcess || x.IsFastProcess == null);
            }

            return repo.AllReadonly<ActKind>()
                       .Where(isFastProcessWhere)
                       .Where(x => x.ActTypeId == actTypeId)
                       .ToSelectList(true);
        }

        /// <summary>
        /// Извличане на актове за архивиране
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownListForArchive(int caseId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = repo.AllReadonly<CaseSessionAct>()
                             .Where(x => x.CaseSession.CaseId == caseId)
                             .Where(x => x.ActDeclaredDate != null)
                             .Where(x => x.DateExpired == null)
                             .OrderBy(x => x.ActDeclaredDate)
                .Select(x => new SelectListItem()
                {
                    Text = x.ActType.Label + " " + x.ActState.Label + " " + (x.RegNumber ?? string.Empty) + ((x.RegDate != null) ? "/" + (x.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty),
                    Value = x.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        /// <summary>
        /// Генериране на екли номер
        /// </summary>
        /// <param name="model"></param>
        private async Task<bool> GenerateActEcliNumber(CaseSessionAct model)
        {
            if (!model.IsFinalDoc || !string.IsNullOrEmpty(model.EcliCode) || (model.ActDeclaredDate != null) || (model.DateExpired != null))
            {
                return false;
            }

            int caseId = model.CaseId ?? 0;
            if (caseId == 0)
            {
                caseId = await repo.AllReadonly<CaseSession>()
                                .Where(this.FilterExpireInfo<CaseSession>(false))
                                .Where(x => x.Id == model.CaseSessionId)
                                .Select(x => x.CaseId)
                                .FirstOrDefaultAsync();
            }

            var countFinalActs = await repo.AllReadonly<CaseSessionAct>()
                                .Where(this.FilterExpireInfo<CaseSessionAct>(false))
                                .Where(x => x.CaseSession.CaseId == caseId)
                                .Where(x => x.IsFinalDoc == true)
                                .Where(x => x.Id != model.Id)
                                .CountAsync() + 1;

            var caseInfo = await repo.AllReadonly<Case>()
                                .Where(x => x.Id == caseId)
                                .Select(x => new
                                {
                                    Year = x.RegDate.Year,
                                    ShortNumber = x.ShortNumberValue,
                                    CourtCode = x.Court.EcliCode,
                                    CharacterCode = x.CaseCharacter.Code
                                })
                                .FirstOrDefaultAsync();

            string result = $"ECLI:BG:{caseInfo.CourtCode}:{model.ActDate.Value.Year:D4}:{caseInfo.Year:D4}{caseInfo.CharacterCode}{caseInfo.ShortNumber:D5}.{countFinalActs:D3}";

            model.EcliCode = result;
            return true;
        }

        /// <summary>
        /// Автоматично обезличаване на съдебни актове
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public List<DepersonalizationHistoryItem> AutoDepersonalizeAct_GenerateRules(int caseId)
        {
            //var _case = model.CaseSession.Case;

            var _persons = repo.AllReadonly<Case>()
                                .Include(x => x.CasePersons)
                                .ThenInclude(x => x.Addresses)
                                .ThenInclude(x => x.Address)
                                .Where(x => x.Id == caseId)
                                .SelectMany(x => x.CasePersons.Where(p => p.CaseSessionId == null))
                                .ToList();

            List<DepersonalizationHistoryItem> rules = new List<DepersonalizationHistoryItem>();
            //Добавяне на адреси на лица
            foreach (var _person in _persons)
            {
                if (_person.Addresses != null)
                    foreach (var _adr in _person.Addresses)
                    {
                        rules.Add(new DepersonalizationHistoryItem()
                        {
                            SearchValue = _adr.Address.FullAddress
                        });
                        if (!string.IsNullOrEmpty(_adr.Address.Email))
                        {
                            rules.Add(new DepersonalizationHistoryItem()
                            {
                                SearchValue = _adr.Address.Email
                            });
                        }
                        if (!string.IsNullOrEmpty(_adr.Address.Phone))
                        {
                            rules.Add(new DepersonalizationHistoryItem()
                            {
                                SearchValue = _adr.Address.Phone
                            });
                        }
                    }
            }
            //Добавяне на страни
            foreach (var _person in _persons)
            {
                //Само за физически лица
                if (!(_person.UicTypeId == NomenclatureConstants.UicTypes.EGN
                    || _person.UicTypeId == NomenclatureConstants.UicTypes.LNCh
                    || _person.UicTypeId == NomenclatureConstants.UicTypes.BirthDate))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(_person.FirstName))
                {
                    rules.Add(new DepersonalizationHistoryItem()
                    {
                        SearchValue = _person.FirstName,
                        ReplaceValue = $"{_person.FirstName[0]}."
                    });
                }
                if (!string.IsNullOrEmpty(_person.MiddleName))
                {
                    rules.Add(new DepersonalizationHistoryItem()
                    {
                        SearchValue = _person.MiddleName,
                        ReplaceValue = $"{_person.MiddleName[0]}."
                    });
                }
                if (!string.IsNullOrEmpty(_person.FamilyName))
                {
                    rules.Add(new DepersonalizationHistoryItem()
                    {
                        SearchValue = _person.FamilyName,
                        ReplaceValue = $"{_person.FamilyName[0]}."
                    });
                }
                if (!string.IsNullOrEmpty(_person.Family2Name))
                {
                    rules.Add(new DepersonalizationHistoryItem()
                    {
                        SearchValue = _person.Family2Name,
                        ReplaceValue = $"{_person.Family2Name[0]}."
                    });
                }

                //rules.Add(new DepersonalizationHistoryItem()
                //{
                //    SearchValue = _person.FullName,
                //    ReplaceValue = _person.FullName_Initials
                //});

                if (!string.IsNullOrEmpty(_person.Uic))
                {
                    rules.Add(new DepersonalizationHistoryItem()
                    {
                        SearchValue = _person.Uic
                    });
                }
            }
            foreach (var item in rules)
            {
                item.SearchValue = item.SearchValue.Decode();
            }
            return rules.Where(x => !string.IsNullOrEmpty(x.SearchValue)).Where(x => x.SearchValue?.Length > 2).ToList();
        }

        /// <summary>
        /// Обезличаване на съдебни актове
        /// </summary>
        /// <param name="rules"></param>
        /// <param name="html"></param>
        /// <returns></returns>
        public string AutoDepersonalizeAct(IEnumerable<DepersonalizationHistoryItem> rules, string html)
        {
            string result = html;
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }
            string defReplaceValue = "**********";

            //Обезличава, започвайки от най-дългия текст
            foreach (var item in rules.OrderByDescending(x => x.SearchValue.Length))
            {
                item.ReplaceValue = item.ReplaceValue ?? defReplaceValue;
                item.IsCaseSensitive = false;

                //Escape-ват се специалните символи но интервала се връща като чист тест - той с епремахва после
                string searchValue = Regex.Escape(item.SearchValue)
                                        .Replace("\\ ", " ", StringComparison.InvariantCultureIgnoreCase);

                //Премахва повече от един интервал, включително и HTML интервалите &nbsp;
                var searchText = searchValue.Replace(" ", "[&nbsp;]{0,}[ ]{1,}[&nbsp;]{0,}", StringComparison.InvariantCultureIgnoreCase);
                result = Regex.Replace(result, searchText, item.ReplaceValue, RegexOptions.IgnoreCase);
            }

            // caseService.SaveDataDepersonalizationHistory(model.CaseId, replaceItems, int.Parse(model.SourceId));

            var matches = Regex.Matches(result, @"(?<!\d)\d{10}(?!\d)", RegexOptions.IgnoreCase);
            foreach (var item in matches)
            {
                var text = item.ToString();
                if (Utils.Validation.IsEGN(text))
                {
                    result = Regex.Replace(result, text, defReplaceValue, RegexOptions.IgnoreCase);
                }
            }


            return result;
        }

        /// <summary>
        /// Извличане на разводи по ид на акт
        /// </summary>
        /// <param name="actId"></param>
        /// <returns></returns>
        public CaseSessionActDivorce GetDivorceByActId(int actId)
        {
            return repo.AllReadonly<CaseSessionActDivorce>().Where(x => x.CaseSessionActId == actId && x.DateExpired == null).FirstOrDefault();
        }

        /// <summary>
        /// Запис на развод
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) CaseSessionActDivorce_SaveData(CaseSessionActDivorce model)
        {
            try
            {
                if (model.Id == 0)
                {
                    var exists = repo.AllReadonly<CaseSessionActDivorce>()
                                           .Where(x => x.Id != model.Id)
                                           .Where(x => x.CaseSessionActId == model.CaseSessionActId)
                                           .Where(x => x.DateExpired == null)
                                           .Any();
                    if (exists == true)
                    {
                        return (result: false, errorMessage: "Вече има въведени данни");
                    }
                }

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseSessionActDivorce>(model.Id);

                    saved.CountryCode = model.CountryCode;
                    saved.CountryCodeDate = model.CountryCodeDate;
                    saved.MarriageNumber = model.MarriageNumber;
                    saved.MarriageDate = model.MarriageDate;
                    saved.MarriagePlace = model.MarriagePlace;
                    saved.MarriageFault = model.MarriageFault;
                    saved.MarriageFaultDescription = model.MarriageFaultDescription;
                    saved.ChildrenUnder18 = model.ChildrenUnder18;
                    saved.ChildrenOver18 = model.ChildrenOver18;
                    saved.CasePersonManId = model.CasePersonManId;
                    saved.BirthDayMan = model.BirthDayMan;
                    saved.NameAfterMarriageMan = model.NameAfterMarriageMan;
                    saved.MarriedStatusBeforeMan = model.MarriedStatusBeforeMan;
                    saved.MarriageCountMan = model.MarriageCountMan;
                    saved.DivorceCountMan = model.DivorceCountMan;
                    saved.NationalityMan = model.NationalityMan;
                    saved.EducationMan = model.EducationMan;
                    saved.CasePersonWomanId = model.CasePersonWomanId;
                    saved.BirthDayWoman = model.BirthDayWoman;
                    saved.NameAfterMarriageWoman = model.NameAfterMarriageWoman;
                    saved.MarriedStatusBeforeWoman = model.MarriedStatusBeforeWoman;
                    saved.MarriageCountWoman = model.MarriageCountWoman;
                    saved.DivorceCountWoman = model.DivorceCountWoman;
                    saved.NationalityWoman = model.NationalityWoman;
                    saved.EducationWoman = model.EducationWoman;
                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;
                }
                else
                {
                    //Insert
                    if (counterService.Counter_GetDivorceCounter(model, userContext.CourtId) == false)
                    {
                        return (result: false, errorMessage: "Проблем при вземане на номер");
                    }

                    model.UserId = userContext.UserId;
                    model.DateWrt = DateTime.Now;
                    repo.Add<CaseSessionActDivorce>(model);
                }

                repo.SaveChanges();
                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CaseSessionActDivorce Id={model.Id}");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Премахване на съобщение за граждански брак
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) CaseSessionActDivorce_SaveExpired(ExpiredInfoVM model)
        {
            try
            {
                var expireObject = repo.GetById<CaseSessionActDivorce>(model.Id);

                var haveDocumentTemplate = repo.AllReadonly<DocumentTemplate>()
                                           .Where(x => x.DateExpired == null)
                                           .Where(x => x.SourceType == SourceTypeSelectVM.CaseSessionActDivorce)
                                           .Where(x => x.SourceId == model.Id)
                                           .Any();
                if (haveDocumentTemplate)
                {
                    return (result: false, errorMessage: "За съобщението има изготвен документ");
                }

                expireObject.DateExpired = DateTime.Now;
                expireObject.UserExpiredId = userContext.UserId;
                expireObject.DescriptionExpired = model.DescriptionExpired;

                repo.SaveChanges();
                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при премахване на съобщение за прекратен граждански брак с Id={model.Id}");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Извличане на актове за комбо
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownList_CaseSessionAct(int CaseId, bool finalOnly = true, bool addDefaultElement = true, bool addAllElement = false)
        {
            Expression<Func<CaseSessionAct, bool>> whereIsFinal = x => x.IsFinalDoc == true;
            if (!finalOnly)
            {
                whereIsFinal = x => true;
            }

            var result = repo.AllReadonly<CaseSessionAct>()
                             .Where(x => x.CaseSession.CaseId == CaseId && x.ActDeclaredDate != null)
                             .Where(whereIsFinal)
                             .Where(FilterExpireInfo<CaseSessionAct>(false))
                             .Select(x => new SelectListItem()
                             {
                                 Text = x.ActType.Label + " - " + x.RegNumber + "/" + (x.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"),
                                 Value = x.Id.ToString()
                             })
                             .ToList();

            if (result.Count != 1)
            {
                if (addDefaultElement)
                {
                    result = result
                        .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                        .ToList();
                }

                if (addAllElement)
                {
                    result = result
                        .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                        .ToList();
                }
            }

            return result;
        }

        /// <summary>
        /// Извличане на актове от движение на дело за комбо
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="CourtId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_CaseSessionActFromMigration(int CaseId, int CourtId, bool addDefaultElement = true, bool addAllElement = false)
        {

            DateTime dateNow = DateTime.Now;

            var initCasesQuery = repo.AllReadonly<CaseMigration>()
                                     .Where(m => m.DateExpired == null &&
                                                 m.CaseId == CaseId)
                                     .Select(m => m.InitialCaseId);

            var caseIdsQuery = repo.AllReadonly<CaseMigration>()
                                   .Where(mm => initCasesQuery.Contains(mm.InitialCaseId) &&
                                               mm.DateExpired == null &&
                                               mm.Case.CourtId == CourtId)
                                   .Select(mm => mm.CaseId);

            List<SelectListItem> result = repo.AllReadonly<CaseSessionAct>()
                                              .Where(a => caseIdsQuery.Contains(a.CaseId ?? 0) &&
                                                          a.IsFinalDoc &&
                                                          !string.IsNullOrEmpty(a.RegNumber) &&
                                                          a.DateExpired == null)
                                              .OrderByDescending(a => a.RegDate)
                                              .Select(a => new SelectListItem()
                                              {
                                                  Text = $"{a.ActType.Label} {a.RegNumber}/{(a.RegDate ?? dateNow).ToString("dd.MM.yyyy")} Дело: {a.Case.RegNumber}/{a.Case.RegDate.ToString("dd.MM.yyyy")}",
                                                  Value = a.Id.ToString()
                                              })
                                              .ToList();

            if (!result.Any())
            {
                result = repo.AllReadonly<CaseSessionAct>()
                             .Where(a => a.CaseId == CaseId &&
                                         a.IsFinalDoc &&
                                         !string.IsNullOrEmpty(a.RegNumber) &&
                                         a.DateExpired == null)
                             .OrderByDescending(a => a.RegDate)
                             .Select(a => new SelectListItem()
                             {
                                 Text = $"{a.ActType.Label} {a.RegNumber}/{(a.RegDate ?? dateNow).ToString("dd.MM.yyyy")} Дело: {a.Case.RegNumber}/{a.Case.RegDate.ToString("dd.MM.yyyy")}",
                                 Value = a.Id.ToString()
                             })
                             .ToList();
            }

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        /// <summary>
        /// Справка за изпълнителни листове
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionActELSprVM> CaseSessionActELSpr_Select(int courtId, CaseSessionActELSprFilterVM model)
        {
            DateTime fromDateNull = (model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom).Date;
            DateTime toDateNull = (model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            Expression<Func<CaseSessionAct, bool>> regNumberSearch = x => true;
            if (!string.IsNullOrEmpty(model.RegNumber))
                regNumberSearch = x => EF.Functions.ILike(x.RegNumber, model.RegNumber.ToPaternSearch());

            Expression<Func<CaseSessionAct, bool>> actKindIdSearch = x => true;
            if (model.ActKindId > 0)
                actKindIdSearch = x => x.ActKindId == model.ActKindId;

            Expression<Func<CaseSessionAct, bool>> leftSideSearch = x => true;
            if (!string.IsNullOrEmpty(model.LeftSide))
                leftSideSearch = x => x.Case.CasePersons.Any(p => p.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.LeftSide &&
                                                                              p.CaseSessionId == null &&
                                                                              EF.Functions.ILike(p.FullName, model.LeftSide.ToPaternSearch()));

            Expression<Func<CaseSessionAct, bool>> rightSideSearch = x => true;
            if (!string.IsNullOrEmpty(model.RightSide))
                rightSideSearch = x => x.Case.CasePersons.Any(p => p.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.RightSide &&
                                                                              p.CaseSessionId == null &&
                                                                              EF.Functions.ILike(p.FullName, model.RightSide.ToPaternSearch()));

            return repo.AllReadonly<CaseSessionAct>()
                       .Where(x => (x.Case.CourtId == courtId) &&
                                   ((fromDateNull <= x.RegDate) && (x.RegDate <= toDateNull)) &&
                                   (x.ActTypeId == NomenclatureConstants.ActType.ExecListPrivatePerson))
                       .Where(regNumberSearch)
                       .Where(actKindIdSearch)
                       .Where(leftSideSearch)
                       .Where(rightSideSearch)
                       .Select(x => new CaseSessionActELSprVM()
                       {
                           Id = x.Id,
                           CaseId = x.CaseId ?? 0,
                           RegNumber = x.RegNumber,
                           RegDate = (x.RegDate ?? DateTime.Now),
                           LeftSide = string.Join(", ", x.Case.CasePersons.Where(p => p.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.LeftSide && p.CaseSessionId == null).Select(p => p.FullName)),
                           RightSide = string.Join(", ", x.Case.CasePersons.Where(p => p.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.RightSide && p.CaseSessionId == null).Select(p => p.FullName)),
                           ActKindName = x.ActKind.Label
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Извличане на данни за справка за съдебни актове
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<CaseSessionActReportVM> CaseSessionActReport_Select(CaseSessionActReportFilterVM filter)
        {
            DateTime dateNow = DateTime.Now;

            filter.DateFrom = filter.DateFrom.ForceStartDateWithAddYear(-100);
            filter.DateTo = filter.DateTo.ForceEndDateWithAddYear(100);
            filter.ActInforcedDateFrom = filter.ActInforcedDateFrom.ForceStartDateWithAddYear(-100);
            filter.ActInforcedDateTo = filter.ActInforcedDateTo.ForceEndDateWithAddYear(100);

            Expression<Func<CaseSessionAct, bool>> whereRegDate = x => true;
            if (filter.DateFrom != null && filter.DateTo != null)
                whereRegDate = x => x.RegDate >= filter.DateFrom && x.RegDate <= filter.DateTo;

            Expression<Func<CaseSessionAct, bool>> whereInforceDate = x => true;
            if (filter.ActInforcedDateFrom != null && filter.ActInforcedDateTo != null)
                whereInforceDate = x => x.ActInforcedDate >= filter.ActInforcedDateFrom && x.ActInforcedDate <= filter.ActInforcedDateTo;

            Expression<Func<CaseSessionAct, bool>> caseGroupIdWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupIdWhere = x => x.Case.CaseGroupId == filter.CaseGroupId;

            Expression<Func<CaseSessionAct, bool>> caseTypeIdWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeIdWhere = x => x.Case.CaseTypeId == filter.CaseTypeId;

            Expression<Func<CaseSessionAct, bool>> caseCodeIdWhere = x => true;
            if (filter.CaseCodeId > 0)
                caseCodeIdWhere = x => x.Case.CaseCodeId == filter.CaseCodeId;

            Expression<Func<CaseSessionAct, bool>> processPriorityIdWhere = x => true;
            if (filter.ProcessPriorityId > 0)
                processPriorityIdWhere = x => x.Case.ProcessPriorityId == filter.ProcessPriorityId;

            Expression<Func<CaseSessionAct, bool>> actTypeIdWhere = x => true;
            if (filter.ActTypeId > 0)
                actTypeIdWhere = x => x.ActTypeId == filter.ActTypeId;

            Expression<Func<CaseSessionAct, bool>> actStateIdWhere = x => true;
            if (filter.ActStateId > 0)
                actStateIdWhere = x => x.ActStateId == filter.ActStateId;

            Expression<Func<CaseSessionAct, bool>> documentGroupIdWhere = x => true;
            if (filter.DocumentGroupId > 0)
                documentGroupIdWhere = x => x.Case.Document.DocumentGroupId == filter.DocumentGroupId;

            Expression<Func<CaseSessionAct, bool>> documentTypeIdWhere = x => true;
            if (filter.DocumentTypeId > 0)
                documentTypeIdWhere = x => x.Case.Document.DocumentTypeId == filter.DocumentTypeId;

            Expression<Func<CaseSessionAct, bool>> actComplainResultIdWhere = x => true;
            if (filter.ActComplainResultId > 0)
                actComplainResultIdWhere = x => x.ActComplainResultId == filter.ActComplainResultId;

            Expression<Func<CaseSessionAct, bool>> sessionResultIdWhere = x => true;
            if (filter.SessionResultId > 0)
                sessionResultIdWhere = x => x.CaseSession.CaseSessionResults.Any(r => r.SessionResultId == filter.SessionResultId &&
                                                                                      r.DateExpired == null);

            Expression<Func<CaseSessionAct, bool>> judgeReporterIdWhere = x => true;
            if (filter.JudgeReporterId > 0)
                judgeReporterIdWhere = x => x.CaseSession.CaseLawUnits.Any(a => (a.DateTo ?? dateNow.AddYears(100)).Date >= x.CaseSession.DateFrom.Date &&
                                                                                a.LawUnitId == filter.JudgeReporterId &&
                                                                                a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            Expression<Func<CaseSessionAct, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.Case.CaseCodeId ?? 0);
            }

            return repo.AllReadonly<CaseSessionAct>()
                       .Where(x => x.CourtId == userContext.CourtId &&
                                   x.DateExpired == null &&
                                  !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                       .Where(whereRegDate)
                       .Where(whereInforceDate)
                       .Where(caseGroupIdWhere)
                       .Where(caseTypeIdWhere)
                       .Where(caseCodeIdWhere)
                       .Where(processPriorityIdWhere)
                       .Where(actTypeIdWhere)
                       .Where(actStateIdWhere)
                       .Where(documentGroupIdWhere)
                       .Where(documentTypeIdWhere)
                       .Where(actComplainResultIdWhere)
                       .Where(sessionResultIdWhere)
                       .Where(judgeReporterIdWhere)
                       .Where(caseCodeIdsWhere)
                       .Select(x => new CaseSessionActReportVM()
                       {
                           Id = x.Id,
                           CaseId = x.CaseId ?? 0,
                           ActRegNumYear = x.RegNumber + "/" + (x.RegDate ?? DateTime.Now).Year + "г.",
                           ActTypeLabel = x.ActType.Label,
                           RegDate = x.RegDate,
                           ReturnDate = x.ActDate,
                           ActInforcedDate = x.ActInforcedDate,
                           CaseActInfoLabel = x.Case.CaseType.Code + " " + x.Case.RegNumber,
                           DocumentInfo = x.Case.Document.DocumentType.Label + " " + x.Case.Document.DocumentNumber + "/" + x.Case.Document.DocumentDate.ToString("dd.MM.yyyy"),
                           ActStateName = x.ActState.Label,
                           JudgeReport = x.CaseSession.CaseLawUnits.Where(l => l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                                               (l.DateTo ?? DateTime.Now.AddYears(100)) >= x.CaseSession.DateFrom)
                                                                   .OrderByDescending(l => l.DateFrom)
                                                                   .Select(l => l.LawUnit.FullName + ((l.CourtDepartment != null) ? " състав: " + l.CourtDepartment.Label : string.Empty))
                                                                   .FirstOrDefault(),
                           ActComplainResultLabel = x.ActComplainResultId != null ? x.ActComplainResult.Label : string.Empty
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Вземане на последне подписал
        /// </summary>
        /// <param name="caseLawunitid"></param>
        /// <param name="actId"></param>
        /// <returns></returns>
        public DateTime GetLastSignCaseDate(int caseLawunitid, int? actId)
        {
            var caseLawUnit = repo.GetById<CaseLawUnit>(caseLawunitid);
            Expression<Func<CaseSessionAct, bool>> selectAct = x => true;
            if (actId != null)
            {
                selectAct = x => x.Id == actId;

            }
            var lastDate = repo.AllReadonly<CaseSessionAct>()
                            .Where(selectAct)
                            .Where(x => x.CaseId == caseLawUnit.CaseId)
                            .Select(x => x.ActDeclaredDate).Max();


            return (lastDate ?? DateTime.Now.AddYears(-50));
        }

        /// <summary>
        /// Извличане на информация за Данни за регистрация на фирма
        /// </summary>
        /// <param name="actId"></param>
        /// <returns></returns>
        public CaseSessionActCompany GetCompanyByActId(int actId)
        {
            return repo.AllReadonly<CaseSessionActCompany>().Where(x => x.CaseSessionActId == actId).FirstOrDefault();
        }

        /// <summary>
        /// Запис на Данни за регистрация на фирма
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) CaseSessionActCompany_SaveData(CaseSessionActCompany model)
        {
            try
            {
                if (model.Id == 0)
                {
                    var exists = repo.AllReadonly<CaseSessionActCompany>()
                                           .Where(x => x.Id != model.Id)
                                           .Where(x => x.CaseSessionActId == model.CaseSessionActId)
                                           .Any();
                    if (exists == true)
                    {
                        return (result: false, errorMessage: "Вече има въведени данни");
                    }
                }

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseSessionActCompany>(model.Id);

                    saved.RegisterDate = model.RegisterDate;
                    saved.RegisterNumber = model.RegisterNumber;
                    saved.Chapter = model.Chapter;
                    saved.PageNumber = model.PageNumber;
                    saved.Batch = model.Batch;
                    saved.Level = model.Level;
                    saved.Authorization = model.Authorization;
                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;
                }
                else
                {
                    //Insert
                    model.UserId = userContext.UserId;
                    model.DateWrt = DateTime.Now;
                    repo.Add<CaseSessionActCompany>(model);
                }

                repo.SaveChanges();
                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CaseSessionActCompany Id={model.Id}");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Извличане на данни за акт
        /// </summary>
        /// <param name="actId"></param>
        /// <returns></returns>
        public CaseSessionAct GetByIdWithOtherData(int actId)
        {
            return repo.AllReadonly<CaseSessionAct>()
                       .Include(x => x.ActType)
                       .Where(x => x.Id == actId)
                       .FirstOrDefault();
        }

        /// <summary>
        /// Извличане на актове от дело за обжалване за комбо
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_CanAppealAct(int caseId)
        {
            List<SelectListItem> result = new List<SelectListItem>();

            //var caseSessionActs = repo.AllReadonly<CaseSessionActComplain>()
            //                          .Where(x => x.CaseId == caseId && x.DateExpired == null)
            //                          .Select(x => x.CaseSessionAct)
            //                          .Include(x => x.ActType)
            //                          .Where(x => x.CaseId == caseId)
            //                          // && (x.CanAppeal == true) - К.Борисов - дори и да не е маркиран акта като подлежащ на обжалване, ако има пуснато обжалване - става
            //                          .Where(x => x.RegDate != null)
            //                          .OrderByDescending(x => x.RegDate)
            //                          .ToList();

            var caseSessionActs = await repo.AllReadonly<CaseSessionActComplain>()
                                     .Where(x => x.CaseId == caseId && x.DateExpired == null)
                                     // && (x.CanAppeal == true) - К.Борисов - дори и да не е маркиран акта като подлежащ на обжалване, ако има пуснато обжалване - става
                                     .Where(x => x.CaseSessionAct.RegDate != null)
                                     .OrderByDescending(x => x.CaseSessionAct.RegDate)
                                     .Select(x => new
                                     {
                                         Id = x.CaseSessionActId,
                                         x.CaseSessionAct.RegDate,
                                         x.CaseSessionAct.RegNumber,
                                         ActTypeLabel = x.CaseSessionAct.ActType.Label
                                     })
                                     .ToListAsync();

            foreach (var caseSessionAct in caseSessionActs)
            {
                if (!result.Any(x => x.Value == caseSessionAct.Id.ToString()))
                {
                    var act = new SelectListItem
                    {
                        Value = caseSessionAct.Id.ToString(),
                        Text = $"{caseSessionAct.ActTypeLabel} {caseSessionAct.RegNumber}/{caseSessionAct.RegDate:dd.MM.yyyy}"
                    };

                    result.Add(act);
                }
            }
            ;

            result = result.Prepend(new SelectListItem() { Value = "-1", Text = "Изберете" }).ToList();
            return result;
        }

        public async Task<List<SelectListItem>> GetDropDownList_CaseSessionActEnforced(int CaseId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = await repo.AllReadonly<CaseSessionAct>()
                                   .Where(FilterExpireInfo<CaseSessionAct>(false))
                                   .Where(x => x.CaseSession.CaseId == CaseId &&
                                               NomenclatureConstants.SessionActState.EnforcedStates.Contains(x.ActStateId))
                                   .OrderByDescending(x => x.Id)
                                   .Select(x => new SelectListItem()
                                   {
                                       Text = $"{x.ActType.Label} {x.RegNumber} ({x.CaseSession.SessionType.Label} {x.CaseSession.DateFrom:dd.MM.yyyy})",
                                       Value = x.Id.ToString()
                                   })
                                   .ToListAsync();

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        public List<CaseSessionActVM> GetSessionActsFinal(int CaseId)
        {
            return repo.AllReadonly<CaseSessionAct>()
                       .Where(x => x.CaseId == CaseId &&
                                   x.DateExpired == null &&
                                   x.ActStateId == NomenclatureConstants.SessionActState.ComingIntoForce)
                       .Select(x => new CaseSessionActVM()
                       {
                           Id = x.Id,
                           CaseSessionId = x.CaseSessionId,
                           CaseId = x.CaseSession.CaseId,
                           CaseSessionLabel = (x.CaseSession != null) ? x.CaseSession.SessionType.Label + "/" + x.CaseSession.DateFrom.ToString("dd.MM.yyyy HH:mm") : string.Empty,
                           CaseLabel = x.Case.RegNumber + "/" + x.Case.RegDate.ToString("dd.MM.yyyy"),
                           ActTypeLabel = (x.ActType != null) ? x.ActType.Label : string.Empty,
                           ActStateLabel = (x.ActState != null) ? x.ActState.Label : string.Empty,
                           RegNumber = x.RegNumber,
                           RegDate = x.RegDate.Value,
                           IsFinalDoc = x.IsFinalDoc,
                           DateWrt = x.DateWrt,
                           EcliCode = x.EcliCode,
                           Description = x.Description
                       })
                       .ToList();
        }

        /// <summary>
        /// извличане на данни за Справка съдебни актове и протоколи
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionActVM> CaseSessionActSpr_Select(int courtId, CaseSessionActFilterVM model, bool forLawUnitCurrent = false)
        {
            DateTime dateNow = DateTime.Now;
            DateTime dateFromSearch = model.DateFrom ?? DateTime.Now.AddYears(-100);
            DateTime dateToSearch = model.DateTo ?? DateTime.Now.AddYears(100);
            DateTime dateAddYear = DateTime.Now.AddYears(100);
            DateTime dateTimeBegin = new DateTime(1900, 1, 1);

            Expression<Func<CaseSessionAct, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate >= dateFromSearch.ForceStartDate() && x.RegDate <= dateToSearch.ForceEndDate();

            Expression<Func<CaseSessionAct, bool>> yearSearch = x => true;
            if ((model.Year ?? 0) > 0)
                yearSearch = x => x.Case.RegDate >= NomenclatureExtensions.GetPastDate() && x.Case.RegDate.Year == model.Year;

            Expression<Func<CaseSessionAct, bool>> caseRegnumberSearch = x => true;
            if (!string.IsNullOrEmpty(model.CaseRegNumber))
                caseRegnumberSearch = x => EF.Functions.ILike(x.Case.RegNumber, model.CaseRegNumber.ToCasePaternSearch());

            Expression<Func<CaseSessionAct, bool>> finalActSearch = x => true;
            if (model.IsFinalDoc == true)
                finalActSearch = x => x.IsFinalDoc == true && NomenclatureConstants.SessionActState.EnforcedStates.Contains(x.ActStateId);

            Expression<Func<CaseSessionAct, bool>> actLawBaseSearch = x => true;
            if (model.ActLawBaseId > 0)
                actLawBaseSearch = x => x.CaseSessionActLawBases.Where(a => a.LawBaseId == model.ActLawBaseId).Any();

            Expression<Func<CaseSessionAct, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseSession.CaseLawUnits.Where(a => (a.DateTo ?? dateAddYear).Date >= x.CaseSession.DateFrom && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            Expression<Func<CaseSessionAct, bool>> regnumberSearch = x => true;
            if (!string.IsNullOrEmpty(model.RegNumber))
                regnumberSearch = x => x.RegNumber == model.RegNumber;

            //Expression<Func<CaseSessionAct, bool>> actTypeSearch = x => true;
            //if (model.ActTypeId > 0)
            //    actTypeSearch = x => x.ActTypeId == model.ActTypeId;

            Expression<Func<CaseSessionAct, bool>> actTypeSearch = x => true;
            if (!string.IsNullOrEmpty(model.ActTypeIds_text))
            {
                var listActTypeIds = new List<int>();
                if (!string.IsNullOrEmpty(model.ActTypeIds_text))
                    listActTypeIds = model.ActTypeIds_text.Split(',').Select(Int32.Parse).ToList();
                actTypeSearch = x => listActTypeIds.Contains(x.ActTypeId);
            }


            Expression<Func<CaseSessionAct, bool>> courtDepartment = x => true;
            if (model.CourtDepartmentId > 0)
                courtDepartment = x => x.CaseSession.CaseLawUnits.Any(a => a.CourtDepartmentId == model.CourtDepartmentId);

            Expression<Func<CaseSessionAct, bool>> caseGroupWhere = x => true;
            if (!string.IsNullOrEmpty(model.CaseGroupIds_text))
            {
                var listGroupIds = new List<int>();
                if (!string.IsNullOrEmpty(model.CaseGroupIds_text))
                    listGroupIds = model.CaseGroupIds_text.Split(',').Select(Int32.Parse).ToList();
                caseGroupWhere = x => listGroupIds.Contains(x.Case.CaseGroupId);
            }

            Expression<Func<CaseSessionAct, bool>> caseTypeWhere = x => true;
            if (!string.IsNullOrEmpty(model.CaseTypeIds_text))
            {
                var listTypeIds = new List<int>();
                if (!string.IsNullOrEmpty(model.CaseTypeIds_text))
                    listTypeIds = model.CaseTypeIds_text.Split(',').Select(Int32.Parse).ToList();
                caseTypeWhere = x => listTypeIds.Contains(x.Case.CaseTypeId);
            }

            Expression<Func<CaseSessionAct, bool>> whereForLawUnitCurrent = x => true;
            if (forLawUnitCurrent)
                whereForLawUnitCurrent = x => x.CaseSession.CaseLawUnits.Any(a => a.LawUnitId == userContext.LawUnitId);

            return repo.AllReadonly<CaseSessionAct>()
                       .Where(x => x.CourtId == courtId)
                       .Where(x => x.DateExpired == null)
                       .Where(dateSearch)
                       .Where(yearSearch)
                       .Where(caseRegnumberSearch)
                       .Where(finalActSearch)
                       .Where(actLawBaseSearch)
                       .Where(judgeReporterSearch)
                       .Where(regnumberSearch)
                       .Where(actTypeSearch)
                       .Where(courtDepartment)
                       .Where(caseGroupWhere)
                       .Where(whereForLawUnitCurrent)
                       .Where(caseTypeWhere)
                       .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                       .Select(x => new CaseSessionActVM()
                       {
                           Id = x.Id,
                           CaseSessionId = x.CaseSessionId,
                           CaseId = x.CaseId ?? 0,
                           CaseSessionDate = x.CaseSession.DateFrom,
                           CaseSessionLabel = x.CaseSession.SessionType.Label + "/" + x.CaseSession.DateFrom.ToString("dd.MM.yyyy HH:mm"),
                           CaseLabel = x.Case.RegNumber,
                           CaseDate = x.Case.RegDate,
                           ActTypeLabel = x.ActType.Label,
                           ActStateLabel = x.ActState.Label + (x.ActCoordination.Any(c => c.CoordinationDeclaredDate != null) ? " (ОМ)" : string.Empty),
                           RegNumber = x.RegNumber,
                           RegDate = x.RegDate.Value,
                           JudgeReport = x.CaseSession.CaseLawUnits.Where(l => l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                                               (l.DateTo ?? dateAddYear) >= x.CaseSession.DateFrom)
                                                                   .Select(l => l.LawUnit.FullName)
                                                                   .FirstOrDefault(),
                           DepartmentOtdelenieText = (x.Case.OtdelenieId != null && x.Case.Otdelenie.DepartmentTypeId != NomenclatureConstants.DepartmentType.Napravlenie ? x.Case.Otdelenie.Label : string.Empty) +
                                                     (x.Case.JudicalCompositionId != null ? (x.Case.OtdelenieId != null && x.Case.Otdelenie.DepartmentTypeId != NomenclatureConstants.DepartmentType.Napravlenie ? " / " + x.Case.JudicalComposition.Label : x.Case.JudicalComposition.Label) : string.Empty)
                       })
                       .AsQueryable();
        }

        public CaseSessionAct GetByRelatedActId(int actId)
        {
            return repo.AllReadonly<CaseSessionAct>()
                             .Where(x => x.RelatedActId == actId)
                             .FirstOrDefault();
        }

        public bool IsExistCaseSessionActByCase(int CaseId)
        {
            return repo.AllReadonly<CaseSessionAct>()
                       .Any(x => x.CaseId == CaseId &&
                                 x.DateExpired == null &&
                                 x.ActStateId != NomenclatureConstants.SessionActState.Project);
        }

        public List<SelectListItem> GetDropDownList_CaseSessionActByCaseBySession(int? CaseId, int? CaseSessionId, bool addDefAllOne = false, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = repo.AllReadonly<CaseSessionAct>()
                             .Include(x => x.CaseSession)
                             .Include(x => x.ActType)
                             .Where(x => (CaseId != null ? x.CaseId == CaseId : true) &&
                                         (CaseSessionId != null ? x.CaseSessionId == CaseSessionId : true) &&
                                         x.DateExpired == null &&
                                         x.ActDeclaredDate != null)
                             .OrderByDescending(x => x.RegDate)
                             .Select(x => new SelectListItem()
                             {
                                 Text = x.ActType.Label + " - " + x.RegNumber + "/" + (x.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"),
                                 Value = x.Id.ToString()
                             })
                             .ToList();

            if (result.Count != 1)
            {
                if (addDefaultElement)
                {
                    result = result
                        .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                        .ToList();
                }

                if (addAllElement)
                {
                    result = result
                        .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                        .ToList();
                }
            }
            else
            {
                if (addDefAllOne)
                {
                    if (addDefaultElement)
                    {
                        result = result
                            .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                            .ToList();
                    }

                    if (addAllElement)
                    {
                        result = result
                            .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                            .ToList();
                    }
                }
            }

            return result;
        }

        public bool RemoveDepersonalizationInfo(int id)
        {
            var model = ReadById<CaseSessionAct>(id);
            model.DepersonalizeUserId = null;
            model.DepersonalizeEndDate = null;
            repo.SaveChanges();
            return true;
        }

        public bool RemoveMotiveDepersonalizationInfo(int id)
        {
            var model = ReadById<CaseSessionAct>(id);
            model.DepersonalizeMotiveUserId = null;
            model.DepersonalizeMotiveEndDate = null;
            repo.SaveChanges();
            return true;
        }


        public List<SelectListItem> GetActDirectionItems()
        {
            return new List<SelectListItem> {
                new SelectListItem("Ищец-Ответник",NomenclatureConstants.ActBlankDirection.LeftToRight.ToString()),
                new SelectListItem("Ответник-Ищец",NomenclatureConstants.ActBlankDirection.RightToLeft.ToString())};
        }

        public async Task<SaveResultVM> FixActDeclaration(int actId)
        {
            var actModel = await GetReadonlyAsync<CaseSessionAct>(actId);
            if (actModel.DateExpired != null)
            {
                return new SaveResultVM(false, $"Актът {actModel.RegNumber} е изтрит");
            }
            if (actModel.ActDeclaredDate != null)
            {
                return new SaveResultVM(true, $"Актът {actModel.RegNumber} е вече постановен на дата: {actModel.ActDeclaredDate}");
            }

            var lastsSendForSignTaskId = await repo.AllReadonly<WorkTask>()
                                                    .Where(x => x.TaskTypeId == WorkTaskConstants.Types.CaseSessionAct_SentToSign
                                                    && x.SourceType == SourceTypeSelectVM.CaseSessionAct
                                                    && x.SourceId == (long)actId
                                                    && x.DateCompleted != null)
                                                    .OrderByDescending(x => x.Id)
                                                    .Select(x => x.Id)
                                                    .FirstOrDefaultAsync();

            if (lastsSendForSignTaskId == 0)
            {
                return new SaveResultVM(false, $"Актът {actModel.RegNumber} няма изпълнена задача за изпращане за подпис");
            }

            var lastSignTasks = await repo.AllReadonly<WorkTask>()
                                    .Where(x => x.TaskTypeId == WorkTaskConstants.Types.CaseSessionAct_Sign
                                    && x.SourceType == SourceTypeSelectVM.CaseSessionAct
                                    && x.SourceId == (long)actId
                                    && x.ParentTaskId == lastsSendForSignTaskId
                                    )
                                    .Select(x => new
                                    {
                                        x.Id,
                                        x.TaskStateId,
                                        x.DateCompleted
                                    }).ToListAsync();

            if (!lastSignTasks.Any())
            {
                return new SaveResultVM(false, $"Актът {actModel.RegNumber} няма задачи за подпис");
            }

            if (lastSignTasks.Any(x => x.DateCompleted == null))
            {
                return new SaveResultVM(false, $"Актът {actModel.RegNumber} има неизпълнена задача за подпис");
            }

            var pdfFile = await repo.AllReadonly<MongoFile>()
                                    .Where(x => x.SourceType == SourceTypeSelectVM.CaseSessionActPdf
                                    && x.SourceId == actId.ToString())
                                    .FirstOrDefaultAsync();

            if (pdfFile == null)
            {
                return new SaveResultVM(false, $"Актът {actModel.RegNumber} няма pdf файл");
            }


            if (pdfFile.SignituresCount != lastSignTasks.Count)
            {
                return new SaveResultVM(false, $"Файлът на акта {actModel.RegNumber} е подписан от {pdfFile.SignituresCount} лица, а задачите са {lastSignTasks.Count}");
            }

            var lastSignedTaskId = lastSignTasks.OrderByDescending(x => x.DateCompleted).Select(x => x.Id).FirstOrDefault();

            return await taskService.UpdateAfterCompleteTask(await GetReadonlyAsync<WorkTask>(lastSignedTaskId));
        }

        /// <summary>
        /// Извличане на запазените секретари от сесии или всички служители за избор
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDLSelect2_SecretaryList(int caseSessionId, int actId = 0)
        {
            if (caseSessionId > 0)
            {
                var sessionSecretaries = repo.AllReadonly<CaseSessionMeetingUser>()
                                          .Where(x => x.CaseSessionMeeting.CaseSessionId == caseSessionId)
                                          .Where(x => x.SecretaryUserId != null)
                                          .OrderBy(x => x.SecretaryUser.LawUnit.FullName)
                                          .Select(x => new SelectListItem
                                          {
                                              Value = x.SecretaryUserId,
                                              Text = x.SecretaryUser.LawUnit.FullName
                                          })
                                          .Distinct()
                                          .ToList();


                List<SelectListItem> result = null;

                if (actId > 0)
                {
                    var savedActLawunits = repo.AllReadonly<CaseSessionActLawunit>()
                                    .Where(x => x.CaseSessionActId == actId && x.JudgeRoleId == NomenclatureConstants.JudgeRole.Secretary)
                                    .Select(x => new SelectListItem
                                    {
                                        Value = x.LawUnitUserId,
                                        Text = x.LawUnit.FullName
                                    })
                                    .ToArray();

                    result = sessionSecretaries.Union(savedActLawunits).DistinctBy(x => x.Value).ToList();
                    if (result.Count == 0)
                    {
                        var secretaryUserFromActId = GetPropById<CaseSessionAct, string>(actId, x => x.SecretaryUserId);
                        if (!string.IsNullOrEmpty(secretaryUserFromActId))
                        {
                            result = repo.AllReadonly<ApplicationUser>()
                                    .Where(x => x.Id == secretaryUserFromActId)
                                    .Select(x => new SelectListItem
                                    {
                                        Value = x.Id,
                                        Text = x.LawUnit.FullName
                                    })
                                    .ToList();
                        }
                    }
                }
                else
                {
                    result = sessionSecretaries;
                }

                if (!sessionSecretaries.Any())
                {
                    result.Add(new SelectListItem("Изберете от служители на съда", "-1"));

                }

                return result;
            }

            var dtNow = DateTime.Now;

            Expression<Func<ApplicationUser, bool>> whereSavedUsers = x =>
             x.LawUnit.Courts.Any(c => c.CourtId == userContext.CourtId
                                        && NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(c.PeriodTypeId)
                                        && (c.DateTo ?? dtNow) >= dtNow && (c.MandateDateTo ?? dtNow) >= dtNow)
               && (x.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.OtherEmployee)
               && (x.LawUnit.DateTo ?? dtNow) >= dtNow
               && x.IsActive;
            if (actId > 0)
            {
                var savedUserIds = repo.AllReadonly<CaseSessionActLawunit>()
                                        .Where(x => x.CaseSessionActId == actId)
                                        .Select(x => x.LawUnitUserId)
                                        .ToArray();

                var actSecretaryUserId = repo.GetPropById<CaseSessionAct, string>(x => x.Id == actId, x => x.SecretaryUserId);
                if (!string.IsNullOrEmpty(actSecretaryUserId))
                {
                    savedUserIds = savedUserIds.Append(actSecretaryUserId).ToArray();
                }
                whereSavedUsers = x => savedUserIds.Contains(x.Id);
            }

            var allUsersInCourt = repo.AllReadonly<ApplicationUser>()
                                        .Where(whereSavedUsers)
                                        .OrderBy(x => x.LawUnit.FullName)
                                        .Select(x => new SelectListItem
                                        {
                                            Value = x.Id,
                                            Text = x.LawUnit.FullName
                                        });

            return allUsersInCourt.ToList();
        }

        public SaveResultVM testtracking()
        {

            var model = repo.AllReadonly<CaseSessionAct>()
                            .Include(x => x.ActCreatorUser)
                        .Where(x => x.Id == 8446)
                        .FirstOrDefault();

            return new SaveResultVM(repo.StopTrackingApplicationUser());
        }

        public async Task<List<CdnItemVM>> SelectPdfFilesBySourceType(int caseId, int? sourceType)
        {
            var result = new List<CdnItemVM>();

            if (sourceType == SourceTypeSelectVM.Document || sourceType == null)
            {
                //Добавят се всички pdf файлове на съпровождащи и иницииращи документи
                var initDocId = await repo.GetPropByIdAsync<Case, long>(x => x.Id == caseId, x => x.DocumentId);


                var docInfos = await repo.AllReadonly<Document>()
                                          .Where(x => x.Id == initDocId)
                                          .Union(repo.AllReadonly<Document>()
                                                .Where(x => x.DocumentCaseInfo.Any(dc => dc.CaseId == caseId))
                                                )
                                          .Union(repo.AllReadonly<Document>()
                                                    .Where(x => x.Id == initDocId)
                                                    .Select(x => x.AssignmentDocument)
                                                )

                                          .Select(x => new DocumentContentElementVM
                                          {
                                              SourceTypes = SourceTypeSelectVM.DocumentAllFiles,
                                              SourceId = x.Id,
                                              DirLabel = x.DocumentDirection.Label,
                                              DocumentTypeLabel = x.DocumentType.Label,
                                              DocumentNumber = x.DocumentNumber,
                                              DocumentDate = x.DocumentDate
                                          }).ToListAsync();

                var initResolutionList = await repo.AllReadonly<DocumentResolution>()
                                                .Where(x => x.DocumentId == initDocId)
                                                .Where(x => x.DeclaredDate != null && x.RegDate != null)
                                                .Select(x => new DocumentContentElementVM
                                                {
                                                    SourceTypes = new[] { SourceTypeSelectVM.DocumentResolutionPdf },
                                                    SourceId = x.Id,
                                                    DirLabel = null,
                                                    DocumentTypeLabel = x.ResolutionType.Label,
                                                    DocumentNumber = x.RegNumber,
                                                    DocumentDate = x.RegDate.Value
                                                }).ToListAsync();

                docInfos.AddRange(initResolutionList);

                foreach (var item in docInfos)
                {
                    var fileObject = $"{item.DocumentTypeLabel} {item.DocumentNumber}/{item.DocumentDate:dd.MM.yyyy}";
                    var docFiles = await repo.AllReadonly<MongoFile>()
                                            .Where(x => item.SourceTypes.Contains(x.SourceType) && x.SourceId == item.SourceId.ToString())
                                            .Where(x => EF.Functions.ILike(x.FileName, "%.pdf"))
                                            .Select(x => new CdnItemVM
                                            {
                                                FileId = x.FileId,
                                                Title = $"{fileObject} : {x.FileName}",
                                                DateUploaded = item.DocumentDate
                                            }).ToArrayAsync();
                    result.AddRange(docFiles);
                }

            }

            if (sourceType == SourceTypeSelectVM.CaseSessionAct || sourceType == null)
            {
                //Добавят се всички pdf файлове на актове                

                var actInfos = await repo.AllReadonly<CaseSessionAct>()
                                          .Where(x => x.CaseId == caseId)
                                          .Where(x => x.RegDate != null)
                                          .Select(x => new
                                          {
                                              x.Id,
                                              ActTypeLabel = x.ActType.Label,
                                              x.RegNumber,
                                              x.RegDate
                                          }).ToListAsync();

                foreach (var item in actInfos)
                {
                    var fileObject = $"{item.ActTypeLabel} {item.RegNumber}/{item.RegDate:dd.MM.yyyy}";
                    var actFiles = await repo.AllReadonly<MongoFile>()
                                            .Where(x => SourceTypeSelectVM.CaseSessionActDocs.Contains(x.SourceType) && x.SourceId == item.Id.ToString())
                                            .Select(x => new CdnItemVM
                                            {
                                                FileId = x.FileId,
                                                Title = x.Title,//$"{fileObject} : {x.FileName}",
                                                DateUploaded = item.RegDate.Value
                                            }).ToArrayAsync();

                    result.AddRange(actFiles);
                }
            }

            return result;
        }

        public async Task<SaveResultVM> CheckBeforeSignRNFLAct(int actId)
        {
            var actInfo = await repo.AllReadonly<CaseSessionAct>()
                                .Where(x => x.Id == actId)
                                .Where(x => x.Case.IspnKind == NomenclatureConstants.IspnKinds.Rnfl)
                                .Where(x => x.ActISPNReasonId > 0)
                                .Select(x => new
                                {
                                    CaseId = x.Case.Id,
                                    x.ActISPNReasonId
                                }).FirstOrDefaultAsync();

            if (actInfo == null)
            {
                return new SaveResultVM(true);
            }

            string startLegalBaseCode = await repo.AllReadonly<CodeMapping>()
                                            .Where(x => x.Alias == RnflConstants.CodeMapping.StartLegalBase
                                                    && x.InnerCode == actInfo.ActISPNReasonId.ToString())
                                            .Select(x => x.OuterCode)
                                            .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(startLegalBaseCode) || (startLegalBaseCode != "rnfl_start"))
            {
                return new SaveResultVM(true);
            }

            var debtorRolesStr = await repo.AllReadonly<CodeMapping>()
                                            .Where(x => x.Alias == RnflConstants.CodeMapping.PersonRoles
                                                    && x.OuterCode == RnflConstants.TargetMethods.Debtor)
                                            .Select(x => x.InnerCode)
                                            .ToArrayAsync();
            try
            {
                int[] debtorRoles = debtorRolesStr.Select(x => int.Parse(x)).ToArray();

                var debtorInfo = await repo.AllReadonly<CasePerson>()
                                      .Where(x => x.CaseId == actInfo.CaseId && x.CaseSessionId == null)
                                      .Where(x => debtorRoles.Contains(x.PersonRoleId))
                                      .Where(x => x.DateExpired == null)
                                      .Select(x => new
                                      {
                                          x.Id,
                                          x.Uic
                                      }).FirstOrDefaultAsync();

                if (debtorInfo == null)
                {
                    return new SaveResultVM(false, "Моля, въведете страна по делото в роля 'Длъжник'!");
                }
                if (string.IsNullOrWhiteSpace(debtorInfo.Uic))
                {
                    return new SaveResultVM(false, "Моля, въведете идентификатор на страната по делото в роля 'Длъжник'!");
                }
            }
            catch (Exception ex)
            {
                return new SaveResultVM(true);
            }
            return new SaveResultVM(true);

        }
    }
}
