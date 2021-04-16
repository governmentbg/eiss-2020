using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
   
    [Table("id_list")]
    public class ID_List
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("remark")]
        public string Remark { get; set; }
    }
}
