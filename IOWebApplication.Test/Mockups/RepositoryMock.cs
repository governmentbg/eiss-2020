using IOWebApplication.Infrastructure.Data.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IOWebApplication.Test.Mockups
{
    public class RepositoryMock : IRepository
    {
        private List<object> dbsets = new List<object>();

        public int TrackerCount => 0;

        protected List<T> DbSet<T>() where T : class
        {
            object dbset = dbsets.FirstOrDefault(s => s.GetType() == typeof(List<T>));

            if (dbset == null)
            {
                dbset = new List<T>();
                dbsets.Add(dbset);
            }

            return (List<T>)dbset;
        }

        public void Add<T>(T entity) where T : class
        {
            DbSet<T>().Add(entity);
        }

        public void AddRange<T>(IEnumerable<T> entities) where T : class
        {
            DbSet<T>().AddRange(entities);
        }

        public IQueryable<T> All<T>() where T : class
        {
            return DbSet<T>().AsQueryable();
        }

        public IQueryable<T> All<T>(Expression<Func<T, bool>> search) where T : class
        {
            return DbSet<T>()
                .AsQueryable()
                .Where(search)
                .AsQueryable();
        }

        public IQueryable<T> AllReadonly<T>() where T : class
        {
            return DbSet<T>()
                .AsReadOnly()
                .AsQueryable();
        }

        public IQueryable<T> AllReadonly<T>(Expression<Func<T, bool>> search) where T : class
        {
            return DbSet<T>()
                .AsQueryable()
                .Where(search)
                .ToList()
                .AsReadOnly()
                .AsQueryable();
        }

        public void Delete<T>(object id) where T : class
        {
            var entity = GetById<T>(id);

            if (entity == null)
            {
                throw new ArgumentException("There is no Entity with this id in the collection");
            }

            Delete<T>(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            DbSet<T>().Remove(entity);
        }

        public void DeleteRange<T>(IEnumerable<T> entities) where T : class
        {
            foreach (var entity in entities)
            {
                Delete<T>(entity);
            }
        }

        public void DeleteRange<T>(Expression<Func<T, bool>> deleteWhereClause) where T : class
        {
            DeleteRange<T>(All<T>(deleteWhereClause));
        }

        public void Detach<T>(T entity) where T : class
        {
        }

        public void Dispose()
        {
            dbsets = null;
        }

        public IEnumerable<T> ExecuteProc<T>(string procedureName, params object[] args) where T : class
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ExecuteSQL<T>(string query, params object[] args) where T : class
        {
            throw new NotImplementedException();
        }

        public T GetById<T>(object id) where T : class
        {
            var properties = typeof(T).GetProperties();
            string keyProperty = null;
            T entity = null;

            foreach (var property in properties)
            {
                if (Attribute.IsDefined(property, typeof(KeyAttribute)))
                {
                    keyProperty = property.Name;
                    break;
                }
            }

            if (keyProperty == null)
            {
                string key = properties
                    .Where(p => p.Name?.ToUpper() == "ID")
                    .Select(p => p.Name)
                    .FirstOrDefault();

                if (key != null)
                {
                    keyProperty = key;
                }
                else
                {
                    throw new ArgumentException("No key property found");
                }
            }

            foreach (var item in DbSet<T>())
            {
                var pi = typeof(T).GetProperty(keyProperty);

                if (pi.GetValue(item).Equals(id))
                {
                    entity = item;

                    break;
                }
            }

            if (entity == null)
            {
                throw new ArgumentException($"No entity with this id = {id} found");
            }

            return entity;
        }

        public T GetByIds<T>(object[] id) where T : class
        {
            throw new NotImplementedException();
        }

        public int SaveChanges()
        {
            return 1;
        }

        public Task<int> SaveChangesAsync()
        {
            throw new NotImplementedException();
        }

        public void Update<T>(T entity) where T : class
        {
            var properties = typeof(T).GetProperties();
            string keyProperty = null;
            T oldEntity = null;

            foreach (var property in properties)
            {
                if (Attribute.IsDefined(property, typeof(KeyAttribute)))
                {
                    keyProperty = property.Name;
                    break;
                }
            }

            if (keyProperty == null)
            {
                string key = properties
                    .Where(p => p.Name?.ToUpper() == "ID")
                    .Select(p => p.Name)
                    .FirstOrDefault();

                if (key != null)
                {
                    keyProperty = key;
                }
                else
                {
                    throw new ArgumentException("No key property found");
                }
            }

            foreach (var item in DbSet<T>())
            {
                var pi = typeof(T).GetProperty(keyProperty);

                if (pi.GetValue(item).Equals(pi.GetValue(entity)))
                {
                    oldEntity = item;

                    break;
                }
            }

            if (oldEntity != null)
            {
                int index = DbSet<T>().IndexOf(oldEntity);
                DbSet<T>()[index] = entity;
            }
            else
            {
                throw new ArgumentException("No such entity in collection");
            }
        }

        public void UpdateRange<T>(IEnumerable<T> entities) where T : class
        {
            foreach (var entity in entities)
            {
                Update<T>(entity);
            }
        }

        public void RefreshDbContext(string connectionString)
        {
            throw new NotImplementedException();
        }

        public Tprop GetPropById<T, Tprop>(Expression<Func<T, bool>> where, Expression<Func<T, Tprop>> select)
            where T : class
        {
            throw new NotImplementedException();
        }

        public void PropUnmodified<T>(Expression<Func<T>> selectProp) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
