using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class HtmlTemplateFilterVM
    {
        /// <summary>
        /// Вид бланка
        /// </summary>
        [Display(Name = "Вид документ")]
        public int HtmlTemplateTypeId { get; set; }
    }
}
