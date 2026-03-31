// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Начин на плащане - ПОС, Платежно
    /// </summary>
    [Table("nom_bank_file_payment_type")]
    public class BankFilePaymentType : BaseCommonNomenclature
    {
    }
}
