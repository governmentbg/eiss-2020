using AutoMapper.QueryableExtensions;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Extensions;
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
    public class CourtLawUnitService : BaseService, ICourtLawUnitService
    {
        private readonly ICommonService commonService;
        private readonly IRelationManyToManyDateService relationService;
        private readonly ICaseLawUnitService caseLawUnitService;
        public CourtLawUnitService(
            ILogger<CourtLawUnitService> _logger,
            ICommonService _commonService,
            ICaseLawUnitService _caseLawUnitService,
            IRepository _repo,
            IUserContext _userContext,
            IRelationManyToManyDateService _relationService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            commonService = _commonService;
            caseLawUnitService = _caseLawUnitService;
            relationService = _relationService;
        }

        public IQueryable<CourtLawUnitVM> CourtLawUnit_Select(int courtId, int periodType, int lawUnitType)
        {
            return repo.AllReadonly<CourtLawUnit>()
                .Include(x => x.LawUnit)
                .Include(x => x.CourtOrganization)
                .Include(x => x.LawUnitPosition)
                .Where(x => x.CourtId == courtId && x.PeriodTypeId == periodType && x.LawUnit.LawUnitTypeId == lawUnitType)
                .Select(x => new CourtLawUnitVM()
                {
                    Id = x.Id,
                    LawUnitTypeId = x.LawUnit.LawUnitTypeId,
                    LawUnitName = x.LawUnit.FullName,
                    CourtOrganizationName = x.CourtOrganization.Label,
                    LawUnitPositionName = x.LawUnitPosition.Label,
                    DateFrom = x.DateFrom,
                    DateTo = x.DateTo
                }).AsQueryable();
        }

        public IQueryable<CourtLawUnitVM> CourtLawUnitSpr_Select(int LawUnitId, int PeriodTypeId, DateTime? DateFrom, DateTime? DateTo)
        {
            DateTo = DateTo.MakeEndDate();
            return repo.AllReadonly<CourtLawUnit>()
                       .Include(x => x.Court)
                       .Include(x => x.CourtOrganization)
                       .Include(x => x.LawUnitPosition)
                       .Include(x => x.PeriodType)
                       .Include(x => x.LawUnit)
                       .ThenInclude(x => x.LawUnitType)
                       .Where(x => (x.LawUnitId == LawUnitId) &&
                                   (PeriodTypeId > 0 ? x.PeriodTypeId == PeriodTypeId : true) &&
                                   (x.DateFrom >= DateFrom && x.DateFrom <= DateTo))
                       .Select(x => new CourtLawUnitVM()
                       {
                           Id = x.Id,
                           LawUnitTypeId = x.LawUnit.LawUnitTypeId,
                           LawUnitTypeLabel = x.LawUnit.LawUnitType.Label,
                           LawUnitName = x.LawUnit.FullName,
                           CourtLabel = x.Court.Label,
                           CourtOrganizationName = x.CourtOrganization != null ? x.CourtOrganization.Label : string.Empty,
                           LawUnitPositionName = x.LawUnitPosition != null ? x.LawUnitPosition.Label : string.Empty,
                           PeriodTypeLabel = x.PeriodType.Label,
                           PeriodTypeId = x.PeriodTypeId,
                           DateFrom = x.DateFrom,
                           DateTo = x.DateTo
                       }).AsQueryable();
        }

        public (bool result, string errorMessage) CourtLawUnit_SaveData(CourtLawUnit model)
        {
            try
            {
                //Проверка за припокриване на периоди
                DateTime dateNow = DateTime.Now.Date;
                List<int> periods = new List<int>();
                if (model.PeriodTypeId == NomenclatureConstants.PeriodTypes.Holiday || model.PeriodTypeId == NomenclatureConstants.PeriodTypes.Ill)
                {
                    periods.Add(NomenclatureConstants.PeriodTypes.Holiday);
                    periods.Add(NomenclatureConstants.PeriodTypes.Ill);
                }
                else
                    periods.Add(model.PeriodTypeId);
                var exists = repo.AllReadonly<CourtLawUnit>()
                                .Where(x => x.Id != model.Id && x.CourtId == model.CourtId && periods.Contains(x.PeriodTypeId) &&
                                x.LawUnitId == model.LawUnitId &&
                                ((x.DateTo ?? dateNow).Date >= model.DateFrom.Date && (x.DateTo ?? dateNow).Date <= (model.DateTo ?? dateNow).Date ||
                                  (model.DateTo ?? dateNow).Date >= x.DateFrom.Date && (model.DateTo ?? dateNow).Date <= (x.DateTo ?? dateNow).Date)
                                 ).Any();

                if (exists == true)
                {
                    return (result: false, errorMessage: "За избрания служител вече има въведени данни за периода.");
                }


                //Ако е назначаване или преместване тогава да се сетва
                if (!NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(model.PeriodTypeId))
                {
                    model.CourtOrganizationId = null;
                    model.LawUnitPositionId = null;
                }
                model.CourtOrganizationId = (model.CourtOrganizationId ?? 0) <= 0 ? null : model.CourtOrganizationId;
                model.LawUnitPositionId = (model.LawUnitPositionId ?? 0) <= 0 ? null : model.LawUnitPositionId;

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CourtLawUnit>(model.Id);
                    saved.LawUnitId = model.LawUnitId;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo;
                    saved.CourtOrganizationId = model.CourtOrganizationId;
                    saved.LawUnitPositionId = model.LawUnitPositionId;
                    saved.Description = model.Description;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    repo.Add<CourtLawUnit>(model);
                    repo.SaveChanges();
                }
                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CourtLawUnit Id={ model.Id }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        public IQueryable<MultiSelectTransferPercentVM> CourtLawUnitGroup_Select(int courtId, int lawUnitId)
        {
            DateTime dateTomorrow = DateTime.Now.AddDays(1).Date;
            return repo.AllReadonly<CourtLawUnitGroup>()
           .Include(x => x.CourtGroup)
           .Where(x => x.CourtId == courtId && x.LawUnitId == lawUnitId && (x.DateTo ?? dateTomorrow).Date > DateTime.Now)
           .Select(x => new MultiSelectTransferPercentVM()
           {
               Id = x.CourtGroupId,
               Order = x.CourtGroup.OrderNumber,
               Text = x.CourtGroup.Label,
               Percent = x.LoadIndex
           }).AsQueryable();
        }

        public bool CourtLawUnitGroup_SaveData(int courtId, int lawUnitId, List<MultiSelectTransferPercentVM> codeGroups)
        {
            return relationService.SaveDataPercent<CourtLawUnitGroup>(lawUnitId, codeGroups,
                x => x.CourtId == courtId,
                x => x.LawUnitId,
                x => x.CourtGroupId,
                x => x.DateFrom,
                x => x.DateTo,
                x => x.LoadIndex,
                (x) =>
                {
                    x.CourtId = courtId;
                    return true;
                }
            );
        }

        //public IQueryable<CompartmentVM> Compartment_Select(int courtId, int lawUnitId)
        //{
        //    return repo.AllReadonly<Compartment>()
        //        .Include(x => x.LawUnit)
        //        .Where(x => x.CourtId == courtId && x.LawUnitId == lawUnitId)
        //        .Select(x => new CompartmentVM()
        //        {
        //            Id = x.Id,
        //            Label = x.Label,
        //            DateFrom = x.DateFrom,
        //            DateTo = x.DateTo
        //        }).AsQueryable();
        //}

        //public IQueryable<MultiSelectTransferVM> CompartmentLawUnit_Select(int compartmentId)
        //{
        //    return repo.AllReadonly<CompartmentLawUnit>()
        //   .Include(x => x.LawUnit)
        //   .Where(x => x.CompartmentId == compartmentId)
        //   .Select(x => new MultiSelectTransferVM()
        //   {
        //       Id = x.LawUnitId,
        //       Order = 0,
        //       Text = x.LawUnit.FullName
        //   }).AsQueryable();
        //}

        public IQueryable<MultiSelectTransferVM> LawUnitjJudgeForSelect_Select(int courtId, int excludelawUnitId)
        {
            IQueryable<LawUnit> lawUnits = commonService.LawUnit_JudgeByCourtDate(courtId, DateTime.Now);
            return (from item in lawUnits
                    where (item.Id != excludelawUnitId)
                    select new MultiSelectTransferVM()
                    {
                        Id = item.Id,
                        Order = 0,
                        Text = item.FullName
                    }).AsQueryable();
        }
        private IQueryable<LawUnit> LawUnitForCourt_Select(int lawUnitType, int forCourtId, bool noIllHoliday)
        {
            DateTime dateSelect = DateTime.Now;
            DateTime enddatenull = dateSelect.AddDays(1);
            var courtLawUnit = repo.AllReadonly<CourtLawUnit>()
                                   .Where(c => c.CourtId == forCourtId &&
                                          NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(c.PeriodTypeId) &&
                                          c.DateFrom <= dateSelect &&
                                          (c.DateTo ?? enddatenull) >= dateSelect);
            var courtLawUnitIllHoliday = repo.AllReadonly<CourtLawUnit>()
                                   .Where(c => c.CourtId == forCourtId &&
                                          (c.PeriodTypeId == NomenclatureConstants.PeriodTypes.Ill || c.PeriodTypeId == NomenclatureConstants.PeriodTypes.Holiday) &&
                                          c.DateFrom <= dateSelect &&
                                          (c.DateTo ?? enddatenull) >= dateSelect);

            var result = repo.AllReadonly<LawUnit>()
                             .Where(x => x.LawUnitTypeId == lawUnitType &&
                                    ((x.DateTo ?? dateSelect.Date) >= dateSelect.Date) &&
                                    courtLawUnit.Where(c => c.LawUnitId == x.Id).Any() &&
                                    (!noIllHoliday || !courtLawUnitIllHoliday.Where(c => c.LawUnitId == x.Id).Any())
                              )
                       .OrderBy(x => x.FullName)
                       .AsQueryable();
            return result;
        }

        public List<SelectListItem> LawUnitForCourt_SelectDDL(int lawUnitType, int forCourtId, bool noIllHoliday = false)
        {
            var result = LawUnitForCourt_Select(lawUnitType, forCourtId, noIllHoliday)
                       .Select(x => new SelectListItem()
                       {
                           Text = x.FullName,
                           Value = x.Id.ToString()
                       }).ToList() ?? new List<SelectListItem>();
            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }
        public List<Select2ItemVM> LawUnitForCourt_Select2Data(int lawUnitType, int forCourtId, bool noIllHoliday = false)
        {
            var result = LawUnitForCourt_Select(lawUnitType, forCourtId, false)
                       .Select(x => new Select2ItemVM()
                       {
                           Text = x.FullName,
                           Id = x.Id
                       }).ToList() ?? new List<Select2ItemVM>();
            result.Insert(0, new Select2ItemVM() { Text = "Избери", Id = -1 });
            return result;
        }
        //public bool Compartment_SaveData(Compartment model, List<int> codes)
        //{
        //    try
        //    {
        //        if (model.Id > 0)
        //        {
        //            //Update
        //            var saved = repo.GetById<Compartment>(model.Id);
        //            saved.Label = model.Label;
        //            saved.Description = model.Description;
        //            saved.DateFrom = model.DateFrom;
        //            saved.DateTo = model.DateTo;
        //            repo.Update(saved);

        //            //Взима всичко за това ид и го трие
        //            var compartment_lawunit = repo.AllReadonly<CompartmentLawUnit>().Where(a => a.CompartmentId == model.Id).ToList();
        //            foreach (var item in compartment_lawunit)
        //            {
        //                repo.Delete<CompartmentLawUnit>(item);
        //            }
        //        }
        //        else
        //        {
        //            //Insert
        //            repo.Add<Compartment>(model);
        //        }

        //        //записва листа със съдиите за състава
        //        foreach (var code in codes)
        //        {
        //            CompartmentLawUnit newCompartmentLawunit = new CompartmentLawUnit();
        //            newCompartmentLawunit.CompartmentId = model.Id;
        //            newCompartmentLawunit.LawUnitId = code;
        //            repo.Add<CompartmentLawUnit>(newCompartmentLawunit);
        //        }

        //        repo.SaveChanges();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, $"Грешка при запис на Compartment Id={ model.Id }");
        //        return false;
        //    }
        //}

        public CourtLawUnitGroupVM GetCourtLawUnitById(int id)
        {
            return repo.AllReadonly<CourtLawUnit>()
           .Include(x => x.LawUnit)
           .Where(x => x.Id == id)
           .Select(x => new CourtLawUnitGroupVM()
           {
               CourtLawUnitId = x.Id,
               LawUnitId = x.LawUnitId,
               LawUnitName = x.LawUnit.FullName,
               PeriodTypeId = x.PeriodTypeId,
               CaseGroupId = -1,
               LawUnitTypeId = x.LawUnit.LawUnitTypeId
           }).DefaultIfEmpty(null).FirstOrDefault();
        }

        public CourtLawUnit GetCourtLawUnitById_WithLawUnit(int id)
        {
            return repo.AllReadonly<CourtLawUnit>()
           .Include(x => x.LawUnit)
           .Where(x => x.Id == id)
           .FirstOrDefault();
        }

        public CourtLawUnit GetCourtLawUnitAllDatabyLawUnitId(int courtId, int lawUnitId)
        {
            DateTime dateNow = DateTime.Now.Date;
            DateTime dateEnd = dateNow.AddDays(1);

            return repo.AllReadonly<CourtLawUnit>()
                                .Include(x => x.LawUnit)
                                .Include(x => x.LawUnitPosition)
                                .Include(x => x.CourtOrganization)
                                .Where(x => x.CourtId == courtId)
                                .Where(x => x.LawUnitId == lawUnitId && dateNow >= x.DateFrom && dateNow <= (x.DateTo ?? dateEnd))
                                .FirstOrDefault();
        }

        public string GetLawUnitPosition(int courtId, int lawUnitId)
        {
            DateTime dateNow = DateTime.Now.Date;
            DateTime dateEnd = dateNow.AddDays(1);

            return repo.AllReadonly<CourtLawUnit>()
                                .Include(x => x.LawUnitPosition)
                                .Where(x => x.CourtId == courtId)
                                .Where(x => x.LawUnitId == lawUnitId && dateNow >= x.DateFrom && dateNow <= (x.DateTo ?? dateEnd))
                                .Select(x => x.LawUnitPosition.Label)
                                .DefaultIfEmpty("")
                                .FirstOrDefault();
        }

        public IQueryable<CourtLawUnitVM> CourtLawUnitOrder_Select(int courtId)
        {
            return repo.AllReadonly<CourtLawUnitOrder>()
                                .Include(x => x.LawUnit)
                                .Where(x => x.CourtId == courtId)
                                .OrderBy(x => x.OrderNumber)
                                .Select(x => new CourtLawUnitVM
                                {
                                    Id = x.Id,
                                    OrderNumber = x.OrderNumber,
                                    LawUnitId = x.LawUnitId,
                                    LawUnitName = x.LawUnit.FullName
                                })
                                .AsQueryable();
        }

        public bool CourtLawUnitOrder_Actualize(int courtId)
        {
            var savedOrders = repo.All<CourtLawUnitOrder>().Where(x => x.CourtId == courtId).ToList();

            DateTime dateNow = DateTime.Now.Date;
            DateTime dateEnd = dateNow.AddDays(1);

            var lawUnitsIncourt = repo.AllReadonly<CourtLawUnit>()
                                        .Include(x => x.LawUnit)
                                        .Where(x => x.CourtId == courtId)
                                        .Where(x => x.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Judge)
                                        .Where(x => NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(x.PeriodTypeId))
                                        .Where(x => dateNow >= x.DateFrom && dateNow <= (x.DateTo ?? dateEnd))
                                        .Select(x => x.LawUnitId)
                                        .Distinct()
                                        .ToArray();

            bool hasChange = false;

            //Всички, които не са налични в съда се премахват
            foreach (var item in savedOrders.Where(x => !lawUnitsIncourt.Any(a => a == x.LawUnitId)))
            {
                repo.Delete(item);

                hasChange = true;
            }

            //Добавят се всички, които не съществуват в записите с подредбата
            foreach (var item in lawUnitsIncourt.Where(x => !savedOrders.Any(a => a.LawUnitId == x)))
            {
                var newOrder = new CourtLawUnitOrder()
                {
                    CourtId = courtId,
                    LawUnitId = item
                };
                repo.Add(newOrder);
                repo.SaveChanges();
                newOrder.OrderNumber = newOrder.Id;
                repo.SaveChanges();

                hasChange = true;
            }

            return hasChange;
        }

        public SaveResultVM CourtLawUnitOrder_ActualizeForCase(int caseId)
        {
            var lawunitOrder = CourtLawUnitOrder_Select(userContext.CourtId).ToList();

            if (!lawunitOrder.Any())
            {
                return new SaveResultVM(false);
            }

            var dtNow = DateTime.Now;
            var lawunitsSelect = repo.AllReadonly<CaseLawUnit>()
                                  .Where(x => x.DateFrom <= dtNow && (x.DateTo ?? DateTime.MaxValue) >= dtNow)
                                  .Where(x => x.CaseId == caseId);

            var caseLawunits = lawunitsSelect.Where(x => x.CaseSessionId == null).ToList();

            var casePredsedatelId = caseLawunits.Where(x => x.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel)
                                                .Select(x => x.LawUnitId).FirstOrDefault();

            var newPredsedatel = (from o in lawunitOrder
                                  from c in caseLawunits
                                  where o.LawUnitId == c.LawUnitId
                                  orderby o.OrderNumber
                                  select new
                                  {
                                      LawUnitId = o.LawUnitId,
                                      CaseLawUnitId = c.Id
                                  }).FirstOrDefault();

            if (newPredsedatel == null)
            {
                return new SaveResultVM(false);
            }

            if ((newPredsedatel.LawUnitId == 0) || (casePredsedatelId == newPredsedatel.LawUnitId))
            {
                return new SaveResultVM(false);
            }

            if (
            caseLawUnitService.GetCaseLawUnitChangeDepRol_Save(new Infrastructure.Models.ViewModels.Case.CaseLawUnitChangeDepRolVM()
            {
                CaseId = caseId,
                CaseLawUnitId = newPredsedatel.CaseLawUnitId,
                CaseSessionId = null,
                DepartmentId = null
            }))
            {
                return new SaveResultVM(true);
            }
            else
            {
                {
                    return new SaveResultVM(false);
                }
            }
        }



        public IQueryable<CourtLawUnitSubstitutionVM> CourtLawUnitSubstitution_Select(CourtLawUnitSubstitutionFilter filter)
        {
            return repo.AllReadonly<CourtLawUnitSubstitution>()
                            .Include(x => x.LawUnit)
                            .Include(x => x.SubstituteLawUnit)
                            .Where(x => x.CourtId == userContext.CourtId)
                            .Where(x => x.DateFrom <= (filter.DateTo ?? DateTime.MaxValue))
                            .Where(x => x.DateTo >= (filter.DateFrom ?? DateTime.MinValue))
                            .Where(FilterExpireInfo<CourtLawUnitSubstitution>(false))
                            .OrderByDescending(x => x.DateFrom)
                            .ProjectTo<CourtLawUnitSubstitutionVM>(CourtLawUnitSubstitutionVM.GetMapping())
                            .AsQueryable();
        }

        public string CourtLawUnitSubstitution_Validate(CourtLawUnitSubstitution model)
        {
            if (repo.AllReadonly<CourtLawUnitSubstitution>()
                                .Where(x => x.CourtId == userContext.CourtId && x.LawUnitId == model.LawUnitId && x.Id != model.Id)
                                .Where(x => x.DateFrom <= model.DateTo && x.DateTo >= model.DateFrom)
                                .Where(x => x.SubstituteLawUnitId == model.SubstituteLawUnitId)
                                .Where(FilterExpireInfo<CourtLawUnitSubstitution>(false))
                                .Any())
            {
                var lawunitName = repo.GetById<LawUnit>(model.LawUnitId).FullName;
                return $"За избрания период вече съществува заместващо лице за {lawunitName}";
            }

            if (model.LawUnitId == model.SubstituteLawUnitId)
            {
                return "Избрали сте едно и също лице";
            }
            return null;
        }

        public bool CourtLawUnitSubstitution_SaveData(CourtLawUnitSubstitution model)
        {
            try
            {
                model.DateTo = model.DateTo.ForceEndDate();
                if (model.Id > 0)
                {
                    var saved = repo.GetById<CourtLawUnitSubstitution>(model.Id);
                    saved.LawUnitId = model.LawUnitId;
                    saved.SubstituteLawUnitId = model.SubstituteLawUnitId;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo;
                    saved.Description = model.Description;
                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;

                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    model.CourtId = userContext.CourtId;
                    model.UserId = userContext.UserId;
                    model.DateWrt = DateTime.Now;

                    repo.Add(model);
                    repo.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CourtLawUnitSubstitution Id={ model.Id }");
                return false;
            }
        }


    }
}
