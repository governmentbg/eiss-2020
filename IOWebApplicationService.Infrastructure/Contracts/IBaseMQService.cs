using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Contracts
{
    public interface IBaseMQService
    {
        Task<bool> PushMQWithFetch(int fetchCount);
    }
}
