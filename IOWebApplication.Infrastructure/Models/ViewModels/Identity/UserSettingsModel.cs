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
        [Display(Name = "Покажи търсене на дела")]
        public bool CaseFastSearchVisible { get; set; }

        [Display(Name = "Последно избран скенер")]
        public string LastSelectedScaner { get; set; }

        [Display(Name = "Покажи Известия Заповедно производство")]
        public bool FPnotificationsVisible { get; set; }

        public UserSettingsModel()
        {
            CalendarStyle = "month";
            CalendarVisible = true;
            WorkTaskVisible = true;
            CaseMoveVisible = true;
            NewsVisible = true;
            CaseFastSearchVisible = true;
            FPnotificationsVisible = false;
        }

        public class Set
        {
            public const string CalendarStyle = "calendarStyle";
            public const string LastSelectedScaner = "LastSelectedScaner";
        }

        public string AsText()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
