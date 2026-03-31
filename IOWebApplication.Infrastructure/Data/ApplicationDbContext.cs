using Microsoft.EntityFrameworkCore;

namespace IOWebApplication.Infrastructure.Data.Models
{
    public class ApplicationDbContext : EissDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }
    }
}
