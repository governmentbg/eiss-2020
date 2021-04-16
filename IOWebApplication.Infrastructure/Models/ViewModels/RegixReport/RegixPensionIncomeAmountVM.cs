using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.RegixReport
{
    public enum IdentifierTypePensionVM
    {

        /// <remarks/>
        [Display(Name = "ЕГН")]
        ЕГН,

        /// <remarks/>
        [Display(Name = "ЛНЧ")]
        ЛНЧ,
    }

    public class RegixPensionIncomeAmountVM
    {
        public RegixReportVM Report { get; set; }

        public RegixPensionIncomeAmountFilterVM PensionIncomeAmountFilter { get; set; }

        public RegixPensionIncomeAmountResponseVM PensionIncomeAmountResponse { get; set; }

        public RegixPensionIncomeAmountVM()
        {
            Report = new RegixReportVM();
            PensionIncomeAmountFilter = new RegixPensionIncomeAmountFilterVM();
            PensionIncomeAmountResponse = new RegixPensionIncomeAmountResponseVM();
        }

    }

    public class RegixPensionIncomeAmountFilterVM
    {
        [Display(Name = "ЕГН/ЛНЧ")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string IdentifierFilter { get; set; }

        [Display(Name = "Тип на идентификатор")]
        [Range(0, int.MaxValue, ErrorMessage = "Изберете")]
        public int IdentifierTypeFilter { get; set; }

        [Display(Name = "От дата")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public DateTime DateFromFilter { get; set; }

        [Display(Name = "До дата")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public DateTime DateToFilter { get; set; }
    }

    public class RegixPensionIncomeAmountResponseVM
    {
        [Display(Name = "ТП на НОИ, където лицето получава пенсия:")]
        public string TerritorialDivisionNOI { get; set; }

        [Display(Name = "ЕГН:")]
        public string Identifier { get; set; }

        [Display(Name = "Трите имена на лицето:")]
        public string Names { get; set; }

        [Display(Name = "Статус на пенсионер:")]
        public string PensionerStatus { get; set; }

        [Display(Name = "Дата на смърт:")]
        public string DateOfDeath { get; set; }

        [Display(Name = "Текст на уверение:")]
        public string ContentText { get; set; }

        public List<RegixPensionIncomeAmountPaymentVM> Payments { get; set; }

        public RegixPensionIncomeAmountResponseVM()
        {
            Payments = new List<RegixPensionIncomeAmountPaymentVM>();
        }
    }

    public class RegixPensionIncomeAmountPaymentVM
    {
        [Display(Name = "Месец:")]
        public string Month { get; set; }

        [Display(Name = "Общо получена сума:")]
        public string TotalAmount { get; set; }

        [Display(Name = "Пенсии:")]
        public string PensionAmount { get; set; }

        [Display(Name = "Добавка за чужда помощ:")]
        public string AdditionForAssistance { get; set; }

        [Display(Name = "Други добавки:")]
        public string OtherAddition { get; set; }
    }

}
