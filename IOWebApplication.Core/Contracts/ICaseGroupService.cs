using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseGroupService : IBaseService
    {
        IQueryable<CaseGroupVM> CaseGroup_Select();

        bool CaseGroup_SaveData(CaseGroup model);

        IQueryable<CaseTypeVM> CaseType_Select(int caseGroupId);

        bool CaseType_SaveData(CaseType model);

        IQueryable<CaseCodeVM> CaseCode_Select(int caseTypeId);

        bool CaseCode_SaveData(CaseCode model, List<int> types);

        /// <summary>
        /// CaseType multiselect за CaseCode
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IQueryable<MultiSelectTransferVM> CaseTypeForSelect_Select(int caseCodeId);

        IQueryable<CaseTypeUnitVM> CaseTypeUnit_Select(int caseTypeId);
        CaseTypeUnitEditVM GetById_CaseTypeUnit(int id);
        ICollection<ListNumberVM> GetList_CaseTypeUnitCounts();
        bool CaseTypeUnit_SaveData(CaseTypeUnitEditVM model);
    }
}
