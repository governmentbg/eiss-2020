using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;

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
        private readonly IDocumentTemplateService documentTemplateService;
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
        private readonly IRegixReportService regixReportService;
        private readonly IDocumentService documentService;

        public CaseService(
        ILogger<CaseService> _logger,
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
        IDocumentTemplateService _documentTemplateService,
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
        IRegixReportService _regixReportService,
        IDocumentService _documentService,
        IUrlHelper _url,
        ICaseSessionFastDocumentService _caseSessionFastDocumentService,
        AutoMapper.IMapper _mapper)
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
            documentTemplateService = _documentTemplateService;
            mapper = _mapper;
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
            regixReportService = _regixReportService;
            caseSessionFastDocumentService = _caseSessionFastDocumentService;
            documentService = _documentService;
        }

        /// <summary>
        /// Извличане на данни за дела
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseVM> Case_Select(int courtId, CaseFilter model)
        {
            //var ct = repo.GetPropById<Court, dynamic>(x => x.Id == courtId, x => new { x.CourtTypeId, x.Label });
            //var ct = repo.GetPropById<Court, string>(x => x.Id == courtId, x => x.Label);


            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;
            DateTime dateNow = DateTime.Now;

            Expression<Func<Case, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate.Date >= dateFromSearch.Date && x.RegDate.Date <= dateToSearch.Date;

            Expression<Func<Case, bool>> yearSearch = x => true;
            if ((model.CaseYear ?? 0) > 0)
                yearSearch = x => x.RegDate.Year == model.CaseYear;

            Expression<Func<Case, bool>> documentSearch = x => true;
            if (!string.IsNullOrEmpty(model.DocumentNumber))
                documentSearch = x => EF.Functions.ILike(x.Document.DocumentNumber, model.DocumentNumber.ToEndingPaternSearch());

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                      (a.DateTo ?? dateEnd).Date >= dateNow.Date && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            Expression<Func<Case, bool>> caseClassificationWhere = x => true;
            if (model.CaseClassificationId > 0)
                caseClassificationWhere = x => x.CaseClassifications
                             .Where(a => a.ClassificationId == model.CaseClassificationId &&
                                         a.DateTo == null &&
                                         a.CaseSessionId == null).Any();
            Expression<Func<Case, bool>> caseDepartmentWhere = x => true;
            if (model.CourtDepartmentId > 0)
            {
                caseDepartmentWhere = x => x.JudicalCompositionId == model.CourtDepartmentId;
            }
            Expression<Func<Case, bool>> caseDepartmentOtdelenieWhere = x => true;
            if (model.CourtDepartmentOtdelenieId > 0)
            {
                caseDepartmentOtdelenieWhere = x => x.OtdelenieId == model.CourtDepartmentOtdelenieId;
            }

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

            return repo.AllReadonly<Case>()
                .Include(x => x.Court)
                .Include(x => x.CaseGroup)
                .Include(x => x.CaseType)
                .Include(x => x.CaseCode)
                .Include(x => x.ProcessPriority)
                .Include(x => x.CaseState)
                .Include(x => x.CaseReason)
                .Where(x => x.CourtId == courtId &&
                            !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null) &&
                            ((model.CaseStateId > 0) ? (x.CaseStateId == model.CaseStateId) : true) &&
                            ((model.CaseCodeId > 0) ? (x.CaseCodeId == model.CaseCodeId) : true) &&
                            EF.Functions.ILike(x.RegNumber, model.RegNumber.ToCasePaternSearch())
                            )
                .Where(x => x.CaseStateId != NomenclatureConstants.CaseState.Draft)
                .Where(dateSearch)
                .Where(yearSearch)
                .Where(documentSearch)
                .Where(judgeReporterSearch)
                .Where(caseClassificationWhere)
                .Where(caseDepartmentWhere)
                .Where(caseDepartmentOtdelenieWhere)
                .Where(caseGroupWhere)
                .Where(caseTypeWhere)
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
                    DepartmentOtdelenieText = (x.Otdelenie != null ? x.Otdelenie.Label : string.Empty) + (x.JudicalComposition != null ? (x.Otdelenie != null ? " / " + x.JudicalComposition.Label : x.JudicalComposition.Label) : string.Empty)
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
                yearWhere = x => x.RegDate.Year == model.CaseYear;

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
                       .Include(x => x.Court)
                       .Include(x => x.CaseGroup)
                       .Include(x => x.CaseType)
                       .Include(x => x.CaseCode)
                       .Include(x => x.ProcessPriority)
                       .Include(x => x.CaseState)
                       .Include(x => x.CaseReason)
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
                           ShortNumber = x.ShortNumber,
                           RegNumber = x.RegNumber,
                           RegDate = x.RegDate,
                           CaseReasonLabel = (x.CaseReason != null) ? x.CaseReason.Label : string.Empty,
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
        private CaseLifecycle FillCaseLifecycle(int caseId, int lifecycleTypeId, DateTime dateTime)
        {
            return new CaseLifecycle()
            {
                CaseId = caseId,
                CourtId = userContext.CourtId,
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
        /// Запис на дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Case_SaveData(CaseEditVM model)
        {
            //Тука е само редакция защото делото е записано
            try
            {
                model.ProcessPriorityId = model.ProcessPriorityId.NumberEmptyToNull();
                model.LoadGroupLinkId = model.LoadGroupLinkId.EmptyToNull();
                model.ComplexIndexActual = model.ComplexIndexActual.NumberEmptyToNull();
                model.ComplexIndexLegal = model.ComplexIndexLegal.NumberEmptyToNull();
                model.CaseReasonId = model.CaseReasonId.NumberEmptyToNull();
                var caseCurrent = repo.GetById<Case>(model.Id);
                using (var ts = TransactionScopeBuilder.CreateReadCommitted())
                {

                    caseCurrent.CaseCharacterId = model.CaseCharacterId;
                    caseCurrent.CaseTypeId = model.CaseTypeId;
                    caseCurrent.CaseCodeId = model.CaseCodeId;
                    if (model.CaseCodeId == 0)
                    {
                        caseCurrent.CaseCodeId = null;
                    }
                    caseCurrent.CaseGroupId = repo.GetById<Infrastructure.Data.Models.Nomenclatures.CaseType>(model.CaseTypeId).CaseGroupId;
                    caseCurrent.CourtGroupId = model.CourtGroupId;
                    caseCurrent.CaseReasonId = model.CaseReasonId.EmptyToNull();
                    caseCurrent.CaseTypeUnitId = model.CaseTypeUnitId;
                    caseCurrent.Description = model.Description;
                    caseCurrent.CaseStateDescription = model.CaseStateDescription;
                    caseCurrent.CaseInforcedDate = model.CaseInforcedDate;
                    caseCurrent.ProcessPriorityId = model.ProcessPriorityId;
                    caseCurrent.IsNewCaseNewNumber = model.IsNewCaseNewNumber;
                    caseCurrent.ComplexIndexActual = model.ComplexIndexActual;
                    caseCurrent.ComplexIndexLegal = model.ComplexIndexLegal;
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
                        var caseCodeLoadIndex = repo.AllReadonly<Infrastructure.Data.Models.Nomenclatures.CaseCode>().Where(x => x.Id == model.CaseCodeId)
                                                       .Select(x => x.LoadIndex).DefaultIfEmpty(0).FirstOrDefault();
                        caseCurrent.LoadGroupLinkId = null;
                        caseCurrent.ComplexIndex = model.ComplexIndex;
                        caseCurrent.LoadIndex = (caseCodeLoadIndex + caseCurrent.ComplexIndex) / 2;
                    }
                    else
                    {
                        caseCurrent.LoadGroupLinkId = model.LoadGroupLinkId;
                        caseCurrent.LoadIndex = repo.AllReadonly<LoadGroupLink>().Where(x => x.Id == caseCurrent.LoadGroupLinkId)
                                                    .Select(x => x.LoadIndex).DefaultIfEmpty(0).FirstOrDefault();
                    }

                    caseCurrent.DateWrt = DateTime.Now;
                    caseCurrent.UserId = userContext.UserId;
                    bool isNewCase = false;

                    if (string.IsNullOrEmpty(caseCurrent.RegNumber) && caseCurrent.CaseStateId == NomenclatureConstants.CaseState.Draft && model.CaseStateId == NomenclatureConstants.CaseState.New)
                    {
                        caseCurrent.CaseStateId = model.CaseStateId;
                        caseCurrent.IsOldNumber = model.IsOldNumber;
                        //запис на лицата и адреси от Ducument
                        var casePersonList = repo.AllReadonly<DocumentPerson>().Include(x => x.Addresses).ThenInclude(x => x.Address)
                                                 .Where(x => x.DocumentId == caseCurrent.DocumentId).ToList();

                        int rownumber = 1;
                        foreach (var item in casePersonList)
                        {
                            var casePersonSave = new CasePerson()
                            {
                                CaseId = caseCurrent.Id,
                                CourtId = userContext.CourtId,
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

                                CreateHistory<CasePersonAddress, CasePersonAddressH>(addressSave);
                                casePersonSave.Addresses.Add(addressSave);
                            }

                            CreateHistory<CasePerson, CasePersonH>(casePersonSave);
                            caseCurrent.CasePersons.Add(casePersonSave);
                        }
                        int? oldNumber = null;
                        if (caseCurrent.IsOldNumber == true && !string.IsNullOrEmpty(model.OldNumber))
                        {
                            oldNumber = int.Parse(model.OldNumber);
                        }
                        if (counterService.Counter_GetCaseCounter(caseCurrent, oldNumber, model.OldDate) == false)
                        {
                            return false;
                        }
                        foreach (var person in caseCurrent.CasePersons)
                        {
                            person.DateFrom = caseCurrent.RegDate.AddSeconds(1);
                        }
                        isNewCase = true;

                        var caseLifecycle = FillCaseLifecycle(caseCurrent.Id, NomenclatureConstants.LifecycleType.InProgress, caseCurrent.RegDate);
                        repo.Add(caseLifecycle);

                        //Ако делото е несъстоятелност
                        bool isInsolvency = repo.AllReadonly<DocumentTypeGrouping>()
                            .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.Insolvency)
                            .Where(x => x.DocumentTypeId == model.DocumentTypeId)
                            .Any();
                        caseCurrent.IsISPNcase = isInsolvency;


                        Case_AddInPriorMigration(caseCurrent);
                        Case_SaveData_FinishTask(caseCurrent.DocumentId);

                    }
                    else
                    {
                        caseCurrent.CaseStateId = model.CaseStateId;
                    }
                    caseCurrent.UserId = userContext.UserId;
                    caseCurrent.DateWrt = DateTime.Now;
                    CreateHistory<Case, CaseH>(caseCurrent);
                    Case_GenerateCaseCount(model);
                    repo.Update(caseCurrent);

                    caseDeadlineService.DeadLineOnCase(caseCurrent);
                    repo.SaveChanges();

                    if (NomenclatureConstants.CaseState.UnregisteredManageble.Contains(model.CaseStateId))
                    {
                        Case_AddInPriorMigration(caseCurrent);
                        Case_SaveData_FinishTask(caseCurrent.DocumentId);
                    }

                    if (!string.IsNullOrEmpty(caseCurrent.RegNumber))
                    {
                        mqEpepService.AppendCase(caseCurrent, isNewCase ? EpepConstants.ServiceMethod.Add : EpepConstants.ServiceMethod.Update);
                    }
                    ts.Complete();
                }
                model.CaseStateId = caseCurrent.CaseStateId;
                model.RegNumber = caseCurrent.RegNumber;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Case Id={ model.Id }");
                return false;
            }
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
        private void Case_AddInPriorMigration(Case model)
        {
            CaseMigrationVM lastMigration;

            lastMigration = Case_GetPriorCase(model.DocumentId);
            if (lastMigration == null)
            {
                lastMigration = Case_GetPriorCaseEISPP(model.DocumentId, model.EISSPNumber);
            }
            if (lastMigration == null)
            {
                return;
            }

            var incommingMigrationTypeId = repo.AllReadonly<CaseMigrationType>()
                                            .Where(x => x.PriorMigrationTypeId == lastMigration.MigrationTypeId)
                                            .Where(x => x.IsActive)
                                            .Select(x => x.Id)
                                            .FirstOrDefault();
            if (incommingMigrationTypeId == 0)
            {
                return;
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
        }

        /// <summary>
        /// Запис на приключена задача
        /// </summary>
        /// <param name="documentId"></param>
        private void Case_SaveData_FinishTask(long documentId)
        {

            int[] docToCaseTasks = { WorkTaskConstants.Types.Case_SelectLawUnit, WorkTaskConstants.Types.Case_ForReject };
            var docTasks = repo.All<WorkTask>(
                x => x.SourceId == documentId
                && x.SourceType == SourceTypeSelectVM.Document
                && x.UserId == userContext.UserId
                && docToCaseTasks.Contains(x.TaskTypeId)
                && x.TaskStateId == WorkTaskConstants.States.Accepted);

            var docResolutions = repo.AllReadonly<DocumentResolution>()
                                        .Where(x => x.DocumentId == documentId)
                                        .Select(x => x.Id).ToArray();

            var resolutionTasks = repo.All<WorkTask>(
               x => docResolutions.Contains(x.SourceId)
               && x.SourceType == SourceTypeSelectVM.DocumentResolution
               && x.UserId == userContext.UserId
               && x.TaskTypeId == WorkTaskConstants.Types.Case_SelectLawUnit
               && x.TaskStateId == WorkTaskConstants.States.Accepted);

            foreach (var item in docTasks)
            {
                workTaskService.CompleteTask(item);
            }
            foreach (var item in resolutionTasks)
            {
                workTaskService.CompleteTask(item);
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
        /// Изчитане на данни за дело по ид
        /// </summary>
        /// <param name="id"></param>
        /// <param name="readOnly"></param>
        /// <returns></returns>
        private Case Case_GetById(long id, bool readOnly)
        {
            IQueryable<Case> cases = repo.All<Case>();
            if (readOnly)
            {
                cases = repo.AllReadonly<Case>();
            }
            var caseSelect = cases
                            .Include(x => x.CaseArchives)
                            .ThenInclude(x => x.CourtArchiveIndex)
                            .Include(x => x.Court)
                            .Include(x => x.CaseGroup)
                            .Include(x => x.CaseType)
                            .Include(x => x.CaseCode)
                            .Include(x => x.ProcessPriority)
                            .Include(x => x.CaseState)
                            .Include(x => x.LoadGroupLink)
                            .ThenInclude(x => x.LoadGroup)
                            .Include(x => x.Document)
                            .ThenInclude(x => x.DocumentType)
                            .Include(x => x.CaseReason)
                            .Where(x => x.Id == id)
                            .FirstOrDefault();
            return caseSelect;
        }

        /// <summary>
        /// Изчитане на данни за дело по ид
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CaseVM Case_GetById(long id)
        {
            var caseSelect = Case_GetById(id, true);
            if (caseSelect == null)
            {
                return null;
            }

            var caseOutMigrations = caseMigrationService.SelectOutMove(Convert.ToInt32(id));
            var lastCaseMigration = caseOutMigrations.OrderByDescending(x => x.Id).FirstOrDefault();

            var model = new CaseVM()
            {
                Id = caseSelect.Id,
                CourtId = caseSelect.CourtId,
                CourtLabel = caseSelect.Court.Label,
                CaseGroupId = caseSelect.CaseGroupId,
                CaseGroupLabel = (caseSelect.CaseGroup != null) ? caseSelect.CaseGroup.Label : string.Empty,
                CaseTypeId = caseSelect.CaseTypeId,
                CaseTypeLabel = (caseSelect.CaseType != null) ? caseSelect.CaseType.Label : string.Empty,
                CaseTypeCode = (caseSelect.CaseType != null) ? caseSelect.CaseType.Code : string.Empty,
                CaseCodeId = caseSelect.CaseCodeId,
                CaseCodeLabel = (caseSelect.CaseCode != null) ? caseSelect.CaseCode.Code + " " + caseSelect.CaseCode.Label : string.Empty,
                ProcessPriorityId = caseSelect.ProcessPriorityId,
                ProcessPriorityLabel = (caseSelect.ProcessPriority != null) ? caseSelect.ProcessPriority.Label : string.Empty,
                CaseStateId = caseSelect.CaseStateId,
                CaseStateLabel = (caseSelect.CaseState != null) ? caseSelect.CaseState.Label : string.Empty,
                EISSPNumber = caseSelect.EISSPNumber,
                ShortNumber = caseSelect.ShortNumber,
                RegNumber = caseSelect.RegNumber,
                RegDate = caseSelect.RegDate,
                LoadGroupLinkLabel = (caseSelect.LoadGroupLink != null) ? caseSelect.LoadGroupLink.LoadGroup.Label : string.Empty,
                LastMovment = caseMovementService.GetLastMovmentForCaseId(caseSelect.Id),
                DocumentId = caseSelect.DocumentId,
                DocumentName = caseSelect.Document.DocumentNumber + " / " + caseSelect.Document.DocumentDate.ToString("dd.MM.yyyy"),
                DocumentTypeId = caseSelect.Document.DocumentTypeId,
                DocumentTypeName = caseSelect.Document.DocumentType.Label,
                CaseInstanceId = (caseSelect.CaseType != null) ? caseSelect.CaseType.CaseInstanceId : 0,
                CaseReasonLabel = (caseSelect.CaseReason != null) ? caseSelect.CaseReason.Label : string.Empty,
                CaseStateDescription = caseSelect.CaseStateDescription,
                CaseInforcedDate = caseSelect.CaseInforcedDate,
                LastMigration = (lastCaseMigration != null) ? ((!lastCaseMigration.IsReturned ?? true) ? (lastCaseMigration.SentToName + " с Изх.№: " + lastCaseMigration.OutDocumentLabel) : string.Empty) : string.Empty
            };

            if (caseSelect.CaseArchives.Count() > 0)
            {
                var caseArchive = caseSelect.CaseArchives.FirstOrDefault();
                model.ArchRegNumber = caseArchive.RegNumber;
                model.ArchRegDate = caseArchive.RegDate;
                model.ArchiveIndexLabel = caseArchive.CourtArchiveIndex != null ? caseArchive.CourtArchiveIndex.Label : string.Empty;
                model.ArchiveLink = caseArchive.ArchiveLink;
                model.StorageYears = caseArchive.StorageYears;
                model.BookNumber = caseArchive.BookNumber;
                model.BookYear = caseArchive.BookYear;
            }

            return model;
        }

        /// <summary>
        /// Изчитане на данни в модел за редакция
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CaseEditVM Case_SelectForEdit(int id)
        {
            var result = repo.AllReadonly<Case>()
                             .Include(x => x.Court)
                             .ThenInclude(x => x.CourtType)
                             .Include(x => x.Document)
                             .ThenInclude(x => x.DocumentType)
                             .Include(x => x.CaseGroup)
                             .Include(x => x.CaseType)
                             .Where(x => x.Id == id)
                             .Select(x => new CaseEditVM()
                             {
                                 Id = x.Id,
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
                                 Description = x.Description,
                                 CaseStateDescription = x.CaseStateDescription,
                                 DocumentId = x.DocumentId,
                                 DocumentName = x.Document.DocumentNumber + " / " + x.Document.DocumentDate.ToString("dd.MM.yyyy"),
                                 DocumentTypeId = x.Document.DocumentTypeId,
                                 DocumentTypeName = x.Document.DocumentType.Label,
                                 RegNumber = x.RegNumber,
                                 IsOldNumber = (x.IsOldNumber ?? false),
                                 CaseReasonId = x.CaseReasonId,
                                 CaseInforcedDate = x.CaseInforcedDate,
                                 ProcessPriorityId = x.ProcessPriorityId,
                                 IsNewCaseNewNumber = x.IsNewCaseNewNumber,
                                 ComplexIndexActual = x.ComplexIndexActual,
                                 ComplexIndexLegal = x.ComplexIndexLegal
                             }).FirstOrDefault();

            if (result == null)
            {
                return null;
            }

            var caseLawUnitCount = repo.AllReadonly<CaseLawUnitCount>(x => x.CaseId == id)
                        .Select(x => new { x.JudgeRoleId, x.PersonCount })
                        .Where(x => NomenclatureConstants.JudgeRole.ReserveRolesList.Contains(x.JudgeRoleId))
                        .ToList();
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
        private CaseFolderItemVM GetCase_FolderItem(int id)
        {
            var _case = Case_GetById(id);
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

        private CaseFolderItemVM GetCaseInforceDate_FolderItem(int id)
        {
            var _case = Case_GetById(id);
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
        private List<CaseFolderItemVM> GetCaseInDocs_FolderItem(int caseId)
        {
            return repo.AllReadonly<DocumentCaseInfo>()
                       .Include(x => x.Document)
                       .ThenInclude(x => x.DocumentType)
                       .Where(x => x.CaseId == caseId && x.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument)
                       .Select(x => new CaseFolderItemVM
                       {
                           SourceType = SourceTypeSelectVM.CaseInDoc,
                           SourceId = x.Document.Id.ToString(),
                           Title = $"Вх.№ {x.Document.DocumentNumber}/{x.Document.DocumentDate:dd.MM.yyyy} {x.Document.DocumentType.Label}",
                           Date = x.Document.DocumentDate,
                           TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseInDoc),
                           ElementUrl = urlHelper.Action("View", "Document", new { id = x.Document.Id })
                       }).ToList();
        }

        /// <summary>
        /// Хронология: данни за престъпления
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private List<CaseFolderItemVM> GetCaseCrime_FolderItem(int caseId)
        {
            return repo.AllReadonly<CaseCrime>()
                       .Include(x => x.Case)
                       .Where(x => x.CaseId == caseId)
                       .Select(x => new CaseFolderItemVM
                       {
                           SourceType = SourceTypeSelectVM.CaseCrime,
                           SourceId = x.Id.ToString(),
                           Title = $"ЕИСПП № {x.EISSPNumber} {x.CrimeName}",
                           Date = x.Case.RegDate,
                           TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseCrime),
                           ElementUrl = urlHelper.Action("IndexCaseCrime", "CasePersonSentence", new { caseId = x.CaseId })
                       }).ToList();
        }

        /// <summary>
        /// Хронология: данни за хора към престъпление
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private List<CaseFolderItemVM> GetCasePersonCrime_FolderItem(int caseId)
        {
            return repo.AllReadonly<CasePersonCrime>()
                       .Include(x => x.CasePerson)
                       .Include(x => x.CaseCrime)
                       .Include(x => x.PersonRoleInCrime)
                       .Include(x => x.RecidiveType)
                       .Include(x => x.Case)
                       .Where(x => x.CaseId == caseId)
                       .Select(x => new CaseFolderItemVM
                       {
                           SourceType = SourceTypeSelectVM.CasePersonCrime,
                           SourceId = x.Id.ToString(),
                           Title = x.CaseCrime.CrimeName + " - " + x.CasePerson.FullName + " - " + x.PersonRoleInCrime.Label + " - " + x.RecidiveType.Label,
                           Date = x.Case.RegDate,
                           TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CasePersonCrime),
                           ElementUrl = urlHelper.Action("IndexCasePersonCrime", "CasePersonSentence", new { caseCrimeId = x.CaseCrimeId })
                       }).ToList();
        }

        /// <summary>
        /// Хронология: данни за бюлетин
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private List<CaseFolderItemVM> GetCasePersonSentenceBulletin_FolderItem(int caseId)
        {
            return repo.AllReadonly<CasePersonSentenceBulletin>()
                       .Include(x => x.OutDocument)
                       .Include(x => x.CasePerson)
                       .Where(x => x.CaseId == caseId)
                       .Select(x => new CaseFolderItemVM
                       {
                           SourceType = SourceTypeSelectVM.CasePersonBulletin,
                           SourceId = x.Id.ToString(),
                           Title = x.CasePerson.FullName +
                                   (!string.IsNullOrEmpty(x.BirthDayPlace) ? " местораждане: " + x.BirthDayPlace : string.Empty) +
                                   (" дата на раждане: " + x.BirthDay.ToString("dd.MM.yyyy")) +
                                   (!string.IsNullOrEmpty(x.Nationality) ? " гражданство: " + x.Nationality : string.Empty) +
                                   (!string.IsNullOrEmpty(x.FamilyMarriage) ? " фамилно име преди сключване на брак: " + x.FamilyMarriage : string.Empty),
                           Date = x.DateWrt,
                           TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CasePersonBulletin),
                           ElementUrl = urlHelper.Action("Index", "CasePersonSentence", new { casePersonId = x.CasePersonId })
                       }).ToList();
        }

        /// <summary>
        /// Хронология: данни за присъди
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private List<CaseFolderItemVM> GetCasePersonSentence_FolderItem(int caseId)
        {
            return repo.AllReadonly<CasePersonSentence>()
                       .Include(x => x.CasePerson)
                       .Include(x => x.DecreedCourt)
                       .Include(x => x.CaseSessionAct)
                       .Include(x => x.Case)
                       .Where(x => x.CaseId == caseId)
                       .Select(x => new CaseFolderItemVM
                       {
                           SourceType = SourceTypeSelectVM.CasePersonSentence,
                           SourceId = x.Id.ToString(),
                           Title = x.CasePerson.FullName + " - постановена от: " + x.DecreedCourt.Label + " - акт: " + x.CaseSessionAct.RegNumber + "/" + (x.CaseSessionAct.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") + "г.",
                           Date = x.CaseSessionAct.RegDate,
                           TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CasePersonSentence),
                           ElementUrl = urlHelper.Action("Index", "CasePersonSentence", new { casePersonId = x.CasePersonId })
                       }).ToList();
        }

        /// <summary>
        /// Хронология: данни за наказания към присъда
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private List<CaseFolderItemVM> GetCasePersonSentencePunishment_FolderItem(int caseId)
        {
            return repo.AllReadonly<CasePersonSentencePunishment>()
                       .Include(x => x.SentenceType)
                       .Where(x => x.CaseId == caseId)
                       .Select(x => new CaseFolderItemVM
                       {
                           SourceType = SourceTypeSelectVM.CasePersonSentencePunishment,
                           SourceId = x.Id.ToString(),
                           Title = $"Наложено наказание по НК: {x.SentenceType.Label} Сумарен ред за присъди: {(x.IsSummaryPunishment ? "Да" : "Не")}",
                           Date = x.DateFrom,
                           TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CasePersonSentencePunishment),
                           ElementUrl = urlHelper.Action("IndexCasePersonSentencePunishment", "CasePersonSentence", new { casePersonSentenceId = x.CasePersonSentenceId })
                       }).ToList();
        }

        /// <summary>
        /// Хронология: данни за престъпления към присъда
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private List<CaseFolderItemVM> GetCasePersonSentencePunishmentCrime_FolderItem(int caseId)
        {
            return repo.AllReadonly<CasePersonSentencePunishmentCrime>()
                       .Include(x => x.CasePersonSentencePunishment)
                       .Include(x => x.CaseCrime)
                       .Include(x => x.PersonRoleInCrime)
                       .Include(x => x.RecidiveType)
                       .Where(x => x.CaseId == caseId)
                       .Select(x => new CaseFolderItemVM
                       {
                           SourceType = SourceTypeSelectVM.CasePersonSentencePunishmentCrime,
                           SourceId = x.Id.ToString(),
                           Title = $"Участие в наложени наказания към присъда. Престъпление: {x.CaseCrime.CrimeName} роля: {x.PersonRoleInCrime.Label} рецидив: {x.RecidiveType.Label}",
                           Date = x.CasePersonSentencePunishment.DateFrom,
                           TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CasePersonSentencePunishmentCrime),
                           ElementUrl = urlHelper.Action("IndexCasePersonSentencePunishmentCrime", "CasePersonSentence", new { CasePersonSentencePunishmentId = x.CasePersonSentencePunishmentId })
                       }).ToList();
        }

        /// <summary>
        /// Хронология: данни за интервали
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private List<CaseFolderItemVM> GetCaseLifecycle_FolderItem(int caseId)
        {
            return repo.AllReadonly<CaseLifecycle>()
                       .Include(x => x.LifecycleType)
                       .Where(x => x.CaseId == caseId)
                       .Select(x => new CaseFolderItemVM
                       {
                           SourceType = SourceTypeSelectVM.CaseLifecycle,
                           SourceId = x.Id.ToString(),
                           Title = $"Интервал по дело: {x.LifecycleType.Label} повторение {x.Iteration} от {x.DateFrom:dd.MM.yyyy HH mm}",
                           Date = x.DateFrom,
                           TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseLifecycle),
                           ElementUrl = urlHelper.Action("Index", "CaseLifecycle", new { id = x.CaseId })
                       }).ToList();
        }

        /// <summary>
        /// Хронология: данни за разводи
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private List<CaseFolderItemVM> GetCaseSessionActDivorce_FolderItem(int caseId)
        {
            return repo.AllReadonly<CaseSessionActDivorce>()
                       .Where(x => x.CaseId == caseId)
                       .Select(x => new CaseFolderItemVM
                       {
                           SourceType = SourceTypeSelectVM.CaseSessionActDivorce,
                           SourceId = x.Id.ToString(),
                           Title = $"Съобщение за прекратяване на граждански брак {x.RegNumber} от {x.RegDate:dd.MM.yyyy HH mm}",
                           Date = x.RegDate,
                           TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseSessionActDivorce),
                           ElementUrl = urlHelper.Action("Index", "CaseLifecycle", new { id = x.CaseId })
                       }).ToList();
        }

        /// <summary>
        /// Хронология: данни за наследство
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private List<CaseFolderItemVM> GetCasePersonInheritance_FolderItem(int caseId)
        {
            return repo.AllReadonly<CasePersonInheritance>()
                       .Include(x => x.CasePerson)
                       .Include(x => x.Court)
                       .Include(x => x.CaseSessionAct)
                       .Include(x => x.CasePersonInheritanceResult)
                       .Where(x => x.CaseId == caseId && x.DateExpired == null)
                       .Select(x => new CaseFolderItemVM
                       {
                           SourceType = SourceTypeSelectVM.CasePersonInheritance,
                           SourceId = x.Id.ToString(),
                           Title = $"Наследство на {x.CasePerson.FullName} Постановена от {x.Court.Label} Акт: {x.CaseSessionAct.RegNumber} {x.CaseSessionAct.RegDate:dd.MM.yyyy HH mm} - {x.CasePersonInheritanceResult.Label}",
                           Date = x.CaseSessionAct.RegDate,
                           TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CasePersonInheritance),
                           ElementUrl = urlHelper.Action("Index", "CaseLifecycle", new { id = x.CaseId })
                       }).ToList();
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
                                   x.ExecListObligations.Select(o => o.Amount).DefaultIfEmpty(0).Sum(),
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
        private List<CaseFolderItemVM> GetCaseSession_FolderItem(int caseId)
        {
            var caseSessionVMs = caseSessionService.CaseSession_OldSelect(caseId, null, null).ToList();
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
                result.AddRange(GetNotification_FolderItem(caseId, item.Id, null));
                result.AddRange(GetMoney_FolderItem(caseId, item.Id));
                //result.AddRange(GetSessionResult_FolderItem(item.Id, sessionFolder.Title));
                result.AddRange(GetSessionMeeting_FolderItem(item.Id, sessionFolder.Title));
                result.AddRange(GetSessionDoc_FolderItem(item.Id, sessionFolder.Title));
                result.AddRange(GetSessionAct_FolderItem(item.Id, sessionFolder.Title));
                result.AddRange(GetCaseSessionFastDocument_FolderItem(item.Id, sessionFolder.Title, sessionFolder.Date ?? DateTime.Now));
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за движение
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private List<CaseFolderItemVM> GetCaseMovement_FolderItem(int caseId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseMovements = caseMovementService.Select(caseId);

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

            return result;
        }

        /// <summary>
        /// Хронология: данни за плащания
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private List<CaseFolderItemVM> GetPaymentCase_FolderItem(int caseId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var paymentCases = moneyService.PaymentForCase_Select(caseId);

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
        private List<CaseFolderItemVM> GetExecListCase_FolderItem(int caseId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var execLists = moneyService.ExecListForCase_Select(caseId);

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
        private List<CaseFolderItemVM> GetDocumentTemplate_FolderItem(int caseId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            //var documentTemplates = documentTemplateService.DocumentTemplate_Select(SourceTypeSelectVM.Case, caseId);

            var caseOutDocs = repo.AllReadonly<DocumentCaseInfo>()
                                            .Include(x => x.Document)
                                            .ThenInclude(x => x.DocumentType)
                                            .Where(x => x.CaseId == caseId && x.Document.DocumentDirectionId == DocumentConstants.DocumentDirection.OutGoing)
                                            .Select(x => new DocumentInfoVM
                                            {
                                                Id = x.Document.Id,
                                                Title = $"Изх.№ {x.Document.DocumentNumber}/{x.Document.DocumentDate:dd.MM.yyyy} {x.Document.DocumentType.Label}",
                                                DirectionId = x.Document.DocumentDirectionId,
                                                IsSecret = (x.Document.IsSecret ?? false),
                                                IsRestriction = x.Document.IsRestictedAccess,
                                                DocumentDate = x.Document.DocumentDate
                                            }).ToList();

            var caseOutDocsFromTemplate = repo.AllReadonly<DocumentTemplate>()
                                                .Include(x => x.Document)
                                                .ThenInclude(x => x.DocumentType)
                                                .Where(x => x.CaseId == caseId && x.DocumentId > 0)
                                                .Select(x => new DocumentInfoVM
                                                {
                                                    Id = x.Document.Id,
                                                    Title = $"Изх.№ {x.Document.DocumentNumber}/{x.Document.DocumentDate:dd.MM.yyyy} {x.Document.DocumentType.Label}",
                                                    DirectionId = x.Document.DocumentDirectionId,
                                                    IsSecret = (x.Document.IsSecret ?? false),
                                                    IsRestriction = x.Document.IsRestictedAccess,
                                                    DocumentDate = x.Document.DocumentDate
                                                }).ToList();

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
        private List<CaseFolderItemVM> GetLifecycle_FolderItem(int caseId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseLifecycles = caseLifecycleService.CaseLifecycle_Select(caseId);

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
        private List<CaseFolderItemVM> GetSelectionProtokol_FolderItem(int caseId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseSelectionProtokols = caseSelectionProtokolService.CaseSelectionProtokol_Select(caseId);

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
        private List<CaseFolderItemVM> GetLawUnit_FolderItem(int caseId, int? caseSessionId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseLawUnits = caseLawUnitService.CaseLawUnit_Select(caseId, caseSessionId).ToList();

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
        private List<CaseFolderItemVM> GetPerson_FolderItem(int caseId, int? caseSessionId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var casePersonLists = casePersonService.CasePerson_Select(caseId, caseSessionId, false, false, false).ToList();

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
        private List<CaseFolderItemVM> GetLawUnitDismisal_FolderItem(int caseId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseLawUnitDismisals = caseLawUnitService.CaseLawUnitDismisal_Select(caseId);

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
        private List<CaseFolderItemVM> GetEvidence_FolderItem(int caseId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseEvidences = caseEvidenceService.CaseEvidence_Select(caseId, null, null, string.Empty, string.Empty, 0);

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
                result.AddRange(GetEvidenceMovment_FolderItem(item.Id));
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за движение на доказателство
        /// </summary>
        /// <param name="evidenceId"></param>
        /// <returns></returns>
        private List<CaseFolderItemVM> GetEvidenceMovment_FolderItem(int evidenceId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseEvidenceMovements = caseEvidenceService.CaseEvidenceMovement_Select(evidenceId);

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
        private List<CaseFolderItemVM> GetNotification_FolderItem(int caseId, int? caseSessionId, int? caseSessionActId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseNotifications = caseNotificationService.CaseNotification_Select(caseId, caseSessionId, caseSessionActId).ToList();

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
        private List<CaseFolderItemVM> GetClassification_FolderItem(int caseId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseClassifications = caseClassificationService.CaseClassification_SelectObject(caseId, null).ToList();

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
        private List<CaseFolderItemVM> GetMoney_FolderItem(int caseId, int? caseSessionId)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseMoney = caseMoneyService.CaseMoney_Select(caseId, caseSessionId).ToList();

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
        private List<CaseFolderItemVM> GetSessionMeeting_FolderItem(int caseSessionId, string sessionName)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseSessionMeetings = caseSessionMeetingService.CaseSessionMeeting_Select(caseSessionId).ToList();

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
                result.AddRange(GetSessionMeetingUser_FolderItem(item.Id, documentFolder.Title));
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
        private List<CaseFolderItemVM> GetCaseSessionFastDocument_FolderItem(int caseSessionId, string sessionName, DateTime dateSession)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseSessionFastDocuments = caseSessionFastDocumentService.CaseSessionFastDocument_Select(caseSessionId).ToList();

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
                result.AddRange(GetSessionMeetingUser_FolderItem(item.Id, documentFolder.Title));
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за секретар към сесия
        /// </summary>
        /// <param name="caseSessionMeetingId"></param>
        /// <param name="meetingName"></param>
        /// <returns></returns>
        private List<CaseFolderItemVM> GetSessionMeetingUser_FolderItem(int caseSessionMeetingId, string meetingName)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseSessionMeetings = caseSessionMeetingService.CaseSessionMeetingUser_Select(caseSessionMeetingId).ToList();

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
        private List<CaseFolderItemVM> GetSessionDoc_FolderItem(int caseSessionId, string sessionName)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseSessionDocs = caseSessionDocService.CaseSessionDoc_Select(caseSessionId).ToList();

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
        private List<CaseFolderItemVM> GetSessionAct_FolderItem(int caseSessionId, string sessionName)
        {
            List<CaseFolderItemVM> result = new List<CaseFolderItemVM>();
            var caseSessionActs = caseSessionActService.CaseSessionAct_Select(caseSessionId, null, null, null, null, null).ToList();

            foreach (var item in caseSessionActs.Where(x => x.ActDeclaredDate != null))
            {
                var documentFolder = new CaseFolderItemVM()
                {
                    SourceType = SourceTypeSelectVM.CaseSessionAct,
                    SourceId = item.Id.ToString(),
                    Title = $"{sessionName} {item.ActTypeLabel} {item.ActStateLabel} номер: {item.RegNumber}",
                    Date = item.RegDate ?? item.DateWrt,
                    TypeName = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseSessionAct),
                    ElementUrl = urlHelper.Action("Preview", "CaseSession", new { id = caseSessionId }) + "#tabSessionAct"
                };

                result.Add(documentFolder);
            }

            return result;
        }

        /// <summary>
        /// Хронология: данни за всичко в едно дело за хронология в ел. папка
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<CaseFolderItemVM> Case_SelectFolder(int id)
        {
            var result = new List<CaseFolderItemVM>
            {
                GetCase_FolderItem(id)
            };

            var caseFolder = GetCaseInforceDate_FolderItem(id);
            if (caseFolder != null)
                result.Add(caseFolder);

            result.AddRange(GetCaseSession_FolderItem(id));
            result.AddRange(GetCaseInDocs_FolderItem(id));
            result.AddRange(GetCaseCrime_FolderItem(id));
            result.AddRange(GetCasePersonCrime_FolderItem(id));
            result.AddRange(GetCasePersonSentence_FolderItem(id));
            result.AddRange(GetCasePersonSentencePunishment_FolderItem(id));
            result.AddRange(GetCasePersonSentencePunishmentCrime_FolderItem(id));
            result.AddRange(GetCasePersonSentenceBulletin_FolderItem(id));
            result.AddRange(GetCaseLifecycle_FolderItem(id));
            result.AddRange(GetCaseSessionActDivorce_FolderItem(id));
            result.AddRange(GetCasePersonInheritance_FolderItem(id));
            //result.AddRange(GetExecList_FolderItem(id));
            result.AddRange(GetCaseMovement_FolderItem(id));
            result.AddRange(GetDocumentTemplate_FolderItem(id));
            result.AddRange(GetLifecycle_FolderItem(id));
            result.AddRange(GetSelectionProtokol_FolderItem(id));
            result.AddRange(GetLawUnit_FolderItem(id, null));
            result.AddRange(GetPerson_FolderItem(id, 0));
            result.AddRange(GetLawUnitDismisal_FolderItem(id));
            result.AddRange(GetEvidence_FolderItem(id));
            result.AddRange(GetNotification_FolderItem(id, null, null));
            result.AddRange(GetClassification_FolderItem(id));
            result.AddRange(GetMoney_FolderItem(id, null));
            result.AddRange(GetPaymentCase_FolderItem(id));
            result.AddRange(GetExecListCase_FolderItem(id));

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
                            .Include(x => x.CaseGroup)
                            .Where(x => x.RegNumber != null)
                            .Where(filter)
                            .Select(x => new LabelValueVM()
                            {
                                Value = x.Id.ToString(),
                                Label = $"{x.RegNumber} ({x.CaseGroup.Code})"
                            });
        }

        /// <summary>
        /// Данни за актове по дело за комбо
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="declaredOnly"></param>
        /// <returns></returns>
        public IEnumerable<SelectListItem> GetDDL_SessionActsByCase(int caseId, bool addDefaultElement = false, bool declaredOnly = true)
        {
            Expression<Func<CaseSessionAct, bool>> filter = x => x.ActDeclaredDate != null;
            if (!declaredOnly)
            {
                //Поне да е регистриран, ако не постановен
                filter = x => x.RegDate != null;
            }
            var result = repo.AllReadonly<CaseSessionAct>()
                             .Include(x => x.CaseSession)
                             .ThenInclude(x => x.SessionType)
                             .Include(x => x.ActType)
                             .Where(x => x.CaseSession.CaseId == caseId)
                             .Where(filter)
                             .Where(FilterExpireInfo<CaseSessionAct>(false))
                             .OrderByDescending(x => x.RegDate)
                             .Select(x => new SelectListItem()
                             {
                                 Value = x.Id.ToString(),
                                 Text = $"{x.ActType.Label} {x.RegNumber}/{x.RegDate:dd.MM.yyyy} ({x.CaseSession.SessionType.Label} {x.CaseSession.DateFrom:dd.MM.yyyy})"
                             }).ToList();
            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }
            return result;
        }

        public CaseProceedingsVM CaseProceedings_Select(int CaseId)
        {
            CaseElectronicFolderVM caseElectronicFolderVM = CaseElectronicFolder_Select(CaseId);

            var caseOutMigrations = caseMigrationService.SelectOutMove(CaseId);
            var lastCaseMigration = caseOutMigrations.OrderByDescending(x => x.Id).FirstOrDefault();

            List<CaseProceedingsObjectsVM> objectsVMs = new List<CaseProceedingsObjectsVM>();
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromDocumentResolution(caseElectronicFolderVM.DocumentResolutions.ToList()));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromDocumentResolution(caseElectronicFolderVM.DocumentResolutionCase.ToList()));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromSessions(caseElectronicFolderVM));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromCaseInDocuments(caseElectronicFolderVM));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromOutMigration(caseOutMigrations.ToList()));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromCaseSessionDocs(caseElectronicFolderVM));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromSessionFastDocuments(caseElectronicFolderVM));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromCaseOutDocuments(caseElectronicFolderVM));
            objectsVMs.AddRange(FillCaseProceedingsObjectsFromWorkTask(caseElectronicFolderVM));

            CaseProceedingsVM result = new CaseProceedingsVM()
            {
                Id = caseElectronicFolderVM.Id,
                RegNumber = caseElectronicFolderVM.RegNumber,
                RegDate = caseElectronicFolderVM.RegDate,
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
                        Description = act.Description
                    };
                    result.Add(actElement);
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
                                              (!string.IsNullOrEmpty(doc.Description) ? " - " + doc.Description : string.Empty) +
                                              (!string.IsNullOrEmpty(doc.DocumentTemplateLabel) ? " Бланка: " + doc.DocumentTemplateLabel : string.Empty) +
                                              (!string.IsNullOrEmpty(doc.DocumentPersonLabel) ? " Вносител: " + doc.DocumentPersonLabel : string.Empty)
                };
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
                                                 (!string.IsNullOrEmpty(documentInfo.DocumentPersonLabel) ? " Адресат: " + documentInfo.DocumentPersonLabel : string.Empty)
                };
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
                    Label = "Движение с изходящ документ: " + caseMigration.OutDocumentLabel + " изпращане до: " + caseMigration.SentToName
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
        /// Извлича данни за дело за ел. папка
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public CaseElectronicFolderVM CaseElectronicFolder_Select(int CaseId)
        {
            var caseSelectionProtokolListVMs = caseSelectionProtokolService.CaseSelectionProtokol_Select(CaseId).ToList();
            var caseSessionDocVMs = caseSessionDocService.CaseSessionDocByCaseId_Select(CaseId).ToList();
            //var casePersonLists = casePersonService.CasePerson_Select(CaseId, null, false, false, false).ToList();
            var casePersonLists = repo.AllReadonly<CasePerson>()
                                      .Where(x => x.CaseId == CaseId &&
                                                  x.DateExpired == null)
                                      .Select(x => new CasePersonListVM()
                                      {
                                          Id = x.Id,
                                          CaseId = x.CaseId,
                                          CaseSessionId = x.CaseSessionId,
                                          Uic = x.Uic,
                                          UicTypeLabel = (x.UicType != null) ? x.UicType.Label : string.Empty,
                                          FullName = x.FullName,
                                          RoleName = x.PersonRole.Label,
                                          PersonRoleId = x.PersonRole.Id,
                                          PersonRoleLabel = x.PersonRole.Label,
                                          RoleKindId = x.PersonRole.RoleKindId,
                                          DateFrom = x.DateFrom,
                                          DateTo = x.DateTo,
                                      })
                                      .ToList();
            var caseSessionNotificationLists = caseNotificationService.CaseSessionNotificationList_SelectByCaseId(CaseId).ToList();
            var caseSessionActs = caseSessionActService.CaseSessionAct_Select(0, CaseId, null, null, null, null).ToList();
            var caseLawUnitsAll = caseLawUnitService.CaseLawUnit_Select(CaseId, null, true).ToList();
            var caseLawUnitsActive = caseLawUnitService.CaseLawUnit_Select(CaseId, null).ToList();
            var caseSessionLawUnits = caseLawUnitService.CaseLawUnitByCaseFromSession_Select(CaseId).ToList();
            var caseSessionResults = caseSessionService.CaseSessionResultStringList_SelectByCaseId(CaseId).ToList();
            var caseSessionMeetings = caseSessionMeetingService.CaseSessionMeeting_SelectByCaseId(CaseId).ToList();
            var caseMigrations = caseMigrationService.Select(CaseId).ToList();
            var regixListVMs = regixReportService.RegixListByCase_Select(CaseId).Where(x => x.RegixRequestTypeId == NomenclatureConstants.RegixRequestTypes.FromReport).ToList();

            List<CaseMigrationVM> caseMigrationResult = new List<CaseMigrationVM>();
            foreach (var migration in caseMigrations.Where(x => x.CaseId != CaseId))
            {
                if (!caseMigrationResult.Any(x => x.CaseId == migration.CaseId))
                {
                    caseMigrationResult.Add(migration);
                }
            }

            var caseOutDocs = repo.AllReadonly<DocumentCaseInfo>()
                                                .Include(x => x.Document)
                                                .ThenInclude(x => x.DocumentType)
                                                .Where(x => x.CaseId == CaseId &&
                                                            x.Document.DocumentDirectionId == DocumentConstants.DocumentDirection.OutGoing &&
                                                            x.Document.DateExpired == null)
                                                .Select(x => new DocumentInfoVM
                                                {
                                                    Id = x.Document.Id,
                                                    Title = $"Изх.№ {x.Document.DocumentNumber}/{x.Document.DocumentDate:dd.MM.yyyy} {x.Document.DocumentType.Label}",
                                                    DirectionId = x.Document.DocumentDirectionId,
                                                    IsSecret = (x.Document.IsSecret ?? false),
                                                    IsRestriction = x.Document.IsRestictedAccess,
                                                    DocumentDate = x.Document.DocumentDate,
                                                    DocumentResolutions = x.Document.DocumentResolutions.Where(c => c.DateExpired == null && c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced).Select(c => new DocumentResolutionListVM() { Id = c.Id, ResolutionTypeLabel = c.ResolutionType.Label, Label = c.ResolutionType.Label + " " + c.RegNumber + "/" + (c.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"), RegDate = c.RegDate, RegNumber = c.RegNumber }).ToList() ?? new List<DocumentResolutionListVM>(),
                                                    DocumentTemplateLabel = (x.Document.DocumentTemplates.Any(d => d.DateExpired == null) ? (x.Document.DocumentTemplates.FirstOrDefault()).HtmlTemplate.Label : string.Empty),
                                                    DocumentPersonLabel = string.Join(", ", x.Document.DocumentPersons.Select(p => p.FullName))
                                                }).ToList();

            var caseInDocs = repo.AllReadonly<DocumentCaseInfo>()
                                                .Include(x => x.Document)
                                                .ThenInclude(x => x.DocumentType)
                                                .Include(x => x.Document)
                                                .ThenInclude(x => x.DocumentResolutions)
                                                .ThenInclude(x => x.ResolutionType)
                                                .Where(x => x.CaseId == CaseId &&
                                                            x.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument &&
                                                            x.Document.DateExpired == null)
                                                .Select(x => new DocumentInfoVM
                                                {
                                                    Id = x.Document.Id,
                                                    Title = $"Вх.№ {x.Document.DocumentNumber}/{x.Document.DocumentDate:dd.MM.yyyy} {x.Document.DocumentType.Label}",
                                                    DirectionId = x.Document.DocumentDirectionId,
                                                    IsSecret = (x.Document.IsSecret ?? false),
                                                    IsRestriction = x.Document.IsRestictedAccess,
                                                    DocumentResolutions = x.Document.DocumentResolutions.Where(c => c.DateExpired == null && c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced).Select(c => new DocumentResolutionListVM() { Id = c.Id, ResolutionTypeLabel = c.ResolutionType.Label, Label = c.ResolutionType.Label + " " + c.RegNumber + "/" + (c.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"), RegDate = c.RegDate, RegNumber = c.RegNumber }).ToList() ?? new List<DocumentResolutionListVM>(),
                                                    DocumentDate = x.Document.DocumentDate,
                                                    DocumentTemplateLabel = (x.Document.DocumentTemplates.Any(d => d.DateExpired == null) ? (x.Document.DocumentTemplates.FirstOrDefault()).HtmlTemplate.Label : string.Empty),
                                                    DocumentPersonLabel = string.Join(", ", x.Document.DocumentPersons.Select(p => p.FullName))
                                                }).ToList();

            foreach (var documentInfo in caseInDocs)
            {
                documentInfo.Description = string.Join(", ", repo.AllReadonly<WorkTask>()
                                                                 .Where(x => x.SourceType == SourceTypeSelectVM.Document &&
                                                                             x.SourceId == documentInfo.Id &&
                                                                             x.DescriptionCreated != null)
                                                                 .Select(x => x.DescriptionCreated));
            }

            //var caseOutDocsFromTemplate = repo.AllReadonly<DocumentTemplate>()
            //                                    .Include(x => x.Document)
            //                                    .ThenInclude(x => x.DocumentType)
            //                                    .Where(x => x.CaseId == CaseId &&
            //                                                x.DocumentId > 0 &&
            //                                                x.Document.DateExpired == null)
            //                                    .Select(x => new DocumentInfoVM
            //                                    {
            //                                        Id = x.Document.Id,
            //                                        Title = $"Изх.№ {x.Document.DocumentNumber}/{x.Document.DocumentDate:dd.MM.yyyy} {x.Document.DocumentType.Label}",
            //                                        DirectionId = x.Document.DocumentDirectionId,
            //                                        IsSecret = (x.Document.IsSecret ?? false),
            //                                        IsRestriction = x.Document.IsRestictedAccess,
            //                                        DocumentDate = x.Document.DocumentDate,
            //                                        DocumentTemplateLabel = (x.Document.DocumentTemplates.Any(d => d.DateExpired == null) ? (x.Document.DocumentTemplates.FirstOrDefault()).HtmlTemplate.Label : string.Empty),
            //                                        DocumentPersonLabel = string.Join(", ", x.Document.DocumentPersons.Select(p => p.FullName))
            //                                    }).ToList() ?? new List<DocumentInfoVM>();

            //if (caseOutDocs != null)
            //{
            //    foreach (var documentInfoVM in caseOutDocs)
            //    {
            //        if (!caseOutDocsFromTemplate.Any(x => x.Id == documentInfoVM.Id))
            //        {
            //            caseOutDocsFromTemplate.Add(documentInfoVM);
            //        }
            //    }
            //    //caseOutDocsFromTemplate.AddRange(caseOutDocs);
            //}

            var caseElectronicFolderVM = repo.AllReadonly<Case>()
                                             .Include(x => x.CaseArchives)
                                             .Include(x => x.CaseSessions)
                                             .ThenInclude(x => x.SessionType)
                                             .Include(x => x.CaseSessions)
                                             .ThenInclude(x => x.CourtHall)
                                             .Include(x => x.CaseSessions)
                                             .ThenInclude(x => x.SessionState)
                                             .Include(x => x.Court)
                                             .Include(x => x.CaseGroup)
                                             .Include(x => x.CaseType)
                                             .Include(x => x.CaseCode)
                                             .Include(x => x.ProcessPriority)
                                             .Include(x => x.CaseState)
                                             .Include(x => x.Document)
                                             .ThenInclude(x => x.DeliveryType)
                                             .Include(x => x.Document)
                                             .ThenInclude(x => x.DocumentResolutions)
                                             .ThenInclude(x => x.ResolutionType)
                                             .Include(x => x.Document)
                                             .ThenInclude(x => x.DocumentCaseInfo)
                                             .Where(x => x.Id == CaseId)
                                             .Select(x => new CaseElectronicFolderVM()
                                             {
                                                 Id = x.Id,
                                                 CourtId = x.CourtId,
                                                 IsOnlyFiles = x.CourtId != userContext.CourtId,
                                                 CourtLabel = x.Court.Label,
                                                 CaseGroupLabel = (x.CaseGroup != null) ? x.CaseGroup.Label : string.Empty,
                                                 CaseTypeLabel = (x.CaseType != null) ? x.CaseType.Code : string.Empty,
                                                 CaseCodeLabel = (x.CaseCode != null) ? x.CaseCode.Code + " " + x.CaseCode.Label : string.Empty,
                                                 CaseStateLabel = (x.CaseState != null) ? x.CaseState.Label : string.Empty,
                                                 RegNumber = x.RegNumber,
                                                 RegDate = x.RegDate,
                                                 DocumentId = x.DocumentId,
                                                 DocumentLabel = "Вх.№ " + x.Document.DocumentNumber + "/" + x.Document.DocumentDate.ToString("dd.MM.yyyy") + " " + x.Document.DocumentType.Label,
                                                 DocumentResolutions = x.Document.DocumentResolutions.Where(c => c.DateExpired == null && c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced).Select(c => new DocumentResolutionListVM() { Id = c.Id, ResolutionTypeId = c.ResolutionTypeId, ResolutionTypeLabel = c.ResolutionType.Label, Label = c.ResolutionType.Label + " " + c.RegNumber + "/" + (c.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"), RegDate = c.RegDate, RegNumber = c.RegNumber }).ToList(),
                                                 DocumentSecret = (x.Document.IsSecret ?? false),
                                                 DocumentRestriction = x.IsRestictedAccess,
                                                 CaseSelectionProtokols = caseSelectionProtokolListVMs,
                                                 CaseInDocuments = caseInDocs,
                                                 RegixReports = regixListVMs,
                                                 CaseOutDocuments = caseOutDocs,
                                                 CaseReasonLabel = (x.CaseReason != null) ? x.CaseReason.Label : string.Empty,
                                                 CaseStateDescription = x.CaseStateDescription,
                                                 CasePersons = casePersonLists.Where(person => person.CaseSessionId == null).ToList(),
                                                 CaseSessionFinalActs = caseSessionActs.Where(a => a.ActDeclaredDate != null).ToList(),
                                                 DocumentCaseInfos = x.Document.DocumentCaseInfo.Where(f => (f.IsLegacyCase ?? false)).ToList(),
                                                 ArchRegNumber = (x.CaseArchives != null) ? x.CaseArchives.FirstOrDefault().RegNumber : string.Empty,
                                                 ArchRegDate = (x.CaseArchives != null) ? x.CaseArchives.FirstOrDefault().RegDate : DateTime.Now,
                                                 CaseInforcedDate = x.CaseInforcedDate,
                                                 CaseSessions = x.CaseSessions.Where(p => p.DateExpired == null).Select(p => new CaseSessionElectronicFolderVM()
                                                 {
                                                     Id = p.Id,
                                                     CourtId = p.CourtId ?? 0,
                                                     SessionTypeLabel = (p.SessionType != null) ? p.SessionType.Label : string.Empty,
                                                     CourtHallName = (p.CourtHall != null) ? p.CourtHall.Name : string.Empty,
                                                     SessionStateLabel = p.SessionState.Label,
                                                     DateFrom = p.DateFrom,
                                                     DateTo = p.DateTo,
                                                     Description = x.Description,
                                                     DateTo_Minutes = Convert.ToInt32(((TimeSpan)(p.DateTo ?? p.DateFrom).Subtract(p.DateFrom)).TotalMinutes),
                                                     CaseSessionNotificationLists = caseSessionNotificationLists.Where(nl => nl.CaseSessionId == p.Id).ToList(),
                                                     CaseSessionActs = caseSessionActs.Where(s => s.CaseSessionId == p.Id && s.ActDeclaredDate != null).ToList(),
                                                     SessionMeetings = caseSessionMeetings.Where(meet => meet.CaseSessionId == p.Id).ToList(),
                                                     CaseSessionDocs = caseSessionDocVMs.Where(sessdoc => sessdoc.CaseSessionId == p.Id).ToList(),
                                                     SessionResultText = string.Join(", ", p.CaseSessionResults.Where(r => r.DateExpired == null).Select(r => r.SessionResult.Label))
                                                 }).ToList()
                                             })
                                             .FirstOrDefault();

            var judgeRep = caseLawUnitsActive.Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).FirstOrDefault();
            if (judgeRep != null)
            {
                caseElectronicFolderVM.JudgeRapporteur = judgeRep.LawUnitName + ((!string.IsNullOrEmpty(judgeRep.DepartmentLabel)) ? " състав: " + judgeRep.DepartmentLabel : string.Empty);
            }

            foreach (var caseSession in caseElectronicFolderVM.CaseSessions)
            {
                caseSession.CaseNotifications = caseNotificationService.CaseNotification_Select(CaseId, caseSession.Id, null).ToList();
                caseSession.SessionFastDocuments = caseSessionFastDocumentService.CaseSessionFastDocument_Select(caseSession.Id).ToList();

                var judgeRepSession = caseSessionLawUnits.Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter && x.CaseSessionId == caseSession.Id).FirstOrDefault();
                caseSession.JudgeRapporteur = judgeRepSession != null ? judgeRepSession.LawUnitName + ((!string.IsNullOrEmpty(judgeRepSession.DepartmentLabel)) ? " състав: " + judgeRepSession.DepartmentLabel : string.Empty) : string.Empty;

                foreach (var item in caseSessionResults.Where(r => r.Value == caseSession.Id.ToString()))
                    caseSession.SessionStateString += item.Text + " ";

                caseSession.Prokuror = string.Empty;
                foreach (var item in casePersonLists.Where(x => x.CaseSessionId == caseSession.Id && x.PersonRoleId == NomenclatureConstants.PersonRole.Prokuror))
                {
                    caseSession.Prokuror += item.FullName + "; ";
                }

                foreach (var caseSessionMeeting in caseSession.SessionMeetings)
                {
                    caseSessionMeeting.UsersNames = string.Empty;

                    var caseSessionMeetingUsers = caseSessionMeetingService.CaseSessionMeetingUser_Select(caseSessionMeeting.Id).ToList();

                    foreach (var caseSessionMeetingUser in caseSessionMeetingUsers)
                    {
                        caseSessionMeeting.UsersNames += caseSessionMeetingUser.SecretaryUserName + "; ";
                    }
                }
            }

            caseElectronicFolderVM.CaseLawUnits = caseLawUnitsAll.Where(x => x.CaseSessionId == null).ToList();
            caseElectronicFolderVM.CaseClassifications = caseClassificationService.CaseClassification_SelectObject(CaseId, null).ToList();
            caseElectronicFolderVM.CaseMigrations = caseMigrationResult.ToList();
            caseElectronicFolderVM.DocumentInstitutionCaseInfos = repo.AllReadonly<DocumentInstitutionCaseInfo>()
                                                                      .Include(x => x.Institution)
                                                                      .Include(x => x.InstitutionCaseType)
                                                                      .Where(x => x.DocumentId == caseElectronicFolderVM.DocumentId)
                                                                      .ToList();
            caseElectronicFolderVM.PaymentCases = moneyService.PaymentForCase_Select(caseElectronicFolderVM.Id).ToList();
            caseElectronicFolderVM.ExecLists = moneyService.ExecListForCase_Select(caseElectronicFolderVM.Id).ToList();
            caseElectronicFolderVM.ExpenseOrders = moneyService.ExpenseOrderForCase_Select(caseElectronicFolderVM.Id).ToList();
            caseElectronicFolderVM.DocumentDecisionCaseLists = documentService.DocumentDecisionCaseByCase_Select(caseElectronicFolderVM.Id).ToList();

            caseElectronicFolderVM.DocumentsOtherFromDifferentCourt = documentService.DocumentsOtherFromDifferentCourtByCaseId_Select(caseElectronicFolderVM.Id).ToList();
            caseElectronicFolderVM.DocumentsOtherFromSameCourt = documentService.DocumentsOtherFromSameCourtByCaseId_Select(caseElectronicFolderVM.Id).ToList();

            List<WorkTaskReportVM> workTaskReports = new List<WorkTaskReportVM>();

            workTaskReports.AddRange(repo.AllReadonly<WorkTask>()
                                         .Where(x => x.SourceType == SourceTypeSelectVM.Document &&
                                                     x.SourceId == caseElectronicFolderVM.DocumentId)
                                         .Select(x => new WorkTaskReportVM()
                                         {
                                             Id = x.Id,
                                             TaskTypeId = x.TaskTypeId,
                                             TaskTypeName = x.TaskType.Label,
                                             DateCreated = x.DateCreated,
                                             DateCompleted = x.DateCompleted,
                                             DescriptionCreated = x.DescriptionCreated,
                                             Description = x.Description
                                         })
                                         .ToList());

            foreach (var documentResolution in caseElectronicFolderVM.DocumentResolutions)
            {
                workTaskReports.AddRange(repo.AllReadonly<WorkTask>()
                                             .Where(x => x.SourceType == SourceTypeSelectVM.DocumentResolution &&
                                                         x.SourceId == documentResolution.Id)
                                             .Select(x => new WorkTaskReportVM()
                                             {
                                                 Id = x.Id,
                                                 TaskTypeId = x.TaskTypeId,
                                                 TaskTypeName = x.TaskType.Label,
                                                 DateCreated = x.DateCreated,
                                                 DateCompleted = x.DateCompleted,
                                                 DescriptionCreated = x.DescriptionCreated,
                                                 Description = x.Description
                                             })
                                             .ToList());
            }

            workTaskReports.AddRange(repo.AllReadonly<WorkTask>()
                                         .Where(x => x.SourceType == SourceTypeSelectVM.Case &&
                                                     x.SourceId == caseElectronicFolderVM.Id)
                                         .Select(x => new WorkTaskReportVM()
                                         {
                                             Id = x.Id,
                                             TaskTypeId = x.TaskTypeId,
                                             TaskTypeName = x.TaskType.Label,
                                             DateCreated = x.DateCreated,
                                             DateCompleted = x.DateCompleted,
                                             DescriptionCreated = x.DescriptionCreated,
                                             Description = x.Description
                                         })
                                         .ToList());

            caseElectronicFolderVM.WorkTaskReports = workTaskReports;

            caseElectronicFolderVM.DocumentResolutionCase = repo.AllReadonly<DocumentResolutionCase>()
                                                                .Where(x => x.CaseId == CaseId &&
                                                                            x.DocumentResolution.ResolutionTypeId == DocumentConstants.ResolutionTypes.ResolutionForSelection &&
                                                                            x.DocumentResolution.DateExpired == null)
                                                                .Select(x => new DocumentResolutionListVM()
                                                                {
                                                                    Id = x.DocumentResolution.Id,
                                                                    ResolutionTypeId = x.DocumentResolution.ResolutionTypeId,
                                                                    ResolutionTypeLabel = x.DocumentResolution.ResolutionType.Label,
                                                                    Label = x.DocumentResolution.ResolutionType.Label + " " + x.DocumentResolution.RegNumber + "/" + (x.DocumentResolution.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"),
                                                                    RegDate = x.DocumentResolution.RegDate,
                                                                    RegNumber = x.DocumentResolution.RegNumber
                                                                })
                                                                .ToList();

            return caseElectronicFolderVM;
        }

        public CaseMigrationVM Case_GetPriorCase(long documentId)
        {
            return caseMigrationService.Case_GetPriorCase(documentId);
        }

        public CaseMigrationVM Case_GetPriorCaseEISPP(long documentId, string eisppNumber)
        {
            var priorCaseId = repo.All<DocumentCaseInfo>(x => x.DocumentId == documentId).Select(x => x.CaseId).FirstOrDefault() ?? 0;
            if (priorCaseId == 0)
            {
                return null;
            }

            var eisppMigrationToProsecutors = repo.AllReadonly<CaseMigration>()
                                            .Include(x => x.CaseMigrationType)
                                            .Include(x => x.Case)
                                            .ThenInclude(x => x.Court)
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

        public bool SaveDataDepersonalizationHistory(int CaseId, IEnumerable<DepersonalizationHistoryItem> model, int actId,
                     bool saveDepersonalizeUser)
        {
            bool save = false;
            if (saveDepersonalizeUser)
            {
                //Винаги при натискане на бутон финализиране при обезличаването да се записва потребителя
                var act = GetById<CaseSessionAct>(actId);
                act.DepersonalizeUserId = userContext.UserId;
                repo.Update(act);
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
            int oldNumberInt = Utils.ToInt(oldNumber);
            var hasOldedAutoNumberedCases = repo.AllReadonly<Case>()
                        .Where(x => x.CourtId == userContext.CourtId && x.CaseGroupId == CaseGroupId
                        && x.ShortNumberValue <= oldNumberInt && x.RegDate.Year == oldDate.Year
                        && (x.IsOldNumber ?? false) == false)
                        .Any();
            return !hasOldedAutoNumberedCases && !repo.AllReadonly<Case>()
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

            //ДЕЛО
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<Case, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate.Date >= dateFromSearch.Date && x.RegDate.Date <= dateToSearch.Date;

            Expression<Func<Case, bool>> yearSearch = x => true;
            if ((model.CaseYear ?? 0) > 0)
                yearSearch = x => x.RegDate.Year == model.CaseYear;

            Expression<Func<Case, bool>> caseRegnumberSearch = x => true;
            if (!string.IsNullOrEmpty(model.RegNumber))
                caseRegnumberSearch = x => EF.Functions.ILike(x.RegNumber, model.RegNumber.ToCasePaternSearch());

            Expression<Func<Case, bool>> caseEisppnumberSearch = x => true;
            if (!string.IsNullOrEmpty(model.EisppNumber))
            {
                caseEisppnumberSearch = x => EF.Functions.ILike(x.EISSPNumber, model.EisppNumber.ToPaternSearch());
            }

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.CaseGroupId == model.CaseGroupId;

            Expression<Func<Case, bool>> caseTypeWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeWhere = x => x.CaseTypeId == model.CaseTypeId;

            Expression<Func<Case, bool>> caseStateWhere = x => true;
            if (model.CaseStateId == NomenclatureConstants.CaseState.WithoutArchive)
                caseStateWhere = x => x.CaseStateId != NomenclatureConstants.CaseState.Archive;
            else if (model.CaseStateId == NomenclatureConstants.CaseState.WithArchive)
                caseStateWhere = x => x.CaseStateId == NomenclatureConstants.CaseState.Archive;

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                      (a.DateTo ?? dateEnd).Date >= x.RegDate.Date && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            Expression<Func<Case, bool>> caseClassificationWhere = x => true;
            if (model.CaseClassificationId > 0)
                caseClassificationWhere = x => x.CaseClassifications
                             .Where(a => a.ClassificationId == model.CaseClassificationId &&
                                         a.DateTo == null &&
                                         a.CaseSessionId == null).Any();

            //ЗАСЕДАНИЕ
            DateTime dateFromSession = model.Session_DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.Session_DateFrom;
            DateTime dateToSession = model.SessionDateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.SessionDateTo;

            Expression<Func<Case, bool>> dateSessionSearch = x => true;
            if (model.Session_DateFrom != null || model.SessionDateTo != null)
                dateSessionSearch = x => x.CaseSessions.Where(a => a.DateFrom.Date >= dateFromSession.Date && a.DateFrom.Date <= dateToSession.Date).Any();

            Expression<Func<Case, bool>> sessionTypeWhere = x => true;
            if (model.SessionTypeId > 0)
                sessionTypeWhere = x => x.CaseSessions.Where(a => a.SessionTypeId == model.SessionTypeId).Any();

            Expression<Func<Case, bool>> hallWhere = x => true;
            if (model.CourtHallId > 0)
                hallWhere = x => x.CaseSessions.Where(a => a.CourtHallId == model.CourtHallId).Any();

            Expression<Func<Case, bool>> sessionStateWhere = x => true;
            if (model.SessionStateId > 0)
                sessionStateWhere = x => x.CaseSessions.Where(a => a.SessionStateId == model.SessionStateId).Any();

            Expression<Func<Case, bool>> sessionResultWhere = x => true;
            if (model.SessionResultId > 0)
                sessionResultWhere = x => x.CaseSessions.Where(a => a.CaseSessionResults.Any(res => res.SessionResultId == model.SessionResultId)).Any();


            //АКТОВЕ
            Expression<Func<Case, bool>> sessionActWhere = x => true;
            if ((model.ActTypeId > 0) ||
                (model.ActDate != null) ||
                !string.IsNullOrEmpty(model.ActNumber) ||
                model.ActIsFinalDocHidden ||
                (model.ActLawBaseId > 0))
            {
                sessionActWhere = x => x.CaseSessionActs.Any(a => (a.DateExpired == null) &&
                                                                  ((model.ActTypeId > 0) ? a.ActTypeId == model.ActTypeId : true) &&
                                                                  ((model.ActDate != null) ? (a.ActDate != null && ((DateTime)a.ActDate).Date == (model.ActDate ?? DateTime.Now).Date) : true) &&
                                                                  (!string.IsNullOrEmpty(model.ActNumber) ? (a.RegNumber == model.ActNumber) : true) &&
                                                                  (model.ActIsFinalDocHidden ? (a.IsFinalDoc == true && a.ActStateId == NomenclatureConstants.SessionActState.ComingIntoForce && a.ActDeclaredDate != null) : true) &&
                                                                  ((model.ActLawBaseId > 0) ? a.CaseSessionActLawBases.Where(b => b.LawBaseId == model.ActLawBaseId).Any() : true));
            }

            //Expression<Func<Case, bool>> sessionActTypeWhere = x => true;
            //if (model.ActTypeId > 0)
            //    sessionActTypeWhere = x => x.CaseSessions.Where(a => a.CaseSessionActs.Where(b => b.ActTypeId == model.ActTypeId).Any()).Any();

            //Expression<Func<Case, bool>> sessionActDateWhere = x => true;
            //if (model.ActDate != null)
            //    sessionActDateWhere = x => x.CaseSessions.Where(a => a.CaseSessionActs.Where(b => b.ActDate != null &&
            //                    ((DateTime)b.ActDate).Date == ((DateTime)model.ActDate).Date).Any()).Any();

            //Expression<Func<Case, bool>> sessionActNumberWhere = x => true;
            //if (string.IsNullOrEmpty(model.ActNumber) == false)
            //    sessionActNumberWhere = x => x.CaseSessions.Where(a => a.CaseSessionActs.Where(b => b.RegNumber == model.ActNumber).Any()).Any();

            //Expression<Func<Case, bool>> finalActSearch = x => true;
            //if (model.ActIsFinalDocHidden == true)
            //    finalActSearch = x => x.CaseSessionActs.Where(a => a.IsFinalDoc == true && a.DateExpired == null &&
            //                                     NomenclatureConstants.SessionActState.EnforcedStates.Contains(a.ActStateId)).Any();

            //Expression<Func<Case, bool>> actLawBaseSearch = x => true;
            //if (model.ActLawBaseId > 0)
            //    actLawBaseSearch = x => x.CaseSessionActs.Where(a => a.DateExpired == null &&
            //                        a.CaseSessionActLawBases.Where(b => b.LawBaseId == model.ActLawBaseId).Any()).Any();

            //СВЪРЗАНИ ДЕЛА
            Expression<Func<Case, bool>> linkCaseCourtWhere = x => true;
            if (model.LinkDelo_CourtId > 0)
                linkCaseCourtWhere = x => x.Document.DocumentCaseInfo.Where(a => a.Case.CourtId == model.LinkDelo_CourtId).Any();

            Expression<Func<Case, bool>> linkCaseIdWhere = x => true;
            if (!string.IsNullOrEmpty(model.LinkDelo_RegNumber))
                linkCaseIdWhere = x => x.Document.DocumentCaseInfo.Where(a => a.Case != null && EF.Functions.ILike(a.Case.RegNumber, model.LinkDelo_RegNumber.ToCasePaternSearch())).Any();

            Expression<Func<Case, bool>> linkDescriptionWhere = x => true;
            if (!string.IsNullOrEmpty(model.LinkDelo_Description))
            {
                var stringFind = model.LinkDelo_Description.ToUpper();
                linkDescriptionWhere = x => x.Document.DocumentCaseInfo.Where(a => a.Case != null && EF.Functions.ILike(a.Description, stringFind.ToPaternSearch())).Any();
            }

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

            Expression<Func<Case, bool>> regNumOtherSystem = x => true;
            if (!string.IsNullOrEmpty(model.RegNumberOtherSystem))
                regNumOtherSystem = x => x.Document.DocumentCaseInfo.Where(a => (EF.Functions.ILike(a.CaseRegNumber, model.RegNumberOtherSystem.ToEndingPaternSearch())) && (a.IsLegacyCase ?? false)).Any();

            Expression<Func<Case, bool>> yearOtherSystem = x => true;
            if (model.YearOtherSystem > 0)
                yearOtherSystem = x => x.Document.DocumentCaseInfo.Where(a => (a.CaseYear == model.YearOtherSystem) && (a.IsLegacyCase ?? false)).Any();

            Expression<Func<Case, bool>> courtOtherSystem = x => true;
            if (model.CourtOtherSystem > 0)
                courtOtherSystem = x => x.Document.DocumentCaseInfo.Where(a => (a.CourtId == model.CourtOtherSystem) && (a.IsLegacyCase ?? false)).Any();

            //ДОКУМЕНТИ
            Expression<Func<Case, bool>> documentDateWhere = x => true;
            if (model.DateDoc != null)
                documentDateWhere = x => (x.Document.DocumentDate.Date == model.DateDoc || repo.AllReadonly<DocumentCaseInfo>()
                                                                                               .Any(a => a.CaseId == x.Id &&
                                                                                                         a.Document.DocumentDate.Date == model.DateDoc));

            Expression<Func<Case, bool>> documentNumberWhere = x => true;
            if (string.IsNullOrEmpty(model.NumberDoc) == false)
                documentNumberWhere = x => (x.Document.DocumentNumber == model.NumberDoc) || repo.AllReadonly<DocumentCaseInfo>()
                                                                                                 .Where(a => a.CaseId == x.Id &&
                                                                                                             a.Document.DocumentNumber == model.NumberDoc)
                                                                                                 .Any();

            //СТРАНИ
            Expression<Func<Case, bool>> identifikatorPersonWhere = x => true;
            if (string.IsNullOrEmpty(model.IdentifikatorPerson) == false)
                identifikatorPersonWhere = x => x.CasePersons.Where(a => a.CaseSessionId == null && a.Uic.ToLower() == model.IdentifikatorPerson.ToLower()).Any();

            Expression<Func<Case, bool>> namePersonWhere = x => true;
            if (string.IsNullOrEmpty(model.NamePerson) == false)
                namePersonWhere = x => x.CasePersons.Where(a => a.CaseSessionId == null && EF.Functions.ILike(a.FullName, model.NamePerson.ToPaternSearch())).Any();

            //СЪДЕБЕН СЪСТАВ
            Expression<Func<Case, bool>> identifikatorCaseLawUnitWhere = x => true;
            if (string.IsNullOrEmpty(model.IdentifikatorCaseLawUnit) == false)
                identifikatorCaseLawUnitWhere = x => x.CaseLawUnits.Where(a => a.CaseSessionId == null && a.LawUnit.Uic.ToLower() == model.IdentifikatorCaseLawUnit.ToLower()).Any();

            Expression<Func<Case, bool>> nameCaseLawUnitWhere = x => true;
            if (string.IsNullOrEmpty(model.NameCaseLawUnit) == false)
                nameCaseLawUnitWhere = x => x.CaseLawUnits.Where(a => a.CaseSessionId == null && EF.Functions.ILike(a.LawUnit.FullName, model.NameCaseLawUnit.ToPaternSearch())).Any();

            Expression<Func<Case, bool>> courtDepartment = x => true;
            if (model.CourtDepartmentId > 0)
                courtDepartment = x => x.CaseLawUnits.Where(a => a.CaseSessionId == null && a.CourtDepartmentId == model.CourtDepartmentId).Any();

            //Коментирано от К.Борисов - не се ползваше
            //var c = repo.AllReadonly<Case>()
            //           .Include(x => x.CaseType)
            //           .Include(x => x.CaseCode)
            //           .Include(x => x.ProcessPriority)
            //           .Include(x => x.CaseState)
            //           .Include(x => x.CaseReason)
            //           .Where(x => x.CourtId == courtId)
            //           .Where(x => x.CaseStateId != NomenclatureConstants.CaseState.Draft)
            //           .Where(dateSearch)
            //           .Where(caseRegnumberSearch)
            //           .Where(yearSearch)
            //           .Where(caseGroupWhere)
            //           .Where(caseTypeWhere)
            //           .Where(caseStateWhere)
            //           .Where(judgeReporterSearch)
            //           .Where(dateSessionSearch)
            //           .Where(sessionTypeWhere)
            //           .Where(hallWhere)
            //           .Where(sessionStateWhere)
            //           .Where(sessionResultWhere)
            //           .Where(linkCaseCourtWhere)
            //           .Where(linkCaseIdWhere)
            //           .Where(linkDescriptionWhere)
            //           .Where(linkInstitutionTypeWhere)
            //           .Where(linkInstitutionIdWhere)
            //           .Where(linkInstitutionYearWhere)
            //           .Where(linkInstitutionCaseNumberWhere)
            //           .Where(regNumOtherSystem)
            //           .Where(documentDateWhere)
            //           .Where(documentNumberWhere)
            //           .Where(identifikatorPersonWhere)
            //           .Where(namePersonWhere)
            //           .Where(identifikatorCaseLawUnitWhere)
            //           .Where(nameCaseLawUnitWhere)
            //           .Where(caseClassificationWhere)
            //           .Where(courtDepartment)
            //           .Where(sessionActWhere)
            //           //.Where(finalActSearch)
            //           //.Where(actLawBaseSearch)
            //           //.Where(sessionActTypeWhere)
            //           //.Where(sessionActDateWhere)
            //           //.Where(sessionActNumberWhere)
            //           .Select(x => new CaseVM()
            //           {
            //               Id = x.Id,
            //               CourtId = x.CourtId,
            //               CaseTypeLabel = (x.CaseType != null) ? x.CaseType.Code : string.Empty,
            //               CaseCodeLabel = (x.CaseCode != null) ? x.CaseCode.Code + " " + x.CaseCode.Label : string.Empty,
            //               ProcessPriorityLabel = (x.ProcessPriority != null) ? x.ProcessPriority.Label : string.Empty,
            //               CaseStateLabel = (x.CaseState != null) ? x.CaseState.Label : string.Empty,
            //               ShortNumberVal = x.ShortNumberValue ?? 0,
            //               ShortNumber = Convert.ToString(int.Parse(x.ShortNumber)),
            //               RegNumber = x.RegNumber,
            //               RegDate = x.RegDate,
            //               CaseReasonLabel = (x.CaseReason != null) ? x.CaseReason.Label : string.Empty,
            //               CaseStateDescription = x.CaseStateDescription
            //           }).ToSql();

            return repo.AllReadonly<Case>()
                       .Include(x => x.CaseType)
                       .Include(x => x.CaseCode)
                       .Include(x => x.ProcessPriority)
                       .Include(x => x.CaseState)
                       .Include(x => x.CaseReason)
                       .Where(x => x.CourtId == courtId)
                       .Where(x => !NomenclatureConstants.CaseState.FakeCase.Contains(x.CaseStateId))
                       .Where(dateSearch)
                       .Where(caseRegnumberSearch)
                       .Where(caseEisppnumberSearch)
                       .Where(yearSearch)
                       .Where(caseGroupWhere)
                       .Where(caseTypeWhere)
                       .Where(caseStateWhere)
                       .Where(judgeReporterSearch)
                       .Where(dateSessionSearch)
                       .Where(sessionTypeWhere)
                       .Where(hallWhere)
                       .Where(sessionStateWhere)
                       .Where(sessionResultWhere)
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
                       .Where(documentDateWhere)
                       .Where(documentNumberWhere)
                       .Where(identifikatorPersonWhere)
                       .Where(namePersonWhere)
                       .Where(identifikatorCaseLawUnitWhere)
                       .Where(nameCaseLawUnitWhere)
                       .Where(caseClassificationWhere)
                       .Where(courtDepartment)
                       .Where(sessionActWhere)
                       //.Where(finalActSearch)
                       //.Where(actLawBaseSearch)
                       //.Where(sessionActTypeWhere)
                       //.Where(sessionActDateWhere)
                       //.Where(sessionActNumberWhere)
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
                foreach (var cdnItem in cdnItems)
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

            if (caseElectronic.JudgeRapporteur != string.Empty)
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
            var caseCase = CaseElectronicFolder_Select(CaseId);
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
                    }

                    if (caseCase.CaseMigrations.Count() > 0)
                    {
                        fileHtml.Add("8. Свързани дела: </br>");
                        int num = 0;
                        foreach (var caseMigration in caseCase.CaseMigrations)
                        {
                            num++;
                            var _row = "8." + num + ". " + "Дело: " + caseMigration.CaseRegNumber + "/" + caseMigration.CaseRegDate.ToString("dd.MM.yyyy") + " - " + caseMigration.CaseCourtName + "</br>";
                            fileHtml.Add(_row);
                        }
                    }

                    if (caseCase.RegixReports.Count() > 0)
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
        /// Справка за Образувани дела с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseSprVM> CaseReportMaturity_Select(int courtId, CaseFilterReport model)
        {
            List<CaseSprVM> result = new List<CaseSprVM>();

            model.DateFrom = NomenclatureExtensions.ForceStartDate(model.DateFrom);
            model.DateTo = NomenclatureExtensions.ForceEndDate(model.DateTo);
            var nowDateTime = DateTime.Now;

            var cases = repo.AllReadonly<Case>()
                            .Include(x => x.CasePersons)
                            .ThenInclude(x => x.PersonMaturity)
                            .Include(x => x.CasePersons)
                            .ThenInclude(x => x.PersonRole)
                            .Include(x => x.CaseGroup)
                            .Include(x => x.CaseType)
                            .Include(x => x.CaseCode)
                            .Include(x => x.CaseLawUnits)
                            .ThenInclude(x => x.LawUnit)
                            .Include(x => x.CaseLawUnits)
                            .ThenInclude(x => x.JudgeRole)
                            .Include(x => x.CaseLawUnits)
                            .ThenInclude(x => x.CourtDepartment)
                            .Where(x => (x.CourtId == courtId) &&
                                        ((x.RegDate >= model.DateFrom) && (x.RegDate <= (model.DateTo ?? nowDateTime.AddYears(100)))) &&
                                        (x.CaseStateId != NomenclatureConstants.CaseState.Draft) &&
                                        (x.CasePersons.Any(p => p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderAged || p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderLegalAge)) &&
                                        ((model.CaseGroupId > 0) ? x.CaseGroupId == model.CaseGroupId : true) &&
                                        ((model.CaseTypeId > 0) ? x.CaseTypeId == model.CaseTypeId : true) &&
                                        ((model.CaseCodeId > 0) ? x.CaseCodeId == model.CaseCodeId : true) &&
                                        ((model.JudgeReporterId > 0) ? (x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                                                                                                  (a.DateTo ?? nowDateTime.AddYears(100)) >= nowDateTime && a.LawUnitId == model.JudgeReporterId &&
                                                                                                  a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any()) : true))
                            .Where(x => !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                            .ToList();

            var resultFinish = repo.AllReadonly<SessionResultGrouping>()
                .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseWithoutFinalAct_Result)
                .Select(x => x.SessionResultId)
                .ToList();

            foreach (var @case in cases)
            {
                var judgeRep = @case.CaseLawUnits.Where(x => x.CaseSessionId == null && (x.DateTo ?? DateTime.Now.AddYears(100)).Date >= DateTime.Now.Date && x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).OrderByDescending(a => a.DateFrom).FirstOrDefault();
                var act = repo.AllReadonly<CaseSessionAct>()
                              .Where(x => x.CaseId == @case.Id &&
                                          x.IsFinalDoc)
                              .FirstOrDefault();

                var isFinishResult = repo.AllReadonly<CaseSession>()
                                         .Any(x => (x.CaseId == @case.Id) &&
                                                   (x.DateExpired == null) &&
                                                   (x.CaseSessionResults.Where(a => a.DateExpired == null && resultFinish.Contains(a.SessionResultId)).Any()));

                var item = new CaseSprVM()
                {
                    Id = @case.Id,
                    CaseGroupLabel = @case.CaseGroup.Label + " - " + @case.CaseType.Label,
                    CaseCodeLabel = @case.CaseCode.Code + " " + @case.CaseCode.Label,
                    CaseRegNum = @case.RegNumber + "/" + @case.RegDate.Year.ToString() + "г.",
                    CaseRegDate = @case.RegDate,
                    CaseBeginDate = @case.RegDate,
                    CasePersons = GetStringPerson(@case.CasePersons.Where(p => p.CaseSessionId == null && (p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderAged || p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderLegalAge)).ToList()),
                    JudgeReport = (judgeRep != null) ? judgeRep.LawUnit.FullName + ((judgeRep.CourtDepartment != null) ? " състав: " + judgeRep.CourtDepartment.Label : string.Empty) : string.Empty,
                    CaseEndDate = ((act != null) && (isFinishResult)) ? act.ActDeclaredDate : null
                };

                result.Add(item);
            }

            return result.AsQueryable();
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

            var resultFinish = repo.AllReadonly<SessionResultGrouping>()
                .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseWithoutFinalAct_Result)
                .Select(x => x.SessionResultId)
                .ToList();

            var caseCase = repo.AllReadonly<Case>()
                               .Where(x => (x.CourtId == courtId) &&
                                           ((x.RegDate >= model.DateFrom) && (x.RegDate <= (model.DateTo ?? DateTime.Now.AddYears(100)))) &&
                                           (x.CaseStateId != NomenclatureConstants.CaseState.Draft) &&
                                           ((model.CaseGroupId > 0) ? x.CaseGroupId == model.CaseGroupId : true) &&
                                           ((model.CaseTypeId > 0) ? x.CaseTypeId == model.CaseTypeId : true) &&
                                           ((x.CaseSessionResults.Where(r => r.DateExpired == null && resultFinish.Contains(r.SessionResultId)).Any() && (!x.CaseSessionActs.Any(a => a.DateExpired == null && a.IsFinalDoc))) ||
                                            (x.CaseSessionResults.Where(r => r.DateExpired == null && resultFinish.Contains(r.SessionResultId)).Any() && (x.CaseSessionActs.Any(a => a.DateExpired == null && a.ActDeclaredDate == null && a.IsFinalDoc))) ||
                                            (!x.CaseSessionResults.Where(r => r.DateExpired == null && resultFinish.Contains(r.SessionResultId)).Any() && (x.CaseSessionActs.Any(a => a.DateExpired == null && a.IsFinalDoc && a.ActDeclaredDate == null)))))
                               .Select(x => new CaseSprVM()
                               {
                                   Id = x.Id,
                                   JudgeReport = x.CaseLawUnits.Where(l => l.CaseSessionId == null && l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).OrderByDescending(l => l.DateFrom).Select(l => l.LawUnit.FullName + ((l.CourtDepartment != null) ? " състав: " + l.CourtDepartment.Label : string.Empty)).FirstOrDefault(),
                                   CaseTypeLabel = x.CaseType.Label,
                                   CaseRegNum = x.RegNumber + "/" + x.RegDate.Year.ToString() + "г.",
                                   CaseCodeLabel = x.CaseCode.Code + " " + x.CaseCode.Label,
                                   CaseRegDate = x.RegDate,
                                   CaseEndDate = null,
                               })
                               .ToList();
            result.AddRange(caseCase);

            //var caseSessions = repo.AllReadonly<CaseSession>()
            //                       .Include(x => x.Case)
            //                       .ThenInclude(x => x.CaseGroup)
            //                       .Include(x => x.Case)
            //                       .ThenInclude(x => x.CaseType)
            //                       .Include(x => x.Case)
            //                       .ThenInclude(x => x.CaseCode)
            //                       .Include(x => x.Case)
            //                       .ThenInclude(x => x.CaseLawUnits)
            //                       .ThenInclude(x => x.LawUnit)
            //                       .Include(x => x.Case)
            //                       .ThenInclude(x => x.CaseLawUnits)
            //                       .ThenInclude(x => x.JudgeRole)
            //                       .Include(x => x.Case)
            //                       .ThenInclude(x => x.CaseLawUnits)
            //                       .ThenInclude(x => x.CourtDepartment)
            //                       .Include(x => x.Case)
            //                       .ThenInclude(x => x.CaseLifecycles)
            //                       .Where(x => (x.CourtId == courtId) &&
            //                                ((x.Case.RegDate >= model.DateFrom) && (x.Case.RegDate <= (model.DateTo ?? DateTime.Now.AddYears(100)))) &&
            //                                (x.Case.CaseStateId != NomenclatureConstants.CaseState.Draft) &&
            //                                ((model.CaseGroupId > 0) ? x.Case.CaseGroupId == model.CaseGroupId : true) &&
            //                                ((model.CaseTypeId > 0) ? x.Case.CaseTypeId == model.CaseTypeId : true) &&
            //                                ((!x.CaseSessionResults.Where(a => a.DateExpired == null && resultFinish.Contains(a.SessionResultId)).Any()) ||
            //                                (!x.CaseSessionActs.Any(b => b.DateExpired == null && b.IsFinalDoc && (b.ActStateId != NomenclatureConstants.SessionActState.Project && b.ActStateId != NomenclatureConstants.SessionActState.Registered))) ||
            //                                (x.Case.CaseLifecycles.Any(l => l.DateExpired == null && l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress && l.DateTo == null))))
            //                       .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
            //                       .ToList();

            //foreach (var caseSession in caseSessions)
            //{
            //    var judgeRep = caseSession.Case.CaseLawUnits.Where(x => x.CaseSessionId == null && x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).OrderByDescending(a => a.DateFrom).FirstOrDefault();
            //    //var caseLifecycles = caseLifecycleService.CaseLifecycle_Select(caseSession.CaseId).ToList();
            //    //var caseLifecyclesText = string.Empty;
            //    //if (caseLifecycles != null)
            //    //{
            //    //    caseLifecyclesText = caseLifecycles.Where(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress).Sum(x => x.DurationMonths).ToString();
            //    //}

            //    if (!result.Any(x => x.Id == caseSession.CaseId))
            //    {
            //        var item = new CaseSprVM()
            //        {
            //            Id = caseSession.CaseId,
            //            JudgeReport = (judgeRep != null) ? judgeRep.LawUnit.FullName + ((judgeRep.CourtDepartment != null) ? " състав: " + judgeRep.CourtDepartment.Label : string.Empty) : string.Empty,
            //            CaseTypeLabel = caseSession.Case.CaseType.Label,
            //            CaseRegNum = caseSession.Case.RegNumber + "/" + caseSession.Case.RegDate.Year.ToString() + "г.",
            //            CaseCodeLabel = caseSession.Case.CaseCode.Code + " " + caseSession.Case.CaseCode.Label,
            //            CaseRegDate = caseSession.Case.RegDate,
            //            CaseEndDate = null,
            //        };

            //        result.Add(item);
            //    }
            //}

            return result.AsQueryable();
        }

        public byte[] CaseWithoutFinalExportExcel(CaseFilterReport model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = CaseWithoutFinal_Select(userContext.CourtId, model).ToList();

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
                    x => x.CaseLifeCycle,
                    x => x.CaseStateName,
                },
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );

            return excelService.ToArray();
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
        /// Справка за Oбразувани и свършени дела за корупционни престъпления
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseSprVM> CaseCorruptCrimes_Select(int courtId, CaseFilterReport model)
        {
            List<CaseSprVM> result = new List<CaseSprVM>();

            model.DateFrom = NomenclatureExtensions.ForceStartDate(model.DateFrom);
            model.DateTo = NomenclatureExtensions.ForceEndDate(model.DateTo);
            var dateAddYear = DateTime.Now.AddYears(100);

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Where(a => a.CaseSessionId == null && (a.DateTo ?? dateAddYear).Date >= x.RegDate && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            var caseCases = repo.AllReadonly<Case>()
                                .Include(x => x.CasePersons)
                                .ThenInclude(x => x.PersonMaturity)
                                .Include(x => x.CasePersons)
                                .ThenInclude(x => x.PersonRole)
                                .Include(x => x.Court)
                                .Include(x => x.CaseCode)
                                .Include(x => x.CaseClassifications)
                                .Include(x => x.CaseLawUnits)
                                .ThenInclude(x => x.LawUnit)
                                .Include(x => x.CaseLawUnits)
                                .ThenInclude(x => x.CourtDepartment)
                                .Where(x => (x.CourtId == courtId) &&
                                            ((x.RegDate >= model.DateFrom) && (x.RegDate <= (model.DateTo ?? DateTime.Now.AddYears(100)))) &&
                                            (x.CaseClassifications.Any(c => c.DateTo == null && c.ClassificationId == NomenclatureConstants.CaseClassifications.CorruptCase))
                                            /*(repo.AllReadonly<CaseSessionAct>().Where(a => a.CaseId == x.Id).Any(a => a.IsFinalDoc))*/)
                                .Where(judgeReporterSearch)
                                .Where(x => !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                                .ToList();

            foreach (var @case in caseCases)
            {
                var casePersonSentences = repo.AllReadonly<CasePersonSentence>()
                                              .Include(x => x.SentenceResultType)
                                              .Include(x => x.CasePerson)
                                              .ThenInclude(x => x.PersonRole)
                                              .Include(x => x.CaseSessionAct)
                                              .ThenInclude(x => x.ActType)
                                              .Include(x => x.CasePersonSentenceLawbases)
                                              .ThenInclude(x => x.SentenceLawbase)
                                              .Include(x => x.CasePersonSentencePunishments)
                                              .ThenInclude(x => x.SentenceType)
                                              .Include(x => x.CasePersonSentencePunishments)
                                              .ThenInclude(x => x.CasePersonSentencePunishmentCrimes)
                                              .Where(x => x.CaseId == @case.Id)
                                              .ToList();

                var item = new CaseSprVM()
                {
                    Id = @case.Id,
                    CaseRegNum = @case.RegNumber + "/" + @case.RegDate.Year.ToString() + "г.",
                    CaseRegDate = @case.RegDate,
                    CourtLabel = @case.Court.Label,
                    CasePersons = GetStringPersonNameNewLine(@case.CasePersons.Where(p => p.CaseSessionId == null && p.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.RightSide).ToList()),
                    CasePersonRoles = GetStringPersonRoleNameNewLine(@case.CasePersons.Where(p => p.CaseSessionId == null && p.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.RightSide).ToList()),
                    CaseCodeLabel = @case.CaseCode.Label,
                    CasePersonSentenceInfo = GetSentenseInfo(casePersonSentences),
                    JudgeReport = @case.CaseLawUnits.Where(l => l.CaseSessionId == null && l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                          (l.DateTo ?? DateTime.Now.AddYears(100)) >= @case.RegDate)
                                                .Select(l => l.LawUnit.FullName + ((l.CourtDepartment != null) ? " състав: " + l.CourtDepartment.Label : string.Empty))
                                                .FirstOrDefault(),
                    IsExistFinalAct = repo.AllReadonly<CaseSessionAct>().Where(a => a.CaseId == @case.Id).Any(a => a.IsFinalDoc) ? "Да" : "Не"
                };

                result.Add(item);
            }

            return result.AsQueryable();
        }

        /// <summary>
        /// Справка Свършили/Несвършили дела с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="WithFinalAct"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseSprVM> CaseFinalActMaturity_Select(int courtId, bool WithFinalAct, CaseFilterReport model)
        {
            List<CaseSprVM> result = new List<CaseSprVM>();

            model.DateFrom = NomenclatureExtensions.ForceStartDate(model.DateFrom);
            model.DateTo = NomenclatureExtensions.ForceEndDate(model.DateTo);

            var nowDateTime = DateTime.Now;

            var resultFinish = repo.AllReadonly<SessionResultGrouping>()
                .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseWithoutFinalAct_Result)
                .Select(x => x.SessionResultId)
                .ToList();

            var caseSessions = repo.AllReadonly<CaseSession>()
                                   .Include(x => x.Case)
                                   .ThenInclude(x => x.CaseGroup)
                                   .Include(x => x.Case)
                                   .ThenInclude(x => x.CaseType)
                                   .Include(x => x.Case)
                                   .ThenInclude(x => x.CaseCode)
                                   .Include(x => x.Case)
                                   .ThenInclude(x => x.CaseLawUnits)
                                   .ThenInclude(x => x.LawUnit)
                                   .Include(x => x.Case)
                                   .ThenInclude(x => x.CaseLawUnits)
                                   .ThenInclude(x => x.JudgeRole)
                                   .Include(x => x.Case)
                                   .ThenInclude(x => x.CaseLawUnits)
                                   .ThenInclude(x => x.CourtDepartment)
                                   .Include(x => x.Case)
                                   .ThenInclude(x => x.CasePersons)
                                   .ThenInclude(x => x.PersonMaturity)
                                   .Include(x => x.Case)
                                   .ThenInclude(x => x.CasePersons)
                                   .ThenInclude(x => x.PersonRole)
                                   .Where(x => (x.CourtId == courtId) &&
                                            ((x.Case.RegDate >= model.DateFrom) && (x.Case.RegDate <= (model.DateTo ?? nowDateTime.AddYears(100)))) &&
                                            (x.Case.CaseStateId != NomenclatureConstants.CaseState.Draft) &&
                                            (x.Case.CasePersons.Any(p => p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderAged || p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderLegalAge)) &&
                                            ((model.CaseGroupId > 0) ? x.Case.CaseGroupId == model.CaseGroupId : true) &&
                                            ((model.CaseTypeId > 0) ? x.Case.CaseTypeId == model.CaseTypeId : true) &&
                                            ((model.CaseCodeId > 0) ? x.Case.CaseCodeId == model.CaseCodeId : true) &&
                                            ((WithFinalAct) ? ((x.CaseSessionResults.Where(a => a.DateExpired == null && resultFinish.Contains(a.SessionResultId)).Any()) &&
                                                              (x.CaseSessionActs.Any(b => b.DateExpired == null && (b.ActDeclaredDate >= model.ActDeclaredDateFrom && b.ActDeclaredDate <= model.ActDeclaredDateTo) && b.IsFinalDoc && (b.ActStateId != NomenclatureConstants.SessionActState.Project && b.ActStateId != NomenclatureConstants.SessionActState.Registered)))) :
                                                              ((!x.CaseSessionResults.Where(a => a.IsActive && !resultFinish.Contains(a.SessionResultId)).Any()) ||
                                                              (!x.CaseSessionActs.Any(b => b.DateExpired == null && b.ActDeclaredDate <= model.DateToSpr && b.IsFinalDoc && (b.ActStateId != NomenclatureConstants.SessionActState.Project && b.ActStateId != NomenclatureConstants.SessionActState.Registered))))) &&
                                            ((model.JudgeReporterId > 0) ? (x.Case.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                                                                                                      (a.DateTo ?? nowDateTime.AddYears(100)) >= nowDateTime && a.LawUnitId == model.JudgeReporterId &&
                                                                                                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any()) : true))
                                   .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                                   .ToList();

            foreach (var caseSession in caseSessions)
            {
                var judgeRep = caseSession.Case.CaseLawUnits.Where(x => x.CaseSessionId == null && (x.DateTo ?? DateTime.Now.AddYears(100)).Date >= DateTime.Now.Date && x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).OrderByDescending(a => a.DateFrom).FirstOrDefault();
                var act = repo.AllReadonly<CaseSessionAct>()
                              .Where(x => x.CaseId == caseSession.CaseId &&
                                          x.IsFinalDoc)
                              .FirstOrDefault();


                if (!result.Any(x => x.Id == caseSession.CaseId))
                {
                    var item = new CaseSprVM()
                    {
                        Id = caseSession.CaseId,
                        JudgeReport = (judgeRep != null) ? judgeRep.LawUnit.FullName + ((judgeRep.CourtDepartment != null) ? " състав: " + judgeRep.CourtDepartment.Label : string.Empty) : string.Empty,
                        CaseTypeLabel = caseSession.Case.CaseType.Label,
                        CaseRegNum = caseSession.Case.RegNumber + "/" + caseSession.Case.RegDate.Year.ToString() + "г.",
                        CaseCodeLabel = caseSession.Case.CaseCode.Label + " - " + caseSession.Case.CaseCode.Code,
                        CaseRegDate = caseSession.Case.RegDate,
                        CaseEndDate = (WithFinalAct) ? ((act != null) ? act.ActDeclaredDate : null) : null,
                        CasePersons = GetStringPerson(caseSession.Case.CasePersons.Where(p => p.CaseSessionId == null && (p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderAged || p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderLegalAge)).ToList()),
                    };

                    result.Add(item);
                }
            }

            return result.AsQueryable();
        }

        /// <summary>
        /// Справка за Справка Несвършени дела към дата 
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseSprVM> CaseWithoutFinal_Select(int courtId, CaseFilterReport model)
        {
            List<CaseSprVM> result = new List<CaseSprVM>();
            List<CaseSprVM> result1 = new List<CaseSprVM>();

            if (model.DateFrom == null)
                model.DateFrom = DateTime.Now.AddYears(-100);

            if (model.DateTo == null)
                model.DateTo = NomenclatureExtensions.ForceEndDate(DateTime.Now);

            if (model.DateToSpr == null)
                model.DateToSpr = DateTime.Now;
            else
                model.DateToSpr = NomenclatureExtensions.ForceEndDate(model.DateToSpr);

            if (model.DateTo > model.DateToSpr)
                model.DateTo = NomenclatureExtensions.ForceEndDate(model.DateToSpr);

            model.DateFrom = NomenclatureExtensions.ForceStartDate(model.DateFrom);
            model.DateTo = NomenclatureExtensions.ForceEndDate(model.DateTo);

            DateTime nowDateTime = DateTime.Now;

            var resultFinish = repo.AllReadonly<SessionResultGrouping>()
                                   .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseWithoutFinalAct_Result)
                                   .Select(x => x.SessionResultId)
                                   .ToList();

            Expression<Func<Case, bool>> caseStateWhere = x => true;
            if ((model.CaseStateId ?? 0) > 0)
                caseStateWhere = x => x.CaseStateId == model.CaseStateId;

            var caseCases = repo.AllReadonly<Case>()
                                .Include(x => x.CaseGroup)
                                .Include(x => x.CaseType)
                                .Include(x => x.CaseCode)
                                .Include(x => x.CaseLawUnits)
                                .ThenInclude(x => x.LawUnit)
                                .Include(x => x.CaseLawUnits)
                                .ThenInclude(x => x.JudgeRole)
                                .Include(x => x.CaseLawUnits)
                                .ThenInclude(x => x.CourtDepartment)
                                .Include(x => x.CaseSessions)
                                .ThenInclude(x => x.CaseSessionResults)
                                .Include(x => x.CaseState)
                                .Where(x => (x.CourtId == courtId) &&
                                            ((x.RegDate >= model.DateFrom) && (x.RegDate <= (model.DateTo ?? nowDateTime.AddYears(100)))) &&
                                            (x.CaseStateId != NomenclatureConstants.CaseState.Draft) &&
                                            ((model.CaseGroupId > 0) ? x.CaseGroupId == model.CaseGroupId : true) &&
                                            ((model.CaseTypeId > 0) ? x.CaseTypeId == model.CaseTypeId : true) &&
                                            ((model.CaseCodeId > 0) ? x.CaseCodeId == model.CaseCodeId : true) &&
                                            ((!x.CaseSessions.Where(s => s.DateExpired == null).Any()) ||
                                             (!x.CaseSessions.Any(a => a.DateExpired == null && a.CaseSessionResults.Any(r => r.DateExpired == null && resultFinish.Contains(r.SessionResultId)))) ||
                                             (!x.CaseSessions.Any(a => a.DateExpired == null && a.CaseSessionActs.Any(b => b.ActDeclaredDate <= model.DateToSpr && b.IsFinalDoc && (b.ActStateId != NomenclatureConstants.SessionActState.Project && b.ActStateId != NomenclatureConstants.SessionActState.Registered))))) &&
                                            ((model.JudgeReporterId > 0) ? (x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                                                                                                      (a.DateTo ?? nowDateTime.AddYears(100)) >= (model.DateToSpr ?? nowDateTime) && a.LawUnitId == model.JudgeReporterId &&
                                                                                                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any()) : true))
                                .Where(caseStateWhere)
                                .Where(x => !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                                .ToList();

            foreach (var caseCase in caseCases)
            {
                var judgeRep = caseCase.CaseLawUnits.Where(x => x.CaseSessionId == null && (((x.DateTo ?? DateTime.Now.AddYears(100)).Date >= (model.DateToSpr ?? DateTime.Now).Date) && (x.DateFrom.Date <= (model.DateToSpr ?? DateTime.Now).Date)) && x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).OrderByDescending(a => a.DateFrom).FirstOrDefault();
                var caseLifecyclesText = string.Empty;
                var durationMonths = caseLifecycleService.GetCalcMonthToDate(caseCase.Id, model.DateToSpr ?? DateTime.Now);
                var cseMigration = repo.AllReadonly<CaseMigration>().Where(x => x.CaseId == caseCase.Id).FirstOrDefault();
                if (cseMigration != null)
                {
                    var cseMigrations = repo.AllReadonly<CaseMigration>()
                                            .Include(x => x.Case)
                                            .Where(x => x.InitialCaseId == cseMigration.InitialCaseId &&
                                                        x.CaseId < caseCase.Id &&
                                                        x.CourtId == caseCase.CourtId).ToList() ?? new List<CaseMigration>();

                    foreach (var caseMigration in cseMigrations)
                    {
                        var caseMigrationLifecycles = caseLifecycleService.CaseLifecycle_Select(caseCase.Id).ToList();
                        durationMonths += caseMigrationLifecycles.Where(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress).Sum(x => x.DurationMonths);
                        caseLifecyclesText += caseMigration.Case.RegNumber + "/" + caseMigration.Case.RegDate.ToString("dd.MM.yyyy") + "; ";
                    }
                }

                caseLifecyclesText += durationMonths;

                var act = repo.AllReadonly<CaseSessionAct>()
                              .Where(x => x.CaseId == caseCase.Id &&
                                          x.IsFinalDoc)
                              .FirstOrDefault();

                if (!result.Any(x => x.Id == caseCase.Id))
                {
                    var item = new CaseSprVM()
                    {
                        Id = caseCase.Id,
                        JudgeReport = (judgeRep != null) ? judgeRep.LawUnit.FullName + ((judgeRep.CourtDepartment != null) ? " състав: " + judgeRep.CourtDepartment.Label : string.Empty) : string.Empty,
                        CaseTypeLabel = caseCase.CaseType.Label,
                        CaseRegNum = caseCase.RegNumber + "/" + caseCase.RegDate.Year.ToString() + "г.",
                        CaseCodeLabel = caseCase.CaseCode.Label + " - " + caseCase.CaseCode.Code,
                        CaseRegDate = caseCase.RegDate,
                        CaseEndDate = ((act != null) && (caseCase.CaseSessions.Any(s => s.CaseSessionResults.Where(a => a.DateExpired == null && resultFinish.Contains(a.SessionResultId)).Any()))) ? (act.ActDeclaredDate > model.DateToSpr ? act.ActDeclaredDate : null) : null,
                        CaseLifeCycle = caseLifecyclesText,
                        CaseStateName = caseCase.CaseState.Label,
                    };

                    result.Add(item);
                }
            }

            return result.AsQueryable();
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

            model.DateFrom = NomenclatureExtensions.ForceStartDate(model.DateFrom);
            model.DateTo = NomenclatureExtensions.ForceEndDate(model.DateTo);

            var caseMigrations = repo.AllReadonly<CaseMigration>()
                                     .Include(x => x.CaseMigrationType)
                                     .Include(x => x.OutDocument)
                                     .ThenInclude(x => x.DocumentType)
                                     .Include(x => x.Case)
                                     .ThenInclude(x => x.CaseGroup)
                                     .Include(x => x.User)
                                     .ThenInclude(x => x.LawUnit)
                                     .Where(x => (x.CourtId == courtId) &&
                                                 ((x.OutDocument.DocumentDate >= model.DateFrom) && (x.OutDocument.DocumentDate <= model.DateTo)) &&
                                                 ((model.CaseGroupId > 0) ? x.Case.CaseGroupId == model.CaseGroupId : true) &&
                                                 ((model.CaseTypeId > 0) ? x.Case.CaseTypeId == model.CaseTypeId : true) &&
                                                 ((model.DocumentGroupId > 0) ? x.OutDocument.DocumentGroupId == model.DocumentGroupId : true))
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

            var caseMovements = repo.AllReadonly<CaseMovement>()
                                    .Where(x => (x.CourtId == courtId) &&
                                                ((x.DateSend >= model.DateFrom) && (x.DateSend <= model.DateTo)) &&
                                                ((model.CaseGroupId > 0) ? x.Case.CaseGroupId == model.CaseGroupId : true) &&
                                                ((model.CaseTypeId > 0) ? x.Case.CaseTypeId == model.CaseTypeId : true))
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

            var caseEvidenceMovements = repo.AllReadonly<CaseEvidenceMovement>()
                                            .Where(x => (x.CourtId == courtId) &&
                                                        ((x.MovementDate >= model.DateFrom) && (x.MovementDate <= model.DateTo)) &&
                                                        ((model.CaseGroupId > 0) ? x.Case.CaseGroupId == model.CaseGroupId : true) &&
                                                        ((model.CaseTypeId > 0) ? x.Case.CaseTypeId == model.CaseTypeId : true))
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
        /// Справка за Справка за срочност за насрочване на дела
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseSprVM> CaseBeginReport_Select(int courtId, CaseFilterReport model)
        {
            model.DateFrom = NomenclatureExtensions.ForceStartDate(model.DateFrom);
            model.DateTo = NomenclatureExtensions.ForceEndDate(model.DateTo);

            return repo.AllReadonly<Case>()
                       .Where(x => (x.CourtId == courtId) &&
                                   ((x.RegDate >= model.DateFrom) && (x.RegDate <= (model.DateTo ?? DateTime.Now.AddYears(100)))) &&
                                   (x.CaseStateId != NomenclatureConstants.CaseState.Draft) &&
                                   ((model.SessionDateToId == NomenclatureConstants.SessionToDateValue.SessionTo1MonthValue) ? (x.CaseSessions.Where(s => s.DateExpired == null && s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession).OrderBy(s => s.DateFrom).Select(s => s.DateFrom).DefaultIfEmpty(DateTime.Now.AddYears(100)).FirstOrDefault() <= x.RegDate.AddDays(30)) : true) &&
                                   ((model.SessionDateToId == NomenclatureConstants.SessionToDateValue.SessionTo2MonthValue) ? (x.CaseSessions.Where(s => s.DateExpired == null && s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession).OrderBy(s => s.DateFrom).Select(s => s.DateFrom).DefaultIfEmpty(DateTime.Now.AddYears(100)).FirstOrDefault() >= x.RegDate.AddDays(31) && x.CaseSessions.Where(s => s.DateExpired == null && s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession).OrderBy(s => s.DateFrom).Select(s => s.DateFrom).DefaultIfEmpty(DateTime.Now.AddYears(100)).FirstOrDefault() <= x.RegDate.AddDays(60)) : true) &&
                                   ((model.SessionDateToId == NomenclatureConstants.SessionToDateValue.SessionTo3MonthValue) ? (x.CaseSessions.Where(s => s.DateExpired == null && s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession).OrderBy(s => s.DateFrom).Select(s => s.DateFrom).DefaultIfEmpty(DateTime.Now.AddYears(100)).FirstOrDefault() >= x.RegDate.AddDays(61) && x.CaseSessions.Where(s => s.DateExpired == null && s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession).OrderBy(s => s.DateFrom).Select(s => s.DateFrom).DefaultIfEmpty(DateTime.Now.AddYears(100)).FirstOrDefault() <= x.RegDate.AddDays(90)) : true) &&
                                   ((model.SessionDateToId == NomenclatureConstants.SessionToDateValue.SessionUp3MonthValue) ? (x.CaseSessions.Where(s => s.DateExpired == null && s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession).OrderBy(s => s.DateFrom).Select(s => s.DateFrom).DefaultIfEmpty(x.RegDate).FirstOrDefault() >= x.RegDate.AddDays(91)) : true) &&
                                   ((model.SessionDateToId == NomenclatureConstants.SessionToDateValue.NoSessionValue) ? (!x.CaseSessions.Any(s => s.DateExpired == null && s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession)) : true) &&
                                   ((model.CaseGroupId > 0) ? x.CaseGroupId == model.CaseGroupId : true) &&
                                   ((model.CaseTypeId > 0) ? x.CaseTypeId == model.CaseTypeId : true) &&
                                   ((model.CaseCodeId > 0) ? x.CaseCodeId == model.CaseCodeId : true) &&
                                   ((model.SessionTypeId > 0) ? (x.CaseSessions.Any(s => s.DateExpired == (DateTime?)null && s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession) ? (x.CaseSessions.Where(s => s.DateExpired == null && s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession).OrderBy(s => s.DateFrom).FirstOrDefault()).SessionTypeId == model.SessionTypeId : false) : true) &&
                                   ((model.JudgeReporterId > 0) ? (x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                                                                                             (a.DateTo ?? DateTime.Now.AddYears(100)).Date >= x.RegDate.Date && a.LawUnitId == model.JudgeReporterId &&
                                                                                             a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any()) : true))
                       .Where(x => !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                       .Select(x => new CaseSprVM()
                       {
                           Id = x.Id,
                           CaseTypeLabel = x.CaseType.Label,
                           CaseRegNum = x.RegNumber + "/" + x.RegDate.Year.ToString() + "г.",
                           CaseCodeLabel = x.CaseCode.Code + " " + x.CaseCode.Label,
                           DocumentDate = x.Document.DocumentDate,
                           CaseRegDate = x.RegDate,
                           JudgeReport = x.CaseLawUnits.Where(l => l.CaseSessionId == null &&
                                                                   l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                                   (l.DateTo ?? DateTime.Now.AddYears(100)) >= DateTime.Now)
                                                       .Select(l => l.LawUnit.FullName + ((l.CourtDepartment != null) ? " състав: " + l.CourtDepartment.Label : string.Empty))
                                                       .FirstOrDefault(),
                           SessionTypeLabel = x.CaseSessions.Where(s => s.DateExpired == null && s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession).OrderBy(s => s.DateFrom).FirstOrDefault().SessionType.Label,
                           SessionDateFrom = x.CaseSessions.Where(s => s.DateExpired == null && s.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession).OrderBy(s => s.DateFrom).FirstOrDefault().DateFrom,
                           ActFinalInfo = repo.AllReadonly<CaseSessionAct>().Where(a => a.CaseId == x.Id && a.ActStateId != NomenclatureConstants.SessionActState.Project && a.DateExpired == null && a.IsFinalDoc).Select(a => a.ActType.Label + " " + a.RegNumber + "/" + (a.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy")).FirstOrDefault()
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
            model.DateFrom = NomenclatureExtensions.ForceStartDate(model.DateFrom);
            model.DateTo = NomenclatureExtensions.ForceEndDate(model.DateTo);
            model.Session_DateFrom = NomenclatureExtensions.ForceStartDate(model.Session_DateFrom);
            model.SessionDateTo = NomenclatureExtensions.ForceEndDate(model.SessionDateTo);
            model.ActDateFrom = NomenclatureExtensions.ForceStartDate(model.ActDateFrom);
            model.ActDateTo = NomenclatureExtensions.ForceEndDate(model.ActDateTo);

            Expression<Func<CaseSessionAct, bool>> finalActSearch = x => true;
            if (model.ActIsFinalDoc == true)
                finalActSearch = x => x.IsFinalDoc == true && NomenclatureConstants.SessionActState.EnforcedStates.Contains(x.ActStateId);

            return repo.AllReadonly<CaseSessionAct>()
                       .Where(x => (x.CourtId == courtId) &&
                                   (x.DateExpired == null) &&
                                   (x.ActStateId != NomenclatureConstants.SessionActState.Project) &&
                                   ((x.Case.RegDate >= model.DateFrom) && (x.Case.RegDate <= (model.DateTo ?? DateTime.Now.AddYears(100)))) &&
                                   ((x.RegDate >= model.ActDateFrom) && (x.RegDate <= (model.ActDateTo ?? DateTime.Now.AddYears(100)))) &&
                                   ((x.CaseSession.DateFrom >= model.Session_DateFrom) && (x.CaseSession.DateFrom <= (model.SessionDateTo ?? DateTime.Now.AddYears(100)))) &&
                                   (x.Case.CaseStateId != NomenclatureConstants.CaseState.Draft) &&
                                   ((model.ActDateToId == NomenclatureConstants.ActToDateValue.ActDateTo1MonthValue) ? (x.ActDeclaredDate >= x.CaseSession.DateFrom && x.ActDeclaredDate <= x.CaseSession.DateFrom.AddDays(30)) : true) &&
                                   ((model.ActDateToId == NomenclatureConstants.ActToDateValue.ActDateTo2MonthValue) ? (x.ActDeclaredDate >= x.CaseSession.DateFrom.AddDays(31) && x.ActDeclaredDate <= x.CaseSession.DateFrom.AddDays(60)) : true) &&
                                   ((model.ActDateToId == NomenclatureConstants.ActToDateValue.ActDateTo3MonthValue) ? (x.ActDeclaredDate >= x.CaseSession.DateFrom.AddDays(61) && x.ActDeclaredDate <= x.CaseSession.DateFrom.AddDays(90)) : true) &&
                                   ((model.ActDateToId == NomenclatureConstants.ActToDateValue.ActDateToUp3MonthValue) ? (x.ActDeclaredDate >= x.CaseSession.DateFrom.AddDays(91)) : true) &&
                                   ((model.CaseGroupId > 0) ? x.Case.CaseGroupId == model.CaseGroupId : true) &&
                                   ((model.CaseTypeId > 0) ? x.Case.CaseTypeId == model.CaseTypeId : true) &&
                                   ((model.CaseCodeId > 0) ? x.Case.CaseCodeId == model.CaseCodeId : true) &&
                                   ((model.SessionTypeId > 0) ? x.Case.CaseSessions.Where(s => s.DateExpired == null).OrderBy(s => s.DateFrom).FirstOrDefault().SessionTypeId == model.SessionTypeId : true) &&
                                   (model.SessionResultId > 0 ? x.CaseSession.CaseSessionResults.Any(r => r.SessionResultId == model.SessionResultId && r.DateExpired == null) : true))
                       .Where(finalActSearch)
                       .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                       .Select(x => new CaseSprVM()
                       {
                           Id = x.CaseId ?? 0,
                           CaseTypeLabel = x.Case.CaseType.Label,
                           CaseRegNum = x.Case.RegNumber + "/" + x.Case.RegDate.Year.ToString() + "г.",
                           CaseCodeLabel = x.Case.CaseCode.Code + " " + x.Case.CaseCode.Label,
                           SessionDateFrom = x.CaseSession.DateFrom,
                           SessionResult = string.Join(",", x.CaseSession.CaseSessionResults.Where(r => r.DateExpired == null).Select(r => r.SessionResult.Label + (r.SessionResultBaseId != null ? " - " + r.SessionResultBase.Label : string.Empty))),
                           JudgeReport = x.CaseSession.CaseLawUnits.Where(l => l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                                               (l.DateTo ?? DateTime.Now.AddYears(100)) >= x.CaseSession.DateFrom)
                                                                   .Select(l => l.LawUnit.FullName + ((l.CourtDepartment != null) ? " състав: " + l.CourtDepartment.Label : string.Empty))
                                                                   .FirstOrDefault(),
                           ActTypeLabel = x.ActType.Label,
                           SessionActDate = x.ActDate,
                           ActReturnDate = x.ActReturnDate
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Справка за Справка за времетраене на размяната на книжата (първи интервал)
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseSprVM> CaseFirstLifecyclie_Select(int courtId, CaseFilterReport model)
        {
            model.DateFrom = NomenclatureExtensions.ForceStartDate(model.DateFrom);
            model.DateTo = NomenclatureExtensions.ForceEndDate(model.DateTo);
            model.Session_DateFrom = NomenclatureExtensions.ForceStartDate(model.Session_DateFrom);
            model.SessionDateTo = NomenclatureExtensions.ForceEndDate(model.SessionDateTo);

            var temp = repo.AllReadonly<CaseSession>()
                       .Include(x => x.Case)
                       .ThenInclude(x => x.CaseLifecycles)
                       .Where(x => (x.CourtId == courtId) &&
                                   ((x.DateFrom >= model.Session_DateFrom) && (x.DateFrom <= (model.SessionDateTo ?? DateTime.Now.AddYears(100)))) &&
                                   ((x.Case.RegDate >= model.DateFrom) && (x.Case.RegDate <= (model.DateTo ?? DateTime.Now.AddYears(100)))) &&
                                   (x.Case.CaseStateId != NomenclatureConstants.CaseState.Draft) &&
                                   ((model.CaseGroupId > 0) ? x.Case.CaseGroupId == model.CaseGroupId : x.Case.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo) &&
                                   ((model.CaseTypeId > 0) ? x.Case.CaseTypeId == model.CaseTypeId : true) &&
                                   ((model.CaseCodeId > 0) ? x.Case.CaseCodeId == model.CaseCodeId : true) &&
                                   ((model.ProcessPriorityId > 0) ? x.Case.ProcessPriorityId == model.ProcessPriorityId : true) &&
                                   (x.SessionTypeId == NomenclatureConstants.SessionType.ClosedSession || x.SessionTypeId == NomenclatureConstants.SessionType.DispositionalSession) &&
                                   (x.CaseSessionResults.Any(r => r.SessionResultId == NomenclatureConstants.CaseSessionResult.ScheduledFirstSession)) &&
                                   ((model.IsDoubleExchangeDoc) ? x.Case.CaseClassifications.Any(c => c.ClassificationId == NomenclatureConstants.CaseClassifications.DoubleExchangeDoc) :
                                                                  !x.Case.CaseClassifications.Any(c => c.ClassificationId == NomenclatureConstants.CaseClassifications.DoubleExchangeDoc)) &&
                                   (x.Case.CaseTypeId != NomenclatureConstants.CaseTypes.VGD && x.Case.CaseTypeId != NomenclatureConstants.CaseTypes.VChGD && x.Case.CaseCodeId != NomenclatureConstants.CaseCode.AdmissionOfAdoption_0107_1))
                       .ToList();

            string res = string.Empty;
            foreach (var item in temp)
            {
                var test3 = item.Case.CaseLifecycles.Where(l => l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                    ((l.DateFrom <= item.DateFrom) && ((l.DateTo ?? DateTime.Now.AddYears(100)) >= item.DateTo))).Select(l => l.Iteration).FirstOr(0);

                var test4 = item.Case.CaseLifecycles.Where(l => l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                    ((l.DateFrom <= item.DateFrom) && ((l.DateTo ?? DateTime.Now.AddYears(100)) >= item.DateTo)));

                if ((item.Case.CaseLifecycles.Where(l => l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                    ((l.DateFrom <= item.DateFrom) && ((l.DateTo ?? DateTime.Now.AddYears(100)) >= item.DateTo))).Select(l => l.Iteration).FirstOr(0) < 2))
                {
                    var test1 = item.Case.CaseLifecycles.Where(l => l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                        ((l.DateFrom <= item.DateFrom) && ((l.DateTo ?? DateTime.Now.AddYears(100)) >= item.DateTo))).Select(l => l.Iteration).FirstOr(0);

                    var test2 = item.Case.CaseLifecycles.Where(l => l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                        ((l.DateFrom <= item.DateFrom) && ((l.DateTo ?? DateTime.Now.AddYears(100)) >= item.DateTo)));
                    if ((item.Case.CaseLifecycles.Where(l => l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                        ((l.DateFrom <= item.DateFrom) && ((l.DateTo ?? DateTime.Now.AddYears(100)) >= item.DateTo))).Select(l => l.Iteration).FirstOr(0) == 1))
                    {
                        res = caseLifecycleService.CalcMonthFromDateFromDateTo(item.Case.RegDate, item.DateFrom).ToString();
                    }
                    else
                        res = "0";

                }
                else
                {
                    res = (caseLifecycleService.CalcMonthFromDateFromDateTo(item.Case.CaseLifecycles.Where(l => l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                    ((l.DateFrom <= item.DateFrom) && ((l.DateTo ?? DateTime.Now.AddYears(100)) >= item.DateTo))).Select(l => l.DateFrom).FirstOr(DateTime.Now), item.DateFrom).ToString());
                }
            };

            return repo.AllReadonly<CaseSession>()
                       .Include(x => x.Case)
                       .ThenInclude(x => x.CaseLifecycles)
                       .Where(x => (x.CourtId == courtId) &&
                                   ((x.DateFrom >= model.Session_DateFrom) && (x.DateFrom <= (model.SessionDateTo ?? DateTime.Now.AddYears(100)))) &&
                                   ((x.Case.RegDate >= model.DateFrom) && (x.Case.RegDate <= (model.DateTo ?? DateTime.Now.AddYears(100)))) &&
                                   (x.Case.CaseStateId != NomenclatureConstants.CaseState.Draft) &&
                                   ((model.CaseGroupId > 0) ? x.Case.CaseGroupId == model.CaseGroupId : x.Case.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo) &&
                                   ((model.CaseTypeId > 0) ? x.Case.CaseTypeId == model.CaseTypeId : true) &&
                                   ((model.CaseCodeId > 0) ? x.Case.CaseCodeId == model.CaseCodeId : true) &&
                                   ((model.ProcessPriorityId > 0) ? x.Case.ProcessPriorityId == model.ProcessPriorityId : true) &&
                                   (x.SessionTypeId == NomenclatureConstants.SessionType.ClosedSession || x.SessionTypeId == NomenclatureConstants.SessionType.DispositionalSession) &&
                                   (x.CaseSessionResults.Any(r => r.SessionResultId == NomenclatureConstants.CaseSessionResult.ScheduledFirstSession)) &&
                                   ((model.IsDoubleExchangeDoc) ? x.Case.CaseClassifications.Any(c => c.ClassificationId == NomenclatureConstants.CaseClassifications.DoubleExchangeDoc) :
                                                                  !x.Case.CaseClassifications.Any(c => c.ClassificationId == NomenclatureConstants.CaseClassifications.DoubleExchangeDoc)) &&
                                   (x.Case.CaseTypeId != NomenclatureConstants.CaseTypes.VGD && x.Case.CaseTypeId != NomenclatureConstants.CaseTypes.VChGD && x.Case.CaseCodeId != NomenclatureConstants.CaseCode.AdmissionOfAdoption_0107_1))
                       .Select(x => new CaseSprVM()
                       {
                           Id = x.CaseId,
                           CaseTypeLabel = x.Case.CaseType.Label,
                           CaseRegNum = x.Case.RegNumber + "/" + x.Case.RegDate.Year.ToString() + "г.",
                           CaseCodeLabel = x.Case.CaseCode.Code + " " + x.Case.CaseCode.Label,
                           CaseRegDate = x.Case.RegDate,
                           SessionDateFrom = x.DateFrom,
                           LifecycleInfo = (x.Case.CaseLifecycles.Where(l => l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                             ((l.DateFrom <= x.DateFrom) && ((l.DateTo ?? DateTime.Now.AddYears(100)) >= x.DateTo))).Select(l => l.Iteration).FirstOr(0) < 2) ?
                                                                             ((x.Case.CaseLifecycles.Where(l => l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                                                               ((l.DateFrom <= x.DateFrom) && ((l.DateTo ?? DateTime.Now.AddYears(100)) >= x.DateTo))).Select(l => l.Iteration).FirstOr(0) == 1) ?
                                                                                                               (caseLifecycleService.CalcMonthFromDateFromDateTo(x.Case.RegDate, x.DateFrom).ToString()) : "0") : (caseLifecycleService.CalcMonthFromDateFromDateTo(x.Case.CaseLifecycles.Where(l => l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                                                                                                                                                                                                       ((l.DateFrom <= x.DateFrom) && ((l.DateTo ?? DateTime.Now.AddYears(100)) >= x.DateTo))).Select(l => l.DateFrom).FirstOr(DateTime.Now), x.DateFrom).ToString())
                       })
                       .AsQueryable();
        }

        public bool IsRegisterCompany(int caseId)
        {
            var documentTypeId = repo.AllReadonly<Case>()
                                 .Where(x => x.Id == caseId)
                                 .Select(x => x.Document.DocumentTypeId)
                                 .FirstOrDefault();

            return repo.AllReadonly<DocumentTypeGrouping>()
                                 .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.RegisterCompany)
                                 .Where(x => x.DocumentTypeId == documentTypeId)
                                 .Any();
        }

        public bool IsCaseRestricted(int CaseId)
        {
            return repo.AllReadonly<CaseClassification>()
                       .Where(x => x.CaseId == CaseId)
                       .Any(x => NomenclatureConstants.CaseClassifications.RestictedAccess.Contains(x.ClassificationId));
        }
    }
}
