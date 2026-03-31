// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class BlankTemplateService : BaseService, IBlankTemplateService
    {
        public BlankTemplateService(
            IRepository _repo,
            ILogger<BlankTemplateService> _logger,
            IUserContext _userContext)
        {
            repo = _repo;
            logger = _logger;
            userContext = _userContext;
        }

        public IQueryable<BlankTemplateListVM> BlankTemplateSelect()
        {
            var actTypes = repo.AllReadonly<ActType>()
                                    .Select(x => new
                                    {
                                        x.Id,
                                        x.Label
                                    });

            var resTypes = repo.AllReadonly<ResolutionType>()
                                    .Select(x => new
                                    {
                                        x.Id,
                                        x.Label
                                    });

            return repo.AllReadonly<BlankTemplate>()
                            .Select(x => new BlankTemplateListVM
                            {
                                Id = x.Id,
                                Label = x.Label,
                                SourceType = x.SourceType,
                                SourceId = x.SourceId,
                                SourceIdText = (x.SourceType == SourceTypeSelectVM.CaseSessionAct) ? actTypes.Where(a => a.Id == x.SourceId).Select(a => a.Label).FirstOrDefault() : resTypes.Where(a => a.Id == x.SourceId).Select(a => a.Label).FirstOrDefault(),
                                IsActive = x.IsActive
                            }).AsQueryable();
        }

        public async Task<SaveResultVM> BlankTemplateSaveData(BlankTemplate model)
        {
            try
            {
                if (model.Id > 0)
                {
                    var saved = repo.GetById<BlankTemplate>(model.Id);
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.DateStart = model.DateStart;
                    saved.DateEnd = model.DateEnd;
                    saved.IsActive = model.IsActive;
                    saved.MainText = model.MainText;
                    saved.AddText = model.AddText;
                }
                else
                {
                    model.OrderNumber = 0;
                    repo.Add(model);
                }
                await repo.SaveChangesAsync().ConfigureAwait(false);

                return new SaveResultVM(true);
            }
            catch (Exception ex)
            {
                logger.LogError("BlankTemplate.SaveData", ex);
                return new SaveResultVM(false);
            }
        }

        public async Task<List<SelectListItem>> GetBlankTemplates(int sourceType, int sourceId, int? caseId = null)
        {
            Expression<Func<BlankTemplateLink, bool>> whereCase = x => true;
            if (caseId > 0)
            {
                var _case = repo.AllReadonly<Case>()
                                        .Where(x => x.Id == caseId)
                                        .Select(x => new
                                        {
                                            x.CaseGroupId,
                                            CaseInstanceId = x.CaseType.CaseInstanceId

                                        }).FirstOrDefault();
                whereCase = x => (_case.CaseGroupId == (x.CaseGroupId ?? _case.CaseGroupId)) && (_case.CaseInstanceId == (x.CaseInstanceId ?? _case.CaseInstanceId));
            }
            var blanksList = await repo.AllReadonly<BlankTemplateLink>()
                                    .Include(x => x.BlankTemplate)
                                    .Where(x => x.BlankTemplate.SourceType == sourceType && x.BlankTemplate.SourceId == sourceId)
                                    .Where(x => userContext.CourtTypeId == (x.CourtTypeId ?? userContext.CourtTypeId))
                                    .Where(whereCase)
                                    .Select(x => x.BlankTemplateId).ToArrayAsync().ConfigureAwait(false);


            return await repo.AllReadonly<BlankTemplate>()
                            .Where(x => blanksList.Contains(x.Id))
                            .Where(x => x.SourceType == sourceType && x.SourceId == sourceId)
                            .Where(x => x.IsActive)
                            .Select(x => new SelectListItem
                            {
                                Text = x.Label,
                                Value = x.Id.ToString()
                            }).ToListAsync().ConfigureAwait(false);
        }

        public IQueryable<LawUnitTemplateListVM> LawUnitTemplateSelect()
        {
            return repo.AllReadonly<LawUnitTemplate>()
                                .Where(x => x.LawunitId == userContext.LawUnitId)
                                .OrderBy(x => x.Label)
                                .Select(x => new LawUnitTemplateListVM
                                {
                                    Id = x.Id,
                                    Label = x.Label,
                                    IsActive = x.IsActive,
                                    CaseGroup = (x.CaseGroupId > 0) ? x.CaseGroup.Code : "всички",
                                    ActType = (x.ActTypeId > 0) ? x.ActType.Label : "всички"
                                });
        }

        public async Task<SaveResultVM> LawUnitTemplateSaveData(LawUnitTemplate model)
        {
            model.CaseGroupId = model.CaseGroupId.EmptyToNull(-2);
            model.ActTypeId = model.ActTypeId.EmptyToNull(-2);
            try
            {
                if (model.Id > 0)
                {
                    var saved = repo.GetById<LawUnitTemplate>(model.Id);
                    saved.Label = model.Label;
                    saved.CaseGroupId = model.CaseGroupId;
                    saved.ActTypeId = model.ActTypeId;
                    saved.IsActive = model.IsActive;
                    saved.ActMain = model.ActMain;
                    saved.ActDispositive = model.ActDispositive;
                    model.DateWrt = DateTime.Now;
                }
                else
                {
                    model.LawunitId = userContext.LawUnitId;
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    repo.Add(model);
                }
                await repo.SaveChangesAsync();

                return new SaveResultVM(true);
            }
            catch (Exception ex)
            {
                logger.LogError("LawUnitTemplateSaveData.SaveData", ex);
                return new SaveResultVM(false);
            }
        }
        public async Task<List<SelectListItem>> GetLawUnitTemplates(int actId)
        {
            Expression<Func<LawUnitTemplate, bool>> whereSearch = x => true;
            if (actId > 0)
            {
                var actInfo = await repo.AllReadonly<CaseSessionAct>()
                                            .Where(x => x.Id == actId)
                                            .Select(x => new
                                            {
                                                x.ActTypeId,
                                                x.Case.CaseGroupId
                                            }).FirstOrDefaultAsync();

                whereSearch = x => (x.CaseGroupId ?? actInfo.CaseGroupId) == actInfo.CaseGroupId &&
                                    (x.ActTypeId ?? actInfo.ActTypeId) == actInfo.ActTypeId;
            }

            return await repo.AllReadonly<LawUnitTemplate>()
                           .Where(x => x.LawunitId == userContext.LawUnitId)
                           .Where(whereSearch)
                           .Where(x => x.IsActive)
                           .Select(x => new SelectListItem
                           {
                               Text = x.Label,
                               Value = x.Id.ToString()
                           }).ToListAsync();
        }
    }
}
