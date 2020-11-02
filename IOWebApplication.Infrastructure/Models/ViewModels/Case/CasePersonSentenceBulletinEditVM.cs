// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CasePersonSentenceBulletinEditVM
    {
        public int Id { get; set; }
        public int? CourtId { get; set; }
        public int? CaseId { get; set; }
        public int CasePersonId { get; set; }
        public int CaseTypeId { get; set; }

        [Display(Name = "Месторождение")]
        public string BirthDayPlace { get; set; }

        [Display(Name = "Дата на раждане")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime BirthDay { get; set; }

        [Display(Name = "Гражданство")]
        public string Nationality { get; set; }

        [Display(Name = "Фамилно име придобито при сключване на брак")]
        public string FamilyMarriage { get; set; }

        [Display(Name = "Име на бащата")]
        public string FatherName { get; set; }

        [Display(Name = "Име на майката")]
        public string MotherName { get; set; }

        [Display(Name = "по чл.78а НК")]
        public bool? IsAdministrativePunishment { get; set; }

        [Display(Name = "Присъда")]
        public string SentenceDescription { get; set; }

        [Display(Name = "Осъждан")]
        public bool IsConvicted { get; set; }

        [Display(Name = "Съдия")]
        public int LawUnitSignId { get; set; }
    }
}
