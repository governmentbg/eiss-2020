using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    [Table("nom_lawyer_help_type_case_group")]
    public class LawyerHelpTypeCaseGroup
    {
        [Column("lawyer_help_type_id")]
        public int LawyerHelpTypeId { get; set; }

        [Column("case_group_id")]
        public int CaseGroupId { get; set; }

        [ForeignKey(nameof(LawyerHelpTypeId))]
        public virtual LawyerHelpType LawyerHelpType { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }
    }
}
