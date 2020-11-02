// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOWebApplication.Controllers
{
    public class CourtArchiveController : BaseController
    {
        private readonly ICourtArchiveService service;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;

        public CourtArchiveController(ICourtArchiveService _service, INomenclatureService _nomService, ICommonService _commonService)
        {
            service = _service;
            nomService = _nomService;
            commonService = _commonService;
        }

        /// <summary>
        /// Експертни комисии
        /// </summary>
        /// <returns></returns>
        public IActionResult ArchiveCommittee()
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ArchiveCommittee().DeleteOrDisableLast();
            return View();
        }

        /// <summary>
        /// Извличане на Експертни комисии
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataArchiveCommittee(IDataTablesRequest request)
        {
            var data = service.CourtArchiveCommittee_Select(userContext.CourtId);

            return request.GetResponse(data);
        }

        public void SetBreadcrumsArchiveCommittee(int id)
        {
            if (id > 0)
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ArchiveCommitteeEdit(id).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ArchiveCommitteeAdd().DeleteOrDisableLast();
        }

        /// <summary>
        /// Добавяне на Експертни комисии
        /// </summary>
        /// <returns></returns>
        public IActionResult AddArchiveCommittee()
        {
            SetBreadcrumsArchiveCommittee(0);
            CourtArchiveCommittee model = new CourtArchiveCommittee()
            {
                CourtId = userContext.CourtId,
                DateStart = DateTime.Now
            };
            return View(nameof(EditArchiveCommittee), model);
        }

        /// <summary>
        /// Редакция на Експертни комисии
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditArchiveCommittee(int id)
        {
            SetBreadcrumsArchiveCommittee(id);
            var model = service.GetById<CourtArchiveCommittee>(id);
            return View(nameof(EditArchiveCommittee), model);
        }

        /// <summary>
        /// Запис на Експертни комисии
        /// </summary>
        /// <param name="model"></param>
        /// <param name="lawUnitJson"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditArchiveCommittee(CourtArchiveCommittee model, string lawUnitJson)
        {
            List<int> lawUnits = new List<int>();
            string errLawUnits = "";
            try
            {
                lawUnits = JsonConvert.DeserializeObject<List<int>>(lawUnitJson);
            }
            catch (Exception ex)
            {
                errLawUnits = "Проблем със списъка с потребители" + ex.Message;
                ModelState.AddModelError("lawUnitJson", errLawUnits);
            }

            if (!ModelState.IsValid)
            {
                SetBreadcrumsArchiveCommittee(model.Id);
                return View(nameof(EditArchiveCommittee), model);
            }
            var currentId = model.Id;
            if (service.CourtArchiveCommittee_SaveData(model, lawUnits))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditArchiveCommittee), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            SetBreadcrumsArchiveCommittee(model.Id);
            return View(nameof(EditArchiveCommittee), model);
        }

        /// <summary>
        /// Избрани служители в експертна комисия
        /// </summary>
        /// <param name="committeeId"></param>
        /// <returns></returns>
        public JsonResult LawUnitRightList(int committeeId)
        {
            var data = service.CourtArchiveCommitteeLawUnit_Select(committeeId);
            return Json(data);
        }

        /// <summary>
        /// Списък със служители за добавяне към експертна комисия
        /// </summary>
        /// <returns></returns>
        public JsonResult LawUnitLeftList()
        {
            var data = commonService.LawUnitMultiSelect_ByCourt(userContext.CourtId);
            return Json(data);
        }

        /// <summary>
        /// Номенклатурни индекси
        /// </summary>
        /// <returns></returns>
        public IActionResult ArchiveIndex()
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ArchiveIndex().DeleteOrDisableLast();
            return View();
        }

        /// <summary>
        /// Извличане на данни Номенклатурни индекси
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataArchiveIndex(IDataTablesRequest request)
        {
            var data = service.CourtArchiveIndex_Select(userContext.CourtId);

            return request.GetResponse(data);
        }

        public void SetBreadcrumsArchiveIndex(int id)
        {
            if (id > 0)
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ArchiveIndexEdit(id).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ArchiveIndexAdd().DeleteOrDisableLast();
        }

        public void SetViewBagArchiveIndex()
        {
            ViewBag.filterCaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.CourtArchiveCommitteeId_ddl = service.ArchiveCommittee_SelectDDL(userContext.CourtId);
        }

        /// <summary>
        /// Добавяне на Номенклатурни индекси
        /// </summary>
        /// <returns></returns>
        public IActionResult AddArchiveIndex()
        {
            SetViewBagArchiveIndex();
            SetBreadcrumsArchiveIndex(0);
            CourtArchiveIndexEditVM model = new CourtArchiveIndexEditVM()
            {
                CourtId = userContext.CourtId,
                DateStart = DateTime.Now
            };
            return View(nameof(EditArchiveIndex), model);
        }

        /// <summary>
        /// Редакция на Номенклатурни индекси
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditArchiveIndex(int id)
        {
            SetViewBagArchiveIndex();
            SetBreadcrumsArchiveIndex(id);
            var model = service.GetByIdVM(id);
            return View(nameof(EditArchiveIndex), model);
        }

        /// <summary>
        /// Запис на Номенклатурни индекси
        /// </summary>
        /// <param name="model"></param>
        /// <param name="caseCodesJson"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditArchiveIndex(CourtArchiveIndexEditVM model, string caseCodesJson)
        {
            SetViewBagArchiveIndex();
            List<int> caseCodes = new List<int>();
            string errcaseCodes = "";
            try
            {
                caseCodes = JsonConvert.DeserializeObject<List<int>>(caseCodesJson);
            }
            catch (Exception ex)
            {
                errcaseCodes = "Проблем със списъка с шифри" + ex.Message;
                ModelState.AddModelError("caseCodesJson", errcaseCodes);
            }

            if (!ModelState.IsValid)
            {
                SetBreadcrumsArchiveIndex(model.Id);
                return View(nameof(EditArchiveIndex), model);
            }
            var currentId = model.Id;
            (bool result, string errorMessage) = service.CourtArchiveIndex_SaveData(model, caseCodes);
            if (result == true)
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditArchiveIndex), new { id = model.Id });
            }
            else
            {
                if (string.IsNullOrEmpty(errorMessage))
                    errorMessage = MessageConstant.Values.SaveFailed;
                SetErrorMessage(errorMessage);
            }
            SetBreadcrumsArchiveIndex(model.Id);
            return View(nameof(EditArchiveIndex), model);
        }

        /// <summary>
        /// Избрани шифри в експртна комисия
        /// </summary>
        /// <param name="indexId"></param>
        /// <returns></returns>
        public JsonResult CodeRightList(int indexId)
        {
            var data = service.CourtArchiveIndexCode_Select(indexId);
            return Json(data);
        }

        /// <summary>
        /// Списък с шифри за избор в експертна комисия
        /// </summary>
        /// <param name="caseGroupId"></param>
        /// <returns></returns>
        public JsonResult CodeLeftList(int caseGroupId)
        {
            var data = nomService.CaseCodeForSelect_Select(caseGroupId);
            return Json(data);
        }
    }
}