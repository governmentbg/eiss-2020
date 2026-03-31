using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using IOWebApplication.Infrastructure.Models.ViewModels.Report.ReportWorkJudicialMediationCenters;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IReportService : IBaseService
    {
        IQueryable<PaymentPosReportVM> PaymentPosReport_Select(int courtId, PaymentPosFilterReportVM model);
        IQueryable<FineReportVM> FineReport_Select(int courtId, FineFilterReportVM model);
        IQueryable<StateFeeReportVM> StateFeeReport_Select(int courtId, StateFeeFilterReportVM model);
        IQueryable<ObligationJuryReportVM> ObligationJuryReport_Select(int courtId, ObligationJuryFilterReportVM model);

        /// <summary>
        /// Метод извличащ ексел за азбучника
        /// </summary>
        /// <param name="courtId">Съд</param>
        /// <param name="model">Филтър попълнен от потребител</param>
        /// <returns></returns>
        Task<byte[]> CaseAlphabetical_ToExcel(int courtId, CaseAlphabeticalFilterVM model);

        Task<byte[]> ZzdnReportToExcelOne(ZzdnFilterReportVM model);
        Task<byte[]> EuropeanHeritageReportToExcelOne(EuropeanHeritageFilterReportVM model);
        Task<byte[]> InsolvencyReportToExcelOne(InsolvencyFilterReportVM model);
        Task<byte[]> PublicInformationReportToExcelOne(PublicInformationFilterReportVM model);
        Task<byte[]> PublicInformationReportToExcelOnePrev(PublicInformationFilterReportVM model);
        Task<byte[]> CaseDecisionReportToExcelOne(CaseDecisionFilterReportVM model, string url);
        Task<byte[]> HeritageReportToExcelOne(HeritageFilterReportVM model);
        Task<byte[]> CaseFirstInstanceReportToExcelOne(CaseFirstInstanceFilterReportVM model);
        Task<byte[]> CaseMigrationReturnReportToExcelOne(CaseMigrationReturnFilterReportVM model);
        Task<byte[]> CaseMigrationReturnReportToExcelOnePrev(CaseMigrationReturnFilterReportVM model);
        Task<byte[]> CaseArchiveReportToExcelOne(CaseArchiveFilterReportVM model);
        Task<byte[]> CaseArchiveReportToExcelOnePrev(CaseArchiveFilterReportVM model);
        Task<byte[]> DivorceReportToExcelOne(DivorceFilterReportVM model);
        Task<byte[]> CaseSecondInstanceReportToExcelOne(CaseSecondInstanceFilterReportVM model);
        Task<byte[]> SentenceReportToExcelOne(SentenceFilterReportVM model);
        Task<byte[]> DocumentOutGoingReportToExcelOnePrev(DocumentOutFilterReportVM model);
        Task<byte[]> DocumentOutGoingReportToExcelOne(DocumentOutFilterReportVM model);

        Task<byte[]> DocumentInGoingReportToExcelOnePrev(DocumentInFilterReportVM model);
        Task<byte[]> DeliveryBookReportToExcelOne(DeliveryBookFilterVM model);
        Task<byte[]> DismisalReportToExcelOne(DismisalReportFilterVM model);
        byte[] CaseObligationReportToExcelOne(CaseObligationFilterReportVM model);
        Task<byte[]> ExecListReportToExcelOne(ExecListFilterReportVM model);
        Task<byte[]> ExecListReportToExcelOnePrev(ExecListFilterReportVM model);
        Task<byte[]> CaseArchiveListReportToExcelOne(CaseArchiveListFilterReportVM model);
        Task<byte[]> DocumentOutListReportToExcelOne(DocumentOutListFilterReportVM model);
        Task<byte[]> PosDeviceReportToExcelOne(PosDeviceFilterReportVM model);
        Task<byte[]> CaseSessionPrivateReportToExcelOneTemplate(CaseSessionPrivateFilterReportVM model);
        Task<byte[]> CaseSessionPublicReportToExcelOne(CaseSessionPublicFilterReportVM model);
        Task<byte[]> FineReportToExcelOne(FineFilterReportVM model);
        Task<byte[]> StateFeeReportExportExcel(StateFeeFilterReportVM model);
        Task<byte[]> PaymentPosReportToExcelOne(PaymentPosFilterReportVM model);
        byte[] CourtStatsReport(DateTime? date);
        byte[] CourtReportGeneric();
        IQueryable<CaseArchiveListReportVM> CaseArchiveListReport_Select(int courtId, CaseArchiveListFilterReportVM model);
        IQueryable<DocumentOutListReportVM> DocumentOutListReport_Select(int courtId, DocumentOutListFilterReportVM model, string newLine);
        IQueryable<PosDeviceReportVM> PosDeviceReport_Select(int courtId, PosDeviceFilterReportVM model);
        byte[] ObligationJuryReportToExcelOne(ObligationJuryFilterReportVM model);
        IQueryable<CaseLinkReportVM> CaseLinkReport_Select(int courtId, CaseLinkFilterReportVM model, string newLine);
        byte[] CaseLinkReportExportExcel(CaseLinkFilterReportVM model);
        List<TableDescription> TableDescription_Select();
        IQueryable<CaseDecisionReportVM> CaseDecisionReport_Select(int courtId, CaseDecisionFilterReportVM model);

        /// <summary>
        /// Справка влезли в сила присъди
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <param name="newLine">Знак за нов ред</param>
        /// <returns></returns>
        IQueryable<SentenceListReportVM> SentenceListReport_Select(SentenceListFilterReportVM filter, string newLine);

        Task<byte[]> SentenceListReportExportExcel(SentenceListFilterReportVM model);

        /// <summary>
        /// Метод извличащ данни за актове подлежащи на обезличаване
        /// </summary>
        /// <param name="model">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<SessionActForDepersonalizeReportVM> SessionActForDepersonalizeReport_Select(SessionActForDepersonalizeFilterReportVM model);

        /// <summary>
        /// Справка Съдени и осъдени лица
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <param name="newLine">Знак за нов ред</param>
        /// <returns></returns>
        IQueryable<CasePersonDefendantListReportVM> CasePersonDefendantListReport_Select(CasePersonDefendantListFilterReportVM filter, string newLine);

        Task<byte[]> CasePersonDefendantListReportExportExcel(CasePersonDefendantListFilterReportVM model);

        /// <summary>
        /// Справка постъпили дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<CaseFirstInstanceListReportVM> CaseFirstInstanceListReport_Select(CaseFirstInstanceListFilterReportVM filter);

        /// <summary>
        /// Експорт Справка постъпили дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<byte[]> CaseFirstInstanceListReportExportExcel(CaseFirstInstanceListFilterReportVM model);

        Task<byte[]> CaseSecondInstanceListReportExportExcel(CaseSecondInstanceListFilterReportVM model);

        /// <summary>
        /// Експорт Справка постъпили дела за период – първоинстанционни дела - със съд
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<byte[]> CaseFirstInstanceWithCourtListReportExportExcel(CaseFirstInstanceListFilterReportVM model);

        /// <summary>
        /// Справка Постъпили дела за период – въззивни дела
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<CaseSecondInstanceListReportVM> CaseSecondInstanceListReport_Select(CaseSecondInstanceListFilterReportVM filter);

        /// <summary>
        /// Справка Свършени дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <param name="newLine"></param>
        /// <returns></returns>
        IQueryable<CaseFinishListReportVM> CaseFinishFirstInstanceListReport_Select(CaseFinishListFilterReportVM filter, string newLine);

        /// <summary>
        /// Експорт Справка Свършени дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<byte[]> CaseFinishFirstInstanceListReportExportExcel(CaseFinishListFilterReportVM model);

        /// <summary>
        /// Справка свършени дела за период – въззивни/касационни дела
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <param name="newLine">Символ за нов ред</param>
        /// <returns></returns>
        IQueryable<CaseFinishListReportVM> CaseFinishSecondInstanceListReport_Select(CaseFinishListFilterReportVM filter, string newLine);

        /// <summary>
        /// Справка свършени дела за период – въззивни/касационни дела в ексел
        /// </summary>
        /// <param name="model">Филтър попълнен от потребител</param>
        /// <returns></returns>
        Task<byte[]> CaseFinishSecondInstanceListReportExportExcel(CaseFinishListFilterReportVM model);

        Task<byte[]> DocumentInGoingReportToExcelOne(DocumentInFilterReportVM model);
        Task<byte[]> HeritageReportToExcelOneNew(HeritageFilterReportVM model);

        /// <summary>
        /// Справка за съдебни актове
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<CaseSessionActReportVM> ActReport_Select(CaseSessionActReportFilterVM filter);

        /// <summary>
        /// Справка за Дела с ненаписани съдебни актове от всички съдии
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<CaseSprVM> CaseWithoutFinalAct_Select(CaseFilterReport filter);

        /// <summary>
        /// Извличане на данни за Информация за страни
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<CasePersonReportVM> CasePersonInformation_Select(CasePersonFilterVM filter);

        /// <summary>
        /// Справка документи, постъпили чрез ЕЕСПП
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<DocumentsReceivedEESPPReportVM> DocumentsReceivedEESPP_Select(DocumentsReceivedEESPPFilterVM filter);

        /// <summary>
        /// Извличане на данни за движение на дела
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<MigrationReportVM> MigrationReport_Select(MigrationReportFilterVM filter);

        /// <summary>
        /// Специализирана справка - заявка 13
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<SpecializedReportVM> SpecializedReport_Select(SpecializedReportFilterVM filter);

        /// <summary>
        /// Дела от движенията към дело от специализирана справка  - заявка 13
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        IQueryable<SpecializedReportVM> SpecializedReportMigrationCase_Select(int caseId);

        Task<DataTableResponseVM<CaseFirstInstanceListReportVM>> CaseFirstInstanceListReportDataTable_Select(CaseFirstInstanceListFilterReportVM filter, int start, int length, List<DataTablesSortColumnVM> sortedColumns);

        /// <summary>
        /// Справка разпределение на дела
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<CaseSelectionProtokolFastProcessVM> CaseSelectionProtokolFastProcess_Select(CaseSelectionProtokolFilterFastProcessVM filter);

        /// <summary>
        /// Справка на заповедните производства
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<Infrastructure.Models.ViewModels.Report.CaseFastProcessVM> CaseFastProcess_Select(CaseFilterFastProcessVM filter);

        /// <summary>
        /// Справка на списък на длъжници/заявители за дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<ListDebtorsApplicantsFastProcessVM> ListDebtorsApplicantsFilterFastProcess_Select(ListDebtorsApplicantsFilterFastProcessVM filter);

        /// <summary>
        /// Справка за регистрираните документи за определен период за дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<RegisteredDocumentsFastProcessVM> RegisteredDocumentsFastProcess_Select(RegisteredDocumentsFilterFastProcessVM filter);

        /// <summary>
        /// Извличане на данни за справка необработени документ по чл 410 и 417 от ЕИСС
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<RawDocumentsFastProcessVM> GetRawDocumentsFastProcess_Select(RawDocumentsFilterFastProcessVM filter);

        /// <summary>
        /// Справка нотификации по дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<WorkNotificationFatsProcessVM> WorkNotificationFatsProcess_Select(WorkNotificationFilterFatsProcessVM filter);

        /// <summary>
        /// Групова отмяна на нотификации
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<bool> WorkNotificationsSetIsRead(WorkNotificationSetIsReadVM model);

        /// <summary>
        /// Груповo гасене на нотификации
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<bool> WorkNotificationsSetTurnOff(WorkNotificationSetIsReadVM model);

        /// <summary>
        /// Справка изпълнителни листове по дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<ExecListFatsProcessVM> ExecListFatsProcess_Select(ExecListFilterFatsProcessVM filter);

        /// <summary>
        /// Извличане на данни за справка действителен зает щат в съд
        /// </summary>
        /// <returns></returns>
        IQueryable<CourtJudgeCountVM> CourtJudgeCount_Select();

        /// <summary>
        /// Справка за актове по дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<CaseSessionActFastProcessVM> CaseSessionActFastProcess_Select(CaseSessionActFastProcessFilterVM filter);

        /// <summary>
        /// Извличане на типове актове за падащо меню за справка за актове по дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        /// <param name="addDefaultElement">Дали да добави елемент "Избери"</param>
        /// <param name="addAllElement">Дали да добави елемент "Всички"</param>
        /// <returns></returns>
        List<SelectListItem> GetDDLActTypeForCaseSessionActFastProcess(bool addDefaultElement = false, bool addAllElement = true);

        /// <summary>
        /// Справка за медиация
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        Task<IQueryable<MediationCaseVM>> CaseMediation_Select(MediationCaseFilterVM filter);

        /// <summary>
        /// Справка за срещи за медиация 
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        Task<IQueryable<MediationCaseSessionVM>> CaseMediationSession_Select(MediationCaseSessionFilterVM filter);

        /// <summary>
        /// Отчет за работата на съдебните центрове по медиация
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <param name="start">От коя позиция да дръпне данните</param>
        /// <param name="length">Дължина</param>
        /// <returns></returns>
        Task<DataTableResponseVM<ReportWorkJudicialMediationCentersVM>> GetReportWorkJudicialMediationCenters(ReportWorkJudicialMediationCentersFilterVM filter, int start, int length);
    }
}
