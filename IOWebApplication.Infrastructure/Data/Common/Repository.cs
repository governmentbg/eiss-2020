using IOWebApplication.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace IOWebApplication.Infrastructure.Data.Common
{
    /// <summary>
    /// Implementation of repository access methods
    /// for Relational Database Engine
    /// </summary>
    public class Repository : BaseRepository, IRepository
    {
        //public Repository(
        //    IDbContextFactory<ApplicationDbContext> contextFactory,
        //    //ApplicationDbContext context,
        //    ILogger<Repository> _logger,
        //    IHttpContextAccessor _httpContextAccessor)
        //{
        //    this.Context = contextFactory.CreateDbContext();
        //    this.logger = _logger;
        //    if (_httpContextAccessor.HttpContext != null)
        //        httpContext = _httpContextAccessor.HttpContext;
        //}
        public Repository(
            ApplicationDbContext context,
            ILogger<Repository> _logger,
            IHttpContextAccessor _httpContextAccessor)
        {
            this.Context = context;
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

            //net 2.2 obsolete
            //if (Context != null)
            //{
            //    Context.Dispose();

            //}
            //var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            //optionsBuilder.UseNpgsql(connectionString);
            //Context = new ApplicationDbContext(optionsBuilder.Options);
        }


    }
}
