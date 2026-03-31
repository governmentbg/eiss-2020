using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
{
    /// <summary>
    /// Отразяване доставката на призовка като прочетена от съд
    /// </summary>
    public class SummonCourtRead
    {
        /// <summary>
        /// Идентификатор на призовка
        /// </summary>
        public Guid SummonId { get; set; }

        /// <summary>
        /// Прочетена на
        /// </summary>
        public DateTime CourtReadTime { get; set; }

        /// <summary>
        /// Информация за прочитането от съда
        /// </summary>
        public string CourtReadDescription { get; set; }
    }
}
