using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class ReportCourtGenericVM
    {
        [Display(Name = "Вид справка")]
        [Column("report")]
        public string Report { get; set; }

        [Display(Name = "Етикет")]
        [Column("label")]
        public string Label { get; set; }

        [Display(Name = "Брой")]
        [Column("cnt")]
        public int Count { get; set; }        
    }
}
