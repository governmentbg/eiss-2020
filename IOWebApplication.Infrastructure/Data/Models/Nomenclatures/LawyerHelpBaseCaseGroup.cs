using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    [Table("nom_lawyer_help_base_case_group")]
    public class LawyerHelpBaseCaseGroup
    {
        [Column("lawyer_help_base_id")]
        public int LawyerHelpBaseId { get; set; }

        [Column("case_group_id")]
        public int CaseGroupId { get; set; }

        [ForeignKey(nameof(LawyerHelpBaseId))]
        public virtual LawyerHelpBase LawyerHelpBase { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }
    }
}
