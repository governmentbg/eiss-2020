using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
   public class WorkNotificationFilterVM
    {
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
        public string UserId { get; set; }
        public int CourtId { get; set; }
        public int SourceType { get; set; }
        public long SourceId { get; set; }
   }
}
