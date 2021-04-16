using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class ExchangeDocEditVM
    {
        public int Id { get; set; }

        public bool ForPopUp { get; set; }

        public string RegNumber { get; set; }
        public DateTime? RegDate { get; set; }

        public string ExecListIds { get; set; }

        public int InstitutionId { get; set; }
        public string InstitutionName { get; set; }

    }
}
