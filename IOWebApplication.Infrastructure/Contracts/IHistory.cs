using IOWebApplication.Infrastructure.Data.Models.Base;
using System;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface IHistory : IUserDateWRT
    {
        int Id { get; set; }

        int HistoryId { get; set; }

        DateTime? HistoryDateExpire { get; set; }
    }
}
