using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;

namespace IOWebApplication.Core.Services
{
    public class CaseMigrationService : BaseService, ICaseMigrationService
    {
        private readonly ICaseLifecycleService caseLifecycleService;
        private readonly IWorkNotificationService workNotificationService;

        public CaseMigrationService(ILogger<CaseMigrationService> _logger,
                                    IRepository _repo,
                                    IUserContext _userContext,
                                    ICaseLifecycleService _caseLifecycleService,
                                    IWorkNotificationService _workNotificationService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            caseLifecycleService = _caseLifecycleService;
            workNotificationService = _workNotificationService;
        }

        /// <summary>
        /// Извличане на данни за Вертикално движение на дело - между институциите
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IQueryable<CaseMigrationVM> Select(int caseId)
        {
            int[] initCaseIds = GetInitialCasesByCaseId(caseId);

            return repo.AllReadonly<CaseMigration>()
                       .Where(x => initCaseIds.Contains(x.InitialCaseId) && x.Case.CaseStateId != NomenclatureConstants.CaseState.Deleted /* && x.Case.CaseStateId != NomenclatureConstants.CaseState.Rejected*/)
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
                           SentFromName = (x.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing) ? x.Case.Court.Label : x.PriorCase.Court.Label + (x.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.CaseConnection ? " - " + x.PriorCase.RegNumber : string.Empty),
                           SentToName = (x.SendToCourt != null) ? x.SendToCourt.Label : (x.SendToInstitution != null ? x.SendToInstitution.FullName : ""),
                           SendToCortId = x.SendToCourtId,
                           Description = x.Description,
                           CanEdit = x.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing && x.CaseId == caseId && !x.InCaseMigrations.Any(),
                           CanAccept = (x.MigrationKind == null) && x.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing && !x.InCaseMigrations.Any() && ((x.CaseId != caseId && x.SendToCourtId == userContext.CourtId) || CaseMigrationTypes.SendCaseTypesCanAccept.Contains(x.CaseMigrationTypeId) || ((x.SendToTypeId == NomenclatureConstants.CaseMigrationSendTo.Institution) && CaseMigrationTypes.SendCaseTypesCanAcceptToInstitution.Contains(x.CaseMigrationTypeId))),
                           DateWrt = x.DateWrt,
                           CaseStateId = x.Case.CaseStateId,
                           CaseStateName = x.Case.CaseState.Label,
                           InitDocumentNumber = x.Case.Document.DocumentNumber,
                           InitDocumentDate = x.Case.Document.DocumentDate,
                           InitDocumentType = x.Case.Document.DocumentType.Label,
                           IsSendCompetence = x.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendCompetence,
                           HasAcceptWithInterval = NomenclatureConstants.CaseMigrationTypes.HasAcceptWithInterval.Contains(x.CaseMigrationTypeId),
                           MigrationKind = x.MigrationKind,
                           OutDocumentId = x.OutDocumentId,
                           OutDocumentLabel = (x.OutDocumentId > 0) ? $"{x.OutDocument.DocumentType} {x.OutDocument.DocumentNumber}/{x.OutDocument.DocumentDate:dd.MM.yyyy}" : ""
                       }).AsQueryable();
        }

        public IQueryable<CaseMigrationVM> SelectOutMove(int caseId)
        {
            int[] initCaseIds = GetInitialCasesByCaseId(caseId);

            return repo.AllReadonly<CaseMigration>()
                       .Where(x => x.CaseId == caseId &&
                                   x.Case.CaseStateId != NomenclatureConstants.CaseState.Deleted &&
                                   x.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing)
                       .OrderBy(x => x.Id)
                       .Select(x => new CaseMigrationVM
                       {
                           Id = x.Id,
                           IsReturned = repo.AllReadonly<CaseMigration>().Any(c => c.CaseId == caseId &&
                                                                                   c.Case.CaseStateId != NomenclatureConstants.CaseState.Deleted &&
                                                                                   c.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Incoming &&
                                                                                   c.Id > x.Id),
                           SentToName = (x.SendToCourt != null) ? x.SendToCourt.Label : (x.SendToInstitution != null ? x.SendToInstitution.FullName : ""),
                           OutDocumentLabel = ((x.OutDocumentId != null) ? (x.OutDocument.DateExpired == null ? x.OutDocument.DocumentType.Label + " " + x.OutDocument.DocumentNumber + "/" + x.OutDocument.DocumentDate.ToString("dd.MM.yyyy") : string.Empty) : string.Empty),
                           OutDocumentDate = ((x.OutDocumentId != null) ? (x.OutDocument.DateExpired == null ? x.OutDocument.DocumentDate : (DateTime?)null) : (DateTime?)null),
                           Description = x.Description,
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Сетване на първото дело по Вертикално движение на дело - между институциите
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public async Task<CaseMigration> InitNewMigration(int caseId)
        {
            var caseCourtId = await repo.GetPropByIdAsync<Case, int>(x => x.Id == caseId, x => x.CourtId);
            CaseMigration result = new CaseMigration()
            {
                CaseId = caseId,
                CourtId = caseCourtId,
                SendToTypeId = CaseMigrationSendTo.Court,
                PriorCaseId = caseId
            };

            int[] initCaseIds = GetInitialCasesByCaseId(caseId);

            var lastCaseMigration = await repo.AllReadonly<CaseMigration>()
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
                                            }).FirstOrDefaultAsync();

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
        public int[] GetInitialCasesByCaseId(int caseId)
        {
            return GetInitialCasesByCaseIdAsync(caseId).GetAwaiter().GetResult();
        }

        public int[] GetConnectedCasesByCaseId(int caseId, bool activeOnly = true)
        {
            int[] initCasesIds = GetInitialCasesByCaseId(caseId);

            Expression<Func<CaseMigration, bool>> filterActive = x => true;
            if (activeOnly)
            {
                filterActive = x => x.DateExpired == null;
            }

            var connectedCaseIds = repo.AllReadonly<CaseMigration>()
                                        .Where(x => initCasesIds.Contains(x.InitialCaseId))
                                        .Where(filterActive)
                                        .Select(x => x.CaseId)
                                        .ToList();

            connectedCaseIds.AddRange(initCasesIds);

            return connectedCaseIds.Distinct().ToArray();
        }


        [Obsolete]
        private int[] OLD_get_InitialCases(int caseId)
        {
            return repo.AllReadonly<CaseMigration>()
                       .Where(x => x.CaseId == caseId ||
                                   (x.PriorCaseId == caseId && NomenclatureConstants.CaseMigrationTypes.CaseUnionConnection.Contains(x.CaseMigrationTypeId)))
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

                    FixInitialCase(model);

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
                    }

                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на движение на дело Id={model.Id}");
                return false;
            }
        }

        /// <summary>
        /// При връщане на дело се проверява дали InitialCaseId е общо и за върнатото дело. 
        /// Ако не се намира първото общо дело, за да се зареди в движенията на върнатото
        /// </summary>
        /// <param name="model"></param>

        private void FixInitialCase(CaseMigration model)
        {
            if ((model.ReturnCaseId ?? 0) == 0)
            {
                return;
            }

            var returnInitCases = GetInitialCasesByCaseId(model.ReturnCaseId.Value);

            if (!returnInitCases.Contains(model.InitialCaseId))
            {
                var caseInitCases = GetInitialCasesByCaseId(model.CaseId);

                var firstCommonInitCase = caseInitCases.Where(x => returnInitCases.Contains(x)).FirstOrDefault();

                if (firstCommonInitCase > 0)
                {
                    model.InitialCaseId = firstCommonInitCase;
                }
            }
        }

        /// <summary>
        /// Извличане на тип на Вертикално движение на дело - между институциите
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public List<SelectListItem> Get_MigrationTypes(int direction, int[] migrationTypes = null, int? caseId = null)
        {
            Expression<Func<CaseMigrationType, bool>> whereIds = x => true;
            if (migrationTypes != null)
            {
                whereIds = x => migrationTypes.Contains(x.Id);
            }
            Expression<Func<CaseMigrationType, bool>> whereFilterFP = x => true;
            if (caseId > 0)
            {
                bool isFastProcess = GetPropById<Case, bool>(x => x.Id == caseId.Value, x => x.IsFastProcess ?? false);
                if (!isFastProcess)
                {
                    whereFilterFP = x => !NomenclatureConstants.CaseMigrationTypes.SendCase_FromAssignment.Contains(x.Id);
                }
            }


            return repo.AllReadonly<CaseMigrationType>()
                    .Where(x => x.MigrationDirection == direction)
                    .Where(x => x.IsActive)
                    .Where(whereIds)
                    .Where(whereFilterFP)
                    .OrderBy(x => x.OrderNumber)
                    .ToSelectList(x => x.Id, x => x.Label);
        }

        /// <summary>
        /// Извличане на всчики съдилища от Вертикално движение на дело - между институциите за комбо
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="addDefaultElement">Добавяне на елемен "Избери"</param>
        /// <param name="addAllElement">Добавяне на елемент "Всички"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDropDownList_Court(int caseId, bool addDefaultElement = true, bool addAllElement = false)
        {
            int[] initialCaseIds = await repo.AllReadonly<CaseMigration>()
                                             .Where(x => x.CaseId == caseId)
                                             .Select(x => x.InitialCaseId)
                                             .ToArrayAsync()
                                             .ConfigureAwait(false);

            List<SelectListItem> result = initialCaseIds.Any() ? await repo.AllReadonly<CaseMigration>()
                                                                           .Where(x => initialCaseIds.Contains(x.InitialCaseId))
                                                                           .Select(x => new
                                                                           {
                                                                               Value = x.Case.CourtId.ToString(),
                                                                               Text = x.Case.Court.Label
                                                                           })
                                                                           .GroupBy(x => new { x.Value, x.Text })
                                                                           .Select(g => new SelectListItem()
                                                                           {
                                                                               Text = g.Key.Text,
                                                                               Value = g.Key.Value,
                                                                           })
                                                                           .OrderBy(x => x.Text)
                                                                           .ToListAsync()
                                                                           .ConfigureAwait(false) : new List<SelectListItem>();

            if (!result.Any())
            {
                result.Add(new SelectListItem() { Text = userContext.CourtName, Value = userContext.CourtId.ToString() });
            }

            if (addDefaultElement)
            {
                result = result.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                               .ToList();
            }

            if (addAllElement)
            {
                result = result.Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
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

            var caseMigrationFind = repo.AllReadonly<CaseMigration>().Where(x => x.CaseId == caseId).OrderByDescending(x => x.DateWrt).FirstOrDefault();

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
                                     .OrderByDescending(x => x.DateWrt)
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
            int[] initCaseIds = GetInitialCasesByCaseId(caseId);

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
        public SaveResultVM AcceptCaseMigration(int id, int caseId, string description = null, bool isNewInterval = false, int? migrationKind = null)
        {
            var outMigration = repo.AllReadonly<CaseMigration>()
                                        .Include(x => x.InCaseMigrations)
                                        .Where(x => x.Id == id)
                                        .FirstOrDefault();

            if (outMigration.InCaseMigrations.Any())
            {
                return new SaveResultVM(false, "", "exists");
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
                    SendToTypeId = outMigration.SendToTypeId,
                    SendToCourtId = outMigration.SendToCourtId,
                    SendToInstitutionId = outMigration.SendToInstitutionId,
                    SendToInstitutionTypeId = outMigration.SendToInstitutionTypeId,
                    Description = description,
                    OutCaseMigrationId = outMigration.Id,
                    MigrationKind = migrationKind
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
                            if (!string.IsNullOrEmpty(userContext.UserId))
                            {
                                act.UserId = userContext.UserId;
                            }
                        }
                    }
                }

                repo.SaveChanges();

                if (NomenclatureConstants.CaseMigrationTypes.HasAcceptWithInterval.Contains(outMigration.CaseMigrationTypeId) && isNewInterval)
                {
                    caseLifecycleService.CaseLifecycle_NewIntervalSave(caseId, DateTime.Now, inMigration.Id);
                }

                return new SaveResultVM(true)
                {
                    ObjectId = inMigration.Id
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"CaseMigrationService.AcceptCaseMigration - Приемане на Вертикално движение на дело - между институциите");
                return new SaveResultVM(false, ex.Message);
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
                                                .FirstOrValue(model.CaseId);

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
                                             .FirstOrValue(0);
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

        public async Task<CaseMigrationVM> Case_GetPriorCase(long documentId)
        {
            int priorCaseId = await repo.AllReadonly<DocumentCaseInfo>(x => x.DocumentId == documentId).Select(x => x.CaseId).FirstOrDefaultAsync() ?? 0;
            if (priorCaseId == 0)
            {
                return null;
            }
            var lastCaseMigration = await loadLastmigrationByCase(priorCaseId, 0);
            if (lastCaseMigration == null)
            {
                var lastInitialCaseId = await repo.AllReadonly<CaseMigration>()
                                               .Where(x => x.CaseId == priorCaseId)
                                               .OrderBy(x => x.Id)
                                               .Select(x => x.InitialCaseId)
                                               .FirstOrValueAsync(priorCaseId);

                lastCaseMigration = await loadLastmigrationByCase(0, lastInitialCaseId);
            }
            if (lastCaseMigration != null)
            {
                if (lastCaseMigration.CaseId != priorCaseId)
                {
                    var priorCaseInfo = await repo.AllReadonly<Case>()
                                                .Where(x => x.Id == priorCaseId)
                                                .Select(x => new
                                                {
                                                    x.RegDate,
                                                    x.RegNumber,
                                                    CourtName = x.Court.Label
                                                })
                                                .FirstOrDefaultAsync();
                    lastCaseMigration.CaseRegNumber = priorCaseInfo.RegNumber;
                    lastCaseMigration.SentToName = priorCaseInfo.CourtName;
                    lastCaseMigration.MigrationTypeName = "Свързано дело";
                    lastCaseMigration.PriorCaseMigration = true;
                }
            }
            return lastCaseMigration;
        }

        private Task<CaseMigrationVM> loadLastmigrationByCase(int caseId, int initCaseId)
        {
            Expression<Func<CaseMigration, bool>> whereCase = x => x.CaseId == caseId;
            if (initCaseId > 0)
            {
                whereCase = x => x.InitialCaseId == initCaseId;
            }

            return repo.AllReadonly<CaseMigration>()
                                            .Where(x => x.SendToCourtId == userContext.CourtId)
                                            .Where(whereCase)
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
                                            }).FirstOrDefaultAsync();
        }

        public SaveResultVM CheckData(CaseMigration model)
        {
            switch (model.CaseMigrationTypeId)
            {
                case NomenclatureConstants.CaseMigrationTypes.ReturnCase_AfterComplain:
                    int caseStateId = repo.GetPropById<Case, int>(x => x.Id == model.CaseId, x => x.CaseStateId);

                    if (!NomenclatureConstants.CaseState.UnregisteredManageble.Contains(caseStateId))
                    {
                        var finalActs = repo.AllReadonly<CaseSessionAct>()
                                                .Where(FilterExpireInfo<CaseSessionAct>(false))
                                                .Where(x => x.CaseId == model.CaseId)
                                                .Where(x => x.IsFinalDoc == true && x.ActDeclaredDate != null)
                                                .Select(x => new
                                                {
                                                    x.Id,
                                                    x.ActComplainResultId
                                                })
                                                .ToList();
                        if (finalActs.Count() == 0)
                        {
                            return new SaveResultVM(false, "По делото все още няма финализиращ акт.");
                        }
                        if (!finalActs.Any(a => a.ActComplainResultId != null))
                        {
                            return new SaveResultVM(false, "Няма избран Резултат/степен на уважаване на иска.");
                        }
                    }
                    break;

                case NomenclatureConstants.CaseMigrationTypes.SendCase_FromRandomAssignment:
                case NomenclatureConstants.CaseMigrationTypes.SendCase_FromAssignmentByAddress:
                    bool isNewFPcase = repo.GetPropById<Case, bool>(x => x.Id == model.CaseId, x => x.IsFastProcess ?? false);
                    if (!isNewFPcase)
                    {
                        return new SaveResultVM(false, "Това движение е приложимо само за дела заповедни производства, образувани в централизираната регистратура ");
                    }
                    break;
            }
            return new SaveResultVM(true);
        }

        /// <summary>
        /// Връща данни за предходно свързано към иницииращ документ дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public async Task<CaseMigrationPriorVM> GetPriorCaseInfo(int caseId)
        {
            var initDocId = await repo.GetPropByIdAsync<Case, long>(x => x.Id == caseId, x => x.DocumentId);

            var priorCaseInfo = await repo.AllReadonly<DocumentCaseInfo>()
                                            .Where(x => x.DocumentId == initDocId)
                                            .Where(x => x.CaseId > 0)
                                            .Select(x => new
                                            {
                                                CourtId = x.Case.CourtId,
                                                CaseId = x.CaseId.Value,
                                                CourtName = x.Case.Court.Label,
                                                x.Case.RegNumber,
                                                x.Case.RegDate,
                                                CaseGroupCode = x.Case.CaseGroup.Code
                                            }).FirstOrDefaultAsync();

            if (priorCaseInfo == null)
            {
                return null;
            }

            return new CaseMigrationPriorVM()
            {
                PriorCourtId = priorCaseInfo.CourtId,
                PriorCaseId = priorCaseInfo.CaseId,
                CaseId = caseId,
                CourtName = priorCaseInfo.CourtName,
                CaseInfo = $"{priorCaseInfo.CaseGroupCode} {priorCaseInfo.RegNumber}",
                HasMigrations = await repo.AllReadonly<CaseMigration>().Where(x => x.CaseId == caseId).AnyAsync()
            };
        }

        public async Task<SaveResultVM> SaveData_PriorCase(CaseMigrationPriorVM model)
        {
            try
            {
                using (var ts = repo.BeginTransaction())
                {

                    var priorInfo = await GetPriorCaseInfo(model.CaseId);

                    var caseMigration = new CaseMigration()
                    {
                        InitialCaseId = priorInfo.PriorCaseId,
                        CaseId = priorInfo.PriorCaseId,
                        PriorCaseId = priorInfo.PriorCaseId,
                        CourtId = priorInfo.PriorCourtId,
                        SendToTypeId = NomenclatureConstants.CaseMigrationSendTo.Court,
                        SendToCourtId = userContext.CourtId,
                        UserId = userContext.UserId,
                        DateWrt = DateTime.Now,
                        CaseMigrationTypeId = model.CaseMigrationTypeId.Value
                    };

                    repo.Add<CaseMigration>(caseMigration);

                    // При изпращане за обжалване се сменя статуса на дело на Обжалвано
                    if (model.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendNextLevel)
                    {
                        var caseCase = repo.GetById<Case>(model.PriorCaseId);
                        if (caseCase.CaseStateId != NomenclatureConstants.CaseState.Appealed)
                        {
                            caseCase.CaseStateId = NomenclatureConstants.CaseState.Appealed;
                            caseCase.DateWrt = DateTime.Now;
                            caseCase.UserId = userContext.UserId;
                        }
                    }

                    await repo.SaveChangesAsync();

                    var accRes = AcceptCaseMigration(caseMigration.Id, model.CaseId);

                    if (accRes.Result)
                    {
                        ts.Commit();
                    }

                    await workNotificationService.SaveNotificationsForNewCaseHigherInstanceWith0604_1_2FastProcess(model.CaseId);

                    accRes.ObjectId = caseMigration.Id;
                    return accRes;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на движение на дело Id={model.CaseId}");
                return new SaveResultVM(false);
            }
        }
    }
}
