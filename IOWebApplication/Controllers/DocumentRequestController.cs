// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class DocumentRequestController : BaseController
    {
        private readonly IDocumentRequestService requestService;
        private readonly INomenclatureService nomenclatureService;
        public DocumentRequestController(
            IDocumentRequestService _requestService,
            INomenclatureService _nomService)
        {
            requestService = _requestService;
            nomenclatureService = _nomService;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">DocumentId от ЦР</param>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index(long id, int caseId = 0)
        {
            if (id > 0)
            {
                if (!await CheckAccessAsync(requestService, SourceTypeSelectVM.Document, id, AuditConstants.Operations.Update))
                    return Redirect_Denied();
            }
            if (caseId > 0)
            {
                if (!await CheckAccessAsync(requestService, SourceTypeSelectVM.Case, caseId, AuditConstants.Operations.Update))
                    return Redirect_Denied();
            }


            var model = await requestService.GetDocumentRequestById(id, caseId);

            await SetViewbagRightSide(id, caseId);
            SetViewbag_MoneyClaims();
            SetViewbag_Expense();
            await SetViewbag_ClaimCircumstances(model.RequestTypeCode);
            SetViewbag_CompetencyBases();
            ViewData["IsInEuro"] = userContext.IsPeriodEuro;
            if (caseId > 0)
                ViewBag.selectDocument_SelectDocumentRequestId_ddl = await requestService.GetDDL_AllDocumentRequests(caseId);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Index(FastProcessRequestVM model, string recalcExpense = null)
        {
            //Този ред е необходим при изтриване на ред и въвеждане в същата сесия на нов ред, тогава част от полетата на изтрития елемент се копират
            //по имена от предходния ред, защото съществуват в ModelState със съответния индекс
            //ModelState.Clear();
            var validationErrors = model.ValidateRequest(userContext.IsPeriodEuro);
            var serverErrors = await requestService.ValidateFastProcessRequestData(model);
            validationErrors.AddRange(serverErrors);
            if (validationErrors.Count > 0 && !model.SubmitOnErrors)
            {
                await requestService.InitRequestSides(model);
                await SetViewbagRightSide(model.DocumentId, model.CaseId);
                SetViewbag_MoneyClaims();
                SetViewbag_Expense();
                await SetViewbag_ClaimCircumstances(model.RequestTypeCode);
                SetViewbag_CompetencyBases();
                SetErrorMessage(MessageConstant.Values.MessageDataValidation);
                model.RecreateObject();
                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError(error.Control, error.Error);
                }
                model.ErrorCount = validationErrors.Count;
                return View(model);
            }

            var result = await requestService.SaveDocumentRequest(model, recalcExpense != null);

            if (result.Result)
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                SetErrorMessage(result.ErrorMessage);
            }

            return RedirectToAction(nameof(Index), new { id = model.DocumentId, caseId = model.CaseId });
        }

        [HttpPost]
        public async Task<IActionResult> SelectDocumentDataToCase(int caseId, long documentId)
        {
            var saveResult = await requestService.SelectDocumentDataToCase(caseId, documentId);
            if (saveResult.Result)
            {
                SetSuccessMessage("Зареждането на данни от документа премина успешно");
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return Json(saveResult);
        }

        public async Task<IActionResult> Display(int caseId)
        {
            var model = new BaseBlankRequestVM();
            model.IsInEuro = userContext.IsPeriodEuro;
            model.Data = await requestService.GetDocumentRequestById(0, caseId);
            model.Nomenclatures = await requestService.LoadAliasNomenclatures(NomenclatureConstants.FPaliases.FastProcessNomenclatures, model.Data);
            return View(model);
        }


        public IActionResult New_MoneyClaim(int index)
        {
            var model = new FastProcessMoneyClaimVM()
            {
                Index = index,
                CurrencyCode = userContext.CurrentCurrencyCode,
                Gid = Guid.NewGuid().ToString().ToLower()
            };
            ViewData["IsInEuro"] = userContext.IsPeriodEuro;
            ViewData.TemplateInfo.HtmlFieldPrefix = model.GetPath;
            SetViewbag_MoneyClaims();
            return PartialView("_FastProcessMoneyClaimItem", model);
        }

        public IActionResult New_ItemSubstitutionClaim(int index)
        {
            var model = new FastProcessItemSubstitutionClaimVM()
            {
                Index = index,
                Gid = Guid.NewGuid().ToString().ToLower()
            };
            ViewData["IsInEuro"] = userContext.IsPeriodEuro;
            ViewData.TemplateInfo.HtmlFieldPrefix = model.GetPath;
            return PartialView("_FastProcessItemSubstitutionItem", model);
        }

        public async Task<IActionResult> New_DebtDistribution(int index, long documentId, int caseId)
        {
            var model = new FastProcessDebtDistributionVM()
            {
                Index = index,
                CurrencyCode = userContext.CurrentCurrencyCode,
                ShareProcent = 100
            };
            await SetViewbagRightSide(documentId, caseId);
            ViewBag.CurrenciesDDL = nomenclatureService.GetDropDownListCodeCode<Currency>(true);

            ViewData.TemplateInfo.HtmlFieldPrefix = model.GetPath;
            ViewData["IsInEuro"] = userContext.IsPeriodEuro;
            return PartialView("_FastProcessDebtDistributionItem", model);
        }
        public IActionResult New_Expense(int index)
        {
            var model = new FastProcessExpenseVM()
            {
                Index = index,
                Gid = Guid.NewGuid().ToString().ToLower()
            };
            SetViewbag_Expense();
            ViewData.TemplateInfo.HtmlFieldPrefix = model.GetPath;
            ViewData["IsInEuro"] = userContext.IsPeriodEuro;
            return PartialView("_FastProcessExpenseItem", model);
        }

        async Task SetViewbagRightSide(long documentId, int caseId)
        {
            ViewBag.RightSideDDL = await requestService.GetDDL_DocumentSideList(documentId, caseId, NomenclatureConstants.RoleKind.RightSide);
        }
        void SetViewbag_MoneyClaims()
        {
            ViewBag.CurrenciesDDL = nomenclatureService.GetDropDownListCodeDescription<Currency>(true);
            ViewBag.MoneyClaimTypesDDL = nomenclatureService.GetDropDownListFromCode<FastProcessMoneyClaimType>(true);
        }
        void SetViewbag_Expense()
        {
            ViewBag.ExpenseTypesDDL = nomenclatureService.GetDropDownListFromCode<FastProcessExpenseType>(true);
        }
        async Task SetViewbag_ClaimCircumstances(string requestTypeCode)
        {
            ViewBag.ClaimCircumstancesCodeDDL = await requestService.GetDDL_ClaimCircumstancesCode(requestTypeCode, true);

        }
        void SetViewbag_CompetencyBases()
        {
            ViewBag.CompetencyBaseDDL = nomenclatureService.GetDropDownListFromCode<FastProcess417CompetencyBase>(true);
        }

        public async Task<IActionResult> LoadPersonList(long documentId, int caseId)
        {
            var model = await requestService.GetDocumentRequestById(documentId, caseId);
            return PartialView("_FastProcessSidesDisplay", model);
        }

        public async Task<IActionResult> UpdateRepresentatives(long documentId)
        {
            var model = await requestService.DocumentPersonLinks_Select(documentId);
            return PartialView("_UpdateRepresentatives", model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRepresentatives(DocumentPersonLinkVM model)
        {
            var saveResult = await requestService.DocumentPersonLinks_SaveData(model);
            return Json(saveResult);
        }
    }
}
