using IOWebApplication.Infrastructure.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.Documents
{
    /// <summary>
    /// Филтър за данни за регистрирани документи
    /// </summary>
    public class DocumentFilterVM
    {
        public bool GlobalAssignmentRegister { get; set; }

        /// <summary>
        /// Регистратура
        /// </summary>
        [Display(Name = "Регистратура")]
        public int? CourtOrganizationId { get; set; }

        /// <summary>
        /// Направление
        /// </summary>
        [Display(Name = "Направление")]
        public int? DocumentDirectionId { get; set; }

        /// <summary>
        /// Тип документ
        /// </summary>
        [Display(Name = "Тип документ")]
        public int? DocumentKindId { get; set; }

        /// <summary>
        /// Основен вид
        /// </summary>
        [Display(Name = "Основен вид")]
        public int? DocumentGroupId { get; set; }

        /// <summary>
        /// Точен вид
        /// </summary>
        [Display(Name = "Точен вид")]
        public int? DocumentTypeId { get; set; }

        /// <summary>
        /// Номер на документ
        /// </summary>
        [Display(Name = "Номер на документ")]
        public string DocumentNumber { get; set; }

        /// <summary>
        /// Година
        /// </summary>
        [Display(Name = "Година")]
        public int? DocumentYear { get; set; }

        /// <summary>
        /// От дата
        /// </summary>
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// До дата
        /// </summary>
        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        [Display(Name = "Описание")]
        public string Description { get; set; }

        /// <summary>
        /// Имена / Наименование на лице
        /// </summary>
        [Display(Name = "Имена / Наименование на лице")]
        public string PersonName { get; set; }

        /// <summary>
        /// Идентификатор на лице
        /// </summary>
        [Display(Name = "Идентификатор на лице")]
        public string PersonUIC { get; set; }

        /// <summary>
        /// Вид лице
        /// </summary>
        [Display(Name = "Вид лице")]
        public int? PersonRoleId { get; set; }

        /// <summary>
        /// Свързано дело от съд
        /// </summary>
        [Display(Name = "Свързано дело от съд")]
        public int? LinkDelo_CourtId { get; set; }

        /// <summary>
        /// Свързано дело номер
        /// </summary>
        [Display(Name = "Свързано дело номер")]
        public int? LinkDelo_CaseId { get; set; }

        /// <summary>
        /// Свързано дело забележка
        /// </summary>
        [Display(Name = "Свързано дело забележка")]
        public string LinkDelo_Description { get; set; }

        /// <summary>
        /// Дело от друга система
        /// </summary>
        [Display(Name = "Дело от друга система")]
        public bool VisibleOtherSystem { get; set; }

        /// <summary>
        /// Дело от друга система номер
        /// </summary>
        [Display(Name = "Дело от друга система номер")]
        public string RegNumberOtherSystem { get; set; }

        /// <summary>
        /// Дело от друга система година
        /// </summary>
        [Display(Name = "Дело от друга система година")]
        public int? YearOtherSystem { get; set; }

        /// <summary>
        /// Дело от друга система от съд
        /// </summary>
        [Display(Name = "Дело от друга система от съд")]
        public int? CourtOtherSystem { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }

        /// <summary>
        /// ЕИСПП номер
        /// </summary>
        [Display(Name = "ЕИСПП номер")]
        public string CaseEisppNumber { get; set; }

        /// <summary>
        /// Вид външна институция
        /// </summary>
        [Display(Name = "Вид външна институция")]
        public int? InstitutionTypeId { get; set; }

        /// <summary>
        /// Институция
        /// </summary>
        [Display(Name = "Институция")]
        public int? InstitutionId { get; set; }

        /// <summary>
        /// Дело на външна институция
        /// </summary>
        [Display(Name = "Дело на външна институция")]
        public string InstitutionCaseNumber { get; set; }

        /// <summary>
        /// Година
        /// </summary>
        [Display(Name = "Година")]
        public int? InstitutionCaseYear { get; set; }

        /// <summary>
        /// Начин на получаване
        /// </summary>
        [Display(Name = "Начин на получаване")]
        public int? DeliveryGroupInputId { get; set; }

        /// <summary>
        /// Начин на изпращане
        /// </summary>
        [Display(Name = "Начин на изпращане")]
        public int? DeliveryGroupOutputId { get; set; }

        //----------Централна регистратура

        [Display(Name = "ЕПЕП номер")]
        public string EpepNumber { get; set; }

        [Display(Name = "Входирано в съд")]
        public int? CreatedCourtId { get; set; }

        [Display(Name = "Разпределено в съд")]
        public int? AssignedInCourtId { get; set; }

        [Display(Name = "Номер дело")]
        public string AssignedCaseNumber { get; set; }

        [Display(Name = "Вид заявление")]
        public int? DocumentRequestTypeId { get; set; }

        /// <summary>
        /// Сетване на дефалтни стойности
        /// </summary>
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
            EpepNumber = EpepNumber.EmptyToNull();
            CreatedCourtId = CreatedCourtId.EmptyToNull();
            AssignedInCourtId = AssignedInCourtId.EmptyToNull();
            AssignedCaseNumber = AssignedCaseNumber.EmptyToNull();
            DocumentRequestTypeId = DocumentRequestTypeId.EmptyToNull();
        }
    }
}
