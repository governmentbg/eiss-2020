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
using System.Text;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Constants;
using System.Linq.Expressions;

namespace IOWebApplication.Core.Services
{
    public class CourtDepartmentService : BaseService, ICourtDepartmentService
    {
        private readonly ICommonService commonService;

        public CourtDepartmentService(
            ILogger<CourtDepartmentService> _logger,
            ICommonService _commonService,
            IRepository _repo)
        {
            logger = _logger;
            repo = _repo;
            commonService = _commonService;
        }

        /// <summary>
        /// Изчитане на всичко от CourtDepartment
        /// </summary>
        /// <param name="courtId">Ид на съд</param>
        /// <param name="label"></param>
        /// <returns></returns>
        public IQueryable<CourtDepartmentVM> CourtDepartment_Select(int courtId, string label)
        {
            label = label?.ToLower();
            return repo.AllReadonly<CourtDepartment>()
                .Include(x => x.CaseGroup)
                .Include(x => x.DepartmentType)
                .Include(x => x.ParentDepartment)
                .Include(x => x.Court)
                .Where(x => x.CourtId == courtId)
                .Select(x => new CourtDepartmentVM()
                {
                    Id = x.Id,
                    Label = x.Label,
                    CourtLabel = x.Court.Label,
                    CaseGroupLabel = (x.CaseGroup != null) ? x.CaseGroup.Label : string.Empty,
                    ParentLabel = (x.ParentDepartment != null) ? x.ParentDepartment.Label : x.Court.Label,
                    DepartmentTypeLabel = (x.DepartmentType != null) ? x.DepartmentType.Label : string.Empty,
                    MasterId = x.MasterId,
                    ParentId = (x.ParentId ?? 0)
                })
                .AsQueryable();
        }

        /// <summary>
        /// Запис на CourtDepartment
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CourtDepartment_SaveData(CourtDepartment model)
        {
            try
            {
                if (model.ParentId != null)
                {
                    model.CaseGroupId = repo.GetById<CourtDepartment>(model.ParentId).CaseGroupId;
                }

                if (model.DepartmentTypeId != NomenclatureConstants.DepartmentType.Systav)
                    model.CaseInstanceId = null;

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CourtDepartment>(model.Id);
                    saved.Label = model.Label;
                    saved.ParentId = model.ParentId;
                    saved.MasterId = (model.ParentId == null) ? model.Id : repo.GetById<CourtDepartment>(model.ParentId).MasterId;
                    saved.Description = model.Description;
                    saved.DepartmentTypeId = model.DepartmentTypeId;
                    saved.CaseGroupId = model.CaseGroupId;
                    saved.CaseInstanceId = model.CaseInstanceId;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    model.MasterId = (model.ParentId == null) ? model.Id : repo.GetById<CourtDepartment>(model.ParentId).MasterId;
                    repo.Add<CourtDepartment>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Съдебни нива Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Функция която връща списък на елементите (родителски) за попълване на комбо
        /// </summary>
        /// <param name="courtId">Ид на съд</param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownList(int courtId)
        {
            var courtDepartments = repo.All<CourtDepartment>()
                                       .Where(x => x.CourtId == courtId &&
                                                   x.DepartmentTypeId != NomenclatureConstants.DepartmentType.Systav)
                                       .ToList();

            var result = new List<SelectListItem>();

            foreach (var courtDepartment in courtDepartments.OrderBy(x => x.MasterId).ThenBy(x => (x.ParentId ?? 0)).ThenBy(x => x.Label))
            {
                var selectList = new SelectListItem()
                {
                    Text = courtDepartment.Label,
                    Value = courtDepartment.Id.ToString(),

                };
                result.Add(selectList);
            };

            var court = repo.GetById<Court>(courtId);

            result = result.Prepend(new SelectListItem() { Text = court.Label, Value = "0" })
                    .ToList();

            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });

            return result;
        }

        /// <summary>
        /// Изчитане на данни от CourtDepartmentLawUnit
        /// </summary>
        /// <param name="departmentId">Ид на департамент</param>
        /// <returns></returns>
        public IList<CourtDepartmentLawUnit> CourtDepartmentLowUnit_Select(int departmentId)
        {
            return repo.AllReadonly<CourtDepartmentLawUnit>()
                .Where(x => x.CourtDepartmentId == departmentId)
                .Select(x => x)
                .ToList();
        }

        /// <summary>
        /// Попълване на елемент за чек бокса
        /// </summary>
        /// <param name="law"></param>
        /// <param name="courtDepartmentLawUnits"></param>
        /// <returns></returns>
        private CheckListVM FillCheckListVM(LawUnit law, IList<CourtDepartmentLawUnit> courtDepartmentLawUnits)
        {
            CheckListVM checkListVM = new CheckListVM
            {
                Value = law.Id.ToString(),
                Label = law.FullName,
                Checked = courtDepartmentLawUnits.Any(x => x.LawUnitId == law.Id && (x.DateFrom <= DateTime.Now && ((x.DateTo != null) ? x.DateTo >= DateTime.Now : true)))
            };

            return checkListVM;
        }

        /// <summary>
        /// Връща списък с елементите за чекбокса
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        private IList<CheckListVM> FillCheckListVMs(int courtId, int departmentId)
        {
            IList<CheckListVM> checkListVMs = new List<CheckListVM>();

            var courtDepartmentLowUnits = CourtDepartmentLowUnit_Select(departmentId);
            IQueryable<LawUnit> lawUnits = commonService.LawUnit_JudgeByCourtDate(courtId, DateTime.Now);

            foreach (var law in lawUnits)
                checkListVMs.Add(FillCheckListVM(law, courtDepartmentLowUnits));

            return checkListVMs.OrderBy(x => x.Label).ToList();
        }

        /// <summary>
        /// Попълване на обекта нужен за вюто с чек боксовете
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        public CheckListViewVM CheckListViewVM_Fill(int courtId, int departmentId)
        {
            var courtDepartment = repo.GetById<CourtDepartment>(departmentId);
            var departmentType = repo.GetById<DepartmentType>(courtDepartment.DepartmentTypeId);
            CheckListViewVM checkListViewVM = new CheckListViewVM
            {
                CourtId = courtId,
                ObjectId = departmentId,
                Label = "Изберете съдии към " + departmentType.Label + "-" + courtDepartment.Label,
                checkListVMs = FillCheckListVMs(courtId, departmentId)
            };

            return checkListViewVM;
        }

        /// <summary>
        /// Формиране на списък и извикване на функцията за запис в CourtDepartmentLawUnit
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CourtDepartmentLawUnit_SaveData(CheckListViewVM model)
        {
            #region Попълване на данните за запис
            List<CourtDepartmentLawUnit> courtDepartmentLowUnits = CourtDepartmentLowUnit_Select(model.ObjectId).ToList();

            // сетване на всички до днешна дата без 1 секунда
            // courtDepartmentLowUnits.ToList().ForEach(x => x.DateTo = DateTime.Now.AddSeconds(-1));

            List<CourtDepartmentLawUnit> courtDepartmentLawUnitNews = new List<CourtDepartmentLawUnit>();

            // въртя списъка с елементи визуализирани на екрана
            foreach (var check in model.checkListVMs)
            {
                // търси елемента от екрана в списъка с записани елементи
                var court = courtDepartmentLowUnits.Where(x => x.LawUnitId == int.Parse(check.Value))
                                                          .Where(x => x.DateTo == null)
                                                           .DefaultIfEmpty(null).FirstOrDefault();

                if (court != null)
                {
                    // ако  го е намерил

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
                    // ако ne go е намерен в записаните елементи
                    if (check.Checked)
                    {
                        // ако е маркиран
                        CourtDepartmentLawUnit courtDepartmentLawUnit = new CourtDepartmentLawUnit
                        {
                            CourtDepartmentId = model.ObjectId,
                            LawUnitId = int.Parse(check.Value),
                            DateFrom = DateTime.Now

                        };

                        courtDepartmentLawUnitNews.Add(courtDepartmentLawUnit);
                    }
                }
            }

            courtDepartmentLowUnits.AddRange(courtDepartmentLawUnitNews);
            #endregion

            #region Запис на данните

            return SaveLawUnit(courtDepartmentLowUnits);

            #endregion
        }

        /// <summary>
        /// Запис на CourtDepartmentLawUnit
        /// </summary>
        /// <param name="courtDepartmentLawUnits"></param>
        /// <returns></returns>
        public bool SaveLawUnit(List<CourtDepartmentLawUnit> courtDepartmentLawUnits)
        {
            try
            {
                foreach (var unit in courtDepartmentLawUnits)
                {
                    if (unit.Id > 0)
                        repo.Update(unit);
                    else
                        repo.Add<CourtDepartmentLawUnit>(unit);
                }

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Съдии към Съдебен състав Id={ courtDepartmentLawUnits[0].CourtDepartmentId }");
                return false;
            }
        }

        public IQueryable<CourtDepartmentLawUnitVM> CourtDepartmentLawUnit_Select(int courtDepartmentId)
        {
            return repo.AllReadonly<CourtDepartmentLawUnit>()
                .Include(x => x.LawUnit)
                .Include(x => x.JudgeDepartmentRole)
                .Where(x => x.CourtDepartmentId == courtDepartmentId &&
                            (x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.Now.AddDays(1)) >= DateTime.Now))
                .Select(x => new CourtDepartmentLawUnitVM()
                {
                    Id = x.Id,
                    LawUnitName = (x.LawUnit != null) ? x.LawUnit.FullName : string.Empty,
                    LawUnitId = x.LawUnitId,
                    JudgeDepartmentRoleLabel = (x.JudgeDepartmentRole != null) ? x.JudgeDepartmentRole.Label : string.Empty,
                    JudgeDepartmentRoleId = x.JudgeDepartmentRoleId
                })
                .AsQueryable();
        }

        public bool CourtDepartmentLawUnit_SaveData(CourtDepartmentLawUnit model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CourtDepartmentLawUnit>(model.Id);
                    saved.CourtDepartmentId = model.CourtDepartmentId;
                    saved.LawUnitId = model.LawUnitId;
                    saved.JudgeDepartmentRoleId = model.JudgeDepartmentRoleId;
                    //saved.DateFrom = model.DateFrom;
                    //saved.DateTo = model.DateTo;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    model.DateFrom = DateTime.Now.Date;
                    //Insert
                    repo.Add<CourtDepartmentLawUnit>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Съдебни нива - съдии Id={ model.Id }");
                return false;
            }
        }

        public bool StornoCourtDepartment(int CourtDepartmentId)
        {
            var saved = repo.GetById<CourtDepartmentLawUnit>(CourtDepartmentId);
            try
            {
                saved.DateTo = DateTime.Now;
                repo.Update(saved);
                repo.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при сторниране на Съдебни нива - съдии Id={ saved.Id }");
                return false;
            }
        }

        public List<SelectListItem> Department_SelectDDL(int courtId, int departmentTypeId)
        {
            Expression<Func<CourtDepartment, bool>> departmentTypeWhere = x => true;
            if (departmentTypeId > 0)
                departmentTypeWhere = x => x.DepartmentTypeId == departmentTypeId;

            var result = repo.All<CourtDepartment>()
                .Where(x => x.CourtId == courtId)
                .Where(departmentTypeWhere)
                .Select(x => new SelectListItem()
                {
                    Text = x.Label + " - " + (x.ParentDepartment.Label ?? ""),
                    Value = x.Id.ToString()
                })
                .OrderBy(x => x.Text)
                .ToList() ?? new List<SelectListItem>();

            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }

        public IQueryable<CourtDepartmentVM> CourtDepartmentByLawUnit_Select(int LawUnitId, int CourtId)
        {
            return repo.AllReadonly<CourtDepartmentLawUnit>()
                       .Where(x => x.CourtDepartment.CourtId == CourtId && 
                                   x.LawUnitId == LawUnitId &&
                                   x.CourtDepartment.DepartmentTypeId == NomenclatureConstants.DepartmentType.Systav &&
                                   (x.DateFrom <= DateTime.Now && ((x.DateTo != null) ? x.DateTo >= DateTime.Now : true)))
                       .Select(x => new CourtDepartmentVM()
                       {
                           Id = x.CourtDepartment.Id,
                           Label = x.CourtDepartment.Label,
                           CourtLabel = x.CourtDepartment.Court.Label,
                           DepartmentTypeLabel = (x.CourtDepartment.DepartmentType != null) ? x.CourtDepartment.DepartmentType.Label : string.Empty,
                           MasterId = x.CourtDepartment.MasterId,
                           ParentId = (x.CourtDepartment.ParentId ?? 0)
                       })
                       .AsQueryable();
        }
    }
}
