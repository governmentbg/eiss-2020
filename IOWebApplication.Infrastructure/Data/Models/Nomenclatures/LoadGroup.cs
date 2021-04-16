using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Групи по натовареност
    /// </summary>
    [Table("nom_load_group")]
    public class LoadGroup : BaseCommonNomenclature
    {
        public virtual ICollection<LoadGroupLink> LoadGroupLinks { get; set; }
    }
}
