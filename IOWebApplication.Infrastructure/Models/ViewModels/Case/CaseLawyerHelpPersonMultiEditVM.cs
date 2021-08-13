using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseLawyerHelpPersonMultiEditVM
    {
        public int CaseLawyerHelpId { get; set; }

        [Display(Name = "Назначен адвокат")]
        public int? AssignedLawyerId { get; set; }

        [Display(Name = "Искан адвокат от лицето")]
        public int? SpecifiedLawyerLawUnitId { get; set; }

        public virtual List<CheckListVM> CaseLawyerHelpPeople { get; set; }
    }
}
