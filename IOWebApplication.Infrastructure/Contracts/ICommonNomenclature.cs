using System;

namespace IOWebApplication.Infrastructure.Contracts
{
    /// <summary>
    /// Author: Stamo Petkov
    /// Created: 14.11.2016
    /// Description: Интерфейс за достъп до полетата 
    /// на общи номенклатури
    /// </summary>
    public interface ICommonNomenclature
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Код на номенклатурата
        /// </summary>
        string Code { get; set; }

        /// <summary>
        /// Етикет на номенклатурата
        /// </summary>
        string Label { get; set; }

        /// <summary>
        /// Описание на номенклатурата
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Пореден номер
        /// </summary>
        int OrderNumber { get; set; }

        /// <summary>
        /// Начало на периода на валидност
        /// </summary>
        DateTime DateStart { get; set; }

        /// <summary>
        /// Край на периода на валидност
        /// Ако е NULL, е валидна след начална дата
        /// </summary>
        DateTime? DateEnd { get; set; }

        /// <summary>
        /// Дали записа е активен
        /// </summary>
        bool IsActive { get; set; }
    }
}
