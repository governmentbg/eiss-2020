using AutoMapper.QueryableExtensions;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Identity;
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
        private readonly ICaseMigrationService migrationService;
        private readonly IWorkTaskService worktaskService;
        public DocumentResolutionService(
            ILogger<DocumentResolutionService> _logger,
            IRepository _repo,
            IUserContext _userContext,
            ICaseMigrationService _migrationService,
            ICounterService _counterService,
            IWorkTaskService _worktaskService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            counterService = _counterService;
            migrationService = _migrationService;
            worktaskService = _worktaskService;
        }

        public IQueryable<DocumentResolutionVM> Select(DocumentResolutionFilterVM filter)
        {
            filter.NormalizeValues();

            return repo.AllReadonly<DocumentResolution>()
                            .Include(x => x.JudgeDecisionLawunit)
                            .Include(x => x.JudgeDecisionLawunit2)
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
                            .Where(FilterExpireInfo<DocumentResolution>(false))
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
                            .Include(x => x.JudgeDecisionLawunit2)
                            .Include(x => x.ResolutionType)
                            .Include(x => x.ResolutionState)
                            .Include(x => x.Court)
                            .Where(idSearch)
                            .Where(FilterExpireInfo<DocumentResolution>(false))
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
            if (model.JudgeDecisionCount == 2 && model.JudgeDecisionLawunit2Id.HasValue)
            {
                model.JudgeDecisionUser2Id = GetUserIdByLawUnitId(model.JudgeDecisionLawunit2Id.Value);
                if (string.IsNullOrEmpty(model.JudgeDecisionUser2Id))
                {
                    return new SaveResultVM(false, "За избрания съдия няма потребител в системата");
                }
            }
            else
            {
                model.JudgeDecisionLawunit2Id = null;
                model.JudgeDecisionUser2Id = null;
            }
            try
            {
                if (model.Id > 0)
                {
                    var saved = repo.GetById<DocumentResolution>(model.Id);
                    saved.JudgeDecisionCount = model.JudgeDecisionCount;
                    saved.JudgeDecisionLawunitId = model.JudgeDecisionLawunitId;
                    saved.JudgeDecisionUserId = model.JudgeDecisionUserId;
                    saved.JudgeDecisionLawunit2Id = model.JudgeDecisionLawunit2Id;
                    saved.JudgeDecisionUser2Id = model.JudgeDecisionUser2Id;
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

        public SaveResultVM UpdateAfterSign(long id)
        {
            var model = repo.GetById<DocumentResolution>(id);
            if (model == null)
            {
                return new SaveResultVM(false, "Грешен идентификатор на решение");
            }
            var caseModel = repo.AllReadonly<Case>().Where(x => x.DocumentId == model.DocumentId).FirstOrDefault();

            var precCaseId = repo.AllReadonly<Document>()
                                      .Include(x => x.DocumentCaseInfo)
                                      .Where(x => x.Id == model.DocumentId)
                                      .SelectMany(x => x.DocumentCaseInfo.Select(s => s.CaseId))
                                      .FirstOrDefault() ?? 0;

            if (caseModel == null || precCaseId <= 0)
            {
                return new SaveResultVM(false);
            }

            switch (caseModel.CaseStateId)
            {
                case NomenclatureConstants.CaseState.Rejected:
                    //Когато има дело и то е с Отказ от образуване и съществува движение на делото към текущия съд - се връща на подателя
                    var lastMigration = migrationService.Case_GetPriorCase(model.DocumentId);
                    if (lastMigration != null)
                    {
                        //var newMigration = new CaseMigration()
                        //{
                        //    CaseId = lastMigration.CaseId,
                        //    CourtId = model.CourtId,
                        //    PriorCaseId = lastMigration.CaseId,
                        //    InitialCaseId = lastMigration.InitialCaseId,
                        //    CaseMigrationTypeId = incommingMigrationTypeId,
                        //    SendToTypeId = NomenclatureConstants.CaseMigrationSendTo.Court,
                        //    SendToCourtId = model.CourtId,
                        //    Description = model.Description,
                        //    DateWrt = DateTime.Now,
                        //    UserId = userContext.UserId,
                        //    OutCaseMigrationId = lastMigration.Id
                        //};
                        //repo.Add(newMigration);
                    }
                    break;
                default:
                    break;
            }



            return new SaveResultVM(true);
        }

        public IQueryable<DocumentResolutionCaseVM> SelectCasesByResolution(long documentResolutionId)
        {
            return repo.AllReadonly<DocumentResolutionCase>()
                             .Include(x => x.Case)
                             .ThenInclude(x => x.CaseType)
                             .Where(x => x.DocumentResolutionId == documentResolutionId)
                             .Select(x => new DocumentResolutionCaseVM
                             {
                                 Id = x.Id,
                                 CaseId = x.CaseId,
                                 CaseTypeName = x.Case.CaseType.Code,
                                 CaseNumber = x.Case.RegNumber,
                                 CaseShortNumber = x.Case.ShortNumberValue ?? 0,
                                 CaseYear = x.Case.RegDate.Year
                             }).AsQueryable();
        }

        public bool AppendCaseToResolution(long documentResolutionId, int caseId)
        {
            var saved = repo.AllReadonly<DocumentResolutionCase>()
                            .Where(x => x.DocumentResolutionId == documentResolutionId && x.CaseId == caseId)
                            .FirstOrDefault();

            if (saved == null)
            {
                repo.Add(new DocumentResolutionCase()
                {
                    DocumentResolutionId = documentResolutionId,
                    CaseId = caseId
                });
                repo.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool RemoveCaseToResolution(long documentResolutionCaseId)
        {
            var saved = repo.All<DocumentResolutionCase>()
                            .Where(x => x.Id == documentResolutionCaseId)
                            .FirstOrDefault();
            if (saved != null)
            {
                repo.Delete(saved);
                repo.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        public SaveResultVM ResolutionExpire(ExpiredInfoVM model)
        {
            try
            {
                var saved = repo.GetById<DocumentResolution>(model.LongId);

                if (saved.DateExpired != null)
                {
                    return new SaveResultVM(false, "Разпореждането вече е изтрито.");
                }

                if (saved != null)
                {
                    saved.DateExpired = DateTime.Now;
                    saved.UserExpiredId = userContext.UserId;
                    saved.DescriptionExpired = model.DescriptionExpired;
                    repo.Update(saved);

                    var docTasks = repo.All<WorkTask>()
                                    .Where(x => x.SourceType == SourceTypeSelectVM.DocumentResolution && x.SourceId == model.LongId)
                                    .ToList();
                    if (docTasks.Any())
                    {
                        foreach (var task in docTasks)
                        {
                            task.TaskStateId = WorkTaskConstants.States.Deleted;
                        }
                    }

                    repo.SaveChanges();

                    return new SaveResultVM(true);
                }
                else
                    return new SaveResultVM(false, "Ненамерено разпореждане.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при премахване на разпореждане Id={ model.LongId }");
                return new SaveResultVM(false);
            }
        }

        public (bool canAccess, string lawunitName) CheckActBlankAccess(bool forBlank, long id, DocumentResolution model = null)
        {
            var act = model ?? repo.GetById<DocumentResolution>(id);

            if (forBlank && act.CourtId != userContext.CourtId)
            {

                return (false, string.Empty);
            }
            //Ако акта е постановен - има достъп
            if (act.DeclaredDate != null)
            {
                return (true, string.Empty);
            }
            if (act.ActCreatorUserId == userContext.UserId ||
                act.JudgeDecisionUserId == userContext.UserId ||
                act.JudgeDecisionUser2Id == userContext.UserId)
            {
                return (true, string.Empty);
            }

            if (!string.IsNullOrEmpty(act.ActCreatorUserId))
            {
                var lawunitName = repo.All<ApplicationUser>()
                                              .Include(x => x.LawUnit)
                                              .Where(x => x.Id == act.ActCreatorUserId)
                                              .Select(x => (x.LawUnit != null) ? x.LawUnit.FullName : "")
                                              .FirstOrDefault();

                return (false, lawunitName);
            }

            return (true, string.Empty);
        }

        public SaveResultVM UpdateActCreator(long id)
        {
            var act = repo.GetById<DocumentResolution>(id);
            if (act == null)
            {
                return new SaveResultVM(false);
            }
            //Ако акта няма създател и акта все още не е издаден 
            if (string.IsNullOrEmpty(act.ActCreatorUserId) && act.DeclaredDate == null)
            {
                act.ActCreatorUserId = userContext.UserId;
                repo.SaveChanges();
                return new SaveResultVM(true);
            }
            return new SaveResultVM(false);
        }
    }
}
