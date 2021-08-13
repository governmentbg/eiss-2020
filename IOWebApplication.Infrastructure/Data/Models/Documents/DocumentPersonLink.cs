// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Documents
{
    [Table("document_person_link")]
    public class DocumentPersonLink : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("document_id")]
        public long DocumentId { get; set; }


        [Column("document_person_id")]
        [Display(Name = "Страна")]
        [Range(1, long.MaxValue, ErrorMessage = "Изберете")]
        public long DocumentPersonId { get; set; }

        [Column("document_person_rel_id")]
        [Display(Name = "Упълномощено лице")]
        [Range(1, long.MaxValue, ErrorMessage = "Изберете")]
        public long DocumentPersonRelId { get; set; }

        /// <summary>
        /// Допълнително лице по връзката, втори представляващ на първия такъв
        /// </summary>
        [Column("document_person_second_rel_id")]
        [Display(Name = "Втори представляващ")]
        public long? DocumentPersonSecondRelId { get; set; }

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
        [NotMapped]
        public long? DocumentResolutionId { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }

        [ForeignKey(nameof(DocumentPersonId))]
        public virtual DocumentPerson DocumentPerson { get; set; }

        [ForeignKey(nameof(DocumentPersonRelId))]
        public virtual DocumentPerson DocumentPersonRel { get; set; }

        [ForeignKey(nameof(DocumentPersonSecondRelId))]
        public virtual DocumentPerson DocumentPersonSecondRel { get; set; }

        [ForeignKey(nameof(LinkDirectionId))]
        public virtual LinkDirection LinkDirection { get; set; }

        [ForeignKey(nameof(LinkDirectionSecondId))]
        public virtual LinkDirection LinkDirectionSecond { get; set; }
    }
}
