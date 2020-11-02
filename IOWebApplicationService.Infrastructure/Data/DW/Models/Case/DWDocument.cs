// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplicationService.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Data.DW.Models
{
  [Table("dw_document")]
  public class DWDocument : DWUserDateWRT
  {

    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }

    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// Посока на движение на документ: Входящи, Изходящи, вътрешен
    /// </summary>
    [Column("document_direction_id")]
    public int DocumentDirectionId { get; set; }

    [Column("document_direction_name")]
    public string DocumentDirectionName { get; set; }

    [Display(Name = "Основен вид документ")]
    [Column("document_group_id")]
    public int DocumentGroupId { get; set; }

    [Display(Name = "Основен вид документ")]
    [Column("document_group_name")]
    public string DocumentGroupName { get; set; }

    [Display(Name = "Точен вид документ")]
    [Column("document_type_id")]
    public int DocumentTypeId { get; set; }

    [Display(Name = "Точен вид документ")]
    [Column("document_type_name")]
    public string DocumentTypeName{ get; set; }

    [Column("document_number_value")]
    public int? DocumentNumberValue { get; set; }

    [Column("document_number")]
    public string DocumentNumber { get; set; }

    [Column("document_date")]
    public DateTime DocumentDate { get; set; }

    [Column("actual_document_date")]
    public DateTime? ActualDocumentDate { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("is_resticted_access")]
    public bool IsRestictedAccess { get; set; }

    [Column("is_secret")]
    public bool? IsSecret { get; set; }

    [Column("is_old_number")]
    public bool? IsOldNumber { get; set; }

    /// <summary>
    /// Начин на изпращане: Призовкар,Поща,куриер,факс
    /// </summary>
    [Column("delivery_group_id")]
    public int? DeliveryGroupId { get; set; }
    [Column("delivery_group_name")]
    public string DeliveryGroupName { get; set; }

    /// <summary>
    /// Указания за изпращане: Обикновено,препоръчано,колет
    /// </summary>
    [Column("delivery_type_id")]
    public int? DeliveryTypeId { get; set; }
    [Column("delivery_type_name")]
    public string DeliveryTypeName { get; set; }

  }
}
