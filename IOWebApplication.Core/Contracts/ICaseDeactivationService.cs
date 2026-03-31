using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseDeactivationService : IBaseService
    {
        IQueryable<CaseDeactivationVM> Select(CaseDeactivationFilterVM filter);

        SaveResultVM Add(CaseDeactivation model);

        Task<bool> DeclareDeactivation(int id);
    }
}
