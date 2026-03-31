// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    /// <summary>
    /// Медиация
    /// </summary>
    public interface IMediationService : IBaseService
    {
        #region Работа с дела за медиация

        /// <summary>
        /// Метод извличащ данни за дела подлежащи на медиация
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        Task<IQueryable<MediationCaseListDataVM>> GetMediationCases(MediationCaseFilterVM filter);

        /// <summary>
        /// Метод извличащ данни за дела подлежащи на медиация за падащ списък
        /// </summary>
        /// <param name="caseId">Идентификатор на текущо дело</param>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент "Избери"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDDL_MediationCases(int caseId, bool addDefaultElement = true);

        /// <summary>
        /// Извличане на данни за дело подлежащо на медиация
        /// </summary>
        /// <param name="id">Идентификатор на дело</param>
        /// <returns></returns>
        Task<MediationCasePreviewVM> GetMediationCasePreview(int id);

        /// <summary>
        /// Метод извличащ данни свързани с медиация от дело за редакция
        /// </summary>
        /// <param name="id">Идентификатор на дело</param>
        /// <returns></returns>
        Task<MediationCaseEditVM> GetMediationCaseEditById(int id);

        /// <summary>
        /// Редкация на данни от дело подлежащо на медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<int?> SaveMediationCase(MediationCaseEditVM model);

        /// <summary>
        /// Метод който връща списък с подшифри по шифър взет от дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент "Избери"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDDL_CaseCodeSub(int caseId, bool addDefaultElement = true);

        /// <summary>
        /// Метод за проверка за съществуваща среща в дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        Task<bool> IsExistMediationCaseSession(int caseId);

        #endregion

        #region Медиатори

        /// <summary>
        /// Метод извличащ данни за медиатори към дело
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<MediationCaseMediatorListDataVM> GetMediationCaseMediators(MediationCaseMediatorFilterVM filter);

        /// <summary>
        /// Метод извличащ данни за медиатори към дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент "Избери"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDDL_MediationCaseMediators(int caseId, bool addDefaultElement = true);

        /// <summary>
        /// Извличане на данни за медиатори към дело
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        Task<MediationCaseMediatorVM> GetMediationCaseMediatorEditById(int id);

        /// <summary>
        /// Добавяне/редкация на данни за медиатори към дело
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<int?> SaveMediationCaseMediator(MediationCaseMediatorVM model);

        /// <summary>
        /// Проверка за медиатор, дали съществува в дело
        /// </summary>
        /// <param name="mediatorId">Идентификатор на медиатор</param>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="id">Идентификатор на запис</param>
        /// <returns></returns>
        Task<bool> IsExistMediatorInCase(int mediatorId, int caseId, int? id = null);

        #endregion

        #region Срещи

        /// <summary>
        /// Метод извличащ данни за срещи към дело
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<MediationCaseSessionListDataVM> GetMediationCaseSessions(MediationCaseSessionFilterVM filter);

        /// <summary>
        /// Извличане на данни за среща към дело
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        Task<MediationCaseSessionVM> GetMediationCaseSessionEditById(int id);

        /// <summary>
        /// Извличане данни за навигация от среща
        /// </summary>
        /// <param name="id">Идентификатор на среща</param>
        /// <returns></returns>
        Task<MediationCaseNavigationVM> GetSessionDataNavigation(int id);

        /// <summary>
        /// Добавяне/редкация на данни за среща към дело
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<int?> SaveMediationCaseSession(MediationCaseSessionVM model);

        /// <summary>
        /// Извличане на данни за среща за медиация
        /// </summary>
        /// <param name="id">Идентификатор на среща</param>
        /// <returns></returns>
        Task<MediationSessionPreviewVM> GetMediationSessionPreview(int id);

        /// <summary>
        /// Метод проверяващ за съществуване на насрочена среща
        /// </summary>
        /// <param name="dateFrom">От дата на нова среща</param>
        /// <param name="dateTo">До дата на нова среща</param>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="mediationSessionId">Идентификатор на среща</param>
        /// <returns></returns>
        Task<bool> IsExsitMediationSession(DateTime dateFrom, DateTime dateTo, int caseId, int? mediationSessionId);

        #endregion

        #region Лица в среща

        /// <summary>
        /// Метод извличащ данни за лица към среща
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <param name="start">От коя позиция да дръпне данните</param>
        /// <param name="length">Дължина</param>
        /// <param name="sortedColumns">Колони по които се сортира</param>
        /// <returns></returns>
        Task<DataTableResponseVM<MediationCasePersonListDataVM>> GetMediationCasePeople(MediationCasePersonFilterVM filter, int start, int length, List<DataTablesSortColumnVM> sortedColumns);

        /// <summary>
        /// Извличане на данни за лице от среща за медиация
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        Task<MediationCasePersonVM> GetMediationCasePersonEditById(int id);

        /// <summary>
        /// Редкация на данни за лице от среща за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<int?> SaveMediationCasePerson(MediationCasePersonVM model);

        /// <summary>
        /// Метод извличащ данни за страни от дело в среща
        /// </summary>
        /// <param name="mediationCaseSessionId">Идентификатор на среща за медиация</param>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент "Избери"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDDL_MediationCasePerson(int mediationCaseSessionId, bool addDefaultElement = true);

        #endregion

        #region Резултати в среща за медиация

        /// <summary>
        /// Метод извличащ данни за резултати в среща за медиация
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<MediationCaseSessionResultListDataVM> GetMediationCaseSessionResults(MediationCaseSessionResultFilterVM filter);

        /// <summary>
        /// Извличане на данни за резултат в среща за медиация
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        Task<MediationCaseSessionResultVM> GetMediationCaseSessionResultEditById(int id);

        /// <summary>
        /// Добавяне/редкация на данни за резултат в среща за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<int?> SaveMediationCaseSessionResult(MediationCaseSessionResultVM model);

        #endregion

        #region Документи към среща за медиация

        /// <summary>
        /// Метод извличащ данни за документи в среща за медиация
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<MediationCaseSessionDocumentListDataVM> GetMediationCaseSessionDocuments(MediationCaseSessionDocumentFilterVM filter);

        /// <summary>
        /// Извличане на данни за документ в среща за медиация
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        Task<MediationCaseSessionDocumentVM> GetMediationCaseSessionDocumentEditById(int id);

        /// <summary>
        /// Добавяне/редкация на данни за документ в среща за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<int?> SaveMediationCaseSessionDocument(MediationCaseSessionDocumentVM model);

        #endregion

        #region Оценка

        /// <summary>
        /// Метод извличащ данни за оценка към дело
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<MediationCaseMediatorAppraisalListDataVM> GetMediationCaseMediatorAppraisals(MediationCaseMediatorAppraisalFilterVM filter);

        /// <summary>
        /// Извличане на данни за оценка в среща за медиация
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        Task<MediationCaseMediatorAppraisalVM> GetMediationCaseMediatorAppraisalEditById(int id);

        /// <summary>
        /// Добавяне/редкация на данни за оценка в среща за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<int?> SaveMediationCaseMediatorAppraisal(MediationCaseMediatorAppraisalVM model);

        /// <summary>
        /// Метод за проверка на медиатор в среща дали има оценка
        /// </summary>
        /// <param name="mediatorId">Идентификатор на медиатор</param>
        /// <param name="mediationCaseSessionId">Идентификатор на среща</param>
        /// <param name="appraisalId">Идентификатор на текущ запис</param>
        /// <returns></returns>
        Task<bool> IsExistAppraisalMediator(int mediatorId, int mediationCaseSessionId, int? appraisalId);

        #endregion

        #region Заплащане на медиатори

        /// <summary>
        /// Метод извличащ данни за суми на заседатели
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<MediationObligationListDataVM> GetMediationObligations(MediationObligationFilterVM filter);

        /// <summary>
        /// Извличане на данни за сума на заседател за редакция
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        Task<MediationObligationVM> GetMediationObligationEditById(int id);

        /// <summary>
        /// Редкация на данни за сума на заседател към среща
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<int?> SaveMediationObligation(MediationObligationVM model);

        #endregion
    }
}
