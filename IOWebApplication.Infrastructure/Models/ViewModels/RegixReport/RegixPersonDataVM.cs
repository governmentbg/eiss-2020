using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.RegixReport
{
    public class RegixPersonDataVM
    {
        public RegixReportVM Report { get; set; }

        public RegixPersonDataFilterVM PersonDataFilter { get; set; }

        public RegixPersonDataResponseVM PersonDataResponse { get; set; }

        public RegixPersonDataVM()
        {
            Report = new RegixReportVM();
            PersonDataFilter = new RegixPersonDataFilterVM();
            PersonDataResponse = new RegixPersonDataResponseVM();
        }
    }

    public class RegixPersonDataFilterVM 
    {
        [Display(Name = "ЕГН")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string EgnFilter { get; set; }

        [Display(Name = "Имена")]
        public bool PersonNamesCheck { get; set; }

        [Display(Name = "Псевдоним")]
        public bool AliasCheck { get; set; }

        [Display(Name = "Имена на латиница")]
        public bool LatinNamesCheck { get; set; }

        [Display(Name = "Други имена в чужбина")]
        public bool ForeignNamesCheck { get; set; }

        [Display(Name = "Пол")]
        public bool GenderCheck { get; set; }

        [Display(Name = "ЕГН")]
        public bool EgnCheck { get; set; }

        [Display(Name = "Дата на раждане")]
        public bool BirthDateCheck { get; set; }

        [Display(Name = "Място на раждане")]
        public bool PlaceBirthCheck { get; set; }

        [Display(Name = "Гражданство")]
        public bool NationalityCheck { get; set; }

        [Display(Name = "Дата на смърт")]
        public bool DeathDateCheck { get; set; }
    }

    public class RegixPersonDataResponseVM
    {
        [Display(Name = "Собствено име:")]
        public string PersonNamesFirstName { get; set; }

        [Display(Name = "Бащино име:")]
        public string PersonNamesSurName { get; set; }

        [Display(Name = "Фамилно име:")]
        public string PersonNamesFamilyName { get; set; }

        [Display(Name = "Псевдоним:")]
        public string Alias { get; set; }

        [Display(Name = "Собствено име латиница:")]
        public string LatinNamesFirstName { get; set; }

        [Display(Name = "Бащино име латиница:")]
        public string LatinNamesSurName { get; set; }

        [Display(Name = "Фамилно име латиница:")]
        public string LatinNamesFamilyName { get; set; }

        [Display(Name = "Собствено име в чужбина:")]
        public string ForeignNamesFirstName { get; set; }

        [Display(Name = "Бащино име в чужбина:")]
        public string ForeignNamesSurName { get; set; }

        [Display(Name = "Фамилно име в чужбина:")]
        public string ForeignNamesFamilyName { get; set; }

        [Display(Name = "Пол:")]
        public string GenderName { get; set; }

        [Display(Name = "ЕГН:")]
        public string Egn { get; set; }

        [Display(Name = "Дата на раждане:")]
        public string BirthDate { get; set; }

        [Display(Name = "Място на раждане:")]
        public string PlaceBirth { get; set; }

        [Display(Name = "Код на гражданство:")]
        public string NationalityCode { get; set; }

        [Display(Name = "Гражданство:")]
        public string NationalityName { get; set; }

        [Display(Name = "Код на второ гражданство:")]
        public string NationalityCode2 { get; set; }

        [Display(Name = "Второ гражданство:")]
        public string NationalityName2 { get; set; }

        [Display(Name = "Дата на смърт:")]
        public string DeathDate { get; set; }

        [Display(Name = "Име:")]
        public string PersonFullName {
            get 
            {
                return (this.PersonNamesFirstName ?? "") + " " + (this.PersonNamesSurName ?? "") + " " + (this.PersonNamesFamilyName ?? "");
            }
        }


    }
}
