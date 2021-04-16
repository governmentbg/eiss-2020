using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryAreaAddressTestVM
    {
        public Address Address { get; set; }

        [Display(Name = "Район")]
        public string AreaName { get; set; }
 
        [Display(Name = "Призовкар")]
        public string LawUnitName { get; set; }

        [Display(Name = "Населено място")]
        public string City { get; set; }

        [Display(Name = "Улицa")]
        public string Street { get; set; }

        [Display(Name = "Квартал")]
        public string ResidentionArea { get; set; }

        [Display(Name = "Тип номера")]
        public string NumberType { get; set; }

        [Display(Name = "Oт номер")]
        public string NumberFrom { get; set; }

        [Display(Name = "До номер")]
        public string NumberTo { get; set; }
    }
}
