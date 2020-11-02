// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
//using IO.RegixClient;
using IOWebApplication.Infrastructure.Models.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace IOWebApplication.Controllers
{
    public class DocumentController : BaseController
    {
        private readonly IDocumentService docService;
        private readonly IDocumentTemplateService templateService;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly IWorkTaskService taskService;
        private readonly IEisppService eisppService;
        private readonly ICdnService cdnService;
        private readonly IRegixReportService regixService;
        public DocumentController(
            IDocumentService _docService,
            IDocumentTemplateService _templateService,
            INomenclatureService _nomService,
            ICommonService _commonService,
            IWorkTaskService _taskService,
            IEisppService _eisppService,
            ICdnService _cdnService,
            IRegixReportService _regixService
        )
        {
            docService = _docService;
            templateService = _templateService;
            nomService = _nomService;
            commonService = _commonService;
            taskService = _taskService;
            eisppService = _eisppService;
            cdnService = _cdnService;
            regixService = _regixService;
        }


        /// <summary>
        /// Справка за регистрирани документи- Съдебна регистратура
        /// </summary>
        /// <param name="CourtOrganizationId">Деловодна регистратура</param>
        /// <param name="DocumentDirectionId">Направление</param>
        /// <param name="DocumentKindId">Вид документ</param>
        /// <param name="DocumentGroupId">Основен вид</param>
        /// <param name="DocumentTypeId">Точен вид</param>
        /// <param name="DocumentNumber">Документи номер</param>
        /// <param name="DocumentYear">Година на регистриране</param>
        /// <param name="DateFrom">Регистриран от дата</param>
        /// <param name="DateTo">Регистриран до дата</param>
        /// <returns></returns>
        public IActionResult Index(
                                     int? CourtOrganizationId,
                                     int? DocumentDirectionId,
                                     int? DocumentKindId,
                                     int? DocumentGroupId,
                                     int? DocumentTypeId,
                                     string DocumentNumber,
                                     int? DocumentYear,
                                     DateTime? DateFrom,
                                     DateTime? DateTo
                                    )
        {
            if (!CheckAccess(docService, SourceTypeSelectVM.Document, null, AuditConstants.Operations.View))
            {
                return Redirect_Denied();
            }

            DocumentFilterVM model = new DocumentFilterVM()
            {
                CourtOrganizationId = CourtOrganizationId,
                DocumentDirectionId = DocumentDirectionId,
                DocumentKindId = DocumentKindId,
                DocumentGroupId = DocumentGroupId,
                DocumentTypeId = DocumentTypeId,
                DocumentNumber = DocumentNumber,
                DocumentYear = DateTime.Now.Year,
                DateFrom = DateFrom,
                DateTo = DateTo
            };

            ViewBag.CourtOrganizationId_ddl = docService.GetDocumentRegistratures(true);
            ViewBag.DocumentDirectionId_ddl = nomService.GetDropDownList<DocumentDirection>();
            ViewBag.PersonRoleId_ddl = nomService.GetDropDownList<PersonRole>(orderByNumber: false);

            ViewBag.HasRegNumberOtherSystem = false;
            if (userContext.CourtInstances.Contains(NomenclatureConstants.CaseInstanceType.SecondInstance) ||
                 userContext.CourtInstances.Contains(NomenclatureConstants.CaseInstanceType.ThirdInstance))
            {
                ViewBag.HasRegNumberOtherSystem = true;
            }
            SetHelpFile(HelpFileValues.CourtRegistry);

            return View(model);
        }

        /// <summary>
        /// Извличане на данни за справка Съдебна регистратура
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, DocumentFilterVM model)
        {

            var data = docService.Document_Select(model);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Регистриране на нов документ
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public IActionResult Add(int direction)
        {
            if (!CheckAccess(docService, SourceTypeSelectVM.Document, null, AuditConstants.Operations.Append))
            {
                return Redirect_Denied();
            }
            var model = docService.Document_Init(direction);
            SetViewBag(model);

            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Регистриране на нов документ по подадена бланка на документ
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public IActionResult AddFromTemplate(int templateId)
        {
            if (!CheckAccess(docService, SourceTypeSelectVM.Document, null, AuditConstants.Operations.Append))
            {
                return Redirect_Denied();
            }
            var template = templateService.GetById<DocumentTemplate>(templateId);
            if (template == null)
            {
                return RedirectToAction(nameof(Add), new { direction = DocumentConstants.DocumentDirection.OutGoing });
            }
            var model = docService.Document_Init(DocumentConstants.DocumentDirection.OutGoing, templateId);

            SetViewBag(model);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редактиране на документ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(long id)
        {
            if (!CheckAccess(docService, SourceTypeSelectVM.Document, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = docService.Document_GetById(id);
            if (model == null)
            {
                throw new NotFoundException("Търсения от Вас документ не е намерен и/или нямате достъп до него.");
            }
            if (model.CaseId > 0 && !string.IsNullOrEmpty(model.CaseRegisterNumber))
            {
                //Ако по намерения документ вече има регистрирано дело потребителя се пренасочва към преглед
                return RedirectToAction(nameof(View), new { id = id });
            }
            SetViewBag(model);
            return View(nameof(Edit), model);
        }

        public IActionResult Correction(long id)
        {
            if (!CheckAccess(docService, SourceTypeSelectVM.Document, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = docService.Document_GetById(id);
            if (model == null)
            {
                throw new NotFoundException("Търсения от Вас документ не е намерен и/или нямате достъп до него.");
            }

            SetViewBag(model);
            return View(model);
        }

        /// <summary>
        /// Преглед на документ
        /// </summary>
        /// <param name="id"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public IActionResult View(long id, long? taskId = null)
        {
            if (!CheckAccess(docService, SourceTypeSelectVM.Document, id, AuditConstants.Operations.View))
            {
                return Redirect_Denied();
            }
            var model = docService.Document_GetById(id);
            if (model == null)
            {
                throw new NotFoundException("Търсения от Вас документ не е намерен и/или нямате достъп до него.");
            }
            if (taskId > 0)
            {
                var task = taskService.Select_ById(taskId.Value);
                if (task != null && task.TaskStateId != WorkTaskConstants.States.Completed)
                {
                    switch (task.TaskTypeId)
                    {
                        case WorkTaskConstants.Types.Document_Sign:
                            taskService.CompleteTask(taskId.Value);
                            break;
                    }
                }
            }
            SetViewBag(model);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на документ/регистриране на нов документ
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [RequestFormLimits(ValueCountLimit = 4096)]
        public IActionResult Edit(DocumentVM model)
        {
            ValidateModel(model);
            if (!ModelState.IsValid)
            {
                SetViewBag(model);
                return View(nameof(Edit), model);
            }
            ModelState.Clear();
            long currentId = model.Id;
            if (docService.Document_SaveData(model))
            {
                SetAuditContext(docService, SourceTypeSelectVM.Document, model.Id, currentId == 0);

                if (currentId == 0 && model.TemplateId > 0)
                {
                    return RedirectToAction("GenerateDocumentFile", "DocumentTemplate", new { id = model.TemplateId, documentId = model.Id });
                }
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                this.SaveLogOperation(currentId == 0, model.Id, null, "edit");
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            SetViewBag(model);
            return View(nameof(Edit), model);
        }

        [HttpPost]
        [RequestFormLimits(ValueCountLimit = 4096)]
        public IActionResult Correction(DocumentVM model)
        {
            ModelState.Clear();

            long currentId = model.Id;
            if (docService.Document_CorrectData(model))
            {
                SetAuditContext(docService, SourceTypeSelectVM.Document, model.Id, false);
                SetSuccessMessage("Коригираните данни на документа са актуализирани успешно.");
                this.SaveLogOperation(false, model.Id, null, "edit");
                return RedirectToAction(nameof(Correction), new { id = model.Id });
            }

            SetViewBag(model);
            return View(nameof(Correction), model);
        }

        /// <summary>
        /// Валидиране на подадените данни за документ
        /// </summary>
        /// <param name="model"></param>
        void ValidateModel(DocumentVM model)
        {
            if (model.IsOldNumber && model.Id == 0)
            {
                if (string.IsNullOrEmpty(model.OldDocumentNumber))
                {
                    ModelState.AddModelError(nameof(DocumentVM.OldDocumentNumber), "Въведете 'Стар номер'.");
                }
                int numVal = 0;
                if (!int.TryParse(model.OldDocumentNumber, out numVal))
                {
                    ModelState.AddModelError(nameof(DocumentVM.OldDocumentNumber), "Невалиден стар номер.");
                }
                if (!model.OldDocumentDate.HasValue)
                {
                    ModelState.AddModelError(nameof(DocumentVM.OldDocumentDate), "Въведете стара дата на документ.");
                }
                else
                {
                    if (model.OldDocumentDate.Value.Date >= DateTime.Now.Date || model.OldDocumentDate.Value.Year < 1900)
                    {
                        ModelState.AddModelError(nameof(DocumentVM.OldDocumentDate), "Невалидна стара дата на документ.");
                    }
                    if (!docService.CheckDocumentOldNumber(userContext.CourtId, model.DocumentDirectionId, model.OldDocumentNumber, model.OldDocumentDate.Value))
                    {
                        ModelState.AddModelError(nameof(DocumentVM.OldDocumentNumber), "Въвели сте съществуващ стар номер.");
                    }
                }

            }
            for (int i = 0; i < model.DocumentPersons.Count(); i++)
            {
                var person = model.DocumentPersons[i];
                if (person.PersonRoleId < 1)
                {
                    ModelState.AddModelError($"{nameof(DocumentVM.DocumentPersons)}[{i}].{nameof(DocumentPersonVM.PersonRoleId)}", "Изберете 'Вид лице'.");
                }
                if (person.Addresses != null)
                {
                    for (int pa = 0; pa < person.Addresses.Count(); pa++)
                    {
                        var adr = person.Addresses[pa].Address;
                        if (adr.AddressTypeId < 0)
                        {
                            ModelState.AddModelError($"{nameof(DocumentVM.DocumentPersons)}[{i}].Addresses[{pa}].Address.AddressTypeId", "Изберете 'Вид адрес'.");
                            person.Addresses[pa].Collapsed = false;
                        }
                        if (adr.CountryCode == NomenclatureConstants.CountryBG && (string.IsNullOrEmpty(adr.CityCode) || adr.CityCode == "0"))
                        {
                            ModelState.AddModelError($"{nameof(DocumentVM.DocumentPersons)}[{i}].Addresses[{pa}].Address.CityCode", "Изберете 'Населено място'.");
                            person.Addresses[pa].Collapsed = false;
                        }
                    }
                }
                //Ако е избрана институция: не се прави валидация на име и идентификатор
                if (model.DocumentPersons[i].Person_SourceType > 0)
                {
                    continue;
                }
                switch (person.UicTypeId)
                {
                    case NomenclatureConstants.UicTypes.Bulstat:
                    case NomenclatureConstants.UicTypes.EIK:
                        if (string.IsNullOrEmpty(person.FullName))
                        {
                            ModelState.AddModelError($"{nameof(DocumentVM.DocumentPersons)}[{i}].{nameof(DocumentPersonVM.FullName)}", "Въведете 'Наименование'.");
                        }
                        break;
                    default:
                        if (string.IsNullOrEmpty(person.FirstName))
                        {
                            ModelState.AddModelError($"{nameof(DocumentVM.DocumentPersons)}[{i}].{nameof(DocumentPersonVM.FirstName)}", "Въведете поне едно име.");
                        }
                        break;
                }
                //Проверка за валиден идентификатор
                if (!string.IsNullOrEmpty(person.Uic))
                {
                    switch (person.UicTypeId)
                    {
                        case NomenclatureConstants.UicTypes.EGN:
                            if (!Utils.Validation.IsEGN(person.Uic))
                            {
                                ModelState.AddModelError($"{nameof(DocumentVM.DocumentPersons)}[{i}].{nameof(DocumentPersonVM.Uic)}", "Невалидно ЕГН.");
                            }
                            break;
                        case NomenclatureConstants.UicTypes.EIK:
                            if (!Utils.Validation.IsEIK(person.Uic))
                            {
                                ModelState.AddModelError($"{nameof(DocumentVM.DocumentPersons)}[{i}].{nameof(DocumentPersonVM.Uic)}", "Невалидно ЕИК.");
                            }
                            break;
                    }
                }

            }
            for (int i = 0; i < model.InstitutionCaseInfo.Count(); i++)
            {
                var instCase = model.InstitutionCaseInfo[i];
                if (!(instCase.InstitutionId > 0))
                {
                    ModelState.AddModelError($"{nameof(DocumentVM.InstitutionCaseInfo)}[{i}].InstitutionId", "Изберете институция");
                }
                if (!(instCase.InstitutionCaseTypeId > 0))
                {
                    ModelState.AddModelError($"{nameof(DocumentVM.InstitutionCaseInfo)}[{i}].InstitutionCaseTypeId", "Изберете вид дело");
                }
                if (string.IsNullOrEmpty(instCase.CaseNumber))
                {
                    ModelState.AddModelError($"{nameof(DocumentVM.InstitutionCaseInfo)}[{i}].CaseNumber", "Въведете номер дело");
                }
            }
            if (model.DocumentKindId == DocumentConstants.DocumentKind.InitialDocument)
            {
                if ((model.CaseTypeId ?? 0) <= 0)
                {
                    ModelState.AddModelError($"{nameof(DocumentVM.CaseTypeId)}", "Изберете 'Точен вид дело'");
                }
            }
            if (model.CaseTypeId > 0)
            {
                var caseType = docService.GetById<CaseType>(model.CaseTypeId.Value);
                if (caseType.CaseInstanceId >= NomenclatureConstants.CaseInstanceType.SecondInstance)
                {
                    if ((
                        !model.HasCaseInfo
                        || (model.DocumentCaseInfo.CaseId <= 0 && (string.IsNullOrEmpty(model.DocumentCaseInfo.CaseRegNumber)))
                        ) 
                        && (model.InstitutionCaseInfo.Count == 0))
                    {
                        ModelState.AddModelError($"{nameof(DocumentVM.HasCaseInfo)}", "Изберете 'Свързано дело' или добавете дело на външна институция");
                    }
                }
                if (model.ProcessPriorityId < 1)
                {
                    ModelState.AddModelError($"{nameof(DocumentVM.ProcessPriorityId)}", "Изберете 'Вид производство'");
                }
            }
            if (model.DocumentPersons.Count() == 0)
            {
                ModelState.AddModelError("", "Въведете поне едно свързано лице.");
            }
            if (model.HasCaseInfo)
            {

                if (model.DocumentCaseInfo.IsLegacyCase)
                {

                    var caseNumberDecoded = nomService.DecodeCaseRegNumber(model.DocumentCaseInfo.CaseRegNumber);
                    if (!caseNumberDecoded.IsValid)
                    {
                        ModelState.AddModelError("DocumentCaseInfo.CaseRegNumber", caseNumberDecoded.ErrorMessage);
                    }
                    else
                    {
                        model.DocumentCaseInfo.CourtId = caseNumberDecoded.CourtId;
                        model.DocumentCaseInfo.CaseShortNumber = caseNumberDecoded.ShortNumber;
                        model.DocumentCaseInfo.CaseYear = caseNumberDecoded.Year;
                    }

                }
                else
                {

                    if (model.DocumentCaseInfo.CourtId <= 0)
                    {
                        ModelState.AddModelError("DocumentCaseInfo.CourtId", "Изберете съд.");
                    }
                    else
                    {
                        if (model.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument)
                        {
                            if (model.DocumentCaseInfo.CourtId != userContext.CourtId)
                            {
                                ModelState.AddModelError("DocumentCaseInfo.CourtId", "Можете да регистрирате съпровождащи документи само по дела на " + userContext.CourtName);
                            }
                        }
                    }
                    if (model.DocumentCaseInfo.CaseId <= 0)
                    {
                        ModelState.AddModelError("DocumentCaseInfo_CaseId_case", "");
                        ModelState.AddModelError("DocumentCaseInfo.CaseId", "Изберете дело.");
                    }
                }
            }
            if (!model.IsMultiNumber)
            {
                model.MultiDocumentCounter = 0;
            }

            for (int i = 0; i < model.DocumentLinks.Count(); i++)
            {
                var item = model.DocumentLinks[i];
                if (item.CourtId <= 0)
                {
                    ModelState.AddModelError($"{nameof(DocumentVM.DocumentLinks)}[{i}].CourtId", "Изберете съд");
                }
                if (item.IsLegacyDocument)
                {
                    model.DocumentLinks[i].PrevDocumentId = null;
                    if (string.IsNullOrEmpty(item.PrevDocumentNumber))
                    {
                        ModelState.AddModelError($"{nameof(DocumentVM.DocumentLinks)}[{i}].PrevDocumentNumber", "Въведете номер на документ");
                    }
                }
                else
                {
                    model.DocumentLinks[i].PrevDocumentNumber = null;
                    model.DocumentLinks[i].PrevDocumentDate = null;
                    if ((item.PrevDocumentId ?? 0) <= 0)
                    {
                        ModelState.AddModelError($"{nameof(DocumentVM.DocumentLinks)}[{i}].PrevDocumentId", "Изберете документ");
                    }
                }
            }
        }

        /// <summary>
        /// Зареждане данни за падащи списъци в екран за Регистриране/Редактиране на документ
        /// </summary>
        /// <param name="model"></param>
        void SetViewBag(DocumentVM model)
        {
            ViewBag.CourtOrganizationId_ddl = docService.GetDocumentRegistratures();
            ViewBag.docDirectionLabel = nomService.GetById<DocumentDirection>(model.DocumentDirectionId).Description;
            ViewBag.DocumentDirectionDDL = nomService.GetDropDownList<DocumentDirection>();
            ViewBag.DocumentKinds = nomService.GetDDL_DocumentKind(model.DocumentDirectionId);
            ViewBag.DeliveryGroupId_ddl = docService.GetDeliveryGroups(model.DocumentDirectionId);
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.ProcessPriorityId_ddl = nomService.GetDropDownList<ProcessPriority>(false);

            SetViewBag_NewPerson();
            SetViewBag_NewAddress();

            ViewBag.InstitutionTypes = nomService.GetDropDownList<InstitutionType>();
            ViewBag.InstitutionCaseTypes = nomService.GetDropDownList<InstitutionCaseType>(true, false, false);
            ViewBag.CourtsDDL = nomService.GetCourts();
            ViewBag.DocumentCaseInfo_CourtId_ddl = nomService.GetCourts();
            ViewBag.ActTypeDDL = nomService.GetDropDownList<ActType>();

            switch (model.DocumentDirectionId)
            {
                case DocumentConstants.DocumentDirection.Incoming:
                    SetHelpFile(HelpFileValues.IncommingDocuments);
                    break;
                case DocumentConstants.DocumentDirection.OutGoing:
                    SetHelpFile(HelpFileValues.OutgoingDocuments);
                    break;
                case DocumentConstants.DocumentDirection.Internal:
                    SetHelpFile(HelpFileValues.InternalDocuments);
                    break;
                default:
                    break;
            }

            if (this.ActionName == nameof(View))
            {
                ViewBag.canCorrect = userContext.IsUserInFeature(AccountConstants.Features.DocumentReactivate);
            }
        }

        /// <summary>
        /// Зареждане данни за падащи списъци за панел Лица
        /// </summary>
        private void SetViewBag_NewPerson()
        {
            ViewBag.PersonRoles = nomService.GetDropDownList<PersonRole>(orderByNumber: false);
            ViewBag.MilitaryRangs = nomService.GetDropDownList<MilitaryRang>();
            ViewBag.PersonMaturities = nomService.GetDropDownList<PersonMaturity>();
            if (!NomenclatureConstants.CourtType.MillitaryCourts.Contains(userContext.CourtTypeId))
            {
                ViewBag.MilitaryRangs = null;
            }
        }

        /// <summary>
        /// Зареждане данни за падащи списъци за панел Адрес
        /// </summary>
        private void SetViewBag_NewAddress()
        {
            ViewBag.CountriesDDL = nomService.GetCountries();
            ViewBag.AddressTypesDDL = nomService.GetDropDownList<AddressType>();
        }

        /// <summary>
        /// Създаване на динамичен панел Лице
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IActionResult NewItem_DocumentPerson(int index)
        {
            var model = new DocumentPersonVM()
            {
                Index = index
            };
            SetViewBag_NewPerson();
            ViewData.TemplateInfo.HtmlFieldPrefix = model.GetPath;
            return PartialView("_DocumentPersonItem", model);
        }

        /// <summary>
        /// Създаване на динамичен панел Лице, иницииран от избор на институция
        /// </summary>
        /// <param name="index"></param>
        /// <param name="sourceType">Вид источник</param>
        /// <param name="sourceId">Идентификатор на данни</param>
        /// <returns></returns>
        public IActionResult NewItem_DocumentPerson_Institution(int index, int sourceType, long sourceId)
        {
            var model = new DocumentPersonVM()
            {
                Index = index,
                Person_SourceType = sourceType,
                Person_SourceId = sourceId
            };
            if (sourceType == SourceTypeSelectVM.LawUnit)
            {
                var lawUnit = docService.GetById<LawUnit>((int)sourceId);
                switch (lawUnit.LawUnitTypeId)
                {
                    case NomenclatureConstants.LawUnitTypes.Lawyer:
                        model.PersonRoleId = 1;
                        break;
                    case NomenclatureConstants.LawUnitTypes.Expert:
                        model.PersonRoleId = 4;
                        break;
                    case NomenclatureConstants.LawUnitTypes.Prosecutor:
                        model.PersonRoleId = 43;
                        break;
                    default:
                        break;
                }
                model.FirstName = lawUnit.FirstName;
                model.MiddleName = lawUnit.MiddleName;
                model.FamilyName = lawUnit.FamilyName;
                model.Family2Name = lawUnit.Family2Name;
            }
            var entityData = commonService.SelectEntity_Select(sourceType, null, null, sourceId).FirstOrDefault();
            if (entityData != null)
            {
                model.DepartmentName = entityData.ObjectTypeName;
                model.FullName = entityData.Label;
                model.UicTypeId = entityData.UicTypeId;
                model.Uic = entityData.Uic;

                if (entityData.SourceType == SourceTypeSelectVM.Instutution)
                {
                    var inst = commonService.GetById<Institution>((int)sourceId);
                    if (inst != null)
                    {
                        model.FirstName = inst.FirstName;
                        model.MiddleName = inst.MiddleName;
                        model.FamilyName = inst.FamilyName;
                        model.Family2Name = inst.Family2Name;
                        switch (inst.InstitutionTypeId)
                        {
                            case NomenclatureConstants.InstitutionTypes.Syndic:
                                model.PersonRoleId = 47;
                                break;
                        }
                    }
                }
            }
            var instAddress = commonService.SelectEntity_SelectAddress(sourceType, sourceId);
            foreach (var adr in instAddress)
            {
                var newAdr = new DocumentPersonAddressVM()
                {
                    PersonIndex = model.Index,
                    Index = model.Addresses.Count()
                };
                newAdr.Address.CopyFrom(adr);
                model.Addresses.Add(newAdr);
            }
            ViewData.TemplateInfo.HtmlFieldPrefix = model.GetPath;
            ViewBag.PersonRoles = nomService.GetDropDownList<PersonRole>(orderByNumber: false);
            SetViewBag_NewAddress();
            return PartialView("_DocumentPersonInstitutionItem", model);
        }

        /// <summary>
        /// Зареждане на панел с избор на лица от свързани данни
        /// </summary>
        /// <param name="eisppNumber">ЕИСПП номер</param>
        /// <param name="priorCaseId">Свързано дело</param>
        /// <param name="priorDocumentId">Свързан документ</param>
        /// <returns></returns>
        public async Task<IActionResult> DocumentPersons_SelectData(string eisppNumber, int? priorCaseId, long priorDocumentId)
        {
            var model = await documentPersonsData(eisppNumber, priorCaseId, priorDocumentId);
            if (model.Count > 0)
            {
                ViewBag.dataUrl = Url.Action(nameof(DocumentPersons_GetData), new { eisppNumber, priorCaseId, priorDocumentId });
                //ViewBag.data = HttpUtility.HtmlEncode(JsonConvert.SerializeObject(model));//.EscapeSingleQuotes();
                return PartialView("_PersonSelectData");
            }
            else
            {
                return Content("");
            }
        }


        public async Task<JsonResult> DocumentPersons_GetData(string eisppNumber, int? priorCaseId, long priorDocumentId)
        {
            var model = await documentPersonsData(eisppNumber, priorCaseId, priorDocumentId);
            return Json(model);
        }

        private async Task<List<DocumentSelectPersonsVM>> documentPersonsData(string eisppNumber, int? priorCaseId, long priorDocumentId)
        {
            var model = new List<DocumentSelectPersonsVM>();
            //Добавяне на лица и адреси по ЕИСПП номер
            if (!string.IsNullOrEmpty(eisppNumber))
            {
                var selectFromEisppPersons = new DocumentSelectPersonsVM()
                {
                    SourceType = SourceTypeSelectVM.Integration_EISPP,
                    SourceId = eisppNumber,
                    SourceTypeName = "Лица по ЕИСПП номер: " + eisppNumber
                };
                var eisppActualData = await eisppService.GetActualData(eisppNumber);
                if (eisppActualData != null)
                {
                    foreach (var eisppPerson in eisppActualData.Persons)
                    {
                        var selectPerson = new DocumentSelectPersonItemVM();
                        selectPerson.ConvertFromEisppPerson(eisppPerson);
                        selectFromEisppPersons.Persons.Add(selectPerson);
                    }
                    model.Add(selectFromEisppPersons);
                }
            }

            //Добавяне на лица и адреси по свързано дело
            if (priorCaseId > 0)
            {
                var selectFromPriorCase = docService.Case_SelectPersons(priorCaseId.Value);
                //3280, РС Бургас
                //selectFromPriorCase.Persons.RemoveAt(0);
                //selectFromPriorCase.Persons.RemoveAt(0);
                //selectFromPriorCase.Persons = selectFromPriorCase.Persons.Take(1).ToList();
                model.Add(selectFromPriorCase);
            }

            //Добавяне на лица и адреси по свързан документ
            if (priorDocumentId > 0)
            {
                var selectFromPriorDocument = docService.Document_SelectPersons(priorDocumentId);
                model.Add(selectFromPriorDocument);
            }

            return model;
        }

        /// <summary>
        /// Добавяне на избраните лица и адреси от свързаните данни
        /// </summary>
        /// <param name="model"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DocumentPersons_SelectData(DocumentSelectPersonsVM model, int index)
        {
            var personList = new List<DocumentPersonVM>();
            switch (model.SourceType)
            {
                case SourceTypeSelectVM.Integration_EISPP:
                    var eisppActualData = await eisppService.GetActualData(model.SourceId);
                    foreach (var eisppPerson in eisppActualData.Persons)
                    {
                        if (model.Persons.Any(x => x.Id == eisppPerson.Sid))
                        {
                            var docPerson = new DocumentPersonVM();

                            eisppService.ConvertEisppPersonToDocumentPerson(eisppPerson, docPerson, index++);

                            personList.Add(docPerson);
                        }
                    }
                    break;
                case SourceTypeSelectVM.Case:
                    personList.AddRange(docService.SelectDocumentPersonsFromCase(model, index));
                    break;
                case SourceTypeSelectVM.Document:
                    personList.AddRange(docService.SelectDocumentPersonsFromDocument(model, index));
                    break;
                default:
                    break;
            }
            SetViewBag_NewPerson();
            SetViewBag_NewAddress();
            string html = "";
            foreach (var docPerson in personList)
            {
                ViewData.TemplateInfo.HtmlFieldPrefix = docPerson.GetPath;
                if (docPerson.Person_SourceType > 0)
                {
                    html += await this.RenderPartialViewAsync("~/Views/Document/", "_DocumentPersonInstitutionItem.cshtml", docPerson, true);
                }
                else
                {
                    html += await this.RenderPartialViewAsync("~/Views/Document/", "_DocumentPersonItem.cshtml", docPerson, true);
                }
            }
            return Content(html);
        }

        /// <summary>
        /// Създаване на нов панел за адрес към лице
        /// </summary>
        /// <param name="personIndex"></param>
        /// <param name="index"></param>
        /// <param name="addressId"></param>
        /// <returns></returns>
        public IActionResult NewItem_DocumentPersonAddress(int personIndex, int index, long? addressId = null)
        {
            var model = new DocumentPersonAddressVM()
            {
                PersonIndex = personIndex,
                Index = index,
                Collapsed = false
            };
            ViewData.TemplateInfo.HtmlFieldPrefix = model.GetPath;
            SetViewBag_NewAddress();
            if (addressId > 0)
            {
                var loadedAddress = docService.GetById<Address>(addressId);
                if (loadedAddress != null)
                {
                    model.Address.CopyFrom(loadedAddress);
                    return PartialView("_DocumentPersonAddressItem", model);
                }
                else
                {
                    return Content("");
                }
            }
            return PartialView("_DocumentPersonAddressItem", model);
        }
        /// <summary>
        /// Създаване на нов панел за адрес към лице
        /// </summary>
        /// <param name="personIndex"></param>
        /// <param name="index"></param>
        /// <param name="uic"></param>
        /// <param name="adrTypeId"></param>
        /// <returns></returns>
        public IActionResult NewItem_DocumentPersonAddressByEGN(int personIndex, int index, string uic, int adrTypeId)
        {
            var model = new DocumentPersonAddressVM()
            {
                PersonIndex = personIndex,
                Index = index,
                Collapsed = false
            };
            ViewData.TemplateInfo.HtmlFieldPrefix = model.GetPath;
            SetViewBag_NewAddress();
            switch (adrTypeId)
            {
                case NomenclatureConstants.AddressType.Permanent:
                    var pAdres = regixService.GetPermanentAddress(uic);
                    model.Address = pAdres.ToEntity();
                    commonService.Address_LocationCorrection(model.Address);
                    break;
                case NomenclatureConstants.AddressType.Current:
                    var tAdres = regixService.GetCurrentAddress(uic);
                    model.Address = tAdres.ToEntity();
                    commonService.Address_LocationCorrection(model.Address);
                    break;
            }
            if (model.Address == null)
            {
                return Content("null");
            }
            SetViewBag_NewAddress();
            return PartialView("_DocumentPersonAddressItem", model);
        }

        /// <summary>
        /// Търсене на предходно въведен адрес по лице
        /// </summary>
        /// <param name="containerId"></param>
        /// <returns></returns>
        public IActionResult DocumentPersonAddress_Search(string containerId)
        {
            ViewBag.containerId = containerId;
            return PartialView("_DocumentPersonAddressSearch");
        }

        /// <summary>
        /// Избор и добавяне на предходно въведен адрес
        /// </summary>
        /// <param name="request"></param>
        /// <param name="uic"></param>
        /// <param name="uicTypeId"></param>
        /// <param name="personSourceType"></param>
        /// <param name="personSourceId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DocumentPersonAddress_Search(IDataTablesRequest request, string uic, int uicTypeId, int? personSourceType,
                        long? personSourceId)
        {
            var data = docService.SelectAddressListByPerson(uic, uicTypeId, personSourceType, personSourceId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Създаване на нов панел за свързан документ
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IActionResult NewItem_DocumentLink(int index)
        {
            var model = new DocumentLinkVM()
            {
                Index = index,
                CourtId = userContext.CourtId
            };
            ViewData.TemplateInfo.HtmlFieldPrefix = model.GetPath;
            ViewBag.DocumentDirectionDDL = nomService.GetDropDownList<DocumentDirection>();
            //ViewBag.CourtsDDL = nomService.GetCourts();
            return PartialView("_DocumentLinkItem", model);
        }

        /// <summary>
        /// Създаване на нов панел за дело на външна институция
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IActionResult NewItem_DocumentInstitutionCaseInfo(int index)
        {
            var model = new DocumentInstitutionCaseInfoVM()
            {
                Index = index,
                CaseYear = DateTime.Now.Year,
            };
            ViewData.TemplateInfo.HtmlFieldPrefix = model.GetPath;
            ViewBag.InstitutionTypes = nomService.GetDropDownList<InstitutionType>();
            ViewBag.InstitutionCaseTypes = nomService.GetDropDownList<InstitutionCaseType>(true, false, false);
            return PartialView("_DocumentInstitutionCaseInfoItem", model);
        }

        /// <summary>
        /// Подаване на създадения документ за електронен подпис
        /// </summary>
        /// <param name="id"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public IActionResult SendDocumentForSign(int id, long taskId)
        {
            Uri urlSuccess = new Uri(Url.Action("View", "Document", new { id = id, taskId = taskId }), UriKind.Relative);
            Uri url = new Uri(Url.Action("View", "Document", new { id = id }), UriKind.Relative);

            var model = new SignPdfInfo()
            {
                SourceId = id.ToString(),
                SourceType = SourceTypeSelectVM.DocumentPdf,
                DestinationType = SourceTypeSelectVM.DocumentPdf,
                Location = "Sofia",
                Reason = "Test",
                SuccessUrl = urlSuccess,
                CancelUrl = url,
                ErrorUrl = url
            };
            var lu = taskService.GetLawUnitByTaskId(taskId);
            if (lu != null)
            {
                model.SignerName = lu.FullName;
                model.SignerUic = lu.Uic;
            }
            return View("_SignPdf", model);
        }


        /// <summary>
        /// Извличане данни за документ по номер
        /// </summary>
        /// <param name="query"></param>
        /// <param name="courtId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult SearchDocument(string query, int courtId = 0)
        {
            if (courtId <= 0)
            {
                courtId = userContext.CourtId;
            }
            return Json(docService.GetDocument(courtId, query));
        }

        /// <summary>
        /// Извличане данни за документ по идентификатор
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetDocumentById(int id)
        {
            var document = docService.GetDocumentById(id);

            if (document == null)
            {
                return BadRequest();
            }

            return Json(document);
        }

        /// <summary>
        /// Справка за Решения по документи
        /// </summary>
        /// <returns></returns>
        public IActionResult DocumentDecision()
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_DocumentDecision().DeleteOrDisableLast();

            DocumentDecisionFilterVM model = new DocumentDecisionFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = DateTime.Now
            };
            SetHelpFile(HelpFileValues.RegisteredDocumentsDecisions);
            return View(model);
        }

        /// <summary>
        /// Извличане данни за Справка за Решения по документи
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DocumentDecisionListData(IDataTablesRequest request, DocumentDecisionFilterVM model)
        {
            var data = docService.DocumentDecision_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на ново решение по документ
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public IActionResult AddDocumentDecision(long documentId)
        {
            var documentDecision = docService.DocumentDecision_SelectForDocument(documentId);

            if (documentDecision != null)
            {
                return RedirectToAction(nameof(EditDocumentDecision), new { id = documentDecision.Id });
            }
            else
            {
                SetViewBagEditDocumentDecision(documentId, 0);
                var model = new DocumentDecision()
                {
                    CourtId = userContext.CourtId,
                    DocumentId = documentId
                };
                return View(nameof(EditDocumentDecision), model);
            }
        }

        /// <summary>
        /// Редактиране на решение по документ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditDocumentDecision(long id)
        {
            var model = docService.GetById<DocumentDecision>(id);
            SetViewBagEditDocumentDecision(model.DocumentId, id);
            return View(nameof(EditDocumentDecision), model);
        }

        /// <summary>
        /// Зареждане на данни за падащи списъци екран Решения
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="id"></param>
        void SetViewBagEditDocumentDecision(long documentId, long id)
        {
            if (id > 0)
                ViewBag.breadcrumbs = commonService.Breadcrumbs_DocumentDecisionEdit(id).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_DocumentDecisionAdd(documentId).DeleteOrDisableLast();

            var document = docService.GetById<Document>(documentId);
            var documentType = docService.GetById<DocumentType>(document.DocumentTypeId);
            ViewBag.documentData = "Решение към Вх.№ " + document.DocumentNumber + "/" + document.DocumentDate.ToString("dd.MM.yyyy").ToString() + " " + documentType.Label;

            ViewBag.DecisionTypeId_ddl = nomService.GetDDL_DecisionType(document.DocumentTypeId);
            ViewBag.DocumentDecisionStateId_ddl = nomService.GetDropDownList<DocumentDecisionState>();
            ViewBag.caseView = false;
            if (id > 0)
            {
                ViewBag.caseView = (documentType.DecisionCaseSelect ?? false) == true;
            }
            SetHelpFile(HelpFileValues.DecisionTask);
        }

        /// <summary>
        /// Валидиране на подадени данни за решение
        /// </summary>
        /// <param name="model"></param>
        void ValidateModelDecision(DocumentDecision model)
        {
            if (model.DocumentDecisionStateId == NomenclatureConstants.DocumentDecisionStates.Resolution && model.DecisionTypeId <= 0)
            {
                ModelState.AddModelError("DecisionTypeId", "Изберете решение");
            }
        }

        /// <summary>
        /// Запис на ново/редактирано решение по документ
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditDocumentDecision(DocumentDecision model)
        {
            SetViewBagEditDocumentDecision(model.DocumentId, model.Id);
            ValidateModelDecision(model);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditDocumentDecision), model);
            }

            var currentId = model.Id;
            (bool result, string errorMessage) = docService.DocumentDecision_SaveData(model);
            if (result == true)
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditDocumentDecision), new { id = model.Id });
            }
            else
            {
                if (string.IsNullOrEmpty(errorMessage))
                    errorMessage = MessageConstant.Values.SaveFailed;
                SetErrorMessage(errorMessage);
            }
            return View(nameof(EditDocumentDecision), model);
        }


        [HttpPost]
        public IActionResult DocumentDecisionCaseListData(IDataTablesRequest request, long documentDecisionId)
        {
            var data = docService.DocumentDecisionCase_Select(documentDecisionId);
            return request.GetResponse(data);
        }

        void SetViewBagEditDocumentDecisionCase(long documentDecisionId)
        {
            var documentDecision = docService.GetById<DocumentDecision>(documentDecisionId);
            var document = docService.GetById<Document>(documentDecision.DocumentId);

            ViewBag.DecisionTypeId_ddl = nomService.GetDDL_DecisionType(document.DocumentTypeId);
        }

        public IActionResult AddDocumentDecisionCase(long documentDecisionId)
        {
            SetViewBagEditDocumentDecisionCase(documentDecisionId);
            var model = new DocumentDecisionCase()
            {
                DocumentDecisionId = documentDecisionId
            };
            return PartialView(nameof(EditDocumentDecisionCase), model);
        }

        public IActionResult EditDocumentDecisionCase(long id)
        {
            var model = docService.GetById<DocumentDecisionCase>(id);
            SetViewBagEditDocumentDecisionCase(model.DocumentDecisionId);

            return PartialView("EditDocumentDecisionCase", model);
        }

        [HttpPost]
        public JsonResult EditDocumentDecisionCase(DocumentDecisionCase model)
        {
            var res = true;
            var error = "";
            if (model.CaseId <= 0)
            {
                res = false;
                error = "Изберете дело";
            }

            if (res == true)
            {
                (bool result, string errorMessage) = docService.DocumentDecisionCase_SaveData(model);
                res = result;
                if (res == false)
                    error = string.IsNullOrEmpty(errorMessage) ? "Проблем при запис" : errorMessage;
            }

            return Json(new { result = res, message = error });
        }

        [HttpPost]
        public IActionResult Document_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!CheckAccess(docService, SourceTypeSelectVM.Document, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }
            if (string.IsNullOrEmpty(model.DescriptionExpired))
            {
                return Json(new { result = false, message = MessageConstant.Values.DescriptionExpireRequired });
            }

            if (!docService.IsCanExpireCompliantDocument(model.LongId))
            {
                return Json(new { result = false, message = "Този документ не може да бъде премахнат, тъй като е разгледан или в процес на разглеждане." });
            }

            if (docService.DocumentExpire(model))
            {
                SetAuditContextDelete(docService, SourceTypeSelectVM.Document, model.Id);
                SetSuccessMessage(MessageConstant.Values.DocumentExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action(nameof(Index)) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        private void SetViewbagDocumentCaseInfoSpr()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.DocumentGroupId_ddl = nomService.GetDDL_DocumentGroupByDirection(DocumentConstants.DocumentDirection.Incoming);
        }

        public IActionResult IndexDocumentCaseInfoSpr()
        {
            SetViewbagDocumentCaseInfoSpr();
            var filter = new DocumentCaseInfoSprFilterVM()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear()
            };

            return View(filter);
        }

        [HttpPost]
        public IActionResult ListDataDocumentCaseInfoSpr(IDataTablesRequest request, DocumentCaseInfoSprFilterVM model)
        {
            var data = docService.DocumentCaseInfoSpr_Select(model);
            return request.GetResponse(data);
        }

        public IActionResult ReactivateDocument()
        {
            if (!userContext.IsUserInFeature(AccountConstants.Features.DocumentReactivate))
            {
                return Redirect_Denied();
            }
            var model = new DocumentReactivateVM();
            ViewBag.DocumentDirectionId_ddl = nomService.GetDropDownList<DocumentDirection>();
            return View(model);
        }
        [HttpPost]
        public IActionResult ReactivateDocument(DocumentReactivateVM model, string search = null)
        {
            if (search != null)
            {
                model.Id = 0;
            }
            docService.Reactivate(model);
            ViewBag.DocumentDirectionId_ddl = nomService.GetDropDownList<DocumentDirection>();
            return View(model);
        }

        public IActionResult IndexDocumentInstitutionCaseInfoList(int id)
        {
            var caseCase = docService.GetById<Case>(id);
            if (!CheckAccess(docService, SourceTypeSelectVM.Document, caseCase.DocumentId, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = new CaseMainDataVM()
            {
                Id = id,
                DocumentId = caseCase.DocumentId
            };
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(id);

            return View(model);
        }

        [HttpPost]
        public IActionResult ListDataDocumentInstitutionCaseInfoList(IDataTablesRequest request, long documentId)
        {
            var data = docService.DocumentInstitutionCaseInfo_Select(documentId);
            return request.GetResponse(data);
        }

        public IActionResult AddDocumentInstitutionCaseInfo(int caseId, long documentId)
        {
            if (!CheckAccess(docService, SourceTypeSelectVM.Case, caseId, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = new DocumentInstitutionCaseInfoEditVM()
            {
                CaseId = caseId,
                DocumentId = documentId,
                CaseYear = DateTime.Now.Year
            };
            SetViewbagDocumentInstitutionCaseInfo(caseId);
            return View(nameof(EditDocumentInstitutionCaseInfo), model);
        }


        public IActionResult EditDocumentInstitutionCaseInfo(int id)
        {
            var model = docService.GetById_InstitutionCaseInfoEditVM(id);
            if (model == null)
            {
                throw new NotFoundException("Търсеният от Вас интервал не е намерен и/или нямате достъп до него.");
            }
            if (!CheckAccess(docService, SourceTypeSelectVM.Case, model.CaseId, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            SetViewbagDocumentInstitutionCaseInfo(model.CaseId);
            return View(nameof(EditDocumentInstitutionCaseInfo), model);
        }

        void SetViewbagDocumentInstitutionCaseInfo(int caseId)
        {
            ViewBag.InstitutionTypeId_ddl = nomService.GetDropDownList<InstitutionType>();
            ViewBag.InstitutionCaseTypeId_ddl = nomService.GetDropDownList<InstitutionCaseType>();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForDocumentInstitutionCaseInfoCase(caseId);
        }

        [HttpPost]
        public IActionResult EditDocumentInstitutionCaseInfo(DocumentInstitutionCaseInfoEditVM model)
        {
            SetViewbagDocumentInstitutionCaseInfo(model.CaseId);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditDocumentInstitutionCaseInfo), model);
            }

            string _isvalid = IsValidDocumentInstitutionCaseInfo(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditDocumentInstitutionCaseInfo), model);
            }

            var currentId = model.Id;
            if (docService.DocumentInstitutionCaseInfo_SaveData(model))
            {
                SetAuditContext(docService, SourceTypeSelectVM.Case, model.CaseId, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditDocumentInstitutionCaseInfo), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditDocumentInstitutionCaseInfo), model);
        }

        private string IsValidDocumentInstitutionCaseInfo(DocumentInstitutionCaseInfoEditVM model)
        {
            if (model.InstitutionId < 1)
                return "Изберете институция";

            if (model.InstitutionCaseTypeId < 1)
                return "Изберете вид дело";

            if (string.IsNullOrEmpty(model.CaseNumber))
                return "Въведете номер дело";

            if (model.CaseYear < 1)
                return "Въведете година";

            return string.Empty;
        }
    }
}