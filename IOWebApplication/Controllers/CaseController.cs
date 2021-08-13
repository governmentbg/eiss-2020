using System;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rotativa.Extensions;

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

        public CaseController(
            ICaseService _service,
            INomenclatureService _nomService,
            ICommonService _commonService,
            ICaseClassificationService _classficationService,
            ICaseSelectionProtokolService _caseSelectProtokolService,
            ICourtDepartmentService _courtDepartmentService,
            IDocumentResolutionService _docResolutionService)
        {
            service = _service;
            nomService = _nomService;
            commonService = _commonService;
            classficationService = _classficationService;
            caseSelectProtokolService = _caseSelectProtokolService;
            courtDepartmentService = _courtDepartmentService;
            docResolutionService = _docResolutionService;
        }

        /// <summary>
        /// Страница за дела по критерии от филтър
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            if (!CheckAccess(service, SourceTypeSelectVM.Case, null, AuditConstants.Operations.View))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }
            CaseFilter filter = new CaseFilter()
            {
                CaseYear = DateTime.Now.Year
            };
            SetViewbagIndex();
            SetHelpFile(HelpFileValues.CaseIndex);
            return View(filter);
        }

        /// <summary>
        /// Дела с непълен състав
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseForSelection()
        {
            CaseFilter filter = new CaseFilter();
            SetViewbagIndex();
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
            var data = service.Case_Select(userContext.CourtId, model);
            return request.GetResponse(data);
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
        public IActionResult Edit(int id, long? taskid = null)
        {
            var model = service.Case_SelectForEdit(id);
            if (model == null)
            {
                return Redirect_Denied("Търсения от Вас ресурс е невалиден или недостъпен в момента.");
            }
            if (!string.IsNullOrEmpty(model.RegNumber))
            {
                if (!CheckAccess(service, SourceTypeSelectVM.Case, id, AuditConstants.Operations.Update))
                {
                    return Redirect_Denied();
                }
            }
            else
            {
                if (!CheckAccess(service, SourceTypeSelectVM.Case, id, AuditConstants.Operations.View))
                {
                    return Redirect_Denied();
                }

                var document = service.GetById<Document>(model.DocumentId);
                if (document.IsOldNumber == true)
                {
                    model.IsOldNumber = true;
                }
                model.Description = document.Description;
            }

            int newCaseStateId = NomenclatureConstants.CaseState.New;
            if(taskid > 0)
            {
                var _task= service.GetById<Infrastructure.Data.Models.Common.WorkTask>(taskid);
                if(_task != null && _task.TaskTypeId == WorkTaskConstants.Types.Case_ForReject)
                {
                    newCaseStateId = NomenclatureConstants.CaseState.Rejected;
                }
            }

            //Ако не е образувано делото (чернова) - се предлага по подразбиране Образувано
            if (model.CaseStateId == NomenclatureConstants.CaseState.Draft && string.IsNullOrEmpty(model.RegNumber))
            {
                model.CaseStateId = newCaseStateId;
            }

            SetViewBagEdit(model);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CaseEditVM model)
        {
            SetViewBagEdit(model);

            ValidateCaseEdit(model);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }

            var currentId = model.Id;
            var isInsert = string.IsNullOrEmpty(model.RegNumber);
            if (service.Case_SaveData(model))
            {
                if (isInsert && !string.IsNullOrEmpty(model.RegNumber))
                {
                    CheckAccess(service, SourceTypeSelectVM.Case, model.Id, AuditConstants.Operations.Init);
                }
                else
                {
                    SetAuditContext(service, SourceTypeSelectVM.Case, model.Id, currentId == 0);
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
                SetErrorMessage(MessageConstant.Values.SaveFailed);
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
                var caseGroup = nomService.GetById<CaseType>(model.CaseTypeId);
                if (!service.CheckCaseOldNumber(caseGroup.CaseGroupId, model.OldNumber, model.OldDate.Value))
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
                    var @case = service.GetById<Case>(model.Id);
                    if (model.CaseInforcedDate != null)
                    {
                        if ((model.CaseInforcedDate ?? DateTime.Now).Date < @case.RegDate.Date)
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
        }

        /// <summary>
        /// Зареждане на пълна информация за дело с табове
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult CasePreview(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.Case, id, AuditConstants.Operations.View))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }

            var model = service.Case_GetById(id);
            if (model == null)
            {
                throw new NotFoundException("Търсеното от Вас дело не е намерено и/или нямате достъп до него.");
            }
            if (string.IsNullOrEmpty(model.RegNumber))
            {
                return RedirectToAction(nameof(Edit), new { id });
            }
            SetViewbag(id);
            ViewBag.isAutorized = this.CurrentContext.CanAccess;
            ViewBag.isFastProcess = nomService.CaseCodeGroup_Check(NomenclatureConstants.CaseCodeGroupAlias.CaseFastProcess, model.CaseCodeId ?? 0);
            SetHelpFile(HelpFileValues.CaseMainData);
            return View(nameof(CasePreview), model);
        }

        /// <summary>
        /// Старата електронна папка
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult CaseFolder(int id)
        {
            var model = service.Case_GetById(id);
            if (model == null)
            {
                throw new NotFoundException("Търсеното от Вас дело не е намерено и/или нямате достъп до него.");
            }
            if (!CheckAccess(service, SourceTypeSelectVM.Case, id, AuditConstants.Operations.View))
            {
                return Redirect_Denied();
            }
            return View(model);
        }

        /// <summary>
        /// Електронна папка за дело
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult CaseTimeLinePreview(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.Case, id, AuditConstants.Operations.View))
            {
                return Redirect_Denied();
            }
            var model = service.CaseElectronicFolder_Select(id);
            if (model == null)
            {
                throw new NotFoundException("Търсеното от Вас дело не е намерено и/или нямате достъп до него.");
            }
            SetViewbag(id);
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
        public IActionResult CaseFolder_ListData(IDataTablesRequest request, int caseId)
        {
            var data = service.Case_SelectFolder(caseId);
            return request.GetResponse(data);
        }

        void SetViewbagIndex()
        {
            ViewBag.CaseGroupIds_ddl = nomService.GetDropDownList<CaseGroup>(false);
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.CaseClassificationId_ddl = nomService.GetDropDownList<Classification>();
            ViewBag.CaseStateId_ddl = nomService.GetDropDownList<CaseState>();

            ViewBag.CourtDepartmentId_ddl = courtDepartmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
            ViewBag.CourtDepartmentOtdelenieId_ddl = courtDepartmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Otdelenie);

        }

        void SetViewbag(int id)
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.CaseClassification_ddl = classficationService.CaseClassification_Select(id, null);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(id);
            //ViewBag.CaseTypeId_ddl = nomService.GetDropDownList<CaseType>();
            //ViewBag.CaseCodeId_ddl = nomService.GetDropDownList<CaseCode>();
        }

        void SetViewBagEdit(CaseEditVM model)
        {
            int? caseCharacter = null;
            if (!string.IsNullOrEmpty(model.RegNumber))
            {
                caseCharacter = model.CaseCharacterId;
            }
            ViewBag.CaseTypeId_ddl = nomService.GetDDL_CaseTypeByDocType(model.DocumentTypeId, caseCharacter);
            ViewBag.CaseProtokolLawUnit_Count = caseSelectProtokolService.CaseSelectionProtokolLawUnit_SelectCount(model.Id);
            bool isInitial = string.IsNullOrEmpty(model.RegNumber);
            var states = nomService.GetDDL_CaseState(isInitial, !isInitial);
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
            ViewBag.lastMigration = service.Case_GetPriorCase(model.DocumentId);
            ViewBag.eisppMigration = service.Case_GetPriorCaseEISPP(model.DocumentId, model.EISSPNumber);
            ViewBag.ProcessPriorityId_ddl = nomService.GetDropDownList<ProcessPriority>();
            //ViewBag.CaseReasonId_ddl = nomService.GetDropDownList<CaseReason>();
            ViewBag.ComplexIndexActual_ddl = nomService.GetDDL_ComplexIndex();
            ViewBag.ComplexIndexLegal_ddl = nomService.GetDDL_ComplexIndex();
            var savedCase = service.GetById<Case>(model.Id);
            ViewBag.savedCaseStateId = savedCase.CaseStateId;
            if (NomenclatureConstants.CaseState.UnregisteredManageble.Contains(savedCase.CaseStateId))
            {
                ViewBag.unregisteredManagebleCase = true;
                var docResolution = docResolutionService.Select(savedCase.DocumentId).FirstOrDefault();
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

        /// <summary>
        /// Разширено търсене на дела
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseReport()
        {
            CaseFilterReport filter = new CaseFilterReport()
            {
                CaseCodeId = -1,
                CaseGroupId = -1,
                CaseTypeId = -1,
                //DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                //DateTo = new DateTime(DateTime.Now.Year, 12, 31)
            };
            SetViewbagReport();
            return View("CaseReport", filter);
        }

        void SetViewbagReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.CaseStateId_ddl = nomService.GetDDL_CaseStateHand(false, true);
            ViewBag.LinkDelo_CourtId_ddl = nomService.GetCourts();
            ViewBag.CaseClassificationId_ddl = nomService.GetDropDownList<Classification>();

            //Заседания
            ViewBag.CourtHallId_ddl = commonService.GetDropDownList_CourtHall(userContext.CourtId);
            ViewBag.SessionTypeId_ddl = nomService.GetDropDownList<SessionType>();
            ViewBag.SessionStateId_ddl = nomService.GetDropDownList<SessionState>();
            ViewBag.SessionResultId_ddl = nomService.GetDropDownList<SessionResult>();

            //Актове
            ViewBag.ActTypeId_ddl = nomService.GetDropDownList<ActType>();

            //Свързани дела
            ViewBag.Institution_InstitutionTypeId_ddl = nomService.GetDropDownList<InstitutionType>();
            ViewBag.HasRegNumberOtherSystem = false;
            if (userContext.CourtInstances.Contains(NomenclatureConstants.CaseInstanceType.SecondInstance) ||
                 userContext.CourtInstances.Contains(NomenclatureConstants.CaseInstanceType.ThirdInstance))
            {
                ViewBag.HasRegNumberOtherSystem = true;
            }

            // Съдебен състав
            ViewBag.CourtDepartmentId_ddl = courtDepartmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
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

        /// <summary>
        /// Извличане на архив на ел. папка
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public async Task<IActionResult> CaseArchive(int CaseId)
        {
            var caseCase = service.Case_GetById(CaseId);
            var nameDelo = "Дело_" + caseCase.RegNumber + "_" + caseCase.RegDate.Day.ToString("00") + "_" + caseCase.RegDate.Month.ToString("00") + "_" + caseCase.RegDate.Year.ToString("0000") + ".zip";
            var caseArchive = await service.CaseArchive(CaseId);
            return File(caseArchive, System.Net.Mime.MediaTypeNames.Application.Zip, nameDelo);
        }

        /// <summary>
        /// Справка за Образувани дела с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexReportMaturity()
        {
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
            };
            SetViewbagIndex();
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
            var data = service.CaseReportMaturity_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Справка Дела с ненаписани съдебни актове от всички съдии
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexCaseWithoutFinalAct()
        {
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
            };
            SetViewbagIndex();
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


        /// <summary>
        /// Справка Oбразувани и свършени дела за коруп. престъпления
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexCaseCorruptCrimes()
        {
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
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseCorruptCrimes(IDataTablesRequest request, CaseFilterReport model)
        {
            var data = service.CaseCorruptCrimes_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Справка Несвършени дела с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexWithoutCaseFinalActMaturity()
        {
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
                DateToSpr = DateTime.Now
            };
            SetViewbagIndex();
            SetHelpFile(HelpFileValues.Report5);
            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за Несвършени дела с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataWithoutCaseFinalActMaturity(IDataTablesRequest request, CaseFilterReport model)
        {
            var data = service.CaseFinalActMaturity_Select(userContext.CourtId, false, model);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Справка Свършили дела с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexWithCaseFinalActMaturity()
        {
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
                ActDeclaredDateFrom = NomenclatureExtensions.GetStartYear(),
                ActDeclaredDateTo = NomenclatureExtensions.GetEndYear(),
            };
            SetViewbagIndex();
            SetHelpFile(HelpFileValues.Report4);
            return View(filter);
        }

        /// <summary>
        /// Извличане на данни Свършили дела с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataWithCaseFinalActMaturity(IDataTablesRequest request, CaseFilterReport model)
        {
            var data = service.CaseFinalActMaturity_Select(userContext.CourtId, true, model);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Справка Справка Несвършени дела към дата
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexCaseWithoutFinal()
        {
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
                DateToSpr = DateTime.Now
            };
            SetViewbagIndex();
            SetHelpFile(HelpFileValues.Report1);
            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за Справка Несвършени дела към дата
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseWithoutFinal(IDataTablesRequest request, CaseFilterReport model)
        {
            var data = service.CaseWithoutFinal_Select(userContext.CourtId, model);
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

        /// <summary>
        /// Справка за предоставени/върнати документи
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexDocumentProvidedReturned()
        {
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
            };
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.DocumentGroupId_ddl = nomService.GetDropDownList<DocumentGroup>();
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

        /// <summary>
        /// Справка за срочност за насрочване на дела
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexCaseBeginReport()
        {
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
            };
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.SessionTypeId_ddl = nomService.GetDDL_SessionTypeWithoutClosedSession();
            ViewBag.SessionDateToId_ddl = nomService.GetDDL_SessionToDate();
            SetHelpFile(HelpFileValues.Report9);

            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за Справка за срочност за насрочване на дела
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseBeginReport(IDataTablesRequest request, CaseFilterReport model)
        {
            var data = service.CaseBeginReport_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Справка за срочност за изготвяне на съдебен акт
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexCaseActReport()
        {
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
                Session_DateFrom = NomenclatureExtensions.GetStartYear(),
                SessionDateTo = NomenclatureExtensions.GetEndYear(),
                ActDateFrom = NomenclatureExtensions.GetStartYear(),
                ActDateTo = NomenclatureExtensions.GetEndYear(),
            };
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.SessionTypeId_ddl = nomService.GetDropDownList<SessionType>();
            ViewBag.SessionResultId_ddl = nomService.GetDropDownList<SessionResult>();
            ViewBag.ActDateToId_ddl = nomService.GetDDL_ActToDate();
            SetHelpFile(HelpFileValues.Report22);

            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за Справка за срочност за изготвяне на съдебен акт
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseActReport(IDataTablesRequest request, CaseFilterReport model)
        {
            var data = service.CaseActReport_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Справка за времетраене на размяната на книжата (първи интервал)
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexCaseFirstLifecyclie()
        {
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
                Session_DateFrom = NomenclatureExtensions.GetStartYear(),
                SessionDateTo = NomenclatureExtensions.GetEndYear(),
            };
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>().Where(x => x.Value != NomenclatureConstants.CaseGroups.NakazatelnoDelo.ToString()).ToList();
            ViewBag.ProcessPriorityId_ddl = nomService.GetDropDownList<ProcessPriority>();
            SetHelpFile(HelpFileValues.Report10);

            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за Справка за времетраене на размяната на книжата (първи интервал)
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseFirstLifecyclie(IDataTablesRequest request, CaseFilterReport model)
        {
            model.IsDoubleExchangeDoc = false;
            var data = service.CaseFirstLifecyclie_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Справка за времетраене на размяната на книжата (двойна размяна)
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexCaseFirstLifecyclieDoubleExchange()
        {
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
                Session_DateFrom = NomenclatureExtensions.GetStartYear(),
                SessionDateTo = NomenclatureExtensions.GetEndYear(),
            };
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>().Where(x => x.Value != NomenclatureConstants.CaseGroups.NakazatelnoDelo.ToString()).ToList();
            ViewBag.ProcessPriorityId_ddl = nomService.GetDropDownList<ProcessPriority>();
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
            var data = service.CaseFirstLifecyclie_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        public async Task<IActionResult> CaseProceedings(int id)
        {
            var model = service.CaseProceedings_Select(id);
            string html = await this.RenderPartialViewAsync("~/Views/Case/", "CaseProceedings.cshtml", model, true);
            var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }).GetByte(this.ControllerContext);
            return File(pdfBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, "CaseProceedings" + id.ToString() + ".pdf");
        }

        public IActionResult GetDepersonalizationHistory(int caseId)
        {
            return Json(service.GetDepersonalizationHistory(caseId));
        }
    }
}