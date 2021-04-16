using System.Collections.Generic;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface IHaveHistory<IHistory>
    {
        ICollection<IHistory> History { get; set; }
    }
}
