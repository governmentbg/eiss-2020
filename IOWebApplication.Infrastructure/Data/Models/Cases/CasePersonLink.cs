// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Връзки по страни, може и втори представляващ
    /// </summary>
    [Table("case_person_link")]
    public class CasePersonLink: IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_session_id")]
        public int? CaseSessionId { get; set; }

        [Column("case_person_id")]
        [Display(Name = "Страна")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int CasePersonId { get; set; }

        [Column("case_person_rel_id")]
        [Display(Name = "Упълномощено лице")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int CasePersonRelId { get; set; }

        /// <summary>
        /// Допълнително лице по връзката, втори представляващ на първия такъв
        /// </summary>
        [Column("case_person_second_rel_id")]
        [Display(Name = "Втори представляващ")]
        public int? CasePersonSecondRelId { get; set; }

        /// <summary>
        /// Ред на представляване: CasePersonId Чрез CasePersonRelId/ CasePersonRelId като CasePersonRel.PersonRole на CasePersonId
        /// </summary>
        [Column("link_direction_id")]
        [Display(Name = "Ред на представляване")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int LinkDirectionId { get; set; }

        [Column("link_direction_second_id")]
        [Display(Name = "Ред на представляване")]
        public int? LinkDirectionSecondId { get; set; }

        [Column("date_from")]
        [Display(Name = "От дата")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseSessionId))]
        public virtual CaseSession CaseSession { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }

        [ForeignKey(nameof(CasePersonRelId))]
        public virtual CasePerson CasePersonRel { get; set; }

        [ForeignKey(nameof(CasePersonSecondRelId))]
        public virtual CasePerson CasePersonSecondRel { get; set; }

        [ForeignKey(nameof(LinkDirectionId))]
        public virtual LinkDirection LinkDirection { get; set; }

        [ForeignKey(nameof(LinkDirectionSecondId))]
        public virtual LinkDirection LinkDirectionSecond { get; set; }
    }
}
