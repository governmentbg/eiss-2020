// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IOWebApplication.Components
{
    public class UserInfoComponent : ViewComponent
    {
        private readonly IUserContext userContext;
        public UserInfoComponent(IUserContext _userContext)
        {
            userContext = _userContext;
        }
        public async Task<IViewComponentResult> InvokeAsync(string info)
        {
            switch (info)
            {
                case "CourtInfo":
                    var courtName = userContext.CourtName;
                    return await Task.FromResult<IViewComponentResult>(View(info, courtName));
            }
            return null;
        }
    }
}
