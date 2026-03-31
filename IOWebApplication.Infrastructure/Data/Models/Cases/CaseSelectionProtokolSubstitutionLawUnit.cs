using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Протокол за разпределяне: Списък с участвали лица
    /// </summary>
    [Table("case_selection_protokol_substitution_lawunit")]
    public class CaseSelectionProtokolSubstitutionLawUnit : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

     
        [Column("case_selection_protokol_id")]
        public int CaseSelectionProtokoSubstitutionlId { get; set; }

         /// <summary>
        /// id на избрания съдия/заседател
        /// </summary>
        [Column("lawunit_id")]
        public int LawUnitId { get; set; }

        [Column("load_index")]
        public int LoadIndex { get; set; }

        [Column("case_count")]
        public int CaseCount { get; set; }

        [Column("case_court_count")]
        public int? CaseCourtCount { get; set; }

        [Column("state_id")]
        public int StateId { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseSelectionProtokoSubstitutionlId))]
        public virtual CaseSelectionProtokolSubstitution CaseSelectionProtokolSubstitution { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        [ForeignKey(nameof(StateId))]
        public virtual SelectionLawUnitState State { get; set; }
    }
}
