using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class ReportCourtStatsVM
    {
        [Display(Name = "Съд")]
        [Column("CourtName")]
        public string CourtName { get; set; }

        [Display(Name = "Документи")]
        [Column("DocCount")]
        public int DocCount { get; set; }

        [Display(Name = "Дела")]
        [Column("CaseCount")]
        public int CaseCount { get; set; }
        [Display(Name = "Заседания")]
        [Column("CaseSessionCount")]
        public int CaseSessionCount { get; set; }
        [Display(Name = "Актове")]
        [Column("ActCount")]
        public int ActCount { get; set; }
        [Display(Name = "Подписани актове")]
        [Column("ActSignedCount")]
        public int ActSignedCount { get; set; }
    }
}
