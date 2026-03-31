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
    public interface ICaseLawyerHelpService: IBaseService
    {
        IQueryable<CaseLawyerHelpVM> CaseLawyerHelp_Select(int CaseId);
        IQueryable<CaseLawyerHelpPersonVM> CaseLawyerHelpPerson_Select(int CaseLawyerHelpId);
        CaseLawyerHelpEditVM CaseLawyerHelp_GetById(int Id);
        CaseLawyerHelpPersonVM CaseLawyerHelpPerson_GetById(int Id);
        List<CheckListVM> FillCaseLawyerHelpOtherLawyers(int? CaseLawyerHelpId, int CaseId);
        Task<bool> CaseLawyerHelp_SaveData(CaseLawyerHelpEditVM model);
        bool CaseLawyerHelpPerson_SaveData(CaseLawyerHelpPerson model);
        List<CheckListVM> FillLeftRightSide(int CaseId);
        List<SelectListItem> GetDDL_LeftRightSide(int CaseLawyerHelpId, int? CasePersonId, bool addDefaultElement = true);
        List<SelectListItem> GetDDL_Lawyer(int CaseLawyerHelpId, bool addDefaultElement = true);
        List<SelectListItem> GetDDL_LawyerHelpBase(int CaseId, bool addDefaultElement = true);
        CaseLawyerHelpPersonMultiEditVM CaseLawyerHelpPersonMultiEdit_Get(int CaseLawyerHelpId);
        bool CaseLawyerHelpPersonMulti_UpdateData(CaseLawyerHelpPersonMultiEditVM model);
        bool IsExistPerson_CaseLawyerHelp(int Id);
        bool IsExistDocumentTemplate_CaseLawyerHelp(int Id);
        IQueryable<CaseLawyerHelpAssignedLawyerVM> CaseLawyerHelpAssignedLawyer_Select(int CaseLawyerHelpId);
        Task<bool> CaseLawyerHelpAssignedLawyer_ChangeState(int CaseId, int Id, int? LawyerStateId, DateTime? dateTime);
        CaseLawyerHelpAssignedLawyerVM CaseLawyerHelpAssignedLawyerVM_GetById(int Id);
        CaseLawyerHelpAssignedLawyerEditVM CaseLawyerHelpAssignedLawyerEditVM_GetById(int Id);
        bool CaseLawyerHelpAssignedLawyer_SaveData(CaseLawyerHelpAssignedLawyerEditVM model);
    }
}
