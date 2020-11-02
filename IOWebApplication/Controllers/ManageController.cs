// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class ManageController : BaseController
    {
        public IActionResult ExpiredInfo(int id, long longId, string submitUrl, string returnUrl)
        {
            var model = new ExpiredInfoVM()
            {
                Id = id,
                LongId = longId,
                ExpireSubmitUrl = submitUrl,
                ReturnUrl = returnUrl
            };
            return PartialView(model);
        }
    }
}