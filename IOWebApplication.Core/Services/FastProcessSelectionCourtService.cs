using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.FastProcess;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;

namespace IOWebApplication.Core.Services
{
    public class FastProcessSelectionCourtService : BaseService, IFastProcessSelectionCourtService

    {
        private readonly ICdnService cdnService;

        private readonly IWorkingDaysService workingDays;



        public FastProcessSelectionCourtService(ILogger<FastProcessSelectionCourtService>
            _logger,
            IRepository _repo,
            IUserContext _userContext,
            ICdnService _cdnService,
            IWorkingDaysService _workingDays,
            ICommonService _commonService

        )

        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            cdnService = _cdnService;
            workingDays = _workingDays;


        }
        /// <summary>
        /// Създава ред за конкретен съд в таблица FastProcessSelectionCourt  в която има таргета и оставащите бройки за ибор на бързи разпределения
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="selection_date"></param>
        /// <param name="simulation"></param>
        /// <returns></returns>
        public async Task<bool> CreateSelectionForCourtByDate(int courtId, DateTime selection_date, string simulation = "0")
        {
            bool result = true;
            int year = selection_date.Year;
            int month = selection_date.Month;
            int day = selection_date.Day;
            string zero_simulation = simulation;
            try
            {
                int selectionCount = await repo.AllReadonly<FastProcessSelectionCourt>()
                                                         .Where(x => x.CourtId == courtId)
                                                         .Where(x => x.Simulation == simulation)
                                                         .Where(x => x.YearSel == year)
                                                         .Where(x => x.MonthSel == month)
                                                         .Where(x => x.DaySel == day)
                                                         .CountAsync();
                if (selectionCount == 0)
                {
                    int last_selection_day = -1;
                    var last_selection_day_list = await repo.AllReadonly<FastProcessSelectionCourt>()
                                                                 .Where(x => x.CourtId == courtId)
                                                                 .Where(x => x.YearSel == year)
                                                                 .Where(x => x.MonthSel == month)
                                                                 .Where(x => x.Simulation == simulation)
                                                                 .Select(x => x.DaySel).ToListAsync();
                    if (last_selection_day_list.Count > 0)
                    {
                        last_selection_day = last_selection_day_list.Max();
                    }


                    if (last_selection_day == -1)
                    {
                        last_selection_day = 0;
                        zero_simulation = "0";

                    }

                    FastProcessSelectionCourt selection = await repo.AllReadonly<FastProcessSelectionCourt>()
                                                                .Where(x => x.CourtId == courtId)
                                                                .Where(x => x.YearSel == year)
                                                                .Where(x => x.MonthSel == month)
                                                                .Where(x => x.Simulation == zero_simulation)
                                                                .Where(x => x.DaySel == last_selection_day)
                                                                .Select(x => new FastProcessSelectionCourt
                                                                {
                                                                    CourtId = x.CourtId,
                                                                    YearSel = x.YearSel,
                                                                    MonthSel = x.MonthSel,
                                                                    DaySel = day,
                                                                    SelectionDate = selection_date,
                                                                    StartTarget = x.StartTarget,
                                                                    AddedAfterZero = x.AddedAfterZero,
                                                                    SelectedToNow = x.SelectedToNow,
                                                                    SelectefForDay = 0,
                                                                    LeftForSelection = x.LeftForSelection,
                                                                    Simulation = simulation,
                                                                    UserId = userContext.UserId,
                                                                    DateWrt = DateTime.Now,
                                                                    JudgeCount = x.JudgeCount
                                                                }).FirstOrDefaultAsync();
                    repo.Add<FastProcessSelectionCourt>(selection);
                    await repo.SaveChangesAsync();




                }

            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        /// <summary>
        /// Избира случаен съд на база таргет броя за деня
        /// </summary>
        /// <param name="selection_date"></param>
        /// <param name="simulation"></param>
        /// <returns></returns>
        public async Task<int> RandomizeCourtByDate(DateTime selection_date, int[] AvailableCourts, string simulation = "0")
        {
            int selectedCourt = 0;
            int year = selection_date.Year;
            int month = selection_date.Month;
            int day = selection_date.Day;

            //Въвеждат се максимален брой дела на съдия за ден (средно)
            int max_average_day_fast_cases = 100;
            try
            {
                var param_max_average_day_fast_cases = SystemParam_Select("max_average_day_fast_cases");
                max_average_day_fast_cases = int.Parse(param_max_average_day_fast_cases.ParamValue);
            }
            catch (Exception)
            {


            }


            try
            {
                var selectionList = await repo.AllReadonly<FastProcessSelectionCourt>()
                                                          .Where(x => x.Simulation == simulation)
                                                          //.Where(x => x.SelectionDate == selection_date.Date)
                                                          .Where(x => x.YearSel == year)
                                                          .Where(x => x.MonthSel == month)
                                                          .Where((x) => x.DaySel == day)
                                                          .Where(x => AvailableCourts.Contains(x.CourtId))
                                                           //Да се изключат тези достигнали максималните за деня
                                                           .Where(x =>x.SelectefForDay<((x.JudgeCount??1)* max_average_day_fast_cases))
                                                          //Да се изключат тези достигнали максималните за деня
                                                          .Select(x => new FastProcessSelectionCourtVM
                                                          {
                                                              CourtId = x.CourtId,
                                                              LeftForSelection = x.LeftForSelection
                                                          })
                                                          .ToListAsync();





                List<int> BasketList = new List<int>();




                while (selectionList.Where(x => (x.LeftForSelection > 0)).Count() > 0)

                {
                    foreach (var selection in selectionList)
                    {
                        if (selection.LeftForSelection > 0)
                        {
                            BasketList.Add(selection.CourtId);
                            selection.LeftForSelection = selection.LeftForSelection - 1;
                        }

                    }
                }

                Random random = new Random();
                int randomIndex = random.Next(BasketList.Count);
                selectedCourt = BasketList[randomIndex];







            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RandomizeCourtByDate");
            }

            return selectedCourt;
        }
        /// <summary>
        /// За тестови цели без да търси налични съдии
        /// </summary>
        /// <param name="selection_date"></param>
        /// <param name="simulation"></param>
        /// <returns></returns>
        public async Task<int> RandomizeCourtByDateNoAvailableCourts(DateTime selection_date, string simulation = "0")
        {
            int selectedCourt = 0;
            int year = selection_date.Year;
            int month = selection_date.Month;
            int day = selection_date.Day;

            try
            {
                var selectionList = await repo.AllReadonly<FastProcessSelectionCourt>()
                                                          .Where(x => x.Simulation == simulation)
                                                          //.Where(x => x.SelectionDate == selection_date.Date)
                                                          .Where(x => x.YearSel == year)
                                                          .Where(x => x.MonthSel == month)
                                                          .Where((x) => x.DaySel == day)
                                                          .Select(x => new FastProcessSelectionCourtVM
                                                          {
                                                              CourtId = x.CourtId,
                                                              LeftForSelection = x.LeftForSelection
                                                          })
                                                          .ToListAsync();





                List<int> BasketList = new List<int>();
                //int TotalLeftselection = selectionList.Select(x => x.LeftForSelection).Sum();

                //int medianRestForSelection = TotalLeftselection / selectionList.Where(x => (x.LeftForSelection > 0)).Count();

                //selectionList=selectionList.Where(x=>x.LeftForSelection >= medianRestForSelection).ToList();



                while (selectionList.Where(x => (x.LeftForSelection > 0)).Count() > 0)

                {
                    foreach (var selection in selectionList)
                    {
                        if (selection.LeftForSelection > 0)
                        {
                            BasketList.Add(selection.CourtId);
                            selection.LeftForSelection = selection.LeftForSelection - 1;
                        }

                    }
                }

                Random random = new Random();
                int randomIndex = random.Next(BasketList.Count);
                selectedCourt = BasketList[randomIndex];


                // ДА БЪДЕ ИЗБРАНО 2 пъти
                //List<int> SelectedCourtsList = new List<int>();
                //bool ContiniueSelecting = true;
                //while (ContiniueSelecting)
                //{
                //    Random random = new Random();
                //    int randomIndex = random.Next(BasketList.Count);
                //    SelectedCourtsList.Add(BasketList[randomIndex]);
                //    if (SelectedCourtsList.Where(x => x == BasketList[randomIndex]).Count() > 1)
                //    {
                //        selectedCourt = BasketList[randomIndex];
                //        ContiniueSelecting = false;
                //    }
                //}





            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RandomizeCourtByDateNoAvailableCourts");
            }

            return selectedCourt;
        }


        /// <summary>
        /// Взема ид-та  на районни съдилища
        /// </summary>
        /// <returns></returns>
        public async Task<int[]> GetRegionalCourts()
        {

            return await repo.AllReadonly<Court>().Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
                                                  .OrderBy(x => x.Id)
                                                  .Select(x => x.Id)
                                                  .ToArrayAsync();



        }
        /// <summary>
        /// Проверка за наличност на редове за съответна дата и ако няма създава за всеки съд
        /// </summary>
        /// <param name="selection_date"></param>
        /// <param name="simulation"></param>
        /// <returns></returns>

        public async Task<bool> CheckIsCreatedSelectionDay(DateTime selection_date, string simulation = "0")
        {
            bool result = false;
            try
            {

                result = await repo.AllReadonly<FastProcessSelectionCourt>()
                                                         .Where(x => x.Simulation == simulation)
                                                         .Where(x => x.MonthSel == selection_date.Month)
                                                         .Where(x => x.DaySel == selection_date.Day)
                                                         .Where(x => x.YearSel == selection_date.Year)
                                                         .AnyAsync();
                if (!result)
                {
                    var courts = await GetRegionalCourts();

                    foreach (var court in courts)
                    {
                        var res = await CreateSelectionForCourtByDate(court, selection_date.Date, simulation);
                    }


                }
                result = true;

            }
            catch (Exception ex)
            {

                logger.LogError(ex, "CheckIsCreatedSelectionDay");
            }



            return result;



        }
        /// <summary>
        /// след избор на съд  се променят таргетите и броя избрани дела за съда
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="selection_date"></param>
        /// <param name="simulation"></param>
        /// <returns></returns>
        public async Task<bool> UpdateCourtForSelectionDate(int courtId, DateTime selection_date, string simulation = "0")
        {
            bool result = false;
            try
            {

                FastProcessSelectionCourt fastProcessSelectionCourt = await repo.All<FastProcessSelectionCourt>()
                                                         .Where(x => x.CourtId == courtId)
                                                         .Where(x => x.Simulation == simulation)
                                                         .Where(x => x.DaySel == selection_date.Day)
                                                          .Where(x => x.MonthSel == selection_date.Month)
                                                           .Where(x => x.YearSel == selection_date.Year).FirstOrDefaultAsync();


                fastProcessSelectionCourt.SelectedToNow = fastProcessSelectionCourt.SelectedToNow + 1;
                fastProcessSelectionCourt.SelectefForDay = fastProcessSelectionCourt.SelectefForDay + 1;
                fastProcessSelectionCourt.LeftForSelection = fastProcessSelectionCourt.LeftForSelection - 1;
                fastProcessSelectionCourt.UserId = userContext.UserId;
                fastProcessSelectionCourt.DateWrt = DateTime.Now;

                // repo.Update<FastProcessSelectionCourt>(fastProcessSelectionCourt);
                await repo.SaveChangesAsync();
                result = true;

            }
            catch (Exception)

            {

                throw;
            }



            return result;



        }
        ///// <summary>
        ///// Проверка за налчност на съдия за бързо разпределение в съда
        ///// </summary>
        ///// <param name="courtID"></param>
        ///// <returns></returns>
        //public async Task<List<int>> CheckHasAvailabeJudgesInFastSelectionGroup(int courtID)
        //{
        //    List<int> result=new List<int>();   
        //    try
        //    {
        //        DateTime today= DateTime.Now;

        //       result = await repo.AllReadonly<CourtLawUnitGroup>()
        //                                      .Where(x => x.CourtId == courtID)
        //                                      .Where(x => x.CourtGroup.GroupKind == NomenclatureConstants.CourtGroupKinds.FastProcessCentral)
        //                                      .Select(x=>x.CourtId)
        //                                      .ToListAsync();
        //    }
        //    catch (Exception)

        //    {

        //        throw;
        //    }



        //    return result;
        //}

        public async Task<int> GetSelectedCourtForSelectionDate(DateTime selection_date, int[] available_courts, string simulation = "0")
        {
            int selectedCourtId = 0;
            try
            {
                selectedCourtId = await RandomizeCourtByDate(selection_date, available_courts, simulation);

                if (!await UpdateCourtForSelectionDate(selectedCourtId, selection_date))
                { selectedCourtId = 0; }


            }
            catch (Exception)

            {

                throw;
            }



            return selectedCourtId;



        }

        public async Task<bool> AddAfterZeroCourtForSelectionDat(DateTime selection_date, int[] available_courts, string simulation = "0")
        {
            bool result = false;
            int year = selection_date.Year;
            int month = selection_date.Month;
            int day = selection_date.Day;

            try
            {

                bool hasForSelection = await repo.All<FastProcessSelectionCourt>()
                                                         .Where(x => x.LeftForSelection > 0)
                                                         .Where(x => x.Simulation == simulation)
                                                         //.Where(x => x.SelectionDate == selection_date.Date)
                                                         .Where(x => x.YearSel == year)
                                                         .Where(x => x.MonthSel == month)
                                                         .Where(x => x.DaySel == day)
                                                         .Where(x => available_courts.Contains(x.CourtId)).AnyAsync();
                if (!hasForSelection)
                {

                    List<FastProcessSelectionCourt> fastProcessSelectionCourtList = await repo.All<FastProcessSelectionCourt>()
                                                             .Where(x => x.Simulation == simulation)
                                                             .Where(x => x.YearSel == year)
                                                             .Where(x => x.MonthSel == month)
                                                             .Where(x => x.DaySel == day).ToListAsync();

                    foreach (var fastProcessSelectionCourt in fastProcessSelectionCourtList)
                    {


                        fastProcessSelectionCourt.AddedAfterZero = fastProcessSelectionCourt.AddedAfterZero + (fastProcessSelectionCourt.JudgeCount ?? 0);
                        fastProcessSelectionCourt.LeftForSelection = fastProcessSelectionCourt.LeftForSelection + (fastProcessSelectionCourt.JudgeCount ?? 0);
                        fastProcessSelectionCourt.UserId = userContext.UserId;
                        fastProcessSelectionCourt.DateWrt = DateTime.Now;
                    }

                    await repo.SaveChangesAsync();
                }

                result = true;

            }
            catch (Exception ex)

            {
                logger.LogError(ex, $"AddAfterZeroCourtForSelectionDat {selection_date}");
                return false;
            }



            return result;



        }


        public async Task<FastProcessSelectionProtokolPreviewVM> FastProcessSelectionProtokol_Preview(int id)
        {
            var result = await repo.All<CaseSelectionProtokol>()
                 .Where(x => x.Id == id)
                 .Select(x => new FastProcessSelectionProtokolPreviewVM()
                 {
                     Id = x.Id,
                     CaseId = x.CaseId,
                     CourtId = x.CourtId,
                     CourtName = x.Court.Label,
                     SelectionDate = x.SelectionDate.ToString("dd.MM.yyyy HH:mm"),
                     SelectionDateDateTime = x.SelectionDate,
                     RegNumber = x.Case.RegNumber,
                     JudgeRoleName = x.JudgeRole.Label,
                     JudgeRoleId = x.JudgeRoleId,
                     SelectionModeId = x.SelectionModeId,
                     SelectionModeName = x.SelectionMode.Label,
                     CaseTypeName = x.Case.CaseType.Label,
                     CaseCodeName = x.Case.CaseCode.Code + " - " + x.Case.CaseCode.Label,
                     CaseYear = x.Case.RegDate.Year,
                     Document_Number = x.Case.Document.DocumentNumber + "/" + x.Case.Document.DocumentDate.ToString("dd.MM.yyyy"),
                     Description = x.Description,
                     SelectedLawUnitName = (x.SelectedLawUnit.FullName ?? NomenclatureConstants.SelectionProtocolConstants.NoAvailableJudges),
                     SelectedLawUnitId = x.SelectedLawUnit.Id,
                     SelectedLawUnitTypeName = x.SelectedLawUnit.LawUnitType.Label,
                     CourtGroupName = x.Case.CourtGroup.Label,
                     CourtDutyName = x.CourtDuty.Label,
                     LoadGroupLinkName = x.Case.LoadGroupLink.LoadGroup.Label,
                     UserName = x.User.LawUnit.FullName,
                     UserUIC = x.User.LawUnit.Uic,
                     SelectionProtokolStateId = x.SelectionProtokolStateId,
                     IncludeComparementJudges = x.IncludeCompartmentJudges,
                     ComparementID = x.CompartmentID,
                     ComparentmentName = x.CompartmentName,
                     DismisalReason = x.CaseLawUnitDismisal.Description,
                     DismisalId = x.CaseLawUnitDismisalId,
                     LawUnits = x.LawUnits.Select(p => new FastProcessSelectionProtokolLawUnitPreviewVM
                     {
                         Id = p.LawUnitId,
                         LawUnitFullName = p.LawUnit.FullName,
                         LoadIndex = p.LoadIndex,
                         StateId = p.StateId,
                         CaseCount = p.CaseCount,
                         CaseCourtTotalCount = p.CaseCourtCount,
                         SelectedFromCaseGroup = p.SelectedFromCaseGroup,
                         CaseGroupId = p.CaseGroupId,
                         Description = p.Description,
                         CourtId = p.CourtId,
                         CourtName = p.Court.Label
                     }
                  )
                 }).FirstOrDefaultAsync();


            return result;
        }

        //Вземане  на период за групата 
        public async Task<CourtLoadPeriod> GetLoadPeriod(int courtGroupid)
        {





            CourtLoadPeriod courtLoadPeriod = await repo.AllReadonly<CourtLoadPeriod>()
                                                  .Include(x => x.CourtLoadResetPeriod)
                                                  .Where(x => x.CourtLoadResetPeriod.DateFrom <= DateTime.Now)
                                                  .Where(x => (x.CourtLoadResetPeriod.DateTo ?? DateTime.Now) >= DateTime.Now)
                                                  .Where(x => x.CourtGroupId == courtGroupid).FirstOrDefaultAsync();


            if (courtLoadPeriod == null)
            {

                try
                {
                    var resetPeriodId = await repo.AllReadonly<CourtLoadResetPeriod>()
                                                    .Where(x => x.CourtId == NomenclatureConstants.Courts.RandomAssignment)
                                                    .Where(x => x.DateFrom <= DateTime.Now)
                                                    .Where(x => (x.DateTo ?? DateTime.Now) >= DateTime.Now)
                                                    .Select(x => x.Id)
                                                    .FirstOrDefaultAsync();

                    if (resetPeriodId == 0)
                    {
                        throw new Exception($"Няма зададен период за разпределение за дела Заповедно производство");
                    }

                    courtLoadPeriod = new CourtLoadPeriod();
                    courtLoadPeriod.CourtGroupId = courtGroupid;
                    courtLoadPeriod.CourtLoadResetPeriodId = resetPeriodId;
                    courtLoadPeriod.DateFrom = DateTime.Now;
                    repo.Add<CourtLoadPeriod>(courtLoadPeriod);
                    await repo.SaveChangesAsync();


                }
                catch (Exception ex)
                {

                    logger.LogError(ex, $"Грешка при запис на период за група  courtGroupId ={courtGroupid}");


                }
            }

            return courtLoadPeriod;
        }

        //При Разпределение Създава инициализиращи редове за ДЕНЯ за всички участници в разпределението, за които все още не са създадени
        public async Task<int> MakeDaylyLoadPeriodLawuitRowsByGroup(CaseSelectionProtokol caseSelectionProtocol)
        {
            var courtLoadPeriodId = 0;


            var courtGroupID = (await repo.AllReadonly<Case>().Where(x => x.Id == caseSelectionProtocol.CaseId).Select(x => x.CourtGroupId).FirstOrDefaultAsync() ?? 0);


            var courtLoadPeriod = await GetLoadPeriod(courtGroupID);

            courtLoadPeriodId = courtLoadPeriod.Id;


            if (courtLoadPeriod != null)
            {
                await FSMakeDaylyLoadPeriodLawuitRowsTotal(caseSelectionProtocol, courtLoadPeriod.Id);
                foreach (var lawUnit in caseSelectionProtocol.LawUnits)
                {
                    await FSMakeDaylyLoadPeriodLawuitRowsForLowUnit(caseSelectionProtocol, courtLoadPeriod.Id, lawUnit.LawUnitId);
                }





            }
            ;
            return courtLoadPeriodId;
        }


        //Създава един ред за потребител в дневна таблица сразпределени  дела ако няма такъв.


        public async Task<bool> FSMakeDaylyLoadPeriodLawuitRowsForLowUnit(CaseSelectionProtokol caseSelectionProtocol, int courtLoadPeriodId, int lawUnitId)
        {
            bool res = true;
            try
            {

                var courtLoadPeriodLawUnit = await repo.All<CourtLoadPeriodLawUnit>()
                                                    .Where(x => x.CourtLoadPeriodId == courtLoadPeriodId)
                                                    .Where(x => x.LawUnitId == lawUnitId).ToListAsync();




                if (courtLoadPeriodLawUnit.Where(x => x.SelectionDate.Date == DateTime.Now.Date).ToList().Count == 0)
                {



                    var courtLoadPeriodLawUnitTotal = repo.All<CourtLoadPeriodLawUnit>()
                                                    .Where(x => x.CourtLoadPeriodId == courtLoadPeriodId)
                                                    .Where(x => x.LawUnitId == null);


                    var curentLawUnit = caseSelectionProtocol.LawUnits.Where(x => x.LawUnitId == lawUnitId).FirstOrDefault();

                    CourtLoadPeriodLawUnit currentCourtLoadPeriodLawUnit = new CourtLoadPeriodLawUnit();
                    currentCourtLoadPeriodLawUnit.CourtLoadPeriodId = courtLoadPeriodId;
                    currentCourtLoadPeriodLawUnit.LawUnitId = lawUnitId;
                    currentCourtLoadPeriodLawUnit.SelectionDate = DateTime.Now.Date;
                    if (curentLawUnit.StateId == NomenclatureConstants.SelectionProtokolLawUnitState.Absent || curentLawUnit.StateId == NomenclatureConstants.SelectionProtokolLawUnitState.Exclude)
                    { currentCourtLoadPeriodLawUnit.IsAvailable = false; }
                    else
                    { currentCourtLoadPeriodLawUnit.IsAvailable = true; }
                    currentCourtLoadPeriodLawUnit.DayCases = 0;
                    currentCourtLoadPeriodLawUnit.TotalDayCases = 0;
                    currentCourtLoadPeriodLawUnit.LoadIndex = curentLawUnit.LoadIndex;
                    currentCourtLoadPeriodLawUnit.AverageCases = 0;
                    repo.Add<CourtLoadPeriodLawUnit>(currentCourtLoadPeriodLawUnit);
                    await repo.SaveChangesAsync();

                }



            }

            catch (Exception ex)
            {
                res = false;
                logger.LogError(ex, $"Грешка при запис на информация за разпределение член съдебен състав lawUnitId={lawUnitId}");
            }
            return res;

        }

        //Създава един ред за  за общо разпределените и средно дневните дела в дневна таблица дела ако няма такъв.
        public async Task<bool> FSMakeDaylyLoadPeriodLawuitRowsTotal(CaseSelectionProtokol caseSelectionProtocol, int courtLoadPeriodId)
        {
            bool res = true;
            try
            {
                List<CourtLoadPeriodLawUnit> courtLoadPeriodLawUnitTotal = null;

                courtLoadPeriodLawUnitTotal = await repo.All<CourtLoadPeriodLawUnit>()
                                      .Where(x => x.CourtLoadPeriodId == courtLoadPeriodId)
                                      .Where(x => x.LawUnitId == null)
                                      .Where(x => x.SelectionDate.Date == DateTime.Now.Date)
                                      .ToListAsync();



                if (courtLoadPeriodLawUnitTotal.Count == 0)
                {
                    CourtLoadPeriodLawUnit currentCourtLoadPeriodLawUnit = new CourtLoadPeriodLawUnit();
                    currentCourtLoadPeriodLawUnit.CourtLoadPeriodId = courtLoadPeriodId;
                    currentCourtLoadPeriodLawUnit.LawUnitId = null;
                    currentCourtLoadPeriodLawUnit.SelectionDate = DateTime.Now.Date;
                    currentCourtLoadPeriodLawUnit.IsAvailable = true;
                    currentCourtLoadPeriodLawUnit.DayCases = 0;
                    currentCourtLoadPeriodLawUnit.TotalDayCases = 0;
                    currentCourtLoadPeriodLawUnit.LoadIndex = 0;
                    currentCourtLoadPeriodLawUnit.AverageCases = 0;

                    repo.Add<CourtLoadPeriodLawUnit>(currentCourtLoadPeriodLawUnit);
                    await repo.SaveChangesAsync();

                }

            }
            catch (Exception ex)
            {
                res = false;
                logger.LogError(ex, $"Грешка при запис на обобщена информация за разпределение за courtLoadPeriodId={courtLoadPeriodId}");
            }
            return res;

        }

        /// <summary>
        /// Списък на съдии от съд и група
        /// </summary>
        /// <param name="caseId">ИД на дело </param>
        /// <param name="groupKind">Тип група</param>
        /// <returns></returns>
        public async Task<IEnumerable<FastProcessSelectionProtokolLawUnitVM>> FastProcessLawUnit_LoadJudge(int caseId, int groupKind)
        {
            int courtId = 0;
            if (caseId != 0)
            {

                var _case = await repo.AllReadonly<Case>()
                           .Where(x => x.Id == caseId)
                           .Select(x => new
                           {
                               x.CourtId,
                               x.CourtGroup.GroupKind
                           })
                           .FirstOrDefaultAsync();
                groupKind = _case.GroupKind;
                courtId = _case.CourtId;

            }

            var today = DateTime.Now;
            var endDate = today.AddDays(1);
            var result = await repo.AllReadonly<CourtLawUnitGroup>()
                                    //.Where(x => x.CourtGroup.GroupKind == NomenclatureConstants.CourtGroupKinds.FastProcessCentral)
                                    .Where(x => x.CourtGroup.GroupKind == groupKind)
                                    .Where(x => x.DateFrom <= today && (x.DateTo ?? endDate) >= today)
                                    .Where(x => x.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Judge)
                                     //За да не вхаща тези които не са назначени в момента
                                     .Where(x => x.LawUnit.DateFrom <= today && (x.LawUnit.DateTo ?? endDate) >= today)
                                    //За да не вхаща тези които не са назначени в момента
                                    .Where(x => x.LawUnit.Courts
                                                    .Where(c => NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(c.PeriodTypeId)
                                                    && c.DateFrom <= today && (c.DateTo ?? endDate) >= today
                                                    && (c.MandateDateTo ?? endDate) >= today).Any())

                                    .Select(x => new FastProcessSelectionProtokolLawUnitVM
                                    {
                                        IsLoaded = true,
                                        CourtId = x.CourtId,
                                        LoadIndex = x.LoadIndex,
                                        LawUnitId = x.LawUnitId,
                                        LawUnitFullName = x.LawUnit.FullName,
                                        SelectedFromCaseGroup = true,
                                        StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Include,
                                        CourtDepartmentID = x.CourtDepartmentId
                                    }).ToArrayAsync();

            if (groupKind == NomenclatureConstants.CourtGroupKinds.FastProcessCentralDistributionJurisdiction && courtId > 0)
            {
                result = result.Where(x => x.CourtId == courtId).ToArray();
            }


            await FastProcessSetDataExcludeLawUnit(result, caseId);
            return result;
        }

        //Версия на разпределение когато е необходимо да е наличен само в един от текуш, следващ или по-следващ ден
        //public async Task<IEnumerable<FastProcessSelectionProtokolLawUnitVM>> FastProcessSetDataExcludeLawUnit(FastProcessSelectionProtokolLawUnitVM[] model, int caseId)
        //{
        //    var caseLawUnitDismis = await repo.AllReadonly<CaseLawUnitDismisal>()
        //                                      .Include(x => x.CaseLawUnit)
        //                                      .Where(CaseExtensions.ConfirmedDismissalsOnly())
        //                                      .Where(x => x.CaseLawUnit.CaseId == caseId).ToListAsync();



        //    //var caseLawUnit = await repo.AllReadonly<CaseLawUnit>()
        //    //                             .Where(x => x.CaseId == caseId)
        //    //                             .Where(x => x.CaseSessionId == null).ToListAsync();

        //    var today = DateTime.Now.Date;
        //    var futureDay = today.AddDays(100);
        //    var secondDay = today.AddDays(1);
        //    while (!workingDays.IsWorkingDay(NomenclatureConstants.Courts.RandomAssignment, secondDay))
        //    {
        //        secondDay = secondDay.AddDays(1);

        //    }
        //    var thirdDay = secondDay.AddDays(1);

        //    while (!workingDays.IsWorkingDay(NomenclatureConstants.Courts.RandomAssignment, thirdDay))
        //    {
        //        thirdDay = thirdDay.AddDays(1);

        //    }
        //    var lawunitsIds = model.Select(x => x.LawUnitId).ToArray();
        //    var excludePeriodsForLawunits = await repo.AllReadonly<CourtLawUnit>()
        //                           .Include(x => x.LawUnit)
        //                           .Where(x => lawunitsIds.Contains(x.LawUnitId))
        //                            .Where(x => x.DateExpired == null && x.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Judge &&
        //                            (
        //                                (x.DateFrom.Date <= today && (x.MandateDateTo ?? futureDay) >= today && (x.DateTo ?? futureDay) >= today) ||
        //                                (x.DateFrom.Date <= secondDay && (x.MandateDateTo ?? futureDay) >= secondDay && (x.DateTo ?? futureDay) >= secondDay) ||
        //                                (x.DateFrom.Date <= thirdDay && (x.MandateDateTo ?? futureDay) >= thirdDay && (x.DateTo ?? futureDay) >= thirdDay)
        //                              )
        //                              &&
        //                              NomenclatureConstants.PeriodTypes.ExcludeForSelection.Contains(x.PeriodTypeId))
        //                            .ToListAsync();

        //    var oldDate = DateTime.Now.AddYears(-100);



        //    //Деактивиране на периоди за друг съд, или командироване в същия
        //    foreach (var excludePeriod in excludePeriodsForLawunits)
        //    {
        //        var lawunit = model.Where(m => m.LawUnitId == excludePeriod.LawUnitId).FirstOrDefault();
        //        if ((excludePeriod.PeriodTypeId == NomenclatureConstants.PeriodTypes.Ill || excludePeriod.PeriodTypeId == NomenclatureConstants.PeriodTypes.Holiday) && excludePeriod.CourtId != lawunit.CourtId)
        //        {
        //            excludePeriod.DateTo = oldDate;
        //        }
        //        if ((excludePeriod.PeriodTypeId == NomenclatureConstants.PeriodTypes.Move) && excludePeriod.CourtId == lawunit.CourtId)
        //        {
        //            excludePeriod.DateTo = oldDate;
        //        }
        //    }

        //    excludePeriodsForLawunits = excludePeriodsForLawunits.Where(x => (x.DateTo ?? futureDay) >= x.DateFrom).ToList();

        //    for (int i = 0; i < model.Length; i++)
        //    {

        //        bool IsPeriodExcluded = true;
        //        if (excludePeriodsForLawunits.Where(x => x.LawUnitId == model[i].LawUnitId)
        //                        .Where(x => x.DateFrom.Date <= today && (x.MandateDateTo ?? futureDay) >= today && (x.DateTo ?? futureDay) >= today).Count() == 0)
        //        {
        //            IsPeriodExcluded = false;
        //        }
        //        if (excludePeriodsForLawunits.Where(x => x.LawUnitId == model[i].LawUnitId)
        //                .Where(x => x.DateFrom.Date <= secondDay && (x.MandateDateTo ?? futureDay) >= secondDay && (x.DateTo ?? futureDay) >= secondDay).Count() == 0)
        //        {
        //            IsPeriodExcluded = false;
        //        }
        //        if (excludePeriodsForLawunits.Where(x => x.LawUnitId == model[i].LawUnitId)
        //                                .Where(x => x.DateFrom.Date <= thirdDay && (x.MandateDateTo ?? futureDay) >= today && (x.DateTo ?? futureDay) >= thirdDay).Count() == 0)
        //        {
        //            IsPeriodExcluded = false;
        //        }

        //        if (IsPeriodExcluded)
        //        {
        //            model[i].EnableState = false;
        //            model[i].Description = "";

        //            //Сетват се тези които са болни
        //            var clul = excludePeriodsForLawunits.Where(x => x.LawUnitId == model[i].LawUnitId && x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Ill).ToList();
        //            // var clu = courtlawUnit.Where(x => x.LawUnitId == model[i].LawUnitId && x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Ill).FirstOrDefault();
        //            foreach (var clu in clul)
        //            {
        //                if (clu != null)
        //                {
        //                    string desc = "";
        //                    model[i].StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;


        //                    desc = "Болничен от: " + clu.DateFrom.Date.ToString("dd.MM.yyyy" + " г.");
        //                    if (clu.DateTo != null)
        //                    { desc = desc + " до: " + clu.DateTo.Value.ToString("dd.MM.yyyy" + " г."); }

        //                    if (model[i].Description.Length == 0)
        //                    { model[i].Description = desc; }
        //                    else
        //                    { model[i].Description = model[i].Description + "; " + desc; }

        //                    // continue;
        //                }
        //            }

        //            //Сетват се тези които са отпуска
        //            clul = excludePeriodsForLawunits.Where(x => x.LawUnitId == model[i].LawUnitId && x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Holiday).ToList();
        //            //clu = courtlawUnit.Where(x => x.LawUnitId == model[i].LawUnitId && x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Holiday).FirstOrDefault();
        //            foreach (var clu in clul)
        //            {

        //                if (clu != null)
        //                {
        //                    string desc = "";
        //                    model[i].StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;
        //                    desc = "Отпуск от: " + clu.DateFrom.Date.ToString("dd.MM.yyyy" + " г.");
        //                    if (clu.DateTo != null)
        //                    { desc = desc + " до: " + clu.DateTo.Value.ToString("dd.MM.yyyy" + " г."); }


        //                    if (model[i].Description.Length == 0)
        //                    { model[i].Description = desc; }
        //                    else
        //                    { model[i].Description = model[i].Description + "; " + desc; }
        //                    //continue;
        //                }
        //            }

        //            clul = excludePeriodsForLawunits.Where(x => x.LawUnitId == model[i].LawUnitId && x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Move).ToList();

        //            //clu = courtlawUnit.Where(x => x.LawUnitId == model[i].LawUnitId && x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Move && x.CourtId != model[i].CourtId).FirstOrDefault();
        //            foreach (var clu in clul)
        //            {
        //                if (clu != null)
        //                {
        //                    string desc = "";
        //                    model[i].StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;
        //                    desc = "Командировка от: " + clu.DateFrom.Date.ToString("dd.MM.yyyy" + " г.");
        //                    if (clu.DateTo != null)
        //                    { desc = desc + " до: " + clu.DateTo.Value.ToString("dd.MM.yyyy" + " г."); }

        //                    if (model[i].Description.Length == 0)
        //                    { model[i].Description = desc; }
        //                    else
        //                    { model[i].Description = model[i].Description + "; " + desc; }
        //                    //continue;
        //                }
        //            }
        //            //var allReasons = courtlawUnit.Where(x => x.LawUnitId == model[i].LawUnitId
        //            //&& (
        //            //(x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Move && x.CourtId != model[i].CourtId)
        //            //||
        //            //(x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Holiday)
        //            //||
        //            //(x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Ill)
        //            // )
        //            //).ToList();
        //            //Ако е болничен , командировка или командирован , но не и в един от трите дни
        //            //if ((allReasons.Where(x => x.DateFrom <= today && today <= (x.DateTo ?? today)).Count() == 0) ||
        //            //    (allReasons.Where(x => x.DateFrom <= secondDay && secondDay <= (x.DateTo ?? today)).Count() == 0) ||
        //            //    (allReasons.Where(x => x.DateFrom <= thirdDay && thirdDay <= (x.DateTo ?? today)).Count() == 0)
        //            //    )
        //            //{
        //            //    model[i].StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Include;
        //            //}


        //        }

        //        //Сетват се тези които са с отвод
        //        if (caseLawUnitDismis.Where(x => x.CaseLawUnit.LawUnitId == model[i].LawUnitId)
        //                                      .Where(x => x.DismisalTypeId == NomenclatureConstants.DismisalType.Otvod).Any())
        //        {
        //            model[i].StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;
        //            model[i].Description = "Отвод";
        //            model[i].EnableState = false;
        //            continue;
        //        }
        //        //Сетват се тези които са със отвод
        //        if (caseLawUnitDismis.Where(x => x.CaseLawUnit.LawUnitId == model[i].LawUnitId)
        //                              .Where(x => x.DismisalTypeId == NomenclatureConstants.DismisalType.SamoOtvod).Any())
        //        {
        //            model[i].StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;
        //            model[i].Description = "Самоотвод";
        //            model[i].EnableState = false;
        //            continue;
        //        }

        //    }

        //    return model;
        //}

        // Версия когато се изключва ако има отсъствие в текущия ден и ден преди и ден след ИЛИ   теушщ и двата предходни ИЛИ текуш и двата следващи

        public async Task<IEnumerable<FastProcessSelectionProtokolLawUnitVM>> FastProcessSetDataExcludeLawUnit(FastProcessSelectionProtokolLawUnitVM[] model, int caseId)
        {
            var caseLawUnitDismis = await repo.AllReadonly<CaseLawUnitDismisal>()
                                              .Include(x => x.CaseLawUnit)
                                              .Where(CaseExtensions.ConfirmedDismissalsOnly())
                                              .Where(x => x.CaseLawUnit.CaseId == caseId).ToListAsync();



            //var caseLawUnit = await repo.AllReadonly<CaseLawUnit>()
            //                             .Where(x => x.CaseId == caseId)
            //                             .Where(x => x.CaseSessionId == null).ToListAsync();

            var today = DateTime.Now.Date;
            var futureDay = today.AddDays(100);
            var secondDay = today.AddDays(1);
            while (!workingDays.IsWorkingDay(NomenclatureConstants.Courts.RandomAssignment, secondDay))
            {
                secondDay = secondDay.AddDays(1);

            }
            var secondPreviousDay = today.AddDays(-1);
            while (!workingDays.IsWorkingDay(NomenclatureConstants.Courts.RandomAssignment, secondPreviousDay))
            {
                secondPreviousDay = secondPreviousDay.AddDays(-1);

            }
            var thirdDay = secondDay.AddDays(1);

            while (!workingDays.IsWorkingDay(NomenclatureConstants.Courts.RandomAssignment, thirdDay))
            {
                thirdDay = thirdDay.AddDays(1);

            }
            var thirdPreviousDay = secondPreviousDay.AddDays(-1);

            while (!workingDays.IsWorkingDay(NomenclatureConstants.Courts.RandomAssignment, thirdPreviousDay))
            {
                thirdPreviousDay = thirdPreviousDay.AddDays(-1);

            }
            var lawunitsIds = model.Select(x => x.LawUnitId).ToArray();
            var excludePeriodsForLawunits = await repo.AllReadonly<CourtLawUnit>()
                                   .Include(x => x.LawUnit)
                                   .Where(x => lawunitsIds.Contains(x.LawUnitId))
                                    .Where(x => x.DateExpired == null && x.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Judge &&
                                    (
                                        (x.DateFrom.Date <= today && (x.MandateDateTo ?? futureDay) >= today && (x.DateTo ?? futureDay) >= today) ||
                                        (x.DateFrom.Date <= secondDay && (x.MandateDateTo ?? futureDay) >= secondDay && (x.DateTo ?? futureDay) >= secondDay) ||
                                        (x.DateFrom.Date <= thirdDay && (x.MandateDateTo ?? futureDay) >= thirdDay && (x.DateTo ?? futureDay) >= thirdDay) ||
                                        (x.DateFrom.Date <= secondPreviousDay && (x.MandateDateTo ?? futureDay) >= secondPreviousDay && (x.DateTo ?? futureDay) >= secondPreviousDay) ||
                                        (x.DateFrom.Date <= thirdPreviousDay && (x.MandateDateTo ?? futureDay) >= thirdPreviousDay && (x.DateTo ?? futureDay) >= thirdPreviousDay)
                                      )
                                      &&
                                      NomenclatureConstants.PeriodTypes.ExcludeForSelection.Contains(x.PeriodTypeId))
                                    .ToListAsync();

            var oldDate = DateTime.Now.AddYears(-100);



            //Деактивиране на периоди за друг съд, или командироване в същия
            foreach (var excludePeriod in excludePeriodsForLawunits)
            {
                var lawunit = model.Where(m => m.LawUnitId == excludePeriod.LawUnitId).FirstOrDefault();
                if ((excludePeriod.PeriodTypeId == NomenclatureConstants.PeriodTypes.Ill || excludePeriod.PeriodTypeId == NomenclatureConstants.PeriodTypes.Holiday) && excludePeriod.CourtId != lawunit.CourtId)
                {
                    excludePeriod.DateTo = oldDate;
                }
                if ((excludePeriod.PeriodTypeId == NomenclatureConstants.PeriodTypes.Move) && excludePeriod.CourtId == lawunit.CourtId)
                {
                    excludePeriod.DateTo = oldDate;
                }
            }

            excludePeriodsForLawunits = excludePeriodsForLawunits.Where(x => (x.DateTo ?? futureDay) >= x.DateFrom).ToList();

            for (int i = 0; i < model.Length; i++)
            {
                bool AvailableFirstDay = true;
                bool AvailableSecondDay = true;
                bool AvailableThirdDay = true;
                bool AvailablePreviousSecondDay = true;
                bool AvailablePreviousThirdDay = true;




                bool IsPeriodExcluded = false;
                if (excludePeriodsForLawunits.Where(x => x.LawUnitId == model[i].LawUnitId)
                                .Where(x => x.DateFrom.Date <= today && (x.MandateDateTo ?? futureDay) >= today && (x.DateTo ?? futureDay) >= today).Count() > 0)
                {
                    AvailableFirstDay = false;
                }
                if (excludePeriodsForLawunits.Where(x => x.LawUnitId == model[i].LawUnitId)
                        .Where(x => x.DateFrom.Date <= secondDay && (x.MandateDateTo ?? futureDay) >= secondDay && (x.DateTo ?? futureDay) >= secondDay).Count() > 0)
                {
                    AvailableSecondDay = false;
                }
                if (excludePeriodsForLawunits.Where(x => x.LawUnitId == model[i].LawUnitId)
                                        .Where(x => x.DateFrom.Date <= thirdDay && (x.MandateDateTo ?? futureDay) >= thirdDay && (x.DateTo ?? futureDay) >= thirdDay).Count() > 0)
                {
                    AvailableThirdDay = false;
                }
                if (excludePeriodsForLawunits.Where(x => x.LawUnitId == model[i].LawUnitId)
                    .Where(x => x.DateFrom.Date <= secondPreviousDay && (x.MandateDateTo ?? futureDay) >= secondPreviousDay && (x.DateTo ?? futureDay) >= secondPreviousDay).Count() > 0)
                {
                    AvailablePreviousSecondDay = false;
                }
                if (excludePeriodsForLawunits.Where(x => x.LawUnitId == model[i].LawUnitId)
                        .Where(x => x.DateFrom.Date <= thirdPreviousDay && (x.MandateDateTo ?? futureDay) >= thirdPreviousDay && (x.DateTo ?? futureDay) >= thirdPreviousDay).Count() > 0)
                {
                    AvailablePreviousThirdDay = false;
                }
                if ((AvailableFirstDay == false) && (AvailableSecondDay == false) && (AvailableThirdDay == false))
                {
                    excludePeriodsForLawunits = excludePeriodsForLawunits.Where(x =>
                                        (x.DateFrom.Date <= today && (x.MandateDateTo ?? futureDay) >= today && (x.DateTo ?? futureDay) >= today) ||
                                        (x.DateFrom.Date <= secondDay && (x.MandateDateTo ?? futureDay) >= secondDay && (x.DateTo ?? futureDay) >= secondDay)
                                     || (x.DateFrom.Date <= thirdDay && (x.MandateDateTo ?? futureDay) >= thirdDay && (x.DateTo ?? futureDay) >= thirdDay)
                                      //    || (x.DateFrom.Date <= secondPreviousDay && (x.MandateDateTo ?? futureDay) >= secondDay && (x.DateTo ?? futureDay) >= secondPreviousDay)
                                      //    || (x.DateFrom.Date <= thirdPreviousDay && (x.MandateDateTo ?? futureDay) >= thirdDay && (x.DateTo ?? futureDay) >= thirdPreviousDay)
                                      ).ToList();
                    IsPeriodExcluded = true;
                }

                else
                {
                    if ((AvailableFirstDay == false) && (AvailableSecondDay == false) && (AvailablePreviousSecondDay == false))
                    {
                        excludePeriodsForLawunits = excludePeriodsForLawunits.Where(x =>
                                            (x.DateFrom.Date <= today && (x.MandateDateTo ?? futureDay) >= today && (x.DateTo ?? futureDay) >= today) ||
                                            (x.DateFrom.Date <= secondDay && (x.MandateDateTo ?? futureDay) >= secondDay && (x.DateTo ?? futureDay) >= secondDay)
                                          //    || (x.DateFrom.Date <= thirdDay && (x.MandateDateTo ?? futureDay) >= thirdDay && (x.DateTo ?? futureDay) >= thirdDay)
                                          || (x.DateFrom.Date <= secondPreviousDay && (x.MandateDateTo ?? futureDay) >= secondPreviousDay && (x.DateTo ?? futureDay) >= secondPreviousDay)
                                          //     || (x.DateFrom.Date <= thirdPreviousDay && (x.MandateDateTo ?? futureDay) >= thirdDay && (x.DateTo ?? futureDay) >= thirdPreviousDay)
                                          ).ToList();
                        IsPeriodExcluded = true;
                    }

                    else
                    {
                        if ((AvailableFirstDay == false) && (AvailablePreviousSecondDay == false) && (AvailablePreviousThirdDay == false))
                        {
                            excludePeriodsForLawunits = excludePeriodsForLawunits.Where(x =>
                                                (x.DateFrom.Date <= today && (x.MandateDateTo ?? futureDay) >= today && (x.DateTo ?? futureDay) >= today)
                                             //    (x.DateFrom.Date <= secondDay && (x.MandateDateTo ?? futureDay) >= secondDay && (x.DateTo ?? futureDay) >= secondDay)
                                             //  || (x.DateFrom.Date <= thirdDay && (x.MandateDateTo ?? futureDay) >= thirdDay && (x.DateTo ?? futureDay) >= thirdDay)
                                             || (x.DateFrom.Date <= secondPreviousDay && (x.MandateDateTo ?? futureDay) >= secondPreviousDay && (x.DateTo ?? futureDay) >= secondPreviousDay)
                                             || (x.DateFrom.Date <= thirdPreviousDay && (x.MandateDateTo ?? futureDay) >= thirdPreviousDay && (x.DateTo ?? futureDay) >= thirdPreviousDay)
                                              ).ToList();
                            IsPeriodExcluded = true;
                        }
                    }

                }





                if (IsPeriodExcluded)
                {
                    model[i].EnableState = false;
                    model[i].Description = "";

                    //Сетват се тези които са болни
                    var clul = excludePeriodsForLawunits.Where(x => x.LawUnitId == model[i].LawUnitId && x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Ill).ToList();
                    // var clu = courtlawUnit.Where(x => x.LawUnitId == model[i].LawUnitId && x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Ill).FirstOrDefault();
                    foreach (var clu in clul)
                    {
                        if (clu != null)
                        {
                            string desc = "";
                            model[i].StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;


                            desc = "Болничен от: " + clu.DateFrom.Date.ToString("dd.MM.yyyy" + " г.");
                            if (clu.DateTo != null)
                            { desc = desc + " до: " + clu.DateTo.Value.ToString("dd.MM.yyyy" + " г."); }

                            if (model[i].Description.Length == 0)
                            { model[i].Description = desc; }
                            else
                            { model[i].Description = model[i].Description + "; " + desc; }

                            // continue;
                        }
                    }

                    //Сетват се тези които са отпуска
                    clul = excludePeriodsForLawunits.Where(x => x.LawUnitId == model[i].LawUnitId && x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Holiday).ToList();
                    //clu = courtlawUnit.Where(x => x.LawUnitId == model[i].LawUnitId && x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Holiday).FirstOrDefault();
                    foreach (var clu in clul)
                    {

                        if (clu != null)
                        {
                            string desc = "";
                            model[i].StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;
                            desc = "Отпуск от: " + clu.DateFrom.Date.ToString("dd.MM.yyyy" + " г.");
                            if (clu.DateTo != null)
                            { desc = desc + " до: " + clu.DateTo.Value.ToString("dd.MM.yyyy" + " г."); }


                            if (model[i].Description.Length == 0)
                            { model[i].Description = desc; }
                            else
                            { model[i].Description = model[i].Description + "; " + desc; }
                            //continue;
                        }
                    }

                    clul = excludePeriodsForLawunits.Where(x => x.LawUnitId == model[i].LawUnitId && x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Move).ToList();

                    //clu = courtlawUnit.Where(x => x.LawUnitId == model[i].LawUnitId && x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Move && x.CourtId != model[i].CourtId).FirstOrDefault();
                    foreach (var clu in clul)
                    {
                        if (clu != null)
                        {
                            string desc = "";
                            model[i].StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;
                            desc = "Командировка от: " + clu.DateFrom.Date.ToString("dd.MM.yyyy" + " г.");
                            if (clu.DateTo != null)
                            { desc = desc + " до: " + clu.DateTo.Value.ToString("dd.MM.yyyy" + " г."); }

                            if (model[i].Description.Length == 0)
                            { model[i].Description = desc; }
                            else
                            { model[i].Description = model[i].Description + "; " + desc; }
                            //continue;
                        }
                    }
                    //var allReasons = courtlawUnit.Where(x => x.LawUnitId == model[i].LawUnitId
                    //&& (
                    //(x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Move && x.CourtId != model[i].CourtId)
                    //||
                    //(x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Holiday)
                    //||
                    //(x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Ill)
                    // )
                    //).ToList();
                    //Ако е болничен , командировка или командирован , но не и в един от трите дни
                    //if ((allReasons.Where(x => x.DateFrom <= today && today <= (x.DateTo ?? today)).Count() == 0) ||
                    //    (allReasons.Where(x => x.DateFrom <= secondDay && secondDay <= (x.DateTo ?? today)).Count() == 0) ||
                    //    (allReasons.Where(x => x.DateFrom <= thirdDay && thirdDay <= (x.DateTo ?? today)).Count() == 0)
                    //    )
                    //{
                    //    model[i].StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Include;
                    //}


                }

                //Сетват се тези които са с отвод
                if (caseLawUnitDismis.Where(x => x.CaseLawUnit.LawUnitId == model[i].LawUnitId)
                                              .Where(x => x.DismisalTypeId == NomenclatureConstants.DismisalType.Otvod).Any())
                {
                    model[i].StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;
                    model[i].Description = "Отвод";
                    model[i].EnableState = false;
                    continue;
                }
                //Сетват се тези които са със отвод
                if (caseLawUnitDismis.Where(x => x.CaseLawUnit.LawUnitId == model[i].LawUnitId)
                                      .Where(x => x.DismisalTypeId == NomenclatureConstants.DismisalType.SamoOtvod).Any())
                {
                    model[i].StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;
                    model[i].Description = "Самоотвод";
                    model[i].EnableState = false;
                    continue;
                }

            }

            return model;
        }



        /// <summary>
        /// Създава разпределение по id на дело и връща id на протокол
        /// </summary>
        /// <param name="id"></param> Дело
        /// <returns></returns>
        public async Task<FastProcessProtocolInsertResultVM> FastProcessCreateSelectionProtocol(int id)
        {
            int selectionProtocolId = 0;
            var _case = await repo.AllReadonly<Case>()
                                  .Include(x => x.CourtGroup)
                                  .Where(x => x.Id == id).FirstOrDefaultAsync();



            try
            {



                //CaseSelectionProtokolVM model = new CaseSelectionProtokolVM();


                DateTime dateNow = DateTime.Now;



                CaseSelectionProtokol caseSelectionProtokol = new CaseSelectionProtokol();
                caseSelectionProtokol.CaseId = _case.Id;
                caseSelectionProtokol.CourtId = _case.CourtId;
                caseSelectionProtokol.SelectionDate = dateNow;


                caseSelectionProtokol.JudgeRoleId = NomenclatureConstants.JudgeRole.JudgeReporter;
                caseSelectionProtokol.SelectionModeId = NomenclatureConstants.SelectionMode.SelectByGroups;
                caseSelectionProtokol.CourtDutyId = null;
                caseSelectionProtokol.CourtDepartmentId = null;
                caseSelectionProtokol.SpecialityId = null;
                //  caseSelectionProtokol.Description = model.Description;
                // caseSelectionProtokol.CaseLawUnitDismisalId = model.CaseLawUnitDismisalId;
                caseSelectionProtokol.SelectionProtokolStateId = NomenclatureConstants.SelectionProtokolState.Signed;

                caseSelectionProtokol.LawUnits = new List<CaseSelectionProtokolLawUnit>();
                var lawunitsForSelection = await FastProcessLawUnit_LoadJudge(_case.Id, _case.CourtGroup.GroupKind);


                var courtLoadPeriod = await GetLoadPeriod(_case.CourtGroupId.Value);

                var courtLoadPeriodId = courtLoadPeriod.Id;

                var common_court_load_period_lawunit = await repo.AllReadonly<CourtLoadPeriodLawUnit>()
                                                               .Where(x => x.CourtLoadPeriodId == courtLoadPeriodId).ToListAsync();

                foreach (var lawUnit in lawunitsForSelection)
                {
                    var lawUnitNew = new CaseSelectionProtokolLawUnit();
                    lawUnitNew.CaseId = caseSelectionProtokol.CaseId;
                    lawUnitNew.CourtId = lawUnit.CourtId;
                    lawUnitNew.LawUnitId = lawUnit.LawUnitId;
                    lawUnitNew.LoadIndex = lawUnit.LoadIndex;
                    lawUnitNew.StateId = lawUnit.StateId;
                    lawUnitNew.Description = lawUnit.Description;
                    lawUnitNew.CaseCount = (int)common_court_load_period_lawunit.Where(x => x.LawUnitId == lawUnit.LawUnitId).Sum(x => x.DayCases);
                    //делата за текущия ден на съдията
                    lawUnitNew.CaseCourtCount = (int)common_court_load_period_lawunit.Where(x => x.LawUnitId == lawUnit.LawUnitId)
                                                                                    .Where(x => x.SelectionDate == DateTime.Now.Date)
                                                                                     .Sum(x => x.DayCases);
                    lawUnitNew.SelectedFromCaseGroup = true;
                    lawUnitNew.CaseGroupId = _case.CaseGroupId;
                    lawUnitNew.DateWrt = caseSelectionProtokol.SelectionDate;
                    caseSelectionProtokol.LawUnits.Add(lawUnitNew);
                }

                int periodId = await MakeDaylyLoadPeriodLawuitRowsByGroup(caseSelectionProtokol);

                caseSelectionProtokol.DateWrt = dateNow;



                FastProcessSetSelectedLawUnit_SaveCaseLawUnit(caseSelectionProtokol, periodId, lawunitsForSelection.ToList());



                repo.Add<CaseSelectionProtokol>(caseSelectionProtokol);
                await repo.SaveChangesAsync();
                var lawunitResult = await AddProtocolLawUnitToCase(caseSelectionProtokol);
                if (!lawunitResult.Result)
                {
                    throw new Exception("AddProtocolLawUnitToCase error");
                }
                await Update_CourtLoadPeriodLawunit(periodId, caseSelectionProtokol.SelectedLawUnitId.Value, caseSelectionProtokol.SelectionDate);
                selectionProtocolId = caseSelectionProtokol.Id;

                return new FastProcessProtocolInsertResultVM()
                {
                    Result = true,
                    ProtocolId = caseSelectionProtokol.Id,
                    CaseLawunitId = (int)lawunitResult.ObjectId
                };

                ///////////////////////////////////////////////////////////
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"FastProcessCreateSelectionProtocol; courtId={_case.CourtId}");
                return new FastProcessProtocolInsertResultVM()
                {
                    Result = false
                };
            }
        }


        public void FastProcessSetSelectedLawUnit_SaveCaseLawUnit(CaseSelectionProtokol model, int periodID, List<FastProcessSelectionProtokolLawUnitVM> lawunits_data)
        {

            var JudgeCount = model.LawUnits.Where(x => x.CourtId == model.CourtId).Count();
            var lawunits = model.LawUnits.Where(x => x.CourtId == model.CourtId)
                                         .Where(x => x.StateId == NomenclatureConstants.SelectionProtokolLawUnitState.Include).ToList();

            //var lawunit_periods = await repo.AllReadonly<CourtLoadPeriodLawUnit>()
            //                                     .Where(x => x.CourtLoadPeriodId == periodID)
            //                                     .Where(x => lawunits.Select(c => c.LawUnitId).Contains(x.LawUnitId.Value))
            //                                     .Where(x => x.SelectionDate == model.SelectionDate.Date)
            //                                     .ToListAsync();
            //foreach (var lawunit in lawunits)
            //{
            //    lawunit.CaseCount = (int)lawunit_periods.Where(x => x.LawUnitId == lawunit.LawUnitId).Sum(x => x.DayCases);
            //}            

            List<int> excludedLawunits = new List<int>();

            var tolerance = 1;
            foreach (var base_item in lawunits)
            {
                foreach (var second_item in lawunits)
                {
                    //if (second_item.CaseCount - base_item.CaseCount > tolerance) разлика без процент
                    if (((decimal)second_item.CaseCourtCount.Value / (second_item.LoadIndex / 100M)) - ((decimal)base_item.CaseCourtCount.Value / (base_item.LoadIndex / 100M)) >= tolerance) // разлика за деня със заложено процентно натоварване
                    { excludedLawunits.Add(second_item.LawUnitId); }

                }
            }

            var lawunitsArr = lawunits.Where(x => !excludedLawunits.Contains(x.LawUnitId))
                                      .Where(x => x.CourtId == model.CourtId)
                                      .Select(x => x.LawUnitId).ToArray();
            Random random = new Random();
            int randomNumber = random.Next(0, lawunitsArr.Length - 1);
            model.SelectedLawUnitId = lawunitsArr[randomNumber];
            model.CourtDepartmentId = lawunits_data
                                 .Where(x => x.LawUnitId == model.SelectedLawUnitId)
                                 .Select(x => x.CourtDepartmentID).FirstOrDefault();

        }

        /// <summary>
        /// Попълва реалните дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<CaseSelectionProtokol> Fill_Case_Count(CaseSelectionProtokol model)
        {
            var courtGroupID = await repo.AllReadonly<Case>().Where(x => x.Id == model.CaseId).Select(x => x.CaseGroupId).FirstOrDefaultAsync();
            var courtLoadPeriod = await GetLoadPeriod(courtGroupID);
            var courtLoadPeriodLawUnitList = await repo.AllReadonly<CourtLoadPeriodLawUnit>().Where(x => x.CourtLoadPeriodId == courtLoadPeriod.Id).ToListAsync();
            foreach (var item in model.LawUnits)
            {
                item.CaseCount = (int)Math.Round(courtLoadPeriodLawUnitList.Where(x => x.LawUnitId == item.LawUnitId).Sum(x => x.DayCases));

            }


            return model;

        }

        /// <summary>
        /// Подава се  CourtID=0 когато искаме съд на случаен принцип или ID на съд за да проверим наличност на съдии в него. 
        /// Връща ID  на съд в който има налични съдии или 0 ако е конкретен и в него няма съдии 
        /// 
        /// !!!!! 
        /// </summary>
        /// <param name="courtID"></param>
        /// <returns></returns>
        public async Task<int> GetSelectedAvailableCourtForFastSelection(int courtID, int groupKindId = 0, string string_date = "")
        {

            DateTime date_now = DateTime.Now;

            var lawunits = await FastProcessLawUnit_LoadJudge(0, groupKindId);
            int[] availableCourts = lawunits.Where(x => x.StateId == NomenclatureConstants.SelectionProtokolLawUnitState.Include)
                                              .Select(x => x.CourtId).ToArray();

            // FOR TEST
            if (string_date != "")
            {
                date_now = (Utils.SafeParseDate(string_date) ?? DateTime.Now);
                availableCourts = await GetRegionalCourts();
            }

            //FOR TEST
            await CheckIsCreatedSelectionDay(date_now);
            await AddAfterZeroCourtForSelectionDat(date_now, availableCourts.ToArray());




            if (courtID == 0)
            {
                if (availableCourts.Count() == 0)
                {
                    courtID = 0;
                }
                else

                {

                    courtID = await RandomizeCourtByDate(date_now, availableCourts);


                }



            }
            else
            {


                if (!availableCourts.Contains(courtID))
                {
                    courtID = 0;
                }

            }
            ;

            if (courtID > 0 && groupKindId == NomenclatureConstants.CourtGroupKinds.FastProcessCentral)
            {



                await UpdateCourtForSelectionDate(courtID, date_now);

            }

            return courtID;

        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="lawUnitId"></param>
        ///// <returns></returns>
        //private async Task<string> GetUserIdByLawUnitIdAsync(int lawUnitId)
        //{
        //    return await repo.AllReadonly<ApplicationUser>()
        //                     .Where(x => x.LawUnitId == lawUnitId && x.IsActive)
        //                     .Select(x => x.Id)
        //                     .FirstOrDefaultAsync();
        //}

        public async Task<SaveResultVM> AddProtocolLawUnitToCase(CaseSelectionProtokol protocol)
        {
            try
            {
                var case_obj = await GetByIdAsync<Case>(protocol.CaseId);
                CaseLawUnit judgeCaseLawUnit = new CaseLawUnit();
                judgeCaseLawUnit.CourtId = case_obj.CourtId;
                judgeCaseLawUnit.CaseId = protocol.CaseId;
                judgeCaseLawUnit.LawUnitId = protocol.SelectedLawUnitId.Value;
                judgeCaseLawUnit.LawUnitUserId = await GetUserIdByLawUnitIdAsync(judgeCaseLawUnit.LawUnitId);
                judgeCaseLawUnit.CaseSelectionProtokolId = protocol.Id;
                judgeCaseLawUnit.JudgeDepartmentRoleId = NomenclatureConstants.JudgeDepartmentRole.Predsedatel;
                judgeCaseLawUnit.JudgeRoleId = protocol.JudgeRoleId;
                judgeCaseLawUnit.DateFrom = DateTime.Now;
                judgeCaseLawUnit.Description = "Разпределен автоматично от система";
                judgeCaseLawUnit.CourtDepartmentId = protocol.CourtDepartmentId;

                repo.Add(judgeCaseLawUnit);
                var date = DateTime.Now;

                //Връща Id на CourtLawUnit на назначаването/командироването на съдия в локалния избран съд
                var lastCourtLawUnitId = await repo.AllReadonly<CourtLawUnit>()
                                                   .Where(x => x.CourtId == judgeCaseLawUnit.CourtId)
                                                   .Where(x => x.LawUnitId == judgeCaseLawUnit.LawUnitId)
                                                   .Where(x => x.DateFrom <= date)
                                                   .Where(x => (x.DateTo ?? date) >= date)
                                                   .Where(x => NomenclatureConstants.PeriodTypes.CurrentlyCourtActions.Contains(x.PeriodTypeId))
                                                   .OrderByDescending(x => x.Id)
                                                   .Select(x => x.Id)
                                                   .FirstOrDefaultAsync();

                List<CourtLawUnitAssistant> unitAssistants = await repo.AllReadonly<CourtLawUnitAssistant>()
                                                                       .Where(x => x.CourtLawUnitId == lastCourtLawUnitId)
                                                                       .Where(x => x.DateExpired == null)
                                                                       .ToListAsync();

                List<CaseLawUnit> assistantsSave = [];
                if (unitAssistants != null && unitAssistants.Count() > 0)
                {
                    foreach (CourtLawUnitAssistant assistant in unitAssistants)
                    {
                        string LawUnitUserId = await GetUserIdByLawUnitIdAsync(assistant.LawUnitId);
                        assistantsSave.Add(new()
                        {
                            CourtId = protocol.CourtId,
                            CaseId = protocol.CaseId,
                            LawUnitId = assistant.LawUnitId,
                            LawUnitUserId = LawUnitUserId,
                            //  CaseSelectionProtokolId = protocol.Id,
                            JudgeRoleId = assistant.JudgeRoleId,
                            DateFrom = DateTime.Now,
                            Description = "Разпределен автоматично от система",
                            CourtDepartmentId = protocol.CourtDepartmentId

                        });
                    }
                }

                if (assistantsSave.Count() > 0)
                    repo.AddRange(assistantsSave);

                await repo.SaveChangesAsync();
                return new SaveResultVM()
                {
                    Result = true,
                    ObjectId = judgeCaseLawUnit.Id
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Прехвърляне на състав в дело CaseId={protocol.CaseId}");
                return new SaveResultVM(false);
            }
        }
        public async Task<bool> Update_CourtLoadPeriodLawunit(int periodId, int selectedLawunitId, DateTime data)
        {
            bool result = false;
            try
            {
                var rowsForUpadte = await repo.All<CourtLoadPeriodLawUnit>()
                                             .Where(x => x.CourtLoadPeriodId == periodId)
                                             .Where(x => x.SelectionDate == data.Date)
                                             .Where(x => (x.LawUnitId == selectedLawunitId || x.LawUnitId == null)).ToListAsync();
                foreach (var item in rowsForUpadte)
                {
                    item.DayCases = item.DayCases + 1;
                    item.TotalDayCases = item.TotalDayCases + 1;

                }
                await repo.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                result = false;
                logger.LogError(ex, $"Ъпдейт на брой дела в период selectedLawunitId={selectedLawunitId}");
            }
            return result;
        }


        public async Task<bool> CreateInitializationSelectionForCourtByDate(DateTime selection_date, string simulation = "0")
        {
            bool result = true;
            int year = selection_date.Year;
            int month = selection_date.Month;
            // int day = selection_date.Day;
            string zero_simulation = simulation;
            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            DateTime firstDayOfPreviousMonth = firstDayOfMonth.AddMonths(-1);

            try
            {
                int selectionCount = await repo.AllReadonly<FastProcessSelectionCourt>()
                                                         .Where(x => x.Simulation == simulation)
                                                         .Where(x => x.YearSel == year)
                                                         .Where(x => x.MonthSel == month)
                                                         .Where(x => x.DaySel == 0)
                                                         .CountAsync();
                if (selectionCount == 0)
                {
                    var courts = await repo.AllReadonly<Court>().Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt).ToListAsync();

                    List<FastProcessSelectionCourt> fastProcessSelectionCourtList = new List<FastProcessSelectionCourt>();

                    decimal totalCourtJadgesCount = 0;
                    decimal totalLoadIndex = 0;

                    foreach (var court in courts)
                    {
                        FastProcessSelectionCourt item = new FastProcessSelectionCourt();
                        item.CourtId = court.Id;
                        item.YearSel = year;
                        item.MonthSel = month;
                        item.DaySel = 0;
                        item.SelectionDate = firstDayOfMonth.AddDays(-1);
                        item.StartTarget = 0; //To do
                        item.AddedAfterZero = 0;
                        item.SelectedToNow = 0;
                        item.SelectefForDay = 0;
                        item.LeftForSelection = 0;
                        item.Simulation = simulation;
                        item.JudgeCount = court.JudgeCount;
                        item.BazovKoefCourt = await repo.AllReadonly<Case>()
                                                            .Where(x => x.CourtId == court.Id)
                                                            .Where(x => x.RegDate >= firstDayOfPreviousMonth)
                                                            .Where(x => x.RegDate < firstDayOfMonth)
                                                              //.Where(x => x.CaseStateId != 10)
                                                              //.Where(x => x.CaseCodeId != 170)
                                                              //.Where(x => x.CaseCodeId != 172)
                                                              .Where(x => x.CourtGroup.GroupKind != CourtGroupKinds.FastProcessCentral)
                                                            .SumAsync(x => x.LoadIndex);
                        item.BazovKoefCourtJudge = item.BazovKoefCourt / court.JudgeCount;

                        totalLoadIndex = totalLoadIndex + item.BazovKoefCourt.Value;
                        totalCourtJadgesCount = totalCourtJadgesCount + item.JudgeCount.Value;

                        fastProcessSelectionCourtList.Add(item);
                    }

                    decimal bazovKoefSredenAllCourtJudge = totalLoadIndex / totalCourtJadgesCount;

                    foreach (var item in fastProcessSelectionCourtList)

                    {
                        item.StartTarget = (int)(Math.Round((bazovKoefSredenAllCourtJudge - item.BazovKoefCourtJudge.Value) * 10M * item.JudgeCount.Value));
                        item.LeftForSelection = item.StartTarget;
                        item.BazovKoefSredenALLCourtJudge = bazovKoefSredenAllCourtJudge;
                        repo.Add<FastProcessSelectionCourt>(item);
                    }
                }


                await repo.SaveChangesAsync();

                return true;

            }
            catch (Exception ex)
            {
                result = false;
                logger.LogError(ex, $"Генериране натовареност ЦР за дата {selection_date}");
            }

            return result;
        }

        public List<SelectListItem> SelectionMonths_ForDropDownList(int year)
        {
            List<SelectListItem> result = null;

            var selection_month = repo.AllReadonly<FastProcessSelectionCourt>()
                                             .Where(x => x.DaySel == 0 && x.YearSel == year)
                                             .Select(x => x.MonthSel).Distinct()
                                             .ToList();

            result = (from l in selection_month


                      select new SelectListItem()
                      {
                          Text = l.ToString(),
                          Value = l.ToString()

                      }
                      ).ToList();


            return result;
        }

        public async Task<List<SelectListItem>> SelectionYears_ForDropDownList()
        {
            List<SelectListItem> result = null;

            var selection_year = await repo.AllReadonly<FastProcessSelectionCourt>()
                                             .Where(x => x.DaySel == 0)
                                             .Select(x => x.YearSel).Distinct()
                                             .ToListAsync();


            result = (from l in selection_year


                      select new SelectListItem()
                      {
                          Text = l.ToString(),
                          Value = l.ToString()

                      }
                      ).ToList();


            return result;
        }
        public async Task<FastProcessSelectionMonthsVM> GetFastSelectioForToday()

        {
            FastProcessSelectionMonthsVM result = new FastProcessSelectionMonthsVM();

            result.SelectionDate = DateTime.Now.Date;
            result.SelectionCount = 0;
            result.SelectedCount = await repo.AllReadonly<FastProcessSelectionCourt>()
                                         .Where(x => x.DaySel == result.SelectionDate.Day)
                                         .Where(x => x.MonthSel == result.SelectionDate.Month)
                                         .Where(x => x.YearSel == result.SelectionDate.Year)
                                         .SumAsync(x => x.SelectefForDay);






            return result;


        }

        public async Task<FastProcessSelectionCourtReportVM> FastProcessSelectionCorut_SelectForReport(FastProcessSelectionCourtReportFilterVM model)
        {

            var result = new FastProcessSelectionCourtReportVM();
            var list = await repo.AllReadonly<FastProcessSelectionCourt>()
                                                         .Include(x => x.Court)
                                                         .Where(x => x.MonthSel == model.MonthId)
                                                         .Where(x => x.YearSel == model.YearId).ToListAsync();

            int maxSelectionDate = list.Select(x => x.DaySel).Max();

            decimal m1 = list.Where(y => y.DaySel == 0).Sum(y => y.BazovKoefCourt ?? 0);
            decimal m2 = (decimal)(list.Where(y => y.DaySel > 0).Sum(y => y.SelectefForDay)) * 0.1M;
            decimal m3 = (list.Where(y => y.DaySel == 0).Sum(y => y.JudgeCount ?? 0));


            decimal bazoveKoefVkarqAll = (m1 + m2) / m3;



            result.rows = list
                                  .Where(x => x.DaySel == 0).OrderBy(x => x.CourtId)
                                  .Select(x => new FastProcessSelectionCourtReportRowVM
                                  {
                                      CourtId = x.CourtId,
                                      CourtName = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 0).Select(y => y.Court.Label).FirstOrDefault(),
                                      JudgeCount = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 0).Select(y => y.JudgeCount).FirstOrDefault(),
                                      BazovKoefCourt = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 0).Select(y => y.BazovKoefCourt).FirstOrDefault(),
                                      BazovKoefCourtJudge = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 0).Select(y => y.BazovKoefCourtJudge).FirstOrDefault(),
                                      BazovKoefSredenALLCourtJudge = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 0).Select(y => y.BazovKoefSredenALLCourtJudge).FirstOrDefault(),
                                      YearSel = list.Select(y => y.YearSel).FirstOrDefault(),
                                      MonthSel = list.Select(y => y.MonthSel).FirstOrDefault(),
                                      D01 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 1).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D02 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 2).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D03 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 3).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D04 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 4).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D05 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 5).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D06 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 6).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D07 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 7).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D08 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 8).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D09 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 9).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D10 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 10).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D11 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 11).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D12 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 12).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D13 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 13).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D14 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 14).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D15 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 15).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D16 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 16).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D17 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 17).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D18 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 18).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D19 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 19).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D20 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 20).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D21 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 21).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D22 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 22).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D23 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 23).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D24 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 24).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D25 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 25).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D26 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 26).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D27 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 27).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D28 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 28).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D29 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 29).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D30 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 30).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      D31 = list.Where(y => y.CourtId == x.CourtId && y.DaySel == 31).Select(y => y.SelectefForDay).FirstOrDefault(),
                                      StartTarget = list.Where(y => y.CourtId == x.CourtId && y.DaySel == maxSelectionDate).Select(y => y.StartTarget).FirstOrDefault(),
                                      AddedAfterZero = list.Where(y => y.CourtId == x.CourtId && y.DaySel == maxSelectionDate).Select(y => y.AddedAfterZero).FirstOrDefault(),
                                      OkonchatelenTarget = list.Where(y => y.CourtId == x.CourtId && y.DaySel == maxSelectionDate).Select(y => y.StartTarget).FirstOrDefault() + list.Where(y => y.CourtId == x.CourtId && y.DaySel == maxSelectionDate).Select(y => y.AddedAfterZero).FirstOrDefault(),
                                      PolucheniDela = list.Where(y => y.CourtId == x.CourtId && y.DaySel > 0).Sum(y => y.SelectefForDay),
                                      BazovKoefSredenALLVkraqCourtJudge = bazoveKoefVkarqAll


                                  }).ToList();

            foreach (var item in result.rows)
            {
                item.NatovarbvenePolucheniDela = (decimal)(item.PolucheniDela * 0.1);
                item.NatovarbveneBazovNachalo_PolucheniDela = (decimal)(item.PolucheniDela * 0.1) + item.BazovKoefCourt.Value;
                item.BazovNaSydiaVkraq = ((decimal)(item.PolucheniDela * 0.1) + item.BazovKoefCourt.Value) / (decimal)(item.JudgeCount);

            }
            result.listOfDates = list.Select(x => x.DaySel).Distinct().ToList();

            return result;
        }
        public async Task<bool> GenerateRandomForDateAndCount(string strindDate, int count)
        {
            var result = false;
            try
            {

                DateTime date = DateTime.Parse(strindDate);


                var all_courts = await GetRegionalCourts();


                for (int j = 0; j < count; j++)

                {

                    //bool r = await CheckIsCreatedSelectionDay(date, "0");
                    //bool rf = await AddAfterZeroCourtForSelectionDat(date, all_courts, "0");
                    //var t = await GetSelectedCourtForSelectionDate(date, all_courts, "0");
                    var t = await GetSelectedAvailableCourtForFastSelection(0, 3, strindDate);

                }


                result = true;
            }
            catch
            {

                throw;
            }
            return result;
        }

    }
}

