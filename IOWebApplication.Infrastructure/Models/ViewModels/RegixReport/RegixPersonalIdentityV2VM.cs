using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.RegixReport
{
    public class RegixPersonalIdentityV2VM
    {
        public RegixReportVM Report { get; set; }

        public RegixPersonalIdentityV2FilterVM PersonalIdentityV2Filter { get; set; }

        public RegixPersonalIdentityV2ResponseVM PersonalIdentityV2Response { get; set; }

        public RegixPersonalIdentityV2VM()
        {
            Report = new RegixReportVM();
            PersonalIdentityV2Filter = new RegixPersonalIdentityV2FilterVM();
            PersonalIdentityV2Response = new RegixPersonalIdentityV2ResponseVM();
        }
    }

    public class RegixPersonalIdentityV2FilterVM
    {
        [Display(Name = "ЕГН/ЛНЧ")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string EGN { get; set; }

        [Display(Name = "Номер на документ")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string IdentityDocumentNumber { get; set; }

    }

    public class RegixPersonalIdentityV2ResponseVM
    {
        [Display(Name = "Описание на грешка:")]
        public string InfoError { get; set; }

        [Display(Name = "ЕГН:")]
        public string EGN { get; set; }

        [Display(Name = "Имена:")]
        public string PersonNames { get; set; }

        [Display(Name = "Имена на латиница:")]
        public string PersonNamesLatin { get; set; }

        [Display(Name = "Общи ограничения за СУМПС:")]
        public string DLCommonRestrictions { get; set; }


        [Display(Name = "ЕГН:")]
        public string ForeignPIN { get; set; }

        [Display(Name = "Личен номер на чужденец:")]
        public string ForeignPN { get; set; }

        [Display(Name = "Имена:")]
        public string ForeignPersonNames { get; set; }

        [Display(Name = "Имена на латиница:")]
        public string ForeignPersonNamesLatin { get; set; }

        [Display(Name = "Гражданство:")]
        public string ForeignNationality { get; set; }

        [Display(Name = "Пол:")]
        public string ForeignGender { get; set; }

        [Display(Name = "Дата на раждане:")]
        public string ForeignBirthDate { get; set; }


        [Display(Name = "Забележки в док.Разрешение за преб.:")]
        public string RPRemarks { get; set; }

        [Display(Name = "Тип пребиваване в док. Разрешение за пребиваване:")]
        public string RPTypeofPermit { get; set; }

        [Display(Name = "Вид документ:")]
        public string DocumentType { get; set; }

        [Display(Name = "Актуален статус на документ:")]
        public string DocumentActualStatus { get; set; }

        [Display(Name = "Дата на актуалния статус:")]
        public string ActualStatusDate { get; set; }

        [Display(Name = "Причина за статус на документ:")]
        public string DocumentStatusReason { get; set; }

        [Display(Name = "Номер на документ за самоличност:")]
        public string IdentityDocumentNumber { get; set; }

        [Display(Name = "Дата на издаване:")]
        public string IssueDate { get; set; }

        [Display(Name = "Място на издаване:")]
        public string IssuerPlace { get; set; }

        [Display(Name = "Издаващ орган:")]
        public string IssuerName { get; set; }

        [Display(Name = "Дата на валидност:")]
        public string ValidDate { get; set; }

        [Display(Name = "Дата на раждане:")]
        public string BirthDate { get; set; }

        [Display(Name = "Място на раждане:")]
        public string BirthPlace { get; set; }

        [Display(Name = "Пол:")]
        public string GenderName { get; set; }

        [Display(Name = "Гражданство:")]
        public string Nationality { get; set; }

        [Display(Name = "Постоянен адрес:")]
        public string PermanentAddress { get; set; }

        [Display(Name = "Височина в см.:")]
        public string Height { get; set; }

        [Display(Name = "Цвят на очите:")]
        public string EyesColor { get; set; }

        public List<RegixPersonalIdentityV2CategоriesResponseVM> PersonalIdentityV2CategоriesResponse { get; set; }

        public RegixPersonalIdentityV2ResponseVM()
        {
            PersonalIdentityV2CategоriesResponse = new List<RegixPersonalIdentityV2CategоriesResponseVM>();
        }
    }

    public class RegixPersonalIdentityV2CategоriesResponseVM
    {
        [Display(Name = "Категория:")]
        public string Category { get; set; }

        [Display(Name = "От дата:")]
        public string DateCategory { get; set; }

        [Display(Name = "До дата:")]
        public string EndDateCategory { get; set; }

        [Display(Name = "Ограничения:")]
        public string Restrictions { get; set; }

    }
}
