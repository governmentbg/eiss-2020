using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class LawUnitVM
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Department { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string[] CourtList { get; set; }
    }

    public class LawUnitFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Специалност")]
        public int SpecialityId { get; set; }

        [Display(Name = "Имена")]
        public string FullName { get; set; }

        [Display(Name = "Показване на неназначени лица")]
        public bool ShowFree { get; set; }

        [Display(Name = "Назначен/командирован в съд")]
        public int? CourtId { get; set; }
    }

    public class JuryYearDays
    {
        public int Id { get; set; }
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }
        [Display(Name = "Специалност")]
        public string SpecialityName { get; set; }

        public int SpecialityId { get; set; }

        [Display(Name = "Имена")]
        public string FullName { get; set; }
        [Display(Name = "Брой участия(дни)")]
        public int? DaysCount { get; set; }
        [Display(Name = "Брой насрочени (дни)")]
        public int? DaysCountAppointed { get; set; }
        [Display(Name = "Общо (дни)")]
        public int? DaysCountTotal
        {
            get { return (this.DaysCount ?? 0) + (this.DaysCountAppointed ?? 0); }

        }
        public int? SessionStateID { get; set; }
    }
}

