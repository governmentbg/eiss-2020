using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class DocumentTemplateService : BaseService, IDocumentTemplateService
    {
        private readonly IUrlHelper urlHelper;
        private readonly IDocumentService documentService;
        private readonly ICounterService counterService;
        private readonly IMQEpepService mqService;

        public DocumentTemplateService(
            ILogger<DocumentTemplateService> _logger,
            IDocumentService _documentService,
            IRepository _repo,
            IUrlHelper _urlHelper,
            IUserContext _userContext,
            ICounterService _counterService,
            IMQEpepService _mqService)
        {
            this.logger = _logger;
            this.repo = _repo;
            documentService = _documentService;
            this.urlHelper = _urlHelper;
            this.userContext = _userContext;
            counterService = _counterService;
            mqService = _mqService;
        }

        public DocumentTemplate DocumentTemplate_Init(int sourceType, long sourceId)
        {
            var model = new DocumentTemplate()
            {
                SourceType = sourceType,
                SourceId = sourceId,
                AuthorId = userContext.UserId,
                DocumentTemplateStateId = DocumentConstants.TemplateStates.Draft
            };
            int? caseId = null;
            switch (model.SourceType)
            {
                case SourceTypeSelectVM.Case:
                    caseId = (int)model.SourceId;
                    break;
                case SourceTypeSelectVM.CaseSessionAct:
                    caseId = repo.AllReadonly<CaseSessionAct>()
                                        .Include(x => x.CaseSession)
                                        .Where(x => x.Id == (int)model.SourceId)
                                        .Select(x => x.CaseSession.CaseId)
                                        .FirstOrDefault();
                    break;
                case SourceTypeSelectVM.CaseNotification:
                    {
                        var info = repo.AllReadonly<CaseNotification>()
                                            .Include(x => x.HtmlTemplate)
                                            .Where(x => x.Id == (int)model.SourceId)
                                            .Select(x => new
                                            {
                                                CaseId = x.CaseId,
                                                DocAlias = (x.HtmlTemplate != null) ? x.HtmlTemplate.DocumentTypeAlias : ""
                                            })
                                            .FirstOrDefault();
                        caseId = info.CaseId;
                        if (!string.IsNullOrEmpty(info.DocAlias))
                        {
                            var docInfo = repo.AllReadonly<DocumentType>()
                                                  .Include(x => x.DocumentGroup)
                                                  .Where(x => x.Alias == info.DocAlias)
                                                  .Select(x => new
                                                  {
                                                      DocTypeId = x.Id,
                                                      DocGroupId = x.DocumentGroupId,
                                                      DocumentKind = x.DocumentGroup.DocumentKindId
                                                  }).FirstOrDefault();

                            if (docInfo != null)
                            {
                                model.DocumentKindId = docInfo.DocumentKind;
                                model.DocumentGroupId = docInfo.DocGroupId;
                                model.DocumentTypeId = docInfo.DocTypeId;
                            }


                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseMigration:
                    caseId = repo.AllReadonly<CaseMigration>()
                                        .Where(x => x.Id == (int)model.SourceId)
                                        .Select(x => x.CaseId)
                                        .FirstOrDefault();
                    break;
                case SourceTypeSelectVM.CaseSessionActDivorce:
                    caseId = repo.AllReadonly<CaseSessionActDivorce>()
                                        .Include(x => x.CaseSessionAct)
                                        .Include(x => x.CaseSessionAct.CaseSession)
                                        .Where(x => x.Id == (int)model.SourceId)
                                        .Select(x => x.CaseSessionAct.CaseSession.CaseId)
                                        .FirstOrDefault();
                    break;
                case SourceTypeSelectVM.ExecList:
                    caseId = repo.AllReadonly<ExecList>()
                                        .Where(x => x.Id == (int)model.SourceId)
                                        .Select(x => x.ExecListObligations.Select(a => a.Obligation.CaseSessionAct.CaseSession.CaseId).FirstOrDefault())
                                        .FirstOrDefault();
                    break;
                case SourceTypeSelectVM.CasePersonBulletin:
                    caseId = repo.AllReadonly<CasePersonSentenceBulletin>()
                                        .Where(x => x.Id == (int)model.SourceId)
                                        .Select(x => x.CasePerson.CaseId)
                                        .FirstOrDefault();
                    break;
                case SourceTypeSelectVM.CasePersonSentence:
                    caseId = repo.AllReadonly<CasePersonSentence>()
                                        .Where(x => x.Id == (int)model.SourceId)
                                        .Select(x => x.CaseId)
                                        .FirstOrDefault();
                    break;
            }

            model.CaseId = caseId;
            return model;
        }


        public IQueryable<DocumentTemplateVM> DocumentTemplate_Select(int sourceType, long sourceId)
        {
            return repo.AllReadonly<DocumentTemplate>()
                            .Include(x => x.DocumentType)
                            .Include(x => x.Author)
                            .ThenInclude(x => x.LawUnit)
                            .Include(x => x.DocumentTemplateState)
                            .Include(x => x.Document)
                            .ThenInclude(x => x.DocumentType)
                            .Where(x => x.SourceType == sourceType &&
                                        x.SourceId == sourceId &&
                                        x.Document.DateExpired == null)
                            .OrderByDescending(x => x.DateWrt)
                            .Select(x => new DocumentTemplateVM()
                            {
                                Id = x.Id,
                                AuthorName = x.Author.LawUnit.FullName,
                                DocumentTypeLabel = x.DocumentType.Label,
                                DateWrt = x.DateWrt,
                                StateName = x.DocumentTemplateState.Label,
                                DocumentId = x.DocumentId,
                                DocumentNumber = (x.Document != null) ? $"{x.Document.DocumentNumber}/{x.Document.DocumentDate:dd.MM.yyyy}" : "",
                                HtmlTemplateName = x.HtmlTemplateId == null ? "Общ формуляр" : x.HtmlTemplate.Label,
                            }).AsQueryable();
        }

        public bool DocumentTemplate_SaveData(DocumentTemplate model)
        {
            try
            {
                model.HtmlTemplateId = model.HtmlTemplateId.EmptyToNull();
                model.CasePersonId = model.CasePersonId.EmptyToNull();
                model.CasePersonAddressId = model.CasePersonAddressId.EmptyToNull();

                if (model.Id > 0)
                {
                    var saved = repo.GetById<DocumentTemplate>(model.Id);
                    saved.DocumentKindId = model.DocumentKindId;
                    saved.DocumentGroupId = model.DocumentGroupId;
                    saved.DocumentTypeId = model.DocumentTypeId;
                    saved.HtmlTemplateId = model.HtmlTemplateId;
                    saved.AuthorId = model.AuthorId;
                    saved.Description = model.Description;
                    saved.DocumentTemplateStateId = model.DocumentTemplateStateId;
                    saved.CasePersonId = model.CasePersonId;
                    saved.CasePersonAddressId = model.CasePersonAddressId;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {

                    model.CourtId = userContext.CourtId;
                    model.UserId = userContext.UserId;
                    model.DateWrt = DateTime.Now;
                    repo.Add(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при DocumentTemplate_SaveData Id={ model.Id }");
            }
            return false;
        }

        public IEnumerable<LabelValueVM> DocumentTemplate_LoadBreadCrumbs(DocumentTemplate model)
        {
            var result = new List<LabelValueVM>();
            switch (model.SourceType)
            {
                case SourceTypeSelectVM.CaseSessionAct:
                    var actModel = repo.AllReadonly<CaseSessionAct>()
                                    .Include(x => x.ActType)
                                    .FirstOrDefault(x => x.Id == model.SourceId);

                    var sessionModel = repo.AllReadonly<CaseSession>()
                                     .Include(x => x.SessionType)
                                     .Include(x => x.Case)
                                     .FirstOrDefault(x => x.Id == actModel.CaseSessionId);


                    result.Add(new LabelValueVM
                    {
                        Value = urlHelper.Action("CasePreview", "Case", new { id = sessionModel.CaseId }),
                        Label = $"Дело {sessionModel.Case.RegNumber}"
                    });
                    result.Add(new LabelValueVM
                    {
                        Value = urlHelper.Action("Preview", "CaseSession", new { id = actModel.Id }),
                        Label = $"{sessionModel.SessionType.Label} {sessionModel.DateFrom:dd.MM.yyyy}"
                    });
                    result.Add(new LabelValueVM
                    {
                        Value = urlHelper.Action("Edit", "CaseSessionAct", new { id = actModel.Id }),
                        Label = $"{actModel.ActType.Label} {actModel.RegNumber} / {actModel.RegDate:dd.MM.yyyy}"
                    });

                    break;
                default:
                    return null;

            };
            return result;
        }

        public DocumentTemplateHeaderVM DocumentTemplate_InitHeader(int id)
        {
            var model = repo.AllReadonly<DocumentTemplate>()
                                .Include(x => x.Court)
                                .Include(x => x.DocumentType)
                                .Include(x => x.Document)
                                .Include(x => x.Author)
                                .ThenInclude(x => x.LawUnit)
                                .Where(x => x.Id == id)
                                .Select(x => new DocumentTemplateHeaderVM()
                                {
                                    Id = x.Id,
                                    CaseId = x.CaseId,
                                    DocumentId = x.DocumentId,
                                    DocumentNumber = (x.Document != null) ? x.Document.DocumentNumber : "",
                                    DocumentDate = (x.Document != null) ? x.Document.DocumentDate : DateTime.Now,
                                    AuthorId = x.AuthorId,
                                    AuthorName = (x.Author != null) ? x.Author.LawUnit.FullName_MiddleNameInitials : "",
                                    CourtName = x.Court.Label,
                                    CourAddress = $"{x.Court.CityName}, {x.Court.Address}",
                                    DocumentTypeLabel = x.DocumentType.Label,
                                    HeaderTemplateName = x.DocumentType.DocumentTemplateName,
                                    HtmlTemplateTypeId = x.DocumentType.HtmlTemplateTypeId ?? 0,
                                    CourtLogo = x.Court.CourtLogo
                                })
                                .FirstOrDefault();

            if (model.CaseId > 0)
            {
                var judgesLU = repo.AllReadonly<CaseLawUnit>()
                                    .Include(x => x.LawUnit)
                                    .Where(x => x.CaseId == model.CaseId)
                                    .Where(x => x.CaseSessionId == null)
                                    .Where(x => x.DateFrom <= model.DocumentDate && (x.DateTo ?? DateTime.MaxValue) >= model.DocumentDate)
                                    .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                    .Select(x => x.LawUnit)
                                    .FirstOrDefault();
                if (judgesLU != null)
                {
                    model.JudgeName = judgesLU.FullName_MiddleNameInitials;
                }
            }
            if (model.DocumentId > 0)
            {
                var docModel = documentService.Document_GetById(model.DocumentId.Value).Result;
                var firstPerson = docModel.DocumentPersons.FirstOrDefault();
                model.DocumentReccipientName = firstPerson.FullName;
                model.DocumentReccipientAddress = firstPerson.Addresses.FirstOrDefault()?.Address.FullAddress;
            }

            return model;
        }

        public bool DocumentTemplate_UpdateDocumentId(int id, long documentId)
        {
            var saved = repo.GetById<DocumentTemplate>(id);
            if (saved != null)
            {
                saved.DocumentId = documentId;
                repo.Update(saved);

                switch (saved.SourceType)
                {
                    case SourceTypeSelectVM.CaseMigration:
                        {
                            var caseMigration = repo.GetById<CaseMigration>((int)saved.SourceId);
                            if (caseMigration.OutDocumentId == null)
                            {
                                caseMigration.OutDocumentId = documentId;
                                repo.Update(caseMigration);
                            }
                            if (caseMigration.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendNextLevel && caseMigration.CaseSessionActId > 0)
                            {
                                mqService.LegalActs_SendAct(caseMigration.CaseSessionActId.Value, EpepConstants.ServiceMethod.Update);
                            }
                        }
                        break;
                    case SourceTypeSelectVM.DocumentDecision:
                        {
                            var documentDecision = repo.GetById<DocumentDecision>(saved.SourceId);
                            if (documentDecision.OutDocumentId == null)
                            {
                                documentDecision.OutDocumentId = documentId;
                                repo.Update(documentDecision);
                            }
                        }
                        break;
                    case SourceTypeSelectVM.CaseSessionActDivorce:
                        {
                            var caseDivorce = repo.GetById<CaseSessionActDivorce>((int)saved.SourceId);
                            if (caseDivorce.OutDocumentId == null)
                            {
                                caseDivorce.OutDocumentId = documentId;
                                repo.Update(caseDivorce);
                            }
                        }
                        break;
                    case SourceTypeSelectVM.ExecList:
                        {
                            var execList = repo.GetById<ExecList>((int)saved.SourceId);
                            if (execList.OutDocumentId == null)
                            {
                                execList.OutDocumentId = documentId;
                                repo.Update(execList);
                            }
                        }
                        break;
                    case SourceTypeSelectVM.ExchangeDoc:
                        {
                            var exchangeDoc = repo.GetById<ExchangeDoc>((int)saved.SourceId);
                            if (exchangeDoc.OutDocumentId == null)
                            {
                                var document = repo.AllReadonly<Document>().Where(x => x.Id == documentId).FirstOrDefault();

                                if (counterService.Counter_GetExchangeCounter(exchangeDoc) == false)
                                {
                                    return false;
                                }
                                exchangeDoc.OutDocumentId = documentId;
                                repo.Update(exchangeDoc);
                            }
                        }
                        break;
                    case SourceTypeSelectVM.CasePersonBulletin:
                        {
                            var bulletin = repo.GetById<CasePersonSentenceBulletin>((int)saved.SourceId);
                            if (bulletin.OutDocumentId == null)
                            {
                                bulletin.OutDocumentId = documentId;
                                repo.Update(bulletin);
                            }
                        }
                        break;
                    case SourceTypeSelectVM.CasePersonSentence:
                        {
                            var sentence = repo.GetById<CasePersonSentence>((int)saved.SourceId);
                            if (sentence.OutDocumentId == null)
                            {
                                sentence.OutDocumentId = documentId;
                                repo.Update(sentence);
                            }
                        }
                        break;
                }
                repo.SaveChanges();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Извличат се данни за конкретен документ
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public DocumentTemplate DocumentTemplate_SelectByDocumentId(long documentId)
        {
            return repo.AllReadonly<DocumentTemplate>()
                                .Where(x => x.DocumentId == documentId)
                                .FirstOrDefault();
        }
    }
}
