// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

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
