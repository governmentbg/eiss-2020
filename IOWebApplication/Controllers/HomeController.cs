// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    //[Authorize]
    [DisableAudit]
    public class HomeController : BaseController
    {
        public static string ControlerName = "Home";
        private readonly ILogger<HomeController> logger;
        private readonly INewsService newsService;

        public HomeController(
            ILogger<HomeController> _logger,
            INewsService _newsService)
        {
            logger = _logger;
            newsService = _newsService;
        }

        //[Route("signin-stampit?{error}")]
        //[AllowAnonymous]

        //public IActionResult StampitError(string error = null, string error_reason = null)
        //{
        //    throw new Exception("Невалиден сертификат");
        //}

        public async Task<IActionResult> Index()
        {
            SetHelpFile(HelpFileValues.HomeDashboard);
            ViewBag.userSettings = await userContext.Settings();
            return View();
        }
        public IActionResult AccessDenied(string message = null)
        {
            ViewBag.message = message ?? "Нямате достъп до избрания от Вас ресурс или функционалност.";
            return View();
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            var feature = this.HttpContext.Features.Get<IExceptionHandlerFeature>();
            var error = feature.Error.Message;
            var errorModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                Message = error,
                InnerException = feature.Error.InnerException?.Message
            };
            if (feature.Error is NotFoundException)
            {
                errorModel.Title = "Ненамерен ресурс";
            }
            logger.LogError(feature.Error, "EissWeb");
            return View(errorModel);
        }
    }
}
