// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Rotativa.Extensions;

namespace IOWebApplication.Controllers
{
  public class CaseSelectionProtokolController : BaseController
  {
    private readonly ICaseSelectionProtokolService service;
    private readonly INomenclatureService nomService;
    private readonly ICourtDutyService courtDutyService;
    private readonly ICourtGroupService courtGroupService;
    private readonly ICourtDepartmentService courtDepartmentservice;
    private readonly ICourtLoadPeriodService courtLoadPeriodService;
    private readonly ICdnService cdnService;
    private readonly ICommonService commonService;

    public CaseSelectionProtokolController(
        ICaseSelectionProtokolService _service,
        INomenclatureService _nomService,
        ICourtDutyService _courtDutyService,
        ICourtGroupService _courtGroupService,
        ICourtDepartmentService _courtDepartmentservice,
        ICourtLoadPeriodService _courtLoadPeriodService,
        ICdnService _cdnService,
        ICommonService _commonService)
    {
      service = _service;
      nomService = _nomService;
      courtDutyService = _courtDutyService;
      courtGroupService = _courtGroupService;
      courtDepartmentservice = _courtDepartmentservice;
      courtLoadPeriodService = _courtLoadPeriodService;
      cdnService = _cdnService;
      commonService = _commonService;
    }

    public IActionResult Index(int id)
    {
      if (!CheckAccess(service, SourceTypeSelectVM.CaseSelectionProtokol, null, AuditConstants.Operations.View, null))
      {
        return Redirect_Denied();
      }

      ViewBag.caseId = id;
      ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(id);
      SetHelpFile(HelpFileValues.CaseLawunit);
      return View();
    }
    public IActionResult MakeLoadPeriodForAll()
    {
      courtLoadPeriodService.MakeLoadPeriodForAll();
      return null;
    }

    [HttpPost]
    public IActionResult ListData(IDataTablesRequest request, int caseId)
    {
      var data = service.CaseSelectionProtokol_Select(caseId);
      return request.GetResponse(data);
    }

    void SetViewBag(int courtId, int caseId)
    {
      var roles = service.AvailableJudgeRolesForFelect_SelectForDropDownList(caseId);
      ViewBag.JudgeRoleId_ddl = roles;
      ViewBag.countAvailableJudgeRole = roles.Count();
      ViewBag.SelectionModes = nomService.GetDropDownList<SelectionMode>(false);
      ViewBag.CourtDutyId_ddl = courtDutyService.CourtDuty_SelectForDropDownList(courtId);
      ViewBag.CourtDepartmentId_ddl = courtDepartmentservice.CourtDepartment_Select(courtId, null).ToSelectList(x => x.Id, x => x.Label).ToList();
      //ViewBag.SpecialityId_ddl = nomService.GetDropDownList<Speciality>(false, true);
      ViewBag.SpecialityId_ddl = nomService.GetDDL_Specyality_ByLowUnit_Type(NomenclatureConstants.LawUnitTypes.Jury, false, true);
      ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSelectionProtokol(caseId);
      SetHelpFile(HelpFileValues.CaseLawunit);
    }

    public IActionResult Add(int caseId)
    {
      if (!CheckAccess(service, SourceTypeSelectVM.CaseSelectionProtokol, null, AuditConstants.Operations.Append, caseId))
      {
        return Redirect_Denied();
      }
      var tcase = service.GetById<Case>(caseId);
      var model = new CaseSelectionProtokolVM()
      {
        CaseId = caseId,
        CourtId = tcase.CourtId,
        CaseGroupId = tcase.CaseGroupId,
        CaseCodeId = tcase.CaseCodeId ?? 0,
        CourtGroupId = tcase.CourtGroupId,
        SelectionModeId = 1
      };
      SetViewBag(model.CourtId, caseId);
      var caseGroups = service.CaseGroup_WithLawUnits(model.CourtId, "");
      if (caseGroups.Length == 0)
      {
        SetErrorMessage("Няма налични съдии за избор. Бутон \"Запис\" ще генерира протокол за липса на съдии!");
      }

      if (service.HsaUnsignedProtocol(caseId))
      {
        SetErrorMessage("Не може да бъде създаден нов протокол, докато има неподписан такъв.");
        return RedirectToAction("Index", new { id = caseId });
      }
      else
      {
        if (ViewBag.countAvailableJudgeRole > 0)
          return View(nameof(Edit), model);
        else
        {
          SetErrorMessage("Съдебният състав по делото е запълнен. Не могат да бъдат добавени повече позиции.");
          return RedirectToAction("Index", new { id = caseId });
        }
      }

    }

    private void SetSelectedTabBySelectionMode(CaseSelectionProtokolVM model)
    {
      switch (model.SelectionModeId)
      {
        case NomenclatureConstants.SelectionMode.SelectByGroups:
          model.SelectedTab = "#tabSelectByGroup";
          break;
        case NomenclatureConstants.SelectionMode.ManualSelect:
          model.SelectedTab = "#tabManualSelect";
          break;
        case NomenclatureConstants.SelectionMode.SelectByDuty:
          model.SelectedTab = "#tabSelectByDuty";
          break;
      }
    }

    void ValidateModel(CaseSelectionProtokolVM model)
    {

      if (model.SelectionModeId == NomenclatureConstants.SelectionMode.ManualSelect && model.Description == null)
      {
        ModelState.AddModelError("", "Моля, въведете основание за ръчен избор");
      }
      if (model.SelectionModeId == NomenclatureConstants.SelectionMode.ManualSelect && model.Description != null && NomenclatureConstants.JudgeRole.JuriRolesList.Contains(model.JudgeRoleId))
      {
        if (service.GetActiveLawUnits(model.CaseId).Contains(model.LawUnits.FirstOrDefault().LawUnitId))
          ModelState.AddModelError("", "Съдебният заседател вече е добавен в делото ");
      }


      if (NomenclatureConstants.JudgeRole.JuriRolesList.Contains(model.JudgeRoleId) && model.SelectionModeId != NomenclatureConstants.SelectionMode.ManualSelect)
      {
        var jury = service.LawUnit_LoadJury(model.CourtId, model.CaseId, model.SpecialityId);
        if (jury.Count() == 0)
        {
          ModelState.AddModelError("", "Няма повече налични съдебни заседатели");
        }
      }
      if (NomenclatureConstants.JudgeRole.JuriRolesList.Contains(model.JudgeRoleId))
      // if (model.JudgeRoleId == NomenclatureConstants.JudgeRole.Jury || model.JudgeRoleId == NomenclatureConstants.JudgeRole.ReserveJury)
      {

        string errorDescription = "";
        for (int i = 0; i < model.LawUnits.Count(); i++)
        {
          var lawUnit = model.LawUnits[i];

          if (lawUnit.StateId == NomenclatureConstants.SelectionProtokolLawUnitState.AddedManually && lawUnit.LawUnitId <= 0)
          {
            ModelState.AddModelError($"{nameof(CaseSelectionProtokolVM.LawUnits)}[{i}].{nameof(CaseSelectionProtokolLawUnitVM.LawUnitId)}", "Моля, въведете участник");
          }




          if (lawUnit.StateId == NomenclatureConstants.SelectionProtokolLawUnitState.Exclude && lawUnit.Description == null)
          {
            errorDescription = "Моля, въведете причина за неучастие на " + lawUnit.LawUnitFullName;
            ModelState.AddModelError($"{nameof(CaseSelectionProtokolVM.LawUnits)}[{i}].{nameof(CaseSelectionProtokolLawUnitVM.Description)}", errorDescription);
          }


        }
      }
      else
      {
        string errorDescription = "";
        bool hasActiveCourtGroup = false;
        bool hasCaseGroup = false;
        for (int i = 0; i < model.LawUnits.Count(); i++)
        {
          var lawUnit = model.LawUnits[i];

          //Проверява дали има валидни съдии от групата на делото и от някое направление добавени
          if (lawUnit.CaseGroupId != null)
            hasCaseGroup = true;

          if (lawUnit.CaseGroupId == null && NomenclatureConstants.SelectionProtokolLawUnitState.ActiveState.Contains(lawUnit.StateId))
            hasActiveCourtGroup = true;

          if (lawUnit.StateId == NomenclatureConstants.SelectionProtokolLawUnitState.Exclude && lawUnit.Description == null)
          {
            errorDescription = "Моля, въведете причина за неучастие на " + lawUnit.LawUnitFullName;
            ModelState.AddModelError($"{nameof(CaseSelectionProtokolVM.LawUnits)}[{i}].{nameof(CaseSelectionProtokolLawUnitVM.Description)}", errorDescription);
          }
          //if (lawUnit.StateId == NomenclatureConstants.SelectionProtokolLawUnitState.AddedManually && lawUnit.Description == null)
          //{
          //    errorDescription = "Моля, въведете причина за ръчен избор на " + lawUnit.LawUnitFullName;
          //    ModelState.AddModelError($"{nameof(CaseSelectionProtokolVM.LawUnits)}[{i}].{nameof(CaseSelectionProtokolLawUnitVM.Description)}", errorDescription);
          //}
        }

        bool hasActiveLawUnit = model.LawUnits.Where(x => NomenclatureConstants.SelectionProtokolLawUnitState.ActiveState.Contains(x.StateId) && x.LawUnitId > 0).Any();
        if (hasActiveLawUnit == false)
        {
          string idStr = "";
          foreach (var item in model.LawUnits)
          {
            idStr = idStr + item.LawUnitId.ToString() + ",";
          }
          var available_users_in_otherGroups = service.Return_Available_CaseGroup_forAdditionalSelect(idStr, "", model.CaseId);
          if (available_users_in_otherGroups.Count > 0)
          {
            ModelState.AddModelError("", "Въведете поне едно активно лице");
          }
          else { model.IsProtokolNoSelection = true; }
        }

        if (hasCaseGroup == true && hasActiveCourtGroup == true)
          ModelState.AddModelError("", "Има активен съдия от групата на делото и добавен съдия от отделение");
      }
    }

    void SetDataError(CaseSelectionProtokolVM model)
    {
      SetSelectedTabBySelectionMode(model);
      SetViewBagLawUnits(model.SelectionModeId);
      if (model.SelectionModeId == NomenclatureConstants.SelectionMode.ManualSelect)
      {
        for (int i = 0; i < model.LawUnits.Count(); i++)
        {
          model.LawUnits[i].LawUnitTypeId = NomenclatureConstants.LawUnitTypes.Judge.ToString();
          if (NomenclatureConstants.JudgeRole.JuriRolesList.Contains(model.JudgeRoleId))
            //if (model.JudgeRoleId == NomenclatureConstants.JudgeRole.Jury || model.JudgeRoleId == NomenclatureConstants.JudgeRole.ReserveJury)

            model.LawUnits[i].LawUnitTypeId = NomenclatureConstants.LawUnitTypes.Jury.ToString();
        }
      }
    }

    [RequestFormLimits(ValueCountLimit = 15000)]
    [HttpPost]
    public IActionResult Edit(CaseSelectionProtokolVM model)
    {
      bool ExitByTime = false;
      SetViewBag(model.CourtId, model.CaseId);
      ViewBag.RoleId = model.JudgeRoleId;
      ValidateModel(model);


      if (!ModelState.IsValid)
      {
        SetDataError(model);
        return View(nameof(Edit), model);
      }
      var currentId = model.Id;
      string errorMessage = "";
      bool save = true;
      if (NomenclatureConstants.JudgeRole.JuriRolesList.Contains(model.JudgeRoleId))
      {
        int protokolId = service.Select_Create_AddJuryProtocol(model, ref errorMessage);
        if (protokolId > 0)
        {
          model.Id = protokolId;
          return RedirectToAction("SignDoc", new { id = model.Id });
        }
        else
        {
          save = false;
        }
      }
      else
      {
        
        if (!service.CaseSelectionProtokol_SaveData(model, ref errorMessage))
          save = false;
      }

      if (save == true)
      {
        SetAuditContext(service, SourceTypeSelectVM.CaseSelectionProtokol, model.Id, currentId == 0);
        //this.SaveLogOperation(currentId == 0, model.Id);
        SetSuccessMessage(MessageConstant.Values.SaveOK);
      }
      else
      {
        if (errorMessage == "")
          errorMessage = MessageConstant.Values.SaveFailed;
        if (ExitByTime)
        {
          errorMessage = MessageConstant.Values.TimeoutSelectProtokol;
        }
        SetErrorMessage(errorMessage);
        SetDataError(model);

        return View(nameof(Edit), model);
      }
      //В случай няма наличен състав се отива на подпис

      var saved_model = service.CaseSelectionProtokol_Preview(model.Id);
      var comparentmentLisCount = service.GetJudgeComprentmetList((saved_model.SelectedLawUnitId ?? 0), saved_model.CourtId, saved_model.CaseId).Count;

      if (comparentmentLisCount > 1)
      {
        return RedirectToAction("PreviewDoc", new { id = model.Id });
      }
      else
      {
        var res = service.CaseSelectionProtokol_UpdateBeforeDocForSign(saved_model);
        return RedirectToAction("SignDoc", new { id = saved_model.Id });

      }

      //    return RedirectToAction("PreviewDoc", new { id = model.Id });
    }

    void SetViewBagLawUnits(int selectionMode)
    {
      ViewBag.states = nomService.GetSelectionLawUnitState(selectionMode);
    }

    public IActionResult LawUnits_LoadByGroup(int caseId, int judgeRoleId, int specialityId)
    {
      var caseModel = nomService.GetById<Case>(caseId);
      IEnumerable<CaseSelectionProtokolLawUnitVM> model = null;
      ViewBag.RoleId = judgeRoleId;
      switch (judgeRoleId)
      {
        case NomenclatureConstants.JudgeRole.Judge:
        case NomenclatureConstants.JudgeRole.JudgeReporter:
        case NomenclatureConstants.JudgeRole.ReserveJudge:
        case NomenclatureConstants.JudgeRole.ExtJudge:
          model = service.LawUnit_LoadJudge(caseModel.CourtGroupId ?? 0, caseId, userContext.CourtId,judgeRoleId);
          break;
        case NomenclatureConstants.JudgeRole.Jury:
        case NomenclatureConstants.JudgeRole.ReserveJury:
        case NomenclatureConstants.JudgeRole.ExtJury:
          model = service.LawUnit_LoadJury(caseModel.CourtId, caseModel.Id, specialityId);
          break;
      }
      SetViewBagLawUnits(NomenclatureConstants.SelectionMode.SelectByGroups);
      return PartialView("_LoadedLawUnits", model);
    }

    public IActionResult LawUnits_LoadByDuty(int courtDutyId, int caseId)
    {
      ViewBag.RoleId = NomenclatureConstants.JudgeRole.JudgeReporter;
      var model = service.LawUnit_LoadByCourtDutyId(courtDutyId, caseId, userContext.CourtId, NomenclatureConstants.JudgeRole.JudgeReporter);
      //ViewData.TemplateInfo.HtmlFieldPrefix = model.GetPath;
      SetViewBagLawUnits(NomenclatureConstants.SelectionMode.SelectByDuty);
      return PartialView("_LoadedLawUnits", model);
    }

    public IActionResult LawUnits_AddNew(string lawUnitTypeId)
    {
      var model = new CaseSelectionProtokolLawUnitVM()
      {
        IsLoaded = false,
        Index = 0,
        StateId = NomenclatureConstants.SelectionProtokolLawUnitState.AddedManually,
        LoadIndex = 100,
        SelectedFromCaseGroup = false,
        EnableState = true,
        LawUnitTypeId = lawUnitTypeId
      };
      ViewData.TemplateInfo.HtmlFieldPrefix = model.GetPath;
      SetViewBagLawUnits(NomenclatureConstants.SelectionMode.ManualSelect);
      if (lawUnitTypeId == NomenclatureConstants.LawUnitTypes.Judge.ToString())
      {
        ViewBag.RoleId = NomenclatureConstants.JudgeRole.Judge;
      }
      else
      {
        ViewBag.RoleId = NomenclatureConstants.JudgeRole.Jury;
      }
      var result = new List<CaseSelectionProtokolLawUnitVM>() { model };

      //  return PartialView("_LawUnitsItem", model);
      return PartialView("_LoadedLawUnits", result);
    }

    public IActionResult Preview(int id)
    {
      var model = service.CaseSelectionProtokol_Preview(id);
      ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSelectionProtokol(model.CaseId);
      return View(model);
    }

    public IActionResult PreviewDoc(int id)
    {
      if (!CheckAccess(service, SourceTypeSelectVM.CaseSelectionProtokol, id, AuditConstants.Operations.View))
      {
        return Redirect_Denied();
      }
      var model = service.CaseSelectionProtokol_Preview(id);
      ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSelectionProtokol(model.CaseId);
      ViewBag.comparement_ddl = service.GetJudgeComprentmetList((model.SelectedLawUnitId ?? 0), model.CourtId, model.CaseId);

      return View("PreviewDoc", model);
    }

    [HttpPost]
    public IActionResult PreviewDoc(CaseSelectionProtokolPreviewVM model)
    {
      var res = service.CaseSelectionProtokol_UpdateBeforeDocForSign(model);
      return RedirectToAction("SignDoc", new { id = model.Id });
    }

    public async Task<IActionResult> SignDoc(int id)
    {
      var protokolModel = service.CaseSelectionProtokol_Preview(id);

      string html = await this.RenderPartialViewAsync("~/Views/CaseSelectionProtokol/", "Preview.cshtml", protokolModel, true);
      var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true).GetByte(this.ControllerContext);
      var pdfRequest = new CdnUploadRequest()
      {
        SourceType = SourceTypeSelectVM.CaseSelectionProtokol,
        SourceId = id.ToString(),
        FileName = "selectionProtokol.pdf",
        ContentType = "application/pdf",
        Title = $"Протокол за разпределение {protokolModel.SelectedLawUnitName} ({protokolModel.JudgeRoleName})",
        FileContentBase64 = Convert.ToBase64String(pdfBytes)
      };
      if (await cdnService.MongoCdn_AppendUpdate(pdfRequest))
      {
        return RedirectToAction(nameof(SendForSign), new { id = id });
      }
      else
      {
        SetErrorMessage("Проблем при създаване на протокол!");
        return RedirectToAction(nameof(PreviewDoc), new { id = id });
      }
    }

    public IActionResult SendForSign(int id)
    {
      Uri urlSuccess = new Uri(Url.Action("SignedDoc", "CaseSelectionProtokol", new { id = id }), UriKind.Relative);
      Uri url = new Uri(Url.Action("PreviewDoc", "CaseSelectionProtokol", new { id = id }), UriKind.Relative);

      var signModel = new Core.Models.SignPdfInfo()
      {
        SourceId = id.ToString(),
        SourceType = SourceTypeSelectVM.CaseSelectionProtokol,
        DestinationType = SourceTypeSelectVM.CaseSelectionProtokol,
        Location = "Sofia",
        Reason = "Test",
        SuccessUrl = urlSuccess,
        CancelUrl = url,
        ErrorUrl = url
      };
      var protokolModel = service.CaseSelectionProtokol_Preview(id);
      if (protokolModel != null)
      {
        signModel.SignerName = protokolModel.UserName;
        signModel.SignerUic = protokolModel.UserUIC;
      }
      return View("_SignPdf", signModel);
    }

    public IActionResult SignedDoc(int id)
    {
      service.CaseSelectionProtokol_UpdateBeforeAfterSign(id);
      SetSuccessMessage("Протоколът беше подписан успешно!");
      return RedirectToAction(nameof(PreviewDoc), new { id = id });
    }

    public IActionResult CaseSelectionProtokolList()
    {
      var model = new CaseSelectionProtokolFilterVM();
      ViewBag.JudgeRoleId_ddl = service.SelectJudgeRole_ForDropDownList();
      ViewBag.SelectionModeId_ddl = service.SelectSelectionMode_ForDropDownList();
      ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
      ViewBag.ProtokolState_ddl = nomService.GetDropDownList<SelectionProtokolState>();
      ViewBag.CourtGroupId_ddl = nomService.GetDDL_CourtGroup(userContext.CourtId);
      ViewBag.LoadGroupLinkId_ddl = nomService.GetDDL_LoadGroupLink();
      SetHelpFile(HelpFileValues.AssignmentInfo);
      return View(model);
    }

    [HttpPost]
    public IActionResult ListDataReport(IDataTablesRequest request, CaseSelectionProtokolFilterVM model)
    {
      var data = service.CaseSelectionProtokol_SelectForReport(userContext.CourtId, model);

      return request.GetResponse(data);
    }

    public IActionResult LoadByCaseGroup(string idStr, string groups, int caseId)
    {
      var ddl_list = service.Return_Available_CaseGroup_forAdditionalSelect(idStr, groups, caseId);

      ViewBag.CaseGroupId_ddl = ddl_list;
      CaseSelectionProtokolVM model = new CaseSelectionProtokolVM();
      model.IdStr = idStr;
      model.CaseId = caseId;

      if (ddl_list.Count > 0)
      {
        return PartialView(model);
      }
      else { return Content("empty"); }
    }

    [HttpPost]
    public IActionResult LoadByCaseGroup(CaseSelectionProtokolVM model)
    {
      IEnumerable<CaseSelectionProtokolLawUnitVM> modelView = null;
      ViewBag.RoleId = NomenclatureConstants.JudgeRole.Judge;
      modelView = service.LawUnit_LoadJudgeByCaseGroup(userContext.CourtId, model.CaseGroupId, model.IdStr ?? "", model.CaseId,model.JudgeRoleId);
      SetViewBagLawUnits(NomenclatureConstants.SelectionMode.SelectByGroups);
      return PartialView("_LoadedLawUnits", modelView);
    }
    public IActionResult IndexSpr()
    {
      var model = new CourtLawUnitFilter()
      {
        DateFrom = NomenclatureExtensions.GetStartYear(),
        DateTo = NomenclatureExtensions.GetEndYear(),
      };
      return View(model);
    }

    [HttpPost]
    public IActionResult ListDataJuryDays(IDataTablesRequest request, int? LawUnitId, DateTime fromData, DateTime toData)
    {
      var data = service.JuryYearDays_Select(userContext.CourtId, DateTime.Now.Year, fromData, toData, LawUnitId);
      return request.GetResponse(data);
    }



    public IActionResult LawUnitGroupReport()
    {
      var model = new CaseSelectionProtokoLUGrouplFilterVM();


      ViewBag.GroupId_ddl = service.GetCourtGroups(userContext.CourtId);


      return View(model);
    }
    [HttpPost]
    public IActionResult ListLawUnitGroup(IDataTablesRequest request, CaseSelectionProtokoLUGrouplFilterVM model)
    {
      var data = service.LawUnitReportByGroup(userContext.CourtId, model.GroupId, model.LawUnitID);
      Dictionary<string, object> addParams = new Dictionary<string, object>();
      addParams.Add("total", data.Sum(x => x.CaseCount));
      return request.GetResponse(data, null, addParams);

    }
  }
}