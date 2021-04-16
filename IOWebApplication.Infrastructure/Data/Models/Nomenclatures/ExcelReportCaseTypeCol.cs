// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Статистика Данни по CaseType по колони - Справките за съдиите
    /// </summary>
    [Table("nom_excel_report_case_type_col")]
    public class ExcelReportCaseTypeCol
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("court_type_id")]
        public int CourtTypeId { get; set; }

        //CaseType отделени със запетая
        [Column("case_type_id")]
        public string CaseTypeId { get; set; }

        //Видове документи отделени със запетая
        [Column("document_type_id")]
        public string DocumentTypeId { get; set; }

        //CaseCode отделени със запетая
        [Column("case_code_id")]
        public string CaseCodeId { get; set; }

        [Column("col_index")]
        public int ColIndex { get; set; }

        //Вид на справката - там са 5 справки
        [Column("report_type_id")]
        public int ReportTypeId { get; set; }

        [Column("is_true")]
        public bool IsTrue { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }
    }
}
