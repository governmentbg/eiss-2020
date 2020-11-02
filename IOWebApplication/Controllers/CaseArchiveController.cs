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
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class CaseArchiveController : BaseController
    {
        private readonly ICaseArchiveService service;
        private readonly INomenclatureService nomService;
        private readonly ICaseSessionActService actService;
        private readonly ICourtArchiveService courtArchiveService;
        private readonly IPriceService priceService;

        public CaseArchiveController(ICaseArchiveService _service, INomenclatureService _nomService, ICaseSessionActService _actService,
                                        ICourtArchiveService _courtArchiveService, IPriceService _priceService)
        {
            service = _service;
            nomService = _nomService;
            actService = _actService;
            courtArchiveService = _courtArchiveService;
            priceService = _priceService;
        }

        /// <summary>
        /// Дела за архивиране
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseForArchive()
        {
            SetHelpFile(HelpFileValues.Archive);
            return View();
        }

        /// <summary>
        /// Извличане на данни за дела за архивиране
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseForArchive(IDataTablesRequest request)
        {
            var data = service.CaseForArchive_Select(userContext.CourtId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Архивирани дела
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        public IActionResult CaseArchive(DateTime? dateFrom, DateTime? dateTo)
        {
            CaseArchiveFilterVM filter = new CaseArchiveFilterVM()
            {
                DateFrom = dateFrom ?? new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = dateTo ?? DateTime.Now
            };
            SetHelpFile(HelpFileValues.Archive);
            return View(filter);
        }
        
        /// <summary>
        /// Извличане на данни за дела в архив
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseArchive(IDataTablesRequest request, CaseArchiveFilterVM model)
        {
            var data = service.CaseArchive_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        void SetViewbag(int caseId, string comeFrom, int caseArchiveId)
        {
            var caseModel = service.GetById<Case>(caseId);
            ViewBag.CaseNumber = caseModel.RegNumber + "/" + caseModel.RegDate.ToString("dd.MM.yyyy");
            var acts = actService.GetDropDownListForArchive(caseId);
            ViewBag.CaseSessionActId_ddl = acts;
            ViewBag.ArchiveIndexId_ddl = courtArchiveService.ArchiveIndex_SelectDDL(caseModel.CourtId, (caseModel.CaseCodeId ?? 0));
            ViewBag.ComeFrom = comeFrom;
            ViewBag.MessageArchivePeriod = "";
            if (caseArchiveId == 0)
            {
                int month = (int)priceService.GetPriceValue(null, NomenclatureConstants.PriceDescKeyWord.ArchivePeriod);
                if (caseModel.CaseInforcedDate == null)
                    ViewBag.MessageArchivePeriod = "Няма въведена дата на влизане в законна сила";
                else if ((caseModel.CaseInforcedDate ?? DateTime.Now).AddMonths(month).Date >= DateTime.Now.Date)
                    ViewBag.MessageArchivePeriod = "Не са изминали " + month + " месеца от датата на влизане в законна сила!";
            }
            SetHelpFile(HelpFileValues.Archive);
        }

        bool IsForDestroy(DateTime? dateDestroy, string comeFrom)
        {
            return dateDestroy != null || comeFrom == "CaseForDestroy";
        }

        /// <summary>
        /// Добавяне на дело в архив
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IActionResult Add(int caseId)
        {
            var caseArchive = service.CaseArchiveByCaseId_Select(caseId);
            if (caseArchive == null)
            {
                SetViewbag(caseId, "CaseForArchive", 0);
                var model = new CaseArchive()
                {
                    CaseId = caseId,
                    CourtId = userContext.CourtId,
                    IsOldNumber = false
                };
                return View(nameof(Edit), model);
            }
            else
            {
                return RedirectToAction(nameof(Edit), new { id = caseArchive.Id, comeFrom = "CaseForArchive" });
            }
        }

        /// <summary>
        /// Редакция на дело в архив 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="comeFrom">от къде е извикан</param>
        /// <returns></returns>
        public IActionResult Edit(int id, string comeFrom)
        {
            var model = service.GetById<CaseArchive>(id);
            SetViewbag(model.CaseId, comeFrom, id);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Валидация преди запис на дело в архив
        /// </summary>
        /// <param name="model"></param>
        /// <param name="isDestroy"></param>
        void ValidateModel(CaseArchive model, bool isDestroy)
        {
            if ((model.IsOldNumber ?? false) && model.Id == 0)
            {
                if (string.IsNullOrEmpty(model.RegNumber))
                {
                    ModelState.AddModelError(nameof(Infrastructure.Data.Models.Cases.CaseArchive.RegNumber), "Въведете 'Стар номер'.");
                }
                if (model.RegDate.Date >= DateTime.Now.Date || model.RegDate.Year < 1900)
                {
                    ModelState.AddModelError(nameof(Infrastructure.Data.Models.Cases.CaseArchive.RegDate), "Невалидна стара дата.");
                }
            }
            else
            {
                if (ModelState.ContainsKey("RegDate"))
                {
                    if (model.Id == 0)
                        model.RegDate = DateTime.Now;
                    ModelState["RegDate"].Errors.Clear();
                    ModelState["RegDate"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
                }
            }

            if (isDestroy == true && string.IsNullOrEmpty(model.ActDestroyLabel))
            {
                ModelState.AddModelError(nameof(Infrastructure.Data.Models.Cases.CaseArchive.ActDestroyLabel), "Въведете протокол за унищожаване");
            }

            var act = service.GetById<CaseSessionAct>(model.CaseSessionActId);
            if (act.ActInforcedDate != null)
            {
                if ((model.BookYear ?? 0) != ((DateTime)act.ActInforcedDate).Year)
                {
                    ModelState.AddModelError(nameof(Infrastructure.Data.Models.Cases.CaseArchive.BookYear), "Година на Том не може да е различна от година на влизане в сила на акта");
                }
            }
        }

        /// <summary>
        /// Запис на дело в архив
        /// </summary>
        /// <param name="model"></param>
        /// <param name="comeFrom"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CaseArchive model, string comeFrom)
        {
            bool isDestroy = IsForDestroy(model.DateDestroy, comeFrom);
            ValidateModel(model, isDestroy);
            SetViewbag(model.CaseId, comeFrom, model.Id);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;
            string messageError = "";
            if (service.CaseArchive_SaveData(model, ref messageError, isDestroy))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id, comeFrom = comeFrom });
            }
            else
            {
                SetErrorMessage(messageError != "" ? messageError : MessageConstant.Values.SaveFailed);
            }
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Извличане на години на съхранение по индекс
        /// </summary>
        /// <param name="indexId"></param>
        /// <returns></returns>
        public IActionResult Get_StorageYears(int indexId)
        {
            var model = courtArchiveService.GetById<CourtArchiveIndex>(indexId);
            return Json(new
            {
                years = model.StorageYears
            });
        }

        /// <summary>
        /// Дела за унищожаване
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseForDestroy()
        {
            SetHelpFile(HelpFileValues.Archive);
            return View();
        }

        /// <summary>
        /// Извличане на данни за дела за унищожаване
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseForDestroy(IDataTablesRequest request, CaseForDestroyFilterVM model)
        {
            var data = service.CaseForDestroy_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }
    }
}