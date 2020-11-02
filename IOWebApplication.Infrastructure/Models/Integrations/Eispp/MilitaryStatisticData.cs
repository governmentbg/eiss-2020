// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// VSL
    /// Статистически данни за военнослужещ
    /// </summary>
    public class MilitaryStatisticData
    {
        /// <summary>
        /// vsldlj
        /// Военна длъжност
        /// Номенклатура nmk_vsldlj
        /// </summary>
        [XmlAttribute("vsldlj")]
        public string MilitaryPosition { get; set; }

        /// <summary>
        /// vslktg
        /// Категория военнослужещ
        /// Номенклатура nmk_vslktg
        /// </summary>
        [XmlAttribute("vslktg")]
        public int Category { get; set; }

        /// <summary>
        /// vslpdl
        /// Поделение, където служи
        /// </summary>
        [XmlAttribute("vslpdl")]
        public int Division { get; set; }

        /// <summary>
        /// vslsid
        /// Системен идентификатор
        /// </summary>
        [XmlAttribute("vslsid")]
        public int StatisticDataId { get; set; }

        /// <summary>
        /// vslzvn
        /// Военно звание
        /// Номенклатура nmk_vslzvn
        /// </summary>
        [XmlAttribute("vslzvn")]
        public int MilitaryRank { get; set; }
    }
}