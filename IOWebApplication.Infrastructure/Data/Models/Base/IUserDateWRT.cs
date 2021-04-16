using System;
using IOWebApplication.Infrastructure.Data.Models.Identity;

namespace IOWebApplication.Infrastructure.Data.Models.Base
{
    public interface IUserDateWRT
    {
        string UserId { get; set; }
        DateTime DateWrt { get; set; }
        DateTime? DateTransferedDW { get; set; }
    }
}