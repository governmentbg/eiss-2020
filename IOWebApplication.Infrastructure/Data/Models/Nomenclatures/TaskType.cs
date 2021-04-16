using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Видове действия, движения
    /// </summary>
    [Table("nom_task_type")]
    public class TaskType : BaseCommonNomenclature
    {
        /// <summary>
        /// Задача, която възниква с изпълнение на други задачи
        /// Неможе да се избира самостоятелно
        /// </summary>
        [Column("automated_task")]
        public bool? AutomatedTask { get; set; }

        /// <summary>
        /// Самоназначаваща се задача
        /// </summary>
        [Column("self_task")]
        public bool? SelfTask { get; set; }

        /// <summary>
        /// Задача, която може да бъде изпълнявана от отдел в структура или към повече от един човек едновременно
        /// </summary>
        [Column("multi_user_task")]

        public bool? MultiUserTask { get; set; }
    }
}
