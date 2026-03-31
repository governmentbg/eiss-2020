// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class EpepCaseMigrationService : EpepService, IEpepCaseMigrationService
    {
        private readonly ICaseMigrationService caseMigrationService;
        public EpepCaseMigrationService(
            IRepository _repo,
            IEpepConnectionService _connector,
            ILogger<EpepService> _logger,
            IConfiguration configuration,
            ICdnService _cdnService,
            ICaseMigrationService _caseMigrationService,
            IDeliveryItemService _deliveryItemService) : base(_repo, _connector, _logger, configuration, _cdnService, _deliveryItemService)
        {
            caseMigrationService = _caseMigrationService;
        }

        public async Task<bool> FetchCaseMigrations()
        {
            try
            {
                if (!await InitChanel())
                {
                    return false;
                }

                await ManageResultCaseMigration();

            }
            finally
            {
                await CloseChanel();
            }
            return true;
        }

        private async Task ManageResultCaseMigration()
        {
            var courtEissIds = (await repo.AllReadonly<CodeMapping>()
                                        .Where(x => x.Alias == EpepConstants.Nomenclatures.CaseMigrationCourts)
                                        .Where(x => x.OuterCode == "yes")
                                        .Select(x => x.InnerCode)
                                        .ToListAsync()).ToArray();

            var courtEpepCodes = await repo.AllReadonly<CodeMapping>()
                                        .Where(x => x.Alias == EpepConstants.Nomenclatures.Courts)
                                        .Where(x => courtEissIds.Contains(x.InnerCode))
                                        .Select(x => x.OuterCode)
                                        .ToListAsync();
            try
            {
                foreach (var courtCode in courtEpepCodes)
                {
                    var guidList = await serviceClient.GetFinishedCaseMigrationsAsync(courtCode, null);
                    foreach (var guid in guidList)
                    {
                        try
                        {
                            await ResultCaseMigration(guid);
                        }
                        catch (Exception ex)
                        {

                            logger.LogError(ex, $"FetchCaseMigrations;MigrationGid:{guid};CourtCode:{courtCode}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                logger.LogError(ex, $"GetFinishedCaseMigrationsAsync");
            }
        }

        private async Task ResultCaseMigration(Guid gid)
        {
            var caseMigrationFromEPEP = await serviceClient.GetResultCaseMigrationAsync(gid);

            int caseMigrationId = (int)getSourceIdByOuterCode(SourceTypeSelectVM.CaseMigrationRegistration, gid.ToString());

            if (caseMigrationId == 0)
            {
                return;
            }

            var outMigration = repo.GetById<CaseMigration>(caseMigrationId);

            string result = "";
            if (caseMigrationFromEPEP.Results != null)
            {
                result = string.Join(';', caseMigrationFromEPEP.Results);
            }

            int? migrationKind = null;
            if (caseMigrationFromEPEP.HasResultCase && caseMigrationFromEPEP.CaseId != null && caseMigrationFromEPEP.CaseId != Guid.Empty)
            {
                migrationKind = NomenclatureConstants.CaseMigrationKinds.EpepInMigration;
            }

            var inResult = caseMigrationService.AcceptCaseMigration(outMigration.Id, outMigration.CaseId, result, false, migrationKind);
            if (inResult.Result)
            {
                if (migrationKind.HasValue)
                {

                    var summaryCase = await serviceClient.GetSummaryCaseAsync(caseMigrationFromEPEP.CaseId.Value);

                    var jsonSummaryCase = Newtonsoft.Json.JsonConvert.SerializeObject(summaryCase);

                    await cdnService.MongoCdn_AppendUpdate(new IOWebApplication.Infrastructure.Models.Cdn.CdnUploadRequest()
                    {
                        SourceType = SourceTypeSelectVM.CaseMigrationRegistration,
                        SourceId = inResult.ObjectId.ToString(),
                        FileName = "summaryCase.json",
                        FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonSummaryCase))
                    });

                    AddIntegrationKey(SourceTypeSelectVM.CaseMigrationRegistration, (int)inResult.ObjectId, gid.ToString());
                }
                await serviceClient.EndProcessCaseMigrationAsync(gid);

            }
            else
            {
                if (inResult.SaveMethod == "exists")
                {
                    await serviceClient.EndProcessCaseMigrationAsync(gid);
                }
                //logger.LogError($"AcceptCaseMigration : {inResult.ErrorMessage}");
            }
        }
    }
}
