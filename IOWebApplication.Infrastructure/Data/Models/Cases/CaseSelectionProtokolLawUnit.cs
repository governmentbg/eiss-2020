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
    [Table("case_selection_protokol_lawunit")]
    public class CaseSelectionProtokolLawUnit : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_selection_protokol_id")]
        public int CaseSelectionProtokolId { get; set; }

        /// <summary>
        /// true - когато е избран по групата на делото
        /// </summary>
        [Column("selected_from_case_group")]
        public bool SelectedFromCaseGroup { get; set; }

        [Column("case_group_id")]
        [Display(Name = "Основен вид делo")]
        public int? CaseGroupId { get; set; }

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

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseSelectionProtokolId))]
        public virtual CaseSelectionProtokol CaseSelectionProtokol { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        [ForeignKey(nameof(StateId))]
        public virtual SelectionLawUnitState State { get; set; }
    }
}
