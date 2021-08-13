// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Eispp
{
    public class EisppCrimePersonVM
    {
        [Display(Name = "Номер на престъпление")]
        public string PneNumber { get; set; }

        [Display(Name = "ЕГН/ЛНЧ")]
        public string Uic { get; set; }

        [Display(Name = "Име на лицето")]
        public string PersonName { get; set; }

        public bool HavePKG { get; set; }
        public bool HaveTS { get; set; }

        [Display(Name = "Разлика")]
        public string Difference { get; set; }
    }
}
