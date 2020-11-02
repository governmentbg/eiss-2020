// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
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
    public class CourtGroupService : BaseService, ICourtGroupService
    {
        private readonly ICourtLoadPeriodService loadPeriodService;

        public CourtGroupService(
            ILogger<CourtGroupService> _logger,
            ICourtLoadPeriodService _loadPeriodService,
            IRepository _repo)
        {
            logger = _logger;
            repo = _repo;
            loadPeriodService = _loadPeriodService;
        }

        public IQueryable<CourtGroupVM> CourtGroup_Select(int courtId, int caseGroupId)
        {

            return repo.AllReadonly<CourtGroup>()
           .Include(x => x.CaseGroup)
           .Where(x => x.CourtId == courtId && (x.CaseGroupId == caseGroupId || caseGroupId <= 0))
           .Select(x => new CourtGroupVM()
           {
               Id = x.Id,
               Label = x.Label,
               OrderNumber = x.OrderNumber,
               CaseGroupLabel = x.CaseGroup.Label,
               DateFrom = x.DateFrom,
               DateTo = x.DateTo,
               CountCode = x.CourtGroupCodes.Where(a => a.DateTo == null).Count(),
               CountLawUnit = x.CourtLawUnitGroups.Where(a => a.DateTo == null).Count()
           }).AsQueryable()
           .OrderBy(x => x.OrderNumber);
        }
        public CourtGroupVM GetCourtGroupVMById(int Id)
        {
            var courtGroup = repo.GetById<CourtGroup>(Id);
            var caseGroup = repo.GetById<CaseGroup>(courtGroup.CaseGroupId);
            return new CourtGroupVM()
            {
                Id = courtGroup.Id,
                Label = courtGroup.Label,
                OrderNumber = courtGroup.OrderNumber,
                CaseGroupLabel = caseGroup.Label,
                DateFrom = courtGroup.DateFrom,
                DateTo = courtGroup.DateTo,
            };
        }
        public bool CourtGroup_SaveData(CourtGroup model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CourtGroup>(model.Id);
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.CaseGroupId = model.CaseGroupId;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo.ForceEndDate();
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    model.OrderNumber = CourtGroup_Select(model.CourtId, 0).Max(p => (int?)p.OrderNumber) ?? 0;
                    model.OrderNumber++;
                    repo.Add<CourtGroup>(model);
                    repo.SaveChanges();

                    //loadPeriodService.GetLoadPeriod(model.Id, null);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CourtGroup Id={ model.Id }");
                return false;
            }
        }

        public IQueryable<MultiSelectTransferPercentVM> CourtGroupForSelect_Select(int courtId, int caseGroupId)
        {
            DateTime dateTomorrow = DateTime.Now.AddDays(1).Date;
            return repo.AllReadonly<CourtGroup>().Where(x => x.CourtId == courtId && (x.DateTo ?? dateTomorrow).Date > DateTime.Now.Date && 
                                                         (caseGroupId <= 0 || x.CaseGroupId == caseGroupId))
                .Select(x => new MultiSelectTransferPercentVM()
                {
                    Id = x.Id,
                    Order = x.OrderNumber,
                    Text = x.Label,
                    Percent = 100
                }).AsQueryable();
        }

        public List<SelectListItem> CourtGroup_SelectForDropDownList(int courtId, int CaseCodeId)
        {
            DateTime dateTomorrow = DateTime.Now.AddDays(1).Date;

            var result = repo.AllReadonly<CourtGroupCode>()
                .Include(x=>x.CourtGroup)
                .Where(x => x.CaseCodeId == CaseCodeId && x.CourtGroup.CourtId == courtId &&
                          (x.DateTo ?? dateTomorrow).Date >= DateTime.Now.Date &&
                          (x.CourtGroup.DateTo ?? dateTomorrow).Date > DateTime.Now.Date)
                 .OrderBy(x => x.CourtGroup.Label)
                                 .Select(x => new SelectListItem()
                                 {
                                     Value = x.CourtGroup.Id.ToString(),
                                     Text = x.CourtGroup.Label
                                 }).ToList();


            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });

            return result;
        }
    }
}
