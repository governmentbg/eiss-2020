using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Money
{
    //Изпълнителни листове
    [Table("money_exec_list")]
    public class ExecList : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("exec_list_type_id")]
        public int ExecListTypeId { get; set; }

        [Column("reg_number")]
        public string RegNumber { get; set; }

        [Column("reg_date")]
        public DateTime? RegDate { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("out_document_id")]
        [Display(Name = "Документ")]
        public long? OutDocumentId { get; set; }

        /// <summary>
        /// изпълнително дело, образувано от ЧСИ, ДСИ - не е обект на ЕИСС
        /// </summary>
        [Column("case_number")]
        public string CaseNumber { get; set; }

        /// <summary>
        /// Дата на връчване на ИЛ в съда
        /// </summary>
        [Column("delivery_date")]
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        /// Лице на което е връчен ИЛ - засега го оставяме да си го въвеждат на ръка. За целите на книгата е.
        /// </summary>
        [Column("delivery_person_name")]
        public string DeliveryPersonName { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        /// <summary>
        /// Основание
        /// </summary>
        [Column("exec_list_law_base_id")]
        public int? ExecListLawBaseId { get; set; }

        [Column("lawunit_sign_id")]
        [Display(Name = "Съдия")]
        public int? LawUnitSignId { get; set; }

        [Column("exec_list_state_id")]
        [Display(Name = "Статус")]
        public int? ExecListStateId { get; set; }


        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(ExecListTypeId))]
        public virtual ExecListType ExecListType { get; set; }

        [ForeignKey(nameof(OutDocumentId))]
        public virtual Document OutDocument { get; set; }

        [ForeignKey(nameof(ExecListLawBaseId))]
        public virtual ExecListLawBase ExecListLawBase { get; set; }

        [ForeignKey(nameof(LawUnitSignId))]
        public virtual LawUnit LawUnitSign { get; set; }

        [ForeignKey(nameof(ExecListStateId))]
        public virtual ExecListState ExecListState { get; set; }

        public virtual ICollection<ExecListObligation> ExecListObligations { get; set; }

        public virtual ICollection<ExchangeDocExecList> ExchangeDocExecLists { get; set; }

        public ExecList()
        {
            ExecListObligations = new HashSet<ExecListObligation>();
            ExchangeDocExecLists = new HashSet<ExchangeDocExecList>();
        }
    }
}
