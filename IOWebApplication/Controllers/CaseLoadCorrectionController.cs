using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class CaseLoadCorrectionController : BaseController
    {
        private readonly ICaseLoadCorrectionService service;
        private readonly INomenclatureService nomService;
        private readonly ICaseService caseService;

        public CaseLoadCorrectionController(ICaseLoadCorrectionService _service,
                                            INomenclatureService _nomService,
                                            ICaseService _caseService)
        {
            service = _service;
            nomService = _nomService;
            caseService = _caseService;
        }

        #region Case Load Correction Activity

        /// <summary>
        /// Страница с Коригиращи индекси за трудност на дело
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexCaseLoadCorrectionActivity()
        {
            return View();
        }

        /// <summary>
        /// Извличане на Коригиращи индекси за трудност на дело
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseLoadCorrectionActivity(IDataTablesRequest request)
        {
            var data = service.CaseLoadCorrectionActivity_Select();
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на Коригиращи индекси за трудност на дело
        /// </summary>
        /// <returns></returns>
        public IActionResult AddCaseLoadCorrectionActivity()
        {
            var caseLoadCorrection = service.GetMaxId();
            var model = new CaseLoadCorrectionActivity()
            {
                DateStart = DateTime.Now,
                IsActive = true,
                CaseGroupId = (caseLoadCorrection != null) ? caseLoadCorrection.CaseGroupId : 0,
                CaseInstanceId = (caseLoadCorrection != null) ? caseLoadCorrection.CaseInstanceId : 0
            };
            SetViewbagCaseLoadCorrectionActivity();
            return View(nameof(EditCaseLoadCorrectionActivity), model);
        }

        /// <summary>
        /// Метод за копиране на Коригиращи индекси за трудност на дело
        /// </summary>
        /// <param name="CaseLoadCorrectionActivityId"></param>
        /// <returns></returns>
        public IActionResult CopyCaseLoadCorrectionActivity(int CaseLoadCorrectionActivityId)
        {
            var loadCorrectionActivity = service.GetById<CaseLoadCorrectionActivity>(CaseLoadCorrectionActivityId);
            var model = new CaseLoadCorrectionActivity()
            {
                CaseGroupId = loadCorrectionActivity.CaseGroupId,
                CaseInstanceId = loadCorrectionActivity.CaseInstanceId,
                Label = loadCorrectionActivity.Label,
                LoadIndex = loadCorrectionActivity.LoadIndex,
                IsActive = true,
                DateStart = DateTime.Now
            };
            SetViewbagCaseLoadCorrectionActivity();
            return View(nameof(EditCaseLoadCorrectionActivity), model);
        }

        /// <summary>
        /// Редакция на Коригиращи индекси за трудност на дело
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditCaseLoadCorrectionActivity(int id)
        {
            var model = service.GetById<CaseLoadCorrectionActivity>(id);
            SetViewbagCaseLoadCorrectionActivity();
            return View(nameof(EditCaseLoadCorrectionActivity), model);
        }

        /// <summary>
        /// Валидация на Коригиращи индекси за трудност на дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidCaseLoadCorrectionActivity(CaseLoadCorrectionActivity model)
        {
            if (model.CaseGroupId < 1)
                return "Изберете основен вид дело";

            if (model.Label == string.Empty)
                return "Въведете име";

            return string.Empty;
        }

        /// <summary>
        /// запис на Коригиращи индекси за трудност на дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditCaseLoadCorrectionActivity(CaseLoadCorrectionActivity model)
        {
            SetViewbagCaseLoadCorrectionActivity();
            if (!ModelState.IsValid)
            {
                return View(nameof(EditCaseLoadCorrectionActivity), model);
            }

            string _isvalid = IsValidCaseLoadCorrectionActivity(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditCaseLoadCorrectionActivity), model);
            }

            var currentId = model.Id;
            if (service.CaseLoadCorrectionActivity_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditCaseLoadCorrectionActivity), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditCaseLoadCorrectionActivity), model);
        }

        void SetViewbagCaseLoadCorrectionActivity()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.CaseInstanceId_ddl = nomService.GetDropDownList<CaseInstance>();
        }

        /// <summary>
        /// Страница с индекси за трудност на дело към Коригиращи индекси за трудност на дело
        /// </summary>
        /// <param name="CaseLoadCorrectionActivityId"></param>
        /// <returns></returns>
        public IActionResult IndexCaseLoadCorrectionActivityIndex(int CaseLoadCorrectionActivityId)
        {
            SetViewbagCaseLoadCorrectionActivityIndex(CaseLoadCorrectionActivityId);
            return View();
        }

        /// <summary>
        /// Извличанена индекс към Коригиращи индекси за трудност на дело
        /// </summary>
        /// <param name="request"></param>
        /// <param name="CaseLoadCorrectionActivityId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseLoadCorrectionActivityIndex(IDataTablesRequest request, int CaseLoadCorrectionActivityId)
        {
            var data = service.CaseLoadCorrectionActivityIndex_Select(CaseLoadCorrectionActivityId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на индекс
        /// </summary>
        /// <param name="CaseLoadCorrectionActivityId"></param>
        /// <returns></returns>
        public IActionResult AddCaseLoadCorrectionActivityIndex(int CaseLoadCorrectionActivityId)
        {
            var model = new CaseLoadCorrectionActivityIndex()
            {
                CaseLoadCorrectionActivityId = CaseLoadCorrectionActivityId,
                DateStart = DateTime.Now,
                IsActive = true
            };
            SetViewbagCaseLoadCorrectionActivityIndex(model.CaseLoadCorrectionActivityId);
            return View(nameof(EditCaseLoadCorrectionActivityIndex), model);
        }

        /// <summary>
        /// Редакция на индекс
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditCaseLoadCorrectionActivityIndex(int id)
        {
            var model = service.GetById<CaseLoadCorrectionActivityIndex>(id);
            SetViewbagCaseLoadCorrectionActivityIndex(model.CaseLoadCorrectionActivityId);
            return View(nameof(EditCaseLoadCorrectionActivityIndex), model);
        }

        /// <summary>
        /// Валидация преди запис на индекс
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidCaseLoadCorrectionActivityIndex(CaseLoadCorrectionActivityIndex model)
        {
            if (model.CaseInstanceId < 1)
                return "Изберете вид съд";

            if (model.LoadIndex < 0)
                return "Въведете индекс";

            return string.Empty;
        }

        /// <summary>
        /// запис на индекс
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditCaseLoadCorrectionActivityIndex(CaseLoadCorrectionActivityIndex model)
        {
            SetViewbagCaseLoadCorrectionActivityIndex(model.CaseLoadCorrectionActivityId);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditCaseLoadCorrectionActivityIndex), model);
            }

            string _isvalid = IsValidCaseLoadCorrectionActivityIndex(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditCaseLoadCorrectionActivityIndex), model);
            }

            var currentId = model.Id;
            if (service.CaseLoadCorrectionActivityIndex_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditCaseLoadCorrectionActivityIndex), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditCaseLoadCorrectionActivityIndex), model);
        }

        void SetViewbagCaseLoadCorrectionActivityIndex(int CaseLoadCorrectionActivityId)
        {
            var caseLoadCorrectionActivity = service.GetById<CaseLoadCorrectionActivity>(CaseLoadCorrectionActivityId);
            ViewBag.caseLoadCorrectionActivityName = caseLoadCorrectionActivity.Label;
            ViewBag.caseLoadCorrectionActivityId = caseLoadCorrectionActivity.Id;
            ViewBag.CaseInstanceId_ddl = nomService.GetDropDownList<CaseInstance>();
        }

        #endregion

        #region Case Load Correction

        /// <summary>
        /// Страница с Коригиращи коефициенти по дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public IActionResult Index(int CaseId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLoadCorrection, null, AuditConstants.Operations.View, CaseId))
            {
                return Redirect_Denied();
            }
            var caseCase = service.GetById<Case>(CaseId);
            ViewBag.caseId = CaseId;
            ViewBag.CaseName = caseCase.RegNumber;
            SetHelpFile(HelpFileValues.CaseLoadCorrection);
            return View();
        }
        /// <summary>
        /// Извличане на информация за Коригиращи коефициенти по дело
        /// </summary>
        /// <param name="request"></param>
        /// <param name="CaseId"></param>
        /// <returns></returns>

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int CaseId)
        {
            var data = service.CaseLoadCorrection_Select(CaseId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на Коригиращи коефициенти по дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IActionResult Add(int caseId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLoadCorrection, null, AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }
            var model = new CaseLoadCorrection()
            {
                CaseId = caseId,
                CourtId = userContext.CourtId,
                CorrectionDate = DateTime.Now
            };
            SetViewbag(caseId);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на Коригиращи коефициенти по дело
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            var model = service.GetById<CaseLoadCorrection>(id);
            if (model == null)
            {
                throw new NotFoundException("Търсеният от Вас коригиращи коефициенти по дело не е намерен и/или нямате достъп до него.");
            }
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLoadCorrection, id, AuditConstants.Operations.Append, model.CaseId))
            {
                return Redirect_Denied();
            }
            SetViewbag(model.CaseId);
            return View(nameof(Edit), model);
        }

        void SetViewbag(int caseId)
        {
            var caseCase = caseService.Case_GetById(caseId);
            ViewBag.CaseName = caseCase.RegNumber;

            ViewBag.CaseLoadCorrectionActivityId_ddl = service.GetDDL_CaseLoadCorrectionActivity(caseCase.CaseGroupId, caseCase.CaseInstanceId);
            SetHelpFile(HelpFileValues.CaseLoadCorrection);
        }

        /// <summary>
        /// Валидация преди запис Коригиращи коефициенти по дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValid(CaseLoadCorrection model)
        {
            if (model.CaseLoadCorrectionActivityId < 1)
                return "Изберете вид корекция";

            if (model.CorrectionDate == null)
                return "Въведете дата на корекция";

            if (service.IsExistCaseLoadCorrection(model.Id, model.CaseId, model.CaseLoadCorrectionActivityId))
                return "Има въведен такъв коригиращ коефициент";

            return string.Empty;
        }

        /// <summary>
        /// Запис на Коригиращи коефициенти по дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CaseLoadCorrection model)
        {
            SetViewbag(model.CaseId);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }

            string _isvalid = IsValid(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(Edit), model);
            }

            var currentId = model.Id;
            if (service.CaseLoadCorrection_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseLoadCorrection, model.Id, currentId == 0);
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

        #endregion
    }
}