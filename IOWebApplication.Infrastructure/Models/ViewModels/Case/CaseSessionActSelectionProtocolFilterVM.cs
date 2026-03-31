using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSessionActSelectionProtocolFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Финализиращ акт")]

        public bool IsFinal { get; set; }


        [Display(Name = "За прекратяване")]
        public bool IsCanceling { get; set; }


        [Display(Name = "Влязъл в сила")]
        public bool IsBeacameFinal { get; set; }

        [Display(Name = "Резултат/степен на уважаване на иска")]
        public int? ActResultId { get; set; }

 


        [Display(Name = "Съдия-докладчик")]
        public int JudgeReporterId { get; set; }



    }
}
