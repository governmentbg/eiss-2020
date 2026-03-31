// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Documents
{
    [Table("electronic_document")]
    public class ElectronicDocument
    {
        public ElectronicDocument()
        {
            Persons = new HashSet<ElectronicDocumentPerson>();
        }

        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("epep_id")]
        public Guid EpepId { get; set; }

        [Column("epep_user_id")]
        public int EpepUserId { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("document_kind_id")]
        public int DocumentKindId { get; set; }

        [Column("request_type_code")]
        [MaxLength(50)]
        public string RequestTypeCode { get; set; }

        [Column("document_group_id")]
        public int DocumentGroupId { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_person_id")]
        public int? CasePersonId { get; set; }

        [Column("apply_number")]
        public string ApplyNumber { get; set; }

        [Column("apply_date")]
        public DateTime ApplyDate { get; set; }

        [Column("currency_code")]
        public string CurrencyCode { get; set; }

        [Column("money_fee_type_id")]
        public int? MoneyFeeTypeId { get; set; }

        [Column("base_amount")]
        public decimal? BaseAmount { get; set; }

        [Column("tax_amount")]
        public decimal? TaxAmount { get; set; }

        [Column("paid_date")]
        public DateTime? PaidDate { get; set; }

        [Column("payment_type_id")]
        public int? PaymentTypeId { get; set; }

        //Дата на приемане в ЕИСС от ЕПЕП
        [Column("date_court_accept")]
        public DateTime? DateCourtAccept { get; set; }

        //Дата на регистриране в ЕИСС
        [Column("document_date")]
        public DateTime? DocumentDate { get; set; }

        /// <summary>
        /// Платено на ВПОС към избран съд - при подаване
        /// </summary>
        [Column("vpos_paid_in_court_id")]
        public int? VPOSPaidInCourtId { get; set; }

        [Column("from_api")]
        public bool? FromAPI { get; set; }

        /// <summary>
        /// Имената на лицето, с API ключа на което е подаден документа
        /// </summary>
        [Column("create_user_name")]
        public string CreateUserName { get; set; }

        [ForeignKey(nameof(EpepUserId))]
        public virtual EpepUser EpepUser { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(DocumentKindId))]
        public virtual DocumentKind DocumentKind { get; set; }

        [ForeignKey(nameof(DocumentGroupId))]
        public virtual DocumentGroup DocumentGroup { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }

        [ForeignKey(nameof(MoneyFeeTypeId))]
        public virtual MoneyFeeType MoneyFeeType { get; set; }

        [ForeignKey(nameof(PaymentTypeId))]
        public virtual PaymentType PaymentType { get; set; }

        [ForeignKey(nameof(VPOSPaidInCourtId))]
        public virtual Court VPOSPaidInCourt { get; set; }

        public virtual ICollection<ElectronicDocumentPerson> Persons { get; set; }


        [NotMapped]
        public string FileError { get; set; }

        public string MapErrorDescription
        {
            get
            {
                string result = "";

                if (CourtId <= 0 && string.IsNullOrEmpty(RequestTypeCode))
                {
                    result += "CourtId;";
                }
                if (DocumentKindId <= 0)
                {
                    result += "DocumentKindId;";
                }
                if (DocumentGroupId <= 0)
                {
                    result += "DocumentGroupId;";
                }
                if (EpepUserId <= 0)
                {
                    result += "EpepUserId;";
                }


                if (Persons != null)
                {
                    foreach (var person in Persons)
                    {
                        if (person.PersonRoleId <= 0)
                        {
                            result += $"person {person.FullName} - role;";
                        }
                        if (person.CitizenshipId == 0)
                        {
                            result += $"person {person.FullName} - citizenship;";
                        }
                    }
                }

                if (!string.IsNullOrEmpty(FileError))
                {
                    result += $"; file error: {FileError}";
                }

                return result;

            }
        }
    }
}
