using Microsoft.EntityFrameworkCore;

namespace IOWebApplication.Infrastructure.Data.Models
{
    public class ReadonlyDbContext : EissDbContext
    {
        public ReadonlyDbContext(DbContextOptions<ReadonlyDbContext> options)
           : base(options)
        {
        }
    }
}
