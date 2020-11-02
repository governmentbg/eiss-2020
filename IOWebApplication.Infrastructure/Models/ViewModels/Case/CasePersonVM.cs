// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CasePersonVM : PersonNamesBase
    {
        public int Id { get; set; }

        public int? CourtId { get; set; }

        public int CaseId { get; set; }
        public int CaseTypeId { get; set; }
        public int CaseGroupId { get; set; }
        public int? CaseSessionId { get; set; }

        [Display(Name = "Вид лице")]
        [Range(1, int.MaxValue, ErrorMessage = "Вид лице")]
        public int PersonRoleId { get; set; }

        [Display(Name = "Характер на лицето")]
        public int? PersonMaturityId { get; set; }

        [Display(Name = "Военно звание")]
        public int? MilitaryRangId { get; set; }

        //[Display(Name = "Първоначална страна")]
        //public bool IsInitialPerson { get; set; }

        [Display(Name = "От дата")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        public int FromPersonId { get; set; }
        public bool IsExpired { get; set; }

        [Display(Name = "Задържан")]
        public bool IsArrested { get; set; }

        [Display(Name = "Вид на фирмата")]
        public int? CompanyTypeId { get; set; }

        [Display(Name = "Данъчен номер")]
        public string TaxNumber { get; set; }

        [Display(Name = "Дата на пререгистрация в АВ")]
        public DateTime? ReRegisterDate { get; set; }

        [Display(Name = "Починало лице")]
        public bool IsDeceased { get; set; }
    }
}
