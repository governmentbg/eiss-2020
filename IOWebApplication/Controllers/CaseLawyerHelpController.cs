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
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class CaseLawyerHelpController : BaseController
    {
        private readonly ICaseLawyerHelpService service;
        private readonly ICommonService commonService;
        private readonly INomenclatureService nomService;
        private readonly ICaseSessionActService caseSessionActService;
        private readonly ICaseSessionService caseSessionService;

        public CaseLawyerHelpController(ICaseLawyerHelpService _service,
                                        ICommonService _commonService,
                                        ICaseSessionActService _caseSessionActService,
                                        ICaseSessionService _caseSessionService,
                                        INomenclatureService _nomService)
        {
            service = _service;
            commonService = _commonService;
            nomService = _nomService;
            caseSessionActService = _caseSessionActService;
            caseSessionService = _caseSessionService;
        }

        public IActionResult Index(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLawyerHelp, null, AuditConstants.Operations.View, id))
            {
                return Redirect_Denied();
            }
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(id);
            SetHelpFile(HelpFileValues.Lawyerhelp);

            return View(id);
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int caseId)
        {
            var data = service.CaseLawyerHelp_Select(caseId);
            return request.GetResponse(data);
        }

        public IActionResult Add(int caseId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLawyerHelp, null, AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }
            var model = new CaseLawyerHelpEditVM()
            {
                CaseId = caseId,
                CourtId = userContext.CourtId,
                HasInterestConflict = false,
                CaseLawyerHelpOtherLawyers = service.FillCaseLawyerHelpOtherLawyers(null, caseId),
                CaseLawyerHelpPeople = service.FillLeftRightSide(caseId)
            };
            SetViewbag(caseId);
            return View(nameof(Edit), model);
        }

        public IActionResult Edit(int id)
        {
            var model = service.CaseLawyerHelp_GetById(id);
            if (model == null)
            {
                throw new NotFoundException("Търсеното искане за правна помощ не е намерено и/или нямате достъп до него.");
            }
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLawyerHelp, id, AuditConstants.Operations.Update, model.CaseId))
            {
                return Redirect_Denied();
            }
            SetViewbag(model.CaseId);
            return View(nameof(Edit), model);
        }

        private void SetViewbag(int caseId)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseLawyerHelp(caseId);
            ViewBag.LawyerHelpBaseId_ddl = service.GetDDL_LawyerHelpBase(caseId);
            ViewBag.LawyerHelpTypeId_ddl = nomService.GetDropDownList<LawyerHelpType>();
            ViewBag.LawyerHelpBasisAppointmentId_ddl = nomService.GetDropDownList<LawyerHelpBasisAppointment>();
            ViewBag.CaseSessionActId_ddl = caseSessionActService.GetDropDownList_CaseSessionActByCaseBySession(caseId, null);
            ViewBag.ActAppointmentId_ddl = caseSessionActService.GetDropDownList_CaseSessionActByCaseBySession(caseId, null, true);
            ViewBag.CaseSessionToGoId_ddl = caseSessionService.GetDropDownList_CaseSessionByCase(caseId, DateTime.Now);
            SetHelpFile(HelpFileValues.Lawyerhelp);
        }

        private string IsValid(CaseLawyerHelpEditVM model)
        {
            if (model.LawyerHelpBaseId < 1)
                return "Няма избрано основание за изпращане";

            if (model.LawyerHelpTypeId < 1)
                return "Няма избран вид правна помощ";

            if (model.CaseSessionActId < 1)
                return "Няма избран акт";

            return string.Empty;
        }

        [HttpPost]
        public IActionResult Edit(CaseLawyerHelpEditVM model)
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
            if (service.CaseLawyerHelp_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseLawyerHelp, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);

                return RedirectToAction("Edit", "CaseLawyerHelp", new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(Edit), model);
        }

        [HttpPost]
        public IActionResult ListDataCaseLawyerHelpPerson(IDataTablesRequest request, int caseLawyerHelpId)
        {
            var data = service.CaseLawyerHelpPerson_Select(caseLawyerHelpId);
            return request.GetResponse(data);
        }

        public IActionResult AddCaseLawyerHelpPerson(int caseLawyerHelpId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLawyerHelpPerson, null, AuditConstants.Operations.Append, caseLawyerHelpId))
            {
                return Redirect_Denied();
            }

            var caseLawyerHelp = service.GetById<CaseLawyerHelp>(caseLawyerHelpId);
            var model = new CaseLawyerHelpPerson()
            {
                CaseLawyerHelpId = caseLawyerHelpId
            };

            SetViewbagCaseLawyerHelpPerson(caseLawyerHelp.Id, null);
            return View(nameof(EditCaseLawyerHelpPerson), model);
        }

        void SetViewbagCaseLawyerHelpPerson(int caseLawyerHelpId, int? CasePersonId)
        {
            ViewBag.CasePersonId_ddl = service.GetDDL_LeftRightSide(caseLawyerHelpId, CasePersonId);
            ViewBag.AssignedLawyerId_ddl = service.GetDDL_Lawyer(caseLawyerHelpId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseLawyerHelpEdit(caseLawyerHelpId);
            SetHelpFile(HelpFileValues.Lawyerhelp);
        }

        public IActionResult EditCaseLawyerHelpPerson(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLawyerHelpPerson, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }

            var model = service.GetById<CaseLawyerHelpPerson>(id);

            SetViewbagCaseLawyerHelpPerson(model.CaseLawyerHelpId, model.CasePersonId);
            return View(nameof(EditCaseLawyerHelpPerson), model);
        }

        private string IsValidCaseLawyerHelpPerson(CaseLawyerHelpPerson model)
        {
            if (model.CasePersonId < 1)
                return "Няма избрано лице";

            return string.Empty;
        }

        [HttpPost]
        public IActionResult EditCaseLawyerHelpPerson(CaseLawyerHelpPerson model)
        {
            SetViewbagCaseLawyerHelpPerson(model.CaseLawyerHelpId, model.CasePersonId);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditCaseLawyerHelpPerson), model);
            }

            string _isvalid = IsValidCaseLawyerHelpPerson(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditCaseLawyerHelpPerson), model);
            }

            var currentId = model.Id;
            if (service.CaseLawyerHelpPerson_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseLawyerHelpPerson, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);

                return RedirectToAction("Edit", "CaseLawyerHelp", new { id = model.CaseLawyerHelpId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }

            return View(nameof(EditCaseLawyerHelpPerson), model);
        }

        void SetViewbagCaseLawyerHelpPersonMulti(int caseLawyerHelpId)
        {
            ViewBag.AssignedLawyerId_ddl = service.GetDDL_Lawyer(caseLawyerHelpId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseLawyerHelpEdit(caseLawyerHelpId);
            SetHelpFile(HelpFileValues.Lawyerhelp);
        }

        public IActionResult EditMultiCaseLawyerHelpPerson(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLawyerHelp, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }

            var model = service.CaseLawyerHelpPersonMultiEdit_Get(id);

            SetViewbagCaseLawyerHelpPersonMulti(id);
            return View(nameof(EditMultiCaseLawyerHelpPerson), model);
        }

        private string IsValidMultiCaseLawyerHelpPerson(CaseLawyerHelpPersonMultiEditVM model)
        {
            if (model.CaseLawyerHelpPeople == null)
                return "Няма избрани лица";

            if (!model.CaseLawyerHelpPeople.Any(x => x.Checked))
                return "Няма избрани лица";

            return string.Empty;
        }

        [HttpPost]
        public IActionResult EditMultiCaseLawyerHelpPerson(CaseLawyerHelpPersonMultiEditVM model)
        {
            SetViewbagCaseLawyerHelpPersonMulti(model.CaseLawyerHelpId);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditMultiCaseLawyerHelpPerson), model);
            }

            string _isvalid = IsValidMultiCaseLawyerHelpPerson(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditMultiCaseLawyerHelpPerson), model);
            }

            var currentId = model.CaseLawyerHelpId;
            if (service.CaseLawyerHelpPersonMulti_UpdateData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseLawyerHelp, model.CaseLawyerHelpId, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.CaseLawyerHelpId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);

                return RedirectToAction("Edit", "CaseLawyerHelp", new { id = model.CaseLawyerHelpId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }

            return View(nameof(EditMultiCaseLawyerHelpPerson), model);
        }

        [HttpPost]
        public IActionResult CaseLawyerHelpPerson_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLawyerHelpPerson, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }

            var expireObject = service.GetById<CaseLawyerHelpPerson>(model.Id);
            if (service.SaveExpireInfo<CaseLawyerHelpPerson>(model))
            {
                SetSuccessMessage(MessageConstant.Values.CaseLoadIndexExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("Edit", "CaseLawyerHelp", new { id = expireObject.CaseLawyerHelpId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        [HttpPost]
        public IActionResult CaseLawyerHelp_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLawyerHelp, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }

            if (service.IsExistPerson_CaseLawyerHelp(model.Id))
            {
                return Json(new { result = false, message = "Има активни лица по това искане." });
            }

            if (service.IsExistDocumentTemplate_CaseLawyerHelp(model.Id))
            {
                return Json(new { result = false, message = "Има активно писмо." });
            }

            var expireObject = service.GetById<CaseLawyerHelp>(model.Id);
            if (service.SaveExpireInfo<CaseLawyerHelp>(model))
            {
                SetSuccessMessage(MessageConstant.Values.CaseLoadIndexExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("Index", "CaseLawyerHelp", new { id = expireObject.CaseId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }
    }
}
