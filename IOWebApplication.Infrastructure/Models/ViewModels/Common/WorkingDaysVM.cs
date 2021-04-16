using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class WorkingDaysVM
    {      
        public int Id { get; set; }
      
        public DateTime Day { get; set; }

        /// <summary>
        /// Ако е null, важи за всички съдилища
        /// </summary>       
        public int? CourtId { get; set; }

        public string CourtName { get; set; }

        /// <summary>
        /// Вид ден: 1-Почивен,2-Работен
        /// </summary>      
        public int DayType { get; set; }

        public string DayTypeName { get; set; }

        public string Description { get; set; }


    }
}
