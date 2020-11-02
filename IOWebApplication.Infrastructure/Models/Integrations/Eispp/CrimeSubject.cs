// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// PSG
    /// Предмет на посегателство
    /// </summary>
    public class CrimeSubject
    {
        /// <summary>
        /// psgsid
        /// Системен идентификатор
        /// </summary>
        [XmlAttribute("psgsid")]
        public int SubjectId { get; set; }

        /// <summary>
        /// psgtxt
        /// Описание на предмет на посегателство
        /// </summary>
        [XmlAttribute("psgtxt")]
        public string Description { get; set; }

        /// <summary>
        /// psgvid
        /// Вид предмет на посегателство
        /// Номенклатура nmk_psgvid
        /// задължително
        /// </summary>
        [XmlAttribute("psgvid")]
        public int SubjectType { get; set; }
    }
}