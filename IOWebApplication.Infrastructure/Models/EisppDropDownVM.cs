using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Infrastructure.Models
{
    public class EisppDropDownVM
    {
        public string Label { get; set; }
        public int Flags { get; set; }
        public List<SelectListItem> DDList { get; set; }
        public string GetSelectedText(int? selected)
        {
            return DDList?.Where(x => x.Value == selected.ToString()).FirstOrDefault()?.Text ?? "";
        }
    }
}
