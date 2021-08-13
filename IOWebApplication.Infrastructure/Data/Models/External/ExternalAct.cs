//using IOWebApplication.Infrastructure.Contracts;
//using IOWebApplication.Infrastructure.Data.Models.Base;
//using IOWebApplication.Infrastructure.Data.Models.Common;
//using IOWebApplication.Infrastructure.Data.Models.Identity;
//using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace IOWebApplication.Infrastructure.Data.Models.Cases
//{
//    /// <summary>
//    /// Съдебни актове по дела от външни системи
//    /// </summary>
//    [Table("ext_act")]
//    public class ExternalAct : UserDateWRT, IExpiredInfo
//    {
//        [Column("id")]
//        [Key]
//        public int Id { get; set; }

//        [Column("court_id")]
//        public int CourtId { get; set; }
        
//        [Column("case_group_id")]
//        public int CaseGroupId { get; set; }
//        public int CaseTypeId { get; set; }
//        public string CaseRegNumber { get; set; }
//        public string ActTypeId { get; set; }
//        public string ActRegNumber { get; set; }
//        public DateTime ActRegDate { get; set; }

//        /// <summary>
//        /// Дата на обявяване на акта: подписване от последния съдия
//        /// </summary>
//        [Display(Name = "Дата на постановяване")]
//        [Column("act_declared_date")]
//        public DateTime? ActDeclaredDate { get; set; }

//        /// <summary>
//        /// Дата на обявяване на мотивите: подписване от последния съдия
//        /// </summary>
//        [Display(Name = "Дата на обявяване на мотивите")]
//        [Column("act_motives_declared_date")]
//        public DateTime? ActMotivesDeclaredDate { get; set; }


//        [ForeignKey(nameof(CourtId))]
//        public virtual Court Court { get; set; }



//        [ForeignKey(nameof(ActTypeId))]
//        public virtual ActType ActType { get; set; }

//        [ForeignKey(nameof(ActCreatorUserId))]
//        public virtual ApplicationUser ActCreatorUser { get; set; }

//        [ForeignKey(nameof(MotiveCreatorUserId))]
//        public virtual ApplicationUser MotiveCreatorUser { get; set; }

//        public virtual ICollection<CaseSessionActCoordination> ActCoordination { get; set; }

//        [Column("date_expired")]
//        [Display(Name = "Дата на анулиране")]
//        public DateTime? DateExpired { get; set; }

//        [Column("user_expired_id")]
//        public string UserExpiredId { get; set; }

//        [Column("description_expired")]
//        [Display(Name = "Причина за анулиране")]
//        public string DescriptionExpired { get; set; }

//        [ForeignKey(nameof(UserExpiredId))]
//        public virtual ApplicationUser UserExpired { get; set; }

       

//        public CaseSessionAct()
//        {
//            ActCoordination = new HashSet<CaseSessionActCoordination>();
          
//        }
//    }

//    /// <summary>
//    /// Съдебни актове - история
//    /// </summary>
//    [Table("case_session_act_h")]
//    public class CaseSessionActH : BaseInfo_CaseSessionAct, IHistory
//    {
//        [Column("history_id")]
//        public int HistoryId { get; set; }

//        [Column("history_date_expire")]
//        public DateTime? HistoryDateExpire { get; set; }


//        [ForeignKey(nameof(Id))]
//        public virtual CaseSessionAct CaseSessionAct { get; set; }


//    }
//    public class BaseInfo_CaseSessionAct : UserDateWRT
//    {
//        [Column("id")]
//        public int Id { get; set; }

//        [Column("court_id")]
//        public int? CourtId { get; set; }

//        [Column("case_id")]
//        public int? CaseId { get; set; }

//        [Column("case_session_id")]
//        public int CaseSessionId { get; set; }

//        /// <summary>
//        /// Вид на документ
//        /// </summary>
//        [Column("act_type_id")]
//        [Display(Name = "Тип")]
//        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
//        public int ActTypeId { get; set; }

//        /// <summary>
//        /// Под-вид на документ
//        /// </summary>
//        [Column("act_kind_id")]
//        [Display(Name = "Вид")]
//        public int? ActKindId { get; set; }

//        /// <summary>
//        /// Вид на резултат
//        /// </summary>
//        [Column("act_result_id")]
//        [Display(Name = "Резултат от обжалване")]
//        public int? ActResultId { get; set; }

//        /// <summary>
//        /// По съд, основен вид дело, вид акт
//        /// </summary>
//        [Column("reg_number")]
//        [Display(Name = "Рег. номер")]
//        public string RegNumber { get; set; }

//        [Column("reg_date")]
//        [Display(Name = "Дата на регистрация")]
//        public DateTime? RegDate { get; set; }

//        /// <summary>
//        /// Дали е финализиращ документ, да генерира ЕКЛИ код
//        /// </summary>
//        [Column("is_final_doc")]
//        [Display(Name = "Финализиращ акт")]
//        public bool IsFinalDoc { get; set; }

//        /// <summary>
//        /// Дали е готов за публикуване
//        /// </summary>
//        [Column("is_ready_for_publish")]
//        [Display(Name = "Готово за публикуване")]
//        public bool IsReadyForPublish { get; set; }

//        [Column("ecli_code")]
//        [Display(Name = "ECLI код")]
//        public string EcliCode { get; set; }

//        /// <summary>
//        /// Дата на постановяване
//        /// </summary>
//        [Column("act_date")]
//        [Display(Name = "Дата")]
//        public DateTime? ActDate { get; set; }

//        /// <summary>
//        /// Дата на обявяване на акта: подписване от последния съдия
//        /// </summary>
//        [Display(Name = "Дата на постановяване")]
//        [Column("act_declared_date")]
//        public DateTime? ActDeclaredDate { get; set; }

//        /// <summary>
//        /// Дата на обявяване на мотивите: подписване от последния съдия
//        /// </summary>
//        [Display(Name = "Дата на обявяване на мотивите")]
//        [Column("act_motives_declared_date")]
//        public DateTime? ActMotivesDeclaredDate { get; set; }

//        /// <summary>
//        /// Дата на влизане в сила на акта: след приключване на обжалването
//        /// </summary>
//        [Display(Name = "Дата на влизане в сила на акта")]
//        [Column("act_inforced_date")]
//        public DateTime? ActInforcedDate { get; set; }

//        /// <summary>
//        /// Диспозитив
//        /// </summary>
//        [Column("description")]
//        [Display(Name = "Диспозитив")]
//        public string Description { get; set; }

//        /// <summary>
//        /// В процес на изготвяне,изготвен, обявен
//        /// </summary>
//        [Column("act_state_id")]
//        [Display(Name = "Статус")]
//        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
//        public int ActStateId { get; set; }

//        [Column("secretary_user_id")]
//        [Display(Name = "Секретар")]
//        public string SecretaryUserId { get; set; }

//        /// <summary>
//        /// Потребител, създал бланката на акта
//        /// </summary>
//        [Column("act_creator_user_id")]
//        public string ActCreatorUserId { get; set; }

//        /// <summary>
//        /// Потребител, създал бланката на мотивите
//        /// </summary>
//        [Column("motive_creator_user_id")]
//        public string MotiveCreatorUserId { get; set; }

//        /// <summary>
//        /// Потребител обезличил акта
//        /// </summary>
//        [Column("depersonalize_user_id")]
//        public string DepersonalizeUserId { get; set; }

//        [Column("can_appeal")]
//        [Display(Name = "Подлежи на обжалване")]
//        public bool? CanAppeal { get; set; }

//        [Column("act_complain_result_id")]
//        [Display(Name = "Резултат/степен на уважаване на иска")]
//        public int? ActComplainResultId { get; set; }

//        [Column("act_complain_index_id")]
//        [Display(Name = "Индекс на резултат от обжалване")]
//        public int? ActComplainIndexId { get; set; }

//        [Column("act_term")]
//        [Display(Name = "За срок")]
//        public string ActTerm { get; set; }

//        [Column("act_ispn_reason_id")]
//        [Display(Name = "Основание по ТЗ")]
//        public int? ActISPNReasonId { get; set; }

//        [Column("act_ispn_debtor_state_id")]
//        [Display(Name = "Статус на длъжник")]
//        public int? ActISPNDebtorStateId { get; set; }

//        [Column("related_act_id")]
//        [Display(Name = "Свързан съдебен акт")]
//        public int? RelatedActId { get; set; }

//        /// <summary>
//        /// Дата на връщане
//        /// </summary>
//        [Display(Name = "Дата на връщане")]
//        [Column("act_return_date")]
//        public DateTime? ActReturnDate { get; set; }

//        /// <summary>
//        /// Дата на начало на обезличаването
//        /// </summary>
//        [Column("depersonalize_start_date")]
//        public DateTime? DepersonalizeStartDate { get; set; }

//        /// <summary>
//        /// Дата на финализиране на обезличаването
//        /// </summary>
//        [Column("depersonalize_end_date")]
//        public DateTime? DepersonalizeEndDate { get; set; }

//        /// <summary>
//        /// Съдия, подписващ изпълнителен лист
//        /// </summary>
//        [Display(Name = "Съдия")]
//        [Column("sign_judge_lawunit_id")]
//        public int? SignJudgeLawUnitId { get; set; }
//    }
//}
