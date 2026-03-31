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
    /// Протокол за разпределяне
    /// </summary>
    [Table("case_selection_protokol_substitution")]
    public class CaseSelectionProtokolSubstitution : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }
        [Column("court_group_id")]
        public int CourtGroupId { get; set; }

        /// <summary>
        /// Начин на разпределение:1-автоматично,2-ръчно
        /// </summary>
        [Column("selection_mode_id")]
        public int SelectionModeId { get; set; }

 
    
        [Column("description")]
        public string Description { get; set; }

        [Column("selection_date")]
        public DateTime SelectionDate { get; set; }
        [Column("declare_date")]
        public DateTime DeclareDate { get; set; }
        [Column("substitution_from_date")]
        public DateTime SubstitutionDate { get; set; }
        [Column("substitution_to_date")]
        public DateTime SubstitutionToDate { get; set; }


        [Column("substituted_lawunit_id")]
        public int SubstitutedLawUnitId { get; set; }

        /// <summary>
        /// id на избрания съдия/заседател
        /// </summary>
        [Column("selected_lawunit_id")]
        public int? SelectedLawUnitId { get; set; }


        [Column("selection_protokol_state_id")]
        public int SelectionProtokolStateId { get; set; }

       
 

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CourtGroupId))]
        public virtual CourtGroup CourtGroup { get; set; }


        [ForeignKey(nameof(SelectionModeId))]
        public virtual SelectionMode SelectionMode { get; set; }

     
        [ForeignKey(nameof(SelectedLawUnitId))]
        public virtual LawUnit SelectedLawUnit { get; set; }

        [ForeignKey(nameof(SubstitutedLawUnitId))]
        public virtual LawUnit SubstitutedLawUnit { get; set; }


        [ForeignKey(nameof(SelectionProtokolStateId))]
        public virtual SelectionProtokolState SelectionProtokolState { get; set; }

        
   

        public virtual ICollection<CaseSelectionProtokolSubstitutionLawUnit> LawUnits { get; set; }

    }
}
