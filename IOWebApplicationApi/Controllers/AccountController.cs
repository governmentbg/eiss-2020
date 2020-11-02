// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplicationApi.Contracts;
using IOWebApplicationApi.Data.Models;
using IOWebApplicationApi.Models.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;


namespace IOWebApplicationApi.Controllers
{
    // [Authorize]
    [Authorize(AuthenticationSchemes = "MobileBearer")]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly IAccountService accountService;
        public AccountController(
            IAccountService _accountService
            )
        {
            accountService = _accountService;
        }

        [AllowAnonymous]
        [HttpPost("register/{registerGuid}")]
        [HttpGet("register/{registerGuid}")]
        public async Task<JsonResult> Register(string registerGuid)
        {
            DeliveryAccountVM result = await accountService.Register(registerGuid);
            return Json(result);
        }

        [HttpPost("SavePin")]
        public JsonResult SavePin([FromBody]AuthenticateModel model)
        {
            var result = accountService.SavePin(model.RegisterGuid, model.PassWord);
            return Json(result);
        }
        
        [HttpPost("LoginPin")]
        public async Task<JsonResult> LoginPin([FromBody]AuthenticateModel model)
        {
            var result = await accountService.LoginPin(model.RegisterGuid, model.PassWord);
            return Json(result);
        }

        [HttpPost("test")]
        public JsonResult Test()
        {
            return Json(new {data1 = "Data1", data2 = "Data2" }); 
        }
    }
}
