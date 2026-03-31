using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.ApiModels.DocumentRequests;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        //private readonly IRegixReportService regixService;
        private readonly IMQEpepService epepService;
        private readonly IApiDocumentService apiDocService;
        public DocumentController(
            IDocumentService _docService,
            IDocumentTemplateService _templateService,
            INomenclatureService _nomService,
            ICommonService _commonService,
            IWorkTaskService _taskService,
            IEisppService _eisppService,
            ICdnService _cdnService,
            //IRegixReportService _regixService,
            IMQEpepService _epepService,
            IApiDocumentService _apiDocService
        )
        {
            docService = _docService;
            templateService = _templateService;
            nomService = _nomService;
            commonService = _commonService;
            taskService = _taskService;
            eisppService = _eisppService;
            cdnService = _cdnService;
            //regixService = _regixService;
            epepService = _epepService;
            apiDocService = _apiDocService;
        }

        #region Регистрирани документи

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
        public async Task<IActionResult> Index(int? CourtOrganizationId,
                                   int? DocumentDirectionId,
                                   int? DocumentKindId,
                                   int? DocumentGroupId,
                                   int? DocumentTypeId,
                                   string DocumentNumber,
                                   int? DocumentYear,
                                   DateTime? DateFrom,
                                   DateTime? DateTo)
        {
            if (!await CheckAccessAsync(docService, SourceTypeSelectVM.Document, null, AuditConstants.Operations.View))
                return Redirect_Denied();

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
                DateTo = DateTo,
            };

            await ViewBagIndex();
            SetHelpFile(HelpFileValues.CourtRegistry);
            CurrentContext_SetObjectInfo("Търсене в списъчен екран Съдебна регистратура");
            return View(model);
        }

        public async Task<IActionResult> GlobalAssignment()
        {
            DocumentFilterVM model = new DocumentFilterVM()
            {
                GlobalAssignmentRegister = true,
                DocumentDirectionId = DocumentConstants.DocumentDirection.Incoming,
                DocumentYear = DateTime.Now.Year
            };

            await ViewBagIndex();
            SetHelpFile(HelpFileValues.CourtRegistry);
            CurrentContext_SetObjectInfo("Търсене в списъчен екран Централно разпеделение регистратура");
            return View(nameof(Index), model);
        }

        /// <summary>
        /// Извличане на данни за справка Съдебна регистратура
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, DocumentFilterVM filter)
        {
            if (filter.VisibleOtherSystem)
            {
                filter.LinkDelo_CourtId = null;
                filter.LinkDelo_CaseId = null;
                filter.LinkDelo_Description = string.Empty;
            }
            else
            {
                filter.CourtOtherSystem = null;
                filter.YearOtherSystem = null;
                filter.RegNumberOtherSystem = string.Empty;
            }

            var data = docService.Document_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Метод зареждащ номеклатурите за преглед на данни за регистрирани документи
        /// </summary>
        private async Task ViewBagIndex()
        {
            ViewBag.CourtOrganizationId_ddl = await docService.GetDocumentRegistratures(true);
            ViewBag.DocumentDirectionId_ddl = await nomService.GetDropDownListAsync<DocumentDirection>();
            ViewBag.InstitutionTypeId_ddl = await nomService.GetDropDownListAsync<InstitutionType>();

            ViewBag.HasRegNumberOtherSystem = false;
            if (userContext.CourtInstances.Contains(NomenclatureConstants.CaseInstanceType.SecondInstance) ||
                 userContext.CourtInstances.Contains(NomenclatureConstants.CaseInstanceType.ThirdInstance))
            {
                ViewBag.HasRegNumberOtherSystem = true;
            }

            ViewBag.DeliveryGroupInputId_ddl = docService.GetDeliveryGroups(DocumentConstants.DocumentDirection.Incoming, true);
            ViewBag.DeliveryGroupOutputId_ddl = docService.GetDeliveryGroups(DocumentConstants.DocumentDirection.OutGoing, true);
            ViewBag.PersonRoleId_ddl = await nomService.GetDropDownListAsync<PersonRole>();
            ViewBag.DocumentRequestTypeId_ddl = await docService.GetDDL_DocumentRequestTypes(true);
        }

        #endregion


        public async Task<IActionResult> AddForAssignment(int requestType)
        {
            if (!await CheckAccessAsync(docService, SourceTypeSelectVM.Document, null, AuditConstants.Operations.Append))
            {
                return Redirect_Denied();
            }
            var model = await docService.Document_InitFromRequestCodeForAssignment(requestType);
            await SetViewBag(model);
            SetDataKey(model.Id);

            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Регистриране на нов документ
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public async Task<IActionResult> Add(int direction)
        {
            if (!await CheckAccessAsync(docService, SourceTypeSelectVM.Document, null, AuditConstants.Operations.Append))
            {
                return Redirect_Denied();
            }
            var model = await docService.Document_Init(direction);
            await SetViewBag(model);
            SetDataKey(model.Id);

            return View(nameof(Edit), model);
        }

        public IActionResult TestUploadRequest()
        {
            return View("_TestUploadRequest");
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> TestUploadRequest(ICollection<IFormFile> file)
        {
            if (!file.IsEmpty())
            {
                var fileItem = file.First();
                using (var ms = new MemoryStream())
                {
                    fileItem.CopyTo(ms);
                    var json = System.Text.Encoding.UTF8.GetString(ms.ToArray());

                    var testModel = apiDocService.GenerateDocumentWithRequestFromString(json);
                    if (testModel.Item1 == null)
                    {
                        SetErrorMessage("Грешка при изчитане на json файл.");
                        return RedirectToAction(nameof(TestUploadRequest));
                    }
                    var doc = testModel.Item2;
                    var request = testModel.Item1;
                    if (await docService.Document_SaveData(doc))
                    {
                        var jsonContent = JsonConvert.SerializeObject(request);
                        var fileUploadRequest = new CdnUploadRequest()
                        {
                            SourceType = SourceTypeSelectVM.DocumentFileFromAPI,
                            SourceId = doc.Id.ToString(),
                            FileName = "request.json",
                            ContentType = NomenclatureConstants.ContentTypes.Json,
                            FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonContent))
                        };
                        await cdnService.MongoCdn_AppendUpdate(fileUploadRequest);

                        return RedirectToAction(nameof(Edit), new { id = doc.Id });
                    }
                    SetErrorMessage("Проблем при регистриране на документ.");
                    return RedirectToAction(nameof(TestUploadRequest));
                }
            }
            SetErrorMessage("Изберете json файл.");
            return RedirectToAction(nameof(TestUploadRequest));

        }

        public async Task<IActionResult> AddFromApi()
        {
            if (!await CheckAccessAsync(docService, SourceTypeSelectVM.Document, null, AuditConstants.Operations.Append))
            {
                return Redirect_Denied();
            }
            var testModel = apiDocService.GenerateDocumentWithRequest();
            var doc = testModel.Item2;
            var request = testModel.Item1;
            if (await docService.Document_SaveData(doc))
            {
                var jsonContent = JsonConvert.SerializeObject(request);
                var fileUploadRequest = new CdnUploadRequest()
                {
                    SourceType = SourceTypeSelectVM.DocumentFileFromAPI,
                    SourceId = doc.Id.ToString(),
                    FileName = "request.json",
                    ContentType = NomenclatureConstants.ContentTypes.Json,
                    FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonContent))
                };
                await cdnService.MongoCdn_AppendUpdate(fileUploadRequest);

                return RedirectToAction(nameof(Edit), new { id = doc.Id });
            }
            return Redirect_Denied("Проблем при регистриране на документ по заявка.");
        }

        public async Task<IActionResult> ApiRequest(long id)
        {
            var requestContent = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect()
            { SourceType = SourceTypeSelectVM.DocumentFileFromAPI, SourceId = id.ToString() }).ConfigureAwait(true);

            if (string.IsNullOrEmpty(requestContent))
            {
                return Content("");
            }

            DocumentRequestFastProcess dataModel = null;
            try
            {
                dataModel = JsonConvert.DeserializeObject<DocumentRequestFastProcess>(requestContent);

                if (dataModel == null)
                {
                    throw new Exception($"DocumentRequestFastProcess error: DocId = {id}");
                }

            }
            catch (Exception ex)
            {

            }

            ViewBag.bankAccountType = nomService.GetDropDownListFromCode<CaseBankAccountType>();
            ViewBag.claimGroup = nomService.GetDropDownListFromCode<CaseMoneyClaimGroup>();
            ViewBag.claimType = nomService.GetDropDownListFromCode<CaseMoneyClaimType>();
            ViewBag.collectionGroup = nomService.GetDropDownListFromCode<CaseMoneyCollectionGroup>();
            ViewBag.collectionType = nomService.GetDropDownListFromCode<CaseMoneyCollectionType>();
            ViewBag.collectionKind = nomService.GetDropDownListFromCode<CaseMoneyCollectionKind>();
            ViewBag.expenseType = nomService.GetDropDownListFromCode<CaseMoneyExpenseType>();

            return PartialView("_ApiRequest", dataModel);
        }

        /// <summary>
        /// Регистриране на нов документ по подадена бланка на документ
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddFromTemplate(int templateId)
        {
            if (!await CheckAccessAsync(docService, SourceTypeSelectVM.Document, null, AuditConstants.Operations.Append))
            {
                return Redirect_Denied();
            }
            var template = await templateService.GetReadonlyAsync<DocumentTemplate>(templateId);
            if (template == null)
            {
                return RedirectToAction(nameof(Add), new { direction = DocumentConstants.DocumentDirection.OutGoing });
            }
            var model = await docService.Document_Init(DocumentConstants.DocumentDirection.OutGoing, templateId);

            await SetViewBag(model);
            SetDataKey(model.Id);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Регистриране на нов документ по документ от портала
        /// </summary>
        /// <param name="electronicDocumentId"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddFromElectronicDocument(long electronicDocumentId)
        {
            if (!await CheckAccessAsync(docService, SourceTypeSelectVM.Document, null, AuditConstants.Operations.Append))
            {
                return Redirect_Denied();
            }
            var savedDocumentId = await docService.CheckForRegisteredDocumentByElectronicId(electronicDocumentId);
            if (savedDocumentId > 0)
            {
                SetErrorMessage("Електронния документ вече е регистриран.");
                return RedirectToAction(nameof(View), new { id = savedDocumentId });
            }
            var model = await docService.Document_Init(DocumentConstants.DocumentDirection.Incoming, 0, electronicDocumentId);

            await SetViewBag(model);
            SetDataKey(model.Id);
            if (model.ElectronicDocumentId > 0)
            {
                ViewBag.elDocInfo = await docService.GetElectronicDocumentInfo(model.ElectronicDocumentId.Value);
            }

            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редактиране на документ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(long id)
        {
            if (id == 0)
            {
                //Това е нарочно за да не гърми с грешка при проблем с валидирането на нов документ
                return Redirect_Denied("Търсения от Вас документ не е намерен и/или нямате достъп до него.");
            }
            if (!await CheckAccessAsync(docService, SourceTypeSelectVM.Document, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = await docService.Document_GetById(id).ConfigureAwait(false);
            if (model == null)
            {
                return NotFoundError("Търсения от Вас документ не е намерен и/или нямате достъп до него.");
            }
            if (model.DateExpired != null)
            {
                return NotFoundError(MessageConstant.Values.ObjectWasDeleted);
            }
            if (model.CourtId == NomenclatureConstants.Courts.RandomAssignment)
                if (!CurrentContext.CanChange)
                {
                    return RedirectToAction(nameof(View), new { id = id });
                }
            if (model.CaseId > 0 && !string.IsNullOrEmpty(model.CaseRegisterNumber))
            {
                //Ако по намерения документ вече има регистрирано дело потребителя се пренасочва към преглед
                return RedirectToAction(nameof(View), new { id = id });
            }
            await SetViewBag(model);
            SetDataKey(model.Id);
            if (model.ElectronicDocumentId > 0)
            {
                ViewBag.elDocInfo = await docService.GetElectronicDocumentInfo(model.ElectronicDocumentId.Value);
            }
            return View(nameof(Edit), model);
        }

        public async Task<IActionResult> Correction(long id)
        {
            if (!await CheckAccessAsync(docService, SourceTypeSelectVM.Document, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = await docService.Document_GetById(id).ConfigureAwait(false);
            if (model == null)
            {
                return NotFoundError("Търсения от Вас документ не е намерен и/или нямате достъп до него.");
            }
            if (model.DateExpired != null)
            {
                return NotFoundError(MessageConstant.Values.ObjectWasDeleted);
            }
            if ((model.CaseId ?? 0) == 0 || string.IsNullOrEmpty(model.CaseRegisterNumber))
            {
                //Ако по намерения документ вече има регистрирано дело потребителя се пренасочва към преглед
                return RedirectToAction(nameof(Edit), new { id = id });
            }
            SetDataKey(model.Id);
            await SetViewBag(model);
            return View(model);
        }

        /// <summary>
        /// Преглед на документ
        /// </summary>
        /// <param name="id"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public async Task<IActionResult> View(long id, long? taskId = null)
        {
            if (!await CheckAccessAsync(docService, SourceTypeSelectVM.Document, id, AuditConstants.Operations.View))
            {
                return Redirect_Denied();
            }
            var model = await docService.Document_GetById(id);
            if (model == null)
            {
                return NotFoundError("Търсения от Вас документ не е намерен и/или нямате достъп до него.");
            }
            if (model.DateExpired != null)
            {
                return NotFoundError(MessageConstant.Values.ObjectWasDeleted);
            }
            if (taskId > 0)
            {
                var task = await taskService.ReadByIdAsync<WorkTask>(taskId.Value);
                if (task != null && task.TaskStateId != WorkTaskConstants.States.Completed)
                {
                    switch (task.TaskTypeId)
                    {
                        case WorkTaskConstants.Types.Document_Sign:
                            if (await taskService.CompleteTask(taskId.Value))
                            {
                                await taskService.UpdateAfterCompleteTask(task);
                                //var docFile = cdnService.Select(SourceTypeSelectVM.DocumentPdf, id.ToString()).FirstOrDefault();
                                //epepService.AppendFile(new CdnUploadRequest()
                                //{
                                //    SourceType = docFile.SourceType,
                                //    SourceId = docFile.SourceId,
                                //    FileId = docFile.FileId
                                //}, EpepConstants.ServiceMethod.Add);
                            }
                            break;
                    }
                }
            }
            await SetViewBag(model);
            if (model.ElectronicDocumentId > 0)
            {
                ViewBag.elDocInfo = await docService.GetElectronicDocumentInfo(model.ElectronicDocumentId.Value);
            }
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на документ/регистриране на нов документ
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [RequestFormLimits(ValueCountLimit = 99999999)]
        public async Task<IActionResult> Edit(DocumentVM model)
        {
            if (model.ElectronicDocumentId > 0 && model.Id == 0)
            {
                //Проверка за вече регистриран електронен документ се прави само при добавяне
                var savedDocumentId = await docService.CheckForRegisteredDocumentByElectronicId(model.ElectronicDocumentId.Value);
                if (savedDocumentId > 0)
                {
                    SetErrorMessage("Електронния документ вече е регистриран.");
                    return RedirectToAction(nameof(View), new { id = savedDocumentId });
                }

            }
            await ValidateModel(model);
            CheckDataKey(model.Id);
            if (!ModelState.IsValid)
            {
                if (CurrentContext != null)
                    if (!CurrentContext.CanChange)
                        SetAuditContext(docService, SourceTypeSelectVM.Document, model.Id, model.Id == 0);

                await SetViewBag(model);
                if (model.ElectronicDocumentId > 0)
                {
                    ViewBag.elDocInfo = await docService.GetElectronicDocumentInfo(model.ElectronicDocumentId.Value);
                }
                return View(nameof(Edit), model);
            }
            ModelState.Clear();
            long currentId = model.Id;
            if (await docService.Document_SaveData(model).ConfigureAwait(true))
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
            await SetViewBag(model);
            if (model.ElectronicDocumentId > 0)
            {
                ViewBag.elDocInfo = await docService.GetElectronicDocumentInfo(model.ElectronicDocumentId.Value);
            }
            return View(nameof(Edit), model);
        }

        [HttpPost]
        [RequestFormLimits(ValueCountLimit = 99999999)]
        public async Task<IActionResult> Correction(DocumentVM model)
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

            await SetViewBag(model);
            return View(nameof(Correction), model);
        }

        /// <summary>
        /// Валидиране на подадените данни за документ
        /// </summary>
        /// <param name="model"></param>
        async Task ValidateModel(DocumentVM model)
        {
            if (model.DocumentGroupId > 0)
            {
                var docKindId = docService.GetPropById<DocumentGroup, int>(model.DocumentGroupId, x => x.DocumentKindId);
                if (model.DocumentKindId != docKindId)
                {
                    ModelState.AddModelError($"{nameof(model.DocumentKindId)}", "Грешен вид документ.");
                }
            }
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
                        model.DocumentPersons[i].IsDeceased = null;
                        model.DocumentPersons[i].DateDeceased = null;
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
                        case NomenclatureConstants.UicTypes.LNCh:
                            if (!Utils.Validation.IsLnch(person.Uic))
                            {
                                ModelState.AddModelError($"{nameof(DocumentVM.DocumentPersons)}[{i}].{nameof(DocumentPersonVM.Uic)}", "Невалидно ЛНЧ.");
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
            var personValidationRandomCourt = await docService.ValidatePersonOrgs(model);
            if (!personValidationRandomCourt.Result)
            {
                ModelState.AddModelError("", personValidationRandomCourt.ErrorMessage);
            }

            var noCRvalidation = await docService.ValidateDocumentAfterCR(model);
            if (!noCRvalidation.Result)
            {
                ModelState.AddModelError("", noCRvalidation.ErrorMessage);
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
            switch (model.DocumentKindId)
            {
                case DocumentConstants.DocumentKind.InitialDocument:
                    {
                        if ((model.CaseTypeId ?? 0) <= 0)
                        {
                            ModelState.AddModelError($"{nameof(DocumentVM.CaseTypeId)}", "Изберете 'Точен вид дело'");
                        }

                        if (commonService.CheckCourtRestriction(NomenclatureConstants.CourtRestrictionTypes.DisableInitDocument))
                        {
                            ModelState.AddModelError($"{nameof(DocumentVM.DocumentKindId)}", $"Не можете да регистрирате иницииращи документ в {userContext.CourtName}.");
                        }
                    }
                    break;
                //case DocumentConstants.DocumentKind.CompliantDocument:
                //    {
                //        if (model.DocumentCaseInfo.CaseId <= 0)
                //        {
                //            ModelState.AddModelError("DocumentCaseInfo_CaseId_case", "");
                //            ModelState.AddModelError("DocumentCaseInfo.CaseId", "Изберете дело.");
                //        }
                //    }
                //    break;
                default:
                    break;
            }

            if (DocumentConstants.Types.DocumentsMustHaveCaseInfo.Contains(model.DocumentTypeId ?? 0) && !model.HasCaseInfo && model.InstitutionCaseInfo.Count == 0)
            {
                ModelState.AddModelError($"{nameof(DocumentVM.HasCaseInfo)}", "Изберете 'Свързано дело' или добавете дело на външна институция");
            }

            if (model.CaseTypeId > 0)
            {
                var caseTypeCaseInstanceId = docService.GetPropById<CaseType, int>(model.CaseTypeId.Value, x => x.CaseInstanceId);
                if (caseTypeCaseInstanceId >= NomenclatureConstants.CaseInstanceType.SecondInstance)
                {
                    var mustSelectCase = true;
                    if (userContext.CourtTypeId == NomenclatureConstants.CourtType.VKS
                        && DocumentConstants.Types.DocumentsNoNeedCaseInfo.Contains(model.DocumentTypeId ?? 0))
                    {
                        mustSelectCase = false;
                    }
                    if (mustSelectCase && (
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
            if (model.HasCaseInfo || (model.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument))
            {
                ValidateModel_CaseInfo(model);

                if (model.DocumentGroupId == NomenclatureConstants.DocumentGroup.DocumentForComplain_AccompanyingDocument)
                {
                    if (((model.DocumentCaseInfo.SessionActId <= 0) || (!model.DocumentCaseInfo.HasLawAct)) && !NomenclatureConstants.DocumentType.ComplainDocsWithoutAct.Contains(model.DocumentTypeId ?? 0))
                    {
                        ModelState.AddModelError("DocumentCaseInfo.HasLawAct", "Изберете съдебен акт.");
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
                if (item.IsLegacyDocument ?? false)
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

        void ValidateModel_CaseInfo(DocumentVM model)
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
                    //Ако делото е от друга система се премахва идентификатор на дело, ако има
                    model.CaseId = null;
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
                        if (model.DocumentCaseInfo.CourtId != userContext.CourtId && model.DocumentCaseInfo.CaseId > 0)
                        {
                            var isNewFastpProcessCase = docService.GetPropById<Case, bool>(x => x.Id == model.DocumentCaseInfo.CaseId, c => c.IsFastProcess ?? false);
                            if (!isNewFastpProcessCase)
                            {
                                ModelState.AddModelError("DocumentCaseInfo.CourtId", "Можете да регистрирате съпровождащи документи само по дела на " + userContext.CourtName);
                            }
                        }
                    }
                }
                if ((model.DocumentCaseInfo.CaseId ?? 0) <= 0)
                {
                    ModelState.AddModelError("DocumentCaseInfo_CaseId_case", "");
                    ModelState.AddModelError("DocumentCaseInfo.CaseId", "Изберете дело.");
                }
            }
        }

        /// <summary>
        /// Зареждане данни за падащи списъци в екран за Регистриране/Редактиране на документ
        /// </summary>
        /// <param name="model"></param>
        async Task SetViewBag(DocumentVM model)
        {
            ViewBag.CourtOrganizationId_ddl = await docService.GetDocumentRegistratures();
            ViewBag.docDirectionLabel = await nomService.GetPropByIdAsync<DocumentDirection, string>(x => x.Id == model.DocumentDirectionId, x => x.Description);
            ViewBag.DocumentDirectionDDL = await nomService.GetDropDownListAsync<DocumentDirection>();
            var docKinds = nomService.GetDDL_DocumentKind(model.DocumentDirectionId);
            ViewBag.DocumentKinds = docKinds;
            if (model.DocumentKindId == 0 && docKinds.Count > 0)
            {
                model.DocumentKindId = int.Parse(docKinds.First().Value);
            }
            ViewBag.DeliveryGroupId_ddl = docService.GetDeliveryGroups(model.DocumentDirectionId);
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.ProcessPriorityId_ddl = await nomService.GetDropDownListAsync<ProcessPriority>(false);

            await SetViewBag_NewPerson();
            SetViewBag_NewAddress();

            ViewBag.InstitutionTypes = await nomService.GetDropDownListAsync<InstitutionType>();
            ViewBag.InstitutionCaseTypes = await nomService.GetDropDownListAsync<InstitutionCaseType>(true, false, false);
            var courtsList = nomService.GetCourts();
            ViewBag.CourtsDDL = courtsList;
            ViewBag.DocumentCaseInfo_CourtId_ddl = courtsList;
            ViewBag.ActTypeDDL = await nomService.GetDropDownListAsync<ActType>();
            ViewBag.hasApiRequest = false;
            if (model.DeliveryGroupId == DocumentConstants.DeliveryGroups.WebPortal && model.Id > 0)
            {
                ViewBag.hasApiRequest = await cdnService.Select(SourceTypeSelectVM.DocumentFileFromAPI, model.Id.ToString()).AnyAsync();
            }

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

            if (this.ActionName == nameof(View) && model.CourtId == userContext.CourtId)
            {
                ViewBag.canCorrect = userContext.IsUserInFeature(AccountConstants.Features.DocumentReactivate);
            }
            model.RegixRequestReason.RegixReasonDocumentId = model.Id;
            model.RegixRequestReason.RegixRequestTypeId = NomenclatureConstants.RegixRequestTypes.FromDocument;
            //if (model.CourtId == NomenclatureConstants.Courts.RandomAssignment && model.Id > 0)
            //{
            //    ViewBag.AssignedDocuments = await docService.GetAssignedDocumentsInfo(model.Id);
            //}
        }

        /// <summary>
        /// Зареждане данни за падащи списъци за панел Лица
        /// </summary>
        private async Task SetViewBag_NewPerson()
        {
            //ViewBag.PersonRoles = nomService.GetDropDownList<PersonRole>(orderByNumber: false);
            ViewBag.MilitaryRangs = await nomService.GetDropDownListAsync<MilitaryRang>();
            ViewBag.PersonMaturities = await nomService.GetDropDownListAsync<PersonMaturity>();
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
        public async Task<IActionResult> NewItem_DocumentPerson(int index)
        {
            var model = new DocumentPersonVM()
            {
                Index = index,
                PersonGid = docService.PersonNamesBase_GeneratePersonGid(),
                NewDynamicItem = true
            };
            await SetViewBag_NewPerson();
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
                PersonGid = docService.PersonNamesBase_GeneratePersonGid(),
                Person_SourceType = sourceType,
                Person_SourceId = sourceId,
                NewDynamicItem = true
            };
            if (sourceType == SourceTypeSelectVM.LawUnit)
            {
                var lawUnit = docService.GetReadonly<LawUnit>((int)sourceId);
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
                    var inst = commonService.GetReadonly<Institution>((int)sourceId);
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
            var instAddress = commonService.SelectEntity_SelectAddress(sourceType, sourceId).ToList();
            foreach (var adr in instAddress)
            {
                if (adr != null)
                {
                    var newAdr = new DocumentPersonAddressVM()
                    {
                        PersonIndex = model.Index,
                        Index = model.Addresses.Count()
                    };
                    newAdr.Address.CopyFrom(adr);
                    model.Addresses.Add(newAdr);
                }
            }
            ViewData.TemplateInfo.HtmlFieldPrefix = model.GetPath;
            //ViewBag.PersonRoles = nomService.GetDropDownList<PersonRole>(orderByNumber: false);
            if (model.Addresses.Any())
            {
                SetViewBag_NewAddress();
            }
            return PartialView("_DocumentPersonInstitutionItem", model);
        }

        /// <summary>
        /// Зареждане на панел с избор на лица от свързани данни
        /// </summary>
        /// <param name="eisppNumber">ЕИСПП номер</param>
        /// <param name="priorCaseId">Свързано дело</param>
        /// <param name="priorDocumentId">Свързан документ</param>
        /// <returns></returns>
        [DisableAudit]
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
                            var _modelPerson = model.Persons.Where(x => x.Id == eisppPerson.Sid).FirstOrDefault();
                            eisppPerson.SelectedAddresses = _modelPerson.Addresses.Select(x => x.Id.ToString()).ToArray();
                            eisppService.ConvertEisppPersonToDocumentPerson(eisppPerson, docPerson, index++);

                            personList.Add(docPerson);
                        }
                    }
                    break;
                case SourceTypeSelectVM.Case:
                    personList.AddRange(await docService.SelectDocumentPersonsFromCase(model, index));
                    break;
                case SourceTypeSelectVM.Document:
                    personList.AddRange(docService.SelectDocumentPersonsFromDocument(model, index));
                    break;
                default:
                    break;
            }
            await SetViewBag_NewPerson();
            SetViewBag_NewAddress();
            string html = "";
            int lastIndex = personList.Max(x => x.Index);
            foreach (var docPerson in personList)
            {
                if (docPerson.Index == lastIndex)
                {
                    docPerson.NewDynamicItem = true;
                }
                else
                {
                    docPerson.NewDynamicItem = false;
                }
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
                var loadedAddress = docService.GetReadonly<Address>(addressId.Value);
                if (loadedAddress != null)
                {
                    model.Address.CopyFrom(loadedAddress);
                    model.Address.Id = 0;
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
        /// <param name="regixReasonDocumentId"></param>
        /// <param name="regixReasonCaseId"></param>
        /// <param name="regixReasonDescription"></param>
        /// <param name="regixReasonGuid"></param>
        /// <returns></returns>
        public async Task<IActionResult> NewItem_DocumentPersonAddressByEGN(int personIndex, int index, string uic, int adrTypeId,
            long? regixReasonDocumentId, int? regixReasonCaseId, string regixReasonDescription, string regixReasonGuid)
        {
            var _regixService = (IRegixReportService)HttpContext.RequestServices.GetService(typeof(IRegixReportService));


            var model = new DocumentPersonAddressVM()
            {
                PersonIndex = personIndex,
                Index = index,
                Collapsed = false
            };
            ViewData.TemplateInfo.HtmlFieldPrefix = model.GetPath;
            SetViewBag_NewAddress();
            string baseInfo = "Регистрацията на документа не е завършена от потребител";
            if (regixReasonDocumentId > 0)
            {
                var doc = docService.GetReadonly<Document>(regixReasonDocumentId.Value);
                if (doc != null)
                {
                    baseInfo = $"Документ {doc.DocumentNumber}";
                }
            }

            switch (adrTypeId)
            {
                case NomenclatureConstants.AddressType.Permanent:
                    var pAdres = await _regixService.GetPermanentAddressAndSave(uic, regixReasonDocumentId, regixReasonCaseId, regixReasonDescription, regixReasonGuid, NomenclatureConstants.RegixRequestTypes.FromDocument);
                    model.Address = pAdres.ToEntity();
                    if (model.Address != null)
                        commonService.Address_LocationCorrection(model.Address);
                    AddAuditInfo("Преглед", baseInfo, $"Проверка в НБД за постоянен адрес на лице по ЕГН {uic}");
                    break;
                case NomenclatureConstants.AddressType.Current:
                    var tAdres = await _regixService.GetCurrentAddressAndSave(uic, regixReasonDocumentId, regixReasonCaseId, regixReasonDescription, regixReasonGuid, NomenclatureConstants.RegixRequestTypes.FromDocument);
                    model.Address = tAdres.ToEntity();
                    if (model.Address != null)
                        commonService.Address_LocationCorrection(model.Address);
                    AddAuditInfo("Преглед", baseInfo, $"Проверка в НБД за настоящ адрес на лице по ЕГН {uic}");
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
        public async Task<IActionResult> DocumentPersonAddress_Search(IDataTablesRequest request, string uic, int uicTypeId, int? personSourceType,
                        long? personSourceId)
        {
            var data = await docService.SelectAddressListByPerson(uic, uicTypeId, personSourceType, personSourceId);
            bool fromDataBase = personSourceType > 0;
            return request.GetResponse(data, null, null, fromDataBase);
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
        public IActionResult SendDocumentForSign(long id, long taskId)
        {
            Uri urlSuccess = new Uri(Url.Action("View", "Document", new { id = id }), UriKind.Relative);
            Uri url = new Uri(Url.Action("View", "Document", new { id = id }), UriKind.Relative);

            var model = new SignPdfInfo()
            {
                SourceId = id.ToString(),
                SourceType = SourceTypeSelectVM.DocumentPdf,
                DestinationType = SourceTypeSelectVM.DocumentPdf,
                Location = userContext.CourtName,
                Reason = "Подписване на изходящ документ",
                SuccessUrl = urlSuccess,
                CancelUrl = url,
                ErrorUrl = url,
                WorkTaskId = taskId
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
        /// <param name="docDir"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult SearchDocument(string query, int courtId = 0, int docDir = 0)
        {
            if (courtId <= 0)
            {
                courtId = userContext.CourtId;
            }
            return Json(docService.GetDocument(courtId, query, docDir));
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
            CurrentContext_SetObjectInfo("Търсене в списъчен екран Решения по документи");
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
        public async Task<IActionResult> AddDocumentDecision(long documentId)
        {
            var documentDecision = docService.DocumentDecision_SelectForDocument(documentId);

            if (!await CheckAccessAsync(docService, SourceTypeSelectVM.DocumentDecision, null, AuditConstants.Operations.Append, documentId))
            {
                return Redirect_Denied();
            }

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
                SetDataKey(model.Id);
                return View(nameof(EditDocumentDecision), model);
            }
        }

        /// <summary>
        /// Редактиране на решение по документ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditDocumentDecision(long id)
        {
            var model = docService.GetById<DocumentDecision>(id);

            if (!await CheckAccessAsync(docService, SourceTypeSelectVM.DocumentDecision, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            SetViewBagEditDocumentDecision(model.DocumentId, id);
            SetDataKey(model.Id);
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

            var document = docService.GetReadonly<Document>(documentId);
            var documentType = docService.GetReadonly<DocumentType>(document.DocumentTypeId);
            ViewBag.documentData = "Решение към Вх.№ " + document.DocumentNumber + "/" + document.DocumentDate.ToString("dd.MM.yyyy").ToString() + " " + documentType.Label;

            ViewBag.DecisionTypeId_ddl = nomService.GetDDL_DecisionType(document.DocumentTypeId);
            ViewBag.DocumentDecisionStateId_ddl = nomService.GetDropDownList<DocumentDecisionState>();
            ViewBag.caseView = false;
            ViewBag.epepView = document.DocumentTypeId != NomenclatureConstants.DocumentType.PublicInformation;
            if (id > 0)
            {
                ViewBag.caseView = (documentType.DecisionCaseSelect ?? false) == true;
                //04.04.2023, Потребители в ЕПЕП се създават само през портала
                var epepUser = epepService.EpepUser_GetByDocument(documentId);
                if (epepUser != null)
                {
                    ViewBag.epepUserId = epepUser.Id;
                }
            }
            SetHelpFile(HelpFileValues.DecisionTask);
        }

        /// <summary>
        /// Валидиране на подадени данни за решение
        /// </summary>
        /// <param name="model"></param>
        void ValidateModelDecision(DocumentDecision model)
        {
            if (model.DecisionTypeId <= 0)
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
        public async Task<IActionResult> EditDocumentDecision(DocumentDecision model)
        {
            SetViewBagEditDocumentDecision(model.DocumentId, model.Id);
            ValidateModelDecision(model);
            CheckDataKey(model.Id);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditDocumentDecision), model);
            }

            var currentId = model.Id;
            (bool result, string errorMessage) = await docService.DocumentDecision_SaveData(model);
            if (result == true)
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                var decisionTypeLabel = docService.GetPropById<DecisionType, string>(model.DecisionTypeId ?? 0, x => x.Label);
                CurrentContext_SetOperation($"Решение: {decisionTypeLabel}");
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

        [DisableAudit]
        [HttpPost]
        public IActionResult DocumentDecisionCaseListData(IDataTablesRequest request, long documentDecisionId)
        {
            var data = docService.DocumentDecisionCase_Select(documentDecisionId);
            return request.GetResponse(data);
        }

        void SetViewBagEditDocumentDecisionCase(long documentDecisionId)
        {
            var documentId = docService.GetPropById<DocumentDecision, long>(documentDecisionId, x => x.DocumentId);
            var documentTypeId = docService.GetPropById<Document, int>(documentId, x => x.DocumentTypeId);

            ViewBag.DecisionTypeId_ddl = nomService.GetDDL_DecisionType(documentTypeId);
            ViewBag.DecisionRequestTypeId_ddl = nomService.GetDropDownList<DecisionRequestType>(false);
        }

        [DisableAudit]
        public PartialViewResult AddDocumentDecisionCase(long documentDecisionId)
        {
            SetViewBagEditDocumentDecisionCase(documentDecisionId);
            var model = new DocumentDecisionCase()
            {
                DocumentDecisionId = documentDecisionId
            };
            return PartialView(nameof(EditDocumentDecisionCase), model);
        }

        [DisableAudit]
        public IActionResult EditDocumentDecisionCase(long id)
        {
            var model = docService.GetById<DocumentDecisionCase>(id);
            if (model == null)
            {
                return NotFoundError("Търсения от Вас обект не е намерен и/или нямате достъп до него.");
            }
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
            else
            {

                var _case = docService.GetReadonly<IOWebApplication.Infrastructure.Data.Models.Cases.Case>(model.CaseId);
                if (_case.CourtId != userContext.CourtId)
                {
                    res = false;
                    error = "Нямате достъп до избраното дело";
                }
                if (NomenclatureConstants.CaseState.DisableEditStates.Contains(_case.CaseStateId))
                {
                    res = false;
                    error = "Избраното дело е анулирано или унищожено!";
                }
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
        public async Task<IActionResult> Document_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!await CheckAccessAsync(docService, SourceTypeSelectVM.Document, model.LongId, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }
            if (!this.CurrentContext.CanChangeFull)
            {
                return Json(new { result = false, message = "Този документ не може да бъде премахнат." });
            }

            if (!CheckSourceKey(SourceTypeSelectVM.ExpireObject, model.KeyString, $"ExpInfo{userContext?.UserId}"))
            {
                return SourceKeyExpireJsonError();
            }
            if (!model.IsValidDescription)
            {
                return SourceKeyExpireJsonErrorDescription();
            }

            var checkStatus = docService.CheckCanExpireDocument(model.LongId);
            if (!checkStatus.Result)
            {
                return Json(new { result = false, message = checkStatus.ErrorMessage });
            }

            if (!docService.IsCanExpireCompliantDocument(model.LongId))
            {
                return Json(new { result = false, message = "Този документ не може да бъде премахнат, тъй като е разгледан или в процес на разглеждане." });
            }

            if (await docService.DocumentExpire(model))
            {
                SetAuditContextDelete(docService, SourceTypeSelectVM.Document, model.LongId);
                SetSuccessMessage(MessageConstant.Values.DocumentExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action(nameof(Index)) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        #region Справка за съпровождащи документи

        /// <summary>
        /// Страница за справка съпровождащи документи
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public IActionResult IndexDocumentCaseInfoSpr()
        {
            SetViewbagDocumentCaseInfoSpr();
            var filter = new DocumentCaseInfoSprFilterVM()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear()
            };
            SetHelpFile(HelpFileValues.Report29);

            return View(filter);
        }

        /// <summary>
        /// Метод извличащ данни за справка съпровождащи документи
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataDocumentCaseInfoSpr(IDataTablesRequest request, DocumentCaseInfoSprFilterVM filter)
        {
            var data = docService.DocumentCaseInfoSpr_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зарежда номенклатури за филтър за справка съпровождащи документи
        /// </summary>
        private void SetViewbagDocumentCaseInfoSpr()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            //ViewBag.DocumentGroupId_ddl = nomService.GetDDL_DocumentGroupByDirection(DocumentConstants.DocumentDirection.Incoming);
            ViewBag.DocumentGroupId_ddl = nomService.GetDDL_DocumentGroup(DocumentConstants.DocumentKind.CompliantDocument);
            ViewBag.SessionDocTypeId_ddl = nomService.GetDropDownList<SessionDocType>();
        }

        #endregion

        [DisableAudit]
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
        public async Task<IActionResult> ReactivateDocument(DocumentReactivateVM model, string search = null)
        {
            if (search != null)
            {
                model.Id = 0;
            }
            await docService.Reactivate(model);
            ViewBag.DocumentDirectionId_ddl = nomService.GetDropDownList<DocumentDirection>();
            if (model.IsActivated)
            {
                AddAuditInfo(AuditConstants.Operations.Patch, "Възстановяване на документ", $"{model.DocumentNumber}/{model.DocumentDate:dd.MM.yyyy} - {model.DocumentInfo}", SourceTypeSelectVM.Document);
            }
            else
            {
                DisableAudit();
            }
            return View(model);
        }

        public async Task<IActionResult> IndexDocumentInstitutionCaseInfoList(int id)
        {
            var caseDocumentId = docService.GetPropById<Case, long>(id, x => x.DocumentId);
            if (!await CheckAccessAsync(docService, SourceTypeSelectVM.Document, caseDocumentId, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = new CaseMainDataVM()
            {
                Id = id,
                DocumentId = caseDocumentId
            };
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(id);
            SetHelpFile(HelpFileValues.OtherInstitution);
            CurrentContext_SetOperation(AuditConstants.Operations.List);
            CurrentContext_SetObjectInfo("Дела на други институции");
            return View(model);
        }

        [HttpPost]
        public IActionResult ListDataDocumentInstitutionCaseInfoList(IDataTablesRequest request, long documentId)
        {
            var data = docService.DocumentInstitutionCaseInfo_Select(documentId);
            return request.GetResponse(data);
        }

        public async Task<IActionResult> AddDocumentInstitutionCaseInfo(int caseId, long documentId)
        {
            if (!await CheckAccessAsync(docService, SourceTypeSelectVM.Document, documentId, AuditConstants.Operations.Append))
            {
                return Redirect_Denied();
            }
            var model = new DocumentInstitutionCaseInfoEditVM()
            {
                CaseId = caseId,
                DocumentId = documentId,
                CaseYear = DateTime.Now.Year
            };
            CurrentContext_SetOperation(AuditConstants.Operations.Append);
            CurrentContext_SetObjectInfo("Дела на други институции");

            SetViewbagDocumentInstitutionCaseInfo(caseId);
            return View(nameof(EditDocumentInstitutionCaseInfo), model);
        }


        public async Task<IActionResult> EditDocumentInstitutionCaseInfo(int id)
        {
            var model = docService.GetById_InstitutionCaseInfoEditVM(id);
            if (model == null)
            {
                return NotFoundError("Търсеният от Вас интервал не е намерен и/или нямате достъп до него.");
            }
            if (!await CheckAccessAsync(docService, SourceTypeSelectVM.Document, model.DocumentId, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            CurrentContext_SetOperation(AuditConstants.Operations.Update);
            CurrentContext_SetObjectInfo($"{model.InstitutionName} {model.CaseNumber}/{model.CaseYear}");

            SetViewbagDocumentInstitutionCaseInfo(model.CaseId);
            return View(nameof(EditDocumentInstitutionCaseInfo), model);
        }

        void SetViewbagDocumentInstitutionCaseInfo(int caseId)
        {
            ViewBag.InstitutionTypeId_ddl = nomService.GetDropDownList<InstitutionType>();
            ViewBag.InstitutionCaseTypeId_ddl = nomService.GetDropDownList<InstitutionCaseType>();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForDocumentInstitutionCaseInfoCase(caseId);
            SetHelpFile(HelpFileValues.OtherInstitution);
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
                SetAuditContext(docService, SourceTypeSelectVM.Document, model.DocumentId, currentId == 0);
                var instName = docService.GetPropById<Institution, string>(x => x.Id == model.InstitutionId, x => x.FullName);
                CurrentContext_SetObjectInfo($"{instName} {model.CaseNumber}/{model.CaseYear}");
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

        public IActionResult Get_PersonsByDocument(long documentId)
        {
            return Json(docService.GetDocumentPersonsByDocumentId(documentId));
        }

        public async Task<IActionResult> ConvertToCompliant(long id)
        {
            if (!await CheckAccessAsync(docService, SourceTypeSelectVM.Document, id, AuditConstants.Operations.Update)
                || !userContext.IsUserInRole(AccountConstants.Roles.Supervisor))
            {
                return Redirect_Denied();
            }
            var model = await docService.Document_GetById(id);
            if (model == null)
            {
                return NotFoundError("Търсения от Вас документ не е намерен и/или нямате достъп до него.");
            }
            if (model.DateExpired != null)
            {
                return NotFoundError(MessageConstant.Values.ObjectWasDeleted);
            }
            if (model.DocumentDirectionId != DocumentConstants.DocumentDirection.Incoming)
            {
                SetErrorMessage("Избраният документ не е входящ от Обща администрация");
                return RedirectToAction(nameof(View), new { id });
            }
            model.DocumentCaseInfo.CourtId = userContext.CourtId;
            SetViewBag_ConvertToCompliant();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConvertToCompliant(DocumentVM model)
        {
            Validate_ConvertToCompliant(model);
            if (!ModelState.IsValid)
            {
                SetViewBag_ConvertToCompliant();
                return View(model);
            }
            if (await docService.Document_SaveCommonToCompliant(model))
            {
                SetAuditContext(docService, SourceTypeSelectVM.Document, model.Id, false);
                SetSuccessMessage("Избраният документ е променен като съпровождащ.");
                this.SaveLogOperation(false, model.Id, null, "edit");

                return RedirectToAction(nameof(View), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
                SetViewBag_ConvertToCompliant();
                return View(model);
            }
        }

        void Validate_ConvertToCompliant(DocumentVM model)
        {
            ValidateModel_CaseInfo(model);

            if (!model.DocumentCaseInfo.HasLawAct)
            {
                model.DocumentCaseInfo.SessionActId = null;
            }

            if (model.DocumentGroupId == NomenclatureConstants.DocumentGroup.DocumentForComplain_AccompanyingDocument)
            {
                if (((model.DocumentCaseInfo.SessionActId <= 0) || (!model.DocumentCaseInfo.HasLawAct)) && !NomenclatureConstants.DocumentType.ComplainDocsWithoutAct.Contains(model.DocumentTypeId ?? 0))
                {
                    ModelState.AddModelError("DocumentCaseInfo.HasLawAct", "Изберете съдебен акт.");
                }
            }
        }

        void SetViewBag_ConvertToCompliant()
        {
            ViewBag.DocumentGroupId_ddl = nomService.GetDDL_DocumentGroupByCourt(DocumentConstants.DocumentKind.CompliantDocument, null);
        }

        public IActionResult NewElectronicDocuments()
        {
            return View();
        }

        [HttpPost]
        public IActionResult NewElectronicDocuments_ListData(IDataTablesRequest request)
        {
            var data = docService.GetElectronicDocumentNew();
            return request.GetResponse(data);
        }

    }
}