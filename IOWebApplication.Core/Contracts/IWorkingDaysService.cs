using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IWorkingDaysService : IBaseService
    {
        Dictionary<DateTime, int> GetWorkingDays(int courtId, DateTime dateFrom, DateTime dateTo);
        List<string> GetWorkingDaysMobile(int courtId);
        bool IsWorkingDay(int courtId, DateTime date);
    }
}
