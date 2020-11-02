// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

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
        //public async Task<IActionResult> recoverdata()
        //{
        //    epepClient = await connector.Connect();
        //    return Content(service.RecoverData(epepClient));
        //}

        public IActionResult EpepUser()
        {
            return View();
        }
        [HttpPost]
        public IActionResult EpepUser_ListData(IDataTablesRequest request, int? userType)
        {
            var data = service.EpepUser_Select(userType, request.Search?.Value);
            return request.GetResponse(data);
        }

        public IActionResult EpepUser_Add()
        {
            var model = new EpepUser()
            {
                EpepUserTypeId = EpepConstants.UserTypes.Person
            };
            SetViewBag_EpepUser();
            return View(nameof(EpepUser_Edit), model);
        }
        public IActionResult EpepUser_Edit(int id)
        {
            var model = service.GetById<EpepUser>(id);
            SetViewBag_EpepUser();
            return View(nameof(EpepUser_Edit), model);
        }
        [HttpPost]
        public IActionResult EpepUser_Edit(EpepUser model)
        {
            var error = service.EpepUser_Validate(model);
            if (!string.IsNullOrEmpty(error))
            {
                ModelState.AddModelError("", error);
            }
            if (!ModelState.IsValid)
            {
                SetViewBag_EpepUser();
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
                SetViewBag_EpepUser();
                return View(nameof(EpepUser_Edit), model);
            }

        }

        private void SetViewBag_EpepUser()
        {
            ViewBag.EpepUserTypeId_ddl = nomService.GetDropDownList<EpepUserType>();
            ViewBag.breadcrumbs = new List<BreadcrumbsVM>()
            {
                { new BreadcrumbsVM(){
                Title = "ЕПЕП - Потребители",
                Href = Url.Action("EpepUser", "Epep")}
                }
            };
        }

        public IActionResult ReturnAllErrors()
        {
            if (!userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }
            var updatedCount = service.ReturnAllErrorsInMQ();
            return Content($"Върнати заявки в опашката {updatedCount} бр.");
        }

        public async  Task<IActionResult> GetRegInfo(string id, string infoType)
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

        public IActionResult MqInfo(int integrationType, int sourceType, long sourceId)
        {
            var model = service.MQEpep_Select(integrationType, sourceType, sourceId);
            ViewBag.isGlobalAdmin = userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator);
            return PartialView("_MqInfo", model);
        }


        /*
        public IActionResult ManageData()
        {
            if (!userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }

            var model = new EpepManageDataVM();
            SetViewBagManage();
            return View(model);
        }

        [HttpPost]
        public IActionResult ManageData(EpepManageDataVM model)
        {
            //if (!model.CheckCode())
            //{
            //    ModelState.AddModelError(nameof(EpepManageDataVM.SecurityCode), "Грешен код");
            //}

            epepClient = connector.Connect().Result;

            Guid _id = Guid.Empty;
            try
            {
                _id = Guid.Parse(model.ObjectId);
            }
            catch
            {
                ModelState.AddModelError(nameof(EpepManageDataVM.ObjectId), "Невалиден идентификатор");
            }
            var intKey = service.IntegrationKey_GetByOuterKey(NomenclatureConstants.IntegrationTypes.EPEP, model.ObjectId);
            if (intKey != null)
            {
                ModelState.AddModelError(nameof(EpepManageDataVM.ObjectId), "Идентификатора се използва. Невъзможно изтриването.");
            }

            if (!ModelState.IsValid)
            {
                model.SecurityCode = string.Empty;
                SetViewBagManage();
                return View(model);
            }
            bool submitOk = false;
            try
            {
                switch (model.DataType)
                {
                    case SourceTypeSelectVM.Case:
                        submitOk = epepClient.DeleteCase(_id);
                        break;
                    case SourceTypeSelectVM.CaseSelectionProtokol:
                        submitOk = epepClient.DeleteAssignment(_id);
                        break;
                    case SourceTypeSelectVM.CaseSession:
                        submitOk = epepClient.DeleteHearing(_id);
                        break;
                    case SourceTypeSelectVM.CaseSessionAct:
                        submitOk = epepClient.DeleteAct(_id);
                        break;
                    case SourceTypeSelectVM.CaseSessionActPreparator:
                        submitOk = epepClient.DeleteActPreparator(_id);
                        break;
                    case SourceTypeSelectVM.CasePerson:
                        submitOk = epepClient.DeleteSide(_id);
                        break;
                    default:
                        break;
                }

                if (submitOk)
                {
                    model.ResponseMessage = "Данните са премахнати успешно.";
                }
                else
                {
                    model.ResponseMessage = "Проблем при премахването на данни.";
                }
            }
            catch (Exception ex)
            {
                model.ResponseMessage = ex.Message;
            }
            SetViewBagManage();
            return View(model);
        }

        public IActionResult CorrectIK(int sourceType)
        {
            epepClient = connector.Connect().Result;

            var model = service.IntegrationKey_SelectToCorrect(sourceType);
            int totalCount = model.Count();
            int removedCount = 0;
            foreach (var itemToCorrect in model)
            {

                bool submitOk = false;
                try
                {
                    Guid _id = Guid.Parse(itemToCorrect.OuterCode);
                    switch (sourceType)
                    {
                        case SourceTypeSelectVM.Document:
                            if (itemToCorrect.DateTransferedDW.Value.Month == 1)
                            {
                                submitOk = epepClient.DeleteIncomingDocument(_id);
                            }
                            else
                            {
                                submitOk = epepClient.DeleteOutgoingDocument(_id);
                            }
                            //submitOk = epepClient.DeleteOutgoingDocument(_id);
                            break;
                        case SourceTypeSelectVM.CaseSession:
                            submitOk = epepClient.DeleteHearing(_id);
                            break;
                        case SourceTypeSelectVM.CaseSessionAct:
                            submitOk = epepClient.DeleteAct(_id);
                            break;
                        case SourceTypeSelectVM.CaseSessionActPreparator:
                            submitOk = epepClient.DeleteActPreparator(_id);
                            break;
                        case SourceTypeSelectVM.CasePerson:
                            submitOk = epepClient.DeleteSide(_id);
                            break;
                        case SourceTypeSelectVM.CaseLawUnit:
                            submitOk = epepClient.DeleteReporter(_id);
                            break;
                        default:
                            break;
                    }

                    if (submitOk)
                    {
                        service.IntegrationKey_Correct(itemToCorrect, false);
                        removedCount++;
                    }

                }
                catch (FaultException fex)
                {
                    try
                    {
                        var errorElement = XElement.Parse(fex.CreateMessageFault().GetReaderAtDetailContents().ReadOuterXml());
                        var errorDictionary = errorElement.Elements().ToDictionary(key => key.Name.LocalName, val => val.Value);
                        var errorDetails = string.Join(";", errorDictionary).ToLower();
                        if (errorDetails.Contains("съществува"))
                        {
                            service.IntegrationKey_Correct(itemToCorrect, false);
                            removedCount++;
                        }
                        else
                        {
                            service.IntegrationKey_Correct(itemToCorrect, true);
                        }

                    }
                    catch
                    {
                        service.IntegrationKey_Correct(itemToCorrect, true);
                    }
                }
                catch (Exception ex)
                {
                    service.IntegrationKey_Correct(itemToCorrect, true);
                }


            }

            if (epepClient != null)
            {
                epepClient.CloseAsync();
            }

            return Content($"Коригирани {removedCount} бр. от {totalCount} бр.");
        }

        private void SetViewBagManage()
        {
            int[] managebleDataTypes = {
                SourceTypeSelectVM.Case,
                SourceTypeSelectVM.CaseSelectionProtokol,
                SourceTypeSelectVM.CasePerson,
                SourceTypeSelectVM.CaseSession,
                SourceTypeSelectVM.CaseSessionAct,
                SourceTypeSelectVM.CaseSessionActPreparator
            };
            List<SelectListItem> dataTypes = new List<SelectListItem>();
            foreach (var item in managebleDataTypes)
            {
                dataTypes.Add(new SelectListItem(SourceTypeSelectVM.GetSourceTypeName(item), item.ToString()));
            }
            dataTypes.Prepend(new SelectListItem("Изберете", "-1"));
            ViewBag.DataType_ddl = dataTypes;
        }

        */

        //[HttpPost]
        //public async Task<IActionResult> Lawyers_ListData(IDataTablesRequest request)
        //{

        //    epepClient = connector.Connect().Result;
        //    if (epepClient != null)
        //    {
        //        var data = epepClient.GetAllLawyersAsync().Result
        //                            .Where(x => x.Name.Contains(request.Search?.Value ?? x.Name, StringComparison.InvariantCultureIgnoreCase))
        //                            .OrderBy(x => x.Name)
        //                            .AsQueryable();

        //        await epepClient.CloseAsync();
        //        return request.GetResponse(data);
        //    }
        //    return null;
        //}

        //public async Task<IActionResult> ImportLawyers()
        //{
        //    epepClient = connector.Connect().Result;
        //    if (epepClient != null)
        //    {
        //        var dataLawyers = epepClient.GetAllLawyersAsync().Result;

        //        foreach (var lawyer in dataLawyers)
        //        {
        //            var model = new LawUnit()
        //            {
        //                LawUnitTypeId = NomenclatureConstants.LawUnitTypes.Lawyer,
        //                FirstName = lawyer.Name,
        //                FullName = lawyer.Name,
        //                Code = lawyer.Number,
        //                Department = lawyer.College,
        //                UicTypeId = NomenclatureConstants.UicTypes.LNCh,
        //                LatinName = lawyer.LawyerId.ToString()
        //            };
        //            model.DateFrom = new DateTime(2020, 04, 16);
        //            model.UserId = userContext.UserId;
        //            db.LawUnits.Add(model);
        //        }
        //        db.SaveChanges();
        //        await epepClient.CloseAsync();

        //    }
        //    return null;
        //}

        //public async Task<IActionResult> PersonReg()
        //{
        //    var epep = new PersonRegistration()
        //    {
        //        EGN = "7706010002",
        //        BirthDate = new DateTime(1977, 6, 1),
        //        Name = "Константин Борисов",
        //        Email = "cborisoff@mail.bg",
        //        Address = "София",
        //        Description = "Тестов епеп потребител 1"
        //    };

        //    epepClient = connector.Connect().Result;
        //    var result = epepClient.InsertPersonRegistration(epep);
        //    await epepClient.CloseAsync();
        //    return Content(result.ToString());
        //}


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
                return RedirectToAction(nameof(EpepUserAssignment_Edit), new { id = model.Id });
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
        }
        public IActionResult Get_CasePerson(int caseId)
        {
            var model = casePersonService.CasePerson_Select(caseId, 0)
                                .Select(x => new SelectListItem
                                {
                                    Value = x.Id.ToString(),
                                    Text = $"{x.FullName} ({x.RoleName})"
                                }).ToList();
            return Json(model);
        }

    }
}