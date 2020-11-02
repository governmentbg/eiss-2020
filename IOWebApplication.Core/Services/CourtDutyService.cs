// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IOWebApplication.Core.Services
{
    public class CourtDutyService : BaseService, ICourtDutyService
    {
        private readonly ICommonService commonService;
        private readonly ICourtLoadPeriodService courtLoadPeriodService;

        public CourtDutyService(
            ILogger<CourtDutyService> _logger,
            ICommonService _commonService,
            IRepository _repo,
            ICourtLoadPeriodService _courtLoadPeriodService)
        {
            logger = _logger;
            repo = _repo;
            commonService = _commonService;
            courtLoadPeriodService = _courtLoadPeriodService;
        }

        /// <summary>
        /// Извличане на данни за Дежурства към съд
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public IQueryable<CourtDutyVM> CourtDuty_Select(int courtId, string label)
        {
            label = label?.ToLower();
            return repo.AllReadonly<CourtDuty>()
                       .Include(x => x.Court)
                       .Where(x => x.CourtId == courtId)
                       .Select(x => new CourtDutyVM()
                       {
                           Id = x.Id,
                           CourtLabel = x.Court.Label,
                           Label = x.Label,
                           Description = x.Description,
                           DateFrom = x.DateFrom,
                           DateTo = x.DateTo,
                           CountLawUnit = x.CourtDutyLawUnits.Where(a => a.DateTo == null).Count()
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Запис на Дежурства към съд
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CourtDuty_SaveData(CourtDuty model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CourtDuty>(model.Id);
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.ActNomer = model.ActNomer;
                    saved.ActDate = model.ActDate;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    repo.Add<CourtDuty>(model);
                    repo.SaveChanges();
                    //courtLoadPeriodService.GetLoadPeriod(null, model.Id);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CourtDuty Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за Съдии към дежурство
        /// </summary>
        /// <param name="dutyId"></param>
        /// <returns></returns>
        public IList<CourtDutyLawUnit> CourtDutyLowUnit_Select(int dutyId)
        {
            return repo.AllReadonly<CourtDutyLawUnit>()
                .Where(x => x.CourtDutyId == dutyId)
                .Select(x => x)
                .ToList();
        }

        /// <summary>
        /// Избор на съдии към дежурство за чекбокс
        /// </summary>
        /// <param name="law"></param>
        /// <param name="courtDutyLawUnits"></param>
        /// <returns></returns>
        private CheckListVM FillCheckListVM(LawUnit law, IList<CourtDutyLawUnit> courtDutyLawUnits)
        {
            CheckListVM checkListVM = new CheckListVM
            {
                Value = law.Id.ToString(),
                Label = law.FullName,
                Checked = courtDutyLawUnits.Any(x => x.LawUnitId == law.Id && (x.DateFrom <= DateTime.Now && ((x.DateTo != null) ? x.DateTo >= DateTime.Now : true)))
            };

            return checkListVM;
        }

        /// <summary>
        /// Избор на съдии за чекбокс
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="dutyId"></param>
        /// <returns></returns>
        private IList<CheckListVM> FillCheckListVMs(int courtId, int dutyId)
        {
            IList<CheckListVM> checkListVMs = new List<CheckListVM>();

            var courtDutyLowUnits = CourtDutyLowUnit_Select(dutyId);
            IQueryable<LawUnit> lawUnits = commonService.LawUnit_JudgeByCourtDate(courtId, DateTime.Now);

            foreach (var law in lawUnits)
                checkListVMs.Add(FillCheckListVM(law, courtDutyLowUnits));

            return checkListVMs.OrderBy(x => x.Label).ToList();
        }

        /// <summary>
        /// Избор на съдии към дежурство
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="dutyId"></param>
        /// <returns></returns>
        public CheckListViewVM CheckListViewVM_Fill(int courtId, int dutyId)
        {
            var courtDuty = repo.GetById<CourtDuty>(dutyId);
            CheckListViewVM checkListViewVM = new CheckListViewVM
            {
                CourtId = courtId,
                ObjectId = dutyId,
                Label = "Изберете съдии към дежурство " + courtDuty.Label,
                checkListVMs = FillCheckListVMs(courtId, dutyId)
            };

            return checkListViewVM;
        }

        /// <summary>
        /// Запис на Съдии към дежурство
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CourtDutyLawUnit_SaveData(CheckListViewVM model)
        {
            #region Попълване на данните за запис
            List<CourtDutyLawUnit> courtDutyLowUnits = CourtDutyLowUnit_Select(model.ObjectId).ToList();

            // сетване на всички до днешна дата без 1 секунда
            courtDutyLowUnits.ToList().ForEach(x => x.DateTo = DateTime.Now.AddSeconds(-1));

            List<CourtDutyLawUnit> courtDutyLawUnitNews = new List<CourtDutyLawUnit>();

            // въртя списъка с елементи визуализирани на екрана
            foreach (var check in model.checkListVMs)
            {
                // търси елемента от екрана в списъка с записани елементи
                var court = courtDutyLowUnits.Where(x => x.LawUnitId == int.Parse(check.Value)).DefaultIfEmpty(null).FirstOrDefault();

                if (court != null)
                {
                    // ако не го е намерил

                    if (check.Checked)
                    {
                        //ако е маркирано в екрана
                        if ((court.DateTo != null) && (court.DateTo <= DateTime.Now))
                            court.DateTo = null;
                    }
                    else
                    {
                        // ако не е маркирано в екрана
                        if (court.DateTo == null)
                            court.DateTo = DateTime.Now.AddSeconds(-1);
                    }
                }
                else
                {
                    // ако е намерен в записаните елементи
                    if (check.Checked)
                    {
                        // ако е маркиран
                        CourtDutyLawUnit courtDepartmentLawUnit = new CourtDutyLawUnit
                        {
                            CourtDutyId = model.ObjectId,
                            LawUnitId = int.Parse(check.Value)
                        };

                        courtDutyLawUnitNews.Add(courtDepartmentLawUnit);
                    }
                }
            }

            courtDutyLowUnits.AddRange(courtDutyLawUnitNews);
            #endregion

            #region Запис на данните

            return SaveLawUnit(courtDutyLowUnits);

            #endregion
        }

        /// <summary>
        /// Запис на Съдии към дежурство
        /// </summary>
        /// <param name="courtDutyLawUnits"></param>
        /// <returns></returns>
        public bool SaveLawUnit(List<CourtDutyLawUnit> courtDutyLawUnits)
        {
            try
            {
                foreach (var unit in courtDutyLawUnits)
                {
                    if (unit.Id > 0)
                        repo.Update(unit);
                    else
                        repo.Add<CourtDutyLawUnit>(unit);
                }

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logger.log(ex)
                return false;
            }
        }

        /// <summary>
        /// Избор на Дежурства към съд за комбо бокс
        /// </summary>
        /// <param name="courtId"></param>
        /// <returns></returns>
        public List<SelectListItem> CourtDuty_SelectForDropDownList(int courtId)
        {
            DateTime dateTomorrow = DateTime.Now.AddDays(1).Date;

            var result = repo.AllReadonly<CourtDuty>()
                .Where(x => x.CourtId == courtId && (x.DateTo ?? dateTomorrow) > DateTime.Now)
                 .OrderBy(x => x.Label)
                                 .Select(x => new SelectListItem()
                                 {
                                     Value = x.Id.ToString(),
                                     Text = x.Label + "; Период от " + x.DateFrom.ToString("dd.MM.yyyy HH:mm") + 
                                                     (x.DateTo == null ? "" : (" до " + (x.DateTo??DateTime.Now).ToString("dd.MM.yyyy HH:mm"))) 
                                 }).ToList();
      if (result.Count==0)
      {
        result.Insert(0, new SelectListItem() { Text = "Няма активно дежурство", Value = "-1" });
      }
          

            return result;
        }
    }
}
