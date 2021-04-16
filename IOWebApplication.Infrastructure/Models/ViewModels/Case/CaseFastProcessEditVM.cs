using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseFastProcessEditVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int CourtId { get; set; }
        public string DescriptionSave { get; set; }
        
        [Display(Name = "Допълнителна иформация")]
        public string DescriptionEdit { get; set; }
    }
}
