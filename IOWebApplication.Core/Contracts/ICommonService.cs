using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.Collections.Generic;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels;
using System.Linq;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Account;
using Microsoft.AspNetCore.Mvc.Rendering;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels.Identity;
using System.Threading.Tasks;
using IOWebApplication.Infrastructure.Constants;

namespace IOWebApplication.Core.Contracts
{
    public interface ICommonService : IBaseService
    {
        IQueryable<InstitutionVM> Institution_Select(int institutionType, string name, int? id = null, string institutionTypeIds = null);
        bool Institution_SaveData(Institution model);
        string Institution_Validate(Institution model);
        string LawUnit_Validate(LawUnit model);
        IQueryable<LawUnitVM> LawUnit_Select(int lawUnitType, string name, DateTime? fromDate, DateTime? toDate, int specialityId,bool showFree);
        IQueryable<LawUnitVM> LawUnitForDate_Select(int lawUnitType, DateTime? date);
        List<SelectListItem> LawUnitForDate_SelectDDL(int lawUnitType, DateTime? date);
        bool LawUnit_SaveData(LawUnit model);
        bool IsExistLawUnit_ByUicUicType(string uic, int? id = null);
        Person Person_FindByUic(string uic, int uicType);

        /// <summary>
        /// Четене по име и/или uic
        /// </summary>
        /// <param name="lawUnitType"></param>
        /// <param name="name"></param>
        /// <param name="uic"></param>
        /// <returns></returns>
        IEnumerable<LabelValueVM> GetLawUnitName_Uic(int lawUnitType, string name, string uic, int courtId, string lawUnitTypes = null);

        /// <summary>
        /// Информация за autocomplete контрола за lawUnit
        /// </summary>
        /// <param name="query">Част от име на обект</param>
        /// Ако има подаден съд, то се взимат всички хора в този съд към текущата дата
        /// <returns></returns>
        IEnumerable<LabelValueVM> GetLawUnitAutoComplete(int lawUnitType, string lawUnitTypes, string query, int courtId, string selectmode = NomenclatureConstants.LawUnitSelectMode.All);

        /// <summary>
        /// Информация за autocomplete контрола за lawUnit
        /// </summary>
        /// <param name="id">Id на lawunit</param>
        /// <returns></returns>
        LabelValueVM GetLawUnitById(int id);
       
        /// <summary>
        /// Всички назначени/командировани съдии в съд
        /// </summary>
        /// <param name="court"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        IQueryable<LawUnit> LawUnit_JudgeByCourtDate(int court, DateTime? date);

        IQueryable<UserProfileVM> Users_Select(int courtId, string fullName, string userId, bool forList = false);

        string Users_GetByLawUnitUIC(string uic);
        string Users_GetUserIdByLawunit(int lawUnitId);
        bool Users_CheckUserByLawUnit(string userId, int lawUnitId);
        bool Users_CheckUserByEmail(string userId, string emailAddress);

        string Users_ValidateEmailLawUnit(string email, int lawUnitId);

        Task<bool> Users_UpdateSetting(string setting, string value);
        bool Users_UpdateSetting(UserSettingsModel model);
        bool Users_GenerateEissId(string userId);

        #region "WorkingDays"


        /// <summary>
        /// Връща
        /// </summary>
        /// <param name="dateFrom">От дата as Datetime</param>
        /// <param name="dateTo">До дата as Datetime</param>
        /// <param name="dayType">Вид работен ден</param>
        /// <returns></returns>
        IQueryable<WorkingDaysVM> WorkingDay_GetList(DateTime? dateFrom, DateTime? dateTo, int dayType = 0);



        /// <summary>
        /// Записва/Променя данните за работен ден
        /// </summary>
        /// <param name="model">Модел as WorkingDays</param>
        /// <returns> >0 - Успешен запис/промяна; <=0 - Неуспешен запис/редакция</returns>
        int WorkingDay_SaveData(WorkingDay model);



        /// <summary>
        /// Изтрива работен ден
        /// </summary>
        /// <param name="Id">Идентификатор as int</param>
        /// <returns>True - Успешно изтриване; False - НЕУСПЕШНО изтриване</returns>
        bool WorkingDay_Delete(int Id);


        /// <summary>
        /// Проверка за съществуващ запис за Работен ден
        /// </summary>
        /// <param name="Day">Дата as DateTime</param>
        /// <param name="Id">ID на записа as int</param>
        /// <param name="CourtId">ID на съд as int?</param>
        /// <returns>True-Съществува, False-НЕ Съществува</returns>
        bool WorkingDay_IsExist(DateTime Day, int Id, int? CourtId);



        #endregion

        List<SelectListItem> GetDropDownList_CourtHall(int courtId, bool addDefaultElement = true, bool addAllElement = false);

        CheckListViewVM LawUnitSpeciality_SelectForCheck(int lawUnitId);

        bool LawUnitSpeciality_SaveData(CheckListViewVM model);

        IEnumerable<LabelValueVM> CourtSelect_ByUser(string userId);
        IEnumerable<LabelValueVM> Get_Courts(string term, int? id);
        IEnumerable<LabelValueVM> Get_Organizations(string term, int? id);
        IEnumerable<LabelValueVM> Get_CaseReasons(string term, int? id);

        IQueryable<LawUnit> LawUnit_ByCourtDate(int court, DateTime? date, int organizationId);

        /// <summary>
        /// Съдилища за ръчно въвеждане на призовки
        /// </summary>
        /// <param name="courtId"></param>
        List<SelectListItem> CourtForDelivery_SelectDDL(int courtId);

        IQueryable<CourtBankAccountVM> CourtBankAccount_Select(int courtId);

        bool CourtBankAccount_SaveData(CourtBankAccount model);

        List<SelectListItem> BankAccount_SelectDDL(int courtId, int moneyGroupId, bool addDefaultElement = false, bool addAllElement = false);

        List<SelectListItem> SelectEntity_LawUnitTypes();

        IQueryable<SelectEntityItemVM> SelectEntity_Select(int sourceType, string search, int? objectTypeId = null, long? sourceId = null);
        IQueryable<Address> SelectEntity_SelectAddress(int personSourceType, long personSourceId);

        List<SelectListItem> LawUnitAddress_SelectDDL_ByCaseLawUnitId(int caseLawUnitId, bool addDefaultElement = true, bool addAllElement = false);

        IQueryable<CourtVM> CourtsByType(int courtTypeId);
        bool CourtSaveData(Court model);

        IQueryable<LawUnitAddressListVM> LawUnitAddress_Select(int lawUnitId);

        (bool result, string errorMessage) LawUnitAddress_SaveData(LawUnitAddress model);

        LawUnitAddress LawUnitAddress_GetById(int lawUnitId, long addressId);

        List<BreadcrumbsVM> Breadcrumbs_GetForCase(int CaseId);
        List<BreadcrumbsVM> Breadcrumbs_Document(long documentId);
        List<BreadcrumbsVM> Breadcrumbs_DocumentResolution(long documentResolutionId);
        List<BreadcrumbsVM> Breadcrumbs_GetForCaseSession(int CaseSessionId, bool IsViewRowSession = true);
        List<BreadcrumbsVM> Breadcrumbs_GetCaseSessionFastDocument(int CaseSessionId);
        List<BreadcrumbsVM> Breadcrumbs_GetForCaseSessionAct(int CaseSessionActId);
        List<BreadcrumbsVM> Breadcrumbs_GetForCaseSessionMeeting(int CaseSessionMeetingId, bool sessionOnly = false);
        List<BreadcrumbsVM> Breadcrumbs_GetForDocumentTemplate(int sourceType, long sourceId);
        List<BreadcrumbsVM> Breadcrumbs_GetForDocumentInstitutionCaseInfoCase(int caseId);
        IQueryable<CourtHallVM> CourtHall_Select(int CourtId);
        bool CourtHall_SaveData(CourtHall model);

        IQueryable<CourtJuryFeeListVM> CourtJuryFee_Select(int courtId);

        bool CourtJuryFee_SaveData(CourtJuryFee model, ref string errorMessage);

        List<SelectListItem> COMPort();

        IQueryable<CourtBankAccount> CourtBankAccountForCourt_Select(int courtId);

        IQueryable<CourtPosDeviceListVM> CourtPosDevice_Select(int courtId);

        bool CourtPosDevice_SaveData(CourtPosDevice model);

        List<SelectListItem> CourtPosDevice_SelectDDL(int courtId, bool addDefaultElement = false, bool addAllElement = false);

        List<BreadcrumbsVM> Breadcrumbs_GetForCaseSelectionProtokol(int CaseId);

        List<BreadcrumbsVM> Breadcrumbs_GetForCaseSessionActMoney(int CaseSessionActId);

        List<BreadcrumbsVM> Breadcrumbs_GetForCaseSessionActLawBase(int CaseSessionActId);
        List<BreadcrumbsVM> Breadcrumbs_GetForCaseSessionActComplain(int CaseSessionActId);
        List<BreadcrumbsVM> Breadcrumbs_GetForCaseSessionActComplainEdit(int CaseSessionActComplainId);
        List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonAddress(int casePersonId);
        List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonMeasure(int casePersonId);
        List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonDocument(int casePersonId);
        List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonSentence(int casePersonId);
        List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonInheritance(int casePersonId);
        List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonSentencePunishment(int CasePersonSentenceId);
        List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonSentencePunishmentCrime(int CasePersonSentencePunishmentId);
        List<BreadcrumbsVM> Breadcrumbs_GetForCaseCrime(int CaseId);
        List<BreadcrumbsVM> Breadcrumbs_GetForCaseLoadIndex(int CaseId);
        List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonCrime(int CaseCrimeId);

        IEnumerable<SelectListItem> GetEnumSelectList<T>();

        Court GetCourt(int id);

        List<BreadcrumbsVM> Breadcrumbs_GetForCurrentCourt(int courtId, string returnUrl);
        List<BreadcrumbsVM> Breadcrumbs_GetForCourts();
        List<BreadcrumbsVM> Breadcrumbs_GetForCourt(int courtId, string returnUrl);
        List<BreadcrumbsVM> Breadcrumbs_ForDeliveryAreas();
        List<BreadcrumbsVM> Breadcrumbs_ForDeliveryArea(int deliveryAreaId);
        List<BreadcrumbsVM> Breadcrumbs_ForDeliveryAreaAddresses(int deliveryAreaId);
        List<BreadcrumbsVM> Breadcrumbs_ForDeliveryAreaAddressesDuplication();
        List<BreadcrumbsVM> Breadcrumbs_ForDeliveryAreaAddressEdit(int deliveryAreaId, int deliveryAreaAddressId);
        List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItems(int filterType);
        List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemEditRaion(int filterType, int deliveryItemId);
        List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemEditReturn(int filterType, int deliveryItemId);
        List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemHistoryOpers(int filterType, int deliveryItemId);
        List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemAddOper(int filterType, int deliveryItemId);
        List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemsTrans(int toNotificationStateId);
        List<BreadcrumbsVM> Breadcrumbs_ForCaseNotifications(int notificationId, int notificationListTypeId);
        List<BreadcrumbsVM> Breadcrumbs_ForCaseNotifications(int caseId, int? caseSessionId, int? caseSessionActId, int notificationListTypeId);
        List<BreadcrumbsVM> Breadcrumbs_ForCaseNotificationEdit(int notificationId);

        List<BreadcrumbsVM> Breadcrumbs_ForCourtGroups(int filterCaseGroupId);
        List<BreadcrumbsVM> Breadcrumbs_ForCourtGroupEdit(int filterCaseGroupId, int id);
        List<BreadcrumbsVM> Breadcrumbs_ForCourtGroupAdd(int filterCaseGroupId);
        List<BreadcrumbsVM> Breadcrumbs_ForEditCourtGroupLawUnit(int filterCaseGroupId);
        List<BreadcrumbsVM> Breadcrumbs_ForLawUnit(int lawUnitTypeId);
        List<BreadcrumbsVM> Breadcrumbs_ForLawUnitEdit(int lawUnitTypeId, int id);
        List<BreadcrumbsVM> Breadcrumbs_ForLawUnitAdd(int lawUnitTypeId);
        List<BreadcrumbsVM> Breadcrumbs_ForCourtBankAccount();
        List<BreadcrumbsVM> Breadcrumbs_ForCourtPosDevice();
        List<BreadcrumbsVM> Breadcrumbs_ForCourtBankAccountEdit(int id);
        List<BreadcrumbsVM> Breadcrumbs_ForCourtBankAccountAdd();
        List<BreadcrumbsVM> Breadcrumbs_ForCourtPosDeviceEdit(int id);
        List<BreadcrumbsVM> Breadcrumbs_ForCourtPosDeviceAdd();
        List<BreadcrumbsVM> Breadcrumbs_ForSpeciality(int lawUnitId);
        List<BreadcrumbsVM> Breadcrumbs_ForLawUnitAddress(int lawUnitId);
        List<BreadcrumbsVM> Breadcrumbs_ForLawUnitAddressEdit(int lawUnitId, long id);
        List<BreadcrumbsVM> Breadcrumbs_ForLawUnitAddressAdd(int lawUnitId);
        List<BreadcrumbsVM> Breadcrumbs_ForCourtLawUnit(int periodTypeId, int lawUnitTypeId);
        List<BreadcrumbsVM> Breadcrumbs_ForCourtLawUnitAdd(int periodTypeId, int lawUnitTypeId);
        List<BreadcrumbsVM> Breadcrumbs_ForCourtLawUnitEdit(int periodTypeId, int lawUnitTypeId, int id);
        List<BreadcrumbsVM> Breadcrumbs_ForCourtLawUnitGroup(int courtLawUnitId);
        List<BreadcrumbsVM> Breadcrumbs_ForCourtLawUnitCompartment(int courtLawUnitId);
        List<BreadcrumbsVM> Breadcrumbs_ForCourtLawUnitCompartmentAdd(int courtLawUnitId);
        List<BreadcrumbsVM> Breadcrumbs_ForCourtLawUnitCompartmentEdit(int courtLawUnitId, int compartmentId);
        List<BreadcrumbsVM> Breadcrumbs_ForCourtJuryFee();
        List<BreadcrumbsVM> Breadcrumbs_ForCourtJuryFeeEdit(int id);
        List<BreadcrumbsVM> Breadcrumbs_ForCourtJuryFeeAdd();
        List<BreadcrumbsVM> Breadcrumbs_Account();
        List<BreadcrumbsVM> Breadcrumbs_AccountEdit(string id);
        List<BreadcrumbsVM> Breadcrumbs_AccountAdd();
        List<BreadcrumbsVM> Breadcrumbs_ForCaseEvidence(int caseEvidenceId);
        List<BreadcrumbsVM> Breadcrumbs_Institution(int institutionTypeId);
        List<BreadcrumbsVM> Breadcrumbs_InstitutionEdit(int id);
        List<BreadcrumbsVM> Breadcrumbs_InstitutionAdd(int institutionTypeId);
        List<BreadcrumbsVM> Breadcrumbs_Counter();
        List<BreadcrumbsVM> Breadcrumbs_CounterEdit(int id);
        List<BreadcrumbsVM> Breadcrumbs_CounterAdd();
        List<BreadcrumbsVM> Breadcrumbs_WorkingDays();
        List<BreadcrumbsVM> Breadcrumbs_WorkingDaysEdit(int id);
        List<BreadcrumbsVM> Breadcrumbs_WorkingDaysAdd();
        List<BreadcrumbsVM> Breadcrumbs_CaseCode();
        List<BreadcrumbsVM> Breadcrumbs_CaseCodeEdit(int id);
        List<BreadcrumbsVM> Breadcrumbs_CaseCodeAdd();
        List<BreadcrumbsVM> Breadcrumbs_LoadGroup();
        List<BreadcrumbsVM> Breadcrumbs_LoadGroupEdit(int id);
        List<BreadcrumbsVM> Breadcrumbs_LoadGroupAdd();
        List<BreadcrumbsVM> Breadcrumbs_LoadGroupLink(int loadGroupId);
        List<BreadcrumbsVM> Breadcrumbs_LoadGroupLinkEdit(int loadGroupId, int id);
        List<BreadcrumbsVM> Breadcrumbs_LoadGroupLinkAdd(int loadGroupId);
        List<BreadcrumbsVM> Breadcrumbs_HtmlTemplate();
        List<BreadcrumbsVM> Breadcrumbs_HtmlTemplateLink(int htmlTemplateId);
        List<BreadcrumbsVM> Breadcrumbs_HtmlTemplateParam(int htmlTemplateId);
        List<BreadcrumbsVM> Breadcrumbs_HtmlTemplateEdit(int id);
        List<BreadcrumbsVM> Breadcrumbs_HtmlTemplateAdd();
        List<BreadcrumbsVM> Breadcrumbs_HtmlTemplatePreview(int id);
        List<BreadcrumbsVM> Breadcrumbs_Document();
        List<BreadcrumbsVM> Breadcrumbs_DocumentEdit(long id);
        List<BreadcrumbsVM> Breadcrumbs_DocumentObligation(long id);
        List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemOpers(int filterType, int deliveryItemId);
        List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemOpers(int notificationId);
        List<BreadcrumbsVM> Breadcrumbs_ForCaseNotificationDeliveryOper(int notificationId);
        List<BreadcrumbsVM> Breadcrumbs_ForCaseNotificationDeliveryOperEdit(int notificationId, int deliveryOperId);
        List<BreadcrumbsVM> Breadcrumbs_ForCaseNotificationEdit(CaseNotification notification, int notificationListTypeId);
        List<BreadcrumbsVM> Breadcrumbs_ForCaseNotificationEditTinyMCE(CaseNotification notification);
        List<BreadcrumbsVM> Breadcrumbs_ForCaseNotificationEditReturn(int notificationId);
        List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemOperEdit(int filterType, int deliveryItemOperId);
        List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemChangeLawUnit();
        CourtBankAccount GetCourtBankAccountForMoneyType(int moneyTypeId);
        IQueryable<LawUnit> LawUnit_ByCourt(int court);
        IQueryable<MultiSelectTransferVM> LawUnitMultiSelect_ByCourt(int courtId);
        List<BreadcrumbsVM> Breadcrumbs_ArchiveCommittee();
        List<BreadcrumbsVM> Breadcrumbs_ArchiveCommitteeEdit(int id);
        List<BreadcrumbsVM> Breadcrumbs_ArchiveCommitteeAdd();
        List<BreadcrumbsVM> Breadcrumbs_ArchiveIndex();
        List<BreadcrumbsVM> Breadcrumbs_ArchiveIndexEdit(int id);
        List<BreadcrumbsVM> Breadcrumbs_ArchiveIndexAdd();
        IQueryable<InstitutionAddressListVM> InstitutionAddress_Select(int institutionId);
        (bool result, string errorMessage) InstitutionAddress_SaveData(InstitutionAddress model);
        InstitutionAddress InstitutionAddress_GetById(int institutionId, long addressId);
        List<BreadcrumbsVM> Breadcrumbs_ForInstitutionAddress(int institutionId);
        List<BreadcrumbsVM> Breadcrumbs_ForInstitutionAddressEdit(int institutionId, long id);
        List<BreadcrumbsVM> Breadcrumbs_ForInstitutionAddressAdd(int institutionId);

        List<SelectListItem> GetDDL_Institution(int institutionTypeId);
        List<BreadcrumbsVM> Breadcrumbs_DocumentDecision();
        List<BreadcrumbsVM> Breadcrumbs_DocumentDecisionEdit(long id);
        List<BreadcrumbsVM> Breadcrumbs_DocumentDecisionAdd(long documentId);
        List<BreadcrumbsVM> Breadcrumbs_CaseDeadLine();
        List<BreadcrumbsVM> Breadcrumbs_ForDeliveryResultList();
        List<BreadcrumbsVM> Breadcrumbs_ForDeliveryOutList();
        List<BreadcrumbsVM> Breadcrumbs_CourtRegionEdit(int courtRegionId);
        List<BreadcrumbsVM> Breadcrumbs_CourtRegion();
        List<BreadcrumbsVM> Breadcrumbs_CourtRegionIndexArea(int courtRegionId);
        List<BreadcrumbsVM> Breadcrumbs_CourtRegionIndexAreaEdit(int courtRegionId, int courtAreaId);
        Court Court_GetById(int id);
        void FillCourtAddress();
        List<BreadcrumbsVM> Breadcrumbs_HtmlTemplateParamEdit(int htmlTemplateId, int id);
        List<BreadcrumbsVM> Breadcrumbs_HtmlTemplateLinkEdit(int htmlTemplateId, int id);
        List<BreadcrumbsVM> Breadcrumbs_AccountMobileToken(string userId);
        List<BreadcrumbsVM> Breadcrumbs_AccountMobileTokenRegister(string userId);
        List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonLink(int CaseId);
        List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonLinkEdit(int CaseId, int casePersonLinkId);
        List<BreadcrumbsVM> Breadcrumbs_ForDeliveryItemAdd(int filterType);
        IQueryable<AddressVM> Address_Select(AddressFilterVM model);
        bool Address_SaveData(Address model);

        void Address_LocationCorrection(Address model);
        List<BreadcrumbsVM> Breadcrumbs_GetForEisppEvents(int caseId);
        List<BreadcrumbsVM> Breadcrumbs_GetForEisppEventsCourt(int courtId);
        List<BreadcrumbsVM> Breadcrumbs_GetForEisppEventEdit(int caseId, string mode);
        List<BreadcrumbsVM> Breadcrumbs_GetForEisppEventChangeEdit(int caseId, bool isDelete);
        List<BreadcrumbsVM> Breadcrumbs_GetForEisppEventChangeEditNew(int caseId, bool isDelete);
        List<BreadcrumbsVM> Breadcrumbs_GetForEisppEventChangeEditOld(int caseId, bool isDelete);
        IQueryable<BankAccountVM> BankAccount_Select(int sourceType, long sourceId);
        BankAccountEditVM BankAccount_GetById(int id);
        bool BankAccount_SaveData(BankAccountEditVM model);
        List<BreadcrumbsVM> BankAccount_LoadBreadCrumbs(int sourceType, long sourceId);
        List<BreadcrumbsVM> BankAccount_LoadBreadCrumbsAddEdit(int sourceType, long sourceId);
        List<BreadcrumbsVM> Breadcrumbs_GetForCasePersonSentenceBulletin(int id);
        List<BreadcrumbsVM> Breadcrumbs_GetForCaseSessionActDivorce(int id);
        CourtLawUnit GetGeneralJudgeCourtLawUnit(int courtId);
        bool UpdateCaseJudicalCompositionOtdelenie(int CaseId);
        List<BreadcrumbsVM> Breadcrumbs_ForExecList();
        List<BreadcrumbsVM> Breadcrumbs_ForExecListEdit(int id);
        List<BreadcrumbsVM> Breadcrumbs_ForCaseNotificationListPrint(int caseSessionId, int notificationListTypeId);
    }
}
