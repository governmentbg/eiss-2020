// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// NPRFZLPNE
    /// Връзка между Наказателно производство, лице и престъпление
    /// </summary>
    public class CPPersonCrime
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

		/// <summary>
		/// pnesid
		/// Системен идентификатор
		/// Използва се с префикс 'p' (p34)
		/// Прави връзка с останалите асти на XML пакета
		/// човек - престъпление например
		/// </summary>
		[XmlAttribute("pnesid")]
		public int CrimeId { get; set; }

		/// <summary>
		/// fzlpnesid
		/// Системен идентификатор
		/// Използва се с префикс 'q' (q34)
		/// Прави връзка с останалите асти на XML пакета
		/// човек - престъпление например
		/// </summary>
		[XmlAttribute("fzlpnesid")]
		public int PersonCrimeId { get; set; }

		/// <summary>
		/// SCQ
		/// Процесуална санкция за престъпление
		/// </summary>
		[XmlElement("SCQ")]
		public CrimeSanction CrimeSanction { get; set; }

		/// <summary>
		/// FZLPNESPF
		/// Специфика на субект на престъпление
		/// </summary>
		[XmlElement("FZLPNESPF")]
		public CrimeSubjectDetails[] CrimeSubjectDetails { get; set; }

		/// <summary>
		/// NPL
		/// Статистически данни за непълнолетен
		/// </summary>
		[XmlElement("NPL")]
		public MinorStatisticData MinorStatistic { get; set; }

		/// <summary>
		/// SBC
		/// Статистически данни за субект на престъпление
		/// </summary>
		[XmlElement("SBC")]
		public CrimeSubjectStatisticData CrimeSubjectStatisticData { get; set; }

		/// <summary>
		/// VSL
		/// Статистически данни за военнослужещ
		/// </summary>
		[XmlElement("VSL")]
		public MilitaryStatisticData MilitaryStatisticData { get; set; }
		
		[XmlIgnore]
		[Display(Name = "Участие в престъпление")]
		public bool IsSelected { get; set; }

		[XmlIgnore]
		public bool IsSelectedReadOnly { get; set; } = false;



		[XmlIgnore]
		[Display(Name = "Име на лицето")]
		public string PersonName { get; set; }
	}
}