using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSessionVM
    {
        public int Id { get; set; }
        public DateTime DateWrt { get; set; }
        public int? CourtId { get; set; }
        public int CaseId { get; set; }

        [Display(Name = "Вид заседаниe")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int SessionTypeId { get; set; }

        [Display(Name = "Зала")]
        public int? CourtHallId { get; set; }

        [Display(Name = "Статус на заседание")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int SessionStateId { get; set; }
        [Display(Name = "Номер на дело")]
        public string CaseName { get; set; }
        [Display(Name = "Вид дело")]
        public string CaseTypeLabel { get; set; }
        [Display(Name = "Вид заседание")]
        public string SessionTypeLabel { get; set; }
        [Display(Name = "Зала")]
        public string CourtHallName { get; set; }
        [Display(Name = "Статус")]
        public string SessionStateLabel { get; set; }

        [Display(Name = "Резултат")]
        public string SessionResultLabel { get; set; }

        [Display(Name = "Резултат")]
        public string SessionResultIds { get; set; }

        [Display(Name = "Адрес на онлайн заседание")]
        public string VideoUrl { get; set; }

        [Display(Name = "Забележка")]
        public string Description { get; set; }

        [Display(Name = "Начало")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }
        public DateTime DateFromTime { get; set; }

        [Display(Name = "Дата")]
        public string DateFromDateString { get; set; }

        [Display(Name = "Час")]
        public string DateFromTimeString { get; set; }

        [Display(Name = "Край")]
        public DateTime? DateTo { get; set; }


        private int dateTo_Minutes { get; set; } = -1;
        [Display(Name = "Прогнозна продължителност")]
        public int DateTo_Minutes
        {
            get
            {
                if (dateTo_Minutes < 0)
                {
                    dateTo_Minutes = Convert.ToInt32(((TimeSpan)(DateTo ?? DateFrom).Subtract(DateFrom)).TotalMinutes);
                }
                return dateTo_Minutes;
            }

            set { dateTo_Minutes = value; }
        }

        public bool IsExpired { get; set; }

        public int CaseTypeId { get; set; }
        public int? NotificationListTypeId { get; set; }
        [Display(Name = "Съдия докладчик")]
        public string JudgeReporterLabel { get; set; }

        [Display(Name = "Състав")]
        public string JudgeCompositionLabel { get; set; }

        [Display(Name = "Тип акт")]
        public int? ActTypeId { get; set; }

        [Display(Name = "Вид")]
        public int? ActKindId { get; set; }

        [Display(Name = "Свързан съдебен акт")]
        public int? RelatedActId { get; set; }

        [Display(Name = "Подлежи на обжалване")]
        public bool? ActCanAppeal { get; set; }

        [Display(Name = "Финализиращ акт")]
        public bool? IsFinalDoc { get; set; }

        public int? ActSaveId { get; set; }
        public string ActSaveType { get; set; }

        public int? CaseSessionOldId { get; set; }
        [Display(Name = "Номер")]
        public int? NumberRow { get; set; }

        [Display(Name = "Отделение/Състав")]
        public string DepartmentOtdelenieText { get; set; }

        [Display(Name = "Резултат/степен на уважаване на иска")]
        public int? ActComplainResultId { get; set; }

        [Display(Name = "Промяна на състава по делото")]
        public int? VksLawunitChange { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseNameText { get; set; }

        [Display(Name = "Правно основание")]
        public int? ActISPNReasonId { get; set; }

        [Display(Name = "Незабавно изпълнение на акта")]
        public bool? RnflEffectiveImmediately { get; set; }

        #region Резултат от заседание

        [Display(Name = "Резултат от заседанието")]
        public int? SessionResultId { get; set; }

        /// <summary>
        ///  Основание за резултат от заседание
        /// </summary>
        [Display(Name = "Основание")]
        public int? SessionResultBaseId { get; set; }

        [Display(Name = "Забележка")]
        public string DescriptionResult { get; set; }

        [Display(Name = "Основен резултат")]
        public bool IsMainResult { get; set; }

        #endregion

        [Display(Name = "ЗЗ в делото без акт")]
        public int? CaseSessionAddActId { get; set; }

        /// <summary>
        /// Вписване/обявяване в АИСТН
        /// </summary>
        [Column("td_act_for_registration")]
        [Display(Name = "Вписване/обявяване в АИСТН")]
        public bool? TDActForRegistration { get; set; }

        [Display(Name = "Коригиране на акт (ОФГ)")]
        public bool HasCorrectedAct { get; set; }

        [Display(Name = "Коригирани съдебни актове")]
        public string[] CorrectedActsIds { get; set; }

        public virtual List<CheckListVM> CaseLawUnitByCase { get; set; }
        public virtual List<CheckListVM> CaseSessions { get; set; }

        public CaseSessionVM()
        {
            CorrectedActsIds = new string[] { };
        }
    }
}
