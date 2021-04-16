using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionResultEditVM
    {
        public int Id { get; set; }

        public int? CourtId { get; set; }

        public int? CaseId { get; set; }

        public int CaseSessionId { get; set; }

        /// <summary>
        /// Резултат на заседание, отложено, обявен за решаване, спряно ит.н.
        /// </summary>
        [Display(Name = "Резултат от заседанието")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете резултат от заседанието")]
        public int SessionResultId { get; set; }

        /// <summary>
        ///  Основание за резултат от заседание
        /// </summary>
        [Display(Name = "Основание")]
        public int? SessionResultBaseId { get; set; }

        [Display(Name = "Забележка")]
        public string Description { get; set; }

        [Display(Name = "Активен резултат")]
        public bool IsActive { get; set; }

        [Display(Name = "Основен резултат")]
        public bool IsMain { get; set; }

        public int CallFromActId { get; set; }

        public virtual List<CheckListVM> CaseLawUnitByCase { get; set; }
    }
}
