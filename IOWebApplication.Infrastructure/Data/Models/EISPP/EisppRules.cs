// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.EISPP
{
    /// <summary>
    /// Правила за допустимост на пропъртита по събития за ЕИСПП
    /// </summary>
    [Table("nom_eispp_rules")]
    public class EisppRules
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
        /// Обект.Поле.
        /// </summary>
        [Column("item_name")]
        [Display(Name = "Обект или поле")]
        public string ItemName { get; set; }

        /// <summary>
        /// Дали е задължително и допустимо полето или обекта
        /// </summary>
        [Column("flag")]
        [Display(Name = "Вид")]
        public string Flag { get; set; }

        /// <summary>
        /// Допустими стоиности
        /// </summary>
        [Column("values")]
        [Display(Name = "Допустими стоиности")]
        public string Values { get; set; }

        /// <summary>
        /// Флаг така както е дошъл от еиспп
        /// </summary>
        [Column("flag_real")]
        [Display(Name = "Вид")]
        public string FlagReal { get; set; }
    }
}
