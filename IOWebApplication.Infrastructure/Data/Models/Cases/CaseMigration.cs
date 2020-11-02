// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Attributes;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Вертикално движение на дело - между институциите
    /// </summary>
    [Table("case_migration")]
    public class CaseMigration : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_session_act_id")]
        [Display(Name = "Обжалван акт")]
        public int? CaseSessionActId { get; set; }

        [Column("prior_case_id")]
        public int PriorCaseId { get; set; }

        [Column("initial_case_id")]
        [Display(Name = "Първоначално дело")]
        public int InitialCaseId { get; set; }

        [Column("case_migration_type_id")]
        [Display(Name = "Вид движение")]
        public int CaseMigrationTypeId { get; set; }

        [Column("send_to_type_id")]
        public int SendToTypeId { get; set; }

        [Column("send_to_court_id")]
        [Display(Name = "Насрещен съд")]
        [IORequired]
        public int? SendToCourtId { get; set; }

        [Column("send_to_institution_type_id")]
        [Display(Name = "Насрещна институция")]
        public int? SendToInstitutionTypeId { get; set; }

        [Column("send_to_institution_id")]
        [Display(Name = "Институция")]
        [IORequired]
        public int? SendToInstitutionId { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Column("out_document_id")]
        [Display(Name = "Документ за пренасочване")]
        public long? OutDocumentId { get; set; }

        [Column("return_case_id")]
        [Display(Name = "Дело, подлежащо на връщане")]
        public int? ReturnCaseId { get; set; }

        [Column("out_case_migration_id")]
        public int? OutCaseMigrationId { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(PriorCaseId))]
        public virtual Case PriorCase { get; set; }

        [ForeignKey(nameof(InitialCaseId))]
        public virtual Case InitialCase { get; set; }

        [ForeignKey(nameof(CaseMigrationTypeId))]
        public virtual CaseMigrationType CaseMigrationType { get; set; }

        [ForeignKey(nameof(SendToCourtId))]
        public virtual Court SendToCourt { get; set; }

        [ForeignKey(nameof(SendToInstitutionId))]
        public virtual Institution SendToInstitution { get; set; }

        [ForeignKey(nameof(OutDocumentId))]
        public virtual Document OutDocument { get; set; }

        [ForeignKey(nameof(ReturnCaseId))]
        public virtual Case ReturnCase { get; set; }

        [ForeignKey(nameof(OutCaseMigrationId))]
        public virtual CaseMigration OutCaseMigration { get; set; }

        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }

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

        [InverseProperty(nameof(OutCaseMigration))]
        public virtual ICollection<CaseMigration> InCaseMigrations { get; set; }

        public CaseMigration()
        {
            InCaseMigrations = new HashSet<CaseMigration>();
        }
    }
}
