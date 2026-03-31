// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Election
{
    [Table("election_protocol")]
    public class ElectionProtocol
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("election_group_id")]
        public int ElectionGroupId { get; set; }

        //Нарочно е нулево, за да може да се запише без избрано и след записа на колекцията  ElectionPersons да се редактира на правилното
        [Column("selected_election_person_id")]
        public int? SelectedElectionPersonId { get; set; }

        [Column("selected_lawunit_id")]
        public int? SelectedLawunitId { get; set; }

        [Column("selected_lawunit_fullname")]
        public string SelectedLawunitFullName { get; set; }

        [Column("selected_lawunit_court_id")]
        public int? SelectedLawunitCourtId { get; set; }

        [Column("prev_lawunit_id")]
        public int? PrevLawunitId { get; set; }

        [Column("prev_lawunit_fullname")]
        public string PrevLawunitFullName { get; set; }

        [Column("prev_lawunit_court_id")]
        public int? PrevLawunitCourtId { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("user_added_id")]
        public string UserAddedId { get; set; }

        [Column("date_added")]
        public DateTime DateAdded { get; set; }

        [Column("user_election_id")]
        public string UserElectionId { get; set; }

        [Column("date_election")]
        public DateTime? DateElection { get; set; }

        [Column("date_signed")]
        public DateTime? DateSigned { get; set; }


        [ForeignKey(nameof(ElectionGroupId))]
        public ElectionGroup ElectionGroup { get; set; }

        [ForeignKey(nameof(SelectedElectionPersonId))]
        public ElectionPerson SelectedElectionPerson { get; set; }

        [ForeignKey(nameof(SelectedLawunitId))]
        public LawUnit SelectedLawunit { get; set; }

        [ForeignKey(nameof(SelectedLawunitCourtId))]
        public Court SelectedLawunitCourt { get; set; }

        [ForeignKey(nameof(PrevLawunitId))]
        public LawUnit PrevLawunit { get; set; }

        [ForeignKey(nameof(PrevLawunitCourtId))]
        public Court PrevLawunitCourt { get; set; }


        [ForeignKey(nameof(UserAddedId))]
        public ApplicationUser UserAdded { get; set; }

        [ForeignKey(nameof(UserElectionId))]
        public ApplicationUser UserElection { get; set; }

        [InverseProperty(nameof(ElectionPerson.ElectionProtocol))]
        public virtual ICollection<ElectionPerson> ElectionPersons { get; set; }

        public ElectionProtocol()
        {
            ElectionPersons = new HashSet<ElectionPerson>();
        }
    }
}
