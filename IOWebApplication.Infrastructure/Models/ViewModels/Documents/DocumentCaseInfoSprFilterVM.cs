using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentCaseInfoSprFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Основен вид докумет регистратура")]
        public int DocumentGroupId { get; set; }

        [Display(Name = "Точен вид документ регистратура")]
        public int DocumentTypeId { get; set; }

        [Display(Name = "Вид документ представен в заседание")]
        public int SessionDocTypeId { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "Предмет на дело")]
        public int CaseCodeId { get; set; }
    }
}
