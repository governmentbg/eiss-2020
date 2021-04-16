using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplicationService.Infrastructure.Data.DW.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IOWebApplicationService.Infrastructure.Data.DW
{
  public class DWDbContext : DbContext
  {
    public DWDbContext(DbContextOptions<DWDbContext> options)
        : base(options)
    {
    }

    #region OnModelCreating / Builder configuration
    protected override void OnModelCreating(ModelBuilder builder)
    {
      //builder.ApplyConfiguration(new ApplicationUserConfiguration());
      //builder.ApplyConfiguration(new ApplicationRoleConfiguration());
      //builder.ApplyConfiguration(new ApplicationUserRoleConfiguration());
      //builder.ApplyConfiguration(new ApplicationUserClaimConfiguration());
      //builder.ApplyConfiguration(new ApplicationUserLoginConfiguration());
      //builder.ApplyConfiguration(new ApplicationRoleClaimConfiguration());
      //builder.ApplyConfiguration(new ApplicationUserTokenConfiguration());
    }

    #endregion

    //public DbSet<TestTable> TestTable { get; set; }
    public DbSet<DWCase> DWCase { get; set; }
    public DbSet<DWCasePerson> DWCasePerson { get; set; }
    public DbSet<DWCaseLifecycle> DWCaseLifecycle { get; set; }
    public DbSet<DWCaseLawUnit> DWCaseLawUnit { get; set; }
    public DbSet<DWCaseSessionLawUnit> DWCaseSessionLawUnit { get; set; }
    public DbSet<DWCaseSelectionProtocol> DWDWCaseSelectionProtocol { get; set; }
    public DbSet<DWCaseSelectionProtocolLawunit> DWCaseSelectionProtocolLawunit { get; set; }

    public DbSet<DWCaseSelectionProtocolCompartment> DWCaseSelectionProtocolCompartment { get; set; }


    public DbSet<DWCaseSession> DWCaseSession { get; set; }
    public DbSet<DWCaseSessionResult> DWCaseSessionResult { get; set; }
    public DbSet<DWCaseSessionAct> DWCaseSessionAct { get; set; }
    public DbSet<DWCaseSessionActComplain> DWCaseSessionActComplain { get; set; }
    public DbSet<DWCaseSessionActCoordination> DWCaseSessionActCoordination { get; set; }
    public DbSet<DWCaseSessionActComplainPerson> DWCaseSessionActComplainPerson { get; set; }
    public DbSet<DWCaseSessionActComplainResult> DWCaseSessionActComplainResult { get; set; }

    public DbSet<DWCaseSessionActDivorce> DWCaseSessionActDivorce { get; set; }
    public DbSet<DWDocument> DWDocument { get; set; }
    public DbSet<DWDocumentDecision> DWDocumentDecision { get; set; }
    public DbSet<DWDocumentDecisionCase> DWDocumentDecisionCase { get; set; }
    public DbSet<DWDocumentPerson> DWDocumentPerson { get; set; }
    public DbSet<DWDocumentLink> DWDocumentLink { get; set; }
    public DbSet<DWDocumentInstitutionCaseInfo> DWDocumentInstitutionCaseInfo { get; set; }

    public DbSet<DWDocumentCaseInfo> DWDocumentCaseInfo { get; set; }
    public DbSet<DWErrorLog> DWErrorLog { get; set; }
    #region Common

    //public DbSet<Court> Courts { get; set; }

    #endregion


    #region Nomenclatures

    // public DbSet<ActCoordinationType> ActCoordinationType { get; set; }
    //public DbSet<ActMotiveState> ActMotiveState { get; set; }
    //public DbSet<ActResult> ActResults { get; set; }

    //public DbSet<ActState> ActStates { get; set; }

    //public DbSet<ActType> ActTypes { get; set; }
    //public DbSet<ActKind> ActKinds { get; set; }
    //public DbSet<ActTypeCourtLink> ActTypeCourtTypes { get; set; }

    //public DbSet<AddressType> AddressTypes { get; set; }
    //public DbSet<Bank> Banks { get; set; }
    //public DbSet<CaseBankAccountType> CaseBankAccountTypes { get; set; }
    //public DbSet<CaseCharacter> CaseCharacters { get; set; }
    //public DbSet<CaseCodeGroup> CaseCodeGroups { get; set; }
    //public DbSet<CaseCodeGrouping> CaseCodeGroupings { get; set; }

    //public DbSet<CaseGroup> CaseGroups { get; set; }
    //public DbSet<CaseCode> CaseCode { get; set; }
    //public DbSet<CaseType> CaseType { get; set; }
    //public DbSet<CaseTypeUnit> CaseTypeUnit { get; set; }
    //public DbSet<CourtGroup> CourtGroup { get; set; }
    //public DbSet<ProcessPriority> ProcessPriority { get; set; }
    //public DbSet<CaseState> CaseState { get; set; }

    // public DbSet<ApplicationUser> ApplicationUser { get; set; }

    //public DbSet<CaseInstance> CaseInstances { get; set; }
    //public DbSet<CaseLoadAddActivity> CaseLoadAddActivity { get; set; }
    //public DbSet<CaseLoadAddActivityIndex> CaseLoadAddActivityIndex { get; set; }
    //public DbSet<CaseLoadElementGroup> CaseLoadElementGroup { get; set; }
    //public DbSet<CaseLoadElementType> CaseLoadElementType { get; set; }
    //public DbSet<CaseMoneyExpenseType> CaseMoneyExpenseTypes { get; set; }
    //public DbSet<CaseMoneyClaimGroup> CaseMoneyClaimGroups { get; set; }
    //public DbSet<CaseMoneyClaimType> CaseMoneyClaimTypes { get; set; }
    //public DbSet<CaseMoneyCollectionGroup> CaseMoneyCollectionGroups { get; set; }
    //public DbSet<CaseMoneyCollectionKind> CaseMoneyCollectionKinds { get; set; }
    //public DbSet<CaseMoneyCollectionType> CaseMoneyCollectionTypes { get; set; }
    //public DbSet<Currency> Currencies { get; set; }
    //public DbSet<CaseMigrationType> CaseMigrationType { get; set; }

    //public DbSet<CaseState> CaseStates { get; set; }

    //public DbSet<CaseType> CaseTypes { get; set; }
    //public DbSet<CaseTypeCode> CaseTypeCode { get; set; }
    //public DbSet<CaseTypeCharacter> CaseTypeCharacter { get; set; }
    //public DbSet<CaseTypeUnit> CaseTypeUnit { get; set; }
    //public DbSet<CaseTypeUnitCount> CaseTypeUnitCount { get; set; }
    //public DbSet<CaseReason> CaseReason { get; set; }

    //public DbSet<Classification> Classifications { get; set; }
    //public DbSet<CodeMapping> CodeMapping { get; set; }
    //public DbSet<ComplainType> ComplainTypes { get; set; }

    //public DbSet<CounterResetType> CounterResetTypes { get; set; }
    //public DbSet<CounterType> CounterTypes { get; set; }
    //public DbSet<CourtType> CourtTypes { get; set; }
    //public DbSet<CourtTypeCaseType> CourtTypeCaseType { get; set; }
    //public DbSet<CourtTypeSessionType> CourtTypeSessionType { get; set; }

    //public DbSet<DayType> DayTypes { get; set; }
    //public DbSet<DeadlineGroup> DeadlineGroup { get; set; }
    //public DbSet<DeadlineType> DeadlineType { get; set; }

    //public DbSet<DeliveryDirectionGroup> DeliveryDirectionGroups { get; set; }
    //public DbSet<DeliveryGroup> DeliveryGroups { get; set; }
    //public DbSet<DeliveryNumberType> DeliveryNumberType { get; set; }
    //public DbSet<DeliveryOper> DeliveryOper { get; set; }

    //public DbSet<DeliveryType> DeliveryTypes { get; set; }
    //public DbSet<DepartmentType> DepartmentTypes { get; set; }
    //public DbSet<DismisalType> DismisalType { get; set; }
    //public DbSet<DocumentDirection> DocumentDirections { get; set; }
    //public DbSet<DocumentGroup> DocumentGroups { get; set; }
    //public DbSet<DocumentKind> DocumentKinds { get; set; }
    //public DbSet<DocumentRegister> DocumentRegisters { get; set; }
    //public DbSet<DocumentRegisterType> DocumentRegisterTypes { get; set; }
    //public DbSet<DocumentTemplateState> DocumentTemplateStates { get; set; }
    //public DbSet<DocumentType> DocumentTypes { get; set; }
    //public DbSet<DocumentTypeCaseType> DocumentTypeCaseTypes { get; set; }
    //public DbSet<DocumentTypeCourtType> DocumentTypeCourtTypes { get; set; }
    //public DbSet<DocumentTypeGrouping> DocumentTypeGroupings { get; set; }
    //public DbSet<EisppTbl> EisppTbls { get; set; }
    //public DbSet<EisppTblElement> EisppTblElements { get; set; }

    //public DbSet<EvidenceState> EvidenceState { get; set; }
    //public DbSet<ExecListMoneyType> ExecListMoneyTypes { get; set; }

    //public DbSet<ExpenceMoneyType> ExpenceMoneyTypes { get; set; }

    //public DbSet<HtmlTemplateType> HtmlTemplateTypes { get; set; }
    //public DbSet<HtmlTemplateParam> HtmlTemplateParam { get; set; }
    //public DbSet<InstitutionCaseType> InstitutionCaseTypes { get; set; }
    //public DbSet<InstitutionType> InstitutionTypes { get; set; }
    //public DbSet<IntergationType> IntergationType { get; set; }

    //public DbSet<JudgeRole> JudgeRoles { get; set; }
    //public DbSet<JudgeSeniority> JudgeSeniorities { get; set; }

    //public DbSet<LawBase> LawBase { get; set; }

    //public DbSet<LawUnitType> LawUnitTypes { get; set; }
    //public DbSet<LawUnitPosition> LawUnitPosition { get; set; }

    //public DbSet<LifecycleType> LifecycleTypes { get; set; }

    //public DbSet<LinkDirection> LinkDirections { get; set; }
    //public DbSet<LoadGroup> LoadGroups { get; set; }
    //public DbSet<LoadGroupLink> LoadGroupLinks { get; set; }
    //public DbSet<LoadGroupLinkCode> LoadGroupLinkCodes { get; set; }

    //public DbSet<MilitaryRang> MilitaryRangs { get; set; }

    //public DbSet<MoneyGroup> MoneyGroup { get; set; }
    //public DbSet<MoneyType> MoneyTypes { get; set; }
    //public DbSet<MoneyFeeType> MoneyFeeTypes { get; set; }
    //public DbSet<MoneyFeeDocumentGroup> MoneyFeeDocumentGroups { get; set; }

    //public DbSet<MovementType> MovementType { get; set; }

    //public DbSet<NotificationDeliveryGroup> NotificationDeliveryGroups { get; set; }
    //public DbSet<NotificationDeliveryType> NotificationDeliveryTypes { get; set; }
    //public DbSet<NotificationDeliveryGroupState> NotificationDeliveryGroupStates { get; set; }
    //public DbSet<NotificationMode> NotificationModes { get; set; }

    //public DbSet<NotificationState> NotificationStates { get; set; }

    //public DbSet<NotificationType> NotificationTypes { get; set; }
    //public DbSet<OrganizationLevel> OrganizationLevel { get; set; }

    //public DbSet<PaymentType> PaymentTypes { get; set; }
    //public DbSet<PeriodType> PeriodTypes { get; set; }
    //public DbSet<PersonMaturity> PersonMaturities { get; set; }
    //public DbSet<PersonRole> PersonRoles { get; set; }
    //public DbSet<PersonRoleCaseType> PersonRoleCaseTypes { get; set; }

    //public DbSet<ProcessPriority> ProcessPriorities { get; set; }

    //public DbSet<PunishmentActivity> PunishmentActivities { get; set; }


    //public DbSet<RecidiveType> RecidiveTypes { get; set; }

    //public DbSet<RoleKind> RoleKinds { get; set; }

    //public DbSet<SelectionLawUnitState> SelectionLawUnitStates { get; set; }
    //public DbSet<SelectionProtokolState> SelectionProtokolState { get; set; }
    //public DbSet<SelectionMode> SelectionModes { get; set; }
    //public DbSet<SentenceExecPeriod> SentenceExecPeriods { get; set; }
    //public DbSet<SentenceRegimeType> SentenceRegimeTypes { get; set; }

    //public DbSet<SentenceResultType> SentenceResultTypes { get; set; }

    //public DbSet<SentenceType> SentenceTypes { get; set; }
    //public DbSet<SessionActGroup> SessionActGroups { get; set; }
    //public DbSet<SessionActType> SessionActTypes { get; set; }
    //public DbSet<SessionDocState> SessionDocStates { get; set; }
    //public DbSet<SessionDuration> SessionDuration { get; set; }
    //public DbSet<SessionMeetingType> SessionMeetingType { get; set; }

    //public DbSet<SessionResult> SessionResults { get; set; }
    //public DbSet<SessionResultBase> SessionResultBase { get; set; }

    //public DbSet<SessionState> SessionState { get; set; }
    //public DbSet<SessionType> SessionTypes { get; set; }
    //public DbSet<SessionTypeState> SessionTypeStates { get; set; }

    //public DbSet<Speciality> Specialities { get; set; }

    //public DbSet<TaskAction> TaskActions { get; set; }
    //public DbSet<TaskExecution> TaskExecution { get; set; }
    //public DbSet<TaskType> TaskType { get; set; }
    //public DbSet<TaskTypeSourceType> TaskTypeSourceType { get; set; }

    //public DbSet<TaskState> TaskStates { get; set; }

    //public DbSet<UicType> UicTypes { get; set; }
    //public DbSet<RegixType> RegixTypes { get; set; }

    //public DbSet<WorkNotificationType> WorkNotificationTypes { get; set; }
    //public DbSet<DecisionType> DecisionTypes { get; set; }
    //public DbSet<DocumentTypeDecisionType> DocumentTypeDecisionTypes { get; set; }
    //public DbSet<DocumentDecisionState> DocumentDecisionStates { get; set; }
    //public DbSet<SessionResultGrouping> SessionResultGroupings { get; set; }
    #endregion Nomenclatures


  }
}
