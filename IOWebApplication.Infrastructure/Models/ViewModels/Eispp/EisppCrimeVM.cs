using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Eispp
{
    public class EisppCrimeVM
    {
        /// <summary>
        /// ид дело
        /// </summary>
        public int CaseId { get; set; }

        [Display(Name = "ЕИСПП номер на НП")]
        [Required(ErrorMessage = "Невалиден {0}.")]
        [RegularExpression("[А-Я]{3}[0-9]{8}[В-Г]{1}[А-Я]{2}", ErrorMessage = "Невалиден {0}.")]
        [Remote(action: "VerifyEISPPNumber", controller: "Eispp")]
        public string EISPPNumber { get; set; }

        [Display(Name = "Номер на престъпление")]
        public string PneNumber { get; set; }

        public string Label { get; set; }
    }
}
