// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Хоризонтално движение на дело - в рамките на същия съд
    /// </summary>
    [Table("case_movement")]
    public class CaseMovement : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("movement_type_id")]
        public int MovementTypeId { get; set; }

        [Column("to_user_id")]
        public string ToUserId { get; set; }

        [Column("court_organization_id")]
        [Display(Name = "Структура")]
        public int? CourtOrganizationId { get; set; }

        [Column("accept_user_id")]
        public string AcceptUserId { get; set; }

        [Column("other_institution")]
        [Display(Name = "Външна институция")]
        public string OtherInstitution { get; set; }

        [Column("date_send")]
        public DateTime DateSend { get; set; }

        [Column("date_accept")]
        public DateTime? DateAccept { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Column("accept_description")]
        [Display(Name = "Описание")]
        public string AcceptDescription { get; set; }

        [Column("disable_description")]
        [Display(Name = "Забележка за анулиране")]
        public string DisableDescription { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(MovementTypeId))]
        public virtual MovementType MovementType { get; set; }

        [ForeignKey(nameof(ToUserId))]
        public virtual ApplicationUser ToUser { get; set; }

        [ForeignKey(nameof(AcceptUserId))]
        public virtual ApplicationUser AcceptUser { get; set; }

        [ForeignKey(nameof(CourtOrganizationId))]
        public virtual CourtOrganization CourtOrganization { get; set; }
    }
}
