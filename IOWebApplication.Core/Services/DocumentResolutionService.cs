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
using System.Threading.Tasks;

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

        private Expression<Func<DocumentResolution, DocumentResolutionVM>> expressionSelectResolution()
        {
            Expression<Func<DocumentResolution, DocumentResolutionVM>> select =
                    x => new DocumentResolutionVM
                    {
                        Id = x.Id,
                        DocumentId = x.DocumentId,
                        ResolutionTypeId = x.ResolutionTypeId,
                        RegNumber = x.RegNumber,
                        RegDate = x.RegDate,
                        DeclaredDate = x.DeclaredDate,
                        DocumentNumber = x.Document.DocumentNumber,
                        CourtId = x.CourtId,

                        DocumentTypeName = x.Document.DocumentType.Label,
                        ResolutionTypeName = x.ResolutionType.Label,
                        JudgeUserId = x.JudgeDecisionUserId,
                        JudgeName = x.JudgeDecisionLawunit.FullName,
                        JudgeDecisionLawunitId = x.JudgeDecisionLawunitId,
                        StateName = x.ResolutionState.Label,
                        CourtName = x.Court.Label,
                        CourtCity = x.Court.CityName,
                        JudgeCount = x.JudgeDecisionCount ?? 1,
                        JudgeUser2Id = x.JudgeDecisionUser2Id,
                        JudgeName2 = (x.JudgeDecisionLawunit2Id > 0) ? x.JudgeDecisionLawunit2.FullName : "",
                        JudgeDecisionLawunit2Id = x.JudgeDecisionLawunit2Id,
                    };

            return select;
        }

        /*
         *                 cfg.CreateMap<DocumentResolution, DocumentResolutionVM>()
                    .ForMember(d => d.DocumentNumber, s => s.MapFrom(m => m.Document.DocumentNumber))
                    .ForMember(d => d.DocumentTypeName, s => s.MapFrom(m => m.Document.DocumentType.Label))
                    .ForMember(d => d.ResolutionTypeName, s => s.MapFrom(m => m.ResolutionType.Label))
                    .ForMember(d => d.JudgeUserId, s => s.MapFrom(m => m.JudgeDecisionUserId))
                    .ForMember(d => d.JudgeName, s => s.MapFrom(m => m.JudgeDecisionLawunit.FullName))
                    .ForMember(d => d.StateName, s => s.MapFrom(m => m.ResolutionState.Label))
                    .ForMember(d => d.CourtName, s => s.MapFrom(m => m.Court.Label))
                    .ForMember(d => d.CourtCity, s => s.MapFrom(m => m.Court.CityName))
                    .ForMember(d => d.JudgeCount, s => s.MapFrom(m => m.JudgeDecisionCount ?? 1))
                    .ForMember(d => d.JudgeUser2Id, s => s.MapFrom(m => m.JudgeDecisionUser2Id))
                    .ForMember(d => d.JudgeName2, s => s.MapFrom(m => (m.JudgeDecisionLawunit2 != null) ? m.JudgeDecisionLawunit2.FullName : ""))

         */

        public IQueryable<DocumentResolutionVM> Select(DocumentResolutionFilterVM filter)
        {
            filter.NormalizeValues();

            return repo.AllReadonly<DocumentResolution>()                            
                            .Where(x => x.CourtId == userContext.CourtId)
                            .Where(x => x.RegDate != null)
                            .Where(x => x.RegDate >= (filter.DateFrom.ForceStartDate() ?? x.RegDate) && x.RegDate <= (filter.DateTo.ForceEndDate() ?? x.RegDate))
                            .Where(x => x.RegNumber == (filter.ResolutionNumber ?? x.RegNumber))
                            .Where(x => x.JudgeDecisionLawunitId == (filter.JudgeId ?? x.JudgeDecisionLawunitId))
                            .Where(x => x.Document.DocumentNumber == (filter.DocumentNumber ?? x.Document.DocumentNumber))
                            .Where(x => x.Document.DocumentDate.Year == (filter.DocumentYear ?? x.Document.DocumentDate.Year))
                            .Where(FilterExpireInfo<DocumentResolution>(false))
                            .Select(expressionSelectResolution())
                            .AsQueryable();
        }

        public IQueryable<DocumentResolutionVM> Select(long documentId, long? id = null)
        {

            Expression<Func<DocumentResolution, bool>> idSearch = x => x.DocumentId == documentId;
            if (id > 0)
            {
                idSearch = x => x.Id == id;
            }
            var result = repo.AllReadonly<DocumentResolution>()
                            .Include(x => x.JudgeDecisionLawunit)
                            .Include(x => x.JudgeDecisionLawunit2)
                            .Include(x => x.ResolutionType)
                            .Include(x => x.ResolutionState)
                            .Include(x => x.Court)
                            .Where(idSearch)
                            .Where(FilterExpireInfo<DocumentResolution>(false))
                            .Select(expressionSelectResolution())
                            .AsQueryable();

            if (id != null && result.Any())
            {
                var judge = result.FirstOrDefault();

                judge.JudgePosition = getJudgePosition(judge.CourtId, judge.JudgeDecisionLawunitId);
                if (judge.JudgeDecisionLawunit2Id > 0)
                {
                    judge.JudgePosition2 = getJudgePosition(judge.CourtId, judge.JudgeDecisionLawunit2Id.Value);
                }
                return new List<DocumentResolutionVM>() { judge }.AsQueryable();
            }

            return result;
        }

        private string getJudgePosition(int courtId, int lawUnitId)
        {
            var dtNow = DateTime.Now;
            var dtEnd = DateTime.Now.AddYears(1);
            return repo.AllReadonly<CourtLawUnit>()
                                    .Where(x => x.CourtId == courtId && x.LawUnitId == lawUnitId)
                                    .Where(x => NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(x.PeriodTypeId))
                                    .Where(x => x.DateFrom <= dtNow && (x.DateTo ?? dtEnd) > dtNow)
                                    .Select(x => (x.LawUnitPositionId > 0) ? x.LawUnitPosition.Label : "").FirstOrDefault();
        }


        public async Task<SaveResultVM> SaveData(DocumentResolution model)
        {
            model.JudgeDecisionUserId = GetUserIdByLawUnitId(model.JudgeDecisionLawunitId);
            model.TaskUserId = model.TaskUserId.EmptyToNull().EmptyToNull("0");
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
                    var saved = await repo.GetByIdAsync<DocumentResolution>(model.Id);
                    saved.JudgeDecisionCount = model.JudgeDecisionCount;
                    saved.JudgeDecisionLawunitId = model.JudgeDecisionLawunitId;
                    saved.JudgeDecisionUserId = model.JudgeDecisionUserId;
                    saved.JudgeDecisionLawunit2Id = model.JudgeDecisionLawunit2Id;
                    saved.JudgeDecisionUser2Id = model.JudgeDecisionUser2Id;
                    saved.ResolutionTypeId = model.ResolutionTypeId;
                    saved.UserDecisionId = model.UserDecisionId;
                    saved.Description = model.Description;
                    SetUserDateWRT(saved);
                    await repo.SaveChangesAsync();
                    return new SaveResultVM(true);
                }
                else
                {
                    model.CourtId = userContext.CourtId;
                    model.ResolutionStateId = 1;
                    SetUserDateWRT(model);
                    repo.Add(model);
                    await repo.SaveChangesAsync();
                    await DocumentResolution_SaveData_FinishTask(model.DocumentId);
                    return new SaveResultVM(true);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DocumentResolutionService.SaveData");
                return new SaveResultVM(false);
            }
        }


        private async Task DocumentResolution_SaveData_FinishTask(long documentId)
        {
            var myRouteTasks = await repo.AllReadonly<WorkTask>(
                x => x.SourceId == documentId
                && x.SourceType == SourceTypeSelectVM.Document
                && x.UserId == userContext.UserId
                && x.TaskTypeId == WorkTaskConstants.Types.ForDocumentResolution
                && x.TaskStateId == WorkTaskConstants.States.Accepted)
                .ToListAsync();

            foreach (var item in myRouteTasks)
            {
                await worktaskService.CompleteTask(item);
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
                    DateWrt = DateTime.Now,
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
                logger.LogError(ex, $"Грешка при премахване на разпореждане Id={model.LongId}");
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
                var lawunitName = repo.AllReadonly<ApplicationUser>()
                                              .Where(x => x.Id == act.ActCreatorUserId)
                                              .Select(x => x.LawUnit.FullName)
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
