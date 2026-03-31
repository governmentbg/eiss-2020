using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class InstitutionVM
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Code { get; set; }
        public string EISPPCode { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
