using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Services;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class WorkNotificationController : BaseController
    {
        private readonly IWorkNotificationService service;
        private readonly ICourtDepartmentService courtDepartmentService;

        public WorkNotificationController(IWorkNotificationService _service,
                                          ICourtDepartmentService _courtDepartmentService)
        {
            service = _service;
            courtDepartmentService = _courtDepartmentService;
        }

        /// <summary>
        /// Зареждане на страница със нотификации
        /// </summary>
        /// <param name="id">Идентификатор на нотификация</param>
        /// <param name="wnTypeId">Тип на нотификация</param>
        /// <returns></returns>
        public async Task<IActionResult> Index(int? id, int? wnTypeId, int? nKind)
        {
            CurrentContext_SetObjectInfo("Преглед на регистрирани нотификации");
            ViewBag.WorkNotificationTypeId_ddl = service.GetDDL_WorkNotificationTypes(0);
            ViewBag.ReadTypeId_ddl = service.ReadTypeId_SelectDDL();
            ViewBag.UserCourtDepartmentId_ddl = await courtDepartmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
            WorkNotificationFilterVM model = service.MakeDefaultFilter();
            model.Id = id;
            model.WorkNotificationTypeId = wnTypeId ?? -1;
            model.NotificationKind = nKind;

            return View(model);
        }

        /// <summary>
        /// Метод променящ дата на визуализация на нотификацията
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public IActionResult EditDateCreated(int id)
        {
            CurrentContext_SetObjectInfo("Зареждане на данни за редакция на дата за визуализация на нотификация");
            WorkNotificationEditDateVM model = new() { NotificationId = id };
            return View(nameof(EditDateCreated), model);
        }

        /// <summary>
        /// Запис на дата на визуализация на нотификацията
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditDateCreated(WorkNotificationEditDateVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(EditDateCreated), model);
            }

            if (await service.EditDateCreatedWNFastProcess(model))
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            return RedirectToAction(nameof(Index), new { id = model.NotificationId });
        }

        public IActionResult Dashboard()
        {
            ViewBag.WorkNotificationTypeId_ddl = service.GetDDL_WorkNotificationTypes(0);
            ViewBag.ReadTypeId_ddl = service.ReadTypeId_SelectDDL();
            WorkNotificationFilterVM model = service.MakeDefaultFilter();
            return View(model);
        }
        public IActionResult DashboardReload(WorkNotificationFilterVM filterData)
        {
            ViewBag.WorkNotificationTypeId_ddl = service.GetDDL_WorkNotificationTypes(0);
            ViewBag.ReadTypeId_ddl = service.ReadTypeId_SelectDDL();
            filterData.UserId = userContext.UserId;
            filterData.CourtId = userContext.CourtId;
            filterData.SourceType = 0;
            filterData.SourceId = 0;

            return PartialView("_Dashboard", filterData);
        }
        public JsonResult SaveReaded(long id)
        {
            bool result = (service.SaveWorkNotificationReadAll(id) != null);
            return Json(result);
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, WorkNotificationFilterVM filterData)
        {
            filterData.UserId = userContext.UserId;
            filterData.CourtId = userContext.CourtId;
            filterData.SourceType = 0;
            filterData.SourceId = 0;
            var data = service.SelectWorkNotifications(filterData);
            return request.GetResponse(data);
        }
        public IActionResult RedirectToSource(int id)
        {
            var model = service.SaveWorkNotificationRead(id);
            if (model == null)
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
                return RedirectToAction("Index");
            }
            switch (model.SourceType)
            {
                case SourceTypeSelectVM.Case:
                    return RedirectToAction("CasePreview", "Case", new { id = model.SourceId });
                case SourceTypeSelectVM.CaseNotification:
                    return RedirectToAction("Edit", "CaseNotification", new { id = model.SourceId });
                case SourceTypeSelectVM.CaseSession:
                    {
                        if (model.WorkNotificationTypeId == NomenclatureConstants.WorkNotificationType.DeadLine)
                            return RedirectToAction("Preview", "CaseSession", new { id = model.SourceId, tab = "#tabSessionMainData" });
                        else
                            return RedirectToAction("Preview", "CaseSession", new { id = model.SourceId, tab = "#tabPersonNotification" });
                    }
                case SourceTypeSelectVM.CaseLawyerHelp:
                    return RedirectToAction("Edit", "CaseLawyerHelp", new { id = model.SourceId });
                case SourceTypeSelectVM.Document:
                        return RedirectToAction("Edit", "Document", new { id = model.SourceId });
                case SourceTypeSelectVM.CaseSessionAct:
                    {
                        if ((model.WorkNotificationTypeId == NomenclatureConstants.WorkNotificationType.ActInforcedAnotherInstanceFastProcess) ||
                            (model.WorkNotificationTypeId == NomenclatureConstants.WorkNotificationType.N11))
                            return RedirectToAction("CaseTimeLinePreview", "Case", new { id = model.CaseId });
                        else
                            return RedirectToAction("Edit", "CaseSessionAct", new { id = model.SourceId });
                    }
                case SourceTypeSelectVM.ExecList:
                    return RedirectToAction("EditExecList", "Money", new { id = model.SourceId });
                default:
                    return RedirectToAction("Index");
            }
        }
    }
}