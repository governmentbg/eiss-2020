using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseMovementVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int? CourtId { get; set; }
        public string CaseName { get; set; }

        [Display(Name = "Тип насочване")]
        public string MovementTypeLabel { get; set; }

        [Display(Name = "Тип насочване")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете тип насочване")]
        public int MovementTypeId { get; set; }

        [Display(Name = "Насочено към")]
        public string NameFor { get; set; }

        [Display(Name = "Служител")]
        [Required(ErrorMessage = "Изберете служител")]
        public string ToUserId { get; set; }

        [Display(Name = "Отдел/Звено/Дирекция")]
        [Required(ErrorMessage = "Изберете Отдел/Звено/Дирекция")]
        public int? CourtOrganizationId { get; set; }

        [Display(Name = "Външно лице")]
        [Required(ErrorMessage = "Въведете име на външно лице")]
        public string OtherInstitution { get; set; }

        [Display(Name = "Дата на изпращане")]
        public DateTime DateSend { get; set; }

        [Display(Name = "Приел задачата")]
        public string AcceptUserId{ get; set; }

        [Display(Name = "Приел задачата")]
        public string AcceptLawUnitName { get; set; }

        [Display(Name = "Дата на приемане")]
        public DateTime? DateAccept { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Описание приемане")]
        public string AcceptDescription { get; set; }

        [Display(Name = "Забележка анулиране")]
        public string DisableDescription { get; set; }

        [Display(Name = "Активен")]
        public string IsActiveText { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; }

        public string UserId { get; set; }
        public int UserLawUnitId { get; set; }
        public string UserLawUnitName { get; set; }
        public bool IsEdit { get; set; }
        public bool IsAccept { get; set; }
        public bool IsEditAccept { get; set; }
        public string ViewUrl { get; set; }
    }
}
