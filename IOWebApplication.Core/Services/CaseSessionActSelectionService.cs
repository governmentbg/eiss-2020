using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
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
    public class CaseSessionActSelectionService : BaseService, ICaseSessionActSelectionService
    {
        public CaseSessionActSelectionService(IRepository _repo, ILogger<CaseSessionActSelectionService> _logger, IUserContext _userContext)
        {
                repo = _repo;
                logger = _logger;
                userContext = _userContext;
        }
        public IQueryable<CaseSessionActSelectionProtocolVM> CaseSessionActSelectionProtol_sel(int courtId, CaseSessionActSelectionProtocolFilterVM model)
        {
            DateTime dateNow = DateTime.Now;
            DateTime dateFromSearch = model.DateFrom ?? DateTime.Now.AddYears(-100);
            DateTime dateToSearch = model.DateTo ?? DateTime.Now.AddYears(100);
            DateTime dateAddYear = DateTime.Now.AddYears(100);
            DateTime dateTimeBegin = new DateTime(1900, 1, 1);

            Expression<Func<CaseSessionActSelectionProtokol, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.SelectionDate >= dateFromSearch.ForceStartDate() && x.SelectionDate <= dateToSearch.ForceEndDate();            

            Expression<Func<CaseSessionActSelectionProtokol, bool>> finalActSearch = x => true;
            if (model.IsFinal == true)
                finalActSearch = x => x.IsFinal == true;

            Expression<Func<CaseSessionActSelectionProtokol, bool>> IsCancelingSearch = x => true;
            if (model.IsCanceling == true)
                IsCancelingSearch = x => x.IsCanceling == true;
            Expression<Func<CaseSessionActSelectionProtokol, bool>> IsBeacameFinalSearch = x => true;
            if (model.IsBeacameFinal == true)
                IsBeacameFinalSearch = x => x.IsBeacameFinal == true;       

            Expression<Func<CaseSessionActSelectionProtokol, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.SelectedLawUnitId==model.JudgeReporterId;
            Expression<Func<CaseSessionActSelectionProtokol, bool>> ResultIdSearch = x => true;
            if (model.ActResultId!=null)
                if (model.ActResultId >0)
                    ResultIdSearch = x => x.ActResultId == model.ActResultId;


            return repo.AllReadonly<CaseSessionActSelectionProtokol>()
                       .Where(x => x.CourtId == courtId)
                           .Where(dateSearch)
                       .Where(finalActSearch)
                       .Where(IsCancelingSearch)
                       .Where(IsBeacameFinalSearch)
                       .Where(judgeReporterSearch)
                        .Where(ResultIdSearch)
                       .Include(x => x.SelectedLawUnit)

                       .Select(x => new CaseSessionActSelectionProtocolVM()
                       {
                           Id = x.Id,
                           SelectedLawUnitId = x.SelectedLawUnitId,
                           SelectedLawUnitName = x.SelectedLawUnit.FullName,
                           IsBeacameFinal = x.IsBeacameFinal,
                           IsCanceling = x.IsCanceling,
                           IsFinal = x.IsFinal,
                           DateFrom = x.FromDate,
                           DateTo = x.ToDate,
                           ActResultId = x.ActResultId,
                           SelectionDate = x.SelectionDate,
                           IsFinalStr = x.IsFinal == true ? "Финализиращ" : "",
                           IsCancelingstr = x.IsCanceling==true ? "За прекратяване" : "",
                           IsBeacameFinalStr = x.IsBeacameFinal == true ? "Влязъл в сила" : "",
                           SelectionProtocolStateName = x.SelectionProtokolStateId == NomenclatureConstants.SelectionProtokolState.Signed ? "Подписан" : "Изготвен документ",


                       })
                      
                       .AsQueryable();
        }

       public IQueryable<CaseSessionActSelectionActVM> CaseSessionActSelectionAct_sel(int courtId, CaseSessionActSelectionProtocolFilterVM model)
        {
            DateTime dateNow = DateTime.Now;
            DateTime dateFromSearch = model.DateFrom ?? DateTime.Now.AddYears(-100);
            DateTime dateToSearch = model.DateTo ?? DateTime.Now.AddYears(100);
            DateTime dateAddYear = DateTime.Now.AddYears(100);
            DateTime dateTimeBegin = new DateTime(1900, 1, 1);

            Expression<Func<CaseSessionAct, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate >= dateFromSearch.ForceStartDate() && x.RegDate <= dateToSearch.ForceEndDate();

        


            Expression<Func<CaseSessionAct, bool>> finalActSearch = x => true;
            if (model.IsFinal == true)
                finalActSearch = x => x.IsFinalDoc == true && NomenclatureConstants.SessionActState.EnforcedStates.Contains(x.ActStateId);
     Expression<Func<CaseSessionAct, bool>> IsCancelingSearch = x => true;
            var canceling_results = repo.AllReadonly<SessionResult>().Where(x => x.SessionResultGroupId == 1).Select(x => x.Id).ToList();

            if (model.IsCanceling == true)
            {
     
                IsCancelingSearch = x => x.CaseSession.CaseSessionResults.Any(r=> r.IsMain && r.IsActive&& canceling_results.Contains(r.SessionResultId));
            }

            Expression<Func<CaseSessionAct, bool>> IsBeacameFinalSearch = x => true;
            if (model.IsBeacameFinal == true)
                IsBeacameFinalSearch = x => x.ActInforcedDate != null;

            Expression<Func<CaseSessionAct, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseSession.CaseLawUnits.Where(a => (a.DateTo ?? dateAddYear).Date >= x.CaseSession.DateFrom && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            Expression<Func<CaseSessionAct, bool>> ResultIdSearch = x => true;
            if (model.ActResultId != null)
                if (model.ActResultId > 0)
                    ResultIdSearch = x => x.ActComplainResult.Id == model.ActResultId;


            return repo.AllReadonly<CaseSessionAct>()
                       .Where(x => x.CourtId == courtId)
                       .Where(x => x.DateExpired == null)
                       .Where(dateSearch)
                       .Where(finalActSearch)
                       .Where(IsCancelingSearch)
                       .Where(judgeReporterSearch)
                       .Where(ResultIdSearch)
                       .Select(x => new CaseSessionActSelectionActVM()
                       {
                           Id = x.Id,
                           //CaseSessionId = x.CaseSessionId,
                           //CaseId = x.CaseId ?? 0,
                           //CaseSessionDate = x.CaseSession.DateFrom,
                           //CaseSessionLabel = x.CaseSession.SessionType.Label + "/" + x.CaseSession.DateFrom.ToString("dd.MM.yyyy HH:mm"),
                           Vid_RegNumber=x.ActType.Label + " Nº " + x.RegNumber,
                           RegDate = x.RegDate.Value,
                           CaseType_CaseLabel = x.Case.CaseType.Code  + " Nº " + x.Case.RegNumber,
                           ActInforcedDate=x.ActInforcedDate,
                           Result=x.ActResult.Label,
                           ResultId=x.ActResultId,
                           IsFinal=x.IsFinalDoc,    
                           IsCanceling= x.CaseSession.CaseSessionResults.Any(r => r.IsMain && r.IsActive && canceling_results.Contains(r.SessionResultId)),
                           IsBeacameFinal= (x.ActInforcedDate != null),



            JudgeReport = x.CaseSession.CaseLawUnits.Where(l => l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                                               (l.DateTo ?? dateAddYear) >= x.CaseSession.DateFrom)
                                                                   .Select(l => l.LawUnit.FullName)
                                                                   .FirstOrDefault()

                       })
                       .AsQueryable();
        }


        public async Task<CaseSessionActSelectionProtocolVM> GetSelectionActProtocolByID(int id)
        {
            var canceling_results = await repo.AllReadonly<SessionResult>().Where(x => x.SessionResultGroupId == 1).Select(x => x.Id).ToListAsync();
            var result = await repo.AllReadonly<CaseSessionActSelectionProtokol>()
                .Where(x=>x.Id== id)
                .Select(x=>new CaseSessionActSelectionProtocolVM()
                {
                    Id = x.Id,
                    SelectedLawUnitId = x.SelectedLawUnitId,
                    SelectedLawUnitName = x.SelectedLawUnit.FullName,
                    IsBeacameFinal = x.IsBeacameFinal,
                    IsCanceling = x.IsCanceling,
                    IsFinal = x.IsFinal,
                    DateFrom = x.FromDate,
                    DateTo = x.ToDate,
                    ActResultId = x.ActResultId,
                    SelectionDate = x.SelectionDate,
                    IsFinalStr = x.IsFinal == true ? "Финализиращ" : "",
                    IsCancelingstr = x.IsCanceling == true ? "За прекратяване" : "",
                    IsBeacameFinalStr = x.IsBeacameFinal == true ? "Влязъл в сила" : "",
                    SelectionProtocolStateId=x.SelectionProtokolStateId,
                    SelectionProtocolStateName=x.SelectionProtokolStateId==NomenclatureConstants.SelectionProtokolState.Signed ? "Подписан": "Изготвен документ",
                    UserName = x.User.LawUnit.FullName,
                    UserUIK = x.User.LawUnit.Uic,


                    SelectionActs =x.CaseSessionActSelectionProtocolActs.Select(a=> new CaseSessionActSelectionActVM
                    {
                        Id=a.Id,
                        Vid_RegNumber=a.Act.ActType.Label+ " Nº " + a.Act.RegNumber,
                        RegDate = a.Act.ActDate,
                        CaseType_CaseLabel = a.Act.Case.CaseType.Code + " Nº " + a.Act.Case.RegNumber,
                        ActInforcedDate = a.Act.ActInforcedDate,
                        Result = a.Act.ActResult.Label,
                        ResultId = a.ActResultId,
                        IsFinal = a.Act.IsFinalDoc,
                        IsCanceling = a.IsCanceling,
                        IsBeacameFinal = ( a.Act.ActInforcedDate != null),
                        IsSelected=a.IsSelected

                    }
                    ).ToList()
                }
                     ).FirstOrDefaultAsync();

            return result;
        }

        public async Task<int> CreateActSelection(int courtId, CaseSessionActSelectionProtocolFilterVM model)

        {   int result=0;
            DateTime dateNow = DateTime.Now;
            DateTime dateFromSearch = model.DateFrom ?? DateTime.Now.AddYears(-100);
            DateTime dateToSearch = model.DateTo ?? DateTime.Now.AddYears(100);
            DateTime dateAddYear = DateTime.Now.AddYears(100);
            DateTime dateTimeBegin = new DateTime(1900, 1, 1);

            Expression<Func<CaseSessionAct, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate >= dateFromSearch.ForceStartDate() && x.RegDate <= dateToSearch.ForceEndDate();




            Expression<Func<CaseSessionAct, bool>> finalActSearch = x => true;
            if (model.IsFinal == true)
                finalActSearch = x => x.IsFinalDoc == true && NomenclatureConstants.SessionActState.EnforcedStates.Contains(x.ActStateId);
            var canceling_results = repo.AllReadonly<SessionResult>().Where(x => x.SessionResultGroupId == 1).Select(x => x.Id).ToList();
            Expression<Func<CaseSessionAct, bool>> IsCancelingSearch = x => true;
            if (model.IsCanceling == true)
            {

                IsCancelingSearch = x => canceling_results.Contains(x.ActResultId ?? 0);
            }

            Expression<Func<CaseSessionAct, bool>> IsBeacameFinalSearch = x => true;
            if (model.IsBeacameFinal == true)
                IsBeacameFinalSearch = x => x.ActInforcedDate != null;

            Expression<Func<CaseSessionAct, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseSession.CaseLawUnits.Where(a => (a.DateTo ?? dateAddYear).Date >= x.CaseSession.DateFrom && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();



            CaseSessionActSelectionProtokol protokol = new CaseSessionActSelectionProtokol(); 
            protokol.CourtId = courtId;
            protokol.SelectedLawUnitId = model.JudgeReporterId;
            protokol.SelectionDate = DateTime.Now;
            protokol.FromDate = model.DateFrom.Value;
            protokol.ToDate = model.DateTo.Value;
            protokol.IsFinal = model.IsFinal;
            protokol.IsCanceling = model.IsCanceling;
            protokol.IsBeacameFinal=model.IsBeacameFinal;
            protokol.ActResultId = model.ActResultId;
            protokol.SelectionProtokolStateId = NomenclatureConstants.SelectionProtokolState.Generated;
            protokol.UserId = userContext.UserId;
            protokol.CaseSessionActSelectionProtocolActs = await repo.AllReadonly<CaseSessionAct>()
                       .Where(x => x.CourtId == courtId)
                       .Where(x => x.DateExpired == null)
                       .Where(dateSearch)
                       .Where(finalActSearch)
                       .Where(IsCancelingSearch)
                       .Where(judgeReporterSearch)
                       .Select(x => new CaseSessionActSelectionProtocolAct()
                       {
                           CaseSessionActId = x.Id,
                           CaseId = x.CaseId.Value,


                           IsFinal = x.IsFinalDoc,
                           IsCanceling = x.CaseSession.CaseSessionResults.Any(r => r.IsMain && r.IsActive && canceling_results.Contains(r.SessionResultId)),
                           IsBeacameFinal = (x.ActInforcedDate != null),
                           IsSelected=false,
                           ActResultId = x.ActResultId   // Da se proveri dali ne e complain
                           


                       })
                       .ToListAsync();
            var acts = protokol.CaseSessionActSelectionProtocolActs.ToList();

            if (acts.Count >= 3)
            {
                var rnd = new Random();

                var selectedActs = acts.OrderBy(x => rnd.Next()).Take(3).ToList();

                foreach (var act in selectedActs)
                {
                    act.IsSelected = true;
                }
            }
            else
            {
                
                foreach (var act in acts)
                {
                    act.IsSelected = true;
                }
            }

            protokol.CaseSessionActSelectionProtocolActs = acts;

           repo.Add<CaseSessionActSelectionProtokol>(protokol);

           await repo.SaveChangesAsync();
            result = protokol.Id;

            return result;
            ;
        }

        public async Task<int> ActSelectionProtokol_SignUpdate(int id)
        {
            int result = 0;
            var protocol= await repo.All<CaseSessionActSelectionProtokol>()
                .Where(x => x.Id == id).FirstOrDefaultAsync();
            if (protocol != null)
            { protocol.SelectionProtokolStateId = NomenclatureConstants.SelectionProtokolState.Signed;
                result = protocol.Id;
        
                await repo.SaveChangesAsync();  
            }
             
          
            return result;
                
        }


    }


}
