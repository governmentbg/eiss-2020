using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class CaseSessionFastDocumentService : BaseService, ICaseSessionFastDocumentService
    {
        private readonly ICasePersonService casePersonService;

        public CaseSessionFastDocumentService(
        ILogger<CaseSessionFastDocumentService> _logger,
        IRepository _repo,
        IUserContext _userContext,
        ICasePersonService _casePersonService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            casePersonService = _casePersonService;
        }

        /// <summary>
        /// Извличане на данни за Съпровождащ документ представен в заседание
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionFastDocumentVM> CaseSessionFastDocument_Select(int CaseSessionId)
        {
            return repo.AllReadonly<CaseSessionFastDocument>()
                       .Include(x => x.CaseSession)
                       .Include(x => x.CasePerson)
                       .ThenInclude(x => x.PersonRole)
                       .Include(x => x.SessionDocType)
                       .Include(x => x.SessionDocState)
                       .Include(x => x.CaseSessionFastDocumentInit)
                       .ThenInclude(x => x.CaseSession)
                       .Where(x => x.CaseSessionId == CaseSessionId &&
                                   x.DateExpired == null)
                       .Select(x => new CaseSessionFastDocumentVM()
                       {
                           Id = x.Id,
                           CasePersonName = x.CasePerson.FullName + " " + "(" + x.CasePerson.PersonRole.Label + ")",
                           SessionDocTypeLabel = x.SessionDocType.Label,
                           SessionDocStateLabel = x.SessionDocState.Label,
                           CaseSessionFastDocumentInitDateSession = ((x.CaseSessionFastDocumentInit != null) ? (x.CaseSessionFastDocumentInit.CaseSession.DateFrom) : (DateTime?)null),
                           DateSession = x.CaseSession.DateFrom
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Извличане на Съпровождащ документ представен в заседание по първоначален документ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionFastDocument> CaseSessionFastDocument_SelectByInitId(int id)
        {
            return repo.AllReadonly<CaseSessionFastDocument>()
                       .Include(x => x.CasePerson)
                       .ThenInclude(x => x.PersonRole)
                       .Include(x => x.SessionDocType)
                       .Include(x => x.SessionDocState)
                       .Where(x => ((x.Id == id) || ((x.CaseSessionFastDocumentInitId ?? 0) == id)) &&
                                   x.DateExpired == null)
                       .AsQueryable();
        }

        /// <summary>
        /// Запис на Съпровождащ документ представен в заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseSessionFastDocument_SaveData(CaseSessionFastDocument model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseSessionFastDocument>(model.Id);
                    saved.CasePersonId = model.CasePersonId;
                    saved.SessionDocTypeId = model.SessionDocTypeId;
                    saved.SessionDocStateId = model.SessionDocStateId;
                    saved.Description = model.Description;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;

                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    repo.Add<CaseSessionFastDocument>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Съпровождащ документ представен в заседание Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за Съпровождащ документ представен в заседание за копиране в друго заседание
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        private List<CaseSessionFastDocument> CaseSessionFastDocument_SelectForCopy(int CaseSessionId)
        {
            return repo.AllReadonly<CaseSessionFastDocument>()
                       .Include(x => x.SessionDocType)
                       .Include(x => x.CasePerson)
                       .ThenInclude(x => x.PersonRole)
                       .Where(x => x.CaseSessionId == CaseSessionId &&
                                   x.SessionDocStateId == NomenclatureConstants.SessionDocState.Presented &&
                                   x.DateExpired == null)
                       .ToList();
        }

        /// <summary>
        /// Извличане на данни за Съпровождащ документ представен в заседание за чеклист
        /// </summary>
        /// <param name="CaseSessionFromId"></param>
        /// <param name="CaseSessionToId"></param>
        /// <returns></returns>
        private IList<CheckListVM> FillCheckListVMs_SelectForCheck(int CaseSessionFromId, int CaseSessionToId)
        {
            IList<CheckListVM> checkListVMs = new List<CheckListVM>();

            var caseSessionFasts = CaseSessionFastDocument_SelectForCopy(CaseSessionFromId);
            var caseSessionFastTos = CaseSessionFastDocument_SelectForCopy(CaseSessionToId);

            foreach (var caseSessionFast in caseSessionFasts)
            {
                if (!caseSessionFastTos.Any(x => ((x.CaseSessionFastDocumentInitId ?? -1) == caseSessionFast.Id || (x.CaseSessionFastDocumentInitId ?? -1) == (caseSessionFast.CaseSessionFastDocumentInitId ?? 0))))
                {
                    checkListVMs.Add(new CheckListVM()
                    {
                        Checked = false,
                        Value = caseSessionFast.Id.ToString(),
                        Label = caseSessionFast.SessionDocType.Label + " " +
                                caseSessionFast.CasePerson.FullName + " " +
                                caseSessionFast.CasePerson.Uic + " - " +
                                caseSessionFast.CasePerson.PersonRole.Label
                    });
                }
            }

            return checkListVMs.OrderBy(x => x.Label).ToList();
        }

        /// <summary>
        /// Извличане на данни за Съпровождащ документ представен в заседание за чеклист
        /// </summary>
        /// <param name="CaseSessionFromId"></param>
        /// <param name="CaseSessionToId"></param>
        /// <returns></returns>
        public CheckListViewVM CaseSessionFastDocument_SelectForSessionCheck(int CaseSessionFromId, int CaseSessionToId)
        {
            CheckListViewVM checkListViewVM = new CheckListViewVM
            {
                CourtId = CaseSessionFromId,
                ObjectId = CaseSessionToId,
                Label = "Изберете документ",
                checkListVMs = FillCheckListVMs_SelectForCheck(CaseSessionFromId, CaseSessionToId)
            };

            return checkListViewVM;
        }

        /// <summary>
        /// Запис на Съпровождащ документ представен в заседание от чеклист
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseSessionFastDocument_SaveSelectForSessionCheck(CheckListViewVM model)
        {
            try
            {
                var caseSession = repo.GetById<CaseSession>(model.ObjectId);
                var caseSessionFasts = CaseSessionFastDocument_SelectForCopy(model.CourtId);
                var casePersonLists = casePersonService.CasePerson_Select(caseSession.CaseId, caseSession.Id, true, false, false);

                foreach (var fastDocument in model.checkListVMs.Where(x => x.Checked))
                {
                    var sessionFastDocument = caseSessionFasts.Where(x => x.Id == int.Parse(fastDocument.Value)).FirstOrDefault();
                    var person = casePersonLists.Where(x => x.Uic == sessionFastDocument.CasePerson.Uic).FirstOrDefault();

                    if ((sessionFastDocument != null) && (person != null))
                    {
                        var caseSessionFastDocumentSave = new CaseSessionFastDocument()
                        {
                            CourtId = caseSession.CourtId ?? 0,
                            CaseId = caseSession.CaseId,
                            CaseSessionId = caseSession.Id,
                            CasePersonId = person.Id,
                            SessionDocTypeId = sessionFastDocument.SessionDocTypeId,
                            Description = sessionFastDocument.Description,
                            SessionDocStateId = sessionFastDocument.SessionDocStateId,
                            CaseSessionFastDocumentInitId = ((sessionFastDocument.CaseSessionFastDocumentInitId != null) ? sessionFastDocument.CaseSessionFastDocumentInitId : sessionFastDocument.Id),
                            DateWrt = DateTime.Now,
                            UserId = userContext.UserId
                        };

                        repo.Add<CaseSessionFastDocument>(caseSessionFastDocumentSave);
                    }
                }
                
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при копиране на Съпровождащ документ представен в заседание от заседание Id={ model.CourtId } в заседание Id={model.ObjectId}");
                return false;
            }
        }
    }
}
