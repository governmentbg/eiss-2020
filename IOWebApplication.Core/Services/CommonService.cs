using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Models.BreadcrumbsModels;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Account;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using IOWebApplication.Infrastructure.Models.ViewModels.Identity;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using static IOWebApplication.Infrastructure.Constants.AccountConstants;

namespace IOWebApplication.Core.Services
{
    public class CommonService : BaseService, ICommonService
    {

        private readonly INomenclatureService nomService;
        private readonly IUrlHelper urlHelper;
        private readonly IDBUserContext dBUserContext;
        public CommonService(ILogger<CommonService> _logger,
                             IUserContext _userContext,
                             IDBUserContext dBUserContext,
                             IUrlHelper _url,
                             INomenclatureService _nomService,
                             IRepository _repo)
        {
            logger = _logger;
            userContext = _userContext;
            this.dBUserContext = dBUserContext;
            //mapper = _mapper;
            nomService = _nomService;
            urlHelper = _url;
            repo = _repo;
        }

        public IQueryable<InstitutionVM> Institution_Select(int institutionType, string name, int? id = null, string institutionTypeIds = null)
        {
            Expression<Func<Institution, bool>> whereInstitutionType = x => true;
            if (institutionType > 0)
                whereInstitutionType = x => x.InstitutionTypeId == institutionType;

            Expression<Func<Institution, bool>> whereInstitutionTypeIds = x => true;
            if (string.IsNullOrEmpty(institutionTypeIds) == false)
            {
                int[] intTypes = institutionTypeIds.Split(',').Select(x => int.Parse(x)).ToArray();
                whereInstitutionTypeIds = x => intTypes.Contains(x.InstitutionTypeId);
            }

            Expression<Func<Institution, bool>> whereSelect = x => true;
            if (!string.IsNullOrEmpty(name))
            {
                whereSelect = x => EF.Functions.ILike(x.FullName, name.ToPaternSearch());
            }
            Expression<Func<Institution, bool>> whereId = x => true;
            //Нарочно е оставено и 0 - така се изчиства контрола
            if (id >= 0)
            {
                whereId = x => x.Id == id;
                whereSelect = x => true;
                whereInstitutionType = x => true;
                whereInstitutionTypeIds = x => true;
            }

            if (institutionType < 1 &&
                string.IsNullOrEmpty(institutionTypeIds) &&
                string.IsNullOrEmpty(name) &&
                (id == null || id < 0))
            {
                whereSelect = x => false;
            }

            return repo.AllReadonly<Institution>()
                        .Where(whereId)
                        .Where(whereSelect)
                        .Where(whereInstitutionType)
                        .Where(whereInstitutionTypeIds)
                        .Select(x => new InstitutionVM
                        {
                            Id = x.Id,
                            FullName = x.FullName,
                            Code = x.Code,
                            DateFrom = x.DateFrom,
                            DateTo = x.DateTo,
                            EISPPCode = x.EISPPCode
                        })

                        .AsQueryable();
        }

        public bool Institution_SaveData(Institution model)
        {
            try
            {
                if (model.Id > 0)
                {
                    var saved = repo.GetById<Institution>(model.Id);
                    saved.CopyFrom(model);
                    saved.Code = model.Code;
                    saved.DepartmentName = model.DepartmentName;
                    saved.EISPPCode = model.EISPPCode;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo;
                    PersonNamesBase_SaveData(saved, true);
                    //repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    model.Person = null;
                    PersonNamesBase_SaveData(model, false);
                    repo.Add(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Institution Id={model.Id}");
                return false;
            }
        }

        public Person Person_FindByUic(string uic, int uicType)
        {
            return repo.AllReadonly<Person>(x => x.Uic == uic && x.UicTypeId == uicType).FirstOrDefault();
        }

        public IQueryable<LawUnitVM> LawUnit_Select(int lawUnitType, string name, DateTime? fromDate, DateTime? toDate, int specialityId, bool showFree, int? courtId)
        {
            DateTime dateTomorrow = DateTime.Now.AddDays(1).Date;

            Expression<Func<LawUnit, bool>> nameWhere = x => true;
            if (!string.IsNullOrEmpty(name))
            {
                nameWhere = x => EF.Functions.ILike(x.FullName, name.ToPaternSearch());
            }
            //nameWhere = x => x.FullName.Contains(name, StringComparison.InvariantCultureIgnoreCase);

            Expression<Func<LawUnit, bool>> specialityWhere = x => true;
            if (NomenclatureConstants.LawUnitTypes.HasSpecialities.Contains(lawUnitType) && specialityId > 0)
            {
                specialityWhere = x => repo.AllReadonly<LawUnitSpeciality>()
                                                  .Where(s => s.LawUnitId == x.Id && s.SpecialityId == specialityId && (s.DateTo ?? dateTomorrow).Date > DateTime.Now.Date)
                                                  .Any();
            }
            else if (NomenclatureConstants.LawUnitTypes.HasSpecialities.Contains(lawUnitType) && specialityId == 0) // Без чекната специалност
            {
                specialityWhere = x => repo.AllReadonly<LawUnitSpeciality>()
                                                  .Where(s => s.LawUnitId == x.Id && (s.DateTo ?? dateTomorrow).Date > DateTime.Now.Date)
                                                  .Any() == false;
            }
            DateTime dtNow = DateTime.Now;
            DateTime endDate = DateTime.Now.AddYears(100);
            DateTime fromDateNull = (fromDate == null ? DateTime.Now.AddYears(-100) : (DateTime)fromDate).Date;
            DateTime toDateNull = (toDate == null ? endDate : (DateTime)toDate).Date;
            Expression<Func<LawUnit, bool>> dateWhere = x => true;
            if (fromDate != null || toDate != null)
            {
                dateWhere = x => ((toDateNull >= x.DateFrom.Date && toDateNull <= (x.DateTo ?? endDate).Date) ||
                                 ((x.DateTo ?? endDate).Date >= fromDateNull && (x.DateTo ?? endDate).Date <= toDateNull));
            }
            Expression<Func<LawUnit, bool>> filterByCourtWhere = x => true;
            //Ако не е GlobalAdmin и 
            if (!userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator)
                        && NomenclatureConstants.LawUnitTypes.LocalViewOnly.Contains(lawUnitType))
            {
                if (showFree)
                {
                    filterByCourtWhere = x => x.Courts.Count == 0
                            || x.Courts.Where(c => c.CourtId == userContext.CourtId
                                && NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(c.PeriodTypeId)).Any();
                }
                else
                {
                    filterByCourtWhere = x => x.Courts.Where(c => c.CourtId == userContext.CourtId
                                && NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(c.PeriodTypeId)).Any();
                }
            }

            Expression<Func<LawUnit, bool>> whereCourt = x => true;
            if (courtId > 0)
            {
                whereCourt = x => x.Courts.Where(c => c.CourtId == courtId.Value)
                                          .Where(c => NomenclatureConstants.PeriodTypes.CurrentlyAvailableExtended.Contains(c.PeriodTypeId))
                                          .Where(c => c.DateFrom <= dtNow && (c.DateTo ?? DateTime.MaxValue) >= dtNow).Any();
            }
            var result = repo.AllReadonly<LawUnit>()
                    .Where(x => x.LawUnitTypeId == lawUnitType)
                    .Where(nameWhere)
                    .Where(dateWhere)
                    .Where(specialityWhere)
                    .Where(filterByCourtWhere)
                    .Where(whereCourt)
                    .Select(x => new LawUnitVM
                    {
                        Id = x.Id,
                        FullName = x.FullName,
                        Department = x.Department,
                        DateFrom = x.DateFrom,
                        DateTo = x.DateTo,
                        CourtList = x.Courts
                            .Where(x => NomenclatureConstants.PeriodTypes.CurrentlyAvailableExtended.Contains(x.PeriodTypeId))
                            .Where(x => x.DateFrom <= dtNow && (x.DateTo ?? DateTime.MaxValue) >= dtNow)
                            .Select(x => $"{x.Court.Label} ({x.PeriodType.Code}{((x.PeriodTypeId == NomenclatureConstants.PeriodTypes.ActAs) ? " " + x.LawUnitType.Label : "")})")
                            .ToArray()
                    })
                    .AsQueryable();

            //string sql = result.ToSql();
            return result;
        }

        public bool LawUnit_SaveData(LawUnit model)
        {
            try
            {
                if (model.Id > 0)
                {
                    var saved = repo.GetById<LawUnit>(model.Id);
                    saved.Code = model.Code;
                    saved.Department = model.Department;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo;
                    saved.JudgeSeniorityId = model.JudgeSeniorityId;
                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;
                    saved.CopyFrom(model);
                    PersonNamesBase_SaveData(saved, true);
                    CreateHistory<LawUnit, LawUnitH>(saved);
                    repo.SaveChanges();
                }
                else
                {
                    model.UserId = userContext.UserId;
                    model.DateWrt = DateTime.Now;
                    PersonNamesBase_SaveData(model, false);
                    CreateHistory<LawUnit, LawUnitH>(model);
                    repo.Add(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Institution Id={model.Id}");
                return false;
            }
        }

        public SaveResultVM LawUnit_ChangeLawunitType(int id, int lawunitTypeId)
        {
            try
            {
                var saved = repo.GetById<LawUnit>(id);
                saved.LawUnitTypeId = lawunitTypeId;
                saved.DateWrt = DateTime.Now;
                saved.UserId = userContext.UserId;
                CreateHistory<LawUnit, LawUnitH>(saved);
                repo.SaveChanges();
                return new SaveResultVM(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на ChangeLawunitType Id={id}");
                return new SaveResultVM(false);
            }
        }

        public IEnumerable<LabelValueVM> GetLawUnitName_Uic(int lawUnitType, string name, string uic, int courtId, string lawUnitTypes = null)
        {
            name = name?.ToLower();
            uic = uic?.ToLower();

            Expression<Func<LawUnit, bool>> lawUnitTypeSearch = x => true;
            if (!string.IsNullOrEmpty(lawUnitTypes))
            {
                int[] intTypes = lawUnitTypes.Split(',').Select(x => int.Parse(x)).ToArray();
                lawUnitTypeSearch = x => intTypes.Contains(x.LawUnitTypeId);
            }
            else
            {
                lawUnitTypeSearch = x => x.LawUnitTypeId == lawUnitType;
            }
            Expression<Func<LawUnit, bool>> lawUnitFilter = x => true;
            if (!string.IsNullOrEmpty(uic))
            {
                lawUnitFilter = x => x.Uic.ToLower() == uic || x.Code == uic;
            }
            if (!string.IsNullOrEmpty(name))
            {
                lawUnitFilter = x => EF.Functions.ILike(x.FullName, name.ToPaternSearch());
            }
            Expression<Func<LawUnit, bool>> lawUnitCourtSearch = x => true;
            if (courtId == 0)
            {
                courtId = userContext.CourtId;
            }
            if (courtId > 0)
            {
                List<int> plus = NomenclatureConstants.PeriodTypes.CurrentlyAvailable.ToList();
                DateTime dateSelect = DateTime.Now;
                DateTime enddatenull = dateSelect.AddDays(1);
                lawUnitCourtSearch = x => repo.AllReadonly<CourtLawUnit>().Any(d => d.LawUnitId == x.Id && d.DateExpired == null) ? repo.AllReadonly<CourtLawUnit>().Where(c => c.LawUnitId == x.Id && c.CourtId == courtId && c.DateExpired == null &&
                                                          plus.Contains(c.PeriodTypeId) && c.DateFrom <= dateSelect && (c.DateTo ?? enddatenull) >= dateSelect).Any() : true;
            }

            var result = repo.AllReadonly<LawUnit>()
                            .Where(lawUnitTypeSearch)
                            .Where(lawUnitFilter)
                            .Where(lawUnitCourtSearch)
                            .Where(x => DateTime.Now.Date <= (x.DateTo ?? DateTime.Now).Date)
                            .OrderBy(x => x.FullName)
                            .Select(x => new LabelValueVM
                            {
                                Value = x.Id.ToString(),
                                Label = $"{x.FullName} {x.Code} ({x.LawUnitType.Label})"
                            }).ToList();

            return result;
        }

        private IEnumerable<LabelValueVM> GetLawUnitName_UicNew(int lawUnitType, string name, string uic, int courtId, string lawUnitTypes = null, string selectmode = NomenclatureConstants.LawUnitSelectMode.All)
        {
            name = name?.ToLower();
            uic = uic?.ToLower();
            int[] intTypes;

            if (!string.IsNullOrEmpty(lawUnitTypes))
            {
                intTypes = lawUnitTypes.Split(',').Select(x => int.Parse(x)).ToArray();
            }
            else
            {
                intTypes = new List<int> { lawUnitType }.ToArray();
            }
            Expression<Func<LawUnit, bool>> lawUnitFilter = x => true;
            if (!string.IsNullOrEmpty(uic))
            {
                lawUnitFilter = x => x.Uic.ToLower() == uic || x.Code == uic;
            }
            if (!string.IsNullOrEmpty(name))
            {
                lawUnitFilter = x => EF.Functions.ILike(x.FullName, name.ToPaternSearch());
            }
            if (courtId == 0)
            {
                courtId = userContext.CourtId;
            }

            if (intTypes.Any(x => !NomenclatureConstants.LawUnitTypes.EissUserTypes.Contains(x)))
            {
                selectmode = NomenclatureConstants.LawUnitSelectMode.All;
            }

            IQueryable<LawUnit> data = SelectLawUnit_ByTypes(courtId, intTypes, null, selectmode); ;
            //switch (selectmode)
            //{
            //    case NomenclatureConstants.LawUnitSelectMode.Current:
            //        data = SelectLawUnit_ByTypes(courtId, intTypes, null, true);
            //        break;
            //    case NomenclatureConstants.LawUnitSelectMode.All:
            //        data = SelectLawUnit_ByTypes(courtId, intTypes, null, false);
            //        break;
            //    default:
            //        break;
            //}

            var result = data
                            .Where(lawUnitFilter)
                            .OrderBy(x => x.FullName)
                            .Select(x => new LabelValueVM
                            {
                                Value = x.Id.ToString(),
                                Label = $"{x.FullName} {x.Code} ({x.LawUnitType.Label}){((x.DateTo != null) ? " *Н" : "")}"
                            }).ToList();

            return result;
        }

        public IEnumerable<LabelValueVM> GetLawUnitAutoComplete(int lawUnitType, string lawUnitTypes, string query, int courtId, string selectmode = NomenclatureConstants.LawUnitSelectMode.All)
        {
            query = query?.ToLower();
            var result = GetLawUnitName_UicNew(lawUnitType, null, query, courtId, lawUnitTypes, selectmode);

            if (!result.Any())
                result = GetLawUnitName_UicNew(lawUnitType, query, null, courtId, lawUnitTypes, selectmode);

            return result;
        }

        public LabelValueVM GetLawUnitById(int id)
        {
            return repo.AllReadonly<LawUnit>()
                        .Where(x => x.Id == id)
                        .OrderBy(x => x.FullName)
                        .Select(x => new LabelValueVM
                        {
                            Value = x.Id.ToString(),
                            Label = $"{x.FullName} {x.Code} ({x.LawUnitType.Label})"
                        }).FirstOrDefault();
        }


        public IQueryable<LawUnit> LawUnit_JudgeByCourtDate(int court, DateTime? date)
        {
            DateTime dateSelect = date ?? DateTime.Now;
            DateTime enddatenull = dateSelect.AddDays(1);

            List<int> plus = NomenclatureConstants.PeriodTypes.CurrentlyAvailable.ToList();
            return repo.AllReadonly<LawUnit>().Where(x => x.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Judge && ((x.DateTo ?? dateSelect.Date) >= dateSelect.Date)
                                   && repo.AllReadonly<CourtLawUnit>().Where(c => c.LawUnitId == x.Id && c.CourtId == court && c.DateExpired == null &&
                                          plus.Contains(c.PeriodTypeId) && c.DateFrom <= dateSelect && (c.DateTo ?? enddatenull) >= dateSelect).Any()
                                                          ).AsQueryable();
        }


        public IQueryable<LawUnit> LawUnit_JudgeAndUserByCourtDate(int court, DateTime? date)
        {
            DateTime dateSelect = date ?? DateTime.Now;
            DateTime enddatenull = dateSelect.AddDays(1);

            List<int> plus = NomenclatureConstants.PeriodTypes.CurrentlyAvailable.ToList();
            return repo.AllReadonly<LawUnit>().Where(x => NomenclatureConstants.LawUnitTypes.SpecialAccess.Contains(x.LawUnitTypeId) && ((x.DateTo ?? dateSelect.Date) >= dateSelect.Date)
                                   && repo.AllReadonly<CourtLawUnit>().Where(c => c.LawUnitId == x.Id && c.CourtId == court && c.DateExpired == null &&
                                          plus.Contains(c.PeriodTypeId) && c.DateFrom <= dateSelect && (c.DateTo ?? enddatenull) >= dateSelect).Any()
                                                          ).AsQueryable();
        }



        #region "WorkingDays"


        /// <summary>
        /// Връща
        /// </summary>
        /// <param name="dateFrom">От дата as Datetime</param>
        /// <param name="dateTo">До дата as Datetime</param>
        /// <param name="dayType">Вид работен ден</param>
        /// <returns></returns>
        public IQueryable<WorkingDaysVM> WorkingDay_GetList(DateTime? dateFrom, DateTime? dateTo, int dayType = 0)
        {
            return repo.AllReadonly<WorkingDay>()
                       .Where(s => s.DayTypeId == dayType || dayType == 0)
                       .Where(s => s.Day >= dateFrom || dateFrom == DateTime.MinValue)
                       .Where(s => s.Day <= dateTo || dateTo == DateTime.MinValue)
                       .Select(sel => new WorkingDaysVM
                       {
                           Id = sel.Id,
                           CourtId = sel.CourtId,
                           Day = sel.Day,
                           CourtName = sel.CourtId == null ? "Всички" : sel.Court.Label,
                           DayType = sel.DayTypeId,
                           DayTypeName = sel.DayType.Label,
                           Description = sel.Description ?? String.Empty // За да не гърми DataTables при търсене
                       });

        }



        /// <summary>
        /// Записва/Променя данните за работен ден
        /// </summary>
        /// <param name="model">Модел as WorkingDays</param>
        /// <returns> >0 - Успешен запис/промяна; по малко от 0 - Неуспешен запис/редакция</returns>
        public int WorkingDay_SaveData(WorkingDay model)
        {
            int res = -1;

            // ReMap на CourtId
            model.CourtId = model.CourtId <= 0 ? null : model.CourtId;

            try
            {
                if (model.Id > 0) // update
                {
                    repo.Update<WorkingDay>(model);
                }
                else // insert new record
                {
                    repo.Add<WorkingDay>(model);
                }
                model.UserId = userContext.UserId;
                model.DateWrt = DateTime.Now;
                repo.SaveChanges();
                res = model.Id;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message, ex);
                //throw new Exception("IOWebApplication.Core.Services->SaveWorkingDay", ex);
            }
            return res;
        }



        /// <summary>
        /// Изтрива работен ден
        /// </summary>
        /// <param name="Id">Идентификатор as int</param>
        /// <returns>True - Успешно изтриване; False - НЕУСПЕШНО изтриване</returns>
        public bool WorkingDay_Delete(int Id)
        {
            bool res = false;
            try
            {
                //Delete Record By ID
                repo.Delete<WorkingDay>(Id);
                repo.SaveChanges();
                res = true;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message, ex);
                //throw new Exception("IOWebApplication.Core.Services->DeleteWorkingDay", ex);
            }
            return res;
        }


        /// <summary>
        /// Проверка за съществуващ запис за Работен ден
        /// </summary>
        /// <param name="Day">Дата as DateTime</param>
        /// <param name="Id">ID на записа as int</param>
        /// <param name="CourtId">ID на съд as int?</param>
        /// <returns>True-Съществува, False-НЕ Съществува</returns>
        public bool WorkingDay_IsExist(DateTime Day, int Id, int? CourtId)
        {
            // ReMap на CourtId
            int? Court_Id = CourtId <= 0 ? null : CourtId;

            return repo.AllReadonly<WorkingDay>().Where(s => s.Id != Id)
                                                 .Where(s => s.CourtId == Court_Id)
                                                 .Where(s => s.Day == Day)
                                                 .Any();
        }

        /// <summary>
        /// Метод връщащ работен ден от дата
        /// </summary>
        /// <param name="date"></param>
        /// <param name="workingDaysDayOff">Списък с почивни дни</param>
        /// <returns></returns>
        public async Task<DateTime> GetWorkDayFromDate(DateTime date, IEnumerable<WorkingDay> workingDaysDayOff = null)
        {
            IEnumerable<WorkingDay> workingDays = null;

            if (workingDaysDayOff != null)
                workingDays = workingDaysDayOff.Where(x => x.Day.Date >= date.Date);
            else
                workingDays = await repo.AllReadonly<WorkingDay>()
                                        .Where(x => x.CourtId == userContext.CourtId &&
                                                    x.Day.Date >= date.Date &&
                                                    x.DayTypeId == NomenclatureConstants.DayTypeConstants.DayOff)
                                        .ToListAsync()
                                        .ConfigureAwait(false);

            while (workingDays.Any(w => w.Day.Date == date.Date))
                date = date.AddDays(1);

            return date;
        }

        /// <summary>
        /// Метод връщащ месеци между 2 дати за актовете
        /// </summary>
        /// <param name="dateFrom">От дата</param>
        /// <param name="dateTo">До дата</param>
        /// <param name="workingDaysDayOff">Списък с почивни дни</param>
        /// <returns></returns>
        public async Task<int> GetMonthsBetweenTwoDatesForActs(DateTime dateFrom, DateTime dateTo, IEnumerable<WorkingDay> workingDaysDayOff = null)
        {
            DateTime dateToCalc = await GetWorkDayFromDate(dateTo, workingDaysDayOff).ConfigureAwait(false);
            int monthsApart = (12 * (dateToCalc.Year - dateFrom.Year) + dateToCalc.Month - dateFrom.Month) + (dateToCalc.Day > dateFrom.Day ? 1 : 0);
            monthsApart = monthsApart == 0 ? 1 : monthsApart;
            return Math.Abs(monthsApart);
        }

        /// <summary>
        /// Пакетно сетване на срок в който е постановен акта
        /// </summary>
        /// <param name="takeRow">Брой записи които да вземе</param>
        /// <returns></returns>
        public async Task<bool> SetDeclaredMonthCountForActs(int takeRow)
        {
            try
            {
                DateTime dateNow = DateTime.Now;

                List<ActDeclaredMonthVM> caseSessionActs = await repo.AllReadonly<CaseSessionAct>()
                                                                     .Where(x => x.DateExpired == null &&
                                                                                 x.ActDeclaredDate != null &&
                                                                                 x.DeclaredMonthCount == null)
                                                                     .OrderByDescending(x => x.Id)
                                                                     .Select(x => new ActDeclaredMonthVM()
                                                                     {
                                                                         CourtId = x.CourtId ?? 0,
                                                                         ActId = x.Id,
                                                                         ActDeclaredDate = x.ActDeclaredDate ?? dateNow,
                                                                         SessionDateFrom = x.CaseSession.DateFrom
                                                                     })
                                                                     .Take(takeRow)
                                                                     .ToListAsync()
                                                                     .ConfigureAwait(false);

                if (caseSessionActs.Count == 0)
                {
                    return false;
                }

                IEnumerable<WorkingDay> workingDaysDayOff = await repo.AllReadonly<WorkingDay>()
                                                                      .Where(x => x.DayTypeId == NomenclatureConstants.DayTypeConstants.DayOff)
                                                                      .ToListAsync()
                                                                      .ConfigureAwait(false);


                foreach (var act in caseSessionActs)
                {
                    CaseSessionAct modelSave = await repo.All<CaseSessionAct>()
                                                         .Where(x => x.Id == act.ActId)
                                                         .FirstOrDefaultAsync()
                                                         .ConfigureAwait(false);

                    modelSave.DeclaredMonthCount = await GetMonthsBetweenTwoDatesForActs(act.SessionDateFrom, act.ActDeclaredDate, workingDaysDayOff.Where(x => x.CourtId == act.CourtId)).ConfigureAwait(false);
                }

                await repo.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при пакетно сетване на срок в който е постановен акта");
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Извличане на данни за потребители
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <param name="forList"></param>
        /// <returns></returns>
        public IQueryable<UserProfileVM> Users_Select(UserFilterVM filter, bool forList = false)
        {
            filter.UpdateNullables();
            Expression<Func<ApplicationUser, bool>> whereCourt = x => x.LawUnit.Courts.Any(c => c.CourtId == userContext.CourtId && c.DateFrom <= DateTime.Now && (c.DateTo ?? DateTime.MaxValue) >= DateTime.Now && NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(c.PeriodTypeId));
            if (forList && userContext.IsUserInRole(Roles.GlobalAdministrator))
                whereCourt = x => true;

            Expression<Func<ApplicationUser, bool>> activeUserOnly = x => true;
            if (!forList)
                activeUserOnly = x => x.IsActive;

            Expression<Func<ApplicationUser, bool>> userRolesWhere = x => true;
            if (filter.UserRoles != null && filter.UserRoles.Any())
                userRolesWhere = x => x.UserRoles.Any(r => filter.UserRoles.Contains(r.RoleId));

            return repo.AllReadonly<ApplicationUser>()
                       .Where(whereCourt)
                       .Where(activeUserOnly)
                       .Where(userRolesWhere)
                       .Where(x => EF.Functions.ILike(x.LawUnit.FullName, filter.FullName.ToPaternSearch()))
                       .Where(x => EF.Functions.ILike(x.Email, filter.Email.ToPaternSearch()))
                       .Where(x => x.Id == (filter.UserId ?? x.Id))
                       .Select(x => new UserProfileVM
                       {
                           Id = x.Id,
                           CourtId = x.CourtId,
                           CourtName = x.Court.Label,
                           Email = x.Email,
                           Uic = x.LawUnit.Uic,
                           FullName = x.LawUnit.FullName,
                           LawUnitId = x.LawUnitId,
                           LawUnitTypeName = x.LawUnit.LawUnitType.Label,
                           IsActive = x.IsActive,
                           RolesLabel = string.Join(", ", x.UserRoles.Select(r => r.Role.Label))
                       })
                       .AsQueryable();
        }

        public IQueryable<UserProfileVM> Users_SelectForAutocomplete(UserFilterVM filter, string selectMode = NomenclatureConstants.LawUnitSelectMode.Current)
        {
            filter.UpdateNullables();
            Expression<Func<ApplicationUser, bool>> whereFilter = x => x.IsActive && x.LawUnit.Courts.Any(c => c.CourtId == userContext.CourtId && c.DateFrom <= DateTime.Now && (c.DateTo ?? DateTime.MaxValue) >= DateTime.Now && NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(c.PeriodTypeId));

            switch (selectMode)
            {
                case NomenclatureConstants.LawUnitSelectMode.CurrentWithHistory:
                    whereFilter = x => x.LawUnit.Courts.Any(c => c.CourtId == userContext.CourtId && c.DateFrom <= DateTime.Now && NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(c.PeriodTypeId));
                    break;
            }

            if (!string.IsNullOrEmpty(filter.UserId))
            {
                whereFilter = x => x.Id == filter.UserId;
            }

            return repo.AllReadonly<ApplicationUser>()
                            .Where(whereFilter)
                            .Where(x => EF.Functions.ILike(x.LawUnit.FullName, filter.FullName.ToPaternSearch()))
                            .Where(x => EF.Functions.ILike(x.Email, filter.Email.ToPaternSearch()))
                            .Where(x => x.Id == (filter.UserId ?? x.Id))
                            .Select(x => new UserProfileVM
                            {
                                Id = x.Id,
                                Email = x.Email,
                                FullName = x.LawUnit.FullName,
                                LawUnitTypeName = x.LawUnit.LawUnitType.Label,
                                IsActive = x.IsActive && x.LawUnit.Courts.Any(c => c.CourtId == userContext.CourtId && c.DateFrom <= DateTime.Now && (c.DateTo ?? DateTime.MaxValue) >= DateTime.Now && NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(c.PeriodTypeId))
                            }).AsQueryable();

        }

        public async Task<List<SelectListItem>> GetDropDownList_CourtHall(int courtId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = (await repo.AllReadonly<CourtHall>()
                                   .Where(x => x.CourtId == courtId)
                                   .Select(x => new SelectListItem()
                                   {
                                       Text = x.Name + (!string.IsNullOrEmpty(x.Location) ? "  " + x.Location : string.Empty),
                                       Value = x.Id.ToString()
                                   })
                                   .ToListAsync()) ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        private IList<CheckListVM> FillCheckListVMs(int lawUnitId)
        {
            IList<CheckListVM> checkListVMs = new List<CheckListVM>();

            DateTime dateTomorrow = DateTime.Now.AddDays(1).Date;
            //var speciality = nomService.GetDropDownList<Speciality>(false, false);
            var lawUnit = repo.GetById<LawUnit>(lawUnitId);
            var speciality = nomService.GetDDL_Specyality_ByLowUnit_Type(lawUnit.LawUnitTypeId, false, false);

            var lawUnitSpeciality = repo.AllReadonly<LawUnitSpeciality>().Where(x => x.LawUnitId == lawUnitId &&
                                                         (x.DateTo ?? dateTomorrow).Date > DateTime.Now.Date).ToList();

            foreach (var nom in speciality)
            {
                var checkItem = new CheckListVM();
                checkItem.Value = nom.Value;
                checkItem.Label = nom.Text;
                int id = int.Parse(nom.Value);
                checkItem.Checked = lawUnitSpeciality.Where(x => x.SpecialityId == id).Any();
                checkListVMs.Add(checkItem);
            }

            return checkListVMs;
        }

        public CheckListViewVM LawUnitSpeciality_SelectForCheck(int lawUnitId)
        {
            CheckListViewVM checkListViewVM = new CheckListViewVM
            {
                CourtId = lawUnitId,
                ObjectId = lawUnitId,
                Label = "Специалности",
                checkListVMs = FillCheckListVMs(lawUnitId)
            };

            return checkListViewVM;
        }

        public bool LawUnitSpeciality_SaveData(CheckListViewVM model)
        {
            try
            {
                DateTime dateTomorrow = DateTime.Now.AddDays(1).Date;
                DateTime fromDate = DateTime.Now;
                DateTime toDate = DateTime.Now.AddSeconds(-1);

                var expiryList = repo.All<LawUnitSpeciality>()
                    .Where(x => x.LawUnitId == model.CourtId && (x.DateTo ?? dateTomorrow).Date > DateTime.Now.Date)
                    .ToList();
                foreach (var item in expiryList)
                {
                    item.DateTo = toDate;
                }
                foreach (var speciality in model.checkListVMs)
                {
                    if (speciality.Checked == false) continue;
                    LawUnitSpeciality newLawUnitspeciality = new LawUnitSpeciality();
                    newLawUnitspeciality.LawUnitId = model.CourtId;
                    newLawUnitspeciality.SpecialityId = int.Parse(speciality.Value);
                    newLawUnitspeciality.DateFrom = fromDate;
                    repo.Add<LawUnitSpeciality>(newLawUnitspeciality);
                }
                repo.SaveChanges();
                return true;
            }
            catch
            {
                //logger.log(ex)
                return false;
            }
        }

        public IEnumerable<LabelValueVM> CourtSelect_ByUser(string userId)
        {
            var user = repo.AllReadonly<ApplicationUser>()
                                .Include(x => x.LawUnit)
                                .Include(x => x.Court)
                                .Where(x => x.Id == userId)
                                .FirstOrDefault();

            List<LabelValueVM> result = new List<LabelValueVM>()
            {
                new LabelValueVM() { Value = user.Court.Id.ToString(), Label = user.Court.Label }
            };



            if (userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator))
            {
                result.AddRange(repo.AllReadonly<Court>()
                        .Include(x => x.CourtType)
                        .Where(x => x.Id != user.CourtId && x.IsActive)
                        .OrderBy(x => x.Label)
                        .Select(x => new LabelValueVM
                        {
                            Value = x.Id.ToString(),
                            Label = x.Label,
                            Description = x.CourtType.Label
                        }));
            }
            else
            {
                var userCourts = repo.AllReadonly<CourtLawUnit>()
                        .Include(x => x.Court)
                        .ThenInclude(x => x.CourtType)
                        .Where(x => x.CourtId != user.CourtId && x.LawUnitId == user.LawUnitId && x.DateExpired == null
                            && NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(x.PeriodTypeId))
                        .Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.MaxValue).Date >= DateTime.Now.Date)
                        .Select(x => new
                        {
                            Id = x.CourtId,
                            Label = x.Court.Label,
                            CourtTypeName = x.Court.CourtType.Label
                        })
                        .Distinct()
                        .OrderBy(x => x.Label)
                        .Select(x => new LabelValueVM
                        {
                            Value = x.Id.ToString(),
                            Label = x.Label,
                            Description = x.CourtTypeName
                        });
                if (userCourts != null && userCourts.Any())
                {
                    result.AddRange(userCourts);
                }
            }

            return result;
        }

        public IQueryable<LawUnit> LawUnit_ByCourtDate(int court, DateTime? date, int organizationId)
        {
            DateTime dateSelect = date ?? DateTime.Now;
            DateTime enddatenull = dateSelect.AddDays(1);

            List<int> plus = NomenclatureConstants.PeriodTypes.CurrentlyAvailable.ToList();
            //трябва да се направи константа за съдия и да се замени тук
            return repo.AllReadonly<LawUnit>().Where(x => ((x.DateTo ?? dateSelect.Date) >= dateSelect.Date)
                                   && repo.AllReadonly<CourtLawUnit>().Where(c => c.LawUnitId == x.Id && c.CourtId == court && c.DateExpired == null &&
                                          c.CourtOrganizationId == organizationId &&
                                          plus.Contains(c.PeriodTypeId) && c.DateFrom <= dateSelect && (c.DateTo ?? enddatenull) >= dateSelect).Any()
                                                          ).AsQueryable();
        }
        public List<SelectListItem> CourtForDelivery_SelectDDL(int courtId)
        {
            var result = repo.AllReadonly<Court>()
                       .Where(x => x.Id != courtId && x.IsActive)
                       .OrderBy(x => x.Label)
                       .Select(x => new SelectListItem()
                       {
                           Text = x.Label,
                           Value = x.Id.ToString()
                       }).ToList() ?? new List<SelectListItem>();
            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }

        public async Task<List<SelectListItem>> CourtForDelivery_SelectDDLAsync(int courtId)
        {
            var result = await repo.AllReadonly<Court>()
                             .Where(x => x.Id != courtId && x.IsActive)
                             .OrderBy(x => x.Label)
                             .Select(x => new SelectListItem()
                             {
                                 Text = x.Label,
                                 Value = x.Id.ToString()
                             }).ToListAsync() ?? new List<SelectListItem>();
            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }

        public IQueryable<CourtBankAccount> CourtBankAccountForCourt_Select(int courtId)
        {
            return repo.AllReadonly<CourtBankAccount>()
                .Include(x => x.MoneyGroup)
                        .Where(x => x.CourtId == courtId)
                        .AsQueryable();
        }

        public IQueryable<CourtBankAccountVM> CourtBankAccount_Select(int courtId)
        {
            return repo.AllReadonly<CourtBankAccount>()
                .Include(x => x.MoneyGroup)
                            .Where(x => x.CourtId == courtId)
                            .Select(x => new CourtBankAccountVM
                            {
                                Id = x.Id,
                                Label = x.Label,
                                Iban = x.Iban,
                                MoneyGroupName = x.MoneyGroup.Label,
                                IsActive = x.IsActive,
                                ComPortPos = x.ComPortPos
                            }).AsQueryable();
        }

        public bool CourtBankAccount_SaveData(CourtBankAccount model)
        {
            try
            {
                if (model.Id > 0)
                {
                    var saved = repo.GetById<CourtBankAccount>(model.Id);
                    saved.Label = model.Label;
                    saved.Iban = model.Iban;
                    saved.MoneyGroupId = model.MoneyGroupId;
                    saved.IsActive = model.IsActive;
                    saved.ComPortPos = model.ComPortPos;
                    repo.SaveChanges();
                }
                else
                {
                    model.DateStart = DateTime.Now;
                    repo.Add(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CourtBankAccount Id={model.Id}");
                return false;
            }
        }

        public List<SelectListItem> BankAccount_SelectDDL(int courtId, int moneyGroupId, bool addDefaultElement = false, bool addAllElement = false)
        {
            DateTime today = DateTime.Now.Date;
            var result = repo.AllReadonly<CourtBankAccount>()
                       .Where(x => x.CourtId == courtId && x.IsActive == true)
                       .Where(x => (x.MoneyGroupId == moneyGroupId || moneyGroupId == 0))
                       .Where(x => x.DateStart.Date <= today.Date && today.Date <= (x.DateEnd ?? DateTime.Now).Date)
                       .Select(x => new SelectListItem()
                       {
                           Text = x.MoneyGroup.Label + " - " + x.Label + "(" + x.Iban + ")",
                           Value = x.Id.ToString()
                       }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
                result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            if (addAllElement)
                result.Insert(0, new SelectListItem() { Text = "Всички", Value = "-2" });

            return result;
        }

        public List<SelectListItem> COMPort()
        {
            var list = new List<SelectListItem>();
            for (int i = 1; i < 17; i++)
            {
                list.Add(new SelectListItem() { Text = "COM" + i.ToString(), Value = "COM" + i.ToString() });
            }
            list.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return list;
        }

        public List<SelectListItem> SelectEntity_LawUnitTypes()
        {
            return repo.AllReadonly<LawUnitType>()
                            .Where(x => NomenclatureConstants.LawUnitTypes.CasePersonSelectables.Contains(x.Id))
                            .ToSelectList();
        }

        public IQueryable<SelectEntityItemVM> SelectEntity_Select(int sourceType, string search, int? objectTypeId = null, long? sourceId = null)
        {
            IEnumerable<SelectEntityItemVM> result = null;
            var dtNow = DateTime.Now;
            int? searchId = null;
            if (sourceId > 0)
            {
                searchId = (int)sourceId;
            }
            switch (sourceType)
            {
                case SourceTypeSelectVM.Court:
                    result = repo.AllReadonly<Court>()
                                .Where(x => x.IsActive)
                                .Where(x => EF.Functions.ILike(x.Label, search.ToPaternSearch()))
                                .Where(x => x.Id == (searchId ?? x.Id))
                                .OrderBy(x => x.Label)
                                .Select(x => new SelectEntityItemVM()
                                {
                                    SourceType = sourceType,
                                    SourceId = x.Id,
                                    Label = x.Label,
                                    UicTypeId = NomenclatureConstants.UicTypes.Bulstat
                                });

                    break;
                case SourceTypeSelectVM.Instutution:
                    Expression<Func<Institution, bool>> whereActive = x => true;
                    if ((searchId ?? 0) <= 0)
                        whereActive = x => (x.DateTo ?? dtNow) >= dtNow;

                    result = repo.AllReadonly<Institution>()
                                .Where(x => x.InstitutionTypeId == (objectTypeId ?? x.InstitutionTypeId))
                                .Where(x => EF.Functions.ILike(x.FullName, search.ToPaternSearch())
                                || EF.Functions.ILike(x.Code, search.ToPaternSearch())
                                || EF.Functions.ILike(x.DepartmentName, search.ToPaternSearch())
                                )
                                .Where(x => x.Id == (searchId ?? x.Id))
                                .Where(whereActive)
                                .OrderBy(x => x.FullName)
                                .Select(x => new SelectEntityItemVM()
                                {
                                    SourceType = sourceType,
                                    SourceId = x.Id,
                                    ObjectTypeName = x.InstitutionType.Label,
                                    Label = x.FullName + (x.Code != null ? $" ({x.Code} {x.DepartmentName})" : ""),
                                    UicTypeId = x.UicTypeId,
                                    Uic = x.Uic
                                });

                    break;
                case SourceTypeSelectVM.LawUnit:
                    result = repo.AllReadonly<LawUnit>()
                                .Where(x => x.LawUnitTypeId == (objectTypeId ?? x.LawUnitTypeId))
                                .Where(x => (x.DateTo ?? dtNow) >= dtNow)
                                .Where(x => EF.Functions.ILike(x.FullName, search.ToPaternSearch())
                                || EF.Functions.ILike(x.Code, search.ToPaternSearch())
                                || EF.Functions.ILike(x.Department, search.ToPaternSearch())
                                )
                                .Where(x => x.Id == (searchId ?? x.Id))
                                .OrderBy(x => x.FullName)
                                .Select(x => new SelectEntityItemVM()
                                {
                                    SourceType = sourceType,
                                    SourceId = x.Id,
                                    ObjectTypeName = x.LawUnitType.Label,
                                    //Само за прокурори се крие служебния им номер след избор
                                    Label = x.FullName + (x.Code != null ? $" ({((x.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Prosecutor) ? "" : x.Code + " ")}{x.Department})" : ""),
                                    LabelFull = x.FullName + (x.Code != null ? $" ({x.Code} {x.Department})" : ""),
                                    UicTypeId = x.UicTypeId,
                                    Uic = x.Uic
                                });

                    break;
                default:
                    break;
            }
            return result.AsQueryable();
            //return result.Where(x =>
            //                    EF.Functions.ILike(x.LabelFull, search.ToPaternSearch())
            //                    //(x.Label ?? (search ?? "")).Contains(search ?? (x.Label ?? ""), StringComparison.InvariantCultureIgnoreCase)
            //                    && x.SourceId == (sourceId ?? x.SourceId)
            //                    ).AsQueryable();
        }

        public async Task<string> Users_GetByLawUnitUIC(string uic)
        {
            DateTime dtNow = DateTime.Now;
            var userInfo = await repo.AllReadonly<ApplicationUser>()
                                .Where(x => x.LawUnit.Uic == uic)
                                .Where(x => (x.LawUnit.DateTo ?? dtNow) >= (dtNow))
                                .Where(x => x.IsActive)
                                .Select(x => new
                                {
                                    x.Id,
                                    x.LawUnitId,
                                    x.CourtId,
                                    IsGlobalAdmin = x.UserRoles.Any(r => r.Role.Name == AccountConstants.Roles.GlobalAdministrator)
                                })
                                .FirstOrDefaultAsync();
            if (userInfo == null)
            {
                return null;
            }

            int[] courtListIds = await repo.AllReadonly<CourtLawUnit>()
                                        .Where(x => x.LawUnitId == userInfo.LawUnitId && x.DateExpired == null)
                                        .Where(x => NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(x.PeriodTypeId))
                                        .Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                        .Select(x => x.CourtId)
                                        .ToArrayAsync();

            if (!userInfo.IsGlobalAdmin)
            {
                if (courtListIds.Length == 0)
                {
                    return null;
                }

                if (!courtListIds.Contains(userInfo.CourtId))
                {
                    var user = await repo.GetByIdAsync<ApplicationUser>(userInfo.Id);
                    user.CourtId = courtListIds.First();
                    await repo.SaveChangesAsync();
                }
            }

            if (userInfo != null)
            {
                return userInfo.Id;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<LabelValueVM> Get_Courts(string term, int? id)
        {
            term = term.SafeLower();
            Expression<Func<Court, bool>> filter = x => false;
            if (id > 0)
            {
                filter = x => x.Id == id;
            }
            else
            {
                //При търсене по име да не излиза съда за случайно разпределение
                filter = x => EF.Functions.ILike(x.Label, term.ToPaternSearch()) && x.Id != NomenclatureConstants.Courts.RandomAssignment;
            }
            return repo.AllReadonly<Court>()
                            .Where(filter)
                            .Where(x => x.IsActive)
                            .OrderBy(x => x.Label)
                            .Select(x => new LabelValueVM
                            {
                                Value = x.Id.ToString(),
                                Label = x.Label
                            }).ToList();
        }

        public List<SelectListItem> LawUnitAddress_SelectDDL_ByCaseLawUnitId(int caseLawUnitId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var caseLawUnit = repo.AllReadonly<CaseLawUnit>()
                                  .Where(x => x.Id == caseLawUnitId)
                                  .FirstOrDefault();
            int lawUnitId = caseLawUnit?.LawUnitId ?? 0;
            var result = repo.AllReadonly<LawUnitAddress>()
                       .Include(x => x.Address)
                       .Where(x => x.LawUnitId == lawUnitId)
                       .Select(x => new SelectListItem()
                       {
                           Text = x.Address.FullAddressNotification(),
                           Value = x.AddressId.ToString()
                       }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
                result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });

            if (addAllElement)
                result.Insert(0, new SelectListItem() { Text = "Всички", Value = "-2" });

            return result;
        }

        public async Task<List<SelectListItem>> LawUnitAddress_SelectDDL_ByCaseLawUnitIdAsync(int caseLawUnitId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var caseLawUnit = await repo.AllReadonly<CaseLawUnit>()
                                        .Where(x => x.Id == caseLawUnitId)
                                        .FirstOrDefaultAsync();

            int lawUnitId = caseLawUnit?.LawUnitId ?? 0;
            var result = await repo.AllReadonly<LawUnitAddress>()
                                   .Where(x => x.LawUnitId == lawUnitId)
                                   .Select(x => new SelectListItem()
                                   {
                                       Text = x.Address.FullAddressNotification(),
                                       Value = x.AddressId.ToString()
                                   })
                                   .ToListAsync() ?? new List<SelectListItem>();

            if (addDefaultElement)
                result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });

            if (addAllElement)
                result.Insert(0, new SelectListItem() { Text = "Всички", Value = "-2" });

            return result;
        }

        public IQueryable<CourtVM> CourtsByType(int courtTypeId)
        {
            return repo.AllReadonly<Court>()
                            .Where(x => x.IsActive &&
                                   (courtTypeId <= 0 || x.CourtTypeId == courtTypeId))
                            .Include(x => x.CourtType)
                            .Select(x => new CourtVM
                            {
                                Id = x.Id,
                                CityName = x.CityName ?? "",
                                Address = x.Address ?? "",
                                Label = x.Label,
                                CourtTypeName = x.CourtType == null ? "" : x.CourtType.Label,
                                EcliCode = x.EcliCode,
                                Code = x.Code,

                            }).AsQueryable();
        }
        public bool CourtSaveData(Court model)
        {
            try
            {
                model.CourtRegionId = model.CourtRegionId.EmptyToNull();
                if (model.Id > 0)
                {
                    var saved = repo.All<Court>()
                                     .Include(x => x.CourtAddress)
                                     .Where(x => x.Id == model.Id)
                                     .FirstOrDefault();
                    saved.CityCode = model.CityCode;
                    saved.CityName = model.CityName;
                    saved.Address = model.Address;
                    saved.CourtRegionId = model.CourtRegionId;
                    saved.PhoneNumber = model.PhoneNumber;
                    saved.Email = model.Email;
                    saved.JudgeCount = model.JudgeCount;
                    if (!String.IsNullOrEmpty(model.CourtLogo))
                        saved.CourtLogo = model.CourtLogo;

                    if (saved.AddressId == null)
                        saved.CourtAddress = new Address();
                    saved.CourtAddress.CopyFrom(model.CourtAddress);
                    //repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    repo.Add(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на съд Id={model.Id}");
                return false;
            }

        }

        public IQueryable<LawUnitAddressListVM> LawUnitAddress_Select(int lawUnitId)
        {
            return repo.AllReadonly<LawUnitAddress>()
                .Where(x => x.LawUnitId == lawUnitId)
                .Select(x => new LawUnitAddressListVM
                {
                    LawUnitId = x.LawUnitId,
                    AddressId = x.AddressId,
                    FullAddress = x.Address.FullAddress,
                    AddressTypeName = x.Address.AddressType.Label
                }).AsQueryable();
        }

        public (bool result, string errorMessage) LawUnitAddress_SaveData(LawUnitAddress model)
        {
            try
            {
                if (model.Address.AddressTypeId == NomenclatureConstants.AddressType.Permanent)
                {
                    var existsPermanentAddress = repo.AllReadonly<LawUnitAddress>().Where(x => x.LawUnitId == model.LawUnitId &&
                                    x.Address.AddressTypeId == NomenclatureConstants.AddressType.Permanent &&
                                    x.AddressId != model.AddressId).Any();
                    if (existsPermanentAddress == true)
                    {
                        return (result: false, errorMessage: "Не може да има повече от един постоянен адрес. При необходимост коригирайте данните във вече въведения постоянен адрес.");
                    }
                }

                if (model.AddressId > 0)
                {
                    //Update
                    var saved = repo.All<LawUnitAddress>().Include(x => x.Address).Where(x => x.LawUnitId == model.LawUnitId &&
                                                  x.AddressId == model.AddressId).FirstOrDefault();
                    saved.Address.CopyFrom(model.Address);
                    nomService.SetFullAddress(saved.Address);

                    //repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    nomService.SetFullAddress(model.Address);

                    repo.Add<LawUnitAddress>(model);
                    repo.SaveChanges();
                }
                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на LawUnitAddress Id={model.AddressId}");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        public LawUnitAddress LawUnitAddress_GetById(int lawUnitId, long addressId)
        {
            return repo.AllReadonly<LawUnitAddress>().Include(x => x.Address).Where(x => x.LawUnitId == lawUnitId && x.AddressId == addressId).FirstOrDefault();
        }

        private BreadcrumbsVM FillBreadcrumbs(string title, string href)
        {
            return new BreadcrumbsVM()
            {
                Title = title,
                Href = href
            };
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCase(int CaseId)
        {
            var caseInfo = repo.AllReadonly<Case>()
                               .Where(x => x.Id == CaseId)
                               .Select(x => new BCCaseModel
                               {
                                   CaseStateId = x.CaseStateId,
                                   CaseTypeCode = x.CaseType.Code,
                                   RegDate = x.RegDate,
                                   ShortNumber = x.ShortNumber
                               })
                               .FirstOrDefault();

            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Съдебни дела", urlHelper.Action("Index", "Case")));
            result.Add(FillBreadcrumbs(CaseExtensions.GetCaseNameBreadcrumbs(caseInfo), urlHelper.Action("CasePreview", "Case", new { id = CaseId })));

            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCaseSession(int CaseSessionId, bool IsViewRowSession = true)
        {

            var sessionInfo = repo.AllReadonly<CaseSession>()
                             .Where(x => x.Id == CaseSessionId)
                             .Select(x => new
                             {
                                 x.CaseId,
                                 CaseTypeCode = x.Case.CaseType.Code,
                                 x.Case.CaseStateId,
                                 CaseShortNumber = x.Case.ShortNumber,
                                 CaseRegDate = x.Case.RegDate,
                                 SessionTypeLabel = x.SessionType.Label,
                                 CaseSessionDateFrom = x.DateFrom
                             })
                             .FirstOrDefault();

            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Съдебни дела", urlHelper.Action("Index", "Case")));
            result.Add(FillBreadcrumbs(CaseExtensions.GetCaseNameBreadcrumbs(new BCCaseModel()
            {
                CaseStateId = sessionInfo.CaseStateId,
                CaseTypeCode = sessionInfo.CaseTypeCode,
                RegDate = sessionInfo.CaseRegDate,
                ShortNumber = sessionInfo.CaseShortNumber
            }), urlHelper.Action("CasePreview", "Case", new { id = sessionInfo.CaseId })));

            if (IsViewRowSession)
                result.Add(FillBreadcrumbs(CaseExtensions.GetCaseSessionNameBreadcrumbs(new BCCaseSessionModel() { SessionType = sessionInfo.SessionTypeLabel, DateFrom = sessionInfo.CaseSessionDateFrom }), urlHelper.Action("Preview", "CaseSession", new { id = CaseSessionId })));

            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetCaseSessionFastDocument(int CaseSessionId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCaseSession(CaseSessionId);
            result.Add(FillBreadcrumbs("Съпровождащи документи представен в заседание", urlHelper.Action("Index", "CaseSessionFastDocument", new { CaseSessionId = CaseSessionId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCaseSessionAct(int CaseSessionActId)
        {
            return Breadcrumbs_GetForCaseSessionActAsync(CaseSessionActId).Result;
        }
        public async Task<List<BreadcrumbsVM>> Breadcrumbs_GetForCaseSessionActAsync(int CaseSessionActId)
        {
            var actInfo = await repo.AllReadonly<CaseSessionAct>()
                              .Where(x => x.Id == CaseSessionActId)
                              .AsSplitQuery()
                              .Select(x => new
                              {
                                  x.CaseSession.CaseId,
                                  CaseTypeCode = x.Case.CaseType.Code,
                                  x.Case.CaseStateId,
                                  CaseShortNumber = x.Case.ShortNumber,
                                  CaseRegDate = x.Case.RegDate,
                                  x.CaseSessionId,
                                  SessionTypeLabel = x.CaseSession.SessionType.Label,
                                  CaseSessionDateFrom = x.CaseSession.DateFrom,
                                  ActTypeLabel = x.ActType.Label,
                                  ActRegNumber = x.RegNumber,
                                  ActRegDate = x.RegDate,
                              })
                              .FirstOrDefaultAsync();

            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Съдебни дела", urlHelper.Action("Index", "Case")));
            result.Add(FillBreadcrumbs(CaseExtensions.GetCaseNameBreadcrumbs(new BCCaseModel()
            {
                CaseStateId = actInfo.CaseStateId,
                CaseTypeCode = actInfo.CaseTypeCode,
                RegDate = actInfo.CaseRegDate,
                ShortNumber = actInfo.CaseShortNumber
            }), urlHelper.Action("CasePreview", "Case", new { id = actInfo.CaseId })));
            result.Add(FillBreadcrumbs(CaseExtensions.GetCaseSessionNameBreadcrumbs(new BCCaseSessionModel() { SessionType = actInfo.SessionTypeLabel, DateFrom = actInfo.CaseSessionDateFrom }), urlHelper.Action("Preview", "CaseSession", new { id = actInfo.CaseSessionId })));
            result.Add(FillBreadcrumbs(CaseExtensions.GetCaseSessionActNameBreadcrumbs(new BCCaseSessionActModel() { ActTypeLabel = actInfo.ActTypeLabel, RegNumber = actInfo.ActRegNumber, RegDate = actInfo.ActRegDate }), urlHelper.Action("Edit", "CaseSessionAct", new { id = CaseSessionActId })));

            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCaseSelectionProtokol(int CaseId)
        {

            List<BreadcrumbsVM> result = Breadcrumbs_GetForCase(CaseId);
            result.Add(FillBreadcrumbs("Протоколи", urlHelper.Action("Index", "CaseSelectionProtokol", new { id = CaseId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCaseSessionActMoney(int CaseSessionActId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCaseSessionAct(CaseSessionActId);
            result.Add(FillBreadcrumbs("Суми към съдебен акт", urlHelper.Action("Obligation", "Money", new { caseSessionActId = CaseSessionActId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCaseSessionActLawBase(int CaseSessionActId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCaseSessionAct(CaseSessionActId);
            result.Add(FillBreadcrumbs("Текстове съдебен акт", urlHelper.Action("Index", "CaseSessionActLawBase", new { caseSessionActId = CaseSessionActId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonAddress(int casePersonId)
        {
            var casePerson = repo.GetById<CasePerson>(casePersonId);
            List<BreadcrumbsVM> result = null;
            if (casePerson.CaseSessionId == null)
                result = Breadcrumbs_GetForCase(casePerson.CaseId);
            else
                result = Breadcrumbs_GetForCaseSession(casePerson.CaseSessionId ?? 0);
            result.Add(FillBreadcrumbs("Адреси " + casePerson.FullName, urlHelper.Action("Edit", "CasePerson", new { id = casePersonId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonSentence(int casePersonId)
        {
            var casePerson = repo.GetById<CasePerson>(casePersonId);
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCase(casePerson.CaseId);
            result.Add(FillBreadcrumbs("Присъда на " + casePerson.FullName, urlHelper.Action("Index", "CasePersonSentence", new { casePersonId = casePersonId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForCaseNotifications(int notificationId, int notificationListTypeId)
        {
            var notification = repo.AllReadonly<CaseNotification>().Where(x => x.Id == notificationId).FirstOrDefault();
            List<BreadcrumbsVM> result = Breadcrumbs_ForCaseNotifications(notification.CaseId, notification.CaseSessionId, notification.CaseSessionActId, notificationListTypeId);
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForCaseNotifications(int caseId, int? caseSessionId, int? caseSessionActId, int notificationListTypeId)
        {
            List<BreadcrumbsVM> result;
            //if ((caseSessionActId ?? 0) > 0)
            //{
            //    result = Breadcrumbs_GetForCaseSessionAct(caseSessionActId ?? 0);
            //}
            //else
            //{
            if ((caseSessionId ?? 0) > 0)
            {
                result = Breadcrumbs_GetForCaseSession(caseSessionId ?? 0);
                if (result != null && result.Any() && notificationListTypeId > 0)
                {
                    result.Last().Href += $"?notifListTypeId={notificationListTypeId}";
                }
            }
            else
            {
                result = Breadcrumbs_GetForCase(caseId);
            }

            // }
            //result.Add(new BreadcrumbsVM()
            //{
            //    Title = "Уведомления",
            //    Href = urlHelper.Action("Index", "CaseNotification", new { id = caseId, caseSessionId, caseSessionActId})
            //}); ;
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForCaseNotificationListPrint(int caseSessionId, int notificationListTypeId)
        {
            var caseSession = repo.AllReadonly<CaseSession>()
                                   .Where(x => x.Id == caseSessionId)
                                  .FirstOrDefault();
            List<BreadcrumbsVM> result = Breadcrumbs_ForCaseNotifications(caseSession?.CaseId ?? 0, caseSessionId, null, notificationListTypeId);
            result.Add(FillBreadcrumbs("Редакция на списък ", urlHelper.Action("EditTinyMCE", "CaseNotification", new { caseSessionId, NotificationListTypeId = notificationListTypeId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForCaseNotificationEdit(int notificationId)
        {
            var notification = GetReadonly<CaseNotification>(notificationId);
            if (notification == null)
            {
                return new List<BreadcrumbsVM>();
            }
            return Breadcrumbs_ForCaseNotificationEdit(notification, 0);
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForCaseNotificationEdit(CaseNotification notification, int notificationListTypeId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForCaseNotifications(notification.CaseId, notification.CaseSessionId, notification.CaseSessionActId, notificationListTypeId);
            result.Add(FillBreadcrumbs("Уведомление " + notification.RegNumber, urlHelper.Action("Edit", "CaseNotification", new { id = notification.Id })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForCaseNotificationEditTinyMCE(CaseNotification notification)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForCaseNotificationEdit(notification, 0);
            result.Add(FillBreadcrumbs("Редакция бланка " + notification.RegNumber, urlHelper.Action("EditTinyMCE", "CaseNotification", new { id = notification.Id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForDocumentNotificationEditTinyMCE(DocumentNotification notification)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_DocumentNotification(notification.Id, notification.DocumentId ?? 0, notification.DocumentResolutionId, notification.NotificationTypeId ?? 0);
            result.Add(FillBreadcrumbs("Редакция бланка " + notification.RegNumber, urlHelper.Action("EditTinyMCE", "DocumentNotification", new { id = notification.Id })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForCaseNotificationDeliveryOper(int notificationId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForCaseNotificationEdit(notificationId);
            result.Add(FillBreadcrumbs("Посещения", urlHelper.Action("Index", "DeliveryItemOper", new { notificationId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForDocumentNotificationDeliveryOper(int notificationId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_DocumentNotification(notificationId, 0, 0, 0);
            result.Add(FillBreadcrumbs("Посещения", urlHelper.Action("IndexDocument", "DeliveryItemOper", new { notificationId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForCaseNotificationDeliveryOperEdit(int notificationId, int deliveryOperId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForCaseNotificationDeliveryOper(notificationId);
            result.Add(FillBreadcrumbs("Редакция посещение", urlHelper.Action("Edit", "DeliveryItemOper", new { id = notificationId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForDocumentNotificationDeliveryOperEdit(int notificationId, int deliveryOperId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForDocumentNotificationDeliveryOper(notificationId);
            result.Add(FillBreadcrumbs("Редакция посещение", urlHelper.Action("EditDocument", "DeliveryItemOper", new { id = notificationId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForCaseNotificationEditReturn(int notificationId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForCaseNotificationEdit(notificationId);
            var deliveryItem = repo.AllReadonly<DeliveryItem>().Where(x => x.CaseNotificationId == notificationId).FirstOrDefault();
            result.Add(FillBreadcrumbs("Връщане на отрязък", urlHelper.Action("EditReturn", "DeliveryItem", new { Id = deliveryItem?.Id })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForDocumentNotificationEditReturn(int notificationId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_DocumentNotification(notificationId, 0, 0, 0);
            var deliveryItem = repo.AllReadonly<DeliveryItem>().Where(x => x.CaseNotificationId == notificationId).FirstOrDefault();
            result.Add(FillBreadcrumbs("Връщане на отрязък", urlHelper.Action("EditReturnDocument", "DeliveryItem", new { Id = deliveryItem?.Id })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_GetForDocumentTemplate(int sourceType, long sourceId)
        {
            List<BreadcrumbsVM> result = null;
            switch (sourceType)
            {
                case SourceTypeSelectVM.CaseSessionAct:
                    result = Breadcrumbs_GetForCaseSessionAct((int)sourceId);
                    break;
                case SourceTypeSelectVM.Case:
                    result = Breadcrumbs_GetForCase((int)sourceId);
                    break;
                case SourceTypeSelectVM.CaseNotification:
                    {
                        var notificationInfo = repo.GetById<CaseNotification>((int)sourceId);
                        result = Breadcrumbs_ForCaseNotificationEdit(notificationInfo, 0);
                    }
                    break;
                case SourceTypeSelectVM.CasePersonBulletin:
                    result = Breadcrumbs_GetForCasePersonSentenceBulletin((int)sourceId);
                    break;
                case SourceTypeSelectVM.CaseSessionActDivorce:
                    result = Breadcrumbs_GetForCaseSessionActDivorce((int)sourceId);
                    break;
                case SourceTypeSelectVM.CaseLawyerHelp:
                    result = Breadcrumbs_GetForCaseLawyerHelpEdit((int)sourceId);
                    break;
                case SourceTypeSelectVM.DocumentResolution:
                    result = Breadcrumbs_DocumentResolution(sourceId);
                    break;
                default:
                    break;
            }

            return result;
        }

        public IQueryable<CourtHallVM> CourtHall_Select(int CourtId)
        {
            return repo.AllReadonly<CourtHall>()
                       .Where(x => x.CourtId == CourtId)
                       .Select(x => new CourtHallVM()
                       {
                           Id = x.Id,
                           Name = x.Name,
                           Location = x.Location
                       })
                       .AsQueryable();
        }

        public bool CourtHall_SaveData(CourtHall model)
        {
            try
            {
                if (model.Id > 0)
                {
                    var saved = repo.GetById<CourtHall>(model.Id);
                    saved.Name = model.Name;
                    saved.Location = model.Location;
                    saved.Description = model.Description;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo;
                    repo.SaveChanges();
                }
                else
                {
                    repo.Add(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CourtHall Id={model.Id}");
                return false;
            }
        }

        public IQueryable<CourtJuryFeeListVM> CourtJuryFee_Select(int courtId)
        {
            return repo.AllReadonly<CourtJuryFee>()
                            .Where(x => x.CourtId == courtId)
                            .Select(x => new CourtJuryFeeListVM
                            {
                                Id = x.Id,
                                HourFee = x.HourFee,
                                HourFeeEUR = x.HourFeeEUR,
                                MinDayFee = x.MinDayFee,
                                MinDayFeeEUR = x.MinDayFeeEUR,
                                DateFrom = x.DateFrom,
                                DateTo = x.DateTo
                            }).AsQueryable();
        }

        public bool CourtJuryFee_SaveData(CourtJuryFee model, ref string errorMessage)
        {
            try
            {
                DateTime dateEnd = DateTime.Now.AddYears(100).Date;
                //Проверка за припокриване на периоди
                var exists = repo.AllReadonly<CourtJuryFee>()
                                .Where(x => x.Id != model.Id && x.CourtId == model.CourtId &&
                                ((x.DateTo ?? dateEnd).Date >= model.DateFrom.Date && (x.DateTo ?? dateEnd).Date <= (model.DateTo ?? dateEnd).Date ||
                                  (model.DateTo ?? dateEnd).Date >= x.DateFrom.Date && (model.DateTo ?? dateEnd).Date <= (x.DateTo ?? dateEnd).Date)
                                 ).Any();

                if (exists == true)
                {
                    errorMessage = "За периода има въведени данни";
                    return false;
                }

                if (model.Id > 0)
                {
                    var saved = repo.GetById<CourtJuryFee>(model.Id);
                    saved.HourFee = model.HourFee;
                    saved.HourFeeEUR = model.HourFeeEUR;
                    saved.MinDayFee = model.MinDayFee;
                    saved.MinDayFeeEUR = model.MinDayFeeEUR;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo;
                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;
                    repo.SaveChanges();
                }
                else
                {
                    model.UserId = userContext.UserId;
                    model.DateWrt = DateTime.Now;
                    repo.Add(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CourtJuryFee Id={model.Id}");
                return false;
            }
        }

        public IQueryable<CourtPosDeviceListVM> CourtPosDevice_Select(int courtId)
        {
            return repo.AllReadonly<CourtPosDevice>()
                            .Where(x => x.CourtId == courtId)
                            .Select(x => new CourtPosDeviceListVM
                            {
                                Id = x.Id,
                                Label = x.Label,
                                CourtBankAccountName = x.CourtBankAccount.Label,
                                Tid = x.Tid,
                                IsActive = x.IsActive
                            }).AsQueryable();
        }

        public bool CourtPosDevice_SaveData(CourtPosDevice model)
        {
            try
            {
                if (model.Id > 0)
                {
                    var saved = repo.GetById<CourtPosDevice>(model.Id);
                    saved.Label = model.Label;
                    saved.CourtBankAccountId = model.CourtBankAccountId;
                    saved.Tid = model.Tid;
                    saved.IsActive = model.IsActive;
                    saved.BIC = model.BIC;
                    saved.BankName = model.BankName;
                    repo.SaveChanges();
                }
                else
                {
                    model.DateStart = DateTime.Now;
                    repo.Add(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CourtPosDevice Id={model.Id}");
                return false;
            }
        }

        public async Task<List<SelectListItem>> CourtPosDevice_SelectDDL(int courtId, bool addDefaultElement = false, bool addAllElement = false)
        {
            DateTime today = DateTime.Now.Date;
            var result = await repo.AllReadonly<CourtPosDevice>()
                                   .Where(x => x.CourtId == courtId)
                                   .Select(x => new SelectListItem()
                                   {
                                       Text = x.Label + " - " + x.Tid,
                                       Value = x.Tid
                                   }).ToListAsync() ?? new List<SelectListItem>();

            if (addDefaultElement)
                result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            if (addAllElement)
                result.Insert(0, new SelectListItem() { Text = "Всички", Value = "-2" });

            return result;
        }

        public IEnumerable<SelectListItem> GetEnumSelectList<T>()
        {
            //return (Enum.GetValues(typeof(T)).Cast<int>().Select(e => new SelectListItem() { Text = Enum.GetName(typeof(T), e), Value = e.ToString() })).ToList();
            return (Enum.GetValues(typeof(T)).Cast<int>().Select(e => new SelectListItem()
            {
                Text = (typeof(T)).GetMember(Enum.GetName(typeof(T), e)).First().GetCustomAttribute<DisplayAttribute>().Name,
                Value = e.ToString()
            })).ToList();
        }

        public Court GetCourt(int id)
        {
            return repo.AllReadonly<Court>().Where(x => x.Id == id).FirstOrDefault();
        }
        public List<BreadcrumbsVM> Breadcrumbs_GetForCourts()
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Съдилища", urlHelper.Action("Index", "Court")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCurrentCourt(int courtId, string returnUrl)
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Данни за текущия съд", urlHelper.Action("Edit", "Court", new { Id = courtId, returnUrl = returnUrl })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_GetForCourt(int courtId, string returnUrl)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCourts();
            result.Add(FillBreadcrumbs("Данни за съд", urlHelper.Action("Edit", "Court", new { Id = courtId, returnUrl = returnUrl })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForDeliveryAreas()
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Райони за призоваване", urlHelper.Action("Index", "DeliveryArea")));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForDeliveryArea(int deliveryAreaId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForDeliveryAreas();
            result.Add(FillBreadcrumbs("Район за призоваване", urlHelper.Action("Edit", "DeliveryArea", new { deliveryAreaId = deliveryAreaId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForDeliveryAreaAddresses(int deliveryAreaId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForDeliveryAreas();
            result.Add(FillBreadcrumbs("Адреси към район", urlHelper.Action("Index", "DeliveryAreaAddress", new { deliveryAreaId = deliveryAreaId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForDeliveryAreaAddressesDuplication()
        {
            var result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Адреси към различни райони", urlHelper.Action("IndexDuplication", "DeliveryAreaAddress")));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForDeliveryAreaAddressEdit(int deliveryAreaId, int deliveryAreaAddressId)
        {
            List<BreadcrumbsVM> result;
            if (deliveryAreaId != 0)
                result = Breadcrumbs_ForDeliveryAreaAddresses(deliveryAreaId);
            else
                result = Breadcrumbs_ForDeliveryAreaAddressesDuplication();
            result.Add(FillBreadcrumbs("Адрес", urlHelper.Action("Edit", "DeliveryAreaAddress", new { deliveryAreaAddressId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItems(int filterType)
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs(new DeliveryItemFilterVM() { FilterType = filterType }.getDeliveryTypeName(), "postToFilterDeliveryItems()"));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemsTrans(int toNotificationStateId)
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs(DeliveryItemTransFilterVM.GetTitle(toNotificationStateId), "postToFilterDeliveryItemsTrans()"));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemEditRaion(int filterType, int deliveryItemId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForDeliveryItems(filterType);
            result.Add(FillBreadcrumbs("Смяна на район и призовкар", urlHelper.Action("Edit", "DeliveryItem", new { Id = deliveryItemId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemAdd(int filterType)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForDeliveryItems(filterType);
            result.Add(FillBreadcrumbs("Смяна на район и призовкар", urlHelper.Action("AddReceived", "DeliveryItem")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemEditReturn(int filterType, int deliveryItemId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForDeliveryItems(filterType);
            result.Add(FillBreadcrumbs("Връщане на отрязък", urlHelper.Action("EditReturn", "DeliveryItem", new { Id = deliveryItemId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemAddOper(int filterType, int deliveryItemId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForDeliveryItems(filterType);
            result.Add(FillBreadcrumbs("Посещения", urlHelper.Action("Add", "DeliveryItemOper", new { Id = deliveryItemId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemChangeLawUnit()
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Смяна на призовкар", urlHelper.Action("CahngeLawUnit", "DeliveryItem")));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForDeliveryOutList()
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Описна книга на призовкар", urlHelper.Action("OutList", "DeliveryItem")));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForDeliveryResultList()
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Книга за получени и върнати призовки и други съдебни книжа", urlHelper.Action("ResultList", "DeliveryItem")));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemOperEdit(int filterType, int deliveryItemOperId)
        {
            var oper = repo.AllReadonly<DeliveryItemOper>().Where(x => x.Id == deliveryItemOperId).FirstOrDefault();
            List<BreadcrumbsVM> result = Breadcrumbs_ForDeliveryItemOpers(filterType, oper?.DeliveryItemId ?? 0);
            result.Add(FillBreadcrumbs("Редакция на посещение", urlHelper.Action("Edit", "DeliveryItemOper", new { Id = deliveryItemOperId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemHistoryOpers(int filterType, int deliveryItemId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForDeliveryItems(filterType);
            result.Add(FillBreadcrumbs("Проследяване", urlHelper.Action("Index", "DeliveryItemOper", new { Id = deliveryItemId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemOpers(int filterType, int deliveryItemId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForDeliveryItems(filterType);
            result.Add(FillBreadcrumbs("Посещения", "postToFilterDeliveryItems()"));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemOpers(int notificationId)
        {
            var notification = repo.AllReadonly<CaseNotification>().Where(x => x.Id == notificationId).FirstOrDefault();
            var deliveryItem = repo.AllReadonly<DeliveryItem>().Where(x => x.CaseNotificationId == notificationId).FirstOrDefault();
            List<BreadcrumbsVM> result;
            if ((notification.CaseSessionId ?? 0) > 0)
            {
                result = Breadcrumbs_GetForCaseSession(notification.CaseSessionId ?? 0);
            }
            else
            {
                result = Breadcrumbs_GetForCase(notification.CaseId);
            }
            result.Add(FillBreadcrumbs("Посещения", urlHelper.Action("Index", "DeliveryItemOper", new { notificationId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForCourtGroups(int filterCaseGroupId)
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Групи шифри", urlHelper.Action("Index", "CourtGroup", new { filterCaseGroupId = filterCaseGroupId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForCourtGroupEdit(int filterCaseGroupId, int id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForCourtGroups(filterCaseGroupId);
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("Edit", "CourtGroup", new { filterCaseGroupId = filterCaseGroupId, id = id })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForCourtGroupAdd(int filterCaseGroupId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForCourtGroups(filterCaseGroupId);
            result.Add(FillBreadcrumbs("Добавяне", urlHelper.Action("Add", "CourtGroup", new { filterCaseGroupId = filterCaseGroupId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForEditCourtGroupLawUnit(int filterCaseGroupId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForCourtGroups(filterCaseGroupId);
            result.Add(FillBreadcrumbs("Съдии към група", urlHelper.Action("EditCourtGroupLawUnit", "CourtGroup", new { filterCaseGroupId = filterCaseGroupId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForLawUnit(int lawUnitTypeId)
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            var lawUnitTypeDescription = GetPropById<LawUnitType, string>(x => x.Id == lawUnitTypeId, x => x.Description);
            result.Add(FillBreadcrumbs(lawUnitTypeDescription, urlHelper.Action("Index", "LawUnit", new { lawUnitType = lawUnitTypeId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForLawUnitEdit(int lawUnitTypeId, int id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForLawUnit(lawUnitTypeId);
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("Edit", "LawUnit", new { id = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForLawUnitAdd(int lawUnitTypeId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForLawUnit(lawUnitTypeId);
            result.Add(FillBreadcrumbs("Добавяне", urlHelper.Action("Add", "LawUnit", new { lawUnitType = lawUnitTypeId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForCourtBankAccount()
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Списък банкови сметки", urlHelper.Action("Index", "CourtBankAccount")));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForCourtBankAccountEdit(int id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForCourtBankAccount();
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("Edit", "CourtBankAccount", new { id = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForCourtBankAccountAdd()
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForCourtBankAccount();
            result.Add(FillBreadcrumbs("Добавяне", urlHelper.Action("Add", "CourtBankAccount")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForCourtPosDevice()
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Списък ПОС устройства", urlHelper.Action("PosDevice", "CourtBankAccount")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForCourtPosDeviceEdit(int id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForCourtPosDevice();
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("EditPosDevice", "CourtBankAccount", new { id = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForCourtPosDeviceAdd()
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForCourtPosDevice();
            result.Add(FillBreadcrumbs("Добавяне", urlHelper.Action("AddPosDevice", "CourtBankAccount")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForSpeciality(int lawUnitId)
        {
            var lawUnit = GetById<LawUnit>(lawUnitId);
            List<BreadcrumbsVM> result = Breadcrumbs_ForLawUnit(lawUnit.LawUnitTypeId);
            result.Add(FillBreadcrumbs("Специалности за " + lawUnit.FullName, urlHelper.Action("LawUnitSpeciality", "LawUnit", new { lawUnitId = lawUnitId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForLawUnitAddress(int lawUnitId)
        {
            var lawUnit = GetById<LawUnit>(lawUnitId);
            List<BreadcrumbsVM> result = Breadcrumbs_ForLawUnit(lawUnit.LawUnitTypeId);
            result.Add(FillBreadcrumbs("Адреси за " + lawUnit.FullName, urlHelper.Action("Edit", "LawUnit", new { id = lawUnitId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForLawUnitAddressEdit(int lawUnitId, long id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForLawUnitAddress(lawUnitId);
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("EditLawUnitAdr", "LawUnit", new { lawUnitId = lawUnitId, addressId = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForLawUnitAddressAdd(int lawUnitId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForLawUnitAddress(lawUnitId);
            result.Add(FillBreadcrumbs("Добавяне", urlHelper.Action("AddLawUnitAdr", "LawUnit", new { lawUnitId = lawUnitId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForCourtLawUnit(int periodTypeId, int lawUnitTypeId)
        {
            var lawUnitTypeDescription = GetPropById<LawUnitType, string>(x => x.Id == lawUnitTypeId, x => x.Description);
            var periodTypeLabel = GetPropById<PeriodType, string>(x => x.Id == periodTypeId, x => x.Label);
            List<BreadcrumbsVM> result = Breadcrumbs_ForLawUnit(lawUnitTypeId);
            result.Add(FillBreadcrumbs(lawUnitTypeDescription + " " + periodTypeLabel, urlHelper.Action("Index", "CourtLawUnit", new { periodType = periodTypeId, lawUnitType = lawUnitTypeId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForCourtLawUnitAdd(int periodTypeId, int lawUnitTypeId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForCourtLawUnit(periodTypeId, lawUnitTypeId);
            result.Add(FillBreadcrumbs("Добавяне", urlHelper.Action("Add", "CourtLawUnit", new { periodType = periodTypeId, lawUnitType = lawUnitTypeId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForCourtLawUnitEdit(int periodTypeId, int lawUnitTypeId, int id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForCourtLawUnit(periodTypeId, lawUnitTypeId);
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("Edit", "CourtLawUnit", new { id = id })));
            return result;
        }

        /// <summary>
        /// Breadcrumbs за редактиране на асистент/помощник/секретар
        /// </summary>
        /// <param name="periodTypeId">Тип на периода</param>
        /// <param name="lawUnitTypeId">Тип на служител</param>
        /// <param name="courtLawUnitId">Идентификатор на служител</param>
        /// <returns></returns>
        public List<BreadcrumbsVM> Breadcrumbs_ForCourtLawUnitAssistantAdd(int periodTypeId, int lawUnitTypeId, int courtLawUnitId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForCourtLawUnit(periodTypeId, lawUnitTypeId);
            result.Add(FillBreadcrumbs("Редакция на служител", urlHelper.Action("Edit", "CourtLawUnit", new { id = courtLawUnitId })));
            result.Add(FillBreadcrumbs("Редакция на секретар", urlHelper.Action("AddCourtLawUnitAssistant", "CourtLawUnit", new { courtLawUnitId = courtLawUnitId })));
            return result;
        }

        /// <summary>
        /// Breadcrumbs за редактиране на асистент/помощник/секретар
        /// </summary>
        /// <param name="periodTypeId">Тип на периода</param>
        /// <param name="lawUnitTypeId">Тип на служител</param>
        /// <param name="courtLawUnitId">Идентификатор на служител</param>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public List<BreadcrumbsVM> Breadcrumbs_ForCourtLawUnitAssistantEdit(int periodTypeId, int lawUnitTypeId, int courtLawUnitId, int id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForCourtLawUnit(periodTypeId, lawUnitTypeId);
            result.Add(FillBreadcrumbs("Редакция на служител", urlHelper.Action("Edit", "CourtLawUnit", new { id = courtLawUnitId })));
            result.Add(FillBreadcrumbs("Редакция на секретар", urlHelper.Action("EditCourtLawUnitAssistant", "CourtLawUnit", new { id = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForCourtLawUnitGroup(int courtLawUnitId)
        {
            var courtLawUnitInfo = repo.AllReadonly<CourtLawUnit>().Where(x => x.Id == courtLawUnitId)
                                                .Select(x => new
                                                {
                                                    x.PeriodTypeId,
                                                    x.LawUnit.FullName,
                                                    LawUnitTypeId = x.LawUnitTypeId ?? 0
                                                })
                                                .FirstOrDefault();

            List<BreadcrumbsVM> result = Breadcrumbs_ForCourtLawUnit(courtLawUnitInfo.PeriodTypeId, courtLawUnitInfo.LawUnitTypeId);
            result.Add(FillBreadcrumbs("Групи към съдия - " + courtLawUnitInfo.FullName, urlHelper.Action("EditCourtLawUnitGroup", "CourtLawUnit", new { id = courtLawUnitId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForCourtLawUnitCompartment(int courtLawUnitId)
        {
            var info = repo.AllReadonly<CourtLawUnit>()
                             .Where(x => x.Id == courtLawUnitId)
                             .Select(x =>
                             new
                             {
                                 x.LawUnit.FullName,
                                 x.PeriodTypeId,
                                 x.LawUnit.LawUnitTypeId
                             }).FirstOrDefault();

            List<BreadcrumbsVM> result = Breadcrumbs_ForCourtLawUnit(info.PeriodTypeId, info.LawUnitTypeId);
            result.Add(FillBreadcrumbs("Състави към съдия - " + info.FullName, urlHelper.Action("CompartmentList", "CourtLawUnit", new { id = courtLawUnitId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForCourtLawUnitCompartmentAdd(int courtLawUnitId)
        {
            var lawUnitId = GetPropById<CourtLawUnit, int>(x => x.Id == courtLawUnitId, x => x.LawUnitId);

            List<BreadcrumbsVM> result = Breadcrumbs_ForCourtLawUnitCompartment(courtLawUnitId);
            result.Add(FillBreadcrumbs("Добавяне", urlHelper.Action("AddCompartment", "CourtLawUnit", new { lawUnitId = lawUnitId, courtLawUnitId = courtLawUnitId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForCourtLawUnitCompartmentEdit(int courtLawUnitId, int compartmentId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForCourtLawUnitCompartment(courtLawUnitId);
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("EditCompartment", "CourtLawUnit", new { id = compartmentId, courtLawUnitId = courtLawUnitId })));
            return result;
        }


        public List<BreadcrumbsVM> Breadcrumbs_GetForCaseSessionMeeting(int CaseSessionMeetingId, bool sessionOnly = false)
        {
            var _info = repo.AllReadonly<CaseSessionMeeting>()
                            .Where(x => x.Id == CaseSessionMeetingId)
                            .Select(x => new
                            {
                                CaseSessionId = x.CaseSessionId,
                                DateFrom = x.DateFrom
                            })
                            .FirstOrDefault();


            List<BreadcrumbsVM> result = Breadcrumbs_GetForCaseSession(_info.CaseSessionId);
            if (!sessionOnly)
            {
                var _label = string.Empty;
                if (_info != null)
                {
                    _label = $"Секретари към сесия на заседание от  {_info.DateFrom:dd.MM.yyyy HH:mm}";
                }

                result.Add(FillBreadcrumbs(_label, urlHelper.Action("IndexMeetingUser", "CaseSessionMeeting", new { caseSessionMeetingId = CaseSessionMeetingId })));
            }
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForCourtJuryFee()
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Ставка възнаграждение за заседатели", urlHelper.Action("Index", "CourtJuryFee")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForCourtJuryFeeEdit(int id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForCourtJuryFee();
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("Edit", "CourtJuryFee", new { id = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForCourtJuryFeeAdd()
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForCourtJuryFee();
            result.Add(FillBreadcrumbs("Добавяне", urlHelper.Action("Add", "CourtJuryFee")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_Account()
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Потребители", urlHelper.Action("Index", "Account")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_AccountEdit(string id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_Account();
            result.Add(FillBreadcrumbs("Редактиране на потребител", urlHelper.Action("Edit", "Account", new { id = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_AccountAdd()
        {
            List<BreadcrumbsVM> result = Breadcrumbs_Account();
            result.Add(FillBreadcrumbs("Регистриране на потребител", urlHelper.Action("Register", "Account")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_AccountMobileToken(string userId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_Account();
            var userEmail = GetPropById<ApplicationUser, string>(x => x.Id == userId, x => x.Email);

            result.Add(FillBreadcrumbs("Мобилни токени за " + userEmail, urlHelper.Action("Index", "DeliveryAccount", new { userId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_AccountMobileTokenRegister(string userId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_AccountMobileToken(userId);
            result.Add(FillBreadcrumbs("Сдвояване токен", urlHelper.Action("TyingUser", "DeliveryAccount")));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForCaseEvidence(int caseEvidenceId)
        {
            throw new NotImplementedException();
        }

        public List<BreadcrumbsVM> Breadcrumbs_Institution(int institutionTypeId)
        {
            var institutionTypeLabel = GetPropById<InstitutionType, string>(x => x.Id == institutionTypeId, x => x.Label);
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs(institutionTypeLabel, urlHelper.Action("Index", "Institution", new { institutionType = institutionTypeId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_InstitutionEdit(int id)
        {
            var institutionTypeId = GetPropById<Institution, int>(x => x.Id == id, x => x.InstitutionTypeId);
            List<BreadcrumbsVM> result = Breadcrumbs_Institution(institutionTypeId);
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("Edit", "Institution", new { id = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_InstitutionAdd(int institutionTypeId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_Institution(institutionTypeId);
            result.Add(FillBreadcrumbs("Въвеждане", urlHelper.Action("Add", "Institution")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_Counter()
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Управление на броячи", urlHelper.Action("Index", "Counter")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_CounterEdit(int id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_Counter();
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("Edit", "Counter", new { id = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_CounterAdd()
        {
            List<BreadcrumbsVM> result = Breadcrumbs_Counter();
            result.Add(FillBreadcrumbs("Въвеждане", urlHelper.Action("Add", "Counter")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_WorkingDays()
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Pаботни и почивни дни", urlHelper.Action("Index", "WorkingDays")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_WorkingDaysEdit(int id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_WorkingDays();
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("Edit", "WorkingDays", new { Id = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_WorkingDaysAdd()
        {
            List<BreadcrumbsVM> result = Breadcrumbs_WorkingDays();
            result.Add(FillBreadcrumbs("Въвеждане", urlHelper.Action("Add", "WorkingDays")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_CaseCode()
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Списък Шифри", urlHelper.Action("CaseCodeList", "CaseGroup")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_CaseCodeEdit(int id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_CaseCode();
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("EditCaseCode", "CaseGroup", new { id = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_CaseCodeAdd()
        {
            List<BreadcrumbsVM> result = Breadcrumbs_CaseCode();
            result.Add(FillBreadcrumbs("Въвеждане", urlHelper.Action("AddCaseCode", "CaseGroup")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_LoadGroup()
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Групи по натовареност", urlHelper.Action("Index", "LoadGroup")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_LoadGroupEdit(int id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_LoadGroup();
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("Edit", "LoadGroup", new { id = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_LoadGroupAdd()
        {
            List<BreadcrumbsVM> result = Breadcrumbs_LoadGroup();
            result.Add(FillBreadcrumbs("Въвеждане", urlHelper.Action("Add", "LoadGroup")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_LoadGroupLink(int loadGroupId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_LoadGroup();
            var model = GetById<LoadGroup>(loadGroupId);
            result.Add(FillBreadcrumbs("Натовареност към група - " + model.Label, urlHelper.Action("LoadGroupLinkList", "LoadGroup", new { loadGroupId = loadGroupId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_LoadGroupLinkEdit(int loadGroupId, int id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_LoadGroupLink(loadGroupId);
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("EditLoadGroupLink", "LoadGroup", new { id = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_LoadGroupLinkAdd(int loadGroupId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_LoadGroupLink(loadGroupId);
            result.Add(FillBreadcrumbs("Въвеждане", urlHelper.Action("AddLoadGroupLink", "LoadGroup", new { loadGroupId = loadGroupId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_HtmlTemplate()
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Бланки на документи", "postToFilterHtmlTemplate()"));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_HtmlTemplateLink(int htmlTemplateId)
        {
            var htmlTemplateLabel = GetPropById<HtmlTemplate, string>(x => x.Id == htmlTemplateId, x => x.Label);
            List<BreadcrumbsVM> result = Breadcrumbs_HtmlTemplate();
            result.Add(FillBreadcrumbs("Връзки по вид съд/дело " + htmlTemplateLabel, "postToFilterHtmlTemplateLink()"));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_HtmlTemplateLinkEdit(int htmlTemplateId, int id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_HtmlTemplateLink(htmlTemplateId);
            result.Add(FillBreadcrumbs(id > 0 ? "Добавяне " : "Редакция", "postToFilterHtmlTemplateLink()"));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_HtmlTemplateParam(int htmlTemplateId)
        {
            var htmlTemplateDescription = GetPropById<HtmlTemplate, string>(x => x.Id == htmlTemplateId, x => x.Description);
            List<BreadcrumbsVM> result = Breadcrumbs_HtmlTemplate();
            result.Add(FillBreadcrumbs("Параметри в бланка " + htmlTemplateDescription, "postToFilterHtmlTemplateParam()"));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_HtmlTemplateParamEdit(int htmlTemplateId, int id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_HtmlTemplateParam(htmlTemplateId);
            result.Add(FillBreadcrumbs(id > 0 ? "Добавяне " : "Редакция", "postToFilterHtmlTemplate()"));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_HtmlTemplateEdit(int id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_HtmlTemplate();
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("Edit", "HtmlTemplate", new { id = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_HtmlTemplateAdd()
        {
            List<BreadcrumbsVM> result = Breadcrumbs_HtmlTemplate();
            result.Add(FillBreadcrumbs("Въвеждане", urlHelper.Action("Add", "HtmlTemplate")));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_HtmlTemplatePreview(int id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_HtmlTemplate();
            result.Add(FillBreadcrumbs("Преглед", urlHelper.Action("Preview", "HtmlTemplate", new { id = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_Document()
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Съдебна регистратура", urlHelper.Action("Index", "Document")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_DocumentEdit(long id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_Document();
            var document = GetReadonly<Document>(id);
            result.Add(FillBreadcrumbs("Документ " + document.DocumentNumber + "/" + document.DocumentDate.ToString("dd.MM.yyyy"), urlHelper.Action("Edit", "Document", new { id = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_DocumentObligation(long id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_DocumentEdit(id);
            result.Add(FillBreadcrumbs("Суми към документ", urlHelper.Action("Obligation", "Money", new { documentId = id })));
            return result;
        }

        public CourtBankAccount GetCourtBankAccountForMoneyType(int moneyTypeId)
        {
            var moneyType = GetById<Infrastructure.Data.Models.Nomenclatures.MoneyType>(moneyTypeId);

            return repo.AllReadonly<CourtBankAccount>()
                .Where(x => x.MoneyGroupId == moneyType.MoneyGroupId)
                        .FirstOrDefault();
        }

        public IQueryable<LawUnit> LawUnit_ByCourt(int court)
        {
            DateTime dateSelect = DateTime.Now;
            DateTime enddatenull = dateSelect.AddDays(1);

            List<int> plus = NomenclatureConstants.PeriodTypes.CurrentlyAvailable.ToList();
            return repo.AllReadonly<LawUnit>()
                     .Include(x => x.LawUnitType)
                     .Where(x => ((x.DateTo ?? dateSelect.Date) >= dateSelect.Date)
                                   && repo.AllReadonly<CourtLawUnit>().Where(c => c.LawUnitId == x.Id && c.CourtId == court && c.DateExpired == null &&
                                          plus.Contains(c.PeriodTypeId) && c.DateFrom <= dateSelect && (c.DateTo ?? enddatenull) >= dateSelect).Any()
                                                          ).AsQueryable();
        }

        public IQueryable<MultiSelectTransferVM> LawUnitMultiSelect_ByCourt(int courtId)
        {
            int[] lawunitTypes = new int[] { NomenclatureConstants.LawUnitTypes.Judge, NomenclatureConstants.LawUnitTypes.OtherEmployee };
            IQueryable<LawUnit> lawUnits = LawUnit_ByCourt(courtId).Where(x => lawunitTypes.Contains(x.LawUnitTypeId));
            return (from item in lawUnits
                    select new MultiSelectTransferVM()
                    {
                        Id = item.Id,
                        OrderInt = 0,
                        Text = item.FullName + " - " + item.LawUnitType.Label
                    }).AsQueryable();
        }

        public List<BreadcrumbsVM> Breadcrumbs_ArchiveCommittee()
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Експертни комисии", urlHelper.Action("ArchiveCommittee", "CourtArchive")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ArchiveCommitteeEdit(int id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ArchiveCommittee();
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("EditArchiveCommittee", "CourtArchive", new { id = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ArchiveCommitteeAdd()
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ArchiveCommittee();
            result.Add(FillBreadcrumbs("Въвеждане", urlHelper.Action("AddArchiveCommittee", "CourtArchive")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ArchiveIndex()
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Номенклатурни индекси", urlHelper.Action("ArchiveIndex", "CourtArchive")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ArchiveIndexEdit(int id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ArchiveIndex();
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("EditArchiveIndex", "CourtArchive", new { id = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ArchiveIndexAdd()
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ArchiveIndex();
            result.Add(FillBreadcrumbs("Въвеждане", urlHelper.Action("AddArchiveIndex", "CourtArchive")));
            return result;
        }

        public IQueryable<InstitutionAddressListVM> InstitutionAddress_Select(int institutionId)
        {
            return repo.AllReadonly<InstitutionAddress>()
                       .Where(x => x.InstitutionId == institutionId)
                       .Select(x => new InstitutionAddressListVM
                       {
                           InstitutionId = x.InstitutionId,
                           AddressId = x.AddressId,
                           FullAddress = x.Address.FullAddress,
                           AddressTypeName = x.Address.AddressType.Label
                       }).AsQueryable();
        }

        public (bool result, string errorMessage) InstitutionAddress_SaveData(InstitutionAddress model)
        {
            try
            {
                if (model.Address.AddressTypeId == NomenclatureConstants.AddressType.Permanent)
                {
                    var existsPermanentAddress = repo.AllReadonly<InstitutionAddress>().Where(x => x.InstitutionId == model.InstitutionId &&
                                    x.Address.AddressTypeId == NomenclatureConstants.AddressType.Permanent &&
                                    x.AddressId != model.AddressId).Any();
                    if (existsPermanentAddress == true)
                    {
                        return (result: false, errorMessage: "Не може да има повече от един постоянен адрес. При необходимост коригирайте данните във вече въведения постоянен адрес.");
                    }
                }

                if (model.AddressId > 0)
                {
                    //Update
                    var saved = repo.All<InstitutionAddress>().Include(x => x.Address).Where(x => x.InstitutionId == model.InstitutionId &&
                                                  x.AddressId == model.AddressId).FirstOrDefault();
                    saved.Address.CopyFrom(model.Address);
                    nomService.SetFullAddress(saved.Address);

                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    nomService.SetFullAddress(model.Address);

                    repo.Add<InstitutionAddress>(model);
                    repo.SaveChanges();
                }
                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на InstitutionAddress Id={model.AddressId}");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        public InstitutionAddress InstitutionAddress_GetById(int institutionId, long addressId)
        {
            return repo.AllReadonly<InstitutionAddress>().Include(x => x.Address).Where(x => x.InstitutionId == institutionId && x.AddressId == addressId).FirstOrDefault();
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForInstitutionAddress(int institutionId)
        {
            var institution = GetReadonly<Institution>(institutionId);
            List<BreadcrumbsVM> result = Breadcrumbs_Institution(institution.InstitutionTypeId);
            result.Add(FillBreadcrumbs("Адреси за " + institution.FullName, urlHelper.Action("Edit", "Institution", new { id = institutionId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForInstitutionAddressEdit(int institutionId, long id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForInstitutionAddress(institutionId);
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("EditInstitutionAdr", "Institution", new { institutionId = institutionId, addressId = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForInstitutionAddressAdd(int institutionId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForInstitutionAddress(institutionId);
            result.Add(FillBreadcrumbs("Добавяне", urlHelper.Action("AddInstitutionAdr", "Institution", new { institutionId = institutionId })));
            return result;
        }

        public IQueryable<Address> SelectEntity_SelectAddress(int personSourceType, long personSourceId)
        {
            IQueryable<Address> result = null;
            if (personSourceType == SourceTypeSelectVM.Court)
            {
                result = repo.AllReadonly<Court>()
                                    .Include(x => x.CourtAddress)
                                    .Where(x => x.Id == personSourceId)
                                    .Select(x => x.CourtAddress)
                                    .AsQueryable();
            }
            else if (personSourceType == SourceTypeSelectVM.LawUnit)
            {
                result = repo.AllReadonly<LawUnitAddress>()
                                    .Include(x => x.Address)
                                    .Where(x => x.LawUnitId == personSourceId)
                                    .Select(x => x.Address)
                                    .OrderBy(x => x.Id)
                                    .AsQueryable();
            }
            else if (personSourceType == SourceTypeSelectVM.Instutution)
            {
                result = repo.AllReadonly<InstitutionAddress>()
                                    .Include(x => x.Address)
                                    .Where(x => x.InstitutionId == personSourceId)
                                    .Select(x => x.Address)
                                    .OrderBy(x => x.Id)
                                    .AsQueryable();
            }
            if (result == null)
                result = Enumerable.Empty<Address>().AsQueryable();

            return result;
        }

        public List<SelectListItem> GetDDL_Institution(int institutionTypeId)
        {
            var result = repo.AllReadonly<Institution>()
                .Where(x => x.InstitutionTypeId == institutionTypeId)
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.FullName
                })
                .OrderBy(x => x.Text)
                .ToList();

            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });

            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_DocumentDecision()
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Решения по регистрирани документи", urlHelper.Action("DocumentDecision", "Document")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_DocumentDecisionEdit(long id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_DocumentDecision();
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("EditArchiveIndex", "Document", new { id = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_DocumentDecisionAdd(long documentId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_DocumentDecision();
            result.Add(FillBreadcrumbs("Въвеждане", urlHelper.Action("AddDocumentDecision", "Document", new { documentId = documentId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_CaseDeadLine()
        {

            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Срокове", urlHelper.Action("Index", "CaseDeadLine")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCaseCrime(int CaseId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCase(CaseId);
            result.Add(FillBreadcrumbs("Престъпления", urlHelper.Action("IndexCaseCrime", "CasePersonSentence", new { caseId = CaseId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCaseLoadIndex(int CaseId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCase(CaseId);
            result.Add(FillBreadcrumbs("Натовареност по дело", urlHelper.Action("Index", "CaseLoadIndex", new { id = CaseId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCaseLawyerHelp(int CaseId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCase(CaseId);
            result.Add(FillBreadcrumbs("Искане за правна помощ", urlHelper.Action("Index", "CaseLawyerHelp", new { id = CaseId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCaseLawyerHelpEdit(int CaseLawyerHelpId)
        {
            var caseLawyerHelpCaseId = GetPropById<CaseLawyerHelp, int>(CaseLawyerHelpId, x => x.CaseId);
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCaseLawyerHelp(caseLawyerHelpCaseId);
            result.Add(FillBreadcrumbs("Данни за служебен защитник", urlHelper.Action("Edit", "CaseLawyerHelp", new { id = CaseLawyerHelpId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonCrime(int CaseCrimeId)
        {
            var caseCrime = GetReadonly<CaseCrime>(CaseCrimeId);
            var caseCrimeVM = new CaseCrimeVM()
            {
                Id = caseCrime.Id,
                CaseId = caseCrime.CaseId,
                CrimeCode = caseCrime.CrimeCode,
                CrimeName = caseCrime.CrimeName,
                ValueEISSId = caseCrime.EISSId,
                ValueEISSPNumber = caseCrime.EISSPNumber
            };
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCaseCrime(caseCrime.CaseId);
            result.Add(FillBreadcrumbs("Лица към престъпление " + caseCrimeVM.ValueEISSPNumber, urlHelper.Action("IndexCasePersonCrime", "CasePersonSentence", new { caseCrimeId = CaseCrimeId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonSentencePunishment(int CasePersonSentenceId)
        {
            var info = repo.AllReadonly<CasePersonSentence>()
                                     .Where(x => x.Id == CasePersonSentenceId)
                                     .Select(x => new
                                     {
                                         x.CasePersonId,
                                         x.CasePerson.FullName
                                     })
                                     .FirstOrDefault();

            List<BreadcrumbsVM> result = Breadcrumbs_GetForCasePersonSentence(info.CasePersonId);
            result.Add(FillBreadcrumbs("Наказания на " + info.FullName, urlHelper.Action("IndexCasePersonSentencePunishment", "CasePersonSentence", new { casePersonSentenceId = CasePersonSentenceId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonSentencePunishmentCrime(int CasePersonSentencePunishmentId)
        {
            var casePersonSentencePunishment = repo.AllReadonly<CasePersonSentencePunishment>()
                                                   .Where(x => x.Id == CasePersonSentencePunishmentId)
                                                   .Select(x => new CasePersonSentencePunishmentVM()
                                                   {
                                                       Id = x.Id,
                                                       IsSummaryPunishment = x.IsSummaryPunishment,
                                                       IsSummaryPunishmentText = (x.IsSummaryPunishment) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No,
                                                       SentenceText = x.SentenceText,
                                                       SentenceTypeLabel = x.SentenceType.Label,
                                                       SentenseMoney = x.SentenseMoney,
                                                       CasePersonSentenceId = x.CasePersonSentenceId
                                                   })
                                                   .FirstOrDefault(); ;
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCasePersonSentencePunishment(casePersonSentencePunishment.CasePersonSentenceId);
            result.Add(FillBreadcrumbs("Участие в " + casePersonSentencePunishment.SentenceTypeLabel, urlHelper.Action("IndexCasePersonSentencePunishmentCrime", "CasePersonSentence", new { CasePersonSentencePunishmentId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCaseSessionActComplain(int CaseSessionActId)
        {
            var caseSessionActCaseSessionId = GetPropById<CaseSessionAct, int>(CaseSessionActId, x => x.CaseSessionId);
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCaseSession(caseSessionActCaseSessionId);
            result.Add(FillBreadcrumbs("Обжалвания към съдебен акт", urlHelper.Action("Index", "CaseSessionActComplain", new { caseSessionActId = CaseSessionActId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCaseSessionActComplainEdit(int CaseSessionActComplainId)
        {
            var caseSessionActComplainCaseSessionActId = GetPropById<CaseSessionActComplain, int>(CaseSessionActComplainId, c => c.CaseSessionActId);
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCaseSessionActComplain(caseSessionActComplainCaseSessionActId);
            result.Add(FillBreadcrumbs("Редакция на обжалване", urlHelper.Action("Edit", "CaseSessionActComplain", new { id = CaseSessionActComplainId })));
            return result;
        }

        public async Task<bool> Users_UpdateSetting(string setting, string value)
        {
            try
            {
                UserSettingsModel model = await dBUserContext.Settings();
                var user = repo.GetById<ApplicationUser>(userContext.UserId);

                switch (setting)
                {
                    case UserSettingsModel.Set.CalendarStyle:
                        model.CalendarStyle = value;
                        break;
                    case UserSettingsModel.Set.LastSelectedScaner:
                        model.LastSelectedScaner = value;
                        break;
                }
                user.UserSettings = JsonConvert.SerializeObject(model);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Грешка в CommonService.Users_UpdateSetting");
                return false;
            }
        }

        public async Task<bool> Users_UpdateSetting(UserSettingsModel model)
        {
            try
            {
                var savedModel = await dBUserContext.Settings();
                var user = repo.GetById<ApplicationUser>(userContext.UserId);
                model.LastSelectedScaner = savedModel.LastSelectedScaner;
                user.UserSettings = JsonConvert.SerializeObject(model);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Грешка в CommonService.Users_UpdateSetting");
                return false;
            }
        }



        public List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonInheritance(int casePersonId)
        {
            var casePerson = repo.GetById<CasePerson>(casePersonId);
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCase(casePerson.CaseId);
            result.Add(FillBreadcrumbs("Наследство на " + casePerson.FullName, urlHelper.Action("IndexInheritance", "CasePerson", new { casePersonId = casePersonId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_CourtRegion()
        {
            var result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Съдебни райони", urlHelper.Action("Index", "CourtRegion")));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_CourtRegionEdit(int courtRegionId)
        {
            var result = Breadcrumbs_CourtRegion();
            var courtRegionLabel = GetPropById<CourtRegion, string>(courtRegionId, x => x.Label);
            result.Add(FillBreadcrumbs(courtRegionId <= 0 ? "Добавяне съдебeн район" : "Съдебeн район на " + courtRegionLabel, urlHelper.Action("Edit", "CourtRegion", new { id = courtRegionId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_CourtRegionIndexArea(int courtRegionId)
        {
            var result = Breadcrumbs_CourtRegionEdit(courtRegionId);
            result.Add(FillBreadcrumbs("Област-Община", urlHelper.Action("IndexArea", "CourtRegion", new { Id = courtRegionId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_CourtRegionIndexAreaEdit(int courtRegionId, int courtAreaId)
        {
            var result = Breadcrumbs_CourtRegionIndexArea(courtRegionId);
            result.Add(FillBreadcrumbs(courtAreaId <= 0 ? "Добавяне Област-Община" : "Редакция Област-Община", urlHelper.Action("IndexArea", "CourtRegion", new { Id = courtAreaId })));
            return result;
        }

        public Court Court_GetById(int id)
        {
            return repo.AllReadonly<Court>().Include(x => x.CourtAddress).Where(x => x.Id == id).FirstOrDefault();
        }


        public List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonMeasure(int casePersonId)
        {
            var casePerson = repo.AllReadonly<CasePerson>()
                                 .Where(x => x.Id == casePersonId)
                                 .Select(x => new
                                 {
                                     x.CaseId,
                                     x.FullName
                                 }).FirstOrDefault();
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCase(casePerson.CaseId);
            result.Add(FillBreadcrumbs("Мерки към " + casePerson.FullName, urlHelper.Action("IndexCasePersonMeasure", "CasePerson", new { casePersonId = casePersonId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonDocument(int casePersonId)
        {
            var casePerson = repo.AllReadonly<CasePerson>()
                                 .Where(x => x.Id == casePersonId)
                                 .Select(x => new
                                 {
                                     x.CaseId,
                                     x.FullName
                                 }).FirstOrDefault();
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCase(casePerson.CaseId);
            result.Add(FillBreadcrumbs("Лични документи на " + casePerson.FullName, urlHelper.Action("IndexCasePersonDocument", "CasePerson", new { casePersonId = casePersonId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonLink(int CaseId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCase(CaseId);
            result.Add(FillBreadcrumbs("Връзки", urlHelper.Action("Index", "CasePersonLink", new { id = CaseId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonLinkEdit(int CaseId, int casePersonLinkId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCasePersonLink(CaseId);
            result.Add(FillBreadcrumbs("Редакция на връзка", urlHelper.Action("Edit", "CasePersonLink", new { id = casePersonLinkId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_GetForDocumentPersonLink(long documentId, long documentResolutionId)
        {
            // List<BreadcrumbsVM> result = Breadcrumbs_DocumentEdit(documentId);
            List<BreadcrumbsVM> result = Breadcrumbs_DocumentResolution(documentResolutionId);
            result.Add(FillBreadcrumbs("Връзки", urlHelper.Action("Index", "DocumentPersonLink", new { documentId, documentResolutionId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_GetForDocumentPersonLinkEdit(long documentId, long documentResolutionId, int documentPersonLinkId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_GetForDocumentPersonLink(documentId, documentResolutionId);
            result.Add(FillBreadcrumbs("Връзкa", urlHelper.Action("Edit", "DocumentPersonLink", new { id = documentPersonLinkId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForMediation(int mediationSessionId)
        {
            List<BreadcrumbsVM> result = new();
            var mediationSession = repo.GetById<MediationCaseSession>(mediationSessionId);
            result.Add(FillBreadcrumbs("Връзки", urlHelper.Action("IndexCaseMediation", "Mediation")));
            result.Add(FillBreadcrumbs("Съдебно дело-медиация", urlHelper.Action("CasePreviewMediation", "Mediation", new { id = mediationSession.CaseId })));
            result.Add(FillBreadcrumbs("Среща за медиация", urlHelper.Action("SessionPreviewMediation", "Mediation", new { id = mediationSessionId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_GetForMediationNotificationEdit(int mediationSessionId, int notificationId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_GetForMediation(mediationSessionId);
            result.Add(FillBreadcrumbs("Призовка", urlHelper.Action("Edit", "MediationNotification", new {id= notificationId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForMediationNotificationEditTinyMCE(MediationNotification notification)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_GetForMediationNotificationEdit(notification.MediationCaseSessionId, notification.Id);
            result.Add(FillBreadcrumbs("Редакция бланка " + notification.RegNumber, urlHelper.Action("EditTinyMCE", "MediationNotification", new { id = notification.Id })));
            return result;
        }
        public bool IsExistLawUnit_ByUicUicType(string uic, int? id = null)
        {
            return repo.AllReadonly<LawUnit>()
                       .Any(x => (x.Uic == uic) &&
                                 (x.DateTo == null) &&
                                 ((id != null) ? (x.Id != id) : true));
        }

        public string Users_ValidateEmailLawUnit(string email, int lawUnitId)
        {
            var sameUser = repo.AllReadonly<ApplicationUser>()
                                .Where(x => x.LawUnitId == lawUnitId && x.IsActive)
                                .FirstOrDefault();

            string result = string.Empty;

            if (sameUser != null)
            {
                result += "Вече съществува потребител за избрания служител";
            }

            return result;
        }

        public string Users_GetUserIdByLawunit(int lawUnitId)
        {
            return repo.AllReadonly<ApplicationUser>()
                            .Where(x => x.LawUnitId == lawUnitId && x.IsActive)
                            .Select(x => x.Id)
                            .FirstOrDefault();
        }


        public bool Users_GenerateEissId(string userId)
        {
            try
            {
                var eissId = generateEissId(userId);
                bool isOk = false;
                while (!isOk)
                {
                    isOk = !repo.AllReadonly<ApplicationUser>().Where(x => x.EissId == eissId).Any();

                    if (!isOk)
                    {
                        eissId = generateEissId(Guid.NewGuid().ToString());
                    }
                }

                var userModel = repo.GetById<ApplicationUser>(userId);
                userModel.EissId = eissId;
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }

        private string generateEissId(string key)
        {
            return key.Substring(key.Length - 9).ToUpper();
        }

        /// <summary>
        /// Извличане на данни за адреси
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<AddressVM> Address_Select(AddressFilterVM model)
        {
            return repo.AllReadonly<Address>()
                       .Where(x => (model.AddressTypeId > 0 ? x.AddressTypeId == model.AddressTypeId : true) &&
                                   (!string.IsNullOrEmpty(model.CountryCode) ? x.CountryCode == model.CountryCode : true) &&
                                   (!string.IsNullOrEmpty(model.CityCode) ? x.CityCode == model.CityCode : true))
                       .Select(x => new AddressVM()
                       {
                           Id = x.Id,
                           AddressTypeLabel = x.AddressType.Label,
                           FullAddress = x.FullAddress
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Добавяне/редакция на адрес
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Address_SaveData(Address model)
        {
            try
            {
                nomService.SetFullAddress(model);
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<Address>(model.Id);
                    saved.AddressTypeId = model.AddressTypeId;
                    saved.CountryCode = model.CountryCode;
                    saved.DistrictCode = model.DistrictCode;
                    saved.MunicipalityCode = model.MunicipalityCode;
                    saved.CityCode = model.CityCode;
                    saved.RegionCode = model.RegionCode;
                    saved.StreetCode = model.StreetCode;
                    saved.ForeignAddress = model.ForeignAddress;
                    saved.Block = model.Block;
                    saved.ResidentionAreaCode = model.ResidentionAreaCode;
                    saved.StreetNumber = model.StreetNumber;
                    saved.SubNumber = model.SubNumber;
                    saved.Entrance = model.Entrance;
                    saved.Floor = model.Floor;
                    saved.Appartment = model.Appartment;
                    saved.Phone = model.Phone;
                    saved.Fax = model.Fax;
                    saved.Email = model.Email;
                    saved.Description = model.Description;
                    saved.FullAddress = model.FullAddress;
                    saved.SubBlock = model.SubBlock;

                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    repo.Add<Address>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на адрес Id={model.Id}");
                return false;
            }
        }

        public bool Users_CheckUserByLawUnit(string userId, int lawUnitId)
        {
            return !repo.AllReadonly<ApplicationUser>()
                            .Where(x => x.LawUnitId == lawUnitId && x.Id != userId && x.IsActive)
                            .Any();
        }

        public bool Users_CheckUserByEmail(string userId, string emailAddress)
        {
            return !repo.AllReadonly<ApplicationUser>()
                        .Where(x => (!string.IsNullOrEmpty(userId) ? x.Id != userId : true) &&
                                    (x.Email == emailAddress))
                        .Any();
        }
        public List<BreadcrumbsVM> Breadcrumbs_GetForEisppEvents(int caseId)
        {
            var result = Breadcrumbs_GetForCase(caseId);
            result.Add(FillBreadcrumbs("Събития ЕИСПП", urlHelper.Action("Index", "Eispp", new { caseId = caseId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_GetForEisppEventsCourt(int courtId)
        {
            var result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Събития ЕИСПП", urlHelper.Action("IndexAll", "Eispp", new { courtId = courtId })));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_GetForEisppEventEdit(int caseId, string mode)
        {
            var result = mode == "all" ? Breadcrumbs_GetForEisppEventsCourt(userContext.CourtId) : Breadcrumbs_GetForEisppEvents(caseId);
            result.Add(FillBreadcrumbs("Събитиe ЕИСПП", urlHelper.Action("SendPackage", "Eispp")));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_GetForEisppEventChangeEdit(int caseId, bool isDelete)
        {
            var result = Breadcrumbs_GetForEisppEvents(caseId);
            if (isDelete)
            {
                result.Add(FillBreadcrumbs("ИЗТРИВАНЕ Събитиe ЕИСПП", "PostToEventChange()"));
            }
            else
            {
                result.Add(FillBreadcrumbs("Корекция на Събитиe ЕИСПП", "PostToEventChange()"));
            }
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_GetForEisppEventChangeEditNew(int caseId, bool isDelete)
        {
            var result = Breadcrumbs_GetForEisppEventChangeEdit(caseId, isDelete);
            result.Add(FillBreadcrumbs("Ново събитиe", "#"));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_GetForEisppEventChangeEditOld(int caseId, bool isDelete)
        {
            var result = Breadcrumbs_GetForEisppEventChangeEdit(caseId, isDelete);
            result.Add(FillBreadcrumbs("Преглед старо състояние", "#"));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_GetForEisppEventUnionEdit(int caseId)
        {
            var result = Breadcrumbs_GetForEisppEvents(caseId);
            result.Add(FillBreadcrumbs("ЕИСПП Обединяване на НД", "PostToEventUnion()"));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_GetForEisppEventUnionEditPrincipal(int caseId)
        {
            var result = Breadcrumbs_GetForEisppEventUnionEdit(caseId);
            result.Add(FillBreadcrumbs("Основно НД", "#"));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_GetForEisppEventUnionEditAdded(int caseId)
        {
            var result = Breadcrumbs_GetForEisppEventUnionEdit(caseId);
            result.Add(FillBreadcrumbs("Добавено НД", "#"));
            return result;
        }


        public string Institution_Validate(Institution model)
        {
            if (model.Id > 0)
            {
                return null;
            }
            if (!string.IsNullOrEmpty(model.Code))
                if (repo.AllReadonly<Institution>()
                            .Where(x => x.Code == model.Code && x.InstitutionTypeId == model.InstitutionTypeId)
                            .Any())
                {
                    return $"Вече съществува институция с код {model.Code}";
                }

            return null;
        }

        public string LawUnit_Validate(LawUnit model)
        {
            var dtNow = DateTime.Now;
            if (!string.IsNullOrEmpty(model.Uic))
            {
                if (NomenclatureConstants.LawUnitTypes.EissUserTypes.Contains(model.LawUnitTypeId))
                {
                    if (repo.AllReadonly<LawUnit>()
                               .Where(x => x.Uic == model.Uic && x.UicTypeId == model.UicTypeId && NomenclatureConstants.LawUnitTypes.EissUserTypes.Contains(x.LawUnitTypeId))
                               //.Where(x => (x.DateTo ?? DateTime.MaxValue) >= dtNow)
                               .Where(x => x.Id != model.Id)
                               .Any())
                    {
                        return $"Съществува създаден профил с посочения идентификатор {model.Uic}";
                    }
                }
                else
                {

                    if (repo.AllReadonly<LawUnit>()
                                .Where(x => x.Uic == model.Uic && x.UicTypeId == model.UicTypeId && x.LawUnitTypeId == model.LawUnitTypeId)
                                .Where(x => (x.DateTo ?? DateTime.MaxValue) >= dtNow)
                                .Where(x => x.Id != model.Id)
                                .Any())
                    {
                        return $"Вече съществува лице с идентификатор {model.Code}";
                    }
                }
            }
            return null;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForDocumentInstitutionCaseInfoCase(int caseId)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCase(caseId);
            result.Add(FillBreadcrumbs("Дела др. институции", urlHelper.Action("IndexDocumentInstitutionCaseInfoList", "Document", new { id = caseId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_Document(long documentId)
        {
            var docInfo = repo.AllReadonly<Document>()
                                        .Include(x => x.DocumentType)
                                        .Where(x => x.Id == documentId)
                                        .Select(x =>
                                        $"{x.DocumentType.Label} {x.DocumentNumber}/{x.DocumentDate:dd.MM.yyyy}"
                                        )
                                        .FirstOrDefault();

            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Съдебна регистратура", urlHelper.Action("Index", "Document")));
            result.Add(FillBreadcrumbs(docInfo, urlHelper.Action("View", "Document", new { id = documentId })));

            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_DocumentResolution(long documentResolutionId, string tabName = "")
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Разпореждания по документи", urlHelper.Action("Index", "DocumentResolution")));

            if (documentResolutionId > 0)
            {
                var info = repo.AllReadonly<DocumentResolution>()
                               .Where(x => x.Id == documentResolutionId)
                               .Select(x =>
                               new
                               {
                                   x.DocumentId,
                                   label = (x.RegDate != null) ? $"{x.ResolutionType.Label} {x.RegNumber}/{x.RegDate:dd.MM.yyyy}" : $"{x.ResolutionType.Label}",
                                   caseId = (x.Document.Cases != null) ? x.Document.Cases.Select(s => s.Id).FirstOrDefault() : 0
                               })
                               .FirstOrDefault();

                result.Add(Breadcrumbs_Document(info.DocumentId).Last());
                if (info.caseId > 0)
                {
                    result.Add(Breadcrumbs_GetForCase(info.caseId).Last());
                }
                result.Add(FillBreadcrumbs(info.label, urlHelper.Action("Edit", "DocumentResolution", new { id = documentResolutionId }) + $"#{tabName}"));
            }

            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_DocumentNotification(int documentNotificationId, long documentId, long? documentResolutionId, int notificationTypeId)
        {
            if (documentId <= 0 && documentNotificationId > 0)
            {
                var notification = repo.AllReadonly<DocumentNotification>()
                                       .Where(x => x.Id == documentNotificationId)
                                       .FirstOrDefault();
                if (notification != null)
                {
                    documentId = notification.DocumentId ?? 0;
                    documentResolutionId = notification.DocumentResolutionId;
                    notificationTypeId = notification.NotificationTypeId ?? 0;
                }
            }
            List<BreadcrumbsVM> result = Breadcrumbs_DocumentResolution(documentResolutionId ?? 0);
            if (documentNotificationId > 0)
            {
                result.Add(FillBreadcrumbs("Редакция на известие", urlHelper.Action("Edit", "DocumentNotification", new { id = documentNotificationId })));
            }
            else
            {
                result.Add(FillBreadcrumbs("Добавяне на известие", urlHelper.Action("Add", "DocumentNotification", new { documentId, documentResolutionId, notificationTypeId })));
            }
            return result;
        }
        /// <summary>
        /// Изчитане на банкови сметки за обект
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public IQueryable<BankAccountVM> BankAccount_Select(int sourceType, long sourceId)
        {
            return repo.AllReadonly<BankAccount>()
                       .Where(x => x.SourceType == sourceType && x.SourceId == sourceId)
                       .OrderByDescending(x => x.DateWrt)
                       .Select(x => new BankAccountVM()
                       {
                           Id = x.Id,
                           BIC = x.BIC,
                           IBAN = x.IBAN,
                           BankName = x.BankName,
                           IsMainAccount = x.IsMainAccount,
                       }).AsQueryable();
        }

        /// <summary>
        /// Изчитане на банкова сметка по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BankAccountEditVM BankAccount_GetById(int id)
        {
            return repo.AllReadonly<BankAccount>()
                       .Where(x => x.Id == id)
                       .Select(x => new BankAccountEditVM()
                       {
                           Id = x.Id,
                           SourceId = x.SourceId,
                           SourceType = x.SourceType,
                           BIC = x.BIC,
                           IBAN = x.IBAN,
                           BankName = x.BankName,
                           IsMainAccount = x.IsMainAccount,
                       }).FirstOrDefault();
        }

        /// <summary>
        /// Запис на банкова сметка към обект
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool BankAccount_SaveData(BankAccountEditVM model)
        {
            try
            {
                BankAccount saved = null;
                if (model.Id > 0)
                {
                    saved = repo.GetById<BankAccount>(model.Id);
                }
                else
                {
                    saved = new BankAccount();
                    saved.SourceId = model.SourceId;
                    saved.SourceType = model.SourceType;
                }

                //Ако тази е основа и има други основни - да ги направи другите неосвновни
                if (model.IsMainAccount && (saved.IsMainAccount == false || model.Id == 0))
                {
                    var mainAccounts = repo.All<BankAccount>()
                                         .Where(x => x.SourceId == model.SourceId)
                                         .Where(x => x.SourceType == model.SourceType)
                                         .Where(x => x.IsMainAccount)
                                         .Where(x => x.Id != model.Id)
                                         .ToList();
                    foreach (var item in mainAccounts)
                    {
                        item.IsMainAccount = false;
                        //repo.Update(item);
                    }
                }

                saved.BankName = model.BankName;
                saved.BIC = model.BIC;
                saved.IBAN = model.IBAN;
                saved.IsMainAccount = model.IsMainAccount;

                if (model.Id == 0)
                {
                    repo.Add(saved);
                }

                repo.SaveChanges();
                model.Id = saved.Id;

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при BankAccount_SaveData Id={model.Id}");
            }
            return false;
        }

        public List<BreadcrumbsVM> BankAccount_LoadBreadCrumbs(int sourceType, long sourceId)
        {
            List<BreadcrumbsVM> result = null;
            switch (sourceType)
            {
                case SourceTypeSelectVM.Instutution:

                    var model = repo.AllReadonly<Institution>()
                                    .Where(x => x.Id == ((int)sourceId))
                                    .Select(x => new
                                    {
                                        x.InstitutionTypeId,
                                        x.FullName
                                    }).FirstOrDefault();
                    result = Breadcrumbs_Institution(model.InstitutionTypeId);
                    result.Add(FillBreadcrumbs(model.FullName, urlHelper.Action("Edit", "Institution", new { id = (int)sourceId })));
                    break;
                default:
                    return null;

            };
            return result;
        }

        public List<BreadcrumbsVM> BankAccount_LoadBreadCrumbsAddEdit(int sourceType, long sourceId)
        {
            List<BreadcrumbsVM> result = BankAccount_LoadBreadCrumbs(sourceType, sourceId);
            result.Add(FillBreadcrumbs("Банкови сметки", urlHelper.Action("BankAccount", "Common", new { sourceType = sourceType, sourceId = sourceId })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonSentenceBulletin(int id)
        {
            var model = repo.GetById<CasePersonSentenceBulletin>(id);
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCasePersonSentence(model.CasePersonId);

            result.Add(FillBreadcrumbs("Бюлетин", urlHelper.Action("EditBulletin", "CasePersonSentence", new { id = id })));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_GetForCaseSessionActDivorce(int id)
        {
            var model = repo.GetById<CaseSessionActDivorce>(id);
            List<BreadcrumbsVM> result = Breadcrumbs_GetForCaseSessionAct(model.CaseSessionActId);

            result.Add(FillBreadcrumbs("Съобщение", urlHelper.Action("EditDivorce", "CaseSessionAct", new { id = id })));
            return result;
        }

        private (int? number, string subNumber) GetNumberSubNumber(string text)
        {
            if (string.IsNullOrEmpty(text) == true)
                return (null, null);

            int resultNumber = 0;
            if (int.TryParse(text, out resultNumber))
            {
                return (resultNumber, null);
            }
            else
            {
                string numberFromText = "";
                string textFromText = null;
                for (int i = 0; i < text.Length; i++)
                {
                    if (Char.IsDigit(text[i]))
                        numberFromText += text[i];
                    else
                    {
                        textFromText = i == 0 ? text : text.Replace(numberFromText, "");
                        break;
                    }
                }

                if (string.IsNullOrEmpty(numberFromText) == false && int.TryParse(numberFromText, out resultNumber))
                    return (resultNumber, textFromText);
                else
                    return (null, text);
            }
        }

        public void Address_LocationCorrection(Address model)
        {
            var location = repo.AllReadonly<EkStreet>()
                                    .Where(x => x.Ekatte == model.CityCode && x.Code == model.ResidentionAreaCode)
                                    .FirstOrDefault();
            if (location != null)
            {
                if (location.StreetType == NomenclatureConstants.EkStreetTypes.Street)
                {
                    model.StreetCode = model.ResidentionAreaCode;
                    model.SubNumber = model.SubBlock;

                    try
                    {
                        (model.StreetNumber, model.SubNumber) = GetNumberSubNumber(model.SubNumber);
                        //model.StreetNumber = int.Parse(model.SubNumber);
                        //model.SubNumber = null;
                    }
                    catch { }


                    model.ResidentionAreaCode = null;
                    model.SubBlock = null;
                }
                else
                {
                    try
                    {
                        (model.Block, model.SubBlock) = GetNumberSubNumber(model.SubBlock);

                        //model.Block = int.Parse(model.SubBlock);
                        //model.SubBlock = null;
                    }
                    catch { }
                }
            }

        }

        public CourtLawUnit GetGeneralJudgeCourtLawUnit(int courtId)
        {
            DateTime dateNow = DateTime.Now.Date;
            DateTime dateEnd = dateNow.AddDays(1);

            return repo.AllReadonly<CourtLawUnit>()
                                .Include(x => x.LawUnit)
                                .Where(x => x.CourtId == courtId && x.DateExpired == null)
                                .Where(x => x.LawUnitPositionId == NomenclatureConstants.LawUnitPosition.GeneralJudge)
                                .Where(x => dateNow >= x.DateFrom && dateNow <= (x.DateTo ?? dateEnd))
                                .FirstOrDefault();
        }

        /// <summary>
        /// CourtDepartmentVM.Label - Състав;CourtDepartmentVM.ParentLabel - Отделение
        /// </summary>
        /// <param name="lawunit_id"></param>
        /// <param name="dtNow"></param>
        /// <returns></returns>
        public CourtDepartmentVM Read_LawUnitOtdelenieSystav(int lawunit_id, DateTime? dtNow = null, int? court_id = 0)
        {
            dtNow = dtNow ?? DateTime.Now;
            var result = repo.AllReadonly<CourtDepartmentLawUnit>()
                            .Where(x => x.LawUnitId == lawunit_id)
                            .Where(x => x.DateFrom <= dtNow && (x.DateTo ?? DateTime.MaxValue) >= dtNow)
                            .Where(x => NomenclatureConstants.DepartmentType.RealJudgeDepartments.Contains(x.CourtDepartment.DepartmentTypeId))
                            .Where(x => ((x.CourtDepartment.CourtId == court_id) || (court_id == 0)))
                            .Select(x => x.CourtDepartment)
                            //Първо да извади съставите, отделенията и колегията
                            .OrderByDescending(x => x.DepartmentTypeId)
                            .Select(x => new CourtDepartmentVM
                            {
                                Id = x.Id,
                                Label = x.Label,
                                DepartmentTypeId = x.DepartmentTypeId,
                                ParentId = x.ParentId,
                                ParentLabel = (x.ParentDepartment != null) ? x.ParentDepartment.Label : "",
                                DepartmentTypeLabel = x.DepartmentType.Label
                            }).FirstOrDefault();
            if (result != null)
            {
                if (result.DepartmentTypeId == NomenclatureConstants.DepartmentType.Otdelenie)
                {
                    result.ParentId = result.Id;
                    result.ParentLabel = result.Label;
                    result.Id = 0;
                    result.Label = "";
                }
            }
            return result;
        }
        public bool UpdateCaseJudicalCompositionOtdelenie(int CaseId)
        {
            var judgeRep = repo.All<CaseLawUnit>()
                               .Include(x => x.CourtDepartment)
                               .Where(x => x.CaseId == CaseId &&
                                           x.CaseSessionId == null &&
                                           (((x.DateTo ?? DateTime.Now.AddYears(100)).Date >= DateTime.Now.Date) && (x.DateFrom.Date <= DateTime.Now.Date)) &&
                                           x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                               .OrderByDescending(a => a.DateFrom)
                               .FirstOrDefault();

            if (judgeRep == null)
                return false;


            int? otdelenieId = null;
            int? systavId = null;
            var caseModel = repo.GetById<Case>(CaseId);

            if (judgeRep.CourtDepartmentId > 0)
            {
                var dateNow = DateTime.Now;
                var courtDepartment = repo.AllReadonly<CourtDepartment>()
                                            .Include(x => x.ParentDepartment)
                                            .Where(x => (x.Id == judgeRep.CourtDepartmentId) &&
                                                  ((x.DateFrom.Date <= dateNow.Date) && ((x.DateTo ?? dateNow.AddYears(100)) >= dateNow.Date)))
                                            //За да взема структура само от съда на делото
                                            .Where(x => x.CourtId == caseModel.CourtId)

                                            .FirstOrDefault();
                if (courtDepartment != null)
                {
                    systavId = judgeRep.CourtDepartmentId;
                    if (courtDepartment.DepartmentTypeId == NomenclatureConstants.DepartmentType.Systav)
                    {
                        if (courtDepartment.ParentId > 0)
                        {
                            otdelenieId = courtDepartment.ParentId;
                        }
                    }
                }
            }
            else
            {
                if (caseModel.CourtId == NomenclatureConstants.VKScourtId)
                {
                    //Във ВКС НД, няма състави, а отделението се взема от първото насрочване през графика
                    //Състава и отделението може да се вземат от съдебната структура ако:
                    // Не е във ВКС или не е НД или е от точните видове дела, невлизащи в графика на ВКС
                    var excluded_case_types = SystemParam_SelectIntValues(NomenclatureConstants.SystemParamName.VKS_CaseType_CalendarExclude);
                    if (excluded_case_types.Contains(caseModel.CaseTypeId)
                        || caseModel.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo
                        )
                    {
                        var courtDepartment = Read_LawUnitOtdelenieSystav(judgeRep.LawUnitId, null, caseModel.CourtId);
                        if (courtDepartment != null && userContext.CourtTypeId == NomenclatureConstants.CourtType.VKS)
                        {
                            otdelenieId = courtDepartment.ParentId;
                            //if (courtDepartment.Id > 0)
                            //{
                            //    systavId = courtDepartment.Id;
                            //    judgeRep.CourtDepartmentId = systavId;
                            //}
                            //else
                            //{
                            //    judgeRep.CourtDepartmentId = otdelenieId;
                            //}
                            //repo.Update(judgeRep);
                        }
                    }
                }
            }

            if (systavId == caseModel.JudicalCompositionId && otdelenieId == caseModel.OtdelenieId)
                return false;

            try
            {

                caseModel.JudicalCompositionId = systavId;
                caseModel.OtdelenieId = otdelenieId;
                caseModel.UserId = userContext.UserId;
                caseModel.DateWrt = DateTime.Now;
                //repo.Update(caseModel);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при сетване на състав и отделение на дело с Id={CaseId}");
                return false;
            }
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForExecList()
        {
            List<BreadcrumbsVM> result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Списък изпълнителни листове", urlHelper.Action("ExecListIndex", "Money")));
            return result;
        }

        public List<BreadcrumbsVM> Breadcrumbs_ForExecListEdit(int id)
        {
            List<BreadcrumbsVM> result = Breadcrumbs_ForExecList();
            result.Add(FillBreadcrumbs("Редакция", urlHelper.Action("EditExecList", "Money", new { id = id })));
            return result;
        }

        public IEnumerable<LabelValueVM> Get_Organizations(string term, int? id)
        {
            DateTime dtNow = DateTime.Now;
            Expression<Func<CourtOrganization, bool>> filter = x => true;
            if (id > 0)
            {
                filter = x => x.Id == id;
            }
            else
            {
                filter = x => EF.Functions.ILike(x.Label, term.ToPaternSearch())
                && x.DateFrom <= dtNow && (x.DateTo ?? DateTime.MaxValue) >= dtNow;
            }
            return repo.AllReadonly<CourtOrganization>()
                            .Where(filter)
                            .Where(x => x.CourtId == userContext.CourtId)
                            .OrderBy(x => x.Label)
                            .Select(x => new LabelValueVM
                            {
                                Value = x.Id.ToString(),
                                Label = x.Label
                            }).ToList();
        }

        public IEnumerable<LabelValueVM> Get_CaseReasons(string term, int? caseType, int? id)
        {
            DateTime dtNow = DateTime.Now;
            Expression<Func<CaseReason, bool>> filter = x => true;
            if (id > 0)
            {
                filter = x => x.Id == id;
            }
            else
            {
                var _caseGroup = repo.GetPropById<CaseType, int>(x => x.Id == caseType, x => x.CaseGroupId);
                filter = x => EF.Functions.ILike(x.Label, term.ToPaternSearch()) && x.CaseGroupId == _caseGroup;
            }
            return repo.AllReadonly<CaseReason>()
                            .Where(filter)
                            .Where(x => x.IsActive == true)
                            .OrderBy(x => x.Label)
                            .Select(x => new LabelValueVM
                            {
                                Value = x.Id.ToString(),
                                Label = x.Label
                            }).ToList();
        }

        public string GetFullAddress(Address model, bool setContactData)
        {
            if (model == null) return null;
            return nomService.GetFullAddress(model, setContactData, false, false);
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForVksNotificationList()
        {
            var result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Списък за държавен вестник", urlHelper.Action("Index", "VksNotification")));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForVksNotificationPrint()
        {
            var result = new List<BreadcrumbsVM>();
            result.Add(FillBreadcrumbs("Печат списък за държавен вестник", urlHelper.Action("VksNotificationPrint", "VksNotification")));
            return result;
        }
        public List<BreadcrumbsVM> Breadcrumbs_ForVksNotificationListEdit()
        {
            var result = Breadcrumbs_ForVksNotificationList();
            result.Add(FillBreadcrumbs("Преглед списък за държавен вестник", urlHelper.Action("EditTinyMCE", "VksNotification")));
            return result;
        }

        public IQueryable<InstitutionVM> InstitutionByName_Select(string name, int? id = null)
        {
            DateTime dateNow = DateTime.Now;
            Expression<Func<Institution, bool>> whereActive = x => true;
            if ((id ?? 0) <= 0)
                whereActive = x => (x.DateTo == null || x.DateTo > dateNow);

            Expression<Func<Institution, bool>> whereSelect = x => true;
            if (!string.IsNullOrEmpty(name))
            {
                whereSelect = x => EF.Functions.ILike(x.FullName, name.ToPaternSearch());
            }
            Expression<Func<Institution, bool>> whereId = x => true;
            //Нарочно е оставено и 0 - така се изчиства контрола
            if (id >= 0)
            {
                whereId = x => x.Id == id;
                whereSelect = x => true;
            }

            return repo.AllReadonly<Institution>()
                        .Where(whereId)
                        .Where(whereSelect)
                        .Where(whereActive)
                        .Select(x => new InstitutionVM
                        {
                            Id = x.Id,
                            FullName = x.FullName,
                            Code = x.Code,
                            DateFrom = x.DateFrom,
                            DateTo = x.DateTo,
                            EISPPCode = x.EISPPCode
                        })
                        .AsQueryable();
        }

        public LawUnit Get_LawunitByUserId(string userId)
        {
            return repo.AllReadonly<ApplicationUser>()
                            .Where(x => x.Id == userId)
                            .Select(x => x.LawUnit).FirstOrDefault();
        }

        /// <summary>
        /// Изчитане на темплейтите за статистиката
        /// </summary>
        /// <returns></returns>
        public IQueryable<ExcelReportTemplateVM> ExcelReportTemplate_Select()
        {
            return repo.AllReadonly<ExcelReportTemplate>()
                            .Select(x => new ExcelReportTemplateVM()
                            {
                                Id = x.Id,
                                CourtTypeLabel = x.CourtType.Label,
                                DateFrom = x.DateFrom,
                                DateTo = x.DateTo,
                                Label = x.Label,
                                Description = x.Description,
                                FileName = x.FileName,
                                ReportTypeLabel = x.ReportTypeId == NomenclatureConstants.ExcelReportTemplateReportTypes.Normal ? "Нормална" : "Медиация", //Засега го оставям така. Ако се появи нова статистика си е доста писане
                            }).AsQueryable();
        }

        public bool ExcelReportTemplate_SaveData(ICollection<IFormFile> files, ExcelReportTemplate model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    ExcelReportTemplate saved = repo.GetById<ExcelReportTemplate>(model.Id);
                    saved.CourtTypeId = model.CourtTypeId;
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo;
                    saved.ReportTypeId = model.ReportTypeId;

                    FillDataFile(saved, files);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    FillDataFile(model, files);
                    repo.Add(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Грешка в CommonService.ExcelReportTemplate_SaveData");
                return false;
            }
        }

        private void FillDataFile(ExcelReportTemplate report, ICollection<IFormFile> files)
        {
            if (files != null && files.Any())
            {
                var file = files.First();
                using (var memory = new MemoryStream())
                {
                    file.CopyTo(memory);
                    report.Content = memory.ToArray();
                    report.ContentType = file.ContentType;
                    report.FileName = Path.GetFileName(file.FileName);
                }
            }
        }

        public bool CheckCourtRestriction(int restrictionType, int courtId = 0)
        {
            if (courtId == 0)
            {
                courtId = userContext.CourtId;
            }

            DateTime dtNow = DateTime.Now;
            DateTime dtEnd = DateTime.Now.AddYears(100);

            return repo.AllReadonly<CourtRestriction>()
                            .Where(x => x.CourtId == courtId && x.RestrictionType == restrictionType
                                && (x.DateStart <= dtNow) && ((x.DateEnd ?? dtEnd) >= dtNow))
                            .Any();
        }

        /// <summary>
        /// Извличане на данни за съдилища за падащ списък 
        /// </summary>
        /// <param name="typeId">Идентификатор на тип съд</param>
        /// <param name="addDefaultElement">Флаг дали да се добави елемент "Избери"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_Court(int? typeId, bool addDefaultElement = true)
        {
            Expression<Func<Court, bool>> typeIdWhere = x => true;
            if (typeId != null && typeId > 0)
                typeIdWhere = x => x.CourtTypeId == typeId;

            Expression<Func<Court, bool>> courtIdWhere = x => true;
            if (!userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator))
            {
                courtIdWhere = x => x.Id == userContext.CourtId;
            }

            List<SelectListItem> selectListItems = await repo.AllReadonly<Court>()
                                                             .Where(typeIdWhere)
                                                             .Where(courtIdWhere)
                                                             .OrderBy(x => x.OrderNumber)
                                                             .Select(x => new SelectListItem()
                                                             {
                                                                 Value = x.Id.ToString(),
                                                                 Text = x.Label
                                                             })
                                                             .ToListAsync();

            if (addDefaultElement)
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" }).ToList();

            return selectListItems;
        }

        #region FilterTemplates

        /// <summary>
        /// Метод извличащ данни за шаблони за филтри за справки
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IQueryable<FilterTemplatesListVM> GetFilterTemplates_Select(FilterTemplatesFilterVM filter)
        {
            Expression<Func<FilterTemplates, bool>> filterTemplateTypeIdWhere = x => true;
            if (filter.FilterTemplateTypeId > 0)
                filterTemplateTypeIdWhere = x => x.FilterTemplateTypeId == filter.FilterTemplateTypeId;

            return repo.AllReadonly<FilterTemplates>()
                       .Where(filterTemplateTypeIdWhere)
                       .Select(x => new FilterTemplatesListVM()
                       {
                           Id = x.Id,
                           FilterTemplateTypeLabel = x.FilterTemplateType.Label,
                           Label = x.Label,
                           IsActiveLabel = x.IsActive ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Извличане на данни за шаблон за филтър на специализирана справка
        /// </summary>
        /// <param name="id">Идентификатор на запис</param>
        /// <returns></returns>
        public async Task<FilterTemplatesSpecializedReportEditVM> GetFilterTemplatesSpecializedReportById(int id)
        {
            return await repo.AllReadonly<FilterTemplates>()
                             .Where(x => x.Id == id)
                             .Select(x => new FilterTemplatesSpecializedReportEditVM()
                             {
                                 Id = x.Id,
                                 FilterTemplateTypeId = x.FilterTemplateTypeId,
                                 IsActive = x.IsActive,
                                 Label = x.Label,
                                 SpecializedReportFilter = JsonConvert.DeserializeObject<SpecializedReportFilterVM>(x.Data)
                             })
                             .FirstAsync()
                             .ConfigureAwait(false);
        }

        /// <summary>
        /// Метод извличащ записаният филтър за специализирана справка
        /// </summary>
        /// <param name="id">Идентификатор на шаблона</param>
        /// <returns></returns>
        public async Task<SpecializedReportFilterVM> GetFilterTemplatesSpecializedReportDataById(int id)
        {
            return await repo.AllReadonly<FilterTemplates>()
                             .Where(x => x.Id == id)
                             .Select(x => JsonConvert.DeserializeObject<SpecializedReportFilterVM>(x.Data))
                             .FirstAsync()
                             .ConfigureAwait(false);
        }

        /// <summary>
        /// Метод попълващ обект за запис на шаблон за филтър за специализирана справка
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private FilterTemplates FillFilterTemplatesSpecializedReport(FilterTemplatesSpecializedReportEditVM model)
        {
            return new FilterTemplates()
            {
                FilterTemplateTypeId = model.FilterTemplateTypeId,
                Label = model.Label,
                IsActive = model.IsActive,
                Data = JsonConvert.SerializeObject(model.SpecializedReportFilter),
                UserId = userContext.UserId,
                DateWrt = DateTime.Now
            };
        }

        /// <summary>
        /// Метод сетващ стойности за запис при редакция на шаблон за филтър за специализирана справка
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <param name="modelSave">Модел за запис</param>
        private void SetFieldsFilterTemplatesSpecializedReport(FilterTemplatesSpecializedReportEditVM model, FilterTemplates modelSave)
        {
            modelSave.FilterTemplateTypeId = model.FilterTemplateTypeId;
            modelSave.Label = model.Label;
            modelSave.IsActive = model.IsActive;
            modelSave.Data = JsonConvert.SerializeObject(model.SpecializedReportFilter);
            modelSave.UserId = userContext.UserId;
            modelSave.DateWrt = DateTime.Now;
        }

        /// <summary>
        /// Метод за запис на шаблон за филтър за специализирана справка
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<int?> SaveFilterTemplatesSpecializedReport(FilterTemplatesSpecializedReportEditVM model)
        {
            try
            {
                FilterTemplates modelSave = model.Id != null ? await repo.All<FilterTemplates>()
                                                                         .Where(x => x.Id == model.Id)
                                                                         .FirstAsync()
                                                                         .ConfigureAwait(false) : FillFilterTemplatesSpecializedReport(model);

                if (model.Id != null)
                {
                    SetFieldsFilterTemplatesSpecializedReport(model, modelSave);
                }
                else
                {
                    repo.Add(modelSave);
                }

                await repo.SaveChangesAsync().ConfigureAwait(false);
                return modelSave.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на шаблон за специализирана справка");
                return null;
            }
        }

        /// <summary>
        /// Метод зареждащ шаблони за филтри за справки
        /// </summary>
        /// <param name="filterTemplateTypeId">Идентификатор на тип шаблон</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetFilterTemplates_SelectDDL(int filterTemplateTypeId)
        {
            var result = await repo.AllReadonly<FilterTemplates>()
                                   .Where(x => x.FilterTemplateTypeId == filterTemplateTypeId &&
                                               x.IsActive)
                                   .OrderBy(x => x.Label)
                                   .Select(x => new SelectListItem()
                                   {
                                       Text = x.Label,
                                       Value = x.Id.ToString()
                                   })
                                   .ToListAsync()
                                   .ConfigureAwait(false) ?? new List<SelectListItem>();

            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }

        #endregion
    }
}
