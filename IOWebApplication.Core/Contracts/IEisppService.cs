using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.Eispp.ActualData;
using IOWebApplication.Infrastructure.Models.Integrations.Eispp;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Eispp;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IEisppService : IBaseService
    {
        string GetElement(string tblCode, string code);
        Task<EisppTSActualDataVM> GetActualData(string eisppNumber, bool readFromMongoIfFail = true);
        void ConvertEisppPersonToDocumentPerson(EisppTSActualDataPersonVM source, DocumentPersonVM target, int personIndex);
        IQueryable<EisppTblElement> EisppTblElement_Select(string EisppTblCode);
        List<SelectListItem> GetDDL_EISPPTblElement(string EisppTblCode, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_CountriesForEISPP(bool addDefaultElement = true, bool addAllElement = false);
        EisppTblElement GetByCode(string Code);
        IEnumerable<LabelValueVM> Get_EISPPTblElement(string EisppTblCode, string term, string id);

        Task<EisppPackage> GeneratePackage(EisppEventVM model);
        bool SaveCasePackageData(EisppPackage model, int? eventFromId);
        List<SelectListItem> GetDDL_EISPPEventType(int caseCodeId, int caseTypeId, bool isExternal, bool addDefaultElement = true);
         void CPPersonCrimeUnion(EisppPackage eisppPackage);
        EisppEventVM GetEisppEventVM(string sourceType, string sourceId, int caseId, int? caseSessionActId);
        EisppDropDownVM GetDDL_EISPPTblElementWithRules(string EisppTblCode, int eventType, string rulePath, bool addDefaultElement = true);
        string CheckSum(string code);
        List<SelectListItem> GetDDL_ConnectedCases(int caseId, bool addDefaultElement = true);
        Task<bool> SaveCaseMigration(EisppEventVM model);
        IQueryable<EisppEventItemVM> GetPackages(EisppEventFilterVM filter);
        List<SelectListItem> GetLinkTypeDDL(bool addDefaultElement = true);
        List<SelectListItem> CaseSessionActDDL(int caseId, int? eventTypeId, DateTime? DateFrom, DateTime? DateTo, string defaultText = "Избери");
        EisppPackage GetPackage(int packageId);
        (EisppDropDownVM, string, int, bool, bool) GetPunishmentPeriodMode(int eventType, int punishmentKind, int servingType);
        string GetPunishmentServingTypeMode(int servingTypeId);
        EisppDropDownVM GetDDL_FeatureValTblElementWithRules(int eventType, string rulePath, List<SelectListItem> ek_countries, bool addDefaultElement = true);
        bool SaveExpireInfoPlus(ExpiredInfoVM model);
        Task<CdnDownloadResult> GetNPCard(int id);
        Task<CdnDownloadResult> GetEisppResponse(int id);
        byte[] GetEisppRequest(int id);
        List<SelectListItem> GetPersonProceduralCoercionMeasure(int casePersonId, bool isOld, int eventId, bool addDefaultElement = true);
        CasePersonSentence GetSentence(int? casePersonId, int? caseSessionActId);
        EisppChangeVM GeneratePackageFrom(int eventFromId);
        int GeneratePackageDelete(int eventId);
        EisppChangeVM GetPackageChange(int packageId);
        string GetPbcMeasureUnit(int pbcMeasureTypeId);
        int GetSentencePersonId(int? caseSessionActId);
        EisppDropDownVM GetDDL_EISPPTblElementWithRules_DloOsnExactType(string EisppTblCode, int eventType, string rulePath, string caseType, int caseCharacterId, bool addDefaultElement = true);
        EisppDropDownVM GetDDL_EISPPTblElementNomWithRules(int eventType, string rulePath, string nomRulePath, bool addDefaultElement = true);
        Task<execTSAKTSTSResponse1> GetTSAKTSTSResponse(string eisppNumber);
        Task<List<SelectListItem>> GetDDL_PneNumbers(int caseId, string eisppNumber);
        string GetEisppNumber(int caseId);
        string GetElementLabel(string code);
        bool IsForEisppNum(Case caseCurrent);
        bool MakeEisppNumberPNE(CaseCrime caseCrime, int courtId);
        bool MakeEisppNumberNP(Case caseCurrent);
        List<SelectListItem> GetIntegrationStateDDL();
        bool HaveEventForMeasure(int measureId);
        bool HaveEventForPunishment(int casePersonSentencePunishmentId);
        bool HaveEventForCrime(int caseCrimeId);
        List<SelectListItem> GetDDL_CaseMigrations(int caseId);
        List<SelectListItem> DocumentComplaintDDL(int caseId);
        bool GetCaseIsExternal(int caseId);
    }
}
