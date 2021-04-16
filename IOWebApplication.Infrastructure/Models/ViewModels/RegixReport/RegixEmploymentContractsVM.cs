using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.RegixReport
{
    public enum EikTypeTypeEmploymentContractsVM
    {

        /// <remarks/>
        [Display(Name = "ЕИК")]
        Bulstat,

        /// <remarks/>
        [Display(Name = "ЕГН")]
        EGN,

        /// <remarks/>
        [Display(Name = "ЛНЧ")]
        LNC,

        /// <remarks/>
        [Display(Name = "Служебен номер")]
        SystemNo,

        /// <remarks/>
        [Display(Name = "Булстат")]
        BulstatCL,
    }

    public enum ContractsFilterTypeEmploymentContractsVM
    {

        /// <remarks/>
        [Display(Name = "Всички договори")]
        All,

        /// <remarks/>
        [Display(Name = "Действащи договори")]
        Active,
    }

    public class RegixEmploymentContractsVM
    {
        public RegixReportVM Report { get; set; }


        public RegixEmploymentContractsFilterVM EmploymentContractsFilter { get; set; }

        public List<RegixEmploymentContractsResponseVM> EmploymentContractsResponse { get; set; }

        public RegixEmploymentContractsVM()
        {
            Report = new RegixReportVM();
            EmploymentContractsFilter = new RegixEmploymentContractsFilterVM();
            EmploymentContractsResponse = new List<RegixEmploymentContractsResponseVM>();
        }
    }

    public class RegixEmploymentContractsFilterVM
    {
        [Display(Name = "Идентификатор")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string IdentityFilter { get; set; }

        [Display(Name = "Тип идентификатор")]
        [Range(0, int.MaxValue, ErrorMessage = "Изберете")]
        public int EikTypeId { get; set; }

        [Display(Name = "Статус на данните")]
        [Range(0, int.MaxValue, ErrorMessage = "Изберете")]
        public int ContractsFilterTypeId { get; set; }        
    }

    public class RegixEmploymentContractsResponseVM
    {
        [Display(Name = "ЕГН/ЕИК/Сл.номер/БУЛСТАТ:")]
        public string ContractorBulstat { get; set; }

        [Display(Name = "Наименование на работодател:")]
        public string ContractorName { get; set; }

        [Display(Name = "ЕГН:")]
        public string IndividualEIK { get; set; }

        [Display(Name = "Имена на лицето:")]
        public string IndividualNames { get; set; }

        [Display(Name = "Дата на сключване:")]
        public string StartDate { get; set; }

        [Display(Name = "Дата на последно допълнително споразумение/Промяна на работно място:")]
        public string LastAmendDate { get; set; }

        [Display(Name = "Дата на прекратяване:")]
        public string EndDate { get; set; }

        [Display(Name = "Основание:")]
        public string Reason { get; set; }

        [Display(Name = "Срок:")]
        public string TimeLimit { get; set; }

        [Display(Name = "Код КИД:")]
        public string EcoCode { get; set; }

        [Display(Name = "Код НКПД:")]
        public string ProfessionCode { get; set; }

        [Display(Name = "Заплата:")]
        public string Remuneration { get; set; }

        [Display(Name = "Длъжност наименование:")]
        public string ProfessionName { get; set; }
    }
}
