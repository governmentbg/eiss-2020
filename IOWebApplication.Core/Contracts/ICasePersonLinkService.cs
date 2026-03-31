using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Epep;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICasePersonLinkService : IBaseService
    {
        IQueryable<CasePersonLinkListVM> CasePersonLink_Select(int caseId);
        bool CasePersonLink_SaveData(CasePersonLink model);
        List<CaseNotificationLinkVM> GetLinkForPerson(int casePersonId, bool filterPersonOnNotification, int notificationListId, List<int> oldLinks);
        List<CaseNotificationLinkVM> GetPresentByList(int casePersonId, bool filterPersonOnNotification, int notificationTypeId, List<int> oldLinks);
        List<SelectListItem> ListForPersonToDropDown(List<CaseNotificationLinkVM> linkList, int casePersonId, bool addDefaultElement = true, bool addMulti = true);
        List<SelectListItem> LinkDirectionForPersonDDL(int casePersonId);
        List<SelectListItem> RelationalPersonDDL(int caseId, int linkDirectionId, string defaultElementText = null);
        List<SelectListItem> SecondLinkDirectionDDL();
        List<SelectListItem> SeccondRelationalPersonDDL(int caseId, string defaultElementText = null);
        bool HaveCaseNotification(int casePersonLinkId);
        List<SelectListItem> PersonYDDL(int caseId, int linkDirectionId, string defaultElementText = null);
        List<SelectListItem> RoleKindDDL();
        List<CasePersonLinkSideItemVM> GetPersonXBySide(int caseId, int roleKindId);
        bool Save_AddSide(CasePersonLinkSideVM model, List<int> personIds);
        bool HaveSameLink(CasePersonLink model);
        Task<List<CaseNotificationLinkVM>> GetLinkForPersonList(int[] casePersonIds, int caseId, int caseSessionId);
        EpepSummonInfoVM GetEpepSummonInfo(CaseNotification model, bool chechAssignment = true);
        List<CaseNotificationLinkVM> FilterLinkOnSession(List<CaseNotificationLinkVM> links, int? caseSessionId, List<int> oldLinks);
        List<CaseNotificationLinkVM> GetLinkForPersonMediation(int casePersonId, int mediationSessionId);
    }
}
