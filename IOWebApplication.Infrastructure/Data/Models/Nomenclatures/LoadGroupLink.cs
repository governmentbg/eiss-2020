// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид съд и инстанция Групи по натовареност
    /// </summary>
    [Table("nom_load_group_link")]
    public class LoadGroupLink
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("load_group_id")]
        public int LoadGroupId { get; set; }

        [Column("court_type_id")]
        [Display(Name = "Вид съд")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int CourtTypeId { get; set; }

        [Column("case_instance_id")]
        [Display(Name = "Инстанция")]
        public int? CaseInstanceId { get; set; }

        [Column("load_index")]
        [Display(Name = "Натовареност %")]
        public decimal LoadIndex { get; set; }

        [ForeignKey(nameof(LoadGroupId))]
        public virtual LoadGroup LoadGroup { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }

        [ForeignKey(nameof(CaseInstanceId))]
        public virtual CaseInstance CaseInstance { get; set; }

        public virtual ICollection<LoadGroupLinkCode> GroupCodes { get; set; }
    }
}
