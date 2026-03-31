using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Extensions.HTML;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using iText.Kernel.Pdf.Tagging;
using Mapster;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class CaseSelectionProtocolSubstitutionService : BaseService, ICaseSelectionProtocolSubstitutionService
    {
        private readonly ICourtLoadPeriodService courtLoadPeriodService;
        public CaseSelectionProtocolSubstitutionService(IRepository _repo,
            ILogger<CaseSessionActSelectionService> _logger,
            IUserContext _userContext,
            ICourtLoadPeriodService _courtLoadPeriodService)
        {
                repo = _repo;
                logger = _logger;
                userContext = _userContext;
                courtLoadPeriodService = _courtLoadPeriodService;
        }

        public IQueryable<CaseSelectionProtocolSubstitutionVM> CaseSelectionProtolSubstitution_sel(int courtId, CaseSelectionProtocolSubstitutionFilterVM model)
        {
            DateTime dateNow = DateTime.Now;
            DateTime dateFromSearch = model.DateFrom ?? DateTime.Now.AddYears(-100);
            DateTime dateToSearch = model.DateTo ?? DateTime.Now.AddYears(100);
            DateTime dateAddYear = DateTime.Now.AddYears(100);
            DateTime dateTimeBegin = new DateTime(1900, 1, 1);

            Expression<Func<CaseSelectionProtokolSubstitution, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.SelectionDate >= dateFromSearch.ForceStartDate() && x.SelectionDate <= dateToSearch.ForceEndDate();            

           
            Expression<Func<CaseSelectionProtokolSubstitution, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.SelectedLawUnitId==model.JudgeReporterId;
            Expression<Func<CaseSelectionProtokolSubstitution, bool>> SubstitutedjudgeReporterSearch = x => true;
            if (model.SubstitudedJudgeId > 0)
                SubstitutedjudgeReporterSearch = x => x.SubstitutedLawUnitId == model.SubstitudedJudgeId;

            Expression<Func<CaseSelectionProtokolSubstitution, bool>> CourtGroupSearch = x => true;
            if (model.CourtGroupId > 0)
                CourtGroupSearch = x => x.CourtGroupId == model.CourtGroupId;

            return repo.AllReadonly<CaseSelectionProtokolSubstitution>()
                       .Where(x => x.CourtId == courtId)
                       .Where(dateSearch)
                       .Where(judgeReporterSearch)
                       .Where(SubstitutedjudgeReporterSearch)
                        .Where(CourtGroupSearch)
                       .Include(x => x.SelectedLawUnit)

                       .Select(x => new   CaseSelectionProtocolSubstitutionVM
()
                       {
                           Id = x.Id,
                           SelectedLawUnitId = x.SelectedLawUnitId,
                           SelectedLawUnitName = x.SelectedLawUnit.FullName,
                           SubstitudedLawUnitId = x.SubstitutedLawUnitId,
                           SubstitutedLawUnitName = x.SubstitutedLawUnit.FullName,
                           SubstitutionDateFrom = x.SubstitutionDate ,
                           SubstitutionDateTo = x.SubstitutionDate ,
                           SelectionDate= x.SelectionDate,
                           SelectionProtocolStateId=x.SelectionProtokolStateId,
                           SelectionProtocolStateName=x.SelectionProtokolState.Label
                     


                       })
                      
                       .AsQueryable();
        }

        /// <summary>
        /// Метод извличащ данни за избрани протоколи за случаен избор на заместващ в дело/заседание
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public IQueryable<CaseSelectionSubstitutionListDataVM> GetCaseSelectionSubstitution(int caseId)
        {
            return repo.AllReadonly<CaseSelectionSubstitution>()
                       .Where(x => x.CaseId == caseId)
                       .Select(x => new CaseSelectionSubstitutionListDataVM()
                       {
                           Id = x.CaseSelectionProtokolSubstitutionId,
                           DateWrt = x.DateWrt,
                           SessionLabel = $"{x.CaseSession.SessionType.Label} на: {x.CaseSession.DateFrom.ToString("dd.MM.yyyy HH:mm")}",
                           SelectionDate = x.CaseSelectionProtokolSubstitution.SelectionDate,
                           SubstitutedLawUnitName = x.CaseSelectionProtokolSubstitution.SubstitutedLawUnit.FullName,
                           SelectedLawUnitName = x.CaseSelectionProtokolSubstitution.SelectedLawUnit.FullName
                       });
        }

        public bool SubstitutionSelectionProtokol_SaveData(CaseSelectionProtokolVM model, ref string errorMessage)
        {
            try
            {
                //var caseLawUnits = repo.AllReadonly<CaseLawUnit>().Where(x => x.CaseId == model.CaseId && x.CaseSessionId == null).ToList();

                foreach (var item in model.LawUnits)
                {
                    if (item.LawUnitFullName == null)
                    {
                        item.LawUnitFullName = repo.AllReadonly<LawUnit>().Where(x => x.Id == item.LawUnitId).FirstOrDefault().FullName;
                    }
                }

                //if (CheckForJudgeReporter(model, caseLawUnits, ref errorMessage) == false) return false;
                //if (CheckForExistLawUnit(model, caseLawUnits, ref errorMessage) == false) return false;
                //if (CheckForDismisal(model, ref errorMessage) == false) return false;

                //model.CourtDepartmentId = model.CourtDepartmentId.EmptyToNull();
                //model.CourtDutyId = model.CourtDutyId.EmptyToNull();

                //model.SpecialityId = model.SpecialityId.EmptyToNull();
                //if (model.SpecialityId != null)
                //{ if (model.SpecialityId < 0) { model.SpecialityId = null; } }
                //model.CaseLawUnitDismisalId = model.CaseLawUnitDismisalId.EmptyToNull();

                CaseSelectionProtokolSubstitution CaseSelectionProtokolSubstitution = null;

                CaseSelectionProtokolSubstitution = new CaseSelectionProtokolSubstitution();
                CaseSelectionProtokolSubstitution.CourtId = model.CourtId;
                CaseSelectionProtokolSubstitution.CourtGroupId = model.CourtGroupId.Value;
                CaseSelectionProtokolSubstitution.SubstitutedLawUnitId = model.SubstitudedJudgeId.Value;
                CaseSelectionProtokolSubstitution.SelectionDate = DateTime.Now;
                CaseSelectionProtokolSubstitution.DeclareDate = DateTime.Now.AddYears(-200);
                CaseSelectionProtokolSubstitution.SelectionModeId = model.SelectionModeId;
                CaseSelectionProtokolSubstitution.Description = model.DescriptionSubstitution;
                CaseSelectionProtokolSubstitution.SelectionProtokolStateId = NomenclatureConstants.SelectionProtokolState.Generated;
                CaseSelectionProtokolSubstitution.SubstitutionDate = model.DateFrom.Value;
                CaseSelectionProtokolSubstitution.SubstitutionToDate = model.DateTo.Value;



                CaseSelectionProtokolSubstitution.LawUnits = new List<CaseSelectionProtokolSubstitutionLawUnit>();
                foreach (var lawUnit in model.LawUnits)
                {
                    var lawUnitNew = new CaseSelectionProtokolSubstitutionLawUnit();
                    lawUnitNew.CourtId = userContext.CourtId;
                    lawUnitNew.LawUnitId = lawUnit.LawUnitId;
                    lawUnitNew.LoadIndex = lawUnit.LoadIndex;
                    lawUnitNew.StateId = lawUnit.StateId;
                    lawUnitNew.Description = lawUnit.Description;
                    lawUnitNew.CaseCount = lawUnit.CaseCount;
               
                    lawUnitNew.DateWrt = CaseSelectionProtokolSubstitution.SelectionDate;
                    lawUnitNew.UserId = userContext.UserId;
                    CaseSelectionProtokolSubstitution.LawUnits.Add(lawUnitNew);
                }

                CaseSelectionProtokolSubstitution.UserId = userContext.UserId;
                CaseSelectionProtokolSubstitution.DateWrt = DateTime.Now;


                SubstitutionSetSelectedLawUnit_SaveCaseLawUnit(CaseSelectionProtokolSubstitution, model);
                CaseSelectionProtokolSubstitution.LawUnits = SubstitutionSetTotalCourtCaseCount_LawUnit(CaseSelectionProtokolSubstitution.LawUnits, CaseSelectionProtokolSubstitution.CourtId, NomenclatureConstants.JudgeRole.JudgeReporter, CaseSelectionProtokolSubstitution.SelectedLawUnitId);


                repo.Add<CaseSelectionProtokolSubstitution>(CaseSelectionProtokolSubstitution);
                repo.SaveChanges();


                model.Id = CaseSelectionProtokolSubstitution.Id;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CaseSelectionProtokol Id={model.Id}");
                return false;
            }
        }
        private List<CaseSelectionProtokolSubstitutionLawUnit> SubstitutionSetTotalCourtCaseCount_LawUnit(ICollection<CaseSelectionProtokolSubstitutionLawUnit> lawUnits, int courtId, int? judge_role_id, int? selectedLawUnit = -1)
        {
            List<CaseSelectionProtokolSubstitutionLawUnit> result = new List<CaseSelectionProtokolSubstitutionLawUnit>();


            foreach (var item in lawUnits)
            {
                DateTime dt = DateTime.Now;

                var countCC = repo.AllReadonly<CourtLoadPeriodLawUnit>()
                                  //За да отчете период за нулиране на натоварване
                                  .Where(x => (x.CourtLoadPeriod.CourtLoadResetPeriod.DateTo ?? dt) >= dt && x.CourtLoadPeriod.CourtLoadResetPeriod.DateFrom < dt)
                                  .Where(x => x.CourtLoadPeriod.CourtLoadResetPeriod.CourtId == courtId)
                                  .Where(x => x.LawUnitId == item.LawUnitId)
                                     .Select(x => x.DayCases).Sum();


                if (item.LawUnitId == selectedLawUnit && countCC > 0 && judge_role_id == NomenclatureConstants.JudgeRole.JudgeReporter)

                {
                    countCC = countCC - 1;
                }
                item.CaseCourtCount = (int)countCC;
                result.Add(item);
            }

            return result;
        }
        private void SubstitutionSetSelectedLawUnit_SaveCaseLawUnit(CaseSelectionProtokolSubstitution substitutionSelectionProtokol, CaseSelectionProtokolVM caseSelectionProtokolVM, bool isAutomaticMassSelection = false)
        {

            courtLoadPeriodService.MakeDaylyLoadPeriodLawuitRowsByGroup(caseSelectionProtokolVM);
            courtLoadPeriodService.CalculateAllKoef(caseSelectionProtokolVM);

            //Изключване за голямо отклонение
            decimal case_deviation = 10;
            decimal deviation_percent = 10;

            try
            {
                var param_deviation = SystemParam_Select("case_deviation");
                case_deviation = decimal.Parse(param_deviation.ParamValue);
            }
            catch (Exception)
            {


            }

            try
            {
                var param_deviation_percent = SystemParam_Select("case_deviation_percent");
                deviation_percent = decimal.Parse(param_deviation_percent.ParamValue);
            }
            catch (Exception)
            {


            }
            foreach (var lu1 in caseSelectionProtokolVM.LawUnits)

            {
                if (NomenclatureConstants.SelectionProtokolLawUnitState.ActiveState.Contains(lu1.StateId))
                {

                    foreach (var lu2 in caseSelectionProtokolVM.LawUnits.Where(x => NomenclatureConstants.SelectionProtokolLawUnitState.ActiveState.Contains(x.StateId)))

                    {
                        if (lu1.LawUnitId != lu2.LawUnitId)
                        {
                            if (lu1.CasesCountIfWorkAllPeriodInGroup > lu2.CasesCountIfWorkAllPeriodInGroup && NomenclatureConstants.SelectionProtokolLawUnitState.ActiveState.Contains(lu2.StateId))
                            {
                                decimal diff = lu1.CasesCountIfWorkAllPeriodInGroup - lu2.CasesCountIfWorkAllPeriodInGroup;
                                decimal deviation = diff / (lu2.CasesCountIfWorkAllPeriodInGroup + (decimal)0.001) * 100M;
                                //Залагам отклонение 40%  до 10 дела  и отспоред конфигурацията  над 10
                                deviation = Math.Abs(deviation);
                                decimal curent_deviation_percent = deviation_percent;
                                if (lu1.TotalCaseCount < 10)
                                { curent_deviation_percent = 40; }
                                if (deviation > curent_deviation_percent)
                                {
                                    lu1.ExcludeByBigDeviation = true;
                                    break;
                                }


                            }
                        }


                    }
                }
            }
            //Изключване за голямо отклонение


            List<int> lawUnits = new List<int>();
            var protokolLawUnits = new List<CaseSelectionProtokolLawUnitVM>();

            decimal activeNormalizedKoef = 0;
            foreach (var item in caseSelectionProtokolVM.LawUnits)
            {
                if (NomenclatureConstants.SelectionProtokolLawUnitState.ActiveState.Contains(item.StateId) && item.ExcludeByBigDeviation == false)
                {
                    CaseSelectionProtokolLawUnitVM itemNew = new CaseSelectionProtokolLawUnitVM();
                    itemNew.LawUnitId = item.LawUnitId;
                    itemNew.KoefNormalized = item.KoefNormalized;
                    activeNormalizedKoef = activeNormalizedKoef + item.KoefNormalized;
                    protokolLawUnits.Add(itemNew);
                }
            }
            //Нормализиране на активнике коефициенти към 1000
            foreach (var item in protokolLawUnits)
            {
                item.KoefNormalized = item.KoefNormalized / activeNormalizedKoef * 1000;


            }
            //int maxLoadindex = (protokolLawUnits.Select(x => x.KoefNormalized).Max());
            bool exitLoop = false;

            

            //Добавя билети последователно Start


            foreach (var item in protokolLawUnits)
            {
                while (item.KoefNormalized > 0)
                {
                    lawUnits.Add(item.LawUnitId);
                    item.KoefNormalized = item.KoefNormalized - 1;
                }
            }


            //Добавя билети последователно END
            //Избор след като 2 пъти рандом се избере един  съдия
            List<int> LawUnnitsSelectedForFirstTime = new List<int>();
            exitLoop = false;
            int r = 0;
            while (exitLoop == false)
            {
                Random rnd = new Random();
                r = rnd.Next(lawUnits.Count);

                if (LawUnnitsSelectedForFirstTime.Contains(lawUnits[r]))
                {
                    substitutionSelectionProtokol.SelectedLawUnitId = lawUnits[r];
                    exitLoop = true;
                }
                else
                { LawUnnitsSelectedForFirstTime.Add(lawUnits[r]); }


            }



            try
            {
                //TODO: За Заповедно да се види
                if (caseSelectionProtokolVM.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                {
                    if (isAutomaticMassSelection)
                    {
                        courtLoadPeriodService.UpdateDailyLoadPeriod_AutomaticMassSelection(caseSelectionProtokolVM.CourtGroupId, caseSelectionProtokolVM.CourtDutyId, lawUnits[r]);
                    }
                    else
                    {
                        courtLoadPeriodService.UpdateDailyLoadPeriod(caseSelectionProtokolVM.CourtGroupId, caseSelectionProtokolVM.CourtDutyId, lawUnits[r], caseSelectionProtokolVM.SelectionModeId);
                    }
                }
                courtLoadPeriodService.SubstitutionMergeCaseSelectionProtokolAndVM(substitutionSelectionProtokol, caseSelectionProtokolVM);
            }
            catch (Exception ex)
            {

            }
        }
        


        public async Task<CaseSelectionProtocolSubstitutionVM> GetSelectionProtocolSubstitutionByID(int id)
        {
        
            var result = await repo.AllReadonly<CaseSelectionProtokolSubstitution>()
                .Where(x => x.Id == id)
                .Select(x => new CaseSelectionProtocolSubstitutionVM()
                {
                    Id = x.Id,
                    CourtName=x.Court.Label,
                    SelectedLawUnitId = x.SelectedLawUnitId,
                    SelectedLawUnitName = x.SelectedLawUnit.FullName,
                    SubstitudedLawUnitId=x.SubstitutedLawUnitId,
                    SubstitutedLawUnitName=x.SubstitutedLawUnit.FullName,
                    SubstitutionDateFrom=x.SubstitutionDate,
                    SubstitutionDateTo=x.SubstitutionToDate,
                    SelectionTypeName=x.SelectionMode.Label,
                    SelectionDate=x.SelectionDate,
                    DeclareDate=x.DeclareDate,
                    SelectionProtocolStateId = x.SelectionProtokolStateId,
                    SelectionProtocolStateName = x.SelectionProtokolState.Label,
                    UserName = x.User.LawUnit.FullName,
                    UserUIK = x.User.LawUnit.Uic,
                    Description = x.Description,
                    CourtGroupName=x.CourtGroup.Label,
                    


                    ProtokolLawUnit = x.LawUnits.Select(p => new CaseSelectionProtokolLawUnitVM
                  
                      {
                          Id = p.LawUnitId,
                          LawUnitFullName = p.LawUnit.FullName,
                          LoadIndex = p.LoadIndex,
                          StateId = p.StateId,
                          CaseCount = p.CaseCount,
                                       
                          Description = p.Description

                      }
                    ).ToList()
                }
                     ).FirstOrDefaultAsync();

            return result;
        }

        public async Task<int> SelectionProtokolSubstitution_SignUpdate(int id)
        {
            int result = 0;
            var protocol = await repo.All<CaseSelectionProtokolSubstitution>()
                .Where(x => x.Id == id).FirstOrDefaultAsync();
            if (protocol != null)
            {
                protocol.SelectionProtokolStateId = NomenclatureConstants.SelectionProtokolState.Signed;
                result = protocol.Id;

                await repo.SaveChangesAsync();
            }


            return result;

        }


    }


}
