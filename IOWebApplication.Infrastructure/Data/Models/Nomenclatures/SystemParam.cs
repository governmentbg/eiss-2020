using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    [Table("nom_system_param")]
    public class SystemParam
    {
        [Key]
        [Column("param_name")]
        public string ParamName { get; set; }

        [Column("param_value")]
        public string ParamValue { get; set; }

        [Column("description")]
        public string Description { get; set; }
    }
}
