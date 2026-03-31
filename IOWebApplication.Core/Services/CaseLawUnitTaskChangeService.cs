using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.WsTrust;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace IOWebApplication.Core.Services
{
    public class CaseLawUnitTaskChangeService : BaseService, ICaseLawUnitTaskChangeService
    {
        public CaseLawUnitTaskChangeService(
            ILogger<CaseLawUnitTaskChangeService> _logger,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }

        public IQueryable<CaseLawUnitTaskChangeVM> Select(int? id, DateTime? dateFrom, DateTime? dateTo, string caseNumber, string newTaskUserName)
        {
            int courtId = userContext.CourtId;

            Expression<Func<CaseLawUnitTaskChange, bool>> filterId = x => true;
            if (id > 0)
            {
                filterId = x => x.Id == id.Value;
            }
            Expression<Func<CaseLawUnitTaskChange, bool>> filterCaseNumber = x => true;
            if (!string.IsNullOrEmpty(caseNumber))
            {
                filterCaseNumber = x => EF.Functions.ILike(x.Case.RegNumber, caseNumber.ToCasePaternSearch());
            }
            Expression<Func<CaseLawUnitTaskChange, bool>> filterDateFrom = x => true;
            if (dateFrom.HasValue)
            {
                filterDateFrom = x => x.DateWrt >= dateFrom.Value;
            }
            Expression<Func<CaseLawUnitTaskChange, bool>> filterDateTo = x => true;
            if (dateTo.HasValue)
            {
                filterDateTo = x => x.DateWrt <= dateTo.MakeEndDate().Value;
            }
            Expression<Func<CaseLawUnitTaskChange, bool>> filterTaskUser = x => true;
            if (!string.IsNullOrEmpty(newTaskUserName))
            {
                filterTaskUser = x => EF.Functions.ILike(x.NewTaskUser.LawUnit.FullName, newTaskUserName.ToPaternSearch());
            }
            return repo.AllReadonly<CaseLawUnitTaskChange>()
                            .Where(filterId)
                            .Where(x => x.CourtId == userContext.CourtId)
                            .Where(filterCaseNumber)
                            .Where(filterDateFrom)
                            .Where(filterDateTo)
                            .Where(filterTaskUser)
                            .Select(x => new CaseLawUnitTaskChangeVM
                            {
                                Id = x.Id,
                                CaseId = x.CaseId,
                                CaseNumber = x.Case.RegNumber,
                                ActType = x.CaseSessionAct.ActType.Label,
                                ActNumber = x.CaseSessionAct.RegNumber,
                                ActDate = (x.CaseSessionAct.RegDate != null) ? x.CaseSessionAct.RegDate : x.CaseSessionAct.CaseSession.DateFrom,
                                ChangeDate = x.DateWrt,
                                Description = x.Description,
                                TaskDate = x.WorkTask.DateCreated,
                                TaskTypeName = x.WorkTask.TaskType.Label,
                                OldTaskUserName = (x.WorkTask.UserId != null) ? x.WorkTask.User.LawUnit.FullName : "",
                                NewTaskUserName = x.NewTaskUser.LawUnit.FullName,
                                ChangeUserName = x.User.LawUnit.FullName
                            }).AsQueryable();
        }

        public SaveResultVM SaveData(CaseLawUnitTaskChange model)
        {
            SaveResultVM result = new SaveResultVM();

            try
            {
                model.CourtId = userContext.CourtId;
                SetUserDateWRT(model);

                //изчитане на текущата задача за подпис
                var workTask = repo.GetById<WorkTask>(model.WorkTaskId);

                //създаване на нова задача с друг изпълнител
                var newTask = new WorkTask()
                {
                    CourtId = workTask.CourtId,
                    SourceType = workTask.SourceType,
                    SourceId = workTask.SourceId,
                    SubSourceId = workTask.SubSourceId,
                    SourceDescription = workTask.SourceDescription,
                    TaskTypeId = workTask.TaskTypeId,
                    TaskExecutionId = workTask.TaskExecutionId,
                    ParentTaskId = workTask.ParentTaskId,
                    UserCreatedId = userContext.UserId,
                    DateCreated = DateTime.Now,
                    Description = model.Description,
                    DateEnd = workTask.DateEnd,
                    UserId = model.NewTaskUserId,
                    TaskStateId = WorkTaskConstants.States.New
                };

                workTask.TaskStateId = WorkTaskConstants.States.Deleted;

                repo.Add(newTask);

                repo.Add(model);

                repo.SaveChanges();

                result.Result = true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис");
                result.Result = false;
            }

            return result;
        }


    }
}
