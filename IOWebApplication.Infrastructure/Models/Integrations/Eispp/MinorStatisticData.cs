// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// NPL
    /// Статистически данни за непълнолетен
    /// </summary>
    public class MinorStatisticData
    {
		/// <summary>
		/// nplalk
		/// Употреба на алкохол
		/// Номенклатура nmk_nplalk
		/// </summary>
		[XmlAttribute("nplalk")]
		public int AlcoholConsumption { get; set; }

		/// <summary>
		/// nplbnz
		/// Състояние на безнадзорност
		/// Номенклатура nmk_nplbnz
		/// </summary>
		[XmlAttribute("nplbnz")]
		public int NonSupervised { get; set; }

		/// <summary>
		/// npljiv
		/// Лицето живее
		/// Номенклатура nmk_npljiv
		/// </summary>
		[XmlAttribute("npljiv")]
		public int PersonLives { get; set; }

		/// <summary>
		/// npllekpsx
		/// Лекувано в психиатрично заведение
		/// Номенклатура nmk_npllekpsx
		/// </summary>
		[XmlAttribute("npllekpsx")]
		public int PsychiatricInstitutionPatient { get; set; }

		/// <summary>
		/// npllrknrk
		/// Лекувано в специализирано заведение за лечение на наркомании
		/// Номенклатура nmk_npllrknrk
		/// </summary>
		[XmlAttribute("npllrknrk")]
		public int DrugClinicPatient { get; set; }

		/// <summary>
		/// nplmlt
		/// Малтретиране на лицето
		/// Номенклатура nmk_nplmlt
		/// </summary>
		[XmlAttribute("nplmlt")]
		public int ChildAbuse { get; set; }

		/// <summary>
		/// nplmltot
		/// Малтретирано от кого
		/// Номенклатура nmk_nplmltot
		/// </summary>
		[XmlAttribute("nplmltot")]
		public int AbusedBy { get; set; }

		/// <summary>
		/// nplnkzpdm
		/// Изтърпява наказание в ПД
		/// Номенклатура nmk_nplnkzpdm
		/// </summary>
		[XmlAttribute("nplnkzpdm")]
		public int ServingSentance { get; set; }

		/// <summary>
		/// nplnrk
		/// Употреба на наркотични вещества
		/// Номенклатура nmk_nplnrk
		/// </summary>
		[XmlAttribute("nplnrk")]
		public int DrugsUse { get; set; }

		/// <summary>
		/// nplnrkvid
		/// Вид на употребените наркотични вещества
		/// Номенклатура nmk_nplnrkvid
		/// </summary>
		[XmlAttribute("nplnrkvid")]
		public int UsedDrugsType { get; set; }

		/// <summary>
		/// nplotc
		/// Водено на отчет в ДПС
		/// Номенклатура nmk_nplotc
		/// </summary>
		[XmlAttribute("nplotc")]
		public int Reported { get; set; }

		/// <summary>
		/// nplpneprc
		/// Причини за извършване на престъплението
		/// Номенклатура nmk_nplpneprc
		/// </summary>
		[XmlAttribute("nplpneprc")]
		public int CrimeReason { get; set; }

		/// <summary>
		/// nplpro
		/// Проституиране
		/// Номенклатура nmk_nplpro
		/// </summary>
		[XmlAttribute("nplpro")]
		public int Prostitution { get; set; }

		/// <summary>
		/// nplprs
		/// Проси
		/// Номенклатура nmk_nplprs
		/// </summary>
		[XmlAttribute("nplprs")]
		public int Begging { get; set; }

		/// <summary>
		/// nplrzpnrk
		/// Разпространение на наркотични вещества
		/// Номенклатура nmk_nplrzpnrk
		/// </summary>
		[XmlAttribute("nplrzpnrk")]
		public int DrugsDistribution { get; set; }

		/// <summary>
		/// nplsid
		/// Системен идентификатор
		/// </summary>
		[XmlAttribute("nplsid")]
		public int MinorStatisticId { get; set; }

		/// <summary>
		/// nplskt
		/// Влияние на пълнолетно лице за извършване на престъплението
		/// Номенклатура nmk_da_ne
		/// </summary>
		[XmlAttribute("nplskt")]
		public int AdultInfluence { get; set; }

		/// <summary>
		/// nplspi
		/// Настанено в СПИ
		/// Номенклатура nmk_nplspi
		/// </summary>
		[XmlAttribute("nplspi")]
		public int AccommodatedInSocialSchool { get; set; }

		/// <summary>
		/// nplucl
		/// Училищна заетост
		/// Номенклатура nmk_nplucl
		/// </summary>
		[XmlAttribute("nplucl")]
		public int SchoolActivity { get; set; }

		/// <summary>
		/// npluclprc
		/// Причини да не учи
		/// Номенклатура nmk_npluclprc
		/// </summary>
		[XmlAttribute("npluclprc")]
		public int ReasonsNotToStudy { get; set; }

		/// <summary>
		/// nplusljiv
		/// Условия на живот
		/// Номенклатура nmk_nplusljiv
		/// </summary>
		[XmlAttribute("nplusljiv")]
		public int LivingConditions { get; set; }

		/// <summary>
		/// nplvmk
		/// Наложена възпитателна мярка от ЗБППМН
		/// Номенклатура nmk_nplvmk
		/// </summary>
		[XmlAttribute("nplvmk")]
		public int MeasureImposed { get; set; }

		/// <summary>
		/// nplvui
		/// Настанено във ВУИ
		/// Номенклатура nmk_nplvui
		/// </summary>
		[XmlAttribute("nplvui")]
		public int AccommodatedInEducationalInstitution { get; set; }

		/// <summary>
		/// nplxms
		/// С хомосексуални прояви
		/// Номенклатура nmk_nplxms
		/// </summary>
		[XmlAttribute("nplxms")]
		public int Homosexual { get; set; }

		/// <summary>
		/// nplzdrste
		/// Здравословно състояние на лицето
		/// Номенклатура nmk_nplzdrste
		/// </summary>
		[XmlAttribute("nplzdrste")]
		public int MedicalCondition { get; set; }
	}
}