using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    public class CaseLoadElementTypeVM
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public decimal LoadProcent { get; set; }
        
        [Display(Name = "Начална дата")]
        public DateTime DateStart { get; set; }
        [Display(Name = "Крайна дата")]
        public DateTime? DateEnd { get; set; }
    }
}
