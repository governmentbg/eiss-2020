using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
  public class CaseSelectionProtokolLawUnitPreviewVM
  {
    public int Id { get; set; }

    public int LawUnitId { get; set; }
    public string LawUnitFullName { get; set; }

    public int LoadIndex { get; set; }

    public int StateId { get; set; }

    public int CaseCount { get; set; }

    public decimal? CaseCourtTotalCount { get; set; }

    public string Description { get; set; }


    public bool SelectedFromCaseGroup { get; set; }


    public int? CaseGroupId { get; set; }

    public DateTime? FromDateInGROUP { get; set; }

  }
}
