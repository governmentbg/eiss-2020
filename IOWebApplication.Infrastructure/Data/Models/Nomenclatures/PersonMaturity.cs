// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Възраст на лицето: Пълнолетен, непълнолетен, малолетен
    /// </summary>
    [Table("nom_person_maturity")]
    public class PersonMaturity : BaseCommonNomenclature
    {
       
    }
}
