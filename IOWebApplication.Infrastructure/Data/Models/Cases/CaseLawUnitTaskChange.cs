// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Регистър на смяна на изпълнител на задача при обективна невъзможност за изпълнение
    /// </summary>
    [Table("case_lawunit_task_change")]
    public class CaseLawUnitTaskChange : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("case_id")]
        [Display(Name ="Съдебно дело")]
        public int CaseId { get; set; }

        [Column("case_session_id")]
        [Display(Name = "Съдебен акт")]
        [Required(ErrorMessage ="Изберете {0}.")]
        public int CaseSessionActId { get; set; }

        [Column("work_task_id")]
        [Display(Name = "Задача")]
        [Required(ErrorMessage = "Изберете {0}.")]
        public long WorkTaskId { get; set; }

        [Column("new_task_user_id")]
        [Display(Name = "Нов изпълнител на задачата")]
        public string NewTaskUserId { get; set; }        

        [Column("description")]
        [Display(Name = "Основание")]
        public string Description { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }

        [ForeignKey(nameof(WorkTaskId))]
        public virtual WorkTask WorkTask { get; set; }

        [ForeignKey(nameof(NewTaskUserId))]
        public virtual ApplicationUser NewTaskUser { get; set; }

    }
}
