// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// FZL
    /// Данни за лице независимо дали е в ролята на субект
    /// или пострадал
    /// </summary>
    public class EisppPerson
    {
        /// <summary>
        /// fzlsid
        /// Идентификатор на физическо лице
        /// Използва се с префикс 'f' (f34)
        /// Прави връзка с останалите асти на XML пакета
        /// човек - престъпление например
        /// </summary>
        [XmlAttribute("fzlsid")]
        public int PersonId { get; set; }
        [XmlIgnore]
        public long? PersonSourceId { get; set; }
        /// <summary>
        /// fzlgrjbgr
        /// Гражданство
        /// Номенклатура nmk_grj
        /// </summary>
        [Display(Name= "Гражданство")]
        [XmlAttribute("fzlgrjbgr")]
        public int CitizenshipBg { get; set; }

        /// <summary>
        /// fzlgrjchj
        /// Друго гражданство
        /// Номенклатура nmk_grj
        /// </summary>
        [XmlAttribute("fzlgrjchj")]
        public int OtherCitizenship { get; set; }

        /// <summary>
        /// fzlpol
        /// Пол
        /// Номенклатура nmk_fzlpol
        /// </summary>
        [Display(Name= "Пол")]
        [XmlAttribute("fzlpol")]
        public int Gender { get; set; }

        /// <summary>
        /// fzlegn
        /// ЕГН
        /// </summary>
        [Display(Name= "ЕГН")]
        [XmlAttribute("fzlegn")]
        public string Egn { get; set; }

        /// <summary>
        /// fzllnc
        /// ЛНЧ
        /// </summary>
        [Display(Name= "ЛНЧ")]
        [XmlAttribute("fzllnc")]
        public string Lnch { get; set; }

        /// <summary>
        /// fzlime
        /// Име
        /// </summary>
        [Display(Name= "Име")]
        [XmlAttribute("fzlime")]
        public string FirstName { get; set; }

        /// <summary>
        /// fzlprz
        /// Презиме
        /// </summary>
        [Display(Name= "Презиме")]
        [XmlAttribute("fzlprz")]
        public string SecondName { get; set; }

        /// <summary>
        /// fzlfma
        /// Фамилия
        /// </summary>
        [Display(Name= "Фамилия")]
        [XmlAttribute("fzlfma")]
        public string LastName { get; set; }

        /// <summary>
        /// fzlimecyr
        /// Имена на кирилица
        /// </summary>
        [Display(Name= "Имена на кирилица")]
        [XmlAttribute("fzlimecyr")]
        public string FullNameCyr { get; set; }

        /// <summary>
        /// fzlimelat
        /// Имена на латиница
        /// </summary>
        [Display(Name= "Имена на латиница")]
        [XmlAttribute("fzlimelat")]
        public string FullNameLat { get; set; }

        /// <summary>
        /// fzldtarjd
        /// Дата на раждане
        /// </summary>
        [Display(Name = "Дата на раждане")]
        [XmlAttribute("fzldtarjd", DataType = "date")]
        public DateTime BirthDate { get; set; }


        /// <summary>
        /// fzldtapnl
        /// Дата, на която е починал
        /// </summary>
        [XmlAttribute("fzldtapnl", DataType = "date")]
        public DateTime DeathDate { get; set; }

        /// <summary>
        /// MPP
        /// Мярка за процесуална принуда
        /// </summary>
        [XmlElement("MPP")]
        public ProceduralCoercionMeasure[] Measures { get; set; }

        /// <summary>
        /// ZDJ
        /// Задържане на лице
        /// </summary>
        [XmlElement("ZDJ")]
        public PersonDetention Detention { get; set; }

        /// <summary>
        /// IZPNKZ
        /// Изпълнение на наказание
        /// </summary>
        [XmlElement("IZPNKZ")]
        public PunishmentExecution PunishmentExecution { get; set; }

        /// <summary>
        /// NKZ
        /// Наказание
        /// </summary>
        [XmlElement("NKZ")]
        public Punishment[] Punishments { get; set; }

        /// <summary>
        /// NPRFZLSTA
        /// Статус на НП за конкретно лице
        /// </summary>
        [XmlElement("NPRFZLSTA")]
        public PersonCriminalProceeding PersonCPStatus { get; set; }

        /// <summary>
        /// ADR
        /// Адреси на физическо лице
        /// </summary>
        [XmlElement("ADR")]
        public EisppAddress[] Addresses { get; set; }

        /// <summary>
        /// DKS
        /// Документ за самоличност
        /// </summary>
        [XmlElement("DKS")]
        public PersonalDocument[] PersonalDocuments { get; set; }

        /// <summary>
        /// MRD
        /// Месторождение
        /// </summary>
        [XmlElement("MRD")]
        public BirthPlace BirthPlace { get; set; }

        /// <summary>
        /// FZLOPI
        /// Описание на физическо лице
        /// </summary>
        [XmlElement("FZLOPI")]
        public PersonDescription[] PersonDescriptions { get; set; }

        /// <summary>
        /// Дата, която трябва да се игнорира
        /// </summary>
        public static readonly DateTime defaultDate = default;

        /// <summary>
        /// Игнорира празна дата
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeBirthDate()
        {
            return BirthDate != defaultDate;
        }

        /// <summary>
        /// Игнорира празна дата
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeDeathDate()
        {
            return DeathDate != defaultDate;
        }
        [XmlIgnore]
        public int CasePersonId { get; set; }
        
        [XmlIgnore]
        [Display(Name = "Физическо лице")]
        public bool IsSelected { get; set; }
        
        [XmlIgnore]
        public bool IsSelectedReadOnly { get; set; } = false;

        [XmlIgnore]
        [Display(Name = "Гражданство")]
        public int CitizenshipVM
        {
            get
            {
                return CitizenshipBg > 0 ? CitizenshipBg : OtherCitizenship;
            }
            set
            {
                CitizenshipBg = 0;
                OtherCitizenship = 0;
                if (value == EISPPConstants.CountryBG)
                {
                    CitizenshipBg = value;
                } else
                {
                    OtherCitizenship = value;
                }
            }
        }
        [XmlIgnore]
        public bool IsBgCitizen
        {
            get
            {
                return CitizenshipBg > 0;
            }
        }

        [XmlIgnore]
        [Display(Name = "Дата на раждане")]
        public DateTime? BirthDateVM
        {
            get
            {
                return BirthDate > defaultDate ? (DateTime?)BirthDate : (DateTime?)null;
            }
            set
            {
                BirthDate = value ?? defaultDate;
            }
        }
    }
}