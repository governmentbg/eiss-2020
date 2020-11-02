// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

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
