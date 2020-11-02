// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Delivery
{  /// <summary>
   /// Получени архивни файлове от мобилни утройства
   /// </summary>
    [Table("delivery_mobile_file")]
    public class DeliveryMobileFile
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("delivery_account_id")]
        public string DeliveryAccountId { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("content")]
        public byte[] Content { get; set; }

        [Column("is_checked")]
        public bool? IsChecked { get; set; }

        [Column("error_message")]
        public string ErrorMessage { get; set; }
    }
}

