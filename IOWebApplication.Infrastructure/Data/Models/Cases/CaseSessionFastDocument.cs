// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Съпровождащ документ представен в заседание
    /// </summary>
    [Table("case_session_fast_document")]
    public class CaseSessionFastDocument : BaseInfo_CaseSessionFastDocument, IHaveHistory<CaseSessionFastDocumentH>, IExpiredInfo
    {
        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseSessionId))]
        public virtual CaseSession CaseSession { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }

        [ForeignKey(nameof(SessionDocTypeId))]
        public virtual SessionDocType SessionDocType { get; set; }

        [ForeignKey(nameof(SessionDocStateId))]
        public virtual SessionDocState SessionDocState { get; set; }

        [ForeignKey(nameof(CaseSessionFastDocumentInitId))]
        public virtual CaseSessionFastDocument CaseSessionFastDocumentInit { get; set; }

        public virtual ICollection<CaseSessionFastDocumentH> History { get; set; }
        
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
    }

    /// <summary>
    /// Съпровождащ документ представен в заседание - история
    /// </summary>
    [Table("case_session_fast_document_h")]
    public class CaseSessionFastDocumentH : BaseInfo_CaseSessionFastDocument, IHistory
    {
        [Column("history_id")]
        public int HistoryId { get; set; }

        [Column("history_date_expire")]
        public DateTime? HistoryDateExpire { get; set; }

        [ForeignKey(nameof(Id))]
        public virtual CaseSessionFastDocument CaseSessionFastDocument { get; set; }
    }
    public class BaseInfo_CaseSessionFastDocument : UserDateWRT
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_session_id")]
        public int CaseSessionId { get; set; }

        [Column("case_person_id")]
        [Display(Name = "Свързано лице")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int CasePersonId { get; set; }

        [Column("session_doc_type_id")]
        [Display(Name = "Вид документ")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int SessionDocTypeId { get; set; }

        [Column("description")]
        [Display(Name = "Забележка")]
        public string Description { get; set; }

        /// <summary>
        /// Представен; Неразгледан; Разгледан; Окончателно разгледан;
        /// </summary>
        [Column("session_doc_state_id")]
        [Display(Name = "Статус")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int SessionDocStateId { get; set; }

        [Column("case_session_fast_document_init_id")]
        public int? CaseSessionFastDocumentInitId { get; set; }
    }
}
