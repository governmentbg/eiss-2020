// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Models;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation;
using IOWebApplication.Infrastructure.Models.ViewModels.Common.Mediation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Data.Models.Money;
using NPOI.SS.Formula.Functions;

namespace IOWebApplication.Core.Services
{
    /// <summary>
    /// Медиация
    /// </summary>
    public class MediationService : BaseService, IMediationService
    {
        private readonly ICasePersonLinkService casePersonLinkService;
        private readonly ICaseLifecycleService caseLifecycleService;

        /// <summary>
        /// Медиация
        /// </summary>
        /// <param name="_logger"></param>
        /// <param name="_repo"></param>
        /// <param name="_userContext"></param>
        /// <param name="_casePersonLinkService"></param>
        /// <param name="_caseLifecycleService"></param>
        public MediationService(ILogger<MediationService> _logger,
                                IRepository _repo,
                                IUserContext _userContext,
                                ICasePersonLinkService _casePersonLinkService,
                                ICaseLifecycleService _caseLifecycleService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            casePersonLinkService = _casePersonLinkService;
            caseLifecycleService = _caseLifecycleService;
        }

        #region Работа с дела за медиация

        /// <summary>
        /// Where клауза за лицето като координатор за кои съдилища отговаря
        /// </summary>
        /// <returns></returns>
        private async Task<Expression<Func<Case, bool>>> GetUserWhere()
        {
            DateTime dateNow = DateTime.Now;
            DateTime dateNow100 = DateTime.Now.AddYears(100);

            Expression<Func<Case, bool>> userWhere = x => false;

            // TO-DO това трябва да се размаркира
            int lawUnitId = userContext.LawUnitId;

            var dataCoordinator = await repo.AllReadonly<MediationCoordinator>()
                                            .Where(x => x.LawUnitId == lawUnitId)
                                            .Where(x => x.DateFrom <= dateNow)
                                            .Where(x => (x.DateTo ?? dateNow) >= dateNow)
                                            .Select(x => new
                                            {
                                                x.DateFrom,
                                                x.DateTo,
                                                courtIds = x.Centers.SelectMany(c => c.Center.Courts.Select(s => s.CourtId)).ToArray()
                                            })
                                            .FirstOrDefaultAsync();

            if (dataCoordinator == null)
                return userWhere;

            if (dataCoordinator.courtIds == null || dataCoordinator.courtIds.Length < 1)
                return userWhere;

            userWhere = x => (x.IsMediation ?? false) &&
                             dataCoordinator.courtIds.Contains(x.CourtId) &&
                             !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null) &&
                             x.CaseStateId != NomenclatureConstants.CaseState.Draft;

            return userWhere;

        }

        /// <summary>
        /// Where клауза за от дата
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        private Expression<Func<Case, bool>> GetDateFromWhere(MediationCaseFilterVM filter)
        {
            Expression<Func<Case, bool>> dateFromWhere = x => true;
            if (filter.DateFrom != null)
            {
                filter.DateFrom = filter.DateFrom.ForceStartDate();
                dateFromWhere = x => x.RegDate >= filter.DateFrom;
            }

            return dateFromWhere;
        }

        /// <summary>
        /// Where клауза за до дата
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        private Expression<Func<Case, bool>> GetDateToWhere(MediationCaseFilterVM filter)
        {
            Expression<Func<Case, bool>> dateToWhere = x => true;
            if (filter.DateTo != null)
            {
                filter.DateTo = filter.DateTo.ForceEndDate();
                dateToWhere = x => x.RegDate <= filter.DateTo;
            }

            return dateToWhere;
        }

        /// <summary>
        /// Where клауза за година на дело
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        private Expression<Func<Case, bool>> GetYearWhere(MediationCaseFilterVM filter)
        {
            Expression<Func<Case, bool>> yearWhere = x => true;
            if ((filter.CaseYear ?? 0) > 0)
                yearWhere = x => x.RegDate >= NomenclatureExtensions.GetPastDate() && x.RegDate.Year == filter.CaseYear;

            return yearWhere;
        }

        /// <summary>
        /// Where клауза за година на дело
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        private Expression<Func<Case, bool>> GetRegNumberWhere(MediationCaseFilterVM filter)
        {
            Expression<Func<Case, bool>> regNumberWhere = x => true;
            if (!string.IsNullOrEmpty(filter.RegNumber))
                regNumberWhere = x => EF.Functions.ILike(x.RegNumber, filter.RegNumber.ToCasePaternSearch());

            return regNumberWhere;
        }

        /// <summary>
        /// Метод извличащ данни за дела подлежащи на медиация
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public async Task<IQueryable<MediationCaseListDataVM>> GetMediationCases(MediationCaseFilterVM filter)
        {
            DateTime dateNow = DateTime.Now;

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupWhere = x => x.CaseGroupId == filter.CaseGroupId;

            Expression<Func<Case, bool>> caseTypeWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeWhere = x => x.CaseTypeId == filter.CaseTypeId;

            Expression<Func<Case, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.CaseCodeId ?? 0);
            }

            Expression<Func<Case, bool>> caseCodeSubIdWhere = x => true;
            if ((filter.CaseCodeSubId ?? 0) > 0)
                caseCodeSubIdWhere = x => x.CaseCodeSubId == filter.CaseCodeSubId;

            return repo.AllReadonly<Case>()
                       .Where(await GetUserWhere())
                       .Where(GetDateFromWhere(filter))
                       .Where(GetDateToWhere(filter))
                       .Where(GetYearWhere(filter))
                       .Where(GetRegNumberWhere(filter))
                       .Where(caseGroupWhere)
                       .Where(caseTypeWhere)
                       .Where(caseCodeIdsWhere)
                       .Where(caseCodeSubIdWhere)
                       .Select(x => new MediationCaseListDataVM()
                       {
                           Id = x.Id,
                           CourtId = x.CourtId,
                           CourtLabel = x.Court.Label,
                           RegNumber = x.RegNumber,
                           RegDate = x.RegDate,
                           CaseTypeCode = x.CaseType.Code,
                           JudgeReport = x.CaseLawUnits.Where(l => l.CaseSessionId == null && l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                                   (l.DateTo ?? DateTime.Now.AddYears(100)) >= dateNow)
                                                       .Select(l => l.LawUnit.FullName)
                                                       .FirstOrDefault(),
                           DepartmentOtdelenieText = (x.Otdelenie != null && x.Otdelenie.DepartmentTypeId != NomenclatureConstants.DepartmentType.Napravlenie ? x.Otdelenie.Label : string.Empty) +
                                                     (x.JudicalComposition != null ? (x.Otdelenie != null && x.Otdelenie.DepartmentTypeId != NomenclatureConstants.DepartmentType.Napravlenie ? " / " + x.JudicalComposition.Label : x.JudicalComposition.Label) : string.Empty),
                           CaseCodeLabel = x.CaseCode.Code,
                           CaseCodeSubLabel = x.CaseCodeSub.Code,
                           ProcessPriorityLabel = x.ProcessPriority.Label,
                           CaseStateLabel = x.CaseState.Label
                       });
        }

        /// <summary>
        /// Метод извличащ данни за дела подлежащи на медиация за падащ списък
        /// </summary>
        /// <param name="caseId">Идентификатор на текущо дело</param>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент "Избери"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_MediationCases(int caseId, bool addDefaultElement = true)
        {
            List<SelectListItem> selectListItems = await repo.AllReadonly<Case>()
                                                             .Where(x => x.IsMediation ?? false)
                                                             .Where(x => !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                                                             .Where(x => x.CaseStateId != NomenclatureConstants.CaseState.Draft)
                                                             .Where(x => x.Id != caseId)
                                                             .Select(x => new SelectListItem
                                                             {
                                                                 Text = x.RegNumber,
                                                                 Value = x.Id.ToString()
                                                             })
                                                             .ToListAsync();

            if (addDefaultElement)
            {
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                                                 .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Извличане на данни за дело подлежащо на медиация
        /// </summary>
        /// <param name="id">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<MediationCasePreviewVM> GetMediationCasePreview(int id)
        {
            string _parentLinkCases = string.Join(", ", await repo.AllReadonly<MediationCaseSessionLinkCase>()
                                                                  .Where(x => x.LinkCaseId == id)
                                                                  .Select(x => x.MediationCaseSession.Case.RegNumber)
                                                                  .ToListAsync());

            MediationCasePreviewVM casePreview = await repo.AllReadonly<Case>()
                                                           .Where(x => x.Id == id)
                                                           .Select(x => new MediationCasePreviewVM()
                                                           {
                                                               Id = x.Id,
                                                               CourtId = x.CourtId,
                                                               CourtLabel = x.Court.Label,
                                                               CaseGroupLabel = x.CaseGroup.Label,
                                                               CaseTypeLabel = x.CaseType.Label,
                                                               CaseCodeLabel = $"{x.CaseCode.Code} {x.CaseCode.Label}",
                                                               ShortNumber = Convert.ToString(int.Parse(x.ShortNumber)),
                                                               RegNumber = x.RegNumber,
                                                               RegDate = x.RegDate,
                                                           })
                                                           .FirstAsync();

            casePreview.ParentLinkCases = _parentLinkCases;
            return casePreview;
        }

        /// <summary>
        /// Метод извличащ данни свързани с медиация от дело за редакция
        /// </summary>
        /// <param name="id">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<MediationCaseEditVM> GetMediationCaseEditById(int id)
        {
            return await repo.AllReadonly<Case>()
                             .Where(x => x.Id == id)
                             .Select(x => new MediationCaseEditVM
                             {
                                 Id = x.Id,
                                 IsMediation = x.IsMediation ?? false,
                                 CaseCodeSubId = x.CaseCodeSubId,
                                 MediationProcedureId = x.MediationProcedureId
                             })
                             .FirstAsync();
        }

        /// <summary>
        /// Редкация на данни от дело подлежащо на медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<int?> SaveMediationCase(MediationCaseEditVM model)
        {
            try
            {
                Case modelSave = await repo.All<Case>()
                                           .Where(x => x.Id == model.Id)
                                           .FirstAsync();

                modelSave.IsMediation = model.IsMediation ?? false;
                modelSave.StartMediationDate = model.IsMediation ?? false ? DateTime.Now : null;
                modelSave.CaseCodeSubId = model.CaseCodeSubId.NumberEmptyToNull();
                modelSave.MediationProcedureId = model.MediationProcedureId.NumberEmptyToNull();

                await repo.SaveChangesAsync();

                if (model.IsMediation ?? false)
                    await caseLifecycleService.StartLifecycleMediation(model.Id);

                return modelSave.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при редакция на дело CaseId = {model.Id}");
                return null;
            }
        }

        /// <summary>
        /// Метод който връща списък с подшифри по шифър взет от дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент "Избери"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_CaseCodeSub(int caseId, bool addDefaultElement = true)
        {
            int? caseCodeId = await repo.GetPropByIdAsync<Case, int?>(x => x.Id == caseId, x => x.CaseCodeId);

            DateTime dateNow = DateTime.Now;

            List<SelectListItem> selectListItems = (caseCodeId ?? 0) > 0 ? await repo.AllReadonly<CaseCodeSub>()
                                                                                     .Where(x => x.IsActive)
                                                                                     .Where(x => x.DateStart <= dateNow)
                                                                                     .Where(x => (x.DateEnd ?? dateNow) >= dateNow)
                                                                                     .Where(x => x.CaseCodeId == caseCodeId || x.CaseCodeId == null)
                                                                                     .Select(x => new SelectListItem
                                                                                     {
                                                                                         Text = x.Code + " " + x.Label,
                                                                                         Value = x.Id.ToString()
                                                                                     })
                                                                                     .ToListAsync() : [];

            selectListItems = selectListItems.OrderBy(x => x.Text).ToList();

            if (addDefaultElement)
            {
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                                                 .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Метод за проверка за съществуваща среща в дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<bool> IsExistMediationCaseSession(int caseId)
        {
            return await repo.AllReadonly<MediationCaseSession>()
                             .AnyAsync(x => x.CaseId == caseId &&
                                            x.DateExpired == null);
        }

        #endregion

        #region Медиатори

        /// <summary>
        /// Where клауза за дата за извличане на даянни за медиатори
        /// </summary>
        /// <param name="dateTime">Дата към която да показва медиаторите към делото</param>
        /// <returns></returns>
        private Expression<Func<MediationCaseMediator, bool>> GetMediatorsDateWhere(DateTime dateTime)
        {
            Expression<Func<MediationCaseMediator, bool>> mediatorsDateWhere = x => x.DateFrom <= dateTime &&
                                                                                    (x.DateTo ?? dateTime) >= dateTime;

            return mediatorsDateWhere;
        }

        /// <summary>
        /// Метод извличащ данни за медиатори към дело
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<MediationCaseMediatorListDataVM> GetMediationCaseMediators(MediationCaseMediatorFilterVM filter)
        {
            return repo.AllReadonly<MediationCaseMediator>()
                       .Where(x => x.CaseId == filter.CaseId)
                       .Where(x => x.DateExpired == null)
                       .Select(x => new MediationCaseMediatorListDataVM()
                       {
                           Id = x.Id,
                           MediationMediatorName = x.Mediator.Name,
                           MediationTypeChoiceMediatorLabel = x.MediationTypeChoiceMediator.Label,
                           DateFrom = x.DateFrom,
                           DateTo = x.DateTo,
                           //SumAppraisal = x.Appraisals.Any(a => a.DateExpired == null) ? (x.Appraisals.Sum(a => (a.TypeAppraisal == NomenclatureConstants.MediationCaseMediatorAppraisalTypeConstants.Detailed ? a.Data.Sum(d => d.Rating) 
                           //                                                                                                                                                                                    : a.Data.Sum(d => d.Rating * (d.PointAppraisal.MultiplicationValue ?? 0)))) / x.Appraisals.Count()) 
                           //                                                            : -1
                       });
        }

        /// <summary>
        /// Метод извличащ данни за медиатори към дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент "Избери"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_MediationCaseMediators(int caseId, bool addDefaultElement = true)
        {
            List<SelectListItem> selectListItems = await repo.AllReadonly<MediationCaseMediator>()
                                                             .Where(x => x.CaseId == caseId)
                                                             .Where(x => x.DateExpired == null)
                                                             .Where(GetMediatorsDateWhere(DateTime.Now))
                                                             .Select(x => new SelectListItem
                                                             {
                                                                 Text = x.Mediator.Name,
                                                                 Value = x.Id.ToString()
                                                             })
                                                             .ToListAsync();

            if (selectListItems.Count != 1)
            {
                if (addDefaultElement)
                {
                    selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                                                     .ToList();
                }
            }

            return selectListItems;
        }

        /// <summary>
        /// Извличане на данни за медиатори към дело
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<MediationCaseMediatorVM> GetMediationCaseMediatorEditById(int id)
        {
            MediationCaseMediatorVM result = await repo.AllReadonly<MediationCaseMediator>()
                                                       .Where(x => x.Id == id)
                                                       .Select(x => new MediationCaseMediatorVM()
                                                       {
                                                           Id = x.Id,
                                                           CaseId = x.CaseId,
                                                           MediationMediatorId = x.MediationMediatorId,
                                                           MediationTypeChoiceMediatorId = x.MediationTypeChoiceMediatorId,
                                                           DateFrom = x.DateFrom,
                                                           DateTo = x.DateTo,
                                                           Description = x.Description,
                                                       })
                                                       .FirstAsync();

            bool existAppraisal = await repo.AllReadonly<MediationCaseMediatorAppraisal>()
                                            .Where(x => x.DateExpired == null)
                                            .Where(x => x.CaseId == result.CaseId)
                                            .Where(x => x.MediationCaseMediatorId == id)
                                            .AnyAsync();

            bool existObligation = await repo.AllReadonly<Obligation>()
                                             .Where(x => x.MediationMediatorId == result.MediationMediatorId)
                                             .Where(x => x.CaseId == result.CaseId)
                                             .AnyAsync();

            result.AllowedExpired = !(existAppraisal || existObligation);

            return result;
        }

        /// <summary>
        /// Попълване на обект за добавяне на медиатори към дело
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private async Task<MediationCaseMediator> FillMediationCaseMediator(MediationCaseMediatorVM model)
        {
            int courtId = await repo.GetPropByIdAsync<Infrastructure.Data.Models.Cases.Case, int>(x => x.Id == model.CaseId, x => x.CourtId);

            return new()
            {
                CourtId = courtId,
                CaseId = model.CaseId,
                MediationMediatorId = model.MediationMediatorId,
                MediationTypeChoiceMediatorId = model.MediationTypeChoiceMediatorId.NumberEmptyToNull(),
                Description = model.Description,
                DateFrom = model.DateFrom,
                DateTo = model.DateTo,
                UserId = userContext.UserId,
                DateWrt = DateTime.Now,
            };
        }

        /// <summary>
        /// Попълване на данни за редакция на медиатори към дело
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <param name="modelSave">Модел за редакция</param>
        private static void SetEditFieldsMediationCaseMediator(MediationCaseMediatorVM model, MediationCaseMediator modelSave)
        {
            modelSave.MediationMediatorId = model.MediationMediatorId;
            modelSave.MediationTypeChoiceMediatorId = model.MediationTypeChoiceMediatorId.NumberEmptyToNull();
            modelSave.Description = model.Description;
            modelSave.DateFrom = model.DateFrom;
            modelSave.DateTo = model.DateTo;
        }

        /// <summary>
        /// Добавяне/редкация на данни за медиатори към дело
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<int?> SaveMediationCaseMediator(MediationCaseMediatorVM model)
        {
            try
            {
                MediationCaseMediator modelSave = (model.Id > 0) ? await repo.All<MediationCaseMediator>()
                                                                             .Where(x => x.Id == model.Id)
                                                                             .FirstAsync() : await FillMediationCaseMediator(model);

                if (model.Id > 0)
                    SetEditFieldsMediationCaseMediator(model, modelSave);
                else
                    repo.Add(modelSave);

                await repo.SaveChangesAsync();
                return modelSave.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на медиатори към дело CaseId = {model.CaseId} и id = {model.Id}");
                return null;
            }
        }

        /// <summary>
        /// Проверка за медиатор, дали съществува в дело
        /// </summary>
        /// <param name="mediatorId">Идентификатор на медиатор</param>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="id">Идентификатор на запис</param>
        /// <returns></returns>
        public async Task<bool> IsExistMediatorInCase(int mediatorId, int caseId, int? id = null)
        {
            Expression<Func<MediationCaseMediator, bool>> idWhere = x => true;
            if ((id ?? 0) > 0)
                idWhere = x => x.Id != id;

            return await repo.AllReadonly<MediationCaseMediator>()
                             .Where(x => x.CaseId == caseId)
                             .Where(x => x.MediationMediatorId == mediatorId)
                             .Where(x => x.DateExpired == null)
                             .Where(idWhere)
                             .AnyAsync();
        }

        #endregion

        #region Срещи

        /// <summary>
        /// Метод извличащ данни за срещи към дело
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<MediationCaseSessionListDataVM> GetMediationCaseSessions(MediationCaseSessionFilterVM filter)
        {
            return repo.AllReadonly<MediationCaseSession>()
                       .Where(x => x.CaseId == filter.CaseId)
                       .Where(x => x.DateExpired == null)
                       .Select(x => new MediationCaseSessionListDataVM()
                       {
                           Id = x.Id,
                           MediationTypeLabel = x.MediationType.Label,
                           MediationLocationLabel = x.MediationLocation.Label,
                           DateFrom = x.DateFrom,
                           DateTo = x.DateTo,
                           MediationStateLabel = x.MediationState.Label
                       });
        }

        /// <summary>
        /// Извличане на данни за среща към дело
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<MediationCaseSessionVM> GetMediationCaseSessionEditById(int id)
        {
            return await repo.AllReadonly<MediationCaseSession>()
                             .Where(x => x.Id == id)
                             .Where(x => NomenclatureConstants.MediationStateConstants.StateForEdit.Contains(x.MediationStateId ?? 0))
                             .Select(x => new MediationCaseSessionVM()
                             {
                                 Id = x.Id,
                                 CaseId = x.CaseId,
                                 MediationTypeId = x.MediationTypeId,
                                 MediationLocationId = x.MediationLocationId,
                                 MediationLocationDescription = x.MediationLocationDescription,
                                 DateFrom = x.DateFrom,
                                 DateTo = x.DateTo,
                                 MediationStateId = x.MediationStateId,
                                 Description = x.Description,
                                 LinkCaseIds = x.LinkCases.Select(x => x.LinkCaseId.ToString()).ToArray()
                             })
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Извличане данни за навигация от среща
        /// </summary>
        /// <param name="id">Идентификатор на среща</param>
        /// <returns></returns>
        public async Task<MediationCaseNavigationVM> GetSessionDataNavigation(int id)
        {
            return await repo.AllReadonly<MediationCaseSession>()
                             .Where(x => x.Id == id)
                             .Select(x => new MediationCaseNavigationVM
                             {
                                 CourtId = x.CourtId ?? 0,
                                 CaseId = x.CaseId,
                                 MediationCaseSessionId = x.Id
                             })
                             .FirstAsync();
        }

        /// <summary>
        /// Попълване на обект за добавяне на среща към дело
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private async Task<MediationCaseSession> FillMediationCaseSession(MediationCaseSessionVM model)
        {
            int courtId = await repo.GetPropByIdAsync<Infrastructure.Data.Models.Cases.Case, int>(x => x.Id == model.CaseId, x => x.CourtId);
            
            int? mediationCenterId = await repo.AllReadonly<MediationCenter>()
                                               .Where(x => x.Courts.Any(c => c.CourtId == courtId))
                                               .Select(x => (int?)x.Id)
                                               .FirstOrDefaultAsync();

            return new()
            {
                CourtId = courtId,
                CaseId = model.CaseId,
                MediationCenterId = mediationCenterId.NumberEmptyToNull(),
                MediationTypeId = model.MediationTypeId,
                MediationLocationId = model.MediationLocationId.NumberEmptyToNull(),
                MediationLocationDescription = model.MediationLocationDescription,
                MediationStateId = model.MediationStateId.NumberEmptyToNull(),
                Description = model.Description,
                DateFrom = model.DateFrom,
                DateTo = model.DateTo,
                UserId = userContext.UserId,
                DateWrt = DateTime.Now,
            };
        }

        /// <summary>
        /// Попълване на данни за редакция на среща към дело
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <param name="modelSave">Модел за редакция</param>
        private static void SetEditFieldsMediationCaseSession(MediationCaseSessionVM model, MediationCaseSession modelSave)
        {
            modelSave.MediationTypeId = model.MediationTypeId;
            modelSave.MediationLocationId = model.MediationLocationId.NumberEmptyToNull();
            modelSave.MediationLocationDescription = model.MediationLocationDescription;
            modelSave.MediationStateId = model.MediationStateId.NumberEmptyToNull();
            modelSave.Description = model.Description;
            modelSave.DateFrom = model.DateFrom;
            modelSave.DateTo = model.DateTo;
        }

        /// <summary>
        /// Метод извличащ страни по делото за сесията
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="dateTo">Към дата</param>
        /// <returns></returns>
        private async Task<List<CasePerson>> GetCasePeople(int caseId, DateTime dateTo)
        {
            return await repo.AllReadonly<CasePerson>()
                             .Where(x => x.CaseId == caseId)
                             .Where(x => x.CaseSessionId == null)
                             .Where(x => x.DateExpired == null)
                             .Where(x => x.DateFrom <= dateTo)
                             .Where(x => (x.DateTo ?? dateTo) >= dateTo)
                             .ToListAsync();

        }

        /// <summary>
        /// Метод подготвящ страни от делото за запис в среща за медиация
        /// </summary>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="dateTo">Към дата</param>
        /// <returns></returns>
        private async Task<List<MediationCasePerson>> GetMediationCasePeople(int? courtId, int caseId, DateTime dateTo)
        {
            List<CasePerson> casePeople = await GetCasePeople(caseId, dateTo);
            return casePeople.Select(x => new MediationCasePerson()
            {
                CourtId = courtId,
                CaseId = caseId,
                CasePersonId = x.Id,
                UserId = userContext.UserId,
                DateWrt = DateTime.Now,
            })
                             .ToList();
        }

        /// <summary>
        /// Метод вземащ продължителността на срещата
        /// </summary>
        /// <param name="dateFrom">От дата</param>
        /// <param name="dateTo">До дата</param>
        /// <returns></returns>
        private int GetHoursBetweenDateFromTo(DateTime dateFrom, DateTime dateTo)
        {
            TimeSpan timeSpan = dateTo.Subtract(dateFrom);
            return timeSpan.Hours + (timeSpan.Minutes >= 15 ? 1 : 0);
        }

        /// <summary>
        /// Метод извличащ идентификаторите и имената на медиаторите към начална дата на среща за медиация
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="dateFrom">Начална дата на среща</param>
        /// <returns></returns>
        private async Task<List<SelectListItem>> GetMediatorIds(int caseId, DateTime dateFrom)
        {
            return await repo.AllReadonly<MediationCaseMediator>()
                             .Where(x => x.DateExpired == null)
                             .Where(GetMediatorsDateWhere(dateFrom))
                             .Where(x => x.CaseId == caseId)
                             .Select(x => new SelectListItem
                             {
                                 Value = x.MediationMediatorId.ToString(),
                                 Text = x.Mediator.Name
                             })
                             .ToListAsync();
        }

        /// <summary>
        /// Метод връщащ сума за медиатор след провеждане на среща за медиация
        /// </summary>
        /// <param name="mediationTypeId">Идентификатор на вид среща за медиация</param>
        /// <param name="dateFrom">Начален час и дата на среща</param>
        /// <param name="dateTo">Краен час и дата на среща</param>
        /// <returns></returns>
        private async Task<(int id, decimal amount)> GetSessionMediatorAmount(int mediationTypeId, DateTime dateFrom, DateTime dateTo)
        {
            int hoursSession = GetHoursBetweenDateFromTo(dateFrom, dateTo);

            MediatorFee? hourFee = await repo.AllReadonly<MediatorFee>()
                                            .Where(x => x.DateFrom <= dateFrom)
                                            .Where(x => (x.DateTo ?? dateFrom) >= dateFrom)
                                            .Where(x => x.MediationTypeId == mediationTypeId)
                                            .FirstOrDefaultAsync();

            if (hourFee == null)
                return (0, 0);
            else
                return (hourFee.Id, (hoursSession * hourFee.HourFeeEUR));
        }

        /// <summary>
        /// Метод връщащ списък със възнаграждение на медиатор към съдебен център по медиация за участие в информационна среща
        /// </summary>
        /// <param name="model">Среща за медиация</param>
        /// <returns></returns>
        private async Task<List<Obligation>> GetObligations(MediationCaseSession model)
        {
            if (NomenclatureConstants.MediationStateConstants.StateForEdit.Contains(model.MediationStateId ?? 0))
                return [];

            List<SelectListItem> mediators = await GetMediatorIds(model.CaseId, model.DateFrom);

            if (mediators.Count() < 1)
                return [];

            var amountMediator = await GetSessionMediatorAmount(model.MediationTypeId, model.DateFrom, model.DateTo ?? DateTime.Now);

            if (amountMediator.id == 0)
                return [];

            return mediators.Select(x => new Obligation
                             {
                                 CourtId = model.CourtId ?? 0,
                                 CaseId = model.CaseId,
                                 ObligationInfo = "Възнаграждение на медиатор към съдебен център по медиация за участие в информационна среща",
                                 ObligationDescription = "Среща от: " + model.DateFrom.ToString("dd.MM.yyyy HH:mm") + " до: " + (model.DateTo ?? DateTime.Now).ToString("dd.MM.yyyy HH:mm"),
                                 ObligationDate = DateTime.Now,
                                 MoneyTypeId = NomenclatureConstants.MoneyType.Mediation,
                                 Amount = amountMediator.amount,
                                 AmountBGN = Math.Round(amountMediator.amount * (decimal)1.95583, 2, MidpointRounding.AwayFromZero),
                                 Description = "Възнаграждение на медиатор към съдебен център по медиация за участие в информационна среща",
                                 IsActive = true,
                                 MediationCaseSessionId = model.Id,
                                 MediationMediatorId = int.Parse(x.Value),
                                 MediatorFeeId = amountMediator.id,
                                 MoneySign = -1,
                                 FullName = x.Text,
                                 UicTypeId = 1,
                                 UserId = userContext.UserId,
                                 DateWrt = DateTime.Now
                             })
                            .ToList();
        }

        /// <summary>
        /// Добавяне/редкация на данни за среща към дело
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<int?> SaveMediationCaseSession(MediationCaseSessionVM model)
        {
            try
            {
                MediationCaseSession modelSave = (model.Id > 0) ? await repo.All<MediationCaseSession>()
                                                                             .Where(x => x.Id == model.Id)
                                                                             .FirstAsync() : await FillMediationCaseSession(model);

                if (model.Id > 0)
                {
                    repo.DeleteRange<MediationCaseSessionLinkCase>(x => x.MediationCaseSessionId == model.Id);
                    SetEditFieldsMediationCaseSession(model, modelSave);
                    
                    if (model.LinkCaseIds != null && model.LinkCaseIds.Length > 0)
                        repo.AddRange(model.LinkCaseIds.Select(x => new MediationCaseSessionLinkCase() { MediationCaseSessionId = model.Id, LinkCaseId = int.Parse(x) }).ToList());
                }
                else
                {
                    if (model.LinkCaseIds != null && model.LinkCaseIds.Length > 0)
                        modelSave.LinkCases = model.LinkCaseIds.Select(x => new MediationCaseSessionLinkCase() { LinkCaseId = int.Parse(x) }).ToList();

                    modelSave.People = await GetMediationCasePeople(modelSave.CourtId, modelSave.CaseId, modelSave.DateFrom);
                    repo.Add(modelSave);
                }
                
                await repo.SaveChangesAsync();

                if (!NomenclatureConstants.MediationStateConstants.StateForEdit.Contains(model.MediationStateId ?? 0))
                {
                    List<Obligation> obligations = await GetObligations(modelSave);
                    if (obligations != null && obligations.Count > 0)
                    {
                        repo.AddRange(obligations);
                        await repo.SaveChangesAsync();
                    }
                }

                return modelSave.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на среща към дело CaseId = {model.CaseId} и id = {model.Id}");
                return null;
            }
        }

        /// <summary>
        /// Извличане на данни за среща за медиация
        /// </summary>
        /// <param name="id">Идентификатор на среща</param>
        /// <returns></returns>
        public async Task<MediationSessionPreviewVM> GetMediationSessionPreview(int id)
        {
            DateTime dateTime = DateTime.Now;

            return await repo.AllReadonly<MediationCaseSession>()
                             .Where(x => x.Id == id)
                             .Select(x => new MediationSessionPreviewVM
                             {
                                 Id = x.Id,
                                 CaseId = x.CaseId,
                                 CaseLabel = x.Case.CaseType.Code + " " + x.Case.ShortNumber + "/" + x.Case.RegDate.ToString("yyyy"),
                                 MediationTypeLabel = x.MediationType.Label,
                                 MediationLocationLabel = x.MediationLocation.Label,
                                 MediationLocationDescription = x.MediationLocationDescription,
                                 SessionDateText = $"От: {x.DateFrom.ToString("dd.MM.yyyy HH:mm:ss")} до: {(x.DateTo ?? dateTime).ToString("dd.MM.yyyy HH:mm:ss")}",
                                 MediationStateLabel = x.MediationState.Label,
                                 IsEdit = NomenclatureConstants.MediationStateConstants.StateForEdit.Contains(x.MediationStateId ?? 0),
                                 Mediators = string.Join(Environment.NewLine, x.Case
                                                                               .MediationCaseMediators
                                                                               .Where(m => m.DateFrom <= dateTime)
                                                                               .Where(m => (m.DateTo ?? dateTime) >= dateTime)
                                                                               .Select(m => m.Mediator.Name + (!string.IsNullOrEmpty(m.MediationTypeChoiceMediator.Label) ? " (" + m.MediationTypeChoiceMediator.Label + ")" : string.Empty))),
                                 LinkCases = string.Join(", ", x.LinkCases.Select(x => x.Case.RegNumber + " / " + x.Case.RegDate.Year.ToString() + "г."))
                             })
                             .FirstAsync();
        }

        /// <summary>
        /// Метод връщащ where клауза за ид на среща за медиация
        /// </summary>
        /// <param name="mediationSessionId">Идентификатор на среща за медиация</param>
        /// <returns></returns>
        private Expression<Func<MediationCaseSession, bool>> GetMediationSessionIdWhere(int? mediationSessionId)
        {
            Expression<Func<MediationCaseSession, bool>> mediationSessionIdWhere = x => true;
            if ((mediationSessionId ?? 0) > 0)
                mediationSessionIdWhere = x => x.Id != mediationSessionId;

            return mediationSessionIdWhere;
        }

        /// <summary>
        /// Метод проверяващ за съществуване на насрочена среща
        /// </summary>
        /// <param name="dateFrom">От дата на нова среща</param>
        /// <param name="dateTo">До дата на нова среща</param>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="mediationSessionId">Идентификатор на среща</param>
        /// <returns></returns>
        public async Task<bool> IsExsitMediationSession(DateTime dateFrom, DateTime dateTo, int caseId, int? mediationSessionId)
        {
            return await repo.AllReadonly<MediationCaseSession>()
                             .Where(GetMediationSessionIdWhere(mediationSessionId))
                             .AnyAsync(x => x.DateExpired == null &&
                                            x.CaseId == caseId &&
                                            dateTo >= x.DateFrom &&
                                            dateFrom <= x.DateTo &&
                                            x.MediationStateId == NomenclatureConstants.MediationStateConstants.Scheduled);
        }

        #endregion

        #region Лица в среща

        /// <summary>
        /// Кюери за извличане на данни за лица в среща
        /// </summary>
        /// <param name="filter">>Филтър попълнен от потребител</param>
        /// <returns></returns>
        private IQueryable<MediationCasePerson> GetMediationCasePeople(MediationCasePersonFilterVM filter)
        {
            return repo.AllReadonly<MediationCasePerson>()
                       .Where(x => x.MediationCaseSessionId == filter.MediationCaseSessionId)
                       .Where(x => x.DateExpired == null);
        }

        /// <summary>
        /// Метод извличащ данни за лица към среща
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <param name="start">От коя позиция да дръпне данните</param>
        /// <param name="length">Дължина</param>
        /// <param name="sortedColumns">Колони по които се сортира</param>
        /// <returns></returns>
        public async Task<DataTableResponseVM<MediationCasePersonListDataVM>> GetMediationCasePeople(MediationCasePersonFilterVM filter, int start, int length, List<DataTablesSortColumnVM> sortedColumns)
        {
            DataTableResponseVM<MediationCasePersonListDataVM> result = new();

            IQueryable<MediationCasePerson> casePeopleQuery = GetMediationCasePeople(filter);

            result.TotalCount = await casePeopleQuery.CountAsync();

            List<MediationCasePersonListDataVM> people = await casePeopleQuery.Select(x => new MediationCasePersonListDataVM
                                                                                           {
                                                                                              Id = x.Id,
                                                                                              CasePersonId = x.CasePersonId,
                                                                                              CasePersonFullName = x.Person.FullName,
                                                                                              CasePersonRoleName = x.Person.PersonRole.Label,
                                                                                              CasePersonDateFrom = x.Person.DateFrom,
                                                                                              CasePersonDateTo = x.Person.DateTo,
                                                                                              MediationPersonSessionStateLabel = x.State.Label
                                                                                           })
                                                                              .OrderBy(sortedColumns)
                                                                              .Skip(start)
                                                                              .Take(length)
                                                                              .ToListAsync();

            foreach (MediationCasePersonListDataVM person in people)
            {
                var linkListVM = casePersonLinkService.GetLinkForPerson(person.CasePersonId, false, 0, null);
                if (linkListVM != null && linkListVM.Count() > 0)
                    person.CasePersonLinkForPerson = string.Join(", ", linkListVM.Select(x => x.Label));
            }

            result.Records = people;
            return result;
        }

        /// <summary>
        /// Извличане на данни за лице от среща за медиация
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<MediationCasePersonVM> GetMediationCasePersonEditById(int id)
        {
            return await repo.AllReadonly<MediationCasePerson>()
                             .Where(x => x.Id == id)
                             .Select(x => new MediationCasePersonVM
                             {
                                 Id = x.Id,
                                 CaseId = x.CaseId,
                                 MediationCaseSessionId = x.MediationCaseSessionId,
                                 CasePersonDataLabel = $"{x.Person.FullName} ({x.Person.PersonRole.Label})",
                                 CasePersonAddress = string.Join(",", x.Person.Addresses.Select(a => a.Address.FullAddress)),
                                 MediationPersonSessionStateId = x.MediationPersonSessionStateId,
                                 Description = x.Description,
                             })
                             .FirstAsync();
        }

        /// <summary>
        /// Попълване на данни за редакция на лице от среща за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <param name="modelSave">Модел за редакция</param>
        private static void SetEditFieldsMediationCasePerson(MediationCasePersonVM model, MediationCasePerson modelSave)
        {
            modelSave.MediationPersonSessionStateId = model.MediationPersonSessionStateId.NumberEmptyToNull();
            modelSave.Description = model.Description;
        }

        /// <summary>
        /// Редкация на данни за лице от среща за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<int?> SaveMediationCasePerson(MediationCasePersonVM model)
        {
            try
            {
                MediationCasePerson modelSave = await repo.All<MediationCasePerson>()
                                                          .Where(x => x.Id == model.Id)
                                                          .FirstAsync();

                SetEditFieldsMediationCasePerson(model, modelSave);
                await repo.SaveChangesAsync();
                return modelSave.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на лице от среща за медиация CaseId = {model.CaseId} и id = {model.Id}");
                return null;
            }
        }

        /// <summary>
        /// Метод извличащ данни за страни от дело в среща
        /// </summary>
        /// <param name="mediationCaseSessionId">Идентификатор на среща за медиация</param>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент "Избери"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_MediationCasePerson(int mediationCaseSessionId, bool addDefaultElement = true)
        {
            List<SelectListItem> selectListItems = await repo.AllReadonly<MediationCasePerson>()
                                                             .Where(x => x.MediationCaseSessionId == mediationCaseSessionId)
                                                             .Where(x => x.DateExpired == null)
                                                             .Select(x => new SelectListItem
                                                             {
                                                                 Text = x.Person.FullName,
                                                                 Value = x.Id.ToString()
                                                             })
                                                             .ToListAsync();

            if (selectListItems.Count != 1)
            {
                if (addDefaultElement)
                {
                    selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                                                     .ToList();
                }
            }

            return selectListItems;
        }

        #endregion

        #region Резултати в среща за медиация

        /// <summary>
        /// Метод извличащ данни за резултати в среща за медиация
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<MediationCaseSessionResultListDataVM> GetMediationCaseSessionResults(MediationCaseSessionResultFilterVM filter)
        {
            return repo.AllReadonly<MediationCaseSessionResult>()
                       .Where(x => x.MediationCaseSessionId == filter.MediationCaseSessionId)
                       .Where(x => x.DateExpired == null)
                       .Select(x => new MediationCaseSessionResultListDataVM()
                       {
                           Id = x.Id,
                           MediationResultLabel = x.Result.Label,
                           MediationResultBaseLabel = x.ResultBase.Label,
                           IsMainText = x.IsMain ? MessageConstant.Yes : MessageConstant.No
                       });
        }

        /// <summary>
        /// Извличане на данни за резултат в среща за медиация
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<MediationCaseSessionResultVM> GetMediationCaseSessionResultEditById(int id)
        {
            return await repo.AllReadonly<MediationCaseSessionResult>()
                             .Where(x => x.Id == id)
                             .Select(x => new MediationCaseSessionResultVM()
                             {
                                 Id = x.Id,
                                 CaseId = x.CaseId,
                                 CourtId = x.CourtId ?? 0,
                                 MediationCaseSessionId = x.MediationCaseSessionId,
                                 MediationResultId = x.MediationResultId,
                                 MediationResultBaseId = x.MediationResultBaseId,
                                 IsMain = x.IsMain,
                                 Description = x.Description
                             })
                             .FirstAsync();
        }

        /// <summary>
        /// Попълване на обект за добавяне на резултат в среща за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private MediationCaseSessionResult FillMediationCaseSessionResult(MediationCaseSessionResultVM model)
        {
            return new()
            {
                CourtId = model.CourtId,
                CaseId = model.CaseId,
                MediationCaseSessionId = model.MediationCaseSessionId,
                MediationResultId = model.MediationResultId,
                MediationResultBaseId = model.MediationResultBaseId.NumberEmptyToNull(),
                IsMain = model.IsMain,
                Description = model.Description,
                UserId = userContext.UserId,
                DateWrt = DateTime.Now,
            };
        }

        /// <summary>
        /// Попълване на данни за редакция на резултат в среща за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <param name="modelSave">Модел за редакция</param>
        private static void SetEditFieldsMediationCaseSessionResult(MediationCaseSessionResultVM model, MediationCaseSessionResult modelSave)
        {
            modelSave.MediationResultId = model.MediationResultId;
            modelSave.MediationResultBaseId = model.MediationResultBaseId.NumberEmptyToNull();
            modelSave.IsMain = model.IsMain;
            modelSave.Description = model.Description;
        }

        /// <summary>
        /// Добавяне/редкация на данни за резултат в среща за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<int?> SaveMediationCaseSessionResult(MediationCaseSessionResultVM model)
        {
            try
            {
                MediationCaseSessionResult modelSave = (model.Id > 0) ? await repo.All<MediationCaseSessionResult>()
                                                                                  .Where(x => x.Id == model.Id)
                                                                                  .FirstAsync() : FillMediationCaseSessionResult(model);

                if (model.Id > 0)
                    SetEditFieldsMediationCaseSessionResult(model, modelSave);
                else
                    repo.Add(modelSave);

                await repo.SaveChangesAsync();
                return modelSave.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на резултат в среща за медиация CaseId = {model.CaseId} и id = {model.Id}");
                return null;
            }
        }

        #endregion

        #region Документи към среща за медиация

        /// <summary>
        /// Метод извличащ данни за документи в среща за медиация
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<MediationCaseSessionDocumentListDataVM> GetMediationCaseSessionDocuments(MediationCaseSessionDocumentFilterVM filter)
        {
            return repo.AllReadonly<MediationCaseSessionDocument>()
                       .Where(x => x.MediationCaseSessionId == filter.MediationCaseSessionId)
                       .Where(x => x.DateExpired == null)
                       .Select(x => new MediationCaseSessionDocumentListDataVM()
                       {
                           Id = x.Id,
                           DateUpload = x.DateUpload,
                           Description = x.Description,
                       });
        }

        /// <summary>
        /// Извличане на данни за документ в среща за медиация
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<MediationCaseSessionDocumentVM> GetMediationCaseSessionDocumentEditById(int id)
        {
            return await repo.AllReadonly<MediationCaseSessionDocument>()
                             .Where(x => x.Id == id)
                             .Select(x => new MediationCaseSessionDocumentVM()
                             {
                                 Id = x.Id,
                                 CaseId = x.CaseId,
                                 CourtId = x.CourtId ?? 0,
                                 MediationCaseSessionId = x.MediationCaseSessionId,
                                 DateUpload = x.DateUpload,
                                 Description = x.Description,
                             })
                             .FirstAsync();
        }

        /// <summary>
        /// Попълване на обект за добавяне на документ в среща за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private MediationCaseSessionDocument FillMediationCaseSessionDocument(MediationCaseSessionDocumentVM model)
        {
            return new()
            {
                CourtId = model.CourtId,
                CaseId = model.CaseId,
                MediationCaseSessionId = model.MediationCaseSessionId,
                DateUpload = model.DateUpload,
                Description = model.Description,
                UserId = userContext.UserId,
                DateWrt = DateTime.Now,
            };
        }

        /// <summary>
        /// Попълване на данни за редакция на документ в среща за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <param name="modelSave">Модел за редакция</param>
        private static void SetEditFieldsMediationCaseSessionDocument(MediationCaseSessionDocumentVM model, MediationCaseSessionDocument modelSave)
        {
            modelSave.DateUpload = model.DateUpload;
            modelSave.Description = model.Description;
        }

        /// <summary>
        /// Добавяне/редкация на данни за документ в среща за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<int?> SaveMediationCaseSessionDocument(MediationCaseSessionDocumentVM model)
        {
            try
            {
                MediationCaseSessionDocument modelSave = (model.Id > 0) ? await repo.All<MediationCaseSessionDocument>()
                                                                                  .Where(x => x.Id == model.Id)
                                                                                  .FirstAsync() : FillMediationCaseSessionDocument(model);

                if (model.Id > 0)
                    SetEditFieldsMediationCaseSessionDocument(model, modelSave);
                else
                    repo.Add(modelSave);


                await repo.SaveChangesAsync();
                return modelSave.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на документ в среща за медиация CaseId = {model.CaseId} и id = {model.Id}");
                return null;
            }
        }

        #endregion

        #region Оценка

        /// <summary>
        /// Метод извличащ данни за оценка към дело
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<MediationCaseMediatorAppraisalListDataVM> GetMediationCaseMediatorAppraisals(MediationCaseMediatorAppraisalFilterVM filter)
        {
            return repo.AllReadonly<MediationCaseMediatorAppraisal>()
                       .Where(x => x.MediationCaseSessionId == filter.MediationCaseSessionId)
                       .Where(x => x.DateExpired == null)
                       .Select(x => new MediationCaseMediatorAppraisalListDataVM()
                       {
                           Id = x.Id,
                           MediationCaseMediatorFullName = x.CaseMediator.Mediator.Name,
                           DateAppraisal = x.DateAppraisal,
                           Description = x.Description,
                           MediationCasePersonFullName = x.CasePerson.Person.FullName,
                           SumAppraisal = x.TypeAppraisal == NomenclatureConstants.MediationCaseMediatorAppraisalTypeConstants.Detailed ? x.Data.Sum(d => d.Rating) : x.Data.Sum(d => d.Rating * (d.PointAppraisal.MultiplicationValue ?? 0)),
                           TypeAppraisal = x.TypeAppraisal ?? NomenclatureConstants.MediationCaseMediatorAppraisalTypeConstants.Detailed
                       });
        }

        /// <summary>
        /// Извличане на данни за оценка в среща за медиация
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<MediationCaseMediatorAppraisalVM> GetMediationCaseMediatorAppraisalEditById(int id)
        {
            return await repo.AllReadonly<MediationCaseMediatorAppraisal>()
                             .Where(x => x.Id == id)
                             .Select(x => new MediationCaseMediatorAppraisalVM()
                             {
                                 Id = x.Id,
                                 CaseId = x.CaseId,
                                 CourtId = x.CourtId ?? 0,
                                 MediationCaseSessionId = x.MediationCaseSessionId,
                                 MediationCaseMediatorId = x.MediationCaseMediatorId,
                                 DateAppraisal = x.DateAppraisal,
                                 TypeAppraisal = x.TypeAppraisal ?? NomenclatureConstants.MediationCaseMediatorAppraisalTypeConstants.Detailed,
                                 Appraisals = x.Data.OrderBy(a => a.PointAppraisal.OrderNumber)
                                                    .Select(a => new RatingVM
                                                    {
                                                        Id = a.MediationPointMediatorAppraisalId,
                                                        Label = a.PointAppraisal.Label,
                                                        IsViewValue = !a.PointAppraisal.WithoutAppraisal,
                                                        Value = a.Rating,
                                                        IsSmall = a.PointAppraisal.TypePoint == NomenclatureConstants.MediationCaseMediatorAppraisalTypeConstants.Summary ? true : false,
                                                    })
                                                    .ToList(),
                                 MediationCasePersonId = x.MediationCasePersonId,
                                 Description = x.Description
                             })
                             .FirstAsync();
        }

        /// <summary>
        /// Попълване на обект за добавяне на оценка в среща за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private MediationCaseMediatorAppraisal FillMediationCaseMediatorAppraisal(MediationCaseMediatorAppraisalVM model)
        {
            return new()
            {
                CourtId = model.CourtId,
                CaseId = model.CaseId,
                MediationCaseSessionId = model.MediationCaseSessionId,
                MediationCaseMediatorId = model.MediationCaseMediatorId,
                DateAppraisal = model.DateAppraisal,
                MediationCasePersonId = model.MediationCasePersonId.NumberEmptyToNull(),
                Description = model.Description,
                UserId = userContext.UserId,
                DateWrt = DateTime.Now,
                TypeAppraisal = model.TypeAppraisal
            };
        }

        /// <summary>
        /// Попълване на данни за редакция на оценка в среща за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <param name="modelSave">Модел за редакция</param>
        private static void SetEditFieldsMediationCaseMediatorAppraisal(MediationCaseMediatorAppraisalVM model, MediationCaseMediatorAppraisal modelSave)
        {
            modelSave.MediationCaseMediatorId = model.MediationCaseMediatorId;
            modelSave.MediationCasePersonId = model.MediationCasePersonId.NumberEmptyToNull();
            modelSave.DateAppraisal = model.DateAppraisal;
            modelSave.Description = model.Description;
        }

        /// <summary>
        /// Добавяне/редкация на данни за оценка в среща за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<int?> SaveMediationCaseMediatorAppraisal(MediationCaseMediatorAppraisalVM model)
        {
            try
            {
                MediationCaseMediatorAppraisal modelSave = (model.Id > 0) ? await repo.All<MediationCaseMediatorAppraisal>()
                                                                                  .Where(x => x.Id == model.Id)
                                                                                  .FirstAsync() : FillMediationCaseMediatorAppraisal(model);

                if (model.Id > 0)
                {
                    repo.DeleteRange<MediationCaseMediatorAppraisalData>(x => x.MediationCaseMediatorAppraisalId == model.Id);
                    SetEditFieldsMediationCaseMediatorAppraisal(model, modelSave);

                    repo.AddRange(model.Appraisals.Select(x => new MediationCaseMediatorAppraisalData
                    {
                        MediationCaseMediatorAppraisalId = model.Id,
                        MediationPointMediatorAppraisalId = x.Id,
                        Rating = x.Value,
                    }));
                }
                else
                {
                    modelSave.Data = model.Appraisals.Select(x => new MediationCaseMediatorAppraisalData
                                                     {
                                                         MediationCaseMediatorAppraisalId = model.Id,
                                                         MediationPointMediatorAppraisalId = x.Id,
                                                         Rating = x.Value,
                                                     })
                                                     .ToList();

                    repo.Add(modelSave);
                }

                await repo.SaveChangesAsync();
                return modelSave.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на оценка в среща за медиация CaseId = {model.CaseId} и id = {model.Id}");
                return null;
            }
        }

        /// <summary>
        /// Метод за проверка на медиатор в среща дали има оценка
        /// </summary>
        /// <param name="mediatorId">Идентификатор на медиатор</param>
        /// <param name="mediationCaseSessionId">Идентификатор на среща</param>
        /// <param name="appraisalId">Идентификатор на текущ запис</param>
        /// <returns></returns>
        public async Task<bool> IsExistAppraisalMediator(int mediatorId, int mediationCaseSessionId, int? appraisalId)
        {
            Expression<Func<MediationCaseMediatorAppraisal, bool>> mediatorIdWhere = x => x.MediationCaseMediatorId == mediatorId;
            Expression<Func<MediationCaseMediatorAppraisal, bool>> mediationCaseSessionIdWhere = x => x.MediationCaseSessionId == mediationCaseSessionId;

            Expression<Func<MediationCaseMediatorAppraisal, bool>> mediatoappraisalIdWhere = x => true;
            if ((appraisalId ?? 0) > 0)
                mediatoappraisalIdWhere = x => x.Id != appraisalId;

            return await repo.AllReadonly<MediationCaseMediatorAppraisal>()
                             .Where(mediatorIdWhere)
                             .Where(mediationCaseSessionIdWhere)
                             .Where(mediatoappraisalIdWhere)
                             .Where(x => x.DateExpired == null)
                             .AnyAsync();
        }

        #endregion

        #region Заплащане на медиатори

        /// <summary>
        /// Метод извличащ данни за суми на заседатели
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<MediationObligationListDataVM> GetMediationObligations(MediationObligationFilterVM filter)
        {
            return repo.AllReadonly<Obligation>()
                       .Where(x => x.MediationCaseSessionId == filter.MediationCaseSessionId)
                       .Select(x => new MediationObligationListDataVM()
                       {
                           Id = x.Id,
                           MediationMediatorFullName = x.Mediator.Name,
                           Amount = x.Amount.ToString("0.00"),
                           RegNumberExpenseOrder = x.ExpenseOrderObligations.Where(e => e.ExpenseOrder.IsActive).Select(e => e.ExpenseOrder.RegNumber ?? string.Empty).FirstOrDefault(),
                           ExpenseOrderId = x.ExpenseOrderObligations.Where(o => o.ExpenseOrder.IsActive == true).Select(o => o.ExpenseOrderId).FirstOrDefault()
                       });
        }

        /// <summary>
        /// Извличане на данни за сума на заседател за редакция
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<MediationObligationVM> GetMediationObligationEditById(int id)
        {
            return await repo.AllReadonly<Obligation>()
                             .Where(x => x.Id == id)
                             .Select(x => new MediationObligationVM()
                             {
                                 Id = x.Id,
                                 CaseId = x.CaseId ?? 0,
                                 CourtId = x.CourtId,
                                 MediationCaseSessionId = x.MediationCaseSessionId ?? 0,
                                 Amount = x.Amount
                             })
                             .FirstAsync();
        }

        /// <summary>
        /// Редкация на данни за сума на заседател към среща
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<int?> SaveMediationObligation(MediationObligationVM model)
        {
            try
            {
                Obligation modelSave = await repo.All<Obligation>()
                                                 .Where(x => x.Id == model.Id)
                                                 .FirstAsync();

                if (model.Id > 0)
                    modelSave.Amount = model.Amount;

                await repo.SaveChangesAsync();
                return modelSave.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на id = {model.Id}");
                return null;
            }
        }

        #endregion
    }
}
