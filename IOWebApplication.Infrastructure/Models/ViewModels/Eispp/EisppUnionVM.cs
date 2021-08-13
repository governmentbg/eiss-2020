using IOWebApplication.Infrastructure.Models.Integrations.Eispp;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Eispp
{
    public class EisppUnionVM
    {
        public int EventId { get; set; }
        public int CasePrincipalId { get; set; }
        public int CaseAddedId { get; set; }
        public bool IsValidatedPrincipal { get; set; }
        public bool IsValidatedAdded { get; set; }
        
        [Display(Name = "Изпращане към ЕИСПП")]
        public bool IsForSend { get; set; }

        public string ModelJson { get; set; }

        public const string NormalDateFormat = "dd.MM.yyyy";

        [JsonIgnore]
        public Event EventPrincipal
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
        public Event EventAdded
        {
            get
            {
                return EisppPackage.Data.Events[0].EventAdded;
            }
            set
            {
                var model = EisppPackage;
                model.Data.Events[0].EventAdded = value;
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
        [JsonIgnore]
        public Event EventPrincipalObj { get; set; }

        [JsonIgnore]
        public Event EventAddedObj { get; set; }

        public long? MQEpepId { get; set; }
        public string Mode { get; set; }

    }
}
