using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using System.Linq;

namespace IOWebApplication.Core.Contracts
{
    public interface IDeactivateItemService : IBaseService
    {
        IQueryable<DeactivateItemVM> Select(DeactivateItemFilterVM filter);
    }
}
