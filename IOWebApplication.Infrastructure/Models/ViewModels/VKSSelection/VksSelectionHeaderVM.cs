using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using IOWebApplication.Infrastructure.Data.Models.VKS;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
  /// <summary>
  /// Полугодишен избор на съдии за ВКС VM
  /// </summary>

  public class VksSelectionHeaderVM
  {
    public int? Id { get; set; }

    [Display(Name = "Колегия")]
    public int KolegiaId { get; set; }

    [Display(Name = "Колегия")]
    public string KolegiaName { get; set; }

    [Display(Name = "Година")]
    public int SelectionYear { get; set; }

    /// <summary>
    /// Полугодие: 1,2
    /// </summary>
    [Display(Name = "Полугодие")]
    public int PeriodNo { get; set; }

    [Display(Name = "Период")]
    public string PeriodNoString { get; set; }

    [Display(Name = "Статус")]
    public int? VksSelectionStateId { get; set; }
    [Display(Name = "Статус")]
    public string VksSelectionStateName { get; set; }
    public string Months { get; set; }

    public virtual List<CheckListWithDdlVM> YearMonths { get; set; }


    public virtual ICollection<VksSelectionVM> Selections { get; set; }



    public VksSelectionHeaderVM()
    { }
    public void AddMonthsNomenclature(int period)
    {
      this.YearMonths = new List<CheckListWithDdlVM>();
 
      if (period == 1)

      {
        this.YearMonths.Add(new CheckListWithDdlVM { Value = "1", Label = "Януари", Checked = true,DdlValue=-1});
        this.YearMonths.Add(new CheckListWithDdlVM { Value = "2", Label = "Февруари", Checked = true, DdlValue = -1 });
        this.YearMonths.Add(new CheckListWithDdlVM { Value = "3", Label = "Март",  Checked = true, DdlValue = -1 });
        this.YearMonths.Add(new CheckListWithDdlVM { Value = "4", Label = "Април", Checked = true, DdlValue = -1 });
        this.YearMonths.Add(new CheckListWithDdlVM { Value = "5", Label = "Май",  Checked = true, DdlValue = -1 });
        this.YearMonths.Add(new CheckListWithDdlVM { Value = "6", Label = "Юни", Checked = true, DdlValue = -1 });

      }
      if (period == 2 )

      {
        this.YearMonths.Add(new CheckListWithDdlVM { Value = "7", Label = "Юли",  Checked = true, DdlValue = -1 });
        this.YearMonths.Add(new CheckListWithDdlVM { Value = "8", Label = "Август",  Checked = true, DdlValue = -1 });
        this.YearMonths.Add(new CheckListWithDdlVM { Value =" 9", Label = "Септември",  Checked = true, DdlValue = -1 });
        this.YearMonths.Add(new CheckListWithDdlVM { Value = "10", Label = "Октомври",  Checked = true, DdlValue = -1 });
        this.YearMonths.Add(new CheckListWithDdlVM { Value = "11", Label = "Новмври",  Checked = true, DdlValue = -1 });
        this.YearMonths.Add(new CheckListWithDdlVM { Value = "12", Label = "Декември",  Checked = true, DdlValue = -1 });


       
      }

    }

  } 

}
