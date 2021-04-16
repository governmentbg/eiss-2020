using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class CaseClassificationService : BaseService, ICaseClassificationService
    {
        private readonly INomenclatureService nomService;

        public CaseClassificationService(
            ILogger<CaseClassificationService> _logger,
            INomenclatureService _nomService,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            nomService = _nomService;
            repo = _repo;
            userContext = _userContext;
        }

        /// <summary>
        /// Зарежда индикаторите по делото заедно с информацията дали е чекнат индикатора или не
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public IList<CheckListVM> FillCheckListVMs(int caseId, int? caseSessionId)
        {
            IList<CheckListVM> checkListVMs = new List<CheckListVM>();

            DateTime dateTomorrow = DateTime.Now.AddDays(1).Date;
            var nomClassification = nomService.GetDropDownList<Classification>(false, false);
            var caseClassification = repo.AllReadonly<CaseClassification>().Where(x => x.CaseId == caseId && 
                                         (x.CaseSessionId ?? 0) == (caseSessionId ?? 0) &&
                                         (x.DateTo ?? dateTomorrow).Date > DateTime.Now.Date
                                         ).ToList();

            foreach (var nom in nomClassification)
            {
                var checkItem = new CheckListVM();
                checkItem.Value = nom.Value;
                checkItem.Label = nom.Text;
                int id = int.Parse(nom.Value);
                checkItem.Checked = caseClassification.Where(x => x.ClassificationId == id).Any();
                checkListVMs.Add(checkItem);
            }

            return checkListVMs;
        }

        /// <summary>
        /// Зарежда индикаторите по делото заедно с информацията дали е чекнат индикатора или не
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public CheckListViewVM CaseClassification_SelectForCheck(int caseId, int? caseSessionId)
        {
            CheckListViewVM checkListViewVM = new CheckListViewVM
            {
                CourtId = caseId,
                ObjectId = caseSessionId ?? 0,
                Label = "Индикатори",
                checkListVMs = FillCheckListVMs(caseId, caseSessionId)
            };

            return checkListViewVM;
        }

        /// <summary>
        /// Запис на индикатори към дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseClassification_SaveData(CheckListViewVM model)
        {
            try
            {
                DateTime dateTomorrow = DateTime.Now.AddDays(1).Date;
                DateTime fromDate = DateTime.Now;
                DateTime toDate = DateTime.Now.AddSeconds(-1);

                var expiryList = repo.All<CaseClassification>()
                    .Where(x => x.CaseId == model.CourtId && (x.CaseSessionId ?? 0) == model.ObjectId && (x.DateTo ?? dateTomorrow).Date > DateTime.Now.Date)
                    .ToList();
                foreach (var item in expiryList)
                {
                    item.DateTo = toDate;
                }
                foreach (var classification in model.checkListVMs)
                {
                    if (classification.Checked == false) continue;
                    CaseClassification newClassification = new CaseClassification();
                    newClassification.CaseId = model.CourtId;
                    newClassification.CourtId = userContext.CourtId;
                    newClassification.ClassificationId = int.Parse(classification.Value);
                    newClassification.CaseSessionId = null;
                    if (model.ObjectId > 0)
                        newClassification.CaseSessionId = model.ObjectId;
                    newClassification.DateFrom = fromDate;
                    repo.Add<CaseClassification>(newClassification);
                }
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CaseClassification CaseId={ model.CourtId }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за индикатори към дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public List<SelectListItem> CaseClassification_Select(int caseId, int? caseSessionId)
        {
            DateTime dateTomorrow = DateTime.Now.AddDays(1).Date;
            return repo.AllReadonly<CaseClassification>()
           .Include(x => x.Classification)
           .Where(x => x.CaseId == caseId && (x.CaseSessionId ?? 0) == (caseSessionId ?? 0) && (x.DateTo ?? dateTomorrow).Date > DateTime.Now.Date)
           .Select(x => new SelectListItem()
           {
               Value = x.Id.ToString(),
               Text = x.Classification.Label,
           }).ToList();
        }

        /// <summary>
        /// Извличане на данни за индикатори по дело/заседание
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public List<CaseClassification> CaseClassification_SelectObject(int caseId, int? caseSessionId)
        {
            DateTime dateTomorrow = DateTime.Now.AddDays(1).Date;
            return repo.AllReadonly<CaseClassification>()
                       .Include(x => x.Classification)
                       .Where(x => x.CaseId == caseId && 
                                   (x.CaseSessionId ?? 0) == (caseSessionId ?? 0) && 
                                   (x.DateTo ?? dateTomorrow).Date > DateTime.Now.Date)
                       .ToList();
        }
    }
}
