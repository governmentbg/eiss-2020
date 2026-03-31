using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using iText.IO.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;

namespace IOWebApplication.Core.Services
{
    public class InterestRateService : BaseService, IInterestRateService
    {
        public InterestRateService(
            ILogger<InterestRateService> _logger,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }

        public string GetInterestTypeName(int interestType)
        {
            switch (interestType)
            {
                case NomenclatureConstants.InterestRateTypes.OLP:
                    return "Основен лихвен процент";
                default:
                    return "n/a";
            }
        }

        public IQueryable<InterestRateDateVM> Select(FilterInterestRate filter)
        {
            Expression<Func<InterestRate, bool>> whereDateFrom = x => true;
            if (filter.DateFrom.HasValue)
            {
                whereDateFrom = x => x.Date >= filter.DateFrom.Value.Date;
            }
            Expression<Func<InterestRate, bool>> whereDateTo = x => true;
            if (filter.DateTo.HasValue)
            {
                whereDateTo = x => x.Date <= filter.DateTo.MakeEndDate();
            }
            return repo.AllReadonly<InterestRate>()
                        .Where(x => x.InterestType == filter.InterestType)
                        .Where(whereDateFrom)
                        .Where(whereDateTo)
                        .Select(x => new InterestRateDateVM
                        {
                            Id = x.Id,
                            Date = x.Date,
                            Rate = x.Rate,
                        });
        }

        public async Task<SaveResultVM> SaveData(InterestRate model)
        {
            try
            {
                model.DateWrt = DateTime.Now;
                model.UserId = userContext.UserId;

                if (model.Id > 0)
                {
                    var saved = await GetByIdAsync<InterestRate>(model.Id);
                    saved.Date = model.Date;
                    saved.Rate = model.Rate;
                    saved.IsActive = model.IsActive;
                    saved.DateWrt = model.DateWrt;
                    saved.UserId = model.UserId;
                }
                else
                {
                    await repo.AddAsync(model);
                }
                await repo.SaveChangesAsync();
                return new SaveResultVM(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(SaveData));
                return new SaveResultVM(false);
            }
        }

        /// <summary>
        /// Изчислява лихви с ОЛП + надбавка
        /// </summary>
        /// <param name="amount">Основа</param>
        /// <param name="fromDate">От дата</param>
        /// <param name="toDate">до дата</param>
        /// <param name="addProcent">Надбавка над ОЛП, за Законова лихва 10%</param>
        /// <returns></returns>
        public async Task<decimal> CalcOLPRates(decimal amount, DateTime fromDate, DateTime toDate, decimal addProcent)
        {
            if (fromDate > toDate)
            {
                return -1;
            }

            var rates = await initRates(fromDate, toDate, NomenclatureConstants.InterestRateTypes.OLP, addProcent);

            decimal totalInterest = 0M;
            var calcDate = fromDate;
            do
            {
                //Взема ОЛП + Надбавка за съотвения ден
                decimal dayInterest = getInterestRateToDate(rates, calcDate.Date);

                //Изчислява сумата за деня на база 360 дни
                totalInterest += ((dayInterest / 360) * amount) / 100M;
                calcDate = calcDate.AddDays(1);

            } while (calcDate < toDate.AddDays(1));

            return Math.Round(totalInterest, 2);
        }

        async Task<List<InterestRateDateVM>> initRates(DateTime fromDate, DateTime toDate, int interestType, decimal addProcent = 0M)
        {
            var firstInterestDate = await repo.AllReadonly<InterestRate>()
                                .Where(x => x.Date.Date <= fromDate.Date)
                                .Where(x => x.InterestType == interestType)
                                .Where(x => x.IsActive == true)
                                .OrderByDescending(x => x.Date)
                                .Select(x => x.Date).FirstOrDefaultAsync();


            List<InterestRateDateVM> result = await repo.AllReadonly<InterestRate>()
                                .Where(x => x.Date.Date >= firstInterestDate)
                                .Where(x => x.Date.Date <= toDate.Date)
                                .Where(x => x.InterestType == interestType)
                                .Where(x => x.IsActive == true)
                                .OrderBy(x => x.Date)
                                .Select(x => new InterestRateDateVM
                                {
                                    Date = x.Date.Date,
                                    Rate = x.Rate + addProcent,
                                }).ToListAsync();

            if (result.Count == 0)
            {
                var lastDate = await repo.AllReadonly<InterestRate>()
                               .Where(x => x.Date.Date >= fromDate.Date)
                               .Where(x => x.Date.Date <= toDate.Date)
                               .Where(x => x.InterestType == interestType)
                               .Where(x => x.IsActive == true)
                               .OrderBy(x => x.Date)
                               .Select(x => new InterestRateDateVM
                               {
                                   Date = x.Date.Date,
                                   Rate = x.Rate + addProcent,
                               }).ToListAsync();
            }
            return result;
        }

        decimal getInterestRateToDate(List<InterestRateDateVM> rates, DateTime currentDate)
        {
            return rates.Where(x => x.Date.Date <= currentDate)
                        .OrderBy(x => x.Date)
                        .Select(x => x.Rate)
                        .FirstOrDefault();
        }


    }
}
