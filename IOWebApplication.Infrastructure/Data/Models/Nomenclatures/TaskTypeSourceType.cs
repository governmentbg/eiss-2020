using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Видове задачи според вида обект
    /// </summary>
    [Table("nom_task_type_source_type")]
    public class TaskTypeSourceType 
    {
        [Column("task_type_id")]
        public int TaskTypeId { get; set; }

        [Column("source_type")]
        public int SourceType { get; set; }

        [ForeignKey(nameof(TaskTypeId))]
        public virtual TaskType TaskType { get; set; }
    }
}
