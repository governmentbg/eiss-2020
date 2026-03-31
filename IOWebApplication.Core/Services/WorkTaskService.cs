using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class WorkTaskService : BaseService, IWorkTaskService
    {
        private readonly IUrlHelper urlHelper;
        private readonly ICaseLifecycleService lifecycleService;
        private readonly IMQEpepService mqEpepService;
        private readonly ICaseDeadlineService caseDeadlineService;
        private readonly IWorkNotificationService workNotificationService;
        private readonly ICaseLoadIndexService caseLoadIndexService;
        private readonly ICaseSessionActCoordinationService coordinationService;
        private readonly ICommonService commonService;
        private readonly ICdnService cdnService;
        private readonly ILazybleService<IMoneyService> lazyMoneyService;

        public WorkTaskService(ILogger<WorkTaskService> _logger,
                               IRepository _repo,
                               IReadonlyRepository _readonlyrepo,
                               IUserContext _userContext,
                               ICaseLifecycleService _lifecycleService,
                               IMQEpepService _mqEpepService,
                               ICaseDeadlineService _caseDeadlineService,
                               ICaseLoadIndexService _caseLoadIndexService,
                               ICaseSessionActCoordinationService _coordinationService,
                               ICdnService _cdnService,
                               ICommonService _commonService,
                               IWorkNotificationService _workNotificationService,
                               ILazybleService<IMoneyService> _lazyMoneyService,
                               IUrlHelper _url)
        {
            logger = _logger;
            repo = _repo;
            readonlyrepo = _readonlyrepo;
            userContext = _userContext;
            lifecycleService = _lifecycleService;
            mqEpepService = _mqEpepService;
            caseDeadlineService = _caseDeadlineService;
            urlHelper = _url;
            caseLoadIndexService = _caseLoadIndexService;
            coordinationService = _coordinationService;
            cdnService = _cdnService;
            commonService = _commonService;
            workNotificationService = _workNotificationService;
            lazyMoneyService = _lazyMoneyService;
        }

        public async Task<WorkTask> ReadById(long id)
        {
            return await repo.All<WorkTask>().Where(x => x.Id == id).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        private IQueryable<WorkTaskVM> selectTasks(bool showToDo, bool showMyTasks, int sourceType, long sourceId, bool customFilter = false)
        {
            int?[] _userOrganizations = userContext.CourtOrganizations.Select(x => (int?)x).ToArray();

            Expression<Func<WorkTask, bool>> whereSelect = x => x.SourceType == sourceType && x.SourceId == sourceId;
            if (showToDo)
            {
                whereSelect = x => (x.TaskStateId == WorkTaskConstants.States.New && (x.UserId == userContext.UserId || _userOrganizations.Contains(x.CourtOrganizationId))
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

        public IQueryable<WorkTaskVM> Select(WorkTaskFilterVM filter)
        {
            filter.Sanitize();

            var _userOrganizations = userContext.CourtOrganizations.Select(x => (int?)x).ToArray();

            Expression<Func<WorkTaskVM, bool>> whereSelect = x => true;
            if (filter.UserMode == 1)
            {
                whereSelect = x => (
                                    (x.TaskStateId == WorkTaskConstants.States.New && _userOrganizations.Contains(x.CourtOrganizationId))
                                    ||
                                    (x.UserId == userContext.UserId)
                                    ) && x.UserCreatedId == (filter.UserId ?? x.UserCreatedId);
            }
            else
            {
                whereSelect = x => (x.UserCreatedId == userContext.UserId) && x.UserId == (filter.UserId ?? x.UserId);
            }
            Expression<Func<WorkTaskVM, bool>> whereTaskType = x => true;
            if (filter.TaskTypeId > 0)
            {
                whereTaskType = x => x.TaskTypeId == filter.TaskTypeId;
            }

            Expression<Func<WorkTaskVM, bool>> whereState = x => x.TaskStateId == (filter.TaskStateId ?? x.TaskStateId);
            if (filter.TaskStateId == WorkTaskConstants.States.NotFinishedId)
            {
                whereState = x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId);
            }

            Expression<Func<WorkTaskVM, bool>> whereSourceDescription = x => true;
            if (!string.IsNullOrEmpty(filter.SourceDescription))
            {
                whereState = x => EF.Functions.ILike(x.SourceDescription ?? "", filter.SourceDescription.ToPaternSearch());
            }
            Expression<Func<WorkTaskVM, bool>> whereParentDescription = x => true;
            if (!string.IsNullOrEmpty(filter.ParentDescription))
            {
                whereState = x => EF.Functions.ILike(x.ParentDescription ?? "", filter.ParentDescription.ToPaternSearch());
            }

            return selectTasks(false, false, 0, 0, true)
                    .Where(x => x.DateCreated >= (filter.DateFrom ?? x.DateCreated) && x.DateCreated <= (filter.DateTo.MakeEndDate() ?? x.DateCreated))
                    .Where(whereSelect)
                    .Where(whereTaskType)
                    .Where(whereState)
                    .Where(whereSourceDescription)
                    .Where(whereParentDescription);
        }

        public IQueryable<WorkTaskVM> SelectAll(WorkTaskFilterVM filter)
        {
            Expression<Func<WorkTaskVM, bool>> userSearch = x => true;
            if (!string.IsNullOrEmpty(filter.AssignedTo))
            {
                userSearch = x => x.UserId == filter.AssignedTo;
            }
            return repo.AllReadonly<WorkTask>()
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
        public async Task<IEnumerable<WorkTaskVM>> Select(int sourceType, long sourceId)
        {
            var result = await selectTasks(false, false, sourceType, sourceId).ToListAsync();
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

        public async Task<int> Select_ToDoCount()
        {
            var _userOrganizations = userContext.CourtOrganizations.Select(x => (int?)x).ToArray();
            string userId = userContext.UserId;


            var result = await readonlyrepo.AllReadonly<WorkTask>()
                           .Where(x => x.CourtId == userContext.CourtId)
                           .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId) && x.UserId == userId)
                           .CountAsync().ConfigureAwait(false);

            result += await readonlyrepo.AllReadonly<WorkTask>()
                           .Where(x => x.CourtId == userContext.CourtId)
                           .Where(x => x.TaskStateId == WorkTaskConstants.States.New && _userOrganizations.Contains(x.CourtOrganizationId)
                           && x.UserId == null)
                           .CountAsync().ConfigureAwait(false);
            return result;
        }

        private void setTaskActions(IEnumerable<WorkTaskVM> model)
        {
            var isGlobalAdmin = userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator);
            var _userOrganizations = userContext.CourtOrganizations.Select(x => (int?)x).ToArray();
            bool isTaskRouter = userContext.IsUserInRole(AccountConstants.Roles.Supervisor);
            foreach (var task in model)
            {
                bool sameUser = isGlobalAdmin;
                switch (task.TaskExecutionId)
                {
                    case WorkTaskConstants.TaskExecution.ByOrganization:
                        sameUser = sameUser || _userOrganizations.Contains(task.CourtOrganizationId);
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

                task.OverDue = (task.DateEnd < DateTime.Now) && WorkTaskConstants.States.NotFinished.Contains(task.TaskStateId);
                if (WorkTaskConstants.Types.TaskCantUpdate.Contains(task.TaskTypeId))
                {
                    task.CanUpdate = false;
                    task.CanRedirect = false;
                }
                if (WorkTaskConstants.Types.SelfCompleteTasks.Contains(task.TaskTypeId))
                {
                    task.CanComplete = false;
                }
                SetDoActionUrl(task);
                if (WorkTaskConstants.Types.AutomatedTasks.Contains(task.TaskTypeId))
                {
                    task.CanAccept = false;
                    task.CanRedirect = false;
                    task.CanComplete = false;
                    task.CanDoAction = false;
                }
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
                case SourceTypeSelectVM.MediationCase:
                    return urlHelper.Action("CasePreviewMediation", "Mediation", new { id = sourceId });
                case SourceTypeSelectVM.CaseSession:
                    return urlHelper.Action("Preview", "CaseSession", new { id = sourceId });
                case SourceTypeSelectVM.CasePerson:
                    return urlHelper.Action("Edit", "CasePerson", new { id = sourceId });
                case SourceTypeSelectVM.CasePersonBulletin:
                    return urlHelper.Action("EditBulletin", "CasePersonSentence", new { id = sourceId });
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
                case SourceTypeSelectVM.Document:
                    caseId = repo.AllReadonly<Document>()
                                    .Where(x => x.Id == sourceId)
                                    .Select(x => x.Cases.Select(c => c.Id).FirstOrDefault())
                                    .FirstOrDefault();

                    if (caseId == 0)
                    {
                        caseId = repo.AllReadonly<Document>()
                                   .Where(x => x.Id == sourceId)
                                   .Select(x => x.DocumentCaseInfo.Select(c => c.CaseId).FirstOrDefault())
                                   .FirstOrDefault();
                    }
                    break;
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
                case SourceTypeSelectVM.CasePersonBulletin:
                    caseId = repo.GetById<CasePersonSentenceBulletin>((int)sourceId)?.CaseId;
                    break;
                case SourceTypeSelectVM.ExecList:
                    caseId = repo.AllReadonly<ExecList>().Where(x => x.Id == (int)sourceId)
                        .Select(x => x.ExecListObligations.Select(a => a.Obligation.CaseId).FirstOrDefault())
                        .FirstOrDefault();
                    break;
                case SourceTypeSelectVM.DocumentResolution:
                    caseId = repo.AllReadonly<DocumentResolution>()
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
                case WorkTaskConstants.Types.CaseSessionAct_SentMotiveToCoordinate:
                    model.DoActionUrl = urlHelper.Action("DoTask_SentMotiveForCoordinate", "CaseSessionAct", new { id = model.Id });
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
                case WorkTaskConstants.Types.CasePersonBulletin_SentToSign:
                case WorkTaskConstants.Types.CasePersonBulletin_SentToSignNewNumber:
                    model.DoActionUrl = urlHelper.Action("DoTask_SentBuletinForSign", "CasePersonSentence", new { id = model.Id });
                    break;
                case WorkTaskConstants.Types.CasePersonBulletin_Sign:
                    model.DoActionUrl = urlHelper.Action("SendBuletinForSign", "CasePersonSentence", new { id = model.SourceId, taskId = model.Id });
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
        public async Task<bool> CreateTask(WorkTaskEditVM model)
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
                    var taskType = await repo.GetByIdAsync<TaskType>(model.TaskTypeId);
                    if (taskType.SelfTask == true || entity.UserId == userContext.UserId)
                    {
                        entity.UserId = userContext.UserId;
                        entity.DateAccepted = DateTime.Now;
                        entity.TaskStateId = WorkTaskConstants.States.Accepted;
                    }
                }
                await CreateTaskSourceDescription(entity);

                repo.Add(entity);
                await repo.SaveChangesAsync();
                await autoCompleteTasks(entity);
                model.Id = entity.Id;
                model.SourceDescription = entity.SourceDescription;
                model.ParentDescription = entity.ParentDescription;
                model.TaskTypeName = repo.GetPropById<TaskType, string>(x => x.Id == model.TaskTypeId, x => x.Label);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }
        private async Task autoCompleteTasks(WorkTask task)
        {
            if (!WorkTaskConstants.Types.AutoCompleteTasks.Contains(task.TaskTypeId))
            {
                return;
            }

            if (await CompleteTask(task))
            {
                await UpdateAfterCompleteTask(task);
            }
        }
        public bool UpdateTask(WorkTaskEditVM model)
        {
            try
            {
                var entity = repo.GetById<WorkTask>(model.Id);
                model.ToEntity(entity);
                //repo.Update(entity);
                repo.SaveChanges();
                model.SourceDescription = entity.SourceDescription;
                model.ParentDescription = entity.ParentDescription;
                model.TaskTypeName = repo.GetPropById<TaskType, string>(x => x.Id == model.TaskTypeId, x => x.Label);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }

        private async Task CreateTaskSourceDescription(WorkTask model)
        {
            try
            {
                switch (model.SourceType)
                {
                    case SourceTypeSelectVM.Document:
                        {
                            var info = await repo.AllReadonly<Document>()
                                            .Where(x => x.Id == model.SourceId)
                                            .Select(x => new
                                            {
                                                sd = $"{x.DocumentType.Label} {x.DocumentNumber}/{x.DocumentDate:dd.MM.yyyy}",
                                                caseId = x.DocumentCaseInfo.Select(x => x.CaseId).FirstOrDefault()
                                                //pdCase = x.DocumentCaseInfo.Select(c => c.Case).FirstOrDefault()
                                            })
                                            .FirstOrDefaultAsync();

                            model.SourceDescription = info.sd;

                            if (info.caseId > 0)
                            {
                                var pdCase = await repo.AllReadonly<Case>()
                                                        .Where(x => x.Id == info.caseId)
                                                        .Select(x => new
                                                        {
                                                            x.CourtId,
                                                            x.RegNumber
                                                        }).FirstOrDefaultAsync();
                                if (pdCase != null && pdCase.CourtId == model.CourtId)
                                {
                                    model.ParentDescription = pdCase.RegNumber;
                                }
                            }
                        }
                        break;
                    case SourceTypeSelectVM.DocumentResolution:
                        {
                            var info = await repo.AllReadonly<DocumentResolution>()
                                           .Where(x => x.Id == model.SourceId)
                                           .Select(x => new
                                           {
                                               sd = $" към {x.Document.DocumentType.Label} {x.Document.DocumentNumber}/{x.Document.DocumentDate:dd.MM.yyyy}",
                                               pd = (x.Document.Cases.Any()) ? x.Document.Cases.Select(c => c.RegNumber).FirstOrDefault() : ""
                                           })
                                           .FirstOrDefaultAsync();
                            model.SourceDescription = info.sd;
                            model.ParentDescription = info.pd;
                        }
                        break;
                    case SourceTypeSelectVM.CaseSessionAct:
                        {
                            var info = await repo.AllReadonly<CaseSessionAct>()
                                            .Where(x => x.Id == (int)model.SourceId)
                                            .Select(x => new
                                            {
                                                sd = $"{x.ActType.Label} {x.RegNumber}/{x.RegDate:dd.MM.yyyy}",
                                                pd = x.Case.RegNumber
                                            })
                                            .FirstOrDefaultAsync();

                            model.SourceDescription = info.sd;
                            model.ParentDescription = info.pd;
                        }
                        break;
                    case SourceTypeSelectVM.ExecList:
                        {
                            var info = await repo.AllReadonly<ExecList>()
                                            .Where(x => x.Id == (int)model.SourceId)
                                            .Select(x => new
                                            {
                                                sd = $"{x.ExecListType.Label} {x.RegNumber}/{x.RegDate:dd.MM.yyyy}",
                                                pd = x.ExecListObligations.Select(a => a.Obligation.Case.RegNumber).FirstOrDefault(),
                                            })
                                            .FirstOrDefaultAsync();

                            model.SourceDescription = info.sd;
                            model.ParentDescription = info.pd;
                        }
                        break;
                    case SourceTypeSelectVM.Case:
                    case SourceTypeSelectVM.MediationCase:
                        {
                            var info = await repo.AllReadonly<Case>()
                                               .Where(x => x.Id == (int)model.SourceId)
                                               .Select(x => new
                                               {
                                                   sd = $"{x.CaseType.Code} {x.ShortNumber}/{x.RegDate:dd.MM.yyyy}",
                                                   pd = x.RegNumber
                                               })
                                               .FirstOrDefaultAsync();

                            model.SourceDescription = info.sd;
                            model.ParentDescription = info.pd;
                        }
                        break;
                    case SourceTypeSelectVM.CasePersonBulletin:
                        {
                            var info = await repo.AllReadonly<CasePersonSentenceBulletin>()
                                                .Where(x => x.Id == (int)model.SourceId)
                                                .Select(x => new
                                                {
                                                    sd = $"{x.CasePerson.FullName}",
                                                    pd = x.Case.RegNumber,
                                                })
                                            .FirstOrDefaultAsync();

                            model.SourceDescription = info.sd;
                            model.ParentDescription = info.pd;
                        }
                        break;
                }
            }
            catch { }
        }

        public async Task<SaveResultVM> AcceptTask(long id)
        {
            try
            {
                var model = await ReadByIdAsync<WorkTask>(id);

                if (!ValidateSourceCourt(model.SourceType, model.SourceId))
                {
                    return new SaveResultVM(false, "Непозволена операция!");
                }
                if (model.TaskStateId == WorkTaskConstants.States.Deleted)
                {
                    return new SaveResultVM(false, "Задачата е отменена, моля презаредете екрана!");
                }
                if (model.TaskStateId == WorkTaskConstants.States.Redirected)
                {
                    return new SaveResultVM(false, "Задачата е пренасочена, моля презаредете екрана!");
                }
                if (model.DateAccepted != null)
                {
                    return new SaveResultVM(false, "Задачата вече е приета!");
                }
                if (!string.IsNullOrEmpty(model.UserId) && (model.UserId != userContext.UserId) && !userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator))
                {
                    return new SaveResultVM(false, "Непозволена операция!");
                }
                if (model.CourtOrganizationId > 0)
                {
                    model.UserId = userContext.UserId;
                }
                model.DateAccepted = DateTime.Now;
                model.TaskStateId = WorkTaskConstants.States.Accepted;
                await repo.SaveChangesAsync();
                return new SaveResultVM(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return new SaveResultVM(false, "Грешка при приемане на задача!"); ;
            }
        }


        public Task<bool> CompleteTask(long id)
        {
            return CompleteTask(new WorkTask() { Id = id });
        }
        public Task<bool> RejectTask(long id, string description)
        {
            return CompleteTask(new WorkTask() { Id = id, Description = description }, WorkTaskConstants.States.Deleted);
        }

        public async Task<bool> CompleteTask(WorkTask model, int completedState = WorkTaskConstants.States.Completed)
        {

            try
            {
                var saved = await ReadByIdAsync<WorkTask>(model.Id);

                if (saved.TaskStateId == WorkTaskConstants.States.Completed)
                {
                    return false;
                }
                saved.TaskActionId = model.TaskActionId;
                saved.Description = model.Description;
                saved.DateCompleted = DateTime.Now;
                saved.TaskStateId = completedState;
                await CreateTaskSourceDescription(saved);
                await CompleteTask_UpdateOthers(saved);
                await repo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, $"Грешка при приключване на задача Id:{model.Id}");
                return false;
            }
        }

        private async Task CompleteTask_UpdateOthers(WorkTask model)
        {
            try
            {
                bool forSearch = false;
                Expression<Func<WorkTask, bool>> selectToDelete = x => false;
                switch (model.TaskTypeId)
                {
                    case WorkTaskConstants.Types.CaseSessionAct_SentToSign:
                        {

                            selectToDelete = x =>
                                (x.TaskTypeId == WorkTaskConstants.Types.CaseSessionAct_SentToSign) ||
                                (x.TaskTypeId == WorkTaskConstants.Types.CaseSessionAct_Sign) ||
                                (x.TaskTypeId == WorkTaskConstants.Types.CaseSessionActCoordination_Sign);
                            forSearch = true;
                        }
                        break;
                    case WorkTaskConstants.Types.CasePersonBulletin_SentToSign:
                    case WorkTaskConstants.Types.CasePersonBulletin_SentToSignNewNumber:
                        {

                            selectToDelete = x =>
                                (x.TaskTypeId == WorkTaskConstants.Types.CasePersonBulletin_SentToSign) ||
                                (x.TaskTypeId == WorkTaskConstants.Types.CasePersonBulletin_SentToSignNewNumber) ||
                                (x.TaskTypeId == WorkTaskConstants.Types.CasePersonBulletin_Sign);
                            forSearch = true;
                        }
                        break;
                    case WorkTaskConstants.Types.CaseSessionActMotives_SentToSign:
                        {

                            selectToDelete = x =>
                                (x.TaskTypeId == WorkTaskConstants.Types.CaseSessionActMotives_SentToSign) ||
                                (x.TaskTypeId == WorkTaskConstants.Types.CaseSessionActMotives_Sign);
                            forSearch = true;
                        }
                        break;
                    case WorkTaskConstants.Types.DocumentResolution_SentToSign:
                        {
                            selectToDelete = x =>
                                (x.TaskTypeId == WorkTaskConstants.Types.DocumentResolution_SentToSign) ||
                                (x.TaskTypeId == WorkTaskConstants.Types.DocumentResolution_Sign);
                            forSearch = true;
                        }
                        break;
                    case WorkTaskConstants.Types.ExecList_SentToSign:
                        {
                            selectToDelete = x =>
                                (x.TaskTypeId == WorkTaskConstants.Types.ExecList_SentToSign) ||
                                (x.TaskTypeId == WorkTaskConstants.Types.ExecList_Sign);
                            forSearch = true;
                        }
                        break;
                }

                if (!forSearch)
                {
                    return;
                }

                //Отменя всички предходни неприключили задачи за подписване
                var signTasks = await repo.All<WorkTask>().Where(x =>
                               x.SourceId == model.SourceId
                               && x.SourceType == x.SourceType
                               && WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId)
                               && (x.ParentTaskId ?? 0) != model.Id
                               && x.Id < model.Id)
                                .Where(selectToDelete)
                                .ToListAsync();
                foreach (var item in signTasks)
                {
                    item.TaskStateId = WorkTaskConstants.States.Deleted;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"CompleteTask_UpdateOthers; TaskId:{model.Id}");
            }
        }

        public async Task<SaveResultVM> UpdateBeforeCompleteTask(long workTaskId)
        {
            var taskModel = await repo.AllReadonly<WorkTask>().Where(x => x.Id == workTaskId).FirstOrDefaultAsync();
            switch (taskModel.TaskTypeId)
            {

                case WorkTaskConstants.Types.CaseSessionAct_Sign:
                    var actInfo = await repo.AllReadonly<CaseSessionAct>()
                                            .Where(x => x.Id == (int)taskModel.SourceId)
                                            .Select(x => new
                                            {
                                                x.ActDeclaredDate
                                            }).FirstOrDefaultAsync();
                    if (actInfo.ActDeclaredDate != null)
                    {
                        int[] tasksToDelete = { WorkTaskConstants.Types.CaseSessionAct_Sign, WorkTaskConstants.Types.CaseSessionAct_SentToSign };
                        var unCompletedTasks = await repo.All<WorkTask>()
                                                            .Where(x => x.SourceType == taskModel.SourceType)
                                                            .Where(x => x.SourceId == taskModel.SourceId)
                                                            .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                                                            .Where(x => tasksToDelete.Contains(x.TaskTypeId))
                                                            .ToListAsync();
                        if (unCompletedTasks.Any())
                        {
                            foreach (var item in unCompletedTasks)
                            {
                                item.TaskStateId = WorkTaskConstants.States.Deleted;
                            }
                            await repo.SaveChangesAsync();

                        }
                        return new SaveResultVM(false, "Актът вече е постановен!", "taskdelete");
                    }
                    break;
                case WorkTaskConstants.Types.CaseSessionActMotives_Sign:
                    var motiveInfo = await repo.AllReadonly<CaseSessionAct>()
                                            .Where(x => x.Id == (int)taskModel.SourceId)
                                            .Select(x => new
                                            {
                                                x.ActMotivesDeclaredDate
                                            }).FirstOrDefaultAsync();
                    if (motiveInfo.ActMotivesDeclaredDate != null)
                    {
                        int[] tasksToDelete = { WorkTaskConstants.Types.CaseSessionActMotives_Sign, WorkTaskConstants.Types.CaseSessionActMotives_SentToSign };
                        var unCompletedTasks = await repo.All<WorkTask>()
                                                            .Where(x => x.SourceType == taskModel.SourceType)
                                                            .Where(x => x.SourceId == taskModel.SourceId)
                                                            .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                                                            .Where(x => tasksToDelete.Contains(x.TaskTypeId))
                                                            .ToListAsync();
                        if (unCompletedTasks.Any())
                        {
                            foreach (var item in unCompletedTasks)
                            {
                                item.TaskStateId = WorkTaskConstants.States.Deleted;
                            }
                            await repo.SaveChangesAsync();

                        }
                        return new SaveResultVM(false, "Мотивите към акта вече са постановени!", "taskdelete");
                    }
                    break;

                case WorkTaskConstants.Types.DocumentResolution_Sign:
                    var resolutionInfo = await repo.AllReadonly<DocumentResolution>()
                                            .Where(x => x.Id == (int)taskModel.SourceId)
                                            .Select(x => new
                                            {
                                                x.DeclaredDate
                                            }).FirstOrDefaultAsync();
                    if (resolutionInfo.DeclaredDate != null)
                    {
                        int[] tasksToDelete = { WorkTaskConstants.Types.DocumentResolution_Sign, WorkTaskConstants.Types.DocumentResolution_SentToSign };
                        var unCompletedTasks = await repo.All<WorkTask>()
                                                            .Where(x => x.SourceType == taskModel.SourceType)
                                                            .Where(x => x.SourceId == taskModel.SourceId)
                                                            .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                                                            .Where(x => tasksToDelete.Contains(x.TaskTypeId))
                                                            .ToListAsync();
                        if (unCompletedTasks.Any())
                        {
                            foreach (var item in unCompletedTasks)
                            {
                                item.TaskStateId = WorkTaskConstants.States.Deleted;
                            }
                            await repo.SaveChangesAsync();

                        }
                        return new SaveResultVM(false, "Разпореждането вече е постановено!", "taskdelete");
                    }
                    break;

                case WorkTaskConstants.Types.DocumentForGlobalAssignment:
                    var hasRequestFile = await repo.AllReadonly<MongoFile>()
                                                   .Where(x => x.SourceId == taskModel.SourceId.ToString() && x.SourceType == SourceTypeSelectVM.DocumentRequest)
                                                   .AnyAsync();
                    if (!hasRequestFile)
                    {
                        int[] tasksToDelete = { WorkTaskConstants.Types.DocumentForGlobalAssignment };
                        var unCompletedTasks = await repo.All<WorkTask>()
                                                            .Where(x => x.SourceType == taskModel.SourceType)
                                                            .Where(x => x.SourceId == taskModel.SourceId)
                                                            .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                                                            .Where(x => tasksToDelete.Contains(x.TaskTypeId))
                                                            .ToListAsync();
                        if (unCompletedTasks.Any())
                        {
                            foreach (var item in unCompletedTasks)
                            {
                                item.TaskStateId = WorkTaskConstants.States.Deleted;
                            }
                            await repo.SaveChangesAsync();

                        }
                        return new SaveResultVM(false, "Моля, въведете данни в заявлението!", "taskdelete");
                    }
                    break;
            }
            return new SaveResultVM(true);
        }

        public async Task<WorkTaskCheckCompletedVM> CheckCompletedTasks(long taskId)
        {
            var task = await repo.AllReadonly<WorkTask>()
                                    .Where(x => x.Id == taskId)
                                    .Select(x => new
                                    {
                                        x.Id,
                                        x.SourceType,
                                        x.SourceId,
                                        x.TaskTypeId,
                                        x.ParentTaskId
                                    }).FirstOrDefaultAsync();
            return await CheckCompletedTasks(task.SourceType, task.SourceId, task.TaskTypeId, task.Id, task.ParentTaskId);
        }

        public Task<WorkTaskCheckCompletedVM> CheckCompletedTasks(WorkTask task)
        {
            return CheckCompletedTasks(task.SourceType, task.SourceId, task.TaskTypeId, task.Id, task.ParentTaskId, task.DateCompleted);
        }
        public async Task<WorkTaskCheckCompletedVM> CheckCompletedTasks(int sourceType, long sourceId, int taskTypeId, long taskId, long? parentTaskId = null, DateTime? taskDateCompleted = null)
        {
            Expression<Func<WorkTask, bool>> whereParentTask = x => true;
            if (parentTaskId > 0)
            {
                whereParentTask = x => x.ParentTaskId == parentTaskId.Value;
            }
            var signTasks = repo.AllReadonly<WorkTask>()
                                .Where(x => x.SourceType == sourceType)
                                .Where(x => x.SourceId == sourceId)
                                .Where(x => x.TaskTypeId == taskTypeId)
                                .Where(whereParentTask);



            var result = new WorkTaskCheckCompletedVM()
            {
                HasTasks = await signTasks.CountAsync() > 0
            };
            if (result.HasTasks)
            {
                result.HasUncompleteTasks = await signTasks.Where(x => x.TaskStateId != WorkTaskConstants.States.Completed)
                                                     .Where(x => x.TaskStateId != WorkTaskConstants.States.Deleted)
                                                     .Where(x => x.Id != taskId)
                                                     .CountAsync() > 0;

                var lastCompletedTaskInfo = await signTasks
                                        .Where(x => x.DateCompleted != null)
                                        .OrderByDescending(x => x.DateCompleted)
                                        .Select(x => new
                                        {
                                            x.UserId,
                                            x.DateCompleted
                                        })
                                        .FirstOrDefaultAsync();
                if (lastCompletedTaskInfo != null)
                {
                    result.LastUserIdCompleted = lastCompletedTaskInfo.UserId;
                    result.LastDateCompleted = lastCompletedTaskInfo.DateCompleted;
                    if (taskDateCompleted.HasValue && taskDateCompleted > result.LastDateCompleted)
                    {
                        result.LastDateCompleted = taskDateCompleted;
                    }
                }
            }
            return result;
        }


        public async Task<SaveResultVM> UpdateAfterCompleteTask(WorkTask model)
        {
            switch (model.TaskTypeId)
            {
                case WorkTaskConstants.Types.Document_Sign:
                    {
                        var hasUnCompletedSignTasks = await repo.AllReadonly<WorkTask>()
                                                            .Where(x => x.SourceType == model.SourceType)
                                                            .Where(x => x.SourceId == model.SourceId)
                                                            .Where(x => x.TaskStateId != WorkTaskConstants.States.Completed)
                                                            .Where(x => x.TaskTypeId == model.TaskTypeId)
                                                            .AnyAsync();
                        if (!hasUnCompletedSignTasks)
                        {
                            var docFileRequest = await repo.AllReadonly<MongoFile>()
                                                            .Where(x => x.SourceType == SourceTypeSelectVM.DocumentPdf
                                                            && x.SourceIdNumber == model.SourceId
                                                            && x.DateExpired == null
                                                            && x.SignituresCount > 0)
                                                            .Select(x => new CdnUploadRequest
                                                            {
                                                                MongoFileId = x.Id,
                                                                SourceType = x.SourceType,
                                                                SourceId = x.SourceId,
                                                                FileId = x.FileId
                                                            }).FirstOrDefaultAsync();

                            if (docFileRequest != null)
                            {

                                var document = await repo.AllReadonly<Document>()
                                                            .Include(x => x.DocumentPersons)
                                                            .ThenInclude(x => x.Addresses)
                                                            .ThenInclude(x => x.Address)
                                                            .Include(x => x.DocumentCaseInfo)
                                                            .Include(x => x.Cases)
                                                            .Where(x => x.Id == model.SourceId)
                                                            .AsSplitQuery()
                                                            .FirstOrDefaultAsync().ConfigureAwait(false);
                                await mqEpepService.AppendDocument(document, EpepConstants.ServiceMethod.Add);

                                mqEpepService.AppendFile(docFileRequest, EpepConstants.ServiceMethod.Add);
                            }

                            var templateInfo = await repo.AllReadonly<DocumentTemplate>().Where(x => x.DocumentId == model.SourceId)
                                                .Select(x => new { x.DocumentTypeId, x.SourceId, x.SourceType }).FirstOrDefaultAsync();
                            if (templateInfo != null)
                            {
                                switch (templateInfo.SourceType)
                                {
                                    case SourceTypeSelectVM.CaseMigration:
                                        mqEpepService.AppendCaseMigrationFull((int)templateInfo.SourceId);
                                        await manageCaseMigrationAfterSign((int)templateInfo.SourceId);
                                        break;
                                    case SourceTypeSelectVM.CaseLawyerHelp:
                                        mqEpepService.EESPP_LawyerHelp(EpepConstants.ServiceMethod.Add, (int)templateInfo.SourceId);
                                        break;
                                    case SourceTypeSelectVM.CaseSessionAct:
                                        await mqEpepService.AppendISPNLetter((int)templateInfo.SourceId, templateInfo.DocumentTypeId);
                                        break;
                                    default:
                                        break;
                                }
                            }

                            await workNotificationService.SaveNotificationsForN24(model.SourceId);

                            return new SaveResultVM(true, "", "reload");
                        }
                        return new SaveResultVM(true);
                    }

                case WorkTaskConstants.Types.CaseSessionAct_Coordinate:
                    {
                        var hasUnCompletedCoTasks = await repo.AllReadonly<WorkTask>()
                                                            .Where(x => x.ParentTaskId == model.ParentTaskId)
                                                            .Where(x => x.TaskStateId != WorkTaskConstants.States.Completed)
                                                            .Where(x => x.TaskTypeId == model.TaskTypeId)
                                                            .AnyAsync();
                        if (!hasUnCompletedCoTasks)
                        {
                            var actModel = await this.ReadByIdAsync<CaseSessionAct>((int)model.SourceId);
                            if (actModel.ActStateId < NomenclatureConstants.SessionActState.Coordinated)
                            {
                                actModel.ActStateId = NomenclatureConstants.SessionActState.Coordinated;
                                await repo.SaveChangesAsync();
                                return new SaveResultVM(true, "", "reload");
                            }
                        }
                        return new SaveResultVM(true);
                    }

                case WorkTaskConstants.Types.CaseSessionAct_Sign:
                    {
                        var signTaskCheck = await CheckCompletedTasks(model);


                        if (!signTaskCheck.HasUncompleteTasks && signTaskCheck.HasTasks)
                        {
                            var actModel = await this.ReadByIdAsync<CaseSessionAct>((int)model.SourceId);
                            if (actModel.ActDeclaredDate == null)
                            {
                                actModel.ActStateId = NomenclatureConstants.SessionActState.Enforced;
                                actModel.ActDeclaredDate = signTaskCheck.LastDateCompleted;
                                actModel.DateWrt = DateTime.Now;
                                var sessionDateFrom = await repo.GetPropByIdAsync<CaseSession, DateTime>(x => x.Id == actModel.CaseSessionId, x => x.DateFrom);
                                int declaredMonthCount = await commonService.GetMonthsBetweenTwoDatesForActs(sessionDateFrom, actModel.ActDeclaredDate.Value);
                                actModel.DeclaredMonthCount = declaredMonthCount;

                                //Автоматично влизане в сила на актове, неподлежащи на обжалване
                                if (actModel.CanAppeal == false)
                                {
                                    actModel.ActStateId = NomenclatureConstants.SessionActState.ComingIntoForce;
                                    actModel.ActInforcedDate = actModel.ActDeclaredDate;
                                }
                                await CreateHistoryAsync<CaseSessionAct, CaseSessionActH>(actModel, "Task.UpdateAfterCompleteTask.Sign");
                                await repo.SaveChangesAsync();


                                try
                                {
                                    mqEpepService.SetImpersonatedUser(signTaskCheck.LastUserIdCompleted);
                                    await mqEpepService.AppendCaseSessionAct(actModel, EpepConstants.ServiceMethod.Add);
                                    await mqEpepService.AppendCaseSessionAct_Private(actModel.Id, EpepConstants.ServiceMethod.Add);

                                    if (NomenclatureConstants.ActType.ExecListActs.Contains(actModel.ActTypeId))
                                    {
                                        await mqEpepService.AppendExecProcess(actModel.Id, 0);
                                    }

                                    //Изпраща всички вече подписани особени мнения
                                    var signedCoordinations = await coordinationService.CaseSessionActCoordination_Select(actModel.Id).ToListAsync();
                                    foreach (var coordination in signedCoordinations.Where(x => x.CoordinationDeclaredDate.HasValue))
                                    {
                                        mqEpepService.AppendAttachedDocument(SourceTypeSelectVM.CaseSessionActCoordinationPdf, coordination.Id, coordination.CaseSessionActId, EpepConstants.ServiceMethod.Add);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, $"Грешка при създаване на задачи за интеграция след подпис на акт Id:{actModel.Id}");
                                }

                                await mqEpepService.EissProcessStart(NomenclatureConstants.EissProcessTypes.ActDeclared, SourceTypeSelectVM.CaseSessionAct, actModel.Id,
                                    new Infrastructure.Models.Integrations.EISS.EissProcessActDeclaredVM()
                                    {
                                        LastUserIdCompleted = signTaskCheck.LastUserIdCompleted,
                                        LastDateCompleted = signTaskCheck.LastDateCompleted
                                    });

                                /*
                                 * Върнати методи, понеже изискват userContext
                                 


                                 */

                                try
                                {
                                    lifecycleService.SetImpersonatedUser(signTaskCheck.LastUserIdCompleted);
                                    lifecycleService.CaseLifecycle_SaveFirst_ForCaseType(actModel.CaseSessionId, signTaskCheck.LastDateCompleted);
                                    if (actModel.IsFinalDoc)
                                        await lifecycleService.CaseLifecycle_CloseInterval(actModel.CaseId ?? 0, actModel.Id, actModel.ActDeclaredDate ?? DateTime.Now);
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, $"Грешка при запис на CaseLifecycle на акт Id:{actModel.Id}");
                                }
                                caseLoadIndexService.CaseLoadIndexAutomationElementGroupe_SRA_SaveData(actModel.CaseSessionId);


                                /* - Тези обработки са преместени в EissProcessService.ProcessActDeclared
                                try
                                {
                                    lifecycleService.SetImpersonatedUser(signTaskCheck.LastUserIdCompleted);
                                    lifecycleService.CaseLifecycle_SaveFirst_ForCaseType(actModel.CaseSessionId, signTaskCheck.LastDateCompleted);
                                    if (actModel.IsFinalDoc)
                                        await lifecycleService.CaseLifecycle_CloseInterval(actModel.CaseId ?? 0, actModel.Id, actModel.ActDeclaredDate ?? DateTime.Now);
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, $"Грешка при запис на CaseLifecycle на акт Id:{actModel.Id}");
                                }

                                try
                                {
                                    mqEpepService.SetImpersonatedUser(signTaskCheck.LastUserIdCompleted);
                                    mqEpepService.AppendCaseSessionAct(actModel, EpepConstants.ServiceMethod.Add);
                                    mqEpepService.AppendCaseSessionAct_Private(actModel.Id, EpepConstants.ServiceMethod.Add);

                                    if (NomenclatureConstants.ActType.ExecListActs.Contains(actModel.ActTypeId))
                                    {
                                        await mqEpepService.AppendExecProcess(actModel.Id, 0);
                                    }

                                    //Изпраща всички вече подписани особени мнения
                                    var signedCoordinations = await coordinationService.CaseSessionActCoordination_Select(actModel.Id).ToListAsync();
                                    foreach (var coordination in signedCoordinations.Where(x => x.CoordinationDeclaredDate.HasValue))
                                    {
                                        mqEpepService.AppendAttachedDocument(SourceTypeSelectVM.CaseSessionActCoordinationPdf, coordination.Id, coordination.CaseSessionActId, EpepConstants.ServiceMethod.Add);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, $"Грешка при създаване на задачи за интеграция след подпис на акт Id:{actModel.Id}");
                                }
                                //using (var ts = repo.BeginTransaction())
                                //{
                                caseDeadlineService.SetImpersonatedUser(signTaskCheck.LastUserIdCompleted);
                                caseDeadlineService.DeadLineMotive(actModel);
                                caseDeadlineService.DeadLineCompleteOnSessionAct(actModel);
                                await caseDeadlineService.CompleteExpiredCaseDeadlineFastProcess(actModel.CaseId ?? 0, actModel.CaseId ?? 0, SourceTypeSelectVM.Case, NomenclatureConstants.DeadlineType.TakingActionFastProcess);
                                await caseDeadlineService.CompleteExpiredCaseDeadlineFastProcess(actModel.CaseId ?? 0, null, SourceTypeSelectVM.Document, NomenclatureConstants.DeadlineType.MissingActForCompliantDocumentFastProcess);
                                await caseDeadlineService.CompleteExpiredMissingActForCompliantDocumentFastProcess(actModel.Id, false);
                                await workNotificationService.TurnOfCompliantDocumentCaseFastProcess(actModel.Id, false);

                                // Автоматизиране на статус - решено
                                var caseCase = await repo.GetByIdAsync<Case>(actModel.CaseId);
                                if ((caseCase.CaseStateId == NomenclatureConstants.CaseState.AnnouncedForResolution) && (actModel.ActTypeId == NomenclatureConstants.ActType.Answer))
                                {
                                    caseCase.CaseStateId = NomenclatureConstants.CaseState.Resolution;
                                    caseCase.DateWrt = DateTime.Now;
                                    caseCase.UserId = userContext.UserId;
                                    //repo.Update(caseCase);
                                    caseDeadlineService.DeadLineOnCase(caseCase);
                                }

                                await repo.SaveChangesAsync();

                                await workNotificationService.SaveNotificationsForN3(actModel.Id);
                                await workNotificationService.SaveNotificationsForN11(actModel.Id);
                                await workNotificationService.SaveNotificationsForDecreeRecusalSelfRecusalFastProcessByActId(actModel.Id);
                                await workNotificationService.SaveNotificationsForActionTakenCourtOfficerDeclatActFastProcess(actModel.Id);
                                await workNotificationService.TurnOffForN1(actModel.Id);
                                await workNotificationService.SaveNotificationsForActInforcedAnotherInstanceFastProcess(actModel.Id);

                                caseLoadIndexService.CaseLoadIndexAutomationElementGroupe_SRA_SaveData(actModel.CaseSessionId);
                                */
                                //ts.Commit();
                                return new SaveResultVM(true, "", "reload");
                                //}
                            }
                            else
                            {
                                //ако вече е постановен - само изпраща новите версии към ЕПЕП
                                await mqEpepService.AppendCaseSessionAct(actModel, EpepConstants.ServiceMethod.Update);
                                await mqEpepService.AppendCaseSessionAct_Private(actModel.Id, EpepConstants.ServiceMethod.Update);
                            }
                        }
                        else
                        {
                            return new SaveResultVM(true);
                        }
                    }
                    break;
                case WorkTaskConstants.Types.CaseSessionActCoordination_Sign:
                    {
                        var coordination = await repo.GetByIdAsync<CaseSessionActCoordination>((int)(model.SubSourceId ?? 0));
                        var act = await repo.GetByIdAsync<CaseSessionAct>((int)(model.SourceId));
                        if (coordination != null && act != null)
                        {
                            coordination.CoordinationDeclaredDate = DateTime.Now;
                            await repo.SaveChangesAsync();
                            if (act.ActDeclaredDate.HasValue)
                            {
                                mqEpepService.AppendAttachedDocument(SourceTypeSelectVM.CaseSessionActCoordinationPdf, coordination.Id, coordination.CaseSessionActId, EpepConstants.ServiceMethod.Add);
                            }
                            return new SaveResultVM(true, "", "reload");
                        }
                        return new SaveResultVM(true);
                    }
                case WorkTaskConstants.Types.CaseSessionActMotives_Sign:
                    {
                        var motivesTasks = await repo.AllReadonly<WorkTask>().Where(x => x.ParentTaskId == model.ParentTaskId).ToListAsync();
                        var hasUnCompletedCoTasks = motivesTasks.Where(x => x.TaskStateId != WorkTaskConstants.States.Completed)
                                                        .Where(x => x.TaskTypeId == model.TaskTypeId)
                                                        .Any();
                        var actModel = await repo.GetByIdAsync<CaseSessionAct>((int)model.SourceId);
                        if (!hasUnCompletedCoTasks)
                        {
                            if (actModel.ActMotivesDeclaredDate == null)
                            {

                                //using (var ts = repo.BeginTransaction())
                                //{
                                actModel.ActMotivesDeclaredDate = motivesTasks.OrderByDescending(x => x.DateCompleted).Select(x => x.DateCompleted).FirstOrDefault();

                                await repo.SaveChangesAsync();
                                mqEpepService.AppendCaseSessionAct_PrivateMotive(actModel.Id, EpepConstants.ServiceMethod.Add);
                                try
                                {
                                    caseDeadlineService.DeadLineMotive(actModel);
                                    await repo.SaveChangesAsync();
                                }
                                catch (Exception ex) { }
                                try
                                {
                                    caseLoadIndexService.CaseLoadIndexAutomationElementGroupe_SRA_SaveData(actModel.CaseSessionId);
                                }
                                catch (Exception ex) { }
                                //  ts.Commit();
                                return new SaveResultVM(true, "", "reload");
                                //}
                            }
                            else
                            {
                                mqEpepService.AppendCaseSessionAct_PrivateMotive(actModel.Id, EpepConstants.ServiceMethod.Update);
                            }
                        }
                        return new SaveResultVM(true);
                    }
                case WorkTaskConstants.Types.DocumentResolution_Sign:
                    {
                        var signTaskCheck = await CheckCompletedTasks(model);


                        //var signTasks = await repo.AllReadonly<WorkTask>().Where(x => x.ParentTaskId == model.ParentTaskId).ToListAsync();
                        //var hasUnCompletedCoTasks = signTasks.Where(x => x.TaskStateId != WorkTaskConstants.States.Completed)
                        //                                .Where(x => x.TaskTypeId == model.TaskTypeId)
                        //                                .Any();

                        var resolutionModel = await repo.All<DocumentResolution>()
                                                        .Include(x => x.ResolutionType)
                                                        .Where(x => x.Id == model.SourceId)
                                                        .FirstOrDefaultAsync();
                        if (!signTaskCheck.HasUncompleteTasks && signTaskCheck.HasTasks && (resolutionModel.DeclaredDate == null))
                        {
                            //using (var ts = repo.BeginTransaction())
                            //{
                            resolutionModel.ResolutionStateId = NomenclatureConstants.ResolutionStates.Enforced;
                            resolutionModel.DeclaredDate = signTaskCheck.LastDateCompleted;
                            resolutionModel.DateWrt = DateTime.Now;

                            await repo.SaveChangesAsync();

                            if (resolutionModel.ResolutionTypeId == DocumentConstants.ResolutionTypes.ResolutionForSelection)
                            {
                                var cases = await repo.AllReadonly<DocumentResolutionCase>().Where(x => x.DocumentResolutionId == resolutionModel.Id)
                                                    .Select(x => x.CaseId).ToListAsync();
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

                                    await CreateTask(newTask);
                                }
                            }

                            //ts.Commit();
                            return new SaveResultVM(true, "", "reload");
                            //}
                        }
                        return new SaveResultVM(true);

                    }


                case WorkTaskConstants.Types.SendFor_Competency:
                    {
                        int[] tasksToDelete = { WorkTaskConstants.Types.DocumentResolution_SentToSign, WorkTaskConstants.Types.DocumentResolution_Sign };
                        var unCompletedTasks = await repo.All<WorkTask>()
                                                            .Where(x => x.SourceType == model.SourceType)
                                                            .Where(x => x.SourceId == model.SourceId)
                                                            .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                                                            .Where(x => tasksToDelete.Contains(x.TaskTypeId))
                                                            .ToListAsync();
                        if (unCompletedTasks.Any())
                        {
                            foreach (var item in unCompletedTasks)
                            {
                                item.TaskStateId = WorkTaskConstants.States.Deleted;
                            }
                            await repo.SaveChangesAsync();

                        }
                        return new SaveResultVM(true);
                    }
                case WorkTaskConstants.Types.ExecList_Sign:
                    {
                        var moneyService = lazyMoneyService.Service;
                        var execList = await moneyService.ReadByIdAsync<ExecList>((int)model.SourceId);
                        if (moneyService.ExecListSign(execList).Result)
                        {
                            await mqEpepService.AppendExecList(execList, EpepConstants.ServiceMethod.Add);
                        }
                        return new SaveResultVM(true);
                    }
                case WorkTaskConstants.Types.CasePersonBulletin_Sign:
                    {
                        var bulletinFile = await repo.GetByIdAsync<CasePersonSentenceBulletinFile>((int)(model.SubSourceId ?? 0));
                        bulletinFile.DateSigned = DateTime.Now;
                        mqEpepService.CAIS_SendBulletin(EpepConstants.ServiceMethod.Add, bulletinFile.Id, bulletinFile.CasePersonSentenceBulletinId);
                        return new SaveResultVM(true);
                    }
                case WorkTaskConstants.Types.DocumentForGlobalAssignment:
                    {
                        await autoCreateMoneyObligationOnDocument(model.SourceId);
                        await mqEpepService.EpepDocument_SendForAssignment(model.SourceId);
                        break;
                    }
            }
            return new SaveResultVM(false, "Неподдържан тип задача");
        }

        /// <summary>
        /// Автоматично създава задължение при изпълнение на задача за Централно разпределение
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        async Task autoCreateMoneyObligationOnDocument(long documentId)
        {
            var service = lazyMoneyService.Service;

            if (await service.Obligation_Select(0, documentId, 0, userContext.CourtId, 0).AnyAsync())
            {
                //Ако има вече добавени задължения по документа - излиза
                return;
            }

            var model = new Infrastructure.Models.ViewModels.Money.ObligationEditVM()
            {
                CourtId = userContext.CourtId,
                DocumentId = documentId,
                Person_SourceType = SourceTypeSelectVM.DocumentPerson,
                IsActive = true
            };

            model.MoneySign = NomenclatureConstants.MoneySign.SignPlus;
            model.MoneyTypeId = NomenclatureConstants.MoneyType.StateFee;

            await service.InitNewObligationFromSource(model);
            model.Description = "Автоматично геренерирано задължение";

            //Задължението е прави само когато има изчислена сума и избрано възможно задължено лице - първия заявител
            if (model.Amount > 0M && model.Person_SourceId > 0)
            {
                (bool result, string errorMessage, bool deactivate) = service.Obligation_SaveData(model);
            }
        }

        /// <summary>
        /// Завършва движението за изпращане за преразпределение и създава задача за преразпреление на документа от регистратура ЦР
        /// </summary>
        /// <param name="caseMigrationId"></param>
        /// <returns></returns>
        async Task manageCaseMigrationAfterSign(int caseMigrationId)
        {
            var migrationInfo = await repo.AllReadonly<CaseMigration>()
                                            .Where(x => x.Id == caseMigrationId)
                                            .Select(x => new
                                            {
                                                x.CaseMigrationTypeId,
                                                x.Case.Document.DocumentRequestTypeId,
                                                x.Case.Document.AssignmentDocumentId
                                            }).FirstOrDefaultAsync();

            //Ако движението не е за преразпределение или документа не е по бланка или няма първичен документ
            if (!NomenclatureConstants.CaseMigrationTypes.SendCase_FromAssignment.Contains(migrationInfo.CaseMigrationTypeId)
                || migrationInfo.DocumentRequestTypeId == null
                || migrationInfo.AssignmentDocumentId == null)
            {
                return;
            }

            var mqItem = new MQEpep()
            {
                IntegrationTypeId = NomenclatureConstants.IntegrationTypes.EpepDocuments,
                SourceType = SourceTypeSelectVM.Document,
                SourceId = migrationInfo.AssignmentDocumentId.Value,
                ParentSourceId = caseMigrationId,
                MethodName = EpepConstants.EpepDocumentMethods.ForAssignment,
                DateWrt = DateTime.Now,
                UserId = ImpersonatedUserId ?? userContext.UserId,
                ErrorCount = 0,
                IntegrationStateId = EpepConstants.IntegrationStates.New
            };
            switch (migrationInfo.CaseMigrationTypeId)
            {
                case NomenclatureConstants.CaseMigrationTypes.SendCase_FromRandomAssignment:
                    mqItem.MethodName = EpepConstants.EpepDocumentMethods.ForAssignment;
                    break;
                case NomenclatureConstants.CaseMigrationTypes.SendCase_FromAssignmentByAddress:
                    mqItem.MethodName = EpepConstants.EpepDocumentMethods.ForAssignmentAddress;
                    break;
            }

            repo.Add(mqItem);
            await repo.SaveChangesAsync();
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

        public async Task<bool> RedirectTask(WorkTaskEditVM model)
        {
            try
            {
                var currentTask = repo.GetById<WorkTask>(model.Id);

                model.DescriptionCreated = currentTask.DescriptionCreated;

                if (!string.IsNullOrEmpty(currentTask.DescriptionCreated) && !string.IsNullOrEmpty(model.DescriptionRedirect))
                {
                    model.DescriptionCreated += ";" + model.DescriptionRedirect;
                }

                if (await CreateTask(model))
                {
                    currentTask.TaskStateId = WorkTaskConstants.States.Redirected;
                    currentTask.DateCompleted = DateTime.Now;
                    currentTask.Description = model.DescriptionRedirect;
                    await repo.SaveChangesAsync();

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
            Expression<Func<TaskType, bool>> whereGlobalAssignment = x => x.Id != WorkTaskConstants.Types.DocumentForGlobalAssignment;
            Expression<Func<TaskType, bool>> sourceIdWhere = x => true;
            Expression<Func<TaskType, bool>> whereDecisions = x => true;
            switch (sourceType)
            {
                case SourceTypeSelectVM.Document:
                    {
                        var _docInfo = repo.AllReadonly<Document>()
                                        .Include(x => x.DocumentGroup)
                                        .Where(x => x.Id == sourceId)
                                        .Select(x => new
                                        {
                                            x.CourtId,
                                            x.DocumentGroup.DocumentKindId,
                                            x.DocumentTypeId
                                        })
                                        .FirstOrDefault();
                        var hasDecisions = repo.AllReadonly<DocumentTypeDecisionType>()
                                                .Where(x => x.DocumentTypeId == _docInfo.DocumentTypeId)
                                                .Any();
                        switch (_docInfo.DocumentKindId)
                        {
                            case DocumentConstants.DocumentKind.InitialDocument:
                                if (_docInfo.CourtId == NomenclatureConstants.Courts.RandomAssignment)
                                {
                                    int[] acceptedTasks = { WorkTaskConstants.Types.ForDocumentResolution, WorkTaskConstants.Types.DocumentForGlobalAssignment };
                                    sourceIdWhere = x => acceptedTasks.Contains(x.Id);
                                    whereGlobalAssignment = x => true;
                                }
                                else
                                {
                                    int[] rejectedTasks = { WorkTaskConstants.Types.DocumentDecision, WorkTaskConstants.Types.DocumentForGlobalAssignment };
                                    sourceIdWhere = x => !rejectedTasks.Contains(x.Id);
                                }
                                break;
                            case DocumentConstants.DocumentKind.CompliantDocument:
                                int[] compliantDocOnlyTasks = { WorkTaskConstants.Types.Case_SelectLawUnit, WorkTaskConstants.Types.Case_ForReject };
                                sourceIdWhere = x => !compliantDocOnlyTasks.Contains(x.Id);
                                if (!hasDecisions)
                                {
                                    whereDecisions = x => x.Id != WorkTaskConstants.Types.DocumentDecision;
                                }
                                break;
                            case DocumentConstants.DocumentKind.InAdministrationDocument:
                                int[] inAdministrationDocAndReportOnlyTasks = { WorkTaskConstants.Types.Case_SelectLawUnit, WorkTaskConstants.Types.Case_ForReject, WorkTaskConstants.Types.ForReport };
                                sourceIdWhere = x => !inAdministrationDocAndReportOnlyTasks.Contains(x.Id);
                                if (!hasDecisions)
                                {
                                    whereDecisions = x => x.Id != WorkTaskConstants.Types.DocumentDecision;
                                }
                                break;
                            default:
                                int[] initialDocAndReportOnlyTasks = { WorkTaskConstants.Types.Case_SelectLawUnit, WorkTaskConstants.Types.Case_ForReject, WorkTaskConstants.Types.ForReport, WorkTaskConstants.Types.DocumentDecision };
                                sourceIdWhere = x => !initialDocAndReportOnlyTasks.Contains(x.Id);
                                if (!hasDecisions)
                                {
                                    whereDecisions = x => x.Id != WorkTaskConstants.Types.DocumentDecision;
                                }
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
                                .Where(whereDecisions)
                                .Where(whereGlobalAssignment)
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
            return (sourceCourtId == userContext.CourtId) || (sourceCourtId == null) || (sourceCourtId == NomenclatureConstants.Courts.RandomAssignment);
        }

        public async Task<SaveResultVM> ValidateBeforeCreate(WorkTaskEditVM model)
        {
            switch (model.TaskTypeId)
            {
                case WorkTaskConstants.Types.DocumentForGlobalAssignment:
                    bool hasOtherAssignmentTask = await repo.AllReadonly<WorkTask>()
                                                            .Where(x => x.SourceType == model.SourceType)
                                                            .Where(x => x.SourceId == model.SourceId)
                                                            .Where(x => x.TaskTypeId == model.TaskTypeId)
                                                            .Where(x => x.TaskStateId != WorkTaskConstants.States.Deleted)
                                                            .AnyAsync();
                    if (hasOtherAssignmentTask)
                    {
                        return new SaveResultVM(false, "Вече има добавена задача за Централизирано разпределение!");
                    }
                    bool is51_52 = await repo.AllReadonly<CaseClassification>()
                                            .Where(x => x.Case.DocumentId == model.SourceId)
                                            .Where(x => x.ClassificationId == NomenclatureConstants.CaseClassifications.FP_5152)
                                            .AnyAsync();
                    if (is51_52 == false)
                    {
                        bool hasRequestFile = await cdnService.Select(SourceTypeSelectVM.DocumentRequest, model.SourceId.ToString()).AnyAsync();
                        if (!hasRequestFile)
                        {
                            return new SaveResultVM(false, "Моля, Въведете данни в заявлението!");
                        }
                    }
                    int[] sourceTypes = { SourceTypeSelectVM.Document, SourceTypeSelectVM.DocumentFromElectronicDocument };
                    bool hasFiles = await cdnService.Select(sourceTypes, model.SourceId.ToString())
                                                    .Where(x => x.DateExpired == null)
                                                    .AnyAsync();
                    if (!hasFiles)
                    {
                        return new SaveResultVM(false, "Моля, прикачете поне един файл към документа!");
                    }
                    break;
            }
            return new SaveResultVM(true);
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
                    //var documentCaseInfo = repo.AllReadonly<Document>()
                    //                    .Include(x => x.DocumentCaseInfo)
                    //                    .Where(x => x.Id == sourceId)
                    //                    .Where(x => x.DocumentCaseInfo.Any())
                    //                    .Select(x => x.DocumentCaseInfo.FirstOrDefault())
                    //                    .FirstOrDefault();

                    var documentCaseInfo = repo.AllReadonly<Document>()
                                       .Include(x => x.DocumentCaseInfo)
                                       .Where(x => x.Id == sourceId)
                                       .SelectMany(x => x.DocumentCaseInfo)
                                       .TagWith(AuditConstants.TagNet8_1)
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
                string expireInfo = $"; Отменена на {DateTime.Now:dd.MM.yyyy HH:mm:ss} от {userContext.FullName};{description}";
                task.TaskStateId = WorkTaskConstants.States.Deleted;
                task.DescriptionCreated = (task.DescriptionCreated ?? "");
                task.DescriptionCreated += expireInfo;

                if (WorkTaskConstants.Types.ExpireConnectedTasks.Contains(task.TaskTypeId) && task.ParentTaskId > 0)
                {
                    expireConnectedSignTasks(task.ParentTaskId.Value, task.Id, expireInfo);
                }
            }

            repo.SaveChanges();
            return true;

        }

        private void expireConnectedSignTasks(long parentTaskId, long currentTaskId, string description)
        {
            var connectedTasks = repo.All<WorkTask>()
                                     .Where(x => x.ParentTaskId == parentTaskId && x.Id != currentTaskId)
                                     .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                                     .ToList();
            foreach (var task in connectedTasks)
            {
                task.TaskStateId = WorkTaskConstants.States.Deleted;
                task.DescriptionCreated = (task.DescriptionCreated ?? "");
                task.DescriptionCreated += description;
            }
        }

        public async Task<bool> RerouteTasks(long[] taskIds, WorkTaskManageVM model)
        {
            var tasks = await repo.All<WorkTask>()
                             .Where(x => taskIds.Contains(x.Id))
                             .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                             .Where(x => !WorkTaskConstants.Types.TaskCantReroute.Contains(x.TaskTypeId))
                             .ToListAsync();

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
                if (await CreateTask(newTask))
                {

                    task.TaskStateId = WorkTaskConstants.States.Redirected;
                    task.DescriptionCreated = (task.DescriptionCreated ?? "");
                    task.DescriptionCreated += $"; Пренасочена на {DateTime.Now:dd.MM.yyyy HH:mm:ss} от {userContext.FullName};{model.Description}";
                    //repo.Update(task);
                }
            }

            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<string> MakeSignComfirmMessage(WorkTask taskModel)
        {
            switch (taskModel.SourceType)
            {
                case SourceTypeSelectVM.CaseSessionAct:
                    var actInfo = await repo.AllReadonly<CaseSessionAct>()
                                            .Where(x => x.Id == (int)taskModel.SourceId)
                                            .Select(x => new
                                            {
                                                x.ActTypeId,
                                                x.IsFinalDoc
                                            }).FirstOrDefaultAsync();

                    if (NomenclatureConstants.ActType.SignComfirmMessage1.Contains(actInfo.ActTypeId) && actInfo.IsFinalDoc)
                    {
                        return "Актът, който ще подпишете е отразен като ФИНАЛИЗИРАЩ, с подписването му ще бъде генериран ECLI номер и същият подлежи на публикуване в ЦУБИПСА. С приключването на задачата за подпис съдебният акт ще бъде регистриран и няма да бъде възможно редактирането на параметър „Финализиращ акт“!";
                    }
                    if (NomenclatureConstants.ActType.SignComfirmMessage2.Contains(actInfo.ActTypeId) && !actInfo.IsFinalDoc)
                    {
                        return "Актът, който ще подпишете НЕ е отразен като финализиращ, при подписването му НЯМА да бъде генериран ECLI номер и същият няма да подлежи на публикуване в ЦУБИПСА. С приключването на задачата за подпис съдебният акт ще бъде регистриран и няма да бъде възможно редактирането на параметър „Финализиращ акт“!";
                    }
                    //Текст по подразбиране при подписване на последната задача за подпис на акт
                    return "С приключването на задачата за подпис съдебният акт ще бъде регистриран!";
            }
            return null;
        }

    }
}
