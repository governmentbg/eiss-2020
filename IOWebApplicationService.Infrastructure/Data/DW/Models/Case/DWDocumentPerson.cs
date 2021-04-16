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
  [Table("dw_document_person")]
  public class DWDocumentPerson : DWUserDateWRT
  {

    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }

    [Column("id")]
    public long Id { get; set; }

    [Column("document_id")]
    public long DocumentId { get; set; }
    [Column("person_id")]
    public int? PersonId { get; set; }

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


  }
}
