using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Ред на представляване: CasePersonId Чрез CasePersonRelId/ CasePersonRelId като CasePersonRel.PersonRole на CasePersonId
    /// </summary>
    [Table("nom_link_direction")]
    public class LinkDirection : BaseCommonNomenclature
    {
        [Column("link_template")]
        [Display(Name = "Изписване на връзката")]
        public string LinkTemplate { get; set; }
    }
}
