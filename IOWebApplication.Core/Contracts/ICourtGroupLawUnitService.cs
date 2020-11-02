// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
  public interface ICourtGroupLawUnitService
  {
    IQueryable<MultiSelectTransferPercentVM> CourtGroupLawUnitSaved(int courtId, int courtGroupId);
    IQueryable<MultiSelectTransferPercentVM> CourtGroupLawUnitForSelect(int courtId);
    bool CourtGroupLawUnitSaveData(int courtId, int courtGroupId, List<MultiSelectTransferPercentVM> lawUnits);
    IQueryable<CourtLawUnitLoadVM> CourtGroup_LawUnitsHistory_Select(int courtGroupId);
  }
}
