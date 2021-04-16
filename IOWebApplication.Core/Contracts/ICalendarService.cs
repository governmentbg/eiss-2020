using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Collections.Generic;

namespace IOWebApplication.Core.Contracts
{
    public interface ICalendarService : IBaseService
    {
        IEnumerable<CalendarVM> SelectByPerson(DateTime start,DateTime end);
        IEnumerable<CalendarVM> SelectSessionHallUse(int CourtHallId, DateTime start, DateTime end);

    }
}
