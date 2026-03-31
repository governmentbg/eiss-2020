using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseSelectionProtocolSubstitutionService : IBaseService
    {
        IQueryable<CaseSelectionProtocolSubstitutionVM> CaseSelectionProtolSubstitution_sel(int courtId, CaseSelectionProtocolSubstitutionFilterVM model);

        /// <summary>
        /// Метод извличащ данни за избрани протоколи за случаен избор на заместващ в дело/заседание
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        IQueryable<CaseSelectionSubstitutionListDataVM> GetCaseSelectionSubstitution(int caseId);

        bool SubstitutionSelectionProtokol_SaveData(CaseSelectionProtokolVM model, ref string errorMessage);
        Task<CaseSelectionProtocolSubstitutionVM> GetSelectionProtocolSubstitutionByID(int id);
        Task<int> SelectionProtokolSubstitution_SignUpdate(int id);


    }
 }
