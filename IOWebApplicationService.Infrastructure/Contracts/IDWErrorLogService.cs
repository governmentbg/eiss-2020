using IOWebApplicationService.Infrastructure.Data.Models.Base;
using System;

namespace IOWebApplicationService.Infrastructure.Contracts
{
    public interface IDWErrorLogService
    {
    bool LogError(int court_id, string court_name, string table_name, Int64 table_id, string e_msg);
    }
}
