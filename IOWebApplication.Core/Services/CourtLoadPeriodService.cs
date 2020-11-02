// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

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
            if (curentLawUnit.StateId == NomenclatureConstants.SelectionProtokolLawUnitState.Absent)
            { currentCourtLoadPeriodLawUnit.IsAvailable = false; }
            else
            { currentCourtLoadPeriodLawUnit.IsAvailable = true; }
            currentCourtLoadPeriodLawUnit.DayCases = 0;
            currentCourtLoadPeriodLawUnit.TotalDayCases = 0;
            currentCourtLoadPeriodLawUnit.LoadIndex = curentLawUnit.LoadIndex;
            currentCourtLoadPeriodLawUnit.AverageCases = 0;
            //Когато този потребител се включва на по късен етап и получава  приравнителни средно дневни
            if (courtLoadPeriodLawUnit.ToList().Count == 0 && courtLoadPeriodLawUnitTotal.Where(x => x.SelectionDate < DateTime.Now.Date).ToList().Count > 0)
            {

              foreach (var row in courtLoadPeriodLawUnitTotal.Where(x => x.SelectionDate != DateTime.Now.Date).ToList())
              {
                currentCourtLoadPeriodLawUnit.AverageCases = currentCourtLoadPeriodLawUnit.AverageCases + row.AverageCases;
                currentCourtLoadPeriodLawUnit.TotalDayCases = currentCourtLoadPeriodLawUnit.TotalDayCases + row.AverageCases;
              }
              repo.Add<CourtLoadPeriodLawUnit>(currentCourtLoadPeriodLawUnit);
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
        logger.LogError(ex, $"Грешка при запис на информация за разпределение член съдебен състав lawUnitId={ lawUnitId}");
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
        logger.LogError(ex, $"Грешка при запис на обобщена информация за разпределение за courtLoadPeriodId={ courtLoadPeriodId}");
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
                                            .Where(x => (x.CourtLoadResetPeriod.DateTo ?? DateTime.Now) >= DateTime.Now)
                                            .Where(courtGroupDutySearch).FirstOrDefault();


      if (courtLoadPeriod == null)
      {

        try
        {
          var resetPeriodId = repo.AllReadonly<CourtLoadResetPeriod>()
                                          .Where(x => x.CourtId == userContext.CourtId)
                                          .Where(x => x.DateFrom <= DateTime.Now)
                                          .Where(x => (x.DateTo ?? DateTime.Now) >= DateTime.Now)
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
            logger.LogError(ex, $"Грешка при запис на период за група  courtGroupId ={ courtGroupid}");
          }
          else
          {
            logger.LogError(ex, $"Грешка при запис на период за група  courtDutyId ={ courtDutyId}");
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
      int totalPeriodDays = repo.AllReadonly<CourtLoadPeriodLawUnit>()
                              .Where(x => x.CourtLoadPeriodId == courtLoadPeriod.Id)
                              .Where(x => (x.LawUnitId ?? 0) == 0)
                              .Count();

      

      decimal totalKoef = 0;
      // foreach (var lawUnit in caseSelectionProtocol.LawUnits.Where(x => NomenclatureConstants.SelectionProtokolLawUnitState.ActiveState.Contains(x.StateId)))
      foreach (var lawUnit in caseSelectionProtocol.LawUnits)
      {
        CalculateLawUnitDataInGroup(lawUnit, courtLoadPeriod.Id);
        totalKoef = totalKoef + lawUnit.Koef;

      }
      // foreach (var lawUnit in caseSelectionProtocol.LawUnits.Where(x => NomenclatureConstants.SelectionProtokolLawUnitState.ActiveState.Contains(x.StateId)))
      foreach (var lawUnit in caseSelectionProtocol.LawUnits)
      {
        lawUnit.KoefNormalized = lawUnit.Koef / totalKoef * 100M;

        ////Когато нормализираният коефициент е прекалено малък го приравняваме на 1 за да има поне 1 участие  като вероятност
        //if ((lawUnit.KoefNormalized > 0) && (lawUnit.KoefNormalized < 1))
        //{ lawUnit.KoefNormalized = 1; }

        int lawUnitPeriodDays = repo.AllReadonly<CourtLoadPeriodLawUnit>()
                              .Where(x => x.CourtLoadPeriodId == courtLoadPeriod.Id)
                              .Where(x => x.LawUnitId  == lawUnit.LawUnitId)
                              .Count();
        lawUnit.CasesCountIfWorkAllPeriodInGroup = (decimal)totalPeriodDays/(decimal)lawUnitPeriodDays * 100M /(decimal)lawUnit.LoadIndex * lawUnit.CaseCount;
        lawUnit.ExcludeByBigDeviation = false;
      }

      return caseSelectionProtocol;
    }
   

    //Изчислява коефициентите за всеки един съдия в рамките на група или дежурство
    public void CalculateLawUnitDataInGroup(CaseSelectionProtokolLawUnitVM caseSelectionProtokolLawUnit, int courtLoadPeriodId)

    {
      var courtLoadPeriodLawunits = repo.AllReadonly<CourtLoadPeriodLawUnit>().Where(x => x.LawUnitId == caseSelectionProtokolLawUnit.LawUnitId)
                                                                           .Where(x => x.CourtLoadPeriodId == courtLoadPeriodId).ToList();
      foreach (var courtLoadPeriodLawunit in courtLoadPeriodLawunits)
      {
        caseSelectionProtokolLawUnit.TotalCaseCount = caseSelectionProtokolLawUnit.TotalCaseCount + courtLoadPeriodLawunit.TotalDayCases;
        caseSelectionProtokolLawUnit.CaseCount = caseSelectionProtokolLawUnit.CaseCount + (int)courtLoadPeriodLawunit.DayCases;
      }
      if (caseSelectionProtokolLawUnit.TotalCaseCount > 0)
      {
        caseSelectionProtokolLawUnit.Koef = caseSelectionProtokolLawUnit.LoadIndex / caseSelectionProtokolLawUnit.TotalCaseCount;
      }
      else
      //Когато разпределените дела са 0 на някой  се т.к деление на 0 е невъзможно даваме вместо 0 малка стойноат 0.01 
      { caseSelectionProtokolLawUnit.Koef = caseSelectionProtokolLawUnit.LoadIndex * 100; }






    }
    //Преизчислява редовете с дневни брой дела за група /дежурство по избран съдия
    public void UpdateDailyLoadPeriod(int? CourtGroupId, int? CourtDutyId, int selectedLawUnit)

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


      var courtLoadPeriodLawUnits = repo.All<CourtLoadPeriodLawUnit>().Where(courtLoadPeriodRowsLawunitsSelect);

      decimal totalAverage = 0;

      //Изчислява сумарния дневен ред
      foreach (var courtLoadPeriodLawUnit in courtLoadPeriodLawUnits)
      {
        if (courtLoadPeriodLawUnit.LawUnitId == null)
        {
          courtLoadPeriodLawUnit.TotalDayCases = courtLoadPeriodLawUnit.TotalDayCases + 1;
          courtLoadPeriodLawUnit.DayCases = courtLoadPeriodLawUnit.DayCases + 1;
          courtLoadPeriodLawUnit.AverageCases = courtLoadPeriodLawUnit.TotalDayCases / (courtLoadPeriodLawUnits.Count() - 1);
          totalAverage = courtLoadPeriodLawUnit.AverageCases;
        }
      }




      foreach (var lawUnit in courtLoadPeriodLawUnits)
      {

        if (lawUnit.LawUnitId != null)
        {

          //Ако не е наличен променя само среднодневните бройки
          if (lawUnit.IsAvailable == false)
          {
            lawUnit.AverageCases = totalAverage;
            lawUnit.TotalDayCases = totalAverage;
          }

          //Ако е избран увеличава  дневната му ставка
          if (lawUnit.LawUnitId == selectedLawUnit)
          {
            lawUnit.DayCases = lawUnit.DayCases + 1;
            lawUnit.TotalDayCases = lawUnit.TotalDayCases + 1;
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
        logger.LogError(ex, $"Грешка при запис на CourtLoadResetPeriod Id={ model.Id }");
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


      var courtLoadPeriodLawUnits = repo.All<CourtLoadPeriodLawUnit>().Where(courtLoadPeriodRowsLawunitsSelect);

      decimal totalAverage = 0;

      //Изчислява сумарния дневен ред
      foreach (var courtLoadPeriodLawUnit in courtLoadPeriodLawUnits)
      {
        if (courtLoadPeriodLawUnit.LawUnitId == null)
        {
          courtLoadPeriodLawUnit.DayCases = courtLoadPeriodLawUnit.DayCases - 1;
          courtLoadPeriodLawUnit.TotalDayCases = courtLoadPeriodLawUnit.TotalDayCases - 1;
          courtLoadPeriodLawUnit.AverageCases = courtLoadPeriodLawUnit.DayCases / (courtLoadPeriodLawUnits.Count() - 1);
          totalAverage = courtLoadPeriodLawUnit.AverageCases;
        }
      }




      foreach (var lawUnit in courtLoadPeriodLawUnits)
      {

        if (lawUnit.LawUnitId != null)
        {

          //Ако не е наличен променя само среднодневните бройки
          if (lawUnit.IsAvailable == false)
          {
            lawUnit.AverageCases = totalAverage;
            lawUnit.TotalDayCases = totalAverage;
          }

          //Ако е избран намалява дневната му ставка
          if (lawUnit.LawUnitId == case_law_unit.LawUnitId)
          {
            lawUnit.DayCases = lawUnit.DayCases - 1;
            lawUnit.TotalDayCases = lawUnit.TotalDayCases - 1;
          }




        }
      }
      repo.SaveChanges();

    }

  }



}
