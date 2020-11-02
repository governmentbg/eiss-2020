// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.EISPP
{
    /// <summary>
    /// Събития ЕИСПП 
    /// </summary>
    [Table("eispp_event_item")]
    public class EisppEventItem : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        /// <summary>
        /// sbevid
        /// вид на събитието
        /// системен код на елемент от nmk_sbevid
        /// </summary>
        [Column("event_type")]
        [Display(Name = "Вид на събитието")]
        public int EventType { get; set; }

        /// <summary>
        /// sbevid
        /// EventType е EisppRules за изтриване и корекция
        /// </summary>
        [Column("event_type_rules")]
        [Display(Name = "Вид на връзка")]
        public int? EventTypeRules { get; set; }

        /// <summary>
        /// До дата на събитие
        /// </summary>
        [Display(Name = "Дата събитие")]
        public DateTime? EventDate { get; set; }

        [Column("event_from_id")]
        public int? EventFromId { get; set; }

        [Column("mq_epep_id")]
        public long? MQEpepId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_session_id")]
        public int? CaseSessionId { get; set; }

        [Column("case_session_act_id")]
        public int? CaseSessionActId { get; set; }

        [Column("case_preson_id")]
        public int? CasePersonId { get; set; }

        [Column("preson_old_measure_id")]
        public int? PersonOldMeasureId { get; set; }

        [Column("preson_measure_id")]
        public int? PersonMeasureId { get; set; }

        [Column("punishment_id")]
        public int? PunishmentId { get; set; }

        [Column("source_type")]
        public int SourceType { get; set; }

        [Column("source_id")]
        public long SourceId { get; set; }

        [Column("request_data", TypeName = "jsonb")]
        public string RequestData { get; set; }

        [Column("response_data", TypeName = "jsonb")]
        public string ResponseData { get; set; }

        [Column("request_xml")]
        public string RequestXML { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }
        
        [ForeignKey(nameof(MQEpepId))]
        public virtual MQEpep MQEpep { get; set; }

        [ForeignKey(nameof(CaseSessionId))]
        public virtual CaseSession CaseSession { get; set; }

        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }
    }
}
