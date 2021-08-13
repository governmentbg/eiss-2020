using IOWebApplication.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.Documents
{
    public class DocumentFilterVM
    {
        [Display(Name = "Регистратура")]
        public int? CourtOrganizationId { get; set; }

        [Display(Name = "Направление")]
        public int? DocumentDirectionId { get; set; }
        [Display(Name = "Тип документ")]
        public int? DocumentKindId { get; set; }
        [Display(Name = "Основен вид")]
        public int? DocumentGroupId { get; set; }
        [Display(Name = "Точен вид")]
        public int? DocumentTypeId { get; set; }

        [Display(Name = "Номер на документ")]
        public string DocumentNumber { get; set; }
        [Display(Name = "Година")]
        public int? DocumentYear { get; set; }

        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Имена / Наименование на лице")]
        public string PersonName { get; set; }

        [Display(Name = "Идентификатор на лице")]
        public string PersonUIC { get; set; }

        [Display(Name = "Вид лице")]
        public int? PersonRoleId { get; set; }

        [Display(Name = "Свързано дело от съд")]
        public int? LinkDelo_CourtId { get; set; }

        [Display(Name = "Свързано дело номер")]
        public int? LinkDelo_CaseId { get; set; }

        [Display(Name = "Свързано дело забележка")]
        public string LinkDelo_Description { get; set; }

        [Display(Name = "Дело от друга система")]
        public bool VisibleOtherSystem { get; set; }

        [Display(Name = "Дело от друга система номер")]
        public string RegNumberOtherSystem { get; set; }

        [Display(Name = "Дело от друга система година")]
        public int? YearOtherSystem { get; set; }

        [Display(Name = "Дело от друга система от съд")]
        public int? CourtOtherSystem { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }

        [Display(Name = "ЕИСПП номер")]
        public string CaseEisppNumber { get; set; }

        [Display(Name = "Вид външна институция")]
        public int? InstitutionTypeId { get; set; }

        [Display(Name = "Институция")]
        public int? InstitutionId { get; set; }

        [Display(Name = "Дело на външна институция")]
        public string InstitutionCaseNumber { get; set; }


        [Display(Name = "Година")]
        public int? InstitutionCaseYear { get; set; }

        public void NormalizeValues()
        {
            CourtOrganizationId = CourtOrganizationId.EmptyToNull();
            DocumentDirectionId = DocumentDirectionId.EmptyToNull();
            DocumentKindId = DocumentKindId.EmptyToNull();
            DocumentGroupId = DocumentGroupId.EmptyToNull();
            DocumentTypeId = DocumentTypeId.EmptyToNull();
            DocumentNumber = DocumentNumber.EmptyToNull();
            DocumentYear = DocumentYear.EmptyToNull();
            DateTo = DateTo.MakeEndDate();
            PersonName = PersonName.EmptyToNull();
            PersonUIC = PersonUIC.EmptyToNull();
            PersonRoleId = PersonRoleId.EmptyToNull();
            CaseRegNumber = CaseRegNumber.EmptyToNull();
            CaseEisppNumber = CaseEisppNumber.EmptyToNull();

            InstitutionTypeId = InstitutionTypeId.EmptyToNull();
            InstitutionId = InstitutionId.EmptyToNull();
            InstitutionCaseNumber = InstitutionCaseNumber.EmptyToNull();
            InstitutionCaseYear = InstitutionCaseYear.EmptyToNull();
        }
    }
}
