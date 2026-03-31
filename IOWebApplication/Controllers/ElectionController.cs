// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Election;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Election;
using Microsoft.AspNetCore.Mvc;
using Rotativa.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class ElectionController : BaseController
    {
        private readonly IElectionService service;
        private readonly ICdnService cdnService;
        public ElectionController(IElectionService _service, ICdnService _cdnService)
        {
            service = _service;
            cdnService = _cdnService;
        }
        public IActionResult PersonAdd(int groupId)
        {
            var model = new ElectionPersonAddVM()
            {
                ElectionGroupId = groupId
            };

            var valResult = service.ValidatePersonAdd(model);
            if (!valResult.Result)
            {
                SetErrorMessage(valResult.ErrorMessage);
                return RedirectToAction("ElectionPersons", "Election", new { id = groupId });
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult PersonAdd(ElectionPersonAddVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (service.CheckIfLawunitIsAddedPersonAdd(model))
            {
                SetSuccessMessage("Съдия вече е добавен в сесията");
                return View(model);
            }
            var result = service.PersonAdd(model);
            if (result.Result)
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(PersonView), new { id = result.ObjectId });
            }
            else
            {
                SetSuccessMessage(MessageConstant.Values.SaveFailed);
                return View(model);
            }
        }

        public IActionResult PersonView(int id)
        {
            var model = service.PersonGetById(id);
            return View(model);
        }

        public IActionResult PersonChange(int id)
        {
            var person = service.GetById<ElectionPerson>(id);
            ViewBag.ElectionPersonStateId_ddl = service.GetDDL_PersonStatesForPerson(id);
            ViewBag.ElectionPersonDismissalTypeId_ddl = service.GetDDL_DismissalTypeForPerson(id);
            var model = new ElectionPersonChangeVM()
            {
                Id = person.Id,
                ElectionPersonStateId = person.ElectionPersonStateId,
                DescriptionState = person.DescriptionState,
                ElectionPersonDismissalTypeId = person.ElectionPersonDismissalTypeId,
                DescriptionRemoved = person.DescriptionRemoved
            };
            return PartialView("_PersonChange", model);
        }

        [HttpPost]
        public IActionResult PersonChange(ElectionPersonChangeVM model)
        {
            var result = service.PersonChange(model);
            return Json(result);
        }


        [HttpGet]
        public IActionResult SearchLawUnit(string query)
        {
            return Json(service.SearchLawunitsForElection(query));
        }
        public IActionResult Index()
        {
            if (!userContext.IsUserInFeature(AccountConstants.Features.Modules.VKSAdministration))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);

            }


            return View();
        }
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request)
        {
            var data = service.ElectionGroup_Select();

            var ls = data.ToList();
            return request.GetResponse(data);
        }
        [HttpPost]
        public IActionResult ListElectionPerson(IDataTablesRequest request, int electionGroupId)
        {
            var data = service.Persons_Select(electionGroupId);

            var ls = data.ToList();
            return request.GetResponse(data);
        }
        [HttpPost]
        public IActionResult ListElectionPersonFinished(IDataTablesRequest request, int protocolId)
        {
            var data = service.Persons_SelectByProtocol(protocolId);

            var ls = data.ToList();
            return request.GetResponse(data);
        }

        public IActionResult ElectionPersons(int id)
        {
            if (!userContext.IsUserInFeature(AccountConstants.Features.Modules.VKSAdministration))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);

            }
            int notFiledProtocolId = service.GetNotFilledProtocol_ID(id);
            if (notFiledProtocolId > 0)
            {
                return RedirectToAction("ElectionPersonsFinal", "Election", new { id = notFiledProtocolId });
            }


            var electionGroup = service.GetElectionGroupByID(id);
            ViewBag.Title = electionGroup.Label;


            return View(electionGroup);
        }
        public IActionResult ElectionPersonsFinal(int id)
        {
            if (!userContext.IsUserInFeature(AccountConstants.Features.Modules.VKSAdministration))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);

            }
            var electionProtocol = service.Get_ElectionProtokol(id);
            ViewBag.ElectionProtocolID = id;
            ViewBag.HasUnsignedProtocol = service.GetNotSignedProtocol_ID(electionProtocol.ElectionGroupId);
            var electionGroup = service.GetElectionGroupByID(electionProtocol.ElectionGroupId);
            ViewBag.Title = electionGroup.Label;


            return View(electionGroup);
        }

        public IActionResult Edit(int id)
        {
            if (!userContext.IsUserInFeature(AccountConstants.Features.Modules.VKSAdministration))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);

            }
            ViewBag.ElectionTypeId_ddl = service.GetDDL_ElectionType();

            ElectionGroupVM model = service.ElectionGroup_SelectById(id);

            return View(model);
        }
        public IActionResult Add()
        {
            if (!userContext.IsUserInFeature(AccountConstants.Features.Modules.VKSAdministration))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);

            }
            ViewBag.ElectionTypeId_ddl = service.GetDDL_ElectionType();

            ElectionGroupVM model = new ElectionGroupVM();

            model.ElectionTypeId = -1;


            return View(nameof(Edit), model);

        }

        [HttpPost]
        public IActionResult Add(ElectionGroupVM model)
        {
            ViewBag.ElectionTypeId_ddl = service.GetDDL_ElectionType();


            if (model.DocumentId == 0L)
            {
                SetErrorMessage("Изберете номер на документ");
                return View(nameof(Edit), model);
            }
            else
            {

                var cur_id = service.Election_Save(model);
                if (cur_id > 0)
                {
                    this.SaveLogOperation(cur_id == 0, model.Id);
                    SetSuccessMessage(MessageConstant.Values.SaveOK);
                    return RedirectToAction(nameof(Edit), new { id = cur_id });
                }
                else
                {
                    SetErrorMessage(MessageConstant.Values.SaveFailed);
                    return View(nameof(Edit), model);
                }
            }

            return View(nameof(Edit), model);


        }
        [HttpPost]
        public IActionResult Edit(ElectionGroupVM model)
        {
            ViewBag.ElectionTypeId_ddl = service.GetDDL_ElectionType();


            if (model.DocumentId == 0)
            {
                SetErrorMessage("Изберете номер на документ");
                return View(nameof(Edit), model);
            }
            else
            {

                var cur_id = service.Election_Save(model);
                if (cur_id > 0)
                {
                    this.SaveLogOperation(cur_id == 0, model.Id);
                    SetSuccessMessage(MessageConstant.Values.SaveOK);
                    return RedirectToAction(nameof(Edit), new { id = cur_id });
                }
                else
                {
                    SetErrorMessage(MessageConstant.Values.SaveFailed);
                    return View(nameof(Edit), model);
                }
            }

        }
        public IActionResult Preview(int id)
        {
            var model = service.ElectionProtokol_Preview(id);

            return View(model);
        }
        public IActionResult PreviewDoc(int id)
        {
            var model = service.ElectionProtokol_Preview(id);

            return View(model);
        }
        [HttpPost]
        public IActionResult ListDataProtocol(IDataTablesRequest request, int electionGroupId)
        {

            var data = service.ElectionProtocolList(electionGroupId);


            return request.GetResponse(data);
        }
        public IActionResult ProtocolList(int id)
        {
            if (!userContext.IsUserInFeature(AccountConstants.Features.Modules.VKSAdministration))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);

            }
            // var protocol = service.GetElectionProtocolByID(id);
            ViewBag.ElectionGroupId = id;
            ViewBag.Title = service.ElectionGroup_SelectById(id).Label;
            ViewBag.NotFilledProtocol_ID = service.GetNotFilledProtocol_ID(id);



            return View();
        }
        public async Task<IActionResult> CreateProtocol(int id)
        {

            bool sucess = service.ElectionProtocolSetSelectedLawUnit(id);

            var protokolModel = service.ElectionProtokol_Preview(id);
            string html = await this.RenderPartialViewAsync("~/Views/Election/", "Preview.cshtml", protokolModel, true);
            var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true).GetByte(this.ControllerContext);
            var protocol = service.Get_ElectionProtokol(id);

            protocol.DateElection = DateTime.Now;

            var save = service.Save_ElectionProtokol(protocol);

            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.ElectionProtocol,
                SourceId = id.ToString(),

                //FileName = "Протокол" +protocol.DateGenerated.Day.ToString()+"_"+ protocol.DateGenerated.Month.ToString() + "_" + protocol.DateGenerated.Year.ToString() + ".pdf",
                FileName = "Протокол" + protocol.DateElection.Value.ToString("_dd_MM_yyyy") + ".pdf",

                ContentType = "application/pdf",
                Title = $"Протокол",
                FileContentBase64 = Convert.ToBase64String(pdfBytes)
            };
            if (await cdnService.MongoCdn_AppendUpdate(pdfRequest))
            {
                // return RedirectToAction(nameof(SendForSign), new { selectionId = id, protocilId=protocol.Id});
                return RedirectToAction("PreviewDoc", new { id = protocol.Id });
            }
            else
            {
                SetErrorMessage("Проблем при създаване на протокол!");
                return RedirectToAction("ProtocolList", new { id = protocol.ElectionGroupId });
            }
        }

        public IActionResult SendForSign(int id)
        {

            var protocol = service.Get_ElectionProtokol(id);
            ///////////
            Uri urlSuccess = new Uri(Url.Action("ProtocolSignUpdate", "Election", new { id = id }), UriKind.Relative);
            Uri url = new Uri(Url.Action("ProtocolList", "Election", new { id = protocol.ElectionGroupId }), UriKind.Relative);

            var signModel = new Core.Models.SignPdfInfo()
            {
                SourceId = id.ToString(),
                SourceType = SourceTypeSelectVM.ElectionProtocol,
                DestinationType = SourceTypeSelectVM.ElectionProtocol,
                Location = "Sofia",
                Reason = "Test",
                SuccessUrl = urlSuccess,
                CancelUrl = url,
                ErrorUrl = url
            };


            signModel.SignerName = userContext.FullName;
            var electionPerson = service.GetElectionPersonBySelectedElectionPersonId(protocol.SelectedElectionPersonId.Value);
            service.WriteElectionLog(protocol.ElectionGroupId, electionPerson.PersonGid, null, NomenclatureConstants.ElectionLogOperation.ElectionProtocolSign, string.Format("Избран съдия: {0};", protocol.SelectedLawunitFullName), electionPerson.Id);

            return View("_SignPdf", signModel);
        }

        public IActionResult ProtocolSignUpdate(int id)
        {
            var protocol = service.GetElectionProtocolByID(id);
            protocol.DateSigned = DateTime.Now;

            var updated = service.Save_ElectionProtokol(protocol);
            if (!updated)
            {
                SetErrorMessage("Проблем при запис");
            }
            return RedirectToAction("ProtocolList", "Election", new { id = protocol.ElectionGroupId });
        }
        public IActionResult Finalize(int id)
        {
            int protocolId = service.PersonFinish_Create(id);
            return RedirectToAction("ElectionPersonsFinal", "Election", new { id = protocolId });
        }

        public IActionResult HasDeclaration(int id)
        {
            var saveResult = service.PersonSaveDeclaration(id);
            if (saveResult.Result)
            {
                SetSuccessMessage("Декларацията на лицето е успешно добавена.");
            }
            else
            {
                SetErrorMessage(saveResult.ErrorMessage);
            }
            return RedirectToAction("PersonView", new { id });
        }

        [HttpPost]
        public IActionResult GetLogData(IDataTablesRequest request, int? electionGroupId, Guid? personGid, int? electionProtocolId)
        {
            var data = service.SelectLog(electionGroupId, personGid, electionProtocolId);

            return request.GetResponse(data);
        }

        public IActionResult MakeMultipleSelection(int id)
        {
            for (int i = 0; i < 1000; i++)
            {
                int protocolId = service.PersonFinish_Create(id);

                bool sucess = service.ElectionProtocolSetSelectedLawUnit(protocolId);

                var protokolModel = service.ElectionProtokol_Preview(protocolId);

                var protocol = service.Get_ElectionProtokol(protocolId);

                protocol.DateElection = DateTime.Now;

                var save = service.Save_ElectionProtokol(protocol);


            }


            return RedirectToAction("ProtocolList", "Election", new { id = id });
        }
        public IActionResult SessionHistory(int id)
        {
            var model = service.ElectionGroup_SelectById(id);

            return View(model);
        }

    }
}
