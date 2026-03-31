using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;

namespace IOWebApplication.Core.Services
{
    public class CourtLoadPeriodService : BaseService, ICourtLoadPeriodService
    {
        public CourtLoadPeriodService(ILogger<CourtLoadPeriodService> _logger,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }

        public CourtLoadPeriod CourtLoadPeriod_GetById(long id, bool readOnly)
        {
            IQueryable<CourtLoadPeriod> courtLoadPeriods = repo.All<CourtLoadPeriod>();
            if (readOnly)
            {
                courtLoadPeriods = repo.AllReadonly<CourtLoadPeriod>();
            }
            var courtLoadPeriod = courtLoadPeriods
                             .Where(x => x.Id == id)

                             .FirstOrDefault();
            return courtLoadPeriod;
        }
        //При Разпределение Създава инициализиращи редове за ДЕНЯ за всички участници в разпределението, за които все още не са създадени
        public bool MakeDaylyLoadPeriodLawuitRowsByGroup(CaseSelectionProtokolVM caseSelectionProtocol)
        {
            bool res = true;
            //Expression<Func<CourtLoadPeriod, bool>> courtGroupDutySearch = x => true;
            //if (caseSelectionProtocol.CourtGroupId != null)
            //{

            //    courtGroupDutySearch = x => x.CourtGroupId == caseSelectionProtocol.CourtGroupId;
            //}
            //if (caseSelectionProtocol.CourtDutyId != null)
            //{

            //    courtGroupDutySearch = x => x.CourtDutyId == caseSelectionProtocol.CourtDutyId;
            //}
            //var courtLoadPeriod = repo.AllReadonly<CourtLoadPeriod>()
            //                                 .Include(x => x.CourtLoadResetPeriod)
            //                                 .Where(x => x.CourtLoadResetPeriod.DateFrom <= DateTime.Now)
            //                                 .Where(x => (x.CourtLoadResetPeriod.DateTo ?? DateTime.Now) >= DateTime.Now)
            //                                 .Where(courtGroupDutySearch).FirstOrDefault();

            if (caseSelectionProtocol.SelectionModeId != NomenclatureConstants.SelectionMode.SelectByDuty)
            { caseSelectionProtocol.CourtDutyId = null; }
            var courtLoadPeriod = GetLoadPeriod(caseSelectionProtocol.CourtGroupId, caseSelectionProtocol.CourtDutyId);

            bool isDuty = (caseSelectionProtocol.CourtDutyId ?? 0) > 0;


            if (courtLoadPeriod != null)
            {
                MakeDaylyLoadPeriodLawuitRowsTotal(caseSelectionProtocol, courtLoadPeriod.Id, isDuty);
                foreach (var lawUnit in caseSelectionProtocol.LawUnits)
                {
                    MakeDaylyLoadPeriodLawuitRowsForLowUnit(caseSelectionProtocol, courtLoadPeriod.Id, lawUnit.LawUnitId, isDuty);
                }





            };
            return res;
        }


        //Създава един ред за потребител в дневна таблица сразпределени  дела ако няма такъв.
        //Ако до сега няма такъв дава за стартова стойност на средно дневни сума на всички среднодневни до днес

        public bool MakeDaylyLoadPeriodLawuitRowsForLowUnit(CaseSelectionProtokolVM caseSelectionProtocol, int courtLoadPeriodId, int lawUnitId, bool IsDuty)
        {
            bool res = true;
            try
            {

                var courtLoadPeriodLawUnit = repo.All<CourtLoadPeriodLawUnit>()
                                                    .Where(x => x.CourtLoadPeriodId == courtLoadPeriodId)
                                                    .Where(x => x.LawUnitId == lawUnitId);
                //.Where(x => x.SelectionDate.Date == DateTime.Now.Date)
                if (IsDuty)

                //По дежурство
                {
                    if (courtLoadPeriodLawUnit.ToList().Count == 0)
                    {



                        var courtLoadPeriodLawUnitTotal = repo.All<CourtLoadPeriodLawUnit>()
                                                        .Where(x => x.CourtLoadPeriodId == courtLoadPeriodId)
                                                        .Where(x => x.LawUnitId == null);


                        var curentLawUnit = caseSelectionProtocol.LawUnits.Where(x => x.LawUnitId == lawUnitId).FirstOrDefault();

                        CourtLoadPeriodLawUnit currentCourtLoadPeriodLawUnit = new CourtLoadPeriodLawUnit();
                        currentCourtLoadPeriodLawUnit.CourtLoadPeriodId = courtLoadPeriodId;
                        currentCourtLoadPeriodLawUnit.LawUnitId = lawUnitId;
                        currentCourtLoadPeriodLawUnit.SelectionDate = DateTime.Now.Date;

                        currentCourtLoadPeriodLawUnit.IsAvailable = true;
                        currentCourtLoadPeriodLawUnit.DayCases = 0;
                        currentCourtLoadPeriodLawUnit.TotalDayCases = 0;
                        currentCourtLoadPeriodLawUnit.LoadIndex = curentLawUnit.LoadIndex;
                        currentCourtLoadPeriodLawUnit.AverageCases = 0;

                        repo.Add<CourtLoadPeriodLawUnit>(currentCourtLoadPeriodLawUnit);
                        repo.SaveChanges();

                    }
                }
                else
                //В Група
                {
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
                        //Проверка съдията в групата ли е или е от друга група
                        bool b_user_in_courtGroup = true;
                        try
                        {
                            DateTime d_naw = DateTime.Now;
                            var period = repo.GetById<CourtLoadPeriod>(courtLoadPeriodId);
                            var user_in_group = repo.AllReadonly<CourtLawUnitGroup>()
                                                  .Where(x => x.CourtGroupId == period.CourtGroupId)
                                                  .Where(x => x.LawUnitId == lawUnitId)
                                                  .Where(x => x.DateFrom < d_naw)
                                                   .Where(x => (x.DateTo ?? d_naw) >= d_naw).ToList().Count();

                            if (!(user_in_group > 0))
                            {
                                b_user_in_courtGroup = false;
                            }
                        }
                        catch (Exception)
                        {

                        }





                        //Проверка съдията в групата ли е или е от друга група
                        //Когато този потребител се включва на по късен етап и получава  приравнителни средно дневни
                        //if (courtLoadPeriodLawUnit.ToList().Count == 0 && courtLoadPeriodLawUnitTotal.Where(x => x.SelectionDate < DateTime.Now.Date).ToList().Count > 0)
                        if (courtLoadPeriodLawUnit.ToList().Count == 0 && b_user_in_courtGroup && courtLoadPeriodLawUnitTotal.Where(x => x.SelectionDate < DateTime.Now.Date).ToList().Count > 0)
                        {     //Празен ред за деня
                            repo.Add<CourtLoadPeriodLawUnit>(currentCourtLoadPeriodLawUnit);
                            repo.SaveChanges();
                            //Изравнителен ред с по-ранна дата за да не се заличава при отсъствие
                            CourtLoadPeriodLawUnit averageCourtLoadPeriodLawUnit = new CourtLoadPeriodLawUnit();
                            averageCourtLoadPeriodLawUnit.CourtLoadPeriodId = courtLoadPeriodId;
                            averageCourtLoadPeriodLawUnit.LawUnitId = lawUnitId;
                            averageCourtLoadPeriodLawUnit.SelectionDate = repo.All<CourtLoadPeriodLawUnit>()
                                                          .Where(x => x.CourtLoadPeriodId == courtLoadPeriodId)
                                                          .Where(x => x.LawUnitId == lawUnitId).Select(x => x.SelectionDate).Min().AddDays(-1);
                            //averageCourtLoadPeriodLawUnit.IsAvailable = false;
                            averageCourtLoadPeriodLawUnit.IsAvailable = true;
                            averageCourtLoadPeriodLawUnit.DayCases = 0;
                            averageCourtLoadPeriodLawUnit.TotalDayCases = 0;
                            averageCourtLoadPeriodLawUnit.LoadIndex = curentLawUnit.LoadIndex;
                            averageCourtLoadPeriodLawUnit.AverageCases = 0;

                            //foreach (var row in courtLoadPeriodLawUnitTotal.Where(x => x.SelectionDate != DateTime.Now.Date).ToList())
                            //{
                            //  averageCourtLoadPeriodLawUnit.AverageCases = averageCourtLoadPeriodLawUnit.AverageCases + row.AverageCases;
                            //  averageCourtLoadPeriodLawUnit.TotalDayCases = averageCourtLoadPeriodLawUnit.TotalDayCases + row.AverageCases;
                            //}
                            //repo.Add<CourtLoadPeriodLawUnit>(averageCourtLoadPeriodLawUnit);
                            //repo.SaveChanges();

                            foreach (var row in courtLoadPeriodLawUnitTotal.Where(x => x.SelectionDate != DateTime.Now.Date).ToList())
                            {
                                averageCourtLoadPeriodLawUnit.AverageCases = averageCourtLoadPeriodLawUnit.AverageCases + row.AverageCases;
                            }
                            //За да са спрямо процента на натоварване, с който се добавя
                            averageCourtLoadPeriodLawUnit.AverageCases = averageCourtLoadPeriodLawUnit.AverageCases * averageCourtLoadPeriodLawUnit.LoadIndex / 100;
                            //Намаляване на среднодневните с процента отклонение заложен в конфигурацията 

                            decimal deviation_percent = 10;
                            try
                            {
                                var param_deviation_percent = SystemParam_Select("case_deviation_percent");
                                deviation_percent = decimal.Parse(param_deviation_percent.ParamValue);
                            }
                            catch (Exception)
                            {

                            }
                            averageCourtLoadPeriodLawUnit.AverageCases = averageCourtLoadPeriodLawUnit.AverageCases * (100 / (100 + deviation_percent));

                            //Намаляване на среднодневните с процента отклонение заложен в конфигурацията 

                            averageCourtLoadPeriodLawUnit.TotalDayCases = averageCourtLoadPeriodLawUnit.AverageCases;
                            repo.Add<CourtLoadPeriodLawUnit>(averageCourtLoadPeriodLawUnit);
                            repo.SaveChanges();
                        }
                        else
                        //Когато този потребител има разпределения до момента в групата -Нормално разпределение за следващ  ден

                        {  //Ако все още няма разпределение за текущия ден
                            if (courtLoadPeriodLawUnit.Where(x => x.SelectionDate == DateTime.Now.Date).ToList().Count == 0)
                            {
                                repo.Add<CourtLoadPeriodLawUnit>(currentCourtLoadPeriodLawUnit);
                                repo.SaveChanges();
                            }
                        }
                    }
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
        public bool MakeDaylyLoadPeriodLawuitRowsTotal(CaseSelectionProtokolVM caseSelectionProtocol, int courtLoadPeriodId, bool IsDuty)
        {
            bool res = true;
            try
            {
                List<CourtLoadPeriodLawUnit> courtLoadPeriodLawUnitTotal = null;
                if (IsDuty)
                {
                    courtLoadPeriodLawUnitTotal = repo.All<CourtLoadPeriodLawUnit>()
                                                .Where(x => x.CourtLoadPeriodId == courtLoadPeriodId)
                                                .Where(x => x.LawUnitId == null)
                                                .ToList();
                }
                else
                {
                    courtLoadPeriodLawUnitTotal = repo.All<CourtLoadPeriodLawUnit>()
                                          .Where(x => x.CourtLoadPeriodId == courtLoadPeriodId)
                                          .Where(x => x.LawUnitId == null)
                                          .Where(x => x.SelectionDate.Date == DateTime.Now.Date)
                                          .ToList();
                }


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
                    repo.SaveChanges();

                }

            }
            catch (Exception ex)
            {
                res = false;
                logger.LogError(ex, $"Грешка при запис на обобщена информация за разпределение за courtLoadPeriodId={courtLoadPeriodId}");
            }
            return res;

        }

        ////Създаване на период за група или дежурство
        //public bool MakeLoadPeriod(int? courtGroupid, int? courtDutyId)
        //{
        //    bool res = true;

        //    Expression<Func<CourtLoadPeriod, bool>> courtGroupDutySearch = x => true;
        //    if (courtGroupid != null)
        //    {

        //        courtGroupDutySearch = x => x.CourtGroupId == courtGroupid;
        //    }
        //    if (courtDutyId != null)
        //    {

        //        courtGroupDutySearch = x => x.CourtDutyId == courtDutyId;
        //    }



        //    CourtLoadPeriod courtLoadPeriod = repo.AllReadonly<CourtLoadPeriod>()
        //                                          .Where(courtGroupDutySearch)
        //                                          .FirstOrDefault();
        //    if (courtLoadPeriod == null)
        //    {

        //        try
        //        {
        //            courtLoadPeriod = new CourtLoadPeriod();
        //            if (courtGroupid != null)
        //            { courtLoadPeriod.CourtGroupId = courtGroupid; }
        //            if (courtDutyId != null)
        //            { courtLoadPeriod.CourtDutyId = courtDutyId; }
        //            courtLoadPeriod.DateFrom = DateTime.Now;
        //            repo.Add<CourtLoadPeriod>(courtLoadPeriod);
        //            repo.SaveChanges();


        //        }
        //        catch (Exception ex)
        //        {
        //            res = false;
        //            if (courtGroupid != null)
        //            {
        //                logger.LogError(ex, $"Грешка при запис на период за група  courtGroupId ={ courtGroupid}");
        //            }
        //            else
        //            { logger.LogError(ex, $"Грешка при запис на период за група  courtDutyId ={ courtDutyId}"); }
        //        }
        //    }

        //    return res;
        //}


        //Вземане  на период за група или дежурство
        public CourtLoadPeriod GetLoadPeriod(int? courtGroupid, int? courtDutyId)
        {


            Expression<Func<CourtLoadPeriod, bool>> courtGroupDutySearch = x => true;
            if (courtGroupid != null)
            {

                courtGroupDutySearch = x => x.CourtGroupId == courtGroupid && ((x.CourtDutyId ?? 0) < 1);
            }
            if (courtDutyId != null)
            {

                courtGroupDutySearch = x => x.CourtDutyId == courtDutyId;
            }



            CourtLoadPeriod courtLoadPeriod = repo.AllReadonly<CourtLoadPeriod>()
                                                  .Include(x => x.CourtLoadResetPeriod)
                                                  .Where(x => x.CourtLoadResetPeriod.DateFrom <= DateTime.Now)
                                                  .Where(x => (x.CourtLoadResetPeriod.DateTo ?? dtTomorrow) >= DateTime.Now)
                                                  .Where(courtGroupDutySearch).FirstOrDefault();


            if (courtLoadPeriod == null)
            {

                try
                {
                    var resetPeriodId = repo.AllReadonly<CourtLoadResetPeriod>()
                                                    .Where(x => x.CourtId == userContext.CourtId)
                                                    .Where(x => x.DateFrom <= DateTime.Now)
                                                    .Where(x => (x.DateTo ?? dtTomorrow) >= DateTime.Now)
                                                    .Select(x => x.Id)
                                                    .FirstOrDefault();

                    if (resetPeriodId == 0)
                    {
                        throw new Exception($"Няма зададен период за разпределение за съд {userContext.CourtName}");
                    }

                    courtLoadPeriod = new CourtLoadPeriod();
                    if (courtGroupid != null)
                    {
                        courtLoadPeriod.CourtGroupId = courtGroupid;
                    }
                    if (courtDutyId != null)
                    {
                        courtLoadPeriod.CourtDutyId = courtDutyId;
                    }
                    courtLoadPeriod.CourtLoadResetPeriodId = resetPeriodId;
                    courtLoadPeriod.DateFrom = DateTime.Now;
                    repo.Add<CourtLoadPeriod>(courtLoadPeriod);
                    repo.SaveChanges();


                }
                catch (Exception ex)
                {
                    if (courtGroupid != null)
                    {
                        logger.LogError(ex, $"Грешка при запис на период за група  courtGroupId ={courtGroupid}");
                    }
                    else
                    {
                        logger.LogError(ex, $"Грешка при запис на период за група  courtDutyId ={courtDutyId}");
                    }
                }
            }

            return courtLoadPeriod;
        }


        //Създава периоди за всички несъздали до момента групи или дежурства
        public bool MakeLoadPeriodForAll()
        {
            bool res = true;
            try
            {
                var courtGroups = repo.AllReadonly<CourtGroup>().ToList();
                foreach (var courtGroup in courtGroups)
                {
                    GetLoadPeriod(courtGroup.Id, null);
                }

                var courtDuties = repo.AllReadonly<CourtDuty>().ToList();
                foreach (var courtDuty in courtDuties)
                {
                    GetLoadPeriod(null, courtDuty.Id);
                }
            }
            catch (Exception)
            {
                res = false;

            }

            return res;
        }
        //Изчисляване на коефициента за разпределение според коефициента 
        public CaseSelectionProtokolVM CalculateAllKoef(CaseSelectionProtokolVM caseSelectionProtocol)

        {
            if (caseSelectionProtocol.SelectionModeId != NomenclatureConstants.SelectionMode.SelectByDuty)
            { caseSelectionProtocol.CourtDutyId = null; }

            CourtLoadPeriod courtLoadPeriod = GetLoadPeriod(caseSelectionProtocol.CourtGroupId, caseSelectionProtocol.CourtDutyId);

            //Optimisacia2020.01.09 s
            //int totalPeriodDays = repo.AllReadonly<CourtLoadPeriodLawUnit>()
            //                        .Where(x => x.CourtLoadPeriodId == courtLoadPeriod.Id)
            //                        .Where(x => (x.LawUnitId ?? 0) == 0)
            //                        .Count();
            //Optimisacia2020.01.09 e


            decimal totalKoef = 0;
            //2020.01.08 Optimisation
            List<int> LawUnitsArray = caseSelectionProtocol.LawUnits.Select(x => x.LawUnitId).ToList();
            LawUnitsArray.Add(0);

            // foreach (var lawUnit in caseSelectionProtocol.LawUnits.Where(x => NomenclatureConstants.SelectionProtokolLawUnitState.ActiveState.Contains(x.StateId)))
            var courtLoadPeriodLawunitsList = repo.AllReadonly<CourtLoadPeriodLawUnit>().Where(x => LawUnitsArray.Contains(x.LawUnitId ?? 0))
                                                                                 .Where(x => x.CourtLoadPeriodId == courtLoadPeriod.Id).ToList();
            //Optimisacia2020.01.09 s
            int totalPeriodDays = courtLoadPeriodLawunitsList
                                    .Where(x => (x.LawUnitId ?? 0) == 0)
                                    .Count();
            //Optimisacia2020.01.09 e

            foreach (var lawUnit in caseSelectionProtocol.LawUnits)
            {
                // CalculateLawUnitDataInGroup(lawUnit, courtLoadPeriod.Id);
                CalculateLawUnitDataInGroup(lawUnit, courtLoadPeriod.Id, courtLoadPeriodLawunitsList);
                //2020.01.08 Optimisation
                totalKoef = totalKoef + lawUnit.Koef;

            }
            // foreach (var lawUnit in caseSelectionProtocol.LawUnits.Where(x => NomenclatureConstants.SelectionProtokolLawUnitState.ActiveState.Contains(x.StateId)))
            foreach (var lawUnit in caseSelectionProtocol.LawUnits)
            {
                lawUnit.KoefNormalized = lawUnit.Koef / totalKoef * 100M;

                ////Когато нормализираният коефициент е прекалено малък го приравняваме на 1 за да има поне 1 участие  като вероятност
                //if ((lawUnit.KoefNormalized > 0) && (lawUnit.KoefNormalized < 1))
                //{ lawUnit.KoefNormalized = 1; }
                //2020.01.08 Optimisation 1
                //int lawUnitPeriodDays = repo.AllReadonly<CourtLoadPeriodLawUnit>()
                //                      .Where(x => x.CourtLoadPeriodId == courtLoadPeriod.Id)
                //                      .Where(x => x.LawUnitId == lawUnit.LawUnitId)
                //                      .Where(x => x.AverageCases == 0)
                //                      .Count();
                int lawUnitPeriodDays = courtLoadPeriodLawunitsList
                                     .Where(x => x.LawUnitId == lawUnit.LawUnitId)
                                     .Where(x => x.AverageCases == 0)
                                     .Count();
                //2020.01.08 Optimisation 1
                if (lawUnitPeriodDays == 0)
                { lawUnitPeriodDays = 1; }
                if (lawUnit.LoadIndex == 0)
                {
                    lawUnit.LoadIndex = 100;
                }
                //lawUnit.CasesCountIfWorkAllPeriodInGroup = (decimal)totalPeriodDays / (decimal)lawUnitPeriodDays * 100M / (decimal)lawUnit.LoadIndex * lawUnit.CaseCount;
                lawUnit.CasesCountIfWorkAllPeriodInGroup = 100M / (decimal)lawUnit.LoadIndex * lawUnit.TotalCaseCount;
                lawUnit.ExcludeByBigDeviation = false;
            }

            return caseSelectionProtocol;
        }


        //Изчислява коефициентите за всеки един съдия в рамките на група или дежурство
        public void CalculateLawUnitDataInGroup(CaseSelectionProtokolLawUnitVM caseSelectionProtokolLawUnit, int courtLoadPeriodId, List<CourtLoadPeriodLawUnit> courtLoadPeriodLawunitsList)

        {
            //var courtLoadPeriodLawunits = repo.AllReadonly<CourtLoadPeriodLawUnit>().Where(x => x.LawUnitId == caseSelectionProtokolLawUnit.LawUnitId)
            //                                                                     .Where(x => x.CourtLoadPeriodId == courtLoadPeriodId).ToList();

            var courtLoadPeriodLawunits = courtLoadPeriodLawunitsList.Where(x => x.LawUnitId == caseSelectionProtokolLawUnit.LawUnitId);
            //Optimisacia2020.01.09 s
            //foreach (var courtLoadPeriodLawunit in courtLoadPeriodLawunits)
            //{
            //  caseSelectionProtokolLawUnit.TotalCaseCount = caseSelectionProtokolLawUnit.TotalCaseCount + courtLoadPeriodLawunit.TotalDayCases;
            //  caseSelectionProtokolLawUnit.CaseCount = caseSelectionProtokolLawUnit.CaseCount + (int)courtLoadPeriodLawunit.DayCases;
            //}
            caseSelectionProtokolLawUnit.TotalCaseCount = courtLoadPeriodLawunits.Sum(x => x.TotalDayCases);
            caseSelectionProtokolLawUnit.CaseCount = courtLoadPeriodLawunits.Sum(x => (int)x.DayCases);
            //Optimisacia2020.01.09 e
            if (caseSelectionProtokolLawUnit.TotalCaseCount > 0)
            {//2022.05.04 Да се взема процента на натоварване  т.к е изравнен при смяна на процента със тотал делата
             // caseSelectionProtokolLawUnit.Koef = caseSelectionProtokolLawUnit.LoadIndex/ caseSelectionProtokolLawUnit.TotalCaseCount;

                caseSelectionProtokolLawUnit.Koef = (caseSelectionProtokolLawUnit.LoadIndex * caseSelectionProtokolLawUnit.LoadIndex) / (caseSelectionProtokolLawUnit.TotalCaseCount * 100M);
            }
            else
            //Когато разпределените дела са 0 на някой  се т.к деление на 0 е невъзможно даваме вместо 0 малка стойноат 0.01 
            { caseSelectionProtokolLawUnit.Koef = caseSelectionProtokolLawUnit.LoadIndex * 100; }






        }
        //Преизчислява редовете с дневни брой дела за група /дежурство по избран съдия
        public void UpdateDailyLoadPeriod(int? CourtGroupId, int? CourtDutyId, int selectedLawUnit, int selectionMode)

        {
            CourtLoadPeriod courtLoadPeriod = GetLoadPeriod(CourtGroupId, CourtDutyId);

            Expression<Func<CourtLoadPeriodLawUnit, bool>> courtLoadPeriodRowsLawunitsSelect = x => true;
            if (CourtDutyId == null)
            {
                //Ако е група
                courtLoadPeriodRowsLawunitsSelect = x => (x.CourtLoadPeriodId == courtLoadPeriod.Id) && (x.SelectionDate.Date == DateTime.Now.Date);
            }
            else
            {
                //Ако е по дежурство
                courtLoadPeriodRowsLawunitsSelect = x => (x.CourtLoadPeriodId == courtLoadPeriod.Id);
            }


            var courtLoadPeriodLawUnits = repo.All<CourtLoadPeriodLawUnit>().Where(courtLoadPeriodRowsLawunitsSelect).ToList();

            decimal totalAverage = 0;

            //Изчислява сумарния дневен ред
            foreach (var courtLoadPeriodLawUnit in courtLoadPeriodLawUnits)
            {
                if (courtLoadPeriodLawUnit.LawUnitId == null)
                {
                    courtLoadPeriodLawUnit.TotalDayCases = courtLoadPeriodLawUnit.TotalDayCases + 1;
                    courtLoadPeriodLawUnit.DayCases = courtLoadPeriodLawUnit.DayCases + 1;
                    //Да изчислява средно дневни на база процента на натовареност  
                    var availableJudges = courtLoadPeriodLawUnits.Where(x => (x.IsAvailable || x.DayCases > 0) && x.LawUnitId != null).ToList();
                    decimal availableCount = 0;
                    foreach (var item in availableJudges)
                    {
                        availableCount = availableCount + item.LoadIndex / 100M;
                    }
                    // courtLoadPeriodLawUnit.AverageCases = courtLoadPeriodLawUnit.TotalDayCases / (courtLoadPeriodLawUnits.Where(x => (x.IsAvailable || x.DayCases > 0) && x.LawUnitId != null).Count());
                    // Когато са само ръчни до момента за деня да се сметнат правилно среднодневните 2022.09.02
                    var today = DateTime.Now;
                    var endDate = today.AddDays(1);
                    var autamatic_selection_in_group = repo.AllReadonly<CaseSelectionProtokol>()
                                        .Include(x => x.Case)
                                        .Where(x => x.Case.CourtGroupId == CourtGroupId)
                                          .Where(x => x.CourtDutyId == null)
                                        .Where(x => x.SelectionModeId == NomenclatureConstants.SelectionMode.SelectByGroups)

                                        .Where(x => x.SelectionDate.Date == today.Date).Count();
                    if (selectionMode == NomenclatureConstants.SelectionMode.ManualSelect)
                    {
                        //if (courtLoadPeriodLawUnit.TotalDayCases == 1)
                        if (autamatic_selection_in_group == 0)
                        {

                            var courtID = repo.GetById<CourtGroup>(CourtGroupId).CourtId;
                            availableCount = repo.AllReadonly<CourtLawUnitGroup>()
                                 .Include(x => x.LawUnit)
                                    .ThenInclude(x => x.Courts)
                                    .Include(x => x.CourtGroup)

                                    .Where(x => x.CourtGroupId == CourtGroupId)
                                    .Where(x => x.DateFrom <= today && (x.DateTo ?? endDate) >= today)
                              .Where(x => x.LawUnit.Courts.Where(c => c.CourtId == courtID && NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(c.PeriodTypeId) && c.DateFrom <= today && (c.DateTo ?? endDate) >= today).Any()).Count();


                        }
                    }
                    // Когато са само ръчни до момента за деня се сметнат правилно среднодневните 2022.09.02
                    courtLoadPeriodLawUnit.AverageCases = courtLoadPeriodLawUnit.TotalDayCases / availableCount;

                    //Да изчислява средно дневни на база процента на натовареност  
                    totalAverage = courtLoadPeriodLawUnit.AverageCases;
                }
            }




            foreach (var lawUnit in courtLoadPeriodLawUnits)
            {

                if (lawUnit.LawUnitId != null)
                {

                    //Ако не е наличен променя само среднодневните бройки
                    if (lawUnit.IsAvailable == false && lawUnit.DayCases < 1M)
                    {
                        lawUnit.AverageCases = totalAverage * lawUnit.LoadIndex / 100;
                        lawUnit.TotalDayCases = totalAverage * lawUnit.LoadIndex / 100;
                    }

                    //Ако е избран увеличава  дневната му ставка
                    if (lawUnit.LawUnitId == selectedLawUnit)
                    {
                        ////// Когато не е изключван до момента но му се разпредели дело става  наличен за целия ден 2021.11.19
                        lawUnit.DayCases = lawUnit.DayCases + 1;
                        lawUnit.TotalDayCases = lawUnit.DayCases;
                        lawUnit.IsAvailable = true;
                        if (lawUnit.AverageCases > 0M)
                        {
                            lawUnit.AverageCases = 0M;
                        }


                        //lawUnit.DayCases = lawUnit.DayCases + 1;
                        //lawUnit.TotalDayCases = lawUnit.TotalDayCases + 1;

                        ////// Когато не е изключван до момента но му се разпредели дело става  наличен за целия ден 2021.11.19
                    }

                    //foreach (var item in caseSelectionProtocol.LawUnits)
                    //{
                    //  if (item.LawUnitId==lawUnit.LawUnitId)
                    //  {
                    //    item.TotalCaseCount = lawUnit.TotalDayCases;

                    //  }
                    //}


                }
            }
            repo.SaveChanges();

        }

        public void MergeCaseSelectionProtokolAndVM(CaseSelectionProtokol caseSelectionProtokol, CaseSelectionProtokolVM caseSelectionProtokolVM)
        {

            foreach (var item in caseSelectionProtokol.LawUnits)
            {
                foreach (var vm_item in caseSelectionProtokolVM.LawUnits)
                {
                    if (item.LawUnitId == vm_item.LawUnitId)
                    { item.CaseCount = vm_item.CaseCount; }
                }
            }
        }
        public void SubstitutionMergeCaseSelectionProtokolAndVM(CaseSelectionProtokolSubstitution substitutionSelectionProtokol, CaseSelectionProtokolVM caseSelectionProtokolVM)
        {

            foreach (var item in substitutionSelectionProtokol.LawUnits)
            {
                foreach (var vm_item in caseSelectionProtokolVM.LawUnits)
                {
                    if (item.LawUnitId == vm_item.LawUnitId)
                    { item.CaseCount = vm_item.CaseCount; }
                }
            }
        }

        public IQueryable<CourtLoadResetPeriod> CourtLoadResetPeriod_Select(int CourtId)
        {
            return repo.AllReadonly<CourtLoadResetPeriod>()
                       .Where(x => x.CourtId == CourtId)
                       .AsQueryable();
        }

        public bool CourtLoadResetPeriod_SaveData(CourtLoadResetPeriod model)
        {
            try
            {
                if (model.Id > 0)
                {
                    var saved = repo.GetById<CourtLoadResetPeriod>(model.Id);
                    saved.Description = model.Description;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo.MakeEndDate();
                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    model.DateTo = model.DateTo.MakeEndDate();
                    model.UserId = userContext.UserId;
                    model.DateWrt = DateTime.Now;
                    repo.Add(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CourtLoadResetPeriod Id={model.Id}");
                return false;
            }
        }


        public IEnumerable<CourtLoadResetPeriod> Get_CourtLoadResetPeriod_CrossPeriod(CourtLoadResetPeriod newPeriod)
        {
            DateTime futureData = new DateTime(2100, 12, 31);
            return repo.AllReadonly<CourtLoadResetPeriod>()
                       .Where(x => x.CourtId == newPeriod.CourtId)
                       .Where(x => x.Id != newPeriod.Id)
                       .Where(x => (((x.DateFrom >= newPeriod.DateFrom) && (x.DateFrom <= (newPeriod.DateTo ?? futureData))) || ((newPeriod.DateFrom >= x.DateFrom) && (newPeriod.DateFrom <= (x.DateTo ?? futureData)))))
                       .ToList();
        }




        //Преизчислява редовете с дневни брой дела за група /дежурство по избран съдия
        public void UpdateDailyLoadPeriod_RemoveByDismisal(int case_lawunit_id)

        {
            
            var case_law_unit = repo.GetById<CaseLawUnit>(case_lawunit_id);


           

            CourtLoadPeriod courtLoadPeriod = GetLoadPeriod(case_law_unit.CourtGroupId, case_law_unit.CourtDutyId);

            Expression<Func<CourtLoadPeriodLawUnit, bool>> courtLoadPeriodRowsLawunitsSelect = x => true;
            if ((courtLoadPeriod.CourtDutyId ?? 0) < 1)
            {
                //Ако е група
                courtLoadPeriodRowsLawunitsSelect = x => (x.CourtLoadPeriodId == courtLoadPeriod.Id) && (x.SelectionDate.Date == case_law_unit.DateFrom.Date);
            }
            else
            {
                //Ако е по дежурство
                courtLoadPeriodRowsLawunitsSelect = x => (x.CourtLoadPeriodId == courtLoadPeriod.Id);
            }


            var courtLoadPeriodLawUnits = repo.All<CourtLoadPeriodLawUnit>().Where(courtLoadPeriodRowsLawunitsSelect).ToList();

            decimal totalAverage = 0;
            decimal totalDivisor = 1;

            //Изчислява сумарния дневен ред
            foreach (var courtLoadPeriodLawUnit in courtLoadPeriodLawUnits)
            {
                if (courtLoadPeriodLawUnit.LawUnitId == null)
                {
                    courtLoadPeriodLawUnit.DayCases = courtLoadPeriodLawUnit.DayCases - 1;
                    courtLoadPeriodLawUnit.TotalDayCases = courtLoadPeriodLawUnit.TotalDayCases - 1;
                    //courtLoadPeriodLawUnit.AverageCases = courtLoadPeriodLawUnit.DayCases / (courtLoadPeriodLawUnits.Where(x => x.IsAvailable && x.LawUnitId != null).Count());
                    // courtLoadPeriodLawUnit.AverageCases = courtLoadPeriodLawUnit.DayCases / (courtLoadPeriodLawUnits.Where(x => (x.IsAvailable || x.DayCases > 0) && x.LawUnitId != null && x.AverageCases == 0).Count());
                    if (courtLoadPeriodLawUnits.Where(x => (x.IsAvailable || x.DayCases > 0) && x.LawUnitId != null && x.AverageCases == 0).Count() > 0)
                    { 
                        totalDivisor = courtLoadPeriodLawUnits.Where(x => (x.IsAvailable || x.DayCases > 0) && x.LawUnitId != null && x.AverageCases == 0).Count();
                    }
                    courtLoadPeriodLawUnit.AverageCases = courtLoadPeriodLawUnit.DayCases / totalDivisor;
                    totalAverage = courtLoadPeriodLawUnit.AverageCases;
                }
            }




            foreach (var lawUnit in courtLoadPeriodLawUnits)
            {

                if (lawUnit.LawUnitId != null)
                {

                    //Ако не е наличен променя само среднодневните бройки
                    if (lawUnit.IsAvailable == false && lawUnit.DayCases < 1M)
                    {


                        lawUnit.AverageCases = totalAverage * lawUnit.LoadIndex / 100;
                        lawUnit.TotalDayCases = totalAverage * lawUnit.LoadIndex / 100;
                    }

                    //Ако е избран намалява дневната му ставка
                    if (lawUnit.LawUnitId == case_law_unit.LawUnitId)
                    {
                        if (lawUnit.DayCases > 0)
                        {
                            lawUnit.DayCases = lawUnit.DayCases - 1;
                        }
                        if (lawUnit.TotalDayCases > 0)
                        {
                            lawUnit.TotalDayCases = lawUnit.TotalDayCases - 1;
                        }

                    }




                }
            }
            repo.SaveChanges();
         

            
           

        }



        /// <summary>
        /// Коригира среднодневните на база смяна на процента на анатоварване в групата
        /// </summary>
        /// <returns></returns>
        public CourtLoadPeriodLawUnit UpdateChangedProcentAverageCases(int lawunitId, int courtGroupId, decimal newPercent)
        {
            CourtLoadPeriodLawUnit result = null;
            try
            {


                var lastPreviousPervcent = repo.AllReadonly<CourtLawUnitGroup>()
                                              .Where(x => x.LawUnitId == lawunitId)
                                               .Where(x => x.CourtGroupId == courtGroupId)
                                               .OrderByDescending(x => x.DateFrom)
                                               .FirstOrDefault();
                DateTime dtNow = DateTime.Now;
                var lawunitsPeriodList = repo.AllReadonly<CourtLoadPeriodLawUnit>()
                                            .Where(x => x.LawUnitId == lawunitId || x.LawUnitId == null)
                                            .Where(x => x.CourtLoadPeriod.CourtGroupId == courtGroupId && x.CourtLoadPeriod.CourtDutyId == null)
                                            .Where(x => (x.CourtLoadPeriod.CourtLoadResetPeriod.DateTo ?? dtNow) >= dtNow)
                                             .Where(x => x.CourtLoadPeriod.CourtLoadResetPeriod.DateFrom < dtNow).ToList();

                //Ако се добавя за първи път в тази група 2024.01.30
                if (lastPreviousPervcent == null)
                { // и няма разпределени ръчнодела

                    if (!lawunitsPeriodList.Where(x => x.LawUnitId == lawunitId).Any())
                    {
                        return result;
                    }
                    //end-- и няма разпределени ръчнодела

                    //има разпределени  ръчно  без да е бил в тази група 2024.01.30

                    if (lawunitsPeriodList.Where(x => x.LawUnitId == lawunitId).Any())
                    {

                        decimal all_average_cases = lawunitsPeriodList.Where(x => x.LawUnitId == null)

                                                                   .Where(x => x.SelectionDate <= dtNow.Date).Sum(x => x.AverageCases) * newPercent / 100;


                        decimal real_cases = lawunitsPeriodList.Where(x => x.LawUnitId == lawunitId)

                                                                          .Where(x => x.SelectionDate <= dtNow.Date).Sum(x => x.DayCases);

                        decimal average_cases = lawunitsPeriodList.Where(x => x.LawUnitId == lawunitId)

                                                                        .Where(x => x.SelectionDate <= dtNow.Date).Sum(x => x.AverageCases);
                        result = new CourtLoadPeriodLawUnit();
                        result.CourtLoadPeriodId = lawunitsPeriodList.FirstOrDefault().CourtLoadPeriodId;
                        result.LawUnitId = lawunitId;
                        result.SelectionDate = lawunitsPeriodList.Where(x => x.LawUnitId == lawunitId).Select(x => x.SelectionDate).Min().AddDays(-1);
                        //result.IsAvailable = false;
                        result.IsAvailable = true;
                        result.DayCases = 0;
                        //залага се долната граница на 10% отклонение 100/110
                        result.AverageCases = ((all_average_cases) / 100 * newPercent * 100 / 110) - real_cases - average_cases;
                        result.TotalDayCases = result.AverageCases;
                        result.LoadIndex = newPercent;


                        return result;
                    }
                    //end-- има разпределени  ръчно  без да е бил в тази група2024.01.30
                }


                //Ако няма записи за разпределения в групата
                if (!lawunitsPeriodList.Where(x => x.LawUnitId == lawunitId).Any())
                {
                    return result;
                }
                //Ако е бил в групата  но е изключен за даден период
                if (lastPreviousPervcent.DateTo != null)

                {
                    //Средно-днвени за времето на отсъствие
                    decimal absent_average_cases = lawunitsPeriodList.Where(x => x.LawUnitId == null)
                                                                     .Where(x => x.SelectionDate >= (lastPreviousPervcent.DateTo ?? dtNow).Date)
                                                                      .Where(x => x.SelectionDate <= dtNow.Date).Sum(x => x.AverageCases) * lastPreviousPervcent.LoadIndex / 100;

                    decimal real_cases = lawunitsPeriodList.Where(x => x.LawUnitId == lawunitId)

                                                                      .Where(x => x.SelectionDate <= dtNow.Date).Sum(x => x.DayCases);

                    decimal average_cases = lawunitsPeriodList.Where(x => x.LawUnitId == lawunitId)

                                                                    .Where(x => x.SelectionDate <= dtNow.Date).Sum(x => x.AverageCases);

                    result = new CourtLoadPeriodLawUnit();
                    result.CourtLoadPeriodId = lawunitsPeriodList.FirstOrDefault().CourtLoadPeriodId;
                    result.LawUnitId = lawunitId;
                    result.SelectionDate = lawunitsPeriodList.Where(x => x.LawUnitId == lawunitId).Select(x => x.SelectionDate).Min().AddDays(-1);
                    //result.IsAvailable = false;
                    result.IsAvailable = true;
                    result.DayCases = 0;
                    result.AverageCases = ((absent_average_cases + real_cases + average_cases) / lastPreviousPervcent.LoadIndex * newPercent) - real_cases - average_cases;
                    result.TotalDayCases = result.AverageCases;
                    result.LoadIndex = newPercent;


                    return result;
                }
                //Ако е бил в групата и сменя процента
                if (lastPreviousPervcent.DateTo == null)

                {
                    decimal real_cases = lawunitsPeriodList.Where(x => x.LawUnitId == lawunitId)

                                                                 .Where(x => x.SelectionDate <= dtNow.Date).Sum(x => x.DayCases);

                    decimal average_cases = lawunitsPeriodList.Where(x => x.LawUnitId == lawunitId)

                                                                    .Where(x => x.SelectionDate <= dtNow.Date).Sum(x => x.AverageCases);

                    result = new CourtLoadPeriodLawUnit();
                    result.CourtLoadPeriodId = lawunitsPeriodList.FirstOrDefault().CourtLoadPeriodId;
                    result.LawUnitId = lawunitId;
                    result.SelectionDate = lawunitsPeriodList.Where(x => x.LawUnitId == lawunitId).Select(x => x.SelectionDate).Min().AddDays(-1);
                    //result.IsAvailable = false;
                    result.IsAvailable = true;
                    result.DayCases = 0;
                    result.AverageCases = ((real_cases + average_cases) / lastPreviousPervcent.LoadIndex * newPercent) - real_cases - average_cases;
                    result.TotalDayCases = result.AverageCases;
                    result.LoadIndex = newPercent;
                    return result;
                }
            }
            catch (Exception)
            {


            }
            return result;
        }

        /// <summary>
        ///При автоматичен отвод за съдия докладчик по делото намалява бройката с дело  но не влияеща в/у разпределението му (добавя средно дневни)
        /// </summary>
        /// <param name="case_lawunit_id"></param>

        public void UpdateDailyLoadPeriod_RemoveByAutomaticDismisal(int case_lawunit_id)

        {
            try
            {
                var case_law_unit = repo.GetById<CaseLawUnit>(case_lawunit_id);




                CourtLoadPeriod courtLoadPeriod = GetLoadPeriod(case_law_unit.CourtGroupId, case_law_unit.CourtDutyId);

                Expression<Func<CourtLoadPeriodLawUnit, bool>> courtLoadPeriodRowsLawunitsSelect = x => true;
                if ((courtLoadPeriod.CourtDutyId ?? 0) < 1)
                {
                    //Ако е група
                    courtLoadPeriodRowsLawunitsSelect = x => (x.CourtLoadPeriodId == courtLoadPeriod.Id) && (x.SelectionDate.Date == case_law_unit.DateFrom.Date);
                }
                else
                {
                    //Ако е по дежурство
                    courtLoadPeriodRowsLawunitsSelect = x => (x.CourtLoadPeriodId == courtLoadPeriod.Id);
                }


                var courtLoadPeriodLawUnit = repo.All<CourtLoadPeriodLawUnit>()
                                                  .Where(courtLoadPeriodRowsLawunitsSelect)
                                                  .Where(x => x.LawUnitId == case_law_unit.LawUnitId).FirstOrDefault();


                // 2024.07.08 Бройки се променят само ако делото е в актуален период/ Когато е в минал няма да се променят(т.к. в разпределението участва само актуалния)

                if (courtLoadPeriodLawUnit != null)
                {
                    courtLoadPeriodLawUnit.AverageCases = courtLoadPeriodLawUnit.AverageCases + 1;
                    if (courtLoadPeriodLawUnit.DayCases > 0)
                    {
                        courtLoadPeriodLawUnit.DayCases = courtLoadPeriodLawUnit.DayCases - 1;
                    }



                    repo.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка При автоматичен отвод за съдия докладчик по делото намалява бройката с дело  но не влияеща в/у разпределението му (добавя средно дневни) case_lawunit_id ={case_lawunit_id}");

            }
        }



        //При автоматично масово преразпределене да добави едно дело и намаля с -1 средно дневно за да не влияе на разпределенията
        public void UpdateDailyLoadPeriod_AutomaticMassSelection(int? CourtGroupId, int? CourtDutyId, int selectedLawUnit)

        {
            CourtLoadPeriod courtLoadPeriod = GetLoadPeriod(CourtGroupId, CourtDutyId);

            Expression<Func<CourtLoadPeriodLawUnit, bool>> courtLoadPeriodRowsLawunitsSelect = x => true;
            if (CourtDutyId == null)
            {
                //Ако е група
                courtLoadPeriodRowsLawunitsSelect = x => (x.CourtLoadPeriodId == courtLoadPeriod.Id) && (x.SelectionDate.Date == DateTime.Now.Date);
            }
            else
            {
                //Ако е по дежурство
                courtLoadPeriodRowsLawunitsSelect = x => (x.CourtLoadPeriodId == courtLoadPeriod.Id);
            }


            var courtLoadPeriodLawUnit = repo.All<CourtLoadPeriodLawUnit>().Where(courtLoadPeriodRowsLawunitsSelect).Where(x => x.LawUnitId == selectedLawUnit).FirstOrDefault();

            courtLoadPeriodLawUnit.DayCases = courtLoadPeriodLawUnit.DayCases + 1;
            if (courtLoadPeriodLawUnit.AverageCases > 0)
            {
                courtLoadPeriodLawUnit.AverageCases = courtLoadPeriodLawUnit.AverageCases - 1;
            }




            repo.SaveChanges();

        }

    }



}
