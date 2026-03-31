using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IBaseService
    {
        T GetById<T>(object id) where T : class;

        /// <summary>
        /// Gets single property value  of type Tprop via where expression passed
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <typeparam name="TProp">Type of property</typeparam>
        /// <param name="where">Where clause expression</param>
        /// <param name="select">Select property expression</param>
        /// <returns></returns>
        TProp GetPropById<T, TProp>(Expression<Func<T, bool>> where, Expression<Func<T, TProp>> select)
            where T : class;

        string GetNomLabelById<T>(int id) where T : class, ICommonNomenclature;
        string GetNomCodeById<T>(int id) where T : class, ICommonNomenclature;

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
        string SystemParam_SelectValue(string paramName);

        /// <summary>
        /// Връща стойност
        /// </summary>
        /// <param name="paramName">Име на параметър</param>
        /// <returns></returns>
        Task<string> SystemParamGetValue(string paramName);

        int[] SystemParam_SelectIntValues(string paramName);
        Task<string> GetCaseInfoById(int id);
        string[] SystemParam_SelectStringValues(string paramName);
        T ReadById<T>(int id) where T : class, IHaveId;
        DateTime? GetFirstHistoryDate<T>(int id) where T : class, IHistory;
        Task<bool> CheckCaseFeature(int caseId, string featureName);
        Task<bool> CheckCaseFeature(CaseFeatureInfoVM caseInfo, string featureName);
        string GetUserIdByLawUnitId(int lawUnitId);

        /// <summary>
        /// Извличане на идентификатор на потребител
        /// </summary>
        /// <param name="lawUnitId">Идентификатор на лице</param>
        /// <returns></returns>
        Task<string> GetUserIdByLawUnitIdAsync(int lawUnitId);

        Task<T> GetByIdAsync<T>(object id) where T : class;
        Task<TProp> GetPropByIdAsync<T, TProp>(Expression<Func<T, bool>> where, Expression<Func<T, TProp>> select) where T : class;
        Task<T> ReadByIdAsync<T>(int id) where T : class, IHaveId;
        Task<T> ReadByIdAsync<T>(long id) where T : class, IHaveLongId;
        Task<CurrentContextModel> GetCurrentContextAsync(int sourceType, long? sourceId, string operation = "", object parentId = null);
        Task<T> GetReadonlyAsync<T>(long id) where T : class, IHaveLongId;
        Task<T> GetReadonlyAsync<T>(int id) where T : class, IHaveId;
        /// <summary>
        /// ИЗПОЛЗВАЙ ИЗКЛЮЧИТЕЛНО ВНИМАТЕЛНО!
        /// Подменя UserId при запис на обект при поискване на userContext.UserId
        /// </summary>
        /// <param name="impersonatedUserId"></param>
        void SetImpersonatedUser(string impersonatedUserId);
        IDbContextTransaction BeginTransaction();
        void ClearEntityTracker();
        bool StopTrackingApplicationUser();
        T GetReadonly<T>(long id) where T : class, IHaveLongId;
        T GetReadonly<T>(int id) where T : class, IHaveId;
        TProp GetPropById<T, TProp>(int id, Expression<Func<T, TProp>> select) where T : class, IHaveId;
        Task<TProp> GetPropByIdAsync<T, TProp>(int id, Expression<Func<T, TProp>> select) where T : class, IHaveId;
        Task<TProp> GetPropByIdAsync<T, TProp>(long id, Expression<Func<T, TProp>> select) where T : class, IHaveLongId;
        TProp GetPropById<T, TProp>(long id, Expression<Func<T, TProp>> select) where T : class, IHaveLongId;
        Task<string> GetIntegrationKey(int integrationType, int sourceType, long sourceId);
        string PersonNamesBase_GeneratePersonGid(string savedGid = null);
        Task<DateTime?> GetParamValueDate(string paramName, string defaultValue);
        Task<bool> IsNewExecProcessCase(int? caseId);
        void SetImpersonatedCourt(int impersonatedCourtId);
    }
}
