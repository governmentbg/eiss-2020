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
    public class CourtOrganizationService : BaseService, ICourtOrganizationService
    {
        public CourtOrganizationService(
            ILogger<CourtOrganizationService> _logger,
            IRepository _repo)
        {
            logger = _logger;
            repo = _repo;
        }

        public IQueryable<CourtOrganizationVM> CourtOrganization_Select(int courtId)
        {
            return repo.AllReadonly<CourtOrganization>()
                .Include(x => x.ParentOrganization)
                .Include(x => x.OrganizationLevel)
                .Include(x => x.Court)
                .Where(x => x.CourtId == courtId)
                .Select(x => new CourtOrganizationVM()
                {
                    Id = x.Id,
                    ParentId = x.ParentId ?? 0,
                    Label = x.Label,
                    CourtLabel = x.Court.Label,
                    ParentLabel = (x.ParentOrganization != null) ? x.ParentOrganization.Label : x.Court.Label,
                    OrganizationLevelLabel = (x.OrganizationLevel != null) ? x.OrganizationLevel.Label : string.Empty
                })
                .AsQueryable();
        }

        public bool CourtOrganization_SaveData(CourtOrganizationEditVM model)
        {
            try
            {
                model.ParentId = model.ParentId.NumberEmptyToNull();
                model.ParentId = (model.ParentId == 999999) ? null : model.ParentId;
                var modelSave = FillCourtOrganization(model);
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CourtOrganization>(model.Id);
                    saved.Label = modelSave.Label;
                    saved.ParentId = modelSave.ParentId;
                    saved.Description = modelSave.Description;
                    saved.OrganizationLevelId = modelSave.OrganizationLevelId;
                    saved.IsDocumentRegistry = modelSave.IsDocumentRegistry;
                    repo.Update(saved);

                    var courtOrganizationCaseGroups = repo.AllReadonly<CourtOrganizationCaseGroup>().Where(x => x.CourtOrganizationId == model.Id);
                    repo.DeleteRange(courtOrganizationCaseGroups);
                }
                else
                {
                    //Insert
                    repo.Add<CourtOrganization>(modelSave);
                }

                if (model.IsDocumentRegistry ?? false)
                {
                    var modelSaveCaseGroupe = FillCourtOrganizationCaseGroup(model);
                    modelSaveCaseGroupe.ForEach(x => x.CourtOrganizationId = modelSave.Id);
                    repo.AddRange(modelSaveCaseGroupe);
                }
                repo.SaveChanges();
                model.Id = modelSave.Id;

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Организационна структура на съд Id={ model.Id }");
                return false;
            }
        }

        public List<SelectListItem> GetDropDownList(int courtId, int id = 0, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = repo.All<CourtOrganization>()
                             .Where(x => x.CourtId == courtId &&
                                         (id > 0 ? x.Id != id : true))
                             .Select(x => new SelectListItem()
                             {
                                 Text = x.Label,
                                 Value = x.Id.ToString()
                             }).ToList() ?? new List<SelectListItem>();

            if (result.Count > 0)
            {
                result = result.OrderBy(x => x.Text).ToList();
            }

            var court = repo.GetById<Court>(courtId);

            result = result.Prepend(new SelectListItem() { Text = court.Label, Value = "999999" })
                    .ToList();

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

        public List<SelectListItem> CourtOrganization_SelectForDropDownList(int courtId)
        {
            DateTime dateTomorrow = DateTime.Now.AddDays(1).Date;

            var result = repo.AllReadonly<CourtOrganization>()
                .Where(x => x.CourtId == courtId &&
                          ((x.DateTo ?? dateTomorrow).Date > DateTime.Now.Date)
                      )
                 .OrderBy(x => x.Label)
                                 .Select(x => new SelectListItem()
                                 {
                                     Value = x.Id.ToString(),
                                     Text = x.Label
                                 }).ToList();


            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });

            return result;
        }

        public CourtOrganizationEditVM CourtOrganization_GetById(int id)
        {
            return FillCourtOrganizationEditVM(repo.GetById<CourtOrganization>(id));
        }

        private CourtOrganization FillCourtOrganization(CourtOrganizationEditVM model)
        {
            return new CourtOrganization()
            {
                Id = model.Id,
                CourtId = model.CourtId,
                ParentId = model.ParentId,
                Label = model.Label,
                Description = model.Description,
                OrganizationLevelId = model.OrganizationLevelId,
                IsDocumentRegistry = model.IsDocumentRegistry,
                DateFrom = model.DateFrom,
                DateTo = model.DateTo
            };
        }

        private List<CourtOrganizationCaseGroup> FillCourtOrganizationCaseGroup(CourtOrganizationEditVM model)
        {
            return model.CourtOrganizationCaseGroups
                        .Where(x => x.Checked)
                        .Select(x => new CourtOrganizationCaseGroup()
                        {
                            CaseGroupId = int.Parse(x.Value),
                            CourtOrganizationId = model.Id
                        })
                        .ToList();
        }

        private CourtOrganizationEditVM FillCourtOrganizationEditVM(CourtOrganization model)
        {
            return new CourtOrganizationEditVM()
            {
                Id = model.Id,
                CourtId = model.CourtId,
                ParentId = model.ParentId ?? 999999,
                Label = model.Label,
                Description = model.Description,
                OrganizationLevelId = model.OrganizationLevelId,
                IsDocumentRegistry = model.IsDocumentRegistry,
                DateFrom = model.DateFrom,
                DateTo = model.DateTo,
                CourtOrganizationCaseGroups = FillCheckListCourtOrganizationCaseGroups(model.Id)
            };
        }

        private List<CheckListVM> FillCheckListCourtOrganizationCaseGroups(int id)
        {
            var result = repo.AllReadonly<CaseGroup>()
                             .Where(x => x.IsActive)
                             .Select(x => new CheckListVM()
                             {
                                 Checked = false,
                                 Label = x.Label,
                                 Value = x.Id.ToString(),
                             })
                             .ToList() ?? new List<CheckListVM>();

            if (id > 0)
            {
                var courtOrganizationCaseGroups = repo.AllReadonly<CourtOrganizationCaseGroup>()
                                                      .Where(x => x.CourtOrganizationId == id)
                                                      .ToList();

                foreach (var courtOrganizationCaseGroup in courtOrganizationCaseGroups)
                {
                    var checkList = result.Where(x => x.Value == courtOrganizationCaseGroup.CaseGroupId.ToString()).FirstOrDefault();
                    if (checkList != null)
                    {
                        checkList.Checked = true;
                    }
                }
            }

            return result;
        }

        public List<CheckListVM> FillCheckListCourtOrganizationCaseGroups()
        {
            return FillCheckListCourtOrganizationCaseGroups(0);
        }
    }
}
