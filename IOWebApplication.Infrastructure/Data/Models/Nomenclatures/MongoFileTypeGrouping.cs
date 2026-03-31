// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Групиране на видове прикачени документи
    /// </summary>
    [Table("nom_mongo_file_type_grouping")]
    public class MongoFileTypeGrouping
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("mongo_file_type_id")]
        public int MongoFileTypeId { get; set; }

        [Column("type_group")]
        [MaxLength(50)]
        public string TypeGroup { get; set; }

        [ForeignKey(nameof(MongoFileTypeId))]
        public virtual MongoFileType MongoFileType { get; set; }
    }
}
