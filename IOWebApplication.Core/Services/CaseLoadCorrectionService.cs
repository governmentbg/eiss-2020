using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class CaseLoadCorrectionService: BaseService, ICaseLoadCorrectionService
    {
        public CaseLoadCorrectionService(
        ILogger<CaseLoadCorrectionService> _logger,
        IRepository _repo,
        IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }

        /// <summary>
        /// Извличане на данни за Коригиращи индекси за трудност на дело
        /// </summary>
        /// <returns></returns>
        public IQueryable<CaseLoadCorrectionActivityVM> CaseLoadCorrectionActivity_Select()
        {
            return repo.AllReadonly<CaseLoadCorrectionActivity>()
                       .Select(x => new CaseLoadCorrectionActivityVM()
                       {
                           Id = x.Id,
                           CaseGroupLabel = x.CaseGroup.Label,
                           CaseInstanceLabel = x.CaseInstance.Label,
                           LoadIndex = x.LoadIndex,
                           CorrectionGroup = x.CorrectionGroup,
                           Label = x.Label
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Метод за вземане на елемент с най-голямо ИД на Коригиращи индекси за трудност на дело
        /// </summary>
        /// <returns></returns>
        public CaseLoadCorrectionActivity GetMaxId()
        {
            return repo.AllReadonly<CaseLoadCorrectionActivity>()
                       .Include(x => x.CaseGroup)
                       .Include(x => x.CaseInstance)
                       .OrderByDescending(x => x.Id)
                       .FirstOrDefault();
        }

        /// <summary>
        /// Запис на Коригиращи индекси за трудност на дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseLoadCorrectionActivity_SaveData(CaseLoadCorrectionActivity model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLoadCorrectionActivity>(model.Id);
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.IsActive = model.IsActive;
                    saved.CaseGroupId = model.CaseGroupId;
                    saved.CaseInstanceId = model.CaseInstanceId;
                    saved.CorrectionGroup = model.CorrectionGroup;
                    saved.LoadIndex = model.LoadIndex;
                    saved.DateEnd = model.DateEnd.ForceEndDate();
                    saved.DateStart = model.DateStart.ForceStartDate();
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    repo.Add<CaseLoadCorrectionActivity>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Коригиращи индекси за трудност на дело Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за Коригиращи индекси за трудност на дело
        /// </summary>
        /// <param name="CaseLoadCorrectionActivityId"></param>
        /// <returns></returns>
        public IQueryable<CaseLoadCorrectionActivityIndexVM> CaseLoadCorrectionActivityIndex_Select(int CaseLoadCorrectionActivityId)
        {
            return repo.AllReadonly<CaseLoadCorrectionActivityIndex>()
                       .Where(x => x.CaseLoadCorrectionActivityId == CaseLoadCorrectionActivityId)
                       .Select(x => new CaseLoadCorrectionActivityIndexVM()
                       {
                           Id = x.Id,
                           CaseInstanceLabel = x.CaseInstance.Label,
                           LoadIndex = x.LoadIndex
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Запис на Коригиращи индекси за трудност на дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseLoadCorrectionActivityIndex_SaveData(CaseLoadCorrectionActivityIndex model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLoadCorrectionActivityIndex>(model.Id);
                    saved.CaseLoadCorrectionActivityId = model.CaseLoadCorrectionActivityId;
                    saved.CaseInstanceId = model.CaseInstanceId;
                    saved.LoadIndex = model.LoadIndex;
                    saved.IsActive = model.IsActive;
                    saved.DateEnd = model.DateEnd.ForceEndDate();
                    saved.DateStart = model.DateStart.ForceStartDate();
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    repo.Add<CaseLoadCorrectionActivityIndex>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Коригиращи индекси за трудност на дело Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за Коригиращи коефициенти по дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public IQueryable<CaseLoadCorrectionVM> CaseLoadCorrection_Select(int CaseId)
        {
            return repo.AllReadonly<CaseLoadCorrection>()
                       .Where(x => x.CaseId == CaseId && x.DateExpired == null)
                       .Select(x => new CaseLoadCorrectionVM()
                       {
                           Id = x.Id,
                           CorrectionDate = x.CorrectionDate,
                           CaseLoadCorrectionActivityLabel = x.CaseLoadCorrectionActivity.Label,
                           CorrectionLoadIndex = x.CorrectionLoadIndex
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Извличане на индекс
        /// </summary>
        /// <param name="CaseLoadCorrectionActivityId"></param>
        /// <returns></returns>
        private async Task<decimal> GetLoadCorrection(int CaseLoadCorrectionActivityId)
        {
            var caseLoadCorrectionActivity = await repo.GetByIdAsync<CaseLoadCorrectionActivity>(CaseLoadCorrectionActivityId);
            return (caseLoadCorrectionActivity != null) ? caseLoadCorrectionActivity.LoadIndex : 0;
        }

        /// <summary>
        /// Сума на индекси по дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        private decimal GetSumCorrectionForCaseId(int CaseId)
        {
            return repo.AllReadonly<CaseLoadCorrection>()
                       .Where(x => x.CaseId == CaseId && x.DateExpired == null)
                       .Sum(x => x.CorrectionLoadIndex);
        }

        /// <summary>
        /// Запис на Коригиращи коефициенти по дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> CaseLoadCorrection_SaveData(CaseLoadCorrection model)
        {
            try
            {
                var caseSave = await repo.GetByIdAsync<Case>(model.CaseId);

                if (model.Id > 0)
                {
                    //Update
                    var saved = await repo.GetByIdAsync<CaseLoadCorrection>(model.Id);
                    saved.CaseId = model.CaseId;
                    saved.CorrectionDate = model.CorrectionDate;

                    var _changeCorrection = false;
                    if (saved.CaseLoadCorrectionActivityId != model.CaseLoadCorrectionActivityId)
                    {
                        saved.CaseLoadCorrectionActivityId = model.CaseLoadCorrectionActivityId;
                        saved.CorrectionLoadIndex = await GetLoadCorrection(model.CaseLoadCorrectionActivityId);
                        _changeCorrection = true;
                    }

                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    repo.Update(saved);
                    await repo.SaveChangesAsync();

                    if (_changeCorrection)
                    {
                        caseSave.CorrectionLoadIndex = await GetCaseLoadCorrectionAsync(saved.CaseId); 
                        caseSave.DateWrt = DateTime.Now;
                        caseSave.UserId = userContext.UserId;
                        repo.Update(caseSave);
                        await repo.SaveChangesAsync();
                    }
                }
                else
                {
                    model.CorrectionLoadIndex = await GetLoadCorrection(model.CaseLoadCorrectionActivityId);
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    repo.Add(model);
                    await repo.SaveChangesAsync();

                    caseSave.CorrectionLoadIndex = await GetCaseLoadCorrectionAsync(model.CaseId);
                    caseSave.DateWrt = DateTime.Now;
                    caseSave.UserId = userContext.UserId;
                    repo.Update(caseSave);
                    await repo.SaveChangesAsync();
                }

                // тази част я има и като метод в CaseLoadIndexService
                var caseLoadIndices = await repo.All<CaseLoadIndex>()
                                                .Where(x => x.CaseId == model.CaseId &&
                                                            x.DateExpired == null)
                                                .ToListAsync();

                foreach (var caseLoad in caseLoadIndices)
                {
                    var caseLoadCorrectionIdex = await GetCaseLoadCorrectionToDateAsync(model.CaseId, caseLoad.DateActivity);
                    caseLoad.BaseIndex = caseLoadCorrectionIdex > 0 ? caseSave.LoadIndex * caseLoadCorrectionIdex : caseSave.LoadIndex;
                    caseLoad.DateWrt = DateTime.Now;
                    caseLoad.UserId = userContext.UserId;
                }
                await repo.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Коригиращи коефициенти по дело Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Зареждане в комбо на Коригиращи индекси за трудност на дело
        /// </summary>
        /// <param name="CaseGroupId"></param>
        /// <param name="CaseInstanceId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_CaseLoadCorrectionActivity(int CaseGroupId, int CaseInstanceId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = repo.AllReadonly<CaseLoadCorrectionActivity>()
                                        .Where(x => (x.CaseGroupId == CaseGroupId) &&
                                                    (x.CaseInstanceId == CaseInstanceId) &&
                                                    ((x.DateStart <= DateTime.Now) && ((x.DateEnd ?? DateTime.Now.AddYears(100)) >= DateTime.Now)))
                                        .Select(x => new SelectListItem()
                                        {
                                            Text = x.Label,
                                            Value = x.Id.ToString()
                                        })
                                        .OrderBy(x => x.Text)
                                        .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Зареждане в комбо на Коригиращи индекси за трудност на дело
        /// </summary>
        /// <param name="CaseGroupId"></param>
        /// <param name="CaseInstanceId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_CaseLoadCorrectionActivityAsync(int CaseGroupId, int CaseInstanceId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = await repo.AllReadonly<CaseLoadCorrectionActivity>()
                                            .Where(x => (x.CaseGroupId == CaseGroupId) &&
                                                        (x.CaseInstanceId == CaseInstanceId) &&
                                                        ((x.DateStart <= DateTime.Now) && ((x.DateEnd ?? DateTime.Now.AddYears(100)) >= DateTime.Now)))
                                            .Select(x => new SelectListItem()
                                            {
                                                Text = x.Label,
                                                Value = x.Id.ToString()
                                            })
                                            .OrderBy(x => x.Text)
                                            .ToListAsync() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Проверка дали съществува Коригиращи коефициенти по дело в дело
        /// </summary>
        /// <param name="ModelId"></param>
        /// <param name="CaseId"></param>
        /// <param name="CaseLoadCorrectionActivityId"></param>
        /// <returns></returns>
        public bool IsExistCaseLoadCorrection(int ModelId, int CaseId, int CaseLoadCorrectionActivityId)
        {
            return repo.AllReadonly<CaseLoadCorrection>()
                       .Any(x => x.CaseId == CaseId && x.DateExpired == null &&
                                 ((ModelId > 0) ? x.Id != ModelId : true) &&
                                 x.CaseLoadCorrectionActivityId == CaseLoadCorrectionActivityId);
        }

        public decimal GetCaseLoadCorrection(int CaseId)
        {
            var caseLoadCorrections = repo.AllReadonly<CaseLoadCorrection>()
                                          .Where(x => x.CaseId == CaseId &&
                                                      x.DateExpired == null)
                                          .ToList() ?? new List<CaseLoadCorrection>();

            var result = (decimal)0;
            if (caseLoadCorrections.Count > 0)
            {
                result = caseLoadCorrections.Sum(x => x.CorrectionLoadIndex) - (caseLoadCorrections.Count - 1);
            }

            return result;
        }

        public async Task<decimal> GetCaseLoadCorrectionAsync(int CaseId)
        {
            var caseLoadCorrections = await repo.AllReadonly<CaseLoadCorrection>()
                                                .Where(x => x.CaseId == CaseId &&
                                                            x.DateExpired == null)
                                                .ToListAsync() ?? new List<CaseLoadCorrection>();

            var result = (decimal)0;
            if (caseLoadCorrections.Count > 0)
            {
                result = caseLoadCorrections.Sum(x => x.CorrectionLoadIndex) - (caseLoadCorrections.Count - 1);
            }

            return result;
        }

        public decimal GetCaseLoadCorrectionToDate(int CaseId, DateTime dateTime)
        {
            var caseLoadCorrections = repo.AllReadonly<CaseLoadCorrection>()
                                          .Where(x => x.CaseId == CaseId &&
                                                      x.CorrectionDate.Date <= dateTime.Date &&
                                                      x.DateExpired == null)
                                          .ToList() ?? new List<CaseLoadCorrection>();

            return GetCaseLoadCorrectionToDateFromList(caseLoadCorrections, dateTime);
        }

        public async Task<decimal> GetCaseLoadCorrectionToDateAsync(int CaseId, DateTime dateTime)
        {
            var caseLoadCorrections = await repo.AllReadonly<CaseLoadCorrection>()
                                                .Where(x => x.CaseId == CaseId &&
                                                            x.CorrectionDate.Date <= dateTime.Date &&
                                                            x.DateExpired == null)
                                                .ToListAsync() ?? new List<CaseLoadCorrection>();

            return GetCaseLoadCorrectionToDateFromList(caseLoadCorrections, dateTime);
        }

        public decimal GetCaseLoadCorrectionToDateFromList(List<CaseLoadCorrection> models, DateTime dateTime)
        {
            var caseLoadCorrections = models.Where(x => x.CorrectionDate.Date <= dateTime.Date &&
                                                        x.DateExpired == null)
                                            .ToList() ?? new List<CaseLoadCorrection>();

            var result = (decimal)0;
            if (caseLoadCorrections.Count > 0)
            {
                result = caseLoadCorrections.Sum(x => x.CorrectionLoadIndex) - (caseLoadCorrections.Count - 1);
            }

            return result;
        }
        
        public bool IsExistCaseLoadIndex(int CaseId, DateTime dateTime)
        {
            return repo.AllReadonly<CaseLoadIndex>()
                       .Any(x => x.CaseId == CaseId &&
                                 x.DateExpired == null &&
                                 x.DateActivity >= dateTime);
        }
    }
}
