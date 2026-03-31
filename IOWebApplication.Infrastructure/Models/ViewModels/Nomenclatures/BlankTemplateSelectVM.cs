// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    public class BlankTemplateSelectVM
    {
        [Display(Name="Изберете бланка")]
        public int BlankTemplateId { get; set; }
    }
}
