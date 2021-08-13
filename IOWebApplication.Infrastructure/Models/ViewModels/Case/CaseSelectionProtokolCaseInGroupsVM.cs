using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
  public class CaseSelectionProtokolCaseInGroupsVM
  {
    public int CourtGroupId { get; set; }
    public string CourtGroupName { get; set; }
    public string LawunitFullName { get; set; }


    public decimal TotalCases { get; set; }
    public decimal RealCases { get; set; }
    public decimal AverageCases { get; set; }
    public int CourtId { get; set; }

  }

   
}
