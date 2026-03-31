using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSelectionProtocolSubstitutionFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }


        [Display(Name = "Съдия-докладчик")]
        public int? JudgeReporterId { get; set; }

        [Display(Name = "Заместван съдия")]
        public int? SubstitudedJudgeId { get; set; }

        [Display(Name = "Група на заместване")]
        public int ?CourtGroupId { get; set; }



    }
}
