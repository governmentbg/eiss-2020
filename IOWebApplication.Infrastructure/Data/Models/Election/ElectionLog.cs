// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Election
{
    [Table("election_log")]
    public class ElectionLog
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("election_group_id")]
        public int ElectionGroupId { get; set; }

        [Column("election_protocol_id")]
        public int? ElectionProtocolId { get; set; }
        [Column("election_person_id")]
        public int? ElectionPersonlId { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("date_wrt")]
        public DateTime DateWrt { get; set; }

        [Column("person_gid")]
        public Guid? PersonGid { get; set; }

        [Column("lawunit_id")]
        public int? LawunitId { get; set; }

        [Column("operation")]
        public string Operation { get; set; }

        [Column("description")]
        public string Description { get; set; }


        [ForeignKey(nameof(ElectionGroupId))]
        public ElectionGroup ElectionGroup { get; set; }

        [ForeignKey(nameof(ElectionProtocolId))]
        public ElectionProtocol ElectionProtocol { get; set; }
        [ForeignKey(nameof(ElectionPersonlId))]
        public ElectionPerson ElectionPerson { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

    }
}
