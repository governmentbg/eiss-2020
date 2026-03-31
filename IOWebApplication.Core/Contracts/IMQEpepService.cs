using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Epep;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IOWebApplication.Infrastructure.Constants.EpepConstants;

namespace IOWebApplication.Core.Contracts
{
    public interface IMQEpepService : IBaseService
    {
        #region ЕПЕП


        Task ResendDataToEPEP(int sourceType, long sourceId, bool appendChild);
        Task<string> RecoverData(object client);

        Task<bool> AppendDocument(Document model, ServiceMethod method);
        bool AppendFile(CdnUploadRequest model, ServiceMethod method);
        Task<bool> AppendCase(Case model, ServiceMethod method);
        Task<bool> AppendCasePerson(CasePerson model, ServiceMethod method);
        bool AppendCaseSelectionProtocol(CaseSelectionProtokol model, ServiceMethod method, string assignorName = null);
        bool AppendJudgeReporter(int caseLawunitId, ServiceMethod method);
        bool AppendCaseSession(CaseSession model, ServiceMethod method);
        Task<bool> AppendCaseSessionAct(CaseSessionAct model, ServiceMethod method);
        Task<bool> AppendCaseSessionAct_Private(int actId, ServiceMethod method);
        Task<bool> AppendCaseSessionAct_Public(int actId, ServiceMethod method);
        bool AppendCaseSessionAct_PrivateMotive(int actId, ServiceMethod method);
        bool AppendCaseSessionAct_PublicMotive(int actId, ServiceMethod method);
        /// <summary>
        /// Подава всички подписани актове по време на заседанието на услугата за интеграция: ЕПЕП, ЦУБИПСА, ЕПРО
        /// </summary>
        /// <param name="sessionId">Id на заседание</param>
        Task<bool> AppendActsFromSession(int sessionId);
        bool AppendCaseSessionLawUnit(CaseLawUnit model, ServiceMethod method);
        bool AppendCaseNotification(CaseNotification model, EpepSummonInfoVM notificationInfo, ServiceMethod method);
        bool AppendCaseNotificationFile(int caseNotificationId);
        bool AppendEpepUserAssignment(EpepUserAssignment model, ServiceMethod method);

        IQueryable<EpepUserVM> EpepUser_Select(EpepUserFilterVM filter);
        bool EpepUser_SaveData(EpepUser model);
        string EpepUser_Validate(EpepUser model);

        EpepUser EpepUser_GetByDocument(long documentId);
        EpepUser EpepUser_InitFromDocument(long? documentId);
        EpepDocumentInfoVM EpepUser_DocumentInfo(long? documentId);

        IQueryable<EpepUserAssignmentVM> EpepUserAssignment_Select(int epepUserId);
        bool EpepUserAssignment_SaveData(EpepUserAssignment model);
        string EpepUserAssignment_Validate(EpepUserAssignment model);

        IntegrationKey IntegrationKey_GetByOuterKey(int integrationType, string key);

        List<IntegrationKey> IntegrationKey_SelectToCorrect(int sourceType);
        bool IntegrationKey_Correct(IntegrationKey model, bool withError);

        Task MQEpep_ResetError(int integrationType, int sourceType, long sourceId);
        Task<List<MQEpepVM>> MQEpep_Select(int integrationType, int sourceType, long sourceId);

        #endregion

        #region ЦУБИПСА
        bool LegalActs_SendAct(int actId, ServiceMethod method);
        #endregion

        void InitMQ(int integrationTypeId, int sourceType, long sourceId, EpepConstants.ServiceMethod method, long? parentSourceId = null, object model = null);
        long InitMQFromString(int integrationTypeId, int sourceType, long sourceId, EpepConstants.ServiceMethod method, long? parentSourceId, string message);
        bool ISPN_CaseSessionResult(int resultId, ServiceMethod method, long? caseId);
        bool ISPN_CaseSessionActComplain(int actComplainId, ServiceMethod method, long? caseId);
        bool ISPN_IsISPN(Case _case, int caseId);

        LawUnit GetLawyerByNumber(string lawyerNumber);

        #region ЕПРО

        /// <summary>
        /// Добавя заявка за регистриране на отвод в ЕПРО
        /// </summary>
        /// <param name="caseDismissalId"></param>
        /// <param name="dismissalTypeId"></param>
        void EPRO_AppendDismissal(int caseDismissalId, int dismissalTypeId);

        /// <summary>
        /// Добавя заявка за нов съдия към отвод
        /// </summary>
        /// <param name="caseSelectionProtocolId"></param>
        /// <param name="caseDismissalId"></param>
        void EPRO_AppendReplace(int caseSelectionProtocolId, int caseDismissalId);

        /// <summary>
        /// Добавя заявка за обезличен акт към отвод
        /// </summary>
        /// <param name="actModel"></param>
        void EPRO_AppendActFile(CaseSessionAct actModel);
        bool ISPN_CaseSession(int sessionId, ServiceMethod method, long? caseId);
        bool ISPN_Case(int caseId, ServiceMethod method);
        void EESPP_AppendLawyerAssignment(int lawyerAssignmentId);
        void EESPP_AppendLawyerAssignmentByAct(int caseSessionActId);
        void EESPP_LawyerHelp(ServiceMethod method, int lawyerHelpId);
        string NotMappedActs(int caseId);
        IQueryable<EpepUserAssignmentVM> EpepUserAssignments_SelectByCase(int caseId);
        bool AppendAttachedDocument(int sourceType, int sourceId, long parentId, ServiceMethod method);
        bool AppendCaseSessionFastDocument(CaseSessionFastDocument model, ServiceMethod method);
        void Set_AUTOSAVECHANGES(bool value);
        bool AppendCaseMigration(CaseMigration caseMigration);
        bool AppendCaseDataChange(int caseId);
        bool AppendCaseMigrationFull(int caseMigrationId);
        Task<SummaryCaseInfoVM> LoadConnectedCase(int inMigrationId, bool refreshData = false);
        SaveResultVM MqRestartCase(int caseId);
        Task AppendISPNLetter(int caseSessionActId, int documentTypeId);
        bool CheckOutDocumentForSend(long documentId);
        void CAIS_SendBulletin(ServiceMethod method, int bulletinFileId, int bulletinId);
        Task EpepDocument_SendForAssignment(long documentId);
        Task<bool> AppendExecProcess(int caseSessionActId, int execListId);
        Task<bool> AppendExecList(ExecList model, ServiceMethod method);
        Task<ExecProcessInfoVM> LoadExecProcess(int caseSessionActId, int execListId, bool refreshData = false);
        Task<SaveResultVM> DeleteExecProcessAccess(int caseSessionActId, int execListId, Guid accessGid);
        Task<SaveResultVM> AppendExecProcessAccess(int caseSessionActId, int execListId, Guid execProcessGid);
        Task<ExecAccessDocumentVM> GetAccessDocument(int caseSessionActId, int execListId, Guid accessGid);
        Task<bool> CheckForSavedDocumentRequestForCase(int caseSessionActId);
        bool AppendCaseNotificationSummonReport(int caseNotificationId);
        IQueryable<EpepUserVM> EpepUser_SelectByCasePerson(int casePersonId, bool forSummonOnly = true);
        bool AppendCaseSelectionSubstitution(int caseSelectionSubstitutionId);

        /// <summary>
        /// Стартира задачи за последваща обработка
        /// </summary>
        /// <param name="processType"></param>
        /// <param name="sourceType"></param>
        /// <param name="sourceId"></param>
        /// <param name="processContext"></param>
        /// <returns></returns>
        Task EissProcessStart(string processType, int sourceType, long sourceId, object processContext = null);
        Task RNFL_SendAppeal(int actComplainId, ServiceMethod method = ServiceMethod.Add);
        Task<bool> RNFL_SendAct(int actId, int caseId, ServiceMethod method = ServiceMethod.Add);
        Task AppendAutomaticEpepAccessForPerson(CasePerson personModel);
        void Set_MqID(string value);
        Task RNFL_SendSummon(int caseNotificationId, ServiceMethod method = ServiceMethod.Add);
        Task<(bool isRNFL, bool transferStarted)> RNFL_CheckCase(int caseId);
        Task<FileSignerInfoVM> GetEpepUserInfo(CdnDownloadResult fileInfo);
        Task RNFL_SendSide(int casePersonId, int caseId, ServiceMethod method = ServiceMethod.Add, bool checkCase = true);

        #endregion

    }
}
