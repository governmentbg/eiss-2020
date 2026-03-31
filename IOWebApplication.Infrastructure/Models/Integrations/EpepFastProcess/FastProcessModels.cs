using iText.Kernel.Utils.Objectpathitems;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess
{

    public class BaseRequestVM : IBaseRequestVM
    {
        public string RequestFullTitle { get; set; }
        public string RequestTitle { get; set; }
        public string RequestTypeCode { get; set; }
        public long DocumentId { get; set; }
        public int CaseId { get; set; }
        public decimal TaxAmount { get; set; }
        public BaseRequestPersonInfoVM[] LeftSide { get; set; }
        public BaseRequestPersonInfoVM[] RightSide { get; set; }
        public bool HasRepresentatives { get; set; }

        public virtual void RecreateObject() { }

        public void SanitizeObject()
        {
            switch (this.RequestTypeCode)
            {
                case FastProcessRequestVM.FastProcess410:
                case FastProcessRequestVM.FastProcess417:
                    var model = (FastProcessRequestVM)this;
                    if (!model.ClaimTypes.MoneyClaim)
                    {
                        model.MoneyClaims = new FastProcessMoneyClaimVM[] { };
                    }
                    if (!model.ClaimTypes.ItemSubstitutionClaim)
                    {
                        model.ItemSubstitutionClaims = new FastProcessItemSubstitutionClaimVM[] { };
                    }
                    break;
            }
        }

        /// <summary>
        /// Метод валидиращ в разпределената отговорност на няма сума по-голяма от 100
        /// </summary>
        /// <param name="debtDistributions">Списък с данни за разпределена отговорност</param>
        /// <returns></returns>
        private List<ValidationErrorModel> ValidateDebtDistributions(FastProcessDebtDistributionVM[] debtDistributions)
        {
            var validationErrors = new List<ValidationErrorModel>();
            var debtsProcent = debtDistributions.GroupBy(x => new { x.ClaimCode })
                                         .Select(g => new
                                         {
                                             g.Key.ClaimCode,
                                             ShareProcentSum = g.Sum(d => d.ShareProcent)
                                         })
                                         .ToList();

            if (debtsProcent.Where(x => x.ShareProcentSum > 100).Any())
            {
                validationErrors.Add(new ValidationErrorModel()
                {
                    Error = "Сумата от дяловете е по-голяма от 100",
                    Control = nameof(FastProcessRequestVM.DebtDistributionValidations)
                });
            }


            var debtsDublicates = debtDistributions.GroupBy(x => new { x.PersonCode, x.ClaimCode })
                             .Select(g => new
                             {
                                 g.Key.PersonCode,
                                 g.Key.ClaimCode,
                                 PersonClaimCount = g.Count()
                             })
                             .ToList();

            if (debtsDublicates.Where(x => x.PersonClaimCount > 1).Any())
            {
                validationErrors.Add(new ValidationErrorModel()
                {
                    Error = "Едно лице не може да фигурира повече от един път по дадено задължение",
                    Control = nameof(FastProcessRequestVM.DebtDistributionValidations)
                });
            }

            return validationErrors;
        }

        /// <summary>
        /// Валидация на разпределена отговорност
        /// </summary>
        /// <param name="debtDistributions">Списък с данни за разпределена отговорност</param>
        /// <param name="isInEuro">Флаг дали се работи с евро</param>
        /// <returns></returns>
        private List<ValidationErrorModel> ValidateDebtDistributions(FastProcessDebtDistributionVM[] debtDistributions, bool isInEuro)
        {
            List<ValidationErrorModel> errorModels = [];

            for (int i = 0; i < debtDistributions.Length; i++)
            {
                FastProcessDebtDistributionVM item = debtDistributions[i];
                if (item.PersonCode == FastProcessRequestVM.NullVal || string.IsNullOrEmpty(item.PersonCode))
                    errorModels.Add(new ValidationErrorModel("Изберете длъжник", $"DebtDistributions[{i}].PersonCode"));

                if (item.ClaimCode == FastProcessRequestVM.NullVal)
                    errorModels.Add(new ValidationErrorModel("Изберете вземане", $"DebtDistributions[{i}].ClaimCode"));

                if (item.ShareProcent < 1)
                    errorModels.Add(new ValidationErrorModel("Изберете дял по-голям или равен 1", $"DebtDistributions[{i}].ShareProcent"));

                if (item.ShareProcent > 100)
                    errorModels.Add(new ValidationErrorModel("Изберете дял по-малък или равен на 100", $"DebtDistributions[{i}].ShareProcent"));

                if (item.CurrencyCode == FastProcessRequestVM.NullVal)
                    errorModels.Add(new ValidationErrorModel("Изберете валута", $"DebtDistributions[{i}].CurrencyCode"));

                if (item.Amount == 0M)
                    errorModels.Add(new ValidationErrorModel("Въведете Размер", $"DebtDistributions[{i}].Amount"));

                if (isInEuro)
                {
                    if (item.TotalAmountEUR == 0M)
                        errorModels.Add(new ValidationErrorModel("Въведете Сума", $"DebtDistributions[{i}].TotalAmountEUR"));
                }
                else
                {
                    if (item.TotalAmountBGN == 0M)
                        errorModels.Add(new ValidationErrorModel("Въведете сума", $"DebtDistributions[{i}].TotalAmountBGN"));
                }
            }

            errorModels.AddRange(ValidateDebtDistributions(debtDistributions));

            return errorModels;
        }

        public List<ValidationErrorModel> ValidateRequest(bool isInEuro)
        {
            SanitizeObject();

            var result = new List<ValidationErrorModel>();
            switch (this.RequestTypeCode)
            {
                case FastProcessRequestVM.FastProcess410:
                case FastProcessRequestVM.FastProcess417:
                    var model = (FastProcessRequestVM)this;
                    if (!model.ClaimTypes.MoneyClaim && !model.ClaimTypes.ItemClaim && !model.ClaimTypes.ItemSubstitutionClaim && !model.ClaimTypes.PropertyClaim)
                    {
                        result.Add(new ValidationErrorModel("Изберете вид вземане", $"ClaimTypes.ClaimTypeValidations"));
                    }
                    if (model.MoneyClaims != null && model.ClaimTypes.MoneyClaim)
                        for (int i = 0; i < model.MoneyClaims.Length; i++)
                        {
                            FastProcessMoneyClaimVM item = model.MoneyClaims[i];
                            if (item.MoneyClaimTypeCode == FastProcessRequestVM.NullVal)
                            {
                                result.Add(new ValidationErrorModel("Изберете вид вземане", $"MoneyClaims[{i}].MoneyClaimTypeCode"));
                            }

                            if (item.CurrencyCode == FastProcessRequestVM.NullVal)
                            {
                                result.Add(new ValidationErrorModel("Изберете валута", $"MoneyClaims[{i}].CurrencyCode"));
                            }
                            if (item.Amount == 0M)
                            {
                                result.Add(new ValidationErrorModel("Въведете Размер", $"MoneyClaims[{i}].Amount"));
                            }
                            if (item.HasStatutoryinterest == true && item.StatutoryinterestDate == null)
                            {
                                result.Add(new ValidationErrorModel("Въведете Дата", $"MoneyClaims[{i}].StatutoryinterestDate"));
                            }
                            if (isInEuro)
                            {
                                if (item.TotalAmountEUR == 0M)
                                {
                                    result.Add(new ValidationErrorModel("Въведете Сума", $"MoneyClaims[{i}].TotalAmountEUR"));
                                }
                            }
                            else
                            {
                                if (item.TotalAmountBGN == 0M)
                                {
                                    result.Add(new ValidationErrorModel("Въведете сума", $"MoneyClaims[{i}].TotalAmountBGN"));
                                }
                            }
                            if (item.DateTo.HasValue)
                                if (FastProcessRequestVM.MoneyClaimTypesNoFutureDates.Contains(item.MoneyClaimTypeCode) && item.DateTo.Value > DateTime.Now)
                                {
                                    result.Add(new ValidationErrorModel("Дата до не може да бъде бъдеща", $"MoneyClaims[{i}].DateTo"));
                                }
                        }
                    if (model.ItemSubstitutionClaims != null && model.ClaimTypes.ItemSubstitutionClaim)
                        for (int i = 0; i < model.ItemSubstitutionClaims.Length; i++)
                        {
                            FastProcessItemSubstitutionClaimVM item = model.ItemSubstitutionClaims[i];
                            if (string.IsNullOrEmpty(item.TypeName))
                            {
                                result.Add(new ValidationErrorModel("Въведете Описание", $"ItemSubstitutionClaims[{i}].TypeName"));
                            }
                            if (isInEuro)
                            {
                                if (item.TotalAmountEUR == 0M)
                                {
                                    result.Add(new ValidationErrorModel("Въведете Сума", $"ItemSubstitutionClaims[{i}].TotalAmountEUR"));
                                }
                            }
                            else
                            {
                                if (item.TotalAmountBGN == 0M)
                                {
                                    result.Add(new ValidationErrorModel("Въведете сума", $"ItemSubstitutionClaims[{i}].TotalAmountBGN"));
                                }
                            }
                        }
                    if (model.ClaimCircumstances != null)
                        if (model.ClaimCircumstances.ClaimCircumstancesCode == FastProcessRequestVM.NullVal)
                        {
                            result.Add(new ValidationErrorModel("Изберете Обстоятелство", $"ClaimCircumstances.ClaimCircumstancesCode"));
                        }
                    if (model.DebtDistributions != null)
                        result.AddRange(ValidateDebtDistributions(model.DebtDistributions, isInEuro));
                    if (model.Expenses != null)
                        for (int i = 0; i < model.Expenses.Length; i++)
                        {
                            FastProcessExpenseVM item = model.Expenses[i];
                            if (item.ExpenseTypeCode == FastProcessRequestVM.NullVal)
                            {
                                result.Add(new ValidationErrorModel("Изберете вид разноска", $"Expenses[{i}].ExpenseTypeCode"));
                            }
                            if (isInEuro)
                            {
                                if (item.TotalAmountEUR == 0M)
                                {
                                    result.Add(new ValidationErrorModel("Въведете Сума", $"Expenses[{i}].TotalAmountEUR"));
                                }
                            }
                            else
                            {
                                if (item.TotalAmountBGN == 0M)
                                {
                                    result.Add(new ValidationErrorModel("Въведете сума", $"Expenses[{i}].TotalAmountBGN"));
                                }
                            }
                        }

                    if (this.RequestTypeCode == FastProcessRequestVM.FastProcess417)
                    {
                        if (FastProcessClaimCircumstancesVM.Req417a1MustBeChecked.Contains(model.ClaimCircumstances.ClaimCircumstancesCode)
                            && !model.CompetencyBase417.ForCompetencyBase)
                        {
                            result.Add(new ValidationErrorModel(" Избрали сте основание, което налага да бъде маркирано 'Заявление по чл.417, т.3, 4 или 10 от ГПК'", $"CompetencyBase417.ForCompetencyBase"));

                        }
                    }
                    break;
                default:
                    break;
            }
            return result;
        }
    }

    public class BaseBlankRequestVM
    {
        public List<NomenclatureItemVM> Nomenclatures { get; set; }
        public IBaseRequestVM Data { get; set; }

        public FileListVM[] Files { get; set; }

        public string getNomenclature(string alias, string value)
        {
            return Nomenclatures.Where(n => n.Alias == alias && n.Value == value).Select(n => n.Label).FirstOrDefault();
        }
        public string formatMoney(decimal value)
        {
            return value.ToString("N2").Replace(",", ".");
        }
        public string formatDate(DateTime? value)
        {
            if (value == null)
            {
                return "";
            }
            return value.Value.ToString("dd.MM.yyyy г.");
        }
        public string formatIntBool(int? value)
        {
            switch (value)
            {
                case 1: return "ДА";
                case 2: return "НЕ";
            }
            return string.Empty;
        }

        public bool IsInEuro { get; set; }
    }


    public class FastProcessRequestVM : BaseRequestVM
    {
        public const string NullVal = "-1";
        public const string FastProcess410 = "R04410";
        public const string FastProcess417 = "R04417";
        public const string FastProcessCompl410 = "R22410";
        public const string FastProcessCompl417 = "R22417";
        public static string[] InitRequests = { FastProcess410, FastProcess417 };
        public static string[] ComplainRequests = { FastProcessCompl410, FastProcessCompl417 };

        public const string MoneyClaimType_Base = "21";
        public static string[] MoneyClaimTypesNoFutureDates = { "22", "25", "26", "27" };


        public const int IntRadio_Yes = 1;
        public const int IntRadio_No = 2;
        public const string ObligationType_SubstItem = "2|1";
        public const string ObligationType_Item = "3|1";

        public int ErrorCount { get; set; }

        [Display(Name = "Потвърдете записа, въпреки валидационните грешки!")]
        public bool SubmitOnErrors { get; set; }

        public FastProcessCompetencyBase417VM CompetencyBase417 { get; set; }

        [Display(Name = "Описание")]
        public string ClaimDescription { get; set; }

        public FastProcessClaimTypeVM ClaimTypes { get; set; }

        public FastProcessMoneyClaimVM[] MoneyClaims { get; set; }

        public FastProcessItemSubstitutionClaimVM[] ItemSubstitutionClaims { get; set; }

        public FastProcessItemClaimVM ItemClaim { get; set; }

        /// <summary>
        /// 3.2. Обстоятелства, от които произтича вземането
        /// </summary>
        public FastProcessClaimCircumstancesVM ClaimCircumstances { get; set; }

        /// <summary>
        /// 0 - Не е избрано
        /// 1 - Да
        /// 2 - Не
        /// </summary>
        [Display(Name = "Вземането е цедирано на заявителя")]
        public int? CededClaim { get; set; }

        [Display(Name = "Уточнения относно цедирано вземане")]
        public string CededClaimDescription { get; set; }

        /// <summary>
        /// 0 - Не е избрано
        /// 1 - Да
        /// 2 - Не
        /// </summary>
        [Display(Name = "3.3 Разпределение на отговорността при повече от един длъжник")]
        public int? JoinedDestributionTypeId { get; set; }
        public string DebtDistributionValidations { get; set; }
        public FastProcessDebtDistributionVM[] DebtDistributions { get; set; }

        public FastProcessBankAccountVM BankAccount { get; set; }


        [Display(Name = "5. Допълнителни изявления и допълнителна информация (при необходимост)")]
        public string OtherInfo { get; set; }

        public FastProcessExpenseVM[] Expenses { get; set; }

        public string SideValidations { get; set; }

        public override void RecreateObject()
        {
            CompetencyBase417 = CompetencyBase417 ?? new FastProcessCompetencyBase417VM();
            ClaimTypes = ClaimTypes ?? new FastProcessClaimTypeVM();
            MoneyClaims = MoneyClaims ?? new FastProcessMoneyClaimVM[] { };
            for (int i = 0; i < MoneyClaims.Length; i++)
            {
                MoneyClaims[i].Index = i;
            }
            ItemSubstitutionClaims = ItemSubstitutionClaims ?? new FastProcessItemSubstitutionClaimVM[] { };
            for (int i = 0; i < ItemSubstitutionClaims.Length; i++)
            {
                ItemSubstitutionClaims[i].Index = i;
            }
            ItemClaim = ItemClaim ?? new FastProcessItemClaimVM();
            ClaimCircumstances = ClaimCircumstances ?? new FastProcessClaimCircumstancesVM();
            DebtDistributions = DebtDistributions ?? new FastProcessDebtDistributionVM[] { };
            for (int i = 0; i < DebtDistributions.Length; i++)
            {
                DebtDistributions[i].Index = i;
            }
            BankAccount = BankAccount ?? new FastProcessBankAccountVM();
            Expenses = Expenses ?? new FastProcessExpenseVM[] { };
            for (int i = 0; i < Expenses.Length; i++)
            {
                Expenses[i].Index = i;
            }
        }

    }




    public class DocumentPersonInfoVM
    {
        public string PersonGid { get; set; }

        public string Identifier { get; set; }

        public string FullName { get; set; }

        public string RoleName { get; set; }

        public long SideInvolvementKindId { get; set; }

        public string RepresentsPersonGid { get; set; }

    }

    public class BaseRequestPersonReadInfoVM : BaseRequestPersonInfoVM
    {
        public new int CasePersonId { get; set; }
        public string RepresentsPersonGid { get; set; }
        public int RoleKind { get; set; }
    }

    /// <summary>
    /// Информация за едно лице със съответните му представители
    /// </summary>
    public class BaseRequestPersonInfoVM
    {
        [NotMapped]
        public int CasePersonId { get; set; }

        /// <summary>
        /// Вътрешен идентификатор на лице
        /// </summary>
        public string PersonGid { get; set; }

        [Display(Name = "Идентификатор")]
        public string Identifier { get; set; }

        [Display(Name = "Имена")]
        public string FullName { get; set; }

        [Display(Name = "Качество")]
        public string RoleName { get; set; }

        public BaseRequestPersonInfoVM[] Representatives { get; set; }

        public BaseRequestPersonAddressInfoVM[] Addresses { get; set; }

    }

    public class BaseRequestPersonAddressInfoVM
    {
        [Display(Name = "Адрес")]
        public string FullAddress { get; set; }
        [Display(Name = "Тип")]
        public string AddressTypeName { get; set; }
    }

    public class FastProcessClaimTypeVM
    {
        [Display(Name = "Парично вземане")]
        public bool MoneyClaim { get; set; }

        [Display(Name = "Вземане за предаване на заместими вещи")]
        public bool ItemSubstitutionClaim { get; set; }

        [Display(Name = "Вземане за предаване на движима вещ")]
        public bool ItemClaim { get; set; }

        /// <summary>
        /// Само за 417!
        /// </summary>
        [Display(Name = "Вземане за предаване на недвижим имот")]
        public bool PropertyClaim { get; set; }

        public string ClaimTypeValidations { get; set; }
    }


    public class ValidationErrorModel
    {
        public string Control { get; set; }
        public string Error { get; set; }

        public ValidationErrorModel(string error, string control = "")
        {
            Error = error;
            Control = control;
        }
        public ValidationErrorModel()
        {

        }
    }


    /// <summary>
    /// 410 3.1 Парично вземане
    /// </summary>
    public class FastProcessMoneyClaimVM
    {
        [JsonIgnore]
        public int Index { get; set; }
        [JsonIgnore]
        public string GetPath
        {
            get
            {
                return $"MoneyClaims[{Index}]";
            }
        }
        public string Gid { get; set; }

        /// <summary>
        /// Вид: главница/ лихва/ такса/ неустойка/ друго
        /// </summary>
        [Display(Name = "Вид")]
        public string MoneyClaimTypeCode { get; set; }

        [Display(Name = "Размер")]
        public decimal Amount { get; set; }

        [Display(Name = "Валута")]
        public string CurrencyCode { get; set; }

        [Display(Name = "Сума в лева")]
        public decimal TotalAmountBGN { get; set; }

        [Display(Name = "Сума в евро")]
        public decimal TotalAmountEUR { get; set; }

        [Display(Name = "Дата от")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "Дата до")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Обяснителна част")]
        public string Description { get; set; }

        [Display(Name = "Законна лихва")]
        public bool HasStatutoryinterest { get; set; }

        [Display(Name = "Дата")]
        public DateTime? StatutoryinterestDate { get; set; }
    }

    /// <summary>
    /// 410 3.1 Вземане за предаване на заместими вещи
    /// </summary>
    public class FastProcessItemSubstitutionClaimVM
    {
        [JsonIgnore]
        public int Index { get; set; }
        [JsonIgnore]
        public string GetPath
        {
            get
            {
                return $"{nameof(FastProcessRequestVM.ItemSubstitutionClaims)}[{Index}]";
            }
        }
        public string Gid { get; set; }

        /// <summary>
        /// Описание на заместимите вещи (вид, качество)
        /// </summary>
        [Display(Name = "Вид/Качество")]
        public string TypeName { get; set; }

        [Display(Name = "Количество")]
        public string QuantityText { get; set; }

        [Display(Name = "Сума в лева")]
        public decimal TotalAmountBGN { get; set; }

        [Display(Name = "Сума в евро")]
        public decimal TotalAmountEUR { get; set; }
    }

    /// <summary>
    /// 3.1 Вземане за предаване на движима вещ
    /// </summary>
    public class FastProcessItemClaimVM
    {
        [Display(Name = "Описание на вещта")]
        public string Description { get; set; }

        [Display(Name = "Получена от длъжника със задължение за връщане")]
        public bool ClaimedByDebtorWithReturn { get; set; }

        [Display(Name = "Движима вещ, която е обременена със залог")]
        public bool ItemWithPledge { get; set; }

        [Display(Name = "Движима вещ, която е прехвърлена от длъжника със задължение да предаде владението")]
        public bool ItemClaimedWithReturn { get; set; }

        /// <summary>
        /// Само за 417
        /// </summary>
        [Display(Name = "Договор")]
        public bool Contract { get; set; }

        [Display(Name = "Сума в лева")]
        public decimal TotalAmountBGN { get; set; }

        [Display(Name = "Сума в евро")]
        public decimal TotalAmountEUR { get; set; }
    }
    /// <summary>
    /// 3.2. Обстоятелства, от които произтича вземането
    /// </summary>
    public class FastProcessClaimCircumstancesVM
    {
        [Display(Name = "Обстоятелство")]
        public string ClaimCircumstancesCode { get; set; }

        [Display(Name = "Номер")]
        public string Number { get; set; }

        [Display(Name = "Дата")]
        public DateTime? Date { get; set; }

        [Display(Name = "Уточнения")]
        public string Description { get; set; }

        /*
         * Нотариален акт, спогодба или друг договор, с нотариална заверка на подписите (чл. 417, ал.1, т.3 от ГПК)
         * Договор за залог или ипотечен акт (чл. 417, ал.1, т.6 от ГПК)
         * Запис на заповед, менителница или друга ценна книга на заповед (чл. 417, ал.1, т.10 от ГПК)
         */
        public static string[] Req417a1MustBeChecked = { "82", "85", "89" };
    }



    /// <summary>
    /// 410 3.3 Разделна отговорност
    /// </summary>
    public class FastProcessDebtDistributionVM
    {
        [JsonIgnore]
        public int Index { get; set; }
        [JsonIgnore]
        public string GetPath
        {
            get
            {
                return $"{nameof(FastProcessRequestVM.DebtDistributions)}[{Index}]";
            }
        }

        /// <summary>
        /// Вътрешен инедтификатор на лице
        /// </summary>
        public string PersonCode { get; set; }

        //[Display(Name = "Длъжник")]
        //public string DebtorName { get; set; }

        [Display(Name = "По вземане пореден номер")]
        public string ClaimCode { get; set; }

        [Display(Name = "Дял, %")]
        public decimal ShareProcent { get; set; }

        [Display(Name = "Размер")]
        public decimal Amount { get; set; }

        [Display(Name = "Валута")]
        public string CurrencyCode { get; set; }

        [Display(Name = "Сума в лева")]
        public decimal TotalAmountBGN { get; set; }

        [Display(Name = "Сума в евро")]
        public decimal TotalAmountEUR { get; set; }
    }

    /// <summary>
    /// 410 4.1 Плащане по банкова сметка
    /// </summary>
    public class FastProcessBankAccountVM
    {
        [Display(Name = "Притежател на сметката")]
        public string OwnerNames { get; set; }

        [Display(Name = "Име на банката (BIC) или друг подходящ банков код")]
        public string BankName { get; set; }

        [Display(Name = "Международен номер на банковата сметка (IBAN)")]
        public string IBAN { get; set; }

        [Display(Name = "Валута на сметката")]
        public string CurrencyCode { get; set; }

        [Display(Name = "4.2 Плащане по друг начин – описание")]
        public string OtherPaymentDescription { get; set; }
    }

    /// <summary>
    /// 410 6 Разноски
    /// </summary>
    public class FastProcessExpenseVM
    {
        [JsonIgnore]
        public int Index { get; set; }
        [JsonIgnore]
        public string GetPath
        {
            get
            {
                return $"Expenses[{Index}]";
            }
        }
        public string Gid { get; set; }

        /// <summary>
        /// Вид: държавна такса/адвокатско възнаграждение/ юрисконсултско възнаграждение/ друго (моля, уточнете)
        /// </summary>
        [Display(Name = "Вид")]
        public string ExpenseTypeCode { get; set; }

        [Display(Name = "Уточнение")]
        public string Description { get; set; }

        [Display(Name = "Сума в лева")]
        public decimal TotalAmountBGN { get; set; }

        [Display(Name = "Сума в евро")]
        public decimal TotalAmountEUR { get; set; }
    }

    /// <summary>
    /// Основание за компетентност 417
    /// </summary>
    public class FastProcessCompetencyBase417VM
    {
        [Display(Name = "Заявление по чл.417, ал.1, т.3, 6 или 10 от ГПК")]
        public bool ForCompetencyBase { get; set; }

        [Display(Name = "Основание за компетентност на съда")]
        public string CompetencyBaseCode { get; set; }
    }

    public class FastProcessConstants
    {
        public class CompetencyBases417
        {
            /// <summary>
            /// Настоящ адрес/седалище на заявителя
            /// </summary>
            public const string CurrentAddressApplicant = "31";

            /// <summary>
            /// Постоянен адрес на заявителя
            /// </summary>
            public const string ResidentAddressApplicant = "32";

            /// <summary>
            /// Настоящ адрес/седалище на длъжника
            /// </summary>
            public const string CurrentAddressDebtor = "33";

            /// <summary>
            /// Постоянен адрес на длъжника
            /// </summary>
            public const string ResidentAddressDebtor = "34";
        }
    }
}
