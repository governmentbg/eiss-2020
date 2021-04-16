using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class CaseObligationReportVM
    {
        [Display(Name = "№ по ред")]
        public int Index { get; set; }

        [Display(Name = "Вид, №/година на дело")]
        public string CaseData { get; set; }

        [Display(Name = "Участие Име, презиме, фамилия на лицето")]
        public string PersonName { get; set; }

        [Display(Name = "Дата")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ObligationDate { get; set; }

        [Display(Name = "Сума")]
        public decimal Amount { get; set; }

        [Display(Name = "Забележка")]
        public string Description { get; set; }

        [Display(Name = "Подпис")]
        public string Signature { get; set; }

        public decimal AmountPay { get; set; }
        public bool IsActive { get; set; }

        [Display(Name = "Забележка")]
        public string DescriptionText
        {
            get
            {
                string result = "";

                if (IsActive == false)
                    result = "Отменена";
                else
                    result = Amount - AmountPay > 0.001M ? "Наложена" : "Платена";

                result = (string.IsNullOrEmpty(Description) ? "" : Description + " - ") + result;
                return result;
            }
        }

        [Display(Name = "Дата")]
        public string ObligationDateData { get; set; }
    }

    public class CaseObligationFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Съдебен състав")]
        public int DepartmentId { get; set; }
    }
}
