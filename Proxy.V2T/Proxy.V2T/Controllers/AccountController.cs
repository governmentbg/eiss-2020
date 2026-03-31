// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using Proxy.EISS.Models;
using System.Threading.Tasks;
using Proxy.EISS.Services;
using Proxy.EISS.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;

namespace Proxy.EISS.Controllers
{
    [Route("[controller]")]
    [Produces("application/json")]
    [ApiController]
    [EnableCors(policyName: "allowAll")]
    [AllowAnonymous]
    public class AccountController : ControllerBase
    {
        private readonly int TOKEN_EXPIRES_IN_MINUTES = 60;
        private readonly IApiService apiService;
        public AccountController(IApiService apiService)
        {
            this.apiService = apiService;
        }

        [HttpGet]
        [Route("Health", Name = "Health")]
        public string Health()
        {
            return "EISS.ProxyAPI - Healthy";
        }

        /// <summary>
        /// Метод за генериране на ауторизационен токън
        /// Headers.Authorization: Bearer token.{appKey}
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet("GetToken", Name = "GetToken")]
        public async Task<AuthTokenVM> GetToken(ApiTokenRequestVM model)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            if (model == null || string.IsNullOrEmpty(model.Data) || string.IsNullOrEmpty(model.Hash))
            {
                return new AuthTokenVM(false, "Invalid data.");
            }
            var token = Request.GetBearerToken();
            if (string.IsNullOrEmpty(token))
            {

                return new AuthTokenVM(false, "Invalid Token provided.");
            }
            var parts = token.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {

                return null;
            }
            string appKeyFromToken = parts[1];

            if (model.Data != DateTime.Now.ToString("yyyyMMdd"))
            {
                return new AuthTokenVM(false, "Invalid data key.");
            }

            var dataContent = System.Text.Encoding.UTF8.GetBytes(model.Data);
            SaveResultVM getSecretResult = await apiService.GetAppSecretByKey(appKeyFromToken);
            if (!getSecretResult.Result)
            {
                return new AuthTokenVM(false, getSecretResult.ErrorMessage ?? "Invalid app key");
            }
            var appSecret = getSecretResult.ObjectId.ToString();
            if (!AuthExtensions.CheckHashData(dataContent, model.Hash, appSecret))
            {
                return new AuthTokenVM(false, "Invalid request hash");
            }

            var loginToken = GenerateToken(64);

            var result = new AuthTokenVM(true)
            {
                Token = loginToken,
                ExpiresIn = DateTime.Now.AddMinutes(TOKEN_EXPIRES_IN_MINUTES)
                //ExpiresIn = DateTime.Now.AddDays(TOKEN_EXPIRES_IN_MINUTES)
            };
            var updateResult = await apiService.UpdateTokenKey(appKeyFromToken, result.Token, result.ExpiresIn);
            if (!updateResult.Result)
            {
                return new AuthTokenVM(false);
            }

            Response.StatusCode = StatusCodes.Status200OK;
            return result;
        }

        //[HttpGet("NewKey", Name = "NewKey")]
        //public async Task<SaveResultVM> NewKey()
        //{
        //    var newKeyResult = await apiService.CreateApiKey(GenerateToken(12), GenerateToken(32), "Интеграция ЦАИС Съдебен статус");
        //    return newKeyResult;
        //}

        /// <summary>
        /// Generate a fixed length token that can be used in url without endcoding it
        /// </summary>
        /// <returns></returns>
        private string GenerateToken(int numberOfBytes = 32)
        {
            byte[] ret = new byte[numberOfBytes];
            Span<byte> spanB = new Span<byte>(ret);
            RandomNumberGenerator.Fill(spanB);
            var result = WebEncoders.Base64UrlEncode(spanB);
            result = result.Replace("_", "").Replace("-", "d").ToLower();
            return result;
        }
    }
}
