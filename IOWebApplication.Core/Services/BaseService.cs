using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.EISPP;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using IOWebApplication.Infrastructure.Models.ViewModels.Money;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class BaseService : IBaseService
    {
        protected ILogger logger { get; set; }
        protected IRepository repo { get; set; }
        protected IReadonlyRepository readonlyrepo { get; set; }
        protected IUserContext userContext { get; set; }
        protected string applicationDbConnectionString { get; set; }

        private DbEuroConfigVM _dbEuroConfig { get; set; }

        protected DateTime dtTomorrow { get { return DateTime.Now.AddDays(1); } }

        /// <summary>
        /// Настройка за евро зона с директно четене от базата
        /// </summary>
        protected DbEuroConfigVM DbEuroConfig
        {
            get
            {
                if (_dbEuroConfig != null)
                {
                    return _dbEuroConfig;
                }

                string[] euroDataParamNames = new string[] { NomenclatureConstants.SystemParamName.InterimPeriodEuroStart, NomenclatureConstants.SystemParamName.InterimPeriodEuroEnd,
                                                         NomenclatureConstants.SystemParamName.EuroExchangeRate };
                var euroParams = repo.AllReadonly<SystemParam>()
                                           .Where(x => euroDataParamNames.Contains(x.ParamName))
                                           .Select(x => new
                                           {
                                               x.ParamName,
                                               x.ParamValue
                                           })
                                           .ToList();
                DbEuroConfigVM result = new DbEuroConfigVM();
                decimal euroRate = NomenclatureExtensions.ParseDecimal(euroParams.Where(x => x.ParamName == NomenclatureConstants.SystemParamName.EuroExchangeRate).Select(x => x.ParamValue).DefaultIfEmpty("").FirstOrDefault());
                result.EuroExchangeRate = euroRate;
                DateTime date = DateTime.Now.AddYears(1);

                try
                {
                    var dateStr = euroParams.Where(x => x.ParamName == NomenclatureConstants.SystemParamName.InterimPeriodEuroStart).Select(x => x.ParamValue).DefaultIfEmpty("").FirstOrDefault();

                    if (string.IsNullOrEmpty(dateStr) == false)
                    {
                        if (DateTime.TryParseExact(dateStr, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                        {
                            result.InterimPeriodEuroStart = date;
                        }
                    }
                }
                catch (Exception)
                {
                    result.InterimPeriodEuroStart = DateTime.MaxValue;
                }
                try
                {
                    var dateStr = euroParams.Where(x => x.ParamName == NomenclatureConstants.SystemParamName.InterimPeriodEuroEnd).Select(x => x.ParamValue).DefaultIfEmpty("").FirstOrDefault();

                    if (string.IsNullOrEmpty(dateStr) == false)
                    {
                        if (DateTime.TryParseExact(dateStr, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                        {
                            result.InterimPeriodEuroEnd = date;
                        }
                    }
                }
                catch (Exception)
                {
                    result.InterimPeriodEuroEnd = DateTime.MaxValue;
                }

                _dbEuroConfig = result;
                return result;
            }
        }

        /// <summary>
        /// UserId на потребител при корекция на данни
        /// </summary>
        protected string ImpersonatedUserId { get; set; } = null;

        public void SetImpersonatedUser(string impersonatedUserId)
        {
            this.ImpersonatedUserId = impersonatedUserId;
        }

        /// <summary>
        /// UserId на потребител при корекция на данни
        /// </summary>
        protected int? ImpersonatedCourtId { get; set; } = null;

        public void SetImpersonatedCourt(int impersonatedCourtId)
        {
            this.ImpersonatedCourtId = impersonatedCourtId;
        }

        public bool ChangeOrder<T>(object id, bool moveUp, Func<T, int?> orderProp, Expression<Func<T, int?>> setterProp, Expression<Func<T, bool>> predicate = null) where T : class
        {
            var DbSet = repo.All<T>();
            var current = repo.GetById<T>(id);

            try
            {
                T next;
                IQueryable<T> items = DbSet;
                if (predicate != null)
                {
                    items = DbSet.Where(predicate);
                }
                if (moveUp)
                {
                    next = items.AsEnumerable().Where(x => orderProp(x) < orderProp(current)).OrderByDescending(x => orderProp(x)).FirstOrDefault();
                }
                else
                {
                    next = items.AsEnumerable().Where(x => orderProp(x) > orderProp(current)).OrderBy(x => orderProp(x)).FirstOrDefault();
                }
                if (next != null)
                {
                    int? middleValue = orderProp(current);
                    current.SetPropertyValue<T, int?>(setterProp, orderProp(next));
                    next.SetPropertyValue<T, int?>(setterProp, middleValue.Value);
                    repo.SaveChanges();

                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<T> GetByIdAsync<T>(object id) where T : class
        {
            return await repo.GetByIdAsync<T>(id);
        }
        public T GetById<T>(object id) where T : class
        {
            return repo.GetById<T>(id);
        }

        public T ReadById<T>(int id) where T : class, IHaveId
        {
            return repo.All<T>().Where(x => x.Id == id).FirstOrDefault();
        }
        public T ReadById<T>(long id) where T : class, IHaveLongId
        {
            return repo.All<T>().Where(x => x.Id == id).FirstOrDefault();
        }

        public Task<T> ReadByIdAsync<T>(int id) where T : class, IHaveId
        {
            return repo.All<T>().Where(x => x.Id == id).FirstOrDefaultAsync();
        }
        public Task<T> ReadByIdAsync<T>(long id) where T : class, IHaveLongId
        {
            return repo.All<T>().Where(x => x.Id == id).FirstOrDefaultAsync();
        }
        public Task<T> GetReadonlyAsync<T>(long id) where T : class, IHaveLongId
        {
            return repo.AllReadonly<T>().Where(x => x.Id == id).FirstOrDefaultAsync();
        }
        public Task<T> GetReadonlyAsync<T>(int id) where T : class, IHaveId
        {
            return repo.AllReadonly<T>().Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        public T GetReadonly<T>(long id) where T : class, IHaveLongId
        {
            return repo.AllReadonly<T>().Where(x => x.Id == id).FirstOrDefault();
        }
        public T GetReadonly<T>(int id) where T : class, IHaveId
        {
            return repo.AllReadonly<T>().Where(x => x.Id == id).FirstOrDefault();
        }
        public TProp GetPropById<T, TProp>(long id, Expression<Func<T, TProp>> select)
           where T : class, IHaveLongId
        {
            return repo.GetPropById<T, TProp>(x => x.Id == id, select);
        }
        public TProp GetPropById<T, TProp>(int id, Expression<Func<T, TProp>> select)
           where T : class, IHaveId
        {
            return repo.GetPropById<T, TProp>(x => x.Id == id, select);
        }
        public TProp GetPropById<T, TProp>(Expression<Func<T, bool>> where, Expression<Func<T, TProp>> select)
            where T : class
        {
            return repo.GetPropById<T, TProp>(where, select);
        }

        public Task<TProp> GetPropByIdAsync<T, TProp>(int id, Expression<Func<T, TProp>> select)
          where T : class, IHaveId
        {
            return repo.GetPropByIdAsync<T, TProp>(x => x.Id == id, select);
        }
        public Task<TProp> GetPropByIdAsync<T, TProp>(long id, Expression<Func<T, TProp>> select)
          where T : class, IHaveLongId
        {
            return repo.GetPropByIdAsync<T, TProp>(x => x.Id == id, select);
        }
        public Task<TProp> GetPropByIdAsync<T, TProp>(Expression<Func<T, bool>> where, Expression<Func<T, TProp>> select)
           where T : class
        {
            return repo.GetPropByIdAsync<T, TProp>(where, select);
        }

        public string PersonNamesBase_GeneratePersonGid(string savedGid = null)
        {
            return savedGid ?? Guid.NewGuid().ToString().ToLower();
        }

        protected void PersonNamesBase_SaveData(PersonNamesBase model, bool isUpdate)
        {
            switch (model.UicTypeId)
            {
                case NomenclatureConstants.UicTypes.EGN:
                case NomenclatureConstants.UicTypes.LNCh:
                case NomenclatureConstants.UicTypes.BirthDate:
                    model.FullName = model.MakeFullName();
                    break;
            }
            if (string.IsNullOrEmpty(model.Uic) || (model.UicTypeId == NomenclatureConstants.UicTypes.BirthDate))
            {
                //if (model.PersonId > 0)
                //{
                //    //Ако е избрано лице се премахва foreignkey-а за да се редактира първоначалния запис

                //    //Ако има намерен резултат за лице
                //    //var savedPerson = repo.GetById<Person>(model.PersonId);
                //    //savedPerson.CopyFrom(model);
                //    //Person_SaveData(savedPerson);

                //}

                model.Person = null;
                model.PersonId = null;
            }
            else
            {
                //търсене по UIC
                var savedPerson = repo.AllReadonly<Person>(x => x.Uic == model.Uic && x.UicTypeId == model.UicTypeId).FirstOrDefault();
                if (savedPerson != null)
                {
                    model.PersonId = savedPerson.Id;
                    savedPerson.CopyFrom(model);
                    if (isUpdate)
                        if (model.Person == null || model.Person?.Id == 0)
                        {
                            model.Person = savedPerson;
                        }
                    //repo.Update<Person>(savedPerson);
                    //Person_SaveData(savedPerson);
                }
                else
                {
                    model.PersonId = null;
                    model.Person = new Person();
                    model.Person.ActualtoDate = DateTime.Now;
                    model.Person.CopyFrom(model);
                }
            }
        }

        /// <summary>
        /// Създава запис за историята на обекта, връща последното състояние от историята, което е деактивирано с текущия запис
        /// </summary>
        /// <typeparam name="TActive"></typeparam>
        /// <typeparam name="THistory"></typeparam>
        /// <param name="source"></param>
        /// <param name="historyType"></param>
        /// <returns></returns>
        protected THistory CreateHistory<TActive, THistory>(TActive source, string historyType = null)
            where THistory : class, IHistory
            where TActive : class, IHaveHistory<THistory>
        {
            THistory historyRecord = source.Adapt<THistory>();

            THistory lastHistory = ExpireHistory<THistory>(historyRecord);
            source.History = source.History ?? new HashSet<THistory>();
            source.History.Add(historyRecord);

            if (typeof(IHaveHistoryType).IsAssignableFrom(typeof(THistory)))
            {
                ((IHaveHistoryType)historyRecord).HistoryType = historyType;
            }

            foreach (var history in source.History)
            {
                history.ClearForeignKeys();
            }

            return lastHistory;
        }

        /// <summary>
        /// Създава запис за историята на обекта, връща последното състояние от историята, което е деактивирано с текущия запис
        /// </summary>
        /// <typeparam name="TActive"></typeparam>
        /// <typeparam name="THistory"></typeparam>
        /// <param name="source"></param>
        /// <param name="historyType"></param>
        /// <returns></returns>
        protected async Task<THistory> CreateHistoryAsync<TActive, THistory>(TActive source, string historyType = null)
            where THistory : class, IHistory
            where TActive : class, IHaveHistory<THistory>
        {
            TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
            TypeAdapterConfig.GlobalSettings.Default.ShallowCopyForSameType(true);
            TypeAdapterConfig.GlobalSettings.Default.MaxDepth(1);


            THistory historyRecord = source.Adapt<THistory>();

            THistory lastHistory = await ExpireHistoryAsync<THistory>(historyRecord);
            source.History = source.History ?? new HashSet<THistory>();
            source.History.Add(historyRecord);

            if (typeof(IHaveHistoryType).IsAssignableFrom(typeof(THistory)))
            {
                ((IHaveHistoryType)historyRecord).HistoryType = historyType;
            }

            foreach (var history in source.History)
            {
                history.ClearForeignKeys();
            }

            return lastHistory;
        }

        private T ExpireHistory<T>(T lastVersion) where T : class, IHistory
        {
            if (lastVersion.Id == 0)
            {
                return null;
            }
            var priorHistories = repo.All<T>().Where(x => x.Id == lastVersion.Id && x.HistoryDateExpire == null).OrderBy(x => x.HistoryId).ToList();
            if (priorHistories != null)
            {
                foreach (var item in priorHistories)
                {
                    item.HistoryDateExpire = lastVersion.DateWrt.AddMilliseconds(-1);
                }
                return priorHistories.FirstOrDefault();
            }
            return null;
        }
        private async Task<T> ExpireHistoryAsync<T>(T lastVersion) where T : class, IHistory
        {
            if (lastVersion.Id == 0)
            {
                return null;
            }
            var priorHistories = await repo.All<T>().Where(x => x.Id == lastVersion.Id && x.HistoryDateExpire == null).OrderBy(x => x.HistoryId).ToListAsync();
            if (priorHistories != null)
            {
                foreach (var item in priorHistories)
                {
                    item.HistoryDateExpire = lastVersion.DateWrt.AddMilliseconds(-1);
                }
                return priorHistories.FirstOrDefault();
            }
            return null;
        }

        public void ClearEntityTracker()
        {
            repo.ClearEntityTracker();
        }

        public bool StopTrackingApplicationUser()
        {
            return repo.StopTrackingApplicationUser();
        }

        public DateTime? GetFirstHistoryDate<T>(int id) where T : class, IHistory
        {
            return repo.AllReadonly<T>().Where(x => x.Id == id).OrderBy(x => x.HistoryId).Select(x => x.DateWrt).FirstOrDefault();
        }

        public string GetUserIdByLawUnitId(int lawUnitId)
        {
            return repo.AllReadonly<ApplicationUser>().Where(x => x.LawUnitId == lawUnitId && x.IsActive).Select(x => x.Id).FirstOrDefault();
        }

        /// <summary>
        /// Извличане на идентификатор на потребител
        /// </summary>
        /// <param name="lawUnitId">Идентификатор на лице</param>
        /// <returns></returns>
        public async Task<string> GetUserIdByLawUnitIdAsync(int lawUnitId)
        {
            return await repo.AllReadonly<ApplicationUser>()
                             .Where(x => x.LawUnitId == lawUnitId &&
                                         x.IsActive)
                             .Select(x => x.Id)
                             .FirstOrDefaultAsync();
        }

        #region Проверка за право на достъп до обекти

        public bool SaveExpireInfo<T>(ExpiredInfoVM model) where T : class, IExpiredInfo
        {
            T saved;
            if (model.LongId > 0)
            {
                saved = repo.GetById<T>(model.LongId);
            }
            else
            {
                saved = repo.GetById<T>(model.Id);
            }

            if (saved != null)
            {
                saved.DateExpired = DateTime.Now;
                saved.UserExpiredId = userContext.UserId;
                saved.DescriptionExpired = model.DescriptionExpired;
                //repo.Update(saved);
                repo.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        public Expression<Func<T, bool>> FilterExpireInfo<T>(bool showExpired) where T : class, IExpiredInfo
        {
            Expression<Func<T, bool>> result = x => x.DateExpired == null;
            if (showExpired)
            {
                result = x => true;
            }
            return result;
        }

        #endregion

        protected IEnumerable<CourtLawUnit> GetActual_CourtLawUnitsByDate(int courtId, int lawUnitTypeId, DateTime? date)
        {
            DateTime dateActualTo = (date ?? DateTime.Now);

            var dateActualToEndDate = dateActualTo.AddDays(1);
            var result = repo.AllReadonly<CourtLawUnit>()
                                 .Include(x => x.LawUnit)
                                 .Where(x => x.CourtId == courtId && x.DateExpired == null)
                                 .Where(x => x.LawUnit.LawUnitTypeId == lawUnitTypeId)
                                 .Where(x => x.DateFrom <= dateActualTo && (x.DateTo ?? dateActualToEndDate) >= dateActualTo)
                                 .Where(x => NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(x.PeriodTypeId))
                                 .Distinct().ToList();
            return result;

        }

        protected void SetUserDateWRT(IUserDateWRT model)
        {
            model.UserId = userContext.UserId;
            model.DateWrt = DateTime.Now;
        }

        public CurrentContextModel GetCurrentContext(int sourceType, long? sourceId, string operation = "", object parentId = null)
        {
            return GetCurrentContextAsync(sourceType, sourceId, operation, parentId).Result;

        }
        public async Task<CurrentContextModel> GetCurrentContextAsync(int sourceType, long? sourceId, string operation = "", object parentId = null)
        {
            CurrentContextModel model = new CurrentContextModel(sourceType, sourceId, operation);

            model.Info.CourtId = userContext.CourtId;
            model.Info.UserId = userContext.UserId;
            //return model;
            //TODO

            switch (sourceType)
            {
                case SourceTypeSelectVM.Document:
                    {
                        await setAccessRightsForDocument(model, sourceId);
                    }
                    break;
                case SourceTypeSelectVM.DocumentResolution:
                    {
                        if (sourceId > 0)
                        {
                            var info = await repo.AllReadonly<DocumentResolution>()
                                                 .Where(x => x.Id == sourceId.Value)
                                                 .Select(x => new
                                                 {
                                                     x.DocumentId,
                                                     x.DeclaredDate,
                                                     x.DateExpired,
                                                     x.CourtId
                                                 })
                                                 .FirstOrDefaultAsync();

                            await setAccessRightsForDocument(model, info.DocumentId);
                            model.CanChangeFull = info.DateExpired == null && info.DeclaredDate == null
                                && info.CourtId == userContext.CourtId
                                && userContext.IsUserInRole(AccountConstants.Roles.Supervisor);
                        }
                        else
                        {
                            if (parentId != null)
                            {
                                await setAccessRightsForDocument(model, (long)parentId);
                            }
                            else
                            {
                                await setAccessRightsForDocument(model, null);
                            }
                        }
                    }
                    break;
                case SourceTypeSelectVM.DocumentNotification:
                    {
                        if (sourceId > 0)
                        {
                            var info = await repo.AllReadonly<DocumentNotification>()
                                                 .Where(x => x.Id == sourceId.Value)
                                                 .Select(x => new
                                                 {
                                                     notificationId = x.Id,
                                                     documentResolutionId = x.DocumentResolutionId,
                                                     documentId = x.DocumentResolution.DocumentId
                                                 })
                                                 .FirstOrDefaultAsync();

                            if (info != null)
                                await setAccessRightsForDocument(model, info.documentId);
                        }
                        else
                        {
                            await setAccessRightsForDocument(model, null);
                        }
                    }
                    break;
                case SourceTypeSelectVM.DocumentDecision:
                    {
                        if (sourceId > 0)
                        {
                            long? documentId = await repo.AllReadonly<DocumentDecision>()
                                                         .Where(x => x.Id == sourceId.Value)
                                                         .Select(x => (long?)x.DocumentId)
                                                         .FirstOrDefaultAsync();

                            if ((documentId ?? 0) > 0)
                            {
                                await setAccessRightsForDocument(model, documentId);
                            }
                            else
                            {
                                model.CanAccess = false;
                                model.CanChange = false;
                            }
                        }
                        else
                        {
                            await setAccessRightsForDocument(model, parentId);
                        }
                    }
                    break;
                case SourceTypeSelectVM.DocumentObligation:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetObligationDocument((int)sourceId);
                            await setAccessRightsForDocument(model, info.DocumentId);
                        }
                        else
                        {
                            //parentId  е Id на документ
                            await setAccessRightsForDocument(model, parentId);
                        }
                    }
                    break;
                case SourceTypeSelectVM.Case:
                    {
                        await setAccessRightsForCaseAsync(model, sourceId);
                    }
                    break;
                case SourceTypeSelectVM.CasePersonBulletin:
                    {
                        await setAccessRightsForCaseAsync(model, null);
                    }
                    break;
                case SourceTypeSelectVM.CaseSelectionProtokol:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseSelectionProtokol((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            await setAccessRightsForCaseAsync(model, parentId, "Регистрация на протокол");
                        }
                        if (operation == AuditConstants.Operations.Append)
                        {
                            model.CanAccess = userContext.IsUserInRole(AccountConstants.Roles.CaseInit);
                        }
                        model.CanChange &= userContext.IsUserInRole(AccountConstants.Roles.CaseInit);
                        model.CanChangeFull = userContext.IsUserInRole(AccountConstants.Roles.Supervisor);
                    }
                    break;
                case SourceTypeSelectVM.CasePerson:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCasePerson((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            await setAccessRightsForCaseAsync(model, parentId, "Регистрация на страна към дело");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionPerson:
                    {
                        //parentId е Id на заседание
                        var info = await caseInfo_GetCasePersonForSession((int)parentId);
                        await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                    }
                    break;
                case SourceTypeSelectVM.CasePersonAddress:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCasePersonAddress((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на страна по дело
                            var info = await caseInfo_GetCasePerson((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CasePersonLink:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCasePersonLink((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            await setAccessRightsForCaseAsync(model, (int)parentId, "Регистрация на връзки по дело");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSession:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseSession((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            await setAccessRightsForCaseAsync(model, parentId, "Регистрация на заседание");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionAct:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseSessionAct((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на заседание
                            var info = await caseInfo_GetCaseSession((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, $"Заседание: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionActCoordination:
                    {
                        if (parentId != null)
                        {
                            var actInfo = await caseInfo_GetCaseSessionAct((int)parentId);
                            await setAccessRightsForCaseAsync(model, actInfo.CaseId, $"Съгласуване на акт {actInfo.Info}");
                            if (sourceId > 0
                                && model.CanAccess
                                && !model.CanChange
                                && userContext.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Jury)
                            {
                                model.CanChange = (await repo.AllReadonly<CaseSessionActCoordination>()
                                                        .Where(x => x.Id == sourceId)
                                                        .Select(x => x.CaseLawUnit.LawUnitId)
                                                        .FirstOrDefaultAsync()) == userContext.LawUnitId;
                            }
                            if (actInfo.Declared)
                            {
                                model.CanChange = false;
                            }
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseNotification:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseNotification((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            await setAccessRightsForCaseAsync(model, (int)parentId);
                        }
                    }
                    break;
                case SourceTypeSelectVM.DeliveryItem:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetDeliveryItem((int)sourceId);
                            setAccessRightsForDeliveryItem(model, info);
                        }
                        else
                        {
                            var info = new DeliveryInfoVM()
                            {
                                Info = "Добавяне на призовка от друг съд"
                            };
                            setAccessRightsForDeliveryItem(model, info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionNotification:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseNotification((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на заседание
                            var info = await caseInfo_GetCaseSession((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, $"Заседание: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionActNotification:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseNotification((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на акт
                            var info = await caseInfo_GetCaseSessionAct((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, $"Акт: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseEvidence:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseEvidence((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            await setAccessRightsForCaseAsync(model, (int)parentId, "Регистрация на доказателство");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseEvidenceMovement:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseEvidenceMovement((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId е Id на доказателство
                            var info = await caseInfo_GetCaseEvidence((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseMovement:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseMovement((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            await setAccessRightsForCaseAsync(model, (int)parentId, "Регистрация на местоположение");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseLoadIndex:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseLoadIndex((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            await setAccessRightsForCaseAsync(model, (int)parentId, "Регистрация на натовареност към дело");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseLawyerHelp:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseLawyerHelp((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            await setAccessRightsForCaseAsync(model, (int)parentId, "Регистрация на искане за правна помощ");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseLawyerHelpPerson:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseLawyerHelpPerson((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на заседание
                            var info = await caseInfo_GetCaseLawyerHelp((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, $"Искане за правна помощ: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseLawyerHelpAssignedLawyer:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseLawyerHelpAssignedLawyer((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на заседание
                            var info = await caseInfo_GetCaseLawyerHelp((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, $"Искане за правна помощ: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseLoadCorrection:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseLoadCorrection((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            await setAccessRightsForCaseAsync(model, (int)parentId, "Регистрация на коригиращи коефициенти по дело");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseCrime:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseCrime((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            await setAccessRightsForCaseAsync(model, (int)parentId, "Регистрация на престъпление");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CasePersonCrime:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCasePersonCrime((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на престъпление
                            var info = await caseInfo_GetCaseCrime((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CasePersonMeasure:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCasePersonMeasure((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на лице в дело
                            var info = await caseInfo_GetCasePerson((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CasePersonDocument:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCasePersonDocument((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на лице в дело
                            var info = await caseInfo_GetCasePerson((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CasePersonSentence:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetPersonSentence((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на лице в дело
                            var info = await caseInfo_GetCasePerson((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CasePersonSentencePunishment:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetPersonSentencePunishment((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на присъда
                            var info = await caseInfo_GetPersonSentence((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CasePersonSentencePunishmentCrime:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetPersonSentencePunishmentCrime((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на наказание
                            var info = await caseInfo_GetPersonSentencePunishment((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.DocumentTemplate:
                    {
                        // Да го коментираме
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetDocumentTemplate((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на лице в дело
                            await setAccessRightsForCaseAsync(model, (int)parentId, "Регистрация на изходящ документ");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseLifecycle:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseLifecycle((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на лице в дело
                            await setAccessRightsForCaseAsync(model, (int)parentId, "Регистрация на интервал по дело");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CasePersonInheritance:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetPersonInheritance((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на лице в дело
                            var info = await caseInfo_GetCasePerson((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseLawUnit:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseLawUnit((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            await setAccessRightsForCaseAsync(model, (int)parentId);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseLawUnitDismisal:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseLawUnitDismisal((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на член от състава
                            var info = await caseInfo_GetCaseLawUnit((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseLawUnitDismisalList:
                    {
                        //parentId  е Id на дело
                        await setAccessRightsForCaseAsync(model, (int)parentId);
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionResult:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseSessionResult((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на заседание
                            var info = await caseInfo_GetCaseSession((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, $"Заседание: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionMeeting:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseSessionMeeting((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на заседание
                            var info = await caseInfo_GetCaseSession((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, $"Заседание: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionMeetingUser:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseSessionMeetingUser((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на сесия
                            var info = await caseInfo_GetCaseSessionMeeting((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, $"Сесия: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionActLawBase:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseSessionActLawBase((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на акт
                            var info = await caseInfo_GetCaseSessionAct((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, $"Акт: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionActDivorce:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseSessionActDivorce((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на акт
                            var info = await caseInfo_GetCaseSessionAct((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, $"Акт: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionActCompany:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseSessionActCompany((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на акт
                            var info = await caseInfo_GetCaseSessionAct((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, $"Акт: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionActComplain:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseSessionActComplain((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на акт
                            var info = await caseInfo_GetCaseSessionAct((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, $"Акт: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.SessionActObligation:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetObligation((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на акт
                            var info = await caseInfo_GetCaseSessionAct((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, $"Акт: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.SessionObligation:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetObligation((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на заседание
                            var info = await caseInfo_GetCaseSession((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, $"Заседание: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionFastDocument:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseSessionFastDocument((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на заседание
                            var info = await caseInfo_GetCaseSession((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, $"Заседание: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionActComplainResult:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseSessionActComplainResult((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId е Id на обжалване
                            var info = await caseInfo_GetCaseSessionActComplain((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, $"Обжалване: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionActComplainPerson:
                    {
                        //parentId е Id на обжалване - само това се подава
                        var info = await caseInfo_GetCaseSessionActComplainPerson((int)parentId);
                        await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                    }
                    break;
                case SourceTypeSelectVM.CaseMigration:
                    {
                        if (parentId != null && sourceId != null)
                        {
                            var info = await caseInfo_GetCaseMigration((int)sourceId);
                            await setAccessRightsForCaseAsync(model, (int)parentId, info.Info);
                        }
                        else
                        {
                            if (parentId == null)
                            {
                                parentId = await repo.AllReadonly<CaseMigration>()
                                                     .Where(x => x.Id == (int)sourceId)
                                                     .Select(x => x.CaseId)
                                                     .FirstOrDefaultAsync();
                            }

                            //parentId е Id на дело
                            await setAccessRightsForCaseAsync(model, (int)parentId, "Регистрация на движение");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseDeadLine:
                    {
                        //parentId е Id на дело
                        await setAccessRightsForCaseAsync(model, (int)parentId, "Срокове");
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionLawUnit:
                    {
                        //parentId е Id на заседание
                        var info = await caseInfo_GetCaseSessionLawUnit((int)parentId);
                        await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionNotificationList:
                    {
                        // тук е само редакция
                        var info = await caseInfo_GetCaseSessionNotificationList((int)sourceId);
                        await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionNotificationListLawUnit:
                    {
                        //parentId е Id на заседание
                        var sessionInfo = await repo.AllReadonly<CaseSession>()
                                                    .Where(x => x.Id == (int)parentId)
                                                    .Select(x => new CaseInfoVM
                                                    {
                                                        CaseId = x.CaseId
                                                    })
                                                    .FirstOrDefaultAsync();

                        //var info = caseInfo_GetCaseSessionNotificationList((int)parentId, NomenclatureConstants.NotificationPersonType.CaseLawUnit);
                        await setAccessRightsForCaseAsync(model, sessionInfo.CaseId, sessionInfo.Info);
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionNotificationListPerson:
                    {
                        //parentId е Id на заседание
                        var sessionInfo = await repo.AllReadonly<CaseSession>()
                                                    .Where(x => x.Id == (int)parentId)
                                                    .Select(x => new CaseInfoVM
                                                    {
                                                        CaseId = x.CaseId
                                                    })
                                                    .FirstOrDefaultAsync();

                        //var info = caseInfo_GetCaseSessionNotificationList((int)parentId, NomenclatureConstants.NotificationPersonType.CasePerson);
                        await setAccessRightsForCaseAsync(model, sessionInfo.CaseId, sessionInfo.Info);
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionNotificationListPersonLawUnit:
                    {
                        //parentId е Id на заседание
                        var sessionInfo = await repo.AllReadonly<CaseSession>()
                                                    .Where(x => x.Id == (int)parentId)
                                                    .Select(x => new CaseInfoVM
                                                    {
                                                        CaseId = x.CaseId
                                                    })
                                                    .FirstOrDefaultAsync();

                        //var info = caseInfo_GetCaseSessionNotificationList((int)parentId, 0);
                        await setAccessRightsForCaseAsync(model, sessionInfo.CaseId, sessionInfo.Info);
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionDoc:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseSessionDocById((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId е Id на заседание
                            var info = await caseInfo_GetCaseSessionDoc((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseFastProcess:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseFastProcess((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId е Id на дело
                            await setAccessRightsForCaseAsync(model, (int)parentId, "Заповедно производство");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseBankAccount:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseBankAccount((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId е Id на дело
                            await setAccessRightsForCaseAsync(model, (int)parentId, "Регистрация на движение");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseMoneyClaim:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseMoneyClaim((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId е Id на дело
                            await setAccessRightsForCaseAsync(model, (int)parentId, "Регистрация на движение");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseMoneyExpense:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseMoneyExpense((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId е Id на дело
                            await setAccessRightsForCaseAsync(model, (int)parentId, "Регистрация на движение");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseMoneyCollection:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetCaseMoneyCollection((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId е Id на Обстоятелства по заповедни производства
                            var info = await caseInfo_GetCaseMoneyClaim((int)parentId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, $"Обстоятелствo: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.ExecList:
                    {
                        setAccessRightsForMoney(model, sourceId);
                    }
                    break;
                case SourceTypeSelectVM.ExchangeDoc:
                    {
                        setAccessRightsForMoney(model, sourceId);
                    }
                    break;
                case SourceTypeSelectVM.Integration_EISPP:
                    {
                        if (sourceId != null)
                        {
                            var info = await caseInfo_GetEisppEvent((int)sourceId);
                            await setAccessRightsForCaseAsync(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            await setAccessRightsForCaseAsync(model, (int)parentId, $"Добавяне ЕИСПП събитие");
                        }
                    }
                    break;
                default:
                    model.IsRead = false;
                    break;
            }

            return model;
        }

        /// <summary>
        /// Определяне права за достъп до документ от Централна Регистратура, courtId=184
        /// </summary>
        /// <param name="model"></param>
        /// <param name="documentId"></param>
        /// <param name="createdCourtId"></param>
        /// <returns></returns>
        private async Task setAccessRightsForDocumentCR(CurrentContextModel model, long documentId, int? createdCourtId)
        {
            model.CanAccess = userContext.IsUserInFeature(AccountConstants.Features.Modules.Documents);

            if (createdCourtId == NomenclatureConstants.Courts.RandomAssignment)
            {
                var hasRegCases = await repo.AllReadonly<Case>()
                                            .Where(x => x.Document.AssignmentDocumentId == documentId)
                                            .Where(x => x.RegNumber != null)
                                            .AnyAsync();
                if (hasRegCases)
                {
                    model.CanChange = false;
                    model.CanChangeFull = false;
                }
                else
                {
                    model.CanAccess = false;
                }
                return;
            }
            var hasAssignmentTask = await repo.AllReadonly<WorkTask>()
                                             .Where(x => x.SourceType == SourceTypeSelectVM.Document && x.SourceId == documentId)
                                             .Where(x => x.TaskTypeId == WorkTaskConstants.Types.DocumentForGlobalAssignment)
                                             .AnyAsync();
            model.CanChange = false;
            model.CanChangeFull = false;

            if (hasAssignmentTask)
            {
                //Ако е пуснато за разпределение, документа е достъпен от всички съдилища, без промяна
            }
            else
            {
                //Ако още няма задача за централно разпределяне само съда, входирал документа може да го променя
                if (createdCourtId == userContext.CourtId)
                {
                    model.CanChange = true;
                    model.CanChangeFull = true;
                }
                else
                {
                    model.CanAccess = false;
                }
            }
            if (model.Info.Operation == AuditConstants.Operations.View)
            {
                model.CanChange = false;
                model.CanChangeFull = false;
            }

        }
        private async Task setAccessRightsForDocument(CurrentContextModel model, object sourceId)
        {
            model.CanAccess = userContext.IsUserInFeature(AccountConstants.Features.Modules.Documents);
            model.CanChange = model.CanAccess;
            if (sourceId != null)
            {
                long documentId = (long)sourceId;
                var info = await repo.AllReadonly<Document>()
                                    .Where(x => x.Id == documentId)
                                    .Select(x => new
                                    {
                                        Id = x.Id,
                                        CourtId = x.CourtId,
                                        DocumentKindId = x.DocumentGroup.DocumentKindId,
                                        DocumentDirectionId = x.DocumentDirectionId,
                                        DateExpired = x.DateExpired,
                                        BaseObject = $"{x.DocumentType.Label} {x.DocumentNumber}/{x.DocumentDate:dd.MM.yyyy}",
                                        IsRestrictedAccess = x.IsRestictedAccess || (x.IsSecret == true),
                                        ConnectedCaseId = x.DocumentCaseInfo.Select(dc => dc.CaseId).FirstOrDefault(),
                                        SourceType = x.DocumentTemplates.Select(t => t.SourceType).FirstOrDefault(),
                                        SourceId = x.DocumentTemplates.Select(t => t.SourceId).FirstOrDefault(),
                                        x.CreatedCourtId
                                    }).FirstOrDefaultAsync();

                if (info != null)
                {

                    model.Info.BaseObject = info.BaseObject;
                    if (info.CourtId == NomenclatureConstants.Courts.RandomAssignment)
                    {
                        await setAccessRightsForDocumentCR(model, info.Id, info.CreatedCourtId);
                        return;
                    }

                    //Ако няма достъп и метода е за преглед
                    if (!model.CanAccess && model.Info.Operation == AuditConstants.Operations.View)
                    {
                        //Ако няма достъп до модул Регистратура, но има задача насочена към лицето
                        model.CanAccess = await repo.AllReadonly<WorkTask>()
                                              .Where(x => x.SourceType == SourceTypeSelectVM.Document && x.SourceId == info.Id)
                                              .Where(x => x.UserId == userContext.UserId)
                                              .Select(x => x.Id)
                                              .AnyAsync();
                    }
                    if (info.IsRestrictedAccess)
                    {
                        model.CanAccess &= userContext.IsUserInRole(AccountConstants.Roles.RestrictedAccess);
                    }
                    int documentCourtId = info.CreatedCourtId ?? info.CourtId;
                    //TODO: Права!?!
                    bool sameCourt = (userContext.CourtId == documentCourtId);
                    model.CanAccess &= sameCourt;


                    model.CanChangeFull = sameCourt && info.DateExpired == null && userContext.IsUserInRole(AccountConstants.Roles.Supervisor);
                    if (model.CanChangeFull)
                    {
                        switch (info.DocumentKindId)
                        {
                            case DocumentConstants.DocumentKind.InitialDocument:


                                //Иницииращите документи могат да се премахват до образуването на делото
                                var initCaseInfo = await repo.AllReadonly<Case>(x => x.DocumentId == documentId)
                                                .Select(x => new
                                                {
                                                    hasSpecialAccess = x.CaseClassifications.Any(c => c.ClassificationId == NomenclatureConstants.CaseClassifications.SpecialAccess && c.DateTo == null)
                                                    ,
                                                    x.RegNumber
                                                    ,
                                                    x.CaseCodeId
                                                    ,
                                                    CaseId = x.Id
                                                })
                                                .FirstOrDefaultAsync();
                                if (initCaseInfo != null && !string.IsNullOrEmpty(initCaseInfo.RegNumber))
                                {
                                    model.CanChangeFull = false;
                                    //Проверката за специален достъп се прави през новоинициираното дело на иницииращите документи
                                    if (initCaseInfo.hasSpecialAccess)
                                    {
                                        model.CanAccess &= await checkSpecialAccessForCase(documentCourtId, initCaseInfo.CaseId, initCaseInfo.CaseCodeId ?? 0);
                                    }
                                }

                                break;
                            case DocumentConstants.DocumentKind.CompliantDocument:
                                //Съпровождащите документи се премахват преди да са разгледани/окончателно разгледани в заседание
                                if (await repo.AllReadonly<CaseSessionDoc>()
                                .Where(x => x.DocumentId == documentId && NomenclatureConstants.SessionDocState.UsedInSession.Contains(x.SessionDocStateId)).AnyAsync())
                                {
                                    model.CanChangeFull = false;
                                }
                                break;
                        }
                    }

                    int connectedCaseId = info.ConnectedCaseId ?? 0;
                    if (connectedCaseId > 0)
                    {
                        var isSpecialAccess = false;
                        if (info.DocumentKindId != DocumentConstants.DocumentKind.InitialDocument)
                        {
                            //Проверката за специален достъп прави през свързаното дело на съпровождащи/изходящи документи
                            repo.AllReadonly<CaseClassification>().Any(c => c.CaseId == connectedCaseId && c.CaseSessionId == null && c.ClassificationId == NomenclatureConstants.CaseClassifications.SpecialAccess && c.DateTo == null);
                        }
                        if ((documentCourtId != userContext.CourtId) || isSpecialAccess)
                        {
                            var caseContext = new CurrentContextModel()
                            {
                                Info = new ContextInfoModel()
                            };
                            await setAccessRightsForCaseAsync(caseContext, connectedCaseId);
                            model.CanAccess = caseContext.CanAccess;
                            model.CanChange = false;
                            model.CanChangeFull = false;
                        }
                    }
                    if (info.SourceType > 0 && info.SourceId > 0)
                    {
                        switch (info.SourceType)
                        {
                            case SourceTypeSelectVM.CaseSessionAct:
                                model.Info.ObjectInfo = (await caseInfo_GetCaseSessionAct((int)info.SourceId)).Info;
                                break;
                            case SourceTypeSelectVM.Case:
                                model.Info.ObjectInfo = (await auditInfo_Case((int)info.SourceId))?.Info;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }


            switch (model.Info.Operation)
            {
                case AuditConstants.Operations.View:
                    model.CanChange = false;
                    break;
            }
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseBankAccount(int id)
        {
            return await repo.AllReadonly<CaseBankAccount>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = x.CaseBankAccountType.Label + " IBAN " + x.IBAN + " BIC: " + x.BIC + " Име на банката: " + x.BankName
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseFastProcess(int id)
        {
            return await repo.AllReadonly<CaseFastProcess>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = "Заповедно производство по дело: " + (x.Case.RegNumber ?? string.Empty)
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseMoneyClaim(int id)
        {
            return await repo.AllReadonly<CaseMoneyClaim>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = x.CaseMoneyClaimGroup.Label + " " + x.CaseMoneyClaimType.Label + " номер " + x.ClaimNumber
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseMoneyCollection(int id)
        {
            return await repo.AllReadonly<CaseMoneyCollection>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = x.CaseMoneyCollectionGroup.Label +
                                        (x.CaseMoneyCollectionType != null ? " " + x.CaseMoneyCollectionType.Label : string.Empty) +
                                        (x.CaseMoneyCollectionKind != null ? " " + x.CaseMoneyCollectionKind.Label : string.Empty)
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseMoneyExpense(int id)
        {
            return await repo.AllReadonly<CaseMoneyExpense>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = x.CaseMoneyExpenseType.Label + " " + x.Amount.ToString("0.00") + " " + x.Currency.Label
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseSessionFastDocument(int id)
        {
            return await repo.AllReadonly<CaseSessionFastDocument>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = x.CasePerson.FullName + " " + x.SessionDocType.Label + " " + x.SessionDocState.Label
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetObligation(int id)
        {
            return await repo.AllReadonly<Obligation>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseSession != null ? x.CaseSession.CaseId : x.CaseSessionAct.CaseId ?? 0,
                                 Info = x.ObligationNumber + "/" + x.ObligationDate.ToString("dd.MM.yyyy") + " " + x.Amount.ToString("0.00")
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<DocumentInfoLogVM> caseInfo_GetObligationDocument(int id)
        {
            return await repo.AllReadonly<Obligation>()
                             .Where(x => x.Id == id)
                             .Select(x => new DocumentInfoLogVM()
                             {
                                 DocumentId = x.DocumentId ?? 0,
                                 Info = x.ObligationNumber + "/" + x.ObligationDate.ToString("dd.MM.yyyy") + " " + x.Amount.ToString("0.00")
                             })
                             .FirstOrDefaultAsync() ?? new DocumentInfoLogVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseSessionDoc(int CaseSessionId)
        {
            return await repo.AllReadonly<CaseSession>()
                             .Where(x => x.Id == CaseSessionId)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = "Съпровождащи документи: " + string.Join(",", x.CaseSessionDocs.Select(a => a.Document.DocumentNumber + "/" + a.Document.DocumentDate.ToString("dd.MM.yyyy") + " " + a.SessionDocState.Label))
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseSessionDocById(int id)
        {
            return await repo.AllReadonly<CaseSessionDoc>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId ?? 0,
                                 Info = "Съпровождащ документ: " + x.Document.DocumentNumber + "/" + x.Document.DocumentDate.ToString("dd.MM.yyyy") + " " + x.SessionDocState.Label
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseMigration(int id)
        {
            return await repo.AllReadonly<CaseMigration>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = x.CaseMigrationType.Label + " изпратено от " + ((x.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Outgoing) ? x.Case.Court.Label : x.PriorCase.Court.Label) +
                                                                    " изпратено към " + ((x.SendToCourt != null) ? x.SendToCourt.Label : (x.SendToInstitution != null ? x.SendToInstitution.FullName : ""))
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseSessionNotificationList(int id)
        {
            return await repo.AllReadonly<CaseSessionNotificationList>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId ?? 0,
                                 Info = (x.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CaseLawUnit) ? x.CaseLawUnit.LawUnit.FullName + " " + x.CaseLawUnit.JudgeRole.Label :
                                                                                                                                 x.CasePerson.FullName + " " + x.CasePerson.PersonRole.Label
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseSessionNotificationList(int CaseSessionId, int NotificationPersonType)
        {
            var result = new CaseInfoVM()
            {
                CaseId = (await repo.GetByIdAsync<CaseSession>(CaseSessionId)).CaseId
            };

            return result;
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseSessionLawUnit(int CaseSessionId)
        {
            return await repo.AllReadonly<CaseSession>()
                             .Where(x => x.Id == CaseSessionId)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = "Състав: " + string.Join(",", x.CaseLawUnits.Select(a => a.LawUnit.FullName + " " + a.JudgeRole.Label))
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseSessionActComplainPerson(int CaseSessionActComplainId)
        {
            return await repo.AllReadonly<CaseSessionActComplain>()
                             .Where(x => x.Id == CaseSessionActComplainId)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId ?? 0,
                                 Info = $"{x.ComplainDocument.DocumentType.Label} {x.ComplainDocument.DocumentNumber}/{x.ComplainDocument.DocumentDate:dd.MM.yyyy}, {x.ComplainState.Label}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseSessionActComplainResult(int id)
        {
            return await repo.AllReadonly<CaseSessionActComplainResult>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = "Резултат от обжалване: " + x.CaseSessionAct.ActType.Label + " " + x.CaseSessionAct.RegNumber + "/" + (x.CaseSessionAct.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") + " - " + x.ActResult.Label
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseSessionActComplain(int id)
        {
            return await repo.AllReadonly<CaseSessionActComplain>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId ?? 0,
                                 Info = "Обжалване по съпровождащ документ: " + x.ComplainDocument.DocumentType.Label + " " + x.ComplainDocument.DocumentNumber + "/" + x.ComplainDocument.DocumentDate.ToString("dd.MM.yyyy")
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseSessionActLawBase(int id)
        {
            return await repo.AllReadonly<CaseSessionActLawBase>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId ?? 0,
                                 Info = "Нормативен текст: " + x.LawBase.Label
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseSessionMeetingUser(int id)
        {
            return await repo.AllReadonly<CaseSessionMeetingUser>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId ?? 0,
                                 Info = "Секретар към сесия: " + x.SecretaryUser.LawUnit.FullName
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseSessionMeeting(int id)
        {
            return await repo.AllReadonly<CaseSessionMeeting>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId ?? 0,
                                 Info = x.SessionMeetingType.Label + " от: " + x.DateFrom.ToString("dd.MM.yyyy") + " до: " + x.DateTo.ToString("dd.MM.yyyy")
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseSessionResult(int id)
        {
            return await repo.AllReadonly<CaseSessionResult>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId ?? 0,
                                 Info = x.SessionResult.Label + (x.SessionResultBase != null ? " - " + x.SessionResultBase.Label : string.Empty)
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseLawUnit(int id)
        {
            return await repo.AllReadonly<CaseLawUnit>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = $"{x.LawUnit.FullName} {x.JudgeRole.Label}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseLawUnitDismisal(int id)
        {
            return await repo.AllReadonly<CaseLawUnitDismisal>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseLawUnit.CaseId,
                                 Info = $"{x.CaseLawUnit.LawUnit.FullName} {x.DismisalType.Label} {x.DismisalDate:dd.MM.yyyy HH mm}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseSessionActDivorce(int id)
        {
            return await repo.AllReadonly<CaseSessionActDivorce>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId ?? 0,
                                 Info = $"Съобщение за прекратяване на граждански брак {x.RegNumber} от {x.RegDate:dd.MM.yyyy HH mm}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseLifecycle(int id)
        {
            return await repo.AllReadonly<CaseLifecycle>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = $"Интервал по дело: {x.LifecycleType.Label} повторение {x.Iteration} от {x.DateFrom:dd.MM.yyyy HH mm}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetDocumentTemplate(int id)
        {
            return await repo.AllReadonly<DocumentTemplate>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId ?? 0,
                                 Info = $"Изх. документ към дело: {x.DocumentKind.Label} {x.DocumentGroup.Label} {x.DocumentType.Label}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetPersonInheritance(int id)
        {
            return await repo.AllReadonly<CasePersonInheritance>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = $"Наследство на {x.CasePerson.FullName} Постановена от {x.Court.Label} Акт: {x.CaseSessionAct.RegNumber} {x.CaseSessionAct.RegDate:dd.MM.yyyy HH mm} - {x.CasePersonInheritanceResult.Label}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCasePersonMeasure(int id)
        {
            return await repo.AllReadonly<CasePersonMeasure>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = "Мярка към: " + x.CasePerson.FullName + " институция, определила мярката: " + x.MeasureInstitution.FullName + " вид мярка: " + x.MeasureTypeLabel
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCasePersonDocument(int id)
        {
            return await repo.AllReadonly<CasePersonDocument>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = "Личен документ на: " + x.CasePerson.FullName + " държава: " + x.IssuerCountryName + " документ: " + x.PersonalDocumentTypeLabel + " номер: " + x.DocumentNumber
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetPersonSentence(int id)
        {
            return await repo.AllReadonly<CasePersonSentence>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = $"{x.CasePerson.FullName} Постановена от {x.Court.Label} Акт: {x.CaseSessionAct.RegNumber} {x.CaseSessionAct.RegDate:dd.MM.yyyy HH mm}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetPersonSentencePunishment(int id)
        {
            return await repo.AllReadonly<CasePersonSentencePunishment>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId ?? 0,
                                 Info = $"Наложено наказание по НК: {x.SentenceType.Label} Сумарен ред за присъди: {(x.IsSummaryPunishment ? "Да" : "Не")}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetPersonSentencePunishmentCrime(int id)
        {
            return await repo.AllReadonly<CasePersonSentencePunishmentCrime>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId ?? 0,
                                 Info = $"Участие в наложени наказания към присъда. Престъпление: {x.CaseCrime.CrimeName} роля: {x.PersonRoleInCrime.Label} рецидив: {x.RecidiveType.Label}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseCrime(int id)
        {
            return await repo.AllReadonly<CaseCrime>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = $"{x.EISSPNumber} {x.CrimeName}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCasePersonCrime(int id)
        {
            return await repo.AllReadonly<CasePersonCrime>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = $"{x.CasePerson.FullName} {x.PersonRoleInCrime.Label} {x.RecidiveType.Label}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseLoadCorrection(int id)
        {
            return await repo.AllReadonly<CaseLoadCorrection>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = $"{x.CaseLoadCorrectionActivity.Label} {x.CorrectionDate:dd.MM.yyyy HH mm}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseLoadIndex(int id)
        {
            return await repo.AllReadonly<CaseLoadIndex>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = x.IsMainActivity ? $"Основна дейност {x.LawUnit.FullName} {x.CaseLoadElementGroup.Label} {x.CaseLoadElementType.Label} {x.DateActivity:dd.MM.yyyy HH mm}" : $"Допълнителна дейност {x.LawUnit.FullName} {x.CaseLoadAddActivity.Label} {x.DateActivity:dd.MM.yyyy HH mm}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseLawyerHelp(int id)
        {
            return await repo.AllReadonly<CaseLawyerHelp>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = x.LawyerHelpBase.Label + " " + x.LawyerHelpType.Label + (!string.IsNullOrEmpty(x.Description) ? " " + x.Description : string.Empty)
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseLawyerHelpPerson(int id)
        {
            return await repo.AllReadonly<CaseLawyerHelpPerson>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseLawyerHelp.CaseId,
                                 Info = "Правна помощ за: " + x.CasePerson.FullName + (x.AssignedLawyerId != null ? " назначен адвокат: " + x.AssignedLawyer.FullName : string.Empty)
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseLawyerHelpAssignedLawyer(int id)
        {
            return await repo.AllReadonly<CaseLawyerHelpAssignedLawyer>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseLawyerHelp.CaseId,
                                 Info = $"Върнати адвокати по заявка за правна помощ от ЕЕСПП: {x.LawyerNumber + " " + x.LawyerName} за лице/лица: {string.Join(", ", x.Persons.Select(p => p.CaseLawyerHelpPerson.CasePerson.FullName + " (" + p.CaseLawyerHelpPerson.CasePerson.PersonRole.Label + ")"))}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseMovement(int id)
        {
            return await repo.AllReadonly<CaseMovement>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = $"{x.MovementType.Label} {x.DateSend:dd.MM.yyyy HH mm}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseEvidence(int id)
        {
            return await repo.AllReadonly<CaseEvidence>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = $"{x.EvidenceType.Label} {x.RegNumber}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseEvidenceMovement(int id)
        {
            return await repo.AllReadonly<CaseEvidenceMovement>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId ?? 0,
                                 Info = $"{x.EvidenceMovementType.Label} {x.MovementDate:dd.MM.yyyy HH mm}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCasePerson(int id)
        {
            return await repo.AllReadonly<CasePerson>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = $"{x.FullName} {x.PersonRole.Label}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCasePersonLink(int id)
        {
            return await repo.AllReadonly<CasePersonLink>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = x.CasePerson.FullName + (x.CasePersonRel != null ? " Упълномощено лице: " + x.CasePersonRel.FullName : string.Empty) +
                                                                (x.CasePersonSecondRel != null ? " Втори представляващ: " + x.CasePersonSecondRel.FullName : string.Empty)
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCasePersonForSession(int CaseSessionId)
        {
            return await repo.AllReadonly<CaseSession>()
                             .Where(x => x.Id == CaseSessionId)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = $"{x.SessionType.Label} {x.DateFrom:dd.MM.yyyy HH:mm}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCasePersonAddress(int id)
        {
            return await repo.AllReadonly<CasePersonAddress>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CasePerson.CaseId,
                                 Info = $"{x.CasePerson.FullName} {x.Address.FullAddress}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseSession(int id)
        {
            return (await repo.AllReadonly<CaseSession>()
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = $"{x.SessionState.Label} {x.SessionType.Label} {x.DateFrom:dd.MM.yyyy HH mm}"
                       }).FirstOrDefaultAsync()) ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseSessionAct(int id)
        {
            return (await repo.AllReadonly<CaseSessionAct>()
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId ?? 0,
                           Declared = x.ActDeclaredDate != null,
                           Info = $"{x.ActState.Label} {x.ActType.Label}" + (x.RegDate != null ? $" {x.RegNumber}/{x.RegDate:dd.MM.yyyy HH:mm}" : ""),
                       }).FirstOrDefaultAsync()) ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseNotification(int id)
        {
            return (await repo.AllReadonly<CaseNotification>()
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = $"{x.NotificationType.Label} {x.NotificationState.Label} {x.RegNumber}/{x.RegDate:dd.MM.yyyy}",
                       }).FirstOrDefaultAsync()) ?? new CaseInfoVM();
        }

        private async Task<DeliveryInfoVM> caseInfo_GetDeliveryItem(int id)
        {
            return (await repo.AllReadonly<DeliveryItem>()
                       .Where(x => x.Id == id)
                       .Select(x => new DeliveryInfoVM()
                       {
                           CaseInfo = x.CaseInfo,
                           Info = $"{x.NotificationType.Label} {x.NotificationState.Label}" + (x.RegDate != null ? $" {x.RegNumber}/{x.RegDate:dd.MM.yyyy}" : ""),
                       }).FirstOrDefaultAsync()) ?? new DeliveryInfoVM();
        }
        private async Task<CaseInfoVM> caseInfo_GetCaseSelectionProtokol(int id)
        {
            return await repo.AllReadonly<CaseSelectionProtokol>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = $"{x.SelectedLawUnit.FullName} ({x.JudgeRole.Label})"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetCaseSessionActCompany(int id)
        {
            return await repo.AllReadonly<CaseSessionActCompany>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = $"Заявление за регистрация с Акт: {x.CaseSessionAct.RegNumber} {x.CaseSessionAct.RegDate:dd.MM.yyyy HH mm}"
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task<CaseInfoVM> caseInfo_GetEisppEvent(int id)
        {
            var eisppTblElements = repo.AllReadonly<EisppTblElement>();

            return await repo.AllReadonly<EisppEventItem>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseInfoVM()
                             {
                                 CaseId = x.CaseId,
                                 Info = $"{x.Case.EISSPNumber} {x.EventDate:dd.MM.yyyy} " +
                                        (eisppTblElements.Where(e => e.Code == x.EventType.ToString())
                                                         .Select(e => e.Label)
                                                         .FirstOrDefault() ?? ""),
                             })
                             .FirstOrDefaultAsync() ?? new CaseInfoVM();
        }

        private async Task setAccessRightsForCaseAsync(CurrentContextModel model, object id, string objectInfo = "")
        {
            //3.1.Разпределение на дела
            var hasCaseInitRole = userContext.IsUserInRole(AccountConstants.Roles.CaseInit);
            //Да има Роля Деловодство 
            model.CanAccess = userContext.IsUserInRole(AccountConstants.Roles.DocumentEdit)
                            //или 3.1. Разпределение на дела
                            || hasCaseInitRole
                            //или 3.5. Ръководство
                            || userContext.IsUserInRole(AccountConstants.Roles.CourtManager);

            if (id != null)
            {
                if (Convert.ToInt32(id) > 0)
                {
                    int caseId = Convert.ToInt32(id);

                    var info = await auditInfo_Case(caseId);

                    model.Info.BaseObject = info.Info;
                    model.Info.ObjectInfo = objectInfo;
                    bool judgListCheck = info.JudgeLawUnits.Contains(userContext.LawUnitId);
                    if (info.CourtId == userContext.CourtId)
                    {
                        //Ако делото е на текущия съд, достъпа за редакция е същия като за преглед
                        //или Управление на дела но да е част от делото
                        model.CanAccess |= (userContext.IsUserInRole(AccountConstants.Roles.CaseEdit) && judgListCheck);

                        if (info.IsRestrictedAccess)
                        {
                            model.CanAccess &= userContext.IsUserInRole(AccountConstants.Roles.RestrictedAccess);
                        }

                        if (model.CanAccess && info.IsSpecialAccess && !judgListCheck && !hasCaseInitRole)
                        {
                            model.CanAccess = await checkSpecialAccessForCase(info.CourtId, 0, info.CaseCodeId);
                        }

                        if (!model.CanAccess && userContext.IsUserInRole(AccountConstants.Roles.CaseEdit))
                        {
                            //Ако делото е в същия съд на друг съдия, но участва в свързано дело
                            model.CanAccess = info.AllMigrationJudgeLawUnits.Contains(userContext.LawUnitId);
                            model.CanChange = false;
                        }

                        ////Проверява се кое е делото, в което е последното примащо движение.
                        ////Ако няма движения - това е текущото дело
                        //05.10.2020 - Премахва се заключването на редакцията при изпращане в друг съд
                        //Заключват се дела при изпращане на повече от един акт за обжалване и връщането на едното
                        //model.CanChange = info.LastInMigrationCaseId == caseId;

                        model.CanChangeFull = userContext.IsUserInRole(AccountConstants.Roles.Supervisor);

                        //Забранява се редакцията на делата, които са в определени статуси 
                        if (NomenclatureConstants.CaseState.DisableEditStates.Contains(info.CaseStateId))
                        {
                            //TODO: GlobalAdmin - може би трябва да може да пипа
                            model.CanChange = false;
                            model.CanChangeFull = false;
                        }
                    }
                    else
                    {
                        //Ако е делото от друг съд, право на достъп имат само лицата в свързаните дела
                        model.CanAccess = info.OtherCourtsJudgeLawUnits.Contains(userContext.LawUnitId)
                            || userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator)
                            //Ако делото има движение към или от текущия съд
                            || (info.AllMigrationCourts.Contains(userContext.CourtId) && userContext.IsUserInFeature(AccountConstants.Features.Modules.CaseAccessData))
                            //Новите дела по Заповедно производство се достъпват от всички съдилища, без промяна
                            || info.IsFastProcess;
                        model.CanChange = false;
                        model.CanChangeFull = false;

                        if (model.Info.SourceType == SourceTypeSelectVM.CaseSessionAct && AuditConstants.Operations.ChangingOperations.Contains(model.Info.Operation))
                        {
                            //От друг съд няма право на достъп до актовете
                            model.CanAccess = false;
                        }
                    }

                    if (!model.CanAccess && userContext.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Jury)
                    {
                        model.CanAccess = await repo.AllReadonly<CaseLawUnit>()
                                                .Where(x => x.CaseId == caseId && x.LawUnitId == userContext.LawUnitId)
                                                .AnyAsync();
                        model.CanChange = false;
                    }
                }
            }
            else
            {
                model.CanAccess |= userContext.IsUserInRole(AccountConstants.Roles.CaseEdit);
            }


        }

        async Task<bool> checkSpecialAccessForCase(int courtId, int caseId, int caseCodeId)
        {
            if (caseCodeId == 0)
                return true;
            var dtToday = DateTime.Now;
            var dtNexDay = DateTime.Now.AddDays(1);
            var isCaseLawunit = false;
            if (caseId > 0 && courtId == userContext.CourtId)
            {
                isCaseLawunit = await repo.AllReadonly<CaseLawUnit>()
                                    .Where(x => x.CaseId == caseId && x.CaseSessionId == null)
                                    .Where(x => x.LawUnitId == userContext.LawUnitId)
                                    .Where(x => (x.DateTo ?? dtNexDay) > dtToday)
                                    .AnyAsync();
            }
            var hasSpecialGroup = false;
            if (!isCaseLawunit)
            {
                hasSpecialGroup = await repo.AllReadonly<CourtGroup>()
                                    .Where(x => x.CourtId == courtId)
                                    .Where(x => x.GroupKind == NomenclatureConstants.CourtGroupKinds.SpecialAccess)
                                    .Where(x => x.CourtLawUnitGroups.Any(g => g.LawUnitId == userContext.LawUnitId && (g.DateTo ?? dtNexDay) > dtToday))
                                    .Where(x => x.CourtGroupCodes.Any(g => g.CaseCodeId == caseCodeId && (g.DateTo ?? dtNexDay) > dtToday))
                                    .Where(x => (x.DateTo ?? dtNexDay) > dtToday)
                                    .AnyAsync();
            }

            return isCaseLawunit || hasSpecialGroup;
        }

        public Task<string> GetCaseInfoById(int id)
        {
            return repo.AllReadonly<Case>()
                                   .Where(x => x.Id == id)
                                   .Select(x => (string.IsNullOrEmpty(x.RegNumber)) ? $"{x.CaseType.Code} по {x.Document.DocumentType.Label} {x.Document.DocumentNumber}" : $"{x.CaseType.Code} {x.RegNumber}/{x.RegDate:dd.MM.yyyy}")
                                   .FirstOrDefaultAsync();
        }

        private async Task<CaseAuditInfoVM> auditInfo_Case(int caseId)
        {
            var result = await repo.AllReadonly<Case>()
                                    .Where(x => x.Id == caseId)
                                    .Select(x => new CaseAuditInfoVM
                                    {
                                        LastInMigrationCaseId = caseId,
                                        CourtId = x.CourtId,
                                        CaseId = x.Id,
                                        CaseCodeId = x.CaseCodeId ?? 0,
                                        CaseStateId = x.CaseStateId,
                                        Info = (string.IsNullOrEmpty(x.RegNumber)) ? $"{x.CaseType.Code} по {x.Document.DocumentType.Label} {x.Document.DocumentNumber}" : $"{x.CaseType.Code} {x.RegNumber}/{x.RegDate:dd.MM.yyyy}",
                                        JudgeLawUnits = x.CaseLawUnits
                                                         //Гледат се всички, независимо дали са в делото или в заседанието - заради заместванията
                                                         //.Where(c => c.CaseSessionId == null)
                                                         .Where(c => c.DateFrom <= DateTime.Now && (c.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                                         .Where(c => NomenclatureConstants.JudgeRole.JudgeAndManualRoles.Contains(c.JudgeRoleId))
                                                         .Select(c => c.LawUnitId).ToArray(),
                                        //InitMigrations = x.CaseMigrations.Select(m => m.InitialCaseId).Distinct().ToArray(),
                                        OtherCourts = x.CaseMigrations.Where(x => x.CourtId > 0).Select(m => m.CourtId.Value).Distinct().ToArray(),
                                        ToCourts = x.CaseMigrations.Where(x => x.SendToCourtId > 0).Select(m => m.SendToCourtId.Value).Distinct().ToArray(),
                                        IsRestrictedAccess = (x.CaseClassifications != null) ? x.CaseClassifications.Any(c => c.DateTo == null && NomenclatureConstants.CaseClassifications.RestictedAccess.Contains(c.ClassificationId)) : false,
                                        IsSpecialAccess = (x.CaseClassifications != null) ? x.CaseClassifications.Any(c => c.DateTo == null && NomenclatureConstants.CaseClassifications.SpecialAccess == c.ClassificationId) : false,
                                        IsFastProcess = x.IsFastProcess ?? false
                                    }).FirstOrDefaultAsync();

            if (result != null)
            {
                result.InitMigrations = await GetInitialCasesByCaseIdAsync(caseId);
            }

            if (result.InitMigrations.Length > 0)
            {
                if (result.CourtId != userContext.CourtId)
                {
                    int[] allMigrationCaseIds = (await repo.AllReadonly<CaseMigration>()
                                                          .Where(x => result.InitMigrations.Contains(x.InitialCaseId))
                                                          .Select(x => x.CaseId).Distinct().ToArrayAsync()).Union(result.InitMigrations).Distinct().ToArray();


                    //TODO: Да се изтеглят всички дела по initcase
                    result.OtherCourtsJudgeLawUnits = await repo.AllReadonly<CaseLawUnit>()
                                                          .Where(x => allMigrationCaseIds.Contains(x.CaseId))
                                                          .Where(c => c.CaseSessionId == null)
                                                          .Where(c => c.DateFrom <= DateTime.Now && (c.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                                          .Where(c => NomenclatureConstants.JudgeRole.JudgeAndManualRoles.Contains(c.JudgeRoleId))
                                                          .Select(c => c.LawUnitId).ToArrayAsync();



                    var allCourts = await repo.AllReadonly<CaseMigration>()
                                                         .Where(x => result.InitMigrations.Contains(x.InitialCaseId))
                                                         .Where(x => NomenclatureConstants.CaseMigrationDirections.DirectionsForAccess.Contains(x.CaseMigrationType.MigrationDirection))
                                                         .Select(x => new
                                                         {
                                                             toCourt = x.SendToCourtId ?? 0,
                                                             fromCourt = x.CourtId ?? 0
                                                         })
                                                         .ToArrayAsync();

                    result.AllMigrationCourts = allCourts.Select(x => x.fromCourt).Union(allCourts.Select(x => x.toCourt)).Where(x => x > 0).Distinct().ToArray();
                }
                else
                {
                    result.LastInMigrationCaseId = await repo.AllReadonly<CaseMigration>()
                                                 .Where(x => result.InitMigrations.Contains(x.InitialCaseId))
                                                 .Where(
                                                    m => m.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming
                                                    && m.OutCaseMigration.SendToCourtId > 0
                                                  )
                                                 .OrderByDescending(m => m.Id)
                                                 .Select(m => m.CaseId).FirstOrValueAsync(caseId);

                    result.AllMigrationJudgeLawUnits = await repo.AllReadonly<CaseMigration>()
                                      .Where(x => result.InitMigrations.Contains(x.InitialCaseId))
                                      .SelectMany(x => x.Case.CaseLawUnits)
                                      .Where(c => c.CaseSessionId == null)
                                      .Where(c => c.DateFrom <= DateTime.Now && (c.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                      .Where(c => NomenclatureConstants.JudgeRole.JudgeAndManualRoles.Contains(c.JudgeRoleId))
                                      .Select(c => c.LawUnitId).ToArrayAsync();

                }
            }
            return result ?? new CaseAuditInfoVM();
        }

        /// <summary>
        /// Извличане на първото дело от Вертикално движение на дело - между институциите
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        protected async Task<int[]> GetInitialCasesByCaseIdAsync(int caseId)
        {
            List<int> result = await repo.AllReadonly<CaseMigration>()
                                         .Where(x => x.CaseId == caseId)
                                         .Select(x => x.InitialCaseId)
                                         .Distinct()
                                         .ToListAsync();

            //Заради движенията по свързване и обединяване се добавя и текущото дело
            result.Add(caseId);

            //Предходни свързани дела
            result.AddRange(await repo.AllReadonly<CaseMigration>()
                                      .Where(x => (result.Contains(x.CaseId)) && NomenclatureConstants.CaseMigrationTypes.CaseUnionConnection.Contains(x.CaseMigrationTypeId))
                                      .Select(x => x.PriorCaseId)
                                      .ToArrayAsync());

            //Последващи свързани дела
            result.AddRange(await repo.AllReadonly<CaseMigration>()
                                      .Where(x => result.Contains(x.PriorCaseId) && NomenclatureConstants.CaseMigrationTypes.CaseUnionConnection.Contains(x.CaseMigrationTypeId))
                                      .Select(x => x.CaseId)
                                      .ToArrayAsync());

            return result.Distinct().ToArray();
        }

        private void setAccessRightsForMoney(CurrentContextModel model, object id, string objectInfo = "")
        {
            //Да има Роля Счетоводство
            model.CanAccess = userContext.IsUserInRole(AccountConstants.Roles.MoneyAccount) ||
                                userContext.IsUserInRole(AccountConstants.Roles.DocumentEdit);
            model.CanChangeFull = userContext.IsUserInRole(AccountConstants.Roles.Supervisor);
        }
        private void setAccessRightsForDeliveryItem(CurrentContextModel model, DeliveryInfoVM info)
        {
            model.CanAccess = userContext.IsUserInRole(AccountConstants.Roles.DeliveryUser) ||
                userContext.IsUserInRole(AccountConstants.Roles.Supervisor) ||
                userContext.IsUserInRole(AccountConstants.Roles.Administrator);
            model.CanChange = userContext.IsUserInRole(AccountConstants.Roles.DeliveryUser) ||
                userContext.IsUserInRole(AccountConstants.Roles.Supervisor) ||
                userContext.IsUserInRole(AccountConstants.Roles.Administrator);
            model.CanChangeFull = userContext.IsUserInRole(AccountConstants.Roles.DeliveryUser) ||
                userContext.IsUserInRole(AccountConstants.Roles.Supervisor) ||
                userContext.IsUserInRole(AccountConstants.Roles.Administrator);

            model.Info.BaseObject = info.CaseInfo;
            model.Info.ObjectInfo = info.Info;
        }


        protected IQueryable<LawUnit> SelectLawUnit_ByTypes(int courtId, int[] lawUnitTypes, DateTime? dtNow = null, string selectMode = NomenclatureConstants.LawUnitSelectMode.Current)
        {
            dtNow = dtNow ?? DateTime.Now;
            if (courtId == 0)
            {
                courtId = userContext.CourtId;
            }
            Expression<Func<LawUnit, bool>> courtSearch = x => lawUnitTypes.Contains(x.LawUnitTypeId); ;
            Expression<Func<LawUnit, bool>> activeOnly = x => x.DateFrom <= dtNow && ((x.DateTo ?? DateTime.MaxValue) >= dtNow);
            if (courtId > 0)
            {
                switch (selectMode)
                {
                    case NomenclatureConstants.LawUnitSelectMode.Current:
                        courtSearch = x => x.Courts.Any(c =>
                          c.CourtId == courtId
                       && NomenclatureConstants.PeriodTypes.CurrentlyAvailableExtended.Contains(c.PeriodTypeId)
                       && lawUnitTypes.Contains(c.LawUnitTypeId ?? x.LawUnitTypeId)
                       && c.DateFrom <= dtNow && (((c.DateTo ?? DateTime.MaxValue) >= dtNow)));
                        break;

                    case NomenclatureConstants.LawUnitSelectMode.CurrentWithHistory:
                        courtSearch = x => x.Courts.Any(c =>
                          c.CourtId == courtId
                       && NomenclatureConstants.PeriodTypes.CurrentlyAvailableExtended.Contains(c.PeriodTypeId)
                       && lawUnitTypes.Contains(c.LawUnitTypeId ?? x.LawUnitTypeId)
                       && c.DateFrom <= dtNow);
                        break;
                    case NomenclatureConstants.LawUnitSelectMode.CurrentWithHistoryNoVAS:
                        courtSearch = x => x.Courts.Any(c =>
                          c.CourtId == courtId
                       && NomenclatureConstants.PeriodTypes.CurrentlyAvailableExtendedNoVas.Contains(c.PeriodTypeId)
                       && lawUnitTypes.Contains(c.LawUnitTypeId ?? x.LawUnitTypeId)
                       && c.DateFrom <= dtNow);
                        break;
                    case NomenclatureConstants.LawUnitSelectMode.AllWithHistoryNoVAS:
                        courtSearch = x => x.Courts.Any(c =>
                          c.CourtId == courtId
                       && NomenclatureConstants.PeriodTypes.CurrentlyAvailableExtendedNoVas.Contains(c.PeriodTypeId)
                       && lawUnitTypes.Contains(c.LawUnitTypeId ?? x.LawUnitTypeId)
                       && c.DateFrom <= dtNow);

                        activeOnly = x => true;
                        break;
                }
            }


            return repo.AllReadonly<LawUnit>()
                    .Where(courtSearch)
                    .Where(activeOnly)
                    .AsQueryable();
        }

        public SystemParam SystemParam_Select(string paramName)
        {
            return repo.GetById<SystemParam>(paramName);
        }

        public string SystemParam_SelectValue(string paramName)
        {
            return repo.AllReadonly<SystemParam>()
                            .Where(x => x.ParamName == paramName)
                            .Select(x => x.ParamValue)
                            .FirstOrDefault();
        }

        /// <summary>
        /// Връща стойност
        /// </summary>
        /// <param name="paramName">Име на параметър</param>
        /// <returns></returns>
        public async Task<string> SystemParamGetValue(string paramName)
        {
            return await repo.AllReadonly<SystemParam>()
                             .Where(x => x.ParamName == paramName)
                             .Select(x => x.ParamValue)
                             .FirstOrDefaultAsync();
        }

        public int[] SystemParam_SelectIntValues(string paramName)
        {
            string txtValue = SystemParam_SelectValue(paramName);
            if (string.IsNullOrEmpty(txtValue))
            {
                return new List<int>().ToArray();
            }

            return txtValue.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();
        }
        public string[] SystemParam_SelectStringValues(string paramName)
        {
            string txtValue = SystemParam_SelectValue(paramName);
            if (string.IsNullOrEmpty(txtValue))
            {
                return new List<string>().ToArray();
            }

            return txtValue.Split(',', StringSplitOptions.RemoveEmptyEntries).ToArray();
        }

        public string GetNomLabelById<T>(int id) where T : class, ICommonNomenclature
        {
            return GetPropById<T, string>(x => x.Id == id, x => x.Label);
        }

        public string GetNomCodeById<T>(int id) where T : class, ICommonNomenclature
        {
            return GetPropById<T, string>(x => x.Id == id, x => x.Code);
        }

        public async Task<bool> CheckCaseFeature(int caseId, string featureName)
        {
            var caseInfo = await repo.AllReadonly<Case>()
                                    .Where(x => x.Id == caseId)
                                    .Select(x => new CaseFeatureInfoVM
                                    {
                                        CourtTypeId = x.Court.CourtTypeId,
                                        CaseTypeId = x.CaseTypeId,
                                        CaseCodeId = x.CaseCodeId ?? 0
                                    }).FirstOrDefaultAsync().ConfigureAwait(false);

            return await CheckCaseFeature(caseInfo, featureName).ConfigureAwait(false);
        }

        public async Task<bool> CheckCaseFeature(CaseFeatureInfoVM caseInfo, string featureName)
        {
            if (caseInfo == null)
            {
                return false;
            }

            return await repo.AllReadonly<CaseFeature>()
                            .Where(x => x.Code == featureName)
                            .Where(x => x.AllCourtTypes || x.CourtTypes.Any(ct => ct.CourtTypeId == caseInfo.CourtTypeId))
                            .Where(x => x.AllCaseTypes || x.CaseTypes.Any(ct => ct.CaseTypeId == caseInfo.CaseTypeId))
                            .Where(x => x.AllCaseCodes || x.CaseCodes.Any(ct => ct.CaseCodeId == caseInfo.CaseCodeId))
                            .AnyAsync().ConfigureAwait(false);
        }

        public IDbContextTransaction BeginTransaction()
        {
            return repo.BeginTransaction();
        }

        public Task<string> GetIntegrationKey(int integrationType, int sourceType, long sourceId)
        {
            return repo.AllReadonly<IntegrationKey>()
                        .Where(x => x.IntegrationTypeId == integrationType && x.SourceType == sourceType && x.SourceId == sourceId)
                        .Select(x => x.OuterCode)
                        .FirstOrDefaultAsync();
        }


        protected string GetParamValue(string paramName, string defaultValue)
        {
            return repo.AllReadonly<Infrastructure.Data.Models.Nomenclatures.SystemParam>()
                       .Where(x => x.ParamName == NomenclatureConstants.SystemParamName.ZP_StartRegDate)
                       .Select(x => x.ParamValue)
                       .FirstOrDefault() ?? defaultValue;
        }

        private async Task<string> GetParamValueAsync(string paramName, string defaultValue)
        {
            return await repo.AllReadonly<Infrastructure.Data.Models.Nomenclatures.SystemParam>()
                       .Where(x => x.ParamName == NomenclatureConstants.SystemParamName.ZP_StartRegDate)
                       .Select(x => x.ParamValue)
                       .FirstOrDefaultAsync() ?? defaultValue;
        }
        public async Task<DateTime?> GetParamValueDate(string paramName, string defaultValue)
        {
            var newZPCaseRegDateFrom = await GetParamValueAsync(paramName, defaultValue);
            try
            {
                DateTime date;
                if (DateTime.TryParseExact(newZPCaseRegDateFrom, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                {
                    return date;
                }
            }
            catch (Exception)
            {

            }

            return null;

        }

        public async Task<bool> IsNewExecProcessCase(int? caseId)
        {
            if (!caseId.HasValue)
            {
                return false;
            }

            DateTime? ZPstartDate = await GetParamValueDate(NomenclatureConstants.SystemParamName.ZP_StartRegDate, "01.07.2025");
            if (!ZPstartDate.HasValue)
            {
                return false;
            }

            DateTime caseRegDate = await repo.GetPropByIdAsync<Case, DateTime>(x => x.Id == caseId.Value, x => x.RegDate);
            return caseRegDate > ZPstartDate;
        }
    }
}
