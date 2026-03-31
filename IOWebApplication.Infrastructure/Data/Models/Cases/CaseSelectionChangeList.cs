// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    [Table("case_selection_change_list")]
    public class CaseSelectionChangeList
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("case_selection_change_id")]
        public int CaseSelectionChangeId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("from_lawunit_id")]
        public int? FromLawunitId { get; set; }

        [Column("to_lawunit_id")]
        public int? ToLawunitId { get; set; }      

        [Column("case_lawunit_dismisal_id")]
        public int? CaseLawunitDismisalId { get; set; }

        [Column("case_selection_protokol_id")]
        public int? CaseSelectionProtokolId { get; set; }

        [ForeignKey(nameof(CaseSelectionChangeId))]
        public virtual CaseSelectionChange CaseSelectionChange { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(FromLawunitId))]
        public virtual LawUnit FromLawunit { get; set; }

        [ForeignKey(nameof(ToLawunitId))]
        public virtual LawUnit ToLawunit { get; set; }

        [ForeignKey(nameof(CaseSelectionProtokolId))]
        public virtual CaseSelectionProtokol CaseSelectionProtokol { get; set; }

        [ForeignKey(nameof(CaseLawunitDismisalId))]
        public virtual CaseLawUnitDismisal CaseLawunitDismisal { get; set; }
    }
}
