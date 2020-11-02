// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// PRVKCQ
    /// Правна квалификация
    /// </summary>
    public class LegalQualification
    {
        /// <summary>
        /// pnekcq
        /// Квалификация по НК
        /// Номенклатура nmk_pnekcq
        /// задължителнои
        /// </summary>
        [XmlAttribute("pnekcq")]
        public int CrimeQualification { get; set; }

        /// <summary>
        /// prjkcq
        /// Предложение
        /// </summary>
        [XmlAttribute("prjkcq")]
        public string QualificationProposal { get; set; }

        /// <summary>
        /// izrkcq
        /// Изречение
        /// </summary>
        [XmlAttribute("izrkcq")]
        public string QualificationSentance { get; set; }

        /// <summary>
        /// prdnmr
        /// Пореден номер във връзка
        /// </summary>
        [XmlAttribute("prdnmr")]
        public string SequentialNumber { get; set; }
    }
}