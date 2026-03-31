// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class ToolsController : BaseController
    {
        private readonly IInterestRateService interestRateService;

        public ToolsController(IInterestRateService _interestRateService)
        {
            interestRateService = _interestRateService;
        }

        [TitleAudit(Operation = AuditConstants.Operations.List)]
        public IActionResult InterestRate(int interestType = NomenclatureConstants.InterestRateTypes.OLP)
        {
            FilterInterestRate model = new()
            {
                InterestType = interestType
            };
            ViewBag.InterestType_ddl = new List<SelectListItem>()
            {
                new SelectListItem(interestRateService.GetInterestTypeName(NomenclatureConstants.InterestRateTypes.OLP),NomenclatureConstants.InterestRateTypes.OLP.ToString())
            };
            return View("InterestRateIndex", model);
        }

        [HttpPost]
        [DisableAudit]
        public IActionResult InterestRate_LoadData(IDataTablesRequest request, FilterInterestRate model)
        {
            var data = interestRateService.Select(model);

            return request.GetResponse(data);
        }

        public IActionResult InterestRateAdd(int interestType)
        {
            InterestRate model = new()
            {
                InterestType = interestType,
                IsActive = true
            };
            return View(nameof(InterestRateEdit), model);
        }

        public async Task<IActionResult> InterestRateEdit(int id)
        {
            InterestRate model = await interestRateService.GetByIdAsync<InterestRate>(id);

            auditInfoInterestRate(AuditConstants.Operations.View, model);

            return View(nameof(InterestRateEdit), model);
        }

        [HttpPost]
        public async Task<IActionResult> InterestRateEdit(InterestRate model)
        {
            var currentId = model.Id;
            SaveResultVM result = await interestRateService.SaveData(model);
            if (result.Result)
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                auditInfoInterestRate(currentId == 0 ? AuditConstants.Operations.Append : AuditConstants.Operations.Update, model);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                SetSuccessMessage(MessageConstant.Values.SaveFailed);
            }
            return RedirectToAction(nameof(InterestRate), new { interestType = model.InterestType });
        }

        void auditInfoInterestRate(string operation, InterestRate model, string add = "")
        {
            if (model != null)
            {
                AddAuditInfo(operation, $"{interestRateService.GetInterestTypeName(NomenclatureConstants.InterestRateTypes.OLP)}, Дата: {model.Date:dd.MM.yyyy}, Процент: {model.Rate:N2}", add, "Лихвени проценти");
            }
        }

        public IActionResult Calculators()
        {
            ToolsCalculatorVM model = new();
            return PartialView(model);
        }

        public async Task<IActionResult> Calc_LegalInterest(decimal amount, DateTime fromDate, DateTime toDate)
        {
            var result = await interestRateService.CalcOLPRates(amount, fromDate, toDate, 10M);
            return Json(result);
        }
    }
}
