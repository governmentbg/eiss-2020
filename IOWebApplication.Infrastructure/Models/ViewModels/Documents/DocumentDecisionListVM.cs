using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentDecisionListVM
    {
        public long Id { get; set; }

        public string DocumentNumber { get; set; }

        public DateTime DocumentDate { get; set; }

        public string DecisionNumber { get; set; }

        public DateTime? DecisionDate { get; set; }

        public string DecisionName { get; set; }

        public string DecisionUserName { get; set; }

        public string DocumentTypeName { get; set; }

        public long DocumentId { get; set; }
    }

    public class DocumentDecisionFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime DateTo { get; set; }

        [Display(Name = "Номер на документ")]
        public string DocumentNumber { get; set; }

        [Display(Name = "Година")]
        public int? DocumentYear { get; set; }
    }
}
