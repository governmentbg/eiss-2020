using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.Extensions.Logging;
using IOWebApplication.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using IOWebApplication.Infrastructure.Constants;
using iText.Kernel.XMP.Impl;

namespace IOWebApplication.Core.Services
{
    public class CaseDeactivationService : BaseService, ICaseDeactivationService
    {
        private readonly ICaseDeadlineService caseDeadlineService;
        private readonly ICourtLoadPeriodService courtLoadPeriodService;
        private readonly IMQEpepService mqEpepService;
        public CaseDeactivationService(
            ILogger<CaseDeactivationService> _logger,
            INomenclatureService _nomService,
            IRepository _repo,
            IUserContext _userContext,
            ICaseDeadlineService _caseDeadlineService,
            ICourtLoadPeriodService _courtLoadPeriodService,
            IMQEpepService _mqEpepService
        )
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            caseDeadlineService = _caseDeadlineService;
            courtLoadPeriodService = _courtLoadPeriodService;
            mqEpepService = _mqEpepService;
        }

        public IQueryable<CaseDeactivationVM> Select(CaseDeactivationFilterVM filter)
        {
            return repo.AllReadonly<CaseDeactivation>()
                            .Include(x => x.Case)
                            .ThenInclude(x => x.CourtGroup)
                            .Include(x => x.Case)
                            .ThenInclude(x => x.CaseCode)
                            .Include(x => x.Case)
                            .ThenInclude(x => x.CaseType)
                            .Include(x => x.Case)
                            .ThenInclude(x => x.Document)
                            .Include(x => x.DeactivateUser)
                            .ThenInclude(x => x.LawUnit)
                            .Include(x => x.Court)
                            .Where(x => x.CourtId == userContext.CourtId)
                            .Where(x => (x.DeclaredDate ?? x.DateWrt) >= filter.DateFrom.OrMinDate())
                            .Where(x => (x.DeclaredDate ?? x.DateWrt) <= filter.DateTo.OrMaxDate())
                            .Where(x => EF.Functions.ILike(x.Case.RegNumber, filter.RegNumber.ToEndingPaternSearch()))
                            .Where(x => x.Id == (filter.Id ?? x.Id))
                            .Where(FilterExpireInfo<CaseDeactivation>(false))
                            .Select(x => new CaseDeactivationVM
                            {
                                Id = x.Id,
                                CaseId = x.CaseId,
                                CaseNumber = x.Case.RegNumber,
                                CaseDate = x.Case.RegDate,
                                CourtName = x.Court.Label,
                                CourtGroupName = (x.Case.CourtGroup != null) ? x.Case.CourtGroup.Label : "",
                                CaseTypeName = x.Case.CaseType.Label,
                                CaseCodeName = x.Case.CaseCode.Label,
                                DocumentNumber = $"{x.Case.Document.DocumentNumber}/{x.Case.Document.DocumentDate:dd.MM.yyyy}",
                                DeclaredDate = x.DeclaredDate,
                                Description = x.Description,
                                DateWrt = x.DateWrt,
                                DeactivateUserName = x.DeactivateUser.LawUnit.FullName,
                                DeactivateUserUIC = x.DeactivateUser.LawUnit.Uic
                            }).AsQueryable();
        }

        public SaveResultVM Add(CaseDeactivation model)
        {
            try
            {
                if (model.CaseId <= 0)
                {
                    return new SaveResultVM(false, "Изберете дело");
                }
                var caseModel = repo.GetById<Case>(model.CaseId);
                if (!NomenclatureConstants.CaseState.CanDeleteStates.Contains(caseModel.CaseStateId))
                {
                    return new SaveResultVM(false, "Статуса на избраното дело не позволява да бъде анулирано.");
                }
                if (string.IsNullOrEmpty(model.Description))
                {
                    return new SaveResultVM(false, "Въведете основание");
                }
                model.CourtId = userContext.CourtId;
                model.DeactivateUserId = userContext.UserId;
                SetUserDateWRT(model);
                repo.Add(model);
                repo.SaveChanges();

                return new SaveResultVM(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return new SaveResultVM(false);
            }
        }

        public bool DeclareDeactivation(int id)
        {
            try
            {
                var model = repo.GetById<CaseDeactivation>(id);

                if (model == null)
                {
                    return false;
                }
                var caseModel = repo.GetById<Case>(model.CaseId);

                model.DeclaredDate = DateTime.Now;
                repo.Update(model);

                caseModel.CaseStateId = NomenclatureConstants.CaseState.Deleted;
                caseModel.CaseStateDescription = model.Description;
                repo.Update(caseModel);

                mqEpepService.AppendCase(caseModel, EpepConstants.ServiceMethod.Delete);
                caseDeadlineService.DeadLineOnCase(caseModel);
                repo.SaveChanges();

                var judgeReportedId = repo.AllReadonly<CaseLawUnit>()
                            .Where(x => x.CaseId == caseModel.Id && x.CaseSessionId == null)
                            .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                            .Where(x => x.DateFrom <= model.DeclaredDate && (x.DateTo ?? DateTime.MaxValue) >= model.DeclaredDate)
                            .Select(x => x.Id)
                            .FirstOrDefault();

                if (judgeReportedId > 0)
                {
                    courtLoadPeriodService.UpdateDailyLoadPeriod_RemoveByDismisal(judgeReportedId);
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return false;
            }
        }
    }
}
