using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplicationService.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Data.DW.Models
{
  [Table("dw_case_session_act_complain")]
  public class DWCaseSessionActComplain : DWUserDateWRT
  {

    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }

    [Column("id")]
    
    public int Id { get; set; }

    [Column("case_session_act_id")]
    public int CaseSessionActId { get; set; }

    [Column("case_id")]
    public int? CaseId { get; set; }
    [Column("case_session_id")]
    public int? CaseSessionId { get; set; }


    /// <summary>
    /// Id на Document от CaseSessionDoc - съпровождащ документ със жалбата
    /// </summary>
    [Column("complain_document_id")]
    [Display(Name = "Съпровождащ документ")]
    [Range(1, long.MaxValue, ErrorMessage = "Изберете {0}.")]
    public long ComplainDocumentId { get; set; }


    [Column("document_name")]
    public string ComplainDocumentName { get; set; }

    [Column("document_type_id")]
    public long ComplainDocumentTypeId { get; set; }

    [Column("document_type_name")]
    public string ComplainDocumentTypeName { get; set; }

    [Column("document_date")]
     public DateTime? DocumentDate { get; set; }
    [Column("document_date_str")]
  
    public string DocumentDateStr { get; set; }
    [Column("document_number")]

      public string DocumentNumber { get; set; }
    /// <summary>
    /// Id на OUTDocument от CaseSessionDoc - съпровождащ документ със жалбата
    /// </summary>
    [Column("out_complain_document_id")]
    [Display(Name = "Съпровождащ документ")]
    [Range(1, long.MaxValue, ErrorMessage = "Изберете {0}.")]
    public long? OutComplainDocumentId { get; set; }


    [Column("out_document_name")]
    public string OutComplainDocumentName { get; set; }

    [Column("out_document_type_id")]
    public long? OutComplainDocumentTypeId { get; set; }

    [Column("out_document_type_name")]
    public string OutComplainDocumentTypeName { get; set; }

    [Column("out_document_date")]
    public DateTime? OutDocumentDate { get; set; }
    [Column("out_document_date_str")]

    public string OutDocumentDateStr { get; set; }
    [Column("out_document_number")]

    public string OutDocumentNumber { get; set; }

    [Column("out_court_id")]
    public long? OutCourtId { get; set; }

    [Column("out_court_name")]

    public string OutCourtName { get; set; }

    /// <summary>
    /// Забележка при връщане,
    /// </summary>
    [Column("reject_description")]
    [Display(Name = "Забележка при връщане")]
    public string RejectDescription { get; set; }

    /// <summary>
    /// Статус на обжалването
    /// </summary>
    [Column("complaint_state_id")]
    [Display(Name = "Статус")]
       public int ComplainStateId { get; set; }
    [Column("complaint_state_name")]
    [Display(Name = "Статус")]
    public string ComplainStateName { get; set; }


    [Column("date_expired")]
    [Display(Name = "Дата на анулиране сесия")]
    public DateTime? DateExpired { get; set; }
    [Column("date_expired_str")]
    [Display(Name = "Дата на анулиране сесия")]
    public string DateExpiredStr { get; set; }
  }
}
