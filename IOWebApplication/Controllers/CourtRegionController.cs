using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Data.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class CourtRegionController : GlobalAdminBaseController
    {
        private readonly ICourtRegionService service;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        public CourtRegionController(
            ICourtRegionService _service, 
            INomenclatureService _nomService,
            ICommonService _commonService)
        {
            service = _service;
            nomService = _nomService;
            commonService = _commonService;
        }

        /// <summary>
        /// Страница със съдебни региони
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_CourtRegion().DeleteOrDisableLast();
            return View();
        }

        /// <summary>
        /// Извличане на данни за съдебни региони
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request)
        {
            var data = service.CourtRegion_Select();
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на съдебен регион
        /// </summary>
        /// <returns></returns>
        public IActionResult Add()
        {
            var model = new CourtRegion();
            SetViewbag();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_CourtRegionEdit(0).DeleteOrDisableLast();
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на съдебен регион
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            var model = service.GetById<CourtRegion>(id);
            SetViewbag();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_CourtRegionEdit(id).DeleteOrDisableLast();
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на съдебен регион
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CourtRegion model)
        {
            SetViewbag();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_CourtRegionEdit(model.Id).DeleteOrDisableLast();
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }

            var currentId = model.Id;
            if (service.CourtRegion_SaveData(model))
            {
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

        void SetViewbag()
        {
            ViewBag.ParentId_ddl = service.GetDropDownList();
        }

        void SetViewbagArea(int Id)
        {
            var courtRegion = service.GetById<CourtRegion>(Id);
            ViewBag.CourtRegionId = Id;
            ViewBag.CourtRegionName = courtRegion.Label;
            ViewBag.DistrictCode_ddl = nomService.GetDDL_EkDistrict(false);
        }

        /// <summary>
        /// Страница за Съдебни райони Област-Община
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public IActionResult IndexArea(int Id)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_CourtRegionIndexArea(Id).DeleteOrDisableLast();
            SetViewbagArea(Id);
            return View();
        }

        /// <summary>
        /// Извличане на данни Съдебни райони Област-Община
        /// </summary>
        /// <param name="request"></param>
        /// <param name="CourtRegionId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataArea(IDataTablesRequest request, int CourtRegionId)
        {
            var data = service.CourtRegionArea_Select(CourtRegionId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на Съдебни райони Област-Община
        /// </summary>
        /// <param name="CourtRegionId"></param>
        /// <returns></returns>
        public IActionResult AddArea(int CourtRegionId)
        {
            var model = new CourtRegionArea()
            {
                CourtRegionId = CourtRegionId
            };
            SetViewbagArea(CourtRegionId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_CourtRegionIndexAreaEdit(CourtRegionId, 0).DeleteOrDisableLast();
            return View(nameof(EditArea), model);
        }

        /// <summary>
        /// Редакция на Съдебни райони Област-Община
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditArea(int id)
        {
            var model = service.GetById<CourtRegionArea>(id);
            SetViewbagArea(model.CourtRegionId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_CourtRegionIndexAreaEdit(model.CourtRegionId, id).DeleteOrDisableLast();
            return View(nameof(EditArea), model);
        }

        /// <summary>
        /// Запис на Съдебни райони Област-Община
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditArea(CourtRegionArea model)
        {
            SetViewbagArea(model.CourtRegionId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_CourtRegionIndexAreaEdit(model.CourtRegionId, model.Id).DeleteOrDisableLast();
            if (!ModelState.IsValid)
            {
                return View(nameof(EditArea), model);
            }

            var currentId = model.Id;
            if (service.CourtRegionArea_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditArea), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditArea), model);
        }
    }
}