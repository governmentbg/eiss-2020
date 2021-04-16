using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
  public class CaseSelectionProtokolLawUnitVM
  {
    public int Id { get; set; }

    public int Index { get; set; }

    public bool IsLoaded { get; set; }

    public int LawUnitId { get; set; }

    public string LawUnitFullName { get; set; }

    public int LoadIndex { get; set; }

    public int StateId { get; set; }

    public int CaseCount { get; set; }

    public string Description { get; set; }

    public decimal Koef { get; set; }
    public decimal KoefNormalized { get; set; }
    public decimal TotalCaseCount { get; set; }

 

    public bool SelectedFromCaseGroup { get; set; }

    public int? CaseGroupId { get; set; }
    public bool EnableState { get; set; }
    public string LawUnitTypeId { get; set; }

    public decimal CasesCountIfWorkAllPeriodInGroup { get; set; }
    public bool ExcludeByBigDeviation { get; set; }

    public string GetPrefix
    {
      get
      {
        return nameof(CaseSelectionProtokolVM.LawUnits);
      }
    }
    public string GetPath
    {
      get
      {
        return string.Format("{0}[{1}]", this.GetPrefix, Index);
      }
    }
  }
}
