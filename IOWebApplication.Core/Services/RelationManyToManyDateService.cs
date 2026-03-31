using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class RelationManyToManyDateService : BaseService, IRelationManyToManyDateService
    {
        private readonly ICourtLoadPeriodService courtLoadPeriodService;
        public RelationManyToManyDateService(
             ILogger<RelationManyToManyDateService> _logger,
             IRepository _repo
             , ICourtLoadPeriodService _courtLoadPeriodService
            )
        {
            logger = _logger;
            repo = _repo;
            courtLoadPeriodService = _courtLoadPeriodService;


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
        public async Task<bool> SaveData<T>(int parentId, List<int> codes,
                                Expression<Func<T, int>> parentProp,
                                Expression<Func<T, bool>> savedDataFilter,
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
                var forSaveList = await repo.All<T>()
                    .Where(savedDataFilter)
                    //.Where(x => parentProp2(x) == parentId)
                    .ToListAsync();
                foreach (var item in forSaveList)
                {
                    if (!codes.Any(x => x == itemProp2(item)))
                        SetPropertyValue<T, DateTime?>(item, dateToProp, DateTime.Now.Date);
                    else
                    {
                        if (dateToProp2(item) != null)
                            SetPropertyValue<T, DateTime?>(item, dateToProp, null);
                    }
                }
                foreach (var code in codes)
                {
                    if (!forSaveList.Any(x => itemProp2(x) == code))
                    {
                        T newCode = new T();
                        setNew?.Invoke(newCode);
                        SetPropertyValue<T, int>(newCode, parentProp, parentId);
                        SetPropertyValue<T, int>(newCode, itemProp, code);
                        SetPropertyValue<T, DateTime?>(newCode, dateFromProp, DateTime.Now);
                        repo.Add<T>(newCode);
                    }
                }
                await repo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на шифри");
                return false;
            }
        }
        public async Task<bool> SaveDataPercent<T>(int parentId, List<MultiSelectTransferPercentVM> codes,
                                Expression<Func<T, bool>> courtIdWhere,
                                Expression<Func<T, int>> parentProp,
                                Expression<Func<T, bool>> savedDataFilter,
                                Expression<Func<T, int>> itemProp,
                                Expression<Func<T, DateTime?>> dateFromProp,
                                Expression<Func<T, DateTime?>> dateToProp,
                                Expression<Func<T, int>> percentProp,
                                Func<T, bool> setNew, bool IsLawunit)

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
                var forSaveList = await repo.All<T>()
                    //.Where(x => parentProp2(x) == parentId && dateToProp2(x) == null)
                    .Where(savedDataFilter)
                    .Where(courtIdWhere)
                        // .Where(setNew ?? (x => true))
                        .ToListAsync();
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

                        ///////////////////// Преизчислява среднодневните на база сменения процент
                        CourtLoadPeriodLawUnit courtLoadPeriodLawUnit = null;
                        if (IsLawunit)
                        {
                            courtLoadPeriodLawUnit = courtLoadPeriodService.UpdateChangedProcentAverageCases(parentId, code.Id, code.Percent);
                        }
                        else
                        {
                            courtLoadPeriodLawUnit = courtLoadPeriodService.UpdateChangedProcentAverageCases(code.Id, parentId, code.Percent);
                        }

                        if (courtLoadPeriodLawUnit != null)
                        {
                            repo.Add<CourtLoadPeriodLawUnit>(courtLoadPeriodLawUnit);
                        }

                        ///////////////////// Преизчислява среднодневните на база сменения процент

                    }
                }
                await repo.SaveChangesAsync();
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
