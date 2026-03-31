using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation;
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Съдебни дела
    /// </summary>
    [Table("case")]
    public class Case : BaseInfo_Case, IHaveHistory<CaseH>, IHaveId
    {
        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }

        [ForeignKey(nameof(CaseCharacterId))]
        public virtual CaseCharacter CaseCharacter { get; set; }

        [ForeignKey(nameof(CaseTypeId))]
        public virtual CaseType CaseType { get; set; }

        [ForeignKey(nameof(CaseCodeId))]
        public virtual CaseCode CaseCode { get; set; }

        /// <summary>
        /// Подшифър
        /// </summary>
        [ForeignKey(nameof(CaseCodeSubId))]
        public virtual CaseCodeSub CaseCodeSub { get; set; }

        [ForeignKey(nameof(CaseTypeUnitId))]
        public virtual CaseTypeUnit CaseTypeUnit { get; set; }

        [ForeignKey(nameof(CourtGroupId))]
        public virtual CourtGroup CourtGroup { get; set; }

        [ForeignKey(nameof(LoadGroupLinkId))]
        public virtual LoadGroupLink LoadGroupLink { get; set; }

        [ForeignKey(nameof(ProcessPriorityId))]
        public virtual ProcessPriority ProcessPriority { get; set; }

        [ForeignKey(nameof(CaseStateId))]
        public virtual CaseState CaseState { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }

        [ForeignKey(nameof(CaseReasonId))]
        public virtual CaseReason CaseReason { get; set; }

        [ForeignKey(nameof(JudicalCompositionId))]
        public virtual CourtDepartment JudicalComposition { get; set; }

        [ForeignKey(nameof(OtdelenieId))]
        public virtual CourtDepartment Otdelenie { get; set; }

        [ForeignKey(nameof(IspnCaseCompetenceId))]
        public virtual IspnCaseCompetence IspnCaseCompetence { get; set; }

        [ForeignKey(nameof(MediationProcedureId))]
        public virtual MediationProcedure MediationProcedure { get; set; }

        [ForeignKey(nameof(RnflProcessTypeId))]
        public virtual RnflProcessType RnflProcessType { get; set; }

        public virtual ICollection<CasePerson> CasePersons { get; set; }
        public virtual ICollection<CaseCrime> CaseCrimes { get; set; }
        public virtual ICollection<CasePersonCrime> CasePersonCrimes { get; set; }
        public virtual ICollection<CaseSession> CaseSessions { get; set; }
        public virtual ICollection<MediationCaseSession> MediationCaseSessions { get; set; }
        public virtual ICollection<MediationCaseMediator> MediationCaseMediators { get; set; }
        public virtual ICollection<CaseLawUnit> CaseLawUnits { get; set; }
        public virtual ICollection<CaseLawUnitCount> CaseLawUnitCount { get; set; }
        public virtual ICollection<CaseArchive> CaseArchives { get; set; }
        public virtual ICollection<CaseClassification> CaseClassifications { get; set; }
        public virtual ICollection<CaseLifecycle> CaseLifecycles { get; set; }

        //[InverseProperty(nameof(CaseMigration.CaseId))]
        public virtual ICollection<CaseMigration> CaseMigrations { get; set; }

        public virtual ICollection<CaseSessionAct> CaseSessionActs { get; set; }
        public virtual ICollection<CaseSessionResult> CaseSessionResults { get; set; }
        public virtual ICollection<CaseSessionActComplain> CaseSessionActComplains { get; set; }

        public virtual ICollection<CasePersonSentencePunishment> CasePersonSentencePunishments { get; set; }
        public virtual ICollection<CaseSessionDoc> CaseSessionDocs { get; set; }

        public virtual ICollection<CaseDeactivation> CaseDeactivations { get; set; }

        public virtual ICollection<CaseH> History { get; set; }

        public virtual ICollection<DocumentCaseInfo> DocumentCaseInfos { get; set; }

        public virtual ICollection<ExecList> ExecLists { get; set; }

        [InverseProperty(nameof(Case))]
        public virtual ICollection<CaseSimilarCase> SimilarCases { get; set; }

        public Case()
        {
            CasePersons = new HashSet<CasePerson>();
            CaseSessions = new HashSet<CaseSession>();
            CaseLawUnits = new HashSet<CaseLawUnit>();
            CaseArchives = new HashSet<CaseArchive>();
            CaseLawUnitCount = new HashSet<CaseLawUnitCount>();
            CaseLifecycles = new HashSet<CaseLifecycle>();
            CaseCrimes = new HashSet<CaseCrime>();
            CasePersonCrimes = new HashSet<CasePersonCrime>();
            CaseMigrations = new HashSet<CaseMigration>();
            CaseSessionActs = new HashSet<CaseSessionAct>();
            CaseSessionResults = new HashSet<CaseSessionResult>();
            CaseSessionActComplains = new HashSet<CaseSessionActComplain>();
            CasePersonSentencePunishments = new HashSet<CasePersonSentencePunishment>();
            CaseSessionDocs = new HashSet<CaseSessionDoc>();
            CaseDeactivations = new HashSet<CaseDeactivation>();
            MediationCaseSessions = new HashSet<MediationCaseSession>();
            MediationCaseMediators = new HashSet<MediationCaseMediator>();
            DocumentCaseInfos = new HashSet<DocumentCaseInfo>();
            ExecLists = new HashSet<ExecList>();
            SimilarCases = new HashSet<CaseSimilarCase>();
        }
    }
    /// <summary>
    /// Съдебни дела - история
    /// </summary>
    [Table("case_h")]
    public class CaseH : BaseInfo_Case, IHistory
    {
        [Column("history_id")]
        public int HistoryId { get; set; }

        [Column("history_date_expire")]
        public DateTime? HistoryDateExpire { get; set; }

        [ForeignKey(nameof(Id))]
        public virtual Case Case { get; set; }

        public void ClearForeignKeys()
        {
            Case = null;
        }
    }
    public class BaseInfo_Case : UserDateWRT
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("document_id")]
        public long DocumentId { get; set; }

        [Column("process_priority_id")]
        public int? ProcessPriorityId { get; set; }

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
        public DateTime RegDate { get; set; } = new DateTime(1800, 1, 1);

        [Column("is_old_number")]
        public bool? IsOldNumber { get; set; }


        [Column("case_group_id")]
        public int CaseGroupId { get; set; }

        [Column("case_character_id")]
        public int CaseCharacterId { get; set; }

        [Column("case_type_id")]
        public int CaseTypeId { get; set; }

        [Column("case_code_id")]
        public int? CaseCodeId { get; set; }

        /// <summary>
        /// Идентификатор на подшифри в дело
        /// </summary>
        [Column("case_code_sub_id")]
        [Comment("Идентификатор на подшифри в дело")]
        public int? CaseCodeSubId { get; set; }

        /// <summary>
        /// Съдебна група за разпределяне
        /// </summary>
        [Column("court_group_id")]
        public int? CourtGroupId { get; set; }

        /// <summary>
        /// Група по натовареност, за всички без ВКС
        /// </summary>
        [Column("load_group_link_id")]
        public int? LoadGroupLinkId { get; set; }

        /// <summary>
        /// Само за ВКС, ръчна, фактическа сложност на делото
        /// </summary>
        [Column("complex_index")]
        public decimal ComplexIndex { get; set; }

        /// <summary>
        /// Само за ВКС, фактическа сложност
        /// </summary>
        [Column("complex_index_actual")]
        public int? ComplexIndexActual { get; set; }

        /// <summary>
        /// Само за ВКС, правна сложност
        /// </summary>
        [Column("complex_index_legal")]
        public int? ComplexIndexLegal { get; set; }

        [Column("case_reason_id")]
        public int? CaseReasonId { get; set; }


        [Column("case_type_unit_id")]
        public int? CaseTypeUnitId { get; set; }

        //[Column("case_type_lawunit_count_id")]
        //public int? CaseTypeLawunitCountId { get; set; }

        //[Column("add_judge_count")]
        //public int AddJudgeCount { get; set; }

        //[Column("add_jury_count")]
        //public int AddJuryCount { get; set; }

        /// <summary>
        /// Оценка на натовареността по делото, изчислена средноаритметична за ВКС(CaseCode.LoadIndex,Case.ComplexIndex)
        /// или LoadGroupLink.LoadIndex за останалите съдилища
        /// </summary>
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

        /// <summary>
        /// Дата на влизане в законна сила
        /// </summary>
        [Column("case_inforced_date")]
        public DateTime? CaseInforcedDate { get; set; }

        /// <summary>
        /// Делото е по несъстоятелност
        /// </summary>
        [Column("is_ispn_case")]
        public bool? IsISPNcase { get; set; }

        [Column("case_state_description")]
        public string CaseStateDescription { get; set; }

        /// <summary>
        /// Върнато за ново разглеждане под нов номер
        /// </summary>
        [Column("is_new_case_new_number")]
        public bool? IsNewCaseNewNumber { get; set; }

        /// <summary>
        /// Съдебен състав
        /// </summary>
        [Column("judical_composition_id")]
        public int? JudicalCompositionId { get; set; }

        /// <summary>
        /// Отделение
        /// </summary>
        [Column("otdelenie_id")]
        public int? OtdelenieId { get; set; }

        /// <summary>
        /// дали ЕИСПП Номера е генериран в ЕИСС
        /// </summary>
        [Column("is_generated_eispp_number")]
        public bool? IsGeneratedEisppNumber { get; set; }

        /// <summary>
        /// Дело за възобновяване
        /// </summary>
        [Column("is_renew_case")]
        public bool? IsRenewCase { get; set; }

        /// <summary>
        /// Компетентност за образуване на дело
        /// </summary>
        [Column("ispn_case_competence_id")]
        public int? IspnCaseCompetenceId { get; set; }

        /// <summary>
        /// Брой длъжници по шифри: 21110, 21111 24100, 24111
        /// </summary>
        [Column("debtors_count")]
        public int? DebtorsCount { get; set; }

        /// <summary>
        /// Флаг оказващ делото дали е бързо производство
        /// </summary>
        [Column("is_fast_process")]
        public bool? IsFastProcess { get; set; }

        /// <summary>
        /// Флаг оказващ делото дали подлежи на медиация
        /// </summary>
        [Column("is_mediation")]
        public bool? IsMediation { get; set; }

        /// <summary>
        /// Начална дата на медиация
        /// </summary>
        [Column("start_mediation_date")]
        public DateTime? StartMediationDate { get; set; }

        /// <summary>
        /// Идентификатор на вид процедура по медиация
        /// </summary>
        [Column("mediation_procedure_id")]
        public int? MediationProcedureId { get; set; }

        /// <summary>
        /// Номер на версия на Ред
        /// </summary>
        [Column("row_version")]
        [DefaultValue(0)]
        public int RowVersion { get; set; }

        /// <summary>
        /// Тип ИСПН производство:1-Състояние до 2026 (при дело IsIspn)];2-Предприемачи;3-РНФЛ
        /// </summary>
        [Column("ispn_kind")]
        public int? IspnKind { get; set; }

        /// <summary>
        /// Флаг показващ дали са заредени данни за сходни дела за бързо производство
        /// </summary>
        [Column("is_read_similar_cases")]
        public bool? IsReadSimilarCases { get; set; }

        /*
         * За РНФЛ: датата на обявявяне на акт за стартиране на производство
         */
        /// <summary>
        /// Дата на стартиране изпращането на данни по делото към външни системи, при отложено изпращане
        /// </summary>
        [Column("transfer_start_date")]
        public DateTime? TransferStartDate { get; set; }


        /// <summary>
        /// РНФЛ - Вид производство, по подразбиране - Главно
        /// </summary>
        [Column("rnfl_process_type_id")]
        public int? RnflProcessTypeId { get; set; }
    }
}
