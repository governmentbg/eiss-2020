using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Linq.Expressions;

namespace IOWebApplication.Core.Contracts
{
    public interface IBaseService
    {
        T GetById<T>(object id) where T : class;

        /// <summary>
        /// Размества подреждането в дадена таблица Т
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">ID на текущия елемент</param>
        /// <param name="moveUp">Посока на преместване. True - нагоре(по-малък индекс)</param>
        /// <param name="orderProp">Linq израз на полето за преместване</param>
        /// <param name="setterProp">Linq израз на полето за преместване</param>
        /// <param name="predicate">Where клауза за филтриране, ако се налага</param>
        /// <returns>true, ако всички записи преминат успешно</returns>
        bool ChangeOrder<T>(object id, bool moveUp, Func<T, int?> orderProp, Expression<Func<T, int?>> setterProp, Expression<Func<T, bool>> predicate = null) where T : class;

        bool SaveExpireInfo<T>(ExpiredInfoVM model) where T : class, IExpiredInfo;

        CurrentContextModel GetCurrentContext(int sourceType, long? sourceId, string operation = "", object parentId = null);

        SystemParam SystemParam_Select(string paramName);
    }
}
