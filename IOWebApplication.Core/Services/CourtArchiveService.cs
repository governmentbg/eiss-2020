// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class CourtArchiveService : BaseService, ICourtArchiveService
    {
        public CourtArchiveService(ILogger<CourtArchiveService> _logger,
            IRepository _repo)
        {
            logger = _logger;
            repo = _repo;
        }

        public IQueryable<CourtArchiveCommittee> CourtArchiveCommittee_Select(int courtId)
        {
            return repo.AllReadonly<CourtArchiveCommittee>()
           .Where(x => x.CourtId == courtId)
           .AsQueryable();
        }

        public bool CourtArchiveCommittee_SaveData(CourtArchiveCommittee model, List<int> lawUnitIds)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CourtArchiveCommittee>(model.Id);
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.DateStart = model.DateStart;
                    saved.DateEnd = model.DateEnd;
                    repo.Update(saved);
                }
                else
                {
                    //Insert
                    repo.Add<CourtArchiveCommittee>(model);
                }

                DateTime fromDate = DateTime.Now;
                DateTime toDate = DateTime.Now.AddSeconds(-1);
                var lawUnitList = repo.All<CourtArchiveCommitteeLawUnit>().Where(x => x.CourtArchiveCommitteeId == model.Id).ToList();

                lawUnitList.ForEach(x => x.DateTo = toDate);
                foreach (var itemId in lawUnitIds)
                {
                    var units = lawUnitList.Where(x => x.LawUnitId == itemId).FirstOrDefault();

                    if (units != null)
                    {
                        units.DateTo = null;
                    }
                    else
                    {
                        CourtArchiveCommitteeLawUnit newLawUnit = new CourtArchiveCommitteeLawUnit();
                        newLawUnit.CourtArchiveCommitteeId = model.Id;
                        newLawUnit.LawUnitId = itemId;
                        newLawUnit.DateFrom = fromDate;
                        repo.Add<CourtArchiveCommitteeLawUnit>(newLawUnit);
                    }
                }

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CourtArchiveCommittee Id={ model.Id }");
                return false;
            }
        }

        public IQueryable<MultiSelectTransferVM> CourtArchiveCommitteeLawUnit_Select(int committeeId)
        {
            DateTime dateTomorrow = DateTime.Now.AddDays(1).Date;

            return repo.AllReadonly<CourtArchiveCommitteeLawUnit>()
                .Include(x => x.LawUnit)
           .Where(x => x.CourtArchiveCommitteeId == committeeId &&
                       (x.DateTo ?? dateTomorrow) > DateTime.Now)
           .Select(x => new MultiSelectTransferVM()
           {
               Id = x.LawUnitId,
               Order = 0,
               Text = x.LawUnit.FullName
           }).AsQueryable();
        }

        public IQueryable<CourtArchiveIndexVM> CourtArchiveIndex_Select(int courtId)
        {
            return repo.AllReadonly<CourtArchiveIndex>()
                .Include(x => x.CourtArchiveCommitteeId)
                .Where(x => x.CourtId == courtId)
                .Select(x => new CourtArchiveIndexVM()
                {
                    Id = x.Id,
                    Code = x.Code,
                    Label = x.Label,
                    CourtArchiveCommitteeName = x.CourtArchiveCommittee.Label,
                    StorageYears = x.StorageYears,
                    DateStart = x.DateStart,
                    DateEnd = x.DateEnd
                }).AsQueryable();
        }

        public CourtArchiveIndexEditVM GetByIdVM(int id)
        {
            return repo.AllReadonly<CourtArchiveIndex>()
                .Where(x => x.Id == id)
                .Select(x => new CourtArchiveIndexEditVM()
                {
                    Id = x.Id,
                    CourtId = x.CourtId,
                    Code = x.Code,
                    Label = x.Label,
                    CourtArchiveCommitteeId = x.CourtArchiveCommitteeId,
                    StorageYears = x.StorageYears,
                    DateStart = x.DateStart,
                    DateEnd = x.DateEnd,
                    Description = x.Description
                }).FirstOrDefault();
        }

        public (bool result, string errorMessage) CourtArchiveIndex_SaveData(CourtArchiveIndexEditVM model, List<int> codeIds)
        {
            try
            {
                var existsCode = repo.AllReadonly<CourtArchiveIndex>().Where(x => x.CourtId == model.CourtId &&
                                                      x.Id != model.Id && x.Code.ToLower() == model.Code.ToLower()).Any();
                if (existsCode == true)
                    return (result: false, errorMessage: "Вече има въведен индекс този код");

                model.CourtArchiveCommitteeId = (model.CourtArchiveCommitteeId ?? 0) <= 0 ? null : model.CourtArchiveCommitteeId;
                CourtArchiveIndex saved = null;
                if (model.Id > 0)
                {
                    //Update
                    saved = repo.GetById<CourtArchiveIndex>(model.Id);
                }
                else
                {
                    //Insert
                    saved = new CourtArchiveIndex();
                    saved.IsActive = true;
                    saved.CourtId = model.CourtId;
                }

                saved.Label = model.Label;
                saved.Code = model.Code;
                saved.CourtArchiveCommitteeId = model.CourtArchiveCommitteeId;
                saved.StorageYears = model.StorageYears;
                saved.Description = model.Description;
                saved.DateStart = model.DateStart;
                saved.DateEnd = model.DateEnd;

                if (model.Id > 0)
                {
                    //Update
                    repo.Update(saved);
                }
                else
                {
                    //Insert
                    repo.Add<CourtArchiveIndex>(saved);
                }

                DateTime fromDate = DateTime.Now;
                DateTime toDate = DateTime.Now.AddSeconds(-1);
                var codeList = repo.All<CourtArchiveIndexCode>().Where(x => x.CourtArchiveIndexId == model.Id).ToList();
                var codeIdsDistinct = codeIds.Distinct().ToList();
                codeList.ForEach(x => x.DateTo = toDate);
                foreach (var itemId in codeIdsDistinct)
                {
                    var units = codeList.Where(x => x.CaseCodeId == itemId).FirstOrDefault();

                    if (units != null)
                    {
                        units.DateTo = null;
                    }
                    else
                    {
                        CourtArchiveIndexCode newCode = new CourtArchiveIndexCode();
                        newCode.CourtArchiveIndexId = saved.Id;
                        newCode.CaseCodeId = itemId;
                        newCode.DateFrom = fromDate;
                        repo.Add<CourtArchiveIndexCode>(newCode);
                    }
                }

                repo.SaveChanges();
                model.Id = saved.Id;
                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CourtArchiveIndex Id={ model.Id }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        public IQueryable<MultiSelectTransferVM> CourtArchiveIndexCode_Select(int indexId)
        {
            DateTime dateTomorrow = DateTime.Now.AddDays(1).Date;

            return repo.AllReadonly<CourtArchiveIndexCode>()
                .Include(x => x.CaseCode)
           .Where(x => x.CourtArchiveIndexId == indexId &&
                       (x.DateTo ?? dateTomorrow) > DateTime.Now)
           .Select(x => new MultiSelectTransferVM()
           {
               Id = x.CaseCodeId,
               Order = 0,
               Text = $"{x.CaseCode.Code} {x.CaseCode.Label}"
           }).AsQueryable();
        }
        public List<SelectListItem> ArchiveCommittee_SelectDDL(int courtId)
        {
            var result = repo.AllReadonly<CourtArchiveCommittee>()
                       .Where(x=>x.CourtId == courtId)
                       .OrderBy(x => x.Label)
                       .Select(x => new SelectListItem()
                       {
                           Text = x.Label,
                           Value = x.Id.ToString()
                       }).ToList() ?? new List<SelectListItem>();
            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }

        public List<SelectListItem> ArchiveIndex_SelectDDL(int courtId, int caseCodeId)
        {
            DateTime dateTomorrow = DateTime.Now.AddDays(1).Date;
            var result = repo.AllReadonly<CourtArchiveIndex>()
                       .Include(x => x.CourtArchiveIndexCodes)
                       .Where(x => x.CourtId == courtId)
                       .Where(x => (x.DateEnd ?? dateTomorrow).Date >= DateTime.Now)
                       .Where(x => x.CourtArchiveIndexCodes.Where(c => (c.DateTo ?? dateTomorrow).Date >= DateTime.Now && c.CaseCodeId == caseCodeId).Any())
                       .OrderBy(x => x.Label)
                       .Select(x => new SelectListItem()
                       {
                           Text = (x.Code ?? "") + " - " + x.Label,
                           Value = x.Id.ToString()
                       }).ToList() ?? new List<SelectListItem>();
            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }

    }
}
