using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Data.Common
{
    /// <summary>
    /// Abstraction of repository access methods
    /// </summary>
    /// <typeparam name="T">Repository type / db table</typeparam>
    public interface IRepository : IDisposable
    {
        /// <summary>
        /// All records in a table
        /// </summary>
        /// <returns>Queryable expression tree</returns>
        IQueryable<T> All<T>() where T : class;

        /// <summary>
        /// All records in a table
        /// </summary>
        /// <returns>Queryable expression tree</returns>
        IQueryable<T> All<T>(Expression<Func<T, bool>> search) where T : class;

        /// <summary>
        /// The result collection won't be tracked by the context
        /// </summary>
        /// <returns>Expression tree</returns>
        IQueryable<T> AllReadonly<T>() where T : class;

        /// <summary>
        /// The result collection won't be tracked by the context
        /// </summary>
        /// <returns>Expression tree</returns>
        IQueryable<T> AllReadonly<T>(Expression<Func<T, bool>> search) where T : class;

        /// <summary>
        /// Gets specific record from database by primary key
        /// </summary>
        /// <param name="id">record identificator</param>
        /// <returns>Single record</returns>
        T GetById<T>(object id) where T : class;

        T GetByIds<T>(object[] id) where T : class;

        /// <summary>
        /// Adds entity to the database
        /// </summary>
        /// <param name="entity">Entity to add</param>
        void Add<T>(T entity) where T : class;

        /// <summary>
        /// Ads collection of entities to the database
        /// </summary>
        /// <param name="entities">Enumerable list of entities</param>
        void AddRange<T>(IEnumerable<T> entities) where T : class;

        /// <summary>
        /// Updates a record in database
        /// </summary>
        /// <param name="entity">Entity for record to be updated</param>
        void Update<T>(T entity) where T : class;

        /// <summary>
        /// Updates set of records in the database
        /// </summary>
        /// <param name="entities">Enumerable collection of entities to be updated</param>
        void UpdateRange<T>(IEnumerable<T> entities) where T : class;

        /// <summary>
        /// Deletes a record from database
        /// </summary>
        /// <param name="id">Identificator of record to be deleted</param>
        void Delete<T>(object id) where T : class;

        /// <summary>
        /// Deletes a record from database
        /// </summary>
        /// <param name="entity">Entity representing record to be deleted</param>
        void Delete<T>(T entity) where T : class;

        void DeleteRange<T>(IEnumerable<T> entities) where T : class;
        void DeleteRange<T>(Expression<Func<T, bool>> deleteWhereClause) where T : class;


        /// <summary>
        /// Detaches given entity from the context
        /// </summary>
        /// <param name="entity">Entity to be detached</param>
        void Detach<T>(T entity) where T : class;

        /// <summary>
        /// Saves all made changes in trasaction
        /// </summary>
        /// <returns>Error code</returns>
        int SaveChanges();
        Task<int> SaveChangesAsync();

        IEnumerable<T> ExecuteProc<T>(string procedureName, params object[] args) where T : class;
        IEnumerable<T> ExecuteSQL<T>(string query, params object[] args) where T : class;

        int TrackerCount { get; }
        void RefreshDbContext(string connectionString);
    }
}
