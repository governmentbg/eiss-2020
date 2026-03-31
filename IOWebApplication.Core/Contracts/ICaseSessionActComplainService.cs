using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseSessionActComplainService : IBaseService
    {
        #region CaseSessionActComplain
        IQueryable<CaseSessionActComplainVM> CaseSessionActComplain_Select(int CaseSessionActId);

        /// <summary>
        /// Извличане на данни за справка за обжалване
        /// </summary>
        /// <param name="filter">Филтър</param>
        /// <returns></returns>
        IQueryable<CaseSessionActComplainSprVM> CaseSessionActComplainSpr_Select(CaseSessionActComplainFilterVM filter);

        /// <summary>
        /// Проверка по текущото дело, дали има обжалване с този съпровождащ документ
        /// </summary>
        /// <param name="CaseId">ИД на текущо дело</param>
        /// <param name="ComplainDocumentId">ИД на съпровождащ документ</param>
        /// <returns></returns>
        bool IsExistComplain(int CaseId, long ComplainDocumentId);
        Task<bool> CaseSessionActComplain_SaveData(CaseSessionActComplain model);
        bool CaseSessionActComplain_CreateFromDocument(long DocumentId);
        List<SelectListItem> GetDropDownList_GetDocumentCaseInfo(int CaseSessionId, bool addDefaultElement = true, bool addAllElement = false);
        List<CheckListVM> GetCheckListCaseSessionActComplains(int CaseSessionActComplainId, int CaseSessionActId);
        bool IsExistComplainByDocumentIdDifferentStatusRecived(long DocumentId);
        #endregion

        #region CaseSessionActComplainResult
        IQueryable<CaseSessionActComplainResultVM> CaseSessionActComplainResult_Select(int CaseSessionActComplainId);
        bool CaseSessionActComplainResult_SaveData(CaseSessionActComplainResultEditVM model);
        List<SelectListItem> GetDropDownList_CaseSessionActFromCaseSessionActComplainResult(int CaseId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDropDownList_ActResultFromCaseSessionActComplainResult(int CaseSessionActId, bool addDefaultElement = true, bool addAllElement = false);
        CaseSessionActComplainResultEditVM CaseSessionActComplainResult_GetById(int Id);
        #endregion

        #region CaseSessionActComplainPerson
        IQueryable<CaseSessionActComplainPersonVM> CaseSessionActComplainPerson_Select(int CaseSessionActComplainId);
        bool CaseSessionActComplainPerson_SaveData(CheckListViewVM model);
        CheckListViewVM CheckListViewVM_FillCasePerson(int CaseSessionActComplainId);
        List<SelectListItem> GetDropDownListForAct(int caseId, int[] caseSessionActIds, int htmlTemplateId, bool addDefaultElement = true, bool addAllElement = false);
        #endregion
    }
}
