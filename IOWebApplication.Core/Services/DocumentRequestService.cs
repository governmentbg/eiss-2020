// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels.Money;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Org.BouncyCastle.Tls.Crypto.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using EpepRest = IOWebApplication.Infrastructure.Models.Integrations.EpepRest;

namespace IOWebApplication.Core.Services
{
    public class DocumentRequestService : BaseService, IDocumentRequestService
    {
        private readonly IPriceService priceService;
        private readonly ICdnService cdnService;
        private DbEuroConfigVM dbEuroConfig;
        public DocumentRequestService(
            ILogger<DocumentRequestService> _logger,
            IPriceService _priceService,
            IRepository _repo,
            IUserContext _userContext,
            ICdnService _cdnService)
        {
            logger = _logger;
            userContext = _userContext;
            priceService = _priceService;
            repo = _repo;
            cdnService = _cdnService;
        }

        public void InitDbEuro(DbEuroConfigVM euroConfig)
        {
            dbEuroConfig = euroConfig;
        }

        private Task<string> getRequestCodeByCaseCodeId(int documentType, int caseCodeId)
        {
            return repo.AllReadonly<DocumentRequestType>()
                        .Where(x => x.DocumentTypeId == documentType && x.CaseCodeId == caseCodeId)
                        .Select(x => x.RequestCode)
                        .FirstOrDefaultAsync();

        }

        public async Task<List<SelectListItem>> GetDDL_AllDocumentRequests(int caseId)
        {
            var initDocument = await repo.AllReadonly<Case>()
                                        .Where(x => x.Id == caseId)
                                        .Select(x => new
                                        {
                                            DocumentId = x.Document.AssignmentDocumentId ?? 0,
                                            DocumentNumber = x.Document.DocumentNumber,
                                            DocumentDate = x.Document.DocumentDate,
                                            DocumentType = x.Document.DocumentType.Label,
                                            RequestCode = x.Document.DocumentRequestType.RequestCode
                                        })
                                        .FirstOrDefaultAsync();

            var documentList = new List<SelectListItem>();
            if (initDocument != null && initDocument.DocumentId > 0)
            {
                documentList.Add(new SelectListItem($"{initDocument.DocumentType} {initDocument.DocumentNumber}/{initDocument.DocumentDate:dd.MM.yyyy} - Иницииращ", initDocument.DocumentId.ToString()));
            }
            documentList.AddRange(await repo.AllReadonly<DocumentCaseInfo>()
                                            .Where(x => x.CaseId == caseId)
                                            .Where(x => x.Document.DocumentRequestType.InitRequestCode == initDocument.RequestCode)
                                            .OrderBy(x => x.DocumentId)
                                            .Select(x => new SelectListItem
                                            {
                                                Value = x.DocumentId.ToString(),
                                                Text = $"{x.Document.DocumentType.Label} {x.Document.DocumentNumber}/{x.Document.DocumentDate:dd.MM.yyyy} - Съпровождащ"
                                            }).ToListAsync());
            return documentList;
        }

        public async Task<SaveResultVM> SelectDocumentDataToCase(int caseId, long documentId)
        {
            string jsonData = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect()
            {
                SourceId = documentId.ToString(),
                SourceType = SourceTypeSelectVM.DocumentRequest
            });
            if (string.IsNullOrEmpty(jsonData))
            {
                return new SaveResultVM(false);
            }
            var uplRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.DocumentCaseRequest,
                SourceId = caseId.ToString(),
                FileName = $"caseRequest{caseId}.json",
                FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonData))
            };
            var result = new SaveResultVM(true, "Зареждането на данни от документа премина успешно");

            result.Result = await cdnService.MongoCdn_AppendUpdate(uplRequest);
            return result;
        }

        public async Task<IBaseRequestVM> GetDocumentRequestById(long documentId, int caseId, bool savedOnly = false)
        {
            long randomCourtDocumentId = documentId;
            Expression<Func<Case, bool>> filterWhere = x => x.DocumentId == documentId;
            if (caseId > 0)
            {
                filterWhere = x => x.Id == caseId;
                randomCourtDocumentId = await repo.AllReadonly<Case>()
                                        .Where(x => x.Id == caseId)
                                        .Select(x => x.Document.AssignmentDocumentId ?? documentId)
                                        .FirstOrDefaultAsync();
            }
            var documentRequestInfo = await repo.AllReadonly<Document>()
                                                .Where(x => x.Id == randomCourtDocumentId)
                                                .Where(x => x.DocumentRequestTypeId > 0)
                                                .Select(x => new
                                                {
                                                    documentRequestId = x.DocumentRequestType.Id,
                                                    documentRequestCode = x.DocumentRequestType.RequestCode
                                                }).FirstOrDefaultAsync();


            string requestData = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect()
            {
                SourceId = randomCourtDocumentId.ToString(),
                SourceType = SourceTypeSelectVM.DocumentRequest
            });

            if (caseId > 0)
            {
                string caseHtml = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect()
                {
                    SourceId = caseId.ToString(),
                    SourceType = SourceTypeSelectVM.DocumentCaseRequest
                });

                if (!string.IsNullOrEmpty(caseHtml))
                {
                    requestData = caseHtml;
                }
            }
            if (savedOnly && string.IsNullOrEmpty(requestData))
            {
                return null;
            }
            var result = await getOrInitDocumentRequest(documentRequestInfo.documentRequestId, documentRequestInfo.documentRequestCode, requestData, randomCourtDocumentId, caseId);

            return result;
        }

        async Task<IBaseRequestVM> getOrInitDocumentRequest(int requestTypeId, string requestTypeCode, string jsonData, long documentId, int caseId)
        {

            IBaseRequestVM result = null;

            switch (requestTypeCode)
            {
                case FastProcessRequestVM.FastProcess410:
                    result = initBaseRequest<FastProcessRequestVM>(jsonData, requestTypeCode);
                    if (result.RequestTypeCode != requestTypeCode)
                    {
                        //При смяна на бланката всички данни се изтриват
                        result = initBaseRequest<FastProcessRequestVM>(string.Empty, requestTypeCode);
                    }
                    result.RequestTitle = "Заявление по чл.410";
                    result.RequestFullTitle = "Заявление за издаване на заповед за изпълнение по чл.410 ГПК";
                    break;
                case FastProcessRequestVM.FastProcess417:

                    result = initBaseRequest<FastProcessRequestVM>(jsonData, requestTypeCode);
                    if (string.IsNullOrEmpty(jsonData) && requestTypeId == DocumentConstants.ElectronicDocumentRequestTypes.IDs.FastProcess417a1t3610)
                    {
                        ((FastProcessRequestVM)result).CompetencyBase417.ForCompetencyBase = true;
                        ((FastProcessRequestVM)result).CompetencyBase417.CompetencyBaseCode = FastProcessConstants.CompetencyBases417.CurrentAddressApplicant;
                    }
                    if (result.RequestTypeCode != requestTypeCode)
                    {
                        //При смяна на бланката всички данни се изтриват
                        result = initBaseRequest<FastProcessRequestVM>(string.Empty, requestTypeCode);
                    }
                    result.RequestTitle = "Заявление по чл.417";
                    result.RequestFullTitle = "Заявление за издаване на заповед за изпълнение по чл.417 ГПК";

                    break;
                default:
                    break;
            }


            result.DocumentId = documentId;
            result.CaseId = caseId;
            await InitRequestSides(result);
            if (documentId > 0)
            {
                result.TaxAmount = await repo.AllReadonly<DocumentRequestInfo>()
                                                .Where(x => x.DocumentId == documentId)
                                                .Select(x => x.TaxAmount ?? 0M)
                                                .FirstOrDefaultAsync();
            }
            if (caseId > 0)
            {
                result.TaxAmount = await repo.AllReadonly<DocumentRequestInfo>()
                                                .Where(x => x.CaseId == caseId)
                                                .Select(x => x.TaxAmount ?? 0M)
                                                .FirstOrDefaultAsync();
            }

            return result;
        }

        T initBaseRequest<T>(string jsonData, string requestTypeCode) where T : class, IBaseRequestVM, new()
        {
            T result = null;
            if (!string.IsNullOrEmpty(jsonData))
            {
                try
                {
                    result = JsonConvert.DeserializeObject<T>(jsonData);
                }
                catch (Exception) { }
            }
            result ??= new T();

            result.RequestTypeCode = requestTypeCode;
            result.RecreateObject();

            if (typeof(T) == typeof(FastProcessRequestVM))
            {
                bool isInEuro = true;

                if (dbEuroConfig != null)
                {
                    isInEuro = dbEuroConfig.IsInEuro;
                }
                else
                {
                    isInEuro = userContext.IsPeriodEuro;
                }

                concertRequestToEuro(result as FastProcessRequestVM, isInEuro);
            }

            return result;
        }


        void concertRequestToEuro(FastProcessRequestVM request, bool isInEuro)
        {
            if (!isInEuro)
            {
                //Преди 01.01.2026
                return;
            }

            decimal euroRate = 1.95583M;

            if (dbEuroConfig != null)
            {
                euroRate = dbEuroConfig.EuroExchangeRate;
            }
            else
            {
                euroRate = userContext.EuroExchangeRate;
            }

            if (request.MoneyClaims != null)
            {
                foreach (var item in request.MoneyClaims)
                {
                    if (item.CurrencyCode == NomenclatureConstants.CurrencyCode.BGN)
                    {
                        item.Amount = Utils.GetAmountEUR(item.Amount, true, euroRate);
                        item.CurrencyCode = NomenclatureConstants.CurrencyCode.EUR;
                    }
                    if (item.TotalAmountBGN > 0M && item.TotalAmountEUR == 0M)
                    {
                        if (item.CurrencyCode == NomenclatureConstants.CurrencyCode.EUR)
                        {
                            item.TotalAmountEUR = item.Amount;
                        }
                        else
                        {
                            item.TotalAmountEUR = Utils.GetAmountEUR(item.TotalAmountBGN, true, euroRate);
                        }
                    }
                }
            }

            if (request.ItemClaim != null)
            {
                if (request.ItemClaim.TotalAmountBGN > 0M && request.ItemClaim.TotalAmountEUR == 0M)
                {
                    request.ItemClaim.TotalAmountEUR = Utils.GetAmountEUR(request.ItemClaim.TotalAmountBGN, true, euroRate);
                }
            }

            if (request.ItemSubstitutionClaims != null)
            {
                foreach (var item in request.ItemSubstitutionClaims)
                {
                    if (item.TotalAmountBGN > 0M && item.TotalAmountEUR == 0M)
                    {
                        item.TotalAmountEUR = Utils.GetAmountEUR(item.TotalAmountBGN, true, euroRate);
                    }
                }
            }

            if (request.DebtDistributions != null)
            {
                foreach (var item in request.DebtDistributions)
                {
                    if (item.CurrencyCode == NomenclatureConstants.CurrencyCode.BGN)
                    {
                        item.Amount = Utils.GetAmountEUR(item.Amount, true, euroRate);
                        item.CurrencyCode = NomenclatureConstants.CurrencyCode.EUR;
                    }
                    if (item.TotalAmountBGN > 0M && item.TotalAmountEUR == 0M)
                    {
                        if (item.CurrencyCode == NomenclatureConstants.CurrencyCode.EUR)
                        {
                            item.TotalAmountEUR = item.Amount;
                        }
                        else
                        {
                            item.TotalAmountEUR = Utils.GetAmountEUR(item.TotalAmountBGN, true, euroRate);
                        }
                    }
                }
            }

            if (request.Expenses != null)
            {
                foreach (var item in request.Expenses)
                {
                    if (item.TotalAmountBGN > 0M && item.TotalAmountEUR == 0M)
                    {
                        item.TotalAmountEUR = Utils.GetAmountEUR(item.TotalAmountBGN, true, euroRate);
                    }
                }
            }

            if (request.BankAccount != null)
            {
                if (request.BankAccount.CurrencyCode == NomenclatureConstants.CurrencyCode.BGN)
                {
                    request.BankAccount.CurrencyCode = NomenclatureConstants.CurrencyCode.EUR;
                }
            }
        }



        public async Task InitRequestSides(IBaseRequestVM request)
        {
            List<BaseRequestPersonReadInfoVM> personList = null;
            if (request.CaseId > 0)
            {
                personList = await initRequestFromCase(request);
            }
            else
            {
                personList = await initRequestFromDocument(request);
            }

            foreach (var person in personList)
            {
                person.PersonGid = (person.PersonGid ?? "").ToLower();
                person.RepresentsPersonGid = (person.RepresentsPersonGid ?? "-1").ToLower();
            }

            if (request.DocumentId > 0)
            {
                request.HasRepresentatives = personList.Any(x => x.RoleKind == NomenclatureConstants.RoleKind.Representative);
            }

            request.LeftSide = personList.Where(x => x.RoleKind == NomenclatureConstants.PersonKinds.LeftSide)
                                        .Select(leftSide => new BaseRequestPersonInfoVM
                                        {
                                            CasePersonId = leftSide.CasePersonId,
                                            Identifier = leftSide.Identifier,
                                            FullName = leftSide.FullName,
                                            PersonGid = leftSide.PersonGid,
                                            RoleName = leftSide.RoleName,
                                            Representatives = personList.Where(representative => representative.RepresentsPersonGid == leftSide.PersonGid)
                                                            .Select(representative => new BaseRequestPersonInfoVM
                                                            {
                                                                Identifier = representative.Identifier,
                                                                FullName = representative.FullName,
                                                                PersonGid = representative.PersonGid,
                                                                RoleName = representative.RoleName,
                                                                Addresses = representative.Addresses
                                                            }).ToArray(),
                                            Addresses = leftSide.Addresses
                                        }).ToArray();

            request.RightSide = personList.Where(x => x.RoleKind == NomenclatureConstants.PersonKinds.RightSide)
                                      .Select(rightSide => new BaseRequestPersonInfoVM
                                      {
                                          CasePersonId = rightSide.CasePersonId,
                                          Identifier = rightSide.Identifier,
                                          FullName = rightSide.FullName,
                                          PersonGid = rightSide.PersonGid,
                                          RoleName = rightSide.RoleName,
                                          Representatives = personList.Where(representative => representative.RepresentsPersonGid == rightSide.PersonGid)
                                                          .Select(representative => new BaseRequestPersonInfoVM
                                                          {
                                                              Identifier = representative.Identifier,
                                                              FullName = representative.FullName,
                                                              PersonGid = representative.PersonGid,
                                                              RoleName = representative.RoleName,
                                                              Addresses = representative.Addresses
                                                          }).ToArray(),
                                          Addresses = rightSide.Addresses
                                      }).ToArray();
        }


        public Task<List<BaseRequestPersonReadInfoVM>> initRequestFromDocument(IBaseRequestVM request)
        {

            return repo.AllReadonly<DocumentPerson>()
                                              .Where(x => x.DocumentId == request.DocumentId)
                                              .Select(x => new BaseRequestPersonReadInfoVM
                                              {
                                                  PersonGid = x.PersonGid,
                                                  FullName = x.FullName,
                                                  Identifier = x.Uic,
                                                  RoleName = x.PersonRole.Label,
                                                  RoleKind = x.PersonRole.RoleKindId,
                                                  RepresentsPersonGid = x.RepresentsGid ?? "-1",
                                                  Addresses = x.Addresses.Select(a => new BaseRequestPersonAddressInfoVM
                                                  {
                                                      FullAddress = a.Address.FullAddress,
                                                      AddressTypeName = a.Address.AddressType.Label
                                                  }).ToArray()
                                              }).ToListAsync();

        }

        public async Task<List<BaseRequestPersonReadInfoVM>> initRequestFromCase(IBaseRequestVM request)
        {

            var documentPersonList = await repo.AllReadonly<CasePerson>()
                                              .Where(x => x.CaseId == request.CaseId)
                                              .Where(x => x.DateExpired == null && x.CaseSessionId == null)
                                              .Select(x => new BaseRequestPersonReadInfoVM
                                              {
                                                  CasePersonId = x.Id,
                                                  PersonGid = x.PersonGid,
                                                  FullName = x.FullName,
                                                  Identifier = x.Uic,
                                                  RoleName = x.PersonRole.Label,
                                                  RoleKind = x.PersonRole.RoleKindId,
                                                  RepresentsPersonGid = x.RepresentsGid ?? "-1",
                                                  Addresses = x.Addresses.Select(a => new BaseRequestPersonAddressInfoVM
                                                  {
                                                      FullAddress = a.Address.FullAddress,
                                                      AddressTypeName = a.Address.AddressType.Label
                                                  }).ToArray()
                                              }).ToListAsync();

            if (documentPersonList.Any(p => p.RoleKind == NomenclatureConstants.RoleKind.Representative))
            {
                foreach (var rep in documentPersonList.Where(p => p.RoleKind == NomenclatureConstants.RoleKind.Representative
                && p.RepresentsPersonGid == "-1"))
                {
                    var linkNormal = await repo.AllReadonly<CasePersonLink>()
                                            .Where(x => x.LinkDirectionId == NomenclatureConstants.LinkDirectionType.Represent)
                                            .Where(x => x.CasePersonRelId == rep.CasePersonId)
                                            .Where(x => x.DateExpired == null)
                                            .Select(x => x.CasePersonId)
                                            .ToListAsync();

                    var inverseLinks = await repo.AllReadonly<CasePersonLink>()
                                            .Where(x => x.LinkDirectionId == NomenclatureConstants.LinkDirectionType.RepresentBy)
                                            .Where(x => x.CasePersonId == rep.CasePersonId)
                                            .Where(x => x.DateExpired == null)
                                            .Select(x => x.CasePersonRelId)
                                            .ToListAsync();

                    linkNormal = linkNormal.Union(inverseLinks).ToList();
                    if (linkNormal.Count > 0)
                    {
                        int masterPesronId = linkNormal.First();
                        rep.RepresentsPersonGid = documentPersonList.Where(x => x.CasePersonId == masterPesronId).Select(x => x.PersonGid).FirstOrDefault();
                    }
                }



            }
            return documentPersonList;
        }

        public Task<List<SelectListItem>> GetDDL_DocumentSideList(long documentId, int caseId, int? sideType = null)
        {
            if (caseId > 0)
            {
                Expression<Func<CasePerson, bool>> filterSideType = x => true;
                if (sideType > 0)
                {
                    filterSideType = x => x.PersonRole.RoleKindId == sideType.Value;
                }
                return repo.AllReadonly<CasePerson>()
                                               .Where(x => x.CaseId == caseId)
                                               .Where(x => x.CaseSessionId == null)
                                               .Where(x => x.DateExpired == null)
                                               .Where(filterSideType)
                                               .Select(x => new SelectListItem
                                               {
                                                   Text = x.FullName,
                                                   Value = x.PersonGid
                                               }).ToListAsync();
            }
            else
            {
                Expression<Func<DocumentPerson, bool>> filterSideType = x => true;
                if (sideType > 0)
                {
                    filterSideType = x => x.PersonRole.RoleKindId == sideType.Value;
                }
                return repo.AllReadonly<DocumentPerson>()
                                               .Where(x => x.DocumentId == documentId)
                                               .Where(filterSideType)
                                               .Select(x => new SelectListItem
                                               {
                                                   Text = x.FullName,
                                                   Value = x.PersonGid
                                               }).ToListAsync();
            }
        }
        public async Task<List<SelectListItem>> GetDDL_ClaimCircumstancesCode(string groupCode, bool addAllItem = false, string allItem = "Изберете")
        {
            var result = await repo.AllReadonly<FastProcessClaimCircumstanceGroup>()
                        .Where(x => x.GroupCode == groupCode)
                        .Select(x => new SelectListItem
                        {
                            Value = x.FastProcessClaimCircumstance.Code,
                            Text = x.FastProcessClaimCircumstance.Label
                        }).ToListAsync();
            if (addAllItem)
            {
                result.Insert(0, new SelectListItem(allItem, NomenclatureConstants.NullVal.ToString()));
            }
            return result;
        }

        public async Task<List<ValidationErrorModel>> ValidateFastProcessRequestData(FastProcessRequestVM model)
        {
            var result = new List<ValidationErrorModel>();
            if (model.CompetencyBase417 != null && model.RequestTypeCode == FastProcessRequestVM.FastProcess417)
            {
                switch (model.CompetencyBase417.CompetencyBaseCode)
                {
                    case FastProcessConstants.CompetencyBases417.CurrentAddressApplicant:
                        if (!await checkHasSideAddress(model.DocumentId, NomenclatureConstants.AddressType.CurrentManageAddresses, NomenclatureConstants.PersonKinds.LeftSide))
                        {
                            result.Add(new ValidationErrorModel("ВАЖНО!: Моля въведете Настоящ адрес/седалище на заявителя!", "SideValidations"));
                        }
                        break;
                    case FastProcessConstants.CompetencyBases417.ResidentAddressApplicant:
                        if (!await checkHasSideAddress(model.DocumentId, NomenclatureConstants.AddressType.ResidentAddresses, NomenclatureConstants.PersonKinds.LeftSide))
                        {
                            result.Add(new ValidationErrorModel("ВАЖНО!: Моля въведете Постоянен адрес на заявителя!", "SideValidations"));
                        }
                        break;
                    case FastProcessConstants.CompetencyBases417.CurrentAddressDebtor:
                        if (!await checkHasSideAddress(model.DocumentId, NomenclatureConstants.AddressType.CurrentManageAddresses, NomenclatureConstants.PersonKinds.RightSide))
                        {
                            result.Add(new ValidationErrorModel("ВАЖНО!: Моля въведете Настоящ адрес/седалище на длъжника!", "SideValidations"));
                        }
                        break;
                    case FastProcessConstants.CompetencyBases417.ResidentAddressDebtor:
                        if (!await checkHasSideAddress(model.DocumentId, NomenclatureConstants.AddressType.ResidentAddresses, NomenclatureConstants.PersonKinds.RightSide))
                        {
                            result.Add(new ValidationErrorModel("ВАЖНО!: Моля въведете Постоянен адрес на длъжника!", "SideValidations"));
                        }
                        break;
                    default:
                        break;
                }
            }

            List<BaseRequestPersonReadInfoVM> personList = null;
            if (model.CaseId > 0)
            {
                personList = await initRequestFromCase(model);
            }
            else
            {
                personList = await initRequestFromDocument(model);
            }
            var sideInfo = personList.Select(x => new
            {
                x.RoleKind
            }).ToList();


            var sideKinds = sideInfo.Where(x => NomenclatureConstants.RoleKind.MainSides.Contains(x.RoleKind)).Select(x => x.RoleKind).Distinct().Count();
            if (sideKinds != 2)
            {
                //Няма лява и дясна страна в заявлението
                result.Add(new ValidationErrorModel("Моля въведете поне един заявител и поне един длъжник", "SideValidations"));
            }
            if (sideInfo.Where(x => x.RoleKind == NomenclatureConstants.RoleKind.RightSide).Count() > 1 && (model.JoinedDestributionTypeId ?? 0) == 0)
            {
                result.Add(new ValidationErrorModel("При повече от един длъжник, моля изберете Разпределение на отговорността", "JoinedDestributionTypeId"));
            }
            //При разделна отговорност трябва да има повече от 1 длъжник
            if (model.JoinedDestributionTypeId == 2)
            {
                if (sideInfo.Where(x => x.RoleKind == NomenclatureConstants.RoleKind.RightSide).Count() < 2)
                {
                    result.Add(new ValidationErrorModel("За разделна отговорност трябва да има повече от един длъжник", "JoinedDestributionTypeId"));
                }
            }
            return result;
        }


        private Task<bool> checkHasSideAddress(long documentId, int[] addressTypes, int roleKind)
        {
            return repo.AllReadonly<DocumentPersonAddress>()
                       .Where(x => x.DocumentPerson.DocumentId == documentId)
                       .Where(x => x.DocumentPerson.PersonRole.RoleKindId == roleKind)
                       .Where(x => addressTypes.Contains(x.Address.AddressTypeId))
                       .AnyAsync();
        }

        public async Task<string> GetCityCodeByCompetencyBases417(long documentId, string competencyBaseCode)
        {
            int[] addressTypes = null;
            int roleKind = 0;
            switch (competencyBaseCode)
            {
                case FastProcessConstants.CompetencyBases417.CurrentAddressApplicant:
                    addressTypes = NomenclatureConstants.AddressType.CurrentManageAddresses;
                    roleKind = NomenclatureConstants.PersonKinds.LeftSide;
                    break;
                case FastProcessConstants.CompetencyBases417.ResidentAddressApplicant:
                    addressTypes = NomenclatureConstants.AddressType.ResidentAddresses;
                    roleKind = NomenclatureConstants.PersonKinds.LeftSide;
                    break;
                case FastProcessConstants.CompetencyBases417.CurrentAddressDebtor:
                    addressTypes = NomenclatureConstants.AddressType.CurrentManageAddresses;
                    roleKind = NomenclatureConstants.PersonKinds.RightSide;
                    break;
                case FastProcessConstants.CompetencyBases417.ResidentAddressDebtor:
                    addressTypes = NomenclatureConstants.AddressType.ResidentAddresses;
                    roleKind = NomenclatureConstants.PersonKinds.RightSide;
                    break;
                default:
                    break;
            }
            if (roleKind == 0)
            {
                return string.Empty;
            }

            return await repo.AllReadonly<DocumentPersonAddress>()
                             .Where(x => x.DocumentPerson.DocumentId == documentId)
                             .Where(x => x.DocumentPerson.PersonRole.RoleKindId == roleKind)
                             .Where(x => addressTypes.Contains(x.Address.AddressTypeId))
                             .OrderBy(x => x.Id)
                             .Select(x => x.Address.CityCode)
                             .FirstOrDefaultAsync();
        }

        public async Task<SaveResultVM> SaveDocumentRequest(IBaseRequestVM request, bool recalcExpense = false)
        {
            request.LeftSide = null;
            request.RightSide = null;

            SaveResultVM result = new(false);
            if (recalcExpense)
            {
                await recalcExpenseInCase(request);
            }
            try
            {
                if (FastProcessRequestVM.InitRequests.Contains(request.RequestTypeCode))
                {
                    var calcResult = await calcTax_FastProcess((FastProcessRequestVM)request);
                    if (!calcResult.Result)
                    {
                        return new SaveResultVM(false, "Грешка при изчисляване на такса");
                    }
                }


                var jsonData = JsonConvert.SerializeObject(request);
                var uplRequest = new CdnUploadRequest()
                {
                    SourceType = SourceTypeSelectVM.DocumentRequest,
                    SourceId = request.DocumentId.ToString(),
                    FileName = $"documentRequest{request.DocumentId}.json",
                    FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonData))
                };
                if (request.CaseId > 0)
                {
                    uplRequest.SourceType = SourceTypeSelectVM.DocumentCaseRequest;
                    uplRequest.SourceId = request.CaseId.ToString();
                    uplRequest.FileName = $"caseRequest{request.CaseId}.json";
                }

                result.Result = await cdnService.MongoCdn_AppendUpdate(uplRequest);

                return result;

                /*
                DocumentTaxCalcVM taxCalcModel = null;
                switch (request.RequestTypeCode)
                {
                    case DocumentConstants.RequestTypes.FastProcess410:
                    case DocumentConstants.RequestTypes.FastProcess417:
                        taxCalcModel = await calcTax_FastProcess((FastProcessRequestVM)request);
                        break;
                }


                if (taxCalcModel != null)
                {
                    var electronicDocument = await GetByGidAsync<ElectronicDocument>(request.DocumentGid);
                    electronicDocument.MoneyPricelistId = taxCalcModel.MoneyPriceListId;
                    electronicDocument.MoneyCurrencyId = taxCalcModel.CurrencyId;
                    if (electronicDocument.MoneyCurrencyId == NomenclatureConstants.Currencies.BGN)
                    {
                        electronicDocument.BaseAmount = taxCalcModel.MaterialInterestBGN;
                        electronicDocument.TaxAmount = taxCalcModel.TaxBGN;
                        electronicDocument.BaseAmountBGN = taxCalcModel.MaterialInterestBGN;
                        electronicDocument.TaxAmountBGN = taxCalcModel.TaxBGN;
                    }
                    else
                    {
                        electronicDocument.BaseAmount = taxCalcModel.MaterialInterestEUR;
                        electronicDocument.TaxAmount = taxCalcModel.TaxEUR;
                        if (taxCalcModel.MaterialInterestEUR > 0M)
                        {
                            electronicDocument.BaseAmountBGN = Math.Round(taxCalcModel.MaterialInterestEUR / euroConfig.BgnToEuro, 2);
                        }
                        if (taxCalcModel.TaxEUR > 0M)
                        {
                            electronicDocument.TaxAmountBGN = Math.Round(taxCalcModel.TaxEUR / euroConfig.BgnToEuro, 2);
                        }
                    }
                }
                */

            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"DocumentId:{request.DocumentId};CaseId:{request.CaseId}");
                return new SaveResultVM(false);
            }
        }

        private async Task recalcExpenseInCase(IBaseRequestVM request)
        {
            if (request.CaseId == 0)
            {
                return;
            }

            long randomCourtDocumentId = await repo.AllReadonly<Case>()
                                                    .Where(x => x.Id == request.CaseId)
                                                    .Select(x => x.Document.AssignmentDocumentId ?? 0)
                                                    .FirstOrDefaultAsync();

            var documentRequest = await GetDocumentRequestById(randomCourtDocumentId, 0);

            decimal documentClaimed = calcClaimAmount((FastProcessRequestVM)documentRequest);

            if (documentClaimed == 0M)
            {
                return;
            }

            decimal caseConfirmed = calcClaimAmount((FastProcessRequestVM)request);

            decimal claimedRatio = caseConfirmed / documentClaimed;

            FastProcessExpenseVM[] recalcedExpences = ((FastProcessRequestVM)documentRequest)
                                                        .Expenses
                                                        .Select(e => new FastProcessExpenseVM
                                                        {
                                                            Gid = e.Gid,
                                                            ExpenseTypeCode = e.ExpenseTypeCode,
                                                            Index = e.Index,
                                                            Description = e.Description,
                                                            TotalAmountBGN = Math.Round(e.TotalAmountBGN * claimedRatio, 2),
                                                            TotalAmountEUR = Math.Round(e.TotalAmountEUR * claimedRatio, 2)
                                                        }).ToArray();

            ((FastProcessRequestVM)request).Expenses = recalcedExpences;
            return;
        }

        private decimal calcClaimAmount(FastProcessRequestVM request)
        {
            decimal result = 0M;

            if (request.ClaimTypes.MoneyClaim)
            {
                result += request.MoneyClaims.Select(c => c.TotalAmountEUR).Sum();
            }
            if (request.ClaimTypes.ItemSubstitutionClaim)
            {
                result += request.ItemSubstitutionClaims.Select(c => c.TotalAmountEUR).Sum();
            }
            if (request.ClaimTypes.ItemClaim || request.ClaimTypes.PropertyClaim)
            {
                result += request.ItemClaim.TotalAmountEUR;
            }
            return result;
        }

        public async Task<List<NomenclatureItemVM>> LoadAliasNomenclatures(string[] aliasList, IBaseRequestVM request)
        {
            var result = new List<NomenclatureItemVM>();
            if (aliasList.Contains(NomenclatureConstants.FPaliases.FP_CompetencyBases))
            {
                result.AddRange(await loadAliasNomenclatureCode<FastProcess417CompetencyBase>(NomenclatureConstants.FPaliases.FP_CompetencyBases));
            }
            if (aliasList.Contains(NomenclatureConstants.FPaliases.FP_MoneyClaimTypes))
            {
                result.AddRange(await loadAliasNomenclatureCode<FastProcessMoneyClaimType>(NomenclatureConstants.FPaliases.FP_MoneyClaimTypes));
            }
            if (aliasList.Contains(NomenclatureConstants.FPaliases.FP_ClaimCircumstances))
            {
                result.AddRange(await loadAliasNomenclatureCode<FastProcessClaimCircumstance>(NomenclatureConstants.FPaliases.FP_ClaimCircumstances));
            }
            if (aliasList.Contains(NomenclatureConstants.FPaliases.FP_ExpenseTypes))
            {
                result.AddRange(await loadAliasNomenclatureCode<FastProcessExpenseType>(NomenclatureConstants.FPaliases.FP_ExpenseTypes));
            }
            if (aliasList.Contains(NomenclatureConstants.FPaliases.FP_DebtList) && request != null)
            {
                var fpRequest = (FastProcessRequestVM)request;
                var debtList = new List<NomenclatureItemVM>();
                debtList.AddRange(fpRequest.MoneyClaims.Select(x => new NomenclatureItemVM
                {
                    Alias = NomenclatureConstants.FPaliases.FP_DebtList,
                    Value = $"1|{x.Gid}",
                    Label = $"Парично вземане {x.Index + 1}"
                }));
                debtList.AddRange(fpRequest.ItemSubstitutionClaims.Select(x => new NomenclatureItemVM
                {
                    Alias = NomenclatureConstants.FPaliases.FP_DebtList,
                    Value = $"2|{x.Gid}",
                    Label = $"Заместима вещ  {x.Index + 1}"
                }));
                debtList.Add(new NomenclatureItemVM
                {
                    Alias = NomenclatureConstants.FPaliases.FP_DebtList,
                    Value = $"3|",
                    Label = $"Движима вещ"
                });
                debtList.AddRange(fpRequest.Expenses.Select(x => new NomenclatureItemVM
                {
                    Alias = NomenclatureConstants.FPaliases.FP_DebtList,
                    Value = $"4|{x.Gid}",
                    Label = $"Разноска  {x.Index + 1}"
                }));
                result.AddRange(debtList);
            }
            //if (aliasList.Contains(NomenclatureConstants.FPaliases.FileTypes))
            //{
            //    result.AddRange(await loadAliasNomenclatureId<AttachmentFileType>(NomenclatureConstants.Aliases.FileTypes));
            //}

            return result;
        }

        Task<List<NomenclatureItemVM>> loadAliasNomenclatureCode<T>(string alias) where T : class, ICommonNomenclature
        {
            return repo.AllReadonly<T>()
                        .Select(x => new NomenclatureItemVM
                        {
                            Alias = alias,
                            Value = x.Code,
                            Label = x.Label
                        }).ToListAsync();
        }

        async Task<SaveResultVM> calcTax_FastProcess(FastProcessRequestVM model)
        {
            decimal materialInterestBGN = 0M;
            decimal materialInterestEUR = 0M;

            if (model.MoneyClaims != null && model.ClaimTypes.MoneyClaim)
            {
                materialInterestBGN += model.MoneyClaims.Select(m => m.TotalAmountBGN).Sum();
                materialInterestEUR += model.MoneyClaims.Select(m => m.TotalAmountEUR).Sum();
            }
            if (model.ItemClaim != null && (model.ClaimTypes.ItemClaim || model.ClaimTypes.PropertyClaim))
            {
                materialInterestBGN += model.ItemClaim.TotalAmountBGN;
                materialInterestEUR += model.ItemClaim.TotalAmountEUR;
            }
            if (model.ItemSubstitutionClaims != null && model.ClaimTypes.ItemSubstitutionClaim)
            {
                materialInterestBGN += model.ItemSubstitutionClaims.Select(m => m.TotalAmountBGN).Sum();
                materialInterestEUR += model.ItemSubstitutionClaims.Select(m => m.TotalAmountEUR).Sum();
            }

            //32084 - 410
            //32085 - 417

            string priceListCode = "";
            string moneyFeeType = "";
            switch (model.RequestTypeCode)
            {
                case DocumentConstants.ElectronicDocumentRequestTypes.FastProcess410:
                    priceListCode = NomenclatureConstants.PriceDescKeyWord.KeyMoneyCase410;
                    moneyFeeType = "32084";
                    break;
                case DocumentConstants.ElectronicDocumentRequestTypes.FastProcess417:
                    priceListCode = NomenclatureConstants.PriceDescKeyWord.KeyMoneyCase417;
                    moneyFeeType = "32085";
                    break;
            }

            decimal baseAmount = materialInterestBGN;
            if (userContext.IsPeriodEuro)
            {
                baseAmount = materialInterestEUR;
            }

            var pcent = priceService.GetPriceValue(null, priceListCode, 0, null, 0, 0, NomenclatureConstants.PriceDescKeyWord.RowMoneyPercent);
            var minvalue = priceService.GetPriceValue(null, priceListCode, 0, null, 0, 0, NomenclatureConstants.PriceDescKeyWord.RowMoneyMinValue);

            var taxAmount = Math.Round((baseAmount * (pcent / 100)), 2);
            taxAmount = (taxAmount > minvalue) ? taxAmount : minvalue;

            DocumentRequestInfo documentRequestInfo = null;
            if (model.DocumentId > 0)
            {
                documentRequestInfo = await repo.All<DocumentRequestInfo>()
                                                .Where(x => x.DocumentId == model.DocumentId)
                                                .FirstOrDefaultAsync() ?? new DocumentRequestInfo() { DocumentId = model.DocumentId };
            }
            if (model.CaseId > 0)
            {
                documentRequestInfo = await repo.All<DocumentRequestInfo>()
                                                .Where(x => x.CaseId == model.CaseId)
                                                .FirstOrDefaultAsync() ?? new DocumentRequestInfo() { CaseId = model.CaseId };
            }
            if (documentRequestInfo == null) { return new SaveResultVM(false); }

            if (documentRequestInfo.Id == 0)
            {
                await repo.AddAsync(documentRequestInfo);
            }
            documentRequestInfo.MoneyFeeTypeId = await GetPropByIdAsync<MoneyFeeType, int>(x => x.Code == moneyFeeType && x.IsActive, x => x.Id);
            documentRequestInfo.BaseAmount = baseAmount;
            documentRequestInfo.BaseAmountBGN = Utils.GetAmountBGN(baseAmount, userContext.IsPeriodEuro, userContext.EuroExchangeRate);
            documentRequestInfo.TaxAmount = taxAmount;
            documentRequestInfo.TaxAmountBGN = Utils.GetAmountBGN(taxAmount, userContext.IsPeriodEuro, userContext.EuroExchangeRate);
            if (model.CompetencyBase417 != null)
            {
                documentRequestInfo.ForCompetencyBase = model.CompetencyBase417.ForCompetencyBase;
                documentRequestInfo.CompetencyBaseCode = model.CompetencyBase417.CompetencyBaseCode;
            }
            try
            {
                await repo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"calcTax_FastProcess; DocumentId={model.DocumentId}; CaseId={model.CaseId}");
                return new SaveResultVM(false);
            }

            return new SaveResultVM(true);
        }

        public async Task<DocumentPersonLinkVM> DocumentPersonLinks_Select(long documentId)
        {
            var docPersons = await repo.AllReadonly<DocumentPerson>()
                                        .Where(x => x.DocumentId == documentId)
                                        .Select(x => new
                                        {
                                            x.Id,
                                            x.PersonGid,
                                            x.RepresentsGid,
                                            PersonName = x.FullName,
                                            RoleName = x.PersonRole.Label,
                                            RoleKind = x.PersonRole.RoleKindId,
                                        }).ToListAsync();

            var result = new DocumentPersonLinkVM()
            {
                DocumentId = documentId,
                Representatives = docPersons.Where(x => x.RoleKind == NomenclatureConstants.RoleKind.Representative)
                                            .Select(x => new DocumentPersonLinkSideVM
                                            {
                                                DocumentPersonId = x.Id,
                                                PersonGid = x.PersonGid,
                                                PersonName = x.PersonName,
                                                RoleKind = x.RoleKind,
                                                RepresentsGid = x.RepresentsGid,
                                                SavedRepresentsGid = x.RepresentsGid
                                            }).ToArray(),
                MainSides = docPersons.Where(x => NomenclatureConstants.RoleKind.MainSides.Contains(x.RoleKind))
                                        .OrderBy(x => x.RoleKind)
                                        .Select(x => new SelectListItem
                                        {
                                            Value = x.PersonGid,
                                            Text = $"{x.PersonName} ({x.RoleName})"
                                        }).ToList().Prepend(new SelectListItem
                                        {
                                            Value = NomenclatureConstants.NullText,
                                            Text = "-Без избор на представлявана страна-"
                                        }).ToList()

            };


            return result;
        }

        public async Task<SaveResultVM> DocumentPersonLinks_SaveData(DocumentPersonLinkVM model)
        {
            try
            {
                foreach (var rep in model.Representatives)
                {
                    if (rep.SavedRepresentsGid != rep.RepresentsGid)
                    {
                        var representative = await repo.GetByIdAsync<DocumentPerson>(rep.DocumentPersonId);
                        representative.RepresentsGid = rep.RepresentsGid.EmptyToNull().EmptyToNull(NomenclatureConstants.NullText);
                        await repo.SaveChangesAsync();
                    }
                }
                return new SaveResultVM(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"DocumentPersonLinks_SaveData; DocumentID = {model.DocumentId}");
                return new SaveResultVM(false);
            }
        }
    }
}
