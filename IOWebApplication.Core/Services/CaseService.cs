using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Data.Models.Regix;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels.Money;
using IOWebApplication.Infrastructure.Models.ViewModels.RegixReport;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;
using Org.BouncyCastle.Asn1.IsisMtt.X509;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace IOWebApplication.Core.Services
{
    public class CaseService : BaseService, ICaseService
    {
        private readonly ICounterService counterService;
        private readonly IWorkTaskService workTaskService;
        private readonly IUrlHelper urlHelper;
        private readonly ICaseSelectionProtokolService caseSelectionProtokolService;
        private readonly ICaseSessionDocService caseSessionDocService;
        private readonly ICasePersonService casePersonService;
        private readonly ICaseSessionActService caseSessionActService;
        private readonly ICaseNotificationService caseNotificationService;
        private readonly ICaseLawUnitService caseLawUnitService;
        private readonly ICaseSessionService caseSessionService;
        private readonly ICaseSessionMeetingService caseSessionMeetingService;
        private readonly ICaseMovementService caseMovementService;
        private readonly ICaseLifecycleService caseLifecycleService;
        private readonly ICaseEvidenceService caseEvidenceService;
        private readonly ICaseClassificationService caseClassificationService;
        private readonly ICaseMoneyService caseMoneyService;
        private readonly ICaseMigrationService caseMigrationService;
        private readonly ICaseGroupService caseGroupService;
        private readonly ICaseDeadlineService caseDeadlineService;
        private readonly ICdnService cdnService;
        private readonly IMQEpepService mqEpepService;
        private readonly IMoneyService moneyService;
        private readonly ICaseSessionFastDocumentService caseSessionFastDocumentService;
        private readonly IWorkNotificationService workNotificationService;

        public CaseService(ILogger<CaseService> _logger,
                           ICounterService _counterService,
                           IWorkTaskService _workTaskService,
                           IRepository _repo,
                           IUserContext _userContext,
                           ICaseSelectionProtokolService _caseSelectionProtokolService,
                           ICaseSessionDocService _caseSessionDocService,
                           ICasePersonService _casePersonService,
                           ICaseSessionActService _caseSessionActService,
                           ICaseNotificationService _caseNotificationService,
                           ICaseLawUnitService _caseLawUnitService,
                           ICaseSessionService _caseSessionService,
                           ICaseSessionMeetingService _caseSessionMeetingService,
                           ICaseMovementService _caseMovementService,
                           ICaseLifecycleService _caseLifecycleService,
                           ICaseEvidenceService _caseEvidenceService,
                           ICaseClassificationService _caseClassificationService,
                           ICaseMoneyService _caseMoneyService,
                           ICaseMigrationService _caseMigrationService,
                           ICaseGroupService _caseGroupService,
                           ICaseDeadlineService _caseDeadlineService,
                           ICdnService _cdnService,
                           IMQEpepService _mqEpepService,
                           IMoneyService _moneyService,
                           IUrlHelper _url,
                           ICaseSessionFastDocumentService _caseSessionFastDocumentService,
                           IReadonlyRepository _readonlyrepo,
                           IWorkNotificationService _workNotificationService)
        {
            logger = _logger;
            counterService = _counterService;
            workTaskService = _workTaskService;
            repo = _repo;
            userContext = _userContext;
            urlHelper = _url;
            caseSelectionProtokolService = _caseSelectionProtokolService;
            caseSessionDocService = _caseSessionDocService;
            casePersonService = _casePersonService;
            caseSessionActService = _caseSessionActService;
            caseNotificationService = _caseNotificationService;
            caseLawUnitService = _caseLawUnitService;
            caseSessionService = _caseSessionService;
            caseSessionMeetingService = _caseSessionMeetingService;
            caseMovementService = _caseMovementService;
            caseLifecycleService = _caseLifecycleService;
            caseEvidenceService = _caseEvidenceService;
            caseClassificationService = _caseClassificationService;
            caseMoneyService = _caseMoneyService;
            caseMigrationService = _caseMigrationService;
            caseGroupService = _caseGroupService;
            caseDeadlineService = _caseDeadlineService;
            cdnService = _cdnService;
            mqEpepService = _mqEpepService;
            moneyService = _moneyService;
            caseSessionFastDocumentService = _caseSessionFastDocumentService;
            readonlyrepo = _readonlyrepo;
            workNotificationService = _workNotificationService;
        }

        /// <summary>
        /// Извличане на данни за дела
        /// </summary>
        /// <param name="model">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<CaseVM> Case_Select(CaseFilter model)
        {
            DateTime dateNow = DateTime.Now;
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime pastDate = new DateTime(1900, 1, 1);
            model.DateFrom = (model.DateFrom ?? dateNow.AddYears(-100)).ForceStartDate();
            model.DateTo = (model.DateTo ?? dateEnd).ForceEndDate();


            Expression<Func<Case, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate >= model.DateFrom && x.RegDate.Date <= model.DateTo;

            Expression<Func<Case, bool>> yearSearch = x => true;
            if ((model.CaseYear ?? 0) > 0)
                yearSearch = x => x.RegDate >= NomenclatureExtensions.GetPastDate() && x.RegDate.Year == model.CaseYear;

            Expression<Func<Case, bool>> documentSearch = x => true;
            if (!string.IsNullOrEmpty(model.DocumentNumber))
                documentSearch = x => EF.Functions.ILike(x.Document.DocumentNumber, model.DocumentNumber.ToEndingPaternSearch());

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Any(a => a.CaseSessionId == null &&
                                                                   (a.DateTo ?? dateEnd).Date >= dateNow.Date && a.LawUnitId == model.JudgeReporterId &&
                                                                   a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            Expression<Func<Case, bool>> caseClassificationWhere = x => true;
            if (model.CaseClassificationId > 0)
                caseClassificationWhere = x => x.CaseClassifications.Any(a => a.ClassificationId == model.CaseClassificationId &&
                                                                              a.DateTo == null &&
                                                                              a.CaseSessionId == null);

            Expression<Func<Case, bool>> caseDepartmentWhere = x => true;
            if (model.CourtDepartmentId > 0)
                caseDepartmentWhere = x => x.JudicalCompositionId == model.CourtDepartmentId;

            Expression<Func<Case, bool>> caseDepartmentOtdelenieWhere = x => true;
            if (model.CourtDepartmentOtdelenieId > 0)
                caseDepartmentOtdelenieWhere = x => x.OtdelenieId == model.CourtDepartmentOtdelenieId;

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (!string.IsNullOrEmpty(model.CaseGroupIds_text))
            {
                var listGroupIds = new List<int>();
                if (!string.IsNullOrEmpty(model.CaseGroupIds_text))
                    listGroupIds = model.CaseGroupIds_text.Split(',').Select(Int32.Parse).ToList();
                caseGroupWhere = x => listGroupIds.Contains(x.CaseGroupId);
            }

            Expression<Func<Case, bool>> caseTypeWhere = x => true;
            if (!string.IsNullOrEmpty(model.CaseTypeIds_text))
            {
                var listTypeIds = new List<int>();
                if (!string.IsNullOrEmpty(model.CaseTypeIds_text))
                    listTypeIds = model.CaseTypeIds_text.Split(',').Select(Int32.Parse).ToList();
                caseTypeWhere = x => listTypeIds.Contains(x.CaseTypeId);
            }

            Expression<Func<Case, bool>> caseStateIdWhere = x => true;
            if (model.CaseStateId > 0)
                caseStateIdWhere = x => x.CaseStateId == model.CaseStateId;

            Expression<Func<Case, bool>> caseCodeIdsWhere = x => true;
            if (model.CaseCodeIds != null && model.CaseCodeIds.Any())
            {
                int[] caseCodeIds = model.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.CaseCodeId ?? 0);
            }

            Expression<Func<Case, bool>> regNumberWhere = x => true;
            if (!string.IsNullOrEmpty(model.RegNumber))
                regNumberWhere = x => EF.Functions.ILike(x.RegNumber, model.RegNumber.ToCasePaternSearch());

            return repo.AllReadonly<Case>()
                       .Where(dateSearch)
                       .Where(yearSearch)
                       .Where(documentSearch)
                       .Where(judgeReporterSearch)
                       .Where(caseClassificationWhere)
                       .Where(caseDepartmentWhere)
                       .Where(caseDepartmentOtdelenieWhere)
                       .Where(caseGroupWhere)
                       .Where(caseTypeWhere)
                       .Where(caseStateIdWhere)
                       .Where(caseCodeIdsWhere)
                       .Where(regNumberWhere)
                       .Where(x => x.CourtId == userContext.CourtId &&
                                   !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null) &&
                                   x.CaseStateId != NomenclatureConstants.CaseState.Draft)
                       .Select(x => new CaseVM()
                       {
                           Id = x.Id,
                           CourtId = x.CourtId,
                           CourtLabel = x.Court.Label,
                           CaseGroupId = x.CaseGroupId,
                           CaseGroupLabel = (x.CaseGroup != null) ? x.CaseGroup.Label : string.Empty,
                           CaseTypeId = x.CaseTypeId,
                           CaseTypeLabel = (x.CaseType != null) ? x.CaseType.Code : string.Empty,
                           CaseCodeId = x.CaseCodeId,
                           CaseCodeLabel = (x.CaseCode != null) ? x.CaseCode.Code + " " + x.CaseCode.Label : string.Empty,
                           ProcessPriorityId = x.ProcessPriorityId,
                           ProcessPriorityLabel = (x.ProcessPriority != null) ? x.ProcessPriority.Label : string.Empty,
                           CaseStateId = x.CaseStateId,
                           CaseStateLabel = (x.CaseState != null) ? x.CaseState.Label : string.Empty,
                           EISSPNumber = x.EISSPNumber,
                           ShortNumber = Convert.ToString(int.Parse(x.ShortNumber)),
                           RegNumber = x.RegNumber,
                           RegDate = x.RegDate,
                           CaseReasonLabel = (x.CaseReason != null) ? x.CaseReason.Label : string.Empty,
                           CaseStateDescription = x.CaseStateDescription,
                           JudgeReport = x.CaseLawUnits.Where(l => l.CaseSessionId == null && l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                                   (l.DateTo ?? DateTime.Now.AddYears(100)) >= dateNow)
                                                       .Select(l => l.LawUnit.FullName)
                                                       .FirstOrDefault(),
                           DepartmentOtdelenieText = (x.Otdelenie != null && x.Otdelenie.DepartmentTypeId != NomenclatureConstants.DepartmentType.Napravlenie ? x.Otdelenie.Label : string.Empty) +
                                                      (x.JudicalComposition != null ? (x.Otdelenie != null && x.Otdelenie.DepartmentTypeId != NomenclatureConstants.DepartmentType.Napravlenie ? " / " + x.JudicalComposition.Label : x.JudicalComposition.Label) : string.Empty)

                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Данни за дела за избор
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseVM> Case_SelectForSelection(int courtId, CaseFilter model)
        {
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<Case, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate.Date >= dateFromSearch.Date && x.RegDate.Date <= dateToSearch.Date;

            //Expression<Func<Case, bool>> caseGroupWhere = x => true;
            //if (model.CaseGroupId > 0)
            //    caseGroupWhere = x => x.CaseGroupId == model.CaseGroupId;

            //Expression<Func<Case, bool>> caseTypeWhere = x => true;
            //if (model.CaseTypeId > 0)
            //    caseTypeWhere = x => x.CaseTypeId == model.CaseTypeId;

            Expression<Func<Case, bool>> caseRegnumberSearch = x => true;
            if (!string.IsNullOrEmpty(model.RegNumber))
                caseRegnumberSearch = x => EF.Functions.ILike(x.RegNumber, model.RegNumber.ToCasePaternSearch());

            Expression<Func<Case, bool>> yearWhere = x => true;
            if ((model.CaseYear ?? 0) > 0)
                yearWhere = x => x.RegDate >= NomenclatureExtensions.GetPastDate() && x.RegDate.Year == model.CaseYear;

            Expression<Func<Case, bool>> documentSearch = x => true;
            if (!string.IsNullOrEmpty(model.DocumentNumber))
                documentSearch = x => EF.Functions.ILike(x.Document.DocumentNumber, model.DocumentNumber.ToEndingPaternSearch());

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (!string.IsNullOrEmpty(model.CaseGroupIds_text))
            {
                var listGroupIds = new List<int>();
                if (!string.IsNullOrEmpty(model.CaseGroupIds_text))
                    listGroupIds = model.CaseGroupIds_text.Split(',').Select(Int32.Parse).ToList();
                caseGroupWhere = x => listGroupIds.Contains(x.CaseGroupId);
            }

            Expression<Func<Case, bool>> caseTypeWhere = x => true;
            if (!string.IsNullOrEmpty(model.CaseTypeIds_text))
            {
                var listTypeIds = new List<int>();
                if (!string.IsNullOrEmpty(model.CaseTypeIds_text))
                    listTypeIds = model.CaseTypeIds_text.Split(',').Select(Int32.Parse).ToList();
                caseTypeWhere = x => listTypeIds.Contains(x.CaseTypeId);
            }

            var date_now = DateTime.Now;

            return repo.AllReadonly<Case>()
                       .Where(x => x.CourtId == courtId)
                       .Where(x => !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                       .Where(x => x.CaseStateId != NomenclatureConstants.CaseState.Draft)
                       .Where(dateSearch)
                       .Where(caseGroupWhere)
                       .Where(caseTypeWhere)
                       .Where(caseRegnumberSearch)
                       .Where(yearWhere)
                       .Where(documentSearch)
                       .Where(x => x.CaseLawUnits.Where(l => l.CaseSessionId == null && l.DateFrom < date_now && (l.DateTo ?? date_now) >= date_now).Count() < x.CaseLawUnitCount.Select(c => c.PersonCount).Sum())
                       .Select(x => new CaseVM()
                       {
                           Id = x.Id,
                           CourtId = x.CourtId,
                           CourtLabel = x.Court.Label,
                           CaseGroupId = x.CaseGroupId,
                           CaseGroupLabel = x.CaseGroup.Label,
                           CaseTypeId = x.CaseTypeId,
                           CaseTypeLabel = x.CaseType.Code,
                           CaseCodeId = x.CaseCodeId,
                           CaseCodeLabel = (x.CaseCodeId != null) ? x.CaseCode.Code + " " + x.CaseCode.Label : string.Empty,
                           ProcessPriorityId = x.ProcessPriorityId,
                           ProcessPriorityLabel = (x.ProcessPriorityId != null) ? x.ProcessPriority.Label : string.Empty,
                           CaseStateId = x.CaseStateId,
                           CaseStateLabel = x.CaseState.Label,
                           EISSPNumber = x.EISSPNumber,
                           ShortNumber = x.ShortNumber,
                           RegNumber = x.RegNumber,
                           RegDate = x.RegDate,
                           CaseReasonLabel = (x.CaseReasonId != null) ? x.CaseReason.Label : string.Empty,
                           CaseStateDescription = x.CaseStateDescription
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Попълване на данни за нов интервал
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="lifecycleTypeId"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        private CaseLifecycle FillCaseLifecycle(int caseId, int courtId, int lifecycleTypeId, DateTime dateTime)
        {
            return new CaseLifecycle()
            {
                CaseId = caseId,
                CourtId = courtId,
                LifecycleTypeId = lifecycleTypeId,
                Iteration = 1,
                DateFrom = dateTime,
                DurationMonths = 0,
                Description = string.Empty,
                UserId = userContext.UserId,
                DateWrt = DateTime.Now
            };
        }

        /// <summary>
        /// Метод проверчващ по дело дали има интервал
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<bool> ExistsCaseLifecycle(int caseId)
        {
            return await repo.AllReadonly<CaseLifecycle>()
                             .AnyAsync(x => x.CaseId == caseId &&
                                            x.Iteration == 1 &&
                                            x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress);
        }

        /// <summary>
        /// Запис на интервал
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <param name="dateTime">Дата за начало на интервала</param>
        /// <returns></returns>
        private async Task SaveCaseLifecycle(int caseId, int courtId, DateTime dateTime)
        {
            bool isExistsLifecycle = await ExistsCaseLifecycle(caseId);

            if (isExistsLifecycle)
                return;

            CaseLifecycle caseLifecycle = FillCaseLifecycle(caseId, courtId, NomenclatureConstants.LifecycleType.InProgress, dateTime);
            await repo.AddAsync(caseLifecycle);
        }

        /// <summary>
        /// Запис на дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<SaveResultVM> Case_SaveData(CaseEditVM model)
        {
            //Тука е само редакция защото делото е записано
            try
            {
                model.ProcessPriorityId = model.ProcessPriorityId.NumberEmptyToNull();
                model.LoadGroupLinkId = model.LoadGroupLinkId.EmptyToNull();
                model.ComplexIndexActual = model.ComplexIndexActual.NumberEmptyToNull();
                model.ComplexIndexLegal = model.ComplexIndexLegal.NumberEmptyToNull();
                model.CaseReasonId = model.CaseReasonId.NumberEmptyToNull();
                model.IspnCaseCompetenceId = model.IspnCaseCompetenceId.NumberEmptyToNull();

                var hasCaseCompetence = await this.CheckCaseFeature(new CaseFeatureInfoVM()
                {
                    CourtTypeId = model.CourtTypeId,
                    CaseTypeId = model.CaseTypeId,
                    CaseCodeId = model.CaseCodeId
                }, NomenclatureConstants.CaseFeatures.ISPN_HasCaseCompetence);

                if (!hasCaseCompetence)
                {
                    model.IspnCaseCompetenceId = null;
                }
                if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.FearProtectsVineyard)
                    && !model.DisableTransaction)
                {
                    try
                    {
                        ClearEntityTracker();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "ClearEntityTrackerError.CaseNotification_SaveData");
                    }
                }
                if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.ClearTrackedUsers)
                    && !model.DisableTransaction)
                {
                    try
                    {
                        repo.StopTrackingApplicationUser();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "StopTrackingApplicationUser.CaseNotification_SaveData");
                    }
                }
                var crValidation = await validateCaseCR(model);
                if (!crValidation.Result)
                {
                    return crValidation;
                }

                //Ако не е изключена проверката за версия на реда за Дело и не е подадено от Quartz задачата за Централно Разпределяне
                if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.DisableRowVersion) && !model.DisableTransaction)
                {
                    //Проверка на версия на ред
                    var caseVersion = await ReadByIdAsync<Case>(model.Id);
                    if (caseVersion.RowVersion > model.RowVersion)
                    {
                        return new SaveResultVM()
                        {
                            Result = false,
                            ReloadNeeded = true
                        };
                    }
                    else
                    {
                        caseVersion.RowVersion++;
                        await repo.SaveChangesAsync();
                        model.RowVersion = caseVersion.RowVersion;
                    }
                }

                using (var ts = repo.BeginTransaction(model.DisableTransaction))
                {
                    var caseCurrent = await ReadByIdAsync<Case>(model.Id);
                    if (caseCurrent.DateWrt > model.DateWrt.AddSeconds(1))
                    {
                        return new SaveResultVM()
                        {
                            Result = false,
                            ReloadNeeded = true
                        };
                    }
                    if (string.IsNullOrEmpty(model.RegNumber))

                        if (!string.IsNullOrEmpty(caseCurrent.RegNumber))
                        {
                            return new SaveResultVM()
                            {
                                Result = false,
                                ReloadNeeded = true
                            };
                        }

                    if (string.IsNullOrEmpty(caseCurrent.RegNumber))
                    {
                        //Проверка в историческите данни за издаден номер
                        var lastHystoryWithNumber = await repo.AllReadonly<CaseH>()
                                                        .Where(x => x.Id == model.Id && x.RegNumber != null && x.ShortNumberValue > 0)
                                                        .OrderByDescending(x => x.HistoryId)
                                                        .FirstOrDefaultAsync();

                        if (lastHystoryWithNumber != null && lastHystoryWithNumber.RegDate.Year > 2000)
                        {
                            caseCurrent.RegNumber = lastHystoryWithNumber.RegNumber;
                            caseCurrent.ShortNumber = lastHystoryWithNumber.ShortNumber;
                            caseCurrent.ShortNumberValue = lastHystoryWithNumber.ShortNumberValue;
                            caseCurrent.RegDate = lastHystoryWithNumber.RegDate;
                            await repo.SaveChangesAsync();
                            ts.Commit();
                            return new SaveResultVM()
                            {
                                Result = false,
                                ReloadNeeded = true
                            };
                        }
                    }

                    caseCurrent.CaseCharacterId = model.CaseCharacterId;
                    caseCurrent.CaseTypeId = model.CaseTypeId;
                    caseCurrent.CaseCodeId = model.CaseCodeId;
                    if (model.CaseCodeId == 0)
                    {
                        caseCurrent.CaseCodeId = null;
                    }
                    caseCurrent.CaseGroupId = await repo.GetPropByIdAsync<CaseType, int>(x => x.Id == model.CaseTypeId, x => x.CaseGroupId);
                    caseCurrent.CourtGroupId = model.CourtGroupId;
                    caseCurrent.CaseReasonId = model.CaseReasonId.EmptyToNull();
                    caseCurrent.CaseTypeUnitId = model.CaseTypeUnitId;
                    caseCurrent.Description = model.Description;
                    caseCurrent.CaseStateDescription = model.CaseStateDescription;
                    caseCurrent.CaseInforcedDate = model.CaseInforcedDate;
                    caseCurrent.ProcessPriorityId = model.ProcessPriorityId;
                    caseCurrent.IsNewCaseNewNumber = model.IsNewCaseNewNumber;
                    caseCurrent.IsRenewCase = model.IsRenewCase;
                    caseCurrent.ComplexIndexActual = model.ComplexIndexActual;
                    caseCurrent.ComplexIndexLegal = model.ComplexIndexLegal;
                    caseCurrent.IspnCaseCompetenceId = model.IspnCaseCompetenceId;
                    caseCurrent.DebtorsCount = model.DebtorsCount;
                    caseCurrent.RnflProcessTypeId = model.RnflProcessTypeId;
                    if (userContext.IsUserInRole(AccountConstants.Roles.Supervisor))
                    {
                        //Супервайзорите могат да променят EISPP номера на делото
                        caseCurrent.EISSPNumber = model.EISSPNumber.EmptyToNull();
                        if (!string.IsNullOrEmpty(caseCurrent.EISSPNumber))
                        {
                            caseCurrent.EISSPNumber = caseCurrent.EISSPNumber.ToUpper();
                        }
                    }

                    if (model.CourtTypeId == NomenclatureConstants.CourtType.VKS)
                    {
                        var caseCodeLoadIndex = await repo.AllReadonly<Infrastructure.Data.Models.Nomenclatures.CaseCode>().Where(x => x.Id == model.CaseCodeId)
                                                       .Select(x => x.LoadIndex).FirstOrValueAsync(0);
                        caseCurrent.LoadGroupLinkId = null;
                        caseCurrent.ComplexIndex = model.ComplexIndex;
                        caseCurrent.LoadIndex = (caseCodeLoadIndex + caseCurrent.ComplexIndex) / 2;
                    }
                    else
                    {
                        caseCurrent.LoadGroupLinkId = model.LoadGroupLinkId;
                        caseCurrent.LoadIndex = await repo.AllReadonly<LoadGroupLink>().Where(x => x.Id == caseCurrent.LoadGroupLinkId)
                                                    .Select(x => x.LoadIndex).FirstOrValueAsync(0);
                    }

                    caseCurrent.DateWrt = DateTime.Now;
                    caseCurrent.UserId = userContext.UserId;
                    bool isNewCase = false;
                    CaseMigration newCaseMigration = null;

                    if (string.IsNullOrEmpty(caseCurrent.RegNumber) && caseCurrent.CaseStateId == NomenclatureConstants.CaseState.Draft && model.CaseStateId == NomenclatureConstants.CaseState.New)
                    {
                        caseCurrent.CaseStateId = model.CaseStateId;
                        caseCurrent.IsOldNumber = model.IsOldNumber;
                        //запис на лицата и адреси от Ducument
                        var casePersonList = await repo.AllReadonly<DocumentPerson>().Include(x => x.Addresses).ThenInclude(x => x.Address)
                                                 .Where(x => x.DocumentId == caseCurrent.DocumentId)
                                                 .OrderBy(x => x.Id)
                                                 .ToListAsync();

                        int rownumber = 1;
                        foreach (var item in casePersonList)
                        {
                            var casePersonSave = new CasePerson()
                            {
                                CaseId = caseCurrent.Id,
                                CourtId = caseCurrent.CourtId,
                                PersonId = item.PersonId,
                                PersonRoleId = item.PersonRoleId,
                                PersonMaturityId = item.PersonMaturityId,
                                MilitaryRangId = item.MilitaryRangId,
                                IsDeceased = item.IsDeceased,
                                DateDeceased = item.DateDeceased,
                                IsInitialPerson = true, //тука всички дошли от документа са иницираща страна
                                DateFrom = DateTime.Now,
                                RowNumber = rownumber,
                                UserId = userContext.UserId,
                                DateWrt = DateTime.Now,
                                PersonGid = item.PersonGid,
                                RepresentsGid = item.RepresentsGid
                            };

                            rownumber++;
                            casePersonSave.CopyFrom(item);
                            casePersonSave.CasePersonIdentificator = Guid.NewGuid().ToString().ToLower();

                            foreach (var itemAddress in item.Addresses)
                            {
                                var addressSave = new CasePersonAddress
                                {
                                    Address = new Infrastructure.Data.Models.Common.Address()
                                };
                                addressSave.Address.CopyFrom(itemAddress.Address);
                                addressSave.Address.FullAddress = itemAddress.Address.FullAddress;
                                addressSave.UserId = userContext.UserId;
                                addressSave.DateWrt = DateTime.Now;
                                addressSave.CasePersonAddressIdentificator = Guid.NewGuid().ToString().ToLower();
                                addressSave.CaseId = caseCurrent.Id;
                                addressSave.CourtId = caseCurrent.CourtId;

                                //await CreateHistoryAsync<CasePersonAddress, CasePersonAddressH>(addressSave);
                                casePersonSave.Addresses.Add(addressSave);
                            }

                            //await CreateHistoryAsync<CasePerson, CasePersonH>(casePersonSave);
                            caseCurrent.CasePersons.Add(casePersonSave);
                        }
                        int? oldNumber = null;
                        if (caseCurrent.IsOldNumber == true && !string.IsNullOrEmpty(model.OldNumber))
                        {
                            oldNumber = int.Parse(model.OldNumber);
                        }
                        if (counterService.Counter_GetCaseCounter(caseCurrent, oldNumber, model.OldDate) == false)
                        {
                            return new SaveResultVM(false);
                        }
                        foreach (var person in caseCurrent.CasePersons)
                        {
                            person.DateFrom = caseCurrent.RegDate.AddSeconds(1);
                        }
                        isNewCase = true;

                        // Запис на интервал
                        await SaveCaseLifecycle(caseCurrent.Id, caseCurrent.CourtId, caseCurrent.RegDate);

                        // Ако делото е несъстоятелност
                        bool isInsolvency = await repo.AllReadonly<CaseCodeGrouping>()
                            .Where(x => x.CaseCodeGroup == NomenclatureConstants.CaseCodeGroupings.Insolvency)
                            .Where(x => x.CaseCodeId == model.CaseCodeId)
                            .AnyAsync();

                        caseCurrent.IsISPNcase = isInsolvency;

                        // Ако делото е несъстоятелност - РНФЛ
                        bool isInsolvencyRNFL = await repo.AllReadonly<CaseCodeGrouping>()
                            .Where(x => x.CaseCodeGroup == NomenclatureConstants.CaseCodeGroupings.InsolvencyRNFL)
                            .Where(x => x.CaseCodeId == model.CaseCodeId)
                            .AnyAsync();
                        if (isInsolvencyRNFL)
                        {
                            caseCurrent.IspnKind = NomenclatureConstants.IspnKinds.Rnfl;
                            if (caseCurrent.RnflProcessTypeId == null)
                                caseCurrent.RnflProcessTypeId = NomenclatureConstants.RnflProcessTypes.Main;
                        }

                        newCaseMigration = await Case_AddInPriorMigration(caseCurrent, model.RandomAssignmentOutCaseMigrationId);
                        await Case_SaveData_FinishTask(caseCurrent.DocumentId);
                        setSpecialAccessByGroups(caseCurrent);

                        string documentRequestType = await repo.AllReadonly<Infrastructure.Data.Models.Documents.Document>()
                                                            .Where(x => x.Id == caseCurrent.DocumentId)
                                                            .Select(x => (x.DocumentRequestTypeId > 0) ? x.DocumentRequestType.RequestCode : (string)null)
                                                            .FirstOrDefaultAsync();

                        if (!string.IsNullOrEmpty(documentRequestType))
                        {
                            if (DocumentConstants.ElectronicDocumentRequestTypes.FastProcess.Contains(documentRequestType))
                            {
                                caseCurrent.IsFastProcess = true;
                            }
                        }
                    }
                    else
                    {
                        caseCurrent.CaseStateId = model.CaseStateId;
                    }
                    caseCurrent.UserId = userContext.UserId;
                    caseCurrent.DateWrt = DateTime.Now;
                    await CreateHistoryAsync<Case, CaseH>(caseCurrent);
                    Case_GenerateCaseCount(model);

                    caseDeadlineService.DeadLineOnCase(caseCurrent);
                    await repo.SaveChangesAsync();

                    if (NomenclatureConstants.CaseState.UnregisteredManageble.Contains(model.CaseStateId))
                    {
                        newCaseMigration = await Case_AddInPriorMigration(caseCurrent);
                        await Case_SaveData_FinishTask(caseCurrent.DocumentId);
                    }

                    if (!string.IsNullOrEmpty(caseCurrent.RegNumber))
                    {
                        await mqEpepService.AppendCase(caseCurrent, isNewCase ? EpepConstants.ServiceMethod.Add : EpepConstants.ServiceMethod.Update);
                        if (newCaseMigration != null)
                        {
                            mqEpepService.AppendCaseMigration(newCaseMigration);
                        }
                    }

                    await mqEpepService.EissProcessStart(NomenclatureConstants.EissProcessTypes.CaseSave, SourceTypeSelectVM.Case, caseCurrent.Id);

                    /* - Тези обработки са преместени в EissProcessService.ProcessCaseSave
                   
                   await workNotificationService.SaveNotificationsForNewCaseHigherInstanceWith0604_1_2FastProcess(caseCurrent.Id);
                   await workNotificationService.SaveNotificationsForNewCaseHigherInstanceWithout0604_1_2FastProcess(caseCurrent.Id);
                   await workNotificationService.TurnOffNotificationsForFilingClaimFastProcess(caseCurrent.Id);
                   await workNotificationService.TurnOffNotificationsForLackSubmittedObjectionFastProcess(caseCurrent.Id);
                   */

                    ts.Commit();
                    model.CaseStateId = caseCurrent.CaseStateId;
                    model.RegNumber = caseCurrent.RegNumber;
                }

                return new SaveResultVM(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Case Id={model.Id}");
                return new SaveResultVM(false, "", "counterCheck");
            }
        }

        async Task<SaveResultVM> validateCaseCR(CaseEditVM caseModel)
        {
            var caseInfo = await repo.AllReadonly<Case>()
                                    .Where(x => x.Id == caseModel.Id)
                                    .Select(x => new
                                    {
                                        IsFastProcess = x.IsFastProcess ?? false,
                                        x.RegDate,
                                        x.RegNumber
                                    }).FirstOrDefaultAsync();
            if (!caseInfo.IsFastProcess)
            {
                return new SaveResultVM(true);
            }
            DateTime? zpStartDate = await GetParamValueDate(NomenclatureConstants.SystemParamName.ZP_StartRegDate, "01.07.2025");
            if (caseInfo.RegDate < zpStartDate)
            {
                return new SaveResultVM(true);
            }

            if (((caseInfo.RegNumber != null) ? caseInfo.RegDate : DateTime.Now) > zpStartDate)
            {
                if (!await repo.AllReadonly<DocumentRequestType>()
                                .Where(x => x.CaseCodeId == caseModel.CaseCodeId)
                                .AnyAsync())
                {
                    return new SaveResultVM(false, "Непозвозволен шифър на дело по Заповедно производство");
                }
            }
            return new SaveResultVM(true);
        }

        void setSpecialAccessByGroups(Case model)
        {
            var isSpecialAccess = repo.AllReadonly<CaseClassification>()
                                        .Where(x => x.CaseId == model.Id && x.ClassificationId == NomenclatureConstants.CaseClassifications.SpecialAccess)
                                        .Any();

            var isRestrictedAccess = repo.AllReadonly<CaseClassification>()
                                        .Where(x => x.CaseId == model.Id && x.ClassificationId == NomenclatureConstants.CaseClassifications.Restriction)
                                        .Any();
            if (isSpecialAccess)
            {
                return;
            }

            var hasSpecialAccessGroup = repo.AllReadonly<CourtGroup>()
                                            .Where(x => x.CourtId == model.CourtId)
                                            .Where(x => x.DateTo == null)
                                            .Where(x => x.GroupKind == NomenclatureConstants.CourtGroupKinds.SpecialAccess)
                                            .Where(x => x.CourtGroupCodes.Any(g => g.CaseCodeId == model.CaseCodeId && g.DateTo == null))
                                            .Any();

            if (!hasSpecialAccessGroup)
            {
                return;
            }

            var specialAccess = new CaseClassification()
            {
                CaseId = model.Id,
                CaseSessionId = null,
                ClassificationId = NomenclatureConstants.CaseClassifications.SpecialAccess,
                DateFrom = model.RegDate
            };

            repo.Add(specialAccess);
            if (!isRestrictedAccess)
            {
                var restrictedAccess = new CaseClassification()
                {
                    CaseId = model.Id,
                    CaseSessionId = null,
                    ClassificationId = NomenclatureConstants.CaseClassifications.Restriction,
                    DateFrom = model.RegDate
                };
                repo.Add(restrictedAccess);
            }
            repo.SaveChanges();
        }

        /// <summary>
        /// Генерира запис за дело за нужният брой от хора в състава
        /// </summary>
        /// <param name="model"></param>
        private void Case_GenerateCaseCount(CaseEditVM model)
        {
            repo.DeleteRange<CaseLawUnitCount>(x => x.CaseId == model.Id);

            var unitCounts = caseGroupService.GetById_CaseTypeUnit(model.CaseTypeUnitId ?? 0);

            List<CaseLawUnitCount> caseLawUnitCount = new List<CaseLawUnitCount>();
            if (unitCounts != null)
            {
                var counts = unitCounts.CaseTypeUnitCounts.Where(x => x.Value > 0);
                caseLawUnitCount.AddRange(
                    counts
                    .Where(x => !NomenclatureConstants.JudgeRole.ReserveRolesList.Contains(x.Id))
                    .Select(x => new CaseLawUnitCount()
                    {
                        JudgeRoleId = x.Id,
                        PersonCount = x.Value
                    }));

                switch (model.CaseTypeUnitReserves)
                {
                    case NomenclatureConstants.JudgeRole.ReserveJudgeAndJury:
                        caseLawUnitCount.AddRange(
                        counts
                        .Where(x => NomenclatureConstants.JudgeRole.ReserveRolesList.Contains(x.Id))
                        .Select(x => new CaseLawUnitCount()
                        {
                            JudgeRoleId = x.Id,
                            PersonCount = x.Value
                        }));
                        break;
                    case NomenclatureConstants.JudgeRole.ReserveJudge:
                    case NomenclatureConstants.JudgeRole.ReserveJury:
                        caseLawUnitCount.AddRange(
                           counts
                           .Where(x => model.CaseTypeUnitReserves == x.Id)
                           .Select(x => new CaseLawUnitCount()
                           {
                               JudgeRoleId = x.Id,
                               PersonCount = x.Value
                           }));
                        break;
                }
            }
            foreach (var item in caseLawUnitCount)
            {
                item.CourtId = model.CourtId;
                item.CaseId = model.Id;
                item.UserId = userContext.UserId;
                item.DateWrt = DateTime.Now;
            }

            repo.AddRange<CaseLawUnitCount>(caseLawUnitCount);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        private async Task<CaseMigration> Case_AddInPriorMigration(Case model, int? randomAssignmentOutMigrationId = null)
        {
            CaseMigrationVM lastMigration;

            lastMigration = await Case_GetPriorCase(model.DocumentId);
            if (lastMigration == null)
            {
                lastMigration = Case_GetPriorCaseEISPP(model.DocumentId, model.EISSPNumber);
            }
            if (randomAssignmentOutMigrationId > 0)
            {
                lastMigration = await repo.AllReadonly<CaseMigration>()
                                            .Where(x => x.Id == randomAssignmentOutMigrationId.Value)
                                            .Select(x => new CaseMigrationVM
                                            {
                                                Id = x.Id,
                                                CaseId = x.CaseId,
                                                InitialCaseId = x.InitialCaseId,
                                                MigrationTypeId = x.CaseMigrationTypeId,

                                            }).FirstOrDefaultAsync();
            }
            if (lastMigration == null)
            {
                return null;
            }

            var incommingMigrationTypeId = repo.AllReadonly<CaseMigrationType>()
                                            .Where(x => x.PriorMigrationTypeId == lastMigration.MigrationTypeId)
                                            .Where(x => x.IsActive)
                                            .Select(x => x.Id)
                                            .FirstOrDefault();
            if (incommingMigrationTypeId == 0)
            {
                return null;
            }
            var newMigration = new CaseMigration()
            {
                CaseId = model.Id,
                CourtId = model.CourtId,
                PriorCaseId = lastMigration.CaseId,
                InitialCaseId = lastMigration.InitialCaseId,
                CaseMigrationTypeId = incommingMigrationTypeId,
                SendToTypeId = NomenclatureConstants.CaseMigrationSendTo.Court,
                SendToCourtId = model.CourtId,
                Description = model.Description,
                DateWrt = DateTime.Now,
                UserId = userContext.UserId,
                OutCaseMigrationId = lastMigration.Id
            };
            repo.Add(newMigration);
            return newMigration;
        }

        /// <summary>
        /// Запис на приключена задача
        /// </summary>
        /// <param name="documentId"></param>
        private async Task Case_SaveData_FinishTask(long documentId)
        {

            int[] docToCaseTasks = { WorkTaskConstants.Types.Case_SelectLawUnit, WorkTaskConstants.Types.Case_ForReject };
            var docTasks = await repo.AllReadonly<WorkTask>(
                x => x.SourceId == documentId
                && x.SourceType == SourceTypeSelectVM.Document
                && x.UserId == userContext.UserId
                && docToCaseTasks.Contains(x.TaskTypeId)
                && x.TaskStateId == WorkTaskConstants.States.Accepted)
                .ToListAsync();

            var docResolutions = await repo.AllReadonly<DocumentResolution>()
                                        .Where(x => x.DocumentId == documentId)
                                        .Select(x => x.Id).ToArrayAsync();

            var resolutionTasks = await repo.AllReadonly<WorkTask>(
               x => docResolutions.Contains(x.SourceId)
               && x.SourceType == SourceTypeSelectVM.DocumentResolution
               && x.UserId == userContext.UserId
               && x.TaskTypeId == WorkTaskConstants.Types.Case_SelectLawUnit
               && x.TaskStateId == WorkTaskConstants.States.Accepted)
                .ToListAsync();

            foreach (var item in docTasks)
            {
                await workTaskService.CompleteTask(item);
            }
            foreach (var item in resolutionTasks)
            {
                await workTaskService.CompleteTask(item);
            }
        }

        public bool TestHistory(int id)
        {
            var model = repo.GetById<Case>(id);
            model.UserId = userContext.UserId;
            model.DateWrt = DateTime.Now;
            CreateHistory<Case, CaseH>(model);
            repo.SaveChanges();
            return true;
        }

        /// <summary>
        /// Метод вземащ колко месеца преди текущото дело трябва да е създадено делото за да излиза дали съществува дело с тези хора за бързо производство
        /// </summary>
        /// <returns></returns>
        private async Task<int> GetExsistCaseWithSamePeopleMonthsAgo()
        {
            string exsist_case_with_same_people_months_ago = await SystemParamGetValue("exsist_case_with_same_people_months_ago");

            return (string.IsNullOrEmpty(exsist_case_with_same_people_months_ago)) ? -2 : (int.Parse(exsist_case_with_same_people_months_ago) * -1);
        }

        /// <summary>
        /// Извлича данни за хора в делото за проверка за еднакъв състав
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<PersonSmallVM>> GetDataCasePersonsForExsistCaseWithSamePeople(int caseId)
        {
            return await repo.AllReadonly<CasePerson>()
                             .Where(x => x.CaseId == caseId)
                             .Where(x => x.CaseSessionId == null)
                             .Where(x => x.DateExpired == null)
                             .Where(x => NomenclatureConstants.PersonRole.PersonFastProcess.Contains(x.PersonRoleId))
                             .Select(x => new PersonSmallVM()
                             {
                                 Id = x.Id,
                                 Uic = x.Uic,
                                 FullName = x.FullName
                             })
                             .ToListAsync();
        }

        /// <summary>
        /// Метод връщащ кюери за дела с еднакъв състав с дадено дело
        /// </summary>
        /// <param name="peopleFind">Данн за състава</param>
        /// <param name="caseId">Идентификатор на текущо дело</param>
        /// <param name="regDate">Дата на регистарция на делото</param>
        /// <param name="monthsAgo">Колко месеца назад да търси дело с еднакъв състав</param>
        /// <returns></returns>
        private IQueryable<Case> GetQueryForExsistCaseWithSamePeople(List<PersonSmallVM> peopleFind, int caseId, DateTime regDate, int monthsAgo)
        {
            Expression<Func<Case, bool>> regDateFromWhere = x => x.RegDate >= regDate.AddMonths(monthsAgo);
            Expression<Func<Case, bool>> regDateToWhere = x => x.RegDate < regDate;

            IQueryable<Case> caseQuery = repo.AllReadonly<Case>()
                                             .Where(x => x.IsFastProcess ?? false)
                                             .Where(x => !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                                             .Where(x => x.Id != caseId)
                                             .Where(regDateFromWhere)
                                             .Where(regDateToWhere);

            foreach (var person in peopleFind)
            {
                person.Uic = string.IsNullOrEmpty(person.Uic) ? person.Id.ToString() : person.Uic;
                person.FullName = string.IsNullOrEmpty(person.FullName) ? person.Id.ToString() : person.FullName;

                string _null_string = "null_string";

                Expression<Func<Case, bool>> uicWhere = x => x.CasePersons
                                                              .Where(p => p.CaseSessionId == null)
                                                              .Any(p => (((p.Uic + p.FullName) == (person.Uic + person.FullName)) ||
                                                                         ((p.Uic ?? _null_string) == person.Uic) ||
                                                                         ((p.FullName ?? _null_string) == person.FullName)) &&
                                                                        NomenclatureConstants.PersonRole.PersonFastProcess.Contains(p.PersonRoleId) &&
                                                                        p.DateExpired == null);
                caseQuery = caseQuery.Where(uicWhere);
            }

            Expression<Func<Case, bool>> countPersonWhere = x => x.CasePersons
                                                                  .Where(p => p.CaseSessionId == null)
                                                                  .Count(p => p.DateExpired == null &&
                                                                              NomenclatureConstants.PersonRole.PersonFastProcess.Contains(p.PersonRoleId)) == peopleFind.Count;
            caseQuery = caseQuery.Where(countPersonWhere);

            return caseQuery;
        }


        /// <summary>
        /// Извличане на данни за дела с еднакъв състав с текущо дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="regDate">Дата на регистрация на дело</param>
        /// <returns></returns>
        public async Task<IQueryable<CaseVM>> GetExsistCaseWithSamePeople(int caseId, DateTime regDate)
        {
            List<PersonSmallVM> people = await GetDataCasePersonsForExsistCaseWithSamePeople(caseId);

            int monthsAgo = await GetExsistCaseWithSamePeopleMonthsAgo();

            IQueryable<Case> cases = GetQueryForExsistCaseWithSamePeople(people, caseId, regDate, monthsAgo);

            DateTime dateNow = DateTime.Now;

            return cases.Select(x => new CaseVM()
            {
                Id = x.Id,
                CourtId = x.CourtId,
                CourtLabel = x.Court.Label,
                CaseGroupId = x.CaseGroupId,
                CaseGroupLabel = (x.CaseGroup != null) ? x.CaseGroup.Label : string.Empty,
                CaseTypeId = x.CaseTypeId,
                CaseTypeLabel = (x.CaseType != null) ? x.CaseType.Code : string.Empty,
                CaseCodeId = x.CaseCodeId,
                CaseCodeLabel = (x.CaseCode != null) ? x.CaseCode.Code + " " + x.CaseCode.Label : string.Empty,
                ProcessPriorityId = x.ProcessPriorityId,
                ProcessPriorityLabel = (x.ProcessPriority != null) ? x.ProcessPriority.Label : string.Empty,
                CaseStateId = x.CaseStateId,
                CaseStateLabel = (x.CaseState != null) ? x.CaseState.Label : string.Empty,
                EISSPNumber = x.EISSPNumber,
                ShortNumber = Convert.ToString(int.Parse(x.ShortNumber)),
                RegNumber = x.RegNumber,
                RegDate = x.RegDate,
                CaseReasonLabel = (x.CaseReason != null) ? x.CaseReason.Label : string.Empty,
                CaseStateDescription = x.CaseStateDescription,
                JudgeReport = x.CaseLawUnits.Where(l => l.CaseSessionId == null && l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                        (l.DateTo ?? DateTime.Now.AddYears(100)) >= dateNow)
                                                                                                         .Select(l => l.LawUnit.FullName)
                                                                                                         .FirstOrDefault(),
                DepartmentOtdelenieText = (x.Otdelenie != null && x.Otdelenie.DepartmentTypeId != NomenclatureConstants.DepartmentType.Napravlenie ? x.Otdelenie.Label : string.Empty) +
                                                                                                        (x.JudicalComposition != null ? (x.Otdelenie != null && x.Otdelenie.DepartmentTypeId != NomenclatureConstants.DepartmentType.Napravlenie ? " / " + x.JudicalComposition.Label : x.JudicalComposition.Label) : string.Empty)

            })
                        .AsQueryable();
        }

        /// <summary>
        /// Извличане на данни за еднаквост на дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public IQueryable<CaseVM> GetCaseSimiliarCase(int caseId)
        {
            DateTime dateNow = DateTime.Now;

            return repo.AllReadonly<CaseSimilarCase>()
                       .Where(x => x.CaseId == caseId)
                       .Select(x => new CaseVM()
                       {
                           Id = x.SimilarCase.Id,
                           CourtId = x.SimilarCase.CourtId,
                           CourtLabel = x.SimilarCase.Court.Label,
                           CaseGroupId = x.SimilarCase.CaseGroupId,
                           CaseGroupLabel = (x.SimilarCase.CaseGroup != null) ? x.SimilarCase.CaseGroup.Label : string.Empty,
                           CaseTypeId = x.SimilarCase.CaseTypeId,
                           CaseTypeLabel = (x.SimilarCase.CaseType != null) ? x.SimilarCase.CaseType.Code : string.Empty,
                           CaseCodeId = x.SimilarCase.CaseCodeId,
                           CaseCodeLabel = (x.SimilarCase.CaseCode != null) ? x.SimilarCase.CaseCode.Code + " " + x.SimilarCase.CaseCode.Label : string.Empty,
                           ProcessPriorityId = x.SimilarCase.ProcessPriorityId,
                           ProcessPriorityLabel = (x.SimilarCase.ProcessPriority != null) ? x.SimilarCase.ProcessPriority.Label : string.Empty,
                           CaseStateId = x.SimilarCase.CaseStateId,
                           CaseStateLabel = (x.SimilarCase.CaseState != null) ? x.SimilarCase.CaseState.Label : string.Empty,
                           EISSPNumber = x.SimilarCase.EISSPNumber,
                           ShortNumber = Convert.ToString(int.Parse(x.SimilarCase.ShortNumber)),
                           RegNumber = x.SimilarCase.RegNumber,
                           RegDate = x.SimilarCase.RegDate,
                           CaseReasonLabel = (x.SimilarCase.CaseReason != null) ? x.SimilarCase.CaseReason.Label : string.Empty,
                           CaseStateDescription = x.SimilarCase.CaseStateDescription,
                           JudgeReport = x.SimilarCase.CaseLawUnits.Where(l => l.CaseSessionId == null && l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                                   (l.DateTo ?? DateTime.Now.AddYears(100)) >= dateNow)
                                                                                                         .Select(l => l.LawUnit.FullName)
                                                                                                         .FirstOrDefault(),
                           DepartmentOtdelenieText = (x.SimilarCase.Otdelenie != null && x.SimilarCase.Otdelenie.DepartmentTypeId != NomenclatureConstants.DepartmentType.Napravlenie ? x.SimilarCase.Otdelenie.Label : string.Empty) +
                                                                                                        (x.SimilarCase.JudicalComposition != null ? (x.SimilarCase.Otdelenie != null && x.SimilarCase.Otdelenie.DepartmentTypeId != NomenclatureConstants.DepartmentType.Napravlenie ? " / " + x.SimilarCase.JudicalComposition.Label : x.SimilarCase.JudicalComposition.Label) : string.Empty)

                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Проверка и запис на подобни дела за бързо производство
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="regDate">Дата на регистрация на делото</param>
        /// <returns></returns>
        public async Task<string[]> GetSimilarCase(int caseId, DateTime regDate)
        {
            try
            {
                List<CaseVM> cases = (await (await GetExsistCaseWithSamePeople(caseId, regDate)).ToListAsync()).GroupBy(g => g.Id).Select(g => g.FirstOrDefault()).ToList();

                Case @case = await repo.All<Case>()
                                     .Where(x => x.Id == caseId)
                                     .Where(x => !(x.IsReadSimilarCases ?? false))
                                     .FirstOrDefaultAsync();

                if (@case == null)
                    return null;

                @case.IsReadSimilarCases = true;

                if (cases.Count < 1)
                {
                    await repo.SaveChangesAsync();
                    return null;
                }

                repo.AddRange(cases.Select(x => new CaseSimilarCase()
                {
                    CaseId = caseId,
                    SimilarCaseId = x.Id
                }));

                await repo.SaveChangesAsync();

                return cases.OrderBy(x => x.RegDate)
                            .Select(x => x.RegNumber + " (" + x.CourtLabel + ")")
                            .ToArray();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Проблем при проверка и запис на подобни дела за бързо производство CaseId={caseId}");
                return null;
            }
        }

        /// <summary>
        /// Метод извличащ всички идентификатори на предходни дела от миграциите
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<int>> GetPriorCaseIds(int caseId)
        {
            return await repo.AllReadonly<CaseMigration>()
                             .Where(x => x.CaseId == caseId)
                             .Select(x => x.PriorCaseId)
                             .ToListAsync();
        }

        /// <summary>
        /// Метод връщащ данни за дело
        /// </summary>
        /// <param name="id">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<CaseVM> GetCaseData(int id)
        {
            DateTime dateNow = DateTime.Now;
            DateTime dateAddYesr = DateTime.Now.AddYears(100);

            return await repo.AllReadonly<Case>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseVM()
                             {
                                 Id = x.Id,
                                 CourtId = x.CourtId,
                                 CourtLabel = x.Court.Label,
                                 CaseGroupId = x.CaseGroupId,
                                 CaseGroupLabel = x.CaseGroup.Label,
                                 CaseTypeId = x.CaseTypeId,
                                 CaseTypeLabel = x.CaseType.Label,
                                 CaseTypeCode = x.CaseType.Code,
                                 CaseCodeId = x.CaseCodeId,
                                 CaseCodeLabel = (x.CaseCodeId != null) ? x.CaseCode.Code + " " + x.CaseCode.Label : string.Empty,
                                 ProcessPriorityId = x.ProcessPriorityId,
                                 ProcessPriorityLabel = (x.ProcessPriorityId != null) ? x.ProcessPriority.Label : string.Empty,
                                 CaseStateId = x.CaseStateId,
                                 CaseStateLabel = x.CaseState.Label,
                                 EISSPNumber = x.EISSPNumber,
                                 ShortNumber = x.ShortNumber,
                                 RegNumber = x.RegNumber,
                                 RegNumberText = x.CaseType.Code + " " + x.ShortNumber + "/" + x.RegDate.ToString("yyyy"),
                                 RegDate = x.RegDate,
                                 LoadGroupLinkLabel = (x.LoadGroupLinkId != null) ? x.LoadGroupLink.LoadGroup.Label : string.Empty,
                                 DocumentId = x.DocumentId,
                                 DocumentName = x.Document.DocumentNumber + " / " + x.Document.DocumentDate.ToString("dd.MM.yyyy"),
                                 DocumentTypeId = x.Document.DocumentTypeId,
                                 DocumentTypeName = x.Document.DocumentType.Label,
                                 CaseInstanceId = x.CaseType.CaseInstanceId,
                                 CaseReasonLabel = (x.CaseReasonId != null) ? x.CaseReason.Label : string.Empty,
                                 CaseStateDescription = x.CaseStateDescription,
                                 CaseInforcedDate = x.CaseInforcedDate,
                                 IsFastProcess = x.IsFastProcess ?? false,
                                 DepartmentOtdelenieText = x.OtdelenieId != null && x.Otdelenie.DepartmentTypeId == NomenclatureConstants.DepartmentType.Otdelenie ? x.Otdelenie.Label : string.Empty,
                                 IsSecret = x.CaseClassifications.Where(c => (c.DateTo ?? dateAddYesr) > dateNow)
                                                                 .Any(c => c.ClassificationId == CaseClassifications.Secret),
                                 IsSpecial = x.CaseClassifications.Where(c => (c.DateTo ?? dateAddYesr) > dateNow)
                                                                  .Any(c => c.ClassificationId == CaseClassifications.SpecialAccess),
                                 IsRestriction = x.CaseClassifications.Where(c => (c.DateTo ?? dateAddYesr) > dateNow)
                                                                      .Any(c => c.ClassificationId == CaseClassifications.Restriction),
                                 IsUnderAge = x.CaseClassifications.Where(c => (c.DateTo ?? dateAddYesr) > dateNow)
                                                                   .Any(c => c.ClassificationId == CaseClassifications.UnderAge),
                                 IsExistsSignedExecList = (x.CaseSessionActs.Any(a => NomenclatureConstants.ActType.ExecListActs.Contains(a.ActTypeId) &&
                                                                                              a.DateExpired == null &&
                                                                                              a.ActDeclaredDate != null) ||
                                                           x.ExecLists.Any(e => e.DateSigned != null &&
                                                                                e.DateExpired == null)),
                                 IsFirsInstantsFastProcessCase = x.CaseMigrations.Any(m => m.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptanceSubordinateCourtAppeal &&
                                                                                           (m.PriorCase.IsFastProcess ?? false) &&
                                                                                           m.DateExpired == null),
                                 AssignmentDocumentId = x.Document.AssignmentDocumentId,
                                 ElectronicDocumentId = x.Document.ElectronicDocumentId,
                                 IsMediation = x.IsMediation ?? false,
                                 IsCaseCode_0602_0604 = false,
                                 IsReadSimilarCases = x.IsReadSimilarCases ?? false,
                                 IsExsistCaseWithSamePeople = (x.IsReadSimilarCases ?? false) ? string.Join(", ", x.SimilarCases.Select(s => s.SimilarCase.RegNumber + " (" + s.SimilarCase.Court.Label + ")")) : string.Empty
                             })
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Сетване на индикатори по дело
        /// </summary>
        /// <param name="model">Модел с данни за дело</param>
        /// <returns></returns>
        private async Task CaseSetClassifications(CaseVM model)
        {
            DateTime dateNow = DateTime.Now;
            DateTime dateAddYesr = DateTime.Now.AddYears(100);

            List<CaseClassification> classifications = await repo.AllReadonly<CaseClassification>()
                                                                 .Where(x => x.CaseId == model.Id)
                                                                 .Where(x => (x.DateTo ?? dateAddYesr) > dateNow)
                                                                 .ToListAsync();

            model.IsSecret = classifications.Any(c => c.ClassificationId == CaseClassifications.Secret);
            model.IsSpecial = classifications.Any(c => c.ClassificationId == CaseClassifications.SpecialAccess);
            model.IsRestriction = classifications.Any(c => c.ClassificationId == CaseClassifications.Restriction);
            model.IsUnderAge = classifications.Any(c => c.ClassificationId == CaseClassifications.UnderAge);
        }

        /// <summary>
        /// Метод сетващ в данни за дело, полета свързани с хора
        /// </summary>
        /// <param name="model">Модел с данни за дело</param>
        /// <returns></returns>
        private async Task CaseSetPerson(CaseVM model)
        {
            var people = await repo.AllReadonly<CasePerson>()
                                   .Where(x => x.CaseId == model.Id)
                                   .Where(x => x.DateExpired == null)
                                   .Where(x => x.CaseSessionId == null)
                                   .Select(x => new
                                   {
                                       PersonFullName = x.FullName,
                                       PersonRoleLabel = x.PersonRole.Label,
                                       PersonIsDeceased = x.IsDeceased ?? false,
                                       PersonRoleKindId = x.PersonRole.RoleKindId
                                   })
                                   .ToListAsync();

            model.IsDeceased = people.Any(p => p.PersonIsDeceased);
            model.CasePersonMain = people.Where(p => p.PersonRoleKindId == PersonKinds.LeftSide)
                                         .Select(p => p.PersonFullName + " (" + p.PersonRoleLabel + ")")
                                         .FirstOrDefault() + " - " +
                                   people.Where(p => p.PersonRoleKindId == PersonKinds.RightSide)
                                         .Select(p => p.PersonFullName + " (" + p.PersonRoleLabel + ")")
                                    .FirstOrDefault() + " ...";
        }

        /// <summary>
        /// Метод връщащ дали има обвързано дело за бързо производство
        /// </summary>
        /// <param name="model">Модел с данни за дело</param>
        /// <returns></returns>
        private async Task<bool> CaseSetIsCaseCode_0602_0604(CaseVM model)
        {
            DateTime date = new(2025, 7, 1);

            if (NomenclatureConstants.CaseCode.CaseCode_0602_1_2_0604_1_2.Contains(model.CaseCodeId ?? 0) && model.RegDate >= date)
            {
                List<int> priorCaseIds = await GetPriorCaseIds(model.Id);
                priorCaseIds.Add(model.Id);

                bool isAnyCaseCode_FastProcessNull = await repo.AllReadonly<Case>()
                                                               .Where(x => priorCaseIds.Contains(x.Id))
                                                               .Where(x => x.CaseMigrations.Any(m => NomenclatureConstants.CaseCode.CaseCode_FastProcessNull.Contains(m.PriorCase.CaseCodeId) &&
                                                                                                     m.DateExpired == null))
                                                               .AnyAsync();

                return !isAnyCaseCode_FastProcessNull;
            }

            return false;
        }

        /// <summary>
        /// Метод вземащ последната миграция за дело
        /// </summary>
        /// <param name="id">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<string> CaseSetLastMigration(int id)
        {
            return await caseMigrationService.SelectOutMove(id)
                                             .OrderByDescending(x => x.Id)
                                             .Select(x => (!x.IsReturned ?? true) ? (x.SentToName + " с Изх.№: " + x.OutDocumentLabel) : string.Empty)
                                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Метод сетващ данни за архив на дело
        /// </summary>
        /// <param name="model">Модел с данни за дело</param>
        /// <returns></returns>
        private async Task CaseSetArchive(CaseVM model)
        {
            var archive = await repo.AllReadonly<CaseArchive>()
                                    .Where(x => x.CaseId == model.Id)
                                    .Select(x => new
                                    {
                                        HasArchive = true,
                                        ArchRegNumber = x.RegNumber,
                                        ArchRegDate = x.RegDate,
                                        ArchiveIndexLabel = x.ArchiveIndexId != null ? x.CourtArchiveIndex.Label : string.Empty,
                                        ArchiveLink = x.ArchiveLink,
                                        StorageYears = x.StorageYears,
                                        BookNumber = x.BookNumber,
                                        BookYear = x.BookYear
                                    })
                                    .FirstOrDefaultAsync();

            if (archive != null)
            {
                model.HasArchive = archive.HasArchive;
                model.ArchRegNumber = archive.ArchRegNumber;
                model.ArchRegDate = archive.ArchRegDate;
                model.ArchiveIndexLabel = archive.ArchiveIndexLabel;
                model.ArchiveLink = archive.ArchiveLink;
                model.StorageYears = archive.StorageYears;
                model.BookNumber = archive.BookNumber;
                model.BookYear = archive.BookYear;
            }
        }

        /// <summary>
        /// Изчитане на данни за дело по ид
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CaseVM> Case_GetById(int id)
        {
            CaseVM result = await GetCaseData(id);
            await CaseSetPerson(result);
            result.IsCaseCode_0602_0604 = await CaseSetIsCaseCode_0602_0604(result);
            result.LastMovment = await caseMovementService.GetLastMovmentForCaseId(result.Id);
            result.LastMigration = await CaseSetLastMigration(id);
            await CaseSetArchive(result);

            if (result.IsFastProcess && !result.IsReadSimilarCases)
            {
                string[] caseReg = await GetSimilarCase(result.Id, result.RegDate);
                if (caseReg != null && caseReg.Any())
                    result.IsExsistCaseWithSamePeople = string.Join(", ", caseReg);
            }

            if (result.IsFastProcess)
            {
                if ((result.AssignmentDocumentId ?? 0) > 0)
                {
                    //result.ExistsPayToAssignmentDocument = await moneyService.HasDocumentObligationPayments(result.AssignmentDocumentId ?? 0);
                    result.ExistsPayToAssignmentDocument = await moneyService.Obligation_Select(0, result.DocumentId, 0, 0, result.AssignmentDocumentId ?? 0).AnyAsync(x => x.AmountPay > (decimal)0.001);
                }
            }

            return result;
        }


        /// <summary>
        /// Изчитане на минимални данни за дело по ид само за призовка
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CaseForNotificationVM> Case_GetByIdForNotification(int id)
        {
            return await repo.AllReadonly<Case>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseForNotificationVM()
                             {
                                 Id = x.Id,
                                 CaseTypeCode = x.CaseType.Code,
                                 RegNumber = x.RegNumber,
                                 RegDate = x.RegDate,
                                 DepartmentOtdelenieText = x.OtdelenieId != null && x.Otdelenie.DepartmentTypeId == NomenclatureConstants.DepartmentType.Otdelenie ? x.Otdelenie.Label : string.Empty,
                             })
                             .FirstOrDefaultAsync();
        }
        /// <summary>
        /// Изчитане на данни в модел за редакция
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CaseEditVM> Case_SelectForEdit(int id)
        {
            var result = await repo.AllReadonly<Case>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseEditVM()
                             {
                                 Id = x.Id,
                                 DateWrt = x.DateWrt,
                                 CaseGroupId = x.CaseGroupId,
                                 CaseGroupName = x.CaseGroup.Label,
                                 EISSPNumber = x.EISSPNumber,
                                 CaseTypeId = x.CaseTypeId,
                                 CaseCodeId = x.CaseCodeId ?? 0,
                                 CaseTypeUnitId = x.CaseTypeUnitId,
                                 CaseCharacterId = x.CaseCharacterId,
                                 LoadGroupLinkId = x.LoadGroupLinkId,
                                 ComplexIndex = x.ComplexIndex,
                                 CourtTypeId = x.Court.CourtType.Id,
                                 CaseStateId = x.CaseStateId,
                                 CourtId = x.CourtId,
                                 CourtGroupId = x.CourtGroupId ?? 0,
                                 CourtGroupName = (x.CourtGroupId > 0) ? x.CourtGroup.Label : "",
                                 Description = x.Description,
                                 CaseStateDescription = x.CaseStateDescription,
                                 DocumentId = x.DocumentId,
                                 DocumentName = x.Document.DocumentNumber + " / " + x.Document.DocumentDate.ToString("dd.MM.yyyy"),
                                 DocumentTypeId = x.Document.DocumentTypeId,
                                 DocumentTypeName = x.Document.DocumentType.Label,
                                 RegNumber = x.RegNumber,
                                 RegDate = x.RegDate,
                                 RegNumberText = !string.IsNullOrEmpty(x.RegNumber) ? x.CaseType.Code + " " + x.ShortNumber + "/" + x.RegDate.ToString("yyyy") : string.Empty,
                                 IsOldNumber = (x.IsOldNumber ?? false),
                                 CaseReasonId = x.CaseReasonId,
                                 CaseInforcedDate = x.CaseInforcedDate,
                                 ProcessPriorityId = x.ProcessPriorityId,
                                 IsNewCaseNewNumber = x.IsNewCaseNewNumber,
                                 IsRenewCase = x.IsRenewCase,
                                 ComplexIndexActual = x.ComplexIndexActual,
                                 ComplexIndexLegal = x.ComplexIndexLegal,
                                 IspnCaseCompetenceId = x.IspnCaseCompetenceId,
                                 DebtorsCount = x.DebtorsCount,
                                 IsFastProcess = x.IsFastProcess ?? false,
                                 RnflProcessTypeId = x.RnflProcessTypeId,
                                 IspnKind = x.IspnKind,
                                 RowVersion = x.RowVersion
                             }).FirstOrDefaultAsync();

            if (result == null)
            {
                return null;
            }

            var caseLawUnitCount = await repo.AllReadonly<CaseLawUnitCount>(x => x.CaseId == id)
                        .Select(x => new { x.JudgeRoleId, x.PersonCount })
                        .Where(x => NomenclatureConstants.JudgeRole.ReserveRolesList.Contains(x.JudgeRoleId))
                        .ToListAsync();
            switch (caseLawUnitCount.Count)
            {
                case 1:
                    result.CaseTypeUnitReserves = caseLawUnitCount.First().JudgeRoleId;
                    break;
                case 2:
                    result.CaseTypeUnitReserves = NomenclatureConstants.JudgeRole.ReserveJudgeAndJury;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за дело
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<CaseFolderItemVM> GetCase_FolderItem(int id)
        {
            var _case = await GetCaseInfo(id);
            return new CaseFolderItemVM()
            {
                SourceType = SourceTypeSelectVM.Case,
                SourceId = _case.Id.ToString(),
                Title = $"{_case.CaseGroupLabel} {_case.RegNumber}/{_case.RegDate:dd.MM.yyyy HH:mm}",
                Date = _case.RegDate,
                TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.Case),
                ElementUrl = urlHelper.Action("CasePreview", "Case", new { id }) + "#tabMainData"
            };
        }

        private async Task<CaseFolderItemVM> GetCaseInforceDate_FolderItem(int id)
        {
            var _case = await GetCaseInfo(id);
            if (_case.CaseInforcedDate != null)
            {
                return new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.CaseInforcedDate,
                    SourceId = _case.Id.ToString(),
                    Title = $"Дата на влизане в законна сила {_case.CaseInforcedDate:dd.MM.yyyy}",
                    Date = _case.CaseInforcedDate,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.Case),
                    ElementUrl = urlHelper.Action("CasePreview", "Case", new { id }) + "#tabMainData"
                };
            }
            else
                return null;
        }

        /// <summary>
        /// Хронология: данни за входящи документи
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetCaseInDocs_FolderItem(int caseId)
        {
            List<CaseFolderItemVM> results = new();

            var queryDocumentCaseInfo = repo.AllReadonly<DocumentCaseInfo>()
                                            .Where(x => x.CaseId == caseId &&
                                                        x.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument &&
                                                        x.Document.DateExpired == null);

            results.AddRange(await queryDocumentCaseInfo.Select(x => new CaseFolderItemVM
            {
                SourceType = SourceTypeSelectVM.CaseInDoc,
                SourceId = x.Document.Id.ToString(),
                Title = $"Вх.№ {x.Document.DocumentNumber}/{x.Document.DocumentDate:dd.MM.yyyy} {x.Document.DocumentType.Label}",
                Date = x.Document.DocumentDate,
                TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseInDoc),
                ElementUrl = urlHelper.Action("View", "Document", new { id = x.Document.Id }),
                Description = x.Document.Description
            })
            .ToListAsync());

            results.AddRange(await repo.AllReadonly<WorkTask>()
                                       .Where(x => x.SourceType == SourceTypeSelectVM.Document)
                                       .Where(x => queryDocumentCaseInfo.Select(d => d.DocumentId).ToArray().Contains(x.SourceId))
                                       .Select(w => new CaseFolderItemVM()
                                       {
                                           SourceType = SourceTypeSelectVM.WorkTask,
                                           SourceId = w.Id.ToString(),
                                           Title = "Задача към съпровождащ документ: " + queryDocumentCaseInfo.Where(d => d.Id == w.SourceId)
                                                                                                              .Select(d => $"Вх.№ {d.Document.DocumentNumber}/{d.Document.DocumentDate:dd.MM.yyyy} {d.Document.DocumentType.Label}")
                                                                                                              .FirstOrDefault() + " " +
                                                   queryDocumentCaseInfo.Where(d => d.Id == w.SourceId)
                                                                        .Select(d => d.Document.DocumentDate.ToString("dd.MM.yyyy"))
                                                                        .FirstOrDefault() + " задача: " + w.TaskType.Label +
                                                   " - изпратена на: " + w.DateCreated.ToString("dd.MM.yyyy") +
                                                   (w.DateCompleted != null ? " дата на приключване: " + (w.DateCompleted ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty) +
                                                   (!string.IsNullOrEmpty(w.Description) ? " забележка: " + w.Description : string.Empty) +
                                                   (!string.IsNullOrEmpty(w.DescriptionCreated) ? " описание: " + w.DescriptionCreated : string.Empty),
                                           Date = w.DateCreated,
                                           TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.WorkTask)
                                       })
                                       .ToListAsync());

            return results;
        }

        private async Task<CaseFolderItemVM> GetCaseDocument_FolderItem(int caseId)
        {
            return await repo.AllReadonly<Case>()
                             .Where(x => x.Id == caseId)
                             .Select(x => new CaseFolderItemVM()
                             {
                                 SourceType = SourceTypeSelectVM.CaseInDoc,
                                 SourceId = x.DocumentId.ToString(),
                                 Title = "Вх.№ " + x.Document.DocumentNumber + "/" + x.Document.DocumentDate.ToString("dd.MM.yyyy") + " " + x.Document.DocumentType.Label,
                                 Date = x.Document.DocumentDate,
                                 TypeName = "Иницииращ документ",
                                 ElementUrl = urlHelper.Action("View", "Document", new { id = x.DocumentId }),
                                 Description = x.Document.Description
                             })
                             .FirstAsync();
        }

        private async Task<List<CaseFolderItemVM>> GetWorkTask_FolderItem(int caseId)
        {
            var queryCase = repo.AllReadonly<Case>()
                                .Where(c => c.Id == caseId);

            return await repo.AllReadonly<WorkTask>()
                             .Where(w => w.SourceType == SourceTypeSelectVM.Document)
                             .Where(w => (w.SourceType == SourceTypeSelectVM.Document && w.SourceId == queryCase.Select(c => c.DocumentId).First()) ||
                                         (w.SourceType == SourceTypeSelectVM.Case && w.SourceId == caseId) ||
                                         (w.SourceType == SourceTypeSelectVM.DocumentResolution && queryCase.Any(c => c.Document
                                                                                                                       .DocumentResolutions
                                                                                                                       .Any(c => c.DateExpired == null && c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced &&
                                                                                                                                 c.Id == w.SourceId))))
                             .Select(w => new CaseFolderItemVM()
                             {
                                 SourceType = SourceTypeSelectVM.WorkTask,
                                 SourceId = w.Id.ToString(),
                                 Title = "Задача: " + w.TaskType.Label +
                                         " - изпратена на: " + w.DateCreated.ToString("dd.MM.yyyy") +
                                         (w.DateCompleted != null ? " дата на приключване: " + (w.DateCompleted ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty) +
                                         (!string.IsNullOrEmpty(w.Description) ? " забележка: " + w.Description : string.Empty) +
                                         (!string.IsNullOrEmpty(w.DescriptionCreated) ? " описание: " + w.DescriptionCreated : string.Empty),
                                 Date = w.DateCreated,
                                 TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.WorkTask)
                             })
                             .ToListAsync();
        }

        private async Task<List<CaseFolderItemVM>> GetCaseMigration_FolderItem(int caseId)
        {
            var caseMigrations = await caseMigrationService.Select(caseId).ToListAsync();
            return caseMigrations.Select(x => new CaseFolderItemVM()
            {
                SourceType = SourceTypeSelectVM.CaseMigration,
                SourceId = x.Id.ToString(),
                Title = string.IsNullOrEmpty(x.CaseRegNumber) ? $"Дело номер {x.CaseStateName} Вид движение: {x.MigrationTypeName} Движение от-към: {x.SentFromName} - {x.SentToName} " : $"Дело номер {x.CaseRegNumber}/{x.CaseRegDate:dd.MM.yyyy} Вид движение: {x.MigrationTypeName} Движение от-към: {x.SentFromName} - {x.SentToName} ",
                Date = x.DateWrt,
                TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseMigration),
                ElementUrl = urlHelper.Action("Index", "CaseMigration", new { caseId = caseId })
            })
                .ToList();
        }

        /// <summary>
        /// Хронология: данни за престъпления
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetCaseCrime_FolderItem(int caseId)
        {
            return await repo.AllReadonly<CaseCrime>()
                             .Where(x => x.CaseId == caseId)
                             .Select(x => new CaseFolderItemVM
                             {
                                 SourceType = SourceTypeSelectVM.CaseCrime,
                                 SourceId = x.Id.ToString(),
                                 Title = $"ЕИСПП № {x.EISSPNumber} {x.CrimeName}",
                                 Date = x.Case.RegDate,
                                 TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseCrime),
                                 ElementUrl = urlHelper.Action("IndexCaseCrime", "CasePersonSentence", new { caseId = x.CaseId })
                             })
                             .ToListAsync();
        }

        /// <summary>
        /// Хронология: данни за хора към престъпление
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetCasePersonCrime_FolderItem(int caseId)
        {
            return await repo.AllReadonly<CasePersonCrime>()
                             .Where(x => x.CaseId == caseId)
                             .Select(x => new CaseFolderItemVM
                             {
                                 SourceType = SourceTypeSelectVM.CasePersonCrime,
                                 SourceId = x.Id.ToString(),
                                 Title = x.CaseCrime.CrimeName + " - " + x.CasePerson.FullName + " - " + x.PersonRoleInCrime.Label + " - " + x.RecidiveType.Label,
                                 Date = x.Case.RegDate,
                                 TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CasePersonCrime),
                                 ElementUrl = urlHelper.Action("IndexCasePersonCrime", "CasePersonSentence", new { caseCrimeId = x.CaseCrimeId })
                             })
                             .ToListAsync();
        }

        /// <summary>
        /// Хронология: данни за бюлетин
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetCasePersonSentenceBulletin_FolderItem(int caseId)
        {
            return await repo.AllReadonly<CasePersonSentenceBulletin>()
                             .Where(x => x.CaseId == caseId)
                             .Select(x => new CaseFolderItemVM
                             {
                                 SourceType = SourceTypeSelectVM.CasePersonBulletin,
                                 SourceId = x.Id.ToString(),
                                 Title = x.CasePerson.FullName +
                                         (!string.IsNullOrEmpty(x.BirthDayPlace) ? " местораждане: " + x.BirthDayPlace : string.Empty) +
                                         (" дата на раждане: " + x.BirthDay.ToString("dd.MM.yyyy")) +
                                         (!string.IsNullOrEmpty(x.Nationality) ? " гражданство: " + x.Nationality : string.Empty),
                                 //+(!string.IsNullOrEmpty(x.FamilyMarriage) ? " фамилно име преди сключване на брак: " + x.FamilyMarriage : string.Empty),
                                 Date = x.DateWrt,
                                 TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CasePersonBulletin),
                                 ElementUrl = urlHelper.Action("Index", "CasePersonSentence", new { casePersonId = x.CasePersonId })
                             })
                             .ToListAsync();
        }

        /// <summary>
        /// Хронология: данни за присъди
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetCasePersonSentence_FolderItem(int caseId)
        {
            return await repo.AllReadonly<CasePersonSentence>()
                             .Where(x => x.CaseId == caseId)
                             .Select(x => new CaseFolderItemVM
                             {
                                 SourceType = SourceTypeSelectVM.CasePersonSentence,
                                 SourceId = x.Id.ToString(),
                                 Title = x.CasePerson.FullName + " - постановена от: " + x.DecreedCourt.Label + " - акт: " + x.CaseSessionAct.RegNumber + "/" + (x.CaseSessionAct.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") + "г.",
                                 Date = x.CaseSessionAct.RegDate,
                                 TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CasePersonSentence),
                                 ElementUrl = urlHelper.Action("Index", "CasePersonSentence", new { casePersonId = x.CasePersonId })
                             })
                             .ToListAsync();
        }

        /// <summary>
        /// Хронология: данни за наказания към присъда
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetCasePersonSentencePunishment_FolderItem(int caseId)
        {
            return await repo.AllReadonly<CasePersonSentencePunishment>()
                             .Where(x => x.CaseId == caseId)
                             .Select(x => new CaseFolderItemVM
                             {
                                 SourceType = SourceTypeSelectVM.CasePersonSentencePunishment,
                                 SourceId = x.Id.ToString(),
                                 Title = $"Наложено наказание по НК: {x.SentenceType.Label} Сумарен ред за присъди: {(x.IsSummaryPunishment ? "Да" : "Не")}",
                                 Date = x.DateFrom,
                                 TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CasePersonSentencePunishment),
                                 ElementUrl = urlHelper.Action("IndexCasePersonSentencePunishment", "CasePersonSentence", new { casePersonSentenceId = x.CasePersonSentenceId })
                             })
                             .ToListAsync();
        }

        /// <summary>
        /// Хронология: данни за престъпления към присъда
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetCasePersonSentencePunishmentCrime_FolderItem(int caseId)
        {
            return await repo.AllReadonly<CasePersonSentencePunishmentCrime>()
                             .Where(x => x.CaseId == caseId)
                             .Select(x => new CaseFolderItemVM
                             {
                                 SourceType = SourceTypeSelectVM.CasePersonSentencePunishmentCrime,
                                 SourceId = x.Id.ToString(),
                                 Title = $"Участие в наложени наказания към присъда. Престъпление: {x.CaseCrime.CrimeName} роля: {x.PersonRoleInCrime.Label} рецидив: {x.RecidiveType.Label}",
                                 Date = x.CasePersonSentencePunishment.DateFrom,
                                 TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CasePersonSentencePunishmentCrime),
                                 ElementUrl = urlHelper.Action("IndexCasePersonSentencePunishmentCrime", "CasePersonSentence", new { CasePersonSentencePunishmentId = x.CasePersonSentencePunishmentId })
                             })
                             .ToListAsync();
        }

        /// <summary>
        /// Хронология: данни за интервали
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetCaseLifecycle_FolderItem(int caseId)
        {
            return await repo.AllReadonly<CaseLifecycle>()
                             .Where(x => x.CaseId == caseId)
                             .Select(x => new CaseFolderItemVM
                             {
                                 SourceType = SourceTypeSelectVM.CaseLifecycle,
                                 SourceId = x.Id.ToString(),
                                 Title = $"Интервал по дело: {x.LifecycleType.Label} повторение {x.Iteration} от {x.DateFrom:dd.MM.yyyy HH mm}",
                                 Date = x.DateFrom,
                                 TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseLifecycle),
                                 ElementUrl = urlHelper.Action("Index", "CaseLifecycle", new { id = x.CaseId })
                             })
                             .ToListAsync();
        }

        /// <summary>
        /// Хронология: данни за разводи
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetCaseSessionActDivorce_FolderItem(int caseId)
        {
            return await repo.AllReadonly<CaseSessionActDivorce>()
                             .Where(x => x.CaseId == caseId)
                             .Select(x => new CaseFolderItemVM
                             {
                                 SourceType = SourceTypeSelectVM.CaseSessionActDivorce,
                                 SourceId = x.Id.ToString(),
                                 Title = $"Съобщение за прекратяване на граждански брак {x.RegNumber} от {x.RegDate:dd.MM.yyyy HH mm}",
                                 Date = x.RegDate,
                                 TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseSessionActDivorce),
                                 ElementUrl = urlHelper.Action("Index", "CaseLifecycle", new { id = x.CaseId })
                             })
                             .ToListAsync();
        }

        /// <summary>
        /// Хронология: данни за наследство
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetCasePersonInheritance_FolderItem(int caseId)
        {
            return await repo.AllReadonly<CasePersonInheritance>()
                             .Where(x => x.CaseId == caseId && x.DateExpired == null)
                             .Select(x => new CaseFolderItemVM
                             {
                                 SourceType = SourceTypeSelectVM.CasePersonInheritance,
                                 SourceId = x.Id.ToString(),
                                 Title = $"Наследство на {x.CasePerson.FullName} Постановена от {x.Court.Label} Акт: {x.CaseSessionAct.RegNumber} {x.CaseSessionAct.RegDate:dd.MM.yyyy HH mm} - {x.CasePersonInheritanceResult.Label}",
                                 Date = x.CaseSessionAct.RegDate,
                                 TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CasePersonInheritance),
                                 ElementUrl = urlHelper.Action("Index", "CaseLifecycle", new { id = x.CaseId })
                             })
                             .ToListAsync();
        }

        /// <summary>
        /// Хронология: данни за изпълнителни листове
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private List<CaseFolderItemVM> GetExecList_FolderItem(int caseId)
        {
            return repo.AllReadonly<ExecList>()
                       .Where(x => x.IsActive == true &&
                                   x.ExecListObligations.Where(a => a.Obligation.CaseSessionAct.CaseId == caseId).Any())
                       .Select(x => new CaseFolderItemVM
                       {
                           SourceType = SourceTypeSelectVM.ExecList,
                           SourceId = x.Id.ToString(),
                           Title = (x.RegDate != null ? (x.RegNumber + "/" + x.RegDate.DateToStr(FormattingConstant.NormalDateFormat) + " ") : "") +
                                   x.ExecListType.Label + " " +
                                   string.Join(",", x.ExecListObligations.Select(o => o.Obligation.FullName).Distinct()) + " " +
                                   x.ExecListObligations.Sum(o => o.Amount ?? 0),
                           Date = x.RegDate,
                           TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.ExecList),
                           ElementUrl = string.Empty
                       }).ToList();
        }

        /// <summary>
        /// Хронология: данни за заседания
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetCaseSession_FolderItem(int caseId)
        {
            var caseSessionVMs = await caseSessionService.CaseSession_OldSelect(caseId, null, null)
                                                         .ToListAsync();

            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();

            foreach (var item in caseSessionVMs)
            {
                var sessionFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.CaseSession,
                    SourceId = item.Id.ToString(),
                    Title = $"{item.SessionTypeLabel} от {item.DateFrom.ToString("dd.MM.yyyy HH:mm")} до {(item.DateTo ?? DateTime.Now).ToString("dd.MM.yyyy HH:mm")}",
                    Date = item.DateFrom,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseSession),
                    ElementUrl = urlHelper.Action("Preview", "CaseSession", new { id = item.Id }) + "#tabMainData"
                };

                result.Add(sessionFolder);
                //result.AddRange(GetLawUnit_FolderItem(caseId, item.Id));
                //result.AddRange(GetPerson_FolderItem(caseId, item.Id));
                result.AddRange(await GetNotification_FolderItem(caseId, item.Id, null));
                result.AddRange(await GetMoney_FolderItem(caseId, item.Id));
                //result.AddRange(GetSessionResult_FolderItem(item.Id, sessionFolder.Title));
                result.AddRange(await GetSessionMeeting_FolderItem(item.Id, sessionFolder.Title));
                result.AddRange(await GetSessionDoc_FolderItem(item.Id, sessionFolder.Title));
                result.AddRange(await GetSessionAct_FolderItem(item.Id, sessionFolder.Title));
                result.AddRange(await GetCaseSessionFastDocument_FolderItem(item.Id, sessionFolder.Title, sessionFolder.Date ?? DateTime.Now));
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за движение
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetCaseMovement_FolderItem(int caseId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseMovements = await caseMovementService.GetCaseMovementData(caseId);

            foreach (var item in caseMovements)
            {
                var movementFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.CaseMovement,
                    SourceId = item.Id.ToString(),
                    Title = $"Вид: {item.MovementTypeLabel} към: {item.NameFor} изпратено: {item.DateSend.ToString("dd.MM.yyyy HH:mm")} прието от: {item.UserLawUnitName}",
                    Date = item.DateSend,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseMovement),
                    ElementUrl = urlHelper.Action("Index", "CaseMovement", new { item.CaseId })
                };

                result.Add(movementFolder);
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Хронология: данни за плащания
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetPaymentCase_FolderItem(int caseId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var paymentCases = await moneyService.PaymentForCase_Select(caseId).ToListAsync();

            foreach (var item in paymentCases)
            {
                var paymentFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.Payment,
                    SourceId = item.Id.ToString(),
                    Title = "Име: " + item.PersonNames +
                            " / Тип: " + item.MoneyTypeNames +
                            " / Сума дело: " + item.AmountForCase +
                            " / Сума плащане: " + item.AmountForPayment +
                            " / Вид плащане: " + item.PaymentTypeName +
                            " / Дата: " + item.PaidDate.ToString("dd.MM.yyyy"),
                    Date = item.PaidDate,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.Payment),
                    ElementUrl = string.Empty
                };

                result.Add(paymentFolder);
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за изпълнителни листове
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetExecListCase_FolderItem(int caseId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var execLists = await moneyService.ExecListForCase_Select(caseId).ToListAsync();

            foreach (var item in execLists)
            {
                var execListFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.ExecList,
                    SourceId = item.Id.ToString(),
                    Title = "Тип: " + item.ExecListTypeName +
                            " / Лица: " + item.FullName +
                            " / Лица в чиято полза е сумата: " + item.FullNameReceive +
                            " / Сума: " + item.Amount.ToString("0.00") +
                            " / Номер: " + (item.RegDate != null ? (item.RegNumber + "/" + item.RegDate.DateToStr(FormattingConstant.NormalDateFormat)) : "В проект"),
                    Date = item.RegDate,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.ExecList),
                    ElementUrl = urlHelper.Action("EditExecList", "Money", new { item.Id })
                };

                result.Add(execListFolder);
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за изходящи документи
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetDocumentTemplate_FolderItem(int caseId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            //var documentTemplates = documentTemplateService.DocumentTemplate_Select(SourceTypeSelectVM.Case, caseId);

            var caseOutDocs = await repo.AllReadonly<DocumentCaseInfo>()
                                        .Where(x => x.CaseId == caseId &&
                                                    x.Document.DocumentDirectionId == DocumentConstants.DocumentDirection.OutGoing &&
                                                    x.Document.DateExpired == null)
                                        .Select(x => new DocumentInfoVM
                                        {
                                            Id = x.Document.Id,
                                            Title = $"Изх.№ {x.Document.DocumentNumber}/{x.Document.DocumentDate:dd.MM.yyyy} {x.Document.DocumentType.Label}",
                                            DirectionId = x.Document.DocumentDirectionId,
                                            IsSecret = (x.Document.IsSecret ?? false),
                                            IsRestriction = x.Document.IsRestictedAccess,
                                            DocumentDate = x.Document.DocumentDate
                                        })
                                        .ToListAsync();

            var caseOutDocsFromTemplate = await repo.AllReadonly<DocumentTemplate>()
                                                    .Where(x => x.CaseId == caseId && x.DocumentId > 0)
                                                    .Select(x => new DocumentInfoVM
                                                    {
                                                        Id = x.Document.Id,
                                                        Title = $"Изх.№ {x.Document.DocumentNumber}/{x.Document.DocumentDate:dd.MM.yyyy} {x.Document.DocumentType.Label}",
                                                        DirectionId = x.Document.DocumentDirectionId,
                                                        IsSecret = (x.Document.IsSecret ?? false),
                                                        IsRestriction = x.Document.IsRestictedAccess,
                                                        DocumentDate = x.Document.DocumentDate
                                                    })
                                                    .ToListAsync();

            if (caseOutDocs != null)
            {
                foreach (var documentInfoVM in caseOutDocs)
                {
                    if (!caseOutDocsFromTemplate.Any(x => x.Id == documentInfoVM.Id))
                    {
                        caseOutDocsFromTemplate.Add(documentInfoVM);
                    }
                }
            }

            foreach (var item in caseOutDocsFromTemplate)
            {
                var documentFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.DocumentTemplate,
                    SourceId = item.Id.ToString(),
                    Title = item.Title,
                    Date = item.DocumentDate,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.DocumentTemplate),
                    ElementUrl = urlHelper.Action("View", "Document", new { id = item.Id })
                };

                result.Add(documentFolder);
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за интервали
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetLifecycle_FolderItem(int caseId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseLifecycles = await caseLifecycleService.CaseLifecycle_Select(caseId).ToListAsync();

            foreach (var item in caseLifecycles)
            {
                var title = item.LifecycleTypeLabel + " от " + item.DateFrom.ToString("dd.MM.yyyy HH:mm") + (item.DateTo != null ? " до " + item.DateTo?.ToString("dd.MM.yyyy HH:mm") : string.Empty);
                var documentFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.CaseLifecycle,
                    SourceId = item.Id.ToString(),
                    Title = title,
                    Date = item.DateFrom,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseLifecycle),
                    ElementUrl = urlHelper.Action("Index", "CaseLifecycle", new { id = caseId })
                };

                result.Add(documentFolder);
            }

            return result;
        }

        /// <summary>
        /// Данни за протоколи от случайното разпределение
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetSelectionProtokol_FolderItem(int caseId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseSelectionProtokols = await caseSelectionProtokolService.CaseSelectionProtokol_Select(caseId).ToListAsync();

            foreach (var item in caseSelectionProtokols)
            {
                var documentFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.CaseSelectionProtokol,
                    SourceId = item.Id.ToString(),
                    Title = $"{item.SelectionModeName} избран: {item.SelectedLawUnitName} ({item.JudgeRoleName})",
                    Date = item.SelectionDate,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseSelectionProtokol),
                    ElementUrl = urlHelper.Action("Index", "CaseSelectionProtokol", new { id = caseId })
                };

                result.Add(documentFolder);
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за съдебен състав
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetLawUnit_FolderItem(int caseId, int? caseSessionId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseLawUnits = await caseLawUnitService.CaseLawUnit_Select(caseId, caseSessionId).ToListAsync();

            foreach (var item in caseLawUnits)
            {
                var title = item.LawUnitName + " " + item.JudgeRoleLabel + " от " + item.DateFrom.ToString("dd.MM.yyyy HH:mm") + ((item.DateTo != null) ? " до " + item.DateTo?.ToString("dd.MM.yyyy HH:mm") : string.Empty);
                if (caseSessionId != 0)
                    title = item.CaseSessionLabel + " - " + title;

                var documentFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.CaseLawUnit,
                    SourceId = item.Id.ToString(),
                    Title = title,
                    Date = item.DateFrom,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseLawUnit),
                    ElementUrl = urlHelper.Action("CasePreview", "Case", new { id = item.CaseId }) + "#tabLawUnit"
                };

                result.Add(documentFolder);
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за лица по дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetPerson_FolderItem(int caseId, int? caseSessionId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var casePersonLists = await casePersonService.CasePersonFast_SelectForCasePreview(caseId, caseSessionId).ToListAsync();

            foreach (var item in casePersonLists)
            {
                var title = item.FullName + " " + item.PersonRoleLabel + " от " + item.DateFrom.ToString("dd.MM.yyyy HH:mm") + ((item.DateTo != null) ? " до " + item.DateTo?.ToString("dd.MM.yyyy HH:mm") : string.Empty);
                if (caseSessionId != 0)
                    title = item.CaseSessionLabel + " - " + title;

                var documentFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.CasePerson,
                    SourceId = item.Id.ToString(),
                    Title = title,
                    Date = item.DateFrom,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CasePerson),
                    ElementUrl = urlHelper.Action("CasePreview", "Case", new { id = item.CaseId }) + "#tabPerson"
                };

                result.Add(documentFolder);
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за отводи
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetLawUnitDismisal_FolderItem(int caseId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseLawUnitDismisals = await caseLawUnitService.CaseLawUnitDismisal_Select(caseId).ToListAsync();

            foreach (var item in caseLawUnitDismisals)
            {
                var documentFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.CaseLawUnitDismisal,
                    SourceId = item.Id.ToString(),
                    Title = $"{item.DismisalTypeLabel} {item.CaseLawUnitName} ({item.CaseLawUnitRole})",
                    Date = item.DismisalDate,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseLawUnitDismisal),
                    ElementUrl = urlHelper.Action("IndexDismisal", "CaseLawUnit", new { id = caseId })
                };

                result.Add(documentFolder);
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за доказателства
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetEvidence_FolderItem(int caseId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseEvidences = await caseEvidenceService.CaseEvidence_Select(caseId, null, null, string.Empty, string.Empty, 0).ToListAsync();

            foreach (var item in caseEvidences)
            {
                var documentFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.CaseEvidence,
                    SourceId = item.Id.ToString(),
                    Title = $"{item.EvidenceTypeLabel} {item.Description} {item.EvidenceStateLabel}",
                    Date = item.DateWrt,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseEvidence),
                    ElementUrl = urlHelper.Action("CasePreview", "Case", new { id = item.CaseId }) + "#tabEvidence"
                };

                result.Add(documentFolder);
                result.AddRange(await GetEvidenceMovment_FolderItem(item.Id));
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за движение на доказателство
        /// </summary>
        /// <param name="evidenceId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetEvidenceMovment_FolderItem(int evidenceId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseEvidenceMovements = await caseEvidenceService.CaseEvidenceMovement_Select(evidenceId).ToListAsync();

            foreach (var item in caseEvidenceMovements)
            {
                var documentFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.CaseEvidenceMovement,
                    SourceId = item.Id.ToString(),
                    Title = $"{item.CaseEvidenceLabel} {item.EvidenceMovementTypeLabel}",
                    Date = item.MovementDate,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseEvidenceMovement)
                };

                result.Add(documentFolder);
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за уведомления
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <param name="caseSessionActId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetNotification_FolderItem(int caseId, int? caseSessionId, int? caseSessionActId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseNotifications = await caseNotificationService.CaseNotification_Select(caseId, caseSessionId, caseSessionActId)
                                                                 .ToListAsync();

            foreach (var item in caseNotifications)
            {
                var documentFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.CaseNotification,
                    SourceId = item.Id.ToString(),
                    Title = (caseSessionActId != null) ? $"По акт: {item.NotificationTypeLabel} {item.NotificationNumber} {item.CasePersonName}" : ((caseSessionId != null) ? $"По заседание: {item.NotificationTypeLabel} {item.NotificationNumber} {item.CasePersonName}" : $"По дело: {item.NotificationTypeLabel} {item.NotificationNumber} {item.CasePersonName}"),
                    Date = item.RegDate,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseNotification),
                    ElementUrl = (caseSessionActId != null) ? urlHelper.Action("Index", "CaseNotification", new { id = caseId, caseSessionId, caseSessionActId }) :
                                                              ((caseSessionId != null) ? urlHelper.Action("Preview", "CaseSession", new { id = caseSessionId }) + "#tabNotification" :
                                                                                         urlHelper.Action("CasePreview", "Case", new { id = caseId }) + "#tabNotification")
                };

                result.Add(documentFolder);
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за индикатори
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetClassification_FolderItem(int caseId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseClassifications = await caseClassificationService.CaseClassification_SelectObject(caseId, null);

            foreach (var item in caseClassifications)
            {
                var documentFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.CaseClassification,
                    SourceId = item.Id.ToString(),
                    Title = $"{item.Classification.Label}",
                    Date = item.DateFrom,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseClassification),
                    ElementUrl = urlHelper.Action("CasePreview", "Case", new { id = item.CaseId }) + "#tabMainData"
                };

                result.Add(documentFolder);
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за  плащания/задължения
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetMoney_FolderItem(int caseId, int? caseSessionId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseMoney = await caseMoneyService.CaseMoney_Select(caseId, caseSessionId).ToListAsync();

            foreach (var item in caseMoney)
            {
                var documentFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.CaseMoney,
                    SourceId = item.Id.ToString(),
                    Title = (caseSessionId != null) ? $"По заседание: {item.MoneyTypeName} {item.MoneySignName} {item.CaseLawUnitName} {item.Amount.ToString("0.00")}" : $"По дело: {item.MoneyTypeName} {item.MoneySignName} {item.CaseLawUnitName} {item.Amount.ToString("0.00")}",
                    Date = item.PaidDate,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseMoney),
                    ElementUrl = (caseSessionId == null) ? urlHelper.Action("CasePreview", "Case", new { id = item.CaseId }) + "#tabMoney" : urlHelper.Action("Preview", "CaseSession", new { id = item.CaseSessionId }) + "#tabMoney"
                };

                result.Add(documentFolder);
            }

            return result;
        }

        private List<CaseFolderItemVM> GetSessionResult_FolderItem(int caseSessionId, string sessionName)
        {
            // Да се оправи датата
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseSessionResults = caseSessionService.CaseSessionResult_Select(caseSessionId).ToList();

            foreach (var item in caseSessionResults)
            {
                var documentFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.CaseSessionResult,
                    SourceId = item.Id.ToString(),
                    Title = $"{sessionName} {item.SessionResultLabel} {item.SessionResultBaseLabel}",
                    Date = DateTime.Now,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseSessionResult)
                };

                result.Add(documentFolder);
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за сесии
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <param name="sessionName"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetSessionMeeting_FolderItem(int caseSessionId, string sessionName)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseSessionMeetings = await caseSessionMeetingService.CaseSessionMeeting_Select(caseSessionId).ToListAsync();

            foreach (var item in caseSessionMeetings)
            {
                var documentFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.CaseSessionMeeting,
                    SourceId = item.Id.ToString(),
                    Title = $"{sessionName} {item.SessionMeetingTypeLabel} от {item.DateFrom.ToString("dd.MM.yyyy HH:mm")} до {item.DateTo.ToString("dd.MM.yyyy HH:mm")}",
                    Date = item.DateFrom,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseSessionMeeting),
                    ElementUrl = urlHelper.Action("Preview", "CaseSession", new { id = caseSessionId }) + "#tabMainData"
                };

                result.Add(documentFolder);
                result.AddRange(await GetSessionMeetingUser_FolderItem(item.Id, documentFolder.Title));
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за Съпровождащ документ представен в заседание
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <param name="sessionName"></param>
        /// <param name="dateSession"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetCaseSessionFastDocument_FolderItem(int caseSessionId, string sessionName, DateTime dateSession)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseSessionFastDocuments = await caseSessionFastDocumentService.CaseSessionFastDocument_Select(caseSessionId).ToListAsync();

            foreach (var item in caseSessionFastDocuments)
            {
                var documentFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.CaseSessionFastDocument,
                    SourceId = item.Id.ToString(),
                    Title = $"{sessionName} свързано лице: {item.CasePersonName} вид: {item.SessionDocTypeLabel} статус: {item.SessionDocStateLabel}",
                    Date = dateSession,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseSessionFastDocument),
                    ElementUrl = urlHelper.Action("Index", "CaseSessionFastDocument", new { CaseSessionId = caseSessionId })
                };

                result.Add(documentFolder);
                result.AddRange(await GetSessionMeetingUser_FolderItem(item.Id, documentFolder.Title));
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за секретар към сесия
        /// </summary>
        /// <param name="caseSessionMeetingId"></param>
        /// <param name="meetingName"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetSessionMeetingUser_FolderItem(int caseSessionMeetingId, string meetingName)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseSessionMeetings = await caseSessionMeetingService.CaseSessionMeetingUser_Select(caseSessionMeetingId).ToListAsync();

            foreach (var item in caseSessionMeetings)
            {
                var documentFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.CaseSessionMeetingUser,
                    SourceId = item.Id.ToString(),
                    Title = $"{meetingName} {item.SecretaryUserName}",
                    Date = item.DateWrt,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseSessionMeetingUser)
                };

                result.Add(documentFolder);
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за документи към заседание
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <param name="sessionName"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetSessionDoc_FolderItem(int caseSessionId, string sessionName)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseSessionDocs = await caseSessionDocService.CaseSessionDoc_Select(caseSessionId).ToListAsync();

            foreach (var item in caseSessionDocs)
            {
                var documentFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.Document,
                    SourceId = item.Id.ToString(),
                    Title = $"{sessionName} {item.DocumentLabel} {item.SessionDocStateLabel}",
                    Date = item.DateFrom,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.Document),
                    ElementUrl = urlHelper.Action("Preview", "CaseSession", new { id = caseSessionId }) + "#tabSessionDoc"
                };

                result.Add(documentFolder);
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за актове
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <param name="sessionName"></param>
        /// <returns></returns>
        private async Task<List<CaseFolderItemVM>> GetSessionAct_FolderItem(int caseSessionId, string sessionName)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseSessionActs = await caseSessionActService.CaseSessionAct_Select(caseSessionId, null, null, null, null, null).ToListAsync();

            foreach (var item in caseSessionActs.Where(x => x.ActDeclaredDate != null))
            {
                var documentFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.CaseSessionAct,
                    SourceId = item.Id.ToString(),
                    Title = $"{sessionName} {item.ActTypeLabel} {item.ActStateLabel} номер: {item.RegNumber}",
                    Description = !string.IsNullOrEmpty(item.Description) ? " Диспозитив: " + item.Description.Substring(0, ((item.Description.Length - 1) <= 500 ? item.Description.Length : 500)) : string.Empty,
                    Date = item.RegDate ?? item.DateWrt,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseSessionAct),
                    ElementUrl = urlHelper.Action("Preview", "CaseSession", new { id = caseSessionId }) + "#tabSessionAct"
                };

                result.Add(documentFolder);

                if (item.ActTypeId == NomenclatureConstants.ActType.Sentence || item.ActTypeId == NomenclatureConstants.ActType.Answer)
                {
                    if (item.ActMotivesDeclaredDate != null)
                    {
                        var documentFolderMotive = new CaseFolderItemVM()
                        {
                            SourceType = SourceTypeSelectVM.CaseSessionAct,
                            SourceId = item.Id.ToString(),
                            Title = $"Постановен мотив към акт: {sessionName} {item.ActTypeLabel} {item.ActStateLabel} номер: {item.RegNumber} ",
                            Description = !string.IsNullOrEmpty(item.Description) ? " Диспозитив: " + item.Description.Substring(0, ((item.Description.Length - 1) <= 500 ? item.Description.Length : 500)) : string.Empty,
                            Date = item.ActMotivesDeclaredDate ?? item.DateWrt,
                            TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseSessionAct),
                            ElementUrl = urlHelper.Action("Preview", "CaseSession", new { id = caseSessionId }) + "#tabSessionAct"
                        };

                        result.Add(documentFolderMotive);
                    }
                }

                result.AddRange(await repo.AllReadonly<CaseSessionActCoordination>()
                                          .Where(x => x.CaseSessionActId == item.Id &&
                                                      x.CoordinationDeclaredDate != null &&
                                                      NomenclatureConstants.ActCoordinationTypes.WithOpinion.Contains(x.ActCoordinationTypeId))
                                          .Select(x => new CaseFolderItemVM()
                                          {
                                              SourceType = SourceTypeSelectVM.CaseSessionAct,
                                              SourceId = x.CaseSessionActId.ToString(),
                                              Title = $"Постановено особенно мнение към акт: {sessionName} {item.ActTypeLabel} {item.ActStateLabel} номер: {item.RegNumber} на: {x.CaseLawUnit.LawUnit.FullName}",
                                              Description = string.Empty,
                                              Date = x.CoordinationDeclaredDate ?? item.DateWrt,
                                              TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseSessionAct),
                                              ElementUrl = urlHelper.Action("Preview", "CaseSession", new { id = caseSessionId }) + "#tabSessionAct"
                                          })
                                          .ToListAsync());
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за всичко в едно дело за хронология в ел. папка
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IQueryable<CaseFolderItemVM>> Case_SelectFolder(int id)
        {
            var result = new List<CaseFolderItemVM>
            {
                await GetCase_FolderItem(id)
            };

            var caseFolder = await GetCaseInforceDate_FolderItem(id);
            if (caseFolder != null)
                result.Add(caseFolder);

            result.Add(await GetCaseDocument_FolderItem(id));
            result.AddRange(await GetCaseSession_FolderItem(id));
            result.AddRange(await GetCaseInDocs_FolderItem(id));
            result.AddRange(await GetCaseMigration_FolderItem(id));
            result.AddRange(await GetCaseCrime_FolderItem(id));
            result.AddRange(await GetCasePersonCrime_FolderItem(id));
            result.AddRange(await GetCasePersonSentence_FolderItem(id));
            result.AddRange(await GetCasePersonSentencePunishment_FolderItem(id));
            result.AddRange(await GetCasePersonSentencePunishmentCrime_FolderItem(id));
            result.AddRange(await GetCasePersonSentenceBulletin_FolderItem(id));
            result.AddRange(await GetCaseLifecycle_FolderItem(id));
            result.AddRange(await GetCaseSessionActDivorce_FolderItem(id));
            result.AddRange(await GetCasePersonInheritance_FolderItem(id));
            //result.AddRange(GetExecList_FolderItem(id));
            result.AddRange(await GetCaseMovement_FolderItem(id));
            result.AddRange(await GetDocumentTemplate_FolderItem(id));
            result.AddRange(await GetLifecycle_FolderItem(id));
            result.AddRange(await GetSelectionProtokol_FolderItem(id));
            result.AddRange(await GetLawUnit_FolderItem(id, null));
            result.AddRange(await GetPerson_FolderItem(id, 0));
            result.AddRange(await GetLawUnitDismisal_FolderItem(id));
            result.AddRange(await GetEvidence_FolderItem(id));
            result.AddRange(await GetNotification_FolderItem(id, null, null));
            result.AddRange(await GetClassification_FolderItem(id));
            result.AddRange(await GetMoney_FolderItem(id, null));
            result.AddRange(await GetPaymentCase_FolderItem(id));
            result.AddRange(await GetExecListCase_FolderItem(id));
            result.AddRange(await GetWorkTask_FolderItem(id));

            return result.OrderByDescending(x => x.Date).AsQueryable();
        }

        /// <summary>
        /// Извличане на данни за дела в съд
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="caseId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<LabelValueVM> GetCasesByCourt(int courtId, int? caseId, string query)
        {
            Expression<Func<Case, bool>> filter = x => true;
            if (caseId > 0)
            {
                filter = x => x.Id == caseId;
            }
            else
            {
                filter = x => (x.CourtId == courtId) && (x.CaseStateId >= NomenclatureConstants.CaseState.New)
                && EF.Functions.ILike(x.RegNumber, query.ToCasePaternSearch());
            }
            return repo.AllReadonly<Case>()
                            .Where(x => x.RegNumber != null)
                            .Where(filter)
                            .Select(x => new
                            {
                                x.Id,
                                x.RegDate.Year,
                                x.RegNumber,
                                x.CaseGroupId,
                                x.CaseGroup.Code
                            })
                            .OrderByDescending(x => x.Year)
                            .ThenBy(x => x.CaseGroupId)
                            .Select(x => new LabelValueVM()
                            {
                                Value = x.Id.ToString(),
                                Label = $"{x.RegNumber} ({x.Code})"
                            });
        }

        /// <summary>
        /// Данни за актове по дело за комбо
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="declaredOnly"></param>
        /// <returns></returns>
        public IEnumerable<SelectListItem> GetDDL_SessionActsByCase(int caseId, bool addDefaultElement = false, bool notRegistered = false, bool declaredOnly = true)
        {
            Expression<Func<CaseSessionAct, bool>> filter = x => x.ActDeclaredDate != null;
            if (!declaredOnly)
            {
                if (notRegistered)
                {
                    //При обективна невъзможност акта не е регистриран
                    filter = x => true;
                }
                else
                {
                    //Поне да е регистриран, ако не постановен
                    filter = x => x.RegDate != null;
                }
            }
            var result = repo.AllReadonly<CaseSessionAct>()
                             .Where(x => x.CaseSession.CaseId == caseId)
                             .Where(filter)
                             .Where(FilterExpireInfo<CaseSessionAct>(false))
                             .OrderByDescending(x => x.RegDate)
                             .Select(x => new
                             {
                                 x.Id,
                                 ActType = x.ActType.Label,
                                 x.RegDate,
                                 x.RegNumber,
                                 SessionType = x.CaseSession.SessionType.Label,
                                 SessionDate = x.CaseSession.DateFrom
                             })
                             .ToList()
                             .Select(x => new SelectListItem()
                             {
                                 Value = x.Id.ToString(),
                                 Text = (x.RegNumber == null) ? $"{x.ActType} ({x.SessionType} {x.SessionDate:dd.MM.yyyy})" : $"{x.ActType} {x.RegNumber}/{x.RegDate:dd.MM.yyyy} ({x.SessionType} {x.SessionDate:dd.MM.yyyy})"
                             }).ToList();
            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }
            return result;
        }

        public async Task<CaseProceedingsVM> CaseProceedings_Select(int CaseId)
        {
            CaseElectronicFolderVM caseElectronicFolderVM = await CaseElectronicFolder_Select(CaseId);

            var caseOutMigrations = caseMigrationService.SelectOutMove(CaseId);
            var lastCaseMigration = await caseOutMigrations.OrderByDescending(x => x.Id).FirstOrDefaultAsync();

            var caseSessionActComplainResults = await repo.AllReadonly<CaseSessionActComplainResult>()
                                                    .Include(x => x.ActResult)
                                                    .Include(x => x.CaseSessionActComplain)
                                                    .ThenInclude(x => x.ComplainDocument)
                                                    .ThenInclude(x => x.DocumentType)
                                                    .Where(x => x.CaseId == CaseId)
                                                    .ToListAsync();

            List<CaseProceedingsObjectsVM> objectsVMs = new List<CaseProceedingsObjectsVM>();
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromDocumentResolution(caseElectronicFolderVM.DocumentResolutions.ToList()));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromDocumentResolution(caseElectronicFolderVM.DocumentResolutionCase.ToList()));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromDocumentLink(caseElectronicFolderVM.DocumentLinks.ToList(), caseElectronicFolderVM.DocumentDate, caseElectronicFolderVM.DocumentLabel));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromSessions(caseElectronicFolderVM));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromCaseInDocuments(caseElectronicFolderVM));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromOutMigration(await caseOutMigrations.ToListAsync()));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromCaseSessionDocs(caseElectronicFolderVM));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromSessionFastDocuments(caseElectronicFolderVM));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromCaseOutDocuments(caseElectronicFolderVM));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromWorkTask(caseElectronicFolderVM));
            objectsVMs.AddRange(FillCaseSessionActComplainResult(caseSessionActComplainResults));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromMigration(CaseId));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromMoneyDocument(caseElectronicFolderVM.MoneyDocument.ToList()));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromMoneyDocument(caseElectronicFolderVM.Evidences.ToList()));

            CaseProceedingsVM result = new CaseProceedingsVM()
            {
                Id = caseElectronicFolderVM.Id,
                RegNumber = caseElectronicFolderVM.RegNumber,
                RegDate = caseElectronicFolderVM.RegDate,
                RegNumberText = caseElectronicFolderVM.RegNumberText,
                CaseGroupLabel = caseElectronicFolderVM.CaseGroupLabel,
                CaseTypeLabel = caseElectronicFolderVM.CaseTypeLabel,
                CaseCodeLabel = caseElectronicFolderVM.CaseCodeLabel,
                CaseStateLabel = caseElectronicFolderVM.CaseStateLabel,
                CaseReasonLabel = caseElectronicFolderVM.CaseReasonLabel,
                CaseStateDescription = caseElectronicFolderVM.CaseStateDescription,
                ArchRegNumber = caseElectronicFolderVM.ArchRegNumber,
                ArchRegDate = caseElectronicFolderVM.ArchRegDate,
                JudgeRapporteur = caseElectronicFolderVM.JudgeRapporteur,
                DocumentLabel = caseElectronicFolderVM.DocumentLabel,
                DocumentDescription = caseElectronicFolderVM.DocumentDescription,
                LastMigration = (lastCaseMigration != null) ? ((!lastCaseMigration.IsReturned ?? true) ? (lastCaseMigration.SentToName + " с " + lastCaseMigration.OutDocumentLabel) : string.Empty) : string.Empty,
                CaseInforcedDate = caseElectronicFolderVM.CaseInforcedDate,
                CaseClassifications = caseElectronicFolderVM.CaseClassifications,
                CaseMigrations = caseElectronicFolderVM.CaseMigrations,
                DocumentCaseInfos = caseElectronicFolderVM.DocumentCaseInfos,
                DocumentInstitutionCaseInfos = caseElectronicFolderVM.DocumentInstitutionCaseInfos,
                CasePersons = caseElectronicFolderVM.CasePersons,
                CaseLawUnits = caseElectronicFolderVM.CaseLawUnits,
                CaseProceedingsObjects = objectsVMs
            };

            return result;
        }

        private List<CaseProceedingsObjectsVM> FillCaseProceedingsObjectsFromSessions(CaseElectronicFolderVM caseElectronicFolderVM)
        {
            bool isSpecialAccess = caseElectronicFolderVM.CaseClassifications.Any(c => c.ClassificationId == NomenclatureConstants.CaseClassifications.SpecialAccess);
            List<CaseProceedingsObjectsVM> result = new List<CaseProceedingsObjectsVM>();
            foreach (var caseSession in caseElectronicFolderVM.CaseSessions)
            {
                CaseProceedingsObjectsVM element = new CaseProceedingsObjectsVM()
                {
                    Id = caseSession.Id,
                    ParentId = null,
                    Date = caseSession.DateFrom,
                    Label = caseSession.SessionTypeLabel + " " + caseSession.DateFrom.ToString("dd.MM.yyyy HH:mm") + (!string.IsNullOrEmpty(caseSession.SessionStateLabel) ? " статус: " + caseSession.SessionStateLabel : string.Empty) + (!string.IsNullOrEmpty(caseSession.SessionResultText) ? " Резултат/и: " + caseSession.SessionResultText : string.Empty)
                };
                result.Add(element);

                foreach (var act in caseSession.CaseSessionActs.Where(x => x.RegDate != null))
                {
                    CaseProceedingsObjectsVM actElement = new CaseProceedingsObjectsVM()
                    {
                        Id = act.Id,
                        ParentId = caseSession.Id,
                        Date = act.RegDate ?? DateTime.Now,
                        Label = act.ActTypeLabel + " " + act.RegNumber + "/" + (act.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"),
                        Description = (isSpecialAccess) ? "***" : act.Description
                    };
                    result.Add(actElement);

                    if (act.ActTypeId == NomenclatureConstants.ActType.Sentence || act.ActTypeId == NomenclatureConstants.ActType.Answer)
                    {
                        if (act.ActDeclaredDate != null && act.ActMotivesDeclaredDate != null)
                        {
                            CaseProceedingsObjectsVM actElementMotive = new CaseProceedingsObjectsVM()
                            {
                                Id = act.Id,
                                ParentId = caseSession.Id,
                                Date = act.ActMotivesDeclaredDate ?? DateTime.Now,
                                Label = "Постановен мотив към акт: " + act.ActTypeLabel + " " + act.RegNumber + "/" + (act.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") + " Дата на обявяване: " + (act.ActMotivesDeclaredDate ?? DateTime.Now).ToString("dd.MM.yyyy")
                            };
                            result.Add(actElementMotive);
                        }
                    }

                    result.AddRange(repo.AllReadonly<CaseSessionActCoordination>()
                                    .Where(x => x.CaseSessionActId == act.Id &&
                                                x.CoordinationDeclaredDate != null &&
                                                NomenclatureConstants.ActCoordinationTypes.WithOpinion.Contains(x.ActCoordinationTypeId))
                                    .Select(x => new CaseProceedingsObjectsVM()
                                    {
                                        Id = act.Id,
                                        ParentId = caseSession.Id,
                                        Date = act.ActMotivesDeclaredDate ?? DateTime.Now,
                                        Label = "Постановенo особенно мнение към акт: " + act.ActTypeLabel + " " + act.RegNumber + "/" + (act.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") + " Дата на обявяване: " + (act.ActMotivesDeclaredDate ?? DateTime.Now).ToString("dd.MM.yyyy") + " на: " + x.CaseLawUnit.LawUnit.FullName
                                    }));
                }
            }

            return result;
        }

        private List<CaseProceedingsObjectsVM> FillCaseProceedingsObjectsFromCaseInDocuments(CaseElectronicFolderVM caseElectronicFolderVM)
        {
            List<CaseProceedingsObjectsVM> result = new List<CaseProceedingsObjectsVM>();

            foreach (var doc in caseElectronicFolderVM.CaseInDocuments.Where(x => x.DocumentDate != null))
            {
                CaseProceedingsObjectsVM element = new CaseProceedingsObjectsVM()
                {
                    LongId = doc.Id,
                    ParentId = null,
                    Date = doc.DocumentDate ?? DateTime.Now,
                    Label = doc.Title + " " + (doc.DocumentDate ?? DateTime.Now).ToString("dd.MM.yyyy") +
                                              (!string.IsNullOrEmpty(doc.DocumentTemplateLabel) ? " Бланка: " + doc.DocumentTemplateLabel : string.Empty) +
                                              (!string.IsNullOrEmpty(doc.DocumentPersonLabel) ? " Вносител: " + doc.DocumentPersonLabel : string.Empty),
                    Description = !string.IsNullOrEmpty(doc.Description) ? " - " + doc.Description : string.Empty
                };
                if (caseElectronicFolderVM.IsSpecialAccess)
                {
                    element.Label = doc.Title + " " + (doc.DocumentDate ?? DateTime.Now).ToString("dd.MM.yyyy") +
                                              (!string.IsNullOrEmpty(doc.DocumentTemplateLabel) ? " Бланка: " + doc.DocumentTemplateLabel : string.Empty);
                }
                result.Add(element);

                foreach (var documentResolution in doc.DocumentResolutions)
                {
                    CaseProceedingsObjectsVM caseProceedingsObjects = new CaseProceedingsObjectsVM()
                    {
                        LongId = documentResolution.Id,
                        ParentId = null,
                        Date = documentResolution.RegDate ?? DateTime.Now,
                        Label = documentResolution.Label
                    };
                    result.Add(caseProceedingsObjects);
                }

                foreach (var workTaskReportVM in doc.WorkTasks)
                {
                    CaseProceedingsObjectsVM elementTask = new CaseProceedingsObjectsVM()
                    {
                        LongId = workTaskReportVM.Id,
                        ParentId = null,
                        Date = workTaskReportVM.DateCreated,
                        Label = "Задача към съпровождащ документ: " + doc.Title + " " + (doc.DocumentDate ?? DateTime.Now).ToString("dd.MM.yyyy") + " задача: " + workTaskReportVM.TaskTypeName +
                                " - изпратена на: " + workTaskReportVM.DateCreated.ToString("dd.MM.yyyy") +
                                (workTaskReportVM.DateCompleted != null ? " дата на приключване: " + (workTaskReportVM.DateCompleted ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty) +
                                (!string.IsNullOrEmpty(workTaskReportVM.Description) ? " забележка: " + workTaskReportVM.Description : string.Empty) +
                                (!string.IsNullOrEmpty(workTaskReportVM.DescriptionCreated) ? " описание: " + workTaskReportVM.DescriptionCreated : string.Empty)
                    };
                    result.Add(elementTask);
                }
            }

            return result;
        }

        private List<CaseProceedingsObjectsVM> FillCaseProceedingsObjectsFromCaseOutDocuments(CaseElectronicFolderVM caseElectronicFolderVM)
        {
            List<CaseProceedingsObjectsVM> result = new List<CaseProceedingsObjectsVM>();

            foreach (var documentInfo in caseElectronicFolderVM.CaseOutDocuments.OrderBy(x => x.DocumentDate))
            {
                CaseProceedingsObjectsVM element = new CaseProceedingsObjectsVM()
                {
                    LongId = documentInfo.Id,
                    ParentId = null,
                    Date = documentInfo.DocumentDate ?? caseElectronicFolderVM.RegDate,
                    Label = documentInfo.Title + (!string.IsNullOrEmpty(documentInfo.DocumentTemplateLabel) ? " Бланка: " + documentInfo.DocumentTemplateLabel : string.Empty) +
                                                 (!string.IsNullOrEmpty(documentInfo.DocumentPersonLabel) ? " Адресат: " + documentInfo.DocumentPersonLabel : string.Empty),
                    Description = documentInfo.Description + " " + documentInfo.DocumentTemplateDescription
                };
                if (caseElectronicFolderVM.IsSpecialAccess)
                {
                    element.Label = documentInfo.Title + (!string.IsNullOrEmpty(documentInfo.DocumentTemplateLabel) ? " Бланка: " + documentInfo.DocumentTemplateLabel : string.Empty);
                }
                result.Add(element);

                foreach (var documentResolution in documentInfo.DocumentResolutions)
                {
                    CaseProceedingsObjectsVM caseProceedingsObjects = new CaseProceedingsObjectsVM()
                    {
                        LongId = documentResolution.Id,
                        ParentId = null,
                        Date = documentResolution.RegDate ?? DateTime.Now,
                        Label = documentResolution.Label
                    };
                    result.Add(caseProceedingsObjects);
                }
            }

            return result;
        }

        private List<CaseProceedingsObjectsVM> FillCaseProceedingsObjectsFromCaseSessionDocs(CaseElectronicFolderVM caseElectronicFolderVM)
        {
            List<CaseProceedingsObjectsVM> result = new List<CaseProceedingsObjectsVM>();

            foreach (var caseSession in caseElectronicFolderVM.CaseSessions)
            {
                foreach (var caseSessionDoc in caseSession.CaseSessionDocs)
                {
                    CaseProceedingsObjectsVM element = new CaseProceedingsObjectsVM()
                    {
                        LongId = caseSessionDoc.Id,
                        ParentId = null,
                        Date = caseSessionDoc.DateFrom,
                        Label = "Съпровождащ документ: " + caseSessionDoc.DocumentLabel + " - " + caseSessionDoc.SessionDocStateLabel + " (" + caseSession.SessionTypeLabel + " " + caseSession.DateFrom.ToString("dd.MM.yyyy") + ")"
                    };
                    result.Add(element);

                    if (caseSessionDoc.WorkTask != null)
                    {
                        foreach (var workTaskReportVM in caseSessionDoc.WorkTask)
                        {
                            CaseProceedingsObjectsVM elementTask = new CaseProceedingsObjectsVM()
                            {
                                LongId = workTaskReportVM.Id,
                                ParentId = null,
                                Date = workTaskReportVM.DateCreated,
                                Label = "Задача към съпровождащ документ: " + caseSessionDoc.DocumentLabel + " задача: " + workTaskReportVM.TaskTypeName +
                                        " - изпратена на: " + workTaskReportVM.DateCreated.ToString("dd.MM.yyyy") +
                                        (workTaskReportVM.DateCompleted != null ? " дата на приключване: " + (workTaskReportVM.DateCompleted ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty) +
                                        (!string.IsNullOrEmpty(workTaskReportVM.Description) ? " забележка: " + workTaskReportVM.Description : string.Empty) +
                                        (!string.IsNullOrEmpty(workTaskReportVM.DescriptionCreated) ? " описание: " + workTaskReportVM.DescriptionCreated : string.Empty)
                            };
                            result.Add(elementTask);
                        }
                    }
                }
            }

            return result;
        }

        private List<CaseProceedingsObjectsVM> FillCaseProceedingsObjectsFromWorkTask(CaseElectronicFolderVM caseElectronicFolderVM)
        {
            List<CaseProceedingsObjectsVM> result = new List<CaseProceedingsObjectsVM>();

            foreach (var workTaskReport in caseElectronicFolderVM.WorkTaskReports)
            {
                CaseProceedingsObjectsVM element = new CaseProceedingsObjectsVM()
                {
                    LongId = workTaskReport.Id,
                    ParentId = null,
                    Date = workTaskReport.DateCreated,
                    Label = "Задача: " + workTaskReport.TaskTypeName +
                            " - изпратена на: " + workTaskReport.DateCreated.ToString("dd.MM.yyyy") +
                            (workTaskReport.DateCompleted != null ? " дата на приключване: " + (workTaskReport.DateCompleted ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty) +
                            (!string.IsNullOrEmpty(workTaskReport.Description) ? " забележка: " + workTaskReport.Description : string.Empty) +
                            (!string.IsNullOrEmpty(workTaskReport.DescriptionCreated) ? " описание: " + workTaskReport.DescriptionCreated : string.Empty)
                };
                result.Add(element);
            }

            return result;
        }

        private List<CaseProceedingsObjectsVM> FillCaseProceedingsObjectsFromMigration(int caseId)
        {
            List<CaseProceedingsObjectsVM> result = new List<CaseProceedingsObjectsVM>();

            var caseMigrations = caseMigrationService.Select(caseId);
            return caseMigrations.Select(x => new CaseProceedingsObjectsVM()
            {
                Id = x.Id,
                ParentId = null,
                Date = x.DateWrt,
                Label = string.IsNullOrEmpty(x.CaseRegNumber) ? $"Дело номер {x.CaseStateName} Вид движение: {x.MigrationTypeName} Движение от-към: {x.SentFromName} - {x.SentToName} Дата: {x.DateWrt:dd.MM.yyyy}" : $"Дело номер {x.CaseRegNumber}/{x.CaseRegDate:dd.MM.yyyy} Вид движение: {x.MigrationTypeName} Движение от-към: {x.SentFromName} - {x.SentToName} Дата: {x.DateWrt:dd.MM.yyyy}",
                Description = x.Description
            }).ToList();
        }

        private List<CaseProceedingsObjectsVM> FillCaseSessionActComplainResult(List<CaseSessionActComplainResult> caseSessionActComplainResults)
        {
            List<CaseProceedingsObjectsVM> result = new List<CaseProceedingsObjectsVM>();

            foreach (var caseSessionActComplainResult in caseSessionActComplainResults)
            {
                CaseProceedingsObjectsVM element = new CaseProceedingsObjectsVM()
                {
                    Id = caseSessionActComplainResult.Id,
                    ParentId = null,
                    Date = caseSessionActComplainResult.DateResult ?? DateTime.Now,
                    Label = "Резултат от обжалване: " + caseSessionActComplainResult.ActResult.Label +
                            " - Дата на отразяване на резултат: " + (caseSessionActComplainResult.DateResult ?? DateTime.Now).ToString("dd.MM.yyyy") +
                            " към: " + caseSessionActComplainResult.CaseSessionActComplain.ComplainDocument.DocumentType.Label + " " + caseSessionActComplainResult.CaseSessionActComplain.ComplainDocument.DocumentNumber + "/" + caseSessionActComplainResult.CaseSessionActComplain.ComplainDocument.DocumentDate.ToString("dd.MM.yyyy") +
                            (!string.IsNullOrEmpty(caseSessionActComplainResult.Description) ? " Описание: " + caseSessionActComplainResult.Description : string.Empty)
                };
                result.Add(element);
            }

            return result;
        }

        private List<CaseProceedingsObjectsVM> FillCaseProceedingsObjectsFromSessionFastDocuments(CaseElectronicFolderVM caseElectronicFolderVM)
        {
            List<CaseProceedingsObjectsVM> result = new List<CaseProceedingsObjectsVM>();

            foreach (var caseSession in caseElectronicFolderVM.CaseSessions)
            {
                foreach (var caseSessionFastDocument in caseSession.SessionFastDocuments)
                {
                    CaseProceedingsObjectsVM element = new CaseProceedingsObjectsVM()
                    {
                        LongId = caseSessionFastDocument.Id,
                        ParentId = null,
                        Date = (caseSessionFastDocument.CaseSessionFastDocumentInitDateSession ?? caseSessionFastDocument.DateSession),
                        Label = "Съпровождащ документ представени в заседание: " + ((caseSessionFastDocument.CaseSessionFastDocumentInitDateSession ?? caseSessionFastDocument.DateSession).ToString("dd.MM.yyyy")) + " " +
                                                            " " + caseSessionFastDocument.SessionDocTypeLabel +
                                                            " " + caseSessionFastDocument.CasePersonName +
                                                            " - " + caseSessionFastDocument.SessionDocStateLabel +
                                                            " (" + caseSession.SessionTypeLabel + " " + caseSession.DateFrom.ToString("dd.MM.yyyy") + ")"
                    };
                    result.Add(element);
                }
            }

            return result;
        }

        private List<CaseProceedingsObjectsVM> FillCaseProceedingsObjectsFromOutMigration(List<CaseMigrationVM> caseMigrations)
        {
            List<CaseProceedingsObjectsVM> result = new List<CaseProceedingsObjectsVM>();

            foreach (var caseMigration in caseMigrations.Where(x => x.OutDocumentDate != null))
            {
                CaseProceedingsObjectsVM element = new CaseProceedingsObjectsVM()
                {
                    LongId = caseMigration.Id,
                    ParentId = null,
                    Date = caseMigration.OutDocumentDate ?? DateTime.Now,
                    Label = "Движение с изходящ документ: " + caseMigration.OutDocumentLabel + " изпращане до: " + caseMigration.SentToName,
                    Description = caseMigration.Description
                };
                result.Add(element);
            }

            return result;
        }

        private List<CaseProceedingsObjectsVM> FillCaseProceedingsObjectsFromDocumentResolution(List<DocumentResolutionListVM> documentResolutions)
        {
            List<CaseProceedingsObjectsVM> result = new List<CaseProceedingsObjectsVM>();

            foreach (var documentResolution in documentResolutions)
            {
                CaseProceedingsObjectsVM element = new CaseProceedingsObjectsVM()
                {
                    LongId = documentResolution.Id,
                    ParentId = null,
                    Date = documentResolution.RegDate ?? DateTime.Now,
                    Label = documentResolution.Label
                };
                result.Add(element);
            }

            return result;
        }

        /// <summary>
        /// Метод връщащ списък за ход на дело от съпровождащи документи към иницииращ документ
        /// </summary>
        /// <param name="documentLinks">Спсисък с документи</param>
        /// <param name="documentDate">Дата на иницииращ документ</param>
        /// <param name="documentLabel">Иницииращ документ</param>
        /// <returns></returns>
        private List<CaseProceedingsObjectsVM> FillCaseProceedingsObjectsFromDocumentLink(List<DocumentLinkEFVM> documentLinks, DateTime documentDate, string documentLabel)
        {
            List<CaseProceedingsObjectsVM> result = new List<CaseProceedingsObjectsVM>();

            foreach (var link in documentLinks)
            {
                CaseProceedingsObjectsVM element = new CaseProceedingsObjectsVM()
                {
                    LongId = link.DocumentId,
                    ParentId = null,
                    Date = documentDate,
                    Label = $"Свързан документ {link.PrevDocumentLabel} към иницииращ документ"
                };
                result.Add(element);
            }

            return result;
        }

        private List<CaseProceedingsObjectsVM> FillCaseProceedingsObjectsFromMoneyDocument(List<ObligationVM> obligationVMs)
        {
            List<CaseProceedingsObjectsVM> result = new List<CaseProceedingsObjectsVM>();

            foreach (var obligationVM in obligationVMs)
            {
                CaseProceedingsObjectsVM element = new CaseProceedingsObjectsVM()
                {
                    LongId = obligationVM.DocumentId ?? 0,
                    ParentId = null,
                    Date = obligationVM.ObligationDate,
                    Label = "Документ: " + obligationVM.DocumentTypeLabel +
                            " / задължение № " + obligationVM.ObligationNumber + "/" + obligationVM.ObligationDate.ToString("dd.MM.yyyy") +
                            " / Лице: " + obligationVM.CasePersonName +
                            " / Вид сума/задължение: " + obligationVM.MoneyTypeName +
                            " / Сума: " + obligationVM.Amount.ToString("0.00") +
                            " / Платено: " + obligationVM.AmountPay.ToString("0.00")
                };
                result.Add(element);
            }

            return result;
        }

        /// <summary>
        /// Метод връщащ списък с доказателства към дело за ход на дело
        /// </summary>
        /// <param name="caseEvidences">Списък с доказателства</param>
        /// <returns></returns>
        private List<CaseProceedingsObjectsVM> FillCaseProceedingsObjectsFromMoneyDocument(List<CaseEvidenceEFVM> caseEvidences)
        {
            List<CaseProceedingsObjectsVM> result = new List<CaseProceedingsObjectsVM>();

            foreach (var caseEvidence in caseEvidences)
            {
                CaseProceedingsObjectsVM element = new CaseProceedingsObjectsVM()
                {
                    LongId = caseEvidence.Id,
                    ParentId = null,
                    Date = caseEvidence.DateAccept,
                    Label = $"Доказателство към дело: {caseEvidence.EvidenceTypeLabel} {caseEvidence.Description}, дата на регистрация: {caseEvidence.DateAccept.ToString("dd.MM.yyyy")} {caseEvidence.EvidenceStateLabel}"
                };
                result.Add(element);
            }

            return result;
        }

        //public CaseElectronicFolderVM CaseElectronicFolder_Select(int CaseId)
        //{
        //    var caseSelectionProtokolListVMs = caseSelectionProtokolService.CaseSelectionProtokol_Select(CaseId).ToList();
        //    var caseSessionDocVMs = caseSessionDocService.CaseSessionDocByCaseId_Select(CaseId).ToList();

        //    foreach (var caseSessionDocVM in caseSessionDocVMs)
        //    {
        //        caseSessionDocVM.WorkTask = repo.AllReadonly<WorkTask>()
        //                                        .Where(x => x.SourceType == SourceTypeSelectVM.Document &&
        //                                                    x.SourceId == caseSessionDocVM.DocumentId)
        //                                        .Select(x => new WorkTaskReportVM()
        //                                        {
        //                                            Id = x.Id,
        //                                            TaskTypeId = x.TaskTypeId,
        //                                            TaskTypeName = x.TaskType.Label,
        //                                            DateCreated = x.DateCreated,
        //                                            DateCompleted = x.DateCompleted,
        //                                            DescriptionCreated = x.DescriptionCreated,
        //                                            Description = x.Description
        //                                        })
        //                                        .ToList();
        //    }

        //    var casePersonsQuery = repo.AllReadonly<CasePerson>()
        //                          .Where(x => x.CaseId == CaseId && x.CaseSessionId == null);

        //    //var casePersonLists = casePersonService.CasePerson_Select(CaseId, null, false, false, false).ToList();
        //    var casePersonLists = repo.AllReadonly<CasePerson>()
        //                              .Where(x => x.CaseId == CaseId &&
        //                                          x.DateExpired == null)
        //                              .Select(x => new CasePersonListVM()
        //                              {
        //                                  Id = x.Id,
        //                                  CaseId = x.CaseId,
        //                                  CaseSessionId = x.CaseSessionId,
        //                                  Uic = x.Uic,
        //                                  UicTypeId = x.UicTypeId,
        //                                  UicTypeLabel = (x.UicType != null) ? x.UicType.Label : string.Empty,
        //                                  FirstName = x.FirstName,
        //                                  MiddleName = x.MiddleName,
        //                                  FamilyName = x.FamilyName,
        //                                  Family2Name = x.Family2Name,
        //                                  FullName = x.FullName,
        //                                  RoleName = x.PersonRole.Label,
        //                                  PersonRoleId = x.PersonRole.Id,
        //                                  PersonRoleLabel = x.PersonRole.Label,
        //                                  RoleKindId = x.PersonRole.RoleKindId,
        //                                  DateFrom = x.DateFrom,
        //                                  DateTo = x.DateTo,
        //                                  IsDeceased = x.IsDeceased,
        //                                  RowNumber = x.CaseSessionId == null ? x.RowNumber : casePersonsQuery.Where(a => a.CasePersonIdentificator == x.CasePersonIdentificator)
        //                                                                                                      .Select(a => a.RowNumber)
        //                                                                                                      .FirstOrDefault()
        //                              })
        //                              .ToList();

        //    var caseSessionNotificationLists = caseNotificationService.CaseSessionNotificationList_SelectByCaseId(CaseId).ToList();
        //    var caseSessionActs = caseSessionActService.CaseSessionAct_Select(0, CaseId, null, null, null, null).ToList();
        //    var caseLawUnitsAll = caseLawUnitService.CaseLawUnit_Select(CaseId, null, true).ToList();
        //    var caseLawUnitsActive = caseLawUnitService.CaseLawUnit_Select(CaseId, null).ToList();
        //    var caseSessionLawUnits = caseLawUnitService.CaseLawUnitByCaseFromSession_Select(CaseId).ToList();
        //    var caseSessionResults = caseSessionService.CaseSessionResultStringList_SelectByCaseId(CaseId).ToList();
        //    var caseSessionMeetings = caseSessionMeetingService.CaseSessionMeeting_SelectByCaseId(CaseId).ToList();
        //    var caseMigrations = caseMigrationService.Select(CaseId).ToList();
        //    var regixListVMs = regixReportService.RegixListByCase_Select(CaseId).Where(x => x.RegixRequestTypeId == NomenclatureConstants.RegixRequestTypes.FromReport).ToList();

        //    List<CaseMigrationVM> caseMigrationResult = new List<CaseMigrationVM>();
        //    foreach (var migration in caseMigrations.Where(x => x.CaseId != CaseId))
        //    {
        //        if (!caseMigrationResult.Any(x => x.CaseId == migration.CaseId))
        //        {
        //            caseMigrationResult.Add(migration);
        //        }
        //    }

        //    var caseOutDocs = repo.AllReadonly<DocumentCaseInfo>()
        //                                        .Include(x => x.Document)
        //                                        .ThenInclude(x => x.DocumentType)
        //                                        .Where(x => x.CaseId == CaseId &&
        //                                                    x.Document.DocumentDirectionId == DocumentConstants.DocumentDirection.OutGoing &&
        //                                                    x.Document.DateExpired == null)
        //                                        .Select(x => new DocumentInfoVM
        //                                        {
        //                                            Id = x.Document.Id,
        //                                            Title = $"Изх.№ {x.Document.DocumentNumber}/{x.Document.DocumentDate:dd.MM.yyyy} {x.Document.DocumentType.Label}",
        //                                            DirectionId = x.Document.DocumentDirectionId,
        //                                            IsSecret = (x.Document.IsSecret ?? false),
        //                                            IsRestriction = x.Document.IsRestictedAccess,
        //                                            DocumentDate = x.Document.DocumentDate,
        //                                            DocumentResolutions = x.Document.DocumentResolutions.Where(c => c.DateExpired == null && c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced).Select(c => new DocumentResolutionListVM() { Id = c.Id, ResolutionTypeLabel = c.ResolutionType.Label, Label = c.ResolutionType.Label + " " + c.RegNumber + "/" + (c.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"), RegDate = c.RegDate, RegNumber = c.RegNumber }).ToList() ?? new List<DocumentResolutionListVM>(),
        //                                            DocumentTemplateLabel = (x.Document.DocumentTemplates.Any(d => d.DateExpired == null) ? (x.Document.DocumentTemplates.FirstOrDefault()).HtmlTemplate.Label : string.Empty),
        //                                            DocumentTemplateDescription = (x.Document.DocumentTemplates.Any(d => d.DateExpired == null) ? (x.Document.DocumentTemplates.FirstOrDefault()).Description : string.Empty),
        //                                            DocumentPersonLabel = string.Join(", ", x.Document.DocumentPersons.Select(p => p.FullName)),
        //                                            Description = (x.Description ?? string.Empty) + (!string.IsNullOrEmpty(x.Document.Description) ? (!string.IsNullOrEmpty(x.Description) ? " " : string.Empty) + x.Document.Description : string.Empty)
        //                                        }).ToList();

        //    var caseInDocs = repo.AllReadonly<DocumentCaseInfo>()
        //                                        .Include(x => x.Document)
        //                                        .ThenInclude(x => x.DocumentType)
        //                                        .Include(x => x.Document)
        //                                        .ThenInclude(x => x.DocumentResolutions)
        //                                        .ThenInclude(x => x.ResolutionType)
        //                                        .Where(x => x.CaseId == CaseId &&
        //                                                    x.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument &&
        //                                                    x.Document.DateExpired == null)
        //                                        .Select(x => new DocumentInfoVM
        //                                        {
        //                                            Id = x.Document.Id,
        //                                            Title = $"Вх.№ {x.Document.DocumentNumber}/{x.Document.DocumentDate:dd.MM.yyyy} {x.Document.DocumentType.Label}",
        //                                            DirectionId = x.Document.DocumentDirectionId,
        //                                            IsSecret = (x.Document.IsSecret ?? false),
        //                                            IsRestriction = x.Document.IsRestictedAccess,
        //                                            DocumentResolutions = x.Document.DocumentResolutions.Where(c => c.DateExpired == null && c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced).Select(c => new DocumentResolutionListVM() { Id = c.Id, ResolutionTypeLabel = c.ResolutionType.Label, Label = c.ResolutionType.Label + " " + c.RegNumber + "/" + (c.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"), RegDate = c.RegDate, RegNumber = c.RegNumber }).ToList() ?? new List<DocumentResolutionListVM>(),
        //                                            DocumentDate = x.Document.DocumentDate,
        //                                            DocumentTemplateLabel = (x.Document.DocumentTemplates.Any(d => d.DateExpired == null) ? (x.Document.DocumentTemplates.FirstOrDefault()).HtmlTemplate.Label : string.Empty),
        //                                            DocumentTemplateDescription = (x.Document.DocumentTemplates.Any(d => d.DateExpired == null) ? (x.Document.DocumentTemplates.FirstOrDefault()).Description : string.Empty),
        //                                            DocumentPersonLabel = string.Join(", ", x.Document.DocumentPersons.Select(p => p.FullName)),
        //                                            Description = (x.Description ?? string.Empty) + (!string.IsNullOrEmpty(x.Document.Description) ? (!string.IsNullOrEmpty(x.Description) ? " " : string.Empty) + x.Document.Description : string.Empty)
        //                                        }).ToList();

        //    foreach (var documentInfo in caseInDocs)
        //    {
        //        documentInfo.WorkTasks = repo.AllReadonly<WorkTask>()
        //                                        .Where(x => x.SourceType == SourceTypeSelectVM.Document &&
        //                                                    x.SourceId == documentInfo.Id)
        //                                        .Select(x => new WorkTaskReportVM()
        //                                        {
        //                                            Id = x.Id,
        //                                            TaskTypeId = x.TaskTypeId,
        //                                            TaskTypeName = x.TaskType.Label,
        //                                            DateCreated = x.DateCreated,
        //                                            DateCompleted = x.DateCompleted,
        //                                            DescriptionCreated = x.DescriptionCreated,
        //                                            Description = x.Description
        //                                        })
        //                                        .ToList();
        //    }

        //    //var caseOutDocsFromTemplate = repo.AllReadonly<DocumentTemplate>()
        //    //                                    .Include(x => x.Document)
        //    //                                    .ThenInclude(x => x.DocumentType)
        //    //                                    .Where(x => x.CaseId == CaseId &&
        //    //                                                x.DocumentId > 0 &&
        //    //                                                x.Document.DateExpired == null)
        //    //                                    .Select(x => new DocumentInfoVM
        //    //                                    {
        //    //                                        Id = x.Document.Id,
        //    //                                        Title = $"Изх.№ {x.Document.DocumentNumber}/{x.Document.DocumentDate:dd.MM.yyyy} {x.Document.DocumentType.Label}",
        //    //                                        DirectionId = x.Document.DocumentDirectionId,
        //    //                                        IsSecret = (x.Document.IsSecret ?? false),
        //    //                                        IsRestriction = x.Document.IsRestictedAccess,
        //    //                                        DocumentDate = x.Document.DocumentDate,
        //    //                                        DocumentTemplateLabel = (x.Document.DocumentTemplates.Any(d => d.DateExpired == null) ? (x.Document.DocumentTemplates.FirstOrDefault()).HtmlTemplate.Label : string.Empty),
        //    //                                        DocumentPersonLabel = string.Join(", ", x.Document.DocumentPersons.Select(p => p.FullName))
        //    //                                    }).ToList() ?? new List<DocumentInfoVM>();

        //    //if (caseOutDocs != null)
        //    //{
        //    //    foreach (var documentInfoVM in caseOutDocs)
        //    //    {
        //    //        if (!caseOutDocsFromTemplate.Any(x => x.Id == documentInfoVM.Id))
        //    //        {
        //    //            caseOutDocsFromTemplate.Add(documentInfoVM);
        //    //        }
        //    //    }
        //    //    //caseOutDocsFromTemplate.AddRange(caseOutDocs);
        //    //}

        //    var caseElectronicFolderVM = repo.AllReadonly<Case>()
        //                                     .Include(x => x.CaseArchives)
        //                                     .Include(x => x.CaseSessions)
        //                                     .ThenInclude(x => x.SessionType)
        //                                     .Include(x => x.CaseSessions)
        //                                     .ThenInclude(x => x.CourtHall)
        //                                     .Include(x => x.CaseSessions)
        //                                     .ThenInclude(x => x.SessionState)
        //                                     .Include(x => x.Court)
        //                                     .Include(x => x.CaseGroup)
        //                                     .Include(x => x.CaseType)
        //                                     .Include(x => x.CaseCode)
        //                                     .Include(x => x.ProcessPriority)
        //                                     .Include(x => x.CaseState)
        //                                     .Include(x => x.Document)
        //                                     .ThenInclude(x => x.DeliveryType)
        //                                     .Include(x => x.Document)
        //                                     .ThenInclude(x => x.DocumentResolutions)
        //                                     .ThenInclude(x => x.ResolutionType)
        //                                     .Include(x => x.Document)
        //                                     .ThenInclude(x => x.DocumentCaseInfo)
        //                                     .Where(x => x.Id == CaseId)
        //                                     .Select(x => new CaseElectronicFolderVM()
        //                                     {
        //                                         Id = x.Id,
        //                                         CourtId = x.CourtId,
        //                                         IsOnlyFiles = x.CourtId != userContext.CourtId,
        //                                         CourtLabel = x.Court.Label,
        //                                         CaseGroupLabel = (x.CaseGroup != null) ? x.CaseGroup.Label : string.Empty,
        //                                         CaseTypeLabel = (x.CaseType != null) ? x.CaseType.Code : string.Empty,
        //                                         CaseCodeLabel = (x.CaseCode != null) ? x.CaseCode.Code + " " + x.CaseCode.Label : string.Empty,
        //                                         CaseStateLabel = (x.CaseState != null) ? x.CaseState.Label : string.Empty,
        //                                         RegNumber = x.RegNumber,
        //                                         RegDate = x.RegDate,
        //                                         RegNumberText = x.CaseStateId == NomenclatureConstants.CaseState.Rejected ? "Отказ от образуване" : x.CaseType.Code + " " + x.ShortNumber + "/" + x.RegDate.ToString("yyyy"),
        //                                         DocumentId = x.DocumentId,
        //                                         DocumentDate = x.Document.DocumentDate,
        //                                         DocumentLabel = "Вх.№ " + x.Document.DocumentNumber + "/" + x.Document.DocumentDate.ToString("dd.MM.yyyy") + " " + x.Document.DocumentType.Label,
        //                                         DocumentDescription = x.Document.Description,
        //                                         DocumentResolutions = x.Document.DocumentResolutions
        //                                                                         .Where(c => c.DateExpired == null && c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced)
        //                                                                         .Select(c => new DocumentResolutionListVM()
        //                                                                         {
        //                                                                             Id = c.Id,
        //                                                                             ResolutionTypeId = c.ResolutionTypeId,
        //                                                                             ResolutionTypeLabel = c.ResolutionType.Label,
        //                                                                             Label = c.ResolutionType.Label + " " + c.RegNumber + "/" + (c.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"),
        //                                                                             RegDate = c.RegDate,
        //                                                                             RegNumber = c.RegNumber,
        //                                                                         }).ToList(),
        //                                         DocumentSecret = (x.Document.IsSecret ?? false),
        //                                         DocumentRestriction = x.Document.IsRestictedAccess,//x.IsRestictedAccess,
        //                                         CaseSelectionProtokols = caseSelectionProtokolListVMs,
        //                                         CaseInDocuments = caseInDocs,
        //                                         RegixReports = regixListVMs,
        //                                         CaseOutDocuments = caseOutDocs,
        //                                         CaseReasonLabel = (x.CaseReason != null) ? x.CaseReason.Label : string.Empty,
        //                                         CaseStateDescription = x.CaseStateDescription,
        //                                         CasePersons = casePersonLists.Where(person => person.CaseSessionId == null).ToList(),
        //                                         CaseSessionFinalActs = caseSessionActs.Where(a => a.ActDeclaredDate != null).ToList(),
        //                                         DocumentCaseInfos = x.Document.DocumentCaseInfo.Where(f => (f.IsLegacyCase ?? false)).ToList(),
        //                                         ArchRegNumber = (x.CaseArchives != null) ? x.CaseArchives.FirstOrDefault().RegNumber : string.Empty,
        //                                         ArchRegDate = (x.CaseArchives != null) ? x.CaseArchives.FirstOrDefault().RegDate : DateTime.Now,
        //                                         CaseInforcedDate = x.CaseInforcedDate,
        //                                         EISSPNumber = x.EISSPNumber,
        //                                         CaseSessions = x.CaseSessions.Where(p => p.DateExpired == null).Select(p => new CaseSessionElectronicFolderVM()
        //                                         {
        //                                             Id = p.Id,
        //                                             CourtId = p.CourtId ?? 0,
        //                                             SessionTypeLabel = (p.SessionType != null) ? p.SessionType.Label : string.Empty,
        //                                             CourtHallName = (p.CourtHall != null) ? p.CourtHall.Name : string.Empty,
        //                                             SessionStateLabel = p.SessionState.Label,
        //                                             DateFrom = p.DateFrom,
        //                                             DateTo = p.DateTo,
        //                                             Description = x.Description,
        //                                             DateTo_Minutes = Convert.ToInt32(((TimeSpan)(p.DateTo ?? p.DateFrom).Subtract(p.DateFrom)).TotalMinutes),
        //                                             CaseSessionNotificationLists = caseSessionNotificationLists.Where(nl => nl.CaseSessionId == p.Id).ToList(),
        //                                             CaseSessionActs = caseSessionActs.Where(s => s.CaseSessionId == p.Id && s.ActDeclaredDate != null).ToList(),
        //                                             SessionMeetings = caseSessionMeetings.Where(meet => meet.CaseSessionId == p.Id).ToList(),
        //                                             CaseSessionDocs = caseSessionDocVMs.Where(sessdoc => sessdoc.CaseSessionId == p.Id).ToList(),
        //                                             SessionResultText = string.Join(", ", p.CaseSessionResults.Where(r => r.DateExpired == null).Select(r => r.SessionResult.Label))
        //                                         }).ToList()
        //                                     })
        //                                     .FirstOrDefault();

        //    var judgeRep = caseLawUnitsActive.Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).FirstOrDefault();
        //    if (judgeRep != null)
        //    {
        //        caseElectronicFolderVM.JudgeRapporteur = judgeRep.LawUnitName + ((!string.IsNullOrEmpty(judgeRep.DepartmentLabel)) ? " състав: " + judgeRep.DepartmentLabel : string.Empty);
        //    }

        //    foreach (var caseSession in caseElectronicFolderVM.CaseSessions)
        //    {
        //        caseSession.CaseNotifications = caseNotificationService.CaseNotification_Select(CaseId, caseSession.Id, null).ToList();
        //        caseSession.SessionFastDocuments = caseSessionFastDocumentService.CaseSessionFastDocument_Select(caseSession.Id).ToList();

        //        var judgeRepSession = caseSessionLawUnits.Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter && x.CaseSessionId == caseSession.Id).FirstOrDefault();
        //        caseSession.JudgeRapporteur = judgeRepSession != null ? judgeRepSession.LawUnitName + ((!string.IsNullOrEmpty(judgeRepSession.DepartmentLabel)) ? " състав: " + judgeRepSession.DepartmentLabel : string.Empty) : string.Empty;

        //        foreach (var item in caseSessionResults.Where(r => r.Value == caseSession.Id.ToString()))
        //            caseSession.SessionStateString += item.Text + " ";

        //        caseSession.Prokuror = string.Empty;
        //        foreach (var item in casePersonLists.Where(x => x.CaseSessionId == caseSession.Id && x.PersonRoleId == NomenclatureConstants.PersonRole.Prokuror))
        //        {
        //            caseSession.Prokuror += item.FullName + "; ";
        //        }

        //        foreach (var caseSessionMeeting in caseSession.SessionMeetings)
        //        {
        //            caseSessionMeeting.UsersNames = string.Empty;

        //            var caseSessionMeetingUsers = caseSessionMeetingService.CaseSessionMeetingUser_Select(caseSessionMeeting.Id).ToList();

        //            foreach (var caseSessionMeetingUser in caseSessionMeetingUsers)
        //            {
        //                caseSessionMeeting.UsersNames += caseSessionMeetingUser.SecretaryUserName + "; ";
        //            }
        //        }
        //    }

        //    caseElectronicFolderVM.CaseLawUnits = caseLawUnitsAll.Where(x => x.CaseSessionId == null).ToList();
        //    caseElectronicFolderVM.CaseClassifications = caseClassificationService.CaseClassification_SelectObject(CaseId, null).ToList();
        //    caseElectronicFolderVM.CaseMigrations = caseMigrationResult.ToList();
        //    caseElectronicFolderVM.DocumentInstitutionCaseInfos = repo.AllReadonly<DocumentInstitutionCaseInfo>()
        //                                                              .Include(x => x.Institution)
        //                                                              .Include(x => x.InstitutionCaseType)
        //                                                              .Where(x => x.DocumentId == caseElectronicFolderVM.DocumentId &&
        //                                                                          x.Document.DateExpired == null)
        //                                                              .ToList();
        //    caseElectronicFolderVM.PaymentCases = moneyService.PaymentForCase_Select(caseElectronicFolderVM.Id).ToList();
        //    caseElectronicFolderVM.ExecLists = moneyService.ExecListForCase_Select(caseElectronicFolderVM.Id).ToList();
        //    caseElectronicFolderVM.ExpenseOrders = moneyService.ExpenseOrderForCase_Select(caseElectronicFolderVM.Id).ToList();
        //    caseElectronicFolderVM.DocumentDecisionCaseLists = documentService.DocumentDecisionCaseByCase_Select(caseElectronicFolderVM.Id).ToList();

        //    caseElectronicFolderVM.DocumentsOtherFromDifferentCourt = documentService.DocumentsOtherFromDifferentCourtByCaseId_Select(caseElectronicFolderVM.Id).ToList();
        //    caseElectronicFolderVM.DocumentsOtherFromSameCourt = documentService.DocumentsOtherFromSameCourtByCaseId_Select(caseElectronicFolderVM.Id).ToList();

        //    List<WorkTaskReportVM> workTaskReports = new List<WorkTaskReportVM>();

        //    workTaskReports.AddRange(repo.AllReadonly<WorkTask>()
        //                                 .Where(x => x.SourceType == SourceTypeSelectVM.Document &&
        //                                             x.SourceId == caseElectronicFolderVM.DocumentId)
        //                                 .Select(x => new WorkTaskReportVM()
        //                                 {
        //                                     Id = x.Id,
        //                                     TaskTypeId = x.TaskTypeId,
        //                                     TaskTypeName = x.TaskType.Label,
        //                                     DateCreated = x.DateCreated,
        //                                     DateCompleted = x.DateCompleted,
        //                                     DescriptionCreated = x.DescriptionCreated,
        //                                     Description = x.Description
        //                                 })
        //                                 .Union(repo.AllReadonly<WorkTask>()
        //                                            .Where(d => d.SourceType == SourceTypeSelectVM.Case &&
        //                                                        d.SourceId == caseElectronicFolderVM.Id)
        //                                            .Select(d => new WorkTaskReportVM()
        //                                            {
        //                                                Id = d.Id,
        //                                                TaskTypeId = d.TaskTypeId,
        //                                                TaskTypeName = d.TaskType.Label,
        //                                                DateCreated = d.DateCreated,
        //                                                DateCompleted = d.DateCompleted,
        //                                                DescriptionCreated = d.DescriptionCreated,
        //                                                Description = d.Description
        //                                            }))
        //                                 .ToList());

        //    foreach (var documentResolution in caseElectronicFolderVM.DocumentResolutions)
        //    {
        //        documentResolution.DocumentTemplateVMs = documentTemplateService.DocumentTemplate_Select(SourceTypeSelectVM.DocumentResolution, documentResolution.Id).ToList();

        //        workTaskReports.AddRange(repo.AllReadonly<WorkTask>()
        //                                     .Where(x => x.SourceType == SourceTypeSelectVM.DocumentResolution &&
        //                                                 x.SourceId == documentResolution.Id)
        //                                     .Select(x => new WorkTaskReportVM()
        //                                     {
        //                                         Id = x.Id,
        //                                         TaskTypeId = x.TaskTypeId,
        //                                         TaskTypeName = x.TaskType.Label,
        //                                         DateCreated = x.DateCreated,
        //                                         DateCompleted = x.DateCompleted,
        //                                         DescriptionCreated = x.DescriptionCreated,
        //                                         Description = x.Description
        //                                     })
        //                                     .ToList());
        //    }

        //    caseElectronicFolderVM.WorkTaskReports = workTaskReports;

        //    caseElectronicFolderVM.DocumentResolutionCase = repo.AllReadonly<DocumentResolutionCase>()
        //                                                        .Where(x => x.CaseId == CaseId &&
        //                                                                    x.DocumentResolution.ResolutionTypeId == DocumentConstants.ResolutionTypes.ResolutionForSelection &&
        //                                                                    x.DocumentResolution.DateExpired == null)
        //                                                        .Select(x => new DocumentResolutionListVM()
        //                                                        {
        //                                                            Id = x.DocumentResolution.Id,
        //                                                            ResolutionTypeId = x.DocumentResolution.ResolutionTypeId,
        //                                                            ResolutionTypeLabel = x.DocumentResolution.ResolutionType.Label,
        //                                                            Label = x.DocumentResolution.ResolutionType.Label + " " + x.DocumentResolution.RegNumber + "/" + (x.DocumentResolution.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"),
        //                                                            RegDate = x.DocumentResolution.RegDate,
        //                                                            RegNumber = x.DocumentResolution.RegNumber
        //                                                        })
        //                                                        .ToList();
        //    var obligationVMs = new List<ObligationVM>();
        //    obligationVMs.AddRange(moneyService.Obligation_Select(0, caseElectronicFolderVM.DocumentId, 0, caseElectronicFolderVM.CourtId).ToList());
        //    foreach (var documentInfoVM in caseElectronicFolderVM.CaseInDocuments)
        //    {
        //        obligationVMs.AddRange(moneyService.Obligation_Select(0, documentInfoVM.Id, 0, caseElectronicFolderVM.CourtId).ToList());
        //    }

        //    caseElectronicFolderVM.MoneyDocument = obligationVMs;

        //    return caseElectronicFolderVM;
        //}

        private IQueryable<CaseSelectionProtokol> GetQueryProtokol()
        {
            return readonlyrepo.AllReadonly<CaseSelectionProtokol>();
        }

        private IQueryable<CaseSessionDoc> GetQueryCaseSessionDoc()
        {
            return readonlyrepo.AllReadonly<CaseSessionDoc>()
                       .Where(d => d.DateExpired == null &&
                                   d.Document.DateExpired == null);
        }

        private IQueryable<WorkTask> GetQueryWorkTask()
        {
            return readonlyrepo.AllReadonly<WorkTask>()
                       .Where(x => x.SourceType == SourceTypeSelectVM.Document);
        }

        private IQueryable<CasePerson> GetQueryCasePerson()
        {
            return readonlyrepo.AllReadonly<CasePerson>()
                       .Where(p => p.DateExpired == null);
        }

        private IQueryable<DocumentCaseInfo> GetQueryDocumentCaseInfo()
        {
            return readonlyrepo.AllReadonly<DocumentCaseInfo>()
                       .Where(d => d.Document.DateExpired == null);
        }

        private IQueryable<RegixReport> GetQueryRegixReport()
        {
            return readonlyrepo.AllReadonly<RegixReport>();
        }

        private IQueryable<CaseSessionAct> GetQueryCaseSessionAct()
        {
            return readonlyrepo.AllReadonly<CaseSessionAct>()
                       .Where(a => a.DateExpired == null);
        }

        private IQueryable<CaseSessionNotificationList> GetQueryCaseSessionNotificationList()
        {
            return readonlyrepo.AllReadonly<CaseSessionNotificationList>()
                       .Where(a => a.DateExpired == null);
        }

        private IQueryable<CaseNotification> GetQueryCaseNotification()
        {
            return readonlyrepo.AllReadonly<CaseNotification>()
                       .Where(a => a.DateExpired == null);
        }

        private IQueryable<Infrastructure.Data.Models.Nomenclatures.NotificationDeliveryGroup> GetQueryNotificationDeliveryGroup()
        {
            return readonlyrepo.AllReadonly<Infrastructure.Data.Models.Nomenclatures.NotificationDeliveryGroup>();
        }

        private IQueryable<CaseSessionFastDocument> GetQueryCaseSessionFastDocument()
        {
            return readonlyrepo.AllReadonly<CaseSessionFastDocument>()
                       .Where(d => d.DateExpired == null);
        }

        private IQueryable<CaseMigration> GetQueryCaseMigration()
        {
            return readonlyrepo.AllReadonly<CaseMigration>()
                       .Where(d => d.DateExpired == null);
        }

        private IQueryable<DocumentInstitutionCaseInfo> GetQueryDocumentInstitutionCaseInfo()
        {
            return readonlyrepo.AllReadonly<DocumentInstitutionCaseInfo>()
                       .Include(d => d.Institution)
                       .Include(d => d.InstitutionCaseType)
                       .Where(d => d.Document.DateExpired == null);
        }

        private IQueryable<Payment> GetQueryPayment()
        {
            return readonlyrepo.AllReadonly<Payment>()
                       .Where(p => p.IsActive);
        }

        private IQueryable<ExecList> GetQueryExecList()
        {
            return readonlyrepo.AllReadonly<ExecList>()
                       .Where(d => d.IsActive);
        }

        private IQueryable<ExpenseOrder> GetQueryExpenseOrder()
        {
            return readonlyrepo.AllReadonly<ExpenseOrder>()
                       .Where(d => d.IsActive);
        }

        private IQueryable<DocumentDecisionCase> GetQueryDocumentDecisionCase()
        {
            return readonlyrepo.AllReadonly<DocumentDecisionCase>();
        }

        private IQueryable<Case> GetQueryCase()
        {
            return readonlyrepo.AllReadonly<Case>();
        }

        private IQueryable<DocumentTemplate> GetQueryDocumentTemplate()
        {
            return readonlyrepo.AllReadonly<DocumentTemplate>()
                       .Where(d => d.DateExpired == null &&
                                   d.Document.DateExpired == null);
        }

        private IQueryable<DocumentLink> GetQueryDocumentLink()
        {
            return readonlyrepo.AllReadonly<DocumentLink>();
        }

        private IQueryable<DocumentResolutionCase> GetQueryDocumentResolutionCase()
        {
            return readonlyrepo.AllReadonly<DocumentResolutionCase>()
                       .Where(d => d.DocumentResolution.DateExpired == null);
        }

        private IQueryable<Obligation> GetQueryObligation()
        {
            return readonlyrepo.AllReadonly<Obligation>()
                       .Where(o => o.CourtId == userContext.CourtId);
        }

        private IQueryable<CaseClassification> GetQueryCaseClassification()
        {
            return readonlyrepo.AllReadonly<CaseClassification>()
                               .Include(c => c.Classification);
        }

        ///// <summary>
        ///// Извлича данни за дело за ел. папка
        ///// </summary>
        ///// <param name="caseId"></param>
        ///// <returns></returns>
        //public async Task<CaseElectronicFolderVM> CaseElectronicFolder_Select(int caseId)
        //{
        //    try
        //    {
        //        var queryProtokol = GetQueryProtokol();
        //        var querySessionDoc = GetQueryCaseSessionDoc();
        //        var queryWorkTask = GetQueryWorkTask();
        //        var queryPerson = GetQueryCasePerson();
        //        var queryDocumentCaseInfo = GetQueryDocumentCaseInfo();
        //        var queryRegixReport = GetQueryRegixReport();
        //        var queryCaseSessionAct = GetQueryCaseSessionAct();
        //        var queryCaseSessionNotificationList = GetQueryCaseSessionNotificationList();
        //        var queryCaseNotification = GetQueryCaseNotification();
        //        var queryNotificationDeliveryGroup = GetQueryNotificationDeliveryGroup();
        //        var queryCaseSessionFastDocument = GetQueryCaseSessionFastDocument();
        //        var queryCaseMigration = GetQueryCaseMigration();
        //        var queryDocumentInstitutionCaseInfo = GetQueryDocumentInstitutionCaseInfo();
        //        var queryPayment = GetQueryPayment();
        //        var queryExecList = GetQueryExecList();
        //        var queryExpenseOrder = GetQueryExpenseOrder();
        //        var queryDocumentDecisionCase = GetQueryDocumentDecisionCase();
        //        var queryCase = GetQueryCase();
        //        var queryDocumentTemplate = GetQueryDocumentTemplate();
        //        var queryDocumentResolutionCase = GetQueryDocumentResolutionCase();
        //        var queryObligation = GetQueryObligation();
        //        var queryCaseClassification = GetQueryCaseClassification();

        //        var caseElectronicFolderVM = await repo.AllReadonly<Case>()
        //                                               .Where(x => x.Id == caseId)
        //                                               .Select(x => new CaseElectronicFolderVM()
        //                                               {
        //                                                   Id = x.Id,
        //                                                   CourtId = x.CourtId,
        //                                                   IsOnlyFiles = x.CourtId != userContext.CourtId,
        //                                                   CourtLabel = x.Court.Label,
        //                                                   CaseGroupLabel = x.CaseGroup.Label,
        //                                                   CaseTypeLabel = x.CaseType.Code,
        //                                                   CaseCodeLabel = x.CaseCode.Code + " " + x.CaseCode.Label,
        //                                                   CaseStateLabel = x.CaseState.Label,
        //                                                   RegNumber = x.RegNumber,
        //                                                   RegDate = x.RegDate,
        //                                                   JudgeRapporteur = x.CaseLawUnits
        //                                                                      .Where(l => l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
        //                                                                      .Select(l => l.LawUnit.FullName + ((l.CourtDepartmentId != null) ? " състав: " + l.CourtDepartment.Label : string.Empty))
        //                                                                      .FirstOrDefault(),
        //                                                   RegNumberText = x.CaseStateId == NomenclatureConstants.CaseState.Rejected ? "Отказ от образуване"
        //                                                                                                                             : string.Concat(x.CaseType.Code, " ", x.ShortNumber, "/", x.RegDate.ToString("yyyy")),
        //                                                   DocumentId = x.DocumentId,
        //                                                   DocumentDate = x.Document.DocumentDate,
        //                                                   DocumentLabel = "Вх.№ " + x.Document.DocumentNumber + "/" + x.Document.DocumentDate.ToString("dd.MM.yyyy") + " " + x.Document.DocumentType.Label,
        //                                                   DocumentDescription = x.Document.Description,
        //                                                   DocumentResolutions = x.Document.DocumentResolutions
        //                                                                                   .Where(c => c.DateExpired == null && c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced)
        //                                                                                   .Select(c => new DocumentResolutionListVM()
        //                                                                                   {
        //                                                                                       Id = c.Id,
        //                                                                                       ResolutionTypeId = c.ResolutionTypeId,
        //                                                                                       ResolutionTypeLabel = c.ResolutionType.Label,
        //                                                                                       Label = c.ResolutionType.Label + " " + c.RegNumber + "/" + (c.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"),
        //                                                                                       RegDate = c.RegDate,
        //                                                                                       RegNumber = c.RegNumber,
        //                                                                                       DocumentTemplateVMs = queryDocumentTemplate.Where(d => d.SourceType == SourceTypeSelectVM.DocumentResolution &&
        //                                                                                                                                              d.SourceId == c.Id)
        //                                                                                                                                  .OrderByDescending(d => d.DateWrt)
        //                                                                                                                                  .Select(d => new DocumentTemplateVM()
        //                                                                                                                                  {
        //                                                                                                                                      Id = d.Id,
        //                                                                                                                                      AuthorName = d.Author.LawUnit.FullName,
        //                                                                                                                                      DocumentTypeLabel = d.DocumentType.Label,
        //                                                                                                                                      DateWrt = d.DateWrt,
        //                                                                                                                                      StateName = d.DocumentTemplateState.Label,
        //                                                                                                                                      DocumentId = d.DocumentId,
        //                                                                                                                                      DocumentNumber = (d.Document != null) ? $"{d.Document.DocumentNumber}/{d.Document.DocumentDate:dd.MM.yyyy}" : "",
        //                                                                                                                                      HtmlTemplateName = d.HtmlTemplateId == null ? "Общ формуляр" : d.HtmlTemplate.Label
        //                                                                                                                                  })
        //                                                                                                                                  .ToList()
        //                                                                                   })
        //                                                                                   .ToList(),
        //                                                   DocumentSecret = x.Document.IsSecret ?? false,
        //                                                   DocumentRestriction = x.Document.IsRestictedAccess,
        //                                                   CaseSelectionProtokols = queryProtokol.Where(p => p.CaseId == x.Id)
        //                                                                                         .Select(p => new CaseSelectionProtokolListVM()
        //                                                                                         {
        //                                                                                             Id = p.Id,
        //                                                                                             SelectionDate = p.SelectionDate,
        //                                                                                             JudgeRoleName = p.JudgeRole.Label,
        //                                                                                             SelectionModeName = p.SelectionMode.Label,
        //                                                                                             SelectedLawUnitName = p.SelectedLawUnit.FullName,
        //                                                                                             SelectionProtokolStateName = p.SelectionProtokolState.Label
        //                                                                                         })
        //                                                                                         .ToList(),
        //                                                   CaseInDocuments = queryDocumentCaseInfo.Where(i => i.CaseId == x.Id &&
        //                                                                                                      i.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument)
        //                                                                                          .Select(i => new DocumentInfoVM()
        //                                                                                          {
        //                                                                                              Id = i.DocumentId,
        //                                                                                              Title = $"Вх.№ {i.Document.DocumentNumber}/{i.Document.DocumentDate:dd.MM.yyyy} {i.Document.DocumentType.Label}",
        //                                                                                              DirectionId = i.Document.DocumentDirectionId,
        //                                                                                              IsSecret = (i.Document.IsSecret ?? false),
        //                                                                                              IsRestriction = i.Document.IsRestictedAccess,
        //                                                                                              DocumentResolutions = i.Document
        //                                                                                                                     .DocumentResolutions
        //                                                                                                                     .Where(c => c.DateExpired == null &&
        //                                                                                                                                 c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced)
        //                                                                                                                     .Select(c => new DocumentResolutionListVM()
        //                                                                                                                     {
        //                                                                                                                         Id = c.Id,
        //                                                                                                                         ResolutionTypeLabel = c.ResolutionType.Label,
        //                                                                                                                         Label = c.ResolutionType.Label + " " + c.RegNumber + "/" + (c.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"),
        //                                                                                                                         RegDate = c.RegDate,
        //                                                                                                                         RegNumber = c.RegNumber
        //                                                                                                                     })
        //                                                                                                                     .ToList(),
        //                                                                                              DocumentDate = i.Document.DocumentDate,
        //                                                                                              DocumentTemplateLabel = i.Document
        //                                                                                                                       .DocumentTemplates
        //                                                                                                                       .Any(d => d.DateExpired == null) ? i.Document
        //                                                                                                                                                           .DocumentTemplates
        //                                                                                                                                                           .Where(d => d.DateExpired == null)
        //                                                                                                                                                           .Select(t => t.HtmlTemplate.Label)
        //                                                                                                                                                           .FirstOrDefault() : string.Empty,
        //                                                                                              DocumentTemplateDescription = i.Document
        //                                                                                                                             .DocumentTemplates
        //                                                                                                                             .Any(d => d.DateExpired == null) ? i.Document
        //                                                                                                                                                                 .DocumentTemplates
        //                                                                                                                                                                 .Select(d => d.Description)
        //                                                                                                                                                                 .FirstOrDefault() : string.Empty,
        //                                                                                              DocumentPersonLabel = string.Join(", ", i.Document.DocumentPersons.Select(p => p.FullName)),
        //                                                                                              Description = (i.Description ?? string.Empty) + (!string.IsNullOrEmpty(i.Document.Description) ? (!string.IsNullOrEmpty(i.Description) ? " " : string.Empty) + i.Document.Description : string.Empty),
        //                                                                                              WorkTasks = queryWorkTask.Where(w => w.SourceId == i.Id)
        //                                                                                                                       .Select(w => new WorkTaskReportVM()
        //                                                                                                                       {
        //                                                                                                                           Id = w.Id,
        //                                                                                                                           TaskTypeId = w.TaskTypeId,
        //                                                                                                                           TaskTypeName = w.TaskType.Label,
        //                                                                                                                           DateCreated = w.DateCreated,
        //                                                                                                                           DateCompleted = w.DateCompleted,
        //                                                                                                                           DescriptionCreated = w.DescriptionCreated,
        //                                                                                                                           Description = w.Description
        //                                                                                                                       })
        //                                                                                                                       .ToList()
        //                                                                                          })
        //                                                                                          .ToList(),
        //                                                   RegixReports = queryRegixReport.Where(r => r.CaseId == x.Id)
        //                                                                                  .Select(r => new RegixListVM()
        //                                                                                  {
        //                                                                                      Id = r.Id,
        //                                                                                      CaseId = r.CaseId ?? 0,
        //                                                                                      RegixTypeName = r.RegixType.Label,
        //                                                                                      UserName = r.User.LawUnit.FullName,
        //                                                                                      CaseRegNumber = r.Case.RegNumber,
        //                                                                                      DocumentNumber = r.DocumentId != null ? (r.Document.DocumentType.Label + " " +
        //                                                                                                                               r.Document.DocumentNumber + "/" +
        //                                                                                                                               r.Document.DocumentDate.ToString(FormattingConstant.NormalDateFormat)) : string.Empty,
        //                                                                                      DocumentOnlyNumber = r.DocumentId != null ? (r.Document.DocumentNumber + "/" +
        //                                                                                                                                   r.Document.DocumentDate.ToString(FormattingConstant.NormalDateFormat)) : string.Empty,
        //                                                                                      ActRegNumber = r.CaseSessionActId != null ? (r.CaseSessionAct.ActType.Label + " " + r.CaseSessionAct.RegNumber + "/" + (r.CaseSessionAct.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy")) : string.Empty,
        //                                                                                      DateWrt = r.DateWrt,
        //                                                                                      RegixRequestTypeId = r.RegixRequestTypeId
        //                                                                                  })
        //                                                                                  .ToList(),
        //                                                   CaseOutDocuments = queryDocumentCaseInfo.Where(i => i.CaseId == x.Id &&
        //                                                                                                       i.Document.DocumentDirectionId == DocumentConstants.DocumentDirection.OutGoing)
        //                                                                                           .Select(i => new DocumentInfoVM()
        //                                                                                           {
        //                                                                                               Id = i.Document.Id,
        //                                                                                               Title = $"Изх.№ {i.Document.DocumentNumber}/{i.Document.DocumentDate:dd.MM.yyyy} {i.Document.DocumentType.Label}",
        //                                                                                               DirectionId = i.Document.DocumentDirectionId,
        //                                                                                               IsSecret = (i.Document.IsSecret ?? false),
        //                                                                                               IsRestriction = i.Document.IsRestictedAccess,
        //                                                                                               DocumentResolutions = i.Document
        //                                                                                                                      .DocumentResolutions
        //                                                                                                                      .Where(c => c.DateExpired == null &&
        //                                                                                                                                  c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced)
        //                                                                                                                      .Select(c => new DocumentResolutionListVM()
        //                                                                                                                      {
        //                                                                                                                          Id = c.Id,
        //                                                                                                                          ResolutionTypeLabel = c.ResolutionType.Label,
        //                                                                                                                          Label = c.ResolutionType.Label + " " + c.RegNumber + "/" + (c.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"),
        //                                                                                                                          RegDate = c.RegDate,
        //                                                                                                                          RegNumber = c.RegNumber
        //                                                                                                                      })
        //                                                                                                                      .ToList(),
        //                                                                                               DocumentDate = i.Document.DocumentDate,
        //                                                                                               DocumentTemplateLabel = i.Document
        //                                                                                                                        .DocumentTemplates
        //                                                                                                                        .Any(d => d.DateExpired == null) ? i.Document
        //                                                                                                                                                            .DocumentTemplates
        //                                                                                                                                                            .Where(d => d.DateExpired == null)
        //                                                                                                                                                            .Select(t => t.HtmlTemplate.Label)
        //                                                                                                                                                            .FirstOrDefault() : string.Empty,
        //                                                                                               DocumentTemplateDescription = i.Document
        //                                                                                                                              .DocumentTemplates
        //                                                                                                                              .Any(d => d.DateExpired == null) ? i.Document
        //                                                                                                                                                                  .DocumentTemplates
        //                                                                                                                                                                  .Select(d => d.Description)
        //                                                                                                                                                                  .FirstOrDefault() : string.Empty,
        //                                                                                               DocumentPersonLabel = string.Join(", ", i.Document.DocumentPersons.Select(p => p.FullName)),
        //                                                                                               Description = (i.Description ?? string.Empty) + (!string.IsNullOrEmpty(i.Document.Description) ? (!string.IsNullOrEmpty(i.Description) ? " " : string.Empty) + i.Document.Description : string.Empty),
        //                                                                                           })
        //                                                                                           .ToList(),
        //                                                   CaseReasonLabel = (x.CaseReason != null) ? x.CaseReason.Label : string.Empty,
        //                                                   CaseStateDescription = x.CaseStateDescription,
        //                                                   CasePersons = x.CasePersons.Where(p => p.CaseSessionId == null &&
        //                                                                                          p.DateExpired == null)
        //                                                                              .Select(p => new CasePersonListVM()
        //                                                                              {
        //                                                                                  Id = p.Id,
        //                                                                                  CaseId = p.CaseId,
        //                                                                                  CaseSessionId = p.CaseSessionId,
        //                                                                                  Uic = p.Uic,
        //                                                                                  UicTypeId = p.UicTypeId,
        //                                                                                  UicTypeLabel = (p.UicType != null) ? p.UicType.Label : string.Empty,
        //                                                                                  FirstName = p.FirstName,
        //                                                                                  MiddleName = p.MiddleName,
        //                                                                                  FamilyName = p.FamilyName,
        //                                                                                  Family2Name = p.Family2Name,
        //                                                                                  FullName = p.FullName,
        //                                                                                  RoleName = p.PersonRole.Label,
        //                                                                                  PersonRoleId = p.PersonRole.Id,
        //                                                                                  PersonRoleLabel = p.PersonRole.Label,
        //                                                                                  RoleKindId = p.PersonRole.RoleKindId,
        //                                                                                  DateFrom = p.DateFrom,
        //                                                                                  DateTo = p.DateTo,
        //                                                                                  IsDeceased = p.IsDeceased,
        //                                                                                  RowNumber = p.CaseSessionId == null ? p.RowNumber : x.CasePersons.Where(a => a.CasePersonIdentificator == p.CasePersonIdentificator)
        //                                                                                                                                                   .Select(a => a.RowNumber)
        //                                                                                                                                                   .FirstOrDefault()
        //                                                                              })
        //                                                                              .ToList(),
        //                                                   CaseSessionFinalActs = x.CaseSessionActs.Where(a => a.ActDeclaredDate != null)
        //                                                                                           .Select(a => new CaseSessionActVM()
        //                                                                                           {
        //                                                                                               Id = a.Id,
        //                                                                                               CaseSessionId = a.CaseSessionId,
        //                                                                                               CaseId = a.CaseSession.CaseId,
        //                                                                                               CaseSessionLabel = (a.CaseSession != null) ? a.CaseSession.SessionType.Label + "/" + a.CaseSession.DateFrom.ToString("dd.MM.yyyy HH:mm") : string.Empty,
        //                                                                                               CaseLabel = a.CaseSession.Case.RegNumber,
        //                                                                                               ActTypeLabel = (a.ActType != null) ? a.ActType.Label : string.Empty,
        //                                                                                               ActTypeId = a.ActTypeId,
        //                                                                                               ActStateLabel = (a.ActState != null) ? a.ActState.Label + (a.ActCoordination.Any(c => c.CoordinationDeclaredDate != null) ? " (ОМ)" : string.Empty) : string.Empty,
        //                                                                                               RegNumber = a.RegNumber,
        //                                                                                               RegNumberNew = a.RegNumber,
        //                                                                                               RegDate = a.RegDate.Value,
        //                                                                                               IsFinalDoc = a.IsFinalDoc,
        //                                                                                               DateWrt = a.DateWrt,
        //                                                                                               EcliCode = a.EcliCode,
        //                                                                                               Description = a.Description,
        //                                                                                               ActDeclaredDate = a.ActDeclaredDate,
        //                                                                                               ActInforcedDate = a.ActInforcedDate,
        //                                                                                               ActMotivesDeclaredDate = a.ActMotivesDeclaredDate,
        //                                                                                               ActCoordinationLabel = a.ActCoordination.Any(c => c.CoordinationDeclaredDate != null) ? "ОМ" : string.Empty
        //                                                                                           })
        //                                                                                           .ToList(),
        //                                                   DocumentCaseInfos = x.Document.DocumentCaseInfo.Where(f => (f.IsLegacyCase ?? false)).ToList(),
        //                                                   ArchRegNumber = x.CaseArchives.Any() ? x.CaseArchives.FirstOrDefault().RegNumber : string.Empty,
        //                                                   ArchRegDate = x.CaseArchives.Any() ? x.CaseArchives.FirstOrDefault().RegDate : DateTime.Now,
        //                                                   CaseInforcedDate = x.CaseInforcedDate,
        //                                                   EISSPNumber = x.EISSPNumber,
        //                                                   CaseSessions = x.CaseSessions.Where(p => p.DateExpired == null)
        //                                                                                .Select(p => new CaseSessionElectronicFolderVM()
        //                                                                                {
        //                                                                                    Id = p.Id,
        //                                                                                    CourtId = p.CourtId ?? 0,
        //                                                                                    SessionTypeLabel = (p.SessionType != null) ? p.SessionType.Label : string.Empty,
        //                                                                                    CourtHallName = (p.CourtHall != null) ? p.CourtHall.Name : string.Empty,
        //                                                                                    SessionStateLabel = p.SessionState.Label,
        //                                                                                    DateFrom = p.DateFrom,
        //                                                                                    DateTo = p.DateTo,
        //                                                                                    Description = p.Description,
        //                                                                                    DateTo_Minutes = Convert.ToInt32(((TimeSpan)(p.DateTo ?? p.DateFrom).Subtract(p.DateFrom)).TotalMinutes),
        //                                                                                    CaseSessionNotificationLists = queryCaseSessionNotificationList.Where(nl => nl.CaseSessionId == p.Id)
        //                                                                                                                                                   .Select(nl => new CaseSessionNotificationListVM()
        //                                                                                                                                                   {
        //                                                                                                                                                       Id = nl.Id,
        //                                                                                                                                                       CaseSessionId = nl.CaseSessionId,
        //                                                                                                                                                       PersonName = (nl.CasePersonId != null) ? nl.CasePerson.FullName : nl.CaseLawUnit.LawUnit.FullName,
        //                                                                                                                                                       PersonRole = (nl.CasePersonId != null) ? nl.CasePerson.PersonRole.Label : nl.CaseLawUnit.JudgeRole.Label,
        //                                                                                                                                                       PersonId = (nl.CasePersonId != null) ? nl.CasePerson.Id : nl.CaseLawUnit.Id,
        //                                                                                                                                                       RowNumber = nl.RowNumber,
        //                                                                                                                                                       NotificationPersonType = nl.NotificationPersonType,
        //                                                                                                                                                       AddressString = nl.NotificationAddress.FullAddressNotification(),
        //                                                                                                                                                       NotificationListTypeId = nl.NotificationListTypeId
        //                                                                                                                                                   })
        //                                                                                                                                                   .ToList(),
        //                                                                                    CaseSessionActs = p.CaseSessionActs
        //                                                                                                       .Where(a => a.ActDeclaredDate != null &&
        //                                                                                                                   a.DateExpired == null)
        //                                                                                                       .Select(a => new CaseSessionActVM()
        //                                                                                                       {
        //                                                                                                           Id = a.Id,
        //                                                                                                           CaseSessionId = a.CaseSessionId,
        //                                                                                                           CaseId = a.CaseSession.CaseId,
        //                                                                                                           CaseSessionLabel = (a.CaseSession != null) ? a.CaseSession.SessionType.Label + "/" + a.CaseSession.DateFrom.ToString("dd.MM.yyyy HH:mm") : string.Empty,
        //                                                                                                           CaseLabel = a.CaseSession.Case.RegNumber,
        //                                                                                                           ActTypeLabel = (a.ActType != null) ? a.ActType.Label : string.Empty,
        //                                                                                                           ActTypeId = a.ActTypeId,
        //                                                                                                           ActStateLabel = (a.ActState != null) ? a.ActState.Label + (a.ActCoordination.Any(c => c.CoordinationDeclaredDate != null) ? " (ОМ)" : string.Empty) : string.Empty,
        //                                                                                                           RegNumber = a.RegNumber,
        //                                                                                                           RegNumberNew = a.RegNumber,
        //                                                                                                           RegDate = a.RegDate.Value,
        //                                                                                                           IsFinalDoc = a.IsFinalDoc,
        //                                                                                                           DateWrt = a.DateWrt,
        //                                                                                                           EcliCode = a.EcliCode,
        //                                                                                                           Description = a.Description,
        //                                                                                                           ActDeclaredDate = a.ActDeclaredDate,
        //                                                                                                           ActInforcedDate = a.ActInforcedDate,
        //                                                                                                           ActMotivesDeclaredDate = a.ActMotivesDeclaredDate,
        //                                                                                                           ActCoordinationLabel = a.ActCoordination.Any(c => c.CoordinationDeclaredDate != null) ? "ОМ" : string.Empty
        //                                                                                                       })
        //                                                                                                       .ToList(),
        //                                                                                    SessionMeetings = p.CaseSessionMeetings
        //                                                                                                       .Where(meet => meet.DateExpired == null)
        //                                                                                                       .Select(meet => new CaseSessionMeetingVM()
        //                                                                                                       {
        //                                                                                                           Id = meet.Id,
        //                                                                                                           CaseSessionId = meet.CaseSessionId,
        //                                                                                                           DateFrom = meet.DateFrom,
        //                                                                                                           DateTo = meet.DateTo,
        //                                                                                                           Description = meet.Description,
        //                                                                                                           SessionMeetingTypeLabel = meet.SessionMeetingType.Label,
        //                                                                                                           UsersNames = string.Join("; ", meet.CaseSessionMeetingUsers
        //                                                                                                                                              .Select(u => u.SecretaryUser.LawUnit.FullName))
        //                                                                                                       })
        //                                                                                                       .ToList(),
        //                                                                                    CaseSessionDocs = p.CaseSessionDocs
        //                                                                                                       .Where(d => d.DateExpired == null)
        //                                                                                                       .Select(d => new CaseSessionDocVM()
        //                                                                                                       {
        //                                                                                                           Id = d.Id,
        //                                                                                                           CaseSessionId = d.CaseSessionId,
        //                                                                                                           DocumentId = d.DocumentId,
        //                                                                                                           DocumentLabel = (d.Document != null) ? "Вх.№ " + d.Document.DocumentNumber + "/" + d.Document.DocumentDate.ToString("dd.MM.yyyy") + " " + d.Document.DocumentType.Label : string.Empty,
        //                                                                                                           SessionDocStateLabel = (d.SessionDocState != null) ? d.SessionDocState.Label : string.Empty,
        //                                                                                                       })
        //                                                                                                       .ToList(),
        //                                                                                    SessionResultText = string.Join(", ", p.CaseSessionResults.Where(r => r.DateExpired == null).Select(r => r.SessionResult.Label)),
        //                                                                                    CaseNotifications = queryCaseNotification.Where(n => n.CaseSessionId == p.Id)
        //                                                                                                                             .Select(n => new CaseNotificationVM()
        //                                                                                                                             {
        //                                                                                                                                 Id = n.Id,
        //                                                                                                                                 CaseId = n.CaseId,
        //                                                                                                                                 CaseSessionId = n.CaseSessionId,
        //                                                                                                                                 CaseSessionActId = n.CaseSessionActId,
        //                                                                                                                                 NotificationTypeLabel = (n.NotificationType != null) ? n.NotificationType.Label : string.Empty,
        //                                                                                                                                 NotificationTypeId = n.NotificationTypeId,
        //                                                                                                                                 CasePersonName = (n.IsMultiLink == true && n.CaseNotificationMLinks != null) ? string.Join("<br>", n.CaseNotificationMLinks.Where(l => l.IsActive && l.IsChecked).Select(m => m.PersonSummonedName)) + "<br>  чрез: " + n.NotificationPersonName
        //                                                                                                                                                                                                              : n.NotificationPersonName,
        //                                                                                                                                 NotificationStateLabel = (n.NotificationState != null) ? n.NotificationState.Label : string.Empty,
        //                                                                                                                                 HtmlTemplateLabel = (n.HtmlTemplate != null) ? n.HtmlTemplate.Label : string.Empty,
        //                                                                                                                                 RegNumber = n.RegNumber,
        //                                                                                                                                 RegDate = n.RegDate,
        //                                                                                                                                 NotificationNumber = n.NotificationNumber,
        //                                                                                                                                 NotificationDeliveryGroupLabel = queryNotificationDeliveryGroup.Where(g => g.Id == n.NotificationDeliveryGroupId)
        //                                                                                                                                                                                                .Select(g => g.Label)
        //                                                                                                                                                                                                .FirstOrDefault()
        //                                                                                                                             })
        //                                                                                                                             .ToList(),
        //                                                                                    SessionFastDocuments = queryCaseSessionFastDocument.Where(d => d.CaseSessionId == p.Id)
        //                                                                                                                                       .Select(d => new CaseSessionFastDocumentVM()
        //                                                                                                                                       {
        //                                                                                                                                           Id = d.Id,
        //                                                                                                                                           CasePersonName = d.CasePerson.FullName + " " + "(" + d.CasePerson.PersonRole.Label + ")",
        //                                                                                                                                           SessionDocTypeLabel = d.SessionDocType.Label,
        //                                                                                                                                           SessionDocStateLabel = d.SessionDocState.Label,
        //                                                                                                                                           CaseSessionFastDocumentInitDateSession = ((d.CaseSessionFastDocumentInit != null) ? (d.CaseSessionFastDocumentInit.CaseSession.DateFrom) : (DateTime?)null),
        //                                                                                                                                           DateSession = p.DateFrom
        //                                                                                                                                       })
        //                                                                                                                                       .ToList(),
        //                                                                                    JudgeRapporteur = p.CaseLawUnits
        //                                                                                                       .Where(l => l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
        //                                                                                                       .Select(l => l.LawUnit.FullName + ((l.CourtDepartmentId != null) ? " състав: " + l.CourtDepartment.Label : string.Empty))
        //                                                                                                       .FirstOrDefault(),
        //                                                                                    SessionStateString = string.Join(" ", p.CaseSessionResults.Where(r => r.DateExpired == null)
        //                                                                                                                                              .Select(r => r.SessionResult.Label + (r.SessionResultBaseId != null ? " " + r.SessionResultBase.Label : string.Empty) + ";")),
        //                                                                                    Prokuror = string.Join("; ", p.CasePersons
        //                                                                                                                  .Where(l => l.DateExpired == null &&
        //                                                                                                                              l.PersonRoleId == NomenclatureConstants.PersonRole.Prokuror)
        //                                                                                                                  .Select(l => l.Person.FullName)),
        //                                                                                })
        //                                                                                .ToList(),
        //                                                   CaseLawUnits = x.CaseLawUnits
        //                                                                   .Where(l => l.CaseSessionId == null)
        //                                                                   .Select(l => new CaseLawUnitVM()
        //                                                                   {
        //                                                                       Id = l.Id,
        //                                                                       CaseId = l.CaseId,
        //                                                                       CaseSessionId = l.CaseSessionId,
        //                                                                       LawUnitId = l.LawUnitId,
        //                                                                       LawUnitUserId = l.LawUnitUserId,
        //                                                                       JudgeRoleId = l.JudgeRoleId,
        //                                                                       JudgeRoleLabel = (l.JudgeRole != null) ? l.JudgeRole.Label : string.Empty,
        //                                                                       JudgeDepartmentRoleId = l.JudgeDepartmentRoleId,
        //                                                                       JudgeDepartmentRoleLabel = (l.JudgeDepartmentRole != null) ? l.JudgeDepartmentRole.Label : string.Empty,
        //                                                                       LawUnitName = (l.LawUnit != null) ? l.LawUnit.FullName : string.Empty,
        //                                                                       LawUnitNameShort = (l.LawUnit != null) ? l.LawUnit.FullName_MiddleNameInitials : string.Empty,
        //                                                                       LawUnitNameInitials = (l.LawUnit != null) ? l.LawUnit.FirstNameInitial_Family : string.Empty,
        //                                                                       DepartmentLabel = (l.CourtDepartment != null) ? " " + l.CourtDepartment.Label : string.Empty,
        //                                                                       DateFrom = l.DateFrom,
        //                                                                       DateTo = l.DateTo,
        //                                                                       CaseSessionLabel = string.Empty,
        //                                                                       DepartmentId = l.CourtDepartmentId,
        //                                                                       SubstitutionId = l.LawUnitSubstitutionId,
        //                                                                       SubstitutedLawUnitId = (l.LawUnitSubstitution != null) ? l.LawUnitSubstitution.LawUnitId : l.LawUnitId,
        //                                                                       SubstitutedLawUnitName = (l.LawUnitSubstitution != null) ? l.LawUnitSubstitution.LawUnit.FullName : string.Empty
        //                                                                   })
        //                                                                   .ToList(),
        //                                                   CaseClassifications = queryCaseClassification.Where(c => c.CaseId == x.Id &&
        //                                                                                                            c.CaseSessionId == null)
        //                                                                                                .ToList(),
        //                                                   CaseMigrations = queryCaseMigration.Where(m => x.CaseMigrations.Any(d => d.InitialCaseId == m.InitialCaseId) &&
        //                                                                                                  m.Case.CaseStateId != NomenclatureConstants.CaseState.Deleted &&
        //                                                                                                  m.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing &&
        //                                                                                                  m.CaseId != x.Id)
        //                                                                                      .OrderBy(m => m.Id)
        //                                                                                      .Select(m => new CaseMigrationVM
        //                                                                                      {
        //                                                                                          Id = m.Id,
        //                                                                                          InitialCaseId = m.InitialCaseId,
        //                                                                                          CaseId = m.CaseId,
        //                                                                                          CaseRegNumber = m.Case.RegNumber,
        //                                                                                          CaseRegDate = m.Case.RegDate,
        //                                                                                          CaseSessionAct = (m.CaseSessionAct != null) ? $"{m.CaseSessionAct.ActType.Label} {m.CaseSessionAct.RegNumber}/{m.CaseSessionAct.RegDate:dd.MM.yyyy}" : "",
        //                                                                                          CaseCourtName = m.Case.Court.Label,
        //                                                                                          MigrationDirection = m.CaseMigrationType.MigrationDirection,
        //                                                                                          MigrationTypeId = m.CaseMigrationTypeId,
        //                                                                                          MigrationTypeName = m.CaseMigrationType.Label,
        //                                                                                          SentFromName = (m.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing) ? m.Case.Court.Label : m.PriorCase.Court.Label,
        //                                                                                          SentToName = (m.SendToCourt != null) ? m.SendToCourt.Label : (m.SendToInstitution != null ? m.SendToInstitution.FullName : ""),
        //                                                                                          SendToCortId = m.SendToCourtId,
        //                                                                                          Description = m.Description,
        //                                                                                          CanEdit = m.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing && m.CaseId == caseId && !m.InCaseMigrations.Any(),
        //                                                                                          CanAccept = (m.MigrationKind == null) && m.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing && !m.InCaseMigrations.Any() && ((m.CaseId != caseId && m.SendToCourtId == userContext.CourtId) || CaseMigrationTypes.SendCaseTypesCanAccept.Contains(m.CaseMigrationTypeId)),
        //                                                                                          DateWrt = m.DateWrt,
        //                                                                                          CaseStateId = m.Case.CaseStateId,
        //                                                                                          CaseStateName = m.Case.CaseState.Label,
        //                                                                                          InitDocumentNumber = m.Case.Document.DocumentNumber,
        //                                                                                          InitDocumentDate = m.Case.Document.DocumentDate,
        //                                                                                          InitDocumentType = m.Case.Document.DocumentType.Label,
        //                                                                                          IsSendCompetence = m.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendCompetence,
        //                                                                                          MigrationKind = m.MigrationKind
        //                                                                                      })
        //                                                                                      .ToList(),
        //                                                   DocumentInstitutionCaseInfos = queryDocumentInstitutionCaseInfo.Where(d => d.DocumentId == x.DocumentId)
        //                                                                                                                  .ToList(),
        //                                                   PaymentCases = queryPayment.Where(p => p.ObligationPayments
        //                                                                                           .Any(a => a.IsActive == true &&
        //                                                                                                     a.Obligation.CaseSessionAct.CaseId == x.Id))
        //                                                                              .Select(p => new PaymentCaseVM()
        //                                                                              {
        //                                                                                  Id = p.Id,
        //                                                                                  //PersonNames = string.Join(", ", p.ObligationPayments
        //                                                                                  //                                 .Where(a => a.IsActive == true && a.Obligation.CaseSessionAct.CaseId == caseId)
        //                                                                                  //                                 .Select(a => a.Obligation.FullName).Distinct()),
        //                                                                                  PersonNames = string.Join(", ", p.ObligationPayments
        //                                                                                                                   .Where(a => a.IsActive == true && a.Obligation.CaseSessionAct.CaseId == caseId)
        //                                                                                                                   .Select(a => a.Obligation.FullName)),
        //                                                                                  AmountForCase = p.ObligationPayments
        //                                                                                                   .Where(a => a.IsActive == true && a.Obligation.CaseSessionAct.CaseId == caseId)
        //                                                                                                   .Select(a => a.Amount).Sum(),
        //                                                                                  AmountForPayment = p.Amount,
        //                                                                                  //MoneyTypeNames = string.Join(", ", p.ObligationPayments
        //                                                                                  //                                    .Where(a => a.IsActive == true && a.Obligation.CaseSessionAct.CaseId == caseId)
        //                                                                                  //                                    .Select(a => a.Obligation.MoneyType.Label).Distinct()),
        //                                                                                  MoneyTypeNames = string.Join(", ", p.ObligationPayments
        //                                                                                                                      .Where(a => a.IsActive == true && a.Obligation.CaseSessionAct.CaseId == caseId)
        //                                                                                                                      .Select(a => a.Obligation.MoneyType.Label)),
        //                                                                                  PaidDate = p.PaidDate,
        //                                                                                  PaymentTypeName = p.PaymentType.Label
        //                                                                              })
        //                                                                              .ToList(),
        //                                                   ExecLists = queryExecList.Where(e => e.ExecListObligations.Where(a => a.Obligation.CaseId == x.Id).Any() &&
        //                                                                                        e.RegDate != null)
        //                                                                            .Select(e => new ExecListVM()
        //                                                                            {
        //                                                                                Id = e.Id,
        //                                                                                RegNumber = e.RegNumber,
        //                                                                                RegDate = e.RegDate,
        //                                                                                ExecListTypeName = e.ExecListType.Label,
        //                                                                                //FullName = string.Join(",", e.ExecListObligations.Select(o => o.Obligation.FullName).Distinct()),
        //                                                                                //FullNameReceive = string.Join("<br>", e.ExecListObligations.Select(o => o.Obligation.ObligationReceives.Select(a => a.FullName).FirstOrDefault()).Distinct()),
        //                                                                                FullName = string.Join(",", e.ExecListObligations.Select(o => o.Obligation.FullName)),
        //                                                                                FullNameReceiveArray = e.ExecListObligations.Select(o => o.Obligation.ObligationReceives.Select(a => a.FullName).FirstOrDefault()).ToArray(),
        //                                                                                Amount = e.ExecListObligations.Select(o => o.Obligation.Amount).Sum(),
        //                                                                            })
        //                                                                            .ToList(),
        //                                                   ExpenseOrders = queryExpenseOrder.Where(e => e.ExpenseOrderObligations.Where(a => a.Obligation.CaseId == caseId).Any())
        //                                                                                    .Select(e => new ExpenseOrderVM()
        //                                                                                    {
        //                                                                                        Id = e.Id,
        //                                                                                        RegNumber = e.RegNumber,
        //                                                                                        RegDate = e.RegDate,
        //                                                                                        //FullName = string.Join(",", e.ExpenseOrderObligations.Select(o => o.Obligation.FullName).Distinct()),
        //                                                                                        FullName = string.Join(",", e.ExpenseOrderObligations.Select(o => o.Obligation.FullName)),
        //                                                                                        Amount = e.ExpenseOrderObligations.Select(o => o.Obligation.Amount).Sum(),
        //                                                                                    })
        //                                                                                    .ToList(),
        //                                                   DocumentDecisionCaseLists = queryDocumentDecisionCase.Where(d => d.CaseId == x.Id)
        //                                                                                                        .Select(d => new DocumentDecisionCaseListVM
        //                                                                                                        {
        //                                                                                                            Id = d.Id,
        //                                                                                                            CaseRegNumber = d.Case.RegNumber,
        //                                                                                                            CaseRegDate = d.Case.RegDate,
        //                                                                                                            DecisionName = d.DecisionType.Label,
        //                                                                                                            DecisionRequestTypeName = d.DecisionRequestType.Label,
        //                                                                                                            DocumentLable = d.DocumentDecision.Document.DocumentType.Label + " " + d.DocumentDecision.Document.DocumentNumber + "/" + d.DocumentDecision.Document.DocumentDate.ToString("dd.MM.yyyy"),
        //                                                                                                            DocumentShortLable = d.DocumentDecision.Document.DocumentNumber + "/" + d.DocumentDecision.Document.DocumentDate.ToString("dd.MM.yyyy"),
        //                                                                                                            DocumentId = d.DocumentDecision.DocumentId
        //                                                                                                        })
        //                                                                                                        .ToList(),
        //                                                   DocumentsOtherFromDifferentCourt = queryDocumentCaseInfo.Where(d => d.Document.CourtId != userContext.CourtId &&
        //                                                                                                                       d.CaseId == x.Id &&
        //                                                                                                                       d.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.InitialDocument &&
        //                                                                                                                       d.Document.DateExpired == null)
        //                                                                                                           .Select(d => new DocumentInfoVM
        //                                                                                                           {
        //                                                                                                               Id = d.Document.Id,
        //                                                                                                               Title = $"Вх.№ {d.Document.DocumentNumber}/{d.Document.DocumentDate:dd.MM.yyyy} {d.Document.DocumentType.Label}",
        //                                                                                                               DirectionId = d.Document.DocumentDirectionId,
        //                                                                                                               IsSecret = (d.Document.IsSecret ?? false),
        //                                                                                                               IsRestriction = d.Document.IsRestictedAccess,
        //                                                                                                               DocumentResolutions = d.Document.DocumentResolutions.Where(c => c.DateExpired == null && c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced).Select(c => new DocumentResolutionListVM() { Id = c.Id, ResolutionTypeLabel = c.ResolutionType.Label, Label = c.ResolutionType.Label + " " + c.RegNumber + "/" + (c.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"), RegDate = c.RegDate, RegNumber = c.RegNumber }).ToList(),
        //                                                                                                               DocumentDate = d.Document.DocumentDate,
        //                                                                                                               CourtId = d.Document.CourtId,
        //                                                                                                               CourtLabel = d.Document.Court.Label
        //                                                                                                           })
        //                                                                                                           .ToList(),
        //                                                   DocumentsOtherFromSameCourt = queryDocumentCaseInfo.Where(d => d.Document.CourtId == userContext.CourtId &&
        //                                                                                                                  (d.CaseId ?? 0) == x.Id &&
        //                                                                                                                  d.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.InitialDocument &&
        //                                                                                                                  d.Document.DateExpired == null &&
        //                                                                                                                  d.DocumentId != x.DocumentId)
        //                                                                                                      .Select(d => new DocumentInfoVM
        //                                                                                                      {
        //                                                                                                          Id = d.Document.Id,
        //                                                                                                          Title = $"Вх.№ {d.Document.DocumentNumber}/{d.Document.DocumentDate:dd.MM.yyyy} {d.Document.DocumentType.Label}",
        //                                                                                                          DirectionId = d.Document.DocumentDirectionId,
        //                                                                                                          IsSecret = (d.Document.IsSecret ?? false),
        //                                                                                                          IsRestriction = d.Document.IsRestictedAccess,
        //                                                                                                          DocumentResolutions = d.Document.DocumentResolutions.Where(c => c.DateExpired == null && c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced).Select(c => new DocumentResolutionListVM() { Id = c.Id, ResolutionTypeLabel = c.ResolutionType.Label, Label = c.ResolutionType.Label + " " + c.RegNumber + "/" + (c.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"), RegDate = c.RegDate, RegNumber = c.RegNumber }).ToList(),
        //                                                                                                          DocumentDate = d.Document.DocumentDate
        //                                                                                                      })
        //                                                                                                      .ToList(),
        //                                                   WorkTaskReports = queryWorkTask.Where(w => (w.SourceType == SourceTypeSelectVM.Document && w.SourceId == x.DocumentId) ||
        //                                                                                              (w.SourceType == SourceTypeSelectVM.Case && w.SourceId == x.Id) ||
        //                                                                                              (w.SourceType == SourceTypeSelectVM.DocumentResolution && x.Document
        //                                                                                                                                                         .DocumentResolutions
        //                                                                                                                                                         .Any(c => c.DateExpired == null && c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced &&
        //                                                                                                                                                                   c.Id == w.SourceId)))
        //                                                                                  .Select(w => new WorkTaskReportVM()
        //                                                                                  {
        //                                                                                      Id = w.Id,
        //                                                                                      TaskTypeId = w.TaskTypeId,
        //                                                                                      TaskTypeName = w.TaskType.Label,
        //                                                                                      DateCreated = w.DateCreated,
        //                                                                                      DateCompleted = w.DateCompleted,
        //                                                                                      DescriptionCreated = w.DescriptionCreated,
        //                                                                                      Description = w.Description
        //                                                                                  })
        //                                                                                  .ToList(),
        //                                                   DocumentResolutionCase = queryDocumentResolutionCase.Where(d => d.CaseId == x.Id &&
        //                                                                                                                   d.DocumentResolution.ResolutionTypeId == DocumentConstants.ResolutionTypes.ResolutionForSelection)
        //                                                                                                       .Select(d => new DocumentResolutionListVM()
        //                                                                                                       {
        //                                                                                                           Id = d.DocumentResolution.Id,
        //                                                                                                           ResolutionTypeId = d.DocumentResolution.ResolutionTypeId,
        //                                                                                                           ResolutionTypeLabel = d.DocumentResolution.ResolutionType.Label,
        //                                                                                                           Label = d.DocumentResolution.ResolutionType.Label + " " + d.DocumentResolution.RegNumber + "/" + (d.DocumentResolution.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"),
        //                                                                                                           RegDate = d.DocumentResolution.RegDate,
        //                                                                                                           RegNumber = d.DocumentResolution.RegNumber
        //                                                                                                       })
        //                                                                                                       .ToList(),
        //                                                   MoneyDocument = queryObligation.Where(oblg => oblg.DocumentId != null &&
        //                                                                                                 (oblg.DocumentId == x.DocumentId ||
        //                                                                                                  queryDocumentCaseInfo.Any(i => i.CaseId == x.Id &&
        //                                                                                                                                   i.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument &&
        //                                                                                                                                   i.DocumentId == oblg.DocumentId)))
        //                                                                                  .Select(oblg => new ObligationVM()
        //                                                                                  {
        //                                                                                      Id = oblg.Id,
        //                                                                                      ObligationNumber = oblg.ObligationNumber,
        //                                                                                      ObligationDate = oblg.ObligationDate,
        //                                                                                      CasePersonUic = oblg.Uic,
        //                                                                                      CasePersonName = oblg.FullName,
        //                                                                                      MoneyTypeName = oblg.MoneyType.Label,
        //                                                                                      Amount = oblg.Amount,
        //                                                                                      AmountPay = oblg.ObligationPayments.Where(a => a.IsActive == true).Select(a => a.Amount).Sum(),
        //                                                                                      IsActive = oblg.IsActive ?? true,
        //                                                                                      RegNumberExpenseOrder = string.Empty,
        //                                                                                      RegNumberExecList = string.Empty,
        //                                                                                      ExecListId = 0,
        //                                                                                      ExpenseOrderId = 0,
        //                                                                                      DocumentNumber = oblg.DocumentId > 0 ? oblg.Document.DocumentNumber : "",
        //                                                                                      DocumentDate = oblg.DocumentId > 0 ? oblg.Document.DocumentDate : (DateTime?)null,
        //                                                                                      DocumentTypeLabel = oblg.DocumentId > 0 ? oblg.Document.DocumentType.Label : string.Empty,
        //                                                                                      DocumentId = oblg.DocumentId
        //                                                                                  })
        //                                                                                  .ToList()
        //                                               })
        //                                               .AsSplitQuery()
        //                                               .FirstAsync();

        //        return caseElectronicFolderVM;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, $"Грешка при caseElectronicFolderVM");
        //        return null;
        //    }
        //}

        /// <summary>
        /// Метод извличащ CaseElectronicFolderVM за ел. папка
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<CaseElectronicFolderVM> GetCaseElectronicFolder(int caseId)
        {
            DateTime dateNow = DateTime.Now;
            DateTime dateNow100 = dateNow.AddYears(100);

            return await readonlyrepo.AllReadonly<Case>()
                                     .Where(x => x.Id == caseId)
                                     .Select(x => new CaseElectronicFolderVM()
                                     {
                                         Id = x.Id,
                                         CourtId = x.CourtId,
                                         IsOnlyFiles = x.CourtId != userContext.CourtId,
                                         CourtLabel = x.Court.Label,
                                         CaseGroupLabel = x.CaseGroup.Label,
                                         CaseTypeLabel = x.CaseType.Code,
                                         CaseCodeLabel = x.CaseCode.Code + " " + x.CaseCode.Label,
                                         CaseStateLabel = x.CaseState.Label,
                                         RegNumber = x.RegNumber,
                                         RegDate = x.RegDate,
                                         JudgeRapporteur = x.CaseLawUnits
                                                            .Where(l => l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                            .Where(l => (l.DateTo ?? dateNow100) >= dateNow)
                                                            .Where(l => l.CaseSessionId == null)
                                                            .Select(l => l.LawUnit.FullName + ((l.CourtDepartmentId != null) ? " състав: " + l.CourtDepartment.Label : string.Empty))
                                                            .FirstOrDefault(),
                                         RegNumberText = x.CaseStateId == NomenclatureConstants.CaseState.Rejected ? "Отказ от образуване"
                                                                                                                   : string.Concat(x.CaseType.Code, " ", x.ShortNumber, "/", x.RegDate.ToString("yyyy")),
                                         DocumentId = x.DocumentId,
                                         DocumentDate = x.Document.DocumentDate,
                                         DocumentLabel = "Вх.№ " + x.Document.DocumentNumber + "/" + x.Document.DocumentDate.ToString("dd.MM.yyyy") + " " + x.Document.DocumentType.Label,
                                         DocumentShortLabel = x.Document.DocumentNumber + "/" + x.Document.DocumentDate.ToString("dd.MM.yyyy"),
                                         DocumentDescription = x.Document.Description,
                                         DocumentSecret = x.Document.IsSecret ?? false,
                                         DocumentRestriction = x.Document.IsRestictedAccess,
                                         AssignmentDocumentId = x.Document.AssignmentDocumentId != null ? x.Document.AssignmentDocumentId : null,
                                         AssignmentDocumentDate = x.Document.AssignmentDocumentId != null ? x.Document.AssignmentDocument.DocumentDate : null,
                                         AssignmentDocumentLabel = x.Document.AssignmentDocumentId != null ? "Вх.№ " + x.Document.AssignmentDocument.DocumentNumber + "/" + x.Document.AssignmentDocument.DocumentDate.ToString("dd.MM.yyyy") + " " + x.Document.AssignmentDocument.DocumentType.Label + " (Централна регистратура)" : null,
                                         AssignmentDocumentShortLabel = x.Document.AssignmentDocumentId != null ? x.Document.AssignmentDocument.DocumentNumber + "/" + x.Document.AssignmentDocument.DocumentDate.ToString("dd.MM.yyyy") : null,
                                         AssignmentDocumentDescription = x.Document.AssignmentDocumentId != null ? x.Document.AssignmentDocument.Description : null,
                                         CaseReasonLabel = (x.CaseReasonId != null) ? x.CaseReason.Label : string.Empty,
                                         CaseStateDescription = x.CaseStateDescription,
                                         DocumentCaseInfos = x.Document.DocumentCaseInfo.Where(f => (f.IsLegacyCase ?? false)).ToList(),
                                         ArchRegNumber = x.CaseArchives.Select(a => a.RegNumber).FirstOrDefault(),
                                         ArchRegDate = x.CaseArchives.Select(a => (DateTime?)a.RegDate).FirstOrDefault() ?? dateNow,
                                         CaseInforcedDate = x.CaseInforcedDate,
                                         EISSPNumber = x.EISSPNumber,
                                     })
                                     .FirstAsync();
        }

        /// <summary>
        /// Метод извличащ заседания за ел. папка
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task<List<CaseSessionElectronicFolderVM>> GetCaseSessionElectronicFolder(int caseId)
        {
            DateTime dateNow = DateTime.Now;
            DateTime dateNow100 = dateNow.AddYears(100);

            return await readonlyrepo.AllReadonly<CaseSession>()
                                     .Where(p => p.CaseId == caseId &&
                                                 p.DateExpired == null)
                                     .Select(p => new CaseSessionElectronicFolderVM()
                                     {
                                         Id = p.Id,
                                         CourtId = p.CourtId ?? 0,
                                         SessionTypeLabel = p.SessionType.Label,
                                         CourtHallName = (p.CourtHallId != null) ? p.CourtHall.Name : string.Empty,
                                         SessionStateLabel = p.SessionState.Label,
                                         DateFrom = p.DateFrom,
                                         DateTo = p.DateTo,
                                         Description = p.Description,
                                         SessionResultText = string.Join(", ", p.CaseSessionResults.Where(r => r.DateExpired == null).Select(r => r.SessionResult.Label)),
                                         JudgeRapporteur = p.CaseLawUnits
                                                            .Where(l => l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                            .Where(l => (l.DateTo ?? dateNow100) >= dateNow)
                                                            .Where(l => l.CaseSessionId == p.Id)
                                                            .Select(l => l.LawUnit.FullName + ((l.CourtDepartmentId != null) ? " състав: " + l.CourtDepartment.Label : string.Empty))
                                                            .FirstOrDefault(),
                                         SessionStateString = string.Join(" ", p.CaseSessionResults.Where(r => r.DateExpired == null)
                                                                                                   .Select(r => r.SessionResult.Label + (r.SessionResultBaseId != null ? " " + r.SessionResultBase.Label : string.Empty) + ";")),
                                         Prokuror = string.Join("; ", p.CasePersons
                                                                       .Where(l => l.DateExpired == null &&
                                                                                   l.PersonRoleId == NomenclatureConstants.PersonRole.Prokuror)
                                                                       .Select(l => l.Person.FullName))
                                     })
                                     .ToListAsync();
        }


        /// <summary>
        /// Метод извличащ DocumentResolutionListVM за ел. папка
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        private async Task<List<DocumentResolutionListVM>> GetDocumentResolutionsEF(long documentId)
        {
            var queryDocumentTemplate = GetQueryDocumentTemplate();

            return await readonlyrepo.AllReadonly<DocumentResolution>()
                                     .Where(c => c.DocumentId == documentId &&
                                                 c.DateExpired == null &&
                                                 c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced)
                                     .Select(c => new DocumentResolutionListVM()
                                     {
                                         Id = c.Id,
                                         ResolutionTypeId = c.ResolutionTypeId,
                                         ResolutionTypeLabel = c.ResolutionType.Label,
                                         Label = c.ResolutionType.Label + " " + c.RegNumber + "/" + (c.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"),
                                         RegDate = c.RegDate,
                                         RegNumber = c.RegNumber,
                                         DocumentTemplateVMs = queryDocumentTemplate.Where(d => d.SourceType == SourceTypeSelectVM.DocumentResolution &&
                                                                                                d.SourceId == c.Id)
                                                                                    .OrderByDescending(d => d.DateWrt)
                                                                                    .Select(d => new DocumentTemplateVM()
                                                                                    {
                                                                                        Id = d.Id,
                                                                                        AuthorName = d.Author.LawUnit.FullName,
                                                                                        DocumentTypeLabel = d.DocumentType.Label,
                                                                                        DateWrt = d.DateWrt,
                                                                                        StateName = d.DocumentTemplateState.Label,
                                                                                        DocumentId = d.DocumentId,
                                                                                        DocumentNumber = (d.DocumentId != null) ? $"{d.Document.DocumentNumber}/{d.Document.DocumentDate:dd.MM.yyyy}" : "",
                                                                                        HtmlTemplateName = d.HtmlTemplateId == null ? "Общ формуляр" : d.HtmlTemplate.Label
                                                                                    })
                                                                                    .ToList()
                                     })
                                     .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ свързани документи към иницииращ такъв за ел. папка
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        private async Task<List<DocumentLinkEFVM>> GetDocumentLinkEF(long documentId)
        {
            return await readonlyrepo.AllReadonly<DocumentLink>()
                                     .Where(d => d.DocumentId == documentId)
                                     .Select(d => new DocumentLinkEFVM()
                                     {
                                         Id = d.Id,
                                         DocumentId = d.DocumentId,
                                         PrevDocumentCourtLabel = d.Court.Label,
                                         PrevDocumentDirectionLabel = d.DocumentDirection.Label,
                                         PrevDocumentLabel = d.PrevDocument.DocumentNumber + "/" + d.PrevDocument.DocumentDate.ToString("dd.MM.yyyy") + " " + d.PrevDocument.DocumentType.Label
                                     })
                                     .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ CaseSelectionProtokolListVM за ел. папка
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<CaseSelectionProtokolListVM>> GetCaseSelectionProtokolsEF(int caseId)
        {
            var queryProtokol = GetQueryProtokol();

            return await queryProtokol.Where(p => p.CaseId == caseId)
                                      .Select(p => new CaseSelectionProtokolListVM()
                                      {
                                          Id = p.Id,
                                          SelectionDate = p.SelectionDate,
                                          JudgeRoleName = p.JudgeRole.Label,
                                          SelectionModeName = p.SelectionMode.Label,
                                          SelectedLawUnitName = p.SelectedLawUnitId == null ? string.Empty : p.SelectedLawUnit.FullName,
                                          SelectionProtokolStateName = p.SelectionProtokolState.Label
                                      })
                                      .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ DocumentInfoVM за ел. папка
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<DocumentInfoVM>> GetCaseInDocumentsEF(int caseId)
        {
            var queryDocumentCaseInfo = GetQueryDocumentCaseInfo();
            var queryWorkTask = GetQueryWorkTask();

            int courtId = userContext.CourtId;

            return await queryDocumentCaseInfo.Where(i => i.CaseId == caseId &&
                                                          i.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument)
                                              .Select(i => new DocumentInfoVM()
                                              {
                                                  Id = i.DocumentId,
                                                  Title = $"Вх.№ {i.Document.DocumentNumber}/{i.Document.DocumentDate:dd.MM.yyyy} {i.Document.DocumentType.Label} {(i.Document.CourtId != courtId ? $" - входиран в: {i.Document.Court.Label}" : string.Empty)}",
                                                  DirectionId = i.Document.DocumentDirectionId,
                                                  IsSecret = i.Document.IsSecret ?? false,
                                                  IsRestriction = i.Document.IsRestictedAccess,
                                                  DocumentResolutions = i.Document
                                                                         .DocumentResolutions
                                                                         .Where(c => c.DateExpired == null &&
                                                                                     c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced)
                                                                         .Select(c => new DocumentResolutionListVM()
                                                                         {
                                                                             Id = c.Id,
                                                                             ResolutionTypeLabel = c.ResolutionType.Label,
                                                                             Label = c.ResolutionType.Label + " " + c.RegNumber + "/" + (c.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"),
                                                                             RegDate = c.RegDate,
                                                                             RegNumber = c.RegNumber
                                                                         })
                                                                         .ToList(),
                                                  DocumentDate = i.Document.DocumentDate,
                                                  DocumentTemplateLabel = i.Document
                                                                           .DocumentTemplates
                                                                           .Any(d => d.DateExpired == null) ? i.Document
                                                                                                               .DocumentTemplates
                                                                                                               .Where(d => d.DateExpired == null)
                                                                                                               .Select(t => t.HtmlTemplate.Label)
                                                                                                               .FirstOrDefault() : string.Empty,
                                                  DocumentTemplateDescription = i.Document
                                                                                 .DocumentTemplates
                                                                                 .Any(d => d.DateExpired == null) ? i.Document
                                                                                                                     .DocumentTemplates
                                                                                                                     .Select(d => d.Description)
                                                                                                                     .FirstOrDefault() : string.Empty,
                                                  DocumentPersonLabel = string.Join(", ", i.Document.DocumentPersons.Select(p => p.FullName)),
                                                  Description = (i.Description ?? string.Empty) + (!string.IsNullOrEmpty(i.Document.Description) ? (!string.IsNullOrEmpty(i.Description) ? " " : string.Empty) + i.Document.Description : string.Empty),
                                                  WorkTasks = queryWorkTask.Where(w => w.SourceId == i.DocumentId)
                                                                           .Select(w => new WorkTaskReportVM()
                                                                           {
                                                                               Id = w.Id,
                                                                               TaskTypeId = w.TaskTypeId,
                                                                               TaskTypeName = w.TaskType.Label,
                                                                               DateCreated = w.DateCreated,
                                                                               DateCompleted = w.DateCompleted,
                                                                               DescriptionCreated = w.DescriptionCreated,
                                                                               Description = w.Description
                                                                           })
                                                                           .ToList()
                                              })
                                              .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ RegixListVM за ел. папка
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<RegixListVM>> GetRegixReportsEF(int caseId)
        {
            var queryRegixReport = GetQueryRegixReport();

            return await queryRegixReport.Where(r => r.CaseId == caseId)
                                         .Select(r => new RegixListVM()
                                         {
                                             Id = r.Id,
                                             CaseId = r.CaseId ?? 0,
                                             RegixTypeName = r.RegixType.Label,
                                             UserName = r.User.LawUnit.FullName,
                                             CaseRegNumber = r.Case.RegNumber,
                                             DocumentNumber = r.DocumentId != null ? (r.Document.DocumentType.Label + " " +
                                                                                      r.Document.DocumentNumber + "/" +
                                                                                      r.Document.DocumentDate.ToString(FormattingConstant.NormalDateFormat)) : string.Empty,
                                             DocumentOnlyNumber = r.DocumentId != null ? (r.Document.DocumentNumber + "/" +
                                                                                          r.Document.DocumentDate.ToString(FormattingConstant.NormalDateFormat)) : string.Empty,
                                             ActRegNumber = r.CaseSessionActId != null ? (r.CaseSessionAct.ActType.Label + " " + r.CaseSessionAct.RegNumber + "/" + (r.CaseSessionAct.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy")) : string.Empty,
                                             DateWrt = r.DateWrt,
                                             RegixRequestTypeId = r.RegixRequestTypeId
                                         })
                                         .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ DocumentInfoVM за ел. папка
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<DocumentInfoVM>> GetCaseOutDocumentsEF(int caseId)
        {
            var queryDocumentCaseInfo = GetQueryDocumentCaseInfo();

            return await queryDocumentCaseInfo.Where(i => i.CaseId == caseId &&
                                                          i.Document.DocumentDirectionId == DocumentConstants.DocumentDirection.OutGoing)
                                              .Select(i => new DocumentInfoVM()
                                              {
                                                  Id = i.Document.Id,
                                                  Title = $"Изх.№ {i.Document.DocumentNumber}/{i.Document.DocumentDate:dd.MM.yyyy} {i.Document.DocumentType.Label}",
                                                  DirectionId = i.Document.DocumentDirectionId,
                                                  IsSecret = i.Document.IsSecret ?? false,
                                                  IsRestriction = i.Document.IsRestictedAccess,
                                                  DocumentResolutions = i.Document
                                                                         .DocumentResolutions
                                                                         .Where(c => c.DateExpired == null &&
                                                                                     c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced)
                                                                         .Select(c => new DocumentResolutionListVM()
                                                                         {
                                                                             Id = c.Id,
                                                                             ResolutionTypeLabel = c.ResolutionType.Label,
                                                                             Label = c.ResolutionType.Label + " " + c.RegNumber + "/" + (c.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"),
                                                                             RegDate = c.RegDate,
                                                                             RegNumber = c.RegNumber
                                                                         })
                                                                         .ToList(),
                                                  DocumentDate = i.Document.DocumentDate,
                                                  DocumentTemplateLabel = i.Document
                                                                           .DocumentTemplates
                                                                           .Any(d => d.DateExpired == null) ? i.Document
                                                                                                               .DocumentTemplates
                                                                                                               .Where(d => d.DateExpired == null)
                                                                                                               .Select(t => t.HtmlTemplate.Label)
                                                                                                               .FirstOrDefault() : string.Empty,
                                                  DocumentTemplateDescription = i.Document
                                                                                 .DocumentTemplates
                                                                                 .Any(d => d.DateExpired == null) ? i.Document
                                                                                                                     .DocumentTemplates
                                                                                                                     .Select(d => d.Description)
                                                                                                                     .FirstOrDefault() : string.Empty,
                                                  DocumentPersonLabel = string.Join(", ", i.Document.DocumentPersons.Select(p => p.FullName)),
                                                  Description = (i.Description ?? string.Empty) + (!string.IsNullOrEmpty(i.Document.Description) ? (!string.IsNullOrEmpty(i.Description) ? " " : string.Empty) + i.Document.Description : string.Empty),
                                              })
                                              .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ CasePersonListVM за ел. папка
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<CasePersonListVM>> GetCasePersonsEF(int caseId)
        {
            return await readonlyrepo.AllReadonly<CasePerson>()
                                     .Where(p => p.CaseId == caseId &&
                                                 p.CaseSessionId == null &&
                                                 p.DateExpired == null)
                                     .Select(p => new CasePersonListVM()
                                     {
                                         Id = p.Id,
                                         CaseId = p.CaseId,
                                         CaseSessionId = p.CaseSessionId,
                                         Uic = p.Uic,
                                         UicTypeId = p.UicTypeId,
                                         UicTypeLabel = p.UicType.Label,
                                         FirstName = p.FirstName,
                                         MiddleName = p.MiddleName,
                                         FamilyName = p.FamilyName,
                                         Family2Name = p.Family2Name,
                                         FullName = p.FullName,
                                         RoleName = p.PersonRole.Label,
                                         PersonRoleId = p.PersonRole.Id,
                                         PersonRoleLabel = p.PersonRole.Label,
                                         RoleKindId = p.PersonRole.RoleKindId,
                                         DateFrom = p.DateFrom,
                                         DateTo = p.DateTo,
                                         IsDeceased = p.IsDeceased,
                                         RowNumber = p.RowNumber
                                     })
                                     .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ CaseSessionActVM за ел. папка
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<CaseSessionActVM>> GetCaseSessionFinalActsEF(int caseId)
        {
            return await readonlyrepo.AllReadonly<CaseSessionAct>()
                                     .Where(a => a.CaseId == caseId &&
                                                 a.ActDeclaredDate != null)
                                     .Select(a => new CaseSessionActVM()
                                     {
                                         Id = a.Id,
                                         CaseSessionId = a.CaseSessionId,
                                         CaseId = a.CaseSession.CaseId,
                                         CaseSessionLabel = a.CaseSession.SessionType.Label + "/" + a.CaseSession.DateFrom.ToString("dd.MM.yyyy HH:mm"),
                                         CaseLabel = a.Case.RegNumber,
                                         ActTypeLabel = a.ActType.Label,
                                         ActTypeId = a.ActTypeId,
                                         ActStateLabel = a.ActState.Label + (a.ActCoordination.Any(c => c.CoordinationDeclaredDate != null) ? " (ОМ)" : string.Empty),
                                         RegNumber = a.RegNumber,
                                         RegNumberNew = a.RegNumber,
                                         RegDate = a.RegDate.Value,
                                         IsFinalDoc = a.IsFinalDoc,
                                         DateWrt = a.DateWrt,
                                         EcliCode = a.EcliCode,
                                         Description = a.Description,
                                         ActDeclaredDate = a.ActDeclaredDate,
                                         ActInforcedDate = a.ActInforcedDate,
                                         ActMotivesDeclaredDate = a.ActMotivesDeclaredDate,
                                         ActCoordinationLabel = a.ActCoordination.Any(c => c.CoordinationDeclaredDate != null) ? "ОМ" : string.Empty
                                     })
                                     .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ CaseLawUnitVM за ел. папка
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<CaseLawUnitVM>> GetCaseLawUnitsEF(int caseId)
        {
            return await readonlyrepo.AllReadonly<CaseLawUnit>()
                                     .Where(l => l.CaseId == caseId &&
                                                 l.CaseSessionId == null)
                                     .Select(l => new CaseLawUnitVM()
                                     {
                                         Id = l.Id,
                                         CaseId = l.CaseId,
                                         CaseSessionId = l.CaseSessionId,
                                         LawUnitId = l.LawUnitId,
                                         LawUnitUserId = l.LawUnitUserId,
                                         JudgeRoleId = l.JudgeRoleId,
                                         JudgeRoleLabel = l.JudgeRole.Label,
                                         JudgeDepartmentRoleId = l.JudgeDepartmentRoleId,
                                         JudgeDepartmentRoleLabel = (l.JudgeDepartmentRoleId != null) ? l.JudgeDepartmentRole.Label : string.Empty,
                                         LawUnitName = l.LawUnit.FullName,
                                         LawUnitNameShort = l.LawUnit.FullName_MiddleNameInitials,
                                         LawUnitNameInitials = l.LawUnit.FirstNameInitial_Family,
                                         DepartmentLabel = (l.CourtDepartmentId != null) ? " " + l.CourtDepartment.Label : string.Empty,
                                         DateFrom = l.DateFrom,
                                         DateTo = l.DateTo,
                                         CaseSessionLabel = string.Empty,
                                         DepartmentId = l.CourtDepartmentId,
                                         SubstitutionId = l.LawUnitSubstitutionId,
                                         SubstitutedLawUnitId = (l.LawUnitSubstitutionId != null) ? l.LawUnitSubstitution.LawUnitId : l.LawUnitId,
                                         SubstitutedLawUnitName = (l.LawUnitSubstitutionId != null) ? l.LawUnitSubstitution.LawUnit.FullName : string.Empty
                                     })
                                     .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ CaseMigrationVM за ел. папка
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<CaseMigrationVM>> GetCaseMigrationsEF(int caseId)
        {
            int[] connectedCaseIds = caseMigrationService.GetConnectedCasesByCaseId(caseId);

            return await repo.AllReadonly<Case>().Where(c => connectedCaseIds.Contains(c.Id) &&
                                                                               c.CaseStateId != NomenclatureConstants.CaseState.Deleted &&
                                                                               c.Id != caseId)
                                                                   .OrderBy(c => c.Id)
                                                                   .Select(c => new CaseMigrationVM
                                                                   {
                                                                       CaseId = c.Id,
                                                                       CaseRegNumber = c.RegNumber,
                                                                       CaseRegDate = c.RegDate,
                                                                       CaseCourtName = c.Court.Label,
                                                                       DateWrt = c.DateWrt,
                                                                       CaseStateId = c.CaseStateId,
                                                                       CaseStateName = c.CaseState.Label,
                                                                       InitDocumentNumber = c.Document.DocumentNumber,
                                                                       InitDocumentDate = c.Document.DocumentDate,
                                                                       InitDocumentType = c.Document.DocumentType.Label
                                                                   })
                                                                   .ToListAsync();


        }

        [Obsolete]
        private async Task<List<CaseMigrationVM>> OLD_GetCaseMigrationsEF(int caseId)
        {
            var queryCaseMigration = GetQueryCaseMigration();

            //int[] initCase = await queryCaseMigration.Where(d => d.CaseId == caseId).Select(d => d.InitialCaseId).ToArrayAsync();
            int[] initCase = caseMigrationService.GetInitialCasesByCaseId(caseId);

            List<CaseMigrationVM> result = await queryCaseMigration.Where(m => initCase.Contains(m.InitialCaseId) &&
                                                                               m.Case.CaseStateId != NomenclatureConstants.CaseState.Deleted &&
                                                                               !NomenclatureConstants.CaseMigrationTypes.CaseUnionConnection.Contains(m.CaseMigrationTypeId) &&
                                                                               m.CaseId != caseId)
                                                                   .OrderBy(m => m.Id)
                                                                   .Select(m => new CaseMigrationVM
                                                                   {
                                                                       Id = m.Id,
                                                                       InitialCaseId = m.InitialCaseId,
                                                                       CaseId = m.CaseId,
                                                                       CaseRegNumber = m.Case.RegNumber,
                                                                       CaseRegDate = m.Case.RegDate,
                                                                       CaseSessionAct = (m.CaseSessionActId != null) ? $"{m.CaseSessionAct.ActType.Label} {m.CaseSessionAct.RegNumber}/{m.CaseSessionAct.RegDate:dd.MM.yyyy}" : "",
                                                                       CaseCourtName = m.Case.Court.Label,
                                                                       MigrationDirection = m.CaseMigrationType.MigrationDirection,
                                                                       MigrationTypeId = m.CaseMigrationTypeId,
                                                                       MigrationTypeName = m.CaseMigrationType.Label,
                                                                       SentFromName = (m.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing) ? m.Case.Court.Label : m.PriorCase.Court.Label,
                                                                       SentToName = (m.SendToCourtId != null) ? m.SendToCourt.Label : (m.SendToInstitutionId != null ? m.SendToInstitution.FullName : ""),
                                                                       SendToCortId = m.SendToCourtId,
                                                                       Description = m.Description,
                                                                       CanEdit = m.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing && m.CaseId == caseId && !m.InCaseMigrations.Any(),
                                                                       CanAccept = (m.MigrationKind == null) && m.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing && !m.InCaseMigrations.Any() && ((m.CaseId != caseId && m.SendToCourtId == userContext.CourtId) || CaseMigrationTypes.SendCaseTypesCanAccept.Contains(m.CaseMigrationTypeId)),
                                                                       DateWrt = m.DateWrt,
                                                                       CaseStateId = m.Case.CaseStateId,
                                                                       CaseStateName = m.Case.CaseState.Label,
                                                                       InitDocumentNumber = m.Case.Document.DocumentNumber,
                                                                       InitDocumentDate = m.Case.Document.DocumentDate,
                                                                       InitDocumentType = m.Case.Document.DocumentType.Label,
                                                                       IsSendCompetence = m.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendCompetence,
                                                                       MigrationKind = m.MigrationKind
                                                                   })
                                                                   .ToListAsync();

            result.AddRange(await queryCaseMigration.Where(m => m.CaseId == caseId &&
                                                                m.PriorCase.CaseStateId != NomenclatureConstants.CaseState.Deleted &&
                                                                NomenclatureConstants.CaseMigrationTypes.CaseUnionConnection.Contains(m.CaseMigrationTypeId))
                                                    .Select(m => new CaseMigrationVM
                                                    {
                                                        Id = m.Id,
                                                        InitialCaseId = m.InitialCaseId,
                                                        CaseId = m.PriorCaseId,
                                                        CaseRegNumber = m.PriorCase.RegNumber,
                                                        CaseRegDate = m.PriorCase.RegDate,
                                                        CaseSessionAct = (m.CaseSessionActId != null) ? $"{m.CaseSessionAct.ActType.Label} {m.CaseSessionAct.RegNumber}/{m.CaseSessionAct.RegDate:dd.MM.yyyy}" : "",
                                                        CaseCourtName = m.PriorCase.Court.Label,
                                                        MigrationDirection = m.CaseMigrationType.MigrationDirection,
                                                        MigrationTypeId = m.CaseMigrationTypeId,
                                                        MigrationTypeName = m.CaseMigrationType.Label,
                                                        SentFromName = m.PriorCase.Court.Label,
                                                        SentToName = (m.SendToCourtId != null) ? m.SendToCourt.Label : (m.SendToInstitutionId != null ? m.SendToInstitution.FullName : ""),
                                                        SendToCortId = m.SendToCourtId,
                                                        Description = m.Description,
                                                        DateWrt = m.DateWrt,
                                                        CaseStateId = m.PriorCase.CaseStateId,
                                                        CaseStateName = m.PriorCase.CaseState.Label,
                                                        InitDocumentNumber = m.PriorCase.Document.DocumentNumber,
                                                        InitDocumentDate = m.PriorCase.Document.DocumentDate,
                                                        InitDocumentType = m.PriorCase.Document.DocumentType.Label,
                                                        IsSendCompetence = m.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendCompetence,
                                                        MigrationKind = m.MigrationKind
                                                    })
                                                    .ToListAsync());

            result.AddRange(await queryCaseMigration.Where(m => m.PriorCaseId == caseId &&
                                                                m.Case.CaseStateId != NomenclatureConstants.CaseState.Deleted &&
                                                                NomenclatureConstants.CaseMigrationTypes.CaseUnionConnection.Contains(m.CaseMigrationTypeId))
                                                    .Select(m => new CaseMigrationVM
                                                    {
                                                        Id = m.Id,
                                                        InitialCaseId = m.InitialCaseId,
                                                        CaseId = m.CaseId,
                                                        CaseRegNumber = m.Case.RegNumber,
                                                        CaseRegDate = m.Case.RegDate,
                                                        CaseSessionAct = (m.CaseSessionActId != null) ? $"{m.CaseSessionAct.ActType.Label} {m.CaseSessionAct.RegNumber}/{m.CaseSessionAct.RegDate:dd.MM.yyyy}" : "",
                                                        CaseCourtName = m.Case.Court.Label,
                                                        MigrationDirection = m.CaseMigrationType.MigrationDirection,
                                                        MigrationTypeId = m.CaseMigrationTypeId,
                                                        MigrationTypeName = m.CaseMigrationType.Label,
                                                        SentFromName = (m.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing) ? m.Case.Court.Label : m.PriorCase.Court.Label,
                                                        SentToName = (m.SendToCourtId != null) ? m.SendToCourt.Label : (m.SendToInstitutionId != null ? m.SendToInstitution.FullName : ""),
                                                        SendToCortId = m.SendToCourtId,
                                                        Description = m.Description,
                                                        DateWrt = m.DateWrt,
                                                        CaseStateId = m.Case.CaseStateId,
                                                        CaseStateName = m.Case.CaseState.Label,
                                                        InitDocumentNumber = m.Case.Document.DocumentNumber,
                                                        InitDocumentDate = m.Case.Document.DocumentDate,
                                                        InitDocumentType = m.Case.Document.DocumentType.Label,
                                                        IsSendCompetence = m.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendCompetence,
                                                        MigrationKind = m.MigrationKind
                                                    })
                                                    .ToListAsync());


            return result.GroupBy(x => x.CaseId)
                         .Select(g => g.FirstOrDefault())
                         .ToList();
        }

        /// <summary>
        /// Метод извличащ CaseClassification за ел. папка
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<CaseClassification>> GetCaseClassificationsEF(int caseId)
        {
            var queryCaseClassification = GetQueryCaseClassification();

            DateTime _date = DateTime.Now;

            return await queryCaseClassification.Where(c => c.CaseId == caseId &&
                                                            c.CaseSessionId == null &&
                                                            c.DateFrom <= _date &&
                                                            (c.DateTo ?? _date.AddYears(100)) >= _date)
                                                .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ DocumentInstitutionCaseInfo за ел. папка
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        private async Task<List<DocumentInstitutionCaseInfo>> GetDocumentInstitutionCaseInfosEF(long documentId)
        {
            var queryDocumentInstitutionCaseInfo = GetQueryDocumentInstitutionCaseInfo();

            return await queryDocumentInstitutionCaseInfo.Where(d => d.DocumentId == documentId)
                                                         .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ PaymentCaseVM за ел. папка
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<PaymentCaseVM>> GetPaymentCasesEF(int caseId)
        {
            var queryPayment = GetQueryPayment();

            return await queryPayment.Where(p => p.ObligationPayments
                                                  .Any(a => a.IsActive == true &&
                                                            a.Obligation.CaseSessionAct.CaseId == caseId))
                                     .Select(p => new PaymentCaseVM()
                                     {
                                         Id = p.Id,
                                         PersonNames = string.Join(", ", p.ObligationPayments
                                                                          .Where(a => a.IsActive == true && a.Obligation.CaseSessionAct.CaseId == caseId)
                                                                          .Select(a => a.Obligation.FullName)),
                                         AmountForCase = p.ObligationPayments
                                                          .Where(a => a.IsActive == true && a.Obligation.CaseSessionAct.CaseId == caseId)
                                                          .Select(a => a.Amount).Sum(),
                                         AmountForPayment = p.Amount,
                                         MoneyTypeNames = string.Join(", ", p.ObligationPayments
                                                                             .Where(a => a.IsActive == true && a.Obligation.CaseSessionAct.CaseId == caseId)
                                                                             .Select(a => a.Obligation.MoneyType.Label)),
                                         PaidDate = p.PaidDate,
                                         PaymentTypeName = p.PaymentType.Label
                                     })
                                     .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ ExecListVM за ел. папка
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<ExecListVM>> GetExecListsEF(int caseId)
        {
            var queryExecList = GetQueryExecList();
            return await queryExecList.Where(e => e.ExecListObligations.Where(a => a.Obligation.CaseId == caseId).Any() &&
                                                  e.RegDate != null)
                                      .Select(e => new ExecListVM()
                                      {
                                          Id = e.Id,
                                          RegNumber = e.RegNumber,
                                          RegDate = e.RegDate,
                                          ExecListTypeName = e.ExecListType.Label,
                                          FullName = string.Join(",", e.ExecListObligations.Select(o => o.Obligation.FullName)),
                                          FullNameReceiveArray = e.ExecListObligations.Select(o => o.Obligation.ObligationReceives.Select(a => a.FullName).FirstOrDefault()).ToArray(),
                                          Amount = e.ExecListObligations.Select(o => o.Obligation.Amount).Sum(),
                                      })
                                      .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ ExpenseOrderVM за ел. папка
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<ExpenseOrderVM>> GetExpenseOrdersEF(int caseId)
        {
            var queryExpenseOrder = GetQueryExpenseOrder();
            return await queryExpenseOrder.Where(e => e.ExpenseOrderObligations.Where(a => a.Obligation.CaseId == caseId).Any())
                                          .Select(e => new ExpenseOrderVM()
                                          {
                                              Id = e.Id,
                                              RegNumber = e.RegNumber,
                                              RegDate = e.RegDate,
                                              FullName = string.Join(",", e.ExpenseOrderObligations.Select(o => o.Obligation.FullName)),
                                              Amount = e.ExpenseOrderObligations.Select(o => o.Obligation.Amount).Sum(),
                                          })
                                          .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ DocumentDecisionCaseListVM за ел. папка
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<DocumentDecisionCaseListVM>> GetDocumentDecisionCaseListsEF(int caseId)
        {
            var queryDocumentDecisionCase = GetQueryDocumentDecisionCase();

            return await queryDocumentDecisionCase.Where(d => d.CaseId == caseId)
                                                  .Select(d => new DocumentDecisionCaseListVM
                                                  {
                                                      Id = d.Id,
                                                      CaseRegNumber = d.Case.RegNumber,
                                                      CaseRegDate = d.Case.RegDate,
                                                      DecisionName = d.DecisionType.Label,
                                                      DecisionRequestTypeName = d.DecisionRequestType.Label,
                                                      DocumentLable = d.DocumentDecision.Document.DocumentType.Label + " " + d.DocumentDecision.Document.DocumentNumber + "/" + d.DocumentDecision.Document.DocumentDate.ToString("dd.MM.yyyy"),
                                                      DocumentShortLable = d.DocumentDecision.Document.DocumentNumber + "/" + d.DocumentDecision.Document.DocumentDate.ToString("dd.MM.yyyy"),
                                                      DocumentId = d.DocumentDecision.DocumentId
                                                  })
                                                  .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ DocumentInfoVM за ел. папка
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<DocumentInfoVM>> GetDocumentsOtherFromDifferentCourtEF(int caseId)
        {
            var queryDocumentCaseInfo = GetQueryDocumentCaseInfo();
            return await queryDocumentCaseInfo.Where(d => d.Document.CourtId != userContext.CourtId &&
                                                          d.CaseId == caseId &&
                                                          d.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.InitialDocument &&
                                                          d.Document.DateExpired == null)
                                              .Select(d => new DocumentInfoVM
                                              {
                                                  Id = d.Document.Id,
                                                  Title = $"Вх.№ {d.Document.DocumentNumber}/{d.Document.DocumentDate:dd.MM.yyyy} {d.Document.DocumentType.Label}",
                                                  DirectionId = d.Document.DocumentDirectionId,
                                                  IsSecret = d.Document.IsSecret ?? false,
                                                  IsRestriction = d.Document.IsRestictedAccess,
                                                  DocumentResolutions = d.Document.DocumentResolutions.Where(c => c.DateExpired == null && c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced).Select(c => new DocumentResolutionListVM() { Id = c.Id, ResolutionTypeLabel = c.ResolutionType.Label, Label = c.ResolutionType.Label + " " + c.RegNumber + "/" + (c.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"), RegDate = c.RegDate, RegNumber = c.RegNumber }).ToList(),
                                                  DocumentDate = d.Document.DocumentDate,
                                                  CourtId = d.Document.CourtId,
                                                  CourtLabel = d.Document.Court.Label
                                              })
                                              .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ DocumentInfoVM за ел. папка
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        private async Task<List<DocumentInfoVM>> GetDocumentsOtherFromSameCourtEF(int caseId, long documentId)
        {
            var queryDocumentCaseInfo = GetQueryDocumentCaseInfo();

            return await queryDocumentCaseInfo.Where(d => d.Document.CourtId == userContext.CourtId &&
                                                          (d.CaseId ?? 0) == caseId &&
                                                          d.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.InitialDocument &&
                                                          d.Document.DateExpired == null &&
                                                          d.DocumentId != documentId)
                                              .Select(d => new DocumentInfoVM
                                              {
                                                  Id = d.Document.Id,
                                                  Title = $"Вх.№ {d.Document.DocumentNumber}/{d.Document.DocumentDate:dd.MM.yyyy} {d.Document.DocumentType.Label}",
                                                  DirectionId = d.Document.DocumentDirectionId,
                                                  IsSecret = d.Document.IsSecret ?? false,
                                                  IsRestriction = d.Document.IsRestictedAccess,
                                                  DocumentResolutions = d.Document.DocumentResolutions.Where(c => c.DateExpired == null && c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced).Select(c => new DocumentResolutionListVM() { Id = c.Id, ResolutionTypeLabel = c.ResolutionType.Label, Label = c.ResolutionType.Label + " " + c.RegNumber + "/" + (c.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"), RegDate = c.RegDate, RegNumber = c.RegNumber }).ToList(),
                                                  DocumentDate = d.Document.DocumentDate
                                              })
                                              .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ WorkTaskReportVM за ел. папка
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        private async Task<List<WorkTaskReportVM>> GetWorkTaskReportsEF(int caseId, long documentId)
        {
            var queryWorkTask = GetQueryWorkTask();
            var queryDocumentresolution = readonlyrepo.AllReadonly<DocumentResolution>()
                                                      .Where(c => c.DocumentId == documentId &&
                                                                  c.DateExpired == null);

            return await queryWorkTask.Where(w => (w.SourceType == SourceTypeSelectVM.Document && w.SourceId == documentId) ||
                                                  (w.SourceType == SourceTypeSelectVM.Case && w.SourceId == caseId) ||
                                                  (w.SourceType == SourceTypeSelectVM.DocumentResolution && queryDocumentresolution.Any(c => c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced &&
                                                                                                                                             c.Id == w.SourceId)))
                                      .Select(w => new WorkTaskReportVM()
                                      {
                                          Id = w.Id,
                                          TaskTypeId = w.TaskTypeId,
                                          TaskTypeName = w.TaskType.Label,
                                          DateCreated = w.DateCreated,
                                          DateCompleted = w.DateCompleted,
                                          DescriptionCreated = w.DescriptionCreated,
                                          Description = w.Description
                                      })
                                      .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ DocumentResolutionListVM за ел. папка
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<DocumentResolutionListVM>> GetDocumentResolutionCaseEF(int caseId)
        {
            var queryDocumentResolutionCase = GetQueryDocumentResolutionCase();

            return await queryDocumentResolutionCase.Where(d => d.CaseId == caseId &&
                                                                d.DocumentResolution.ResolutionTypeId == DocumentConstants.ResolutionTypes.ResolutionForSelection)
                                                    .Select(d => new DocumentResolutionListVM()
                                                    {
                                                        Id = d.DocumentResolution.Id,
                                                        ResolutionTypeId = d.DocumentResolution.ResolutionTypeId,
                                                        ResolutionTypeLabel = d.DocumentResolution.ResolutionType.Label,
                                                        Label = d.DocumentResolution.ResolutionType.Label + " " + d.DocumentResolution.RegNumber + "/" + (d.DocumentResolution.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"),
                                                        RegDate = d.DocumentResolution.RegDate,
                                                        RegNumber = d.DocumentResolution.RegNumber
                                                    })
                                                    .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ ObligationVM за ел. папка
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        private async Task<List<ObligationVM>> GetMoneyDocumentEF(int caseId, long documentId)
        {
            var queryObligation = GetQueryObligation();
            var queryDocumentCaseInfo = GetQueryDocumentCaseInfo();

            return await queryObligation.Where(oblg => oblg.DocumentId != null &&
                                                       (oblg.DocumentId == documentId ||
                                                        queryDocumentCaseInfo.Any(i => i.CaseId == caseId &&
                                                                                       i.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument &&
                                                                                       i.DocumentId == oblg.DocumentId)))
                                        .Select(oblg => new ObligationVM()
                                        {
                                            Id = oblg.Id,
                                            ObligationNumber = oblg.ObligationNumber,
                                            ObligationDate = oblg.ObligationDate,
                                            CasePersonUic = oblg.Uic,
                                            CasePersonName = oblg.FullName,
                                            MoneyTypeName = oblg.MoneyType.Label,
                                            Amount = oblg.Amount,
                                            AmountPay = oblg.ObligationPayments.Where(a => a.IsActive == true).Select(a => a.Amount).Sum(),
                                            IsActive = oblg.IsActive ?? true,
                                            RegNumberExpenseOrder = string.Empty,
                                            RegNumberExecList = string.Empty,
                                            ExecListId = 0,
                                            ExpenseOrderId = 0,
                                            DocumentNumber = oblg.DocumentId > 0 ? oblg.Document.DocumentNumber : "",
                                            DocumentDate = oblg.DocumentId > 0 ? oblg.Document.DocumentDate : (DateTime?)null,
                                            DocumentTypeLabel = oblg.DocumentId > 0 ? oblg.Document.DocumentType.Label : string.Empty,
                                            DocumentId = oblg.DocumentId
                                        })
                                        .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ ObligationVM за ел. папка
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        private async Task<List<ObligationVM>> GetMoneyDocumentFromMoneyEF(long documentId, long? assignmentDocumentId, ICollection<DocumentInfoVM> documents)
        {
            List<ObligationVM> result = await moneyService.Obligation_Select(0, documentId, 0, userContext.CourtId, assignmentDocumentId ?? 0).ToListAsync() ?? new();

            if (documents != null && documents.Count > 0)
            {
                foreach (var doc in documents)
                {
                    result.AddRange(await moneyService.Obligation_Select(0, doc.Id, 0, userContext.CourtId, 0).ToListAsync());
                }
            }

            return result;
        }

        /// <summary>
        /// Метод извличащ данните за срещи за медиация
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<MediationCaseSessionListDataVM>> GetMediationCaseSessions(int caseId)
        {
            return await repo.AllReadonly<MediationCaseSession>()
                             .Where(x => x.DateExpired == null)
                             .Where(x => x.CaseId == caseId)
                             .Select(x => new MediationCaseSessionListDataVM
                             {
                                 Id = x.Id,
                                 MediationTypeLabel = x.MediationType.Label,
                                 MediationStateLabel = x.MediationState.Label,
                                 DateFrom = x.DateFrom,
                                 DateTo = x.DateTo
                             })
                             .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ доказателства към дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<CaseEvidenceEFVM>> GetCaseEvidences(int caseId)
        {
            return await repo.AllReadonly<CaseEvidence>()
                             .Where(x => x.DateExpired == null)
                             .Where(x => x.CaseId == caseId)
                             .Select(x => new CaseEvidenceEFVM
                             {
                                 Id = x.Id,
                                 DateAccept = x.DateAccept,
                                 Description = x.Description,
                                 EvidenceStateLabel = x.EvidenceState.Label,
                                 EvidenceTypeLabel = x.EvidenceType.Label
                             })
                             .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ CaseSessionNotificationListVM за ел. папка
        /// </summary>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <returns></returns>
        private async Task<List<CaseSessionNotificationListVM>> GetCaseSessionNotificationListsEF(int caseSessionId)
        {
            var queryCaseSessionNotificationList = GetQueryCaseSessionNotificationList();

            return await queryCaseSessionNotificationList.Where(nl => nl.CaseSessionId == caseSessionId)
                                                         .Select(nl => new CaseSessionNotificationListVM()
                                                         {
                                                             Id = nl.Id,
                                                             CaseSessionId = nl.CaseSessionId,
                                                             PersonName = (nl.CasePersonId != null) ? nl.CasePerson.FullName : nl.CaseLawUnit.LawUnit.FullName,
                                                             PersonRole = (nl.CasePersonId != null) ? nl.CasePerson.PersonRole.Label : nl.CaseLawUnit.JudgeRole.Label,
                                                             PersonId = (nl.CasePersonId != null) ? nl.CasePerson.Id : nl.CaseLawUnit.Id,
                                                             RowNumber = nl.RowNumber,
                                                             NotificationPersonType = nl.NotificationPersonType,
                                                             AddressString = nl.NotificationAddress.FullAddressNotification(),
                                                             NotificationListTypeId = nl.NotificationListTypeId
                                                         })
                                                         .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ CaseSessionActVM за ел. папка
        /// </summary>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <returns></returns>
        private async Task<List<CaseSessionActVM>> GetCaseSessionActsEF(int caseSessionId)
        {
            return await readonlyrepo.AllReadonly<CaseSessionAct>()
                                     .Where(a => a.CaseSessionId == caseSessionId &&
                                                 a.ActDeclaredDate != null &&
                                                 a.DateExpired == null)
                                     .Select(a => new CaseSessionActVM()
                                     {
                                         Id = a.Id,
                                         CaseSessionId = a.CaseSessionId,
                                         CaseId = a.CaseSession.CaseId,
                                         CaseSessionLabel = a.CaseSession.SessionType.Label + "/" + a.CaseSession.DateFrom.ToString("dd.MM.yyyy HH:mm"),
                                         CaseLabel = a.Case.RegNumber,
                                         ActTypeLabel = a.ActType.Label,
                                         ActTypeId = a.ActTypeId,
                                         ActStateLabel = a.ActState.Label + (a.ActCoordination.Any(c => c.CoordinationDeclaredDate != null) ? " (ОМ)" : string.Empty),
                                         RegNumber = a.RegNumber,
                                         RegNumberNew = a.RegNumber,
                                         RegDate = a.RegDate.Value,
                                         IsFinalDoc = a.IsFinalDoc,
                                         DateWrt = a.DateWrt,
                                         EcliCode = a.EcliCode,
                                         Description = a.Description,
                                         ActDeclaredDate = a.ActDeclaredDate,
                                         ActInforcedDate = a.ActInforcedDate,
                                         ActMotivesDeclaredDate = a.ActMotivesDeclaredDate,
                                         ActCoordinationLabel = a.ActCoordination.Any(c => c.CoordinationDeclaredDate != null) ? "ОМ" : string.Empty
                                     })
                                     .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ CaseSessionMeetingVM за ел. папка
        /// </summary>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <returns></returns>
        private async Task<List<CaseSessionMeetingVM>> GetSessionMeetingsEF(int caseSessionId)
        {
            return await readonlyrepo.AllReadonly<CaseSessionMeeting>()
                                     .Where(meet => meet.DateExpired == null &&
                                                    meet.CaseSessionId == caseSessionId)
                                     .Select(meet => new CaseSessionMeetingVM()
                                     {
                                         Id = meet.Id,
                                         CaseSessionId = meet.CaseSessionId,
                                         DateFrom = meet.DateFrom,
                                         DateTo = meet.DateTo,
                                         Description = meet.Description,
                                         SessionMeetingTypeLabel = meet.SessionMeetingType.Label,
                                         UsersNames = string.Join("; ", meet.CaseSessionMeetingUsers.Select(u => u.SecretaryUser.LawUnit.FullName))
                                     })
                                     .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ CaseSessionDocVM за ел. папка
        /// </summary>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <returns></returns>
        private async Task<List<CaseSessionDocVM>> GetCaseSessionDocsEF(int caseSessionId)
        {
            return await readonlyrepo.AllReadonly<CaseSessionDoc>()
                                     .Where(d => d.DateExpired == null &&
                                                 d.CaseSessionId == caseSessionId)
                                     .Select(d => new CaseSessionDocVM()
                                     {
                                         Id = d.Id,
                                         CaseSessionId = d.CaseSessionId,
                                         DocumentId = d.DocumentId,
                                         DocumentLabel = "Вх.№ " + d.Document.DocumentNumber + "/" + d.Document.DocumentDate.ToString("dd.MM.yyyy") + " " + d.Document.DocumentType.Label,
                                         SessionDocStateLabel = d.SessionDocState.Label
                                     })
                                     .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ CaseNotificationVM за ел. папка
        /// </summary>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <returns></returns>
        private async Task<List<CaseNotificationVM>> GetCaseNotificationsEF(int caseSessionId)
        {
            var queryCaseNotification = GetQueryCaseNotification();
            var queryNotificationDeliveryGroup = GetQueryNotificationDeliveryGroup();

            return await queryCaseNotification.Where(n => n.CaseSessionId == caseSessionId)
                                              .Select(n => new CaseNotificationVM()
                                              {
                                                  Id = n.Id,
                                                  CaseId = n.CaseId,
                                                  CaseSessionId = n.CaseSessionId,
                                                  CaseSessionActId = n.CaseSessionActId,
                                                  NotificationTypeLabel = (n.NotificationTypeId != null) ? n.NotificationType.Label : string.Empty,
                                                  NotificationTypeId = n.NotificationTypeId,
                                                  CasePersonName = (n.IsMultiLink == true && n.CaseNotificationMLinks != null) ? string.Join("<br>", n.CaseNotificationMLinks.Where(l => l.IsActive && l.IsChecked).Select(m => m.PersonSummonedName)) + "<br>  чрез: " + n.NotificationPersonName
                                                                                                                               : n.NotificationPersonName,
                                                  NotificationStateLabel = n.NotificationState.Label,
                                                  HtmlTemplateLabel = (n.HtmlTemplateId != null) ? n.HtmlTemplate.Label : string.Empty,
                                                  RegNumber = n.RegNumber,
                                                  RegDate = n.RegDate,
                                                  NotificationNumber = n.NotificationNumber,
                                                  NotificationDeliveryGroupLabel = queryNotificationDeliveryGroup.Where(g => g.Id == n.NotificationDeliveryGroupId)
                                                                                                                 .Select(g => g.Label)
                                                                                                                 .FirstOrDefault()
                                              })
                                              .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ CaseSessionFastDocumentVM за ел. папка
        /// </summary>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <param name="dateFrom">Начало на заседание</param>
        /// <returns></returns>
        private async Task<List<CaseSessionFastDocumentVM>> GetSessionFastDocumentsEF(int caseSessionId, DateTime dateFrom)
        {
            var queryCaseSessionFastDocument = GetQueryCaseSessionFastDocument();

            return await queryCaseSessionFastDocument.Where(d => d.CaseSessionId == caseSessionId)
                                                     .Select(d => new CaseSessionFastDocumentVM()
                                                     {
                                                         Id = d.Id,
                                                         CasePersonName = d.CasePerson.FullName + " " + "(" + d.CasePerson.PersonRole.Label + ")",
                                                         SessionDocTypeLabel = d.SessionDocType.Label,
                                                         SessionDocStateLabel = d.SessionDocState.Label,
                                                         CaseSessionFastDocumentInitDateSession = ((d.CaseSessionFastDocumentInitId != null) ? (d.CaseSessionFastDocumentInit.CaseSession.DateFrom) : (DateTime?)null),
                                                         DateSession = dateFrom
                                                     })
                                                     .ToListAsync();
        }

        /// <summary>
        /// Метод попълващ данни за заседание за ел. папка
        /// </summary>
        /// <param name="caseSession">Заседание</param>
        /// <returns></returns>
        private async Task SetCaseSessionsListEF(CaseSessionElectronicFolderVM caseSession)
        {
            caseSession.CaseSessionNotificationLists = await GetCaseSessionNotificationListsEF(caseSession.Id);
            caseSession.CaseSessionActs = await GetCaseSessionActsEF(caseSession.Id);
            caseSession.SessionMeetings = await GetSessionMeetingsEF(caseSession.Id);
            caseSession.CaseSessionDocs = await GetCaseSessionDocsEF(caseSession.Id);
            caseSession.CaseNotifications = await GetCaseNotificationsEF(caseSession.Id);
            caseSession.SessionFastDocuments = await GetSessionFastDocumentsEF(caseSession.Id, caseSession.DateFrom);
        }

        /// <summary>
        /// Извлича данни за дело за ел. папка
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public async Task<CaseElectronicFolderVM> CaseElectronicFolder_Select(int caseId)
        {
            try
            {
                CaseElectronicFolderVM caseElectronicFolderVM = await GetCaseElectronicFolder(caseId);
                caseElectronicFolderVM.CaseSessions = await GetCaseSessionElectronicFolder(caseId);
                caseElectronicFolderVM.DocumentResolutions = await GetDocumentResolutionsEF(caseElectronicFolderVM.DocumentId);
                caseElectronicFolderVM.DocumentLinks = await GetDocumentLinkEF(caseElectronicFolderVM.DocumentId);
                caseElectronicFolderVM.CaseSelectionProtokols = await GetCaseSelectionProtokolsEF(caseElectronicFolderVM.Id);
                caseElectronicFolderVM.CaseInDocuments = await GetCaseInDocumentsEF(caseElectronicFolderVM.Id);
                caseElectronicFolderVM.RegixReports = await GetRegixReportsEF(caseElectronicFolderVM.Id);
                caseElectronicFolderVM.CaseOutDocuments = await GetCaseOutDocumentsEF(caseElectronicFolderVM.Id);
                caseElectronicFolderVM.CasePersons = await GetCasePersonsEF(caseElectronicFolderVM.Id);
                caseElectronicFolderVM.CaseSessionFinalActs = await GetCaseSessionFinalActsEF(caseElectronicFolderVM.Id);
                caseElectronicFolderVM.CaseLawUnits = await GetCaseLawUnitsEF(caseElectronicFolderVM.Id);
                caseElectronicFolderVM.CaseMigrations = await GetCaseMigrationsEF(caseElectronicFolderVM.Id);
                caseElectronicFolderVM.CaseClassifications = await GetCaseClassificationsEF(caseElectronicFolderVM.Id);
                caseElectronicFolderVM.DocumentInstitutionCaseInfos = await GetDocumentInstitutionCaseInfosEF(caseElectronicFolderVM.DocumentId);
                caseElectronicFolderVM.PaymentCases = await GetPaymentCasesEF(caseElectronicFolderVM.Id);
                caseElectronicFolderVM.ExecLists = await GetExecListsEF(caseElectronicFolderVM.Id);
                caseElectronicFolderVM.ExpenseOrders = await GetExpenseOrdersEF(caseElectronicFolderVM.Id);
                caseElectronicFolderVM.DocumentDecisionCaseLists = await GetDocumentDecisionCaseListsEF(caseElectronicFolderVM.Id);
                caseElectronicFolderVM.DocumentsOtherFromDifferentCourt = await GetDocumentsOtherFromDifferentCourtEF(caseElectronicFolderVM.Id);
                caseElectronicFolderVM.DocumentsOtherFromSameCourt = await GetDocumentsOtherFromSameCourtEF(caseElectronicFolderVM.Id, caseElectronicFolderVM.DocumentId);
                caseElectronicFolderVM.WorkTaskReports = await GetWorkTaskReportsEF(caseElectronicFolderVM.Id, caseElectronicFolderVM.DocumentId);
                caseElectronicFolderVM.DocumentResolutionCase = await GetDocumentResolutionCaseEF(caseElectronicFolderVM.Id);
                caseElectronicFolderVM.MoneyDocument = await GetMoneyDocumentFromMoneyEF(caseElectronicFolderVM.DocumentId, caseElectronicFolderVM.AssignmentDocumentId, caseElectronicFolderVM.CaseInDocuments);
                caseElectronicFolderVM.MediationCaseSessions = await GetMediationCaseSessions(caseElectronicFolderVM.Id);
                caseElectronicFolderVM.Evidences = await GetCaseEvidences(caseElectronicFolderVM.Id);

                foreach (var session in caseElectronicFolderVM.CaseSessions)
                    await SetCaseSessionsListEF(session);

                return caseElectronicFolderVM;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при caseElectronicFolderVM");
                return null;
            }

        }

        public async Task<CaseMigrationVM> Case_GetPriorCase(long documentId)
        {
            return await caseMigrationService.Case_GetPriorCase(documentId);
        }

        public CaseMigrationVM Case_GetPriorCaseEISPP(long documentId, string eisppNumber)
        {
            var priorCaseId = repo.AllReadonly<DocumentCaseInfo>(x => x.DocumentId == documentId).Select(x => x.CaseId).FirstOrDefault() ?? 0;
            if (priorCaseId == 0)
            {
                return null;
            }

            var eisppMigrationToProsecutors = repo.AllReadonly<CaseMigration>()
                                            .Where(x => x.CaseMigrationTypeId == CaseMigrationTypes.SentToProsecutors && x.CaseId == priorCaseId)
                                            .Where(x => x.Case.EISSPNumber == eisppNumber)
                                            .Where(FilterExpireInfo<CaseMigration>(false))
                                            .OrderByDescending(x => x.Id)
                                            .Select(x => new CaseMigrationVM
                                            {
                                                Id = x.Id,
                                                CaseId = x.CaseId,
                                                InitialCaseId = x.InitialCaseId,
                                                CaseRegNumber = x.Case.RegNumber,
                                                CaseRegDate = x.Case.RegDate,
                                                MigrationTypeId = x.CaseMigrationTypeId,
                                                MigrationTypeName = x.CaseMigrationType.Label,
                                                Description = x.Description,
                                                SentToName = x.Case.Court.Label
                                            }).FirstOrDefault();
            return eisppMigrationToProsecutors;
        }


        private string GetStr_CasePersons(List<CasePersonListVM> models)
        {
            string result = string.Empty;

            string _leftSide = string.Empty;
            foreach (var casePerson in models.Where(x => x.RoleKindId == NomenclatureConstants.PersonKinds.LeftSide))
            {
                _leftSide += (!string.IsNullOrEmpty(_leftSide)) ? ", " : string.Empty;
                _leftSide += casePerson.FullName;
            }

            string _rightSide = string.Empty;
            foreach (var casePerson in models.Where(x => x.RoleKindId == NomenclatureConstants.PersonKinds.RightSide))
            {
                _rightSide += (!string.IsNullOrEmpty(_rightSide)) ? ", " : string.Empty;
                _rightSide += casePerson.FullName;
            }

            return "Име на ищците: " + _leftSide + " Ответни страни: " + _rightSide;
        }

        public IEnumerable<DepersonalizationHistoryItem> GetDepersonalizationHistory(int CaseId)
        {
            return repo.AllReadonly<CaseDepersonalizationValue>()
                                    .Where(x => x.CaseId == CaseId)
                                    .Select(x => new DepersonalizationHistoryItem
                                    {
                                        SearchValue = x.SearchValue,
                                        ReplaceValue = x.ReplaceValue,
                                        IsCaseSensitive = x.IsCaseInsensitive
                                    }).ToList();
        }

        public IEnumerable<DepersonalizationHistoryItem> GetSimilarDepersonalizationHistory(int CaseId)
        {
            var _caseModel = repo.GetById<Case>(CaseId);
            var fromDate = DateTime.Now.AddDays(-60);
            return repo.AllReadonly<CaseDepersonalizationValue>()
                                    .Where(x => x.CourtId == _caseModel.CourtId)
                                    .Where(x => x.CaseId != CaseId)
                                    .Where(x => x.Case.CaseTypeId == _caseModel.CaseTypeId)
                                    .Where(x => x.DateWrt >= fromDate)
                                    .OrderByDescending(x => x.Id)
                                    .Select(x => new DepersonalizationHistoryItem
                                    {
                                        SearchValue = x.SearchValue,
                                        ReplaceValue = x.ReplaceValue,
                                        IsCaseSensitive = x.IsCaseInsensitive
                                    }).Take(5000).Where(x => x.SearchValue != null && x.SearchValue.Length >= 4).ToList();
        }

        public bool SaveDataDepersonalizationHistory(int CaseId, IEnumerable<DepersonalizationHistoryItem> model, int sourceType, int sourceId,
                     bool saveDepersonalizeUser)
        {
            bool save = false;
            if (saveDepersonalizeUser)
            {
                switch (sourceType)
                {
                    case SourceTypeSelectVM.CaseSessionAct:
                        {
                            //Винаги при натискане на бутон финализиране при обезличаването да се записва потребителя
                            var act = GetById<CaseSessionAct>(sourceId);
                            act.DepersonalizeUserId = userContext.UserId;
                            act.DepersonalizeEndDate = DateTime.Now;
                        }
                        break;
                    case SourceTypeSelectVM.CaseSessionActMotive:
                        {
                            //Винаги при натискане на бутон финализиране при обезличаването на мотиви да се записва потребителя
                            var act = GetById<CaseSessionAct>(sourceId);
                            act.DepersonalizeMotiveUserId = userContext.UserId;
                            act.DepersonalizeMotiveEndDate = DateTime.Now;
                        }
                        break;
                    case SourceTypeSelectVM.CaseSessionActCoordination:
                        //Винаги при натискане на бутон финализиране при обезличаването да се записва потребителя
                        var coordination = GetById<CaseSessionActCoordination>(sourceId);
                        coordination.DepersonalizeEndDate = DateTime.Now;
                        coordination.DepersonalizeUserId = userContext.UserId;
                        break;
                    default:
                        break;
                }

                save = true;
            }

            if (model != null && model.Where(x => !string.IsNullOrEmpty(x.SearchValue)).Any())
            {
                var saved = GetDepersonalizationHistory(CaseId);

                var newValues = model.Where(x => !string.IsNullOrEmpty(x.SearchValue))
                    .Select(x => new CaseDepersonalizationValue
                    {
                        CourtId = userContext.CourtId,
                        CaseId = CaseId,
                        DateWrt = DateTime.Now,
                        SearchValue = x.SearchValue,
                        ReplaceValue = x.ReplaceValue,
                        IsCaseInsensitive = x.IsCaseSensitive
                    }).Where(x => !saved.Any(s => s.SearchValue == x.SearchValue));


                repo.AddRange<CaseDepersonalizationValue>(newValues);

                repo.SaveChanges();
                return true;
            }
            else
            {
                //Заради ъпдейта на акта
                if (save)
                    repo.SaveChanges();
            }

            return false;
        }

        public bool CheckCaseOldNumber(int CaseGroupId, string oldNumber, DateTime oldDate)
        {
            //int oldNumberInt = Utils.ToInt(oldNumber);
            //var hasOldedAutoNumberedCases = repo.AllReadonly<Case>()
            //            .Where(x => x.CourtId == userContext.CourtId && x.CaseGroupId == CaseGroupId
            //            && x.ShortNumberValue <= oldNumberInt && x.RegDate.Year == oldDate.Year
            //            && (x.IsOldNumber ?? false) == false)
            //            .Any();
            //!hasOldedAutoNumberedCases && 
            return !repo.AllReadonly<Case>()
                       .Where(x => x.CourtId == userContext.CourtId && x.CaseGroupId == CaseGroupId
                       && x.ShortNumber == oldNumber && x.RegDate.Year == oldDate.Year)
                       .Any();
        }

        /// <summary>
        /// Справка за дела по критерии от линкващи таблици
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseVM> CaseReport_Select(int courtId, CaseFilterReport model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateNow = DateTime.Now;

            #region ДЕЛО

            //ДЕЛО
            model.DateFrom = (model.DateFrom ?? dateNow.AddYears(-100)).ForceStartDate();
            model.DateTo = (model.DateTo ?? dateNow.AddYears(100)).ForceEndDate();

            Expression<Func<Case, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate >= model.DateFrom && x.RegDate <= model.DateTo;

            Expression<Func<Case, bool>> caseInforcedDateFromSearch = x => true;
            if (model.CaseInforcedDateFrom != null)
                caseInforcedDateFromSearch = x => x.CaseInforcedDate >= model.CaseInforcedDateFrom;

            Expression<Func<Case, bool>> caseInforcedDateToSearch = x => true;
            if (model.CaseInforcedDateTo != null)
                caseInforcedDateToSearch = x => x.CaseInforcedDate <= model.CaseInforcedDateTo;

            Expression<Func<Case, bool>> yearSearch = x => true;
            if ((model.CaseYear ?? 0) > 0)
                yearSearch = x => x.RegDate >= NomenclatureExtensions.GetPastDate() && x.RegDate.Year == model.CaseYear;

            Expression<Func<Case, bool>> caseRegnumberSearch = x => true;
            if (!string.IsNullOrEmpty(model.RegNumber))
                caseRegnumberSearch = x => EF.Functions.ILike(x.RegNumber, model.RegNumber.ToCasePaternSearch());

            Expression<Func<Case, bool>> caseEisppnumberSearch = x => true;
            if (!string.IsNullOrEmpty(model.EisppNumber))
                caseEisppnumberSearch = x => EF.Functions.ILike(x.EISSPNumber, model.EisppNumber.ToPaternSearch());

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.CaseGroupId == model.CaseGroupId;

            Expression<Func<Case, bool>> caseTypeWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeWhere = x => x.CaseTypeId == model.CaseTypeId;

            Expression<Func<Case, bool>> caseStateWhere = x => true;
            if (model.CaseStateId > 0)
                caseStateWhere = x => x.CaseStateId == model.CaseStateId;

            Expression<Func<Case, bool>> processPriorityIdWhere = x => true;
            if (model.ProcessPriorityId > 0)
                processPriorityIdWhere = x => x.ProcessPriorityId == model.ProcessPriorityId;

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits
                                            .Any(a => a.CaseSessionId == null &&
                                                     (a.DateTo ?? dateEnd).Date >= x.RegDate.Date &&
                                                     a.LawUnitId == model.JudgeReporterId &&
                                                     a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            Expression<Func<Case, bool>> caseClassificationWhere = x => true;
            if (model.CaseClassificationId > 0)
                caseClassificationWhere = x => x.CaseClassifications
                                                .Any(a => a.ClassificationId == model.CaseClassificationId &&
                                                          a.DateTo == null &&
                                                          a.CaseSessionId == null);

            #endregion

            #region ЗАСЕДАНИЕ

            //ЗАСЕДАНИЕ
            DateTime dateFromSession = model.Session_DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.Session_DateFrom;
            DateTime dateToSession = model.SessionDateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.SessionDateTo;

            Expression<Func<Case, bool>> dateSessionSearch = x => true;
            if ((model.Session_DateFrom != null || model.SessionDateTo != null) ||
                (model.SessionTypeId > 0) ||
                (model.CourtHallId > 0) ||
                (model.SessionStateId > 0) ||
                (model.SessionResultId > 0))
            {
                Expression<Func<CaseSession, bool>> sessionDateWhere = a => true;
                if (model.Session_DateFrom != null || model.SessionDateTo != null)
                    sessionDateWhere = a => a.DateFrom.Date >= dateFromSession.Date && a.DateFrom.Date <= dateToSession.Date;

                Expression<Func<CaseSession, bool>> sessionTypeIdWhere = a => true;
                if (model.SessionTypeId > 0)
                    sessionTypeIdWhere = a => a.SessionTypeId == model.SessionTypeId;

                Expression<Func<CaseSession, bool>> courtHallIdWhere = a => true;
                if (model.CourtHallId > 0)
                    courtHallIdWhere = a => a.CourtHallId == model.CourtHallId;

                Expression<Func<CaseSession, bool>> sessionStateIdWhere = a => true;
                if (model.SessionStateId > 0)
                    sessionStateIdWhere = a => a.SessionStateId == model.SessionStateId;

                Expression<Func<CaseSession, bool>> sessionResultIdWhere = a => true;
                if (model.SessionResultId > 0)
                    sessionResultIdWhere = a => a.CaseSessionResults
                                                 .Any(res => res.SessionResultId == model.SessionResultId &&
                                                             res.DateExpired == null);

                var queryCaseSession = repo.AllReadonly<CaseSession>()
                                           .Where(a => a.DateExpired == null)
                                           .Where(sessionDateWhere)
                                           .Where(sessionTypeIdWhere)
                                           .Where(courtHallIdWhere)
                                           .Where(sessionStateIdWhere)
                                           .Where(sessionResultIdWhere);

                dateSessionSearch = x => queryCaseSession.Any(a => a.CaseId == x.Id);
            }

            #endregion

            #region АКТОВЕ

            //АКТОВЕ
            Expression<Func<Case, bool>> sessionActWhere = x => true;
            if (model.ActTypeId > 0 ||
                (model.ActRegDateFrom != null && model.ActRegDateTo != null) ||
                (model.ActInforcedDateFrom != null && model.ActInforcedDateTo != null) ||
                !string.IsNullOrEmpty(model.ActNumber) ||
                model.ActIsFinalDocHidden ||
                model.ActLawBaseId > 0)
            {
                Expression<Func<CaseSessionAct, bool>> actTypeIdWhere = a => true;
                if (model.ActTypeId > 0)
                    actTypeIdWhere = a => a.ActTypeId == model.ActTypeId;

                Expression<Func<CaseSessionAct, bool>> actRegDateFromWhere = a => true;
                if (model.ActRegDateFrom != null)
                {
                    model.ActRegDateFrom = model.ActRegDateFrom.ForceStartDate();
                    actRegDateFromWhere = a => a.ActDate != null && a.ActDate >= model.ActRegDateFrom;
                }

                Expression<Func<CaseSessionAct, bool>> actRegDateToWhere = a => true;
                if (model.ActRegDateTo != null)
                {
                    model.ActRegDateTo = model.ActRegDateTo.ForceEndDate();
                    actRegDateToWhere = a => a.ActDate != null && a.ActDate <= model.ActRegDateTo;
                }

                Expression<Func<CaseSessionAct, bool>> actInforcedDateFromWhere = a => true;
                if (model.ActInforcedDateFrom != null)
                {
                    model.ActInforcedDateFrom = model.ActInforcedDateFrom.ForceStartDate();
                    actInforcedDateFromWhere = a => a.ActInforcedDate != null && a.ActInforcedDate >= model.ActInforcedDateFrom;
                }

                Expression<Func<CaseSessionAct, bool>> actInforcedDateToWhere = a => true;
                if (model.ActInforcedDateTo != null)
                {
                    model.ActInforcedDateTo = model.ActInforcedDateTo.ForceStartDate();
                    actInforcedDateToWhere = a => a.ActInforcedDate != null && a.ActInforcedDate <= model.ActInforcedDateTo;
                }

                Expression<Func<CaseSessionAct, bool>> actNumberWhere = a => true;
                if (!string.IsNullOrEmpty(model.ActNumber))
                    actNumberWhere = a => a.RegNumber == model.ActNumber;

                Expression<Func<CaseSessionAct, bool>> actIsFinalDocWhere = a => true;
                if (model.ActIsFinalDocHidden)
                    actIsFinalDocWhere = a => a.IsFinalDoc == true && NomenclatureConstants.SessionActState.EnforcedStatesWithOutCanceled.Contains(a.ActStateId) && a.ActDeclaredDate != null;

                Expression<Func<CaseSessionAct, bool>> actLawBaseIdWhere = a => true;
                if (model.ActLawBaseId > 0)
                    actLawBaseIdWhere = a => a.CaseSessionActLawBases.Any(b => b.LawBaseId == model.ActLawBaseId);

                var caseSessionActsQuery = repo.AllReadonly<CaseSessionAct>()
                                               .Where(actTypeIdWhere)
                                               .Where(actRegDateFromWhere)
                                               .Where(actRegDateToWhere)
                                               .Where(actInforcedDateFromWhere)
                                               .Where(actInforcedDateToWhere)
                                               .Where(actNumberWhere)
                                               .Where(actIsFinalDocWhere)
                                               .Where(actLawBaseIdWhere)
                                               .Where(a => a.DateExpired == null);

                sessionActWhere = x => caseSessionActsQuery.Any(a => a.CaseId == x.Id);
            }

            #endregion

            #region СВЪРЗАНИ ДЕЛА

            var initCaseIds = repo.AllReadonly<CaseMigration>();
            var caseMigration = repo.AllReadonly<CaseMigration>();

            //СВЪРЗАНИ ДЕЛА
            Expression<Func<Case, bool>> linkCaseCourtWhere = x => true;
            if (model.LinkDelo_CourtId > 0)
            {
                linkCaseCourtWhere = x => (x.Document.DocumentCaseInfo.Where(a => a.Case.CourtId == model.LinkDelo_CourtId).Any() ||
                                           caseMigration.Any(m => initCaseIds.Where(im => im.CaseId == x.Id)
                                                                             .Select(im => im.InitialCaseId)
                                                                             .Distinct()
                                                                             .Contains(m.InitialCaseId) &&
                                                                  m.Case.CaseStateId != NomenclatureConstants.CaseState.Deleted &&
                                                                  m.CaseId != x.Id &&
                                                                  m.Case.CourtId == model.LinkDelo_CourtId));
            }

            Expression<Func<Case, bool>> linkCaseIdWhere = x => true;
            if (!string.IsNullOrEmpty(model.LinkDelo_RegNumber))
            {
                linkCaseIdWhere = x => (x.Document.DocumentCaseInfo.Where(a => a.Case != null && EF.Functions.ILike(a.Case.RegNumber, model.LinkDelo_RegNumber.ToCasePaternSearch())).Any() ||
                                        caseMigration.Any(m => initCaseIds.Where(im => im.CaseId == x.Id)
                                                                          .Select(im => im.InitialCaseId)
                                                                          .Distinct()
                                                                          .Contains(m.InitialCaseId) &&
                                                               m.Case.CaseStateId != NomenclatureConstants.CaseState.Deleted &&
                                                               m.CaseId != x.Id &&
                                                               EF.Functions.ILike(m.Case.RegNumber, model.LinkDelo_RegNumber.ToCasePaternSearch())));
            }

            Expression<Func<Case, bool>> linkDescriptionWhere = x => true;
            if (!string.IsNullOrEmpty(model.LinkDelo_Description))
            {
                var stringFind = model.LinkDelo_Description.ToUpper();
                linkDescriptionWhere = x => x.Document.DocumentCaseInfo.Where(a => a.Case != null && EF.Functions.ILike(a.Description, stringFind.ToPaternSearch())).Any();
            }

            Expression<Func<Case, bool>> regNumOtherSystem = x => true;
            if (!string.IsNullOrEmpty(model.RegNumberOtherSystem))
                regNumOtherSystem = x => x.Document.DocumentCaseInfo.Where(a => (EF.Functions.ILike(a.CaseRegNumber, model.RegNumberOtherSystem.ToEndingPaternSearch())) && (a.IsLegacyCase ?? false)).Any();

            Expression<Func<Case, bool>> yearOtherSystem = x => true;
            if (model.YearOtherSystem > 0)
                yearOtherSystem = x => x.Document.DocumentCaseInfo.Where(a => (a.CaseYear == model.YearOtherSystem) && (a.IsLegacyCase ?? false)).Any();

            Expression<Func<Case, bool>> courtOtherSystem = x => true;
            if (model.CourtOtherSystem > 0)
                courtOtherSystem = x => x.Document.DocumentCaseInfo.Where(a => (a.CourtId == model.CourtOtherSystem) && (a.IsLegacyCase ?? false)).Any();

            Expression<Func<Case, bool>> linkInstitutionTypeWhere = x => true;
            if (model.Institution_InstitutionTypeId > 0)
                linkInstitutionTypeWhere = x => x.Document.DocumentInstitutionCaseInfo.Where(a => a.Institution.InstitutionTypeId == model.Institution_InstitutionTypeId).Any();

            Expression<Func<Case, bool>> linkInstitutionIdWhere = x => true;
            if (model.Institution_InstitutionId > 0)
                linkInstitutionIdWhere = x => x.Document.DocumentInstitutionCaseInfo.Where(a => a.InstitutionId == model.Institution_InstitutionId).Any();

            Expression<Func<Case, bool>> linkInstitutionYearWhere = x => true;
            if (model.Institution_CaseYear > 0)
                linkInstitutionYearWhere = x => x.Document.DocumentInstitutionCaseInfo.Where(a => a.CaseYear == model.Institution_CaseYear).Any();

            Expression<Func<Case, bool>> linkInstitutionCaseNumberWhere = x => true;
            if (string.IsNullOrEmpty(model.Institution_RegNumber) == false)
                linkInstitutionCaseNumberWhere = x => x.Document.DocumentInstitutionCaseInfo.Where(a => a.CaseNumber == model.Institution_RegNumber).Any();

            #endregion

            #region ДОКУМЕНТИ

            //ДОКУМЕНТИ

            var queryDocumentCaseInfo = repo.AllReadonly<DocumentCaseInfo>();
            Expression<Func<Case, bool>> documentWhere = x => true;
            if (model.DateDoc != null && !string.IsNullOrEmpty(model.NumberDoc))
            {
                documentWhere = x => (x.Document.DocumentDate.Date == model.DateDoc &&
                                      x.Document.DocumentNumber == model.NumberDoc) ||
                                     queryDocumentCaseInfo.Any(a => a.CaseId == x.Id &&
                                                                    a.Document.DocumentDate.Date == model.DateDoc &&
                                                                    a.Document.DocumentNumber == model.NumberDoc);
            }
            else
            {

                if (model.DateDoc != null)
                    documentWhere = x => x.Document.DocumentDate.Date == model.DateDoc || queryDocumentCaseInfo.Any(a => a.CaseId == x.Id &&
                                                                                                                         a.Document.DocumentDate.Date == model.DateDoc);

                if (string.IsNullOrEmpty(model.NumberDoc) == false)
                    documentWhere = x => x.Document.DocumentNumber == model.NumberDoc || queryDocumentCaseInfo.Any(a => a.CaseId == x.Id &&
                                                                                                                        a.Document.DocumentNumber == model.NumberDoc);
            }

            #endregion

            #region СТРАНИ

            //СТРАНИ
            Expression<Func<Case, bool>> identifikatorPersonWhere = x => true;
            if (!string.IsNullOrEmpty(model.IdentifikatorPerson))
            {
                identifikatorPersonWhere = x => x.CasePersons.Where(a => a.CaseSessionId == null &&
                                                                         a.Uic == model.IdentifikatorPerson &&
                                                                         a.DateExpired == null)
                                                             .Any();
            }

            Expression<Func<Case, bool>> namePersonWhere = x => true;
            if (!string.IsNullOrEmpty(model.NamePerson))
            {
                namePersonWhere = x => x.CasePersons.Where(a => a.CaseSessionId == null &&
                                                                EF.Functions.ILike(a.FullName, model.NamePerson.ToPaternSearch()) &&
                                                                a.DateExpired == null)
                                                    .Any();
            }

            #endregion

            #region СЪДЕБЕН СЪСТАВ

            //СЪДЕБЕН СЪСТАВ
            Expression<Func<Case, bool>> caseLawUnitWhere = x => true;
            if (!string.IsNullOrEmpty(model.IdentifikatorCaseLawUnit) ||
                !string.IsNullOrEmpty(model.NameCaseLawUnit) ||
                model.CourtDepartmentId > 0)
            {
                Expression<Func<CaseLawUnit, bool>> identifikatorCaseLawUnitWhere = x => true;
                if (!string.IsNullOrEmpty(model.IdentifikatorCaseLawUnit))
                    identifikatorCaseLawUnitWhere = a => a.LawUnit.Uic == model.IdentifikatorCaseLawUnit;

                Expression<Func<CaseLawUnit, bool>> nameCaseLawUnitWhere = x => true;
                if (!string.IsNullOrEmpty(model.NameCaseLawUnit))
                    nameCaseLawUnitWhere = a => EF.Functions.ILike(a.LawUnit.FullName, model.NameCaseLawUnit.ToPaternSearch());

                Expression<Func<CaseLawUnit, bool>> courtDepartment = x => true;
                if (model.CourtDepartmentId > 0)
                    courtDepartment = a => a.CourtDepartmentId == model.CourtDepartmentId;

                var queryCaseLawUnits = repo.AllReadonly<CaseLawUnit>()
                                            .Where(a => (a.DateTo ?? dateNow.AddYears(1)) >= dateNow)
                                            .Where(a => !NomenclatureConstants.JudgeRole.ManualRoles.Contains(a.JudgeRoleId))
                                            .Where(a => a.CaseSessionId == null)
                                            .Where(identifikatorCaseLawUnitWhere)
                                            .Where(nameCaseLawUnitWhere)
                                            .Where(courtDepartment);

                caseLawUnitWhere = x => queryCaseLawUnits.Any(a => a.CaseId == x.Id);
            }

            #endregion

            return repo.AllReadonly<Case>()
                       .Where(x => x.CourtId == courtId)
                       .Where(x => !NomenclatureConstants.CaseState.FakeCase.Contains(x.CaseStateId))
                       .Where(dateSearch)
                       .Where(caseInforcedDateFromSearch)
                       .Where(caseInforcedDateToSearch)
                       .Where(caseRegnumberSearch)
                       .Where(caseEisppnumberSearch)
                       .Where(yearSearch)
                       .Where(caseGroupWhere)
                       .Where(caseTypeWhere)
                       .Where(caseStateWhere)
                       .Where(processPriorityIdWhere)
                       .Where(judgeReporterSearch)
                       .Where(dateSessionSearch)
                       .Where(linkCaseCourtWhere)
                       .Where(linkCaseIdWhere)
                       .Where(linkDescriptionWhere)
                       .Where(linkInstitutionTypeWhere)
                       .Where(linkInstitutionIdWhere)
                       .Where(linkInstitutionYearWhere)
                       .Where(linkInstitutionCaseNumberWhere)
                       .Where(regNumOtherSystem)
                       .Where(yearOtherSystem)
                       .Where(courtOtherSystem)
                       .Where(documentWhere)
                       .Where(identifikatorPersonWhere)
                       .Where(namePersonWhere)
                       .Where(caseLawUnitWhere)
                       .Where(caseClassificationWhere)
                       .Where(sessionActWhere)
                       .Select(x => new CaseVM()
                       {
                           Id = x.Id,
                           CourtId = x.CourtId,
                           CaseTypeLabel = (x.CaseType != null) ? x.CaseType.Code : string.Empty,
                           CaseCodeLabel = (x.CaseCode != null) ? x.CaseCode.Code + " " + x.CaseCode.Label : string.Empty,
                           ProcessPriorityLabel = (x.ProcessPriority != null) ? x.ProcessPriority.Label : string.Empty,
                           CaseStateLabel = (x.CaseState != null) ? x.CaseState.Label : string.Empty,
                           ShortNumberVal = x.ShortNumberValue ?? 0,
                           ShortNumber = Convert.ToString(int.Parse(x.ShortNumber ?? "0")),
                           RegNumber = x.RegNumber,
                           RegDate = x.RegDate,
                           CaseReasonLabel = (x.CaseReason != null) ? x.CaseReason.Label : string.Empty,
                           CaseStateDescription = x.CaseStateDescription
                       })
                       .AsQueryable();
        }

        private async Task<List<CdnDownloadResult>> ReadFile(int SourceTypeSelectId, int Id, CdnFileSelect.PostProcess postProcess = CdnFileSelect.PostProcess.None)
        {
            var cdnItems = cdnService.Select(SourceTypeSelectId, Id.ToString()).ToList();

            List<CdnDownloadResult> results = new List<CdnDownloadResult>();
            if (cdnItems != null)
            {
                foreach (var cdnItem in cdnItems.Where(x => x.DateExpired == null))
                {
                    var cdnDownload = await cdnService.MongoCdn_Download(cdnItem.FileId, postProcess).ConfigureAwait(false);
                    results.Add(cdnDownload);
                }

                return results;
            }

            return null;
        }

        /// <summary>
        /// Добавяне на файлове от дело в архив на ел. папка
        /// </summary>
        /// <param name="zip"></param>
        /// <param name="ObjectId"></param>
        /// <param name="SourceTypeSelectId"></param>
        /// <param name="NameDir"></param>
        /// <param name="NameElement"></param>
        /// <returns></returns>
        private async Task<List<string>> FillFilesInArchive(ZipArchive zip, int? ObjectId, long? ObjectLongId, int SourceTypeSelectId, string NameDir, string NameElement, CdnFileSelect.PostProcess postProcess = CdnFileSelect.PostProcess.None)
        {
            List<string> result = new List<string>();

            var cdnDownloadResults = await ReadFile(SourceTypeSelectId, (ObjectId != null ? (ObjectId ?? 0) : Convert.ToInt32(ObjectLongId ?? 0)), postProcess).ConfigureAwait(false);

            foreach (CdnDownloadResult f in cdnDownloadResults)
            {
                // add the item name to the zip
                ZipArchiveEntry zipItem = zip.CreateEntry(NameDir + f.FileName.Replace(" ", "_"));
                result.Add("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href=" + NameDir + f.FileName.Replace(" ", "_") + ">" + NameElement + "</a></br>");
                // add the item bytes to the zip entry by opening the original file and copying the bytes
                using (System.IO.MemoryStream originalFileMemoryStream = new System.IO.MemoryStream(f.GetBytes()))
                {
                    using (System.IO.Stream entryStream = zipItem.Open())
                    {
                        originalFileMemoryStream.CopyTo(entryStream);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Страница за архива за дело на ел. папка
        /// </summary>
        /// <param name="streamWriter"></param>
        /// <param name="rowList"></param>
        /// <param name="caseElectronic"></param>
        private void FillHtml(StreamWriter streamWriter, List<string> rowList, CaseElectronicFolderVM caseElectronic)
        {
            streamWriter.Write("<!DOCTYPE html>");
            streamWriter.Write("<html>");
            streamWriter.Write("<head>");
            streamWriter.Write("<meta charset=\"utf-8\"/>");
            streamWriter.Write("<title></title>");
            streamWriter.Write("</head>");
            streamWriter.Write("<body>");
            streamWriter.Write("<center>");
            streamWriter.Write("<p><b>Експорт на " + caseElectronic.CaseGroupLabel + " " + caseElectronic.RegNumber + "/" + caseElectronic.RegDate.ToString("dd.MM.yyyy") + "</b></p>");
            streamWriter.Write("</center>");
            streamWriter.Write("</br>");
            streamWriter.Write("</br>");
            streamWriter.Write("<b>Основни данни</b></br>");
            streamWriter.Write("<b>Точен вид дело:</b>" + caseElectronic.CaseTypeLabel + "</br>");
            streamWriter.Write("<b>Шифър:</b>" + caseElectronic.CaseCodeLabel + "</br>");
            streamWriter.Write("<b>Статус:</b>" + caseElectronic.CaseStateLabel + "</br>");

            if (!string.IsNullOrEmpty(caseElectronic.JudgeRapporteur))
            {
                streamWriter.Write("<b>Съдия докладчик:</b>" + caseElectronic.JudgeRapporteur + "</br>");
            }
            streamWriter.Write("</br>");
            streamWriter.Write("</br>");

            foreach (var row in rowList)
            {
                streamWriter.Write(row);
            }

            streamWriter.Write("</body>");
            streamWriter.Write("</html>");
        }

        /// <summary>
        /// Метод за архивиране на ел. папка
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public async Task<byte[]> CaseArchive(int CaseId)
        {
            var caseCase = await CaseElectronicFolder_Select(CaseId);
            var nameDelo = "Дело_" + caseCase.RegNumber + "_" + caseCase.RegDate.Day.ToString("00") + "_" + caseCase.RegDate.Month.ToString("00") + "_" + caseCase.RegDate.Year.ToString("0000");
            List<string> fileHtml = new List<string>();
            // the output bytes of the zip

            byte[] fileBytes = null;

            // create a working memory stream
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                // create a zip
                using (ZipArchive zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    fileHtml.Add("1. Иницииращ документи: </br>");
                    fileHtml.AddRange(await FillFilesInArchive(zip, null, caseCase.DocumentId, SourceTypeSelectVM.Document, nameDelo + "/01_Иницииращи_документи/", "Иницииращ документи").ConfigureAwait(false));

                    fileHtml.Add("1.1. Резолюции: </br>");
                    foreach (var documentResolution in caseCase.DocumentResolutions)
                    {
                        var docres = documentResolution.ResolutionTypeLabel.Replace(" ", "_") + "_" + documentResolution.RegNumber + "_" + (documentResolution.RegDate ?? DateTime.Now).Day.ToString("00") + "_" + (documentResolution.RegDate ?? DateTime.Now).Month.ToString("00") + "_" + (documentResolution.RegDate ?? DateTime.Now).Year.ToString("0000");
                        var docresElement = documentResolution.Label;
                        fileHtml.AddRange(await FillFilesInArchive(zip, null, documentResolution.Id, SourceTypeSelectVM.DocumentResolutionPdf, nameDelo + "/01_01_Резолюции/" + docres + "/", docresElement).ConfigureAwait(false));
                    }

                    fileHtml.Add("2. Списък на лицата: </br>");
                    fileHtml.AddRange(await FillFilesInArchive(zip, caseCase.Id, null, SourceTypeSelectVM.CasePerson, nameDelo + "/02_Списък_на_лицата/", "Списък на лицата").ConfigureAwait(false));

                    fileHtml.Add("3. Протоколи от разпределение: </br>");
                    foreach (var caseSelectionProtokol in caseCase.CaseSelectionProtokols)
                    {
                        var nameProtokol = caseSelectionProtokol.SelectedLawUnitName.Replace(" ", "_") + "_" + caseSelectionProtokol.SelectionDate.Day.ToString("00") + "_" + caseSelectionProtokol.SelectionDate.Month.ToString("00") + "_" + caseSelectionProtokol.SelectionDate.Year.ToString("0000") + "_" + caseSelectionProtokol.SelectionDate.Hour.ToString("00") + "_" + caseSelectionProtokol.SelectionDate.Minute.ToString("00");
                        var nameProtokolElement = caseSelectionProtokol.SelectedLawUnitName + " " + caseSelectionProtokol.SelectionDate.ToString("dd.MM.yyyy");
                        fileHtml.AddRange(await FillFilesInArchive(zip, caseSelectionProtokol.Id, null, SourceTypeSelectVM.CaseSelectionProtokol, nameDelo + "/03_Протоколи_от_разпределение/" + nameProtokol + "/", nameProtokolElement).ConfigureAwait(false));
                    }

                    fileHtml.Add("4. Съпровождащи документи: </br>");
                    foreach (var documentInfo in caseCase.CaseInDocuments)
                    {
                        var nameDoc = documentInfo.Title.Replace(" ", "_");
                        fileHtml.AddRange(await FillFilesInArchive(zip, null, documentInfo.Id, SourceTypeSelectVM.Document, nameDelo + "/04_Съпровождащи_документи/" + nameDoc + "/", documentInfo.Title).ConfigureAwait(false));
                    }

                    foreach (var documentDecisionCase in caseCase.DocumentDecisionCaseLists)
                    {
                        var nameDoc = documentDecisionCase.DocumentShortLable.Replace(" ", "_").Replace("/", "_").Replace(".", "_");
                        var textDocument = documentDecisionCase.DocumentLable +
                                           (!string.IsNullOrEmpty(documentDecisionCase.DecisionRequestTypeName) ? " - " + documentDecisionCase.DecisionRequestTypeName : string.Empty) +
                                           (!string.IsNullOrEmpty(documentDecisionCase.DecisionName) ? ": " + documentDecisionCase.DecisionName : string.Empty);
                        fileHtml.AddRange(await FillFilesInArchive(zip, null, documentDecisionCase.DocumentId, SourceTypeSelectVM.Document, nameDelo + "/04_Съпровождащи_документи/" + nameDoc + "/", textDocument).ConfigureAwait(false));
                        fileHtml.AddRange(await FillFilesInArchive(zip, null, documentDecisionCase.DocumentId, SourceTypeSelectVM.DocumentDecision, nameDelo + "/04_Съпровождащи_документи/" + nameDoc + "/", textDocument).ConfigureAwait(false));
                    }

                    fileHtml.Add("5. Суми по дело: </br>");
                    foreach (var paymentCase in caseCase.PaymentCases)
                    {
                        var namePaymentDir = paymentCase.PersonNames +
                                             " " + paymentCase.PaidDate.ToString("dd.MM.yyyy HH mm");
                        var namePayment = "Име: " + paymentCase.PersonNames +
                                          " Тип: " + paymentCase.MoneyTypeNames +
                                          " Сума дело: " + paymentCase.AmountForCase +
                                          " Сума плащане: " + paymentCase.AmountForPayment +
                                          " Вид плащане: " + paymentCase.PaymentTypeName +
                                          " Дата: " + paymentCase.PaidDate.ToString("dd.MM.yyyy");
                        fileHtml.AddRange(await FillFilesInArchive(zip, paymentCase.Id, null, SourceTypeSelectVM.Payment, nameDelo + "/05_Суми_по_дело/" + "Плащане_" + namePaymentDir.Replace(" ", "_").Replace(".", "_").Replace(",", "_").Replace(":", "_") + "/", namePayment).ConfigureAwait(false));
                    }

                    foreach (var execList in caseCase.ExecLists.Where(x => x.RegDate != null))
                    {
                        var nameExecListsDir = execList.RegNumber + "_" + execList.RegDate?.ToString("dd_MM_yyyy HH mm");
                        var nameExecLists = "Тип: " + execList.ExecListTypeName +
                                            " Лице: " + execList.FullName +
                                            " Сума: " + execList.Amount.ToString("0.00") +
                                            " Номер: " + execList.RegDate != null ? (execList.RegNumber + "/" + execList.RegDate.DateToStr(FormattingConstant.NormalDateFormat)) : "В проект";
                        fileHtml.AddRange(await FillFilesInArchive(zip, execList.Id, null, SourceTypeSelectVM.ExecList, nameDelo + "/05_Суми_по_дело/" + "ИЛ_" + nameExecListsDir.Replace(" ", "_").Replace(".", "_").Replace(",", "_").Replace(":", "_") + "/", nameExecLists).ConfigureAwait(false));
                    }

                    foreach (var expenseOrder in caseCase.ExpenseOrders)
                    {
                        var nameЕxpenseOrderDir = expenseOrder.RegNumber + "_" + expenseOrder.RegDate.ToString("dd_MM_yyyy HH mm");
                        var nameЕxpenseOrder = expenseOrder.FullName +
                                               " Номер: " + expenseOrder.RegNumber + "/" + expenseOrder.RegDate.ToString("dd.MM.yyyy") +
                                               " сума: " + expenseOrder.Amount.ToString("0.00"); ;
                        fileHtml.AddRange(await FillFilesInArchive(zip, expenseOrder.Id, null, SourceTypeSelectVM.ExpenseOrder, nameDelo + "/05_Суми_по_дело/" + "РКО_" + nameЕxpenseOrderDir.Replace(" ", "_").Replace(".", "_").Replace(",", "_").Replace(":", "_") + "/", nameЕxpenseOrder).ConfigureAwait(false));
                    }

                    fileHtml.Add("6. Изходящи документи: </br>");
                    foreach (var documentInfo in caseCase.CaseOutDocuments)
                    {
                        var nameDoc = documentInfo.Title.Replace(" ", "_");
                        fileHtml.AddRange(await FillFilesInArchive(zip, null, documentInfo.Id, SourceTypeSelectVM.Document, nameDelo + "/06_Изходящи_документи/" + nameDoc + "/", documentInfo.Title).ConfigureAwait(false));
                        fileHtml.AddRange(await FillFilesInArchive(zip, null, documentInfo.Id, SourceTypeSelectVM.DocumentPdf, nameDelo + "/06_Изходящи_документи/" + nameDoc + "/", documentInfo.Title).ConfigureAwait(false));
                    }

                    fileHtml.Add("7. Заседания: </br>");
                    foreach (var caseSession in caseCase.CaseSessions)
                    {
                        var nameSession = "Заседание_" + caseSession.SessionTypeLabel.Replace(" ", "_") + "_" +
                                                         caseSession.DateFrom.Day.ToString("00") + "_" +
                                                         caseSession.DateFrom.Month.ToString("00") + "_" +
                                                         caseSession.DateFrom.Year.ToString("0000") + "_" +
                                                         caseSession.DateFrom.Hour.ToString("00") + "_" +
                                                         caseSession.DateFrom.Minute.ToString("00");
                        fileHtml.Add("&nbsp;&nbsp;&nbsp;- Заседание: " + caseSession.SessionTypeLabel + " " + caseSession.DateFrom.ToString("dd.MM.yyyy") + "</br>");

                        fileHtml.Add("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;* Списък на призованите/уведомените/съобщените лица: </br>");
                        fileHtml.AddRange(await FillFilesInArchive(zip, caseSession.Id, null, SourceTypeSelectVM.CaseSessionNotificationList, nameDelo + "/" + nameSession + "/01(а)_Списък_на_призованите_лица/", "Списък на призованите лица").ConfigureAwait(false));
                        fileHtml.AddRange(await FillFilesInArchive(zip, caseSession.Id, null, SourceTypeSelectVM.CaseSessionNotificationListNotification, nameDelo + "/" + nameSession + "/01(б)_Списък_на_уведомените_лица/", "Списък на уведомените лица").ConfigureAwait(false));
                        fileHtml.AddRange(await FillFilesInArchive(zip, caseSession.Id, null, SourceTypeSelectVM.CaseSessionNotificationListMessage, nameDelo + "/" + nameSession + "/01(в)_Списък_на_съобщените_лица/", "Списък на съобщените лица").ConfigureAwait(false));

                        fileHtml.Add("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;* Актове и протоколи: </br>");
                        foreach (var caseSessionAct in caseSession.CaseSessionActs.OrderBy(x => x.ActDeclaredDate))
                        {
                            var nameAct = caseSessionAct.ActTypeLabel.Replace(" ", "_") + "_" + caseSessionAct.RegNumber + "_" + caseSessionAct.RegDate?.Day.ToString("00") + "_" + caseSessionAct.RegDate?.Month.ToString("00") + "_" + caseSessionAct.RegDate?.Year.ToString("0000");
                            var nameActElement = caseSessionAct.ActTypeLabel + " " + caseSessionAct.RegNumber + "/" + caseSessionAct.RegDate?.ToString("dd.MM.yyyy") + " (Дата на постановяване: " + (caseSessionAct.ActDeclaredDate ?? DateTime.Now).ToString("dd.MM.yyyy") + ")";
                            fileHtml.AddRange(await FillFilesInArchive(zip, caseSessionAct.Id, null, SourceTypeSelectVM.CaseSessionActPdf, nameDelo + "/" + nameSession + "/07_Актове_и_протоколи/" + nameAct + "/", nameActElement, CdnFileSelect.PostProcess.Flatten).ConfigureAwait(false));
                            fileHtml.AddRange(await FillFilesInArchive(zip, caseSessionAct.Id, null, SourceTypeSelectVM.CaseSessionActDepersonalized, nameDelo + "/" + nameSession + "/07_Актове_и_протоколи/" + nameAct + "/", "Обезличен_" + nameActElement, CdnFileSelect.PostProcess.Flatten).ConfigureAwait(false));
                            fileHtml.AddRange(await FillFilesInArchive(zip, caseSessionAct.Id, null, SourceTypeSelectVM.CaseSessionActMotivePdf, nameDelo + "/" + nameSession + "/07_Актове_и_протоколи/" + nameAct + "/", "Мотив_" + nameActElement, CdnFileSelect.PostProcess.Flatten).ConfigureAwait(false));
                            fileHtml.AddRange(await FillFilesInArchive(zip, caseSessionAct.Id, null, SourceTypeSelectVM.CaseSessionActMotiveDepersonalized, nameDelo + "/" + nameSession + "/07_Актове_и_протоколи/" + nameAct + "/", "Мотив_Обезличен_" + nameActElement, CdnFileSelect.PostProcess.Flatten).ConfigureAwait(false));
                            fileHtml.AddRange(await FillFilesInArchive(zip, caseSessionAct.Id, null, SourceTypeSelectVM.CaseSessionActManualUpload, nameDelo + "/" + nameSession + "/07_Актове_и_протоколи/" + nameAct + "/", "Прикачен_" + nameActElement, CdnFileSelect.PostProcess.Flatten).ConfigureAwait(false));

                        }

                        fileHtml.Add("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;* Уведомления: </br>");
                        foreach (var caseNotification in caseSession.CaseNotifications)
                        {
                            var nameNotification = caseNotification.NotificationTypeLabel.Replace(" ", "_") + "_" + caseNotification.RegNumber + "_" + caseNotification.RegDate.Day.ToString("00") + "_" + caseNotification.RegDate.Month.ToString("00") + "_" + caseNotification.RegDate.Year.ToString("0000");
                            var nameNotificationElement = caseNotification.NotificationTypeLabel + " " + caseNotification.RegNumber + "/" + caseNotification.RegDate.ToString("dd.MM.yyyy");
                            fileHtml.AddRange(await FillFilesInArchive(zip, caseNotification.Id, null, SourceTypeSelectVM.CaseNotificationPrint, nameDelo + "/" + nameSession + "/07_Уведомления/" + nameNotification + "/", nameNotificationElement).ConfigureAwait(false));
                            fileHtml.AddRange(await FillFilesInArchive(zip, caseNotification.Id, null, SourceTypeSelectVM.CaseNotificationReturn, nameDelo + "/" + nameSession + "/07_Уведомления/" + nameNotification + "/", "Сканиран_отрязък" + nameNotificationElement).ConfigureAwait(false));
                        }

                        fileHtml.Add("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;* Съпровождащи документи, представени в заседание: </br>");
                        foreach (var caseSessionFastDocument in caseSession.SessionFastDocuments)
                        {
                            var nameSessionFastDocuments = caseSessionFastDocument.Id.ToString() + "_" + caseSessionFastDocument.SessionDocTypeLabel.Replace(" ", "_");
                            var nameSessionFastDocumentsElement = caseSessionFastDocument.SessionDocTypeLabel + " " + caseSessionFastDocument.CasePersonName;
                            fileHtml.AddRange(await FillFilesInArchive(zip, caseSessionFastDocument.Id, null, SourceTypeSelectVM.CaseSessionFastDocument, nameDelo + "/" + nameSession + "/08_Съпровождащи_документи_представени_в_заседание/" + nameSessionFastDocuments + "/", nameSessionFastDocumentsElement).ConfigureAwait(false));
                        }
                    }

                    if (caseCase.CaseMigrations.Any())
                    {
                        fileHtml.Add("8. Свързани дела: </br>");
                        int num = 0;
                        foreach (var caseMigration in caseCase.CaseMigrations)
                        {
                            num++;
                            var _row = "8." + num + ". " +
                                       ((!string.IsNullOrEmpty(caseMigration.CaseRegNumber)) ? "Дело: " + caseMigration.CaseRegNumber + "/" + caseMigration.CaseRegDate.ToString("dd.MM.yyyy") + " - " + caseMigration.CaseCourtName :
                                                                                               caseMigration.CaseStateName + " : " + caseMigration.InitDocumentType + " " + caseMigration.InitDocumentNumber + "/" + caseMigration.InitDocumentDate.ToString("dd.MM.yyyy") + " - " + caseMigration.CaseCourtName) + "</br>";
                            fileHtml.Add(_row);
                        }
                    }

                    if (caseCase.RegixReports.Any())
                    {
                        fileHtml.Add("9. Справки външни системи: </br>");
                        foreach (var regix in caseCase.RegixReports.OrderBy(x => x.DateWrt))
                        {
                            var nameRegix = (regix.RegixTypeName + " " + regix.DateWrt.ToString("dd_MM_yyyy_HH_mm")).Replace(" ", "_").Replace("/", "_");
                            var titleRegix = regix.RegixTypeName +
                                             (!string.IsNullOrEmpty(regix.ActRegNumber) ? " Акт: " + regix.ActRegNumber : string.Empty) +
                                             (!string.IsNullOrEmpty(regix.DocumentNumber) ? " Документ: " + regix.DocumentNumber : string.Empty);
                            fileHtml.AddRange(await FillFilesInArchive(zip, null, regix.Id, SourceTypeSelectVM.RegixReport, nameDelo + "/09_Справки_външни_системи/" + nameRegix + "/", titleRegix).ConfigureAwait(false));
                        }
                    }

                    var htmlDelo = zip.CreateEntry(nameDelo + ".html");
                    var entryStream = htmlDelo.Open();
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        FillHtml(streamWriter, fileHtml, caseCase);
                    }
                }

                fileBytes = memoryStream.ToArray();
            }

            return fileBytes;
        }

        private string GetStringPerson(ICollection<CasePerson> model)
        {
            var result = string.Empty;

            foreach (var person in model)
            {
                if (!string.IsNullOrEmpty(result))
                    result += ", ";

                result += person.FullName + (person.PersonRole != null ? " (" + person.PersonRole.Label + ")" : string.Empty) + (person.PersonMaturity != null ? " - " + person.PersonMaturity.Label : string.Empty);
            }

            return result;
        }

        /// <summary>
        /// Справка за образувани дела с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <param name="model">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<CaseSprVM> CaseReportMaturity_Select(CaseFilterReport model)
        {
            List<CaseSprVM> result = new List<CaseSprVM>();

            var dateNow = DateTime.Now;
            model.DateFrom = (model.DateFrom ?? dateNow.AddYears(-100)).ForceStartDate();
            model.DateTo = (model.DateTo ?? dateNow.AddYears(100)).ForceEndDate();

            Expression<Func<Case, bool>> caseGroupIdWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupIdWhere = x => x.CaseGroupId == model.CaseGroupId;

            Expression<Func<Case, bool>> caseTypeIdWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeIdWhere = x => x.CaseTypeId == model.CaseTypeId;

            Expression<Func<Case, bool>> caseCodeIdsWhere = x => true;
            if (model.CaseCodeIds != null && model.CaseCodeIds.Any())
            {
                int[] caseCodeIds = model.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.CaseCodeId ?? 0);
            }

            Expression<Func<Case, bool>> judgeReporterIdWhere = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterIdWhere = x => x.CaseLawUnits.Any(a => a.CaseSessionId == null &&
                                                                    (a.DateTo ?? dateNow.AddYears(100)) >= dateNow && a.LawUnitId == model.JudgeReporterId &&
                                                                    a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            return repo.AllReadonly<Case>()
                       .Where(x => (x.CourtId == userContext.CourtId) &&
                                   ((x.RegDate >= model.DateFrom) && (x.RegDate <= (model.DateTo ?? dateNow.AddYears(100)))) &&
                                   (x.CaseStateId != NomenclatureConstants.CaseState.Draft) &&
                                   (x.CasePersons.Any(p => p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderAged || p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderLegalAge)))
                       .Where(caseGroupIdWhere)
                       .Where(caseTypeIdWhere)
                       .Where(caseCodeIdsWhere)
                       .Where(judgeReporterIdWhere)
                       .Where(x => !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                       .Select(x => new CaseSprVM()
                       {
                           Id = x.Id,
                           CaseGroupLabel = x.CaseGroup.Label + " - " + x.CaseType.Label,
                           CaseCodeLabel = x.CaseCode.Code + " " + x.CaseCode.Label,
                           CaseRegNum = x.RegNumber,
                           CaseRegDate = x.RegDate,
                           CaseBeginDate = x.RegDate,
                           CasePersons = string.Join(", ", x.CasePersons
                                                            .Where(p => p.CaseSessionId == null &&
                                                                        (p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderAged || p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderLegalAge))
                                                            .Select(p => p.FullName +
                                                                         " (" + p.PersonRole.Label + ")" +
                                                                         (p.PersonMaturityId != null ? " - " + p.PersonMaturity.Label : string.Empty))),
                           JudgeReport = x.CaseLawUnits
                                          .Where(l => l.CaseSessionId == null &&
                                                      (l.DateTo ?? DateTime.Now.AddYears(100)).Date >= DateTime.Now.Date &&
                                                      l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                      .OrderByDescending(l => l.DateFrom)
                                                      .Select(l => l.LawUnit.FullName +
                                                                   ((l.CourtDepartmentId != null) ? " състав: " + l.CourtDepartment.Label : string.Empty))
                                                      .FirstOrDefault(),
                           CaseEndDate = (x.CaseSessions.Any(s => s.DateExpired == null &&
                                                                  s.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                r.SessionResult.SessionResultGroupId == NomenclatureConstants.SessionResultGroupings.CaseWithoutFinalAct_Result)) &&
                                          x.CaseSessionActs.Any(ac => ac.DateExpired == null &&
                                                                      ac.ActDeclaredDate != null &&
                                                                      ac.IsFinalDoc)) ? x.CaseSessionActs.Where(ac => ac.DateExpired == null &&
                                                                                                                      ac.ActDeclaredDate != null &&
                                                                                                                      ac.IsFinalDoc)
                                                                                                         .Select(ac => ac.ActDeclaredDate)
                                                                                                         .FirstOrDefault() : null
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Справка за Дела с ненаписани съдебни актове от всички съдии
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseSprVM> CaseWithoutFinalAct_Select(int courtId, CaseFilterReport model)
        {
            List<CaseSprVM> result = new List<CaseSprVM>();

            model.DateFrom = NomenclatureExtensions.ForceStartDate(model.DateFrom);
            model.DateTo = NomenclatureExtensions.ForceEndDate(model.DateTo);
            var dateAddYear = DateTime.Now.AddYears(100);

            var resultFinish = repo.AllReadonly<SessionResultGrouping>()
                .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseWithoutFinalAct_Result)
                .Select(x => x.SessionResultId)
                .ToList();

            Expression<Func<Case, bool>> caseGroupIdWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupIdWhere = x => x.CaseGroupId == model.CaseGroupId;

            Expression<Func<Case, bool>> caseTypeIdWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeIdWhere = x => x.CaseTypeId == model.CaseTypeId;

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Where(a => a.CaseSessionId == null && (a.DateTo ?? dateAddYear).Date >= x.RegDate && a.LawUnitId == model.JudgeReporterId &&
                                                                     a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            return repo.AllReadonly<Case>()
                       .Where(x => (x.CourtId == courtId) &&
                                   ((x.RegDate >= model.DateFrom) && (x.RegDate <= (model.DateTo ?? DateTime.Now.AddYears(100)))) &&
                                   (x.CaseStateId != NomenclatureConstants.CaseState.Draft) &&
                                   ((x.CaseSessionResults.Where(r => r.DateExpired == null && resultFinish.Contains(r.SessionResultId)).Any() && (!x.CaseSessionActs.Any(a => a.DateExpired == null && a.IsFinalDoc))) ||
                                    (x.CaseSessionResults.Where(r => r.DateExpired == null && resultFinish.Contains(r.SessionResultId)).Any() && (x.CaseSessionActs.Any(a => a.DateExpired == null && a.ActDeclaredDate == null && a.IsFinalDoc))) ||
                                    (!x.CaseSessionResults.Where(r => r.DateExpired == null && resultFinish.Contains(r.SessionResultId)).Any() && (x.CaseSessionActs.Any(a => a.DateExpired == null && a.IsFinalDoc && a.ActDeclaredDate == null)))))
                       .Where(caseGroupIdWhere)
                       .Where(caseTypeIdWhere)
                       .Where(judgeReporterSearch)
                       .Select(x => new CaseSprVM()
                       {
                           Id = x.Id,
                           JudgeReport = x.CaseLawUnits.Where(l => l.CaseSessionId == null && l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).OrderByDescending(l => l.DateFrom).Select(l => l.LawUnit.FullName + ((l.CourtDepartmentId != null) ? " състав: " + l.CourtDepartment.Label : string.Empty)).FirstOrDefault(),
                           CaseTypeLabel = x.CaseType.Label,
                           CaseRegNum = x.RegNumber,
                           CaseCodeLabel = x.CaseCode.Code,
                           CaseRegDate = x.RegDate,
                           CaseEndDate = null,
                       })
                       .AsQueryable();
        }

        private string GetStringPersonNameNewLine(ICollection<CasePerson> model)
        {
            var result = string.Empty;

            foreach (var person in model)
            {
                result += person.FullName + "</p>";
            }

            return result;
        }

        private string GetStringPersonRoleNameNewLine(ICollection<CasePerson> model)
        {
            var result = string.Empty;

            foreach (var person in model)
            {
                result += person.PersonRole.Label + "</p>";
            }

            return result;
        }

        private string GetSentenseInfo(List<CasePersonSentence> model)
        {
            var result = string.Empty;

            foreach (var casePersonSentence in model.Where(x => (x.IsActive ?? false) == true))
            {
                result = casePersonSentence.CasePerson.FullName + ": " +
                         casePersonSentence.CaseSessionAct.ActType.Label + " " +
                         casePersonSentence.CaseSessionAct.RegNumber + "/" +
                         (casePersonSentence.CaseSessionAct.RegDate ?? DateTime.Now).Year + " " +
                         casePersonSentence.Description + " " +
                         casePersonSentence.SentenceResultType.Label + " ";

                foreach (var casePersonSentencePunishment in casePersonSentence.CasePersonSentencePunishments)
                {
                    result += casePersonSentencePunishment.SentenceType.Label + " " +
                              ((casePersonSentencePunishment.SentenseDays > 0) ? "Дни: " + casePersonSentencePunishment.SentenseDays + " " : string.Empty) +
                              ((casePersonSentencePunishment.SentenseWeeks > 0) ? "Седмици: " + casePersonSentencePunishment.SentenseWeeks + " " : string.Empty) +
                              ((casePersonSentencePunishment.SentenseMonths > 0) ? "Месеци: " + casePersonSentencePunishment.SentenseMonths + " " : string.Empty) +
                              ((casePersonSentencePunishment.SentenseYears > 0) ? "Години: " + casePersonSentencePunishment.SentenseYears + " " : string.Empty) +
                              ((casePersonSentencePunishment.SentenseMoney > 0) ? "Сума: " + casePersonSentencePunishment.SentenseMoney + " " : string.Empty);
                }

                foreach (var casePersonSentenceLawbase in casePersonSentence.CasePersonSentenceLawbases)
                {
                    result += casePersonSentenceLawbase.SentenceLawbase.Label + " ";
                }

            }

            return result;
        }

        /// <summary>
        /// Справка oбразувани и свършени дела за корупционни престъпления
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<CaseSprVM> CaseCorruptCrimes_Select(CaseFilterReport filter)
        {
            List<CaseSprVM> result = new List<CaseSprVM>();

            DateTime dateNow = DateTime.Now;

            filter.DateFrom = filter.DateFrom.ForceStartDateWithAddYear(-100);
            filter.DateTo = filter.DateTo.ForceEndDateWithAddYear(100);
            filter.LifecycleDateToFrom = filter.LifecycleDateToFrom.ForceStartDate();
            filter.LifecycleDateToTo = filter.LifecycleDateToTo.ForceEndDate();

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (filter.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Any(a => a.CaseSessionId == null &&
                                                                   (a.DateTo ?? dateNow.AddYears(100)).Date >= x.RegDate &&
                                                                   a.LawUnitId == filter.JudgeReporterId &&
                                                                   a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            Expression<Func<Case, bool>> lifecycleDateToSearch = x => true;
            if (filter.LifecycleDateToFrom != null && filter.LifecycleDateToTo != null)
                lifecycleDateToSearch = x => x.CaseLifecycles.Any(l => l.DateExpired == null &&
                                                                       l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                       l.DateTo != null &&
                                                                       l.DateTo >= filter.LifecycleDateToFrom &&
                                                                       l.DateTo <= filter.LifecycleDateToTo);


            var casePersonSentenceQuery = repo.AllReadonly<CasePersonSentence>();
            var caseSessionActQery = repo.AllReadonly<CaseSessionAct>();

            return repo.AllReadonly<Case>()
                       .Where(x => x.CourtId == userContext.CourtId &&
                                   x.RegDate >= filter.DateFrom &&
                                   x.RegDate <= filter.DateTo &&
                                   x.CaseClassifications.Any(c => c.DateTo == null && c.ClassificationId == NomenclatureConstants.CaseClassifications.CorruptCase) &&
                                   !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                       .Where(judgeReporterSearch)
                       .Where(lifecycleDateToSearch)
                       .Select(x => new CaseSprVM()
                       {
                           Id = x.Id,
                           CaseRegNum = x.RegNumber + "/" + x.RegDate.Year.ToString() + "г.",
                           CaseRegDate = x.RegDate,
                           CourtLabel = x.Court.Label,
                           CasePersons = string.Join("", x.CasePersons.Where(p => p.CaseSessionId == null &&
                                                                                  p.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.RightSide)
                                                                      .Select(p => p.FullName + "</p>")),
                           CasePerson = x.CasePersons.Where(p => p.CaseSessionId == null &&
                                                                 p.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.RightSide)
                                                     .Select(p => p.FullName)
                                                     .FirstOrDefault(),
                           CasePersonRoles = string.Join("", x.CasePersons.Where(p => p.CaseSessionId == null &&
                                                                                      p.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.RightSide)
                                                                          .Select(p => p.PersonRole.Label + "</p>")),
                           CasePersonRole = x.CasePersons.Where(p => p.CaseSessionId == null &&
                                                                     p.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.RightSide)
                                                         .Select(p => p.PersonRole.Label)
                                                         .FirstOrDefault(),
                           CaseCodeLabel = x.CaseCode.Label,
                           CasePersonSentenceInfo = string.Join(" ", casePersonSentenceQuery.Where(s => s.CaseId == x.Id &&
                                                                                                        (s.IsActive ?? false))
                                                                                            .Select(s => s.CasePerson.FullName + ": " +
                                                                                                         s.CaseSessionAct.ActType.Label + " " +
                                                                                                         s.CaseSessionAct.RegNumber + "/" +
                                                                                                         (s.CaseSessionAct.RegDate ?? dateNow).Year + " " +
                                                                                                         s.Description + " " +
                                                                                                         s.SentenceResultType.Label + " " +
                                                                                                         string.Join(" ", s.CasePersonSentencePunishments.Select(p => p.SentenceType.Label + " " +
                                                                                                                                                                      ((p.SentenseDays > 0) ? "Дни: " + p.SentenseDays + " " : string.Empty) +
                                                                                                                                                                      ((p.SentenseWeeks > 0) ? "Седмици: " + p.SentenseWeeks + " " : string.Empty) +
                                                                                                                                                                      ((p.SentenseMonths > 0) ? "Месеци: " + p.SentenseMonths + " " : string.Empty) +
                                                                                                                                                                      ((p.SentenseYears > 0) ? "Години: " + p.SentenseYears + " " : string.Empty) +
                                                                                                                                                                      ((p.SentenseMoney > 0) ? "Сума: " + p.SentenseMoney + " " : string.Empty))) +
                                                                                                        string.Join("", s.CasePersonSentenceLawbases.Select(l => " " + l.SentenceLawbase.Label)))),
                           JudgeReport = x.CaseLawUnits.Where(l => l.CaseSessionId == null &&
                                                                   l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                                   (l.DateTo ?? DateTime.Now.AddYears(100)) >= x.RegDate)
                                                       .Select(l => l.LawUnit.FullName + ((l.CourtDepartmentId != null) ? " състав: " + l.CourtDepartment.Label : string.Empty))
                                                       .FirstOrDefault(),
                           IsExistFinalAct = caseSessionActQery.Any(a => a.CaseId == x.Id &&
                                                                         a.DateExpired == null &&
                                                                         a.IsFinalDoc) ? "Да" : "Не",
                           DateFinishCase = x.CaseLifecycles.Any(l => l.DateExpired == null &&
                                                                      l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                      l.DateTo != null) ? x.CaseLifecycles.Where(l => l.DateExpired == null &&
                                                                                                                      l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                                                                      l.DateTo != null)
                                                                                                          .OrderByDescending(l => l.Id)
                                                                                                          .Select(l => l.DateTo)
                                                                                                          .FirstOrDefault() : (DateTime?)null
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Справка Свършили/Несвършили дела с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<CaseSprVM> CaseFinalActMaturity_Select(CaseFilterReport filter)
        {
            DateTime dateNow = DateTime.Now;

            filter.DateFrom = filter.DateFrom.ForceStartDateWithAddYear(-100);
            filter.DateTo = filter.DateTo.ForceEndDateWithAddYear(0);
            filter.DateToSpr = filter.DateToSpr.ForceEndDateWithAddYear(0);

            var resultFinish = repo.AllReadonly<SessionResultGrouping>()
                                   .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseWithoutFinalAct_Result)
                                   .Select(x => x.SessionResultId);

            Expression<Func<Case, bool>> caseStateWhere = x => true;
            if ((filter.CaseStateId ?? 0) > 0)
                caseStateWhere = x => x.CaseStateId == filter.CaseStateId;

            Expression<Func<Case, bool>> caseGroupIdWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupIdWhere = x => x.CaseGroupId == filter.CaseGroupId;

            Expression<Func<Case, bool>> caseTypeIdWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeIdWhere = x => x.CaseTypeId == filter.CaseTypeId;

            Expression<Func<Case, bool>> caseCodeIdWhere = x => true;
            if (filter.CaseCodeId > 0)
                caseCodeIdWhere = x => x.CaseCodeId == filter.CaseCodeId;

            Expression<Func<Case, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.CaseCodeId ?? 0);
            }

            Expression<Func<Case, bool>> judgeReporterIdWhere = x => true;
            if (filter.JudgeReporterId > 0)
                judgeReporterIdWhere = x => x.CaseLawUnits.Any(a => a.CaseSessionId == null &&
                                                                    (a.DateTo ?? dateNow.AddYears(100)) >= filter.DateToSpr && a.LawUnitId == filter.JudgeReporterId &&
                                                                    a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            Expression<Func<Case, bool>> withFinalActWhere = x => !x.CaseLifecycles.Any(l => l.DateExpired == null &&
                                                                                             l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                                             l.DateTo == null) &&
                                                                  x.CaseSessionActs.Any(a => a.DateExpired == null &&
                                                                                             a.IsFinalDoc &&
                                                                                             a.RegDate >= x.CaseLifecycles.Where(l => l.DateExpired == null &&
                                                                                                                                      l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                                                                                                          .OrderByDescending(l => l.DateFrom)
                                                                                                                          .Select(l => l.DateFrom)
                                                                                                                          .FirstOrDefault()) &&
                                                                 x.CaseSessions.Any(s => s.DateExpired == null &&
                                                                                         s.DateFrom >= x.CaseLifecycles.Where(l => l.DateExpired == null &&
                                                                                                                                   l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                                                                                                       .OrderByDescending(l => l.DateFrom)
                                                                                                                       .Select(l => l.DateFrom)
                                                                                                                       .FirstOrDefault() &&
                                                                                         s.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                                       resultFinish.Contains(r.SessionResultId)));

            if (!filter.WithFinalAct)
                withFinalActWhere = x => x.CaseLifecycles.Any(l => l.DateFrom <= filter.DateToSpr &&
                                                                   l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                   l.DateExpired == null &&
                                                                   (l.DateTo == null || l.DateTo >= filter.DateToSpr)) ||
                                         !x.CaseSessionActs.Any(a => a.DateExpired == null &&
                                                                     a.IsFinalDoc &&
                                                                     a.RegDate <= filter.DateToSpr &&
                                                                     a.RegDate >= x.CaseLifecycles.Where(l => l.DateFrom <= filter.DateToSpr &&
                                                                                                              l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                                                              l.DateExpired == null &&
                                                                                                              (l.DateTo == null || l.DateTo >= filter.DateToSpr))
                                                                                                  .OrderByDescending(l => l.DateFrom)
                                                                                                  .Select(l => l.DateFrom)
                                                                                                  .FirstOrDefault()) ||
                                         !x.CaseSessions.Any(s => s.DateExpired == null &&
                                                                  s.DateFrom <= filter.DateToSpr &&
                                                                  s.DateFrom >= x.CaseLifecycles.Where(l => l.DateFrom <= filter.DateToSpr &&
                                                                                                              l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                                                              l.DateExpired == null &&
                                                                                                              (l.DateTo == null || l.DateTo >= filter.DateToSpr))
                                                                                                   .OrderByDescending(l => l.DateFrom)
                                                                                                   .Select(l => l.DateFrom)
                                                                                                   .FirstOrDefault() &&
                                                                  s.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                resultFinish.Contains(r.SessionResultId)));

            return repo.AllReadonly<Case>()
                       .Where(x => x.CourtId == userContext.CourtId &&
                                   x.RegDate >= filter.DateFrom &&
                                   x.RegDate <= (filter.DateTo ?? dateNow.AddYears(100)) &&
                                   x.RegDate <= filter.DateToSpr &&
                                   x.CaseStateId != NomenclatureConstants.CaseState.Draft &&
                                   x.CasePersons.Any(p => p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderAged ||
                                                          p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderLegalAge) &&
                                   !x.CaseDeactivations.Any(d => d.CaseId == x.Id &&
                                                                 d.DateExpired == null))
                       .Where(caseStateWhere)
                       .Where(caseGroupIdWhere)
                       .Where(caseTypeIdWhere)
                       .Where(caseCodeIdWhere)
                       .Where(judgeReporterIdWhere)
                       .Where(withFinalActWhere)
                       .Where(caseCodeIdsWhere)
                       .Select(x => new CaseSprVM()
                       {
                           Id = x.Id,
                           JudgeReport = x.CaseLawUnits.Where(l => l.CaseSessionId == null &&
                                                                   (l.DateTo ?? dateNow.AddYears(100)) >= filter.DateToSpr &&
                                                                   l.DateFrom <= filter.DateToSpr &&
                                                                   l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                       .OrderByDescending(l => l.DateFrom)
                                                       .Select(l => l.LawUnit.FullName + ((l.CourtDepartment != null) ? " състав: " + l.CourtDepartment.Label : string.Empty))
                                                       .FirstOrDefault(),
                           CaseTypeLabel = x.CaseType.Label,
                           CaseRegNum = x.RegNumber,
                           CaseCodeLabel = x.CaseCode.Label + " - " + x.CaseCode.Code,
                           CaseRegDate = x.RegDate,
                           CaseEndDate = filter.WithFinalAct ? x.CaseSessionActs.Where(a => a.IsFinalDoc &&
                                                                                            a.DateExpired == null)
                                                                                .Select(a => a.ActDeclaredDate)
                                                                                .FirstOrDefault() : null,
                           CasePersons = string.Join(", ", x.CasePersons.Where(p => p.CaseSessionId == null &&
                                                                                    (p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderAged ||
                                                                                    p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderLegalAge))
                                                                        .Select(p => p.FullName + (p.PersonRole != null ? " (" + p.PersonRole.Label + ")" : string.Empty) + (p.PersonMaturity != null ? " - " + p.PersonMaturity.Label : string.Empty))),
                           CasePerson = x.CasePersons.Where(p => p.CaseSessionId == null &&
                                                                 (p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderAged ||
                                                                 p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderLegalAge))
                                                     .Select(p => p.FullName)
                                                     .FirstOrDefault()
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Справка за несвършени дела към дата 
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<CaseSprVM> CaseWithoutFinal_Select(CaseFilterReport filter)
        {
            DateTime dateNow = DateTime.Now;

            filter.DateFrom = filter.DateFrom.ForceStartDateWithAddYear(-100);
            filter.DateTo = filter.DateTo.ForceEndDateWithAddYear(0);
            filter.DateToSpr = filter.DateToSpr.ForceEndDateWithAddYear(0);

            Expression<Func<Case, bool>> dateWhere = x => (x.RegDate >= filter.DateFrom) && (x.RegDate <= filter.DateTo) && (x.RegDate <= filter.DateToSpr);

            var resultFinishQuery = readonlyrepo.AllReadonly<SessionResultGrouping>()
                                                .Where(g => g.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseWithoutFinalAct_Result)
                                                .Select(g => g.SessionResultId);

            Expression<Func<Case, bool>> caseStateWhere = x => true;
            if ((filter.CaseStateId ?? 0) > 0)
                caseStateWhere = x => x.CaseStateId == filter.CaseStateId;

            Expression<Func<Case, bool>> caseGroupIdWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupIdWhere = x => x.CaseGroupId == filter.CaseGroupId;

            Expression<Func<Case, bool>> caseTypeIdWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeIdWhere = x => x.CaseTypeId == filter.CaseTypeId;

            Expression<Func<Case, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.CaseCodeId ?? 0);
            }

            Expression<Func<Case, bool>> judgeReporterIdWhere = x => true;
            if (filter.JudgeReporterId > 0)
                judgeReporterIdWhere = x => x.CaseLawUnits.Any(a => a.CaseSessionId == null &&
                                                                    a.DateFrom <= filter.DateToSpr &&
                                                                    (a.DateTo ?? dateNow.AddYears(100)) >= filter.DateToSpr && a.LawUnitId == filter.JudgeReporterId &&
                                                                    a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            Expression<Func<Case, bool>> firstInstanceCourtIdWhere = x => true;
            if (filter.FirstInstanceCourtId > 0)
                firstInstanceCourtIdWhere = x => x.Document.DocumentCaseInfo.Any(c => c.CourtId == filter.FirstInstanceCourtId);

            var cseMigrationQuery = readonlyrepo.AllReadonly<CaseMigration>();

            var cseMigrationsInitCaseQuery = readonlyrepo.AllReadonly<CaseMigration>();

            var caseLifecyclesInProgressQuery = readonlyrepo.AllReadonly<CaseLifecycle>()
                                                    .Where(l => l.DateExpired == null &&
                                                                l.DateFrom <= filter.DateToSpr &&
                                                                l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress);

            var caseLifecyclesStopQuery = readonlyrepo.AllReadonly<CaseLifecycle>()
                                              .Where(l => l.DateExpired == null &&
                                                          l.DateFrom <= filter.DateToSpr &&
                                                          l.LifecycleTypeId == NomenclatureConstants.LifecycleType.Stop);

            var cseMigrationsQuery = readonlyrepo.AllReadonly<CaseMigration>()
                                                 .Include(m => m.Case);

            Expression<Func<Case, bool>> courtIdWhere = x => true;
            if (filter.CourtId == null)
                courtIdWhere = x => x.CourtId == userContext.CourtId;
            else
            {
                if (filter.CourtId > 0)
                    courtIdWhere = x => x.CourtId == filter.CourtId;
            }

            if (filter.CourtId != null && filter.CourtId > 0)
                courtIdWhere = x => x.CourtId == filter.CourtId;

            return readonlyrepo.AllReadonly<Case>()
                               .Where(x => x.CaseStateId != NomenclatureConstants.CaseState.Draft)
                               .Where(x => x.CaseLifecycles.Any(l => l.DateFrom <= filter.DateToSpr &&
                                                                     l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                     l.DateExpired == null &&
                                                                     (l.DateTo == null || l.DateTo >= filter.DateToSpr)) ||
                                           !x.CaseSessionActs.Any(a => a.DateExpired == null &&
                                                                       a.IsFinalDoc &&
                                                                       a.RegDate <= filter.DateToSpr &&
                                                                       a.RegDate >= x.CaseLifecycles.Where(l => l.DateFrom <= filter.DateToSpr &&
                                                                                                                l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                                                                l.DateExpired == null &&
                                                                                                                (l.DateTo == null || l.DateTo >= filter.DateToSpr))
                                                                                                    .OrderByDescending(l => l.DateFrom)
                                                                                                    .Select(l => l.DateFrom)
                                                                                                    .FirstOrDefault()) ||
                                           !x.CaseSessions.Any(s => s.DateExpired == null &&
                                                                    s.DateFrom <= filter.DateToSpr &&
                                                                    s.DateFrom.Date >= x.CaseLifecycles.Where(l => l.DateFrom <= filter.DateToSpr &&
                                                                                                                   l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                                                                   l.DateExpired == null &&
                                                                                                                   (l.DateTo == null || l.DateTo >= filter.DateToSpr))
                                                                                                        .OrderByDescending(l => l.DateFrom)
                                                                                                        .Select(l => l.DateFrom.Date)
                                                                                                        .FirstOrDefault() &&
                                                                    s.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                  resultFinishQuery.Contains(r.SessionResultId))))
                               .Where(dateWhere)
                               .Where(caseStateWhere)
                               .Where(caseGroupIdWhere)
                               .Where(caseTypeIdWhere)
                               .Where(caseCodeIdsWhere)
                               .Where(judgeReporterIdWhere)
                               .Where(firstInstanceCourtIdWhere)
                               .Where(courtIdWhere)
                               .Where(x => !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                               .Select(x => new CaseSprVM()
                               {
                                   Id = x.Id,
                                   CourtId = x.CourtId,
                                   CourtLabel = x.Court.Label,
                                   JudgeReport = x.CaseLawUnits.Where(u => u.CaseSessionId == null &&
                                                                           ((u.DateTo ?? dateNow.AddYears(100)) >= filter.DateToSpr) &&
                                                                           (u.DateFrom.Date <= (filter.DateToSpr ?? dateNow).Date) &&
                                                                           u.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                               .OrderByDescending(u => u.DateFrom)
                                                               .Select(u => u.LawUnit.FullName + ((u.CourtDepartment != null) ? " състав: " + u.CourtDepartment.Label : string.Empty))
                                                               .FirstOrDefault(),
                                   CaseTypeLabel = x.CaseType.Label,
                                   CaseRegNum = x.RegNumber,
                                   CaseCodeLabel = x.CaseCode.Label + " - " + x.CaseCode.Code,
                                   CaseRegDate = x.RegDate,
                                   CaseEndDate = (x.CaseSessions.Any(s => s.CaseSessionActs.Any(a => a.IsFinalDoc &&
                                                                                                     a.DateExpired == null &&
                                                                                                     a.ActDeclaredDate > filter.DateToSpr)) &&
                                                  x.CaseSessions.Any(s => s.CaseSessionResults.Any(a => a.DateExpired == null &&
                                                                                                        resultFinishQuery.Contains(a.SessionResultId)))) ? x.CaseSessions
                                                                                                                                                            .Where(s => s.CaseSessionActs.Any(a => a.IsFinalDoc &&
                                                                                                                                                                                                   a.DateExpired == null))
                                                                                                                                                            .Select(s => s.CaseSessionActs
                                                                                                                                                                          .Where(a => a.IsFinalDoc &&
                                                                                                                                                                                      a.DateExpired == null &&
                                                                                                                                                                                      a.ActDeclaredDate > filter.DateToSpr)
                                                                                                                                                                          .Select(a => a.ActDeclaredDate)
                                                                                                                                                                          .FirstOrDefault())
                                                                                                                                                            .FirstOrDefault()
                                                                                                                                                         : null,
                                   CaseStateName = x.CaseState.Label,
                                   FirstInstanceCourtLabel = x.Document.DocumentCaseInfo.Select(c => c.Court.Label).FirstOrDefault(),
                                   LifeCycleDateFrom = x.CaseLifecycles.Where(l => l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                                   l.DateFrom <= filter.DateToSpr &&
                                                                                   l.DateExpired == null &&
                                                                                   (l.DateTo == null || l.DateTo >= filter.DateToSpr))
                                                                       .Select(l => l.DateFrom)
                                                                       .FirstOrDefault(),
                                   LifeCycleDateTo = x.CaseLifecycles.Where(l => l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                                 l.DateFrom <= filter.DateToSpr &&
                                                                                 l.DateExpired == null &&
                                                                                 (l.DateTo == null || l.DateTo >= filter.DateToSpr))
                                                                     .Select(l => l.DateFrom)
                                                                     .FirstOrDefault(),
                                   DurationMonths = x.CaseLifecycles.Where(l => l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                                l.DateExpired == null &&
                                                                                l.DateFrom <= filter.DateToSpr &&
                                                                                l.DateTo != null && l.DateTo <= filter.DateToSpr)
                                                                    .Sum(l => l.DurationMonths) +
                                                    (cseMigrationsQuery.Any(m => m.InitialCaseId == cseMigrationsQuery.Where(b => b.CaseId == x.Id).Select(b => b.InitialCaseId).FirstOrDefault() &&
                                                                                 m.CaseId < x.Id &&
                                                                                 m.CourtId == x.CourtId) ? cseMigrationsQuery.Where(m => m.InitialCaseId == cseMigrationsQuery.Where(b => b.CaseId == x.Id).Select(b => b.InitialCaseId).FirstOrDefault() &&
                                                                                                                                         m.CaseId < x.Id &&
                                                                                                                                         m.CourtId == x.CourtId)
                                                                                                                             .Sum(m => caseLifecyclesInProgressQuery.Where(l => l.CaseId == m.CaseId).Sum(l => l.DurationMonths)) : 0),
                                   CaseMigrationLifeCycleInfo = cseMigrationsQuery.Any(m => m.InitialCaseId == cseMigrationsQuery.Where(b => b.CaseId == x.Id).Select(b => b.InitialCaseId).FirstOrDefault() &&
                                                                                            m.CaseId < x.Id &&
                                                                                            m.CourtId == x.CourtId) ? string.Join("", cseMigrationsQuery.Where(m => m.InitialCaseId == cseMigrationsQuery.Where(b => b.CaseId == x.Id).Select(b => b.InitialCaseId).FirstOrDefault() &&
                                                                                                                                                                     m.CaseId < x.Id &&
                                                                                                                                                                     m.CourtId == x.CourtId)
                                                                                                                                                        .Select(m => m.Case.RegNumber + "/" + m.Case.RegDate.ToString("dd.MM.yyyy") + "; ")) : string.Empty,
                                   LifeCycleDaysProgress = x.CaseLifecycles.Where(l => l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                                       l.DateFrom <= filter.DateToSpr &&
                                                                                       l.DateExpired == null &&
                                                                                       (l.DateTo == null || l.DateTo >= filter.DateToSpr))
                                                                           .Select(l => ((l.DateTo == null ? (filter.DateToSpr ?? dateNow) : (l.DateTo > filter.DateToSpr ? (filter.DateToSpr ?? dateNow) : (l.DateTo ?? dateNow))) - l.DateFrom).Days)
                                                                           .FirstOrDefault(),
                                   LifeCycleDaysStop = x.CaseLifecycles.Where(l => l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                                   l.DateFrom <= filter.DateToSpr &&
                                                                                   l.DateExpired == null &&
                                                                                   (l.DateTo == null || l.DateTo >= filter.DateToSpr))
                                                                       .Select(l => caseLifecyclesStopQuery.Where(s => s.CaseId == l.CaseId && s.Iteration == l.Iteration)
                                                                                                            .Sum(s => (s.DateTo != null && s.DateTo <= filter.DateToSpr) ? ((s.DateTo ?? dateNow) - s.DateFrom.Date).Days : ((filter.DateToSpr ?? dateNow) - s.DateFrom.Date).Days))
                                                                       .FirstOrDefault(),
                               })
                               .AsQueryable();
        }

        /// <summary>
        /// Експорт на ексел справка несвършени дела към дата
        /// </summary>
        /// <param name="model">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public byte[] CaseWithoutFinalExportExcel(CaseFilterReport model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = CaseWithoutFinal_Select(model).ToList();

            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Справка Несвършени дела към дата " + model.DateToSpr?.ToString(FormattingConstant.NormalDateFormat), 8,
                      styleTitle); excelService.AddRow();

            excelService.AddList(
                dataRows,
                new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<CaseSprVM, object>>>()
                {
                    x => x.CaseTypeLabel,
                    x => x.CaseRegNum,
                    x => x.JudgeReport,
                    x => x.CaseCodeLabel,
                    x => x.CaseRegDate,
                    x => x.CaseEndDate,
                    x => x.LifeCycleInfo,
                    x => x.CaseStateName,
                    x => x.FirstInstanceCourtLabel,
                },
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );

            return excelService.ToArray();
        }

        /// <summary>
        /// Експорт на ексел справка несвършени дела към дата със съд
        /// </summary>
        /// <param name="model">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public byte[] CaseWithoutFinalWithCourtExportExcel(CaseFilterReport model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = CaseWithoutFinal_Select(model).ToList();

            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Справка Несвършени дела към дата " + model.DateToSpr?.ToString(FormattingConstant.NormalDateFormat), 8,
                      styleTitle); excelService.AddRow();

            excelService.AddList(
                dataRows,
                new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<CaseSprVM, object>>>()
                {
                    x => x.CourtLabel,
                    x => x.CaseTypeLabel,
                    x => x.CaseRegNum,
                    x => x.JudgeReport,
                    x => x.CaseCodeLabel,
                    x => x.CaseRegDate,
                    x => x.CaseEndDate,
                    x => x.LifeCycleInfo,
                    x => x.CaseStateName,
                    x => x.FirstInstanceCourtLabel,
                },
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );

            return excelService.ToArray();
        }

        /// <summary>
        /// Справка за Справка за предоставени/върнати документи
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<DocumentProvidedReturnedSprVM> DocumentProvidedReturned_Select(int courtId, CaseFilterReport model)
        {
            List<DocumentProvidedReturnedSprVM> result = new List<DocumentProvidedReturnedSprVM>();

            model.DateFrom = NomenclatureExtensions.ForceStartDate(model.DateFrom ?? DateTime.Now.AddYears(-100));
            model.DateTo = NomenclatureExtensions.ForceEndDate(model.DateTo ?? DateTime.Now.AddYears(100));

            Expression<Func<CaseMigration, bool>> caseGroupIdWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupIdWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            Expression<Func<CaseMigration, bool>> caseTypeIdWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeIdWhere = x => x.Case.CaseTypeId == model.CaseTypeId;

            Expression<Func<CaseMigration, bool>> documentGroupIdWhere = x => true;
            if (model.DocumentGroupId > 0)
                documentGroupIdWhere = x => x.OutDocument.DocumentGroupId == model.DocumentGroupId;

            var caseMigrations = repo.AllReadonly<CaseMigration>()
                                     .Where(x => (x.CourtId == courtId) &&
                                                 ((x.OutDocument.DocumentDate >= model.DateFrom) && (x.OutDocument.DocumentDate <= model.DateTo)))
                                     .Where(caseGroupIdWhere)
                                     .Where(caseTypeIdWhere)
                                     .Where(documentGroupIdWhere)
                                     .Select(x => new DocumentProvidedReturnedSprVM()
                                     {
                                         CaseInfo = x.Case.CaseGroup.Label + " " + x.Case.RegNumber + "/" + x.Case.RegDate.ToString("dd.MM.yyyy"),
                                         Id = x.Case.Id,
                                         DocumentInfo = x.OutDocument.DocumentType.Label + " " + x.OutDocument.DocumentNumber + "/" + x.OutDocument.DocumentDate.ToString("dd.MM.yyyy"),
                                         StateLabel = x.CaseMigrationType.Label,
                                         Date = x.OutDocument.DocumentDate,
                                         Description = x.Description,
                                         UserName = x.User.LawUnit.FullName
                                     })
                                     .ToList() ?? new List<DocumentProvidedReturnedSprVM>();

            Expression<Func<CaseMovement, bool>> caseGroupIdCaseMovementWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupIdCaseMovementWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            Expression<Func<CaseMovement, bool>> caseTypeIdCaseMovementWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeIdCaseMovementWhere = x => x.Case.CaseTypeId == model.CaseTypeId;

            var caseMovements = repo.AllReadonly<CaseMovement>()
                                    .Where(x => (x.CourtId == courtId) &&
                                                ((x.DateSend >= model.DateFrom) && (x.DateSend <= model.DateTo)))
                                    .Where(caseGroupIdCaseMovementWhere)
                                    .Where(caseTypeIdCaseMovementWhere)
                                    .Select(x => new DocumentProvidedReturnedSprVM()
                                    {
                                        CaseInfo = x.Case.CaseGroup.Label + " " + x.Case.RegNumber + "/" + x.Case.RegDate.ToString("dd.MM.yyyy"),
                                        Id = x.Case.Id,
                                        DocumentInfo = x.MovementType.Label,
                                        StateLabel = "Приета на: " + (x.DateAccept ?? x.DateSend).ToString("dd.MM.yyyy HH:mm"),
                                        Date = x.DateSend,
                                        Description = x.Description,
                                        UserName = x.User.LawUnit.FullName
                                    })
                                    .ToList() ?? new List<DocumentProvidedReturnedSprVM>();

            Expression<Func<CaseEvidenceMovement, bool>> caseGroupIdCaseEvidenceMovementWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupIdCaseEvidenceMovementWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            Expression<Func<CaseEvidenceMovement, bool>> caseTypeIdCaseEvidenceMovementWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeIdCaseEvidenceMovementWhere = x => x.Case.CaseTypeId == model.CaseTypeId;

            var caseEvidenceMovements = repo.AllReadonly<CaseEvidenceMovement>()
                                            .Where(x => (x.CourtId == courtId) &&
                                                        ((x.MovementDate >= model.DateFrom) && (x.MovementDate <= model.DateTo)))
                                            .Where(caseGroupIdCaseEvidenceMovementWhere)
                                            .Where(caseTypeIdCaseEvidenceMovementWhere)
                                            .Select(x => new DocumentProvidedReturnedSprVM()
                                            {
                                                CaseInfo = x.Case.CaseGroup.Label + " " + x.Case.RegNumber + "/" + x.Case.RegDate.ToString("dd.MM.yyyy"),
                                                Id = x.Case.Id,
                                                DocumentInfo = x.CaseEvidence.EvidenceType.Label + " " + x.CaseEvidence.Description,
                                                StateLabel = x.EvidenceMovementType.Label,
                                                Date = x.MovementDate,
                                                Description = x.CaseEvidence.Description + " " + x.CaseEvidence.AddInfo,
                                                UserName = x.User.LawUnit.FullName
                                            })
                                            .ToList() ?? new List<DocumentProvidedReturnedSprVM>();

            result.AddRange(caseMigrations);
            result.AddRange(caseMovements);
            result.AddRange(caseEvidenceMovements);

            return result.AsQueryable();
        }

        /// <summary>
        /// Справка за срочност за насрочване на дела
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<CaseSprVM> CaseBeginReport_Select(CaseFilterReport filter)
        {
            DateTime dateNow = DateTime.Now;

            filter.DateFrom = filter.DateFrom.ForceStartDateWithAddYear(-100);
            filter.DateTo = filter.DateTo.ForceEndDateWithAddYear(100);

            Expression<Func<Case, bool>> sessionTo1MonthValueWhere = x => true;
            if (filter.SessionDateToId == NomenclatureConstants.SessionToDateValue.SessionTo1MonthValue)
                sessionTo1MonthValueWhere = x => x.CaseSessions.Any(s => s.DateExpired == null &&
                                                                         s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession) ? x.CaseSessions.Where(s => s.DateExpired == null &&
                                                                                                                                                                         s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession)
                                                                                                                                                             .OrderBy(s => s.DateFrom)
                                                                                                                                                             .Select(s => s.DateFrom)
                                                                                                                                                             .FirstOrDefault() <= x.RegDate.AddDays(30) : false;

            Expression<Func<Case, bool>> sessionTo2MonthValueWhere = x => true;
            if (filter.SessionDateToId == NomenclatureConstants.SessionToDateValue.SessionTo2MonthValue)
                sessionTo2MonthValueWhere = x => x.CaseSessions.Any(s => s.DateExpired == null &&
                                                                         s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession) ? x.CaseSessions.Where(s => s.DateExpired == null &&
                                                                                                                                                                         s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession)
                                                                                                                                                             .OrderBy(s => s.DateFrom)
                                                                                                                                                             .Select(s => s.DateFrom)
                                                                                                                                                             .FirstOrDefault() >= x.RegDate.AddDays(31) &&
                                                                                                                                               x.CaseSessions.Where(s => s.DateExpired == null &&
                                                                                                                                                                         s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession)
                                                                                                                                                             .OrderBy(s => s.DateFrom)
                                                                                                                                                             .Select(s => s.DateFrom)
                                                                                                                                                             .FirstOrDefault() >= x.RegDate.AddDays(60) : false;

            Expression<Func<Case, bool>> sessionTo3MonthValueWhere = x => true;
            if (filter.SessionDateToId == NomenclatureConstants.SessionToDateValue.SessionTo3MonthValue)
                sessionTo3MonthValueWhere = x => x.CaseSessions.Any(s => s.DateExpired == null &&
                                                                         s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession) ? x.CaseSessions.Where(s => s.DateExpired == null &&
                                                                                                                                                                         s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession)
                                                                                                                                                             .OrderBy(s => s.DateFrom)
                                                                                                                                                             .Select(s => s.DateFrom)
                                                                                                                                                             .FirstOrDefault() >= x.RegDate.AddDays(61) &&
                                                                                                                                               x.CaseSessions.Where(s => s.DateExpired == null &&
                                                                                                                                                                         s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession)
                                                                                                                                                             .OrderBy(s => s.DateFrom)
                                                                                                                                                             .Select(s => s.DateFrom)
                                                                                                                                                             .FirstOrDefault() >= x.RegDate.AddDays(90) : false;

            Expression<Func<Case, bool>> sessionUp3MonthValueWhere = x => true;
            if (filter.SessionDateToId == NomenclatureConstants.SessionToDateValue.SessionUp3MonthValue)
                sessionUp3MonthValueWhere = x => x.CaseSessions.Any(s => s.DateExpired == null &&
                                                                         s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession) ? x.CaseSessions.Where(s => s.DateExpired == null &&
                                                                                                                                                                         s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession)
                                                                                                                                                             .OrderBy(s => s.DateFrom)
                                                                                                                                                             .Select(s => s.DateFrom)
                                                                                                                                                             .FirstOrDefault() >= x.RegDate.AddDays(91) : false;

            Expression<Func<Case, bool>> noSessionValueWhere = x => true;
            if (filter.SessionDateToId == NomenclatureConstants.SessionToDateValue.NoSessionValue)
                noSessionValueWhere = x => !x.CaseSessions.Any(s => s.DateExpired == null &&
                                                                    s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession);

            Expression<Func<Case, bool>> caseGroupIdWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupIdWhere = x => x.CaseGroupId == filter.CaseGroupId;

            Expression<Func<Case, bool>> caseTypeIdWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeIdWhere = x => x.CaseTypeId == filter.CaseTypeId;

            Expression<Func<Case, bool>> sessionTypeIdWhere = x => true;
            if (filter.SessionTypeId > 0)
                sessionTypeIdWhere = x => x.CaseSessions.Where(s => s.DateExpired == null &&
                                                                    s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession)
                                                        .OrderBy(s => s.DateFrom)
                                                        .Select(s => s.SessionTypeId)
                                                        .FirstOrDefault() == filter.SessionTypeId;

            Expression<Func<Case, bool>> judgeReporterIdWhere = x => true;
            if (filter.JudgeReporterId > 0)
                judgeReporterIdWhere = x => x.CaseLawUnits.Any(a => a.CaseSessionId == null &&
                                                                    (a.DateTo ?? dateNow.AddYears(100)).Date >= x.RegDate.Date &&
                                                                    a.LawUnitId == filter.JudgeReporterId &&
                                                                    a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            Expression<Func<Case, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.CaseCodeId ?? 0);
            }

            return readonlyrepo.AllReadonly<Case>()
                       .Where(x => x.CourtId == userContext.CourtId)
                       .Where(x => !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                       .Where(x => x.RegDate >= filter.DateFrom && x.RegDate <= (filter.DateTo ?? dateNow.AddYears(100)))
                       .Where(sessionTo1MonthValueWhere)
                       .Where(sessionTo2MonthValueWhere)
                       .Where(sessionTo3MonthValueWhere)
                       .Where(sessionUp3MonthValueWhere)
                       .Where(noSessionValueWhere)
                       .Where(caseGroupIdWhere)
                       .Where(caseTypeIdWhere)
                       .Where(caseCodeIdsWhere)
                       .Where(sessionTypeIdWhere)
                       .Where(judgeReporterIdWhere)
                       .Select(x => new CaseSprVM()
                       {
                           Id = x.Id,
                           CaseTypeLabel = x.CaseType.Label,
                           CaseRegNum = x.RegNumber,
                           CaseCodeLabel = x.CaseCode.Code + " " + x.CaseCode.Label,
                           DocumentDate = x.Document.DocumentDate,
                           ResolutionJudge = x.Document.DocumentResolutions.Where(d => d.ResolutionTypeId == DocumentConstants.ResolutionTypes.ResolutionForScheduling &&
                                                                                       d.DateExpired == null)
                                                                           .Select(d => (d.JudgeDecisionCount == 2) ? d.JudgeDecisionLawunit.FullName + ", " + d.JudgeDecisionLawunit2.FullName : ((d.JudgeDecisionCount == 1) ? d.JudgeDecisionLawunit.FullName : string.Empty))
                                                                           .FirstOrDefault(),
                           CaseRegDate = x.RegDate,
                           JudgeReport = x.CaseLawUnits.Where(l => l.CaseSessionId == null &&
                                                                   l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                                   (l.DateTo ?? DateTime.Now.AddYears(100)) >= DateTime.Now)
                                                       .Select(l => l.LawUnit.FullName + ((l.CourtDepartment != null) ? " състав: " + l.CourtDepartment.Label : string.Empty))
                                                       .FirstOrDefault(),
                           SessionTypeLabel = x.CaseSessions.Where(s => s.DateExpired == null &&
                                                                        s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession)
                                                            .OrderBy(s => s.DateFrom)
                                                            .Select(s => s.SessionType.Label)
                                                            .FirstOrDefault(),
                           SessionDateFrom = x.CaseSessions.Any(s => s.DateExpired == null &&
                                                                     s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession) ? x.CaseSessions.Where(s => s.DateExpired == null &&
                                                                                                                                                                     s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession)
                                                                                                                                                         .OrderBy(s => s.DateFrom)
                                                                                                                                                         .Select(s => s.DateFrom)
                                                                                                                                                         .FirstOrDefault() : (DateTime?)null,
                           ActFinalInfo = x.CaseSessionActs.Where(a => a.ActStateId != NomenclatureConstants.SessionActState.Project &&
                                                                       a.DateExpired == null &&
                                                                       a.IsFinalDoc)
                                                           .Select(a => a.ActType.Label + " " + a.RegNumber + "/" + (a.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"))
                                                           .FirstOrDefault()
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Справка за Справка за срочност за изготвяне на съдебен акт
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseSprVM> CaseActReport_Select(int courtId, CaseFilterReport model)
        {
            model.DateFrom = model.DateFrom.ForceStartDate();
            model.DateTo = model.DateTo.ForceEndDate();
            model.Session_DateFrom = model.Session_DateFrom.ForceStartDate();
            model.SessionDateTo = model.SessionDateTo.ForceEndDate();
            model.ActDateFrom = model.ActDateFrom.ForceStartDate();
            model.ActDateTo = model.ActDateTo.ForceEndDate();

            Expression<Func<CaseSessionAct, bool>> regDateSearch = x => true;
            if (model.DateFrom != null && model.DateTo != null)
                regDateSearch = x => x.Case.RegDate >= model.DateFrom && x.Case.RegDate <= model.DateTo;

            Expression<Func<CaseSessionAct, bool>> sessionDateSearch = x => true;
            if (model.Session_DateFrom != null && model.SessionDateTo != null)
                sessionDateSearch = x => x.CaseSession.DateFrom >= model.Session_DateFrom && x.CaseSession.DateFrom <= model.SessionDateTo;

            Expression<Func<CaseSessionAct, bool>> actDateSearch = x => true;
            if (model.ActDateFrom != null && model.ActDateTo != null)
                actDateSearch = x => x.ActDeclaredDate >= model.ActDateFrom && x.ActDeclaredDate <= model.ActDateTo;

            Expression<Func<CaseSessionAct, bool>> finalActSearch = x => true;
            if (model.ActIsFinalDoc == true)
                finalActSearch = x => x.IsFinalDoc == true && NomenclatureConstants.SessionActState.EnforcedStates.Contains(x.ActStateId);

            Expression<Func<CaseSessionAct, bool>> canAppealSearch = x => true;
            if (model.CanAppeal == true)
                canAppealSearch = x => x.CanAppeal == true;

            Expression<Func<CaseSessionAct, bool>> caseRegNumber = x => true;
            if (!string.IsNullOrEmpty(model.RegNumber))
                caseRegNumber = x => EF.Functions.ILike(x.Case.RegNumber, model.RegNumber.ToCasePaternSearch());

            Expression<Func<CaseSessionAct, bool>> actDateToSearch = x => true;
            if (model.ActDateToIds != null && model.ActDateToIds.Any())
            {
                bool isExistActDateTo1MonthValue = model.ActDateToIds.Any(m => int.Parse(m) == NomenclatureConstants.ActToDateValue.ActDateTo1MonthValue);
                bool isExistActDateTo2MonthValue = model.ActDateToIds.Any(m => int.Parse(m) == NomenclatureConstants.ActToDateValue.ActDateTo2MonthValue);
                bool isExistActDateTo3MonthValue = model.ActDateToIds.Any(m => int.Parse(m) == NomenclatureConstants.ActToDateValue.ActDateTo3MonthValue);
                bool isExistActDateTo1YeаrValue = model.ActDateToIds.Any(m => int.Parse(m) == NomenclatureConstants.ActToDateValue.ActDateTo1YeаrValue);
                bool isExistActDateUo1YeаrValue = model.ActDateToIds.Any(m => int.Parse(m) == NomenclatureConstants.ActToDateValue.ActDateUo1YeаrValue);
                actDateToSearch = x => (isExistActDateTo1MonthValue ? ((x.DeclaredMonthCount ?? 0) > 0 ? (x.DeclaredMonthCount == 1) : (x.ActDeclaredDate >= x.CaseSession.DateFrom && x.ActDeclaredDate <= x.CaseSession.DateFrom.AddDays(30))) : false) ||
                                       (isExistActDateTo2MonthValue ? ((x.DeclaredMonthCount ?? 0) > 0 ? (x.DeclaredMonthCount == 2) : (x.ActDeclaredDate >= x.CaseSession.DateFrom.AddDays(31) && x.ActDeclaredDate <= x.CaseSession.DateFrom.AddDays(60))) : false) ||
                                       (isExistActDateTo3MonthValue ? ((x.DeclaredMonthCount ?? 0) > 0 ? (x.DeclaredMonthCount == 3) : (x.ActDeclaredDate >= x.CaseSession.DateFrom.AddDays(61) && x.ActDeclaredDate <= x.CaseSession.DateFrom.AddDays(90))) : false) ||
                                       (isExistActDateTo1YeаrValue ? ((x.DeclaredMonthCount ?? 0) > 0 ? (x.DeclaredMonthCount > 3 && x.DeclaredMonthCount <= 12) : (x.ActDeclaredDate >= x.CaseSession.DateFrom.AddDays(91) && x.ActDeclaredDate <= x.CaseSession.DateFrom.AddDays(365))) : false) ||
                                       (isExistActDateUo1YeаrValue ? ((x.DeclaredMonthCount ?? 0) > 0 ? (x.DeclaredMonthCount > 12) : (x.ActDeclaredDate > x.CaseSession.DateFrom.AddDays(365))) : false);
            }

            Expression<Func<CaseSessionAct, bool>> actMotiveDateTo15DayValueSearch = x => true;
            if (model.ActMotiveDateToId == NomenclatureConstants.ActMotiveToDateValue.ActMotiveDateTo15DayValue)
                actMotiveDateTo15DayValueSearch = x => (x.ActTypeId == NomenclatureConstants.ActType.Sentence || x.ActTypeId == NomenclatureConstants.ActType.Answer) && x.ActMotivesDeclaredDate != null && ((x.ActDeclaredDate ?? DateTime.Now).AddDays(15) >= x.ActMotivesDeclaredDate);

            Expression<Func<CaseSessionAct, bool>> actMotiveDateTo60DayValueSearch = x => true;
            if (model.ActMotiveDateToId == NomenclatureConstants.ActMotiveToDateValue.ActMotiveDateTo60DayValue)
                actMotiveDateTo60DayValueSearch = x => (x.ActTypeId == NomenclatureConstants.ActType.Sentence || x.ActTypeId == NomenclatureConstants.ActType.Answer) && x.ActMotivesDeclaredDate != null && ((x.ActDeclaredDate ?? DateTime.Now).AddDays(60) >= x.ActMotivesDeclaredDate) && ((x.ActDeclaredDate ?? DateTime.Now).AddDays(15) < x.ActMotivesDeclaredDate);

            Expression<Func<CaseSessionAct, bool>> actMotiveDateToUp60DayValueSearch = x => true;
            if (model.ActMotiveDateToId == NomenclatureConstants.ActMotiveToDateValue.ActMotiveDateToUp60DayValue)
                actMotiveDateToUp60DayValueSearch = x => (x.ActTypeId == NomenclatureConstants.ActType.Sentence || x.ActTypeId == NomenclatureConstants.ActType.Answer) && x.ActMotivesDeclaredDate != null && ((x.ActDeclaredDate ?? DateTime.Now).AddDays(60) <= x.ActMotivesDeclaredDate);

            Expression<Func<CaseSessionAct, bool>> caseGroupIdWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupIdWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            Expression<Func<CaseSessionAct, bool>> caseTypeIdWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeIdWhere = x => x.Case.CaseTypeId == model.CaseTypeId;

            Expression<Func<CaseSessionAct, bool>> caseCodeIdWhere = x => true;
            if (model.CaseCodeId > 0)
                caseCodeIdWhere = x => x.Case.CaseCodeId == model.CaseCodeId;

            Expression<Func<CaseSessionAct, bool>> sessionTypeIdWhere = x => true;
            if (model.SessionTypeId > 0)
                sessionTypeIdWhere = x => x.Case.CaseSessions.Where(s => s.DateExpired == null).OrderBy(s => s.DateFrom).FirstOrDefault().SessionTypeId == model.SessionTypeId;

            Expression<Func<CaseSessionAct, bool>> sessionResultIdWhere = x => true;
            if (model.SessionResultId > 0)
                sessionResultIdWhere = x => x.CaseSession.CaseSessionResults.Any(r => r.SessionResultId == model.SessionResultId && r.DateExpired == null);

            Expression<Func<CaseSessionAct, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseSession.CaseLawUnits.Where(l => l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                                                 (l.DateTo ?? DateTime.Now.AddYears(100)) >= x.CaseSession.DateFrom &&
                                                                                 l.LawUnitId == model.JudgeReporterId).Any();

            return readonlyrepo.AllReadonly<CaseSessionAct>()
                       .Where(x => x.CourtId == courtId &&
                                   x.DateExpired == null &&
                                   x.ActStateId != NomenclatureConstants.SessionActState.Project &&
                                   x.Case.CaseStateId != NomenclatureConstants.CaseState.Draft &&
                                   x.ActDeclaredDate != null &&
                                   !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                       .Where(finalActSearch)
                       .Where(canAppealSearch)
                       .Where(caseRegNumber)
                       .Where(actDateToSearch)
                       .Where(actMotiveDateTo15DayValueSearch)
                       .Where(actMotiveDateTo60DayValueSearch)
                       .Where(actMotiveDateToUp60DayValueSearch)
                       .Where(caseGroupIdWhere)
                       .Where(caseTypeIdWhere)
                       .Where(caseCodeIdWhere)
                       .Where(sessionTypeIdWhere)
                       .Where(sessionResultIdWhere)
                       .Where(judgeReporterSearch)
                       .Where(regDateSearch)
                       .Where(sessionDateSearch)
                       .Where(actDateSearch)
                       .Select(x => new CaseSprVM()
                       {
                           Id = x.CaseId ?? 0,
                           CaseTypeLabel = x.Case.CaseType.Label,
                           CaseRegNum = x.Case.RegNumber,
                           CaseCodeLabel = x.Case.CaseCode.Code + " " + x.Case.CaseCode.Label,
                           SessionDateFrom = x.CaseSession.DateFrom,
                           SessionResult = string.Join(",", x.CaseSession.CaseSessionResults.Where(r => r.DateExpired == null).Select(r => r.SessionResult.Label + (r.SessionResultBaseId != null ? " - " + r.SessionResultBase.Label : string.Empty))),
                           SessionResultFirst = x.CaseSession
                                                 .CaseSessionResults
                                                 .Where(r => r.DateExpired == null)
                                                 .Select(r => r.SessionResult.Label)
                                                 .FirstOrDefault(),
                           JudgeReport = x.CaseSession.CaseLawUnits.Where(l => l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                                               (l.DateTo ?? DateTime.Now.AddYears(100)) >= x.CaseSession.DateFrom)
                                                                   .Select(l => l.LawUnit.FullName + ((l.CourtDepartment != null) ? " състав: " + l.CourtDepartment.Label : string.Empty))
                                                                   .FirstOrDefault(),
                           ActTypeLabel = x.ActType.Label,
                           SessionActDate = x.ActDate,
                           ActReturnDate = x.ActReturnDate,
                           ActMotivesDeclaredDate = x.ActMotivesDeclaredDate,
                           ActDeclaredDate = x.ActDeclaredDate
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Справка за Справка за времетраене на размяната на книжата (първи интервал)
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<CaseSprVM> CaseFirstLifecyclie_Select(CaseFilterReport filter)
        {
            filter.DateFrom = filter.DateFrom.ForceStartDateWithAddYear(-100);
            filter.DateTo = filter.DateTo.ForceEndDateWithAddYear(100);
            filter.Session_DateFrom = filter.Session_DateFrom.ForceStartDateWithAddYear(-100);
            filter.SessionDateTo = filter.SessionDateTo.ForceEndDateWithAddYear(100);

            DateTime dateNow = DateTime.Now;

            Expression<Func<CaseSession, bool>> caseGroupIdWhere = x => x.Case.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo;
            if (filter.CaseGroupId > 0)
                caseGroupIdWhere = x => x.Case.CaseGroupId == filter.CaseGroupId;

            Expression<Func<CaseSession, bool>> caseTypeIdWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeIdWhere = x => x.Case.CaseTypeId == filter.CaseTypeId;

            Expression<Func<CaseSession, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.Case.CaseCodeId ?? 0);
            }

            Expression<Func<CaseSession, bool>> processPriorityIdWhere = x => true;
            if (filter.ProcessPriorityId > 0)
                processPriorityIdWhere = x => x.Case.ProcessPriorityId == filter.ProcessPriorityId;

            Expression<Func<CaseSession, bool>> isDoubleExchangeDocWhere = x => x.Case.CaseClassifications.Any(c => c.ClassificationId == NomenclatureConstants.CaseClassifications.DoubleExchangeDoc);
            if (!filter.IsDoubleExchangeDoc)
                isDoubleExchangeDocWhere = x => !x.Case.CaseClassifications.Any(c => c.ClassificationId == NomenclatureConstants.CaseClassifications.DoubleExchangeDoc);

            return repo.AllReadonly<CaseSession>()
                       .Where(x => x.CourtId == userContext.CourtId)
                       .Where(x => x.DateFrom >= filter.Session_DateFrom)
                       .Where(x => x.DateFrom <= filter.SessionDateTo)
                       .Where(x => x.Case.RegDate >= filter.DateFrom)
                       .Where(x => x.Case.RegDate <= filter.DateTo)
                       .Where(x => x.Case.CaseStateId != NomenclatureConstants.CaseState.Draft)
                       .Where(x => x.SessionTypeId == NomenclatureConstants.SessionType.ClosedSession || x.SessionTypeId == NomenclatureConstants.SessionType.DispositionalSession)
                       .Where(x => x.Case.CaseTypeId != NomenclatureConstants.CaseTypes.VGD)
                       .Where(x => x.Case.CaseTypeId != NomenclatureConstants.CaseTypes.VChGD)
                       .Where(x => x.Case.CaseCodeId != NomenclatureConstants.CaseCode.AdmissionOfAdoption_0107_1)
                       .Where(isDoubleExchangeDocWhere)
                       .Where(caseGroupIdWhere)
                       .Where(caseTypeIdWhere)
                       .Where(caseCodeIdsWhere)
                       .Where(processPriorityIdWhere)
                       .Select(x => new CaseSprVM()
                       {
                           Id = x.CaseId,
                           CaseTypeLabel = x.Case.CaseType.Label,
                           CaseRegNum = x.Case.RegNumber,
                           CaseCodeLabel = x.Case.CaseCode.Code + " " + x.Case.CaseCode.Label,
                           CaseRegDate = x.Case.RegDate,
                           SessionDateFrom = x.DateFrom,
                           LifeCycleDateFrom = x.Case.CaseLifecycles.Any(l => l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                                l.DateFrom.Date <= x.DateFrom.Date &&
                                                                                (l.DateTo ?? dateNow.AddYears(100)).Date >= (x.DateTo ?? dateNow.AddYears(100)).Date &&
                                                                                l.Iteration == 1) ? x.Case.CaseLifecycles.Where(l => l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                                                                                     l.DateFrom.Date <= x.DateFrom.Date &&
                                                                                                                                     (l.DateTo ?? dateNow.AddYears(100)).Date >= (x.DateTo ?? dateNow.AddYears(100)).Date)
                                                                                                                         .Select(l => l.DateFrom)
                                                                                                                         .FirstOrDefault() : x.Case.RegDate,
                           LifeCycleDateTo = x.DateFrom
                       })
                       .OrderBy(x => x.SessionDateFrom)
                       .AsQueryable();
        }

        public async Task<bool> IsRegisterCompany(int caseId)
        {
            var documentTypeId = await repo.AllReadonly<Case>()
                                           .Where(x => x.Id == caseId)
                                           .Select(x => x.Document.DocumentTypeId)
                                           .FirstOrDefaultAsync();

            return await repo.AllReadonly<DocumentTypeGrouping>()
                             .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.RegisterCompany)
                             .Where(x => x.DocumentTypeId == documentTypeId)
                             .AnyAsync();
        }

        public bool IsCaseRestricted(int CaseId)
        {
            return repo.AllReadonly<CaseClassification>()
                       .Where(x => x.CaseId == CaseId)
                       .Any(x => NomenclatureConstants.CaseClassifications.RestictedAccess.Contains(x.ClassificationId));
        }

        public bool IsDoneCase(int CaseId)
        {
            var resultFinish = repo.AllReadonly<SessionResultGrouping>()
                                   .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseWithoutFinalAct_Result)
                                   .Select(x => x.SessionResultId)
                                   .ToArray();

            var result = repo.AllReadonly<Case>()
                             .Where(x => (x.Id == CaseId) &&
                                         (x.CaseStateId != NomenclatureConstants.CaseState.Draft) &&
                                         ((x.CaseSessionResults.Where(a => a.DateExpired == null && resultFinish.Contains(a.SessionResultId)).Any()) &&
                                         (x.CaseSessionActs.Any(b => b.DateExpired == null && b.IsFinalDoc && (b.ActStateId != NomenclatureConstants.SessionActState.Project && b.ActStateId != NomenclatureConstants.SessionActState.Registered)))))
                             .Where(x => !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                             .Any();

            return result;
        }

        /// <summary>
        /// Метод връщащ информация за дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<CaseDataInfoVM> GetCaseInfo(int caseId)
        {
            return await repo.AllReadonly<Case>()
                             .Where(x => x.Id == caseId)
                             .Select(x => new CaseDataInfoVM()
                             {
                                 Id = x.Id,
                                 RegNumber = x.RegNumber,
                                 RegDate = x.RegDate,
                                 ShortNumber = x.ShortNumber,
                                 CaseTypeCode = x.CaseType.Code,
                                 CaseTypeLabel = x.CaseType.Label,
                                 CaseInforcedDate = x.CaseInforcedDate,
                                 CaseGroupLabel = x.CaseGroup.Label,
                                 CaseGroupId = x.CaseGroupId,
                                 CaseInstanceId = x.CaseType.CaseInstanceId
                             })
                             .FirstAsync();
        }

        public async Task<bool> CheckCaseCodeForDebtorsCount(int caseCodeId)
        {
            return await repo.AllReadonly<CaseCodeGrouping>()
                    .Where(x => x.CaseCodeId == caseCodeId && x.CaseCodeGroup == CaseCodeGroupings.CaseCodesForDebtorsCountField)
                    .AnyAsync();
        }

        #region Обединяване на дела - бързо производство

        /// <summary>
        /// Зареждане на информация за обединяване на дела
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<MergerCaseFastProcessVM> GetMergerCaseFastProcess(int caseId)
        {
            return await repo.AllReadonly<Case>()
                             .Where(x => x.Id == caseId)
                             .Select(x => new MergerCaseFastProcessVM()
                             {
                                 CaseId = x.Id,
                                 CourtId = x.CourtId,
                                 DocumentId = x.DocumentId,
                                 CaseLabel = x.ShortNumber + "/" + x.RegDate.ToString("yyyy"),
                                 IsExistsDocumentCaseInfo = x.Document.DocumentCaseInfo.Any()
                             })
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Метод връщащ инфо за дело за обединяване на дела за бързо производство
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<string> GetFromCaseInfoForMergerCaseFastProcess(int caseId)
        {
            return await repo.AllReadonly<Case>()
                             .Where(x => x.Id == caseId)
                             .Select(x => x.ShortNumber +
                                          "/" + x.RegDate.ToString("yyyy") +
                                          ", тип: " + x.CaseType.Code +
                                          ", съд: " + x.Court.Label +
                                          ", шифър: " + x.CaseCode.Code +
                                          " страни: " + string.Join(", ", x.CasePersons
                                                                           .Where(p => p.DateExpired == null &&
                                                                                       p.CaseSessionId == null)
                                                                           .Select(p => p.FullName + " (" + p.PersonRole.Label + ")")))
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Метод връщащ запис към иницииращ документ за обединяване на дело
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private DocumentCaseInfo GetDocumentCaseInfoForMergerCaseFastProcess(MergerCaseFastProcessVM model)
        {
            return new()
            {
                DocumentId = model.DocumentId,
                CourtId = model.FromCourtId,
                CaseId = model.FromCaseId
            };
        }

        /// <summary>
        /// Метод връщащ запис в движение на дело за обединяване на дело
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private CaseMigration GetCaseMigrationForMergerCaseFastProcess(MergerCaseFastProcessVM model)
        {
            return new CaseMigration()
            {
                CourtId = model.CourtId,
                CaseId = model.CaseId,
                PriorCaseId = model.FromCaseId ?? 0,
                InitialCaseId = model.FromCaseId ?? 0,
                CaseMigrationTypeId = NomenclatureConstants.CaseMigrationTypes.CaseConnection,
                SendToCourtId = model.CourtId,
                SendToTypeId = NomenclatureConstants.CaseMigrationSendTo.Court,
                DateWrt = DateTime.Now,
                UserId = userContext.UserId,
            };
        }

        /// <summary>
        /// Обединяване на дела за бързо производство
        /// </summary>
        /// <param name="model">Model popylnen ot potrebitel</param>
        /// <returns></returns>
        public async Task<bool> MergerCaseFastProcess(MergerCaseFastProcessVM model)
        {
            try
            {
                DocumentCaseInfo documentCaseInfo = GetDocumentCaseInfoForMergerCaseFastProcess(model);
                CaseMigration caseMigration = GetCaseMigrationForMergerCaseFastProcess(model);
                await repo.AddAsync(documentCaseInfo);
                await repo.AddAsync(caseMigration);
                await repo.SaveChangesAsync();
                mqEpepService.AppendCaseMigration(caseMigration);

                await workNotificationService.TurnOffNotificationsForLackSubmittedObjectionFastProcess(model.CaseId);
                await workNotificationService.SaveNotificationsForNewCaseHigherInstanceWith0604_1_2FastProcess(model.CaseId);
                await workNotificationService.TurnOffNotificationsForFilingClaimFastProcess(model.CaseId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Проблем при обединяване на дело за бързо производство с ид: {model.CaseId} и ид {model.FromCaseId}");
                return false;
            }
        }

        #endregion
    }
}
