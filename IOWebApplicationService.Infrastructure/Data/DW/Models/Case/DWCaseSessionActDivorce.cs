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
  [Table("dw_case_session_act_divorce")]
  public class DWCaseSessionActDivorce : DWUserDateWRT
  {

    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }
    [Column("id")]
   
    public int Id { get; set; }

 

    [Column("case_id")]
    public int? CaseId { get; set; }

    [Column("case_session_act_id")]
    public int CaseSessionActId { get; set; }

    [Column("reg_number")]
    [Display(Name = "Номер")]
    public string RegNumber { get; set; }

    [Column("reg_date")]
    [Display(Name = "Дата")]
    public DateTime RegDate { get; set; }

    [Column("out_document_id")]
    public long? OutDocumentId { get; set; }

    [Column("country_code")]
    [Display(Name = "Държава при прекратяване на брака в чужбина")]
    public string CountryCode { get; set; }


    [Column("country_code_date")]
    [Display(Name = "Дата")]
    public DateTime? CountryCodeDate { get; set; }

    [Column("marriage_number")]
    [Display(Name = "Акт за сключен граждански брак №")]
    public string MarriageNumber { get; set; }

    [Column("marriage_date")]
    [Display(Name = "Акт за сключен граждански брак дата")]
    [Required(ErrorMessage = "Въведете {0}.")]
    public DateTime MarriageDate { get; set; }

    [Column("marriage_place")]
    [Display(Name = "Място на съставяне")]
    public string MarriagePlace { get; set; }

    [Column("marriage_fault")]
    [Display(Name = "По чия вина се прекратява брака")]
    public string MarriageFault { get; set; }

    [Column("marriage_fault_description")]
    [Display(Name = "Причина за прекратяване на брака")]
    public string MarriageFaultDescription { get; set; }

    [Column("children_under_18")]
    [Display(Name = "Деца под 18 г.")]
    public int ChildrenUnder18 { get; set; }

    [Column("children_over_18")]
    [Display(Name = "Деца над 18 г.")]
    public int ChildrenOver18 { get; set; }

    [Column("case_person_man_id")]
    [Display(Name = "Мъж")]

    public int CasePersonManId { get; set; }

    [Column("case_person_man_name")]
    [Display(Name = "Мъж")]
   
    public string CasePersonManName { get; set; }

    [Column("birth_day_man")]
    [Display(Name = "Дата на раждане")]
    [Required(ErrorMessage = "Въведете {0}.")]
    public DateTime BirthDayMan { get; set; }

    [Column("name_after_marriage_man")]
    [Display(Name = "Име след брака")]
    public string NameAfterMarriageMan { get; set; }

    [Column("married_status_before_man")]
    [Display(Name = "Сем. положение при сключване на брака")]
    public string MarriedStatusBeforeMan { get; set; }

    [Column("marriage_count_man")]
    [Display(Name = "Поредност на брака")]
    public int MarriageCountMan { get; set; }

    [Column("divorce_count_man")]
    [Display(Name = "Поредност на развода")]
    public int DivorceCountMan { get; set; }

    [Column("nationality_man")]
    [Display(Name = "Гражданство")]
    public string NationalityMan { get; set; }

    [Column("education_man")]
    [Display(Name = "Степен на образование")]
    public string EducationMan { get; set; }


    [Column("case_person_woman_id")]
    [Display(Name = "Жена")]
    
    public int CasePersonWomanId { get; set; }

    [Column("case_person_woman_name")]
    [Display(Name = "Жена")]
  
    public string CasePersonWomanName { get; set; }

    [Column("birth_day_woman")]
    [Display(Name = "Дата на раждане")]
    [Required(ErrorMessage = "Въведете {0}.")]
    public DateTime BirthDayWoman { get; set; }

    [Column("name_after_marriage_woman")]
    [Display(Name = "Име след брака")]
    public string NameAfterMarriageWoman { get; set; }

    [Column("married_status_before_woman")]
    [Display(Name = "Сем. положение при сключване на брака")]
    public string MarriedStatusBeforeWoman { get; set; }

    [Column("marriage_count_woman")]
    [Display(Name = "Поредност на брака")]
    public int MarriageCountWoman { get; set; }

    [Column("divorce_count_woman")]
    [Display(Name = "Поредност на развода")]
    public int DivorceCountWoman { get; set; }

    [Column("nationality_woman")]
    [Display(Name = "Гражданство")]
    public string NationalityWoman { get; set; }

    [Column("education_woman")]
    [Display(Name = "Степен на образование")]
    public string EducationWoman { get; set; }


    [Column("date_expired")]
    [Display(Name = "Дата на анулиране сесия")]
    public DateTime? DateExpired { get; set; }
    [Column("date_expired_str")]
    [Display(Name = "Дата на анулиране сесия")]
    public string DateExpiredStr { get; set; }

  }
}
