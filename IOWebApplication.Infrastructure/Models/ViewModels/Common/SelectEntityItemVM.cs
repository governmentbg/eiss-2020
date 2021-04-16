using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class SelectEntityItemVM
    {
        public int SourceType { get; set; }
        public int SourceId { get; set; }
        public string ObjectTypeName { get; set; }
        public string Label { get; set; }
        private string _labelFull;

        public string LabelFull
        {
            get
            {
                return _labelFull ?? Label;
            }
            set
            {
                _labelFull = value;
            }
        }
        public int UicTypeId { get; set; }
        public string Uic { get; set; }
    }
}
