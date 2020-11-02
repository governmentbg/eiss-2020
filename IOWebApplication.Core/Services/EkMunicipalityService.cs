// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

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
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using IOWebApplication.Infrastructure.Extensions;

namespace IOWebApplication.Core.Services
{
    public class EkMunicipalityService : BaseService, IEkMunicipalityService
    {
        public EkMunicipalityService(
            ILogger<EkMunincipality> _logger,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }
        public IQueryable<EkMunincipality> EkMunicipalitySelect()
        {
            return repo.AllReadonly<EkMunincipality>()
                .Select(x => x)
                .AsQueryable();
        }
     
        public List<SelectListItem> GetDropDownList(bool addDefaultElement = true, bool addAllElement = false) 
        {
            var result = repo.AllReadonly<EkMunincipality>()
                        .OrderBy(x => x.Name)
                        .Select(x => new SelectListItem()
                        {
                            Text = x.Name,
                            Value = x.MunicipalityId.ToString()
                        }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }
            return result;
        }

    }
}
