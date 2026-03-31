using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class CaseSessionActCoordinationService : BaseService, ICaseSessionActCoordinationService
    {
        public CaseSessionActCoordinationService(
        ILogger<CaseSessionActCoordinationService> _logger,
        IRepository _repo,
        IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }

        /// <summary>
        /// Извличане на данни за Съгласуване на актове
        /// </summary>
        /// <param name="CaseSessionActId"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionActCoordinationVM> CaseSessionActCoordination_Select(int CaseSessionActId, int? CaseSessionActCoordinationId = null, int coordinationType = NomenclatureConstants.CoordinationTypes.Act)
        {
            Expression<Func<CaseSessionActCoordination, bool>> whereSelect = x => x.CaseSessionActId == CaseSessionActId;
            if (CaseSessionActCoordinationId > 0)
            {
                whereSelect = x => x.Id == CaseSessionActCoordinationId;
            }
            Expression<Func<CaseSessionActCoordination, bool>> whereCoordinations = x => x.CoordinationType == coordinationType;
            if (coordinationType == NomenclatureConstants.NullVal)
            {
                whereCoordinations = x => true;
            }

            bool isGlobal = userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator);
            return repo.AllReadonly<CaseSessionActCoordination>()
                .Where(whereSelect)
                .Where(whereCoordinations)
                .Select(x => new CaseSessionActCoordinationVM()
                {
                    Id = x.Id,
                    CaseId = x.CaseId ?? 0,
                    CaseSessionActId = x.CaseSessionActId,
                    LawUnitId = x.CaseLawUnit.LawUnitId,
                    LawUnitTypeId = x.CaseLawUnit.LawUnit.LawUnitTypeId,
                    CaseLawUnitName = x.CaseLawUnit.LawUnit.FullName,
                    ActCoordinationTypeLabel = x.ActCoordinationType.Label,
                    JudgeRoleLabel = x.CaseLawUnit.JudgeRole.Label,
                    Content = x.Content,
                    ActCoordinationTypeId = x.ActCoordinationTypeId,
                    CoordinationType = x.CoordinationType,
                    ActTypeName = x.CaseSessionAct.ActType.Label,
                    ActNumber = x.CaseSessionAct.RegNumber,
                    ActDate = x.CaseSessionAct.ActDate,
                    CoordinationDeclaredDate = x.CoordinationDeclaredDate,
                    CanUpdate = x.CaseSessionAct.ActDeclaredDate == null && (x.CaseLawUnit.LawUnitId == userContext.LawUnitId || isGlobal)
                }).AsQueryable();
        }

        /// <summary>
        /// Запис на Съгласуване на актове
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseSessionActCoordination_SaveData(CaseSessionActCoordination model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseSessionActCoordination>(model.Id);
                    saved.Content = model.Content;
                    saved.ActCoordinationTypeId = model.ActCoordinationTypeId;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;

                    int taskType = WorkTaskConstants.Types.CaseSessionAct_Coordinate;
                    if (saved.CoordinationType == NomenclatureConstants.CoordinationTypes.Motive)
                    {
                        taskType = WorkTaskConstants.Types.CaseSessionAct_MotiveCoordinate;
                    }


                    var coordinationTasks = repo.All<WorkTask>()
                                                    .Where(x => x.SourceType == SourceTypeSelectVM.CaseSessionAct && x.SourceId == model.CaseSessionActId)
                                                    .Where(x => x.TaskTypeId == taskType)
                                                    .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                                                    .Where(x => x.UserId == userContext.UserId)
                                                    .ToList();
                    foreach (var _task in coordinationTasks)
                    {
                        _task.TaskStateId = WorkTaskConstants.States.Completed;
                        _task.DateCompleted = DateTime.Now;
                    }
                    repo.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на съгласуване Id={model.Id}");
            }
            return false;
        }

        public async Task<IEnumerable<CoordinationDepersonalizeVM>> GetDepersonalizationInfo(int actId)
        {
            return await repo.AllReadonly<CaseSessionActCoordination>()
                        .Where(x => x.CaseSessionActId == actId)
                        .Where(x => NomenclatureConstants.ActCoordinationTypes.WithOpinion.Contains(x.ActCoordinationTypeId))
                        .Select(x => new CoordinationDepersonalizeVM
                        {
                            Id = x.Id,
                            JudgeName = x.CaseLawUnit.LawUnit.FullName,
                            JudgeRole = x.CaseLawUnit.JudgeRole.Label,
                            HasPublicFile = x.DepersonalizeEndDate.HasValue,
                            HasSignedPrivateFile = x.CoordinationDeclaredDate.HasValue
                        }).ToListAsync().ConfigureAwait(false);
        }

        public bool RemoveDepersonalizationInfo(int id)
        {
            var model = repo.GetById<CaseSessionActCoordination>(id);
            model.DepersonalizeUserId = null;
            model.DepersonalizeEndDate = null;
            repo.SaveChanges();
            return true;
        }
    }
}
