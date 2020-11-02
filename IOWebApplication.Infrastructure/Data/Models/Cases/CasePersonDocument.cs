// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Лични документи на лица по дело
    /// </summary>
    [Table("case_person_documents")]
    public class CasePersonDocument : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_person_id")]
        [Display(Name = "Лице")]
        public int CasePersonId { get; set; }

        [Column("issuer_country_code")]
        [Display(Name = "Държава на издаване")]
        [Required(ErrorMessage = "Изберете {0}.")]
        public string IssuerCountryCode { get; set; }

        [Column("issuer_country_name")]
        [Display(Name = "Държава на издаване")]
        public string IssuerCountryName { get; set; }

        [Column("personal_document_id")]
        [Display(Name = "Вид документ")]
        [Required(ErrorMessage = "Изберете {0}.")]
        //eispp_tbl_code = 254
        public string PersonalDocumentTypeId { get; set; }

        [Column("personal_document_label")]
        [Display(Name = "Вид документ")]
        //eispp_tbl_code = 254
        public string PersonalDocumentTypeLabel { get; set; }

        [Column("document_number")]
        [Display(Name = "Номер документ")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public string DocumentNumber { get; set; }

        [Column("document_date")]
        [Display(Name = "Дата на издаване")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DocumentDate { get; set; }

        [Column("document_date_to")]
        [Display(Name = "Валидност")]
        public DateTime? DocumentDateTo { get; set; }

        [Column("issuer_name")]
        [Display(Name = "Издаден от")]
        public string IssuerName { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }


        //################################################################################
        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }
    }
}
