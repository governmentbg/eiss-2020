using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSelectionProtokolFilterVM
    {
        public int Id { get; set; }

        public int CaseId { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }

        [Display(Name = "Тип разпределение")]
        public int JudgeRoleId { get; set; }

        [Display(Name = "Начин на разпределение")]
        public int SelectionModeId { get; set; }

        [Display(Name = "Имена на разпрeделен")]
        public string FullName { get; set; }

        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Година")]
        public int? Year { get; set; }

        [Display(Name = "Имена на разпределящ")]
        public string UserId { get; set; }

        [Display(Name = "Номер на инициращ документ")]
        public string DocumentNumber { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Основен вид дело")]
        public string CaseGroupIds { get; set; }
        public string CaseGroupIds_text { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "Точен вид дело")]
        public string CaseTypeIds { get; set; }
        public string CaseTypeIds_text { get; set; }

        [Display(Name = "Статус на протокол")]
        public int ProtokolState { get; set; }

        [Display(Name = "Съдебна група за разпределяне")]
        public int CourtGroupId { get; set; }

        [Display(Name = "Съдебна група за разпределяне")]
        public string CourtGroupIds { get; set; }
        public string CourtGroupIds_text { get; set; }

        [Display(Name = "Група по натовареност")]
        public int LoadGroupLinkId { get; set; }

        [Display(Name = "Група по натовареност")]
        public string LoadGroupLinkIds { get; set; }
        public string LoadGroupLinkIds_text { get; set; }
    }
}
