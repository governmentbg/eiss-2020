// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IO.RegixClient.ServiceModels.RA;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Common.Mediation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IOWebApplication.Core.Services
{
    /// <summary>
    /// Допълнителни регистри за медиацията
    /// </summary>
    public class MediationCommonService : BaseService, IMediationCommonService
    {
        /// <summary>
        /// Допълнителни регистри за медиацията
        /// </summary>
        /// <param name="_logger"></param>
        /// <param name="_repo"></param>
        /// <param name="_userContext"></param>
        public MediationCommonService(ILogger<MediationCommonService> _logger,
                                      IRepository _repo,
                                      IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }

        #region Центрове за медиация

        /// <summary>
        /// Извличане на данни за центрове за медиация
        /// </summary>
        /// <param name="filter">Филтър</param>
        /// <returns></returns>
        public IQueryable<MediationCenterListDataVM> GetMediationCenters(MediationCenterFilterVM filter)
        {
            Expression<Func<MediationCenter, bool>> dateExpiredWhere = x => x.DateExpired == null;

            Expression<Func<MediationCenter, bool>> centerNameWhere = x => true;
            if (!string.IsNullOrEmpty(filter.CenterName))
                centerNameWhere = x => EF.Functions.ILike(x.Name, filter.CenterName.ToPaternSearch());

            return repo.AllReadonly<MediationCenter>()
                       .Where(dateExpiredWhere)
                       .Where(centerNameWhere)
                       .Select(x => new MediationCenterListDataVM()
                       {
                           Id = x.Id,
                           Name = x.Name,
                           AddressText = x.AddressText,
                           ContactDetails = x.ContactDetails,
                           DateFrom = x.DateFrom,
                           DateTo = x.DateTo,
                           ParentText = x.ParentId != null ? x.Parent.Name : null
                       });
        }

        /// <summary>
        /// Извличане на центрове за медиация за списък за избиране
        /// </summary>
        /// <param name="idSelected">Идентификаотр на текущият, за да се премахме от списъка</param>
        /// <param name="addDefaultElement">Дали да добави елемент "Избери"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetMediationCenterDDL(int? idSelected = null, bool addDefaultElement = true)
        {
            Expression<Func<MediationCenter, bool>> idSelectedWhere = x => true;
            if ((idSelected ?? 0) > 0)
                idSelectedWhere = x => x.Id != idSelected;

            List<SelectListItem> selectListItems = await repo.AllReadonly<MediationCenter>()
                                                             .Where(idSelectedWhere)
                                                             .Select(x => new SelectListItem()
                                                             {
                                                                 Text = x.Name,
                                                                 Value = x.Id.ToString()
                                                             })
                                                             .OrderBy(x => x.Text)
                                                             .ToListAsync();

            if (addDefaultElement)
            {
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                                                 .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Извличане на данни за редакция на център зая медиация
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<MediationCenterVM> GetMediationCenterEditById(int id)
        {
            return await repo.AllReadonly<MediationCenter>()
                             .Where(x => x.Id == id)
                             .Select(x => new MediationCenterVM()
                             {
                                 Id = x.Id,
                                 ParentId = x.ParentId,
                                 Name = x.Name,
                                 AddressText = x.AddressText,
                                 ContactDetails = x.ContactDetails,
                                 Description = x.Description,
                                 DateFrom = x.DateFrom,
                                 DateTo = x.DateTo,
                                 CourtIds = x.Courts.Select(c => c.CourtId.ToString()).ToArray()
                             })
                             .FirstAsync();
        }

        /// <summary>
        /// Попълване на обект за добавяне на център за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private MediationCenter FillMediationCenter(MediationCenterVM model)
        {
            return new()
            {
                ParentId = model.ParentId.NumberEmptyToNull(),
                Name = model.Name,
                AddressText = model.AddressText,
                ContactDetails = model.ContactDetails,
                Description = model.Description,
                DateFrom = model.DateFrom,
                DateTo = model.DateTo,
                UserId = userContext.UserId,
                DateWrt = DateTime.Now,
            };
        }

        /// <summary>
        /// Попълване на данни за редакция на център за медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <param name="modelSave">Модел за редакция</param>
        private static void SetEditFieldsMediationCenter(MediationCenterVM model, MediationCenter modelSave)
        {
            modelSave.ParentId = model.ParentId.NumberEmptyToNull();
            modelSave.Name = model.Name;
            modelSave.AddressText = model.AddressText;
            modelSave.ContactDetails = model.ContactDetails;
            modelSave.Description = model.Description;
            modelSave.DateFrom = model.DateFrom;
            modelSave.DateTo = model.DateTo;
        }

        /// <summary>
        /// Добавяне/редкация на данни за център зая медиация
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<int?> SaveMediationCenter(MediationCenterVM model)
        {
            try
            {
                MediationCenter modelSave = (model.Id > 0) ? await repo.All<MediationCenter>()
                                                                       .Where(x => x.Id == model.Id)
                                                                       .FirstAsync() : FillMediationCenter(model);

                if (model.Id > 0)
                {
                    repo.DeleteRange<MediationCenterCourt>(x => x.MediationCenterId == model.Id);
                    SetEditFieldsMediationCenter(model, modelSave);

                    if (model.CourtIds != null && model.CourtIds.Length > 0)
                        repo.AddRange(model.CourtIds.Select(x => new MediationCenterCourt() { MediationCenterId = model.Id, CourtId = int.Parse(x) }).ToList());
                }
                else
                {
                    if (model.CourtIds != null && model.CourtIds.Length > 0)
                        modelSave.Courts = model.CourtIds.Select(x => new MediationCenterCourt() { CourtId = int.Parse(x) }).ToList();

                    repo.Add(modelSave);
                }

                await repo.SaveChangesAsync();
                return modelSave.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на център зая медиация id = {model.Id}");
                return null;
            }
        }

        #endregion

        #region Медиатори

        /// <summary>
        /// Извличане на данни за медиатори
        /// </summary>
        /// <param name="filter">Филтър</param>
        /// <returns></returns>
        public IQueryable<MediationMediatorListDataVM> GetMediationMediators(MediationMediatorFilterVM filter)
        {
            Expression<Func<MediationMediator, bool>> dateExpiredWhere = x => x.DateExpired == null;

            Expression<Func<MediationMediator, bool>> mediatorNameWhere = x => true;
            if (!string.IsNullOrEmpty(filter.MediatorName))
                mediatorNameWhere = x => EF.Functions.ILike(x.Name, filter.MediatorName.ToPaternSearch());

            return repo.AllReadonly<MediationMediator>()
                       .Where(dateExpiredWhere)
                       .Where(mediatorNameWhere)
                       .Select(x => new MediationMediatorListDataVM()
                       {
                           Id = x.Id,
                           Name = x.Name,
                           DateFrom = x.DateFrom,
                           DateTo = x.DateTo,
                       });
        }

        /// <summary>
        /// Извличане на данни за редакция на медиатор
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<MediationMediatorVM> GetMediationMediatorEditById(int id)
        {
            return await repo.AllReadonly<MediationMediator>()
                             .Where(x => x.Id == id)
                             .Select(x => new MediationMediatorVM()
                             {
                                 Id = x.Id,
                                 Name = x.Name,
                                 AdditionalQualification = x.AdditionalQualification,
                                 MainProfession = x.MainProfession,
                                 PracticeSpecificAreaLawYear = x.PracticeSpecificAreaLawYear,
                                 ExperienceMediation = x.ExperienceMediation,
                                 Description = x.Description,
                                 DateFrom = x.DateFrom,
                                 DateTo = x.DateTo,
                                 DateEntry = x.DateEntry,
                                 Education = x.Education,
                                 CenterIds = x.Centers.Select(x => x.MediationCenterId.ToString()).ToArray()
                             })
                             .FirstAsync();
        }

        /// <summary>
        /// Попълване на обект за добавяне на медиатор
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private MediationMediator FillMediationMediator(MediationMediatorVM model)
        {
            return new()
            {
                Name = model.Name,
                AdditionalQualification = model.AdditionalQualification,
                MainProfession = model.MainProfession,
                PracticeSpecificAreaLawYear = model.PracticeSpecificAreaLawYear,
                ExperienceMediation = model.ExperienceMediation,
                Description = model.Description,
                DateFrom = model.DateFrom,
                DateTo = model.DateTo,
                DateEntry = model.DateEntry,
                Education = model.Education,
                UserId = userContext.UserId,
                DateWrt = DateTime.Now,
            };
        }

        /// <summary>
        /// Попълване на данни за редакция на медиатор
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <param name="modelSave">Модел за редакция</param>
        private static void SetEditFieldsMediationMediator(MediationMediatorVM model, MediationMediator modelSave)
        {
            modelSave.Name = model.Name;
            modelSave.AdditionalQualification = model.AdditionalQualification;
            modelSave.MainProfession = model.MainProfession;
            modelSave.PracticeSpecificAreaLawYear = model.PracticeSpecificAreaLawYear;
            modelSave.ExperienceMediation = model.ExperienceMediation;
            modelSave.Description = model.Description;
            modelSave.DateFrom = model.DateFrom;
            modelSave.DateTo = model.DateTo;
            modelSave.DateEntry = model.DateEntry;
            modelSave.Education = model.Education;
        }

        /// <summary>
        /// Метод връщащ център към медиатор
        /// </summary>
        /// <param name="mediationCenterId">Идентификатор на център</param>
        /// <param name="mediationMediatorId">Идентификатор на медиатор</param>
        /// <returns></returns>
        private MediationMediatorCenter GetMediationMediatorCenter(int mediationCenterId, int mediationMediatorId = 0)
        {
            return new()
            {
                MediationMediatorId = mediationMediatorId,
                MediationCenterId = mediationCenterId,
                DateFrom = DateTime.Now.AddYears(-100),
                UserId = userContext.UserId,
                DateWrt = DateTime.Now
            };
        }

        /// <summary>
        /// Добавяне/редкация на данни за медиатор
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<int?> SaveMediationMediator(MediationMediatorVM model)
        {
            try
            {
                MediationMediator modelSave = (model.Id > 0) ? await repo.All<MediationMediator>()
                                                                         .Where(x => x.Id == model.Id)
                                                                         .FirstAsync() : FillMediationMediator(model);

                if (model.Id > 0)
                {
                    repo.DeleteRange<MediationMediatorCenter>(x => x.MediationMediatorId == model.Id);
                    SetEditFieldsMediationMediator(model, modelSave);

                    if (model.CenterIds != null && model.CenterIds.Length > 0)
                        repo.AddRange(model.CenterIds.Select(x => GetMediationMediatorCenter(int.Parse(x), model.Id)).ToList());
                }
                else
                {
                    if (model.CenterIds != null && model.CenterIds.Length > 0)
                        modelSave.Centers = model.CenterIds.Select(x => GetMediationMediatorCenter(int.Parse(x))).ToList();

                    repo.Add(modelSave);
                }

                await repo.SaveChangesAsync();
                return modelSave.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на медиатор id = {model.Id}");
                return null;
            }
        }

        /// <summary>
        /// Where клауза за дата
        /// </summary>
        /// <param name="dateTime">Дата към която да показва медиаторите към делото</param>
        /// <returns></returns>
        private static Expression<Func<MediationMediator, bool>> GetMediationMediatorDateWhere(DateTime dateTime)
        {
            Expression<Func<MediationMediator, bool>> dateWhere = x => x.DateFrom <= dateTime &&
                                                                       (x.DateTo ?? dateTime) >= dateTime;

            return dateWhere;
        }

        /// <summary>
        /// Where клауза за дата
        /// </summary>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <returns></returns>
        private static Expression<Func<MediationMediator, bool>> GetMediationMediatorCourtWhere(int? courtId)
        {
            Expression<Func<MediationMediator, bool>> courtIdWhere = x => true;
            if ((courtId ?? 0) > 0)
                courtIdWhere = x => x.Centers
                                     .Any(c => c.Center
                                                .Courts
                                                .Any(r => r.CourtId == courtId));

            return courtIdWhere;
        }

        /// <summary>
        /// Извличане на медиатори за падащ списък
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="dateTimeTo">Към коя дата да се извлечат данни</param>
        /// <param name="addDefaultElement">Дали да добави елемент "Избери"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetMediationMediatorsDDL(int caseId, DateTime dateTimeTo, bool addDefaultElement = true)
        {
            int courtId = await repo.GetPropByIdAsync<Infrastructure.Data.Models.Cases.Case, int>(x => x.Id == caseId, x => x.CourtId);

            List<SelectListItem> selectListItems = await repo.AllReadonly<MediationMediator>()
                                                             .Where(GetMediationMediatorDateWhere(dateTimeTo))
                                                             .Where(x => x.Centers
                                                                          .Any(c => c.Center
                                                                                     .Courts
                                                                                     .Any(r => r.CourtId == courtId)))
                                                             .Select(x => new SelectListItem()
                                                             {
                                                                 Value = x.Id.ToString(),
                                                                 Text = x.Name
                                                             })
                                                             .ToListAsync();

            if (addDefaultElement)
            {
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                                                 .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Извличане на медиатори за падащ списък
        /// </summary>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <param name="dateTimeTo">Към коя дата да се извлечат данни</param>
        /// <param name="addDefaultElement">Дали да добави елемент "Избери"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_MediationMediatorsByCourt(int courtId, DateTime dateTimeTo, bool addDefaultElement = true)
        {
            List<SelectListItem> selectListItems = await repo.AllReadonly<MediationMediator>()
                                                             .Where(GetMediationMediatorDateWhere(dateTimeTo))
                                                             .Where(GetMediationMediatorCourtWhere(courtId))
                                                             .Select(x => new SelectListItem()
                                                             {
                                                                 Value = x.Id.ToString(),
                                                                 Text = x.Name
                                                             })
                                                             .ToListAsync();

            if (addDefaultElement)
            {
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                                                 .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Извличане на медиатори от центрове на които логнатият юзер е кординатор за падащ списък
        /// </summary>
        /// <param name="dateTimeTo">Към коя дата да се извлечат данни</param>
        /// <param name="addDefaultElement">Дали да добави елемент "Избери"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_MediationMediatorsByCoordinator(DateTime dateTimeTo, bool addDefaultElement = true)
        {
            List<SelectListItem> selectListItems = await repo.AllReadonly<MediationMediator>()
                                                             //.Where(GetMediationMediatorDateWhere(dateTimeTo))
                                                             .Where(x => x.Centers.Any(c => c.Center.Coordinators.Any(k => k.Coordinator.LawUnitId == userContext.LawUnitId)))
                                                             .Select(x => new SelectListItem()
                                                             {
                                                                 Value = x.Id.ToString(),
                                                                 Text = x.Name
                                                             })
                                                             .ToListAsync();

            if (addDefaultElement)
            {
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                                                 .ToList();
            }

            return selectListItems;
        }

        #endregion

        #region Координатори

        /// <summary>
        /// Извличане на данни за координатор
        /// </summary>
        /// <param name="filter">Филтър</param>
        /// <returns></returns>
        public IQueryable<MediationCoordinatorListDataVM> GetMediationCoordinators(MediationCoordinatorFilterVM filter)
        {
            Expression<Func<MediationCoordinator, bool>> dateExpiredWhere = x => x.DateExpired == null;

            Expression<Func<MediationCoordinator, bool>> coordinatorNameWhere = x => true;
            if (!string.IsNullOrEmpty(filter.CoordinatorName))
                coordinatorNameWhere = x => EF.Functions.ILike(x.LawUnit.FullName, filter.CoordinatorName.ToPaternSearch());

            return repo.AllReadonly<MediationCoordinator>()
                       .Where(dateExpiredWhere)
                       .Where(coordinatorNameWhere)
                       .Select(x => new MediationCoordinatorListDataVM()
                       {
                           Id = x.Id,
                           FullName = x.LawUnit.FullName,
                           Position = x.Position,
                           DateFrom = x.DateFrom,
                           DateTo = x.DateTo,
                       });
        }

        /// <summary>
        /// Извличане на данни за редакция на координатор
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<MediationCoordinatorVM> GetMediationCoordinatorEditById(int id)
        {
            return await repo.AllReadonly<MediationCoordinator>()
                             .Where(x => x.Id == id)
                             .Select(x => new MediationCoordinatorVM()
                             {
                                 Id = x.Id,
                                 LawUnitId = x.LawUnitId,
                                 Position = x.Position,
                                 Description = x.Description,
                                 DateFrom = x.DateFrom,
                                 DateTo = x.DateTo,
                                 CenterIds = x.Centers.Select(x => x.MediationCenterId.ToString()).ToArray()
                             })
                             .FirstAsync();
        }

        /// <summary>
        /// Попълване на обект за добавяне на координатор
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private MediationCoordinator FillMediationCoordinator(MediationCoordinatorVM model)
        {
            return new()
            {
                LawUnitId = model.LawUnitId,
                Position = model.Position,
                Description = model.Description,
                DateFrom = model.DateFrom,
                DateTo = model.DateTo,
                UserId = userContext.UserId,
                DateWrt = DateTime.Now,
            };
        }

        /// <summary>
        /// Попълване на данни за редакция на координатор
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <param name="modelSave">Модел за редакция</param>
        private static void SetEditFieldsMediationCoordinator(MediationCoordinatorVM model, MediationCoordinator modelSave)
        {
            modelSave.LawUnitId = model.LawUnitId;
            modelSave.Position = model.Position;
            modelSave.Description = model.Description;
            modelSave.DateFrom = model.DateFrom;
            modelSave.DateTo = model.DateTo;
        }

        /// <summary>
        /// Метод връщащ център към координатор
        /// </summary>
        /// <param name="mediationCenterId">Идентификатор на център</param>
        /// <param name="mediationCoordinatorId">Идентификатор на координатор</param>
        /// <returns></returns>
        private MediationCoordinatorCenter GetMediationCoordinatorCenter(int mediationCenterId, int mediationCoordinatorId = 0)
        {
            return new()
            {
                MediationCoordinatorId = mediationCoordinatorId,
                MediationCenterId = mediationCenterId,
                DateFrom = DateTime.Now.AddYears(-100),
                UserId = userContext.UserId,
                DateWrt = DateTime.Now
            };
        }

        /// <summary>
        /// Добавяне/редкация на данни за координатор
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<int?> SaveMediationCoordinator(MediationCoordinatorVM model)
        {
            try
            {
                MediationCoordinator modelSave = (model.Id > 0) ? await repo.All<MediationCoordinator>()
                                                                         .Where(x => x.Id == model.Id)
                                                                         .FirstAsync() : FillMediationCoordinator(model);

                if (model.Id > 0)
                {
                    repo.DeleteRange<MediationCoordinatorCenter>(x => x.MediationCoordinatorId == model.Id);
                    SetEditFieldsMediationCoordinator(model, modelSave);

                    if (model.CenterIds != null && model.CenterIds.Length > 0)
                        repo.AddRange(model.CenterIds.Select(x => GetMediationCoordinatorCenter(int.Parse(x), model.Id)).ToList());
                }
                else
                {
                    if (model.CenterIds != null && model.CenterIds.Length > 0)
                        modelSave.Centers = model.CenterIds.Select(x => GetMediationCoordinatorCenter(int.Parse(x))).ToList();

                    repo.Add(modelSave);
                }

                await repo.SaveChangesAsync();
                return modelSave.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на координатор id = {model.Id}");
                return null;
            }
        }

        /// <summary>
        /// Метод извличащ съдилища към кординатор
        /// </summary>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент "Избери"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_MediationCoordinatorCourt(bool addDefaultElement = true)
        {
            List<SelectListItem> selectListItems = await repo.AllReadonly<MediationCenterCourt>()
                                                             .Where(x => x.MediationCenter.Coordinators.Any(c => c.Coordinator.LawUnitId == userContext.LawUnitId))
                                                             .Select(x => new SelectListItem
                                                             {
                                                                 Text = x.Court.Label,
                                                                 Value = x.Court.Id.ToString()
                                                             })
                                                             .ToListAsync() ?? [];

            if (selectListItems.Count < 1)
            {
                selectListItems.Add(new SelectListItem { Value = userContext.CourtId.ToString(), Text = userContext.CourtName });
            }

            if (selectListItems.Count != 1)
            {
                if (addDefaultElement)
                {
                    selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                                                     .ToList();
                }
            }

            return selectListItems;
        }

        #endregion

        #region Ставки за заплащане на медиатор

        /// <summary>
        /// Извличане на данни за ставки за заплащане на медиатори
        /// </summary>
        /// <param name="filter">Филтър</param>
        /// <returns></returns>
        public IQueryable<MediatorFeeListDataVM> GetMediatorFees(MediatorFeeFilterVM filter)
        {
            return repo.AllReadonly<MediatorFee>()
                       .Select(x => new MediatorFeeListDataVM()
                       {
                           Id = x.Id,
                           MediationTypeLabel = x.MediationType.Label,
                           HourFee = x.HourFee.ToString("0.00"),
                           HourFeeEUR = x.HourFeeEUR.ToString("0.00"),
                           DateFrom = x.DateFrom,
                           DateTo = x.DateTo,
                       });
        }

        /// <summary>
        /// Извличане на данни за редакция на ставка за заплащане на медиатори
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<MediatorFeeVM> GetMediatorFeeEditById(int id)
        {
            return await repo.AllReadonly<MediatorFee>()
                             .Where(x => x.Id == id)
                             .Select(x => new MediatorFeeVM()
                             {
                                 Id = x.Id,
                                 MediationTypeId = x.MediationTypeId,
                                 HourFee = x.HourFee,
                                 HourFeeEUR = x.HourFeeEUR,
                                 DateFrom = x.DateFrom,
                                 DateTo = x.DateTo,
                             })
                             .FirstAsync();
        }

        /// <summary>
        /// Попълване на обект за добавяне на ставка за заплащане на медиатори
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private MediatorFee FillMediatorFee(MediatorFeeVM model)
        {
            return new()
            {
                MediationTypeId = model.MediationTypeId,
                HourFee = model.HourFee,
                HourFeeEUR = model.HourFeeEUR,
                DateFrom = model.DateFrom,
                DateTo = model.DateTo,
                UserId = userContext.UserId,
                DateWrt = DateTime.Now,
            };
        }

        /// <summary>
        /// Попълване на данни за редакция на ставка за заплащане на медиатори
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <param name="modelSave">Модел за редакция</param>
        private static void SetEditFieldsMediatorFee(MediatorFeeVM model, MediatorFee modelSave)
        {
            modelSave.MediationTypeId = model.MediationTypeId;
            modelSave.HourFee = model.HourFee;
            modelSave.HourFeeEUR = model.HourFeeEUR;
            modelSave.DateFrom = model.DateFrom;
            modelSave.DateTo = model.DateTo;
        }

        /// <summary>
        /// Добавяне/редкация на данни за ставка за заплащане на медиатори
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<int?> SaveMediatorFee(MediatorFeeVM model)
        {
            try
            {
                MediatorFee modelSave = (model.Id > 0) ? await repo.All<MediatorFee>()
                                                                   .Where(x => x.Id == model.Id)
                                                                   .FirstAsync() : FillMediatorFee(model);

                if (model.Id > 0)
                    SetEditFieldsMediatorFee(model, modelSave);
                else
                    repo.Add(modelSave);

                await repo.SaveChangesAsync();
                return modelSave.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на ставка за заплащане на медиатори id = {model.Id}");
                return null;
            }
        }

        /// <summary>
        /// Проверка за съществуващ период в ставките за заплащане на медиатори
        /// </summary>
        /// <param name="mediationTypeId">Идентификатор на вид среща за медиация</param>
        /// <param name="dateFrom">От дата</param>
        /// <param name="dateTo">До дата</param>
        /// <param name="id">Идентификатор на текушията запис</param>
        /// <returns></returns>
        public async Task<bool> IsExsistingMediatorFeeWithSamePeriod(int mediationTypeId, DateTime dateFrom, DateTime? dateTo, int? id)
        {
            DateTime dateTime = DateTime.Now.AddYears(100);

            Expression<Func<MediatorFee, bool>> idWhere = x => true;
            if ((id ?? 0) > 0)
                idWhere = x => x.Id != id;

            return await repo.AllReadonly<MediatorFee>()
                             .Where(idWhere)
                             .Where(x => x.MediationTypeId == mediationTypeId)
                             .AnyAsync(x => x.DateFrom <= (dateTo ?? dateTime) &&
                                            (x.DateTo ?? dateTime) >= dateFrom);
        }

        #endregion
    }
}
