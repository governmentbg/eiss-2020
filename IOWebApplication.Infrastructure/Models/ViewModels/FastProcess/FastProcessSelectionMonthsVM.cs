using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class FastProcessSelectionMonthsVM
    {



        [Display(Name = "Брой Разпределени за деня")]
        public int SelectedCount { get; set; }
        [Display(Name = "Брой за разпределяне")]
        public int SelectionCount { get; set; }
        [Display(Name = "Дата на разпределяне")]
        public DateTime SelectionDate { get; set; }

        
    }


}
