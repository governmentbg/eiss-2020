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
    [Table("case_session_act_selection_protokol")]
    public class CaseSessionActSelectionProtokol : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        /// <summary>
        /// id на избрания съдия
        /// </summary>
        [Column("selected_lawunit_id")]
        public int SelectedLawUnitId { get; set; }


        [Column("from_date")]
        public DateTime FromDate { get; set; }
        [Column("to_date")]
        public DateTime ToDate { get; set; }

        [Column("selection_date")]
        public DateTime SelectionDate { get; set; }

        [Column("is_final")]
        [Display(Name = "Финализиращ")]
        public bool IsFinal { get; set; }

        [Column("is_canceleling")]
        [Display(Name = "За прекратяване")]
        public bool IsCanceling { get; set; }

        [Column("is_became_final_doc")]
        [Display(Name = "Влязъл в сила")]
        public bool IsBeacameFinal { get; set; }

        [Column("act_result_id")]
        [Display(Name = "Резултат/степен на уважаване на иска")]
        public int? ActResultId { get; set; }

        [Column("description")]
        public string Description { get; set; }


        [Column("selection_protokol_state_id")]
        public int SelectionProtokolStateId { get; set; }



        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }


        [ForeignKey(nameof(SelectedLawUnitId))]
        public virtual LawUnit SelectedLawUnit { get; set; }


        public virtual ICollection<CaseSessionActSelectionProtocolAct> CaseSessionActSelectionProtocolActs { get; set; }

      
    }
}
