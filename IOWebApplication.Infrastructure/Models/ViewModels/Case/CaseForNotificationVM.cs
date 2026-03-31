using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseForNotificationVM
    {
        public int Id { get; set; }

        [Display(Name = "Номер на дело")]
        public string RegNumber { get; set; }

        [Display(Name = "Дата на образуване")]
        public DateTime RegDate { get; set; }

        [Display(Name = "Отделение/Състав")]
        public string DepartmentOtdelenieText { get; set; }

        public string CaseTypeCode { get; set; }

    }
}
