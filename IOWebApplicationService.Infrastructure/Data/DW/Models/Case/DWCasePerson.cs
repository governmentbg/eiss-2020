// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplicationService.Infrastructure.Data.DW.Models;
using IOWebApplicationService.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplicationService.Infrastructure.Data.DW.Models
{
  /// <summary>
  /// Съдебен състав по дело - заседатели
  /// </summary>
  [Table("dw_case_person")]
  public class DWCasePerson : DWUserDateWRT
  {
    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }

    [Column("id")]
    public int Id { get; set; }

   

    [Column("case_id")]
    public int CaseId { get; set; }


    [Column("uic")]
    [Display(Name = "Идентификатор")]
    public string Uic { get; set; }

    [Column("uic_type_id")]
    [Display(Name = "Вид идентификатор")]
    public int UicTypeId { get; set; }
    [Column("uic_type_name")]
    [Display(Name = "Вид идентификатор")]
    public string UicTypeName { get; set; }

    [Column("first_name")]
    [Display(Name = "Собствено име")]
    public string FirstName { get; set; }

    [Column("middle_name")]
    [Display(Name = "Бащино име")]
    public string MiddleName { get; set; }

    [Column("family_name")]
    [Display(Name = "Фамилия 1")]
    public string FamilyName { get; set; }

    [Column("family_2_name")]
    [Display(Name = "Фамилия 2")]
    public string Family2Name { get; set; }

    [Column("full_name")]
    [Display(Name = "Наименование")]
    public string FullName { get; set; }

    [Column("department_name")]
    public string DepartmentName { get; set; }

    [Column("latin_name")]
    public string LatinName { get; set; }

    [Column("person_source_type")]
    public int? Person_SourceType { get; set; }
    [Column("person_source_type_name")]
    public string Person_SourceTypeName { get; set; }
    [Column("person_source_id")]
    public long? Person_SourceId { get; set; }
    [Column("person_source_name")]
    public string Person_SourceName { get; set; }

    [Column("case_session_id")]
    public int? CaseSessionId { get; set; }



    [Column("person_role_id")]
    public int PersonRoleId { get; set; }
    [Column("person_role_name")]
    public string PersonRoleName { get; set; }

    [Column("military_rang_id")]
    public int? MilitaryRangId { get; set; }
    [Column("military_rang_name")]
    public string MilitaryRangName { get; set; }

    [Column("person_maturity_id")]
    public int? PersonMaturityId { get; set; }

    [Column("person_maturity_name")]
    public string PersonMaturityName { get; set; }

    /// <summary>
    /// Първоначална страна по делото, добавена със създаването му
    /// </summary>
    [Column("is_initial_person")]
    public bool IsInitialPerson { get; set; }

    /// <summary>
    /// Guid на лице, при добавяне в делото се генерира и се пренася във всички заседания
    /// </summary>
    [Column("case_person_identificator")]
    public string CasePersonIdentificator { get; set; }

    [Column("date_from")]
    public DateTime DateFrom { get; set; }
    [Column("date_from_str")]
    public string DateFromStr { get; set; }

    [Column("date_to")]
    public DateTime? DateTo { get; set; }
    [Column("date_to_str")]
    public string DateToStr { get; set; }

    [Column("row_number")]
    public int RowNumber { get; set; }

    /// <summary>
    /// Да бъде призован
    /// </summary>
    [Column("for_notification")]
    public bool? ForNotification { get; set; }

    [Column("notification_number")]
    public int? NotificationNumber { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }
    [Column("date_wrt")]
    public DateTime DateWrt { get; set; }



    [Column("date_transfered_dw")]
    public DateTime? DateTransferedDW { get; set; }

    /// <summary>
    /// Задържан
    /// </summary>
    [Column("is_arrested")]
    public bool? IsArrested { get; set; }

    //Място на раждане
    [Column("birth_country_code")]
    [Display(Name = "Държава")]
    public string BirthCountryCode { get; set; }
    [Column("birth_country_name")]
    [Display(Name = "Държава")]
    public string BirthCountryName { get; set; }

    [Column("birth_city_code")]
    [Display(Name = "Населено място")]
    public string BirthCityCode { get; set; }
    [Column("birth_city_name")]
    [Display(Name = "Населено място")]
    public string BirthCityName { get; set; }

    //Ако не е България - текстово поле
    [Column("birth_foreign_place")]
    [Display(Name = "Населено място")]
    public string BirthForeignPlace { get; set; }

    [Column("date_expired")]
    [Display(Name = "Дата на анулиране")]
    public DateTime? DateExpired { get; set; }

    [Column("date_expired_str")]
    [Display(Name = "Дата на анулиране")]
    public string DateExpiredStr { get; set; }

    [Column("user_expired_id")]
    public string UserExpiredId { get; set; }
    [Column("user_expired_name")]
    public string UserExpiredName { get; set; }

    [Column("description_expired")]
    [Display(Name = "Причина за анулиране")]
    public string DescriptionExpired { get; set; }

    [Column("link_relations_string")]
    [Display(Name = "Връзки в делото")]
    public string  LinkRelationsString{ get; set; }

  }
}
