// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IOWebApplication.Core.Services
{
    public class CaseFastProcessService : BaseService, ICaseFastProcessService
    {
        private readonly IPriceService priceService;

        public CaseFastProcessService(
            ILogger<CaseFastProcessService> _logger,
            IRepository _repo,
            IPriceService _priceService,
            IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            priceService = _priceService;
        }

        /// <summary>
        /// Метод за изчитане на Вземане към обстоятелство по заповедни производства
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        private List<CaseMoneyCollectionVM> CaseMoneyCollections_Select(int CaseId)
        {
            return repo.AllReadonly<CaseMoneyCollection>()
                       .Include(x => x.CaseMoneyCollectionGroup)
                       .Include(x => x.CaseMoneyCollectionType)
                       .Include(x => x.CaseMoneyCollectionKind)
                       .Include(x => x.Currency)
                       .Include(x => x.MoneyCollectionEndDateType)
                       .Where(x => x.CaseId == CaseId)
                       .Select(x => new CaseMoneyCollectionVM()
                       {
                           Id = x.Id,
                           CaseId = x.CaseId,
                           MainCaseMoneyCollectionId = x.MainCaseMoneyCollectionId,
                           CaseMoneyClaimId = x.CaseMoneyClaimId,
                           CaseMoneyCollectionGroupId = x.CaseMoneyCollectionGroupId,
                           CaseMoneyCollectionGroupLabel = x.CaseMoneyCollectionGroup.Label,
                           CaseMoneyCollectionTypeLabel = (!string.IsNullOrEmpty(x.Label)) ? x.Label : x.CaseMoneyCollectionType.Label,
                           CaseMoneyCollectionKindLabel = (!string.IsNullOrEmpty(x.Label)) ? x.Label : x.CaseMoneyCollectionKind.Label,
                           CurrencyLabel = x.Currency.Label,
                           CurrencyCode = x.Currency.Code,
                           CurrencyId = x.CurrencyId,
                           InitialAmount = x.InitialAmount,
                           InitialAmountString = Extensions.MoneyExtensions.MoneyToString(x.InitialAmount, x.CurrencyId),
                           PretendedAmount = x.PretendedAmount,
                           PretendedAmountString = Extensions.MoneyExtensions.MoneyToString(x.PretendedAmount, x.CurrencyId),
                           RespectedAmount = x.RespectedAmount,
                           RespectedAmountString = Extensions.MoneyExtensions.MoneyToString(x.RespectedAmount, x.CurrencyId),
                           DateFrom = x.DateFrom,
                           DateToLabel = (x.MoneyCollectionEndDateTypeId == null) ? "до " + (x.DateTo ?? DateTime.Now).ToString("dd.MM.yyyy") : 
                                                                                    ((x.MoneyCollectionEndDateTypeId == NomenclatureConstants.MoneyCollectionEndDateType.WithDate) ? "до " + (x.DateTo ?? DateTime.Now).ToString("dd.MM.yyyy") : 
                                                                                                                                                                                     ((x.MoneyCollectionEndDateTypeId == NomenclatureConstants.MoneyCollectionEndDateType.Nothing) ? string.Empty : "до " + x.MoneyCollectionEndDateType.Label)),
                           Description = x.Description,
                           JointDistributionBool = x.JointDistribution,
                           JointDistribution = (x.JointDistribution) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No,
                           IsMoney = (NomenclatureConstants.CaseMoneyCollectionGroup.Money == x.CaseMoneyCollectionGroupId),
                           IsMovables = (NomenclatureConstants.CaseMoneyCollectionGroup.Movables == x.CaseMoneyCollectionGroupId),
                           IsItem = (NomenclatureConstants.CaseMoneyCollectionGroup.Property == x.CaseMoneyCollectionGroupId)
                       })
                       .ToList();
        }

        /// <summary>
        /// Изчитане на лица към Вземане към обстоятелство по заповедни производства
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        private List<CaseMoneyCollectionPersonVM> CaseMoneyCollectionPersons_Select(int CaseId)
        {
            return repo.AllReadonly<CaseMoneyCollectionPerson>()
                       .Include(x => x.CasePerson)
                       .Include(x => x.CaseMoneyCollection)
                       .ThenInclude(x => x.Currency)
                       .Where(x => x.CaseId == CaseId)
                       .Select(x => new CaseMoneyCollectionPersonVM()
                       {
                           Id = x.Id,
                           CaseId = x.CaseId,
                           CaseMoneyCollectionId = x.CaseMoneyCollectionId,
                           CasePersonLabel = x.CasePerson.FullName,
                           PersonAmount = x.PersonAmount,
                           PersonAmountString = Extensions.MoneyExtensions.MoneyToString(x.PersonAmount, x.CaseMoneyCollection.Currency.Id),
                           RespectedAmount = x.RespectedAmount,
                           RespectedAmountString = Extensions.MoneyExtensions.MoneyToString(x.RespectedAmount, x.CaseMoneyCollection.Currency.Id),
                           CurrencyCode = x.CaseMoneyCollection.Currency.Code
                       })
                       .ToList();
        }

        /// <summary>
        /// Метод за сетване на лица към Обстоятелства по заповедни производства
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="caseMoneyClaims"></param>
        private void SetMoneyCollections(int CaseId, List<CaseMoneyClaimVM> caseMoneyClaims)
        {
            var caseMoneyCollections = CaseMoneyCollections_Select(CaseId);
            var caseMoneyCollectionPeople = CaseMoneyCollectionPersons_Select(CaseId);

            foreach (var moneyCollectionVM in caseMoneyCollections)
            {
                moneyCollectionVM.MoneyCollectionPersons = caseMoneyCollectionPeople.Where(x => x.CaseMoneyCollectionId == moneyCollectionVM.Id).ToList();
            }

            foreach (var caseMoneyClaim in caseMoneyClaims)
            {
                caseMoneyClaim.CaseMoneyCollections = caseMoneyCollections.Where(x => x.CaseMoneyClaimId == caseMoneyClaim.Id && x.MainCaseMoneyCollectionId == null).ToList();
                caseMoneyClaim.FullSum = caseMoneyClaim.CaseMoneyCollections.Sum(x => x.RespectedAmount);
                foreach (var caseMoneyCollection in caseMoneyClaim.CaseMoneyCollections)
                {
                    caseMoneyCollection.CaseMoneyCollectionExtras = caseMoneyCollections.Where(x => x.CaseMoneyClaimId == caseMoneyClaim.Id && x.MainCaseMoneyCollectionId == caseMoneyCollection.Id).ToList();
                    caseMoneyClaim.FullSum += caseMoneyCollection.CaseMoneyCollectionExtras.Sum(x => x.RespectedAmount);
                }
                caseMoneyClaim.FullSumText = Extensions.MoneyExtensions.MoneyToString(caseMoneyClaim.FullSum, caseMoneyClaim.CaseMoneyCollections.Select(x => x.CurrencyId).FirstOrDefault());
            }
        }

        /// <summary>
        /// Метод за изчитане на Обстоятелства по заповедни производства
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        private List<CaseMoneyClaimVM> CaseMoneyClaim_Select(int CaseId)
        {
            var caseMoneyClaims = repo.AllReadonly<CaseMoneyClaim>()
                                      .Include(x => x.CaseMoneyClaimGroup)
                                      .Include(x => x.CaseMoneyClaimType)
                                      .Where(x => x.CaseId == CaseId)
                                      .Select(x => new CaseMoneyClaimVM()
                                      {
                                          Id = x.Id,
                                          CaseMoneyClaimGroupLabel = x.CaseMoneyClaimGroup.Label,
                                          CaseMoneyClaimTypeLabel = x.CaseMoneyClaimType.Label,
                                          ClaimNumber = x.ClaimNumber,
                                          ClaimDate = x.ClaimDate,
                                          Description = x.Description,
                                          Motive = x.Motive
                                      })
                                      .OrderBy(x => x.Id)
                                      .ToList();

            SetMoneyCollections(CaseId, caseMoneyClaims);

            return caseMoneyClaims;
        }

        /// <summary>
        /// Изчитане на Заповедни дела
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public CaseFastProcessVM CaseFastProcess_Select(int CaseId)
        {
            var caseFastProcesses = repo.AllReadonly<CaseFastProcess>()
                                      .Include(x => x.Currency)
                                      .Where(x => x.CaseId == CaseId)
                                      .Select(x => new CaseFastProcessVM()
                                      {
                                          Id = x.Id,
                                          Description = x.Description,
                                          TaxAmount = x.TaxAmount,
                                          TaxAmountString = Extensions.MoneyExtensions.MoneyToString(x.TaxAmount, x.CurrencyId),
                                          CurrencyLabel = x.Currency.Label,
                                          CurrencyCode = x.Currency.Code,
                                      })
                                      .DefaultIfEmpty()
                                      .FirstOrDefault();

            return caseFastProcesses ?? new CaseFastProcessVM();
        }

        /// <summary>
        /// Изчитане на Претендиран разноски по заповедни производства
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        private List<CaseMoneyExpenseVM> CaseMoneyExpense_Select(int CaseId)
        {
            var caseMoneyExpenses = repo.AllReadonly<CaseMoneyExpense>()
                                      .Include(x => x.CaseMoneyExpenseType)
                                      .Include(x => x.Currency)
                                      .Where(x => x.CaseId == CaseId)
                                      .Select(x => new CaseMoneyExpenseVM()
                                      {
                                          Id = x.Id,
                                          CaseMoneyExpenseTypeLabel = x.CaseMoneyExpenseType.Label,
                                          CurrencyLabel = x.Currency.Label,
                                          CurrencyCode = x.Currency.Code,
                                          Amount = x.Amount,
                                          AmountString = Extensions.MoneyExtensions.MoneyToString(x.Amount, x.CurrencyId),
                                          Description = x.Description,
                                          JointDistributionBool = (x.JointDistribution ?? true),
                                          JointDistribution = (x.JointDistribution ?? true) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No,
                                      })
                                      .ToList();

            var caseMoneyExpensePeople = CaseMoneyExpensePerson_Select(CaseId);

            foreach (var caseMoney in caseMoneyExpenses)
            {
                caseMoney.MoneyExpensePeople = caseMoneyExpensePeople.Where(x => x.CaseMoneyExpenseId == caseMoney.Id).ToList();
            }

            return caseMoneyExpenses;
        }

        private List<CaseMoneyExpensePersonVM> CaseMoneyExpensePerson_Select(int CaseId)
        {
            return repo.AllReadonly<CaseMoneyExpensePerson>()
                       .Include(x => x.CasePerson)
                       .Include(x => x.CaseMoneyExpense)
                       .ThenInclude(x => x.Currency)
                       .Where(x => x.CaseId == CaseId)
                       .Select(x => new CaseMoneyExpensePersonVM()
                       {
                           Id = x.Id,
                           CaseId = x.CaseId,
                           CaseMoneyExpenseId = x.CaseMoneyExpenseId,
                           CasePersonLabel = x.CasePerson.FullName,
                           PersonAmount = x.PersonAmount,
                           PersonAmountString = Extensions.MoneyExtensions.MoneyToString(x.PersonAmount, x.CaseMoneyExpense.Currency.Id),
                           CurrencyCode = x.CaseMoneyExpense.Currency.Code
                       })
                       .ToList();
        }

        /// <summary>
        /// Метод за изчитане на Начин на плащане/изпълнение, Заповедни производства
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        private List<CaseBankAccountVM> CaseBankAccount_Select(int CaseId)
        {
            var caseMoneyExpenses = repo.AllReadonly<CaseBankAccount>()
                                      .Include(x => x.CaseBankAccountType)
                                      .Where(x => x.CaseId == CaseId)
                                      .Select(x => new CaseBankAccountVM()
                                      {
                                          Id = x.Id,
                                          LabelIBAN = x.IBAN,
                                          LabelBIC = x.BIC,
                                          BankName = x.BankName,
                                          CaseBankAccountTypeId = x.CaseBankAccountTypeId,
                                          CaseBankAccountTypeLabel = x.CaseBankAccountType.Label,
                                          Description = x.Description,
                                          IsBankAccount = (x.CaseBankAccountTypeId == NomenclatureConstants.CaseBankAccountType.BankAccount)
                                      })
                                      .ToList();

            return caseMoneyExpenses;
        }

        /// <summary>
        /// Изчитане на всички данни за заповедно производство по дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public CaseFastProcessViewVM Select(int CaseId)
        {
            CaseFastProcessViewVM result = new CaseFastProcessViewVM();
            result.CaseMoneyClaims = CaseMoneyClaim_Select(CaseId);
            result.CaseMoneyExpenses = CaseMoneyExpense_Select(CaseId);
            result.FastProcessVM = CaseFastProcess_Select(CaseId);
            result.CaseBankAccounts = CaseBankAccount_Select(CaseId);

            return result;
        }

        /// <summary>
        /// Запис на Обстоятелства по заповедни производства
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int CaseMoneyClaim_SaveData(CaseMoneyClaim model)
        {
            try
            {
                model.CaseMoneyClaimTypeId = model.CaseMoneyClaimTypeId.EmptyToNull(0);
                if (model.Id > 0)
                {
                    var saved = repo.GetById<CaseMoneyClaim>(model.Id);
                    saved.CaseMoneyClaimTypeId = model.CaseMoneyClaimTypeId;
                    saved.ClaimNumber = model.ClaimNumber;
                    saved.ClaimDate = model.ClaimDate;
                    saved.PartyNames = model.PartyNames;
                    saved.Description = model.Description;
                    saved.Motive = model.Motive;
                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;
                    repo.Update(saved);
                    repo.SaveChanges();
                    return saved.Id;
                }
                else
                {
                    model.UserId = userContext.UserId;
                    model.DateWrt = DateTime.Now;
                    repo.Add(model);
                    repo.SaveChanges();
                    return model.Id;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CaseMoneyClaim_SaveData id={0};caseid={1}", model.Id, model.CaseId);
                return -1; ;
            }
        }

        /// <summary>
        /// Метод за преливане от модел за редекция към основен модел за Вземане към обстоятелство по заповедни производства
        /// </summary>
        /// <param name="modelEdit"></param>
        /// <param name="model"></param>
        private void FillObjectFromEditModelToModel(CaseMoneyCollectionEditVM modelEdit, CaseMoneyCollection model)
        {
            model.CaseId = modelEdit.CaseId;
            model.CourtId = modelEdit.CourtId;
            model.MainCaseMoneyCollectionId = modelEdit.MainCaseMoneyCollectionId;
            model.CaseMoneyClaimId = modelEdit.CaseMoneyClaimId;

            model.CaseMoneyCollectionGroupId = modelEdit.CaseMoneyCollectionGroupId;

            model.CaseMoneyCollectionTypeId = (NomenclatureConstants.CaseMoneyCollectionGroup.Money == modelEdit.CaseMoneyCollectionGroupId) ? modelEdit.Money_CaseMoneyCollectionTypeId : modelEdit.Movables_CaseMoneyCollectionTypeId;
            model.CaseMoneyCollectionKindId = modelEdit.CaseMoneyCollectionKindId;
            model.CurrencyId = modelEdit.CurrencyId;

            if (NomenclatureConstants.CaseMoneyCollectionGroup.Money == modelEdit.CaseMoneyCollectionGroupId)
            {
                model.InitialAmount = modelEdit.InitialAmount;
                model.PretendedAmount = modelEdit.PretendedAmount;
                model.RespectedAmount = modelEdit.RespectedAmount;
            }
            else
                model.PretendedAmount = modelEdit.Amount;

            model.Label = modelEdit.Label;
            model.MoneyCollectionEndDateTypeId = modelEdit.MoneyCollectionEndDateTypeId;
            model.DateFrom = modelEdit.DateFrom;
            model.DateTo = modelEdit.DateTo;
            model.JointDistribution = modelEdit.JointDistribution;
            model.Description = modelEdit.Description;
            model.UserId = userContext.UserId;
            model.DateWrt = DateTime.Now;

            //var caseFastProcess = repo.AllReadonly<CaseFastProcess>().Where(x => x.CaseId == model.CaseId).DefaultIfEmpty().FirstOrDefault();
            //if (caseFastProcess == null)
            //{
            //    model.RespectedAmount = model.PretendedAmount;
            //}
            //else
            //{
            //    if (caseFastProcess.IsRespectedAmount != true)
            //        model.RespectedAmount = model.PretendedAmount;
            //}
        }

        /// <summary>
        /// Пълнене модел за редакция от основен модел за Вземане към обстоятелство по заповедни производства
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private CaseMoneyCollectionEditVM FillObjectFromModelToEditModel(CaseMoneyCollection model)
        {
            CaseMoneyCollectionEditVM caseMoneyCollectionEditVM = new CaseMoneyCollectionEditVM()
            {
                Id = model.Id,
                CourtId = model.CourtId,
                CaseId = model.CaseId,
                MainCaseMoneyCollectionId = model.MainCaseMoneyCollectionId,
                CaseMoneyClaimId = model.CaseMoneyClaimId,
                CaseMoneyCollectionGroupId = model.CaseMoneyCollectionGroupId,
                Money_CaseMoneyCollectionTypeId = (NomenclatureConstants.CaseMoneyCollectionGroup.Money == model.CaseMoneyCollectionGroupId) ? model.CaseMoneyCollectionTypeId : null,
                Movables_CaseMoneyCollectionTypeId = (NomenclatureConstants.CaseMoneyCollectionGroup.Money != model.CaseMoneyCollectionGroupId) ? model.CaseMoneyCollectionTypeId : null,
                CaseMoneyCollectionKindId = model.CaseMoneyCollectionKindId,
                InitialAmount = model.InitialAmount,
                PretendedAmount = (NomenclatureConstants.CaseMoneyCollectionGroup.Money == model.CaseMoneyCollectionGroupId) ? model.PretendedAmount : 0,
                RespectedAmount = (NomenclatureConstants.CaseMoneyCollectionGroup.Money == model.CaseMoneyCollectionGroupId) ? model.RespectedAmount : 0,
                Amount = (NomenclatureConstants.CaseMoneyCollectionGroup.Money != model.CaseMoneyCollectionGroupId) ? model.PretendedAmount : 0,
                Label = model.Label,
                MoneyCollectionEndDateTypeId = model.MoneyCollectionEndDateTypeId ?? NomenclatureConstants.MoneyCollectionEndDateType.WithDate,
                DateFrom = model.DateFrom,
                DateTo = model.DateTo,
                JointDistribution = model.JointDistribution,
                Description = model.Description,
                CurrencyId = model.CurrencyId,
                CasePersonListDecimals = FillPersonList(model.CaseId, model.Id)
            };

            return caseMoneyCollectionEditVM;
        }

        /// <summary>
        /// Изчитане на модел за редакция на Вземане към обстоятелство по заповедни производства
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public CaseMoneyCollectionEditVM GetById_EditVM(int Id)
        {
            var caseMoneyCollection = repo.GetById<CaseMoneyCollection>(Id);
            return (caseMoneyCollection != null) ? FillObjectFromModelToEditModel(caseMoneyCollection) : null;
        }

        /// <summary>
        /// Пълнене на основен модел от модел за редакция на Разпределение на вземане към обстоятелство по длъжници
        /// </summary>
        /// <param name="moneyCollection"></param>
        /// <param name="casePersonListDecimal"></param>
        /// <returns></returns>
        private CaseMoneyCollectionPerson FillMoneyCollectionPerson(CaseMoneyCollection moneyCollection, CasePersonListDecimalVM casePersonListDecimal)
        {
            return new CaseMoneyCollectionPerson()
            {
                CaseId = moneyCollection.CaseId,
                CourtId = moneyCollection.CourtId,
                CaseMoneyCollectionId = moneyCollection.Id,
                CasePersonId = casePersonListDecimal.Id,
                PersonAmount = (moneyCollection.JointDistribution) ? 0 : casePersonListDecimal.ValueOne,
                RespectedAmount = (moneyCollection.JointDistribution) ? 0 : casePersonListDecimal.ValueTwo,
                Description = string.Empty,
                UserId = userContext.UserId,
                DateWrt = DateTime.Now
            };
        }

        private string GetName_CaseMoneyCollectionRespectSum(CaseMoneyCollection model)
        {
            return model.CaseMoneyClaim.CaseMoneyClaimGroup.Label + " " +
                   ((model.CaseMoneyCollectionType != null) ? model.CaseMoneyCollectionType.Label : string.Empty) +
                   ((model.CaseMoneyCollectionKind != null) ? model.CaseMoneyCollectionKind.Label : string.Empty) +
                   " Претендирано: " + model.PretendedAmount.ToString("0.00") + " " + model.Currency.Code;
        }

        private List<CasePersonListDecimalVM> FillCasePersonListDecimals(CaseMoneyCollection model)
        {
            var result = new List<CasePersonListDecimalVM>();

            if (!model.JointDistribution)
            {
                foreach (var collectionPerson in model.CaseMoneyCollectionPersons)
                {
                    result.Add(new CasePersonListDecimalVM()
                    {
                        Id = collectionPerson.CasePersonId,
                        Label = collectionPerson.CasePerson.FullName,
                        ValueOne = collectionPerson.PersonAmount,
                        ValueTwo = collectionPerson.RespectedAmount,
                        LabelOne = "Претендирано",
                        LabelTwo = "Уважено"
                    });
                }
            }

            return result;
        }

        public List<CaseMoneyCollectionRespectSumVM> FillCaseMoneyCollectionRespectSum(int caseId)
        {
            var caseMoneyCollections = repo.AllReadonly<CaseMoneyCollection>()
                                           .Include(x => x.CaseMoneyClaim)
                                           .ThenInclude(x => x.CaseMoneyClaimGroup)
                                           .Include(x => x.Currency)
                                           .Include(x => x.CaseMoneyCollectionType)
                                           .Include(x => x.CaseMoneyCollectionKind)
                                           .Include(x => x.CaseMoneyCollectionPersons)
                                           .ThenInclude(x => x.CasePerson)
                                           .Where(x => x.CaseId == caseId &&
                                                       x.CaseMoneyCollectionGroupId == NomenclatureConstants.CaseMoneyCollectionGroup.Money)
                                           .ToList();

            var result = new List<CaseMoneyCollectionRespectSumVM>();

            foreach (var caseMoney in caseMoneyCollections.Where(x => x.MainCaseMoneyCollectionId == null))
            {
                result.Add(new CaseMoneyCollectionRespectSumVM()
                {
                    Id = caseMoney.Id,
                    CaseId = caseId,
                    Label = GetName_CaseMoneyCollectionRespectSum(caseMoney),
                    Value = caseMoney.RespectedAmount,
                    CasePersonListDecimals = FillCasePersonListDecimals(caseMoney),
                    CaseMoneyCollectionRespectSumOthers = caseMoneyCollections.Where(x => x.MainCaseMoneyCollectionId == caseMoney.Id)
                                                                              .Select(x => new CaseMoneyCollectionRespectSumVM()
                                                                              {
                                                                                  Id = x.Id,
                                                                                  CaseId = caseId,
                                                                                  Label = GetName_CaseMoneyCollectionRespectSum(x),
                                                                                  Value = x.RespectedAmount,
                                                                                  CasePersonListDecimals = FillCasePersonListDecimals(x),
                                                                                  CaseMoneyCollectionRespectSumOthers = new List<CaseMoneyCollectionRespectSumVM>()
                                                                              })
                                                                              .ToList() ?? new List<CaseMoneyCollectionRespectSumVM>()
                });
            }

            return result;
        }

        private void SetRespectedAmountCaseMoneyCollection(CaseMoneyCollection model, decimal value)
        {
            model.RespectedAmount = value;
            model.DateWrt = DateTime.Now;
            model.UserId = userContext.UserId;
        }

        private void SetAmountCaseMoneyCollectionPerson(CaseMoneyCollectionPerson model, decimal value)
        {
            model.PersonAmount = value;
            model.DateWrt = DateTime.Now;
            model.UserId = userContext.UserId;
        }

        public bool CaseMoneyCollectionRespectSum_SaveData(List<CaseMoneyCollectionRespectSumVM> model)
        {
            try
            {
                var caseMoneyCollections = repo.AllReadonly<CaseMoneyCollection>().Where(x => x.CaseId == model[0].CaseId).ToList();
                var caseMoneyCollectionPeople = repo.AllReadonly<CaseMoneyCollectionPerson>().Where(x => x.CaseId == model[0].CaseId).ToList();
                var caseFastProcess = repo.AllReadonly<CaseFastProcess>().Where(x => x.CaseId == model[0].CaseId).FirstOrDefault();

                caseFastProcess.IsRespectedAmount = true;
                caseFastProcess.DateWrt = DateTime.Now;
                caseFastProcess.UserId = userContext.UserId;
                repo.Update(caseFastProcess);

                foreach (var caseMoney in model)
                {
                    var caseMoneyCollection = caseMoneyCollections.Where(x => x.Id == caseMoney.Id).FirstOrDefault();
                    SetRespectedAmountCaseMoneyCollection(caseMoneyCollection, caseMoney.Value);
                    repo.Update(caseMoneyCollection);

                    foreach (var casePerson in caseMoney.CasePersonListDecimals)
                    {
                        var collectionPerson = caseMoneyCollectionPeople.Where(x => x.CasePersonId == casePerson.Id).FirstOrDefault();
                        SetAmountCaseMoneyCollectionPerson(collectionPerson, casePerson.ValueOne);
                        repo.Update(collectionPerson);
                    }

                    foreach (var caseMoneyOther in caseMoney.CaseMoneyCollectionRespectSumOthers)
                    {
                        var caseMoneyCollectionOther = caseMoneyCollections.Where(x => x.Id == caseMoneyOther.Id).FirstOrDefault();
                        SetRespectedAmountCaseMoneyCollection(caseMoneyCollectionOther, caseMoneyOther.Value);
                        repo.Update(caseMoneyCollectionOther);

                        foreach (var casePersonOther in caseMoney.CasePersonListDecimals)
                        {
                            var collectionPersonOther = caseMoneyCollectionPeople.Where(x => x.CasePersonId == casePersonOther.Id).FirstOrDefault();
                            SetAmountCaseMoneyCollectionPerson(collectionPersonOther, casePersonOther.ValueOne);
                            repo.Update(collectionPersonOther);
                        }
                    }
                }
                repo.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CaseMoneyCollectionRespectSum_SaveData caseid={1}", model[0].CaseId);
                return false;
            }
        }

        /// <summary>
        /// Запис на Вземане към обстоятелство по заповедни производства
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int CaseMoneyCollection_SaveData(CaseMoneyCollectionEditVM model)
        {
            model.Money_CaseMoneyCollectionTypeId = model.Money_CaseMoneyCollectionTypeId.EmptyToNull(0);
            model.Movables_CaseMoneyCollectionTypeId = model.Movables_CaseMoneyCollectionTypeId.EmptyToNull(0);
            model.CaseMoneyCollectionKindId = model.CaseMoneyCollectionKindId.EmptyToNull(0);

            try
            {
                CaseMoneyCollection saved;
                if (model.Id > 0)
                {
                    saved = repo.GetById<CaseMoneyCollection>(model.Id);
                    FillObjectFromEditModelToModel(model, saved);
                    repo.Update(saved);

                    if (saved.CaseMoneyCollectionGroupId == NomenclatureConstants.CaseMoneyCollectionGroup.Money)
                    {
                        var caseMoneyCollectionPeople = GetMoneyCollectionPerson(saved.Id);
                        foreach (var casePerson in model.CasePersonListDecimals)
                        {
                            var caseMoneyCollectionPerson = caseMoneyCollectionPeople.Where(x => x.CasePersonId == casePerson.Id).FirstOrDefault();
                            if (caseMoneyCollectionPerson == null)
                            {
                                repo.Add(FillMoneyCollectionPerson(saved, casePerson));
                            }
                            else
                            {
                                caseMoneyCollectionPerson.PersonAmount = (saved.JointDistribution) ? 0 : casePerson.ValueOne;
                                caseMoneyCollectionPerson.RespectedAmount = (saved.JointDistribution) ? 0 : casePerson.ValueTwo;
                                repo.Update(caseMoneyCollectionPerson);
                            }
                        }
                    }

                    repo.SaveChanges();
                }
                else
                {
                    saved = new CaseMoneyCollection();
                    FillObjectFromEditModelToModel(model, saved);
                    repo.Add(saved);
                    if (saved.CaseMoneyCollectionGroupId == NomenclatureConstants.CaseMoneyCollectionGroup.Money)
                    {
                        if (!saved.JointDistribution)
                        {
                            foreach (var casePerson in model.CasePersonListDecimals)
                            {
                                repo.Add(FillMoneyCollectionPerson(saved, casePerson));
                            }
                        }
                    }

                    repo.SaveChanges();
                }

                ReCalcCountryTax(model.CaseId);

                return saved.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CaseMoneyCollection_SaveData id={0};caseid={1}", model.Id, model.CaseId);
                return -1;
            }
        }

        /// <summary>
        /// Метод за смятане на такси към заповедно производство
        /// </summary>
        /// <param name="caseId"></param>
        private void ReCalcCountryTax(int caseId)
        {
            var caseCase = repo.GetById<Case>(caseId);
            var caseMoneyCollections = CaseMoneyCollections_Select(caseId);

            var pcent = priceService.GetPriceValue(null, ((caseCase.CaseCodeId == NomenclatureConstants.CaseCode.Case410) ? NomenclatureConstants.PriceDescKeyWord.KeyMoneyCase410 : NomenclatureConstants.PriceDescKeyWord.KeyMoneyCase417), 0, null, 0, 0, NomenclatureConstants.PriceDescKeyWord.RowMoneyPercent);
            var minvalue = priceService.GetPriceValue(null, ((caseCase.CaseCodeId == NomenclatureConstants.CaseCode.Case410) ? NomenclatureConstants.PriceDescKeyWord.KeyMoneyCase410 : NomenclatureConstants.PriceDescKeyWord.KeyMoneyCase417), 0, null, 0, 0, NomenclatureConstants.PriceDescKeyWord.RowMoneyMinValue);

            var sumAll = caseMoneyCollections.Sum(x => x.PretendedAmount);
            var tax = Math.Round((sumAll * (pcent / 100)), 2);
            tax = (tax > minvalue) ? tax : minvalue;

            var caseFastProcess = repo.AllReadonly<CaseFastProcess>().Where(x => x.CaseId == caseId).DefaultIfEmpty().FirstOrDefault();

            try
            {
                if (caseFastProcess == null)
                {
                    caseFastProcess = new CaseFastProcess()
                    {
                        CaseId = caseId,
                        CourtId = caseCase.CourtId,
                        Description = string.Empty,
                        TaxAmount = tax,
                        CurrencyId = NomenclatureConstants.Currency.BGN,
                        UserId = userContext.UserId,
                        DateWrt = DateTime.Now,
                    };

                    repo.Add(caseFastProcess);
                }
                else
                {
                    caseFastProcess.TaxAmount = tax;
                    caseFastProcess.UserId = userContext.UserId;
                    caseFastProcess.DateWrt = DateTime.Now;
                    repo.Update(caseFastProcess);
                }

                repo.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "FastProcess_ReCalcCountryTax id={0};caseid={1}", caseFastProcess.Id, caseFastProcess.CaseId);
            }
        }

        /// <summary>
        /// Изчитане на лица от дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private List<CasePerson> GetCasePerson(int caseId)
        {
            return repo.AllReadonly<CasePerson>()
                .Include(x => x.PersonRole)
                .Include(x => x.Addresses)
                .Where(x => x.CaseId == caseId &&
                            x.CaseSessionId == null &&
                            x.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.RightSide)
                .ToList();
        }

        /// <summary>
        /// Изчитане на данни за Разпределение на вземане към обстоятелство по длъжници
        /// </summary>
        /// <param name="moneyCollectionId"></param>
        /// <returns></returns>
        private List<CaseMoneyCollectionPerson> GetMoneyCollectionPerson(int moneyCollectionId)
        {
            return repo.AllReadonly<CaseMoneyCollectionPerson>()
                       .Include(x => x.CasePerson)
                       .Where(x => x.CaseMoneyCollectionId == moneyCollectionId)
                       .ToList();
        }

        /// <summary>
        /// Пълнене на лист от модели за редкация на Разпределение на вземане към обстоятелство по длъжници
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="moneyCollectionId"></param>
        /// <returns></returns>
        public List<CasePersonListDecimalVM> FillPersonList(int caseId, int? moneyCollectionId)
        {
            List<CasePersonListDecimalVM> results = new List<CasePersonListDecimalVM>();

            var casePeople = GetCasePerson(caseId);
            var moneyCollectionPeople = GetMoneyCollectionPerson(moneyCollectionId ?? 0);

            foreach (var person in casePeople)
            {
                var collectionPerson = moneyCollectionPeople.Where(x => x.CasePersonId == person.Id).FirstOrDefault();
                if (collectionPerson == null)
                {
                    CasePersonListDecimalVM casePersonList = new CasePersonListDecimalVM()
                    {
                        Id = person.Id,
                        Label = person.FullName,
                        ValueOne = 0,
                        ValueTwo = 0,
                        LabelOne = "Претендирано",
                        LabelTwo = "Уважено"
                    };

                    results.Add(casePersonList);
                }
                else
                {
                    CasePersonListDecimalVM casePersonList = new CasePersonListDecimalVM()
                    {
                        Id = collectionPerson.CasePersonId,
                        Label = collectionPerson.CasePerson.FullName,
                        ValueOne = collectionPerson.PersonAmount,
                        ValueTwo = collectionPerson.RespectedAmount,
                        LabelOne = "Претендирано",
                        LabelTwo = "Уважено"
                    };

                    results.Add(casePersonList);
                }
            }

            return results;
        }

        private CaseMoneyExpenseEditVM FillCaseMoneyExpenseEditVM(CaseMoneyExpense model)
        {
            return new CaseMoneyExpenseEditVM()
            {
                Id = model.Id,
                CourtId = model.CourtId,
                CaseId = model.CaseId,
                CaseMoneyExpenseTypeId = model.CaseMoneyExpenseTypeId,
                CurrencyId = model.CurrencyId,
                Amount = model.Amount,
                Description = model.Description,
                JointDistribution = model.JointDistribution ?? true,
                CasePersonListDecimals = FillPersonListExpense(model.CaseId, model.Id)
            };
        }

        public CaseMoneyExpenseEditVM GetById_ExpenseEditVM(int Id)
        {
            return FillCaseMoneyExpenseEditVM(repo.GetById<CaseMoneyExpense>(Id));
        }

        private List<CaseMoneyExpensePerson> GetMoneyExpensePeople(int caseMoneyExpenseId)
        {
            return repo.AllReadonly<CaseMoneyExpensePerson>()
                       .Include(x => x.CasePerson)
                       .Where(x => x.CaseMoneyExpenseId == caseMoneyExpenseId)
                       .ToList() ?? new List<CaseMoneyExpensePerson>();
        }

        public List<CasePersonListDecimalVM> FillPersonListExpense(int caseId, int? caseMoneyExpenseId)
        {
            List<CasePersonListDecimalVM> results = new List<CasePersonListDecimalVM>();

            var casePeople = GetCasePerson(caseId);
            var moneyExpensePeople = GetMoneyExpensePeople(caseMoneyExpenseId ?? 0);

            foreach (var person in casePeople)
            {
                var moneyExpensePerson = moneyExpensePeople.Where(x => x.CasePersonId == person.Id).FirstOrDefault();
                if (moneyExpensePerson == null)
                {
                    CasePersonListDecimalVM casePersonList = new CasePersonListDecimalVM()
                    {
                        Id = person.Id,
                        Label = person.FullName,
                        ValueOne = 0,
                        ValueTwo = 0,
                        LabelOne = "Сума",
                        LabelTwo = "Уважено"
                    };

                    results.Add(casePersonList);
                }
                else
                {
                    CasePersonListDecimalVM casePersonList = new CasePersonListDecimalVM()
                    {
                        Id = moneyExpensePerson.CasePersonId,
                        Label = moneyExpensePerson.CasePerson.FullName,
                        ValueOne = moneyExpensePerson.PersonAmount,
                        LabelOne = "Сума",
                        LabelTwo = "Уважено"
                    };

                    results.Add(casePersonList);
                }
            }

            return results;
        }

        private CaseMoneyExpensePerson FillCaseMoneyExpensePerson(CaseMoneyExpense moneyExpense, CasePersonListDecimalVM casePersonListDecimal)
        {
            return new CaseMoneyExpensePerson()
            {
                CaseId = moneyExpense.CaseId,
                CourtId = moneyExpense.CourtId,
                CaseMoneyExpenseId = moneyExpense.Id,
                CasePersonId = casePersonListDecimal.Id,
                PersonAmount = (moneyExpense.JointDistribution ?? true) ? 0 : casePersonListDecimal.ValueOne,
                RespectedAmount = (moneyExpense.JointDistribution ?? true) ? 0 : casePersonListDecimal.ValueTwo,
                Description = string.Empty,
                UserId = userContext.UserId,
                DateWrt = DateTime.Now
            };
        }

        /// <summary>
        /// Запис на Претендиран разноски по заповедни производства
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int CaseMoneyExpense_SaveData(CaseMoneyExpenseEditVM model)
        {
            var modelSave = new CaseMoneyExpense();
            try
            {
                model.ToEntity(modelSave);

                if (modelSave.Id > 0)
                {
                    var saved = repo.GetById<CaseMoneyExpense>(modelSave.Id);
                    saved.CaseMoneyExpenseTypeId = modelSave.CaseMoneyExpenseTypeId;
                    saved.CurrencyId = modelSave.CurrencyId;
                    saved.Amount = modelSave.Amount;
                    saved.Description = modelSave.Description;
                    saved.JointDistribution = modelSave.JointDistribution;
                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;
                    repo.Update(saved);

                    var caseMoneyExpensePeople = GetMoneyExpensePeople(saved.Id);
                    foreach (var casePerson in model.CasePersonListDecimals)
                    {
                        var caseMoneyExpensePerson = caseMoneyExpensePeople.Where(x => x.CasePersonId == casePerson.Id).FirstOrDefault();
                        if (caseMoneyExpensePerson == null)
                        {
                            repo.Add(FillCaseMoneyExpensePerson(saved, casePerson));
                        }
                        else
                        {
                            caseMoneyExpensePerson.PersonAmount = (saved.JointDistribution ?? true) ? 0 : casePerson.ValueOne;
                            caseMoneyExpensePerson.RespectedAmount = (saved.JointDistribution ?? true) ? 0 : casePerson.ValueTwo;
                            repo.Update(caseMoneyExpensePerson);
                        }
                    }
                    
                    repo.SaveChanges();
                    return saved.Id;
                }
                else
                {
                    modelSave.UserId = userContext.UserId;
                    modelSave.DateWrt = DateTime.Now;
                    repo.Add(modelSave);

                    if (!(model.JointDistribution ?? true))
                    {
                        foreach (var casePersonList in model.CasePersonListDecimals)
                        {
                            repo.Add(FillCaseMoneyExpensePerson(modelSave, casePersonList));
                        }
                    }

                    repo.SaveChanges();
                    return modelSave.Id;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CaseMoneyExpense_SaveData id={0};caseid={1}", modelSave.Id, modelSave.CaseId);
                return -1; ;
            }
        }

        /// <summary>
        /// Изтриване на Претендиран разноски по заповедни производства
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public bool CaseMoneyExpense_DeleteData(int Id)
        {
            try
            {
                var caseMoneyExpense = repo.GetById<CaseMoneyExpense>(Id);
                repo.Delete(caseMoneyExpense);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CaseMoneyExpense_DeleteData id={0}", Id);
                return false; ;
            }
        }

        /// <summary>
        /// Изтриване на Вземане към обстоятелство по заповедни производства и лицата
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public bool CaseMoneyCollection_DeleteData(int Id)
        {
            try
            {
                var caseMoneyCollection = repo.GetById<CaseMoneyCollection>(Id);
                var caseMoneyCollections = repo.AllReadonly<CaseMoneyCollection>().Where(x => x.MainCaseMoneyCollectionId == Id).ToList();

                List<CaseMoneyCollectionPerson> caseMoneyCollectionPeople = new List<CaseMoneyCollectionPerson>();

                caseMoneyCollectionPeople.AddRange(repo.AllReadonly<CaseMoneyCollectionPerson>().Where(x => x.CaseMoneyCollectionId == caseMoneyCollection.Id));
                foreach (var moneyCollection in caseMoneyCollections)
                {
                    caseMoneyCollectionPeople.AddRange(repo.AllReadonly<CaseMoneyCollectionPerson>().Where(x => x.CaseMoneyCollectionId == moneyCollection.Id));
                }

                repo.DeleteRange(caseMoneyCollectionPeople);
                repo.DeleteRange(caseMoneyCollections);
                repo.Delete(caseMoneyCollection);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CaseMoneyCollection_DeleteData id={0}", Id);
                return false; ;
            }
        }

        /// <summary>
        /// Изтриване на Обстоятелства по заповедни производства
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public bool CaseMoneyClaim_DeleteData(int Id)
        {
            try
            {
                var caseMoneyClaim = repo.GetById<CaseMoneyClaim>(Id);
                var moneyCollections = repo.AllReadonly<CaseMoneyCollection>().Where(x => x.CaseMoneyClaimId == Id && x.MainCaseMoneyCollectionId == null).ToList();

                foreach (var caseMoneyCollection in moneyCollections)
                {
                    var caseMoneyCollections = repo.AllReadonly<CaseMoneyCollection>().Where(x => x.MainCaseMoneyCollectionId == caseMoneyCollection.Id).ToList();

                    List<CaseMoneyCollectionPerson> caseMoneyCollectionPeople = new List<CaseMoneyCollectionPerson>();

                    caseMoneyCollectionPeople.AddRange(repo.AllReadonly<CaseMoneyCollectionPerson>().Where(x => x.CaseMoneyCollectionId == caseMoneyCollection.Id));
                    foreach (var moneyCollection in caseMoneyCollections)
                    {
                        caseMoneyCollectionPeople.AddRange(repo.AllReadonly<CaseMoneyCollectionPerson>().Where(x => x.CaseMoneyCollectionId == moneyCollection.Id));
                    }

                    repo.DeleteRange(caseMoneyCollectionPeople);
                    repo.DeleteRange(caseMoneyCollections);
                }

                repo.DeleteRange(moneyCollections);
                repo.Delete(caseMoneyClaim);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CaseMoneyClaim_DeleteData id={0}", Id);
                return false; ;
            }
        }

        /// <summary>
        /// Запис на Начин на плащане/изпълнение, Заповедни производства
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int CaseBankAccount_SaveData(CaseBankAccount model)
        {
            try
            {
                if (model.Id > 0)
                {
                    var saved = repo.GetById<CaseBankAccount>(model.Id);
                    saved.CaseBankAccountTypeId = model.CaseBankAccountTypeId;
                    saved.IBAN = model.IBAN;
                    saved.BIC = model.BIC;
                    saved.BankName = model.BankName;
                    saved.Description = model.Description;
                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;
                    saved.CasePersonId = model.CasePersonId;
                    repo.Update(saved);
                    repo.SaveChanges();
                    return saved.Id;
                }
                else
                {
                    model.UserId = userContext.UserId;
                    model.DateWrt = DateTime.Now;
                    repo.Add(model);
                    repo.SaveChanges();
                    return model.Id;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CaseBankAccount_SaveData id={0};caseid={1}", model.Id, model.CaseId);
                return -1; ;
            }
        }

        /// <summary>
        /// Изтриване на Начин на плащане/изпълнение, Заповедни производства
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public bool CaseBankAccount_DeleteData(int Id)
        {
            try
            {
                var caseBankAccount = repo.GetById<CaseBankAccount>(Id);
                repo.Delete(caseBankAccount);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CaseBankAccount_DeleteData id={0}", Id);
                return false; ;
            }
        }

        private string ReplaceNewLineBREnd(string text, string stringForReplace, string stringReplace)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return text.Replace(stringForReplace, stringReplace);
        }

        private string ReplaceNewLine(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            
            return Regex.Replace(text, @"\t|\n|\r", "<br />");
        }

        public CaseFastProcessEditVM GetByCaseId_CaseFastProcess(int CaseId)
        {
            return repo.AllReadonly<CaseFastProcess>()
                       .Where(x => x.CaseId == CaseId)
                       .Select(x => new CaseFastProcessEditVM()
                       {
                           Id = x.Id,
                           CaseId = x.CaseId,
                           CourtId = x.CourtId ?? 0,
                           DescriptionSave = x.Description,
                           DescriptionEdit = ReplaceNewLineBREnd(x.Description, "<br />", System.Environment.NewLine)
                       })
                       .FirstOrDefault() ?? new CaseFastProcessEditVM();
        }

        public bool CaseFastProcess_SaveData(CaseFastProcessEditVM model)
        {
            try
            {
                if (model.Id > 0)
                {
                    var saved = repo.GetById<CaseFastProcess>(model.Id);
                    saved.Description = ReplaceNewLine(model.DescriptionEdit);
                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    var caseFastProcess = new CaseFastProcess()
                    {
                        CaseId = model.CaseId,
                        CourtId = model.CourtId,
                        Description = ReplaceNewLineBREnd(model.DescriptionEdit, System.Environment.NewLine, "<br />"),
                        TaxAmount = 0,
                        CurrencyId = NomenclatureConstants.Currency.BGN,
                        UserId = userContext.UserId,
                        DateWrt = DateTime.Now,
                    };

                    repo.Add(caseFastProcess);
                    repo.SaveChanges();
                    model.Id = caseFastProcess.Id;
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CaseFastProcess_SaveData id={0};caseid={1}", model.Id, model.CaseId);
                return false; ;
            }
        }

        public CaseBankAccount CaseBankAccount_GetLastByPerson(int CasePersonId, int? CaseBankAccountId)
        {
            var casePerson = repo.GetById<CasePerson>(CasePersonId);

            if (casePerson == null)
                return new CaseBankAccount();

            return repo.AllReadonly<CaseBankAccount>()
                       .Where(x => x.CasePerson.UicTypeId == casePerson.UicTypeId &&
                                   x.CasePerson.Uic == casePerson.Uic &&
                                   x.CaseBankAccountTypeId == NomenclatureConstants.CaseBankAccountType.BankAccount &&
                                   (CaseBankAccountId != null ? x.Id != CaseBankAccountId : true))
                       .OrderByDescending(x => x.DateWrt)
                       .FirstOrDefault() ?? new CaseBankAccount();
        }
    }
}
