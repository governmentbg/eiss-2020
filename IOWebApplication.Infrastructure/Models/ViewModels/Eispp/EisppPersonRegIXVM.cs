// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Eispp
{
    public class EisppPersonRegIXVM
    {
        [Display(Name = "ЕГН")]
        public string Uic { get; set; }

        [Display(Name = "Име на лицето")]
        public string PersonName { get; set; }
        
        [Display(Name = "Име RegIX")]
        public string PersonNameRegIX { get; set; }
        
        [Display(Name = "Разлика")]
        public string Difference { get; set; }
    }
}
