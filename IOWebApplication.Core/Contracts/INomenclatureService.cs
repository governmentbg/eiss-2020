using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    /// <summary>
    /// Общи номенклатури
    /// </summary>
    public interface INomenclatureService : IBaseService
    {
        /// <summary>
        /// Локализиран списък за показване в таблица
        /// </summary>
        /// <typeparam name="T">Тип на номенклатурата</typeparam>
        /// <returns></returns>
        IQueryable<CommonNomenclatureListItem> GetList<T>() where T : class, ICommonNomenclature;

        /// <summary>
        /// Конкретен елемент от номенклатура
        /// </summary>
        /// <typeparam name="T">Тип на номенклатурата</typeparam>
        /// <param name="id">Идентификатор на запис</param>
        /// <returns></returns>
        T GetItem<T>(int id) where T : class, ICommonNomenclature;

        /// <summary>
        /// Запис на елемент от номенклатура
        /// </summary>
        /// <typeparam name="T">Тип на номенклатурата</typeparam>
        /// <param name="entity">Елемент за запис</param>
        /// <returns></returns>
        bool SaveItem<T>(T entity) where T : class, ICommonNomenclature;

        /// <summary>
        /// Промяна на подредбата с бутони
        /// </summary>
        /// <typeparam name="T">Тип на номенклатурата</typeparam>
        /// <param name="model">Посока и идентификатор на запис</param>
        /// <returns></returns>
        bool ChangeOrder<T>(ChangeOrderModel model) where T : class, ICommonNomenclature;

        /// <summary>
        /// Генерира списък от елементи на номенклатура за комбо
        /// </summary>
        /// <typeparam name="T">Тип на номенклатурата</typeparam>
        /// <param name="addDefaultElement">Дали да добави елемент "Изберете"
        /// по подразбиране е изтина</param>
        /// <returns></returns>
        List<SelectListItem> GetDropDownList<T>(bool addDefaultElement = true, bool addAllElement = false, bool orderByNumber = true) where T : class, ICommonNomenclature;

        /// <summary>
        /// Генерира списък от елементи на номенклатура за комбо
        /// </summary>
        /// <typeparam name="T">Тип на номенклатурата</typeparam>
        /// <param name="addDefaultElement">Дали да добави елемент "Изберете"
        /// по подразбиране е изтина</param>
        /// <returns></returns>
        List<SelectListItem> GetDropDownListDescription<T>(bool addDefaultElement = true, bool addAllElement = false) where T : class, ICommonNomenclature;

        /// <summary>
        /// Генерира списък от елементи на номенклатура за комбо
        /// Подредени по OrderNumber
        /// </summary>
        /// <typeparam name="T">Тип на номенклатурата</typeparam>
        /// <param name="addDefaultElement">Дали да добави елемент "Изберете"
        /// по подразбиране е изтина</param>
        /// <returns></returns>
        List<SelectListItem> GetDropDownOrderedList<T>(bool addDefaultElement = true, bool addAllElement = false) where T : class, ICommonNomenclature;

        /// <summary>
        /// Информация за autocomplete контрола за Екатте
        /// </summary>
        /// <param name="query">Част от име на обект</param>
        /// <returns></returns>
        HierarchicalNomenclatureDisplayModel GetEkatte(string query);

        /// <summary>
        /// Стойност на Екатте по идентификатор
        /// </summary>
        /// <param name="id">Код по Екатте</param>
        /// <returns></returns>
        HierarchicalNomenclatureDisplayItem GetEkatteById(string id);

        /// <summary>
        /// Информация за autocomplete контрола за Street
        /// </summary>
        /// <param name="query">Част от име на обект</param>
        /// <returns></returns>
        IEnumerable<LabelValueVM> GetStreet(string ekatte, string query, int? streetType = null);

        /// <summary>
        /// Връща List SelectListItem от value=Code,text=Label на BaseCommonNomenclature
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <param name="orderByNumber"></param>
        /// <returns></returns>
        List<SelectListItem> GetDropDownListFromCode<T>(bool addDefaultElement = true, bool addAllElement = false, bool orderByNumber = true) where T : class, ICommonNomenclature;


        /// <summary>
        /// Информация за autocomplete контрола за Street
        /// </summary>
        /// <param name="code">Код на улица</param>
        /// <returns></returns>
        LabelValueVM GetStreetByCode(string ekatte, string code, int? streetType = null);
        LabelValueVM GetEkatteByEisppCode(string eisppCode);

        List<SelectListItem> GetDDL_DocumentGroup(int documentKindId);

        Task<List<SelectListItem>> GetDDL_DocumentGroupAsync(int documentKindId);

        List<SelectListItem> GetDDL_DocumentGroupByCourt(int documentKindId, int? courtOrganizationId = null);

        /// <summary>
        /// Зарежда точни видове документи по основен
        /// </summary>
        /// <param name="documentGroupId"></param>
        /// <returns></returns>
        List<SelectListItem> GetDDL_DocumentType(int documentGroupId, bool addDefaultElement = false);

        /// <summary>
        /// Метод зареждащ видове документи за падащ списък
        /// </summary>
        /// <param name="addDefaultElement">Дали да добави елемент "Избери"</param>
        /// <param name="documentKind">Вид документ</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDDL_DocumentTypeSortByName(bool addDefaultElement = true, int? documentKind = null);

        /// <summary>
        /// Метод зареждащ видове документи за падащ списък
        /// </summary>
        /// <param name="caseTypeId">Тип дело</param>
        /// <param name="documentKind">Вид документ</param>
        /// <param name="addDefaultElement">Дали да добави елемент "Избери"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDDL_DocumentTypeByCaseType(int caseTypeId, int? documentKind = null, bool addDefaultElement = true);

        List<SelectListItem> GetDDL_DocumentTypeByCourt(int documentGroupId, bool addDefaultElement = false, bool addAllElement = false, int? courtOrganizationId = null);
        List<SelectListItem> GetDDL_DocumentKind(int documentDirectionId, bool addDefaultElement = false, bool addAllElement = false);
        Task<List<SelectListItem>> GetDDL_DocumentKindAsync(int documentDirectionId, bool addDefaultElement = false, bool addAllElement = false);

        /// <summary>
        /// Зарежда точни видове дела по основен
        /// </summary>
        /// <param name="caseGroupId"></param>
        /// <returns></returns>
        List<SelectListItem> GetDDL_CaseType(int caseGroupId, bool addDefaultElement = true);
        List<SelectListItem> GetDDL_CaseTypes(string caseGroupIds, bool addDefaultElement = true);
        List<SelectListItem> GetDDL_CaseTypeFromCourtType(int caseGroupId, string caseInstanceIds, bool addDefaultElement = true);

        List<SelectListItem> GetDDL_CaseTypeByDocType(int documentTypeId, int? caseCharacter = null, int? courtOrganizationId = null);

        List<SelectListItem> GetDDL_CaseCodeDropDown(int[] caseTypeIds);

        /// <summary>
        /// Зарежда шифри на дела по точен вид
        /// </summary>
        /// <returns></returns>
        List<LabelValueVM> GetDDL_CaseCode(int[] caseTypeIds, string search = null, int? caseCodeId = null, bool byLoadGroup = false);
        //IQueryable<CaseCode> Get_CaseCode(int caseTypeId, string search = null, int? caseCodeId = null, bool byLoadGroup = false);

        /// <summary>
        /// Зарежда характер на делото
        /// </summary>
        /// <param name="caseTypeId"></param>
        /// <returns></returns>
        List<SelectListItem> GetDDL_CaseCharacter(int caseTypeId, int? caseCharacterId = null);

        /// <summary>
        /// Зарежда списък с държави
        /// </summary>
        /// <param name="addDefaultElement">Дали да добави елемент "Изберете"</param>
        /// <returns></returns>
        List<SelectListItem> GetCountries(bool addDefaultElement = false);

        /// <summary>
        /// Зарежда списък с държави
        /// </summary>
        /// <param name="addDefaultElement">Дали да добави елемент "Изберете"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetCountriesAsync(bool addDefaultElement = false);

        /// <summary>
        /// Зарежда списък с държави
        /// </summary>
        /// <param name="addDefaultElement">Дали да добави елемент "Изберете"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDDL_Countries(bool addDefaultElement = false);

        /// <summary>
        /// Зарежда списък с съдилища
        /// </summary>
        /// <returns></returns>
        List<SelectListItem> GetCourts();

        /// <summary>
        /// Зарежда списък с съдилища
        /// </summary>
        /// <returns></returns>
        Task<List<SelectListItem>> GetCourtsAsync();


        List<SelectListItem> GetDDL_DeliveryType(int deliveryGroupId);

        void InitEkStreets(IEnumerable<EkStreet> model);

        /// <summary>
        /// Номенклатура за типове дела
        /// </summary>
        /// <returns></returns>
        IQueryable<CaseType> CaseTypeNow(int courtId);

        /// <summary>
        /// Връща списък с години: от 2000 до сега.Year
        /// </summary>
        /// <returns></returns>
        List<SelectListItem> GetCaseYears();

        void SetFullAddress(Address model);

        string GetFullAddress(Address model, bool setContactData, bool munAreaNameFirst, bool vksCase);

        List<SelectListItem> GetDDL_LoadGroupLink(int courtTypeId, int caseTypeId, int caseCodeId = NomenclatureConstants.NullVal);

        List<HtmlTemplateDdlVM> GetDDL_HtmlTemplate(int notificationTypeId, int caseId, int? htmlTemplateId, bool addDefaultElement = true);
        List<SelectListItem> GetDDL_HtmlTemplateByDocType(int documentTypeId, int caseId, int sourceType, int courtTypeId, bool setDefault);

        /// <summary>
        /// Всички codes за една група за мултиселекта
        /// </summary>
        /// <param name="caseGroupId"></param>
        /// <returns></returns>
        IQueryable<MultiSelectTransferVM> CaseCodeForSelect_Select(int caseGroupId);
        IQueryable<MultiSelectTransferVM> CaseCodeForSelect_SelectAll(int caseGroupId, int courtTypeId, int caseInstanceId);

        List<SelectListItem> GetDDL_CaseReason(int caseTypeId);


        /// <summary>
        /// Всички типове номера за райони за доставка
        /// </summary>
        List<SelectListItem> GetDDL_DeliveryNumberType();


        List<SelectListItem> GetDDL_NotificationStateFromDeliveryGroup(int deliveryTypeId, int norificationStateId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_NotificationDeliveryType(int deliveryGroupId, bool addDefaultElement = true);
        List<SelectListItem> GetDDL_SessionResult(bool addDefaultElement = true, bool addAllElement = false);
        Task<List<SelectListItem>> GetDDL_SessionResultAsync(bool addDefaultElement = true, bool addAllElement = false);

        /// <summary>
        /// Метод връщащ списък с основания към резултати
        /// </summary>
        /// <param name="sessionResultId">Идентификатор на резултат</param>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент "Избери"</param>
        /// <param name="addAllElement">Флаг за добавяне на елемент "Всички"</param>
        /// <param name="withDateCheck">Флаг указващ метода дали да гледа датите</param>
        /// <returns></returns>
        List<SelectListItem> GetDDL_SessionResultBase(int sessionResultId, bool addDefaultElement = true, bool addAllElement = false, bool withDateCheck = false, int? caseId = null, int? caseSessionId = null);

        EkDistrict GetEkDistrictByEkatte(string Ekatte);
        List<SelectListItem> GetDDL_EkDistrict(bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_EkMunincipality(string EkatteDistrict, bool addDefaultElement = true, bool addAllElement = false);
        Task<List<SelectListItem>> GetDDL_SessionDuration(bool addDefaultElement = true, bool addAllElement = false);

        Task<List<SelectListItem>> GetDDL_SessionTypesByCase(int caseId, int? oldSessionTypeId, bool addDefaultElement = true, bool addAllElement = false, DateTime? dtNow = null);
        Task<List<SelectListItem>> GetDDL_SessionTypesByCaseByGroupe(int caseId, int SessionTypeGroupId, bool addDefaultElement = true, bool addAllElement = false, DateTime? dtNow = null);
        List<SelectListItem> GetDDL_CaseTypeForCourt(int courtId);

        List<SelectListItem> GetDDL_CaseStateHand(bool addDefaultElement = true, bool addAllElement = false);

        List<SelectListItem> GetSelectionLawUnitState(int selectionMode, bool disabledOnly = false);

        List<SelectListItem> GetDDL_MoneyClaimType(int moneyClaimGroupId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_MoneyCollectionType(int moneyCollectionGroupId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_MoneyCollectionKind(int moneyCollectionGroupId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDismisalTypes_SelectForDropDownList(int dismisalKindId);
        Task<List<SelectListItem>> GetDismisalTypes_SelectForDropDownListAsync(int CaseLawUnitId);
        List<SelectListItem> GetDDL_SpecialityForFilter(int? lawUnitTypeId = null);
        List<SelectListItem> GetDDL_VksSessionLawunitChange();

        bool CaseCodeGroup_Check(string alias, int caseCodeId);
        List<SelectListItem> GetDDL_MoneyFeeType(int documentGroupId);
        Bank GetBankByCodeSearch(string codeSearch);
        List<SelectListItem> GetDDL_CaseTypeGroupInstance(int caseGroupId, int caseInstanceId, string caseInstanceIds);
        List<SelectListItem> GetDDL_DecisionType(int documentTypeId);
        int[] GetCaseCodeGroupingByGroup(int groupId);
        List<SelectListItem> GetCountriesWitoutBG_DDL();
        List<SelectListItem> GetDDL_LawUnitPosition(int LawUnitTypeId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_ActComplainResult(int CaseTypeId, bool addDefaultElement = true, bool addAllElement = false);
        Task<List<SelectListItem>> GetDDL_ActComplainResultAsync(int CaseTypeId, bool addDefaultElement = true, bool addAllElement = false);
        Task<List<SelectListItem>> GetDDL_ActComplainResult(bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_ActComplainIndex(int CaseId, int CaseSessionActId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_ActComplainIndexByCourtType(bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_ActResult(bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_ActResult(int CaseFromId, int CaseSessionActComplainId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_ActResultOtherCase(string CaseRegNumberOtherSystem, int CaseSessionActComplainId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_ExecListLawBase(int caseGroupId, bool addDefaultElement = true, bool addAllElement = false);

        /// <summary>
        /// Четене на ид-та за PersonRole по група
        /// </summary>
        /// <param name="personRoleGroup"></param>
        /// <returns></returns>
        Task<int[]> GetPersonRoleIdsByGroup(int personRoleGroup);

        Task<List<SelectListItem>> GetDDL_DocumentGroupByDirection(int documentDirectionId);
        Task<List<SelectListItem>> GetDDL_CaseState(bool InitialOnly, bool HideInitialStates);
        List<SelectListItem> GetDDL_CaseSessionState(bool InitialOnly);
        List<SelectListItem> GetDDL_CaseSessionActState(bool InitialOnly, bool HideInitialStates, bool actIsDeclared);
        List<SelectListItem> GetDDL_JudgeRoleManualRoles(bool addDefaultElement = true, bool addAllElement = false);
        Task<List<SelectListItem>> GetDDL_JudgeRoleManualRolesAsync(bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_ByCourtTypeInstanceList(int[] courtTypeInstanceList, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_SessionState(bool InitialOnly);
        List<SelectListItem> GetDDL_SessionStateFiltered(int currentStateId);
        Task<List<SelectListItem>> GetDDL_SessionStateRoute(int currentStateId);
        List<SelectListItem> GetDDL_Specyality_ByLowUnit_Type(int lawunitTypeId, bool addDefaultElement = true, bool addAllElement = false);
        int GetInnerCodeFromCodeMapping(string alias, string outerCode);
        string GetOuterCodeFromCodeMapping(string alias, string innerCode);
        CodeMapping GetInnerCodeFromCodeMappingStr(string alias, string outerCode);
        List<SelectListItem> GetDDL_CaseSessionResult(bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_CaseTypeByCourtType(int caseGroupId, int courtTypeId, bool addDefaultElement);
        List<SelectListItem> GetDDL_MoneyFineType(int caseGroupId, bool addDefaultElement = true);
        List<SelectListItem> GetDDL_SessionTypeWithoutClosedSession(bool addDefaultElement = true);
        List<SelectListItem> GetDDL_CourtGroup(int courtId, bool addDefaultElement = true);
        List<SelectListItem> GetDDL_CourtGroup_Substitution(int courtId, bool addDefaultElement = true);
        List<SelectListItem> GetDDL_LoadGroupLink(bool addDefaultElement = true);
        List<SelectListItem> GetDDL_SessionResultFromRules(int CaseSessionId, bool addDefaultElement = true);
        Task<List<SelectListItem>> GetDDL_SessionResultFromRulesAsync(int CaseSessionId, bool addDefaultElement = true);
        Task<List<SelectListItem>> GetDDL_SessionResultFromRulesByCaseId(int CaseId, bool addDefaultElement = true);
        List<SelectListItem> GetDDL_SessionResultFromRulesByCaseLoadElementTypeAndSessionType(int CaseLoadElementTypeId, int SessionTypeId, bool addDefaultElement = true);
        Task<List<SelectListItem>> GetDDL_CaseMigrationType(int directionId, bool addDefaultElement = true);
        IQueryable<MoneyType> Get_MoneyType();
        List<SelectListItem> GetDDL_StreetType(bool addDefaultElement = true);
        IQueryable<EkStreetVM> EkStreet_Select(EkStreetFilterVM model);
        bool EkStreet_SaveData(EkStreet model);
        List<SelectListItem> GetDDL_SessionToDate(bool addDefaultElement = true);

        /// <summary>
        /// Зареждане на списък с разлика между дата на заседание и дата на обявяване на акта
        /// </summary>
        /// <param name="addDefaultElement">Добавя елемент "Избери"</param>
        /// <returns></returns>
        List<SelectListItem> GetDDL_ActToDate(bool addDefaultElement = true);

        List<SelectListItem> GetDDL_ComplexIndex(bool addDefaultElement = true);
        Task<List<SelectListItem>> GetDDL_SessionResultGrouping(int groupId);
        List<SelectListItem> GetDDL_CaseCreateFroms(int instanceId);
        Task<List<SelectListItem>> GetDDL_SessionResultFromRulesByFilter(int caseGroupId, int courtTypeId, bool addDefaultElement = true);
        List<SelectListItem> GetDDL_IsFinalAct(bool addDefaultElement = true);
        CaseRegNumberVM DecodeCaseRegNumber(string regNumber);
        IEnumerable<LabelValueVM> Get_ActLawBase(string query, int CaseId, int id);
        IEnumerable<LabelValueVM> Get_PersonRoles(string query, int? id);
        List<SelectListItem> GetDDL_ObligationJuryReportPersonType();

        List<SelectListItem> GetDDL_DocumentGroupWithKind();


        int[] GetHtmlTemplateForCasePerson();

        int[] GetHtmlTemplateForFromToDate();

        List<SelectListItem> GetDDL_MoneyCountryReceiver();
        HierarchicalNomenclatureDisplayModel GetEkatteEispp(string query);
        HierarchicalNomenclatureDisplayItem GetEkatteByEisppCodeCategory(string eisppCode);

        IQueryable<LawBaseVM> LawBase_Select(int CaseId);
        bool LawBase_SaveData(LawBaseEditVM model);
        Case GetCaseWithIncluded(int CaseId);
        LawBaseEditVM LawBase_GetById(int id);
        IEnumerable<SelectListItem> GetDDL_LawBase(int CaseId, bool addDefaultElement = true, bool addAllElement = false);
        bool IsExistsNameLawBase(string Label, int CaseId);
        List<HtmlTemplateDdlVM> GetDDL_HtmlTemplateDocument(int notificationTypeId, bool addDefaultElement = true);
        IEnumerable<LabelValueVM> Get_EISPPTblElement(string EisppTblCode, string term, string id);
        EisppTblElement GetByCode_EISPPTblElement(string Code);
        Task<EisppTblElement> GetByCode_EISPPTblElementAsync(string Code);
        List<SelectListItem> GetDDL_ActMotiveToDate(bool addDefaultElement = true);
        EkEkatte GetEkatteByEkatte(string ekatte);
        List<SelectListItem> GetDDL_CaseSelectionChangeType(int typeId, bool addDefaultElement = true);

        /// <summary>
        /// Метод извличащ тип състав по точен вид дело
        /// </summary>
        /// <param name="caseTypeId">Вид дело</param>
        /// <param name="addDefaultElement">Флаг дали да се добави елемент "Избери"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDDL_CaseTypeUnit(int caseTypeId, bool addDefaultElement = true);
        int[] GetPunishmentFineNoValidate();
        List<SelectListItem> GetDDL_IsGenerated();
        Task<List<SelectListItem>> GetDropDownListAsync<T>(bool addDefaultElement = true, bool addAllElement = false, bool orderByNumber = true) where T : class, ICommonNomenclature;

        /// <summary>
        /// Извличане на данни за падащ списък за видове мерки за престъпление
        /// </summary>
        /// <returns></returns>
        List<SelectListItem> GetDDL_MeasureKind();

        /// <summary>
        /// Метод извличащ настройки за наказанията
        /// </summary>
        /// <param name="sentenceTypeId">Идентификатор на записа</param>
        /// <returns></returns>
        Task<SentenceTypeHasVM> GetSentenceTypeHas(int sentenceTypeId);

        /// <summary>
        /// Извличане на данни за падащ списък за форма на вината за престъпление
        /// </summary>
        /// <returns></returns>
        List<SelectListItem> GetDDL_FormGuilt();

        /// <summary>
        /// Извличане на данни за падащ списък занаселени места
        /// </summary>
        /// <param name="countryId">Идентификатор на държава</param>
        /// <param name="addDefaultElement">Флаг дали да се добави елемент "Избери"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDDL_EkatteSobr(int? countryId, bool addDefaultElement = true);

        /// <summary>
        /// Извличане на данни за падащ списък за населени места от ЕИСПП
        /// </summary>
        /// <param name="countryId">Идентификатор на държава</param>
        /// <param name="addDefaultElement">Флаг дали да се добави елемент "Избери"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDDL_EisppEkatte(int? countryId, bool addDefaultElement = true);

        List<SelectListItem> GetDropDownListCodeCode<T>(bool addDefaultElement = true, bool addAllElement = false, bool orderByNumber = true) where T : class, ICommonNomenclature;

        /// <summary>
        /// Извличане на данни за падащ списък за WorkNotificationType
        /// </summary>
        /// <param name="expressionWhere">Where клауза</param>
        /// <param name="addDefaultElement">Флаг дали да се добави елемент "Избери"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDDL_WorkNotificationType(Expression<Func<WorkNotificationType, bool>> expressionWhere = null, bool addDefaultElement = true);
        Task<List<SelectListItem>> GetDDL_MongoFileTypes(string fileGroup);
        List<SelectListItem> GetDropDownListCodeDescription<T>(bool addDefaultElement = true, bool addAllElement = false) where T : class, ICommonNomenclature;

        /// <summary>
        /// Метод връщащ списък с основания към резултати за среща за медиация
        /// </summary>
        /// <param name="mediationResultId">Идентификатор на избраният резултат</param>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент "Избери"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDDL_MediationResultBase(int mediationResultId, bool addDefaultElement = true);

        /// <summary>
        /// Метод извличащ точки за оценка на медиатор в среща за бедиация
        /// </summary>
        /// <param name="typePoint">Тип оценка от NomenclatureConstants.MediationCaseMediatorAppraisalTypeConstants</param>
        /// <returns></returns>
        Task<List<RatingVM>> GetMediationPointMediatorAppraisals(int typePoint);

        #region Подкодове на шифри

        /// <summary>
        /// Извличане на данни за подкодове на шифри
        /// </summary>
        /// <param name="filter">Филтър</param>
        /// <returns></returns>
        IQueryable<CaseCodeSubListDataVM> GetCaseCodeSubs(CaseCodeSubFilterVM filter);

        /// <summary>
        /// Извличане на данни за редакция на подкодове на шифри
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        Task<CaseCodeSubVM> GetCaseCodeSubEditById(int id);

        /// <summary>
        /// Добавяне/редкация на данни за подкодове на шифри
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<int?> SaveCaseCodeSub(CaseCodeSubVM model);

        /// <summary>
        /// Метод извличащ шифри по дело за падащ списък за избор
        /// </summary>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент "Избери"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDDL_CaseCode(bool addDefaultElement = true);

        /// <summary>
        /// Метод извличащ подшифри по дело за падащ списък за избор
        /// </summary>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент "Избери"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDDL_CaseCodeSub(bool addDefaultElement = true);

        List<HtmlTemplateDdlVM> GetDDL_HtmlTemplateMediation(int notificationTypeId, bool addDefaultElement = true);
        List<SelectListItem> GetDDL_ExcelReportTemplateReportType(bool addDefaultElement = true, bool addAllElement = false);
        Task<List<SelectListItem>> GetDLL_ActIspnReasonByGroup(int group, bool addDefaultElement = true, bool addAllElement = false, bool orderByNumber = true);
        string GetStreetAddress(Address model, bool vksCase);

        #endregion
    }
}
