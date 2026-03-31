// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.ViewModels.Common.Mediation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    /// <summary>
    /// Допълнителни регистри за медиацията
    /// </summary>
    public interface IMediationCommonService : IBaseService
    {
        #region Центрове за медиация

        /// <summary>
        /// Извличане на данни за центрове за медиация
        /// </summary>
        /// <param name="filter">Филтър</param>
        /// <returns></returns>
        IQueryable<MediationCenterListDataVM> GetMediationCenters(MediationCenterFilterVM filter);

        /// <summary>
        /// Извличане на центрове за медиация за списък за избиране
        /// </summary>
        /// <param name="idSelected">Идентификаотр на текущият, за да се премахме от списъка</param>
        /// <param name="addDefaultElement">Дали да добави елемент "Избери"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetMediationCenterDDL(int? idSelected = null, bool addDefaultElement = true);

        /// <summary>
        /// Извличане на данни за редакция на център зая медиация
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        Task<MediationCenterVM> GetMediationCenterEditById(int id);

        /// <summary>
        /// Добавяне/редкация на данни за център зая медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<int?> SaveMediationCenter(MediationCenterVM model);

        #endregion

        #region Медиатори

        /// <summary>
        /// Извличане на данни за медиатори
        /// </summary>
        /// <param name="filter">Филтър</param>
        /// <returns></returns>
        IQueryable<MediationMediatorListDataVM> GetMediationMediators(MediationMediatorFilterVM filter);

        /// <summary>
        /// Извличане на данни за редакция на медиатор
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        Task<MediationMediatorVM> GetMediationMediatorEditById(int id);

        /// <summary>
        /// Добавяне/редкация на данни за медиатор
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<int?> SaveMediationMediator(MediationMediatorVM model);

        /// <summary>
        /// Извличане на медиатори за падащ списък
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="dateTimeTo">Към коя дата да се извлечат данни</param>
        /// <param name="addDefaultElement">Дали да добави елемент "Избери"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetMediationMediatorsDDL(int caseId, DateTime dateTimeTo, bool addDefaultElement = true);

        /// <summary>
        /// Извличане на медиатори за падащ списък
        /// </summary>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <param name="dateTimeTo">Към коя дата да се извлечат данни</param>
        /// <param name="addDefaultElement">Дали да добави елемент "Избери"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDDL_MediationMediatorsByCourt(int courtId, DateTime dateTimeTo, bool addDefaultElement = true);

        /// <summary>
        /// Извличане на медиатори от центрове на които логнатият юзер е кординатор за падащ списък
        /// </summary>
        /// <param name="dateTimeTo">Към коя дата да се извлечат данни</param>
        /// <param name="addDefaultElement">Дали да добави елемент "Избери"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDDL_MediationMediatorsByCoordinator(DateTime dateTimeTo, bool addDefaultElement = true);

        #endregion

        #region Координатори

        /// <summary>
        /// Извличане на данни за координатор
        /// </summary>
        /// <param name="filter">Филтър</param>
        /// <returns></returns>
        IQueryable<MediationCoordinatorListDataVM> GetMediationCoordinators(MediationCoordinatorFilterVM filter);

        /// <summary>
        /// Извличане на данни за редакция на координатор
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        Task<MediationCoordinatorVM> GetMediationCoordinatorEditById(int id);

        /// <summary>
        /// Добавяне/редкация на данни за координатор
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<int?> SaveMediationCoordinator(MediationCoordinatorVM model);

        /// <summary>
        /// Метод извличащ съдилища към кординатор
        /// </summary>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент "Избери"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDDL_MediationCoordinatorCourt(bool addDefaultElement = true);

        #endregion

        #region Ставки за заплащане на медиатор

        /// <summary>
        /// Извличане на данни за ставки за заплащане на медиатори
        /// </summary>
        /// <param name="filter">Филтър</param>
        /// <returns></returns>
        IQueryable<MediatorFeeListDataVM> GetMediatorFees(MediatorFeeFilterVM filter);

        /// <summary>
        /// Извличане на данни за редакция на ставка за заплащане на медиатори
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        Task<MediatorFeeVM> GetMediatorFeeEditById(int id);

        /// <summary>
        /// Добавяне/редкация на данни за ставка за заплащане на медиатори
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<int?> SaveMediatorFee(MediatorFeeVM model);

        /// <summary>
        /// Проверка за съществуващ период в ставките за заплащане на медиатори
        /// </summary>
        /// <param name="mediationTypeId">Идентификатор на вид среща за медиация</param>
        /// <param name="dateFrom">От дата</param>
        /// <param name="dateTo">До дата</param>
        /// <param name="id">Идентификатор на текушията запис</param>
        /// <returns></returns>
        Task<bool> IsExsistingMediatorFeeWithSamePeriod(int mediationTypeId, DateTime dateFrom, DateTime? dateTo, int? id);

        #endregion
    }
}
