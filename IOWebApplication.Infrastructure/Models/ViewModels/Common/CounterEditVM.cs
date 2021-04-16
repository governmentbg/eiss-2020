using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CounterEditVM : Counter
    {
        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Направление на документ")]
        public int DocumentDirectionId { get; set; }

        [Display(Name = "Група съдебни актове")]
        public int SessionActGroupId { get; set; }
    }
}
