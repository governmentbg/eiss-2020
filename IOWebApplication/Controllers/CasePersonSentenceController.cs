// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using Elasticsearch.Net;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Eispp;
using Microsoft.AspNetCore.Mvc;
using Rotativa.Extensions;

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
                                            ICaseLawUnitService _lawUnitService)
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
        }

        /// <summary>
        /// Страница за присъди към лице
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public IActionResult Index(int casePersonId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonSentence, null, AuditConstants.Operations.View, casePersonId))
            {
                return Redirect_Denied();
            }
            var casePerson = casePersonService.GetById<CasePerson>(casePersonId);
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
        public IActionResult Add(int casePersonId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonSentence, null, AuditConstants.Operations.Append, casePersonId))
            {
                return Redirect_Denied();
            }
            var casePerson = casePersonService.GetById<CasePerson>(casePersonId);
            SetViewbag(casePerson.CaseId, casePerson.Id, 0);
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
        public IActionResult Edit(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonSentence, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.CasePersonSentence_GetById(id);
            SetViewbag(model.CaseId, model.CasePersonId, id);
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
                if (model.InforcedDate < caseSessionAct.ActInforcedDate)
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
        public IActionResult Edit(CasePersonSentenceEditVM model)
        {
            SetViewbag(model.CaseId, model.CasePersonId, model.Id);

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
            if (service.CasePersonSentence_SaveData(model))
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

        void SetViewbag(int caseId, int casePersonId, int ModelId)
        {
            ViewBag.DecreedCourtId_ddl = caseMigrationService.GetDropDownList_Court(caseId);
            //ViewBag.CaseSessionActId_ddl = caseSessionActService.GetDropDownList(caseId);
            ViewBag.SentenceResultTypeId_ddl = nomService.GetDropDownList<SentenceResultType>();
            ViewBag.PunishmentActivityId_ddl = nomService.GetDropDownList<PunishmentActivity>();
            ViewBag.SentenceExecPeriodId_ddl = nomService.GetDropDownList<SentenceExecPeriod>();
            ViewBag.InforcerInstitutionId_ddl = commonService.GetDDL_Institution(NomenclatureConstants.InstitutionTypes.Attourney);
            ViewBag.ExecInstitutionId_ddl = commonService.GetDDL_Institution(NomenclatureConstants.InstitutionTypes.Prison);
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
        public IActionResult IndexCaseCrime(int caseId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseCrime, null, AuditConstants.Operations.View, caseId))
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
        public IActionResult AddCaseCrime(int caseId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseCrime, null, AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }
            SetViewbagCaseCrime(caseId);
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
        public IActionResult EditCaseCrime(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseCrime, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.GetById<CaseCrime>(id);
            SetViewbagCaseCrime(model.CaseId);
            return View(nameof(EditCaseCrime), model);
        }

        /// <summary>
        /// Валидация преди запис на престъпления към дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private void ValidateCaseCrime(CaseCrime model)
        {
            if (string.IsNullOrEmpty(model.EISSPNumber))
                ModelState.AddModelError("EISSPNumber", "Въведете код по ЕИСПП");

            if (model.CrimeCode == "0" || model.CrimeCode == "-1")
                ModelState.AddModelError("CrimeCode", "Изберете престъпление");
        }

        /// <summary>
        /// запис на престъпления към дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditCaseCrime(CaseCrime model)
        {
            SetViewbagCaseCrime(model.CaseId);
            ValidateCaseCrime(model);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditCaseCrime), model);
            }

            var currentId = model.Id;
            if (service.CaseCrime_SaveData(model))
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

        void SetViewbagCaseCrime(int caseId)
        {
            //ViewBag.CrimeCode_ddl = еISPPService.GetDDL_EISPPTblElement(EISPPConstants.EisppTableCode.EISS_PNE);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseCrime(caseId);
            ViewBag.StartDateTypeDDL = eisppService.GetDDL_EISPPTblElement("1632");
            ViewBag.CrimeStatusDDL = eisppService.GetDDL_EISPPTblElement("206");
            ViewBag.CompletitionDegreeDDL = eisppService.GetDDL_EISPPTblElement("207");
            SetHelpFile(HelpFileValues.CasePersonSentence);
        }

        /// <summary>
        /// Страница с хора към присъда
        /// </summary>
        /// <param name="caseCrimeId"></param>
        /// <returns></returns>
        public IActionResult IndexCasePersonCrime(int caseCrimeId)
        {
            var caseCrime = service.CaseCrime_GetById(caseCrimeId);
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonCrime, null, AuditConstants.Operations.View, caseCrimeId))
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
        public IActionResult AddCasePersonCrime(int caseCrimeId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonCrime, null, AuditConstants.Operations.Append, caseCrimeId))
            {
                return Redirect_Denied();
            }
            var caseCrime = service.GetById<CaseCrime>(caseCrimeId);
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
        public IActionResult EditCasePersonCrime(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonCrime, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.GetById<CasePersonCrime>(id);
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
            ViewBag.CasePersonId_ddl = casePerson.GetDropDownList(caseId, null, false, 0, 0, false);
            ViewBag.RecidiveTypeId_ddl = nomService.GetDropDownList<RecidiveType>();
            ViewBag.PersonRoleInCrimeId_ddl = nomService.GetDropDownList<PersonRoleInCrime>();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonCrime(caseCrimeId);
            SetHelpFile(HelpFileValues.CasePersonSentence);
        }

        [HttpPost]
        public IActionResult CasePersonCrime_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonCrime, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }
            
            var expireObject = service.GetById<CasePersonCrime>(model.Id);
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
        public IActionResult IndexCasePersonSentencePunishment(int casePersonSentenceId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonSentencePunishment, null, AuditConstants.Operations.View, casePersonSentenceId))
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
        public IActionResult AddCasePersonSentencePunishment(int casePersonSentenceId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonSentencePunishment, null, AuditConstants.Operations.Append, casePersonSentenceId))
            {
                return Redirect_Denied();
            }
            var casePersonSentence = service.GetById<CasePersonSentence>(casePersonSentenceId);
            SetViewbagCasePersonSentencePunishment(casePersonSentenceId);
            var model = new CasePersonSentencePunishment()
            {
                CasePersonSentenceId = casePersonSentenceId,
                CaseId = casePersonSentence.CaseId,
                CourtId = userContext.CourtId,
                IsSummaryPunishment = false,
                DateFrom = DateTime.Now
            };
            return View(nameof(EditCasePersonSentencePunishment), model);
        }

        /// <summary>
        /// Редакция на наказания на лица към присъда
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditCasePersonSentencePunishment(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonSentencePunishment, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.GetById<CasePersonSentencePunishment>(id);
            SetViewbagCasePersonSentencePunishment(model.CasePersonSentenceId);
            return View(nameof(EditCasePersonSentencePunishment), model);
        }

        /// <summary>
        /// Валидация преди запис на наказания на лица към присъда
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidCasePersonSentencePunishment(CasePersonSentencePunishment model)
        {
            if (model.SentenceTypeId < 1)
                return "Изберете наказание.";

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
        public IActionResult EditCasePersonSentencePunishment(CasePersonSentencePunishment model)
        {
            SetViewbagCasePersonSentencePunishment(model.CasePersonSentenceId);

            if (!ModelState.IsValid)
            {
                return View(nameof(EditCasePersonSentencePunishment), model);
            }

            string _isvalid = IsValidCasePersonSentencePunishment(model);
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
            ViewBag.SentenceRegimeTypeId_ddl = nomService.GetDropDownList<SentenceRegimeType>();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonSentencePunishment(casePersonSentenceId);
            SetHelpFile(HelpFileValues.CasePerson);
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
        public IActionResult IndexCasePersonSentencePunishmentCrime(int CasePersonSentencePunishmentId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonSentencePunishmentCrime, null, AuditConstants.Operations.View, CasePersonSentencePunishmentId))
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

        /// <summary>
        /// Добавяне на Наложени наказания към присъда
        /// </summary>
        /// <param name="CasePersonSentencePunishmentId"></param>
        /// <returns></returns>
        public IActionResult AddCasePersonSentencePunishmentCrime(int CasePersonSentencePunishmentId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonSentencePunishmentCrime, null, AuditConstants.Operations.Append, CasePersonSentencePunishmentId))
            {
                return Redirect_Denied();
            }
            var casePersonSentencePunishment = service.GetById<CasePersonSentencePunishment>(CasePersonSentencePunishmentId);
            SetViewbagCasePersonSentencePunishmentCrime(CasePersonSentencePunishmentId);
            var model = new CasePersonSentencePunishmentCrime()
            {
                CaseId = casePersonSentencePunishment.CaseId,
                CourtId = userContext.CourtId,
                CasePersonSentencePunishmentId = CasePersonSentencePunishmentId
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
        /// Валидация преди запис на Наложени наказания към присъда
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidCasePersonSentencePunishmentCrime(CasePersonSentencePunishmentCrime model)
        {
            if (model.CaseCrimeId < 1)
                return "Изберете престъпление";

            if (model.PersonRoleInCrimeId < 1)
                return "Изберете роля";

            if (model.RecidiveTypeId < 1)
                return "Изберете рецидив";

            return string.Empty;
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

            string _isvalid = IsValidCasePersonSentencePunishmentCrime(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
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

        void SetViewbagCasePersonSentencePunishmentCrime(int CasePersonSentencePunishmentId)
        {
            var casePersonSentencePunishment = service.GetById<CasePersonSentencePunishment>(CasePersonSentencePunishmentId);
            var casePersonSentence = service.GetById<CasePersonSentence>(casePersonSentencePunishment.CasePersonSentenceId);
            ViewBag.PersonRoleInCrimeId_ddl = nomService.GetDropDownList<PersonRoleInCrime>();
            ViewBag.RecidiveTypeId_ddl = nomService.GetDropDownList<RecidiveType>();
            ViewBag.CaseCrimeId_ddl = service.GetDropDownList_CasePersonCrime(casePersonSentence.CaseId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonSentencePunishmentCrime(CasePersonSentencePunishmentId);
            SetHelpFile(HelpFileValues.CasePerson);
        }

        void SetViewBagBulletin(int personId)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonSentence(personId);
            SetHelpFile(HelpFileValues.CasePerson);
        }

        /// <summary>
        /// Добавяне на бюлетин към лице
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        public IActionResult AddBulletin(int personId)
        {
            var bulletin = service.CasePersonSentenceBulletin_GetByIdPerson(personId);
            if (bulletin == null)
            {
                SetViewBagBulletin(personId);

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
                    SentenceDescription = caseSentence.Description.Replace(Environment.NewLine, "<p>"),
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
        public IActionResult EditBulletin(int id)
        {
            var model = service.CasePersonSentenceBulletin_GetById(id);
            SetViewBagBulletin(model.CasePersonId);

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
            SetViewBagBulletin(model.CasePersonId);
            if (model.LawUnitSignId <= 0)
            {
                ModelState.AddModelError(nameof(CasePersonSentenceBulletinEditVM.LawUnitSignId), "Изберете подписващ съдия");
            }

            if (!ModelState.IsValid)
            {
                return View(nameof(EditBulletin), model);
            }
            var currentId = model.Id;
            (bool result, string errorMessage) = service.CasePersonSentenceBulletin_SaveData(model);
            if (result == true)
            {
                await SaveFileBulletin(model.Id);
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

        /// <summary>
        /// запис на файл за бюлетин към лице
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> SaveFileBulletin(int id)
        {
            TinyMCEVM htmlModel = printDocumentService.FillHtmlTemplateSentenceBulletin(id);
            string html = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
            var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html })
            {
                PageSize = Rotativa.AspNetCore.Options.Size.B5
            }
            .GetByte(this.ControllerContext);
            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.CasePersonBulletin,
                SourceId = id.ToString(),
                FileName = "bulletin.pdf",
                ContentType = "application/pdf",
                Title = "Бюлетин за съдимост",
                FileContentBase64 = Convert.ToBase64String(pdfBytes)
            };
            bool result = await cdnService.MongoCdn_AppendUpdate(pdfRequest);
            Response.Headers.Clear();

            return result;
        }


        /// <summary>
        /// Добаяне импортирано престъпление от ЕИСПП
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddCaseCrimeEispp(int caseId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.Case, caseId, AuditConstants.Operations.Update))
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
            return View(nameof(Edit), model);
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
    }
}
