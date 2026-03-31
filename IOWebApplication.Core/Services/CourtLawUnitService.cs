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
using Nest;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;

namespace IOWebApplication.Core.Services
{
    public class CourtLawUnitService : BaseService, ICourtLawUnitService
    {
        private readonly ICommonService commonService;
        private readonly IRelationManyToManyDateService relationService;
        private readonly ICaseLawUnitService caseLawUnitService;
        private readonly IWorkNotificationService workNotificationService;

        public CourtLawUnitService(ILogger<CourtLawUnitService> _logger,
                                   ICommonService _commonService,
                                   ICaseLawUnitService _caseLawUnitService,
                                   IRepository _repo,
                                   IUserContext _userContext,
                                   IRelationManyToManyDateService _relationService,
                                   IWorkNotificationService _workNotificationService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            commonService = _commonService;
            caseLawUnitService = _caseLawUnitService;
            relationService = _relationService;
            workNotificationService = _workNotificationService;
        }

        public IQueryable<CourtLawUnitVM> CourtLawUnit_Select(int courtId, CourtLawUnitFilter filter)
        {
            Expression<Func<CourtLawUnit, bool>> filterName = x => true;
            if (!string.IsNullOrEmpty(filter.Fullname))
            {
                filterName = x => EF.Functions.ILike(x.LawUnit.FullName, filter.Fullname.ToPaternSearch());
            }
            Expression<Func<CourtLawUnit, bool>> filterDateFrom = x => true;
            if (filter.DateFrom.HasValue)
            {
                filterDateFrom = x => (x.DateTo ?? DateTime.MaxValue) > filter.DateFrom.Value;
            }
            Expression<Func<CourtLawUnit, bool>> filterDateTo = x => true;
            if (filter.DateTo.HasValue)
            {
                filterDateTo = x => x.DateFrom <= filter.DateTo.Value.MakeEndDate();
            }
            return repo.AllReadonly<CourtLawUnit>()
                .Where(x => x.CourtId == courtId &&
                            x.DateExpired == null &&
                            x.PeriodTypeId == filter.PeriodTypeId &&
                            x.LawUnit.LawUnitTypeId == filter.LawUnitTypeId)
                .Where(filterName)
                .Where(filterDateFrom)
                .Where(filterDateTo)
                .OrderBy(x => x.LawUnit.FullName)
                .ThenByDescending(x => x.DateFrom)
                .Select(x => new CourtLawUnitVM()
                {
                    Id = x.Id,
                    LawUnitTypeId = x.LawUnit.LawUnitTypeId,
                    LawUnitName = x.LawUnit.FullName,
                    CourtOrganizationName = x.CourtOrganization.Label,
                    LawUnitPositionName = x.LawUnitPosition.Label,
                    DateFrom = x.DateFrom,
                    DateTo = x.DateTo,
                    MandateDateTo = x.MandateDateTo
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
                                   (x.DateFrom >= DateFrom && x.DateFrom <= DateTo) &&
                                   x.DateExpired == null)
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

        /// <summary>
        /// Метод извличащ данни за асистент/помощник/секретар
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<CourtLawUnitAssistantViewModel> CourtLawUnitAssistant_Select(CourtLawUnitAssistantFilterViewModel filter)
        {
            return repo.AllReadonly<CourtLawUnitAssistant>()
                       .Where(x => x.CourtLawUnitId == filter.CourtLawUnitId)
                       .Where(x => x.DateExpired == null)
                       .Select(x => new CourtLawUnitAssistantViewModel()
                       {
                           Id = x.Id,
                           LawUnitFullName = x.LawUnit.FullName,
                           JudgeRoleLabel = x.JudgeRole.Label
                       });
        }

        public (bool result, string errorMessage) CourtLawUnit_SaveData(CourtLawUnit model)
        {
            try
            {
                //Проверка за припокриване на периоди
                DateTime dateNow = DateTime.Now.Date;
                DateTime dateFuture = DateTime.Now.AddYears(100);
                List<int> periods = new List<int>();
                if (model.PeriodTypeId == NomenclatureConstants.PeriodTypes.Holiday || model.PeriodTypeId == NomenclatureConstants.PeriodTypes.Ill)
                {
                    periods.Add(NomenclatureConstants.PeriodTypes.Holiday);
                    periods.Add(NomenclatureConstants.PeriodTypes.Ill);
                }
                else
                    periods.Add(model.PeriodTypeId);
                //var exists = repo.AllReadonly<CourtLawUnit>()
                //                 .Where(x => x.Id != model.Id && x.CourtId == model.CourtId && periods.Contains(x.PeriodTypeId) &&
                //                             x.LawUnitId == model.LawUnitId && x.DateExpired == null &&
                //                             ((x.DateTo ?? dateNow).Date >= model.DateFrom.Date && (x.DateTo ?? dateNow).Date <= (model.DateTo ?? dateNow).Date ||
                //                             (model.DateTo ?? dateNow).Date >= x.DateFrom.Date && (model.DateTo ?? dateNow).Date <= (x.DateTo ?? dateNow).Date))
                //                 .Any();
                var exists = repo.AllReadonly<CourtLawUnit>()
                 .Where(x => x.Id != model.Id && x.CourtId == model.CourtId && periods.Contains(x.PeriodTypeId) &&
                             x.LawUnitId == model.LawUnitId && x.DateExpired == null &&
                             ((x.DateTo ?? dateFuture).Date >= model.DateFrom.Date && (x.DateTo ?? dateFuture).Date <= (model.DateTo ?? dateFuture).Date ||
                             (model.DateTo ?? dateFuture).Date >= x.DateFrom.Date && (model.DateTo ?? dateFuture).Date <= (x.DateTo ?? dateFuture).Date))
                 .Any();

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

                if (model.MandateDateTo.HasValue)
                {
                    model.MandateDateTo = model.MandateDateTo.ForceEndDate();
                }
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CourtLawUnit>(model.Id);
                    saved.LawUnitId = model.LawUnitId;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo;
                    saved.MandateDateTo = model.MandateDateTo;
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
                logger.LogError(ex, $"Грешка при запис на CourtLawUnit Id={model.Id}");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Извличане на данни за редакция на CourtLawUnitAssistant
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<CourtLawUnitAssistantEditViewModel> GetCourtLawUnitAssistantById(int id)
        {
            return await repo.AllReadonly<CourtLawUnitAssistant>()
                             .Where(x => x.Id == id)
                             .Select(x => new CourtLawUnitAssistantEditViewModel()
                             {
                                 Id = x.Id,
                                 CourtLawUnitId = x.CourtLawUnitId,
                                 CourtId = x.CourtLawUnit.CourtId,
                                 LawUnitId = x.LawUnitId,
                                 JudgeRoleId = x.JudgeRoleId,
                             })
                             .FirstAsync();
        }

        /// <summary>
        /// Запис на CourtLawUnitAssistant
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<bool> CourtLawUnitAssistant_SaveData(CourtLawUnitAssistantEditViewModel model)
        {
            try
            {
                CourtLawUnitAssistant save = model.Id > 0 ? await repo.All<CourtLawUnitAssistant>()
                                                                  .Where(x => x.Id == model.Id)
                                                                  .FirstAsync() : new()
                                                                  {
                                                                      CourtLawUnitId = model.CourtLawUnitId,
                                                                      LawUnitId = model.LawUnitId,
                                                                      JudgeRoleId = model.JudgeRoleId,
                                                                      DateFrom = DateTime.Now,
                                                                  };

                if (model.Id > 0)
                {
                    save.LawUnitId = model.LawUnitId;
                    save.JudgeRoleId = model.JudgeRoleId;
                }
                else
                    await repo.AddAsync(save);

                await repo.SaveChangesAsync();
                model.Id = save.Id;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CourtLawUnitAssistant Id={model.Id}");
                return false;
            }
        }

        /// <summary>
        /// Метод проверяващ за съществуващ запис за този служител
        /// </summary>
        /// <param name="courtLawUnitId">Идентификатор на записа за съдията</param>
        /// <param name="lawUnitId">Идентификатор на служителя</param>
        /// <param name="id">Идентификатор на записа за служителя</param>
        /// <returns></returns>
        public async Task<bool> IsExistsCourtLawUnitAssistant(int courtLawUnitId, int lawUnitId, int id)
        {
            Expression<Func<CourtLawUnitAssistant, bool>> idWhere = x => true;
            if (id > 0)
                idWhere = x => x.Id != id;

            return await repo.AllReadonly<CourtLawUnitAssistant>()
                             .Where(x => x.CourtLawUnitId == courtLawUnitId)
                             .Where(x => x.DateExpired == null)
                             .Where(idWhere)
                             .AnyAsync(x => x.LawUnitId == lawUnitId);
        }

        /// <summary>
        /// Сторниране на секретар към съдия
        /// </summary>
        /// <param name="id">Идентификатор на запис за секретар към съдия</param>
        /// <returns></returns>
        public async Task<bool> CourtLawUnitAssistantExpired(int id)
        {
            try
            {
                CourtLawUnitAssistant expiredAssistant = await repo.All<CourtLawUnitAssistant>()
                                                                   .Where(x => x.Id == id)
                                                                   .FirstAsync();

                expiredAssistant.DateExpired = DateTime.Now;
                await repo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при сторно на CourtLawUnitAssistant Id={id}");
                return false;
            }
        }

        public IQueryable<MultiSelectTransferPercentVM> CourtLawUnitGroup_Select(int courtId, int lawUnitId)
        {
            DateTime dateTomorrow = DateTime.Now.AddDays(1).Date;
            return repo.AllReadonly<CourtLawUnitGroup>()
                       .Include(x => x.CourtGroup)
                       .Where(x => x.CourtId == courtId &&
                                   x.LawUnitId == lawUnitId &&
                                   (x.DateTo ?? dateTomorrow).Date > DateTime.Now)
                       .Where(x => x.CourtGroup.GroupKind == NomenclatureConstants.CourtGroupKinds.JudgeSelection)
                       .Select(x => new MultiSelectTransferPercentVM()
                       {
                           Id = x.CourtGroupId,
                           Order = x.CourtGroup.OrderNumber,
                           Text = x.CourtGroup.Label,
                           Percent = x.LoadIndex
                       }).AsQueryable();
        }

        public async Task<bool> CourtLawUnitGroup_SaveData(int courtId, int lawUnitId, List<MultiSelectTransferPercentVM> codeGroups)
        {
            return await relationService.SaveDataPercent<CourtLawUnitGroup>(lawUnitId, codeGroups,
                x => x.CourtId == courtId && x.CourtGroup.GroupKind == NomenclatureConstants.CourtGroupKinds.JudgeSelection,
                x => x.LawUnitId,
                x => x.LawUnitId == lawUnitId && x.DateTo == null,
                x => x.CourtGroupId,
                x => x.DateFrom,
                x => x.DateTo,
                x => x.LoadIndex,
                (x) =>
                {
                    x.CourtId = courtId;
                    return true;
                }
           , true);
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
                        OrderInt = 0,
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
                                          (c.DateTo ?? enddatenull) >= dateSelect &&
                                          c.DateExpired == null);
            var courtLawUnitIllHoliday = repo.AllReadonly<CourtLawUnit>()
                                   .Where(c => c.CourtId == forCourtId &&
                                          (c.PeriodTypeId == NomenclatureConstants.PeriodTypes.Ill || c.PeriodTypeId == NomenclatureConstants.PeriodTypes.Holiday) &&
                                          c.DateFrom <= dateSelect &&
                                          (c.DateTo ?? enddatenull) >= dateSelect &&
                                          c.DateExpired == null);

            var result = repo.AllReadonly<LawUnit>()
                             .Where(x => (lawUnitType <= 0 || x.LawUnitTypeId == lawUnitType) &&
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
           }).FirstOrDefault();
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
                                .Where(x => x.CourtId == courtId && x.DateExpired == null)
                                .Where(x => x.LawUnitId == lawUnitId && dateNow >= x.DateFrom && dateNow <= (x.DateTo ?? dateEnd))
                                .FirstOrDefault();
        }

        public string GetLawUnitPosition(int courtId, int lawUnitId)
        {
            DateTime dateNow = DateTime.Now.Date;
            DateTime dateEnd = dateNow.AddDays(1);

            return repo.AllReadonly<CourtLawUnit>()
                                .Where(x => x.CourtId == courtId && x.DateExpired == null)
                                .Where(x => x.LawUnitId == lawUnitId && dateNow >= x.DateFrom && dateNow <= (x.DateTo ?? dateEnd))
                                .Select(x => x.LawUnitPosition.Label)
                                .FirstOrValue("");
        }

        public IQueryable<CourtLawUnitVM> CourtLawUnitOrder_Select(int courtId)
        {
            var result = repo.AllReadonly<CourtLawUnitOrder>()
                                .Include(x => x.LawUnit)
                                .Where(x => x.CourtId == courtId)
                                .OrderBy(x => x.OrderNumber)
                                .Select(x => new CourtLawUnitVM
                                {
                                    Id = x.Id,
                                    OrderNumber = x.OrderNumber,
                                    LawUnitId = x.LawUnitId,
                                    LawUnitName = x.LawUnit.FullName
                                }).ToArray();

            for (int i = 1; i <= result.Length; i++)
            {
                result[i - 1].RowNo = i;
            }

            return result.AsQueryable();
        }

        public SaveResultVM CourtLawUnitOrder_ComboSave(CourtLawunitOrderComboVM model)
        {
            var items = CourtLawUnitOrder_Select(userContext.CourtId);
            if (model.NewRowNo <= 0 || model.NewRowNo > items.Count())
            {
                return new SaveResultVM(false, "Невалидна нова позиция на съдия");
            }

            var id = items.Where(x => x.RowNo == model.CurrentRowNo).Select(x => x.Id).FirstOrDefault();
            if (id == 0)
            {
                return new SaveResultVM(false, "Невалиден съдия");
            }
            for (int i = 1; i <= Math.Abs(model.NewRowNo - model.CurrentRowNo); i++)
            {
                Func<CourtLawUnitOrder, int?> orderProp = x => x.OrderNumber;
                Expression<Func<CourtLawUnitOrder, int?>> setterProp = (x) => x.OrderNumber;
                var result = ChangeOrder<CourtLawUnitOrder>(id, model.NewRowNo < model.CurrentRowNo, orderProp, setterProp, x => x.CourtId == userContext.CourtId);
            }
            return new SaveResultVM(true);
        }

        public bool CourtLawUnitOrder_Actualize(int courtId)
        {
            var savedOrders = repo.All<CourtLawUnitOrder>().Where(x => x.CourtId == courtId).ToList();

            DateTime dateNow = DateTime.Now.Date;
            DateTime dateEnd = dateNow.AddDays(1);

            var lawUnitsIncourt = repo.AllReadonly<CourtLawUnit>()
                                        .Include(x => x.LawUnit)
                                        .Where(x => x.CourtId == courtId && x.DateExpired == null)
                                        .Where(x => x.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Judge)
                                        .Where(x => NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(x.PeriodTypeId))
                                        .Where(x => dateNow >= x.DateFrom && dateNow <= (x.DateTo ?? dateEnd))
                                        .Where(x => (x.MandateDateTo ?? dateEnd) >= dateNow)
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

            if (hasChange)
            {
                repo.SaveChanges();
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


        public SaveResultVM CourtDepartmentUnitOrder_ActualizeForCase(int caseId)
        {
            var dtNow = DateTime.Now;


            var caseLawunits = repo.AllReadonly<CaseLawUnit>()
                                  .Where(x => x.CaseId == caseId)
                                  .Where(x => x.CaseSessionId == null)
                                  .Where(x => x.DateFrom <= dtNow && (x.DateTo ?? DateTime.MaxValue) >= dtNow)
                                  .Select(x =>
                                             new
                                             {
                                                 x.Id,
                                                 x.JudgeDepartmentRoleId,
                                                 x.LawUnitId,
                                                 CourtDepartmentId = (x.RealCourtDepartmentId ?? 0)
                                             }).ToList();


            int[] caseJudges = caseLawunits.Select(lu => lu.LawUnitId).ToArray();

            var lawunitOrder = CourtLawUnitOrder_Select(userContext.CourtId)
                                .Where(x => caseJudges.Contains(x.LawUnitId))
                                .Select(x => new
                                {
                                    x.LawUnitId,
                                    x.OrderNumber
                                })
                                .ToList();

            //Ако няма запис за старшинство, но е само 1 съдия - да го добави като отбележи като Председател на състава
            if (caseLawunits.Count == 1 && !lawunitOrder.Any())
            {
                lawunitOrder.Add(new { LawUnitId = caseLawunits.First().LawUnitId, OrderNumber = 1 });
            }

            var caseLawunitDepartment = caseLawunits.Select(x => x.CourtDepartmentId).Distinct();
            //При ново разпределяне, и наличие на състав, председателя по делото се определя от състава, не по старшинство
            if (caseLawunitDepartment.Count() == 1 && caseLawunitDepartment.FirstOrDefault() > 0)
            {
                var JudicalCompositionId = caseLawunitDepartment.FirstOrDefault();
                int DepartmentPredsedatel = repo.AllReadonly<CourtDepartmentLawUnit>()
                             .Where(x => x.CourtDepartmentId == JudicalCompositionId
                             && x.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel)
                             .Where(x => caseJudges.Contains(x.LawUnitId))
                             .Select(x => x.LawUnitId)
                             .FirstOrDefault();

                if (DepartmentPredsedatel > 0)
                {
                    lawunitOrder.Clear();
                    lawunitOrder.Add(new { LawUnitId = DepartmentPredsedatel, OrderNumber = 1 });
                }
            }

            if (!lawunitOrder.Any())
            {
                return new SaveResultVM(false);
            }

            var casePredsedatels = caseLawunits.Where(x => x.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel)
                                                .Select(x => x.LawUnitId).ToList();

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

            if ((newPredsedatel.LawUnitId == 0) || (casePredsedatels.Count == 1 && casePredsedatels.FirstOrDefault() == newPredsedatel.LawUnitId))
            {
                return new SaveResultVM(false, "Няма промяна в председателя на състава.");
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
                            .Select(x => new CourtLawUnitSubstitutionVM
                            {
                                Id = x.Id,
                                DateFrom = x.DateFrom,
                                DateTo = x.DateTo,
                                LawUnitId = x.LawUnitId,
                                LawUnitName = x.LawUnit.FullName,
                                Description = x.Description,
                                SubstituteLawUnitId = x.SubstituteLawUnitId,
                                SubstituteLawUnitName = x.SubstituteLawUnit.FullName
                            })
                            .AsQueryable();
        }

        /// <summary>
        /// Проверка за валиден запис на заместване
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<string> CourtLawUnitSubstitution_Validate(CourtLawUnitSubstitution model)
        {
            if (await repo.AllReadonly<CourtLawUnitSubstitution>()
                          .Where(x => x.CourtId == userContext.CourtId && x.LawUnitId == model.LawUnitId && x.Id != model.Id)
                          .Where(x => x.DateFrom <= model.DateTo && x.DateTo >= model.DateFrom)
                          .Where(x => x.SubstituteLawUnitId == model.SubstituteLawUnitId)
                          .Where(FilterExpireInfo<CourtLawUnitSubstitution>(false))
                          .AnyAsync())
            {
                var lawunitName = (await repo.GetByIdAsync<LawUnit>(model.LawUnitId)).FullName;
                return $"За избрания период вече съществува заместващо лице за {lawunitName}";
            }

            if (model.LawUnitId == model.SubstituteLawUnitId)
            {
                return "Избрали сте едно и също лице";
            }
            return null;
        }

        /// <summary>
        /// Метод за запис на заместване
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<bool> CourtLawUnitSubstitution_SaveData(CourtLawUnitSubstitution model)
        {
            try
            {
                model.DateTo = model.DateTo.ForceEndDate();

                if (model.Id > 0)
                {
                    CourtLawUnitSubstitution saved = await repo.All<CourtLawUnitSubstitution>()
                                                               .Where(x => x.Id == model.Id)
                                                               .FirstAsync();

                    saved.LawUnitId = model.LawUnitId;
                    saved.SubstituteLawUnitId = model.SubstituteLawUnitId;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo;
                    saved.Description = model.Description;
                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;

                    await workNotificationService.TurnOffNotificationForSubstituteJudgeReporter(saved.LawUnitId, saved.SubstituteLawUnitId, saved.DateTo);
                }
                else
                {
                    model.CourtId = userContext.CourtId;
                    model.UserId = userContext.UserId;
                    model.DateWrt = DateTime.Now;
                    await repo.AddAsync(model);
                }

                await repo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CourtLawUnitSubstitution Id={model.Id}");
                return false;
            }
        }

        #region Група Централизирано разпределение ГД

        /// <summary>
        /// Извличане на данни за служители в Група Централизирано разпределение ГД
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IQueryable<CourtLawUnitGroupCCDataVM> GetDataCentralDistributionCC(CourtLawUnitGroupCCFilterVM filter)
        {
            Expression<Func<CourtLawUnitGroup, bool>> courtGroupWhere = x => x.CourtGroup.GroupKind == filter.CourtGroupKind;

            Expression<Func<CourtLawUnitGroup, bool>> lawUnitNameWhere = x => true;
            if (!string.IsNullOrEmpty(filter.LawUnitName))
                lawUnitNameWhere = x => EF.Functions.ILike(x.LawUnit.FullName, filter.LawUnitName.ToPaternSearch());

            Expression<Func<CourtLawUnitGroup, bool>> courtIdWhere = x => true;
            if (filter.CourtId != null && filter.CourtId > 0)
                courtIdWhere = x => x.CourtId == filter.CourtId;

            int userCourtId = userContext.CourtId;

            return repo.AllReadonly<CourtLawUnitGroup>()
                       .Where(courtGroupWhere)
                       .Where(lawUnitNameWhere)
                       .Where(courtIdWhere)
                       .Select(x => new CourtLawUnitGroupCCDataVM()
                       {
                           Id = x.Id,
                           CourtId = x.CourtId,
                           CourtName = x.Court.Label,
                           LawUnitId = x.LawUnitId,
                           CourtDepartmentLabel = x.CourtDepartment.Label,
                           LawUnitName = x.LawUnit.FullName,
                           DateFrom = x.DateFrom,
                           DateTo = x.DateTo,
                           IsEdit = x.CourtId == userCourtId,
                           LoadIndex = x.LoadIndex
                       });
        }

        /// <summary>
        /// Извличане на данни за редакция на служители в Група Централизирано разпределение ГД
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<CourtLawUnitGroupCCEditVM> GetCentralDistributionCCEditById(int id)
        {
            return await repo.AllReadonly<CourtLawUnitGroup>()
                             .Where(x => x.Id == id)
                             .Select(x => new CourtLawUnitGroupCCEditVM()
                             {
                                 Id = x.Id,
                                 CourtId = x.CourtId,
                                 LawUnitId = x.LawUnitId,
                                 DateFrom = x.DateFrom,
                                 DateTo = x.DateTo,
                                 CourtDepartmentId = x.CourtDepartmentId,
                                 DateToDescription = x.DateToDescription,
                                 CourtGroupKind = x.CourtGroup.GroupKind,
                                 LoadIndex = x.LoadIndex,
                                 LoadIndexOld = x.LoadIndex,
                                 LoadIndexDescription = x.LoadIndexDescription,
                                 LoadIndexDescriptionOld = x.LoadIndexDescription
                             })
                             .FirstAsync();
        }

        /// <summary>
        /// Попълване на обект за добавяне за служители в Група Централизирано разпределение ГД
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private async Task<CourtLawUnitGroup> FillCourtLawUnitGroupCentralDistributionCC(CourtLawUnitGroupCCEditVM model)
        {
            int courtGroupId = await repo.AllReadonly<CourtGroup>()
                                          .Where(g => g.GroupKind == model.CourtGroupKind)
                                          .Select(g => g.Id)
                                          .FirstOrDefaultAsync();

            return new()
            {
                CourtId = model.CourtId,
                LawUnitId = model.LawUnitId,
                DateFrom = model.DateFrom.ForceStartDate(),
                DateTo = model.DateTo != null ? model.DateTo.ForceEndDate() : null,
                CourtGroupId = courtGroupId,
                CourtDepartmentId = model.CourtDepartmentId.NumberEmptyToNull(),
                DateToDescription = model.DateToDescription,
                LoadIndex = model.LoadIndex,
                LoadIndexDescription = model.LoadIndexDescription
            };
        }

        /// <summary>
        /// Попълване на данни за редакция за служители в Група Централизирано разпределение ГД
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <param name="modelSave">Модел за редакция</param>
        private static void SetEditFieldsCourtLawUnitGroupCentralDistributionCC(CourtLawUnitGroupCCEditVM model, CourtLawUnitGroup modelSave)
        {
            modelSave.CourtId = model.CourtId;
            modelSave.LawUnitId = model.LawUnitId;
            modelSave.DateFrom = model.DateFrom.ForceStartDate();
            modelSave.DateTo = model.DateTo != null ? model.DateTo.ForceEndDate() : null;
            modelSave.CourtDepartmentId = model.CourtDepartmentId.NumberEmptyToNull();
            modelSave.DateToDescription = model.DateToDescription;
            modelSave.LoadIndex = model.LoadIndex;
            modelSave.LoadIndexDescription = model.LoadIndexDescription;
        }

        /// <summary>
        /// Добавяне/редакция на данни за служители в Група Централизирано разпределение ГД
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<int?> SavelCourtLawUnitGroupCentralDistributionCC(CourtLawUnitGroupCCEditVM model)
        {
            try
            {
                CourtLawUnitGroup modelSave = (model.Id > 0) ? await repo.All<CourtLawUnitGroup>()
                                                                         .Where(x => x.Id == model.Id)
                                                                         .FirstAsync() : await FillCourtLawUnitGroupCentralDistributionCC(model);

                if (model.Id > 0)
                {
                    SetEditFieldsCourtLawUnitGroupCentralDistributionCC(model, modelSave);
                }
                else
                {
                    if (modelSave.CourtGroupId == 0)
                    {
                        return null;
                    }
                    repo.Add(modelSave);
                }
                await repo.SaveChangesAsync();
                return modelSave.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на служител в група Централизирано разпределение ГД с id = {model.Id}; kind={model.CourtGroupKind};courtId:{model.CourtId}");
                return null;
            }
        }

        /// <summary>
        /// Метод връщащ основното кюери за зареждане на служители за Група Централизирано разпределение ГД 
        /// </summary>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <returns></returns>
        private IQueryable<CourtLawUnit> GetCourtLawUnitsQueryable(int courtId)
        {
            Expression<Func<CourtLawUnit, bool>> courtIdWhere = x => true;
            if (courtId > 0)
                courtIdWhere = x => x.CourtId == courtId;

            return repo.AllReadonly<CourtLawUnit>()
                       .Where(courtIdWhere)
                       .Where(x => NomenclatureConstants.PeriodTypes.CurrentlyCourtActions.Contains(x.PeriodTypeId));
        }

        /// <summary>
        /// Добавяне на избраната стойност в падащият списък ако служителя е с конфигурирана дата до
        /// </summary>
        /// <param name="models">Списък със служители</param>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <param name="lawUnitId">Служител за редакция и да се провери дали го има в списъка, ако е с конфигурирана дата до</param>
        /// <returns></returns>
        private async Task AddCourtLawUnitMissing(List<SelectListItem> models, int courtId, int? lawUnitId = null)
        {
            if (lawUnitId != null && lawUnitId > 0)
            {
                if (!models.Any(x => x.Value == lawUnitId.ToString()))
                {
                    DateTime dateNow = DateTime.Now;
                    Expression<Func<CourtLawUnit, bool>> lawUnitIdWhere = x => x.LawUnitId == lawUnitId;

                    SelectListItem courtLawUnitMissing = await GetCourtLawUnitsQueryable(courtId).Where(lawUnitIdWhere)
                                                                                                  .Select(x => new SelectListItem()
                                                                                                  {
                                                                                                      Text = x.LawUnit.FullName + (x.DateTo != null ? " - дата до: " + (x.DateTo ?? dateNow).ToString("dd.MM.yyyy") : string.Empty),
                                                                                                      Value = x.LawUnitId.ToString()
                                                                                                  })
                                                                                                  .FirstOrDefaultAsync();

                    if (courtLawUnitMissing != null)
                        models.Insert(0, courtLawUnitMissing);
                }
            }
        }

        /// <summary>
        /// Метод за зареждане на списък с налични служители в съд
        /// </summary>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <param name="lawUnitId">Служител за редакция и да се провери дали го има в списъка, ако е с конфигурирана дата до</param>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент Избери</param>
        /// <param name="addAllElement">Флаг за добавяне на елемент Всички</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_CommonCourtLawUnitCentralDistributionCC(int courtId, int? lawUnitId = null, bool addDefaultElement = true, bool addAllElement = false)
        {
            DateTime dateNow = DateTime.Now;
            DateTime dateNowAdd10Year = DateTime.Now.AddYears(10);

            List<SelectListItem> results = await GetCourtLawUnitsQueryable(courtId).Where(x => (x.DateTo ?? dateNowAdd10Year) >= dateNow)
                                                                                   .Where(x => x.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Judge)
                                                                                   .Select(x => new SelectListItem()
                                                                                   {
                                                                                       Text = x.LawUnit.FullName,
                                                                                       Value = x.LawUnitId.ToString()
                                                                                   })
                                                                                   .OrderBy(x => x.Text)
                                                                                   .ToListAsync() ?? new List<SelectListItem>();

            await AddCourtLawUnitMissing(results, courtId, lawUnitId);

            if (addDefaultElement)
                results.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });

            if (addAllElement)
                results.Insert(0, new SelectListItem() { Text = "Всички", Value = "-2" });

            return results;
        }

        /// <summary>
        /// Проверка дали служителят е вече добавен в групата, без значение съда
        /// </summary>
        /// <param name="lawUnitId">Идентификатор на служител</param>
        /// <param name="courtGroupKind">Kind на група</param>
        /// <param name="idSave">Идентификатор на запис</param>
        /// <returns></returns>
        public async Task<bool> IsExistLawUnitCentralDistributionCC(int lawUnitId, int courtGroupKind, int? idSave = null)
        {
            Expression<Func<CourtLawUnitGroup, bool>> idSaveWhere = x => true;
            if (idSave != null)
                idSaveWhere = x => x.Id != idSave;

            DateTime dtNow = DateTime.Now;
            DateTime dtTomorow = DateTime.Now.AddDays(1);

            return await repo.AllReadonly<CourtLawUnitGroup>()
                             .Where(idSaveWhere)
                             .AnyAsync(x => x.LawUnitId == lawUnitId &&
                                            x.CourtGroup.GroupKind == courtGroupKind &&
                                            x.DateFrom < dtNow &&
                                            (x.DateTo ?? dtTomorow) > dtNow);
        }

        #endregion
    }
}
