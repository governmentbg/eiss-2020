// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Роля на лицето в престъпление
    /// </summary>
    [Table("nom_person_role_in_crime")]
    public class PersonRoleInCrime : BaseCommonNomenclature
    {
    }
}
