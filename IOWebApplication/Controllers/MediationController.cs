// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    /// <summary>
    /// Медиация
    /// </summary>
    public class MediationController : BaseController
    {
        private readonly IMediationService mediationService;
        private readonly IMediationCommonService mediationCommonService;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;

        /// <summary>
        /// Медиация
        /// </summary>
        /// <param name="_mediationService"></param>
        /// <param name="_mediationCommonService"></param>
        /// <param name="_nomService"></param>
        public MediationController(IMediationService _mediationService,
                                   IMediationCommonService _mediationCommonService,
                                   INomenclatureService _nomService,
                                   ICommonService _commonService)
        {
            mediationService = _mediationService;
            mediationCommonService = _mediationCommonService;
            nomService = _nomService;
            commonService = _commonService;
        }

        #region Работа с дела за медиация

        /// <summary>
        /// Страница с дела подлежащи на медиация
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> IndexCaseMediation()
        {
            MediationCaseFilterVM filter = new() { CaseYear = DateTime.Now.Year };
            await ViewBagIndexCaseMediation();
            return View(filter);
        }

        /// <summary>
        /// Метод зареждащ номенклатури за справка за дейността на медиаторите
        /// </summary>
        private async Task ViewBagIndexCaseMediation()
        {
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.CaseCodeSubId_ddl = await nomService.GetDDL_CaseCodeSub();
        }

        /// <summary>
        /// извличане на данни за дела подлежащи на медиация
        /// </summary>
        /// <param name="request">IDataTablesRequest</param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ListDataCaseMediation(IDataTablesRequest request, MediationCaseFilterVM filter)
        {
            IQueryable<MediationCaseListDataVM> data = await mediationService.GetMediationCases(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне/редактиране на данни за медиация
        /// </summary>
        /// <param name="id">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<IActionResult> CasePreviewMediation(int id)
        {
            MediationCasePreviewVM model = await mediationService.GetMediationCasePreview(id);
            return View(nameof(CasePreviewMediation), model);
        }

        /// <summary>
        /// Зареждане на списъци за редактиране на дело подлежащо на медиация
        /// </summary>
        /// <param name="caseId">Идентификаотр на дело</param>
        /// <returns></returns>
        private async Task SetViewBagEditMediationCase(int caseId)
        {
            ViewBag.CaseCodeSubId_ddl = await mediationService.GetDDL_CaseCodeSub(caseId);
            ViewBag.MediationProcedureId_ddl = await nomService.GetDropDownListAsync<MediationProcedure>();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(caseId);
        }

        /// <summary>
        /// Валидация на дело подлежащо на медиация преди запис
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private async Task<string> ValidateMediationCase(MediationCaseEditVM model)
        {
            if (!(model.IsMediation ?? false))
                if (await mediationService.IsExistMediationCaseSession(model.Id))
                    return "Има създадени срещи и не може да размаркирате делото, че не подлежи на медиация";

            return string.Empty;
        }

        /// <summary>
        /// Редакция на дело подлежащо на медиация
        /// </summary>
        /// <param name="id">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<IActionResult> EditMediationCase(int id)
        {
            CurrentContext_SetObjectInfo("Преглед за редакция на данни за дело за медиация");
            MediationCaseEditVM model = await mediationService.GetMediationCaseEditById(id);
            await SetViewBagEditMediationCase(model.Id);
            return View(nameof(EditMediationCase), model);
        }

        /// <summary>
        /// Запис на дело подлежащо на медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditMediationCase(MediationCaseEditVM model)
        {
            await SetViewBagEditMediationCase(model.Id);

            if (!ModelState.IsValid)
            {
                return View(nameof(EditMediationCase), model);
            }

            string _isvalid = await ValidateMediationCase(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditMediationCase), model);
            }

            bool isAdd = model.Id == 0;
            int? saveId = await mediationService.SaveMediationCase(model);
            if (saveId != null)
            {
                SaveLogOperation(isAdd, saveId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditMediationCase), new { id = saveId });
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            return View(nameof(EditMediationCase), model);
        }

        #endregion

        #region Медиатори

        /// <summary>
        /// Извличане на данни за медиатори към дело
        /// </summary>
        /// <param name="request">IDataTablesRequest</param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataMediators(IDataTablesRequest request, MediationCaseMediatorFilterVM filter)
        {
            IQueryable<MediationCaseMediatorListDataVM> data = mediationService.GetMediationCaseMediators(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на списъци за добавяне/редактиране на медиатори към дело
        /// </summary>
        /// <param name="caseId">Идентификаотр на дело</param>
        /// <returns></returns>
        private async Task SetViewBagEditMediationCaseMediator(int caseId)
        {
            ViewBag.MediationMediatorId_ddl = await mediationCommonService.GetMediationMediatorsDDL(caseId, DateTime.Now);
            ViewBag.MediationTypeChoiceMediatorId_ddl = await nomService.GetDropDownListAsync<MediationTypeChoiceMediator>();
        }

        /// <summary>
        /// Валидация на медиатори към дело преди запис
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private async Task<string> ValidateMediationCaseMediator(MediationCaseMediatorVM model)
        {
            if (model.MediationMediatorId < 1)
                return "Изберете медиатор";

            if (await mediationService.IsExistMediatorInCase(model.MediationMediatorId, model.CaseId, model.Id))
                return "Избраният медиатор съществува в делото";

            return string.Empty;
        }

        /// <summary>
        /// Добавяне на медиатори към дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<IActionResult> AddMediationCaseMediator(int caseId)
        {
            MediationCaseMediatorVM model = new()
            {
                CaseId = caseId,
                DateFrom = DateTime.Now
            };
            await SetViewBagEditMediationCaseMediator(caseId);
            return View(nameof(EditMediationCaseMediator), model);
        }

        /// <summary>
        /// Редакция на медиатори към дело
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<IActionResult> EditMediationCaseMediator(int id)
        {
            MediationCaseMediatorVM model = await mediationService.GetMediationCaseMediatorEditById(id);
            await SetViewBagEditMediationCaseMediator(model.CaseId);
            return View(nameof(EditMediationCaseMediator), model);
        }

        /// <summary>
        /// Запис на медиатори към дело
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditMediationCaseMediator(MediationCaseMediatorVM model)
        {
            await SetViewBagEditMediationCaseMediator(model.CaseId);

            if (!ModelState.IsValid)
            {
                return View(nameof(EditMediationCaseMediator), model);
            }

            string _isvalid = await ValidateMediationCaseMediator(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditMediationCaseMediator), model);
            }

            bool isAdd = model.Id == 0;
            int? saveId = await mediationService.SaveMediationCaseMediator(model);
            if (saveId != null)
            {
                SaveLogOperation(isAdd, saveId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditMediationCaseMediator), new { id = saveId });
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            return View(nameof(EditMediationCaseMediator), model);
        }

        /// <summary>
        /// Премахване на медиатор
        /// </summary>
        /// <param name="model">Модел с данни за обекта за премахване</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult MediationCaseMediator_ExpiredInfo(ExpiredInfoVM model)
        {
            var expireObject = mediationService.GetById<MediationCaseMediator>(model.Id);
            if (mediationService.SaveExpireInfo<MediationCaseMediator>(model))
            {
                SetSuccessMessage("Медиатора е премахната успешно");
                return Json(new { result = true, redirectUrl = Url.Action("CasePreviewMediation", "Mediation", new { id = expireObject.CaseId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        #endregion

        #region Срещи

        /// <summary>
        /// извличане на данни за срещи към дело
        /// </summary>
        /// <param name="request">IDataTablesRequest</param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataSessions(IDataTablesRequest request, MediationCaseSessionFilterVM filter)
        {
            IQueryable<MediationCaseSessionListDataVM> data = mediationService.GetMediationCaseSessions(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на списъци за добавяне/редактиране на срещи към дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task SetViewBagEditMediationCaseSession(int caseId)
        {
            ViewBag.MediationTypeId_ddl = await nomService.GetDropDownListAsync<MediationType>();
            ViewBag.MediationLocationId_ddl = await nomService.GetDropDownListAsync<MediationLocation>();
            ViewBag.MediationStateId_ddl = await nomService.GetDropDownListAsync<MediationState>();
            ViewBag.LinkCaseIds_ddl = await mediationService.GetDDL_MediationCases(caseId, false);
        }

        /// <summary>
        /// Валидация на среща към дело преди запис
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private async Task<string> ValidateMediationCaseSession(MediationCaseSessionVM model)
        {
            if (model.MediationTypeId < 1)
                return "Изеберете вид среща";

            if (model.DateTo == null)
                return "Изеберете до дата";

            if (model.DateTo < model.DateFrom)
                return "Крайната дата не може да е по-малка от началната дата";

            if (model.MediationStateId < 1)
                return "Изберете статус";

            if (model.MediationStateId == NomenclatureConstants.MediationStateConstants.Held)
            {
                if (model.DateFrom > DateTime.Now)
                    return "Не може да отразите проведена среща с бъдеща дата/час.";
            }

            if (model.MediationStateId == NomenclatureConstants.MediationStateConstants.Scheduled)
            {
                if (model.DateFrom < DateTime.Now)
                    return "Не може да насрочвате среща с минала дата/час.";
            }

            if (model.MediationStateId == NomenclatureConstants.MediationStateConstants.NotHeld)
            {
                if (string.IsNullOrEmpty(model.Description))
                    return "Въведете причина за непровеждане";
            }

            if (model.MediationStateId == NomenclatureConstants.MediationStateConstants.Scheduled)
            {
                DateTime dateNow = DateTime.Now;
                if (await mediationService.IsExsitMediationSession(model.DateFrom, (model.DateTo ?? dateNow), model.CaseId, model.Id))
                    return "Има насрочена среща в този период";
            }

            return string.Empty;
        }

        /// <summary>
        /// Добавяне на среща към дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<IActionResult> AddMediationCaseSession(int caseId)
        {
            MediationCaseSessionVM model = new()
            {
                CaseId = caseId,
                DateFrom = DateTime.Now,
                MediationStateId = NomenclatureConstants.MediationStateConstants.Scheduled
            };

            model.DateFrom = model.DateFrom.AddMinutes(-model.DateFrom.Minute).AddHours(1);
            model.DateTo = model.DateFrom.AddMinutes(15);

            await SetViewBagEditMediationCaseSession(caseId);
            return View(nameof(EditMediationCaseSession), model);
        }

        /// <summary>
        /// Редакция на среща към дело
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<IActionResult> EditMediationCaseSession(int id)
        {
            MediationCaseSessionVM model = await mediationService.GetMediationCaseSessionEditById(id);

            if (model == null)
                return RedirectToAction("SessionPreviewMediation", "Mediation", new { id });

            await SetViewBagEditMediationCaseSession(model.CaseId);
            return View(nameof(EditMediationCaseSession), model);
        }

        /// <summary>
        /// Запис на среща към дело
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditMediationCaseSession(MediationCaseSessionVM model)
        {
            await SetViewBagEditMediationCaseSession(model.CaseId);

            if (!ModelState.IsValid)
            {
                return View(nameof(EditMediationCaseSession), model);
            }

            string _isvalid = await ValidateMediationCaseSession(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditMediationCaseSession), model);
            }

            bool isAdd = model.Id == 0;
            int? saveId = await mediationService.SaveMediationCaseSession(model);
            if (saveId != null)
            {
                SaveLogOperation(isAdd, saveId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                if (NomenclatureConstants.MediationStateConstants.StateForEdit.Contains(model.MediationStateId ?? 0))
                    return RedirectToAction(nameof(EditMediationCaseSession), new { id = saveId });
                else
                    return RedirectToAction("SessionPreviewMediation", "Mediation", new { id = saveId });
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            return View(nameof(EditMediationCaseSession), model);
        }

        /// <summary>
        /// Добавяне/редактиране на данни за среща за медиация
        /// </summary>
        /// <param name="id">Идентификатор на среща</param>
        /// <returns></returns>
        public async Task<IActionResult> SessionPreviewMediation(int id)
        {
            MediationSessionPreviewVM model = await mediationService.GetMediationSessionPreview(id);
            return View(nameof(SessionPreviewMediation), model);
        }

        /// <summary>
        /// Премахване на среща
        /// </summary>
        /// <param name="model">Модел с данни за обекта за премахване</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult MediationCaseSession_ExpiredInfo(ExpiredInfoVM model)
        {
            var expireObject = mediationService.GetById<MediationCaseSession>(model.Id);
            if (mediationService.SaveExpireInfo<MediationCaseSession>(model))
            {
                SetSuccessMessage("Срещата е премахната успешно");
                return Json(new { result = true, redirectUrl = Url.Action("CasePreviewMediation", "Mediation", new { id = expireObject.CaseId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        #endregion

        #region Лица в среща

        /// <summary>
        /// Извличане на данни за лице в срещи за медиация
        /// </summary>
        /// <param name="request">IDataTablesRequest</param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ListDataPeople(IDataTablesRequest request, MediationCasePersonFilterVM filter)
        {
            DataTableResponseVM<MediationCasePersonListDataVM> data = await mediationService.GetMediationCasePeople(filter, request.Start, request.Length < 0 ? 1000000 : request.Length, request.GetSortedColumnsForOrderBy());
            return request.GetResponseServerPaging(data.Records, data.TotalCount);
        }

        /// <summary>
        /// Зареждане на списъци за редактиране на лице в срещи за медиация
        /// </summary>
        /// <returns></returns>
        private async Task SetViewBagEditMediationCasePerson()
        {
            ViewBag.MediationPersonSessionStateId_ddl = await nomService.GetDropDownListAsync<MediationPersonSessionState>();
        }

        /// <summary>
        /// Редакция на лице в срещи за медиация
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<IActionResult> EditMediationCasePerson(int id)
        {
            MediationCasePersonVM model = await mediationService.GetMediationCasePersonEditById(id);
            await SetViewBagEditMediationCasePerson();
            return View(nameof(EditMediationCasePerson), model);
        }

        /// <summary>
        /// Запис на среща към дело
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditMediationCasePerson(MediationCasePersonVM model)
        {
            await SetViewBagEditMediationCasePerson();

            if (!ModelState.IsValid)
            {
                return View(nameof(EditMediationCasePerson), model);
            }

            bool isAdd = model.Id == 0;
            int? saveId = await mediationService.SaveMediationCasePerson(model);
            if (saveId != null)
            {
                SaveLogOperation(isAdd, saveId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditMediationCasePerson), new { id = saveId });
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            return View(nameof(EditMediationCasePerson), model);
        }

        #endregion

        #region Резултати в среща за медиация

        /// <summary>
        /// Извличане на данни за резултати в среща за медиация
        /// </summary>
        /// <param name="request">IDataTablesRequest</param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataResults(IDataTablesRequest request, MediationCaseSessionResultFilterVM filter)
        {
            IQueryable<MediationCaseSessionResultListDataVM> data = mediationService.GetMediationCaseSessionResults(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на списъци за добавяне/редактиране на резултати в среща за медиация
        /// </summary>
        /// <returns></returns>
        private async Task SetViewBagEditMediationCaseSessionResult()
        {
            ViewBag.MediationResultId_ddl = await nomService.GetDropDownListAsync<MediationResult>();
        }

        /// <summary>
        /// Валидация на резултати в среща за медиация преди запис
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private string ValidateMediationCaseSessionResult(MediationCaseSessionResultVM model)
        {
            if (model.MediationResultId < 1)
                return "Изберете резултат от среща";

            return string.Empty;
        }

        /// <summary>
        /// Добавяне на резултати в среща за медиация
        /// </summary>
        /// <param name="mediationCaseSessionId">Идентификатор на среща</param>
        /// <returns></returns>
        public async Task<IActionResult> AddMediationCaseSessionResult(int mediationCaseSessionId)
        {
            MediationCaseNavigationVM navigationVM = await mediationService.GetSessionDataNavigation(mediationCaseSessionId);
            MediationCaseSessionResultVM model = new()
            {
                CaseId = navigationVM.CaseId,
                CourtId = navigationVM.CourtId,
                MediationCaseSessionId = navigationVM.MediationCaseSessionId
            };

            await SetViewBagEditMediationCaseSessionResult();
            return View(nameof(EditMediationCaseSessionResult), model);
        }

        /// <summary>
        /// Редакция на резултати в среща за медиация
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<IActionResult> EditMediationCaseSessionResult(int id)
        {
            MediationCaseSessionResultVM model = await mediationService.GetMediationCaseSessionResultEditById(id);
            await SetViewBagEditMediationCaseSessionResult();
            return View(nameof(EditMediationCaseSessionResult), model);
        }

        /// <summary>
        /// Запис на резултати в среща за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditMediationCaseSessionResult(MediationCaseSessionResultVM model)
        {
            await SetViewBagEditMediationCaseSessionResult();

            if (!ModelState.IsValid)
            {
                return View(nameof(EditMediationCaseSessionResult), model);
            }

            string _isvalid = ValidateMediationCaseSessionResult(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditMediationCaseSessionResult), model);
            }

            bool isAdd = model.Id == 0;
            int? saveId = await mediationService.SaveMediationCaseSessionResult(model);
            if (saveId != null)
            {
                SaveLogOperation(isAdd, saveId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditMediationCaseSessionResult), new { id = saveId });
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            return View(nameof(EditMediationCaseSessionResult), model);
        }

        #endregion

        #region Документи към среща за медиация

        /// <summary>
        /// Извличане на данни за резултати в среща за медиация
        /// </summary>
        /// <param name="request">IDataTablesRequest</param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataDocuments(IDataTablesRequest request, MediationCaseSessionDocumentFilterVM filter)
        {
            IQueryable<MediationCaseSessionDocumentListDataVM> data = mediationService.GetMediationCaseSessionDocuments(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Валидация на резултати в среща за медиация преди запис
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private string ValidateMediationCaseSessionDocument(MediationCaseSessionDocumentVM model)
        {
            if (model.DateUpload == null)
                return "Изберете дата на качване";

            if (string.IsNullOrEmpty(model.Description))
                return "Въведете описание на файл";

            return string.Empty;
        }

        /// <summary>
        /// Добавяне на резултати в среща за медиация
        /// </summary>
        /// <param name="mediationCaseSessionId">Идентификатор на среща</param>
        /// <returns></returns>
        public async Task<IActionResult> AddMediationCaseSessionDocument(int mediationCaseSessionId)
        {
            MediationCaseNavigationVM navigationVM = await mediationService.GetSessionDataNavigation(mediationCaseSessionId);
            MediationCaseSessionDocumentVM model = new()
            {
                CaseId = navigationVM.CaseId,
                CourtId = navigationVM.CourtId,
                MediationCaseSessionId = navigationVM.MediationCaseSessionId,
                DateUpload = DateTime.Now
            };

            return View(nameof(EditMediationCaseSessionDocument), model);
        }

        /// <summary>
        /// Редакция на резултати в среща за медиация
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<IActionResult> EditMediationCaseSessionDocument(int id)
        {
            MediationCaseSessionDocumentVM model = await mediationService.GetMediationCaseSessionDocumentEditById(id);
            return View(nameof(EditMediationCaseSessionDocument), model);
        }

        /// <summary>
        /// Запис на резултати в среща за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditMediationCaseSessionDocument(MediationCaseSessionDocumentVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(EditMediationCaseSessionDocument), model);
            }

            string _isvalid = ValidateMediationCaseSessionDocument(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditMediationCaseSessionDocument), model);
            }

            bool isAdd = model.Id == 0;
            int? saveId = await mediationService.SaveMediationCaseSessionDocument(model);
            if (saveId != null)
            {
                SaveLogOperation(isAdd, saveId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditMediationCaseSessionDocument), new { id = saveId });
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            return View(nameof(EditMediationCaseSessionDocument), model);
        }

        #endregion

        #region Оценка

        /// <summary>
        /// Извличане на данни за оценка в среща за медиация
        /// </summary>
        /// <param name="request">IDataTablesRequest</param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataAppraisals(IDataTablesRequest request, MediationCaseMediatorAppraisalFilterVM filter)
        {
            IQueryable<MediationCaseMediatorAppraisalListDataVM> data = mediationService.GetMediationCaseMediatorAppraisals(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Валидация на оценка в среща за медиация преди запис
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private string ValidateMediationCaseMediatorAppraisal(MediationCaseMediatorAppraisalVM model)
        {
            if (model.MediationCaseMediatorId < 1)
                return "Изберете медиатор";

            //if (await mediationService.IsExistAppraisalMediator(model.MediationCaseMediatorId, model.MediationCaseSessionId, model.Id < 1 ? null : model.Id))
            //    return "Този медиатор има оценка в текущата среща";

            if (model.TypeAppraisal == NomenclatureConstants.MediationCaseMediatorAppraisalTypeConstants.Summary)
            {
                if (model.Appraisals.Sum(x => x.Value) > 14)
                    return "Максималният брой оценки трябва да е 14";
            }

            return string.Empty;
        }

        /// <summary>
        /// Зареждане на списъци за добавяне/редактиране на оценка в среща за медиация
        /// </summary>
        /// <param name="caseId">идентификатор на дело</param>
        /// <param name="mediationCaseSessionId">Идентификатор на среща</param>
        /// <returns></returns>
        private async Task SetViewBagEditMediationCaseMediatorAppraisal(int caseId, int mediationCaseSessionId)
        {
            ViewBag.MediationCaseMediatorId_ddl = await mediationService.GetDDL_MediationCaseMediators(caseId);
            ViewBag.MediationCasePersonId_ddl = await mediationService.GetDDL_MediationCasePerson(mediationCaseSessionId);
        }

        /// <summary>
        /// Добавяне на оценка в среща за медиация
        /// </summary>
        /// <param name="mediationCaseSessionId">Идентификатор на среща</param>
        /// <returns></returns>
        public async Task<IActionResult> AddMediationCaseMediatorAppraisal(int mediationCaseSessionId)
        {
            MediationCaseNavigationVM navigationVM = await mediationService.GetSessionDataNavigation(mediationCaseSessionId);
            MediationCaseMediatorAppraisalVM model = new()
            {
                CaseId = navigationVM.CaseId,
                CourtId = navigationVM.CourtId,
                MediationCaseSessionId = navigationVM.MediationCaseSessionId,
                DateAppraisal = DateTime.Now,
                Appraisals = await nomService.GetMediationPointMediatorAppraisals(NomenclatureConstants.MediationCaseMediatorAppraisalTypeConstants.Detailed),
                TypeAppraisal = NomenclatureConstants.MediationCaseMediatorAppraisalTypeConstants.Detailed
            };

            await SetViewBagEditMediationCaseMediatorAppraisal(navigationVM.CaseId, mediationCaseSessionId);
            return View(nameof(EditMediationCaseMediatorAppraisal), model);
        }

        /// <summary>
        /// Добавяне на оценка в среща за медиация
        /// </summary>
        /// <param name="mediationCaseSessionId">Идентификатор на среща</param>
        /// <returns></returns>
        public async Task<IActionResult> AddMediationCaseMediatorAppraisalSummary(int mediationCaseSessionId)
        {
            MediationCaseNavigationVM navigationVM = await mediationService.GetSessionDataNavigation(mediationCaseSessionId);
            MediationCaseMediatorAppraisalVM model = new()
            {
                CaseId = navigationVM.CaseId,
                CourtId = navigationVM.CourtId,
                MediationCaseSessionId = navigationVM.MediationCaseSessionId,
                DateAppraisal = DateTime.Now,
                Appraisals = await nomService.GetMediationPointMediatorAppraisals(NomenclatureConstants.MediationCaseMediatorAppraisalTypeConstants.Summary),
                TypeAppraisal = NomenclatureConstants.MediationCaseMediatorAppraisalTypeConstants.Summary
            };

            await SetViewBagEditMediationCaseMediatorAppraisal(navigationVM.CaseId, mediationCaseSessionId);
            return View(nameof(EditMediationCaseMediatorAppraisal), model);
        }

        /// <summary>
        /// Редакция на оценка в среща за медиация
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<IActionResult> EditMediationCaseMediatorAppraisal(int id)
        {
            MediationCaseMediatorAppraisalVM model = await mediationService.GetMediationCaseMediatorAppraisalEditById(id);
            await SetViewBagEditMediationCaseMediatorAppraisal(model.CaseId, model.MediationCaseSessionId);
            return View(nameof(EditMediationCaseMediatorAppraisal), model);
        }

        /// <summary>
        /// Запис на оценка в среща за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditMediationCaseMediatorAppraisal(MediationCaseMediatorAppraisalVM model)
        {
            await SetViewBagEditMediationCaseMediatorAppraisal(model.CaseId, model.MediationCaseSessionId);

            if (!ModelState.IsValid)
            {
                return View(nameof(EditMediationCaseMediatorAppraisal), model);
            }

            string _isvalid = ValidateMediationCaseMediatorAppraisal(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditMediationCaseMediatorAppraisal), model);
            }

            bool isAdd = model.Id == 0;
            int? saveId = await mediationService.SaveMediationCaseMediatorAppraisal(model);
            if (saveId != null)
            {
                SaveLogOperation(isAdd, saveId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditMediationCaseMediatorAppraisal), new { id = saveId });
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            return View(nameof(EditMediationCaseMediatorAppraisal), model);
        }

        /// <summary>
        /// Премахване на оценка
        /// </summary>
        /// <param name="model">Модел с данни за обекта за премахване</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult MediationCaseMediatorAppraisal_ExpiredInfo(ExpiredInfoVM model)
        {
            var expireObject = mediationService.GetById<MediationCaseMediatorAppraisal>(model.Id);
            if (mediationService.SaveExpireInfo<MediationCaseMediatorAppraisal>(model))
            {
                SetSuccessMessage("Оценката е премахната успешно.");
                return Json(new { result = true, redirectUrl = Url.Action("SessionPreviewMediation", "Mediation", new { id = expireObject.MediationCaseSessionId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        #endregion

        #region Заплащане на медиатори

        /// <summary>
        /// Извличане на данни за оценка в среща за медиация
        /// </summary>
        /// <param name="request">IDataTablesRequest</param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataMediationObligations(IDataTablesRequest request, MediationObligationFilterVM filter)
        {
            IQueryable<MediationObligationListDataVM> data = mediationService.GetMediationObligations(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Валидация на резултати в среща за медиация преди запис
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private string ValidateMediationObligation(MediationObligationVM model)
        {
            if (model.Amount < 1)
                return "Въведете сума по-голяма или равна на 1";

            return string.Empty;
        }

        /// <summary>
        /// Редакция на резултати в среща за медиация
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<IActionResult> EditMediationObligation(int id)
        {
            MediationObligationVM model = await mediationService.GetMediationObligationEditById(id);
            return View(nameof(EditMediationObligation), model);
        }

        /// <summary>
        /// Запис на резултати в среща за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditMediationObligation(MediationObligationVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(EditMediationObligation), model);
            }

            string _isvalid = ValidateMediationObligation(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditMediationObligation), model);
            }

            bool isAdd = model.Id == 0;
            int? saveId = await mediationService.SaveMediationObligation(model);
            if (saveId != null)
            {
                SaveLogOperation(isAdd, saveId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditMediationObligation), new { id = saveId });
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            return View(nameof(EditMediationObligation), model);
        }

        #endregion
    }
}
