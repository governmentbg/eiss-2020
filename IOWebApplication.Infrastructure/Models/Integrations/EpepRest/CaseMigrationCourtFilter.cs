using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
{
    /// <summary>
    /// Филтър за търсене на движения на дела към подаден съд
    /// </summary>

    public class CaseMigrationCourtFilter
    {
        /// <summary>
        /// Код на съд
        /// </summary>
        public string CourtCode { get; set; }

        /// <summary>
        /// Начална дата на движенията за търсене, NULL - връща всички движения
        /// </summary>
        public DateTime? FromDate { get; set; } = null;

    }
}