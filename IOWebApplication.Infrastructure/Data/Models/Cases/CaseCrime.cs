using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Престъпления по НД по дело
    /// </summary>
    [Table("case_crimes")]
    public class CaseCrime : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Display(Name = "id от ЕИСПП")]
        [Column("eispp_id")]
        public string EISSId { get; set; }

        [Column("eispp_number")]
        [Display(Name = "Код по ЕИСПП")]
        [RegularExpression("[А-Я]{3}[0-9]{8}[А-Б]{1}[А-Я]{2}", ErrorMessage = "Невалиден {0}.")]
        [Remote(action: "VerifyEISSPNumber", controller: "CasePersonSentence")]
        public string EISSPNumber { get; set; }

        /// <summary>
        /// от NomEisppTblElements, table code = eiss_pne
        /// </summary>
        [Column("crime_code")]
        [Display(Name = "Вид престъпление")]
        public string CrimeCode { get; set; }

        [Column("crime_name")]
        [Display(Name = "Престъпление")]
        public string CrimeName { get; set; }

        
        [Column("start_date_type")]
        [Display(Name = "Тип на дата на престъпление")]
        public int? StartDateType { get; set; }

        [Column("date_from")]
        [Display(Name = "Срок от")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "Срок до")]
        public DateTime? DateTo { get; set; }

        [Column("completition_degree")]
        [Display(Name = "Степен на довършеност")]
        public int? CompletitionDegree { get; set; }

        [Display(Name = "Статус")]
        [Column("status")]
        public int? Status { get; set; }

        [Column("status_date")]
        [Display(Name = "Дата на статус")]
        public DateTime? StatusDate { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        public virtual ICollection<CasePersonCrime> CasePersonCrimes { get; set; }


        //################################################################################
        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }
    }
}
