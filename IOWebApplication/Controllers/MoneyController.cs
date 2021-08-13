using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Money;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Rotativa.Extensions;

namespace IOWebApplication.Controllers
{
    public class MoneyController : BaseController
    {
        private readonly IMoneyService service;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly ICaseSessionService sessionService;
        private readonly ICasePersonService casePersonService;
        private readonly IDocumentService documentService;
        private readonly ICaseLawUnitService caseLawUnitService;
        private readonly IPrintDocumentService printDocumentService;
        private readonly IPriceService priceService;
        private readonly ICdnService cdnService;
        private readonly IWorkTaskService taskService;


        public MoneyController(IMoneyService _service, INomenclatureService _nomService, ICaseSessionService _sessionService,
                              ICasePersonService _casePersonService, ICommonService _commonService, IDocumentService _documentService,
                              ICaseLawUnitService _caseLawUnitService,
                              IPrintDocumentService _printDocumentService,
                              IPriceService _priceService,
                              ICdnService _cdnService,
                              IWorkTaskService _taskService)
        {
            service = _service;
            nomService = _nomService;
            sessionService = _sessionService;
            casePersonService = _casePersonService;
            commonService = _commonService;
            documentService = _documentService;
            caseLawUnitService = _caseLawUnitService;
            printDocumentService = _printDocumentService;
            priceService = _priceService;
            cdnService = _cdnService;
            taskService = _taskService;
        }


        public IActionResult testreq(int id)
        {
            CheckAccess(service, SourceTypeSelectVM.SessionActObligation, id, AuditConstants.Operations.View);
            return Content("");
        }
        /// <summary>
        /// Страница за задължения към Акт/Документ/Заседание
        /// </summary>
        /// <param name="caseSessionActId"></param>
        /// <param name="documentId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public IActionResult Obligation(int? caseSessionActId, long? documentId, int? caseSessionId)
        {
            if (documentId > 0)
            {
                if (!CheckAccess(service, SourceTypeSelectVM.DocumentObligation, null, AuditConstants.Operations.List, documentId))
                {
                    return Redirect_Denied();
                }
            }
            else
            {
                if (caseSessionActId > 0 || caseSessionId > 0)
                {
                    if (!CheckAccess(service, caseSessionActId > 0 ? SourceTypeSelectVM.SessionActObligation : SourceTypeSelectVM.SessionObligation, null, AuditConstants.Operations.View, caseSessionActId > 0 ? caseSessionActId : caseSessionId))
                    {
                        return Redirect_Denied();
                    }
                }
            }

            ViewBag.caseSessionActId = caseSessionActId;
            ViewBag.documentId = documentId;
            ViewBag.caseSessionId = caseSessionId;
            if ((caseSessionActId ?? 0) > 0)
            {
                ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSessionAct(caseSessionActId ?? 0);
                SetHelpFile(HelpFileValues.SessionAct);
            }
            else if ((documentId ?? 0) > 0)
            {
                ViewBag.breadcrumbs = commonService.Breadcrumbs_DocumentEdit(documentId ?? 0);
            }

            return View();
        }

        /// <summary>
        /// Извличане на задължения за Datatable
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseSessionActId"></param>
        /// <param name="documentId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataObligation(IDataTablesRequest request, int caseSessionActId, long documentId, int caseSessionId)
        {
            var data = service.Obligation_Select(caseSessionActId, documentId, caseSessionId, userContext.CourtId);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на задължение
        /// </summary>
        /// <param name="caseSessionActId"></param>
        /// <param name="documentId"></param>
        /// <param name="caseSessionId"></param>
        /// <param name="sourceTypeId"></param>
        /// <returns></returns>
        public IActionResult AddObligation(int caseSessionActId, long documentId, int caseSessionId, int sourceTypeId)
        {
            if (caseSessionActId > 0 || caseSessionId > 0)
            {
                if (!CheckAccess(service, caseSessionActId > 0 ? SourceTypeSelectVM.SessionActObligation : SourceTypeSelectVM.SessionObligation, null, AuditConstants.Operations.Append, caseSessionActId > 0 ? caseSessionActId : caseSessionId))
                {
                    return Redirect_Denied();
                }
            }
            else
            {
                if (documentId > 0)
                {
                    if (!CheckAccess(service, SourceTypeSelectVM.DocumentObligation, null, AuditConstants.Operations.Append, documentId))
                    {
                        return Redirect_Denied();
                    }
                }
            }

            if (sourceTypeId == 0)
            {
                if (caseSessionActId > 0)
                    sourceTypeId = SourceTypeSelectVM.CasePerson;
                else if (documentId > 0)
                    sourceTypeId = SourceTypeSelectVM.DocumentPerson;
                else if (caseSessionId > 0)
                    sourceTypeId = SourceTypeSelectVM.CaseLawUnit;
            }
            SetViewbagObligation(caseSessionActId, documentId, caseSessionId, sourceTypeId);
            var model = new ObligationEditVM()
            {
                CourtId = userContext.CourtId,
                CaseSessionActId = caseSessionActId == 0 ? (int?)null : caseSessionActId,
                DocumentId = documentId == 0 ? (long?)null : documentId,
                CaseSessionId = caseSessionId == 0 ? (int?)null : caseSessionId,
                Person_SourceType = sourceTypeId,
                IsActive = true
            };
            if (caseSessionActId > 0)
            {
                model.MoneySign = NomenclatureConstants.MoneySign.SignPlus;
            }
            else if (documentId > 0)
            {
                model.MoneySign = NomenclatureConstants.MoneySign.SignPlus;
                model.MoneyTypeId = NomenclatureConstants.MoneyType.StateFee;
            }
            else if (caseSessionId > 0)
            {
                model.MoneySign = NomenclatureConstants.MoneySign.SignMinus;
            }
            return View(nameof(EditObligation), model);
        }

        /// <summary>
        /// Редакция на задължение
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditObligation(int id)
        {
            var model = service.Obligation_GetById(id);
            if (model.CaseSessionActId > 0 || model.CaseSessionId > 0)
            {
                if (!CheckAccess(service, model.CaseSessionActId > 0 ? SourceTypeSelectVM.SessionActObligation : SourceTypeSelectVM.SessionObligation, null, AuditConstants.Operations.Append, model.CaseSessionActId > 0 ? model.CaseSessionActId : model.CaseSessionId))
                {
                    return Redirect_Denied();
                }
            }
            else
            {
                if (model.DocumentId > 0)
                {
                    if (!CheckAccess(service, SourceTypeSelectVM.DocumentObligation, id, AuditConstants.Operations.Update))
                    {
                        return Redirect_Denied();
                    }
                }
            }
            SetViewbagObligation(model.CaseSessionActId, model.DocumentId, model.CaseSessionId, model.Person_SourceType ?? 0);
            return View(nameof(EditObligation), model);
        }

        /// <summary>
        /// Валидация преди запис на задължение
        /// </summary>
        /// <param name="model"></param>
        public void ValidateModelObligation(ObligationEditVM model)
        {
            if (model.CaseSessionId > 0)
            {
                var _session = service.GetById<CaseSession>(model.CaseSessionId.Value);
                if (_session.DateExpired != null)
                {
                    ModelState.AddModelError("", "Заседанието е изтрито. Проверете данните по делото.");
                }
            }
            if (model.DocumentId > 0)
            {
                var _parent = service.GetById<Document>(model.DocumentId.Value);
                if (_parent.DateExpired != null)
                {
                    ModelState.AddModelError("", "Документът е изтрит.");
                }
            }

            if ((model.DocumentId ?? 0) > 0 && (model.MoneyFeeTypeId ?? 0) <= 0 && string.IsNullOrEmpty(model.Description) == true)
            {
                ModelState.AddModelError(nameof(ObligationEditVM.Description), "Въведете описание");
            }

            if ((model.ExecListTypeId ?? 0) == NomenclatureConstants.ExecListTypes.Country)
            {
                if ((model.ReceiveSourceId ?? 0) <= 0)
                    ModelState.AddModelError(nameof(ObligationEditVM.ReceiveSourceId), "Изберете В полза на");

                if (model.MoneySign == NomenclatureConstants.MoneySign.SignMinus)
                    ModelState.AddModelError(nameof(ObligationEditVM.MoneySign), "Разход не може да бъде В полза на Държавата");
            }
            else if ((model.ExecListTypeId ?? 0) == NomenclatureConstants.ExecListTypes.ThirdPerson)
            {
                if ((model.PersonReceiveId ?? 0) <= 0)
                    ModelState.AddModelError(nameof(ObligationEditVM.PersonReceiveId), "Изберете В полза на");
            }
        }

        /// <summary>
        /// Запис на задължение
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditObligation(ObligationEditVM model)
        {
            ValidateModelObligation(model);
            SetViewbagObligation(model.CaseSessionActId, model.DocumentId, model.CaseSessionId, model.Person_SourceType ?? 0);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditObligation), model);
            }
            var currentId = model.Id;
            (bool result, string errorMessage) = service.Obligation_SaveData(model);
            if (result)
            {
                if (model.CaseSessionActId > 0 || model.CaseSessionId > 0)
                {
                    CheckAccess(service, model.CaseSessionActId > 0 ? SourceTypeSelectVM.SessionActObligation : SourceTypeSelectVM.SessionObligation, null, AuditConstants.Operations.Append, model.CaseSessionActId > 0 ? model.CaseSessionActId : model.CaseSessionId);
                }
                else
                {
                    if (model.DocumentId > 0)
                    {
                        SetAuditContext(service, SourceTypeSelectVM.DocumentObligation, model.Id, currentId == 0);
                    }

                }
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                if ((model.CaseSessionId ?? 0) > 0)
                {
                    return RedirectToAction("Preview", "CaseSession", new { id = model.CaseSessionId });
                }
                else
                {
                    return RedirectToAction("Obligation", new
                    {
                        caseSessionActId = model.CaseSessionActId,
                        documentId = model.DocumentId,
                        caseSessionId = model.CaseSessionId
                    });
                }
            }
            else
            {
                if (string.IsNullOrEmpty(errorMessage))
                    errorMessage = MessageConstant.Values.SaveFailed;
                SetErrorMessage(errorMessage);
            }
            return View(nameof(EditObligation), model);
        }

        /// <summary>
        /// Попълване на данни за Добавяне/Редакция на задължение
        /// </summary>
        /// <param name="caseSessionActId"></param>
        /// <param name="documentId"></param>
        /// <param name="caseSessionId"></param>
        /// <param name="sourceType"></param>
        void SetViewbagObligation(int? caseSessionActId, long? documentId, int? caseSessionId, int sourceType)
        {
            ViewBag.MoneyTypeJson = JsonConvert.SerializeObject(nomService.Get_MoneyType());

            if ((caseSessionActId ?? 0) > 0)
            {
                var caseSessionAct = service.GetById<CaseSessionAct>(caseSessionActId);
                var caseSession = sessionService.CaseSessionById(caseSessionAct.CaseSessionId);
                if (sourceType == SourceTypeSelectVM.CasePerson)
                {
                    ViewBag.Person_SourceId_ddl = casePersonService.CasePerson_SelectForDropDownList(caseSession.CaseId, null);
                    ViewBag.PersonReceiveId_ddl = casePersonService.CasePerson_SelectForDropDownList(caseSession.CaseId, null);
                    ViewBag.ExecListTypeId_ddl = nomService.GetDropDownList<ExecListType>();
                    ViewBag.ReceiveSourceTypeId_ddl = nomService.GetDDL_MoneyCountryReceiver();
                }
                else if (sourceType == SourceTypeSelectVM.CaseLawUnit)
                    ViewBag.Person_SourceId_ddl = caseLawUnitService.CaseLawUnitForCaseObligation_SelectForDropDownList(caseSession.CaseId);
                ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSessionActMoney(caseSessionActId ?? 0);
                ViewBag.MoneySign_ddl = DropDownSign(false);
                ViewBag.MoneyTypeId_ddl = nomService.GetDropDownList<MoneyType>(true, false, false);
                ViewBag.MoneyFineTypeId_ddl = nomService.GetDDL_MoneyFineType(caseSession.Case.CaseGroupId);
                SetHelpFile(HelpFileValues.SessionAct);
            }
            else if ((documentId ?? 0) > 0)
            {
                var document = documentService.GetByIdWithData(documentId ?? 0);
                ViewBag.Person_SourceId_ddl = documentService.DocumentPerson_SelectForDropDownList(documentId ?? 0);
                ViewBag.MoneyFeeTypeId_ddl = nomService.GetDDL_MoneyFeeType(document.DocumentGroupId);
                ViewBag.breadcrumbs = commonService.Breadcrumbs_DocumentObligation(documentId ?? 0);
                ViewBag.MoneySign_ddl = DropDownSign(false, false);
                if (document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.InitialDocument)
                {
                    ViewBag.MoneyTypeId_ddl = nomService.GetDropDownList<MoneyType>(true, false, false)
                           .Where(x => x.Value == NomenclatureConstants.MoneyType.StateFee.ToString()).ToList();
                }
                else if (document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument)
                {
                    ViewBag.MoneyTypeId_ddl = nomService.GetDropDownList<MoneyType>(true, false, false)
                           .Where(x => NomenclatureConstants.MoneyType.MoneyCompliantDocumentList.Contains(int.Parse(x.Value))).ToList();
                }
                else
                {
                    ViewBag.MoneyTypeId_ddl = nomService.GetDropDownList<MoneyType>(true, false, false);
                }
            }
            else if ((caseSessionId ?? 0) > 0)
            {
                var caseSession = sessionService.CaseSessionById(caseSessionId ?? 0);
                if (sourceType == SourceTypeSelectVM.CasePerson)
                    ViewBag.Person_SourceId_ddl = casePersonService.CasePerson_SelectForDropDownList(caseSession.CaseId, null);
                else if (sourceType == SourceTypeSelectVM.CaseLawUnit)
                    ViewBag.Person_SourceId_ddl = caseLawUnitService.GetJuryForSession_SelectForDropDownList(caseSessionId ?? 0);
                ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSession(caseSessionId ?? 0);
                ViewBag.MoneySign_ddl = DropDownSign(false);
                ViewBag.MoneyTypeId_ddl = nomService.GetDropDownList<MoneyType>(true, false, false);
            }
        }

        /// <summary>
        /// Зареждане на данни за филтър на страница за дължими суми
        /// </summary>
        void SetViewBagObligationForPayFilter()
        {
            List<SelectListItem> statusObl = new List<SelectListItem>();
            statusObl.Add(new SelectListItem() { Text = "Всички", Value = "-1" });
            statusObl.Add(new SelectListItem() { Text = MoneyConstants.ObligationStatus.StatusPaidStr, Value = MoneyConstants.ObligationStatus.StatusPaid.ToString() });
            statusObl.Add(new SelectListItem() { Text = MoneyConstants.ObligationStatus.StatusNotEndStr, Value = MoneyConstants.ObligationStatus.StatusNotEnd.ToString() });
            ViewBag.Status_ddl = statusObl;
            ViewBag.MoneyTypeId_ddl = nomService.GetDropDownList<MoneyType>(true, false, false);
        }

        /// <summary>
        /// Страница за дължими суми
        /// </summary>
        /// <param name="sign"></param>
        /// <returns></returns>
        public IActionResult ObligationForPay(int sign)
        {
            SetViewBagObligationForPayFilter();
            var model = new ObligationForPayFilterVM();
            model.Status = MoneyConstants.ObligationStatus.StatusNotEnd;
            model.Sign = sign == 0 ? NomenclatureConstants.MoneySign.SignPlus : sign;
            if (model.Sign == NomenclatureConstants.MoneySign.SignMinus)
                SetHelpFile(HelpFileValues.Finance1);
            else
                SetHelpFile(HelpFileValues.Finance2);
            return View(model);
        }

        /// <summary>
        ///  Извличане на дължими суми за Datatable
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataObligationForPay(IDataTablesRequest request, ObligationForPayFilterVM model)
        {
            var data = service.ObligationForPay_Select(userContext.CourtId, model);

            return request.GetResponse(data);
        }


        /// <summary>
        /// Добавяне на плащане към задължение
        /// </summary>
        /// <param name="idStr"></param>
        /// <returns></returns>
        public IActionResult Payment(string idStr)
        {
            if (string.IsNullOrEmpty(idStr))
                return Content("Изберете поне едно задължение");

            List<int> moneyGroup = service.MoneyGroup_Select(idStr);
            if (moneyGroup.Count() == 0)
                return Content("Проблем при четене на Банкови сметки");
            else if (moneyGroup.Count() > 1)
                return Content("Избрали сте задължения от различни видове сметки");

            var bankAccount = commonService.BankAccount_SelectDDL(userContext.CourtId, moneyGroup[0]);
            ViewBag.PaymentTypeId_ddl = nomService.GetDropDownList<PaymentType>();
            ViewBag.BankAccountJson = JsonConvert.SerializeObject(commonService.CourtBankAccountForCourt_Select(userContext.CourtId));

            if (bankAccount.Count() == 0)
                return Content("Няма конфигурирана банкова сметка за този вид сметка");

            var model = new PaymentVM();
            model.CourtId = userContext.CourtId;
            model.CourtBankAccountId = int.Parse(bankAccount[0].Value);
            model.ObligationIds = idStr;
            model.PaidDate = DateTime.Now;
            model.Amount = service.GetSumForPay(idStr);
            model.ForPopUp = true;

            bankAccount.Insert(0, new SelectListItem() { Text = "По друга сметка", Value = "-1" });
            ViewBag.CourtBankAccountId_ddl = bankAccount;

            return PartialView("EditPayment", model);
        }

        /// <summary>
        /// Запис на плащане към задължение
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Payment(PaymentVM model)
        {
            var res = true;
            var error = "";
            if (model.PaymentTypeId <= 0)
            {
                res = false;
                error = "Изберете начин на плащане";
            }
            if (Math.Abs(model.Amount) <= 0)
            {
                res = false;
                error = "Въведете сума";
            }
            if (model.PaidDate.Date > DateTime.Now.Date)
            {
                res = false;
                error = "Плащането не може да е с бъдеща дата";
            }

            if (model.CourtBankAccountId <= 0 && model.PaymentTypeId != NomenclatureConstants.PaymentType.Bank)
            {
                res = false;
                error = "Изберете банкова сметка";
            }

            if (res == true)
            {
                (bool result, string errorMessage) = service.MakePayment(model);
                res = result;
                error = errorMessage;
                if (res == false && string.IsNullOrEmpty(error))
                    error = "Проблем при запис на плащането";
            }

            return Json(new { result = res, message = error, id = model.Id, paymenttypeid = model.PaymentTypeId });
        }

        /// <summary>
        /// Страница за извършение плащания
        /// </summary>
        /// <returns></returns>
        public IActionResult PaymentList()
        {
            ViewBag.MoneyGroupId_ddl = nomService.GetDropDownList<MoneyGroup>();
            ViewBag.PaymentTypeId_ddl = nomService.GetDropDownList<PaymentType>();
            ViewBag.PosDeviceTid_ddl = commonService.CourtPosDevice_SelectDDL(userContext.CourtId, true);
            ViewBag.HasStorno = userContext.IsUserInRole(AccountConstants.Roles.Supervisor);
            var model = new PaymentFilterVM();
            model.DateFrom = DateTime.Now;
            model.DateTo = DateTime.Now;
            model.UserId = userContext.UserId;
            model.ActivePayment = true;
            SetHelpFile(HelpFileValues.Finance5);

            return View(model);
        }

        /// <summary>
        /// Извличане на данните за извършени плащания за Datatable
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataPayment(IDataTablesRequest request, PaymentFilterVM model)
        {
            var data = service.Payment_Select(userContext.CourtId, model);

            return request.GetResponse(data);
        }

        void SetViewBagPayment()
        {
            ViewBag.PaymentTypeId_ddl = nomService.GetDropDownList<PaymentType>();
            ViewBag.CourtBankAccountId_ddl = commonService.BankAccount_SelectDDL(userContext.CourtId, 0, true);
            ViewBag.BankAccountJson = JsonConvert.SerializeObject(commonService.CourtBankAccountForCourt_Select(userContext.CourtId));
            SetHelpFile(HelpFileValues.Finance4);
        }

        /// <summary>
        /// Добавяне на авансово плащане
        /// </summary>
        /// <returns></returns>
        public IActionResult AddAvansPayment()
        {
            SetViewBagPayment();
            var model = new PaymentVM()
            {
                CourtId = userContext.CourtId,
                PaidDate = DateTime.Now,
                IsAvans = true,
                ForPopUp = false
            };

            return View(nameof(EditPayment), model);
        }

        /// <summary>
        /// Редакция на авансово плащане
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditPayment(int id)
        {
            SetViewBagPayment();
            var model = service.Payment_GetById(id);
            model.ForPopUp = false;
            return View(nameof(EditPayment), model);
        }

        /// <summary>
        /// Валидация преди запис на авансово плащане
        /// </summary>
        /// <param name="model"></param>
        void ValidatePayment(PaymentVM model)
        {
            if (Math.Abs(model.Amount) <= 0)
            {
                ModelState.AddModelError("", "Въведете сума");
            }
            if (model.PaidDate.Date > DateTime.Now.Date)
            {
                ModelState.AddModelError("", "Плащането не може да е с бъдеща дата");
            }
        }

        /// <summary>
        /// Запис на авансово плащане
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditPayment(PaymentVM model)
        {
            SetViewBagPayment();
            ValidatePayment(model);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditPayment), model);
            }
            var currentId = model.Id;
            if (service.Payment_SaveData(model))
            {
                await SaveFilePayment(model.Id, model.PaymentTypeId);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditPayment), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditPayment), model);
        }

        /// <summary>
        /// Сторно на плащане
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult StornoPayment(int id)
        {
            object res = null;
            string erroMessage = "";
            bool result = service.Payment_Storno(id, ref erroMessage);

            if (result == true)
            {
                res = new { result = result, message = "Деактивирането премина успешно" };
            }
            else
            {
                if (erroMessage == "")
                    erroMessage = "Проблем при деактивиране на плащането";
                res = new { result = result, message = erroMessage };
            }

            return Json(res);
        }

        /// <summary>
        /// Задължения за едно плащане
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult ObligationsForPayment(int id)
        {
            ViewBag.HasStorno = userContext.IsUserInRole(AccountConstants.Roles.Supervisor);

            return PartialView(id);
        }

        /// <summary>
        /// Извличане на задълженията за едно плащане
        /// </summary>
        /// <param name="request"></param>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataObligationsForPayment(IDataTablesRequest request, int paymentId)
        {
            var data = service.ObligationPaymentForPayment_Select(paymentId);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Плащания за едно задължение
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult PaymentsForObligation(int id)
        {
            ViewBag.HasStorno = userContext.IsUserInRole(AccountConstants.Roles.Supervisor);

            return PartialView(id);
        }

        /// <summary>
        /// Извличане на плащанията към едно задължение
        /// </summary>
        /// <param name="request"></param>
        /// <param name="obligationId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataPaymentsForObligation(IDataTablesRequest request, int obligationId)
        {
            var data = service.ObligationPaymentForObligation_Select(obligationId);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Сторно на връзка задължение-плащане
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult StornoObligationPayment(int id)
        {
            object res = null;
            string erroMessage = "";
            bool result = service.ObligationPayment_Storno(id, ref erroMessage);

            if (result == true)
            {
                res = new { result = result, message = "Деактивирането премина успешно" };
            }
            else
            {
                if (erroMessage == "")
                    erroMessage = "Проблем при деактивиране";
                res = new { result = result, message = erroMessage };
            }

            return Json(res);
        }

        /// <summary>
        /// Извличане на авансови плащания
        /// </summary>
        /// <param name="moneyGroupId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult SearchBalancePayment(int moneyGroupId, string query)
        {
            return Json(service.GetBalancePayment(userContext.CourtId, query, moneyGroupId));
        }

        /// <summary>
        /// Извличане на плащане по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetPayment(int id)
        {
            var payment = service.GetPaymentById(id);

            if (payment == null)
            {
                return BadRequest();
            }

            return Json(payment);
        }

        /// <summary>
        /// Въвеждане на авансово плащане
        /// </summary>
        /// <param name="idStr"></param>
        /// <returns></returns>
        public IActionResult BalancePayment(string idStr)
        {
            if (string.IsNullOrEmpty(idStr))
                return Content("Изберете поне едно задължение");

            List<int> moneyGroup = service.MoneyGroup_Select(idStr);
            if (moneyGroup.Count() == 0)
                return Content("Проблем при четене на Банкови сметки");
            else if (moneyGroup.Count() > 1)
                return Content("Избрали сте задължения от различни видове сметки");

            var model = new BalancePaymentVM();
            model.MoneyGroupId = moneyGroup[0];
            model.ObligationIds = idStr;
            model.AmountForPay = service.GetSumForPay(idStr);

            return PartialView(model);
        }

        /// <summary>
        /// Извличане на данните за авансово плащане
        /// </summary>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetBalancePayment(int paymentId)
        {
            var data = service.GetPaymentById_BalancePayment(paymentId);
            return Json(new { amount = data.Amount, amountObligationPay = data.AmountPayObligation });
        }

        /// <summary>
        /// Запис на авансово плащане
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult BalancePayment(BalancePaymentVM model)
        {
            var error = "";

            var res = service.BalancePayment_SaveData(model, ref error);
            if (res == false)
            {
                if (error == "")
                    error = "Проблем при запис на плащането";
            }

            return Json(new { result = res, message = error });
        }

        /// <summary>
        /// Запис на плащане през ПОС
        /// </summary>
        /// <param name="json"></param>
        /// <param name="bankAccountId"></param>
        /// <param name="amount"></param>
        /// <param name="senderName"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult PosPaymentResult(string json, int bankAccountId, decimal amount, string senderName)
        {
            PosPaymentResult model = JsonConvert.DeserializeObject<PosPaymentResult>(json);
            model.CourtId = userContext.CourtId;
            model.CourtBankAccountId = bankAccountId;
            model.Amount = amount;
            model.SenderName = senderName;
            object res = null;
            string erroMessage = "";
            bool result = service.PosPaymentResult_SaveData(model);
            if (model.Status != MoneyConstants.PosPaymentResultStatus.StatusOk)
            {
                result = false;
                erroMessage = "Отказана транзакция";
            }
            if (result == true)
            {
                res = new { result = result, message = MessageConstant.Values.SaveOK, id = model.Id };
            }
            else
            {
                if (erroMessage == "")
                    erroMessage = "Проблем при плащане с ПОС";
                res = new { result = result, message = erroMessage };
            }

            return Json(res);
        }

        /// <summary>
        /// Страница с незаписани плащания през ПОС
        /// </summary>
        /// <returns></returns>
        public IActionResult UnsavedPosPayment()
        {
            SetHelpFile(HelpFileValues.Finance);
            return View();
        }


        /// <summary>
        /// Извличане на незаписаните плащания през ПОС
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataUnsavedPosPayment(IDataTablesRequest request)
        {
            var data = service.UnsavedPosPayment_Select(userContext.CourtId);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Създаване на плащания за ПОС транзакция
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> MakePosPaymentFromPosResult(int id)
        {
            object res = null;
            (bool result, string errormessage, int paymentId) = service.MakePosPaymentFromPosResult(id);

            if (result == true)
            {
                await SaveFilePayment(paymentId, NomenclatureConstants.PaymentType.Pos);
                res = new { result = result, message = "Плащането премина успешно" };
            }
            else
            {
                if (string.IsNullOrEmpty(errormessage))
                    errormessage = "Проблем при създаване на плащане";
                res = new { result = result, message = errormessage };
            }

            return Json(res);
        }

        /// <summary>
        /// Запис на плащане в pdf
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> PreviewRaw(int id)
        {
            TinyMCEVM htmlModel = printDocumentService.FillHtmlTemplatePayment(id);
            if (htmlModel != null)
            {
                string html = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
                var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }).GetByte(this.ControllerContext);
                return File(pdfBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, "Payment" + id.ToString() + ".pdf");
            }
            else
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }
        }

        /// <summary>
        /// Запис на файл за РКО
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> SaveFileExpenseOrder(int id)
        {
            (TinyMCEVM htmlModel, string errorMessage) = printDocumentService.FillHtmlTemplateExpenseOrder(id);
            if (htmlModel != null)
            {
                string html = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
                var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }).GetByte(this.ControllerContext);
                var pdfRequest = new CdnUploadRequest()
                {
                    SourceType = SourceTypeSelectVM.ExpenseOrder,
                    SourceId = id.ToString(),
                    FileName = "expenseOrder.pdf",
                    ContentType = "application/pdf",
                    Title = "РКО",
                    FileContentBase64 = Convert.ToBase64String(pdfBytes)
                };
                bool result = await cdnService.MongoCdn_AppendUpdate(pdfRequest);
                Response.Headers.Clear();

                return result;
            }
            else
                return false;
        }

        /// <summary>
        /// Страница за разходни ордери
        /// </summary>
        /// <returns></returns>
        public IActionResult ExpenseOrderList()
        {
            var model = new ExpenseOrderFilterVM();
            model.DateFrom = DateTime.Now;
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Finance7);
            return View(model);
        }

        /// <summary>
        /// Извличане на данните за разходни ордери
        /// </summary>
        /// <param name="request"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="name"></param>
        /// <param name="expenseOrderRegNumber"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataExpenseOrder(IDataTablesRequest request, DateTime? fromDate, DateTime? toDate, string name, string expenseOrderRegNumber)
        {
            var data = service.ExpenseOrder_Select(userContext.CourtId, fromDate, toDate, name, expenseOrderRegNumber);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Сторно на разходен ордер
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult StornoExpenseOrder(int id)
        {
            object res = null;
            (bool result, string errormessage) = service.ExpenseOrder_Storno(id);

            if (result == true)
            {
                res = new { result = result, message = "Деактивирането премина успешно" };
            }
            else
            {
                if (errormessage == "")
                    errormessage = "Проблем при деактивиране";
                res = new { result = result, message = errormessage };
            }

            return Json(res);
        }

        void SetViewBagExpenseOrder()
        {
            ViewBag.ExpenseOrderStateId_ddl = nomService.GetDropDownList<ExpenseOrderState>();
            SetHelpFile(HelpFileValues.Finance7);
        }

        /// <summary>
        /// Редакция на разходен ордер
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditExpenseOrder(int id)
        {
            SetViewBagExpenseOrder();
            var model = service.ExpenseOrder_GetById(id);
            model.ForPopUp = false;
            return View(nameof(EditExpenseOrder), model);
        }

        /// <summary>
        /// Запис на разходен ордер
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditExpenseOrder(ExpenseOrderEditVM model)
        {
            if (model.LawUnitSignId <= 0)
            {
                ModelState.AddModelError(nameof(ExpenseOrderEditVM.LawUnitSignId), "Изберете подписващ съдия");
            }

            SetViewBagExpenseOrder();
            if (!ModelState.IsValid)
            {
                return View(nameof(EditExpenseOrder), model);
            }
            var currentId = model.Id;
            (bool result, string errormessage) = service.ExpenseOrder_Update(model);
            if (result == true)
            {
                await SaveFileExpenseOrder(model.Id);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditExpenseOrder), new { id = model.Id });
            }
            else
            {
                if (errormessage == "")
                    errormessage = MessageConstant.Values.SaveFailed;
                SetErrorMessage(errormessage);
            }
            return View(nameof(EditExpenseOrder), model);
        }

        /// <summary>
        /// Въвеждане на разходен ордер
        /// </summary>
        /// <param name="idStr"></param>
        /// <returns></returns>
        public IActionResult ExpenseOrder(string idStr)
        {
            if (string.IsNullOrEmpty(idStr))
                return Content("Изберете поне едно задължение");

            var expenseOrder = service.ExpenseOrder_LastOrderForPerson(idStr);

            var model = new ExpenseOrderEditVM();
            model.ForPopUp = true;
            model.ObligationIdStr = idStr;
            if (expenseOrder != null)
            {
                model.RegionName = expenseOrder.RegionName;
                model.FirmName = expenseOrder.FirmName;
                model.FirmCity = expenseOrder.FirmCity;
                model.Iban = expenseOrder.Iban;
                model.BIC = expenseOrder.BIC;
                model.BankName = expenseOrder.BankName;
                model.LawUnitSignId = expenseOrder.LawUnitSignId ?? 0;
            }

            return PartialView("EditExpenseOrder", model);
        }

        /// <summary>
        /// Запис на разходен ордер
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExpenseOrder(ExpenseOrderEditVM model)
        {
            if (model.LawUnitSignId <= 0)
            {
                ModelState.AddModelError(nameof(ExpenseOrderEditVM.LawUnitSignId), "Изберете подписващ съдия");
            }

            if (!ModelState.IsValid)
            {
                string messageModalResult = string.Join(", ", ModelState.Values
                                                                    .SelectMany(v => v.Errors)
                                                                    .Select(e => e.ErrorMessage));
                return Json(new { result = false, message = messageModalResult, id = model.Id });
            }
            (bool result, string errormessage) = service.ExpenseOrder_Save(model);
            if (result == true)
            {
                await SaveFileExpenseOrder(model.Id);
            }
            else
            {
                if (errormessage == "")
                    errormessage = MessageConstant.Values.SaveFailed;
            }

            return Json(new { result = result, message = errormessage, id = model.Id });
        }

        /// <summary>
        /// Извличане на цена за основание към документ
        /// </summary>
        /// <param name="moneyFeeId"></param>
        /// <returns></returns>
        public IActionResult Get_PriceFee(int moneyFeeId)
        {
            var result = priceService.GetPriceValue(null, NomenclatureConstants.PriceDescKeyWord.KeyMoneyFee, 0, null, 0, 0, moneyFeeId.ToString());
            return Json(new
            {
                price = result
            });
        }

        /// <summary>
        /// Извличане на данни за банка по iban
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public IActionResult GetBankDataFromIban(string search)
        {
            var result = nomService.GetBankByCodeSearch(search);
            if (result == null)
                result = new Bank();
            return Json(new
            {
                bic = result.BIC,
                bankName = result.Label
            });
        }

        List<SelectListItem> DropDownSign(bool addAll, bool addMinus = true)
        {
            List<SelectListItem> sign = new List<SelectListItem>();
            if (addAll)
                sign.Add(new SelectListItem() { Text = "Всички", Value = "0" });
            sign.Add(new SelectListItem() { Text = NomenclatureConstants.MoneySign.SignPlusName, Value = NomenclatureConstants.MoneySign.SignPlus.ToString() });
            if (addMinus)
                sign.Add(new SelectListItem() { Text = NomenclatureConstants.MoneySign.SignMinusName, Value = NomenclatureConstants.MoneySign.SignMinus.ToString() });
            return sign;
        }

        void SetViewBagExecListIndex()
        {
            ViewBag.ExecListTypeId_ddl = nomService.GetDropDownList<ExecListType>();
            ViewBag.InstitutionId_ddl = commonService.GetDDL_Institution(NomenclatureConstants.InstitutionTypes.NAP);
        }

        /// <summary>
        /// Страница за изпълнителни листове
        /// </summary>
        /// <returns></returns>
        public IActionResult ExecListIndex()
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForExecList().DeleteOrDisableLast();

            SetViewBagExecListIndex();
            var model = new ExecListFilterVM();
            model.DateFrom = DateTime.Now.AddDays(-7);
            model.DateTo = DateTime.Now;
            model.ActiveExecList = true;
            SetHelpFile(HelpFileValues.Finance8);

            return View(model);
        }

        /// <summary>
        /// Справка за изпълнителни листове
        /// </summary>
        /// <returns></returns>
        public IActionResult ExecListIndexReport()
        {
            SetViewBagExecListIndex();
            var model = new ExecListFilterVM();
            model.DateFrom = DateTime.Now.AddDays(-7);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Report36);

            return View(model);
        }

        /// <summary>
        /// Извличане на данни за изпълнителни листове
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataExecList(IDataTablesRequest request, ExecListFilterVM model)
        {
            var data = service.ExecList_Select(userContext.CourtId, model);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Извличане на данни за справка за изпълнителни листове
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataExecListReport(IDataTablesRequest request, ExecListFilterVM model)
        {
            var data = service.ExecListReport_Select(userContext.CourtId, model, "<br>");

            return request.GetResponse(data);
        }

        /// <summary>
        /// Сторно на изпълнителен лист
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult StornoExecList(int id)
        {
            object res = null;
            (bool result, string errormessage) = service.ExecList_Storno(id);

            if (result == true)
            {
                res = new { result = result, message = "Деактивирането премина успешно" };
            }
            else
            {
                if (errormessage == "")
                    errormessage = "Проблем при деактивиране";
                res = new { result = result, message = errormessage };
            }

            return Json(res);
        }

        void SetViewBagExecList(ExecListEditVM model)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForExecListEdit(model.Id).DeleteOrDisableLast();

            if (model.ExecListTypeId == NomenclatureConstants.ExecListTypes.ThirdPerson)
                ViewBag.ExecListLawBaseId_ddl = nomService.GetDDL_ExecListLawBase(model.CaseGroupId);

            if (model.Id > 0)
                ViewBag.ExecListStateId_ddl = nomService.GetDropDownList<ExecListState>();

            SetHelpFile(HelpFileValues.Finance8);
        }

        /// <summary>
        /// Редакция на изпълнителен лист
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditExecList(int id)
        {
            var model = service.ExecList_GetById(id);
            model.ForPopUp = false;
            SetViewBagExecList(model);
            return View(nameof(EditExecList), model);
        }

        /// <summary>
        /// Запис на изпълнителен лист
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditExecList(ExecListEditVM model)
        {
            SetViewBagExecList(model);
            if (model.LawUnitSignId <= 0)
            {
                ModelState.AddModelError(nameof(ExecListEditVM.LawUnitSignId), "Изберете подписващ съдия");
            }

            if (!ModelState.IsValid)
            {
                return View(nameof(EditExecList), model);
            }
            var currentId = model.Id;
            (bool result, string errormessage) = service.ExecList_Update(model);
            if (result == true)
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditExecList), new { id = model.Id });
            }
            else
            {
                if (errormessage == "")
                    errormessage = MessageConstant.Values.SaveFailed;
                SetErrorMessage(errormessage);
            }
            return View(nameof(EditExecList), model);
        }

        /// <summary>
        /// Въвеждане на изпълнителен лист
        /// </summary>
        /// <param name="idStr"></param>
        /// <returns></returns>
        public IActionResult ExecList(string idStr)
        {
            if (string.IsNullOrEmpty(idStr))
                return Content("Изберете поне едно задължение");

            var model = new ExecListEditVM();
            model.ForPopUp = true;
            model.ObligationIdStr = idStr;
            (bool result, string errorMessage) = service.ExecList_PrepareSave(model);
            if (result == false)
                return Content(errorMessage);

            SetViewBagExecList(model);

            return PartialView("EditExecList", model);
        }

        /// <summary>
        /// Запис на изпълнителен лист
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExecList(ExecListEditVM model)
        {
            SetViewBagExecList(model);

            if (model.LawUnitSignId <= 0)
            {
                ModelState.AddModelError(nameof(ExecListEditVM.LawUnitSignId), "Изберете подписващ съдия");
            }

            if (!ModelState.IsValid)
            {
                string messageModalResult = string.Join(", ", ModelState.Values
                                                                    .SelectMany(v => v.Errors)
                                                                    .Select(e => e.ErrorMessage));
                return Json(new { result = false, message = messageModalResult, id = model.Id });
            }
            (bool result, string erroMessage) = service.ExecList_Save(model);
            if (result == true)
            {
            }
            else
            {
                if (erroMessage == "")
                    erroMessage = MessageConstant.Values.SaveFailed;
            }

            return Json(new { result = result, message = erroMessage, id = model.Id });
        }

        /// <summary>
        /// Извличане на данни за получател на сума
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public IActionResult LastDataForReceive(int sourceType, long sourceId)
        {
            var result = service.LastDataForReceive_Select(sourceType, sourceId);
            return Json(new
            {
                iban = result.Iban,
                bic = result.BIC,
                bankName = result.BankName
            });
        }

        /// <summary>
        /// Файлове за обект
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="sourceID"></param>
        /// <returns></returns>
        public IActionResult GetFileList(int sourceType, int sourceID)
        {
            var model = cdnService.Select(sourceType, sourceID.ToString()).SetCanDelete(false).ToList();

            return Json(model);
        }

        /// <summary>
        /// Запис на файл за плащане
        /// </summary>
        /// <param name="id"></param>
        /// <param name="paymentType"></param>
        /// <returns></returns>
        private async Task<byte[]> SaveFilePayment(int id, int paymentType)
        {
            if (paymentType != NomenclatureConstants.PaymentType.Pos) return null;
            TinyMCEVM htmlModel = printDocumentService.FillHtmlTemplatePayment(id);
            if (htmlModel != null)
            {
                string html = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
                var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }).GetByte(this.ControllerContext);

                var pdfRequest = new CdnUploadRequest()
                {
                    SourceType = SourceTypeSelectVM.Payment,
                    SourceId = id.ToString(),
                    FileName = "payment.pdf",
                    ContentType = "application/pdf",
                    Title = "Плащане",
                    FileContentBase64 = Convert.ToBase64String(pdfBytes)
                };
                bool result = await cdnService.MongoCdn_AppendUpdate(pdfRequest);
                Response.Headers.Clear();

                return pdfBytes;
            }
            else
            {
                return null;
            }
        }

        public async Task<IActionResult> SaveAndShowFilePayment(int id, int paymentType)
        {
            var pdfBytes = await SaveFilePayment(id, paymentType);
            if (pdfBytes != null)
            {
                return File(pdfBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, "Payment" + id.ToString() + ".pdf");
            }
            else
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }
        }

        /// <summary>
        /// Страница с протоколи за изпълнителни листове
        /// </summary>
        /// <returns></returns>
        public IActionResult ExchangeDocList()
        {
            ViewBag.InstitutionId_ddl = commonService.GetDDL_Institution(NomenclatureConstants.InstitutionTypes.NAP);
            var model = new ExchangeDocFilterVM();
            SetHelpFile(HelpFileValues.Finance9);

            return View(model);
        }

        /// <summary>
        /// Извличане на данни за протоколи
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataExchangeDoc(IDataTablesRequest request, ExchangeDocFilterVM model)
        {
            var data = service.ExchangeDoc_Select(userContext.CourtId, model);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Сторно на протокол
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult StornoExchangeDoc(int id)
        {
            object res = null;
            (bool result, string errormessage) = service.ExchangeDoc_Storno(id);

            if (result == true)
            {
                res = new { result = result, message = "Деактивирането премина успешно" };
            }
            else
            {
                if (errormessage == "")
                    errormessage = "Проблем при деактивиране";
                res = new { result = result, message = errormessage };
            }

            return Json(res);
        }

        /// <summary>
        /// Редакция на протокол
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditExchangeDoc(int id)
        {
            var model = service.ExchangeDoc_GetById(id);
            SetHelpFile(HelpFileValues.Finance9);

            return View(nameof(EditExchangeDoc), model);
        }

        /// <summary>
        /// Запис на протокол
        /// </summary>
        /// <param name="idStr"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ExchangeDoc(string idStr)
        {
            if (string.IsNullOrEmpty(idStr))
            {
                Json(new { result = false, message = "Изберете поне един ИЛ" });
            }

            (bool result, string erroMessage, int id) = service.ExchangeDoc_Save(idStr);
            if (result == true)
            {
                //SaveFileExchangeDoc(id);
                return Json(new { result = result, message = MessageConstant.Values.SaveOK });
            }
            else
            {
                if (erroMessage == "")
                    erroMessage = "Проблем при запис";
                return Json(new { result = result, message = erroMessage });
            }
        }

        /// <summary>
        /// Експорт в ексел на справка за изпълнителни листове
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ExecListReportExportExcel(ExecListFilterVM model)
        {
            var xlsBytes = service.ExecListReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        void SetViewBagObligationThirdPersonFilter()
        {
            ViewBag.MoneyTypeId_ddl = nomService.GetDropDownList<MoneyType>(true, false, false);
        }

        /// <summary>
        /// Страница със задължения към трети лица
        /// </summary>
        /// <returns></returns>
        public IActionResult ObligationThirdPerson()
        {
            SetViewBagObligationThirdPersonFilter();
            var model = new ObligationThirdPersonFilterVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Finance3);

            return View(model);
        }

        /// <summary>
        /// Извличане на данните за задължения в полза на трети лица
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataObligationThirdPerson(IDataTablesRequest request, ObligationThirdPersonFilterVM model)
        {
            var data = service.ObligationThirdPerson_Select(userContext.CourtId, model);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на бланка за ИЛ
        /// </summary>
        /// <param name="id"></param>
        /// <param name="del"></param>
        /// <returns></returns>
        private async Task<BlankEditVM> InitBlankFromTemplateExecList(int id, bool del)
        {
            int sourceType = SourceTypeSelectVM.ExecListBlank;
            if (del)
            {
                var deleted = await cdnService.MongoCdn_DeleteFiles(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });
            }
            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });
            if (string.IsNullOrEmpty(html))
            {
                TinyMCEVM htmlModel = printDocumentService.FillHtmlTemplateExecList(id);
                html = htmlModel.Text;
            }

            var model = new BlankEditVM()
            {
                Title = "Изготвяне на " + SourceTypeSelectVM.GetSourceTypeName(sourceType).ToLower(),
                SourceType = sourceType,
                SourceId = id.ToString(),
                SessionName = userContext.GenHash(id, sourceType),
                HtmlHeader = string.Empty,
                HtmlContent = html,
                HtmlFooter = string.Empty,
                FooterIsEditable = false,
                ReturnUrl = Url.Action(nameof(EditExecList), new { id }),
                HasPreviewButton = true,
            };

            return model;
        }

        /// <summary>
        /// Бланка за ИЛ
        /// </summary>
        /// <param name="id"></param>
        /// <param name="del"></param>
        /// <returns></returns>
        public async Task<IActionResult> BlankExecList(int id, bool del = false)
        {
            BlankEditVM blankModel = await InitBlankFromTemplateExecList(id, del);

            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForExecListEdit(id);
            return View("BlankEdit", blankModel);
        }

        private async Task<IActionResult> blankPreviewExecList(BlankEditVM model)
        {
            byte[] pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = model.HtmlContent }, true, GetFooterInfoUrl(userContext.CourtId)).GetByte(this.ControllerContext);

            return File(pdfBytes, NomenclatureConstants.ContentTypes.Pdf);
        }

        public async Task<bool> BlankExecListSave(string sourceId, string actHTML)
        {
            var htmlRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.ExecListBlank,
                SourceId = sourceId,
                FileName = "draft.html",
                ContentType = "text/html",
                FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(actHTML))
            };
            return await cdnService.MongoCdn_AppendUpdate(htmlRequest);
        }

        [HttpPost]
        public async Task<IActionResult> BlankExecList(BlankEditVM model, string btnPreview = null)
        {
            if (!userContext.CheckHash(model))
            {
                return Redirect_Denied();
            }
            if (string.IsNullOrEmpty(btnPreview))
            {
                if (await BlankExecListSave(model.SourceId, model.HtmlContent ?? ""))
                {
                    return RedirectToAction(nameof(EditExecList), new { id = model.SourceId });
                }
                else
                {
                    SetErrorMessage(MessageConstant.Values.SaveFailed);
                }
            }
            else
            {
                return await blankPreviewExecList(model);
            }

            return RedirectToAction(nameof(BlankExecList), new { id = model.SourceId });
        }

        private async Task<string> GetExecListHTML(int id)
        {
            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.ExecListBlank, SourceId = id.ToString() });
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            return html;
        }


        public async Task<bool> SaveFileFromBlankExecList(string html, int id)
        {
            var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }).GetByte(this.ControllerContext);
            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.ExecList,
                SourceId = id.ToString(),
                FileName = "execList.pdf",
                ContentType = "application/pdf",
                Title = "Изпълнителен лист",
                FileContentBase64 = Convert.ToBase64String(pdfBytes)
            };
            bool result = await cdnService.MongoCdn_AppendUpdate(pdfRequest);
            Response.Headers.Clear();

            return result;
        }

        public async Task<IActionResult> QuickSentForSignExecList(int execListid)
        {
            string actHTML = await GetExecListHTML(execListid);
            if (string.IsNullOrEmpty(actHTML))
            {
                SetErrorMessage("Няма изготвен ИЛ.");
                return RedirectToAction("EditExecList", new { id = execListid });
            }

            var newTask = new WorkTaskEditVM()
            {
                SourceType = SourceTypeSelectVM.ExecList,
                SourceId = execListid,
                TaskTypeId = WorkTaskConstants.Types.ExecList_SentToSign,
                TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser
            };

            if (taskService.CreateTask(newTask))
            {
                return await DoTask_SentForSign(newTask.Id);
            }
            else
            {
                SetErrorMessage("Проблем при изпращане за подпис");
                return RedirectToAction("EditExecList", new { id = execListid });
            }
        }

        public async Task<IActionResult> DoTask_SentForSign(long id)
        {
            var task = taskService.Select_ById(id);
            switch (task.SourceType)
            {
                case SourceTypeSelectVM.ExecList:
                    var actId = (int)task.SourceId;
                    var actModel = service.ExecList_GetById(actId);

                    string actHTML = await GetExecListHTML(actId);
                    if (string.IsNullOrEmpty(actHTML))
                    {
                        SetErrorMessage("Няма изготвен ИЛ.");
                        return RedirectToAction("EditExecList", new { id = actId });
                    }

                    await SaveFileFromBlankExecList(actHTML, actId);

                    var newTask = new WorkTaskEditVM()
                    {
                        ParentTaskId = id,
                        SourceType = SourceTypeSelectVM.ExecList,
                        SourceId = actId,
                        TaskTypeId = WorkTaskConstants.Types.ExecList_Sign,
                        TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser,
                        UserId = commonService.Users_GetUserIdByLawunit(actModel.LawUnitSignId),
                    };

                    if (taskService.CreateTask(newTask))
                    {
                        SetSuccessMessage("Задачата за подпис е създадена успешно.");
                        taskService.CompleteTask(id);
                    }
                    else
                    {
                        SetErrorMessage("Проблем при създаване на задача");
                    }

                    return RedirectToAction("EditExecList", new { id = actId });
                default:
                    return null;
            }
        }

        public async Task SaveFileAfterRegister(int idInt, ExecList execList)
        {
            string actHTML = await GetExecListHTML(idInt);
            //Ако се запише номер да попълня датата и номера за и да презапиша html
            actHTML = actHTML.Replace("{F_EXEC_LIST_NO}", execList.RegNumber);
            actHTML = actHTML.Replace("{F_TODAY}", execList.RegDate.DateToStr(FormattingConstant.NormalDateFormat));

            await BlankExecListSave(idInt.ToString(), actHTML);
            await SaveFileFromBlankExecList(actHTML, idInt);
        }

        public async Task<IActionResult> SendForSign(long id, long taskId)
        {
            int idInt = (int)id;

            Uri urlSuccess = new Uri(Url.Action(nameof(SignedOk), new { taskId }), UriKind.Relative);
            Uri url = new Uri(Url.Action("EditExecList", new { id = id }), UriKind.Relative);

            var model = new SignPdfInfo()
            {
                SourceId = id.ToString(),
                SourceType = SourceTypeSelectVM.ExecList,
                DestinationType = SourceTypeSelectVM.ExecList,
                Location = userContext.CourtName,
                Reason = "Подписване на ИЛ",
                SuccessUrl = urlSuccess,
                CancelUrl = url,
                ErrorUrl = url
            };

            var execList = service.GetById<ExecList>(idInt);

            var registerResult = service.ExecListRegister(execList);
            if (!registerResult.Result)
            {
                SetErrorMessage(registerResult.ErrorMessage);
                return RedirectToAction("EditExecList", new { id = id });
            }
            else
            {
                if (registerResult.SaveMethod == "register")
                {
                    await SaveFileAfterRegister(idInt, execList);
                    return RedirectToAction(nameof(SendForSign), new { id, taskId });
                }
            }

            var lu = taskService.GetLawUnitByTaskId(taskId);
            if (lu != null)
            {
                model.SignerName = lu.FullName;
                model.SignerUic = lu.Uic;
            }

            return View("_SignPdf", model);
        }

        public async Task<IActionResult> RegisterExecList(int id)
        {
            string actHTML = await GetExecListHTML(id);
            if (string.IsNullOrEmpty(actHTML))
            {
                SetErrorMessage("Няма изготвен ИЛ.");
                return RedirectToAction("EditExecList", new { id = id });
            }

            var execList = service.GetById<ExecList>(id);

            var registerResult = service.ExecListRegister(execList);
            if (!registerResult.Result)
            {
                SetErrorMessage(registerResult.ErrorMessage);
            }
            else
            {
                if (registerResult.SaveMethod == "register")
                {
                    await SaveFileAfterRegister(id, execList);
                }
            }

            return RedirectToAction("EditExecList", new { id = id });
        }

        public IActionResult SignedOk(long taskId)
        {
            var task = taskService.Select_ById(taskId);
            if (task != null && task.TaskStateId != WorkTaskConstants.States.Completed)
            {
                switch (task.TaskTypeId)
                {
                    case WorkTaskConstants.Types.ExecList_Sign:
                        taskService.CompleteTask(taskId);
                        var saveResult = taskService.UpdateAfterCompleteTask(task);
                        if (saveResult.Result)
                        {
                            SetSuccessMessage("Подписването на документа премина успешно.");
                        }
                        break;
                }

                return RedirectToAction(nameof(EditExecList), new { id = task.SourceId });
            }

            return RedirectToAction("Index", "Home");
        }
    }
}