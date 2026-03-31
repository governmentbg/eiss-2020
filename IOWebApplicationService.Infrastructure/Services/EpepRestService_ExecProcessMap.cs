// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Money;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EpepRest = IOWebApplication.Infrastructure.Models.Integrations.EpepRest;

namespace IOWebApplicationService.Infrastructure.Services
{
    public partial class EpepRestService
    {
        private async Task<SaveResultVM> mapExecProcessFromExecList(MQEpep mq, EpepRest.ExecProcess epepRequest)
        {
            int execListId = (int)mq.ParentSourceId;
            int caseId = await repo.GetPropByIdAsync<ExecList, int>(x => x.Id == execListId, x => x.CaseId ?? 0);
            if (caseId == 0)
            {
                return new SaveResultVM(false, "Invalid CaseId;");
            }

            var execListObligations = await EpepListObligation_Select(execListId);
            List<EpepRest.ExecProcessSideModel> sideList = new List<EpepRest.ExecProcessSideModel>();
            List<EpepRest.ExecProcessObligationModel> obligations = new List<EpepRest.ExecProcessObligationModel>();
            foreach (var eListModel in execListObligations)
            {
                var beneficiary = await mapExecListSide(eListModel.Beneficiary, NomenclatureConstants.PersonRole.Kreditor);

                var savedBeneficiary = sideList.Where(s => s.SideUic == beneficiary.SideUic && s.SideInvolvementKindCode == beneficiary.SideInvolvementKindCode && !string.IsNullOrEmpty(beneficiary.SideUic)).FirstOrDefault();
                if (savedBeneficiary != null)
                {
                    beneficiary = savedBeneficiary;
                }
                else
                {
                    sideList.Add(beneficiary);
                }
                var debtor = await mapExecListSide(eListModel.Debtor, NomenclatureConstants.PersonRole.Debtor);
                var savedDebtor = sideList.Where(s => s.SideUic == debtor.SideUic && s.SideInvolvementKindCode == debtor.SideInvolvementKindCode && !string.IsNullOrEmpty(debtor.SideUic)).FirstOrDefault();
                if (savedDebtor != null)
                {
                    debtor = savedDebtor;
                }
                else
                {
                    sideList.Add(debtor);
                }
                var newObligation = new EpepRest.ExecProcessObligationModel()
                {
                    Gid = Guid.NewGuid(),
                    BeneficiaryGid = beneficiary.Gid,
                    Amount = eListModel.Amount,
                    CurrencyCode = eListModel.CurrencyCode,
                    Description = eListModel.Description,
                    ObligationTypeCode = GetNomValue(EpepConstants.Nomenclatures.ExecListMoneyType, eListModel.MoneyTypeId),
                    Debtors = new[] { debtor.Gid }
                };
                obligations.Add(newObligation);
            }
            epepRequest.Sides = sideList.ToArray();
            epepRequest.Obligations = obligations.ToArray();

            return new SaveResultVM(true);
        }

        /// <summary>
        /// Извличане данни за Изпълнителен лист за изпращане към ЕПЕП
        /// </summary>
        /// <param name="execListId"></param>
        /// <returns></returns>
        async Task<List<EpepExecListObligationM>> EpepListObligation_Select(int execListId)
        {
            var execListObligations = await repo.AllReadonly<ExecListObligation>()
                                        .Where(x => x.ExecListId == execListId)
                                        .OrderBy(x => x.Id)
                                        .Select(x => x.Obligation)
                                        .Select(x => new EpepExecListObligationM
                                        {
                                            ObligationId = x.Id,
                                            Amount = x.Amount,
                                            //Всички суми са по подразбиране евро
                                            CurrencyCode = NomenclatureConstants.CurrencyCode.EUR,
                                            MoneyTypeId = x.MoneyTypeId,
                                            Description = $"{x.ObligationNumber}/{x.ObligationDate:dd.MM.yyyy}; {x.Description}",
                                            Debtor = new EpepExecListSideVM
                                            {
                                                Uic = x.Uic,
                                                UicTypeId = x.UicTypeId,
                                                FullName = x.FullName,
                                                PersonSourceType = x.Person_SourceType,
                                                PersonSourceId = x.Person_SourceId
                                            },
                                            Beneficiary = x.ObligationReceives.Select(r => new EpepExecListSideVM
                                            {
                                                Uic = r.Uic,
                                                UicTypeId = r.UicTypeId,
                                                FullName = r.FullName,
                                                PersonSourceType = (r.CasePersonId > 0) ? SourceTypeSelectVM.CasePerson : r.Person_SourceType,
                                                PersonSourceId = (r.CasePersonId > 0) ? r.CasePersonId : r.Person_SourceId
                                            }).FirstOrDefault()
                                        })
                                        .ToListAsync();

            return execListObligations;
        }

        private async Task<EpepRest.ExecProcessSideModel> mapExecListSide(EpepExecListSideVM model, int defaultPersonRole = 0)
        {
            var result = new EpepRest.ExecProcessSideModel();
            result.Gid = Guid.NewGuid();
            result.SubjectKind = (NomenclatureConstants.UicTypes.PersonTypes.Contains(model.UicTypeId)) ? EpepConstants.SubjectKinds.Person : EpepConstants.SubjectKinds.Entity;
            result.SideUic = model.Uic;
            result.SideName = model.FullName;


            int personRoleId = 0;
            string countryCode = string.Empty;
            string addressFull = string.Empty;
            int? addressTypeId = 0;
            if (model.PersonSourceId > 0)
                switch (model.PersonSourceType)
                {
                    case SourceTypeSelectVM.DocumentPerson:
                        var documentPersonInfo = await repo.AllReadonly<DocumentPerson>()
                                                    .Where(x => x.Id == model.PersonSourceId)
                                                    .Select(x => new
                                                    {
                                                        x.PersonRoleId,
                                                        FirstAddress = x.Addresses.Select(a => new
                                                        {
                                                            a.Address.CountryCode,
                                                            a.Address.AddressTypeId,
                                                            a.Address.FullAddress
                                                        }).FirstOrDefault()
                                                    }).FirstOrDefaultAsync();
                        if (documentPersonInfo != null)
                        {
                            personRoleId = documentPersonInfo.PersonRoleId;
                            countryCode = documentPersonInfo.FirstAddress?.CountryCode;
                            addressFull = documentPersonInfo.FirstAddress?.CountryCode;
                            addressTypeId = documentPersonInfo.FirstAddress?.AddressTypeId;
                        }
                        break;
                    case SourceTypeSelectVM.CasePerson:
                        {
                            var casePersonId = (int)model.PersonSourceId;
                            var casePersonInfo = await repo.AllReadonly<CasePerson>()
                                                        .Where(x => x.Id == casePersonId)
                                                        .Select(x => new
                                                        {
                                                            x.PersonRoleId
                                                        }).FirstOrDefaultAsync();

                            if (casePersonInfo != null)
                            {

                                var firstsAddress = await repo.AllReadonly<CasePersonAddress>()
                                                           .Where(x => x.CasePersonId == casePersonId)
                                                           .Select(x => new
                                                           {
                                                               x.Address.CountryCode,
                                                               x.Address.AddressTypeId,
                                                               x.Address.FullAddress
                                                           }).FirstOrDefaultAsync();

                                personRoleId = casePersonInfo.PersonRoleId;
                                countryCode = firstsAddress?.CountryCode;
                                addressFull = firstsAddress?.CountryCode;
                                addressTypeId = firstsAddress?.AddressTypeId;
                            }
                        }
                        break;
                    default:
                        break;
                }
            if (personRoleId == 0 && defaultPersonRole > 0)
            {
                personRoleId = defaultPersonRole;
            }
            if (personRoleId > 0)
            {
                result.SideInvolvementKindCode = GetNomValue(EpepConstants.Nomenclatures.PersonRoles, personRoleId);
                if (addressTypeId > 0)
                {
                    result.AddressTypeCode = GetNomValue(EpepConstants.Nomenclatures.AddressTypes, addressTypeId);
                    result.CountryCode = countryCode;
                    result.Address = addressFull;
                }
            }

            return result;
        }


        private async Task<SaveResultVM> mapExecProcessFromFastProcess(MQEpep mq, EpepRest.ExecProcess epepRequest)
        {
            int caseId = await repo.GetPropByIdAsync<CaseSessionAct, int>(x => x.Id == (int)mq.ParentSourceId, x => x.CaseId ?? 0);
            if (caseId == 0)
            {
                return new SaveResultVM(false, "Invalid CaseId;");
            }
            lazyDocumentRequestService.Service.InitDbEuro(this.DbEuroConfig);
            FastProcessRequestVM request = (FastProcessRequestVM)(await lazyDocumentRequestService.Service.GetDocumentRequestById(0, caseId, true));

            if (request == null)
            {
                return new SaveResultVM(false, $"Invalid FastProcessRequestVM; caseId={caseId}");
            }

            List<EpepRest.ExecProcessSideModel> sideList = new List<EpepRest.ExecProcessSideModel>();
            List<EpepRest.ExecProcessObligationModel> obligations = new List<EpepRest.ExecProcessObligationModel>();

            BaseRequestPersonInfoVM firstLeftSide = request.LeftSide.FirstOrDefault();
            if (firstLeftSide == null)
            {
                return new SaveResultVM(false, "No beneficiary;");
            }

            EpepRest.ExecProcessSideModel beneficiarySideModel = await getExecProcessCasePerson(firstLeftSide.PersonGid, caseId, false, sideList, firstLeftSide.CasePersonId);
            if (beneficiarySideModel == null)
            {
                return new SaveResultVM(false, "Invalid beneficiary");
            }
            epepRequest.JointDistribution = true;
            if (request.JoinedDestributionTypeId == FastProcessRequestVM.IntRadio_No)
            {
                epepRequest.JointDistribution = false;
                foreach (var debt in request.DebtDistributions)
                {
                    string obligationType = FastProcessGetObligationTypeCodeFromClaimCode(request, debt.ClaimCode);
                    if (string.IsNullOrEmpty(obligationType))
                    {
                        return new SaveResultVM(false, $"Invalid obligationType; claimCode: {debt.ClaimCode}");
                    }

                    var obligation = new EpepRest.ExecProcessObligationModel();
                    obligation.Gid = Guid.NewGuid();
                    obligation.ObligationTypeCode = GetNomValue(EpepConstants.Nomenclatures.ExecProcessObligation, obligationType);
                    obligation.BeneficiaryGid = beneficiarySideModel.Gid;

                    EpepRest.ExecProcessSideModel debtor = await getExecProcessCasePerson(debt.PersonCode, caseId, true, sideList);
                    if (debtor == null)
                    {
                        return new SaveResultVM(false, $"Invalid CasePerson Debtor, debt {debt.Index}");
                    }

                    if (string.IsNullOrEmpty(debtor.SideInvolvementKindCode))
                    {
                        return new SaveResultVM(false, $"Invalid EpepId obligation: {debt.ClaimCode}");
                    }
                    var debtorGids = new List<Guid>()
                    {
                        debtor.Gid,
                    };
                    obligation.Debtors = debtorGids.ToArray();
                    obligation.Amount = debt.Amount;
                    obligation.CurrencyCode = debt.CurrencyCode;
                    if (obligationType.StartsWith("1|"))
                    {
                        var moneyClaim = request.MoneyClaims.Where(m => $"1|{m.Gid}" == debt.ClaimCode).FirstOrDefault();
                        if (moneyClaim != null)
                        {
                            obligation.StatutoryInterestDate = moneyClaim.StatutoryinterestDate;
                        }
                    }

                    obligation.Description = $"Дял: {debt.ShareProcent} %";
                    obligations.Add(obligation);
                }
            }
            else
            {
                for (int d = 0; d < request.RightSide.Length; d++)
                {
                    await getExecProcessCasePerson(request.RightSide[d].PersonGid, caseId, true, sideList, request.RightSide[d].CasePersonId);

                }
                foreach (var side in sideList)
                {
                    if (!string.IsNullOrEmpty(side.AddressTypeCode))
                    {
                        side.AddressTypeCode = GetNomValue(EpepConstants.Nomenclatures.AddressTypes, side.AddressTypeCode);
                        if (string.IsNullOrEmpty(side.AddressTypeCode))
                        {
                            return new SaveResultVM(false, $"Invalid AddressTypeCode: {side.AddressTypeCode}; {side.SideName}");
                        }
                    }
                }

                var mappedObligationsResult = MapClaimsToObligationsJointDistribution(request, caseId, sideList);
                if (!mappedObligationsResult.Result)
                {
                    return mappedObligationsResult;
                }
                obligations = (List<EpepRest.ExecProcessObligationModel>)mappedObligationsResult.ObjectId;
                foreach (var obligation in obligations)
                {
                    obligation.BeneficiaryGid = beneficiarySideModel.Gid;
                    obligation.ObligationTypeCode = GetNomValue(EpepConstants.Nomenclatures.ExecProcessObligation, obligation.ObligationTypeCode);
                    if (string.IsNullOrEmpty(obligation.ObligationTypeCode))
                    {
                        return new SaveResultVM(false, $"Invalid ObligationTypeCode: {obligation.ObligationTypeCode}");
                    }
                }
            }

            //Добавяне на задължение: 1015 Други задължения без суми
            var commonObligation = new EpepRest.ExecProcessObligationModel();
            commonObligation.Gid = Guid.NewGuid();
            commonObligation.ObligationTypeCode = "1015";
            commonObligation.BeneficiaryGid = beneficiarySideModel.Gid;
            commonObligation.Debtors = sideList.Where(x => x.IsDebtor == true).Select(x => x.Gid).ToArray();
            commonObligation.Amount = 0M;
            commonObligation.CurrencyCode = (DbEuroConfig.IsInEuro) ? "EUR" : "BGN";
            commonObligation.Description = "";
            obligations.Add(commonObligation);

            epepRequest.Sides = sideList.ToArray();
            epepRequest.Obligations = obligations.ToArray();

            return new SaveResultVM(true);
        }

        private string FastProcessGetObligationTypeCodeFromClaimCode(FastProcessRequestVM request, string claimCode)
        {
            if (claimCode.StartsWith("1|"))
            {
                return request.MoneyClaims.Where(x => x.Gid == claimCode.Replace("1|", "")).Select(x => "1|" + x.MoneyClaimTypeCode).FirstOrDefault();
            }
            if (claimCode.StartsWith("2|"))
            {
                return FastProcessRequestVM.ObligationType_SubstItem;
            }
            if (claimCode.StartsWith("3|"))
            {
                return FastProcessRequestVM.ObligationType_Item;
            }
            if (claimCode.StartsWith("4|"))
            {
                return request.Expenses.Where(x => x.Gid == claimCode.Replace("4|", "")).Select(x => "4|" + x.ExpenseTypeCode).FirstOrDefault();
            }
            return string.Empty;
        }
        public SaveResultVM MapClaimsToObligationsJointDistribution(FastProcessRequestVM request, int caseId, List<EpepRest.ExecProcessSideModel> sideList)
        {
            var result = new List<EpepRest.ExecProcessObligationModel>();


            int debtorCount = request.RightSide.Length;
            if (request.ClaimTypes.MoneyClaim)
            {

                foreach (var moneyClaim in request.MoneyClaims)
                {
                    var obligation = new EpepRest.ExecProcessObligationModel();
                    obligation.Gid = Guid.NewGuid();
                    obligation.Amount = moneyClaim.Amount;
                    obligation.ObligationTypeCode = $"1|{moneyClaim.MoneyClaimTypeCode}";
                    obligation.CurrencyCode = moneyClaim.CurrencyCode;
                    obligation.StatutoryInterestDate = moneyClaim.StatutoryinterestDate;
                    obligation.Description = moneyClaim.Description ?? "";
                    if (moneyClaim.HasStatutoryinterest)
                    {
                        obligation.StatutoryInterestDate = moneyClaim.StatutoryinterestDate;
                    }
                    obligation.Debtors = sideList.Where(s => s.IsDebtor).Select(s => s.Gid).ToArray();
                    result.Add(obligation);
                }
            }
            if (request.ClaimTypes.ItemSubstitutionClaim)
            {
                foreach (var itemSubstitution in request.ItemSubstitutionClaims)
                {
                    var obligation = new EpepRest.ExecProcessObligationModel();
                    obligation.Gid = Guid.NewGuid();
                    obligation.Amount = getAmountFromBGN_EUR(itemSubstitution.TotalAmountBGN, itemSubstitution.TotalAmountEUR);
                    obligation.ObligationTypeCode = FastProcessRequestVM.ObligationType_SubstItem;
                    obligation.CurrencyCode = getCurrencyCodeFromBGN_EUR(itemSubstitution.TotalAmountBGN, itemSubstitution.TotalAmountEUR);
                    obligation.Description = $"{itemSubstitution.TypeName} - {itemSubstitution.QuantityText}";

                    obligation.Debtors = sideList.Where(s => s.IsDebtor).Select(s => s.Gid).ToArray();
                    result.Add(obligation);
                }
            }
            if (request.ClaimTypes.ItemClaim)
            {

                var obligation = new EpepRest.ExecProcessObligationModel();
                obligation.Gid = Guid.NewGuid();
                obligation.Amount = getAmountFromBGN_EUR(request.ItemClaim.TotalAmountBGN, request.ItemClaim.TotalAmountEUR);
                obligation.ObligationTypeCode = FastProcessRequestVM.ObligationType_Item;
                obligation.CurrencyCode = getCurrencyCodeFromBGN_EUR(request.ItemClaim.TotalAmountBGN, request.ItemClaim.TotalAmountEUR);
                obligation.Description = request.ItemClaim.Description;
                obligation.Debtors = sideList.Where(s => s.IsDebtor).Select(s => s.Gid).ToArray();
                result.Add(obligation);
            }


            foreach (var expence in request.Expenses)
            {
                var obligation = new EpepRest.ExecProcessObligationModel();
                obligation.Gid = Guid.NewGuid();
                obligation.Amount = getAmountFromBGN_EUR(expence.TotalAmountBGN, expence.TotalAmountEUR);
                obligation.ObligationTypeCode = $"4|{expence.ExpenseTypeCode}";
                obligation.CurrencyCode = getCurrencyCodeFromBGN_EUR(expence.TotalAmountBGN, expence.TotalAmountEUR);
                obligation.Description = expence.Description;
                obligation.Debtors = sideList.Where(s => s.IsDebtor).Select(s => s.Gid).ToArray();
                result.Add(obligation);
            }


            return new SaveResultVM(true)
            {
                ObjectId = result
            };

        }


        private decimal getAmountFromBGN_EUR(decimal totalBGN, decimal totalEUR)
        {
            if (totalEUR > 0M)
            {
                return totalEUR;
            }
            return totalBGN;
        }
        private string getCurrencyCodeFromBGN_EUR(decimal totalBGN, decimal totalEUR)
        {
            if (totalEUR > 0M)
            {
                return NomenclatureConstants.CurrencyCode.EUR;
            }
            return NomenclatureConstants.CurrencyCode.BGN;
        }

        private async Task<EpepRest.ExecProcessSideModel> getExecProcessCasePerson(string personGid, int caseId, bool isDebtor, List<EpepRest.ExecProcessSideModel> sideList, int? casePersonId = null)
        {
            Expression<Func<CasePerson, bool>> whereFindPerson = x => x.PersonGid == personGid;
            if (casePersonId > 0)
            {
                whereFindPerson = x => x.Id == casePersonId.Value;
            }
            var casePersonInfo = await repo.AllReadonly<CasePerson>()
                                         .Where(x => x.CaseId == caseId && x.CaseSessionId == null)
                                         .Where(whereFindPerson)
                                         .Select(x => new
                                         {
                                             x.Id,
                                             x.IsPerson,
                                             x.UicTypeId,
                                             x.Uic,
                                             x.FullName,
                                             x.PersonRoleId
                                         })
                                         .FirstOrDefaultAsync();

            if (casePersonInfo == null)
            {
                return null;
            }

            var mappedPerson = sideList.Where(s => s.EissId == casePersonInfo.Id).FirstOrDefault();
            if (mappedPerson != null)
            {
                return mappedPerson;
            }


            var newPerson = new EpepRest.ExecProcessSideModel();
            newPerson.Gid = Guid.NewGuid();
            newPerson.EissId = casePersonInfo.Id;
            newPerson.IsDebtor = isDebtor;
            newPerson.SubjectKind = (casePersonInfo.IsPerson) ? EpepConstants.SubjectKinds.Person : EpepConstants.SubjectKinds.Entity;
            newPerson.SideUic = casePersonInfo.Uic;
            newPerson.SideName = casePersonInfo.FullName;
            newPerson.SideInvolvementKindCode = GetNomValue(EpepConstants.Nomenclatures.PersonRoles, casePersonInfo.PersonRoleId);

            var addressInfo = await repo.AllReadonly<CasePersonAddress>()
                                                .Where(x => x.CasePersonId == casePersonInfo.Id && x.DateExpired == null)
                                                .OrderBy(x => x.Id)
                                                .Select(x => new
                                                {
                                                    x.Address.CountryCode,
                                                    x.Address.AddressTypeId,
                                                    x.Address.FullAddress
                                                }).FirstOrDefaultAsync();

            if (addressInfo != null)
            {
                newPerson.AddressTypeCode = GetNomValue(EpepConstants.Nomenclatures.AddressTypes, addressInfo.AddressTypeId);
                newPerson.CountryCode = addressInfo.CountryCode;
                newPerson.Address = addressInfo.FullAddress;
            }
            sideList.Add(newPerson);
            return newPerson;
        }

        private async Task<Guid> getCasePersonEpepIdByGid(string personGid, int caseId, int loadedId = 0)
        {
            var casePersonId = loadedId;
            if (loadedId == 0)
            {
                casePersonId = await repo.AllReadonly<CasePerson>()
                                         .Where(x => x.CaseId == caseId && x.CaseSessionId == null)
                                         .Where(x => x.PersonGid == personGid)
                                         .Select(x => x.Id)
                                         .FirstOrDefaultAsync();
            }
            if (casePersonId == 0)
            {
                return Guid.Empty;
            }

            return getKeyGuid(SourceTypeSelectVM.CasePerson, casePersonId);
        }
    }
}
