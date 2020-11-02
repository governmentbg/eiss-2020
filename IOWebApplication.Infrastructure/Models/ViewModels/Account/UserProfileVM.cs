// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Account
{
    public class UserProfileVM
    {
        public string Id { get; set; }
        [Display(Name = "Електронна поща")]
        public string Email { get; set; }
        public string Uic { get; set; }
        public string FullName { get; set; }
        [Display(Name = "Съд")]
        public int CourtId { get; set; }
        public string CourtName { get; set; }
        [Display(Name = "Код на потребител")]
        public string EissId { get; set; }
        [Display(Name = "Служител")]
        public int LawUnitId { get; set; }
        public string LawUnitTypeName { get; set; }
        [Display(Name = "Изпращане напомняния по емайл")]
        public bool WorkNotificationToMail { get; set; }
        [Display(Name = "Активен потребител")]
        public bool IsActive { get; set; }

        [Display(Name = "Групи")]
        public IList<CheckListVM> Roles { get; set; }

        public UserProfileVM()
        {
            Roles = new List<CheckListVM>();
        }
    }
}
