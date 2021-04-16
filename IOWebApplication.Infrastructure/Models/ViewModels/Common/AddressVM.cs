using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class AddressVM
    {
        public long Id { get; set; }

        [Display(Name = "Вид адрес")]
        public string AddressTypeLabel { get; set; }

        [Display(Name = "Адрес")]
        public string FullAddress { get; set; }
    }
}
