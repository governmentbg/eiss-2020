using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
  /// <summary>
  /// Полугодишен избор на съдии за ВКС VM
  /// </summary>

  public class VksSelectionVM
  {

    public int ? Id { get; set; }
    [Display(Name = "Отделение")]
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
    [Display(Name = "Месеци")]
    public string MonthString { get; set; }
    public virtual List<CheckListVM> YearMonths { get; set; }
    public virtual List<CheckListVM> ChairmanInMonth { get; set; }

    public VksSelectionVM()
    { }
    public void AddMonthsNomenclature(int period)
    {
      this.YearMonths = new List<CheckListVM>();
      this.ChairmanInMonth = new List<CheckListVM>();
      if (period == 1)

      {
        this.YearMonths.Add(new CheckListVM { Value = "1", Label = "Януари", Checked = true });
        this.YearMonths.Add(new CheckListVM { Value = "2", Label = "Февруари", Checked = true });
        this.YearMonths.Add(new CheckListVM { Value = "3", Label = "Март",  Checked = true });
        this.YearMonths.Add(new CheckListVM { Value = "4", Label = "Априли", Checked = true });
        this.YearMonths.Add(new CheckListVM { Value = "5", Label = "Май",  Checked = true });
        this.YearMonths.Add(new CheckListVM { Value = "6", Label = "Юни", Checked = true });


        this.ChairmanInMonth.Add(new CheckListVM { Value = "1", Label = "Я", Checked = false });
        this.ChairmanInMonth.Add(new CheckListVM { Value = "2", Label = "Ф", Checked = false });
        this.ChairmanInMonth.Add(new CheckListVM { Value = "3", Label = "М", Checked = false });
        this.ChairmanInMonth.Add(new CheckListVM { Value = "4", Label = "А", Checked = false });
        this.ChairmanInMonth.Add(new CheckListVM { Value = "5", Label = "М", Checked = false });
        this.ChairmanInMonth.Add(new CheckListVM { Value = "6", Label = "Ю", Checked = false });
      }
      if (period == 2 )

      {
        this.YearMonths.Add(new CheckListVM { Value = "7", Label = "Юли",  Checked = true });
        this.YearMonths.Add(new CheckListVM { Value = "8", Label = "Август",  Checked = true });
        this.YearMonths.Add(new CheckListVM { Value =" 9", Label = "Септември",  Checked = true });
        this.YearMonths.Add(new CheckListVM { Value = "10", Label = "Октомври",  Checked = true });
        this.YearMonths.Add(new CheckListVM { Value = "11", Label = "Ноeмври",  Checked = true });
        this.YearMonths.Add(new CheckListVM { Value = "12", Label = "Декември",  Checked = true });


        this.ChairmanInMonth.Add(new CheckListVM { Value = "7", Label = "Ю", Checked = false });
        this.ChairmanInMonth.Add(new CheckListVM { Value = "8", Label = "А", Checked = false });
        this.ChairmanInMonth.Add(new CheckListVM { Value = " 9", Label = "С", Checked = false });
        this.ChairmanInMonth.Add(new CheckListVM { Value = "10", Label = "О", Checked = false });
        this.ChairmanInMonth.Add(new CheckListVM { Value = "11", Label = "Н", Checked = false });
        this.ChairmanInMonth.Add(new CheckListVM { Value = "12", Label = "Д", Checked = false });
      }

    }

  } 

}
