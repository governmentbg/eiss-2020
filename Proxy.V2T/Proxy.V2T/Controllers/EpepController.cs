// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Integration.Epep;
using IOWebApplication.Infrastructure.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using RestEpep = IOWebApplication.Infrastructure.Models.Integrations.EpepRest;

namespace Proxy.EISS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EpepController : ControllerBase
    {
        private readonly IEpepRestClient epepRestClient;
        private readonly ILogger<EpepController> logger;
        public EpepController(
            IEpepRestClient epepRestClient,
            ILogger<EpepController> logger)
        {
            this.epepRestClient = epepRestClient;
            this.logger = logger;
        }

        [HttpGet]
        [Route("GetResultCaseMigration/{gid}", Name = "GetResultCaseMigration")]
        public Task<CaseMigrationResult> GetResultCaseMigration(Guid gid)
        {
            return queryRestEpep(async (epepClient) =>
            {
                if (epepClient == null)
                {
                    return null;
                }
                return await epepClient.GetResultCaseMigration(gid);
            }, $"{nameof(IEpepRestClient.GetResultCaseMigration)} : {gid}");
        }

        [HttpGet]
        [Route("GetSummaryCase/{gid}", Name = "GetSummaryCase")]
        public Task<SummaryCase> GetSummaryCase(Guid gid)
        {
            return queryRestEpep(async (epepClient) =>
            {
                if (epepClient == null)
                {
                    return null;
                }
                return await epepRestClient.GetSummaryCase(gid);
            }, $"{nameof(IEpepRestClient.GetSummaryCase)} : {gid}");
        }

        [HttpGet]
        [Route("GetExecProcess/{gid}", Name = "GetExecProcess")]
        public Task<RestEpep.ExecProcessDetailsVM> GetExecProcess(Guid gid)
        {
            return queryRestEpep(async (epepClient) =>
            {
                if (epepClient == null)
                {
                    return null;
                }
                return await epepClient.GetExecProcessById(gid);
            }, $"{nameof(IEpepRestClient.GetExecProcessById)} : {gid}");
        }

        async Task<Tres> queryRestEpep<Tres>(Func<IEpepRestClient, Task<Tres>> func, string funcName = "") where Tres : class
        {
            Tres result = null;
            try
            {
                epepRestClient.InitClient();
                result = await func(epepRestClient);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, funcName);
            }
            finally
            {

            }
            return result;

        }
    }
}
