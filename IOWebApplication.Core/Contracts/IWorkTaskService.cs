using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IWorkTaskService : IBaseService
    {
        Task<IEnumerable<WorkTaskVM>> Select(int sourceType, long sourceId);
        IEnumerable<WorkTaskVM> Select_ToDo(int pageSize = 0);
        IQueryable<WorkTaskVM> Select(WorkTaskFilterVM filter);
        IQueryable<WorkTaskVM> SelectAll(WorkTaskFilterVM filter);
        Task<int> Select_ToDoCount();

        WorkTaskEditVM InitTask(int sourceType, long sourceId);
        WorkTask Select_ById(long id);
        WorkTaskEditVM Get_ById(long id);
        bool ValidateSourceCourt(int sourceType, long sourceId);
        int? GetSourceCourtId(int sourceType, long sourceId);

        bool UpdateTask(WorkTaskEditVM model);
        Task<bool> CreateTask(WorkTaskEditVM model);
        Task<bool> RedirectTask(WorkTaskEditVM model);
        Task<SaveResultVM> AcceptTask(long id);
        Task<bool> CompleteTask(long id);
        Task<bool> CompleteTask(WorkTask model, int completedState = WorkTaskConstants.States.Completed);
        Task<SaveResultVM> UpdateAfterCompleteTask(WorkTask model);
        List<SelectListItem> GetDDL_TaskActions(int taskTypeId);
        List<SelectListItem> GetDDL_TaskTypes(int sourceType, long sourceId = 0);

        /// <summary>
        /// Връща Case.ID по подадено ID на задача, прикачена към документ
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        int GetCaseIdByDocTaskId(long taskId);
        int[] GetSelfTask();

        string GetTaskObjectUrl(int sourceType, long sourceId);
        string GetTaskParentObjectUrl(int sourceType, long sourceId);

        LawUnit GetLawUnitByTaskId(long id);
        bool ExpireAllUnfinishedTasks(int sourceType, long sourceId);
        bool ExpireTasks(long[] taskIds, string description);
        Task<bool> RerouteTasks(long[] taskIds, WorkTaskManageVM model);
        Task<WorkTask> ReadById(long id);
        Task<bool> RejectTask(long id, string description);
        Task<SaveResultVM> UpdateBeforeCompleteTask(long workTaskId);
        Task<WorkTaskCheckCompletedVM> CheckCompletedTasks(int sourceType, long sourceId, int taskTypeId, long taskId, long? parentTaskId = null, DateTime? taskDateCompleted = null);
        Task<WorkTaskCheckCompletedVM> CheckCompletedTasks(WorkTask task);
        Task<WorkTaskCheckCompletedVM> CheckCompletedTasks(long taskId);
        Task<SaveResultVM> ValidateBeforeCreate(WorkTaskEditVM model);
        Task<string> MakeSignComfirmMessage(WorkTask taskModel);
    }
}
