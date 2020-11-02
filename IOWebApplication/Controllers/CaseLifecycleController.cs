// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class CaseLifecycleController : BaseController
    {
        private readonly ICaseLifecycleService service;
        private readonly INomenclatureService nomService;

        public CaseLifecycleController(ICaseLifecycleService _service, INomenclatureService _nomService)
        {
            service = _service;
            nomService = _nomService;
        }

        public IActionResult Index(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLifecycle, null, AuditConstants.Operations.View, id))
            {
                return Redirect_Denied();
            }
            var tcase = service.GetById<Case>(id);
            ViewBag.caseId = id;
            ViewBag.CaseName = tcase.RegNumber;
            SetHelpFile(HelpFileValues.CaseLifecycle);

            return View();
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int caseId)
        {
            var data = service.CaseLifecycle_Select(caseId);
            return request.GetResponse(data);
        }

        public IActionResult Add(int caseId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLifecycle, null, AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }
            var model = new CaseLifecycle()
            {
                CaseId = caseId,
                CourtId = userContext.CourtId
            };
            SetViewbag(caseId);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на интервал
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLifecycle, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.GetById<CaseLifecycle>(id);
            if (model == null)
            {
                throw new NotFoundException("Търсеният от Вас интервал не е намерен и/или нямате достъп до него.");
            }
            SetViewbag(model.CaseId);
            return View(nameof(Edit), model);
        }

        void SetViewbag(int caseId)
        {
            ViewBag.LifecycleTypeId_ddl = nomService.GetDropDownList<LifecycleType>().Where(x => x.Value != NomenclatureConstants.LifecycleType.InProgress.ToString()).ToList();

            var caseCase = service.GetById<Case>(caseId);
            ViewBag.CaseName = caseCase.RegNumber;
            SetHelpFile(HelpFileValues.CaseLifecycle);
        }

        /// <summary>
        /// Валидация преди запис на интервал
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValid(CaseLifecycle model)
        {
            if (model.LifecycleTypeId < 0)
                return "Няма избран вид";

            if (model.DateFrom == null)
                return "Няма въведена начална дата";

            if (model.DateTo != null)
            {
                if (model.DateFrom > model.DateTo)
                    return "Началната дата е по-голяма от крайната";
            }

            var caseLifecycles = service.CaseLifecycle_Select(model.CaseId).ToList();

            if (model.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
            {
                if (model.Id < 1)
                {
                    if (!caseLifecycles.Any(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress))
                        return "Не може да въведете ръчно първоначален интервал.";

                    if (caseLifecycles.Any(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress && x.DateTo == null))
                        return "Има интервал който не е приключен";

                    model.Iteration = caseLifecycles.Where(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress).Max(x => x.Iteration) + 1;
                }
                else
                {
                    if (model.DateTo != null)
                    {
                        if (caseLifecycles.Any(x => x.Iteration == model.Iteration && x.LifecycleTypeId == NomenclatureConstants.LifecycleType.Stop && x.DateTo == null))
                            return "Има спиране без въведена крайна дата.";
                    }
                }
            }
            else
            {
                if (model.Id < 1)
                {
                    if (model.LifecycleTypeId == NomenclatureConstants.LifecycleType.Stop)
                    {
                        if (!caseLifecycles.Any(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress))
                            return "Няма интервал в процес за да въведете спиране.";

                        if (!caseLifecycles.Any(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress && x.DateTo == null))
                            return "Няма интервал в процес, който не е приключен за да въведете спиране.";

                        if (caseLifecycles.Any(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.Stop && x.DateTo == null))
                            return "Има спиране в което не е въведен крайният срок.";

                        model.Iteration = caseLifecycles.Where(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress && x.DateTo == null).FirstOrDefault().Iteration;
                    }
                }
                else
                {
                    if (model.LifecycleTypeId == NomenclatureConstants.LifecycleType.Stop)
                    {
                        if (!caseLifecycles.Any(x => x.Iteration == model.Iteration && x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress && x.DateTo == null))
                            return "Не можете да редактирате спирането, защото интервалът в процес е въведена крайна дата.";
                    }
                }

                if (model.LifecycleTypeId == NomenclatureConstants.LifecycleType.Stop)
                {
                    var caseLifecycle = caseLifecycles.Where(x => x.Iteration == model.Iteration && x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress).FirstOrDefault();

                    if (model.DateFrom <= caseLifecycle.DateFrom)
                        return "Началната дата на спирането е по-малка от началната дата на интервала";

                    if (model.DateTo != null)
                    {
                        if (caseLifecycle.DateTo != null)
                        {
                            if (model.DateTo > caseLifecycle.DateTo)
                                return "Крайната дата на спирането е по-голяма от крайната дата на интервала.";
                        }
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Запис на интервал
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CaseLifecycle model)
        {
            SetViewbag(model.CaseId);
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
            if (service.CaseLifecycle_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseLifecycle, model.Id, currentId == 0);
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
    }
}