// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид съд
    /// </summary>
    [Table("nom_court_type")]
    public class CourtType : BaseCommonNomenclature
    {
        [Column("main_court_type_id")]
        public int? MainCourtTypeId { get; set; }

        /// <summary>
        /// comma separated instance list
        /// </summary>
        [Column("instance_list")]
        public string InstanceList { get; set; }
    }
}
