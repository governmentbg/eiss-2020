// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class DocumentNotificationService : BaseService, IDocumentNotificationService
    {
        private readonly ICounterService counterService;
        private readonly IDeliveryItemService deliveryItemService;
        private readonly IDocumentPersonLinkService documentPersonLinkService;
        private readonly INomenclatureService nomenclatureService;
        private readonly ICdnService cdnService;

        public DocumentNotificationService(
            ILogger<DocumentNotificationService> _logger,
            AutoMapper.IMapper _mapper,
            ICounterService _counterService,
            IDeliveryItemService _deliveryItemService,
            IDocumentPersonLinkService _documentPersonLinkService,
            INomenclatureService _nomenclatureService,
            ICdnService _cdnService,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            mapper = _mapper;
            userContext = _userContext;
            counterService = _counterService;
            deliveryItemService = _deliveryItemService;
            nomenclatureService = _nomenclatureService;
            documentPersonLinkService = _documentPersonLinkService;
            cdnService = _cdnService;
        }
        public IQueryable<DocumentNotificationVM> DocumentNotification_Select(long documentId, long? documentResolutionId)
        {
            var result = repo.AllReadonly<DocumentNotification>()
               .Include(x => x.NotificationType)
               .Include(x => x.NotificationState)
               .Include(x => x.HtmlTemplate)
               .Where(x => x.DocumentId == documentId && 
                           (documentResolutionId <= 0 || x.DocumentResolutionId == documentResolutionId) &&
                           x.DateExpired == null)
               .Select(x => new DocumentNotificationVM()
               {
                   Id = x.Id,
                   DocumentId = x.DocumentId ?? 0,
                   DocumentResolutionId = x.DocumentResolutionId ?? 0,
                   NotificationTypeLabel = (x.NotificationType != null) ? x.NotificationType.Label : string.Empty,
                   NotificationTypeId = x.NotificationTypeId,
                   PersonName = x.DocumentPerson.FamilyName,
                   NotificationStateLabel = (x.NotificationState != null) ? x.NotificationState.Label : string.Empty,
                   HtmlTemplateLabel = (x.HtmlTemplate != null) ? x.HtmlTemplate.Label : string.Empty,
                   RegNumber = x.RegNumber,
                   RegDate = x.RegDate,
                   NotificationNumber = x.NotificationNumber,
               }).AsQueryable();
            return result;
        }

        private void CreateDeliveryItem(DocumentNotification notification, bool operIsChanged)
        {
            if (notification.NotificationStateId == NomenclatureConstants.NotificationState.Proekt)
                return;
            if (!NomenclatureConstants.NotificationDeliveryGroup.DeliveryGroupForDeliveryItem.Contains(notification.NotificationDeliveryGroupId ?? 0))
                return;
            DeliveryItem deliveryItem = null;
            if (notification.Id > 0)
                deliveryItem = deliveryItemService.GetDeliveryItemByDocumentNotificationId(notification.Id);
            deliveryItem = deliveryItem ?? new DeliveryItem();
            bool stateIsChanged = (deliveryItem.NotificationStateId != notification.NotificationStateId);
            deliveryItem.FromCourtId = notification.Document?.CourtId ?? userContext.CourtId;
            if (deliveryItem.DateSend == null && notification.NotificationStateId == NomenclatureConstants.DeliveryOper.Send)
                deliveryItem.DateSend = DateTime.Now;
            deliveryItem.DateAccepted = notification.DateAccepted;
            deliveryItem.DeliveryDate = notification.DeliveryDate;
            deliveryItem.ReturnDate = notification.ReturnDate;
            deliveryItem.RegNumber = notification.RegNumber ?? "";
            deliveryItem.RegDate = notification.RegDate;
            deliveryItem.DocumentNotificationId = notification.Id;
            deliveryItem.NotificationStateId = notification.NotificationStateId;
            deliveryItem.NotificationTypeId = notification.NotificationTypeId;
            deliveryItem.PersonName = notification.NotificationPersonName;
            deliveryItem.Address = null;
            deliveryItem.AddressId = notification.NotificationAddressId ?? 0;

            deliveryItem.CourtId = notification.ToCourtId ?? (notification.CourtId ?? 0);
            deliveryItem.DeliveryAreaId = notification.DeliveryAreaId; //deliveryAreaService.GetDeliveryAreaIdByLawUnitId(deliveryItem.CourtId, notification.LawUnitId);
            deliveryItem.LawUnitId = notification.LawUnitId;

            Document aDocument = notification.Document;
            if (aDocument == null)
            {
                aDocument = repo.AllReadonly<Document>()
                        .Where(x => x.Id == notification.DocumentId)
                        .Include(x => x.DocumentType)
                        .FirstOrDefault();
            }
            DocumentType aDocumentType = aDocument?.DocumentType;
            if (aDocument != null && aDocumentType == null)
                aDocumentType = repo.AllReadonly<DocumentType>()
                                .Where(x => x.Id == aDocument.DocumentTypeId)
                                .FirstOrDefault();
            if (aDocument != null)
            {
                if (aDocumentType != null)
                    deliveryItem.CaseInfo = $"{aDocumentType.Code} {aDocument.DocumentNumber} / {aDocument.DocumentDate.ToString(FormattingConstant.NormalDateFormat)}";
            }

            deliveryItem.HtmlTemplateId = notification.HtmlTemplateId;
            deliveryItem.PersonName = deliveryItem.PersonName ?? "";
            deliveryItem.DateWrt = DateTime.Now;
            deliveryItem.UserId = userContext.UserId;
            if (stateIsChanged || operIsChanged)
            {
                deliveryItemService.CreateDeliveryItemOper(deliveryItem, notification.DeliveryOperId ?? notification.NotificationStateId);
            }

            notification.DeliveryItems = notification.DeliveryItems ?? new HashSet<DeliveryItem>();
            notification.DeliveryItems.Add(deliveryItem);
            if (deliveryItem.Id > 0)
                repo.Update(deliveryItem);
            else
                repo.Add(deliveryItem);
        }
        public bool DocumentNotification_SaveData(DocumentNotification model, List<DocumentNotificationMLink> documentNotificationMLinks)
        {
            try
            {
                using (var scope = TransactionScopeBuilder.CreateReadCommitted())
                {

                    model.DocumentPersonId = model.DocumentPersonId <= 0? null : model.DocumentPersonId;
                    model.DocumentPersonLinkId = model.DocumentPersonLinkId.EmptyToNull();
                    model.NotificationAddressId = model.NotificationAddressId <= 0 ? null : model.NotificationAddressId;
                    model.ToCourtId = model.ToCourtId.EmptyToNull();
                    model.LawUnitId = model.LawUnitId.EmptyToNull();
                    model.DeliveryOperId = model.DeliveryOperId.EmptyToNull();
                    model.DeliveryAreaId = model.DeliveryAreaId.EmptyToNull();

                    if (model.DocumentPersonLinkId == -2)
                    {
                        model.DocumentPersonLinkId = null;
                        model.IsMultiLink = true;
                    }
                    else
                    {
                        model.IsMultiLink = false;
                    }

                    var documentPerson = repo.AllReadonly<DocumentPerson>()
                                         .Include(x => x.PersonRole)
                                         .Where(x => x.Id == model.DocumentPersonId)
                                         .FirstOrDefault();

                    model.NotificationPersonName = documentPerson.FullName;
                    model.NotificationPersonRole = documentPerson.PersonRole.Label;
                    model.NotificationLinkName = null;

                    var documentPersonAddress = repo.AllReadonly<DocumentPersonAddress>()
                                            .Include(x => x.Address)
                                            .Where(x => x.Id == model.DocumentPersonAddressId)
                                            .FirstOrDefault();


                    if (documentPersonAddress?.Address != null)
                    {
                        model.NotificationAddress = new Address();
                        model.NotificationAddress.CopyFrom(documentPersonAddress.Address);
                        if (model.Id < 1)
                            model.NotificationAddress.Id = 0;
                    }
                    if (model.NotificationAddress != null)
                        nomenclatureService.SetFullAddress(model.NotificationAddress);

                    var htmlTemplate = repo.AllReadonly<HtmlTemplate>()
                                           .FirstOrDefault(x => x.Id == model.HtmlTemplateId);
                    
                    if (model.Id > 0)
                    {
                        //Update
                        var saved = repo.All<DocumentNotification>()
                                        .Include(x => x.DocumentNotificationMLinks) 
                                        .Where(x => x.Id == model.Id)
                                        .FirstOrDefault();

                        if (saved.DocumentNotificationMLinks == null || saved.DocumentNotificationMLinks.Count == 0)
                        {
                            saved.DocumentNotificationMLinks = documentNotificationMLinks;
                        }
                        else
                        {
                            foreach (var toLink in saved.DocumentNotificationMLinks)
                            {
                                if (documentNotificationMLinks == null || !documentNotificationMLinks.Any(x => x.DocumentPersonLinkId == toLink.DocumentPersonLinkId))
                                {
                                    toLink.IsChecked = false;
                                    toLink.IsActive = false;
                                }
                            }
                            if (documentNotificationMLinks != null)
                            {
                                foreach (var fromLink in documentNotificationMLinks)
                                {
                                    var toLink = saved.DocumentNotificationMLinks.FirstOrDefault(x => x.DocumentPersonLinkId == fromLink.DocumentPersonLinkId);
                                    if (toLink == null)
                                    {
                                        saved.DocumentNotificationMLinks.Add(fromLink);
                                    }
                                    else
                                    {
                                        toLink.DocumentNotificationId = fromLink.DocumentNotificationId;
                                        toLink.DocumentResolutionId = fromLink.DocumentResolutionId;
                                        toLink.DocumentPersonLinkId = fromLink.DocumentPersonLinkId;
                                        toLink.DocumentPersonSummonedId = fromLink.DocumentPersonSummonedId;
                                        toLink.DocumentPersonId = fromLink.DocumentPersonId;
                                        toLink.PersonSummonedName = fromLink.PersonSummonedName;
                                        toLink.PersonSummonedRole = fromLink.PersonSummonedRole;

                                        toLink.IsChecked = fromLink.IsChecked;
                                        toLink.IsActive = true;
                                    }
                                }

                            }
                        }
                        bool operIsChanged = (saved.DeliveryOperId != model.DeliveryOperId);
                        saved.DocumentId = model.DocumentId;
                        saved.DocumentResolutionId = model.DocumentResolutionId;
                        saved.DocumentPersonId = model.DocumentPersonId;
                        saved.DocumentPersonLinkId = model.DocumentPersonLinkId;
                        saved.DocumentPersonAddressId = model.DocumentPersonAddressId;
                        saved.NotificationTypeId = model.NotificationTypeId;
                        saved.NotificationNumber = model.NotificationNumber;
                        saved.NotificationPersonName = model.NotificationPersonName;
                        saved.NotificationPersonRole = model.NotificationPersonRole;
                        saved.NotificationAddress = model.NotificationAddress;
                        saved.NotificationAddressId = model.NotificationAddressId;
                        saved.Description = model.Description;
                        saved.NotificationStateId = model.NotificationStateId;
                        saved.NotificationDeliveryGroupId = model.NotificationDeliveryGroupId;
                        if (NomenclatureConstants.NotificationDeliveryGroup.OnMoment(saved.NotificationDeliveryGroupId) ||
                            saved.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.WithCityHall ||
                            saved.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.WithCourier)
                        {
                            saved.DeliveryDate = model.DeliveryDate;
                            saved.DeliveryInfo = model.DeliveryInfo;
                        }
                        saved.HaveАppendix = model.HaveАppendix;
                        saved.IsOfficialNotification = model.IsOfficialNotification;
                        saved.HtmlTemplateId = model.HtmlTemplateId;
                        saved.DeliveryAreaId = model.DeliveryAreaId;
                        saved.LawUnitId = model.LawUnitId;
                        saved.ToCourtId = model.ToCourtId;
                        saved.IsFromEmail = model.IsFromEmail;
                        if (model.NotificationStateId == NomenclatureConstants.NotificationState.UnDeliveredMail)
                        {
                            CreateDeliveryItem(saved, operIsChanged);
                            saved.NotificationDeliveryGroupId = NomenclatureConstants.NotificationDeliveryGroup.WithSummons;
                            saved.NotificationStateId = NomenclatureConstants.NotificationState.Ready;
                            saved.DateSend = null;
                            saved.IsFromEmail = true;
                        }

                        if (model.DatePrint != null)
                            saved.DatePrint = model.DatePrint;
                        saved.DateWrt = DateTime.Now;
                        saved.UserId = userContext.UserId;
                        // CreateHistory<CaseNotification, CaseNotificationH>(saved);
                        repo.Update(saved);
                        repo.SaveChanges();
                        CreateDeliveryItem(saved, operIsChanged);
                    }
                    else
                    {
                        model.DocumentNotificationMLinks = documentNotificationMLinks;
                        if (counterService.Counter_GetNotificationCounter(model, userContext.CourtId))
                        {
                            if (NomenclatureConstants.NotificationDeliveryGroup.OnMoment(model.NotificationDeliveryGroupId))
                            {
                                if (model.DeliveryDate == null)
                                    model.DeliveryDate = DateTime.Now;
                            }
                            if (model.NotificationStateId == NomenclatureConstants.NotificationState.UnDeliveredMail)
                            {
                                CreateDeliveryItem(model, true);
                                model.NotificationDeliveryGroupId = NomenclatureConstants.NotificationDeliveryGroup.WithSummons;
                                model.NotificationStateId = NomenclatureConstants.NotificationState.Ready;
                                model.DateSend = null;
                                model.IsFromEmail = true;
                            }

                            model.DateWrt = DateTime.Now;
                            model.UserId = userContext.UserId;
                            // CreateHistory<CaseNotification, CaseNotificationH>(model);
                            repo.Add(model);
                            repo.SaveChanges();
                            CreateDeliveryItem(model, true);
                        }
                    }

                    repo.SaveChanges();
                    scope.Complete();
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на уведомление към документ Id={ model.Id }");
            }
            return false;
        }
        public DocumentNotification ReadById(int? id)
        {
            var result = repo.AllReadonly<DocumentNotification>()
                             .Include(x => x.DocumentNotificationMLinks)
                             .Where(x => x.Id == id)
                             .FirstOrDefault();
            if (result != null)
            {
                if (result.IsMultiLink == true)
                    result.DocumentPersonLinkId = -2;
                result.DeliveryDateCC = result.DeliveryDate;
                result.DeliveryInfoCC = result.DeliveryInfo;
            }
            return result;
        }
        public List<SelectListItem> NotificationDeliveryGroupDDL(int notificationTypeId)
        {
            DateTime today = DateTime.Today;
            var deliveryGroup = repo.AllReadonly<DeliveryTypeGroup>()
                                    .Where(x => x.NotificationTypeId == notificationTypeId &&
                                                x.NotificationDeliveryGroup.IsActive &&
                                                x.NotificationDeliveryGroup.DateStart <= today &&
                                               (x.NotificationDeliveryGroup.DateEnd ?? today) >= today)
                                    .Select(x => x.NotificationDeliveryGroup);

            //if (aCase?.CaseGroupId != NomenclatureConstants.CaseGroups.GrajdanskoDelo &&
            //    aCase?.CaseGroupId != NomenclatureConstants.CaseGroups.Trade)
            //{
            //    deliveryGroup = deliveryGroup.Where(x => x.Id != NomenclatureConstants.NotificationDeliveryGroup.OnMember50);
            //}
            deliveryGroup = deliveryGroup.Where(x => x.Id != NomenclatureConstants.NotificationDeliveryGroup.OnMember50 &&
                                                     x.Id != NomenclatureConstants.NotificationDeliveryGroup.OnSession &&
                                                     x.Id != NomenclatureConstants.NotificationDeliveryGroup.OnMember56 &&
                                                     x.Id != NomenclatureConstants.NotificationDeliveryGroup.ByEPEP &&
                                                     x.Id != NomenclatureConstants.NotificationDeliveryGroup.WillBeen);
            deliveryGroup = deliveryGroup.OrderBy(x => x.OrderNumber);

            var result = deliveryGroup.Select(x => new SelectListItem()
            {
                Text = x.Label,
                Value = x.Id.ToString()
            }).ToList() ?? new List<SelectListItem>();

            result = result
                .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                .ToList();
            return result;
        }
        public async Task<CdnDownloadResult> ReadPrintedFile(int Id)
        {
            CdnItemVM aFile = cdnService.Select(SourceTypeSelectVM.DocumentNotificationPrint, Id.ToString()).Where(x => x.FileName.EndsWith(".pdf")).FirstOrDefault();
            if (aFile != null)
                return await cdnService.MongoCdn_Download(aFile).ConfigureAwait(false);
            return null;
        }
        public async Task<CdnDownloadResult> ReadDraftFile(int Id)
        {
            CdnItemVM aFile = cdnService.Select(SourceTypeSelectVM.DocumentNotificationPrint, Id.ToString()).Where(x => x.FileName == "draft.html").FirstOrDefault();
            if (aFile != null)
                return await cdnService.MongoCdn_Download(aFile).ConfigureAwait(false);
            return null;
        }
        public async Task<bool> SavePrintedFile(int Id, byte[] pdfBytes)
        {
            var notification = repo.AllReadonly<DocumentNotification>().Where(x => x.Id == Id).FirstOrDefault();

            var printRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.DocumentNotificationPrint,
                SourceId = Id.ToString(),
                FileName = notification.RegNumber + ".pdf",
                ContentType = "application/pdf",
                Title = notification.RegNumber,
                FileContentBase64 = Convert.ToBase64String(pdfBytes)
            };
            var result = await cdnService.MongoCdn_AppendUpdate(printRequest).ConfigureAwait(false);
            return result;
        }
        public bool SaveExpireInfoPlus(ExpiredInfoVM model)
        {
            var saved = repo.GetById<DocumentNotification>(model.Id);
            if (saved != null)
            {
                saved.DateExpired = DateTime.Now;
                saved.UserExpiredId = userContext.UserId;
                saved.DescriptionExpired = model.DescriptionExpired;
                var deliveryItem = repo.All<DeliveryItem>()
                                       .Where(x => x.DocumentNotificationId == model.Id)
                                       .FirstOrDefault();
                if (deliveryItem != null)
                {
                    deliveryItem.DateExpired = DateTime.Now;
                    deliveryItem.UserExpiredId = userContext.UserId;
                    deliveryItem.DescriptionExpired = model.DescriptionExpired;
                    repo.Update(deliveryItem);
                }
                repo.Update(saved);
                repo.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }

        }

        public List<DocumentNotificationMLink> DocumentPersonLinks(DocumentNotification documentNotification, int notificationTypeId)
        {
            if (documentNotification.DocumentNotificationMLinks == null)
                documentNotification.DocumentNotificationMLinks = repo.AllReadonly<DocumentNotificationMLink>()
                                                              .Where(x => x.DocumentNotificationId == documentNotification.Id)
                                                              .ToList();
            var links = documentNotification.DocumentNotificationMLinks ?? new List<DocumentNotificationMLink>();
            List<int> oldLinks = links.Select(x => x.DocumentPersonLinkId ?? 0).ToList();
            oldLinks.Add(documentNotification.DocumentPersonLinkId ?? 0);
            var linksVm = documentPersonLinkService.GetPresentByList(documentNotification.DocumentPersonId ?? 0, notificationTypeId, oldLinks);

            foreach (var link in links)
                if (!linksVm.Any(x => x.Id == link.DocumentPersonLinkId))
                    link.IsActive = false;
            foreach (var linkVM in linksVm)
            {
                var link = links.FirstOrDefault(x => x.DocumentPersonLinkId == linkVM.Id);

                if (link == null)
                {
                    link = new DocumentNotificationMLink()
                    {
                        CourtId = documentNotification.CourtId,
                        DocumentId = documentNotification.DocumentId,
                        DocumentNotificationId = documentNotification.Id,
                        DocumentPersonLinkId = linkVM.Id,
                        DocumentPersonSummonedId = linkVM.isXFirst ? linkVM.PersonId : linkVM.PersonRelId,
                        DocumentPersonId = documentNotification.DocumentPersonId,
                        PersonSummonedName = linkVM.isXFirst ? linkVM.PersonName : linkVM.PersonRelName,
                        PersonSummonedRole = linkVM.isXFirst ? linkVM.PersonRole : linkVM.PersonRelRole,
                        LinkLabel = linkVM.Label,
                        IsActive = true,
                        IsChecked = true
                    };
                    links.Add(link);
                }
                else
                {
                    link.DocumentPersonSummonedId = linkVM.isXFirst ? linkVM.PersonId : linkVM.PersonRelId;
                    link.DocumentPersonId = documentNotification.DocumentPersonId;
                    link.PersonSummonedName = linkVM.isXFirst ? linkVM.PersonName : linkVM.PersonRelName;
                    link.PersonSummonedRole = linkVM.isXFirst ? linkVM.PersonRole : linkVM.PersonRelRole;
                    link.LinkLabel = linkVM.Label;
                    link.IsActive = true;
                }
                if (linkVM.PersonSecondRelId == documentNotification.DocumentPersonId)
                {
                    // TODO:
                    // link.PersonSummonedName = linkVM.LabelWithoutSecondRel;
                    link.PersonSummonedRole = string.Empty;
                }
            }
            links = links.Where(x => x.DocumentPersonSummonedId > 0).ToList();
            foreach (var link in links)
                link.DocumentNotification = null;
            return links.ToList();
        }
        public string DocumentPersonLinksJson(DocumentNotification documentNotification, int notificationTypeId)
        {
            var links = DocumentPersonLinks(documentNotification, notificationTypeId).Where(x => x.IsActive).ToList();
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            return JsonConvert.SerializeObject(links.ToList(), serializerSettings);
        }
        public List<DocumentNotificationMLink> DocumentPersonLinksByNotificationId(int documentNotificationId, int documentPersonId, int notificationTypeId)
        {
            var documentNotification = repo.AllReadonly<DocumentNotification>()
                                       .Include(x => x.DocumentNotificationMLinks)
                                       .Where(x => x.Id == documentNotificationId)
                                       .FirstOrDefault() ?? new DocumentNotification();

            documentNotification.DocumentPersonId = documentPersonId;
            return DocumentPersonLinks(documentNotification, notificationTypeId);
        }
    }
}
