using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseLoadCorrectionService: IBaseService
    {
        IQueryable<CaseLoadCorrectionActivityVM> CaseLoadCorrectionActivity_Select();
        CaseLoadCorrectionActivity GetMaxId();
        bool CaseLoadCorrectionActivity_SaveData(CaseLoadCorrectionActivity model);
        bool IsExistCaseLoadCorrection(int ModelId, int CaseId, int CaseLoadCorrectionActivityId);
        List<SelectListItem> GetDDL_CaseLoadCorrectionActivity(int CaseGroupId, int CaseInstanceId, bool addDefaultElement = true, bool addAllElement = false);
        IQueryable<CaseLoadCorrectionActivityIndexVM> CaseLoadCorrectionActivityIndex_Select(int CaseLoadCorrectionActivityId);
        bool CaseLoadCorrectionActivityIndex_SaveData(CaseLoadCorrectionActivityIndex model);
        IQueryable<CaseLoadCorrectionVM> CaseLoadCorrection_Select(int CaseId);
        bool CaseLoadCorrection_SaveData(CaseLoadCorrection model);
        decimal GetCaseLoadCorrection(int CaseId);
        decimal GetCaseLoadCorrectionToDate(int CaseId, DateTime dateTime);
    }
}
