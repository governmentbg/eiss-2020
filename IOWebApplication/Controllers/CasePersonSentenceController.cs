using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Contracts.Integration;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Eispp;
using IOWebApplication.Infrastructure.Models.ViewModels.Integrations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rotativa.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class CasePersonSentenceController : BaseController
    {
        private readonly ICasePersonSentenceService service;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly ICaseSessionActService caseSessionActService;
        private readonly ICasePersonService casePersonService;
        private readonly ICaseMigrationService caseMigrationService;
        private readonly IEisppService eisppService;
        private readonly ICasePersonService casePerson;
        private readonly ICaseSessionActComplainService caseSessionActComplainService;
        private readonly IPrintDocumentService printDocumentService;
        private readonly ICdnService cdnService;
        private readonly ICaseLawUnitService lawUnitService;
        private readonly ICaisBuletinService caisBuletinService;
        private readonly ICaisMapperService caisMapperService;
        private readonly IWorkTaskService taskService;
        private readonly IProxyEissService proxyService;

        public CasePersonSentenceController(ICasePersonSentenceService _service,
                                            INomenclatureService _nomService,
                                            ICommonService _commonService,
                                            ICaseSessionActService _caseSessionActService,
                                            ICasePersonService _casePersonService,
                                            ICaseMigrationService _caseMigrationService,
                                            IEisppService _eisppService,
                                            ICasePersonService _casePerson,
                                            ICaseSessionActComplainService _caseSessionActComplainService,
                                            IPrintDocumentService _printDocumentService,
                                            ICdnService _cdnService,
                                            ICaseLawUnitService _lawUnitService,
                                            ICaisBuletinService _caisBuletinService,
                                            IWorkTaskService _workTaskService,
                                            ICaisMapperService _caisMapperService,
                                            IProxyEissService _proxyService)
        {
            service = _service;
            nomService = _nomService;
            commonService = _commonService;
            caseSessionActService = _caseSessionActService;
            casePersonService = _casePersonService;
            caseMigrationService = _caseMigrationService;
            eisppService = _eisppService;
            casePerson = _casePerson;
            caseSessionActComplainService = _caseSessionActComplainService;
            printDocumentService = _printDocumentService;
            cdnService = _cdnService;
            lawUnitService = _lawUnitService;
            caisBuletinService = _caisBuletinService;
            taskService = _workTaskService;
            caisMapperService = _caisMapperService;
            proxyService = _proxyService;
        }

        /// <summary>
        /// Страница за присъди към лице
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index(int casePersonId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonSentence, null, AuditConstants.Operations.View, casePersonId))
            {
                return Redirect_Denied();
            }
            var casePerson = await casePersonService.GetByIdAsync<CasePerson>(casePersonId);
            ViewBag.casePersonId = casePersonId;
            ViewBag.casePersonName = casePerson.FullName;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(casePerson.CaseId);
            SetHelpFile(HelpFileValues.CasePerson);

            return View();
        }

        /// <summary>
        /// Извличане на данни за присъди към лице
        /// </summary>
        /// <param name="request"></param>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int casePersonId)
        {
            var data = service.CasePersonSentence_Select(casePersonId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на присъда към лице
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public async Task<IActionResult> Add(int casePersonId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonSentence, null, AuditConstants.Operations.Append, casePersonId))
            {
                return Redirect_Denied();
            }
            var casePerson = await casePersonService.GetByIdAsync<CasePerson>(casePersonId);
            await SetViewbag(casePerson.CaseId, casePerson.Id, 0);
            var model = new CasePersonSentenceEditVM()
            {
                CasePersonId = casePerson.Id,
                CaseId = casePerson.CaseId,
                CourtId = userContext.CourtId,
                DecreedCourtId = userContext.CourtId,
                IsActive = true,
                LawBases = service.FillLawBase()
            };
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на присъда към лице
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonSentence, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.CasePersonSentence_GetById(id);
            await SetViewbag(model.CaseId, model.CasePersonId, id);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Валидация преди запис за присъда към лице
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValid(CasePersonSentenceEditVM model)
        {
            if (model.CaseSessionActId < 1)
                return "Изберете акт";

            if (model.DecreedCourtId < 1)
                return "Изберете от кой е постановена присъдата";

            if (model.SentenceResultTypeId < 1)
                return "Изберете резултат от съдебното производство";

            if (model.InforcedDate != null)
            {
                var caseSessionAct = service.GetById<CaseSessionAct>(model.CaseSessionActId);

                if (caseSessionAct.ActInforcedDate == null)
                    return "Няма въведена дата на влизане в сила на акта";

                if ((model.InforcedDate ?? DateTime.Now).Date < (caseSessionAct.ActInforcedDate ?? DateTime.Now).Date)
                    return "Дата на влизане в сила на присъдатае по-малка от тази на акта";
            }

            return string.Empty;
        }

        /// <summary>
        /// запис на присъда към лице
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Edit(CasePersonSentenceEditVM model)
        {
            await SetViewbag(model.CaseId, model.CasePersonId, model.Id);

            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }

            string _isvalid = IsValid(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(Edit), model);
            }

            var currentId = model.Id;
            if (await service.CasePersonSentence_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CasePersonSentence, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(Edit), model);
        }

        private async Task SetViewbag(int caseId, int casePersonId, int ModelId)
        {
            ViewBag.DecreedCourtId_ddl = await caseMigrationService.GetDropDownList_Court(caseId);
            //ViewBag.CaseSessionActId_ddl = caseSessionActService.GetDropDownList(caseId);
            ViewBag.SentenceResultTypeId_ddl = nomService.GetDropDownList<SentenceResultType>();
            ViewBag.PunishmentActivityId_ddl = nomService.GetDropDownList<PunishmentActivity>();
            ViewBag.SentenceExecPeriodId_ddl = nomService.GetDropDownList<SentenceExecPeriod>();
            ViewBag.InforcerInstitutionId_ddl = commonService.GetDDL_Institution(NomenclatureConstants.InstitutionTypes.Attourney);
            ViewBag.ChangedCasePersonSentenceId_ddl = service.GetDropDownList_CasePersonSentence(casePersonId, ModelId);
            //ViewBag.ChangeCaseSessionActId_ddl = caseSessionActComplainService.GetDropDownList_CaseSessionActFromCaseSessionActComplainResult(caseId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonSentence(casePersonId);

            SetHelpFile(HelpFileValues.CasePerson);
        }

        [HttpGet]
        public IActionResult GetDDL_CaseSessionActFromMigration(int courtId, int caseId)
        {
            var model = caseSessionActService.GetDDL_CaseSessionActFromMigration(caseId, courtId);
            return Json(model);
        }

        [HttpPost]
        public JsonResult Get_ActDescription(int ActId)
        {
            var caseSessionAct = service.GetById<CaseSessionAct>(ActId);
            return Json(new { result = ((caseSessionAct != null) ? caseSessionAct.Description : string.Empty) });
        }

        /// <summary>
        /// Списък с престъпления към дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public async Task<IActionResult> IndexCaseCrime(int caseId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseCrime, null, AuditConstants.Operations.View, caseId))
            {
                return Redirect_Denied();
            }
            ViewBag.caseId = caseId;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(caseId);
            SetHelpFile(HelpFileValues.CasePersonSentence);
            return View();
        }

        /// <summary>
        /// Извличане на данни за престъпления към дело
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseCrime(IDataTablesRequest request, int caseId)
        {
            var data = service.CaseCrime_Select(caseId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на престъпления към дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddCaseCrime(int caseId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseCrime, null, AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }
           
            await SetViewbagCaseCrime(caseId);
            var model = new CaseCrime()
            {
                CaseId = caseId,
                CourtId = userContext.CourtId,
                DateFrom = DateTime.Now
            };
            return View(nameof(EditCaseCrime), model);
        }

        /// <summary>
        /// Редакция на престъпления към дело
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditCaseCrime(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseCrime, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = await service.GetByIdAsync<CaseCrime>(id);
            await SetViewbagCaseCrime(model.CaseId);
            return View(nameof(EditCaseCrime), model);
        }

        /// <summary>
        /// Валидация преди запис на престъпления към дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private void ValidateCaseCrime(CaseCrime model)
        {
            if (string.IsNullOrEmpty(model.EISSPNumber) && model.Id == 0)
            {
                var caseModel = service.GetById<Case>(model.CaseId);
                if (model.IsGeneratedEisppNumber != true && !eisppService.IsForEisppNum(caseModel))
                    ModelState.AddModelError("EISSPNumber", "Въведете код по ЕИСПП");
            }

            if (string.IsNullOrEmpty(model.CrimeCode) || model.CrimeCode == "0" || model.CrimeCode == "-1")
                ModelState.AddModelError("CrimeCode", "Изберете категория деяние");

            if (model.Id <= 0)
            {
                if (service.IsEISPPNumberExists(model.CaseId, model.EISSPNumber))
                {
                    var message = $"{model.EISSPNumber} на престъпление вече е добавен към делото";
                    model.EISSPNumber = string.Empty;
                    ModelState.Clear();
                    ModelState.AddModelError("EISSPNumber", message);
                    
                }
            }

            if ((model.CategoryCommonDeedId == null) || (model.CategoryCommonDeedId < 1))
                ModelState.AddModelError("CategoryCommonDeedId", $"Изберете обща категория");

            if (string.IsNullOrEmpty(model.DescriptionOffence))
                ModelState.AddModelError("DescriptionOffence", $"Въведете описание на деянието");

            if ((model.CrimeSceneCountryId == null) || (model.CrimeSceneCountryId < 1))
                ModelState.AddModelError("CrimeSceneCountryId", $"Изберете място на престъплението - държава");

            //if (!string.IsNullOrEmpty(model.DescriptionOffence))
            //{`
            //    if (model.DescriptionOffence.Length > 255)
            //        ModelState.AddModelError("DescriptionOffence", "Описание на деянието трябва да е по-малко от 255 символа");
            //}

            if (!string.IsNullOrEmpty(model.LegalQualificationText))
            {
                if (model.LegalQualificationText.Length > 255)
                    ModelState.AddModelError("LegalQualificationText", "Правна квалификация трябва да е по-малко от 255 символа");
            }

            if (!string.IsNullOrEmpty(model.CrimeSceneText))
            {
                if (model.CrimeSceneText.Length > 255)
                    ModelState.AddModelError("CrimeSceneText", "Описание трябва да е по-малко от 255 символа");
            }
        }

        /// <summary>
        /// запис на престъпления към дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditCaseCrime(CaseCrime model)
        {
            await SetViewbagCaseCrime(model.CaseId);
            var caseModel = await service.GetByIdAsync<Case>(model.CaseId);
            ValidateCaseCrime(model);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditCaseCrime), model);
            }

            var currentId = model.Id;
            if (await service.CaseCrime_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseCrime, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditCaseCrime), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditCaseCrime), model);
        }

        [HttpPost]
        public async Task<IActionResult> CaseCrime_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseCrime, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }

            var expireObject = await service.GetByIdAsync<CaseCrime>(model.Id);

            if (eisppService.HaveEventForCrime(model.Id))
            {
                return Json(new { result = false, message = "Може да премахнете престъпление, само ако не е изпратено към ЕИСПП." });
            }

            if (service.SaveExpireInfo<CaseCrime>(model))
            {
                SetAuditContextDelete(service, SourceTypeSelectVM.CaseCrime, model.Id);
                SetSuccessMessage(MessageConstant.Values.CaseCrimeExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("IndexCaseCrime", "CasePersonSentence", new { caseId = expireObject.CaseId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        private async Task SetViewbagCaseCrime(int caseId)
        {
            //ViewBag.CrimeCode_ddl = еISPPService.GetDDL_EISPPTblElement(EISPPConstants.EisppTableCode.EISS_PNE);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseCrime(caseId);
            ViewBag.StartDateTypeDDL = await eisppService.GetDDL_EISPPTblElementAsync("1632");
            ViewBag.CrimeStatusDDL = await eisppService.GetDDL_EISPPTblElementAsync("206");
            ViewBag.CompletitionDegreeDDL = await eisppService.GetDDL_EISPPTblElementAsync("207");
            ViewBag.CategoryDeedId_ddl = await nomService.GetDropDownListAsync<CategoryDeed>();
            ViewBag.CategoryCommonDeedId_ddl = await nomService.GetDropDownListAsync<CategoryCommonDeed>();
            ViewBag.CrimeSceneCountryId_ddl = await nomService.GetDDL_Countries(true);
            ViewBag.FormGuiltId_ddl = nomService.GetDDL_FormGuilt();
            ViewBag.CrimeSceneLocalization_ddl = await eisppService.GetDDL_EISPPTblElementAsync(EISPPConstants.EisppTableCode.Localization);
            var caseModel = await service.GetByIdAsync<Case>(caseId);
            ViewBag.GenerateNumber = false;
            if (eisppService.IsForEisppNum(caseModel))
            {
                ViewBag.GenerateNumber = true;
            }
            if (eisppService.CanGenerateEisppNumForCrime(caseModel))
            {
                ViewBag.GenerateNumber = null;
            }
            SetHelpFile(HelpFileValues.CasePersonSentence);
        }

        /// <summary>
        /// Страница с хора към присъда
        /// </summary>
        /// <param name="caseCrimeId"></param>
        /// <returns></returns>
        public async Task<IActionResult> IndexCasePersonCrime(int caseCrimeId)
        {
            var caseCrime = service.CaseCrime_GetById(caseCrimeId);
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonCrime, null, AuditConstants.Operations.View, caseCrimeId))
            {
                return Redirect_Denied();
            }
            ViewBag.caseCrimeId = caseCrimeId;
            ViewBag.valueEISSPNumber = caseCrime.ValueEISSPNumber;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseCrime(caseCrime.CaseId);
            SetHelpFile(HelpFileValues.CasePersonSentence);

            return View();
        }

        /// <summary>
        /// Извличане на данни за хора към присъда
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseCrimeId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCasePersonCrime(IDataTablesRequest request, int caseCrimeId)
        {
            var data = service.CasePersonCrime_Select(caseCrimeId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на хора към присъда
        /// </summary>
        /// <param name="caseCrimeId"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddCasePersonCrime(int caseCrimeId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonCrime, null, AuditConstants.Operations.Append, caseCrimeId))
            {
                return Redirect_Denied();
            }
            var caseCrime = await service.GetByIdAsync<CaseCrime>(caseCrimeId);
            SetViewbagCasePersonCrime(caseCrime.CaseId, caseCrimeId);
            var model = new CasePersonCrime()
            {
                CaseId = caseCrime.CaseId,
                CourtId = userContext.CourtId,
                CaseCrimeId = caseCrimeId,
            };
            return View(nameof(EditCasePersonCrime), model);
        }

        /// <summary>
        /// Редакция на хора към присъда
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditCasePersonCrime(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonCrime, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = await service.GetByIdAsync<CasePersonCrime>(id);
            SetViewbagCasePersonCrime(model.CaseId, model.CaseCrimeId);
            return View(nameof(EditCasePersonCrime), model);
        }

        /// <summary>
        /// Валидация преди запис на хора към присъда
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidCasePersonCrime(CasePersonCrime model)
        {
            if (model.CasePersonId < 1)
                return "Изберете лице";

            if (model.RecidiveTypeId < 1)
                return "Изберете рецидив";

            if (model.Id < 1)
            {
                if (service.IsExistPersonCasePersonCrime(model.CaseCrimeId, model.CasePersonId))
                    return "Това лице е добавено вече";
            }

            return string.Empty;
        }

        /// <summary>
        /// запис на хора към присъда
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditCasePersonCrime(CasePersonCrime model)
        {
            SetViewbagCasePersonCrime(model.CaseId, model.CaseCrimeId);

            if (!ModelState.IsValid)
            {
                return View(nameof(EditCasePersonCrime), model);
            }

            string _isvalid = IsValidCasePersonCrime(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditCasePersonCrime), model);
            }

            var currentId = model.Id;
            if (service.CasePersonCrime_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CasePersonCrime, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditCasePersonCrime), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditCasePersonCrime), model);
        }

        void SetViewbagCasePersonCrime(int caseId, int caseCrimeId)
        {
            //ViewBag.CasePersonId_ddl = casePerson.GetDropDownList(caseId, null, false, 0, 0, false);
            ViewBag.CasePersonId_ddl = casePerson.CasePerson_SelectForDropDownList(caseId, null, string.Empty, string.Empty, DateTime.Now, false);
            ViewBag.RecidiveTypeId_ddl = nomService.GetDropDownList<RecidiveType>();
            ViewBag.PersonRoleInCrimeId_ddl = nomService.GetDropDownList<Infrastructure.Data.Models.Nomenclatures.PersonRoleInCrime>();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonCrime(caseCrimeId);
            SetHelpFile(HelpFileValues.CasePersonSentence);
        }

        [HttpPost]
        public async Task<IActionResult> CasePersonCrime_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonCrime, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }

            var expireObject = await service.GetByIdAsync<CasePersonCrime>(model.Id);
            if (service.SaveExpireInfo<CasePersonCrime>(model))
            {
                SetAuditContextDelete(service, SourceTypeSelectVM.CasePersonCrime, model.Id);
                SetSuccessMessage(MessageConstant.Values.CaseSessionExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("IndexCasePersonCrime", "CasePersonSentence", new { caseCrimeId = expireObject.CaseCrimeId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        /// <summary>
        /// Страница с наказания на лица към присъда
        /// </summary>
        /// <param name="casePersonSentenceId"></param>
        /// <returns></returns>
        public async Task<IActionResult> IndexCasePersonSentencePunishment(int casePersonSentenceId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonSentencePunishment, null, AuditConstants.Operations.View, casePersonSentenceId))
            {
                return Redirect_Denied();
            }
            var casePersonSentence = service.CasePersonSentence_GetById(casePersonSentenceId);
            ViewBag.casePersonSentenceId = casePersonSentenceId;
            ViewBag.casePersonSentenceName = casePersonSentence.CasePersonName;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonSentence(casePersonSentence.CasePersonId);
            SetHelpFile(HelpFileValues.CasePerson);

            return View();
        }

        /// <summary>
        /// Извличане на данни за наказания на лица към присъда
        /// </summary>
        /// <param name="request"></param>
        /// <param name="casePersonSentenceId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCasePersonSentencePunishment(IDataTablesRequest request, int casePersonSentenceId)
        {
            var data = service.CasePersonSentencePunishment_Select(casePersonSentenceId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на наказания на лица към присъда
        /// </summary>
        /// <param name="casePersonSentenceId"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddCasePersonSentencePunishment(int casePersonSentenceId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonSentencePunishment, null, AuditConstants.Operations.Append, casePersonSentenceId))
            {
                return Redirect_Denied();
            }
            var casePersonSentence = await service.GetByIdAsync<CasePersonSentence>(casePersonSentenceId);
            SetViewbagCasePersonSentencePunishment(casePersonSentenceId);
            var model = new CasePersonSentencePunishment()
            {
                CasePersonSentenceId = casePersonSentenceId,
                CaseId = casePersonSentence.CaseId,
                CourtId = userContext.CourtId,
                IsSummaryPunishment = false,
                DateFrom = DateTime.Now,
                IsMainPunishment = !service.IsExistMainPunishment(casePersonSentenceId, null)
            };
            return View(nameof(EditCasePersonSentencePunishment), model);
        }

        /// <summary>
        /// Редакция на наказания на лица към присъда
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditCasePersonSentencePunishment(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonSentencePunishment, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = await service.GetByIdAsync<CasePersonSentencePunishment>(id);
            SetViewbagCasePersonSentencePunishment(model.CasePersonSentenceId);
            return View(nameof(EditCasePersonSentencePunishment), model);
        }

        /// <summary>
        /// Валидация преди запис на наказания на лица към присъда
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<string> IsValidCasePersonSentencePunishment(CasePersonSentencePunishment model)
        {
            if (model.SentenceTypeId == null || model.SentenceTypeId < 1)
                return "Изберете наложено наказание.";

            if (model.PunishmentGeneralCategoryId == null || model.PunishmentGeneralCategoryId < 1)
                return "Изберете обща категория.";

            var sentenceTypeHas = await nomService.GetSentenceTypeHas(model.SentenceTypeId ?? 0);

            if (sentenceTypeHas.HasMoney && model.SentenceTypeId == NomenclatureConstants.SentenceTypes.Fine)
            {
                if (model.SentenseMoney < (decimal)0.001)
                    return "Въведете размер в лева";
            }

            if (model.IsMainPunishment)
            {
                if (service.IsExistMainPunishment(model.CasePersonSentenceId, (model.Id == 0 ? (int?)null : model.Id)))
                {
                    return "Има наказание маркирано като основно.";
                }
            }



            //if (model.SentenseDays < 0)
            //    return "Не може да въведете отрицателен брой дни";

            return string.Empty;
        }

        /// <summary>
        /// Запис на наказания на лица към присъда
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditCasePersonSentencePunishment(CasePersonSentencePunishment model)
        {
            SetViewbagCasePersonSentencePunishment(model.CasePersonSentenceId);

            if (!ModelState.IsValid)
            {
                return View(nameof(EditCasePersonSentencePunishment), model);
            }

            string _isvalid = await IsValidCasePersonSentencePunishment(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditCasePersonSentencePunishment), model);
            }

            var currentId = model.Id;
            if (service.CasePersonSentencePunishment_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CasePersonSentencePunishment, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditCasePersonSentencePunishment), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditCasePersonSentencePunishment), model);
        }

        void SetViewbagCasePersonSentencePunishment(int casePersonSentenceId)
        {
            ViewBag.SentenceTypeId_ddl = nomService.GetDropDownList<SentenceType>();
            ViewBag.PunishmentGeneralCategoryId_ddl = nomService.GetDropDownList<PunishmentGeneralCategory>();
            ViewBag.SentenceRegimeTypeId_ddl = nomService.GetDropDownList<SentenceRegimeType>();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonSentencePunishment(casePersonSentenceId);
            ViewBag.IsViewBGN = userContext.IsInterimPeriodEuro;
            SetHelpFile(HelpFileValues.CasePerson);
        }
        [HttpPost]
        public async Task<IActionResult> CasePersonPunishment_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonSentencePunishment, model.Id, AuditConstants.Operations.Delete))
            {
                return Json(new { result = false, message = "Нямате права да изтриете наказанието" });
            }
            if (eisppService.HaveEventForPunishment(model.Id))
            {
                return Json(new { result = false, message = "Има ЕИСПП събитие за наказанието" });
            }
            if (service.SaveExpireInfo<CasePersonSentencePunishment>(model))
            {
                SetAuditContextDelete(service, SourceTypeSelectVM.CasePersonSentencePunishment, model.Id);
                SetSuccessMessage(MessageConstant.Values.CasePersonSentencePunishmentExpireOK);
                return Json(new { result = true, redirectUrl = model.ReturnUrl });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        private int Get_SentenceType_Select(int sentenceTypeId)
        {
            var sentenceType = service.GetById<SentenceType>(sentenceTypeId);

            var valResult = NomenclatureConstants.SentenceType_Select.NoChoice;

            if (sentenceType != null)
            {
                if ((sentenceType.HasPeriod ?? false) && (sentenceType.HasMoney ?? false))
                {
                    valResult = NomenclatureConstants.SentenceType_Select.AllChoice;
                }
                else
                {
                    if (sentenceType.HasPeriod ?? false)
                        valResult = NomenclatureConstants.SentenceType_Select.HasPeriod;
                    else
                    {
                        if (sentenceType.HasMoney ?? false)
                            valResult = NomenclatureConstants.SentenceType_Select.HasMoney;
                    }
                }
            }
            return valResult;
        }

        /// <summary>
        /// Метод извличащ настройките на присъда
        /// </summary>
        /// <param name="sentenceTypeId">Идентификатор на записа</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> GetSentenceTypeHas(int sentenceTypeId)
        {
            return Json(new { result = await nomService.GetSentenceTypeHas(sentenceTypeId) });
        }

        [HttpPost]
        public JsonResult Is_Period(int sentenceTypeId)
        {
            return Json(new { result = Get_SentenceType_Select(sentenceTypeId) });
        }

        [HttpPost]
        public JsonResult Is_Probation(int sentenceTypeId)
        {
            return Json(new { result = service.GetById<SentenceType>(sentenceTypeId).HasProbation ?? false });
        }

        /// <summary>
        /// Страница с Наложени наказания към присъда
        /// </summary>
        /// <param name="CasePersonSentencePunishmentId"></param>
        /// <returns></returns>
        public async Task<IActionResult> IndexCasePersonSentencePunishmentCrime(int CasePersonSentencePunishmentId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonSentencePunishmentCrime, null, AuditConstants.Operations.View, CasePersonSentencePunishmentId))
            {
                return Redirect_Denied();
            }
            var casePersonSentencePunishment = service.CasePersonSentencePunishment_GetById(CasePersonSentencePunishmentId);
            ViewBag.casePersonSentencePunishmentId = CasePersonSentencePunishmentId;
            ViewBag.casePersonSentencePunishmentName = casePersonSentencePunishment.SentenceTypeLabel;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonSentencePunishment(casePersonSentencePunishment.CasePersonSentenceId);
            SetHelpFile(HelpFileValues.CasePerson);

            return View();
        }

        /// <summary>
        /// Извличане на данни за Наложени наказания към присъда
        /// </summary>
        /// <param name="request"></param>
        /// <param name="CasePersonSentencePunishmentId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCasePersonSentencePunishmentCrime(IDataTablesRequest request, int CasePersonSentencePunishmentId)
        {
            var data = service.CasePersonSentencePunishmentCrime_Select(CasePersonSentencePunishmentId);
            return request.GetResponse(data);
        }

        public JsonResult GetDataCasePersonSentencePunishmentCrime(int CasePersonSentencePunishmentId)
        {
            var model = service.CasePersonSentencePunishmentCrime_Select(CasePersonSentencePunishmentId);
            return Json(model);
        }

        /// <summary>
        /// Добавяне на Наложени наказания към присъда
        /// </summary>
        /// <param name="CasePersonSentencePunishmentId"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddCasePersonSentencePunishmentCrime(int CasePersonSentencePunishmentId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonSentencePunishmentCrime, null, AuditConstants.Operations.Append, CasePersonSentencePunishmentId))
            {
                return Redirect_Denied();
            }
            var casePersonSentencePunishment = await service.GetByIdAsync<CasePersonSentencePunishment>(CasePersonSentencePunishmentId);
            SetViewbagCasePersonSentencePunishmentCrime(CasePersonSentencePunishmentId);
            var model = new CasePersonSentencePunishmentCrime()
            {
                CaseId = casePersonSentencePunishment.CaseId,
                CourtId = userContext.CourtId,
                CasePersonSentencePunishmentId = CasePersonSentencePunishmentId,
            };
            return View(nameof(EditCasePersonSentencePunishmentCrime), model);
        }

        /// <summary>
        /// Редакция на Наложени наказания към присъда
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditCasePersonSentencePunishmentCrime(int id)
        {
            var model = service.GetById<CasePersonSentencePunishmentCrime>(id);
            SetViewbagCasePersonSentencePunishmentCrime(model.CasePersonSentencePunishmentId);
            return View(nameof(EditCasePersonSentencePunishmentCrime), model);
        }


        /// <summary>
        /// запис на Наложени наказания към присъда
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditCasePersonSentencePunishmentCrime(CasePersonSentencePunishmentCrime model)
        {
            SetViewbagCasePersonSentencePunishmentCrime(model.CasePersonSentencePunishmentId);

            if (!ModelState.IsValid)
            {
                return View(nameof(EditCasePersonSentencePunishmentCrime), model);
            }

            var currentId = model.Id;
            if (service.CasePersonSentencePunishmentCrime_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CasePersonSentencePunishmentCrime, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditCasePersonSentencePunishmentCrime), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditCasePersonSentencePunishmentCrime), model);
        }

        public async Task<IActionResult> CasePersonSentencePunishmentCrime(int CasePersonSentencePunishmentId, int? id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonSentencePunishmentCrime, null, AuditConstants.Operations.Append, CasePersonSentencePunishmentId))
            {
                return Redirect_Denied();
            }

            CasePersonSentencePunishmentCrime model;
            if (id > 0)
            {
                model = await nomService.GetByIdAsync<CasePersonSentencePunishmentCrime>(id);
            }
            else
            {
                var casePersonSentencePunishment = await service.GetByIdAsync<CasePersonSentencePunishment>(CasePersonSentencePunishmentId);
                model = new CasePersonSentencePunishmentCrime()
                {
                    CaseId = casePersonSentencePunishment.CaseId,
                    CourtId = userContext.CourtId,
                    CasePersonSentencePunishmentId = CasePersonSentencePunishmentId,
                    SentenceTypeId = casePersonSentencePunishment.SentenceTypeId
                };
            }
            SetViewbagCasePersonSentencePunishmentCrime(CasePersonSentencePunishmentId);
            return PartialView("EditCasePersonSentencePunishmentCrimeNew", model);
        }

        /// <summary>
        /// Запис на Обстоятелства по заповедни производства
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CasePersonSentencePunishmentCrime(CasePersonSentencePunishmentCrime model)
        {
            string validationError = string.Empty;
            if (!string.IsNullOrEmpty(validationError))
            {
                return Json(new { result = false, message = validationError });
            }

            validationError = ValidateCasePersonSentencePunishmentCrime(model);
            if (!string.IsNullOrEmpty(validationError))
            {
                return Json(new { result = false, message = validationError });
            }

            var currentId = model.Id;
            var res = service.CasePersonSentencePunishmentCrime_SaveData(model);
            if (res)
                SetAuditContext(service, SourceTypeSelectVM.CasePersonSentencePunishmentCrime, model.Id, currentId == 0);

            return Json(new { result = res });
            //return RedirectToAction(nameof(EditCasePersonSentencePunishment), new { id = model.CasePersonSentencePunishmentId });
        }

        private string ValidateCasePersonSentencePunishmentCrime(CasePersonSentencePunishmentCrime model)
        {
            if (model.CaseCrimeId < 1)
            {
                return "Изберете престъпление.";
            }

            if (model.PersonRoleInCrimeId < 1)
            {
                return "Изберете роля на лицето в престъплението.";
            }

            if (model.RecidiveTypeId < 1)
            {
                return "Изберете рецидив.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Изтриване на Участие в престъпления
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> CasePersonSentencePunishmentCrime_Delete(int Id)
        {
            await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonSentencePunishmentCrime, Id, AuditConstants.Operations.Delete);
            return Json(new { result = await service.CasePersonSentencePunishmentCrime_DeleteData(Id) });
        }

        /// <summary>
        /// Създаване на динамичен панел Наказание към престъпление
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IActionResult NewItem_CasePersonCrimePunishment(int index)
        {
            var model = new CasePersonCrimePunishment()
            {
                Index = index
            };
            ViewBag.PunishmentKindDDL = EisppDropDownVM(eisppService.GetDDL_EISPPTblElement(EISPPConstants.EisppTableCode.PunishmentKind)); // nkzvid 209 Видове наказания
            ViewData.TemplateInfo.HtmlFieldPrefix = $"CasePersonCrimePunishmentsVM[{index}]";
            return PartialView("_CrimePunishment", model);
        }
        private EisppDropDownVM EisppDropDownVM(List<SelectListItem> ddList, int defaultFlags = 77)
        {
            return new EisppDropDownVM()
            {
                DDList = ddList,
                Label = "",
                Flags = defaultFlags
            };
        }
        void SetViewbagCasePersonSentencePunishmentCrime(int CasePersonSentencePunishmentId)
        {
            var casePersonSentencePunishment = service.GetById<CasePersonSentencePunishment>(CasePersonSentencePunishmentId);
            var casePersonSentence = service.GetById<CasePersonSentence>(casePersonSentencePunishment.CasePersonSentenceId);
            ViewBag.PersonRoleInCrimeId_ddl = nomService.GetDropDownList<Infrastructure.Data.Models.Nomenclatures.PersonRoleInCrime>();
            ViewBag.RecidiveTypeId_ddl = nomService.GetDropDownList<RecidiveType>();
            ViewBag.CaseCrimeId_ddl = service.GetDropDownList_CasePersonCrime(casePersonSentence.CaseId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonSentencePunishmentCrime(CasePersonSentencePunishmentId);
            ViewBag.PunishmentKindDDL = EisppDropDownVM(eisppService.GetDDL_EISPPTblElement(EISPPConstants.EisppTableCode.PunishmentKind)); // nkzvid 209 Видове наказания
            SetHelpFile(HelpFileValues.CasePerson);
            ViewBag.IsViewBGN = userContext.IsInterimPeriodEuro;
        }

        private async Task SetViewBagBulletin(int personId, int buletinId)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonSentence(personId);
            SetHelpFile(HelpFileValues.CasePerson);
            ViewBag.IssuingCountryId_ddl = await nomService.GetDDL_Countries(true);
            ViewBag.BirthDayPlaceCountryId_ddl = await nomService.GetDDL_Countries(true);
            ViewBag.NationalityCountryOneId_ddl = await nomService.GetDDL_Countries(true);
            ViewBag.NationalityCountryTwoId_ddl = await nomService.GetDDL_Countries(true);
            ViewBag.OtherUicIssuingCountryId_ddl = await nomService.GetDDL_Countries(true);

            if (buletinId > 0)
            {
                ViewBag.DateRegisteredInCais = await caisBuletinService.SelectFiles(buletinId)
                                                        .Where(x => x.DateRegisteredInCais != null)
                                                        .Select(x => x.DateRegisteredInCais)
                                                        .MaxAsync();
            }
        }

        /// <summary>
        /// Добавяне на бюлетин към лице
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddBulletin(int personId)
        {
            var bulletin = service.CasePersonSentenceBulletin_GetByIdPerson(personId);
            if (bulletin == null)
            {
                await SetViewBagBulletin(personId, 0);

                var caseSentence = service.CasePersonSentence_GetByPerson(personId);
                var caseModel = service.GetById<Case>(caseSentence.CaseId);
                var caseLawUnit = lawUnitService.GetJudgeReporter(caseSentence.CaseId);
                DateTime? birthDay = caseSentence.CasePerson.UicTypeId == NomenclatureConstants.UicTypes.EGN ?
                              Utils.Validation.GetBirthDayFromEgn(caseSentence.CasePerson.Uic) : null;

                var model = new CasePersonSentenceBulletinEditVM()
                {
                    CasePersonId = personId,
                    CaseId = caseSentence.CaseId,
                    CourtId = userContext.CourtId,
                    CaseTypeId = caseModel.CaseTypeId,
                    SentenceDescription = "<div style='text-align:justify'>" + service.GetTextNewBulletin(caseSentence.Id, caseSentence.ChangedCasePersonSentenceId) +
                    "<br>" + caseSentence.Description.Replace(Environment.NewLine, "<br>") + "</div>",
                    LawUnitSignId = caseLawUnit != null ? caseLawUnit.LawUnitId : 0,
                };
                if (birthDay != null)
                    model.BirthDay = (DateTime)birthDay;
                return View(nameof(EditBulletin), model);
            }
            else
            {
                return RedirectToAction(nameof(EditBulletin), new { id = bulletin.Id });
            }
        }

        /// <summary>
        /// Редакция на бюлетин към лице
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditBulletin(int id)
        {
            var model = await service.CasePersonSentenceBulletin_GetById(id);
            await SetViewBagBulletin(model.CasePersonId, id);
            return View(nameof(EditBulletin), model);
        }

        /// <summary>
        /// Запис на бюлетин към лице
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditBulletin(CasePersonSentenceBulletinEditVM model)
        {
            await SetViewBagBulletin(model.CasePersonId, model.Id);
            if (model.LawUnitSignId <= 0)
            {
                ModelState.AddModelError(nameof(CasePersonSentenceBulletinEditVM.LawUnitSignId), "Изберете подписващ съдия");
            }

            if (!ModelState.IsValid)
            {
                return View(nameof(EditBulletin), model);
            }
            var currentId = model.Id;
            (bool result, string errorMessage) = await service.CasePersonSentenceBulletin_SaveData(model);
            if (result == true)
            {
                //await SaveFileBulletin(model.Id, model.CasePersonId);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditBulletin), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(string.IsNullOrEmpty(errorMessage) == false ? errorMessage : MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditBulletin), model);
        }

        public async Task<FileContentVM> GetBulletinBytes(int id)
        {
            FileContentVM result = new FileContentVM();

            if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.Request9_2024))
            {
                CaisBuletinModel model = await caisBuletinService.InitBuletinModel(id);
                if (!string.IsNullOrEmpty(model.ErrorMessage))
                {
                    result.FileError = model.ErrorMessage;
                    return result;
                }
                result.RegNumber = model.BuletinInfo.BuletinNumber;
                string html = await this.RenderPartialViewAsync("~/Views/CasePersonSentence/", "_CriminalReport.cshtml", model, true);
                result.Content = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html })
                {
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    CustomSwitches = "--disable-smart-shrinking --margin-bottom 10mm --margin-top 5mm --margin-right 5mm --footer-right [page] --footer-font-size 8 --footer-font-name \"Times New Roman\" --footer-spacing 6"
                }
                .GetByte(this.ControllerContext);
            }
            else
            {
                TinyMCEVM htmlModel = printDocumentService.FillHtmlTemplateSentenceBulletin(id);
                string html = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
                result.Content = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html })
                {
                    PageSize = Rotativa.AspNetCore.Options.Size.B5,
                    CustomSwitches = "--disable-smart-shrinking --margin-top 16mm --margin-right 15mm  --margin-left 15mm"
                }
                .GetByte(this.ControllerContext);
            }
            return result;
        }

        public async Task<IActionResult> PreviewBulletin(int id)
        {
            var buletinFile = await GetBulletinBytes(id);

            if (!string.IsNullOrEmpty(buletinFile.FileError))
            {
                SetErrorMessage(buletinFile.FileError);
                return RedirectToAction(nameof(EditBulletin), new { id });
            }

            var contentDispositionHeader = new ContentDisposition()
            {
                Inline = true,
                FileName = $"casePersonBulletin_{DateTime.Now:yyyyMMdd}_preview.pdf"
            };

            Response.Headers.Append("Content-Disposition", contentDispositionHeader.ToString());

            return File(buletinFile.Content, NomenclatureConstants.ContentTypes.Pdf);
        }

        /// <summary>
        /// запис на файл за бюлетин към лице
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<SaveResultVM> SaveFileBulletin(int id, int xmlFileId)
        {
            var pdfContent = await GetBulletinBytes(id);
            if (!string.IsNullOrEmpty(pdfContent.FileError))
            {
                return new SaveResultVM(false, pdfContent.FileError);
            }

            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.CasePersonBulletinPdf,
                SourceId = xmlFileId.ToString(),
                FileName = $"casePersonBulletin_{pdfContent.RegNumber}.pdf",
                ContentType = NomenclatureConstants.ContentTypes.Pdf,
                Title = $"Бюлетин за съдимост {pdfContent.RegNumber}/{DateTime.Now:dd.MM}",
                FileContentBase64 = Convert.ToBase64String(pdfContent.Content)
            };

            bool result = await cdnService.MongoCdn_AppendUpdate(pdfRequest);
            Response.Headers.Clear();

            return new SaveResultVM()
            {
                Result = result,
                ObjectId = pdfRequest.FileId
            };
        }


        /// <summary>
        /// Добавяне импортирано престъпление от ЕИСПП
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddCaseCrimeEispp(int caseId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.Case, caseId, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = new EisppCrimeVM()
            {
                CaseId = caseId,
                EISPPNumber = eisppService.GetEisppNumber(caseId)
            };
            await SetViewbagEispp(model.CaseId, model.EISPPNumber);
            return View(nameof(AddCaseCrimeEispp), model);
        }

        /// <summary>
        /// запис на импорт на престъпление от еиспп
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddCaseCrimeEispp(EisppCrimeVM model)
        {
            await SetViewbagEispp(model.CaseId, model.EISPPNumber);
            if (!ModelState.IsValid)
            {
                return View(nameof(AddCaseCrimeEispp), model);
            }

            if (await service.CasePersonCrimeFillFromEispp_SaveData(model.CaseId, model.PneNumber))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(AddCaseCrimeEispp), new { caseId = model.CaseId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(AddCaseCrimeEispp), model);
        }
        async Task SetViewbagEispp(int caseId, string eisppNumber)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseCrime(caseId);
            ViewBag.PneNumber_ddl = await eisppService.GetDDL_PneNumbers(caseId, eisppNumber);
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult VerifyEISSPNumber(string EISSPNumber)
        {
            string err = "Грешна контролна сума";
            if (EISSPNumber?.Length == 14)
            {
                var checkSum = eisppService.CheckSum(EISSPNumber);
                if (checkSum == EISSPNumber.Substring(12, 2))
                    return Json(true);
            }
            return Json(err);
        }

        public async Task<IActionResult> DoTask_SentBuletinForSign(long id)
        {
            var task = await taskService.ReadById(id);

            int buletinId = (int)task.SourceId;
            if (CheckDoublePostback("asgn"))
            {
                return RedirectToAction(nameof(EditBulletin), new { id = buletinId });
            }

            if (task.TaskStateId == WorkTaskConstants.States.Completed || task.DateCompleted != null)
            {
                if (task.DateCompleted < DateTime.Now.AddSeconds(-1))
                {
                    SetErrorMessage("Задачата вече е изпълнена успешно.");
                }
                else
                {
                    SetSuccessMessage("Задачите за подписване са създадени успешно.");
                }
                return RedirectToAction(nameof(EditBulletin), new { id = buletinId });
            }

            var buletinModel = await caisBuletinService.InitBuletinModel(buletinId);
            if (!string.IsNullOrEmpty(buletinModel.ErrorMessage))
            {
                SetErrorMessage(buletinModel.ErrorMessage);
                return RedirectToAction(nameof(EditBulletin), new { id = buletinId });
            }


            if (task.TaskStateId == WorkTaskConstants.States.Completed || task.DateCompleted != null)
            {
                SetErrorMessage("Задачата вече е изпълнена успешно.");
                return RedirectToAction(nameof(EditBulletin), new { id = buletinId });
            }

            var bulletinFileId = await caisBuletinService.InitOrGetBulletinFile(buletinId, task.TaskTypeId == WorkTaskConstants.Types.CasePersonBulletin_SentToSignNewNumber);

            if (bulletinFileId == 0)
            {
                SetErrorMessage("Грешка при създаване на задача. Моля, опитайте по-късно.");
                return RedirectToAction(nameof(EditBulletin), new { id = buletinId });
            }

            //Валидира xml на бюлетин, преди изпращане към ЦАЙС Съдебен статус
            //Ако има валидационни грешки задачата се отменя и грешките остават в описание на задачата
            var caisValidationResult = await proxyService.CaisValidateBulletin(buletinId);
            if (!caisValidationResult.Result)
            {
                await taskService.RejectTask(id, caisValidationResult.Content);
                SetErrorMessage(caisValidationResult.Content);
                return RedirectToAction(nameof(EditBulletin), new { id = buletinId });
            }


            var taskInitResult = await service.SendBuletinForSign_Init(buletinId, bulletinFileId, id);
            if (taskInitResult.Result)
            {
                SetSuccessMessage("Задачите за подписване са създадени успешно.");
                await taskService.CompleteTask(id);
            }
            else
            {
                SetErrorMessage(taskInitResult.ErrorMessage);
            }


            return RedirectToAction(nameof(EditBulletin), new { id = buletinId });

        }

        public async Task<IActionResult> test(int id)
        {
            var valRes = await proxyService.CaisValidateBulletin(id);
            return Ok(valRes);
        }

        public async Task<IActionResult> SendBuletinForSign(int id, long taskId)
        {
            var buletinModel = await service.GetReadonlyAsync<CasePersonSentenceBulletin>(id);
            if (buletinModel == null)
            {
                return Redirect_Denied("Търсения от Вас обект не беше намерен!");
            }
            //if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionAct, buletinModel.Id, AuditConstants.Operations.Update, _act.CaseSessionId))
            //{
            //    return Redirect_Denied();
            //}

            var xmlFileId = await service.GetPropByIdAsync<WorkTask, long?>(taskId, x => x.SubSourceId) ?? 0;

            var registerResult = await caisBuletinService.RegisterBulletinFile((int)xmlFileId);

            var safeFileResult = await SaveFileBulletin(buletinModel.Id, (int)xmlFileId);
            if (!safeFileResult.Result)
            {
                SetErrorMessage("Грешка при създаване на бюлетин за съдимост");
                return RedirectToAction(nameof(EditBulletin), new { id });
            }

            CaisBuletinModel caisBuletinModel = await caisBuletinService.InitBuletinModel(id);
            var caisXmlData = caisMapperService.GetXml(caisBuletinModel);
            var xmlRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.CasePersonBulletinXml,
                SourceId = xmlFileId.ToString(),
                FileName = $"caisXmlData_{caisBuletinModel.BuletinInfo.BuletinNumber}.xml",
                ContentType = MediaTypeNames.Application.Xml,
                Title = $"Бюлетин за съдимост {DateTime.Now:yyyyMMdd}",
                FileContentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(caisXmlData))
            };
            bool resultXml = await cdnService.MongoCdn_AppendUpdate(xmlRequest);
            Response.Headers.Clear();

            Uri url = new Uri(Url.Action(nameof(EditBulletin), new { id }), UriKind.Relative);

            var model = new SignPdfInfo()
            {
                SourceId = xmlFileId.ToString(),
                SourceType = SourceTypeSelectVM.CasePersonBulletinXml,
                PreviewFileId = safeFileResult.ObjectId.ToString(),
                DestinationType = SourceTypeSelectVM.CasePersonBulletinXml,
                Location = userContext.CourtName,
                Reason = "Подписване на бюлетин за съдимост",
                SuccessUrl = url,
                CancelUrl = url,
                ErrorUrl = url,
                WorkTaskId = taskId
            };

            var lu = taskService.GetLawUnitByTaskId(taskId);
            if (lu != null)
            {
                model.SignerName = lu.FullName;
                model.SignerUic = lu.Uic;
            }

            return View("_SignXml", model);
        }

        public async Task<IActionResult> zzzGetXml(int id)
        {
            if (!userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator))
            {
                return null;
            }
            CaisBuletinModel caisBuletinModel = await caisBuletinService.InitBuletinModel(id);
            var caisXmlData = caisMapperService.GetXml(caisBuletinModel);
            return Content(caisXmlData);
        }

        //public async Task<IActionResult> xml(int id)
        //{

        //    CaisBuletinModel model = await caisBuletinService.InitBuletinModel(id);

        //    var mappedData = caisMapperService.MapData(model);
        //    var xmlData = caisMapperService.GetXmlDataForSign(mappedData);

        //    return Content(xmlData);

        //}
    }
}
