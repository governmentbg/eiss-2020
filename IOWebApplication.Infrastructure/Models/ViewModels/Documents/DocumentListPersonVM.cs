using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class DocumentListPersonVM
    {
        public long Id { get; set; }
        public string RoleName { get; set; }
        public string Uic { get; set; }
        public string Name { get; set; }
    }
}
