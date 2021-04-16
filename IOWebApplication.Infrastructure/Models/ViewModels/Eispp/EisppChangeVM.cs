using IOWebApplication.Infrastructure.Models.Integrations.Eispp;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Eispp
{
    public class EisppChangeVM
    {
        public int EventId { get; set; }

        [Display(Name = "Изпращане към ЕИСПП")]
        public bool IsForSend { get; set; }

        public string ModelJson { get; set; }

        public const string NormalDateFormat = "dd.MM.yyyy";

        [JsonIgnore]
        public Event OldEvent
        {
            get
            {
                 return EisppPackage.Data.Events[0]; 
            }
            set
            {
                var model = EisppPackage;
                model.Data.Events[0] = value;
                EisppPackage = model;
            }
        }

        [JsonIgnore]
        public Event NewEvent
        {
            get
            {
                return EisppPackage.Data.Events.Length > 1 ? EisppPackage.Data.Events[1] : null;
            }
            set
            {
                var model = EisppPackage;
                model.Data.Events[1] = value;
                EisppPackage = model;
            }
        }
        [JsonIgnore]
        public EisppPackage EisppPackage
        {
            get
            {
                var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = NormalDateFormat };
                return JsonConvert.DeserializeObject<EisppPackage>(ModelJson, dateTimeConverter);
            }
            set
            {
                var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = NormalDateFormat };
                ModelJson = JsonConvert.SerializeObject(value, dateTimeConverter);
            }
        }
        public Event NewEventObj { get; set; }
        public int? EventFromId { get; set; }

        public long? MQEpepId { get; set; }
        public string Mode { get; set; }
    }
}
