// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class CaseSelectionChangeController : BaseController
    {
        private readonly ICaseSelectionChangeService service;
        private readonly INomenclatureService nomService;
        private readonly ICdnService cdnService;
        private readonly ICourtGroupLawUnitService glawunitService;
        private readonly ICourtDepartmentService depService;
        private readonly ICommonService commonService;
        public CaseSelectionChangeController(
            ICaseSelectionChangeService _service,
            INomenclatureService _nomService,
            ICdnService _cdnService,
            ICourtGroupLawUnitService _glawunitService,
            ICourtDepartmentService _depService,
            ICommonService _commonService)
        {
            service = _service;
            nomService = _nomService;
            cdnService = _cdnService;
            glawunitService = _glawunitService;
            depService = _depService;
            commonService = _commonService;
        }

        private bool checkAccess()
        {
            return userContext.IsUserInRole(AccountConstants.Roles.CaseInit);
        }

        [TitleAudit(Operation = AuditConstants.Operations.List)]
        public IActionResult Index()
        {
            if (!checkAccess())
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }

            //ViewBag.ChangeTypeId_ddl = nomService.GetDropDownList<CaseSelectionChangeType>(false, true);
            ViewBag.ChangeTypeId_ddl = nomService.GetDDL_CaseSelectionChangeType(NomenclatureConstants.CaseSelectionChangeTypes.Prerazpredelenie, false);
            ViewBag.CourtGroupId_ddl = nomService.GetDDL_CourtGroup(userContext.CourtId);
            var model = new CaseSelectionChangeFilterVM();
            SetHelpFile(HelpFileValues.Massredistribution);
            return View(model);
        }
        [TitleAudit(Operation = AuditConstants.Operations.List)]
        public IActionResult IndexJudge()
        {
            if (!checkAccess())
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }

            //ViewBag.ChangeTypeId_ddl = nomService.GetDropDownList<CaseSelectionChangeType>(false, true);
            ViewBag.ChangeTypeId_ddl = nomService.GetDDL_CaseSelectionChangeType(NomenclatureConstants.CaseSelectionChangeTypes.PrerazpredelenieJudge, false);
            ViewBag.CourtGroupId_ddl = nomService.GetDDL_CourtGroup(userContext.CourtId);
            var model = new CaseSelectionChangeFilterVM();
            SetHelpFile(HelpFileValues.Massredistribution);
            return View(model);
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, CaseSelectionChangeFilterVM filter)
        {
            var data = service.Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Преразпределение: 1 към 1 съдия
        /// </summary>
        /// <returns></returns>
        [DisableAudit]
        public IActionResult ReplaceJudge()
        {
            if (!checkAccess())
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }


            var model = new CaseSelectionChangeFilterVM()
            {
                ChangeTypeId = NomenclatureConstants.CaseSelectionChangeTypes.Prerazpredelenie
            };
            SetViewbag();
            return View(model);
        }
        /// <summary>
        /// Преразпределение съдия от състав: 1 към 1 съдия
        /// </summary>
        /// <returns></returns>
        [DisableAudit]
        public IActionResult ReplaceJudgeJudge()
        {
            if (!checkAccess())
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }


            var model = new CaseSelectionChangeFilterVM()
            {
                ChangeTypeId = NomenclatureConstants.CaseSelectionChangeTypes.PrerazpredelenieJudge
            };
            SetViewbag();
            return View(model);
        }


        [HttpPost]
        public IActionResult ReplaceJudge_ListData(IDataTablesRequest request, CaseSelectionChangeFilterVM filter)
        {
            var data = service.Select_Cases(filter);
            return request.GetResponse(data);
        }
        [HttpPost]
        public IActionResult ReplaceJudgeJudge_ListData(IDataTablesRequest request, CaseSelectionChangeFilterVM filter)
        {
            var data = service.Select_CasesJudge(filter);
            return request.GetResponse(data);
        }

        [HttpPost]
        public async Task<JsonResult> ReplaceJudge_SaveData(CaseSelectionChangeFilterVM filter)
        {
            //return Json(new SaveResultVM(false,"test"));
            var saveResult = await service.ReplaceJudge_SaveData(filter);
            if (saveResult.Result)
            {
                saveResult.Result &= await prepareProtokolPdf((int)saveResult.ObjectId);
            }
            if (saveResult.Result)
            {
                AddAuditInfo(AuditConstants.Operations.Append, saveResult.AuditInfo, "Създаване на протокол", "Масово преразпределение");
            }
            return Json(saveResult);
        }
        [HttpPost]
        public async Task<JsonResult> ReplaceJudgeJudge_SaveData(CaseSelectionChangeFilterVM filter)
        {
            //return Json(new SaveResultVM(false,"test"));
            var saveResult = await service.ReplaceJudge_SaveData(filter);
            if (saveResult.Result)
            {
                saveResult.Result &= await prepareProtokolPdf((int)saveResult.ObjectId);
            }
            if (saveResult.Result)
            {
                AddAuditInfo(AuditConstants.Operations.Append, saveResult.AuditInfo, "Създаване на протокол", "Масово преразпределение");
            }
            return Json(saveResult);
        }

        public async Task<IActionResult> View(int id, bool? signResult = null)
        {
            var model = service.GetModelForProtokol(id);
            if (signResult == true)
            {
                SetSuccessMessage("Протоколът е подписан успешно.");
                if (model.ChangeStateId == NomenclatureConstants.CaseSelectionChangeStates.New)
                {
                    var res = await service.DeclareChange(id);
                    if (res.Result)
                    {
                        if (model.ChangeTypeId==NomenclatureConstants.CaseSelectionChangeTypes.Prerazpredelenie)
                        {
                            await saveEnforcement(id);
                        }
                        if (model.ChangeTypeId == NomenclatureConstants.CaseSelectionChangeTypes.PrerazpredelenieJudge)
                        {
                            await saveEnforcementJudge(id);
                        }

                        model = service.GetModelForProtokol(id);
                    }
                }
            }

            if (signResult == false)
            {
                SetErrorMessage("Проблем при подписване на протокол.");
            }
            SetHelpFile(HelpFileValues.Massredistribution);

            return View(model);
        }

        private async Task saveEnforcement(int id)
        {
            var result = await service.EnforceChange(id);
            if (result.Result)
            {
                AddAuditInfo(AuditConstants.Operations.Patch, result.AuditInfo, "Постановяване на протокол", "Масово преразпределение");
            }
            else
            {
                SetErrorMessage("Съществуват проблеми при записване на преразпределение по дела. Опитайте отново.");
            }
        }
        private async Task saveEnforcementJudge(int id)
        {
            var result = await service.EnforceChangeJudge(id);
            if (result.Result)
            {
                AddAuditInfo(AuditConstants.Operations.Patch, result.AuditInfo, "Постановяване на протокол", "Масово преразпределение");
            }
            else
            {
                SetErrorMessage("Съществуват проблеми при записване на преразпределение по дела. Опитайте отново.");
            }
        }

        public async Task<IActionResult> EnforceChange(int id)
        {
            await saveEnforcement(id);
            return RedirectToAction(nameof(View), new { id = id });
        }

        void SetViewbag()
        {
            ViewBag.CaseGroupIds_ddl = nomService.GetDropDownList<CaseGroup>(false);
            ViewBag.CourtGroupId_ddl = nomService.GetDDL_CourtGroup(userContext.CourtId);
            SetHelpFile(HelpFileValues.Massredistribution);
        }

        async Task<bool> prepareProtokolPdf(int id)
        {
            var model = service.GetModelForProtokol(id);
            var viewName = "";
            switch (model.ChangeTypeId)
            {
                case NomenclatureConstants.CaseSelectionChangeTypes.Prerazpredelenie:
                    viewName = "ReplaceJudge_Protokol.cshtml";
                    break;
                case NomenclatureConstants.CaseSelectionChangeTypes.PrerazpredelenieJudge:
                    viewName = "ReplaceJudge_Protokol.cshtml";
                    break;
                default:
                    break;
            }
            var html = await this.RenderPartialViewAsync("~/Views/CaseSelectionChange/", viewName, model, true);

            byte[] pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true).GetByte(this.ControllerContext);

            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.CaseSelectionChangeProtokol,
                SourceId = model.Id.ToString(),
                FileName = "protokol.pdf",
                ContentType = NomenclatureConstants.ContentTypes.Pdf,
                Title = model.ChangeTypeLabel,
                FileContentBase64 = Convert.ToBase64String(pdfBytes)
            };

            return await cdnService.MongoCdn_AppendUpdate(pdfRequest);
        }

        public IActionResult SendForSign(int id)
        {
            Uri urlSuccess = new Uri(Url.Action("View", "CaseSelectionChange", new { id = id, signResult = true }), UriKind.Relative);
            Uri url = new Uri(Url.Action("View", "CaseSelectionChange", new { id = id, signResult = false }), UriKind.Relative);
            var userId = service.GetPropById<CaseSelectionChange, string>(x => x.Id == id, x => x.UserId);
            var lu = commonService.Get_LawunitByUserId(userId);
            var model = new SignPdfInfo()
            {
                SourceId = id.ToString(),
                SourceType = SourceTypeSelectVM.CaseSelectionChangeProtokol,
                DestinationType = SourceTypeSelectVM.CaseSelectionChangeProtokol,
                Location = userContext.CourtName,
                Reason = "Подписване на протокол за преразпределяне",
                SuccessUrl = urlSuccess,
                CancelUrl = url,
                ErrorUrl = url,
                SignerName = lu?.FullName,
                SignerUic = lu?.Uic
            };
            return View("_SignPdf", model);
        }

        public IActionResult Get_LawunitsByCourtGroup(int courtGroupId)
        {
            // var model = glawunitService.GetLawUnitsByCourtGroup(courtGroupId);
            var model = glawunitService.GetLawUnitsByCourtGroup(courtGroupId).Prepend(new SelectListItem() { Value = null, Text = "Изберете" }).ToList();
            return Json(model);
        }

        public IActionResult Get_DepartmentsByLawunit(int lawunitId,bool noChange=false)
        {
            var model = new List<SelectListItem>();
            if (noChange)
            {
                model.Add(new SelectListItem() { Text = "Без промяна на състава по делото", Value = null });
              
            }
            else
            {
                model = depService.CourtDepartmentByLawUnit_Select(lawunitId, userContext.CourtId)
                                .Where(x => x.DepartmentTypeId == NomenclatureConstants.DepartmentType.Systav)
                                .Select(x => new SelectListItem
                                {
                                    Value = x.Id.ToString(),
                                    Text = x.Label
                                }).ToList().Prepend(new SelectListItem() { Value = null, Text = "Без промяна на състава по делото" }).ToList();
            }
            return Json(model);
        }
    }

    internal interface IQuarable<T>
    {
    }
}
