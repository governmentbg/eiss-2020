using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Eispp
{
    public class EisppDataDocumentVM
    {
        [Display(Name = "Номер документ")]
        public string Number { get; set; }
        public DateTime Date { get; set; }
    }
}
