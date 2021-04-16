using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Documents;
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
    public class CaseSessionDocService: BaseService, ICaseSessionDocService
    {
        public CaseSessionDocService(
        ILogger<CaseSessionDocService> _logger,
        IRepository _repo,
        IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }

        /// <summary>
        /// Извличане на данни за Съпровождащи документи, прикачени към заседание
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionDocVM> CaseSessionDoc_Select(int CaseSessionId)
        {
            return repo.AllReadonly<CaseSessionDoc>()
                       .Include(x => x.CaseSession)
                       .Include(x => x.Document)
                       .ThenInclude(x => x.DocumentType)
                       .Include(x => x.SessionDocState)
                       .Where(x => x.CaseSessionId == CaseSessionId &&
                                   x.DateExpired == null)
                       .Select(x => new CaseSessionDocVM()
                       {
                           Id = x.Id,
                           CaseSessionId = x.CaseSessionId,
                           DocumentId = x.DocumentId,
                           DocumentLabel = (x.Document != null) ? x.Document.DocumentNumber + " / " + x.Document.DocumentDate.ToString("dd.MM.yyyy") + " / " + x.Document.DocumentType.Label : string.Empty,
                           SessionDocStateLabel = (x.SessionDocState != null) ? x.SessionDocState.Label : string.Empty,
                           DateFrom = x.DateFrom
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Запис на Съпровождащи документи, прикачени към заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseSessionDoc_SaveData(CaseSessionDoc model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseSessionDoc>(model.Id);
                    saved.SessionDocStateId = model.SessionDocStateId;
                    saved.Description = model.Description;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo;

                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    repo.Add<CaseSessionDoc>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                //logger.log(ex)
                return false;
            }
        }

        /// <summary>
        /// Извличанена данни за Съпровождащи документи, прикачени към заседание за чеклист
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private CheckListVM FillCheckListVM(Document document)
        {
            CheckListVM checkListVM = new CheckListVM
            {
                Value = document.Id.ToString(),
                Label = document.DocumentNumber + " / " + document.DocumentDate.ToString("dd.MM.yyyy") + " / " + document.DocumentType.Label,
                Checked = false
            };

            return checkListVM;
        }

        /// <summary>
        /// Извличане на данни за Съпровождащи документи, прикачени към заседание за чеклист
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <param name="SessionDateFrom"></param>
        /// <returns></returns>
        private IList<CheckListVM> FillCheckListVMs(int caseId, int caseSessionId, DateTime SessionDateFrom)
        {
            IList<CheckListVM> checkListVMs = new List<CheckListVM>();

            var documents = repo.AllReadonly<DocumentCaseInfo>()
                                .Include(x => x.Document)
                                .ThenInclude(x => x.DocumentType)
                                .Where(x => x.CaseId == caseId &&
                                            x.Document.DocumentDate.Date <= SessionDateFrom.Date &&
                                            x.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument &&
                                            x.Document.DateExpired == null)
                                .ToList();

            var casesessiondocs = repo.AllReadonly<CaseSessionDoc>()
                                      .Where(x => x.CaseSessionId == caseSessionId &&
                                                  x.DateExpired == null)
                                      .ToList();

            foreach (var doc in documents)
            {
                if (!casesessiondocs.Any(x => x.DocumentId == doc.DocumentId))
                    checkListVMs.Add(FillCheckListVM(doc.Document));
            }

            return checkListVMs.OrderBy(x => x.Label).ToList();
        }

        /// <summary>
        /// Извличане на данни за чеклис за Съпровождащи документи, прикачени към заседание
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        public CheckListViewVM CheckListViewVM_Fill(int CaseSessionId)
        {
            var session = repo.GetById<CaseSession>(CaseSessionId);

            CheckListViewVM checkListViewVM = new CheckListViewVM
            {
                CourtId = session.CaseId,
                ObjectId = session.Id,
                Label = "Изберете документи ",
                checkListVMs = FillCheckListVMs(session.CaseId, session.Id, session.DateFrom)
            };

            return checkListViewVM;
        }

        /// <summary>
        /// Попълване на основен обект за Съпровождащи документи, прикачени към заседание
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <param name="DocumentId"></param>
        /// <returns></returns>
        private CaseSessionDoc FillData(int CaseSessionId, int DocumentId)
        {
            var session = repo.GetById<CaseSession>(CaseSessionId);
            return new CaseSessionDoc()
            {
                CourtId = session.CourtId,
                CaseId = session.CaseId,
                CaseSessionId = CaseSessionId,
                DocumentId = DocumentId,
                SessionDocStateId = NomenclatureConstants.SessionDocState.Nerazgledan,
                Description = "Неразгледан",
                DateFrom = DateTime.Now
            };
        }

        /// <summary>
        /// Пълнене на лист от Съпровождащи документи, прикачени към заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<CaseSessionDoc> FillListSave(CheckListViewVM model)
        {
            List<CaseSessionDoc> sessionDocs = new List<CaseSessionDoc>();

            foreach (var check in model.checkListVMs)
            {
                if (check.Checked)
                    sessionDocs.Add(FillData(model.ObjectId, int.Parse(check.Value)));
            }

            return sessionDocs;
        }

        /// <summary>
        /// Запис на лист от Съпровождащи документи, прикачени към заседание
        /// </summary>
        /// <param name="caseSessionDocs"></param>
        /// <returns></returns>
        public bool SaveCaseSessionDoc(List<CaseSessionDoc> caseSessionDocs)
        {
            try
            {
                foreach (var doc in caseSessionDocs)
                    repo.Add<CaseSessionDoc>(doc);

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logger.log(ex)
                return false;
            }
        }

        /// <summary>
        /// Запис на избрани Съпровождащи документи, прикачени към заседание от чеклист
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseSessionDoc_SaveDataAdd(CheckListViewVM model)
        {
            return SaveCaseSessionDoc(FillListSave(model));
        }

        /// <summary>
        /// Извличане на данни за Съпровождащи документи, прикачени към заседание по дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionDocVM> CaseSessionDocByCaseId_Select(int CaseId)
        {
            return repo.AllReadonly<CaseSessionDoc>()
                       .Include(x => x.CaseSession)
                       .Include(x => x.Document)
                       .ThenInclude(x => x.DocumentType)
                       .Include(x => x.SessionDocState)
                       .Where(x => x.CaseSession.CaseId == CaseId &&
                                   x.DateExpired == null)
                       .Select(x => new CaseSessionDocVM()
                       {
                           Id = x.Id,
                           CaseSessionId = x.CaseSessionId,
                           DocumentId = x.DocumentId,
                           DocumentLabel = (x.Document != null) ? "Вх.№ " + x.Document.DocumentNumber + "/" + x.Document.DocumentDate.ToString("dd.MM.yyyy") + " " + x.Document.DocumentType.Label : string.Empty,
                           SessionDocStateLabel = (x.SessionDocState != null) ? x.SessionDocState.Label : string.Empty,
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Извличане на данни за Съпровождащи документи, прикачени към заседание за комбо
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_CaseSessionDoc(int CaseSessionId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = CaseSessionDoc_Select(CaseSessionId)
                                  .Select(x => new SelectListItem()
                                  {
                                      Text = x.DocumentLabel,
                                      Value = x.DocumentId.ToString()
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
        /// Проверка за съпровождащ докумен в Съпровождащи документи, прикачени към заседание с различен статус от неразгледан по всички заседания
        /// </summary>
        /// <param name="DocumentId"></param>
        /// <returns></returns>
        public bool IsExistDocumentIdDifferentStatusNerazgledan(long DocumentId)
        {
            return repo.AllReadonly<CaseSessionDoc>()
                       .Any(x => x.DocumentId == DocumentId &&
                                 x.DateExpired == null &&
                                 x.SessionDocStateId != NomenclatureConstants.SessionDocState.Nerazgledan);
        }
    }
}
