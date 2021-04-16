using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class DocumentInstitutionCaseInfoVM
    {
        public long Id { get; set; }
        public int Index { get; set; }
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

        [Display(Name = "Забележка")]
        public string Description { get; set; }

        public string GetPrefix
        {
            get
            {
                return nameof(DocumentVM.InstitutionCaseInfo);
            }
        }
        public string GetPath
        {
            get
            {
                return string.Format("{0}[{1}]", this.GetPrefix, Index);
            }
        }
        public void ToEntity(DocumentInstitutionCaseInfo model)
        {
            model.InstitutionId = this.InstitutionId;
            model.InstitutionCaseTypeId = this.InstitutionCaseTypeId;
            model.CaseNumber = this.CaseNumber;
            model.CaseYear = this.CaseYear;
            model.Description = this.Description;
        }
    }

}
