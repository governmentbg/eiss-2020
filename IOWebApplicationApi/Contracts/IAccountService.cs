using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplicationApi.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplicationApi.Contracts
{
    public interface IAccountService
    {
        Task<string> GenerateJwtToken(DeliveryAccount account);
        Task<DeliveryAccountVM> Register(string registerGuid);
        bool SavePin(string registerGuid, string pin);
        Task<string> LoginPin(string registerGuid, string pin);
    }
}
