using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Съдебни актове
    /// </summary>
    [Table("case_session_act_selection_protocol_acts")]
    public class CaseSessionActSelectionProtocolAct : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

       
        [Column("case_session_act_selection_protocol_id")]
        public int CaseSessionActSelectionProtokolId { get; set; }

        [Column("case_session_act_id")]
        public int CaseSessionActId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("is_final")]
        [Display(Name = "Финализиращ")]
        public bool IsFinal { get; set; }

        [Column("is_canceleling")]
        [Display(Name = "За прекратяване")]
        public bool IsCanceling { get; set; }

        [Column("is_became_final_doc")]
        [Display(Name = "Влязъл в сила")]
        public bool IsBeacameFinal{ get; set; }

        [Column("act_result_id")]
        [Display(Name = "Резултат/степен на уважаване на иска")]
        public int? ActResultId { get; set; }

        [Column("is_selected")]
        [Display(Name = "Избран")]
        public bool IsSelected { get; set; }


        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct Act { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        //[ForeignKey(nameof(ActComplainResultId))]
        //public virtual ActComplainResult ActComplainResult { get; set; }

        [ForeignKey(nameof(ActResultId))]
        public virtual ActResult ActResult { get; set; }

        
      

    }
}
