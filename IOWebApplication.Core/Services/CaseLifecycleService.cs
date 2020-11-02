// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class CaseLifecycleService : BaseService, ICaseLifecycleService
    {
        public CaseLifecycleService(
        ILogger<CaseLifecycleService> _logger,
        IRepository _repo,
        IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }

        /// <summary>
        /// Обръщане на арабско в римско число
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private string ToRoman(int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
            if (number < 1) return string.Empty;
            if (number >= 1000) return "M" + ToRoman(number - 1000);
            if (number >= 900) return "CM" + ToRoman(number - 900);
            if (number >= 500) return "D" + ToRoman(number - 500);
            if (number >= 400) return "CD" + ToRoman(number - 400);
            if (number >= 100) return "C" + ToRoman(number - 100);
            if (number >= 90) return "XC" + ToRoman(number - 90);
            if (number >= 50) return "L" + ToRoman(number - 50);
            if (number >= 40) return "XL" + ToRoman(number - 40);
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            throw new ArgumentOutOfRangeException("something bad happened");
        }

        /// <summary>
        /// Зареждане на данни за интервали
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public IQueryable<CaseLifecycleVM> CaseLifecycle_Select(int CaseId)
        {
            return repo.AllReadonly<CaseLifecycle>()
                       .Include(x => x.Case)
                       .Include(x => x.LifecycleType)
                       .Where(x => x.CaseId == CaseId)
                       .Select(x => new CaseLifecycleVM()
                       {
                           Id = x.Id,
                           CaseId = x.CaseId,
                           LifecycleTypeLabel = (x.LifecycleType != null) ? x.LifecycleType.Label : string.Empty,
                           LifecycleTypeId = x.LifecycleTypeId,
                           DateFrom = x.DateFrom,
                           DateTo = x.DateTo,
                           Iteration = x.Iteration,
                           IterationText = ToRoman(x.Iteration) + " интервал",
                           DurationMonths = x.DurationMonths,
                           ModelEdit = (x.LifecycleTypeId != NomenclatureConstants.LifecycleType.InProgress)
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Изчисляване на срок от дати
        /// </summary>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <returns></returns>
        public int CalcMonthFromDateFromDateTo(DateTime DateFrom, DateTime DateTo)
        {
            var isLastDayFromMonthFrom = (DateFrom.Day == DateTime.DaysInMonth(DateFrom.Year, DateFrom.Month));
            var isLastDayFromMonthTo = (DateTo.Day == DateTime.DaysInMonth(DateTo.Year, DateTo.Month));

            var monthCount = ((DateTo.Year - DateFrom.Year) * 12) + DateTo.Month - DateFrom.Month;

            if (DateTo.Month == DateFrom.Month)
                monthCount += 1;

            if (DateTo.Month != DateFrom.Month)
            {
                if (DateTo.Day > DateFrom.Day)
                {
                    if (!isLastDayFromMonthFrom || !isLastDayFromMonthTo)
                        monthCount += 1;
                }
            }

            return monthCount;
        }

        /// <summary>
        /// Изчисляване на срок по дело и интервал
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="Iteration"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="CaseLifecycleIterations"></param>
        /// <returns></returns>
        private int ClacMonth(int CaseId, int Iteration, DateTime DateFrom, DateTime DateTo, List<CaseLifecycle> CaseLifecycleIterations = null)
        {
            var caseLifecycles = CaseLifecycleIterations == null ? repo.AllReadonly<CaseLifecycle>().Where(x => x.CaseId == CaseId && x.Iteration == Iteration).ToList() : CaseLifecycleIterations;
            var daysProgres = (DateTo.Date - DateFrom.Date).TotalDays;
            double daysStop = 0;

            foreach (var caseLifecycle in caseLifecycles.Where(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.Stop))
                daysStop += ((caseLifecycle.DateTo ?? caseLifecycle.DateFrom).Date - caseLifecycle.DateFrom.Date).TotalDays;

            var dateEnd = DateFrom.AddDays(daysProgres - daysStop);

            return CalcMonthFromDateFromDateTo(DateFrom, dateEnd);
        }

        /// <summary>
        /// Запис на интервал
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseLifecycle_SaveData(CaseLifecycle model)
        {
            try
            {
                var caseLifecycles = repo.AllReadonly<CaseLifecycle>().Where(x => x.CaseId == model.CaseId);

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLifecycle>(model.Id);
                    saved.LifecycleTypeId = model.LifecycleTypeId;
                    saved.Iteration = model.Iteration;
                    saved.Description = model.Description;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo;

                    if (saved.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress && saved.DateTo != null)
                        saved.DurationMonths = ClacMonth(saved.CaseId, saved.Iteration, saved.DateFrom, saved.DateTo ?? saved.DateFrom);
                    else
                        saved.DurationMonths = model.DurationMonths;

                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;

                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    repo.Add<CaseLifecycle>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на интервал по дело Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Запис на първи интервал
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public bool CaseLifecycle_SaveFirst(int CaseId, DateTime dateTime)
        {
            try
            {
                var caseLifecycle = repo.AllReadonly<CaseLifecycle>().Where(x => x.CaseId == CaseId && x.Iteration == 1 && x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress).FirstOrDefault();
                if (caseLifecycle != null)
                {
                    caseLifecycle.DateFrom = dateTime;
                    caseLifecycle.UserId = userContext.UserId;
                    caseLifecycle.DateWrt = DateTime.Now;

                    if (caseLifecycle.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress && caseLifecycle.DateTo != null)
                        caseLifecycle.DurationMonths = ClacMonth(caseLifecycle.CaseId, caseLifecycle.Iteration, caseLifecycle.DateFrom, caseLifecycle.DateTo ?? caseLifecycle.DateFrom);

                    repo.Update<CaseLifecycle>(caseLifecycle);
                    repo.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на интервал по дело CaseId={ CaseId }");
                return false;
            }
        }

        /// <summary>
        /// Запис на първи интервал по тип дело
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        public bool CaseLifecycle_SaveFirst_ForCaseType(int CaseSessionId)
        {
            var caseSession = repo.AllReadonly<CaseSession>()
                                  .Include(x => x.Case)
                                  .Include(x => x.CaseSessionResults)
                                  .Where(x => x.Id == CaseSessionId)
                                  .FirstOrDefault();

            if (caseSession == null)
            {
                return true;
            }
            else
            {
                if (caseSession.Case.CaseTypeId != NomenclatureConstants.CaseTypes.ChGD &&
                    caseSession.Case.CaseTypeId != NomenclatureConstants.CaseTypes.GD &&
                    caseSession.Case.CaseTypeId != NomenclatureConstants.CaseTypes.TD &&
                    caseSession.Case.CaseTypeId != NomenclatureConstants.CaseTypes.ChTD)
                    return true;
            }

            var caseSessionAct = repo.AllReadonly<CaseSessionAct>()
                                     .Where(x => x.CaseSessionId == CaseSessionId &&
                                                 x.ActStateId == NomenclatureConstants.SessionActState.Enforced &&
                                                 x.RegDate != null)
                                     .FirstOrDefault();

            if (caseSessionAct == null)
                return true;

            var isSessionAndResult = ((caseSession.CaseSessionResults.Any(x => x.SessionResultId == NomenclatureConstants.CaseSessionResult.ScheduledFirstSession)) &&
                                      (caseSession.SessionStateId == NomenclatureConstants.SessionState.Provedeno));

            if (!isSessionAndResult)
                return true;

            return CaseLifecycle_SaveFirst(caseSession.CaseId, (caseSessionAct.RegDate ?? DateTime.Now));
        }

        /// <summary>
        /// Затваряне на интервал
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="CaseSessionActId"></param>
        /// <param name="DateToLifeCycle"></param>
        /// <returns></returns>
        public bool CaseLifecycle_CloseInterval(int CaseId, int CaseSessionActId, DateTime DateToLifeCycle)
        {
            try
            {
                // Изчита всички интервали за делото
                var caseLifecycles = repo.AllReadonly<CaseLifecycle>()
                                         .Where(x => x.CaseId == CaseId)
                                         .ToList();

                // Търси главен интервал, които не е затворен и ако са повече от един взема първият (не би трянвало да са повече от един)
                var caseLifecycle = caseLifecycles.Where(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                              x.DateTo == null)
                                                  .OrderBy(x => x.DateFrom)
                                                  .FirstOrDefault();

                if (caseLifecycle != null)
                {
                    var caseLifecyclesStop = caseLifecycles.Where(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.Stop &&
                                                                       x.Iteration == caseLifecycle.Iteration &&
                                                                       x.DateTo == null)
                                                           .ToList() ?? new List<CaseLifecycle>();

                    foreach (var lifecycleStop in caseLifecyclesStop)
                    {
                        lifecycleStop.DateTo = DateToLifeCycle;
                        lifecycleStop.DateWrt = DateTime.Now;
                        lifecycleStop.UserId = userContext.UserId;

                        repo.Update(lifecycleStop);
                    }

                    caseLifecycle.CaseSessionActId = CaseSessionActId;
                    caseLifecycle.DurationMonths = ClacMonth(CaseId, caseLifecycle.Iteration, caseLifecycle.DateFrom, DateToLifeCycle, caseLifecyclesStop);
                    caseLifecycle.DateTo = DateToLifeCycle;
                    caseLifecycle.DateWrt = DateTime.Now;
                    caseLifecycle.UserId = userContext.UserId;
                    repo.Update(caseLifecycle);
                    repo.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при затваряне на интервал по дело Id={ CaseId }");
                return false;
            }
        }

        /// <summary>
        /// Отваряне на затворен интервал по акт
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="CaseSessionActId"></param>
        /// <returns></returns>
        public bool CaseLifecycle_UndoCloseInterval(int CaseId, int CaseSessionActId)
        {
            try
            {
                // Изчита всички интервали за делото
                var caseLifecycles = repo.AllReadonly<CaseLifecycle>()
                                         .Where(x => x.CaseId == CaseId)
                                         .ToList();

                // Търси главен интервал, които не е затворен и ако са повече от един взема първият (не би трянвало да са повече от един)
                var caseLifecycle = caseLifecycles.Where(x => x.CaseSessionActId == CaseSessionActId)
                                                  .FirstOrDefault();

                if (caseLifecycle != null)
                {
                    if (!caseLifecycles.Any(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                 x.Id != caseLifecycle.Id &&
                                                 x.DateFrom > caseLifecycle.DateFrom))
                    {
                        caseLifecycle.CaseSessionActId = null;
                        caseLifecycle.DurationMonths = 0;
                        caseLifecycle.DateTo = null;
                        caseLifecycle.DateWrt = DateTime.Now;
                        caseLifecycle.UserId = userContext.UserId;
                        repo.Update(caseLifecycle);
                        repo.SaveChanges();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при затваряне на интервал по дело Id={ CaseId }");
                return false;
            }
        }

        /// <summary>
        /// Проверка дали съществува интервал след този затворен с акт
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="CaseSessionActId"></param>
        /// <returns></returns>
        public bool CaseLifecycle_IsExistLifcycleAfter(int CaseId, int CaseSessionActId)
        {
            var caseLifecycles = repo.AllReadonly<CaseLifecycle>()
                                         .Where(x => x.CaseId == CaseId)
                                         .ToList();

            var caseLifecycle = caseLifecycles.Where(x => x.CaseSessionActId == CaseSessionActId)
                                                  .FirstOrDefault();

            return caseLifecycle != null ? caseLifecycles.Any(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                   x.Id != caseLifecycle.Id &&
                                                                   x.DateFrom > caseLifecycle.DateFrom) : false;
        }

        /// <summary>
        /// Запис на нов главен интервал
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="DateFromLifeCycle"></param>
        /// <returns></returns>
        public bool CaseLifecycle_NewIntervalSave(int CaseId, DateTime DateFromLifeCycle)
        {
            try
            {
                // Изчита всички интервали за делото
                var caseLifecycles = repo.AllReadonly<CaseLifecycle>()
                                         .Where(x => x.CaseId == CaseId)
                                         .ToList();

                if (!caseLifecycles.Any(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                             x.DateTo == null))
                {
                    repo.Add(new CaseLifecycle()
                    {
                        CourtId = userContext.CourtId,
                        CaseId = CaseId,
                        LifecycleTypeId = NomenclatureConstants.LifecycleType.InProgress,
                        Iteration = caseLifecycles.Max(x => x.Iteration) + 1,
                        DateFrom = DateFromLifeCycle,
                        DateWrt = DateTime.Now,
                        UserId = userContext.UserId
                    });
                    repo.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при стартиране на интервал по дело Id={ CaseId }");
                return false;
            }
        }

        /// <summary>
        /// Изчисляване на срок до дата
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="DateTo"></param>
        /// <returns></returns>
        public int GetCalcMonthToDate(int CaseId, DateTime DateTo)
        {
            var caseLifecycles = repo.AllReadonly<CaseLifecycle>()
                                     .Where(x => x.CaseId == CaseId &&
                                                 x.DateFrom < DateTo)
                                     .ToList() ?? new List<CaseLifecycle>();
            
            var result = caseLifecycles.Where(x => (x.DateTo ?? DateTime.Now.AddYears(100)) <= DateTo && x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress).Sum(x => x.DurationMonths);
            var caseLifecycle = caseLifecycles.Where(x => (x.DateTo ?? DateTime.Now.AddYears(100)) > DateTo && x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress).FirstOrDefault();
            if (caseLifecycle != null)
            {
                var lifecycles = caseLifecycles.Where(x => x.Iteration == caseLifecycle.Iteration).ToList();
                foreach (var lifecycle in lifecycles.Where(x => x.LifecycleTypeId != NomenclatureConstants.LifecycleType.InProgress))
                {
                    if (lifecycle.DateTo == null)
                        lifecycle.DateTo = DateTo;
                    else
                    {
                        if (lifecycle.DateTo > DateTo)
                            lifecycle.DateTo = DateTo;
                    }    
                }

                result += ClacMonth(CaseId, caseLifecycle.Iteration, caseLifecycle.DateFrom, DateTo, lifecycles);
            }

            return result;
        }

        /// <summary>
        /// Проверка дали всички интервали са затворени
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public bool CaseLifecycle_IsAllLifcycleClose(int CaseId)
        {
            return !repo.AllReadonly<CaseLifecycle>()
                        .Any(x => x.CaseId == CaseId &&
                                  x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                  x.DateTo == null);
        }
    }
}
