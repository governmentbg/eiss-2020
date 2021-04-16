using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICourtOrganizationService: IBaseService
    {
        IQueryable<CourtOrganizationVM> CourtOrganization_Select(int courtId);
        bool CourtOrganization_SaveData(CourtOrganizationEditVM model);
        List<SelectListItem> GetDropDownList(int courtId, int id = 0, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> CourtOrganization_SelectForDropDownList(int courtId);
        CourtOrganizationEditVM CourtOrganization_GetById(int id);
        List<CheckListVM> FillCheckListCourtOrganizationCaseGroups();
    }
}
