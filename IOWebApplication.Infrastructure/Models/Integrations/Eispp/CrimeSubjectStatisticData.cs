// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// SBC
    /// Статистически данни за субект на престъпление
    /// </summary>
    public class CrimeSubjectStatisticData
    {
		/// <summary>
		/// sbcetn
		/// Етническа група
		/// Номенклатура nmk_sbcetn 314
		/// </summary>
		[Display(Name = "Етническа група")]
		[XmlAttribute("sbcetn")]
		public int ЕthnicGroup { get; set; }

		/// <summary>
		/// sbcgrj
		/// Гражданство на субект на престъпление
		/// </summary>
		[Display(Name = "Гражданство")]
		[XmlAttribute("sbcgrj")]
		public int Citizenship { get; set; }

		/// <summary>
		/// sbcobr
		/// Образование
		/// Номенклатура nmk_sbcobr 311
		/// </summary>
		[Display(Name = "Образование")]
		[XmlAttribute("sbcobr")]
		public int Education { get; set; }

		/// <summary>
		/// sbcple
		/// Пълнолетие
		/// Номенклатура nmk_sbcple 309
		/// </summary>
		[Display(Name = "Пълнолетие")]
		[XmlAttribute("sbcple")]
		public int LawfulAge { get; set; }

		/// <summary>
		/// sbcrcd
		/// Рецидив
		/// Номенклатура nmk_sbcrcd 308
		/// </summary>
		[Display(Name = "Рецидив")]
		[XmlAttribute("sbcrcd")]
		public int Relaps { get; set; }

		/// <summary>
		/// sbcsid
		/// Системен идентификатор
		/// </summary>
		[XmlAttribute("sbcsid")]
		public int SubjectStatisticDataId { get; set; }

		/// <summary>
		/// sbcspj
		/// Семейно положение
		/// Номенклатура nmk_sbcspj 310
		/// </summary>
		[Display(Name = "Семейно положение")]
		[XmlAttribute("sbcspj")]
		public int MeritalStatus { get; set; }

		/// <summary>
		/// sbctrd
		/// Трудова активност
		/// Номенклатура nmk_sbctrd 312
		/// </summary>
		[Display(Name = "Трудова активност")]
		[XmlAttribute("sbctrd")]
		public int LaborActivity { get; set; }

		/// <summary>
		/// sbcznq
		/// Занятие
		/// Номенклатура nmk_sbcznq  nmk_fzlpne_znt 1504
		/// </summary>
		[Display(Name = "Занятие")]
		[XmlAttribute("sbcznq")]
		public int Occupation { get; set; }

		/// <summary>
		/// sbcrge
		/// Предишни регистрации
		/// Номенклатура nmk_sbcrge 12478
		/// </summary> 
		[Display(Name = "Предишни регистрации")]
		[XmlAttribute("sbcrge")]
		public int FormerRegistrations { get; set; }
	}
}