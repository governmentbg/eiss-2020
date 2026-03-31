using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class CaseSessionActComplainService : BaseService, ICaseSessionActComplainService
    {
        private readonly ICasePersonService casePersonService;
        private readonly ICaseLawUnitService caseLawUnitService;
        private readonly ICaseLifecycleService caseLifecycleService;
        private readonly ICaseMigrationService caseMigrationService;
        private readonly IMQEpepService epepService;


        public CaseSessionActComplainService(ILogger<CaseSessionActComplainService> _logger,
                                             IRepository _repo,
                                             IUserContext _userContext,
                                             ICasePersonService _casePersonService,
                                             ICaseLawUnitService _caseLawUnitService,
                                             ICaseLifecycleService _caseLifecycleService,
                                             ICaseMigrationService _caseMigrationService,
                                             IReadonlyRepository _readonlyrepo,
                                             IMQEpepService epepService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            casePersonService = _casePersonService;
            caseLawUnitService = _caseLawUnitService;
            caseLifecycleService = _caseLifecycleService;
            caseMigrationService = _caseMigrationService;
            readonlyrepo = _readonlyrepo;
            this.epepService = epepService;
        }

        #region CaseSessionActComplain

        /// <summary>
        /// Извличане на данни за Обжалвания към съдебен акт
        /// </summary>
        /// <param name="CaseSessionActId"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionActComplainVM> CaseSessionActComplain_Select(int CaseSessionActId)
        {
            var caseSessionActComplains = repo.AllReadonly<CaseSessionActComplain>()
                                              .Include(x => x.CasePersons)
                                              .ThenInclude(x => x.CasePerson)
                                              .Include(x => x.ComplainDocument)
                                              .ThenInclude(x => x.DocumentType)
                                              .Include(x => x.ComplainState)
                                              .Where(x => x.CaseSessionActId == CaseSessionActId &&
                                                          x.DateExpired == null)
                                              .ToList();

            var result = new List<CaseSessionActComplainVM>();

            foreach (var caseSessionActComplain in caseSessionActComplains)
            {
                var sessionActComplainVM = new CaseSessionActComplainVM()
                {
                    Id = caseSessionActComplain.Id,
                    ComplainDocumentName = caseSessionActComplain.ComplainDocument.DocumentType.Label + " " + caseSessionActComplain.ComplainDocument.DocumentNumber + "/" + caseSessionActComplain.ComplainDocument.DocumentDate.ToString("dd.MM.yyyy"),
                    ComplainStateLabel = caseSessionActComplain.ComplainState.Label,
                    CasePersonName = GetListPersonName(caseSessionActComplain.CasePersons)
                };

                result.Add(sessionActComplainVM);
            }

            return result.AsQueryable();
        }

        /// <summary>
        /// Проверка за обжалване по съпровождащ документ
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="ComplainDocumentId"></param>
        /// <returns></returns>
        public bool IsExistComplain(int CaseId, long ComplainDocumentId)
        {
            return repo.AllReadonly<CaseSessionActComplain>()
                       .Any(x => x.CaseId == CaseId &&
                                 x.ComplainDocumentId == ComplainDocumentId &&
                                 x.DateExpired == null);
        }


        /// <summary>
        /// Проверка за обжалване по документ със различен статус от постъпил
        /// </summary>
        /// <param name="DocumentId"></param>
        /// <returns></returns>
        public bool IsExistComplainByDocumentIdDifferentStatusRecived(long DocumentId)
        {
            return repo.AllReadonly<CaseSessionActComplain>()
                       .Any(x => x.ComplainDocumentId == DocumentId &&
                                 x.DateExpired == null &&
                                 x.ComplainStateId != NomenclatureConstants.ComplainState.Recived);
        }

        /// <summary>
        /// Извличане на данни за справка за обжалване
        /// </summary>
        /// <param name="filter">Филтър</param>
        /// <returns></returns>
        public IQueryable<CaseSessionActComplainSprVM> CaseSessionActComplainSpr_Select(CaseSessionActComplainFilterVM filter)
        {
            filter.DateFrom = NomenclatureExtensions.ForceStartDate(filter.DateFrom);
            filter.DateTo = NomenclatureExtensions.ForceEndDate(filter.DateTo);
            filter.DateFromActReturn = NomenclatureExtensions.ForceStartDate(filter.DateFromActReturn);
            filter.DateToActReturn = NomenclatureExtensions.ForceEndDate(filter.DateToActReturn);
            filter.DateFromSendDocument = NomenclatureExtensions.ForceStartDate(filter.DateFromSendDocument);
            filter.DateToSendDocument = NomenclatureExtensions.ForceEndDate(filter.DateToSendDocument);

            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateNow = DateTime.Now;

            Expression<Func<CaseSessionActComplain, bool>> caseGroupIdWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupIdWhere = x => x.Case.CaseGroupId == filter.CaseGroupId;

            Expression<Func<CaseSessionActComplain, bool>> caseTypeIdWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeIdWhere = x => x.Case.CaseTypeId == filter.CaseTypeId;

            Expression<Func<CaseSessionActComplain, bool>> actReturnWhere = x => true;
            if (filter.DateFromActReturn != null && filter.DateToActReturn != null)
                actReturnWhere = x => x.CaseSessionAct.ActReturnDate >= filter.DateFromActReturn && x.CaseSessionAct.ActReturnDate <= filter.DateToActReturn;

            Expression<Func<CaseSessionActComplain, bool>> documentDateWhere = x => true;
            if (filter.DateFrom != null && filter.DateTo != null)
                documentDateWhere = x => x.ComplainDocument.DocumentDate >= filter.DateFrom && x.ComplainDocument.DocumentDate <= filter.DateTo;

            Expression<Func<CaseSessionActComplain, bool>> sendDocumentWhere = x => true;
            if (filter.DateFromSendDocument != null && filter.DateToSendDocument != null)
                sendDocumentWhere = x => readonlyrepo.AllReadonly<CaseMigration>().Any(m => m.CaseId == x.CaseId &&
                                                                                    m.CaseSessionActId == x.CaseSessionActId &&
                                                                                    m.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendNextLevel &&
                                                                                    (m.OutDocument.DocumentDate >= filter.DateFromSendDocument && m.OutDocument.DocumentDate <= filter.DateToSendDocument));

            Expression<Func<CaseSessionActComplain, bool>> caseRegNumFromWhere = x => true;
            if (filter.CaseRegNumFrom > 0)
                caseRegNumFromWhere = x => x.Case.ShortNumberValue >= filter.CaseRegNumFrom;

            Expression<Func<CaseSessionActComplain, bool>> caseRegNumToWhere = x => true;
            if (filter.CaseRegNumTo > 0)
                caseRegNumToWhere = x => x.Case.ShortNumberValue <= filter.CaseRegNumTo;

            Expression<Func<CaseSessionActComplain, bool>> actComplainIndexIdWhere = x => true;
            if (filter.ActComplainIndexId > 0)
                actComplainIndexIdWhere = x => x.CaseSessionAct.ActComplainIndexId == filter.ActComplainIndexId;

            Expression<Func<CaseSessionActComplain, bool>> actResultIdWhere = x => true;
            if (filter.ActResultId > 0)
                actResultIdWhere = x => x.ComplainResults.Any(r => r.ActResultId == filter.ActResultId);

            Expression<Func<CaseSessionActComplain, bool>> judgeReporterIdWhere = x => true;
            if (filter.JudgeReporterId > 0)
                judgeReporterIdWhere = x => x.CaseSessionAct.CaseSession.CaseLawUnits.Where(a => (a.DateTo ?? DateTime.Now.AddYears(100)).Date >= x.CaseSessionAct.CaseSession.DateFrom && a.LawUnitId == filter.JudgeReporterId &&
                                                                                                 a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            Expression<Func<CaseSessionActComplain, bool>> judgeReporterFinalActIdWhere = x => true;
            if (filter.JudgeReporterFinalActId > 0)
                judgeReporterFinalActIdWhere = x => x.CaseSessionAct
                                                     .CaseSession
                                                     .CaseLawUnits
                                                     .Any(a => (a.DateTo ?? dateEnd) >= dateNow && a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter && a.LawUnitId == filter.JudgeReporterFinalActId);

            Expression<Func<CaseSessionActComplain, bool>> caseRegnumberSearch = x => true;
            if (!string.IsNullOrEmpty(filter.CaseRegNumber))
                caseRegnumberSearch = x => EF.Functions.ILike(x.Case.RegNumber, filter.CaseRegNumber.ToCasePaternSearch());

            Expression<Func<CaseSessionActComplain, bool>> actRegNumberSearch = x => true;
            if (!string.IsNullOrEmpty(filter.ActRegNumber))
                actRegNumberSearch = x => EF.Functions.ILike(x.CaseSessionAct.RegNumber, filter.ActRegNumber.ToEndingPaternSearch());

            Expression<Func<CaseSessionActComplain, bool>> actIsFinalDocSearch = x => true;
            if (filter.ActIsFinalDoc)
                actIsFinalDocSearch = x => x.CaseSessionAct.IsFinalDoc;

            var caseLawUnit = readonlyrepo.AllReadonly<CaseLawUnit>()
                                  .Where(l => l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                              (l.DateTo ?? dateNow.AddYears(1)) >= dateNow &&
                                              l.CaseSessionId == null);

            var migrationSend = readonlyrepo.AllReadonly<CaseMigration>()
                                    .Where(m => (m.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendNextLevel) ||
                                                (m.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendCase_ToOtherSystem &&
                                                 m.OutDocument.DocumentTypeId == NomenclatureConstants.DocumentType.LetterOfTransmittalForAppeal));

            var migrationRecive = readonlyrepo.AllReadonly<CaseMigration>();

            return readonlyrepo.AllReadonly<CaseSessionActComplain>()
                       .Where(x => x.CourtId == userContext.CourtId && x.DateExpired == null)
                       .Where(documentDateWhere)
                       .Where(caseGroupIdWhere)
                       .Where(caseTypeIdWhere)
                       .Where(actReturnWhere)
                       .Where(sendDocumentWhere)
                       .Where(caseRegNumFromWhere)
                       .Where(caseRegNumToWhere)
                       .Where(actComplainIndexIdWhere)
                       .Where(actResultIdWhere)
                       .Where(judgeReporterIdWhere)
                       .Where(judgeReporterFinalActIdWhere)
                       .Where(caseRegnumberSearch)
                       .Where(actRegNumberSearch)
                       .Where(actIsFinalDocSearch)
                       .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                       .Select(x => new CaseSessionActComplainSprVM()
                       {
                           Id = x.Id,
                           JudgeName = caseLawUnit.Where(l => l.CaseId == x.CaseId)
                                                  .Select(l => l.LawUnit.FullName + ((l.CourtDepartmentId != null) ? " състав: " + l.CourtDepartment.Label : string.Empty))
                                                  .FirstOrDefault(),
                           JudgeReporterFinalActName = x.CaseSessionAct
                                                        .CaseSession
                                                        .CaseLawUnits
                                                        .Where(a => (a.DateTo ?? dateEnd) >= dateNow && a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                        .Select(a => a.LawUnit.FullName)
                                                        .FirstOrDefault(),
                           ComplainDocumentNumber = migrationSend.Where(m => m.CaseId == x.CaseId &&
                                                                             m.CaseSessionActId == x.CaseSessionActId)
                                                                 .OrderByDescending(m => m.Id)
                                                                 .Select(m => m.OutDocumentId != null ? m.OutDocument.DocumentNumber : string.Empty)
                                                                 .FirstOrDefault(),
                           ComplainDocumentDate = migrationSend.Where(m => m.CaseId == x.CaseId &&
                                                                           m.CaseSessionActId == x.CaseSessionActId)
                                                               .OrderByDescending(m => m.Id)
                                                               .Select(m => m.OutDocumentId != null ? (DateTime?)m.OutDocument.DocumentDate : null)
                                                               .FirstOrDefault(),
                           ComplainDocumentType = migrationSend.Where(m => m.CaseId == x.CaseId &&
                                                                           m.CaseSessionActId == x.CaseSessionActId)
                                                               .OrderByDescending(m => m.Id)
                                                               .Select(m => m.OutDocumentId != null ? m.OutDocument.DocumentType.Label : null)
                                                               .FirstOrDefault(),
                           ActName = x.CaseSessionAct.ActType.Label + " " + x.CaseSessionAct.RegNumber + "/" + (x.CaseSessionAct.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") + " ",
                           ActDate = x.CaseSessionAct.RegDate,
                           CaseGroupLabel = x.Case.CaseType.Code + " " + x.Case.CaseCode.Code,
                           CaseNumber = x.Case.ShortNumber + "/" + x.Case.RegDate.Year + "г.",
                           CaseId = x.CaseId ?? 0,
                           DateReturn = x.CaseSessionAct.ActReturnDate,
                           Result = x.ComplainResults.Select(r => r.ActResult.Label).FirstOrDefault(),
                           Instance = migrationRecive.Where(mr => mr.OutCaseMigrationId == migrationSend.Where(m => m.CaseId == x.CaseId &&
                                                                                                                    m.CaseSessionActId == x.CaseSessionActId)
                                                                                                        .OrderByDescending(m => m.Id)
                                                                                                        .Select(m => m.Id)
                                                                                                        .FirstOrDefault())
                                                     .Select(mr => mr.Case.Court.Label)
                                                     .FirstOrDefault(),
                           IndexLabel = (x.CaseSessionAct.ActComplainIndex != null) ? x.CaseSessionAct.ActComplainIndex.Code + " - " + x.CaseSessionAct.ActComplainIndex.Label : string.Empty
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Запис на Обжалвания към съдебен акт
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> CaseSessionActComplain_SaveData(CaseSessionActComplain model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseSessionActComplain>(model.Id);
                    saved.CourtId = model.CourtId;
                    saved.CaseId = model.CaseId;
                    saved.CaseSessionActId = model.CaseSessionActId;
                    saved.ComplainDocumentId = model.ComplainDocumentId;
                    saved.RejectDescription = model.RejectDescription;
                    saved.ComplainStateId = model.ComplainStateId;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    await repo.SaveChangesAsync();                   
                }
                else
                {
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    repo.Add<CaseSessionActComplain>(model);
                    await repo.SaveChangesAsync();                    
                }
                (bool isRNFL, bool transferStarted) = await epepService.RNFL_CheckCase(model.CaseId ?? 0);
                if (isRNFL && transferStarted)
                {
                    await epepService.RNFL_SendAppeal(model.Id, EpepConstants.ServiceMethod.Add);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Обжалвания към съдебен акт Id={model.Id}");
                return false;
            }
        }

        /// <summary>
        /// Създаване на обжалване по документ - автоматично
        /// </summary>
        /// <param name="DocumentId"></param>
        /// <returns></returns>
        public bool CaseSessionActComplain_CreateFromDocument(long DocumentId)
        {
            try
            {
                var document = repo.AllReadonly<Document>()
                                   .Include(x => x.DocumentCaseInfo)
                                   .Include(x => x.DocumentPersons)
                                   .Where(x => x.Id == DocumentId)
                                   .FirstOrDefault();

                if (document != null)
                {
                    if ((document.DocumentDirectionId == DocumentConstants.DocumentDirection.Incoming) &&
                        (document.DocumentGroupId == NomenclatureConstants.DocumentGroup.DocumentForComplain_AccompanyingDocument) &&
                        (document.DocumentCaseInfo.Any(x => x.SessionActId != null)))
                    {
                        var caseSessionAct = repo.GetById<CaseSessionAct>(document.DocumentCaseInfo.Select(x => x.SessionActId).FirstOrDefault());
                        //var casePersons = casePersonService.CasePerson_Select(caseSessionAct.CaseId ?? 0, null, false, false, false).Where(x => x.CaseSessionId == null).ToList();
                        var casePersons = casePersonService.CasePersonFast_SelectForCasePreview(caseSessionAct.CaseId ?? 0).ToList();

                        //caseSessionAct.CanAppeal = true;
                        //caseSessionAct.DateWrt = DateTime.Now;
                        //caseSessionAct.UserId = userContext.UserId;

                        var caseSessionActComplain = new CaseSessionActComplain()
                        {
                            CourtId = caseSessionAct.CourtId,
                            CaseId = caseSessionAct.CaseId,
                            CaseSessionActId = caseSessionAct.Id,
                            ComplainDocumentId = document.Id,
                            ComplainStateId = NomenclatureConstants.ComplainState.Recived,
                            UserId = userContext.UserId,
                            DateWrt = DateTime.Now
                        };


                        foreach (var documentPerson in document.DocumentPersons)
                        {
                            var casePerson = new CasePersonListVM();
                            if (!string.IsNullOrEmpty(documentPerson.Uic))
                                casePerson = casePersons.Where(x => x.Uic == documentPerson.Uic).FirstOrDefault();
                            else
                                casePerson = null;

                            if (casePerson != null)
                            {
                                var caseSessionActComplainPerson = new CaseSessionActComplainPerson()
                                {
                                    CourtId = caseSessionAct.CourtId,
                                    CaseId = caseSessionAct.CaseId,
                                    //CaseSessionActComplainId = caseSessionActComplain.Id,
                                    CasePersonId = casePerson.Id,
                                    UserId = userContext.UserId,
                                    DateWrt = DateTime.Now
                                };
                                caseSessionActComplain.CasePersons = caseSessionActComplain.CasePersons ?? new List<CaseSessionActComplainPerson>();
                                caseSessionActComplain.CasePersons.Add(caseSessionActComplainPerson);
                                //repo.Add(caseSessionActComplainPerson);
                            }
                        }

                        repo.Add(caseSessionActComplain);

                        repo.SaveChanges();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Обжалвания към съдебен акт Id={DocumentId}");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за документ по текущото дело за комбобокс
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownList_GetDocumentCaseInfo(int CaseSessionId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var caseSession = repo.GetById<CaseSession>(CaseSessionId);
            var result = repo.AllReadonly<DocumentCaseInfo>()
                             .Include(x => x.Document)
                             .ThenInclude(x => x.DocumentType)
                             .Where(x => x.CaseId == caseSession.CaseId &&
                                         x.Document.DocumentDirectionId == DocumentConstants.DocumentDirection.Incoming &&
                                         x.Document.DocumentDate >= caseSession.DateFrom && x.Document.DocumentGroupId == NomenclatureConstants.DocumentGroup.DocumentForComplain_AccompanyingDocument)
                             .Select(x => new SelectListItem()
                             {
                                 Text = x.Document.DocumentType.Label + " " + x.Document.DocumentNumber + "/" + x.Document.DocumentDate.ToString("dd.MM.yyyy"),
                                 Value = x.Document.Id.ToString()
                             }).ToList() ?? new List<SelectListItem>();

            if (result.Count > 0)
            {
                result = result.OrderBy(x => x.Text).ToList();
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
        /// Извличане на данни за акт за комбобокс
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionActIds"></param>
        /// <param name="htmlTemplateId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownListForAct(int caseId, int[] caseSessionActIds, int htmlTemplateId, bool addDefaultElement = true, bool addAllElement = false)
        {
            bool complainActFree = repo.AllReadonly<HtmlTemplate>()
                                   .FirstOrDefault(x => x.Id == htmlTemplateId)?
                                   .HaveActComplainFree ?? false;
            var caseSessionActComplain = repo.AllReadonly<CaseSessionActComplain>()
                                             .Where(x => x.DateExpired == null);


            if (complainActFree)
            {
                caseSessionActComplain = caseSessionActComplain.Where(x => x.CaseSessionAct.CaseId == caseId);
            }
            else
            {
                caseSessionActComplain = caseSessionActComplain.Where(x => caseSessionActIds.Contains(x.CaseSessionActId));
            }

            var result = caseSessionActComplain.Select(x => new SelectListItem()
            {
                Text = (x.ComplainDocument.DocumentType.Label ?? "") + " " + x.ComplainDocument.DocumentNumber + "/" + x.ComplainDocument.DocumentDate.ToString("dd.MM.yyyy"),
                Value = x.Id.ToString()
            }).ToList() ?? new List<SelectListItem>();

            if (result.Count > 0)
            {
                result = result.OrderBy(x => x.Text).ToList();
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
        /// Извличане на обжалвания за чеклист
        /// </summary>
        /// <param name="CaseSessionActComplainId"></param>
        /// <param name="CaseSessionActId"></param>
        /// <returns></returns>
        public List<CheckListVM> GetCheckListCaseSessionActComplains(int CaseSessionActComplainId, int CaseSessionActId)
        {
            return repo.AllReadonly<CaseSessionActComplain>()
                       .Include(x => x.ComplainDocument)
                       .ThenInclude(x => x.DocumentType)
                       .Include(x => x.ComplainResults)
                       .Where(x => x.CaseSessionActId == CaseSessionActId &&
                                   x.Id != CaseSessionActComplainId &&
                                   x.DateExpired == null &&
                                   !x.ComplainResults.Any(r => r.CaseSessionActComplainId == x.Id))
                       .Select(x => new CheckListVM()
                       {
                           Checked = false,
                           Value = x.Id.ToString(),
                           Label = (x.ComplainDocument.DocumentType.Label ?? "") + " " + x.ComplainDocument.DocumentNumber + "/" + x.ComplainDocument.DocumentDate.ToString("dd.MM.yyyy")
                       })
                       .ToList() ?? new List<CheckListVM>();
        }

        #endregion

        #region CaseSessionActComplainResult

        /// <summary>
        /// Извличане на данни резултат от обжалване
        /// </summary>
        /// <param name="CaseSessionActComplainId"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionActComplainResultVM> CaseSessionActComplainResult_Select(int CaseSessionActComplainId)
        {
            return repo.AllReadonly<CaseSessionActComplainResult>()
                       .Where(x => x.CaseSessionActComplainId == CaseSessionActComplainId)
                       .Select(x => new CaseSessionActComplainResultVM()
                       {
                           Id = x.Id,
                           CaseName = (x.CaseSessionActId != null ? x.CaseSessionAct.Case.Court.Label + " - " + x.CaseSessionAct.Case.RegNumber + "/" + x.CaseSessionAct.Case.RegDate.ToString("dd.MM.yyyy") : x.ComplainCourt.Label + " - дело: " + x.CaseRegNumberOtherSystem + "/" + x.CaseYearOtherSystem),
                           CaseSessionActName = (x.CaseSessionActId != null ? x.CaseSessionAct.ActType.Label + " " + x.CaseSessionAct.RegNumber + "/" + (x.CaseSessionAct.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : x.CaseSessionActOtherSystem),
                           CaseSessionActId = x.CaseSessionActId ?? 0,
                           ActResultLabel = x.ActResult.Label,
                           DateResult = x.DateResult
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Извличане на данни за резултати към обжалване по дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        private IList<CaseSessionActComplainResultVM> CaseSessionActComplainResultForCaseId_Select(int CaseId)
        {
            return repo.AllReadonly<CaseSessionActComplainResult>()
                       .Where(x => x.CaseSessionActComplain.CaseSessionAct.CaseSession.CaseId == CaseId)
                       .Select(x => new CaseSessionActComplainResultVM()
                       {
                           Id = x.Id,
                           CaseName = x.ComplainCase.Court.Label + " - " + x.ComplainCase.RegNumber + "/" + x.ComplainCase.RegDate.ToString("dd.MM.yyyy"),
                           CaseSessionActName = x.CaseSessionAct.ActType.Label + " " + x.CaseSessionAct.RegNumber + "/" + (x.CaseSessionAct.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"),
                           CaseSessionActId = x.CaseSessionActId ?? 0,
                           ActResultLabel = x.ActResult.Label,
                           DateResult = x.DateResult
                       })
                       .ToList();
        }

        /// <summary>
        /// Метод за пълнене на основен обек за резултат от обжалване
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private CaseSessionActComplainResult FillCaseSessionActComplainResult(CaseSessionActComplainResultEditVM model)
        {
            return new CaseSessionActComplainResult()
            {
                Id = model.Id,
                ComplainCourtId = model.ComplainCourtId,
                ComplainCaseId = model.ComplainCaseId,
                CaseSessionActComplainId = model.CaseSessionActComplainId,
                CourtId = model.CourtId,
                CaseId = model.CaseId,
                CaseSessionActId = model.CaseSessionActId,
                ActResultId = model.ActResultId,
                Description = model.Description,
                DateResult = model.DateResult,
                CaseSessionActOtherSystem = model.CaseSessionActOtherSystem,
                CaseYearOtherSystem = model.CaseYearOtherSystem,
                CaseShortNumberOtherSystem = model.CaseShortNumberOtherSystem,
                CaseRegNumberOtherSystem = model.CaseRegNumberOtherSystem,
                DateFromLifeCycle = model.DateFromLifeCycle
            };
        }

        /// <summary>
        /// Пълнене на обек за резултат за обжалване за редакция
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private CaseSessionActComplainResultEditVM FillCaseSessionActComplainResultEditVM(CaseSessionActComplainResult model)
        {
            return new CaseSessionActComplainResultEditVM()
            {
                Id = model.Id,
                ComplainCourtId = model.ComplainCourtId,
                ComplainCaseId = model.ComplainCaseId,
                CaseSessionActComplainId = model.CaseSessionActComplainId,
                CourtId = model.CourtId,
                CaseId = model.CaseId,
                CaseSessionActId = model.CaseSessionActId,
                ActResultId = model.ActResultId,
                Description = model.Description,
                DateResult = model.DateResult,
                CaseRegNumberOtherSystem = model.CaseRegNumberOtherSystem,
                CaseSessionActOtherSystem = model.CaseSessionActOtherSystem,
                CaseShortNumberOtherSystem = model.CaseShortNumberOtherSystem,
                CaseYearOtherSystem = model.CaseYearOtherSystem,
                CaseOtherSystem = (model.CaseSessionActId == null),
                DateFromLifeCycle = model.DateFromLifeCycle
            };
        }

        /// <summary>
        /// Извличане на данни за резултат от обжалване по ид
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public CaseSessionActComplainResultEditVM CaseSessionActComplainResult_GetById(int Id)
        {
            return repo.AllReadonly<CaseSessionActComplainResult>()
                       .Where(c => c.Id == Id)
                       .Select(x => new CaseSessionActComplainResultEditVM()
                       {
                           Id = x.Id,
                           ComplainCourtId = x.ComplainCourtId,
                           ComplainCaseId = x.ComplainCaseId,
                           CaseSessionActComplainId = x.CaseSessionActComplainId,
                           CourtId = x.CourtId,
                           CaseId = x.CaseId,
                           CaseSessionActId = x.CaseSessionActId,
                           ActResultId = x.ActResultId,
                           Description = x.Description,
                           DateResult = x.DateResult,
                           CaseRegNumberOtherSystem = x.CaseRegNumberOtherSystem,
                           CaseSessionActOtherSystem = x.CaseSessionActOtherSystem,
                           CaseShortNumberOtherSystem = x.CaseShortNumberOtherSystem,
                           CaseYearOtherSystem = x.CaseYearOtherSystem,
                           CaseOtherSystem = (x.CaseSessionActId == null),
                           DateFromLifeCycle = x.DateFromLifeCycle,
                           ActReturn = x.CaseSessionActComplain.CaseSessionAct.ActReturnDate
                       })
                       .First();
        }

        /// <summary>
        /// Запис на резултат от обжалване
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseSessionActComplainResult_SaveData(CaseSessionActComplainResultEditVM model)
        {
            try
            {
                if (!model.CaseOtherSystem)
                    model.ComplainCourtId = repo.GetById<Case>(model.ComplainCaseId).CourtId;

                var modelSave = FillCaseSessionActComplainResult(model);

                if (modelSave.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseSessionActComplainResult>(modelSave.Id);
                    saved.CaseSessionActComplainId = modelSave.CaseSessionActComplainId;
                    saved.CourtId = modelSave.CourtId;
                    saved.CaseId = modelSave.CaseId;
                    saved.ComplainCourtId = modelSave.ComplainCourtId;
                    saved.ComplainCaseId = modelSave.ComplainCaseId;
                    saved.CaseSessionActId = modelSave.CaseSessionActId;
                    saved.ActResultId = modelSave.ActResultId;
                    saved.Description = modelSave.Description;
                    saved.CaseRegNumberOtherSystem = modelSave.CaseRegNumberOtherSystem;
                    saved.CaseSessionActOtherSystem = modelSave.CaseSessionActOtherSystem;
                    saved.CaseShortNumberOtherSystem = modelSave.CaseShortNumberOtherSystem;
                    saved.CaseYearOtherSystem = modelSave.CaseYearOtherSystem;
                    saved.DateFromLifeCycle = model.DateFromLifeCycle;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                }
                else
                {
                    modelSave.DateWrt = DateTime.Now;
                    modelSave.UserId = userContext.UserId;
                    repo.Add<CaseSessionActComplainResult>(modelSave);
                }



                if (model.CaseSessionActComplains != null)
                {
                    foreach (var caseSessionActComplain in model.CaseSessionActComplains.Where(x => x.Checked))
                    {
                        var caseSessionActComplainSave = FillCaseSessionActComplainResult(model);
                        caseSessionActComplainSave.Id = 0;
                        caseSessionActComplainSave.CaseSessionActComplainId = int.Parse(caseSessionActComplain.Value);
                        caseSessionActComplainSave.DateFromLifeCycle = null;
                        caseSessionActComplainSave.DateWrt = DateTime.Now;
                        caseSessionActComplainSave.UserId = userContext.UserId;
                        repo.Add<CaseSessionActComplainResult>(caseSessionActComplainSave);

                        if (model.CaseOtherSystem)
                        {
                            int actComplainId = int.Parse(caseSessionActComplain.Value);
                            CaseSessionActComplain actComplain = repo.AllReadonly<CaseSessionActComplain>()
                                                                     .Where(x => x.Id == actComplainId)
                                                                     .First();

                            CaseSessionAct act = repo.All<CaseSessionAct>()
                                                     .Where(a => a.Id == actComplain.CaseSessionActId)
                                                     .First();

                            act.ActReturnDate = model.ActReturn;
                        }
                    }
                }

                if (model.CaseOtherSystem)
                {
                    CaseSessionActComplain actComplain = repo.AllReadonly<CaseSessionActComplain>()
                                                             .Where(x => x.Id == model.CaseSessionActComplainId)
                                                             .First();

                    CaseSessionAct act = repo.All<CaseSessionAct>()
                                             .Where(a => a.Id == actComplain.CaseSessionActId)
                                             .First();

                    act.ActReturnDate = model.ActReturn;
                }

                repo.SaveChanges();
                if (model.Id < 1)
                    model.Id = modelSave.Id;

                if (model.IsStartNewLifecycle)
                {
                    DateTime? dateAccept = null;
                    if (!model.CaseOtherSystem)
                    {
                        var caseSessionAct = repo.GetById<CaseSessionAct>(model.CaseSessionActId);
                        dateAccept = caseMigrationService.GetDateTimeAcceptCaseAfterComplain(model.CaseId, caseSessionAct.CaseId ?? 0);
                    }
                    else
                    {
                        dateAccept = model.DateFromLifeCycle;
                    }

                    caseLifecycleService.CaseLifecycle_NewIntervalSave(model.CaseId, dateAccept ?? DateTime.Now, null);
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Резултат по Обжалвания към съдебен акт Id={model.Id}");
                return false;
            }
        }

        /// <summary>
        /// Извличане на актове от резултат от обжалване за комбобокс
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownList_CaseSessionActFromCaseSessionActComplainResult(int CaseId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = CaseSessionActComplainResultForCaseId_Select(CaseId)
                         .Select(x => new SelectListItem()
                         {
                             Text = x.CaseSessionActName,
                             Value = x.CaseSessionActId.ToString()
                         }).ToList() ?? new List<SelectListItem>();

            if (result.Count > 0)
            {
                result = result.OrderBy(x => x.Text).ToList();
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
        /// Извличане на резултати от резултат от обжалване за комбобокс
        /// </summary>
        /// <param name="CaseSessionActId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownList_ActResultFromCaseSessionActComplainResult(int CaseSessionActId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var caseSessionActComplainResults = repo.AllReadonly<CaseSessionActComplainResult>()
                                                    .Include(x => x.CaseSessionActComplain)
                                                    .Include(x => x.ActResult)
                                                    .Where(x => x.CaseSessionActComplain.CaseSessionActId == CaseSessionActId)
                                                    .ToList();

            var result = new List<SelectListItem>();

            foreach (var caseSessionActComplainResult in caseSessionActComplainResults)
            {
                if (!result.Any(x => x.Value == caseSessionActComplainResult.ActResult.Id.ToString()))
                {
                    var slItem = new SelectListItem()
                    {
                        Text = caseSessionActComplainResult.ActResult.Label,
                        Value = caseSessionActComplainResult.ActResult.Id.ToString()
                    };

                    result.Add(slItem);
                }
            }

            if (result.Count > 0)
            {
                result = result.OrderBy(x => x.Text).ToList();
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

        #endregion

        #region CaseSessionActComplainPerson

        /// <summary>
        /// Извличане на данни за Страни в Обжалвания към съдебен акт
        /// </summary>
        /// <param name="CaseSessionActComplainId"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionActComplainPersonVM> CaseSessionActComplainPerson_Select(int CaseSessionActComplainId)
        {
            return repo.AllReadonly<CaseSessionActComplainPerson>()
                       .Include(x => x.CasePerson)
                       .Where(x => x.CaseSessionActComplainId == CaseSessionActComplainId)
                       .Select(x => new CaseSessionActComplainPersonVM()
                       {
                           Id = x.Id,
                           CasePersonName = x.CasePerson.FullName,
                           CasePersonId = x.CasePersonId
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Запис на Страни в Обжалвания към съдебен акт
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseSessionActComplainPerson_SaveData(CheckListViewVM model)
        {
            try
            {
                var caseSessionActComplainPeople = repo.AllReadonly<CaseSessionActComplainPerson>().Where(x => x.CaseSessionActComplainId == model.ObjectId).ToList();
                repo.DeleteRange(caseSessionActComplainPeople);
                var caseSessionActComplain = repo.GetById<CaseSessionActComplain>(model.ObjectId);

                foreach (var checkList in model.checkListVMs.Where(x => x.Checked))
                {
                    var person = new CaseSessionActComplainPerson()
                    {
                        CourtId = caseSessionActComplain.CourtId,
                        CaseId = caseSessionActComplain.CaseId,
                        CaseSessionActComplainId = model.ObjectId,
                        CasePersonId = int.Parse(checkList.Value),
                        DateWrt = DateTime.Now,
                        UserId = userContext.UserId
                    };

                    repo.Add<CaseSessionActComplainPerson>(person);
                }
                repo.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Страни в Обжалвания към съдебен акт Id={model.ObjectId}");
                return false;
            }
        }

        /// <summary>
        /// Взема имената на Страни в Обжалвания към съдебен акт
        /// </summary>
        /// <param name="caseSessionActComplainPeople"></param>
        /// <returns></returns>
        private string GetListPersonName(ICollection<CaseSessionActComplainPerson> caseSessionActComplainPeople)
        {
            var result = string.Empty;

            foreach (var actComplainPerson in caseSessionActComplainPeople)
            {
                if (!string.IsNullOrEmpty(result))
                    result += ", ";

                result += actComplainPerson.CasePerson.FullName;
            }

            return result;
        }

        /// <summary>
        /// Взема Страни в Обжалвания към съдебен акт за чеклист
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="CaseSessionActComplainId"></param>
        /// <returns></returns>
        private IList<CheckListVM> FillCheckListVMs(int caseId, int CaseSessionActComplainId)
        {
            IList<CheckListVM> checkListVMs = new List<CheckListVM>();

            var casePersons = casePersonService.CasePersonFast_SelectForCasePreview(caseId).ToList();
            var caseSessionActComplainPeople = CaseSessionActComplainPerson_Select(CaseSessionActComplainId).ToList();

            foreach (var person in casePersons.Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.Now.AddYears(100)) >= DateTime.Now))
            {
                bool check = caseSessionActComplainPeople.Any(x => x.CasePersonId == person.Id);
                checkListVMs.Add(new CheckListVM() { Checked = check, Value = person.Id.ToString(), Label = person.FullName + "(" + (person.Uic ?? "") + ") - " + person.RoleName });
            }

            return checkListVMs.OrderBy(x => x.Label).ToList();
        }

        /// <summary>
        /// Взема Страни в Обжалвания към съдебен акт за чеклист
        /// </summary>
        /// <param name="CaseSessionActComplainId"></param>
        /// <returns></returns>
        public CheckListViewVM CheckListViewVM_FillCasePerson(int CaseSessionActComplainId)
        {
            var caseSessionActComplain = repo.GetById<CaseSessionActComplain>(CaseSessionActComplainId);
            var caseSessionAct = repo.GetById<CaseSessionAct>(caseSessionActComplain.CaseSessionActId);
            var caseSession = repo.GetById<CaseSession>(caseSessionAct.CaseSessionId);

            CheckListViewVM checkListViewVM = new CheckListViewVM
            {
                CourtId = caseSession.CaseId,
                ObjectId = CaseSessionActComplainId,
                Label = "Изберете жалбоподатели",
                checkListVMs = FillCheckListVMs(caseSession.CaseId, CaseSessionActComplainId)
            };

            return checkListViewVM;
        }

        #endregion
    }
}
