using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplicationService.Infrastructure.Contracts;
using IOWebApplicationService.Infrastructure.Data.Common;
using IOWebApplicationService.Infrastructure.Data.DW.Models;
using IOWebApplicationService.Infrastructure.Data.Models.Base;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class DWErrorLogService : IDWErrorLogService
  {
      
        private readonly IDWRepository dwRepo;
        
        public DWErrorLogService(IDWRepository _dwRepo)
        {
           
            this.dwRepo = _dwRepo;
    
        }

    public bool LogError(int court_id,string court_name,string table_name,Int64 table_id,string e_msg)
    {
      bool result = false;

      try
      {
        DWErrorLog err = new DWErrorLog();
        err.CourtId = court_id;
        err.CourtName = court_name;
        err.TableName = table_name;
        err.TableId = table_id;
        err.ErrorMessage = e_msg;
        err.ErrorDate = DateTime.Now;
        dwRepo.Add<DWErrorLog>(err);
        dwRepo.SaveChanges();






      }
      catch (Exception ex)
      {

      }

      return result;

    }
  }
}
