using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Constants;

namespace IOWebApplication.Core.Services
{
    public class CaseGroupService : BaseService, ICaseGroupService
    {
        public CaseGroupService(ILogger<CaseGroupService> _logger,
            IRepository _repo)
        {
            logger = _logger;
            repo = _repo;
        }

        /// <summary>
        /// Извличане на данни за основни видове дела
        /// </summary>
        /// <returns></returns>
        public IQueryable<CaseGroupVM> CaseGroup_Select()
        {
            return repo.AllReadonly<CaseGroup>()
                .Select(x => new CaseGroupVM()
                {
                    Id = x.Id,
                    OrderNumber = x.OrderNumber,
                    Label = x.Label,
                    IsActive = x.IsActive,
                    DateStart = x.DateStart,
                    DateEnd = x.DateEnd
                }).AsQueryable();
        }

        /// <summary>
        /// Запис на на основен вид дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseGroup_SaveData(CaseGroup model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseGroup>(model.Id);
                    saved.Code = model.Code;
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.IsActive = model.IsActive;
                    saved.DateStart = model.DateStart;
                    saved.DateEnd = model.DateEnd;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    int maxOrderNumber = repo.AllReadonly<CaseGroup>()
                        .Select(x => x.OrderNumber)
                        .DefaultIfEmpty(0)
                        .Max();

                    model.OrderNumber = maxOrderNumber + 1;


                    repo.Add<CaseGroup>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CaseGroup Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за точни видове дела
        /// </summary>
        /// <param name="caseGroupId"></param>
        /// <returns></returns>
        public IQueryable<CaseTypeVM> CaseType_Select(int caseGroupId)
        {
            return repo.AllReadonly<CaseType>()
                .Where(x => x.CaseGroupId == caseGroupId)
                .Select(x => new CaseTypeVM()
                {
                    Id = x.Id,
                    OrderNumber = x.OrderNumber,
                    Label = x.Label,
                    IsActive = x.IsActive,
                    DateStart = x.DateStart,
                    DateEnd = x.DateEnd
                }).AsQueryable();
        }

        /// <summary>
        /// Запис на Точен вид дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseType_SaveData(CaseType model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseType>(model.Id);
                    saved.Code = model.Code;
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.IsActive = model.IsActive;
                    saved.DateStart = model.DateStart;
                    saved.DateEnd = model.DateEnd;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    int maxOrderNumber = repo.AllReadonly<CaseType>()
                        .Where(x => x.CaseGroupId == model.CaseGroupId)
                        .Select(x => x.OrderNumber)
                        .DefaultIfEmpty(0)
                        .Max();

                    model.OrderNumber = maxOrderNumber + 1;


                    repo.Add<CaseType>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CaseType Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за шифри
        /// </summary>
        /// <param name="caseTypeId"></param>
        /// <returns></returns>
        public IQueryable<CaseCodeVM> CaseCode_Select(int caseTypeId)
        {
            Expression<Func<CaseTypeCode, bool>> caseTypeWhere = x => true;
            if (caseTypeId > 0)
            {
                caseTypeWhere = x => x.CaseTypeId == caseTypeId;
            }

            return repo.AllReadonly<CaseTypeCode>()
                       .Include(x => x.CaseCode)
                       .Where(caseTypeWhere)
                       .Select(x => new CaseCodeVM()
                       {
                           Id = x.CaseCode.Id,
                           OrderNumber = x.CaseCode.OrderNumber,
                           Label = x.CaseCode.Label,
                           Code = x.CaseCode.Code,
                           IsActive = x.CaseCode.IsActive,
                           DateStart = x.CaseCode.DateStart,
                           DateEnd = x.CaseCode.DateEnd,
                           LawBaseDescription = x.CaseCode.LawBaseDescription
                       })
                       .GroupBy(x => x.Id)
                       .Select(g => g.FirstOrDefault())
                       .AsQueryable();
        }

        /// <summary>
        /// Запис на шифър
        /// </summary>
        /// <param name="model"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public bool CaseCode_SaveData(CaseCode model, List<int> types)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseCode>(model.Id);
                    saved.Code = model.Code;
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.IsActive = model.IsActive;
                    saved.DateStart = model.DateStart;
                    saved.DateEnd = model.DateEnd;
                    saved.LawBaseDescription = model.LawBaseDescription;

                    //Изтриване на caseTypeCode за това ид
                    var caseTypeCodes = repo.AllReadonly<CaseTypeCode>().Where(x => x.CaseCodeId == model.Id).ToList();
                    repo.DeleteRange(caseTypeCodes);

                    repo.Update(saved);
                }
                else
                {
                    //Insert
                    int maxOrderNumber = repo.AllReadonly<CaseCode>()
                        .Select(x => x.OrderNumber)
                        .DefaultIfEmpty(0)
                        .Max();

                    model.OrderNumber = maxOrderNumber + 1;


                    repo.Add<CaseCode>(model);
                }

                //записва код към типове
                foreach (var item in types)
                {
                    CaseTypeCode newCaseTypeCode = new CaseTypeCode();
                    newCaseTypeCode.CaseTypeId = item;
                    newCaseTypeCode.CaseCodeId = model.Id;
                    repo.Add<CaseTypeCode>(newCaseTypeCode);
                }

                repo.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CaseCode Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Списък шифри за точен вид дело
        /// </summary>
        /// <param name="caseCodeId"></param>
        /// <returns></returns>
        public IQueryable<MultiSelectTransferVM> CaseTypeForSelect_Select(int caseCodeId)
        {
            Expression<Func<CaseTypeCode, bool>> caseCodeWhere = x => true;
            if (caseCodeId > 0)
            {
                caseCodeWhere = x => x.CaseCodeId == caseCodeId;
            }

            return repo.AllReadonly<CaseTypeCode>()
           .Include(x => x.CaseType)
           .Where(caseCodeWhere)
           .Select(x => new MultiSelectTransferVM()
           {
               Id = x.CaseTypeId,
               Order = x.CaseType.OrderNumber,
               Text = x.CaseType.Label
           })
           .GroupBy(x => x.Id)
           .Select(g => g.FirstOrDefault())
           .AsQueryable();
        }

        /// <summary>
        /// Извличане на състави по точен вид дело
        /// </summary>
        /// <param name="caseTypeId"></param>
        /// <returns></returns>
        public IQueryable<CaseTypeUnitVM> CaseTypeUnit_Select(int caseTypeId)
        {
            return repo.AllReadonly<CaseTypeUnit>()
                        .Include(x => x.CaseTypeUnitCounts)
                        .ThenInclude(x => x.JudgeRole)
                        //.Include(x => x.CaseTypeUnitCounts.Select(r => r.JudgeRole))
                        .Where(x => x.CaseTypeId == caseTypeId)
                        .OrderBy(x => x.OrderNumber)
                        .Select(x => new CaseTypeUnitVM()
                        {
                            Id = x.Id,
                            OrderNumber = x.OrderNumber,
                            Label = x.Label,
                            IsActiveLabel = (x.IsActive) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No,
                            DateStart = x.DateStart,
                            Counts = x.CaseTypeUnitCounts.Where(r => r.PersonCount > 0)
                                                    .OrderBy(r => r.JudgeRole.OrderNumber)
                                                    .Select(u =>
                                                    new ListNumberVM
                                                    {
                                                        Label = u.JudgeRole.Label,
                                                        Value = u.PersonCount
                                                    }
                                                    )
                        }).AsQueryable();
        }

        /// <summary>
        /// Извличане на данни за състав
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CaseTypeUnitEditVM GetById_CaseTypeUnit(int id)
        {
            var caseTypeUnitEdits = repo.AllReadonly<CaseTypeUnit>()
                                        .Where(x => x.Id == id)
                                        .Select(x => new CaseTypeUnitEditVM()
                                        {
                                            Id = x.Id,
                                            CaseTypeId = x.CaseTypeId,
                                            Label = x.Label,
                                            Description = x.Description,
                                            DateStart = x.DateStart,
                                            DateEnd = x.DateEnd,
                                            IsActive = x.IsActive,
                                        }).FirstOrDefault();

            if (caseTypeUnitEdits == null)
            {
                return null;
            }

            if (caseTypeUnitEdits.CaseTypeUnitCounts == null)
                caseTypeUnitEdits.CaseTypeUnitCounts = new List<ListNumberVM>();
            caseTypeUnitEdits.CaseTypeUnitCounts = GetList_CaseTypeUnitCounts();
            FillList_CaseTypeUnitCounts(caseTypeUnitEdits.Id, caseTypeUnitEdits.CaseTypeUnitCounts);

            return caseTypeUnitEdits;
        }

        public ICollection<ListNumberVM> GetList_CaseTypeUnitCounts()
        {
            var judgeRoles = repo.AllReadonly<JudgeRole>()
                                    .Where(x => !NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.Id))
                                    .OrderBy(x => x.OrderNumber).ToList();
            List<ListNumberVM> result = new List<ListNumberVM>();

            foreach (var judge in judgeRoles)
            {
                ListNumberVM listNumber = new ListNumberVM()
                {
                    Id = judge.Id,
                    Label = judge.Label,
                    Value = 0
                };

                result.Add(listNumber);
            }

            return result;
        }

        private void FillList_CaseTypeUnitCounts(int CaseTypeUnitId, ICollection<ListNumberVM> listNumbers)
        {
            var caseTypeUnits = repo.AllReadonly<CaseTypeUnitCount>()
                                    .Where(x => x.CaseTypeUnitId == CaseTypeUnitId)
                                    .ToList();

            foreach (var listNumber in listNumbers)
            {
                listNumber.Value = caseTypeUnits.Where(x => x.JudgeRoleId == listNumber.Id).Select(x => x.PersonCount).DefaultIfEmpty(0).FirstOrDefault();
            }
        }

        private List<CaseTypeUnitCount> FillList_TypeUnitCount(List<ListNumberVM> listNumbers, int CaseTypeUnitId)
        {
            List<CaseTypeUnitCount> result = new List<CaseTypeUnitCount>();

            foreach (var listNumber in listNumbers.Where(x => x.Value > 0))
            {
                CaseTypeUnitCount caseTypeUnitCount = new CaseTypeUnitCount()
                {
                    CaseTypeUnitId = CaseTypeUnitId,
                    JudgeRoleId = listNumber.Id,
                    PersonCount = listNumber.Value,
                };

                result.Add(caseTypeUnitCount);
            }

            return result;
        }

        /// <summary>
        /// Запис на състав към точен вид дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseTypeUnit_SaveData(CaseTypeUnitEditVM model)
        {
            try
            {
                var saved = (model.Id > 0) ? repo.GetById<CaseTypeUnit>(model.Id) : new CaseTypeUnit();
                saved.CaseTypeId = model.CaseTypeId;
                saved.Label = model.Label;
                saved.Description = model.Description;
                saved.IsActive = model.IsActive;
                saved.DateStart = model.DateStart;
                saved.DateEnd = model.DateEnd;

                if (model.Id > 0)
                {
                    repo.Update(saved);

                    var caseTypeUnits = repo.AllReadonly<CaseTypeUnitCount>()
                                    .Where(x => x.CaseTypeUnitId == saved.Id)
                                    .ToList();
                    repo.DeleteRange(caseTypeUnits);
                }
                else
                {
                    repo.Add<CaseTypeUnit>(saved);
                    repo.SaveChanges();
                    saved.OrderNumber = saved.Id;
                }

                var caseTypeUnitCounts = FillList_TypeUnitCount(model.CaseTypeUnitCounts.ToList(), saved.Id);
                repo.AddRange(caseTypeUnitCounts);

                repo.SaveChanges();
                model.Id = saved.Id;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на интервал по дело Id={ model.Id }");
                return false;
            }
        }
    }
}
