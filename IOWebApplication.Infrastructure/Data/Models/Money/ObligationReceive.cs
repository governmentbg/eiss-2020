// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Money
{
    /// <summary>
    /// Кой трябва да получи парите
    /// </summary>
    [Table("money_obligation_receive")]
    public class ObligationReceive : PersonNamesBase
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("obligation_id")]
        public int ObligationId { get; set; }

        [Column("case_person_id")]
        public int? CasePersonId { get; set; }

        [Column("exec_list_type_id")]
        public int? ExecListTypeId { get; set; }

        [Column("Iban")]
        public string Iban { get; set; }

        [Column("bic")]
        public string BIC { get; set; }

        [Column("bank_name")]
        public string BankName { get; set; }


        [ForeignKey(nameof(ObligationId))]
        public virtual Obligation Obligation { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }

        [ForeignKey(nameof(ExecListTypeId))]
        public virtual ExecListType ExecListType { get; set; }
    }
}
