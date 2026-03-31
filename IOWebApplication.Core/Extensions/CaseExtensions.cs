using IOWebApplication.Core.Models.BreadcrumbsModels;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace IOWebApplication.Core.Extensions
{
    public static class CaseExtensions
    {
        #region Case

        public static string GetCaseNameBreadcrumbs(BCCaseModel model)
        {
            return "Дело " + model.CaseTypeCode + " " + ((model.CaseStateId == NomenclatureConstants.CaseState.Rejected) ? "Отказ от образуване" : (model.ShortNumber == null ? "За образуване" : model.ShortNumber + "/" + model.RegDate.ToString("yyyy")));
        }

        #endregion

        #region CaseSession

        
        public static string GetCaseSessionNameBreadcrumbs(BCCaseSessionModel model)
        {
            return $"{model.SessionType} {model.DateFrom:dd.MM.yyyy}";
        }

        #endregion

        #region CaseSessionAct

        /// <summary>
        /// Връща инфо на акт тип/номер/дата
        /// </summary>
        /// <param name="model">Обект CaseSessionAct с изчетен ActType</param>
        /// <returns></returns>
        public static string GetCaseSessionActNameBreadcrumbs(BCCaseSessionActModel model)
        {
            if (string.IsNullOrEmpty(model.RegNumber))
            {
                return $"{model.ActTypeLabel}";
            }
            else
            {
                return $"{model.ActTypeLabel} {model.RegNumber}/{model.RegDate:dd.MM.yyyy}";
            }
            //return ((model.ActType != null) ? model.ActType.Label + " " : string.Empty) + ((!string.IsNullOrEmpty(model.RegNumber)) ? model.RegNumber + "/" + (model.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty);
        }

        #endregion

        /// <summary>
        /// Проверка за валиден отвод - или няма статус или е Уважено искането за отвод
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<CaseLawUnitDismisal, bool>> ConfirmedDismissalsOnly()
        {
            return x => (x.DismissalStateId ?? NomenclatureConstants.DismissalStates.Confirmed) == NomenclatureConstants.DismissalStates.Confirmed;
        }
    }
}
