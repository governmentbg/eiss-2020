using Newtonsoft.Json;

namespace IOWebApplication.Core.Models
{
    public class CommonNomenclatureListItem
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Код
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// Текст за визуализация
        /// </summary>
        [JsonProperty("label")]
        public string Label { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Пореден номер
        /// </summary>
        [JsonProperty("orderNumber")]
        public int OrderNumber { get; set; }

        /// <summary>
        /// Дали записа е активен
        /// </summary>
        [JsonProperty("isActive")]
        public bool IsActive { get; set; }
    }
}
