using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
  public class CaseSelectionProtocolLoadByGroupVM
  {
    public CheckListVM [] CaseGroups { get; set; }
    public string IdStr { get; set; }
    public int CaseId { get; set; }
    public int judgeRoleId { get; set; }
  }
}
