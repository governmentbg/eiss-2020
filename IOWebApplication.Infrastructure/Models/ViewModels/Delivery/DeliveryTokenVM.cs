// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryTokenVM
    {
        public string Id { get; set; }

        public string UserId { get; set; }
        public string MobileUserId { get; set; }
        public int CourtId { get; set; }
        [Display(Name = "Съд")]
        public string CourtName { get; set; }
        [Display(Name = "Име на потребителя")]
        public string FullName { get; set; }
        [Display(Name = "Електронна поща")]
        public string UserName { get; set; }
        [Display(Name = "Създаден от")]
        public string UserNameCreate { get; set; }
        [Display(Name = "Спрян от")]
        public string UserNameExpired { get; set; }
        [Display(Name = "Спрян на")]
        public DateTime? DateExpired { get; set; }
        [Display(Name = "Създаден")]
        public DateTime DateCreate { get; set; }
        [Display(Name = "Статус")]
        public string StateName { get; set; }
        public bool IsNew { get; set; }
    }
}
