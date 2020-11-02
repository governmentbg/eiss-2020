// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IO.RegixClient;
using IOWebApplication.Infrastructure.Models.Regix.FetchNomenclatures;
using IOWebApplication.Infrastructure.Models.Regix.GetActualStateV3;
using IOWebApplication.Infrastructure.Models.Regix.GetEmploymentContracts;
using IOWebApplication.Infrastructure.Models.Regix.GetPensionIncomeAmountReport;
using IOWebApplication.Infrastructure.Models.Regix.GetPersonalIdentityV2;
using IOWebApplication.Infrastructure.Models.Regix.GetStateOfPlay;
using IOWebApplication.Infrastructure.Models.Regix.SearchDisabilityCompensationByPaymentPeriod;
using IOWebApplication.Infrastructure.Models.Regix.SearchUnemploymentCompensationByPaymentPeriod;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.RegixReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IRegixReportService : IBaseService
    {
        PersonDataResponseType GetPersonalData(string egn);

        PermanentAddressResponseType GetPermanentAddress(string egn);

        TemporaryAddressResponseType GetCurrentAddress(string egn);

        ActualStateResponseV3 GetActualStateV3(string uic);

        EmploymentContractsResponse GetEmploymentContracts(string identityId, EikTypeType eikType, ContractsFilterType contractsFilterType);

        POVNVEDResponseType SearchDisabilityCompensationByPaymentPeriod(string identifier, 
                 Infrastructure.Models.Regix.SearchDisabilityCompensationByPaymentPeriod.IdentifierType identifierType, DateTime dateFrom, DateTime dateTo);

        POBVEDResponseType SearchUnemploymentCompensationByPaymentPeriod(string identifier,
                 Infrastructure.Models.Regix.SearchUnemploymentCompensationByPaymentPeriod.IdentifierType identifierType, DateTime dateFrom, DateTime dateTo);

        UP8ResponseType GetPensionIncomeAmountReport(string identifier,
            Infrastructure.Models.Regix.GetPensionIncomeAmountReport.IdentifierType identifierType, DateTime dateFrom, DateTime dateTo);

        DocumentRegixVM GetPersonalIdentity(string identityDocumentNumber, string egn);

        PersonalIdentityInfoResponseType GetPersonalIdentityV2(string identityDocumentNumber, string egn);

        StateOfPlay GetStateOfPlay(string uic);

        Nomenclatures FetchNomenclatures();

        bool PersonData_SaveData(RegixPersonDataVM model);
        RegixPersonDataVM GetPersonalDataById(int id);


        bool PersonAddress_SaveData(RegixPersonAddressVM model);
        RegixPersonAddressVM GetPersonAddressById(int id);


        bool EmploymentContracts_SaveData(RegixEmploymentContractsVM model);
        RegixEmploymentContractsVM GetEmploymentContractsById(int id);


        bool CompensationByPaymentPeriod_SaveData(RegixCompensationByPaymentPeriodVM model);
        RegixCompensationByPaymentPeriodVM GetCompensationByPaymentPeriodById(int id);


        bool PensionIncomeAmountReport_SaveData(RegixPensionIncomeAmountVM model);
        RegixPensionIncomeAmountVM GetPensionIncomeAmountReportById(int id);


        bool PersonalIdentityV2_SaveData(RegixPersonalIdentityV2VM model);
        RegixPersonalIdentityV2VM GetPersonalIdentityV2ById(int id);


        bool ActualStateV3_SaveData(RegixActualStateV3VM model);
        (bool result, string errorMessage, RegixActualStateV3VM model) GetActualStateV3ById(int id);

        IEnumerable<PersonSearchVM> PersonSearch(int uicType, string uic);


        RegixStateOfPlayVM GetStateOfPlayById(int id);
        bool StateOfPlay_SaveData(RegixStateOfPlayVM model);

        bool PersonDataAddress_SaveData(RegixPersonDataAddressVM model);

        RegixPersonDataAddressVM GetPersonDataAddressById(int id);

        IQueryable<RegixReportListVM> RegixReportList_Select(int courtId, RegixReportListFilterVM model);

        IQueryable<RegixListVM> RegixListByCase_Select(int caseId);
    }
}
