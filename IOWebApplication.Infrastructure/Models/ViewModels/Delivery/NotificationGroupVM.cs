// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class NotificationGroupVM
    {
        public int CaseSessionId { get; set; }

        public int NotificationListTypeId { get; set; }
        public int CaseId { get; set; }
        [Display(Name = "Вид известие")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете вид известие")]
        public int? NotificationTypeId { get; set; }
        [Display(Name = "Бланка")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете бланка")]
        public int HtmlTemplateId { get; set; }
        [Display(Name = "Вид известяване")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете вид известяване")]
        public int? NotificationDeliveryGroupId { get; set; }
        [Display(Name = "Акт/протокол")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете Акт/протокол")]
        public int? CaseSessionActId { get; set; }
        [Display(Name = "Жалба към Акт/протокол")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете жалба")]
        public int? CaseSessionActComplainId { get; set; }
        [Display(Name = "Жалби към Акт/протокол")]
        public string MultiComplainIdVM { get; set; }
        [Display(Name = "Актове/протоколи")]
        public string MultiComplainIdResultVM { get; set; }
        [Display(Name = "Актове/протоколи")]
        public string MultiActIdVM { get; set; }
        public string MultiActIdResultVM { get; set; }

        [Display(Name = "Дата на връчване")]
        public DateTime? DeliveryDate { get; set; }


        [Display(Name = "Oснованиe за покана")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете Oснованиe за покана")]
        public int? NotificationIspnReasonId { get; set; }


        [Display(Name = "Описание на предмета и задачата на експеризата")]
        public string ExpertReport { get; set; }

        [Display(Name = "Крайна дата за изговяне на експертиза")]
        public DateTime? ExpertDeadDate { get; set; }

        [Display(Name = "Подател нa съпровождащ документ")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете Подател нa съпровождащ документ")]
        public int? DocumentSenderPersonId { get; set; }

        [Display(Name = "Дела на други институции или съдебни инстанции")]
        public string InstitutionDocumentId { get; set; }
        [Display(Name = "Статус")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете статус")]
        public int NotificationStateId { get; set; }

        [Display(Name = "От номер")]
        public int FromRowNumber { get; set; }

        [Display(Name = "До номер")]
        public int ToRowNumber { get; set; }


        public List<NotificationItemVM> NotificationItems { get; set; }
    }
}
