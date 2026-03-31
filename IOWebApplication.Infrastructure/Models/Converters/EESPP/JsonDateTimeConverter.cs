using Newtonsoft.Json.Converters;

namespace IOWebApplication.Infrastructure.Models.Converters.EESPP
{
    public class JsonDateTimeConverter : IsoDateTimeConverter
    {
        public JsonDateTimeConverter()
        {
            base.DateTimeFormat = "dd.MM.yyyy HH:mm:ss";
        }
    }
}
