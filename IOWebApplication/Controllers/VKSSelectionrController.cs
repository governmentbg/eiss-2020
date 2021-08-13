using System;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IOWebApplication.Components;
using IOWebApplication.Infrastructure.Constants;
using System.Linq;
using IOWebApplication.Infrastructure.Data.Models.VKS;
using System.Threading.Tasks;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Rotativa.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Contracts;
using System.Collections.Generic;

namespace IOWebApplication.Controllers
{
  public class VKSSelectionController : BaseController
  {
    private readonly ICommonService commonService;
    private readonly INomenclatureService nomService;
    private readonly IVKSSelectionService vksSelectionService;
    private readonly ICdnService cdnService;

    public VKSSelectionController(ICommonService _commonService, INomenclatureService _nomService, IVKSSelectionService _vksSelectionService, ICdnService _cdnService)
    {
      commonService = _commonService;
      nomService = _nomService;
      vksSelectionService = _vksSelectionService;
      cdnService = _cdnService;
    }


    public IActionResult Index()
    {
      if (!userContext.IsUserInFeature(AccountConstants.Features.Modules.VKSAdministration))
      {
        return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);

      }
      ViewBag.CanEdit = (vksSelectionService.GetUserOtdelenieID(userContext.LawUnitId) < 0);

      return View();
    }
    public IActionResult IndexTO()
    {
      if (!userContext.IsUserInFeature(AccountConstants.Features.Modules.VKSAdministration))
      {
        return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);

      }
      ViewBag.CanEdit = (vksSelectionService.GetUserOtdelenieID(userContext.LawUnitId) < 0);
      return View();
    }

    [HttpPost]
    public IActionResult ListData(IDataTablesRequest request)
    {

      var data = vksSelectionService.VKSSelectionsHeader_GetList(true);

      return request.GetResponse(data);
    }
    [HttpPost]
    public IActionResult ListDataTO(IDataTablesRequest request)
    {

      var data = vksSelectionService.VKSSelectionsHeader_GetList(false);

      return request.GetResponse(data);
    }

    public IActionResult ProtocolList(int id)
    {
      if (!userContext.IsUserInFeature(AccountConstants.Features.Modules.VKSAdministration))
      {
        return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);

      }
      ViewBag.selectionId = id;
      ViewBag.Title = vksSelectionService.GetSelectionTitle(id);

      var data = vksSelectionService.VKSSelectionProtocolList(id);
      ViewBag.ShowAddButton = data.Where(x => x.IsSigned == false).Count() == 0;
      ViewBag.CanEdit = vksSelectionService.CanEditCurrentOtdelenie(userContext.LawUnitId, vksSelectionService.GetCourtDepartmentIdFromSelection(id));

      return View();
    }

    [HttpPost]
    public IActionResult ListDataProtocol(IDataTablesRequest request,int selectionId)
    {

      var data = vksSelectionService.VKSSelectionProtocolList(selectionId);
      ViewBag.ShowAddButton = data.Where(x => x.IsSigned == false).Count()== 0;

      return request.GetResponse(data);
    }
    public IActionResult Preview(int id)
    {
      if (!userContext.IsUserInFeature(AccountConstants.Features.Modules.VKSAdministration))
      {
        return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);

      }
      ViewBag.HasSignedProtocol = vksSelectionService.SelectionHasSignedProtocol(id);
      ViewBag.Title = vksSelectionService.GetSelectionTitle(id);
      var model = vksSelectionService.GetSelectionCalendar(id);
      ViewBag.PeriodNo = model.PeriodNo;
      ViewBag.CanEdit = vksSelectionService.CanEditCurrentOtdelenie(userContext.LawUnitId, model.CourtDepartmentId);
      return View(model);
    }
    [HttpPost]
    public IActionResult Preview(VksSelectionCalendarVM model)
    {

      var saved = vksSelectionService.Save_MonthSessionsDates(model);
      
      if (saved == false)
      {
        SetErrorMessage("Проблем при запис");
      }
      else
      {
        SetSuccessMessage(MessageConstant.Values.SaveOK);
      }
      ViewBag.HasSignedProtocol = vksSelectionService.SelectionHasSignedProtocol(model.Id);
      model = vksSelectionService.GetSelectionCalendar(model.Id);
       return RedirectToAction("Preview", new { id = model.Id });
    }

    public IActionResult PreviewEDIT(int id)
    {
      if (!userContext.IsUserInFeature(AccountConstants.Features.Modules.VKSAdministration))
      {
        return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);

      }
      ViewBag.HasSignedProtocol = vksSelectionService.SelectionHasSignedProtocol(id);
      ViewBag.Title = vksSelectionService.GetSelectionTitle(id);
      var model = vksSelectionService.GetSelectionCalendar(id);
      ViewBag.PeriodNo = model.PeriodNo;
      ViewBag.CanEdit = vksSelectionService.CanEditCurrentOtdelenie(userContext.LawUnitId, model.CourtDepartmentId);
      return View(model);
    }
    [HttpPost]
    public IActionResult PreviewEDIT(VksSelectionCalendarVM model)
    {

      var saved = vksSelectionService.Save_MonthSessionsDatesEDIT(model);

      if (saved == false)
      {
        SetErrorMessage("Проблем при запис");
      }
      else
      {
        SetSuccessMessage(MessageConstant.Values.SaveOK);
      }
      ViewBag.HasSignedProtocol = vksSelectionService.SelectionHasSignedProtocol(model.Id);
      model = vksSelectionService.GetSelectionCalendar(model.Id);
      return View(model);
    }
    public IActionResult PreviewProtocol(int id)
    {
      if (!userContext.IsUserInFeature(AccountConstants.Features.Modules.VKSAdministration))
      {
        return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);

      }
      //var r= vksSelectionService.GenerateSelectionSTAFF(57);
      var model = vksSelectionService.GetSelectionCalendar(id);
      return View(model);
    }



    [HttpPost]
    public IActionResult ListDataSelectionLawunits(IDataTablesRequest request, int selectionId)
    {
    
      var data = vksSelectionService.VKSSelections_Lawunit_GetList(selectionId);

      return request.GetResponse(data);
    }

    public IActionResult AddUnknownLawUnits(int id)
    {
      var selection = vksSelectionService.VKSSelection_Get(id);
      var updated = vksSelectionService.AddUnknownLawUnits(selection);
      if (updated == false)
      {
        SetErrorMessage("Проблем при запис");
      }
      return RedirectToAction("LawUnits", new { id = id });
    }
    public IActionResult DeleteLawUnit(int id)
    {
      var selectionId = vksSelectionService.DeleteLawUnits(id);

      return RedirectToAction("LawUnits", new { id = selectionId });
    }


    public IActionResult GenerateSelectionStaff(int id)
    {
      var selectionId = vksSelectionService.GenerateSelectionSTAFF(id);

      return RedirectToAction("Preview", new { id = id });
    }
    public IActionResult LawUnitsUpdate(int id)
    {
      var selection = vksSelectionService.VKSSelection_Get(id);
      var updated = vksSelectionService.SelectionLawUnitsUpdate(selection);
      if (updated == false)
      {
        SetErrorMessage("Проблем при запис");
      }
      return RedirectToAction("LawUnits", new { id = id });
    }

    public IActionResult LawunitReplace(int id)
    {
      var model = vksSelectionService.GetSelectionLawunitByID(id);
      ViewBag.ReplacedLawunitId_ddl = vksSelectionService.GetDDL_ReplacingLawunits(model.VksSelectionId);
      return View(model);
    }
    [HttpPost]
    public IActionResult LawunitReplace(VksSelectionLawunit model)
    {
      if (model.ReplacedLawunitId < 0)
      {
        SetErrorMessage("Не е избран заместващ");
        ViewBag.ReplacedLawunitId_ddl = vksSelectionService.GetDDL_ReplacingLawunits(model.VksSelectionId);
        return View(model);
      }
      else
      {
        if (!vksSelectionService.ReplacedLawunitSave(model))
        {
          SetErrorMessage("Проблем при запис");
        }
        else
        {
          return RedirectToAction("LawUnits", new { id = model.VksSelectionId });
        }
      }
      return View(model);
    }
    public IActionResult LawUnits(int id)

    {
      if (!userContext.IsUserInFeature(AccountConstants.Features.Modules.VKSAdministration))
      {
        return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);

      }
      ViewBag.HasSignedProtocol = vksSelectionService.SelectionHasSignedProtocol(id);
      ViewBag.Title = vksSelectionService.GetSelectionTitle(id);
      var selection = vksSelectionService.VKSSelection_Get(id);
      ViewBag.PeriodNo = selection.VksSelectionHeader.PeriodNo;
      ViewBag.CanEdit = vksSelectionService.CanEditCurrentOtdelenie(userContext.LawUnitId, selection.CourtDepartmentId);
      return View(selection);
    }

    public IActionResult Edit(int id)
    {
      if (!userContext.IsUserInFeature(AccountConstants.Features.Modules.VKSAdministration))
      {
        return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);

      }
      SetViewBag();
      ViewBag.HasSignedProtocol = vksSelectionService.HeaderHasSignedProtocol(id);
      VksSelectionHeaderVM model = vksSelectionService.VKSSelectionsHeader_Get(id);
      ViewBag.Check_ddl = vksSelectionService.GetDDL_Otdelenie(model.KolegiaId);
      return View(model);
    }

    [HttpPost]
    public IActionResult Edit(VksSelectionHeaderVM model)
    {
      SetViewBag();
      var id = vksSelectionService.VKSSelectionHeader_Save(model);


      model = vksSelectionService.VKSSelectionsHeader_Get(id);


      if (id > 0)
      {
        this.SaveLogOperation(id == 0, model.Id);
        SetSuccessMessage(MessageConstant.Values.SaveOK);
        return RedirectToAction(nameof(Edit), new { id = model.Id });
      }
      else
      {
        SetErrorMessage(MessageConstant.Values.SaveFailed);
        return View(model);
      }

    }

    public IActionResult Add()
    {
      if (!userContext.IsUserInFeature(AccountConstants.Features.Modules.VKSAdministration))
      {
        return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);

      }
      SetViewBag();
      ViewBag.HasSignedProtocol = false;
      VksSelectionHeaderVM model = new VksSelectionHeaderVM();

      model.SelectionYear = DateTime.Now.Year;
      

      return View(nameof(Edit), model);

    }


    public IActionResult AddTO()
    {
      SetViewBag(false);
      ViewBag.HasSignedProtocol = false;
      VksSelectionHeaderVM model = new VksSelectionHeaderVM();

      model.SelectionYear = DateTime.Now.Year;


      return View("EditTO", model);

    }
    public IActionResult EditTO(int id)
    {
      if (!userContext.IsUserInFeature(AccountConstants.Features.Modules.VKSAdministration))
      {
        return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);

      }
      SetViewBag();

      VksSelectionHeaderVM model = vksSelectionService.VKSSelectionsHeader_Get(id);
      ViewBag.Check_ddl = vksSelectionService.GetDDL_Otdelenie(model.KolegiaId);
      return View(model);
    }


    [HttpPost]
    public IActionResult Add(VksSelectionHeaderVM model)
    {
      SetViewBag();


      if (vksSelectionService.Already_Exist(model))
      {
        SetErrorMessage("Съществува запис за периода");
        return View(nameof(Edit), model);
      }
      else
      {
        var cur_id = vksSelectionService.VKSSelectionHeader_Save(model);
        if (cur_id > 0)
        {
          this.SaveLogOperation(cur_id == 0, model.Id);
          SetSuccessMessage(MessageConstant.Values.SaveOK);
          return RedirectToAction(nameof(Edit), new { id = cur_id });
        }
        else
        {
          SetErrorMessage(MessageConstant.Values.SaveFailed);
          return View(nameof(Edit), model);
        }
      }

      return View(nameof(Edit), model);


    }
    [HttpPost]
    public IActionResult AddTO(VksSelectionHeaderVM model)
    {
      SetViewBag(false);


      if (vksSelectionService.Already_Exist(model))
      {
        SetErrorMessage("Съществува запис за периода");
        return View("EditTO", model);
      }
      else
      {
        var cur_id = vksSelectionService.VKSSelectionHeaderTO_Save(model);
        if (cur_id > 0)
        {
          this.SaveLogOperation(cur_id == 0, model.Id);
          SetSuccessMessage(MessageConstant.Values.SaveOK);
          return RedirectToAction("EditTO", new { id = cur_id });
        }
        else
        {
          SetErrorMessage(MessageConstant.Values.SaveFailed);
          return View("EditTO", model);
        }
      }

      return View("EditTO", model);


    }

    private void SetViewBag(bool ? Nakazatelno=true)
    {
      ViewBag.SelectionYear_ddl = vksSelectionService.GetDDL_SelectionYear();
      ViewBag.PeriodNo_ddl = vksSelectionService.GetDDL_SelectionPeriod();
      ViewBag.KolegiaId_ddl = vksSelectionService.GetDDL_Kolegia(userContext.CourtId, Nakazatelno);
     
      

    }

    public async Task<IActionResult> CreateProtocol(int id)
    {
      var protokolModel =  vksSelectionService.GetSelectionCalendar(id) ;
      var isDeletedUnsignedProtocols = vksSelectionService.DeleteUnsignedSelectionProtocols(id);
      
      string html = await this.RenderPartialViewAsync("~/Views/VKSSelection/", "PreviewProtocol.cshtml", protokolModel, true);
      var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true).GetByte(this.ControllerContext);

      VksSelectionProtocol protocol= new VksSelectionProtocol();
      protocol.DateGenerated = DateTime.Now;
      protocol.VksSelectionId = id;
      protocol.UserGeneratedId = userContext.UserId;
      protocol = vksSelectionService.Save_VksSelection_protocol(protocol);      



      var pdfRequest = new CdnUploadRequest()
      {
        SourceType = SourceTypeSelectVM.VksSelectionProtocol,
        SourceId = protocol.Id.ToString(),

        //FileName = "Протокол" +protocol.DateGenerated.Day.ToString()+"_"+ protocol.DateGenerated.Month.ToString() + "_" + protocol.DateGenerated.Year.ToString() + ".pdf",
        FileName = "Протокол" + protocol.DateGenerated.ToString("_dd_MM_yyyy")+".pdf",
       
        ContentType = "application/pdf",
        Title = $"Протокол за разпределение   м. {protokolModel.MonthList[0].SelectionMonthString.ToLower()} -  м. {protokolModel.MonthList[protokolModel.MonthList.Count() - 1].SelectionMonthString.ToLower()} {@protokolModel.SelectionYear} г.",
        FileContentBase64 = Convert.ToBase64String(pdfBytes)
      };
      if (await cdnService.MongoCdn_AppendUpdate(pdfRequest))
      {
        // return RedirectToAction(nameof(SendForSign), new { selectionId = id, protocilId=protocol.Id});
         return RedirectToAction("ProtocolList", new { id= id});
      }
      else
      {
        SetErrorMessage("Проблем при създаване на протокол!");
        return RedirectToAction("ProtocolList", new { id = id });
      }
    }

    public IActionResult SendForSign(int Id)
    {

      var protocol = vksSelectionService.Get_VksSelection_protocol(Id);
      ///////////
      Uri urlSuccess = new Uri(Url.Action("ProtocolSignUpdate", "VKSSelection", new { id = protocol.Id }), UriKind.Relative);
      Uri url = new Uri(Url.Action("ProtocolList", "VKSSelection", new { id = protocol.VksSelectionId }), UriKind.Relative);

      var signModel = new Core.Models.SignPdfInfo()
      {
        SourceId = Id.ToString(),
        SourceType = SourceTypeSelectVM.VksSelectionProtocol,
        DestinationType = SourceTypeSelectVM.VksSelectionProtocol,
        Location = "Sofia",
        Reason = "Test",
        SuccessUrl = urlSuccess,
        CancelUrl = url,
        ErrorUrl = url
      };
      
     
        signModel.SignerName = userContext.FullName;
       
      
      return View("_SignPdf", signModel);
    }

    public IActionResult ProtocolSignUpdate(int id)
    {
      var protocol = vksSelectionService.Get_VksSelection_protocol_byId(id);
      protocol.DateSigned = DateTime.Now;
      protocol.UserSignedId = userContext.UserId;
      var updated = vksSelectionService.Save_VksSelection_protocol(protocol);
      if (updated == null)
      {
        SetErrorMessage("Проблем при запис");
      }
      return RedirectToAction("ProtocolList", "VKSSelection", new { id = protocol.VksSelectionId });
    }

    public IActionResult GetSessionDayCalendar()

    {
      int[] LawUnits = new int[]{ 115, 22672 , 22745 ,48,117, 22686 };
      var selection = vksSelectionService.GetvksSessionDayCalendar(LawUnits,796);
      return View("_SessionDayCalendarList", selection);
    }

    public IActionResult MainSession()

    {
      ViewBag.CourtDepartmentId_ddl = vksSelectionService.GetDDL_OtdelenieByCourt(0,userContext.CourtId,0);
      VksMainSessionAddVM model = new VksMainSessionAddVM();
      model.CourtDepartmentId = vksSelectionService.GetUserOtdelenieID(userContext.LawUnitId);
      return View(model);
    }
    [HttpPost]
    public IActionResult MainSession(int courtDepartmentId)

    {
      ViewBag.CourtDepartmentId_ddl = vksSelectionService.GetDDL_OtdelenieByCourt(0,userContext.CourtId,0);
      int[] lawunits = vksSelectionService.GetActualCourtDepartmentLawunits(courtDepartmentId, true).Select(x=>x.LawUnitId).ToArray();
      VksMainSessionAddVM model = new VksMainSessionAddVM();
      model.CourtDepartmentId = courtDepartmentId;
      model.VksSelectionCalendar = vksSelectionService.GetvksSessionDayCalendar(lawunits, courtDepartmentId);
      //int[] lu = new int[] { 118 };
      //model.VksSelectionCalendar = vksSelectionService.GetvksSessionDayCalendar(lu, courtDepartmentId);
 


      return View(model);
    }
    public IActionResult RedirectVksSelection(int id)
    {
      int courtDepartmentId = vksSelectionService.GetCourtDepartmentIdByCaseId(id);
   


      ViewBag.CourtDepartmentId_ddl = vksSelectionService.GetDDL_OtdelenieByCourt(0, userContext.CourtId, 0);
      int[] lawunits = vksSelectionService.GetActualCourtDepartmentLawunits(courtDepartmentId, true).Select(x => x.LawUnitId).ToArray();
      VksMainSessionAddVM model = new VksMainSessionAddVM();
      model.CourtDepartmentId = courtDepartmentId;
      model.VksSelectionCalendar = vksSelectionService.GetvksSessionDayCalendar(lawunits, courtDepartmentId);
   
      return View("MainSession", model);
    }
    public IActionResult MainSessionByLawUnit(int caseId)
    {
      int[] lawunits = new int[] { vksSelectionService.GetLawUnitIdByCaseId(caseId) };


      int courtDepartmentId = vksSelectionService.GetCourtDepartmentIdByCaseId(caseId);

      List<VksSessionDayCalendarVM> model = vksSelectionService.GetvksSessionDayCalendar(lawunits, 0);
 
      return View("MainSessionByLawunit", model);
    }

   
  }

}