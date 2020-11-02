// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид известие: призовка/съобщение
    /// </summary>
    [Table("nom_notification_type")]
    public class NotificationType : BaseCommonNomenclature
    {
        /// <summary>
        /// Призован/Ще бъде призован
        /// </summary>
        [Column("notification_mode_id")]
        [Display(Name = "Уточняване при призоваване")]
        public int NotificationModeId { get; set; }

        /// <summary>
        /// Тип на бланка
        /// </summary>
        [Column("html_template_type_id")]
        [Display(Name = "Вид документ")]
        public int HtmlTemplateTypeId { get; set; }

        [ForeignKey(nameof(NotificationModeId))]
        public virtual NotificationMode NotificationMode { get; set; }

        [ForeignKey(nameof(HtmlTemplateTypeId))]
        public virtual HtmlTemplateType HtmlTemplateType { get; set; }
    }
}
