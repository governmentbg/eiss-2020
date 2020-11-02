// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IO.LogOperation.Models;
using IOWebApplication.Infrastructure.Data.Models.Audit;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.EISPP;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Messages;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Data.Models.Regix;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.EntityFrameworkCore;

namespace IOWebApplication.Infrastructure.Data.Models
{
    public class ApplicationDbContext : DbContext//  IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        #region OnModelCreating / Builder configuration
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // EKATTE Configuration
            //builder.ApplyConfiguration(new EkAreaConfiguration());
            //builder.ApplyConfiguration(new EkCountryConfiguration());
            //builder.ApplyConfiguration(new EkDistrictConfiguration());
            //builder.ApplyConfiguration(new EkMunincipalityConfiguration());
            //builder.ApplyConfiguration(new EkEkatteConfiguration());
            //builder.ApplyConfiguration(new EkRegionConfiguration());
            //builder.ApplyConfiguration(new EkSobrConfiguration());
            //Idenity configurations
            builder.ApplyConfiguration(new ApplicationUserConfiguration());
            builder.ApplyConfiguration(new ApplicationRoleConfiguration());
            builder.ApplyConfiguration(new ApplicationUserRoleConfiguration());
            builder.ApplyConfiguration(new ApplicationUserClaimConfiguration());
            builder.ApplyConfiguration(new ApplicationUserLoginConfiguration());
            builder.ApplyConfiguration(new ApplicationRoleClaimConfiguration());
            builder.ApplyConfiguration(new ApplicationUserTokenConfiguration());

            //builder.ApplyConfiguration(new InitialDataConfiguration<InstitutionType>(@"InitialData/Nomenclatures/institutionType.json"));
            //builder.ApplyConfiguration(new InitialDataConfiguration<UicType>(@"InitialData/Nomenclatures/uicType.json"));
            //builder.ApplyConfiguration(new InitialDataConfiguration<LawUnitType>(@"InitialData/Nomenclatures/lawUnitType.json"));
            //builder.ApplyConfiguration(new InitialDataConfiguration<CaseGroup>(@"InitialData/Nomenclatures/caseGroup.json"));
            //builder.ApplyConfiguration(new InitialDataConfiguration<DocumentRegister>(@"InitialData/Nomenclatures/documentRegister.json"));
            //builder.ApplyConfiguration(new InitialDataConfiguration<DocumentGroup>(@"InitialData/Nomenclatures/documentGroup.json"));
            //builder.ApplyConfiguration(new InitialDataConfiguration<DocumentType>(@"InitialData/Nomenclatures/documentType.json"));
            //builder.ApplyConfiguration(new InitialDataConfiguration<CourtType>(@"InitialData/Nomenclatures/courtType.json"));
            //builder.ApplyConfiguration(new InitialDataConfiguration<Court>(@"InitialData/Nomenclatures/court.json"));

            builder.Entity<AuditLog>()
                .Property(p => p.InsertedDate)
                .HasDefaultValueSql("now()");

            builder.Entity<AuditLog>()
                .Property(p => p.UpdatedDate)
                .HasDefaultValueSql("now()");

            base.OnModelCreating(builder);

            builder.Entity<Case>().HasKey(x => x.Id);
            builder.Entity<CaseH>().HasKey(x => x.HistoryId);

            builder.Entity<CasePerson>().HasKey(x => x.Id);
            builder.Entity<CasePersonH>().HasKey(x => x.HistoryId);

            builder.Entity<CaseNotification>().HasKey(x => x.Id);
            builder.Entity<CaseNotificationH>().HasKey(x => x.HistoryId);

            builder.Entity<CasePersonAddress>().HasKey(x => x.Id);
            builder.Entity<CasePersonAddressH>().HasKey(x => x.HistoryId);

            builder.Entity<CaseSession>().HasKey(x => x.Id);
            builder.Entity<CaseSessionH>().HasKey(x => x.HistoryId);

            builder.Entity<CaseSessionAct>().HasKey(x => x.Id);
            builder.Entity<CaseSessionActH>().HasKey(x => x.HistoryId);

            builder.Entity<LawUnit>().HasKey(x => x.Id);
            builder.Entity<LawUnitH>().HasKey(x => x.HistoryId);

            builder.Entity<LawUnit>()
               .Property(p => p.DateWrt)
               .HasDefaultValueSql("now()");

            builder.Entity<CounterDocument>()
                .HasKey(x => new { x.CounterId, x.DocumentDirectionId });

            builder.Entity<CounterCase>()
                .HasKey(x => new { x.CounterId, x.CaseGroupId });

            builder.Entity<CounterSessionAct>()
                .HasKey(x => new { x.CounterId, x.SessionActGroupId });

            builder.Entity<SessionActType>()
                .HasKey(x => new { x.SessionActGroupId, x.ActTypeId });

            builder.Entity<DeliveryDirectionGroup>()
              .HasKey(x => new { x.DeliveryGroupId, x.DocumentDirectionId });

            builder.Entity<DocumentRegisterType>()
                .HasKey(x => new { x.DocumentRegisterId, x.DocumentTypeId });

            builder.Entity<CourtGroupCode>()
                .HasKey(csn => new { csn.CourtGroupId, csn.CaseCodeId });

            builder.Entity<PersonAddress>()
                .HasKey(x => new { x.PersonId, x.AddressId });

            builder.Entity<InstitutionAddress>()
                .HasKey(x => new { x.InstitutionId, x.AddressId });

            builder.Entity<LawUnitAddress>()
               .HasKey(x => new { x.LawUnitId, x.AddressId });

            builder.Entity<LoadGroupLinkCode>()
              .HasKey(x => new { x.LoadGroupLinkId, x.CaseCodeId });

            builder.Entity<DocumentTypeCaseType>()
            .HasKey(x => new { x.DocumentTypeId, x.CaseTypeId });

            builder.Entity<DocumentTypeCourtType>()
                .HasKey(x => new { x.DocumentTypeId, x.CourtTypeId });

            builder.Entity<CaseTypeCode>()
                .HasKey(x => new { x.CaseTypeId, x.CaseCodeId });
            builder.Entity<CaseTypeCharacter>()
                .HasKey(x => new { x.CaseTypeId, x.CaseCharacterId });

            builder.Entity<NotificationDeliveryGroupState>()
               .HasKey(x => new { x.NotificationDeliveryGroupId, x.NotificationStateId });

            builder.Entity<PersonRoleCaseType>()
               .HasKey(x => new { x.PersonRoleId, x.CaseTypeId });

            builder.Entity<CourtTypeCaseType>()
             .HasKey(x => new { x.CourtTypeId, x.CaseTypeId });

            builder.Entity<SessionTypeState>()
            .HasKey(x => new { x.SessionTypeId, x.SessionStateId });

            builder.Entity<EisppTblElement>()
            .HasKey(x => new { x.EisppTblCode, x.Code });

            builder.Entity<CourtArchiveIndexCode>()
                .HasKey(csn => new { csn.CourtArchiveIndexId, csn.CaseCodeId });

            builder.Entity<CourtArchiveCommitteeLawUnit>()
                .HasKey(csn => new { csn.CourtArchiveCommitteeId, csn.LawUnitId });

            builder.Entity<MoneyFeeDocumentGroup>()
                .HasKey(x => new { x.MoneyFeeTypeId, x.DocumentGroupId });

            builder.Entity<MoneyFineCaseGroup>()
                .HasKey(x => new { x.MoneyFineTypeId, x.CaseGroupId });

            builder.Entity<TaskTypeSourceType>()
                .HasKey(x => new { x.TaskTypeId, x.SourceType });

            builder.Entity<LawUnitTypePosition>()
                .HasKey(x => new { x.LawUnitTypeId, x.PositionId });

            builder.Entity<ActComplainResultCaseType>()
                .HasKey(x => new { x.ActComplainResultId, x.CaseTypeId });

            builder.Entity<ActComplainIndexCourtTypeCaseGroup>()
                .HasKey(x => new { x.ActComplainIndexId, x.CourtTypeId, x.CaseGroupId });

            builder.Entity<ExecListLawBaseCaseGroup>()
          .HasKey(x => new { x.ExecListLawBaseId, x.CaseGroupId });

            builder.Entity<Case>()
               .HasMany(x => x.CaseMigrations).WithOne(x => x.Case).HasForeignKey(x => x.CaseId);

            builder.Entity<PersonRoleLinkDirection>()
               .HasKey(x => new { x.PersonRoleId, x.LinkDirectionId });

            builder.Entity<NewsUser>()
                .HasKey(k => new { k.NewsId, k.UserId });

            builder.Entity<NewsUser>()
                .HasOne(x => x.News)
                .WithMany(x => x.NewsUsers)
                .HasForeignKey(x => x.NewsId);
        }

        #endregion

        public DbQuery<GetCounterValueVM> GetCounterValueVM { get; set; }
        public DbQuery<ReportCourtStatsVM> ReportCourtStatsVM { get; set; }

        #region Cases

        public DbSet<Case> Cases { get; set; }
        public DbSet<CaseH> CaseH { get; set; }
        public DbSet<CaseBankAccount> CaseBankAccounts { get; set; }

        public DbSet<CaseClassification> CaseClassifications { get; set; }
        public DbSet<CaseCrime> CaseCrime { get; set; }
        public DbSet<CaseDeactivation> CaseDeactivations { get; set; }
        public DbSet<CaseDepersonalizationValue> CaseDepersonalizationValues { get; set; }

        public DbSet<CaseEvidence> CaseEvidence { get; set; }
        public DbSet<CaseEvidenceMovement> CaseEvidenceMovement { get; set; }
        public DbSet<CaseFastProcess> CaseFastProcess { get; set; }

        public DbSet<CaseLawUnit> CaseLawUnits { get; set; }
        public DbSet<CaseLawUnitDismisal> CaseLawUnitDismisals { get; set; }
        public DbSet<CaseLawUnitCount> CaseLawUnitCounts { get; set; }
        public DbSet<CaseLawUnitManualJudge> CaseLawUnitManualJudges { get; set; }
        public DbSet<CaseLawUnitReplace> CaseLawUnitReplaces { get; set; }
        public DbSet<CaseLawUnitTaskChange> CaseLawUnitTaskChanges { get; set; }

        public DbSet<CaseLifecycle> CaseLifecycles { get; set; }
        public DbSet<CaseLoadCorrection> CaseLoadCorrection { get; set; }
        public DbSet<CaseLoadIndex> CaseLoadIndex { get; set; }

        public DbSet<CaseMigration> CaseMigration { get; set; }
        public DbSet<CaseMoney> CaseMoney { get; set; }
        public DbSet<CaseMoneyExpense> CaseMoneyExpenses { get; set; }
        public DbSet<CaseMoneyExpensePerson> CaseMoneyExpensePersons { get; set; }
        public DbSet<CaseMoneyClaim> CaseMoneyClaims { get; set; }
        public DbSet<CaseMoneyCollection> CaseMoneyCollections { get; set; }
        public DbSet<CaseMoneyCollectionPerson> CaseMoneyCollectionPersons { get; set; }

        public DbSet<CaseMovement> CaseMovement { get; set; }

        public DbSet<CaseNotification> CaseNotifications { get; set; }

        public DbSet<CaseNotificationMLink> CaseNotificationMlinks { get; set; }

        public DbSet<CasePerson> CasePersons { get; set; }
        public DbSet<CasePersonH> CasePersonsH { get; set; }

        public DbSet<CasePersonAddress> CasePersonAddresses { get; set; }
        public DbSet<CasePersonAddressH> CasePersonAddressesH { get; set; }
        public DbSet<CasePersonCrime> CasePersonCrimes { get; set; }
        public DbSet<CasePersonDocument> CasePersonDocuments { get; set; }
        public DbSet<CasePersonInheritance> CasePersonInheritances { get; set; }
        public DbSet<CasePersonMeasure> CasePersonMeasures { get; set; }
        public DbSet<CasePersonLink> CasePersonLinks { get; set; }
        public DbSet<CasePersonSentence> CasePersonSentences { get; set; }
        public DbSet<CasePersonSentenceLawbase> CasePersonSentenceLawbases { get; set; }
        public DbSet<CasePersonSentencePunishment> CasePersonSentencePunishment { get; set; }
        public DbSet<CasePersonSentencePunishmentCrime> CasePersonSentencePunishmentCrime { get; set; }
        public DbSet<CaseSelectionProtokol> CaseSelectionProtokols { get; set; }
        public DbSet<CaseSelectionProtokolCompartment> CaseSelectionProtokolCompartments { get; set; }
        public DbSet<CaseSelectionProtokolLawUnit> CaseSelectionProtokolLawUnits { get; set; }
        public DbSet<CaseSelectionProtokolLock> CaseSelectionProtokolLock { get; set; }

        public DbSet<CaseSession> CaseSessions { get; set; }
        public DbSet<CaseSessionH> CaseSessionsH { get; set; }

        public DbSet<CaseSessionFastDocument> CaseSessionFastDocuments { get; set; }
        public DbSet<CaseSessionFastDocumentH> CaseSessionFastDocumentsH { get; set; }

        public DbSet<CaseSessionAct> CaseSessionActs { get; set; }
        public DbSet<CaseSessionActH> CaseSessionActsH { get; set; }
        public DbSet<CaseSessionActComplain> CaseSessionActComplains { get; set; }
        public DbSet<CaseSessionActComplainPerson> CaseSessionActComplainPersons { get; set; }
        public DbSet<CaseSessionActComplainResult> CaseSessionActComplainResults { get; set; }
        public DbSet<CaseSessionActCoordination> CaseSessionActCoordination { get; set; }
        public DbSet<CaseSessionActLawBase> CaseSessionActLawBases { get; set; }

        public DbSet<CaseSessionDoc> CaseSessionDocs { get; set; }
        public DbSet<CaseSessionMeeting> CaseSessionMeeting { get; set; }
        public DbSet<CaseSessionMeetingUser> CaseSessionMeetingUser { get; set; }
        public DbSet<CaseSessionNotificationList> CaseSessionNotificationList { get; set; }
        public DbSet<CaseSessionResult> CaseSessionResults { get; set; }
        public DbSet<CaseArchive> CaseArchives { get; set; }
        public DbSet<CaseDeadline> CaseDeadline { get; set; }
        public DbSet<CaseSessionActDivorce> CaseSessionActDivorces { get; set; }
        public DbSet<CasePersonSentenceBulletin> CasePersonSentenceBulletins { get; set; }
        public DbSet<CaseSessionActCompany> CaseSessionActCompanys { get; set; }

        #endregion Case

        #region Common
        public DbSet<ID_List> ID_List { get; set; }
        public DbSet<AuditLog> AuditLog { get; set; }

        public DbSet<Address> Addresses { get; set; }        
        public DbSet<Counter> Counters { get; set; }
        public DbSet<CounterDocument> CounterDocuments { get; set; }
        public DbSet<CounterCase> CounterCases { get; set; }
        public DbSet<CounterSessionAct> CounterSessionActs { get; set; }
        public DbSet<Court> Courts { get; set; }
        public DbSet<CourtArchiveIndex> CourtArchiveIndexes { get; set; }
        public DbSet<CourtArchiveIndexCode> CourtArchiveIndexCodes { get; set; }
        public DbSet<CourtBankAccount> CourtBankAccounts { get; set; }
        public DbSet<CourtDepartment> CourtDepartments { get; set; }
        public DbSet<CourtDepartmentGroup> CourtDepartmentGroups { get; set; }
        public DbSet<CourtDepartmentLawUnit> CourtDepartmentLawUnits { get; set; }
        public DbSet<CourtDuty> CourtDuties { get; set; }
        public DbSet<CourtDutyLawUnit> CourtDutyLawUnits { get; set; }
        public DbSet<CourtGroup> CourtGroups { get; set; }
        public DbSet<CourtGroupCode> CourtGroupCodes { get; set; }
        public DbSet<CourtHall> CourtHalls { get; set; }
        public DbSet<CourtJuryFee> CourtJuryFee { get; set; }
        public DbSet<CourtLawUnit> CourtLawUnits { get; set; }
        public DbSet<CourtLawUnitSubstitution> CourtLawUnitSubstitutions { get; set; }
        public DbSet<CourtLawUnitOrder> CourtLawUnitOrders { get; set; }
        public DbSet<CourtLawUnitGroup> CourtLawUnitGroups { get; set; }
        public DbSet<CourtLawUnitActivity> CourtLawUnitActivities { get; set; }
        public DbSet<CourtLoadResetPeriod> CourtLoadResetPeriod { get; set; }
        public DbSet<CourtLoadPeriod> CourtLoadPeriods { get; set; }
        public DbSet<CourtLoadPeriodLawUnit> CourtLoadPeriodLawUnits { get; set; }
        public DbSet<CourtOrganization> CourtOrganization { get; set; }
        public DbSet<CourtOrganizationCaseGroup> CourtOrganizationCaseGroups { get; set; }
        public DbSet<CourtRegion> CourtRegion { get; set; }
        public DbSet<CourtRegionArea> CourtRegionArea { get; set; }
        public DbSet<EpepUser> EpepUser { get; set; }
        public DbSet<EpepUserAssignment> EpepUserAssignment { get; set; }
        public DbSet<ExcelReportTemplate> ExcelReportTemplate { get; set; }
        public DbSet<ExcelReportData> ExcelReportData { get; set; }
        public DbSet<ExcelReportCaseFilter> ExcelReportCaseFilter { get; set; }
        public DbSet<HtmlTemplate> HtmlTemplates { get; set; }
        public DbSet<HtmlTemplateLink> HtmlTemplateLink { get; set; }
        public DbSet<HtmlTemplateParamLink> HtmlTemplateParamLink { get; set; }
        public DbSet<Institution> Institutions { get; set; }
        public DbSet<InstitutionAddress> InstitutionAddresses { get; set; }
        public DbSet<IntegrationKey> IntegrationKeys { get; set; }
        public DbSet<LawUnit> LawUnits { get; set; }
        public DbSet<LawUnitH> LawUnitsH { get; set; }
        public DbSet<LawUnitAddress> LawUnitAddress { get; set; }
        public DbSet<LawUnitSpeciality> LawUnitSpecialities { get; set; }

        public DbSet<MigrationData> MigrationData { get; set; }
        public DbSet<MongoFile> MongoFile { get; set; }
        public DbSet<MQEpep> MQEpep { get; set; }
        public DbSet<LogOperation> LogOperation { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<PersonAddress> PersonAddresses { get; set; }
        public DbSet<PriceDesc> PriceDesc { get; set; }
        public DbSet<PriceCol> PriceCol { get; set; }
        public DbSet<PriceVal> PriceVal { get; set; }
        public DbSet<Report> Report { get; set; }
        public DbSet<ReportRequest> ReportRequest { get; set; }
        public DbSet<WorkingDay> WorkingDays { get; set; }
        public DbSet<WorkTask> WorkTasks { get; set; }
        public DbSet<CourtPosDevice> CourtPosDevices { get; set; }
        public DbSet<WorkNotification> WorkNotifications { get; set; }
        public DbSet<CourtArchiveCommittee> CourtArchiveCommittees { get; set; }
        public DbSet<CourtArchiveCommitteeLawUnit> CourtArchiveCommitteeLawUnits { get; set; }
        public DbSet<EMailMessage> EMailMessage { get; set; }
        public DbSet<EMailMessageState> EMailMessageState { get; set; }
        public DbSet<EMailFile> EMailFile { get; set; }
        public DbSet<BankAccount> BankAccount { get; set; }
        #endregion Common

        #region Delivery

        public DbSet<DeliveryArea> DeliveryArea { get; set; }
        public DbSet<DeliveryAreaAddress> DeliveryAreaAddress { get; set; }
        public DbSet<DeliveryItem> DeliveryItem { get; set; }
        public DbSet<DeliveryItemOper> DeliveryItemOper { get; set; }
        public DbSet<DeliveryOperState> DeliveryOperState { get; set; }
        public DbSet<DeliveryReason> DeliveryReason { get; set; }
        public DbSet<DeliveryStateReason> DeliveryStateReason { get; set; }
        public DbSet<DeliveryItemVisitMobile> DeliveryItemVisitMobile { get; set; }
        public DbSet<DeliveryAccount> DeliveryAccount { get; set; }
        public DbSet<DeliveryMobileFile> DeliveryMobileFile { get; set; }
        #endregion

        #region EKATTE

        public DbSet<EkArea> EkAreas { get; set; }
        public DbSet<EkCountry> EkCountries { get; set; }
        public DbSet<EkDistrict> EkDistricts { get; set; }
        public DbSet<EkEkatte> EkEkatte { get; set; }
        public DbSet<EkMunincipality> EkMunincipalities { get; set; }
        public DbSet<EkRegion> EkRegions { get; set; }
        public DbSet<EkSobr> EkSobrs { get; set; }
        public DbSet<EkStreet> EkStreets { get; set; }

        #endregion EKATTE

        #region Document

        public DbSet<Document> Documents { get; set; }

        public DbSet<DocumentCaseInfo> DocumentCaseInfos { get; set; }
        public DbSet<DocumentInstitutionCaseInfo> DocumentInstitutionCaseInfos { get; set; }

        public DbSet<DocumentLink> DocumentLinks { get; set; }
        public DbSet<DocumentPerson> DocumentPersons { get; set; }

        public DbSet<DocumentPersonAddress> DocumentPersonAddresses { get; set; }
        public DbSet<DocumentRegisterLink> DocumentRegisterLinks { get; set; }
        public DbSet<DocumentTemplate> DocumentTemplate { get; set; }
        public DbSet<DocumentDecision> DocumentDecisions { get; set; }
        public DbSet<DocumentDecisionCase> DocumentDecisionCases { get; set; }

        public DbSet<DocumentResolution> DocumentResolutions { get; set; }

        #endregion Document

        #region Money

        public DbSet<Obligation> Obligations { get; set; }
        public DbSet<ObligationPayment> ObligationPayments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PosPaymentResult> PosPaymentResults { get; set; }
        public DbSet<ExpenseOrder> ExpenseOrders { get; set; }
        public DbSet<ExpenseOrderObligation> ExpenseOrderObligations { get; set; }
        public DbSet<ObligationReceive> ObligationReceives { get; set; }
        public DbSet<ExecList> ExecLists { get; set; }
        public DbSet<ExecListObligation> ExecListObligations { get; set; }
        public DbSet<ExchangeDoc> ExchangeDocs { get; set; }
        public DbSet<ExchangeDocExecList> ExchangeDocExecLists { get; set; }


        #endregion

        #region Nomenclatures
        public DbSet<ActComplainIndex> ActComplainIndex { get; set; }
        public DbSet<ActComplainIndexCourtTypeCaseGroup> ActComplainIndexCourtTypeCaseGroup { get; set; }
        public DbSet<ActCoordinationType> ActCoordinationType { get; set; }
        public DbSet<ActMotiveState> ActMotiveState { get; set; }
        public DbSet<ActResult> ActResults { get; set; }
        public DbSet<ActResultGrouping> ActResultCaseInstances { get; set; }

        public DbSet<ActState> ActStates { get; set; }

        public DbSet<ActType> ActTypes { get; set; }

        public DbSet<ActTypeSessionTypeGroup> ActTypeSessionTypeGroup { get; set; }

        public DbSet<ActISPNDebtorState> ActISPNDebtorState { get; set; }
        public DbSet<ActISPNReason> ActISPNReasons { get; set; }
        public DbSet<ActKind> ActKinds { get; set; }
        public DbSet<ActTypeCourtLink> ActTypeCourtTypes { get; set; }

        public DbSet<AddressType> AddressTypes { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<CaseBankAccountType> CaseBankAccountTypes { get; set; }
        public DbSet<CaseCharacter> CaseCharacters { get; set; }
        public DbSet<CaseCodeGroup> CaseCodeGroups { get; set; }
        public DbSet<CaseCodeGrouping> CaseCodeGroupings { get; set; }

        public DbSet<CaseGroup> CaseGroups { get; set; }
        public DbSet<CaseInstance> CaseInstances { get; set; }
        public DbSet<CaseLoadAddActivity> CaseLoadAddActivity { get; set; }
        public DbSet<CaseLoadAddActivityIndex> CaseLoadAddActivityIndex { get; set; }
        public DbSet<CaseLoadElementGroup> CaseLoadElementGroup { get; set; }
        public DbSet<CaseLoadElementType> CaseLoadElementType { get; set; }
        public DbSet<CaseLoadElementTypeRule> CaseLoadElementTypeRule { get; set; }
        public DbSet<CaseLoadCorrectionActivity> CaseLoadCorrectionActivity { get; set; }
        public DbSet<CaseLoadCorrectionActivityIndex> CaseLoadCorrectionActivityIndex { get; set; }
        public DbSet<CaseMoneyClaimGroup> CaseMoneyClaimGroups { get; set; }
        public DbSet<CaseMoneyClaimType> CaseMoneyClaimTypes { get; set; }
        public DbSet<CaseMoneyCollectionGroup> CaseMoneyCollectionGroups { get; set; }
        public DbSet<CaseMoneyCollectionKind> CaseMoneyCollectionKinds { get; set; }
        public DbSet<CaseMoneyCollectionType> CaseMoneyCollectionTypes { get; set; }
        public DbSet<CaseMoneyExpenseType> CaseMoneyExpenseTypes { get; set; }
        public DbSet<CasePersonInheritanceResult> CasePersonInheritanceResults { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<CaseMigrationType> CaseMigrationType { get; set; }

        public DbSet<CaseState> CaseStates { get; set; }

        public DbSet<CaseType> CaseTypes { get; set; }
        public DbSet<CaseTypeCode> CaseTypeCode { get; set; }
        public DbSet<CaseTypeCharacter> CaseTypeCharacter { get; set; }
        public DbSet<CaseTypeUnit> CaseTypeUnit { get; set; }
        public DbSet<CaseTypeUnitCount> CaseTypeUnitCount { get; set; }
        public DbSet<CaseReason> CaseReason { get; set; }

        public DbSet<Classification> Classifications { get; set; }
        public DbSet<CodeMapping> CodeMapping { get; set; }
        public DbSet<ComplainType> ComplainTypes { get; set; }

        public DbSet<CounterResetType> CounterResetTypes { get; set; }
        public DbSet<CounterType> CounterTypes { get; set; }
        public DbSet<CourtType> CourtTypes { get; set; }
        public DbSet<CourtTypeCaseType> CourtTypeCaseType { get; set; }
        public DbSet<CourtTypeSessionType> CourtTypeSessionType { get; set; }

        public DbSet<DayType> DayTypes { get; set; }
        public DbSet<DeadlineGroup> DeadlineGroup { get; set; }
        public DbSet<DeadlineType> DeadlineType { get; set; }

        public DbSet<DeliveryDirectionGroup> DeliveryDirectionGroups { get; set; }
        public DbSet<DeliveryGroup> DeliveryGroups { get; set; }
        public DbSet<DeliveryNumberType> DeliveryNumberType { get; set; }
        public DbSet<DeliveryOper> DeliveryOper { get; set; }

        public DbSet<DeliveryType> DeliveryTypes { get; set; }
        public DbSet<DepartmentType> DepartmentTypes { get; set; }
        public DbSet<DismisalType> DismisalType { get; set; }
        public DbSet<DocumentDirection> DocumentDirections { get; set; }
        public DbSet<DocumentGroup> DocumentGroups { get; set; }
        public DbSet<DocumentKind> DocumentKinds { get; set; }
        public DbSet<DocumentRegister> DocumentRegisters { get; set; }
        public DbSet<DocumentRegisterType> DocumentRegisterTypes { get; set; }
        public DbSet<DocumentTemplateState> DocumentTemplateStates { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<DocumentTypeCaseType> DocumentTypeCaseTypes { get; set; }
        public DbSet<DocumentTypeCourtType> DocumentTypeCourtTypes { get; set; }
        public DbSet<DocumentTypeGrouping> DocumentTypeGroupings { get; set; }
        public DbSet<EisppTbl> EisppTbls { get; set; }
        public DbSet<EisppTblElement> EisppTblElements { get; set; }
        public DbSet<EpepUserType> EpepUserTypes { get; set; }

        public DbSet<EvidenceState> EvidenceState { get; set; }
        public DbSet<ExecListMoneyType> ExecListMoneyTypes { get; set; }

        public DbSet<ExpenceMoneyType> ExpenceMoneyTypes { get; set; }

        public DbSet<HtmlTemplateType> HtmlTemplateTypes { get; set; }
        public DbSet<HtmlTemplateParam> HtmlTemplateParam { get; set; }
        public DbSet<InstitutionCaseType> InstitutionCaseTypes { get; set; }
        public DbSet<InstitutionType> InstitutionTypes { get; set; }
        public DbSet<IntegrationState> IntegrationState { get; set; }
        public DbSet<IntegrationType> IntegrationType { get; set; }

        public DbSet<JudgeRole> JudgeRoles { get; set; }
        public DbSet<JudgeSeniority> JudgeSeniorities { get; set; }
        public DbSet<JudgeLoadActivity> JudgeLoadActivity { get; set; }
        public DbSet<JudgeLoadActivityIndex> JudgeLoadActivityIndex { get; set; }

        public DbSet<LawBase> LawBase { get; set; }

        public DbSet<LawUnitType> LawUnitTypes { get; set; }
        public DbSet<LawUnitPosition> LawUnitPosition { get; set; }
        public DbSet<LawUnitTypePosition> LawUnitTypePosition { get; set; }

        public DbSet<LifecycleType> LifecycleTypes { get; set; }

        public DbSet<LinkDirection> LinkDirections { get; set; }
        public DbSet<LoadGroup> LoadGroups { get; set; }
        public DbSet<LoadGroupLink> LoadGroupLinks { get; set; }
        public DbSet<LoadGroupLinkCode> LoadGroupLinkCodes { get; set; }
        public DbSet<MilitaryRang> MilitaryRangs { get; set; }
        public DbSet<MoneyCollectionEndDateType> MoneyCollectionEndDateTypes { get; set; }
        public DbSet<MoneyGroup> MoneyGroup { get; set; }
        public DbSet<MoneyType> MoneyTypes { get; set; }
        public DbSet<MoneyFeeType> MoneyFeeTypes { get; set; }
        public DbSet<MoneyFineType> MoneyFineTypes { get; set; }
        public DbSet<MoneyFeeDocumentGroup> MoneyFeeDocumentGroups { get; set; }
        public DbSet<ExecListType> ExecListTypes { get; set; }
        public DbSet<ExecListLawBase> ExecListLawBases { get; set; }

        public DbSet<MovementType> MovementType { get; set; }

        public DbSet<NotificationDeliveryGroup> NotificationDeliveryGroups { get; set; }
        public DbSet<NotificationDeliveryType> NotificationDeliveryTypes { get; set; }
        public DbSet<NotificationDeliveryGroupState> NotificationDeliveryGroupStates { get; set; }
        public DbSet<NotificationMode> NotificationModes { get; set; }

        public DbSet<NotificationState> NotificationStates { get; set; }

        public DbSet<NotificationType> NotificationTypes { get; set; }
        public DbSet<OrganizationLevel> OrganizationLevel { get; set; }

        public DbSet<PaymentType> PaymentTypes { get; set; }
        public DbSet<PeriodType> PeriodTypes { get; set; }
        public DbSet<PersonMaturity> PersonMaturities { get; set; }
        public DbSet<PersonRole> PersonRoles { get; set; }
        public DbSet<PersonRoleCaseType> PersonRoleCaseTypes { get; set; }
        public DbSet<PersonRoleInCrime> PersonRoleInCrime { get; set; }

        public DbSet<ProcessPriority> ProcessPriorities { get; set; }

        public DbSet<PunishmentActivity> PunishmentActivities { get; set; }


        public DbSet<RecidiveType> RecidiveTypes { get; set; }
        public DbSet<ResolutionType> ResolutionTypes { get; set; }
        public DbSet<ResolutionState> ResolutionStates { get; set; }

        public DbSet<RoleKind> RoleKinds { get; set; }

        public DbSet<SelectionLawUnitState> SelectionLawUnitStates { get; set; }
        public DbSet<SelectionProtokolState> SelectionProtokolState { get; set; }
        public DbSet<SelectionMode> SelectionModes { get; set; }
        public DbSet<SentenceExecPeriod> SentenceExecPeriods { get; set; }
        public DbSet<SentenceLawbase> SentenceLawbase { get; set; }
        public DbSet<SentenceRegimeType> SentenceRegimeTypes { get; set; }

        public DbSet<SentenceResultType> SentenceResultTypes { get; set; }

        public DbSet<SentenceType> SentenceTypes { get; set; }
        public DbSet<SessionActGroup> SessionActGroups { get; set; }
        public DbSet<SessionActType> SessionActTypes { get; set; }
        public DbSet<SessionDocState> SessionDocStates { get; set; }
        public DbSet<SessionDocType> SessionDocTypes { get; set; }
        public DbSet<SessionDuration> SessionDuration { get; set; }
        public DbSet<SessionMeetingType> SessionMeetingType { get; set; }

        public DbSet<SessionResult> SessionResults { get; set; }
        public DbSet<SessionResultBase> SessionResultBase { get; set; }
        public DbSet<SessionResultBaseGrouping> SessionResultBaseGroupings { get; set; }
        public DbSet<SessionResultFilterRule> SessionResultFilterRules { get; set; }

        public DbSet<SessionState> SessionState { get; set; }
        public DbSet<SessionType> SessionTypes { get; set; }
        public DbSet<SessionTypeState> SessionTypeStates { get; set; }

        public DbSet<Speciality> Specialities { get; set; }

        public DbSet<TaskAction> TaskActions { get; set; }
        public DbSet<TaskExecution> TaskExecution { get; set; }
        public DbSet<TaskType> TaskType { get; set; }
        public DbSet<TaskTypeSourceType> TaskTypeSourceType { get; set; }

        public DbSet<TaskState> TaskStates { get; set; }

        public DbSet<UicType> UicTypes { get; set; }
        public DbSet<RegixType> RegixTypes { get; set; }

        public DbSet<WorkNotificationType> WorkNotificationTypes { get; set; }
        public DbSet<DecisionType> DecisionTypes { get; set; }
        public DbSet<DocumentTypeDecisionType> DocumentTypeDecisionTypes { get; set; }
        public DbSet<DocumentDecisionState> DocumentDecisionStates { get; set; }
        public DbSet<SessionResultGrouping> SessionResultGroupings { get; set; }
        public DbSet<ActComplainResultGrouping> ActComplainResultGroupings { get; set; }
        public DbSet<ActResultGroup> ActResultGroups { get; set; }
        public DbSet<PersonRoleLinkDirection> PersonRoleLinkDirection { get; set; }
        public DbSet<CompanyType> CompanyTypes { get; set; }
        public DbSet<TableDescription> TableDescription { get; set; }
        public DbSet<PersonRoleGrouping> PersonRoleGroupings { get; set; }
        public DbSet<ExcelReportIndex> ExcelReportIndexs { get; set; }
        public DbSet<ExcelReportCaseCodeRow> ExcelReportCaseCodeRows { get; set; }
        public DbSet<ExcelReportComplainResult> ExcelReportComplainResults { get; set; }
        #endregion Nomenclatures

        #region Regix
        public DbSet<RegixReport> RegixReports { get; set; }
        public DbSet<RegixMapActualStateV3> RegixMapActualStateV3s { get; set; }
        #endregion

        #region EISPP
        public DbSet<EisppCaseCode> EisppCaseCode { get; set; }
        public DbSet<EisppRules> EisppRules { get; set; }
        public DbSet<EisppEventItem> EisppEventItem { get; set; }
        #endregion

        public DbSet<News> News { get; set; }

        public DbSet<NewsUser> NewsUsers { get; set; }
    }
}
