using IOWebApplication.Infrastructure.Data.Models.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class DocumentLinkVM
    {
        public long Id { get; set; }
        public int Index { get; set; }

        [Display(Name = "Изберете съд")]
        public int? CourtId { get; set; }

        /// <summary>
        /// true при избор на свързан документ от друга система
        /// </summary>
        [Display(Name = "документ от друга система")]
        public bool IsLegacyDocument { get; set; }

        [Display(Name = "Направление")]
        [Range(0, 9999999, ErrorMessage = "Изберете '{0}'.")]
        public int? DocumentDirectionId { get; set; }

        [Display(Name = "Документ номер")]
        public string PrevDocumentNumber { get; set; }

        [Display(Name = "Документ дата")]
        public DateTime? PrevDocumentDate { get; set; }

        [Display(Name = "Изберете документ")]
        public long? PrevDocumentId { get; set; }

        [Display(Name = "Забележка")]
        public string Description { get; set; }

        public string GetPrefix
        {
            get
            {
                return nameof(DocumentVM.DocumentLinks);
            }
        }
        public string GetPath
        {
            get
            {
                return string.Format("{0}[{1}]", this.GetPrefix, Index);
            }
        }

        public void ToEntity(DocumentLink model)
        {
            model.CourtId = this.CourtId;
            model.DocumentDirectionId = this.DocumentDirectionId;
            model.PrevDocumentNumber = this.PrevDocumentNumber;
            model.PrevDocumentDate = this.PrevDocumentDate;
            model.PrevDocumentId = this.PrevDocumentId;
            model.Description = this.Description;
        }
    }
}
