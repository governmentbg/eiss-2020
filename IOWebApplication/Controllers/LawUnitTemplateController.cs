// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class LawUnitTemplateController : BaseController
    {
        private readonly IBlankTemplateService service;
        private readonly INomenclatureService nomService;
        public LawUnitTemplateController(
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
            var data = service.LawUnitTemplateSelect();
            return request.GetResponse(data);
        }

        [DisableAudit]
        public async Task<IActionResult> Add()
        {
            var model = new LawUnitTemplate()
            {
                IsActive = true
            };
            await SetViewBag();
            return View(nameof(Edit), model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var model = service.GetReadonly<LawUnitTemplate>(id);
            await SetViewBag();
            auditInfo(AuditConstants.Operations.View, model);
            return View(nameof(Edit), model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(LawUnitTemplate model)
        {
            if (!ModelState.IsValid)
            {
                await SetViewBag();
                return View(nameof(Edit), model);
            }
            int currentId = model.Id;
            var result = await service.LawUnitTemplateSaveData(model);
            if (result.Result)
            {
                auditInfo((currentId == 0) ? AuditConstants.Operations.Append : AuditConstants.Operations.Update, model);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
                await SetViewBag();
                return View(nameof(Edit), model);
            }
        }

        void auditInfo(string operation, LawUnitTemplate model)
        {
            AddAuditInfo(operation, model.Label, null, "Шаблон на лице");
        }

        async Task SetViewBag()
        {
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>(false, true);
            ViewBag.ActTypeId_ddl = await nomService.GetDropDownListAsync<IOWebApplication.Infrastructure.Data.Models.Nomenclatures.ActType>(false, true);
        }

        [DisableAudit]
        public async Task<IActionResult> Get_LawUnitTemplates(int actId = 0)
        {
            BlankTemplateSelectVM model = new BlankTemplateSelectVM();
            ViewBag.BlankTemplateId_ddl = await service.GetLawUnitTemplates(actId);
            return PartialView(model);
        }

        [DisableAudit]
        [HttpPost]
        public IActionResult Get_LawUnitTemplates(BlankTemplateSelectVM model)
        {
            var data = service.GetReadonly<LawUnitTemplate>(model.BlankTemplateId);
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
                mainText = data.ActMain ?? "",
                addText = data.ActDispositive ?? ""
            });
        }

    }
}
