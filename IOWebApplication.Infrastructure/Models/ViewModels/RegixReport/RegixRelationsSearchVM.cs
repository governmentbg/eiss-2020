using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.RegixReport
{
    public class RegixRelationsSearchVM
    {
        public RegixReportVM Report { get; set; }

        public RegixRelationsSearchFilterVM RelationsSearchFilter { get; set; }

        public List<RegixRelationsSearchResponseVM> RelationsSearchResponse { get; set; }

        public RegixRelationsSearchVM()
        {
            Report = new RegixReportVM();
            RelationsSearchFilter = new RegixRelationsSearchFilterVM();
            RelationsSearchResponse = new List<RegixRelationsSearchResponseVM>();
        }
    }

    public class RegixRelationsSearchFilterVM
    {
        [Display(Name = "ЕГН")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string IdentifierFilter { get; set; }
    }

    public class RegixRelationsSearchResponseVM
    {
        [Display(Name = "Вид родство:")]
        public string RelationTypeLabel { get; set; }

        [Display(Name = "ЕГН:")]
        public string Identifier { get; set; }

        [Display(Name = "Дата на раждане:")]
        public string BirthDate { get; set; }

        [Display(Name = "Собствено име:")]
        public string FirstName { get; set; }

        [Display(Name = "Бащино име:")]
        public string SurName { get; set; }

        [Display(Name = "Фамилно име:")]
        public string FamilyName { get; set; }

        [Display(Name = "Пол:")]
        public string GenderCode { get; set; }

        [Display(Name = "Гражданство:")]
        public string NationalityName { get; set; }

        [Display(Name = "Второ гражданство:")]
        public string NationalityName2 { get; set; }

        [Display(Name = "Дата на смърт:")]
        public string DeathDate { get; set; }
    }
}
