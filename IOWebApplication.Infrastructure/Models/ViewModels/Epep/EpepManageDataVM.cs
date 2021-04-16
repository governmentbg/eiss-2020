using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Epep
{
    public class EpepManageDataVM
    {
        [Display(Name = "Вид обект")]
        [Range(1, 99999999, ErrorMessage = "Въведете {0}.")]
        public int DataType { get; set; }

        [Display(Name = "Идентификатор от ЕПЕП")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public string ObjectId { get; set; }

        [Display(Name = "Код за достъп")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public string SecurityCode { get; set; }

        public string ResponseMessage { get; set; }

        public bool CheckCode()
        {
            return SecurityCode == string.Format("ddHHMMyyyy", DateTime.Now);
        }
    }
}
