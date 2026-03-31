// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class MediationNotificationService : BaseService, IMediationNotificationService
    {
        private readonly ICounterService counterService;
        private readonly IDeliveryItemService deliveryItemService;
        private readonly ICasePersonLinkService casePersonLinkService;
        private readonly ICasePersonService casePersonService;
        private readonly INomenclatureService nomenclatureService;
        private readonly ICdnService cdnService;
        private readonly IDeliveryAreaAddressService deliveryAreaAddressService;

        public MediationNotificationService(
            ILogger<MediationNotificationService> _logger,
            ICounterService _counterService,
            IDeliveryItemService _deliveryItemService,
            ICasePersonLinkService _casePersonLinkService,
            INomenclatureService _nomenclatureService,
            IDeliveryAreaAddressService _deliveryAreaAddressService,
            ICasePersonService _casePersonService,
            ICdnService _cdnService,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            counterService = _counterService;
            deliveryItemService = _deliveryItemService;
            nomenclatureService = _nomenclatureService;
            deliveryAreaAddressService = _deliveryAreaAddressService;
            casePersonLinkService = _casePersonLinkService;
            casePersonService = _casePersonService;
            cdnService = _cdnService;
        }
        private Expression<Func<MediationNotification, bool>> IsNotExpired()
        {
            return x => x.DateExpired == null;
        }

        private async Task<DeliveryItem> CreateDeliveryItem(MediationNotification notification, bool operIsChanged)
        {
            if (notification.NotificationStateId == NomenclatureConstants.NotificationState.Proekt)
                return null;
            if (!NomenclatureConstants.NotificationDeliveryGroup.DeliveryGroupForDeliveryItem.Contains(notification.NotificationDeliveryGroupId ?? 0))
                return null;
            DeliveryItem deliveryItem = null;
            if (notification.Id > 0)
                deliveryItem = deliveryItemService.GetDeliveryItemByMediationNotificationId(notification.Id);
            deliveryItem = deliveryItem ?? new DeliveryItem();
            bool stateIsChanged = (deliveryItem.NotificationStateId != notification.NotificationStateId);
            deliveryItem.FromCourtId = notification.MediationCaseSession?.CourtId ?? userContext.CourtId;
            deliveryItem.ReturnDate = notification.ReturnDate;
            deliveryItem.RegNumber = notification.RegNumber ?? "";
            deliveryItem.RegDate = notification.RegDate;
            deliveryItem.MediationNotificationId = notification.Id;
            deliveryItem.NotificationStateId = notification.NotificationStateId;
            deliveryItem.NotificationTypeId = notification.NotificationTypeId;
            deliveryItem.PersonName = notification.NotificationPersonName;
            deliveryItem.Address = null;
            deliveryItem.AddressId = notification.NotificationAddressId ?? 0;

            deliveryItem.CourtId = notification.ToCourtId ?? (notification.CourtId ?? 0);
            deliveryItem.DeliveryAreaId = notification.DeliveryAreaId; //deliveryAreaService.GetDeliveryAreaIdByLawUnitId(deliveryItem.CourtId, notification.LawUnitId);
            deliveryItem.LawUnitId = notification.LawUnitId;
            deliveryItem.NotificationDeliveryGroupId = notification.NotificationDeliveryGroupId;
            deliveryItem.CaseId = notification.CaseId;
            Case aCase = notification.Case;
            if (aCase == null)
            {
                aCase = repo.AllReadonly<Case>()
                        .Where(x => x.Id == notification.CaseId)
                        .Include(x => x.CaseType)
                        .FirstOrDefault();
            }
            deliveryItem.CaseInfo = $"Медиация {aCase.CaseType.Code} {aCase.RegNumber} / {aCase.RegDate.ToString(FormattingConstant.NormalDateFormat)}";


            deliveryItem.HtmlTemplateId = notification.HtmlTemplateId;
            deliveryItem.PersonName = deliveryItem.PersonName ?? "";
            deliveryItem.DateWrt = DateTime.Now;
            deliveryItem.UserId = userContext.UserId;
            if (stateIsChanged || operIsChanged)
            {
                var oper = deliveryItemService.CreateDeliveryItemOper(deliveryItem, notification.DeliveryOperId ?? notification.NotificationStateId);
                await deliveryItemService.SetDeliveryItemDates(deliveryItem, oper);
            }

            notification.DeliveryItems = notification.DeliveryItems ?? new HashSet<DeliveryItem>();
            notification.DeliveryItems.Add(deliveryItem);
            if (deliveryItem.Id > 0)
                repo.Update(deliveryItem);
            else
                repo.Add(deliveryItem);
            return deliveryItem;
        }

        public async Task<bool> MediationNotification_SaveData(MediationNotification model, List<MediationNotificationMLink> mediationNotificationMLinks, DeliveryLogVM logVM)
        {
            try
            {
                using (var ts = repo.BeginTransaction())
                {
                    await MediationNotification_SaveData_NoTransaction(model, mediationNotificationMLinks, logVM);
                    ts.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на уведомление към документ Id={model.Id}");
            }
            return false;

        }
        public async Task MediationNotification_SaveData_NoTransaction(MediationNotification model, List<MediationNotificationMLink> mediationNotificationMLinks, DeliveryLogVM logVM)
        {
            model.CasePersonLinkId = model.CasePersonLinkId.EmptyToNull();
            model.NotificationAddressId = model.NotificationAddressId <= 0 ? null : model.NotificationAddressId;
            model.ToCourtId = model.ToCourtId.EmptyToNull();
            model.LawUnitId = model.LawUnitId.EmptyToNull();
            model.DeliveryOperId = model.DeliveryOperId.EmptyToNull();
            model.DeliveryAreaId = model.DeliveryAreaId.EmptyToNull();

            if (model.CasePersonLinkId == -2)
            {
                model.CasePersonLinkId = null;
                model.IsMultiLink = true;
            }
            else
            {
                model.IsMultiLink = false;
            }

            var casePerson = repo.AllReadonly<CasePerson>()
                                 .Include(x => x.PersonRole)
                                 .Where(x => x.Id == model.CasePersonId)
                                 .FirstOrDefault();

            model.NotificationPersonName = casePerson.FullName;
            model.NotificationPersonRole = casePerson.PersonRole.Label;
            model.NotificationLinkName = null;

            var casePersonAddress = repo.AllReadonly<CasePersonAddress>()
                                    .Include(x => x.Address)
                                    .Where(x => x.Id == model.CasePersonAddressId)
                                    .FirstOrDefault();


            if (casePersonAddress?.Address != null)
            {
                model.NotificationAddress = new Address();
                model.NotificationAddress.CopyFrom(casePersonAddress.Address);
                if (model.Id < 1)
                    model.NotificationAddress.Id = 0;
            }
            if (model.NotificationAddress != null)
                nomenclatureService.SetFullAddress(model.NotificationAddress);

            var htmlTemplate = repo.AllReadonly<HtmlTemplate>()
                                   .FirstOrDefault(x => x.Id == model.HtmlTemplateId); model.CasePersonL1Id = model.CasePersonId;
            model.CasePersonL2Id = null;
            model.CasePersonL3Id = null;
            model.LinkDirectionId = null;
            model.LinkDirectionSecondId = null;

            CaseNotificationLinkVM casePersonLink = null;
            if (model.IsMultiLink != true && model.CasePersonLinkId > 0)
            {
                var oldLinks = new List<int>() { model.CasePersonLinkId ?? 0 };
                var casePersonLinks = casePersonLinkService.GetLinkForPersonMediation(model.CasePersonId, model.MediationCaseSessionId);
                 casePersonLink = casePersonLinks.Where(x => x.Id == model.CasePersonLinkId).FirstOrDefault();
                if (casePersonLink != null)
                {
                    model.CasePersonL1Id = casePersonLink.PersonId;
                    model.CasePersonL2Id = casePersonLink.PersonRelId;
                    if (!casePersonLink.isXFirst)
                    {
                        model.CasePersonL1Id = casePersonLink.PersonRelId;
                        model.CasePersonL2Id = casePersonLink.PersonId;
                    }
                    model.LinkDirectionId = casePersonLink.LinkDirectionId;
                    model.LinkDirectionSecondId = casePersonLink.LinkDirectionSecondId.EmptyToNull(0);
                    model.CasePersonL3Id = casePersonLink.PersonSecondRelId.EmptyToNull(0);
                }
            }

            if (model.Id > 0)
            {
                //Update
                var saved = repo.All<MediationNotification>()
                                .Include(x => x.MediationNotificationMLinks)
                                .Where(x => x.Id == model.Id)
                                .FirstOrDefault();

                if (saved.MediationNotificationMLinks == null || saved.MediationNotificationMLinks.Count == 0)
                {
                    saved.MediationNotificationMLinks = mediationNotificationMLinks;
                }
                else
                {
                    foreach (var toLink in saved.MediationNotificationMLinks)
                    {
                        if (mediationNotificationMLinks == null || !mediationNotificationMLinks.Any(x => x.CasePersonLinkId == toLink.CasePersonLinkId))
                        {
                            toLink.IsChecked = false;
                            toLink.IsActive = false;
                        }
                    }
                    if (mediationNotificationMLinks != null)
                    {
                        foreach (var fromLink in mediationNotificationMLinks)
                        {
                            var toLink = mediationNotificationMLinks.FirstOrDefault(x => x.CasePersonLinkId == fromLink.CasePersonLinkId);
                            if (toLink == null)
                            {
                                saved.MediationNotificationMLinks.Add(fromLink);
                            }
                            else
                            {
                                toLink.MediationNotificationId = fromLink.MediationNotificationId;
                                toLink.CasePersonLinkId = fromLink.CasePersonLinkId;
                                toLink.CasePersonSummonedId = fromLink.CasePersonSummonedId;
                                toLink.CasePersonId = fromLink.CasePersonId;
                                toLink.PersonSummonedName = fromLink.PersonSummonedName;
                                toLink.PersonSummonedRole = fromLink.PersonSummonedRole;

                                toLink.IsChecked = fromLink.IsChecked;
                                toLink.IsActive = true;
                            }
                        }

                    }
                }
                bool operIsChanged = (saved.DeliveryOperId != model.DeliveryOperId);
                saved.CaseId = model.CaseId;
                saved.MediationCaseSessionId = model.MediationCaseSessionId;
                saved.CasePersonId = model.CasePersonId;
                saved.CasePersonL1Id = model.CasePersonL1Id;
                saved.CasePersonL2Id = model.CasePersonL2Id;
                saved.CasePersonL3Id = model.CasePersonL3Id;
                saved.CasePersonLinkId = model.CasePersonLinkId;
                saved.LinkDirectionId = model.LinkDirectionId;
                saved.LinkDirectionSecondId = model.LinkDirectionSecondId;
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
                    NomenclatureConstants.NotificationDeliveryGroup.WithCourierLike(saved.NotificationDeliveryGroupId))
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
                    await CreateDeliveryItem(saved, operIsChanged);
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
                var deliveryItem = await CreateDeliveryItem(saved, operIsChanged);
                repo.SaveChanges();
                deliveryItemService.CreateDeliveryItemOperLog(deliveryItem, null, logVM);
                repo.SaveChanges();
            }
            else
            {
                model.MediationNotificationMLinks = mediationNotificationMLinks;
                if (counterService.Counter_GetNotificationCounter(model, userContext.CourtId))
                {
                    if (NomenclatureConstants.NotificationDeliveryGroup.OnMoment(model.NotificationDeliveryGroupId))
                    {
                        if (model.DeliveryDate == null)
                            model.DeliveryDate = DateTime.Now;
                    }
                    if (model.NotificationStateId == NomenclatureConstants.NotificationState.UnDeliveredMail)
                    {
                        await CreateDeliveryItem(model, true);
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
                    var deliveryItem = await CreateDeliveryItem(model, true);
                    repo.SaveChanges();
                    deliveryItemService.CreateDeliveryItemOperLog(deliveryItem, null, logVM);
                    repo.SaveChanges();

                }
            }
            repo.SaveChanges();
        }
        public MediationNotification ReadById(int? id)
        {
            var result = repo.AllReadonly<MediationNotification>()
                             .Include(x => x.MediationNotificationMLinks)
                             .Where(x => x.Id == id)
                             .FirstOrDefault();
            if (result != null)
            {
                if (result.IsMultiLink == true)
                    result.CasePersonLinkId = -2;
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
            CdnItemVM aFile = cdnService.Select(SourceTypeSelectVM.MediationNotificationPrint, Id.ToString()).Where(x => x.FileName.EndsWith(".pdf")).FirstOrDefault();
            if (aFile != null)
                return await cdnService.MongoCdn_Download(aFile).ConfigureAwait(false);
            return null;
        }
        public async Task<CdnDownloadResult> ReadDraftFile(int Id)
        {
            CdnItemVM aFile = cdnService.Select(SourceTypeSelectVM.MediationNotificationPrint, Id.ToString()).Where(x => x.FileName == "draft.html").FirstOrDefault();
            if (aFile != null)
                return await cdnService.MongoCdn_Download(aFile).ConfigureAwait(false);
            return null;
        }
        public async Task<bool> SavePrintedFile(int Id, byte[] pdfBytes)
        {
            var notification = repo.AllReadonly<MediationNotification>().Where(x => x.Id == Id).FirstOrDefault();

            var printRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.MediationNotificationPrint,
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
            var saved = repo.GetById<MediationNotification>(model.Id);
            if (saved != null)
            {
                saved.DateExpired = DateTime.Now;
                saved.UserExpiredId = userContext.UserId;
                saved.DescriptionExpired = model.DescriptionExpired;
                var deliveryItem = repo.All<DeliveryItem>()
                                       .Where(x => x.MediationNotificationId == model.Id)
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

        public List<MediationNotificationMLink> MediationPersonLinks(MediationNotification mediationNotification)
        {
            if (mediationNotification.MediationNotificationMLinks == null)
                mediationNotification.MediationNotificationMLinks = repo.AllReadonly<MediationNotificationMLink>()
                                                              .Where(x => x.MediationNotificationId == mediationNotification.Id)
                                                              .ToList();
            var links = mediationNotification.MediationNotificationMLinks ?? new List<MediationNotificationMLink>();
            List<int> oldLinks = links.Select(x => x.CasePersonLinkId ?? 0).ToList();
            oldLinks.Add(mediationNotification.CasePersonLinkId ?? 0);
            // TODO: Да видя филтер он нотифицатион
            var linksVm = casePersonLinkService.GetLinkForPersonMediation(mediationNotification.CasePersonId, mediationNotification.MediationCaseSessionId);

            foreach (var link in links)
                if (!linksVm.Any(x => x.Id == link.CasePersonLinkId))
                    link.IsActive = false;
            foreach (var linkVM in linksVm)
            {
                var link = links.FirstOrDefault(x => x.CasePersonLinkId == linkVM.Id);

                if (link == null)
                {
                    link = new MediationNotificationMLink()
                    {
                        CourtId = mediationNotification.CourtId,
                        CaseId = mediationNotification.CaseId,
                        MediationNotificationId = mediationNotification.Id,
                        CasePersonLinkId = linkVM.Id,
                        CasePersonSummonedId = linkVM.isXFirst ? linkVM.PersonId : linkVM.PersonRelId,
                        CasePersonId = mediationNotification.CasePersonId,
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
                    link.CasePersonSummonedId = linkVM.isXFirst ? linkVM.PersonId : linkVM.PersonRelId;
                    link.CasePersonId = mediationNotification.CasePersonId;
                    link.PersonSummonedName = linkVM.isXFirst ? linkVM.PersonName : linkVM.PersonRelName;
                    link.PersonSummonedRole = linkVM.isXFirst ? linkVM.PersonRole : linkVM.PersonRelRole;
                    link.LinkLabel = linkVM.Label;
                    link.IsActive = true;
                }
                if (linkVM.PersonSecondRelId == mediationNotification.CasePersonId)
                {
                    link.PersonSummonedName = linkVM.LabelWithoutSecondRel;
                    link.PersonSummonedRole = string.Empty;
                }
            }
            links = links.Where(x => x.CasePersonSummonedId > 0).ToList();
            foreach (var link in links)
                link.MediationNotification = null;
            return links.ToList();
        }
        public string MediationPersonLinksJson(MediationNotification mediationNotification)
        {
            var links = MediationPersonLinks(mediationNotification).Where(x => x.IsActive).ToList();
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            return JsonConvert.SerializeObject(links.ToList(), serializerSettings);
        }
        public List<MediationNotificationMLink> MediationPersonLinksByNotificationId(int mediationNotificationId, int casePersonId, int notificationTypeId)
        {
            var mediationNotification = repo.AllReadonly<MediationNotification>()
                                       .Include(x => x.MediationNotificationMLinks)
                                       .Where(x => x.Id == mediationNotificationId)
                                       .FirstOrDefault() ?? new MediationNotification();

            mediationNotification.CasePersonId = casePersonId;
            return MediationPersonLinks(mediationNotification);
        }
        public List<SelectListItem> GetAddrForPerson(List<CaseNotificationLinkVM> linkListVM, int casePersonId, int casePersonLinkId, int notificationDeliveryGroupId)
        {
            List<SelectListItem> addrList;
            if (casePersonLinkId > 0 && linkListVM.Any(x => x.Id == casePersonLinkId))
            {
                int casePersonAddrId = casePersonId;
                var casePersonLink = linkListVM.FirstOrDefault(x => x.Id == casePersonLinkId);
                if (casePersonLink != null)
                {
                    casePersonAddrId = (casePersonLink.PersonSecondRelId ?? 0) != 0 ? (casePersonLink.PersonSecondRelId ?? 0) :
                                           (casePersonLink.isXFirst ? casePersonLink.PersonRelId : casePersonLink.PersonId);
                }
                addrList = casePersonService.GetDDL_CasePersonAddress(casePersonAddrId, notificationDeliveryGroupId);
            }
            else
            {
                addrList = casePersonService.GetDDL_CasePersonAddress(casePersonId, notificationDeliveryGroupId);
            }
            return addrList;
        }


        public List<SelectListItem> GetDDL_PersonList(int caseId, int mediationSessionId, int? notificationTypeId,  bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = repo.AllReadonly<MediationCasePerson>()
                                 .Where(x => x.CaseId == caseId &&
                                             x.MediationCaseSessionId == mediationSessionId)
                                 .Select(x => new SelectListItem()
                                 {
                                    Text = x.Person.FullName,                           
                                    Value = x.Person.Id.ToString()
                                 }).ToList() ?? new List<SelectListItem>();

            if (result.Count > 0)
            {
                result = result.OrderBy(x => x.Text).ToList();
            }

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }
        public async Task<(List<MediationNotificationListVM>, int)> MediationNotification_Select(int mediationSessionId, int start, int length, List<DataTablesSortColumnVM> sortedColumns, string search)
        {
            List<MediationNotificationListVM> result = new();
            int caseId = await repo.AllReadonly<MediationCaseSession>()
                             .Where(x => x.Id == mediationSessionId)
                             .Select(x => x.CaseId)
                             .FirstOrDefaultAsync();
            var query = repo.AllReadonly<MediationCasePerson>()
                             .Where(x => x.MediationCaseSessionId == mediationSessionId &&
                                         x.DateExpired == null)
                             .Select(x => new MediationNotificationListVM()
                             {
                                 Id = x.Id,
                                 MediationSessionId = x.MediationCaseSessionId,
                                 PersonName = x.Person.FullName,
                                 PersonRole = x.Person.PersonRole.Label,
                                 PersonId = x.CasePersonId,
                                 RoleKindId = x.Person.PersonRole.RoleKindId,
                                 DateTo = x.Person.DateTo,
                                 DateExpired = x.Person.DateExpired,
                                 IsDeleted = x.Person.DateExpired != null,
                                 IsDeceased = x.Person.IsDeceased,
                             });
            if (!string.IsNullOrEmpty(search))
            {
                search = $"%{search}%";
                query = query.Where(x => EF.Functions.ILike(x.PersonRole, search) || EF.Functions.ILike(x.PersonName, search));
            }
            var totalCount = 0;
            if (sortedColumns != null)
            {
                query = query.OrderBy(sortedColumns);
            }
            else
            {
                query = query.OrderBy(x => x.RowNumber);
            }

            var mediationNotificationListVMs = query.ToList();
            var mediationNotifications = await repo.AllReadonly<MediationNotification>()
                                         .Include(x => x.NotificationType)
                                         .Include(x => x.NotificationState)
                                         .Include(x => x.NotificationAddress)
                                         .Include(x => x.MediationNotificationMLinks)
                                         .Include(x => x.HtmlTemplate)
                                         .Where(x => x.CaseId == caseId &&
                                                     x.MediationCaseSessionId == mediationSessionId)
                                         .Where(IsNotExpired())
                                         .ToListAsync();
            var dictMLink = new Dictionary<int, List<MediationNotificationMLink>>();
            var caseSessionDateFrom = await GetPropByIdAsync<MediationCaseSession, DateTime>(mediationSessionId, x => x.DateFrom);

            foreach (var item in mediationNotificationListVMs)
            {
                var notifications = mediationNotifications.Where(x =>x.CasePersonL1Id == item.PersonId && x.IsMultiLink != true).ToList();
                var notificationsL = mediationNotifications.Where(x => x.IsMultiLink == true && x.MediationNotificationMLinks != null &&
                                                                  x.MediationNotificationMLinks
                                                                  .Any(m => m.IsActive && m.IsChecked && m.CasePersonSummonedId == item.PersonId)
                                                            ).ToList();
                notifications.AddRange(notificationsL);
                if (notifications.Any())
                {
                    foreach (var notification in notifications)
                    {
                        MakeNotificationListVMItem(result, dictMLink, item, notification);
                    }
                }
                else
                {
                    if ((item.DateTo != null && item.DateTo < caseSessionDateFrom) || item.DateExpired != null)
                        continue;

                    MakeNotificationListVMItem(result, dictMLink, item, null);
                }
            }
            foreach (var notificationListVM in result)
            {
                if (!notificationListVM.Notifications.Any())
                {
                    var notificationVM = new CaseSessionNotificationListNotificationVM();
                    notificationListVM.Notifications.Add(notificationVM);
                }
            }

            totalCount = result.Count;
            if (length > 0)
            {
                result = result.Skip(start).Take(length).ToList();
            }
          
            return (result, totalCount);
        }
        private void MakeNotificationListVMItem(List<MediationNotificationListVM> items, 
                                                Dictionary<int, List<MediationNotificationMLink>> dictMLink,
                                                MediationNotificationListVM item, 
                                                MediationNotification notification)
        {
            var personName = item.PersonName;
            if (notification != null)
            {
                personName = notification.NotificationPersonName;
                if (notification.IsMultiLink == true)
                {
                    List<MediationNotificationMLink> links;
                    if (dictMLink.ContainsKey(notification.Id))
                    {
                        links = dictMLink[notification.Id];
                    }
                    else
                    {
                        links = MediationPersonLinks(notification).Where(x => x.IsActive).ToList();
                        dictMLink.Add(notification.Id, links);
                    }
                    var link = links.Where(x => x.CasePersonSummonedId == item.PersonId).FirstOrDefault();
                    if (link != null)
                        personName = link.LinkLabel;
                }

            }
            var notificationListVM = items.Where(x => x.PersonId == item.PersonId &&
                                                      x.MediationSessionId == item.MediationSessionId &&
                                                      x.PersonName == personName)
                                          .FirstOrDefault();
            if (notificationListVM == null)
            {
                notificationListVM = new MediationNotificationListVM();
                notificationListVM.Id = item.Id;
                notificationListVM.MediationSessionId = item.MediationSessionId;
                notificationListVM.PersonName = personName;
                notificationListVM.PersonRole = item.PersonRole;
                notificationListVM.PersonId = item.PersonId;
                notificationListVM.RowNumber = item.RowNumber;
                notificationListVM.RoleKindId = item.RoleKindId;
                notificationListVM.Notifications = new List<CaseSessionNotificationListNotificationVM>();
                items.Add(notificationListVM);
            }
            if (notification != null)
            {
                var notificationVM = new CaseSessionNotificationListNotificationVM();
                notificationVM.Id = notification.Id;
                notificationVM.AddressString = notification.NotificationAddress?.FullAddressNotification() ?? "";
                notificationVM.Remark = ComposeRemark(notification);
                notificationVM.DateSend = notification.RegDate.ToString(FormattingConstant.NormalDateFormatHHMM);
                notificationListVM.Notifications.Add(notificationVM);
            }
            if (notificationListVM.IsDeceased == true)
                notificationListVM.PersonName += " починал";
        }

        private string ComposeRemark(MediationNotification notification)
        {
            var result = (notification.HtmlTemplate?.Label ?? "") + " - " +
                         (notification.NotificationState?.Label ?? "");
            if (notification.DeliveryDate != null)
                result += " на " + notification.DeliveryDate?.ToString(FormattingConstant.NormalDateFormatHHMM);
            result += (notification.DeliveryInfo != null ? " Данни за известяване: " + notification.DeliveryInfo + " " : string.Empty);
            switch (notification.NotificationDeliveryGroupId)
            {
                case NomenclatureConstants.NotificationDeliveryGroup.OnSession:
                    result = "Уведомен в заседание";
                    break;
                case NomenclatureConstants.NotificationDeliveryGroup.OnEMail:
                    var emailMsg = "Уведомен по електронна поща";
                    if (!string.IsNullOrEmpty(notification.NotificationAddress?.Email))
                        emailMsg += ": " + notification.NotificationAddress.Email;
                    result = $"{emailMsg} {result}";
                    break;
                case NomenclatureConstants.NotificationDeliveryGroup.OnPhone:
                    result = "Уведомен по телефон/факс";
                    if (!string.IsNullOrEmpty(notification.NotificationAddress?.Phone))
                        result += ": " + notification.NotificationAddress.Phone;
                    break;
                case NomenclatureConstants.NotificationDeliveryGroup.OnMember50:
                    result = "Уведомен по чл. 50 ал. 2 от ГПК";
                    break;
                case NomenclatureConstants.NotificationDeliveryGroup.OnMember56:
                    result = "Уведомен по чл. 56 ал. 2 от ГПК";
                    break;
                case NomenclatureConstants.NotificationDeliveryGroup.WillBeen:
                    result = "Ще бъде доведен / ще бъде призован";
                    break;
                case NomenclatureConstants.NotificationDeliveryGroup.ByEPEP:
                    result = "Уведомен чрез ЕПЕП " + result;
                    break;
                default:
                    if (notification.DatePrint == null)
                        return string.Empty;
                    break;
            }

            if (notification.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.OnPhone ||
                notification.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.OnSession ||
                notification.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.OnMember50 ||
                notification.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.OnMember56 ||
                notification.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.WillBeen)
            {
                result += (notification.DeliveryDate != null ? " Дата на уведомяване: " + notification.DeliveryDate?.ToString(FormattingConstant.NormalDateFormatHHMM) + " " : string.Empty) +
                                             (notification.DeliveryInfo != null ? " Данни за известяване: " + notification.DeliveryInfo + " " : string.Empty);
            }
            return result;
        }
        
    }
}
