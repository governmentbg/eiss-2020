using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    /// <summary>
    /// Модел за справка натовареност по дела: основни и допълнителни дейности
    /// </summary>
    public class CaseLoadIndexSprVM
    {
        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Номер дело
        /// </summary>
        public string CaseName { get; set; }

        /// <summary>
        /// Тип и шифър на дело
        /// </summary>
        public string CaseTypeCodeLabel { get; set; }

        /// <summary>
        /// Дата на регистрация на дело
        /// </summary>
        public DateTime CaseRegDate { get; set; }

        /// <summary>
        /// Съдия докладчик
        /// </summary>
        public string JudgeReport { get; set; }

        /// <summary>
        /// Идентификатор на служител
        /// </summary>
        public int LawUnitId { get; set; }

        /// <summary>
        /// Име на служител
        /// </summary>
        public string LawUnitName { get; set; }

        /// <summary>
        /// Натоварване
        /// </summary>
        public decimal CalcValue { get; set; }

        /// <summary>
        /// Базов коефициент
        /// </summary>
        public decimal BaseIndexCase { get; set; }

        /// <summary>
        /// Индекс осн. дейност
        /// </summary>
        public decimal BaseIndexMain { get; set; }

        /// <summary>
        /// Индекс доп. дейност
        /// </summary>
        public decimal BaseIndexNotMain { get; set; }

        /// <summary>
        /// Коригиращи коефициенти по делото
        /// </summary>
        public decimal CorrectionLoadIndex { get; set; }

        /// <summary>
        /// Име на съд
        /// </summary>
        public string CourtLabel { get; set; }

        /// <summary>
        /// Идентификатор на съд
        /// </summary>
        public int? CourtId { get; set; }
    }
}
