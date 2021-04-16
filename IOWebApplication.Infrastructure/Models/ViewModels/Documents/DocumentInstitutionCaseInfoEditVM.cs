using IOWebApplication.Infrastructure.Data.Models.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentInstitutionCaseInfoEditVM
    {
        public long Id { get; set; }
        public long DocumentId { get; set; }
        public int CaseId { get; set; }
        
        [Display(Name = "Вид институция")]
        public int InstitutionTypeId { get; set; }

        [Display(Name = "Институция")]
        public int InstitutionId { get; set; }

        [Display(Name = "Вид дело")]
        public int? InstitutionCaseTypeId { get; set; }

        [Display(Name = "Номер дело")]
        public string CaseNumber { get; set; }

        [Display(Name = "Година")]
        public int CaseYear { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        public void ToEntity(DocumentInstitutionCaseInfo model)
        {
            model.Id = Id;
            model.DocumentId = DocumentId;
            model.InstitutionId = InstitutionId;
            model.CaseNumber = CaseNumber;
            model.CaseYear = CaseYear;
            model.Description = Description;
            model.InstitutionCaseTypeId = InstitutionCaseTypeId;
        }
    }
}
