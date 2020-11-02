// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// RZT
    /// Резултат от обработка на пакет
    /// </summary>
    public class EisppResult
    {
        /// <summary>
        /// KartaNPR
        /// Съдържа HTML на Карта на НП при успешно обработено събитие
        /// </summary>
        [XmlElement("KartaNPR")]
        public string HtmlCard { get; set; }

        /// <summary>
        /// SBE
        /// Събитие
        /// </summary>
        [XmlElement("SBE")]
        public Event[] Events { get; set; }

        /// <summary>
        /// Exception
        /// Текст на грешката при неуспешна обработка
        /// </summary>
        [XmlElement("Exception")]
        public string Exception { get; set; }

        /// <summary>
        /// KST
        /// Контекст на пакет
        /// </summary>
        [XmlElement("KST")]
        public Context Context { get; set; }
    }
}