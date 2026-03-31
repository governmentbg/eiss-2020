// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class BlankTemplateController : BaseController
    {
        private readonly IBlankTemplateService service;
        private readonly INomenclatureService nomService;
        public BlankTemplateController(
            IBlankTemplateService _service,
            INomenclatureService _nomService)
        {
            service = _service;
            nomService = _nomService;
        }

        [TitleAudit(Operation = AuditConstants.Operations.List)]
        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request)
        {
            var data = service.BlankTemplateSelect();
            return request.GetResponse(data);
        }

        [DisableAudit]
        public IActionResult Add()
        {
            var model = new BlankTemplate()
            {
                IsActive = true,
                DateStart = DateTime.Now
            };
            SetViewBag();
            return View(nameof(Edit), model);
        }

        public IActionResult Edit(int id)
        {
            var model = service.GetReadonly<BlankTemplate>(id);
            SetViewBag();
            auditInfo(AuditConstants.Operations.View, model);
            return View(nameof(Edit), model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BlankTemplate model)
        {
            validateModel(model);
            if (!ModelState.IsValid)
            {
                SetViewBag();
                return View(nameof(Edit), model);
            }
            int currentId = model.Id;
            var result = await service.BlankTemplateSaveData(model);
            if (result.Result)
            {
                auditInfo((currentId == 0) ? AuditConstants.Operations.Append : AuditConstants.Operations.Update, model);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
                SetViewBag();
                return View(nameof(Edit), model);
            }
        }

        void validateModel(BlankTemplate model)
        {
            if (string.IsNullOrEmpty(model.Label))
            {
                ModelState.AddModelError(nameof(BlankTemplate.Label), "Въведете Наименование");
            }
            if (model.SourceType <= 0)
            {
                ModelState.AddModelError(nameof(BlankTemplate.SourceType), "Изберете Основен вид");
            }
            if (model.SourceId <= 0)
            {
                ModelState.AddModelError(nameof(BlankTemplate.SourceId), "Изберете Тип");
            }
        }

        public IActionResult Get_SourceIdByType(int sourceType)
        {
            switch (sourceType)
            {
                case SourceTypeSelectVM.CaseSessionAct:
                    return Json(nomService.GetDropDownList<ActType>());
                case SourceTypeSelectVM.DocumentResolution:
                    return Json(nomService.GetDropDownList<ResolutionType>());
                default:
                    return Json(new List<SelectListItem>());
            }
        }

        void auditInfo(string operation, BlankTemplate model)
        {
            AddAuditInfo(operation, model.Label, null, "Шаблон");
        }

        void SetViewBag()
        {
            List<SelectListItem> sTypes = new List<SelectListItem>()
            {
                new SelectListItem(SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.CaseSessionAct),SourceTypeSelectVM.CaseSessionAct.ToString()),
                new SelectListItem(SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.DocumentResolution),SourceTypeSelectVM.DocumentResolution.ToString())
            };
            ViewBag.SourceType_ddl = sTypes;
        }

        [DisableAudit]
        public async Task<IActionResult> Get_BlankTemplates(int sourceType, int sourceId, int? caseId = null)
        {
            BlankTemplateSelectVM model = new BlankTemplateSelectVM();
            ViewBag.BlankTemplateId_ddl = await service.GetBlankTemplates(sourceType, sourceId, caseId);
            return PartialView(model);
        }

        [DisableAudit]
        [HttpPost]
        public IActionResult Get_BlankTemplates(BlankTemplateSelectVM model)
        {
            var data = service.GetReadonly<BlankTemplate>(model.BlankTemplateId);
            if (data == null)
            {
                return Json(new
                {
                    result = false,
                    message = "Изберете шаблон"
                });
            }
            return Json(new
            {
                result = true,
                mainText = data.MainText ?? "",
                addText = data.AddText ?? ""
            });
        }

    }
}
