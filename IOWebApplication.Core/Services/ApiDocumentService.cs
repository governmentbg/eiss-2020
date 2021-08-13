// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.ApiModels.Common;
using IOWebApplication.Infrastructure.Data.ApiModels.Contracts;
using IOWebApplication.Infrastructure.Data.ApiModels.Doc;
using IOWebApplication.Infrastructure.Data.ApiModels.DocumentRequests;
using IOWebApplication.Infrastructure.Data.ApiModels.FastProcess;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.InitialData;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace IOWebApplication.Core.Services
{
    public class ApiDocumentService : BaseService, IApiDocumentService
    {
        private readonly IDocumentService docService;
        private readonly ICdnService cdnService;
        public ApiDocumentService(IRepository _repo,
                               IUserContext _userContext,
                               ILogger<ApiDocumentService> _logger,
                               IDocumentService _docService,
                               ICdnService _cdnService)
        {
            repo = _repo;
            userContext = _userContext;
            logger = _logger;
            docService = _docService;
            cdnService = _cdnService;
        }


        public async Task<bool> UpdateFastProcessFromData(int caseId)
        {

            // return false;




            //TODO SAVE - само ако няма данни в CaseMoneyClaim!!!!
            //ако има данни - return false;
            if (repo.AllReadonly<CaseMoneyClaim>().Any(x => x.CaseId == caseId))
                return false;

            var caseModel = repo.GetById<Case>(caseId);
            var documentModel = repo.GetById<Document>(caseModel.DocumentId);
            //ако документа не е подаден през портала - return false
            if (documentModel.DeliveryGroupId != DocumentConstants.DeliveryGroups.WebPortal)
            {
                return false;
            }
            var requestContent = await cdnService.LoadHtmlFileTemplate(new Infrastructure.Models.Cdn.CdnFileSelect()
            { SourceType = SourceTypeSelectVM.DocumentFileFromAPI, SourceId = caseModel.DocumentId.ToString() }).ConfigureAwait(true);

            if (string.IsNullOrEmpty(requestContent))
            {
                return false;
            }

            DocumentRequestFastProcess dataModel = null;
            FastProcessModel fastProcessModel = null;
            try
            {
                dataModel = JsonConvert.DeserializeObject<DocumentRequestFastProcess>(requestContent);

                if (dataModel == null)
                {
                    throw new Exception($"DocumentRequestFastProcess error: DocId = {caseModel.DocumentId}");
                }

                fastProcessModel = dataModel.Data;

                if (fastProcessModel == null)
                {
                    throw new Exception($"FastProcessModel error: DocId = {caseModel.DocumentId}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ApiDocumentService.UpdateFastProcessFromData");
            }


            var bankAccounts = FillBankAccount(fastProcessModel.BankAccounts, caseModel);
            var moneyClaims = FillMoneyClaim(fastProcessModel.MoneyClaims, caseModel);
            var moneyExpenses = FillMoneyExpense(fastProcessModel.Exprenses, caseModel);

            var caseFastProcess = new CaseFastProcess()
            {
                CourtId = caseModel.CourtId,
                CaseId = caseModel.Id,
                CurrencyId = getNomIdByCode<Currency>(fastProcessModel.Currency),
                TaxAmount = fastProcessModel.TaxAmount,
                Description = fastProcessModel.Description
            };

            try
            {
                using (TransactionScope ts = TransactionScopeBuilder.CreateReadCommitted())
                {
                    repo.AddRange(bankAccounts);
                    repo.AddRange(moneyClaims);
                    repo.AddRange(moneyExpenses);
                    repo.Add(caseFastProcess);

                    repo.SaveChanges();

                    foreach (var col in moneyClaims.SelectMany(x => x.CaseMoneyCollections).ToArray())
                    {

                        var subCollections = fastProcessModel.MoneyClaims.SelectMany(x => x.ClaimMoney)
                                                .Where(x => x.MainClaimMoneyId == col.ApiModelId).ToArray();
                        if (subCollections.Any())
                        {
                            var subClaims = FillMoneyCollectionWithMain(subCollections, caseModel);
                            foreach (var item in subClaims)
                            {
                                item.CaseMoneyClaimId = col.CaseMoneyClaimId;
                                item.MainCaseMoneyCollectionId = col.Id;
                            }

                            repo.AddRange(subClaims);
                            repo.SaveChanges();
                        }
                    }

                    ts.Complete();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private int getCodeMapIdByCode(string alias, string outerCode)
        {
            var innerId = repo.AllReadonly<CodeMapping>()
                                .Where(x => x.Alias == alias && x.OuterCode == outerCode)
                                .Select(x => x.InnerCode)
                                .FirstOrDefault();

            int result = 0;
            if (int.TryParse(innerId, out result))
            {
                return result;
            }
            else
            {
                return 0;
            }
        }
        private int getNomIdByCode<T>(string code) where T : class, ICommonNomenclature
        {
            return repo.AllReadonly<T>()
                                .Where(x => x.Code == code)
                                .Where(x => x.DateEnd == null && x.IsActive == true)
                                .Select(x => x.Id)
                                .FirstOrDefault();

        }

        private DocumentVM mapDocumentToEntity(DocumentModel model)
        {
            if (model == null)
            {
                return null;
            }

            var document = new DocumentVM()
            {
                CourtId = getNomIdByCode<Court>(model.CourtCode),
                DeliveryGroupId = DocumentConstants.DeliveryGroups.WebPortal,
                DocumentTypeId = getCodeMapIdByCode("epep_in_doctype", model.DocumentType),
                Description = model.Description,
                CaseClassifications = new List<CheckListVM>()
            };

            document.DocumentTypeId = 11;//Заявление за издаване заповед за изпълнение

            var docTypeInfo = repo.AllReadonly<DocumentType>()
                                    .Include(x => x.DocumentGroup)
                                    .ThenInclude(x => x.DocumentKind)
                                     .Where(x => x.Id == document.DocumentTypeId)
                                     .Select(x => new
                                     {
                                         docGroup = x.DocumentGroupId,
                                         documentKind = (x.DocumentGroup != null) ? x.DocumentGroup.DocumentKindId : 0,
                                         documentDir = (x.DocumentGroup.DocumentKind != null) ? x.DocumentGroup.DocumentKind.DocumentDirectionId : 0,
                                     })
                                     .FirstOrDefault();

            if (docTypeInfo != null)
            {
                document.DocumentGroupId = docTypeInfo.docGroup;
                document.DocumentKindId = docTypeInfo.documentKind;
                document.DocumentDirectionId = docTypeInfo.documentDir;
            }

            if (!string.IsNullOrEmpty(model.CaseType))
            {
                document.CaseTypeId = getCodeMapIdByCode("epep_casetype", model.CaseType);
                document.CaseCodeId = getNomIdByCode<CaseCode>(model.CaseCode);
                document.ProcessPriorityId = NomenclatureConstants.ProcessPriority.GeneralOrder;
            }

            foreach (var item in model.DocumentPersons)
            {
                var presonItem = mapDocumentPersonToEntity(item);
                if (presonItem != null)
                {
                    document.DocumentPersons.Add(presonItem);
                }
            }
            return document;
        }


        private DocumentPersonAddressVM mapDocumentPersonAddressToEntity(DocumentPersonAddressModel model)
        {
            if (model == null)
            {
                return null;
            }
            var address = new DocumentPersonAddressVM();
            address.Address.AddressTypeId = getNomIdByCode<AddressType>(model.AddressType);
            address.Address.CountryCode = model.CountryCode;
            address.Address.CityCode = model.CityCode;
            address.Address.ForeignAddress = model.ForeignAddress;
            address.Address.Description = model.StreetFullAddress;
            //address.Address.StreetNumber = model.StreetNumber.ToSafeInt()
            return address;
        }

        private DocumentPersonVM mapDocumentPersonToEntity(DocumentPersonModel model)
        {
            if (model == null)
            {
                return null;
            }
            var person = new DocumentPersonVM()
            {
                Uic = model.Uic,
                UicTypeId = getNomIdByCode<UicType>(model.UicType),
                PersonRoleId = getCodeMapIdByCode("epep_personrole", model.PersonRole),
                Person_SourceCode = model.DocumentPersonId,
                Person_SourceType = SourceTypeSelectVM.WebApiPerson
            };
            switch (person.UicTypeId)
            {
                case NomenclatureConstants.UicTypes.EGN:
                case NomenclatureConstants.UicTypes.LNCh:
                case NomenclatureConstants.UicTypes.BirthDate:
                    person.FirstName = model.FirstName;
                    person.MiddleName = model.MiddleName;
                    person.FamilyName = model.FamilyName;
                    person.Family2Name = model.Family2Name;
                    person.FullName = $"{model.FirstName} {model.MiddleName} {model.FamilyName}".TrimSpaces();
                    if (!string.IsNullOrEmpty(model.Family2Name))
                    {
                        person.FullName += $"-{model.Family2Name}";
                    }
                    break;
                case NomenclatureConstants.UicTypes.EIK:
                case NomenclatureConstants.UicTypes.Bulstat:
                    person.FullName = model.EntityName;
                    break;
                default:
                    break;
            }
            if (model.Addresses != null)
                foreach (var item in model.Addresses)
                {
                    var adr = mapDocumentPersonAddressToEntity(item);
                    if (adr != null)
                    {
                        person.Addresses.Add(adr);
                    }
                }
            return person;
        }
        public async Task<DocumentResponseModel> RegisterDocumentAsync(IDocumentRequest model)
        {
            var document = mapDocumentToEntity(model.Document);
            var isOk = await docService.Document_SaveData(document);
            throw new NotImplementedException();
        }

        private List<CasePerson> GetCasePerson(int caseId)
        {
            return repo.AllReadonly<CasePerson>()
                .Include(x => x.PersonRole)
                .Include(x => x.Addresses)
                .Where(x => x.CaseId == caseId &&
                            x.CaseSessionId == null &&
                            x.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.RightSide)
                .ToList();
        }

        /// <summary>
        /// Попълва моделите за банкови сметки
        /// </summary>
        /// <param name="models"></param>
        /// <param name="modelCase"></param>
        /// <returns></returns>
        private List<CaseBankAccount> FillBankAccount(FastProcessBankAccountModel[] models, Case modelCase)
        {
            var result = new List<CaseBankAccount>();
            if (models == null)
            {
                return result;
            }
            var caseBankAccountTypes = repo.AllReadonly<CaseBankAccountType>().Where(x => x.IsActive);

            // CasePersonId - не се подава
            foreach (var bankAccountModel in models)
            {
                var item = new CaseBankAccount()
                {
                    CourtId = modelCase.CourtId,
                    CaseId = modelCase.Id,
                    CaseBankAccountTypeId = getNomIdByCode<CaseBankAccountType>(bankAccountModel.BankAccountType),
                    IBAN = bankAccountModel.IBAN,
                    BIC = bankAccountModel.BIC,
                    BankName = bankAccountModel.BankName,
                    Description = bankAccountModel.Description,
                    UserId = userContext.UserId,
                    DateWrt = DateTime.Now,
                    CasePersonId = null
                };

                result.Add(item);
            }

            return result;
        }

        private List<CaseMoneyCollectionPerson> FillMoneyCollectionPerson(FastProcessDistributionModel[] models, Case modelCase)
        {
            var result = new List<CaseMoneyCollectionPerson>();
            if (models == null)
            {
                return result;
            }
            var casePeople = GetCasePerson(modelCase.Id);

            foreach (var fastProcessDistribution in models)
            {
                var item = new CaseMoneyCollectionPerson()
                {
                    CourtId = modelCase.CourtId,
                    CaseId = modelCase.Id,
                    CasePersonId = (casePeople.Where(x => x.Person_SourceType == SourceTypeSelectVM.WebApiPerson && x.Person_SourceCode == fastProcessDistribution.DocumentPersonId).FirstOrDefault()).Id,
                    AmountNumerator = fastProcessDistribution.AmountNumerator,
                    AmountDenominator = fastProcessDistribution.AmountDenominator,
                    PersonAmount = fastProcessDistribution.PersonAmount ?? 0
                };

                result.Add(item);
            }

            return result;
        }

        private List<CaseMoneyCollection> FillMoneyCollectionWithMain(FastProcessClaimMoneyModel[] models, Case modelCase)
        {
            var result = new List<CaseMoneyCollection>();
            if (models == null)
            {
                return result;
            }

            foreach (var claimMoneyModel in models)
            {
                var item = new CaseMoneyCollection()
                {
                    CourtId = modelCase.CourtId,
                    CaseId = modelCase.Id,
                    ApiModelId = claimMoneyModel.MainClaimMoneyId,
                    MainCaseMoneyCollectionId = null,
                    CaseMoneyClaimId = 0,
                    CaseMoneyCollectionGroupId = getNomIdByCode<CaseMoneyCollectionGroup>(claimMoneyModel.CollectionGroup),
                    CaseMoneyCollectionTypeId = !string.IsNullOrEmpty(claimMoneyModel.CollectionType) ? getNomIdByCode<CaseMoneyCollectionType>(claimMoneyModel.CollectionType) : (int?)null,
                    CaseMoneyCollectionKindId = !string.IsNullOrEmpty(claimMoneyModel.CollectionKind) ? getNomIdByCode<CaseMoneyCollectionKind>(claimMoneyModel.CollectionKind) : (int?)null,
                    CurrencyId = getNomIdByCode<Currency>(claimMoneyModel.Currency),
                    InitialAmount = claimMoneyModel.InitialAmount,
                    PretendedAmount = claimMoneyModel.PretendedAmount,
                    DateFrom = claimMoneyModel.DateFrom,
                    MoneyCollectionEndDateTypeId = getNomIdByCode<MoneyCollectionEndDateType>(claimMoneyModel.DateToType),
                    DateTo = claimMoneyModel.DateTo,
                    Description = claimMoneyModel.Description,
                    Motive = claimMoneyModel.Motive,
                    Label = claimMoneyModel.Label,
                    JointDistribution = claimMoneyModel.IsJointDistribution,
                    IsFraction = claimMoneyModel.IsFraction,
                    CaseMoneyCollectionPersons = FillMoneyCollectionPerson(claimMoneyModel.Distribution, modelCase)
                };

                result.Add(item);
            }

            return result;
        }

        private List<CaseMoneyCollection> FillMoneyCollection(FastProcessClaimMoneyModel[] models, Case modelCase)
        {
            var result = new List<CaseMoneyCollection>();
            if (models == null)
            {
                return result;
            }

            foreach (var claimMoneyModel in models.Where(x => string.IsNullOrEmpty(x.MainClaimMoneyId)))
            {
                var item = new CaseMoneyCollection()
                {
                    CourtId = modelCase.CourtId,
                    CaseId = modelCase.Id,
                    ApiModelId = claimMoneyModel.ClaimMoneyId,
                    //MainCaseMoneyCollectionId = null,
                    //CaseMoneyClaimId = 0,
                    CaseMoneyCollectionGroupId = getNomIdByCode<CaseMoneyCollectionGroup>(claimMoneyModel.CollectionGroup),
                    CaseMoneyCollectionTypeId = !string.IsNullOrEmpty(claimMoneyModel.CollectionType) ? getNomIdByCode<CaseMoneyCollectionType>(claimMoneyModel.CollectionType) : (int?)null,
                    CaseMoneyCollectionKindId = !string.IsNullOrEmpty(claimMoneyModel.CollectionKind) ? getNomIdByCode<CaseMoneyCollectionKind>(claimMoneyModel.CollectionKind) : (int?)null,
                    CurrencyId = getNomIdByCode<Currency>(claimMoneyModel.Currency),
                    InitialAmount = claimMoneyModel.InitialAmount,
                    PretendedAmount = claimMoneyModel.PretendedAmount,
                    DateFrom = claimMoneyModel.DateFrom,
                    MoneyCollectionEndDateTypeId = getNomIdByCode<MoneyCollectionEndDateType>(claimMoneyModel.DateToType),
                    DateTo = claimMoneyModel.DateTo,
                    Description = claimMoneyModel.Description,
                    Motive = claimMoneyModel.Motive,
                    Label = claimMoneyModel.Label,
                    JointDistribution = claimMoneyModel.IsJointDistribution,
                    IsFraction = claimMoneyModel.IsFraction,
                    CaseMoneyCollectionPersons = FillMoneyCollectionPerson(claimMoneyModel.Distribution, modelCase),
                    // CaseMoneyCollections = FillMoneyCollectionWithMain(models.Where(x => x.MainClaimMoneyId == claimMoneyModel.ClaimMoneyId)?.ToArray(), modelCase)
                };

                result.Add(item);
            }

            return result;
        }

        private List<CaseMoneyClaim> FillMoneyClaim(FastProcessClaimModel[] models, Case modelCase)
        {
            var result = new List<CaseMoneyClaim>();
            if (models == null)
            {
                return result;
            }

            foreach (var claimModel in models)
            {
                var item = new CaseMoneyClaim()
                {
                    CourtId = modelCase.CourtId,
                    CaseId = modelCase.Id,
                    CaseMoneyClaimGroupId = getNomIdByCode<CaseMoneyClaimGroup>(claimModel.ClaimGroup),
                    CaseMoneyClaimTypeId = getNomIdByCode<CaseMoneyClaimType>(claimModel.ClaimType),
                    ClaimNumber = claimModel.ClaimNumber,
                    ClaimDate = claimModel.ClaimDate,
                    PartyNames = claimModel.PartyNames,
                    Description = claimModel.Description,
                    Motive = claimModel.Motive,
                    CaseMoneyCollections = FillMoneyCollection(claimModel.ClaimMoney, modelCase)
                };

                result.Add(item);
            }

            return result;
        }

        private List<CaseMoneyExpensePerson> FillMoneyExpensePerson(FastProcessDistributionModel[] models, Case modelCase)
        {
            var result = new List<CaseMoneyExpensePerson>();
            if (models == null)
            {
                return result;
            }
            var casePeople = GetCasePerson(modelCase.Id);

            foreach (var fastProcessDistribution in models)
            {
                var item = new CaseMoneyExpensePerson()
                {
                    CourtId = modelCase.CourtId,
                    CaseId = modelCase.Id,
                    CasePersonId = (casePeople.Where(x => x.Person_SourceType == SourceTypeSelectVM.WebApiPerson && x.Person_SourceCode == fastProcessDistribution.DocumentPersonId).FirstOrDefault()).Id,
                    PersonAmount = fastProcessDistribution.PersonAmount ?? 0
                };

                result.Add(item);
            }

            return result;
        }

        private List<CaseMoneyExpense> FillMoneyExpense(FastProcessExpenseModel[] models, Case modelCase)
        {
            var result = new List<CaseMoneyExpense>();
            if (models == null)
            {
                return result;
            }
            foreach (var fastProcessExpense in models)
            {
                var item = new CaseMoneyExpense()
                {
                    CourtId = modelCase.CourtId,
                    CaseId = modelCase.Id,
                    CaseMoneyExpenseTypeId = getNomIdByCode<CaseMoneyExpenseType>(fastProcessExpense.ExpenseType),
                    CurrencyId = getNomIdByCode<Currency>(fastProcessExpense.Currency),
                    Amount = fastProcessExpense.Amount,
                    Description = fastProcessExpense.Description,
                    JointDistribution = fastProcessExpense.IsJointDistribution,
                    CaseMoneyExpensePeople = FillMoneyExpensePerson(fastProcessExpense.Distribution, modelCase)
                };

                result.Add(item);
            }

            return result;
        }



        public void TestApi()
        {
            var model = generateTestRequest();
            var request = JsonConvert.SerializeObject(model);
            FileHelper.SaveToFile("documentRequestFastProcess.json", request);
        }

        private DocumentRequestFastProcess generateTestRequest()
        {
            var model = new DocumentRequestFastProcess();
            var docModel = new DocumentModel()
            {
                CourtCode = "142",//рс враца
                DocumentType = "8028",//Искане
                CaseType = "2011",//ЧГД
                CaseCode = "1101-1",
                Description = "Подадено през портал"
            };

            var _p1 = new DocumentPersonModel()
            {
                DocumentPersonId = "apiTestPerson1",
                Uic = "111111111",
                UicType = "3",//ЕИК
                EntityName = "Топлофикация София АД",
                PersonRole = "9001"//вносител
            };

            var pa1 = new DocumentPersonAddressModel()
            {
                AddressType = "637",//Седалище и адрес на управление
                CountryCode = NomenclatureConstants.CountryBG,
                CityCode = "68134",//софия

                StreetFullAddress = "Иван Вазов 2"
            };

            _p1.Addresses = new List<DocumentPersonAddressModel>() { pa1 }.ToArray();

            var _p2 = new DocumentPersonModel()
            {
                DocumentPersonId = "apiTestPerson2",
                Uic = "2222222220",
                UicType = "1",//ЕГН
                FirstName = "Иван",
                MiddleName = "Иванов",
                FamilyName = "Иванов",
                PersonRole = "9047"//длъжник
            };

            var _p3 = new DocumentPersonModel()
            {
                DocumentPersonId = "apiTestPerson3",
                Uic = "3333333330",
                UicType = "1",//ЕГН
                FirstName = "Борис",
                MiddleName = "Борисов",
                FamilyName = "Борисов",
                PersonRole = "9047"//длъжник
            };

            docModel.DocumentPersons = new List<DocumentPersonModel>()
            {
                _p1,
                _p2,
                _p3
            }.ToArray();

            model.Document = docModel;
            model.Data = generateModelFastProcess();
            return model;
        }

        private FastProcessModel generateModelFastProcess()
        {
            FastProcessModel model = new FastProcessModel()
            {
                Currency = "BGN",
                TaxAmount = 34.12M,
                Description = "Длъжниците отказват да заплатят сметки за месец ноември, 2020г."
            };
            model.BankAccounts = new List<FastProcessBankAccountModel>()
            {
                new FastProcessBankAccountModel()
                {
                    BankAccountType = "1",//Банкова сметка
                    BankName = "Банка ДСК",
                    IBAN = "BG32DSK0001000001",
                    BIC = "BG32DSK0"
                }
            }.ToArray();

            var _c1 = new FastProcessClaimModel()
            {
                ClaimGroup = "1",//договор
                ClaimType = "1",//Договор за кредит
                ClaimDate = new DateTime(2020, 12, 31),
                ClaimNumber = "32",
                Description = "Договор за потребителски кредит"
            };

            var _cm1 = new FastProcessClaimMoneyModel()
            {
                ClaimMoneyId = "claimMoney1",
                CollectionGroup = "1",//парично вземане
                CollectionType = "1",//Главница
                DateFrom = new DateTime(2020, 12, 31),
                DateToType = "2",//Без избрана крайна дата
                InitialAmount = 960.20M,
                PretendedAmount = 950M,
                Description = "Главница по договор",
                IsJointDistribution = false,
                IsFraction = false,
                Currency = "BGN",
                Label = "Главница по договор",
                Distribution = new List<FastProcessDistributionModel>()
                {
                    new FastProcessDistributionModel()
                    {
                        DocumentPersonId = "apiTestPerson2",//Иван Иванов Иванов
                        PersonAmount = 500M
                    },
                    new FastProcessDistributionModel()
                    {
                        DocumentPersonId = "apiTestPerson3",//Борис Борисов Борисов
                        PersonAmount = 450M
                    }

                }.ToArray()
            };
            var _cm2 = new FastProcessClaimMoneyModel()
            {
                ClaimMoneyId = "claimMoneySub1",
                MainClaimMoneyId = "claimMoney1",
                CollectionGroup = "1",//парично вземане
                CollectionType = "2",//Друг вид вземане
                CollectionKind = "1",//Лихва
                DateFrom = new DateTime(2020, 12, 31),
                DateToType = "2",//Без избрана крайна дата
                InitialAmount = 120.20M,
                PretendedAmount = 100M,
                Description = "лихва по договор",
                IsJointDistribution = false,
                IsFraction = false,
                Currency = "BGN",
                Label = "лихва по договор",
                Distribution = new List<FastProcessDistributionModel>()
                {
                    new FastProcessDistributionModel()
                    {
                        DocumentPersonId = "apiTestPerson2",//Иван Иванов Иванов
                        PersonAmount = 55M
                    },
                    new FastProcessDistributionModel()
                    {
                        DocumentPersonId = "apiTestPerson3",//Борис Борисов Борисов
                        PersonAmount = 45M
                    }

                }.ToArray()
            };
            _c1.ClaimMoney = new List<FastProcessClaimMoneyModel>()
            {
                _cm1,_cm2
            }.ToArray();

            model.MoneyClaims = new List<FastProcessClaimModel>() { _c1 }.ToArray();

            var exp1 = new FastProcessExpenseModel()
            {
                ExpenseType = "1",//Държавна такса
                Currency = "BGN",
                Amount = 24M,
                Description = "дължима държавна такса",
                IsJointDistribution = true
            };

            var exp2 = new FastProcessExpenseModel()
            {
                ExpenseType = "3",//Юрисконсултско възнаграждение
                Currency = "BGN",
                Amount = 350M,
                Description = "възнаграждение",
                Distribution = new List<FastProcessDistributionModel>()
                {
                    new FastProcessDistributionModel()
                    {
                        DocumentPersonId = "apiTestPerson2",//Иван Иванов Иванов
                        PersonAmount = 200M
                    },
                    new FastProcessDistributionModel()
                    {
                        DocumentPersonId = "apiTestPerson3",//Борис Борисов Борисов
                        PersonAmount = 150M
                    }

                }.ToArray()
            };

            model.Exprenses = new List<FastProcessExpenseModel>() { exp1, exp2 }.ToArray();
            return model;
        }

        public DocumentVM TestDocument()
        {
            var request = generateTestRequest();
            var document = mapDocumentToEntity(request.Document);

            return document;
        }

        public (DocumentRequestFastProcess, DocumentVM) GenerateDocumentWithRequest()
        {
            var request = generateTestRequest();
            var document = mapDocumentToEntity(request.Document);
            return (request, document);
        }

        public (DocumentRequestFastProcess, DocumentVM) GenerateDocumentWithRequestFromString(string json)
        {
            try
            {
                var request = JsonConvert.DeserializeObject<DocumentRequestFastProcess>(json);
                var document = mapDocumentToEntity(request.Document);
                return (request, document);
            }
            catch (Exception ex)
            {
                return (null, null);
            }
        }
    }
}
