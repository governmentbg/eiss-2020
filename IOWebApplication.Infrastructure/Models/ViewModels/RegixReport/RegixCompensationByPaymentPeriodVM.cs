// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.RegixReport
{
    public enum IdentifierTypeCompensationByPaymentPeriodVM
    {

        /// <remarks/>
        [Display(Name = "ЕГН")]
        ЕГН,

        /// <remarks/>
        [Display(Name = "ЛНЧ")]
        ЛНЧ,
    }

    public class RegixCompensationByPaymentPeriodVM
    {
        public int CompensationTypeId { get; set; }

        public string CompensationTypeName { get; set; }

        public RegixReportVM Report { get; set; }


        public RegixCompensationByPaymentPeriodFilterVM CompensationByPaymentPeriodFilter { get; set; }

        public RegixCompensationByPaymentPerioResponseVM CompensationByPaymentPerioResponse { get; set; }

        public RegixCompensationByPaymentPeriodVM()
        {
            Report = new RegixReportVM();
            CompensationByPaymentPeriodFilter = new RegixCompensationByPaymentPeriodFilterVM();
            CompensationByPaymentPerioResponse = new RegixCompensationByPaymentPerioResponseVM();
        }
    }

    public class RegixCompensationByPaymentPeriodFilterVM
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

    public class RegixCompensationByPaymentPerioResponseVM
    {
        [Display(Name = "ЕГН/ЛНЧ:")]
        public string Identifier { get; set; }

        [Display(Name = "Тип на идентификатор:")]
        public string IdentifierType { get; set; }

        [Display(Name = "Имена:")]
        public string Names { get; set; }

        public List<RegixCompensationByPaymentPerioPaymenDataVM> Payments { get; set; }

        public RegixCompensationByPaymentPerioResponseVM()
        {
            Payments = new List<RegixCompensationByPaymentPerioPaymenDataVM>();
        }
    }

    public class RegixCompensationByPaymentPerioPaymenDataVM
    {
        [Display(Name = "Вид на обезщетението:")]
        public string BenefitType { get; set; }

        [Display(Name = "Месец от периода на обезщетението:")]
        public string BenefitMonth { get; set; }

        [Display(Name = "Година от период на обезщетението:")]
        public string BenefitYear { get; set; }

        [Display(Name = "Изплатена сума:")]
        public string BenefitAmount { get; set; }

        [Display(Name = "Дата:")]
        public string BenefitDatePayment { get; set; }
    }
}
