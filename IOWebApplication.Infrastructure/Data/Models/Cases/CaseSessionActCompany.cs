using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Данни за регистрация на фирма
    /// </summary>
    [Table("case_session_act_company")]
    public class CaseSessionActCompany : UserDateWRT
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_session_act_id")]
        public int CaseSessionActId { get; set; }

        [Column("register_date")]
        [Display(Name = "Вписване в ТРРЮЛНЦ")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime RegisterDate { get; set; }

        [Column("register_number")]
        [Display(Name = "Регистър")]
        [Range(1, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public int RegisterNumber { get; set; }

        [Column("chapter")]
        [Display(Name = "Том")]
        [Range(1, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public int Chapter { get; set; }

        [Column("page_number")]
        [Display(Name = "Страница")]
        [Range(1, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public int PageNumber { get; set; }

        [Column("batch")]
        [Display(Name = "Партида")]
        [Range(1, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public int Batch { get; set; }

        [Column("level")]
        [Display(Name = "Ниво")]
        [Range(1, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public int Level { get; set; }

        [Column("authorization")]
        [Display(Name = "Одобрение на дружествен договор/устав")]
        public bool Authorization { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }
    }
}
