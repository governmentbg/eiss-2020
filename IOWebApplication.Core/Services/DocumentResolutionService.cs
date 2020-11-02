// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using AutoMapper.QueryableExtensions;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class DocumentResolutionService : BaseService, IDocumentResolutionService
    {
        private readonly ICounterService counterService;
        private readonly IWorkTaskService worktaskService;
        public DocumentResolutionService(
            ILogger<DocumentResolutionService> _logger,
            IRepository _repo,
            IUserContext _userContext,
            ICounterService _counterService,
            IWorkTaskService _worktaskService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            counterService = _counterService;
            worktaskService = _worktaskService;
        }

        public IQueryable<DocumentResolutionVM> Select(DocumentResolutionFilterVM filter)
        {
            filter.NormalizeValues();

            return repo.AllReadonly<DocumentResolution>()
                            .Include(x => x.JudgeDecisionLawunit)
                            .Include(x => x.ResolutionType)
                            .Include(x => x.ResolutionState)
                            .Include(x => x.Court)
                            .Include(x => x.Document)
                            .ThenInclude(x => x.DocumentType)
                            .Where(x => x.CourtId == userContext.CourtId)
                            .Where(x => x.RegDate != null)
                            .Where(x => x.RegDate >= (filter.DateFrom.ForceStartDate() ?? x.RegDate) && x.RegDate <= (filter.DateTo.ForceEndDate() ?? x.RegDate))
                            .Where(x => x.RegNumber == (filter.ResolutionNumber ?? x.RegNumber))
                            .Where(x => x.JudgeDecisionLawunitId == (filter.JudgeId ?? x.JudgeDecisionLawunitId))
                            .Where(x => x.Document.DocumentNumber == (filter.DocumentNumber ?? x.Document.DocumentNumber))
                            .Where(x => x.Document.DocumentDate.Year == (filter.DocumentYear ?? x.Document.DocumentDate.Year))
                            .ProjectTo<DocumentResolutionVM>(DocumentResolutionVM.GetMapping())
                            .AsQueryable();
        }

        public IQueryable<DocumentResolutionVM> Select(long documentId, long? id = null)
        {

            Expression<Func<DocumentResolution, bool>> idSearch = x => x.DocumentId == documentId;
            if (id > 0)
            {
                idSearch = x => x.Id == id;
            }
            return repo.AllReadonly<DocumentResolution>()
                            .Include(x => x.JudgeDecisionLawunit)
                            .Include(x => x.ResolutionType)
                            .Include(x => x.ResolutionState)
                            .Include(x => x.Court)
                            .Where(idSearch)
                            .ProjectTo<DocumentResolutionVM>(DocumentResolutionVM.GetMapping())
                            //.Select(x => new DocumentResolutionVM
                            //{
                            //    Id = x.Id,
                            //    DocumentId = x.DocumentId,
                            //    ResolutionTypeId = x.ResolutionTypeId,
                            //    ResolutionTypeName = x.ResolutionType.Label,
                            //    JudgeName = x.JudgeDecisionLawunit.FullName,
                            //    JudgeUserId = x.JudgeDecisionUserId,
                            //    RegNumber = x.RegNumber,
                            //    RegDate = x.RegDate,
                            //    StateName = x.ResolutionState.Label,
                            //    CourtId = x.CourtId,
                            //    CourtName = x.Court.Label,
                            //    CourtCity = x.Court.CityName
                            //})
                            .AsQueryable();
        }


        public SaveResultVM SaveData(DocumentResolution model)
        {
            model.JudgeDecisionUserId = GetUserIdByLawUnitId(model.JudgeDecisionLawunitId);
            if (string.IsNullOrEmpty(model.JudgeDecisionUserId))
            {
                return new SaveResultVM(false, "За избрания съдия няма потребител в системата");
            }
            try
            {
                if (model.Id > 0)
                {
                    var saved = repo.GetById<DocumentResolution>(model.Id);
                    saved.JudgeDecisionLawunitId = model.JudgeDecisionLawunitId;
                    saved.JudgeDecisionUserId = model.JudgeDecisionUserId;
                    saved.ResolutionTypeId = model.ResolutionTypeId;
                    saved.UserDecisionId = model.UserDecisionId;
                    saved.Description = model.Description;
                    SetUserDateWRT(saved);
                    repo.Update(saved);
                    repo.SaveChanges();
                    return new SaveResultVM(true);
                }
                else
                {
                    model.CourtId = userContext.CourtId;
                    model.ResolutionStateId = 1;
                    repo.Add(model);
                    repo.SaveChanges();
                    DocumentResolution_SaveData_FinishTask(model.DocumentId);
                    return new SaveResultVM(true);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DocumentResolutionService.SaveData");
                return new SaveResultVM(false);
            }
        }


        private void DocumentResolution_SaveData_FinishTask(long documentId)
        {
            var myRouteTasks = repo.All<WorkTask>(
                x => x.SourceId == documentId
                && x.SourceType == SourceTypeSelectVM.Document
                && x.UserId == userContext.UserId
                && x.TaskTypeId == WorkTaskConstants.Types.ForDocumentResolution
                && x.TaskStateId == WorkTaskConstants.States.Accepted);

            foreach (var item in myRouteTasks)
            {
                worktaskService.CompleteTask(item);
            }
        }

        public SaveResultVM Register(DocumentResolution model)
        {
            if (model.RegDate != null)
            {
                return new SaveResultVM(true);
            }

            if (counterService.Counter_GetDocumentResolutionCounter(model))
            {
                repo.Update(model);
                repo.SaveChanges();
                return new SaveResultVM(true, null, "register");
            }
            else
            {
                return new SaveResultVM(false, "Проблем при регистриране на разпореждане.");
            }
        }


    }
}
