using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.AuditLog
{
    public class AuditLogFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Номер")]
        public string RegNumber { get; set; }

        [Display(Name = "Операция")]
        public string Operation { get; set; }

        [Display(Name = "Потребител")]
        public string UserId { get; set; }
    }
}
