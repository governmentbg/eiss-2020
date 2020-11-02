// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Messages
{
    [Table("common_news_user")]
    public class NewsUser
    {
        [Column("news_id")]
        public int NewsId { get; set; }

        public News News { get; set; }

        [Column("user_id")]
        [StringLength(50)]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }
    }
}
