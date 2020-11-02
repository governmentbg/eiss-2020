// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// MRD
    /// Месторождение
    /// </summary>
    public class BirthPlace
    {
        /// <summary>
        /// mrdsid
        /// Системен идентификатор
        /// </summary>
        [XmlAttribute("mrdsid")]
        public int PlaceId { get; set; }

        /// <summary>
        /// mrddrj
        /// Държава на месторождение
        /// Номенклатура nmk_grj
        /// </summary>
        [Display(Name= "Държава на месторождение")]
        [XmlAttribute("mrddrj")]
        public int Country { get; set; }

        /// <summary>
        /// mrdnsmbgr
        /// Населено място на месторождение в България
        /// Номенклатура nmk_nas_miasto
        /// </summary>
        [Display(Name= "Населено място в РБ")]
        [XmlAttribute("mrdnsmbgr")]
        public int SettelmentBg { get; set; }

        /// <summary>
        /// mrdnsmchj
        /// Населено място в чужда държава
        /// </summary>
        [Display(Name = "Населено място в чужда държава")]
        [XmlAttribute("mrdnsmchj")]
        public string SettelmentAbroad { get; set; }
    }
}