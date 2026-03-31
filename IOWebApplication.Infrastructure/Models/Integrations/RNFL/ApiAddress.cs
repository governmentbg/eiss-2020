namespace IOWebApplication.Infrastructure.Models.Integrations.RNFL
{
    /// <summary>
    /// Данни за адрес
    /// </summary>
    public class ApiAddress
    {
        /// <summary>
        /// 2 буквен код на държава, REQ|NOM
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Код на тип адрес, REQ|NOM
        /// </summary>
        public string AddressTypeCode { get; set; }

        /// <summary>
        /// ЕКАТТЕ код на неселено място, REQ (при BG)
        /// </summary>
        public string CityCode { get; set; }

        /// <summary>
        /// Име на неселено място в чужбина
        /// </summary>
        public string ForeignCity { get; set; }

        /// <summary>
        /// Адрес: ул/кв, номер, вх, ет, ап
        /// </summary>
        public string AddressText { get; set; }

    }
}
