using IOWebApplication.Infrastructure.Attributes;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class WorkTaskManageVM
    {
        public string TaskIds { get; set; }

        public bool ShowUser { get; set; }

        [Display(Name="Нов изпълнител на задачите")]
        [IORequired]
        public string NewUserId { get; set; }

        [Display(Name = "Начин на изпълнение")]
        public int? TaskExecutionId { get; set; }
        [Display(Name = "Изберете структура")]
        [IORequired]
        public int? CourtOrganizationId { get; set; }


        [Display(Name = "Основание")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public string Description { get; set; }
    }
}
