using System;
using System.Runtime.Serialization;


namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
{
    /// <summary>
    /// Описва ред от обекта ElectronicDocumentFile, Файл към документ
    /// </summary>
    public class ElectronicDocumentFile
    {
        /// <summary>
        /// Тип на документа
        /// Полето е задължително
        /// </summary>

        public int AttachmentType { get; set; }

        /// <summary>
        /// Описание на файла
        /// Полето не е задължително
        /// </summary>

        public string Title { get; set; }

        /// <summary>
        /// Име на файла
        /// Полето не е задължително
        /// </summary>

        public string FileName { get; set; }


        public long FileSize { get; set; }

        public Guid FileId { get; set; }

        public bool FileIsLoaded { get; set; }

        /// <summary>
        /// Идентификатор на файл за изтегляне
        /// Полето е задължително
        /// </summary>

        public byte[] Content { get; set; }
    }
}