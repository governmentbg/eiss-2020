// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IEkMunicipalityService
    {
        IQueryable<EkMunincipality> EkMunicipalitySelect();
        List<SelectListItem> GetDropDownList(bool addDefaultElement = true, bool addAllElement = false);
    }
}
