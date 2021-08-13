//using IOWebApplication.Infrastructure.Data.Models.Base;
//using IOWebApplication.Infrastructure.Data.Models.Common;
//using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Text;

//namespace IOWebApplication.Infrastructure.Data.Models.Cases
//{
//    /// <summary>
//    /// Съгласуване на актове
//    /// </summary>
//    [Table("case_session_act_coordination")]
//    public class CaseSessionActCoordination : UserDateWRT
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Column("court_id")]
//        public int? CourtId { get; set; }

//        [Column("case_id")]
//        public int? CaseId { get; set; }

//        [Column("case_session_act_id")]
//        public int CaseSessionActId { get; set; }

//        [Column("case_lawunit_id")]
//        [Display(Name = "Съдия")]
//        public int CaseLawUnitId { get; set; }

//        [Column("act_coordination_type_id")]
//        [Display(Name = "Статус")]
//        public int ActCoordinationTypeId { get; set; }

//        [Column("content")]
//        [Display(Name = "Особено мнение")]
//        [AllowHtml]
//        public string Content { get; set; }

//        [ForeignKey(nameof(CourtId))]
//        public virtual Court Court { get; set; }

//        [ForeignKey(nameof(CaseId))]
//        public virtual Case Case { get; set; }

//        [ForeignKey(nameof(CaseSessionActId))]
//        public virtual CaseSessionAct CaseSessionAct { get; set; }

//        [ForeignKey(nameof(CaseLawUnitId))]
//        public virtual CaseLawUnit CaseLawUnit { get; set; }

//        [ForeignKey(nameof(ActCoordinationTypeId))]
//        public virtual ActCoordinationType ActCoordinationType { get; set; }
//    }
//}
