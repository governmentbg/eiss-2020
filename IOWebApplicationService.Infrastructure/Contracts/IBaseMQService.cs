using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Contracts
{
    public interface IBaseMQService
    {
        Task<int> PushMQWithFetch(int fetchCount);

        Task<bool> FetchResult();
        void RefreshDataContext(ref int savedCount, int maxCount, string connStr);
     }
}
