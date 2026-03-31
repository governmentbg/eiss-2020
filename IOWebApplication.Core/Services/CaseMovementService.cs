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
using static IOWebApplication.Infrastructure.Constants.AccountConstants;
using System.Linq.Expressions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System.Threading.Tasks;
using Nest;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using IOWebApplication.Core.Helper;

namespace IOWebApplication.Core.Services
{
    public class CaseMovementService : BaseService, ICaseMovementService
    {
        private readonly ICommonService commonService;
        private readonly IUrlHelper urlHelper;

        public CaseMovementService(ILogger<CaseMovementService> _logger,
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
        /// <param name="caseId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<CaseMovementVM>> GetCaseMovementData(int caseId)
        {
            Expression<Func<CaseMovement, bool>> caseIdWhere = x => true;
            if (caseId > 0)
                caseIdWhere = x => x.CaseId == caseId;

            List<CaseMovementVM> caseMovementVMs = await repo.AllReadonly<CaseMovement>()
                                                             .Where(caseIdWhere)
                                                             .Where(x => x.IsActive)
                                                             .OrderByDescending(x => x.DateSend)
                                                             .Take(10)
                                                             .Select(x => new CaseMovementVM()
                                                             {
                                                                 Id = x.Id,
                                                                 CaseId = x.CaseId,
                                                                 CourtId = x.CourtId,
                                                                 MovementTypeId = x.MovementTypeId,
                                                                 MovementTypeLabel = x.MovementType.Label,
                                                                 NameFor = (x.MovementTypeId == NomenclatureConstants.CaseMovementType.ToPerson) ? !string.IsNullOrEmpty(x.ToUserId) ? x.ToUser.LawUnit.FullName :
                                                                                                                                                                                       string.Empty :
                                                                                                                                                   ((x.MovementTypeId == NomenclatureConstants.CaseMovementType.ToOtdel) ? (x.CourtOrganizationId != null ? x.CourtOrganization.Label :
                                                                                                                                                                                                                                                            string.Empty) :
                                                                                                                                                                                                                           x.OtherInstitution),
                                                                 ToUserId = x.ToUserId,
                                                                 CourtOrganizationId = x.CourtOrganizationId,
                                                                 OtherInstitution = x.OtherInstitution,
                                                                 DateSend = x.DateSend,
                                                                 DateAccept = x.DateAccept,
                                                                 Description = x.Description,
                                                                 DisableDescription = x.DisableDescription,
                                                                 AcceptDescription = x.AcceptDescription,
                                                                 IsActive = x.IsActive,
                                                                 IsActiveText = x.IsActive ? "Активен" : "Неактивен",
                                                                 IsEdit = false,
                                                                 IsAccept = false,
                                                                 AcceptUserId = x.AcceptUserId,
                                                                 AcceptLawUnitName = !string.IsNullOrEmpty(x.AcceptUserId) ? x.AcceptUser.LawUnit.FullName :
                                                                                                                             string.Empty,
                                                                 UserId = x.UserId,
                                                                 UserLawUnitId = x.User.LawUnitId,
                                                                 UserLawUnitName = x.User.LawUnit.FullName
                                                             })
                                                             .ToListAsync();

            SetEditAccept(caseMovementVMs);
            return caseMovementVMs;
        }

        /// <summary>
        /// Сетване на права за местоположение
        /// </summary>
        /// <param name="caseMovementVMs"></param>
        private void SetEditAccept(IEnumerable<CaseMovementVM> caseMovementVMs)
        {
            var maxIdElement = caseMovementVMs.OrderByDescending(x => x.Id)
                                              .FirstOrDefault();

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
            if (model.DateAccept != null)
                return false;

            if (!model.IsActive)
                return false;

            if (model.UserId != userContext.UserId)
            {
                if (userContext.IsUserInRole(Roles.Supervisor) ||
                    userContext.IsUserInRole(Roles.Administrator))
                    return true;
                else
                    return false;
            }

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
            {
                if (userContext.IsUserInRole(Roles.Supervisor) ||
                    userContext.IsUserInRole(Roles.Administrator))
                    return true;
                else
                    return false;
            }

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
                {
                    if (userContext.IsUserInRole(Roles.Supervisor) ||
                        userContext.IsUserInRole(Roles.Administrator))
                        return true;
                    else
                        return false;
                }
            }
            else
            {
                if (model.MovementTypeId == NomenclatureConstants.CaseMovementType.ToOtdel)
                {
                    var lawUnits = commonService.LawUnit_ByCourtDate(userContext.CourtId, DateTime.Now, model.CourtOrganizationId ?? 0);
                    if (!lawUnits.Any(x => x.Id == userContext.LawUnitId))
                    {
                        if (userContext.IsUserInRole(Roles.Supervisor) ||
                            userContext.IsUserInRole(Roles.Administrator))
                            return true;
                        else
                            return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Метод връщащ попълнен нов обект за запис
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private CaseMovement GetCaseMovement(CaseMovementVM model)
        {
            return new CaseMovement()
            {
                CaseId = model.CaseId,
                CourtId = model.CourtId,
                MovementTypeId = model.MovementTypeId,
                ToUserId = model.ToUserId,
                CourtOrganizationId = model.CourtOrganizationId,
                OtherInstitution = model.OtherInstitution,
                DateSend = DateTime.Now,
                Description = model.Description,
                IsActive = true,
                DateWrt = DateTime.Now,
                UserId = userContext.UserId
            };
        }

        /// <summary>
        /// Метод за запис на местоположение
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<bool> CreateMovement(CaseMovementVM model)
        {
            try
            {
                model.CourtOrganizationId = model.CourtOrganizationId.EmptyToNull();
                model.ToUserId = model.ToUserId == "0" ? null : model.ToUserId;
                CaseMovement modelSave = model.Id > 0 ? await repo.All<CaseMovement>()
                                                                  .Where(x => x.Id == model.Id)
                                                                  .FirstAsync() : GetCaseMovement(model);

                if (model.Id > 0)
                {
                    modelSave.MovementTypeId = model.MovementTypeId;
                    modelSave.ToUserId = model.ToUserId;
                    modelSave.CourtOrganizationId = model.CourtOrganizationId;
                    modelSave.OtherInstitution = model.OtherInstitution;
                    modelSave.Description = model.Description;
                }
                else
                    repo.Add(modelSave);

                await repo.SaveChangesAsync();
                model.Id = modelSave.Id;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на движение по дело Id={model.Id}");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за редкация на местоположение
        /// </summary>
        /// <param name="id">Идентификатор на местоположението</param>
        /// <returns></returns>
        public async Task<CaseMovementVM> GetCaseMovementByEdit(int id)
        {
            return await repo.AllReadonly<CaseMovement>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseMovementVM()
                             {
                                 Id = x.Id,
                                 CaseId = x.CaseId,
                                 CourtId = x.CourtId,
                                 MovementTypeId = x.MovementTypeId,
                                 MovementTypeLabel = x.MovementType.Label,
                                 NameFor = (x.MovementTypeId == NomenclatureConstants.CaseMovementType.ToPerson) ? (!string.IsNullOrEmpty(x.ToUserId) ? x.ToUser.LawUnit.FullName :
                                                                                                                                                        string.Empty) :
                                                                                                                   ((x.MovementTypeId == NomenclatureConstants.CaseMovementType.ToOtdel) ? ((x.CourtOrganizationId != null) ? x.CourtOrganization.Label :
                                                                                                                                                                                                                              string.Empty) :
                                                                                                                                                                                           x.OtherInstitution),
                                 ToUserId = x.ToUserId,
                                 CourtOrganizationId = x.CourtOrganizationId,
                                 OtherInstitution = x.OtherInstitution,
                                 DateSend = x.DateSend,
                                 DateAccept = x.DateAccept,
                                 Description = x.Description,
                                 DisableDescription = x.DisableDescription,
                                 AcceptDescription = x.AcceptDescription,
                                 IsActive = x.IsActive,
                                 IsActiveText = x.IsActive ? MessageConstant.Yes : MessageConstant.No,
                                 IsEdit = false,
                                 IsAccept = false,
                                 AcceptUserId = x.AcceptUserId,
                                 AcceptLawUnitName = !string.IsNullOrEmpty(x.AcceptUserId) ? x.AcceptUser.LawUnit.FullName :
                                                                                             string.Empty,
                                 UserId = x.UserId,
                                 UserLawUnitId = x.User.LawUnitId,
                                 UserLawUnitName = x.User.LawUnit.FullName
                             })
                             .FirstAsync();
        }

        /// <summary>
        /// Метод връщащ движение за редакция
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        private async Task<CaseMovement> GetMovementForEdit(int id)
        {
            return await repo.All<CaseMovement>()
                             .Where(x => x.Id == id)
                             .FirstAsync();
        }

        /// <summary>
        /// Сторно на местоположение
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<bool> StornoMovement(CaseMovementVM model)
        {
            try
            {
                model.CourtOrganizationId = model.CourtOrganizationId.EmptyToNull();

                var saved = await GetMovementForEdit(model.Id);
                saved.DisableDescription = model.DisableDescription;
                saved.IsActive = false;

                await repo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при сторниране на движение по дело Id={model.Id}");
                return false;
            }
        }

        /// <summary>
        /// Приемане на местоположение
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<bool> AcceptMovement(int id)
        {
            var saved = await GetMovementForEdit(id);

            if (saved.DateAccept != null)
                return false;

            try
            {
                saved.CourtOrganizationId = saved.CourtOrganizationId.EmptyToNull();
                saved.DateAccept = DateTime.Now;
                saved.AcceptUserId = userContext.UserId;
                await repo.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при приемане на движение по дело Id={saved.Id}");
                return false;
            }
            ;
        }

        /// <summary>
        /// Редакция на приемане на местоположение
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<bool> EditAcceptMovement(CaseMovementVM model)
        {
            try
            {
                model.CourtOrganizationId = model.CourtOrganizationId.EmptyToNull();

                var saved = await GetMovementForEdit(model.Id);
                saved.AcceptDescription = model.AcceptDescription;
                await repo.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при отразяване на изпълнение на движение по дело Id={model.Id}");
                return false;
            }
        }

        /// <summary>
        /// Проверка дали може да се добави местоположение
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<bool> IsAddNewMovement(int caseId)
        {
            var movements = await repo.AllReadonly<CaseMovement>()
                                      .Where(x => x.CaseId == caseId)
                                      .ToListAsync();

            if (!movements.Any())
                return true;

            var maxIdElement = movements.Where(x => x.IsActive)
                                        .OrderByDescending(x => x.Id)
                                        .FirstOrDefault();

            if (maxIdElement != null)
            {
                if (maxIdElement.DateAccept == null)
                    return false;

                if (maxIdElement.AcceptUserId != userContext.UserId)
                {
                    if (userContext.IsUserInRole(Roles.Supervisor) ||
                        userContext.IsUserInRole(Roles.Administrator))
                        return true;
                    else
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Създаване на обратно действие за местоположение
        /// </summary>
        /// <param name="id">Идентификатор на движението</param>
        /// <returns></returns>
        public async Task<int> CreateReturnMovement(int id)
        {
            try
            {
                var movement = await GetMovementForEdit(id);

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

                repo.Add(saved);
                await repo.SaveChangesAsync();
                return saved.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на обратно връщане на движение по дело Id={id}");
                return -1;
            }
        }

        /// <summary>
        /// Връща кюери за движение на дело
        /// </summary>
        /// <returns></returns>
        private IQueryable<CaseMovement> GetCaseMovementQuery()
        {
            var courtLawUnit = repo.AllReadonly<CourtLawUnit>()
                                   .Where(x => x.CourtId == userContext.CourtId &&
                                               x.LawUnitId == userContext.LawUnitId &&
                                               x.DateFrom <= DateTime.Now &&
                                               (x.DateTo ?? DateTime.Now.AddDays(1)) >= DateTime.Now &&
                                               x.DateExpired == null)
                                   .FirstOrDefault();


            var userId = userContext.UserId;
            var courtOrganizationId = (courtLawUnit != null) ? (courtLawUnit.CourtOrganizationId ?? 0) : 0;

            Expression<Func<CaseMovement, bool>> filterUserOrganization = x => x.ToUserId == userId;
            if (courtOrganizationId > 0)
                filterUserOrganization = x => x.ToUserId == userId ||
                                              x.CourtOrganizationId == courtOrganizationId;

            var caseMovementQuery = repo.AllReadonly<CaseMovement>();

            return caseMovementQuery.Where(x => x.Case.CourtId == userContext.CourtId)
                                    .Where(x => x.DateAccept == null)
                                    .Where(x => x.IsActive)
                                    .Where(x => x.Id == caseMovementQuery.Where(m => m.CaseId == x.CaseId &&
                                                                                     m.IsActive)
                                                                         .OrderByDescending(m => m.Id)
                                                                         .Select(m => m.Id)
                                                                         .FirstOrDefault())
                                    .Where(filterUserOrganization);
        }

        /// <summary>
        /// Метод извличащ данни за компонента
        /// </summary>
        /// <returns></returns>
        public IQueryable<CaseMovementVM> Select_ToDoForComponent()
        {
            var caseMovmentQuery = GetCaseMovementQuery();

            return caseMovmentQuery.OrderByDescending(x => x.DateSend)
                                   .Select(x => new CaseMovementVM()
                                   {
                                       Id = x.Id,
                                       CaseId = x.CaseId,
                                       CourtId = x.CourtId,
                                       CaseName = $"{x.Case.CaseType.Code} {x.Case.ShortNumber}/{x.Case.RegDate:yyyy}",
                                       DateSend = x.DateSend,
                                       UserId = x.UserId,
                                       UserLawUnitId = x.User.LawUnitId,
                                       UserLawUnitName = x.User.LawUnit.FullName
                                   });
        }

        /// <summary>
        /// Извличане на данни за местоположение за начален екран
        /// </summary>
        /// <returns></returns>
        public IQueryable<CaseMovementVM> Select_ToDo()
        {
            var caseMovmentQuery = GetCaseMovementQuery();
            return caseMovmentQuery.Select(x => new CaseMovementVM()
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
                UserLawUnitName = x.User.LawUnit.FullName,
                ViewUrl = urlHelper.Action("Index", "CaseMovement", new { CaseId = x.CaseId })
            })
                                   .AsQueryable();
        }

        /// <summary>
        /// Извличанена бройки за начален екран
        /// </summary>
        /// <returns></returns>
        public async Task<int> Select_ToDoCount()
        {
            var caseMovmentQuery = GetCaseMovementQuery();
            return await caseMovmentQuery.CountAsync();
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
            Expression<Func<CaseMovement, bool>> caseRegNumWhere = x => true;
            if (!string.IsNullOrEmpty(CaseRegNum))
                caseRegNumWhere = x => EF.Functions.ILike(x.Case.RegNumber, CaseRegNum.ToCasePaternSearch());

            Expression<Func<CaseMovement, bool>> userIdWhere = x => true;
            if (!string.IsNullOrEmpty(UserId))
                userIdWhere = x => x.AcceptUserId == UserId || x.ToUserId == UserId;

            return repo.AllReadonly<CaseMovement>()
                       .Where(x => x.Case.CourtId == userContext.CourtId)
                       .Where(caseRegNumWhere)
                       .Where(userIdWhere)
                       .Select(x => new CaseMovementVM()
                       {
                           Id = x.Id,
                           CaseId = x.CaseId,
                           CourtId = x.CourtId,
                           CaseName = x.Case.RegNumber,
                           CaseRegDate = x.Case.RegDate,
                           MovementTypeId = x.MovementTypeId,
                           MovementTypeLabel = x.MovementType.Label,
                           NameFor = (x.MovementTypeId == NomenclatureConstants.CaseMovementType.ToPerson) ? (!string.IsNullOrEmpty(x.ToUserId) ? x.ToUser.LawUnit.FullName :
                                                                                                                                                  string.Empty) :
                                                                                                             (x.MovementTypeId == NomenclatureConstants.CaseMovementType.ToOtdel ? ((x.CourtOrganization != null) ? x.CourtOrganization.Label :
                                                                                                                                                                                                                    string.Empty) :
                                                                                                                                                                                   x.OtherInstitution),
                           ToUserId = x.ToUserId,
                           CourtOrganizationId = x.CourtOrganizationId,
                           OtherInstitution = x.OtherInstitution,
                           DateSend = x.DateSend,
                           DateAccept = x.DateAccept,
                           Description = x.Description,
                           DisableDescription = x.DisableDescription,
                           AcceptDescription = x.AcceptDescription,
                           IsActive = x.IsActive,
                           IsActiveText = x.IsActive ? "Активен" : "Неактивен",
                           IsEdit = false,
                           IsAccept = false,
                           AcceptUserId = x.AcceptUserId,
                           AcceptLawUnitName = !string.IsNullOrEmpty(x.AcceptUserId) ? x.AcceptUser.LawUnit.FullName + (x.MovementTypeId == NomenclatureConstants.CaseMovementType.ToOutStructure ? " (" + x.OtherInstitution + (!string.IsNullOrEmpty(x.Description) ? " - " + x.Description : string.Empty) + ")" : string.Empty) : string.Empty,
                           UserId = x.UserId,
                           UserLawUnitId = x.User.LawUnitId,
                           UserLawUnitName = x.User.LawUnit.FullName + (x.MovementTypeId != NomenclatureConstants.CaseMovementType.ToOutStructure ? (!string.IsNullOrEmpty(x.OtherInstitution) ? " (" + x.OtherInstitution + ")" : string.Empty) : string.Empty)
                       });
        }

        /// <summary>
        /// Извличане на последно местоположение за дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public async Task<string> GetLastMovmentForCaseId(int caseId)
        {
            var caseMovementQuery = repo.AllReadonly<CaseMovement>();

            var caseMovement = await caseMovementQuery.Where(x => x.CaseId == caseId)
                                                      .Where(x => x.IsActive)
                                                      .Select(x => new
                                                      {
                                                          Id = x.Id,
                                                          MovementTypeLabel = x.MovementType.Label,
                                                          NameFor = (x.MovementTypeId == NomenclatureConstants.CaseMovementType.ToPerson) ? (!string.IsNullOrEmpty(x.ToUserId) ? x.ToUser.LawUnit.FullName :
                                                                                                                                                                    string.Empty) :
                                                                                                                               (x.MovementTypeId == NomenclatureConstants.CaseMovementType.ToOtdel ? ((x.CourtOrganization != null) ? x.CourtOrganization.Label :
                                                                                                                                                                                                                                      string.Empty) :
                                                                                                                                                                                                     x.OtherInstitution),
                                                      
                                                          DateAccept = x.DateAccept
                                                      })
                                                      .OrderByDescending(m => m.Id)
                                                      .FirstOrDefaultAsync();

            var result = string.Empty;

            if (caseMovement != null)
                result = "Вид: " + caseMovement.MovementTypeLabel + " - насочено към: " + caseMovement.NameFor + " - " + ((caseMovement.DateAccept != null) ? "Прието" : "Неприето");

            return result;
        }
    }
}
