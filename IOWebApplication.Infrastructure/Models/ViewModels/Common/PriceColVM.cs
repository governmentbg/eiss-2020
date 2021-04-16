using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class PriceColVM
    {
        public int Id { get; set; }
        public int PriceDescId { get; set; }
        public int ColType { get; set; }
        public string ColTypeName { get; set; }
        public string Name { get; set; }
    }
}
