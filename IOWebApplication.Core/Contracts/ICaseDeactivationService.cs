using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System.Linq;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseDeactivationService : IBaseService
    {
        IQueryable<CaseDeactivationVM> Select(CaseDeactivationFilterVM filter);

        SaveResultVM Add(CaseDeactivation model);

        bool DeclareDeactivation(int id);
    }
}
