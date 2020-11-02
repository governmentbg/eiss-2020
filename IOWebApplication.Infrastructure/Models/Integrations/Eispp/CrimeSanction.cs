// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// SCQ
    /// Процесуална санкция за престъпление
    /// </summary>
    public class CrimeSanction
    {
		/// <summary>
		/// scqdta
		/// Дата на постановяване на процесуалната санкция
		/// </summary>
		[Display(Name = "Дата процесуалната санкция")]
		[XmlAttribute("scqdta", DataType = "date")]
		public DateTime SanctionDate { get; set; }

		[XmlIgnore]
		[Display(Name = "Дата процесуалната санкция")]
		public DateTime? SanctionDateVM
		{
			get
			{
				return SanctionDate > defaultDate ? (DateTime?)SanctionDate : (DateTime?)null;
			}
			set
			{
				SanctionDate = value ?? defaultDate;
			}
		}
	

		/// <summary>
		/// scqosn
		/// Основание за процесуална санкция
		/// Номенклатура nmk_osn_adm_nkz
		/// </summary>
		[Display(Name = "Основание за процесуална санкция")]
		[XmlAttribute("scqosn")]
		public int SanctionReason { get; set; }

		/// <summary>
		/// scqrlq в САС и XSD
		/// fzlpnerlq в Описание на обектите
		/// Номенклатура nmk_scqrlq / nmk_fzlpnerlq
		/// </summary>
		[XmlAttribute("scqrlq")]
		[Display(Name = "Роля на лицето")]
		public int Role { get; set; }

		/// <summary>
		/// scqsid
		/// Системен Идентификатор
		/// </summary>
		[XmlAttribute("scqsid")]
		public int CrimeSanctionId { get; set; }

		/// <summary>
		/// scqstr
		/// Структура, постановила процесуалната санкция
		/// Номенклатура nmk_strvid
		/// </summary>
		[XmlAttribute("scqstr")]
		public int StructureId { get; set; }

		/// <summary>
		/// scqstv
		/// Състав по НК
		/// </summary>
		[XmlAttribute("scqstv")]
		public string SanctionConstitution { get; set; }

		/// <summary>
		/// scqvid
		/// Вид процесуална санкция
		/// Номенклатура nmk_scqvid
		/// задължително
		/// </summary>
		[XmlAttribute("scqvid")]
		[Display(Name = "Вид процесуална санкция")]
		public int SanctionType { get; set; }

		/// <summary>
		/// NKZPNE
		/// Наказание за престъпление
		/// </summary>
		[XmlElement("NKZPNE")]
		public CrimePunishment[] CrimePunishments { get; set; }

		/// <summary>
		/// PRVKCQ
		/// Правна квалификация
		/// </summary>
		[XmlElement("PRVKCQ")]
		public LegalQualification[] LegalQualifications { get; set; }

		/// <summary>
		/// Дата, която трябва да се игнорира
		/// </summary>
		private static readonly DateTime defaultDate = default;

		/// <summary>
		/// Игнорира празна дата
		/// </summary>
		/// <returns></returns>
		public bool ShouldSerializeSanctionDate()
		{
			return SanctionDate != defaultDate;
		}
	}
}
