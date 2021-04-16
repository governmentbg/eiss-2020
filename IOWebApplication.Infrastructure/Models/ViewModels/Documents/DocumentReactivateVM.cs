using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentReactivateVM
    {
        public long Id { get; set; }
        public bool IsFound { get; set; }
        [Display(Name = "Направление на документ")]
        public int DocumentDirectionId { get; set; }
        [Display(Name = "Номер документ")]
        public string DocumentNumber { get; set; }
        [Required(ErrorMessage ="Въведете {0}.")]
        [Display(Name = "Дата")]
        public DateTime DocumentDate { get; set; }
        [Display(Name = "Информация за документ")]
        public string DocumentInfo { get; set; }
        public string FindMessage { get; set; }
        public bool IsActivated { get; set; }
    }
}
