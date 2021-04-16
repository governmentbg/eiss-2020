using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class CaseMoneyService : BaseService, ICaseMoneyService
    {
        public CaseMoneyService(
            ILogger<CaseMoneyService> _logger,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }

        /// <summary>
        /// Извличане на данни за Суми по дела/заседание/участник в заседание
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public IQueryable<CaseMoneyListVM> CaseMoney_Select(int caseId, int? caseSessionId)
        {
            return repo.AllReadonly<CaseMoney>()
                .Include(x => x.MoneyType)
                .Include(x => x.CaseSessionLawUnit)
                .ThenInclude(x => x.LawUnit)
                .Where(x => x.CaseId == caseId && (x.CaseSessionId ?? 0) == (caseSessionId ?? 0))
                .Select(x => new CaseMoneyListVM()
                {
                    Id = x.Id,
                    CaseId = x.CaseId,
                    CaseSessionId = x.CaseSessionId,
                    CaseLawUnitName = x.CaseSessionLawUnit.LawUnit.FullName,
                    MoneyTypeName = x.MoneyType.Label,
                    Amount = x.Amount,
                    PaidDate = x.PaidDate,
                    MoneySignName = x.MoneyType.MoneySign == 1 ? "Приход" : "Разход"
                }).AsQueryable();
        }

        /// <summary>
        /// Запис на Суми по дела/заседание/участник в заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseMoney_SaveData(CaseMoney model)
        {
            try
            {
                model.CaseLawUnitId = (model.CaseLawUnitId ?? -1) <= 0 ? null : model.CaseLawUnitId;
                if (model.Id > 0)
                {
                    var caseMoney = repo.GetById<CaseMoney>(model.Id);
                    caseMoney.CaseLawUnitId = model.CaseLawUnitId;
                    caseMoney.MoneyTypeId = model.MoneyTypeId;
                    caseMoney.Amount = model.Amount;
                    caseMoney.PaidDate = model.PaidDate;
                    caseMoney.Description = model.Description;
                    caseMoney.UserId = userContext.UserId;
                    caseMoney.DateWrt = DateTime.Now;

                    repo.Update(caseMoney);
                    repo.SaveChanges();
                }
                else
                {
                    model.UserId = userContext.UserId;
                    model.DateWrt = DateTime.Now;

                    repo.Add<CaseMoney>(model);
                    repo.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CaseMoney Id={ model.Id }");
                return false;
            }
        }

    }
}
