using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionActELSprVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }

        [Display(Name = "Номер")]
        public string RegNumber { get; set; }

        [Display(Name = "Дата")]
        public DateTime RegDate { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; }

        [Display(Name = "Кредитор")]
        public string LeftSide { get; set; }

        [Display(Name = "Длъжник")]
        public string RightSide { get; set; }

        [Display(Name = "Тип")]
        public string ActKindName { get; set; }
    }

    public class CaseSessionActELSprFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Номер")]
        public string RegNumber { get; set; }

        [Display(Name = "Кредитор")]
        public string LeftSide { get; set; }

        [Display(Name = "Длъжник")]
        public string RightSide { get; set; }

        [Display(Name = "Тип")]
        public int ActKindId { get; set; }
    }
}
