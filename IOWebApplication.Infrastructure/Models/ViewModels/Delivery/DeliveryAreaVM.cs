using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryAreaVM
    {
        public int Id { get; set; }
  
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Код")]
        public string Code { get; set; }
        
        [Display(Name = "Служител")]
        public string LawUnitName { get; set; }

        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

    }
}
