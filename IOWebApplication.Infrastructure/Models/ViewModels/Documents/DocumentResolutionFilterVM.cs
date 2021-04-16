using IOWebApplication.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentResolutionFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Номер на разпореждане")]
        public string ResolutionNumber { get; set; }

        [Display(Name = "Номер на документ")]
        public string DocumentNumber { get; set; }

        [Display(Name = "Година на документ")]
        public int? DocumentYear { get; set; }

        [Display(Name = "Съдия")]
        public int? JudgeId { get; set; }

        public void NormalizeValues()
        {
            ResolutionNumber = ResolutionNumber.EmptyToNull();
            DocumentNumber = DocumentNumber.EmptyToNull();
            JudgeId = JudgeId.EmptyToNull().EmptyToNull(0);
        }
    }
}
