using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CourtLawUnitVM
    {
        public int Id { get; set; }
        public int OrderNumber { get; set; }
        public int LawUnitId { get; set; }

        public int LawUnitTypeId { get; set; }
        public string LawUnitTypeLabel { get; set; }
        public string CourtLabel { get; set; }
        public string LawUnitName { get; set; }
        public string CourtOrganizationName { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? MandateDateTo { get; set; }
        public string LawUnitPositionName { get; set; }
        public string PeriodTypeLabel { get; set; }
        public int PeriodTypeId { get; set; }

        public int RowNo { get; set; }
    }

    public class CourtLawUnitFilter
    {
        [Display(Name = "Длъжностно лице")]
        public int LawUnitId { get; set; }

        [Display(Name = "Вид")]
        public int PeriodTypeId { get; set; }

        [Required]
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Required]
        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Заседател")]
        public int? LawUnitJuryId { get; set; }

        [Display(Name = "Имена")]
        public string Fullname { get; set; }
        public int LawUnitTypeId { get; set; }
    }

    public class CourtLawunitOrderComboVM
    {
        public int CurrentRowNo { get; set; }
        [Display(Name = "Съдия")]
        public string CurrentLawunit { get; set; }
        [Display(Name = "Нова позиция в старшинството")]
        public int NewRowNo { get; set; }
    }
}
