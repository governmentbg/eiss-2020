using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Identity
{
    public class UserSettingsModel
    {
        [Display(Name = "Покажи календар със събития")]
        public bool CalendarVisible { get; set; }
        [Display(Name = "Стил на календара")]
        public string CalendarStyle { get; set; }
        [Display(Name = "Покажи Моите задачи")]
        public bool WorkTaskVisible { get; set; }
        [Display(Name = "Покажи Дела към мен")]
        public bool CaseMoveVisible { get; set; }
        [Display(Name = "Покажи последна новина")]
        public bool NewsVisible { get; set; }

        public UserSettingsModel()
        {
            CalendarStyle = "month";
            CalendarVisible = true;
            WorkTaskVisible = true;
            CaseMoveVisible = true;
            NewsVisible = true;
        }

        public class Set
        {
            public const string CalendarStyle = "calendarStyle";
        }

        public string AsText()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
