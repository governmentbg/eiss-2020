// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// NFL
    /// Юридическо лице
    /// </summary>
    public class LegalEntity
    {
		/// <summary>
		/// nfldrj
		/// Държава на регистрация на НФЛ
		/// Номенклатура nmk_grj
		/// </summary>
		[XmlAttribute("nfldrj")]
		public int Country { get; set; }

		/// <summary>
		/// nfleik
		/// ЕИК (номер по БУЛСТАТ)
		/// задължително
		/// </summary>
		[XmlAttribute("nfleik")]
		[Display(Name = "ЕИК/БУЛСТАТ")]
		public string RegistrationNumber { get; set; }

		/// <summary>
		/// nflgrp
		/// Група на на НФЛ
		/// Номенклатура nmk_nflgrp
		/// </summary>
		[XmlAttribute("nflgrp")]
		[Display(Name = "Група на НФЛ")]
		public int EntityGroup { get; set; }

		/// <summary>
		/// nfljrdstt
		/// Юридически статут на НФЛ
		/// Номенклатура nmk_nfljrdstt
		/// </summary>
		[XmlAttribute("nfljrdstt")]
		[Display(Name = "Юридически статут")]
		public int EntityStatus { get; set; }

		/// <summary>
		/// nflplnnme
		/// Пълно наименование на НФЛ
		/// </summary>
		[XmlAttribute("nflplnnme")]
		[Display(Name = "Наименование")]
		public string FullName { get; set; }

		/// <summary>
		/// nflsid
		/// Системен идентификатор на Юридическо лице
		/// </summary>
		[XmlAttribute("nflsid")]
		public int EntityId { get; set; }

		/// <summary>
		/// nflskrnme
		/// Съкратено наименование на НФЛ
		/// </summary>
		[XmlAttribute("nflskrnme")]
		public string ShortName { get; set; }

		/// <summary>
		/// nflvid
		/// Вид на НФЛ
		/// Номенклатура nmk_nflvid
		/// </summary>
		[XmlAttribute("nflvid")]
		[Display(Name = "Вид на НФЛ")]
		public int EntityType { get; set; }

		/// <summary>
		/// ADR
		/// Адрес/Място
		/// </summary>
		[XmlElement("ADR")]
		public EisppAddress[] Addresses { get; set; }
	}
}