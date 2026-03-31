using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{


    public class VksMainSessionAddVM
    {
        [Display(Name = "Отделение")]
        public int CourtDepartmentId { get; set; }


        public List<VksSessionDayCalendarVM> VksSelectionCalendar { get; set; }


    }

}


