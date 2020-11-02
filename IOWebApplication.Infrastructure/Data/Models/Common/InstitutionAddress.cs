// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Адреси към институция
    /// </summary>
    [Table("common_institution_address")]
    public class InstitutionAddress
    {
      
        [Column("institution_id")]
        public int InstitutionId { get; set; }
       
        [Column("address_id")]
        public long AddressId { get; set; }

        [ForeignKey(nameof(InstitutionId))]
        public virtual Institution Institution { get; set; }

        [ForeignKey(nameof(AddressId))]
        public virtual Address Address { get; set; }
    }
}
