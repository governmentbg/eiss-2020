using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Data.Models.Cases;
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
    public interface ICasePersonService : IBaseService
    {
        IQueryable<CasePersonListVM> CasePerson_Select(int caseId, int? caseSessionId, bool checkSessionDate, bool showExpired, bool setRowNumberFromCase);

        /// <summary>
        /// Извличане на данни за лица по дело/заседание в лист
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <param name="checkSessionDate"></param>
        /// <param name="showExpired">Флаг да показва изтритите записи</param>
        /// <param name="setRowNumberFromCase">Ако е за заседание и е true да вземе rownumber от делото за този идентификатор</param>
        /// <param name="start">От коя позиция да дръпне данните</param>
        /// <param name="length">Дължина</param>
        /// <param name="sortedColumns">Колони по които се сортира</param>
        /// <returns></returns>
        Task<DataTableResponseVM<CasePersonListVM>> CasePersonList_Select(int caseId, int? caseSessionId, bool checkSessionDate, bool showExpired, bool setRowNumberFromCase, int start, int length, List<DataTablesSortColumnVM> sortedColumns);

        IQueryable<CasePersonListVM> CasePersonFast_SelectForCasePreview(int caseId, int? caseSessionId = null);

        Task<(bool result, string errorMessage)> CasePerson_SaveData(CasePersonVM model);

        Task<CasePersonVM> CasePerson_GetById(int id);

        IQueryable<CasePersonAddressListVM> CasePersonAddress_Select(int casePersonId);

        Task<(bool result, string errorMessage)> CasePersonAddress_SaveData(CasePersonAddress model);

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
        Task<CheckListViewVM> CasePerson_SelectForCheck(int caseId, int caseSessionId, int realCaseSessionId);
        Task<CheckListViewVM> CasePersonPrint_SelectForCheck(int caseId);

        CheckListViewVM CasePersonNotification_SelectForCheck(int caseId, int caseSessionId);

        bool CasePerson_CopyCasePerson(string ids, int caseId, int caseNewSessionId);

        /// <summary>
        /// Извличане на данни за лица по дело/заседание за чекбокс
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <param name="roleKindIds">Идентификатори на тип</param>
        /// <param name="defaultElementText">Друго име на дефолтната стойност</param>
        /// <param name="dateTo">До дата</param>
        /// <param name="isViewUic">Флаг дали да се вижда идентификатора</param>
        /// <returns></returns>
        List<SelectListItem> CasePerson_SelectForDropDownList(int caseId, int? caseSessionId, string roleKindIds = "", string defaultElementText = "", DateTime? dateTo = null, bool isViewUic = true);

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

        /// <summary>
        /// Извличане на адреси за комбобокс
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDDL_AddressByCasePersonAddressAsync(int casePersonId);

        List<SelectListItem> GetDDL_CasePersonAddress(int casePersonId);
        List<CasePersonAddress> Get_CasePersonAddress(int casePersonId);

        void SetCasePersonDataForCopySession(int caseId, int? caseOldSession, int caseNewSessionId, List<CasePerson> casePersonList, DateTime caseNewSessionDateFrom);
        (bool result, string errorMessage) ReloadPersonData(int caseId, int caseSessionId);

        IQueryable<CaseSessionNotificationListVM> PersonListForPrint_Select(CheckListViewVM model);
        (List<SelectListItem> person_ddl, List<SelectListItem> linkDirection_ddl) CasePersonForLinkRel_SelectForDropDownList(int casePersonId);
        (List<SelectListItem> men, List<SelectListItem> women, List<PersonDataVM> personData) GetCasePersonForDivorce(int actId);
        Task<(bool result, string errorMessage)> CasePersonAddress_AddFromSearch(int casePersonId, int addressId);

        IQueryable<CasePersonInheritanceVM> CasePersonInheritance_Select(int CasePersonId);
        Task<bool> CasePersonInheritance_SaveData(CasePersonInheritance model);

        IQueryable<CasePersonMeasureVM> CasePersonMeasure_Select(int CasePersonId, bool showExpired = false);
        Task<bool> CasePersonMeasure_SaveData(CasePersonMeasureEditVM model);
        IQueryable<CasePersonDocumentVM> CasePersonDocument_Select(int CasePersonId, bool showExpired = false);
        Task<bool> CasePersonDocument_SaveData(CasePersonDocument model);
        Task<CasePersonMeasureEditVM> CasePersonMeasure_GetById(int id);
        List<SelectListItem> GetForEispp(int caseId);
        Task<(bool result, string errorMessage)> CheckCasePersonExpired(CasePerson model);
        bool IsPersonDead(int casePersonId);
        Task<bool> CasePerson_SaveExpiredPlus(ExpiredInfoVM model);


        Task<List<SelectListItem>> GetAddressByCasePerson_DropDown(int casePersonId);

        Task<SaveResultVM> CasePersonAddress_IsUsed(CasePersonAddress model);
        bool IsExistFastProcess(int CaseId, int PersonId);
        Task<SaveResultVM> CasePersonPrevName_SaveData(CasePersonPrevName model);
        IQueryable<CasePersonPrevNameVM> CasePersonPrevName_Select(int casePersonId);
        Task<List<CheckListVM>> CasePersonSentencePunishmentMeasure_GetPunishmentChecks(int casePersonMeasureId, int casePersonId);
    }
}
