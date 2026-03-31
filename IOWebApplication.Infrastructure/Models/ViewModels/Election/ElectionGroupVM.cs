// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Election
{
    public class ElectionGroupVM
    {
        public int? Id { get; set; }
        [Display(Name = "Съд")]
        public int CourtId { get; set; }
        [Display(Name = "Номер на документ")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public long DocumentId { get; set; }
        public string DocumentNumber { get; set; }
        [Display(Name = "Наименование")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DocumentDate { get; set; }
        public string Label { get; set; }
        [Display(Name = "Описание")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public string Description { get; set; }
        [Display(Name = "Вид избор")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int ElectionTypeId { get; set; }
        public string ElectionType { get; set; }
        public string UserId { get; set; }

        public DateTime DateWrt { get; set; }

    }
}
