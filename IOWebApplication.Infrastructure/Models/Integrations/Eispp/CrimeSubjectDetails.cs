using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// FZLPNESPF
    /// Специфика на субект на престъпление
    /// </summary>
    public class CrimeSubjectDetails
    {
        /// <summary>
        /// fzlpnespfsid
        /// Системен идентификатор
        /// </summary>
        [XmlAttribute("fzlpnespfsid")]
        public int CrimeSubjectDetailsId { get; set; }

        /// <summary>
        /// fzlpnespfstn
        /// Стойност на специфика на субект на престъпление
        /// Номенклатура nmk_fzlpne_dst
        /// задължително
        /// </summary>
        [XmlAttribute("fzlpnespfstn")]
        public int DetailsValue { get; set; }

        /// <summary>
        /// fzlpnespfvid
        /// Вид специфика на субект на престъпление
        /// Номенклатура nmk_fzlpnespfvid
        /// задължително
        /// </summary>
        [XmlAttribute("fzlpnespfvid")]
        public int DetailsType { get; set; }

        /// <summary>
        /// fzlpnespfopi
        /// Описание на специфики на субект на престъпление
        /// </summary>
        [XmlAttribute("fzlpnespfopi")]
        public string DetailsDescription { get; set; }
    }
}