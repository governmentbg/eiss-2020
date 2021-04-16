using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class CaseClassificationController : BaseController
    {
        private readonly ICaseClassificationService service;
        private readonly ICommonService commonService;

        public CaseClassificationController(ICaseClassificationService _service, ICommonService _commonService)
        {
            service = _service;
            commonService = _commonService;
        }

        /// <summary>
        /// Страница за индикаторите по дело/заседание
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Зареждане за избор на индикаторите по дело/заседание
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CaseClassification(int caseId, int? caseSessionId)
        {
            SetViewBag(caseId, caseSessionId);
            return View("CheckListViewVM", service.CaseClassification_SelectForCheck(caseId, caseSessionId));
        }

        /// <summary>
        /// Запис на чекнатите индикаторите по дело/заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseClassification(CheckListViewVM model)
        {
            if (service.CaseClassification_SaveData(model))
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            SetViewBag(model.CourtId, model.ObjectId);
            return View("CheckListViewVM", service.CaseClassification_SelectForCheck(model.CourtId, model.ObjectId));
        }

        void SetViewBag(int caseId, int? caseSessionId)
        {
            if ((caseSessionId ?? 0) <= 0)
            {
                ViewBag.backUrl = Url.Action("CasePreview", "Case", new { id = caseId });
                ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(caseId);
            }
            else
            {
                ViewBag.backUrl = Url.Action("Preview", "CaseSession", new { id = caseSessionId });
                ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSession(caseSessionId ?? 0);
            }
        }
    }
}