using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IMigrationDataService : IBaseService
    {
        string MigrateForCourt(int courtId);

        string MigrateLawyers();

        string MigrateLoadGroupLinkFromCourtType(int fromCourtType, int fromInstance, int toCourtType, int toInstance, int[] caseGroups);
    }
}
