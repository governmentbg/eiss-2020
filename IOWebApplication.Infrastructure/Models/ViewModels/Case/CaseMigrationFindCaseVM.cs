using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseMigrationFindCaseVM
    {
        [Display(Name = "Изпратено от съд")]
        public int FromCourtId { get; set; }
        [Display(Name = "Изпратено дело")]
        public int FromCaseId { get; set; }

        public int CaseId { get; set; }
        [Display(Name = "Текущо дело")]
        public string CaseInfo { get; set; }

        [Display(Name = "Забележка по послужването")]
        public string Description { get; set; }
    }
}
