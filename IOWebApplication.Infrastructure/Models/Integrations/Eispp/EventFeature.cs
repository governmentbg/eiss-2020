using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// SBH
    /// Характеристика на събитие
    /// </summary>
    public class EventFeature
    {

        /// <summary>
        /// sbhsid
        /// Системен идентификатор на хараkтеристика
        /// </summary>
        [XmlAttribute("sbhsid")]
        public int FeatureId { get; set; }

        /// <summary>
        /// sbhvid
        ///Стойност на хараkтеристика
        /// системен код на елемент от nmk_dkpvid_prt 11993
        /// но реално е от  nmk_psdtip 231
        /// </summary>
        [XmlAttribute("sbhstn")]
        [Display(Name = "Стойност на хараkтеристика")]
        public int FeatureVal { get; set; }

        /// <summary>
        /// sbhvid
        /// Вид на характеристика
        /// системен код на елемент от nmk_sbhvid 228
        /// </summary>
        [XmlAttribute("sbhvid")]
        [Display(Name = "Вид на характеристика")]
        public int FeatureType { get; set; }

        /// <summary>
        /// sbhvid
        /// Текст на хараkтеристика
        /// </summary>
        [XmlAttribute("sbhtxt")]
        [Display(Name = "Текст на характеристика")]
        public string FeatureTxt { get; set; }
    }
}
