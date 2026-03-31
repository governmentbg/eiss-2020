using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplicationService.Infrastructure.Data.DW;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace IOWebApplicationService.Infrastructure.Data.Common
{
    public class DWRepository : BaseRepository, IDWRepository
    {
        public DWRepository(
            DWDbContext dwcontext,
            ILogger<DWRepository> _logger,
            IHttpContextAccessor _httpContextAccessor)
        {
            this.Context = dwcontext;
            this.logger = _logger;
            if (_httpContextAccessor.HttpContext != null)
                httpContext = _httpContextAccessor.HttpContext;
        }

        public int TrackerCount => Context.ChangeTracker.Entries().Count();

        public void PropUnmodified<T>(Expression<Func<T>> selectProp) where T : class
        {
            throw new NotImplementedException();
        }

        public void RefreshDbContext(string connectionString, IConfiguration config = null)
        {
            if (Context != null)
                Context.ChangeTracker.Clear();


            //if (Context != null)
            //{
            //    Context.Dispose();
            //}
            //var optionsBuilder = new DbContextOptionsBuilder<DWDbContext>();
            //optionsBuilder.UseSqlServer(connectionString, m =>
            //{
            //    if (config != null)
            //    {
            //        m.CommandTimeout(config.GetValue<int>("DW:Timeout", 60));
            //    }
            //});

            //Context = new DWDbContext(optionsBuilder.Options);
        }
    }
}
