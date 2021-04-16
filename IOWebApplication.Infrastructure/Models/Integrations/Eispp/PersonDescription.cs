using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// FZLOPI
    /// Описание на физическо лице
    /// </summary>
    public class PersonDescription
    {
        /// <summary>
        /// fzlopisid
        /// Системен идентификатор на описание
        /// </summary>
        [XmlAttribute("fzlopisid")]
        public int DescriptionId { get; set; }

        /// <summary>
        /// fzlopitxt
        /// Описание на лице
        /// </summary>
        [XmlAttribute("fzlopitxt")]
        public string Description { get; set; }

        /// <summary>
        /// fzlopivid
        /// Вид на описание на физическо лице
        /// Номенклатура nmk_fzlopivid
        /// задължително
        /// </summary>
        [XmlAttribute("fzlopivid")]
        public int DescriptionType { get; set; }

        /// <summary>
        /// fzlsid
        /// Системен идентификатор на лице
        /// </summary>
        [XmlAttribute("fzlsid")]
        public int PersonId { get; set; }
    }
}