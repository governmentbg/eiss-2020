using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
  /// <summary>
  /// Избор за един месец
  /// </summary>

  public class VksSelectionCalendarVM
  {
    public int Id { get; set; }
    public int CourtDepartmentId { get; set; }
    [Display(Name = "Отделение")]
    public string CourtDepartmentName { get; set; }
    [Display(Name = "Година")]
    public int SelectionYear { get; set; }
    /// <summary>
    /// Полугодие: 1,2
    [Display(Name = "Полугодие")]
    public int PeriodNo { get; set; }

    [Display(Name = "Период")]
    public string PeriodNoString { get; set; }
    public string SignJudgeName{ get; set; }

  

    public virtual List<VksSelectionOneMonthListVM> MonthList { get; set; }

    public VksSelectionCalendarVM()
    { MonthList = new List<VksSelectionOneMonthListVM>(); }
  }
  public class VksSelectionOneMonthListVM
  {


    public int SelectionMonth { get; set; }

    [Display(Name = "Месец")]
    public string SelectionMonthString { get; set; }

    public virtual List<VksMonthSessions> MonthSessions { get; set; }
    public VksSelectionOneMonthListVM()
    { MonthSessions = new List<VksMonthSessions>(); }




  }
  public class VksMonthSessions
  {
    public int Id { get; set; }
    public int SelectionDay { get; set; }

    public string SelectionStaff { get; set; }
    public string SelectionStaffShortNames { get; set; }
    public DateTime? SessionDate { get; set; }

    public List<CheckListVM> LawunitsList { get; set; }
}

} 


