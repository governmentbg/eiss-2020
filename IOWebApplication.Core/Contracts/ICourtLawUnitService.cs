// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

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
    public interface ICourtLawUnitService : IBaseService
    {
        IQueryable<CourtLawUnitVM> CourtLawUnit_Select(int courtId, int periodType, int lawUnitType);
        IQueryable<CourtLawUnitVM> CourtLawUnitSpr_Select(int LawUnitId, int PeriodTypeId, DateTime? DateFrom, DateTime? DateTo);

        (bool result, string errorMessage) CourtLawUnit_SaveData(CourtLawUnit model);

        IQueryable<MultiSelectTransferPercentVM> CourtLawUnitGroup_Select(int courtId, int lawUnitId);

        bool CourtLawUnitGroup_SaveData(int courtId, int lawUnitId, List<MultiSelectTransferPercentVM> codeGroups);

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
        CourtLawUnit GetGeneralJudgeCourtLawUnit(int courtId);

        IQueryable<CourtLawUnitVM> CourtLawUnitOrder_Select(int courtId);
        bool CourtLawUnitOrder_Actualize(int courtId);
        bool CourtLawUnitOrder_ActualizeForCase(int caseId);

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
        string CourtLawUnitSubstitution_Validate(CourtLawUnitSubstitution model);

        /// <summary>
        /// Запис на данни за заместване
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool CourtLawUnitSubstitution_SaveData(CourtLawUnitSubstitution model);
    }
}
