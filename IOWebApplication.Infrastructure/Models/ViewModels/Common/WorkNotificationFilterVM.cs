using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class WorkNotificationFilterVM
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        public int? Id { get; set; }

        public const int ReadTypeUnRead = 1;
        public const int ReadTypeRead = 2;
        public const int ReadTypeAll = 3;
        
        [Display(Name = "Вид известие")]
        public int WorkNotificationTypeId { get; set; }

        [Display(Name = "Прочетени/непрочетени")]
        public int ReadTypeId { get; set; }

        [Display(Name = "От дата прочитане")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата прочитане")]
        public DateTime? DateTo { get; set; }
        
        [Display(Name = "Към дата")]
        public DateTime? DateCreate { get; set; }

        [Display(Name = "Състав")]
        public int? UserCourtDepartmentId { get; set; }

        public string UserId { get; set; }
        public int CourtId { get; set; }
        public int SourceType { get; set; }
        public long SourceId { get; set; }

        /// <summary>
        /// Група известия: 1- Общи известия,2-Известия ЗП
        /// </summary>
        public int? NotificationKind { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }
    }
}
