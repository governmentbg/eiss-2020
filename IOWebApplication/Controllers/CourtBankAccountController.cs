using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Data.Models.Common;
using Microsoft.AspNetCore.Mvc;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;

namespace IOWebApplication.Controllers
{
    public class CourtBankAccountController : BaseController
    {
        private readonly ICommonService service;
        private readonly INomenclatureService nomService;

        public CourtBankAccountController(ICommonService _service, INomenclatureService _nomService)
        {
            service = _service;
            nomService = _nomService;
        }

        /// <summary>
        /// Банкови сметки
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            ViewBag.breadcrumbs = service.Breadcrumbs_ForCourtBankAccount().DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.Nom15);

            return View();
        }

        /// <summary>
        /// Извличане на данни Банкови сметки
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request)
        {
            var data = service.CourtBankAccount_Select(userContext.CourtId);

            return request.GetResponse(data);
        }

        void SetViewBag() 
        {
            ViewBag.MoneyGroupId_ddl = nomService.GetDropDownList<MoneyGroup>();
            ViewBag.ComPortPos_ddl = service.COMPort();
            SetHelpFile(HelpFileValues.Nom15);
        }

        /// <summary>
        /// Добавяне на Банкови сметки
        /// </summary>
        /// <returns></returns>
        public IActionResult Add()
        {
            SetBreadcrums(0);
            SetViewBag();
            var model = new CourtBankAccount()
            {
                CourtId = userContext.CourtId,
                IsActive = true
            };

            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на Банкови сметки
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            SetBreadcrums(id);
            SetViewBag();
            var model = service.GetById<CourtBankAccount>(id);
            return View(nameof(Edit), model);
        }

        public void SetBreadcrums(int id)
        {
            if (id > 0)
                ViewBag.breadcrumbs = service.Breadcrumbs_ForCourtBankAccountEdit(id).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = service.Breadcrumbs_ForCourtBankAccountAdd().DeleteOrDisableLast();
        }

        /// <summary>
        /// Запис на Банкови сметки
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CourtBankAccount model)
        {
            SetViewBag();
            if (!ModelState.IsValid)
            {
                SetBreadcrums(model.Id);
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;
            if (service.CourtBankAccount_SaveData(model))
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

        /// <summary>
        /// ПОС устройства
        /// </summary>
        /// <returns></returns>
        public IActionResult PosDevice()
        {
            ViewBag.breadcrumbs = service.Breadcrumbs_ForCourtPosDevice().DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.Nom16);
            return View();
        }

        /// <summary>
        /// Извличане на данни ПОС устройства
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataPosDevice(IDataTablesRequest request)
        {
            var data = service.CourtPosDevice_Select(userContext.CourtId);

            return request.GetResponse(data);
        }

        public void SetBreadcrumsPosDevice(int id)
        {
            if (id > 0)
                ViewBag.breadcrumbs = service.Breadcrumbs_ForCourtPosDeviceEdit(id).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = service.Breadcrumbs_ForCourtPosDeviceAdd().DeleteOrDisableLast();
        }

        void SetViewBagPosDevice()
        {
            ViewBag.CourtBankAccountId_ddl = service.BankAccount_SelectDDL(userContext.CourtId, 0, true);
            SetHelpFile(HelpFileValues.Nom16);
        }

        /// <summary>
        /// Добавяне на ПОС устройства
        /// </summary>
        /// <returns></returns>
        public IActionResult AddPosDevice()
        {
            SetBreadcrumsPosDevice(0);
            SetViewBagPosDevice();
            var model = new CourtPosDevice()
            {
                CourtId = userContext.CourtId,
                IsActive = true
            };

            return View(nameof(EditPosDevice), model);
        }

        /// <summary>
        /// Редакция на ПОС устройства
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditPosDevice(int id)
        {
            SetBreadcrumsPosDevice(id);
            SetViewBagPosDevice();
            var model = service.GetById<CourtPosDevice>(id);
            return View(nameof(EditPosDevice), model);
        }

        /// <summary>
        /// Запис на ПОС устройства
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditPosDevice(CourtPosDevice model)
        {
            SetViewBagPosDevice();
            if (!ModelState.IsValid)
            {
                SetBreadcrumsPosDevice(model.Id);
                return View(nameof(EditPosDevice), model);
            }
            var currentId = model.Id;
            if (service.CourtPosDevice_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditPosDevice), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            SetBreadcrumsPosDevice(model.Id);
            return View(nameof(EditPosDevice), model);
        }
    }
}