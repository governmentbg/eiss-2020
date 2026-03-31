using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
{
    /// <summary>
    /// Връчени призовки, които следва да се отразят в ЕИСС
    /// </summary>
    public class SummonReadModel
    {
        /// <summary>
        /// Идентификатор на призовка
        /// </summary>
        public Guid SummonId { get; set; }

        /// <summary>
        /// Прочетена на
        /// </summary>
        public DateTime ReadTime { get; set; }

        /// <summary>
        /// Идентификатор на файл, Отчет за връчване
        /// </summary>
        public Guid? ReadReportFileId { get; set; }
    }
}
