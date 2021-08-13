using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;

namespace IOWebApplication.Core.Services
{
    public class WorkTaskService : BaseService, IWorkTaskService
    {
        private readonly IUrlHelper urlHelper;
        private readonly ICaseLifecycleService lifecycleService;
        private readonly IMQEpepService mqEpepService;
        private readonly ICaseDeadlineService caseDeadlineService;
        private readonly ICounterService counterService;
        private readonly ICaseLoadIndexService caseLoadIndexService;

        public WorkTaskService(
            ILogger<WorkTaskService> _logger,
            IRepository _repo,
            IUserContext _userContext,
            ICaseLifecycleService _lifecycleService,
            IMQEpepService _mqEpepService,
            ICaseDeadlineService _caseDeadlineService,
            ICounterService _counterService,
            ICaseLoadIndexService _caseLoadIndexService,
            IUrlHelper _url)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            lifecycleService = _lifecycleService;
            mqEpepService = _mqEpepService;
            caseDeadlineService = _caseDeadlineService;
            counterService = _counterService;
            urlHelper = _url;
            caseLoadIndexService = _caseLoadIndexService;
        }

        private IEnumerable<WorkTaskVM> selectTasks(bool showToDo, bool showMyTasks, int sourceType, long sourceId, bool customFilter = false)
        {
            var _userOrganizations = userContext.CourtOrganizations;

            Expression<Func<WorkTask, bool>> whereSelect = x => x.SourceType == sourceType && x.SourceId == sourceId;
            if (showToDo)
            {
                whereSelect = x => (x.TaskStateId == WorkTaskConstants.States.New && (x.UserId == userContext.UserId || _userOrganizations.Contains(x.CourtOrganizationId ?? 0))
                                || (x.TaskStateId == WorkTaskConstants.States.Accepted && x.UserId == userContext.UserId));
            }

            Expression<Func<WorkTask, bool>> whereMyTasks = x => true;
            if (showMyTasks)
            {
                whereMyTasks = x => x.UserCreatedId == userContext.UserId;
                whereSelect = x => true;
            }
            if (customFilter)
            {
                whereSelect = x => true;
                whereMyTasks = x => true;
            }
            var result = repo.AllReadonly<WorkTask>()
                            .Include(x => x.TaskType)
                            .Include(x => x.TaskAction)
                            .Include(x => x.TaskState)
                            .Include(x => x.CourtOrganization)
                            .Include(x => x.User)
                            .ThenInclude(x => x.LawUnit)
                            .Include(x => x.UserCreated)
                            .ThenInclude(x => x.LawUnit)
                            .Where(x => x.CourtId == userContext.CourtId)
                            .Where(whereSelect)
                            .Where(whereMyTasks)
                            .OrderBy(x => x.DateCreated)
                            .Select(x => new WorkTaskVM
                            {
                                Id = x.Id,
                                SourceType = x.SourceType,
                                SourceId = x.SourceId,
                                SubSourceId = x.SubSourceId,
                                SourceDescription = x.SourceDescription,
                                ParentDescription = x.ParentDescription,
                                DateCreated = x.DateCreated,
                                DateAccepted = x.DateAccepted,
                                DateCompleted = x.DateCompleted,
                                DateEnd = x.DateEnd,
                                TaskExecutionId = x.TaskExecutionId,
                                UserId = x.UserId,
                                CourtOrganizationId = x.CourtOrganizationId,
                                UserFullName = (x.User != null) ? x.User.LawUnit.FullName : x.CourtOrganization.Label,
                                UserCreatedId = x.UserCreatedId,
                                UserCreatedFullName = x.UserCreated.LawUnit.FullName,
                                Description = x.Description,
                                DescriptionCreated = x.DescriptionCreated,
                                TaskTypeId = x.TaskTypeId,
                                TaskTypeName = x.TaskType.Label,
                                TaskActionId = x.TaskActionId,
                                TaskActionName = (x.TaskAction != null) ? x.TaskAction.Label : "",
                                TaskStateId = x.TaskStateId,
                                TaskStateName = x.TaskState.Label
                            });

            return result;
        }

        private int selectTasksForToDoCount()
        {
            var _userOrganizations = userContext.CourtOrganizations.Select(x => (int?)x).ToArray();
            string userId = userContext.UserId;

            //var result = repo.AllReadonly<WorkTask>()
            //                .Where(x => x.CourtId == userContext.CourtId)
            //                .Where(x=> (x.TaskStateId == WorkTaskConstants.States.New && (x.UserId == userId || _userOrganizations.Contains(x.CourtOrganizationId ?? 0))
            //                    || (x.TaskStateId == WorkTaskConstants.States.Accepted && x.UserId == userId)))
            //                .Count();


            //var result = repo.AllReadonly<WorkTask>()
            //                .Where(x => x.CourtId == userContext.CourtId)
            //                .Where(x => x.TaskStateId == WorkTaskConstants.States.New && x.UserId == userId)
            //                .Union(repo.AllReadonly<WorkTask>()
            //                .Where(x => x.CourtId == userContext.CourtId)
            //                .Where(x => x.TaskStateId == WorkTaskConstants.States.New && _userOrganizations.Contains(x.CourtOrganizationId)))
            //                .Union(repo.AllReadonly<WorkTask>()
            //                .Where(x => x.CourtId == userContext.CourtId)
            //                .Where(x => x.TaskStateId == WorkTaskConstants.States.Accepted && x.UserId == userId))
            //                .Count();

            var result = repo.AllReadonly<WorkTask>()
                           .Where(x => x.CourtId == userContext.CourtId)
                           .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId) && x.UserId == userId)
                           .Count();
            result += repo.AllReadonly<WorkTask>()
                           .Where(x => x.CourtId == userContext.CourtId)
                           .Where(x => x.TaskStateId == WorkTaskConstants.States.New && _userOrganizations.Contains(x.CourtOrganizationId)
                           && x.UserId == null)
                           .Count();
            return result;
        }
        public IEnumerable<WorkTaskVM> Select(WorkTaskFilterVM filter)
        {
            var _userOrganizations = userContext.CourtOrganizations;

            Expression<Func<WorkTaskVM, bool>> whereSelect = x => true;
            if (filter.UserMode == 1)
            {
                whereSelect = x => (
                                    (x.TaskStateId == WorkTaskConstants.States.New && _userOrganizations.Contains(x.CourtOrganizationId ?? 0))
                                    ||
                                    (x.UserId == userContext.UserId)
                                    ) && x.UserCreatedId == (filter.UserId ?? x.UserCreatedId);
            }
            else
            {
                whereSelect = x => (x.UserCreatedId == userContext.UserId) && x.UserId == (filter.UserId ?? x.UserId);
            }
            Expression<Func<WorkTaskVM, bool>> whereState = x => x.TaskStateId == (filter.TaskStateId ?? x.TaskStateId);
            if (filter.TaskStateId == WorkTaskConstants.States.NotFinishedId)
            {
                whereState = x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId);
            }

            return selectTasks(false, false, 0, 0, true).AsQueryable()
                    .Where(x => x.DateCreated >= (filter.DateFrom ?? x.DateCreated) && x.DateCreated <= (filter.DateTo.MakeEndDate() ?? x.DateCreated))
                    .Where(whereSelect)
                    .Where(x => x.TaskTypeId == (filter.TaskTypeId ?? x.TaskTypeId))
                    .Where(whereState)
                    .Where(x => EF.Functions.ILike(x.SourceDescription ?? "", filter.SourceDescription.ToPaternSearch()))
                    .Where(x => EF.Functions.ILike(x.ParentDescription ?? "", filter.ParentDescription.ToEndingPaternSearch()));
        }

        public IEnumerable<WorkTaskVM> SelectAll(WorkTaskFilterVM filter)
        {
            Expression<Func<WorkTaskVM, bool>> userSearch = x => true;
            if (!string.IsNullOrEmpty(filter.AssignedTo))
            {
                userSearch = x => x.UserId == filter.AssignedTo;
            }
            return repo.AllReadonly<WorkTask>()
                            .Include(x => x.TaskType)
                            .Include(x => x.TaskState)
                            .Include(x => x.CourtOrganization)
                            .Include(x => x.User)
                            .ThenInclude(x => x.LawUnit)
                            .Include(x => x.UserCreated)
                            .ThenInclude(x => x.LawUnit)
                            .Where(x => x.CourtId == userContext.CourtId)
                            .Where(x => x.DateCreated >= (filter.DateFrom ?? x.DateCreated) && x.DateCreated <= (filter.DateTo.MakeEndDate() ?? x.DateCreated))
                            .Where(x => x.TaskTypeId == (filter.TaskTypeId ?? x.TaskTypeId) && x.TaskStateId == (filter.TaskStateId ?? x.TaskStateId))
                            .Where(x => EF.Functions.ILike(x.SourceDescription ?? "", filter.SourceDescription.ToPaternSearch()))
                            .Where(x => EF.Functions.ILike(x.ParentDescription ?? "", filter.ParentDescription.ToEndingPaternSearch()))
                            .Select(x => new WorkTaskVM
                            {
                                Id = x.Id,
                                SourceType = x.SourceType,
                                SourceId = x.SourceId,
                                SourceDescription = x.SourceDescription,
                                ParentDescription = x.ParentDescription,
                                DateCreated = x.DateCreated,
                                DateAccepted = x.DateAccepted,
                                DateCompleted = x.DateCompleted,
                                DateEnd = x.DateEnd,
                                UserId = x.UserId,
                                UserFullName = ((x.User != null) ? x.User.LawUnit.FullName : x.CourtOrganization.Label) ?? "",
                                UserCreatedId = x.UserCreatedId,
                                UserCreatedFullName = x.UserCreated.LawUnit.FullName,
                                Description = x.Description,
                                DescriptionCreated = x.DescriptionCreated,
                                TaskStateId = x.TaskStateId,
                                TaskTypeName = x.TaskType.Label,
                                TaskStateName = x.TaskState.Label
                            })
                            .Where(x => x.UserCreatedId == (filter.CreatedBy ?? x.UserCreatedId))
                            .Where(userSearch);
            //Преработено на autocomplete
            //.Where(x => EF.Functions.ILike(x.UserCreatedFullName, filter.CreatedBy.ToPaternSearch()))
            //.Where(x => EF.Functions.ILike(x.UserFullName, filter.AssignedTo.ToPaternSearch()));

        }
        public IEnumerable<WorkTaskVM> Select(int sourceType, long sourceId)
        {
            var result = selectTasks(false, false, sourceType, sourceId).ToList();
            setTaskActions(result);
            if (!ValidateSourceCourt(sourceType, sourceId))
            {
                foreach (var item in result)
                {
                    item.CanAccept = false;
                    item.CanUpdate = false;
                    item.CanDoAction = false;
                    item.CanRedirect = false;
                }
            }
            return result;
        }

        public IEnumerable<WorkTaskVM> Select_ToDo(int pageSize = 0)
        {
            var data = selectTasks(true, false, 0, 0).OrderByDescending(x => x.DateCreated);
            List<WorkTaskVM> result;
            if (pageSize > 0)
            {
                result = data.Take(pageSize).ToList();
            }
            else
            {
                result = data.ToList();
            }
            setTaskActions(result);
            return result;
        }

        public int Select_ToDoCount()
        {
            return selectTasksForToDoCount();
            //int count = 0;
            //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
            //        new TransactionOptions()
            //        {
            //            IsolationLevel = IsolationLevel.ReadUncommitted
            //        }))
            //{
            //    count = selectTasks(true, false, 0, 0).Count();
            //    ts.Complete();
            //}
            //return count;
        }

        private void setTaskActions(IEnumerable<WorkTaskVM> model)
        {
            var isGlobalAdmin = userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator);
            var _userOrganizations = userContext.CourtOrganizations;
            bool isTaskRouter = userContext.IsUserInRole(AccountConstants.Roles.Supervisor);
            foreach (var task in model)
            {
                bool sameUser = isGlobalAdmin;
                switch (task.TaskExecutionId)
                {
                    case WorkTaskConstants.TaskExecution.ByOrganization:
                        sameUser = sameUser || _userOrganizations.Contains(task.CourtOrganizationId ?? 0);
                        break;
                    default:
                        sameUser = sameUser || task.UserId == userContext.UserId;
                        break;
                }
                task.CanUpdate = (task.UserCreatedId == userContext.UserId && task.TaskStateId == WorkTaskConstants.States.New) ||
                                 (isTaskRouter && WorkTaskConstants.States.NotFinished.Contains(task.TaskStateId));
                task.CanAccept = (task.DateAccepted == null) && sameUser && task.TaskStateId == WorkTaskConstants.States.New;
                task.CanRedirect = task.CanAccept || (isTaskRouter && WorkTaskConstants.States.NotFinished.Contains(task.TaskStateId));
                task.CanDoAction = (task.DateAccepted != null) && (task.UserId == userContext.UserId || isGlobalAdmin) && task.DateCompleted == null && task.TaskStateId == WorkTaskConstants.States.Accepted;
                task.CanComplete = task.CanDoAction;
                if (WorkTaskConstants.Types.SelfCompleteTasks.Contains(task.TaskTypeId))
                {
                    task.CanComplete = false;
                }
                task.OverDue = (task.DateEnd < DateTime.Now) && WorkTaskConstants.States.NotFinished.Contains(task.TaskStateId);
                if (WorkTaskConstants.Types.TaskCantUpdate.Contains(task.TaskTypeId))
                {
                    task.CanUpdate = false;
                    task.CanRedirect = false;
                }
                SetDoActionUrl(task);
                task.SourceInfo = SourceTypeSelectVM.GetSourceTypeName(task.SourceType);
                task.ViewUrl = GetTaskObjectUrl(task.SourceType, task.SourceId);
            }
        }
        public string GetTaskObjectUrl(int sourceType, long sourceId)
        {
            switch (sourceType)
            {
                case SourceTypeSelectVM.Document:
                    return urlHelper.Action("View", "Document", new { id = sourceId, tab = "tabname" }).Replace("tabname", "#tabWorkTask", StringComparison.InvariantCultureIgnoreCase);
                case SourceTypeSelectVM.DocumentResolution:
                    return urlHelper.Action("Edit", "DocumentResolution", new { id = sourceId, tab = "tabname" }).Replace("tabname", "#tabWorkTask", StringComparison.InvariantCultureIgnoreCase);
                case SourceTypeSelectVM.Case:
                    return urlHelper.Action("CasePreview", "Case", new { id = sourceId });
                case SourceTypeSelectVM.CaseSession:
                    return urlHelper.Action("Preview", "CaseSession", new { id = sourceId });
                case SourceTypeSelectVM.CasePerson:
                    return urlHelper.Action("Edit", "CasePerson", new { id = sourceId });
                case SourceTypeSelectVM.CaseLawUnit:
                    return urlHelper.Action("Edit", "CaseLawUnit", new { id = sourceId });
                case SourceTypeSelectVM.CaseNotification:
                    return urlHelper.Action("Edit", "CaseNotification", new { id = sourceId });
                case SourceTypeSelectVM.CaseSessionAct:
                    return urlHelper.Action("Edit", "CaseSessionAct", new { id = sourceId, tab = "tabname" }).Replace("tabname", "#tabWorkTask", StringComparison.InvariantCultureIgnoreCase);
                case SourceTypeSelectVM.CaseSessionActCoordination:
                    return urlHelper.Action("Edit", "CaseSessionActCoordination", new { id = sourceId });
                case SourceTypeSelectVM.ExecList:
                    return urlHelper.Action("EditExecList", "Money", new { id = sourceId, tab = "tabname" }).Replace("tabname", "#tabWorkTask", StringComparison.InvariantCultureIgnoreCase);
                default:
                    return string.Empty;
            }
        }

        public string GetTaskParentObjectUrl(int sourceType, long sourceId)
        {
            string result = string.Empty;
            int? caseId = null;

            switch (sourceType)
            {
                case SourceTypeSelectVM.Case:
                    caseId = (int)sourceId;
                    break;
                case SourceTypeSelectVM.CaseNotification:
                    caseId = repo.GetById<CaseNotification>((int)sourceId)?.CaseId;
                    break;
                case SourceTypeSelectVM.CaseSessionAct:
                    caseId = repo.GetById<CaseSessionAct>((int)sourceId)?.CaseId;
                    break;
                case SourceTypeSelectVM.CaseSessionActCoordination:
                    caseId = repo.GetById<CaseSessionActCoordination>((int)sourceId)?.CaseId;
                    break;
                case SourceTypeSelectVM.ExecList:
                    caseId = repo.AllReadonly<ExecList>().Where(x => x.Id == (int)sourceId)
                        .Select(x => x.ExecListObligations.Select(a => a.Obligation.CaseId).FirstOrDefault())
                        .FirstOrDefault();
                    break;
                case SourceTypeSelectVM.DocumentResolution:
                    caseId = repo.AllReadonly<DocumentResolution>()
                                    .Include(x => x.Document)
                                    .ThenInclude(x => x.Cases)
                                    .Where(x => x.Id == sourceId)
                                    .Select(x => x.Document.Cases.Select(c => c.Id).FirstOrDefault())
                                    .FirstOrDefault();
                    break;
            }
            if (caseId > 0)
            {
                return urlHelper.Action("CasePreview", "Case", new { id = caseId });
            }
            return null;

        }


        private void SetDoActionUrl(WorkTaskVM model)
        {
            switch (model.TaskTypeId)
            {
                case WorkTaskConstants.Types.Case_SelectLawUnit:
                case WorkTaskConstants.Types.Case_ForReject:
                    model.DoActionUrl = urlHelper.Action("DoTask_Case_SelectLawUnit", "WorkTask", new { id = model.Id });
                    break;
                case WorkTaskConstants.Types.CaseSessionAct_SentToCoordinate:
                    model.DoActionUrl = urlHelper.Action("DoTask_SentForCoordinate", "CaseSessionAct", new { id = model.Id });
                    model.DoActionWarning = "След изпращане за съгласуване ще се създадат необходимите задачи към всички лица.";
                    break;
                case WorkTaskConstants.Types.CaseSessionAct_SentToSign:
                    model.DoActionUrl = urlHelper.Action("DoTask_SentForSign", "CaseSessionAct", new { id = model.Id });
                    model.DoActionWarning = "След изпращане за подписване, съдебният акт ще бъде регистриран и ще получи номер. Ако съществува вече създаден документ по акта, той ще бъде презареден от актуалната бланка на акта.";
                    break;
                case WorkTaskConstants.Types.CaseSessionActMotives_SentToSign:
                    model.DoActionUrl = urlHelper.Action("DoTask_MotivesSentForSign", "CaseSessionAct", new { id = model.Id });
                    model.DoActionWarning = "Потвърдете изпращането за подпис на мотиви към съдебен акт!";
                    break;
                case WorkTaskConstants.Types.CaseSessionAct_Sign:
                    model.DoActionUrl = urlHelper.Action("SendActForSign", "CaseSessionAct", new { id = model.SourceId, taskId = model.Id });
                    break;
                case WorkTaskConstants.Types.CaseSessionActCoordination_Sign:
                    model.DoActionUrl = urlHelper.Action("SendActForSignCoordination", "CaseSessionAct", new { id = model.SourceId, coordinationId = model.SubSourceId ?? 0, taskId = model.Id });
                    break;
                case WorkTaskConstants.Types.CaseSessionActMotives_Sign:
                    model.DoActionUrl = urlHelper.Action("SendActForSignMotives", "CaseSessionAct", new { id = model.SourceId, taskId = model.Id });
                    break;
                case WorkTaskConstants.Types.Document_Sign:
                    model.DoActionUrl = urlHelper.Action("SendDocumentForSign", "Document", new { id = model.SourceId, taskId = model.Id });
                    break;
                case WorkTaskConstants.Types.DocumentDecision:
                    model.DoActionUrl = urlHelper.Action("AddDocumentDecision", "Document", new { documentId = model.SourceId });
                    break;
                case WorkTaskConstants.Types.ForDocumentResolution:
                    model.DoActionUrl = urlHelper.Action("Add", "DocumentResolution", new { documentId = model.SourceId });
                    break;
                case WorkTaskConstants.Types.DocumentResolution_SentToSign:
                    model.DoActionUrl = urlHelper.Action("DoTask_SentForSign", "DocumentResolution", new { id = model.Id });
                    model.DoActionWarning = "Ако съществува вече създаден документ, той ще бъде презареден от актуалната бланка.";
                    break;
                case WorkTaskConstants.Types.DocumentResolution_Sign:
                    model.DoActionUrl = urlHelper.Action("SendForSign", "DocumentResolution", new { id = model.SourceId, taskId = model.Id });
                    break;
                case WorkTaskConstants.Types.ExecList_SentToSign:
                    model.DoActionUrl = urlHelper.Action("DoTask_SentForSign", "Money", new { id = model.Id });
                    model.DoActionWarning = "Ако съществува вече създаден документ, той ще бъде презареден от актуалната бланка.";
                    break;
                case WorkTaskConstants.Types.ExecList_Sign:
                    model.DoActionUrl = urlHelper.Action("SendForSign", "Money", new { id = model.SourceId, taskId = model.Id });
                    break;
                case WorkTaskConstants.Types.SendFor_NewSession:
                    {
                        var _case = repo.GetById<Case>((int)model.SourceId);
                        model.DoActionUrl = urlHelper.Action("Add", "DocumentResolution", new { documentId = _case.DocumentId });
                    }
                    break;
                default:
                    model.DoActionUrl = null;
                    break;
            }
        }
        public WorkTask Select_ById(long id)
        {
            return repo.AllReadonly<WorkTask>()
                            .Include(x => x.TaskType)
                            .Include(x => x.UserCreated)
                            .ThenInclude(x => x.LawUnit)
                            .Where(x => x.Id == id)
                            .FirstOrDefault();
        }
        public WorkTaskEditVM Get_ById(long id)
        {
            return repo.AllReadonly<WorkTask>()
                             .Include(x => x.TaskType)
                             .Include(x => x.UserCreated)
                             .ThenInclude(x => x.LawUnit)
                             .Where(x => x.Id == id)
                             .Select(x => new WorkTaskEditVM
                             {
                                 Id = x.Id,
                                 SourceType = x.SourceType,
                                 SourceId = x.SourceId,
                                 SubSourceId = x.SubSourceId,
                                 ParentTaskId = x.ParentTaskId,
                                 TaskTypeId = x.TaskTypeId,
                                 TaskTypeName = x.TaskType.Label,
                                 TaskExecutionId = x.TaskExecutionId,
                                 UserId = x.UserId,
                                 CourtOrganizationId = x.CourtOrganizationId,
                                 DateEnd = x.DateEnd,
                                 DescriptionCreated = x.DescriptionCreated
                             })
                             .FirstOrDefault();
        }
        public bool CreateTask(WorkTaskEditVM model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.UserCreatedId))
                {
                    model.UserCreatedId = null;
                }

                var entity = new WorkTask();
                model.ToEntity(entity);

                entity.CourtId = userContext.CourtId;
                entity.DateCreated = DateTime.Now;
                entity.UserCreatedId = model.UserCreatedId ?? userContext.UserId;
                entity.TaskStateId = WorkTaskConstants.States.New;
                if (!model.DisableSelfAcceptCheck)
                {
                    var taskType = repo.GetById<TaskType>(model.TaskTypeId);
                    if (taskType.SelfTask == true || entity.UserId == userContext.UserId)
                    {
                        entity.UserId = userContext.UserId;
                        entity.DateAccepted = DateTime.Now;
                        entity.TaskStateId = WorkTaskConstants.States.Accepted;
                    }
                }
                CreateTaskSourceDescription(entity);
                repo.Add(entity);
                repo.SaveChanges();
                autoCompleteTasks(entity);
                model.Id = entity.Id;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }
        private void autoCompleteTasks(WorkTask task)
        {
            if (!WorkTaskConstants.Types.AutoCompleteTasks.Contains(task.TaskTypeId))
            {
                return;
            }

            if (CompleteTask(task))
            {
                UpdateAfterCompleteTask(task);
            }
        }
        public bool UpdateTask(WorkTaskEditVM model)
        {
            try
            {
                var entity = repo.GetById<WorkTask>(model.Id);
                model.ToEntity(entity);
                repo.Update(entity);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }

        private void CreateTaskSourceDescription(WorkTask model)
        {
            switch (model.SourceType)
            {
                case SourceTypeSelectVM.Document:
                    {
                        var info = repo.AllReadonly<Document>()
                                        .Include(x => x.DocumentType)
                                        .Include(x => x.DocumentCaseInfo)
                                        .ThenInclude(x => x.Case)
                                        .Where(x => x.Id == model.SourceId)
                                        .Select(x => new
                                        {
                                            sd = $"{x.DocumentType.Label} {x.DocumentNumber}/{x.DocumentDate:dd.MM.yyyy}",
                                            pdObject = x.DocumentCaseInfo.FirstOrDefault()
                                        })
                                        .FirstOrDefault();
                        model.SourceDescription = info.sd;
                        if (info.pdObject != null)
                        {
                            if (info.pdObject.Case != null)
                            {
                                model.ParentDescription = info.pdObject.Case.RegNumber;
                            }
                            else
                            {
                                model.ParentDescription = info.pdObject.CaseRegNumber;
                            }
                        }
                    }
                    break;
                case SourceTypeSelectVM.DocumentResolution:
                    {
                        var info = repo.AllReadonly<DocumentResolution>()
                                        .Include(x => x.Document)
                                       .ThenInclude(x => x.DocumentType)
                                       .Include(x => x.Document)
                                       .ThenInclude(x => x.Cases)
                                       .Where(x => x.Id == model.SourceId)
                                       .Select(x => new
                                       {
                                           sd = $" към {x.Document.DocumentType.Label} {x.Document.DocumentNumber}/{x.Document.DocumentDate:dd.MM.yyyy}",
                                           pd = (x.Document.Cases.Any()) ? x.Document.Cases.Select(c => c.RegNumber).FirstOrDefault() : ""
                                       })
                                       .FirstOrDefault();
                        model.SourceDescription = info.sd;
                        model.ParentDescription = info.pd;
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionAct:
                    {
                        var info = repo.AllReadonly<CaseSessionAct>()
                                        .Include(x => x.ActType)
                                        .Include(x => x.Case)
                                        .Where(x => x.Id == (int)model.SourceId)
                                        .Select(x => new
                                        {
                                            sd = $"{x.ActType.Label} {x.RegNumber}/{x.RegDate:dd.MM.yyyy}",
                                            pd = x.Case.RegNumber
                                        })
                                        .FirstOrDefault();

                        model.SourceDescription = info.sd;
                        model.ParentDescription = info.pd;
                    }
                    break;
                case SourceTypeSelectVM.ExecList:
                    {
                        var info = repo.AllReadonly<Infrastructure.Data.Models.Money.ExecList>()
                                        .Where(x => x.Id == (int)model.SourceId)
                                        .Select(x => new
                                        {
                                            sd = $"{x.ExecListType.Label} {x.RegNumber}/{x.RegDate:dd.MM.yyyy}",
                                            pd = x.ExecListObligations.Select(a => a.Obligation.Case.RegNumber).FirstOrDefault(),
                                        })
                                        .FirstOrDefault();

                        model.SourceDescription = info.sd;
                        model.ParentDescription = info.pd;
                    }
                    break;
                case SourceTypeSelectVM.Case:
                    {
                        var info = repo.AllReadonly<Case>()
                                           .Include(x => x.CaseType)
                                           .Where(x => x.Id == (int)model.SourceId)
                                           .Select(x => new
                                           {
                                               sd = $"{x.CaseType.Code} {x.ShortNumber}/{x.RegDate:dd.MM.yyyy}",
                                               pd = x.RegNumber
                                           })
                                           .FirstOrDefault();

                        model.SourceDescription = info.sd;
                        model.ParentDescription = info.pd;
                    }
                    break;
            }
        }

        public bool AcceptTask(long id)
        {
            try
            {
                var model = repo.GetById<WorkTask>(id);

                if (!ValidateSourceCourt(model.SourceType, model.SourceId))
                {
                    return false;
                }

                if (model.DateAccepted != null)
                {
                    return false;
                }
                if (!string.IsNullOrEmpty(model.UserId) && (model.UserId != userContext.UserId) && !userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator))
                {
                    return false;
                }
                if (model.CourtOrganizationId > 0)
                {
                    model.UserId = userContext.UserId;
                }
                model.DateAccepted = DateTime.Now;
                model.TaskStateId = WorkTaskConstants.States.Accepted;
                repo.Update(model);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }
        public bool CompleteTask(long id)
        {
            return CompleteTask(new WorkTask() { Id = id });
        }

        public bool CompleteTask(WorkTask model)
        {
            try
            {
                var saved = repo.GetById<WorkTask>(model.Id);
                saved.TaskActionId = model.TaskActionId;
                saved.Description = model.Description;
                saved.DateCompleted = DateTime.Now;
                saved.TaskStateId = WorkTaskConstants.States.Completed;
                CreateTaskSourceDescription(saved);
                CompleteTask_UpdateOthers(saved);
                repo.Update(saved);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }

        private void CompleteTask_UpdateOthers(WorkTask model)
        {

            Expression<Func<WorkTask, bool>> selectToDelete = x => false;
            switch (model.TaskTypeId)
            {
                case WorkTaskConstants.Types.CaseSessionAct_SentToSign:
                    {

                        selectToDelete = x =>
                            (x.TaskTypeId == WorkTaskConstants.Types.CaseSessionAct_SentToSign) ||
                            (x.TaskTypeId == WorkTaskConstants.Types.CaseSessionAct_Sign) ||
                            (x.TaskTypeId == WorkTaskConstants.Types.CaseSessionActCoordination_Sign);
                    }
                    break;
                case WorkTaskConstants.Types.CaseSessionActMotives_SentToSign:
                    {

                        selectToDelete = x =>
                            (x.TaskTypeId == WorkTaskConstants.Types.CaseSessionActMotives_SentToSign) ||
                            (x.TaskTypeId == WorkTaskConstants.Types.CaseSessionActMotives_Sign);
                    }
                    break;
                case WorkTaskConstants.Types.DocumentResolution_SentToSign:
                    {
                        selectToDelete = x =>
                            (x.TaskTypeId == WorkTaskConstants.Types.DocumentResolution_SentToSign) ||
                            (x.TaskTypeId == WorkTaskConstants.Types.DocumentResolution_Sign);
                    }
                    break;
                case WorkTaskConstants.Types.ExecList_SentToSign:
                    {
                        selectToDelete = x =>
                            (x.TaskTypeId == WorkTaskConstants.Types.ExecList_SentToSign) ||
                            (x.TaskTypeId == WorkTaskConstants.Types.ExecList_Sign);
                    }
                    break;
            }

            //Отменя всички предходни неприключили задачи за подписване
            var signTasks = repo.All<WorkTask>().Where(x =>
                           x.SourceId == model.SourceId
                           && x.SourceType == x.SourceType
                           && WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId)
                           && (x.ParentTaskId ?? 0) != model.Id
                           && x.Id < model.Id
                            )
                            .Where(selectToDelete)
                            .ToList();
            foreach (var item in signTasks)
            {
                item.TaskStateId = WorkTaskConstants.States.Deleted;
            }
        }
        public SaveResultVM UpdateAfterCompleteTask(WorkTask model, object additionalService = null)
        {
            switch (model.TaskTypeId)
            {
                case WorkTaskConstants.Types.CaseSessionAct_Coordinate:
                    {
                        var hasUnCompletedCoTasks = repo.AllReadonly<WorkTask>()
                                                            .Where(x => x.ParentTaskId == model.ParentTaskId)
                                                            .Where(x => x.TaskStateId != WorkTaskConstants.States.Completed)
                                                            .Where(x => x.TaskTypeId == model.TaskTypeId)
                                                            .Any();
                        var actModel = repo.GetById<CaseSessionAct>((int)model.SourceId);
                        if (!hasUnCompletedCoTasks && actModel.ActStateId < NomenclatureConstants.SessionActState.Coordinated)
                        {
                            actModel.ActStateId = NomenclatureConstants.SessionActState.Coordinated;
                            repo.Update(actModel);
                            repo.SaveChanges();
                            return new SaveResultVM(true, "", "reload");
                        }
                    }
                    break;

                case WorkTaskConstants.Types.CaseSessionAct_Sign:
                    {
                        var signTasks = repo.AllReadonly<WorkTask>().Where(x => x.ParentTaskId == model.ParentTaskId).ToList();
                        var hasUnCompletedCoTasks = signTasks.Where(x => x.TaskStateId != WorkTaskConstants.States.Completed)
                                                             .Where(x => x.TaskStateId != WorkTaskConstants.States.Deleted)
                                                             .Where(x => x.TaskTypeId == model.TaskTypeId)
                                                             .Any();

                        var actModel = repo.GetById<CaseSessionAct>((int)model.SourceId);
                        if (!hasUnCompletedCoTasks)
                        {
                            if (actModel.ActDeclaredDate == null)
                            {
                                actModel.ActStateId = NomenclatureConstants.SessionActState.Enforced;
                                actModel.ActDeclaredDate = signTasks.OrderByDescending(x => x.DateCompleted).Select(x => x.DateCompleted).FirstOrDefault();
                                actModel.DateWrt = DateTime.Now;

                                //Автоматично влизане в сила на актове, неподлежащи на обжалване
                                if (actModel.CanAppeal == false)
                                {
                                    actModel.ActStateId = NomenclatureConstants.SessionActState.ComingIntoForce;
                                    actModel.ActInforcedDate = actModel.ActDeclaredDate;
                                }
                                repo.Update(actModel);
                                repo.SaveChanges();
                                using (TransactionScope ts = TransactionScopeBuilder.CreateReadCommitted())
                                {
                                    caseDeadlineService.DeadLineMotive(actModel);
                                    caseDeadlineService.DeadLineDeclaredForResolveComplete(actModel);
                                    // Автоматизиране на статус - решено
                                    var caseCase = repo.GetById<Case>(actModel.CaseId);
                                    if ((caseCase.CaseStateId == NomenclatureConstants.CaseState.AnnouncedForResolution) && (actModel.ActTypeId == NomenclatureConstants.ActType.Answer))
                                    {
                                        caseCase.CaseStateId = NomenclatureConstants.CaseState.Resolution;
                                        caseCase.DateWrt = DateTime.Now;
                                        caseCase.UserId = userContext.UserId;
                                        repo.Update(caseCase);
                                        caseDeadlineService.DeadLineOnCase(caseCase);
                                    }

                                    repo.SaveChanges();

                                    lifecycleService.CaseLifecycle_SaveFirst_ForCaseType(actModel.CaseSessionId);

                                    if (actModel.IsFinalDoc)
                                    {
                                        lifecycleService.CaseLifecycle_CloseInterval(actModel.CaseId ?? 0, actModel.Id, actModel.ActDeclaredDate ?? DateTime.Now);
                                    }

                                    mqEpepService.AppendCaseSessionAct(actModel, EpepConstants.ServiceMethod.Add);
                                    mqEpepService.AppendCaseSessionAct_Private(actModel.Id, EpepConstants.ServiceMethod.Add);

                                    caseLoadIndexService.CaseLoadIndexAutomationElementGroupe_SRA_SaveData(actModel.CaseSessionId);

                                    ts.Complete();
                                    return new SaveResultVM(true, "", "reload");
                                }
                            }
                            else
                            {
                                //ако вече е постановен - само изпраща новите версии към ЕПЕП
                                mqEpepService.AppendCaseSessionAct(actModel, EpepConstants.ServiceMethod.Update);
                                mqEpepService.AppendCaseSessionAct_Private(actModel.Id, EpepConstants.ServiceMethod.Update);
                            }
                        }
                    }
                    break;
                case WorkTaskConstants.Types.CaseSessionActMotives_Sign:
                    {
                        var motivesTasks = repo.AllReadonly<WorkTask>().Where(x => x.ParentTaskId == model.ParentTaskId).ToList();
                        var hasUnCompletedCoTasks = motivesTasks.Where(x => x.TaskStateId != WorkTaskConstants.States.Completed)
                                                        .Where(x => x.TaskTypeId == model.TaskTypeId)
                                                        .Any();
                        var actModel = repo.GetById<CaseSessionAct>((int)model.SourceId);
                        if (!hasUnCompletedCoTasks && actModel.ActMotivesDeclaredDate == null)
                        {
                            using (TransactionScope ts = TransactionScopeBuilder.CreateReadCommitted())
                            {
                                actModel.ActMotivesDeclaredDate = motivesTasks.OrderByDescending(x => x.DateCompleted).Select(x => x.DateCompleted).FirstOrDefault();
                                caseDeadlineService.DeadLineMotive(actModel);
                                repo.Update(actModel);
                                repo.SaveChanges();
                                mqEpepService.AppendCaseSessionAct_PrivateMotive(actModel.Id, EpepConstants.ServiceMethod.Add);
                                caseLoadIndexService.CaseLoadIndexAutomationElementGroupe_SRA_SaveData(actModel.CaseSessionId);
                                ts.Complete();
                                return new SaveResultVM(true, "", "reload");
                            }
                        }
                    }
                    break;
                case WorkTaskConstants.Types.DocumentResolution_Sign:
                    {
                        var signTasks = repo.AllReadonly<WorkTask>().Where(x => x.ParentTaskId == model.ParentTaskId).ToList();
                        var hasUnCompletedCoTasks = signTasks.Where(x => x.TaskStateId != WorkTaskConstants.States.Completed)
                                                        .Where(x => x.TaskTypeId == model.TaskTypeId)
                                                        .Any();

                        var resolutionModel = repo.AllReadonly<DocumentResolution>()
                                                        .Include(x => x.ResolutionType)
                                                        .Where(x => x.Id == model.SourceId)
                                                        .FirstOrDefault(); ;
                        if (!hasUnCompletedCoTasks && (resolutionModel.DeclaredDate == null))
                        {
                            using (TransactionScope ts = TransactionScopeBuilder.CreateReadCommitted())
                            {
                                resolutionModel.ResolutionStateId = NomenclatureConstants.ResolutionStates.Enforced;
                                resolutionModel.DeclaredDate = signTasks.OrderByDescending(x => x.DateCompleted).Select(x => x.DateCompleted).FirstOrDefault();
                                resolutionModel.DateWrt = DateTime.Now;

                                repo.Update(resolutionModel);
                                repo.SaveChanges();

                                if (resolutionModel.ResolutionTypeId == DocumentConstants.ResolutionTypes.ResolutionForSelection)
                                {
                                    var cases = repo.AllReadonly<DocumentResolutionCase>().Where(x => x.DocumentResolutionId == resolutionModel.Id)
                                                        .Select(x => x.CaseId).ToList();
                                    foreach (var caseId in cases)
                                    {
                                        var newTask = new WorkTaskEditVM()
                                        {
                                            SourceType = SourceTypeSelectVM.Case,
                                            SourceId = caseId,
                                            TaskTypeId = WorkTaskConstants.Types.SendFor_NewSelection,
                                            UserCreatedId = resolutionModel.UserDecisionId,
                                            UserId = resolutionModel.TaskUserId,
                                            TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser,
                                            DescriptionCreated = $"{resolutionModel.ResolutionType.Label} {resolutionModel.RegNumber}/{resolutionModel.RegDate:dd.MM.yyyy}"
                                        };

                                        CreateTask(newTask);
                                    }
                                }

                                ts.Complete();
                                return new SaveResultVM(true, "", "reload");
                            }
                        }
                    }
                    break;

                case WorkTaskConstants.Types.SendFor_Competency:
                    {
                        int[] tasksToDelete = { WorkTaskConstants.Types.DocumentResolution_SentToSign, WorkTaskConstants.Types.DocumentResolution_Sign };
                        var unCompletedTasks = repo.All<WorkTask>()
                                                            .Where(x => x.SourceType == model.SourceType)
                                                            .Where(x => x.SourceId == model.SourceId)
                                                            .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                                                            .Where(x => tasksToDelete.Contains(x.TaskTypeId))
                                                            .ToList();
                        if (unCompletedTasks.Any())
                        {
                            foreach (var item in unCompletedTasks)
                            {
                                item.TaskStateId = WorkTaskConstants.States.Deleted;
                            }
                            repo.SaveChanges();
                        }
                    }
                    break;
            }
            return new SaveResultVM(false);
        }
        public List<SelectListItem> GetDDL_TaskActions(int taskTypeId)
        {
            return repo.AllReadonly<TaskAction>()
                            .Where(x => x.TaskTypeId == taskTypeId)
                            .ToSelectList();
        }

        public int GetCaseIdByDocTaskId(long taskId)
        {
            try
            {
                var task = repo.GetById<WorkTask>(taskId);
                long docId = task.SourceId;
                if (task.SourceType == SourceTypeSelectVM.DocumentResolution)
                {
                    var docRes = repo.GetById<DocumentResolution>(task.SourceId);
                    docId = docRes.DocumentId;
                }
                var caseModel = repo.AllReadonly<Case>(x => x.DocumentId == docId).FirstOrDefault();
                if (caseModel != null)
                {
                    return caseModel.Id;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);

            }
            return 0;
        }

        public bool RedirectTask(WorkTaskEditVM model)
        {
            try
            {
                var currentTask = repo.GetById<WorkTask>(model.Id);

                model.DescriptionCreated = currentTask.DescriptionCreated;
                if (!string.IsNullOrEmpty(currentTask.DescriptionCreated) && !string.IsNullOrEmpty(model.DescriptionRedirect))
                {
                    model.DescriptionCreated += ";" + model.DescriptionRedirect;
                }

                if (CreateTask(model))
                {
                    currentTask.TaskStateId = WorkTaskConstants.States.Redirected;
                    currentTask.DateCompleted = DateTime.Now;
                    repo.Update(currentTask);
                    repo.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
            return false;
        }

        public List<SelectListItem> GetDDL_TaskTypes(int sourceType, long sourceId = 0)
        {
            Expression<Func<TaskType, bool>> sourceIdWhere = x => true;
            switch (sourceType)
            {
                case SourceTypeSelectVM.Document:
                    {
                        var _docKind = repo.AllReadonly<Document>()
                                        .Include(x => x.DocumentGroup)
                                        .Where(x => x.Id == sourceId)
                                        .Select(x => x.DocumentGroup.DocumentKindId)
                                        .FirstOrDefault();
                        switch (_docKind)
                        {
                            case DocumentConstants.DocumentKind.InitialDocument:
                                int[] initialDocOnlyTasks = { WorkTaskConstants.Types.DocumentDecision };
                                sourceIdWhere = x => !initialDocOnlyTasks.Contains(x.Id);
                                break;
                            case DocumentConstants.DocumentKind.CompliantDocument:
                                int[] compliantDocOnlyTasks = { WorkTaskConstants.Types.Case_SelectLawUnit, WorkTaskConstants.Types.Case_ForReject };
                                sourceIdWhere = x => !compliantDocOnlyTasks.Contains(x.Id);
                                break;
                            case DocumentConstants.DocumentKind.InAdministrationDocument:
                                int[] inAdministrationDocAndReportOnlyTasks = { WorkTaskConstants.Types.Case_SelectLawUnit, WorkTaskConstants.Types.Case_ForReject, WorkTaskConstants.Types.ForReport };
                                sourceIdWhere = x => !inAdministrationDocAndReportOnlyTasks.Contains(x.Id);
                                break;
                            default:
                                int[] initialDocAndReportOnlyTasks = { WorkTaskConstants.Types.Case_SelectLawUnit, WorkTaskConstants.Types.Case_ForReject, WorkTaskConstants.Types.ForReport, WorkTaskConstants.Types.DocumentDecision };
                                sourceIdWhere = x => !initialDocAndReportOnlyTasks.Contains(x.Id);
                                break;

                        }
                    }
                    break;
                case SourceTypeSelectVM.DocumentResolution:
                    var hasCase = repo.AllReadonly<DocumentResolution>()
                                    .Include(x => x.Document)
                                    .ThenInclude(x => x.Cases)
                                    .Where(x => x.Id == sourceId)
                                    .Where(x => x.Document.Cases != null)
                                    .Select(x => x.Document)
                                    .SelectMany(x => x.Cases)
                                    .Any();
                    if (!hasCase)
                    {
                        int[] caseTaskForDocumentResolution = { WorkTaskConstants.Types.Case_SelectLawUnit, WorkTaskConstants.Types.Case_ForReject };
                        sourceIdWhere = x => !caseTaskForDocumentResolution.Contains(x.Id);
                    }
                    break;
            }
            return repo.AllReadonly<TaskTypeSourceType>()
                                .Include(x => x.TaskType)
                                .Where(x => x.SourceType == sourceType)
                                .Select(x => x.TaskType)
                                .Where(sourceIdWhere)
                                .Where(x => x.AutomatedTask == false)
                                .OrderBy(x => x.OrderNumber)
                                .ToSelectList();
        }

        public int[] GetSelfTask()
        {
            return repo.AllReadonly<TaskType>()
                    .Where(x => x.SelfTask == true)
                    .Select(x => x.Id)
                    .ToArray();
        }

        public LawUnit GetLawUnitByTaskId(long id)
        {
            return repo.AllReadonly<WorkTask>()
                            .Where(x => x.Id == id)
                            .Select(x => x.User.LawUnit)
                            .FirstOrDefault();
        }

        public bool ValidateSourceCourt(int sourceType, long sourceId)
        {
            int? sourceCourtId = GetSourceCourtId(sourceType, sourceId);
            return (sourceCourtId == userContext.CourtId) || (sourceCourtId == null);
        }

        public int? GetSourceCourtId(int sourceType, long sourceId)
        {
            switch (sourceType)
            {
                case SourceTypeSelectVM.Document:
                    return repo.AllReadonly<Document>().Where(x => x.Id == sourceId).Select(x => x.CourtId).FirstOrDefault();
                case SourceTypeSelectVM.CaseSessionAct:
                    return repo.AllReadonly<CaseSessionAct>().Where(x => x.Id == (int)sourceId).Select(x => x.CourtId).FirstOrDefault();
                case SourceTypeSelectVM.Case:
                    return repo.AllReadonly<Case>().Where(x => x.Id == (int)sourceId).Select(x => x.CourtId).FirstOrDefault();
                case SourceTypeSelectVM.DocumentResolution:
                    return repo.AllReadonly<DocumentResolution>().Where(x => x.Id == sourceId).Select(x => x.CourtId).FirstOrDefault();
                default:
                    return null;
            }
        }

        public WorkTaskEditVM InitTask(int sourceType, long sourceId)
        {
            var model = new WorkTaskEditVM()
            {
                SourceType = sourceType,
                SourceId = sourceId,
                TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser
            };

            switch (sourceType)
            {
                case SourceTypeSelectVM.Document:
                    var documentCaseInfo = repo.AllReadonly<Document>()
                                        .Include(x => x.DocumentCaseInfo)
                                        .Where(x => x.Id == sourceId)
                                        .Where(x => x.DocumentCaseInfo.Any())
                                        .Select(x => x.DocumentCaseInfo.FirstOrDefault())
                                        .FirstOrDefault();
                    //Ако в документа има свързано дело от същия съд, задачата се насочва по подразбиране на съдия-докладчика на делото
                    if (documentCaseInfo != null && documentCaseInfo.CourtId == userContext.CourtId && documentCaseInfo.CaseId > 0)
                    {
                        var judgeReporterUserId = repo.AllReadonly<CaseLawUnit>()
                                                        .Where(x => x.CaseId == documentCaseInfo.CaseId && x.CaseSessionId == null)
                                                        .Where(x => (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                                        .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                        .Select(x => x.LawUnitUserId)
                                                        .FirstOrDefault();
                        model.UserId = judgeReporterUserId;
                    }
                    break;
            }

            return model;
        }

        public bool ExpireAllUnfinishedTasks(int sourceType, long sourceId)
        {
            var tasks = repo.All<WorkTask>()
                             .Where(x => x.SourceType == sourceType && x.SourceId == sourceId)
                             .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                             .ToList();
            if (tasks.Any())
            {
                foreach (var item in tasks)
                {
                    item.TaskStateId = WorkTaskConstants.States.Deleted;
                }
                repo.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ExpireTasks(long[] taskIds, string description)
        {
            var tasks = repo.All<WorkTask>()
                            .Where(x => taskIds.Contains(x.Id))
                            .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                            .ToList();

            if (!tasks.Any())
            {
                return false;
            }
            foreach (var task in tasks)
            {
                task.TaskStateId = WorkTaskConstants.States.Deleted;
                task.DescriptionCreated = (task.DescriptionCreated ?? "");
                task.DescriptionCreated += $"; Отменена на {DateTime.Now:dd.MM.yyyy HH:mm:ss} от {userContext.FullName};{description}";
                repo.Update(task);
            }

            repo.SaveChanges();
            return true;

        }

        public bool RerouteTasks(long[] taskIds, WorkTaskManageVM model)
        {
            var tasks = repo.All<WorkTask>()
                             .Where(x => taskIds.Contains(x.Id))
                             .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                             .Where(x => !WorkTaskConstants.Types.TaskCantReroute.Contains(x.TaskTypeId))
                             .ToList();

            if (!tasks.Any())
            {
                return false;
            }
            switch (model.TaskExecutionId)
            {
                case WorkTaskConstants.TaskExecution.ByUser:
                    model.CourtOrganizationId = null;
                    break;
                case WorkTaskConstants.TaskExecution.ByOrganization:
                    model.NewUserId = null;
                    break;
            }
            foreach (var task in tasks)
            {

                var newTask = new WorkTaskEditVM()
                {
                    DescriptionCreated = task.DescriptionCreated,
                    ParentTaskId = task.ParentTaskId,
                    SourceType = task.SourceType,
                    SourceId = task.SourceId,
                    TaskTypeId = task.TaskTypeId,
                    TaskExecutionId = model.TaskExecutionId,
                    DateEnd = task.DateEnd,
                    UserId = model.NewUserId,
                    CourtOrganizationId = model.CourtOrganizationId,
                    DisableSelfAcceptCheck = true
                };
                if (CreateTask(newTask))
                {

                    task.TaskStateId = WorkTaskConstants.States.Redirected;
                    task.DescriptionCreated = (task.DescriptionCreated ?? "");
                    task.DescriptionCreated += $"; Пренасочена на {DateTime.Now:dd.MM.yyyy HH:mm:ss} от {userContext.FullName};{model.Description}";
                    repo.Update(task);
                }
            }

            repo.SaveChanges();
            return true;
        }


    }
}
