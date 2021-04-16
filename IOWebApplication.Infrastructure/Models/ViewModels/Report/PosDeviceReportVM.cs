using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class PosDeviceReportVM
    {
        [Display(Name = "№")]
        public int Number { get; set; }

        [Display(Name = "Наименование на бюджетната организация/структура/звено")]
        public string CourtName { get; set; }

        [Display(Name = "Брой устройства ПОС")]
        public int PosDeviceCount { get; set; }

        [Display(Name = "Брой на ПОС - транзакции")]
        public int PaymentCount { get; set; }

        [Display(Name = "Обща стойност на ПОС - транзакциите")]
        public decimal PaymentSum { get; set; }

        [Display(Name = "BIC код и наименование на банката, обслужваща ПОС-устройство на бюджетната организация")]
        public string BankData { get; set; }
    }

    public class PosDeviceFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "ПОС устройство")]
        public string PosDeviceTid { get; set; }
    }
}
