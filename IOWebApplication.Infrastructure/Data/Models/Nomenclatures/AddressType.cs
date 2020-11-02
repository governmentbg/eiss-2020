// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид адрес: постоянен, настоящ, месторабота, друг
    /// </summary>
    [Table("nom_address_type")]
    public class AddressType : BaseCommonNomenclature
    {      

    }
}
