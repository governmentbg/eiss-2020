using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.RNFL
{
    /// <summary>
    /// Прикачен документ към обект
    /// </summary>
    public class ApiFile
    {
        /// <summary>
        /// Идентификатор на акт
        /// </summary>
        public Guid? Gid { get; set; }

        /// <summary>
        /// Флаг за единичен файл
        /// При true - изтрива съществуващ файл за същия SourceType/SourceGid
        /// </summary>
        public bool AppendUpdate { get; set; }

        /// <summary>
        /// Код на тип файл, REQ|NOM
        /// </summary>
        public int SourceType { get; set; }

        /// <summary>
        /// Идентификатор на обект
        /// </summary>
        public Guid SourceGid { get; set; }

        /// <summary>
        /// Име на файла, REQ
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Съдържание на файла, REQ (base64)
        /// </summary>
        public byte[] FileContent { get; set; }

        /// <summary>
        /// MimeType на файла, REQ
        /// </summary>
        public string ContentType { get; set; }
    }
}
