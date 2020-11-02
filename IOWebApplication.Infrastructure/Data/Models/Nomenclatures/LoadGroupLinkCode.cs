// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Шифри по Вид съд и инстанция към Групи по натовареност
    /// </summary>
    [Table("nom_load_group_link_code")]
    public class LoadGroupLinkCode
    {
        [Column("load_group_link_id")]
        public int LoadGroupLinkId { get; set; }

        [Column("case_code_id")]
        public int CaseCodeId { get; set; }
      
        [ForeignKey(nameof(LoadGroupLinkId))]
        public virtual LoadGroupLink LoadGroupLink { get; set; }

        [ForeignKey(nameof(CaseCodeId))]
        public virtual CaseCode CaseCode { get; set; }
    }
}
