using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Възможни изборни за дадено действие
    /// Прим: Без движение, върнато, за съставяне на дело, пренасочено
    /// </summary>
    [Table("nom_task_action")]
    public class TaskAction : BaseCommonNomenclature
    {
        [Column("task_type_id")]
        public int TaskTypeId { get; set; }

        [ForeignKey(nameof(TaskTypeId))]
        public virtual TaskType TaskType { get; set; }
    }
}
