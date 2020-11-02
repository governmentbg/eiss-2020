// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Eispp.ActualData;
using IOWebApplication.Infrastructure.Models.Integrations.Eispp;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class CasePersonSentenceService : BaseService, ICasePersonSentenceService
    {
        private readonly IEisppService eisppService;
        private readonly INomenclatureService nomenclatureService;

        public CasePersonSentenceService(
        ILogger<CasePersonSentenceService> _logger,
        IRepository _repo,
        IEisppService _eisppService,
        INomenclatureService _nomenclatureService,
        AutoMapper.IMapper _mapper,
        IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            mapper = _mapper;
            userContext = _userContext;
            eisppService = _eisppService;
            nomenclatureService = _nomenclatureService;
        }

        /// <summary>
        /// Извличане на данни за Присъда по лице в дело
        /// </summary>
        /// <param name="CasePersonId"></param>
        /// <returns></returns>
        public IQueryable<CasePersonSentenceVM> CasePersonSentence_Select(int CasePersonId)
        {
            return repo.AllReadonly<CasePersonSentence>()
                       .Include(x => x.CasePerson)
                       .Include(x => x.DecreedCourt)
                       .Include(x => x.SentenceResultType)
                       .Include(x => x.CaseSessionAct)
                       .ThenInclude(x => x.ActType)
                       .Include(x => x.CaseSessionAct)
                       .ThenInclude(x => x.CaseSession)
                       .ThenInclude(x => x.Case)
                       .ThenInclude(x => x.CaseType)
                       .ThenInclude(x => x.CaseInstance)
                       .Where(x => x.CasePersonId == CasePersonId)
                       .Select(x => new CasePersonSentenceVM()
                       {
                           Id = x.Id,
                           CaseId = x.CaseId,
                           CasePersonName = x.CasePerson.FullName,
                           SentenceResultTypeLabel = x.SentenceResultType != null ? x.SentenceResultType.Label : string.Empty,
                           CaseSessionActLabel = x.CaseSessionAct != null ? x.CaseSessionAct.ActType.Label + " " + x.CaseSessionAct.RegNumber + "/" + (x.CaseSessionAct.RegDate != null ? (x.CaseSessionAct.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty) : string.Empty,
                           CourtLabel = x.DecreedCourt.Label,
                           InstanceLabel = x.CaseSessionAct.CaseSession.Case.CaseType.CaseInstance.Code,
                           IsActive = x.IsActive,
                           IsActiveText = (x.IsActive ?? false) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Пълнене на основният модел от моделът за редакция на Присъда по лице в дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private CasePersonSentence FillCasePersonSentence(CasePersonSentenceEditVM model)
        {
            model.InforcerInstitutionId = model.InforcerInstitutionId.NumberEmptyToNull();
            model.ExecInstitutionId = model.ExecInstitutionId.NumberEmptyToNull();
            model.PunishmentActivityId = model.PunishmentActivityId.NumberEmptyToNull();
            model.SentenceExecPeriodId = model.SentenceExecPeriodId.NumberEmptyToNull();
            model.ChangeCaseSessionActId = model.ChangeCaseSessionActId.NumberEmptyToNull();
            model.ChangedCasePersonSentenceId = model.ChangedCasePersonSentenceId.NumberEmptyToNull();

            return new CasePersonSentence()
            {
                Id = model.Id,
                CaseId = model.CaseId,
                CasePersonId = model.CasePersonId,
                CaseSessionActId = model.CaseSessionActId,
                CourtId = model.CourtId,
                DecreedCourtId = model.DecreedCourtId,
                Description = model.Description != null ? model.Description : string.Empty,
                ExecDescription = model.ExecDescription != null ? model.ExecDescription : string.Empty,
                ForInforcementDate = model.ForInforcementDate,
                InforcedDate = model.InforcedDate,
                InforcerInstitutionId = model.InforcerInstitutionId,
                PunishmentActivityDate = model.PunishmentActivityDate,
                PunishmentActivityId = model.PunishmentActivityId,
                SentDate = model.SentDate,
                SentenceExecPeriodId = model.SentenceExecPeriodId,
                SentenceResultTypeId = model.SentenceResultTypeId,
                ChangeCaseSessionActId = model.ChangeCaseSessionActId,
                ChangedCasePersonSentenceId = model.ChangedCasePersonSentenceId,
                AmnestyDocumentNumber = model.AmnestyDocumentNumber != null ? model.AmnestyDocumentNumber : string.Empty,
                EffectiveDateFrom = model.EffectiveDateFrom,
                ExecDate = model.ExecDate,
                ExecInstitutionId = model.ExecInstitutionId,
                ExecRemark = model.ExecRemark != null ? model.ExecRemark : string.Empty,
                InforcerDocumentNumber = model.InforcerDocumentNumber != null ? model.InforcerDocumentNumber : string.Empty,
                NotificationDate = model.NotificationDate,
                EnforceIncomingDocument = model.EnforceIncomingDocument,
                ExecIncomingDocument = model.ExecIncomingDocument,
                IsActive = model.IsActive
            };
        }

        /// <summary>
        /// Запис на Присъда по лице в дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CasePersonSentence_SaveData(CasePersonSentenceEditVM model)
        {
            try
            {
                var modelSave = FillCasePersonSentence(model);
                if (modelSave.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CasePersonSentence>(modelSave.Id);
                    saved.CaseSessionActId = modelSave.CaseSessionActId;
                    saved.DecreedCourtId = modelSave.DecreedCourtId;
                    saved.Description = modelSave.Description != null ? modelSave.Description : string.Empty;
                    saved.ExecDescription = modelSave.ExecDescription != null ? modelSave.ExecDescription : string.Empty;
                    saved.ForInforcementDate = modelSave.ForInforcementDate;
                    saved.InforcedDate = modelSave.InforcedDate;
                    saved.InforcerInstitutionId = modelSave.InforcerInstitutionId;
                    saved.PunishmentActivityDate = modelSave.PunishmentActivityDate;
                    saved.PunishmentActivityId = modelSave.PunishmentActivityId;
                    saved.SentDate = modelSave.SentDate;
                    saved.SentenceExecPeriodId = modelSave.SentenceExecPeriodId;
                    saved.SentenceResultTypeId = modelSave.SentenceResultTypeId;
                    saved.AmnestyDocumentNumber = modelSave.AmnestyDocumentNumber != null ? modelSave.AmnestyDocumentNumber : string.Empty;
                    saved.EffectiveDateFrom = modelSave.EffectiveDateFrom;
                    saved.ExecDate = modelSave.ExecDate;
                    saved.ExecInstitutionId = modelSave.ExecInstitutionId;
                    saved.ExecRemark = modelSave.ExecRemark != null ? modelSave.ExecRemark : string.Empty;
                    saved.InforcerDocumentNumber = modelSave.InforcerDocumentNumber != null ? modelSave.InforcerDocumentNumber : string.Empty;
                    saved.NotificationDate = modelSave.NotificationDate;
                    saved.ChangeCaseSessionActId = modelSave.ChangeCaseSessionActId;
                    saved.ChangedCasePersonSentenceId = modelSave.ChangedCasePersonSentenceId;
                    saved.EnforceIncomingDocument = modelSave.EnforceIncomingDocument;
                    saved.ExecIncomingDocument = modelSave.ExecIncomingDocument;
                    saved.IsActive = model.IsActive;
                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;
                    repo.Update(saved);
                    var casePersonSentenceLawbasesDelete = GetCasePersonSentenceLawbases(modelSave.Id);
                    if (casePersonSentenceLawbasesDelete.Count > 0)
                        repo.DeleteRange(casePersonSentenceLawbasesDelete);
                }
                else
                {
                    var casePersonSentencesUpdate = repo.AllReadonly<CasePersonSentence>().Where(x => x.CaseId == model.CaseId && x.CasePersonId == model.CasePersonId && x.IsActive == true).ToList();
                    foreach (var personSentence in casePersonSentencesUpdate)
                    {
                        personSentence.IsActive = false;
                        repo.Update(personSentence);
                    }

                    modelSave.UserId = userContext.UserId;
                    modelSave.DateWrt = DateTime.Now;
                    repo.Add<CasePersonSentence>(modelSave);
                }

                var casePersonSentencesSave = FillCasePersonSentences(model.LawBases, modelSave.Id, modelSave.CaseId);
                if (casePersonSentencesSave.Count > 0)
                    repo.AddRange<CasePersonSentenceLawbase>(casePersonSentencesSave);

                repo.SaveChanges();

                if (model.Id < 1)
                    model.Id = modelSave.Id;

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Присъда по лице в дело Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни по ид за Присъда по лице в дело в модел за редакция
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CasePersonSentenceEditVM CasePersonSentence_GetById(int id)
        {
            var personSentence = repo.AllReadonly<CasePersonSentence>()
                                     .Include(x => x.CasePerson)
                                     .Where(x => x.Id == id)
                                     .FirstOrDefault();
            var result = new CasePersonSentenceEditVM()
            {
                Id = personSentence.Id,
                CaseId = personSentence.CaseId,
                CourtId = personSentence.CourtId,
                CasePersonId = personSentence.CasePersonId,
                CasePersonName = personSentence.CasePerson.FullName,
                CaseSessionActId = personSentence.CaseSessionActId,
                DecreedCourtId = personSentence.DecreedCourtId,
                Description = personSentence.Description,
                ExecDescription = personSentence.ExecDescription,
                ForInforcementDate = personSentence.ForInforcementDate,
                InforcedDate = personSentence.InforcedDate,
                InforcerInstitutionId = personSentence.InforcerInstitutionId,
                PunishmentActivityDate = personSentence.PunishmentActivityDate,
                PunishmentActivityId = personSentence.PunishmentActivityId,
                SentDate = personSentence.SentDate,
                SentenceExecPeriodId = personSentence.SentenceExecPeriodId,
                SentenceResultTypeId = personSentence.SentenceResultTypeId,
                AmnestyDocumentNumber = personSentence.AmnestyDocumentNumber,
                EffectiveDateFrom = personSentence.EffectiveDateFrom,
                ExecDate = personSentence.ExecDate,
                ExecInstitutionId = personSentence.ExecInstitutionId,
                ExecRemark = personSentence.ExecRemark,
                InforcerDocumentNumber = personSentence.InforcerDocumentNumber,
                NotificationDate = personSentence.NotificationDate,
                LawBases = FillPersonSentenceLawBase(personSentence.Id),
                ChangeCaseSessionActId = personSentence.ChangeCaseSessionActId,
                ChangedCasePersonSentenceId = personSentence.ChangedCasePersonSentenceId,
                IsActive = personSentence.IsActive,
                EnforceIncomingDocument = personSentence.EnforceIncomingDocument,
                ExecIncomingDocument = personSentence.ExecIncomingDocument,
            };

            return result;
        }

        /// <summary>
        /// Пълнене на лист с Нормативна база от НК за присъди
        /// </summary>
        /// <returns></returns>
        public List<CheckListVM> FillLawBase()
        {
            return repo.AllReadonly<SentenceLawbase>()
                       .Select(x => new CheckListVM()
                       {
                           Label = x.Label,
                           Checked = false,
                           Value = x.Id.ToString()
                       })
                       .ToList();
        }

        /// <summary>
        /// Попълване на основният модел на Текстове от НК към присъда
        /// </summary>
        /// <param name="checkListVMs"></param>
        /// <param name="CasePersonSentenceId"></param>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        private List<CasePersonSentenceLawbase> FillCasePersonSentences(List<CheckListVM> checkListVMs, int CasePersonSentenceId, int CaseId)
        {
            List<CasePersonSentenceLawbase> result = new List<CasePersonSentenceLawbase>();

            foreach (var checkList in checkListVMs.Where(x => x.Checked))
            {
                CasePersonSentenceLawbase casePersonSentenceLawbase = new CasePersonSentenceLawbase()
                {
                    CasePersonSentenceId = CasePersonSentenceId,
                    CaseId = CaseId,
                    CourtId = userContext.CourtId,
                    SentenceLawbaseId = int.Parse(checkList.Value),
                    UserId = userContext.UserId,
                    DateWrt = DateTime.Now
                };

                result.Add(casePersonSentenceLawbase);
            }

            return result;
        }

        /// <summary>
        /// Извличане на данни за Текстове от НК към присъда
        /// </summary>
        /// <param name="CasePersonSentenceId"></param>
        /// <returns></returns>
        private List<CasePersonSentenceLawbase> GetCasePersonSentenceLawbases(int CasePersonSentenceId)
        {
            return repo.AllReadonly<CasePersonSentenceLawbase>()
                       .Where(x => x.CasePersonSentenceId == CasePersonSentenceId)
                       .ToList();
        }

        /// <summary>
        /// Попълване в лист за избор на Текстове от НК към присъда
        /// </summary>
        /// <param name="CasePersonSentenceId"></param>
        /// <returns></returns>
        private List<CheckListVM> FillPersonSentenceLawBase(int CasePersonSentenceId)
        {
            var casePersonSentenceLawbases = GetCasePersonSentenceLawbases(CasePersonSentenceId);

            var checkLists = FillLawBase();
            foreach (var checkList in checkLists)
            {
                checkList.Checked = casePersonSentenceLawbases.Any(x => x.SentenceLawbaseId.ToString() == checkList.Value);
            }

            return checkLists;
        }

        /// <summary>
        /// Извличане на Престъпления по НД по дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public IQueryable<CaseCrimeVM> CaseCrime_Select(int CaseId)
        {
            var caseCrimes = repo.AllReadonly<CaseCrime>()
                                 .Where(x => x.CaseId == CaseId)
                                 .Select(x => new CaseCrimeVM()
                                 {
                                     Id = x.Id,
                                     CaseId = x.CaseId,
                                     CrimeCode = x.CrimeCode,
                                     CrimeName = x.CrimeName,
                                     ValueEISSId = x.EISSId,
                                     ValueEISSPNumber = x.EISSPNumber
                                 })
                                 .ToList();

            return caseCrimes.AsQueryable();
        }

        /// <summary>
        /// Запис на Престъпления по НД по дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseCrime_SaveData(CaseCrime model)
        {
            try
            {
                model.StartDateType = model.StartDateType.EmptyToNull(0);
                model.CompletitionDegree = model.CompletitionDegree.EmptyToNull(0);
                model.Status = model.Status.EmptyToNull(0);

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseCrime>(model.Id);
                    saved.CaseId = model.CaseId;
                    saved.EISSId = model.EISSId;
                    saved.EISSPNumber = model.EISSPNumber;

                    if (saved.CrimeCode != model.CrimeCode)
                    {
                        saved.CrimeCode = model.CrimeCode;
                        saved.CrimeName = eisppService.GetByCode(model.CrimeCode).Label;
                    }

                    saved.StartDateType = model.StartDateType;
                    saved.CompletitionDegree = model.CompletitionDegree;
                    saved.Status = model.Status;
                    saved.StatusDate = model.StatusDate;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    model.CrimeName = eisppService.GetByCode(model.CrimeCode).Label;
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    repo.Add<CaseCrime>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Престъпления по НД по дело Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане по ид на Престъпления по НД по дело
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CaseCrimeVM CaseCrime_GetById(int id)
        {
            var caseCrime = repo.GetById<CaseCrime>(id);
            return new CaseCrimeVM()
            {
                Id = caseCrime.Id,
                CaseId = caseCrime.CaseId,
                CrimeCode = caseCrime.CrimeCode,
                CrimeName = caseCrime.CrimeName,
                ValueEISSId = caseCrime.EISSId,
                ValueEISSPNumber = caseCrime.EISSPNumber
            };
        }

        /// <summary>
        /// Пълнене на комбо с Престъпления по НД по дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownList_CasePersonCrime(int caseId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = repo.All<CaseCrime>()
                .Where(x => x.CaseId == caseId && x.DateExpired == null)
                .Select(x => new SelectListItem()
                {
                    Text = x.CrimeName,
                    Value = x.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();

            if (result.Count > 0)
            {
                result = result.OrderBy(x => x.Text).ToList();
            }

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        /// <summary>
        /// Извличане на данни за Престъпления към лица по НД по дело
        /// </summary>
        /// <param name="CaseCrimeId"></param>
        /// <returns></returns>
        public IQueryable<CasePersonCrimeVM> CasePersonCrime_Select(int CaseCrimeId)
        {
            var casePersonCrimes = repo.AllReadonly<CasePersonCrime>()
                                       .Include(x => x.CasePerson)
                                       .Include(x => x.CaseCrime)
                                       .Include(x => x.RecidiveType)
                                       .Include(x => x.PersonRoleInCrime)
                                       .Where(x => x.CaseCrimeId == CaseCrimeId && x.DateExpired == null)
                                       .Select(x => new CasePersonCrimeVM()
                                       {
                                           Id = x.Id,
                                           CasePersonName = x.CasePerson.FullName,
                                           CaseCrimeCode = x.CaseCrime.CrimeCode,
                                           RecidiveTypeLabel = x.RecidiveType.Label,
                                           PersonRoleInCrimeLabel = x.PersonRoleInCrime.Label,
                                           CaseCrimeLabel = x.CaseCrime.CrimeName
                                       })
                                       .ToList();

            return casePersonCrimes.AsQueryable();
        }

        /// <summary>
        /// Запис на Престъпления към лица по НД по дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CasePersonCrime_SaveData(CasePersonCrime model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CasePersonCrime>(model.Id);
                    saved.CasePersonId = model.CasePersonId;
                    saved.CaseCrimeId = model.CaseCrimeId;
                    saved.RecidiveTypeId = model.RecidiveTypeId;
                    saved.PersonRoleInCrimeId = model.PersonRoleInCrimeId;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    repo.Add<CasePersonCrime>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Престъпления към лица Id={ model.Id }");
                return false;
            }
        }

        public bool IsExistPersonCasePersonCrime(int CaseCrimeId, int CasePersonId)
        {
            return repo.AllReadonly<CasePersonCrime>()
                       .Any(x => x.CaseCrimeId == CaseCrimeId &&
                                 x.CasePersonId == CasePersonId &&
                                 x.DateExpired == null);
        }

        /// <summary>
        /// Метод за пълнене на престъпления от еиспп към структурите на текущата система
        /// </summary>
        /// <param name="caseId">ид дело</param>
        /// <param name="pnenmr">номер на престъпление</param>
        /// <returns></returns>
        public async Task<bool> CasePersonCrimeFillFromEispp_SaveData(int caseId, string pnenmr)
        {
            try
            {
                var casePeople = repo.AllReadonly<CasePerson>()
                                     .Where(x => x.CaseId == caseId &&
                                                 x.CaseSessionId == null &&
                                                 x.Person_SourceType == SourceTypeSelectVM.EisppPerson)
                                     .ToList();
                var caseModel = repo.AllReadonly<Case>()
                                    .Where(x => x.Id == caseId)
                                    .FirstOrDefault();
                var eisppModel = await eisppService.GetTSAKTSTSResponse(caseModel.EISSPNumber).ConfigureAwait(false);
                var pne = eisppModel.execTSAKTSTSResponse.sNPRAKTSTS.sPNE.FirstOrDefault(x => x.pnenmr == pnenmr);
                var caseCrime = new CaseCrime()
                {
                    CourtId = userContext.CourtId,
                    CaseId = caseId,
                    EISSId = pne.pnesid,
                    EISSPNumber = pne.pnenmr,
                    CrimeCode = pne.PNESTA.pnekcq,
                    CrimeName = eisppService.GetByCode(pne.PNESTA.pnekcq).Label,
                    DateFrom = pne.pnedtaotd,
                    StartDateType = pne.pneotdtip.ToInt(),
                    DateTo = pne.pnedtadod != Crime.defaultDate ? (DateTime?)pne.pnedtadod : null,
                    Status = pne.PNESTA?.pnests.ToInt(),
                    StatusDate = pne.PNESTA?.pnestsdta != Crime.defaultDate ? (DateTime?)pne.PNESTA?.pnestsdta : null,
                    CompletitionDegree = pne.PNESTA?.pnestpdvs.ToInt() 
                };

                caseCrime.CasePersonCrimes = new List<CasePersonCrime>();

                if (eisppModel.execTSAKTSTSResponse.sNPRAKTSTS.sFZL != null)
                {
                    foreach (var fzl in eisppModel.execTSAKTSTSResponse.sNPRAKTSTS.sFZL)
                    {
                        var fzlpneList = eisppModel.execTSAKTSTSResponse.sNPRAKTSTS.sNPRFZLPNE
                                                   .Where(x => x.pnesid.ToString() == pne.pnesid)
                                                   .Where(x => x.fzlsid.ToString() == fzl.fzlsid)
                                                   .ToList();
                        foreach (var fzlpne in fzlpneList)
                        {
                            var person = casePeople.Where(p => (p.Person_SourceId ?? 0).ToString() == fzl.fzlsid).FirstOrDefault();
                            if (person == null)
                                person = casePeople.Where(p => p.Uic == fzl.fzlegn).FirstOrDefault();
                            if (person != null)
                            {
                                var personCrime = new CasePersonCrime()
                                {
                                    CourtId = userContext.CourtId,
                                    CaseId = caseId,
                                    CasePersonId = person.Id,
                                    CaseCrimeId = caseCrime.Id,
                                    PersonRoleInCrimeId = nomenclatureService.GetInnerCodeFromCodeMapping(NomenclatureConstants.CodeMappingAlias.CasePersonCrimeRole, fzlpne.SCQ.scqrlq),
                                    RecidiveTypeId = nomenclatureService.GetInnerCodeFromCodeMapping(EISPPConstants.EisppMapping.Relaps, fzlpne.SBC?.sbcrcd),
                                };
                                if (personCrime.RecidiveTypeId <= 0)
                                    personCrime.RecidiveTypeId = NomenclatureConstants.RecidiveTypes.None;

                                caseCrime.CasePersonCrimes.Add(personCrime);
                            }
                        }
                    }
                }

                repo.Add(caseCrime);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Престъпление от ЕИСПП Id={ caseId }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за Наложени наказания към присъда
        /// </summary>
        /// <param name="CasePersonSentenceId"></param>
        /// <returns></returns>
        public IQueryable<CasePersonSentencePunishmentVM> CasePersonSentencePunishment_Select(int CasePersonSentenceId)
        {
            return repo.AllReadonly<CasePersonSentencePunishment>()
                .Include(x => x.SentenceType)
                .Where(x => x.CasePersonSentenceId == CasePersonSentenceId)
                .Select(x => new CasePersonSentencePunishmentVM()
                {
                    Id = x.Id,
                    IsSummaryPunishment = x.IsSummaryPunishment,
                    IsSummaryPunishmentText = (x.IsSummaryPunishment) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No,
                    SentenceText = x.SentenceText,
                    SentenceTypeLabel = x.SentenceType.Label,
                    SentenseMoney = x.SentenseMoney,
                    CasePersonSentenceId = x.CasePersonSentenceId
                })
                .AsQueryable();
        }

        /// <summary>
        /// Извличане на данни по ид за Наложени наказания към присъда
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CasePersonSentencePunishmentVM CasePersonSentencePunishment_GetById(int id)
        {
            return repo.AllReadonly<CasePersonSentencePunishment>()
                       .Include(x => x.SentenceType)
                       .Where(x => x.Id == id)
                       .Select(x => new CasePersonSentencePunishmentVM()
                       {
                           Id = x.Id,
                           IsSummaryPunishment = x.IsSummaryPunishment,
                           IsSummaryPunishmentText = (x.IsSummaryPunishment) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No,
                           SentenceText = x.SentenceText,
                           SentenceTypeLabel = x.SentenceType.Label,
                           SentenseMoney = x.SentenseMoney,
                           CasePersonSentenceId = x.CasePersonSentenceId
                       })
                       .FirstOrDefault();
        }

        /// <summary>
        /// Запис на Наложени наказания към присъда
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CasePersonSentencePunishment_SaveData(CasePersonSentencePunishment model)
        {
            try
            {
                model.SentenceTypeId = model.SentenceTypeId.NumberEmptyToNull();
                model.SentenceRegimeTypeId = model.SentenceRegimeTypeId.NumberEmptyToNull();

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CasePersonSentencePunishment>(model.Id);
                    var sentenceType = repo.GetById<SentenceType>(model.SentenceTypeId);
                    saved.CasePersonSentenceId = model.CasePersonSentenceId;
                    saved.IsSummaryPunishment = model.IsSummaryPunishment;
                    saved.SentenceTypeId = model.SentenceTypeId;
                    saved.SentenseMoney = model.SentenseMoney;
                    saved.SentenseDays = model.SentenseDays;
                    saved.SentenseWeeks = model.SentenseWeeks;
                    saved.SentenseMonths = model.SentenseMonths;
                    saved.SentenseYears = model.SentenseYears;
                    saved.SentenceText = model.SentenceText;
                    saved.SentenceRegimeTypeId = model.SentenceRegimeTypeId;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo;
                    saved.Description = model.Description;

                    if (!(sentenceType.HasProbation ?? false))
                    {
                        saved.ProbationStartDate = null;
                        saved.ProbationDays = null;
                        saved.ProbationMonths = null;
                        saved.ProbationWeeks = null;
                        saved.ProbationYears = null;
                    }
                    else
                    {
                        saved.ProbationStartDate = model.ProbationStartDate;
                        saved.ProbationDays = model.ProbationDays;
                        saved.ProbationMonths = model.ProbationMonths;
                        saved.ProbationWeeks = model.ProbationWeeks;
                        saved.ProbationYears = model.ProbationYears;
                    }

                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    repo.Add<CasePersonSentencePunishment>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Наложени наказания към присъда Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за Наложени наказания към присъда
        /// </summary>
        /// <param name="CasePersonSentencePunishmentId"></param>
        /// <returns></returns>
        public IQueryable<CasePersonSentencePunishmentCrimeVM> CasePersonSentencePunishmentCrime_Select(int CasePersonSentencePunishmentId)
        {
            return repo.AllReadonly<CasePersonSentencePunishmentCrime>()
                .Include(x => x.CaseCrime)
                .Include(x => x.PersonRoleInCrime)
                .Include(x => x.RecidiveType)
                .Where(x => x.CasePersonSentencePunishmentId == CasePersonSentencePunishmentId)
                .Select(x => new CasePersonSentencePunishmentCrimeVM()
                {
                    Id = x.Id,
                    CaseCrimeLabel = x.CaseCrime.CrimeCode,
                    PersonRoleInCrimeLabel = x.PersonRoleInCrime.Label,
                    RecidiveTypeLabel = x.RecidiveType.Label
                })
                .AsQueryable();
        }

        /// <summary>
        /// Запис на Наложени наказания към присъда
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CasePersonSentencePunishmentCrime_SaveData(CasePersonSentencePunishmentCrime model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CasePersonSentencePunishmentCrime>(model.Id);
                    saved.CasePersonSentencePunishmentId = model.CasePersonSentencePunishmentId;
                    saved.CaseCrimeId = model.CaseCrimeId;
                    saved.PersonRoleInCrimeId = model.PersonRoleInCrimeId;
                    saved.RecidiveTypeId = model.RecidiveTypeId;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    repo.Add<CasePersonSentencePunishmentCrime>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Наложени наказания към присъда Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за комбо на присъди
        /// </summary>
        /// <param name="CasePersonId"></param>
        /// <param name="ModelId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownList_CasePersonSentence(int CasePersonId, int ModelId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = CasePersonSentence_Select(CasePersonId)
                         .Where(x => (ModelId > 0) ? x.Id != ModelId : true)
                         .Select(x => new SelectListItem()
                         {
                             Text = x.CaseSessionActLabel + " - " + x.SentenceResultTypeLabel,
                             Value = x.Id.ToString()
                         }).ToList() ?? new List<SelectListItem>();

            if (result.Count > 0)
            {
                result = result.OrderBy(x => x.Text).ToList();
            }

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        /// <summary>
        /// Извличане на данни за бюлетин по лице
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        public CasePersonSentenceBulletin CasePersonSentenceBulletin_GetByIdPerson(int personId)
        {
            return repo.AllReadonly<CasePersonSentenceBulletin>()
                                .Where(x => x.CasePersonId == personId)
                                .FirstOrDefault();
        }

        /// <summary>
        /// Извличане на данни за бюлетин по ид
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CasePersonSentenceBulletinEditVM CasePersonSentenceBulletin_GetById(int id)
        {
            return repo.AllReadonly<CasePersonSentenceBulletin>()
                                .Select(x => new CasePersonSentenceBulletinEditVM()
                                {
                                    Id = x.Id,
                                    CaseId = x.CaseId,
                                    CourtId = x.CourtId,
                                    CasePersonId = x.CasePersonId,
                                    BirthDayPlace = x.BirthDayPlace,
                                    BirthDay = x.BirthDay,
                                    Nationality = x.Nationality,
                                    FamilyMarriage = x.FamilyMarriage,
                                    FatherName = x.FatherName,
                                    MotherName = x.MotherName,
                                    CaseTypeId = x.Case.CaseTypeId,
                                    IsAdministrativePunishment = x.IsAdministrativePunishment,
                                    SentenceDescription = x.SentenceDescription,
                                    IsConvicted = x.IsConvicted ?? false,
                                    LawUnitSignId = x.LawUnitSignId ?? 0,
                                })
                                .Where(x => x.Id == id)
                                .FirstOrDefault();
        }

        /// <summary>
        /// Запис на бюлетин
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) CasePersonSentenceBulletin_SaveData(CasePersonSentenceBulletinEditVM model)
        {
            try
            {
                if (model.Id == 0)
                {
                    var exists = repo.AllReadonly<CasePersonSentenceBulletin>()
                                       .Where(x => x.Id != model.Id)
                                       .Where(x => x.CasePersonId == model.CasePersonId)
                                       .Any();
                    if (exists == true)
                    {
                        return (result: false, errorMessage: "Вече има въведени данни");
                    }
                }
                CasePersonSentenceBulletin saved = null;
                if (model.Id > 0)
                {
                    //Update
                    saved = repo.GetById<CasePersonSentenceBulletin>(model.Id);
                    repo.Update(saved);
                }
                else
                {
                    saved = new CasePersonSentenceBulletin();
                    saved.CasePersonId = model.CasePersonId;
                    saved.CaseId = model.CaseId;
                    saved.CourtId = model.CourtId;
                }

                saved.BirthDayPlace = model.BirthDayPlace;
                saved.BirthDay = model.BirthDay;
                saved.Nationality = model.Nationality;
                saved.FamilyMarriage = model.FamilyMarriage;
                saved.FatherName = model.FatherName;
                saved.MotherName = model.MotherName;
                saved.SentenceDescription = model.SentenceDescription;
                saved.UserId = userContext.UserId;
                saved.DateWrt = DateTime.Now;
                saved.IsAdministrativePunishment = model.IsAdministrativePunishment;
                saved.IsConvicted = model.IsConvicted;
                saved.LawUnitSignId = model.LawUnitSignId;
                
                if (model.Id > 0)
                {
                    repo.Update(saved);
                }
                else
                {
                    repo.Add<CasePersonSentenceBulletin>(saved);
                }

                repo.SaveChanges();
                model.Id = saved.Id;
                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CasePersonSentenceBulletin Id={ model.Id }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Извличане на данни за лице по ид
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        public CasePersonSentence CasePersonSentence_GetByPerson(int personId)
        {
            return repo.AllReadonly<CasePersonSentence>()
                       .Include(x => x.CasePerson)
                       .Where(x => x.CasePersonId == personId)
                       .Where(x => x.IsActive == true)
                       .OrderByDescending(x => x.Id)
                       .FirstOrDefault();
        }
    }
}
