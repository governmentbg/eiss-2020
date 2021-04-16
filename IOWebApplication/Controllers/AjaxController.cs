using DataTables.AspNet.Core;
using IO.LogOperation.Models;
using IO.LogOperation.Service;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using System.Web;

namespace IOWebApplication.Controllers
{
    [DisableAudit]
    [Authorize]
    public class AjaxController : Controller
    {
        private readonly INomenclatureService nomenclatureService;
        private readonly ICaseService caseService;
        private readonly IUserContext userContext;
        private readonly ICommonService commonService;
        private readonly ICasePersonService casePersonService;
        private readonly ICalendarService calendarService;
        private readonly ICaseGroupService caseGroupService;
        private readonly ICaseLoadIndexService caseLoadIndexService;
        private readonly IEisppService eisppService;
        private readonly ICdnService cdnService;

        protected readonly ILogOperationService<ApplicationDbContext> logOperation;

        public AjaxController(
            INomenclatureService _nomenclatureService,
            ICaseService _caseService,
            ICommonService _commonService,
            IUserContext _userContext,
            ICasePersonService _casePersonService,
            ICalendarService _calendarService,
            ICaseGroupService _caseGroupService,
            ICaseLoadIndexService _caseLoadIndexService,
            IEisppService _eisppService,
            ILogOperationService<ApplicationDbContext> _logOperation,
            ICdnService _cdnService)
        {
            nomenclatureService = _nomenclatureService;
            caseService = _caseService;
            commonService = _commonService;
            userContext = _userContext;
            logOperation = _logOperation;
            calendarService = _calendarService;
            casePersonService = _casePersonService;
            caseGroupService = _caseGroupService;
            caseLoadIndexService = _caseLoadIndexService;
            eisppService = _eisppService;
            cdnService = _cdnService;
        }

        [HttpGet]
        public IActionResult SearchEkatte(string query)
        {
            return new JsonResult(nomenclatureService.GetEkatte(query));
        }

        [HttpGet]
        public IActionResult SearchEkatteEispp(string query)
        {
            return new JsonResult(nomenclatureService.GetEkatteEispp(query));
        }

        [HttpGet]
        public IActionResult GetEkatte(string id)
        {
            var ekatte = nomenclatureService.GetEkatteById(id);

            if (ekatte == null)
            {
                return BadRequest();
            }

            return new JsonResult(ekatte);
        }

        [ResponseCache(Duration = 3600)]
        [AllowAnonymous]
        public IActionResult GetBlankFooter(int id, string date, string time)
        {
            var court = nomenclatureService.GetById<Infrastructure.Data.Models.Common.Court>(id);

            if (court != null)
            {
                var _info = System.Net.WebUtility.HtmlEncode($"{court.Address}, {court.CityName}");
                return Content($"<!DOCTYPE html><html><head><meta http-equiv=Content-Type content=\"text/html;charset=utf-8\"></head><body style=\"width:80%;text-align:center;\">{_info}</body></html>", "text/html");
            }

            return Content("");
        }

        [HttpGet]
        public IActionResult SearchStreet(string ekatte, string query, int? streetType = null)
        {
            return new JsonResult(nomenclatureService.GetStreet(ekatte, query, streetType));
        }
        [HttpGet]
        public IActionResult GetStreet(string ekatte, string street_code)
        {
            var model = nomenclatureService.GetStreetByCode(ekatte, street_code);

            if (model == null)
            {
                return BadRequest();
            }

            return new JsonResult(model);
        }
        [HttpGet]
        public IActionResult GetEkatteByEisppCode(string eisppCode)
        {
            var model = nomenclatureService.GetEkatteByEisppCode(eisppCode);

            if (model == null)
            {
                return BadRequest();
            }

            return new JsonResult(model);
        }

        [HttpGet]
        public IActionResult GetEkatteByEisppCodeCategory(string eisppCode)
        {
            var model = nomenclatureService.GetEkatteByEisppCodeCategory(eisppCode);

            if (model == null)
            {
                return BadRequest();
            }

            return new JsonResult(model);
        }


        [HttpGet]
        public IActionResult FindPersonByUic(string uic, int uicType)
        {
            var model = commonService.Person_FindByUic(uic, uicType);

            return Json(model);
        }
        public IActionResult GetDDL_DocumentKind(int documentDirectionId, bool addDefaultElement = false)
        {
            return Json(nomenclatureService.GetDDL_DocumentKind(documentDirectionId, addDefaultElement));
        }
        public IActionResult GetDDL_DocumentGroup(int documentKindId)
        {
            return Json(nomenclatureService.GetDDL_DocumentGroup(documentKindId));
        }
        public IActionResult GetDDL_DocumentGroupByCourt(int documentKindId, int? courtOrganizationId = null)
        {
            return Json(nomenclatureService.GetDDL_DocumentGroupByCourt(documentKindId, courtOrganizationId));
        }
        [HttpGet]
        public IActionResult GetDDL_DocumentType(int documentGroupId, bool addDefaultElement = false)
        {
            var model = nomenclatureService.GetDDL_DocumentType(documentGroupId, addDefaultElement);

            return Json(model);
        }
        [HttpGet]
        public IActionResult GetDDL_DocumentTypeByCourt(int documentGroupId, bool addDefaultElement = false, int? courtOrganizationId = null)
        {
            var model = nomenclatureService.GetDDL_DocumentTypeByCourt(documentGroupId, addDefaultElement, false, courtOrganizationId);

            return Json(model);
        }
        [HttpGet]
        public IActionResult GetDDL_CaseType(int caseGroupId, bool addDefaultElement = true)
        {
            var model = nomenclatureService.GetDDL_CaseType(caseGroupId, addDefaultElement);

            return Json(model);
        }

        [HttpGet]
        public IActionResult GetDDL_CaseTypes(string caseGroupIds, bool addDefaultElement = false)
        {
            var model = nomenclatureService.GetDDL_CaseTypes(caseGroupIds, addDefaultElement);

            return Json(model);
        }

        [HttpGet]
        public IActionResult GetDDL_CaseTypeFromCourtType(int caseGroupId, string caseInstanceIds, bool addDefaultElement = true)
        {
            var model = nomenclatureService.GetDDL_CaseTypeFromCourtType(caseGroupId, caseInstanceIds, addDefaultElement);

            return Json(model);
        }


        [HttpGet]
        public IActionResult Get_CaseTypeUnits(int caseTypeId)
        {
            var model = caseGroupService.CaseTypeUnit_Select(caseTypeId).ToList();

            var ddlUnits = model.Select(x => new LabelValueVM(x.Id, x.Label));

            return Json(ddlUnits.ToSelectList());
        }
        [HttpGet]
        public IActionResult Get_CaseTypeUnitsReserves(int caseTypeUnitId)
        {
            if (caseTypeUnitId == 0)
            {
                return null;
            }
            var model = caseGroupService.GetById_CaseTypeUnit(caseTypeUnitId);

            var ddlReserves = new List<LabelValueVM>();
            if (model.CaseTypeUnitCounts.Any(x => x.Id == NomenclatureConstants.JudgeRole.ReserveJudge && x.Value > 0))
            {
                ddlReserves.Add(new LabelValueVM(NomenclatureConstants.JudgeRole.ReserveJudge, $"С резервен съдия"));
            }
            if (model.CaseTypeUnitCounts.Any(x => x.Id == NomenclatureConstants.JudgeRole.ReserveJury && x.Value > 0))
            {
                ddlReserves.Add(new LabelValueVM(NomenclatureConstants.JudgeRole.ReserveJury, $"С резервен заседател"));
            }
            if (ddlReserves.Count() == 2)
            {
                ddlReserves.Insert(1, new LabelValueVM(NomenclatureConstants.JudgeRole.ReserveJudgeAndJury, $"С резервни съдия и заседател"));
            }
            if (ddlReserves.Any())
            {
                ddlReserves.Insert(0, new LabelValueVM(-1, $"Без резервни участници"));
            }
            return Json(ddlReserves.ToSelectList());
        }
        [HttpGet]
        public IActionResult GetDDL_CaseTypeByDocType(int documentTypeId, int? courtOrganizationId = null)
        {
            var model = nomenclatureService.GetDDL_CaseTypeByDocType(documentTypeId, null, courtOrganizationId);

            return Json(model);
        }

        [HttpGet]
        public IActionResult GetDDL_CaseCode(int caseTypeId)
        {
            var model = nomenclatureService.GetDDL_CaseCode(caseTypeId.ValueToArray<int>());

            return Json(model);
        }
        [HttpGet]
        public IActionResult Get_Case(string query, int courtId = 0, int? caseId = null)
        {
            if (courtId == 0)
            {
                courtId = userContext.CourtId;
            }
            var model = caseService.GetCasesByCourt(courtId, caseId, query.EmptyToNull());

            return Json(model);
        }
        [HttpGet]
        public IActionResult GetDDL_CaseSessionActs(int caseId)
        {
            var model = caseService.GetDDL_SessionActsByCase(caseId);

            return Json(model);
        }
        //[HttpGet]
        //public IActionResult Get_CaseCode(int caseTypeId, string query = null, int? id = null)
        //{
        //    var model = nomenclatureService.GetDDL_CaseCode(caseTypeId.ValueToArray<int>(), query, id);

        //    return Json(model);
        //}
        [HttpGet]
        public IActionResult Get_CaseCodeByLoadGroup(string caseTypeId, string query = null, int? id = null)
        {
            var model = nomenclatureService.GetDDL_CaseCode(caseTypeId.StringToIntArray(), query, id, userContext.CourtTypeId != NomenclatureConstants.CourtType.VKS);

            return Json(model);
        }
        [HttpGet]
        public PartialViewResult SearchCaseCode(string containerId, string caseTypeId, string callback)
        {
            var model = new SelectCaseCodeVM()
            {
                ContainerId = containerId,
                CaseTypeId = caseTypeId,
                SelectCallback = callback
            };
            return PartialView("_SelectCaseCode", model);
        }
        public IActionResult LoadData_CaseCodeSearch(IDataTablesRequest request, string caseTypeId)
        {
            var data = nomenclatureService.GetDDL_CaseCode(caseTypeId.StringToIntArray(), request.Search?.Value, null, userContext.CourtTypeId != NomenclatureConstants.CourtType.VKS);

            return request.GetResponse(data.AsQueryable());
        }

        [HttpGet]
        public IActionResult Get_Courts(string query = null, int? id = null)
        {
            var model = commonService.Get_Courts(query.EmptyToNull(), id);
            return Json(model);
        }

        [HttpGet]
        public IActionResult Get_Organizations(string query = null, int? id = null)
        {
            var model = commonService.Get_Organizations(query.EmptyToNull(), id);
            return Json(model);
        }
        [HttpGet]
        public IActionResult Get_CaseReasons(string query = null, int? id = null)
        {
            var model = commonService.Get_CaseReasons(query.EmptyToNull(), id);
            return Json(model);
        }
        public IActionResult GetDDL_AddressType()
        {
            var model = nomenclatureService.GetDropDownList<AddressType>();
            return Json(model);
        }

        public IActionResult GetDDL_DeliveryType(int deliveryGroupId)
        {
            return Json(nomenclatureService.GetDDL_DeliveryType(deliveryGroupId));
        }

        public IActionResult GetDDL_Institution(int institutionTypeId)
        {
            var model = commonService.Institution_Select(institutionTypeId, null).ToSelectList(x => x.Id, x => x.FullName);
            return Json(model);
        }

        [HttpGet]
        public IActionResult Get_Institution(int institutionTypeId = 0, string query = null, int? id = null, string institutionTypeIds = null)
        {
            if (institutionTypeId == 0 && (id ?? 0) == 0)
            {
                institutionTypeId = -1;
                //id = -1;
            }
            Expression<Func<InstitutionVM, bool>> dateSearch = x => true;
            if (!string.IsNullOrEmpty(query))
            {
                var dtNow = DateTime.Now.Date;
                dateSearch = x => x.DateFrom <= dtNow && (x.DateTo ?? DateTime.MaxValue) >= dtNow;
            }
            var model = commonService.Institution_Select(institutionTypeId, query.EmptyToNull(), id.EmptyToNull(), institutionTypeIds)
                                            .Where(dateSearch)
                                            .ToList()
                                            .Select(x => new LabelValueVM
                                            {
                                                Value = x.Id.ToString(),
                                                Label = x.FullName + ((!string.IsNullOrEmpty(x.Code)) ? $" ({x.Code})" : "")
                                            });
            return Json(model);
        }

        #region История на промените

        public JsonResult Get_LogOperation(string controller_name, string action_name, string object_key)
        {
            var model = logOperation.Select(controller_name?.ToLower(), action_name?.ToLower(), object_key)
                .ToList()
                .Select(i => new
                {
                    oper_date = i.OperationDate.ToString("dd.MM.yyyy HH:mm"),
                    user = i.OperationUser,
                    oper = GetOperationName(i.OperationTypeId),
                    id = i.Id
                });
            return Json(model);
        }
        private string GetOperationName(int operType)
        {
            switch (operType)
            {
                case (int)OperationTypes.Insert:
                    return "Добавяне";
                case (int)OperationTypes.Message:
                    return "Известяване";
                case (int)OperationTypes.Delete:
                    return "Изтриване";
                case (int)OperationTypes.Patch:
                    return "Актуализация";
                case (int)OperationTypes.Update:
                default:
                    return "Редакция";
            }
        }
        public ContentResult Get_LogOperationHTML(long id)
        {
            var html = logOperation.LoadData(id);
            if (!string.IsNullOrEmpty(html))
                html = html.Replace("multi-transfer-changed_css", "multi-transfer-changed");
            return Content(html);
        }

        public JsonResult Get_LogOperationHTMLwithPrior(string controller_name, string action_name, string object_key, long currentId)
        {
            var priorOperation = logOperation.Select(controller_name?.ToLower(), action_name?.ToLower(), object_key)
                        .Where(x => x.Id < currentId)
                        .OrderByDescending(x => x.Id)
                        .Take(1)
                        .FirstOrDefault();
            var currentHtml = logOperation.LoadData(currentId);
            if (!string.IsNullOrEmpty(currentHtml))
                currentHtml = currentHtml.Replace("multi-transfer-changed_css", "multi-transfer-changed");
            var priorHtml = string.Empty;
            if (priorOperation != null)
            {
                priorHtml = logOperation.LoadData(priorOperation.Id);
            }
            return Json(new { current = currentHtml, prior = priorHtml });
        }

        #endregion

        [HttpGet]
        public IActionResult GetDDL_LoadGroupLink(int courtTypeId, int caseTypeId, int caseCodeId)
        {
            var model = nomenclatureService.GetDDL_LoadGroupLink(courtTypeId, caseTypeId, caseCodeId).SingleOrChoose();
            return Json(model);
        }
        [HttpGet]
        public IActionResult GetDDL_CaseCharacter(int caseTypeId, int? caseCharacterId = null)
        {
            var model = nomenclatureService.GetDDL_CaseCharacter(caseTypeId, caseCharacterId.EmptyToNull()).SingleOrChoose();
            return Json(model);
        }
        [HttpGet]
        public IActionResult Get_CaseType(int caseTypeId)
        {
            var model = nomenclatureService.GetById<CaseType>(caseTypeId);
            return Json(model);
        }
        [HttpGet]
        public IActionResult GetDDL_CaseReason(int caseTypeId)
        {
            var model = nomenclatureService.GetDDL_CaseReason(caseTypeId);
            return Json(model);
        }
        [HttpGet]
        public IActionResult GetDDL_HtmlTemplate(int notificationTypeId, int caseId)
        {
            var model = nomenclatureService.GetDDL_HtmlTemplate(notificationTypeId, caseId);
            return Json(model);
        }
        [HttpGet]
        public IActionResult GetDDL_HtmlTemplateByDocType(int documentTypeId, int caseId, int sourceType, bool setDefault)
        {
            var model = nomenclatureService.GetDDL_HtmlTemplateByDocType(documentTypeId, caseId, sourceType, userContext.CourtTypeId, setDefault);
            return Json(model);
        }

        [HttpGet]
        public IActionResult GetDDL_AddressFromLawUnitAddressByCaseLawUnitId(int caseLawUnitId)
        {
            var model = commonService.LawUnitAddress_SelectDDL_ByCaseLawUnitId(caseLawUnitId);
            return Json(model);
        }

        [HttpGet]
        public IActionResult GetDDL_NotificationStateFromDeliveryGroup(int notificationDeliveryGroupId, int notificationStateId)
        {
            var model = nomenclatureService.GetDDL_NotificationStateFromDeliveryGroup(notificationDeliveryGroupId, notificationStateId);
            return Json(model);
        }

        [HttpGet]
        public IActionResult GetDDL_SessionResultBase(int sessionResultId)
        {
            var model = nomenclatureService.GetDDL_SessionResultBase(sessionResultId);
            return Json(model);
        }

        [HttpGet]
        public IActionResult GetDDL_SessionResultFromRulesByCaseLoadElementTypeAndSessionType(int caseLoadElementTypeId, int sessionTypeId)
        {
            var model = nomenclatureService.GetDDL_SessionResultFromRulesByCaseLoadElementTypeAndSessionType(caseLoadElementTypeId, sessionTypeId);
            return Json(model);
        }

        [HttpGet]
        public PartialViewResult SearchEntity(string callback)
        {
            var model = new SelectEntityVM()
            {
                SelectEntityCallback = callback
            };
            ViewBag.institutionTypes = nomenclatureService.GetDropDownList<InstitutionType>(false, false);
            ViewBag.lawunitTypes = commonService.SelectEntity_LawUnitTypes();

            return PartialView("_SelectEntity", model);
        }

        [HttpPost]
        public IActionResult SearchEntity_ListData(IDataTablesRequest request, string sourceType)
        {
            int st = SourceTypeSelectVM.Court;
            int? objectTypeId = null;
            if (sourceType.Contains(SourceTypeSelectVM.InstututionPrefix))
            {
                st = SourceTypeSelectVM.Instutution;
                objectTypeId = int.Parse(sourceType.Replace(SourceTypeSelectVM.InstututionPrefix, ""));
            }
            if (sourceType.Contains(SourceTypeSelectVM.LawUnitPrefix))
            {
                st = SourceTypeSelectVM.LawUnit;
                objectTypeId = int.Parse(sourceType.Replace(SourceTypeSelectVM.LawUnitPrefix, ""));
            }
            var model = commonService.SelectEntity_Select(st, request.Search?.Value.EmptyToNull(), objectTypeId);
            return request.GetResponse(model);
        }

        [HttpGet]
        public IActionResult GetDDL_EkMunincipality(string EkatteDistrict)
        {
            var model = nomenclatureService.GetDDL_EkMunincipality(EkatteDistrict);
            return Json(model);
        }

        public IActionResult Calendar_GetByPerson(string start, string end)
        {
            var model = calendarService.SelectByPerson(start.StrToDateFormat("yyyy-MM-dd"), end.StrToDateFormat("yyyy-MM-dd"));
            return Json(model);
        }

        [HttpGet]
        public IActionResult GetDDL_CaseSessionActsWithDefaultElement(int caseId)
        {
            var model = caseService.GetDDL_SessionActsByCase(caseId, true);

            return Json(model);
        }

        [HttpGet]
        public IActionResult GetDDL_CaseTypeGroupInstance(int caseGroupId, int caseInstanceId, string caseInstanceIds)
        {
            var model = nomenclatureService.GetDDL_CaseTypeGroupInstance(caseGroupId, caseInstanceId, caseInstanceIds);

            return Json(model);
        }

        public IActionResult GetDDL_InstitutionFilter(int institutionTypeId)
        {
            var model = commonService.GetDDL_Institution(institutionTypeId);
            return Json(model);
        }

        [HttpGet]
        public IActionResult Get_CaseloadAddActivity(string query = null, int? id = null)
        {
            var model = caseLoadIndexService.Get_CaseLoadAddActivity(query.EmptyToNull(), id);
            return Json(model);
        }

        [HttpGet]
        public IActionResult Get_EISPPTblElement(string eisppTblCode, string query = null, string id = "")
        {
            var model = eisppService.Get_EISPPTblElement(eisppTblCode.EmptyToNull(), query.EmptyToNull(), id.EmptyToNull());
            return Json(model);
        }

        [HttpGet]
        public IActionResult GetDDL_CaseTypeByCourtType(int caseGroupId, bool addDefaultElement)
        {
            var model = nomenclatureService.GetDDL_CaseTypeByCourtType(caseGroupId, userContext.CourtTypeId, addDefaultElement);

            return Json(model);
        }

        [HttpGet]
        public IActionResult GetDDL_ActComplainResult(int caseTypeId)
        {
            var model = nomenclatureService.GetDDL_ActComplainResult(caseTypeId, true);

            return Json(model);
        }

        [HttpGet]
        public IActionResult Search_ActLawBase(string query)
        {
            var model = nomenclatureService.Get_ActLawBase(query, 0);

            return Json(model);
        }

        [HttpGet]
        public IActionResult Get_ActLawBase(int id)
        {
            var model = nomenclatureService.Get_ActLawBase(null, id).FirstOrDefault();

            return Json(model);
        }

        [HttpGet]
        public IActionResult Get_Source(int sourceTypeId = 0, string query = null, long? id = null)
        {
            IEnumerable<LabelValueVM> model;
            int intId = (int)(id ?? -1);
            switch (sourceTypeId)
            {
                case SourceTypeSelectVM.Court:
                    {
                        model = commonService.Get_Courts(query.EmptyToNull(), intId);
                        break;
                    }
                case SourceTypeSelectVM.Instutution:
                    {
                        model = commonService.Institution_Select(0, query.EmptyToNull(), intId)
                                            .ToList()
                                            .Select(x => new LabelValueVM
                                            {
                                                Value = x.Id.ToString(),
                                                Label = x.FullName + ((!string.IsNullOrEmpty(x.Code)) ? $" ({x.Code})" : "")
                                            });
                        break;
                    }
                default:
                    {
                        model = Enumerable.Empty<LabelValueVM>();
                        break;
                    }
            }

            return Json(model);
        }
    }
}