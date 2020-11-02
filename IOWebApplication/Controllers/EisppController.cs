// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.EISPP;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.Integrations.Eispp;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Eispp;
using IOWebApplication.Infrastructure.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using static IOWebApplication.Infrastructure.Constants.EISPPConstants;

namespace IOWebApplication.Controllers
{
    public class EisppController : BaseController
    {
        private readonly IEisppService service;
        private readonly IEisppRulesService serviceRules;
        private readonly IMQEpepService mqService;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly ICasePersonService casePersonService;
        public EisppController(
            IEisppService _service,
            IEisppRulesService _serviceRules,
            IMQEpepService _mqService,
            INomenclatureService _nomService,
            ICommonService _commonService,
            ICasePersonService _casePersonService)
        {
            service = _service;
            serviceRules = _serviceRules;
            nomService = _nomService;
            mqService = _mqService;
            commonService = _commonService;
            casePersonService = _casePersonService;
        }

        public IActionResult GetValue(string tbl, string code)
        {
            return Content(service.GetElement(tbl, code));
        }

        public async Task<IActionResult> GetActualData(string eisppNumber)
        {
            try
            {
                var actualData = await service.GetActualData(eisppNumber);
                return PartialView("_ActualData", actualData);
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }
        public IActionResult SendPackage(int packageId)
        {
            var model = service.GetPackage(packageId);
            var eisppEvent = model.Data.Events[0];
            if (eisppEvent.EventKind == EventKind.OldEvent || eisppEvent.EventType < 0)
            {
                return RedirectToAction(nameof(EisppChangePreview), new { eventId = packageId });
            }
            SetViewBag_SendPackage(eisppEvent, true);
            return View(model);
        }

        public async Task<IActionResult> DeletePackageJson(EisppPackage model)
        {
            await service.DeletePackageJson(model);
            return RedirectToAction(nameof(SendPackage),
                    new { packageId = model.Id });
        }
        
        private void ValidateEvent(string namePrefix, Event eisppEvent, int eventTypeId)
        {
            var aCase = service.GetById<Case>(eisppEvent.CaseId);
            if (eisppEvent.CriminalProceeding.Case.ConnectedCases != null)
            {
                foreach(var connectedCases in eisppEvent.CriminalProceeding.Case.ConnectedCases)
                {
                    connectedCases.IsSelected = (connectedCases.ConnectedCaseId == eisppEvent.CriminalProceeding.Case.ConnectedCaseId);
                }
            }
            if (eisppEvent.CriminalProceeding.Case.ConnectedCaseId == "0") {
                (var rules, var flags) = serviceRules.GetEisppRuleIds(eventTypeId, "NPR.DLO.DLOOSN");
                if (flags > 0)
                {
                    ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.ConnectedCaseId", "Изберете основание");
                }
            }
            if (eisppEvent.EventDate.Date < eisppEvent.CriminalProceeding.Case.Status.StatusDate.Date)
            {
                ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Status.StatusDateVM", "Датата на статус на дело не може да бъде по-малка от дата на събитие");
            }
            if (eisppEvent.CriminalProceeding.Case.Crimes != null)
            {
                for (int i_crime = 0; i_crime < eisppEvent.CriminalProceeding.Case.Crimes.Length; i_crime++)
                {
                    var crime = eisppEvent.CriminalProceeding.Case.Crimes[i_crime];

                    if (crime.StartDateType <= 0)
                        ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Crimes[{i_crime}].StartDateType", "Въведете Тип на дата на престъпление");
                    if (crime.StartDate <= new DateTime(1900, 1, 1) || crime.StartDate > DateTime.Now.Date)
                        ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Crimes[{i_crime}].StartDateVM", "Въведете начална дата на престъпление");
                    if (crime.StartDateType == StartDateType.pneotdper)
                    {
                        if (crime.EndDate <= new DateTime(1900, 1, 1) || crime.EndDate > DateTime.Now.Date)
                            ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Crimes[{i_crime}].EndDateVM",
                                "Начална дата на извършване -> очаква въвеждане на период, т.е. Начална дата и Крайна дата");
                    }
                }
            }
            // ?????? според ЕИСПП майл
            //if (eisppEvent.CriminalProceeding.Case.Crimes != null)
            //{
            //    for (int i = 0; i < eisppEvent.CriminalProceeding.Case.Crimes.Length; i++)
            //    {
            //        var crime = eisppEvent.CriminalProceeding.Case.Crimes[i];
            //        if (crime.CrimeStatus?.Status > 0 && 
            //           (crime.CrimeStatus?.StatusDate < new DateTime(2000,1,1) || crime.CrimeStatus?.StatusDate.Date > DateTime.Now.Date))
            //        {
            //            ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Crimes[{i}].CrimeStatus.StatusDateVM", "Датата на статус e задължителна при избран статус на престъпление");
            //        }
            //    }
            //}
            if (eisppEvent.CriminalProceeding.Case.Persons != null)
            {
                (var rulesPunishment, var flagsPunishment) = serviceRules.GetEisppRuleIds(eventTypeId, "NPR.DLO.DLOOSN");
                for (int i = 0; i < eisppEvent.CriminalProceeding.Case.Persons.Length; i++)
                {
                    var person = eisppEvent.CriminalProceeding.Case.Persons[i];
                    if (person.IsSelected) {
                        if (person.IsBgCitizen && string.IsNullOrEmpty(person.Egn))
                        {
                            ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Persons[{i}].Egn", "ЕГН е задължителнo!");
                            continue;
                        }
                        if (person.BirthDate < new DateTime(1900, 1, 1) || person.BirthDate > DateTime.Now.Date)
                        {
                            if (person.IsBgCitizen)
                            {
                                ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Persons[{i}].BirthDateVM", "Датата на раждане e задължителна Въведете я през лица към делото");
                            }
                            else
                            {
                                ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Persons[{i}].BirthDateVM", "Датата на раждане e задължителна!");
                            }

                        }
                    }
                    int personCrimes = 0;
                    try
                    {
                        personCrimes = eisppEvent.CriminalProceeding.Case.Crimes.Sum(x => x.CPPersonCrimes.Count(cp => cp.IsSelected && cp.PersonId == person.PersonId));
                    }
                    catch
                    {
                    }
      
                    if (person.IsSelected)
                    {
                        if (personCrimes == 0)
                        {
                            ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Persons[{i}].Egn", "Не избрано нито едно участие в престъпление за лицето!");
                        } else
                        {
                            for (int i_crime = 0; i_crime < eisppEvent.CriminalProceeding.Case.Crimes.Length; i_crime++)
                            {
                                var crime = eisppEvent.CriminalProceeding.Case.Crimes[i_crime];
                                if (crime.CPPersonCrimes == null)
                                    continue;

                                for (int i_pc = 0; i_pc < crime.CPPersonCrimes.Length; i_pc++)
                                {
                                    var personCrime = crime.CPPersonCrimes[i_pc];
                                    if (!personCrime.IsSelected)
                                        continue;
                                    if (personCrime.PersonId != person.PersonId)
                                        continue;
                                    if (personCrime.CrimeSanction?.CrimePunishments == null)
                                        continue;
                                    for (int i_pcp = 0; i_pcp < personCrime.CrimeSanction.CrimePunishments.Length; i_pcp++)
                                    {
                                        var crimePunishment = personCrime.CrimeSanction.CrimePunishments[i_pcp];
                                        if (!crimePunishment.IsSelected)
                                            continue;
                                        (var ddl, var punishmentKindMode, var servingTypeId, var showRegim, var showServingType) = service.GetPunishmentPeriodMode(eisppEvent.EventId, crimePunishment.PunishmentKind, 0);
                                        if (punishmentKindMode == PunishmentVal.effective_period ||
                                            punishmentKindMode == PunishmentVal.period ||
                                            punishmentKindMode == PunishmentVal.probation ||
                                            punishmentKindMode == PunishmentVal.probation_period)
                                        {
                                            if (!crimePunishment.HavePeriod())
                                                ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Crimes[{i_crime}].CPPersonCrimes[{i_pc}].CrimeSanction.CrimePunishments[{i_pcp}].PunishmentYears",
                                                    "Въведете за срок на наказание поне едно от дни, седмици, месеци, години!");
                                        }
                                        if (punishmentKindMode == PunishmentVal.fine)
                                        {
                                            if (crimePunishment.FineAmount < 0.001)
                                                ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Crimes[{i_crime}].CPPersonCrimes[{i_pc}].CrimeSanction.CrimePunishments[{i_pcp}].FineAmount",
                                                                         "Въведете глоба!");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else 
                    {
                        if (personCrimes > 0)
                            ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Persons[{i}].Egn", "Избрано е участие в престъпление за лицето!");
                    }
                    if ((flagsPunishment & 2) > 0)
                    {
                        if (person.Punishments == null || !person.Punishments.Any())
                        {
                            ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Persons[{i}].Egn", "Няма въведени наказания!");
                        } else
                        {
                            // ????? Дали е така (според САС трябва да има и двете)
                            if (!person.Punishments.Any(x => x.PunishmentType == PunishmentType.ForExecution))
                                ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Persons[{i}].Egn", "Няма въведенo " + service.GetElementLabel(PunishmentType.ForExecution.ToString()) + "!");
                            if (!person.Punishments.Any(x => x.PunishmentType == PunishmentType.Union))
                                ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Persons[{i}].Egn", "Няма въведенo " + service.GetElementLabel(PunishmentType.Union.ToString()) + "!");
                        }
                    }
                    if (person.Punishments != null)
                    {
                        for (int p_i = 0; p_i < person.Punishments.Length; p_i++)
                        {
                            var punishment = person.Punishments[p_i];
                            (var ddl, var punishmentKindMode, var servingTypeId, var showRegim, var showServingType) = service.GetPunishmentPeriodMode(eisppEvent.EventId, punishment.PunishmentKind, punishment.ServingType);
                            if (punishmentKindMode == PunishmentVal.effective_period ||
                                punishmentKindMode == PunishmentVal.period ||
                                punishmentKindMode == PunishmentVal.probation ||
                                punishmentKindMode == PunishmentVal.probation_period)
                            {
                                if (!punishment.HavePeriod())
                                    ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Persons[{i}].Punishments[{p_i}].PunishmentYears",
                                                             "Въведете за срок на наказание поне едно от дни, седмици, месеци, години!");
                            }
                            if (showRegim)
                            {
                                if (punishment.PunishmentRegime <= 0)
                                    ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Persons[{i}].Punishments[{p_i}].PunishmentRegime",
                                                             "Въведете Режим на изтърпяване!");
                            }
                            if (punishmentKindMode == PunishmentVal.probation_period)
                            {
                                if (!punishment.HaveProbationPeriod())
                                    ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Persons[{i}].Punishments[{p_i}].PunishmentYears",
                                                             "Въведете за изпитателен срок поне едно от дни, седмици, месеци, години!");
                                if (punishment.ProbationStartDate < aCase.RegDate.Date || punishment.ProbationStartDate.Date > DateTime.Now)
                                    ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Persons[{i}].Punishments[{p_i}].ProbationStartDateVM",
                                                             $"Въведете начало изпитателен срок от {aCase.RegDate:dd.MM.yyyy} до {DateTime.Now:dd.MM.yyyy}!");

                            }
                            if (punishment.PunishmentActivityDate < aCase.RegDate.Date || punishment.PunishmentActivityDate.Date > DateTime.Now)
                                ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Persons[{i}].Punishments[{p_i}].PunishmentActivityDateVM",
                                                         $"Въведете начало срок от {aCase.RegDate:dd.MM.yyyy} до {DateTime.Now:dd.MM.yyyy}!");

                            if (punishmentKindMode == PunishmentVal.fine)
                            {
                                if (punishment.FineAmount < 0.001)
                                    ModelState.AddModelError($"{namePrefix}.CriminalProceeding.Case.Persons[{i}].Punishments[{p_i}].FineAmount",
                                                             "Въведете глоба!");
                            }
                        }
                    }
                }
            }

        }

        [HttpPost]
        [RequestFormLimits(ValueCountLimit = 15000)]
        public IActionResult SendPackage(EisppPackage model)
        {
            var errors1 = ModelState.Values.Where(x => x.ValidationState != ModelValidationState.Valid).ToList();
            try
            {
                ValidateEvent("Data.Events[0]", model.Data.Events[0], model.EventTypeId);
            } catch
            {

            }
            ValidateRules(model.EventTypeId, "", "", model);
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.Where(x => x.ValidationState != ModelValidationState.Valid).ToList();
                ViewBag.isPostBack = true;
                SetViewBag_SendPackage(model.Data.Events[0], true);
                return View(model);
            }
            service.SaveCasePackageData(model, null);
            return RedirectToAction(nameof(SendPackage),
                new { packageId = model.Id });
        }

        public IActionResult EisppEvent(string sourceType, string sourceId, int caseId, int? caseSessionActId)
        {
            var model = service.GetEisppEventVM(sourceType, sourceId, caseId, caseSessionActId);
            SetViewBag_EventType(model);
            return View(model);
        }
        private void  ValidateEisppEvent(EisppEventVM modelVM)
        {
            (var ruleIds, var flags) = serviceRules.GetEisppRuleIds(modelVM.EventType, "sbedkpvid");
            if (ruleIds.Any()) { 
                var sessionActDDL = service.CaseSessionActDDL(modelVM.CaseId, modelVM.EventType, null, null);
            
                if (modelVM.CaseSessionActId <= 0 || !sessionActDDL.Any(x => x.Value == modelVM.CaseSessionActId.ToString()))
                    ModelState.AddModelError(nameof(modelVM.CaseSessionActId), "Изберете Акт/Протокол");
            }
            int featureType = serviceRules.GetEisppRuleValue(modelVM.EventType, "SBH.sbhvid").ToInt();
            if (featureType == FeatureType.SentenceType)
            {
                var sentense = service.GetSentence(modelVM.CasePersonId, modelVM.CaseSessionActId);
                if (sentense == null)
                     ModelState.AddModelError(nameof(modelVM.CaseSessionActId), "Няма присъда за лицето по този Акт/Протокол");
            }
        }
        public JsonResult GetSentencePersonId(int caseSessionActId)
        {
            var casePersonId = service.GetSentencePersonId(caseSessionActId);
            return Json(new { casePersonId });
        }
        [HttpPost]
        public async Task<IActionResult> EisppEvent(EisppEventVM modelVM)
        {
            ValidateEisppEvent(modelVM);
            if (!ModelState.IsValid)
            {
                SetViewBag_EventType(modelVM);
                return View(modelVM);
            }
            if (modelVM.EventType == EventType.GetCase)
            {
                var result = await service.GetCase(modelVM);
                if (result)
                {
                    ViewBag.IsGetCaseSaved = true;
                    SetSuccessMessage(MessageConstant.Values.SaveOK);
                }
                else
                    SetErrorMessage(MessageConstant.Values.SaveFailed);
                SetViewBag_EventType(modelVM);
                return View(modelVM);
            }
            else
            {
                ModelState.Clear();
                var model = await service.GeneratePackage(modelVM.CaseId, modelVM.EventType, modelVM.CasePersonId, modelVM.ConnectedCaseId, modelVM.CaseSessionActId, modelVM.PersonOldMeasureId, modelVM.PersonMeasureId);
                model.SourceType = modelVM.SourceType.ToInt();
                model.SourceId = modelVM.SourceId.ToInt();
                model.CaseId = modelVM.CaseId;

                if (modelVM.EventType < -1)
                {
                    var modelChangeVM = new EisppChangeVM();
                    modelChangeVM.EisppPackage = model;
                    return View(nameof(EisppChange), modelChangeVM);
                }
                else
                {
                    SetViewBag_SendPackage(model.Data.Events[0], true);
                    return View(nameof(SendPackage), model);
                }
            }
        }
        public IActionResult EisppChange(EisppChangeVM model)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForEisppEventChangeEdit(model.OldEvent.CaseId, false).DeleteOrDisableLast();
            ModelState.Clear();
            return View(nameof(EisppChange), model);
        }
        public IActionResult EisppChangeFrom(int eventFromId)
        {
            var model = service.GeneratePackageFrom(eventFromId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForEisppEventChangeEdit(model.OldEvent.CaseId, false).DeleteOrDisableLast();
            ModelState.Clear();
            return View(nameof(EisppChange), model);
        }
        public IActionResult EisppChangePreview(int eventId)
        {
            var model = service.GetPackageChange(eventId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForEisppEventChangeEdit(model.OldEvent.CaseId, false).DeleteOrDisableLast();
            return View(nameof(EisppChange), model);
        }

        [HttpPost]
        [RequestFormLimits(ValueCountLimit = 15000)]
        public IActionResult EisppChangeSave(EisppChangeVM model)
        {
            var package = model.EisppPackage;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForEisppEventChangeEdit(model.OldEvent.CaseId, false).DeleteOrDisableLast();
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.Where(x => x.ValidationState != ModelValidationState.Valid).ToList();
                ViewBag.isPostBack = true;
                SetViewBag_SendPackage(model.EisppPackage.Data.Events[0], true);
                return View(model);
            }
            package.IsForSend = model.IsForSend;
            package.Id = model.EventId;
            service.SaveCasePackageData(package, model.EventFromId);
            ModelState.Clear();
            return View(nameof(EisppChange), model);
        }

        [HttpPost]
        public IActionResult OldEvent(EisppChangeVM model)
        {
            SetViewBag_SendPackage(model.OldEvent, false);
            bool isDelete = (model.NewEvent == null);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForEisppEventChangeEditOld(model.OldEvent.CaseId, isDelete).DeleteOrDisableLast();
            return View(nameof(OldEvent), model);
        }

        [HttpPost]
        public IActionResult NewEvent(EisppChangeVM model)
        {
            model.NewEventObj = model.NewEvent;
            SetViewBag_SendPackage(model.NewEventObj, false);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForEisppEventChangeEditOld(model.OldEvent.CaseId, false).DeleteOrDisableLast();
   
            return View(nameof(NewEvent), model);
        }
        
        [HttpPost]
        [RequestFormLimits(ValueCountLimit = 15000)]
        public IActionResult NewEventSave(EisppChangeVM model)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForEisppEventChangeEdit(model.OldEvent.CaseId, false).DeleteOrDisableLast();
            var package = model.EisppPackage;
            try
            {
                ValidateEvent("NewEventObj", model.NewEventObj, package.EventTypeId);
            } catch
            {

            }
            ValidateRules(package.EventTypeId, "DATA.VHD.SBE", "NewEventObj", model.NewEventObj);
            if (!ModelState.IsValid)
            {
                SetViewBag_SendPackage(model.NewEventObj, false);
                return View(nameof(NewEvent), model);
            }
            model.NewEvent = model.NewEventObj;
            ModelState.Clear();
            return View(nameof(EisppChange), model);
        }
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, EisppEventFilterVM filterData)
        {
            var data = service.GetPackages(filterData);
            return request.GetResponse(data);
        }
        public IActionResult Index(int caseId)
        {
            EisppEventFilterVM filter = new EisppEventFilterVM();
            filter.CaseId = caseId;
            SetViewBag_Index(caseId);
            return View(nameof(Index), filter);
        }

        public IActionResult IndexAll(int courtId)
        {
            EisppEventFilterVM filter = new EisppEventFilterVM();
            filter.CourtId = courtId;
            SetViewBag_IndexAll(courtId);
            return View(nameof(Index), filter);
        }
        public JsonResult GetPunishmentKindMode(int eventType, int punishmentKind, int servingType)
        {
            (var ddl , var punishmentKindMode, var servingTypeId, var showRegim, var showServingType) = service.GetPunishmentPeriodMode(eventType, punishmentKind, servingType);
            return Json(new { punishmentKindMode, servingTypeDDL = ddl.DDList, servingTypeId, showRegim, showServingType });
        }
        public JsonResult GetPunishmentServingTypeMode(int servingTypeId)
        {
            string punishmentServingTypeMode = service.GetPunishmentServingTypeMode(servingTypeId);
            return Json(new { punishmentServingTypeMode });
        }
        public JsonResult GetPbcMeasureUnit(int pbcMeasureTypeId)
        {
            string pbcMeasureUnit = service.GetPbcMeasureUnit(pbcMeasureTypeId);
            return Json(new { pbcMeasureUnit });
        }



        private EisppDropDownVM EisppDropDownVM(List<SelectListItem> ddList, int defaultFlags = 5)
        {
            return new EisppDropDownVM()
            {
                DDList = ddList,
                Label = "",
                Flags = defaultFlags
            };
        }
        public JsonResult GetCaseSessionActDDL(int caseId, DateTime? actDateFrom, DateTime? actDateTo)
        {
            var result = service.CaseSessionActDDL(caseId, null, actDateFrom, actDateTo);
            return Json(new { caseSessionActDDL = result });
        }
        [HttpPost]
        public IActionResult ExpiredInfo(ExpiredInfoVM model)
        {
            int eventId = model.ReturnUrl.ToInt();
            var eventItem = service.GetById<EisppEventItem>(eventId);
            var result = false;
            if (eventItem.MQEpepId == null)
            {
                model.Id = eventId;
                result = service.SaveExpireInfoPlus(model);
            }
            else
            {
                result = (service.GeneratePackageDelete(eventId) > 0);
            }
            model.Id = eventId;
            if (result)
            {
                SetSuccessMessage(MessageConstant.Values.EisppEventItemExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("Index", new { caseId = eventItem.CaseId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        private string GetXmlAttrib(PropertyInfo prop)
        {
            object[] attribs = prop.GetCustomAttributes(typeof(System.Xml.Serialization.XmlAttributeAttribute), false);
            bool doesPropertyHaveAttrib = attribs.Length > 0;
            if (doesPropertyHaveAttrib)
            {
                return ((System.Xml.Serialization.XmlAttributeAttribute)attribs[0]).AttributeName;
            }
            return "";
        }
        private string GetXmlElement(PropertyInfo prop)
        {
            object[] attribs = prop.GetCustomAttributes(typeof(XmlElementAttribute), false);
            bool doesPropertyHaveAttrib = attribs.Length > 0;
            if (doesPropertyHaveAttrib)
            {
                return ((XmlElementAttribute)attribs[0]).ElementName;
            }
            return "";
        }

        private string GetXmlArray(PropertyInfo prop)
        {
            object[] attribs = prop.GetCustomAttributes(typeof(XmlArrayAttribute), false);
            bool doesPropertyHaveAttrib = attribs.Length > 0;
            if (doesPropertyHaveAttrib)
            {
                return ((XmlArrayAttribute)attribs[0]).ElementName;
            }
            return "";
        }
        private string GetXmlArrayItem(PropertyInfo prop)
        {
            object[] attribs = prop.GetCustomAttributes(typeof(XmlArrayItemAttribute), false);
            bool doesPropertyHaveAttrib = attribs.Length > 0;
            if (doesPropertyHaveAttrib)
            {
                return ((XmlArrayItemAttribute)attribs[0]).ElementName;
            }
            return "";
        }
        private string GetXmlDisplayName(PropertyInfo prop)
        {
            object[] attribs = prop.GetCustomAttributes(typeof(DisplayAttribute), false);
            bool doesPropertyHaveAttrib = attribs.Length > 0;
            if (doesPropertyHaveAttrib)
            {
                return ((DisplayAttribute)attribs[0]).Name;
            }
            return prop.Name;
        }
       
        private void ValidateRuleVM(PropertyInfo prop, PropertyInfo propVM, int eventType, string rulePrefix, string objectPrefix, object value)
        {
            rulePrefix = rulePrefix.Replace("DATA.VHD.SBE.", "", StringComparison.InvariantCultureIgnoreCase);
            string attrName = GetXmlAttrib(prop);

            if (!string.IsNullOrEmpty(attrName))
            {
                (var rules, var flags) = serviceRules.GetEisppRuleIds(eventType, rulePrefix + attrName);
                if ((flags & 2 ) > 0 && (value == null || value.ToString() == "" || value.ToString() == "0"))
                {
                    ModelState.AddModelError(objectPrefix + propVM.Name, "Изберете " + GetXmlDisplayName(propVM));
                }
            }
        }

        private void ValidateRule(PropertyInfo prop, int eventType, string rulePrefix, string objectPrefix, object value)
        {
            rulePrefix = rulePrefix.Replace("DATA.VHD.SBE.", "", StringComparison.InvariantCultureIgnoreCase);
            string attrName = GetXmlAttrib(prop);
            if (!string.IsNullOrEmpty(attrName))
            {
                (var rules, var flags) = serviceRules.GetEisppRuleIds(eventType, rulePrefix + attrName);
                if ((flags & 2) > 0 && (value == null || value.ToString() == "" || value.ToString() == "0"))
                {
                    ModelState.AddModelError(objectPrefix + prop.Name, "Изберете " + GetXmlDisplayName(prop));
                }
            }
        }
        private void ValidateRuleDouble(PropertyInfo prop, int eventType, string rulePrefix, string objectPrefix, object value)
        {
            rulePrefix = rulePrefix.Replace("DATA.VHD.SBE.", "", StringComparison.InvariantCultureIgnoreCase);
            string attrName = GetXmlAttrib(prop);
            if (!string.IsNullOrEmpty(attrName))
            {
                (var rules, var flags) = serviceRules.GetEisppRuleIds(eventType, rulePrefix + attrName);
                if ((flags & 2) > 0 && (Double)value < 0.01)
                {
                    ModelState.AddModelError(objectPrefix + prop.Name, "Въведете стойност за " + GetXmlDisplayName(prop));
                }
            }
        }
        public void ValidateRules(int eventType, string rulePrefix, string objectPrefix, object value)
        {
            Type t = value.GetType();
            rulePrefix = string.IsNullOrEmpty(rulePrefix) ? "" : rulePrefix + ".";
            objectPrefix = string.IsNullOrEmpty(objectPrefix) ? "" : objectPrefix + "."; 
            var str = "";
            var isSelectedProp = t.GetProperties().Where(x => x.Name == "IsSelected").FirstOrDefault();
            if (isSelectedProp != null)
            {
                bool isSelected = (bool)isSelectedProp.GetValue(value);
                if (!isSelected)
                    return;
            }
            foreach (var prop in t.GetProperties())
            {
                switch (prop.PropertyType.Name)
                {
                    case "Int32":
                        ValidateRule(prop, eventType, rulePrefix, objectPrefix, prop.GetValue(value));
                        break;
                    case "String":
                        ValidateRule(prop, eventType, rulePrefix, objectPrefix, prop.GetValue(value));
                        break;
                    //case "Double":
                    //    ValidateRuleDouble(prop, eventType, rulePrefix, objectPrefix, prop.GetValue(value));
                    //    break;
                    case "DateTime":
                        var propVM = t.GetProperties().FirstOrDefault(x => x.Name == prop.Name + "VM");
                        if (propVM != null)
                        {
                            ValidateRuleVM(prop, propVM, eventType, rulePrefix, objectPrefix, propVM.GetValue(value));
                        } else
                        {
                            ValidateRule(prop, eventType, rulePrefix, objectPrefix, prop.GetValue(value));
                        }
                        break;
                    case "Boolean":
                        break;
                    default:
                        if (prop.PropertyType.BaseType.Name == "Object" || (prop.PropertyType.BaseType.Name != "Array" && prop.PropertyType.BaseType?.BaseType?.Name == "Object"))
                        {
                            string elName = GetXmlElement(prop);
                            if (elName == "KST")
                                continue;
                            if (!string.IsNullOrEmpty(elName))
                            {
                                var elValue = prop.GetValue(value);
                                if (elValue != null)
                                    ValidateRules(eventType, rulePrefix +  elName, objectPrefix  + prop.Name, prop.GetValue(value));
                            }
                            continue;
                        }
                        if (prop.PropertyType.BaseType.Name == "Array")
                        {
                            string arrName = GetXmlArray(prop);
                            string arrItemName = GetXmlArrayItem(prop);
                            var fldName = "";
                            if (!string.IsNullOrEmpty(arrName))
                            {

                                fldName = arrName + "." + arrItemName;
                            }
                            else
                            {
                                fldName = GetXmlElement(prop);
                            }
                            if (!string.IsNullOrEmpty(fldName))
                            {
                                if (fldName == "NPRFZLPNE")
                                {
                                    if (rulePrefix == "DATA.VHD.SBE.NPR.DLO.")
                                        continue;
                                    if (rulePrefix == "DATA.VHD.SBE.NPR.DLO.PNE.")
                                        rulePrefix = "DATA.VHD.SBE.NPR.DLO.";
                                }
                                object[] arrValue = (object[])prop.GetValue(value);
                                if (arrValue != null)
                                {
                                    for (int i = 0; i < arrValue.Length; i++)
                                    {
                                        ValidateRules(eventType, rulePrefix + fldName,  $"{objectPrefix}{prop.Name}[{i}]", arrValue[i]);
                                    }
                                }
                            }
                            continue;
                        }
                        str += prop.PropertyType.Name;
                        break;
                }
            }
                
        }
        public async Task<IActionResult> GetNPCard(int id)
        {
            var cdnResult = await service.GetNPCard(id);
            if (cdnResult != null)
            {
                return File(Convert.FromBase64String(cdnResult.FileContentBase64), System.Net.Mime.MediaTypeNames.Application.Pdf, cdnResult.FileName);
            } else
            {
                return null;
            }
        }
        public async Task<IActionResult> GetEisppResponse(int id)
        {
            var cdnResult = await service.GetEisppResponse(id);
            if (cdnResult != null)
            {
                return File(Convert.FromBase64String(cdnResult.FileContentBase64), System.Net.Mime.MediaTypeNames.Application.Xml, cdnResult.FileName);
            }
            else
            {
                return new NoContentResult();
            }
        }
        public IActionResult GetEisppRequest(int id)
        {
            var result = service.GetEisppRequest(id);
            if (result != null)
            {
                return File(result, System.Net.Mime.MediaTypeNames.Application.Xml, $"{id}.xml");
            }
            else
            {
                return null;
            }
        }
        [HttpPost]
        public JsonResult GetPersonMeasure(int casePersonId)
        {
            var personOldMeasureDDL = service.GetPersonProceduralCoercionMeasure(casePersonId, true);
            var personMeasureDDL = service.GetPersonProceduralCoercionMeasure(casePersonId, false);
            return Json(new { personOldMeasureDDL, personMeasureDDL });
        }
        [HttpPost]
        public JsonResult GetCaseSessionActForEvent(int caseId, int eventTypeId)
        {
            (var ruleIds, var flags) = serviceRules.GetEisppRuleIds(eventTypeId, "sbedkpvid");
            bool sessionActRequired = ruleIds.Any();
            var sessionActDDL = service.CaseSessionActDDL(caseId, eventTypeId, null, null);
            return Json(new { sessionActDDL, sessionActRequired });
        }


        private void SetViewBag_SendPackage(Event model, bool setBreadCrumbs)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForEisppEventEdit(model.CaseId).DeleteOrDisableLast();

            ViewBag.ConnectedCaseId_ddl = service.GetDDL_ConnectedCases(model.CaseId);

            int eventType = model.EventType;
            ViewBag.EventTypeId = eventType;
           // ViewBag.EventType_ddl = service.GetDDL_EISPPEventType(model.CriminalProceeding.Case.CaseCodeId, model.CriminalProceeding.Case.CaseTypeId);
            ViewBag.EventTypeDDL = EisppDropDownVM(service.GetDDL_EISPPEventType(model.CriminalProceeding.Case.CaseCodeId, model.CriminalProceeding.Case.CaseTypeId), 3);
            ViewBag.CiminalProceedingCrimeDDL = EisppDropDownVM(service.GetDDL_EISPPTblElement(EisppTableCode.CiminalProceedingCrime)); // nprpnests

            ViewBag.ExactCaseTypeDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.ExactCaseType, eventType, "NPR.DLO.dlosig"); // dlosig
          

            ViewBag.CaseTypeDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.CaseType, eventType, "NPR.DLO.dlovid"); // dlovid       
            // Тип дело в основание
            ViewBag.CaseTypeCauseDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.CaseType, eventType, "NPR.DLO.DLOOSN.dlovid"); // dlovid 

            ViewBag.CaseSetupTypeDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.CaseSetupType, eventType, "NPR.DLO.dloncnone"); // dloncnone
            ViewBag.CaseReasonDDL = EisppDropDownVM(service.GetDDL_EISPPTblElement(EisppTableCode.CaseReason)); // dlopmt
            ViewBag.CaseStatusDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.CaseStatus, eventType, "NPR.DLO.DLOSTA.dlosts"); // dlosts
            ViewBag.LegalProceedingTypeDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.LegalProceedingType, eventType, "NPR.DLO.dlosprvid");  // dlosprvid
            ViewBag.StartDateTypeDDL = EisppDropDownVM(service.GetDDL_EISPPTblElement(EisppTableCode.StartDateType)); // pneotdtip
            ViewBag.CrimeStatusDDL = EisppDropDownVM(service.GetDDL_EISPPTblElement(EisppTableCode.CrimeStatus));  // pnests
            ViewBag.CompletitionDegreeDDL = EisppDropDownVM(service.GetDDL_EISPPTblElement(EisppTableCode.CompletitionDegree)); // pnestpdvs

            //ViewBag.CrimeConstitutionDDL = service.GetDDL_EISPPTblElement("eiss_pne");
            ViewBag.CrimeSanctionRoleDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.CrimeSanctionRole, eventType, "NPR.DLO.NPRFZLPNE.SCQ.scqrlq"); // scqrlq
            var countriesDDL = service.GetDDL_CountriesForEISPP();
            ViewBag.CountriesOtherDDL = EisppDropDownVM(countriesDDL.Where(x => x.Value != EISPPConstants.CountryBG.ToString()).ToList(), 7);
            ViewBag.CountriesBgDDL = EisppDropDownVM(countriesDDL.Where(x => x.Value == EISPPConstants.CountryBG.ToString()).ToList(),3);
            ViewBag.CountriesDDL = EisppDropDownVM(countriesDDL);

            ViewBag.GenderDDL = EisppDropDownVM(service.GetDDL_EISPPTblElement(EisppTableCode.Gender), 3);
            ViewBag.AddressTypeDDL = EisppDropDownVM(service.GetDDL_EISPPTblElement(EisppTableCode.AddressType)); // NPR.DLO.PNE.ADR.adrtip
            ViewBag.SanctionTypeDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.SanctionType, eventType, "NPR.DLO.NPRFZLPNE.SCQ.scqvid"); // scqvid
            if (model.EventFeature?.FeatureType == FeatureType.SentenceType && model.EventFeature?.FeatureVal == EISPPConstants.SentenceResultType.Innocence)
            {
                ((EisppDropDownVM)ViewBag.SanctionTypeDDL).Flags = 3;
            }
            ViewBag.SanctionReasonDDL = service.GetDDL_EISPPTblElementNomWithRules(eventType, "NPR.DLO.NPRFZLPNE.SCQ.scqosn", ".scqosn"); // scqosn

            ViewBag.PStatusPersonRoleDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.PersonRole, eventType, "NPR.DLO.FZL.NPRFZLSTA.nprfzlkcv"); // nprfzlkcv
            ViewBag.PStatusDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.PersonStatus, eventType, "NPR.DLO.FZL.NPRFZLSTA.nprfzlsts"); // nprfzlsts

            ViewBag.PStatusStatusReasonDDL = service.GetDDL_EISPPTblElementNomWithRules(eventType, "NPR.DLO.FZL.NPRFZLSTA.nprfzlosn", ".nprfzlosn"); // nprfzlosn 

            ViewBag.EntityGroupDDL = EisppDropDownVM(service.GetDDL_EISPPTblElement(EisppTableCode.EntityGroup)); // nfljrdstt
            ViewBag.EntityStatusDDL = EisppDropDownVM(service.GetDDL_EISPPTblElement(EisppTableCode.EntityStatus)); // nfljrdstt
            ViewBag.EntityTypeDDL = EisppDropDownVM(service.GetDDL_EISPPTblElement(EisppTableCode.EntityType));  //nflvid

            ViewBag.MeasureTypeDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.MeasureType, eventType, "NPR.DLO.FZL.MPP.mppvid");
            ViewBag.MeasureStatusDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.MeasureStatus, eventType, "NPR.DLO.FZL.MPP.mppste");

            ViewBag.SrokTypeDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.SrokType, eventType, "SRK.srkvid");

            ViewBag.CaseCodeId_ddl = EisppDropDownVM(nomService.GetDropDownList<CaseCode>());
            ViewBag.MeasureInstitutionTypeId_ddl = nomService.GetDropDownList<InstitutionType>();


            // Наказания
            var punishmentTypeDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.PunishmentType, eventType, "NPR.DLO.FZL.NKZ.nkztip"); // nkztip 208 Типове наказания
            punishmentTypeDDL.Flags = 3;
            ViewBag.PunishmentTypeDDL = punishmentTypeDDL;
            ViewBag.PunishmentKindDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.PunishmentKind, eventType, "NPR.DLO.FZL.NKZ.nkzvid"); // nkzvid 209 Видове наказания
            ViewBag.ServingTypeDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.ServingType, eventType, "NPR.DLO.FZL.NKZ.nkzncn"); // nkzncn 210	Начини на изтърпяване на наказание	
            ViewBag.PunishmentActivityDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.PunishmentActivity, eventType, "NPR.DLO.FZL.NKZ.nkzakt"); // nkzakt 211	Активности на наказание	
            ViewBag.PunishmentRegimeDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.PunishmentRegime, eventType, "NPR.DLO.FZL.NKZ.nkzrjm"); // nkzrjm 234 Видове режими на изтърпяване на наказание
            // Пробация
            ViewBag.ProbationMeasureTypeDDL = EisppDropDownVM(service.GetDDL_EISPPTblElement(EisppTableCode.ProbationMeasureType));  //pbcvid
            ViewBag.MeasureUnitDDL = EisppDropDownVM(service.GetDDL_EISPPTblElement(EisppTableCode.MeasureUnit));  //pbcmered

            //Харктеристики
            ViewBag.FeatureTypeDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.FeatureType,eventType, "SBH.sbhvid");  //sbhvid
            ViewBag.FeatureValDDL = service.GetDDL_FeatureValTblElementWithRules(eventType, "SBH.sbhstn", ((EisppDropDownVM)ViewBag.CountriesOtherDDL).DDList);  //sbhstn 
            if (model.EventFeature?.FeatureType == FeatureType.SentenceType)
            {
                ((EisppDropDownVM)ViewBag.FeatureValDDL).Flags = 3;
            }

            ViewBag.CaseSessionActIdDDL = EisppDropDownVM(service.CaseSessionActDDL(model.CaseId, null, null, null), 30);
            ViewBag.DocumentTypeDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.DocumentType, eventType, "sbedkpvid"); // sbedkpvid 224 или 11993

            //Статистически данни за субект на престъпление CrimeSubjectStatisticData
            ViewBag.ЕthnicGroupDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.ЕthnicGroup, eventType, "NPR.DLO.NPRFZLPNE.SBC.sbcetn"); // sbcetn 314
            ViewBag.EducationDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.Education, eventType, "NPR.DLO.NPRFZLPNE.SBC.sbcobr"); // sbcobr 311
            ViewBag.LawfulAgeDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.LawfulAge, eventType, "NPR.DLO.NPRFZLPNE.SBC.sbcple"); // sbcple 309
            ViewBag.RelapsDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.Relaps, eventType, "NPR.DLO.NPRFZLPNE.SBC.sbcrcd"); // sbcrcd 308
            ViewBag.MeritalStatusDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.MeritalStatus, eventType, "NPR.DLO.NPRFZLPNE.SBC.sbcspj"); // sbcspj 310
            ViewBag.LaborActivityDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.LaborActivity, eventType, "NNPR.DLO.NPRFZLPNE.SBC.sbctrd"); // sbctrd 312
            ViewBag.OccupationDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.Occupation, eventType, "NPR.DLO.NPRFZLPNE.SBC.sbcznq"); // sbcznq  nmk_fzlpne_znt 1504
            ViewBag.FormerRegistrationsDDL = service.GetDDL_EISPPTblElementWithRules(EisppTableCode.FormerRegistrations, eventType, "NPR.DLO.NPRFZLPNE.SBC.sbcrge"); // sbcrge 12478
        }
        private void SetViewBag_EventType(EisppEventVM model)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForEisppEventEdit(model.CaseId).DeleteOrDisableLast();

            ViewBag.EventType_ddl = service.GetDDL_EISPPEventType(model.CaseCodeId, model.CaseTypeId);
            ViewBag.CasePersonId_ddl = casePersonService.GetForEispp(model.CaseId);
            var OnePersonEventDDL = service.GetDDL_EISPPTblElement(EisppTableCode.OnePersonEvent).Where(x => x.Value != "0");
            ViewBag.OnePersonEvent_json = JsonConvert.SerializeObject(OnePersonEventDDL);
            ViewBag.ConnectedCaseId_ddl = service.GetDDL_ConnectedCases(model.CaseId);
            ViewBag.ExactCaseType_ddl = service.GetDDL_EISPPTblElement(EisppTableCode.ExactCaseType);
            ViewBag.CaseSessionActId_ddl = service.CaseSessionActDDL(model.CaseId, null, null, null);
        }

        private void SetViewBag_Index(int caseId)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForEisppEvents(caseId).DeleteOrDisableLast();
            var aCase = service.GetById<Case>(caseId);
            ViewBag.EventTypeId_ddl = service.GetDDL_EISPPEventType(aCase.CaseCodeId ?? 0, aCase.CaseTypeId);
            ViewBag.SessionActId_ddl = service.CaseSessionActDDL(caseId, null, null, null);
            ViewBag.LinkType_ddl = service.GetLinkTypeDDL();
        }
        private void SetViewBag_IndexAll(int courtId)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForEisppEventsCourt(courtId).DeleteOrDisableLast();
            ViewBag.EventTypeId_ddl = service.GetDDL_EISPPEventType(0, 0);
        }
        [AcceptVerbs("Get", "Post")]
        public IActionResult VerifyEISPPNumber(string EISPPNumber)
        {
            string err = "Грешна контролна сума";
            if (EISPPNumber?.Length == 14)
            {
                var checkSum = service.CheckSum(EISPPNumber);
                if (checkSum == EISPPNumber.Substring(12, 2))
                    return Json(true);
            }
            return Json(err);
        }
    }
}