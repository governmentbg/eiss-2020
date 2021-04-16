using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Движение/задачи 
    /// </summary>
    [Table("common_worktask")]
    public class WorkTask
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("parent_task_id")]
        public long? ParentTaskId { get; set; }

        [Column("source_type")]
        public int SourceType { get; set; }

        [Column("source_id")]
        public long SourceId { get; set; }

        [Column("sub_source_id")]
        public long? SubSourceId { get; set; }

        [Column("task_type_id")]
        [Display(Name = "Вид задача")]
        public int TaskTypeId { get; set; }

        [Display(Name = "Действие")]
        [Column("task_action_id")]
        public int? TaskActionId { get; set; }

        [Display(Name = "Начин на изпълнение")]
        [Column("task_execution_id")]
        public int? TaskExecutionId { get; set; }

        [Display(Name = "Изберете потребител")]
        [Column("user_id")]
        public string UserId { get; set; }

        [Column("court_organization_id")]
        [Display(Name = "Структура")]
        public int? CourtOrganizationId { get; set; }

        [Column("description")]
        [Display(Name = "Забележка")]
        public string Description { get; set; }

        [Column("description_created")]
        [Display(Name = "Описание")]
        public string DescriptionCreated { get; set; }

        [Column("user_created_id")]
        public string UserCreatedId { get; set; }

        [Column("task_state_id")]
        public int TaskStateId { get; set; }

        [Column("date_created")]
        [Display(Name = "Дата на създаване")]
        public DateTime DateCreated { get; set; }

        [Column("date_end")]
        [Display(Name = "Срок за изпълнение")]
        public DateTime? DateEnd { get; set; }

        [Column("date_accepted")]
        public DateTime? DateAccepted { get; set; }

        [Column("date_completed")]
        public DateTime? DateCompleted { get; set; }

        [Column("source_description")]
        [Display(Name = "Описание на обекта")]
        public string SourceDescription { get; set; }

        [Column("parent_description")]
        [Display(Name = "Описание на обекта")]
        public string ParentDescription { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(ParentTaskId))]
        public virtual WorkTask ParentTask { get; set; }

        [ForeignKey(nameof(TaskTypeId))]
        public virtual TaskType TaskType { get; set; }

        [ForeignKey(nameof(TaskActionId))]
        public virtual TaskAction TaskAction { get; set; }

        [ForeignKey(nameof(TaskExecutionId))]
        public virtual TaskExecution TaskExecution { get; set; }

        [ForeignKey(nameof(TaskStateId))]
        public virtual TaskState TaskState { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey(nameof(UserCreatedId))]
        public virtual ApplicationUser UserCreated { get; set; }

        [ForeignKey(nameof(CourtOrganizationId))]
        public virtual CourtOrganization CourtOrganization { get; set; }
    }
}
