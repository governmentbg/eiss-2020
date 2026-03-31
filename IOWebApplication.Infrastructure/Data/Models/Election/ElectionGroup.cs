// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Election
{
    [Table("election_group")]
    public class ElectionGroup
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("document_id")]
        public long DocumentId { get; set; }

        [Column("label")]
        public string Label { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("election_type_id")]
        public int ElectionTypeId { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("date_wrt")]
        public DateTime DateWrt { get; set; }

        [ForeignKey(nameof(CourtId))]
        public Court Court { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public Document Document { get; set; }

        [ForeignKey(nameof(ElectionTypeId))]
        public ElectionType ElectionType { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }


        public virtual ICollection<ElectionProtocol> ElectionProtocols { get; set;}
        public virtual ICollection<ElectionPerson> ElectionPersons { get; set;}

        public ElectionGroup()
        {
            ElectionProtocols = new HashSet<ElectionProtocol>();
            ElectionPersons = new HashSet<ElectionPerson>();
        }
    }
}
