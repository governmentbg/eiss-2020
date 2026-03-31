// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    [Table("case_selection_change")]
    public class CaseSelectionChange : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("change_type_id")]
        public int ChangeTypeId { get; set; }

        [Column("court_group_id")]
        public int CourtGroupId { get; set; }

        [Column("from_lawunit_id")]
        public int? FromLawunitId { get; set; }

        [Column("to_lawunit_id")]
        public int? ToLawunitId { get; set; }

        [Column("to_lawunit_department_id")]
        public int? ToLawunitDepartmentId { get; set; }

        [Column("description")]
        public string Desription { get; set; }

        public DateTime? DeclaredDate { get; set; }

        [Column("change_state_id")]
        public int ChangeStateId { get; set; }

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


        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CourtGroupId))]
        public virtual CourtGroup CourtGroup { get; set; }

        [ForeignKey(nameof(FromLawunitId))]
        public virtual LawUnit FromLawunit { get; set; }

        [ForeignKey(nameof(ToLawunitId))]
        public virtual LawUnit ToLawunit { get; set; }

        [ForeignKey(nameof(ToLawunitDepartmentId))]
        public virtual CourtDepartment ToLawunitDepartment { get; set; }

        [ForeignKey(nameof(ChangeTypeId))]
        public virtual CaseSelectionChangeType ChangeType { get; set; }

        [ForeignKey(nameof(ChangeStateId))]
        public virtual CaseSelectionChangeState ChangeState { get; set; }

        public virtual ICollection<CaseSelectionChangeList> CaseList { get; set; }

        public CaseSelectionChange()
        {
            CaseList = new HashSet<CaseSelectionChangeList>();
        }
    }
}
