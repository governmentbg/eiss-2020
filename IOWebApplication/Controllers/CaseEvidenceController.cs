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
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class CaseEvidenceController : BaseController
    {
        private readonly ICaseEvidenceService service;
        private readonly INomenclatureService nomService;
        private readonly IDocumentService documentService;
        private readonly ICaseSessionActService caseSessionActService;
        private readonly ICommonService commonService;

        public CaseEvidenceController(ICaseEvidenceService _service, 
                                      INomenclatureService _nomService, 
                                      ICaseSessionActService _caseSessionActService, 
                                      ICommonService _commonService, 
                                      IDocumentService _documentService)
        {
            service = _service;
            nomService = _nomService;
            caseSessionActService = _caseSessionActService;
            commonService = _commonService;
            documentService = _documentService;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            CaseEvidenceFilterVM filter = new CaseEvidenceFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = new DateTime(DateTime.Now.Year, 12, 31),
                RegNumber = string.Empty,
                CaseRegNumber = string.Empty
            };
            ViewBag.EvidenceTypeId_ddl = nomService.GetDropDownList<EvidenceType>();
            SetHelpFile(HelpFileValues.Evidence);
            return View(filter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CaseEvidenceId"></param>
        /// <returns></returns>
        public IActionResult IndexMovement(int CaseEvidenceId)
        {
            ViewBag.CaseEvidenceId = CaseEvidenceId;
            var caseEvidence = service.GetById<CaseEvidence>(CaseEvidenceId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(caseEvidence.CaseId);
            SetHelpFile(HelpFileValues.CaseEvidence);
            return View();
        }

        /// <summary>
        /// Изчитане на данните за доказателства към дело
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int caseId)
        {
            var data = service.CaseEvidence_Select(caseId, null, null, string.Empty, string.Empty, 0);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Метод за изчитане на доказателства по справка за доказателства
        /// </summary>
        /// <param name="request"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="RegNumber"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataSpr(IDataTablesRequest request, DateTime DateFrom, DateTime DateTo, string RegNumber, string CaseRegNumber, int EvidenceTypeId)
        {
            var data = service.CaseEvidence_Select(0, DateFrom, DateTo, (RegNumber??string.Empty), CaseRegNumber, EvidenceTypeId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на доказателство към дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IActionResult Add(int caseId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseEvidence, null, AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }
            SetViewbag(caseId);
            var caseCase = service.GetById<Case>(caseId);
            var model = new CaseEvidence()
            {
                CaseId = caseId,
                CourtId = userContext.CourtId,
                DateAccept = DateTime.Now,
                FileNumber = documentService.GetDataInstitutionCaseInfoForDocument(caseCase.DocumentId)
            };
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на доказателство към дело
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            var model = service.GetById<CaseEvidence>(id);
            if (model == null)
            {
                throw new NotFoundException("Търсеното от Вас доказателство не е намерено и/или нямате достъп до него.");
            }
            if (!CheckAccess(service, SourceTypeSelectVM.CaseEvidence, id, AuditConstants.Operations.Update, model.CaseId))
            {
                return Redirect_Denied();
            }
            SetViewbag(model.CaseId);
            ViewBag.EvidenceName = model.RegNumber;
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Валидация при запис
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValid(CaseEvidence model)
        {
            if (model.EvidenceTypeId < 1)
                return "Изберете тип доказателство";

            if (model.DateAccept == null)
                return "Въведете дата на регистрация";

            if (model.EvidenceStateId < 1)
                return "Изберете статус";

            if (string.IsNullOrEmpty(model.Description))
                return "Въведете описание";

            if (model.EvidenceStateId == NomenclatureConstants.EvidenceState.Destroyed)
            {
                if (!service.IsExistMovmentType(model.CaseId, NomenclatureConstants.EvidenceMovementType.Destroyed))
                {
                    return "Няма разпоредително действие Унищожаване. Изберете друг статус.";
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Запис на доказателство
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CaseEvidence model)
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
            if (service.CaseEvidence_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseEvidence, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction("CasePreview", "Case", new { id = model.CaseId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(Edit), model);
        }

        void SetViewbag(int caseId)
        {
            ViewBag.EvidenceStateId_ddl = nomService.GetDropDownList<EvidenceState>();
            ViewBag.EvidenceTypeId_ddl = nomService.GetDropDownList<EvidenceType>();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(caseId);
            SetHelpFile(HelpFileValues.CaseEvidence);
        }

        [HttpPost]
        public IActionResult CaseEvidence_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseEvidence, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }

            if (service.IsExistMovment(model.Id))
            {
                return Json(new { result = false, message = "Има движение по това веществено доказателство." });
            }

            var expireObject = service.GetById<CaseEvidence>(model.Id);
            if (service.SaveExpireInfo<CaseEvidence>(model))
            {
                SetAuditContextDelete(service, SourceTypeSelectVM.CaseEvidence, model.Id);
                SetSuccessMessage(MessageConstant.Values.CaseSessionExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("CasePreview", "Case", new { id = expireObject.CaseId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        /// <summary>
        /// Извлизане на данни за движение на доказателство
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseEvidenceId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataMovement(IDataTablesRequest request, int caseEvidenceId)
        {
            var data = service.CaseEvidenceMovement_Select(caseEvidenceId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на движение
        /// </summary>
        /// <param name="caseEvidenceId"></param>
        /// <returns></returns>
        public IActionResult AddMovement(int caseEvidenceId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseEvidenceMovement, null, AuditConstants.Operations.Append, caseEvidenceId))
            {
                return Redirect_Denied();
            }
            SetViewbagMovement(caseEvidenceId);
            var model = new CaseEvidenceMovement()
            {
                CaseEvidenceId = caseEvidenceId,
                CaseId = ViewBag.CaseId,
                CourtId = userContext.CourtId,
                MovementDate = DateTime.Now
            };

            return View(nameof(EditMovement), model);
        }

        /// <summary>
        /// Редакция на движение
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditMovement(int id)
        {
            var model = service.GetById<CaseEvidenceMovement>(id);
            if (model == null)
            {
                throw new NotFoundException("Търсеното от Вас доказателство не е намерено и/или нямате достъп до него.");
            }
            if (!CheckAccess(service, SourceTypeSelectVM.CaseEvidenceMovement, id, AuditConstants.Operations.Append, model.CaseEvidenceId))
            {
                return Redirect_Denied();
            }

            SetViewbagMovement(model.CaseEvidenceId);
            return View(nameof(EditMovement), model);
        }

        /// <summary>
        /// запис на движение
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditMovement(CaseEvidenceMovement model)
        {
            SetViewbagMovement(model.CaseEvidenceId);

            if (!ModelState.IsValid)
            {
                return View(nameof(EditMovement), model);
            }

            string _isvalid = IsValid(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditMovement), model);
            }

            var currentId = model.Id;
            if (service.CaseEvidenceMovement_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseEvidenceMovement, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditMovement), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditMovement), model);
        }

        /// <summary>
        /// Валидация преди запис на движение
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValid(CaseEvidenceMovement model)
        {
            if (model.EvidenceMovementTypeId < 0)
                return "Няма избран вид движение";

            return string.Empty;
        }

        void SetViewbagMovement(int caseEvidenceId)
        {
            var caseEvidence = service.GetById<CaseEvidence>(caseEvidenceId);
            var caseCase = service.GetById<Case>(caseEvidence.CaseId);
            ViewBag.CaseName = caseCase.RegNumber;
            ViewBag.CaseId = caseCase.Id;
            ViewBag.CaseEvidenceName = caseEvidence.RegNumber;

            ViewBag.EvidenceMovementTypeId_ddl = nomService.GetDropDownList<EvidenceMovementType>();
            ViewBag.CaseSessionActId_ddl = caseSessionActService.GetDropDownList(caseCase.Id);
            //ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(caseId);
            SetHelpFile(HelpFileValues.CaseEvidence);
        }

        /// <summary>
        /// Справка за доказателства
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseEvidenceSpr()
        {
            var model = new CaseEvidenceSprFilterVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = new DateTime(DateTime.Now.Year, 12, 31);
            SetHelpFile(HelpFileValues.Register5);

            return View(model);
        }

        /// <summary>
        /// Извличане на екселски файл от справка за доказателства
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseEvidenceSpr(CaseEvidenceSprFilterVM model)
        {
            var xlsBytes = service.CaseEvidenceSpr_ToExcel(userContext.CourtId, model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }
    }
}