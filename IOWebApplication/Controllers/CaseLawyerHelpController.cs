using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class CaseLawyerHelpController : BaseController
    {
        private readonly ICaseLawyerHelpService service;
        private readonly ICommonService commonService;
        private readonly INomenclatureService nomService;
        private readonly ICaseSessionActService caseSessionActService;
        private readonly ICaseSessionService caseSessionService;
        private readonly ICasePersonService casePersonService;

        public CaseLawyerHelpController(ICaseLawyerHelpService _service,
                                        ICommonService _commonService,
                                        ICaseSessionActService _caseSessionActService,
                                        ICaseSessionService _caseSessionService,
                                        ICasePersonService _casePerson,
                                        INomenclatureService _nomService)
        {
            service = _service;
            commonService = _commonService;
            nomService = _nomService;
            caseSessionActService = _caseSessionActService;
            caseSessionService = _caseSessionService;
            casePersonService = _casePerson;
        }

        [TitleAudit(Operation = AuditConstants.Operations.List)]
        public async Task<IActionResult> Index(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseLawyerHelp, null, AuditConstants.Operations.View, id))
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

        void auditInfo(string operation, CaseLawyerHelpEditVM model, string add = "")
        {
            if (model != null)
            {
                AddAuditInfo(operation, $"Основание за изпращане: {model.LawyerHelpBaseLabel} Вид правна помощ: {model.LawyerHelpTypeLabel}", add, $"Искане за правна помощ по дело {model.CaseName}");
            }
        }

        public async Task<IActionResult> Add(int caseId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseLawyerHelp, null, AuditConstants.Operations.Append, caseId))
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

        public async Task<IActionResult> Edit(int id)
        {
            var model = service.CaseLawyerHelp_GetById(id);
            if (model == null)
            {
                return NotFoundError("Търсеното искане за правна помощ не е намерено и/или нямате достъп до него.");
            }
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseLawyerHelp, id, AuditConstants.Operations.Update, model.CaseId))
            {
                return Redirect_Denied();
            }
            SetViewbag(model.CaseId);
            auditInfo(AuditConstants.Operations.View, model);
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

            if (model.Id < 1)
            {
                if (model.CaseLawyerHelpPeople == null || model.CaseLawyerHelpPeople.Where(x => x.Checked).ToList().Count < 1)
                    return "Няма избрани лица, за които се иска правна помощ";
            }
            else
            {
                var caseLawyerHelpPeople = service.CaseLawyerHelpPerson_Select(model.Id);
                if (caseLawyerHelpPeople.Count() < 1)
                    return "Няма избрани лица, за които се иска правна помощ";
            }

            return string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CaseLawyerHelpEditVM model)
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
            if (await service.CaseLawyerHelp_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseLawyerHelp, model.Id, currentId == 0);
                auditInfo(currentId == 0 ? AuditConstants.Operations.Append : AuditConstants.Operations.Update, model);
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

        void auditInfoCaseLawyerHelpPerson(string operation, int id, string add = "")
        {
            var caseLawyerHelpPerson = service.CaseLawyerHelpPerson_GetById(id);
            if (caseLawyerHelpPerson != null)
            {
                AddAuditInfo(operation, $"Лице: {caseLawyerHelpPerson.CasePersonText} Искан адвокат от лицето: {caseLawyerHelpPerson.SpecifiedLawyerLawUnitLabel}", add, $"Данни за служебен защитник по дело {caseLawyerHelpPerson.CaseName}");
            }
        }

        public async Task<IActionResult> AddCaseLawyerHelpPerson(int caseLawyerHelpId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseLawyerHelpPerson, null, AuditConstants.Operations.Append, caseLawyerHelpId))
            {
                return Redirect_Denied();
            }

            var caseLawyerHelp = await service.GetByIdAsync<CaseLawyerHelp>(caseLawyerHelpId);
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
            //ViewBag.CasePersonAddressId_ddl = casePersonService.GetDDL_CasePersonAddress(CasePersonId ?? 0);
            SetHelpFile(HelpFileValues.Lawyerhelp);
        }

        [HttpGet]
        public IActionResult GetDDL_CasePersonAddress(int CasePersonId)
        {
            var model = casePersonService.GetDDL_CasePersonAddress(CasePersonId);
            return Json(model);
        }

        public async Task<IActionResult> EditCaseLawyerHelpPerson(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseLawyerHelpPerson, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }

            var model = await service.GetByIdAsync<CaseLawyerHelpPerson>(id);

            SetViewbagCaseLawyerHelpPerson(model.CaseLawyerHelpId, model.CasePersonId);
            auditInfoCaseLawyerHelpPerson(AuditConstants.Operations.View, id);
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
                auditInfoCaseLawyerHelpPerson(currentId == 0 ? AuditConstants.Operations.Append : AuditConstants.Operations.Update, model.Id);
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

        public async Task<IActionResult> EditMultiCaseLawyerHelpPerson(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseLawyerHelp, id, AuditConstants.Operations.Update))
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
        public async Task<IActionResult> CaseLawyerHelpPerson_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseLawyerHelpPerson, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }

            var expireObject = await service.GetByIdAsync<CaseLawyerHelpPerson>(model.Id);
            if (service.SaveExpireInfo<CaseLawyerHelpPerson>(model))
            {
                auditInfoCaseLawyerHelpPerson(AuditConstants.Operations.Delete, model.Id);
                SetSuccessMessage(MessageConstant.Values.CaseLoadIndexExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("Edit", "CaseLawyerHelp", new { id = expireObject.CaseLawyerHelpId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CaseLawyerHelp_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseLawyerHelp, model.Id, AuditConstants.Operations.Delete))
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

            var expireObject = await service.GetByIdAsync<CaseLawyerHelp>(model.Id);
            if (service.SaveExpireInfo<CaseLawyerHelp>(model))
            {
                var caseLawyerHelpEdit = service.CaseLawyerHelp_GetById(model.Id);
                auditInfo(AuditConstants.Operations.Delete, caseLawyerHelpEdit);
                SetSuccessMessage(MessageConstant.Values.CaseLoadIndexExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("Index", "CaseLawyerHelp", new { id = expireObject.CaseId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        [HttpPost]
        public IActionResult ListDataCaseLawyerHelpAssignedLawyer(IDataTablesRequest request, int caseLawyerHelpId)
        {
            var data = service.CaseLawyerHelpAssignedLawyer_Select(caseLawyerHelpId);
            return request.GetResponse(data);
        }

        //public IActionResult CaseLawyerHelpAssignedLawyer_Confirmed(int id, int CaseLawyerHelpId)
        //{
        //    //if (!CheckAccess(service, SourceTypeSelectVM.CaseLawyerHelpPerson, model.Id, AuditConstants.Operations.Delete))
        //    //{
        //    //    return Redirect_Denied();
        //    //}
            
        //    if (service.CaseLawyerHelpAssignedLawyer_ChnageState(id, NomenclatureConstants.EesppLawyerState.Confirmed, null))
        //    {
        //        //auditInfoCaseLawyerHelpPerson(AuditConstants.Operations.Delete, model.Id);
        //        SetSuccessMessage(MessageConstant.Values.SaveOK);
        //        return RedirectToAction("Edit", "CaseLawyerHelp", new { id = CaseLawyerHelpId });
        //    }
        //    else
        //    {
        //        SetErrorMessage(MessageConstant.Values.SaveFailed);
        //        return RedirectToAction("Edit", "CaseLawyerHelp", new { id = CaseLawyerHelpId });
        //    }
        //}

        //public IActionResult CaseLawyerHelpAssignedLawyer_Declined(int id, int CaseLawyerHelpId)
        //{
        //    //if (!CheckAccess(service, SourceTypeSelectVM.CaseLawyerHelpPerson, model.Id, AuditConstants.Operations.Delete))
        //    //{
        //    //    return Redirect_Denied();
        //    //}

        //    if (service.CaseLawyerHelpAssignedLawyer_ChnageState(id, NomenclatureConstants.EesppLawyerState.Declined, null))
        //    {
        //        //auditInfoCaseLawyerHelpPerson(AuditConstants.Operations.Delete, model.Id);
        //        SetSuccessMessage(MessageConstant.Values.SaveOK);
        //        return RedirectToAction("Edit", "CaseLawyerHelp", new { id = CaseLawyerHelpId });
        //    }
        //    else
        //    {
        //        SetErrorMessage(MessageConstant.Values.SaveFailed);
        //        return RedirectToAction("Edit", "CaseLawyerHelp", new { id = CaseLawyerHelpId });
        //    }
        //}

        public async Task<IActionResult> CaseLawyerHelpAssignedLawyer_Finish(int CaseId, int id, int CaseLawyerHelpId)
        {
            //if (!CheckAccess(service, SourceTypeSelectVM.CaseLawyerHelpPerson, model.Id, AuditConstants.Operations.Delete))
            //{
            //    return Redirect_Denied();
            //}

            if (await service.CaseLawyerHelpAssignedLawyer_ChangeState(CaseId, id, null, DateTime.Now))
            {
                //auditInfoCaseLawyerHelpPerson(AuditConstants.Operations.Delete, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction("Edit", "CaseLawyerHelp", new { id = CaseLawyerHelpId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
                return RedirectToAction("Edit", "CaseLawyerHelp", new { id = CaseLawyerHelpId });
            }
        }

        public async Task<IActionResult> EditCaseLawyerHelpAssignedLawyer(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseLawyerHelpAssignedLawyer, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }

            var model = service.CaseLawyerHelpAssignedLawyerEditVM_GetById(id);

            SetViewbagCaseLawyerHelpAssignedLawyer(model.CaseId);
            auditInfoCaseLawyerHelpAssignedLawyer(AuditConstants.Operations.View, id);
            return View(nameof(EditCaseLawyerHelpAssignedLawyer), model);
        }

        private string IsValidEditCaseLawyerHelpAssignedLawyer(CaseLawyerHelpAssignedLawyerEditVM model)
        {
            if (model.CaseSessionActAssignedId < 1)
                return "Няма избран акт";

            if (model.LawyerStateId < 1)
                return "Няма избран статус";

            return string.Empty;
        }

        [HttpPost]
        public IActionResult EditCaseLawyerHelpAssignedLawyer(CaseLawyerHelpAssignedLawyerEditVM model)
        {
            SetViewbagCaseLawyerHelpAssignedLawyer(model.CaseId);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditCaseLawyerHelpAssignedLawyer), model);
            }

            string _isvalid = IsValidEditCaseLawyerHelpAssignedLawyer(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditCaseLawyerHelpAssignedLawyer), model);
            }

            var currentId = model.Id;
            if (service.CaseLawyerHelpAssignedLawyer_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseLawyerHelpAssignedLawyer, model.Id, currentId == 0);
                auditInfoCaseLawyerHelpAssignedLawyer(currentId == 0 ? AuditConstants.Operations.Append : AuditConstants.Operations.Update, model.Id);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);

                return RedirectToAction("Edit", "CaseLawyerHelp", new { id = model.CaseLawyerHelpId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }

            return View(nameof(EditCaseLawyerHelpAssignedLawyer), model);
        }

        void auditInfoCaseLawyerHelpAssignedLawyer(string operation, int id, string add = "")
        {
            var caseLawyerHelpAssignedLawyerVM = service.CaseLawyerHelpAssignedLawyerVM_GetById(id);
            if (caseLawyerHelpAssignedLawyerVM != null)
            {
                AddAuditInfo(operation, $"Върнати адвокати по заявка за правна помощ от ЕЕСПП: {caseLawyerHelpAssignedLawyerVM.Lawyer} за лице/лица: {caseLawyerHelpAssignedLawyerVM.People}", add, $"Върнати адвокати по заявка за правна помощ от ЕЕСПП по дело {caseLawyerHelpAssignedLawyerVM.CaseName}");
            }
        }

        void SetViewbagCaseLawyerHelpAssignedLawyer(int CaseId)
        {
            ViewBag.CaseSessionActAssignedId_ddl = caseSessionActService.GetDropDownList_CaseSessionActByCaseBySession(CaseId, null);
            ViewBag.LawyerStateId_ddl = nomService.GetDropDownList<EesppLawyerState>();
            SetHelpFile(HelpFileValues.Lawyerhelp);
        }
    }
}
