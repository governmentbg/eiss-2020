using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Брой Заповедни производства по съд
    /// </summary>
    [Table("common_fast_process_selection_court")]
    public class FastProcessSelectionCourt : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        /// <summary>
        /// Съд
        /// </summary>
        [Column("court_id")]
        public int CourtId { get; set; }
        /// <summary>
        /// Година за избор
        /// </summary>
        [Column("year_sel")]
        public int YearSel { get; set; }
        /// <summary>
        /// Месец за избор
        /// </summary>
        [Column("month_sel")]
        public int MonthSel { get; set; }
        /// <summary>
        /// Ден за избор
        /// </summary>
        [Column("day_sel")]
        public int DaySel { get; set; }

        [Column("selection_date")]
        public DateTime SelectionDate { get; set; }
        /// <summary>
        /// Първоначални целеви бройки за изравняване (променят се с всяко разпределено дело и промяна на средната натовареност)
        ///Начален таргет
        /// </summary>
        [Column("start_target")]
        public int StartTarget { get; set; }
        /// <summary>
        /// Разлика в целева стойност за изравняване (след изместване на средната стойност при всяко разпределение -
        /// след като всички по-големи от 0 stanat 0)
        /// </summary>
        [Column("added_after_zero")]
        public int AddedAfterZero { get; set; }
        /// <summary>
        /// Разпределени до текущият момент в месеца
        /// </summary>
        [Column("selected_to_now")]
        public int SelectedToNow { get; set; }
        /// <summary>
        /// Разпределени за деня
        /// </summary>
        [Column("selected_for_day")]
        public int SelectefForDay { get; set; }
        /// <summary>
        /// Оставащи за разпределяне до нулиране на таргета
        /// </summary>
        [Column("left_for_selection")]
        public int LeftForSelection { get; set; }
        /// <summary>
        /// Брой съдии
        /// </summary>
        [Column("judge_count")]
        public int? JudgeCount { get; set; }
        /// <summary>
        /// симулация (в случай че се прави повече от една за месеца - по подразбиране -0)
        /// </summary>
        [Column("simulation")]
        [MaxLength(20)]
        public string? Simulation { get; set; }
        /// <summary>
        /// Базов коефициент (Натовареност на СЪД)
        /// </summary>
        [Column("bazov_koef_court")]
        public decimal? BazovKoefCourt { get; set; }

        /// <summary>
        /// Базов коефициент (Натовареност на съдия в СЪД)
        /// </summary>
        [Column("bazov_koef_court_judge")]
        public decimal? BazovKoefCourtJudge { get; set; }
        /// <summary>
        /// Среден базов коефициент- на съдия (обща средна натовареност на районните съдилища) (съгласно чл.9, т. 1 от Правилата)
        /// </summary>
        [Column("bazov_koef_sreden_all_court_judge")]
        public decimal? BazovKoefSredenALLCourtJudge { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }



    }
}
