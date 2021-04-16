using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{

    public class CounterController : GlobalAdminBaseController
    {
        private readonly ICounterService service;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;

        public CounterController(ICounterService _service, INomenclatureService _nomService, ICommonService _commonService)
        {
            service = _service;
            nomService = _nomService;
            commonService = _commonService;
        }
        //public IActionResult InitAllCounters()
        //{
        //    if (!userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator))
        //    {
        //        return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
        //    }
        //    service.InitAllCounters();
        //    return Content("InitAllCounters done.");
        //}

        public IActionResult SetCounterValues()
        {
            var model = service.Counter_GetCurrentValues(userContext.CourtId);
            ViewBag.CourtId = userContext.CourtId;
            return View(model);
        }

        [HttpPost]
        public IActionResult SetCounterValues(CounterVM[] model)
        {
            if (service.Counter_SetCurrentValues(model))
            {
                this.SaveLogOperation(IO.LogOperation.Models.OperationTypes.Patch, userContext.CourtId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                SetSuccessMessage(MessageConstant.Values.SaveFailed);
            }
            return RedirectToAction(nameof(SetCounterValues));
        }

        /// <summary>
        /// Списък броячи
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_Counter().DeleteOrDisableLast();

            return View();
        }

        /// <summary>
        /// Извличане на данни за броячи
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request)
        {
            var data = service.Counter_Select(userContext.CourtId, request.Search?.Value);

            return request.GetResponse(data);
        }

        public void SetBreadcrums(int id)
        {
            if (id > 0)
                ViewBag.breadcrumbs = commonService.Breadcrumbs_CounterEdit(id).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_CounterAdd().DeleteOrDisableLast();
        }

        /// <summary>
        /// Добавяне на брояч
        /// </summary>
        /// <returns></returns>
        public IActionResult Add()
        {
            SetBreadcrums(0);
            var model = new CounterEditVM()
            {
                CourtId = userContext.CourtId
            };
            SetViewbag();
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на брояч
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            SetBreadcrums(id);

            var model = service.Counter_GetById(id);
            SetViewbag();
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на брояч
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CounterEditVM model)
        {
            SetViewbag();
            if (!ModelState.IsValid)
            {
                SetBreadcrums(model.Id);
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;
            if (service.Counter_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            SetBreadcrums(model.Id);
            return View(nameof(Edit), model);
        }

        void SetViewbag()
        {
            ViewBag.CounterTypeId_ddl = nomService.GetDropDownList<CounterType>();
            ViewBag.ResetTypeId_ddl = nomService.GetDropDownList<CounterResetType>();
            ViewBag.DocumentDirectionId_ddl = nomService.GetDropDownList<DocumentDirection>();
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.SessionActGroupId_ddl = nomService.GetDropDownList<SessionActGroup>();
        }
    }
}