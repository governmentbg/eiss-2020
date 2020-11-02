// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Core.Models
{
    public class NewsViewModel
    {
        [UIHint("hidden")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Полето {0} е задължително")]
        [Display(Name = "Заглавие")]
        [StringLength(200, MinimumLength = 10, ErrorMessage = "Полето трябва да е между {2} и {1} символа")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Полето {0} е задължително")]
        [Display(Name = "Съдържание")]
        public string Content { get; set; }

        [Display(Name = "Публикувана на")]
        public DateTime PublishDate { get; set; }

        [Display(Name = "Автор")]
        public string Author { get; set; }

        public bool IsUnread { get; set; }
    }
}
