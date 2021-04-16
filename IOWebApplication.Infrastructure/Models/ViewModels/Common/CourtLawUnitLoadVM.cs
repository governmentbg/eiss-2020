using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
  public class CourtLawUnitLoadVM
  {
    public string FullName { get; set; }
    public int Id { get; set; }

    public decimal LoadIndex { get; set; }

    public DateTime DateFrom { get; set; }

    public DateTime? DateTo { get; set; }
  }
}
