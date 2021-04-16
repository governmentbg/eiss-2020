using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class BreadcrumbsVM
    {
        public string Href { get; set; }
        public string Title { get; set; }
        public string Tooltip { get; set; }
        public string Icon { get; set; }
        public bool Active { get; set; }
    }
}
