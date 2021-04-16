using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    [Table("common_priceval")]
    public  class PriceVal
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("pricedesc_id")]
        public int PriceDescId { get; set; }
        [Column("row_no")]
        public int RowNo { get; set; }
        [Column("pricecol_id")]
        public int PriceColId { get; set; }

        [Column("value")]
        public decimal Value { get; set; }
        [Column("text")]
        public string Text { get; set; }

        [ForeignKey(nameof(PriceDescId))]
        public virtual PriceDesc PriceDesc { get; set; }

        [ForeignKey(nameof(PriceColId))]
        public virtual PriceCol Col { get; set; }
    }
}
