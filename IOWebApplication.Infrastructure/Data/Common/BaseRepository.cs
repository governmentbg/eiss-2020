using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
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
    public abstract class BaseRepository : IDisposable
    {
        /// <summary>
        /// Entity framework DB context holding connection information and properties
        /// and tracking entity states 
        /// </summary>
        protected DbContext Context { get; set; }

        protected ILogger logger { get; set; }

        protected HttpContext httpContext;

        private string _currentPath;
        protected string currentPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_currentPath))
                {
                    return _currentPath;
                }
                if (httpContext != null && httpContext.Request != null)
                {
                    _currentPath = httpContext.Request.Path;
                    return _currentPath;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Representation of table in database
        /// </summary>
        protected DbSet<T> DbSet<T>() where T : class
        {

            return this.Context.Set<T>();

        }

        public IEnumerable<T> ExecuteProc<T>(string procedureName, params object[] args) where T : class
        {
            return this.DbSet<T>().FromSqlRaw($"/*NO LOAD BALANCE*/ select * from  {procedureName}", args).ToList();

            //return this.Context.Query<T>().FromSql("/*NO LOAD BALANCE*/ select * from " + procedureName, args).ToList();
        }

        public IEnumerable<T> ExecuteSQL<T>(string query, params object[] args) where T : class
        {
            //throw new NotImplementedException();
            return this.DbSet<T>().FromSqlRaw($"/*NO LOAD BALANCE*/ {query}", args).ToList();
        }

        /// <summary>
        /// Adds entity to the database
        /// </summary>
        /// <param name="entity">Entity to add</param>
        public void Add<T>(T entity) where T : class
        {
            this.DbSet<T>().Add(entity);
        }

        public async Task AddAsync<T>(T entity) where T : class
        {
            await this.DbSet<T>().AddAsync(entity);
        }

        /// <summary>
        /// Ads collection of entities to the database
        /// </summary>
        /// <param name="entities">Enumerable list of entities</param>
        public void AddRange<T>(IEnumerable<T> entities) where T : class
        {
            this.DbSet<T>().AddRange(entities);
        }

        /// <summary>
        /// All records in a table
        /// </summary>
        /// <returns>Queryable expression tree</returns>
        public IQueryable<T> All<T>() where T : class
        {
            string tag = $"{AuditConstants.TagNet8_1} - {currentPath}: ";

            return this.DbSet<T>()
                .TagWith(tag)
                .AsQueryable();
        }

        public IQueryable<T> All<T>(Expression<Func<T, bool>> search) where T : class
        {
            string tag = $"{AuditConstants.TagNet8_1} - {currentPath}: ";

            return this.DbSet<T>()
                .Where(search)
                .TagWith(tag)
                .AsQueryable();
        }

        /// <summary>
        /// The result collection won't be tracked by the context
        /// </summary>
        /// <returns>Expression tree</returns>
        public IQueryable<T> AllReadonly<T>() where T : class
        {
            string tag = $"{AuditConstants.TagNet8_1} - {currentPath}: ";

            return this.DbSet<T>()
                .TagWith(tag)
                .AsQueryable()
                .AsNoTracking();
        }
        public IQueryable<T> AllReadonly<T>(Expression<Func<T, bool>> search) where T : class
        {
            string tag = $"{AuditConstants.TagNet8_1} - {currentPath}: ";

            return this.DbSet<T>()
                .Where(search)
                .TagWith(tag)
                .AsQueryable()
                .AsNoTracking();
        }

        /// <summary>
        /// Deletes a record from database
        /// </summary>
        /// <param name="id">Identificator of record to be deleted</param>
        public void Delete<T>(object id) where T : class
        {
            T entity = GetById<T>(id);

            Delete<T>(entity);
        }

        /// <summary>
        /// Deletes a record from database, based on expression
        /// </summary>
        /// <param name="deleteWhereClause">Expression to select entities to delete</param>
        public int ExecuteDelete<T>(Expression<Func<T, bool>> deleteWhereClause) where T : class
        {

            return this.DbSet<T>().Where(deleteWhereClause).ExecuteDelete();
        }

        /// <summary>
        /// Deletes a record from database, based on expression
        /// </summary>
        /// <param name="deleteWhereClause">Expression to select entities to delete</param>
        public async Task<int> ExecuteDeleteAsync<T>(Expression<Func<T, bool>> deleteWhereClause) where T : class
        {

            return await this.DbSet<T>().Where(deleteWhereClause).ExecuteDeleteAsync();
        }

        /// <summary>
        /// Deletes a record from database
        /// </summary>
        /// <param name="entity">Entity representing record to be deleted</param>
        public void Delete<T>(T entity) where T : class
        {
            EntityEntry entry = this.Context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                this.DbSet<T>().Attach(entity);
            }

            entry.State = EntityState.Deleted;
        }

        /// <summary>
        /// Detaches given entity from the context
        /// </summary>
        /// <param name="entity">Entity to be detached</param>
        public void Detach<T>(T entity) where T : class
        {
            EntityEntry entry = this.Context.Entry(entity);

            entry.State = EntityState.Detached;
        }

        /// <summary>
        /// Disposing the context when it is not neede
        /// Don't have to call this method explicitely
        /// Leave it to the IoC container
        /// </summary>
        public void Dispose()
        {
            this.Context.Dispose();
        }

        /// <summary>
        /// Gets specific record from database by primary key
        /// </summary>
        /// <param name="id">record identificator</param>
        /// <returns>Single record</returns>
        public async Task<T> GetByIdAsync<T>(object id) where T : class
        {
            return await this.DbSet<T>().FindAsync(id);
        }

        public T GetById<T>(object id) where T : class
        {
            return this.DbSet<T>().Find(id);
        }

        public T GetByIds<T>(object[] id) where T : class
        {
            return this.DbSet<T>().Find(id);
        }


        public Tprop GetPropById<T, Tprop>(Expression<Func<T, bool>> where, Expression<Func<T, Tprop>> select)
            where T : class
        {
            return this.DbSet<T>().AsNoTracking().Where(where).Select(select).FirstOrDefault();
        }

        public async Task<Tprop> GetPropByIdAsync<T, Tprop>(Expression<Func<T, bool>> where, Expression<Func<T, Tprop>> select)
           where T : class
        {
            return await this.DbSet<T>().AsNoTracking().Where(where).Select(select).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Saves all made changes in trasaction
        /// </summary>
        /// <returns>Error code</returns>
        public int SaveChanges()
        {
            //var ent = this.Context.ChangeTracker.Entries();
            //string info = "";
            //var hasActChanged = ent.Any(x => x.Metadata.Name.EndsWith(typeof(CaseSessionAct).Name)
            //    && x.Properties.Any(p => p.Metadata.Name == nameof(CaseSessionAct.RegNumber) && p.OriginalValue != null && p.CurrentValue == null));
            //if (hasActChanged)
            //{
            //    foreach (var e in ent)
            //    {
            //        if (typeof(IHistory).IsAssignableFrom(e.Metadata.ClrType))
            //        {
            //            continue;
            //        }
            //        info += $"{e.Metadata.Name} - {e.State}; id= {e.Properties.Where(p => p.Metadata.Name == "Id").Select(x => x.CurrentValue).FirstOrDefault()}" + System.Environment.NewLine;
            //        //За да не логва съдържанието на диспозитива
            //        foreach (var p in e.Properties.Where(px => px.Metadata.Name != nameof(CaseSessionAct.Description)))
            //        {
            //            info += $"  {p.Metadata.Name} ({p.OriginalValue})=>({p.CurrentValue})" + System.Environment.NewLine;
            //        }
            //    }
            //    if (!string.IsNullOrEmpty(info) && logger != null)
            //    {
            //        logger.LogCritical("ACT REGNUMBER RESET!!!" + System.Environment.NewLine + info);
            //        this.Context.Dispose();
            //        throw new Exception("ACT REGNUMBER RESET");
            //    }
            //}
            return this.Context.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await this.Context.SaveChangesAsync();
        }

        public void Attach<T>(T entity) where T : class
        {
            this.DbSet<T>().Attach(entity);
        }

        /// <summary>
        /// Updates a record in database
        /// </summary>
        /// <param name="entity">Entity for record to be updated</param>
        public void Update<T>(T entity) where T : class
        {
            this.DbSet<T>().Update(entity);
        }

        /// <summary>
        /// Updates set of records in the database
        /// </summary>
        /// <param name="entities">Enumerable collection of entities to be updated</param>
        public void UpdateRange<T>(IEnumerable<T> entities) where T : class
        {
            this.DbSet<T>().UpdateRange(entities);
        }

        public void DeleteRange<T>(IEnumerable<T> entities) where T : class
        {
            this.DbSet<T>().RemoveRange(entities);
        }

        public void DeleteRange<T>(Expression<Func<T, bool>> deleteWhereClause) where T : class
        {
            var entities = All<T>(deleteWhereClause);
            DeleteRange(entities);
        }

        public IDbContextTransaction BeginTransaction(bool fakeTransaction = false)
        {
            if (fakeTransaction)
            {
                return new MockTransaction();
            }
            return Context.Database.BeginTransaction();
        }

        public bool StopTrackingApplicationUser()
        {
            bool result = false;
            var ent = this.Context.ChangeTracker.Entries();
            foreach (var item in ent.Where(x => x.Metadata.Name.EndsWith(typeof(ApplicationUser).Name)))
            {
                item.State = EntityState.Detached;
                result = true;
            }

            return result;
        }
        public void ClearEntityTracker()
        {
            Context.ChangeTracker.Clear();
        }
    }
}
