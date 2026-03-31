using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{   /// <summary>
    /// Видове известия
    /// </summary>
    [Table("nom_work_notification_type")]
    public class WorkNotificationType : BaseCommonNomenclature
    {
        /// <summary>
        /// Флаг дали нотификацията е за бързо производство
        /// </summary>
        [Comment("Флаг дали нотификацията е за бързо производство")]
        [Column("is_fast_process")]
        public bool? IsFastProcess { get; set; }

        /// <summary>
        /// Цвят за календар
        /// </summary>
        [Comment("Цвят за календар")]
        [Column("calendar_color")]
        public string CalendarColor { get; set; }

        /// <summary>
        /// Служители за нотификация - 1 - Съдебен състав (без ръчни роли) / 2 - Служители (само ръчни роли) / 3 - само съдия докладчик / всички
        /// </summary>
        [Comment("Служители за нотификация - 1 - Съдебен състав (без ръчни роли) / 2 - Служители (само ръчни роли) / 3 - само съдия докладчик / всички")]
        [Column("type_person_load")]
        public int? TypePersonLoad { get; set; }
    }
}
