using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    /// <summary>
    /// Модел за отпечатване на актове
    /// </summary>
    public class CaseSessionActCommandVM
    {
        public virtual CaseSessionActPrintVM CaseSessionActPrint { get; set; }
        public virtual CaseFastProcessViewVM CaseFastProcessView { get; set; }
        public virtual FastProcessRequestVM FastProcessRequest { get; set; } = new();
        public List<NomenclatureItemVM> Nomenclatures { get; set; }

        /// <summary>
        /// Флаг дали сме в евро зоната
        /// </summary>
        public bool IsInEuro { get; set; }

        /// <summary>
        /// Метод връщащ лейбъл на номенклатура
        /// </summary>
        /// <param name="alias">Тип</param>
        /// <param name="value">Код</param>
        /// <returns></returns>
        public string getNomenclature(string alias, string value)
        {
            return Nomenclatures.Where(n => n.Alias == alias && n.Value == value).Select(n => n.Label).FirstOrDefault() ?? "";
        }

        /// <summary>
        /// Форматиране на дата
        /// </summary>
        /// <param name="value">Стойност</param>
        /// <returns></returns>
        public string formatDate(DateTime? value)
        {
            if (value == null)
            {
                return "";
            }
            return value.Value.ToString("dd.MM.yyyy г.");
        }

        /// <summary>
        /// Солидарно разпределение
        /// </summary>
        public bool JointDistribution
        {
            get
            {
                if (FastProcessRequest == null)
                {
                    return false;
                }
                return !((FastProcessRequest.JoinedDestributionTypeId ?? 0) == 2);
            }
        }

        /// <summary>
        /// Суми по хора, ако не е солидарно разпределено
        /// </summary>
        public string[] SumIsNotJointDistribution { get; set; }

        /// <summary>
        /// Суми, ако е солидарно разпределено
        /// </summary>
        public string SumJointDistribution { get; set; }

        /// <summary>
        /// Суми, ако е солидарно разпределено без точка накрая
        /// </summary>
        public string SumJointDistributionWithoutPoint { get; set; }

        public int ActTypeId { get; set; }
        public bool GenerateExecProcess { get; set; }

        /// <summary>
        /// Разноски
        /// </summary>
        public string FastProcessRequestExpenses { get; set; }
    }
}
