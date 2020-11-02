// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IO.RegixClient;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Regix;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Regix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IOWebApplication.Core.Services
{
    public class RegixReportService : BaseService, IRegixReportService
    {
        private readonly IRegixClient client;

        public RegixReportService(ILogger<RegixReportService> _logger,
                     IRegixClient _client,
                     IRepository _repo,
                     IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            client = _client;
            userContext = _userContext;
        }

        private CallContext RegixCallContex()
        {
            return new CallContext()
            {
                AdministrationName = "Висш съдебен съвет",
                AdministrationOId = "2.16.100.1.1.511.1.2",
                EmployeeIdentifier = "1",
                EmployeeNames = "Администратор",
                EmployeePosition = "Администратор",
                LawReason = "За целите на разработката и тестването на ЕИСС",
                Remark = "",
                ServiceType = "За административна услуга",
                ServiceURI = "2.16.100.1.1.511.1.2"
            };
        }

        public PersonDataResponseType GetPersonalData(string egn)
        {
            PersonDataRequestType req = new PersonDataRequestType()
            {
                EGN = egn
            };
            CallContext context = RegixCallContex();

            string operationName = "TechnoLogica.RegiX.GraoNBDAdapter.APIService.INBDAPI.PersonDataSearch";
            return client.GetData<PersonDataResponseType>(operationName, req, context);
        }

        public PermanentAddressResponseType GetPermanentAddress(string egn)
        {
            CallContext context = RegixCallContex();
            return client.GetPermanentAddress(egn, DateTime.Now, context);
        }
        public TemporaryAddressResponseType GetCurrentAddress(string egn)
        {
            CallContext context = RegixCallContex();
            return client.GetCurrentAddress(egn, DateTime.Now, context);
        }

        public ActualStateResponseV3 GetActualStateV3(string uic)
        {
            ActualStateRequestV3 req = new ActualStateRequestV3()
            {
                UIC = uic,
                FieldList = ""
            };
            CallContext context = RegixCallContex();

            string operationName = "TechnoLogica.RegiX.AVTRAdapter.APIService.ITRAPI.GetActualStateV3";
            return client.GetData<ActualStateResponseV3>(operationName, req, context);
        }

        public EmploymentContractsResponse GetEmploymentContracts(string identityId, EikTypeType eikType, ContractsFilterType contractsFilterType)
        {
            IdentityTypeRequest identity = new IdentityTypeRequest() { ID = identityId, TYPE = eikType };
            EmploymentContractsRequest req = new EmploymentContractsRequest()
            {
                Identity = identity,
                ContractsFilter = contractsFilterType,
                ContractsFilterSpecified = true
            };
            CallContext context = RegixCallContex();

            string operationName = "TechnoLogica.RegiX.NRAEmploymentContractsAdapter.APIService.INRAEmploymentContractsAPI.GetEmploymentContracts";
            return client.GetData<EmploymentContractsResponse>(operationName, req, context);
        }

        public POVNVEDResponseType SearchDisabilityCompensationByPaymentPeriod(string identifier, Infrastructure.Models.Regix.SearchDisabilityCompensationByPaymentPeriod.IdentifierType identifierType, DateTime dateFrom, DateTime dateTo)
        {
            POVNVEDRequestType req = new POVNVEDRequestType()
            {
                Identifier = identifier,
                IdentifierType = identifierType,
                DateFrom = dateFrom,
                DateTo = dateTo
            };
            CallContext context = RegixCallContex();

            string operationName = "TechnoLogica.RegiX.NoiROAdapter.APIService.IROAPI.SearchDisabilityCompensationByPaymentPeriod";
            return client.GetData<POVNVEDResponseType>(operationName, req, context);
        }

        public POBVEDResponseType SearchUnemploymentCompensationByPaymentPeriod(string identifier, Infrastructure.Models.Regix.SearchUnemploymentCompensationByPaymentPeriod.IdentifierType identifierType, DateTime dateFrom, DateTime dateTo)
        {
            POBVEDRequestType req = new POBVEDRequestType()
            {
                Identifier = identifier,
                IdentifierType = identifierType,
                DateFrom = dateFrom,
                DateTo = dateTo
            };
            CallContext context = RegixCallContex();

            string operationName = "TechnoLogica.RegiX.NoiROAdapter.APIService.IROAPI.SearchUnemploymentCompensationByPaymentPeriod";
            return client.GetData<POBVEDResponseType>(operationName, req, context);
        }

        public UP8ResponseType GetPensionIncomeAmountReport(string identifier, Infrastructure.Models.Regix.GetPensionIncomeAmountReport.IdentifierType identifierType, DateTime dateFrom, DateTime dateTo)
        {
            PeriodType period = new PeriodType();
            period.From = new MonthType();
            period.To = new MonthType();
            period.From.Month = "--" + dateFrom.Month.ToString("00");
            period.From.Year = dateFrom.Year.ToString();

            period.To.Month = "--" + dateTo.Month.ToString("00");
            period.To.Year = dateTo.Year.ToString();
            UP8RequestType req = new UP8RequestType()
            {
                Identifier = identifier,
                IdentifierType = identifierType,
                Period = period
            };
            CallContext context = RegixCallContex();

            string operationName = "TechnoLogica.RegiX.NoiRPAdapter.APIService.IRPAPI.GetPensionIncomeAmountReport";
            return client.GetData<UP8ResponseType>(operationName, req, context);
        }

        public PersonalIdentityInfoResponseType GetPersonalIdentityV2(string identityDocumentNumber, string egn)
        {
            PersonalIdentityInfoRequestType req = new PersonalIdentityInfoRequestType()
            {
                IdentityDocumentNumber = identityDocumentNumber,
                EGN = egn
            };
            CallContext context = RegixCallContex();

            string operationName = "TechnoLogica.RegiX.MVRBDSAdapter.APIService.IMVRBDSAPI.GetPersonalIdentityV2";
            return client.GetData<PersonalIdentityInfoResponseType>(operationName, req, context);
        }

        public StateOfPlay GetStateOfPlay(string uic)
        {
            GetStateOfPlayRequest req = new GetStateOfPlayRequest()
            {
                UIC = uic
            };
            CallContext context = RegixCallContex();

            string operationName = "TechnoLogica.RegiX.AVBulstat2Adapter.APIService.IAVBulstat2API.GetStateOfPlay";
            return client.GetData<StateOfPlay>(operationName, req, context);
        }

        public Nomenclatures FetchNomenclatures()
        {
            FetchNomenclatures req = new FetchNomenclatures()
            {

            };
            CallContext context = RegixCallContex();

            string operationName = "TechnoLogica.RegiX.AVBulstat2Adapter.APIService.IAVBulstat2API.FetchNomenclatures";
            return client.GetData<Nomenclatures>(operationName, req, context);
        }

        private bool RegixReport_SaveData(RegixReport model, RegixReportVM reportVM, int regixTypeId, string requestData, string responseData, bool saveChanges)
        {
            try
            {
                model.CourtId = reportVM.CourtId;
                model.RegixTypeId = regixTypeId;
                model.RequestData = requestData;
                model.ResponseData = responseData;
                model.CaseId = (reportVM.CaseId ?? 0) > 0 ? reportVM.CaseId : null;
                model.CaseSessionActId = (reportVM.CaseSessionActId ?? 0) > 0 ? reportVM.CaseSessionActId : null;
                model.DocumentId = (reportVM.DocumentId ?? 0) > 0 ? reportVM.DocumentId : null;
                model.Description = reportVM.Description;
                model.UserId = userContext.UserId;
                model.DateWrt = DateTime.Now;
                repo.Add<RegixReport>(model);
                if (saveChanges)
                    repo.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на RegixReport");
                return false;
            }
        }

        public bool PersonData_SaveData(RegixPersonDataVM model)
        {
            try
            {
                var response = GetPersonalData(model.PersonDataFilter.EgnFilter);
                RegixReport saved = new RegixReport();
                if (RegixReport_SaveData(saved, model.Report, NomenclatureConstants.RegixType.PersonData, JsonConvert.SerializeObject(model.PersonDataFilter),
                         JsonConvert.SerializeObject(response), true) == false)
                {
                    return false;
                }

                model.Report.Id = saved.Id;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на PersonData");
                return false;
            }
        }

        private void MapPersonData(PersonDataResponseType fromObj, RegixPersonDataResponseVM toObj)
        {
            toObj.PersonNamesFirstName = fromObj.PersonNames.FirstName;
            toObj.PersonNamesSurName = fromObj.PersonNames.SurName;
            toObj.PersonNamesFamilyName = fromObj.PersonNames.FamilyName;
            toObj.Alias = fromObj.Alias;
            toObj.LatinNamesFirstName = fromObj.LatinNames.FirstName;
            toObj.LatinNamesSurName = fromObj.LatinNames.SurName;
            toObj.LatinNamesFamilyName = fromObj.LatinNames.FamilyName;
            toObj.ForeignNamesFirstName = fromObj.ForeignNames.FirstName;
            toObj.ForeignNamesSurName = fromObj.ForeignNames.SurName;
            toObj.ForeignNamesFamilyName = fromObj.ForeignNames.FamilyName;
            toObj.GenderName = fromObj.Gender.GenderName.ToString();
            toObj.Egn = fromObj.EGN;
            toObj.BirthDate = fromObj.BirthDateSpecified == true ? fromObj.BirthDate.ToString("dd.MM.yyyy") : "";
            toObj.PlaceBirth = fromObj.PlaceBirth;
            toObj.NationalityCode = fromObj.Nationality.NationalityCode;
            toObj.NationalityName = fromObj.Nationality.NationalityName;
            toObj.NationalityCode2 = fromObj.Nationality.NationalityCode2;
            toObj.NationalityName2 = fromObj.Nationality.NationalityName2;
            toObj.DeathDate = fromObj.DeathDateSpecified == true ? fromObj.DeathDate.ToString("dd.MM.yyyy") : "";
        }

        public RegixPersonDataVM GetPersonalDataById(int id)
        {
            var report = GetRegixReportById(id);
            var personData = JsonConvert.DeserializeObject<PersonDataResponseType>(report.ResponseData);

            RegixPersonDataVM model = new RegixPersonDataVM();
            SetRegixReportVM(report, model.Report);
            model.PersonDataFilter = JsonConvert.DeserializeObject<RegixPersonDataFilterVM>(report.RequestData);
            MapPersonData(personData, model.PersonDataResponse);
            return model;
        }

        public bool PersonAddress_SaveData(RegixPersonAddressVM model)
        {
            try
            {
                string responseJson = "";
                if (model.AddressTypeId == NomenclatureConstants.RegixType.PersonPermanentAddress)
                {
                    var response = GetPermanentAddress(model.PersonAddressFilter.EgnFilter);
                    responseJson = JsonConvert.SerializeObject(response);
                }
                else if (model.AddressTypeId == NomenclatureConstants.RegixType.PersonCurrentAddress)
                {
                    var response = GetCurrentAddress(model.PersonAddressFilter.EgnFilter);
                    responseJson = JsonConvert.SerializeObject(response);
                }
                else
                {
                    return false;
                }
                RegixReport saved = new RegixReport();
                if (RegixReport_SaveData(saved, model.Report, model.AddressTypeId, JsonConvert.SerializeObject(model.PersonAddressFilter), responseJson, true) == false)
                {
                    return false;
                }

                model.Report.Id = saved.Id;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на PersonAddress");
                return false;
            }
        }

        private void MapPermanentAddress(PermanentAddressResponseType fromObj, RegixPersonAddressResponseVM toObj)
        {
            toObj.DistrictName = fromObj.DistrictName;
            toObj.MunicipalityName = fromObj.MunicipalityName;
            toObj.SettlementName = fromObj.SettlementName;
            toObj.CityArea = fromObj.CityArea;
            toObj.LocationName = fromObj.LocationName;
            toObj.BuildingNumber = fromObj.BuildingNumber;
            toObj.Entrance = fromObj.Entrance;
            toObj.Floor = fromObj.Floor;
            toObj.Apartment = fromObj.Apartment;
            toObj.FromDate = fromObj.FromDateSpecified == true ? fromObj.FromDate.ToString("dd.MM.yyyy") : "";
        }

        private void MapCurrentAddress(TemporaryAddressResponseType fromObj, RegixPersonAddressResponseVM toObj)
        {
            toObj.CountryName = fromObj.CountryName;
            toObj.DistrictName = fromObj.DistrictName;
            toObj.MunicipalityName = fromObj.MunicipalityName;
            toObj.SettlementName = fromObj.SettlementName;
            toObj.CityArea = fromObj.CityArea;
            toObj.LocationName = fromObj.LocationName;
            toObj.BuildingNumber = fromObj.BuildingNumber;
            toObj.Entrance = fromObj.Entrance;
            toObj.Floor = fromObj.Floor;
            toObj.Apartment = fromObj.Apartment;
            toObj.FromDate = fromObj.FromDateSpecified == true ? fromObj.FromDate.ToString("dd.MM.yyyy") : "";
        }

        public RegixPersonAddressVM GetPersonAddressById(int id)
        {
            var report = GetRegixReportById(id);
            RegixPersonAddressVM model = new RegixPersonAddressVM();
            SetRegixReportVM(report, model.Report);

            model.AddressTypeId = report.RegixTypeId;
            model.PersonAddressFilter = JsonConvert.DeserializeObject<RegixPersonAddressFilterVM>(report.RequestData);

            if (report.RegixTypeId == NomenclatureConstants.RegixType.PersonPermanentAddress)
            {
                var permanentAddress = JsonConvert.DeserializeObject<PermanentAddressResponseType>(report.ResponseData);
                MapPermanentAddress(permanentAddress, model.PersonAddressResponse);
            }
            else if (report.RegixTypeId == NomenclatureConstants.RegixType.PersonCurrentAddress)
            {
                var currentAddress = JsonConvert.DeserializeObject<TemporaryAddressResponseType>(report.ResponseData);
                MapCurrentAddress(currentAddress, model.PersonAddressResponse);
            }
            else
            {

            }
            return model;
        }

        public bool EmploymentContracts_SaveData(RegixEmploymentContractsVM model)
        {
            try
            {
                var response = GetEmploymentContracts(model.EmploymentContractsFilter.IdentityFilter, (EikTypeType)model.EmploymentContractsFilter.EikTypeId,
                                                      (ContractsFilterType)model.EmploymentContractsFilter.ContractsFilterTypeId);
                RegixReport saved = new RegixReport();
                if (RegixReport_SaveData(saved, model.Report, NomenclatureConstants.RegixType.EmploymentContracts,
                                         JsonConvert.SerializeObject(model.EmploymentContractsFilter), JsonConvert.SerializeObject(response), true) == false)
                {
                    return false;
                }

                model.Report.Id = saved.Id;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на EmploymentContracts");
                return false;
            }
        }

        private void MapEmploymentContracts(EContract fromObj, RegixEmploymentContractsResponseVM toObj)
        {
            toObj.ContractorBulstat = fromObj.ContractorBulstat;
            toObj.ContractorName = fromObj.ContractorName;
            toObj.IndividualEIK = fromObj.IndividualEIK;
            toObj.IndividualNames = fromObj.IndividualNames;
            toObj.StartDate = fromObj.StartDateSpecified == true ? fromObj.StartDate.ToString("dd.MM.yyyy") : "";
            toObj.LastAmendDate = fromObj.LastAmendDateSpecified == true ? fromObj.LastAmendDate.ToString("dd.MM.yyyy") : "";
            toObj.EndDate = fromObj.EndDateSpecified == true ? fromObj.EndDate.ToString("dd.MM.yyyy") : "";
            toObj.Reason = fromObj.ReasonSpecified == true ? fromObj.Reason.ToString() : "";
            toObj.TimeLimit = fromObj.TimeLimitSpecified == true ? fromObj.TimeLimit.ToString("dd.MM.yyyy") : "";
            toObj.EcoCode = fromObj.EcoCode;
            toObj.ProfessionCode = fromObj.ProfessionCode;
            toObj.Remuneration = fromObj.Remuneration.ToString();
            toObj.ProfessionName = fromObj.ProfessionName;
        }

        public RegixEmploymentContractsVM GetEmploymentContractsById(int id)
        {
            var report = GetRegixReportById(id);
            var employmentContracts = JsonConvert.DeserializeObject<EmploymentContractsResponse>(report.ResponseData);

            RegixEmploymentContractsVM model = new RegixEmploymentContractsVM();
            SetRegixReportVM(report, model.Report);

            model.EmploymentContractsFilter = JsonConvert.DeserializeObject<RegixEmploymentContractsFilterVM>(report.RequestData);

            foreach (var item in employmentContracts.EContracts.OrderByDescending(x => x.StartDate))
            {
                RegixEmploymentContractsResponseVM employContractsResponse = new RegixEmploymentContractsResponseVM();
                MapEmploymentContracts(item, employContractsResponse);
                model.EmploymentContractsResponse.Add(employContractsResponse);
            }
            return model;
        }

        public bool CompensationByPaymentPeriod_SaveData(RegixCompensationByPaymentPeriodVM model)
        {
            try
            {
                string responseJson = "";
                if (model.CompensationTypeId == NomenclatureConstants.RegixType.DisabilityCompensationByPaymentPeriod)
                {
                    var response = SearchDisabilityCompensationByPaymentPeriod(model.CompensationByPaymentPeriodFilter.IdentifierFilter,
                            (Infrastructure.Models.Regix.SearchDisabilityCompensationByPaymentPeriod.IdentifierType)model.CompensationByPaymentPeriodFilter.IdentifierTypeFilter,
                            model.CompensationByPaymentPeriodFilter.DateFromFilter,
                            model.CompensationByPaymentPeriodFilter.DateToFilter);
                    responseJson = JsonConvert.SerializeObject(response);
                }
                else if (model.CompensationTypeId == NomenclatureConstants.RegixType.UnemploymentCompensationByPaymentPeriod)
                {
                    var response = SearchUnemploymentCompensationByPaymentPeriod(model.CompensationByPaymentPeriodFilter.IdentifierFilter,
                            (Infrastructure.Models.Regix.SearchUnemploymentCompensationByPaymentPeriod.IdentifierType)model.CompensationByPaymentPeriodFilter.IdentifierTypeFilter,
                            model.CompensationByPaymentPeriodFilter.DateFromFilter,
                            model.CompensationByPaymentPeriodFilter.DateToFilter);
                    responseJson = JsonConvert.SerializeObject(response);
                }
                else
                {
                    return false;
                }
                RegixReport saved = new RegixReport();
                if (RegixReport_SaveData(saved, model.Report, model.CompensationTypeId, JsonConvert.SerializeObject(model.CompensationByPaymentPeriodFilter), responseJson, true) == false)
                {
                    return false;
                }

                model.Report.Id = saved.Id;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CompensationByPaymentPeriod");
                return false;
            }
        }

        private void MapDisabilityCompensationByPaymentPeriod(POVNVEDResponseType fromObj, RegixCompensationByPaymentPerioResponseVM toObj)
        {
            toObj.Identifier = fromObj.Identifier;
            toObj.IdentifierType = fromObj.IdentifierType.ToString();
            toObj.Names = fromObj.Names;
            foreach (var item in fromObj.PaymentData)
            {
                RegixCompensationByPaymentPerioPaymenDataVM payment = new RegixCompensationByPaymentPerioPaymenDataVM();
                payment.BenefitType = item.BenefitType;
                payment.BenefitMonth = item.BenefitMonth;
                payment.BenefitYear = item.BenefitYear;
                payment.BenefitAmount = item.BenefitAmount.ToString("0.00");
                payment.BenefitDatePayment = item.BenefitDatePaymentSpecified == true ? item.BenefitDatePayment.ToString("dd.MM.yyyy") : "";
                toObj.Payments.Add(payment);
            }
        }

        private void MapUnemploymentCompensationByPaymentPeriod(POBVEDResponseType fromObj, RegixCompensationByPaymentPerioResponseVM toObj)
        {
            toObj.Identifier = fromObj.Identifier;
            toObj.IdentifierType = fromObj.IdentifierType.ToString();
            toObj.Names = fromObj.Names;
            foreach (var item in fromObj.PaymentData)
            {
                foreach (var itemPayment in item.Payments)
                {
                    RegixCompensationByPaymentPerioPaymenDataVM payment = new RegixCompensationByPaymentPerioPaymenDataVM();
                    payment.BenefitType = item.BenefitType;
                    payment.BenefitMonth = itemPayment.BenefitMonth;
                    payment.BenefitYear = itemPayment.BenefitYear;
                    payment.BenefitAmount = itemPayment.BenefitAmount.ToString("0.00");
                    toObj.Payments.Add(payment);
                }
            }
        }

        public RegixCompensationByPaymentPeriodVM GetCompensationByPaymentPeriodById(int id)
        {
            var report = GetRegixReportById(id);
            RegixCompensationByPaymentPeriodVM model = new RegixCompensationByPaymentPeriodVM();
            SetRegixReportVM(report, model.Report);

            model.CompensationTypeId = report.RegixTypeId;
            model.CompensationTypeName = report.RegixType.Label;
            model.CompensationByPaymentPeriodFilter = JsonConvert.DeserializeObject<RegixCompensationByPaymentPeriodFilterVM>(report.RequestData);

            if (report.RegixTypeId == NomenclatureConstants.RegixType.DisabilityCompensationByPaymentPeriod)
            {
                var disability = JsonConvert.DeserializeObject<POVNVEDResponseType>(report.ResponseData);
                MapDisabilityCompensationByPaymentPeriod(disability, model.CompensationByPaymentPerioResponse);
            }
            else if (report.RegixTypeId == NomenclatureConstants.RegixType.UnemploymentCompensationByPaymentPeriod)
            {
                var unemployment = JsonConvert.DeserializeObject<POBVEDResponseType>(report.ResponseData);
                MapUnemploymentCompensationByPaymentPeriod(unemployment, model.CompensationByPaymentPerioResponse);
            }
            else
            {

            }
            return model;
        }

        public bool PensionIncomeAmountReport_SaveData(RegixPensionIncomeAmountVM model)
        {
            try
            {
                string responseJson = "";
                var response = GetPensionIncomeAmountReport(model.PensionIncomeAmountFilter.IdentifierFilter,
                        (Infrastructure.Models.Regix.GetPensionIncomeAmountReport.IdentifierType)model.PensionIncomeAmountFilter.IdentifierTypeFilter,
                        model.PensionIncomeAmountFilter.DateFromFilter,
                        model.PensionIncomeAmountFilter.DateToFilter);
                responseJson = JsonConvert.SerializeObject(response);
                RegixReport saved = new RegixReport();
                if (RegixReport_SaveData(saved, model.Report, NomenclatureConstants.RegixType.PensionIncomeAmount, JsonConvert.SerializeObject(model.PensionIncomeAmountFilter), responseJson, true) == false)
                {
                    return false;
                }

                model.Report.Id = saved.Id;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на PensionIncomeAmountReport");
                return false;
            }
        }
        private void MapPensionIncomeAmountReport(UP8ResponseType fromObj, RegixPensionIncomeAmountResponseVM toObj)
        {
            toObj.TerritorialDivisionNOI = fromObj.TerritorialDivisionNOI;
            toObj.Identifier = fromObj.Identifier;
            toObj.Names = (fromObj.Names.Name ?? "") + " " + (fromObj.Names.Surname ?? "") + " " + (fromObj.Names.FamilyName ?? "");
            toObj.PensionerStatus = fromObj.PensionerStatus;
            toObj.DateOfDeath = fromObj.DateOfDeathSpecified == true ? fromObj.DateOfDeath.ToString("dd.MM.yyyy") : "";
            toObj.ContentText = fromObj.ContentText;
            foreach (var item in fromObj.PensionPayments)
            {
                RegixPensionIncomeAmountPaymentVM payment = new RegixPensionIncomeAmountPaymentVM();
                payment.Month = item.Month;
                payment.TotalAmount = item.TotalAmount.ToString("0.00");
                payment.PensionAmount = item.PensionAmount.ToString("0.00");
                payment.AdditionForAssistance = item.AdditionForAssistance.ToString("0.00");
                payment.OtherAddition = item.OtherAddition.ToString("0.00");
                toObj.Payments.Add(payment);
            }
        }
        public RegixPensionIncomeAmountVM GetPensionIncomeAmountReportById(int id)
        {
            var report = GetRegixReportById(id);
            RegixPensionIncomeAmountVM model = new RegixPensionIncomeAmountVM();
            SetRegixReportVM(report, model.Report);

            model.PensionIncomeAmountFilter = JsonConvert.DeserializeObject<RegixPensionIncomeAmountFilterVM>(report.RequestData);

            var response = JsonConvert.DeserializeObject<UP8ResponseType>(report.ResponseData);
            MapPensionIncomeAmountReport(response, model.PensionIncomeAmountResponse);
            return model;
        }


        public bool PersonalIdentityV2_SaveData(RegixPersonalIdentityV2VM model)
        {
            try
            {
                var response = GetPersonalIdentityV2(model.PersonalIdentityV2Filter.IdentityDocumentNumber, model.PersonalIdentityV2Filter.EGN);
                RegixReport saved = new RegixReport();
                if (RegixReport_SaveData(saved, model.Report, NomenclatureConstants.RegixType.PersonalIdentityV2, JsonConvert.SerializeObject(model.PersonalIdentityV2Filter),
                         JsonConvert.SerializeObject(response), true) == false)
                {
                    return false;
                }

                model.Report.Id = saved.Id;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на PersonalIdentityV2");
                return false;
            }
        }
        private void MapPersonalIdentityV2(PersonalIdentityInfoResponseType fromObj, RegixPersonalIdentityV2ResponseVM toObj)
        {
            toObj.InfoError = "";
            if (string.IsNullOrEmpty(fromObj.ReturnInformations.ReturnCode) == false && fromObj.ReturnInformations.ReturnCode != "0000")
            {
                if (fromObj.ReturnInformations.ReturnCode == "0100")
                    toObj.InfoError = "Няма данни отговарящи на условието";
                else
                    toObj.InfoError = fromObj.ReturnInformations.Info;
            }

            toObj.EGN = fromObj.EGN;
            toObj.PersonNames = (fromObj.PersonNames.FirstName ?? "") + " " + (fromObj.PersonNames.Surname ?? "") + " " + (fromObj.PersonNames.FamilyName ?? "");
            toObj.PersonNamesLatin = (fromObj.PersonNames.FirstNameLatin ?? "") + " " + (fromObj.PersonNames.SurnameLatin ?? "") + " " + (fromObj.PersonNames.LastNameLatin ?? "");
            toObj.DLCommonRestrictions = fromObj.DLCommonRestrictions;
            toObj.ForeignPIN = fromObj.DataForeignCitizen.PIN;
            toObj.ForeignPN = fromObj.DataForeignCitizen.PN;
            toObj.ForeignPersonNames = (fromObj.DataForeignCitizen.Names.FirstName ?? "") + " " + (fromObj.DataForeignCitizen.Names.Surname ?? "") + " " + (fromObj.DataForeignCitizen.Names.FamilyName ?? "");
            toObj.ForeignPersonNamesLatin = (fromObj.DataForeignCitizen.Names.FirstNameLatin ?? "") + " " + (fromObj.DataForeignCitizen.Names.SurnameLatin ?? "") + " " + (fromObj.DataForeignCitizen.Names.LastNameLatin ?? "");
            toObj.ForeignNationality = "";
            foreach (var item in fromObj.DataForeignCitizen.NationalityList)
            {
                if (toObj.ForeignNationality != "")
                    toObj.ForeignNationality += "; ";
                toObj.ForeignNationality += item.NationalityName;
            }
            toObj.ForeignGender = fromObj.DataForeignCitizen.Gender.Cyrillic;
            toObj.ForeignBirthDate = fromObj.DataForeignCitizen.BirthDateSpecified == true ? fromObj.DataForeignCitizen.BirthDate.ToString("dd.MM.yyyy") : "";

            toObj.RPRemarks = string.Join("; ", fromObj.RPRemarks);
            toObj.RPTypeofPermit = fromObj.RPTypeofPermit;
            toObj.DocumentType = fromObj.DocumentType;
            toObj.DocumentActualStatus = fromObj.DocumentActualStatus;
            toObj.ActualStatusDate = fromObj.ActualStatusDateSpecified == true ? fromObj.ActualStatusDate.ToString("dd.MM.yyyy") : "";
            toObj.DocumentStatusReason = fromObj.DocumentStatusReason;
            toObj.IdentityDocumentNumber = fromObj.IdentityDocumentNumber;
            toObj.IssueDate = fromObj.IssueDateSpecified == true ? fromObj.IssueDate.ToString("dd.MM.yyyy") : "";
            toObj.IssuerPlace = fromObj.IssuerPlace;
            toObj.IssuerName = fromObj.IssuerName;
            toObj.ValidDate = fromObj.ValidDateSpecified == true ? fromObj.ValidDate.ToString("dd.MM.yyyy") : "";
            toObj.BirthDate = fromObj.BirthDateSpecified == true ? fromObj.BirthDate.ToString("dd.MM.yyyy") : "";
            toObj.BirthPlace = fromObj.BirthPlace.CountryName + " " + (fromObj.BirthPlace.TerritorialUnitName ?? "");
            toObj.GenderName = fromObj.GenderName;

            toObj.Nationality = "";
            foreach (var item in fromObj.NationalityList)
            {
                if (toObj.Nationality != "")
                    toObj.Nationality += "; ";
                toObj.Nationality += item.NationalityName;
            }

            toObj.PermanentAddress = "";
            if (string.IsNullOrEmpty(fromObj.PermanentAddress.DistrictName) == false)
                toObj.PermanentAddress += (toObj.PermanentAddress != "" ? ", " : "") + "обл. " + fromObj.PermanentAddress.DistrictName;
            if (string.IsNullOrEmpty(fromObj.PermanentAddress.MunicipalityName) == false)
                toObj.PermanentAddress += (toObj.PermanentAddress != "" ? ", " : "") + "общ. " + fromObj.PermanentAddress.MunicipalityName;
            if (string.IsNullOrEmpty(fromObj.PermanentAddress.SettlementName) == false)
                toObj.PermanentAddress += (toObj.PermanentAddress != "" ? ", " : "") + "населено място " + fromObj.PermanentAddress.SettlementName;
            if (string.IsNullOrEmpty(fromObj.PermanentAddress.LocationName) == false)
                toObj.PermanentAddress += (toObj.PermanentAddress != "" ? ", " : "") + "" + fromObj.PermanentAddress.LocationName;
            if (string.IsNullOrEmpty(fromObj.PermanentAddress.BuildingNumber) == false)
                toObj.PermanentAddress += (toObj.PermanentAddress != "" ? ", " : "") + "номер " + fromObj.PermanentAddress.BuildingNumber;
            if (string.IsNullOrEmpty(fromObj.PermanentAddress.Entrance) == false)
                toObj.PermanentAddress += (toObj.PermanentAddress != "" ? ", " : "") + "вход " + fromObj.PermanentAddress.Entrance;
            if (string.IsNullOrEmpty(fromObj.PermanentAddress.Floor) == false)
                toObj.PermanentAddress += (toObj.PermanentAddress != "" ? ", " : "") + "етаж " + fromObj.PermanentAddress.Floor;
            if (string.IsNullOrEmpty(fromObj.PermanentAddress.Apartment) == false)
                toObj.PermanentAddress += (toObj.PermanentAddress != "" ? ", " : "") + "апартамент " + fromObj.PermanentAddress.Apartment;

            toObj.Height = fromObj.Height.ToString();
            toObj.EyesColor = fromObj.EyesColor;

            foreach (var item in fromObj.DLCategоries)
            {
                RegixPersonalIdentityV2CategоriesResponseVM category = new RegixPersonalIdentityV2CategоriesResponseVM();
                category.Category = item.Category;
                category.DateCategory = item.DateCategory.ToString("dd.MM.yyyy");
                category.EndDateCategory = item.EndDateCategorySpecified == true ? item.EndDateCategory.ToString("dd.MM.yyyy") : "";
                category.Restrictions = string.Join("; ", item.Restrictions);
                toObj.PersonalIdentityV2CategоriesResponse.Add(category);
            }
        }
        public RegixPersonalIdentityV2VM GetPersonalIdentityV2ById(int id)
        {
            var report = GetRegixReportById(id);
            RegixPersonalIdentityV2VM model = new RegixPersonalIdentityV2VM();
            SetRegixReportVM(report, model.Report);

            model.PersonalIdentityV2Filter = JsonConvert.DeserializeObject<RegixPersonalIdentityV2FilterVM>(report.RequestData);

            var response = JsonConvert.DeserializeObject<PersonalIdentityInfoResponseType>(report.ResponseData);
            MapPersonalIdentityV2(response, model.PersonalIdentityV2Response);
            return model;
        }

        public bool ActualStateV3_SaveData(RegixActualStateV3VM model)
        {
            try
            {
                var response = GetActualStateV3(model.ActualStateV3Filter.UIC);
                RegixReport saved = new RegixReport();
                if (RegixReport_SaveData(saved, model.Report, NomenclatureConstants.RegixType.ActualStateV3, JsonConvert.SerializeObject(model.ActualStateV3Filter),
                         JsonConvert.SerializeObject(response), true) == false)
                {
                    return false;
                }

                model.Report.Id = saved.Id;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на ActualStateV3");
                return false;
            }
        }

        private List<string> GetAllMainObject(Dictionary<string, string> jsonKeyValues)
        {
            List<string> addObject = new List<string>();
            //Взимат се всички главни обекти
            for (int index = 0; index < jsonKeyValues.Count; index++)
            {
                var item = jsonKeyValues.ElementAt(index);
                int secondUnderline = item.Key.IndexOf("_", 1);
                if (secondUnderline > 0)
                {
                    string obj = item.Key.Substring(0, secondUnderline + 1);
                    if (addObject.Contains(obj) == false)
                        addObject.Add(obj);
                }
            }
            return addObject;
        }
        private void MapUndefinedCode(List<RegixMapActualStateV3> mapActualState, RegixMapActualStateV3 model, Dictionary<string, string> jsonKeyValues)
        {
            string fields = "";
            string labels = "";
            List<string> addObject = GetAllMainObject(jsonKeyValues);
            for (int i = 0; i < addObject.Count; i++)
            {
                var mapCode = mapActualState
                         .Where(x => x.FieldIdent == addObject[i] &&
                         x.TypeField == NomenclatureConstants.RegixMapActualStateTypeField.FieldObject)
                         .FirstOrDefault();
                //Ако не намери някой обект описан да не прави нищо
                if (mapCode == null) return;
                fields += (string.IsNullOrEmpty(fields) == false ? "," : "") + mapCode.Fields;
                labels += (string.IsNullOrEmpty(labels) == false ? "," : "") + mapCode.Labels;
            }
            model.Fields = fields;
            model.Labels = labels;
        }

        private void MakeMapFieldsAndLabels(List<RegixMapActualStateV3> mapActualState, RegixMapActualStateV3 model, Dictionary<string, string> jsonKeyValues)
        {
            //Ако има настройки за този код и в тях има тип Обект да ги смени с данните за обекта
            if ((model.HasObject ?? false) == true && string.IsNullOrEmpty(model.Fields) == false)
            {
                var mapObjects = mapActualState
                       .Where(x => x.TypeField == NomenclatureConstants.RegixMapActualStateTypeField.FieldObject)
                       .ToList();

                string[] arrnames = (model.Fields ?? "").Split(",");
                string[] arrlabels = (model.Labels ?? "").Split(",");
                bool isChange = false;
                foreach (var item in mapObjects)
                {
                    for (int i = 0; i < arrnames.Length; i++)
                    {
                        if (arrnames[i] == item.FieldIdent)
                        {
                            isChange = true;
                            arrnames[i] = item.Fields;
                        }
                    }
                    for (int i = 0; i < arrlabels.Length; i++)
                    {
                        if (arrlabels[i] == item.FieldIdent)
                        {
                            isChange = true;
                            arrlabels[i] = item.Labels;
                        }
                    }
                }
                if (isChange)
                {
                    model.Fields = string.Join(",", arrnames);
                    model.Labels = string.Join(",", arrlabels);
                }
            }
            else
            {
                if (jsonKeyValues.Count > 1 && string.IsNullOrEmpty(model.Fields))
                    MapUndefinedCode(mapActualState, model, jsonKeyValues);
            }
        }

        private (bool result, string errorMessage) MapActualStateV3(ActualStateResponseV3 fromObj, RegixActualStateV3ResponseVM toObj)
        {
            List<RegixMapActualStateV3> mapActualState = repo.AllReadonly<RegixMapActualStateV3>().ToList();

            toObj.DeedStatus = GetDeedStatus(fromObj.Deed.DeedStatus.ToString());
            toObj.CompanyName = fromObj.Deed.CompanyName;
            toObj.UIC = fromObj.Deed.UIC;
            toObj.LegalForm = fromObj.Deed.LegalForm.ToString();
            toObj.CaseNo = fromObj.Deed.CaseNo;
            toObj.CaseYear = fromObj.Deed.CaseYear;
            toObj.CourtNo = fromObj.Deed.CourtNo;
            toObj.LiquidationOrInsolvency = fromObj.Deed.LiquidationOrInsolvencySpecified == true ? GetLiquidationOrInsolvency(fromObj.Deed.LiquidationOrInsolvency.ToString()) : "";
            toObj.DataFound = fromObj.DataFound;
            foreach (var subdeed in fromObj.Deed.Subdeeds.Subdeed)
            {
                RegixSubdeedVM subdeedVM = new RegixSubdeedVM();
                subdeedVM.SubUIC = subdeed.SubUIC;
                subdeedVM.SubUICType = GetSubUICType(subdeed.SubUICType.ToString());
                subdeedVM.SubdeedStatus = GetSubdeedStatusType(subdeed.SubdeedStatus.ToString());
                subdeedVM.SubUICName = subdeed.SubUICName;
                toObj.Subdeeds.Add(subdeedVM);
                foreach (var record in subdeed.Records)
                {
                    var mapFields = mapActualState.Where(x => x.FieldIdent == record.FieldIdent &&
                                     x.TypeField == NomenclatureConstants.RegixMapActualStateTypeField.FieldCode)
                                    .DefaultIfEmpty(new RegixMapActualStateV3()).FirstOrDefault();

                    //Ако е сетнато да не се показва този код да го прескача
                    if ((mapFields.ForDisplay ?? true) == false) continue;

                    Dictionary<string, string> jsonKeyValues = GetJsonKeyValues(record);
                    //Заместване на обектите с настроените променливи или ако няма настройка - опит за разпознаване
                    MakeMapFieldsAndLabels(mapActualState, mapFields, jsonKeyValues);

                    (bool locResult, string locErrorMessage) = MapCodeNameValue(record, subdeedVM, mapFields.Fields, mapFields.Labels, jsonKeyValues);
                    if (locResult == false)
                        return (result: locResult, errorMessage: locErrorMessage);
                }
            }
            return (result: true, errorMessage: "");
        }

        public (bool result, string errorMessage, RegixActualStateV3VM model) GetActualStateV3ById(int id)
        {
            var report = GetRegixReportById(id);
            RegixActualStateV3VM model = new RegixActualStateV3VM();
            SetRegixReportVM(report, model.Report);

            model.ActualStateV3Filter = JsonConvert.DeserializeObject<RegixActualStateV3FilterVM>(report.RequestData);

            var response = JsonConvert.DeserializeObject<ActualStateResponseV3>(report.ResponseData);

            if (response.Deed == null || response.DataFound == false)
                return (result: false, errorMessage: "Няма намерени данни", model: model);

            (bool locResult, string locErrorMessage) = MapActualStateV3(response, model.ActualStateV3Response);
            return (result: locResult, errorMessage: locErrorMessage, model: model);
        }

        private string GetDeedStatus(string deedStatus)
        {
            string result = "";
            switch (deedStatus)
            {
                case "N":
                    result = "Нова";
                    break;
                case "E":
                    result = "Пререгистрирана фирма по Булстат";
                    break;
                case "C":
                    result = "Нова партида затворена";
                    break;
                case "L":
                    result = "Пререгистрирана фирма по Булстат затворена";
                    break;
                default:
                    result = "";
                    break;
            }

            return result;
        }

        private string GetLiquidationOrInsolvency(string liquidationOrInsolvency)
        {
            string result = "";
            switch (liquidationOrInsolvency)
            {
                case "Liquidation":
                    result = "Ликвидация";
                    break;
                case "Insolvency":
                    result = "Несъстоятелност";
                    break;
                case "InsolvencySecIns":
                    result = "В несъстоятелност(на II инстанция)";
                    break;
                case "InsolvencyThirdIns":
                    result = "В несъстоятелност(на III инстанция)";
                    break;
                default:
                    result = "";
                    break;
            }

            return result;
        }

        private string GetSubUICType(string subUICType)
        {
            string result = "";
            switch (subUICType)
            {
                case "Item1":
                    result = "Основни данни";
                    break;
                case "Item2":
                    result = "Прокура";
                    break;
                case "Item3":
                    result = "Клонове";
                    break;
                case "Item4":
                    result = "Залог на дружествен дял";
                    break;
                case "Item5":
                    result = "Залог на търговско предприятие";
                    break;
                case "Item6":
                    result = "Запор върху дружествен дял";
                    break;
                case "Item7":
                    result = "Ликвидация";
                    break;
                case "Item500":
                    result = "Действителни собственици";
                    break;
                case "Item8":
                    result = "Прехвърляне";
                    break;
                case "Item9":
                    result = "Преобразуване";
                    break;
                case "Item10":
                    result = "Преустройство";
                    break;
                case "Item11":
                    result = "Несъстоятелност";
                    break;
                case "Item13":
                    result = "Обявени актове";
                    break;
                case "Item15":
                    result = "Специален предмет на дейност";
                    break;
                default:
                    result = "";
                    break;
            }

            return result;
        }

        private string GetSubdeedStatusType(string subdeedStatusType)
        {
            string result = "";
            switch (subdeedStatusType)
            {
                case "A":
                    result = "Активен";
                    break;
                case "C":
                    result = "Закрит";
                    break;
                default:
                    result = "";
                    break;
            }

            return result;
        }

        private string GetValueByCodeArray(Dictionary<string, string> jsonKeyValues, string names, string labels)
        {
            string value = "";
            string[] arrnames = (names ?? "").Split(",");
            string[] arrlabels = (labels ?? "").Split(",");
            for (int i = 0; i < arrnames.Length; i++)
            {
                if (jsonKeyValues.ContainsKey(arrnames[i]))
                {
                    if (!string.IsNullOrEmpty(value))
                        value += ", ";
                    value += arrlabels[i] + jsonKeyValues[arrnames[i]];
                }
                else
                {
                }
            }
            return value;
        }

        private Dictionary<string, string> GetJsonKeyValues(Record record)
        {
            Dictionary<string, string> jsonKeyValues = new Dictionary<string, string>();
            JArray parsedArray = JArray.Parse(record.RecordData.ToString());
            foreach (JObject parsedObject in parsedArray.Children<JObject>())
            {
                JsonUtils.ParseJsonProperties(parsedObject, "", jsonKeyValues, true);
            }
            return jsonKeyValues;
        }

        private (bool result, string errorMessage) MapCodeNameValue(Record record, RegixSubdeedVM subdeed, string fields, string labels, Dictionary<string, string> jsonKeyValues)
        {
            string value = "";
            if (string.IsNullOrEmpty(fields) == false)
            {
                value = GetValueByCodeArray(jsonKeyValues, fields, labels);
            }
            else
            {
                if (jsonKeyValues.Count > 1)
                    return (result: false, errorMessage: "Липсва настройка за " + record.FieldIdent);
            }

            if (string.IsNullOrEmpty(value) && jsonKeyValues.Count == 1)
            {
                value = jsonKeyValues.First().Value;
            }


            RegixRecordVM recordMap = new RegixRecordVM();
            recordMap.Name = record.MainField.MainFieldName + ":";
            recordMap.Value = value;

            subdeed.Records.Add(recordMap);
            return (result: true, errorMessage: "");
        }

        public IEnumerable<PersonSearchVM> PersonSearch(int uicType, string uic)
        {
            List<PersonSearchVM> result = new List<PersonSearchVM>();
            switch (uicType)
            {
                case NomenclatureConstants.UicTypes.EGN:
                    var responseNBD = GetPersonalData(uic);
                    if (responseNBD != null)
                    {
                        var itemNBD = new PersonSearchVM()
                        {
                            Uic = uic,
                            UicTypeId = uicType,
                            RegisterName = "Национална база данни",
                            FirstName = responseNBD.PersonNames.FirstName,
                            MiddleName = responseNBD.PersonNames.SurName,
                            FamilyName = responseNBD.PersonNames.FamilyName
                        };
                        result.Add(itemNBD);
                    }
                    //var adr = GetPermanentAddress(uic);
                    break;
                case NomenclatureConstants.UicTypes.EIK:
                    var responseTR = GetActualStateV3(uic);
                    if (responseTR != null && responseTR.Deed != null)
                    {
                        var itemTR = new PersonSearchVM()
                        {
                            Uic = uic,
                            UicTypeId = uicType,
                            RegisterName = "Търговски Регистър",
                            FullName = responseTR.Deed.CompanyName
                        };
                        result.Add(itemTR);
                    }
                    break;
                case NomenclatureConstants.UicTypes.Bulstat:
                    var responseBS = GetStateOfPlay(uic);
                    if (responseBS != null && responseBS.Subject != null)
                    {
                        var itemBS = new PersonSearchVM()
                        {
                            Uic = uic,
                            UicTypeId = uicType,
                            RegisterName = "Регистър БУЛСТАТ"
                        };
                        if (responseBS.Subject.NaturalPersonSubject != null)
                        {
                            itemBS.FullName = responseBS.Subject.NaturalPersonSubject.CyrillicName;
                            //string[] personNames = responseBS.Subject.NaturalPersonSubject.CyrillicName.Replace('-', ' ').Split(' ');

                            //if (personNames.Length > 0)
                            //{
                            //    itemBS.FirstName = personNames[0];
                            //    if (personNames.Length > 1)
                            //    {
                            //        itemBS.MiddleName = personNames[1];
                            //    }
                            //    if (personNames.Length > 2)
                            //    {
                            //        itemBS.FamilyName = personNames[2];
                            //    }
                            //    if (personNames.Length > 3)
                            //    {
                            //        itemBS.Family2Name = personNames[3];
                            //    }
                            //}
                        }
                        else
                        {
                            itemBS.FullName = responseBS.Subject.LegalEntitySubject.CyrillicFullName;
                        }
                        result.Add(itemBS);
                    }

                    break;
            }
            return result;
        }

        public bool StateOfPlay_SaveData(RegixStateOfPlayVM model)
        {
            try
            {
                var response = GetStateOfPlay(model.StateOfPlayFilter.UIC);
                RegixReport saved = new RegixReport();
                if (RegixReport_SaveData(saved, model.Report, NomenclatureConstants.RegixType.StateOfPlay, JsonConvert.SerializeObject(model.StateOfPlayFilter),
                         JsonConvert.SerializeObject(response), true) == false)
                {
                    return false;
                }

                model.Report.Id = saved.Id;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на StateOfPlay");
                return false;
            }
        }

        private string GetFromNomenclature(Nomenclatures nomenclature, string definitionCode, Infrastructure.Models.Regix.GetStateOfPlay.NomenclatureEntry nomenclatureCode)
        {
            string result = "";
            if (nomenclatureCode != null)
            {
                if (!string.IsNullOrEmpty(nomenclatureCode.Code))
                {
                    var item = nomenclature.SimpleNomenclature.Where(x => x.Definition.Code == definitionCode).FirstOrDefault();
                    if (item != null)
                    {
                        result = item.NomElement.Where(x => x.Code == nomenclatureCode.Code).Select(x => x.NameBG).DefaultIfEmpty("").FirstOrDefault();
                    }
                }
            }

            return result;
        }

        private string GetFromNomenclatureCountry(Nomenclatures nomenclature, Infrastructure.Models.Regix.GetStateOfPlay.NomenclatureEntry nomenclatureCode)
        {
            string result = "";

            if (nomenclatureCode != null)
            {
                if (!string.IsNullOrEmpty(nomenclatureCode.Code))
                {
                    result = nomenclature.CountryNomElement.Where(x => x.Code == nomenclatureCode.Code).Select(x => x.NameBG).DefaultIfEmpty("").FirstOrDefault();
                }
            }

            return result;
        }

        private string StateOfPlayMakeAddress(Nomenclatures nomenclature, Address address)
        {
            string result = "";
            if (address != null)
            {
                string country = GetFromNomenclatureCountry(nomenclature, address.Country);
                string location = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.EKATTE, address.Location);
                string region = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.EKATTERegion, address.Location);
                if (!string.IsNullOrEmpty(country))
                    result += country;
                if (!string.IsNullOrEmpty(address.PostalCode))
                    result += (result != "" ? ", " : "") + "пощенски код " + address.PostalCode;
                if (!string.IsNullOrEmpty(address.PostalBox))
                    result += (result != "" ? ", " : "") + "пощенска кутия " + address.PostalBox;
                if (!string.IsNullOrEmpty(address.ForeignLocation))
                    result += (result != "" ? ", " : "") + address.ForeignLocation;
                if (!string.IsNullOrEmpty(location))
                    result += (result != "" ? ", " : "") + location;
                if (!string.IsNullOrEmpty(region))
                    result += (result != "" ? ", " : "") + region;
                if (!string.IsNullOrEmpty(address.Street))
                    result += (result != "" ? ", " : "") + address.Street;
                if (!string.IsNullOrEmpty(address.StreetNumber))
                    result += (result != "" ? ", " : "") + "номер " + address.StreetNumber;
                if (!string.IsNullOrEmpty(address.Building))
                    result += (result != "" ? ", " : "") + "бл. " + address.Building;
                if (!string.IsNullOrEmpty(address.Entrance))
                    result += (result != "" ? ", " : "") + "вх. " + address.Entrance;
                if (!string.IsNullOrEmpty(address.Floor))
                    result += (result != "" ? ", " : "") + "ет. " + address.Floor;
                if (!string.IsNullOrEmpty(address.Apartment))
                    result += (result != "" ? ", " : "") + "ап. " + address.Apartment;
            }

            return result;
        }

        private string StateOfPlayMakeIdentificationDoc(Nomenclatures nomenclature, IdentificationDoc doc)
        {
            string result = "";
            if (doc != null)
            {
                string country = GetFromNomenclatureCountry(nomenclature, doc.Country);
                string docType = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.IdentificationDocType, doc.IDType);

                if (!string.IsNullOrEmpty(docType))
                    result += docType;
                if (!string.IsNullOrEmpty(doc.IDNumber))
                    result += (result != "" ? ", " : "") + "№ " + doc.IDNumber;
                if (!string.IsNullOrEmpty(doc.IssueDate))
                    result += (result != "" ? ", " : "") + "дата на издаване " + doc.IssueDate;
                if (!string.IsNullOrEmpty(doc.ExpiryDate))
                    result += (result != "" ? ", " : "") + "дата на валидност " + doc.ExpiryDate;
                if (!string.IsNullOrEmpty(country))
                    result += (result != "" ? ", " : "") + country;
            }

            return result;
        }

        private void MapStateOfPlaySubject(Nomenclatures nomenclature, Subject fromObj, RegixStateSubjectVM toObj)
        {
            if (fromObj == null) return;
            toObj.UIC = fromObj.UIC != null ? fromObj.UIC.UIC1 : "";
            toObj.SubjectType = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.SubjectType, fromObj.SubjectType);
            toObj.Remark = fromObj.Remark;

            if (fromObj.LegalEntitySubject != null)
            {
                toObj.StateLegalEntitySubject = new RegixStateLegalEntitySubjectVM();
                toObj.StateLegalEntitySubject.Country = GetFromNomenclatureCountry(nomenclature, fromObj.LegalEntitySubject.Country);
                toObj.StateLegalEntitySubject.LegalForm = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.LegalForm, fromObj.LegalEntitySubject.LegalForm);
                toObj.StateLegalEntitySubject.LegalStatute = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.LegalStatut, fromObj.LegalEntitySubject.LegalStatute);
                toObj.StateLegalEntitySubject.SubjectGroup = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.SubjectGroup, fromObj.LegalEntitySubject.SubjectGroup);
                toObj.StateLegalEntitySubject.CyrillicFullName = fromObj.LegalEntitySubject.CyrillicFullName;
                toObj.StateLegalEntitySubject.CyrillicShortName = fromObj.LegalEntitySubject.CyrillicShortName;
                toObj.StateLegalEntitySubject.LatinFullName = fromObj.LegalEntitySubject.LatinFullName;
                toObj.StateLegalEntitySubject.SubordinateLevel = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.SubordinateLevel, fromObj.LegalEntitySubject.SubordinateLevel);
                toObj.StateLegalEntitySubject.TRStatus = fromObj.LegalEntitySubject.TRStatus;
            }

            if (fromObj.NaturalPersonSubject != null)
            {
                toObj.StateNaturalPersonSubject = new RegixStateNaturalPersonSubjectVM();
                toObj.StateNaturalPersonSubject.Country = GetFromNomenclatureCountry(nomenclature, fromObj.NaturalPersonSubject.Country);
                toObj.StateNaturalPersonSubject.EGN = fromObj.NaturalPersonSubject.EGN;
                toObj.StateNaturalPersonSubject.LNC = fromObj.NaturalPersonSubject.LNC;
                toObj.StateNaturalPersonSubject.CyrillicName = fromObj.NaturalPersonSubject.CyrillicName;
                toObj.StateNaturalPersonSubject.LatinName = fromObj.NaturalPersonSubject.LatinName;
                toObj.StateNaturalPersonSubject.BirthDate = fromObj.NaturalPersonSubject.BirthDate;
                toObj.StateNaturalPersonSubject.IdentificationDoc = StateOfPlayMakeIdentificationDoc(nomenclature, fromObj.NaturalPersonSubject.IdentificationDoc);
            }

            if (fromObj.Addresses != null)
            {
                foreach (var item in fromObj.Addresses)
                {
                    RegixStateAddressVM addAddress = new RegixStateAddressVM();
                    addAddress.AddressType = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.AddressType, item.AddressType);
                    addAddress.AddressText = StateOfPlayMakeAddress(nomenclature, item);
                    toObj.StateAddress.Add(addAddress);
                }
            }

            if (fromObj.Communications != null)
            {
                foreach (var item in fromObj.Communications)
                {
                    RegixStateCommunicationsVM addCommunication = new RegixStateCommunicationsVM();
                    addCommunication.CommunicationType = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.CommunicationType, item.Type);
                    addCommunication.CommunicationText = item.Value;
                    toObj.StateCommunications.Add(addCommunication);
                }
            }
        }

        private string StateOfPlayMakeCase(Nomenclatures nomenclature, Case caseObj)
        {
            string result = "";

            if (caseObj != null)
            {
                string court = ""; //GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.EKATTE, address.Location.Code);
                if (!string.IsNullOrEmpty(caseObj.Number))
                    result += "номер на дело " + caseObj.Number;
                if (caseObj.YearSpecified)
                    result += (result != "" ? ", " : "") + "година " + caseObj.Year;
                if (!string.IsNullOrEmpty(court))
                    result += (result != "" ? ", " : "") + "съд " + court;
                if (!string.IsNullOrEmpty(caseObj.Batch))
                    result += (result != "" ? ", " : "") + "отделение " + caseObj.Batch;
                if (caseObj.RegisterSpecified)
                    result += (result != "" ? ", " : "") + "регистър " + caseObj.Register;
                if (!string.IsNullOrEmpty(caseObj.Chapter))
                    result += (result != "" ? ", " : "") + "том " + caseObj.Chapter;
                if (caseObj.PageNumberSpecified)
                    result += (result != "" ? ", " : "") + "страница " + caseObj.PageNumber;
            }

            return result;
        }

        private void MapStateOfPlayEvent(Nomenclatures nomenclature, Event fromObj, RegixStateEventVM toObj)
        {
            if (fromObj == null) return;
            toObj.EventType = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.EventType, fromObj.EventType);
            toObj.EventDate = fromObj.EventDate;
            toObj.LegalBase = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.LegalBase, fromObj.LegalBase);
            toObj.Case = StateOfPlayMakeCase(nomenclature, fromObj.Case);

            if (fromObj.Document != null)
            {
                toObj.DocumentType = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.DocumentType, fromObj.Document.DocumentType);
                toObj.DocumentNumber = fromObj.Document.DocumentNumber;
                toObj.DocumentDate = fromObj.Document.DocumentDate;
                toObj.ValidFromDate = fromObj.Document.ValidFromDate;
                toObj.DocumentName = fromObj.Document.DocumentName;
                toObj.AuthorName = fromObj.Document.AuthorName;
            }
        }

        private void MapStateOfPlayInstallment(Nomenclatures nomenclature, StateOfPlay fromObj, RegixStateOfPlayResponseVM toObj)
        {
            if (fromObj.Installments == null) return;
            foreach (var item in fromObj.Installments)
            {
                RegixStateInstallmentVM addInstallment = new RegixStateInstallmentVM();
                addInstallment.Count = item.Count.ToString();
                addInstallment.Amount = item.Amount.ToString("0.00");
                toObj.StateInstallment.Add(addInstallment);
            }
        }

        private void MapStateOfPlayOwnershipForm(Nomenclatures nomenclature, StateOfPlay fromObj, RegixStateOfPlayResponseVM toObj)
        {
            if (fromObj.OwnershipForms == null) return;
            foreach (var item in fromObj.OwnershipForms)
            {
                RegixStateOwnershipFormVM addOwner = new RegixStateOwnershipFormVM();
                addOwner.Form = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.OwnershipForm, item.Form);
                addOwner.Percent = item.Percent.ToString();
                toObj.StateOwnershipForm.Add(addOwner);
            }
        }

        private void MapStateOfPlayFundingSource(Nomenclatures nomenclature, StateOfPlay fromObj, RegixStateOfPlayResponseVM toObj)
        {
            if (fromObj.FundingSources == null) return;
            foreach (var item in fromObj.FundingSources)
            {
                RegixStateFundingSourceVM addFunding = new RegixStateFundingSourceVM();
                addFunding.Source = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.FundingSource, item.Source);
                addFunding.Percent = item.Percent.ToString();
                toObj.StateFundingSource.Add(addFunding);
            }
        }
        private void MapStateOfPlayManager(Nomenclatures nomenclature, StateOfPlay fromObj, RegixStateOfPlayResponseVM toObj)
        {
            if (fromObj.Managers == null) return;
            foreach (var item in fromObj.Managers)
            {
                RegixStateManagerVM addManager = new RegixStateManagerVM();
                addManager.Position = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.Position, item.Position);
                MapStateOfPlaySubject(nomenclature, item.RelatedSubject, addManager.RelatedSubject);

                if (item.RepresentedSubjects != null)
                {
                    foreach (var itemRepresented in item.RepresentedSubjects)
                    {
                        RegixStateSubjectVM addRepresent = new RegixStateSubjectVM();
                        MapStateOfPlaySubject(nomenclature, itemRepresented, addRepresent);
                        addManager.RepresentedSubjects.Add(addRepresent);
                    }
                }
                toObj.StateManager.Add(addManager);
            }
        }
        private void MapStateOfPlayPartner(Nomenclatures nomenclature, StateOfPlay fromObj, RegixStateOfPlayResponseVM toObj)
        {
            if (fromObj.Partners == null) return;
            foreach (var item in fromObj.Partners)
            {
                RegixStatePartnerVM addPartner = new RegixStatePartnerVM();
                addPartner.Role = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.Partner, item.Role);
                addPartner.Percent = item.Percent.ToString();
                addPartner.Amount = item.Amount.ToString("0.00");
                MapStateOfPlaySubject(nomenclature, item.RelatedSubject, addPartner.RelatedSubject);
                toObj.StatePartner.Add(addPartner);
            }
        }

        private void MapStateOfPlayAssignee(Nomenclatures nomenclature, StateOfPlay fromObj, RegixStateOfPlayResponseVM toObj)
        {
            if (fromObj.Assignee == null) return;
            toObj.StateAssignee.Type = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.Assignee, fromObj.Assignee.Type);

            if (fromObj.Assignee.RelatedSubjects == null) return;
            foreach (var item in fromObj.Assignee.RelatedSubjects)
            {
                RegixStateSubjectVM addSubject = new RegixStateSubjectVM();
                MapStateOfPlaySubject(nomenclature, item, addSubject);
                toObj.StateAssignee.RelatedSubject.Add(addSubject);
            }
        }

        private void MapStateOfPlayBelonging(Nomenclatures nomenclature, StateOfPlay fromObj, RegixStateOfPlayResponseVM toObj)
        {
            toObj.StateBelonging.Type = fromObj.Assignee != null ? GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.Belonging, fromObj.Assignee.Type) : "";
            if (toObj.StateBelonging != null)
            {
                MapStateOfPlaySubject(nomenclature, fromObj.Belonging?.RelatedSubject, toObj.StateBelonging.RelatedSubject);
            }
        }

        private void MapStateOfPlayCollectiveBodies(Nomenclatures nomenclature, StateOfPlay fromObj, RegixStateOfPlayResponseVM toObj)
        {
            if (fromObj.CollectiveBodies == null) return;
            foreach (var item in fromObj.CollectiveBodies)
            {
                RegixCollectiveBodiesVM addCollectiveBody = new RegixCollectiveBodiesVM();
                addCollectiveBody.Type = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.CollectiveBody, fromObj.Assignee.Type);

                if (item.Members != null)
                {
                    foreach (var itemMember in item.Members)
                    {
                        RegixStateManagerVM addMember = new RegixStateManagerVM();
                        addMember.Position = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.Position, itemMember.Position);
                        MapStateOfPlaySubject(nomenclature, itemMember.RelatedSubject, addMember.RelatedSubject);

                        if (itemMember.RepresentedSubjects != null)
                        {
                            foreach (var itemRepresented in itemMember.RepresentedSubjects)
                            {
                                RegixStateSubjectVM addRepresent = new RegixStateSubjectVM();
                                MapStateOfPlaySubject(nomenclature, itemRepresented, addRepresent);
                                addMember.RepresentedSubjects.Add(addRepresent);
                            }
                        }
                        addCollectiveBody.StateMembers.Add(addMember);
                    }
                }

                toObj.CollectiveBodies.Add(addCollectiveBody);
            }
        }

        private void MapStateOfPlayAdditionalActivities2008(Nomenclatures nomenclature, StateOfPlay fromObj, RegixStateOfPlayResponseVM toObj)
        {
            if (fromObj.AdditionalActivities2008 == null) return;
            foreach (var item in fromObj.AdditionalActivities2008)
            {
                string activity = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.MainActivity2008, item.KID2008);
                toObj.AdditionalActivities2008.Add(activity);
            }
        }

        private void MapStateOfPlayProfessions(Nomenclatures nomenclature, StateOfPlay fromObj, RegixStateOfPlayResponseVM toObj)
        {
            if (fromObj.Professions == null) return;
            foreach (var item in fromObj.Professions)
            {
                string addProf = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.Profession, item.Profession);
                toObj.Professions.Add(addProf);
            }
        }

        private void MapStateOfPlay(StateOfPlay fromObj, RegixStateOfPlayResponseVM toObj)
        {
            var nomenclature = FetchNomenclatures();

            if (fromObj.RepresentationType != null)
            {
                toObj.RepresentationType = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.RepresentationType, fromObj.RepresentationType.Type);
                toObj.RepresentationText = fromObj.RepresentationType.Description;
            }
            toObj.ScopeOfActivity = fromObj.ScopeOfActivity != null ? fromObj.ScopeOfActivity.Description : "";
            toObj.MainActivity2008 = fromObj.MainActivity2008 != null ? GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.MainActivity2008, fromObj.MainActivity2008.KID2008) : "";
            toObj.MainActivity2003 = fromObj.MainActivity2003 != null ? GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.MainActivity2003, fromObj.MainActivity2003.NKID2003) : "";

            if (fromObj.LifeTime != null)
            {
                toObj.LifeTimeDate = fromObj.LifeTime.Date;
                toObj.LifeTimeDescription = fromObj.LifeTime.Description;
            }

            toObj.AccountingRecordForm = fromObj.AccountingRecordForm != null ? GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.AccountingRecordForm, fromObj.AccountingRecordForm.Form) : "";
            toObj.State = fromObj.State != null ? GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.SubjectState, fromObj.State.State) : "";

            if (fromObj.ActivityDate != null)
            {
                toObj.ActivityType = GetFromNomenclature(nomenclature, RegixStateDefinitionConstants.ActivityType, fromObj.ActivityDate.Type);
                toObj.ActivityDate = fromObj.ActivityDate.Date;
            }

            MapStateOfPlaySubject(nomenclature, fromObj.Subject, toObj.StateSubject);
            MapStateOfPlayEvent(nomenclature, fromObj.Event, toObj.StateEvent);
            MapStateOfPlayInstallment(nomenclature, fromObj, toObj);
            MapStateOfPlayOwnershipForm(nomenclature, fromObj, toObj);
            MapStateOfPlayFundingSource(nomenclature, fromObj, toObj);
            MapStateOfPlayManager(nomenclature, fromObj, toObj);
            MapStateOfPlayPartner(nomenclature, fromObj, toObj);
            MapStateOfPlayAssignee(nomenclature, fromObj, toObj);
            MapStateOfPlayBelonging(nomenclature, fromObj, toObj);
            MapStateOfPlayCollectiveBodies(nomenclature, fromObj, toObj);
            MapStateOfPlayAdditionalActivities2008(nomenclature, fromObj, toObj);
        }

        public RegixStateOfPlayVM GetStateOfPlayById(int id)
        {
            var report = GetRegixReportById(id);
            RegixStateOfPlayVM model = new RegixStateOfPlayVM();
            SetRegixReportVM(report, model.Report);

            model.StateOfPlayFilter = JsonConvert.DeserializeObject<RegixStateOfPlayFilterVM>(report.RequestData);

            var response = JsonConvert.DeserializeObject<StateOfPlay>(report.ResponseData);
            MapStateOfPlay(response, model.StateOfPlayResponse);
            return model;
        }

        private RegixReport GetRegixReportById(int id)
        {
            return repo.AllReadonly<RegixReport>()
                          .Include(x => x.Court)
                          .Include(x => x.RegixType)
                          .Include(x => x.User)
                          .Include(x => x.User.LawUnit)
                          .Include(x => x.Case)
                          .Include(x => x.Document)
                          .Include(x => x.Document.DocumentType)
                          .Include(x => x.CaseSessionAct.CaseSession)
                          .Include(x => x.CaseSessionAct.CaseSession.SessionType)
                          .Include(x => x.CaseSessionAct.ActType)
                          .Where(x => x.Id == id).FirstOrDefault();
        }

        private void SetRegixReportVM(RegixReport model, RegixReportVM report)
        {
            report.CourtId = model.CourtId;
            report.Id = model.Id;
            report.CaseId = model.CaseId;
            report.CaseSessionActId = model.CaseSessionActId;
            report.DocumentId = model.DocumentId;
            report.Description = model.Description;
            report.HeaderFooter.CourtName = model.Court.Label;
            report.HeaderFooter.RegixTypeName = model.RegixType.Label;
            report.HeaderFooter.UserName = model.User.LawUnit.FullName;
            report.HeaderFooter.Date = model.DateWrt.ToString("dd.MM.yyyy HH:mm");

            if (model.CaseId != null)
            {
                report.HeaderFooter.CaseNumber = model.Case.RegNumber + "/" + model.Case.RegDate.ToString("dd.MM.yyyy");
            }

            if (model.CaseSessionActId != null)
            {
                report.HeaderFooter.CaseSessionActNumber = $"{model.CaseSessionAct.ActType.Label} {model.CaseSessionAct.RegNumber}/{model.CaseSessionAct.RegDate:dd.MM.yyyy} ({model.CaseSessionAct.CaseSession.SessionType.Label} {model.CaseSessionAct.CaseSession.DateFrom:dd.MM.yyyy})";
            }

            if (model.DocumentId != null)
            {
                report.HeaderFooter.DocumentNumber = model.Document.DocumentType.Label + model.Document.DocumentNumber + "/" + model.Document.DocumentDate.ToString("dd.MM.yyyy");
            }

        }

        public DocumentRegixVM GetPersonalIdentity(string identityDocumentNumber, string egn)
        {
            var personalIdentityInfoResponse = GetPersonalIdentityV2(identityDocumentNumber, egn);

            return new DocumentRegixVM()
            {
                DocumentValid = personalIdentityInfoResponse.DocumentActualStatus ?? "НЯМА ДАННИ ЗА ТОЗИ ДОКУМЕНТ",
                PersonName = personalIdentityInfoResponse.DocumentActualStatus != null ? "Име: " + personalIdentityInfoResponse.PersonNames.FirstName + " " + personalIdentityInfoResponse.PersonNames.Surname + " " + personalIdentityInfoResponse.PersonNames.FamilyName : null,
                PersonUic = personalIdentityInfoResponse.DocumentActualStatus != null ? "ЕГН: " + personalIdentityInfoResponse.EGN : null,
                DocumentType = personalIdentityInfoResponse.DocumentActualStatus != null ? "Вид документ: " + personalIdentityInfoResponse.DocumentType : null,
                DocumentNumber = personalIdentityInfoResponse.DocumentActualStatus != null ? "Номер: " + personalIdentityInfoResponse.IdentityDocumentNumber : null,
                DocumentDateFrom = personalIdentityInfoResponse.DocumentActualStatus != null ? "Дата на издаване: " + personalIdentityInfoResponse.ActualStatusDate.ToString("dd.MM.yyyy") : null,
                DocumentDateFromDate = personalIdentityInfoResponse.ActualStatusDate.ToString("dd.MM.yyyy"),
                DocumentDateTo = personalIdentityInfoResponse.DocumentActualStatus != null ? "Валидност: " + personalIdentityInfoResponse.ValidDate.ToString("dd.MM.yyyy") : null,
                DocumentDateToDate = personalIdentityInfoResponse.ValidDate.ToString("dd.MM.yyyy"),
                IssuerName = personalIdentityInfoResponse.DocumentActualStatus != null ? "Издадена от: " + personalIdentityInfoResponse.IssuerName : null
            };
        }

        /// <summary>
        /// Запис на заявката за Лице + адресите
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool PersonDataAddress_SaveData(RegixPersonDataAddressVM model)
        {
            try
            {
                RegixPersonAllDataResponse response = new RegixPersonAllDataResponse();
                response.PersonDataResponseType = GetPersonalData(model.PersonAddressFilter.EgnFilter);
                response.PermanentAddressResponseType = GetPermanentAddress(model.PersonAddressFilter.EgnFilter);
                response.TemporaryAddressResponseType = GetCurrentAddress(model.PersonAddressFilter.EgnFilter);
                string responseJson = JsonConvert.SerializeObject(response);

                RegixReport saved = new RegixReport();
                if (RegixReport_SaveData(saved, model.Report, NomenclatureConstants.RegixType.PersonDataAddress,
                         JsonConvert.SerializeObject(model.PersonAddressFilter), responseJson, true) == false)
                {
                    return false;
                }

                model.Report.Id = saved.Id;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на PersonDataAddress");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данните за Лице + адресите
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RegixPersonDataAddressVM GetPersonDataAddressById(int id)
        {
            var report = GetRegixReportById(id);
            RegixPersonDataAddressVM model = new RegixPersonDataAddressVM();
            SetRegixReportVM(report, model.Report);

            model.PersonAddressFilter = JsonConvert.DeserializeObject<RegixPersonAddressFilterVM>(report.RequestData);

            var personDataAddress = JsonConvert.DeserializeObject<RegixPersonAllDataResponse>(report.ResponseData);
            MapPersonData(personDataAddress.PersonDataResponseType, model.PersonDataResponse);
            MapPermanentAddress(personDataAddress.PermanentAddressResponseType, model.PermanentAddressResponse);
            MapCurrentAddress(personDataAddress.TemporaryAddressResponseType, model.TemporaryAddressResponse);

            return model;
        }

        /// <summary>
        /// Справка търсене във външни регистри
        /// </summary>
        /// <returns></returns>
        public IQueryable<RegixReportListVM> RegixReportList_Select(int courtId, RegixReportListFilterVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateNow = DateTime.Now;
            DateTime dateFromSearch = model.DateFrom ?? DateTime.Now.AddYears(-100);
            DateTime dateToSearch = model.DateTo ?? DateTime.Now.AddYears(100);

            Expression<Func<RegixReport, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.DateWrt.Date >= dateFromSearch.Date && x.DateWrt.Date <= dateToSearch.Date;

            Expression<Func<RegixReport, bool>> userWhere = x => true;
            if (string.IsNullOrEmpty(model.UserId) == false && model.UserId != "0")
                userWhere = x => x.UserId == model.UserId;

            Expression<Func<RegixReport, bool>> regixTypeWhere = x => true;
            if (model.RegixTypeId > 0)
                regixTypeWhere = x => x.RegixTypeId == model.RegixTypeId;

            return repo.AllReadonly<RegixReport>()
                                .Where(x => x.CourtId == courtId)
                                .Where(dateSearch)
                                .Where(userWhere)
                                .Where(regixTypeWhere)
                                .Select(x => new RegixReportListVM
                                {
                                    Id = x.Id,
                                    RegixTypeName = x.RegixType.Label,
                                    UserName = x.User.LawUnit.FullName,
                                    CaseRegNumber = x.Case.RegNumber,
                                    DocumentNumber = x.DocumentId != null ? (x.Document.DocumentType.Label + " " +
                                             x.Document.DocumentNumber + "/" +
                                             x.Document.DocumentDate.ToString(FormattingConstant.NormalDateFormat)) : "",
                                    DateWrt = x.DateWrt,
                                    Request = JsonConvert.DeserializeObject<RegixReportListRequestVM>(x.RequestData),
                                }).AsQueryable();
        }

        public IQueryable<RegixListVM> RegixListByCase_Select(int caseId)
        {
            return repo.AllReadonly<RegixReport>()
                       .Where(x => x.CaseId == caseId)
                       .Select(x => new RegixListVM
                       {
                           Id = x.Id,
                           CaseId = x.CaseId ?? 0,
                           RegixTypeName = x.RegixType.Label,
                           UserName = x.User.LawUnit.FullName,
                           CaseRegNumber = x.Case.RegNumber,
                           DocumentNumber = x.DocumentId != null ? (x.Document.DocumentType.Label + " " +
                                                                    x.Document.DocumentNumber + "/" +
                                                                    x.Document.DocumentDate.ToString(FormattingConstant.NormalDateFormat)) : string.Empty,
                           ActRegNumber = x.CaseSessionActId != null ? (x.CaseSessionAct.ActType.Label + " " + x.CaseSessionAct.RegNumber + "/" + (x.CaseSessionAct.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy")) : string.Empty,
                           DateWrt = x.DateWrt,
                       }).AsQueryable();
        }
    }
}
