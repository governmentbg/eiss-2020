using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид на Бланки: Призовки, Решения,Актове и др.
    /// </summary>
    [Table("nom_html_template_type")]
    public class HtmlTemplateType : BaseCommonNomenclature
    {
        [Display(Name = "Група")]
        [Column("template_group")]
        public int? TemplateGroup { get; set; }
    }
}
