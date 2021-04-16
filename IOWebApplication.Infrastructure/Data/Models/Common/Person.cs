using IOWebApplication.Infrastructure.Data.Models.Base;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Номенклатура на физически лица, без повтаряне по идентификатор
    /// </summary>
    [Table("common_person")]
    public class Person : NamesBase
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("parent_id")]
        public int? ParentId { get; set; }

        [Column("person_type_id")]
        public int PersonTypeId { get; set; }

        [Column("actual_to_date")]
        public DateTime ActualtoDate { get; set; }

        [Column("confirm_date")]
        public DateTime? ConfirmDate { get; set; }

        [ForeignKey(nameof(ParentId))]
        public virtual Person Parent { get; set; }
      
    }
}
