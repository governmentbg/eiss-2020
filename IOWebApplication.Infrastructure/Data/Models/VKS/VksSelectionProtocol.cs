using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.VKS
{
    /// <summary>
    /// Генерирани протоколи към случаен избор ВКС
    /// </summary>
    [Table("vks_selection_protocol")]
    public class VksSelectionProtocol : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("vks_selection_id")]
        public int VksSelectionId { get; set; }

        [Column("date_generated")]
        public DateTime DateGenerated { get; set; }

        [Column("user_generated_id")]
        public string UserGeneratedId { get; set; }

        [Column("date_signed")]
        public DateTime? DateSigned { get; set; }

        [Column("user_signed_id")]
        public string UserSignedId { get; set; }

        [ForeignKey(nameof(VksSelectionId))]
        public virtual VksSelection VksSelection { get; set; }


    [ForeignKey(nameof(UserGeneratedId))]
    public virtual ApplicationUser UserGenerated { get; set; }


    [ForeignKey(nameof(UserSignedId))]
    public virtual ApplicationUser UserSigned { get; set; }
  }
}
