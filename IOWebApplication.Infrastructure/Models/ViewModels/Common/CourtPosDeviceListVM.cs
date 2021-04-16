using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CourtPosDeviceListVM
    {
        public int Id { get; set; }

        [Display(Name = "Етикет")]
        public string Label { get; set; }

        [Display(Name = "По сметка")]
        public string CourtBankAccountName { get; set; }

        [Display(Name = "Номер на ПОС устройство")]
        public string Tid { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; }
    }
}
