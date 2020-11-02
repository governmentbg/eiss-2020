// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class DocumentRegixVM
    {
        [Display(Name = "Валидност")]
        public string DocumentValid { get; set; }

        [Display(Name = "Име")]
        public string PersonName { get; set; }

        [Display(Name = "Идентификатор")]
        public string PersonUic { get; set; }

        [Display(Name = "Вид документ")]
        public string DocumentType { get; set; }

        [Display(Name = "Документ номер")]
        public string DocumentNumber { get; set; }

        [Display(Name = "Издадена от")]
        public string IssuerName { get; set; }

        [Display(Name = "Дата на издаване")]
        public string DocumentDateFrom { get; set; }
        public string DocumentDateFromDate { get; set; }

        [Display(Name = "До дата")]
        public string DocumentDateTo { get; set; }
        public string DocumentDateToDate { get; set; }
    }
}
