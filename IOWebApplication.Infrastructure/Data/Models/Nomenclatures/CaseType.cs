// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Точен вид дело: НОХД,ГД,ЧНД, ВНОХД,ВЧНД,ВНЧХД
    /// </summary>
    [Table("nom_case_type")]
    public class CaseType : BaseCommonNomenclature
    {
        [Column("case_instance_id")]
        [Display(Name = "Инстанция")]
        public int CaseInstanceId { get; set; }

        [Column("case_group_id")]
        [Display(Name ="Основен вид дело")]
        public int CaseGroupId { get; set; }       

        [Column("report_group_azbuchnik")]
        public int? ReportGroupAzbuchnik { get; set; }

        [Column("eispp_code")]
        [Display(Name = "ЕИСПП код")]
        public string EISPPCode { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }

        [ForeignKey(nameof(CaseInstanceId))]
        public virtual CaseInstance CaseInstance { get; set; }

    }
}
