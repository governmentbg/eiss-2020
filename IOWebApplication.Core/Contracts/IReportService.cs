using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IReportService
    {
        IQueryable<PaymentPosReportVM> PaymentPosReport_Select(int courtId, PaymentPosFilterReportVM model);
        IQueryable<FineReportVM> FineReport_Select(int courtId, FineFilterReportVM model);
        IQueryable<StateFeeReportVM> StateFeeReport_Select(int courtId, StateFeeFilterReportVM model);
        IQueryable<ObligationJuryReportVM> ObligationJuryReport_Select(int courtId, ObligationJuryFilterReportVM model);

        byte[] CaseAlphabetical_ToExcel(int courtId, CaseAlphabeticalFilterVM model);
        byte[] ZzdnReportToExcelOne(ZzdnFilterReportVM model);
        byte[] EuropeanHeritageReportToExcelOne(EuropeanHeritageFilterReportVM model);
        byte[] InsolvencyReportToExcelOne(InsolvencyFilterReportVM model);
        byte[] PublicInformationReportToExcelOne(PublicInformationFilterReportVM model);
        byte[] CaseDecisionReportToExcelOne(CaseDecisionFilterReportVM model, string url);
        byte[] HeritageReportToExcelOne(HeritageFilterReportVM model);
        byte[] CaseFirstInstanceReportToExcelOne(CaseFirstInstanceFilterReportVM model);
        byte[] CaseMigrationReturnReportToExcelOne(CaseMigrationReturnFilterReportVM model);
        byte[] CaseArchiveReportToExcelOne(CaseArchiveFilterReportVM model);
        byte[] DivorceReportToExcelOne(DivorceFilterReportVM model);
        byte[] CaseSecondInstanceReportToExcelOne(CaseSecondInstanceFilterReportVM model);
        byte[] SentenceReportToExcelOne(SentenceFilterReportVM model);
        byte[] DocumentOutGoingReportToExcelOne(DocumentOutFilterReportVM model);
        byte[] DocumentInGoingReportToExcelOne(DocumentInFilterReportVM model);
        byte[] DeliveryBookReportToExcelOne(DeliveryBookFilterVM model);
        byte[] DismisalReportToExcelOne(DismisalReportFilterVM model);
        byte[] CaseObligationReportToExcelOne(CaseObligationFilterReportVM model);
        byte[] ExecListReportToExcelOne(ExecListFilterReportVM model);
        byte[] CaseArchiveListReportToExcelOne(CaseArchiveListFilterReportVM model);
        byte[] DocumentOutListReportToExcelOne(DocumentOutListFilterReportVM model);
        byte[] PosDeviceReportToExcelOne(PosDeviceFilterReportVM model);
        byte[] CaseSessionPrivateReportToExcelOneTemplate(CaseSessionPrivateFilterReportVM model);
        byte[] CaseSessionPublicReportToExcelOne(CaseSessionPublicFilterReportVM model);
        byte[] FineReportToExcelOne(FineFilterReportVM model);
        byte[] StateFeeReportExportExcel(StateFeeFilterReportVM model);
        byte[] PaymentPosReportToExcelOne(PaymentPosFilterReportVM model);
        byte[] CourtStatsReport(DateTime? date);
        IQueryable<CaseArchiveListReportVM> CaseArchiveListReport_Select(int courtId, CaseArchiveListFilterReportVM model);
        IQueryable<DocumentOutListReportVM> DocumentOutListReport_Select(int courtId, DocumentOutListFilterReportVM model, string newLine);
        IQueryable<PosDeviceReportVM> PosDeviceReport_Select(int courtId, PosDeviceFilterReportVM model);
        byte[] ObligationJuryReportToExcelOne(ObligationJuryFilterReportVM model);
        IQueryable<CaseLinkReportVM> CaseLinkReport_Select(int courtId, CaseLinkFilterReportVM model, string newLine);
        byte[] CaseLinkReportExportExcel(CaseLinkFilterReportVM model);

        List<TableDescription> TableDescription_Select();
        IQueryable<CaseDecisionReportVM> CaseDecisionReport_Select(int courtId, CaseDecisionFilterReportVM model);
        IQueryable<SentenceListReportVM> SentenceListReport_Select(int courtId, SentenceListFilterReportVM model, string newLine);
        byte[] SentenceListReportExportExcel(SentenceListFilterReportVM model);
        IQueryable<SessionActForDepersonalizeReportVM> SessionActForDepersonalizeReport_Select(int courtId, SessionActForDepersonalizeFilterReportVM model);
        IQueryable<CasePersonDefendantListReportVM> CasePersonDefendantListReport_Select(int courtId, CasePersonDefendantListFilterReportVM model, string newLine);
        byte[] CasePersonDefendantListReportExportExcel(CasePersonDefendantListFilterReportVM model);
        IQueryable<CaseFirstInstanceListReportVM> CaseFirstInstanceListReport_Select(int courtId, CaseFirstInstanceListFilterReportVM model);
        byte[] CaseFirstInstanceListReportExportExcel(CaseFirstInstanceListFilterReportVM model);
        byte[] CaseSecondInstanceListReportExportExcel(CaseSecondInstanceListFilterReportVM model);
        IQueryable<CaseSecondInstanceListReportVM> CaseSecondInstanceListReport_Select(int courtId, CaseSecondInstanceListFilterReportVM model);
        IQueryable<CaseFinishListReportVM> CaseFinishFirstInstanceListReport_Select(int courtId, CaseFinishListFilterReportVM model, string newLine);
        byte[] CaseFinishFirstInstanceListReportExportExcel(CaseFinishListFilterReportVM model);
        IQueryable<CaseFinishListReportVM> CaseFinishSecondInstanceListReport_Select(int courtId, CaseFinishListFilterReportVM model, string newLine);
        byte[] CaseFinishSecondInstanceListReportExportExcel(CaseFinishListFilterReportVM model);
    }
}
