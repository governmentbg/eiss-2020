using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Правораздаващи лица
    /// </summary>
    [Table("common_law_unit")]
    public class LawUnit : BaseInfo_LawUnit, IHaveHistory<LawUnitH>
    {
        public ICollection<CourtLawUnit> Courts { get; set; }
        public ICollection<LawUnitSpeciality> LawUnitSpeciality { get; set; }
        public ICollection<LawUnitH> History { get; set; }
    }

    /// <summary>
    /// Правораздаващи лица - история
    /// </summary>
    [Table("common_law_unit_h")]
    public class LawUnitH : BaseInfo_LawUnit, IHistory
    {
        [Column("history_id")]
        public int HistoryId { get; set; }

        [Column("history_date_expire")]
        public DateTime? HistoryDateExpire { get; set; }

        [ForeignKey(nameof(Id))]
        public virtual LawUnit LawUnit { get; set; }
    }

    public class BaseInfo_LawUnit : PersonNamesBase, IUserDateWRT
    {
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// 1-съдия,2-заседател,3-прокурор,4-вещи лица
        /// </summary>
        [Column("law_unit_type_id")]
        public int LawUnitTypeId { get; set; }

        [Column("judge_seniority_id")]
        public int? JudgeSeniorityId { get; set; }

        [Column("code")]
        [Display(Name = "Код")]
        public string Code { get; set; }

        [Column("department")]
        [Display(Name = "Организация/Отдел/Колегия")]
        public string Department { get; set; }

        [Column("date_from")]
        [Display(Name = "Дата от")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "Дата до")]
        public DateTime? DateTo { get; set; }

        [ForeignKey(nameof(LawUnitTypeId))]
        public virtual LawUnitType LawUnitType { get; set; }

        [ForeignKey(nameof(JudgeSeniorityId))]
        public virtual JudgeSeniority JudgeSeniority { get; set; }
        public string UserId { get; set; }
        public DateTime DateWrt { get; set; }
        [Column("date_transfered_dw")]
        public DateTime? DateTransferedDW { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }

        public BaseInfo_LawUnit()
        {
            Person = new Person()
            {
                PersonTypeId = NomenclatureConstants.PersonTypes.Person,
                UicTypeId = NomenclatureConstants.UicTypes.EGN
            };
        }
    }
}
