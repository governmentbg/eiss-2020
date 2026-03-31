// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Extensions
{
    public static  class LinqNet8Extensions
    {
        /// <summary>
        /// ВАЖНО: Метода НЕ МОЖЕ да се използва във subquery, а само като финализираща
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TSource FirstOrValue<TSource>(this IQueryable<TSource> source, TSource defaultValue)
        {
            TSource result = source.FirstOrDefault();
            if (result == null || result.Equals(default(TSource)))
            {
                return defaultValue;
            }

            return result;
        }

        public static async Task<TSource> FirstOrValueAsync<TSource>(this IQueryable<TSource> source, TSource defaultValue)
        {
            TSource result = await source.FirstOrDefaultAsync();
            if (result == null || result.Equals(default(TSource)))
            {
                return defaultValue;
            }

            return result;
        }
        public static string GetName<T, Tobj>(this Expression<Func<T, Tobj>> action)
        {
            return GetNameFromMemberExpression(action.Body);
        }
        static string GetNameFromMemberExpression(Expression expression)
        {
            if (expression is MemberExpression)
            {
                return (expression as MemberExpression).Member.Name;
            }
            else if (expression is UnaryExpression)
            {
                return GetNameFromMemberExpression((expression as UnaryExpression).Operand);
            }
            return "MemberNameUnknown";
        }
    }
}
