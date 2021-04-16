using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.Eispp.ActualData;
using IOWebApplication.Infrastructure.Models.ViewModels.Eispp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using System;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Extensions;
using System.Linq.Expressions;
using IOWebApplication.Infrastructure.Models.Integrations.Eispp;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using Microsoft.EntityFrameworkCore;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Newtonsoft.Json;
using IOWebApplication.Infrastructure.Utils;
using System.Text;
using IOWebApplication.Infrastructure.Data.Models.EISPP;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using static IOWebApplication.Infrastructure.Constants.EISPPConstants;
using static IOWebApplication.Infrastructure.Constants.EpepConstants;

namespace IOWebApplication.Core.Services
{
    public class EisppService : BaseIntegrationService, IEisppService
    {
        private readonly IConfiguration configuration;
        private readonly INomenclatureService nomService;
        private readonly IMQEpepService mqService;
        private readonly ICdnService cdnService;
        private readonly IEisppRulesService rulesService;
        private readonly ICounterService counterService;
        private int minSidVal = -30;
        
        public EisppService(
            ILogger<DocumentService> _logger,
            IRepository _repo,
            IConfiguration _configuration,
            INomenclatureService _nomService,
            IMQEpepService _mqService,
            ICdnService _cdnService,
            IEisppRulesService _rulesService,
            IUserContext _userContext,
            ICounterService _counterService)
        {
            logger = _logger;
            repo = _repo;
            configuration = _configuration;
            nomService = _nomService;
            mqService = _mqService;
            cdnService = _cdnService;
            rulesService = _rulesService;
            userContext = _userContext;
            counterService = _counterService;
        }
        public const bool ShowCodeInDDL = false;
        public string GetElement(string tblCode, string code)
        {
            var element = repo.GetByIds<EisppTblElement>(new[] { tblCode, code });
            if (element != null)
            {
                return element.Label;
            }

            return "n/a";
        }


        public async Task<EisppTSActualDataVM> GetActualData(string eisppNumber, bool readFromMongoIfFail = true)
        {
            string serviceUrl = configuration.GetValue<string>("Eispp:TS_ActualDataURI");
            TSAKTSTSWebServicePortClient client = new TSAKTSTSWebServicePortClient(TSAKTSTSWebServicePortClient.EndpointConfiguration.TSAKTSTSWebServicePortSoap11, serviceUrl);
            client.Endpoint.Binding.SendTimeout = new TimeSpan(0, 2, 0);
            client.Endpoint.Binding.ReceiveTimeout = new TimeSpan(0, 2, 0);
            string eisppCode = repo.GetById<Court>(userContext.CourtId)?.EISPPCode;

            var request = new execTSAKTSTSRequest()
            {
                ZQKAKTSTS = new ZQKNPRAKTSTSType()
                {
                    NPRNMR = eisppNumber
                },
                context = new KSTType()
                {
                    usrSid = configuration.GetValue<string>("Eispp:TS_usrSid"),
                    armSid = configuration.GetValue<string>("Eispp:TS_armSid"),
                    usrRej = configuration.GetValue<string>("Eispp:TS_usrRej"),
                    usrRab = eisppCode,
                    resSid = configuration.GetValue<string>("Eispp:TS_resSid"),
                    sbeDta = DateTime.Today
                }
            };

            try
            {
                execTSAKTSTSResponse1 response = await client.execTSAKTSTSAsync(request).ConfigureAwait(false);
                await client.CloseAsync().ConfigureAwait(false);
                //return Json(response);
                var actualData = actualData_FromResponse(response.execTSAKTSTSResponse);
                var xml = XmlUtils.SerializeObject(response);
                CdnUploadRequest xmlRequest = new CdnUploadRequest()
                {
                    ContentType = System.Net.Mime.MediaTypeNames.Text.Html,
                    FileContentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml)),
                    FileName = eisppNumber,
                    SourceId = eisppNumber,
                    SourceType = SourceTypeSelectVM.Integration_EISPP_CardTHN,
                    Title = $"КАРТА ЗА СЪСТОЯНИЕ НА НП: {eisppNumber} към { DateTime.Today.ToString("dd.MM.yyyy") }"
                };
                await cdnService.MongoCdn_AppendUpdate(xmlRequest).ConfigureAwait(false);
                return actualData;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"EISPP number {eisppNumber}");
            }
            if (readFromMongoIfFail)
            {
                execTSAKTSTSResponse1 response = await GetTSAKTSTSResponse(eisppNumber).ConfigureAwait(false);
                if (response != null)
                {
                    var actualData = actualData_FromResponse(response.execTSAKTSTSResponse);
                    return actualData;
                }
            }
            return null;
        }

        private EisppTSActualDataVM actualData_FromResponse(execTSAKTSTSResponse response)
        {
            if (response == null || response.sNPRAKTSTS == null)
            {
                return null;
            }

            EisppTSActualDataVM result = new EisppTSActualDataVM();

            if (response.sNPRAKTSTS.sNPRSTE != null)
            {
                result.PhazeName = GetElement("201", response.sNPRAKTSTS.sNPRSTE.nprstefza);
                result.ProcessName = GetElement("202", response.sNPRAKTSTS.sNPRSTE.nprstestd);
                result.ProcessDate = response.sNPRAKTSTS.sNPRSTE.nprstedta;
            }
            if (response.sNPRAKTSTS.sDLO != null)
            {
                foreach (var dlo in response.sNPRAKTSTS.sDLO)
                {
                    var _case = new EisppTSActualDataCaseVM();
                    ConvertDLO(dlo, _case);
                    result.Cases.Add(_case);
                }
            }
            if (response.sNPRAKTSTS.sPNE != null)
            {
                foreach (var pne in response.sNPRAKTSTS.sPNE)
                {
                    var crime = new EisppTSActualDataCrimeVM();
                    ConvertPNE(pne, crime);
                    result.Crimes.Add(crime);
                }
            }
            if (response.sNPRAKTSTS.sFZL != null)
            {
                foreach (var fzl in response.sNPRAKTSTS.sFZL)
                {
                    var person = new EisppTSActualDataPersonVM();
                    ConvertFZL(fzl, person);
                    foreach (var fzlpne in response.sNPRAKTSTS.sNPRFZLPNE)
                    {
                        if (fzlpne.fzlsid.ToString() == fzl.fzlsid)
                        {
                            var _crime = result.Crimes.FirstOrDefault(c => c.Sid == fzlpne.pnesid.ToString());
                            if (_crime != null)
                            {
                                var personCrime = new EisppTSActualDataPersonCrimeVM()
                                {
                                    PersonSid = person.Sid,
                                    CrimeSid = _crime.Sid,
                                    CrimeNumber = _crime.CrimeNumber,
                                    RoleCode = fzlpne.SCQ.scqrlq,
                                    Role = GetElement("220", fzlpne.SCQ.scqrlq)
                                };
                                person.PersonCrimes.Add(personCrime);
                            }
                        }
                    }
                    result.Persons.Add(person);
                }
            }

            return result;
        }


        private void ConvertFZL(sFZLNPRAKTSTSType source, EisppTSActualDataPersonVM target)
        {
            target = target ?? new EisppTSActualDataPersonVM();
            target.Sid = source.fzlsid;
            target.Person_SourceType = SourceTypeSelectVM.EisppPerson;
            target.Person_SourceCode = source.fzlsid;
            try
            {
                target.Person_SourceId = int.Parse(source.fzlsid);
            }
            catch { }

            if (!string.IsNullOrEmpty(source.fzlegn))
            {
                target.UicTypeId = NomenclatureConstants.UicTypes.EGN;
                target.Uic = source.fzlegn;
            }
            else
            {
                target.UicTypeId = NomenclatureConstants.UicTypes.LNCh;
                target.Uic = source.fzllnc;
            }
            target.FirstName = source.fzlime;
            target.MiddleName = source.fzlprz;
            target.FamilyName = source.fzlfma;
            target.FullName = source.fzlimecyr;
            target.SexCode = source.fzlpol;
            target.Sex = GetElement("216", source.fzlpol);
            if (source.ADR != null)
            {
                foreach (var adr in source.ADR)
                {
                    var newAdr = new Address();
                    ConvertADR(adr, newAdr);
                    target.Addresses.Add(newAdr);
                }
            }
        }

        private void ConvertPNE(PNEType source, EisppTSActualDataCrimeVM target)
        {
            target = target ?? new EisppTSActualDataCrimeVM();
            target.Sid = source.pnesid;
            target.CrimeNumber = source.pnenmr;
            target.CrimeDescription = source.pnestv;
        }

        private void ConvertDLO(sDLOType source, EisppTSActualDataCaseVM target)
        {
            target = target ?? new EisppTSActualDataCaseVM();
            target.Sid = source.dlosid;
            target.CaseNumber = source.dlonmr;
            target.CaseYear = source.dlogdn;
            target.CaseTypeName = GetElement("203", source.dlovid);
        }

        private void ConvertADR(ADRType source, Address target)
        {
            target = target ?? new Address();
            var country = repo.AllReadonly<EkCountry>().Where(x => x.EISPPCode == source.adrdrj).FirstOrDefault();
            if (country == null)
            {
                country = repo.AllReadonly<EkCountry>().Where(x => x.Code == NomenclatureConstants.CountryNA).FirstOrDefault();
            }
            target.CountryCode = country.Code;
            if (country.Code == NomenclatureConstants.CountryBG && !string.IsNullOrEmpty(source.adrnsmbgr))
            {
                target.CityCode = repo.AllReadonly<EkEkatte>().Where(x => x.EisppCode == source.adrnsmbgr).FirstOrDefault()?.Ekatte;
            }
            target.AddressTypeId = repo.AllReadonly<AddressType>().FirstOrDefault(x => x.Code == source.adrtip)?.Id ?? NomenclatureConstants.AddressType.Court;
            target.Description = $"{source.adrkrdtxt} {source.adrnmr}";
            target.Entrance = source.adrvhd;
            target.Floor = source.adretj;
            target.Appartment = source.adrapr;

            nomService.SetFullAddress(target);
        }

        public void ConvertEisppPersonToDocumentPerson(EisppTSActualDataPersonVM source, DocumentPersonVM target, int personIndex)
        {
            target = target ?? new DocumentPersonVM();
            target.Index = personIndex;
            target.CopyFrom(source);
            target.Person_SourceType = SourceTypeSelectVM.EisppPerson;
            target.Person_SourceCode = source.Sid;
            foreach (var adr in source.Addresses)
            {
                target.Addresses.Add(new DocumentPersonAddressVM()
                {
                    Address = adr,
                    PersonIndex = personIndex,
                    Index = target.Addresses.Count
                });
            }
        }

        public IQueryable<EisppTblElement> EisppTblElement_Select(string EisppTblCode)
        {
            return repo.AllReadonly<EisppTblElement>()
                                .Where(x => x.EisppTblCode == EisppTblCode)
                                .AsQueryable();
        }

        public EisppTblElement GetByCode(string Code)
        {
            return repo.AllReadonly<EisppTblElement>()
                                .Where(x => x.Code == Code)
                                .FirstOrDefault();
        }

        public List<SelectListItem> GetDDL_EISPPTblElement(string EisppTblCode, bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = repo.AllReadonly<EisppTblElement>()
                                        .Where(x => x.EisppTblCode == EisppTblCode)
                                        .OrderBy(x => x.Label)
                                        .Select(x => new SelectListItem()
                                        {
                                            Text = (ShowCodeInDDL ? x.Code + ": " : "") + x.Label,
                                            Value = x.Code
                                        }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_ConnectedCases(int caseId, bool addDefaultElement = true)
        {
            var aCase = repo.AllReadonly<Case>()
                            .Where(x => x.Id == caseId)
                            .FirstOrDefault();
            var connectedCases = InitCaseCause(new Dictionary<string, int>(), caseId, aCase?.DocumentId ?? 0);
            var caseTypes = GetDDL_EISPPTblElement(EisppTableCode.CaseType);
            var selectListItems = connectedCases.Select(x => new SelectListItem()
            {
                Text = $"{x.ShortNumber} от {x.Year}г. {caseTypes.Where(c => c.Value == x.CaseType.ToString()).FirstOrDefault()?.Text}",
                Value = x.ConnectedCaseId
            }).ToList() ?? new List<SelectListItem>();
            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }
            return selectListItems;
        }
        public List<SelectListItem> GetDDL_CaseMigrations(int caseId)
        {
            var selectListItems = repo.AllReadonly<Infrastructure.Data.Models.Cases.CaseMigration>()
                                       .Where(x => x.CaseId == caseId && x.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Outgoing)
                                       .Select(x => new SelectListItem()
                                       {
                                            Text = $"{x.CaseMigrationType.Label} с {x.OutDocument.DocumentType.Label} {x.OutDocument.DocumentNumber}/{x.OutDocument.DocumentDate:dd.MM.yyyy}г.",
                                            Value = x.Id.ToString()
                                       }).ToList();
            selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
          
            return selectListItems;
        }
        

        public EisppDropDownVM GetDDL_EISPPTblElementWithRules(string EisppTblCode, int eventType, string rulePath, bool addDefaultElement = true)
        {
            (var ruleIds, var flags) = rulesService.GetEisppRuleIds(eventType, rulePath);
            var selectListItemsQ = repo.AllReadonly<EisppTblElement>()
                                        .Where(x => x.EisppTblCode == EisppTblCode);
            if (ruleIds.Length > 0)
                selectListItemsQ = selectListItemsQ.Where(x => ruleIds.Contains(x.Code) || x.Code == "99001");

            var selectListItems = selectListItemsQ.OrderBy(x => x.Label)
                                       .Select(x => new SelectListItem()
                                       {
                                           Text = (ShowCodeInDDL ? x.Code + ": " : "") + x.Label,
                                           Value = x.Code
                                       }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            return new EisppDropDownVM()
            {
                Label = "",
                Flags = flags,
                DDList = selectListItems
            };
        }
        public EisppDropDownVM GetDDL_EISPPTblElementNomWithRules(int eventType, string rulePath, string nomRulePath, bool addDefaultElement = true)
        {
            (var ruleIds, var flags) = rulesService.GetEisppRuleIds(eventType, nomRulePath);
            if (ruleIds.Any())
            {
                return GetDDL_EISPPTblElementWithRules(ruleIds[0], eventType, rulePath);
            }
            else
            {
                var selectListItems = new List<SelectListItem>();
                if (addDefaultElement)
                {
                    selectListItems = selectListItems
                        .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                        .ToList();
                }
                return new EisppDropDownVM()
                {
                    Label = "",
                    Flags = 0,
                    DDList = selectListItems
                };

            }
        }
        public EisppDropDownVM GetDDL_EISPPTblElementWithRules_DloOsnExactType(string EisppTblCode, int eventType, string rulePath, string caseType, int caseCharacterId, bool addDefaultElement = true)
        {
            (var ruleIds, var flags) = rulesService.GetEisppRuleIds(eventType, rulePath);
            var selectListItemsQ = repo.AllReadonly<EisppTblElement>()
                                        .Where(x => x.EisppTblCode == EisppTblCode);
            var mapping = repo.AllReadonly<CodeMapping>()
                              .Where(x => x.Alias == EisppMapping.CaseTypeCauseExactType &&
                                          x.InnerCode == caseType);
            if (ruleIds.Length > 0)
                selectListItemsQ = selectListItemsQ.Where(x => ruleIds.Contains(x.Code));
            selectListItemsQ = selectListItemsQ.Where(x => mapping.Any(m => m.OuterCode == x.Code));
            if (caseCharacterId > 0)
            {
                var caseCheracterTypes = repo.AllReadonly<CaseTypeCharacter>()
                                             .Where(x => x.CaseCharacterId == caseCharacterId);
                // var caseCheracterTypesList = caseCheracterTypes.ToList();
                var mappingCaseType = repo.AllReadonly<CodeMapping>()
                                  .Where(x => x.Alias == EisppMapping.CaseTypes &&
                                              caseCheracterTypes.Any(c => c.CaseTypeId.ToString() == x.InnerCode)

                                               );
                // var mappingCaseTypeList = mappingCaseType.ToList();
                selectListItemsQ = selectListItemsQ.Where(x => mappingCaseType.Any(m => m.OuterCode == x.Code));
            }

            var selectListItems = selectListItemsQ.OrderBy(x => x.Label)
                                       .Select(x => new SelectListItem()
                                       {
                                           Text = (ShowCodeInDDL ? x.Code + ": " : "") + x.Label,
                                           Value = x.Code
                                       }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            return new EisppDropDownVM()
            {
                Label = "",
                Flags = flags,
                DDList = selectListItems
            };
        }

        public EisppDropDownVM GetDDL_FeatureValTblElementWithRules(int eventType, string rulePath, List<SelectListItem> ek_countries, bool addDefaultElement = true)
        {
            string nomRulePath = ".sbhvid";

            (var ruleIds, var flags) = rulesService.GetEisppRuleIds(eventType, nomRulePath);
            if (ruleIds.Any())
            {
                if (ruleIds[0] == "-2")
                {
                    switch (eventType)
                    {
                        case EventType.AskForMove:
                            // TODO: Да се пита Ирина 
                            break;
                        case EventType.ReturnToProsecutor:
                            ruleIds[0] = FeatureType.StructurePrk.ToString();
                            break;
                        case EventType.ReturnToFirst:
                            ruleIds[0] = FeatureType.StructureCourt.ToString();
                            break;
                        default:
                            break;
                    }
                }
                List<SelectListItem> selectListItems = null;
                string label = "";
                switch (ruleIds[0].ToInt())
                {
                    case FeatureType.StructurePrk:
                        selectListItems = GetDDL_InstitutionPrkEISPP();
                        break;
                    case FeatureType.StructureCourt:
                        selectListItems = GetDDL_CourtEISPP();
                        break;
                    case FeatureType.Country:
                        selectListItems = ek_countries;
                        label = repo.AllReadonly<EisppTbl>()
                                    .Where(x => x.Code == ruleIds[0])
                                    .Select(x => x.Label)
                                    .FirstOrDefault();
                        break;
                }
                switch (ruleIds[0].ToInt())
                {
                    case FeatureType.StructurePrk:
                    case FeatureType.Country:
                        label = repo.AllReadonly<EisppTblElement>()
                                    .Where(x => x.Code == ruleIds[0])
                                    .Select(x => x.Label)
                                    .FirstOrDefault();
                        break;
                }
                if (selectListItems != null)
                    return new EisppDropDownVM()
                    {
                        Label = label,
                        Flags = flags,
                        DDList = selectListItems
                    };
            }
            return GetDDL_EISPPTblElementNomWithRules(eventType, rulePath, nomRulePath, addDefaultElement);
        }

        public EisppCaseCode GetEISPPCaseCode(int caseCodeId)
        {
            var caseCode = repo.AllReadonly<CaseCode>()
                               .Where(x => x.Id == caseCodeId)
                               .Select(x => x.Code)
                               .FirstOrDefault();
            return repo.AllReadonly<EisppCaseCode>()
                       .Where(x => x.Code == caseCode)
                       .FirstOrDefault();
        }
        public List<SelectListItem> GetDDL_EISPPEventType(int caseCodeId, int caseTypeId,bool isExternal, bool addDefaultElement = true)
        {
            var caseCodes = "," + GetEISPPCaseCode(caseCodeId)?.EventCodes.Replace(" ", "", StringComparison.InvariantCultureIgnoreCase) + ",";

            string EisppTblCode = EisppTableCode.EventType;
            var selectListItems = repo.AllReadonly<EisppTblElement>()
                                        .Where(x => x.EisppTblCode == EisppTblCode &&
                                                    (caseCodes == ",," ||
                                                     caseCodes.Contains("," + x.Code.Trim() + ",", StringComparison.InvariantCultureIgnoreCase) ||
                                                     x.Code == EventType.CreateCase.ToString() ||
                                                     x.Code == EventType.GetCase.ToString() ||
                                                     x.Code == EventType.ChangeCase.ToString() ||
                                                     x.Code == EventType.ChangePerson.ToString() ||
                                                     x.Code == EventType.ChangePersonCrime.ToString() ||
                                                     x.Code == EventType.ChangeCrime.ToString() ||
                                                     x.Code == EventType.ComplaintReceived.ToString() ||
                                                     x.Code == EventType.SendCase.ToString() ||
                                                     (isExternal && x.Code == EventType.CreateOnExternal.ToString())
                                                   )
                                         )
                                        .OrderBy(x => x.Code != EventType.CreateCase.ToString() && x.Code != EventType.GetCase.ToString())
                                        .ThenBy(x => x.Label)
                                        .Select(x => new SelectListItem()
                                        {
                                            Text = (ShowCodeInDDL ? x.Code + ": " : "") + x.Label,
                                            Value = x.Code
                                        }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-100" })
                    .ToList();
            }

            return selectListItems;
        }
        public List<SelectListItem> GetDDL_CountriesForEISPP(bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = repo.AllReadonly<EkCountry>()
                                       .OrderBy(x => x.Name)
                                       .Select(x => new SelectListItem()
                                       {
                                           Text = x.Name,
                                           Value = x.EISPPCode
                                       }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_CourtEISPP(bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = repo.AllReadonly<Court>()
                                      .Where(x => !string.IsNullOrEmpty(x.EISPPCode))
                                       .Select(x => new SelectListItem()
                                       {
                                           Text = x.Label,
                                           Value = x.EISPPCode
                                       }).ToList() ?? new List<SelectListItem>();
            selectListItems = selectListItems.OrderBy(x => x.Text).ToList();
            if (addDefaultElement)
                selectListItems.Insert(0, new SelectListItem() { Text = "Избери", Value = "0" });
            if (addAllElement)
                selectListItems.Insert(0, new SelectListItem() { Text = "Всички", Value = "0" });

            return selectListItems;
        }
        public List<SelectListItem> GetDDL_InstitutionPrkEISPP(bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = repo.AllReadonly<Institution>()
                                      .Where(x => !string.IsNullOrEmpty(x.EISPPCode) &&
                                                  x.InstitutionTypeId == NomenclatureConstants.InstitutionTypes.Attourney)
                                       .Select(x => new SelectListItem()
                                       {
                                           Text = x.FullName,
                                           Value = x.EISPPCode
                                       }).ToList() ?? new List<SelectListItem>();
            selectListItems = selectListItems.OrderBy(x => x.Text).ToList();
            if (addDefaultElement)
                selectListItems.Insert(0, new SelectListItem() { Text = "Избери", Value = "0" });
            if (addAllElement)
                selectListItems.Insert(0, new SelectListItem() { Text = "Всички", Value = "0" });

            return selectListItems;
        }

        public IEnumerable<LabelValueVM> Get_EISPPTblElement(string EisppTblCode, string term, string id)
        {
            term = term.SafeLower();
            Expression<Func<EisppTblElement, bool>> filter = x => x.Label.Contains(term ?? x.Label, StringComparison.InvariantCultureIgnoreCase);
            if (!string.IsNullOrEmpty(id))
            {
                filter = x => x.Code == id;
            }
            return repo.AllReadonly<EisppTblElement>()
                            .Where(filter)
                            .Where(x => x.EisppTblCode == EisppTblCode && x.IsActive)
                            .OrderBy(x => x.Label)
                            .Select(x => new LabelValueVM
                            {
                                Value = x.Code,
                                Label = x.Label
                            }).ToList();
        }
        public int GetSid(Dictionary<string, int> dictSid, string typeSid, long id, int eisppSid = 0)
        {
            var key = $"{typeSid}{id}";
            if (dictSid.ContainsKey(key))
            {
                return dictSid[key];
            }
            else
            {
                int minSid = minSidVal;
                if (dictSid.Values.Count > 0)
                {
                    minSid = dictSid.Values.Min(x => x);
                }
                if (minSid > minSidVal)
                    minSid = minSidVal;
                if (eisppSid == 0)
                    eisppSid = minSid - 1;
            }
            dictSid.Add(key, eisppSid);
            return eisppSid;
        }
        public EisppPackage GetPackage(int packageId)
        {
            var eisppEventItem = repo.AllReadonly<EisppEventItem>()
                                    .Include(x => x.MQEpep) 
                                    .Where(x => x.Id == packageId)
                                    .FirstOrDefault();
            var model = JsonConvert.DeserializeObject<EisppPackage>(eisppEventItem.RequestData);
            model.Id = packageId;
            if (eisppEventItem.MQEpep?.IntegrationStateId > 0)
                model.IsForEdit = false;
            if (eisppEventItem.MQEpep?.IntegrationStateId == EpepConstants.IntegrationStates.ReplyContainsError)
                model.IsForEdit = true;
            CPPersonCrimeSplit(model);
            return model;
        }
        public EisppChangeVM GetPackageChange(int packageId)
        {
            var eisppEventItem = repo.AllReadonly<EisppEventItem>()
                                    .Where(x => x.Id == packageId)
                                    .FirstOrDefault();
            var model = JsonConvert.DeserializeObject<EisppPackage>(eisppEventItem.RequestData);
            model.Id = packageId;

            CPPersonCrimeSplit(model);
            var result = new EisppChangeVM()
            {
                EisppPackage = model,
                IsForSend = model.IsForSend,
                EventFromId = eisppEventItem.EventFromId,
                MQEpepId = eisppEventItem.MQEpepId,
                EventId = eisppEventItem.Id
            };
            return result;
        }

        public async Task<EisppPackage> GeneratePackage(EisppEventVM model)
        {
            var caseModel = repo.AllReadonly<Case>()
                                    .Include(x => x.CasePersons)
                                    .ThenInclude(x => x.Person)
                                    .Include(x => x.CasePersons)
                                    .ThenInclude(x => x.Addresses)
                                    .ThenInclude(x => x.Address)
                                    .Include(x => x.CaseCrimes)
                                    .Include(x => x.Court)
                                    .Include(x => x.CasePersonCrimes)
                                    .Include(x => x.Document)
                                    .Where(x => x.Id == model.CaseId)
                                    .FirstOrDefault();
            int senderStructure = caseModel.Court.EISPPCode.ToInt();
            EisppPackage eisppPackage = new EisppPackage(senderStructure);
            eisppPackage.EventTypeId = model.EventType;
            eisppPackage.CaseId = model.CaseId;
            eisppPackage.PersonMeasureId = model.PersonMeasureId;
            eisppPackage.PersonOldMeasureId = model.PersonOldMeasureId;
            eisppPackage.IsGeneratedEisppNumber = IsForEisppNum(caseModel);

            var response = await GetTSAKTSTSResponse(caseModel.EISSPNumber).ConfigureAwait(false);
            if (response == null)
            {
                try
                {
                    var data = await GetActualData(caseModel.EISSPNumber).ConfigureAwait(false);
                    response = await GetTSAKTSTSResponse(caseModel.EISSPNumber).ConfigureAwait(false);
                }
                catch
                {

                }
            }
            var eisppEvent = InitEvent(caseModel, model, senderStructure, eisppPackage.IsGeneratedEisppNumber, response);

            if (model.EventType < 0)
            {
                minSidVal = -60000;
                string egnEik = "";
                if (model.CasePersonId > 0)
                {
                    egnEik = caseModel.CasePersons.Where(x => x.Id == model.CasePersonId).Select(x => x.Person.Uic).FirstOrDefault() ?? "";
                }
                var eisppEventOld = InitEventXml(caseModel, model.EventType, senderStructure, egnEik, response);
                eisppPackage.Data = new Data(eisppEventOld, eisppEvent);
            }
            else
            {
                eisppPackage.Data = new Data(eisppEvent);
            }
            CPPersonCrimeSplit(eisppPackage);
            return eisppPackage;
        }

        private void ChangeSID(Event eisppEvent)
        {
            int changeSid = -60000;

            if (eisppEvent.CriminalProceeding.Case.EisppCaseId < 0)
                eisppEvent.CriminalProceeding.Case.EisppCaseId += changeSid;
            if (eisppEvent.EventId < 0)
                eisppEvent.EventId += changeSid;
            if (eisppEvent.CriminalProceeding.Id < 0)
                eisppEvent.CriminalProceeding.Id += changeSid;

            foreach (var person in eisppEvent.CriminalProceeding.Case.Persons)
            {
                if (person.PersonId < 0)
                    person.PersonId += changeSid;
                if (person.Measures != null)
                {
                    foreach (var measure in person.Measures)
                    {
                        if (measure.MeasureId <= 0)
                            measure.MeasureId += changeSid;
                    }
                }
                if (person.PersonCPStatus != null)
                {
                    if (person.PersonCPStatus.PersonId <= 0)
                        person.PersonCPStatus.PersonId += changeSid;
                }
                if (person.Addresses != null)
                {
                    foreach (var address in person.Addresses)
                    {
                        if (address.AddressId < 0)
                            address.AddressId += changeSid;
                    }
                }
            }
            foreach (var crime in eisppEvent.CriminalProceeding.Case.Crimes)
            {
                if (crime.CrimeId < 0)
                    crime.CrimeId += changeSid;
                foreach (var personCrime in crime.CPPersonCrimes)
                {
                    if (personCrime.PersonId < 0)
                        personCrime.PersonId += changeSid;
                    if (personCrime.CrimeId < 0)
                        personCrime.CrimeId += changeSid;
                    if (personCrime.PersonCrimeId < 0)
                        personCrime.PersonCrimeId += changeSid;
                    if (personCrime.CrimeSanction != null)
                    {
                        if (personCrime.CrimeSanction.CrimeSanctionId < 0)
                            personCrime.CrimeSanction.CrimeSanctionId += changeSid;
                    }
                    if (personCrime.CrimeSubjectStatisticData != null)
                    {
                        if (personCrime.CrimeSubjectStatisticData.SubjectStatisticDataId < 0)
                            personCrime.CrimeSubjectStatisticData.SubjectStatisticDataId += changeSid;
                    }

                }
            }
            if (eisppEvent.CriminalProceeding.Case.ConnectedCases != null)
            {
                foreach (var connectedCase in eisppEvent.CriminalProceeding.Case.ConnectedCases)
                {
                    if (connectedCase.EisppCaseId < 0)
                        connectedCase.EisppCaseId += changeSid;
                }
            }
            if (eisppEvent.EventFeature != null)
            {
                if (eisppEvent.EventFeature.FeatureId < 0)
                    eisppEvent.EventFeature.FeatureId += changeSid;
            }
            if (eisppEvent.EisppSrok != null)
            {
                if (eisppEvent.EisppSrok.SrokId < 0)
                    eisppEvent.EisppSrok.SrokId += changeSid;
            }
        }
        public EisppChangeVM GeneratePackageFrom(int eventFromId)
        {
            var eisppChangeVM = new EisppChangeVM();
            var eisppEventItem = repo.AllReadonly<EisppEventItem>()
                                 .Where(x => x.Id == eventFromId)
                                 .FirstOrDefault();
            var model = JsonConvert.DeserializeObject<EisppPackage>(eisppEventItem.RequestData);
            var modelNew = JsonConvert.DeserializeObject<EisppPackage>(eisppEventItem.RequestData);
            model.Data = new Data(model.Data.Events[0], modelNew.Data.Events[0]);
            model.Data.Events[0].EventKind = EventKind.OldEvent;

            CPPersonCrimeSplit(model);
            ChangeSID(model.Data.Events[1]);


            eisppChangeVM.EisppPackage = model;
            eisppChangeVM.EventFromId = eventFromId;
            return eisppChangeVM;
        }

        public int GeneratePackageDelete(int eventId)
        {
            var eisppEventItem = repo.AllReadonly<EisppEventItem>()
                                  .Where(x => x.Id == eventId)
                                  .FirstOrDefault();
            var model = JsonConvert.DeserializeObject<EisppPackage>(eisppEventItem.RequestData);
            model.Id = 0;
            model.Data.Events[0].EventKind = EventKind.OldEvent;
            SaveCasePackageData(model, eventId);
            return model.Id;
        }
        public Event InitEvent(Case caseModel, EisppEventVM model, int senderStructure,  bool IsGeneratedEisppNumber, execTSAKTSTSResponse1 response)
        {
            var dictSid = new Dictionary<string, int>();

            Event eisppEvent = new Event()
            {
                EventDate = caseModel.RegDate,
                StructureId = senderStructure,
                EventType = model.EventType,
                EventId = GetSid(dictSid, SidType.Event, caseModel.Id),
                CaseId = caseModel.Id,
                CasePersonId = model.CasePersonId,
                ConnectedCaseId = model.ConnectedCaseId,
                CaseSessionActId = model.CaseSessionActId,
                CaseComplaintId = model.CaseComplaintId,
            };
            if (model.CaseSessionActId > 0)
            {
                var caseSessionAct = repo.AllReadonly<CaseSessionAct>()
                                         .Where(x => x.Id == model.CaseSessionActId)
                                         .Include(x => x.ActType)
                                         .FirstOrDefault();
                if (caseSessionAct?.ActType != null)
                {
                    var codeMapping = repo.AllReadonly<CodeMapping>()
                                          .Where(x => x.Alias == EisppMapping.MappingActType &&
                                                      x.OuterCode == caseSessionAct.ActType.Id.ToString())
                                          .FirstOrDefault();
                    if (!string.IsNullOrEmpty(codeMapping?.InnerCode))
                        eisppEvent.DocumentType = codeMapping.InnerCode.ToInt();
                }
                eisppEvent.EventDate = caseSessionAct.RegDate ?? eisppEvent.EventDate;
            }
            if (model.CaseComplaintId > 0)
            {
                var doc = repo.AllReadonly<Document>()
                                         .Where(x => x.Id == model.CaseComplaintId)
                                         .FirstOrDefault();
                if (doc != null)
                {
                    var codeMapping = repo.AllReadonly<CodeMapping>()
                                          .Where(x => x.Alias == EisppMapping.ComplaintType &&
                                                      x.InnerCode == doc.DocumentTypeId.ToString())
                                          .FirstOrDefault();
                    if (!string.IsNullOrEmpty(codeMapping?.OuterCode))
                        eisppEvent.DocumentType = codeMapping.OuterCode.ToInt();
                }
                eisppEvent.EventDate = doc.DocumentDate;
            }
            //Срок за обжалване
            var eisppSrok = new EisppSrok();
            eisppEvent.EisppSrok = eisppSrok;
            eisppSrok.SrokId = GetSid(dictSid, SidType.Srok, caseModel.Id);
            var srokType = rulesService.GetEisppRuleValue(model.EventType, "SRK.srkvid").ToInt();
            if (srokType > 0)
            {
                eisppSrok.SrokType = srokType;
            }

            //Характеристики
            var feature = new EventFeature();
            eisppEvent.EventFeature = feature;
            feature.FeatureId = GetSid(dictSid, SidType.Feature, caseModel.Id);
            feature.FeatureType = rulesService.GetEisppRuleValue(model.EventType, "SBH.sbhvid").ToInt();
            if (feature.FeatureType == FeatureType.SentenceType)
            {
                var sentense = GetSentence(model.CasePersonId, model.CaseSessionActId);

                if (sentense.SentenceResultType != null)
                    eisppEvent.EventFeature.FeatureVal = sentense.SentenceResultType.Code.ToInt();
            }

            if (model.EventType == EventType.GetCase)
            {
                var ConnectedCase = InitCaseCause(dictSid, caseModel.Id, caseModel.DocumentId)
                                    .Where(x => x.ConnectedCaseId == model.ConnectedCaseId)
                                    .FirstOrDefault();
                eisppEvent.CriminalProceeding = InitCriminalProceedingGetCase(IsGeneratedEisppNumber, caseModel, senderStructure, ConnectedCase.Year, ConnectedCase.ShortNumber);
                (var rules, var flags) = rulesService.GetEisppRuleIds(model.EventType, "DVJDLO.dvjvid");
                int migrationType = int.Parse(rules[0]);
                eisppEvent.CaseMigration = new Infrastructure.Models.Integrations.Eispp.CaseMigration()
                {
                    MigrationDate = eisppEvent.EventDate,
                    MigrationId = GetSid(dictSid, SidType.GetCaseMigration, caseModel.Id),
                    SendingStructureId = ConnectedCase.StructureId,
                    ReceiverStructureId = senderStructure,
                    MigrationType = migrationType,
                    RegistrationNumber = ConnectedCase.ShortNumber.ToString()

                };
                return eisppEvent;
            }
            if (model.EventType == EventType.SendCase)
            {
                var caseMigration = repo.AllReadonly<Infrastructure.Data.Models.Cases.CaseMigration>()
                                        .Include(x => x.OutDocument)
                                        .Include(x => x.SendToCourt)
                                        .Where(x => x.Id == model.CaseMigrationId)
                                        .FirstOrDefault();

                eisppEvent.CriminalProceeding = InitCriminalProceedingCase(dictSid, caseModel, senderStructure, IsGeneratedEisppNumber);
                eisppEvent.CriminalProceeding.Case.Status = null;
                (var rules, var flags) = rulesService.GetEisppRuleIds(model.EventType, "DVJDLO.dvjvid");
                int migrationType = int.Parse(rules[0]);
                eisppEvent.CaseMigration = new Infrastructure.Models.Integrations.Eispp.CaseMigration()
                {
                    MigrationDate = eisppEvent.EventDate,
                    MigrationId = GetSid(dictSid, SidType.GetCaseMigration, caseMigration.Id),
                    SendingStructureId = senderStructure,
                    ReceiverStructureId = caseMigration.SendToCourt.EISPPCode.ToInt(),
                    MigrationType = migrationType,
                    RegistrationNumber = caseMigration.OutDocument.DocumentNumber,
                    Reason= model.ReasonId ?? 0

                };
                return eisppEvent;
            }

            eisppEvent.CriminalProceeding = InitCriminalProceeding(dictSid, caseModel, model, senderStructure, eisppEvent.EventFeature.FeatureVal, IsGeneratedEisppNumber, response); ;
            return eisppEvent;
        }

        public Event InitEventXml(Case caseModel, int eventType, int senderStructure, string egnEik, execTSAKTSTSResponse1 response)
        {
            var dictSid = new Dictionary<string, int>();

            Event eisppEvent = new Event()
            {
                EventDate = caseModel.RegDate,
                StructureId = senderStructure,
                EventId = GetSid(dictSid, SidType.Event, caseModel.Id),
                CaseId = caseModel.Id,
                EventType = eventType,
                EventKind = EventKind.OldEvent
            };
            eisppEvent.CriminalProceeding = InitCriminalProceedingXML(dictSid, caseModel, senderStructure, egnEik, eventType, response);

            return eisppEvent;
        }
        public async Task<execTSAKTSTSResponse1> GetTSAKTSTSResponse(string eisppNumber)
        {
            execTSAKTSTSResponse1 response = null;
            CdnItemVM aFile = cdnService.Select(SourceTypeSelectVM.Integration_EISPP_CardTHN, eisppNumber).FirstOrDefault();
            if (aFile != null)
            {
                var result = await cdnService.MongoCdn_Download(aFile).ConfigureAwait(false);
                var xml = Encoding.UTF8.GetString(Convert.FromBase64String(result.FileContentBase64));
                response = XmlUtils.DeserializeXml<execTSAKTSTSResponse1>(xml);
            }
            return response;
        }
        public void CPPersonCrimeSplit(EisppPackage eisppPackage)
        {
            foreach (var eisppEvent in eisppPackage.Data.Events)
            {
                if (eisppEvent.CriminalProceeding.Case.CPPersonCrimes != null && eisppEvent.CriminalProceeding.Case.Crimes != null)
                {
                    foreach (var crime in eisppEvent.CriminalProceeding.Case.Crimes)
                    {
                        crime.CPPersonCrimes = eisppEvent.CriminalProceeding.Case.CPPersonCrimes.Where(x => x.CrimeId == crime.CrimeId).ToArray();
                    }
                }
            }
        }

        public void CPPersonCrimeUnion(EisppPackage eisppPackage)
        {
            foreach (var eisppEvent in eisppPackage.Data.Events)
            {
                var CPPersonCrimes = new List<CPPersonCrime>();
                if (eisppEvent.CriminalProceeding.Case.Crimes != null)
                {
                    foreach (var crime in eisppEvent.CriminalProceeding.Case.Crimes)
                    {
                        if (crime.CPPersonCrimes != null)
                            CPPersonCrimes.AddRange(crime.CPPersonCrimes);
                    }
                }
                eisppEvent.CriminalProceeding.Case.CPPersonCrimes = CPPersonCrimes.ToArray();
            }
        }

        public CasePersonSentence GetSentence(int? casePersonId, int? caseSessionActId)
        {
            try
            {
                return repo.AllReadonly<CasePersonSentence>()
                .Include(x => x.SentenceResultType)
                .Where(x => x.IsActive == true &&
                            x.DateExpired == null &&
                            x.CasePersonId == casePersonId &&
                            x.CaseSessionActId == caseSessionActId)
                .SingleOrDefault();
            }
            catch (Exception ex)
            {
                logger.LogError($"Error:{casePersonId} {caseSessionActId} more then one sentence" + Environment.NewLine + ex.Message);
            }
            return null;
        }

        public int GetSentencePersonId(int? caseSessionActId)
        {
            try
            {
                int? casePersonId = repo.AllReadonly<CasePersonSentence>()
                .Include(x => x.SentenceResultType)
                .Where(x => x.IsActive == true &&
                            x.DateExpired == null &&
                            x.CaseSessionActId == caseSessionActId)
                .Select(x => x.CasePersonId)
                .FirstOrDefault();
                return casePersonId ?? 0;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error: {caseSessionActId} more then one sentence" + Environment.NewLine + ex.Message);
            }
            return 0;
        }

        private CriminalProceeding InitCriminalProceedingCase(Dictionary<string, int> dictSid, Case caseModel, int senderStructure, bool isGeneratedEisppNumber)
        {
            //NPR
            CriminalProceeding criminalProceeding = new CriminalProceeding();

            criminalProceeding.EisppNumber = caseModel.EISSPNumber;
            criminalProceeding.Id = -1;
            criminalProceeding.Case = new EisppCase()
            {
                EisppCaseId = GetSid(dictSid, SidType.Case, caseModel.Id),
                Year = caseModel.RegDate.Year,
                ShortNumber = caseModel.ShortNumberValue ?? 0,
                //edit!!!!!!! новообразувано
                CaseSetupType = isGeneratedEisppNumber ? 1900001682 : 722,
                ExactCaseType = getNomValueInt(EisppMapping.CaseTypes, caseModel.CaseTypeId),
                CaseTypeId = caseModel.CaseTypeId,
                CaseCodeId = caseModel.CaseCodeId ?? 0,
                LegalProceedingType = GetEISPPCaseCode(caseModel.CaseCodeId ?? 0)?.LegalProceedingType ?? LegalProceedingType.PIS_ND,
                StructureId = senderStructure,
                IsSelected = true,
                IsGeneratedEisppNumber = isGeneratedEisppNumber,
                Status = new EisppCaseStatus()
                {
                    StatusDate = DateTime.Now,
                    StatusId = GetSid(dictSid, SidType.CaseStatus, caseModel.Id)
                }
            };
            return criminalProceeding;
        }
        private CriminalProceeding InitCriminalProceeding(Dictionary<string, int> dictSid, Case caseModel, EisppEventVM model, int senderStructure,  int featureVal, bool isGeneratedEisppNumber, execTSAKTSTSResponse1 eisppResponse)
        {
            //NPR
            CriminalProceeding criminalProceeding = InitCriminalProceedingCase(dictSid, caseModel, senderStructure, isGeneratedEisppNumber);

            //DLO
            var eisppCase = criminalProceeding.Case;

            criminalProceeding.Case.ConnectedCases = InitCaseCause(dictSid, caseModel.Id, caseModel.DocumentId);

            var _persons = new List<EisppPerson>();
            var _entities = new List<LegalEntity>();

            var xmlPersons = eisppResponse?.execTSAKTSTSResponse?.sNPRAKTSTS?.sFZL ?? Array.Empty<sFZLNPRAKTSTSType>();
            int idBlank = 99999999;
            var personRoles = repo.AllReadonly<PersonRole>()
                                  .Where(x => x.RoleKindId == NomenclatureConstants.PersonKinds.RightSide)
                                  .ToList();
            var casePersons = caseModel.CasePersons
                                       .Where(x => x.CaseSessionId == null &&
                                                   personRoles.Any(r => r.Id == x.PersonRoleId))
                                       .ToList();
            foreach (var item in casePersons)
            {
                if (model.CasePersonId > 0)
                    if (item.Id != model.CasePersonId)
                        continue;
                if (item.IsPerson)
                {
                    var xmlPerson = xmlPersons.Where(x => x.fzlsid == item.Person_SourceCode).FirstOrDefault();
                    var person = InitEisppPerson(dictSid, item, senderStructure, model.EventType, model.PersonOldMeasureId, model.PersonMeasureId, ref idBlank);
                    if (xmlPerson != null)
                    {
                        InitEisppPersonFromXML(dictSid, person, xmlPerson);
                    }
                    person.IsSelectedReadOnly = (model.CasePersonId > 0);
                    _persons.Add(person);
                }
                else
                {
                    var legalEntity = InitLegalEntity(item);
                    _entities.Add(legalEntity);
                }
            }
            eisppCase.Persons = _persons.ToArray();
            eisppCase.LegalEntities = _entities.ToArray();

            var _crimes = new List<Crime>();
            var xmlCrimes = eisppResponse?.execTSAKTSTSResponse?.sNPRAKTSTS?.sPNE ?? Array.Empty<PNEType>();
            foreach (var item in caseModel.CaseCrimes.Where(x => x.DateExpired == null))
            {
                var crime = InitCrime(dictSid, item, xmlCrimes.ToList());
                if (isGeneratedEisppNumber)
                    crime.Addresses = new EisppAddress[1]
                    {
                        new EisppAddress()
                        {
                            AddressId = GetSid(dictSid, SidType.Address, item.Id),
                            AddressType = 635,
                            Country = EISPPConstants.CountryBG
                        }
                    };
                _crimes.Add(crime);
            }
            foreach (var item in xmlCrimes)
            {
                if (!_crimes.Where(x => x.EisppNumber == item.pnenmr).Any())
                {
                    var crime = InitCrimeXMl(item);
                    _crimes.Add(crime);
                }
            }
            eisppCase.Crimes = _crimes.ToArray();

            var _CPPersonCrimes = new List<CPPersonCrime>();
            var sentenseCrimes = repo.AllReadonly<CasePersonSentencePunishmentCrime>()
                                     .Include(x => x.SentenceType)
                                     .Include(x => x.CasePersonSentencePunishment)
                                     .ThenInclude(x => x.CasePersonSentence)
                                     .Where(x => x.DateExpired == null &&
                                                 caseModel.CasePersonCrimes.Any(c => c.CaseCrimeId == x.CaseCrimeId))
                                      .ToList();
            foreach (var person in eisppCase.Persons)
            {
                foreach (var item in caseModel.CasePersonCrimes.Where(x => x.DateExpired == null && x.CasePersonId == person.CasePersonId))
                {
                    var caseCrime = caseModel.CaseCrimes?.Where(x => x.Id == item.CaseCrimeId).FirstOrDefault();
                    var personCrime = InitCPPersonCrime(dictSid,caseCrime?.EISSPNumber, item, senderStructure, featureVal);
                    personCrime.PersonName = person.FullNameCyr;
                    _CPPersonCrimes.Add(personCrime);
                }
                foreach (var sentenseCrime in sentenseCrimes.Where(x => x.CasePersonSentencePunishment.CasePersonSentence.CasePersonId == person.CasePersonId))
                {
                    if (_CPPersonCrimes.Any(x => x.CaseCrimeId == sentenseCrime.CaseCrimeId))
                        continue;
                    var caseCrime = caseModel.CaseCrimes?.Where(x => x.Id == sentenseCrime.CaseCrimeId).FirstOrDefault();
                    var personCrime = InitCPPersonCrime(dictSid, caseCrime?.EISSPNumber, sentenseCrime, person.CasePersonId, senderStructure, featureVal);
                    personCrime.PersonName = person.FullNameCyr;
                    _CPPersonCrimes.Add(personCrime);
                }
            }
            var xmlPersonCrime = eisppResponse?.execTSAKTSTSResponse?.sNPRAKTSTS?.sNPRFZLPNE ?? Array.Empty<NPRFZLPNEType>();
            int statisticDataId = 1;
            foreach (var item in xmlPersonCrime)
            {
                var person = eisppCase.Persons.FirstOrDefault(x => x.PersonSourceId == item.fzlsid);
                if (person != null)
                {
                    var xmlCrime = xmlCrimes?.Where(x => x.pnesid == item.pnesid.ToString()).FirstOrDefault();
                    if (_CPPersonCrimes.Any(x => x.CasePersonId == person.CasePersonId && x.EISPPNumber == xmlCrime.pnenmr) ) 
                        continue;
                    var personCrime = InitCPPersonCrimeXML(item, person, senderStructure, dictSid, ref statisticDataId);
                    personCrime.PersonName = person.FullNameCyr;
                    _CPPersonCrimes.Add(personCrime);
                }
            }

            int crimeNum = 1000;
            foreach (var crime in eisppCase.Crimes)
            {
                foreach (var person in eisppCase.Persons)
                {
                    if (!_CPPersonCrimes.Any(x => x.PersonId == person.PersonId && x.CrimeId == crime.CrimeId))
                    {
                        var personCrime = InitCPPersonCrimeFromPerson(dictSid, crime, person, senderStructure, featureVal, ref crimeNum);
                        personCrime.PersonName = person.FullNameCyr;
                        _CPPersonCrimes.Add(personCrime);
                    }
                }
            }
            int punishmentNum = 1;
            foreach (var personCrime in _CPPersonCrimes)
            {
                InitCPPersonCrimePunishment(dictSid, personCrime, sentenseCrimes, ref punishmentNum);
            }
            eisppCase.CPPersonCrimes = _CPPersonCrimes.ToArray();
                
            return criminalProceeding;

        }

        private CriminalProceeding InitCriminalProceedingXML(Dictionary<string, int> dictSid, Case caseModel, int senderStructure, string egnEik, int eventType, execTSAKTSTSResponse1 eisppResponse)
        {
            //NPR
            CriminalProceeding criminalProceeding = new CriminalProceeding();

            criminalProceeding.EisppNumber = caseModel.EISSPNumber;
            criminalProceeding.Id = -1;

            //DLO
            EisppCase eisppCase = new EisppCase()
            {
                EisppCaseId = GetSid(dictSid, SidType.Case, caseModel.Id),
                Year = caseModel.RegDate.Year,
                ShortNumber = caseModel.ShortNumberValue ?? 0,
                ExactCaseType = getNomValueInt(EisppMapping.CaseTypes, caseModel.CaseTypeId),
                CaseTypeId = caseModel.CaseTypeId,
                CaseCodeId = caseModel.CaseCodeId ?? 0,
                StructureId = senderStructure
            };


            criminalProceeding.Case = eisppCase;

            var _persons = new List<EisppPerson>();
            var _entities = new List<LegalEntity>();

            var xmlPersons = eisppResponse?.execTSAKTSTSResponse?.sNPRAKTSTS?.sFZL ?? Array.Empty<sFZLNPRAKTSTSType>();
            foreach (var item in xmlPersons)
            {
                if (!string.IsNullOrEmpty(egnEik))
                    if (item.fzlegn != egnEik)
                        continue;
                var person = new EisppPerson();
                person.PersonId = GetSid(dictSid, SidType.Person, item.fzlsid.ToInt());
                person.Egn = item.fzlegn;
                person.FullNameCyr = item.fzlimecyr;
                person.FirstName = item.fzlime;
                person.SecondName = item.fzlprz;
                person.LastName = item.fzlfma;
                person.Gender = item.fzlpol.ToInt();
                person.IsSelected = true;
                person.IsSelectedReadOnly = true;
                InitEisppPersonFromXML(dictSid, person, item);
                _persons.Add(person);
            }
            eisppCase.Persons = _persons.ToArray();
            eisppCase.LegalEntities = _entities.ToArray();

            if (eventType == EventType.ChangePerson)
                return criminalProceeding;

            var _crimes = new List<Crime>();
            var xmlCrimes = eisppResponse?.execTSAKTSTSResponse?.sNPRAKTSTS?.sPNE ?? Array.Empty<PNEType>();

            foreach (var item in xmlCrimes)
            {
                if (!_crimes.Where(x => x.EisppNumber == item.pnenmr).Any())
                {
                    var crime = InitCrimeXMl(item);
                    _crimes.Add(crime);
                }
            }
            eisppCase.Crimes = _crimes.ToArray();

            var _CPPersonCrimes = new List<CPPersonCrime>();

            var xmlPersonCrime = eisppResponse?.execTSAKTSTSResponse?.sNPRAKTSTS?.sNPRFZLPNE ?? Array.Empty<NPRFZLPNEType>();
            int statisticDataId = 9000;
            foreach (var item in xmlPersonCrime)
            {
                var person = eisppCase.Persons.FirstOrDefault(x => x.PersonSourceId == item.fzlsid);
                if (person != null)
                {
                    var personCrime = InitCPPersonCrimeXML(item, person, senderStructure, dictSid, ref statisticDataId);
                    personCrime.PersonName = person.FullNameCyr;
                    _CPPersonCrimes.Add(personCrime);
                }
            }
            int crimeNum = 1000;
            int punishmentNum = 999999999;
            foreach (var crime in eisppCase.Crimes)
            {
                foreach (var person in eisppCase.Persons)
                {
                    if (!_CPPersonCrimes.Any(x => x.PersonId == person.PersonId && x.CrimeId == crime.CrimeId))
                    {
                        var personCrime = InitCPPersonCrimeFromPersonXML(dictSid, crime, person, senderStructure, ref crimeNum, person.Punishments, 0, ref punishmentNum);
                        personCrime.PersonName = person.FullNameCyr;
                        _CPPersonCrimes.Add(personCrime);
                    }
                }
            }
            eisppCase.CPPersonCrimes = _CPPersonCrimes.ToArray();
            return criminalProceeding;
        }

        private CriminalProceeding InitCriminalProceedingGetCase(bool IsGeneratedEisppNumber, Case caseModel, int senderStructure,int caseYear, int caseShortNumber)
        {
            //NPR
            CriminalProceeding criminalProceeding = new CriminalProceeding();

            criminalProceeding.EisppNumber = caseModel.EISSPNumber;
            criminalProceeding.Id = -1;

            //DLO
            EisppCase eisppCase = new EisppCase();
            eisppCase.Year = caseYear;
            eisppCase.ShortNumber = caseShortNumber;
            eisppCase.EisppCaseId = -caseModel.Id;
            eisppCase.StructureId = senderStructure;
            eisppCase.IsGeneratedEisppNumber = IsGeneratedEisppNumber;
            criminalProceeding.Case = eisppCase;
            
            return criminalProceeding;
        }
        private EisppBaseCase[] InitCaseCause(Dictionary<string, int> dictSid, int caseId, long documentId)
        {
            var cases = new List<EisppBaseCase>();
            {
                var documentCaseInfo = repo.AllReadonly<DocumentCaseInfo>()
                                           .Where(x => x.DocumentId == documentId)
                                           .Include(x => x.Court)
                                           .FirstOrDefault();
                if (documentCaseInfo != null)
                {
                    var caseCause = new EisppBaseCase();
                    caseCause.Year = documentCaseInfo.CaseYear ?? 0;
                    caseCause.ShortNumber = documentCaseInfo.CaseShortNumber.ToInt();
                    caseCause.EisppCaseId = GetSid(dictSid, SidType.CaseCause, caseId);
                    var caseFrom = repo.AllReadonly<Case>()
                                       .Where(x => x.Id == documentCaseInfo.CaseId)
                                       .Include(x => x.Court)
                                       .FirstOrDefault();
                    if (caseFrom != null)
                    {
                        caseCause.Year = caseFrom.RegDate.Year;
                        caseCause.ShortNumber = caseFrom.ShortNumber.ToInt();
                        try
                        {
                            caseCause.ExactCaseType = getNomValueInt(EISPPConstants.EisppMapping.CaseTypes, caseFrom.CaseTypeId);
                        } catch
                        {

                        }
                        caseCause.CaseTypeId = caseFrom.CaseTypeId;
                        caseCause.CaseCodeId = caseFrom.CaseCodeId ?? 0;
                        caseCause.LegalProceedingType = GetEISPPCaseCode(caseFrom.CaseCodeId ?? 0)?.LegalProceedingType ?? 0;
                        caseCause.StructureId = documentCaseInfo.Court.EISPPCode.ToInt();
                        caseCause.InstitutionTypeName = "Съдилища";
                        caseCause.InstitutionName = caseFrom.Court?.Label ?? "";
                        caseCause.InstitutionCaseTypeName = "Съдебно дело";
                        caseCause.ConnectedCaseId = "C" + caseFrom.Id.ToString("000000000");
                    } else
                    {
                        caseCause.InstitutionTypeName = "Съдилища";
                        caseCause.InstitutionCaseTypeName = "Съдебно дело";
                        caseCause.ConnectedCaseId = "D" + documentCaseInfo.Id.ToString("000000000");
                        var caseNumberDecoded = nomService.DecodeCaseRegNumber(documentCaseInfo.CaseRegNumber);
                        var documentCaseInfoCourt = repo.AllReadonly<Court>()
                                                        .Where(x => x.Id == caseNumberDecoded.CourtId)
                                                        .FirstOrDefault();
                        if (!caseNumberDecoded.IsValid || documentCaseInfoCourt == null)
                        {
                            logger.LogError($"Error DocumentCaseInfo.CaseRegNumber {documentCaseInfo.CaseRegNumber} e невалиден номер на дело" , null);

                        }
                        else
                        {
                            caseCause.InstitutionName = documentCaseInfoCourt.Label ?? "";
                            caseCause.StructureId = documentCaseInfoCourt.EISPPCode.ToInt();
                            caseCause.CaseCharacterId = caseNumberDecoded.CaseCharacterId;
                        }
                    }
                    cases.Add(caseCause);
                }
            }
            var institutionCases = repo.AllReadonly<DocumentInstitutionCaseInfo>()
                                       .Where(x => x.DocumentId == documentId)
                                       .Include(x => x.Institution)
                                       .ThenInclude(x => x.InstitutionType)
                                       .Include(x => x.InstitutionCaseType)
                                       .ToList();

            foreach (var institutionCase in institutionCases)
            {
                var caseCause = new EisppBaseCase();
                caseCause.Year = institutionCase.CaseYear;
                caseCause.ShortNumber = institutionCase.CaseNumber.ToInt();
                caseCause.EisppCaseId = caseCause.EisppCaseId = GetSid(dictSid, SidType.CaseCauseInstitution, institutionCase.Id);
                caseCause.InstitutionCaseTypeName = institutionCase.InstitutionCaseType.Label;
                caseCause.InstitutionName = institutionCase.Institution.FullName;
                caseCause.InstitutionTypeName = institutionCase.Institution.InstitutionType.Label;
                try
                {
                    caseCause.CaseType = getNomValueInt(EISPPConstants.EisppMapping.CaseTypeCause, institutionCase.InstitutionCaseTypeId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error InstitutionCase CaseType");
                    continue;
                }
                string institutionType = institutionCase.Institution?.InstitutionTypeId.ToString() ?? "";
                var mapping = repo.AllReadonly<CodeMapping>()
                                  .Where(x => x.Alias == EisppMapping.CaseTypeCauseIstitution &&
                                              x.OuterCode == caseCause.CaseType.ToString() &&
                                              x.InnerCode == institutionType)
                                  .ToList();
                if (!mapping.Any())
                {
                    logger.LogError("Error Institution " + institutionCase.Institution.FullName + " не може да има тип документ " + institutionCase.InstitutionCaseType.Label, null);
                    continue;
                }
                caseCause.StructureId = institutionCase.Institution.EISPPCode.ToInt();
                caseCause.ConnectedCaseId = "I" + institutionCase.Id.ToString("000000000");
                cases.Add(caseCause);
            }
            return cases.ToArray();
        }
        private EisppPerson InitEisppPerson(Dictionary<string, int> dictSid, CasePerson casePerson, int senderStructure, int eventType, int? oldMeasureId, int? measureId, ref int idBlank)
        {
            int eisppID = casePerson.Person_SourceType == SourceTypeSelectVM.EisppPerson ? casePerson.Person_SourceCode.ToInt() : 0;

            EisppPerson eisppPerson = new EisppPerson()
            {
                PersonId = GetSid(dictSid, SidType.Person, casePerson.Id),
                PersonSourceId = casePerson.Person_SourceId,
                FirstName = casePerson.FirstName?.ToUpper(),
                SecondName = casePerson.MiddleName?.ToUpper(),
                LastName = $"{casePerson.FamilyName} {casePerson.Family2Name}".ToUpper().Trim(),
                FullNameCyr = casePerson.MakeFullNameEISPP().ToUpper(),
                Addresses = null,
                BirthPlace = null,
                CitizenshipBg = casePerson.UicTypeId == NomenclatureConstants.UicTypes.EGN ? EISPPConstants.CountryBG : 0,
                Gender = EISPPConstants.Gender.Male,
                CasePersonId = casePerson.Id,
                IsSelected = true
            };
            eisppPerson.PersonCPStatus = new PersonCriminalProceeding();
            eisppPerson.PersonCPStatus.PersonId = GetSid(dictSid, SidType.Person, eisppPerson.CasePersonId);

            if (!string.IsNullOrEmpty(casePerson.Uic))
            {
                switch (casePerson.UicTypeId)
                {
                    case NomenclatureConstants.UicTypes.EGN:
                        eisppPerson.Egn = casePerson.Uic.Trim();
                        eisppPerson.CitizenshipBg = EISPPConstants.CountryBG;
                        bool? isMale = Utils.Validation.IsMaleFromEGN(casePerson.Uic.Trim());
                        if (isMale ?? true)
                        {
                            eisppPerson.Gender = EISPPConstants.Gender.Male;
                        }
                        else
                        {
                            eisppPerson.Gender = EISPPConstants.Gender.Female;
                        }
                        break;
                    case NomenclatureConstants.UicTypes.LNCh:
                        eisppPerson.Lnch = casePerson.Uic;
                        break;
                    default:
                        break;
                }
                DateTime? birthDay = Utils.Validation.GetBirthDayFromEgn(casePerson.Uic.Trim());
                eisppPerson.BirthDate = birthDay ?? eisppPerson.BirthDate;
            }
            if (!string.IsNullOrEmpty(casePerson.BirthCountryCode))
            {
                eisppPerson.BirthPlace = new BirthPlace();
                eisppPerson.BirthPlace.Country = repo.AllReadonly<EkCountry>()
                                                .Where(x => x.Code == casePerson.BirthCountryCode)
                                                .Select(x => x.EISPPCode)
                                                .DefaultIfEmpty("0").FirstOrDefault().ToInt();
                if (!string.IsNullOrEmpty(casePerson.BirthCityCode))
                {
                    eisppPerson.BirthPlace.SettelmentBg = repo.AllReadonly<EkEkatte>()
                                               .Where(x => x.Ekatte == casePerson.BirthCityCode)
                                               .Select(x => x.EisppCode)
                                               .DefaultIfEmpty("0").FirstOrDefault().ToInt();
                }
                else
                {
                    eisppPerson.BirthPlace.SettelmentAbroad = casePerson.BirthForeignPlace;
                }
            }
            if (casePerson.Addresses?.Count > 0)
            {
                var _addresses = new List<EisppAddress>();
                foreach (var item in casePerson.Addresses)
                {
                    var adr = InitEisppAddress(item.Address);
                    _addresses.Add(adr);
                }
                eisppPerson.Addresses = _addresses.ToArray();
            }
            eisppPerson.Measures = InitProceduralCoercionMeasure(dictSid, casePerson.Id, senderStructure, eventType, oldMeasureId, measureId);
            eisppPerson.Punishments = InitPunishments(dictSid, casePerson, senderStructure, ref idBlank);

            if (eventType == EventType.EarlyRelease)
                eisppPerson.Punishments = eisppPerson.Punishments.Where(x => x.PunishmentType != PunishmentType.Union).ToArray();

            return eisppPerson;
        }
        private void InitEisppPersonFromXML(Dictionary<string, int> dictSid, EisppPerson person, sFZLNPRAKTSTSType xmlPerson)
        {
            if (person.CitizenshipBg == 0 && person.OtherCitizenship == 0)
            {
                person.CitizenshipBg = xmlPerson.fzlgrjbgr.ToInt();
                person.OtherCitizenship = xmlPerson.fzlgrjchj.ToInt();
            }

            if (xmlPerson.MRD != null)
            {
                if (person.BirthPlace == null)
                {
                    person.BirthPlace = new BirthPlace();
                    person.BirthPlace.Country = xmlPerson.MRD.mrddrj.ToInt();
                    person.BirthPlace.PlaceId = xmlPerson.MRD.mrdsid.ToInt();
                    person.BirthPlace.SettelmentBg = xmlPerson.MRD.mrdnsmbgr.ToInt();
                    person.BirthPlace.SettelmentAbroad = xmlPerson.MRD.mrdnsmchj;
                }
                else
                {
                    person.BirthPlace.PlaceId = xmlPerson.MRD.mrdsid.ToInt();
                }
            }
            if (person.BirthDate == EisppPerson.defaultDate)
                person.BirthDate = xmlPerson.fzldtarjd;
            // person.Measures
            if (xmlPerson.NPRFZLSTA?.Length > 0)
            {
                person.PersonCPStatus = person.PersonCPStatus ?? new PersonCriminalProceeding();
                person.PersonCPStatus.PersonRole = xmlPerson.NPRFZLSTA[0].nprfzlkcv.ToInt();
                person.PersonCPStatus.Status = xmlPerson.NPRFZLSTA[0].nprfzlsts.ToInt();
                person.PersonCPStatus.StatusDate = xmlPerson.NPRFZLSTA[0].nprfzldta;
                person.PersonCPStatus.StatusReason = xmlPerson.NPRFZLSTA[0].nprfzlosn.ToInt();
            }
        }

        private void AddBlankPunishments(Dictionary<string, int> dictSid, List<Punishment> punishments, int punishmentType, ref int idBlank, int senderStructure)
        {
            if (!punishments.Any(x => x.PunishmentType == punishmentType))
            {
                idBlank++;
                Punishment punishment = new Punishment()
                {
                    PunishmentId = GetSid(dictSid, SidType.Punishment, idBlank),
                    PunishmentType = punishmentType,
                    StructureId = senderStructure
                };
                punishment.ProbationMeasure = new ProbationMeasure();
                punishment.ProbationMeasure.MeasureId = GetSid(dictSid, SidType.ProbationMeasure, idBlank);
                punishment.InitProbationMeasure();
                punishments.Add(punishment);
            }
        }
        private Punishment[] InitPunishments(Dictionary<string, int> dictSid, CasePerson casePerson, int senderStructure, ref int idBlank)
        {
            var punishmentUnion = InitPunishmentsForType(dictSid, PunishmentType.Union, casePerson, senderStructure, ref idBlank);
            var punishmentForExecution = InitPunishmentsForType(dictSid, PunishmentType.ForExecution, casePerson, senderStructure, ref idBlank);
            punishmentUnion.AddRange(punishmentForExecution);
            return punishmentUnion.OrderBy(x => x.CasePersonSentencePunishmentId).ThenBy(x => x.PunishmentType).ToArray();
        }
        private List<Punishment> InitPunishmentsForType(Dictionary<string, int> dictSid, int punishmentType, CasePerson casePerson, int senderStructure, ref int idBlank)
        {
            List<Punishment> result = new List<Punishment>();
            var sentenses = repo.AllReadonly<CasePersonSentencePunishment>()
                                        .Include(x => x.CasePersonSentence)
                                        .Include(x => x.SentenceType)
                                        .Include(x => x.SentenceRegimeType)
                                        .Where(x => x.DateExpired == null &&
                                                    x.CasePersonSentence.DateExpired == null &&
                                                    x.CasePersonSentence.IsActive == true &&
                                                    x.CasePersonSentence.CasePersonId == casePerson.Id);

            foreach (var sentenseModel in sentenses)
            {

                int punishmentActivity = 0;
                if (sentenseModel.CasePersonSentence?.PunishmentActivityId > 0)
                {
                    punishmentActivity = repo.AllReadonly<PunishmentActivity>()
                                             .Where(x => x.Id == sentenseModel.CasePersonSentence.PunishmentActivityId)
                                             .Select(x => x.Code)
                                             .FirstOrDefault()
                                             .ToInt();
                }
                Punishment punishment = new Punishment()
                {
                    CasePersonSentencePunishmentId = sentenseModel.Id,
                    PunishmentId = GetSid(dictSid, SidType.Punishment + punishmentType.ToString(), sentenseModel.Id),
                    PunishmentType = punishmentType,
                    PunishmentKind = (sentenseModel.SentenceType?.Code ?? "").ToInt(),
                    PunishmentRegime = (sentenseModel.SentenceRegimeType?.Code ?? "").ToInt(),
                    ServingType = sentenseModel.SentenceType.IsEffective == true ? ServingType.Efective : ServingType.Probation,
                    StructureId = senderStructure,
                    IsSelected = true,
                    PunishmentActivityDateVM = sentenseModel.CasePersonSentence?.PunishmentActivityDate,
                    PunishmentActivity =  punishmentActivity,
                };
                if (sentenseModel.SentenceType.HasPeriod == true)
                {
                    punishment.PunishmentYears = sentenseModel.SentenseYears;
                    punishment.PunishmentMonths = sentenseModel.SentenseMonths;
                    punishment.PunishmentWeeks = sentenseModel.SentenseWeeks;
                    punishment.PunishmentDays = sentenseModel.SentenseDays;
                    punishment.ProbationYears = sentenseModel.ProbationYears ?? 0;
                    punishment.ProbationMonths = sentenseModel.ProbationMonths ?? 0;
                    punishment.ProbationWeeks = sentenseModel.ProbationWeeks ?? 0;
                    punishment.ProbationDays = sentenseModel.ProbationDays ?? 0;
                }
                punishment.ProbationMeasure = new ProbationMeasure();
                punishment.ProbationMeasure.MeasureId = GetSid(dictSid, SidType.ProbationMeasure, sentenseModel.Id);
                if (sentenseModel.SentenceType.HasProbation == true)
                {
                    punishment.ProbationMeasure.ValidFrom = sentenseModel.DateFrom;
                    punishment.ProbationMeasure.ValidTill = sentenseModel.DateTo ?? sentenseModel.DateFrom;
                    if (sentenseModel.SentenceType.HasMoney == true)
                    {
                        punishment.ProbationMeasure.MeasureAmount = (double)sentenseModel.SentenseMoney; // ?????
                    }
                }
                punishment.InitProbationMeasure();

                if (sentenseModel.SentenceType.HasMoney == true && sentenseModel.SentenceType.HasProbation != true)
                {
                    punishment.FineAmount = (double)sentenseModel.SentenseMoney;
                }
                result.Add(punishment);
            }
            AddBlankPunishments(dictSid, result, punishmentType, ref idBlank, senderStructure);
            return result;
        }
        private LegalEntity InitLegalEntity(CasePerson casePerson)
        {
            LegalEntity legalEntity = new LegalEntity()
            {
                EntityId = casePerson.Id,
                RegistrationNumber = casePerson.Uic,
                FullName = casePerson.FullName
            };
            if (casePerson.Addresses?.Count > 0)
            {
                var _addresses = new List<EisppAddress>();
                foreach (var item in casePerson.Addresses)
                {
                    var adr = InitEisppAddress(item.Address);
                    _addresses.Add(adr);
                }
                legalEntity.Addresses = _addresses.ToArray();
            }
            return legalEntity;
        }

        private EisppAddress InitEisppAddress(Address addressModel)
        {
            EisppAddress eisppAddress = new EisppAddress();
            var strCountryCode = repo.AllReadonly<EkCountry>()
                                       .Where(x => x.Code == addressModel.CountryCode)
                                       .Select(x => x.EISPPCode)
                                       .FirstOrDefault();

            eisppAddress.AddressType = repo.AllReadonly<AddressType>().Where(x => x.Id == addressModel.AddressTypeId)
                                            .Select(x => x.Code).FirstOrDefault().ToInt();

            eisppAddress.Country = strCountryCode.ToInt();
            if (!string.IsNullOrEmpty(addressModel.CityCode))
            {
                var strCityCode = repo.AllReadonly<EkEkatte>()
                                      .Where(x => x.Ekatte == addressModel.CityCode)
                                      .Select(x => x.EisppCode)
                                      .FirstOrDefault();
                eisppAddress.SettlementBg = strCityCode.ToInt();
                if (!string.IsNullOrEmpty(addressModel.StreetCode))
                {
                    eisppAddress.StreetName = repo.AllReadonly<EkStreet>()
                                                        .Where(x => x.Code == addressModel.StreetCode)
                                                        .Select(x => x.Name).FirstOrDefault();
                }
                eisppAddress.Building = addressModel.Block?.ToString();
                eisppAddress.Number = addressModel.StreetNumber?.ToString();
                eisppAddress.Entrance = addressModel.Entrance;
                eisppAddress.Floor = addressModel.Floor;
                eisppAddress.Appartment = addressModel.Appartment;
                eisppAddress.Description = addressModel.Description;
            }
            else
            {
                eisppAddress.SettlementAbroad = addressModel.ForeignAddress;
            }

            return eisppAddress;
        }

        private Crime InitCrime(Dictionary<string, int> dictSid, CaseCrime crimeModel, List<PNEType> xmlCrimes)
        {
            Crime crime = new Crime();
            crime.CrimeId = GetSid(dictSid, SidType.Crime, crimeModel.Id);
            crime.EisppNumber = crimeModel.EISSPNumber;
            crime.StartDateType = crimeModel.StartDateType ?? 0;
            crime.StartDate = crimeModel.DateFrom;
            crime.EndDate = crimeModel.DateTo ?? Crime.defaultDate;
            crime.CrimeStatus = new CrimeStatus()
            {
                CrimeQualification = int.Parse(crimeModel.CrimeCode),
                Status = crimeModel.Status ?? 0,
                StatusDate = crimeModel.StatusDate ?? Crime.defaultDate,
                CompletitionDegree = crimeModel.CompletitionDegree ?? 0,
            };
            var xmlCrime = xmlCrimes.Where(x => x.pnenmr == crimeModel.EISSPNumber).FirstOrDefault();
            if (xmlCrime != null)
            {
                crime.CiminalProceedingCrime = new CiminalProceedingCrime()
                {
                    Status = xmlCrime.NPRPNESTA.nprpnests.ToInt(),
                    StatusDate = xmlCrime.NPRPNESTA.nprpnedta
                };
            }

            return crime;
        }
        private Crime InitCrimeXMl(PNEType xmlCrime)
        {
            Crime crime = new Crime();
            crime.CrimeId = xmlCrime.pnesid.ToInt();
            crime.EisppNumber = xmlCrime.pnenmr;
            crime.StartDateType = xmlCrime.pneotdtip.ToInt();
            crime.StartDate = xmlCrime.pnedtaotd;
            crime.EndDate = xmlCrime.pnedtadod;
            crime.CrimeStatus = new CrimeStatus()
            {
                CrimeQualification = xmlCrime.PNESTA.pnekcq.ToInt(),
                Status = xmlCrime.PNESTA.pnests.ToInt(),
                StatusDate = xmlCrime.PNESTA.pnestsdta,
                CompletitionDegree = xmlCrime.PNESTA.pnestpdvs.ToInt()
            };
            crime.CiminalProceedingCrime = new CiminalProceedingCrime()
            {
                Status = xmlCrime.NPRPNESTA.nprpnests.ToInt(),
                StatusDate = xmlCrime.NPRPNESTA.nprpnedta
            };
            return crime;
        }
        private CPPersonCrime InitCPPersonCrimePunishment(
            Dictionary<string, int> dictSid, 
            CPPersonCrime personCrime, 
            ICollection<CasePersonSentencePunishmentCrime> sentenseCrimes,
            ref int punishmentNum
        )
        {
            var crimePunishments = new List<CrimePunishment>();
            foreach (var sentenseCrime in sentenseCrimes.Where(x => x.CasePersonSentencePunishment.CasePersonSentence.CasePersonId == personCrime.CasePersonId &&
                                                                    x.CaseCrimeId == personCrime.CaseCrimeId).OrderBy(x => x.Id))
            {
                var crimePunishment = new CrimePunishment();
                crimePunishment.CrimePunishmentId = GetSid(dictSid, SidType.CrimePunishment, punishmentNum);
                punishmentNum++;
                crimePunishment.Id = punishmentNum;
                crimePunishment.PunishmentKind = (sentenseCrime.SentenceType?.Code ?? "").ToInt();
                crimePunishment.PunishmentYears = sentenseCrime.SentenseYears;
                crimePunishment.PunishmentMonths = sentenseCrime.SentenseMonths;
                crimePunishment.PunishmentWeeks = sentenseCrime.SentenseWeeks;
                crimePunishment.PunishmentDays = sentenseCrime.SentenseDays;
                crimePunishment.FineAmount = (double)sentenseCrime.SentenseMoney;
                crimePunishments.Add(crimePunishment);
                int role = getNomValueInt(EisppMapping.PersonInCrimeRole, sentenseCrime.PersonRoleInCrimeId);
                if (role > 0 && personCrime.CrimeSanction != null)
                    personCrime.CrimeSanction.Role = role;
                int relaps = getNomValueInt(EisppMapping.Relaps, sentenseCrime.RecidiveTypeId);
                if (relaps > 0 && personCrime.CrimeSubjectStatisticData != null)
                    personCrime.CrimeSubjectStatisticData.Relaps = relaps;
           }
            if (crimePunishments.Count == 0)
            {
                var crimePunishment = new CrimePunishment();
                crimePunishment.CrimePunishmentId = GetSid(dictSid, SidType.CrimePunishment, punishmentNum);
                punishmentNum++;
                crimePunishment.Id = punishmentNum;
                crimePunishments.Add(crimePunishment);
            }
            personCrime.CrimeSanction.CrimePunishments = crimePunishments.ToArray();
            return personCrime;
        }

        private CPPersonCrime InitCPPersonCrime(Dictionary<string, int> dictSid, string eisppNumber, CasePersonCrime personCrimeModel, int senderStructure, int featureVal)
        {
            CPPersonCrime personCrime = new CPPersonCrime();
            personCrime.PersonCrimeId = GetSid(dictSid, SidType.PersonCrime, personCrimeModel.Id);
            personCrime.PersonId = GetSid(dictSid, SidType.Person, personCrimeModel.CasePersonId);
            personCrime.CrimeId = GetSid(dictSid, SidType.Crime, personCrimeModel.CaseCrimeId);
            personCrime.CaseCrimeId = personCrimeModel.CaseCrimeId;
            personCrime.CasePersonId = personCrimeModel.CasePersonId;
            personCrime.EISPPNumber = eisppNumber;
            personCrime.CrimeSanction = new CrimeSanction()
            {
                CrimeSanctionId = GetSid(dictSid, SidType.CrimeSanction, personCrimeModel.Id),
                Role = getNomValueInt(EisppMapping.PersonInCrimeRole, personCrimeModel.PersonRoleInCrimeId),
                SanctionType = SanctionType.ConnectToCrime,
                StructureId = senderStructure
            };
            if (featureVal == EISPPConstants.SentenceResultType.Innocence)
                personCrime.CrimeSanction.SanctionType = SanctionType.Innocence;

            personCrime.CrimeSubjectStatisticData = new CrimeSubjectStatisticData()
            {
                SubjectStatisticDataId = GetSid(dictSid, SidType.SubjectStatisticData, personCrimeModel.Id),
                Relaps = getNomValueInt(EisppMapping.Relaps, personCrimeModel.RecidiveTypeId)
            };
            personCrime.IsSelected = true;
            return personCrime;
        }
        private CPPersonCrime InitCPPersonCrime(Dictionary<string, int> dictSid, string eisppNumber, CasePersonSentencePunishmentCrime model, int casePersonId, int senderStructure, int featureVal)
        {
            CPPersonCrime personCrime = new CPPersonCrime();
            long modelId = model.Id + 100000000000;
            personCrime.PersonCrimeId = GetSid(dictSid, SidType.PersonCrime, modelId);
            personCrime.PersonId = GetSid(dictSid, SidType.Person, casePersonId);
            personCrime.CrimeId = GetSid(dictSid, SidType.Crime, model.CaseCrimeId);
            personCrime.CaseCrimeId = model.CaseCrimeId;
            personCrime.CasePersonId = casePersonId;
            personCrime.EISPPNumber = eisppNumber;
            personCrime.CrimeSanction = new CrimeSanction()
            {
                CrimeSanctionId = GetSid(dictSid, SidType.CrimeSanction, modelId),
                Role = getNomValueInt(EisppMapping.PersonInCrimeRole, model.PersonRoleInCrimeId),
                SanctionType = SanctionType.ConnectToCrime,
                StructureId = senderStructure
            };
            if (featureVal == EISPPConstants.SentenceResultType.Innocence)
                personCrime.CrimeSanction.SanctionType = SanctionType.Innocence;

            personCrime.CrimeSubjectStatisticData = new CrimeSubjectStatisticData()
            {
                SubjectStatisticDataId = GetSid(dictSid, SidType.SubjectStatisticData, modelId),
                Relaps = getNomValueInt(EisppMapping.Relaps, model.RecidiveTypeId)
            };
            personCrime.IsSelected = true;
            return personCrime;
        }


        private CPPersonCrime InitCPPersonCrimeXML(NPRFZLPNEType personCrimeXML, EisppPerson person, int senderStructure, Dictionary<string, int> dictSid, ref int statisticDataId)
        {
            CPPersonCrime personCrime = new CPPersonCrime();
            personCrime.PersonCrimeId = personCrimeXML.fzlpnesid.ToInt();
            personCrime.PersonId = person.PersonId;
            personCrime.CrimeId = personCrimeXML.pnesid;
            personCrime.CrimeSanction = new CrimeSanction()
            {
                CrimeSanctionId = personCrimeXML.SCQ.scqsid.ToInt(),
                Role = personCrimeXML.SCQ.scqrlq.ToInt(),
                SanctionType = personCrimeXML.SCQ.scqvid.ToInt(),
                SanctionDate = personCrimeXML.SCQ.scqdta,
                SanctionReason = personCrimeXML.SCQ.scqosn.ToInt(),
                StructureId = senderStructure //personCrimeXML.SCQ.scqstr.ToInt()
            };
            if (personCrimeXML.SBC != null)
            {
                personCrime.CrimeSubjectStatisticData = new CrimeSubjectStatisticData()
                {
                    SubjectStatisticDataId = personCrimeXML.SBC.sbcsid.ToInt()
                };
            } else
            {
                statisticDataId++;
                personCrime.CrimeSubjectStatisticData = new CrimeSubjectStatisticData()
                {
                    SubjectStatisticDataId = GetSid(dictSid, SidType.SubjectStatisticDataXML, statisticDataId),
                };
            }
            personCrime.IsSelected = true;
            return personCrime;
        }
        private CPPersonCrime InitCPPersonCrimeFromPerson(Dictionary<string, int> dictSid, Crime crime, EisppPerson person, int senderStructure,int featureVal, ref int crimeNum)
        {
            CPPersonCrime personCrime = new CPPersonCrime();
            crimeNum++;
            personCrime.PersonCrimeId = GetSid(dictSid, SidType.PersonCrime, crimeNum);
            personCrime.PersonId = person.PersonId;
            personCrime.CrimeId = crime.CrimeId;
            personCrime.CrimeSanction = new CrimeSanction()
            {
                CrimeSanctionId = GetSid(dictSid, SidType.CrimeSanction, crimeNum),
                StructureId = senderStructure //personCrimeXML.SCQ.scqstr.ToInt()
            };
            personCrime.CrimeSubjectStatisticData = new CrimeSubjectStatisticData()
            {
                SubjectStatisticDataId = GetSid(dictSid, SidType.SubjectStatisticData, crimeNum)
            };
            if (featureVal == EISPPConstants.SentenceResultType.Innocence)
                personCrime.CrimeSanction.SanctionType = SanctionType.Innocence;
            return personCrime;
        }
        private CPPersonCrime InitCPPersonCrimeFromPersonXML(Dictionary<string, int> dictSid, Crime crime, EisppPerson person, int senderStructure, ref int crimeNum, Punishment[] punishments, int featureVal, ref int punishmentNum)
        {
            var personCrime = InitCPPersonCrimeFromPerson(dictSid, crime, person, senderStructure, featureVal, ref crimeNum);
            var crimePunishments = new List<CrimePunishment>();
            foreach (var punishment in punishments)
            {
                var crimePunishment = new CrimePunishment();
                if (punishment.PunishmentType == PunishmentType.Union)
                    continue;
                crimePunishment.CrimePunishmentId = GetSid(dictSid, SidType.CrimePunishment, punishmentNum);
                punishmentNum++;
                crimePunishment.Id = punishmentNum;
                crimePunishment.PunishmentKind = punishment.PunishmentKind;
                crimePunishments.Add(crimePunishment);
            }
            personCrime.CrimeSanction.CrimePunishments = crimePunishments.ToArray();
            personCrime.IsSelected = false;
            return personCrime;
        }

        private ProceduralCoercionMeasure[] InitProceduralCoercionMeasure(Dictionary<string, int> dictSid, int casePersonId, int senderStructure, int eventType, int? oldMeasureId, int? measureId)
        {
            var result = new List<ProceduralCoercionMeasure>();

            (var ruleIds, var flags) = rulesService.GetEisppRuleIds(eventType, "NPR.DLO.FZL.MPP.mppste");
            List<CasePersonMeasure> casePersonMeasures;
            var measureIsSelected = (eventType == EventType.CoercionMeasureChange ||
                eventType == EventType.CoercionMeasureCancellation ||
                eventType == EventType.CoercionMeasureCreate);
            if (measureIsSelected)
            {
                casePersonMeasures = repo.AllReadonly<CasePersonMeasure>()
                           .Include(x => x.MeasureInstitution)
                           .ThenInclude(x => x.InstitutionType)
                           .Include(x => x.MeasureCourt)
                           .Where(x => x.CasePersonId == casePersonId &&
                                       (x.Id == oldMeasureId || x.Id == measureId))
                           .ToList();
            }
            else
            {
                casePersonMeasures = repo.AllReadonly<CasePersonMeasure>()
                           .Include(x => x.MeasureInstitution)
                           .ThenInclude(x => x.InstitutionType)
                           .Include(x => x.MeasureCourt)
                           .Where(x => x.CasePersonId == casePersonId &&
                                       x.DateExpired == null &&
                                       (x.MQEpepIsSend ?? false) == false)
                           .ToList();
                if (ruleIds.Length > 0)
                {
                    casePersonMeasures = casePersonMeasures.Where(x => ruleIds.Contains(x.MeasureStatus.ToString())).ToList();
                }
            }
            foreach (var item in casePersonMeasures)
            {
                var eisppMeasure = new ProceduralCoercionMeasure()
                {
                    MeasureId = GetSid(dictSid, SidType.Measure, item.Id),
                    MeasureStatus = item.MeasureStatus.ToInt(),
                    MeasureStatusDate = item.MeasureStatusDate,
                    MeasureType = item.MeasureType.ToInt(),
                    BailAmount = item.BailAmount,
                    MeasureStructure = senderStructure,
                    MeasureInstitutionId = item.MeasureInstitutionId,
                    MeasureInstitutionTypeId = item.MeasureInstitution == null ? (int?)null : item.MeasureInstitution.InstitutionTypeId,
                    InstitutionName = item.MeasureInstitution == null ? "" : item.MeasureInstitution.FullName,
                    InstitutionTypeName = item.MeasureInstitution == null ? "" : item.MeasureInstitution.InstitutionType.Label,
                    IsSelected = true,
                    IsSelectedReadOnly = measureIsSelected
                };
                if (item.MeasureCourtId != null)
                {
                    eisppMeasure.MeasureInstitutionId = item.MeasureCourtId;
                    eisppMeasure.MeasureInstitutionTypeId = NomenclatureConstants.InstitutionTypes.Courts;
                    eisppMeasure.InstitutionName = item.MeasureCourt.Label;
                    eisppMeasure.InstitutionTypeName = "Съдилища";
                }
                result.Add(eisppMeasure);
            }
            return result.ToArray();
        }
        public async Task<bool> SaveCaseMigration(EisppEventVM model)
        {
            var packageModel = await GeneratePackage(model).ConfigureAwait(false);
            packageModel.SourceType = SourceTypeSelectVM.Case;
            packageModel.SourceId = model.CaseId;
            if (model.ExactCaseType != null)
                packageModel.Data.Events[0].CriminalProceeding.Case.ExactCaseType = model.ExactCaseType ?? 0;
            packageModel.IsForSend = true;
            if (model.EventType == EventType.SendCase)
            {
                packageModel.Data.Events[0].EisppSrok = null;
                packageModel.Data.Events[0].EventFeature = null;
            }
            return SaveCasePackageData(packageModel, null);
        }
        public bool SaveCasePackageData(EisppPackage model, int? eventFromId)
        {
            CPPersonCrimeUnion(model);
            var eventItem = repo.GetById<EisppEventItem>(model.Id);
            if (eventItem == null)
            {
                var eisppEvent = model.Data.Events.Where(x => x.EventKind == EventKind.NewEvent).FirstOrDefault();
                if (eisppEvent == null && model.Data.Events.Length > 0)
                    eisppEvent = model.Data.Events[0];

                int? caseSessionId = null;
                int? caseSessionActId = eisppEvent.CaseSessionActId.EmptyToNull(0);
                if (caseSessionActId != null)
                    caseSessionId = repo.AllReadonly<CaseSessionAct>()
                                        .Where(x => x.Id == caseSessionActId)
                                        .Select(x => x.CaseSessionId)
                                        .FirstOrDefault();
                eventItem = new EisppEventItem()
                {
                    EventDate = eisppEvent.EventDate,
                    EventType = eisppEvent.EventType,
                    CaseId = eisppEvent.CaseId,
                    SourceType = model.SourceType,
                    SourceId = model.SourceId,
                    CasePersonId = eisppEvent.CasePersonId,
                    CaseSessionActId = caseSessionActId,
                    CaseSessionId = caseSessionId,
                    EventFromId = eventFromId,
                    PersonMeasureId = model.PersonMeasureId,
                    PersonOldMeasureId = model.PersonOldMeasureId,
                };
                repo.Add(eventItem);
            }
            eventItem.UserId = userContext.UserId;
            eventItem.DateWrt = DateTime.Now;
            eventItem.RequestData = JsonConvert.SerializeObject(model);
            if (model.Data.Events.Any(x => x.EventKind == EventKind.OldEvent))
            {
                eventItem.EventTypeRules = EventType.DeleteEvent;
                if (model.Data.Events.Any(x => x.EventKind == EventKind.NewEvent))
                    eventItem.EventTypeRules = EventType.ChangeEvent;
            }
            var isDeleteEvent = model.Data.Events.Any(x => x.EventKind == EventKind.OldEvent) &&
                                !model.Data.Events.Any(x => x.EventKind == EventKind.NewEvent);
            if (isDeleteEvent && eventItem.EventFromId > 0)
            {
                var eventItemFrom = repo.GetById<EisppEventItem>(eventItem.EventFromId);
                eventItemFrom.DateExpired = DateTime.Now;
                eventItemFrom.UserExpiredId = userContext.UserId;
            }

            repo.SaveChanges();
            model.Id = eventItem.Id;
            if (model.IsForSend)
            {
                var modelXml = JsonConvert.DeserializeObject<EisppPackage>(eventItem.RequestData); 
                rulesService.SetIsSelectedAndClear(modelXml);
                rulesService.CreatePunismentFromProbationMeasuares(modelXml);
                string eisppMessage = XmlUtils.SerializeEisppPackage(modelXml);
                long id = mqService.InitMQFromString(NomenclatureConstants.IntegrationTypes.EISPP, model.SourceType, model.SourceId, EpepConstants.ServiceMethod.Update, eventItem.Id, eisppMessage);
                eventItem.MQEpepId = id;
                repo.Update(eventItem);
                repo.SaveChanges();
                model.IsForEdit = false;
            }
            return true;
        }
      
        public string CheckSum(string code)
        {
            string codeLetters = configuration.GetValue<string>("Eispp:Constants:codeLetters");
            int codeLength = codeLetters.Length;
            int[] weights = configuration.GetSection("Eispp:Constants:codeWeights").Get<int[]>();
            int count = weights.Length;
            int modulCheckSum = configuration.GetValue<int>("Eispp:Constants:modulCheckSum");
            if (code.Length < count)
                return "";
            int result = 0;
            for (int i = 1; i <= count; i++)
            {
                int pos = count - i;
                var symbol = code[pos];
                int number = 0;
                if (codeLetters.Contains(symbol, StringComparison.InvariantCultureIgnoreCase))
                {
                    number = codeLetters.IndexOf(symbol, StringComparison.InvariantCultureIgnoreCase);
                }
                else
                {
                    _ = int.TryParse(symbol.ToString(), out number);
                }
                result += weights[pos] * number;
                result = result % modulCheckSum;
            }
            int code1 = (result / codeLength) % codeLength;
            int code2 = result % codeLength;
            return codeLetters[code1].ToString() + codeLetters[code2].ToString();
        }

        public EisppEventVM GetEisppEventVM(string sourceType, string sourceId, int caseId, int? caseSessionActId)
        {
            var caseModel = repo.AllReadonly<Case>()
                                    .Include(x => x.CaseType)
                                    .Where(x => x.Id == caseId)
                                    .FirstOrDefault();
            if (caseModel == null)
                return null;
            if (string.IsNullOrEmpty(caseModel.EISSPNumber) && IsForEisppNum(caseModel))
            {
                MakeEisppNumberNP(caseModel);
                repo.Update(caseModel);
                repo.SaveChanges();
            } 
            return new EisppEventVM()
            {
                CaseId = caseId,
                CaseType = caseModel.CaseType.Label ?? "",
                SourceType = sourceType,
                SourceId = sourceId,
                Year = caseModel.RegDate.Year,
                ShortNumber = caseModel.ShortNumberValue ?? 0,
                CaseCodeId = caseModel.CaseCodeId ?? 0,
                CaseTypeId = caseModel.CaseTypeId,
                CaseSessionActId = caseSessionActId.EmptyToNull(),
                EISPPNumber = caseModel.EISSPNumber ?? ""
            };
        }
        public IQueryable<EisppEventItemVM> GetPackages(EisppEventFilterVM filter)
        {
            filter.CaseRegNumber = filter.CaseRegNumber.ToShortCaseNumber() ?? filter.CaseRegNumber;
            var eventTypes = repo.AllReadonly<EisppTblElement>()
                       .Where(x => x.EisppTblCode == EisppTableCode.EventType);
            var eventListFrom = repo.AllReadonly<EisppEventItem>()
                      .Where(x => x.CaseId == filter.CaseId);

            var eventList = repo.AllReadonly<EisppEventItem>()
                                .Where(x => x.DateExpired == null &&
                                   (filter.CaseId <= 0 || x.CaseId == filter.CaseId) &&
                                   (filter.CourtId <= 0 || x.Case.CourtId == filter.CourtId) && 
                                   (filter.EventTypeId <= 0 || x.EventType == filter.EventTypeId) &&
                                   (filter.EventDateFrom == null || x.EventDate >= filter.EventDateFrom.Value.Date) &&
                                   (filter.EventDateTo == null || x.EventDate.Value.Date <= filter.EventDateTo) &&
                                   (string.IsNullOrEmpty(filter.CaseRegNumber) || EF.Functions.ILike(x.Case.RegNumber ?? "", filter.CaseRegNumber.ToPaternSearch()) )
                              );
            if (filter.LinkType > 0)
            {
                if (filter.LinkType == 1)
                {
                    eventList = eventList.Where(x => x.CaseSessionActId == null);
                }
                else
                {
                    eventList = eventList.Where(x => x.CaseSessionActId != null &&
                                                     (filter.SessionActId <= 0 || x.CaseSessionActId == filter.SessionActId) &&
                                                     (filter.ActDateFrom == null || x.CaseSessionAct.RegDate >= filter.ActDateFrom.Value.Date) &&
                                                     (filter.ActDateTo == null || x.CaseSessionAct.RegDate.Value.Date <= filter.ActDateTo)
                                            );
                }
            }
            if (filter.IntegrationStateId == -3)
            {
                eventList = eventList.Where(x => x.MQEpep.IntegrationStateId != IntegrationStates.TransferOK);
            }
            if (filter.IntegrationStateId == -2)
            {
                eventList = eventList.Where(x => x.MQEpepId == null);
            }
            if (filter.IntegrationStateId > 0)
            {
                eventList = eventList.Where(x => x.MQEpep.IntegrationStateId == filter.IntegrationStateId);
            }

            var zeroDate = new DateTime(1000, 1, 1);
            return eventList.Select(x => new EisppEventItemVM()
            {
                Id = x.Id,
                EventDate = x.EventDate,
                EventTypeName = eventTypes.Where(e => e.Code == x.EventType.ToString()).Select(e => e.Label).FirstOrDefault() ?? "",
                PersonName = x.CasePerson.FullName ?? "",
                SessionAct = x.CaseSessionAct != null ? $"{x.CaseSessionAct.ActType.Label} {x.CaseSessionAct.RegNumber} / {x.CaseSessionAct.RegDate}" : "",
                DateExpired = x.DateExpired,
                ErrorDescription = x.MQEpep.ErrorDescription ?? "",
                StatusTransfer = x.MQEpep.IntegrationState.Label ?? "",
                MqEpepId = x.MQEpepId,
                IntegrationStateId = x.MQEpep.IntegrationStateId ?? 0,
                CaseRegDate = x.Case.RegDate,
                CaseRegNum = x.Case.RegNumber,
                EventFromId = x.EventFromId,
                EventLink = x.EventFromId > 0 ? eventListFrom
                                                  .Where(f => f.Id == x.EventFromId)
                                                  .Select(f => (f.EventTypeRules == EventType.DeleteEvent ? "Корекция " : x.EventTypeRules == EventType.DeleteEvent ? "Изтриване " : "") +
                                                               $" към {f.Id} / " + (f.EventDate > zeroDate ?  $"{f.EventDate:dd.MM.yyyy}" : "") ).FirstOrDefault()
                                              : string.Join(Environment.NewLine,
                                                    eventListFrom.Where(f => x.Id == f.EventFromId)
                                                       .Select(f => (f.EventTypeRules == EventType.DeleteEvent ? "Коригирано " : x.EventTypeRules == EventType.DeleteEvent ? "Изтрито " : "") +
                                                            $" с {f.Id} / " + (f.EventDate > zeroDate ? $"{f.EventDate:dd.MM.yyyy}" : "")))
            });
        }
        public List<SelectListItem> GetLinkTypeDDL(bool addDefaultElement = true)
        {
            var selectListItems = new List<SelectListItem>();
            selectListItems.Insert(0, new SelectListItem() { Text = "Акт/Протокол", Value = "2" });
            selectListItems.Insert(0, new SelectListItem() { Text = "Дело", Value = "1" });
            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }
            return selectListItems;
        }

        public List<SelectListItem> GetIntegrationStateDDL()
        {
            var selectListItems = repo.AllReadonly<IntegrationState>()
                                       .Where(x => x.Id == IntegrationStates.New ||
                                                   x.Id == IntegrationStates.DataContentError ||
                                                   x.Id == IntegrationStates.TransferError ||
                                                   x.Id == IntegrationStates.WaitingForReply ||
                                                   x.Id == IntegrationStates.TransferErrorLimitExceeded ||
                                                   x.Id == IntegrationStates.TransferOK ||
                                                   x.Id == IntegrationStates.ReplyContainsError )
                                       .Select(x => new SelectListItem() { 
                                           Value = x.Id.ToString(),
                                           Text = x.Id == IntegrationStates.DataContentError ? "Грешни данни": x.Label
                                       })
                                       .ToList();
            selectListItems.Insert(0, new SelectListItem() { Text = "Неизпратени", Value = "-2" });
            selectListItems.Insert(0, new SelectListItem() { Text = "Проблемни", Value = "-3" });
            selectListItems.Insert(0, new SelectListItem() { Text = "Избери", Value = "0" });
            return selectListItems;
        }

        public List<SelectListItem> CaseSessionActDDL(int caseId, int? eventTypeId, DateTime? DateFrom, DateTime? DateTo, string defaultText = "Избери")
        {

            var caseSessionAct = repo.AllReadonly<CaseSessionAct>()
               .Include(x => x.CaseSession)
               .ThenInclude(x => x.Case)
               .Include(x => x.CaseSession)
               .ThenInclude(x => x.SessionType)
               .Include(x => x.ActType)
               .Include(x => x.ActState)
               .Where(x => x.DateExpired == null)
               .Where(x => x.ActDeclaredDate != null || x.ActInforcedDate != null)
               .Where(x => (DateFrom == null || x.RegDate.Value.Date >= DateFrom.Value.Date) &&
                           (DateTo == null || x.RegDate.Value.Date <= DateTo) &&
                           x.CaseSession.Case.Id == caseId);
            if (eventTypeId > 0)
            {
                (var ruleIds, var flags) = rulesService.GetEisppRuleIds(eventTypeId ?? 0, "sbedkpvid");
                var mappings = repo.AllReadonly<CodeMapping>()
                                   .Where(x => x.Alias == EISPPConstants.EisppMapping.MappingActType &&
                                               ruleIds.Any(r => r == x.InnerCode));
                if (ruleIds.Any())
                {
                    caseSessionAct = caseSessionAct.Where(x => mappings.Any(m => m.OuterCode == x.ActTypeId.ToString()));
                }
            }
            caseSessionAct = caseSessionAct.OrderBy(x => x.Id);
            var selectListItems = caseSessionAct
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = ((x.ActType != null) ? x.ActType.Label : "") + x.RegNumber + "/" + x.RegDate.Value.ToString("dd.MM.yyyy")
                })
                .ToList();
            selectListItems = selectListItems
                   .Prepend(new SelectListItem() { Text = defaultText, Value = "0" })
                   .ToList();
            return selectListItems;
        }


        public string GetPunishmentServingTypeMode(int servingTypeId)
        {
            return repo.AllReadonly<CodeMapping>()
                       .Where(x => x.Alias == EISPPConstants.EisppMapping.PunismentPeriodMap && x.OuterCode == servingTypeId.ToString())
                       .Select(x => x.InnerCode)
                       .FirstOrDefault() ?? "";
        }

        public string GetPbcMeasureUnit(int pbcMeasureTypeId)
        {
            return repo.AllReadonly<CodeMapping>()
                       .Where(x => x.Alias == EisppMapping.MappingPBC_TypeValue && x.OuterCode == pbcMeasureTypeId.ToString())
                       .Select(x => x.InnerCode)
                       .FirstOrDefault() ?? "";
        }

        public (EisppDropDownVM, string, int, bool, bool) GetPunishmentPeriodMode(int eventType, int punishmentKind, int servingType)
        {
            int def_val = 0;
            var ddl = GetDDL_EISPPTblElementWithRules(EisppTableCode.ServingType, eventType, "NPR.DLO.FZL.NKZ.nkzncn");
            bool showRegim = false;
            bool showServingType = false;
            if (
                punishmentKind == PunismentKind.nkz_dojiv_zatvor ||
                punishmentKind == PunismentKind.nkz_dojiv_zatvor_bez_zamiana ||
                (punishmentKind == PunismentKind.nkz_lishavane_ot_svoboda && servingType == ServingType.Efective)
               )
            {
                showRegim = true;
            }
            var punismentPeriod = repo.AllReadonly<CodeMapping>()
                       .Where(x => x.Alias == EisppMapping.PunismentPeriodMap)
                       .ToList();
            var punishmentKindMode = rulesService.GetPunishmentKindMode(punishmentKind);
   
            ddl.DDList = ddl.DDList.Where(x => x.Value == "0" || punismentPeriod.Any(p => p.OuterCode == x.Value && p.InnerCode == punishmentKindMode)).ToList();
            if (ddl.DDList.Count == 2)
            {
                def_val = (ddl.DDList.Where(x => x.Value != "0").OrderBy(x => x.Value).Select(x => x.Value).FirstOrDefault() ?? "0").ToInt();
                if (def_val == ServingType.EarlyRelease)
                    showServingType = true;
            }
            if (ddl.DDList.Count > 2)
            {
                if (ddl.DDList.Any(x => x.Value == servingType.ToString()))
                    def_val = servingType;
                showServingType = true;
            }
            if (ddl.DDList.Count <= 1 && !string.IsNullOrEmpty(punishmentKindMode))
            {
                ddl = GetDDL_EISPPTblElementWithRules(EisppTableCode.ServingType, eventType, "NPR.DLO.FZL.NKZ.nkzncn");
                showServingType = true;
            }
            if (punishmentKindMode == PunishmentVal.period && (def_val == ServingType.Efective || def_val == ServingType.EarlyRelease))
            {
                punishmentKindMode = PunishmentVal.effective_period;
            }
            if (punishmentKindMode == PunishmentVal.period && def_val == ServingType.Probation)
            {
                punishmentKindMode = PunishmentVal.probation_period;
            }
            return (ddl, punishmentKindMode, def_val, showRegim, showServingType);
        }
        public bool SaveExpireInfoPlus(ExpiredInfoVM model)
        {
            var saved = repo.GetById<EisppEventItem>(model.Id);
            if (saved != null)
            {
                saved.DateExpired = DateTime.Now;
                saved.UserExpiredId = userContext.UserId;
                saved.DescriptionExpired = model.DescriptionExpired;
                repo.Update(saved);
                repo.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }

        }

        private EisppEventItem GetEisppEvent(int id)
        {
            return repo.AllReadonly<EisppEventItem>()
                            .Where(x => x.Id == id)
                            .Include(x => x.MQEpep)
                            .FirstOrDefault();
        }
        public async Task<CdnDownloadResult> GetNPCard(int id)
        {
            var model = GetEisppEvent(id);
            if (model.MQEpep != null)
            {
                CdnItemVM aFile = cdnService.Select(SourceTypeSelectVM.Integration_EISPP_CardNP, model.MQEpep.ParentSourceId.ToString()).FirstOrDefault();
                if (aFile != null)
                    return await cdnService.MongoCdn_Download(aFile).ConfigureAwait(false);
            }
            return null;
        }
        public async Task<CdnDownloadResult> GetEisppResponse(int id)
        {
            var model = GetEisppEvent(id);
            CdnItemVM aFile = cdnService.Select(SourceTypeSelectVM.Integration_EISPP_Response, model.MQEpep.MQId).FirstOrDefault();
            if (aFile != null)
                return await cdnService.MongoCdn_Download(aFile).ConfigureAwait(false);
            return null;
        }
        public byte[] GetEisppRequest(int id)
        {
            var model = GetEisppEvent(id);
            return model?.MQEpep?.Content;
        }

        public List<SelectListItem> GetPersonProceduralCoercionMeasure(int casePersonId, bool isOld, int eventId, bool addDefaultElement = true)
        {
            var casePersonMeasures = repo.AllReadonly<CasePersonMeasure>()
                       .Include(x => x.MeasureInstitution)
                       .ThenInclude(x => x.InstitutionType)
                       .Include(x => x.MeasureCourt)
                       .Where(x => x.CasePersonId == casePersonId &&
                                   x.DateExpired == null)
                       .ToList();
            if (eventId == EventType.CoercionMeasureRefused)
            {
                casePersonMeasures = casePersonMeasures.Where(x => x.MeasureStatus == PersonProceduralCoercionMeasureStatus.Refused).ToList();
            }
            else
            {
                if (isOld)
                {
                    casePersonMeasures = casePersonMeasures.Where(x => x.MeasureStatus == PersonProceduralCoercionMeasureStatus.Canceled).ToList();
                }
                else
                {
                    casePersonMeasures = casePersonMeasures.Where(x => x.MeasureStatus == PersonProceduralCoercionMeasureStatus.Imposed).ToList();
                }
            }
            var selectListItems = casePersonMeasures
                            .OrderBy(x => x.Id)
                            .Select(x => new SelectListItem()
                            {
                                Text = (x.MeasureCourt != null ? x.MeasureCourt.Label : x.MeasureInstitution.FullName) + " " + x.MeasureTypeLabel,
                                Value = x.Id.ToString()
                            }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        private bool IsForEisppNumOnDocType(Case caseCurrent, Document document, string typeEispGroup)
        {
            var docTypes= repo.AllReadonly<CodeMapping>()
                            .Where(x => x.Alias == EisppMapping.GeneriraneNumDoc &&
                                        x.OuterCode == typeEispGroup)
                            .Select(x => x.InnerCode)
                            .ToList();
            foreach (var docType in docTypes)
            {
                var docTypeId = docType.ToInt();
                if (document.DocumentTypeId == docTypeId)
                {
                    var caseCodes = repo.AllReadonly<CaseCode>();
                    if (repo.AllReadonly<EisppTblElement>().Any(x => x.EisppTblCode == typeEispGroup && caseCodes.Any(c => c.Id == caseCurrent.CaseCodeId && c.Code == x.Code)))
                        return true;
                } 
            }
            return false;
        }
        public bool IsForEisppNum(Case caseCurrent)
        {
            if (string.IsNullOrEmpty(caseCurrent.RegNumber))
                return false;
            if (caseCurrent.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo)
                return false;
            if (caseCurrent.CaseTypeId != NomenclatureConstants.CaseTypes.NChHD &&
                caseCurrent.CaseTypeId != NomenclatureConstants.CaseTypes.ChND 
                )
                return false;

            var document = caseCurrent.Document;
            if (document == null)
                document = repo.AllReadonly<Document>()
                               .FirstOrDefault(x => x.Id == caseCurrent.DocumentId);
            if (document == null)
                return false;
            if (IsForEisppNumOnDocType(caseCurrent, document, GeneriraneDocGroup.Tujba))
                return true;
            if (IsForEisppNumOnDocType(caseCurrent, document, GeneriraneDocGroup.Iskane))
                return true;
            return false;
        }
        private string GetPrefixNum(string structureId)
        {
            return repo.AllReadonly<CodeMapping>()
                       .Where(x => x.Alias == EisppMapping.NumberPrefix && x.OuterCode == structureId)
                       .Select(x => x.InnerCode)
                       .FirstOrDefault();
        }
        public bool MakeEisppNumberNP(Case caseCurrent)
        {
            var senderStructure = repo.AllReadonly<Court>()
                                      .Where(x => x.Id == caseCurrent.CourtId)
                                      .Select(x => x.EISPPCode)
                                      .FirstOrDefault();
            var prefixNum = GetPrefixNum(senderStructure);
            if (prefixNum == null)
                return false;
            int yearEispp = caseCurrent.RegDate.Year - 1800;
            int counter = int.Parse(counterService.Counter_GetCaseEisppNumber(caseCurrent.CourtId));
            string eisppNum = $"{prefixNum}{yearEispp}{counter:00000}Г";
            eisppNum += CheckSum(eisppNum);
            caseCurrent.EISSPNumber = eisppNum;
            return true;
        }
        public bool MakeEisppNumberPNE(CaseCrime caseCrime, int courtId)
        {
            var senderStructure = repo.AllReadonly<Court>()
                                     .Where(x => x.Id == courtId)
                                     .Select(x => x.EISPPCode)
                                     .FirstOrDefault();
            var prefixNum = GetPrefixNum(senderStructure);
            if (prefixNum == null)
                return false;
            int yearEispp = caseCrime.DateFrom.Year - 1800;
            int counter = int.Parse(counterService.Counter_GetCrimeEisppNumber(courtId));
            string eisppNum = $"{prefixNum}{yearEispp}{counter:00000}Б";
            eisppNum += CheckSum(eisppNum);
            caseCrime.EISSPNumber = eisppNum;
            return true;
        }
        public string GetEisppNumber(int caseId)
        {
            var caseModel = repo.AllReadonly<Case>()
                              .Where(x => x.Id == caseId)
                              .FirstOrDefault();
            return caseModel.EISSPNumber;
        }
        public async Task<List<SelectListItem>> GetDDL_PneNumbers(int caseId, string eisppNumber)
        {
            var caseCrimes = repo.AllReadonly<CaseCrime>()
                                 .Where(x => x.CaseId == caseId)
                                 .ToList();
            var eisppResponse = await GetTSAKTSTSResponse(eisppNumber).ConfigureAwait(false);
            var result = new List<SelectListItem>();
            var xmlCrimes = eisppResponse?.execTSAKTSTSResponse?.sNPRAKTSTS?.sPNE ?? Array.Empty<PNEType>();
            xmlCrimes = xmlCrimes.Where(x => !caseCrimes.Any(c => c.EISSPNumber == x.pnenmr)).ToArray();
            foreach (var crime in xmlCrimes)
            {
                result.Add(new SelectListItem()
                {
                    Value = crime.pnenmr,
                    Text = crime.pnenmr
                });
            }
            return result; 
        }
        public string GetElementLabel(string code)
        {
            return repo.AllReadonly<EisppTblElement>()
                       .Where(x => x.Code == code)
                       .Select(x => x.Label)
                       .FirstOrDefault() ?? "";
        }
        public bool HaveEventForMeasure(int measureId)
        {
            var caseId = repo.AllReadonly<CasePersonMeasure>()
                             .Where(x => x.Id == measureId)
                             .Select(x => x.CaseId)
                             .FirstOrDefault();
            return repo.AllReadonly<EisppEventItem>()
                      .Where(x => x.CaseId == caseId &&
                                  x.DateExpired == null &&
                                  x.MQEpep.IntegrationStateId == IntegrationStates.TransferOK &&
                                  (x.PersonMeasureId == measureId || x.PersonOldMeasureId == measureId))
                      .Any();
        }
        public bool HaveEventForPunishment(int casePersonSentencePunishmentId)
        {
            var caseId = repo.AllReadonly<CasePersonSentencePunishment>()
                             .Where(x => x.Id == casePersonSentencePunishmentId)
                             .Select(x => x.CaseId)
                             .FirstOrDefault();
            var eisppEvents = repo.AllReadonly<EisppEventItem>()
                                  .Where(x => x.CaseId == caseId &&
                                         x.DateExpired == null &&
                                         x.MQEpep.IntegrationStateId == IntegrationStates.TransferOK);
            foreach(var eisppEvent in eisppEvents)
            {
                var model = JsonConvert.DeserializeObject<EisppPackage>(eisppEvent.RequestData);
                if (model.Data.Events[0].CriminalProceeding.Case.Persons?.Any(x => x.Punishments?.Any(p => p.CasePersonSentencePunishmentId == casePersonSentencePunishmentId) == true) == true)
                    return true;

            }
            return false;
        }
        public bool HaveEventForCrime(int caseCrimeId)
        {
            var caseCrime = repo.AllReadonly<CaseCrime>()
                             .Where(x => x.Id == caseCrimeId)
                             .FirstOrDefault();
            var haveDoub = repo.AllReadonly<CaseCrime>()
                               .Where(x => x.Id != caseCrimeId && x.EISSPNumber == caseCrime.EISSPNumber && x.DateExpired == null)
                               .Any();
            if (haveDoub)
                return false;
            var eisppEvents = repo.AllReadonly<EisppEventItem>()
                                  .Where(x => x.CaseId == caseCrime.CaseId &&
                                         x.DateExpired == null && 
                                         x.MQEpep.IntegrationStateId == IntegrationStates.TransferOK);
            foreach (var eisppEvent in eisppEvents)
            {
                var model = JsonConvert.DeserializeObject<EisppPackage>(eisppEvent.RequestData);
                if (model.Data.Events[0].CriminalProceeding.Case.Crimes?.Any(x => x.EisppNumber == caseCrime.EISSPNumber) == true)
                    return true;
            }
            return false;
        }
        public List<SelectListItem> DocumentComplaintDDL(int caseId)
        {
            var codeMapping = repo.AllReadonly<CodeMapping>()
                                          .Where(x => x.Alias == EisppMapping.ComplaintType);

            var result = repo.AllReadonly<DocumentCaseInfo>()
                            .Where(x => x.CaseId == caseId &&
                                        codeMapping.Any(c => c.InnerCode == x.Document.DocumentTypeId.ToString()) &&
                                        x.Document.DateExpired == null)
                             .Select(x => new SelectListItem()
                             {
                                 Text = x.Document.DocumentType.Label,
                                 Value = x.DocumentId.ToString()
                             }).ToList() ?? new List<SelectListItem>();

            result = result
                .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                .ToList();
            return result;
        }

        public bool GetCaseIsExternal(int caseId)
        {
            var aCase = repo.AllReadonly<Case>()
                            .Include(x => x.Document)
                            .Where(x => x.Id == caseId)
                            .FirstOrDefault();
            return (aCase?.Document?.DocumentTypeId == InitDocumentType.IskaneExternal);
        }
    }
}
