// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.EISPP
{
    /// <summary>
    /// Номенкалатура с населени места от ЕИСПП
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    [Table("eispp_ektte_code")]
    public class EisppEktteCode
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// ЕИСПП Код 
        /// </summary>
        [Comment("ЕИСПП Код ")]
        [Column("code")]
        public string Code { get; set; }

        /// <summary>
        /// Област
        /// </summary>
        [Comment("Област")]
        [Column("district")]
        public string District { get; set; }

        /// <summary>
        /// Община
        /// </summary>
        [Comment("Община")]
        [Column("municipality")]
        public string Municipality { get; set; }

        /// <summary>
        /// тип н.м.
        /// </summary>
        [Comment("тип н.м.")]
        [Column("type_nm")]
        public string TypeNM { get; set; }

        /// <summary>
        /// Населено място
        /// </summary>
        [Comment("Населено място")]
        [Column("name")]
        public string Name { get; set; }

        /// <summary>
        /// Район
        /// </summary>
        [Comment("Район")]
        [Column("rajon")]
        public string Rajon { get; set; }

        /// <summary>
        /// Системен идентификатор
        /// </summary>
        [Comment("Системен идентификатор")]
        [Column("system_code")]
        public string SystemCode { get; set; }

        /// <summary>
        /// Системно име 
        /// </summary>
        [Comment("Системно име")]
        [Column("system_name")]
        public string SystemName { get; set; }

        /// <summary>
        /// ЕКАТЕ
        /// </summary>
        [Comment("ЕКАТЕ")]
        [Column("ektte_code")]
        public string EktteCode { get; set; }

        /// <summary>
        /// Вид акт/з.. 
        /// </summary>
        [Comment("Статус")]
        [Column("active")]
        public string Active { get; set; }

        /// <summary>
        /// От дата
        /// </summary>
        [Comment("От дата")]
        [Column("date_from")]
        public string DateFrom { get; set; }
        /// <summary>
        /// До дата
        /// </summary>
        [Comment("До дата")]
        [Column("date_to")]
        public string DateTo { get; set; }
    }
}
