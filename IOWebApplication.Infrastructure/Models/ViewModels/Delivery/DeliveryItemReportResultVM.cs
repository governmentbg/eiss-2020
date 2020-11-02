// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Delivery;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryItemReportResultVM
    {
        [Display(Name = "Вид на делото")]
        public string CaseGroupLabel { get; set; }
        
        [Display(Name = "№ и година на делото")]
        public string CaseInfo { get; set; }

        [Display(Name = "Точен вид дело")]
        public string CaseTypeLabel { get; set; }

        [Display(Name = "Съд(текущия съд или съд, от който са получени призовките за връчване от текущия)")]
        public string FromCourtName { get; set; }

        [Display(Name = "Дата на изготвяне на книжата за връчване / Дата на постъпване на документите в текущия съд")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "Дата на изготвяне на книжата за връчване / Дата на постъпване на документите в текущия съд")]
        public string DateFromStr { get; set; }

        [Display(Name = "Съдебен призовкар, определен да връчи книжата")]
        public string LawUnitName { get; set; }

        [Display(Name = "Вид на документа")]
        public string DocumentType { get; set; }
        
        [Display(Name = "Точен вид на документа")]
        public string HtmlTemplateName { get; set; }

        [Display(Name = "Статус на документа")]
        public string StateName { get; set; }

        [Display(Name = "Дата на връчване/връщане в цялост")]
        public DateTime? DateResult { get; set; }

        [Display(Name = "Дата на връчване/връщане в цялост")]
        public string DateResultStr { get; set; }

        [Display(Name = "Причина за невръчване на документа")]
        public string ReasonReturn { get; set; }

        [Display(Name = "Страна/Участник")]
        public string PersonName { get; set; }

        [Display(Name = "Адрес за връчване")]
        public string Address { get; set; }

        public int LawUnitId { get; set; }
        public int CaseGroupId { get; set; }
        public int CaseTypeId { get; set; }
        public int FromCourtId { get; set; }
    }
}
