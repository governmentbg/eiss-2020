using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseClassificationService : IBaseService
    {
        IList<CheckListVM> FillCheckListVMs(int caseId, int? caseSessionId);
        CheckListViewVM CaseClassification_SelectForCheck(int caseId, int? caseSessionId);

        bool CaseClassification_SaveData(CheckListViewVM model);

        /// <summary>
        /// Извличане на данни за индикатори към дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        Task<List<SelectListItem>> CaseClassification_Select(int caseId, int? caseSessionId);

        Task<List<CaseClassification>> CaseClassification_SelectObject(int caseId, int? caseSessionId);
        List<CaseClassification> CaseClassification_SelectRow(int caseId, int? caseSessionId);
    }
}
