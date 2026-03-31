using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryItemRecapTransVM
    {
        public int Id { get; set; }

        public int CourtId { get; set; }
        public int FromCourtId { get; set; }

        public int? LawUnitId { get; set; }
        
        [Display(Name = "Изготвена в съд")]
        public string FromCourtName { get; set; }

        [Display(Name = "За доставка в съд")]
        public string CourtName { get; set; }

        [Display(Name = "Призовкар")]
        public string LawUnitName { get; set; }
    }
}
