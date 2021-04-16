using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseCrimeVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }

        [Display(Name = "id от ЕИСПП")]
        public string ValueEISSId { get; set; }

        [Display(Name = "Код по ЕИСПП")]
        public string ValueEISSPNumber { get; set; }

        [Display(Name = "Вид престъпление")]
        public string CrimeCode { get; set; }

        [Display(Name = "Вид престъпление")]
        public string CrimeName { get; set; }
    }
}
