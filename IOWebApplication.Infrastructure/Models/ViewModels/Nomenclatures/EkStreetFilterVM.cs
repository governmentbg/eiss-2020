using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    public class EkStreetFilterVM
    {
        [Display(Name = "Държава")]
        public string CountryCode { get; set; }

        [Display(Name = "Населено място")]
        public string CityCode { get; set; }

        [Display(Name = "Вид")]
        public int? StreetTipeId { get; set; }

        [Display(Name = "Име")]
        public string ElementName { get; set; }
    }
}
