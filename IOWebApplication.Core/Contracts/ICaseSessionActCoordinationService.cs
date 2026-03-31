using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseSessionActCoordinationService : IBaseService
    {
        IQueryable<CaseSessionActCoordinationVM> CaseSessionActCoordination_Select(int CaseSessionActId, int? CaseSessionActCoordinationId = null, int coordinationType = NomenclatureConstants.CoordinationTypes.Act);
        bool CaseSessionActCoordination_SaveData(CaseSessionActCoordination model);
        Task<IEnumerable<CoordinationDepersonalizeVM>> GetDepersonalizationInfo(int actId);
        bool RemoveDepersonalizationInfo(int id);
    }
}
