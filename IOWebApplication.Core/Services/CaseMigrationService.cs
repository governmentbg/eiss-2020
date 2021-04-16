using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;

namespace IOWebApplication.Core.Services
{
    public class CaseMigrationService : BaseService, ICaseMigrationService
    {

        public CaseMigrationService(ILogger<CaseMigrationService> _logger,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }

        /// <summary>
        /// Извличане на данни за Вертикално движение на дело - между институциите
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IQueryable<CaseMigrationVM> Select(int caseId)
        {
            int[] initCaseIds = get_InitialCases(caseId);

            return repo.AllReadonly<CaseMigration>()
                            .Include(x => x.Case)
                            .ThenInclude(x => x.Court)
                            .Include(x => x.PriorCase)
                            .ThenInclude(x => x.Court)
                            .Include(x => x.CaseMigrationType)
                            .Include(x => x.SendToCourt)
                            .Include(x => x.SendToInstitution)
                            .Include(x => x.InCaseMigrations)
                            .Include(x => x.CaseSessionAct)
                            .ThenInclude(x => x.ActType)
                            .Include(x => x.Case.CaseState)
                            .Where(x => initCaseIds.Contains(x.InitialCaseId) && x.Case.CaseStateId != NomenclatureConstants.CaseState.Deleted)
                            .OrderBy(x => x.Id)
                            .Select(x => new CaseMigrationVM
                            {
                                Id = x.Id,
                                InitialCaseId = x.InitialCaseId,
                                CaseId = x.CaseId,
                                CaseRegNumber = x.Case.RegNumber,
                                CaseRegDate = x.Case.RegDate,
                                CaseSessionAct = (x.CaseSessionAct != null) ? $"{x.CaseSessionAct.ActType.Label} {x.CaseSessionAct.RegNumber}/{x.CaseSessionAct.RegDate:dd.MM.yyyy}" : "",
                                CaseCourtName = x.Case.Court.Label,
                                MigrationDirection = x.CaseMigrationType.MigrationDirection,
                                MigrationTypeId = x.CaseMigrationTypeId,
                                MigrationTypeName = x.CaseMigrationType.Label,
                                SentFromName = (x.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing) ? x.Case.Court.Label : x.PriorCase.Court.Label,
                                SentToName = (x.SendToCourt != null) ? x.SendToCourt.Label : (x.SendToInstitution != null ? x.SendToInstitution.FullName : ""),
                                SendToCortId = x.SendToCourtId,
                                Description = x.Description,
                                CanEdit = x.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing && x.CaseId == caseId && !x.InCaseMigrations.Any(),
                                CanAccept = x.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing && !x.InCaseMigrations.Any() && (x.SendToCourtId == userContext.CourtId || CaseMigrationTypes.SendCaseTypesCanAccept.Contains(x.CaseMigrationTypeId)),
                                DateWrt = x.DateWrt,
                                CaseStateName = x.Case.CaseState.Label
                            }).AsQueryable();
        }

        public IQueryable<CaseMigrationVM> SelectOutMove(int caseId)
        {
            int[] initCaseIds = get_InitialCases(caseId);

            return repo.AllReadonly<CaseMigration>()
                            .Include(x => x.Case)
                            .ThenInclude(x => x.Court)
                            .Include(x => x.PriorCase)
                            .ThenInclude(x => x.Court)
                            .Include(x => x.CaseMigrationType)
                            .Include(x => x.SendToCourt)
                            .Include(x => x.SendToInstitution)
                            .Include(x => x.InCaseMigrations)
                            .Include(x => x.CaseSessionAct)
                            .ThenInclude(x => x.ActType)
                            .Where(x => x.CaseId == caseId &&
                                        x.Case.CaseStateId != NomenclatureConstants.CaseState.Deleted &&
                                        x.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing)
                            .OrderBy(x => x.Id)
                            .Select(x => new CaseMigrationVM
                            {
                                Id = x.Id,
                                InitialCaseId = x.InitialCaseId,
                                CaseId = x.CaseId,
                                CaseRegNumber = x.Case.RegNumber,
                                CaseRegDate = x.Case.RegDate,
                                CaseSessionAct = (x.CaseSessionAct != null) ? $"{x.CaseSessionAct.ActType.Label} {x.CaseSessionAct.RegNumber}/{x.CaseSessionAct.RegDate:dd.MM.yyyy}" : "",
                                CaseCourtName = x.Case.Court.Label,
                                MigrationDirection = x.CaseMigrationType.MigrationDirection,
                                MigrationTypeId = x.CaseMigrationTypeId,
                                MigrationTypeName = x.CaseMigrationType.Label,
                                SentFromName = (x.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing) ? x.Case.Court.Label : x.PriorCase.Court.Label,
                                SentToName = (x.SendToCourt != null) ? x.SendToCourt.Label : (x.SendToInstitution != null ? x.SendToInstitution.FullName : ""),
                                SendToCortId = x.SendToCourtId,
                                Description = x.Description,
                                CanEdit = x.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing && x.CaseId == caseId && !x.InCaseMigrations.Any(),
                                CanAccept = x.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing && !x.InCaseMigrations.Any() && (x.SendToCourtId == userContext.CourtId || CaseMigrationTypes.SendCaseTypesCanAccept.Contains(x.CaseMigrationTypeId)),
                                DateWrt = x.DateWrt,
                                OutDocumentLabel = ((x.OutDocumentId != null) ? x.OutDocument.DocumentType.Label + " " + x.OutDocument.DocumentNumber + "/" + x.OutDocument.DocumentDate.ToString("dd.MM.yyyy") : string.Empty),
                                OutDocumentDate = ((x.OutDocumentId != null) ? x.OutDocument.DocumentDate : (DateTime?)null),
                                IsReturned = repo.AllReadonly<CaseMigration>().Any(c => c.CaseId == caseId &&
                                                                                        c.Case.CaseStateId != NomenclatureConstants.CaseState.Deleted &&
                                                                                        c.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Incoming &&
                                                                                        c.Id > x.Id)
                            }).AsQueryable();
        }

        /// <summary>
        /// Сетване на първото дело по Вертикално движение на дело - между институциите
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public CaseMigration InitNewMigration(int caseId)
        {
            var caseCase = repo.GetById<Case>(caseId);
            CaseMigration result = new CaseMigration()
            {
                CaseId = caseId,
                CourtId = caseCase.CourtId,
                SendToTypeId = CaseMigrationSendTo.Court,
                PriorCaseId = caseId
            };

            int[] initCaseIds = get_InitialCases(caseId);

            var lastCaseMigration = repo.AllReadonly<CaseMigration>()
                                            .Include(x => x.CaseMigrationType)
                                            .Include(x => x.Case)
                                            .ThenInclude(x => x.Court)
                                            .Where(x => x.SendToCourtId == userContext.CourtId && initCaseIds.Contains(x.InitialCaseId))
                                            .Where(x => x.SendToTypeId == CaseMigrationSendTo.Court)
                                            .Where(x => x.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing)
                                            .OrderByDescending(x => x.Id)
                                            .Select(x => new CaseMigrationVM
                                            {
                                                Id = x.Id,
                                                CaseId = x.CaseId,
                                                InitialCaseId = x.InitialCaseId,
                                                CaseRegNumber = x.Case.RegNumber,
                                                CaseRegDate = x.Case.RegDate,
                                                MigrationTypeId = x.CaseMigrationTypeId,
                                                MigrationTypeName = x.CaseMigrationType.Label,
                                                Description = x.Description,
                                                SentToName = x.Case.Court.Label
                                            }).FirstOrDefault();

            if (lastCaseMigration != null)
            {
                result.InitialCaseId = lastCaseMigration.InitialCaseId;
            }
            return result;
        }

        /// <summary>
        /// Извличане на първото дело от Вертикално движение на дело - между институциите
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private int[] get_InitialCases(int caseId)
        {
            return repo.AllReadonly<CaseMigration>()
                                       .Where(x => x.CaseId == caseId)
                                       .Select(x => x.InitialCaseId)
                                       .Distinct()
                                       .ToArray();
        }

        /// <summary>
        /// Запис на Вертикално движение на дело - между институциите
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool SaveData(CaseMigration model)
        {
            try
            {
                model.SendToCourtId = model.SendToCourtId.EmptyToNull();
                model.SendToInstitutionTypeId = model.SendToInstitutionTypeId.EmptyToNull();
                model.SendToInstitutionId = model.SendToInstitutionId.EmptyToNull();
                model.ReturnCaseId = model.ReturnCaseId.EmptyToNull().EmptyToNull(0);
                model.CaseSessionActId = model.CaseSessionActId.EmptyToNull();
                switch (model.SendToTypeId)
                {
                    case NomenclatureConstants.CaseMigrationSendTo.Court:
                        model.SendToInstitutionTypeId = null;
                        model.SendToInstitutionId = null;
                        break;
                    case NomenclatureConstants.CaseMigrationSendTo.Institution:
                        model.SendToCourtId = null;
                        break;
                }

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseMigration>(model.Id);
                    saved.CaseSessionActId = model.CaseSessionActId;
                    saved.CaseMigrationTypeId = model.CaseMigrationTypeId;
                    saved.ReturnCaseId = model.ReturnCaseId;
                    saved.SendToTypeId = model.SendToTypeId;
                    saved.SendToCourtId = model.SendToCourtId;
                    saved.SendToInstitutionTypeId = model.SendToInstitutionTypeId;
                    saved.SendToInstitutionId = model.SendToInstitutionId;
                    saved.Description = model.Description;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    if (model.InitialCaseId == 0)
                    {
                        model.InitialCaseId = model.CaseId;
                    }
                    if (model.PriorCaseId == 0)
                    {
                        model.PriorCaseId = model.CaseId;
                    }

                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    repo.Add<CaseMigration>(model);

                    // При изпращане за обжалване се сменя статуса на дело на Обжалвано
                    if (model.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendNextLevel)
                    {
                        var caseCase = repo.GetById<Case>(model.CaseId);
                        caseCase.CaseStateId = NomenclatureConstants.CaseState.Appealed;
                        caseCase.DateWrt = DateTime.Now;
                        caseCase.UserId = userContext.UserId;
                        repo.Update(caseCase);
                    }

                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на движение на дело Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на тип на Вертикално движение на дело - между институциите
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public List<SelectListItem> Get_MigrationTypes(int direction)
        {
            return repo.AllReadonly<CaseMigrationType>()
                    .Where(x => x.MigrationDirection == direction)
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.OrderNumber)
                    .ToSelectList(x => x.Id, x => x.Label);
        }

        /// <summary>
        /// Извличане на всчики съдилища от Вертикално движение на дело - между институциите за комбо
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownList_Court(int caseId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = new List<SelectListItem>();
            var caseMigrationFind = repo.AllReadonly<CaseMigration>().Where(x => x.CaseId == caseId).FirstOrDefault();

            if (caseMigrationFind != null)
            {
                var caseMigrations = repo.AllReadonly<CaseMigration>()
                                     .Include(x => x.Case)
                                     .ThenInclude(x => x.Court)
                                     .Where(x => x.InitialCaseId == caseMigrationFind.InitialCaseId)
                                     .ToList();

                foreach (var caseMigration in caseMigrations.OrderBy(x => x.Case.Court.Label))
                {
                    if (!result.Any(x => x.Value == caseMigration.Case.Court.Id.ToString()))
                    {
                        var selectListItem = new SelectListItem()
                        {
                            Text = caseMigration.Case.Court.Label,
                            Value = caseMigration.Case.Court.Id.ToString()
                        };

                        result.Add(selectListItem);
                    }
                }
            }

            if (result.Count < 1)
            {
                var selectListItem = new SelectListItem()
                {
                    Text = userContext.CourtName,
                    Value = userContext.CourtId.ToString()
                };

                result.Add(selectListItem);
            }

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        /// <summary>
        /// Извличане на всички съдилища от Вертикално движение на дело - между институциите за комбо
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownList_CourtCase(int caseId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = new List<SelectListItem>();

            var caseMigrationFind = repo.AllReadonly<CaseMigration>().Where(x => x.CaseId == caseId).FirstOrDefault();

            if (caseMigrationFind != null)
            {
                var caseMigrations = repo.AllReadonly<CaseMigration>()
                                         .Include(x => x.PriorCase)
                                         .ThenInclude(x => x.Court)
                                         .Where(x => x.InitialCaseId == caseMigrationFind.InitialCaseId &&
                                                     x.CaseId == caseId &&
                                                     x.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptCase_AfterComplain &&
                                                     x.DateExpired == null)
                                         .ToList();



                foreach (var caseMigration in caseMigrations.Where(x => x.PriorCaseId != caseId))
                {
                    if (!result.Any(x => x.Value == caseMigration.PriorCase.Id.ToString()))
                    {
                        var selectListItem = new SelectListItem()
                        {
                            Text = caseMigration.PriorCase.Court.Label + " - " + caseMigration.PriorCase.RegNumber + "/" + caseMigration.PriorCase.RegDate.ToString("dd.MM.yyyy"),
                            Value = caseMigration.PriorCase.Id.ToString()
                        };

                        result.Add(selectListItem);
                    }
                }
            }

            if (result.Count != 1)
            {
                if (addDefaultElement)
                {
                    result = result
                        .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                        .ToList();
                }

                if (addAllElement)
                {
                    result = result
                        .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                        .ToList();
                }
            }

            return result;
        }

        public DateTime? GetDateTimeAcceptCaseAfterComplain(int CaseId, int PriorCaseId)
        {
            var caseMigrations = repo.AllReadonly<CaseMigration>()
                                     .Where(x => x.PriorCaseId == PriorCaseId &&
                                                 x.CaseId == CaseId &&
                                                 x.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptCase_AfterComplain &&
                                                 x.DateExpired == null)
                                     .FirstOrDefault();

            return (caseMigrations != null) ? caseMigrations.DateWrt : (DateTime?)null;
        }

        /// <summary>
        /// Извличане на данни за приемане на движениео обратно
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownList_ReturnCase(int caseId, bool addDefaultElement = true, bool addAllElement = false)
        {
            int[] initCaseIds = get_InitialCases(caseId);

            var commonNomenclatures = repo.AllReadonly<CaseMigration>()
                                          .Include(x => x.CaseMigrationType)
                                          .Include(x => x.Case)
                                          .ThenInclude(x => x.Court)
                                          .Include(x => x.Case)
                                          .ThenInclude(x => x.CaseType)
                                          .Where(x => initCaseIds.Contains(x.InitialCaseId))
                                          .Where(x => x.SendToTypeId == CaseMigrationSendTo.Court)
                                          .Where(x => x.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing)
                                          .Where(x => x.CaseId != caseId)
                                          .OrderByDescending(x => x.Id)
                                          .Select(x => new BaseCommonNomenclature
                                          {
                                              Id = x.CaseId,
                                              OrderNumber = x.Id,
                                              Label = $"{x.Case.CaseType.Code} {x.Case.RegNumber} ({x.Case.Court.Label})",
                                              IsActive = true,
                                              DateStart = DateTime.MinValue
                                          }).ToList();

            var result = new List<BaseCommonNomenclature>();
            foreach (var commonNomenclature in commonNomenclatures)
            {
                if (!result.Any(x => x.Id == commonNomenclature.Id))
                {
                    result.Add(commonNomenclature);
                }
            }

            return result.AsQueryable().ToSelectList(addDefaultElement, addAllElement);
        }

        /// <summary>
        /// Приемане на Вертикално движение на дело - между институциите
        /// </summary>
        /// <param name="id"></param>
        /// <param name="caseId"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public bool AcceptCaseMigration(int id, int caseId, string description = null)
        {
            var outMigration = repo.AllReadonly<CaseMigration>()
                                        .Include(x => x.InCaseMigrations)
                                        .Where(x => x.Id == id)
                                        .FirstOrDefault();

            if (outMigration.InCaseMigrations.Any())
            {
                return false;
            }
            try
            {
                var newMigrationType = repo.AllReadonly<CaseMigrationType>()
                                                .Where(x => x.PriorMigrationTypeId == outMigration.CaseMigrationTypeId)
                                                .Select(x => x.Id)
                                                .FirstOrDefault();
                var caseCase = repo.GetById<Case>(caseId);

                var inMigration = new CaseMigration()
                {
                    InitialCaseId = outMigration.InitialCaseId,
                    CaseId = caseId,
                    CourtId = caseCase.CourtId,
                    PriorCaseId = outMigration.CaseId,
                    CaseMigrationTypeId = newMigrationType,
                    DateWrt = DateTime.Now,
                    UserId = userContext.UserId,
                    SendToTypeId = NomenclatureConstants.CaseMigrationSendTo.Court,
                    SendToCourtId = userContext.CourtId,
                    Description = description,
                    OutCaseMigrationId = outMigration.Id
                };

                repo.Add(inMigration);

                if ((newMigrationType == NomenclatureConstants.CaseMigrationTypes.AcceptCase_AfterComplain) ||
                    (newMigrationType == NomenclatureConstants.CaseMigrationTypes.AcceptCase_ForAdministration))
                {
                    var caseMigrationSend = repo.AllReadonly<CaseMigration>()
                                                .Where(x => x.InitialCaseId == outMigration.InitialCaseId &&
                                                            //x.SendToCourtId == outMigration.CourtId &&
                                                            x.CaseId == caseId &&
                                                            x.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendNextLevel &&
                                                            x.Id < outMigration.Id)
                                                .OrderByDescending(x => x.Id)
                                                .FirstOrDefault();

                    if (caseMigrationSend != null)
                    {
                        if (caseMigrationSend.CaseSessionActId != null)
                        {
                            var act = repo.GetById<CaseSessionAct>(caseMigrationSend.CaseSessionActId);
                            act.ActReturnDate = DateTime.Now;
                            act.DateWrt = DateTime.Now;
                            act.UserId = userContext.UserId;
                            repo.Update(act);
                        }
                    }
                }

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Обединяване на дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool UnionCase(CaseMigrationUnionVM model)
        {
            try
            {
                var lastInitialCaseId = repo.AllReadonly<CaseMigration>()
                                                .Where(x => x.CaseId == model.CaseId)
                                                .OrderBy(x => x.Id)
                                                .Select(x => x.InitialCaseId)
                                                .DefaultIfEmpty(model.CaseId)
                                                .FirstOrDefault();

                var newCase = repo.GetById<Case>(model.CaseId);
                var oldCase = repo.GetById<Case>(model.CaseToUnionId);

                newCase.LoadIndex = Math.Max(newCase.LoadIndex, oldCase.LoadIndex);

                var newCaseMigration = new CaseMigration()
                {
                    InitialCaseId = lastInitialCaseId,
                    CaseId = model.CaseId,
                    CourtId = newCase.CourtId,
                    PriorCaseId = model.CaseToUnionId,
                    CaseMigrationTypeId = NomenclatureConstants.CaseMigrationTypes.CaseUnion,
                    DateWrt = DateTime.Now,
                    UserId = userContext.UserId,
                    SendToTypeId = NomenclatureConstants.CaseMigrationSendTo.Court,
                    SendToCourtId = userContext.CourtId,
                    Description = model.Description
                };
                var oldCaseMigration = new CaseMigration()
                {
                    InitialCaseId = lastInitialCaseId,
                    CaseId = model.CaseToUnionId,
                    CourtId = oldCase.CourtId,
                    PriorCaseId = model.CaseId,
                    CaseMigrationTypeId = NomenclatureConstants.CaseMigrationTypes.CaseUnion,
                    DateWrt = DateTime.Now,
                    UserId = userContext.UserId,
                    SendToTypeId = NomenclatureConstants.CaseMigrationSendTo.Court,
                    SendToCourtId = userContext.CourtId
                };

                repo.Add(newCaseMigration);
                repo.Add(oldCaseMigration);
                repo.Update(newCase);

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"UnionCase CaseId={model.CaseId}; UnionCaseId={model.CaseToUnionId}");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за последно движение
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int GetLastMigrationAcceptToUse(CaseMigrationFindCaseVM model)
        {
            return repo.AllReadonly<CaseMigration>()
                                             .Include(x => x.InCaseMigrations)
                                             .Where(x => x.CaseId == model.FromCaseId)
                                             .Where(FilterExpireInfo<CaseMigration>(false))
                                             .Where(x => CaseMigrationTypes.SendCaseTypesCanAccept.Contains(x.CaseMigrationTypeId))
                                             .Where(x => !x.InCaseMigrations.Any())
                                             .Select(x => x.Id)
                                             .DefaultIfEmpty(0)
                                             .FirstOrDefault();
        }

        public bool AcceptToUse(CaseMigrationFindCaseVM model, int lastMigrationId)
        {
            throw new NotImplementedException();
        }

        public bool IsExistMigrationWithComplainWithDocumentId(long DocumentId)
        {
            return repo.AllReadonly<CaseMigration>()
                       .Any(x => x.CaseSessionAct.CaseSessionActComplains.Any(a => a.DateExpired == null &&
                                                                                   a.ComplainDocumentId == DocumentId));
        }

        public bool IsExistMigrationWithAct(long ActId)
        {
            return repo.AllReadonly<CaseMigration>()
                       .Any(x => x.CaseSessionActId == ActId && x.DateExpired == null);
        }

        public CaseMigrationVM Case_GetPriorCase(long documentId)
        {
            var priorCaseId = repo.All<DocumentCaseInfo>(x => x.DocumentId == documentId).Select(x => x.CaseId).FirstOrDefault() ?? 0;
            if (priorCaseId == 0)
            {
                return null;
            }

            var lastCaseMigration = repo.AllReadonly<CaseMigration>()
                                            .Include(x => x.CaseMigrationType)
                                            .Include(x => x.Case)
                                            .ThenInclude(x => x.Court)
                                            .Where(x => x.SendToCourtId == userContext.CourtId && x.CaseId == priorCaseId)
                                            .Where(x => x.SendToTypeId == CaseMigrationSendTo.Court)
                                            .Where(x => x.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing)
                                            .Where(FilterExpireInfo<CaseMigration>(false))
                                            .OrderByDescending(x => x.Id)
                                            .Select(x => new CaseMigrationVM
                                            {
                                                Id = x.Id,
                                                CaseId = x.CaseId,
                                                InitialCaseId = x.InitialCaseId,
                                                CaseRegNumber = x.Case.RegNumber,
                                                CaseRegDate = x.Case.RegDate,
                                                MigrationTypeId = x.CaseMigrationTypeId,
                                                MigrationTypeName = x.CaseMigrationType.Label,
                                                Description = x.Description,
                                                SentToName = x.Case.Court.Label
                                            }).FirstOrDefault();
            return lastCaseMigration;
        }
    }
}
