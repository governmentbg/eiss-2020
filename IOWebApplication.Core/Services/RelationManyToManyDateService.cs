// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class RelationManyToManyDateService : BaseService, IRelationManyToManyDateService
    {
        public RelationManyToManyDateService(
         ILogger<RelationManyToManyDateService> _logger,
         IRepository _repo)
        {
            logger = _logger;
            repo = _repo;
        }
        private void SetPropertyValue<T, Tobj>(T target, Expression<Func<T, Tobj>> memberLambda, Tobj value)
        {
            var memberSelectorExpression = memberLambda.Body as MemberExpression;

            if (memberSelectorExpression == null)
            {
                var expressionBody = memberLambda.Body;
                if (expressionBody is UnaryExpression expression && expression.NodeType == ExpressionType.Convert)
                {
                    expressionBody = expression.Operand;
                }
                memberSelectorExpression = (MemberExpression)expressionBody;
            }

            if (memberSelectorExpression != null)
            {
                var property = memberSelectorExpression.Member as PropertyInfo;
                if (property != null)
                {
                    property.SetValue(target, value, null);
                }
            }
        }
        public bool SaveData<T>(int parentId, List<int> codes,
                                Expression<Func<T, int>> parentProp,
                                Expression<Func<T, int>> itemProp,
                                Expression<Func<T, DateTime?>> dateFromProp,
                                Expression<Func<T, DateTime?>> dateToProp,
                                Func<T, bool> setNew)

            where T : class, new()
        {
            var parentProp2 = parentProp.Compile();
            var itemProp2 = itemProp.Compile();
            var dateFromPro2 = dateFromProp.Compile();
            var dateToProp2 = dateToProp.Compile();

            try
            {
                var forSaveList = repo.All<T>()
                    .Where(x => parentProp2(x) == parentId)
                    .ToList();
                foreach (var item in forSaveList)
                {
                    if (codes.Count(x => x == itemProp2(item)) == 0)
                        SetPropertyValue<T, DateTime?>(item, dateToProp, DateTime.Now.Date);
                    else
                    {
                        if (dateToProp2(item) != null)
                            SetPropertyValue<T, DateTime?>(item, dateToProp, null);
                    }
                }
                foreach (var code in codes)
                {
                    if (forSaveList.Count(x => itemProp2(x) == code) == 0)
                    {
                        T newCode = new T();
                        setNew?.Invoke(newCode);
                        SetPropertyValue<T, int>(newCode, parentProp, parentId);
                        SetPropertyValue<T, int>(newCode, itemProp, code);
                        SetPropertyValue<T, DateTime?>(newCode, dateFromProp, DateTime.Now);
                        repo.Add<T>(newCode);
                    }
                }
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на шифри");
                return false;
            }
        }
        public bool SaveDataPercent<T>(int parentId, List<MultiSelectTransferPercentVM> codes,
                                Expression<Func<T, bool>> courtIdWhere,
                                Expression<Func<T, int>> parentProp,
                                Expression<Func<T, int>> itemProp,
                                Expression<Func<T, DateTime?>> dateFromProp,
                                Expression<Func<T, DateTime?>> dateToProp,
                                Expression<Func<T, int>> percentProp,
                                Func<T, bool> setNew)

            where T : class, new()
        {
            if (
                (parentProp == null) ||
                (itemProp == null) ||
                (dateFromProp == null) ||
                (dateToProp == null)
               )
                return false;
            var parentProp2 = parentProp.Compile();
            var itemProp2 = itemProp.Compile();
            var dateFromPro2 = dateFromProp.Compile();
            var dateToProp2 = dateToProp.Compile();
            var percentProp2 = percentProp.Compile();

            try
            {
                var forSaveList = repo.All<T>()
                    .Where(x => parentProp2(x) == parentId && dateToProp2(x) == null)
                    .Where(courtIdWhere)
                    // .Where(setNew ?? (x => true))
                    .ToList();
                foreach (var item in forSaveList)
                {
                    if (!codes.Any(x => x.IsDelete != true && x.Id == itemProp2(item) && x.Percent == percentProp2(item)))
                        SetPropertyValue<T, DateTime?>(item, dateToProp, DateTime.Now);
                }
                foreach (var code in codes.Where(x => x.IsDelete != true))
                {
                    if (!forSaveList.Any(x => itemProp2(x) == code.Id && dateToProp2(x) == null && code.Percent == percentProp2(x)))
                    {
                        T newCode = new T();
                        setNew?.Invoke(newCode);
                        SetPropertyValue<T, int>(newCode, percentProp, code.Percent);
                        SetPropertyValue<T, int>(newCode, parentProp, parentId);
                        SetPropertyValue<T, int>(newCode, itemProp, code.Id);
                        SetPropertyValue<T, DateTime?>(newCode, dateFromProp, DateTime.Now);
                        repo.Add<T>(newCode);
                    }
                }
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return false;
            }
        }
    }
}
