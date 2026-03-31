using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.EISPP;
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
    public class CaseCrime : UserDateWRT, IExpiredInfo, IHaveId
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
        [Display(Name = "Категория деяние")]
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

        /// <summary>
        /// дали ЕИСПП Номера е генериран в ЕИСС
        /// </summary>
        [Column("is_generated_eispp_number")]
        [Display(Name = "Генериране на номер")]
        public bool? IsGeneratedEisppNumber { get; set; }

        /// <summary>
        /// Категория деяние
        /// </summary>
        [Column("category_deed_id")]
        [Display(Name = "Категория деяние")]
        public int? CategoryDeedId {  get; set; }

        /// <summary>
        /// Категория обща деяние
        /// </summary>
        [Column("category_common_ceed_id")]
        [Display(Name = "Обща категория")]
        public int? CategoryCommonDeedId { get; set; }

        /// <summary>
        /// Правна квалификация
        /// </summary>
        [Column("legal_qualification_text")]
        [Display(Name = "Правна квалификация")]
        public string? LegalQualificationText { get; set; }

        /// <summary>
        /// Форма на вината
        /// </summary>
        [Column("form_guilt_id")]
        [Display(Name = "Форма на вината")]
        public int? FormGuiltId { get; set; }

        /// <summary>
        /// Постановено изтърпяване на предходна условна присъда
        /// </summary>
        [Column("has_prior_probation")]
        [Display(Name = "Постановено изтърпяване на предходна условна присъда")]
        public bool? HasPriorProbation {  get; set; }

        /// <summary>
        /// Номер на акта на предходната условна присъда
        /// </summary>
        [Column("act_prior_probation")]
        [Display(Name = "Номер на акта на предходната условна присъда")]
        public string? ActPriorProbation { get; set; }

        /// <summary>
        /// Място на престъплението държава
        /// </summary>
        [Column("crime_scene_country_id")]
        [Display(Name = "Държава")]
        public int? CrimeSceneCountryId { get; set; }

        /// <summary>
        /// Място на престъплението населено място
        /// </summary>
        [Column("crime_scene_city_id")]
        [Display(Name = "Населено място")]
        public int? CrimeSceneCityId { get; set; }

        /// <summary>
        /// Място на престъплението населено място - от ЕИСПП
        /// </summary>
        [Column("crime_scene_city_eispp_id")]
        [Display(Name = "Населено място")]
        public int? CrimeSceneCityEisppId { get; set; }

        /// <summary>
		/// Населено място в чужда държава
		/// </summary>
        [Column("crime_scene_settlement_abroad")]
        [Display(Name = "Населено място в чужда държава")]
        public string CrimeSceneSettlementAbroad { get; set; }

        /// <summary>
		/// Наименование на улица
		/// </summary>
        [Display(Name = "Наименование на улица")]
        [Column("crime_scene_street_name")]
        public string CrimeSceneStreetName { get; set; }

        /// <summary>
        /// Номер
        /// </summary>
        [Display(Name = "Номер")]
        [Column("crime_scene_number")]
        public string CrimeSceneNumber { get; set; }

        /// <summary>
        /// Блок
        /// </summary>
        [Display(Name = "Блок")]
        [Column("crime_scene_building")]
        public string CrimeSceneBuilding { get; set; }

        /// <summary>
        /// Вход
        /// </summary>
        [Display(Name = "Вход")]
        [Column("crime_scene_entrance")]
        public string CrimeSceneEntrance { get; set; }

        /// <summary>
        /// Етаж
        /// </summary>
        [Display(Name = "Етаж")]
        [Column("crime_scene_floor")]
        public string CrimeSceneFloor { get; set; }

        /// <summary>
        /// Апартамент
        /// </summary>
        [Display(Name = "Апартамент")]
        [Column("crime_scene_appartment")]
        public string CrimeSceneAppartment { get; set; }

        /// <summary>
        /// Локализация на място
        /// Номенклатура nmk_adrloc
        /// </summary>
        [Column("crime_scene_localization")]
        [Display(Name = "Локализация на място")]
        public int CrimeSceneLocalization { get; set; }

        /// <summary>
        /// Място на престъплението описание
        /// </summary>
        [Column("crime_scene_text")]
        [Display(Name = "Описание")]
        public string? CrimeSceneText { get; set; }

        /// <summary>
        /// Описание на деянието
        /// </summary>
        [Column("description_offence")]
        [Display(Name = "Описание на деянието")]
        public string? DescriptionOffence { get; set; }

        [ForeignKey(nameof(CrimeSceneCityId))]
        public virtual EkEkatte CrimeSceneCity { get; set; }

        [ForeignKey(nameof(CrimeSceneCityEisppId))]
        public virtual EisppEktteCode CrimeSceneCityEispp { get; set; }

        [ForeignKey(nameof(CrimeSceneCountryId))]
        public virtual EkCountry CrimeSceneCountry { get; set; }

        [ForeignKey(nameof(CategoryDeedId))]
        public virtual CategoryDeed CategoryDeed { get; set; }

        [ForeignKey(nameof(CategoryCommonDeedId))]
        public virtual CategoryCommonDeed CategoryCommonDeed { get; set; }

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
