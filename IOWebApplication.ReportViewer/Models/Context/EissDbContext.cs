using IOWebApplication.ReportViewer.Data.Context;
using System.Data.Entity;

namespace IOWebApplication.ReportViewer.Models.Context
{
    public class EissDbContext : DbContext
    {
        public EissDbContext()
            : base("name=EissDbContext")
        {
        }
        public virtual DbSet<Report> Reports { get; set; }
        public virtual DbSet<ReportRequest> ReportRequests { get; set; }
    }
}