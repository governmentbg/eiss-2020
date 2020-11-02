// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Адреси към лица
    /// </summary>
    [Table("common_lawunit_address")]
    public class LawUnitAddress
    {
      
        [Column("lawunit_id")]
        public int LawUnitId { get; set; }
       
        [Column("address_id")]
        public long AddressId { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }
       
        [ForeignKey(nameof(AddressId))]
        public virtual Address Address { get; set; }
    }
}
