using IOWebApplication.Infrastructure.Data.Models.EISPP;
using IOWebApplication.Infrastructure.Models.Integrations.Eispp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IEisppRulesService
    {
        Task<string> ApplyRules(int structureId, string message, int eventType);
        void CreatePunismentFromProbationMeasuares(EisppPackage model);
        (string[], int) GetEisppRuleIds(int eventType, string propName);
        string GetEisppRuleValue(int eventType, string propName);
        string GetPunishmentKindMode(int punishmentKind);
        int GetResSidFromRules(int eventType);
        void SetIsSelectedAndClear(EisppPackage model);
    }
}
