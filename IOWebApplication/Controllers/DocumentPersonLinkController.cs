// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class DocumentPersonLinkController : BaseController
    {
        private readonly IDocumentPersonLinkService service;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly IDocumentResolutionService drService;
        public DocumentPersonLinkController(IDocumentPersonLinkService _service,
            INomenclatureService _nomService,
            ICommonService _commonService,
            IDocumentResolutionService _drService)
        {
            service = _service;
            nomService = _nomService;
            commonService = _commonService;
            drService = _drService;
        }
        /// <summary>
        /// Връзки между страни
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public IActionResult Index(long documentId, long? documentResolutionId)
        {
            if (!CheckAccess(drService, SourceTypeSelectVM.DocumentResolution, null, AuditConstants.Operations.Append, documentId))
            {
                return Redirect_Denied();
            }
            ViewBag.documentId = documentId;
            ViewBag.documentResolutionId = documentResolutionId;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForDocumentPersonLink(documentId, documentResolutionId ?? 0).DeleteOrDisableLast();
            // SetHelpFile(HelpFileValues.CasePerson);
            return View();
        }

        /// <summary>
        /// Извличане на данни Връзки между страни
        /// </summary>
        /// <param name="request"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, long documentId)
        {
            var data = service.DocumentPersonLink_Select(documentId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на Връзки между страни
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public IActionResult Add(long documentId, long? documentResolutionId)
        {
            if (!CheckAccess(drService, SourceTypeSelectVM.DocumentResolution, null, AuditConstants.Operations.Append, documentId))
            {
                return Redirect_Denied();
            }
            SetViewbag(documentId, 0, 0);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForDocumentPersonLinkEdit(documentId, documentResolutionId ??0, 0).DeleteOrDisableLast();
            var model = new DocumentPersonLink()
            {
                DocumentId = documentId,
                DocumentResolutionId = documentResolutionId,
                CourtId = userContext.CourtId,
                DateFrom = DateTime.Now
            };
            return View(nameof(Edit), model);
        }
        /// <summary>
        /// Редакция на Връзки между страни
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id, long? documentResolutionId)
        {
            var model = service.GetById<DocumentPersonLink>(id);

            if (!CheckAccess(drService, SourceTypeSelectVM.DocumentResolution, null, AuditConstants.Operations.Append, model.DocumentId))
            {
                return Redirect_Denied();
            }
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForDocumentPersonLinkEdit(model.DocumentId, documentResolutionId ?? 0,  model.Id).DeleteOrDisableLast();
            SetViewbag(model.DocumentId, model.DocumentPersonId, model.LinkDirectionId);
            return View(nameof(Edit), model);
        }

        void SetViewbag(long documentId, long documentPersonId, int linkDirectionId)
        {
            ViewBag.DocumentName = service.GetById<Document>(documentId).DocumentNumber;

            ViewBag.DocumentPersonId_ddl = service.DocumentPerson_SelectForDropDownList(documentId);
            ViewBag.DocumentPersonRelId_ddl = service.RelationalPersonDDL(documentId, linkDirectionId);
            ViewBag.DocumentPersonSecondRelId_ddl = service.SeccondRelationalPersonDDL(documentId);
            ViewBag.LinkDirectionId_ddl = service.LinkDirectionForPersonDDL(documentPersonId);
            ViewBag.LinkDirectionSecondId_ddl = service.SecondLinkDirectionDDL();
            SetHelpFile(HelpFileValues.CasePerson);
        }
        /// <summary>
        /// Валидация преди запис на Връзки между страни
        /// </summary>
        /// <param name="model"></param>
        private void ValidateModel(DocumentPersonLink model)
        {
            if (model.LinkDirectionSecondId > 0 && model.DocumentPersonSecondRelId <= 0)
                ModelState.AddModelError(nameof(DocumentPersonLink.DocumentPersonSecondRelId), "Изберете Втори представляващ");
            if (model.DocumentPersonSecondRelId == model.DocumentPersonRelId)
                ModelState.AddModelError(nameof(DocumentPersonLink.DocumentPersonSecondRelId), "Трябва да е различно лице от Упълномощено лице");
            if (model.DocumentPersonSecondRelId == model.DocumentPersonId)
                ModelState.AddModelError(nameof(DocumentPersonLink.DocumentPersonSecondRelId), "Трябва да е различно лице от Страна");
            if (model.DocumentPersonRelId == model.DocumentPersonId)
                ModelState.AddModelError(nameof(DocumentPersonLink.DocumentPersonRelId), "Трябва да е различно лице от Страна");
        }

        /// <summary>
        /// Запис на Връзки между страни
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(DocumentPersonLink model)
        {
            SetViewbag(model.DocumentId, model.DocumentPersonId, model.LinkDirectionId);
           // ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonLinkEdit(model.CaseId, model.Id).DeleteOrDisableLast();
            ValidateModel(model);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;
            if (service.DocumentPersonLink_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CasePersonLink, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id, model.DocumentResolutionId});
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(Edit), model);
        }
        [HttpGet]
        public IActionResult FilterLinkDirection(long documentPersonId)
        {
            List<SelectListItem> ddlLinkDirection = service.LinkDirectionForPersonDDL(documentPersonId);
            return Json(new { ddlLinkDirection });
        }
        [HttpGet]
        public IActionResult FilterRelationalPerson(long documentId, int linkDirectionId)
        {
            List<SelectListItem> ddlPersonRel = service.RelationalPersonDDL(documentId, linkDirectionId);
            return Json(new { ddlPersonRel });
        }


    }
}
