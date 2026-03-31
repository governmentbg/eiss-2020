using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CasePersonSentenceBulletinEditVM
    {
        public int Id { get; set; }
        public int? CourtId { get; set; }
        public int? CaseId { get; set; }
        public int CasePersonId { get; set; }
        public int CaseTypeId { get; set; }

        [Display(Name = "Издаваща държава")]
        public int? IssuingCountryId { get; set; }


        [Display(Name = "Месторождение")]
        public string BirthDayPlace { get; set; }

        [Display(Name = "Месторождение държава")]
        public int? BirthDayPlaceCountryId { get; set; }

        [Display(Name = "Месторождение населено място")]
        public int? BirthDayPlaceCityId { get; set; }

        [Display(Name = "Описание, в случай на друго - кирилица")]
        public string BirthDayPlaceCityText { get; set; }

        /// <summary>
        /// Описание, в случай на друго - кирилица
        /// </summary>
        [Display(Name = "Описание, в случай на друго - кирилица")]
        public string BirthDayPlaceDescriptionCir { get; set; }

        /// <summary>
        /// Описание, в случай на друго - латиница
        /// </summary>
        [Display(Name = "Описание, в случай на друго - латиница")]
        public string BirthDayPlaceDescriptionLat { get; set; }

        [Display(Name = "Дата на раждане")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime BirthDay { get; set; }

        [Display(Name = "Гражданство")]
        public string Nationality { get; set; }

        [Display(Name = "Гражданство")]
        public int? NationalityCountryOneId { get; set; }

        [Display(Name = "Гражданство - второ")]
        public int? NationalityCountryTwoId { get; set; }
      
        [Display(Name = "Име на бащата - кирилица")]
        public string FatherName { get; set; }

        [Display(Name = "Име на бащата - латиница")]
        public string FatherNameLatin { get; set; }

        [Display(Name = "Име на майката - кирилица")]
        public string MotherName { get; set; }

        [Display(Name = "Име на майката - латиница")]
        public string MotherNameLatin { get; set; }

        [Display(Name = "по чл.78а НК")]
        public bool? IsAdministrativePunishment { get; set; }

        [Display(Name = "Присъда")]
        [AllowHtml]
        public string SentenceDescription { get; set; }

        [Display(Name = "Осъждан")]
        public bool IsConvicted { get; set; }

        [Display(Name = "Съдия")]
        public int LawUnitSignId { get; set; }

        [Display(Name = "AFIS номер")]
        public string NumberAFIS { get; set; }

        /// <summary>
        /// Чужд идентификатор
        /// </summary>
        [Display(Name = "Чужд идентификатор")]
        public string OtherUic { get; set; }

        /// <summary>
        /// Издаваща държава
        /// </summary>
        [Display(Name = "Издаваща държава - чужд идентификатор")]
        public int? OtherUicIssuingCountryId { get; set; }

        /// <summary>
        /// Наименование - населено място
        /// </summary>
        [Display(Name = "Наименование - чужд идентификатор")]
        public int? OtherUicPlaceCityId { get; set; }
    }
}
