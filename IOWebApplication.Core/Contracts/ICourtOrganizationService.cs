using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICourtOrganizationService: IBaseService
    {
        IQueryable<CourtOrganizationVM> CourtOrganization_Select(int courtId);
        bool CourtOrganization_SaveData(CourtOrganizationEditVM model);
        List<SelectListItem> GetDropDownList(int courtId, int id = 0, bool addDefaultElement = true, bool addAllElement = false);

        /// <summary>
        /// Извличане на данни за организационна структура на съд
        /// </summary>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <returns></returns>
        List<SelectListItem> CourtOrganization_SelectForDropDownList(int courtId);

        /// <summary>
        /// Извличане на данни за организационна структура на съд
        /// </summary>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <returns></returns>
        Task<List<SelectListItem>> CourtOrganization_SelectForDropDownListAsync(int courtId);

        CourtOrganizationEditVM CourtOrganization_GetById(int id);
        List<CheckListVM> FillCheckListCourtOrganizationCaseGroups();
    }
}
