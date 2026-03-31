using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Epep
{
    public class MQEpepVM
    {
        public long Id { get; set; }
        //Нарочно е int? да не се каства при Select-а
        public int? StateId { get; set; }
        public string MethodName { get; set; }
        public string OperName
        {
            get
            {
                //(x.MethodName == "add") ? "Добавяне" : "Редакция",
                switch (MethodName)
                {
                    case "add": return "Добавяне";
                    case "delete": return "Изтриване";
                    default:
                        return "Редакция";
                }
            }
        }
        public DateTime DateWrt { get; set; }
        public DateTime? DateTransfered { get; set; }
        public string ErrorDescription { get; set; }
    }
}
