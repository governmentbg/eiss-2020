using IOWebApplication.Core.Contracts;
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
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class CaseSessionActComplainService : BaseService, ICaseSessionActComplainService
    {
        private readonly ICasePersonService casePersonService;
        private readonly ICaseLawUnitService caseLawUnitService;
        private readonly ICaseLifecycleService caseLifecycleService;
        private readonly ICaseMigrationService caseMigrationService;


        public CaseSessionActComplainService(ILogger<CaseSessionActComplainService> _logger,
                                             IRepository _repo,
                                             AutoMapper.IMapper _mapper,
                                             IUserContext _userContext,
                                             ICasePersonService _casePersonService,
                                             ICaseLawUnitService _caseLawUnitService,
                                             ICaseLifecycleService _caseLifecycleService,
                                             ICaseMigrationService _caseMigrationService)
        {
            logger = _logger;
            repo = _repo;
            mapper = _mapper;
            userContext = _userContext;
            casePersonService = _casePersonService;
            caseLawUnitService = _caseLawUnitService;
            caseLifecycleService = _caseLifecycleService;
            caseMigrationService = _caseMigrationService;
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
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="DateFromActReturn"></param>
        /// <param name="DateToActReturn"></param>
        /// <param name="DateFromSendDocument"></param>
        /// <param name="DateToSendDocument"></param>
        /// <param name="CaseGroupId"></param>
        /// <param name="CaseTypeId"></param>
        /// <param name="CaseRegNumber"></param>
        /// <param name="ActRegNumber"></param>
        /// <param name="CaseRegNumFrom"></param>
        /// <param name="CaseRegNumTo"></param>
        /// <param name="ActComplainIndexId"></param>
        /// <param name="ActResultId"></param>
        /// <param name="JudgeReporterId"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionActComplainSprVM> CaseSessionActComplainSpr_Select(DateTime DateFrom, DateTime DateTo, DateTime? DateFromActReturn, DateTime? DateToActReturn, DateTime? DateFromSendDocument, DateTime? DateToSendDocument, int CaseGroupId, int CaseTypeId, string CaseRegNumber, string ActRegNumber, int CaseRegNumFrom, int CaseRegNumTo, int ActComplainIndexId, int ActResultId, int JudgeReporterId)
        {
            DateFrom = NomenclatureExtensions.ForceStartDate(DateFrom);
            DateTo = NomenclatureExtensions.ForceEndDate(DateTo);
            DateFromActReturn = NomenclatureExtensions.ForceStartDate(DateFromActReturn);
            DateToActReturn = NomenclatureExtensions.ForceEndDate(DateToActReturn);
            DateFromSendDocument = NomenclatureExtensions.ForceStartDate(DateFromSendDocument);
            DateToSendDocument = NomenclatureExtensions.ForceEndDate(DateToSendDocument);

            var caseSessionActComplains = repo.AllReadonly<CaseSessionActComplain>()
                                              .Include(x => x.Case)
                                              .ThenInclude(x => x.CaseGroup)
                                              .Include(x => x.Case)
                                              .ThenInclude(x => x.CaseType)
                                              .Include(x => x.Case)
                                              .ThenInclude(x => x.CaseCode)
                                              .Include(x => x.ComplainDocument)
                                              .ThenInclude(x => x.DocumentType)
                                              .Include(x => x.ComplainState)
                                              .Include(x => x.CaseSessionAct)
                                              .ThenInclude(x => x.ActComplainIndex)
                                              .Include(x => x.CaseSessionAct)
                                              .ThenInclude(x => x.CaseSession)
                                              .ThenInclude(x => x.CaseLawUnits)
                                              .Where(x => (x.CourtId == userContext.CourtId) &&
                                                          (x.DateExpired == null) &&
                                                          (x.ComplainDocument.DocumentDate >= DateFrom && x.ComplainDocument.DocumentDate <= DateTo) &&
                                                          ((DateFromActReturn != null && DateToActReturn != null) ? x.CaseSessionAct.ActReturnDate >= DateFromActReturn && x.CaseSessionAct.ActReturnDate <= DateToActReturn : true) &&
                                                          ((DateFromSendDocument != null && DateToSendDocument != null) ? (repo.AllReadonly<CaseMigration>().Any(m => m.CaseId == x.CaseId &&
                                                                                                                                                                      m.CaseSessionActId == x.CaseSessionActId && 
                                                                                                                                                                      m.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendNextLevel &&
                                                                                                                                                                      (m.OutDocument.DocumentDate >= DateFromSendDocument && m.OutDocument.DocumentDate <= DateToSendDocument))) : true) &&
                                                          (CaseGroupId > 0 ? x.Case.CaseGroupId == CaseGroupId : true) &&
                                                          (CaseTypeId > 0 ? x.Case.CaseTypeId == CaseTypeId : true) &&
                                                          (CaseRegNumFrom > 0 ? x.Case.ShortNumberValue >= CaseRegNumFrom : true) &&
                                                          (CaseRegNumTo > 0 ? x.Case.ShortNumberValue <= CaseRegNumTo : true) &&
                                                          (ActComplainIndexId > 0 ? x.CaseSessionAct.ActComplainIndexId == ActComplainIndexId : true) &&
                                                          (ActResultId > 0 ? x.CaseSessionAct.ActResultId == ActResultId : true) &&
                                                          ((JudgeReporterId > 0) ? (x.CaseSessionAct.CaseSession.CaseLawUnits.Where(a => (a.DateTo ?? DateTime.Now.AddYears(100)).Date >= x.CaseSessionAct.CaseSession.DateFrom && a.LawUnitId == JudgeReporterId &&
                                                                                                                                          a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any()) : true) &&
                                                          (!string.IsNullOrEmpty(CaseRegNumber) ? x.Case.RegNumber.ToLower().Contains(CaseRegNumber.ToLower()) : true) &&
                                                          (!string.IsNullOrEmpty(ActRegNumber) ? x.CaseSessionAct.RegNumber.ToLower().Contains(ActRegNumber.ToLower()) : true))
                                              .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                                              .ToList();

            var result = new List<CaseSessionActComplainSprVM>();

            foreach (var caseSessionActComplain in caseSessionActComplains)
            {
                var migrationSend = repo.AllReadonly<CaseMigration>()
                                        .Include(x => x.OutDocument)
                                        .ThenInclude(x => x.DocumentType)
                                        .Where(x => x.CaseId == caseSessionActComplain.CaseId &&
                                                    x.CaseSessionActId == caseSessionActComplain.CaseSessionActId &&
                                                    x.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendNextLevel).FirstOrDefault();

                CaseMigration migrationRecive = null;
                if (migrationSend != null)
                {
                    migrationRecive = repo.AllReadonly<CaseMigration>()
                                              .Include(x => x.Case)
                                              .ThenInclude(x => x.Court)
                                              .Where(x => x.OutCaseMigrationId == migrationSend.Id)
                                              .FirstOrDefault();
                }

                //var migration = repo.AllReadonly<CaseMigration>()
                //                    .Where(x => x.CaseId == caseSessionActComplain.CaseId &&
                //                                x.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptCase_AfterComplain &&
                //                                x.DateWrt >= caseSessionActComplain.ComplainDocument.DocumentDate).FirstOrDefault();

                var complainResult  = repo.AllReadonly<CaseSessionActComplainResult>()
                                          .Include(x => x.ActResult)
                                          .Include(x => x.ComplainCase)
                                          .ThenInclude(x => x.Court)
                                          .Where(x => x.CaseSessionActComplainId == caseSessionActComplain.Id).FirstOrDefault();

                var caseLawUnitsActive = caseLawUnitService.CaseLawUnit_Select(caseSessionActComplain.CaseId ?? 0, null).ToList();
                var judgeRep = caseLawUnitsActive.Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).FirstOrDefault();
                var sessionActComplainVM = new CaseSessionActComplainSprVM()
                {
                    Id = caseSessionActComplain.Id,
                    JudgeName = (judgeRep != null) ? judgeRep.LawUnitName + ((!string.IsNullOrEmpty(judgeRep.DepartmentLabel)) ? " състав: " + judgeRep.DepartmentLabel : string.Empty) : string.Empty,
                    //ComplainDocumentNumber = caseSessionActComplain.ComplainDocument.DocumentNumber,
                    //ComplainDocumentDate = caseSessionActComplain.ComplainDocument.DocumentDate,
                    //ComplainDocumentType = caseSessionActComplain.ComplainDocument.DocumentType.Label,
                    ComplainDocumentNumber = migrationSend != null ? (migrationSend.OutDocument != null ? migrationSend.OutDocument.DocumentNumber : string.Empty) : string.Empty,
                    ComplainDocumentDate = migrationSend != null ? (migrationSend.OutDocument != null ? (DateTime?)migrationSend.OutDocument.DocumentDate : null) : null,
                    ComplainDocumentType = migrationSend != null ? (migrationSend.OutDocument != null ? migrationSend.OutDocument.DocumentType.Label : string.Empty) : string.Empty,
                    ActName = caseSessionActComplain.CaseSessionAct.RegNumber + "/" + (caseSessionActComplain.CaseSessionAct.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"),
                    CaseGroupLabel = caseSessionActComplain.Case.CaseType.Label + " " + caseSessionActComplain.Case.CaseCode.Code,
                    CaseNumber = caseSessionActComplain.Case.RegNumber + "/" + caseSessionActComplain.Case.RegDate.Year + "г.",
                    CaseId = caseSessionActComplain.Case.Id,
                    DateReturn = caseSessionActComplain.CaseSessionAct.ActReturnDate, /*migration?.DateWrt,*/
                    Result = (complainResult != null) ? complainResult.ActResult.Label : string.Empty,
                    Instance = (migrationRecive != null) ? migrationRecive.Case.Court.Label : string.Empty,
                    IndexLabel = (caseSessionActComplain.CaseSessionAct.ActComplainIndex != null) ? caseSessionActComplain.CaseSessionAct.ActComplainIndex.Code + " - " + caseSessionActComplain.CaseSessionAct.ActComplainIndex.Label : string.Empty
                };

                result.Add(sessionActComplainVM);
            }

            return result.AsQueryable();
        }

        /// <summary>
        /// Запис на Обжалвания към съдебен акт
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseSessionActComplain_SaveData(CaseSessionActComplain model)
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
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    repo.Add<CaseSessionActComplain>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Обжалвания към съдебен акт Id={ model.Id }");
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
                        var casePersons = casePersonService.CasePerson_Select(caseSessionAct.CaseId ?? 0, null, false, false, false).Where(x => x.CaseSessionId == null).ToList();

                        //caseSessionAct.CanAppeal = true;
                        //caseSessionAct.DateWrt = DateTime.Now;
                        //caseSessionAct.UserId = userContext.UserId;
                        //repo.Update(caseSessionAct);

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
                        repo.Add(caseSessionActComplain);

                        foreach (var documentPerson in document.DocumentPersons)
                        {
                            var casePerson = casePersons.Where(x => x.Uic == documentPerson.Uic).FirstOrDefault();

                            if (casePerson != null)
                            {
                                var caseSessionActComplainPerson = new CaseSessionActComplainPerson()
                                {
                                    CourtId = caseSessionAct.CourtId,
                                    CaseId = caseSessionAct.CaseId,
                                    CaseSessionActComplainId = caseSessionActComplain.Id,
                                    CasePersonId = casePerson.Id,
                                    UserId = userContext.UserId,
                                    DateWrt = DateTime.Now
                                };

                                repo.Add(caseSessionActComplainPerson);
                            }
                        }

                        repo.SaveChanges();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Обжалвания към съдебен акт Id={ DocumentId }");
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
        /// <param name="caseSessionActId"></param>
        /// <param name="htmlTemplateId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownListForAct(int caseId, int caseSessionActId, int htmlTemplateId, bool addDefaultElement = true, bool addAllElement = false)
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
                caseSessionActComplain = caseSessionActComplain.Where(x => x.CaseSessionActId == caseSessionActId);
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
                       .Include(x => x.CaseSessionAct)
                       .ThenInclude(x => x.Case)
                       .ThenInclude(x => x.Court)
                       .Include(x => x.CaseSessionAct)
                       .ThenInclude(x => x.ActType)
                       .Include(x => x.ActResult)
                       .Where(x => x.CaseSessionActComplainId == CaseSessionActComplainId)
                       .Select(x => new CaseSessionActComplainResultVM()
                       {
                           Id = x.Id,
                           CaseName = x.CaseSessionAct.Case.Court.Label + " - " + x.CaseSessionAct.Case.RegNumber + "/" + x.CaseSessionAct.Case.RegDate.ToString("dd.MM.yyyy"),
                           CaseSessionActName = x.CaseSessionAct.ActType.Label + " " + x.CaseSessionAct.RegNumber + "/" + (x.CaseSessionAct.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"),
                           CaseSessionActId = x.CaseSessionActId,
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
                       .Include(x => x.ComplainCase)
                       .ThenInclude(x => x.Court)
                       .Include(x => x.CaseSessionAct)
                       .ThenInclude(x => x.ActType)
                       .Include(x => x.ActResult)
                       .Include(x => x.CaseSessionActComplain)
                       .ThenInclude(x => x.CaseSessionAct)
                       .ThenInclude(x => x.CaseSession)
                       .Where(x => x.CaseSessionActComplain.CaseSessionAct.CaseSession.CaseId == CaseId)
                       .Select(x => new CaseSessionActComplainResultVM()
                       {
                           Id = x.Id,
                           CaseName = x.ComplainCase.Court.Label + " - " + x.ComplainCase.RegNumber + "/" + x.ComplainCase.RegDate.ToString("dd.MM.yyyy"),
                           CaseSessionActName = x.CaseSessionAct.ActType.Label + " " + x.CaseSessionAct.RegNumber + "/" + (x.CaseSessionAct.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"),
                           CaseSessionActId = x.CaseSessionActId,
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
                DateResult = model.DateResult
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
                DateResult = model.DateResult
            };
        }

        /// <summary>
        /// Извличане на данни за резултат от обжалване по ид
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public CaseSessionActComplainResultEditVM CaseSessionActComplainResult_GetById(int Id)
        {
            return FillCaseSessionActComplainResultEditVM(repo.GetById<CaseSessionActComplainResult>(Id));
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
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    repo.Update(saved);
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
                        caseSessionActComplainSave.DateWrt = DateTime.Now;
                        caseSessionActComplainSave.UserId = userContext.UserId;
                        repo.Add<CaseSessionActComplainResult>(caseSessionActComplainSave);
                    }
                }

                repo.SaveChanges();
                if (model.Id < 1)
                    model.Id = modelSave.Id;

                if (model.IsStartNewLifecycle)
                {
                    var caseSessionAct = repo.GetById<CaseSessionAct>(model.ActResultId);
                    var dateAccept = caseMigrationService.GetDateTimeAcceptCaseAfterComplain(model.CaseId, caseSessionAct.CaseId ?? 0);
                    caseLifecycleService.CaseLifecycle_NewIntervalSave(model.CaseId, dateAccept ?? DateTime.Now);
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Резултат по Обжалвания към съдебен акт Id={ model.Id }");
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
                logger.LogError(ex, $"Грешка при запис на Страни в Обжалвания към съдебен акт Id={ model.ObjectId }");
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
                if (result != string.Empty)
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

            var casePersons = casePersonService.CasePerson_Select(caseId, null, false, false, false).Where(x => x.CaseSessionId == null).ToList();
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
