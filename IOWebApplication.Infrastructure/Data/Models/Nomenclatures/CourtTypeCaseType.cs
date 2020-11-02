// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Точни видове дела към вид съд
    /// </summary>
    [Table("nom_court_type_case_type")]
    public class CourtTypeCaseType
    {
        [Column("court_type_id")]
        public int CourtTypeId { get; set; }

        [Column("case_type_id")]
        public int CaseTypeId { get; set; }

        //в кой ред на ексела се записва в зависимост от StatisticsConstants.ReportTypes
        [Column("excel_report_col")]
        public string ExcelReportCol { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }

        [ForeignKey(nameof(CaseTypeId))]
        public virtual CaseType CaseType { get; set; }
    }
}
