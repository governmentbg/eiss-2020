using IOWebApplication.Infrastructure.Data.Models.Base;
using System;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface IHistory : IUserDateWRT, IHaveId
    {

        int HistoryId { get; set; }

        DateTime? HistoryDateExpire { get; set; }

        void ClearForeignKeys();
    }
}
