using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
{

    /// <summary>
    /// Данни за улици/квартали по населени места
    /// </summary>
    public class EkStreet
    {
        /// <summary>
        /// 5 Цифрен ЕКАТТЕ код на населено място
        /// </summary>
        public string CityCode { get; set; }

        /// <summary>
        /// Код на улица/квартал
        /// </summary>
        public string StreetCode { get; set; }
        /// <summary>
        /// Тип запис: 1-Улица,2-Квартал/Площад
        /// </summary>
        public int StreetType { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string StreetName { get; set; }

        /// <summary>
        /// В сила от
        /// </summary>
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// В сила до
        /// </summary>
        public DateTime? DateTo { get; set; }

        [JsonIgnore]
        [NotMapped]
        public DateTime? DateWrt { get; set; }
    }

}
