using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace IOWebApplication.Core.Services
{
    public class CaseLoadIndexService : BaseService, ICaseLoadIndexService
    {
        private readonly IPriceService priceService;
        private readonly ICaseLoadCorrectionService caseLoadCorrectionService;
        private readonly IConfiguration configuration;

        public CaseLoadIndexService(ILogger<CaseLoadIndexService> _logger,
                                    IPriceService _priceService,
                                    IRepository _repo,
                                    IConfiguration _configuration,
                                    ICaseLoadCorrectionService _caseLoadCorrectionService,
                                    IUserContext _userContext,
                                    IReadonlyRepository _readonlyrepo)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            priceService = _priceService;
            configuration = _configuration;
            caseLoadCorrectionService = _caseLoadCorrectionService;
            readonlyrepo = _readonlyrepo;
        }

        #region Case Load Index

        /// <summary>
        /// Извличане на данни за Натовареност по дела: основни и допълнителни дейности
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        public IQueryable<CaseLoadIndexVM> CaseLoadIndex_Select(int CaseId, int? CaseSessionId)
        {
            var result = new List<CaseLoadIndexVM>();
            var caseLoadIndices = repo.AllReadonly<CaseLoadIndex>()
                                      .Include(x => x.LawUnit)
                                      .Include(x => x.CaseLoadElementGroup)
                                      .Include(x => x.CaseLoadElementType)
                                      .Include(x => x.CaseLoadAddActivity)
                                      .Include(x => x.Case)
                                      .Where(x => x.CaseId == CaseId &&
                                                  (CaseSessionId != null ? x.CaseSessionId == CaseSessionId : true) &&
                                                  x.DateExpired == null)
                                      .ToList();

            foreach (var caseLoad in caseLoadIndices)
            {
                var caseLoadIndex = new CaseLoadIndexVM()
                {
                    Id = caseLoad.Id,
                    CaseId = caseLoad.CaseId,
                    CaseSessionId = caseLoad.CaseSessionId,
                    BaseIndex = caseLoad.BaseIndex,
                    LawUnitId = caseLoad.LawUnitId,
                    LawUnitName = caseLoad.LawUnit.FullName,
                    LoadValue = (caseLoad.IsMainActivity) ? caseLoad.LoadProcent.ToString("0.00", CultureInfo.InvariantCulture) + "%" : caseLoad.LoadIndex.ToString("0.00", CultureInfo.InvariantCulture),
                    NameActivity = (caseLoad.IsMainActivity) ? caseLoad.CaseLoadElementGroup.Label + " - " + caseLoad.CaseLoadElementType.Label : (caseLoad.CaseLoadElementGroupId != null ? (caseLoad.CaseLoadElementGroup.Label + " - " + caseLoad.CaseLoadElementType.Label) : caseLoad.CaseLoadAddActivity.Label),
                    CalcValue = (caseLoad.IsMainActivity) ? Math.Round(caseLoad.BaseIndex * (caseLoad.LoadProcent / 100), 2, MidpointRounding.AwayFromZero) : caseLoad.LoadIndex,
                    IsMainActivity = caseLoad.IsMainActivity,
                    CaseLoadCorrectionIdex = caseLoadCorrectionService.GetCaseLoadCorrectionToDate(caseLoad.CaseId, caseLoad.DateActivity),
                    CaseLoadIndex = caseLoad.Case.LoadIndex,
                    IsMainActivityText = (caseLoad.IsMainActivity) ? "Основна" : "Допълнителна"
                };

                result.Add(caseLoadIndex);
            }

            foreach (var caseLoadIndexVM in result)
            {
                caseLoadIndexVM.CaseCalcValue = result.Where(x => x.LawUnitId == caseLoadIndexVM.LawUnitId).Sum(x => x.CalcValue);
            }

            return result.AsQueryable();
        }

        public IQueryable<CaseLoadIndexNewVM> CaseLoadIndexNew_Select(int CaseId, int? CaseSessionId)
        {
            var result = new List<CaseLoadIndexNewVM>();
            var caseLoadIndices = repo.AllReadonly<CaseLoadIndex>()
                                      .Include(x => x.LawUnit)
                                      .Include(x => x.CaseLoadElementGroup)
                                      .Include(x => x.CaseLoadElementType)
                                      .Include(x => x.CaseLoadAddActivity)
                                      .Include(x => x.Case)
                                      .Where(x => x.CaseId == CaseId &&
                                                  (CaseSessionId != null ? x.CaseSessionId == CaseSessionId : true) &&
                                                  x.DateExpired == null)
                                      .ToList();

            foreach (var caseLoad in caseLoadIndices.OrderBy(x => x.DateActivity))
            {
                var caseLoadIndex = new CaseLoadIndexNewVM()
                {
                    Id = caseLoad.Id,
                    CaseId = caseLoad.CaseId,
                    CaseSessionId = caseLoad.CaseSessionId,
                    LawUnitId = caseLoad.LawUnitId,
                    LawUnitName = caseLoad.LawUnit.FullName,
                    DateActivity = caseLoad.DateActivity,
                    NameActivityType = (caseLoad.IsMainActivity) ? "Основна" : "Допълнителна",
                    NameActivity = (caseLoad.IsMainActivity) ? caseLoad.CaseLoadElementGroup.Label + " - " + caseLoad.CaseLoadElementType.Label : (caseLoad.CaseLoadElementGroupId != null ? (caseLoad.CaseLoadElementGroup.Label + " - " + caseLoad.CaseLoadElementType.Label) : caseLoad.CaseLoadAddActivity.Label),
                    CaseLoadIndexBegin = (caseLoad.IsMainActivity) ? caseLoad.Case.LoadIndex.ToString("0.00", CultureInfo.InvariantCulture) : "-",
                    CaseLoadCorrectionIdex = (caseLoad.IsMainActivity) ? (caseLoadCorrectionService.GetCaseLoadCorrectionToDate(caseLoad.CaseId, caseLoad.DateActivity)).ToString("0.00", CultureInfo.InvariantCulture) : "-",
                    BaseIndex = (caseLoad.IsMainActivity) ? caseLoad.BaseIndex.ToString("0.00", CultureInfo.InvariantCulture) : "-",
                    LoadValue = (caseLoad.IsMainActivity) ? caseLoad.LoadProcent.ToString("0.00", CultureInfo.InvariantCulture) + "%" : caseLoad.LoadIndex.ToString("0.00", CultureInfo.InvariantCulture),
                    CalcValue = (caseLoad.IsMainActivity) ? Math.Round(caseLoad.BaseIndex * (caseLoad.LoadProcent / 100), 2, MidpointRounding.AwayFromZero) : caseLoad.LoadIndex,
                    CalcValueText = (caseLoad.IsMainActivity) ? Math.Round(caseLoad.BaseIndex * (caseLoad.LoadProcent / 100), 2, MidpointRounding.AwayFromZero).ToString("0.00", CultureInfo.InvariantCulture) : caseLoad.LoadIndex.ToString("0.00", CultureInfo.InvariantCulture),
                };

                result.Add(caseLoadIndex);
            }

            int _order = 0;
            foreach (var caseLoadIndexVM in result.OrderBy(x => x.DateActivity))
            {
                _order++;
                caseLoadIndexVM.Order = _order;
            }

            foreach (var caseLoadIndexVM in result.OrderBy(x => x.Order))
            {
                var c = result.Where(x => x.LawUnitId == caseLoadIndexVM.LawUnitId && x.Order <= caseLoadIndexVM.Order).Select(x => x.DateActivity.Ticks).ToList();
                caseLoadIndexVM.CalcValueLawUnit = result.Where(x => x.LawUnitId == caseLoadIndexVM.LawUnitId && x.Order <= caseLoadIndexVM.Order).Sum(x => x.CalcValue);
                caseLoadIndexVM.CalcValueCase = result.Where(x => x.Order <= caseLoadIndexVM.Order).Sum(x => x.CalcValue);
                caseLoadIndexVM.CalcValueLawUnitText = caseLoadIndexVM.CalcValueLawUnit.ToString("0.00", CultureInfo.InvariantCulture);
                caseLoadIndexVM.CalcValueCaseText = caseLoadIndexVM.CalcValueCase.ToString("0.00", CultureInfo.InvariantCulture);
            }

            if (CaseSessionId == null)
            {
                var caseLoadCorrections = repo.AllReadonly<CaseLoadCorrection>()
                                              .Include(x => x.CaseLoadCorrectionActivity)
                                              .Include(x => x.Case)
                                              .Where(x => x.CaseId == CaseId &&
                                                          x.DateExpired == null)
                                              .ToList();


                foreach (var caseLoadCorrection in caseLoadCorrections.OrderBy(x => x.CorrectionDate))
                {
                    var loadCorrections = caseLoadCorrections.Where(x => x.CorrectionDate <= caseLoadCorrection.CorrectionDate).ToList();
                    var loadCorrection = loadCorrections.Sum(x => x.CorrectionLoadIndex) - (loadCorrections.Count - 1);
                    var caseLoad = new CaseLoadIndexNewVM()
                    {
                        Id = caseLoadCorrection.Id,
                        CaseId = caseLoadCorrection.CaseId,
                        CaseSessionId = null,
                        LawUnitId = 0,
                        LawUnitName = string.Empty,
                        DateActivity = caseLoadCorrection.CorrectionDate,
                        NameActivityType = "Коригиращ (увеличаващ) коефициент",
                        NameActivity = caseLoadCorrection.CaseLoadCorrectionActivity.Label,
                        CaseLoadIndexBegin = caseLoadCorrection.Case.LoadIndex.ToString("0.00", CultureInfo.InvariantCulture),
                        CaseLoadCorrectionIdex = caseLoadCorrection.CorrectionLoadIndex.ToString("0.00", CultureInfo.InvariantCulture),
                        BaseIndex = loadCorrection > 0 ? (caseLoadCorrection.Case.LoadIndex * loadCorrection).ToString("0.00", CultureInfo.InvariantCulture) : caseLoadCorrection.Case.LoadIndex.ToString("0.00", CultureInfo.InvariantCulture),
                        LoadValue = "-",
                        CalcValueText = "-",
                        CalcValueLawUnitText = "-",
                        CalcValueCaseText = "-",
                    };

                    result.Add(caseLoad);
                }
            }

            return result.OrderBy(x => x.DateActivity).ThenBy(x => x.Id).AsQueryable();
        }

        public CaseLoadIndexVM CaseLoadIndexVM_ByID(int id)
        {
            return repo.AllReadonly<CaseLoadIndex>()
                       .Where(x => x.Id == id)
                       .Select(x => new CaseLoadIndexVM()
                       {
                           Id = x.Id,
                           CaseId = x.CaseId,
                           CaseName = x.Case.CaseType.Code + " " + x.Case.ShortNumber + "/" + x.Case.RegDate.ToString("dd.MM.yyyy"),
                           CaseSessionId = x.CaseSessionId,
                           BaseIndex = x.BaseIndex,
                           LawUnitId = x.LawUnitId,
                           LawUnitName = x.LawUnit.FullName,
                           LoadValue = (x.IsMainActivity) ? x.LoadProcent.ToString("0.00") + "%" : x.LoadIndex.ToString("0.00"),
                           NameActivity = (x.IsMainActivity) ? x.CaseLoadElementGroup.Label + " - " + x.CaseLoadElementType.Label : x.CaseLoadAddActivity.Label,
                           CalcValue = (x.IsMainActivity) ? Math.Round(x.BaseIndex * (x.LoadProcent / 100), 2, MidpointRounding.AwayFromZero) : x.LoadIndex,
                           IsMainActivity = x.IsMainActivity,
                           IsMainActivityText = x.IsMainActivity ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No
                       })
                       .FirstOrDefault();
        }



        /// <summary>
        /// Проверка за съществуване в дело на Натовареност по дела: основни и допълнителни дейности
        /// </summary>
        /// <param name="ModelId"></param>
        /// <param name="CaseId"></param>
        /// <param name="isMainActivity"></param>
        /// <param name="JudgeRepLawUnitId"></param>
        /// <param name="caseLoadElementTypeId"></param>
        /// <param name="caseLoadAddActivityId"></param>
        /// <returns></returns>
        public bool IsExistCaseLoadActivity(int ModelId, int CaseId, bool isMainActivity, int JudgeRepLawUnitId, int? caseLoadElementTypeId, int? caseLoadAddActivityId)
        {
            if (!isMainActivity)
            {
                return repo.AllReadonly<CaseLoadIndex>()
                           .Any(x => (x.CaseId == CaseId) &&
                                     (x.DateExpired == null) &&
                                     ((ModelId > 0) ? x.Id != ModelId : true) &&
                                     (x.LawUnitId == JudgeRepLawUnitId) &&
                                     ((isMainActivity) ? x.CaseLoadElementTypeId == caseLoadElementTypeId : x.CaseLoadAddActivityId == caseLoadAddActivityId));
            }
            else
            {
                var _dateNow = DateTime.Now;
                var listInt = new List<int>();
                listInt.Add(caseLoadElementTypeId ?? 0);
                listInt.AddRange(repo.AllReadonly<CaseLoadElementType>()
                                     .Where(x => x.ReplaceCaseLoadElementTypeId == caseLoadElementTypeId &&
                                                 x.IsActive &&
                                                 x.DateStart <= _dateNow &&
                                                 (x.DateEnd ?? _dateNow.AddYears(100)) >= _dateNow)
                                     .Select(x => x.Id)
                                     .ToList() ?? new List<int>());
                listInt.AddRange(repo.AllReadonly<CaseLoadElementTypeStop>()
                                     .Where(x => x.CaseLoadElementTypeId == caseLoadElementTypeId && x.DateExpired == null)
                                     .Select(x => x.CaseLoadElementTypeStopId)
                                     .ToList() ?? new List<int>());

                return repo.AllReadonly<CaseLoadIndex>()
                           .Any(x => (x.CaseId == CaseId) &&
                                     (x.DateExpired == null) &&
                                     ((ModelId > 0) ? x.Id != ModelId : true) &&
                                     (x.LawUnitId == JudgeRepLawUnitId) &&
                                     listInt.Contains(x.CaseLoadElementTypeId ?? 0));
            }
        }

        private bool IsExistCaseLoadActivityInListSave(List<CaseLoadIndex> caseLoadIndicesSave, bool isMainActivity, int JudgeRepLawUnitId, int? caseLoadElementTypeId, int? caseLoadAddActivityId)
        {
            var listInt = new List<int>();
            var caseLoadElementTypeIds = new List<int>();
            var caseLoadElementTypeStopIds = new List<int>();
            if (isMainActivity)
            {
                var _dateNow = DateTime.Now;
                listInt.Add(caseLoadElementTypeId ?? 0);
                listInt.AddRange(repo.AllReadonly<CaseLoadElementType>()
                                     .Where(x => x.ReplaceCaseLoadElementTypeId == caseLoadElementTypeId &&
                                                 x.IsActive &&
                                                 x.DateStart <= _dateNow &&
                                                 (x.DateEnd ?? _dateNow.AddYears(100)) >= _dateNow)
                                     .Select(x => x.Id)
                                     .ToList() ?? new List<int>());
                listInt.AddRange(repo.AllReadonly<CaseLoadElementTypeStop>()
                                     .Where(x => x.CaseLoadElementTypeId == caseLoadElementTypeId && x.DateExpired == null)
                                     .Select(x => x.CaseLoadElementTypeStopId)
                                     .ToList() ?? new List<int>());
            }

            return caseLoadIndicesSave.Any(x => (x.LawUnitId == JudgeRepLawUnitId) && ((isMainActivity) ? listInt.Contains(x.CaseLoadElementTypeId ?? 0) :
                                                                                                          x.CaseLoadAddActivityId == caseLoadAddActivityId));
        }

        public bool IsExistCaseLoadActivity_Additional(int CaseId, int CaseSessionId, int? SessionTypeId, int? CaseSessionActId, int? ActTypeId, int? CaseSessionResultId, int? SessionResultId, bool IsCreatedMotive, int? caseLoadElementTypeId, bool IsSpecialOpinion, int? LawUnitId)
        {
            return repo.AllReadonly<CaseLoadIndex>()
                       .Any(x => !x.IsMainActivity &&
                                 x.CaseId == CaseId &&
                                 x.CaseSessionId == CaseSessionId &&
                                 x.SessionTypeId == SessionTypeId &&
                                 x.CaseSessionActId == CaseSessionActId &&
                                 x.ActTypeId == ActTypeId &&
                                 x.CaseSessionResultId == CaseSessionResultId &&
                                 x.SessionResultId == SessionResultId &&
                                 x.IsCreatedMotive == IsCreatedMotive &&
                                 x.IsSpecialOpinion == IsSpecialOpinion &&
                                 x.CaseLoadElementTypeId == caseLoadElementTypeId &&
                                 x.DateExpired == null &&
                                 (LawUnitId != null ? x.LawUnitId == LawUnitId : true));
        }

        public bool IsExistCaseLoadActivity_Additional(int CaseId, int CaseSessionId, int? caseLoadElementTypeId, int? LawUnitId)
        {
            return repo.AllReadonly<CaseLoadIndex>()
                       .Any(x => !x.IsMainActivity &&
                                 x.CaseId == CaseId &&
                                 x.CaseSessionId == CaseSessionId &&
                                 x.CaseLoadElementTypeId == caseLoadElementTypeId &&
                                 x.DateExpired == null &&
                                 x.LawUnitId == LawUnitId);
        }

        public bool IsExistCaseLoadActivitySpecialOpinion_Additional(int CaseId, int CaseSessionId, int? LawUnitId)
        {
            return repo.AllReadonly<CaseLoadIndex>()
                       .Any(x => !x.IsMainActivity &&
                                 x.CaseId == CaseId &&
                                 x.CaseSessionId == CaseSessionId &&
                                 x.IsSpecialOpinion == true &&
                                 x.DateExpired == null &&
                                 x.LawUnitId == LawUnitId);
        }

        private bool IsExistCaseLoadActivityInListSave_Additional(List<CaseLoadIndex> caseLoadIndicesSave, int CaseSessionId, int? SessionTypeId, int? CaseSessionActId, int? ActTypeId, int? CaseSessionResultId, int? SessionResultId, bool IsCreatedMotive, int? caseLoadElementTypeId, bool IsSpecialOpinion, int? LawUnitId)
        {
            return caseLoadIndicesSave.Any(x => !x.IsMainActivity &&
                                                x.CaseSessionId == CaseSessionId &&
                                                x.SessionTypeId == SessionTypeId &&
                                                x.CaseSessionActId == CaseSessionActId &&
                                                x.ActTypeId == ActTypeId &&
                                                x.CaseSessionResultId == CaseSessionResultId &&
                                                x.SessionResultId == SessionResultId &&
                                                x.IsCreatedMotive == IsCreatedMotive &&
                                                x.IsSpecialOpinion == IsSpecialOpinion &&
                                                x.CaseLoadElementTypeId == caseLoadElementTypeId &&
                                                (LawUnitId != null ? x.LawUnitId == LawUnitId : true));
        }

        private bool IsExistCaseLoadActivityInListSave_Additional(List<CaseLoadIndex> caseLoadIndicesSave, int CaseSessionId, int? caseLoadElementTypeId, int? LawUnitId)
        {
            return caseLoadIndicesSave.Any(x => !x.IsMainActivity &&
                                                x.CaseSessionId == CaseSessionId &&
                                                x.CaseLoadElementTypeId == caseLoadElementTypeId &&
                                                x.LawUnitId == LawUnitId);
        }

        private bool IsExistCaseLoadActivityInListSaveSpecialOpinion_Additional(List<CaseLoadIndex> caseLoadIndicesSave, int CaseSessionId, int? LawUnitId)
        {
            return caseLoadIndicesSave.Any(x => !x.IsMainActivity &&
                                                x.CaseSessionId == CaseSessionId &&
                                                x.IsSpecialOpinion == true &&
                                                x.LawUnitId == LawUnitId);
        }

        /// <summary>
        /// Извличане на Елементи към група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="isMainActivity"></param>
        /// <param name="caseLoadElementTypeId"></param>
        /// <param name="caseLoadAddActivityId"></param>
        /// <param name="courtTypeId"></param>
        /// <returns></returns>
        private decimal GetLoadIndex_CaseLoadIndex(bool isMainActivity, int? caseLoadElementTypeId, int? caseLoadAddActivityId, int? courtTypeId)
        {
            if (isMainActivity)
            {
                var caseLoadElementType = repo.GetById<CaseLoadElementType>(caseLoadElementTypeId);
                return caseLoadElementType.LoadProcent;
            }
            else
            {


                var caseLoad = repo.AllReadonly<CaseLoadAddActivityIndex>().Where(x => x.CaseLoadAddActivityId == caseLoadAddActivityId && x.CourtTypeId == courtTypeId).FirstOrDefault();
                if (caseLoad != null)
                {
                    return caseLoad.LoadIndex;
                }
            }

            return 0;
        }

        /// <summary>
        /// Запис на Натовареност по дела: основни и допълнителни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseLoadIndex_SaveData(CaseLoadIndex model)
        {
            try
            {
                model.DescriptionExpired = null;
                var caseCase = repo.AllReadonly<Case>()
                                   .Include(x => x.Court)
                                   .Where(x => x.Id == model.CaseId)
                                   .FirstOrDefault();

                model.CaseLoadElementGroupId = model.CaseLoadElementGroupId.EmptyToNull(0);
                model.CaseLoadElementTypeId = model.CaseLoadElementTypeId.EmptyToNull(0);
                model.CaseLoadAddActivityId = model.CaseLoadAddActivityId.EmptyToNull(0);

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLoadIndex>(model.Id);
                    saved.LawUnitId = model.LawUnitId;

                    if ((saved.IsMainActivity != model.IsMainActivity) ||
                        (saved.CaseLoadElementGroupId != model.CaseLoadElementGroupId) ||
                        (saved.CaseLoadElementTypeId != model.CaseLoadElementTypeId) ||
                        (saved.CaseLoadAddActivityId != model.CaseLoadAddActivityId))
                    {
                        if (model.IsMainActivity)
                            saved.LoadProcent = GetLoadIndex_CaseLoadIndex(model.IsMainActivity, model.CaseLoadElementTypeId, null, null);
                        else
                            saved.LoadIndex = GetLoadIndex_CaseLoadIndex(model.IsMainActivity, null, model.CaseLoadAddActivityId, caseCase.Court.CourtTypeId);

                        saved.IsMainActivity = model.IsMainActivity;
                        saved.CaseLoadElementGroupId = model.CaseLoadElementGroupId;
                        saved.CaseLoadElementTypeId = model.CaseLoadElementTypeId;
                        saved.CaseLoadAddActivityId = model.CaseLoadAddActivityId;
                        var caseLoadCorrectionIdex = caseLoadCorrectionService.GetCaseLoadCorrectionToDate(model.CaseId, model.DateActivity);
                        saved.BaseIndex = caseLoadCorrectionIdex > 0 ? caseCase.LoadIndex * caseLoadCorrectionIdex : caseCase.LoadIndex;
                    }

                    saved.CaseSessionActId = model.CaseSessionActId;
                    saved.DateActivity = model.DateActivity;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    if (model.IsMainActivity)
                        model.LoadProcent = GetLoadIndex_CaseLoadIndex(model.IsMainActivity, model.CaseLoadElementTypeId, null, null);
                    else
                        model.LoadIndex = GetLoadIndex_CaseLoadIndex(model.IsMainActivity, null, model.CaseLoadAddActivityId, caseCase.Court.CourtTypeId);

                    var caseLoadCorrectionIdex = caseLoadCorrectionService.GetCaseLoadCorrectionToDate(model.CaseId, model.DateActivity);
                    model.BaseIndex = caseLoadCorrectionIdex > 0 ? caseCase.LoadIndex * caseLoadCorrectionIdex : caseCase.LoadIndex;
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    repo.Add<CaseLoadIndex>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Натовареност по дела Id={model.Id}");
                return false;
            }
        }

        private decimal GetLoadIndexFromCaseH(int CaseId, int CourtId, DateTime dateTime)
        {
            var result = (decimal)0.00;

            var caseH = repo.AllReadonly<CaseH>()
                            .Where(x => x.Id == CaseId &&
                                        x.DateWrt <= dateTime &&
                                        x.RegNumber != null &&
                                        x.HistoryDateExpire >= dateTime)
                            .OrderBy(x => x.HistoryId)
                            .FirstOrDefault();

            if (caseH != null)
                return caseH.LoadIndex;

            return result;
        }

        private CaseLoadIndex FillCaseLoadIndex(List<CaseLoadIndex> caseLoadIndices, CaseSession caseSession, CaseSessionAct caseSessionAct, CaseSessionResult caseSessionResult, CaseLawUnit judgeRep, int CaseLoadElementGroupId, int CaseLoadElementTypeId, decimal LoadProcent, int? ReplaceCaseLoadElementTypeId, decimal CaseLoadCorrectionIndex, bool IsMotive)
        {
            var saveCaseLoadIndex = new CaseLoadIndex();

            if (ReplaceCaseLoadElementTypeId != null)
            {
                saveCaseLoadIndex = repo.AllReadonly<CaseLoadIndex>()
                                        .Where(x => x.CaseId == caseSession.CaseId &&
                                                    x.CaseLoadElementTypeId == ReplaceCaseLoadElementTypeId &&
                                                    x.DateExpired == null)
                                        .FirstOrDefault() ?? new CaseLoadIndex();

                if (saveCaseLoadIndex.Id < 1)
                {
                    saveCaseLoadIndex = caseLoadIndices
                                        .Where(x => x.CaseLoadElementTypeId == ReplaceCaseLoadElementTypeId)
                                        .FirstOrDefault() ?? new CaseLoadIndex();
                }
            }

            var loadIndexH = GetLoadIndexFromCaseH(caseSession.CaseId, caseSession.CourtId ?? 0, caseSession.DateFrom);
            var caseLoadIndex = (loadIndexH > 0) ? loadIndexH : caseSession.Case.LoadIndex;

            var dateActivity = caseSessionAct != null ? (IsMotive ? (caseSessionAct.ActMotivesDeclaredDate ?? (caseSessionAct.ActDeclaredDate ?? caseSession.DateFrom)) : (caseSessionAct.ActDeclaredDate ?? caseSession.DateFrom)) :
                                                        caseSession.DateFrom;

            saveCaseLoadIndex.CourtId = caseSession.CourtId;
            saveCaseLoadIndex.CaseId = caseSession.CaseId;
            saveCaseLoadIndex.CaseSessionId = caseSession.Id;
            saveCaseLoadIndex.SessionTypeId = caseSession.SessionTypeId;
            saveCaseLoadIndex.CaseSessionActId = caseSessionAct != null ? caseSessionAct.Id : (int?)null;
            saveCaseLoadIndex.ActTypeId = caseSessionAct != null ? caseSessionAct.ActTypeId : (int?)null;
            saveCaseLoadIndex.CaseSessionResultId = caseSessionResult != null ? caseSessionResult.Id : (int?)null;
            saveCaseLoadIndex.SessionResultId = caseSessionResult != null ? caseSessionResult.SessionResultId : (int?)null;
            saveCaseLoadIndex.LawUnitId = judgeRep.LawUnitId;
            saveCaseLoadIndex.DateActivity = dateActivity;
            saveCaseLoadIndex.IsMainActivity = true;
            saveCaseLoadIndex.CaseLoadElementGroupId = CaseLoadElementGroupId;
            saveCaseLoadIndex.CaseLoadElementTypeId = CaseLoadElementTypeId;
            saveCaseLoadIndex.LoadProcent = LoadProcent;
            saveCaseLoadIndex.BaseIndex = (CaseLoadCorrectionIndex > 0) ? caseLoadIndex * CaseLoadCorrectionIndex : caseLoadIndex;
            saveCaseLoadIndex.DateWrt = DateTime.Now;
            saveCaseLoadIndex.UserId = userContext.UserId;

            return saveCaseLoadIndex;
        }

        private CaseLoadIndex FillCaseLoadIndex_Additional(List<CaseLoadIndex> caseLoadIndices, CaseSession caseSession, CaseSessionAct caseSessionAct, CaseSessionResult caseSessionResult, CaseLawUnit judgeRep, int CaseLoadElementGroupId, int CaseLoadElementTypeId, decimal LoadProcent, int? ReplaceCaseLoadElementTypeId, decimal CaseLoadCorrectionIndex, bool IsCreatedMotive, bool IsSpecialOpinion)
        {
            var saveCaseLoadIndex = new CaseLoadIndex();

            if (ReplaceCaseLoadElementTypeId != null)
            {
                saveCaseLoadIndex = repo.AllReadonly<CaseLoadIndex>()
                                        .Where(x => x.CaseId == caseSession.CaseId &&
                                                    x.CaseLoadElementTypeId == ReplaceCaseLoadElementTypeId &&
                                                    x.DateExpired == null)
                                        .FirstOrDefault() ?? new CaseLoadIndex();

                if (saveCaseLoadIndex.Id < 1)
                {
                    saveCaseLoadIndex = caseLoadIndices
                                        .Where(x => x.CaseLoadElementTypeId == ReplaceCaseLoadElementTypeId)
                                        .FirstOrDefault() ?? new CaseLoadIndex();
                }
            }

            saveCaseLoadIndex.CourtId = caseSession.CourtId;
            saveCaseLoadIndex.CaseId = caseSession.CaseId;
            saveCaseLoadIndex.CaseSessionId = caseSession.Id;
            saveCaseLoadIndex.SessionTypeId = caseSession.SessionTypeId;
            saveCaseLoadIndex.CaseSessionActId = caseSessionAct != null ? caseSessionAct.Id : (int?)null;
            saveCaseLoadIndex.ActTypeId = caseSessionAct != null ? caseSessionAct.ActTypeId : (int?)null;
            saveCaseLoadIndex.CaseSessionResultId = caseSessionResult != null ? caseSessionResult.Id : (int?)null;
            saveCaseLoadIndex.SessionResultId = caseSessionResult != null ? caseSessionResult.SessionResultId : (int?)null;
            saveCaseLoadIndex.LawUnitId = judgeRep.LawUnitId;
            saveCaseLoadIndex.DateActivity = caseSessionAct != null ? (caseSessionAct.ActDeclaredDate ?? caseSession.DateFrom) : caseSession.DateFrom;
            saveCaseLoadIndex.IsMainActivity = false;
            saveCaseLoadIndex.CaseLoadElementGroupId = CaseLoadElementGroupId;
            saveCaseLoadIndex.CaseLoadElementTypeId = CaseLoadElementTypeId;
            saveCaseLoadIndex.LoadIndex = LoadProcent;
            saveCaseLoadIndex.BaseIndex = LoadProcent;
            saveCaseLoadIndex.IsCreatedMotive = IsCreatedMotive;
            saveCaseLoadIndex.IsSpecialOpinion = IsSpecialOpinion;
            saveCaseLoadIndex.DateWrt = DateTime.Now;
            saveCaseLoadIndex.UserId = userContext.UserId;

            return saveCaseLoadIndex;
        }

        private CaseLoadIndex FillCaseLoadIndex(List<CaseLoadIndex> caseLoadIndices, Case caseCase, CaseLawUnit judgeRep, int CaseLoadElementGroupId, int CaseLoadElementTypeId, decimal LoadProcent, int? ReplaceCaseLoadElementTypeId, decimal CaseLoadCorrectionIndex, DateTime dateActive)
        {
            var saveCaseLoadIndex = new CaseLoadIndex();

            if (ReplaceCaseLoadElementTypeId != null)
            {
                saveCaseLoadIndex = repo.AllReadonly<CaseLoadIndex>()
                                        .Where(x => x.CaseId == caseCase.Id &&
                                                    x.CaseLoadElementTypeId == ReplaceCaseLoadElementTypeId &&
                                                    x.DateExpired == null)
                                        .FirstOrDefault() ?? new CaseLoadIndex();

                if (saveCaseLoadIndex.Id < 1)
                {
                    saveCaseLoadIndex = caseLoadIndices
                                        .Where(x => x.CaseLoadElementTypeId == ReplaceCaseLoadElementTypeId)
                                        .FirstOrDefault() ?? new CaseLoadIndex();
                }
            }

            var loadIndexH = GetLoadIndexFromCaseH(caseCase.Id, caseCase.CourtId, dateActive);
            var caseLoadIndex = (loadIndexH > 0) ? loadIndexH : caseCase.LoadIndex;

            saveCaseLoadIndex.CourtId = caseCase.CourtId;
            saveCaseLoadIndex.CaseId = caseCase.Id;
            saveCaseLoadIndex.CaseSessionId = null;
            saveCaseLoadIndex.SessionTypeId = null;
            saveCaseLoadIndex.CaseSessionActId = null;
            saveCaseLoadIndex.ActTypeId = null;
            saveCaseLoadIndex.CaseSessionResultId = null;
            saveCaseLoadIndex.SessionResultId = null;
            saveCaseLoadIndex.LawUnitId = judgeRep.LawUnitId;
            saveCaseLoadIndex.DateActivity = dateActive;
            saveCaseLoadIndex.IsMainActivity = true;
            saveCaseLoadIndex.CaseLoadElementGroupId = CaseLoadElementGroupId;
            saveCaseLoadIndex.CaseLoadElementTypeId = CaseLoadElementTypeId;
            saveCaseLoadIndex.LoadProcent = LoadProcent;
            saveCaseLoadIndex.BaseIndex = (CaseLoadCorrectionIndex > 0) ? caseLoadIndex * CaseLoadCorrectionIndex : caseLoadIndex;
            saveCaseLoadIndex.DateWrt = DateTime.Now;
            saveCaseLoadIndex.UserId = userContext.UserId;

            return saveCaseLoadIndex;
        }

        public bool CaseLoadIndexAutomationElementGroupe_SRA_SaveData(int CaseSessionId, List<CaseLoadElementGroup> loadElementGroups = null)
        {
            //var environmentName = configuration.GetValue<string>("Environment:Name") ?? NomenclatureConstants.Environments.Production;
            //if (environmentName == NomenclatureConstants.Environments.Production)
            //{
            //    return false;
            //}

            var caseSession = repo.AllReadonly<CaseSession>()
                              .Include(x => x.CaseSessionResults)
                              .Include(x => x.CaseSessionActs)
                              .Include(x => x.Case)
                              .ThenInclude(x => x.CaseLawUnits)
                              .Include(x => x.Case)
                              .ThenInclude(x => x.CaseType)
                              .Where(x => x.Id == CaseSessionId)
                              .AsSplitQuery()
                              .FirstOrDefault();

            var judgeRep = caseSession.Case.CaseLawUnits.Where(x => (x.CaseSessionId == null) &&
                                                                    ((x.DateTo ?? caseSession.DateFrom.AddYears(100)).Date >= caseSession.DateFrom.Date) &&
                                                                    (x.DateFrom <= caseSession.DateTo) &&
                                                                    (x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter))
                                                        .OrderByDescending(x => x.DateFrom)
                                                        .FirstOrDefault();
            if (judgeRep == null)
                return false;

            var caseLoadElementGroups = (loadElementGroups == null) ? ReadAllCaseLoadElementGroupByCaseNew(caseSession.CaseId, caseSession.DateFrom) : loadElementGroups;
            if (caseLoadElementGroups.Count < 1)
                return false;

            var caseLoadCorrectionIndex = caseLoadCorrectionService.GetCaseLoadCorrectionToDate(caseSession.CaseId, caseSession.DateFrom);

            var caseLoadIndexSave = new List<CaseLoadIndex>();
            foreach (var caseLoad in caseLoadElementGroups)
            {
                foreach (var caseLoadElementType in caseLoad.CaseLoadElementTypes.OrderBy(x => x.DateStart).ThenBy(x => x.Id))
                {
                    foreach (var caseSessionResult in caseSession.CaseSessionResults.Where(x => x.DateExpired == null).OrderBy(x => x.Id))
                    {
                        foreach (var caseSessionAct in caseSession.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActStateId != NomenclatureConstants.SessionActState.Project).OrderBy(x => x.RegDate))
                        {
                            // Проверка за тип заседание/вид резултат/вид акт
                            if (IsExistCaseLoadElementTypeRules(caseLoadElementType.CaseLoadElementTypeRules,
                                                                caseSession.SessionTypeId,
                                                                caseSessionResult.SessionResultId,
                                                                caseSessionAct.ActTypeId,
                                                                false,
                                                                false,
                                                                false))
                            {
                                if (!IsExistCaseLoadActivity(0, caseSession.CaseId, true, judgeRep.LawUnitId, caseLoadElementType.Id, null) &&
                                    !IsExistCaseLoadActivityInListSave(caseLoadIndexSave, true, judgeRep.LawUnitId, caseLoadElementType.Id, null))
                                {
                                    caseLoadIndexSave.Add(FillCaseLoadIndex(caseLoadIndexSave, caseSession, caseSessionAct, caseSessionResult, judgeRep, caseLoadElementType.CaseLoadElementGroupId, caseLoadElementType.Id, caseLoadElementType.LoadProcent, caseLoadElementType.ReplaceCaseLoadElementTypeId, caseLoadCorrectionIndex, false));
                                }
                            }

                            // Проверка за създаване на мотив
                            if (caseSessionAct.ActMotivesDeclaredDate != null)
                            {
                                // Проверка за тип заседание/вид резултат/вид акт
                                if (IsExistCaseLoadElementTypeRules(caseLoadElementType.CaseLoadElementTypeRules,
                                                                caseSession.SessionTypeId,
                                                                caseSessionResult.SessionResultId,
                                                                caseSessionAct.ActTypeId,
                                                                true,
                                                                false,
                                                                false))
                                {
                                    if (!IsExistCaseLoadActivity(0, caseSession.CaseId, true, judgeRep.LawUnitId, caseLoadElementType.Id, null) &&
                                        !IsExistCaseLoadActivityInListSave(caseLoadIndexSave, true, judgeRep.LawUnitId, caseLoadElementType.Id, null))
                                    {
                                        caseLoadIndexSave.Add(FillCaseLoadIndex(caseLoadIndexSave, caseSession, caseSessionAct, caseSessionResult, judgeRep, caseLoadElementType.CaseLoadElementGroupId, caseLoadElementType.Id, caseLoadElementType.LoadProcent, caseLoadElementType.ReplaceCaseLoadElementTypeId, caseLoadCorrectionIndex, true));
                                    }
                                }

                                // Проверка за тип заседание
                                if (IsExistCaseLoadElementTypeRules(caseLoadElementType.CaseLoadElementTypeRules,
                                                                caseSession.SessionTypeId,
                                                                null,
                                                                null,
                                                                true,
                                                                false,
                                                                false))
                                {
                                    if (!IsExistCaseLoadActivity(0, caseSession.CaseId, true, judgeRep.LawUnitId, caseLoadElementType.Id, null) &&
                                        !IsExistCaseLoadActivityInListSave(caseLoadIndexSave, true, judgeRep.LawUnitId, caseLoadElementType.Id, null))
                                    {
                                        caseLoadIndexSave.Add(FillCaseLoadIndex(caseLoadIndexSave, caseSession, caseSessionAct, null, judgeRep, caseLoadElementType.CaseLoadElementGroupId, caseLoadElementType.Id, caseLoadElementType.LoadProcent, caseLoadElementType.ReplaceCaseLoadElementTypeId, caseLoadCorrectionIndex, true));
                                    }
                                }

                                // Проверка за тип заседание/вид акт
                                if (IsExistCaseLoadElementTypeRules(caseLoadElementType.CaseLoadElementTypeRules,
                                                                caseSession.SessionTypeId,
                                                                null,
                                                                caseSessionAct.ActTypeId,
                                                                true,
                                                                false,
                                                                false))
                                {
                                    if (!IsExistCaseLoadActivity(0, caseSession.CaseId, true, judgeRep.LawUnitId, caseLoadElementType.Id, null) &&
                                        !IsExistCaseLoadActivityInListSave(caseLoadIndexSave, true, judgeRep.LawUnitId, caseLoadElementType.Id, null))
                                    {
                                        caseLoadIndexSave.Add(FillCaseLoadIndex(caseLoadIndexSave, caseSession, caseSessionAct, null, judgeRep, caseLoadElementType.CaseLoadElementGroupId, caseLoadElementType.Id, caseLoadElementType.LoadProcent, caseLoadElementType.ReplaceCaseLoadElementTypeId, caseLoadCorrectionIndex, true));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (caseLoadIndexSave.Count > 0)
            {
                try
                {
                    foreach (var loadIndex in caseLoadIndexSave)
                    {
                        if (loadIndex.Id > 0)
                            repo.Update(loadIndex);
                        else
                            repo.Add<CaseLoadIndex>(loadIndex);
                    }

                    repo.SaveChanges();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Грешка при запис на Натовареност по дела Id={caseSession.CaseId}");
                    return false;
                }
            }

            CaseLoadIndexAutomationElementGroupeAdditional_SRA_SaveData(CaseSessionId);
            return true;
        }

        /// <summary>
        /// Автоматично изчисляване на натовареност за допълнителни дейности
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <param name="loadElementGroups"></param>
        /// <returns></returns>
        public bool CaseLoadIndexAutomationElementGroupeAdditional_SRA_SaveData(int CaseSessionId, List<CaseLoadElementGroup> loadElementGroups = null)
        {
            var caseSession = repo.AllReadonly<CaseSession>()
                              .Include(x => x.CaseSessionResults)
                              .Include(x => x.CaseSessionActs)
                              .ThenInclude(x => x.ActCoordination)
                              .ThenInclude(x => x.CaseLawUnit)
                              .Include(x => x.Case)
                              .ThenInclude(x => x.CaseLawUnits)
                              .Include(x => x.Case)
                              .ThenInclude(x => x.CaseType)
                              .Where(x => x.Id == CaseSessionId)
                              .FirstOrDefault();

            var judgeRep = caseSession.Case.CaseLawUnits.Where(x => (x.CaseSessionId == null) &&
                                                                    ((x.DateTo ?? caseSession.DateFrom.AddYears(100)).Date >= caseSession.DateFrom.Date) &&
                                                                    (x.DateFrom <= caseSession.DateTo) &&
                                                                    (x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter))
                                                        .OrderByDescending(x => x.DateFrom)
                                                        .FirstOrDefault();
            if (judgeRep == null)
                return false;

            var caseLoadElementGroups = (loadElementGroups == null) ? ReadAllCaseLoadElementGroupByCaseNew(caseSession.CaseId, caseSession.DateFrom, true) : loadElementGroups;
            if (caseLoadElementGroups.Count < 1)
                return false;

            var caseLoadCorrectionIndex = caseLoadCorrectionService.GetCaseLoadCorrectionToDate(caseSession.CaseId, caseSession.DateFrom);

            var caseLoadIndexSave = new List<CaseLoadIndex>();
            foreach (var caseLoad in caseLoadElementGroups)
            {
                foreach (var caseLoadElementType in caseLoad.CaseLoadElementTypes.OrderBy(x => x.DateStart).ThenBy(x => x.Id))
                {
                    foreach (var caseSessionResult in caseSession.CaseSessionResults.Where(x => x.DateExpired == null).OrderBy(x => x.Id))
                    {
                        foreach (var caseSessionAct in caseSession.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActStateId != NomenclatureConstants.SessionActState.Project).OrderBy(x => x.RegDate))
                        {
                            // Проверка за тип заседание/вид резултат/вид акт
                            if (IsExistCaseLoadElementTypeRules(caseLoadElementType.CaseLoadElementTypeRules,
                                                                caseSession.SessionTypeId,
                                                                caseSessionResult.SessionResultId,
                                                                caseSessionAct.ActTypeId,
                                                                false,
                                                                false,
                                                                false))
                            {
                                //if (!IsExistCaseLoadActivity_Additional(caseSession.CaseId, caseSession.Id, caseSession.SessionTypeId, caseSessionAct.Id, caseSessionAct.ActTypeId, caseSessionResult.Id, caseSessionResult.SessionResultId, false, caseLoadElementType.Id, false, null) &&
                                //    !IsExistCaseLoadActivityInListSave_Additional(caseLoadIndexSave, caseSession.Id, caseSession.SessionTypeId, caseSessionAct.Id, caseSessionAct.ActTypeId, caseSessionResult.Id, caseSessionResult.SessionResultId, false, caseLoadElementType.Id, false, null))
                                if (!IsExistCaseLoadActivity_Additional(caseSession.CaseId, caseSession.Id, caseLoadElementType.Id, judgeRep.LawUnitId) &&
                                    !IsExistCaseLoadActivityInListSave_Additional(caseLoadIndexSave, caseSession.Id, caseLoadElementType.Id, judgeRep.LawUnitId))
                                {
                                    caseLoadIndexSave.Add(FillCaseLoadIndex_Additional(caseLoadIndexSave, caseSession, caseSessionAct, caseSessionResult, judgeRep, caseLoadElementType.CaseLoadElementGroupId, caseLoadElementType.Id, caseLoadElementType.LoadProcent, caseLoadElementType.ReplaceCaseLoadElementTypeId, caseLoadCorrectionIndex, false, false));
                                }
                            }

                            if (IsExistCaseLoadElementTypeRules(caseLoadElementType.CaseLoadElementTypeRules,
                                                                caseSession.SessionTypeId,
                                                                null,
                                                                null,
                                                                false,
                                                                false,
                                                                true))
                            {
                                foreach (var actCoordination in caseSessionAct.ActCoordination.Where(x => x.ActCoordinationTypeId == NomenclatureConstants.ActCoordinationTypes.AcceptWithOpinion))
                                {
                                    if ((actCoordination.CaseLawUnit.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter) ||
                                        (actCoordination.CaseLawUnit.JudgeRoleId == NomenclatureConstants.JudgeRole.Judge))
                                    {
                                        if (!IsExistCaseLoadActivitySpecialOpinion_Additional(caseSession.CaseId, caseSession.Id, actCoordination.CaseLawUnit.LawUnitId) &&
                                        !IsExistCaseLoadActivityInListSaveSpecialOpinion_Additional(caseLoadIndexSave, caseSession.Id, actCoordination.CaseLawUnit.LawUnitId))
                                        {
                                            caseLoadIndexSave.Add(FillCaseLoadIndex_Additional(caseLoadIndexSave, caseSession, caseSessionAct, null, actCoordination.CaseLawUnit, caseLoadElementType.CaseLoadElementGroupId, caseLoadElementType.Id, caseLoadElementType.LoadProcent, caseLoadElementType.ReplaceCaseLoadElementTypeId, caseLoadCorrectionIndex, false, true));
                                        }
                                    }
                                }
                            }

                            // Проверка за създаване на мотив
                            if (caseSessionAct.ActMotivesDeclaredDate != null)
                            {
                                // Проверка за тип заседание/вид резултат/вид акт
                                if (IsExistCaseLoadElementTypeRules(caseLoadElementType.CaseLoadElementTypeRules,
                                                                caseSession.SessionTypeId,
                                                                caseSessionResult.SessionResultId,
                                                                caseSessionAct.ActTypeId,
                                                                true,
                                                                false,
                                                                false))
                                {
                                    if (!IsExistCaseLoadActivity_Additional(caseSession.CaseId, caseSession.Id, caseSession.SessionTypeId, caseSessionAct.Id, caseSessionAct.ActTypeId, caseSessionResult.Id, caseSessionResult.SessionResultId, true, caseLoadElementType.Id, false, null) &&
                                    !IsExistCaseLoadActivityInListSave_Additional(caseLoadIndexSave, caseSession.Id, caseSession.SessionTypeId, caseSessionAct.Id, caseSessionAct.ActTypeId, caseSessionResult.Id, caseSessionResult.SessionResultId, true, caseLoadElementType.Id, false, null))
                                    {
                                        caseLoadIndexSave.Add(FillCaseLoadIndex_Additional(caseLoadIndexSave, caseSession, caseSessionAct, caseSessionResult, judgeRep, caseLoadElementType.CaseLoadElementGroupId, caseLoadElementType.Id, caseLoadElementType.LoadProcent, caseLoadElementType.ReplaceCaseLoadElementTypeId, caseLoadCorrectionIndex, true, false));
                                    }
                                }

                                // Проверка за тип заседание
                                if (IsExistCaseLoadElementTypeRules(caseLoadElementType.CaseLoadElementTypeRules,
                                                                caseSession.SessionTypeId,
                                                                null,
                                                                null,
                                                                true,
                                                                false,
                                                                false))
                                {
                                    if (!IsExistCaseLoadActivity_Additional(caseSession.CaseId, caseSession.Id, caseSession.SessionTypeId, null, null, null, null, true, caseLoadElementType.Id, false, null) &&
                                    !IsExistCaseLoadActivityInListSave_Additional(caseLoadIndexSave, caseSession.Id, caseSession.SessionTypeId, null, null, null, null, true, caseLoadElementType.Id, false, null))
                                    {
                                        caseLoadIndexSave.Add(FillCaseLoadIndex_Additional(caseLoadIndexSave, caseSession, null, null, judgeRep, caseLoadElementType.CaseLoadElementGroupId, caseLoadElementType.Id, caseLoadElementType.LoadProcent, caseLoadElementType.ReplaceCaseLoadElementTypeId, caseLoadCorrectionIndex, true, false));
                                    }
                                }

                                // Проверка за тип заседание/вид акт
                                if (IsExistCaseLoadElementTypeRules(caseLoadElementType.CaseLoadElementTypeRules,
                                                                caseSession.SessionTypeId,
                                                                null,
                                                                caseSessionAct.ActTypeId,
                                                                true,
                                                                false,
                                                                false))
                                {
                                    if (!IsExistCaseLoadActivity_Additional(caseSession.CaseId, caseSession.Id, caseSession.SessionTypeId, caseSessionAct.Id, caseSessionAct.ActTypeId, null, null, true, caseLoadElementType.Id, false, null) &&
                                    !IsExistCaseLoadActivityInListSave_Additional(caseLoadIndexSave, caseSession.Id, caseSession.SessionTypeId, caseSessionAct.Id, caseSessionAct.ActTypeId, null, null, true, caseLoadElementType.Id, false, null))
                                    {
                                        caseLoadIndexSave.Add(FillCaseLoadIndex_Additional(caseLoadIndexSave, caseSession, caseSessionAct, null, judgeRep, caseLoadElementType.CaseLoadElementGroupId, caseLoadElementType.Id, caseLoadElementType.LoadProcent, caseLoadElementType.ReplaceCaseLoadElementTypeId, caseLoadCorrectionIndex, true, false));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (caseLoadIndexSave.Count > 0)
            {
                try
                {
                    foreach (var loadIndex in caseLoadIndexSave)
                    {
                        if (loadIndex.Id > 0)
                            repo.Update(loadIndex);
                        else
                            repo.Add<CaseLoadIndex>(loadIndex);
                    }

                    repo.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Грешка при запис на Натовареност по дела Id={caseSession.CaseId}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Проверка за съществуващо правило по зададени критерии
        /// </summary>
        /// <param name="caseLoadElementTypeRules"></param>
        /// <param name="SessionTypeId"></param>
        /// <param name="SessionResultId"></param>
        /// <param name="ActTypeId"></param>
        /// <param name="IsCreateCase"></param>
        /// <param name="IsCreateMotive"></param>
        /// <returns></returns>
        private bool IsExistCaseLoadElementTypeRules(ICollection<CaseLoadElementTypeRule> caseLoadElementTypeRules, int? SessionTypeId, int? SessionResultId, int? ActTypeId, bool? IsCreateMotive, bool? IsCreateCase, bool? IsSpecialOpinion)
        {
            return caseLoadElementTypeRules.Any(x => x.SessionTypeId == SessionTypeId &&
                                                     x.SessionResultId == SessionResultId &&
                                                     x.ActTypeId == ActTypeId &&
                                                     x.IsCreateCase == IsCreateCase &&
                                                     x.IsCreateMotive == IsCreateMotive &&
                                                     x.IsSpecialOpinion == IsSpecialOpinion &&
                                                     x.DateExpired == null);
        }

        /// <summary>
        /// Основни дейности
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public bool CaseLoadIndexAutomationElementGroupe_CC_SaveData(int CaseId)
        {
            //var environmentName = configuration.GetValue<string>("Environment:Name") ?? NomenclatureConstants.Environments.Production;
            //if (environmentName == NomenclatureConstants.Environments.Production)
            //{
            //    return false;
            //}
            DateTime _dateNow = DateTime.Now;

            var caseCase = repo.AllReadonly<Case>()
                               .Include(x => x.CaseLawUnits)
                               .Include(x => x.CaseType)
                               .Include(x => x.CaseSessions)
                               .Where(x => x.Id == CaseId)
                               .FirstOrDefault();

            var caseSession = caseCase.CaseSessions.Where(x => x.DateExpired == null).OrderBy(x => x.DateFrom).FirstOrDefault();

            if (caseSession == null)
                return false;

            var judgeRep = caseCase.CaseLawUnits.Where(x => (x.CaseSessionId == null) &&
                                                            ((x.DateTo ?? caseSession.DateFrom.AddYears(100)).Date >= caseSession.DateFrom.Date) &&
                                                            (x.DateFrom <= caseSession.DateTo) &&
                                                            (x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter))
                                                .OrderByDescending(x => x.DateFrom)
                                                .FirstOrDefault();


            if (judgeRep == null)
                return false;

            var caseLoadElementGroups = ReadAllCaseLoadElementGroupByCaseNew(CaseId, caseCase.RegDate);
            if (caseLoadElementGroups.Count < 1)
                return false;

            var caseLoadCorrectionIndex = caseLoadCorrectionService.GetCaseLoadCorrectionToDate(CaseId, caseCase.RegDate);

            var caseLoadIndexSave = new List<CaseLoadIndex>();
            foreach (var caseLoad in caseLoadElementGroups)
            {
                foreach (var caseLoadElementType in caseLoad.CaseLoadElementTypes.OrderBy(x => x.DateStart).ThenBy(x => x.Id))
                {
                    // Проверка за образуване на дело
                    if (IsExistCaseLoadElementTypeRules(caseLoadElementType.CaseLoadElementTypeRules,
                                                        null,
                                                        null,
                                                        null,
                                                        false,
                                                        true,
                                                        false))
                    {
                        if (!IsExistCaseLoadActivity(0, CaseId, true, judgeRep.LawUnitId, caseLoadElementType.Id, null) &&
                            !IsExistCaseLoadActivityInListSave(caseLoadIndexSave, true, judgeRep.LawUnitId, caseLoadElementType.Id, null))
                        {
                            caseLoadIndexSave.Add(FillCaseLoadIndex(caseLoadIndexSave, caseCase, judgeRep, caseLoadElementType.CaseLoadElementGroupId, caseLoadElementType.Id, caseLoadElementType.LoadProcent, caseLoadElementType.ReplaceCaseLoadElementTypeId, caseLoadCorrectionIndex, caseCase.RegDate));
                        }
                    }
                }
            }

            if (caseLoadIndexSave.Count > 0)
            {
                try
                {
                    foreach (var loadIndex in caseLoadIndexSave)
                    {
                        repo.Add<CaseLoadIndex>(loadIndex);
                    }

                    repo.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Грешка при запис на Натовареност по дела Id={CaseId}");
                    return false;
                }
            }

            return false;
        }

        public bool CaseLoadIndexAutomationElementGroupeND_SaveData(int CaseSessionId)
        {
            var caseSession = repo.AllReadonly<CaseSession>()
                                  .Include(x => x.CaseSessionResults)
                                  .Include(x => x.CaseSessionActs)
                                  .Include(x => x.Case)
                                  .ThenInclude(x => x.CaseLawUnits)
                                  .Include(x => x.Case)
                                  .ThenInclude(x => x.CaseType)
                                  .Where(x => x.Id == CaseSessionId)
                                  .FirstOrDefault();

            if (caseSession.Case.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo)
                return false;

            var judgeRep = caseSession.Case.CaseLawUnits.Where(x => x.CaseSessionId == null &&
                                                                    (x.DateTo ?? DateTime.Now.AddYears(100)).Date >= DateTime.Now.Date &&
                                                                    x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).FirstOrDefault();
            if (judgeRep == null)
                return false;

            var caseLoadElementGroups = ReadAllCaseLoadElementGroupByCase(caseSession.CaseId);
            if (caseLoadElementGroups.Count < 1)
                return false;

            var caseLoadCorrectionIndex = caseLoadCorrectionService.GetCaseLoadCorrectionToDate(caseSession.CaseId, DateTime.Now);

            var caseLoadIndexSave = new List<CaseLoadIndex>();
            foreach (var caseLoad in caseLoadElementGroups)
            {
                foreach (var caseLoadElementType in caseLoad.CaseLoadElementTypes.OrderBy(x => x.DateStart).ThenBy(x => x.Id))
                {
                    foreach (var caseSessionResult in caseSession.CaseSessionResults.Where(x => x.DateExpired == null).OrderBy(x => x.Id))
                    {
                        foreach (var caseSessionAct in caseSession.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActStateId != NomenclatureConstants.SessionActState.Project).OrderBy(x => x.RegDate))
                        {
                            if (caseLoadElementType.CaseLoadElementTypeRules.Any(x => x.SessionTypeId == caseSession.SessionTypeId &&
                                                                                      x.SessionResultId == caseSessionResult.SessionResultId &&
                                                                                      x.ActTypeId == caseSessionAct.ActTypeId &&
                                                                                      x.DateExpired == null))
                            {
                                if (!IsExistCaseLoadActivity(0, caseSession.CaseId, true, judgeRep.LawUnitId, caseLoadElementType.Id, null) &&
                                    !IsExistCaseLoadActivityInListSave(caseLoadIndexSave, true, judgeRep.LawUnitId, caseLoadElementType.Id, null))
                                {
                                    caseLoadIndexSave.Add(FillCaseLoadIndex(caseLoadIndexSave, caseSession, caseSessionAct, caseSessionResult, judgeRep, caseLoadElementType.CaseLoadElementGroupId, caseLoadElementType.Id, caseLoadElementType.LoadProcent, caseLoadElementType.ReplaceCaseLoadElementTypeId, caseLoadCorrectionIndex, false));
                                }
                            }
                        }
                    }
                }
            }

            if (caseLoadIndexSave.Count > 0)
            {
                try
                {
                    foreach (var loadIndex in caseLoadIndexSave)
                    {
                        repo.Add<CaseLoadIndex>(loadIndex);
                    }

                    repo.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Грешка при запис на Натовареност по дела Id={caseSession.CaseId}");
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Извличане на основни дейности по ид на дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        private List<CaseLoadElementGroup> ReadAllCaseLoadElementGroupByCase(int CaseId)
        {
            var caseCase = repo.AllReadonly<Case>()
                               .Include(x => x.Document)
                               .Include(x => x.CaseType)
                               .Where(x => x.Id == CaseId)
                               .FirstOrDefault();

            var result = new List<CaseLoadElementGroup>();
            var isND = (caseCase.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo);

            var dateTimeNow = caseCase.RegDate;
            result.AddRange(SetTypeAndRulesOfGroupe(repo.AllReadonly<CaseLoadElementGroup>()
                                                        .Where(x => (!x.IsAdditional) &&
                                                                    (x.IsND == isND) &&
                                                                    (x.CaseInstanceId == caseCase.CaseType.CaseInstanceId) &&
                                                                    (x.CaseTypeId == caseCase.CaseTypeId) &&
                                                                    (x.DocumentTypeIds == null) &&
                                                                    (x.CaseCodeId == null) &&
                                                                    ((x.DateStart <= dateTimeNow) && ((x.DateEnd ?? dateTimeNow.AddYears(100)) >= dateTimeNow)) &&
                                                                    x.IsActive)
                                                        .ToList() ?? new List<CaseLoadElementGroup>(), dateTimeNow));

            result.AddRange(SetTypeAndRulesOfGroupe(repo.AllReadonly<CaseLoadElementGroup>()
                                                        .Where(x => (!x.IsAdditional) &&
                                                                    (x.IsND == isND) &&
                                                                    (x.CaseInstanceId == caseCase.CaseType.CaseInstanceId) &&
                                                                    (x.CaseTypeId == caseCase.CaseTypeId) &&
                                                                    (string.IsNullOrEmpty(x.DocumentTypeIds) ? false : x.DocumentTypeIds.Contains($"{caseCase.Document.DocumentTypeId:D4}")) &&
                                                                    (x.CaseCodeId == null) &&
                                                                    ((x.DateStart <= dateTimeNow) && ((x.DateEnd ?? dateTimeNow.AddYears(100)) >= dateTimeNow)) &&
                                                                    x.IsActive)
                                                        .ToList() ?? new List<CaseLoadElementGroup>(), dateTimeNow));

            result.AddRange(SetTypeAndRulesOfGroupe(repo.AllReadonly<CaseLoadElementGroup>()
                                                        .Where(x => (!x.IsAdditional) &&
                                                                    (x.IsND == isND) &&
                                                                    (x.CaseInstanceId == caseCase.CaseType.CaseInstanceId) &&
                                                                    (x.CaseTypeId == caseCase.CaseTypeId) &&
                                                                    (x.DocumentTypeIds == null) &&
                                                                    (x.CaseCodeId == caseCase.CaseCodeId) &&
                                                                    ((x.DateStart <= dateTimeNow) && ((x.DateEnd ?? dateTimeNow.AddYears(100)) >= dateTimeNow)) &&
                                                                    x.IsActive)
                                                        .ToList() ?? new List<CaseLoadElementGroup>(), dateTimeNow));

            result.AddRange(SetTypeAndRulesOfGroupe(repo.AllReadonly<CaseLoadElementGroup>()
                                                        .Where(x => (!x.IsAdditional) &&
                                                                    (x.IsND == isND) &&
                                                                    (x.CaseInstanceId == caseCase.CaseType.CaseInstanceId) &&
                                                                    (x.CaseTypeId == null) &&
                                                                    (x.DocumentTypeIds == null) &&
                                                                    (x.CaseCodeId == null) &&
                                                                    ((x.DateStart <= dateTimeNow) && ((x.DateEnd ?? dateTimeNow.AddYears(100)) >= dateTimeNow)) &&
                                                                    x.IsActive)
                                                        .ToList() ?? new List<CaseLoadElementGroup>(), dateTimeNow));

            return result;
        }


        private List<CaseLoadElementGroup> SetTypeAndRulesOfGroupe(List<CaseLoadElementGroup> model, DateTime dateTime)
        {
            var dateTimeNow = dateTime;
            foreach (var caseLoad in model)
            {
                caseLoad.CaseLoadElementTypes = repo.AllReadonly<CaseLoadElementType>().Where(e => e.CaseLoadElementGroupId == caseLoad.Id && e.IsActive && e.DateStart <= dateTimeNow && (e.DateEnd ?? dateTimeNow.AddYears(100)) >= dateTimeNow).ToList();
                foreach (var caseLoadElementType in caseLoad.CaseLoadElementTypes)
                {
                    caseLoadElementType.CaseLoadElementTypeRules = repo.AllReadonly<CaseLoadElementTypeRule>().Where(r => r.CaseLoadElementTypeId == caseLoadElementType.Id && r.DateStart <= dateTimeNow && (r.DateEnd ?? dateTimeNow.AddYears(100)) >= dateTimeNow).ToList();
                }
            }

            return model;
        }

        /// <summary>
        /// Извличане на основни дейности по ид на дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        private List<CaseLoadElementGroup> ReadAllCaseLoadElementGroupByCaseNew(int CaseId, DateTime dateTime, bool isAdditional = false)
        {
            var caseCase = repo.AllReadonly<Case>()
                               .Where(x => x.Id == CaseId)
                               .Select(x => new
                               {
                                   x.CaseGroupId,
                                   x.CaseTypeId,
                                   x.CaseType.CaseInstanceId,
                                   x.ProcessPriorityId,
                                   x.Document.DocumentTypeId,
                                   x.CaseCodeId,
                                   x.Court.CourtTypeId
                               })
                               .FirstOrDefault();

            var result = new List<CaseLoadElementGroup>();
            var isND = (caseCase.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo);
            var dateTimeNow = dateTime;

            // Извлича данните по тип документ и тип производство
            result.AddRange(SetTypeAndRulesOfGroupe(repo.AllReadonly<CaseLoadElementGroup>()
                                                        .Where(x => (x.IsAdditional == isAdditional) &&
                                                                    (x.IsND == isND) &&
                                                                    (x.CaseInstanceId == caseCase.CaseInstanceId) &&
                                                                    (x.CaseTypeId == caseCase.CaseTypeId) &&
                                                                    (string.IsNullOrEmpty(x.DocumentTypeIds) ? false : x.DocumentTypeIds.Contains($"{caseCase.DocumentTypeId:D4}")) &&
                                                                    (x.ProcessPriorityId == caseCase.ProcessPriorityId) &&
                                                                    (x.CaseCodeId == null) &&
                                                                    (x.CourtTypeId == null) &&
                                                                    ((x.DateStart <= dateTimeNow) && ((x.DateEnd ?? dateTimeNow.AddYears(100)) >= dateTimeNow)) &&
                                                                    x.IsActive)
                                                        .ToList() ?? new List<CaseLoadElementGroup>(), dateTimeNow));

            // Извлича данните по тип документ и тип производство и по шифър
            result.AddRange(SetTypeAndRulesOfGroupe(repo.AllReadonly<CaseLoadElementGroup>()
                                                        .Where(x => (x.IsAdditional == isAdditional) &&
                                                                    (x.IsND == isND) &&
                                                                    (x.CaseInstanceId == caseCase.CaseInstanceId) &&
                                                                    (x.CaseTypeId == caseCase.CaseTypeId) &&
                                                                    (string.IsNullOrEmpty(x.DocumentTypeIds) ? false : x.DocumentTypeIds.Contains($"{caseCase.DocumentTypeId:D4}")) &&
                                                                    (x.ProcessPriorityId == null) &&
                                                                    (x.CaseCodeId == caseCase.CaseCodeId) &&
                                                                    (x.CourtTypeId == null) &&
                                                                    ((x.DateStart <= dateTimeNow) && ((x.DateEnd ?? dateTimeNow.AddYears(100)) >= dateTimeNow)) &&
                                                                    x.IsActive)
                                                        .ToList() ?? new List<CaseLoadElementGroup>(), dateTimeNow));

            //if (result.Count > 0)
            //    return result;

            // Извлича данните по шифър
            result.AddRange(SetTypeAndRulesOfGroupe(repo.AllReadonly<CaseLoadElementGroup>()
                                                        .Where(x => (x.IsAdditional == isAdditional) &&
                                                                    (x.IsND == isND) &&
                                                                    (x.CaseInstanceId == caseCase.CaseInstanceId) &&
                                                                    (x.CaseTypeId == caseCase.CaseTypeId) &&
                                                                    (x.DocumentTypeIds == null) &&
                                                                    (x.ProcessPriorityId == null) &&
                                                                    (x.CaseCodeId == caseCase.CaseCodeId) &&
                                                                    (x.CourtTypeId == null) &&
                                                                    ((x.DateStart <= dateTimeNow) && ((x.DateEnd ?? dateTimeNow.AddYears(100)) >= dateTimeNow)) &&
                                                                    x.IsActive)
                                                        .ToList() ?? new List<CaseLoadElementGroup>(), dateTimeNow));

            //if (result.Count > 0)
            //    return result;

            // Извлича данните по тип документ
            result.AddRange(SetTypeAndRulesOfGroupe(repo.AllReadonly<CaseLoadElementGroup>()
                                                        .Where(x => (x.IsAdditional == isAdditional) &&
                                                                    (x.IsND == isND) &&
                                                                    (x.CaseInstanceId == caseCase.CaseInstanceId) &&
                                                                    (x.CaseTypeId == caseCase.CaseTypeId) &&
                                                                    (string.IsNullOrEmpty(x.DocumentTypeIds) ? false : x.DocumentTypeIds.Contains($"{caseCase.DocumentTypeId:D4}")) &&
                                                                    (x.CaseCodeId == null) &&
                                                                    (x.ProcessPriorityId == null) &&
                                                                    (x.CourtTypeId == null) &&
                                                                    ((x.DateStart <= dateTimeNow) && ((x.DateEnd ?? dateTimeNow.AddYears(100)) >= dateTimeNow)) &&
                                                                    x.IsActive)
                                                        .ToList() ?? new List<CaseLoadElementGroup>(), dateTimeNow));

            //if (result.Count > 0)
            //    return result;

            // Извлича данните по основните полета (наказателно дело, инстанция и тип дело)
            result.AddRange(SetTypeAndRulesOfGroupe(repo.AllReadonly<CaseLoadElementGroup>()
                                                        .Where(x => (x.IsAdditional == isAdditional) &&
                                                                    (x.IsND == isND) &&
                                                                    (x.CaseInstanceId == caseCase.CaseInstanceId) &&
                                                                    (x.CaseTypeId == caseCase.CaseTypeId) &&
                                                                    (x.DocumentTypeIds == null) &&
                                                                    (x.CaseCodeId == null) &&
                                                                    (x.ProcessPriorityId == null) &&
                                                                    (x.CourtTypeId == null) &&
                                                                    ((x.DateStart <= dateTimeNow) && ((x.DateEnd ?? dateTimeNow.AddYears(100)) >= dateTimeNow)) &&
                                                                    x.IsActive)
                                                        .ToList() ?? new List<CaseLoadElementGroup>(), dateTimeNow));

            //if (result.Count > 0)
            //    return result;

            result.AddRange(SetTypeAndRulesOfGroupe(repo.AllReadonly<CaseLoadElementGroup>()
                                                        .Where(x => (x.IsAdditional == isAdditional) &&
                                                                    (x.IsND == isND) &&
                                                                    (x.CaseInstanceId == caseCase.CaseInstanceId) &&
                                                                    (x.CaseTypeId == null) &&
                                                                    (x.DocumentTypeIds == null) &&
                                                                    (x.CaseCodeId == null) &&
                                                                    (x.ProcessPriorityId == null) &&
                                                                    (x.CourtTypeId == caseCase.CourtTypeId) &&
                                                                    ((x.DateStart <= dateTimeNow) && ((x.DateEnd ?? dateTimeNow.AddYears(100)) >= dateTimeNow)) &&
                                                                    x.IsActive)
                                                        .ToList() ?? new List<CaseLoadElementGroup>(), dateTimeNow));

            //if (result.Count > 0)
            //    return result;

            result.AddRange(SetTypeAndRulesOfGroupe(repo.AllReadonly<CaseLoadElementGroup>()
                                                        .Where(x => (x.IsAdditional == isAdditional) &&
                                                                    (x.IsND == isND) &&
                                                                    (x.CaseInstanceId == caseCase.CaseInstanceId) &&
                                                                    (x.CaseTypeId == null) &&
                                                                    (x.DocumentTypeIds == null) &&
                                                                    (x.CaseCodeId == null) &&
                                                                    (x.ProcessPriorityId == null) &&
                                                                    (x.CourtTypeId == null) &&
                                                                    ((x.DateStart <= dateTimeNow) && ((x.DateEnd ?? dateTimeNow.AddYears(100)) >= dateTimeNow)) &&
                                                                    x.IsActive)
                                                        .ToList() ?? new List<CaseLoadElementGroup>(), dateTimeNow));

            return result;
        }

        /// <summary>
        /// Зареждане на комбо в Вид група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_CaseLoadElementGroup(int CaseId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = ReadAllCaseLoadElementGroupByCase(CaseId)
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
        /// Зареждане на комбо с Елементи към група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="CaseLoadElementGroupeId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_CaseLoadElementType(int CaseLoadElementGroupeId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var dateTimeNow = DateTime.Now;
            var selectListItems = repo.AllReadonly<CaseLoadElementType>()
                                      .Where(x => (x.CaseLoadElementGroupId == CaseLoadElementGroupeId) &&
                                                  ((x.DateStart <= dateTimeNow) && ((x.DateEnd ?? dateTimeNow.AddYears(100)) >= dateTimeNow)))
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
        /// Зареждане на комбо с Елементи за препокриване
        /// </summary>
        /// <param name="CurrentId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_CaseLoadElementType_Replace(int CurrentId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var dateTimeNow = DateTime.Now;
            var selectListItems = repo.AllReadonly<CaseLoadElementType>()
                                      .Include(X => X.CaseLoadElementGroup)
                                      .Where(x => (CurrentId > 0 ? x.Id != CurrentId : true) &&
                                                  ((x.DateStart <= dateTimeNow) && ((x.DateEnd ?? dateTimeNow.AddYears(100)) >= dateTimeNow)) &&
                                                  ((x.CaseLoadElementGroup.DateStart <= dateTimeNow) && ((x.CaseLoadElementGroup.DateEnd ?? dateTimeNow.AddYears(100)) >= dateTimeNow)))
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.Label + " (" + x.CaseLoadElementGroup.Label + ")",
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
        /// Зареждане на комбо с Натовареност по дела - допълнителни дейности
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_CaseLoadAddActivity(int CaseId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var caseCase = repo.AllReadonly<Case>()
                               .Where(x => x.Id == CaseId)
                               .FirstOrDefault();

            var dateTimeNow = DateTime.Now;
            var isND = (caseCase.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo);
            var selectListItems = repo.AllReadonly<CaseLoadAddActivity>()
                                        .Where(x => (x.IsND == isND) &&
                                                    ((x.DateStart <= dateTimeNow) && ((x.DateEnd ?? dateTimeNow.AddYears(100)) >= dateTimeNow)))
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

        // Този метод се дублира и в CaseLoadCorrectionService
        public bool CaseLoadIndexRecalcBaseIndex(int CaseId)
        {
            try
            {
                var caseLoadIndices = repo.AllReadonly<CaseLoadIndex>()
                                          .Include(x => x.Case)
                                          .Where(x => x.CaseId == CaseId &&
                                                      x.DateExpired == null)
                                          .ToList();

                foreach (var caseLoad in caseLoadIndices)
                {
                    var caseLoadCorrectionIdex = caseLoadCorrectionService.GetCaseLoadCorrectionToDate(CaseId, caseLoad.DateActivity);
                    caseLoad.BaseIndex = caseLoadCorrectionIdex > 0 ? caseLoad.Case.LoadIndex * caseLoadCorrectionIdex : caseLoad.Case.LoadIndex;
                    caseLoad.DateWrt = DateTime.Now;
                    caseLoad.UserId = userContext.UserId;
                    repo.Update(caseLoad);
                }

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при преизчисляване на базовият индекс за натовареност по дело Id={CaseId}");
                return false;
            }
        }

        private bool DeleteCaseLoadIndexMainAllCase(int? caseId)
        {
            try
            {
                repo.DeleteRange<CaseLoadIndex>(x => x.IsMainActivity == true && (caseId != null ? x.CaseId == caseId : true));
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при изтриване на натовареноста по всички дела");
                return false;
            }
        }

        private bool SetDateExpiredForCase(int caseId)
        {
            try
            {
                var caseLoadIndices = repo.All<CaseLoadIndex>()
                                          .Where(x => x.CaseId == caseId &&
                                                      x.IsMainActivity == true &&
                                                      x.DateExpired == null)
                                          .ToList() ?? new List<CaseLoadIndex>();

                var dateNow = DateTime.Now;
                foreach (var caseLoadIndex in caseLoadIndices)
                {
                    caseLoadIndex.DateExpired = dateNow;
                    caseLoadIndex.UserExpiredId = userContext.UserId;
                    caseLoadIndex.DateWrt = DateTime.Now;
                    caseLoadIndex.UserId = userContext.UserId;
                }
                if (caseLoadIndices.Count > 0)

                    repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при премахване на натовареност по дело с ид: " + caseId.ToString());
                return false;
            }
        }

        public bool EditSessionAndRecalcCase(int CaseId, int SessionId)
        {
            try
            {
                bool result = true;

                var caseLoadIndex = repo.AllReadonly<CaseLoadIndex>()
                                        .Include(x => x.CaseSession)
                                        .Where(x => x.CaseId == CaseId &&
                                                    x.CaseSessionId == SessionId).FirstOrDefault();

                if (caseLoadIndex == null)
                    return true;


                if (caseLoadIndex.SessionTypeId != caseLoadIndex.CaseSession.SessionTypeId)
                    result = string.IsNullOrEmpty(RecalcCaseLoadIndexByCase(CaseId));

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при преизчисляване на натовареност при редакция на заседание : " + CaseId.ToString());
                return false;
            };
        }

        public bool EditActAndRecalcCase(int CaseId, int ActId)
        {
            try
            {
                bool result = true;

                var caseLoadIndex = repo.AllReadonly<CaseLoadIndex>()
                                        .Include(x => x.CaseSessionAct)
                                        .Where(x => x.CaseId == CaseId &&
                                                    x.CaseSessionActId == ActId).FirstOrDefault();

                if (caseLoadIndex == null)
                    return true;


                if (caseLoadIndex.ActTypeId != caseLoadIndex.CaseSessionAct.ActTypeId)
                    result = string.IsNullOrEmpty(RecalcCaseLoadIndexByCase(CaseId));

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при преизчисляване на натовареност при редакция на акт : " + CaseId.ToString());
                return false;
            };
        }

        public bool EditSessionResultAndRecalcCase(int CaseId, int ResultId)
        {
            try
            {
                bool result = true;
                var dateNow = DateTime.Now;

                var caseLoadIndices = repo.AllReadonly<CaseLoadIndex>()
                                          .Where(x => x.CaseId == CaseId &&
                                                      x.CaseSessionResultId == ResultId &&
                                                      x.DateExpired == null &&
                                                      (x.SessionResultId != x.CaseSessionResult.SessionResultId ||
                                                       x.CaseSessionResult.DateExpired != null))
                                          .ToList();

                var caseLoadIndex = caseLoadIndices.Where(x => x.IsMainActivity).FirstOrDefault();

                if (caseLoadIndex != null)
                {
                    result = string.IsNullOrEmpty(RecalcCaseLoadIndexByCase(CaseId));
                }

                foreach (var caseLoad in caseLoadIndices.Where(x => !x.IsMainActivity))
                {
                    caseLoad.DateExpired = dateNow;
                    caseLoad.UserExpiredId = userContext.UserId;
                    caseLoad.DateWrt = DateTime.Now;
                    caseLoad.UserId = userContext.UserId;
                    repo.Update(caseLoad);
                    repo.SaveChanges();
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при преизчисляване на натовареност при редакция на резултат : " + CaseId.ToString());
                return false;
            };
        }

        public string RecalcAllCase()
        {
            if (!DeleteCaseLoadIndexMainAllCase(null))
                return "Проблем при изтриване на натовареноста по всички дела";

            var listCaseId = repo.AllReadonly<Case>()
                                 .OrderBy(x => x.Id)
                                 .Select(x => x.Id)
                                 .ToList();

            int saved = 0;
            foreach (var caseId in listCaseId)
            {
                if (saved % 250 == 0)
                {
                    repo.RefreshDbContext(configuration.GetConnectionString("DefaultConnection"));
                }

                CaseLoadIndexAutomationElementGroupe_CC_SaveData(caseId);
                var listSessionId = repo.AllReadonly<CaseSession>()
                                        .Where(x => x.CaseId == caseId &&
                                                    x.DateExpired == null)
                                        .OrderBy(x => x.DateFrom)
                                        .Select(x => x.Id)
                                        .ToList();

                foreach (var sessionId in listSessionId)
                {
                    CaseLoadIndexAutomationElementGroupe_SRA_SaveData(sessionId);
                }

                saved++;
            }

            return string.Empty;
        }

        public string RecalcCaseLoadIndexByCase(int CaseId)
        {
            if (!SetDateExpiredForCase(CaseId))
                return "Проблем при премахване на натовареност по текущото дело.";

            CaseLoadIndexAutomationElementGroupe_CC_SaveData(CaseId);
            var listSessionId = repo.AllReadonly<CaseSession>()
                                    .Where(x => x.CaseId == CaseId &&
                                                x.DateExpired == null)
                                    .OrderBy(x => x.DateFrom)
                                    .Select(x => x.Id)
                                    .ToList();

            foreach (var sessionId in listSessionId)
            {
                CaseLoadIndexAutomationElementGroupe_SRA_SaveData(sessionId);
            }

            return string.Empty;
        }

        public bool CaseLoadIndex_ExpiredInfo(ExpiredInfoVM model)
        {
            var saved = repo.GetById<CaseLoadIndex>(model.Id);

            if (saved != null)
            {
                saved.DateExpired = DateTime.Now;
                saved.UserExpiredId = userContext.UserId;
                saved.DescriptionExpired = model.DescriptionExpired;
                repo.Update(saved);
                repo.SaveChanges();

                RecalcCaseLoadIndexByCase(saved.CaseId);

                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Case Load Element

        /// <summary>
        /// Извличане на данни за Вид група за натовареност по дела - основни дейности
        /// </summary>
        /// <returns></returns>
        public IQueryable<CaseLoadElementGroupVM> CaseLoadElementGroup_Select()
        {
            return repo.AllReadonly<CaseLoadElementGroup>()
                       .Include(x => x.CaseInstance)
                       .Include(x => x.CaseType)
                       .Select(x => new CaseLoadElementGroupVM()
                       {
                           Id = x.Id,
                           IsNDLabel = (x.IsND) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No,
                           CaseInstanceLabel = (x.CaseInstance != null) ? x.CaseInstance.Label : string.Empty,
                           CaseTypeLabel = (x.CaseType != null) ? x.CaseType.Label : string.Empty,
                           Label = x.Label,
                           DateStart = x.DateStart,
                           DateEnd = x.DateEnd,
                           IsAdditionalText = x.IsAdditional ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No
                       })
                       .AsQueryable();
        }

        public CaseLoadElementGroupVM CaseLoadElementGroupVM_ById(int id)
        {
            return repo.AllReadonly<CaseLoadElementGroup>()
                       .Include(x => x.CaseInstance)
                       .Include(x => x.CaseType)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseLoadElementGroupVM()
                       {
                           Id = x.Id,
                           IsNDLabel = (x.IsND) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No,
                           CaseInstanceLabel = (x.CaseInstance != null) ? x.CaseInstance.Label : string.Empty,
                           CaseTypeLabel = (x.CaseType != null) ? x.CaseType.Label : string.Empty,
                           Label = x.Label,
                           DateStart = x.DateStart,
                           DateEnd = x.DateEnd,
                           IsAdditionalText = x.IsAdditional ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No
                       })
                       .FirstOrDefault();
        }

        /// <summary>
        /// Запис на Вид група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseLoadElementGroup_SaveData(CaseLoadElementGroup model)
        {
            try
            {
                model.CaseTypeId = model.CaseTypeId.NumberEmptyToNull();
                model.CourtId = model.CourtId.NumberEmptyToNull();
                model.CourtTypeId = model.CourtTypeId.NumberEmptyToNull();
                model.DocumentTypeId = model.DocumentTypeId.NumberEmptyToNull();
                model.CaseCodeId = model.CaseCodeId.NumberEmptyToNull();
                model.ProcessPriorityId = model.ProcessPriorityId.NumberEmptyToNull();
                model.DocumentTypeIds = (model.ArrayDocumentTypeIds != null && model.ArrayDocumentTypeIds.Any()) ? string.Join(",", model.ArrayDocumentTypeIds.Select(x => $"{int.Parse(x):D4}")) : null;

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLoadElementGroup>(model.Id);
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.IsND = model.IsND;
                    saved.CaseInstanceId = model.CaseInstanceId;
                    saved.CaseTypeId = model.CaseTypeId;
                    saved.CaseCodeId = model.CaseCodeId;
                    saved.DocumentTypeId = model.DocumentTypeId;
                    saved.ProcessPriorityId = model.ProcessPriorityId;
                    saved.CourtId = model.CourtId;
                    saved.CourtTypeId = model.CourtTypeId;
                    saved.DateEnd = model.DateEnd.ForceEndDate();
                    saved.DateStart = model.DateStart.ForceStartDate();
                    saved.IsActive = model.IsActive;
                    saved.IsAdditional = model.IsAdditional;
                    saved.DocumentTypeIds = model.DocumentTypeIds;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    repo.Add<CaseLoadElementGroup>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Вид група за натовареност по дела Id={model.Id}");
                return false;
            }
        }

        /// <summary>
        /// Извличане на Елементи към група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="CaseLoadElementGroupId"></param>
        /// <returns></returns>
        public IQueryable<CaseLoadElementTypeVM> CaseLoadElementType_Select(int CaseLoadElementGroupId)
        {
            return repo.AllReadonly<CaseLoadElementType>()
                       .Where(x => x.CaseLoadElementGroupId == CaseLoadElementGroupId)
                       .Select(x => new CaseLoadElementTypeVM()
                       {
                           Id = x.Id,
                           LoadProcent = x.LoadProcent,
                           Label = x.Label,
                           DateStart = x.DateStart,
                           DateEnd = x.DateEnd,
                           IsActiveText = x.IsActive ? MessageConstant.Yes : MessageConstant.No
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Запис на Елементи към група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseLoadElementType_SaveData(CaseLoadElementType model)
        {
            try
            {
                model.ReplaceCaseLoadElementTypeId = model.ReplaceCaseLoadElementTypeId.NumberEmptyToNull();

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLoadElementType>(model.Id);
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.LoadProcent = model.LoadProcent;
                    saved.ReplaceCaseLoadElementTypeId = model.ReplaceCaseLoadElementTypeId;
                    saved.CaseLoadElementGroupId = model.CaseLoadElementGroupId;
                    saved.DateEnd = model.DateEnd.ForceEndDate();
                    saved.DateStart = model.DateStart.ForceStartDate();
                    saved.IsActive = model.IsActive;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    repo.Add<CaseLoadElementType>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Вид група за натовареност по дела Id={model.Id}");
                return false;
            }
        }

        public IQueryable<CaseLoadElementTypeRuleVM> CaseLoadElementTypeRule_Select(int CaseLoadElementTypeId)
        {
            return repo.AllReadonly<CaseLoadElementTypeRule>()
                       .Include(x => x.SessionType)
                       .Include(x => x.SessionResult)
                       .Include(x => x.ActType)
                       .Where(x => x.CaseLoadElementTypeId == CaseLoadElementTypeId && x.DateExpired == null)
                       .Select(x => new CaseLoadElementTypeRuleVM()
                       {
                           Id = x.Id,
                           SessionTypeLabel = x.SessionType.Label,
                           SessionResultLabel = x.SessionResult.Label,
                           ActTypeLabel = x.ActType.Label,
                           IsCreateMotiveText = (x.IsCreateMotive ?? false) ? MessageConstant.Yes : MessageConstant.No,
                           IsSpecialOpinionText = (x.IsSpecialOpinion ?? false) ? MessageConstant.Yes : MessageConstant.No,
                           IsCreateCaseText = (x.IsCreateCase ?? false) ? MessageConstant.Yes : MessageConstant.No,
                           DateStart = x.DateStart,
                           DateEnd = x.DateEnd
                       })
                       .AsQueryable();
        }

        public bool CaseLoadElementTypeRule_SaveData(CaseLoadElementTypeRule model)
        {
            try
            {
                model.SessionTypeId = model.SessionTypeId.NumberEmptyToNull();
                model.SessionResultId = model.SessionResultId.NumberEmptyToNull();
                model.ActTypeId = model.ActTypeId.NumberEmptyToNull();

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLoadElementTypeRule>(model.Id);
                    saved.SessionResultId = model.SessionResultId;
                    saved.SessionTypeId = model.SessionTypeId;
                    saved.ActTypeId = model.ActTypeId;
                    saved.IsCreateMotive = model.IsCreateMotive;
                    saved.IsSpecialOpinion = model.IsSpecialOpinion;
                    saved.IsCreateCase = model.IsCreateCase;
                    saved.DateStart = model.DateStart;
                    saved.DateEnd = model.DateEnd;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    var caseLoad = repo.GetById<CaseLoadElementType>(model.CaseLoadElementTypeId);
                    model.Label = caseLoad.Label;
                    repo.Add<CaseLoadElementTypeRule>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Вид група за натовареност по дела Id={model.Id}");
                return false;
            }
        }

        public bool ElementTypeRule_Expired(ExpiredInfoVM model)
        {
            var saved = repo.GetById<CaseLoadElementTypeRule>(model.Id);

            if (saved != null)
            {
                var LawUnitId = repo.GetPropById<ApplicationUser, int>(x => x.Id == userContext.UserId, x => x.LawUnitId);
                var lawFullName = repo.GetPropById<LawUnit, string>(x => x.Id == LawUnitId, x => x.FullName);

                saved.DateExpired = DateTime.Now;
                saved.UserExpiredId = null;
                saved.DescriptionExpired = model.DescriptionExpired + " " + lawFullName;
                repo.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        public IQueryable<CaseLoadElementTypeStopVM> CaseLoadElementTypeStop_Select(int CaseLoadElementTypeId)
        {
            return repo.AllReadonly<CaseLoadElementTypeStop>()
                       .Where(x => x.CaseLoadElementTypeId == CaseLoadElementTypeId && x.DateExpired == null)
                       .Select(x => new CaseLoadElementTypeStopVM()
                       {
                           Id = x.Id,
                           CaseLoadElementTypeLabel = x.CaseLoadElementType.Label + " " + x.CaseLoadElementType.LoadProcent.ToString("0.00"),
                           CaseLoadElementTypeStopLabel = x.CaseLoadElementTypeStopElement.Label + " " + x.CaseLoadElementTypeStopElement.LoadProcent.ToString("0.00"),
                           CaseLoadElementTypeStopOrder = x.CaseLoadElementTypeStopElement.Label
                       })
                       .AsQueryable();
        }

        public bool CaseLoadElementTypeStop_SaveData(CaseLoadElementTypeStop model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLoadElementTypeStop>(model.Id);
                    saved.CaseLoadElementTypeId = model.CaseLoadElementTypeId;
                    saved.CaseLoadElementTypeStopId = model.CaseLoadElementTypeStopId;
                    repo.SaveChanges();
                }
                else
                {
                    repo.Add<CaseLoadElementTypeStop>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на забрани за натовареност Id={model.Id}");
                return false;
            }
        }

        public bool ElementTypeStop_Expired(ExpiredInfoVM model)
        {
            var saved = repo.GetById<CaseLoadElementTypeStop>(model.Id);

            if (saved != null)
            {
                saved.DateExpired = DateTime.Now;
                saved.UserExpiredId = null;
                saved.DescriptionExpired = model.DescriptionExpired + " " + userContext.FullName;
                repo.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Case Load Activity

        /// <summary>
        /// Извличане на данни за Натовареност по дела - допълнителни дейности
        /// </summary>
        /// <returns></returns>
        public IQueryable<CaseLoadAddActivityVM> CaseLoadAddActivity_Select()
        {
            return repo.AllReadonly<CaseLoadAddActivity>()
                .Select(x => new CaseLoadAddActivityVM()
                {
                    Id = x.Id,
                    IsNDLabel = (x.IsND) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No,
                    Label = x.Label,
                })
                .AsQueryable();
        }

        /// <summary>
        /// Запис на Натовареност по дела - допълнителни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseLoadAddActivity_SaveData(CaseLoadAddActivity model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLoadAddActivity>(model.Id);
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.IsND = model.IsND;
                    saved.DateEnd = model.DateEnd.ForceEndDate();
                    saved.DateStart = model.DateStart.ForceStartDate();
                    saved.IsActive = model.IsActive;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    repo.Add<CaseLoadAddActivity>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Натовареност по дела - допълнителни дейности Id={model.Id}");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="CaseLoadAddActivityId"></param>
        /// <returns></returns>
        public IQueryable<CaseLoadAddActivityIndexVM> CaseLoadAddActivityIndex_Select(int CaseLoadAddActivityId)
        {
            return repo.AllReadonly<CaseLoadAddActivityIndex>()
                       .Include(x => x.CourtType)
                       .Where(x => x.CaseLoadAddActivityId == CaseLoadAddActivityId)
                       .Select(x => new CaseLoadAddActivityIndexVM()
                       {
                           Id = x.Id,
                           CourtTypeLabel = (x.CourtType != null) ? x.CourtType.Label : string.Empty,
                           Label = x.Label,
                           LoadIndex = x.LoadIndex,
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Запис на Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseLoadAddActivityIndex_SaveData(CaseLoadAddActivityIndex model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLoadAddActivityIndex>(model.Id);
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.CourtTypeId = model.CourtTypeId;
                    saved.LoadIndex = model.LoadIndex;
                    saved.DateEnd = model.DateEnd.ForceEndDate();
                    saved.DateStart = model.DateStart.ForceStartDate();
                    saved.IsActive = model.IsActive;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    repo.Add<CaseLoadAddActivityIndex>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Натовареност по дела - допълнителни дейности - стойности по вид съд Id={model.Id}");
                return false;
            }
        }

        /// <summary>
        /// Извличане на Натовареност по дела - допълнителни дейности
        /// </summary>
        /// <param name="term"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<LabelValueVM> Get_CaseLoadAddActivity(string term, int? id)
        {
            term = term.SafeLower();
            Expression<Func<CaseLoadAddActivity, bool>> filter = x => x.Label.Contains(term ?? x.Label, StringComparison.InvariantCultureIgnoreCase);
            if (id > 0)
            {
                filter = x => x.Id == id;
            }
            return repo.AllReadonly<CaseLoadAddActivity>()
                            .Where(filter)
                            .Where(x => ((x.DateStart <= DateTime.Now) && ((x.DateEnd ?? DateTime.Now.AddYears(100)) >= DateTime.Now)))
                            .OrderBy(x => x.Label)
                            .Select(x => new LabelValueVM
                            {
                                Value = x.Id.ToString(),
                                Label = x.Label
                            }).ToList();
        }

        #endregion

        #region Judge Load Activity

        /// <summary>
        /// Извличане на данни за Натовареност на съдии - допълнителни дейности
        /// </summary>
        /// <returns></returns>
        public IQueryable<JudgeLoadActivityVM> JudgeLoadActivity_Select()
        {
            return repo.AllReadonly<JudgeLoadActivity>()
                       .Select(x => new JudgeLoadActivityVM()
                       {
                           Id = x.Id,
                           Label = x.Label,
                           GroupNo = x.GroupNo,
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Запис на Натовареност на съдии - допълнителни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool JudgeLoadActivity_SaveData(JudgeLoadActivity model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<JudgeLoadActivity>(model.Id);
                    saved.Label = model.Label;
                    saved.IsCalcOneTimeForPeriod = model.IsCalcOneTimeForPeriod;
                    saved.Description = model.Description;
                    saved.GroupNo = model.GroupNo;
                    saved.DateEnd = model.DateEnd.ForceEndDate();
                    saved.DateStart = model.DateStart.ForceStartDate();
                    saved.IsActive = model.IsActive;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    model.IsActive = true;
                    repo.Add<JudgeLoadActivity>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Натовареност на съдии - допълнителни дейности Id={model.Id}");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="JudgeLoadActivityId"></param>
        /// <returns></returns>
        public IQueryable<JudgeLoadActivityIndexVM> JudgeLoadActivityIndex_Select(int JudgeLoadActivityId)
        {
            return repo.AllReadonly<JudgeLoadActivityIndex>()
                       .Include(x => x.CourtType)
                       .Where(x => x.JudgeLoadActivityId == JudgeLoadActivityId)
                       .Select(x => new JudgeLoadActivityIndexVM()
                       {
                           Id = x.Id,
                           CourtTypeLabel = (x.CourtType != null) ? x.CourtType.Label : string.Empty,
                           LoadIndex = x.LoadIndex,
                           IsActiveLabel = (x.IsActive) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Запис на Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool JudgeLoadActivityIndex_SaveData(JudgeLoadActivityIndex model)
        {
            try
            {
                model.CourtTypeId = model.CourtTypeId.NumberEmptyToNull();
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<JudgeLoadActivityIndex>(model.Id);
                    saved.JudgeLoadActivityId = model.JudgeLoadActivityId;
                    saved.IsActive = model.IsActive;
                    saved.CourtTypeId = model.CourtTypeId;
                    saved.LoadIndex = model.LoadIndex;
                    saved.DateEnd = model.DateEnd.ForceEndDate();
                    saved.DateStart = model.DateStart.ForceStartDate();
                    saved.IsActive = model.IsActive;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    repo.Add<JudgeLoadActivityIndex>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Натовареност по дела - допълнителни дейности - стойности по вид съд Id={model.Id}");
                return false;
            }
        }

        #endregion

        #region Court Law Unit Activity

        /// <summary>
        /// Извличане на данни за Допълнителни и административни дейности към съдии по съд
        /// </summary>
        /// <param name="CourtId"></param>
        /// <returns></returns>
        public IQueryable<CourtLawUnitActivityVM> CourtLawUnitActivity_Select(int CourtId, CaseLoadIndexFilterVM model)
        {
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<CourtLawUnitActivity, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.ActivityDate.Date >= dateFromSearch.Date && x.ActivityDate.Date <= dateToSearch.Date;

            Expression<Func<CourtLawUnitActivity, bool>> yearSearch = x => true;
            if ((model.Year ?? 0) > 0)
                yearSearch = x => x.ActivityDate.Year == model.Year;

            Expression<Func<CourtLawUnitActivity, bool>> lawunitSearch = x => true;
            if ((model.LawUnitId ?? 0) > 0)
                lawunitSearch = x => x.LawUnitId == model.LawUnitId;

            Expression<Func<CourtLawUnitActivity, bool>> judgeLoadactivitySearch = x => true;
            if (model.JudgeLoadActivityId > 0)
                judgeLoadactivitySearch = x => x.JudgeLoadActivityId == model.JudgeLoadActivityId;

            return repo.AllReadonly<CourtLawUnitActivity>()
                       .Include(x => x.LawUnit)
                       .Include(x => x.JudgeLoadActivity)
                       .Where(x => x.CourtId == CourtId && x.DateExpired == null)
                       .Where(dateSearch)
                       .Where(yearSearch)
                       .Where(lawunitSearch)
                       .Where(judgeLoadactivitySearch)
                       .Select(x => new CourtLawUnitActivityVM()
                       {
                           Id = x.Id,
                           LawUnitLabel = x.LawUnit.FullName,
                           ActivityDate = x.ActivityDate,
                           JudgeLoadActivityLabel = x.JudgeLoadActivity.Label,
                           LoadIndex = x.LoadIndex
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Извличане на индекс за Допълнителни и административни дейности към съдии по съд
        /// </summary>
        /// <param name="JudgeLoadActivityId"></param>
        /// <returns></returns>
        private decimal GetLoadIndex_CourtLawUnitActivity(int JudgeLoadActivityId)
        {
            var judgeLoadActivityIndices = repo.AllReadonly<JudgeLoadActivityIndex>()
                                               .Where(x => x.JudgeLoadActivityId == JudgeLoadActivityId)
                                               .ToList();

            var judgeLoadActivityIndice = judgeLoadActivityIndices.Where(x => x.CourtTypeId == userContext.CourtTypeId).FirstOrDefault();

            if (judgeLoadActivityIndice == null)
                judgeLoadActivityIndice = judgeLoadActivityIndices.FirstOrDefault();

            var valueDel = priceService.GetPriceValue(null, NomenclatureConstants.PriceDescKeyWord.KeyJudgeLoadActivity, 0, null, 0, 0, null);

            return (judgeLoadActivityIndice != null) ? judgeLoadActivityIndice.LoadIndex / valueDel : 0;
        }

        /// <summary>
        /// Запис на Допълнителни и административни дейности към съдии по съд
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CourtLawUnitActivity_SaveData(CourtLawUnitActivity model)
        {
            try
            {
                var beginDate = new DateTime(model.ActivityDate.Year, 1, 1);
                var endDate = new DateTime(model.ActivityDate.Year, 12, 31);
                model.DateWrt = DateTime.Now;
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CourtLawUnitActivity>(model.Id);
                    saved.CourtId = model.CourtId;
                    saved.LawUnitId = model.LawUnitId;
                    saved.ActivityDate = model.ActivityDate;
                    saved.JudgeLoadActivityId = model.JudgeLoadActivityId;
                    saved.LoadIndex = Math.Round(((GetLoadIndex_CourtLawUnitActivity(model.JudgeLoadActivityId) / (decimal)(endDate - beginDate).TotalDays) * (((decimal)((model.DateTo ?? DateTime.Now).Date - model.ActivityDate.Date).TotalDays) + 1)), 2, MidpointRounding.AwayFromZero);
                    saved.DateTo = model.DateTo;
                    saved.Description = model.Description;
                    saved.DateWrt = model.DateWrt;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    model.LoadIndex = Math.Round(((GetLoadIndex_CourtLawUnitActivity(model.JudgeLoadActivityId) / (decimal)(endDate - beginDate).TotalDays) * (((decimal)((model.DateTo ?? DateTime.Now).Date - model.ActivityDate.Date).TotalDays) + 1)), 2, MidpointRounding.AwayFromZero);
                    repo.Add<CourtLawUnitActivity>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Допълнителни и административни дейности към съдии по съд Id={model.Id}");
                return false;
            }
        }

        /// <summary>
        /// Проверка дали съществува по дело Допълнителни и административни дейности към съдии по съд
        /// </summary>
        /// <param name="LawUnitId"></param>
        /// <param name="JudgeLoadActivityId"></param>
        /// <param name="ModelId"></param>
        /// <param name="ActivityDate"></param>
        /// <returns></returns>
        public bool IsExistCourtLawUnitActivity(int LawUnitId, int JudgeLoadActivityId, int ModelId, DateTime ActivityDate)
        {
            var judgeLoadActivity = repo.GetById<JudgeLoadActivity>(JudgeLoadActivityId);

            return repo.AllReadonly<CourtLawUnitActivity>()
                       .Include(x => x.JudgeLoadActivity)
                       .Any(x => ((ModelId > 0) ? x.Id != ModelId : true) &&
                                 (x.DateExpired == null) &&
                                 (x.LawUnitId == LawUnitId) &&
                                 ((judgeLoadActivity.GroupNo == null) ? x.JudgeLoadActivityId == JudgeLoadActivityId : x.JudgeLoadActivity.GroupNo == judgeLoadActivity.GroupNo) &&
                                 (x.ActivityDate.Year == ActivityDate.Year));

        }

        public bool IsExistCourtLawUnitActivityNew(int LawUnitId, int JudgeLoadActivityId, int ModelId, DateTime ActivityDate, DateTime DateTo)
        {
            return repo.AllReadonly<CourtLawUnitActivity>()
                       .Any(x => ((ModelId > 0) ? x.Id != ModelId : true) &&
                                 (x.DateExpired == null) &&
                                 ((x.JudgeLoadActivity.IsCalcOneTimeForPeriod ?? false) ? x.CourtId == userContext.CourtId : x.LawUnitId == LawUnitId) &&
                                 (x.JudgeLoadActivityId == JudgeLoadActivityId) &&
                                 (x.ActivityDate <= DateTo && x.DateTo >= ActivityDate));
        }

        public string RecalcAllCourtLawUnitActivity()
        {
            var lawUnitActivities = repo.AllReadonly<CourtLawUnitActivity>()
                                        .Where(x => x.DateExpired == null)
                                        .OrderBy(x => x.CourtId)
                                        .ToList();

            int saved = 0;
            foreach (var courtLawUnitActivity in lawUnitActivities)
            {
                if (saved % 250 == 0)
                {
                    repo.RefreshDbContext(configuration.GetConnectionString("DefaultConnection"));
                }

                var beginDate = new DateTime(courtLawUnitActivity.ActivityDate.Year, 1, 1);
                var endDate = new DateTime(courtLawUnitActivity.ActivityDate.Year, 12, 31);
                courtLawUnitActivity.DateTo = (courtLawUnitActivity.DateTo == null) ? endDate : courtLawUnitActivity.DateTo;
                courtLawUnitActivity.LoadIndex = Math.Round(((GetLoadIndex_CourtLawUnitActivity(courtLawUnitActivity.JudgeLoadActivityId) / (decimal)(endDate - beginDate).TotalDays) * (decimal)((courtLawUnitActivity.DateTo ?? DateTime.Now) - courtLawUnitActivity.ActivityDate).TotalDays), 2, MidpointRounding.AwayFromZero);
                courtLawUnitActivity.UserId = userContext.UserId;
                courtLawUnitActivity.DateWrt = DateTime.Now;
                repo.Update(courtLawUnitActivity);

                saved++;
            }

            return string.Empty;
        }

        #endregion

        #region Report

        /// <summary>
        /// Натовареност по дела: основни и допълнителни дейности
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<CaseLoadIndexSprVM> CaseLoadIndexSpr_Select(CaseLoadIndexFilterVM filter)
        {
            filter.LawUnitId = filter.LawUnitId.NumberEmptyToNull();
            DateTime dateNow = DateTime.Now;
            filter.DateFrom = filter.DateFrom.ForceStartDateWithAddYear(-100);
            filter.DateTo = filter.DateTo.ForceEndDateWithAddYear(100);


            Expression<Func<CaseLoadIndex, bool>> searchDate = x => true;
            if (filter.DateFrom != null || filter.DateTo != null)
                searchDate = x => x.DateActivity >= filter.DateFrom && x.DateActivity <= filter.DateTo;

            Expression<Func<CaseLoadIndex, bool>> searchLawUnit = x => true;
            if (filter.LawUnitId > 0)
                searchLawUnit = x => x.LawUnitId == filter.LawUnitId;

            Expression<Func<CaseLoadIndex, bool>> searchCaseGroup = x => true;
            if (filter.CaseGroupId > 0)
                searchCaseGroup = x => x.Case.CaseGroupId == filter.CaseGroupId;

            Expression<Func<CaseLoadIndex, bool>> searchCaseType = x => true;
            if (filter.CaseTypeId > 0)
                searchCaseType = x => x.Case.CaseTypeId == filter.CaseTypeId;

            Expression<Func<CaseLoadIndex, bool>> searchCaseCode = x => true;
            if (filter.CaseCodeId > 0)
                searchCaseCode = x => x.Case.CaseCodeId == filter.CaseCodeId;

            Expression<Func<CaseLoadIndex, bool>> searchRegNumber = x => true;
            if (!string.IsNullOrEmpty(filter.RegNumber))
                searchRegNumber = x => EF.Functions.ILike(x.Case.RegNumber, filter.RegNumber.ToCasePaternSearch());

            Expression<Func<CaseLoadIndex, bool>> searchCourtDepartment = x => true;
            if (filter.CourtDepartmentId > 0)
                searchCourtDepartment = x => x.Case.JudicalCompositionId == filter.CourtDepartmentId;

            Expression<Func<CaseLoadIndex, bool>> searchDepartmentOtdelenie = x => true;
            if (filter.CourtDepartmentOtdelenieId > 0)
                searchDepartmentOtdelenie = x => x.Case.OtdelenieId == filter.CourtDepartmentOtdelenieId;

            Expression<Func<CaseLoadIndex, bool>> searchSessionType = x => true;
            if (filter.SessionTypeId > 0)
                searchSessionType = x => x.SessionTypeId == filter.SessionTypeId;

            Expression<Func<CaseLoadIndex, bool>> searchSessionResult = x => true;
            if (filter.SessionResultId > 0)
                searchSessionResult = x => x.SessionResultId == filter.SessionResultId;

            Expression<Func<CaseLoadIndex, bool>> searchActType = x => true;
            if (filter.ActTypeId > 0)
                searchActType = x => x.ActTypeId == filter.ActTypeId;

            Expression<Func<CaseLoadIndex, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.Case.CaseCodeId ?? 0);
            }

            var caseLoadCorrectionQuery = readonlyrepo.AllReadonly<CaseLoadCorrection>();

            Expression<Func<CaseLoadIndex, bool>> courtIdWhere = x => true;
            if (filter.CourtId == null)
                courtIdWhere = x => x.CourtId == userContext.CourtId;
            else
            {
                if (filter.CourtId > 0)
                    courtIdWhere = x => x.CourtId == filter.CourtId;
            }

            List<CaseLoadIndexSprVM> result = readonlyrepo.AllReadonly<CaseLoadIndex>()
                                                          .Where(x => x.DateExpired == null)
                                                          .Where(courtIdWhere)
                                                          .Where(searchDate)
                                                          .Where(searchLawUnit)
                                                          .Where(searchCaseGroup)
                                                          .Where(searchCaseType)
                                                          .Where(searchCaseCode)
                                                          .Where(searchRegNumber)
                                                          .Where(searchCourtDepartment)
                                                          .Where(searchDepartmentOtdelenie)
                                                          .Where(searchSessionType)
                                                          .Where(searchSessionResult)
                                                          .Where(searchActType)
                                                          .Where(caseCodeIdsWhere)
                                                          .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                                                          .Select(x => new CaseLoadIndexSprVM()
                                                          {
                                                              Id = x.CaseId,
                                                              CourtLabel = x.Court.Label,
                                                              CourtId = x.CourtId,
                                                              CaseName = x.Case.RegNumber,
                                                              CaseTypeCodeLabel = x.Case.CaseType.Label,
                                                              CaseRegDate = x.Case.RegDate,
                                                              BaseIndexCase = x.Case.LoadIndex,
                                                              LawUnitId = x.LawUnitId,
                                                              LawUnitName = x.LawUnit.FullName,
                                                              BaseIndexMain = x.IsMainActivity ? Math.Round(x.BaseIndex * (x.LoadProcent / 100), 2, MidpointRounding.AwayFromZero) : 0,
                                                              BaseIndexNotMain = !x.IsMainActivity ? x.LoadIndex : 0,
                                                              CalcValue = x.IsMainActivity ? Math.Round(x.BaseIndex * (x.LoadProcent / 100), 2, MidpointRounding.AwayFromZero) : x.LoadIndex,
                                                              JudgeReport = x.Case.CaseLawUnits.Where(l => l.CaseSessionId == null &&
                                                                                                           l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                                                                           (l.DateTo ?? dateNow.AddYears(100)) >= dateNow)
                                                                                               .Select(l => l.LawUnit.FullName)
                                                                                               .FirstOrDefault(),
                                                              CorrectionLoadIndex = caseLoadCorrectionQuery.Where(c => c.CaseId == x.CaseId &&
                                                                                                                       c.DateExpired == null)
                                                                                                           .Sum(c => c.CorrectionLoadIndex)
                                                          })
                                                          .ToList();
            return result.GroupBy(x => new { x.Id, x.LawUnitId })
                         .Select(g => new CaseLoadIndexSprVM()
                         {
                             Id = g.Key.Id,
                             LawUnitId = g.Key.LawUnitId,
                             CaseName = g.Select(l => l.CaseName).FirstOrDefault(),
                             CourtLabel = g.Select(l => l.CourtLabel).FirstOrDefault(),
                             CaseTypeCodeLabel = g.Select(l => l.CaseTypeCodeLabel).FirstOrDefault(),
                             CaseRegDate = g.Select(l => l.CaseRegDate).FirstOrDefault(),
                             BaseIndexCase = g.Select(l => l.BaseIndexCase).FirstOrDefault(),
                             LawUnitName = g.Select(l => l.LawUnitName).FirstOrDefault(),
                             CalcValue = g.Sum(l => l.CalcValue),
                             BaseIndexMain = g.Sum(l => l.BaseIndexMain),
                             BaseIndexNotMain = g.Sum(l => l.BaseIndexNotMain),
                             JudgeReport = g.Select(l => l.JudgeReport).FirstOrDefault(),
                             CorrectionLoadIndex = g.Select(l => l.CorrectionLoadIndex).FirstOrDefault(),
                         })
                         .AsQueryable();
        }

        public IQueryable<CaseLoadIndexCourtGroupSprVM> CaseLoadIndexCourtGroupSpr_Select(CaseLoadIndexFilterVM model)
        {
            model.LawUnitId = model.LawUnitId.NumberEmptyToNull();
            DateTime dateNow = DateTime.Now;

            Expression<Func<CaseLoadIndex, bool>> searchDate = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                searchDate = x => x.DateActivity >= model.DateFrom && x.DateActivity <= model.DateTo;

            Expression<Func<CaseLoadIndex, bool>> searchLawUnit = x => true;
            if (model.LawUnitId > 0)
                searchLawUnit = x => x.LawUnitId == model.LawUnitId;

            Expression<Func<CaseLoadIndex, bool>> searchCourtGroup = x => true;
            if (model.CourtGroupId > 0)
                searchCourtGroup = x => x.Case.CourtGroupId == model.CourtGroupId;

            Expression<Func<CaseLoadIndex, bool>> courtIdWhere = x => true;
            if (model.CourtId == null)
                courtIdWhere = x => x.CourtId == userContext.CourtId;
            else
            {
                if (model.CourtId > 0)
                    courtIdWhere = x => x.CourtId == model.CourtId;
            }

            var caseLoadIndexQuery = repo.AllReadonly<CaseLoadIndex>()
                                         .Include(x => x.LawUnit)
                                         .Include(x => x.Case)
                                         .ThenInclude(x => x.CourtGroup)
                                         .Include(x => x.Court)
                                         .Where(x => x.DateExpired == null)
                                         .Where(courtIdWhere)
                                         .Where(searchDate)
                                         .Where(searchLawUnit)
                                         .Where(searchCourtGroup)
                                         .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                                         .ToList();

            if (model.CourtId == null)
            {
                return caseLoadIndexQuery.GroupBy(x => new { x.LawUnitId, x.Case.CourtGroupId })
                                         .Select(g => new CaseLoadIndexCourtGroupSprVM()
                                         {
                                             LawUnitId = g.Key.LawUnitId,
                                             LawUnitName = g.Select(l => l.LawUnit.FullName).FirstOrDefault(),
                                             CourtGroupId = g.Select(l => l.Case.CourtGroupId ?? 0).FirstOrDefault(),
                                             CourtGroupLabel = g.Select(l => l.Case.CourtGroupId != null ? l.Case.CourtGroup.Label : string.Empty).FirstOrDefault(),
                                             CalcValue = g.Sum(l => l.IsMainActivity ? Math.Round(l.BaseIndex * (l.LoadProcent / 100), 2, MidpointRounding.AwayFromZero) : l.LoadIndex),
                                             CalcValueAll = caseLoadIndexQuery.Where(i => i.LawUnitId == g.Key.LawUnitId).Sum(i => i.IsMainActivity ? Math.Round(i.BaseIndex * (i.LoadProcent / 100), 2, MidpointRounding.AwayFromZero) : i.LoadIndex)
                                         })
                                         .AsQueryable();
            }
            else
            {
                return caseLoadIndexQuery.GroupBy(x => new { x.LawUnitId, x.CourtId, x.Case.CourtGroupId })
                                         .Select(g => new CaseLoadIndexCourtGroupSprVM()
                                         {
                                             LawUnitId = g.Key.LawUnitId,
                                             LawUnitName = g.Select(l => l.LawUnit.FullName).FirstOrDefault(),
                                             CourtId = g.Key.CourtId,
                                             CourtLabel = g.Select(l => l.Court.Label).FirstOrDefault(),
                                             CourtGroupId = g.Select(l => l.Case.CourtGroupId ?? 0).FirstOrDefault(),
                                             CourtGroupLabel = g.Select(l => l.Case.CourtGroupId != null ? l.Case.CourtGroup.Label : string.Empty).FirstOrDefault(),
                                             CalcValue = g.Sum(l => l.IsMainActivity ? Math.Round(l.BaseIndex * (l.LoadProcent / 100), 2, MidpointRounding.AwayFromZero) : l.LoadIndex),
                                             CalcValueAll = caseLoadIndexQuery.Where(i => i.LawUnitId == g.Key.LawUnitId).Sum(i => i.IsMainActivity ? Math.Round(i.BaseIndex * (i.LoadProcent / 100), 2, MidpointRounding.AwayFromZero) : i.LoadIndex)
                                         })
                                         .AsQueryable();
            }
        }

        /// <summary>
        /// Натоварване на съдии извън дело
        /// </summary>
        /// <param name="dateFrom">От дата</param>
        /// <param name="dateTo">До дата</param>
        /// <param name="lawUnitId">Идентификатор на лице</param>
        /// <param name="judgeLoadActivityId">Идентификатор на judgeLoadActivity</param>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <returns></returns>
        public IQueryable<LawUnitLoadSprVM> CourtLawUnitActivitySpr_Select(DateTime dateFrom, DateTime dateTo, int? lawUnitId, int judgeLoadActivityId, int? courtId)
        {
            Expression<Func<CourtLawUnitActivity, bool>> searchLawUnit = x => true;
            if ((lawUnitId ?? 0) > 0)
                searchLawUnit = x => x.LawUnitId == lawUnitId;

            Expression<Func<CourtLawUnitActivity, bool>> searchJudgeLoadActivityId = x => true;
            if (judgeLoadActivityId > 0)
                searchJudgeLoadActivityId = x => x.JudgeLoadActivityId == judgeLoadActivityId;

            dateFrom = dateFrom.ForceStartDate();
            dateTo = dateTo.ForceEndDate();

            var lawUnitQuerry = readonlyrepo.AllReadonly<LawUnit>();

            Expression<Func<CourtLawUnitActivity, bool>> courtIdWhere = x => true;
            if (courtId == null)
                courtIdWhere = x => x.CourtId == userContext.CourtId;
            else
            {
                if (courtId > 0)
                    courtIdWhere = x => x.CourtId == courtId;
            }

            List<LawUnitLoadSprVM> result = readonlyrepo.AllReadonly<CourtLawUnitActivity>()
                                                        .Where(x => x.CourtId == userContext.CourtId &&
                                                                    x.DateExpired == null &&
                                                                    x.ActivityDate >= dateFrom && x.ActivityDate <= dateTo)
                                                        .Where(searchLawUnit)
                                                        .Where(searchJudgeLoadActivityId)
                                                        .Select(x => new LawUnitLoadSprVM()
                                                        {
                                                            Year = x.ActivityDate.Year,
                                                            LawUnitId = x.LawUnitId,
                                                            LawUnitLabel = x.LawUnit.FullName,
                                                            CourtId = x.CourtId,
                                                            CourtLabel = x.Court.Label,
                                                            SumLoadIndex = x.LoadIndex,
                                                            LoadIndex = x.LoadIndex,
                                                            SumLoadIndex_Col1 = x.JudgeLoadActivity.Code == NomenclatureConstants.JudgeLoadActivityCode.Col1 ? x.LoadIndex : 0,
                                                            SumLoadIndex_Col2 = x.JudgeLoadActivity.Code == NomenclatureConstants.JudgeLoadActivityCode.Col2 ? x.LoadIndex : 0,
                                                            SumLoadIndex_Col3 = x.JudgeLoadActivity.Code == NomenclatureConstants.JudgeLoadActivityCode.Col3 ? x.LoadIndex : 0,
                                                            SumLoadIndex_Col4 = x.JudgeLoadActivity.Code == NomenclatureConstants.JudgeLoadActivityCode.Col4 ? x.LoadIndex : 0,
                                                            SumLoadIndex_Col5 = x.JudgeLoadActivity.Code == NomenclatureConstants.JudgeLoadActivityCode.Col5 ? x.LoadIndex : 0,
                                                            SumLoadIndex_Col6 = x.JudgeLoadActivity.Code == NomenclatureConstants.JudgeLoadActivityCode.Col6 ? x.LoadIndex : 0,
                                                            SumLoadIndex_Col7 = x.JudgeLoadActivity.Code == NomenclatureConstants.JudgeLoadActivityCode.Col7 ? x.LoadIndex : 0,
                                                            SumLoadIndex_Col8 = x.JudgeLoadActivity.Code == NomenclatureConstants.JudgeLoadActivityCode.Col8 ? x.LoadIndex : 0,
                                                            SumLoadIndex_Col9 = x.JudgeLoadActivity.Code == NomenclatureConstants.JudgeLoadActivityCode.Col9 ? x.LoadIndex : 0,
                                                        })
                                                        .ToList();

            if (courtId == null)
            {
                return result.GroupBy(x => new { x.LawUnitId, x.Year })
                             .Select(g => new LawUnitLoadSprVM()
                             {
                                 LawUnitId = g.Key.LawUnitId,
                                 LawUnitLabel = g.Select(a => a.LawUnitLabel).FirstOrDefault(),
                                 Year = g.Key.Year,
                                 SumLoadIndex = g.Sum(a => a.LoadIndex),
                                 LoadIndex = g.Sum(a => a.LoadIndex),
                                 SumLoadIndex_Col1 = g.Sum(a => a.SumLoadIndex_Col1),
                                 SumLoadIndex_Col2 = g.Sum(a => a.SumLoadIndex_Col2),
                                 SumLoadIndex_Col3 = g.Sum(a => a.SumLoadIndex_Col3),
                                 SumLoadIndex_Col4 = g.Sum(a => a.SumLoadIndex_Col4),
                                 SumLoadIndex_Col5 = g.Sum(a => a.SumLoadIndex_Col5),
                                 SumLoadIndex_Col6 = g.Sum(a => a.SumLoadIndex_Col6),
                                 SumLoadIndex_Col7 = g.Sum(a => a.SumLoadIndex_Col7),
                                 SumLoadIndex_Col8 = g.Sum(a => a.SumLoadIndex_Col8),
                                 SumLoadIndex_Col9 = g.Sum(a => a.SumLoadIndex_Col9),
                             })
                             .AsQueryable();
            }
            else
            {
                return result.GroupBy(x => new { x.LawUnitId, x.CourtId, x.Year })
                             .Select(g => new LawUnitLoadSprVM()
                             {
                                 LawUnitId = g.Key.LawUnitId,
                                 LawUnitLabel = g.Select(a => a.LawUnitLabel).FirstOrDefault(),
                                 CourtId = g.Key.CourtId,
                                 CourtLabel = g.Select(a => a.CourtLabel).FirstOrDefault(),
                                 Year = g.Key.Year,
                                 SumLoadIndex = g.Sum(a => a.LoadIndex),
                                 LoadIndex = g.Sum(a => a.LoadIndex),
                                 SumLoadIndex_Col1 = g.Sum(a => a.SumLoadIndex_Col1),
                                 SumLoadIndex_Col2 = g.Sum(a => a.SumLoadIndex_Col2),
                                 SumLoadIndex_Col3 = g.Sum(a => a.SumLoadIndex_Col3),
                                 SumLoadIndex_Col4 = g.Sum(a => a.SumLoadIndex_Col4),
                                 SumLoadIndex_Col5 = g.Sum(a => a.SumLoadIndex_Col5),
                                 SumLoadIndex_Col6 = g.Sum(a => a.SumLoadIndex_Col6),
                                 SumLoadIndex_Col7 = g.Sum(a => a.SumLoadIndex_Col7),
                                 SumLoadIndex_Col8 = g.Sum(a => a.SumLoadIndex_Col8),
                                 SumLoadIndex_Col9 = g.Sum(a => a.SumLoadIndex_Col9),
                             })
                             .AsQueryable();
            }
        }

        /// <summary>
        /// Натовареност - извън и в дело
        /// </summary>
        /// <param name="dateFrom">От дата</param>
        /// <param name="dateTo">До дате</param>
        /// <param name="lawUnitId">Идентификатор на лице</param>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <returns></returns>
        public IQueryable<LawUnitLoadSprVM> LawUnitActivitySpr_Select(DateTime dateFrom, DateTime dateTo, int? lawUnitId, int? courtId)
        {
            var modelFilter = new CaseLoadIndexFilterVM()
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                LawUnitId = lawUnitId ?? 0,
                CourtId = courtId
            };
            var caseLoadIndexSprs = CaseLoadIndexSpr_Select(modelFilter).ToList();
            var lawUnitLoadSprs = CourtLawUnitActivitySpr_Select(dateFrom, dateTo, lawUnitId, 0, courtId).ToList();

            foreach (var caseLoad in caseLoadIndexSprs)
            {
                if (courtId == null)
                {
                    var unitLoadSprVMs = lawUnitLoadSprs.Where(x => x.LawUnitId == caseLoad.LawUnitId).FirstOrDefault();

                    if (unitLoadSprVMs == null)
                    {
                        LawUnitLoadSprVM unitLoadSprVM = new LawUnitLoadSprVM()
                        {
                            LawUnitId = caseLoad.LawUnitId,
                            LawUnitLabel = caseLoad.LawUnitName,
                            CaseLoadIndex = caseLoad.CalcValue,
                            LoadIndex = caseLoad.CalcValue,
                        };

                        lawUnitLoadSprs.Add(unitLoadSprVM);
                    }
                    else
                    {
                        unitLoadSprVMs.CaseLoadIndex += caseLoad.CalcValue;
                        unitLoadSprVMs.LoadIndex = unitLoadSprVMs.CaseLoadIndex + unitLoadSprVMs.SumLoadIndex;
                    }
                }
                else
                {
                    var unitLoadSprVMs = lawUnitLoadSprs.Where(x => x.LawUnitId == caseLoad.LawUnitId &&
                                                                    x.CourtId == caseLoad.CourtId)
                                                        .FirstOrDefault();

                    if (unitLoadSprVMs == null)
                    {
                        LawUnitLoadSprVM unitLoadSprVM = new LawUnitLoadSprVM()
                        {
                            LawUnitId = caseLoad.LawUnitId,
                            LawUnitLabel = caseLoad.LawUnitName,
                            CaseLoadIndex = caseLoad.CalcValue,
                            LoadIndex = caseLoad.CalcValue,
                            CourtId = caseLoad.CourtId,
                            CourtLabel = caseLoad.CourtLabel
                        };

                        lawUnitLoadSprs.Add(unitLoadSprVM);
                    }
                    else
                    {
                        unitLoadSprVMs.CaseLoadIndex += caseLoad.CalcValue;
                        unitLoadSprVMs.LoadIndex = unitLoadSprVMs.CaseLoadIndex + unitLoadSprVMs.SumLoadIndex;
                    }
                }
            }

            return lawUnitLoadSprs.AsQueryable();
        }

        #endregion

        #region Other

        public bool IsCaseExistSessionAct(int CaseId)
        {
            if (!repo.AllReadonly<CaseSession>()
                     .Any(x => x.CaseId == CaseId &&
                               x.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                               x.DateExpired == null))
                return false;

            return repo.AllReadonly<CaseSessionAct>()
                       .Any(x => x.CaseId == CaseId &&
                                 x.DateExpired == null &&
                                 x.ActDeclaredDate != null);
        }

        #endregion
    }
}
