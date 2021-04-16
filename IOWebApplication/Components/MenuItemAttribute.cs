using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Components
{
    public class MenuItemAttribute : Attribute
    {
        public string Value { get; set; }

        public MenuItemAttribute(string value)
        {
            this.Value = value;
        }
    }
}
