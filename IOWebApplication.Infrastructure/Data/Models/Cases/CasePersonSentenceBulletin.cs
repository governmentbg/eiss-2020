using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Бюлетин за съдимост за лице
    /// </summary>
    [Table("case_person_sentence_bulletin")]
    public class CasePersonSentenceBulletin : UserDateWRT, IHaveId
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_person_id")]
        public int CasePersonId { get; set; }

        [Column("birth_day_place")]
        [Display(Name = "Месторождение")]
        public string BirthDayPlace { get; set; }

        [Column("birth_day")]
        [Display(Name = "Дата на раждане")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime BirthDay { get; set; }

        [Column("nationality")]
        [Display(Name = "Гражданство")]
        public string Nationality { get; set; }

        [Column("family_marriage")]
        [Display(Name = "Фамилно име придобито при сключване на брак")]
        public string FamilyMarriage { get; set; }

        [Column("father_name")]
        [Display(Name = "Име на бащата - кирилица")]
        public string FatherName { get; set; }

        /// <summary>
        /// Име на бащата - латиница
        /// </summary>
        [Column("father_name_latin")]
        [Display(Name = "Име на бащата - латиница")]
        public string? FatherNameLatin { get; set; }

        [Column("mother_name")]
        [Display(Name = "Име на майката - кирилица")]
        public string MotherName { get; set; }

        /// <summary>
        /// Име на майката - латиница
        /// </summary>
        [Column("mother_name_latin")]
        [Display(Name = "Име на майката - латиница")]
        public string? MotherNameLatin { get; set; }

        [Column("out_document_id")]
        public long? OutDocumentId { get; set; }

        [Column("is_administrative_punishment")]
        [Display(Name = "по чл.78а НК")]
        public bool? IsAdministrativePunishment { get; set; }

        [Column("sentence_description")]
        [Display(Name = "Присъда")]
        public string SentenceDescription { get; set; }

        [Column("is_convicted")]
        [Display(Name = "Осъждан")]
        public bool? IsConvicted { get; set; }

        [Column("lawunit_sign_id")]
        [Display(Name = "Съдия")]
        public int? LawUnitSignId { get; set; }

        /// <summary>
        /// Издаваща държава
        /// </summary>
        [Column("issuing_country_id")]
        [Display(Name = "Издаваща държава")]
        public int? IssuingCountryId { get; set; }

        /// <summary>
        /// Месторождение държава
        /// </summary>
        [Column("birth_day_place_country_id")]
        [Display(Name = "Месторождение държава")]
        public int? BirthDayPlaceCountryId { get; set; }

        /// <summary>
        /// Месторождение населено място
        /// </summary>
        [Column("birth_day_place_city_id")]
        [Display(Name = "Месторождение населено място")]
        public int? BirthDayPlaceCityId { get; set; }

        /// <summary>
        /// Описание, в случай на друго - кирилица
        /// </summary>
        [Column("birth_day_place_description_cir")]
        [Display(Name = "Описание, в случай на друго - кирилица")]
        public string? BirthDayPlaceDescriptionCir { get; set; }

        /// <summary>
        /// Описание, в случай на друго - латиница
        /// </summary>
        [Column("birth_day_place_description_lat")]
        [Display(Name = "Описание, в случай на друго - латиница")]
        public string? BirthDayPlaceDescriptionLat { get; set; }

        /// <summary>
        /// Месторождение населено място
        /// </summary>
        [Column("birth_day_place_city_text")]
        [Display(Name = "Месторождение населено място")]
        public string? BirthDayPlaceCityText { get; set; }

        /// <summary>
        /// AFIS номер
        /// </summary>
        [Column("number_afis")]
        [Display(Name = "AFIS номер")]
        public string? NumberAFIS { get; set; }

        /// <summary>
        /// Гражданство
        /// </summary>
        [Column("nationality_country_one_id")]
        [Display(Name = "Гражданство")]
        public int? NationalityCountryOneId { get; set; }

        /// <summary>
        /// Гражданство
        /// </summary>
        [Column("nationality_country_two_id")]
        [Display(Name = "Гражданство")]
        public int? NationalityCountryTwoId { get; set; }

        /// <summary>
        /// Чужд идентификатор
        /// </summary>
        [Column("other_uic")]
        [Display(Name = "Чужд идентификатор")]
        public string? OtherUic { get; set; }

        /// <summary>
        /// Издаваща държава
        /// </summary>
        [Column("other_uic_issuing_country_id")]
        [Display(Name = "Издаваща държава")]
        public int? OtherUicIssuingCountryId { get; set; }

        /// <summary>
        /// Наименование - населено място
        /// </summary>
        [Column("other_uic_place_city_id")]
        [Display(Name = "Наименование")]
        public int? OtherUicPlaceCityId { get; set; }

        [Column("last_generated_date")]
        public DateTime? LastGeneratedDate { get; set; }

        [ForeignKey(nameof(IssuingCountryId))]
        public virtual EkCountry IssuingCountry { get; set; }

        [ForeignKey(nameof(BirthDayPlaceCountryId))]
        public virtual EkCountry BirthDayPlaceCountry { get; set; }

        [ForeignKey(nameof(BirthDayPlaceCityId))]
        public virtual EkEkatte BirthDayPlaceCity { get; set; }

        [ForeignKey(nameof(NationalityCountryOneId))]
        public virtual EkCountry NationalityCountryOne { get; set; }

        [ForeignKey(nameof(NationalityCountryTwoId))]
        public virtual EkCountry NationalityCountryTwo { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }

        [ForeignKey(nameof(OutDocumentId))]
        public virtual Document OutDocument { get; set; }

        [ForeignKey(nameof(LawUnitSignId))]
        public virtual LawUnit LawUnitSign { get; set; }

        public virtual ICollection<CasePersonSentenceBulletinFile> ExportFiles { get; set; }
    }
}
