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
  [Table("dw_case")]
  public class DWCase : DWUserDateWRT
  {

    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }

    [Column("case_id")]
    public int CaseId { get; set; }

   

    [Column("document_id")]
    public long DocumentId { get; set; }

    [Column("document_name")]
    public string DocumentName { get; set; }

    [Column("document_type_id")]
    public long DocumentTypeId { get; set; }

    [Column("document_type_name")]
    public string DocumentTypeName { get; set; }

    [Column("process_priority_id")]
    public int? ProcessPriorityId { get; set; }

    [Column("process_priority_name")]
    public string ProcessPriorityName { get; set; }

    [Column("eispp_number")]
    public string EISSPNumber { get; set; }

    /// <summary>
    /// Кратък 5 цифрен номер на делото
    /// </summary>
    [Column("short_number")]
    public string ShortNumber { get; set; }

    [Column("short_number_value")]
    public int? ShortNumberValue { get; set; }

    /// <summary>
    ///Пълен 14 цифрен номер на делото
    /// </summary>
    [Column("reg_number")]
    public string RegNumber { get; set; }

    [Column("reg_date")]
    public DateTime RegDate { get; set; }
    [Column("reg_date_str")]
    public string RegDateStr { get; set; }

    [Column("is_old_number")]
    public bool? IsOldNumber { get; set; }


    [Column("case_group_id")]
    public int CaseGroupId { get; set; }

    [Column("case_group_name")]
    public string CaseGroupName { get; set; }

    [Column("case_character_id")]
    public int CaseCharacterId { get; set; }
    [Column("case_character_name")]
    public string CaseCharacterName { get; set; }


    [Column("case_type_id")]
    public int CaseTypeId { get; set; }

    [Column("case_type_name")]
    public string CaseTypeName { get; set; }

    [Column("case_type_code")]
    public string CaseTypeCode { get; set; }

    [Column("case_code_id")]
    public int? CaseCodeId { get; set; }

    [Column("case_code_name")]
    public string CaseCodeName { get; set; }
    [Column("case_code_lawbase_description")]
    public string CaseCodeLawbaseDescription { get; set; }
    [Column("case_code_full_object")]
    public string CaseCodeFullObject { get; set; }
    /// <summary>
    /// Съдебна група за разпределяне
    /// </summary>
    [Column("court_group_id")]
    public int? CourtGroupId { get; set; }
    
    [Column("court_group_name")]
    public string  CourtGroupName { get; set; }



    /// <summary>
    /// Група по натовареност, за всички без ВКС
    /// </summary>
    [Column("load_group_link_id")]
    public int? LoadGroupLinkId { get; set; }
    [Column("load_group_id")]
    public int? LoadGrouId { get; set; }
    [Column("load_group_name")]
    public string LoadGrouName { get; set; }



    /// <summary>
    /// Само за ВКС, ръчна, фактическа сложност на делото
    /// </summary>
    [Column("complex_index")]
    public decimal ComplexIndex { get; set; }

    [Column("case_reason_id")]
    public int? CaseReasonId { get; set; }

    [Column("case_reason_name")]
    public string CaseReasonName { get; set; }


    [Column("case_type_unit_id")]
    public int? CaseTypeUnitId { get; set; }

    [Column("case_type_unit_name")]
    public string CaseTypeUnitName { get; set; }

    [Column("load_index")]
    public decimal LoadIndex { get; set; }

    /// <summary>
    /// Коефициент за корекция на тежестта на делото
    /// Общия коефициент е сума на LoadIndex и CorrectionLoadIndex
    /// </summary>
    [Column("correction_load_index")]
    public decimal? CorrectionLoadIndex { get; set; }

    [Column("is_resticted_access")]
    public bool IsRestictedAccess { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("case_state_id")]
    public int CaseStateId { get; set; }

    [Column("case_state_name")]
    public string CaseStateName { get; set; }

    [Column("case_duration_months")]
    public int CaseDurationMonths { get; set; }

    /// <summary>
    /// Дата на влизане в законна сила
    /// </summary>
    [Column("case_inforced_date")]
    public DateTime? CaseInforcedDate { get; set; }

    //[ForeignKey(nameof(CourtId))]
    //public virtual Court Court { get; set; }

    //[ForeignKey(nameof(CaseGroupId))]
    //public virtual CaseGroup CaseGroup { get; set; }

    //[ForeignKey(nameof(CaseCharacterId))]
    //public virtual CaseCharacter CaseCharacter { get; set; }

    //[ForeignKey(nameof(CaseTypeId))]
    //public virtual CaseType CaseType { get; set; }

    //[ForeignKey(nameof(CaseCodeId))]
    //public virtual CaseCode CaseCode { get; set; }

    //[ForeignKey(nameof(CaseTypeUnitId))]
    //public virtual CaseTypeUnit CaseTypeUnit { get; set; }

    //[ForeignKey(nameof(CourtGroupId))]
    //public virtual CourtGroup CourtGroup { get; set; }

 

    //[ForeignKey(nameof(ProcessPriorityId))]
    //public virtual ProcessPriority ProcessPriority { get; set; }

    //[ForeignKey(nameof(CaseStateId))]
    //public virtual CaseState CaseState { get; set; }



  }
}
