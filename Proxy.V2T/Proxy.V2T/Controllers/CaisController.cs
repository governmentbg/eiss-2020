// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Contracts.Integration;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Models.Integrations.Cais;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Integrations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Proxy.EISS.Contracts;
using Proxy.EISS.Services;
using System;
using System.Threading.Tasks;

namespace Proxy.EISS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CaisController : ControllerBase
    {

        private readonly ILogger<CaisController> _logger;
        private readonly ICaisBuletinService caisBuletinService;
        private readonly ICaisMapperService caisMapperService;
        private readonly ICaisConnectionFactory caisFactory;
        private readonly IApiService apiService;

        public CaisController(
            ILogger<CaisController> logger,
            ICaisBuletinService caisBuletinService,
            ICaisMapperService caisMapperService,
            ICaisConnectionFactory caisFactory,
            IApiService apiService)
        {
            _logger = logger;
            this.caisBuletinService = caisBuletinService;
            this.caisMapperService = caisMapperService;
            this.caisFactory = caisFactory;
            this.apiService = apiService;
        }



        /// <summary>
        /// Услуга за отразяване регистрирането на бюлетин в бюро Съдимост
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("RegisterBulletin", Name = "RegisterBulletin")]
        public async Task<RegisterBulletinResultModel> RegisterBulletin(RegisterBulletinRequestModel model)
        {
            try
            {
                var token = Request.GetBearerToken();
                if (string.IsNullOrEmpty(token))
                {
                    return new RegisterBulletinResultModel()
                    {
                        Result = false,
                        ErrorCode = "101",
                        Message = "Липсващ и/или невалиден token."
                    };
                }
                var validateTokenResult = await apiService.ValidateToken(token);
                if (!validateTokenResult.Result)
                {
                    return new RegisterBulletinResultModel()
                    {
                        Result = false,
                        ErrorCode = "102",
                        Message = "Липсващ и/или невалиден token."
                    };
                }


                var result = await caisBuletinService.RegisterBulletinCallback(model);
                return new RegisterBulletinResultModel()
                {
                    Result = result.Result,
                    ErrorCode = (!result.Result) ? "103" : string.Empty,
                    Message = result.Content
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RegisterBulletin; BulletinNumber:{model.BulletinNumber}");
                return new RegisterBulletinResultModel()
                {
                    Result = false,
                    ErrorCode = "104",
                    Message = "Server error"
                };
            }
        }

        /// <summary>
        /// Услуга за валидиране на бюлетин преди регистрирането му в бюро Съдимост
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ValidateBulletin", Name = "ValidateBulletin")]
        public async Task<SaveResultVM> ValidateBulletin(int bulletinId)
        {
            try
            {
                CaisBuletinModel model = await caisBuletinService.InitBuletinModel(bulletinId);

                var xmlData = caisMapperService.GetXml(model);
                caisFactory.CreateClient();
                return await caisFactory.SendDataToCais(xmlData, "ValidateBulletin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ValidateBulletin; BulletinId:{bulletinId}");
                return new SaveResultVM(false);
            }
        }
    }
}
