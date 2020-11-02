// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Бюлетин за съдимост за лице
    /// </summary>
    [Table("case_person_sentence_bulletin")]
    public class CasePersonSentenceBulletin : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_person_id")]
        public int CasePersonId { get; set; }

        [Column("birth_day_place")]
        [Display(Name = "Месторождение")]
        public string BirthDayPlace { get; set; }

        [Column("birth_day")]
        [Display(Name = "Дата на раждане")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime BirthDay { get; set; }

        [Column("nationality")]
        [Display(Name = "Гражданство")]
        public string Nationality { get; set; }

        [Column("family_marriage")]
        [Display(Name = "Фамилно име придобито при сключване на брак")]
        public string FamilyMarriage { get; set; }

        [Column("father_name")]
        [Display(Name = "Име на бащата")]
        public string FatherName { get; set; }

        [Column("mother_name")]
        [Display(Name = "Име на майката")]
        public string MotherName { get; set; }

        [Column("out_document_id")]
        public long? OutDocumentId { get; set; }

        [Column("is_administrative_punishment")]
        [Display(Name = "по чл.78а НК")]
        public bool? IsAdministrativePunishment { get; set; }

        [Column("sentence_description")]
        [Display(Name = "Присъда")]
        public string SentenceDescription { get; set; }

        [Column("is_convicted")]
        [Display(Name = "Осъждан")]
        public bool? IsConvicted { get; set; }

        [Column("lawunit_sign_id")]
        [Display(Name = "Съдия")]
        public int? LawUnitSignId { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }

        [ForeignKey(nameof(OutDocumentId))]
        public virtual Document OutDocument { get; set; }

        [ForeignKey(nameof(LawUnitSignId))]
        public virtual LawUnit LawUnitSign { get; set; }
    }
}
