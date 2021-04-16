using System;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// DKS
    /// Документ за самоличност
    /// </summary>
    public class PersonalDocument
    {
        /// <summary>
        /// dkssid
        /// Системен идентификатор
        /// </summary>
        [XmlAttribute("dkssid")]
        public int DocumentId { get; set; }

        /// <summary>
        /// dksdrj
        /// Държава, издала документа за самоличност
        /// Номенклатура nmk_grj
        /// задължително
        /// </summary>
        [XmlAttribute("dksdrj")]
        public int IssuerCountry { get; set; }

        /// <summary>
        /// dksvid
        /// Вид на документ за самоличност
        /// Номенклатура nmk_dksvid
        /// задължително
        /// </summary>
        [XmlAttribute("dksvid")]
        public int DocumentKind { get; set; }

        /// <summary>
        /// dksnmr
        /// Номер на документ за самоличност
        /// задължително
        /// </summary>
        [XmlAttribute("dksnmr")]
        public string DocumentNumber { get; set; }

        /// <summary>
        /// dksdta
        /// Дата на издаване на документ за самоличност
        /// задълживетлно
        /// </summary>
        [XmlAttribute("dksdta", DataType = "date")]
        public DateTime IssueDate { get; set; }

        /// <summary>
		/// Дата, която трябва да се игнорира
		/// </summary>
		private static readonly DateTime defaultDate = default;

        /// <summary>
        /// Игнорира празна дата
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeIssueDate()
        {
            return IssueDate != defaultDate;
        }
    }
}