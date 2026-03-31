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
    public interface ICourtLawUnitService : IBaseService
    {
        IQueryable<CourtLawUnitVM> CourtLawUnit_Select(int courtId, CourtLawUnitFilter filter);
        IQueryable<CourtLawUnitVM> CourtLawUnitSpr_Select(int LawUnitId, int PeriodTypeId, DateTime? DateFrom, DateTime? DateTo);

        /// <summary>
        /// Метод извличащ данни за асистент/помощник/секретар
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<CourtLawUnitAssistantViewModel> CourtLawUnitAssistant_Select(CourtLawUnitAssistantFilterViewModel filter);

        (bool result, string errorMessage) CourtLawUnit_SaveData(CourtLawUnit model);

        /// <summary>
        /// Извличане на данни за редакция на CourtLawUnitAssistant
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        Task<CourtLawUnitAssistantEditViewModel> GetCourtLawUnitAssistantById(int id);

        /// <summary>
        /// Запис на CourtLawUnitAssistant
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<bool> CourtLawUnitAssistant_SaveData(CourtLawUnitAssistantEditViewModel model);

        /// <summary>
        /// Метод проверяващ за съществуващ запис за този служител
        /// </summary>
        /// <param name="courtLawUnitId">Идентификатор на записа за съдията</param>
        /// <param name="lawUnitId">Идентификатор на служителя</param>
        /// <param name="id">Идентификатор на записа за служителя</param>
        /// <returns></returns>
        Task<bool> IsExistsCourtLawUnitAssistant(int courtLawUnitId, int lawUnitId, int id);

        /// <summary>
        /// Сторниране на секретар към съдия
        /// </summary>
        /// <param name="id">Идентификатор на запис за секретар към съдия</param>
        /// <returns></returns>
        Task<bool> CourtLawUnitAssistantExpired(int id);

        IQueryable<MultiSelectTransferPercentVM> CourtLawUnitGroup_Select(int courtId, int lawUnitId);

        Task<bool> CourtLawUnitGroup_SaveData(int courtId, int lawUnitId, List<MultiSelectTransferPercentVM> codeGroups);

        //IQueryable<CompartmentVM> Compartment_Select(int courtId, int lawUnitId);

        ///// <summary>
        ///// Съдебен състав за multiselect
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //IQueryable<MultiSelectTransferVM> CompartmentLawUnit_Select(int compartmentId);

        /// <summary>
        /// Връща съдиите в съд за избор от multiselect
        /// </summary>
        /// <param name="courtId"></param>
        /// <returns></returns>
        IQueryable<MultiSelectTransferVM> LawUnitjJudgeForSelect_Select(int courtId, int excludelawUnitId);

        //bool Compartment_SaveData(Compartment model, List<int> codes);

        /// <summary>
        /// изчитане на данни за един CourtLawUnit за запис на групи към съд/съдия
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        CourtLawUnitGroupVM GetCourtLawUnitById(int id);

        /// <summary>
        /// изчитане на CourtlawUnit заедно с LawUnit
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        CourtLawUnit GetCourtLawUnitById_WithLawUnit(int id);

        /// <summary>
        /// Данни за DDL
        /// </summary>
        /// <param name="lawUnitType">Вид лице</param>
        /// <param name="forCourtId">За съд</param>
        /// <returns></returns>

        List<SelectListItem> LawUnitForCourt_SelectDDL(int lawUnitType, int forCourtId, bool noIllHoliday = false);
        List<Select2ItemVM> LawUnitForCourt_Select2Data(int lawUnitType, int forCourtId, bool noIllHoliday = false);
        string GetLawUnitPosition(int courtId, int lawUnitId);
        CourtLawUnit GetCourtLawUnitAllDatabyLawUnitId(int courtId, int lawUnitId);

        IQueryable<CourtLawUnitVM> CourtLawUnitOrder_Select(int courtId);
        bool CourtLawUnitOrder_Actualize(int courtId);

        SaveResultVM CourtDepartmentUnitOrder_ActualizeForCase(int caseId);

        /// <summary>
        /// Информация за заместване на съдии
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        IQueryable<CourtLawUnitSubstitutionVM> CourtLawUnitSubstitution_Select(CourtLawUnitSubstitutionFilter filter);

        /// <summary>
        /// Проверка за валиден запис на заместване
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<string> CourtLawUnitSubstitution_Validate(CourtLawUnitSubstitution model);

        /// <summary>
        /// Метод за запис на заместване
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<bool> CourtLawUnitSubstitution_SaveData(CourtLawUnitSubstitution model);
        
        SaveResultVM CourtLawUnitOrder_ComboSave(CourtLawunitOrderComboVM model);

        #region Група Централизирано разпределение ГД

        /// <summary>
        /// Извличане на данни за служители в Група Централизирано разпределение ГД
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        IQueryable<CourtLawUnitGroupCCDataVM> GetDataCentralDistributionCC(CourtLawUnitGroupCCFilterVM filter);

        /// <summary>
        /// Извличане на данни за редакция на служители в Група Централизирано разпределение ГД
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        Task<CourtLawUnitGroupCCEditVM> GetCentralDistributionCCEditById(int id);

        /// <summary>
        /// Добавяне/редакция на данни за служители в Група Централизирано разпределение ГД
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<int?> SavelCourtLawUnitGroupCentralDistributionCC(CourtLawUnitGroupCCEditVM model);

        /// <summary>
        /// Метод за зареждане на списък с налични служители в съд
        /// </summary>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <param name="lawUnitId">Служител за редакция и да се провери дали го има в списъка, ако е с конфигурирана дата до</param>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент Избери</param>
        /// <param name="addAllElement">Флаг за добавяне на елемент Всички</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDDL_CommonCourtLawUnitCentralDistributionCC(int courtId, int? lawUnitId = null, bool addDefaultElement = true, bool addAllElement = false);

        /// <summary>
        /// Проверка дали служителят е вече добавен в групата, без значение съда
        /// </summary>
        /// <param name="lawUnitId">Идентификатор на служител</param>
        /// <param name="courtGroupKind">Kind на група</param>
        /// <param name="idSave">Идентификатор на запис</param>
        /// <returns></returns>
        Task<bool> IsExistLawUnitCentralDistributionCC(int lawUnitId, int courtGroupKind, int? idSave = null);

        #endregion
    }
}
