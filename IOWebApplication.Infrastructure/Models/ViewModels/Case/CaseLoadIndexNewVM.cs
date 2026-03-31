using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseLoadIndexNewVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int? CaseSessionId { get; set; }
        public int LawUnitId { get; set; }

        /// <summary>
        /// Име
        /// </summary>
        public string LawUnitName { get; set; }

        /// <summary>
        /// Дата на начисляване
        /// </summary>
        public DateTime DateActivity { get; set; }

        /// <summary>
        /// Вид дейност
        /// </summary>
        public string NameActivityType { get; set; }

        /// <summary>
        /// Дейност
        /// </summary>
        public string NameActivity { get; set; }

        /// <summary>
        /// Първоначален коефициент за тежест на делото (1) - Базов индекс
        /// </summary>
        public string CaseLoadIndexBegin { get; set; }

        /// <summary>
        /// Коригиращ (увеличаващ) коефициент (2) - Увеличаващ индекс
        /// </summary>
        public string CaseLoadCorrectionIdex { get; set; }

        /// <summary>
        /// Текущ коефициент  (3)=(1)х(2) - Общ индекс
        /// </summary>
        public string BaseIndex { get; set; }

        /// <summary>
        /// Стойност на процесуалния етап/допълнителната дейност (4) - Стойност
        /// </summary>
        public string LoadValue { get; set; }

        /// <summary>
        /// Изчислена натовареност за процесуалния етап/допълнителна дейност (5)=(3)х(4) - Изчислено
        /// </summary>
        public decimal CalcValue { get; set; }
        public string CalcValueText { get; set; }

        /// <summary>
        /// Обща натовареност на съдия по делото (с натрупване)
        /// </summary>
        public decimal CalcValueLawUnit { get; set; }
        public string CalcValueLawUnitText { get; set; }

        /// <summary>
        /// Обща натовареност по делото (с натрупване)
        /// </summary>
        public decimal CalcValueCase { get; set; }
        public string CalcValueCaseText { get; set; }
        public int Order { get; set; }
    }
}
