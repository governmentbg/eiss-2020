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
    public interface ICaseSessionActSelectionService : IBaseService
    {
        IQueryable<CaseSessionActSelectionProtocolVM> CaseSessionActSelectionProtol_sel(int courtId, CaseSessionActSelectionProtocolFilterVM model);
        IQueryable<CaseSessionActSelectionActVM> CaseSessionActSelectionAct_sel(int courtId, CaseSessionActSelectionProtocolFilterVM model);
        Task<CaseSessionActSelectionProtocolVM> GetSelectionActProtocolByID(int id);
        Task<int> CreateActSelection(int courtId, CaseSessionActSelectionProtocolFilterVM model);
        Task<int> ActSelectionProtokol_SignUpdate(int id);
    }
 }
