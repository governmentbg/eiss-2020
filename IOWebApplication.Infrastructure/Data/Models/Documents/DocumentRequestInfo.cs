// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Documents
{
    [Table("document_request_info")]
    public class DocumentRequestInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("document_id")]
        public long? DocumentId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("money_fee_type_id")]
        public int? MoneyFeeTypeId { get; set; }

        [Column("base_amount")]
        public decimal? BaseAmount { get; set; }

        [Column("base_amount_bgn")]
        public decimal? BaseAmountBGN { get; set; }

        [Column("tax_amount")]
        public decimal? TaxAmount { get; set; }

        [Column("tax_amount_bgn")]
        public decimal? TaxAmountBGN { get; set; }

        [Column("for_competency_base")]
        public bool? ForCompetencyBase { get; set; }

        [MaxLength(10)]
        [Column("competency_base_code")]
        public string CompetencyBaseCode { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(MoneyFeeTypeId))]
        public virtual MoneyFeeType MoneyFeeType { get; set; }
    }
}
