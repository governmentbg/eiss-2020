using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
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
    public class CaseFastProcessController : BaseController
    {
        private readonly INomenclatureService nomService;
        private readonly ICaseFastProcessService service;
        private readonly ICaseLawUnitService lawUnitService;

        public CaseFastProcessController(INomenclatureService _nomService, 
                                         ICaseFastProcessService _service,
                                         ICaseLawUnitService _lawUnitService)
        {
            nomService = _nomService;
            service = _service;
            lawUnitService = _lawUnitService;
        }

        /// <summary>
        /// Страница с Заповедни производства към дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IActionResult Index(int caseId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseFastProcess, null, AuditConstants.Operations.View, caseId))
            {
                return Redirect_Denied();
            }
            SetViewBag(caseId);
            SetHelpFile(HelpFileValues.CaseFastProcess);
            return View(caseId);
        }

        private void SetViewBag(int CaseId)
        {
            ViewBag.caseId = CaseId;
            var caseCase = service.GetById<Case>(CaseId);
            ViewBag.CaseName = caseCase.RegNumber;
        }

        /// <summary>
        /// Извличане на данни за заповедни производства към дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public JsonResult GetData(int caseId)
        {
            var model = service.Select(caseId);
            return Json(model);
        }

        #region Bank Account

        private void SetViewBag_BankAccount(int CaseId)
        {
            ViewBag.CaseBankAccountTypeId_ddl = nomService.GetDropDownList<CaseBankAccountType>();
            var selectListItems = lawUnitService.GetDDL_LeftSide(CaseId, false);
            ViewBag.CasePersonId_ddl = selectListItems;
            ViewBag.HasPerson = selectListItems.Count > 1;
        }

        /// <summary>
        /// Валидация преди запис на сметки към заповедни производства към дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string ValidateCaseBankAccount(CaseBankAccount model)
        {
            if (model.CaseBankAccountTypeId < 1)
            {
                return "Изберете начин на плащане/изпълнение.";
            }

            if (model.CaseBankAccountTypeId == NomenclatureConstants.CaseBankAccountType.BankAccount)
            {
                if ((model.IBAN ?? string.Empty) == string.Empty)
                {
                    return "Въведете IBAN.";
                }

                if ((model.BIC ?? string.Empty) == string.Empty)
                {
                    return "Въведете BIC.";
                }

                if ((model.BankName ?? string.Empty) == string.Empty)
                {
                    return "Въведете име на банката.";
                }

                ModelState.Remove(nameof(model.Id));
                if (!ModelState.IsValid)
                {
                    foreach (var item in ModelState.Values.Where(x => x.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid))
                    {
                        if (item.Errors.Count() > 0)
                        {
                            return item.Errors[0].ErrorMessage;
                        }
                    }
                }
            }
            else
            {
                if ((model.Description ?? string.Empty) == string.Empty)
                {
                    return "Въведете описание.";
                }
            }

            if (!CheckCourtIdFromCase(model.CaseId))
            {
                return $"Съда е променен на {userContext.CourtName}. Презаредете текущия екран.";
            }

            if (model.CasePersonId == null)
            {
                return "По делото няма кредитори.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Добавяне на сметка към заповедни производства към дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult CaseBankAccount(int caseId, int? id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseBankAccount, (id > 0) ? id : null, (id > 0) ? AuditConstants.Operations.Update : AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }

            CaseBankAccount model;
            if (id > 0)
            {
                model = nomService.GetById<CaseBankAccount>(id);
            }
            else
            {
                model = new CaseBankAccount()
                {
                    CaseId = caseId,
                    CourtId = userContext.CourtId,
                    CaseBankAccountTypeId = NomenclatureConstants.CaseBankAccountType.BankAccount
                };
            }
            SetViewBag_BankAccount(caseId);
            return PartialView(model);
        }

        /// <summary>
        /// Запис на банкова сметка към заповедни производства към дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CaseBankAccount(CaseBankAccount model)
        {
            string validationError = ValidateCaseBankAccount(model);
            if (!string.IsNullOrEmpty(validationError))
            {
                return Json(new { result = false, message = validationError });
            }

            var currentId = model.Id;
            var res = service.CaseBankAccount_SaveData(model);
            if (res > 0)
                SetAuditContext(service, SourceTypeSelectVM.CaseBankAccount, res, currentId == 0);
            return Json(new { result = res });
        }

        /// <summary>
        /// Изтриване на банкова сметка към заповедни производства към дело
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CaseBankAccount_Delete(int Id)
        {
            CheckAccess(service, SourceTypeSelectVM.CaseBankAccount, Id, AuditConstants.Operations.Delete);
            return Json(new { result = service.CaseBankAccount_DeleteData(Id) });
        }

        [HttpPost]
        public JsonResult GetBankAccountLast(int CasePersonId, int id)
        {
            var caseBankAccount = service.CaseBankAccount_GetLastByPerson(CasePersonId, (id > 0 ? id : (int?)null));
            return Json(new { caseBankAccount });
        }

        #endregion

        #region MoneyClaim

        private void SetViewBag_MoneyClaim()
        {
            ViewBag.CaseMoneyClaimGroupId_ddl = nomService.GetDropDownList<CaseMoneyClaimGroup>();
        }

        [HttpGet]
        public IActionResult GetDDL_MoneyClaimType(int moneyClaimGroupId)
        {
            var model = nomService.GetDDL_MoneyClaimType(moneyClaimGroupId);
            return Json(model);
        }

        [HttpGet]
        public IActionResult GetDDL_MoneyCollectionType(int moneyCollectionGroupId)
        {
            var model = nomService.GetDDL_MoneyCollectionType(moneyCollectionGroupId);
            return Json(model);
        }

        [HttpGet]
        public IActionResult GetDDL_MoneyCollectionKind(int moneyCollectionGroupId)
        {
            var model = nomService.GetDDL_MoneyCollectionKind(moneyCollectionGroupId);
            return Json(model);
        }

        /// <summary>
        /// Добавяне на Обстоятелства по заповедни производства
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult CaseMoneyClaim(int caseId, int? id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseMoneyClaim, (id > 0) ? id : null, (id > 0) ? AuditConstants.Operations.Update : AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }

            CaseMoneyClaim model;
            if (id > 0)
            {
                model = nomService.GetById<CaseMoneyClaim>(id);
            }
            else
            {
                model = new CaseMoneyClaim()
                {
                    CaseId = caseId,
                    CourtId = userContext.CourtId,
                    CaseMoneyClaimGroupId = NomenclatureConstants.CaseMoneyClaimGroup.Contract
                };
            }
            SetViewBag_MoneyClaim();
            return PartialView(model);
        }

        /// <summary>
        /// Запис на Обстоятелства по заповедни производства
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CaseMoneyClaim(CaseMoneyClaim model)
        {
            string validationError = ValidateCaseMoneyClaim(model);
            if (!string.IsNullOrEmpty(validationError))
            {
                return Json(new { result = false, message = validationError });
            }

            var currentId = model.Id;
            var res = service.CaseMoneyClaim_SaveData(model);
            if (res > 0)
                SetAuditContext(service, SourceTypeSelectVM.CaseMoneyClaim, res, currentId == 0);

            return Json(new { result = res });
        }

        /// <summary>
        /// Валидация преди запис на Обстоятелства по заповедни производства
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string ValidateCaseMoneyClaim(CaseMoneyClaim model)
        {
            if (model.CaseMoneyClaimGroupId < 1)
            {
                return "Изберете основен вид на обстоятелство.";
            }

            if (model.CaseMoneyClaimGroupId == NomenclatureConstants.CaseMoneyClaimGroup.Contract)
            {
                if (model.CaseMoneyClaimTypeId < 1)
                {
                    return "Изберете точен вид на обстоятелство.";
                }
            }

            //if (model.ClaimDate == null)
            //{
            //    return "Въведете дата.";
            //}

            //if ((model.ClaimNumber ?? string.Empty) == string.Empty)
            //{
            //    return "Въведете номер.";
            //}

            if (!CheckCourtIdFromCase(model.CaseId))
            {
                return $"Съда е променен на {userContext.CourtName}. Презаредете текущия екран.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Изтриване на Обстоятелства по заповедни производства
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CaseMoneyClaim_Delete(int Id)
        {
            CheckAccess(service, SourceTypeSelectVM.CaseMoneyClaim, Id, AuditConstants.Operations.Delete);
            return Json(new { result = service.CaseMoneyClaim_DeleteData(Id) });
        }

        #endregion

        #region MoneyCollection

        private void SetViewBag_MoneyCollection()
        {
            ViewBag.CaseMoneyCollectionGroupId_ddl = nomService.GetDropDownList<CaseMoneyCollectionGroup>();
            ViewBag.MoneyCollectionEndDateTypeId_ddl = nomService.GetDropDownList<MoneyCollectionEndDateType>();
            ViewBag.CurrencyId_ddl = nomService.GetDropDownList<Currency>();
        }

        /// <summary>
        /// Добавяне на Вземане към обстоятелство по заповедни производства
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="moneyClaimId"></param>
        /// <param name="mainMoneyCollectionId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult CaseMoneyCollection(int caseId, int? moneyClaimId, int? mainMoneyCollectionId, int? id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseMoneyCollection, (id > 0) ? id : null, (id > 0) ? AuditConstants.Operations.Update : AuditConstants.Operations.Append, moneyClaimId))
            {
                return Redirect_Denied();
            }

            CaseMoneyCollectionEditVM model;
            if (id > 0)
            {
                model = service.GetById_EditVM(id ?? 0);
            }
            else
            {
                model = new CaseMoneyCollectionEditVM()
                {
                    CaseId = caseId,
                    CourtId = userContext.CourtId,
                    CaseMoneyClaimId = moneyClaimId ?? 0,
                    CurrencyId = NomenclatureConstants.Currency.BGN,
                    MainCaseMoneyCollectionId = mainMoneyCollectionId,
                    CaseMoneyCollectionGroupId = NomenclatureConstants.CaseMoneyCollectionGroup.Money,
                    CasePersonListDecimals = service.FillPersonList(caseId, null),
                    JointDistribution = true,
                    MoneyCollectionEndDateTypeId = NomenclatureConstants.MoneyCollectionEndDateType.WithDate
                };
            }

            SetViewBag_MoneyCollection();
            return PartialView(model);
        }

        /// <summary>
        /// Валидация преди запис на Вземане към обстоятелство по заповедни производства
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string ValidateCaseMoneyCollection(CaseMoneyCollectionEditVM model)
        {
            if (model.CaseMoneyCollectionGroupId < 1)
                return "Изберете вид.";

            switch (model.CaseMoneyCollectionGroupId)
            {
                case NomenclatureConstants.CaseMoneyCollectionGroup.Money:
                    {
                        if (model.MainCaseMoneyCollectionId < 1)
                        {
                            if (model.Money_CaseMoneyCollectionTypeId < 1)
                                return "Изберете тип.";
                        }

                        if (model.CurrencyId < 1)
                            return "Изберете Валута.";

                        //if (model.InitialAmount < (decimal)0.01)
                        //    return "Въведете първоначална стойност.";

                        //if (model.PretendedAmount < (decimal)0.01)
                        //    return "Въведете претендирано.";

                        if (model.DateFrom == null)
                            return "Въведете начална дата.";

                        if (model.MoneyCollectionEndDateTypeId == NomenclatureConstants.MoneyCollectionEndDateType.WithDate)
                        {
                            if (model.DateTo == null)
                                return "Въведете крайна дата.";
                        }

                        if (model.RespectedAmount > model.PretendedAmount)
                        {
                            return "Сумата от уважаването е по-голяма от претендираната.";
                        }

                        if (!model.JointDistribution)
                        {
                            if (!model.CasePersonListDecimals.Any(x => x.ValueOne > (decimal)0.01))
                            {
                                return "Няма въведени стойности в разпределение на вземането.";
                            }

                            if (model.CasePersonListDecimals.Sum(x => x.ValueOne) > model.PretendedAmount)
                            {
                                return "Сумата на разпределението е по-голяма от претендираната сума.";
                            }

                            if (model.CasePersonListDecimals.Sum(x => x.ValueTwo) != model.RespectedAmount)
                            {
                                return "Сумата на разпределението за уважаване е различна от общо уважената сума.";
                            }

                            if (model.CasePersonListDecimals.Sum(x => x.ValueOne) < model.CasePersonListDecimals.Sum(x => x.ValueTwo))
                            {
                                return "Сумата на разпределението за уважаване е по-голяма от сумата от разпределението за претендирането";
                            }
                        }
                    }
                    break;
                case NomenclatureConstants.CaseMoneyCollectionGroup.Property:
                    {
                        if (model.CurrencyId < 1)
                            return "Изберете Валута.";

                        if (model.Amount < (decimal)0.01)
                            return "Въведете сума.";

                        if ((model.Description ?? string.Empty) == string.Empty)
                            return "Въведете описание.";
                    }
                    break;
                case NomenclatureConstants.CaseMoneyCollectionGroup.Movables:
                    {
                        if (model.CurrencyId < 1)
                            return "Изберете Валута.";

                        if (model.Amount < (decimal)0.01)
                            return "Въведете сума.";

                        if (model.Movables_CaseMoneyCollectionTypeId < 1)
                            return "Изберете от какво произтича задължението за предаване.";

                        if ((model.Description ?? string.Empty) == string.Empty)
                            return "Въведете описание.";
                    }
                    break;
            }

            if (!CheckCourtIdFromCase(model.CaseId))
            {
                return $"Съда е променен на {userContext.CourtName}. Презаредете текущия екран.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Запис на Вземане към обстоятелство по заповедни производства
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CaseMoneyCollection(CaseMoneyCollectionEditVM model)
        {
            string validationError = ValidateCaseMoneyCollection(model);
            if (!string.IsNullOrEmpty(validationError))
                return Json(new { result = false, message = validationError });

            var currentId = model.Id;
            var res = service.CaseMoneyCollection_SaveData(model);
            if (res > 0)
                SetAuditContext(service, SourceTypeSelectVM.CaseMoneyCollection, res, currentId == 0);

            return Json(new { result = res });
        }

        /// <summary>
        /// Изтриване на Вземане към обстоятелство по заповедни производства
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CaseMoneyCollection_Delete(int Id)
        {
            CheckAccess(service, SourceTypeSelectVM.CaseMoneyCollection, Id, AuditConstants.Operations.Delete);
            return Json(new { result = service.CaseMoneyCollection_DeleteData(Id) });
        }

        public IActionResult CaseMoneyCollectionSetRespectAmount(int caseId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseMoneyCollection, null, AuditConstants.Operations.Update, caseId))
            {
                return Redirect_Denied();
            }

            List<CaseMoneyCollectionRespectSumVM> model = service.FillCaseMoneyCollectionRespectSum(caseId);
            
            return PartialView(model);
        }

        #endregion

        #region CaseMoneyExpense

        private void SetViewBag_MoneyExpense()
        {
            ViewBag.CaseMoneyExpenseTypeId_ddl = nomService.GetDropDownList<CaseMoneyExpenseType>();
            ViewBag.CurrencyId_ddl = nomService.GetDropDownList<Currency>();
        }

        /// <summary>
        /// Валидация преди запис на Претендиран разноски по заповедни производства
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string ValidateMoneyExpense(CaseMoneyExpenseEditVM model)
        {
            if (model.CaseMoneyExpenseTypeId < 1)
            {
                return "Изберете вид.";
            }

            if (model.CurrencyId < 1)
            {
                return "Изберете валута.";
            }

            if (model.Amount < (decimal)0.01)
            {
                return "Въведете сума.";
            }

            if (!(model.JointDistribution ?? true))
            {
                if (!model.CasePersonListDecimals.Any(x => x.ValueOne > (decimal)0.01))
                {
                    return "Няма въведени стойности в разпределение на вземането.";
                }

                if (model.CasePersonListDecimals.Sum(x => x.ValueOne) != model.Amount)
                {
                    return "Сумата на разпределението е различна от претендираната сума.";
                }
            }

            if (!CheckCourtIdFromCase(model.CaseId))
            {
                return $"Съда е променен на {userContext.CourtName}. Презаредете текущия екран.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Добавяне на Претендиран разноски по заповедни производства
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult CaseMoneyExpense(int caseId, int? id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseMoneyExpense, (id > 0) ? id : null, (id > 0) ? AuditConstants.Operations.Update : AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }

            CaseMoneyExpenseEditVM model;
            if (id > 0)
            {
                model = service.GetById_ExpenseEditVM(id ?? 0);
            }
            else
            {
                model = new CaseMoneyExpenseEditVM()
                {
                    CaseId = caseId,
                    CourtId = userContext.CourtId,
                    CurrencyId = NomenclatureConstants.Currency.BGN,
                    JointDistribution = true,
                    CasePersonListDecimals = service.FillPersonListExpense(caseId, null)
                };
            }

            SetViewBag_MoneyExpense();
            return PartialView(model);
        }

        /// <summary>
        /// Запис на Претендиран разноски по заповедни производства
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CaseMoneyExpense(CaseMoneyExpenseEditVM model)
        {
            string validationError = ValidateMoneyExpense(model);
            if (!string.IsNullOrEmpty(validationError))
            {
                return Json(new { result = false, message = validationError });
            }

            var currentId = model.Id;
            var res = service.CaseMoneyExpense_SaveData(model);
            if (res > 0)
                SetAuditContext(service, SourceTypeSelectVM.CaseMoneyExpense, res, currentId == 0);

            return Json(new { result = res });
        }

        /// <summary>
        /// Изтриване на Претендиран разноски по заповедни производства
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CaseMoneyExpense_Delete(int Id)
        {
            CheckAccess(service, SourceTypeSelectVM.CaseMoneyExpense, Id, AuditConstants.Operations.Delete);
            return Json(new { result = service.CaseMoneyExpense_DeleteData(Id) });
        }

        #endregion

        #region CaseFastProcess

        private string ValidateCaseFastProcess(CaseFastProcessEditVM model)
        {
            return string.Empty;
        }

        public IActionResult CaseFastProcess(int caseId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseFastProcess, null, AuditConstants.Operations.Update, caseId))
            {
                return Redirect_Denied();
            }

            CaseFastProcessEditVM model = service.GetByCaseId_CaseFastProcess(caseId);
            if (model.Id < 1)
            {
                model = new CaseFastProcessEditVM()
                {
                    CaseId = caseId,
                    CourtId = userContext.CourtId
                };
            }
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult CaseFastProcess(CaseFastProcessEditVM model)
        {
            string validationError = ValidateCaseFastProcess(model);
            if (!string.IsNullOrEmpty(validationError))
            {
                return Json(new { result = false, message = validationError });
            }

            var currentId = model.Id;
            var res = service.CaseFastProcess_SaveData(model);
            if (res)
                SetAuditContext(service, SourceTypeSelectVM.CaseFastProcess, model.Id, currentId == 0);
            
            return Json(new { result = res });
        }

        #endregion

        #region Other

        private bool CheckCourtIdFromCase(int CaseId)
        {
            var caseCase = service.GetById<Case>(CaseId);
            return caseCase.CourtId == userContext.CourtId;
        }

        #endregion
    }
}