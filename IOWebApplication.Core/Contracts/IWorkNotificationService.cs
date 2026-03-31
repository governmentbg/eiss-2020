using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IWorkNotificationService : IBaseService
    {
        List<SelectListItem> GetDDL_WorkNotificationTypes(int sourceType);

        /// <summary>
        /// Метод връщащ идентификатор на юзер на лице
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSesionId">Идентификатор на заседание</param>
        /// <returns></returns>
        string GetJudgeUserId(int caseId, int? caseSesionId = null);

        /// <summary>
        /// Метод връщащ идентификатор на юзер на лице
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSesionId">Идентификатор на заседание</param>
        /// <returns></returns>
        Task<string> GetJudgeUserIdAsync(int caseId, int? caseSesionId = null);

        WorkNotificationFilterVM MakeDefaultFilter();
        Task<WorkNotification> NewWorkNotification(int notificationId, int notificationStateId);


        /// <summary>
        /// Метод за запис на срок и нотификация
        /// </summary>
        /// <param name="caseDeadline">Попълнен обект от тип CaseDeadline</param>
        /// <returns></returns>
        Task<List<WorkNotification>> NewWorkNotification(CaseDeadline caseDeadline);

        WorkNotification NewWorkNotification(CaseLawUnit model);
        WorkNotification NewWorkNotification(CaseLawyerHelpAssignedLawyer model);
        List<WorkNotification> NewWorkNotificationSecretary(CaseDeadline caseDeadline);
        List<SelectListItem> ReadTypeId_SelectDDL();
        bool SaveWorkNotification(WorkNotification workNotification);
        WorkNotification SaveWorkNotificationRead(long id);
        WorkNotification SaveWorkNotificationReadAll(long id);

        /// <summary>
        /// Метод извличащ данни за нотификации
        /// </summary>
        /// <param name="filterData">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<WorkNotificationListVM> SelectWorkNotifications(WorkNotificationFilterVM filterData);

        /// <summary>
        /// Метод променящ дата на визуализация на нотификацията
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<bool> EditDateCreatedWNFastProcess(WorkNotificationEditDateVM model);

        /// <summary>
        /// Създаване на нотификация за новообразувано дело по чл. 410 ГПК или чл. 417 ГПК
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForN1(int caseId);

        /// <summary>
        /// Гасене на нотификация за новообразувано дело по чл. 410 ГПК или чл. 417 ГПК
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акт</param>
        /// <returns></returns>
        Task<bool> TurnOffForN1(int caseSessionActId);

        /// <summary>
        /// Създаване на нотификация за липса на предприети действия
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <param name="caseSessionResultId">Идентификатор на резултат</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForN3(int? caseSessionActId, int? caseSessionResultId = null);

        /// <summary>
        /// Запис на нотификация за липса на предприети действия
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForN3(int caseSessionActId);

        /// <summary>
        /// Запис на нотификация за липса на предприети действия от резултат
        /// </summary>
        /// <param name="caseSessionResultId">Идентификатор на резултат</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForN3Result(int caseSessionResultId);

        /// <summary>
        /// Гасене на нотификация за липса на предприети действия
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на акт</param>
        /// <param name="saveChanges"></param>
        /// <returns></returns>
        Task<bool> TurnOfNotificationsForN3(int caseNotificationId, bool saveChanges = true);

        /// <summary>
        /// Създаване на нотификация за постъпване на съпровождащ документ
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForCompliantDocumentCaseFastProcess(long documentId);

        /// <summary>
        /// Запис на нотификация за постъпване на съпровождащ документ
        /// </summary>
        /// <param name="documentId">Идентификатор на акта</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForCompliantDocumentCaseFastProcess(long documentId);

        /// <summary>
        /// Метод коригиращ нотификация за постъпване на съпровождащ документ
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        Task<bool> ExpiredEditNotificationsForCompliantDocumentCaseFastProcess(long documentId);

        /// <summary>
        /// Метод за гасена на нотификация за постъпване на съпровождащ документ
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акт</param>
        /// <param name="isSaveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<bool> TurnOfCompliantDocumentCaseFastProcess(int caseSessionActId, bool isSaveChanges);

        /// <summary>
        /// Създаване на нотификация N23
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForN23(int caseSessionActId);

        /// <summary>
        /// Запис на нотификация N23
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForN23(int caseSessionActId);

        /// <summary>
        /// Редактиране на нотификация N23
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акт</param>
        /// <returns></returns>
        Task<string> SaveUpdateNotificationsForN23(int caseSessionActId, bool saveChanges = true);

        /// <summary>
        /// Създаване на нотификация за предприемане на действия от съдебен служител след подписване на писмо/удостоверение N24
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForN24(long documentId);

        /// <summary>
        /// Запис на нотификация за предприемане на действия от съдебен служител след подписване на писмо/удостоверение N24
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForN24(long documentId);

        /// <summary>
        /// Гасене на нотификация за предприемане на действия от съдебен служител след подписване на писмо/удостоверение N24
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<bool> TurnOfNotificationsForN24(long documentId, bool saveChanges = true);

        /// <summary>
        /// Създаване на нотификация за върнато съобщение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForMessageDeliveredCaseFastProcess(int caseNotificationId, DateTime dateTimeEvent);

        /// <summary>
        /// Запис на нотификация за върнато съобщение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForMessageDeliveredCaseFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges);

        /// <summary>
        /// Редакция на дата на стартиране на нотификация за връчено съобщение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<bool> EditDateEventNotificationsForMessageDeliveredCaseFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges = true);

        /// <summary>
        /// Сторно на нотификация за върнато съобщение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        Task<bool> ExpiredNotificationsForMessageDeliveredCaseFastProcess(int caseNotificationId);

        /// <summary>
        /// Създаване на нотификация за залепено уведомление
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForStuckMessagesFastProcess(int caseNotificationId, DateTime dateTimeEvent);

        /// <summary>
        /// Запис на нотификация за залепено уведомление
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForStuckMessagesFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges);

        /// <summary>
        /// Сторно на нотификация за залепено уведомление
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        Task<bool> ExpiredNotificationsForStuckMessagesFastProcess(int caseNotificationId);

        /// <summary>
        /// Създаване на нотификация за липса на подадено в срок възражение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForLackSubmittedObjectionFastProcess(int caseNotificationId, DateTime dateTimeEvent);

        /// <summary>
        /// Запис на нотификация за липса на подадено в срок възражение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForLackSubmittedObjectionFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges);

        /// <summary>
        /// Редакция на дата на стартиране на нотификация за липса на подадено в срок възражение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<bool> EditDateEventNotificationsForLackSubmittedObjectionFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges = true);

        /// <summary>
        /// Сторно на нотификация за липса на подадено в срок възражение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        Task<bool> ExpiredNotificationsForLackSubmittedObjectionFastProcess(int caseNotificationId);

        /// <summary>
        /// Гасене на нотификация за липса на подадено в срок възражение
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        Task<bool> TurnOffNotificationsForLackSubmittedObjectionFastProcess(int caseId);

        /// <summary>
        /// Гасене на нотификация за липса на подадено в срок възражение от документ с основен вид възражение
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<bool> TurnOffNotificationsObjectionForLackSubmittedObjectionFastProcess(long documentId, bool saveChanges = true);

        /// <summary>
        /// Създаване на нотификация за обжалване на акт
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForAppealActFastProcess(int caseNotificationId, DateTime dateTimeEvent);

        /// <summary>
        /// Запис на нотификация за обжалване на акт
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForAppealActFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges);

        /// <summary>
        /// Сторно на нотификация за обжалване на акт
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        Task<bool> ExpiredNotificationsForAppealActFastProcess(int caseNotificationId);

        

        /// <summary>
        /// Създаване на нотификация за известяване за влязъл в сила акт от друга инстанция
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForActInforcedAnotherInstanceFastProcess(int caseSessionActId);

        /// <summary>
        /// Запис на нотификация за известяване за влязъл в сила акт от друга инстанция
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForActInforcedAnotherInstanceFastProcess(int caseSessionActId);

        /// <summary>
        /// Създаване на нотификация за постановен влязъл в сила финализиращ акт от друга инстанция /при обжалване на акт на заповедния съд/въззивни производства по чл. 413, 419, 420 и 423 от ГПК
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForN11(int caseSessionActId);

        /// <summary>
        /// Запис на нотификация за постановен влязъл в сила финализиращ акт от друга инстанция /при обжалване на акт на заповедния съд/въззивни производства по чл. 413, 419, 420 и 423 от ГПК
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForN11(int caseSessionActId);

        /// <summary>
        /// Създаване на нотификация за образуване на свързано дело на горна инстанция
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForNewCaseHigherInstanceWithout0604_1_2FastProcess(int caseId);

        /// <summary>
        /// Запис на нотификация за образуване на свързано дело на горна инстанция
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForNewCaseHigherInstanceWithout0604_1_2FastProcess(int caseId);

        /// <summary>
        /// Създаване на нотификация за образувано исково дело по чл. 422 ГПК свързано с дело по чл. 410 ГПК или чл. 417 ГПК
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForNewCaseHigherInstanceWith0604_1_2FastProcess(int caseId);

        /// <summary>
        /// Запис на нотификация за образувано исково дело по чл. 422 ГПК свързано с дело по чл. 410 ГПК или чл. 417 ГПК
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForNewCaseHigherInstanceWith0604_1_2FastProcess(int caseId);

        /// <summary>
        /// Създаване на нотификация за обжалване на акт
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForExpressingOpinionObjectionFastProcess(int caseNotificationId, DateTime dateTimeEvent);

        /// <summary>
        /// Запис на нотификация за обжалване на акт
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForExpressingOpinionObjectionFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges);

        /// <summary>
        /// Сторно на нотификация за обжалване на акт
        /// </summary>
        /// <param name="documentId">Идентификатор на </param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<bool> ExpiredNotificationsForExpressingOpinionObjectionFastProcess(long documentId, bool saveChanges = true);

        /// <summary>
        /// Редакция на дата на стартиране на нотификация за изтекъл срок за изразяване на становище по възражение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<bool> EditDateEventNotificationsForExpressingOpinionObjectionFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges = true);

        /// <summary>
        /// Създаване на нотификация за изтекъл срок за предявяване на иск по чл. 422 ГПК
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForFilingClaimFastProcess(int caseNotificationId, DateTime dateTimeEvent);

        /// <summary>
        /// Запис на нотификация за изтекъл срок за предявяване на иск по чл. 422 ГПК
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForFilingClaimFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges);

        /// <summary>
        /// Сторно на нотификация за изтекъл срок за предявяване на иск по чл. 422 ГПК
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<bool> ExpiredNotificationsForFilingClaimFastProcess(int caseId, bool saveChanges = true);

        /// <summary>
        /// Гасене на нотификация за изтекъл срок за предявяване на иск по чл. 422 ГПК
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<bool> TurnOffNotificationsForFilingClaimFastProcess(int caseId, bool saveChanges = true);

        /// <summary>
        /// Редакция на дата на стартиране на нотификация за изтекъл срок за предявяване на иск по чл. 422 ГПК
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<bool> EditDateEventNotificationsForFilingClaimFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges = true);

        /// <summary>
        /// Създаване на нотификация за постановен акт за отвод/самоотвод
        /// </summary>
        /// <param name="actId">Идентификатор на акт</param>
        /// <param name="sessionResultId">Идентификатор на резултат от заседание</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForDecreeRecusalSelfRecusalFastProcess(int? actId, int? sessionResultId);

        /// <summary>
        /// Запис на нотификация за постановен акт за отвод/самоотвод
        /// </summary>
        /// <param name="actId">Идентификатор на акта</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForDecreeRecusalSelfRecusalFastProcessByActId(int actId, bool saveChanges = true);

        /// <summary>
        /// Запис на нотификация за постановен акт за отвод/самоотвод
        /// </summary>
        /// <param name="sessionResultId">Идентификатор на резултат от заседание</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForDecreeRecusalSelfRecusalFastProcessBySessionResultId(int sessionResultId, bool saveChanges = true);

        /// <summary>
        /// Създаване на нотификация за получено съобщение за връчване
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForReceivedMessageDeliveryFastProcess(int caseNotificationId, DateTime dateTimeEvent);

        /// <summary>
        /// Запис на нотификация за получено съобщение за връчване
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForReceivedMessageDeliveryFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges);

        /// <summary>
        /// Сторно на нотификация за получено съобщение за връчване
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        Task<bool> ExpiredNotificationsForReceivedMessageDeliveryFastProcess(int caseNotificationId);

        /// <summary>
        /// Създаване на нотификация за връчен изпълнителен лист от съдебен изпълнител
        /// </summary>
        /// <param name="sourceType">Тип на обекта</param>
        /// <param name="sourceId">Идентификатор на обекта</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="description">Описание</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForDeliveredЕxecutiveListFastProcess(int sourceType, long sourceId, DateTime dateTimeEvent, string description = null);

        /// <summary>
        /// Запис на нотификация за връчен изпълнителен лист от съдебен изпълнител
        /// </summary>
        /// <param name="sourceType">Тип на обекта</param>
        /// <param name="sourceId">Идентификатор на обекта</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="description">Описание</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForDeliveredЕxecutiveListFastProcess(int sourceType, long sourceId, DateTime dateTimeEvent, string description = null, bool saveChanges = true);

        /// <summary>
        /// Създаване на нотификация за не връчено съобщение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotDeliveredMessageDeliveryFastProcess(int caseNotificationId, DateTime dateTimeEvent);

        /// <summary>
        /// Запис на нотификация за невръчено съобщение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForNotDeliveredMessageDeliveryFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges);

        /// <summary>
        /// Сторно на нотификация за невръчено съобщение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението</param>
        /// <returns></returns>
        Task<bool> ExpiredNotificationsForNotDeliveredMessageDeliveryFastProcess(int caseNotificationId);

        /// <summary>
        /// Създаване/редактиране на нотификация за липса на предприето процесуално действие от страна
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акт</param>
        /// <returns></returns>
        Task<string> SaveUpdateNotificationsForNoProceduralActionTakenFastProcess(int caseSessionActId, bool saveChanges = true);

        /// <summary>
        /// Редакция на дата на стартиране на нотификация за липса на предприето процесуално действие от страна
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<bool> EditDateEventNotificationsForNoProceduralActionTakenFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges = true);

        /// <summary>
        /// Създаване на нотификация за известяване за получено съобщение за връчване
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForNoticeServiceReceivedFastProcess(int caseNotificationId, DateTime dateTimeEvent);

        /// <summary>
        /// Запис на нотификация за известяване за получено съобщение за връчване
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForNoticeServiceReceivedFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges);

        /// <summary>
        /// Сторно на нотификация за известяване за получено съобщение за връчване
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        Task<bool> ExpiredNotificationsForNoticeServiceReceivedFastProcess(int caseNotificationId);

        /// <summary>
        /// Създаване на нотификация за предприемане на действия от съдебен служител, при постановяване на съдебен акт
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <returns></returns>
        Task<List<WorkNotification>> GetNotificationsForActionTakenCourtOfficerDeclatActFastProcess(int caseSessionActId);

        /// <summary>
        /// Запис на нотификация за предприемане на действия от съдебен служител, при постановяване на съдебен акт
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <returns></returns>
        Task<string> SaveNotificationsForActionTakenCourtOfficerDeclatActFastProcess(int caseSessionActId);

        /// <summary>
        /// Гасене на нотификация за предприемане на действия от съдебен служител, при постановяване на съдебен акт
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на нотификация</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<bool> TurnOfNotificationsForActionTakenCourtOfficerDeclatActFastProcess(int caseNotificationId, bool saveChanges = true);

        /// <summary>
        /// Запис на нотификация за обжалване на акт
        /// </summary>
        /// <param name="caseSessionAct">Акт</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task SaveNotificationsForAppealActFastProcessOnActSave(CaseSessionAct caseSessionAct, bool saveChanges);

        /// <summary>
        /// Генериране на нотификация при добавяне на служител
        /// </summary>
        /// <param name="caseLawUnitId">Идентификатор на служител</param>
        /// <returns></returns>
        Task<bool> CreateNotificationsAddCaseLawUnit(int caseLawUnitId);

        /// <summary>
        /// Гасене на нотификации за заместващ съдия по дело при промяна на дата до в заместването
        /// </summary>
        /// <param name="judgeReporterId">Идентификатор на съдия</param>
        /// <param name="substituteJudgeReporterId">Идентификатор на заместващ съдия</param>
        /// <param name="dateTo">Дата до на заместването</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        Task<bool> TurnOffNotificationForSubstituteJudgeReporter(int judgeReporterId, int substituteJudgeReporterId, DateTime dateTo, bool saveChanges = true);
    }
}
