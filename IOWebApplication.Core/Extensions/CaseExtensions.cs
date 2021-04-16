using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Core.Extensions
{
    public static class CaseExtensions
    {
        #region Case

        /// <summary>
        /// Връща инфо на делото Тип дело/номер/дата
        /// </summary>
        /// <param name="model">Обект Case с изчетен CaseType</param>
        /// <returns></returns>
        public static string GetCaseName(Case model)
        {
            return ((model.CaseType != null) ? model.CaseType.Label + " " : string.Empty) + model.RegNumber + "/" + model.RegDate.ToString("dd.MM.yyyy");
        }

        /// <summary>
        /// Връща инфо на делото Дело/номер
        /// </summary>
        /// <param name="model">Обект Case с изчетен CaseType</param>
        /// <returns></returns>
        public static string GetCaseNameBreadcrumbs(Case model)
        {
            return "Дело " + model.RegNumber;
        }

        #endregion

        #region CaseSession

        /// <summary>
        /// Връща инфо на заседание тип заседание/дата от
        /// </summary>
        /// <param name="model">Обект CaseSession с изчетен SessionType</param>
        /// <returns></returns>
        public static string GetCaseSessionNameBreadcrumbs(CaseSession model)
        {
            return ((model.SessionType != null) ? model.SessionType.Label + " " : string.Empty) + model.DateFrom.ToString("dd.MM.yyyy");
        }

        #endregion

        #region CaseSessionAct

        /// <summary>
        /// Връща инфо на акт тип/номер/дата
        /// </summary>
        /// <param name="model">Обект CaseSessionAct с изчетен ActType</param>
        /// <returns></returns>
        public static string GetCaseSessionActNameBreadcrumbs(CaseSessionAct model)
        {
            return ((model.ActType != null) ? model.ActType.Label + " " : string.Empty) + ((!string.IsNullOrEmpty(model.RegNumber)) ? model.RegNumber + "/" + (model.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty);
        }

        #endregion
    }
}
