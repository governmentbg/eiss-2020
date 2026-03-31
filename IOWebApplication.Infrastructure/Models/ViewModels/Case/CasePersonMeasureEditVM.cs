using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CasePersonMeasureEditVM
    {
        public int Id { get; set; }
        public int CourtId { get; set; }
        public int CaseId { get; set; }
        public int CasePersonId { get; set; }
        public int? ParentId { get; set; }

        [Display(Name = "Вид институция")]
        public int? MeasureInstitutionTypeId { get; set; }

        [Display(Name = "Институция, определила мярката")]
        public int? MeasureInstitutionId { get; set; }

        [Display(Name = "Съд, определил мярката")]
        public int? MeasureCourtId { get; set; }

        [Display(Name = "Вид мярка")]
        public int? MeasureKindId { get; set; }

        [Display(Name = "Мярка")]
        [Required(ErrorMessage = "Изберете {0}.")]
        // eispp_tbl_code =214
        public string MeasureType { get; set; }

        [Display(Name = "Количество")]
        public int? MeasureQuantity { get; set; }

        [Display(Name = "Мерна единица")]
        public string MeasureUnit { get; set; }

        [Display(Name = "Дата на мярката")]
        [Required(ErrorMessage = "Изберете {0}.")]
        public DateTime MeasureStatusDate { get; set; }

        [Display(Name = "Гаранция, евро")]
        public double BailAmount { get; set; }

        [Display(Name = "Статус")]
        [Required(ErrorMessage = "Изберете {0}.")]
        public string MeasureStatus { get; set; }

        //--------Време-------------------------
        [Range(0, int.MaxValue, ErrorMessage = "Въведете дни в интервала 0-9999")]
        [Display(Name = "Дни")]
        public int MeasureDays { get; set; }

        [Display(Name = "Седмици")]
        [Range(0, int.MaxValue, ErrorMessage = "Въведете седмици в интервала 0-9999")]
        public int MeasureWeeks { get; set; }

        [Display(Name = "Месеци")]
        [Range(0, int.MaxValue, ErrorMessage = "Въведете месеци в интервала 0-9999")]
        public int MeasureMonths { get; set; }

        [Display(Name = "Години")]
        [Range(0, int.MaxValue, ErrorMessage = "Въведете години в интервала 0-9999")]
        public int MeasureYears { get; set; }

        public DateTime? DateExpired { get; set; }

        public List<CheckListVM> Punishments { get; set; } = new();
    }
}
