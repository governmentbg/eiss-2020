using IO.RegixClient;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.RegixReport;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IRegixReportService : IBaseService
    {
        Task<PersonDataResponseType> GetPersonalData(string egn, string remark = "");

        Task<DocumentRegixVM> GetPersonalIdentity(string identityDocumentNumber, string egn, string remark = "");

        Task<bool> PersonData_SaveData(RegixPersonDataVM model);

        RegixPersonDataVM GetPersonalDataById(int id);


        Task<bool> PersonAddress_SaveData(RegixPersonAddressVM model);
        RegixPersonAddressVM GetPersonAddressById(int id);


        Task<bool> EmploymentContracts_SaveData(RegixEmploymentContractsVM model);
        RegixEmploymentContractsVM GetEmploymentContractsById(int id);


        Task<bool> CompensationByPaymentPeriod_SaveData(RegixCompensationByPaymentPeriodVM model);
        RegixCompensationByPaymentPeriodVM GetCompensationByPaymentPeriodById(int id);


        Task<bool> PensionIncomeAmountReport_SaveData(RegixPensionIncomeAmountVM model);
        RegixPensionIncomeAmountVM GetPensionIncomeAmountReportById(int id);


        Task<bool> PersonalIdentityV2_SaveData(RegixPersonalIdentityV2VM model);
        RegixPersonalIdentityV2VM GetPersonalIdentityV2ById(int id);


        Task<bool> ActualStateV3_SaveData(RegixActualStateV3VM model);
        (bool result, string errorMessage, RegixActualStateV3VM model) GetActualStateV3ById(int id);

        Task<IEnumerable<PersonSearchVM>> PersonSearch(int uicType, string uic, long? regixReasonDocumentId, int? regixReasonCaseId, string regixReasonDescription, string regixReasonGuid, int? regixRequestTypeId);


        Task<RegixStateOfPlayVM> GetStateOfPlayById(int id);
        Task<bool> StateOfPlay_SaveData(RegixStateOfPlayVM model);

        Task<bool> PersonDataAddress_SaveData(RegixPersonDataAddressVM model);

        RegixPersonDataAddressVM GetPersonDataAddressById(int id);

        IQueryable<RegixReportListVM> RegixReportList_Select(int courtId, RegixReportListFilterVM model);

        IQueryable<RegixListVM> RegixListByCase_Select(int caseId);

        Task<PermanentAddressResponseType> GetPermanentAddressAndSave(string egn, long? regixReasonDocumentId, int? regixReasonCaseId, string regixReasonDescription, string regixReasonGuid, int? regixRequestTypeId);
        Task<TemporaryAddressResponseType> GetCurrentAddressAndSave(string egn, long? regixReasonDocumentId, int? regixReasonCaseId, string regixReasonDescription, string regixReasonGuid, int? regixRequestTypeId);
        Task<bool> RelationsSearch_SaveData(RegixRelationsSearchVM model);
        RegixRelationsSearchVM GetRelationsSearchById(int id);
        Task<bool> CriminalRecordsReport_SaveData(RegixCriminalRecordsReportVM model);
        RegixCriminalRecordsReportVM GetCriminalRecordsReportById(int id);
    }
}
