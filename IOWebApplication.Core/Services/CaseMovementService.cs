// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using IOWebApplication.Infrastructure.Data.Models.Common;

namespace IOWebApplication.Core.Services
{
    public class CaseMovementService : BaseService, ICaseMovementService
    {
        private readonly ICommonService commonService;
        private readonly IUrlHelper urlHelper;
        public CaseMovementService(
        ILogger<CaseMovementService> _logger,
        IRepository _repo,
        IUserContext _userContext,
        ICommonService _commonService,
        IUrlHelper _url)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            commonService = _commonService;
            urlHelper = _url;
        }

        /// <summary>
        /// Извличане на данни за местоположение
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public IEnumerable<CaseMovementVM> Select(int CaseId)
        {
            var caseMovementVMs = repo.AllReadonly<CaseMovement>()
                .Include(x => x.Case)
                .Include(x => x.MovementType)
                .Include(x => x.ToUser)
                .ThenInclude(x => x.LawUnit)
                .Include(x => x.CourtOrganization)
                .Include(x => x.AcceptUser)
                .ThenInclude(x => x.LawUnit)
                .Include(x => x.User)
                .ThenInclude(x => x.LawUnit)
                .Where(x => ((CaseId > 0) ? (x.CaseId == CaseId) : true) &&
                            x.IsActive)
                .OrderByDescending(x => x.DateSend)
                .Take(10)
                .Select(x => new CaseMovementVM()
                {
                    Id = x.Id,
                    CaseId = x.CaseId,
                    CourtId = x.CourtId,
                    MovementTypeId = x.MovementTypeId,
                    MovementTypeLabel = (x.MovementType != null) ? x.MovementType.Label : string.Empty,
                    NameFor = ((x.MovementTypeId == NomenclatureConstants.CaseMovementType.ToPerson) ? ((x.ToUser != null) ? x.ToUser.LawUnit.FullName : string.Empty) : ((x.MovementTypeId == NomenclatureConstants.CaseMovementType.ToOtdel) ? ((x.CourtOrganization != null) ? x.CourtOrganization.Label : string.Empty) : x.OtherInstitution)),
                    ToUserId = x.ToUserId,
                    CourtOrganizationId = x.CourtOrganizationId,
                    OtherInstitution = x.OtherInstitution,
                    DateSend = x.DateSend,
                    DateAccept = x.DateAccept,
                    Description = x.Description,
                    DisableDescription = x.DisableDescription,
                    AcceptDescription = x.AcceptDescription,
                    IsActive = x.IsActive,
                    IsActiveText = ((x.IsActive) ? "Активен" : "Неактивен"),
                    IsEdit = false,
                    IsAccept = false,
                    AcceptUserId = x.AcceptUserId,
                    AcceptLawUnitName = (x.AcceptUser != null) ? x.AcceptUser.LawUnit.FullName : string.Empty,
                    UserId = x.UserId,
                    UserLawUnitId = x.User.LawUnitId,
                    UserLawUnitName = x.User.LawUnit.FullName
                }).ToList();

            SetEditAccept(caseMovementVMs);
            return caseMovementVMs;
        }

        /// <summary>
        /// Сетване на права за местоположение
        /// </summary>
        /// <param name="caseMovementVMs"></param>
        private void SetEditAccept(IEnumerable<CaseMovementVM> caseMovementVMs)
        {
            var maxIdElement = caseMovementVMs.Where(x => x.IsActive).OrderByDescending(x => x.Id).FirstOrDefault();
            if (maxIdElement != null)
            {
                maxIdElement.IsEdit = IsEdit(maxIdElement);
                maxIdElement.IsAccept = IsAccept(maxIdElement);
                maxIdElement.IsEditAccept = IsEditAccept(maxIdElement);
            }
        }

        /// <summary>
        /// Метод за проверка дали може да бъде редактирано движение
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool IsEdit(CaseMovementVM model)
        {
            if (model.UserId != userContext.UserId)
                return false;

            if (model.DateAccept != null)
                return false;

            if (!model.IsActive)
                return false;

            return true;
        }

        /// <summary>
        /// Метод за проверка дали може да бъде редактирано приемане на движение
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool IsEditAccept(CaseMovementVM model)
        {
            if (model.DateAccept == null)
                return false;

            if (model.AcceptUserId != userContext.UserId)
                return false;

            return true;
        }

        /// <summary>
        /// Метод за проверка дали може да бъде прието движение
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool IsAccept(CaseMovementVM model)
        {
            if (model.DateAccept != null)
                return false;

            if (model.MovementTypeId == NomenclatureConstants.CaseMovementType.ToPerson)
            {
                if (model.ToUserId != userContext.UserId)
                    return false;
            }
            else
            {
                if (model.MovementTypeId == NomenclatureConstants.CaseMovementType.ToOtdel)
                {
                    var lawUnits = commonService.LawUnit_ByCourtDate(userContext.CourtId, DateTime.Now, model.CourtOrganizationId ?? 0);
                    if (!lawUnits.Any(x => x.Id == userContext.LawUnitId))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Метод за създаване на местоположение
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CreateMovement(CaseMovementVM model)
        {
            try
            {
                model.CourtOrganizationId = model.CourtOrganizationId.EmptyToNull();

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseMovement>(model.Id);
                    saved.MovementTypeId = model.MovementTypeId;
                    saved.ToUserId = model.ToUserId;
                    saved.CourtOrganizationId = model.CourtOrganizationId;
                    saved.OtherInstitution = model.OtherInstitution;
                    //saved.DateSend = model.DateSend;
                    saved.Description = model.Description;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    var saved = new CaseMovement();
                    saved.CaseId = model.CaseId;
                    saved.CourtId = model.CourtId;
                    saved.MovementTypeId = model.MovementTypeId;

                    if (saved.MovementTypeId == NomenclatureConstants.CaseMovementType.ToOutStructure)
                    {
                        saved.DateAccept = DateTime.Now;
                        saved.AcceptUserId = userContext.UserId;
                    }

                    saved.ToUserId = model.ToUserId;
                    saved.CourtOrganizationId = model.CourtOrganizationId;
                    saved.OtherInstitution = model.OtherInstitution;
                    saved.DateSend = DateTime.Now;
                    saved.Description = model.Description;
                    saved.IsActive = true;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    repo.Add<CaseMovement>(saved);
                    repo.SaveChanges();
                    model.Id = saved.Id;
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на движение по дело Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни по ид за местоположение
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public CaseMovementVM GetById_CaseMovementVM(int Id)
        {
            return repo.AllReadonly<CaseMovement>()
                .Include(x => x.Case)
                .Include(x => x.MovementType)
                .Include(x => x.ToUser)
                .ThenInclude(x => x.LawUnit)
                .Include(x => x.CourtOrganization)
                .Include(x => x.AcceptUser)
                .ThenInclude(x => x.LawUnit)
                .Include(x => x.User)
                .ThenInclude(x => x.LawUnit)
                .Where(x => x.Id == Id)
                .Select(x => new CaseMovementVM()
                {
                    Id = x.Id,
                    CaseId = x.CaseId,
                    CourtId = x.CourtId,
                    MovementTypeId = x.MovementTypeId,
                    MovementTypeLabel = (x.MovementType != null) ? x.MovementType.Label : string.Empty,
                    NameFor = ((x.MovementTypeId == NomenclatureConstants.CaseMovementType.ToPerson) ? ((x.ToUser != null) ? x.ToUser.LawUnit.FullName : string.Empty) : ((x.MovementTypeId == NomenclatureConstants.CaseMovementType.ToOtdel) ? ((x.CourtOrganization != null) ? x.CourtOrganization.Label : string.Empty) : x.OtherInstitution)),
                    ToUserId = x.ToUserId,
                    CourtOrganizationId = x.CourtOrganizationId,
                    OtherInstitution = x.OtherInstitution,
                    DateSend = x.DateSend,
                    DateAccept = x.DateAccept,
                    Description = x.Description,
                    DisableDescription = x.DisableDescription,
                    AcceptDescription = x.AcceptDescription,
                    IsActive = x.IsActive,
                    IsActiveText = (x.IsActive) ? MessageConstant.Yes : MessageConstant.No,
                    IsEdit = false,
                    IsAccept = false,
                    AcceptUserId = x.AcceptUserId,
                    AcceptLawUnitName = (x.AcceptUser != null) ? x.AcceptUser.LawUnit.FullName : string.Empty,
                    UserId = x.UserId,
                    UserLawUnitId = x.User.LawUnitId,
                    UserLawUnitName = x.User.LawUnit.FullName
                }).FirstOrDefault();
        }

        /// <summary>
        /// Сторно на местоположение
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool StornoMovement(CaseMovementVM model)
        {
            try
            {
                model.CourtOrganizationId = model.CourtOrganizationId.EmptyToNull();

                var saved = repo.GetById<CaseMovement>(model.Id);
                saved.DisableDescription = model.DisableDescription;
                saved.IsActive = false;
                repo.Update(saved);
                repo.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при сторниране на движение по дело Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Приемане на местоположение
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public bool AcceptMovement(int Id)
        {
            var saved = repo.GetById<CaseMovement>(Id);
            if (saved.DateAccept != null)
                return false;

            try
            {
                saved.CourtOrganizationId = saved.CourtOrganizationId.EmptyToNull();
                saved.DateAccept = DateTime.Now;
                saved.AcceptUserId = userContext.UserId;
                repo.Update(saved);
                repo.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при приемане на движение по дело Id={ saved.Id }");
                return false;
            };
        }

        /// <summary>
        /// Редакция на приемане на местоположение
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool EditAcceptMovement(CaseMovementVM model)
        {
            try
            {
                model.CourtOrganizationId = model.CourtOrganizationId.EmptyToNull();

                var saved = repo.GetById<CaseMovement>(model.Id);
                saved.AcceptDescription = model.AcceptDescription;
                repo.Update(saved);
                repo.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при отразяване на изпълнение на движение по дело Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Проверка дали може да се добави местоположение
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public bool IsAddNewMovement(int CaseId)
        {
            var movements = repo.AllReadonly<CaseMovement>().Where(x => x.CaseId == CaseId).ToList();

            if (movements.Count < 1)
                return true;
            else
            {
                var maxIdElement = movements.Where(x => x.IsActive).OrderByDescending(x => x.Id).FirstOrDefault();

                if (maxIdElement != null)
                {
                    if (maxIdElement.DateAccept == null)
                        return false;

                    if (maxIdElement.AcceptUserId != userContext.UserId)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Създаване на обратно действие за местоположение
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public int CreateReturnMovement(int Id)
        {
            try
            {
                var movement = GetById_CaseMovementVM(Id);

                var saved = new CaseMovement()
                {
                    CaseId = movement.CaseId,
                    CourtId = movement.CourtId,
                    MovementTypeId = NomenclatureConstants.CaseMovementType.ToPerson,
                    OtherInstitution = ((movement.MovementTypeId == NomenclatureConstants.CaseMovementType.ToOutStructure) ? movement.OtherInstitution : string.Empty),
                    ToUserId = movement.UserId,
                    DateSend = DateTime.Now,
                    Description = "Автоматично обратно връщане",
                    IsActive = true,
                    DateWrt = DateTime.Now,
                    UserId = userContext.UserId
                };
                
                repo.Add<CaseMovement>(saved);
                repo.SaveChanges();
                return saved.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на обратно връщане на движение по дело Id={ Id }");
                return -1;
            }
        }

        /// <summary>
        /// Извличане на данни за местоположение за начален екран
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CaseMovementVM> Select_ToDo()
        {
            var courtLawUnit = repo.AllReadonly<CourtLawUnit>()
                        .Where(x => x.CourtId == userContext.CourtId &&
                                    x.LawUnitId == userContext.LawUnitId &&
                                    (x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.Now.AddDays(1)) >= DateTime.Now))
                        .FirstOrDefault();
            var courtOrganizationId = (courtLawUnit != null) ? (courtLawUnit.CourtOrganizationId ?? 0) : 0;

            var caseMovementVMs = repo.AllReadonly<CaseMovement>()
                .Include(x => x.Case)
                .Include(x => x.MovementType)
                .Include(x => x.ToUser)
                .ThenInclude(x => x.LawUnit)
                .Include(x => x.CourtOrganization)
                .Include(x => x.AcceptUser)
                .ThenInclude(x => x.LawUnit)
                .Include(x => x.User)
                .ThenInclude(x => x.LawUnit)
                .Where(x => ((courtOrganizationId > 0) ? ((x.ToUserId == userContext.UserId) || (x.CourtOrganizationId == courtOrganizationId)) : (x.ToUserId == userContext.UserId)) &&
                            (x.Case.CourtId == userContext.CourtId) &&
                            (x.DateAccept == null))
                .Select(x => new CaseMovementVM()
                {
                    Id = x.Id,
                    CaseId = x.CaseId,
                    CourtId = x.CourtId,
                    CaseName = x.Case.RegNumber + "/" + x.Case.RegDate.ToString("dd.MM.yyyy"),
                    MovementTypeId = x.MovementTypeId,
                    MovementTypeLabel = (x.MovementType != null) ? x.MovementType.Label : string.Empty,
                    NameFor = ((x.MovementTypeId == NomenclatureConstants.CaseMovementType.ToPerson) ? ((x.ToUser.LawUnit != null) ? x.ToUser.LawUnit.FullName : string.Empty) : ((x.MovementTypeId == NomenclatureConstants.CaseMovementType.ToOtdel) ? ((x.CourtOrganization != null) ? x.CourtOrganization.Label : string.Empty) : x.OtherInstitution)),
                    ToUserId = x.ToUserId,
                    CourtOrganizationId = x.CourtOrganizationId,
                    OtherInstitution = x.OtherInstitution,
                    DateSend = x.DateSend,
                    DateAccept = x.DateAccept,
                    Description = x.Description,
                    DisableDescription = x.DisableDescription,
                    AcceptDescription = x.AcceptDescription,
                    IsActive = x.IsActive,
                    IsActiveText = ((x.IsActive) ? "Активен" : "Неактивен"),
                    IsEdit = false,
                    IsAccept = false,
                    AcceptUserId = x.AcceptUserId,
                    AcceptLawUnitName = (x.AcceptUser != null) ? x.AcceptUser.LawUnit.FullName : string.Empty,
                    UserId = x.UserId,
                    UserLawUnitId = x.User.LawUnitId,
                    UserLawUnitName = x.User.LawUnit.FullName
                }).ToList();

            List<CaseMovementVM> _result = new List<CaseMovementVM>();
            foreach (var movementVM in caseMovementVMs)
            {
                var movementVMs = Select(movementVM.CaseId);
                var maxIdElement = movementVMs.Where(x => x.IsActive).OrderByDescending(x => x.Id).FirstOrDefault();
                if (movementVM.Id == maxIdElement?.Id)
                {
                    movementVM.ViewUrl = urlHelper.Action("Index", "CaseMovement", new { CaseId = movementVM.CaseId });
                    _result.Add(movementVM);
                }
            }

            return _result;
        }

        /// <summary>
        /// Извличанена бройки за начален екран
        /// </summary>
        /// <returns></returns>
        public int Select_ToDoCount()
        {
            return Select_ToDo().Count();
        }

        /// <summary>
        /// Справка за местоположение
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="CaseRegNum"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public IQueryable<CaseMovementVM> Select_Spr(int courtId, string CaseRegNum, string UserId)
        {
            var caseMovementVMs = repo.AllReadonly<CaseMovement>()
                .Include(x => x.Case)
                .Include(x => x.MovementType)
                .Include(x => x.ToUser)
                .ThenInclude(x => x.LawUnit)
                .Include(x => x.CourtOrganization)
                .Include(x => x.AcceptUser)
                .ThenInclude(x => x.LawUnit)
                .Include(x => x.User)
                .ThenInclude(x => x.LawUnit)
                .Where(x => x.Case.CourtId == courtId &&
                            x.Case.RegNumber.Contains(CaseRegNum ?? x.Case.RegNumber) &&
                            (((UserId ?? string.Empty) != string.Empty) ? ((x.AcceptUserId == UserId) || (x.ToUserId == UserId)) : true))
                .Where(x => (x.Case.CourtId == userContext.CourtId))
                .OrderByDescending(x => x.Case.Id)
                .ThenByDescending(x => x.DateSend)
                .Select(x => new CaseMovementVM()
                {
                    Id = x.Id,
                    CaseId = x.CaseId,
                    CourtId = x.CourtId,
                    CaseName = x.Case.RegNumber,
                    MovementTypeId = x.MovementTypeId,
                    MovementTypeLabel = (x.MovementType != null) ? x.MovementType.Label : string.Empty,
                    NameFor = ((x.MovementTypeId == NomenclatureConstants.CaseMovementType.ToPerson) ? ((x.ToUser != null) ? x.ToUser.LawUnit.FullName : string.Empty) : 
                                                                                                       ((x.MovementTypeId == NomenclatureConstants.CaseMovementType.ToOtdel) ? ((x.CourtOrganization != null) ? x.CourtOrganization.Label : string.Empty) : x.OtherInstitution)),
                    ToUserId = x.ToUserId,
                    CourtOrganizationId = x.CourtOrganizationId,
                    OtherInstitution = x.OtherInstitution,
                    DateSend = x.DateSend,
                    DateAccept = x.DateAccept,
                    Description = x.Description,
                    DisableDescription = x.DisableDescription,
                    AcceptDescription = x.AcceptDescription,
                    IsActive = x.IsActive,
                    IsActiveText = ((x.IsActive) ? "Активен" : "Неактивен"),
                    IsEdit = false,
                    IsAccept = false,
                    AcceptUserId = x.AcceptUserId,
                    AcceptLawUnitName = (x.AcceptUser != null) ? x.AcceptUser.LawUnit.FullName + (x.MovementTypeId == NomenclatureConstants.CaseMovementType.ToOutStructure ? " (" + x.OtherInstitution + (!string.IsNullOrEmpty(x.Description) ? " - " + x.Description : string.Empty) + ")" : string.Empty) : string.Empty,
                    UserId = x.UserId,
                    UserLawUnitId = x.User.LawUnitId,
                    UserLawUnitName = x.User.LawUnit.FullName + (x.MovementTypeId != NomenclatureConstants.CaseMovementType.ToOutStructure ? (!string.IsNullOrEmpty(x.OtherInstitution) ? " (" + x.OtherInstitution + ")" : string.Empty) : string.Empty)
                }).ToList();

            return caseMovementVMs.AsQueryable();
        }

        /// <summary>
        /// Извличане на последно местоположение за дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public string GetLastMovmentForCaseId(int CaseId)
        {
            var caseMovment = Select((int)CaseId).Where(x => x.IsActive).OrderByDescending(x => x.Id).FirstOrDefault();
            var result = string.Empty;

            if (caseMovment != null)
                result = "Вид: " + caseMovment.MovementTypeLabel + " - насочено към: " + caseMovment.NameFor + " - " + ((caseMovment.DateAccept != null) ? "Прието" : "Неприето");

            return result;
        }
    }
}
