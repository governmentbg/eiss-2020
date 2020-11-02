// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// ADR
    /// Адрес/Място
    /// </summary>
    public class EisppAddress
    {
		/// <summary>
		/// adrsid
		/// Системен идентификатор
		/// </summary>
		[XmlAttribute("adrsid")]
		public int AddressId { get; set; }

		/// <summary>
		/// adrtip
		/// Тип на адрес
		/// Номенклатура nmk_adrtip
		/// </summary>
        [Display(Name= "Тип на адрес")]
		[XmlAttribute("adrtip")]
		public int AddressType { get; set; }

		/// <summary>
		/// adrdrj
		/// Държава на адрес/място
		/// Номенклатура nmk_grj
		/// </summary>
        [Display(Name= "Държава на адрес/място")]
		[XmlAttribute("adrdrj")]
		public int Country { get; set; }

		/// <summary>
		/// adrnsmbgr
		/// Населено място в РБ
		/// Номенклатура nmk_nas_miasto
		/// </summary>
        [Display(Name= "Населено място в РБ")]
		[XmlAttribute("adrnsmbgr")]
		public int SettlementBg { get; set; }

		/// <summary>
		/// adrrjn
		/// Район
		/// Номенклатура nmk_raion
		/// </summary>
		[XmlAttribute("adrrjn")]
		public int Region { get; set; }

		/// <summary>
		/// adrpstkod
		/// Пощенски код
		/// </summary>
		[XmlAttribute("adrpstkod")]
		public int PostCode { get; set; }

		/// <summary>
		/// adrkrdkod
		/// Код на наименованието на улица
		/// </summary>
		[XmlAttribute("adrkrdkod")]
		public int StreetCode { get; set; }

		/// <summary>
		/// adrmstvid
		/// Конкретно място
		/// Номенклатура nmk_adrmstvid
		/// </summary>
		[XmlAttribute("adrmstvid")]
		public int Place { get; set; }

		/// <summary>
		/// adrkodpdl
		/// Код на поделение
		/// Номенклатура nmk_strvid
		/// </summary>
		[XmlAttribute("adrkodpdl")]
		public int Division { get; set; }

		/// <summary>
		/// adrnsmchj
		/// Населено място в чужда държава
		/// </summary>
        [Display(Name= "Населено място в чужда държава")]
		[XmlAttribute("adrnsmchj")]
		public string SettlementAbroad { get; set; }

		/// <summary>
		/// adrkrdtxt
		/// Наименование на улица
		/// </summary>
        [Display(Name= "Наименование на улица")]
		[XmlAttribute("adrkrdtxt")]
		public string StreetName { get; set; }

		/// <summary>
		/// adrnmr
		/// Номер
		/// </summary>
        [Display(Name= "Номер")]
		[XmlAttribute("adrnmr")]
		public string Number { get; set; }

		/// <summary>
		/// adrblk
		/// Блок
		/// </summary>
        [Display(Name= "Блок")]
		[XmlAttribute("adrblk")]
		public string Building { get; set; }

		/// <summary>
		/// adrvhd
		/// Вход
		/// </summary>
        [Display(Name= "Вход")]
		[XmlAttribute("adrvhd")]
		public string Entrance { get; set; }

		/// <summary>
		/// adretj
		/// Етаж
		/// </summary>
        [Display(Name= "Етаж")]
		[XmlAttribute("adretj")]
		public string Floor { get; set; }

		/// <summary>
		/// adrapr
		/// Апартамент
		/// </summary>
        [Display(Name= "Апартамент")]
		[XmlAttribute("adrapr")]
		public string Appartment { get; set; }

		/// <summary>
		/// adrmsttxt
		/// Описание на мястото
		/// </summary>
        [Display(Name= "Описание на мястото")]
		[XmlAttribute("adrmsttxt")]
		public string Description { get; set; }

		/// <summary>
		/// adrloc
		/// Локализация на място
		/// Номенклатура nmk_adrloc
		/// </summary>
		[XmlAttribute("adrloc")]
		public int Localization { get; set; }
	}
}