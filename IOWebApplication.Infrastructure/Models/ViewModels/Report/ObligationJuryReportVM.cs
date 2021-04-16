using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class ObligationJuryReportVM
    {
        public int Id { get; set; }
        public int? CaseId { get; set; }
        public bool? ExistCase { get; set; }

        [Display(Name = "Точен вид")]
        public string CaseTypeName { get; set; }

        [Display(Name = "Номер дело")]
        public string CaseNumber { get; set; }

        [Display(Name = "Вид заседание")]
        public string SessionTypeName { get; set; }

        [Display(Name = "Дата")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? SessionDate { get; set; }

        [Display(Name = "Лице")]
        public string PersonName { get; set; }
        public string Uic { get; set; }

        [Display(Name = "Сметка")]
        public string BankAccountName { get; set; }

        [Display(Name = "Дата на възнаграждението")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ObligationDate { get; set; }

        [Display(Name = "Сума")]
        public decimal Amount { get; set; }

        [Display(Name = "В т.ч. пътни")]
        public decimal AmountTransport { get; set; }

        public decimal AmountPayment { get; set; }

        [Display(Name = "Статус")]
        public string Status { get { return (this.Amount + this.AmountTransport > this.AmountPayment) ? "Неплатено" : "Платено"; } }

        [Display(Name = "Забележка")]
        public string Description { get; set; }

        [Display(Name = "Вид сметка")]
        public string MoneyGroupName { get; set; }

        [Display(Name = "Изготвени РКО")]
        public string ExpenseOrderDates { get; set; }

        [Display(Name = "Съдебен секретар РКО")]
        public string ExpenseOrderUsers { get; set; }

        [Display(Name = "Съдия РКО")]
        public string ExpenseOrderJudge { get; set; }

        [Display(Name = "Продължителност на СЗ")]
        public string SessionTime { get; set; }

        [Display(Name = "Резултат от СЗ")]
        public string SessionResult { get; set; }
    }

    public class ObligationJuryFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "Вид заседание")]
        public int SessionTypeId { get; set; }

        [Display(Name = "Вид сметка")]
        public int MoneyGroupId { get; set; }

        [Display(Name = "Име на лице")]
        public string PersonName { get; set; }

        [Display(Name = "Вид лице")]
        public string PersonType { get; set; }

        [Display(Name = "Вид лице")]
        public string PersonTypeLabel { get; set; }
    }

}
