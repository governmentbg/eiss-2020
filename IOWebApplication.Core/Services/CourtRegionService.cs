// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using IOWebApplication.Infrastructure.Data.Models;

namespace IOWebApplication.Core.Services
{
    public class CourtRegionService : BaseService, ICourtRegionService
    {
        private readonly ICommonService commonService;
        private readonly INomenclatureService nomenclatureService;

        public CourtRegionService(
            ILogger<CourtRegionService> _logger,
            ICommonService _commonService,
            INomenclatureService _nomenclatureService,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            commonService = _commonService;
            nomenclatureService = _nomenclatureService;
            userContext = _userContext;
        }

        public IQueryable<CourtRegionVM> CourtRegion_Select()
        {
            return repo.AllReadonly<CourtRegion>()
                .Include(x => x.ParentRegion)
                .Select(x => new CourtRegionVM()
                {
                    Id = x.Id,
                    Label = x.Label,
                    ParentLabel = (x.ParentRegion != null) ? x.ParentRegion.Label : "Главен елемент",
                })
                .AsQueryable();
        }
        public List<SelectListItem> CourtRegionSelectDDL()
        {
            var result = repo.AllReadonly<CourtRegion>()
                .OrderBy(x => x.Label)
                .Select(x => new SelectListItem()
                {
                    Text = x.Label,
                    Value = x.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();
            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }
        

        public bool CourtRegion_SaveData(CourtRegion model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CourtRegion>(model.Id);
                    saved.Label = model.Label;
                    saved.ParentId = model.ParentId;
                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    model.IsActive = true;
                    model.UserId = userContext.UserId;
                    model.DateWrt = DateTime.Now;
                    repo.Add<CourtRegion>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Съдебен район Id={ model.Id }");
                return false;
            }
        }

        public List<SelectListItem> GetDropDownList()
        {
            var result = repo.AllReadonly<CourtRegion>()
                .Select(x => new SelectListItem()
                {
                    Text = x.Label,
                    Value = x.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();

            result = result.Prepend(new SelectListItem() { Text = "Главен елемент", Value = null })
                    .ToList();
            return result;
        }

        public IQueryable<CourtRegionAreaVM> CourtRegionArea_Select(int CourtRegionId)
        {
            var ekDistricts = repo.AllReadonly<EkDistrict>().AsQueryable();
            var ekMunincipalities = repo.AllReadonly<EkMunincipality>().AsQueryable();
          
            return repo.AllReadonly<CourtRegionArea>()
                .Where(x => x.CourtRegionId == CourtRegionId)
                .Select(x => new CourtRegionAreaVM()
                {
                    Id = x.Id,
                    DistrictCode = x.DistrictCode,
                    DistrictName = (ekDistricts.Where(x1 => x1.Ekatte == x.DistrictCode).FirstOrDefault() ?? new EkDistrict()).Name,
                    MunicipalityCode = (x.MunicipalityCode != null) ? x.MunicipalityCode : string.Empty,
                    MunicipalityName = (ekMunincipalities.Where(x1 => x1.Municipality == x.MunicipalityCode).FirstOrDefault() ?? new EkMunincipality()).Name,
                })
                .AsQueryable();
        }

        public bool CourtRegionArea_SaveData(CourtRegionArea model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CourtRegionArea>(model.Id);
                    saved.DistrictCode = model.DistrictCode;
                    saved.MunicipalityCode = model.MunicipalityCode;
                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    model.IsActive = true;
                    model.UserId = userContext.UserId;
                    model.DateWrt = DateTime.Now;
                    repo.Add<CourtRegionArea>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Съдебен район Id={ model.Id }");
                return false;
            }
        }
    }
}
