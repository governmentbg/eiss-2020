using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class WorkingDaysService: BaseService, IWorkingDaysService
    {
        public WorkingDaysService(
           ILogger<WorkingDaysService> _logger,
           IRepository _repo,
           IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }
        public Dictionary<DateTime, int> GetWorkingDays(int courtId, DateTime dateFrom, DateTime dateTo)
        {
            Dictionary<DateTime, int> dictionayWD = new Dictionary<DateTime, int>();
            List<WorkingDay> wDays = repo.AllReadonly<WorkingDay>()
                                         .Where(x => dateFrom.Date <= x.Day.Date && x.Day.Date <= dateTo.Date)
                                         .ToList();
            DateTime aDate = dateFrom.Date;
            while (aDate <= dateTo.Date)
            {
                int? dayType = wDays.Where(x => x.CourtId == courtId && x.Day.Date == aDate.Date).Select(x => x.DayTypeId).FirstOrDefault();
                if ((dayType ?? 0) == 0)
                {
                    dayType = CommonContants.WorkingDays.WorkDay;
                    if (aDate.DayOfWeek == DayOfWeek.Saturday || aDate.DayOfWeek == DayOfWeek.Sunday)
                        dayType = CommonContants.WorkingDays.NotWorkDay;
                }
                dictionayWD.Add(aDate, (int)dayType);
                aDate = aDate.AddDays(1);
            }
            return dictionayWD;
        }
        public List<string> GetWorkingDaysMobile(int courtId)
        {
            DateTime dateWD = DateTime.Now.Date;
            var dictWD = GetWorkingDays(courtId, dateWD, dateWD.AddDays(60));
            return dictWD.Where(kv => kv.Value == CommonContants.WorkingDays.NotWorkDay).Select(x => x.Key.ToString("yyyyMMdd")).ToList();
        }
        public bool IsWorkingDay(int courtId, DateTime date)
        {
            var dictWD = GetWorkingDays(courtId, date.Date, date.Date);
            if (dictWD.ContainsKey(date.Date))
                return dictWD[date.Date] == CommonContants.WorkingDays.WorkDay;
            return true;
        }
    }
}
