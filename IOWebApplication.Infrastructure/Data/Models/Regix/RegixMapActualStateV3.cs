// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Regix
{
    /// <summary>
    /// Връзки в полета при интеграция с Regix
    /// </summary>
    [Table("regix_map_actual_state")]
    public class RegixMapActualStateV3
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("field_ident")]
        public string FieldIdent { get; set; }

        [Column("fields")]
        public string Fields { get; set; }

        [Column("labels")]
        public string Labels { get; set; }

        /// <summary>
        /// field_object = Обект, field_code - Код
        /// </summary>
        [Column("type_field")]
        public string TypeField { get; set; }

        /// <summary>
        /// Този код дали да се показва в справката
        /// </summary>
        [Column("for_display")]
        public bool? ForDisplay { get; set; }

        /// <summary>
        /// Има ли обект в кода за да се замени с полетата му
        /// </summary>
        [Column("has_object")]
        public bool? HasObject { get; set; }

    }
}
