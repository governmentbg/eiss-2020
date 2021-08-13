using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace IOWebApplication.Infrastructure.Data.Models.Base
{
    public class NamesBase
    {
        [Column("uic")]
        [Display(Name = "Идентификатор")]
        public string Uic { get; set; }

        [Column("uic_type_id")]
        [Display(Name = "Вид идентификатор")]
        public int UicTypeId { get; set; }

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
        [Display(Name = "Отдел/структура")]
        public string DepartmentName { get; set; }

        [Column("latin_name")]
        public string LatinName { get; set; }

        [Column("is_deceased")]
        [Display(Name = "Починало лице")]
        public bool? IsDeceased { get; set; }

        [Column("date_deceased")]
        [Display(Name = "Дата на смъртта")]
        public DateTime? DateDeceased { get; set; }

        [Column("person_source_type")]
        public int? Person_SourceType { get; set; }
        [Column("person_source_id")]
        public long? Person_SourceId { get; set; }

        [Column("person_source_code")]
        public string Person_SourceCode { get; set; }

        [ForeignKey(nameof(UicTypeId))]
        public virtual UicType UicType { get; set; }



        public string UicTypeLabel
        {
            get
            {
                switch (this.UicTypeId)
                {
                    case NomenclatureConstants.UicTypes.EGN:
                        return "ЕГН";
                    case NomenclatureConstants.UicTypes.LNCh:
                        return "ЛНЧ";
                    case NomenclatureConstants.UicTypes.EIK:
                        return "ЕИК";
                    case NomenclatureConstants.UicTypes.Bulstat:
                        return "БУЛСТАТ";
                    default:
                        return "";
                }
            }
        }

        public bool IsPerson
        {
            get
            {
                switch (this.UicTypeId)
                {
                    case NomenclatureConstants.UicTypes.EGN:
                    case NomenclatureConstants.UicTypes.LNCh:
                    case NomenclatureConstants.UicTypes.BirthDate:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public string FullName_Initials
        {
            get
            {
                var firstName = (!string.IsNullOrEmpty(FirstName) ? FirstName.ToShortNameCyrlillic() + "." + (!string.IsNullOrEmpty(MiddleName + FamilyName + Family2Name) ? " " : string.Empty) : "");
                var middleName = (!string.IsNullOrEmpty(MiddleName) ? MiddleName.ToShortNameCyrlillic() + "." + (!string.IsNullOrEmpty(FamilyName + Family2Name) ? " " : string.Empty) : "");
                var familyName = (!string.IsNullOrEmpty(FamilyName) ? FamilyName.ToShortNameCyrlillic() + "." + (!string.IsNullOrEmpty(Family2Name) ? " " : string.Empty) : "");
                var family2Name = (!string.IsNullOrEmpty(Family2Name) ? Family2Name.ToShortNameCyrlillic() + "." : "");
                return firstName + middleName + familyName + family2Name;
            }
        }

        public string FullName_MiddleNameInitials
        {
            get
            {
                var firstName = (!string.IsNullOrEmpty(FirstName) ? FirstName + (!string.IsNullOrEmpty(MiddleName + FamilyName + Family2Name) ? " " : string.Empty) : "");
                var middleName = (!string.IsNullOrEmpty(MiddleName) ? MiddleName.ToShortNameCyrlillic() + "." + (!string.IsNullOrEmpty(FamilyName + Family2Name) ? " " : string.Empty) : "");
                var familyName = (!string.IsNullOrEmpty(FamilyName) ? FamilyName + (!string.IsNullOrEmpty(Family2Name) ? " " : string.Empty) : "");
                var family2Name = (!string.IsNullOrEmpty(Family2Name) ? Family2Name : "");
                return firstName + middleName + familyName + family2Name;
            }
        }

        public string FirstNameInitial_Family
        {
            get
            {
                var firstName = (!string.IsNullOrEmpty(FirstName) ? FirstName.ToShortNameCyrlillic() + "." + (!string.IsNullOrEmpty(FamilyName) ? " " : string.Empty) : "");
                var familyName = (!string.IsNullOrEmpty(FamilyName) ? FamilyName : "");
                return firstName + familyName;
            }
        }

        public string FirstNameFamilyInitial
        {
            get
            {
                var firstName = !string.IsNullOrEmpty(FirstName) ? FirstName.ToShortNameCyrlillic() + "." + (!string.IsNullOrEmpty(FamilyName) ? " " : string.Empty) : "";
                var familyName = !string.IsNullOrEmpty(FamilyName) ? FamilyName.ToShortNameCyrlillic() + "." : "";
                return firstName + familyName;
            }
        }

        public void SplitPersonNames(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            //Махат се двойните интервали в името и тирета за втора фамилия
            value = value.Replace("-", "").Replace("  ", "").Replace("  ", "");
            var names = value.Split(' ');
            if (names.Length > 0)
            {
                this.FirstName = names[0];

                if (names.Length > 1)
                {
                    this.MiddleName = names[1];

                    if (names.Length > 2)
                    {
                        this.FamilyName = names[2];
                        if (names.Length > 3)
                        {
                            this.Family2Name = names[3];
                        }
                    }
                    else
                    {
                        //ако са само две имена стават собствено име и фамилия.
                        this.FamilyName = this.MiddleName;
                        this.MiddleName = string.Empty;
                    }
                }
            }
        }
    }
}
