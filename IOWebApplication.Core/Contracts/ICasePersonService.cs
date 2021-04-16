using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICasePersonService : IBaseService
    {
        IQueryable<CasePersonListVM> CasePerson_Select(int caseId, int? caseSessionId, bool checkSessionDate, bool showExpired, bool setRowNumberFromCase);

        (bool result, string errorMessage) CasePerson_SaveData(CasePersonVM model);

        CasePersonVM CasePerson_GetById(int id);

        IQueryable<CasePersonAddressListVM> CasePersonAddress_Select(int casePersonId);

        (bool result, string errorMessage) CasePersonAddress_SaveData(CasePersonAddress model);

        CasePersonAddress CasePersonAddress_GetById(int id);

        /// <summary>
        /// Взима всички заседания и делото за комбо и от това комбо ще се зареждат страните за избраното дело/заседание
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        List<SelectListItem> GetDDL_Case_CaseSession_ForPersonCopy(int caseId, int caseSessionId);

        /// <summary>
        /// взима страните за да може да се копират от дело/заседание в друго заседание
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        CheckListViewVM CasePerson_SelectForCheck(int caseId, int caseSessionId, int realCaseSessionId);
        CheckListViewVM CasePersonPrint_SelectForCheck(int caseId);

        CheckListViewVM CasePersonNotification_SelectForCheck(int caseId, int caseSessionId);

        bool CasePerson_CopyCasePerson(string ids, int caseId, int caseNewSessionId);

        List<SelectListItem> CasePerson_SelectForDropDownList(int caseId, int? caseSessionId, string roleKindIds = "", string defaultElementText = "");

        List<SelectListItem> GetDropDownList(int caseId, int? caseSessionId, bool addLinkName3, int? notificationTypeId, int? casePersonId, bool filterPersonOnNotification, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDropDownList_RightSide(int caseId, int? caseSessionId, bool addDefaultElement = true, bool addAllElement = false);

        bool CasePerson_SaveNotification(CheckListViewVM checkListViewVM);

        /// <summary>
        /// Четене за Справка лица
        /// </summary>
        /// <param name="uic"></param>
        /// <param name="fullName"></param>
        /// <returns></returns>
        IQueryable<CasePersonReportVM> CasePerson_SelectForReport(int courtId, string uic, string fullName, string caseRegnumber, DateTime? DateFrom, DateTime? DateTo, DateTime? FinalDateFrom, DateTime? FinalDateTo, DateTime? WithoutFinalDateTo);

        List<SelectListItem> GetDDL_CasePersonAddress(int casePersonId, int notificationDeliveryGroupId);
        List<SelectListItem> GetDDL_AddressByCasePersonAddress(int casePersonId);
        List<CasePersonAddress> Get_CasePersonAddress(int casePersonId);

        void SetCasePersonDataForCopySession(int caseId, int? caseOldSession, int caseNewSessionId, List<CasePerson> casePersonList, DateTime caseNewSessionDateFrom);
        (bool result, string errorMessage) ReloadPersonData(int caseId, int caseSessionId);

        IQueryable<CaseSessionNotificationListVM> PersonListForPrint_Select(CheckListViewVM model);
        (List<SelectListItem> person_ddl, List<SelectListItem> linkDirection_ddl) CasePersonForLinkRel_SelectForDropDownList(int casePersonId);
        (List<SelectListItem> men, List<SelectListItem> women, List<PersonDataVM> personData) GetCasePersonForDivorce(int actId);
        (bool result, string errorMessage) CasePersonAddress_AddFromSearch(int casePersonId, int addressId);

        IQueryable<CasePersonInheritanceVM> CasePersonInheritance_Select(int CasePersonId);
        bool CasePersonInheritance_SaveData(CasePersonInheritance model);

        IQueryable<CasePersonMeasureVM> CasePersonMeasure_Select(int CasePersonId, bool showExpired = false);
        bool CasePersonMeasure_SaveData(CasePersonMeasureEditVM model);
        IQueryable<CasePersonDocumentVM> CasePersonDocument_Select(int CasePersonId, bool showExpired = false);
        bool CasePersonDocument_SaveData(CasePersonDocument model);
        CasePersonMeasureEditVM CasePersonMeasure_GetById(int id);
        List<SelectListItem> GetForEispp(int caseId);
        (bool result, string errorMessage) CheckCasePersonExpired(CasePerson model);
        bool IsPersonDead(int casePersonId);
        bool CasePerson_SaveExpiredPlus(ExpiredInfoVM model);


        List<SelectListItem> GetAddressByCasePerson_DropDown(int casePersonId);

        SaveResultVM CasePersonAddress_IsUsed(CasePersonAddress model);
    }
}
