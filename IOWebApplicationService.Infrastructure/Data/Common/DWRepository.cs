using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplicationService.Infrastructure.Data.DW;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace IOWebApplicationService.Infrastructure.Data.Common
{
    public class DWRepository : BaseRepository, IDWRepository
    {
        public DWRepository(DWDbContext dwcontext)
        {
            this.Context = dwcontext;
        }

        public int TrackerCount => Context.ChangeTracker.Entries().Count();

        public void RefreshDbContext(string connectionString)
        {
            if (Context != null)
            {
                Context.Dispose();
            }
            var optionsBuilder = new DbContextOptionsBuilder<DWDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            Context = new DWDbContext(optionsBuilder.Options);
        }
    }
}
