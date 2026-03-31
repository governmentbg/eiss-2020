using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseDeadlineService : IBaseService
    {
        void DeadLineOnSessionResult(CaseSessionResult sessionResult);
        void DeadLineOnCase(Case caseModel);
        void DeadLineOnSession(CaseSession session);
        IQueryable<CaseDeadLineVM> CaseDeadLineSelect(CaseDeadLineFilterVM filter);
        void DeadLineMotive(CaseSessionAct sessionAct);
        void DeadLineOpenSessionResult(CaseSessionMeetingUser user);
        void DeadLineCompanyCaseStartOnDocument(Document document);
        void DeadLineCompanyCaseByCaseId(int caseId);
        void DeadLineCompleteOnSessionAct(CaseSessionAct caseSessionAct);

        /// <summary>
        /// Приключване на срок за дело по чл. 410 ГПК или чл. 417 ГПК
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="sourceId">SourceId</param>
        /// <param name="sourceType">SourceType</param>
        /// <param name="dedlineTypeId">Идентификатор на тип на срок</param>
        /// <param name="isComplete">Флаг който показва дали е приключване или сторно на срока</param>
        /// <returns></returns>
        Task<bool> CompleteExpiredCaseDeadlineFastProcess(int caseId, long? sourceId, int sourceType, int dedlineTypeId, bool isComplete = true);

        /// <summary>
        /// Стартиране на срок за предприемане на действия по ново образувано дело по чл. 410 ГПК или чл. 417 ГПК
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        Task<bool> StartTakingActionFastProcess(int caseId);

        /// <summary>
        /// Стартиране на срок за липса на произнасяне по съпровождащ документ (или по частна жалба) N5
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        Task<bool> StartMissingActForCompliantDocumentFastProcess(long documentId);

        /// <summary>
        /// Приключване / сторно на срок за липса на произнасяне по съпровождащ документ (или по частна жалба) N5
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акт</param>
        /// <param name="isSaveChanges">Флаг дали да има SaveChanges</param>
        /// <param name="isComplete">Флаг за тип операция приключване/сторно</param>
        /// <returns></returns>
        Task<bool> CompleteExpiredMissingActForCompliantDocumentFastProcess(int caseSessionActId, bool isSaveChanges, bool isComplete = true);

        /// <summary>
        /// Стартиране на срок за невърнато съобщение по чл. 410 ГПК или чл. 417 ГПК
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        Task<bool> StartUnreturnedMessageFastProcess(int caseNotificationId);

        /// <summary>
        /// Приключване / сторно на срок за невърнато съобщение по чл. 410 ГПК или чл. 417 ГПК
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="isSaveChanges">Флаг дали да има SaveChanges</param>
        /// <param name="isComplete">Флаг за тип операция приключване/сторно</param>
        /// <returns></returns>
        Task<bool> CompleteExpiredUnreturnedMessageFastProcess(int caseNotificationId, bool isSaveChanges, bool isComplete = true);
    }
}
