using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Money;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Transactions;

namespace IOWebApplication.Core.Services
{
    public class MoneyService : BaseService, IMoneyService
    {
        private readonly ICounterService counterService;
        private readonly ICommonService commonService;

        public MoneyService(
            ILogger<MoneyService> _logger,
            ICounterService _counterService,
            IRepository _repo,
            IUserContext _userContext,
            ICommonService _commonService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            counterService = _counterService;
            commonService = _commonService;
        }

        /// <summary>
        /// Извличане на задължения за Datatable
        /// </summary>
        /// <param name="caseSessionActId"></param>
        /// <param name="documentId"></param>
        /// <param name="caseSessionId"></param>
        /// <param name="courtId"></param>
        /// <returns></returns>
        public IQueryable<ObligationVM> Obligation_Select(int caseSessionActId, long documentId, int caseSessionId, int courtId)
        {
            Expression<Func<Obligation, bool>> idWhere = x => true;
            if (caseSessionActId > 0)
                idWhere = x => x.CaseSessionActId == caseSessionActId;
            else if (documentId > 0)
                idWhere = x => x.DocumentId == documentId;
            else if (caseSessionId > 0)
                idWhere = x => (x.CaseSessionId == caseSessionId || x.CaseSessionAct.CaseSessionId == caseSessionId);


            return repo.AllReadonly<Obligation>()
           .Where(x => x.CourtId == courtId)
           .Where(idWhere)
           .Select(x => new ObligationVM()
           {
               Id = x.Id,
               ObligationNumber = x.ObligationNumber,
               ObligationDate = x.ObligationDate,
               CasePersonUic = x.Uic,
               CasePersonName = x.FullName,
               MoneyTypeName = x.MoneyType.Label,
               Amount = x.Amount,
               AmountPay = x.ObligationPayments.Where(a => a.IsActive == true).Select(a => a.Amount).DefaultIfEmpty(0).Sum(),
               IsActive = x.IsActive ?? true,
               RegNumberExpenseOrder = caseSessionId > 0 ? x.ExpenseOrderObligations.Where(o => o.ExpenseOrder.IsActive == true).Select(o => o.ExpenseOrder.RegNumber).DefaultIfEmpty("").FirstOrDefault() : "",
               RegNumberExecList = (caseSessionActId > 0 || caseSessionId > 0) ? x.ExecListObligations.Where(o => o.ExecList.IsActive == true).Select(o => o.ExecList.RegNumber ?? "В проект").DefaultIfEmpty("").FirstOrDefault() : "",
               ExecListId = (caseSessionActId > 0 || caseSessionId > 0) ? x.ExecListObligations.Where(o => o.ExecList.IsActive == true).Select(o => o.ExecListId).FirstOrDefault() : 0,
               ExpenseOrderId = caseSessionId > 0 ? x.ExpenseOrderObligations.Where(o => o.ExpenseOrder.IsActive == true).Select(o => o.ExpenseOrderId).FirstOrDefault() : 0,
           }).AsQueryable();
        }

        /// <summary>
        /// Зареждане на данни за получател на задължението
        /// </summary>
        /// <param name="model"></param>
        /// <param name="saved"></param>
        private void SetObligationReceiveData(ObligationEditVM model, ObligationReceive saved)
        {
            saved.ExecListTypeId = model.ExecListTypeId;
            if ((model.ExecListTypeId??0) == NomenclatureConstants.ExecListTypes.Country)
            {
                var entityData = commonService.SelectEntity_Select(model.ReceiveSourceTypeId, null, null, model.ReceiveSourceId).FirstOrDefault();
                if (entityData != null)
                {
                    saved.Person_SourceType = model.ReceiveSourceTypeId;
                    saved.Person_SourceId = model.ReceiveSourceId;
                    saved.FullName = entityData.Label;
                    saved.UicTypeId = entityData.UicTypeId;
                    saved.Uic = entityData.Uic;
                }

                saved.CasePersonId = null;
                saved.Iban = model.Iban;
                saved.BIC = model.BIC;
                saved.BankName = model.BankName;
            }
            else if ((model.ExecListTypeId ?? 0) == NomenclatureConstants.ExecListTypes.ThirdPerson)
            {
                CasePerson casePerson = GetById<CasePerson>(model.PersonReceiveId);
                saved.CopyFrom(casePerson);
                saved.CasePersonId = model.PersonReceiveId;
            }
        }

        /// <summary>
        /// Запис на получател на задължението
        /// </summary>
        /// <param name="model"></param>
        /// <param name="obligation"></param>
        private void Obligation_SaveReceive(ObligationEditVM model, Obligation obligation)
        {
            model.ExecListTypeId = (model.ExecListTypeId ?? 0) <= 0 ? null : model.ExecListTypeId;
            if ((model.ExecListTypeId ?? 0) > 0)
            {
                var saved = obligation.ObligationReceives.FirstOrDefault();
                if (saved != null)
                {
                    SetObligationReceiveData(model, saved);
                    repo.Update<ObligationReceive>(saved);
                }
                else
                {
                    ObligationReceive addNew = new ObligationReceive();
                    SetObligationReceiveData(model, addNew);
                    obligation.ObligationReceives.Add(addNew);
                }
            }
            else
            {
                var saved = obligation.ObligationReceives;
                if (saved.Count > 0)
                {
                    repo.DeleteRange<ObligationReceive>(saved);
                }
            }
        }

        /// <summary>
        /// Запис на задължение
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) Obligation_SaveData(ObligationEditVM model)
        {
            try
            {
                model.CaseSessionActId = (model.CaseSessionActId ?? 0) <= 0 ? null : model.CaseSessionActId;
                model.DocumentId = (model.DocumentId ?? 0) <= 0 ? null : model.DocumentId;
                model.CaseSessionId = (model.CaseSessionId ?? 0) <= 0 ? null : model.CaseSessionId;
                model.MoneyFeeTypeId = (model.MoneyFeeTypeId ?? 0) <= 0 ? null : model.MoneyFeeTypeId;
                model.Person_SourceType = (model.Person_SourceType ?? 0) <= 0 ? null : model.Person_SourceType;
                
                model.MoneyFineTypeId = model.MoneyFineTypeId.EmptyToNull();
                //Това е пояснение само за глоби
                if (model.MoneyTypeId != NomenclatureConstants.MoneyType.Fine)
                    model.MoneyFineTypeId = null;

                if (model.ExecListTypeId == NomenclatureConstants.ExecListTypes.ThirdPerson)
                    model.MoneySign = 0;

                var moneyType = repo.GetById<MoneyType>(model.MoneyTypeId);
                if ((moneyType.NoMoney ?? false) == true)
                    model.Amount = 0;

                Obligation saved = null;
                bool isDeactivate = false;
                if (model.Id > 0)
                {
                    //Update
                    saved = repo.All<Obligation>()
                               .Include(x => x.ObligationReceives)
                               .Where(x => x.Id == model.Id)
                               .FirstOrDefault();

                    isDeactivate = (saved.IsActive ?? true) == true && model.IsActive == false;
                    
                    if ((saved.IsActive ?? true) == false)
                    {
                        return (result: false, errorMessage: "Задължението е деактивирано");
                    }

                    bool hasExecList = repo.AllReadonly<ExecListObligation>()
                                        .Where(x => x.ObligationId == saved.Id && x.ExecList.IsActive == true)
                                        .Any();
                    if (hasExecList)
                    {
                        return (result: false, errorMessage: "Задължението е влезнало в ИЛ");
                    }

                    bool hasExpenseOrder = repo.AllReadonly<ExpenseOrderObligation>()
                                        .Where(x => x.ObligationId == saved.Id && x.ExpenseOrder.IsActive == true)
                                        .Any();
                    if (hasExpenseOrder)
                    {
                        return (result: false, errorMessage: "Задължението е влезнало в Разходен ордер");
                    }
                }
                else
                {
                    //Insert
                    saved = new Obligation();
                    saved.CourtId = model.CourtId;
                    saved.CaseSessionActId = model.CaseSessionActId;
                    saved.DocumentId = model.DocumentId;
                    saved.CaseSessionId = model.CaseSessionId;

                    if (counterService.Counter_GetObligationCounter(saved) == false)
                    {
                        return (result: false, errorMessage: "Проблем при взимане на брояч");
                    }
                }

                saved.MoneyTypeId = model.MoneyTypeId;
                saved.Amount = model.Amount;
                saved.Description = model.Description;
                saved.MoneyFeeTypeId = model.MoneyFeeTypeId;
                saved.MoneyFineTypeId = model.MoneyFineTypeId;
                saved.MoneySign = model.MoneySign;
                saved.IsActive = model.IsActive;
                saved.UserId = userContext.UserId;
                saved.DateWrt = DateTime.Now;

                Obligation_SaveReceive(model, saved);
                if (model.Person_SourceType == SourceTypeSelectVM.CaseLawUnit)
                {
                    var caseLawUnit = repo.AllReadonly<CaseLawUnit>().Include(x => x.LawUnit).Where(x => x.Id == model.Person_SourceId).FirstOrDefault();
                    saved.PersonId = caseLawUnit.LawUnit.PersonId;
                    saved.CopyFrom(caseLawUnit.LawUnit, false);
                }
                else if (model.Person_SourceType == SourceTypeSelectVM.CasePerson)
                {
                    var casePerson = repo.AllReadonly<CasePerson>().Where(x => x.Id == model.Person_SourceId).FirstOrDefault();
                    saved.PersonId = casePerson.PersonId;
                    saved.CopyFrom(casePerson, false);
                }
                else if (model.Person_SourceType == SourceTypeSelectVM.DocumentPerson)
                {
                    var documentPerson = repo.AllReadonly<DocumentPerson>().Where(x => x.Id == model.Person_SourceId).FirstOrDefault();
                    saved.PersonId = documentPerson.PersonId;
                    saved.CopyFrom(documentPerson, false);
                }
                else
                {
                    return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
                }

                //Сетване на данни в зависомост от това дали е от документ или акт
                if (model.CaseSessionActId != null)
                {
                    var actInfo = repo.AllReadonly<CaseSessionAct>()
                                         .Include(x => x.ActType)
                                         .Where(x => x.Id == model.CaseSessionActId).FirstOrDefault();
                    string actDate = "";
                    if (actInfo.ActDate != null)
                        actDate = (actInfo.ActDate ?? DateTime.Now).ToString("dd.MM.yyyy");
                    saved.ObligationInfo = actInfo.ActType.Label + " " + actInfo.RegNumber + "/" + actDate;
                    saved.CaseId = actInfo.CaseId;
                }
                else if (model.DocumentId != null)
                {
                    var documentInfo = repo.AllReadonly<Document>()
                                               .Include(x => x.DocumentGroup)
                                               .Where(x => x.Id == model.DocumentId).FirstOrDefault();
                    saved.ObligationInfo = documentInfo.DocumentGroup.Label + " " + documentInfo.DocumentNumber + "/" +
                                           documentInfo.DocumentDate.ToString("dd.MM.yyyy");

                    saved.CaseId = repo.AllReadonly<Case>()
                                        .Where(x => x.DocumentId == model.DocumentId)
                                        .Select(x => (Int32?)x.Id)
                                        .FirstOrDefault();
                    if (saved.CaseId == null)
                    {
                        saved.CaseId = repo.AllReadonly<DocumentCaseInfo>()
                                            .Where(x => x.DocumentId == model.DocumentId)
                                            .Select(x => x.CaseId)
                                            .FirstOrDefault();
                    }
                }
                else if (model.CaseSessionId != null)
                {
                    var caseSessionInfo = repo.AllReadonly<CaseSession>()
                                               .Include(x => x.SessionType)
                                               .Where(x => x.Id == model.CaseSessionId).FirstOrDefault();
                    saved.ObligationInfo = caseSessionInfo.SessionType.Label + " " + caseSessionInfo.DateFrom.ToString("dd.MM.yyyy");
                    saved.CaseId = caseSessionInfo.CaseId;
                }

                saved.Person_SourceId = model.Person_SourceId;
                if (model.Id > 0)
                {
                    //Update
                    using (TransactionScope ts = new TransactionScope())
                    {
                        repo.Update(saved);
                        repo.SaveChanges();

                        //Ако са направили сумата неактивна и тя е за заседател да преизчисли минималната сума на ден
                        if ((saved.CaseSessionMeetingId ?? 0) > 0 && isDeactivate == true)
                        {
                            var courtJuryFee = repo.AllReadonly<CourtJuryFee>().Where(x => x.CourtId == saved.CourtId).ToList();

                            var caseLawUnitJury = repo.AllReadonly<CaseLawUnit>().Include(x => x.LawUnit)
                                 .Where(x => x.CaseSessionId == saved.CaseSessionId &&
                                           x.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Jury &&
                                           x.Id == saved.Person_SourceId)
                                 .FirstOrDefault();
                            List<CaseLawUnit> caseLawUnitJuries = new List<CaseLawUnit>();
                            caseLawUnitJuries.Add(caseLawUnitJury);

                            List<Obligation> moneys = new List<Obligation>();
                            List<DateTime> dates = new List<DateTime>();
                            dates.Add(saved.ObligationDate.Date);
                            ObligationMinAmountForday_SaveData(saved.CaseSessionId ?? 0, dates,
                                caseLawUnitJuries, courtJuryFee, moneys, saved.CourtId, true);

                            repo.SaveChanges();
                        }

                        ts.Complete();
                    }
                }
                else
                {
                    saved.Person_SourceType = model.Person_SourceType;

                    //Insert
                    repo.Add<Obligation>(saved);
                    repo.SaveChanges();
                }
                model.Id = saved.Id;
                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Obligation Id={ model.Id }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Извличане на данни за задължение
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ObligationEditVM Obligation_GetById(int id)
        {
            var result = new ObligationEditVM();

            var item = repo.AllReadonly<Obligation>().Where(x => x.Id == id)
                .Include(x => x.ObligationReceives)
                .Include(x => x.CaseSessionAct)
                .Include(x => x.CaseSessionAct.CaseSession)
                .Include(x => x.Document)
                .FirstOrDefault();

            result.Id = item.Id;
            result.CourtId = item.CourtId;
            result.CaseSessionActId = item.CaseSessionActId;
            result.DocumentId = item.DocumentId;
            result.CaseSessionId = item.CaseSessionId;
            result.MoneyTypeId = item.MoneyTypeId;
            result.Amount = item.Amount;
            result.Description = item.Description;
            result.MoneyFeeTypeId = item.MoneyFeeTypeId;
            result.MoneySign = item.MoneySign ?? 0;
            result.Person_SourceId = item.Person_SourceId;
            result.Person_SourceType = item.Person_SourceType;
            result.IsActive = item.IsActive ?? true;
            result.MoneyFineTypeId = item.MoneyFineTypeId;

            //Това е за получателите на парите
            var receive = item.ObligationReceives.FirstOrDefault();
            if (receive != null) {
                result.ExecListTypeId = receive.ExecListTypeId;
                result.PersonReceiveId = receive.CasePersonId;
                result.Iban = receive.Iban;
                result.BIC = receive.BIC;
                result.BankName = receive.BankName;
                if (receive.Person_SourceType != null) {
                    result.ReceiveSourceTypeId = receive.Person_SourceType ?? 0;
                    result.ReceiveSourceId = receive.Person_SourceId;
                }
            }

            return result;
        }

        /// <summary>
        /// Извличане на дължими суми за Datatable
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<ObligationForPayVM> ObligationForPay_Select(int courtId, ObligationForPayFilterVM model)
        {
            Expression<Func<ObligationForPayVM, bool>> statusWhere = x => true;
            if (model.Status == MoneyConstants.ObligationStatus.StatusPaid)
                statusWhere = x => Math.Abs(x.Amount - x.AmountPay) < 0.001M;
            else if (model.Status == MoneyConstants.ObligationStatus.StatusNotEnd)
                statusWhere = x => Math.Abs(x.Amount - x.AmountPay) > 0;

            Expression<Func<Obligation, bool>> uicSearch = x => true;
            if (!string.IsNullOrEmpty(model.PersonUicSearch))
                uicSearch = x => x.Uic.ToLower() == model.PersonUicSearch.ToLower();

            Expression<Func<Obligation, bool>> nameSearch = x => true;
            if (!string.IsNullOrEmpty(model.PersonNameSearch))
                nameSearch = x => x.FullName.ToLower().Contains(model.PersonNameSearch.ToLower());

            Expression<Func<ObligationForPayVM, bool>> expenseOrderNumberWhere = x => true;
            if (!string.IsNullOrEmpty(model.ExpenseOrderSearch))
                expenseOrderNumberWhere = x => x.RegNumberExpenseOrder.Contains(model.ExpenseOrderSearch);

            Expression<Func<Obligation, bool>> signWhere = x => true;
            if (model.Sign != 0)
                signWhere = x => x.MoneySign == model.Sign;

            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<Obligation, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.ObligationDate.Date >= dateFromSearch.Date && x.ObligationDate.Date <= dateToSearch.Date;

            Expression<Func<Obligation, bool>> moneyTypeWhere = x => true;
            if (model.MoneyTypeId > 0)
                moneyTypeWhere = x => x.MoneyTypeId == model.MoneyTypeId;

            Expression<Func<Obligation, bool>> caseRegnumberSearch = x => true;
            if (!string.IsNullOrEmpty(model.CaseRegNumber))
                caseRegnumberSearch = x => x.Case.RegNumber.ToLower().Contains(model.CaseRegNumber.ToLower());

            return repo.AllReadonly<Obligation>()
           .Where(x => x.CourtId == courtId && (x.IsActive ?? true))
           .Where(uicSearch)
           .Where(nameSearch)
           .Where(signWhere)
           .Where(dateSearch)
           .Where(moneyTypeWhere)
           .Where(caseRegnumberSearch)
           .Where(x => x.ObligationReceives.Where(a => a.ExecListTypeId == NomenclatureConstants.ExecListTypes.ThirdPerson).Any() == false)
           .Select(x => new ObligationForPayVM()
           {
               Id = x.Id,
               ObligationNumber = x.ObligationNumber ?? "",
               ObligationDate = x.ObligationDate,
               PersonUic = x.Uic ?? "",
               PersonName = x.FullName ?? "",
               MoneyTypeName = x.MoneyType.Label,
               Amount = x.Amount,
               AmountPay = x.ObligationPayments.Where(a => a.IsActive == true).Select(a => a.Amount).DefaultIfEmpty(0).Sum(),
               CaseData = x.Case.CaseGroup.Code + " " + x.Case.RegNumber,
               ObligationInfo = x.ObligationInfo ?? "",
               RegNumberExpenseOrder = model.Sign == NomenclatureConstants.MoneySign.SignPlus ? "" : x.ExpenseOrderObligations.Where(o => o.ExpenseOrder.IsActive == true).Select(o => o.ExpenseOrder.RegNumber).DefaultIfEmpty("").FirstOrDefault(),
               RegNumberExecList = model.Sign == NomenclatureConstants.MoneySign.SignMinus ? "" : x.ExecListObligations.Where(o => o.ExecList.IsActive == true).Select(o => o.ExecList.RegNumber ?? "В проект").DefaultIfEmpty("").FirstOrDefault(),
               ExecListId = model.Sign == NomenclatureConstants.MoneySign.SignMinus ? 0 : x.ExecListObligations.Where(o => o.ExecList.IsActive == true).Select(o => o.ExecListId).FirstOrDefault(),
               ExpenseOrderId = model.Sign == NomenclatureConstants.MoneySign.SignPlus ? 0 : x.ExpenseOrderObligations.Where(o => o.ExpenseOrder.IsActive == true).Select(o => o.ExpenseOrderId).FirstOrDefault(),
           }).Where(statusWhere).Where(expenseOrderNumberWhere).AsQueryable();
        }

        /// <summary>
        /// Извличане на сума за плащане за избрани задължения
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public decimal GetSumForPay(string ids)
        {
            List<string> idList = new List<string>();
            idList = ids.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
            return repo.AllReadonly<Obligation>()
           .Include(x => x.ObligationPayments)
           .Where(x => idList.Contains(x.Id.ToString()))
           .Select(x => x.Amount - x.ObligationPayments.Where(a => a.IsActive == true).Select(a => a.Amount).DefaultIfEmpty(0).Sum()).Sum();
        }

        /// <summary>
        /// Зареждане на данни за плащане
        /// </summary>
        /// <param name="obligationPayment"></param>
        /// <param name="paySum"></param>
        /// <param name="obligation"></param>
        private void FillObjectPay(ObligationPayment obligationPayment, ref decimal paySum, ObligationForPayVM obligation)
        {
            obligationPayment.ObligationId = obligation.Id;
            obligationPayment.IsActive = true;

            if (paySum < (decimal)-0.001 && obligation.AmountPay > (decimal)0.001)
            {
                decimal dPaySum = 0;
                dPaySum = Math.Min(Math.Abs(paySum), obligation.AmountPay);
                obligationPayment.Amount = -1 * dPaySum;
                paySum += dPaySum;
            }
            else
            {
                if (paySum < (decimal)0.001 && obligation.AmountForPay < (decimal)0.001)
                {
                    if (Math.Abs(paySum) - Math.Abs(obligation.AmountForPay) > (decimal)0.001)
                    {
                        obligationPayment.Amount = obligation.AmountForPay;
                        paySum -= obligationPayment.Amount;
                    }
                    else
                    {
                        obligationPayment.Amount = paySum;
                        paySum = 0;
                    }
                }
                else
                {
                    if (paySum - obligation.AmountForPay > (decimal)0.001)
                    {
                        obligationPayment.Amount = obligation.AmountForPay;
                    }
                    else
                    {
                        obligationPayment.Amount = paySum;
                    }
                    paySum -= obligationPayment.Amount;
                }
            }
        }

        private void MakePay(ObligationPayment obligationPayment, ref decimal paySum, ObligationForPayVM obligation, List<ObligationPayment> oblPayments)
        {
            if (obligationPayment == null)
                obligationPayment = new ObligationPayment();
            FillObjectPay(obligationPayment, ref paySum, obligation);
            obligation.AmountPay += obligationPayment.Amount;
            oblPayments.Add(obligationPayment);
        }

        /// <summary>
        /// Плащане на избрани задължения
        /// </summary>
        /// <param name="obligations"></param>
        /// <param name="paySum"></param>
        /// <param name="oblPayments"></param>
        /// <returns></returns>
        public bool SetFieldsPay(List<ObligationForPayVM> obligations, ref decimal paySum, List<ObligationPayment> oblPayments)
        {
            if (obligations.Count == 0) return true;

            //Плащане на минусите
            if (paySum > (decimal)-0.001)
            {
                decimal dPaySum = 0;

                var moneyMinus_S = (from moneyminus in obligations
                                    where moneyminus.Amount < 0 &&
                                          moneyminus.AmountPay < (decimal)0.001 &&
                                          moneyminus.Amount - moneyminus.AmountPay < (decimal)-0.001
                                    select moneyminus).ToList();

                foreach (var tempMoney in moneyMinus_S)
                {
                    dPaySum = tempMoney.AmountForPay; //Това е максималната сума за която може да е минусовото задължение
                    paySum += Math.Abs(dPaySum);

                    //Попълване на плащането
                    MakePay(null, ref dPaySum, tempMoney, oblPayments);
                }
            }


            //Ако платената сума е минусова да мине първо през всички минуси
            if (paySum < (decimal)-0.001)
            {
                var moneyMinus_S = (from moneyminus in obligations
                                    where moneyminus.Amount < (decimal)-0.001 &&
                                    moneyminus.Amount - moneyminus.AmountPay < (decimal)-0.001
                                    select moneyminus).ToList();

                foreach (var tempMoney in moneyMinus_S)
                {
                    //Попълване на плащането
                    MakePay(null, ref paySum, tempMoney, oblPayments);
                }
            }

            //Ако платената сума все още е минусова да мине през всички където има надплащане
            if (paySum < (decimal)-0.001)
            {
                var moneyMinus_S = (from moneyminus in obligations
                                    where moneyminus.AmountPay > (decimal)0.001 &&
                                          moneyminus.AmountPay - moneyminus.Amount > (decimal)0.001
                                    orderby moneyminus.AmountPay - moneyminus.Amount
                                    select moneyminus).ToList();

                foreach (var tempMoney in moneyMinus_S)
                {
                    //Попълване на плащането
                    MakePay(null, ref paySum, tempMoney, oblPayments);
                }
            }

            //Ако платената сума все още е минусова да мине през всички където има плащане
            if (paySum < (decimal)-0.001)
            {
                var moneyMinus_S = (from moneyminus in obligations
                                    where moneyminus.AmountPay > (decimal)0.001
                                    orderby moneyminus.Amount - moneyminus.AmountPay
                                    select moneyminus).ToList();

                foreach (var tempMoney in moneyMinus_S)
                {
                    //Попълване на плащането
                    MakePay(null, ref paySum, tempMoney, oblPayments);
                }
            }

            foreach (var tempMoney in obligations)
            {
                if (tempMoney.Amount - tempMoney.AmountPay < (decimal)0.001) continue;
                if (Math.Abs(paySum) < (decimal)0.001)
                    break;

                //Попълване на плащането
                MakePay(null, ref paySum, tempMoney, oblPayments);
            }

            //Ако са останали пари за плащане, взима се плащане с положителна сума и се слагат парите там
            if (Math.Abs(paySum) > (decimal)0.001)
            {
                for (int j = oblPayments.Count - 1; j >= 0; --j)
                {
                    ObligationPayment tempPay = oblPayments[j];
                    if (tempPay.Amount > (decimal)0.001)
                    {
                        tempPay.Amount += paySum;
                        paySum = 0;
                        break;
                    }
                }
            }

            //Ако са останали пари и няма плащания да направи плащане към някое от задълженията.
            if (Math.Abs(paySum) > (decimal)0.001)
            {
                if (paySum < (decimal)-0.001)
                {
                    foreach (var tempMoney in obligations)
                    {
                        if (tempMoney.AmountForPay < (decimal)-0.001)
                        {
                            ObligationPayment tempPay = new ObligationPayment();
                            MakePay(tempPay, ref paySum, tempMoney, oblPayments);
                            tempPay.Amount += paySum;
                            paySum = 0;
                            break;
                        }
                    }
                }
                else
                {
                    for (int j = obligations.Count - 1; j >= 0; j--)
                    {
                        if (obligations[j].Amount < (decimal)-0.001) continue;

                        ObligationPayment tempPay = new ObligationPayment();
                        MakePay(tempPay, ref paySum, obligations[j], oblPayments);
                        tempPay.Amount += paySum;
                        paySum = 0;
                        break;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Зареждане на данни за плащане
        /// </summary>
        /// <param name="model"></param>
        /// <param name="saved"></param>
        private void SetDataPaymentFromVM(PaymentVM model, Payment saved)
        {
            //Ако е редакция да не пипа тези стойности
            if (model.Id <= 0)
            {
                saved.PaymentTypeId = model.PaymentTypeId;
                saved.Amount = model.Amount;
                saved.PaidDate = model.PaidDate;
                saved.CourtBankAccountId = model.CourtBankAccountId;
            }

            //Ако е ПОС и е ново плащане да сетне днешна дата, а ако е редакция да не я пипа
            if (model.PaymentTypeId == NomenclatureConstants.PaymentType.Pos || model.PaymentTypeId == NomenclatureConstants.PaymentType.Cash)
            {
                if (model.Id <= 0)
                    saved.PaidDate = DateTime.Now;
            }

            saved.SenderName = model.SenderName;
            saved.Description = model.Description;
            saved.UserId = userContext.UserId;
            saved.DateWrt = DateTime.Now;

            //Ако е банка да сет тези пропъртита
            if (model.PaymentTypeId == NomenclatureConstants.PaymentType.Bank)
            {
                saved.PaymentInfo = model.PaymentInfo;
                saved.PaymentDescription = model.PaymentDescription;
            }
        }

        /// <summary>
        /// Запис на плащане
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) MakePayment(PaymentVM model)
        {
            try
            {
                model.CourtBankAccountId = model.CourtBankAccountId.NumberEmptyToNull();
                var obligationForPay = ObligationForPayByIds_Select(model.ObligationIds);

                //Ако задълженията са платени да не могат да се надплатят
                decimal sumForPay = obligationForPay.Select(x => x.AmountForPay).Sum();
                if (model.Amount > 0.001M && sumForPay < 0.001M)
                {
                    return (result: false, errorMessage: "Задължението е платено");
                }

                Payment saved = new Payment();
                SetDataPaymentFromVM(model, saved);
                saved.CourtId = model.CourtId;
                saved.IsActive = true;
                saved.IsAvans = false; //Сетва се на true при плащания без задължения

                List<ObligationPayment> payments = new List<ObligationPayment>();
                decimal paySum = saved.Amount;
                SetFieldsPay(obligationForPay, ref paySum, payments);

                if (Math.Abs(paySum) > (decimal)0.001)
                {
                    return (result: false, errorMessage: "Невалидна сума");
                }

                if (counterService.Counter_GetPaymentCounter(saved) == false)
                {
                    return (result: false, errorMessage: "Проблем при взимане номер на плащането ");
                }

                repo.Add<Payment>(saved);
                foreach (var item in payments)
                {
                    item.PaymentId = saved.Id;
                    item.UserId = userContext.UserId;
                    item.DateWrt = DateTime.Now;
                    repo.Add<ObligationPayment>(item);
                }

                //Ъпдейт на ресултата от плащането  от ПОС
                UpdatePosPaymentResult(saved.Id, model.PosPaymentResultId, model.PaymentTypeId);

                //Ъпдейт статус на разходни ордери
                MakePaidExpenseOrder(payments);

                repo.SaveChanges();

                model.Id = saved.Id;

                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на MakePayment");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Извличане на данни за избрани задължения за плащане
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<ObligationForPayVM> ObligationForPayByIds_Select(string ids)
        {
            List<string> idList = new List<string>();
            idList = ids.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();

            return repo.AllReadonly<Obligation>()
           .Include(x => x.ObligationPayments)
           .Where(x => idList.Contains(x.Id.ToString()) && (x.IsActive ?? true))
           .Select(x => new ObligationForPayVM()
           {
               Id = x.Id,
               Amount = x.Amount,
               AmountPay = x.ObligationPayments.Where(a => a.IsActive == true).Select(a => a.Amount).DefaultIfEmpty(0).Sum()
           }).ToList();
        }

        /// <summary>
        /// Извличане на групите за избрани задължения
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<int> MoneyGroup_Select(string ids)
        {
            List<string> idList = new List<string>();
            idList = ids.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();

            return repo.AllReadonly<Obligation>()
                .Include(x => x.MoneyType)
                .Where(x => idList.Contains(x.Id.ToString()))
                .Select(x => x.MoneyType.MoneyGroupId)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Извличане на данните за извършени плащания за Datatable
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<PaymentListVM> Payment_Select(int courtId, PaymentFilterVM model)
        {
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<Payment, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.PaidDate.Date >= dateFromSearch.Date && x.PaidDate.Date <= dateToSearch.Date;

            Expression<Func<Payment, bool>> moneyGroupWhere = x => true;
            if (model.MoneyGroupId > 0)
                moneyGroupWhere = x => x.CourtBankAccount.MoneyGroupId == model.MoneyGroupId;

            Expression<Func<Payment, bool>> paymentWhere = x => true;
            if (model.PaymentTypeId > 0)
                paymentWhere = x => x.PaymentTypeId == model.PaymentTypeId;

            Expression<Func<Payment, bool>> userWhere = x => true;
            if (string.IsNullOrEmpty(model.UserId) == false && model.UserId != "0")
                userWhere = x => x.UserId == model.UserId;

            Expression<Func<Payment, bool>> posDeviceWhere = x => true;
            if (string.IsNullOrEmpty(model.PosDeviceTid) == false && model.PosDeviceTid != "-1")
                posDeviceWhere = x => x.PosPaymentResults.Any(p => p.PaymentId == x.Id &&
                                                              p.Status == MoneyConstants.PosPaymentResultStatus.StatusOk &&
                                                              p.Tid == model.PosDeviceTid);

            Expression<Func<Payment, bool>> personSearch = x => true;
            if (string.IsNullOrEmpty(model.SenderName) == false)
                personSearch = x => x.SenderName.ToLower().Contains(model.SenderName.ToLower());

            Expression<Func<Payment, bool>> regNumberSearch = x => true;
            if (string.IsNullOrEmpty(model.CaseRegNumber) == false)
                regNumberSearch = x => x.ObligationPayments
                           .Where(a => a.Obligation.Case.RegNumber.ToLower().Contains(model.CaseRegNumber.ToLower())).Any();

            Expression<Func<Payment, bool>> activeWhere = x => true;
            if (model.ActivePayment)
                activeWhere = x => x.IsActive == model.ActivePayment;

            return repo.AllReadonly<Payment>()
                .Where(x => x.CourtId == courtId)
           .Where(personSearch)
           .Where(dateSearch)
           .Where(moneyGroupWhere)
           .Where(paymentWhere)
           .Where(userWhere)
           .Where(posDeviceWhere)
           .Where(regNumberSearch)
           .Where(activeWhere)
           .Select(x => new PaymentListVM()
           {
               Id = x.Id,
               MoneyGroupName = x.CourtBankAccountId == null ? "По друга сметка" : x.CourtBankAccount.MoneyGroup.Label,
               Amount = x.Amount,
               AmountPayObligation = x.ObligationPayments.Where(a => a.IsActive == true).Select(a => a.Amount).DefaultIfEmpty(0).Sum(),
               PaidDate = x.PaidDate,
               SenderName = x.SenderName,
               IsActive = x.IsActive,
               IsAvans = x.IsAvans,
               PaymentNumber = x.PaymentNumber,
               PaymentTypeName = x.PaymentType.Label,
               UserName = x.User.UserName,
               CaseNumbers = string.Join("; ", x.ObligationPayments.Where(a => a.Obligation.CaseId != null)
                               .Select(a => a.Obligation.Case.RegNumber).Distinct())
           }).AsQueryable();
        }

        /// <summary>
        /// Ъпдейт на плащането от ПОС след запис на самото плащане
        /// </summary>
        /// <param name="paymentId"></param>
        /// <param name="posPaymentResultId"></param>
        /// <param name="paymentTypeId"></param>
        private void UpdatePosPaymentResult(int paymentId, int posPaymentResultId, int paymentTypeId)
        {
            if (paymentTypeId == NomenclatureConstants.PaymentType.Pos && posPaymentResultId > 0)
            {
                PosPaymentResult posPayment = GetById<PosPaymentResult>(posPaymentResultId);
                posPayment.PaymentId = paymentId;
                repo.Update(posPayment);
            }
        }

        /// <summary>
        /// Запис на авансово плащане
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Payment_SaveData(PaymentVM model)
        {
            try
            {
                Payment saved;
                if (model.Id > 0)
                {
                    //Update
                    saved = repo.GetById<Payment>(model.Id);
                }
                else
                {
                    saved = new Payment();
                    saved.CourtId = model.CourtId;
                    saved.IsActive = true;
                    saved.IsAvans = model.IsAvans;
                }

                SetDataPaymentFromVM(model, saved);

                if (model.Id > 0)
                {
                    //Update
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    if (counterService.Counter_GetPaymentCounter(saved) == false)
                    {
                        return false;
                    }

                    //Insert
                    repo.Add<Payment>(saved);

                    //Ъпдейт на ресултата от плащането  от ПОС
                    UpdatePosPaymentResult(saved.Id, model.PosPaymentResultId, model.PaymentTypeId);

                    repo.SaveChanges();
                }
                model.Id = saved.Id;
                model.PaymentNumber = saved.PaymentNumber;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Payment Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за плащане
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PaymentVM Payment_GetById(int id)
        {
            return repo.AllReadonly<Payment>()
           .Where(x => x.Id == id)
           .Select(x => new PaymentVM()
           {
               Id = x.Id,
               PaymentTypeId = x.PaymentTypeId,
               CourtBankAccountId = x.CourtBankAccountId,
               Amount = x.Amount,
               PaidDate = x.PaidDate,
               SenderName = x.SenderName,
               PaymentInfo = x.PaymentInfo,
               PaymentDescription = x.PaymentDescription,
               Description = x.Description,
               PaymentNumber = x.PaymentNumber
           }).FirstOrDefault();
        }

        /// <summary>
        /// Сторно на плащане
        /// </summary>
        /// <param name="id"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool Payment_Storno(int id, ref string errorMessage)
        {
            try
            {
                Payment storno = repo.AllReadonly<Payment>().Where(x => x.Id == id).FirstOrDefault();
                if (storno == null)
                {
                    return false;
                }
                if (storno.IsActive == false)
                {
                    errorMessage = "Плащането вече е деактивирано";
                    return false;
                }

                var hasObligationPayment = repo.AllReadonly<ObligationPayment>().Where(x => x.PaymentId == id && x.IsActive == true).Any();
                if (hasObligationPayment == true)
                {
                    errorMessage = "Плащането е насочено към задължение";
                    return false;
                }

                storno.IsActive = false;
                storno.UserId = userContext.UserId;
                storno.DateWrt = DateTime.Now;
                storno.UserDisabledId = userContext.UserId;
                storno.DateDisabled = DateTime.Now;
                repo.Update(storno);
                repo.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Payment_Storno Id={ id }");
                return false;
            }
        }

        /// <summary>
        /// Задължения към едно плащане
        /// </summary>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        public IQueryable<ObligationForPayVM> ObligationPaymentForPayment_Select(int paymentId)
        {
            return repo.AllReadonly<ObligationPayment>()
           .Where(x => x.PaymentId == paymentId)
           .Select(x => new ObligationForPayVM()
           {
               Id = x.Id,
               ObligationNumber = x.Obligation.ObligationNumber,
               ObligationDate = x.Obligation.ObligationDate,
               PersonUic = x.Obligation.Uic,
               PersonName = x.Obligation.FullName,
               MoneyTypeName = x.Obligation.MoneyType.Label,
               Amount = x.Amount,
               CaseData = x.Obligation.Case.CaseGroup.Code + " " + x.Obligation.Case.RegNumber,
               ObligationInfo = x.Obligation.ObligationInfo,
               IsActive = x.IsActive
           }).AsQueryable();
        }

        /// <summary>
        /// Плащания към едно задължение
        /// </summary>
        /// <param name="obligationId"></param>
        /// <returns></returns>
        public IQueryable<PaymentListVM> ObligationPaymentForObligation_Select(int obligationId)
        {
            return repo.AllReadonly<ObligationPayment>()
           .Include(x => x.Payment)
           .Include(x => x.Payment.CourtBankAccount)
           .Include(x => x.Payment.CourtBankAccount.MoneyGroup)
           .Where(x => x.ObligationId == obligationId)
           .Select(x => new PaymentListVM()
           {
               Id = x.Id,
               MoneyGroupName = x.Payment.CourtBankAccount.MoneyGroup.Label,
               Amount = x.Amount,
               PaidDate = x.Payment.PaidDate,
               SenderName = x.Payment.SenderName,
               IsActive = x.IsActive,
               IsAvans = x.Payment.IsAvans,
               PaymentNumber = x.Payment.PaymentNumber
           }).AsQueryable();
        }

        /// <summary>
        /// Сторно на връзка плащане-задължение
        /// </summary>
        /// <param name="id"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool ObligationPayment_Storno(int id, ref string errorMessage)
        {
            try
            {
                ObligationPayment storno = repo.AllReadonly<ObligationPayment>().Where(x => x.Id == id).FirstOrDefault();
                if (storno == null)
                {
                    return false;
                }
                if (storno.IsActive == false)
                {
                    errorMessage = "Записът вече е деактивиран";
                    return false;
                }

                storno.IsActive = false;
                storno.UserId = userContext.UserId;
                storno.DateWrt = DateTime.Now;
                storno.UserDisabledId = userContext.UserId;
                storno.DateDisabled = DateTime.Now;
                repo.Update(storno);

                //Ъпдейт статус на разходни ордери
                ChangeStatusExpenseOrderStorno(storno);

                repo.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на ObligationPayment_Storno Id={ id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на авансови плащания
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="senderName"></param>
        /// <param name="moneyGroupId"></param>
        /// <returns></returns>
        public IEnumerable<LabelValueVM> GetBalancePayment(int courtId, string senderName, int moneyGroupId)
        {
            senderName = senderName?.ToLower();

            var result = repo.AllReadonly<Payment>()
                            .Include(x => x.ObligationPayments)
                            .Include(x => x.CourtBankAccount)
                            .Where(x => x.CourtId == courtId)
                            .Where(x => x.IsActive == true)
                            .Where(x => (x.SenderName.ToLower().Contains(senderName) || x.PaymentNumber == senderName))
                            .Where(x => x.Amount > x.ObligationPayments.Where(p => p.IsActive == true).Select(p => p.Amount).DefaultIfEmpty(0).Sum())
                            .Where(x => x.CourtBankAccount.MoneyGroupId == moneyGroupId)
                            .OrderBy(x => x.SenderName)
                            .Select(x => new LabelValueVM
                            {
                                Value = x.Id.ToString(),
                                Label = (x.PaymentNumber ?? "") + "; " + (x.SenderName ?? "") + "; " + x.PaidDate.ToString("dd.MM.yyyy") + "; " + x.Amount + " лева"
                            }).ToList();

            return result;
        }

        /// <summary>
        /// Извличане на плащане по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LabelValueVM GetPaymentById(int id)
        {
            return repo.AllReadonly<Payment>().Where(x => x.Id == id)
                        .OrderBy(x => x.SenderName)
                        .Select(x => new LabelValueVM
                        {
                            Value = x.Id.ToString(),
                            Label = x.SenderName
                        }).ToList().DefaultIfEmpty(null).FirstOrDefault();
        }

        /// <summary>
        /// Извличане на плащане по id за обвързване на авансово плащане
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PaymentListVM GetPaymentById_BalancePayment(int id)
        {
            return repo.AllReadonly<Payment>()
           .Include(x => x.ObligationPayments)
           .Where(x => x.Id == id)
           .Select(x => new PaymentListVM()
           {
               Id = x.Id,
               Amount = x.Amount,
               AmountPayObligation = x.ObligationPayments.Where(a => a.IsActive == true).Select(a => a.Amount).DefaultIfEmpty(0).Sum()
           }).FirstOrDefault();
        }

        /// <summary>
        /// Запис на прихващане от авансово плащане
        /// </summary>
        /// <param name="model"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool BalancePayment_SaveData(BalancePaymentVM model, ref string errorMessage)
        {
            try
            {
                var obligationForPay = ObligationForPayByIds_Select(model.ObligationIds);
                Payment paymentData = repo.AllReadonly<Payment>().Include(x => x.ObligationPayments).Where(x => x.Id == model.Id).FirstOrDefault();
                if (paymentData == null)
                {
                    errorMessage = "Изберете плащане";
                    return false;
                }
                var balancePay = paymentData.Amount - paymentData.ObligationPayments.Where(x => x.IsActive == true).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                if (model.AmountPay - balancePay > 0.001M)
                {
                    errorMessage = "Оставащата сума за прихващане от плащането е " + balancePay;
                    return false;
                }

                List<ObligationPayment> payments = new List<ObligationPayment>();
                decimal paySum = model.AmountPay;
                SetFieldsPay(obligationForPay, ref paySum, payments);

                if (Math.Abs(paySum) > (decimal)0.001)
                {
                    return false;
                }

                foreach (var item in payments)
                {
                    item.PaymentId = model.Id;
                    item.UserId = userContext.UserId;
                    item.DateWrt = DateTime.Now;
                    repo.Add<ObligationPayment>(item);
                }
                //Ъпдейт статус на разходни ордери
                MakePaidExpenseOrder(payments);

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на BalancePayment");
                return false;
            }
        }

        /// <summary>
        /// Запис на плащане през ПОС
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool PosPaymentResult_SaveData(PosPaymentResult model)
        {
            try
            {
                model.PaidDate = DateTime.Now;
                model.UserId = userContext.UserId;
                model.DateWrt = DateTime.Now;
                repo.Add<PosPaymentResult>(model);

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на PosPaymentResult");
                return false;
            }
        }

        /// <summary>
        /// Извличане на незаписаните плащания през ПОС
        /// </summary>
        /// <param name="courtId"></param>
        /// <returns></returns>
        public IQueryable<PosPaymentResultListVM> UnsavedPosPayment_Select(int courtId)
        {
            return repo.AllReadonly<PosPaymentResult>()
           .Include(x => x.CourtBankAccount)
           .Include(x => x.CourtBankAccount.MoneyGroup)
           .Where(x => x.CourtId == courtId && x.PaymentId == null && x.Status == MoneyConstants.PosPaymentResultStatus.StatusOk)
           .Select(x => new PosPaymentResultListVM()
           {
               Id = x.Id,
               CourtBankAccountName = x.CourtBankAccount.Label,
               MoneyGroupName = x.CourtBankAccount.MoneyGroup.Label,
               Amount = x.Amount,
               PaidDate = x.PaidDate,
               SenderName = x.SenderName
           }).AsQueryable();
        }

        /// <summary>
        /// Създаване на плащания за ПОС транзакция
        /// </summary>
        /// <param name="id"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public (bool result, string errorMessage, int paymentId) MakePosPaymentFromPosResult(int id)
        {
            try
            {
                PosPaymentResult posPaymentResult = repo.GetById<PosPaymentResult>(id);
                if (posPaymentResult == null)
                {
                    return (result: false, errorMessage: "Проблем при взимане номер на плащането", paymentId: 0);
                }
                if (posPaymentResult.PaymentId != null)
                {
                    return (result: false, errorMessage: "Вече има записано плащане", paymentId: 0);
                }

                Payment model = new Payment();
                model.UserId = userContext.UserId;
                model.DateWrt = DateTime.Now;
                model.PaymentTypeId = NomenclatureConstants.PaymentType.Pos;
                model.CourtBankAccountId = posPaymentResult.CourtBankAccountId;
                model.Amount = posPaymentResult.Amount;
                model.PaidDate = posPaymentResult.PaidDate;
                model.SenderName = posPaymentResult.SenderName;
                model.IsActive = true;
                model.CourtId = posPaymentResult.CourtId;
                model.IsAvans = true;

                if (counterService.Counter_GetPaymentCounter(model) == false)
                {
                    return (result: false, errorMessage: "Проблем при взимане на брояч", paymentId: 0);
                }

                repo.Add(model);

                posPaymentResult.PaymentId = model.Id;
                repo.Update(posPaymentResult);
                repo.SaveChanges();

                return (result: true, errorMessage: "", paymentId: model.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Payment Id={ id }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed, paymentId: 0);
            }
        }

        /// <summary>
        /// Ставка за заседател за избрана дата
        /// </summary>
        /// <param name="date"></param>
        /// <param name="courtJuryFee"></param>
        /// <returns></returns>
        private CourtJuryFee GetForDate(DateTime date, List<CourtJuryFee> courtJuryFee)
        {
            var dateEnd = DateTime.Now.AddYears(100);
            return courtJuryFee.Where(x => date.Date >= x.DateFrom.Date && date.Date <= (x.DateTo ?? dateEnd).Date).FirstOrDefault();
        }

        /// <summary>
        /// Данни за възнаграждения за заседател
        /// </summary>
        /// <param name="model"></param>
        /// <param name="courtId"></param>
        /// <param name="lawUnit"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        private (bool result, string errorMessage) SetMainDataForEarnings(Obligation model, int courtId, LawUnit lawUnit, DateTime date)
        {
            model.UserId = userContext.UserId;
            model.DateWrt = DateTime.Now;
            model.CourtId = courtId;
            model.MoneyTypeId = NomenclatureConstants.MoneyType.Earnings;
            model.MoneySign = NomenclatureConstants.MoneySign.SignMinus;
            model.CopyFrom(lawUnit, false);
            model.PersonId = lawUnit.PersonId;

            if (counterService.Counter_GetObligationCounter(model) == false)
            {
                return (result: false, errorMessage: "Проблем при взимане на брояч");
            }
            model.ObligationDate = date;
            return (result: true, errorMessage: "");
        }

        /// <summary>
        /// Запис на възнаграждения за заседатели
        /// </summary>
        /// <param name="caseSession"></param>
        /// <param name="caseLawUnits"></param>
        /// <param name="courtJuryFee"></param>
        /// <param name="moneys"></param>
        /// <param name="courtId"></param>
        /// <returns></returns>
        private (bool result, string errorMessage) ObligationCaseSessionMeeting_SaveData(CaseSession caseSession, List<CaseLawUnit> caseLawUnits,
                                                                 List<CourtJuryFee> courtJuryFee, List<Obligation> moneys, int courtId)
        {
            try
            {
                var sessionType = repo.AllReadonly<SessionType>().Where(x => x.Id == caseSession.SessionTypeId).FirstOrDefault();
                var caseSessionMeetings = repo.AllReadonly<CaseSessionMeeting>().Where(x => x.CaseSessionId == caseSession.Id && x.DateExpired == null).ToList();

                foreach (var item in caseSessionMeetings)
                {
                    foreach (var itemLawUnit in caseLawUnits)
                    {
                        //Тайните сесии не се отнасят за резервните заседатели
                        if (item.SessionMeetingTypeId == NomenclatureConstants.SessionMeetingType.PrivateMeeting && itemLawUnit.JudgeRoleId == NomenclatureConstants.JudgeRole.ReserveJury) continue;
                        var juryFee = GetForDate(item.DateFrom, courtJuryFee);
                        if (juryFee == null)
                        {
                            return (result: false, errorMessage: "Няма въведена ставка за заседатели за дата " + item.DateFrom.ToString("dd.MM.yyyy"));
                        }
                        int hour = (int)Math.Ceiling((item.DateTo - item.DateFrom).TotalHours);

                        Obligation obligation = new Obligation();
                        obligation.Amount = juryFee.HourFee * hour;
                        obligation.ObligationInfo = sessionType.Label + " " + caseSession.DateFrom.ToString("dd.MM.yyyy");
                        obligation.CaseSessionId = caseSession.Id;
                        obligation.CaseSessionMeetingId = item.Id;
                        obligation.CaseId = caseSession.CaseId;

                        (bool resultSave, string errorMessageSave) = SetMainDataForEarnings(obligation, courtId, itemLawUnit.LawUnit, item.DateFrom);
                        if (resultSave == false)
                        {
                            return (result: resultSave, errorMessage: errorMessageSave);
                        }

                        obligation.Person_SourceType = SourceTypeSelectVM.CaseLawUnit;
                        obligation.Person_SourceId = itemLawUnit.Id;
                        obligation.IsActive = true;
                        moneys.Add(obligation);
                    }
                }

                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на ObligationCaseSessionMeetings_SaveData caseSession={ caseSession.Id }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Минимална сума за ден за заседател
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <param name="dates"></param>
        /// <param name="caseLawUnits"></param>
        /// <param name="courtJuryFee"></param>
        /// <param name="moneys"></param>
        /// <param name="courtId"></param>
        /// <param name="isRemove"></param>
        /// <returns></returns>
        private (bool result, string errorMessage) ObligationMinAmountForday_SaveData(int caseSessionId, List<DateTime> dates, List<CaseLawUnit> caseLawUnits,
                                                                 List<CourtJuryFee> courtJuryFee, List<Obligation> moneys, int courtId, bool isRemove)
        {
            try
            {
                foreach (var item in dates)
                {
                    foreach (var itemLawUnit in caseLawUnits)
                    {
                        //Ако сме записали пари за този Lawunit за тази дата да провери за минимална сума на ден
                        decimal sum = moneys.Where(x => x.ObligationDate.Date == item && x.FullName.ToLower() == itemLawUnit.LawUnit.FullName.ToLower()
                               && (x.Uic ?? "") == (itemLawUnit.LawUnit.Uic ?? "") && (x.IsForMinAmount ?? false) == false).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                        if (Math.Abs(sum) > 0.001M || isRemove == true)
                        {
                            var moneyLawUnit = ObligationForEarnings_Select(item, itemLawUnit.LawUnit.Uic, itemLawUnit.LawUnit.FullName, itemLawUnit.LawUnit.UicTypeId, courtId);
                            var itemAmountForDay = moneyLawUnit.Where(x => x.IsForMinAmount == true).FirstOrDefault();
                            var sumForDay = sum + moneyLawUnit.Where(x => (x.IsForMinAmount ?? false) == false).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                            var juryFee = GetForDate(item, courtJuryFee);
                            if (itemAmountForDay != null)
                            {
                                itemAmountForDay.Amount = sumForDay > 0 ? (sumForDay - juryFee.MinDayFee > 0.001M) ? 0 : (juryFee.MinDayFee - sumForDay) : 0;
                                repo.Update(itemAmountForDay);
                            }
                            else
                            {
                                if (juryFee.MinDayFee - sumForDay > 0.001M && isRemove == false)
                                {
                                    Obligation obligation = new Obligation();
                                    obligation.Amount = juryFee.MinDayFee - sumForDay;
                                    obligation.ObligationInfo = "Разлика до минималната сума за ден";
                                    obligation.IsForMinAmount = true;
                                    obligation.IsActive = true;

                                    (bool resultSave, string errorMessageSave) = SetMainDataForEarnings(obligation, courtId, itemLawUnit.LawUnit, item);
                                    if (resultSave == false)
                                    {
                                        return (result: resultSave, errorMessage: errorMessageSave);
                                    }

                                    obligation.Person_SourceType = SourceTypeSelectVM.LawUnit;
                                    obligation.Person_SourceId = itemLawUnit.LawUnitId;
                                    moneys.Add(obligation);
                                }
                            }
                        }
                    }
                }

                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на ObligationMinAmountForday_SaveData caseSession={ caseSessionId }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        private List<Obligation> GetMoneyForSessionEarnings(int sessionId)
        {
            return repo.AllReadonly<Obligation>()
                         .Where(x => x.CaseSessionId == sessionId &&
                              x.CaseSessionMeetingId != null &&
                              x.MoneyTypeId == NomenclatureConstants.MoneyType.Earnings)
                         .ToList();
        }

        private (bool result, string errorMessage) CheckIncludeObligation(List<Obligation> obligations)
        {
            var obligationIds = obligations.Select(x => x.Id).ToArray();
            var expenseOrder = repo.AllReadonly<ExpenseOrderObligation>()
                                   .Where(x => obligationIds.Contains(x.ObligationId))
                                   .Any();
            if (expenseOrder == true)
                return (result: false, errorMessage: "Има изготвен разходен ордер");

            var payment = repo.AllReadonly<ObligationPayment>()
                                   .Where(x => obligationIds.Contains(x.ObligationId))
                                   .Any();
            if (payment == true)
                return (result: false, errorMessage: "Има извършено плащане");

            return (result: true, errorMessage: "");
        }

        /// <summary>
        /// Изчисляване на задължение за заседател
        /// </summary>
        /// <param name="caseSession"></param>
        /// <param name="courtId"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) CalcEarningsJury(CaseSession caseSession, int courtId)
        {
            try
            {
                if (caseSession.SessionStateId != NomenclatureConstants.SessionState.Provedeno)
                {
                    return (result: true, errorMessage: "");
                }

                var caseLawUnitJuries = repo.AllReadonly<CaseLawUnit>().Include(x => x.LawUnit)
                     .Where(x => x.CaseSessionId == caseSession.Id &&
                               x.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Jury)
                     .ToList();
                //Ако няма никакъв заседател(независимо дали е изкаран, защото може да има начислени пари) по заседанието да не прави нищо
                if (caseLawUnitJuries.Count == 0)
                {
                    return (result: true, errorMessage: "");
                }

                //Само активните заседатели по заседанието
                var caseLawUnits = caseLawUnitJuries
                     .Where(x => (x.DateTo ?? caseSession.DateFrom) >= caseSession.DateFrom)
                     .ToList();

                //Заседатели, които вече не са в заседанието, но има начислени пари за тях. На тези трябва да се коригира минималната сума за ден
                List<CaseLawUnit> caseLawUnitsRemove = null;

                var moneysExists = GetMoneyForSessionEarnings(caseSession.Id);
                var dateObligations = moneysExists.Select(x => x.ObligationDate.Date).Distinct().ToList();
                //Ако има записани пари за това заседание, по които пари има плащане или разходен ордер, да не се прави нищо
                if (moneysExists.Count > 0)
                {
                    (bool resultCheckObligation, string errorMessageCheckObligation) = CheckIncludeObligation(moneysExists);
                    if (resultCheckObligation == false)
                    {
                        return (result: true, errorMessage: "");
                    }
                    else
                    {
                        //Всички заседатели, за които няма да се изчислява пари, но има записани такива
                        caseLawUnitsRemove = caseLawUnitJuries.Where(x => caseLawUnits.Where(a => x.LawUnitId == a.LawUnitId).Any() == false &&
                                       moneysExists.Where(a => a.UicTypeId == x.LawUnit.UicTypeId &&
                                       (x.LawUnit.Uic == a.Uic || (a.Uic == null && x.LawUnit.FullName.ToLower() == a.FullName.ToLower()))
                                                         ).Any()
                                                                    )
                                             .ToList();
                        repo.DeleteRange(moneysExists);
                        repo.SaveChanges();
                    }
                }

                List<Obligation> moneys = new List<Obligation>();
                //Запис на парите по дати за заседатели
                var courtJuryFee = repo.AllReadonly<CourtJuryFee>().Where(x => x.CourtId == courtId).ToList();

                if (caseLawUnits.Count > 0)
                {
                    (bool resultSaveMeeting, string errorMessageSaveMeeting) = ObligationCaseSessionMeeting_SaveData(caseSession, caseLawUnits,
                                                                             courtJuryFee, moneys, courtId);
                    if (resultSaveMeeting == false)
                    {
                        return (result: resultSaveMeeting, errorMessage: errorMessageSaveMeeting);
                    }


                    //Запис на парите до минималната сума по дати за заседатели
                    var dates = moneys.Select(x => x.ObligationDate.Date).Distinct().ToList();
                    (bool resultSaveMinAmount, string errorMessageSaveMinAmount) = ObligationMinAmountForday_SaveData(caseSession.Id, dates, caseLawUnits,
                                                                             courtJuryFee, moneys, courtId, false);
                    if (resultSaveMinAmount == false)
                    {
                        return (result: resultSaveMinAmount, errorMessage: errorMessageSaveMinAmount);
                    }

                    repo.AddRange(moneys);
                }

                // Ако има заседатели, за които е било начислено и после са премахнати - да се оправят минималнити суми на ден
                if (caseLawUnitsRemove != null && caseLawUnitsRemove.Count > 0)
                {                    
                    (bool resultSaveMinAmount, string errorMessageSaveMinAmount) = ObligationMinAmountForday_SaveData(caseSession.Id, dateObligations, caseLawUnitsRemove,
                                                                             courtJuryFee, moneys, courtId, true);
                    if (resultSaveMinAmount == false)
                    {
                        return (result: resultSaveMinAmount, errorMessage: errorMessageSaveMinAmount);
                    }

                    repo.AddRange(moneys);
                }

                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CalcEarningsJury caseSession={ caseSession.Id }");
                return (result: false, errorMessage: MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Извличане на възнаграждения за заседател
        /// </summary>
        /// <param name="obligationDate"></param>
        /// <param name="uic"></param>
        /// <param name="fullName"></param>
        /// <param name="uicTypeId"></param>
        /// <returns></returns>
        private List<Obligation> ObligationForEarnings_Select(DateTime obligationDate, string uic, string fullName, int uicTypeId, int courtId)
        {
            Expression<Func<Obligation, bool>> personWhere = CommonExtensions.PersonNamesBase_Where<Obligation>(uicTypeId, uic, fullName);

            return repo.AllReadonly<Obligation>()
                    .Where(x => x.CourtId == courtId)
                    .Where(x => (x.IsActive ?? true) == true)
                    .Where(x => x.ObligationDate.Date == obligationDate.Date && x.MoneyTypeId == NomenclatureConstants.MoneyType.Earnings)
                    .Where(personWhere)
                    .ToList();
        }

        /// <summary>
        /// Извличане на данни за избрани задължения
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public IQueryable<Obligation> ObligationByIds_Select(string ids)
        {
            List<string> idList = new List<string>();
            idList = ids.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();

            return repo.AllReadonly<Obligation>()
           .Include(x => x.CaseSessionAct)
           .Include(x => x.MoneyType)
           .Where(x => idList.Contains(x.Id.ToString()))
           .AsQueryable();
        }

        /// <summary>
        /// Данни за разходен ордер
        /// </summary>
        /// <param name="model"></param>
        /// <param name="saved"></param>
        private void ExpenseOrderSetDataFromModel(ExpenseOrderEditVM model, ExpenseOrder saved)
        {
            saved.UserId = userContext.UserId;
            saved.DateWrt = DateTime.Now;
            saved.RegionName = model.RegionName;
            saved.FirmName = model.FirmName;
            saved.FirmCity = model.FirmCity;
            saved.PaidNote = model.PaidNote;
            saved.Iban = model.Iban?.ToUpper();
            saved.BIC = model.BIC?.ToUpper();
            saved.BankName = model.BankName;
            saved.LawUnitSignId = model.LawUnitSignId;
        }

        /// <summary>
        /// Запис на разходен ордер
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) ExpenseOrder_Save(ExpenseOrderEditVM model)
        {
            try
            {
                List<string> idList = new List<string>();
                idList = model.ObligationIdStr.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();

                List<Obligation> obligations = ObligationByIds_Select(model.ObligationIdStr).ToList();
                if (obligations.Count() == 0)
                {
                    return (result: false, errorMessage: "Изберете задължение");
                }

                bool checkMoneyType = obligations.Where(x => (x.MoneyType.IsTransport ?? false) != true &&
                                                        (x.MoneyType.IsEarning ?? false) != true).Any();
                if (checkMoneyType == true)
                {
                    return (result: false, errorMessage: "Има задължения, които не влизат в РКО");
                }

                bool hasNoActive = obligations.Where(x => x.IsActive == false).Any();
                if (hasNoActive == true)
                {
                    return (result: false, errorMessage: "Има задължения, които са деактивирани");
                }

                Func<Obligation, bool> personWhere = CommonExtensions.PersonNamesBase_Where<Obligation>(obligations[0].UicTypeId,
                                           obligations[0].Uic, obligations[0].FullName).Compile();
                var obligationPersons = obligations.Where(personWhere).ToList();
                if (obligationPersons.Count != obligations.Count)
                {
                    return (result: false, errorMessage: "Изберете задължения на едно лице");
                }

                bool existsOrder = repo.AllReadonly<ExpenseOrder>()
                                           .Include(x => x.ExpenseOrderObligations)
                                           .Where(x => x.IsActive == true)
                                           .Where(x => x.ExpenseOrderObligations.Where(o => idList.Contains(o.ObligationId.ToString())).Any()).Any();
                if (existsOrder == true)
                {
                    return (result: false, errorMessage: "Има задължения влезнали в друг разходен ордер");
                }

                ExpenseOrder saved = new ExpenseOrder();
                ExpenseOrderSetDataFromModel(model, saved);
                saved.CourtId = obligations[0].CourtId;
                saved.IsActive = true;
                saved.ExpenseOrderStateId = NomenclatureConstants.ExpenseOrderState.StateReady;

                if (counterService.Counter_GetExpenseOrderCounter(saved) == false)
                {
                    return (result: false, errorMessage: "Проблем при взимане на брояч");
                }

                foreach (var item in obligations)
                {
                    ExpenseOrderObligation itemAdd = new ExpenseOrderObligation();
                    itemAdd.ObligationId = item.Id;
                    itemAdd.ExpenseOrderId = saved.Id;
                    saved.ExpenseOrderObligations.Add(itemAdd);
                }

                repo.Add<ExpenseOrder>(saved);
                repo.SaveChanges();

                model.Id = saved.Id;
                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на ExpenseOrder obligationIds={ model.ObligationIdStr }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Извличане на данните за разходни ордери
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="name"></param>
        /// <param name="expenseOrderRegNumber"></param>
        /// <returns></returns>
        public IQueryable<ExpenseOrderVM> ExpenseOrder_Select(int courtId, DateTime? fromDate, DateTime? toDate, string name, string expenseOrderRegNumber)
        {
            DateTime fromDateNull = (fromDate == null ? DateTime.Now.AddYears(-100) : (DateTime)fromDate).Date;
            DateTime toDateNull = (toDate == null ? DateTime.Now.AddYears(100) : (DateTime)toDate).Date;
            Expression<Func<ExpenseOrder, bool>> dateWhere = x => true;
            if (fromDate != null || toDate != null)
                dateWhere = x => x.RegDate.Date >= fromDateNull && x.RegDate.Date <= toDateNull;

            Expression<Func<ExpenseOrder, bool>> nameWhere = x => true;
            if (!string.IsNullOrEmpty(name))
                nameWhere = x => x.ExpenseOrderObligations.Where(o => o.Obligation.FullName.ToLower().Contains(name.ToLower())).Any();

            Expression<Func<ExpenseOrder, bool>> expenseOrderNumberWhere = x => true;
            if (!string.IsNullOrEmpty(expenseOrderRegNumber))
                expenseOrderNumberWhere = x => x.RegNumber.Contains(expenseOrderRegNumber);

            return repo.AllReadonly<ExpenseOrder>()
                .Where(x => x.CourtId == courtId)
                .Where(dateWhere)
                .Where(nameWhere)
                .Where(expenseOrderNumberWhere)
                .Select(x => new ExpenseOrderVM()
                {
                    Id = x.Id,
                    RegNumber = x.RegNumber,
                    RegDate = x.RegDate,
                    IsActive = x.IsActive,
                    FullName = x.ExpenseOrderObligations.Select(o => o.Obligation.FullName).DefaultIfEmpty("").FirstOrDefault(),
                    Amount = x.ExpenseOrderObligations.Select(o => o.Obligation.Amount).DefaultIfEmpty(0).Sum(),
                    ExpenseOrderStateName = x.ExpenseOrderState.Label,
                    MoneyGroupName = x.ExpenseOrderObligations.Select(o => o.Obligation.MoneyType.MoneyGroup.Label).DefaultIfEmpty("").FirstOrDefault(),
                }).AsQueryable();
        }

        /// <summary>
        /// Сторно на разходен ордер
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) ExpenseOrder_Storno(int id)
        {
            try
            {
                ExpenseOrder storno = GetById<ExpenseOrder>(id);
                if (storno == null)
                {
                    return (result: false, errorMessage: "Невалиден ордер");
                }
                if (storno.IsActive == false)
                {
                    return (result: false, errorMessage: "Ордерът вече е деактивиран");
                }

                storno.IsActive = false;
                storno.UserId = userContext.UserId;
                storno.DateWrt = DateTime.Now;
                repo.Update(storno);
                repo.SaveChanges();

                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на ExpenseOrder_Storno Id={ id }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Редакция на разходен ордер
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) ExpenseOrder_Update(ExpenseOrderEditVM model)
        {
            try
            {
                var saved = repo.GetById<ExpenseOrder>(model.Id);

                if (saved.IsActive == false)
                {
                    return (result: false, errorMessage: "Разходният ордер е сторниран");
                }

                ExpenseOrderSetDataFromModel(model, saved);
                saved.ExpenseOrderStateId = (model.ExpenseOrderStateId ?? 0) <= 0 ? null : model.ExpenseOrderStateId;

                repo.Update(saved);
                repo.SaveChanges();

                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на ExpenseOrder_Update id={ model.Id }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Извличане на данни за разходен ордер
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ExpenseOrderEditVM ExpenseOrder_GetById(int id)
        {
            return repo.AllReadonly<ExpenseOrder>()
                .Where(x => x.Id == id)
                .Select(x => new ExpenseOrderEditVM()
                {
                    Id = x.Id,
                    RegionName = x.RegionName,
                    FirmName = x.FirmName,
                    FirmCity = x.FirmCity,
                    PaidNote = x.PaidNote,
                    Iban = x.Iban,
                    BIC = x.BIC,
                    BankName = x.BankName,
                    ExpenseOrderStateId = x.ExpenseOrderStateId,
                    LawUnitSignId = x.LawUnitSignId ?? 0,
                })
                .FirstOrDefault();
        }

        /// <summary>
        /// Извличане на данни от послед разходен ордер за лице
        /// </summary>
        /// <param name="obligationIdStr"></param>
        /// <returns></returns>
        public ExpenseOrder ExpenseOrder_LastOrderForPerson(string obligationIdStr)
        {
            ExpenseOrder result = null;
            List<Obligation> obligations = ObligationByIds_Select(obligationIdStr).ToList();
            if (obligations.Count > 0)
            {
                var obligation = obligations[0];

                Expression<Func<ExpenseOrder, bool>> orderPersonWhere = x => true;
                if (!string.IsNullOrEmpty(obligation.Uic))
                    orderPersonWhere = x => x.ExpenseOrderObligations.Where(o => o.Obligation.UicTypeId == obligation.UicTypeId && o.Obligation.Uic == obligation.Uic).Any();
                else
                    orderPersonWhere = x => x.ExpenseOrderObligations.Where(o => o.Obligation.UicTypeId == obligation.UicTypeId
                                                             && o.Obligation.FullName.ToLower().Contains(obligation.FullName.ToLower())).Any();


                result = repo.AllReadonly<ExpenseOrder>()
               .Include(x => x.ExpenseOrderObligations)
               .ThenInclude(x => x.Obligation)
               .Where(x => x.CourtId == obligation.CourtId)
               .Where(x => x.IsActive == true)
               .Where(orderPersonWhere)
               .OrderByDescending(x => x.Id)
               .FirstOrDefault();

                if (result == null)
                    result = new ExpenseOrder();

                //Взима на съдия
                var sessions = obligations
                        .Where(x => x.CaseSessionId != null || x.CaseSessionActId != null)
                        .Select(x => x.CaseSessionId ?? (x.CaseSessionAct.CaseSessionId))
                        .Distinct().ToList();

                var caseLawUnit = repo.AllReadonly<CaseLawUnit>()
                                            .Include(x => x.CaseSession)
                                            .Where(x => sessions.Contains(x.CaseSessionId ?? 0))
                                            .Where(x => x.DateFrom <= x.CaseSession.DateFrom && (x.DateTo ?? x.CaseSession.DateFrom.AddDays(1)) >= x.CaseSession.DateFrom)
                                            .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                            .Select(x => x.LawUnitId)
                                            .Distinct()
                                            .ToList();

                if (caseLawUnit.Count == 1)
                {
                    result.LawUnitSignId = caseLawUnit[0];
                }
                else if (caseLawUnit.Count > 1)
                {
                    var generalJudge = commonService.GetGeneralJudgeCourtLawUnit(obligations[0].CourtId);
                    if (generalJudge != null)
                    {
                        result.LawUnitSignId = generalJudge.LawUnitId;
                    }
                }
                else
                {
                    result.LawUnitSignId = 0;
                }
            }

            return result;
        }

        /// <summary>
        /// Смяна на статус на разходен ордер - платен
        /// </summary>
        /// <param name="payments"></param>
        private void MakePaidExpenseOrder(List<ObligationPayment> payments)
        {
            if (payments.Count == 0) return;

            var obligationIds = payments.Select(x => x.ObligationId).Distinct().ToList();
            //Взимат се всички ордери за задълженията за които има плащане
            var orders = repo.AllReadonly<ExpenseOrderObligation>()
                .Include(x => x.ExpenseOrder)
                .Where(x => x.ExpenseOrder.IsActive == true)
                .Where(x => x.ExpenseOrder.ExpenseOrderStateId != NomenclatureConstants.ExpenseOrderState.StatePaid)
                .Where(x => obligationIds.Contains(x.ObligationId))
                .Select(x => x.ExpenseOrder)
                .Distinct()
                .ToList();

            if (orders.Count > 0)
            {
                //Всички задлжения към ордерите
                var moneys = repo.AllReadonly<ExpenseOrderObligation>()
                                   .Include(x => x.Obligation)
                                   .Include(x => x.Obligation.ObligationPayments)
                                   .Where(x => orders.Select(o => o.Id).Contains(x.ExpenseOrderId))
                                   .Select(x => new ObligationForPayVM()
                                   {
                                       Id = x.ObligationId,
                                       ExpenseOrderId = x.ExpenseOrderId,
                                       Amount = x.Obligation.Amount,
                                       AmountPay = x.Obligation.ObligationPayments.Where(p => p.IsActive == true).Select(p => p.Amount).DefaultIfEmpty(0).Sum()
                                   })
                                   .ToList();

                foreach (var item in payments)
                {
                    var money = moneys.Where(x => x.Id == item.ObligationId).FirstOrDefault();
                    if (money != null)
                        money.AmountPay += item.Amount;
                }

                foreach (var item in orders)
                {
                    decimal sumForPayForOrder = moneys.Where(x => x.ExpenseOrderId == item.Id).Select(x => x.AmountForPay).DefaultIfEmpty(0).Sum();
                    if (sumForPayForOrder < 0.001M)
                    {
                        item.ExpenseOrderStateId = NomenclatureConstants.ExpenseOrderState.StatePaid;
                        repo.Update(item);
                    }
                }
            }
        }

        /// <summary>
        /// Смяна на статус на разходен ордер
        /// </summary>
        /// <param name="payment"></param>
        private void ChangeStatusExpenseOrderStorno(ObligationPayment payment)
        {
            var order = repo.AllReadonly<ExpenseOrderObligation>()
                .Include(x => x.ExpenseOrder)
                .Where(x => x.ExpenseOrder.IsActive == true)
                .Where(x => x.ExpenseOrder.ExpenseOrderStateId == NomenclatureConstants.ExpenseOrderState.StatePaid)
                .Where(x => x.ObligationId == payment.ObligationId)
                .Select(x => x.ExpenseOrder)
                .FirstOrDefault();

            if (order != null)
            {
                //Всички задлжения към ордерите
                var moneys = repo.AllReadonly<ExpenseOrderObligation>()
                                   .Include(x => x.Obligation)
                                   .Include(x => x.Obligation.ObligationPayments)
                                   .Where(x => x.ExpenseOrderId == order.Id)
                                   .Select(x => new ObligationForPayVM()
                                   {
                                       Id = x.ObligationId,
                                       ExpenseOrderId = x.ExpenseOrderId,
                                       Amount = x.Obligation.Amount,
                                       AmountPay = x.Obligation.ObligationPayments.Where(p => p.IsActive == true).Select(p => p.Amount).DefaultIfEmpty(0).Sum()
                                   })
                                   .ToList();

                var money = moneys.Where(x => x.Id == payment.ObligationId).FirstOrDefault();
                if (money != null)
                    money.AmountPay -= payment.Amount;

                decimal sumForPayForOrder = moneys.Where(x => x.ExpenseOrderId == order.Id).Select(x => x.AmountForPay).DefaultIfEmpty(0).Sum();
                if (sumForPayForOrder > 0.001M)
                {
                    order.ExpenseOrderStateId = NomenclatureConstants.ExpenseOrderState.StateDeliver;
                    repo.Update(order);
                }
            }
        }

        /// <summary>
        /// Попълване на забележка за печат на изпълнителен лист
        /// </summary>
        /// <param name="obligations"></param>
        /// <param name="caseId"></param>
        /// <param name="execTypeId"></param>
        /// <returns></returns>
        private string FillExecListDescription(List<Obligation> obligations, int caseId, int execTypeId)
        {
            var caseModel = repo.AllReadonly<Case>()
                              .Include(x => x.CaseType)
                              .Where(x => x.Id == caseId)
                              .FirstOrDefault();

            var casePersons = repo.AllReadonly<CasePerson>()
                             .Include(x => x.Addresses)
                             .ThenInclude(x => x.Address)
                             .Include(x => x.PersonRole)
                             .Where(x => x.CaseSessionId == null)
                             .Where(x => x.DateExpired == null)
                             .ToList();
            var persons = obligations.Select(x => x.FullName + ", " + x.UicType.Label + " " + (x.Uic ?? "") + " " +
                                   casePersons.Where(a => a.Id == x.Person_SourceId)
                                   .Select(a => a.Addresses.Where(b => b.DateExpired == null).Select(b => b.Address.FullAddress).FirstOrDefault() +
                                   (execTypeId == NomenclatureConstants.ExecListTypes.ThirdPerson ? (", " + a.PersonRole.Label) : ""))
                                   .FirstOrDefault())
                              .Distinct().ToList();
            string personNames = string.Join(", ", persons);
            string paragraph = "<p style='text-align:justify;text-indent:36.0pt;'><span>";
            string description = paragraph + "Осъжда " + personNames + ":" + "</span></p>";
            string payText = persons.Count > 1 ? "заплатят" : "заплати";


            if (execTypeId == NomenclatureConstants.ExecListTypes.Country)
            {
                foreach (var item in obligations)
                {
                    description += paragraph;
                    string receivetext = "";
                    var receiveObligation = item.ObligationReceives.FirstOrDefault();
                    if (receiveObligation != null)
                    {
                        receivetext = receiveObligation.Person_SourceType == SourceTypeSelectVM.Court ? "бюджета на съдебната власт" : "Държавата";

                        if ((item.MoneyType.NoMoney ?? false) == false)
                        {
                            receivetext += " по сметка на " + receiveObligation.FullName + ", банкова сметка " + receiveObligation.Iban + ", " +
                                "BIC код " + receiveObligation.BIC + ", банка " + receiveObligation.BankName;
                        }
                    }
                    if ((item.MoneyType.NoMoney ?? false) == false)
                    {
                        decimal sumForPay = item.Amount - item.ObligationPayments.Where(x => x.IsActive).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                        description += "- да " + payText + " сумата от " + sumForPay.ToString("0.00") + " лв. (словом: " +
                         MoneyExtensions.MoneyToString(sumForPay) + " ), представляваща " + item.MoneyType.Label + " в полза на " + receivetext;
                    }
                    else
                    {
                        description += "- " + (item.Description ?? "");
                    }
                    description += "</span></p>";
                }
            }
            else
            {
                foreach (var item in obligations)
                {
                    description += paragraph;
                    if ((item.MoneyType.NoMoney ?? false) == false)
                    {
                        decimal sumForPay = item.Amount - item.ObligationPayments.Where(x => x.IsActive).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                        description += "- да " + payText + " сумата от " + sumForPay.ToString("0.00") + " лв. (словом: " +
                         MoneyExtensions.MoneyToString(sumForPay) + " ), представляваща " + item.MoneyType.Label;
                    }
                    else
                    {
                        description += "- " + (item.Description ?? "");
                    }
                    description += "</span></p>";
                }
            }

            return description;
        }

        /// <summary>
        /// Извличане на данни при запис на изпълнителен лист
        /// </summary>
        /// <param name="obligationIds"></param>
        /// <returns></returns>
        private (List<Obligation> obligations, List<int> execListTypes, List<int> cases) ReadDataForSaveExecList(string obligationIds)
        {
            List<string> idList = new List<string>();
            idList = obligationIds.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();

            List<Obligation> obligations = repo.AllReadonly<Obligation>()
                .Include(x => x.CaseSessionAct)
                .Include(x => x.CaseSessionAct.CaseSession)
                .Include(x => x.ObligationReceives)
                .Include(x => x.MoneyType)
                .Include(x => x.UicType)
                .Include(x => x.ObligationPayments)
                .Where(x => idList.Contains(x.Id.ToString()))
                .ToList();

            List<int> execListTypes = obligations.Select(x => x.ObligationReceives
                                                       .Select(a => a.ExecListTypeId ?? 0).FirstOrDefault()).Distinct().ToList();

            List<int> cases = obligations.Where(x => x.CaseSessionActId != null).Select(x => x.CaseSessionAct.CaseSession.CaseId)
                                 .Distinct().ToList();

            return (obligations: obligations, execListTypes: execListTypes, cases: cases);

        }

        /// <summary>
        /// Валидация преди запис на изпълнителен лист
        /// </summary>
        /// <param name="obligations"></param>
        /// <param name="execListTypes"></param>
        /// <param name="cases"></param>
        /// <returns></returns>
        private (bool result, string errorMessage) VaidateExecList(List<Obligation> obligations, List<int> execListTypes, List<int> cases)
        {
            if (obligations.Count() == 0)
            {
                return (result: false, errorMessage: "Изберете задължение");
            }

            bool hasNoActive = obligations.Where(x => x.IsActive == false).Any();
            if (hasNoActive == true)
            {
                return (result: false, errorMessage: "Има задължения, които са деактивирани");
            }


            bool noExecType = obligations.Where(x => x.ObligationReceives.Any() == false).Any();
            if (noExecType == true)
            {
                return (result: false, errorMessage: "Има задължения, които не са в ничия полза");
            }

            if (execListTypes.Count != 1)
            {
                return (result: false, errorMessage: "Има задължения, които са в полза на Държавата и в полза на страни");
            }

            if (execListTypes[0] != NomenclatureConstants.ExecListTypes.ThirdPerson)
            {
                bool signMinus = obligations.Where(x => x.MoneySign != NomenclatureConstants.MoneySign.SignPlus).Any();
                if (signMinus == true)
                {
                    return (result: false, errorMessage: "Има задължения, които не са приход");
                }
            }

            bool noAct = obligations.Where(x => x.CaseSessionActId == null).Any();
            if (noAct == true)
            {
                return (result: false, errorMessage: "Има задължения, които не са по акт");
            }

            if (cases.Count() > 1)
            {
                return (result: false, errorMessage: "Има задължения по повече от едно дело");
            }

            List<int> obligationIds = obligations.Select(x => x.Id).ToList();
            bool existsOrder = repo.AllReadonly<ExecList>()
                                       .Include(x => x.ExecListObligations)
                                       .Where(x => x.IsActive == true)
                                       .Where(x => x.ExecListObligations.Where(o => obligationIds.Contains(o.ObligationId)).Any()).Any();
            if (existsOrder == true)
            {
                return (result: false, errorMessage: "Има задължения влезнали в друг изпълнителен лист");
            }

            return (result: true, errorMessage: "");
        }

        /// <summary>
        /// Подготовка за запис на изпълнителен лист
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) ExecList_PrepareSave(ExecListEditVM model)
        {
            (List<Obligation> obligations, List<int> execListTypes, List<int> cases) = ReadDataForSaveExecList(model.ObligationIdStr);
            (bool result, string errorMessage) = VaidateExecList(obligations, execListTypes, cases);
            if (result == false)
            {
                return (result: result, errorMessage: errorMessage);
            }

            model.ExecListTypeId = execListTypes[0];
            model.CaseGroupId = repo.AllReadonly<Case>().Where(x => x.Id == cases[0]).Select(x => x.CaseGroupId).FirstOrDefault();

            DateTime dateEnd = DateTime.Now.AddDays(1);
            var caseLawUnit = repo.AllReadonly<CaseLawUnit>()
                         .Where(x => x.CaseId == cases[0])
                         .Where(x => x.CaseSessionId == null)
                         .Where(x => (x.DateTo ?? dateEnd) >= DateTime.Now)
                         .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                         .FirstOrDefault();

            if (caseLawUnit != null)
                model.LawUnitSignId = caseLawUnit.LawUnitId;
            return (result: true, errorMessage: "");
        }

        /// <summary>
        /// Запис на изпълнителен лист
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) ExecList_Save(ExecListEditVM model)
        {
            try
            {
                (List<Obligation> obligations, List<int> execListTypes, List<int> cases) = ReadDataForSaveExecList(model.ObligationIdStr);
                (bool result, string errorMessage) = VaidateExecList(obligations, execListTypes, cases);
                if (result == false)
                {
                    return (result: result, errorMessage: errorMessage);
                }

                model.ExecListLawBaseId = (model.ExecListLawBaseId ?? 0) <= 0 ? null : model.ExecListLawBaseId;

                ExecList saved = new ExecList();
                saved.CourtId = obligations[0].CourtId;
                saved.IsActive = true;
                saved.ExecListTypeId = model.ExecListTypeId;
                //Записвам го тук заради печата на някакви писма. Иначе целия ИЛ минава в tiny
                saved.Description = FillExecListDescription(obligations, cases[0], model.ExecListTypeId);
                saved.ExecListLawBaseId = model.ExecListLawBaseId;
                saved.LawUnitSignId = model.LawUnitSignId;
                saved.ExecListStateId = NomenclatureConstants.ExecListStates.Ready;
                saved.UserId = userContext.UserId;
                saved.DateWrt = DateTime.Now;

                foreach (var item in obligations)
                {
                    ExecListObligation itemAdd = new ExecListObligation();
                    itemAdd.ObligationId = item.Id;
                    itemAdd.ExecListId = saved.Id;
                    //Сължимата сума към момента
                    itemAdd.Amount = item.Amount - item.ObligationPayments
                                 .Where(x => x.IsActive).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                    saved.ExecListObligations.Add(itemAdd);
                }

                repo.Add<ExecList>(saved);
                repo.SaveChanges();

                model.Id = saved.Id;
                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на ExecList obligationIds={ model.ObligationIdStr }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Извличане на данни за изпълнителни листове
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<ExecListVM> ExecList_Select(int courtId, ExecListFilterVM model)
        {
            DateTime fromDateNull = (model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom).Date;
            DateTime toDateNull = (model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo).Date;
            Expression<Func<ExecList, bool>> dateWhere = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateWhere = x => (x.RegDate == null || (x.RegDate >= fromDateNull.ForceStartDate() && x.RegDate <= toDateNull.ForceEndDate()));

            Expression<Func<ExecList, bool>> nameWhere = x => true;
            if (!string.IsNullOrEmpty(model.FullName))
                nameWhere = x => x.ExecListObligations.Where(o => o.Obligation.FullName.ToLower().Contains(model.FullName.ToLower())).Any();

            Expression<Func<ExecList, bool>> nameReceiveWhere = x => true;
            if (!string.IsNullOrEmpty(model.FullNameReceive))
                nameReceiveWhere = x => x.ExecListObligations.Where(o => o.Obligation.ObligationReceives.Where(a => a.FullName.ToLower().Contains(model.FullNameReceive.ToLower())).Any()).Any();

            Expression<Func<ExecList, bool>> execListNumberWhere = x => true;
            if (!string.IsNullOrEmpty(model.RegNumber))
                execListNumberWhere = x => x.RegNumber.Contains(model.RegNumber);

            Expression<Func<ExecList, bool>> execListTypeWhere = x => true;
            if (model.ExecListTypeId > 0)
                execListTypeWhere = x => x.ExecListTypeId == model.ExecListTypeId;

            Expression<Func<ExecList, bool>> institutionWhere = x => true;
            if (model.InstitutionId > 0)
                institutionWhere = x => x.OutDocument.DocumentPersons
                                 .Where(a => a.Person_SourceType == SourceTypeSelectVM.Instutution &&
                                           a.Person_SourceId == model.InstitutionId).Any();

            Expression<Func<ExecList, bool>> activeWhere = x => true;
            if (model.ActiveExecList)
                activeWhere = x => x.IsActive == model.ActiveExecList;

            return repo.AllReadonly<ExecList>()
                .Where(x => x.CourtId == courtId)
                .Where(dateWhere)
                .Where(nameWhere)
                .Where(nameReceiveWhere)
                .Where(execListNumberWhere)
                .Where(execListTypeWhere)
                .Where(institutionWhere)
                .Where(activeWhere)
                .Select(x => new ExecListVM()
                {
                    Id = x.Id,
                    RegNumber = x.RegNumber,
                    RegDate = x.RegDate,
                    IsActive = x.IsActive,
                    FullName = string.Join("<br>", x.ExecListObligations.Select(o => o.Obligation.FullName + " (" + (o.Obligation.Uic ?? "") + ")").Distinct()),
                    FullNameReceive = string.Join("<br>", x.ExecListObligations.Select(o => o.Obligation.ObligationReceives.Select(a => a.FullName).FirstOrDefault()).Distinct()),
                    Amount = x.ExecListObligations.Select(o => o.Amount ?? 0).DefaultIfEmpty(0).Sum(),
                    ExecListTypeName = x.ExecListType.Label,
                    InstitutionNames = string.Join("<br>", x.OutDocument.DocumentPersons.Select(o => o.FullName).Distinct()),
                    ExchangeDocNumber = x.ExchangeDocExecLists
                                     .Where(a => a.ExchangeDoc.IsActive)
                                     .Select(a => a.ExchangeDoc.RegDate != null ? a.ExchangeDoc.RegNumber : "В проект")
                                     .FirstOrDefault(),
                    ExchangeDocId = x.ExchangeDocExecLists
                                     .Where(a => a.ExchangeDoc.IsActive)
                                     .Select(a => a.ExchangeDocId)
                                     .FirstOrDefault(),
                    CaseNumber = x.ExecListObligations.Select(o => o.Obligation.Case.CaseGroup.Code + " " + o.Obligation.Case.RegNumber).FirstOrDefault(),
                    CaseId = x.ExecListObligations.Select(o => o.Obligation.CaseId).FirstOrDefault(),
                    MoneyTypeName = string.Join("<br>", x.ExecListObligations.Select(o => o.Obligation.MoneyType.Label).Distinct()),
                    StateName = x.ExecListState.Label,
                }).AsQueryable();
        }

        /// <summary>
        /// Сторно на изпълнителен лист
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) ExecList_Storno(int id)
        {
            try
            {
                ExecList storno = GetById<ExecList>(id);
                if (storno == null)
                {
                    return (result: false, errorMessage: "Невалиден изпълнителен лист");
                }
                if (storno.IsActive == false)
                {
                    return (result: false, errorMessage: "Изпълнителният лист вече е деактивиран");
                }

                storno.IsActive = false;
                storno.UserId = userContext.UserId;
                storno.DateWrt = DateTime.Now;
                repo.Update(storno);
                repo.SaveChanges();

                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на ExecList_Storno Id={ id }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Запис на изпълнителен лист
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) ExecList_Update(ExecListEditVM model)
        {
            try
            {
                var saved = repo.GetById<ExecList>(model.Id);

                if (saved.IsActive == false)
                {
                    return (result: false, errorMessage: "Изпълнителният лист е деактивиран");
                }

                model.ExecListLawBaseId = (model.ExecListLawBaseId ?? 0) <= 0 ? null : model.ExecListLawBaseId;
                model.ExecListStateId = model.ExecListStateId.EmptyToNull();

                saved.CaseNumber = model.CaseNumber;
                saved.DeliveryDate = model.DeliveryDate;
                saved.DeliveryPersonName = model.DeliveryPersonName;
                saved.ExecListLawBaseId = model.ExecListLawBaseId;
                saved.LawUnitSignId = model.LawUnitSignId;
                saved.ExecListStateId = model.ExecListStateId;

                saved.UserId = userContext.UserId;
                saved.DateWrt = DateTime.Now;

                repo.Update(saved);
                repo.SaveChanges();

                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на ExecList_Update id={ model.Id }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Извличане на данни за изпълнителен лист
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ExecListEditVM ExecList_GetById(int id)
        {
            return repo.AllReadonly<ExecList>()
                .Where(x => x.Id == id)
                .Select(x => new ExecListEditVM()
                {
                    Id = x.Id,
                    ExecListTypeId = x.ExecListTypeId,
                    DeliveryDate = x.DeliveryDate,
                    DeliveryPersonName = x.DeliveryPersonName,
                    CaseNumber = x.CaseNumber,
                    RegNumber = x.RegNumber,
                    RegDate = x.RegDate,
                    ExecListLawBaseId = x.ExecListLawBaseId,
                    CaseGroupId = x.ExecListObligations.Select(a => a.Obligation.Case.CaseGroupId).FirstOrDefault(),
                    LawUnitSignId = x.LawUnitSignId ?? 0,
                    ExecListStateId = x.ExecListStateId,
                })
                .FirstOrDefault();
        }        

        /// <summary>
        ///  Извличане на данни за получател на сума
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public ObligationReceive LastDataForReceive_Select(int sourceType, long sourceId)
        {
            ObligationReceive result = null;
            if (sourceType > 0 && sourceId > 0)
            {
                result = repo.AllReadonly<ObligationReceive>()
                    .Where(x => x.Person_SourceType == sourceType && x.Person_SourceId == sourceId)
                    .OrderByDescending(x => x.ObligationId)
                    .FirstOrDefault();
            }
            return result ?? new ObligationReceive();
        }

        /// <summary>
        /// Извличане на плащания по дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IQueryable<PaymentCaseVM> PaymentForCase_Select(int caseId)
        {
            return repo.AllReadonly<Payment>()
                .Where(x => x.IsActive == true)
                .Where(x => x.ObligationPayments.Where(a => a.IsActive == true && a.Obligation.CaseSessionAct.CaseId == caseId).Any())
                .Select(x => new PaymentCaseVM()
                {
                    Id = x.Id,
                    PersonNames = string.Join(", ", x.ObligationPayments
                                  .Where(a => a.IsActive == true && a.Obligation.CaseSessionAct.CaseId == caseId)
                                  .Select(a => a.Obligation.FullName).Distinct()),
                    AmountForCase = x.ObligationPayments
                                  .Where(a => a.IsActive == true && a.Obligation.CaseSessionAct.CaseId == caseId)
                                  .Select(a => a.Amount).Sum(),
                    AmountForPayment = x.Amount,
                    MoneyTypeNames = string.Join(", ", x.ObligationPayments
                                  .Where(a => a.IsActive == true && a.Obligation.CaseSessionAct.CaseId == caseId)
                                  .Select(a => a.Obligation.MoneyType.Label).Distinct()),
                    PaidDate = x.PaidDate,
                    PaymentTypeName = x.PaymentType.Label
                }).AsQueryable();
        }

        /// <summary>
        /// Извличане на изпълнителни листове за дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IQueryable<ExecListVM> ExecListForCase_Select(int caseId)
        {
            return repo.AllReadonly<ExecList>()
                .Where(x => x.IsActive == true)
                .Where(x => x.ExecListObligations.Where(a => a.Obligation.CaseId == caseId).Any())
                .Where(x => x.RegDate != null)
                .Select(x => new ExecListVM()
                {
                    Id = x.Id,
                    RegNumber = x.RegNumber,
                    RegDate = x.RegDate,
                    ExecListTypeName = x.ExecListType.Label,
                    FullName = string.Join(",", x.ExecListObligations.Select(o => o.Obligation.FullName).Distinct()),
                    Amount = x.ExecListObligations.Select(o => o.Obligation.Amount).DefaultIfEmpty(0).Sum(),
                }).AsQueryable();
        }

        /// <summary>
        /// РКО за дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IQueryable<ExpenseOrderVM> ExpenseOrderForCase_Select(int caseId)
        {
            return repo.AllReadonly<ExpenseOrder>()
                .Where(x => x.IsActive == true)
                .Where(x => x.ExpenseOrderObligations.Where(a => a.Obligation.CaseId == caseId).Any())
                .Select(x => new ExpenseOrderVM()
                {
                    Id = x.Id,
                    RegNumber = x.RegNumber,
                    RegDate = x.RegDate,
                    FullName = string.Join(",", x.ExpenseOrderObligations.Select(o => o.Obligation.FullName).Distinct()),
                    Amount = x.ExpenseOrderObligations.Select(o => o.Obligation.Amount).DefaultIfEmpty(0).Sum(),
                }).AsQueryable();
        }

        /// <summary>
        /// Извличане на данни за протокол за ИЛ
        /// </summary>
        /// <param name="execListIds"></param>
        /// <returns></returns>
        private (List<ExecList> execList, List<int> institutions) ReadDataForSaveExchangeDoc(string execListIds)
        {
            List<string> idList = new List<string>();
            idList = execListIds.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();

            var execList = repo.AllReadonly<ExecList>()
                .Include(x => x.OutDocument)
                .Include(x => x.OutDocument.DocumentPersons)
                .Where(x => idList.Contains(x.Id.ToString()))
                .ToList();

            var institutionNaps = repo.AllReadonly<Institution>()
                                  .Where(x => x.InstitutionTypeId == NomenclatureConstants.InstitutionTypes.NAP)
                                  .Select(x => x.Id)
                                  .ToList();

            var institutions = execList.Where(x => x.OutDocumentId != null &&
                     x.OutDocument.DocumentPersons.Where(a => a.Person_SourceType == SourceTypeSelectVM.Instutution &&
                                    institutionNaps.Contains(Convert.ToInt32(a.Person_SourceId ?? 0))).Any()
                     )
                .Select(x => x.OutDocument.DocumentPersons.Where(a => a.Person_SourceType == SourceTypeSelectVM.Instutution &&
                                      institutionNaps.Contains(Convert.ToInt32(a.Person_SourceId ?? 0)))
                                    .Select(a => Convert.ToInt32(a.Person_SourceId ?? 0))
                                    .FirstOrDefault()
                                    )
                .Distinct()
                .ToList();

            return (execList: execList, institutions: institutions);
        }

        /// <summary>
        /// Валидация на протокол
        /// </summary>
        /// <param name="execList"></param>
        /// <param name="institutions"></param>
        /// <returns></returns>
        private (bool result, string errorMessage) VaidateExchangeDoc(List<ExecList> execList, List<int> institutions)
        {
            if (execList.Count() == 0)
            {
                return (result: false, errorMessage: "Изберете ИЛ");
            }

            bool hasTypeThirdPerson = execList.Where(x => x.ExecListTypeId != NomenclatureConstants.ExecListTypes.Country).Any();
            if (hasTypeThirdPerson == true)
            {
                return (result: false, errorMessage: "Има ИЛ, който не е в полза на Държавата");
            }

            bool hasNoActive = execList.Where(x => x.IsActive == false).Any();
            if (hasNoActive == true)
            {
                return (result: false, errorMessage: "Има ИЛ, който е деактивиран");
            }

            bool noOutDocument = execList.Where(x => x.OutDocumentId == null).Any();
            if (noOutDocument == true)
            {
                return (result: false, errorMessage: "Има ИЛ без възлагателно писмо");
            }

            bool noToNAP = execList.Where(x => x.OutDocument.DocumentPersons
                         .Where(a => a.Person_SourceType == SourceTypeSelectVM.Instutution &&
                         institutions.Contains(Convert.ToInt32(a.Person_SourceId ?? 0))).Any() == false)
                 .Any();
            if (noToNAP == true)
            {
                return (result: false, errorMessage: "Има ИЛ без възлагателно писмо до ТД на НАП");
            }


            if (institutions.Count != 1)
            {
                return (result: false, errorMessage: "ИЛ са с възлагателни писма до повече от една ТД на НАП");
            }

            List<int> execListIds = execList.Select(x => x.Id).ToList();
            bool existsExchange = repo.AllReadonly<ExchangeDoc>()
                                       .Where(x => x.IsActive == true)
                                       .Where(x => x.ExchangeDocExecLists.Where(o => execListIds.Contains(o.ExecListId)).Any()).Any();
            if (existsExchange == true)
            {
                return (result: false, errorMessage: "Има ИЛ влезнали в друг протокол");
            }

            return (result: true, errorMessage: "");
        }

        /// <summary>
        /// Запис на протокол
        /// </summary>
        /// <param name="execListIds"></param>
        /// <returns></returns>
        public (bool result, string errorMessage, int id) ExchangeDoc_Save(string execListIds)
        {
            try
            {
                (List<ExecList> execList, List<int> institutions) = ReadDataForSaveExchangeDoc(execListIds);
                (bool result, string errorMessage) = VaidateExchangeDoc(execList, institutions);
                if (result == false)
                {
                    return (result: result, errorMessage: errorMessage, id: 0);
                }

                ExchangeDoc saved = new ExchangeDoc();
                saved.CourtId = execList[0].CourtId;
                saved.IsActive = true;
                saved.InstitutionId = institutions[0];
                saved.UserId = userContext.UserId;
                saved.DateWrt = DateTime.Now;

                foreach (var item in execList)
                {
                    ExchangeDocExecList itemAdd = new ExchangeDocExecList();
                    itemAdd.ExecListId = item.Id;
                    saved.ExchangeDocExecLists.Add(itemAdd);
                }

                repo.Add<ExchangeDoc>(saved);
                repo.SaveChanges();

                return (result: true, errorMessage: "", id: saved.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на ExchangeDoc_Save execListIds={ execListIds }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed, id: 0);
            }
        }

        /// <summary>
        /// Извличане на данни за протоколи
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<ExchangeDocVM> ExchangeDoc_Select(int courtId, ExchangeDocFilterVM model)
        {
            DateTime fromDateNull = (model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom).ForceStartDate();
            DateTime toDateNull = (model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo).ForceEndDate();
            Expression<Func<ExchangeDoc, bool>> dateWhere = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateWhere = x => x.RegDate!= null && x.RegDate >= fromDateNull && x.RegDate <= toDateNull;

            Expression<Func<ExchangeDoc, bool>> institutionWhere = x => true;
            if (model.InstitutionId > 0)
                institutionWhere = x => x.InstitutionId == model.InstitutionId;

            Expression<Func<ExchangeDoc, bool>> numberWhere = x => true;
            if (!string.IsNullOrEmpty(model.RegNumber))
                numberWhere = x => x.RegNumber.Contains(model.RegNumber);

            return repo.AllReadonly<ExchangeDoc>()
                .Where(x => x.CourtId == courtId)
                .Where(dateWhere)
                .Where(institutionWhere)
                .Where(numberWhere)
                .Select(x => new ExchangeDocVM()
                {
                    Id = x.Id,
                    RegNumber = x.RegNumber,
                    RegDate = x.RegDate,
                    IsActive = x.IsActive,
                    InstitutionName = x.Institution.FullName,
                    Amount = x.ExchangeDocExecLists
                             .Select(a => a.ExecList.ExecListObligations.Select(o => o.Obligation.Amount).DefaultIfEmpty(0).Sum()).Sum(),
                }).AsQueryable();
        }

        /// <summary>
        /// Сторно на протокол
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) ExchangeDoc_Storno(int id)
        {
            try
            {
                ExchangeDoc storno = GetById<ExchangeDoc>(id);
                if (storno == null)
                {
                    return (result: false, errorMessage: "Невалиден протокол");
                }
                if (storno.IsActive == false)
                {
                    return (result: false, errorMessage: "Протоколът вече е деактивиран");
                }

                storno.IsActive = false;
                storno.UserId = userContext.UserId;
                storno.DateWrt = DateTime.Now;
                repo.Update(storno);
                repo.SaveChanges();

                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на ExchangeDoc_Storno Id={ id }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Извличане на данни за протокол
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ExchangeDocEditVM ExchangeDoc_GetById(int id)
        {
            return repo.AllReadonly<ExchangeDoc>()
                .Where(x => x.Id == id)
                .Select(x => new ExchangeDocEditVM()
                {
                    Id = x.Id,
                    InstitutionId = x.InstitutionId,
                    InstitutionName = x.Institution.FullName,
                    RegNumber = x.RegNumber,
                    RegDate = x.RegDate,
                })
                .FirstOrDefault();
        }

        /// <summary>
        /// Извличане на данни за справка за изпълнителни листове
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <param name="newLine"></param>
        /// <returns></returns>
        public IQueryable<ExecListVM> ExecListReport_Select(int courtId, ExecListFilterVM model, string newLine)
        {
            DateTime fromDateNull = (model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom).Date;
            DateTime toDateNull = (model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo).Date;
            Expression<Func<ExecList, bool>> dateWhere = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateWhere = x => x.RegDate >= fromDateNull.ForceStartDate() && x.RegDate <= toDateNull.ForceEndDate();

            Expression<Func<ExecList, bool>> nameWhere = x => true;
            if (!string.IsNullOrEmpty(model.FullName))
                nameWhere = x => x.ExecListObligations.Where(o => o.Obligation.FullName.ToLower().Contains(model.FullName.ToLower())).Any();

            Expression<Func<ExecList, bool>> nameReceiveWhere = x => true;
            if (!string.IsNullOrEmpty(model.FullNameReceive))
                nameReceiveWhere = x => x.ExecListObligations.Where(o => o.Obligation.ObligationReceives.Where(a => a.FullName.ToLower().Contains(model.FullNameReceive.ToLower())).Any()).Any();

            Expression<Func<ExecList, bool>> execListNumberWhere = x => true;
            if (!string.IsNullOrEmpty(model.RegNumber))
                execListNumberWhere = x => x.RegNumber.Contains(model.RegNumber);

            Expression<Func<ExecList, bool>> execListTypeWhere = x => true;
            if (model.ExecListTypeId > 0)
                execListTypeWhere = x => x.ExecListTypeId == model.ExecListTypeId;

            Expression<Func<ExecList, bool>> institutionWhere = x => true;
            if (model.InstitutionId > 0)
                institutionWhere = x => x.OutDocument.DocumentPersons
                                 .Where(a => a.Person_SourceType == SourceTypeSelectVM.Instutution &&
                                           a.Person_SourceId == model.InstitutionId).Any();

            return repo.AllReadonly<ExecList>()
                .Where(x => x.CourtId == courtId && x.IsActive)
                .Where(x => x.RegDate != null)
                .Where(dateWhere)
                .Where(nameWhere)
                .Where(nameReceiveWhere)
                .Where(execListNumberWhere)
                .Where(execListTypeWhere)
                .Where(institutionWhere)
                .Select(x => new ExecListVM()
                {
                    RegNumber = x.RegNumber,
                    CaseData = x.ExecListObligations.Select(o => o.Obligation.Case.CaseType.Code + " " +
                                          o.Obligation.Case.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                                          a.DateTo == null && a.CourtDepartmentId != null)
                                          .Select(a => a.CourtDepartment.Label).DefaultIfEmpty("").FirstOrDefault()).FirstOrDefault(),
                    CaseId = x.ExecListObligations.Select(o => o.Obligation.Case.Id).FirstOrDefault(),
                    SessionAct = string.Join(newLine, x.ExecListObligations.Select(a =>
                                    a.Obligation.CaseSessionAct.ActType.Label + " " +
                                   (a.Obligation.CaseSessionAct.ActDate != null ? (a.Obligation.CaseSessionAct.RegNumber + "/" +
                                   ((DateTime)a.Obligation.CaseSessionAct.ActDate).ToString("dd.MM.yyyy")) : "")
                                   ).Distinct()),
                    MoneyTypeName = string.Join(newLine, x.ExecListObligations.Select(a =>
                                    (a.Obligation.MoneyType.Label + " " + (a.Obligation.Description ?? "") + " " + (a.Obligation.MoneyFineType.Label ?? "")).Trim())
                                   .Distinct()),
                    Amount = x.ExecListObligations.Select(o => o.Amount ?? 0).DefaultIfEmpty(0).Sum(),
                    FullName = string.Join("<br>", x.ExecListObligations.Select(o => o.Obligation.FullName).Distinct()),
                    FullNameReceive = string.Join("<br>", x.ExecListObligations.Select(o => o.Obligation.ObligationReceives.Select(a => a.FullName).FirstOrDefault()).Distinct()),
                    RegDate = x.RegDate,
                }).AsQueryable();
        }

        /// <summary>
        /// Експорт в ексел на справка за изпълнителни листове
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public byte[] ExecListReportToExcelOne(ExecListFilterVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = ExecListReport_Select(userContext.CourtId, model, Environment.NewLine).ToList();

            string dateFrom = model.DateFrom != null ? ((DateTime)model.DateFrom).ToString("dd.MM.yyyy") : "";
            string dateTo = model.DateTo != null ? ((DateTime)model.DateTo).ToString("dd.MM.yyyy") : "";
            excelService.AddRange("СПРАВКА ЗА ИЗПЪЛНИТЕЛНИ ЛИСТОВЕ", 8,
                      excelService.CreateTitleStyle()); excelService.AddRow();
            excelService.AddRange("за периода от " + dateFrom + " до " + dateTo, 8,
                      excelService.CreateTitleStyle()); excelService.AddRow();
            excelService.AddList(
                dataRows,
                new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<ExecListVM, object>>>()
                {
                    x => x.RegNumber,
                    x => x.CaseData,
                    x => x.SessionAct,
                    x => x.MoneyTypeName,
                    x => x.Amount,
                    x => x.FullName,
                    x => x.FullNameReceive,
                    x => x.RegDate,
                },
                //NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                //NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );
            excelService.AddRow();
            excelService.colIndex = 3;
            excelService.AddCell("Всичко");
            excelService.AddCell(dataRows.Select(x => x.Amount).Sum().ToString("0.00"));
            return excelService.ToArray();
        }

        /// <summary>
        /// Извличане на данните за задължения в полза на трети лица
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<ObligationThirdPersonVM> ObligationThirdPerson_Select(int courtId, ObligationThirdPersonFilterVM model)
        {
            Expression<Func<Obligation, bool>> uicSearch = x => true;
            if (!string.IsNullOrEmpty(model.PersonUicSearch))
                uicSearch = x => x.Uic.ToLower() == model.PersonUicSearch.ToLower();

            Expression<Func<Obligation, bool>> nameSearch = x => true;
            if (!string.IsNullOrEmpty(model.PersonNameSearch))
                nameSearch = x => x.FullName.ToLower().Contains(model.PersonNameSearch.ToLower());

            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<Obligation, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.ObligationDate.Date >= dateFromSearch.Date && x.ObligationDate.Date <= dateToSearch.Date;

            Expression<Func<Obligation, bool>> moneyTypeWhere = x => true;
            if (model.MoneyTypeId > 0)
                moneyTypeWhere = x => x.MoneyTypeId == model.MoneyTypeId;

            Expression<Func<Obligation, bool>> caseRegnumberSearch = x => true;
            if (!string.IsNullOrEmpty(model.CaseRegNumber))
                caseRegnumberSearch = x => x.Case.RegNumber.ToLower().Contains(model.CaseRegNumber.ToLower());

            return repo.AllReadonly<Obligation>()
           .Where(x => x.CourtId == courtId && (x.IsActive ?? true))
           .Where(uicSearch)
           .Where(nameSearch)
           .Where(dateSearch)
           .Where(moneyTypeWhere)
           .Where(caseRegnumberSearch)
           .Where(x => x.ObligationReceives.Where(a => a.ExecListTypeId == NomenclatureConstants.ExecListTypes.ThirdPerson).Any())
           .Select(x => new ObligationThirdPersonVM()
           {
               Id = x.Id,
               ObligationDate = x.ObligationDate,
               PersonName = (x.FullName ?? "") + " " + (x.Uic ?? ""),
               PersonReceiveName = x.ObligationReceives.Select(a => (a.FullName ?? "") + " " + (a.Uic ?? "")).FirstOrDefault(),
               MoneyTypeName = x.MoneyType.Label,
               Amount = x.Amount,
               CaseData = x.Case.CaseGroup.Code + " " + x.Case.RegNumber,
               ObligationInfo = x.ObligationInfo ?? "",
               RegNumberExecList = x.ExecListObligations.Where(o => o.ExecList.IsActive == true).Select(o => o.ExecList.RegNumber).DefaultIfEmpty("").FirstOrDefault(),
               ExecListId = x.ExecListObligations.Where(o => o.ExecList.IsActive == true).Select(o => o.ExecListId).FirstOrDefault(),
           }).AsQueryable();
        }

        public SaveResultVM ExecListRegister(ExecList model)
        {
            if (model.RegDate != null)
            {
                return new SaveResultVM(true);
            }

            if (counterService.Counter_GetExecListCounter(model) == true)
            {
                repo.Update(model);
                repo.SaveChanges();
                return new SaveResultVM(true, null, "register");
            }
            else
            {
                return new SaveResultVM(false, "Проблем при регистриране на ИЛ.");
            }
        }
    }
}
