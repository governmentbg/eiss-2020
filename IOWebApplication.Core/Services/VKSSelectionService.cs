using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Data.Models.VKS;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;
using IOWebApplication.Infrastructure.Extensions;

namespace IOWebApplication.Core.Services
{
  public class VKSSelectionService : BaseService, IVKSSelectionService
  {


    public VKSSelectionService(
    ILogger<CaseService> _logger,
    IRepository _repo,
    IUserContext _userContext)
    {
      logger = _logger;

      repo = _repo;
      userContext = _userContext;

    }

    public IQueryable<VksSelectionLawunit> VKSSelections_Lawunit_GetList(int selectionId)
    {
      DateTime dateNow = DateTime.Now;
      return repo.AllReadonly<VksSelectionLawunit>()
        .Include(x => x.JudgeDepartmentRole)
        .Include(x=>x.DepartmentType)

                   .Where(x => x.VksSelectionId == selectionId)
                   .Where(x => x.DateStart <= dateNow)
                    .Where(x => (x.DateEnd ?? dateNow) >= dateNow)
                    .Where(x => (x.DateExpired ?? dateNow) >= dateNow)
                    .AsQueryable();


    }
    public IQueryable<VksSelectionHeaderVM> VKSSelectionsHeader_GetList(bool Nakazatelni)
    {

      return repo.AllReadonly<VksSelectionHeader>()
        .Include(x => x.Selections)
        .Where(x=> (Nakazatelni && (x.Kolegia.CaseGroupId==NomenclatureConstants.CaseGroups.NakazatelnoDelo))||(!Nakazatelni && (x.Kolegia.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo)) )



                 .Select(x => new VksSelectionHeaderVM()
                 {
                   Id = x.Id,
                   KolegiaId = x.KolegiaId,
                   KolegiaName = x.Kolegia.Label + "-" + x.Kolegia.ParentDepartment.Label,
                   SelectionYear = x.SelectionYear,
                   PeriodNo = x.PeriodNo,
                   PeriodNoString = ((x.PeriodNo == 0) ? "Година" : ((x.PeriodNo == 1) ? "Първо полугодие" : "Второ полугодие")),
                   Selections = x.Selections.OrderBy(s=>s.CourtDepartment.Label).Select(s => new VksSelectionVM()
                   {
                     Id = s.Id,
                     CourtDepartmentName = s.CourtDepartment.Label
                   }).ToList()
                 }



    ).ToList().AsQueryable();

    }

    public IQueryable<VksSelectionProtocolVM> VKSSelectionProtocolList(int selectionId)
    {

      var files = repo.AllReadonly<MongoFile>();
      var tbl = repo.All<Document>().Include(x => x.DocumentType);

      return repo.AllReadonly<VksSelectionProtocol>()
        .Include(x => x.UserGenerated)
        .ThenInclude(x => x.LawUnit)
         .Include(x => x.UserSigned)
          .ThenInclude(x => x.LawUnit)

      .Where(x => x.VksSelectionId == selectionId)
      .Select(x => new VksSelectionProtocolVM()
      {
        Id = x.Id,
        DateGenerated = x.DateGenerated,
        DateSigned = x.DateSigned,
        UserGeneratedId = x.UserGeneratedId,
        UserGeneratedName = x.UserGenerated.LawUnit.FullName,
        UserSignedId = x.UserSignedId,
        UserSignedName = x.UserSigned.LawUnit.FullName,
        Selectionid = selectionId,

        FileId = files.Where(a => (a.SourceIdNumber == x.Id) && (a.SourceType == SourceTypeSelectVM.VksSelectionProtocol)).FirstOrDefault().FileId,
        FileName = files.Where(a => (a.SourceIdNumber == x.Id) && (a.SourceType == SourceTypeSelectVM.VksSelectionProtocol)).FirstOrDefault().FileName,
        IsSigned = (x.DateSigned != null)


      }
      )
      .ToList().AsQueryable();

    }
    public List<SelectListItem> GetDDL_SelectionYear()
    {

      var selectListYears = repo.All<VksSelectionHeader>()
                                 .Select(x => new SelectListItem()
                                 {
                                   Text = x.SelectionYear.ToString(),
                                   Value = x.SelectionYear.ToString()
                                 })
                                 .Distinct()

        .ToList() ?? new List<SelectListItem>();

      if (!selectListYears.Any(x => x.Value == DateTime.Now.Year.ToString()))
      {
        selectListYears = selectListYears
          .Prepend(new SelectListItem() { Text = DateTime.Now.Year.ToString(), Value = DateTime.Now.Year.ToString() })
          .ToList();
      }
      if (!selectListYears.Any(x => x.Value == (DateTime.Now.Year + 1).ToString()))
      {
        selectListYears = selectListYears
          .Prepend(new SelectListItem() { Text = (DateTime.Now.Year + 1).ToString(), Value = (DateTime.Now.Year + 1).ToString() })
          .ToList();
      }

      for (int i = DateTime.Now.Year + 2; i < 2031; i++)
      {
        selectListYears = selectListYears
     .Prepend(new SelectListItem() { Text = i.ToString(), Value = i.ToString() })
     .ToList();
      }

      var selectListItems = selectListYears

        .Select(x => new SelectListItem()
        {
          Text = x.Text,
          Value = x.Value
        })
        .Distinct()
        .OrderByDescending(x => x.Text)
        .ToList() ?? new List<SelectListItem>();




      return selectListItems;
    }

    public List<SelectListItem> GetDDL_Kolegia(int courtId, bool? Nakazatelno=true)
    {
      var selectList = repo.All<CourtDepartment>()
                               .Include(x => x.CaseGroup)
                               .Include(x => x.ParentDepartment)
                               .Where(x => x.DepartmentTypeId == NomenclatureConstants.DepartmentType.Kolegia)
                               .Where(x => x.CourtId == courtId)
                                .Where(x => ((x.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo) && (Nakazatelno==true)) || ((x.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo) && (Nakazatelno == false)))
                                 .Select(x => new SelectListItem()
                                 {
                                   Text = x.Label.ToString() + "-" + x.ParentDepartment.Label.ToString(),
                                   Value = x.Id.ToString()
                                 })
                                  .ToList();




      return selectList;
    }
    public List<SelectListItem> GetDDL_Otdelenie(int KolegiaId)
    {
      var selectList = repo.All<CourtDepartment>()
                               .Include(x => x.CaseGroup)
                               .Include(x => x.ParentDepartment)
                               .Where(x => x.DepartmentTypeId == NomenclatureConstants.DepartmentType.Otdelenie)
                               .Where(x => x.ParentId == KolegiaId)

                                 .Select(x => new SelectListItem()
                                 {
                                   Text = "Председaтелят участва в " + x.Label.ToString(),
                                   Value = x.Id.ToString()
                                 })
                                  .ToList();


      selectList = selectList
                      .Prepend(new SelectListItem() { Text = "Председaтелят не участва", Value = "-1" })
                      .ToList();

      return selectList;
    }
    public List<SelectListItem> GetDDL_OtdelenieByCourt(int KolegiaId, int? courtId = 0, int? caseGroupId=0)
    {
      var selectList = repo.All<CourtDepartment>()
                               .Include(x => x.CaseGroup)
                               .Include(x => x.ParentDepartment)
                               .Where(x => x.DepartmentTypeId == NomenclatureConstants.DepartmentType.Otdelenie)
                               .Where(x => (x.ParentId == KolegiaId && KolegiaId > 0) || KolegiaId == 0)
                               .Where(x => ((x.CourtId == courtId) && (courtId > 0)) || (courtId == 0))
                                .Where(x => ((x.CaseGroupId == caseGroupId) && (caseGroupId > 0)) || (caseGroupId == 0))
                                 .Select(x => new SelectListItem()
                                 {
                                   Text = x.Label.ToString(),
                                   Value = x.Id.ToString()
                                 })
                                  .ToList();




      return selectList;
    }
    public List<SelectListItem> GetDDL_ReplacingLawunits(int SelectioId)

    {
      var courtDepartmentId = repo.GetById<VksSelection>(SelectioId).CourtDepartmentId;
      DateTime dateNow = DateTime.Now;
      var curentDepartmentLawunits = GetActualCourtDepartmentLawunits(courtDepartmentId, true);

      int[] included_SelectionLawunits = repo.AllReadonly<VksSelectionLawunit>()
                                            .Where(x => x.VksSelectionId == SelectioId)
                                            .Where(x => x.DateStart < dateNow)
                                            .Where(x => (x.DateEnd ?? dateNow) >= dateNow)
                                            .Where(x => (x.DateExpired ?? dateNow) >= dateNow)
                                            .Select(x => x.LawunitId ?? 0).ToArray();
      List<SelectListItem> selectList = new List<SelectListItem>();

      selectList = curentDepartmentLawunits.Where(x => !included_SelectionLawunits.Contains(x.LawUnitId))


                                 .Select(x => new SelectListItem()
                                 {
                                   Text = x.LawUnit.FullName,
                                   Value = x.LawUnitId.ToString()
                                 })
                                 .ToList();


      selectList = selectList
                 .Prepend(new SelectListItem() { Text = "Изберете", Value = "-1" })
                 .ToList();

      return selectList;
    }

    public List<SelectListItem> GetDDL_SelectionPeriod()
    {
      var selectListItems = new List<SelectListItem>();


      selectListItems = selectListItems
          .Prepend(new SelectListItem() { Text = "Второ полугодие", Value = "2" })
          .ToList();
      selectListItems = selectListItems
            .Prepend(new SelectListItem() { Text = "Първо полугодие", Value = "1" })
            .ToList();




      return selectListItems; ;
    }

    public VksSelection VKSSelection_Get(int id)
    {
      return repo.AllReadonly<VksSelection>()
              .Include(x => x.SelectionLawunit)
               .Include(x => x.Months)
               .Include(x => x.VksSelectionHeader)
               .Include(x => x.CourtDepartment)
               .Where(x => x.Id == id).FirstOrDefault();
    }


    public VksSelectionHeaderVM VKSSelectionsHeader_Get(int id)
    {


      var selection = repo.AllReadonly<VksSelectionHeader>()

                 .Where(x => x.Id == id)
                 .Select(x => new VksSelectionHeaderVM()
                 {
                   Id = x.Id,
                   KolegiaId = x.KolegiaId,
                   Months = x.Months,
                   SelectionYear = x.SelectionYear,
                   PeriodNo = x.PeriodNo,
                   PeriodNoString = ((x.PeriodNo == 1) ? "Първо шестмесечие" : "Второ шестмесечие")

                 }).FirstOrDefault();




      selection.AddMonthsNomenclature(selection.PeriodNo);


      int[] monthsInt = selection.Months.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();

      foreach (var item in selection.YearMonths)
      {
        if (monthsInt.Contains(item.Value.ToInt()))
        { item.Checked = true; }
        else
        {
          { item.Checked = false; }
        }
        int m = item.Value.ToInt();
        item.DdlValue = repo.AllReadonly<VksSelectionMonth>()
                           .Include(x => x.VksSelection)
                          .Where(x => x.VksSelection.VksSelectionHeader.Id == id)
                          .Where(x => x.ChairmanIn == true)
                          .Where(x => x.SelectionMonth == m)
                          .Select(x => x.VksSelection.CourtDepartmentId)
                          .FirstOrDefault();
      }



      return selection;


    }

    public int VKSSelectionHeader_Save(VksSelectionHeaderVM model)

    {
      int current_id = model.Id ?? 0;

      try
      {
        if (current_id > 0)
        // Редакция 
        {
          var saved = repo.All<VksSelectionHeader>()
            .Include(x => x.Selections)

            .Where(x => x.Id == model.Id).FirstOrDefault();

          saved.UserId = userContext.UserId;
          saved.DateWrt = DateTime.Now;
          saved.Months = model.YearMonths.Where(x => x.Checked == true).Select(x => x.Value.ToInt()).ToArray().ConcatenateWithSeparator();
          repo.Update(saved);
          repo.SaveChanges();
          var mod = Add__Rows_For_Departments(saved, model.YearMonths,true);

        }
        else
        //Нов
        {
          VksSelectionHeader selection = new VksSelectionHeader();
          selection.KolegiaId = model.KolegiaId;
          selection.SelectionYear = model.SelectionYear;
          selection.PeriodNo = model.PeriodNo;
          selection.VksSelectionStateId = NomenclatureConstants.VKS_const.SelectionState.New;
          if (model.PeriodNo == 1)
          {
            selection.Months = "1,2,3,4,5,6";
          }
          else
          {
            selection.Months = "7,8,9,10,11,12";
          }
          selection.UserId = userContext.UserId;
          selection.DateWrt = DateTime.Now;



          repo.Add<VksSelectionHeader>(selection);
          repo.SaveChanges();
          var mod = Add__Rows_For_Departments(selection, model.YearMonths,true);
          current_id = selection.Id;
        }

      }
      catch (Exception ex)
      {

        throw;
      }


      return current_id;



    }
    public int VKSSelectionHeaderTO_Save(VksSelectionHeaderVM model)

    {
      int current_id = model.Id ?? 0;

      try
      {
        if (current_id > 0)
        // Редакция 
        {
          var saved = repo.All<VksSelectionHeader>()
            .Include(x => x.Selections)

            .Where(x => x.Id == model.Id).FirstOrDefault();

          saved.UserId = userContext.UserId;
          saved.DateWrt = DateTime.Now;
          saved.Months = model.YearMonths.Where(x => x.Checked == true).Select(x => x.Value.ToInt()).ToArray().ConcatenateWithSeparator();
          repo.Update(saved);
          repo.SaveChanges();
         var mod = Add__Rows_For_Departments(saved, model.YearMonths,false);

        }
        else
        //Нов
        {
          VksSelectionHeader selection = new VksSelectionHeader();
          selection.KolegiaId = model.KolegiaId;
          selection.SelectionYear = model.SelectionYear;
          selection.PeriodNo = 0;
          selection.VksSelectionStateId = NomenclatureConstants.VKS_const.SelectionState.New;
          selection.Months = "1,2,3,4,5,6,7,8,9,10,11,12";
          selection.UserId = userContext.UserId;
          selection.DateWrt = DateTime.Now;
          model.YearMonths = new List<CheckListWithDdlVM>();
          //for (int i = 1; i < 13; i++)
          //{
          //  CheckListWithDdlVM month = new CheckListWithDdlVM();
          //  month.DdlValue = i;
          //  month.Checked = true;
          //  model.YearMonths.Add(month);
          //}


          repo.Add<VksSelectionHeader>(selection);
          repo.SaveChanges();
          var mod = Add__Rows_For_Departments(selection, model.YearMonths,false);
          current_id = selection.Id;
        }

      }
      catch (Exception ex)
      {

        throw;
      }


      return current_id;



    }

    public bool Already_Exist(VksSelectionHeaderVM model)
    {
      var result = false;
      try

      {
        result = (repo.AllReadonly<VksSelectionHeader>()
    .Where(x => x.SelectionYear == model.SelectionYear)
    .Where(x => x.KolegiaId == model.KolegiaId)
    .Where(x => x.PeriodNo == model.PeriodNo).Count() > 0);
      }
      catch (Exception)
      {


      }
      return result;
    }


    private VksSelectionHeader Add__Rows_For_Departments(VksSelectionHeader model, List<CheckListWithDdlVM> YearMonths, Boolean Nakazatelno)
    {
      int[] departmetsId = repo.AllReadonly<CourtDepartment>()
                          .Where(x => x.ParentId == model.KolegiaId)
                          .Select(x => x.Id).ToArray();

      foreach (var dpnt in departmetsId)
      {
        if (Nakazatelno)
        {
          var mod = Add_Department_Row(model, YearMonths, dpnt);
        }
        else 
        {
          var mod = Add_Department_RowTO(model, dpnt);
        }
     
      }

      return model;

    }
    private VksSelectionHeader Add_Department_Row(VksSelectionHeader model, List<CheckListWithDdlVM> YearMonths, int departmentId)
    {
      var now = DateTime.Now;
      int[] monthsInt = model.Months.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();

      int[] chairmannDepattmentMonths = new int[6];
      if (YearMonths != null)
      {
        chairmannDepattmentMonths = YearMonths.Where(x => x.DdlValue == departmentId).Select(x => x.Value.ToInt()).ToArray();
      }


      VksSelection vks_selection = repo.All<VksSelection>()
                                     .Include(x => x.Months)
                                     .Where(x => x.VksSelectionHeaderId == model.Id)
                                     .Where(x => x.CourtDepartmentId == departmentId)
                                     .FirstOrDefault();

      if (vks_selection == null)
      //Ако е нов период
      {
        vks_selection = new VksSelection();
        vks_selection.VksSelectionHeaderId = model.Id;
        vks_selection.CourtDepartmentId = departmentId;
        vks_selection.DateWrt = now;
        vks_selection.UserId = userContext.UserId;

        //Добавя редове по месеци и дати
        //За всеки избран месец 
        foreach (var mnt in monthsInt)
        {
          //За всвка от 6-те дати
          for (int i = 1; i < 7; i++)
          {
            VksSelectionMonth sel_month = new VksSelectionMonth();
            sel_month.SelectionMonth = mnt;
            sel_month.SelectionDay = i;
            sel_month.DateWrt = now;
            sel_month.UserId = userContext.UserId;
            sel_month.ChairmanIn = chairmannDepattmentMonths.Contains(mnt);
            vks_selection.Months.Add(sel_month);

          }
        }

       var actual_lu= GetActualCourtDepartmentLawunits(departmentId, true).ToList();

        vks_selection.SelectionLawunit = actual_lu
                                              .Select(x => new VksSelectionLawunit()
                                              {
                                                LawunitId = x.LawUnitId,
                                                IsUnknownJudge = false,
                                                JudgeDepartmentRoleId = (x.JudgeDepartmentRoleId??2),
                                                CourtDepartmentTypeId=x.CourtDepartment.DepartmentTypeId,
                                                DateWrt = now,
                                                DateStart = now,

                                                LawunitName = x.LawUnit.FullName

                                              })
                                  .ToList();

        foreach (var item in vks_selection.SelectionLawunit)
        {
          item.LawunitKey = Guid.NewGuid().ToString();
        }
        repo.Add<VksSelection>(vks_selection);
        repo.SaveChanges();
      }
      else
      //Ako е редакция
      {
        foreach (var item in vks_selection.Months)
        {
          item.ChairmanIn = chairmannDepattmentMonths.Contains(item.SelectionMonth);
          repo.Update(vks_selection);
        }
        repo.SaveChanges();

        int[] alredyExist = repo.AllReadonly<VksSelectionMonth>()
                              .Where(x => x.VksSelectionId == vks_selection.Id)
                              .Select(x => x.SelectionMonth).Distinct().ToArray();

        int[] new_monthsInt = monthsInt.Where(x => !alredyExist.Contains(x)).ToArray();

        foreach (var mnt in new_monthsInt)
        {
          //За всвка от 6-те дати
          for (int i = 1; i < 7; i++)
          {
            VksSelectionMonth sel_month = new VksSelectionMonth();
            sel_month.SelectionMonth = mnt;
            sel_month.SelectionDay = i;
            sel_month.DateWrt = now;
            sel_month.UserId = userContext.UserId;
            sel_month.ChairmanIn = chairmannDepattmentMonths.Contains(mnt);
            vks_selection.Months.Add(sel_month);

          }
        }

        repo.Update<VksSelection>(vks_selection);
        repo.SaveChanges();
        var for_deletion = vks_selection.Months.Where(x => !monthsInt.Contains(x.SelectionMonth)).ToList();
        foreach (var item in for_deletion)
        {
          vks_selection.Months.Remove(item);
        }
        repo.Update<VksSelection>(vks_selection);
        // repo.DeleteRange<VksSelectionMonth>(for_deletion);
        repo.SaveChanges();


      }

      return model;
    }

    private VksSelectionHeader Add_Department_RowTO(VksSelectionHeader model,  int departmentId)
    {
      var now = DateTime.Now;
      var sastavi = repo.AllReadonly<CourtDepartment>()
                   
                      .Where(x => x.DepartmentTypeId == NomenclatureConstants.DepartmentType.Systav)
                      .Where(x => x.ParentId == departmentId).ToList();

      int[] sastavi_arr = sastavi.Select(r => r.Id).ToArray();
      var lawunits = repo.AllReadonly<CourtDepartmentLawUnit>()
                       .Include(x => x.LawUnit)
                       .Include(x => x.CourtDepartment)
                       .Where(x => sastavi_arr.Contains(x.CourtDepartmentId))
                       .Where(x => x.DateFrom < now)
                       .Where(x => (x.DateTo ?? now) >= now).ToList();

      VksSelection vks_selection = new VksSelection();
      vks_selection = new VksSelection();
      vks_selection.VksSelectionHeaderId = model.Id;
      vks_selection.CourtDepartmentId = departmentId;
      vks_selection.DateWrt = now;
      vks_selection.UserId = userContext.UserId;

      vks_selection.SelectionLawunit = lawunits
                                            .Select(x => new VksSelectionLawunit()
                                            {
                                              LawunitId = x.LawUnitId,
                                              IsUnknownJudge = false,
                                              JudgeDepartmentRoleId = (x.JudgeDepartmentRoleId ?? 2),
                                              CourtDepartmentTypeId = x.CourtDepartment.DepartmentTypeId,
                                              DateWrt = now,
                                              DateStart = now,

                                              LawunitName = x.LawUnit.FullName

                                            })
                                .ToList();

      foreach (var item in vks_selection.SelectionLawunit)
      {
        item.LawunitKey = Guid.NewGuid().ToString();
      }
      repo.Add<VksSelection>(vks_selection);
      repo.SaveChanges();


      foreach (var sastav in sastavi)  //За всеки състав
      {
        for (int mnt = 1; mnt < 13; mnt++) // За всеки месец
        {

          for (int ses = 1; ses < 5; ses++) // По 4 заседания на месец
          {
            VksSelectionMonth sel_month = new VksSelectionMonth();
            sel_month.SelectionMonth = mnt;
            sel_month.SelectionDay = ses;
            sel_month.DateWrt = now;
            sel_month.UserId = userContext.UserId;
            sel_month.ChairmanIn = true;
            foreach (var lawunit in lawunits.Where(x=>x.CourtDepartmentId==sastav.Id))
            {
              VksSelectionMonthLawunit selectionMonthLawunit = new VksSelectionMonthLawunit();
              var cur_lu = vks_selection.SelectionLawunit.Where(x => x.LawunitId == lawunit.LawUnitId).FirstOrDefault();
              selectionMonthLawunit.LawunitKey = cur_lu.LawunitKey;
              selectionMonthLawunit.VksSelectionLawunitId = cur_lu.Id;

              selectionMonthLawunit.UserId = userContext.UserId;
              selectionMonthLawunit.DateWrt = now;
              sel_month.SelectionMonthLawunit.Add(selectionMonthLawunit);
        
    

            }
            //sel_month.SelectionHash = sel_month.SelectionMonthLawunit.Select(x => x.VksSelectionLawunit.LawunitId ?? 0).OrderBy(x => x).ToArray().ConcatenateWithSeparator();

            vks_selection.Months.Add(sel_month);
            


          }

        }

      }
      repo.Update<VksSelection>(vks_selection);
      repo.SaveChanges();



      return model;
    }

    private int[] RandomizeLawUnitsArray(ICollection<VksSelectionLawunit> selectionLawUnitsColection, bool chairman_in)
    {
      var dateNow = DateTime.Now;


      var selectionLawUnits = selectionLawUnitsColection.ToList();
      selectionLawUnits = selectionLawUnits
                         .Where(x => x.DateStart < dateNow)
                         .Where(x => (x.DateEnd ?? dateNow) >= dateNow)
                          .Where(x => (x.DateExpired ?? dateNow) >= dateNow).ToList();

      if (chairman_in == false)
      {
        selectionLawUnits = selectionLawUnits.Where(x =>  x.CourtDepartmentTypeId!=NomenclatureConstants.DepartmentType.Kolegia).ToList();
      }

      int[] rnd_arr = new int[selectionLawUnits.Count + 1];
      int i = 0;
      List<VksSelectionLawunit> NoNameLawUnits = new List<VksSelectionLawunit>();
      List<VksSelectionLawunit> NamedLawUnits = new List<VksSelectionLawunit>();
      NoNameLawUnits = selectionLawUnits.Where(x => x.IsUnknownJudge == true).ToList();

      NamedLawUnits = selectionLawUnits.Where(x => x.IsUnknownJudge == false).ToList();
      System.Random rnd = new System.Random();
      while (NoNameLawUnits.Count > 0)
      {
        i++;
        int index = rnd.Next(NoNameLawUnits.Count - 1);
        rnd_arr[i] = NoNameLawUnits[index].Id;
        NoNameLawUnits.RemoveAll(x => x == NoNameLawUnits[index]);

      }

      while (NamedLawUnits.Count > 0)
      {
        i++;
        int index = rnd.Next(NamedLawUnits.Count - 1);

        rnd_arr[i] = NamedLawUnits[index].Id;
        NamedLawUnits.RemoveAll(x => x == NamedLawUnits[index]);

      }

      return rnd_arr;


    }
    /// <summary>
    /// Генерира съставите за един Месец
    /// </summary>
    /// <param name="selection"></param>
    /// <param name="month"></param>
    /// <returns></returns>
    private bool GenerateSelectionMonthStaff(VksSelection selection, int month)
    {
      var dateNow = DateTime.Now;
      var result = false;
      try
      {
        //Изтриват се предходните разпределения
        repo.DeleteRange<VksSelectionMonthLawunit>(repo.All<VksSelectionMonthLawunit>().Where(x => x.VksSelectionMonth.SelectionMonth == month).Where(x => x.VksSelectionMonth.VksSelectionId == selection.Id));
        repo.SaveChanges();

        //Раздават се случайни номера  на съдиите (разбъркват се)
        var randomized_staff = RandomizeLawUnitsArray(selection.SelectionLawunit, selection.Months.Where(x => x.SelectionMonth == month).FirstOrDefault().ChairmanIn);
        foreach (var m in selection.Months.Where(x => x.SelectionMonth == month).ToList())
        {
          //За всеки от 6 те дни на заседания се взема темплейтен състав
          int[] TemplateStaff = NomenclatureConstants.VKS_const.StringMatrix[m.SelectionDay - 1].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();


          //На всеки темплейтен съдия се взема аналога от разбъркания списък на състава
          foreach (var tempStf in TemplateStaff)
          {
            //За всеки темплейтен номер се проверява дали не е по голям от броя на съдиите в отделението
            if (tempStf <= randomized_staff.Length - 1)
            {
              VksSelectionMonthLawunit selectionMonthLawunit = new VksSelectionMonthLawunit();
              selectionMonthLawunit.VksSelectionMonthId = m.Id;
              var lu = repo.GetById<VksSelectionLawunit>(randomized_staff[tempStf]);
              selectionMonthLawunit.VksSelectionLawunitId = randomized_staff[tempStf];
              selectionMonthLawunit.LawunitKey = lu.LawunitKey;
              selectionMonthLawunit.UserId = userContext.UserId;
              selectionMonthLawunit.DateWrt = dateNow;
              repo.Add<VksSelectionMonthLawunit>(selectionMonthLawunit);
              repo.SaveChanges();
              m.SelectionHash = repo.AllReadonly<VksSelectionMonthLawunit>().Where(x => x.VksSelectionMonthId == m.Id).Select(x => x.VksSelectionLawunit.LawunitId ?? 0).OrderBy(x => x).ToArray().ConcatenateWithSeparator();
              repo.Update<VksSelectionMonth>(m);
              repo.SaveChanges();
            }


          }



        }
        result = true;
      }
      catch (Exception ex)
      {
        throw (ex);

      }

      return result;
    }

    public bool GenerateSelectionSTAFF(int selectionID)
    {
      var result = false;
      try
      {
        var selection = repo.All<VksSelection>()
                          .Include(x => x.VksSelectionHeader)
                          .Include(x => x.Months)
                          .Include(x => x.SelectionLawunit)
                          .Where(x => x.Id == selectionID).FirstOrDefault();

        int[] months = selection.Months.Select(x => x.SelectionMonth).Distinct().ToArray();

        foreach (var m in months)
        {
          int loopCount = 0;
          bool doAgain = true;
          while (doAgain && (loopCount < 2000))
          {
            result = GenerateSelectionMonthStaff(selection, m);
            doAgain = IsSelectionStaffSameAsPrevious(selection, m);
            if (doAgain)
            {
              DeleteSelectionLawUnits(selection.Id, m);
              loopCount++;

            }

          }

        }


      }
      catch (Exception ex)
      {
        throw (ex);
      }
      return result;

    }




    /// <summary>
    /// Актуализира Съдиите от отделението с актуалните в момента
    /// </summary>
    /// <param name="selection"></param>
    /// <returns></returns>
    public bool SelectionLawUnitsUpdate(VksSelection selection)
    {
      var result = false;
      DateTime now = DateTime.Now;
      try
      {
        var actualDepartmentLawUnits = GetActualCourtDepartmentLawunits(selection.CourtDepartmentId, true)

                                             .Select(x => new VksSelectionLawunit()
                                             {
                                               LawunitId = x.LawUnitId,
                                               IsUnknownJudge = false,
                                               JudgeDepartmentRoleId = (x.JudgeDepartmentRoleId??2),
                                               CourtDepartmentTypeId=x.CourtDepartment.DepartmentTypeId,
                                               DateWrt = now,
                                               DateStart = now,

                                               LawunitName = x.LawUnit.FullName

                                             })
                                 .ToList();

        int[] actualDepartmentLawUnitsArr = actualDepartmentLawUnits.Select(x => x.LawunitId ?? 0).ToArray();
        foreach (var lawunit in selection.SelectionLawunit.Where(x => (!actualDepartmentLawUnitsArr.Contains(x.LawunitId ?? 0)) && (x.IsUnknownJudge == false)).ToList())
        {
          lawunit.DateEnd = now;
          lawunit.UserId = userContext.UserId;
          repo.Update<VksSelection>(selection);
          repo.SaveChanges();

        }
        int[] actualSelectionLawUnitsArr = selection.SelectionLawunit
           .Where(x => x.DateStart <= now)
           .Where(x => (x.DateEnd ?? now) >= now)
           .Where(x => (x.DateExpired ?? now) >= now)
          .Select(x => x.LawunitId ?? 0).ToArray();
        foreach (var lawunit in actualDepartmentLawUnits.Where(x => (!actualSelectionLawUnitsArr.Contains(x.LawunitId ?? 0)) && (x.IsUnknownJudge == false)).ToList())
        {
          selection.SelectionLawunit.Add(lawunit);
          repo.Update<VksSelection>(selection);
          repo.SaveChanges();

        }


        result = true;
      }
      catch (Exception ex)
      {
        throw (ex);
      }
      return result;

    }

    public List<CourtDepartmentLawUnit> GetActualCourtDepartmentLawunits(int courtDepartrmentID, bool includeParentPredsedatel = false)
    {
      DateTime now = DateTime.Now;
      var result = repo.AllReadonly<CourtDepartmentLawUnit>()
                                            .Include(x => x.LawUnit)
                                             .Include(x => x.CourtDepartment)
                                            .Where(x => (x.CourtDepartmentId == courtDepartrmentID&&x.CourtDepartment.DepartmentTypeId == NomenclatureConstants.DepartmentType.Otdelenie) ||(x.CourtDepartment.ParentId==courtDepartrmentID && x.CourtDepartment.DepartmentTypeId==NomenclatureConstants.DepartmentType.Systav ))
                                            .Where(x => x.DateFrom < now)
                                            .Where(x => (x.DateTo ?? now) >= now).ToList();
      try
      {
        if (includeParentPredsedatel)
        {
          var parentCourtDepartmentId = repo.GetById<CourtDepartment>(courtDepartrmentID).ParentId ?? 0;
          var predsedatel = repo.AllReadonly<CourtDepartmentLawUnit>()
                                               .Include(x => x.LawUnit)
                                                  .Include(x => x.CourtDepartment)
                                               .Where(x => x.CourtDepartmentId == parentCourtDepartmentId)
                                               .Where(x => x.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel)
                                               .Where(x => x.DateFrom < now)
                                               .Where(x => (x.DateTo ?? now) >= now).FirstOrDefault();
          if (predsedatel != null)
          {
            result.Add(predsedatel);
          }
        }
      }
      catch (Exception)
      {

    
      }
      
      
      return result;
    }

    /// <summary>
    /// Добавя неизвестен с пореден наомер
    /// </summary>
    /// <param name="selection"></param>
    /// <returns></returns>
    public bool AddUnknownLawUnits(VksSelection selection)
    {
      var result = false;
      DateTime now = DateTime.Now;
      try
      {

        var currentUnknownlawUnits = selection.SelectionLawunit.Where(x => x.IsUnknownJudge == true).ToList();
        int unknownNumber = 0;
        if (currentUnknownlawUnits != null)
        {
          unknownNumber = currentUnknownlawUnits.Count();

        }
        unknownNumber++;
        VksSelectionLawunit lu = new VksSelectionLawunit();
        lu.DateStart = now;
        lu.DateWrt = now;
        lu.IsUnknownJudge = true;
        lu.LawunitKey = Guid.NewGuid().ToString();
        lu.LawunitName = "Неизвестен " + unknownNumber.ToString();
        selection.SelectionLawunit.Add(lu);
        repo.Update<VksSelection>(selection);
        repo.SaveChanges();



        result = true;
      }
      catch (Exception ex)
      {
        throw (ex);
      }
      return result;

    }
    /// <summary>
    /// Изтрива Неизвестен с най-голям номер без значение, кой неизвестен е избран
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public int DeleteLastUnknownLawUnits(int id)
    {
      int result = 0;
      DateTime now = DateTime.Now;
      try
      {
        var current = repo.GetById<VksSelectionLawunit>(id);
        result = current.VksSelectionId;
        var currentUnknownlawUnits = repo.All<VksSelectionLawunit>()
                                     .Where(x => x.VksSelectionId == current.VksSelectionId)
                                     .Where(x => x.IsUnknownJudge == true).ToList();

        var unknownNumber = currentUnknownlawUnits.Count();

        foreach (var item in currentUnknownlawUnits)
        {
          if (item.LawunitName.IndexOf(unknownNumber.ToString()) > 0)
          {
            repo.Delete<VksSelectionLawunit>(item);
            repo.SaveChanges();
          }
        }





      }
      catch (Exception ex)
      {
        throw (ex);
      }
      return result;

    }

    public int DeleteLawUnits(int id)
    {
      int result = 0;
      DateTime now = DateTime.Now;
      try
      {
        var current = repo.GetById<VksSelectionLawunit>(id);
        result = current.VksSelectionId;
        if (current.IsUnknownJudge == true)
        {
          result = DeleteLastUnknownLawUnits(id);
        }
        else
        {
          current.DateExpired = now;
          current.DateEnd = now;
          current.UserExpiredId = userContext.UserId;
          repo.Update<VksSelectionLawunit>(current);
          repo.SaveChanges();

        }


      }
      catch (Exception ex)
      {
        throw (ex);
      }
      return result;

    }

    public VksSelectionCalendarVM GetSelectionCalendar(int selection_id)
    {
      DateTime now = DateTime.Now;
      var selection = repo.All<VksSelection>()
                       .Include(x => x.VksSelectionHeader)
                       .Include(x => x.CourtDepartment)
                       .Include(x=>x.SelectionLawunit)
                       .Include(x => x.Months)

                       .ThenInclude(x => x.SelectionMonthLawunit)
                       .ThenInclude(x => x.VksSelectionLawunit)
                            .ThenInclude(x => x.LawUnit)
                        .Include(x => x.User)
                        .ThenInclude(x => x.LawUnit)

                       .Where(x => x.Id == selection_id).FirstOrDefault();
      VksSelectionCalendarVM selectionVM = new VksSelectionCalendarVM();

      selectionVM.Id = selection.Id;
      selectionVM.PeriodNo = selection.VksSelectionHeader.PeriodNo;
      selectionVM.PeriodNoString = (selection.VksSelectionHeader.PeriodNo == 1) ? "Първо полугодие" : "Второ полугодие";
      selectionVM.SelectionYear = selection.VksSelectionHeader.SelectionYear;
      selectionVM.CourtDepartmentId = selection.CourtDepartmentId;
      selectionVM.CourtDepartmentName = selection.CourtDepartment.Label;
      selectionVM.SignJudgeName = userContext.FullName; 
      int[] mnt = selection.Months.OrderBy(x => x.SelectionMonth).Select(x => x.SelectionMonth).Distinct().ToArray();
      var lawUnitsOrder = repo.AllReadonly<CourtLawUnitOrder>()
                                .Where(x => x.CourtId == userContext.CourtId)
                                .ToList();

      


          foreach (var month in mnt)
      {
        VksSelectionOneMonthListVM oneMonthList = new VksSelectionOneMonthListVM();
        oneMonthList.SelectionMonth = month;
        oneMonthList.SelectionMonthString = IntToMonth(month);

        foreach (var selestion_month in selection.Months.Where(x => x.SelectionMonth == month).OrderBy(x => x.SelectionDay))

        {
          int judgeCount = 0;
          VksMonthSessions monthSession = new VksMonthSessions();
          monthSession.Id = selestion_month.Id;
          monthSession.SelectionDay = selestion_month.SelectionDay;
          monthSession.SessionDate = selestion_month.SessionDate;
          foreach (var month_lawunit in selestion_month.SelectionMonthLawunit)
          {
            //Za podrevdane po starshinstvo
            if (month_lawunit.VksSelectionLawunit.IsUnknownJudge)
            {
            
              month_lawunit.VksSelectionLawunit.OrderNumber = int.MaxValue;
              continue;
            }

            if (month_lawunit.VksSelectionLawunit.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel)
            {
              //Председателите излизат първи
              month_lawunit.VksSelectionLawunit.OrderNumber = int.MinValue;
              continue;
            }

            //Останалите лица в състава се редят по реда на старшинство
            var _order = lawUnitsOrder.FirstOrDefault(x => x.LawUnitId == month_lawunit.VksSelectionLawunit.LawunitId);
            if (_order != null)
            {
              month_lawunit.VksSelectionLawunit.OrderNumber = _order.OrderNumber;
            }
          }

            foreach (var month_lawunit in selestion_month.SelectionMonthLawunit.OrderBy(x=>x.VksSelectionLawunit.OrderNumber))
          {
            monthSession.SelectionStaff = monthSession.SelectionStaff + month_lawunit.VksSelectionLawunit.LawunitName + "<br />";
            string separator = "";
            if (judgeCount > 0)
            {
              separator = ", ";
            }

            string shortName = "";
            if (month_lawunit.VksSelectionLawunit.IsUnknownJudge)
            {
              shortName = month_lawunit.VksSelectionLawunit.LawunitName;
            }
            else
            {
              shortName = month_lawunit.VksSelectionLawunit.LawUnit.FamilyName;
            }
            if (judgeCount < 3)
            {
              monthSession.SelectionStaffShortNames = monthSession.SelectionStaffShortNames + separator + shortName;
            }
            else { monthSession.SelectionStaffShortNames = monthSession.SelectionStaffShortNames + separator + "<br />" + shortName + " (" + IntToRome(judgeCount + 1) + " докладчик)"; }

            judgeCount++;
          }

          //////////////////////////////Добавяне на Check за редакция на съдии
          ///
          var staff_arr = selestion_month.SelectionMonthLawunit.Select(r => r.VksSelectionLawunitId).ToArray();
          monthSession.LawunitsList = selection.SelectionLawunit
                                            .Where(x=>x.DateEnd==null)
                                            .Select(x => new CheckListVM()
          {
            Label = x.LawunitName,
            Value = (x.Id).ToString(),
            Checked = staff_arr.Contains(x.Id)

          }).ToList();

            ///
          oneMonthList.MonthSessions.Add(monthSession);



        }
        selectionVM.MonthList.Add(oneMonthList);



      }

      return selectionVM;


    }
    public bool Save_MonthSessionsDates(VksSelectionCalendarVM model)
    {
      bool result = false;
      int updated = 0;
      try
      {


        foreach (var month in model.MonthList)
        {
          foreach (var item in month.MonthSessions)
          {
            var saved = repo.GetById<VksSelectionMonth>(item.Id);
            saved.SessionDate = item.SessionDate;
            repo.Update(saved);
            updated++;

          }
        }
        if (updated > 0)
        {
          repo.SaveChanges();
        }
        result = true;
      }
      catch (Exception ex)
      {


      }

      return result;

    }

    public bool Save_MonthSessionsDatesEDIT(VksSelectionCalendarVM model)
    {
      bool result = false;
      int updated = 0;
      try
      {


        foreach (var month in model.MonthList)
        {
          foreach (var item in month.MonthSessions)
          {
            var saved = repo.GetById<VksSelectionMonth>(item.Id);
            saved.SessionDate = item.SessionDate;
            repo.Update(saved);

            var checked_lawunits = item.LawunitsList.Where(x => x.Checked).ToList();
            var saved_lu = repo.All<VksSelectionMonthLawunit>().Where(x => x.VksSelectionMonthId == item.Id).ToList();
            repo.DeleteRange(saved_lu);
            repo.SaveChanges();
            ///////////////////////////////////////////////////////
            ///
            foreach (var lawunit in checked_lawunits)
            {
              VksSelectionMonthLawunit selectionMonthLawunit = new VksSelectionMonthLawunit();
              selectionMonthLawunit.VksSelectionMonthId = item.Id;
              var lu = repo.GetById<VksSelectionLawunit>(lawunit.Value.ToInt());
              selectionMonthLawunit.VksSelectionLawunitId = lu.Id;
              selectionMonthLawunit.LawunitKey = lu.LawunitKey;
              selectionMonthLawunit.UserId = userContext.UserId;
              selectionMonthLawunit.DateWrt = DateTime.Now;
              repo.Add<VksSelectionMonthLawunit>(selectionMonthLawunit);
              repo.SaveChanges();

              saved.SelectionHash = repo.AllReadonly<VksSelectionMonthLawunit>().Where(x => x.VksSelectionMonthId == saved.Id).Select(x => x.VksSelectionLawunit.LawunitId ?? 0).OrderBy(x => x).ToArray().ConcatenateWithSeparator();
              repo.Update<VksSelectionMonth>(saved);
              repo.SaveChanges();
            }
          
            ////////////////////////



            updated++;

          }
        }
        if (updated > 0)
        {
          repo.SaveChanges();
        }
        result = true;
      }
      catch (Exception ex)
      {


      }

      return result;

    }


    private string IntToMonth(int month)
    {
      string result = "";
      switch (month)
      {
        case 1:
          result = "Януари";
          break;
        case 2:
          result = "Февруари";
          break;
        case 3:
          result = "Март";
          break;
        case 4:
          result = "Април";
          break;
        case 5:
          result = "Май";
          break;
        case 6:
          result = "Юни";
          break;
        case 7:
          result = "Юли";
          break;
        case 8:
          result = "Август";
          break;
        case 9:
          result = "Септември";
          break;
        case 10:
          result = "Октомври";
          break;
        case 11:
          result = "Ноември";
          break;
        case 12:
          result = "Декември";
          break;
        default:
          result = "";
          break;
      }
      return result;


    }
    private string IntToRome(int i)
    {
      string result = "";
      switch (i)
      {
        case 1:
          result = "I";
          break;
        case 2:
          result = "II";
          break;
        case 3:
          result = "III";
          break;
        case 4:
          result = "IV";
          break;
        case 5:
          result = "V";
          break;
        case 6:
          result = "VI";
          break;
        case 7:
          result = "VII";
          break;
        case 8:
          result = "VII";
          break;
        case 9:
          result = "IX";
          break;
        case 10:
          result = "X";
          break;

        default:
          result = "";
          break;
      }
      return result;


    }
    string[] GetPrevSelectionStaffsForDepartment(VksSelection selection, int selectionMonth)
    {
      string[] prevSelectionStaffs = new string[6];
      int curentYear = selection.VksSelectionHeader.SelectionYear;
      var prevsel = repo.AllReadonly<VksSelectionMonth>()
                  .Where(x => x.VksSelection.CourtDepartmentId == selection.CourtDepartmentId)
                   .Where(x => !(x.SelectionMonth == selectionMonth && x.VksSelection.VksSelectionHeader.SelectionYear == selection.VksSelectionHeader.SelectionYear))
                   .OrderByDescending(x => x.VksSelection.VksSelectionHeader.SelectionYear)
                   .ThenByDescending(x => x.SelectionMonth).FirstOrDefault();
      if (prevsel != null)
      {
        prevSelectionStaffs = repo.AllReadonly<VksSelectionMonth>()
          .Where(x => x.VksSelectionId == prevsel.VksSelectionId)
           .Where(x => x.SelectionMonth == prevsel.SelectionMonth)
           .Select(x => x.SelectionHash).ToArray();
      }
      return prevSelectionStaffs;

    }

    Boolean IsSelectionStaffSameAsPrevious(VksSelection selection, int selectionMonth)
    {
      var prevStafF = GetPrevSelectionStaffsForDepartment(selection, selectionMonth);
      return (repo.AllReadonly<VksSelectionMonth>().Where(x => x.VksSelectionId == selection.Id).Where(x => x.SelectionMonth == selectionMonth).Where(x => prevStafF.Contains(x.SelectionHash)).Count() > 0);

    }

    Boolean DeleteSelectionLawUnits(int selectionId, int selectionMonth)
    {
      bool result = false;
      try
      {
        repo.DeleteRange<VksSelectionMonthLawunit>(repo.All<VksSelectionMonthLawunit>().Where(x => x.VksSelectionMonth.SelectionMonth == selectionMonth).Where(x => x.VksSelectionMonth.VksSelectionId == selectionId));
        repo.SaveChanges();
        result = true;
      }
      catch (Exception)
      {


      }
      return result;

    }

    public VksSelectionLawunit GetSelectionLawunitByID(int id)
    {


      return repo.GetById<VksSelectionLawunit>(id);

    }

    public Boolean ReplacedLawunitSave(VksSelectionLawunit model)
    {
      bool result = false;
      DateTime now = DateTime.Now;
      try
      {
        VksSelectionLawunit curent = repo.GetById<VksSelectionLawunit>(model.Id);
        // curent.ReplacedLawunitId = model.ReplacedLawunitId;
        curent.DateEnd = now;
        curent.DateExpired = now;
        curent.UserExpiredId = userContext.UserId;
        //repo.Update<VksSelectionLawunit>(curent);
        //repo.SaveChanges();

        var selection = repo.AllReadonly<VksSelection>()
                          .Include(x => x.CourtDepartment)
                          .Where(x => x.Id == model.VksSelectionId).FirstOrDefault();
        var replaceLawunitWithRole = repo.AllReadonly<CourtDepartmentLawUnit>()
                                       .Include(x => x.CourtDepartment)
                                       .Include(x => x.LawUnit)
                                       .Where(x => x.LawUnitId == model.ReplacedLawunitId)
                                       .Where(x => x.CourtDepartmentId == selection.CourtDepartmentId || x.CourtDepartmentId == selection.CourtDepartment.ParentId).FirstOrDefault();


        VksSelectionLawunit replacing = new VksSelectionLawunit();

        replacing.LawunitId = model.ReplacedLawunitId;
        replacing.DateStart = now;
        replacing.DateWrt = now;
        replacing.IsUnknownJudge = false;
        replacing.LawunitKey = curent.LawunitKey;
        replacing.LawunitName = replaceLawunitWithRole.LawUnit.FullName;
        replacing.JudgeDepartmentRoleId = replaceLawunitWithRole.JudgeDepartmentRoleId;
        replacing.VksSelectionId = curent.VksSelectionId;

        repo.Add<VksSelectionLawunit>(replacing);
        repo.SaveChanges();
        curent.ReplacedLawunitId = replacing.Id;
        repo.Update<VksSelectionLawunit>(curent);
        repo.SaveChanges();

        var monthSelectionsLawunits = repo.All<VksSelectionMonthLawunit>().Where(x => x.VksSelectionLawunitId == curent.Id).ToList();
        foreach (var lu in monthSelectionsLawunits)
        {
          lu.VksSelectionLawunitId = replacing.Id;
          repo.Update<VksSelectionMonthLawunit>(lu);
          repo.SaveChanges();
        }
        result = true;
      }
      catch (Exception ex)
      {

        throw ex;
      }

      return result;



    }
    public VksSelectionProtocol Save_VksSelection_protocol(VksSelectionProtocol protocol)
    {
      if (protocol.Id > 0)
      { repo.Update(protocol); }
      else
      { repo.Add(protocol); }

      repo.SaveChanges();
      return protocol;
    }

    public VksSelectionProtocol Get_VksSelection_protocol(int id)
    {
      return repo.All<VksSelectionProtocol>()
                      .Include(x => x.UserGenerated)
                      .ThenInclude(x => x.LawUnit)
                      .Include(x => x.UserSigned)
                      .ThenInclude(x => x.LawUnit)
                      .Where(x => x.Id == id).FirstOrDefault();


    }
    public VksSelectionProtocol Get_VksSelection_protocol_byId(int id)
    {
      return repo.All<VksSelectionProtocol>()

                      .Where(x => x.Id == id).FirstOrDefault();


    }
    public bool DeleteUnsignedSelectionProtocols(int selectionId)
    {
      bool result = false;

      try
      {
        var unsignedProtocols = repo.All<VksSelectionProtocol>()
                                   .Where(x => x.VksSelectionId == selectionId)
                                   .Where(x => x.DateSigned == null).ToList();
        repo.DeleteRange<VksSelectionProtocol>(unsignedProtocols);
        repo.SaveChanges();

        result = true;
      }
      catch (Exception ex)
      {

        throw;
      }
      return result;
    }

    public string GetSelectionTitle(int selectionId)
    {
      string result = "";
      try
      {
        var selection = VKSSelection_Get(selectionId);
        if (selection.VksSelectionHeader.PeriodNo>0)
        {
          result = result + ((selection.VksSelectionHeader.PeriodNo == 1) ? "Първо полугодие" : "Второ полугодие") + ", ";
        }
        else
        {
          result = result + "Годишен период,";
        }
  
        result = result + selection.VksSelectionHeader.SelectionYear.ToString() + " г. ";
        result = result + selection.CourtDepartment.Label;

      }
      catch (Exception ex)
      {


      }


      return result;
    }

    public bool SelectionHasSignedProtocol(int selectionId)
    {
      bool result = false;
      try
      {
        result = repo.AllReadonly<VksSelectionProtocol>()
                     .Where(x => x.VksSelectionId == selectionId)
                     .Where(x => x.DateSigned != null).ToList().Count() > 0;


      }
      catch (Exception ex)
      {


      }


      return result;
    }
    public bool HeaderHasSignedProtocol(int selectionHeaderId)
    {
      bool result = false;
      try
      {
        result = repo.AllReadonly<VksSelectionProtocol>()
                      .Include(x => x.VksSelection)
                      .ThenInclude(x => x.VksSelectionHeader)
                     .Where(x => x.VksSelection.VksSelectionHeaderId == selectionHeaderId)
                     .Where(x => x.DateSigned != null).ToList().Count() > 0;


      }
      catch (Exception ex)
      {


      }


      return result;
    }

    public List<VksSessionDayCalendarVM> GetvksSessionDayCalendar(int[] LawunitsInCalendar, int? courtDepartmentId=0)
    {
      DateTime now = DateTime.Now;

      var selection_lawUnits = repo.AllReadonly<VksSelectionLawunit>()
                                 .Where(x => LawunitsInCalendar.Contains(x.LawunitId ?? 0))
                                 .Where(x => x.DateStart <= now)
                                 .Where(x => (x.DateEnd ?? now) >= now)
                                  .Select(x => x.Id).ToArray();
      var lawUnitsOrder = repo.AllReadonly<CourtLawUnitOrder>()
                             .Where(x => x.CourtId == userContext.CourtId)
                             .ToList();

      var sessionDayCalendar = repo.All<VksSelectionMonth>()
                       .Include(x => x.VksSelection)
                       .ThenInclude(x => x.VksSelectionHeader)
                       .Include(x => x.VksSelection)
                       .ThenInclude(x => x.CourtDepartment)
                       .Include(x => x.SelectionMonthLawunit)
                        .ThenInclude(x => x.VksSelectionLawunit)


                        .Where (x=>(x.VksSelection.CourtDepartmentId==courtDepartmentId && courtDepartmentId>0)|| (courtDepartmentId == 0))
                       .Where(x => x.SelectionMonthLawunit.Where(y => selection_lawUnits.Contains(y.VksSelectionLawunitId)).Count() > 0)
                       .Where(x => x.SessionDate >= now).OrderBy(x => x.SessionDate)
                          .Select(x => new VksSessionDayCalendarVM()
                          {
                            Id = x.Id,
                            PeriodNo = x.VksSelection.VksSelectionHeader.PeriodNo,
                            PeriodNoString = (x.VksSelection.VksSelectionHeader.PeriodNo == 1) ? "Първо полугодие" : "Второ полугодие",
                            SelectionYear = x.VksSelection.VksSelectionHeader.SelectionYear,
                            CourtDepartmentId = x.VksSelection.CourtDepartmentId,
                            CourtDepartmentName = x.VksSelection.CourtDepartment.Label,
                            SelectionId = x.VksSelectionId,

                            SelectionDay = x.SelectionDay,
                            SessionDate = x.SessionDate,
                            SelectionMonth = x.SelectionMonth,
                            SelectionMonthString = IntToMonth(x.SelectionMonth),


                            LawUnitsList = x.SelectionMonthLawunit.Select(z => new VksSessionDayLawUnit()
                            {
                              LawUnitID = z.VksSelectionLawunit.LawunitId ?? 0,
                              LawUnitRoleID=z.VksSelectionLawunit.JudgeDepartmentRoleId??0,
                              LawUnitName = z.VksSelectionLawunit.LawUnit.FamilyName,
                              LawUnitFullName= z.VksSelectionLawunit.LawUnit.FullName
                            }).ToList()

                          }).ToList();



      foreach (var selection_month in sessionDayCalendar)

      {

        foreach (var month_lawunit in selection_month.LawUnitsList)
        {
          //Za podrevdane po starshinstvo


          if (month_lawunit.LawUnitRoleID == NomenclatureConstants.JudgeDepartmentRole.Predsedatel)
          {
            //Председателите излизат първи
            month_lawunit.LawUnitOrder = int.MinValue;
            continue;
          }

          //Останалите лица в състава се редят по реда на старшинство
          var _order = lawUnitsOrder.FirstOrDefault(x => x.LawUnitId == month_lawunit.LawUnitID);
          if (_order != null)
          {
            month_lawunit.LawUnitOrder = _order.OrderNumber;
          }
        }

        foreach (var month_lawunit in selection_month.LawUnitsList.OrderBy(x=>x.LawUnitOrder))
        {
          selection_month.SelectionStaff = selection_month.SelectionStaff + month_lawunit.LawUnitFullName + "<br />";

        }


      }
      var selected_sessions = sessionDayCalendar.Select(r => r.SelectionId).ToArray();
      var launits_indirect_in_calendar = repo.All<VksSelectionLawunit>()
                                            .Where(x => x.DateStart <= now)
                                            .Where(x => (x.DateEnd ?? now) >= now)
                                            .Where(x => selected_sessions.Contains(x.VksSelectionId))

                                            .Select(x => x.LawunitId).ToArray();
      var all_lawunits = repo.AllReadonly<CaseLawUnit>()
                  .Where(r => launits_indirect_in_calendar.Contains(r.LawUnitId))
                  .Where(r => r.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                  .Where(r => r.CourtId == userContext.CourtId)
                  .Where(r => r.DateFrom < now)
                   .Where(r => (r.DateTo ?? now) >= now).ToList();
      var all_lawunits_sessions_array = all_lawunits.Select(x => x.CaseSessionId).ToArray();

      var all_lawunits_cases_array = all_lawunits.Select(x => x.CaseId).ToArray();

      var SessionDates = sessionDayCalendar.Select(x => (x.SessionDate ?? DateTime.MinValue).Date).ToArray();
      var all_cases_sessions = repo.AllReadonly<CaseSession>()
                       .Include(x => x.Case)
                       .ThenInclude(x => x.CaseLawUnits)
                       .Where(x => x.CourtId == userContext.CourtId)
                       .Where(x => SessionDates.Contains(x.DateFrom.Date))
                       .Where(x => all_lawunits_sessions_array.Contains(x.Id))
                       .Where(x=>x.SessionStateId!=NomenclatureConstants.SessionState.Cancel)  //Да не се вземат отменените
                        .Where(x => x.SessionStateId != NomenclatureConstants.SessionState.Prenasrocheno)  //Да не се вземат пренасрочените
                         .Where(x => x.DateExpired==null)  //Да не се вземат изтрити
                       .ToList();
      var all_case_id_array = all_cases_sessions.Select(x => x.CaseId).ToArray();

      var all_cases = repo.AllReadonly<Case>()
                       .Include(x => x.CaseLawUnits)
                       .Include(x => x.CaseSessions)

                      .Where(x => all_lawunits_cases_array.Contains(x.Id)).ToList();




      foreach (var selection_month in sessionDayCalendar)

      {
        foreach (var lau_unit in selection_month.LawUnitsList.OrderBy(x=>x.LawUnitOrder))
        {
          //Дела с насрочени заседания на датата
          var current_lawunits_sessions_array = all_lawunits.Where(x => x.LawUnitId == lau_unit.LawUnitID).Select(x => x.CaseSessionId).ToArray();
          var current_lu_case_id_array = all_cases_sessions.Where(x => x.DateFrom.Date == (selection_month.SessionDate ?? DateTime.MinValue).Date)
                                                            .Where(x => current_lawunits_sessions_array.Contains(x.Id)).Select(x => x.CaseId).ToArray();

          var lu_cases = all_cases.Where(x => current_lu_case_id_array.Contains(x.Id)).ToList();
          var lu_cases_nasrocheni_array = lu_cases.Select(x => x.Id).ToArray();
          foreach (var item in lu_cases)
          {


            VksSelectionCase c = new VksSelectionCase();
            c.CaseId = item.Id;
            c.CaseDate = item.RegDate;
            c.CaseNumber = item.RegNumber;
            c.CaseSessionDate = item.CaseSessions.Where(x => x.DateFrom.Date == (selection_month.SessionDate ?? DateTime.MinValue).Date).Select(x => x.DateFrom).FirstOrDefault();
            c.FS = item.ComplexIndexActual;
            c.PS = item.ComplexIndexLegal;
            c.HasSession = true;
            if (item.CaseSessions.Where(x => x.DateFrom.Date == (selection_month.SessionDate ?? DateTime.MinValue).Date).FirstOrDefault().SessionTypeId==3)
            {
              c.SessionTypeString = "ЗЗ";

            }
            else
            {
              c.SessionTypeString = "OЗ";
            }
            lau_unit.CaseList.Add(c);
          }
          //Ненасрочвани дела
          var current_lawunits_case_array = all_lawunits.Where(x => x.LawUnitId == lau_unit.LawUnitID).Select(x => x.CaseId).ToArray();
          current_lawunits_sessions_array = all_lawunits.Where(x => x.LawUnitId == lau_unit.LawUnitID).Select(x => x.CaseSessionId).ToArray();
          current_lu_case_id_array = current_lawunits_case_array.Where(x => !current_lawunits_sessions_array.Contains(x))
                                                            .Select(x => x).ToArray();
          var excluded_case_types = SystemParam_SelectIntValues(NomenclatureConstants.SystemParamName.VKS_CaseType_CalendarExclude);
          lu_cases = all_cases.Where(x => x.CaseSessions.Count() == 0)
                               .Where(x=>!excluded_case_types.Contains(x.CaseTypeId))
                               .Where(x => current_lu_case_id_array.Contains(x.Id)).ToList();

          foreach (var item in lu_cases)
          {


            VksSelectionCase c = new VksSelectionCase();
            c.CaseId = item.Id;
            c.CaseDate = item.RegDate;
            c.CaseNumber = item.RegNumber;
            c.CaseSessionDate = item.CaseSessions.Where(x => x.DateFrom.Date == (selection_month.SessionDate ?? DateTime.MinValue).Date).Select(x => x.DateFrom).FirstOrDefault();
            c.FS = item.ComplexIndexActual;
            c.PS = item.ComplexIndexLegal;
            c.HasSession = false;
            c.SessionTypeString = "";
            lau_unit.CaseList.Add(c);
            
            selection_month.NoSessionCaseList.Add(c);

          }
        }
      }

      foreach (var  item in sessionDayCalendar)
      {
        item.LawUnitsList = item.LawUnitsList.OrderBy(x => x.LawUnitOrder??0).ToList();
      }


      return sessionDayCalendar;


    }

    public int GetCourtDepartmentIdByCaseId(int caseId)
    {
      DateTime now = DateTime.Now;
      int result = 0;
      try
      {
       var caseLu = repo.AllReadonly<CaseLawUnit>()
                     
                     .Where(r => r.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                      .Where(r => r.CourtId == userContext.CourtId)
                       .Where(r => r.DateFrom < now)
                       .Where(r => (r.DateTo ?? now) >= now).FirstOrDefault();

        if (caseLu!=null)
        {
          var courtDepartmetLu = repo.AllReadonly<CourtDepartmentLawUnit>()
                            .Where(x => x.LawUnitId == caseLu.LawUnitId)
                            .Where(x=>x.CourtDepartment.DepartmentTypeId==NomenclatureConstants.DepartmentType.Otdelenie)
                            .Where(x => x.CourtDepartment.CourtId == userContext.CourtId).FirstOrDefault();

          if (courtDepartmetLu!=null)
          {
            result = courtDepartmetLu.CourtDepartmentId;
          }
        }


      }
      catch (Exception ex)
      {


      }


      return result;
    }
    public int GetLawUnitIdByCaseId(int caseId)
    {
      DateTime now = DateTime.Now;
      int result = 0;
      try
      {
        var caseLu = repo.AllReadonly<CaseLawUnit>()

                      .Where(r => r.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                       .Where(r => r.CaseId == caseId)
                        .Where(r => r.DateFrom < now)
                        .Where(r => (r.DateTo ?? now) >= now).FirstOrDefault();

        if (caseLu != null)
        {
          result = caseLu.LawUnitId;
        }


      }
      catch (Exception ex)
      {


      }


      return result;
    }

    public int GetUserOtdelenieID(int lawunitId)
    {
      DateTime now = DateTime.Now;
      int result = -1;
      try
      {
        var ob = repo.AllReadonly<CourtDepartmentLawUnit>()
                                          .Include(x => x.LawUnit)
                                           .Include(x => x.CourtDepartment)
                                          .Where(x => x.LawUnitId == lawunitId)
                                           .Where(x => x.CourtDepartment.DepartmentTypeId == NomenclatureConstants.DepartmentType.Otdelenie)
                                          .Where(x => x.DateFrom < now)
                                          .Where(x=>x.CourtDepartment.Court.CourtTypeId==NomenclatureConstants.CourtType.VKS)
                                          .Where(x => (x.DateTo ?? now) >= now).FirstOrDefault();
        if (ob != null)
        { result = ob.CourtDepartmentId; }
      }
      catch (Exception)
      {

      }

      return result;
    }
    public bool CanEditCurrentOtdelenie(int lawunitId, int OtdelenieID)
    {
      bool result = true;
      int luOydelenieID = GetUserOtdelenieID(lawunitId);
      if (luOydelenieID > 0)
      {
        if (luOydelenieID!= OtdelenieID)
        {
          result = false;
        }
      }
        return result;
    }

    public int GetCourtDepartmentIdFromSelection(int selectionId)
    {
      int result = 0;

      try
      {
        result = repo.AllReadonly<VksSelection>().Where(x => x.Id == selectionId).FirstOrDefault().CourtDepartmentId;

      }
      catch (Exception)
      {

     
      }
     
      return result; }
  }
}
