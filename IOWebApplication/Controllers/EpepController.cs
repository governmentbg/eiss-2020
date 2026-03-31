using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Epep;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rotativa.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{

    public class EpepController : BaseController
    {
        //private readonly IEpepConnectionService connector;
        private readonly IMQEpepService service;
        private readonly INomenclatureService nomService;
        private readonly ICasePersonService casePersonService;
        private readonly ICaseNotificationService caseNotificationService;
        private readonly ICommonService commonService;

        private readonly string EPEP_FILE_PATH;

        public EpepController(
            //IEpepConnectionService _connector,
            IMQEpepService _service,
            INomenclatureService _nomService,
            ICasePersonService _casePersonService,
            ICaseNotificationService _caseNotificationService,
            IConfiguration config,
            ICommonService _commonService
            )
        {
            //connector = _connector;
            service = _service;
            nomService = _nomService;
            casePersonService = _casePersonService;
            caseNotificationService = _caseNotificationService;
            commonService = _commonService;
            EPEP_FILE_PATH = config.GetValue<string>("EPEP:FileDownloadPath", "https://ecase.justice.bg/api/file/download/");
        }

        public IActionResult EpepUser()
        {
            SetHelpFile(HelpFileValues.Nom7);
            ViewBag.EpepUserTypeId_ddl = nomService.GetDropDownList<EpepUserType>();
            addToAudit(AuditConstants.Operations.List, new Infrastructure.Data.Models.Common.EpepUser());
            return View();
        }
        [HttpPost]
        public IActionResult EpepUser_ListData(IDataTablesRequest request, EpepUserFilterVM filter)
        {
            var data = service.EpepUser_Select(filter);
            return request.GetResponse(data);
        }

        public IActionResult EpepUser_Add(long? documentId = null)
        {
            var model = service.EpepUser_InitFromDocument(documentId);

            SetViewBag_EpepUser(model);
            addToAudit(AuditConstants.Operations.View, model);
            return View(nameof(EpepUser_Edit), model);
        }
        //public async Task<IActionResult> EpepUser_AddFromEmail(string email)
        //{
        //    var model = await EpepUser_LoadDataByEmail(email);
        //    SetViewBag_EpepUser(model);
        //    addToAudit(AuditConstants.Operations.View, model);
        //    return View(nameof(EpepUser_Edit), model);
        //}
        public IActionResult EpepUser_Edit(int id)
        {
            var model = service.GetById<EpepUser>(id);
            if (model.DateExpired != null)
            {
                return Redirect_Denied("Търсения от Вас обект не беше намерен!");
            }
            SetViewBag_EpepUser(model);
            addToAudit(AuditConstants.Operations.View, model);
            return View(nameof(EpepUser_Edit), model);
        }
        [HttpPost]
        public IActionResult EpepUser_Edit(EpepUser model)
        {
            //if (model.Id == 0)
            //{
            //    await Epep_Validation(model);
            //}
            var error = service.EpepUser_Validate(model);
            if (!string.IsNullOrEmpty(error))
            {
                ModelState.AddModelError("", error);
            }
            if (!ModelState.IsValid)
            {
                SetViewBag_EpepUser(model);
                return View(nameof(EpepUser_Edit), model);
            }
            int currentId = model.Id;
            if (service.EpepUser_SaveData(model))
            {
                SaveLogOperation(currentId == 0, model.Id);
                if (currentId == 0)
                {
                    addToAudit(AuditConstants.Operations.Append, model);
                }
                else
                {
                    addToAudit(AuditConstants.Operations.Update, model);
                }
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EpepUser_Edit), new { id = model.Id });
            }
            else
            {
                SetSuccessMessage(MessageConstant.Values.SaveFailed);
                SetViewBag_EpepUser(model);
                return View(nameof(EpepUser_Edit), model);
            }
        }
        //private async Task Epep_Validation(EpepUser model)
        //{
        //    try
        //    {
        //        epepClient = await connector.Connect();
        //        var regInfo = await epepClient.GetUserRegistrationInfoByUsernameAsync(model.Email.ToLower());
        //        if (regInfo.IsRegistered)
        //        {
        //            if (!regInfo.LawyerRegistrationId.IsEmpty())
        //            {
        //                if (model.EpepUserTypeId == EpepConstants.UserTypes.Lawyer)
        //                {
        //                    return;
        //                }
        //                ModelState.AddModelError("", "Съществува създаден потребител на адвокат в ЕПЕП с тази електронна поща.");
        //            }
        //            if (!regInfo.PersonRegistrationId.IsEmpty())
        //            {
        //                if (model.EpepUserTypeId == EpepConstants.UserTypes.Person)
        //                {
        //                    return;
        //                }
        //                ModelState.AddModelError("", "Съществува създаден потребител на частно лице в ЕПЕП с тази електронна поща.");
        //            }
        //            ViewBag.hasEpepUserOtherType = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    finally
        //    {
        //        if (epepClient != null && epepClient.State == CommunicationState.Opened)
        //        {
        //            epepClient.Close();
        //        }
        //    }
        //}
        void addToAudit(string operation, EpepUser model)
        {
            var baseInfo = string.Empty;
            var addInfo = string.Empty;
            var operationType = $"ЕПЕП - Потребител";
            if (model.Id > 0)
            {
                var userType = service.GetPropById<EpepUserType, string>(x => x.Id == model.EpepUserTypeId, x => x.Label);
                baseInfo = $"{model.FullName} ({userType})";
                addInfo = $"Електронна поща: {model.Email}";
            }

            AddAuditInfo(operation, baseInfo, addInfo, operationType);
        }
        //private async Task<EpepUser> EpepUser_LoadDataByEmail(string email)
        //{
        //    EpepUser model = new EpepUser();
        //    try
        //    {
        //        epepClient = await connector.Connect();
        //        var regInfo = await epepClient.GetUserRegistrationInfoByUsernameAsync(email.ToLower());
        //        if (regInfo.IsRegistered)
        //        {
        //            if (!regInfo.LawyerRegistrationId.IsEmpty())
        //            {
        //                var epepLawyer = await epepClient.GetLawyerRegistrationByIdAsync(regInfo.LawyerRegistrationId.Value);
        //                if (epepLawyer != null)
        //                {
        //                    model.EpepUserTypeId = EpepConstants.UserTypes.Lawyer;
        //                    model.BirthDate = epepLawyer.BirthDate;
        //                    model.Email = epepLawyer.Email;
        //                    var lawyers = await epepClient.GetAllLawyersAsync();
        //                    var regLawyer = lawyers.Where(x => x.LawyerId == epepLawyer.LawyerId).FirstOrDefault();
        //                    if (regLawyer != null)
        //                    {
        //                        model.LawyerNumber = regLawyer.Number;

        //                        var lawunit = service.GetLawyerByNumber(model.LawyerNumber);
        //                        if (lawunit != null)
        //                        {
        //                            model.LawyerLawUnitId = lawunit.Id;
        //                            model.FullName = lawunit.FullName;
        //                        }
        //                        else
        //                        {
        //                            ModelState.AddModelError(nameof(model.LawyerLawUnitId), $"Ненамерен адвокат {regLawyer.Name} с номер {regLawyer.Number}.");
        //                        }
        //                    }
        //                }
        //            }
        //            if (!regInfo.PersonRegistrationId.IsEmpty())
        //            {
        //                var epepPerson = await epepClient.GetPersonRegistrationByIdAsync(regInfo.PersonRegistrationId.Value);
        //                if (epepPerson != null)
        //                {
        //                    model.EpepUserTypeId = EpepConstants.UserTypes.Person;
        //                    model.Uic = epepPerson.EGN;
        //                    model.FullName = epepPerson.Name;
        //                    model.BirthDate = epepPerson.BirthDate;
        //                    model.Address = epepPerson.Address;
        //                }
        //            }
        //        }

        //    }
        //    catch (Exception ex) { }
        //    finally
        //    {
        //        if (epepClient != null && epepClient.State == CommunicationState.Opened)
        //        {
        //            epepClient.Close();
        //        }
        //    }
        //    return model;
        //}

        private void SetViewBag_EpepUser(EpepUser model)
        {
            ViewBag.EpepUserTypeId_ddl = nomService.GetDropDownList<EpepUserType>();
            ViewBag.breadcrumbs = new List<BreadcrumbsVM>()
            {
                { new BreadcrumbsVM(){
                Title = "ЕПЕП - Потребители",
                Href = Url.Action("EpepUser", "Epep")}
                }
            };
            ViewBag.documentInfo = service.EpepUser_DocumentInfo(model.DocumentId);
            ViewBag.epepUserType = service.GetById<EpepUserType>(model.EpepUserTypeId).Label;
            SetHelpFile(HelpFileValues.Nom7);
        }

        [HttpPost]
        public IActionResult EpepUserAssignment_ListData(IDataTablesRequest request, int epepUserId)
        {
            var data = service.EpepUserAssignment_Select(epepUserId);
            return request.GetResponse(data);
        }

        async Task addToAuditAssignment(string operation, EpepUserAssignment model)
        {
            var _user = await service.GetReadonlyAsync<EpepUser>(model.EpepUserId);
            var userType = await service.GetPropByIdAsync<EpepUserType, string>(x => x.Id == _user.EpepUserTypeId, x => x.Label);
            var baseInfo = $"{_user.FullName} ({userType})";

            var addInfo = string.Empty;
            var operationType = $"ЕПЕП - Дела";
            if (model.Id > 0)
            {
                var epepCase = await service.GetPropByIdAsync<EpepUserAssignment, dynamic>(x => x.Id == model.Id, x => new
                {
                    x.CaseId,
                    x.CanSummon
                });
                addInfo = await service.GetCaseInfoById(epepCase.CaseId);
                if (epepCase.CanSummon == true)
                {
                    addInfo += " (ЕП)";
                }
            }

            AddAuditInfo(operation, baseInfo, addInfo, operationType);
        }

        [DisableAudit]
        public IActionResult EpepUserAssignment_Add(int epepUserId)
        {
            var model = new EpepUserAssignment()
            {
                EpepUserId = epepUserId,
                CourtId = userContext.CourtId,
                DateFrom = DateTime.Now
            };
            SetViewBag_EpepUserAssignment(model);
            //addToAuditAssignment(AuditConstants.Operations.View, model);
            return View(nameof(EpepUserAssignment_Edit), model);
        }
        public async Task<IActionResult> EpepUserAssignment_Edit(int id)
        {
            var model = service.GetById<EpepUserAssignment>(id);

            SetViewBag_EpepUserAssignment(model);
            await addToAuditAssignment(AuditConstants.Operations.View, model);
            return View(nameof(EpepUserAssignment_Edit), model);
        }
        [HttpPost]
        public async Task<IActionResult> EpepUserAssignment_Edit(EpepUserAssignment model)
        {
            model.CourtId = userContext.CourtId;
            var _case = service.GetReadonly<IOWebApplication.Infrastructure.Data.Models.Cases.Case>(model.CaseId);
            if (_case == null)
            {
                ModelState.AddModelError("CaseId", "Изберете дело.");
            }
            else
            {
                if (_case.CourtId != userContext.CourtId)
                {
                    ModelState.AddModelError("CaseId", "Нямате достъп до избраното дело.");
                }
                if (NomenclatureConstants.CaseState.DisableEditStates.Contains(_case.CaseStateId))
                {
                    ModelState.AddModelError("CaseId", "Избраното дело е анулирано или унищожено!");
                }
            }
            if (model.CasePersonId <= 0)
            {
                ModelState.AddModelError("CasePersonId", "Изберете страна!");
            }
            string validation = service.EpepUserAssignment_Validate(model);
            if (!string.IsNullOrEmpty(validation))
            {
                ModelState.AddModelError("", validation);
            }
            if (!ModelState.IsValid)
            {
                SetViewBag_EpepUserAssignment(model);
                return View(nameof(EpepUserAssignment_Edit), model);
            }
            int currentId = model.Id;
            if (service.EpepUserAssignment_SaveData(model))
            {
                SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                await addToAuditAssignment((currentId == 0) ? AuditConstants.Operations.Append : AuditConstants.Operations.Update, model);
                return RedirectToAction(nameof(EpepUser_Edit), new { id = model.EpepUserId });
            }
            else
            {
                SetSuccessMessage(MessageConstant.Values.SaveFailed);
                SetViewBag_EpepUserAssignment(model);
                return View(nameof(EpepUserAssignment_Edit), model);
            }
        }
        void SetViewBag_EpepUserAssignment(EpepUserAssignment model)
        {
            var user = service.GetById<EpepUser>(model.EpepUserId);
            ViewBag.breadcrumbs = new List<BreadcrumbsVM>()
            {
                { new BreadcrumbsVM(){
                Title = "ЕПЕП - Потребители",
                Href = Url.Action("EpepUser", "Epep")}
                }
                ,
                {
                new BreadcrumbsVM(){
                Title = user.FullName,
                Href = Url.Action("EpepUser_Edit", "Epep", new { id = model.EpepUserId })
                }
             }
            };
            ViewBag.canChange = model.CourtId == userContext.CourtId;
            ViewBag.AssignmentRole_ddl = new List<SelectListItem>() {
            new SelectListItem("Страна по делото",""),
            new SelectListItem("Адвокат","1"),
            };
            SetHelpFile(HelpFileValues.Nom7);
        }
        public async Task<IActionResult> Get_CasePerson(int caseId)
        {
            var model = (await casePersonService.CasePersonFast_SelectForCasePreview(caseId)
                                .Select(x => new SelectListItem
                                {
                                    Value = x.Id.ToString(),
                                    Text = $"{x.FullName} ({x.RoleName})"
                                }).ToListAsync()).AddAllItem().ToList().SingleOrChoose();
            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> Assigment_ExpiredInfo(ExpiredInfoVM model)
        {

            var expiredModel = await service.GetReadonlyAsync<EpepUserAssignment>(model.Id);
            if (service.SaveExpireInfo<EpepUserAssignment>(model))
            {
                service.AppendEpepUserAssignment(expiredModel, EpepConstants.ServiceMethod.Delete);
                await addToAuditAssignment(AuditConstants.Operations.Delete, expiredModel);
                SetSuccessMessage("Достъпът до делото е премахнат успешно.");

                string caseInfo = await service.GetCaseInfoById(expiredModel.CaseId);
                string html = $"Премахнат достъп до дело {caseInfo}";
                SaveLogOperation("epep", "epepuser_edit", html, IO.LogOperation.Models.OperationTypes.Delete, expiredModel.EpepUserId);

                return Json(new { result = true, redirectUrl = Url.Action(nameof(EpepUser_Edit), new { id = expiredModel.EpepUserId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        [HttpPost]
        public IActionResult EpepUserAssignments_SelectByCase(IDataTablesRequest request, int caseId)
        {
            var data = service.EpepUserAssignments_SelectByCase(caseId);
            return request.GetResponse(data);
        }


        //public async Task<IActionResult> GetRegInfo(string id, string infoType)
        //{
        //    if (!userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator))
        //    {
        //        return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
        //    }

        //    epepClient = await connector.Connect();

        //    try
        //    {
        //        switch (infoType)
        //        {
        //            case "doc":
        //                return Json(epepClient.GetIncomingDocumentById(Guid.Parse(id)));
        //            case "case":
        //                return Json(epepClient.GetCaseById(Guid.Parse(id)));
        //            case "hearing":
        //                return Json(epepClient.GetHearingById(Guid.Parse(id)));
        //            case "person":
        //                return Json(epepClient.GetSideById(Guid.Parse(id)));
        //            case "act":
        //                return Json(epepClient.GetActById(Guid.Parse(id)));
        //            case "act_private":
        //                return Json(epepClient.GetPrivateActFileById(Guid.Parse(id)));
        //            case "act_public":
        //                return Json(epepClient.GetPublicActFileById(Guid.Parse(id)));
        //            case "userp":
        //                return Json(epepClient.GetPersonRegistrationById(Guid.Parse(id)));
        //            case "userl":
        //                return Json(epepClient.GetLawyerRegistrationById(Guid.Parse(id)));
        //        }

        //    }
        //    catch (FaultException fex)
        //    {
        //        return Content(fex.GetMessageFault());
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content(ex.Message);
        //    }
        //    finally
        //    {
        //        epepClient.Close();
        //    }
        //    return Content("Invalid operation");
        //}

        //public IActionResult RecoverRequest(int st, int si)
        //{
        //    bool result = false;
        //    switch (st)
        //    {
        //        case SourceTypeSelectVM.CaseNotification:
        //            var model = service.GetById<Infrastructure.Data.Models.Cases.CaseNotification>(si);
        //            var epepInfo = caseNotificationService.GetEpepSummonInfo(model);
        //            result = service.AppendCaseNotification(model, epepInfo, EpepConstants.ServiceMethod.Add);
        //            break;
        //        default:
        //            break;
        //    }

        //    return Json(result);
        //}



        public async Task<IActionResult> MqInfo(int integrationType, int sourceType, long sourceId, bool returnToMQ = false, bool restartCase = false)
        {
            if (returnToMQ)
            {
                await service.MQEpep_ResetError(integrationType, sourceType, sourceId);
            }
            if (integrationType == NomenclatureConstants.IntegrationTypes.EPEP && sourceType == SourceTypeSelectVM.Case)
            {
                var caseCourtId = service.GetPropById<IOWebApplication.Infrastructure.Data.Models.Cases.Case, int>(x => x.Id == sourceId, x => x.CourtId);
                if (caseCourtId == userContext.CourtId)
                {
                    ViewBag.restartCaseUrl = Url.Action(nameof(MqInfo), new { integrationType, sourceType, sourceId, restartCase = true });
                    if (restartCase)
                    {
                        var rest = service.MqRestartCase((int)sourceId);
                        ViewBag.restartCaseInfo = rest.Content;
                    }
                }
            }
            var model = await service.MQEpep_Select(integrationType, sourceType, sourceId);
            ViewBag.isGlobalAdmin = userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator);
            ViewBag.isSupervisor = userContext.IsUserInRole(AccountConstants.Roles.Supervisor);
            ViewBag.resetUrl = Url.Action(nameof(MqInfo), new { integrationType, sourceType, sourceId, returnToMQ = true });
            if (integrationType == NomenclatureConstants.IntegrationTypes.ISPN)
            {
                ViewBag.IspnRemark = service.NotMappedActs((int)sourceId);
            }
            ViewBag.integrationType = integrationType;
            return PartialView("_MqInfo", model);
        }


        public async Task<IActionResult> EpepShowResultCase(int inMigrationId, bool refreshData = false)
        {
            SummaryCaseInfoVM model = await service.LoadConnectedCase(inMigrationId, refreshData); ;
            if (model != null)
            {
                model.MigrationId = inMigrationId;
                if (refreshData)
                {
                    SetSuccessMessage("Данните са актуализирани успешно.");
                }
            }
            else
            {
                return NotFoundError("Търсеният от Вас акт не е намерен и/или нямате достъп до него.");
            }
            ViewBag.EPEP_FILE_PATH = EPEP_FILE_PATH;
            var _inMigrationCaseId = await service.GetPropByIdAsync<Infrastructure.Data.Models.Cases.CaseMigration, int>(x => x.Id == inMigrationId, x => x.CaseId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(_inMigrationCaseId);
            return View(model);
        }

        public async Task<IActionResult> EpepShowExecProcess(int actId = 0, int execListId = 0, bool refreshData = false)
        {
            if (actId == 0 && execListId == 0)
            {
                return NotFoundError("Търсената от Вас партида не е намерена.");
            }
            ExecProcessInfoVM model = await service.LoadExecProcess(actId, execListId, refreshData); ;
            if (model != null)
            {
                if (refreshData)
                {
                    SetSuccessMessage("Данните са актуализирани успешно.");
                }
            }
            else
            {
                if (actId > 0)
                {
                    bool hasRequest = await service.CheckForSavedDocumentRequestForCase(actId);
                    if (hasRequest)
                    {
                        SetErrorMessage("Моля, рестартирайте заявките за интеграция по делото!");
                        return RedirectToAction("Edit", "CaseSessionAct", new { id = actId });
                    }
                    else
                    {
                        SetErrorMessage("Моля, въведете данни за заявление по делото!");
                        return RedirectToAction("Edit", "CaseSessionAct", new { id = actId });
                    }
                }
                return NotFoundError("Търсената от Вас партида не е намерена.");
            }
            ViewBag.EPEP_FILE_PATH = EPEP_FILE_PATH;
            if (actId > 0)
            {
                ViewBag.breadcrumbs = await commonService.Breadcrumbs_GetForCaseSessionActAsync(actId);
            }
            if (execListId > 0)
            {
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForExecListEdit(execListId);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ExecProcess_NewAccess(Guid processGid, int actId = 0, int execListId = 0)
        {
            var result = await service.AppendExecProcessAccess(actId, execListId, processGid);
            return Json(result);
        }
        [HttpPost]
        public async Task<IActionResult> ExecProcess_DeleteAccess(Guid accessGid, int actId = 0, int execListId = 0)
        {
            var result = await service.DeleteExecProcessAccess(actId, execListId, accessGid);
            return Json(result);
        }

        public async Task<IActionResult> ExecProcess_AccessDocument(Guid accessGid, int actId = 0, int execListId = 0)
        {
            var model = await service.GetAccessDocument(actId, execListId, accessGid);
            string html = await this.RenderPartialViewAsync("~/Views/Epep/", "_EpepShowExecProcess_AccessDocument.cshtml", model, true);
            var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true).GetByte(this.ControllerContext);

            return File(pdfBytes, NomenclatureConstants.ContentTypes.Pdf, "accessKey.pdf");
        }

        public async Task<IActionResult> EpepUsers_SelectByCasePerson(int casePersonId, bool forSummon = true)
        {
            var model = await service.EpepUser_SelectByCasePerson(casePersonId, forSummon).ToListAsync();
            return PartialView("_EpepUsersCasePerson", model);
        }
    }
}