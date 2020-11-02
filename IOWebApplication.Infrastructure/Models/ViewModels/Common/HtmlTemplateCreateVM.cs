// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class HtmlTemplateCreateVM
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Style { get; set; }
        public int PageOrientation { get; set; }

        [Display(Name = "Вид документ")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int HtmlTemplateTypeId { get; set; }

        [Display(Name = "Указател на бланка")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public string Alias { get; set; }

        [Display(Name = "Наименование")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public string Label { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }
    }
}
