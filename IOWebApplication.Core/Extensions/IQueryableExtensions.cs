using IOWebApplication.Core.Models;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IOWebApplication.Core.Extensions
{
    public static class IQueryableExtensions
    {
        private static readonly TypeInfo QueryCompilerTypeInfo = typeof(QueryCompiler).GetTypeInfo();

        private static readonly FieldInfo QueryCompilerField = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields.First(x => x.Name == "_queryCompiler");

        private static readonly FieldInfo QueryModelGeneratorField = QueryCompilerTypeInfo.DeclaredFields.First(x => x.Name == "_queryModelGenerator");

        private static readonly FieldInfo DataBaseField = QueryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_database");

        private static readonly PropertyInfo DatabaseDependenciesField = typeof(Microsoft.EntityFrameworkCore.Storage.Database).GetTypeInfo().DeclaredProperties.Single(x => x.Name == "Dependencies");

        //public static string ToSql<TEntity>(this IQueryable<TEntity> query) where TEntity : class
        //{
        //    var queryCompiler = (QueryCompiler)QueryCompilerField.GetValue(query.Provider);
        //    var modelGenerator = (QueryModelGenerator)QueryModelGeneratorField.GetValue(queryCompiler);
        //    var queryModel = modelGenerator.ParseQuery(query.Expression);
        //    var database = (IDatabase)DataBaseField.GetValue(queryCompiler);
        //    var databaseDependencies = (DatabaseDependencies)DatabaseDependenciesField.GetValue(database);
        //    var queryCompilationContext = databaseDependencies.QueryCompilationContextFactory.Create(false);
        //    var modelVisitor = (RelationalQueryModelVisitor)queryCompilationContext.CreateQueryModelVisitor();
        //    modelVisitor.CreateQueryExecutor<TEntity>(queryModel);
        //    var sql = modelVisitor.Queries.First().ToString();

        //    return sql;
        //}
        /*
        // <summary>
        // Проверява дали в подадения IQueryable основния Select e от GroupBy statement
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        //public static bool HasGroupBy<TEntity>(this IQueryable<TEntity> query) where TEntity : class
        //{
        //    if (query.Provider.GetType().Name != "EntityQueryProvider")
        //    {
        //        return false;
        //    }
        //    var queryCompiler = (QueryCompiler)QueryCompilerField.GetValue(query.Provider);
        //    var modelGenerator = (QueryModelGenerator)QueryModelGeneratorField.GetValue(queryCompiler);
        //    var queryModel = modelGenerator.ParseQuery(query.Expression);
        //    var resType = queryModel.MainFromClause.ItemType.ToString();

        //    return resType.Contains("IGrouping", StringComparison.InvariantCultureIgnoreCase);
        //}
        */

        /// <summary>
        /// Сортиране в DataTables
        /// </summary>
        /// <typeparam name="T">Тип на данните</typeparam>
        /// <param name="source">Дърво, което трябва да бъде подредено</param>
        /// <param name="sortModels">Модел с данни за начина на сортиране</param>
        /// <returns></returns>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, List<DataTablesSortColumnVM> sortModels)
        {
            var expression = source.Expression;
            int count = 0;

            foreach (var item in sortModels)
            {
                var parameter = Expression.Parameter(source.ElementType, "x");
                var selector = Expression.PropertyOrField(parameter, item.Name);
                var method = item.IsAscending == false ?
                    (count == 0 ? "OrderByDescending" : "ThenByDescending") :
                    (count == 0 ? "OrderBy" : "ThenBy");
                expression = Expression.Call(typeof(Queryable), method,
                    new Type[] { source.ElementType, selector.Type },
                    expression, Expression.Quote(Expression.Lambda(selector, parameter)));
                count++;
            }

            return count > 0 ? source.Provider.CreateQuery<T>(expression) : source;
        }
    }
}
