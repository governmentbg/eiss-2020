using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSelectionProtokoLUGrouplFilterVM
  {
    [Display(Name = "Група")]
    public int GroupId { get; set; }
    [Display(Name = "Съдия")]
    public int LawUnitID { get; set; }

       
    }
}
