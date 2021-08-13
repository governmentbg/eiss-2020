using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.Integrations.CSRD;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using IOWebApplication.Core.Extensions;

namespace IOWebApplication.Core.Services
{
  public class CaseSelectionProtokolService : BaseService, ICaseSelectionProtokolService

  {
    private readonly INomenclatureService nomService;
    private readonly ICourtLoadPeriodService courtLoadPeriodService;
    private readonly IMQEpepService mqEpepService;
    private readonly IWorkNotificationService notificationService;
    private readonly ICaseDeadlineService caseDeadlineService;
    private readonly ICommonService commonService;
    private readonly ICourtLawUnitService courtLawUnitService;
    private readonly ICdnService cdnService;
    private readonly IWorkTaskService worktaskService;
    public CaseSelectionProtokolService(ILogger<CaseSelectionProtokolService> _logger,
        IRepository _repo,
        IUserContext _userContext,
        ICourtLoadPeriodService _courtLoadPeriodService,
        IMQEpepService _mqEpepService,
        IWorkNotificationService _notificationService,
        INomenclatureService _nomService,
        ICommonService _commonService,
        ICourtLawUnitService _courtLawUnitService,
        ICaseDeadlineService _caseDeadlineService,
       ICdnService _cdnService,
       IWorkTaskService _worktaskService)

    {
      logger = _logger;
      repo = _repo;
      userContext = _userContext;
      courtLoadPeriodService = _courtLoadPeriodService;
      nomService = _nomService;
      mqEpepService = _mqEpepService;
      notificationService = _notificationService;
      commonService = _commonService;
      caseDeadlineService = _caseDeadlineService;
      cdnService = _cdnService;
      courtLawUnitService = _courtLawUnitService;
      worktaskService = _worktaskService;
    }
    /// <summary>
    /// Зарежда списък с спротокли за избор на съдебен състав
    /// </summary>
    /// <param name="caseId">ИД на дело</param>
    /// <returns></returns>
    public IQueryable<CaseSelectionProtokolListVM> CaseSelectionProtokol_Select(int caseId)
    {
      return repo.AllReadonly<CaseSelectionProtokol>()
          .Include(x => x.JudgeRole)
          .Include(x => x.SelectionMode)
          .Include(x => x.SelectedLawUnit)
          .Include(x => x.SelectionProtokolState)
          .Where(x => x.CaseId == caseId)
          .Select(x => new CaseSelectionProtokolListVM()
          {
            Id = x.Id,
            SelectionDate = x.SelectionDate,
            JudgeRoleName = x.JudgeRole.Label,
            SelectionModeName = x.SelectionMode.Label,
            SelectedLawUnitName = x.SelectedLawUnit.FullName,
            SelectionProtokolStateName = x.SelectionProtokolState.Label
          }).AsQueryable();
    }
    /// <summary>
    /// Добавя данни натоварване за разпределения към модела
    /// </summary>
    /// <param name="caseSelectionProtokol">Модел на протокола</param>
    /// <param name="caseSelectionProtokolVM">Визуален модел</param>
    private void SetSelectedLawUnit_SaveCaseLawUnit(CaseSelectionProtokol caseSelectionProtokol, CaseSelectionProtokolVM caseSelectionProtokolVM)
    {

      courtLoadPeriodService.MakeDaylyLoadPeriodLawuitRowsByGroup(caseSelectionProtokolVM);
      courtLoadPeriodService.CalculateAllKoef(caseSelectionProtokolVM);

      ///Изключване за голямо отклонение
      ///
      decimal case_deviation = 10;
      decimal deviation_percent = 10;

      try
      {
        var param_deviation = SystemParam_Select("case_deviation");
        case_deviation = decimal.Parse(param_deviation.ParamValue);
      }
      catch (Exception)
      {


      }

      try
      {
        var param_deviation_percent = SystemParam_Select("case_deviation_percent");
        deviation_percent = decimal.Parse(param_deviation_percent.ParamValue);
      }
      catch (Exception)
      {


      }
      foreach (var lu1 in caseSelectionProtokolVM.LawUnits)

      {
        if (NomenclatureConstants.SelectionProtokolLawUnitState.ActiveState.Contains(lu1.StateId) && lu1.CaseCount > case_deviation)
        {

          foreach (var lu2 in caseSelectionProtokolVM.LawUnits.Where(x => NomenclatureConstants.SelectionProtokolLawUnitState.ActiveState.Contains(x.StateId)))

          {
            if (lu1.LawUnitId != lu2.LawUnitId)
            {
              if (lu1.CasesCountIfWorkAllPeriodInGroup > lu2.CasesCountIfWorkAllPeriodInGroup && NomenclatureConstants.SelectionProtokolLawUnitState.ActiveState.Contains(lu2.StateId))
              {
                decimal diff = lu1.CasesCountIfWorkAllPeriodInGroup - lu2.CasesCountIfWorkAllPeriodInGroup;
                decimal deviation = diff / (lu2.CasesCountIfWorkAllPeriodInGroup + (decimal)0.001) * 100M;
                //Залагам отклонение 40%  до 10 дела  и отспоред конфигурацията  над 10
                decimal curent_deviation_percent = deviation_percent;
                if (lu1.CaseCount < 10)
                { curent_deviation_percent = 40; }
                if (deviation > curent_deviation_percent)
                {
                  lu1.ExcludeByBigDeviation = true;
                  break;
                }


              }
            }


          }
        }
      }
      //Изключване за голямо отклонение


      List<int> lawUnits = new List<int>();
      var protokolLawUnits = new List<CaseSelectionProtokolLawUnitVM>();

      decimal activeNormalizedKoef = 0;
      foreach (var item in caseSelectionProtokolVM.LawUnits)
      {
        if (NomenclatureConstants.SelectionProtokolLawUnitState.ActiveState.Contains(item.StateId) && item.ExcludeByBigDeviation == false)
        {
          CaseSelectionProtokolLawUnitVM itemNew = new CaseSelectionProtokolLawUnitVM();
          itemNew.LawUnitId = item.LawUnitId;
          itemNew.KoefNormalized = item.KoefNormalized;
          activeNormalizedKoef = activeNormalizedKoef + item.KoefNormalized;
          protokolLawUnits.Add(itemNew);
        }
      }
      //Нормализиране на активнике коефициенти към 1000
      foreach (var item in protokolLawUnits)
      {
        item.KoefNormalized = item.KoefNormalized / activeNormalizedKoef * 1000;


      }
      //int maxLoadindex = (protokolLawUnits.Select(x => x.KoefNormalized).Max());
      bool exitLoop = false;

      //Добавя билети размесени START
      //while (exitLoop == false)

      //{
      //  bool allKoefFinished = true;
      //  foreach (var item in protokolLawUnits)
      //  {
      //    if (item.KoefNormalized <= 0) continue;

      //    if (item.KoefNormalized >= (decimal)0.5)
      //    {
      //      lawUnits.Add(item.LawUnitId);
      //      item.KoefNormalized--;
      //      allKoefFinished = false;
      //    }
      //  }
      //  if (allKoefFinished)
      //  {
      //    exitLoop = true;
      //  }
      //}
      //Добавя билети размесени END

      //Добавя билети последователно Start


      foreach (var item in protokolLawUnits)
      {
        while (item.KoefNormalized > 0)
        {
          lawUnits.Add(item.LawUnitId);
          item.KoefNormalized = item.KoefNormalized - 1;
        }
      }


      //Добавя билети последователно END
      //Избор след като 2 пъти рандом се избере един  съдия
      List<int> LawUnnitsSelectedForFirstTime = new List<int>();
      exitLoop = false;
      int r = 0;
      while (exitLoop == false)
      {
        Random rnd = new Random();
        r = rnd.Next(lawUnits.Count);

        if (LawUnnitsSelectedForFirstTime.Contains(lawUnits[r]))
        {
          caseSelectionProtokol.SelectedLawUnitId = lawUnits[r];
          exitLoop = true;
        }
        else
        { LawUnnitsSelectedForFirstTime.Add(lawUnits[r]); }


      }





      if (caseSelectionProtokolVM.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
      {

        courtLoadPeriodService.UpdateDailyLoadPeriod(caseSelectionProtokolVM.CourtGroupId, caseSelectionProtokolVM.CourtDutyId, lawUnits[r]);
      }
      courtLoadPeriodService.MergeCaseSelectionProtokolAndVM(caseSelectionProtokol, caseSelectionProtokolVM);

    }

    //private void SetSelectedLawUnit_SaveCaseLawUnit(CaseSelectionProtokol caseSelectionProtokol, CaseSelectionProtokolVM caseSelectionProtokolVM)
    //{

    //  courtLoadPeriodService.MakeDaylyLoadPeriodLawuitRowsByGroup(caseSelectionProtokolVM);
    //  courtLoadPeriodService.CalculateAllKoef(caseSelectionProtokolVM);


    //  List<int> lawUnits = new List<int>();
    //  var protokolLawUnits = new List<CaseSelectionProtokolLawUnitVM>();

    //  decimal activeNormalizedKoef = 0;
    //  foreach (var item in caseSelectionProtokolVM.LawUnits)
    //  {
    //    if (NomenclatureConstants.SelectionProtokolLawUnitState.ActiveState.Contains(item.StateId))
    //    {
    //      CaseSelectionProtokolLawUnitVM itemNew = new CaseSelectionProtokolLawUnitVM();
    //      itemNew.LawUnitId = item.LawUnitId;
    //      itemNew.KoefNormalized = item.KoefNormalized;
    //      activeNormalizedKoef = activeNormalizedKoef + item.KoefNormalized;
    //      protokolLawUnits.Add(itemNew);
    //    }
    //  }
    //  //Нормализиране на активнике коефициенти към 100
    //  foreach (var item in protokolLawUnits)
    //  {
    //    item.KoefNormalized = item.KoefNormalized / activeNormalizedKoef * 100;


    //  }
    //  //int maxLoadindex = (protokolLawUnits.Select(x => x.KoefNormalized).Max());
    //  for (int i = 101; i >= 1; i--)
    //  {
    //    foreach (var item in protokolLawUnits)
    //    {
    //      if (item.KoefNormalized <= 0) continue;
    //      decimal percent = (decimal)item.KoefNormalized / (decimal)i;
    //      if (percent >= (decimal)0.5)
    //      {
    //        lawUnits.Add(item.LawUnitId);
    //        item.LoadIndex--;
    //      }
    //    }
    //  }

    //  Random rnd = new Random();
    //  int r = rnd.Next(lawUnits.Count);
    //  caseSelectionProtokol.SelectedLawUnitId = lawUnits[r];


    //  if (caseSelectionProtokolVM.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
    //  {

    //    courtLoadPeriodService.UpdateDailyLoadPeriod(caseSelectionProtokolVM.CourtGroupId, caseSelectionProtokolVM.CourtDutyId, lawUnits[r]);
    //  }
    //  courtLoadPeriodService.MergeCaseSelectionProtokolAndVM(caseSelectionProtokol, caseSelectionProtokolVM);

    //}
    //Ако е съдия докладчик да провери дали вече има разпределен защото не може повече от един
    private bool CheckForJudgeReporter(CaseSelectionProtokolVM model, List<CaseLawUnit> caseLawUnits, ref string errorMessage)
    {
      if (model.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
      {
        DateTime dateEnd = DateTime.Now;
        var existJudgeReporter = caseLawUnits.Where(x => x.JudgeRoleId == model.JudgeRoleId && ((x.DateTo ?? dateEnd.AddDays(1)) >= dateEnd)).Any();
        if (existJudgeReporter == true)
        {
          errorMessage = "Вече има разпределен Съдия докладчик";
          return false;
        }
      }
      return true;
    }


    /// <summary>
    /// Проверка за присъствие на съдия в  в дело
    ///  //Ако един член вече го има разпределен по делото  и е активен към момента  или е разпределян и има отвод или самоотвод не
    //може да се разпределя пак
    /// </summary>
    /// <param name="model"> Модел на протокол</param>
    /// <param name="caseLawUnits"> списък с потребители</param>
    /// <param name="errorMessage">съобщение за грешка</param>
    /// <returns></returns>
    private bool CheckForExistLawUnit(CaseSelectionProtokolVM model, List<CaseLawUnit> caseLawUnits, ref string errorMessage)
    {
      bool result = true;
      var caseLawUnitDismis = repo.AllReadonly<CaseLawUnitDismisal>()
                                        .Include(x => x.CaseLawUnit)
                                        .Where(CaseExtensions.ConfirmedDismissalsOnly())
                                        .Where(x => x.CaseLawUnit.CaseId == model.CaseId).ToList();

      foreach (var item in model.LawUnits)
      {
        if (NomenclatureConstants.SelectionProtokolLawUnitState.ActiveState.Contains(item.StateId) == false) continue;

        //Когато всички са отведени  и е ръчен избор да не се проверява дали съдията е с отвод /самоотвод
        if (HasJudgeChoiceWithoutDismisal(model.CourtId, model.CaseId) && model.SelectionModeId == NomenclatureConstants.SelectionMode.ManualSelect)
        {
          //Проверяват се тези които са с отвод
          if (caseLawUnitDismis.Where(x => x.CaseLawUnit.LawUnitId == item.LawUnitId)
                                     .Where(x => NomenclatureConstants.DismisalType.DismisalList.Contains(x.DismisalTypeId)).Any())
          {
            result = false;

            if (errorMessage != "")
              errorMessage += ", ";
            errorMessage += item.LawUnitFullName + " е с отвод/самоотвод";
            continue;
          }
          //Проверяват се тези които са с отвод
        }
        //Когато всички са отведени  и е ръчен избор да не се проверява дали съдията е с отвод /самоотвод
        DateTime tempdate = DateTime.Now;
        //Проверяват се тези които вече са избрани

        var caseLawUnitsList = caseLawUnits.Where(x => x.LawUnitId == item.LawUnitId)
                        .Where(x => (x.DateTo ?? tempdate) >= tempdate).ToList();




        //if (caseLawUnits.Where(x => x.LawUnitId == item.LawUnitId)
        //                .Where(x => (x.DateTo ?? tempdate) >= tempdate).Any())

        if (caseLawUnitsList.Count > 0 && (model.JudgeRoleId != NomenclatureConstants.JudgeRole.JudgeReporter))
        {
          result = false;
          if (errorMessage != "")
            errorMessage += ", ";
          errorMessage += item.LawUnitFullName + " вече е разпределен по делото";
          continue;
        }
      }

      return result;
    }


    /// <summary>
    /// //Проверка дали отвода вече е зает
    /// </summary>
    /// <param name="model">Модел на протокол</param>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    private bool CheckForDismisal(CaseSelectionProtokolVM model, ref string errorMessage)
    {
      if ((model.CaseLawUnitDismisalId ?? 0) > 0)
      {
        var dismisal = repo.AllReadonly<CaseSelectionProtokol>().Where(x => x.Id != model.Id && x.CaseLawUnitDismisalId == model.CaseLawUnitDismisalId).Any();
        if (dismisal == true)
        {
          errorMessage = "По този отвод вече е направено разпределение";
          return false;
        }
      }
      return true;
    }


    /// <summary>
    /// //Проверка дали съдебният заседател не е активен
    /// </summary>
    /// <param name="model"> Модел на протокол</param>
    /// <param name="errorMessage">Съобщение за грешка</param>
    /// <returns></returns>
    private bool ChecкJury(CaseSelectionProtokolVM model, ref string errorMessage)
    {
      bool result = true;
      //Активен съдебен състав по дело
      if (NomenclatureConstants.JudgeRole.JudgeRolesList.Contains(model.JudgeRoleId))
      {
        var _caseLawUnit = repo.AllReadonly<CaseLawUnit>()
                                       .Where(x => x.DateFrom <= DateTime.Now)
                                       .Where(x => x.DateTo == null)
                                       .Where(x => x.CaseId == model.CaseId)
                                       .Select(x => x.LawUnitId).ToList();

        foreach (var item in model.LawUnits)
        {
          //Проверяват се тези Дали има заседател активен към момента на добявяне
          if (_caseLawUnit.Contains(item.LawUnitId))
          {
            result = false;
            if (errorMessage != "")
              errorMessage += ", ";
            errorMessage += item.LawUnitFullName + " вече е разпределен по делото";
            continue;
          }

        }
      }

      return result;
    }
    /// <summary>
    /// Запис на протокол
    /// </summary>
    /// <param name="model"> Модел на протокол</param>
    /// <param name="errorMessage">Съобщение за грешка</param>
    /// <returns></returns>
    public bool CaseSelectionProtokol_SaveData(CaseSelectionProtokolVM model, ref string errorMessage)
    {
      try
      {
        var caseLawUnits = repo.AllReadonly<CaseLawUnit>().Where(x => x.CaseId == model.CaseId && x.CaseSessionId == null).ToList();

        foreach (var item in model.LawUnits)
        {
          if (item.LawUnitFullName == null)
          {
            item.LawUnitFullName = repo.AllReadonly<LawUnit>().Where(x => x.Id == item.LawUnitId).FirstOrDefault().FullName;
          }
        }

        if (CheckForJudgeReporter(model, caseLawUnits, ref errorMessage) == false) return false;
        if (CheckForExistLawUnit(model, caseLawUnits, ref errorMessage) == false) return false;
        if (CheckForDismisal(model, ref errorMessage) == false) return false;

        model.CourtDepartmentId = model.CourtDepartmentId.EmptyToNull();
        model.CourtDutyId = model.CourtDutyId.EmptyToNull();

        model.SpecialityId = model.SpecialityId.EmptyToNull();
        if (model.SpecialityId != null)
        { if (model.SpecialityId < 0) { model.SpecialityId = null; } }
        model.CaseLawUnitDismisalId = model.CaseLawUnitDismisalId.EmptyToNull();

        CaseSelectionProtokol caseSelectionProtokol = null;
        if (model.Id > 0)
        {
          //не трябва да има редакция ама го оставям защото не се знае
          //Update
          caseSelectionProtokol = repo.GetById<CaseSelectionProtokol>(model.Id);
        }
        else
        {
          caseSelectionProtokol = new CaseSelectionProtokol();
          caseSelectionProtokol.CaseId = model.CaseId;
          caseSelectionProtokol.CourtId = model.CourtId;
          caseSelectionProtokol.SelectionDate = DateTime.Now;
        }

        caseSelectionProtokol.JudgeRoleId = model.JudgeRoleId;
        caseSelectionProtokol.SelectionModeId = model.SelectionModeId;
        if (model.SelectionModeId != NomenclatureConstants.SelectionMode.SelectByDuty)
        { caseSelectionProtokol.CourtDutyId = null; }

        else
        { caseSelectionProtokol.CourtDutyId = model.CourtDutyId; }
        caseSelectionProtokol.CourtDepartmentId = model.CourtDepartmentId;

        caseSelectionProtokol.SpecialityId = model.SpecialityId;
        caseSelectionProtokol.Description = model.Description;
        caseSelectionProtokol.CaseLawUnitDismisalId = model.CaseLawUnitDismisalId;
        caseSelectionProtokol.SelectionProtokolStateId = NomenclatureConstants.SelectionProtokolState.Generated;

        caseSelectionProtokol.LawUnits = new List<CaseSelectionProtokolLawUnit>();
        foreach (var lawUnit in model.LawUnits)
        {
          var lawUnitNew = new CaseSelectionProtokolLawUnit();
          lawUnitNew.CaseId = caseSelectionProtokol.CaseId;
          lawUnitNew.CourtId = userContext.CourtId;
          lawUnitNew.LawUnitId = lawUnit.LawUnitId;
          lawUnitNew.LoadIndex = lawUnit.LoadIndex;
          lawUnitNew.StateId = lawUnit.StateId;
          lawUnitNew.Description = lawUnit.Description;
          lawUnitNew.CaseCount = lawUnit.CaseCount;
          lawUnitNew.SelectedFromCaseGroup = lawUnit.SelectedFromCaseGroup;
          lawUnitNew.CaseGroupId = lawUnit.CaseGroupId;
          lawUnitNew.DateWrt = caseSelectionProtokol.SelectionDate;
          lawUnitNew.UserId = userContext.UserId;
          caseSelectionProtokol.LawUnits.Add(lawUnitNew);
        }

        caseSelectionProtokol.UserId = userContext.UserId;
        caseSelectionProtokol.DateWrt = DateTime.Now;

        if (model.Id > 0)
        {
          //не трябва да има редакция ама го оставям защото не се знае
          //Update
          repo.Update(caseSelectionProtokol);
          repo.SaveChanges();
        }
        else
        {
          if (!model.IsProtokolNoSelection)
          {
            SetSelectedLawUnit_SaveCaseLawUnit(caseSelectionProtokol, model);
            caseSelectionProtokol.LawUnits = SetTotalCourtCaseCount_LawUnit(caseSelectionProtokol.LawUnits, caseSelectionProtokol.CourtId, caseSelectionProtokol.JudgeRoleId, caseSelectionProtokol.SelectedLawUnitId);

          }
          //Insert
          repo.Add<CaseSelectionProtokol>(caseSelectionProtokol);
          repo.SaveChanges();
        }

        model.Id = caseSelectionProtokol.Id;
        return true;
      }
      catch (Exception ex)
      {
        logger.LogError(ex, $"Грешка при запис на CaseSelectionProtokol Id={ model.Id }");
        return false;
      }
    }
    /// <summary>
    /// Брой избрани /състав/ по дело
    /// </summary>
    /// <param name="caseId">ID на дело </param>
    /// <returns></returns>
    public int CaseSelectionProtokolLawUnit_SelectCount(int caseId)
    {
      return repo.AllReadonly<CaseSelectionProtokolLawUnit>()
          .Include(x => x.CaseSelectionProtokol)
          .Where(x => x.CaseSelectionProtokol.CaseId == caseId)
          .Count();
    }
    /// <summary>
    /// Списък на съдии от съд и група
    /// </summary>
    /// <param name="courtGroupId">Ид група на съд</param>
    /// <param name="caseId">ИД на дело </param>
    /// <param name="courtId">Ид на съд</param>
    /// <returns></returns>
    public IEnumerable<CaseSelectionProtokolLawUnitVM> LawUnit_LoadJudge(int courtGroupId, int caseId, int courtId, int judgeRoleId)
    {
      var today = DateTime.Now;
      var endDate = today.AddDays(1);
      var result = repo.AllReadonly<CourtLawUnitGroup>()
                              .Include(x => x.LawUnit)
                              .ThenInclude(x => x.Courts)
                              .Include(x => x.CourtGroup)

                              .Where(x => x.CourtGroupId == courtGroupId)
                              .Where(x => x.DateFrom <= today && (x.DateTo ?? endDate) >= today)

                               //За да не вхаща тези които не са назначени в момента
                               .Where(x => x.LawUnit.DateFrom <= today && (x.LawUnit.DateTo ?? endDate) >= today)
                              //За да не вхаща тези които не са назначени в момента
                              .Where(x => x.LawUnit.Courts.Where(c => c.CourtId == courtId && NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(c.PeriodTypeId) && c.DateFrom <= today && (c.DateTo ?? endDate) >= today).Any())

                              .Select(x => new CaseSelectionProtokolLawUnitVM
                              {
                                IsLoaded = true,
                                LoadIndex = x.LoadIndex,
                                LawUnitId = x.LawUnitId,
                                LawUnitFullName = x.LawUnit.FullName,
                                SelectedFromCaseGroup = false,
                                StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Include
                              }).ToArray();

      SetDataExcludeLawUnit(result, caseId, courtId, judgeRoleId);
      return LawUnit_SetIndex(result);
    }

    /// <summary>
    /// Зарежда списък на заседатели
    /// </summary>
    /// <param name="courtId">ID на съд</param>
    /// <param name="caseId">ID на дело</param>
    /// <param name="specialityId">ID на специализация на заседател</param>
    /// <returns></returns>
    public IEnumerable<CaseSelectionProtokolLawUnitVM> LawUnit_LoadJury(int courtId, int caseId, int? specialityId)
    {
      DateTime date = DateTime.Now;
      var endDate = date.AddDays(1);

      Expression<Func<CourtLawUnit, bool>> selectSpeciality = x => true;
      if (specialityId != null)
      {
        if (specialityId > 0)
          selectSpeciality = x => x.LawUnit.LawUnitSpeciality.Any(a => (a.SpecialityId == specialityId) && (a.DateTo ?? date.AddDays(1)).Date > date.Date);
      }
      var lawUnits = repo.AllReadonly<CourtLawUnit>()
                                   .Include(x => x.LawUnit).ThenInclude(x => x.LawUnitSpeciality)
                                   .Where(selectSpeciality)
                                   .Where(x => x.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Jury)
                                   .Where(x => x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Appoint)
                                   .Where(x => x.CourtId == courtId && x.DateExpired == null)
                                   .Where(x => x.DateFrom <= date && (x.DateTo ?? endDate) >= date)
                                      .Where(x => x.LawUnit.DateFrom <= date && (x.LawUnit.DateTo ?? endDate) >= date)
                                   .ToList();


      //участващи в делото
      var case_lawunits = repo.AllReadonly<CaseLawUnit>()
                      .Where(x => NomenclatureConstants.JudgeRole.JuriRolesList.Contains(x.JudgeRoleId) && x.CaseId == caseId)
                      .Where(x => (x.DateTo ?? date).Date >= date.Date) //За да изключва повторно разпределение по цъщото дело в един ден
                      .Where(x => x.CaseSessionId == null)
                   .Select(x => x.LawUnitId).ToArray();
      //участващи в свързани дела
      int[] conected_cases_lawunits = LawUnitsFromConectedCases(caseId);

      /// Надхвърлили лимита  от дни
      int[] juryOverLimit = JuryYearDays_Select(courtId, DateTime.Now.Year, null, null, null)
                                                  .Where(x => x.DaysCount > 60).Select(x => x.Id).ToArray();

      List<CourtLawUnit> finalLawUnits = lawUnits.Where(x => !case_lawunits.Contains(x.LawUnitId))
                                     .Where(x => !juryOverLimit.Contains(x.LawUnitId))
                                     .Where(x => !conected_cases_lawunits.Contains(x.LawUnitId)).ToList();

      var result = finalLawUnits.Select(x => new CaseSelectionProtokolLawUnitVM
      {
        IsLoaded = true,
        LoadIndex = 100,
        LawUnitId = x.LawUnitId,
        LawUnitFullName = x.LawUnit.FullName,
        SelectedFromCaseGroup = false,
        StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Include,
        EnableState = true
      }).ToArray();
      result = AddJuryCrossMeatings(caseId, courtId, result);
      return LawUnit_SetIndex(result);
    }
    /// <summary>
    /// Зарежда съдии по дежурство
    /// </summary>
    /// <param name="courtDutyId">ID на дежурство</param>
    /// <param name="caseId">ID на дело</param>
    /// <param name="courtId">ID на съд</param>
    /// <returns></returns>
    public IEnumerable<CaseSelectionProtokolLawUnitVM> LawUnit_LoadByCourtDutyId(int courtDutyId, int caseId, int courtId, int judgeRole)
    {
      var today = DateTime.Now;
      var endDate = today.AddDays(1);
      var result = repo.AllReadonly<CourtDutyLawUnit>()
                             .Include(x => x.LawUnit)
                             .Include(x => x.CourtDuty)
                             .Where(x => x.CourtDutyId == courtDutyId)
                             .Where(x => x.DateFrom <= today && (x.DateTo ?? endDate) >= today)
                             .Select(x => new CaseSelectionProtokolLawUnitVM
                             {
                               IsLoaded = true,
                               LoadIndex = 100,
                               LawUnitId = x.LawUnitId,
                               LawUnitFullName = x.LawUnit.FullName,
                               SelectedFromCaseGroup = false,
                               StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Include
                             }).ToArray();

      SetDataExcludeLawUnit(result, caseId, courtId, judgeRole);
      return LawUnit_SetIndex(result);
    }
    /// <summary>
    /// Добавя индекс към потребител
    /// </summary>
    /// <param name="model">Модел на потребители</param>
    /// <returns></returns>
    private IEnumerable<CaseSelectionProtokolLawUnitVM> LawUnit_SetIndex(CaseSelectionProtokolLawUnitVM[] model)
    {
      for (int i = 0; i < model.Length; i++)
      {
        model[i].Index = i;
      }

      return model;
    }
    /// <summary>
    /// Проверява за изключени от избор
    /// </summary>
    /// <param name="model">Модел на потребители</param>
    /// <param name="caseId">ID на дело</param>
    /// <param name="courtId">ID на съд</param>
    /// <returns></returns>
    private IEnumerable<CaseSelectionProtokolLawUnitVM> SetDataExcludeLawUnit(CaseSelectionProtokolLawUnitVM[] model, int caseId, int courtId, int judgeRoleId)
    {
      var caseLawUnitDismis = repo.AllReadonly<CaseLawUnitDismisal>()
                                        .Include(x => x.CaseLawUnit)
                                        .Where(CaseExtensions.ConfirmedDismissalsOnly())
                                        .Where(x => x.CaseLawUnit.CaseId == caseId).ToList();

      //Списък на използвани преразпределения
      List<int> caseProtocol = repo.AllReadonly<CaseSelectionProtokol>()
                                 .Where(x => x.CaseId == caseId && x.CaseLawUnitDismisalId != null)

                                 .Select(x => (x.CaseLawUnitDismisalId ?? 0)).ToList();
      //Списък на използвани преразпределения
      var caseLawUnit = repo.AllReadonly<CaseLawUnit>()
                                   .Where(x => x.CaseId == caseId)
                                   .Where(x => x.CaseSessionId == null).ToList();

      var today = DateTime.Now.Date;
      var endDate = today.AddDays(1).Date;
      var courtlawUnit = repo.AllReadonly<CourtLawUnit>()
                             .Include(x => x.LawUnit)
                               //.Where(x => x.CourtId == courtId &&
                               .Where(x => x.DateExpired == null && x.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Judge &&
                                x.DateFrom.Date <= today && (x.DateTo ?? endDate).Date >= today &&
                                ((x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Ill && x.CourtId == courtId)
                                || (x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Holiday && x.CourtId == courtId)
                                || (x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Move && x.CourtId != courtId))
                             ).ToList();

      int[] conectedCasesLawUnits = LawUnitsFromConectedCases(caseId);
      for (int i = 0; i < model.Length; i++)
      {
        model[i].EnableState = true;

        //Сетват се тези които са с незаето преразпределение
        var dismisal = caseLawUnitDismis.Where(x => x.CaseLawUnit.LawUnitId == model[i].LawUnitId)
                               .Where(x => x.CaseLawUnit.JudgeRoleId == judgeRoleId)
                               .Where(x => x.DismisalTypeId == NomenclatureConstants.DismisalType.Prerazpredelqne)
                               .Where(x => caseProtocol.Contains(x.Id) == false)
                               .FirstOrDefault();
        if (dismisal != null)
        {
          model[i].StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;
          model[i].Description = "Преразпределение:" + dismisal.Description;
          model[i].EnableState = false;
          continue;
        }
        //Сетват се тези които са с незаето преразпределение

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
        DateTime tmpdate = DateTime.Now;
        if (judgeRoleId != NomenclatureConstants.JudgeRole.JudgeReporter)

        ////// Когато се избира докладчик могат в състава могат да участвт и вече избрани членове
        {


          //Сетват се тези които вече са избрани
          if (caseLawUnit.Where(x => x.LawUnitId == model[i].LawUnitId)
                         .Where(x => (x.DateTo ?? tmpdate) >= tmpdate).Any())
          {
            model[i].StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;
            model[i].Description = "Вече е избран";
            model[i].EnableState = false;
            continue;
          }
        }

        //Сетват се тези които са болни
        var clu = courtlawUnit.Where(x => x.LawUnitId == model[i].LawUnitId && x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Ill).FirstOrDefault();

        if (clu != null)
        {
          model[i].StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;
          model[i].Description = "Болничен от: " + clu.DateFrom.Date.ToString("dd.MM.yyyy" + " г.");
          if (clu.DateTo != null)
          { model[i].Description = model[i].Description + " до: " + clu.DateTo.Value.ToString("dd.MM.yyyy" + " г."); }
          continue;
        }

        //Сетват се тези които са отпуска
        clu = courtlawUnit.Where(x => x.LawUnitId == model[i].LawUnitId && x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Holiday).FirstOrDefault();
        if (clu != null)
        {
          model[i].StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;
          model[i].Description = "Отпуск от: " + clu.DateFrom.Date.ToString("dd.MM.yyyy" + " г.");
          if (clu.DateTo != null)
          { model[i].Description = model[i].Description + " до: " + clu.DateTo.Value.ToString("dd.MM.yyyy" + " г."); }
          continue;
        }
        clu = courtlawUnit.Where(x => x.LawUnitId == model[i].LawUnitId && x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Holiday).FirstOrDefault();
        if (clu != null)
        {
          model[i].StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;
          model[i].Description = "Отпуск от: " + clu.DateFrom.Date.ToString("dd.MM.yyyy" + " г.");
          if (clu.DateTo != null)
          { model[i].Description = model[i].Description + " до: " + clu.DateTo.Value.ToString("dd.MM.yyyy" + " г."); }
          continue;
        }
        clu = courtlawUnit.Where(x => x.LawUnitId == model[i].LawUnitId && x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Move && x.CourtId != courtId).FirstOrDefault();
        if (clu != null)
        {
          model[i].StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;
          model[i].Description = "Командировка от: " + clu.DateFrom.Date.ToString("dd.MM.yyyy" + " г.");
          if (clu.DateTo != null)
          { model[i].Description = model[i].Description + " до: " + clu.DateTo.Value.ToString("dd.MM.yyyy" + " г."); }
          continue;
        }

        if (conectedCasesLawUnits.Contains(model[i].LawUnitId))
        {
          model[i].StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;
          model[i].Description = "Участие в свързано дело";
          continue;
        }

      }

      return model;
    }
    /// <summary>
    /// Преглед на протокол
    /// </summary>
    /// <param name="id">ID на протокол за избор</param>
    /// <returns></returns>
    public CaseSelectionProtokolPreviewVM CaseSelectionProtokol_Preview(int id)
    {
      var result = repo.All<CaseSelectionProtokol>()
           .Include(x => x.LawUnits)
           .ThenInclude(x => x.LawUnit)
           .Include(x => x.Case)
           .Include(x => x.Case.CaseType)
           .Include(x => x.Case.Court)
           .Include(x => x.Case.CaseCode)
           .Include(x => x.Case.CourtGroup)
           .Include(x => x.Case.Document)
           .Include(x => x.JudgeRole)
           .Include(x => x.SelectionMode)
           .Include(x => x.SelectedLawUnit)
           .Include(x => x.SelectedLawUnit.LawUnitType)
           .Include(x => x.CourtDuty)
           .Include(x => x.Case.LoadGroupLink)
           .Include(x => x.Case.LoadGroupLink.LoadGroup)
           .Include(x => x.User)
           .Include(x => x.User.LawUnit)
           .Where(x => x.Id == id)
           .Select(x => new CaseSelectionProtokolPreviewVM()
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

             LawUnits = x.LawUnits.Select(p => new CaseSelectionProtokolLawUnitPreviewVM
             {
               Id = p.LawUnitId,
               LawUnitFullName = p.LawUnit.FullName,
               LoadIndex = p.LoadIndex,
               StateId = p.StateId,
               CaseCount = p.CaseCount,
               CaseCourtTotalCount = p.CaseCourtCount,
               SelectedFromCaseGroup = p.SelectedFromCaseGroup,
               CaseGroupId = p.CaseGroupId,
               Description = p.Description

             }
            )
           }).FirstOrDefault();
      int selected = -1;
      if (result.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
      { selected = (result.SelectedLawUnitId ?? -1); }

      //result.LawUnits = SetTotalCourtCaseCount_LawUnit(result.LawUnits.ToList(), result.CourtId, selected);
      if ((result.DismisalId ?? 0) > 0)
      {
        try
        {
          var dismisal = repo.AllReadonly<CaseLawUnitDismisal>()
                             .Include(x => x.CaseLawUnit)
                             .ThenInclude(x => x.LawUnit)
                             .Include(x => x.DismisalType)
                             .Where(x => x.Id == result.DismisalId).FirstOrDefault();

          result.PrevSelectedLawUnitName = dismisal.CaseLawUnit.LawUnit.FullName;
          result.DismisalReason = dismisal.DismisalType.Label + "-" + dismisal.Description;

          if (dismisal.CaseLawUnit.CaseSelectionProtokolId.HasValue)
          {
            var prevProtocol = repo.AllReadonly<CaseSelectionProtokol>()
              .Include(x => x.SelectedLawUnit)
                     .Include(x => x.User.LawUnit)
              .Where(x => x.Id == dismisal.CaseLawUnit.CaseSelectionProtokolId.Value).FirstOrDefault();



            result.PrevUserName = prevProtocol.User.LawUnit.FullName;
          }



        }
        catch (Exception)
        {


        }



      }
      if (NomenclatureConstants.JudgeRole.JuriRolesList.Contains(result.JudgeRoleId ?? 0))
      {
        var _case = repo.GetById<Case>(result.CaseId);

        if (_case.JudicalCompositionId != null)
        {
          result.ComparentmentName = repo.GetById<CourtDepartment>(_case.JudicalCompositionId).Label.ToString();
        }
        var meeating = NextCaseOpenSessionMeeting(result.CaseId, (result.SelectionDateDateTime ?? DateTime.Now));
        if (meeating != null)
        {
          result.NextMeatingDate = meeating.DateFrom.ToString("dd.MM.yyyy HH:mm");
        }
        else
        {
          result.NextMeatingDate = "";
        }

      }
      return result;
    }
    /// <summary>
    /// Зарежда списък с протоколи 
    /// </summary>
    /// <param name="courtId">ID съд</param>
    /// <param name="model">Модел за филтър</param>
    /// <returns></returns>
    public IQueryable<CaseSelectionProtokolReportVM> CaseSelectionProtokol_SelectForReport(int courtId, CaseSelectionProtokolFilterVM model)
    {
      DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
      DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

      Expression<Func<CaseLawUnit, bool>> nameSearch = x => true;
      if (!string.IsNullOrEmpty(model.FullName))
        nameSearch = x => EF.Functions.ILike(x.LawUnit.FullName, model.FullName.ToPaternSearch());

      Expression<Func<CaseSelectionProtokol, bool>> nameSearchNotSigned = x => true;
      if (!string.IsNullOrEmpty(model.FullName))
        nameSearchNotSigned = x => EF.Functions.ILike(x.SelectedLawUnit.FullName, model.FullName.ToPaternSearch());

      Expression<Func<CaseLawUnit, bool>> modeSearch = x => true;
      if (model.SelectionModeId > 0)
      {
        if (model.SelectionModeId == 4)
        {
          modeSearch = x => x.CaseSelectionProtokol.SelectedLawUnitId != x.LawUnitId;
        }
        else
        {
          modeSearch = x => (x.CaseSelectionProtokol.SelectionModeId == model.SelectionModeId) && (x.CaseSelectionProtokol.SelectedLawUnitId == x.LawUnitId);
        }
      }

      Expression<Func<CaseSelectionProtokol, bool>> modeSearchNotSigned = x => true;
      if (model.SelectionModeId > 0)
      {
        modeSearchNotSigned = x => (x.SelectionModeId == model.SelectionModeId);
      }

      Func<CaseSelectionProtokol, bool> dateSearch = x => true;
      if (model.DateFrom != null || model.DateTo != null)
        dateSearch = x => x.SelectionDate.Date >= dateFromSearch.Date && x.SelectionDate.Date <= dateToSearch.Date;



      Func<Case, bool> caseRegnumberSearch = x => true;
      if (!string.IsNullOrEmpty(model.CaseRegNumber))
        caseRegnumberSearch = x => EF.Functions.ILike(x.RegNumber, model.CaseRegNumber.ToCasePaternSearch());

      Func<CaseSelectionProtokol, bool> roleSearch = x => true;
      if (model.JudgeRoleId > 0)
        roleSearch = x => x.JudgeRoleId == model.JudgeRoleId;




      Func<CaseSelectionProtokol, bool> userWhere = x => true;
      if (string.IsNullOrEmpty(model.UserId) == false && model.UserId != "0")
        userWhere = x => x.UserId == model.UserId;

      Func<CaseSelectionProtokol, bool> yearWhere = x => true;
      if ((model.Year ?? 0) > 0)
        yearWhere = x => x.SelectionDate.Year == model.Year;

      //Func<Case, bool> caseGroupWhere = x => true;
      //if (model.CaseGroupId > 0)
      //    caseGroupWhere = x => x.CaseGroupId == model.CaseGroupId;

      //Func<Case, bool> caseTypeWhere = x => true;
      //if (model.CaseTypeId > 0)
      //    caseTypeWhere = x => x.CaseTypeId == model.CaseTypeId;

      Func<Case, bool> caseGroupWhere = x => true;
      if (!string.IsNullOrEmpty(model.CaseGroupIds_text))
      {
        var listGroupIds = new List<int>();
        if (!string.IsNullOrEmpty(model.CaseGroupIds_text))
          listGroupIds = model.CaseGroupIds_text.Split(',').Select(Int32.Parse).ToList();
        caseGroupWhere = x => listGroupIds.Contains(x.CaseGroupId);
      }

      Func<Case, bool> caseTypeWhere = x => true;
      if (!string.IsNullOrEmpty(model.CaseTypeIds_text))
      {
        var listTypeIds = new List<int>();
        if (!string.IsNullOrEmpty(model.CaseTypeIds_text))
          listTypeIds = model.CaseTypeIds_text.Split(',').Select(Int32.Parse).ToList();
        caseTypeWhere = x => listTypeIds.Contains(x.CaseTypeId);
      }

      //Func<Case, bool> courtGroupWhere = x => true;
      //if (model.CourtGroupId > 0)
      //    caseTypeWhere = x => x.CourtGroupId == model.CourtGroupId;

      Func<Case, bool> courtGroupWhere = x => true;
      if (!string.IsNullOrEmpty(model.CourtGroupIds_text))
      {
        var listCourtGroupIds = new List<int>();
        if (!string.IsNullOrEmpty(model.CourtGroupIds_text))
          listCourtGroupIds = model.CourtGroupIds_text.Split(',').Select(Int32.Parse).ToList();
        caseTypeWhere = x => listCourtGroupIds.Contains(x.CourtGroupId ?? 0);
      }

      //Func<Case, bool> loadGroupLinkWhere = x => true;
      //if (model.LoadGroupLinkId > 0)
      //    caseTypeWhere = x => x.LoadGroupLinkId == model.LoadGroupLinkId;

      Func<Case, bool> loadGroupLinkWhere = x => true;
      if (!string.IsNullOrEmpty(model.LoadGroupLinkIds_text))
      {
        var listLoadGroupLinkIds = new List<int>();
        if (!string.IsNullOrEmpty(model.LoadGroupLinkIds_text))
          listLoadGroupLinkIds = model.LoadGroupLinkIds_text.Split(',').Select(Int32.Parse).ToList();
        caseTypeWhere = x => listLoadGroupLinkIds.Contains(x.LoadGroupLinkId ?? 0);
      }

      Func<Document, bool> documentSearch = x => true;
      if (!string.IsNullOrEmpty(model.DocumentNumber))
        documentSearch = x => x.DocumentNumber.ToLower().Contains(model.DocumentNumber.ToLower());

      if (model.ProtokolState == NomenclatureConstants.SelectionProtokolState.Signed)
      {
        return repo.AllReadonly<CaseLawUnit>()
                  .Include(x => x.CaseSelectionProtokol)
                  .ThenInclude(x => x.SelectedLawUnit)
                  .Include(x => x.CaseSelectionProtokol)
                  .ThenInclude(x => x.JudgeRole)
                  .Include(x => x.CaseSelectionProtokol)
                  .ThenInclude(x => x.SelectionMode)
                  .Include(x => x.CaseSelectionProtokol)
                  .ThenInclude(x => x.Case)
                  .ThenInclude(x => x.CaseState)
                  .Include(x => x.CaseSelectionProtokol)
                  .ThenInclude(x => x.Case)
                  .ThenInclude(x => x.Document)
                  .Include(x => x.CaseSelectionProtokol)
                  .ThenInclude(x => x.SelectionProtokolState)
                  .Include(x => x.CaseSelectionProtokol)
                  .ThenInclude(x => x.User)
                  .ThenInclude(x => x.LawUnit)
                  .Where(nameSearch)
                  .Where(modeSearch)
                  .Where(x => x.CaseSelectionProtokol.Case.CourtId == courtId)
                  .Where(x => dateSearch(x.CaseSelectionProtokol))
                  .Where(x => caseRegnumberSearch(x.CaseSelectionProtokol.Case))
                  .Where(x => roleSearch(x.CaseSelectionProtokol))
                  .Where(x => userWhere(x.CaseSelectionProtokol))
                  .Where(x => yearWhere(x.CaseSelectionProtokol))
                  .Where(x => caseGroupWhere(x.CaseSelectionProtokol.Case))
                  .Where(x => caseTypeWhere(x.CaseSelectionProtokol.Case))
                  .Where(x => documentSearch(x.CaseSelectionProtokol.Case.Document))
                  .Where(x => courtGroupWhere(x.CaseSelectionProtokol.Case))
                  .Where(x => loadGroupLinkWhere(x.CaseSelectionProtokol.Case))
                  .Where(x => x.CaseSessionId == null)
                  .Select(x => new CaseSelectionProtokolReportVM()
                  {
                    Id = x.CaseSelectionProtokol.Id,
                    CaseId = x.CaseSelectionProtokol.CaseId,
                    CaseNumber = x.CaseSelectionProtokol.Case.RegNumber,
                    CaseDate = x.CaseSelectionProtokol.Case.RegDate,
                    DocumentId = x.Case.DocumentId,
                    DocumentNumber = x.Case.Document.DocumentNumber,
                    DocumentNumberVal = x.Case.Document.DocumentNumberValue,
                    Uic = x.CaseSelectionProtokol.SelectedLawUnit.Uic,
                    FullName = x.LawUnit.FullName,
                    SelectionDate = x.CaseSelectionProtokol.SelectionDate,
                    JudgeRoleName = (x.CaseSelectionProtokol.SelectedLawUnitId == x.LawUnitId) ? x.CaseSelectionProtokol.JudgeRole.Label : x.CaseSelectionProtokol.JudgeRole.Label + " (избран като член от състав)",
                    SelectionModeName = (x.CaseSelectionProtokol.SelectedLawUnitId == x.LawUnitId) ? x.CaseSelectionProtokol.SelectionMode.Label : "Постоянен състав",
                    CaseStateLabel = x.CaseSelectionProtokol.Case.CaseState.Label,
                    SelectionProtokolStateName = x.CaseSelectionProtokol.SelectionProtokolState.Label,
                    UserName = x.CaseSelectionProtokol.User.LawUnit.FullName,
                    CaseTypeLabel = (x.CaseSelectionProtokol.Case.CaseType != null) ? x.CaseSelectionProtokol.Case.CaseType.Code : string.Empty,
                    CaseCodeLabel = (x.CaseSelectionProtokol.Case.CaseCode != null) ? x.CaseSelectionProtokol.Case.CaseCode.Code : string.Empty,
                  }).AsQueryable();
      }
      else
      {
        return repo.AllReadonly<CaseSelectionProtokol>()
                  .Include(x => x.SelectedLawUnit)
                  .Include(x => x.JudgeRole)
                  .Include(x => x.SelectionMode)
                  .Include(x => x.Case)
                  .Include(x => x.Case.CaseState)
                  .Include(x => x.Case.Document)
                  .Include(x => x.SelectionProtokolState)
                  .Include(x => x.User)
                  .Include(x => x.User.LawUnit)
                  .Where(nameSearchNotSigned)
                  .Where(modeSearchNotSigned)
                  .Where(x => x.Case.CourtId == courtId)
                  .Where(x => dateSearch(x))
                  .Where(x => caseRegnumberSearch(x.Case))
                  .Where(x => roleSearch(x))
                  .Where(x => userWhere(x))
                  .Where(x => yearWhere(x))
                  .Where(x => caseGroupWhere(x.Case))
                  .Where(x => caseTypeWhere(x.Case))
                  .Where(x => documentSearch(x.Case.Document))
                  .Where(x => courtGroupWhere(x.Case))
                  .Where(x => loadGroupLinkWhere(x.Case))
                  .Where(x => x.SelectionProtokolStateId != NomenclatureConstants.SelectionProtokolState.Signed)
                  .Select(x => new CaseSelectionProtokolReportVM()
                  {
                    Id = x.Id,
                    CaseId = x.CaseId,
                    CaseNumber = x.Case.RegNumber,
                    CaseDate = x.Case.RegDate,
                    DocumentId = x.Case.DocumentId,
                    DocumentNumber = x.Case.Document.DocumentNumber,
                    DocumentNumberVal = x.Case.Document.DocumentNumberValue,
                    Uic = x.SelectedLawUnit.Uic,
                    FullName = x.SelectedLawUnit.FullName,
                    SelectionDate = x.SelectionDate,
                    JudgeRoleName = x.JudgeRole.Label,
                    SelectionModeName = x.SelectionMode.Label,
                    CaseStateLabel = x.Case.CaseState.Label,
                    SelectionProtokolStateName = x.SelectionProtokolState.Label,
                    UserName = x.User.LawUnit.FullName,
                    CaseTypeLabel = (x.Case.CaseType != null) ? x.Case.CaseType.Code : string.Empty,
                    CaseCodeLabel = (x.Case.CaseCode != null) ? x.Case.CaseCode.Code : string.Empty,
                  }).AsQueryable();
      }
    }
    /// <summary>
    /// Зарежда съдии по група дела
    /// </summary>
    /// <param name="courtId">ID на съд</param>
    /// <param name="CaseGroups">ID група за разпределение</param>
    /// <param name="lawUnitsIds">списък на използвани групи</param>
    /// <param name="caseId">Id na </param>
    /// <returns></returns>
    public IEnumerable<CaseSelectionProtokolLawUnitVM> LawUnit_LoadJudgeByCaseGroup(int courtId, CheckListVM[] CaseGroups, string lawUnitsIds, int caseId, int judgeRoleId)
    {
      int[] existsIds = (lawUnitsIds ?? "0").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray(); ;

      int[] groups = CaseGroups.Where(x => x.Checked).Select(x => int.Parse(x.Value)).ToArray();
      var today = DateTime.Now;
      var endDate = today.AddDays(1);
      var result = repo.AllReadonly<CourtLawUnitGroup>()
                              .Include(x => x.LawUnit)
                              .ThenInclude(x => x.Courts)
                              .Include(x => x.CourtGroup)
                              //.Where(x => x.CourtGroup.CaseGroupId == caseGroupId && x.CourtId == courtId)
                              .Where(x => groups.Contains(x.CourtGroup.CaseGroupId) && x.CourtId == courtId)
                              .Where(x => x.DateFrom <= today && (x.DateTo ?? endDate) >= today &&
                                     existsIds.Contains(x.LawUnitId) == false)
                             .Where(x => x.LawUnit.Courts.Where(c => c.CourtId == courtId && NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(c.PeriodTypeId) && c.DateFrom <= today && (c.DateTo ?? endDate) >= today).Any())


                              .Select(x => new CaseSelectionProtokolLawUnitVM
                              {
                                IsLoaded = true,
                                LoadIndex = 100,
                                LawUnitId = x.LawUnitId,
                                LawUnitFullName = x.LawUnit.FullName,
                                SelectedFromCaseGroup = true,
                                CaseGroupId = x.CourtGroup.CaseGroupId,
                                StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Include
                              })
                              .GroupBy(x => x.LawUnitId)
                              .Select(grp => grp.FirstOrDefault()).ToArray();

      SetDataExcludeLawUnit(result, caseId, courtId, judgeRoleId);
      return LawUnit_SetIndex(result);
    }

    /// <summary>
    ///  //Списък на групите, в който има поне един съдия освен включените до момента ( string idStr разделени със запетая)

    /// </summary>
    /// <param name="courtId">ID на съд</param>
    /// <param name="idStr">Използвани групи</param>
    /// <returns></returns>
    public int[] CaseGroup_WithLawUnits(int courtId, string idStr)
    {
      string[] alreadyIncludedLawunits = (idStr ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
      int[] ialreadyIncludedLawunits = new int[alreadyIncludedLawunits.Count()];
      for (int i = 0; i < alreadyIncludedLawunits.Count(); i++)
      {
        ialreadyIncludedLawunits[i] = Int32.Parse(alreadyIncludedLawunits[i]);
      }

      var today = DateTime.Now;
      var endDate = today.AddDays(1);
      var validLawUnits = GetActual_CourtLawUnitsByDate(courtId, NomenclatureConstants.LawUnitTypes.Judge, DateTime.Now).Select(x => x.LawUnitId).ToArray();

      var result = repo.AllReadonly<CourtLawUnitGroup>()
                              .Include(x => x.LawUnit)
                              .Include(x => x.CourtGroup)
                              .Where(x => x.CourtId == courtId)
                              .Where(x => x.DateFrom <= today && (x.DateTo ?? endDate) >= today)
                              .Where(x => validLawUnits.Contains(x.LawUnitId))
                              .Where(x => !ialreadyIncludedLawunits.Contains(x.LawUnitId))
                              .Select(x => x.CourtGroup.CaseGroupId).Distinct().ToArray();

      return result;
    }
    /// <summary>
    /// Списък на групите, за избор от други направления по дело
    /// </summary>
    /// <param name="idStr">Списък с вече използзвани съдии</param>
    /// <param name="groups">Списък с вече използзвани групи</param>
    /// <param name="caseId">ID на дело </param>
    /// <returns></returns>
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Return_Available_CaseGroup_forAdditionalSelect(string idStr, string groups, int caseId)
    {
      string[] groupsExcludeArr = (groups ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
      int[] groupsWithLawUnits = CaseGroup_WithLawUnits(userContext.CourtId, idStr);
      string[] groupsIncludeArr = new String[groupsWithLawUnits.Count()];
      for (int i = 0; i < groupsWithLawUnits.Count(); i++)
      {
        groupsIncludeArr[i] = groupsWithLawUnits[i].ToString();
      }
      var caseGroupList = nomService.GetDropDownListDescription<CaseGroup>(false, false);

      var result = caseGroupList.Where(x => groupsExcludeArr.Contains(x.Value) == false)
                                             .Where(x => groupsIncludeArr.Contains(x.Value) == true)
                                            .ToList();
      return result;
    }

    /// <summary>
    /// Зарежда възможните празни позиции за избор от на съдии и заседатели
    /// </summary>
    /// <param name="caseId">ID на дело</param>
    /// <returns></returns>

    public List<SelectListItem> AvailableJudgeRolesForFelect_SelectForDropDownList(int caseId)
    {
      List<SelectListItem> result = null;
      var date = DateTime.Now;
      var law_units_counts = repo.AllReadonly<CaseLawUnitCount>().Include(x => x.JudgeRole).Where(x => x.CaseId == caseId).OrderBy(x => x.JudgeRole.OrderNumber).ToList();
      var case_law_units = repo.AllReadonly<CaseLawUnit>().Where(x => x.CaseId == caseId)
         .Where(x => x.CaseId == caseId && (x.CaseSessionId == null) && (x.DateFrom.Date <= date.Date && (x.DateTo ?? date.AddDays(1)) >= date)).ToList();
      Boolean hasJudgeReporter = false;
      foreach (var u_c in law_units_counts)
      {
        foreach (var lu in case_law_units)
        {
          if (u_c.JudgeRoleId == lu.JudgeRoleId)
          {
            u_c.PersonCount = u_c.PersonCount - 1;
            if (lu.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
            { hasJudgeReporter = true; }
          }
        }
      }
      //Ако има докладчик да не се предлага друг
      if (!hasJudgeReporter)
      {
        foreach (var item in law_units_counts)
        {
          if (item.JudgeRoleId != NomenclatureConstants.JudgeRole.JudgeReporter)
          { item.PersonCount = 0; }
        }

        //Ако има докладчик да не се предлага друг
      }
      result = (from l in law_units_counts

                where l.PersonCount > 0
                select new SelectListItem()
                {
                  Text = l.JudgeRole.Label,
                  Value = l.JudgeRoleId.ToString()

                }
                ).ToList();
      return result;
    }

    /// <summary>
    /// Избира и създава протокол за избор на съдебен заседател
    /// </summary>
    /// <param name="model">модел за протокол</param>
    /// <param name="errorMessage">съобщение за грешка</param>
    /// <returns></returns>
    public int Select_Create_AddJuryProtocol(CaseSelectionProtokolVM model, ref string errorMessage)
    {
      if ((model.SpecialityId ?? 0) <= 0)
        model.SpecialityId = null;
      var date = DateTime.Now;
      int result = -1;

      CaseSelectionProtokolVM tmp = new CaseSelectionProtokolVM();
      foreach (var item in model.LawUnits)
      {
        if (item.LawUnitFullName == null)
        {
          item.LawUnitFullName = repo.AllReadonly<LawUnit>().Where(x => x.Id == item.LawUnitId).FirstOrDefault().FullName;
        }
        if ((item.StateId == 1) || model.SelectionModeId == NomenclatureConstants.SelectionMode.ManualSelect)
        {
          tmp.LawUnits.Add(item);
        }

      }
      model.LawUnits = tmp.LawUnits;

      if (ChecкJury(model, ref errorMessage) == false) return result;

      var _case = repo.AllReadonly<Case>()
                            .Where(x => x.Id == model.CaseId).FirstOrDefault();

      int[] court_joury = null;
      if (model.SelectionModeId != NomenclatureConstants.SelectionMode.ManualSelect)
      {
        //court_joury = LawUnit_LoadJury(model.CourtId, model.CaseId, model.SpecialityId).Select(x => x.LawUnitId).ToArray();
        court_joury = model.LawUnits.Select(x => x.LawUnitId).ToArray();
      }
      else
      {
        court_joury = new int[1];
        court_joury[0] = model.LawUnits[0].LawUnitId;
      }
      if (court_joury.Length == 0)
      { result = -2; }
      else
      {
        try
        {
          Random rnd = new Random();
          int rndLawUnit = rnd.Next(0, court_joury.Count() - 1);

          CaseSelectionProtokol save = new CaseSelectionProtokol();
          save.CourtId = _case.CourtId;
          save.SelectionModeId = model.SelectionModeId;
          save.SelectionDate = date;
          save.SelectedLawUnitId = court_joury[rndLawUnit];
          save.CaseId = model.CaseId;
          save.JudgeRoleId = model.JudgeRoleId;
          save.DateWrt = date;
          save.UserId = userContext.UserId;
          save.SpecialityId = model.SpecialityId;
          save.Description = model.Description;

          save.LawUnits = new List<CaseSelectionProtokolLawUnit>();
          save.SelectionProtokolStateId = NomenclatureConstants.SelectionProtokolState.Generated;

          var lawUnitNew = new CaseSelectionProtokolLawUnit();
          lawUnitNew.CaseId = model.CaseId;
          lawUnitNew.CourtId = _case.CourtId;
          lawUnitNew.LawUnitId = court_joury[rndLawUnit];
          lawUnitNew.LoadIndex = 100;
          lawUnitNew.StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Include;
          lawUnitNew.CaseCount = 0;
          lawUnitNew.SelectedFromCaseGroup = true;
          if (model.SelectionModeId == NomenclatureConstants.SelectionMode.ManualSelect)
            lawUnitNew.Description = model.LawUnits[0].Description;
          save.LawUnits.Add(lawUnitNew);

          repo.Add<CaseSelectionProtokol>(save);
          repo.SaveChanges();
          result = save.Id;
        }

        catch (Exception ex)
        {
          logger.LogError(ex, $"Грешка при запис на  CaseSelectionProtokol CaseId={ model.CaseId }");

        }
      }

      return result;
    }

    //public IEnumerable<CaseSelectionProtokolLawUnitPreviewVM> GetComparentmentJudges(int case_id, int judge_id)
    //{
    //  IEnumerable<CaseSelectionProtokolLawUnitPreviewVM> result = null;
    //  var _case = repo.AllReadonly<Case>()
    //                        .Where(x => x.Id == case_id).FirstOrDefault();

    //  var compartment = repo.AllReadonly<Compartment>()
    //                                   .Where(x => x.LawUnitId == judge_id)
    //                                   .Where(x => x.CourtId == _case.CourtId)
    //                                    .Where(x => x.DateTo == null).FirstOrDefault();

    //  if (compartment != null)
    //  {
    //    var comparement_lu = repo.AllReadonly<CompartmentLawUnit>()
    //                             .Where(x => x.CompartmentId == compartment.Id).Select(x => x.LawUnitId).ToArray();

    //    //Съдиите от състава на избрания съдя
    //    result = repo.AllReadonly<CourtLawUnit>()
    //                                 .Where(x => comparement_lu.Contains(x.LawUnitId))
    //                                    .Select(x => new CaseSelectionProtokolLawUnitPreviewVM
    //                                    {
    //                                      Id = x.LawUnitId,
    //                                      LawUnitFullName = x.LawUnit.FullName,

    //                                    }).Distinct().ToList();
    //  }
    //  if (result != null)
    //  {
    //    var date = DateTime.Now;
    //    //добавени членове в делото
    //    int case_law_units_count = repo.AllReadonly<CaseLawUnit>()
    //                       .Where(x => x.CaseId == case_id)
    //                       .Where(x => x.CaseSessionId == null)
    //                       .Where(x => NomenclatureConstants.JudgeRole.JudgeRolesList.Contains(x.JudgeRoleId))
    //                       .Where(x => x.CaseId == case_id && (x.DateFrom.Date <= date.Date && (x.DateTo ?? date.AddDays(1)).Date >= date.Date))
    //                       .Count();

    //    //възможна бройка
    //    var availabe_judges_places = repo.AllReadonly<CaseLawUnitCount>()
    //                                 .Where(x => x.CaseId == case_id)
    //                                 .Where(x => NomenclatureConstants.JudgeRole.JudgeRolesList.Contains(x.JudgeRoleId))
    //                                 .Select(x => x.PersonCount)
    //                                 .DefaultIfEmpty(0)
    //                                 .Sum();
    //    if (result.Count() > availabe_judges_places - case_law_units_count)
    //    {
    //      result = null;
    //    }
    //  }

    // return result;
    // }
    /// <summary>
    /// Прехвърля съдии от състава при избор на състав
    /// </summary>
    /// <param name="model">Модел на протокол</param>
    /// <returns></returns>
    public bool CaseSelectionProtokol_UpdateBeforeDocForSign(CaseSelectionProtokolPreviewVM model)
    {
      bool result = false;
      try
      {
        CaseSelectionProtokol caseSelectionProtokol = null;
        caseSelectionProtokol = repo.All<CaseSelectionProtokol>().Where(x => x.Id == model.Id).FirstOrDefault();
        if (model.ComparementID > 0)
        {
          caseSelectionProtokol.IncludeCompartmentJudges = true;


          /////////////////////////////////////////////////////////////////////////////////////////////////////
          ///List<SelectListItem> comparentmentList= new List<SelectListItem>();
          var lawunits = repo.AllReadonly<CourtDepartmentLawUnit>()
                    .Include(x => x.CourtDepartment)
                    .Where(x => x.DateFrom <= DateTime.Now && ((x.DateTo != null) ? x.DateTo >= DateTime.Now : true))
                    .Where(x => x.CourtDepartment.Id == model.ComparementID).ToList();

          foreach (var lawunit in lawunits)
          {
            if (model.SelectedLawUnitId != lawunit.LawUnitId)
            {
              CaseSelectionProtokolCompartment comparement_lu = new CaseSelectionProtokolCompartment();
              comparement_lu.UserId = userContext.UserId;
              comparement_lu.DateWrt = DateTime.Now;
              comparement_lu.CaseSelectionProtokolId = model.Id;
              comparement_lu.LawUnitId = lawunit.LawUnitId;
              comparement_lu.CaseId = model.CaseId;
              comparement_lu.CourtId = userContext.CourtId;

              caseSelectionProtokol.CompartmentLawUnits.Add(comparement_lu); caseSelectionProtokol.CompartmentName = lawunit.CourtDepartment.Label;
            }
          }
          ////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        else
        {
          caseSelectionProtokol.IncludeCompartmentJudges = false;
          model.ComparementID = null;
        }

        caseSelectionProtokol.CompartmentID = model.ComparementID;
        caseSelectionProtokol.CourtDepartmentId = model.ComparementID;
        caseSelectionProtokol.SelectionProtokolStateId = NomenclatureConstants.SelectionProtokolState.CreatedDoc;
        repo.SaveChanges();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, $"Грешка при запис на CaseSelectionProtokol_UpdateBeforeDocForSign Id={ model.Id }");
        return false;
      }

      return result;
    }
    /// <summary>
    /// Прехвърля съдиите в делото при подпис на протокол
    /// </summary>
    /// <param name="id">ID на протокол</param>
    /// <returns></returns>
    public bool CaseSelectionProtokol_UpdateBeforeAfterSign(int id)
    {
      var model = repo.GetById<CaseSelectionProtokol>(id);
      model.SelectionProtokolStateId = NomenclatureConstants.SelectionProtokolState.Signed;
      repo.SaveChanges();
      if (model.SelectedLawUnitId != null)
      {
        CaseLowUnitsFroSelectionProtocom_insert(id);
        courtLawUnitService.CourtDepartmentUnitOrder_ActualizeForCase(model.CaseId);
        caseDeadlineService.DeadLineCompanyCaseByCaseId(model.CaseId);
        finishWorkTasks(model);
      }
      return true;
    }

    private void finishWorkTasks(CaseSelectionProtokol model)
    {
      int[] selectionTasks = { WorkTaskConstants.Types.SendFor_NewSelection };
      var caseTasks = repo.All<WorkTask>()
                                      .Where(x => x.SourceType == SourceTypeSelectVM.Case && x.SourceId == model.CaseId)
                                      .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                                      .ToList();
      foreach (var item in caseTasks)
      {
        worktaskService.CompleteTask(item);
      }
    }

    /// <summary>
    /// Прехвърля съдии от протокол в дело
    /// </summary>
    /// <param name="id">Id на протокол</param>
    /// <returns></returns>
    public bool CaseLowUnitsFroSelectionProtocom_insert(int id)
    {
      var judge_to_reporter_case_lawunit_list = new List<CaseLawUnit>();
      var selectionProtokol = repo.AllReadonly<CaseSelectionProtokol>()
                                  .Include(x => x.CompartmentLawUnits)
                                  .Where(x => x.Id == id).FirstOrDefault();
      var case_obj = repo.GetById<Case>(selectionProtokol.CaseId);
      //предцедател състава ако състав 
      var department_predsedatel_lawunit_id = 0;
      if (selectionProtokol.IncludeCompartmentJudges)
      {
        department_predsedatel_lawunit_id = repo.AllReadonly<CourtDepartmentLawUnit>()
                                           .Where(x => x.CourtDepartmentId == selectionProtokol.CourtDepartmentId)
                                           .Where(x => x.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel)
                                           .Where(x => (x.DateTo ?? DateTime.Now.AddDays(1)) > DateTime.Now)
                                           .Select(x => x.LawUnitId).FirstOrDefault();
      }
      bool case_has_active_predsedatel = false;
      // Ако в делото текущо има председател по състав и е активен в делото
      var current_lu = repo.AllReadonly<CaseLawUnit>()
                                     .Where(x => x.CaseId == case_obj.Id)
                                     .Where(x => (x.DateTo ?? DateTime.Now.AddDays(1)) > DateTime.Now)
                                     .Where(x => (x.CaseSessionId ?? 0) == 0)
                                     .Where(x => x.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel).FirstOrDefault();

      case_has_active_predsedatel = (current_lu != null);
      // Ако в делото текущо няма председател по състав  се избира друг активен  в делото  ако има такъв    
      if (current_lu == null)
      {
        current_lu = repo.AllReadonly<CaseLawUnit>()
                                   .Where(x => x.CaseId == case_obj.Id)
                                   .Where(x => (x.DateTo ?? DateTime.Now.AddDays(1)) > DateTime.Now)
                                   .Where(x => (x.CaseSessionId ?? 0) == 0).FirstOrDefault();
      };

      int all_law_units_court_department_id = 0;

      //Взема се състава на активните потребители - активен състав до момента
      if (current_lu != null)
      {
        all_law_units_court_department_id = (current_lu.CourtDepartmentId ?? 0);
      }

      //Ако няма активен състав се взема този  от протокола
      if (all_law_units_court_department_id == 0)
      {
        all_law_units_court_department_id = (selectionProtokol.CompartmentID ?? 0);
      }


      try
      {
        List<CaseLawUnit> addCaseLawUnits = new List<CaseLawUnit>();


        CaseLawUnit first_judge = new CaseLawUnit();
        first_judge.CourtId = selectionProtokol.CourtId;
        first_judge.CaseId = selectionProtokol.CaseId;
        first_judge.LawUnitId = selectionProtokol.SelectedLawUnitId ?? 0;
        first_judge.LawUnitUserId = commonService.Users_GetUserIdByLawunit(first_judge.LawUnitId);
        first_judge.JudgeRoleId = selectionProtokol.JudgeRoleId;

        first_judge.DateFrom = DateTime.Now;
        first_judge.DateWrt = DateTime.Now;
        first_judge.UserId = userContext.UserId;
        first_judge.RealCourtDepartmentId = selectionProtokol.CompartmentID.EmptyToNull();
        first_judge.CourtGroupId = case_obj.CourtGroupId;
        first_judge.CourtDutyId = selectionProtokol.CourtDutyId;
        first_judge.CaseSelectionProtokolId = selectionProtokol.Id;
        if (all_law_units_court_department_id > 0)
        { first_judge.CourtDepartmentId = all_law_units_court_department_id; }


        if (!case_has_active_predsedatel)  //Ako nqma aktiwen precedatel po delo
        {
          //Ako e съсъ състав  и този съдия е прецедателя на състава
          if (selectionProtokol.IncludeCompartmentJudges && first_judge.LawUnitId == department_predsedatel_lawunit_id)
          { first_judge.JudgeDepartmentRoleId = NomenclatureConstants.JudgeDepartmentRole.Predsedatel; }
          //Ako e без състав  и този съдия е докладчик
          if (!selectionProtokol.IncludeCompartmentJudges && first_judge.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
          { first_judge.JudgeDepartmentRoleId = NomenclatureConstants.JudgeDepartmentRole.Predsedatel; }

        }

        if ((first_judge.JudgeDepartmentRoleId ?? 0) < 1)
        { first_judge.JudgeDepartmentRoleId = NomenclatureConstants.JudgeDepartmentRole.Member; }

        //Ако e избран съдия докладчик съдия, който  разпределен като член в делото, то той се експаирва, като член и се сетва като докладичк.
        if (first_judge.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
        {
          var existing_judge = repo.AllReadonly<CaseLawUnit>()
                               .Where(x => x.CaseId == selectionProtokol.CaseId)
                               .Where(x => (x.CaseSessionId ?? 0) < 1)
                               .Where(x => NomenclatureConstants.JudgeRole.JudgeRolesList.Contains(x.JudgeRoleId))
                              .Where(x => x.LawUnitId == first_judge.LawUnitId)
                               .Where(x => x.DateTo == null).FirstOrDefault();
          if (existing_judge != null)
          {
            //existing_judge.DateTo = first_judge.DateFrom;
            //existing_judge.DateTransferedDW = first_judge.DateFrom;
            //repo.Update<CaseLawUnit>(existing_judge);
            //////////////////////////////////Прави се изкуствено  преразпределение и става докладчик

            CaseLawUnitDismisal model = new CaseLawUnitDismisal();

            try
            {


              {
                //Insert
                model.CaseLawUnitId = existing_judge.Id;
                model.DismisalTypeId = NomenclatureConstants.DismisalType.Prerazpredelqne;
                model.DismisalKindId = 1;
                model.CaseId = existing_judge.CaseId;
                model.CourtId = existing_judge.CourtId;
                model.DismisalDate = DateTime.Now;
                model.DateWrt = DateTime.Now;
                model.Description = "Избран за съдия - докладчик";
                model.UserId = userContext.UserId;
                repo.Add<CaseLawUnitDismisal>(model);

              }

              var lawUnit = repo.GetById<CaseLawUnit>(model.CaseLawUnitId);
              lawUnit.DateTo = model.DismisalDate;
              lawUnit.DateWrt = DateTime.Now;
              lawUnit.UserId = userContext.UserId;
              repo.Update(lawUnit);

              var caseLawUnits = repo.AllReadonly<CaseLawUnit>()
                                     .Include(x => x.CaseSession)
                                     .Where(x => x.LawUnitId == lawUnit.LawUnitId &&
                                                 x.CaseSessionId != null &&
                                                 x.CaseSession.DateFrom >= model.DismisalDate &&
                                                 x.CaseId == model.CaseId)
                                     .ToList() ?? new List<CaseLawUnit>();

              foreach (var caseLaw in caseLawUnits)
              {
                caseLaw.DateTo = model.DismisalDate;
                caseLaw.DateWrt = DateTime.Now;
                caseLaw.DateTransferedDW = caseLaw.DateWrt;
                caseLaw.UserId = userContext.UserId;
                repo.Update(caseLaw);
              }
              judge_to_reporter_case_lawunit_list = caseLawUnits;
              repo.SaveChanges();




            }
            catch (Exception ex)
            {
              logger.LogError(ex, $"Грешка при запис на отвод Id={ model.Id }");
              return false;
            }


            /////////////////////////////////////Прави се изкуствено  преразпределение и става докладчик

          }
        }
        //Ако e избран съдия докладчик съдия, който  разпределен като член в делото, то той се експаирва, като член и се сетва като докладичк.


        repo.Add<CaseLawUnit>(first_judge);

        if (selectionProtokol.IncludeCompartmentJudges == true && selectionProtokol.CompartmentLawUnits != null)
        {

          var caseLawUnitDismislUsers = repo.AllReadonly<CaseLawUnitDismisal>()
                                       .Include(x => x.CaseLawUnit)
                                       .Where(x => x.CaseLawUnit.CaseId == selectionProtokol.CaseId)
                                       .Where(CaseExtensions.ConfirmedDismissalsOnly())
                                       .Where(x => NomenclatureConstants.DismisalType.DismisalList.Contains(x.DismisalTypeId))
                                       .Select(x => x.CaseLawUnitId).ToArray();

          var case_lawUnits = repo.AllReadonly<CaseLawUnit>()
                                 .Where(x => x.CaseId == selectionProtokol.CaseId)
                                 .Where(x => (x.CaseSessionId ?? 0) < 1)
                                 .Where(x => NomenclatureConstants.JudgeRole.JudgeRolesList.Contains(x.JudgeRoleId))
                                 .Where(x => x.DateTo == null)

                                 .Select(x => x.LawUnitId).ToArray();


          foreach (var item in selectionProtokol.CompartmentLawUnits)
          {
            if ((!case_lawUnits.Contains(item.LawUnitId)) && (!caseLawUnitDismislUsers.Contains(item.LawUnitId)))
            {
              CaseLawUnit second_judge = new CaseLawUnit();
              second_judge.CourtId = selectionProtokol.CourtId;
              second_judge.CaseId = selectionProtokol.CaseId;
              second_judge.LawUnitId = item.LawUnitId;
              second_judge.LawUnitUserId = commonService.Users_GetUserIdByLawunit(second_judge.LawUnitId);
              second_judge.JudgeRoleId = NomenclatureConstants.JudgeRole.Judge;

              second_judge.DateFrom = DateTime.Now;
              second_judge.DateWrt = DateTime.Now;
              second_judge.UserId = userContext.UserId;
              second_judge.RealCourtDepartmentId = selectionProtokol.CompartmentID.EmptyToNull();
              second_judge.CourtGroupId = case_obj.CourtGroupId;
              second_judge.CourtDutyId = selectionProtokol.CourtDutyId;
              second_judge.CaseSelectionProtokolId = selectionProtokol.Id;
              if (all_law_units_court_department_id > 0)
              { second_judge.CourtDepartmentId = all_law_units_court_department_id; }
              if (!case_has_active_predsedatel)  //Ako nqma aktiwen precedatel po delo
              {
                //Ako e съсъ състав  и този съдия е прецедателя на състава
                if (selectionProtokol.IncludeCompartmentJudges && second_judge.LawUnitId == department_predsedatel_lawunit_id)
                { second_judge.JudgeDepartmentRoleId = NomenclatureConstants.JudgeDepartmentRole.Predsedatel; }


              }
              if ((second_judge.JudgeDepartmentRoleId ?? 0) < 1)
              { second_judge.JudgeDepartmentRoleId = NomenclatureConstants.JudgeDepartmentRole.Member; }
              addCaseLawUnits.Add(second_judge);
              //repo.Add<CaseLawUnit>(second_judge);
            }
          }
        }
        //ЗА да се трансферира нов съдия в DW
        case_obj.DateWrt = DateTime.Now;
        repo.AddRange(addCaseLawUnits);
        repo.SaveChanges();

        ///////////////////////////////////////////Смяна на състава на всички бъдещи заседания когато член е станал докладчик
        if (judge_to_reporter_case_lawunit_list.Count > 0)
        {
          foreach (var item in judge_to_reporter_case_lawunit_list)
          {
            CaseLawUnit new_case_law_unit = new CaseLawUnit();
            new_case_law_unit.CaseId = item.CaseId;
            new_case_law_unit.LawUnitId = first_judge.LawUnitId;
            new_case_law_unit.JudgeRoleId = first_judge.JudgeRoleId;
            new_case_law_unit.JudgeDepartmentRoleId = first_judge.JudgeDepartmentRoleId;
            new_case_law_unit.DateFrom = first_judge.DateFrom;
            new_case_law_unit.CaseSessionId = item.CaseSessionId;
            new_case_law_unit.DateWrt = first_judge.DateWrt;
            new_case_law_unit.UserId = first_judge.UserId;
            new_case_law_unit.CourtDepartmentId = first_judge.CourtDepartmentId;
            new_case_law_unit.CourtDutyId = first_judge.CourtDutyId;
            new_case_law_unit.CourtDutyId = first_judge.CourtDutyId;
            new_case_law_unit.CourtId = first_judge.CourtId;
            new_case_law_unit.LawUnitUserId = first_judge.LawUnitUserId;
            new_case_law_unit.RealCourtDepartmentId = first_judge.RealCourtDepartmentId;
            new_case_law_unit.LawUnitSubstitutionId = item.LawUnitSubstitutionId;
            new_case_law_unit.CaseSelectionProtokolId = first_judge.CaseSelectionProtokolId;
            repo.Add(new_case_law_unit);


          }
          repo.SaveChanges();
        }
        //////////////////////////////////////////

        if (first_judge.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
        {
          mqEpepService.AppendJudgeReporter(first_judge.Id, EpepConstants.ServiceMethod.Add);

        }
        mqEpepService.AppendCaseSelectionProtocol(selectionProtokol, EpepConstants.ServiceMethod.Add);
        if (selectionProtokol.CaseLawUnitDismisalId > 0)
        {
          mqEpepService.EPRO_AppendReplace(selectionProtokol.Id, selectionProtokol.CaseLawUnitDismisalId.Value);
        }

        addCaseLawUnits.Add(first_judge);

        //Заявка 1
        if (userContext.CourtTypeId != NomenclatureConstants.CourtType.VKS)
        {
          foreach (var lawUnit in addCaseLawUnits)
          {
            var notification = notificationService.NewWorkNotification(lawUnit);
            if (notification != null)
            {
              notificationService.SaveWorkNotification(notification);
            }
          }
        }
        commonService.UpdateCaseJudicalCompositionOtdelenie(selectionProtokol.CaseId);
      }

      catch (Exception ex)
      {
        logger.LogError(ex, $"Грешка при запис на CaseSelectionProtokol_UpdateBeforeDocForSign Id={ selectionProtokol.CaseId}");
        return false;
      }

      return true;
    }
    /// <summary>
    /// Проверка за неподписан протокол за разпределение по дело
    /// </summary>
    /// <param name="id">ID на дело</param>
    /// <returns></returns>
    public bool HsaUnsignedProtocol(int id)
    {
      bool result = true;

      var unsigned_prortocols = repo.AllReadonly<CaseSelectionProtokol>().Where(x => x.CaseId == id)
                                                                        .Where(x => x.SelectionProtokolStateId != NomenclatureConstants.SelectionProtokolState.Signed).ToList();
      if (unsigned_prortocols.Count == 0)
      { result = false; }

      return result;
    }
    public List<SelectListItem> GetJudgeComprentmetList(int lawunitId, int courtId, int case_id)
    {
      List<SelectListItem> comparentmentList = new List<SelectListItem>();
      var case_instance = repo.AllReadonly<Case>()
                        .Where(x => x.Id == case_id)
                        .Select(x => x.CaseType.CaseInstanceId)
                        .FirstOrDefault();

      var comparentments = repo.AllReadonly<CourtDepartmentLawUnit>()
                .Include(x => x.CourtDepartment)
                .ThenInclude(x => x.ParentDepartment)
                .Where(x => x.LawUnitId == lawunitId)
                .Where(x => x.CourtDepartment.DepartmentTypeId == NomenclatureConstants.DepartmentType.Systav)
                .Where(x => x.DateTo == null)
                .Where(x => (x.CourtDepartment.CaseInstanceId ?? case_instance) == case_instance)

                      .Where(x => x.CourtDepartment.CourtId == courtId)

                      .ToList();

      //възможна бройка
      int availabe_judges_places = repo.AllReadonly<CaseLawUnitCount>()
                                   .Where(x => x.CaseId == case_id)
                                    // .Where(x => NomenclatureConstants.JudgeRole.JudgeRolesList.Contains(x.JudgeRoleId))
                                    .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter || x.JudgeRoleId == NomenclatureConstants.JudgeRole.Judge)
                                   .Select(x => x.PersonCount).Sum();
      //заети места
      int ocupated_judges_places = repo.AllReadonly<CaseLawUnit>()
                                 .Where(x => x.CaseId == case_id)
                                 .Where(x => (x.CaseSessionId ?? 0) < 1)
                                 .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter || x.JudgeRoleId == NomenclatureConstants.JudgeRole.Judge).Where(x => x.DateTo == null)
                                 .Select(x => x.LawUnitId).Sum();
      availabe_judges_places = availabe_judges_places - ocupated_judges_places;


      foreach (var comparement in comparentments)
      {
        int comparentment_count = 0;
        var lawunits = repo.AllReadonly<CourtDepartmentLawUnit>()
          .Include(x => x.LawUnit)
          .Where(x => x.CourtDepartmentId == comparement.CourtDepartment.Id)
          .Where(x => x.DateFrom <= DateTime.Now && ((x.DateTo != null) ? x.DateTo >= DateTime.Now : true)).ToList();

        string sCourtJudges = comparement.CourtDepartment.Label;
        if (comparement.CourtDepartment.ParentDepartment != null)
        {
          sCourtJudges += $" ({comparement.CourtDepartment.ParentDepartment.Label})";
        }

        sCourtJudges += ": ";
        foreach (var lawunit in lawunits)
        {
          sCourtJudges = sCourtJudges + lawunit.LawUnit.FullName + "; ";
          comparentment_count = comparentment_count + 1;
        }

        SelectListItem ddl_element = new SelectListItem();
        ddl_element.Text = sCourtJudges;
        ddl_element.Value = comparement.CourtDepartment.Id.ToString();
        if (comparentment_count <= availabe_judges_places)
        {
          comparentmentList.Add(ddl_element);
        }

      }
      SelectListItem ddl_empty_element = new SelectListItem();
      ddl_empty_element.Text = "Без постоянен съдебен състав.";
      ddl_empty_element.Value = "-1";
      comparentmentList.Add(ddl_empty_element);
      return comparentmentList;
    }
    /// <summary>
    /// Изчислява броя дела на всеки съдия от протокола
    /// </summary>
    /// <param name="lawUnits">Списък от съдии</param>
    /// <param name="courtId">ID на съд</param>
    /// <returns></returns>
    private List<CaseSelectionProtokolLawUnit> SetTotalCourtCaseCount_LawUnit(ICollection<CaseSelectionProtokolLawUnit> lawUnits, int courtId, int? judge_role_id, int? selectedLawUnit = -1)
    {
      List<CaseSelectionProtokolLawUnit> result = new List<CaseSelectionProtokolLawUnit>();


      foreach (var item in lawUnits)
      {
        DateTime dt = DateTime.Now;

        var countCC = repo.AllReadonly<CourtLoadPeriodLawUnit>()
                             .Include(x => x.CourtLoadPeriod)


                          //За да отчете период за нулиране на натоварване
                          .Where(x => (x.CourtLoadPeriod.CourtLoadResetPeriod.DateTo ?? dt) >= dt && x.CourtLoadPeriod.CourtLoadResetPeriod.DateFrom < dt)
                          .Where(x => x.CourtLoadPeriod.CourtLoadResetPeriod.CourtId == courtId)
                          .Where(x => x.LawUnitId == item.LawUnitId)
                             .Select(x => x.DayCases).Sum();


        if (item.LawUnitId == selectedLawUnit && countCC > 0 && judge_role_id == NomenclatureConstants.JudgeRole.JudgeReporter)

        {
          countCC = countCC - 1;
        }
        item.CaseCourtCount = (int)countCC;
        result.Add(item);
      }

      return result;
    }


    /// <summary>
    /// Връщат се всички  съдии и заседатели по свързани дела
    /// </summary>
    /// <param name="caseId">ID на Дело</param>
    /// <returns></returns>
    public int[] LawUnitsFromConectedCases(int caseId)

    {
      List<int> ConectedLawunits = new List<int>();
      CaseMigration case_migration = repo.AllReadonly<CaseMigration>()
                             .Where(x => x.CaseId == caseId).FirstOrDefault();

      if (case_migration != null)
      {
        int initialCaseId = case_migration.InitialCaseId;

        int[] conectedCasesid = repo.AllReadonly<CaseMigration>()
                                    .Where(x => x.InitialCaseId == initialCaseId)
                                    .Where(x => x.CaseId != caseId).Select(x => x.CaseId).ToArray();

        ConectedLawunits = repo.AllReadonly<CaseLawUnit>()
                                                 .Where(x => conectedCasesid.Contains(x.CaseId))
                                                 .Select(x => x.LawUnitId)
                                                 .Distinct().ToList();
      }

      return ConectedLawunits.ToArray();
    }
    /// <summary>
    /// Изчислява отработени дни на заседатели
    /// </summary>
    /// <param name="courtId">ID на съд </param>
    /// <param name="year">година</param>
    /// <param name="name">Име</param>
    /// <param name="DateFrom">От дата </param>
    /// <param name="DateTo">До дата</param>
    /// <returns></returns>
    public IQueryable<JuryYearDays> JuryYearDays_Select(int courtId, int year, DateTime? DateFrom, DateTime? DateTo, int? LawUnitId)
    {
      DateTime first_date_of_year = (DateFrom ?? (new DateTime(year, 1, 1))).ForceStartDate();
      DateTime last_date_of_year = (DateTo ?? new DateTime(year, 12, 31)).ForceEndDate();

      //var court_year_jury = repo.AllReadonly<CourtLawUnit>()
      //                          .Include(x => x.LawUnit)
      //                          .Where(x => x.CourtId == courtId && x.DateExpired == null)
      //                          .Where(x => x.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Jury)
      //                          .Where(x => ((first_date_of_year <= (x.DateTo ?? first_date_of_year)) && x.DateFrom <= last_date_of_year))
      //                          .ToList();

      var case_sessions = repo.AllReadonly<CaseLawUnit>()
                              .Include(x => x.CaseSession)
                              .Where(x => x.CourtId == courtId)
                              .Where(x => x.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Jury)
                              //.Where(x => (x.DateTo ?? x.CaseSession.DateFrom) >= x.CaseSession.DateFrom)
                              .Where(x => x.CaseSession.DateExpired == null)
                              .Where(x => x.CaseSessionId != null)
                              .Where(x => (first_date_of_year <= x.CaseSession.DateFrom) && (x.CaseSession.DateFrom <= last_date_of_year))
                              .Select(x => new JuryYearDays()
                              {
                                Id = x.LawUnitId,
                                DateFrom = x.CaseSession.DateFrom.Date,
                                SessionStateID = x.CaseSession.SessionStateId
                              }).ToList();

      //var lawunits = case_sessions.Select(x => x.Id).Distinct().ToList();

      Expression<Func<CourtLawUnit, bool>> selectLawUnit = x => true;
      if (LawUnitId != null)
      {
        if (LawUnitId > 0)
          selectLawUnit = x => x.LawUnitId == LawUnitId;
      }

      return repo.AllReadonly<CourtLawUnit>()
                 .Where(selectLawUnit)
                 .Where(x => x.CourtId == courtId && x.DateExpired == null)
                 .Where(x => x.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Jury)
                 //.Where(x => lawunits.Contains(x.LawUnitId))
                 .Where(x => ((first_date_of_year <= (x.DateTo ?? first_date_of_year)) && x.DateFrom <= last_date_of_year))
                 .Select(x => new JuryYearDays()
                 {
                   Id = x.LawUnitId,
                   DateFrom = x.DateFrom,
                   DateTo = x.DateTo,
                   FullName = x.LawUnit.FullName,
                   DaysCount = case_sessions.Where(a => a.Id == x.LawUnitId).Where(a => a.SessionStateID == NomenclatureConstants.SessionState.Provedeno).Select(a => a.DateFrom).Distinct().Count(),
                   DaysCountAppointed = case_sessions.Where(a => a.Id == x.LawUnitId).Where(a => a.SessionStateID == NomenclatureConstants.SessionState.Nasrocheno).Select(a => a.DateFrom).Distinct().Count()

                 }).AsQueryable();
    }

    //Взема  номер за група/дежурство
    public int TakeCaseSelectionProtocolLockNumber(int? courtGroupid, int? courtDutyId)

    {
      int result = 0;
      CaseSelectionProtokolLock lock_row = new CaseSelectionProtokolLock();
      lock_row.CourtGroupId = courtGroupid;
      lock_row.CourtDutyId = courtDutyId;
      lock_row.Date = DateTime.Now;
      repo.Add<CaseSelectionProtokolLock>(lock_row);

      repo.SaveChanges();

      result = lock_row.Id;

      return result;
    }


    /// <summary>
    /// //Взема минималния номер за група/дежурство за за заключване докато се обработва
    /// </summary>
    /// <param name="courtGroupid">ID на група</param>
    /// <param name="courtDutyId">ID  на дежурство</param>
    /// <returns></returns>

    public int TakeCaseSelectionProtocolMinLockNumber(int? courtGroupid, int? courtDutyId)

    {
      int result = 0;
      var forDelete = repo.All<CaseSelectionProtokolLock>()
       .Where(x => (x.DateFinish ?? DateTime.Now) < DateTime.Now.AddHours(-2));
      if (forDelete.Any())
      {
        repo.DeleteRange(forDelete);
        repo.SaveChanges();
      }



      Expression<Func<CaseSelectionProtokolLock, bool>> courtGroupDutySearch = x => true;
      if (courtGroupid != null)
      {
        if (courtGroupid > 0)
        {
          courtGroupDutySearch = x => x.CourtGroupId == courtGroupid;
        }
      }
      if (courtDutyId != null)
      {
        if (courtDutyId > 0)
        {
          courtGroupDutySearch = x => x.CourtDutyId == courtDutyId;
        }
      }



      List<CaseSelectionProtokolLock> lock_rows = repo.AllReadonly<CaseSelectionProtokolLock>()
                                           .Where(courtGroupDutySearch)
                                           .Where(x => x.DateFinish == null).ToList();

      //Ако има редове по-стари от 110sec  ги нулира
      foreach (var row in lock_rows)
      {
        if (row.Date < DateTime.Now.AddSeconds(-80))
        { FinishLockNumber(row.Id); }
      }



      if (lock_rows != null)
      {
        result = lock_rows.Select(x => x.Id).Min();
      }

      return result;
    }

    /// <summary>
    /// Приключва номер  за заключване
    /// </summary>
    /// <param name="lockId">номер</param>
    /// <returns></returns>
    public bool FinishLockNumber(int lockId)
    {
      bool result = false;
      try
      {
        var lock_row = repo.GetById<CaseSelectionProtokolLock>(lockId);

        lock_row.DateFinish = DateTime.Now;
        repo.Update<CaseSelectionProtokolLock>(lock_row);
        repo.SaveChanges();
      }
      catch (Exception ex)
      {

        throw;
      }
      return result;
    }
    /// <summary>
    /// Извлича активният състав по дело
    /// </summary>
    /// <param name="caseId">ID на дело</param>
    /// <returns></returns>
    public int[] GetActiveLawUnits(int caseId)
    {
      var result = repo.AllReadonly<CaseLawUnit>()
                               .Where(x => (x.DateTo ?? DateTime.Now.AddDays(1)).Date > DateTime.Now.Date)
                               .Where(x => x.CaseId == caseId)
                               .Where(x => (x.CaseSessionId ?? 0) < 1)
                         .Select(x => x.LawUnitId).ToArray();
      return result;
    }


    public AssignmentRequestModel GetAssignmentRequestModel(int protocolId)
    {
      AssignmentRequestModel request = new AssignmentRequestModel();

      var protcol = repo.AllReadonly<CaseSelectionProtokol>()
        .Include(x => x.SelectedLawUnit)

        .Where(x => x.Id == protocolId)
        .FirstOrDefault();

      Case current_case = repo.AllReadonly<Case>()
        .Include(x => x.Court)
        .Include(x => x.CaseCharacter)
        .Include(x => x.CaseClassifications)
        .Where(x => x.Id == protcol.CaseId)
        .FirstOrDefault();
      request.Judge_ID = (protcol.SelectedLawUnitId ?? 0);
      request.TypeOfAssignment = protcol.SelectionModeId;
      request.Case_ID = current_case.Id;

      request.CourtNumber = Int32.Parse(repo.AllReadonly<CodeMapping>()
                           .Where(x => x.Alias == "csrd_court")
                           .Where(x => x.InnerCode == current_case.Court.Code)
                          .FirstOrDefault().OuterCode.ToString());


      request.CaseYear = current_case.RegDate.Year;
      string mappingCaseCode = current_case.CaseCharacter.Code;
      if (current_case.CaseClassifications.Where(x => x.ClassificationId == NomenclatureConstants.CaseClassifications.Secret)
                                          .Where(x => (x.CaseSessionId ?? 0) < 1).Any())
      { mappingCaseCode = mappingCaseCode + "secret"; }



      request.CaseCode = Int32.Parse(
        repo.AllReadonly<CodeMapping>()
                           .Where(x => x.Alias == "csrd_character")
                           .Where(x => x.InnerCode == mappingCaseCode)
                           .FirstOrDefault().OuterCode.ToString());


      request.CaseFormationDate = current_case.RegDate;
      request.AssignmentDate = protcol.SelectionDate;
      request.Name = protcol.SelectedLawUnit.FirstName;
      request.Family = protcol.SelectedLawUnit.FamilyName;
      if (protcol.SelectedLawUnit.Family2Name != null)
      { request.Family = request.Family + "-" + protcol.SelectedLawUnit.Family2Name; }

      request.CaseNumber = current_case.ShortNumber;
      request.Protocol = cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSelectionProtokol, SourceId = protocolId.ToString() }).Result.GetBytes();


      return request;
    }

    public IQueryable<CaseSelectionProtokolLawUnitPreviewVM> LawUnitReportByGroup(int courtId, int courtGropId, int? LawUnitId)
    {


      DateTime dateNow = DateTime.Now;


      Expression<Func<CourtLoadPeriodLawUnit, bool>> searchLawUnitId = x => true;
      Expression<Func<CourtLawUnitGroup, bool>> searchLawUnitCG = x => true;
      if ((LawUnitId ?? 0) > 0)
      {
        searchLawUnitId = x => x.LawUnit.Id == LawUnitId;
        searchLawUnitCG = x => x.LawUnit.Id == LawUnitId;
      }

      var result = repo.AllReadonly<CourtLawUnitGroup>()
                          .Include(x => x.LawUnit)
                          .Where(x => x.CourtGroupId == courtGropId)
                          .Where(x => x.DateFrom <= dateNow && (x.DateTo ?? DateTime.MaxValue) > dateNow)
                          .Where(searchLawUnitCG)
                          .Select(x => new CaseSelectionProtokolLawUnitPreviewVM
                          {
                            LawUnitId = x.LawUnitId,
                            LawUnitFullName = x.LawUnit.FullName,
                            FromDateInGROUP = x.DateFrom,
                            LoadIndex = x.LoadIndex
                          }).OrderBy(x => x.LawUnitFullName).ToList();



      var groupCounts = repo.AllReadonly<CourtLoadPeriodLawUnit>()
        .Include(x => x.LawUnit)

        .Include(x => x.CourtLoadPeriod)
        //.ThenInclude(x => x.CourtGroup)


        .Where(x => x.CourtLoadPeriod.CourtLoadResetPeriod.CourtId == courtId)
        .Where(x => x.CourtLoadPeriod.CourtGroupId == courtGropId)
        .Where(x => (x.CourtLoadPeriod.CourtLoadResetPeriod.DateTo ?? dateNow) >= dateNow)
        .Where(x => (x.CourtLoadPeriod.CourtLoadResetPeriod.DateFrom) <= dateNow)
        .Where(x => (x.LawUnitId ?? 0) > 0)
        .Where(searchLawUnitId)
        .GroupBy(x => new
        {
          x.LawUnitId,
          x.LawUnit.FullName
          //,x.CourtLoadPeriod.CourtGroupId
          //,
          //  dateFrom = x.CourtLoadPeriod.CourtGroup.CourtLawUnitGroups.Where(a => a.LawUnitId == x.LawUnitId).
          //OrderByDescending(a => a.DateFrom.Date).Select(a => a.DateFrom.Date).FirstOrDefault(),
          //  loadIndex = x.CourtLoadPeriod.CourtGroup.CourtLawUnitGroups.Where(a => a.LawUnitId == x.LawUnitId).
          //OrderByDescending(a => a.DateFrom.Date).Select(a => a.LoadIndex).FirstOrDefault(),

        })
                                .Select(g => new CaseSelectionProtokolLawUnitPreviewVM
                                {
                                  LawUnitId = g.Key.LawUnitId ?? 0,
                                  LawUnitFullName = (g.Key.FullName == null ? "Общо" : g.Key.FullName),
                                  //CaseCount = g.Sum(x => (g.Key.LawUnitId != null) ? x.DayCases : x.DayCases)),
                                  CaseCount = g.Sum(x => (int)Math.Round(x.DayCases)),
                                  CaseCourtTotalCount = 1
                                  //,
                                  //FromDateInGROUP = g.Key.dateFrom,
                                  //LoadIndex = g.Key.loadIndex


                                }).ToList();

      foreach (var item in result)
      {
        var cnt = groupCounts.FirstOrDefault(x => x.LawUnitId == item.LawUnitId);
        if (cnt != null)
        {
          item.CaseCount = cnt.CaseCount;
          item.CaseCourtTotalCount = cnt.CaseCourtTotalCount;
        }
      }
      return result.AsQueryable();

    }

    public List<SelectListItem> GetCourtGroups(int courtId)
    {

      List<SelectListItem> groupList = repo.AllReadonly<CourtGroup>()
                               .Where(x => x.CourtId == courtId)

                                .Select(x => new SelectListItem()
                                {
                                  Text = x.Label,
                                  Value = x.Id.ToString()
                                }).OrderBy(x => x.Text).ToList();

      return groupList;
    }
    public bool HasJudgeChoiceWithoutDismisal(int courtId, int caseID)
    {
      bool result = true;
      DateTime date = DateTime.Now;
      DateTime endDate = date.AddDays(1);
      var lawUnits = repo.AllReadonly<CourtLawUnit>()
                                   .Where(x => x.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Judge)
                                   .Where(x => NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(x.PeriodTypeId))
                                   .Where(x => x.CourtId == courtId && x.DateExpired == null)
                                   .Where(x => x.DateFrom <= date && (x.DateTo ?? endDate) >= date)
                                  .ToList();
      int[] caseLawUnitDismisel = repo.AllReadonly<CaseLawUnitDismisal>()
                                       .Include(x => x.CaseLawUnit)
                                       .Where(x => x.CaseLawUnit.CaseId == caseID)
                                       .Where(x => x.DismisalTypeId == NomenclatureConstants.DismisalType.Otvod || x.DismisalTypeId == NomenclatureConstants.DismisalType.SamoOtvod)
                                       .Where(CaseExtensions.ConfirmedDismissalsOnly())
                                       .Select(x => x.CaseLawUnit.LawUnitId).ToArray();

      int freelawUnitCount = lawUnits.Where(x => !caseLawUnitDismisel.Contains(x.LawUnitId)).Count();

      if (freelawUnitCount == 0)
      { result = false; }

      return result;
    }

    public List<SelectListItem> SelectSelectionMode_ForDropDownList()
    {
      List<SelectListItem> result = null;

      var selection_mode = repo.AllReadonly<SelectionMode>().OrderBy(x => x.OrderNumber).ToList();

      result = (from l in selection_mode


                select new SelectListItem()
                {
                  Text = l.Label,
                  Value = l.Id.ToString()

                }
                ).ToList();
      SelectListItem department = new SelectListItem()
      {
        Text = "Постоянен състав",
        Value = "4"

      };
      result.Add(department);
      result = result
                     .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                     .ToList();

      return result;
    }
    public List<SelectListItem> SelectJudgeRole_ForDropDownList()
    {
      List<SelectListItem> result = null;

      var judge_roles = repo.AllReadonly<JudgeRole>()
        .Where(x => NomenclatureConstants.JudgeRole.JuriRolesList.Contains(x.Id) || NomenclatureConstants.JudgeRole.JudgeRolesList.Contains(x.Id))
                         .OrderBy(x => x.OrderNumber).ToList();

      result = (from l in judge_roles


                select new SelectListItem()
                {
                  Text = l.Label,
                  Value = l.Id.ToString()

                }
                ).ToList();

      result = result
                  .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                  .ToList();



      return result;
    }
    public Int32 IfJuryReturnCaseIdToRedirect(int id)
    {
      Int32 result = 0;
      var model = repo.GetById<CaseSelectionProtokol>(id);

      if (NomenclatureConstants.JudgeRole.JuriRolesList.Contains(model.JudgeRoleId))
      {
        result = model.CaseId;
      }
      return result;
    }

    public CaseSessionMeeting NextCaseOpenSessionMeeting(int case_id, DateTime dateAfterSearch)
    {

      DateTime current_time = dateAfterSearch;

      var next_meeting = repo.AllReadonly<CaseSessionMeeting>()
                                                 .Include(x => x.CaseSession)
                                                 .ThenInclude(x => x.CaseLawUnits)
                                                 .Include(x => x.Case)
                                                 .Where(x => (x.CaseId == case_id) &&
                                                             //(x.CaseSession.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno) &&
                                                             (x.CaseSession.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession) &&
                                                             (x.CaseSession.DateExpired == null) &&
                                                             (x.DateExpired == null) &&
                                                             (x.DateFrom > current_time)
                                                           )
                                                 .OrderBy(x => x.DateFrom).FirstOrDefault();



      return next_meeting;
    }

    private CaseSelectionProtokolLawUnitVM[] AddJuryCrossMeatings(int case_id, int court_id, CaseSelectionProtokolLawUnitVM[] current_lawunits)
    {


      DateTime current_time = DateTime.Now;

      var next_case_meeting = NextCaseOpenSessionMeeting(case_id, DateTime.Now);

      if (next_case_meeting != null)
      {
        var all_cross_meetings = repo.AllReadonly<CaseSessionMeeting>()
                                                   .Include(x => x.CaseSession)
                                                   .ThenInclude(x => x.CaseLawUnits)
                                                   .Include(x => x.Case)
                                                    .Where(x => (x.CourtId == court_id) &&
                                                             (x.Case.Id != case_id) &&
                                                             //(x.CaseSession.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno) &&
                                                             (x.CaseSession.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession) &&
                                                             (x.CaseSession.DateExpired == null) &&
                                                             (x.DateExpired == null) &&
                                                             (x.DateFrom > current_time) &&
                                                             (
                                                             ((x.DateFrom <= next_case_meeting.DateFrom) && (x.DateTo >= next_case_meeting.DateFrom)) ||
                                                              ((next_case_meeting.DateFrom <= x.DateFrom) && (next_case_meeting.DateTo >= x.DateFrom))
                                                             )
                                                             )
                                                   .ToList();
        var courtlawUnitHolidayOrIll = repo.AllReadonly<CourtLawUnit>()
                                           .Include(x => x.LawUnit)
                                           .Where(x => x.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Jury && x.DateExpired == null &&
                                                       (((x.DateFrom <= next_case_meeting.DateFrom) && ((x.DateTo ?? next_case_meeting.DateFrom) >= next_case_meeting.DateFrom)) ||
                                                        ((next_case_meeting.DateFrom <= x.DateFrom) && (next_case_meeting.DateTo >= x.DateFrom))) &&
                                                       ((x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Ill) || (x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Holiday)) &&
                                                       (x.CourtId == next_case_meeting.CourtId))
                                           .ToList();


        foreach (var lu in current_lawunits)
        {

          var isCross = string.Empty;


          foreach (var caseSessionMeeting in all_cross_meetings)
          {
            if (caseSessionMeeting.CaseSession.CaseLawUnits.Any(x => x.LawUnitId == lu.LawUnitId))
            {
              if (isCross == string.Empty)
                isCross = "Участва в ОСЗ на дата: " + caseSessionMeeting.DateFrom.ToString("dd.MM.yyyy") + " по дело: " + caseSessionMeeting.Case.RegNumber + "/" + caseSessionMeeting.Case.RegDate.ToString("dd.MM.yyyy");
              else
                isCross += ", на дата: " + caseSessionMeeting.DateFrom.ToString("dd.MM.yyyy") + " по дело: " + caseSessionMeeting.Case.RegNumber + "/" + caseSessionMeeting.Case.RegDate.ToString("dd.MM.yyyy");

            }
          }
          if (isCross.Length > 0)
          {
            lu.StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;
            lu.Description = isCross;

          }

          //--------------------------------------------------------
          var clu = courtlawUnitHolidayOrIll.Where(x => x.LawUnitId == lu.LawUnitId && x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Ill).FirstOrDefault();

          if (clu != null)
          {
            lu.StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;
            lu.Description = "Болничен от: " + clu.DateFrom.Date.ToString("dd.MM.yyyy" + " г.");
            if (clu.DateTo != null)
            { lu.Description = lu.Description + " до: " + clu.DateTo.Value.ToString("dd.MM.yyyy" + " г."); }
            continue;
          }

          //Сетват се тези които са отпуска
          clu = courtlawUnitHolidayOrIll.Where(x => x.LawUnitId == lu.LawUnitId && x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Holiday).FirstOrDefault();
          if (clu != null)
          {
            lu.StateId = NomenclatureConstants.SelectionProtokolLawUnitState.Exclude;
            lu.Description = "Отпуск от: " + clu.DateFrom.Date.ToString("dd.MM.yyyy" + " г.");
            if (clu.DateTo != null)
            { lu.Description = lu.Description + " до: " + clu.DateTo.Value.ToString("dd.MM.yyyy" + " г."); }
            continue;
          }
          //--------------------------------------------------------

        }


      }
      return current_lawunits;
    }



    public IQueryable<CaseSelectionProtokolCaseInGroupsVM> CaseSelectionProtokolCaseInGroups(int courtId)
    {

      DateTime date_now = DateTime.Now;
      return repo.AllReadonly<CourtLoadPeriodLawUnit>()

                .Where(x => x.CourtLoadPeriod.CourtLoadResetPeriod.DateFrom < date_now && (x.CourtLoadPeriod.CourtLoadResetPeriod.DateTo ?? date_now) >= date_now)
                .Where(x => x.CourtLoadPeriod.CourtLoadResetPeriod.CourtId == courtId)
                 .Where(x => x.LawUnit.FullName.Length>0)
                  .GroupBy(x => new
                  {
            
                    x.LawUnit.FullName,
                 
                    x.CourtLoadPeriod.CourtGroup.Label

                  })
                                .Select(g => new CaseSelectionProtokolCaseInGroupsVM
                                {


                                  CourtGroupName = g.Key.Label,
                                  LawunitFullName = g.Key.FullName,
                                  RealCases =Math.Round( g.Sum(x => x.DayCases),4),
                                  AverageCases = Math.Round(g.Sum(x => x.AverageCases), 4),
                                  TotalCases = Math.Round(g.Sum(x => x.TotalDayCases), 4)





                                }).AsQueryable();
    }
  }
}

