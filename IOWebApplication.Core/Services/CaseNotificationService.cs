using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class CaseNotificationService : BaseService, ICaseNotificationService
    {
        private readonly ICounterService counterService;
        private readonly IDeliveryAreaService deliveryAreaService;
        private readonly IDeliveryItemService deliveryItemService;
        private readonly ICasePersonService casePersonService;
        private readonly ICaseLawUnitService caseLawUnitService;
        private readonly INomenclatureService nomenclatureService;
        private readonly ICdnService cdnService;
        private readonly IWorkNotificationService workNotificationService;
        private readonly ICasePersonLinkService casePersonLinkService;
        private readonly IMQEpepService epepService;

        public CaseNotificationService(
        ILogger<CaseNotificationService> _logger,
        AutoMapper.IMapper _mapper,
        ICounterService _counterService,
        IDeliveryAreaService _deliveryAreaService,
        IDeliveryItemService _deliveryItemService,
        ICasePersonService _casePersonService,
        ICaseLawUnitService _caseLawUnitService,
        INomenclatureService _nomenclatureService,
        ICdnService _cdnService,
        IWorkNotificationService _workNotificationService,
        ICasePersonLinkService _casePersonlinkService,
        IMQEpepService _epepService,
        IRepository _repo,
        IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            mapper = _mapper;
            userContext = _userContext;
            counterService = _counterService;
            deliveryAreaService = _deliveryAreaService;
            deliveryItemService = _deliveryItemService;
            casePersonService = _casePersonService;
            caseLawUnitService = _caseLawUnitService;
            nomenclatureService = _nomenclatureService;
            epepService = _epepService;
            cdnService = _cdnService;
            workNotificationService = _workNotificationService;
            casePersonLinkService = _casePersonlinkService;
        }

        public IQueryable<CaseNotificationVM> CaseNotification_Select(int CaseId, int? caseSessionId, int? caseSessionActId)
        {
            var result = repo.AllReadonly<CaseNotification>()
               .Include(x => x.NotificationType)
               .Include(x => x.NotificationState)
               .Include(x => x.HtmlTemplate)
               .Where(x => x.CaseId == CaseId && ((x.CaseSessionId ?? 0) == (caseSessionId ?? 0)) &&
                           ((caseSessionActId ?? 0) == 0 || (x.CaseSessionActId ?? 0) == (caseSessionActId ?? 0)))
               .Where(IsNotExpired())
               .Select(x => new CaseNotificationVM()
               {
                   Id = x.Id,
                   CaseId = x.CaseId,
                   CaseSessionId = x.CaseSessionId,
                   CaseSessionActId = x.CaseSessionActId,
                   NotificationTypeLabel = (x.NotificationType != null) ? x.NotificationType.Label : string.Empty,
                   NotificationTypeId = x.NotificationTypeId,
                   CasePersonName = x.IsMultiLink == true && x.CaseNotificationMLinks != null
                                   ? string.Join("<br>", x.CaseNotificationMLinks.Where(l => l.IsActive && l.IsChecked).Select(m => m.PersonSummonedName)) + "<br>  чрез: " + x.NotificationPersonName
                                   : x.NotificationPersonName,
                   NotificationStateLabel = (x.NotificationState != null) ? x.NotificationState.Label : string.Empty,
                   HtmlTemplateLabel = (x.HtmlTemplate != null) ? x.HtmlTemplate.Label : string.Empty,
                   RegNumber = x.RegNumber,
                   RegDate = x.RegDate,
                   NotificationNumber = x.NotificationNumber,
               }).AsQueryable();
            var sql = result.ToSql();
            return result;
        }
        private void CreateDeliveryItem(CaseNotification notification, bool operIsChanged)
        {
            if (notification.NotificationStateId == NomenclatureConstants.NotificationState.Proekt)
                return;
            if (!NomenclatureConstants.NotificationDeliveryGroup.DeliveryGroupForDeliveryItem.Contains(notification.NotificationDeliveryGroupId ?? 0))
                return;
            DeliveryItem deliveryItem = null;
            if (notification.Id > 0)
                deliveryItem = deliveryItemService.GetDeliveryItemByCaseNotificationId(notification.Id);
            deliveryItem = deliveryItem ?? new DeliveryItem();
            bool stateIsChanged = (deliveryItem.NotificationStateId != notification.NotificationStateId);
            deliveryItem.FromCourtId = notification.Case?.CourtId ?? userContext.CourtId;
            if (deliveryItem.DateSend == null && notification.NotificationStateId == NomenclatureConstants.DeliveryOper.Send)
                deliveryItem.DateSend = DateTime.Now;
            deliveryItem.DateAccepted = notification.DateAccepted;
            deliveryItem.DeliveryDate = notification.DeliveryDate;
            deliveryItem.ReturnDate = notification.ReturnDate;
            deliveryItem.RegNumber = notification.RegNumber ?? "";
            deliveryItem.RegDate = notification.RegDate;
            deliveryItem.CaseNotificationId = notification.Id;
            deliveryItem.NotificationStateId = notification.NotificationStateId;
            deliveryItem.NotificationTypeId = notification.NotificationTypeId;
            deliveryItem.PersonName = notification.NotificationPersonName;
            deliveryItem.Address = null;
            deliveryItem.AddressId = notification.NotificationAddressId ?? 0;

            deliveryItem.CourtId = notification.ToCourtId ?? (notification.CourtId ?? 0);
            deliveryItem.DeliveryAreaId = notification.DeliveryAreaId; //deliveryAreaService.GetDeliveryAreaIdByLawUnitId(deliveryItem.CourtId, notification.LawUnitId);
            deliveryItem.LawUnitId = notification.LawUnitId;

            Case aCase = notification.Case;
            if (aCase == null)
            {
                aCase = repo.AllReadonly<Case>()
                        .Where(x => x.Id == notification.CaseId)
                        .Include(x => x.CaseType)
                        .FirstOrDefault();
            }
            CaseType aCaseType = aCase?.CaseType;
            if (aCase != null && aCaseType == null)
                aCaseType = repo.AllReadonly<CaseType>()
                                .Where(x => x.Id == aCase.CaseTypeId)
                                .FirstOrDefault();
            if (aCase != null)
            {
                if (aCaseType != null)
                    deliveryItem.CaseInfo = $"{aCaseType.Code} {aCase.RegNumber} / {aCase.RegDate.ToString(FormattingConstant.NormalDateFormat)}";
                deliveryItem.CaseGroupId = aCase.CaseGroupId;
                deliveryItem.CaseTypeId = aCase.CaseTypeId;
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
        public int InsertDeliveryItem(int? courtId)
        {
            var notifications = repo.AllReadonly<CaseNotification>()
                                    .Where(x => courtId == null || x.CourtId == courtId)
                                    .Where(x => (x.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.WithCourier ||
                                                 x.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.WithCityHall) &&
                                                 !x.DeliveryItems.Any())
                                    .ToList();

            foreach (var notification in notifications)
            {
                CreateDeliveryItem(notification, true);
                repo.SaveChanges();
            }
            return notifications.Count;
        }

        private async Task<bool> SaveScanedFile(string Id, int stateId, ICollection<IFormFile> files)
        {
            if (files != null && files.Any())
            {
                var file = files.First();
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);

                    var scanRequest = new CdnUploadRequest()
                    {
                        SourceType = SourceTypeSelectVM.CaseNotificationReturn,
                        SourceId = Id,
                        FileName = file.FileName,
                        ContentType = file.ContentType,  // "application/pdf",
                        Title = stateId == NomenclatureConstants.NotificationState.UnDelivered ? $"Сканирана призовка в цялост" : $"Сканиран върнат отрязък",
                        FileContentBase64 = Convert.ToBase64String(ms.ToArray())
                    };
                    //scanRequest.FileId
                    return await cdnService.MongoCdn_AppendUpdate(scanRequest).ConfigureAwait(false);
                }
            }
            return false;
        }
        private void CaseNotification_SetMLinkCaseId(CaseNotification model)
        {
            if (model.CaseNotificationMLinks != null)
                foreach (var caseNotificationMLinks in model.CaseNotificationMLinks)
                {
                    caseNotificationMLinks.CaseId = model.CaseId;
                    caseNotificationMLinks.CourtId = model.CourtId;
                }
        }
        public bool CaseNotification_SaveData(CaseNotification model, List<CaseNotificationMLink> caseNotificationMLinks, int[] complainIds)
        {
            try
            {
                using (var scope = new TransactionScope())
                {

                    model.CasePersonId = model.CasePersonId.EmptyToNull();
                    model.CasePersonLinkId = model.CasePersonLinkId.EmptyToNull();
                    model.CasePersonAddressId = model.CasePersonAddressId.EmptyToNull();
                    model.CaseLawUnitId = model.CaseLawUnitId.EmptyToNull();
                    model.LawUnitAddressId = (model.LawUnitAddressId != null) ? ((model.LawUnitAddressId < 1) ? null : model.LawUnitAddressId) : model.LawUnitAddressId;
                    model.CasePersonLinkId = model.CasePersonLinkId.EmptyToNull();
                    model.CaseSessionActId = model.CaseSessionActId.EmptyToNull();
                    model.CaseSessionActComplainId = model.CaseSessionActComplainId.EmptyToNull();
                    model.ToCourtId = model.ToCourtId.EmptyToNull();
                    if (model.CasePersonLinkId == -2)
                    {
                        model.CasePersonLinkId = null;
                        model.IsMultiLink = true;
                    }
                    else
                    {
                        model.IsMultiLink = false;
                    }

                    model.CasePersonL1Id = model.CasePersonId;
                    model.CasePersonL2Id = null;
                    model.CasePersonL3Id = null;
                    model.LinkDirectionId = null;
                    model.LinkDirectionSecondId = null;

                    CaseNotificationLinkVM casePersonLink = null;
                    if (model.IsMultiLink != true && model.CasePersonLinkId > 0)
                    {
                        var oldLinks = new List<int>() { model.CasePersonLinkId ?? 0 };
                        var casePersonLinks = casePersonLinkService.GetLinkForPerson(model.CasePersonId ?? 0, NomenclatureConstants.FilterPersonOnNotification, model.NotificationTypeId ?? 0, oldLinks);
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
                    model.LawUnitId = model.LawUnitId.EmptyToNull();
                    model.NotificationDeliveryTypeId = model.NotificationDeliveryTypeId.EmptyToNull();
                    model.DeliveryOperId = model.DeliveryOperId.EmptyToNull();
                    model.DeliveryAreaId = model.DeliveryAreaId.EmptyToNull();

                    if (model.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson)
                    {
                        var casePerson = repo.AllReadonly<CasePerson>()
                                             .Include(x => x.PersonRole)
                                             .Where(x => x.Id == model.CasePersonL1Id)
                                             .FirstOrDefault();

                        model.NotificationPersonName = casePerson.FullName;
                        model.NotificationPersonDuty = casePerson.PersonRole.Label;
                        if (casePersonLink != null)
                        {
                            model.NotificationPersonName = casePersonLink.Label;
                            model.NotificationPersonDuty = "";
                        }

                        model.NotificationNumber = casePerson.NotificationNumber;


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
                    }
                    else
                    {
                        var caseLawUnit = repo.AllReadonly<CaseLawUnit>()
                                              .Include(x => x.LawUnit)
                                              .Include(x => x.JudgeRole)
                                              .Where(x => x.Id == model.CaseLawUnitId)
                                              .FirstOrDefault();

                        model.NotificationPersonName = caseLawUnit.LawUnit.FullName;
                        model.NotificationPersonDuty = caseLawUnit.JudgeRole.Label;

                        var address = repo.AllReadonly<Address>()
                                          .Where(x => x.Id == model.LawUnitAddressId)
                                          .FirstOrDefault();


                        if (address != null)
                        {
                            model.NotificationAddress = new Address();
                            model.NotificationAddress.CopyFrom(address);
                            if (model.Id < 1)
                                model.NotificationAddress.Id = 0;
                        }
                        model.NotificationNumber = 0;
                    }
                    if (model.NotificationAddress != null)
                        nomenclatureService.SetFullAddress(model.NotificationAddress);
                    var htmlTemplate = repo.AllReadonly<HtmlTemplate>()
                                           .FirstOrDefault(x => x.Id == model.HtmlTemplateId);
                    if (htmlTemplate?.HaveSessionActComplain == true &&
                        htmlTemplate?.HaveMultiActComplain == true &&
                        complainIds != null)
                    {
                        model.CaseNotificationComplains = new List<CaseNotificationComplain>();
                        foreach (var id in complainIds)
                        {
                            var caseNotificationComplain = new CaseNotificationComplain()
                            {
                                CaseNotificationId = model.Id,
                                CaseSessionActComplainId = id,
                                DateWrt = DateTime.Now,
                                UserId = userContext.UserId,
                                IsChecked = true
                            };
                            model.CaseNotificationComplains.Add(caseNotificationComplain);
                        }
                    }

                    if (model.Id > 0)
                    {
                        //Update
                        var saved = repo.All<CaseNotification>()
                                        .Include(x => x.CaseNotificationMLinks)
                                        .Include(x => x.CaseNotificationComplains)
                                        .Where(x => x.Id == model.Id)
                                        .FirstOrDefault();
                        if (saved.CaseNotificationMLinks == null || saved.CaseNotificationMLinks.Count == 0)
                        {
                            saved.CaseNotificationMLinks = caseNotificationMLinks;
                        }
                        else
                        {
                            foreach (var toLink in saved.CaseNotificationMLinks)
                            {
                                if (caseNotificationMLinks == null || !caseNotificationMLinks.Any(x => x.CasePersonLinkId == toLink.CasePersonLinkId))
                                {
                                    toLink.IsChecked = false;
                                    toLink.IsActive = false;
                                }
                            }
                            if (caseNotificationMLinks != null)
                            {
                                foreach (var fromLink in caseNotificationMLinks)
                                {
                                    var toLink = saved.CaseNotificationMLinks.FirstOrDefault(x => x.CasePersonLinkId == fromLink.CasePersonLinkId);
                                    if (toLink == null)
                                    {
                                        saved.CaseNotificationMLinks.Add(fromLink);
                                    }
                                    else
                                    {
                                        toLink.CaseNotificationId = fromLink.CaseNotificationId;
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
                        if (saved.CaseNotificationComplains == null || saved.CaseNotificationComplains.Count == 0)
                        {
                            saved.CaseNotificationComplains = model.CaseNotificationComplains;
                        }
                        else
                        {
                            foreach (var complain in saved.CaseNotificationComplains)
                            {
                                bool isChecked = model.CaseNotificationComplains?
                                                      .Any(x => x.IsChecked && x.CaseSessionActComplainId == complain.CaseSessionActComplainId) ?? false;
                                if (complain.IsChecked != isChecked)
                                {
                                    complain.IsChecked = isChecked;
                                    complain.DateWrt = DateTime.Now;
                                    complain.UserId = userContext.UserId;
                                    repo.Update(complain);
                                }
                            }
                            if (model.CaseNotificationComplains != null)
                            {
                                foreach (var complain in model.CaseNotificationComplains)
                                {
                                    if (!saved.CaseNotificationComplains.Any(x => x.CaseSessionActComplainId == complain.CaseSessionActComplainId))
                                    {
                                        saved.CaseNotificationComplains.Add(complain);
                                        repo.Add(complain);
                                    }
                                }
                            }
                        }

                        bool operIsChanged = (saved.DeliveryOperId != model.DeliveryOperId);
                        saved.CasePersonId = model.CasePersonId;
                        saved.CasePersonLinkId = model.CasePersonLinkId;
                        saved.CasePersonL1Id = model.CasePersonL1Id;
                        saved.CasePersonL2Id = model.CasePersonL2Id;
                        saved.CasePersonL3Id = model.CasePersonL3Id;
                        saved.IsMultiLink = model.IsMultiLink;
                        saved.CasePersonAddressId = model.CasePersonAddressId;
                        saved.NotificationTypeId = model.NotificationTypeId;
                        saved.NotificationNumber = model.NotificationNumber;
                        saved.NotificationPersonName = model.NotificationPersonName;
                        saved.NotificationPersonDuty = model.NotificationPersonDuty;
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
                        //saved.NotificationDeliveryTypeId = model.NotificationDeliveryTypeId;
                        //saved.CourierTrackNum = saved.CourierTrackNum;
                        saved.CaseSessionActId = model.CaseSessionActId;
                        saved.CaseSessionActComplainId = model.CaseSessionActComplainId;
                        saved.HaveАppendix = model.HaveАppendix;
                        saved.IsOfficialNotification = model.IsOfficialNotification;
                        saved.HtmlTemplateId = model.HtmlTemplateId;
                        saved.DeliveryAreaId = model.DeliveryAreaId;
                        saved.LawUnitId = model.LawUnitId;
                        saved.LawUnitAddressId = model.LawUnitAddressId;
                        saved.ToCourtId = model.ToCourtId;
                        saved.ExpertDeadDate = model.ExpertDeadDate;
                        saved.ExpertReport = model.ExpertReport;
                        saved.HaveDispositiv = model.HaveDispositiv;
                        saved.IsFromEmail = model.IsFromEmail;
                        saved.DocumentSenderPersonId = model.DocumentSenderPersonId;
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
                        CaseNotification_SetMLinkCaseId(saved);
                        CreateHistory<CaseNotification, CaseNotificationH>(saved);
                        repo.Update(saved);
                        repo.SaveChanges();
                        CreateDeliveryItem(saved, operIsChanged);
                        epepService.AppendCaseNotification(model, EpepConstants.ServiceMethod.Update);
                    }
                    else
                    {
                        if (counterService.Counter_GetNotificationCounter(model, userContext.CourtId))
                        {
                            if (NomenclatureConstants.NotificationDeliveryGroup.OnMoment(model.NotificationDeliveryGroupId))
                            {
                                if (model.DeliveryDate == null)
                                    model.DeliveryDate = DateTime.Now;
                            }
                            model.CaseNotificationMLinks = caseNotificationMLinks;
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
                            CaseNotification_SetMLinkCaseId(model);
                            CreateHistory<CaseNotification, CaseNotificationH>(model);
                            repo.Add<CaseNotification>(model);
                            repo.SaveChanges();
                            CreateDeliveryItem(model, true);
                            epepService.AppendCaseNotification(model, EpepConstants.ServiceMethod.Add);
                        }
                    }

                    repo.SaveChanges();
                    scope.Complete();
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на уведомление Id={ model.Id }");
            }
            return false;
        }

        public async Task<(bool, int)> DeliveryItemSaveReturn(DeliveryItemReturnVM model, ICollection<IFormFile> returnFiles)
        {
            try
            {
                CaseNotification notification = null;
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<DeliveryItem>(model.Id);
                    saved.ReturnDate = model.ReturnDate;
                    if (saved.CaseNotificationId != null)
                        notification = repo.GetById<CaseNotification>(saved.CaseNotificationId); ;
                    if (notification != null)
                    {
                        await SaveScanedFile(notification.Id.ToString(), notification.NotificationStateId, returnFiles).ConfigureAwait(false);
                        notification.ReturnInfo = model.ReturnInfo;
                        notification.ReturnDate = model.ReturnDate;
                    }
                    else
                    {
                        saved.DeliveryInfo = model.ReturnInfo;
                        await SaveScanedFile($"DI{model.Id}", model.NotificationStateId, returnFiles).ConfigureAwait(false);
                    }
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    repo.Update(saved);
                    if (notification != null)
                    {
                        var workNotification = workNotificationService.NewWorkNotification(notification);
                        if (workNotification != null)
                            repo.Update(workNotification);
                    }
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    return (false, 0);
                }
                return (true, notification?.Id ?? 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на върнат отрязък DeliveryItemId={ model.Id }");
                return (false, 0);
            }
        }

        private void SetPersonIsIsDeceased(List<CasePerson> casePersons, List<CasePerson> caseSessionPersons, CaseSessionNotificationListVM notificationListVM)
        {
            string casePersonIdentificator = caseSessionPersons
                                                      .Where(x => x.Id == notificationListVM.PersonId)
                                                      .Select(x => x.CasePersonIdentificator)
                                                      .FirstOrDefault();
            if (casePersons.Any(p => p.CasePersonIdentificator == casePersonIdentificator && p.IsDeceased == true))
                notificationListVM.PersonName += " починал";
        }

        public IQueryable<CaseSessionNotificationListVM> CaseSessionNotificationList_Select(int caseSessionId, int NotificationListTypeId)
        {
            List<CaseSessionNotificationListVM> result = new List<CaseSessionNotificationListVM>();
            int caseId = repo.AllReadonly<CaseSession>()
                             .Where(x => x.Id == caseSessionId)
                             .Select(x => x.CaseId)
                             .FirstOrDefault();
            var casePersons = repo.AllReadonly<CasePerson>()
                                .Where(x => x.CaseId == caseId &&
                                            x.CaseSessionId == null)
                                .ToList();
            var caseSessionPersons = repo.AllReadonly<CasePerson>()
                                         .Where(x => x.CaseId == caseId &&
                                         x.CaseSessionId == caseSessionId)
                                         .ToList();
            var caseSessionNotificationListVMs = repo.AllReadonly<CaseSessionNotificationList>()
                                                     .Include(x => x.CasePerson)
                                                     .ThenInclude(x => x.PersonRole)
                                                     .Include(x => x.CaseLawUnit)
                                                     .ThenInclude(x => x.LawUnit)
                                                     .Include(x => x.CaseLawUnit)
                                                     .ThenInclude(x => x.JudgeRole)
                                                     .Include(x => x.NotificationAddress)
                                                     .Where(x => x.CaseSessionId == caseSessionId &&
                                                                 x.DateExpired == null &&
                                                                 ((NotificationListTypeId == SourceTypeSelectVM.CaseSessionNotificationList) ? (x.NotificationListTypeId == NotificationListTypeId || x.NotificationListTypeId == null) : x.NotificationListTypeId == NotificationListTypeId))
                                                     .Select(x => new CaseSessionNotificationListVM()
                                                     {
                                                         Id = x.Id,
                                                         CaseSessionId = x.CaseSessionId,
                                                         PersonName = (x.CasePersonId != null) ? x.CasePerson.FullName : x.CaseLawUnit.LawUnit.FullName,
                                                         PersonRole = (x.CasePersonId != null) ? x.CasePerson.PersonRole.Label : x.CaseLawUnit.JudgeRole.Label,
                                                         PersonId = (x.CasePersonId != null) ? x.CasePerson.Id : x.CaseLawUnit.Id,
                                                         RowNumber = x.RowNumber,
                                                         NotificationPersonType = x.NotificationPersonType,
                                                         PersonType = x.NotificationPersonType,
                                                         RoleKindId = (x.CasePerson != null) ? x.CasePerson.PersonRole.RoleKindId : NomenclatureConstants.RoleKind.LeftSide,
                                                         AddressString = (x.NotificationAddress != null) ? x.NotificationAddress.FullAddressNotification() : "",
                                                         IsDeleted = (x.CasePerson != null) ?
                                                                      casePersons.Any(p => p.CasePersonIdentificator == x.CasePerson.CasePersonIdentificator && p.DateExpired != null) :
                                                                      false,
                                                         Remark = ""
                                                     }).OrderBy(x => x.RowNumber).ToList();

            var caseSession = repo.GetById<CaseSession>(caseSessionId);
            var caseNotifications = repo.AllReadonly<CaseNotification>()
                                        .Include(x => x.NotificationType)
                                        .Include(x => x.NotificationState)
                                        .Include(x => x.NotificationAddress)
                                        .Include(x => x.CaseNotificationMLinks)
                                        .Include(x => x.HtmlTemplate)
                                        .Where(x => x.CaseId == caseSession.CaseId &&
                                                    x.CaseSessionId == caseSession.Id &&
                                                    ((NotificationListTypeId == SourceTypeSelectVM.CaseSessionNotificationList) ?
                                                       x.NotificationTypeId == NomenclatureConstants.NotificationType.Subpoena || x.NotificationTypeId == null :
                                                      ((NotificationListTypeId == SourceTypeSelectVM.CaseSessionNotificationListNotification) ?
                                                        x.NotificationTypeId == NomenclatureConstants.NotificationType.Notification :
                                                        x.NotificationTypeId == NomenclatureConstants.NotificationType.Message))) //&&x.CaseSessionActId == null)
                                        .Where(IsNotExpired())
                                        .ToList();
            long[] caseNotificationIds = caseNotifications.Select(c => (long)c.Id)
                                                          .ToArray();

            // Точен вид документ, името на бланката, изх.№ / дата, Начин на изпращане, имената на получател/адресат от регистратурата
            var docTemplates = repo.AllReadonly<DocumentTemplate>()
                                   .Include(x => x.Document)
                                   .ThenInclude(x => x.DeliveryGroup)
                                   .Include(x => x.Document)
                                   .ThenInclude(x => x.DocumentPersons)
                                   .Include(x => x.DocumentType)
                                   .Include(x => x.HtmlTemplate)
                                   .Where(x => x.SourceType == SourceTypeSelectVM.CaseNotification)
                                   .Where(x => caseNotificationIds.Contains(x.SourceId))
                                   .ToList();

            foreach (var item in caseSessionNotificationListVMs)
            {
                var notifications = caseNotifications.Where(x => ((item.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson) ?
                                                                   x.CasePersonL1Id == item.PersonId && x.IsMultiLink != true :
                                                                   x.CaseLawUnitId == item.PersonId)).ToList();
                var notificationsL = caseNotifications.Where(x => x.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson &&
                                                                  x.IsMultiLink == true &&
                                                                  x.CaseNotificationMLinks != null &&
                                                                  x.CaseNotificationMLinks.Any(m => m.IsActive && m.IsChecked && m.CasePersonSummonedId == item.PersonId)).ToList();
                if (notificationsL.Any())
                    notifications.AddRange(notificationsL);
                foreach (var notification in notifications)
                {
                    CaseSessionNotificationListVM notificationListVM = new CaseSessionNotificationListVM();
                    notificationListVM.Id = item.Id;
                    notificationListVM.CaseSessionId = item.CaseSessionId;

                    notificationListVM.PersonRole = item.PersonRole;
                    notificationListVM.PersonId = item.PersonId;
                    notificationListVM.RowNumber = item.RowNumber;
                    notificationListVM.NotificationPersonType = item.NotificationPersonType;
                    notificationListVM.PersonType = item.PersonType;
                    notificationListVM.RoleKindId = item.RoleKindId;

                    notificationListVM.PersonName = notification.NotificationPersonName;
                    if (string.IsNullOrEmpty(notificationListVM.PersonName) || notification.IsMultiLink == true)
                        notificationListVM.PersonName = item.PersonName;
                    SetPersonIsIsDeceased(casePersons, caseSessionPersons, notificationListVM);

                    notificationListVM.Remark = (notification.HtmlTemplate?.Label ?? "") + " - " +
                                                (notification.NotificationState?.Label ?? "");
                    if (notification.DeliveryDate != null)
                        notificationListVM.Remark += " на " + notification.DeliveryDate?.ToString(FormattingConstant.NormalDateFormatHHMM);
                    notificationListVM.Remark += (notification.DeliveryInfo != null ? " Данни за известяване: " + notification.DeliveryInfo + " " : string.Empty);
                    switch (notification.NotificationDeliveryGroupId)
                    {
                        case NomenclatureConstants.NotificationDeliveryGroup.OnSession:
                            notificationListVM.Remark = "Уведомен в заседание";
                            break;
                        case NomenclatureConstants.NotificationDeliveryGroup.OnEMail:
                            notificationListVM.Remark = "Уведомен по електронна поща";
                            if (!string.IsNullOrEmpty(notification.NotificationAddress?.Email))
                                notificationListVM.Remark += ": " + notification.NotificationAddress.Email;
                            break;
                        case NomenclatureConstants.NotificationDeliveryGroup.OnPhone:
                            notificationListVM.Remark = "Уведомен по телефон/факс";
                            if (!string.IsNullOrEmpty(notification.NotificationAddress?.Phone))
                                notificationListVM.Remark += ": " + notification.NotificationAddress.Phone;
                            break;
                        case NomenclatureConstants.NotificationDeliveryGroup.OnMember50:
                            notificationListVM.Remark = "Уведомен по чл. 50 ал. 2 от ГПК";
                            break;
                        case NomenclatureConstants.NotificationDeliveryGroup.OnMember56:
                            notificationListVM.Remark = "Уведомен по чл. 56 ал. 2 от ГПК";
                            break;
                        case NomenclatureConstants.NotificationDeliveryGroup.WillBeen:
                            notificationListVM.Remark = "Ще бъде доведен / ще бъде призован";
                            break;
                        default:
                            break;
                    }

                    if (notification.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.OnPhone ||
                        notification.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.OnSession ||
                        notification.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.OnMember50 ||
                        notification.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.OnMember56 ||
                        notification.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.WillBeen)
                    {
                        notificationListVM.Remark += (notification.DeliveryDate != null ? " Дата на уведомяване: " + notification.DeliveryDate?.ToString(FormattingConstant.NormalDateFormatHHMM) + " " : string.Empty) +
                                                     (notification.DeliveryInfo != null ? " Данни за известяване: " + notification.DeliveryInfo + " " : string.Empty);
                    }

                    var docTemplate = docTemplates.FirstOrDefault(x => x.SourceId == notification.Id);
                    if (docTemplate != null)
                    {
                        notificationListVM.Remark += "</br>" + docTemplate.DocumentType?.Label +
                                                     (docTemplate.HtmlTemplate != null ? " " + docTemplate.HtmlTemplate.Label : string.Empty);
                        if (docTemplate.Document != null)
                        {
                            notificationListVM.Remark += " Изпратен(а, о) на " + docTemplate.Document.DocumentDate.ToString(FormattingConstant.NormalDateFormat);
                            if (docTemplate.Document.DeliveryGroup != null)
                                notificationListVM.Remark += " " + docTemplate.Document.DeliveryGroup.Label;
                            foreach (var person in docTemplate.Document.DocumentPersons)
                                notificationListVM.Remark += " " + person.FullName;
                        }
                    }

                    notificationListVM.DateSend = notification.RegDate;
                    notificationListVM.AddressString = notification.NotificationAddress?.FullAddressNotification() ?? "";
                    if (notification.IsMultiLink == true)
                        notificationListVM.AddressString += "<br>чрез " + notification.NotificationPersonName;
                    result.Add(notificationListVM);
                }

                if (!notifications.Any())
                {
                    SetPersonIsIsDeceased(casePersons, caseSessionPersons, item);
                    result.Add(item);

                }
            }

            return result.OrderBy(x => x.RowNumber).AsQueryable();
        }

        private IList<CheckListVM> FillCheckListVMs_ForNotification(int caseId, int caseSessionId, int NotificationListTypeId, bool isCasePerson)
        {
            IList<CheckListVM> checkListVMs = new List<CheckListVM>();
            var caseSessionNotificationListVMs = CaseSessionNotificationList_Select(caseSessionId, NotificationListTypeId);

            if (isCasePerson)
            {
                var casePersons = casePersonService.CasePerson_Select(caseId, caseSessionId, false, false, false).ToList();

                foreach (var person in casePersons)
                    checkListVMs.Add(new CheckListVM()
                    {
                        Checked = (caseSessionNotificationListVMs.Where(x => x.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson).Any(x => x.PersonId == person.Id)),
                        Value = person.Id.ToString(),
                        Label = person.FullName + "(" + person.Uic + ") - " + person.RoleName
                    });
                return checkListVMs.ToList();
            }
            else
            {
                var caseLawUnits = caseLawUnitService.CaseLawUnit_Select(caseId, caseSessionId, true)
                                                     .Where(x => (x.JudgeRoleId == NomenclatureConstants.JudgeRole.Jury || x.JudgeRoleId == NomenclatureConstants.JudgeRole.ExtJury || x.JudgeRoleId == NomenclatureConstants.JudgeRole.ReserveJury))
                                                     .ToList();

                foreach (var caseLaw in caseLawUnits)
                    checkListVMs.Add(new CheckListVM()
                    {
                        Checked = (caseSessionNotificationListVMs.Where(x => x.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CaseLawUnit).Any(x => x.PersonId == caseLaw.Id)),
                        Value = caseLaw.Id.ToString(),
                        Label = caseLaw.LawUnitName + " - " + caseLaw.JudgeRoleLabel
                    });
                return checkListVMs.OrderBy(x => x.Label).ToList();
            }
        }

        public CheckListViewVM Person_SelectForCheck(int caseId, int caseSessionId, int NotificationListTypeId, bool isCasePerson)
        {
            CheckListViewVM checkListViewVM = new CheckListViewVM
            {
                CourtId = caseId,
                ObjectId = caseSessionId,
                OtherId = NotificationListTypeId,
                Label = "Изберете страни за списък за призоваване",
                ButtonLabel = "Потвърди",
                checkListVMs = FillCheckListVMs_ForNotification(caseId, caseSessionId, NotificationListTypeId, isCasePerson)
            };

            return checkListViewVM;
        }

        private bool CaseNotificationList_CasePerson(CheckListViewVM checkListViewVM)
        {
            var caseSessionNotificationLists = repo.AllReadonly<CaseSessionNotificationList>().Where(x => x.CaseSessionId == checkListViewVM.ObjectId &&
                                                                                                          x.DateExpired == null &&
                                                                                                          (checkListViewVM.OtherId == SourceTypeSelectVM.CaseSessionNotificationList ? (x.NotificationListTypeId == checkListViewVM.OtherId || x.NotificationListTypeId == null) : x.NotificationListTypeId == checkListViewVM.OtherId)).ToList();
            var maxNumber = (caseSessionNotificationLists.Count > 0) ? caseSessionNotificationLists.Max(x => x.RowNumber) : 0;

            try
            {
                var caseSession = repo.GetById<CaseSession>(checkListViewVM.ObjectId);
                foreach (var checkList in checkListViewVM.checkListVMs)
                {
                    var caseSessionNotificationListVM = caseSessionNotificationLists.Where(x => (x.CasePersonId == int.Parse(checkList.Value)) && (x.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson)).FirstOrDefault();

                    if (checkList.Checked)
                    {
                        if (caseSessionNotificationListVM == null)
                        {
                            var casePersonAddresses = casePersonService.Get_CasePersonAddress(int.Parse(checkList.Value));
                            maxNumber++;
                            var notificationList = new CaseSessionNotificationList()
                            {
                                CaseId = caseSession.CaseId,
                                CourtId = caseSession.CourtId,
                                CaseSessionId = checkListViewVM.ObjectId,
                                NotificationListTypeId = checkListViewVM.OtherId,
                                CasePersonId = int.Parse(checkList.Value),
                                RowNumber = maxNumber,
                                NotificationPersonType = NomenclatureConstants.NotificationPersonType.CasePerson,
                                DateWrt = DateTime.Now,
                                UserId = userContext.UserId,
                                NotificationAddressId = ((casePersonAddresses.Count > 0) ? ((casePersonAddresses.Any(x => (x.ForNotification ?? false) == true)) ? (casePersonAddresses.Where(x => x.ForNotification == true).FirstOrDefault().AddressId) : (casePersonAddresses[0].AddressId)) : (long?)null)
                            };

                            repo.Add<CaseSessionNotificationList>(notificationList);
                        }
                    }
                    else
                    {
                        if (caseSessionNotificationListVM != null)
                        {
                            repo.Delete<CaseSessionNotificationList>(caseSessionNotificationListVM);
                        }
                    }
                }

                repo.SaveChanges();
                caseSessionNotificationLists = null;

                var numberSets = repo.All<CaseSessionNotificationList>().Where(x => x.CaseSessionId == checkListViewVM.ObjectId &&
                                                                                    (checkListViewVM.OtherId == SourceTypeSelectVM.CaseSessionNotificationList ? (x.NotificationListTypeId == checkListViewVM.OtherId || x.NotificationListTypeId == null) : x.NotificationListTypeId == checkListViewVM.OtherId)).ToList();
                var num = 0;
                foreach (var caseSessionNotificationList in numberSets.OrderBy(x => x.RowNumber))
                {
                    num++;
                    caseSessionNotificationList.RowNumber = num;
                    repo.Update(caseSessionNotificationList);
                }

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CaseSessionNotificationList CaseSessionId={ checkListViewVM.ObjectId }");
                return false;
            }
        }

        private bool CaseNotificationList_LawUnit(CheckListViewVM checkListViewVM)
        {
            var caseSessionNotificationLists = repo.AllReadonly<CaseSessionNotificationList>().Where(x => x.CaseSessionId == checkListViewVM.ObjectId &&
                                                                                                          x.DateExpired == null &&
                                                                                                          (checkListViewVM.OtherId == SourceTypeSelectVM.CaseSessionNotificationList ? (x.NotificationListTypeId == checkListViewVM.OtherId || x.NotificationListTypeId == null) : x.NotificationListTypeId == checkListViewVM.OtherId)).ToList();
            var maxNumber = (caseSessionNotificationLists.Count > 0) ? caseSessionNotificationLists.Max(x => x.RowNumber) : 0;

            try
            {
                var caseSession = repo.GetById<CaseSession>(checkListViewVM.ObjectId);
                foreach (var checkList in checkListViewVM.checkListVMs)
                {
                    var caseSessionNotificationListVM = caseSessionNotificationLists.Where(x => (x.CaseLawUnitId == int.Parse(checkList.Value)) && (x.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CaseLawUnit)).FirstOrDefault();

                    if (checkList.Checked)
                    {
                        if (caseSessionNotificationListVM == null)
                        {
                            var caseLawUnit = repo.AllReadonly<CaseLawUnit>()
                                                  .Where(x => x.Id == int.Parse(checkList.Value))
                                                  .FirstOrDefault();
                            int lawUnitId = 0;
                            if (caseLawUnit != null)
                                lawUnitId = caseLawUnit.LawUnitId;
                            var unitAddresses = repo.AllReadonly<LawUnitAddress>()
                                        .Include(x => x.Address)
                                        .Where(x => x.LawUnitId == lawUnitId)
                                        .ToList();

                            maxNumber++;
                            var notificationList = new CaseSessionNotificationList()
                            {
                                CourtId = caseSession.CourtId,
                                CaseId = caseSession.CaseId,
                                CaseSessionId = checkListViewVM.ObjectId,
                                NotificationListTypeId = checkListViewVM.OtherId,
                                CaseLawUnitId = int.Parse(checkList.Value),
                                RowNumber = maxNumber,
                                NotificationPersonType = NomenclatureConstants.NotificationPersonType.CaseLawUnit,
                                DateWrt = DateTime.Now,
                                UserId = userContext.UserId,
                                NotificationAddressId = (unitAddresses.Count > 0) ? unitAddresses.FirstOrDefault().AddressId : (long?)null
                            };
                            repo.Add<CaseSessionNotificationList>(notificationList);
                        }
                    }
                    else
                    {
                        if (caseSessionNotificationListVM != null)
                        {
                            repo.Delete<CaseSessionNotificationList>(caseSessionNotificationListVM);
                        }
                    }
                }

                repo.SaveChanges();
                caseSessionNotificationLists = null;

                var numberSets = repo.All<CaseSessionNotificationList>().Where(x => x.CaseSessionId == checkListViewVM.ObjectId &&
                                                                                    (checkListViewVM.OtherId == SourceTypeSelectVM.CaseSessionNotificationList ? (x.NotificationListTypeId == checkListViewVM.OtherId || x.NotificationListTypeId == null) : x.NotificationListTypeId == checkListViewVM.OtherId)).ToList();
                var num = 0;
                foreach (var caseSessionNotificationList in numberSets.OrderBy(x => x.RowNumber))
                {
                    num++;
                    caseSessionNotificationList.RowNumber = num;
                    repo.Update(caseSessionNotificationList);
                }

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CaseSessionNotificationList CaseSessionId={ checkListViewVM.ObjectId }");
                return false;
            }
        }

        public bool CaseNotificationList_Save(CheckListViewVM checkListViewVM, bool isCasePerson)
        {
            if (isCasePerson) return CaseNotificationList_CasePerson(checkListViewVM);
            else return CaseNotificationList_LawUnit(checkListViewVM);
        }

        public IQueryable<CaseSessionNotificationListVM> CaseSessionNotificationList_SelectByCaseId(int caseId)
        {
            return repo.AllReadonly<CaseSessionNotificationList>()
                .Include(x => x.CasePerson)
                .ThenInclude(x => x.PersonRole)
                .Include(x => x.CaseLawUnit)
                .ThenInclude(x => x.LawUnit)
                .Include(x => x.CaseLawUnit)
                .ThenInclude(x => x.JudgeRole)
                .Include(x => x.CaseSession)
                .Include(x => x.NotificationAddress)
                .Where(x => x.CaseSession.CaseId == caseId &&
                            x.DateExpired == null)
                .Select(x => new CaseSessionNotificationListVM()
                {
                    Id = x.Id,
                    CaseSessionId = x.CaseSessionId,
                    PersonName = (x.CasePersonId != null) ? x.CasePerson.FullName : x.CaseLawUnit.LawUnit.FullName,
                    PersonRole = (x.CasePersonId != null) ? x.CasePerson.PersonRole.Label : x.CaseLawUnit.JudgeRole.Label,
                    PersonId = (x.CasePersonId != null) ? x.CasePerson.Id : x.CaseLawUnit.Id,
                    RowNumber = x.RowNumber,
                    NotificationPersonType = x.NotificationPersonType,
                    AddressString = x.NotificationAddress.FullAddressNotification(),
                    NotificationListTypeId = x.NotificationListTypeId
                }).AsQueryable();
        }

        public bool CaseNotificationList_SaveData(CaseSessionNotificationList model)
        {
            try
            {
                model.NotificationAddressId = (model.NotificationAddressId == -1) ? null : model.NotificationAddressId;

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseSessionNotificationList>(model.Id);
                    saved.NotificationAddressId = model.NotificationAddressId;
                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    model.UserId = userContext.UserId;
                    model.DateWrt = DateTime.Now;
                    repo.Add<CaseSessionNotificationList>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на призован Id={ model.Id }");
                return false;
            }
        }

        public bool IsExistsNotification(int CaseSessionId, int NotificationPersonType, int PersonId, int NotificationTypeId)
        {
            return repo.AllReadonly<CaseNotification>()
                       .Any(x => x.CaseSessionId == CaseSessionId &&
                                   //    x.CaseSessionActId == null &&
                                   x.NotificationTypeId == NotificationTypeId &&
                                   ((NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson) ? x.CasePersonId == PersonId : x.CaseLawUnitId == PersonId) &&
                                   x.DateExpired == null);
        }
        public async Task<bool> SavePrintedFile(int Id, byte[] pdfBytes)
        {
            var notification = repo.AllReadonly<CaseNotification>().Where(x => x.Id == Id).FirstOrDefault();

            var printRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.CaseNotificationPrint,
                SourceId = Id.ToString(),
                FileName = notification.RegNumber + ".pdf",
                ContentType = "application/pdf",
                Title = notification.RegNumber,
                FileContentBase64 = Convert.ToBase64String(pdfBytes)
            };
            //scanRequest.FileId
            var result = await cdnService.MongoCdn_AppendUpdate(printRequest).ConfigureAwait(false);

            if (notification.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.ByEPEP)
            {
                epepService.AppendCaseNotificationFile(Id);
            }

            return result;

        }
        public async Task<CdnDownloadResult> ReadPrintedFile(int Id)
        {
            CdnItemVM aFile = cdnService.Select(SourceTypeSelectVM.CaseNotificationPrint, Id.ToString()).Where(x => x.FileName.EndsWith(".pdf")).FirstOrDefault();
            if (aFile != null)
                return await cdnService.MongoCdn_Download(aFile).ConfigureAwait(false);
            return null;
        }
        public async Task<CdnDownloadResult> ReadDraftFile(int Id)
        {
            CdnItemVM aFile = cdnService.Select(SourceTypeSelectVM.CaseNotificationPrint, Id.ToString()).Where(x => x.FileName == "draft.html").FirstOrDefault();
            if (aFile != null)
                return await cdnService.MongoCdn_Download(aFile).ConfigureAwait(false);
            return null;
        }
        public List<CaseNotificationMLink> CasePersonLinksByNotificationId(int caseNotificationId, int casePersonId, bool filterPersonOnNotification, int notificationTypeId)
        {
            var caseNotification = repo.AllReadonly<CaseNotification>()
                                       .Include(x => x.CaseNotificationMLinks)
                                       .Where(x => x.Id == caseNotificationId)
                                       .FirstOrDefault() ?? new CaseNotification();

            caseNotification.CasePersonId = casePersonId;
            return CasePersonLinks(caseNotification, filterPersonOnNotification, notificationTypeId);
        }
        public List<CaseNotificationMLink> CasePersonLinks(CaseNotification caseNotification, bool filterPersonOnNotification, int notificationTypeId)
        {
            if (caseNotification.CaseNotificationMLinks == null)
                caseNotification.CaseNotificationMLinks = repo.AllReadonly<CaseNotificationMLink>()
                                                              .Where(x => x.CaseNotificationId == caseNotification.Id)
                                                              .ToList();
            var links = caseNotification.CaseNotificationMLinks ?? new List<CaseNotificationMLink>();
            List<int> oldLinks = links.Select(x => x.CasePersonLinkId ?? 0).ToList();
            oldLinks.Add(caseNotification.CasePersonLinkId ?? 0);
            var linksVm = casePersonLinkService.GetPresentByList(caseNotification.CasePersonId ?? 0, filterPersonOnNotification, notificationTypeId, oldLinks);

            foreach (var link in links)
                if (!linksVm.Any(x => x.Id == link.CasePersonLinkId))
                    link.IsActive = false;
            foreach (var linkVM in linksVm)
            {
                if (!links.Any(x => x.CasePersonLinkId == linkVM.Id))
                {
                    var link = new CaseNotificationMLink()
                    {
                        CourtId = caseNotification.CourtId,
                        CaseId = caseNotification.CaseId,
                        CaseNotificationId = caseNotification.Id,
                        CasePersonLinkId = linkVM.Id,
                        CasePersonSummonedId = linkVM.isXFirst ? linkVM.PersonId : linkVM.PersonRelId,
                        CasePersonId = caseNotification.CasePersonId,
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
                    var link = links.FirstOrDefault(x => x.CasePersonLinkId == linkVM.Id);
                    link.CasePersonSummonedId = linkVM.isXFirst ? linkVM.PersonId : linkVM.PersonRelId;
                    link.CasePersonId = caseNotification.CasePersonId;
                    link.PersonSummonedName = linkVM.isXFirst ? linkVM.PersonName : linkVM.PersonRelName;
                    link.PersonSummonedRole = linkVM.isXFirst ? linkVM.PersonRole : linkVM.PersonRelRole;
                    link.LinkLabel = linkVM.Label;
                    link.IsActive = true;
                }
            }
            links = links.Where(x => x.CasePersonSummonedId > 0).ToList();
            foreach (var link in links)
                link.CaseNotification = null;
            return links.ToList();
        }
        public string CasePersonLinksJson(CaseNotification caseNotification, bool filterPersonOnNotification, int notificationTypeId)
        {
            var links = CasePersonLinks(caseNotification, filterPersonOnNotification, notificationTypeId).Where(x => x.IsActive).ToList();
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            return JsonConvert.SerializeObject(links.ToList(), serializerSettings);
        }
        public CaseNotification ReadById(int? id)
        {
            var result = repo.AllReadonly<CaseNotification>()
                             .Include(x => x.CaseNotificationMLinks)
                             .Include(x => x.CaseNotificationComplains)
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
        public void InitCaseNotificationComplains(CaseNotification caseNotification)
        {
            if (caseNotification?.CaseNotificationComplains != null &&
                !caseNotification.CaseNotificationComplains.Any() &&
                caseNotification.CaseSessionActComplainId > 0)
            {
                var complain = new CaseNotificationComplain()
                {
                    CaseNotificationId = caseNotification.Id,
                    CaseSessionActComplainId = caseNotification.CaseSessionActComplainId,
                    IsChecked = true
                };
                caseNotification.CaseNotificationComplains.Add(complain);
            }
        }
        public CaseNotification ReadWithMlinkById(int? id)
        {
            var result = repo.AllReadonly<CaseNotification>()
                             .Include(x => x.CaseNotificationMLinks)
                             .Where(x => x.Id == id)
                             .FirstOrDefault();
            if (result != null)
                if (result.IsMultiLink == true)
                    result.CasePersonLinkId = -2;
            return result;
        }
        public List<int> NotificationIdSelect(int? CaseId, int? caseSessionId, int? caseSessionActId, bool existsInNotificationList, int? notificationListTypeId)
        {
            int notificationTypeId = NomenclatureConstants.NotificationType.FromListType(notificationListTypeId);
            var caseNotifications = repo.AllReadonly<CaseNotification>()
                                        .Where(x => x.CaseId == CaseId &&
                                                    x.CaseSessionId == caseSessionId &&
                                                    x.DateExpired == null &&
                                                    (caseSessionActId == null || x.CaseSessionActId == caseSessionActId));
            List<int> result;
            if (existsInNotificationList)
            {
                var caseSessionNotificationListVMs = repo.AllReadonly<CaseSessionNotificationList>()
                                                         .Where(x => x.CaseSessionId == caseSessionId &&
                                                                     x.DateExpired == null &&
                                                                     (x.NotificationListTypeId ?? SourceTypeSelectVM.CaseSessionNotificationList) == notificationListTypeId);
                var notifications = caseNotifications.Where(x => caseSessionNotificationListVMs.Any(item => (item.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson) ?
                                                                                                             x.CasePersonId == item.CasePersonId && x.IsMultiLink != true :
                                                                                                             x.CaseLawUnitId == item.CaseLawUnitId) &&
                                                                 x.NotificationTypeId == notificationTypeId
                                                                ).ToList();
                var notificationsL = caseNotifications.Where(x => caseSessionNotificationListVMs.Any(item => x.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson &&
                                                                                                             x.IsMultiLink == true &&
                                                                                                             x.CaseNotificationMLinks != null &&
                                                                                                             x.CaseNotificationMLinks.Any(m => m.IsActive && m.IsChecked && m.CasePersonSummonedId == item.CasePersonId)) &&
                                                                  x.NotificationTypeId == notificationTypeId
                ).ToList();
                notifications.AddRange(notificationsL);
                foreach (var notification in notifications)
                {
                    if (notification.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson)
                    {

                        if (notification.IsMultiLink == true)
                        {
                            notification.NotificationNumber = caseSessionNotificationListVMs
                                                                 .Where(item => notification.CaseNotificationMLinks != null &&
                                                                                notification.CaseNotificationMLinks.Any(m => m.IsActive && m.IsChecked && m.CasePersonSummonedId == item.CasePersonId))
                                                                 .Select(x => x.RowNumber)
                                                                 .FirstOrDefault();
                        }
                        else
                        {
                            notification.NotificationNumber = caseSessionNotificationListVMs
                                                          .Where(item => notification.CasePersonId == item.CasePersonId)
                                                          .Select(x => x.RowNumber)
                                                          .FirstOrDefault();
                        }
                    }
                }
                result = notifications
                     .OrderBy(x => x.NotificationNumber)
                     .Select(x => x.Id)
                     .ToList();
            }
            else
            {
                result = caseNotifications.Select(x => x.Id).ToList();
            }
            return result;
        }

        private Expression<Func<CaseNotification, bool>> IsNotExpired()
        {
            return x => x.DateExpired == null;
        }

        public bool SaveExpireInfoPlus(ExpiredInfoVM model)
        {
            var saved = repo.GetById<CaseNotification>(model.Id);
            if (saved != null)
            {
                saved.DateExpired = DateTime.Now;
                saved.UserExpiredId = userContext.UserId;
                saved.DescriptionExpired = model.DescriptionExpired;
                var deliveryItem = repo.All<DeliveryItem>()
                                       .Where(x => x.CaseNotificationId == model.Id)
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

        public bool IsExistNotificationForSession(int caseSessionId)
        {
            return repo.AllReadonly<CaseNotification>()
                       .Any(x => x.CaseSessionId == caseSessionId &&
                                 x.CaseSessionActId == null &&
                                 x.DateExpired == null);
        }

        public List<SelectListItem> GetDDL_NotificationListType()
        {
            var selectListItems = new List<SelectListItem>();
            selectListItems.Insert(0, new SelectListItem() { Text = "Списък за призоваване", Value = SourceTypeSelectVM.CaseSessionNotificationList.ToString() });
            selectListItems.Insert(1, new SelectListItem() { Text = "Списък за уведомяване", Value = SourceTypeSelectVM.CaseSessionNotificationListNotification.ToString() });
            selectListItems.Insert(2, new SelectListItem() { Text = "Списък за съобщаване", Value = SourceTypeSelectVM.CaseSessionNotificationListMessage.ToString() });
            return selectListItems;
        }

        public async Task<List<byte[]>> GetLinkDocument(int notificationId)
        {
            var result = new List<byte[]>();
            var docTemplates = repo.AllReadonly<DocumentTemplate>()
                                 .Include(x => x.Document)
                                 .Where(x => x.SourceType == SourceTypeSelectVM.CaseNotification &&
                                             x.SourceId == notificationId)
                                 .ToList();
            foreach (var docTemplate in docTemplates)
            {

                List<CdnItemVM> files = cdnService.Select(SourceTypeSelectVM.DocumentPdf, docTemplate.DocumentId.ToString()).Where(x => x.FileName.EndsWith(".pdf")).ToList();
                foreach (var file in files)
                {
                    var content = await cdnService.MongoCdn_Download(file).ConfigureAwait(false);
                    if (content?.FileContentBase64 != null)
                        result.Add(Convert.FromBase64String(content.FileContentBase64));
                }
            }
            return result;
        }
        public List<SelectListItem> NotificationDeliveryGroupDDL(int notificationTypeId, int caseId)
        {
            DateTime today = DateTime.Today;
            var aCase = repo.AllReadonly<Case>()
                            .Where(x => x.Id == caseId)
                            .FirstOrDefault();
            var deliveryGroup = repo.AllReadonly<DeliveryTypeGroup>()
                                    .Where(x => x.NotificationTypeId == notificationTypeId &&
                                                x.NotificationDeliveryGroup.IsActive &&
                                                x.NotificationDeliveryGroup.DateStart <= today &&
                                               (x.NotificationDeliveryGroup.DateEnd ?? today) >= today)
                                    .Select(x => x.NotificationDeliveryGroup);

            if (aCase?.CaseGroupId != NomenclatureConstants.CaseGroups.GrajdanskoDelo &&
                aCase?.CaseGroupId != NomenclatureConstants.CaseGroups.Trade)
            {
                deliveryGroup = deliveryGroup.Where(x => x.Id != NomenclatureConstants.NotificationDeliveryGroup.OnMember50);
            }
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

        public bool IsNotificationDeliveryGroupByEpep(int caseId, int casePersonId, string casePersonLinkIds)
        {
            //Ако няма пуснато заявление за това дело да не чете надолу излишно
            var decisionCases = repo.AllReadonly<DocumentDecisionCase>()
                        .Include(x => x.DocumentDecision)
                        .Include(x => x.DocumentDecision.Document)
                        .Include(x => x.DocumentDecision.Document.DocumentPersons)
                        .Where(x => x.CaseId == caseId)
                        .Where(x => x.DecisionRequestTypeId == NomenclatureConstants.DecisionRequestTypes.RequestNotification)
                        .Where(x => x.DecisionTypeId == NomenclatureConstants.DecisionTypes.FullAccess)
                        .ToList();

            if (decisionCases.Count == 0) return false;

            List<int> personIds = new List<int>();
            personIds.Add(casePersonId);

            if (string.IsNullOrEmpty(casePersonLinkIds) == false)
            {
                int[] links = casePersonLinkIds.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToArray();
                var casePersonLinks = repo.AllReadonly<CasePersonLink>()
                             .Where(x => x.CaseId == caseId)
                             .Where(x => links.Contains(x.Id))
                             .ToList();

                personIds.AddRange(casePersonLinks.Where(x => x.CasePersonId > 0).Select(x => x.CasePersonId));
                personIds.AddRange(casePersonLinks.Where(x => x.CasePersonRelId > 0).Select(x => x.CasePersonRelId));
                personIds.AddRange(casePersonLinks.Where(x => (x.CasePersonSecondRelId ?? 0) > 0).Select(x => x.CasePersonSecondRelId ?? 0));
            }
            var personGuids = repo.AllReadonly<CasePerson>()
                .Where(x => x.CaseId == caseId)
                .Where(x => personIds.Contains(x.Id))
                .Select(x => x.CasePersonIdentificator);

            var persons = repo.AllReadonly<CasePerson>()
                .Where(x => x.CaseId == caseId)
                .Where(x => x.DateExpired == null)
                .Where(x => personGuids.Contains(x.CasePersonIdentificator))
                .ToList();

            //Или ЕГН/ЕИК или тип институция и SourceType/SourceId
            return persons.Where(x => decisionCases.
                        Where(a => a.DocumentDecision.Document.DocumentPersons
                                .Where(b => (b.UicTypeId == x.UicTypeId && b.Uic == x.Uic) ||
                                    (b.Person_SourceType == SourceTypeSelectVM.Instutution &&
                                     b.Person_SourceType == x.Person_SourceType &&
                                     b.Person_SourceId == x.Person_SourceId))
                                .Any())
                        .Any())
                   .Any();
        }
        public List<SelectListItem> DocumentSenderPersonDDL(int caseId)
        {
            var docs = repo.AllReadonly<DocumentCaseInfo>()
                            .Where(x => x.CaseId == caseId &&
                                        x.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument &&
                                        x.Document.DateExpired == null)
                            .Select(x => x.Document);

            var result = repo.AllReadonly<DocumentPerson>()
                             .Where(x => docs.Any(d => x.DocumentId == d.Id) &&
                                         x.IsPerson)
                             .Select(x => new SelectListItem()
                             {
                                 Text = x.FullName,
                                 Value = x.Id.ToString()
                             }).ToList() ?? new List<SelectListItem>();

            result = result
                .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                .ToList();
            return result;
        }

        public List<CaseNotification> GetNotPrintedEpep()
        {
            var mongoFiles = repo.AllReadonly<MongoFile>()
                                 .Where(x => x.SourceType == SourceTypeSelectVM.CaseNotificationPrint);

            return repo.AllReadonly<CaseNotification>()
                       .Where(x => x.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.ByEPEP &&
                                   mongoFiles.Any(fl => fl.SourceIdNumber == x.Id && fl.FileName == "draft.html") &&
                                   !mongoFiles.Any(fl => fl.SourceIdNumber == x.Id && fl.FileName != "draft.html"))
                       .ToList();
        }
    }
}
