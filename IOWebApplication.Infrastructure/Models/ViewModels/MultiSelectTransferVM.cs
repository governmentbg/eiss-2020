using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class MultiSelectTransferVM
    {
        public int Id { get; set; }
        [JsonIgnore]
        public int OrderInt
        {
            set
            {
                OrderText = value.ToString();
            }
        }

        [JsonIgnore]
        public string OrderText
        {
            get; set;
        }
        public string Order
        {
            get
            {
                return OrderText;
            }
        }
        public int Percent { get; set; }
        public string Text { get; set; }
    }
}
