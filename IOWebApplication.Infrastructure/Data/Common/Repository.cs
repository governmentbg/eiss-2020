using IOWebApplication.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Data.Common
{
    /// <summary>
    /// Implementation of repository access methods
    /// for Relational Database Engine
    /// </summary>
    /// <typeparam name="T">Type of the data table to which 
    /// current reposity is attached</typeparam>
    public class Repository : BaseRepository, IRepository
    {
        public Repository(ApplicationDbContext context)
        {
            this.Context = context;
        }

        public int TrackerCount => Context.ChangeTracker.Entries().Count();

        public void RefreshDbContext(string connectionString)
        {
            if (Context != null)
            {
                Context.Dispose();
            }
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            Context = new ApplicationDbContext(optionsBuilder.Options);
        }

        
    }
}
