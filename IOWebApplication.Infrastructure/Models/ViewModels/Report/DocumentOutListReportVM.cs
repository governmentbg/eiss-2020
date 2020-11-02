// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class DocumentOutListReportVM
    {
        [Display(Name = "Номер")]
        public string DocumentNumber { get; set; }

        [Display(Name = "Година")]
        public int DocumentYear { get; set; }
        
        [Display(Name = "Дата на изпращане")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DocumentDate { get; set; }
        
        [Display(Name = "Вид на документа")]
        public string DocumentData { get; set; }
        
        [Display(Name = "Вид дело")]
        public string CaseData { get; set; }
        
        [Display(Name = "Адресант")]
        public string DocumentPersons { get; set; }

        [Display(Name = "Текст")]
        public string Description { get; set; }
        
        [Display(Name = "Начин на изпращане")]
        public string DeliveryGroupName { get; set; }

        public int DocumentNumberValue { get; set; }
    }

    public class DocumentOutListFilterReportVM
    {
        [Display(Name = "От дата на изпращане")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата на изпращане")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Основен вид на документа")]
        public int DocumentGroupId { get; set; }

        [Display(Name = "Точен вид на документа")]
        public int DocumentTypeId { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

    }
}
