// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class BankAccountVM
    {
        public int Id { get; set; }

        public string IBAN { get; set; }

        public string BIC { get; set; }

        public string BankName { get; set; }

        public bool IsMainAccount { get; set; }
    }

    public class BankAccountEditVM
    {
        public int Id { get; set; }

        public int SourceType { get; set; }

        public long SourceId { get; set; }

        [Display(Name = "IBAN")]
        [RegularExpression("[a-zA-Z]{2}[0-9]{2}[a-zA-Z0-9]{4}[0-9]{6}([a-zA-Z0-9]?){0,16}", ErrorMessage = "Невалиден {0}.")]
        public string IBAN { get; set; }

        [Display(Name = "BIC")]
        public string BIC { get; set; }

        [Display(Name = "Име на банката")]
        public string BankName { get; set; }

        [Display(Name = "Основна сметка")]
        public bool IsMainAccount { get; set; }
    }
}
