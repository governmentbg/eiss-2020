// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Election
{
    [Table("election_person")]
    public class ElectionPerson
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("election_group_id")]
        public int ElectionGroupId { get; set; }

        [Column("election_protocol_id")]
        public int? ElectionProtocolId { get; set; }

        [Column("person_gid")]
        public Guid PersonGid { get; set; }

        [Column("lawunit_id")]
        public int LawunitId { get; set; }

        [Column("lawunit_fullname")]
        public string LawunitFullName { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("user_added_id")]
        public string UserAddedId { get; set; }

        [Column("date_added")]
        public DateTime DateAdded { get; set; }


        [Column("has_decalration")]
        public bool? HasDeclaration { get; set; }

        [Column("date_decalration")]
        public DateTime? DateDeclaration { get; set; }

        //--статус в протокола
        [Column("election_person_state_id")]
        [Display(Name = "Участие в разпределение")]
        public int ElectionPersonStateId { get; set; }

        [Column("description_state")]
        [Display(Name = "Основание")]
        public string DescriptionState { get; set; }
        //--статус в протокола

        //---отвода
        [Column("election_person_dismissal_type_id")]
        [Display(Name = "Причина за неучастие")]
        public int? ElectionPersonDismissalTypeId { get; set; }

        [Column("user_removed_id")]
        public string UserRemovedId { get; set; }

        [Column("date_removed_id")]
        public DateTime? DateRemoved { get; set; }

        [Column("description_removed")]
        [Display(Name = "Основание за неучастие")]
        public string DescriptionRemoved { get; set; }
        //---отвода

        [ForeignKey(nameof(ElectionGroupId))]
        public ElectionGroup ElectionGroup { get; set; }

        [ForeignKey(nameof(ElectionProtocolId))]
        public ElectionProtocol ElectionProtocol { get; set; }

        [ForeignKey(nameof(LawunitId))]
        public LawUnit LawUnit { get; set; }

        [ForeignKey(nameof(CourtId))]
        public Court Court { get; set; }

        [ForeignKey(nameof(ElectionPersonStateId))]
        public ElectionPersonState State { get; set; }

        [ForeignKey(nameof(ElectionPersonDismissalTypeId))]
        public ElectionPersonDismissalType DismissalType { get; set; }

        [ForeignKey(nameof(UserAddedId))]
        public ApplicationUser UserAdded { get; set; }

        [ForeignKey(nameof(UserRemovedId))]
        public ApplicationUser UserRemoved { get; set; }
    }
}
