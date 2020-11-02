// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IWorkTaskService : IBaseService
    {
        IEnumerable<WorkTaskVM> Select(int sourceType, long sourceId);
        IEnumerable<WorkTaskVM> Select_ToDo(int pageSize = 0);
        IEnumerable<WorkTaskVM> Select(WorkTaskFilterVM filter);
        IQueryable<WorkTaskVM> SelectAll(WorkTaskFilterVM filter);
        int Select_ToDoCount();

        WorkTaskEditVM InitTask(int sourceType, long sourceId);
        WorkTask Select_ById(long id);
        WorkTaskEditVM Get_ById(long id);
        bool ValidateSourceCourt(int sourceType, long sourceId);

        bool UpdateTask(WorkTaskEditVM model);
        bool CreateTask(WorkTaskEditVM model);
        bool RedirectTask(WorkTaskEditVM model);
        bool AcceptTask(long id);
        bool CompleteTask(long id);
        bool CompleteTask(WorkTask model);
        SaveResultVM UpdateAfterCompleteTask(WorkTask model, object additionalService = null);
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

        LawUnit GetLawUnitByTaskId(long id);
        bool ExpireAllUnfinishedTasks(int sourceType, long sourceId);
    }
}
