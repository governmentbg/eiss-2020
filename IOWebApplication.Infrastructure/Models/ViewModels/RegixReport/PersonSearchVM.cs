using IOWebApplication.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.RegixReport
{
    public class PersonSearchVM : NamesBase
    {
        public string ID { get; set; }
        public string RegisterName { get; set; }
        public bool IsDead { get; set; }
        public string DeathDate { get; set; }

        public PersonSearchVM()
        {
            ID = Guid.NewGuid().ToString();
        }
    }
}
