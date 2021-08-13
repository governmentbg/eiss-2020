using DataTables.AspNet.Core;
using Integration.Epep;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Epep;
using IOWebApplicationService.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{

    public class EpepController : BaseController
    {
        private readonly IEpepConnectionService connector;
        private IeCaseServiceClient epepClient;
        private readonly IMQEpepService service;
        private readonly INomenclatureService nomService;
        private readonly ICasePersonService casePersonService;

        public EpepController(
            IEpepConnectionService _connector,
            IMQEpepService _service,
            INomenclatureService _nomService,
            ICasePersonService _casePersonService
            )
        {
            connector = _connector;
            service = _service;
            nomService = _nomService;
            casePersonService = _casePersonService;
        }

        //[AllowAnonymous]
        //public async Task<IActionResult> GetUserRegistration(string id)
        //{
        //    epepClient = await connector.Connect();
        //    return Json(await epepClient.GetUserRegistrationInfoByUsernameAsync(id));
        //}

        //[AllowAnonymous]
        //public async Task<IActionResult> GetPersonRegistrationById(string id)
        //{
        //    epepClient = await connector.Connect();
        //    return Json(await epepClient.GetPersonRegistrationByIdAsync(Guid.Parse(id)));
        //}

        //[AllowAnonymous]
        //public async Task<IActionResult> getcaseinfo(string id)
        //{
        //    epepClient = await connector.Connect();
        //    try
        //    {
        //        return Json(epepClient.GetSideIdentifiersByCaseId(Guid.Parse(id)));
        //    }catch(Exception ex)
        //    {
        //        return null;
        //    }
        //}

        //[AllowAnonymous]
        //public async Task<IActionResult> recoverdata()
        //{
        //    // epepClient = await connector.Connect();
        //    return Content(service.RecoverData(null));
        //}

        public IActionResult SendActPreparators(int id)
        {
            service.SendActPreparators(id);
            return Content("Заявката за актуализиране на участници по акта е добавена успешно.");
        }

        public IActionResult EpepUser()
        {
            SetHelpFile(HelpFileValues.Nom7);
            ViewBag.EpepUserTypeId_ddl = nomService.GetDropDownList<EpepUserType>();
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
            return View(nameof(EpepUser_Edit), model);
        }
        public async Task<IActionResult> EpepUser_AddFromEmail(string email)
        {
            var model = await EpepUser_LoadDataByEmail(email);
            SetViewBag_EpepUser(model);
            return View(nameof(EpepUser_Edit), model);
        }
        public IActionResult EpepUser_Edit(int id)
        {
            var model = service.GetById<EpepUser>(id);
            SetViewBag_EpepUser(model);
            return View(nameof(EpepUser_Edit), model);
        }
        [HttpPost]
        public async Task<IActionResult> EpepUser_Edit(EpepUser model)
        {
            var error = service.EpepUser_Validate(model);
            if (!string.IsNullOrEmpty(error))
            {
                ModelState.AddModelError("", error);
            }
            if (model.Id == 0)
            {
                await Epep_Validation(model);
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
        private async Task Epep_Validation(EpepUser model)
        {
            try
            {
                epepClient = await connector.Connect();
                var regInfo = await epepClient.GetUserRegistrationInfoByUsernameAsync(model.Email.ToLower());
                if (regInfo.IsRegistered)
                {
                    if (!regInfo.LawyerRegistrationId.IsEmpty())
                    {
                        if (model.EpepUserTypeId == EpepConstants.UserTypes.Lawyer)
                        {
                            return;
                        }
                        ModelState.AddModelError("", "Съществува създаден потребител на адвокат в ЕПЕП с тази електронна поща.");
                    }
                    if (!regInfo.PersonRegistrationId.IsEmpty())
                    {
                        if (model.EpepUserTypeId == EpepConstants.UserTypes.Person)
                        {
                            return;
                        }
                        ModelState.AddModelError("", "Съществува създаден потребител на частно лице в ЕПЕП с тази електронна поща.");
                    }
                    ViewBag.hasEpepUserOtherType = true;
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (epepClient != null && epepClient.State == CommunicationState.Opened)
                {
                    await epepClient.CloseAsync();
                }
            }
        }

        private async Task<EpepUser> EpepUser_LoadDataByEmail(string email)
        {
            EpepUser model = new EpepUser();
            try
            {
                epepClient = await connector.Connect();
                var regInfo = await epepClient.GetUserRegistrationInfoByUsernameAsync(email.ToLower());
                if (regInfo.IsRegistered)
                {
                    if (!regInfo.LawyerRegistrationId.IsEmpty())
                    {
                        var epepLawyer = await epepClient.GetLawyerRegistrationByIdAsync(regInfo.LawyerRegistrationId.Value);
                        if (epepLawyer != null)
                        {
                            model.EpepUserTypeId = EpepConstants.UserTypes.Lawyer;
                            model.BirthDate = epepLawyer.BirthDate;
                            model.Email = epepLawyer.Email;
                            var lawyers = await epepClient.GetAllLawyersAsync();
                            var regLawyer = lawyers.Where(x => x.LawyerId == epepLawyer.LawyerId).FirstOrDefault();
                            if (regLawyer != null)
                            {
                                model.LawyerNumber = regLawyer.Number;

                                var lawunit = service.GetLawyerByNumber(model.LawyerNumber);
                                if (lawunit != null)
                                {
                                    model.LawyerLawUnitId = lawunit.Id;
                                    model.FullName = lawunit.FullName;
                                }
                                else
                                {
                                    ModelState.AddModelError(nameof(model.LawyerLawUnitId), $"Ненамерен адвокат {regLawyer.Name} с номер {regLawyer.Number}.");
                                }
                            }
                        }
                    }
                    if (!regInfo.PersonRegistrationId.IsEmpty())
                    {
                        var epepPerson = await epepClient.GetPersonRegistrationByIdAsync(regInfo.PersonRegistrationId.Value);
                        if (epepPerson != null)
                        {
                            model.EpepUserTypeId = EpepConstants.UserTypes.Person;
                            model.Uic = epepPerson.EGN;
                            model.FullName = epepPerson.Name;
                            model.BirthDate = epepPerson.BirthDate;
                            model.Address = epepPerson.Address;
                        }
                    }
                }

            }
            catch (Exception ex) { }
            finally
            {
                if (epepClient != null && epepClient.State == CommunicationState.Opened)
                {
                    await epepClient.CloseAsync();
                }
            }
            return model;
        }

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

        public IActionResult EpepUserAssignment_Add(int epepUserId)
        {
            var model = new EpepUserAssignment()
            {
                EpepUserId = epepUserId,
                CourtId = userContext.CourtId,
                DateFrom = DateTime.Now
            };
            SetViewBag_EpepUserAssignment(model);
            return View(nameof(EpepUserAssignment_Edit), model);
        }
        public IActionResult EpepUserAssignment_Edit(int id)
        {
            var model = service.GetById<EpepUserAssignment>(id);

            SetViewBag_EpepUserAssignment(model);
            return View(nameof(EpepUserAssignment_Edit), model);
        }
        [HttpPost]
        public IActionResult EpepUserAssignment_Edit(EpepUserAssignment model)
        {
            model.CourtId = userContext.CourtId;
            var _case = service.GetById<IOWebApplication.Infrastructure.Data.Models.Cases.Case>(model.CaseId);
            if (_case.CourtId != userContext.CourtId)
            {
                ModelState.AddModelError("CaseId", "Нямате достъп до избраното дело.");
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
            SetHelpFile(HelpFileValues.Nom7);
        }
        public IActionResult Get_CasePerson(int caseId)
        {
            var model = casePersonService.CasePerson_Select(caseId, 0, false, false, false)
                                .Select(x => new SelectListItem
                                {
                                    Value = x.Id.ToString(),
                                    Text = $"{x.FullName} ({x.RoleName})"
                                }).ToList();
            return Json(model);
        }

        [HttpPost]
        public IActionResult Assigment_ExpiredInfo(ExpiredInfoVM model)
        {

            var expiredModel = service.GetById<EpepUserAssignment>(model.Id);
            var caseModel = service.GetById<IOWebApplication.Infrastructure.Data.Models.Cases.Case>(expiredModel.CaseId);
            if (service.SaveExpireInfo<EpepUserAssignment>(model))
            {
                service.AppendEpepUserAssignment(expiredModel, EpepConstants.ServiceMethod.Delete);
                SetAuditContextDelete(service, SourceTypeSelectVM.EpepUserAssignment, model.Id);
                SetSuccessMessage("Достъпът до делото е премахнат успешно.");

                string html = $"Премахнат достъп до дело {caseModel.RegNumber}";
                SaveLogOperation("epep", "epepuser_edit", html, IO.LogOperation.Models.OperationTypes.Delete, expiredModel.EpepUserId);

                return Json(new { result = true, redirectUrl = Url.Action(nameof(EpepUser_Edit), new { id = expiredModel.EpepUserId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }



        public async Task<IActionResult> GetRegInfo(string id, string infoType)
        {
            if (!userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }

            epepClient = await connector.Connect();

            try
            {
                switch (infoType)
                {
                    case "doc":
                        return Json(epepClient.GetIncomingDocumentById(Guid.Parse(id)));
                    case "case":
                        return Json(epepClient.GetCaseById(Guid.Parse(id)));
                    case "person":
                        return Json(epepClient.GetSideById(Guid.Parse(id)));
                    case "act":
                        return Json(epepClient.GetActById(Guid.Parse(id)));
                    case "act_private":
                        return Json(epepClient.GetPrivateActFileById(Guid.Parse(id)));
                    case "act_public":
                        return Json(epepClient.GetPublicActFileById(Guid.Parse(id)));
                    case "userp":
                        return Json(epepClient.GetPersonRegistrationById(Guid.Parse(id)));
                    case "userl":
                        return Json(epepClient.GetLawyerRegistrationById(Guid.Parse(id)));
                }

            }
            catch (FaultException fex)
            {
                return Content(fex.GetMessageFault());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
            finally
            {
                await epepClient.CloseAsync();
            }
            return Content("Invalid operation");
        }

        public IActionResult RecoverRequest(int st, int si)
        {
            bool result = false;
            switch (st)
            {
                case SourceTypeSelectVM.CaseNotification:
                    var model = service.GetById<Infrastructure.Data.Models.Cases.CaseNotification>(si);
                    result = service.AppendCaseNotification(model, EpepConstants.ServiceMethod.Add);
                    break;
                default:
                    break;
            }

            return Json(result);
        }

        public async Task<IActionResult> RemoveObject(string id, string infoType)
        {
            if (!userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }

            epepClient = await connector.Connect();

            try
            {
                switch (infoType)
                {
                    case "doc":
                        return Json(epepClient.GetIncomingDocumentById(Guid.Parse(id)));
                    case "case":
                        return Json(epepClient.GetCaseById(Guid.Parse(id)));
                    case "person":
                        return Json(epepClient.GetSideById(Guid.Parse(id)));
                    case "act":
                        return Json(epepClient.GetActById(Guid.Parse(id)));
                    case "act_private":
                        return Json(epepClient.DeletePrivateActFile(Guid.Parse(id)));
                    case "act_public":
                        return Json(epepClient.DeletePublicActFile(Guid.Parse(id)));
                    case "userp":
                        return Json(epepClient.GetPersonRegistrationById(Guid.Parse(id)));
                    case "userl":
                        return Json(epepClient.GetLawyerRegistrationById(Guid.Parse(id)));
                }

            }
            catch (FaultException fex)
            {
                return Content(fex.GetMessageFault());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
            finally
            {
                await epepClient.CloseAsync();
            }
            return Content("Invalid operation");
        }

        public IActionResult MqInfo(int integrationType, int sourceType, long sourceId, bool returnToMQ = false)
        {
            if (returnToMQ)
            {
                service.MQEpep_ResetError(integrationType, sourceType, sourceId);
            }
            var model = service.MQEpep_Select(integrationType, sourceType, sourceId);
            ViewBag.isGlobalAdmin = userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator);
            ViewBag.isSupervisor = userContext.IsUserInRole(AccountConstants.Roles.Supervisor);
            ViewBag.resetUrl = Url.Action(nameof(MqInfo), new { integrationType, sourceType, sourceId, returnToMQ = true });
            return PartialView("_MqInfo", model);
        }



    }
}