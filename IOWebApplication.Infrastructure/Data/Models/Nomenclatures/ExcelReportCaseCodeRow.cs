// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    //Статистика - шифри по редове
    [Table("nom_excel_report_case_code_row")]
    public class ExcelReportCaseCodeRow
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        //Има сумарни кодове - главен код + подкодове. Отделени със запетая
        [Column("case_code_id")]
        public string CaseCodeId { get; set; }

        [Column("sheet_index")]
        public int SheetIndex { get; set; }

        [Column("row_index")]
        public int RowIndex { get; set; }

        [Column("court_type_id")]
        public int? CourtTypeId { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }
    }
}
