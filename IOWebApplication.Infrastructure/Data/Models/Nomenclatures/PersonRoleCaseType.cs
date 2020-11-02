// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Роля на лицето, ищец,ответник,свидетел,експерт
    /// </summary>
    [Table("nom_person_role_case_type")]
    public class PersonRoleCaseType 
    {
        [Column("person_role_id")]
        public int PersonRoleId { get; set; }

        [Column("case_type_id")]
        public int CaseTypeId { get; set; }

        [ForeignKey(nameof(PersonRoleId))]
        public virtual PersonRole PersonRole { get; set; }

        [ForeignKey(nameof(CaseTypeId))]
        public virtual CaseType CaseType { get; set; }
    }
}
