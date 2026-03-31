using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.RegixReport
{
    public enum IdentifierTypeCriminalRecordsReportVM
    {

        /// <remarks/>
        [Display(Name = "ЕГН")]
        ЕГН,

        /// <remarks/>
        [Display(Name = "ЛНЧ")]
        ЛНЧ,
    }
    public class RegixCriminalRecordsReportVM
    {
        public RegixReportVM Report { get; set; }

        public RegixCriminalRecordsReportFilterVM Filter { get; set; }

        public RegixCriminalRecordsReportResponseVM Response { get; set; }

        public RegixCriminalRecordsReportVM()
        {
            Report = new RegixReportVM();
            Filter = new RegixCriminalRecordsReportFilterVM();
            Response = new RegixCriminalRecordsReportResponseVM();
        }
    }

    public class RegixCriminalRecordsReportFilterVM
    {
        [Display(Name = "ЕГН/ЛНЧ")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string IdentifierFilter { get; set; }

        [Display(Name = "Тип на идентификатор")]
        [Range(0, int.MaxValue, ErrorMessage = "Изберете")]
        public int IdentifierTypeFilter { get; set; }
    }

    public class RegixCriminalRecordsReportResponseVM
    {
        public RegixCriminalRecordsReportPersonDataVM PersonData { get; set; }

        public List<RegixCriminalRecordsReportBulletinTypeVM> BulletinTypes { get; set; }

        [Display(Name = "Рег. номер:")]
        public string RegistrationNumber { get; set; }

        [Display(Name = "Валидно от:")]
        public string ValidFrom { get; set; }

        [Display(Name = "Валидно до:")]
        public string ValidTo { get; set; }

        public string MessageError { get; set; }

        public RegixCriminalRecordsReportResponseVM()
        {
            BulletinTypes = new List<RegixCriminalRecordsReportBulletinTypeVM>();
        }
    }

    public class RegixCriminalRecordsReportPersonDataVM
    {        
        [Display(Name = "Имена на кирилица:")]
        public string NamesBg { get; set; }

        [Display(Name = "Имена на латиница:")]
        public string NamesEn { get; set; }

        [Display(Name = "Пол:")]
        public string Gender { get; set; }

        [Display(Name = "Идентификатори на лицето:")]
        public string IdentityNumber { get; set; }

        [Display(Name = "Дата на раждане:")]
        public string BirthDate { get; set; }

        [Display(Name = "Място на раждане:")]
        public string BirthPlace { get; set; }

        [Display(Name = "Гражданство:")]
        public string PersonNationality { get; set; }

        [Display(Name = "Данни за документ за самоличност:")]
        public string PersonIdentificationDocument { get; set; }

        [Display(Name = "Имена на майка:")]
        public string MotherNames { get; set; }

        [Display(Name = "Имена на майка на латиница:")]
        public string MotherNamesEn { get; set; }

        [Display(Name = "Имена на баща:")]
        public string FatherNames { get; set; }

        [Display(Name = "Имена на баща на латиница:")]
        public string FatherNamesEn { get; set; }

        public List<RegixCriminalRecordsReportPrevNameVM> PrevNames { get; set; }

        public RegixCriminalRecordsReportPersonDataVM()
        {
            PrevNames = new List<RegixCriminalRecordsReportPrevNameVM>();
        }
    }

    public class RegixCriminalRecordsReportPrevNameVM
    {
        [Display(Name = "Имена:")]
        public string Names { get; set; }

        [Display(Name = "Вид на името:")]
        public string NameType { get; set; }

        [Display(Name = "Дата на раждане:")]
        public string BirthDate { get; set; }

        [Display(Name = "Място на раждане:")]
        public string BirthPlace { get; set; }

        [Display(Name = "Пол:")]
        public string Gender { get; set; }

        [Display(Name = "Идентификатори на лицето:")]
        public string IdentityNumber { get; set; }
    }

    public class RegixCriminalRecordsReportConvictionVM
    {
        [Display(Name = "Данни за акта:")]
        public string Decision { get; set; }

        [Display(Name = "Данни за делото:")]
        public string CriminalCase { get; set; }

        [Display(Name = "Извлечение от акта на съда:")]
        public string ConvictionRemarks { get; set; }

        [Display(Name = "Престъпление:")]
        public string[] ConvictionOffence { get; set; }

        [Display(Name = "Не се наказва:")]
        public string WithoutSanction { get; set; }

        [Display(Name = "Наказания, за специфични случаи на употреба:")]
        public string[] ConvictionSanction { get; set; }

        [Display(Name = "Постановено изтърпяване на предходна условна присъда:")]
        public string ServingPrevSuspendedSentence { get; set; }

        [Display(Name = "Допълнителни сведения:")]
        public string[] ConvictionDecisions { get; set; }

        [Display(Name = "ЕИСПП номер на наказателно производство:")]
        public string EisppNumber { get; set; }
    }

    public class RegixCriminalRecordsReportBulletinTypeVM
    {
        [Display(Name = "Вид на бюлетин:")]
        public string BulletinType { get; set; }

        public RegixCriminalRecordsReportPersonDataVM Person { get; set; }

        public RegixCriminalRecordsReportConvictionVM Conviction { get; set; }

        [Display(Name = "Данни при съставяне на бюлетина:")]
        public string IssuerData { get; set; }

        [Display(Name = "анни при регистрация на бюлетина в БС по месторождение на лицето:")]
        public string RegistrationData { get; set; }
    }
}
