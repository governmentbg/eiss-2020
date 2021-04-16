using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Core.Services;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IOWebApplication.Controllers
{
    public class CasePersonLinkController : BaseController
    {
        private readonly ICasePersonLinkService service;
        private readonly INomenclatureService nomService;
        private readonly ICasePersonService casePersonService;
        private readonly ICaseSessionService sessionService;
        private readonly ICommonService commonService;

        public CasePersonLinkController(ICasePersonLinkService _service, INomenclatureService _nomService, ICasePersonService _casePersonService,
            ICaseSessionService _sessionService, ICommonService _commonService)
        {
            service = _service;
            nomService = _nomService;
            casePersonService = _casePersonService;
            sessionService = _sessionService;
            commonService = _commonService;
        }

        /// <summary>
        /// Връзки между страни
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Index(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonLink, null, AuditConstants.Operations.View, id))
            {
                return Redirect_Denied();
            }

            ViewBag.caseId = id;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonLink(id).DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.CasePerson);
            return View();
        }

        /// <summary>
        /// Извличане на данни Връзки между страни
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int caseId)
        {
            var data = service.CasePersonLink_Select(caseId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на Връзки между страни
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IActionResult Add(int caseId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonLink, null, AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }
            SetViewbag(caseId, 0, 0);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonLinkEdit(caseId, 0).DeleteOrDisableLast();
            var model = new CasePersonLink()
            {
                CaseId = caseId,
                CourtId = userContext.CourtId,
                CaseSessionId = null,
                DateFrom = DateTime.Now
            };
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Добавяне лява/дясна страна във Връзки между страни
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IActionResult AddSide(int caseId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonLink, null, AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }
            ViewBag.personIdsJson = null;
            SetViewbagSide(caseId, 0);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonLinkEdit(caseId, 0).DeleteOrDisableLast();
            var model = new CasePersonLinkSideVM()
            {
                CaseId = caseId,
                CourtId = userContext.CourtId,
                DateFrom = DateTime.Now
            };
            return View(nameof(AddSide), model);
        }
        [HttpPost]
        public IActionResult AddSide(CasePersonLinkSideVM model, string personIdsJson)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonLink, null, AuditConstants.Operations.Append, model.CaseId))
            {
                return Redirect_Denied();
            }
            var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
            List<int> personIds = JsonConvert.DeserializeObject<List<int>>(personIdsJson, dateTimeConverter);

            SetViewbagSide(model.CaseId, model.LinkDirectionId);
            ViewBag.personIdsJson = personIdsJson;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonLinkEdit(model.CaseId, 0).DeleteOrDisableLast();
            if (!ModelState.IsValid)
            {
                return View(nameof(AddSide), model);
            }
            service.Save_AddSide(model, personIds);
            SetSuccessMessage(MessageConstant.Values.SaveOK);
            return RedirectToAction(nameof(Index), new { id = model.CaseId});
        }
        
        /// <summary>
        /// Редакция на Връзки между страни
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonLink, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.GetById<CasePersonLink>(id);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonLinkEdit(model.CaseId, model.Id).DeleteOrDisableLast();
            SetViewbag(model.CaseId, model.CasePersonId, model.LinkDirectionId);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на Връзки между страни
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CasePersonLink model)
        {
            SetViewbag(model.CaseId, model.CasePersonId, model.LinkDirectionId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonLinkEdit(model.CaseId, model.Id).DeleteOrDisableLast();
            ValidateModel(model);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;
            if (service.CasePersonLink_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CasePersonLink, model.Id, currentId == 0);
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

        /// <summary>
        /// Валидация преди запис на Връзки между страни
        /// </summary>
        /// <param name="model"></param>
        private void ValidateModel(CasePersonLink model)
        {
            if (model.LinkDirectionSecondId > 0 && model.CasePersonSecondRelId <= 0)
                ModelState.AddModelError(nameof(CasePersonLink.CasePersonSecondRelId), "Изберете Втори представляващ");
            if (model.CasePersonSecondRelId == model.CasePersonRelId)
                ModelState.AddModelError(nameof(CasePersonLink.CasePersonSecondRelId), "Трябва да е различно лице от Упълномощено лице");
            if (model.CasePersonSecondRelId == model.CasePersonId)
                ModelState.AddModelError(nameof(CasePersonLink.CasePersonSecondRelId), "Трябва да е различно лице от Страна");
            if (model.CasePersonRelId == model.CasePersonId)
                ModelState.AddModelError(nameof(CasePersonLink.CasePersonRelId), "Трябва да е различно лице от Страна");
        }


        void SetViewbag(int caseId, int casePersonId, int linkDirectionId)
        {
            ViewBag.CaseName = service.GetById<Case>(caseId).RegNumber;

            ViewBag.CasePersonId_ddl = casePersonService.CasePerson_SelectForDropDownList(caseId, null);
            ViewBag.CasePersonRelId_ddl = service.RelationalPersonDDL(caseId, linkDirectionId);
            ViewBag.CasePersonSecondRelId_ddl = service.SeccondRelationalPersonDDL(caseId);

            ViewBag.LinkDirectionId_ddl = service.LinkDirectionForPersonDDL(casePersonId);
            ViewBag.LinkDirectionSecondId_ddl = service.SecondLinkDirectionDDL();
            SetHelpFile(HelpFileValues.CasePerson);
        }

        void SetViewbagSide(int caseId, int linkDirectionId)
        {
            ViewBag.CaseName = service.GetById<Case>(caseId).RegNumber;
            ViewBag.LinkDirectionId_ddl = nomService.GetDropDownList<LinkDirection>()
                                                    .Where(x => x.Value != NomenclatureConstants.LinkDirectionType.RepresentSecond.ToString())
                                                    .ToList();
            ViewBag.RoleKindId_ddl = service.RoleKindDDL();
            ViewBag.CasePersonRelId_ddl = service.PersonYDDL(caseId, linkDirectionId);
            SetHelpFile(HelpFileValues.CasePerson);
        }

        [HttpGet]
        public IActionResult FilterLinkDirection(int casePersonId)
        {
            List<SelectListItem> ddlLinkDirection = service.LinkDirectionForPersonDDL(casePersonId);
            return Json(new { ddlLinkDirection });
        }
        [HttpGet]
        public IActionResult FilterRelationalPerson(int caseId, int linkDirectionId)
        {
            List<SelectListItem> ddlPersonRel = service.RelationalPersonDDL(caseId, linkDirectionId);
            return Json(new { ddlPersonRel });
        }

        [HttpGet]
        public IActionResult FilterPersonY(int caseId, int linkDirectionId)
        {
            List<SelectListItem> ddlPersonRel = service.PersonYDDL(caseId, linkDirectionId);
            return Json(new { ddlPersonRel });
        }
        public JsonResult GetPersonXBySide(int caseId, int roleKindId)
        {
            return Json(service.GetPersonXBySide(caseId, roleKindId));
        }

        [HttpPost]
        public IActionResult CasPersonLink_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonLink, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }
            if (service.HaveCaseNotification(model.Id))
            {
                return Json(new { result = false, message = "Има активни уведомления с тази връзка" });
            }
            if (service.SaveExpireInfo<CasePersonLink>(model))
            {
                SetAuditContextDelete(service, SourceTypeSelectVM.CasePersonLink, model.Id);
                SetSuccessMessage(MessageConstant.Values.CasePersonLinkExpireOK);
                return Json(new { result = true, redirectUrl = model.ReturnUrl });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }
    }
}