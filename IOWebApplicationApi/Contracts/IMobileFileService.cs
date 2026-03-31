using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplicationApi.Contracts
{
    public interface IMobileFileService
    {
        Task<bool> SaveMobileFile(string deliveryAccountId, int courtId, string Content);
    }
}
