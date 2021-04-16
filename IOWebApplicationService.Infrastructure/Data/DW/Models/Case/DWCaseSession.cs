using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplicationService.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Data.DW.Models
{
    /// <summary>
    /// Заседания по делото
    /// </summary>
    [Table("dw_case_session")]
    public class DWCaseSession : DWUserDateWRT
    {
        [Key]
        [Column("dw_Id")]
        public int dw_Id { get; set; }

        [Column("id")]
        public int Id { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        /// <summary>
        /// Вид заседания: Закрито,Първо,второ,друго
        /// </summary>
        [Column("session_type_id")]
        [Display(Name = "Вид заседаниe")]
        public int SessionTypeId { get; set; }

        [Column("session_type_name")]

        public string SessionTypeName { get; set; }

        [Column("compartment_id")]
        public int CompartmentId { get; set; }


        [Column("compartment_name")]
        public string CompartmentName { get; set; }

        [Column("court_hall_id")]
        [Display(Name = "Зала")]
        public int? CourtHallId { get; set; }

        [Column("court_hall_name")]
        [Display(Name = "Зала")]
        public string CourtHallName { get; set; }

        /// <summary>
        /// Дата и час
        /// </summary>
        [Column("date_from")]
        [Display(Name = "Начало")]
        public DateTime DateFrom { get; set; }
    [Column("date_from_str")]
    [Display(Name = "Начало")]
    public String DateFromStr { get; set; }

    [Column("date_to")]
        [Display(Name = "Край")]
        public DateTime? DateTo { get; set; }
    [Column("date_to_str")]
    [Display(Name = "Край")]
    public String DateToStr { get; set; }

    /// <summary>
    /// Статус на заседание: насрочено, отложено
    /// </summary>
    [Column("session_state_id")]
        [Display(Name = "Статус на заседание")]
        public int SessionStateId { get; set; }

        [Column("session_state_name")]

        public string SessionStateName { get; set; }

        [Column("description")]
        [Display(Name = "Забележка")]
        public string Description { get; set; }




        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }
    [Column("date_expired_str")]
    [Display(Name = "Дата на анулиране")]
    public string DateExpiredStr { get; set; }

    [Column("user_expired_id")]
        public string UserExpiredId { get; set; }
        [Column("user_expired_name")]
        public string UserExpiredName { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [Column("date_returned")]
        [Display(Name = "Дата на връщане")]
        public DateTime? DateReturned { get; set; }

    [Column("judge_reporter_id")]
    public int JudgeReporterId { get; set; }
    [Column("judge_reporter_name")]
    public string JudgeReporterName { get; set; }

    [Column("session_judge_staff")]
    public string SessionJudgeStaff { get; set; }

    [Column("session_juri_staff")]
    public string SessionJuriStaff { get; set; }
    [Column("session_full_judge_staff")]
    public string SessionFullJudgeStaff { get; set; }

    [Column("session_full_staff")]
    public string SessionFullStaff { get; set; }



  }


}
