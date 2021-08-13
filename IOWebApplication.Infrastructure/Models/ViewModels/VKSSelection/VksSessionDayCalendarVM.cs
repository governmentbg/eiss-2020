using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
  /// <summary>
  /// ДАТА За насрочване да дела със списък на състава и техните дела за този ден
  /// </summary>

  public class VksSessionDayCalendarVM
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
    public int SelectionId { get; set; }

    [Display(Name = "Период")]
    public string PeriodNoString { get; set; }
    public int SelectionMonth { get; set; }

    [Display(Name = "Месец")]
    public string SelectionMonthString { get; set; }

    public int SelectionDay { get; set; }

    public string SelectionStaff { get; set; }
    public string SelectionStaffShortNames { get; set; }
    public DateTime? SessionDate { get; set; }

    public virtual List<VksSessionDayLawUnit> LawUnitsList { get; set; }

    public virtual List<VksSelectionCase> NoSessionCaseList { get; set; }
    public VksSessionDayCalendarVM()
    { LawUnitsList = new List<VksSessionDayLawUnit>();
      NoSessionCaseList= new List<VksSelectionCase>();
    }



  }
  public class VksSessionDayLawUnit
  {
    public int LawUnitID { get; set; }

    public int LawUnitRoleID { get; set; }
    public int? LawUnitOrder { get; set; }
    public string LawUnitName { get; set; }
    public string LawUnitFullName { get; set; }


    public virtual List<VksSelectionCase> CaseList{ get; set; }
    public VksSessionDayLawUnit()
    { CaseList = new List<VksSelectionCase>(); }




  }
  public class VksSelectionCase
  { 
    public int CaseId { get; set; }
    public string CaseNumber { get; set; }
    public DateTime CaseDate { get; set; }
    public DateTime? CaseSessionDate { get; set; }
    public int? FS { get; set; }
    public int? PS { get; set; }

    public bool HasSession { get; set; }

    public string SessionTypeString { get; set; }

  }

} 


