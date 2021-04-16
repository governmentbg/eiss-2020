using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    public class CourtVM
    {
        public int Id { get; set; }

        [Display(Name = "Екли код")]
        public string EcliCode { get; set; }

        [Display(Name = "Тип")]
        public string Code { get; set; }

        [Display(Name = "Наименование")]
        public string Label { get; set; }
        
        [Display(Name = "Град")]
        public string CityName { get; set; }

        [Display(Name = "Адрес")]
        public string Address { get; set; }

        [Display(Name = "Тип")]
        public string CourtTypeName { get; set; }

    }
}
