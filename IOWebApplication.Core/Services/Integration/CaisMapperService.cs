using Integration.Cais;
using IOWebApplication.Core.Contracts.Integration;
using IOWebApplication.Core.Extensions.XML;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Models.ViewModels.Integrations;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace IOWebApplicationService.Infrastructure.Services.Intergation
{
    public class CaisMapperService : ICaisMapperService
    {
        public CaisMapperService()
        {
        }

        public SendBulletinsDataRequestType MapData(CaisBuletinModel model)
        {

            var result = new SendBulletinsDataRequestType();
            result.BulletinsList = new BulletinsList()
            {
                Bulletin = new BulletinType[] { mapBulletinType(model) }
            };

            return result;
        }

        public string GetXml(CaisBuletinModel model)
        {
            var request = MapData(model);
            string result = "";

            using (var stream = new MemoryStream())
            {
                System.Text.Encoding encoding;
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true
                };
                using (var writer = XmlWriter.Create(stream, settings))
                {
                    if (writer == null)
                    {
                        throw new InvalidOperationException("writer is null");
                    }

                    encoding = writer.Settings.Encoding;
                    var ser = new XmlSerializer(request.GetType());
                    ser.Serialize(writer, request);
                }

                stream.Position = 0;
                using (var reader = new StreamReader(stream, encoding, true))
                {
                    result = reader.ReadToEnd();
                }
            }

            //FIX Element Names


            result = result.Replace("<SendBulletinsDataRequestType", "<SendBulletinsDataRequest xmlns=\"http://cs.mjs.bg/EISSServicesModel-v1.0\" ");
            result = result.Replace("SendBulletinsDataRequestType", "SendBulletinsDataRequest");
            return result;

            //var xml = XmlUtils.SerializeObject(request, true);
            //return xml;
        }

        BulletinType mapBulletinType(CaisBuletinModel model)
        {
            var bulletinType = new BulletinType();
            bulletinType.Type = BulletinTypesType.ConvictionBulletin;
            if (model.BuletinInfo.IsAdministrative78a)
            {
                bulletinType.Type = BulletinTypesType.Bulletin78A;
            }

            bulletinType.Person = mapPersonType(model);

            bulletinType.Conviction = mapConvictionType(model);

            bulletinType.IssuerData = mapIssuerData(model.Context);

            bulletinType.RegistrationData = mapRegistrationData(model);

            return bulletinType;
        }

        IssuerData mapIssuerData(CaisBuletinContextModel context)
        {
            var issuerData = new IssuerData();
            issuerData.BulletinCreatorAuthority = new DecidingAuthorityType()
            {
                DecidingAuthorityCodeEIK = context.AuthorityEIK,
                DecidingAuthorityCodeEISPP = context.AuthorityEISPP,
                DecidingAuthorityName = context.AuthorityName
            };
            issuerData.BulletinCreateDate = context.CreateDate;
            issuerData.BulletinCreatorPerson = new OfficialPersonType()
            {
                Names = mapPersonNameType(context.CreatorPerson, false),
                Position = context.CreatorPerson.PositionName
            };
            issuerData.BulletinApproverPerson = new OfficialPersonType()
            {
                Names = mapPersonNameType(context.ApproverPerson, false),
                Position = context.ApproverPerson.PositionName
            };
            return issuerData;
        }

        RegistrationData mapRegistrationData(CaisBuletinModel model)
        {
            RegistrationData registrationData = new RegistrationData();
            registrationData.RegistrationNumber = model.BuletinInfo.BuletinNumber;
            registrationData.BulletinReceivedDateSpecified = false;
            return registrationData;
        }

        ConvictionType mapConvictionType(CaisBuletinModel model)
        {
            var convictionType = new ConvictionType();
            convictionType.Decision = mapDecisionActType(model.Act);
            convictionType.CriminalCase = mapCriminalCaseType(model.Case);
            convictionType.ConvictionOffence = model.Crimes.Select(x => mapOffenceType(x)).ToArray();
            convictionType.EisppNumber = model.BuletinInfo.NumberEISPP;
            convictionType.WithoutSanction = (model.Crimes.Count() == 0) || !model.Crimes.Any(c => c.NotPunished == false);
            convictionType.WithoutSanctionSpecified = true;

            return convictionType;
        }

        OffenceType mapOffenceType(CaisBuletinCrimeModel crime)
        {
            OffenceType offenceType = new OffenceType();
            offenceType.OffenceId = crime.CaseCrimeId.ToString();
            offenceType.NationalCategoryCode = crime.CategoryDeedCode;
            offenceType.NationalCategoryTitle = crime.CategoryDeedName;
            offenceType.Remarks = crime.CrimeDescription.ClearXmlText();
            offenceType.OffenceCommonCategoryReference = new OffenceCommonCategoryType()
            {
                OffenceCode = crime.CategoryCommonDeedCode,
                OffenceName = crime.CategoryCommonDeedName
            };
            offenceType.OffenceApplicableLegalProvisions = crime.LegalQualificationText;
            offenceType.OffenceStartDate = mapDateTimeType(crime.DateFrom);
            offenceType.OffenceEndDate = mapDateTimeType(crime.DateFrom);
            offenceType.OffencePlace = mapPlaceType(crime.Address);
            offenceType.FormOfGuiltSpecified = true;
            switch (crime.FormOfGuiltId)
            {
                case 1:
                    offenceType.FormOfGuilt = FormOfGuiltType.intentionally;
                    break;
                case 2:
                    offenceType.FormOfGuilt = FormOfGuiltType.recklessly;
                    break;
                default:
                    offenceType.FormOfGuiltSpecified = false;
                    break;
            }
            offenceType.ConvictionSanction = crime.Punishmets.Select(x => mapSanctionType(x)).ToArray();
            offenceType.EisppCode = crime.EISSPNumber;
            switch (crime.RecidiveTypeId)
            {
                case 1:
                    offenceType.OffenceRecidivism = true;
                    offenceType.OffenceRecidivismSpecified = true;
                    offenceType.OffenceRecidivismType = OffenceRecidivismType.common;
                    offenceType.OffenceRecidivismTypeSpecified = true;
                    break;
                case 2:
                    offenceType.OffenceRecidivism = true;
                    offenceType.OffenceRecidivismSpecified = true;
                    offenceType.OffenceRecidivismType = OffenceRecidivismType.dangerous;
                    offenceType.OffenceRecidivismTypeSpecified = true;
                    break;
                case 3:
                    offenceType.OffenceRecidivism = true;
                    offenceType.OffenceRecidivismSpecified = true;
                    offenceType.OffenceRecidivismType = OffenceRecidivismType.special;
                    offenceType.OffenceRecidivismTypeSpecified = true;
                    break;
                case 4:
                    offenceType.OffenceRecidivism = false;
                    offenceType.OffenceRecidivismSpecified = true;
                    offenceType.OffenceRecidivismTypeSpecified = false;
                    break;
                default:
                    offenceType.OffenceRecidivismSpecified = false;
                    offenceType.OffenceRecidivismTypeSpecified = false;
                    break;

            }

            return offenceType;
        }

        SanctionType mapSanctionType(CaisBuletinPunishmentModel model)
        {
            SanctionType sanctionType = new SanctionType();
            sanctionType.SanctionId = model.Id.ToString();

            // Размяна на полета стар вариант
            //sanctionType.NationalCategoryCode = model.PunishmentGeneralCategoryCode;
            //sanctionType.NationalCategoryTitle = model.PunishmentGeneralCategoryName;

            // Размяна на полета нов вариант
            sanctionType.NationalCategoryCode = model.SentenceTypeCode2;
            sanctionType.NationalCategoryTitle = model.SentenceTypeName;

            sanctionType.Remarks = model.Description.ClearXmlText();
            sanctionType.SanctionCommonCategoryReference = new SanctionCommonCategoryType()
            {
                // Размяна на полета стар вариант
                //SanctionCode = model.SentenceTypeCode,
                //SanctionText = model.SentenceTypeName
                SanctionCode = model.PunishmentGeneralCategoryCode,
                SanctionText = model.PunishmentGeneralCategoryName
            };
            sanctionType.Fine = mapSanctionTypeFine(model);

            sanctionType.Probation = model.Probations.Select(p => mapSanctionTypeProbation(p)).ToArray();

            if (model.YesLOS)
            {
                sanctionType.Prison = new SanctionTypePrison()
                {
                    SanctionSentencedPeriod = xmlDuration(model.SentenceYears, model.SentenceMonths, model.SentenceWeeks, model.SentenceDays),
                    SanctionSuspension = xmlDuration(model.ProbationYears, model.ProbationMonths, null, model.ProbationDays),
                    DetentionDescription = xmlDuration(model.PreliminaryDetentionYears, model.PreliminaryDetentionMonths, model.PreliminaryDetentionWeeks, model.PreliminaryDetentionDays)
                };
            }

            return sanctionType;
        }

        string xmlDuration(int? years, int? months, int? weeks, int? days)
        {
            string result = "P";
            if (years.HasValue)
            {
                result += $"{years.Value}Y";
            }
            if (months.HasValue)
            {
                result += $"{months.Value}M";
            }
            if (days.HasValue)
            {
                result += $"{((weeks ?? 0) * 7) + days.Value}D";
            }
            else
            {
                if (weeks.HasValue)
                {
                    result += $"{(weeks.Value) * 7}D";
                }
            }
            return result;
        }

        SanctionTypeProbation mapSanctionTypeProbation(CaisBuletinProbationModel model)
        {
            SanctionTypeProbation sanctionTypeProbation = new SanctionTypeProbation();
            sanctionTypeProbation.ProbationId = model.Id.ToString();

            sanctionTypeProbation.ProbationCategoryCode = model.MeasureTypeCode;
            sanctionTypeProbation.ProbationCategoryTitle = model.MeasureTypeName;

            sanctionTypeProbation.ProbationMeasureCode = model.MeasureUnitCode;
            sanctionTypeProbation.ProbationMeasureTitle = model.MeasureUnitName;
            sanctionTypeProbation.ProbationValue = model.MeasureQuantity ?? 0;

            sanctionTypeProbation.SanctionSentencedPeriod = xmlDuration(model.ProbationYears, model.ProbationMonths, model.ProbationWeeks, model.ProbationDays);

            return sanctionTypeProbation;
        }

        SanctionTypeFine mapSanctionTypeFine(CaisBuletinPunishmentModel model)
        {
            SanctionTypeFine sanctionTypeFine = new SanctionTypeFine();
            if (model.ChargeAmount > 0)
            {
                sanctionTypeFine.SanctionAmountOfIndividualFine = model.ChargeAmount.Value;
                sanctionTypeFine.SanctionCurrencyOfFine = CurrencyType.BGN;
                sanctionTypeFine.SanctionCurrencyOfFineSpecified = true;
            }
            if (model.ChargeAmountEUR > 0)
            {
                sanctionTypeFine.SanctionAmountOfIndividualFine = model.ChargeAmountEUR.Value;
                sanctionTypeFine.SanctionCurrencyOfFine = CurrencyType.EUR;
                sanctionTypeFine.SanctionCurrencyOfFineSpecified = true;
            }



            return sanctionTypeFine;
        }


        CriminalCaseType mapCriminalCaseType(CaisBuletinCaseModel model)
        {
            CriminalCaseType criminalCaseType = new CriminalCaseType()
            {
                CaseNumber = model.CaseNumber,
                CaseYear = model.CaseYear.ToString(),
                CaseType = mapCaseType(model.CaseTypeId),
                CaseTypeSpecified = true,
                CaseAuthority = mapDecidingAuthorityType(model.Court)
            };
            return criminalCaseType;
        }

        CaseType mapCaseType(int caseTypeId)
        {
            //TODO
            switch (caseTypeId)
            {
                case NomenclatureConstants.CaseTypes.NOHD:
                case NomenclatureConstants.CaseTypes.VNOHD:
                case NomenclatureConstants.CaseTypes.KNOHD:
                    return CaseType.sign_noxd;
                case NomenclatureConstants.CaseTypes.NChHD:
                case NomenclatureConstants.CaseTypes.VNChHD:
                case NomenclatureConstants.CaseTypes.KNChHD:
                    return CaseType.sign_ncxd;

                case NomenclatureConstants.CaseTypes.ChND:
                    return CaseType.sign_ncd;

                case NomenclatureConstants.CaseTypes.AND:
                case NomenclatureConstants.CaseTypes.VAND:
                case NomenclatureConstants.CaseTypes.KAND:
                    return CaseType.sign_and;
                default:
                    return CaseType.sign_null;
            }
        }

        PersonType mapPersonType(CaisBuletinModel model)
        {
            var personType = new PersonType();

            personType.NamesBg = mapPersonNameType(model.MainPerson, false);
            personType.NamesEn = mapPersonNameType(model.MainPerson, true);
            if (model.MotherNames != null)
            {
                personType.MotherNames = mapPersonNameType(model.MotherNames, false);
                personType.MotherNamesEn = mapPersonNameType(model.MotherNames, true);
            }
            if (model.FatherNames != null)
            {
                personType.FatherNames = mapPersonNameType(model.FatherNames, false);
                personType.FatherNamesEn = mapPersonNameType(model.FatherNames, true);
            }
            personType.SexSpecified = true;
            switch (model.MainPerson.GenderId)
            {
                case 1:
                    personType.Sex = 2;
                    break;
                case 2:
                    personType.Sex = 1;
                    break;
                default:
                    personType.SexSpecified = false;
                    break;
            }
            var identityNumber = new PersonIdentityNumberType();
            switch (model.MainPerson.UicTypeId)
            {
                case NomenclatureConstants.UicTypes.EGN:
                    identityNumber.EGN = model.MainPerson.UIC;
                    break;
                case NomenclatureConstants.UicTypes.LNCh:
                    identityNumber.LNCh = model.MainPerson.UIC;
                    break;
                default:
                    identityNumber.SUID = model.MainPerson.UIC;
                    break;

            }
            personType.IdentityNumber = identityNumber;
            personType.BirthDate = mapDateType(model.MainPerson.DateOfBirth);
            personType.BirthPlace = mapPlaceType(model.MainPersonBirthPlace);
            personType.PersonNationality = model.Citizenship.Select(x => mapCountryType(x)).ToArray();
            personType.PersonIdentificationDocument = mapIdentificationDocumentType(model);
            personType.AFISNumber = model.BuletinInfo.NumberAFIS;
            return personType;
        }

        PlaceType mapPlaceType(CaisBuletinAddressModel address)
        {
            PlaceType placeType = new PlaceType();
            if (!string.IsNullOrEmpty(address.CityEkatte))
            {
                placeType.City = new CityType()
                {
                    EKATTECode = address.CityEkatte,
                    CityName = address.CityName
                };
                placeType.Descr = address.CityDescription.ClearXmlText();
            }
            else
            {
                placeType.DescrEn = address.CityDescription.ClearXmlText();
            }
            placeType.Country = mapCountryType(address);
            return placeType;
        }

        IdentificationDocumentType mapIdentificationDocumentType(CaisBuletinModel model)
        {
            if (model.IdentityDocuments == null || model.IdentityDocuments.Length == 0)
            {
                return null;
            }
            var document = model.IdentityDocuments[0];
            var identificationDocumentType = new IdentificationDocumentType();

            identificationDocumentType.IdentificationDocumentIssuingAuthority = document.Issuer;
            identificationDocumentType.IdentificationDocumentNumber = document.Number;
            identificationDocumentType.IdentificationDocumentIssuingDate = mapDateType(document.DateIssue);
            identificationDocumentType.IdentificationDocumentValidUntil = mapDateType(document.ValidTo);
            identificationDocumentType.IdentificationDocumentType1 = document.TypeName;
            identificationDocumentType.IdentificationDocumentCategoryReference = mapIdentificationDocumentCategoryType(document.PersonalDocumentId);
            if (document.Country != null)
            {
                identificationDocumentType.IssuingCountry = mapCountryType(document.Country);
            }


            return identificationDocumentType;
        }

        /*
IdentificationDocumentCategoryType - Категории документ за самоличност
ID00001 - Лична карта - National identity card
ID00002 - Удостоверение за гражданска регистрация - Civil register certificate
ID31003 - Дипломатически паспорт - Diplomatic passport
ID32003 - Моряшки паспорт - Seaman's passport
ID33003 - Служебен паспорт - Official passport
ID00003 - Паспорт - Passport
ID00004 - Шофьорска книжка - Driving licence
ID00005 - Карта за социално осигуряване - Social security card
ID61006 - Временен паспорт за окончателно напускане на РБ - Temporary passport for final departure of country
ID00006 - Временна лична карта - Temporary identity card
ID00007 - Разрешение за пребиваване - Residence permit
ID00999 - Друг документ за самоличност - Other identification document
ID91999 - Служебен открит лист за преминаване на границата - Official open sheet for crossing the border
         */

        IdentificationDocumentCategoryType mapIdentificationDocumentCategoryType(string eisppDocumenTypeId)
        {
            switch (eisppDocumenTypeId)
            {
                case "111": return IdentificationDocumentCategoryType.ID00001;
                case "99001": return IdentificationDocumentCategoryType.ID00002;
                case "99002": return IdentificationDocumentCategoryType.ID00003;
                case "99003": return IdentificationDocumentCategoryType.ID00004;
                case "99004": return IdentificationDocumentCategoryType.ID00005;
                case "99005": return IdentificationDocumentCategoryType.ID00006;
                case "99006": return IdentificationDocumentCategoryType.ID00007;

                case "99999": return IdentificationDocumentCategoryType.ID00999;
                default: return IdentificationDocumentCategoryType.ID00999;
            }
        }


        DateType mapDateType(DateTime? date)
        {
            var dateType = new DateType();
            if (date != null)
            {
                dateType.Date = date.Value;
                dateType.DatePrecision = DatePrecisionEnum.YMD;
                dateType.DatePrecisionSpecified = true;
            }
            else
            {
                //TODO: 
            }
            return dateType;
        }
        DateTimeType mapDateTimeType(DateTime? date)
        {
            var dateTimeType = new DateTimeType();
            if (date != null)
            {
                dateTimeType.Date = date.Value;
                dateTimeType.DatePrecision = DatePrecisionEnum.YMD;
                dateTimeType.DatePrecisionSpecified = true;
            }
            else
            {
                //TODO: 
            }
            return dateTimeType;
        }

        CountryType mapCountryType(CaisBuletinAddressModel address)
        {
            var countryType = new CountryType()
            {
                CountryISONumber = address.CountryCodeD,
                CountryISOAlpha3 = address.CountryCodeL,
                CountryName = address.CountryName
            };

            return countryType;
        }

        PersonNameType mapPersonNameType(CaisBuletinPersonModel personModel, bool latinNames)
        {
            var personNameType = new PersonNameType();

            if (latinNames)
            {
                personNameType.FirstName = personModel.LatinFirstName;
                personNameType.SurName = personModel.LatinMiddleName;
                personNameType.FamilyName = personModel.LatinFamilyName;
                personNameType.FullName = personModel.LatinFullName;
            }
            else
            {
                personNameType.FirstName = personModel.FirstName;
                personNameType.SurName = personModel.MiddleName;
                personNameType.FamilyName = personModel.FamilyName;
                personNameType.FullName = personModel.FullName;
            }

            return personNameType;
        }

        DecisionActType mapDecisionActType(CaisBuletinActModel act)
        {

            var result = new DecisionActType()
            {
                FileNumber = act.ActNumber,
                DecidingAuthority = mapDecidingAuthorityType(act.Court),
                DecisionType = mapDecisionType(act.ActTypeId),
                DecisionTypeSpecified = true,
                ECLI = act.ECLInumber
            };


            result.DecisionDate = act.ActDate;
            result.DecisionDateSpecified = true;

            if (act.ActInforceDate.HasValue)
            {
                result.DecisionFinalDate = act.ActInforceDate.Value;
                result.DecisionFinalDateSpecified = true;
            }
            return result;
        }

        DecidingAuthorityType mapDecidingAuthorityType(CaisBuletinCourtModel court)
        {
            DecidingAuthorityType decidingAuthority = new DecidingAuthorityType()
            {
                DecidingAuthorityName = court.CourtName,
                DecidingAuthorityCodeEISPP = court.CourtEispp
            };
            return decidingAuthority;
        }

        DecisionTypeCategories mapDecisionType(int actTypeId)
        {
            switch (actTypeId)
            {
                case NomenclatureConstants.ActType.Sentence:
                    return DecisionTypeCategories.dkp_prisada;
                case NomenclatureConstants.ActType.Answer:
                    return DecisionTypeCategories.dkp_reshenie;
                case NomenclatureConstants.ActType.Definition:
                    return DecisionTypeCategories.dkp_opredelenie;
                case NomenclatureConstants.ActType.Agreement:
                    return DecisionTypeCategories.dkp_sporazumenie;
                default:
                    return DecisionTypeCategories.dkp_null;
            }
        }
    }
}

/*
OffenceRecidivismType - Вид рецидив
common - общ
special - специален
dangerous - опасен

DatePrecisionEnum - Точност на дата
Y - Година
YM - Година, месец
YMD - Точна дата

FormOfGuiltType - Форма на вината
intentionally - Умишлено
recklessly - Непредпазливо

IdentifierType - Вид идентификатор
EGN - ЕГН
LNCH - ЛНЧ
LN - ЛН
SUID - Системно ID
SYSID - Вътрешно ID

DecisionChangeTypeType - Категории допълнителни сведения
DCH-00-R - Реабилитация
DCH-00-N - Край на изтърпяването на наказанието
DCH-00-E - Замяна на наказание/мярка
DCH-00-I - Последващо определяне на едно общо наказание
DCH-00-Q - Допуснато условно предсрочно освобождаване
DCH-00-S - Отмяна на условно предсрочно освобождаване
DCH-00-Y - Постановен съдебен акт по чл. 425 НПК
DCH-88-B - Отменена кумулация
DCH-00-X - Друго
DCH-88-A - Погасено по давност по наказание - изтича срока чл. 82 от НК
DCH-00-P - Амнистия
DCH-00-O - Помилване

IdentificationDocumentCategoryType - Категории документ за самоличност
ID-00-001 - Лична карта - National identity card
ID-00-002 - Удостоверение за гражданска регистрация - Civil register certificate
ID-31-003 - Дипломатически паспорт - Diplomatic passport
ID-32-003 - Моряшки паспорт - Seaman's passport
ID-33-003 - Служебен паспорт - Official passport
ID-00-003 - Паспорт - Passport
ID-00-004 - Шофьорска книжка - Driving licence
ID-00-005 - Карта за социално осигуряване - Social security card
ID-61-006 - Временен паспорт за окончателно напускане на РБ - Temporary passport for final departure of country
ID-00-006 - Временна лична карта - Temporary identity card
ID-00-007 - Разрешение за пребиваване - Residence permit
ID-00-999 - Друг документ за самоличност - Other identification document
ID-91-999 - Служебен открит лист за преминаване на границата - Official open sheet for crossing the border

SexType - The definition of types of human genders, based on the ISO 5218 standard. The values are restricted to 0 = unknown, 1 = male and 2 = female.
0 - unknown
1 - male
2 - female

DecisionTypeCategories - Вид на акта- Присъда;Решение;Определение;Споразумение
dkp_prisada         - Присъда
dkp_reshenie        - Решение
dkp_opredelenie     - Определение
dkp_sporazumenie    - Споразумение
dkp_null            - Неопределено

CaseType - Вид на делото - Наказателно от общ характер;Наказателно от частен характер;Наказателно административен характер;Частно наказателно дело;Административно наказателно дело
sign_noxd - НОХД
sign_ncxd - НЧХД
sign_naxd - НАХД
sign_ncd  - НЧД
sign_and  - АНД
sign_null - Неизвестно

BulletinTypesType   - Вид на бюлетин
ConvictionBulletin  - Бюлетин за съдимост
Bulletin78A         - Бюлетин за наложени административни наказания по чл. 78а от НК
Unspecified         - Неопределен

NameTypesType   - Вид на име
nickname        - псевдоним
previous        - предишно име
maiden          - моминско име
 */ 