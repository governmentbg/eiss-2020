using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rotativa.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class CaseController : BaseController
    {
        private readonly ICaseService service;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly ICaseClassificationService classficationService;
        private readonly ICaseSelectionProtokolService caseSelectProtokolService;
        private readonly ICourtDepartmentService courtDepartmentService;
        private readonly IDocumentResolutionService docResolutionService;
        private readonly IDocumentService documentService;

        public CaseController(
            ICaseService _service,
            INomenclatureService _nomService,
            ICommonService _commonService,
            ICaseClassificationService _classficationService,
            ICaseSelectionProtokolService _caseSelectProtokolService,
            ICourtDepartmentService _courtDepartmentService,
            IDocumentResolutionService _docResolutionService,
            IDocumentService _documentService)
        {
            service = _service;
            nomService = _nomService;
            commonService = _commonService;
            classficationService = _classficationService;
            caseSelectProtokolService = _caseSelectProtokolService;
            courtDepartmentService = _courtDepartmentService;
            docResolutionService = _docResolutionService;
            documentService = _documentService;
        }

        /// <summary>
        /// Страница за дела по критерии от филтър
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.Case, null, AuditConstants.Operations.View))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }
            CurrentContext_SetObjectInfo("Търсене в списъчен екран Обща информация за дела");
            CaseFilter filter = new CaseFilter()
            {
                CaseYear = DateTime.Now.Year
            };
            await SetViewbagIndex();
            SetHelpFile(HelpFileValues.CaseIndex);
            return View(filter);
        }

        /// <summary>
        /// Дела с непълен състав
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CaseForSelection()
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.Case, null, AuditConstants.Operations.View))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }
            CurrentContext_SetObjectInfo("Търсене в списъчен екран Дела с непълен съдебен състав");
            CaseFilter filter = new CaseFilter();
            await SetViewbagIndex();
            SetHelpFile(HelpFileValues.IncompleteConstitutionInfo);
            return View(filter);
        }

        /// <summary>
        /// Извличане на информация за дела по филтър
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, CaseFilter model)
        {
            var data = service.Case_Select(model);
            return request.GetResponse(data);
        }

        [HttpPost]
        public async Task<IActionResult> CaseFastSearch(CaseFastSearchVM model)
        {
            var data = await service.Case_Select(new CaseFilter()
            {
                CaseGroupIds_text = model.CaseGroupId.ToString(),
                RegNumber = model.RegNumber.EmptyToNull() ?? "-666",
                CaseYear = model.CaseYear
            }).FirstOrDefaultAsync();

            if (data == null)
            {
                return Json(new { found = false, error = "Не съществува дело по подадените данни!" });
            }

            if (!await CheckAccessAsync(service, SourceTypeSelectVM.Case, data.Id, AuditConstants.Operations.View))
            {
                return Json(new { found = false, error = "Нямате достъп до търсеното дело!" });
            }

            return Json(new { found = true, id = data.Id });
        }

        /// <summary>
        /// Извличане на информация за дела с непълен състав
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns> 
        [HttpPost]
        public IActionResult ListDataForSelection(IDataTablesRequest request, CaseFilter model)
        {
            var data = service.Case_SelectForSelection(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Редакция на дело
        /// </summary>
        /// <param name="id"></param>
        /// <param name="taskid"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id, long? taskid = null)
        {
            var model = await service.Case_SelectForEdit(id);
            if (model == null)
            {
                return Redirect_Denied("Търсения от Вас ресурс е невалиден или недостъпен в момента.");
            }
            if (!string.IsNullOrEmpty(model.RegNumber))
            {
                if (!await CheckAccessAsync(service, SourceTypeSelectVM.Case, id, AuditConstants.Operations.Update))
                {
                    return Redirect_Denied();
                }
            }
            else
            {
                if (!await CheckAccessAsync(service, SourceTypeSelectVM.Case, id, AuditConstants.Operations.View))
                {
                    return Redirect_Denied();
                }

                var document = await service.GetByIdAsync<Document>(model.DocumentId);
                if (document.IsOldNumber == true)
                {
                    model.IsOldNumber = true;
                }
                model.Description = document.Description;
            }

            int newCaseStateId = NomenclatureConstants.CaseState.New;
            if (taskid > 0)
            {
                var _taskType = await service.GetPropByIdAsync<Infrastructure.Data.Models.Common.WorkTask, int>(x => x.Id == taskid, x => x.TaskTypeId);
                if (_taskType == WorkTaskConstants.Types.Case_ForReject)
                {
                    newCaseStateId = NomenclatureConstants.CaseState.Rejected;
                }
            }

            //Ако не е образувано делото (чернова) - се предлага по подразбиране Образувано
            if (model.CaseStateId == NomenclatureConstants.CaseState.Draft && string.IsNullOrEmpty(model.RegNumber))
            {
                model.CaseStateId = newCaseStateId;
            }

            await SetViewBagEdit(model);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Edit(CaseEditVM model)
        {
            await SetViewBagEdit(model);

            ValidateCaseEdit(model);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }

            var currentId = model.Id;
            var isInsert = string.IsNullOrEmpty(model.RegNumber);
            var saveResult = await service.Case_SaveData(model);
            if (saveResult.Result)
            {
                if (isInsert && !string.IsNullOrEmpty(model.RegNumber))
                {
                    await CheckAccessAsync(service, SourceTypeSelectVM.Case, model.Id, AuditConstants.Operations.Init);
                }
                else
                {
                    await SetAuditContextAsync(service, SourceTypeSelectVM.Case, model.Id, currentId == 0);
                }
                this.SaveLogOperation(isInsert, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                if (isInsert)
                {
                    return RedirectToAction(nameof(Edit), new { id = model.Id });
                }
                else
                {
                    return RedirectToAction(nameof(CasePreview), new { id = model.Id });
                }
            }
            else
            {

                if (saveResult.ReloadNeeded)
                {
                    SetErrorMessage(MessageConstant.Values.NewerDateWrt);
                    return RedirectToAction(nameof(Edit), new { id = model.Id });
                }
                if (!string.IsNullOrEmpty(saveResult.ErrorMessage))
                {
                    SetErrorMessage(saveResult.ErrorMessage);
                }
                else
                {
                    SetErrorMessage(MessageConstant.Values.SaveFailed);
                }
            }
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Валидация преди запис
        /// </summary>
        /// <param name="model"></param>
        private void ValidateCaseEdit(CaseEditVM model)
        {
            if (model.IsOldNumber == true && model.OldDate.HasValue)
            {
                var caseGroupId = nomService.GetPropById<CaseType, int>(model.CaseTypeId, x => x.CaseGroupId);
                if (!service.CheckCaseOldNumber(caseGroupId, model.OldNumber, model.OldDate.Value))
                {
                    ModelState.AddModelError("OldNumber", $"Вече съществува дело с номер {model.OldNumber} от дата {model.OldDate:dd.MM.yyyy}");
                }
            }

            if (NomenclatureConstants.CaseState.UnregisteredManageble.Contains(model.CaseStateId))
            {
                //Ако делото е отказано от образуване - следните полета не са задължителни:
                model.CaseCodeId = 0;
                model.CourtGroupId = null;
                model.LoadGroupLinkId = null;
                model.CaseTypeUnitId = null;
                model.ProcessPriorityId = null;

                if (string.IsNullOrEmpty(model.CaseStateDescription))
                {
                    ModelState.AddModelError("CaseStateDescription", $"Полето 'Основание' е задължително при отказ от образуване.");
                }
            }
            else
            {
                if (model.CaseTypeId <= 0)
                    ModelState.AddModelError("CaseTypeId", $"Изберете 'Точен вид дело'.");

                if (model.CaseCharacterId <= 0)
                    ModelState.AddModelError("CaseCharacterId", $"Изберете 'Характер на дело'.");

                if (model.CaseCodeId <= 0)
                {
                    ModelState.AddModelError("CaseCodeId", $"Изберете 'Шифър'.");
                }

                if ((model.CourtGroupId ?? 0) <= 0)
                    ModelState.AddModelError("CourtGroupId", $"Изберете 'Съдебна група за разпределяне'.");

                if (model.CourtTypeId != NomenclatureConstants.CourtType.VKS && (model.LoadGroupLinkId ?? 0) <= 0)
                    ModelState.AddModelError("LoadGroupLinkId", $"Изберете 'Група по натовареност'.");

                if ((model.CaseTypeUnitId ?? 0) <= 0)
                    ModelState.AddModelError("CaseTypeUnitId", $"Изберете 'Състав по делото'.");

                if ((model.ProcessPriorityId ?? 0) <= 0)
                    ModelState.AddModelError("ProcessPriorityId", $"Изберете 'Вид производство'.");
            }

            if (model.CaseStateId == NomenclatureConstants.CaseState.Deleted)
            {
                if (string.IsNullOrEmpty(model.CaseStateDescription))
                    ModelState.AddModelError("CaseStateDescription", $"Делото е анулирано и е задължително да въведете основание.");
            }

            if (model.CaseInforcedDate != null)
            {
                if (model.Id > 0)
                {
                    var @caseRegDate = service.GetPropById<Case, DateTime>(model.Id, x => x.RegDate);
                    if (model.CaseInforcedDate != null)
                    {
                        if ((model.CaseInforcedDate ?? DateTime.Now).Date < @caseRegDate.Date)
                        {
                            ModelState.AddModelError("CaseInforcedDate", $"Дата на влизане в сила е по-малка от дата на регистрация на делото.");
                        }
                    }
                }
            }

            if (model.CaseStateId == NomenclatureConstants.CaseState.ComingIntoForce)
            {
                if (model.CaseInforcedDate == null)
                {
                    ModelState.AddModelError("CaseInforcedDate", $"Трябва да въведете дата на влизане в законна сила");
                }
            }

            if (model.CaseInforcedDate != null)
            {
                if ((model.CaseInforcedDate ?? DateTime.Now).Date > DateTime.Now.Date)
                {
                    ModelState.AddModelError("CaseInforcedDate", $"Не може да има бъдеща дата за влизане в законна сила");
                }
            }

            //Проверка за характер и точен вид дело да съвпадат
            var charactersByCaseType = nomService.GetDDL_CaseCharacter(model.CaseTypeId, null);
            if (!charactersByCaseType.Any(x => x.Value == model.CaseCharacterId.ToString()))
            {
                ModelState.AddModelError("CaseCharacterId", $"Избрания характер не е приложим за точния вид дело!");
            }

            var currentCaseNumber = nomService.GetPropById<Case, string>(x => x.Id == model.Id, x => x.RegNumber);
            if (string.IsNullOrEmpty(currentCaseNumber))
            {
                if (commonService.CheckCourtRestriction(NomenclatureConstants.CourtRestrictionTypes.DisableInitCase))
                {
                    ModelState.AddModelError("", $"Не можете да образувате нови дела в {userContext.CourtName}.");
                }
            }
        }

        /// <summary>
        /// Зареждане на пълна информация за дело с табове
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> CasePreview(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.Case, id, AuditConstants.Operations.View))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }
            CurrentContext_SetObjectInfo("Преглед на основни данни");

            var model = await service.Case_GetById(id);
            if (model == null)
            {
                return NotFoundError("Търсеното от Вас дело не е намерено и/или нямате достъп до него.");
            }
            if (string.IsNullOrEmpty(model.RegNumber))
            {
                return RedirectToAction(nameof(Edit), new { id });
            }
            await SetViewbag(id);
            ViewBag.isAutorized = this.CurrentContext.CanAccess;
            ViewBag.isFastProcess = nomService.CaseCodeGroup_Check(NomenclatureConstants.CaseCodeGroupAlias.CaseFastProcess, model.CaseCodeId ?? 0);
            ViewBag.isMediationFeature = userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.Mediation);
            if (model.ElectronicDocumentId > 0)
            {
                ViewBag.elDocInfo = await documentService.GetElectronicDocumentInfo(model.ElectronicDocumentId.Value);
            }
            SetHelpFile(HelpFileValues.CaseMainData);
            return View(nameof(CasePreview), model);
        }

        public async Task<IActionResult> Get_HasCaseCompetence(int caseType, int courtType)
        {
            var result = await service.CheckCaseFeature(new CaseFeatureInfoVM()
            {
                CourtTypeId = courtType,
                CaseTypeId = caseType
            }, NomenclatureConstants.CaseFeatures.ISPN_HasCaseCompetence);
            return Json(new SaveResultVM(result));
        }

        /// <summary>
        /// Старата електронна папка
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> CaseFolder(int id)
        {
            var model = await service.Case_GetById(id);
            if (model == null)
            {
                return NotFoundError("Търсеното от Вас дело не е намерено и/или нямате достъп до него.");
            }
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.Case, id, AuditConstants.Operations.View))
            {
                return Redirect_Denied();
            }
            CurrentContext_SetObjectInfo("Преглед на електронна папка");
            return View(model);
        }

        /// <summary>
        /// Електронна папка за дело
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> CaseTimeLinePreview(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.Case, id, AuditConstants.Operations.View))
            {
                return Redirect_Denied();
            }
            var model = await service.CaseElectronicFolder_Select(id);
            if (model == null)
            {
                return NotFoundError("Търсеното от Вас дело не е намерено и/или нямате достъп до него.");
            }
            await SetViewbag(id);
            SetHelpFile(HelpFileValues.CaseFolder);
            return View(nameof(CaseTimeLinePreview), model);
        }

        /// <summary>
        /// Извличане на данни за старата ел. папка
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CaseFolder_ListData(IDataTablesRequest request, int caseId)
        {
            var data = await service.Case_SelectFolder(caseId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на номенклатури за страници
        /// </summary>
        async Task SetViewbagIndex()
        {
            ViewBag.CaseGroupIds_ddl = await nomService.GetDropDownListAsync<CaseGroup>(false);
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.CaseClassificationId_ddl = await nomService.GetDropDownListAsync<Classification>();
            ViewBag.CaseStateId_ddl = await nomService.GetDropDownListAsync<CaseState>();

            ViewBag.CourtDepartmentId_ddl = await courtDepartmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
            ViewBag.CourtDepartmentOtdelenieId_ddl = await courtDepartmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Otdelenie);

            ViewBag.FirstInstanceCourtId_ddl = await nomService.GetDropDownListAsync<Court>();
            ViewBag.CourtId_ddl = await nomService.GetDropDownListAsync<Court>();
        }

        async Task SetViewbag(int id)
        {
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.CaseClassification_ddl = await classficationService.CaseClassification_Select(id, null);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(id);
        }

        async Task SetViewBagEdit(CaseEditVM model)
        {
            int? caseCharacter = null;
            if (!string.IsNullOrEmpty(model.RegNumber))
            {
                caseCharacter = model.CaseCharacterId;
            }
            ViewBag.CaseTypeId_ddl = nomService.GetDDL_CaseTypeByDocType(model.DocumentTypeId, caseCharacter);
            ViewBag.CaseProtokolLawUnit_Count = caseSelectProtokolService.CaseSelectionProtokolLawUnit_SelectCount(model.Id);
            bool isInitial = string.IsNullOrEmpty(model.RegNumber);
            var states = await nomService.GetDDL_CaseState(isInitial, !isInitial);
            if (model.CaseStateId == NomenclatureConstants.CaseState.Archive)
            {
                states.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem("Архивирано", NomenclatureConstants.CaseState.Archive.ToString()));
            }
            if (NomenclatureConstants.CaseState.UnregisteredManageble.Contains(model.CaseStateId))
            {
                ViewBag.CaseStateId_ddl = states.Where(x => x.Value != NomenclatureConstants.CaseState.New.ToString()).ToList();
            }
            else
            {
                ViewBag.CaseStateId_ddl = states;
            }
            ViewBag.lastMigration = await service.Case_GetPriorCase(model.DocumentId);
            ViewBag.eisppMigration = service.Case_GetPriorCaseEISPP(model.DocumentId, model.EISSPNumber);
            ViewBag.ProcessPriorityId_ddl = await nomService.GetDropDownListAsync<ProcessPriority>();
            ViewBag.IspnCaseCompetenceId_ddl = await nomService.GetDropDownListAsync<IspnCaseCompetence>();
            if (model.IspnKind == NomenclatureConstants.IspnKinds.Rnfl)
            {
                ViewBag.RnflProcessTypeId_ddl = await nomService.GetDropDownListAsync<RnflProcessType>();
            }
            //ViewBag.CaseReasonId_ddl = nomService.GetDropDownList<CaseReason>();
            ViewBag.ComplexIndexActual_ddl = nomService.GetDDL_ComplexIndex();
            ViewBag.ComplexIndexLegal_ddl = nomService.GetDDL_ComplexIndex();
            var savedCase = await service.GetByIdAsync<Case>(model.Id);
            ViewBag.savedCaseStateId = savedCase.CaseStateId;
            if (NomenclatureConstants.CaseState.UnregisteredManageble.Contains(savedCase.CaseStateId))
            {
                ViewBag.unregisteredManagebleCase = true;
                var docResolution = await docResolutionService.Select(savedCase.DocumentId).FirstOrDefaultAsync();
                if (docResolution != null)
                {
                    ViewBag.hasDocResolution = true;
                    ViewBag.docResolutionId = docResolution.Id;
                }
                else
                {
                    ViewBag.hasDocResolution = false;
                }
            }

            if (!string.IsNullOrEmpty(model.RegNumber))
            {
                ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(model.Id);
            }

            if (string.IsNullOrEmpty(model.RegNumber))
                SetHelpFile(HelpFileValues.Assignment);
            else
                SetHelpFile(HelpFileValues.CaseEdit);
        }

        #region Разширено търсене на дела

        /// <summary>
        /// Разширено търсене на дела
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CaseReport()
        {
            CaseFilterReport filter = new CaseFilterReport()
            {
                CaseCodeId = -1,
                CaseGroupId = -1,
                CaseTypeId = -1,
                //DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                //DateTo = new DateTime(DateTime.Now.Year, 12, 31)
            };
            await SetViewbagReport();
            SetHelpFile(HelpFileValues.Search);
            AddAuditInfo(AuditConstants.Operations.List, "Преглед на форма за разширено търсене на дела");
            return View("CaseReport", filter);
        }

        async Task SetViewbagReport()
        {
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.CaseStateId_ddl = await nomService.GetDropDownListAsync<CaseState>(false, true); //nomService.GetDDL_CaseStateHand(false, true);
            ViewBag.LinkDelo_CourtId_ddl = nomService.GetCourts();
            ViewBag.CaseClassificationId_ddl = await nomService.GetDropDownListAsync<Classification>();
            ViewBag.ProcessPriorityId_ddl = await nomService.GetDropDownListAsync<ProcessPriority>();

            //Заседания
            ViewBag.CourtHallId_ddl = await commonService.GetDropDownList_CourtHall(userContext.CourtId, true);
            ViewBag.SessionTypeId_ddl = await nomService.GetDropDownListAsync<SessionType>();
            ViewBag.SessionStateId_ddl = await nomService.GetDropDownListAsync<SessionState>();
            ViewBag.SessionResultId_ddl = await nomService.GetDDL_SessionResultAsync();

            //Актове
            ViewBag.ActTypeId_ddl = await nomService.GetDropDownListAsync<ActType>();

            //Свързани дела
            ViewBag.Institution_InstitutionTypeId_ddl = await nomService.GetDropDownListAsync<InstitutionType>();
            ViewBag.HasRegNumberOtherSystem = false;
            if (userContext.CourtInstances.Contains(NomenclatureConstants.CaseInstanceType.SecondInstance) ||
                 userContext.CourtInstances.Contains(NomenclatureConstants.CaseInstanceType.ThirdInstance))
            {
                ViewBag.HasRegNumberOtherSystem = true;
            }

            // Съдебен състав
            ViewBag.CourtDepartmentId_ddl = await courtDepartmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
        }

        /// <summary>
        /// Извличане на данни по филтър
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filterJson"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseReport_ListData(IDataTablesRequest request, [AllowHtml] string filterJson)
        {
            var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
            CaseFilterReport model = JsonConvert.DeserializeObject<CaseFilterReport>(filterJson, dateTimeConverter);

            if (model.VisibleOtherSystemHidden)
            {
                model.LinkDelo_CourtId = null;
                model.LinkDelo_RegNumber = string.Empty;
                model.LinkDelo_Description = string.Empty;
            }
            else
            {
                model.CourtOtherSystem = null;
                model.YearOtherSystem = null;
                model.RegNumberOtherSystem = string.Empty;
            }

            var data = service.CaseReport_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        #endregion

        /// <summary>
        /// Извличане на архив на ел. папка
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public async Task<IActionResult> CaseArchive(int CaseId)
        {
            var caseCase = await service.GetCaseInfo(CaseId);
            var nameDelo = "Дело_" + caseCase.RegNumber + "_" + caseCase.RegDate.Day.ToString("00") + "_" + caseCase.RegDate.Month.ToString("00") + "_" + caseCase.RegDate.Year.ToString("0000") + ".zip";
            var caseArchive = await service.CaseArchive(CaseId);
            return File(caseArchive, System.Net.Mime.MediaTypeNames.Application.Zip, nameDelo);
        }

        #region Образувани дела с участието на малолетни/непълнолетни лица

        /// <summary>
        /// Справка за образувани дела с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> IndexReportMaturity()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за образувани дела с участието на малолетни/непълнолетни лица");
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
            };
            await SetViewbagIndex();
            SetHelpFile(HelpFileValues.Report3);

            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за Образувани дела с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataReportMaturity(IDataTablesRequest request, CaseFilterReport model)
        {
            var data = service.CaseReportMaturity_Select(model);
            return request.GetResponse(data);
        }

        #endregion

        #region Справка Дела с ненаписани съдебни актове от всички съдии

        /// <summary>
        /// Справка дела с ненаписани съдебни актове от всички съдии
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> IndexCaseWithoutFinalAct()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка дела с ненаписани съдебни актове от всички съдии");
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
            };
            await SetViewbagIndex();
            SetHelpFile(HelpFileValues.Report6);
            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за Дела с ненаписани съдебни актове от всички съдии
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseWithoutFinalAct(IDataTablesRequest request, CaseFilterReport model)
        {
            var data = service.CaseWithoutFinalAct_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        #endregion

        #region Образувани и свършени дела за корупционни престъпления

        /// <summary>
        /// Справка образувани и свършени дела за коруп. престъпления
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public IActionResult IndexCaseCorruptCrimes()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка образувани и свършени дела за коруп. престъпления");
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
            };
            SetHelpFile(HelpFileValues.Report7);
            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за Oбразувани и свършени дела за коруп. престъпления
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseCorruptCrimes(IDataTablesRequest request, CaseFilterReport filter)
        {
            var data = service.CaseCorruptCrimes_Select(filter);
            return request.GetResponse(data);
        }

        #endregion

        #region Несвършени дела с участието на малолетни/непълнолетни лица

        /// <summary>
        /// Справка Несвършени дела с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> IndexWithoutCaseFinalActMaturity()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за несвършени дела с участието на малолетни/непълнолетни лица");
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
                DateToSpr = DateTime.Now,
                WithFinalAct = false
            };
            await SetViewbagIndex();
            SetHelpFile(HelpFileValues.Report5);
            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за Несвършени дела с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataWithoutCaseFinalActMaturity(IDataTablesRequest request, CaseFilterReport filter)
        {
            var data = service.CaseFinalActMaturity_Select(filter);
            return request.GetResponse(data);
        }

        #endregion

        #region Свършили дела с участието на малолетни/непълнолетни лица

        /// <summary>
        /// Справка Свършили дела с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> IndexWithCaseFinalActMaturity()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка Свършили дела с участието на малолетни/непълнолетни лица");
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
                ActDeclaredDateFrom = NomenclatureExtensions.GetStartYear(),
                ActDeclaredDateTo = NomenclatureExtensions.GetEndYear(),
                WithFinalAct = true
            };
            await SetViewbagIndex();
            SetHelpFile(HelpFileValues.Report4);
            return View(filter);
        }

        /// <summary>
        /// Извличане на данни Свършили дела с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataWithCaseFinalActMaturity(IDataTablesRequest request, CaseFilterReport filter)
        {
            var data = service.CaseFinalActMaturity_Select(filter);
            return request.GetResponse(data);
        }

        #endregion

        #region Справка несвършени дела към дата

        /// <summary>
        /// Справка не свършени дела към дата
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> IndexCaseWithoutFinal()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка не свършени дела към дата");
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
                DateToSpr = DateTime.Now
            };
            await SetViewbagIndex();
            SetHelpFile(HelpFileValues.Report1);
            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за Справка Несвършени дела към дата
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseWithoutFinal(IDataTablesRequest request, CaseFilterReport filter)
        {
            var data = service.CaseWithoutFinal_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Експорт Справка Несвършени дела към дата
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseWithoutFinalExportExcel(CaseFilterReport model)
        {
            var xlsBytes = service.CaseWithoutFinalExportExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Справка несвършени дела към дата

        /// <summary>
        /// Справка не свършени дела към дата
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> IndexCaseWithoutFinalWithCourt()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка не свършени дела към дата");
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = DateTime.Now.AddMonths(-1),
                DateTo = DateTime.Now,
                DateToSpr = DateTime.Now
            };
            await SetViewbagIndex();
            SetHelpFile(HelpFileValues.Report1);
            return View(filter);
        }

        /// <summary>
        /// Експорт Справка Несвършени дела към дата
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseWithoutFinalWithCourtExportExcel(CaseFilterReport model)
        {
            var xlsBytes = service.CaseWithoutFinalWithCourtExportExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Справка за предоставени/върнати документи

        /// <summary>
        /// Справка за предоставени/върнати документи
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public IActionResult IndexDocumentProvidedReturned()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за предоставени/върнати документи");
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
            };
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.DocumentGroupId_ddl = nomService.GetDDL_DocumentGroupWithKind();
            SetHelpFile(HelpFileValues.Report30);

            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за Справка за предоставени/върнати документи
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataDocumentProvidedReturned(IDataTablesRequest request, CaseFilterReport model)
        {
            var data = service.DocumentProvidedReturned_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        #endregion

        #region Справка за срочност за насрочване на дела

        /// <summary>
        /// Справка за срочност за насрочване на дела
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public IActionResult IndexCaseBeginReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за срочност за насрочване на дела");
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
            };
            ViewBagCaseBeginReport();
            SetHelpFile(HelpFileValues.Report9);
            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за Справка за срочност за насрочване на дела
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseBeginReport(IDataTablesRequest request, CaseFilterReport filter)
        {
            var data = service.CaseBeginReport_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на номенклатури за справка за срочност за насрочване на дела
        /// </summary>
        private void ViewBagCaseBeginReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.SessionTypeId_ddl = nomService.GetDDL_SessionTypeWithoutClosedSession();
            ViewBag.SessionDateToId_ddl = nomService.GetDDL_SessionToDate();
        }

        #endregion

        #region Справка за срочност за изготвяне на съдебен акт

        /// <summary>
        /// Справка за срочност за изготвяне на съдебен акт
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> IndexCaseActReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за срочност за изготвяне на съдебен акт");
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
                Session_DateFrom = NomenclatureExtensions.GetStartYear(),
                SessionDateTo = NomenclatureExtensions.GetEndYear(),
                ActDateFrom = NomenclatureExtensions.GetStartYear(),
                ActDateTo = NomenclatureExtensions.GetEndYear(),
            };

            DateTime dateFrom = new DateTime(2023, 11, 1);
            DateTime dateTo = new DateTime(2023, 12, 1);
            int months = await commonService.GetMonthsBetweenTwoDatesForActs(dateFrom, dateTo);

            ViewBagCaseActReport();
            SetHelpFile(HelpFileValues.Report22);

            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за справка за срочност за изготвяне на съдебен акт
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseActReport(IDataTablesRequest request, CaseFilterReport model)
        {
            var data = service.CaseActReport_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Метод зареждащ номенклатури за справка за срочност за изготвяне на съдебен акт
        /// </summary>
        private void ViewBagCaseActReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.SessionTypeId_ddl = nomService.GetDropDownList<SessionType>();
            ViewBag.SessionResultId_ddl = nomService.GetDDL_SessionResult();
            ViewBag.ActDateToIds_ddl = nomService.GetDDL_ActToDate(false);
            ViewBag.ActMotiveDateToId_ddl = nomService.GetDDL_ActMotiveToDate();
        }

        #endregion

        #region Справка за времетраене на размяната на книжата (първи интервал)

        /// <summary>
        /// Справка за времетраене на размяната на книжата (първи интервал)
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public IActionResult IndexCaseFirstLifecyclie()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за времетраене на размяната на книжата (първи интервал)");
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
                Session_DateFrom = NomenclatureExtensions.GetStartYear(),
                SessionDateTo = NomenclatureExtensions.GetEndYear(),
            };
            ViewBagCaseFirstLifecyclie();
            SetHelpFile(HelpFileValues.Report10);
            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за Справка за времетраене на размяната на книжата (първи интервал)
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseFirstLifecyclie(IDataTablesRequest request, CaseFilterReport filter)
        {
            filter.IsDoubleExchangeDoc = false;
            var data = service.CaseFirstLifecyclie_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на номенклатури за справка за времетраене на размяната на книжата (първи интервал)
        /// </summary>
        private void ViewBagCaseFirstLifecyclie()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>().Where(x => x.Value != NomenclatureConstants.CaseGroups.NakazatelnoDelo.ToString()).ToList();
            ViewBag.ProcessPriorityId_ddl = nomService.GetDropDownList<ProcessPriority>();
        }

        #endregion

        #region Справка за времетраене на размяната на книжата (двойна размяна)

        /// <summary>
        /// Справка за времетраене на размяната на книжата (двойна размяна)
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public IActionResult IndexCaseFirstLifecyclieDoubleExchange()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за времетраене на размяната на книжата (двойна размяна)");
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
                Session_DateFrom = NomenclatureExtensions.GetStartYear(),
                SessionDateTo = NomenclatureExtensions.GetEndYear(),
            };
            ViewBagCaseFirstLifecyclieDoubleExchange();
            SetHelpFile(HelpFileValues.Report11);
            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за Справка за времетраене на размяната на книжата (двойна размяна)
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseFirstLifecyclieDoubleExchange(IDataTablesRequest request, CaseFilterReport model)
        {
            model.IsDoubleExchangeDoc = true;
            var data = service.CaseFirstLifecyclie_Select(model);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на номенклатури за справка за времетраене на размяната на книжата (двойна размяна)
        /// </summary>
        private void ViewBagCaseFirstLifecyclieDoubleExchange()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>().Where(x => x.Value != NomenclatureConstants.CaseGroups.NakazatelnoDelo.ToString()).ToList();
            ViewBag.ProcessPriorityId_ddl = nomService.GetDropDownList<ProcessPriority>();
        }

        #endregion

        public async Task<IActionResult> CaseProceedings(int id)
        {
            var model = await service.CaseProceedings_Select(id);
            model.IsViewBtn = false;
            string html = await this.RenderPartialViewAsync("~/Views/Case/", "CaseProceedings.cshtml", model, true);
            var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }).GetByte(this.ControllerContext);
            return File(pdfBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, "CaseProceedings" + id.ToString() + ".pdf");
        }

        public IActionResult GetDepersonalizationHistory(int caseId)
        {
            return Json(service.GetDepersonalizationHistory(caseId));
        }

        public async Task<IActionResult> CaseProceedingsView(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.Case, id, AuditConstants.Operations.View))
            {
                return Redirect_Denied();
            }
            CurrentContext_SetObjectInfo("Преглед на ход на дело");
            var model = await service.CaseProceedings_Select(id);
            model.IsViewBtn = true;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(id);
            return View("CaseProceedings", model);
        }

        #region Обединяване на дела - бързо производство

        /// <summary>
        /// Стартиране на екран за обединяване на дела за бързопроизводство
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<IActionResult> MergerCasesFastProcess(int caseId)
        {
            MergerCaseFastProcessVM model = await service.GetMergerCaseFastProcess(caseId);
            return View(nameof(MergerCasesFastProcess), model);
        }

        /// <summary>
        /// Метод връщащ инфо за дело за обединяване на дела за бързо производство
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> GetFromCaseInfoForMergerCaseFastProcess(int caseId)
        {
            return Json(new { result = await service.GetFromCaseInfoForMergerCaseFastProcess(caseId) });
        }

        /// <summary>
        /// Валидация при запис
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidMergerCasesFastProcess(MergerCaseFastProcessVM model)
        {
            if ((model.FromCaseId ?? 0) < 1)
                return "Изберете дело";

            return string.Empty;
        }


        [HttpPost]
        public async Task<IActionResult> MergerCasesFastProcess(MergerCaseFastProcessVM model)
        {
            if (!ModelState.IsValid)
                return View(nameof(MergerCasesFastProcess), model);

            string _isvalid = IsValidMergerCasesFastProcess(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(MergerCasesFastProcess), model);
            }

            if (await service.MergerCaseFastProcess(model))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction("Index", "CaseMigration", new { caseId = model.CaseId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
                return View(nameof(MergerCasesFastProcess), model);
            }
        }

        #endregion

        /// <summary>
        /// Извличане на данни за дела със същият състав, като в текущото дело
        /// </summary>
        /// <param name="caseId">Идентификатор на текущо дело</param>
        /// <returns></returns>
        public async Task<IActionResult> IndexExsistCaseWithSamePeople(int caseId)
        {
            Case caseCase = await service.GetByIdAsync<Case>(caseId);

            CurrentContext_SetObjectInfo("Извличане на данни за дела със същият състав, като в текущото дело");

            CaseFilter filter = new()
            {
                CaseId = caseId,
                RegDate = caseCase.RegDate,
                RegNumber = caseCase.RegNumber,
            };

            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за дела
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataExsistCaseWithSamePeople(IDataTablesRequest request, int caseId)
        {
            var data = service.GetCaseSimiliarCase(caseId);
            return request.GetResponse(data);
        }
    }
}