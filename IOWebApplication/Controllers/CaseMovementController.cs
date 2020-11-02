// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using DataTables.AspNet.Core;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Core.Helper.GlobalConstants;

namespace IOWebApplication.Controllers
{
    public class CaseMovementController : BaseController
    {
        private readonly ICaseMovementService service;
        private readonly INomenclatureService nomService;
        private readonly ICourtOrganizationService organizationService;

        public CaseMovementController(ICaseMovementService _service, INomenclatureService _nomService, ICourtOrganizationService _organizationService)
        {
            service = _service;
            nomService = _nomService;
            organizationService = _organizationService;
        }

        /// <summary>
        /// Страница с Местоположение към дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public IActionResult Index(int CaseId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseMovement, null, AuditConstants.Operations.View, CaseId))
            {
                return Redirect_Denied();
            }
            SetViewBag(CaseId);
            SetHelpFile(HelpFileValues.CaseMovement);
            return View();
        }

        public IActionResult MyMovement()
        {
            //return NotFound();
            SetHelpFile(HelpFileValues.CaseMovement);
            return View();
        }

        /// <summary>
        /// Извличане на данни за Местоположение
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public JsonResult Select(int CaseId)
        {
            var model = service.Select(CaseId);
            return Json(model);
        }

        /// <summary>
        /// Проверка на текущият юзер дали може да създаде запис за Местоположение
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public JsonResult IsAddNewMovement(int CaseId)
        {
            var IsAdd = service.IsAddNewMovement(CaseId);
            return Json(IsAdd);
        }

        /// <summary>
        /// Валидация преди запис на Местоположение
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValid(CaseMovementVM model)
        {
            if (model.MovementTypeId < 1)
                return "Изберете тип насочване";

            switch (model.MovementTypeId)
            {
                case NomenclatureConstants.CaseMovementType.ToPerson:
                    {
                        if (string.IsNullOrEmpty(model.ToUserId))
                            return "Изберете служител";
                    }
                    break;
                case NomenclatureConstants.CaseMovementType.ToOtdel:
                    {
                        if (model.CourtOrganizationId < 1)
                            return "Изберете Отдел/Звено/Дирекция";
                    }
                    break;
                case NomenclatureConstants.CaseMovementType.ToOutStructure:
                    {
                        if (string.IsNullOrEmpty(model.OtherInstitution))
                            return "Въведете външно лице";
                    }
                    break;
            }

            var caseCase = service.GetById<Case>(model.CaseId);
            if (caseCase.CourtId != userContext.CourtId)
            {
                return $"Съда е променен на {userContext.CourtName}. Презаредете текущия екран.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Създаване на Местоположение
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="cmId"></param>
        /// <returns></returns>
        public IActionResult CreateMovement(int CaseId, int cmId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseMovement, null, AuditConstants.Operations.Append, CaseId))
            {
                return Redirect_Denied();
            }
            SetViewBag(CaseId);
            var model = new CaseMovementVM();
            if (cmId > 0)
            {
                model = service.GetById_CaseMovementVM(cmId);
            }
            else
            {
                if (!service.IsAddNewMovement(CaseId))
                {
                    return Content("Не може да извършите тази операция (или делото не е при вас или последното движение не е прието)");
                }

                model.CaseId = CaseId;
                model.CourtId = userContext.CourtId;
            }
            
            return PartialView(model);
        }

        /// <summary>
        /// Запис на Местоположение
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CreateMovement(CaseMovementVM model)
        {
            string validationError = IsValid(model);
            if (!string.IsNullOrEmpty(validationError))
            {
                return Json(new { result = false, message = validationError });
            }

            var res = service.CreateMovement(model);
            SetAuditContext(service, SourceTypeSelectVM.CaseMovement, model.Id, true);
            return Json(new { result = res });
        }

        private void SetViewBag(int CaseId)
        {
            ViewBag.caseId = CaseId;
            var caseCase = service.GetById<Case>(CaseId);
            ViewBag.CaseName = caseCase.RegNumber;

            ViewBag.CourtOrganizationId_ddl = organizationService.CourtOrganization_SelectForDropDownList(userContext.CourtId);
            ViewBag.MovementTypeId_ddl = nomService.GetDropDownList<MovementType>(false);
        }

        /// <summary>
        /// Сторниране на Местоположение
        /// </summary>
        /// <param name="cmId"></param>
        /// <returns></returns>
        public IActionResult StornoMovement(int cmId)
        {
            var model = service.GetById_CaseMovementVM(cmId);
            if (!CheckAccess(service, SourceTypeSelectVM.CaseMovement, cmId, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }
            return PartialView(model);
        }

        /// <summary>
        /// Запис на сторно на Местоположение
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult StornoMovement(CaseMovementVM model)
        {
            CheckAccess(service, SourceTypeSelectVM.CaseMovement, model.Id, AuditConstants.Operations.Delete);
            return Json(new { result = service.StornoMovement(model) });
        }

        /// <summary>
        /// Приемане на Местоположение
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AcceptMovement(int Id)
        {
            CheckAccess(service, SourceTypeSelectVM.CaseMovement, Id, AuditConstants.Operations.Update);
            return Json(new { result = service.AcceptMovement(Id) });
        }

        /// <summary>
        /// Редакция на приемане на Местоположение
        /// </summary>
        /// <param name="cmId"></param>
        /// <returns></returns>
        public IActionResult EditAcceptMovement(int cmId)
        {
            var model = service.GetById_CaseMovementVM(cmId);
            if (!CheckAccess(service, SourceTypeSelectVM.CaseMovement, cmId, AuditConstants.Operations.Update, model.CaseId))
            {
                return Redirect_Denied();
            }
            return PartialView(model);
        }

        /// <summary>
        /// Запис на данни за приемане на Местоположение
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditAcceptMovement(CaseMovementVM model)
        {
            SetAuditContext(service, SourceTypeSelectVM.CaseMovement, model.Id, false);
            return Json(new { result = service.EditAcceptMovement(model) });
        }

        /// <summary>
        /// Създаване на обратно движение на Местоположение
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CreateReturnMovement(int Id)
        {
            var res = service.CreateReturnMovement(Id);
            if (res > 0)
                SetAuditContext(service, SourceTypeSelectVM.CaseMovement, res, true);

            return Json(new { result = (res > 0) });
        }

        public IActionResult MyMovmentComponent(string view = "MyMovment")
        {
            return ViewComponent("MyMovmentComponent", new { view });
        }

        public IActionResult MyMovment_LoadData(IDataTablesRequest request)
        {
            var data = service.Select_ToDo();
            return request.GetResponse(data.AsQueryable());
        }

        /// <summary>
        /// Справка за Местоположение за дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public IActionResult Index_SprFromCaeId(int CaseId)
        {
            var caseCase = service.GetById<Case>(CaseId);
            return RedirectToAction(nameof(Index_Spr), new { numberCase = caseCase.RegNumber }); 
        }

        /// <summary>
        /// Справка за местоположение на дела
        /// </summary>
        /// <param name="numberCase"></param>
        /// <returns></returns>
        public IActionResult Index_Spr(string numberCase)
        {
            var model = new CaseMovementFilterVM();
            model.CaseRegNum = numberCase;
            SetHelpFile(string.IsNullOrEmpty(numberCase) ? HelpFileValues.CaseLocation : HelpFileValues.CaseMovement);

            return View(model);
        }

        /// <summary>
        /// Извличане на данни за справка за Местоположение
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseRegNumber"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseMovement_Spr(IDataTablesRequest request, string caseRegNumber, string UserId)
        {
            var data = service.Select_Spr(userContext.CourtId, caseRegNumber, UserId);
            return request.GetResponse(data);
        }
    }
}