using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.VKS
{
    /// <summary>
    /// Участници към месечно разпределение за избор на съдии за ВКС
    /// </summary>
    [Table("vks_selection_month_lawunit")]
    public class VksSelectionMonthLawunit : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("vks_selection_month_id")]
        public int VksSelectionMonthId { get; set; }

        [Column("vks_selection_lawunit_id")]
        public int VksSelectionLawunitId { get; set; }

        /// <summary>
        /// Уникален GUID за всеки нов оригинален съдия или ключа на заместения съдия
        /// </summary>
        [Column("lawunit_key")]
        public string LawunitKey { get; set; }


        [ForeignKey(nameof(VksSelectionMonthId))]
        public virtual VksSelectionMonth VksSelectionMonth { get; set; }

        [ForeignKey(nameof(VksSelectionLawunitId))]
        public virtual VksSelectionLawunit VksSelectionLawunit { get; set; }
    }
}
