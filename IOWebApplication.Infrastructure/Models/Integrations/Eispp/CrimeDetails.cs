// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// PNESPF
    /// Специфика на престъпление
    /// </summary>
    public class CrimeDetails
    {
        /// <summary>
        /// pnespfsid
        /// Системен Идентификатор
        /// </summary>
        [XmlAttribute("pnespfsid")]
        public int CrimeDetailsId { get; set; }

        /// <summary>
        /// pnespfstn
        /// Стойност на специфика на престъпление
        /// Номенклатура nmk_pne_nch
        /// задължително
        /// </summary>
        [XmlAttribute("pnespfstn")]
        public int CrimeDetailValue { get; set; }

        /// <summary>
        /// pnespfvid
        /// Вид специфика на престъпление
        /// Номенклатура nmk_pnespfvid
        /// задължително
        /// </summary>
        [XmlAttribute("pnespfvid")]
        public int CrimeDetailType { get; set; }

        /// <summary>
        /// pnespfopi
        /// Описание на специфики на престъпление
        /// </summary>
        [XmlAttribute("pnespfopi")]
        public string CrimeDetailDescription { get; set; }
    }
}