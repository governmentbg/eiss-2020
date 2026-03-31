namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
{
    /// <summary>
    /// Описва ред от обекта ElectronicDocumentSideAddress, Адрес на страна към документ
    /// </summary>
    public class ElectronicDocumentSideAddress
    {
        /// <summary>
        /// Код на тип адрес
        /// Полето е задължително
        /// </summary>

        public string AddressTypeCode { get; set; }

        /// <summary>
        /// Код на държава, 2 буквен код на държава 
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Адрес в чужбина
        /// </summary>
        public string ForeignAddress { get; set; }

        /// <summary>
        /// ЕКАТТЕ код на населено място
        /// </summary>
        public string CityCode { get; set; }

        /// <summary>
        /// Населено място
        /// </summary>
        public string CityName { get; set; }

        /// <summary>
        /// ЕКАТТЕ код на квартал
        /// </summary>
        public string ResidentionAreaCode { get; set; }

        /// <summary>
        /// Квартал
        /// </summary>
        public string ResidentionAreaName { get; set; }

        /// <summary>
        /// ЕКАТТЕ код на улица
        /// </summary>
        public string StreetCode { get; set; }

        /// <summary>
        /// Улица
        /// </summary>
        public string StreetName { get; set; }

        /// <summary>
        /// Блок номер
        /// </summary>
        public int? Block { get; set; }

        /// <summary>
        /// Под номер блок
        /// </summary>
        public string SubBlock { get; set; }

        /// <summary>
        /// Улица Номер
        /// </summary>
        public int? StreetNumber { get; set; }

        /// <summary>
        /// Под номер улица
        /// </summary>
        public string SubNumber { get; set; }

        /// <summary>
        /// Вход
        /// </summary>
        public string Entrance { get; set; }

        /// <summary>
        /// Етаж
        /// </summary>
        public string Floor { get; set; }

        /// <summary>
        /// Апартамент/офис
        /// </summary>
        public string Apartment { get; set; }


        /// <summary>
        /// Телефон
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Факс
        /// </summary>
        public string Fax { get; set; }

        /// <summary>
        /// Електронна поща
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Пълен адрес
        /// </summary>
        public string FullAddress { get; set; }
    }
}