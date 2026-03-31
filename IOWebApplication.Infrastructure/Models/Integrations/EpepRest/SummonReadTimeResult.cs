using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
{
    /// <summary>
    /// Описва ред от обекта SummonReadTimeResult, резултат при проверка на дата на връчване на призовка
    /// </summary>
    public class SummonReadTimeResult
    {
        /// <summary>
        /// Идентификатор за прочетена призовка
        /// </summary>

        public bool IsRead { get; set; }

        /// <summary>
        /// Дата на прочитане на призовката
        /// Полето е задължително
        /// </summary>

        public DateTime? ReadDate { get; set; }

        /// <summary>
        /// Крайна дата на отсъствие на адвоката - когато е приложимо
        /// Полето е задължително
        /// </summary>
        public DateTime? VacationEndDate { get; set; }
    }
}