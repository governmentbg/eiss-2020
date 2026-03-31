// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Election
{
    public class ElectionPersonAddVM
    {
        public int ElectionGroupId { get; set; }

        [Range(1,int.MaxValue,ErrorMessage ="Изберете {0}.")]
        [Display(Name ="Изберете съдия")]
        public int CourtLawunitId { get; set; }
    }
}
