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
  [Table("dw_case_session_act")]
  public class DWCaseSessionAct : DWUserDateWRT
  {

    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }


    [Column("id")]
    public int Id { get; set; }

    [Column("case_session_id")]
    public int CaseSessionId { get; set; }
    [Column("case_id")]
    public int CaseId { get; set; }

    /// <summary>
    /// Вид на документ
    /// </summary>
    [Column("act_type_id")]
    [Display(Name = "Тип")]
     public int ActTypeId { get; set; }

    [Column("act_type_name")]
     public string ActTypeName { get; set; }
    /// <summary>
    /// Под-вид на документ
    /// </summary>
    [Column("act_kind_id")]
    [Display(Name = "Вид")]
    public int? ActKindId { get; set; }
    [Column("act_kind_name")]
      public string ActKindName { get; set; }


    /// <summary>
    /// Вид на резултат
    /// </summary>
    [Column("act_result_id")]
    [Display(Name = "Резултат от обжалване")]
    public int? ActResultId { get; set; }
    [Column("act_result_name")]
  
    public string ActResultName { get; set; }

    /// <summary>
    /// По съд, основен вид дело, вид акт
    /// </summary>
    [Column("reg_number")]
    [Display(Name = "Рег. номер")]
    public string RegNumber { get; set; }

    [Column("reg_date")]
    [Display(Name = "Дата на регистрация")]
    public DateTime? RegDate { get; set; }
    [Column("reg_date_str")]
    [Display(Name = "Дата на регистрация")]
    public string RegDateStr { get; set; }

    [Display(Name = "Дата на връщане")]
    [Column("act_return_date")]
    public DateTime? ActReturnDate { get; set; }
    [Display(Name = "Дата на връщане")]
    [Column("act_return_date_str")]
    public string ActReturnDateStr { get; set; }

    [Column("reg_number_full")]
    [Display(Name = "Рег. номер")]
    public string RegNumberFull { get; set; }

    /// <summary>
    /// Дали е финализиращ документ, да генерира ЕКЛИ код
    /// </summary>
    [Column("is_final_doc")]
    [Display(Name = "Финализиращ акт")]
    public bool IsFinalDoc { get; set; }

    /// <summary>
    /// Дали е готов за публикуване
    /// </summary>
    [Column("is_ready_for_publish")]
    [Display(Name = "Готово за публикуване")]
    public bool IsReadyForPublish { get; set; }

    

    /// <summary>
    /// Дата на постановяване
    /// </summary>
    [Column("act_date")]
    [Display(Name = "Дата")]
    public DateTime? ActDate { get; set; }

    /// <summary>
    /// Дата на обявяване на акта: подписване от последния съдия
    /// </summary>
    [Display(Name = "Дата на обявяване на акта")]
    [Column("act_declared_date")]
    public DateTime? ActDeclaredDate { get; set; }

    /// <summary>
    /// Дата на обявяване на мотивите: подписване от последния съдия
    /// </summary>
    [Display(Name = "Дата на обявяване на мотивите")]
    [Column("act_motives_declared_date")]
    public DateTime? ActMotivesDeclaredDate { get; set; }

    /// <summary>
    /// Дата на влизане в сила на акта: след приключване на обжалването
    /// </summary>
    [Display(Name = "Дата на влизане в сила на акта")]
    [Column("act_inforced_date")]
    public DateTime? ActInforcedDate { get; set; }

    [Column("act_date_str")]
    [Display(Name = "Дата")]
    public string ActDateStr { get; set; }

    /// <summary>
    /// Дата на обявяване на акта: подписване от последния съдия
    /// </summary>
    [Display(Name = "Дата на обявяване на акта")]
    [Column("act_declared_date_str")]
    public string ActDeclaredDateStr { get; set; }

    /// <summary>
    /// Дата на обявяване на мотивите: подписване от последния съдия
    /// </summary>
    [Display(Name = "Дата на обявяване на мотивите")]
    [Column("act_motives_declared_date_str")]
    public string ActMotivesDeclaredDateStr { get; set; }

    /// <summary>
    /// Дата на влизане в сила на акта: след приключване на обжалването
    /// </summary>
    [Display(Name = "Дата на влизане в сила на акта")]
    [Column("act_inforced_date_str")]
    public string ActInforcedDateStr { get; set; }

    /// <summary>
    /// Диспозитив
    /// </summary>
    [Column("description")]
    [Display(Name = "Диспозитив")]
    public string Description { get; set; }

    /// <summary>
    /// В процес на изготвяне,изготвен, обявен
    /// </summary>
    [Column("act_state_id")]
    [Display(Name = "Статус")]
    [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
    public int ActStateId { get; set; }

    [Column("act_state_name")]
    [Display(Name = "Статус")]

    public string ActStateName { get; set; }

    [Column("secretary_user_id")]
    [Display(Name = "Секретар")]
    public string SecretaryUserId { get; set; }

    [Column("secretary_user_name")]
    [Display(Name = "Секретар")]
    public string SecretaryUserName { get; set; }
    /// <summary>
    /// Потребител обезличил акта
    /// </summary>
    [Column("depersonalize_user_id")]
    public string DepersonalizeUserId { get; set; }

    [Column("depersonalize_user_name")]
    public string DepersonalizeUserName { get; set; }

    [Column("can_appeal")]
    [Display(Name = "Подлежи на обжалване")]
    public bool? CanAppeal { get; set; }

    [Column("act_complain_result_id")]
    [Display(Name = "Резултат/степен на уважаване на иска")]
    public int? ActComplainResultId { get; set; }

    [Column("act_complain_result_state_id")]
 
    public int? ActComplainResultStateId { get; set; }
    [Column("act_complain_result_state_name")]

    public string ActComplainResultStateName { get; set; }


    [Column("date_expired")]
    [Display(Name = "Дата на анулиране сесия")]
    public DateTime? DateExpired { get; set; }
    [Column("date_expired_str")]
    [Display(Name = "Дата на анулиране сесия")]
    public string DateExpiredStr { get; set; }


  }
}
