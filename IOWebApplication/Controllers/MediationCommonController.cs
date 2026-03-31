// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common.Mediation;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    /// <summary>
    /// Допълнителни регистри за медиацията
    /// </summary>
    public class MediationCommonController : BaseController
    {
        private readonly IMediationCommonService mediationCommonService;
        private readonly INomenclatureService nomService;

        /// <summary>
        /// Допълнителни регистри за медиацията
        /// </summary>
        /// <param name="_mediationCommonService"></param>
        /// <param name="_nomService"></param>
        public MediationCommonController(IMediationCommonService _mediationCommonService,
                                         INomenclatureService _nomService)
        {
            mediationCommonService = _mediationCommonService;
            nomService = _nomService;
        }

        #region Центрове за медиация

        /// <summary>
        /// Зареждане на страница с данни за центрове за медиация
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexMediationCenter()
        {
            return View();
        }

        /// <summary>
        /// извличане на данни за центрове за медиация
        /// </summary>
        /// <param name="request">IDataTablesRequest</param>
        /// <param name="filter">Филтър</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataMediationCenter(IDataTablesRequest request, MediationCenterFilterVM filter)
        {
            IQueryable<MediationCenterListDataVM> data = mediationCommonService.GetMediationCenters(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на списъци за добавяне/редактиране на центрове за медиация
        /// </summary>
        /// <param name="idSelected">Идентификаотр на текущият център, за да се премахме от списъка</param>
        /// <returns></returns>
        private async Task SetViewBagEditMediationCenter(int? idSelected = null)
        {
            ViewBag.CourtIds_ddl = await nomService.GetCourtsAsync();
        }

        /// <summary>
        /// Валидация на център за медиация преди запис
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private string ValidateMediationCenter(MediationCenterVM model)
        {
            return string.Empty;
        }

        /// <summary>
        /// Добавяне на център за медиация
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> AddMediationCenter()
        {
            MediationCenterVM model = new()
            {
                DateFrom = DateTime.Now
            };
            await SetViewBagEditMediationCenter();
            return View(nameof(EditMediationCenter), model);
        }

        /// <summary>
        /// Редакция на център за медиация
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<IActionResult> EditMediationCenter(int id)
        {
            MediationCenterVM model = await mediationCommonService.GetMediationCenterEditById(id);
            await SetViewBagEditMediationCenter(id);
            return View(nameof(EditMediationCenter), model);
        }

        /// <summary>
        /// Запис на център за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditMediationCenter(MediationCenterVM model)
        {
            await SetViewBagEditMediationCenter();

            if (!ModelState.IsValid)
            {
                return View(nameof(EditMediationCenter), model);
            }

            string _isvalid = ValidateMediationCenter(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditMediationCenter), model);
            }

            bool isAdd = model.Id == 0;
            int? saveId = await mediationCommonService.SaveMediationCenter(model);
            if (saveId != null)
            {
                SaveLogOperation(isAdd, saveId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditMediationCenter), new { id = saveId });
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            return View(nameof(EditMediationCenter), model);
        }

        #endregion

        #region Медиатори

        /// <summary>
        /// Зареждане на страница с данни за медиатори
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexMediationMediator()
        {
            return View();
        }

        /// <summary>
        /// извличане на данни за медиатори
        /// </summary>
        /// <param name="request">IDataTablesRequest</param>
        /// <param name="filter">Филтър</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataMediationMediator(IDataTablesRequest request, MediationMediatorFilterVM filter)
        {
            IQueryable<MediationMediatorListDataVM> data = mediationCommonService.GetMediationMediators(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на списъци за добавяне/редактиране на медиатори
        /// </summary>
        /// <returns></returns>
        private async Task SetViewBagEditMediationMediator()
        {
            ViewBag.CenterIds_ddl = await mediationCommonService.GetMediationCenterDDL(null, false);
        }

        /// <summary>
        /// Валидация на медиатори преди запис
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private string ValidateMediationMediator(MediationMediatorVM model)
        {
            if ((model.PracticeSpecificAreaLawYear ?? 0) < 0)
                return "Не може да има отрицателна практика в определена област на правото";

            return string.Empty;
        }

        /// <summary>
        /// Добавяне на медиатори
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> AddMediationMediator()
        {
            MediationMediatorVM model = new()
            {
                DateFrom = DateTime.Now
            };
            await SetViewBagEditMediationMediator();
            return View(nameof(EditMediationMediator), model);
        }

        /// <summary>
        /// Редакция на медиатори
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<IActionResult> EditMediationMediator(int id)
        {
            MediationMediatorVM model = await mediationCommonService.GetMediationMediatorEditById(id);
            await SetViewBagEditMediationMediator();
            return View(nameof(EditMediationMediator), model);
        }

        /// <summary>
        /// Запис на медиатори
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditMediationMediator(MediationMediatorVM model)
        {
            await SetViewBagEditMediationMediator();

            if (!ModelState.IsValid)
            {
                return View(nameof(EditMediationMediator), model);
            }

            string _isvalid = ValidateMediationMediator(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditMediationMediator), model);
            }

            bool isAdd = model.Id == 0;
            int? saveId = await mediationCommonService.SaveMediationMediator(model);
            if (saveId != null)
            {
                SaveLogOperation(isAdd, saveId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditMediationMediator), new { id = saveId });
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            return View(nameof(EditMediationMediator), model);
        }

        #endregion

        #region Координатори

        /// <summary>
        /// Зареждане на страница с данни за Координатори
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexMediationCoordinator()
        {
            return View();
        }

        /// <summary>
        /// извличане на данни за Координатори
        /// </summary>
        /// <param name="request">IDataTablesRequest</param>
        /// <param name="filter">Филтър</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataMediationCoordinator(IDataTablesRequest request, MediationCoordinatorFilterVM filter)
        {
            IQueryable<MediationCoordinatorListDataVM> data = mediationCommonService.GetMediationCoordinators(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на списъци за добавяне/редактиране на Координатори
        /// </summary>
        /// <returns></returns>
        private async Task SetViewBagEditMediationCoordinator()
        {
            ViewBag.CenterIds_ddl = await mediationCommonService.GetMediationCenterDDL(null, false);
        }

        /// <summary>
        /// Валидация на Координатори преди запис
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private string ValidateMediationCoordinator(MediationCoordinatorVM model)
        {
            if (model.LawUnitId < 1)
                return "Изберете координатор";

            return string.Empty;
        }

        /// <summary>
        /// Добавяне на Координатори
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> AddMediationCoordinator()
        {
            MediationCoordinatorVM model = new()
            {
                DateFrom = DateTime.Now
            };
            await SetViewBagEditMediationCoordinator();
            return View(nameof(EditMediationCoordinator), model);
        }

        /// <summary>
        /// Редакция на Координатори
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<IActionResult> EditMediationCoordinator(int id)
        {
            MediationCoordinatorVM model = await mediationCommonService.GetMediationCoordinatorEditById(id);
            await SetViewBagEditMediationCoordinator();
            return View(nameof(EditMediationCoordinator), model);
        }

        /// <summary>
        /// Запис на Координатори
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditMediationCoordinator(MediationCoordinatorVM model)
        {
            await SetViewBagEditMediationCoordinator();

            if (!ModelState.IsValid)
            {
                return View(nameof(EditMediationCoordinator), model);
            }

            string _isvalid = ValidateMediationCoordinator(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditMediationCoordinator), model);
            }

            bool isAdd = model.Id == 0;
            int? saveId = await mediationCommonService.SaveMediationCoordinator(model);
            if (saveId != null)
            {
                SaveLogOperation(isAdd, saveId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditMediationCoordinator), new { id = saveId });
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            return View(nameof(EditMediationCoordinator), model);
        }

        #endregion

        #region Ставки за заплащане на медиатор

        /// <summary>
        /// Зареждане на страница със ставки за заплащане на медиатор
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexMediatorFee()
        {
            return View();
        }

        /// <summary>
        /// Извличане на данни за ставки за заплащане на медиатор
        /// </summary>
        /// <param name="request">IDataTablesRequest</param>
        /// <param name="filter">Филтър</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataMediatorFee(IDataTablesRequest request, MediatorFeeFilterVM filter)
        {
            IQueryable<MediatorFeeListDataVM> data = mediationCommonService.GetMediatorFees(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на списъци за добавяне/редактиране на ставки за заплащане на медиатор
        /// </summary>
        /// <returns></returns>
        private async Task SetViewBagEditMediatorFee()
        {
            ViewBag.MediationTypeId_ddl = await nomService.GetDropDownListAsync<MediationType>();
        }

        /// <summary>
        /// Валидация на ставки за заплащане на медиатор преди запис
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private async Task<string> ValidateMediatorFee(MediatorFeeVM model)
        {
            if (model.MediationTypeId < 1)
                return "Изберете вид среща за медиация";

            if (model.HourFee < (decimal)0.001)
                return "Въведетет възнаграждение на час";

            if (model.HourFeeEUR < (decimal)0.001)
                return "Въведетет възнаграждение на час в евро";

            if (await mediationCommonService.IsExsistingMediatorFeeWithSamePeriod(model.MediationTypeId, model.DateFrom, model.DateTo, model.Id))
                return "Има ставка за този период и вид среща за медиация";

            return string.Empty;
        }

        /// <summary>
        /// Добавяне на ставки за заплащане на медиатор
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> AddMediatorFee()
        {
            MediatorFeeVM model = new()
            {
                DateFrom = DateTime.Now
            };
            await SetViewBagEditMediatorFee();
            return View(nameof(EditMediatorFee), model);
        }

        /// <summary>
        /// Редакция на ставки за заплащане на медиатор
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<IActionResult> EditMediatorFee(int id)
        {
            MediatorFeeVM model = await mediationCommonService.GetMediatorFeeEditById(id);
            await SetViewBagEditMediatorFee();
            return View(nameof(EditMediatorFee), model);
        }

        /// <summary>
        /// Запис на ставки за заплащане на медиатор
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditMediatorFee(MediatorFeeVM model)
        {
            await SetViewBagEditMediatorFee();

            if (!ModelState.IsValid)
            {
                return View(nameof(EditMediatorFee), model);
            }

            string _isvalid = await ValidateMediatorFee(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditMediatorFee), model);
            }

            bool isAdd = model.Id == 0;
            int? saveId = await mediationCommonService.SaveMediatorFee(model);
            if (saveId != null)
            {
                SaveLogOperation(isAdd, saveId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditMediatorFee), new { id = saveId });
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            return View(nameof(EditMediatorFee), model);
        }

        #endregion
    }
}
