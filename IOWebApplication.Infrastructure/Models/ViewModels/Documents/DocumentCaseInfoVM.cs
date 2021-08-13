using IOWebApplication.Infrastructure.Attributes;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class DocumentCaseInfoVM
    {
        public long Id { get; set; }
        public int Index { get; set; }
        [Display(Name = "Съд")]
        [IORequired]
        public int CourtId { get; set; }

        public string CourtName { get; set; }

        [Display(Name = "Избор дело")]
        public int? CaseId { get; set; }

        /// <summary>
        /// true при избор на старо свързано дело от друга система
        /// </summary>
        [Display(Name = "Дело от друга система")]
        public bool IsLegacyCase { get; set; }

        [Display(Name = "Номер дело")]
        [IORequired]
        public string CaseRegNumber { get; set; }
        [Display(Name = "Година дело")]
        public int? CaseYear { get; set; }
        [Display(Name = "Код на дело")]
        public string CaseShortNumber { get; set; }       

        [Display(Name = "Избор на съдебен акт")]
        public bool HasLawAct { get; set; }

        [Display(Name = "Съдебен акт")]
        public int? SessionActId { get; set; }

        [Display(Name = "Забележка")]
        public string Description { get; set; }
        

        public void ToEntity(DocumentCaseInfo model)
        {
            model.CourtId = this.CourtId;
            model.IsLegacyCase= this.IsLegacyCase;
            model.CaseRegNumber = this.CaseRegNumber;
            model.CaseShortNumber = this.CaseShortNumber;
            model.CaseYear = this.CaseYear;
            model.CaseId = this.CaseId;
            model.SessionActId = this.SessionActId;
            model.Description = this.Description;
        }
    }

}
