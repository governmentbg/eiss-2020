// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class CaseArchiveService : BaseService, ICaseArchiveService
    {
        private readonly ICounterService counterService;

        public CaseArchiveService(
            ILogger<CaseArchiveService> _logger,
            ICounterService _counterService,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            counterService = _counterService;
        }

        /// <summary>
        /// Извличане на данни за дела за архивиране
        /// </summary>
        /// <param name="courtId"></param>
        /// <returns></returns>
        public IQueryable<CaseForArchiveVM> CaseForArchive_Select(int courtId)
        {
            int[] caseStateForArchive = { NomenclatureConstants.CaseState.Stop, NomenclatureConstants.CaseState.Suspend,
                                          NomenclatureConstants.CaseState.Resolution, NomenclatureConstants.CaseState.ComingIntoForce};

            return repo.AllReadonly<Case>()
                .Where(x => x.CourtId == courtId)
                .Where(x => caseStateForArchive.Contains(x.CaseStateId))
                .Where(x => x.CaseArchives.Any() == false)
                .Select(x => new CaseForArchiveVM()
                {
                    Id = x.Id,
                    CaseTypeLabel = x.CaseType.Code,
                    CaseStateLabel = x.CaseState.Label,
                    RegNumber = x.RegNumber,
                    ShortRegNumber = x.ShortNumber,
                    RegDate = x.RegDate
                })
                .AsQueryable();
        }

        /// <summary>
        /// Извличане на данни за дела в архив
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseArchiveListVM> CaseArchive_Select(int courtId, CaseArchiveFilterVM model)
        {
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<CaseArchive, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate.Date >= dateFromSearch.Date && x.RegDate.Date <= dateToSearch.Date;

            Expression<Func<CaseArchive, bool>> regNumberSearch = x => true;
            if (string.IsNullOrEmpty(model.RegNumber) == false)
                regNumberSearch = x => x.Case.RegNumber.ToLower().Contains(model.RegNumber.ToLower());

            Expression<Func<CaseArchive, bool>> isDestroySearch = x => true;
            if (model.IsDestroy == true)
                isDestroySearch = x => x.DateDestroy != null;

            return repo.AllReadonly<CaseArchive>()
                .Where(x => x.Case.CourtId == courtId)
                .Where(dateSearch)
                .Where(regNumberSearch)
                .Where(isDestroySearch)
                .Select(x => new CaseArchiveListVM()
                {
                    Id = x.Id,
                    CaseId = x.Case.Id,
                    CaseTypeLabel = x.Case.CaseType.Code,
                    CaseStateLabel = x.Case.CaseState.Label,
                    CaseRegNumber = x.Case.RegNumber,
                    CaseRegDate = x.Case.RegDate,
                    RegNumber = x.RegNumber,
                    RegDate = x.RegDate,
                    BookNumber = x.BookNumber,
                    BookYear = x.BookYear,
                    StorageYears = x.StorageYears,
                    CaseArchiveIndexName = x.CourtArchiveIndex.Label,
                    DateDestroy = x.DateDestroy
                })
                .AsQueryable();
        }

        /// <summary>
        /// Запис на дело в архив
        /// </summary>
        /// <param name="model"></param>
        /// <param name="errorMessage"></param>
        /// <param name="forDestroy"></param>
        /// <returns></returns>
        public bool CaseArchive_SaveData(CaseArchive model, ref string errorMessage, bool forDestroy)
        {
            try
            {
                if (forDestroy == false)
                {
                    model.ActDestroyLabel = null;
                    model.Description = null;
                    model.DescriptionInfoDestroy = null;
                }

                if (model.Id == 0)
                {
                    var existsArchive = repo.AllReadonly<CaseArchive>().Where(x => x.CaseId == model.CaseId).Any();
                    if (existsArchive == true)
                    {
                        errorMessage = "Делото вече е вкарано в архив";
                        return false;
                    }
                }

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseArchive>(model.Id);

                    if (forDestroy == false)
                    {
                        saved.CaseSessionActId = model.CaseSessionActId;
                        saved.ArchiveIndexId = model.ArchiveIndexId;
                        saved.ArchiveLink = model.ArchiveLink;
                        saved.DescriptionInfo = model.DescriptionInfo;
                        saved.StorageYears = model.StorageYears;
                    }
                    else
                    {
                        if (saved.DateDestroy == null)
                            saved.DateDestroy = DateTime.Now;
                        saved.ActDestroyLabel = model.ActDestroyLabel;
                        saved.Description = model.Description;
                        saved.DescriptionInfoDestroy = model.DescriptionInfoDestroy;
                    }

                    //тези се редактират винаги
                    saved.BookNumber = model.BookNumber;
                    saved.BookYear = model.BookYear;

                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;
                    repo.Update(saved);

                    model.DateDestroy = saved.DateDestroy;
                }
                else
                {
                    //Insert
                    if (model.IsOldNumber == true)
                    {

                    }
                    else
                    {
                        if (counterService.Counter_GetCaseArchiveCounter(model) == false)
                        {
                            errorMessage = "Проблем при вземане на номер за архив";
                            return false;
                        }
                    }
                    model.DateDestroy = null;
                    model.UserId = userContext.UserId;
                    model.DateWrt = DateTime.Now;
                    repo.Add<CaseArchive>(model);
                }

                //когато се записва архива да смени статуса на делото на Архивирано/Унищожено
                var caseModel = GetById<Case>(model.CaseId);
                if (model.DateDestroy == null)
                    caseModel.CaseStateId = NomenclatureConstants.CaseState.Archive;
                else
                    caseModel.CaseStateId = NomenclatureConstants.CaseState.Destroy;
                repo.Update(caseModel);

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CaseArchive Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за архивирано дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public CaseArchive CaseArchiveByCaseId_Select(int caseId)
        {
            return repo.AllReadonly<CaseArchive>().Where(x => x.CaseId == caseId).FirstOrDefault();
        }

        /// <summary>
        /// Извличане на данни за дела за унищожаване
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseArchiveListVM> CaseForDestroy_Select(int courtId, CaseForDestroyFilterVM model)
        {
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<CaseArchive, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate.AddYears(x.StorageYears).Date >= dateFromSearch.Date && x.RegDate.AddYears(x.StorageYears).Date <= dateToSearch.Date;

            Expression<Func<CaseArchive, bool>> regNumberSearch = x => true;
            if (string.IsNullOrEmpty(model.CaseRegNumber) == false)
                regNumberSearch = x => x.Case.RegNumber.Contains(model.CaseRegNumber);

            int[] caseStateForDestroy = { NomenclatureConstants.CaseState.Archive};

            return repo.AllReadonly<CaseArchive>()
                .Where(x => x.Case.CourtId == courtId)
                .Where(x => caseStateForDestroy.Contains(x.Case.CaseStateId))
                .Where(x => x.DateDestroy == null)
                .Where(x => x.RegDate.AddYears(x.StorageYears).Date < DateTime.Now.Date)
                .Where(dateSearch)
                .Where(regNumberSearch)
                .Select(x => new CaseArchiveListVM()
                {
                    Id = x.Id,
                    CaseId = x.Case.Id,
                    CaseTypeLabel = x.Case.CaseType.Code,
                    CaseStateLabel = x.Case.CaseState.Label,
                    CaseRegNumber = x.Case.RegNumber,
                    CaseRegDate = x.Case.RegDate,
                    RegNumber = x.RegNumber,
                    RegDate = x.RegDate,
                    BookNumber = x.BookNumber,
                    BookYear = x.BookYear,
                    StorageYears = x.StorageYears,
                    CaseArchiveIndexName = x.CourtArchiveIndex.Label
                })
                .AsQueryable();
        }
    }
}
