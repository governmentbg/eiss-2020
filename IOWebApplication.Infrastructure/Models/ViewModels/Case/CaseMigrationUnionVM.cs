using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseMigrationUnionVM
    {
        public int CaseId { get; set; }
        [Display(Name = "Текущо дело")]
        public string CaseInfo { get; set; }

        [Display(Name = "Дело, което ще се обедини с текущото")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете '{0}'.")]
        public int CaseToUnionId { get; set; }
        [Display(Name = "Забележка по обединяването")]
        public string Description { get; set; }
    }
}
