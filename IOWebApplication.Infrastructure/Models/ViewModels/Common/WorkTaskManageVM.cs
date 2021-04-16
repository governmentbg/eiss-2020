using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class WorkTaskManageVM
    {
        public string TaskIds { get; set; }

        public bool ShowUser { get; set; }

        [Display(Name="Нов изпълнител на задачите")]
        [Required(ErrorMessage ="Изберете {0}.")]
        public string NewUserId { get; set; }

        [Display(Name = "Основание")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public string Description { get; set; }
    }
}
