// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class DeliveryAccountController : BaseController
    {
        private readonly IDeliveryAccountService deliveryAccountService;
        private readonly ICommonService commonService;
        public DeliveryAccountController(
            IDeliveryAccountService _deliveryAccountService,
            ICommonService _commonService)
        {
            deliveryAccountService = _deliveryAccountService;
            commonService = _commonService;
        }
        public IActionResult AddBarcodeTying(string userId)
        {
           (string addr, string barcode64) = deliveryAccountService.GenerateBarcodeTying(userId);
            ViewBag.addr = addr;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_AccountMobileTokenRegister(userId).DeleteOrDisableLast();
            return View(nameof(BarcodeTying), barcode64);
        }
        public IActionResult BarcodeTying(string id)
        {
            (string addr, string barcode64) = deliveryAccountService.GetBarcodeTying(id);
            ViewBag.addr = addr;
            var account = deliveryAccountService.GetById<DeliveryAccount>(id);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_AccountMobileTokenRegister(account.MobileUserId).DeleteOrDisableLast();
            return View(nameof(BarcodeTying), barcode64);
        }
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, string userId)
        {
            var data = deliveryAccountService.GetDeliveryTokenForUser(userId);
            return request.GetResponse(data);
        }
        public IActionResult Index(string userId)
        {
            ViewBag.newCount = deliveryAccountService.TokenForUserNewCount(userId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_AccountMobileToken(userId).DeleteOrDisableLast();
            var user = deliveryAccountService.GetById<ApplicationUser>(userId);
            ViewBag.userName = user?.Email;
            return View(nameof(Index), userId);
        }
        [HttpPost]
        public IActionResult ExpiredInfo(ExpiredInfoVM model)
        {
            if (deliveryAccountService.SaveExpireInfoPlus(model))
            {
                SetSuccessMessage(MessageConstant.Values.DeliveryAccountExpireOK);
                var account = deliveryAccountService.GetById<DeliveryAccount>(model.ReturnUrl);
                return Json(new { result = true, redirectUrl = Url.Action("Index", new { userId = account.MobileUserId }) }); 
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }
    }
}