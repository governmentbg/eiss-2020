using Newtonsoft.Json.Converters;

namespace IOWebApplication.Infrastructure.Models.Converters.EESPP
{
    public class JsonDateConverter : IsoDateTimeConverter
    {
        public JsonDateConverter()
        {
            base.DateTimeFormat = "dd.MM.yyyy";
        }
    }
}
