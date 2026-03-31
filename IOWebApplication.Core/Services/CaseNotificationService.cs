using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.Integrations.Eispp;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using IOWebApplication.Infrastructure.Models.ViewModels.Epep;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;



namespace IOWebApplication.Core.Services
{
    public class CaseNotificationService : BaseService, ICaseNotificationService
    {
        private readonly ICounterService counterService;
        private readonly IDeliveryItemService deliveryItemService;
        private readonly IDeliveryItemOperService deliveryItemOperService;
        private readonly ICasePersonService casePersonService;
        private readonly ICaseLawUnitService caseLawUnitService;
        private readonly INomenclatureService nomenclatureService;
        private readonly ICdnService cdnService;
        private readonly IWorkNotificationService workNotificationService;
        private readonly ICasePersonLinkService casePersonLinkService;
        private readonly IMQEpepService epepService;
        private readonly IDeliveryAreaAddressService deliveryAreaAddressService;
        private readonly ICaseDeadlineService caseDeadlineService;
        public CaseNotificationService(
        ILogger<CaseNotificationService> _logger,
        ICounterService _counterService,
        IDeliveryItemService _deliveryItemService,
        IDeliveryItemOperService _deliveryItemOperService,
        ICasePersonService _casePersonService,
        ICaseLawUnitService _caseLawUnitService,
        INomenclatureService _nomenclatureService,
        ICdnService _cdnService,
        IWorkNotificationService _workNotificationService,
        ICasePersonLinkService _casePersonlinkService,
        IDeliveryAreaAddressService _deliveryAreaAddressService,
        IMQEpepService _epepService,
        ICaseDeadlineService _caseDeadlineService,
        IRepository _repo,
        IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            counterService = _counterService;
            deliveryItemService = _deliveryItemService;
            deliveryItemOperService = _deliveryItemOperService;
            casePersonService = _casePersonService;
            caseLawUnitService = _caseLawUnitService;
            nomenclatureService = _nomenclatureService;
            epepService = _epepService;
            cdnService = _cdnService;
            workNotificationService = _workNotificationService;
            casePersonLinkService = _casePersonlinkService;
            deliveryAreaAddressService = _deliveryAreaAddressService;
            caseDeadlineService = _caseDeadlineService;
        }

        public IQueryable<CaseNotificationVM> CaseNotification_Select(int CaseId, int? caseSessionId, int? caseSessionActId)
        {
            var notificationDeliveryGroup = repo.AllReadonly<Infrastructure.Data.Models.Nomenclatures.NotificationDeliveryGroup>();
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
                   NotificationDeliveryGroupLabel = notificationDeliveryGroup.Where(g => g.Id == x.NotificationDeliveryGroupId).Select(g => g.Label).FirstOrDefault()

               }).AsQueryable();
            //var sql = result.ToSql();
            return result;
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

        private void SaveCaseNotificationMLinks(CaseNotification saved, ICollection<CaseNotificationMLink> caseNotificationMLinks)
        {
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
        }

        private void SetSessionActComplain(CaseNotification model, HtmlTemplate htmlTemplate)
        {
            var complainIds = Array.Empty<int>();
            if (!string.IsNullOrEmpty(model.MultiComplainIdResultVM))
            {
                complainIds = model.MultiComplainIdResultVM.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToArray();
                complainIds = complainIds.Where(x => x > 0).ToArray();
            }
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
        }
        private void SaveCaseNotificationComplains(CaseNotification saved, CaseNotification model)
        {
            if (model.CaseNotificationComplains != null)
            {
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
                            //repo.Update(complain);
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
            }
        }

        private void SetDocuments(CaseNotification model, HtmlTemplate htmlTemplate)
        {
            if (htmlTemplate?.HaveDocuments == true)
            {
                var documentIds = Array.Empty<long>();
                if (!string.IsNullOrEmpty(model.DocumentsResultVM))
                {
                    documentIds = model.DocumentsResultVM.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray();
                    documentIds = documentIds.Where(x => x > 0).ToArray();
                }
                model.CaseNotificationDocuments = documentIds.Select(x => new CaseNotificationDocument
                {
                    CaseNotificationId = model.Id,
                    DocumentId = x,
                    DateWrt = DateTime.Now,
                    UserId = userContext.UserId,
                    IsChecked = true
                }).ToList();
            }
        }


        private void SaveCaseNotificationDocuments(CaseNotification saved, CaseNotification model)
        {
            if (model.CaseNotificationDocuments != null)
            {
                if (saved.CaseNotificationDocuments == null || saved.CaseNotificationDocuments.Count == 0)
                {
                    saved.CaseNotificationDocuments = model.CaseNotificationDocuments;
                }
                else
                {
                    foreach (var document in saved.CaseNotificationDocuments)
                    {
                        bool isChecked = model.CaseNotificationDocuments?
                                              .Any(x => x.IsChecked && x.DocumentId == document.DocumentId) ?? false;
                        if (document.IsChecked != isChecked)
                        {
                            document.IsChecked = isChecked;
                            document.DateWrt = DateTime.Now;
                            document.UserId = userContext.UserId;
                        }
                    }
                    foreach (var document in model.CaseNotificationDocuments)
                    {
                        if (!saved.CaseNotificationDocuments.Any(x => x.DocumentId == document.DocumentId))
                        {
                            saved.CaseNotificationDocuments.Add(document);
                            repo.Add(document);
                        }
                    }

                }
            }
        }

        private void SaveCaseNotificationMultiActs(CaseNotification saved, CaseNotification model)
        {
            if (model.CaseNotificationActs != null)
            {
                if (saved.CaseNotificationActs == null || saved.CaseNotificationActs.Count == 0)
                {
                    saved.CaseNotificationActs = model.CaseNotificationActs;
                }
                else
                {
                    foreach (var act in saved.CaseNotificationActs)
                    {
                        bool isChecked = model.CaseNotificationActs?
                                              .Any(x => x.IsChecked && x.CaseSessionActId == act.CaseSessionActId) ?? false;
                        if (act.IsChecked != isChecked)
                        {
                            act.IsChecked = isChecked;
                            act.DateWrt = DateTime.Now;
                            act.UserId = userContext.UserId;
                            //repo.Update(complain);
                        }
                    }
                    if (model.CaseNotificationActs != null)
                    {
                        foreach (var act in model.CaseNotificationActs)
                        {
                            if (!saved.CaseNotificationActs.Any(x => x.CaseSessionActId == act.CaseSessionActId))
                            {
                                saved.CaseNotificationActs.Add(act);
                                repo.Add(act);
                            }
                        }
                    }
                }
            }
        }


        private void SetMultiAct(CaseNotification model, HtmlTemplate htmlTemplate)
        {
            if (htmlTemplate?.HaveSessionMultiAct == true)
            {
                var actIds = Array.Empty<int>();
                if (!string.IsNullOrEmpty(model.MultiActIdResultVM))
                {
                    actIds = model.MultiActIdResultVM.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                    actIds = actIds.Where(x => x > 0).ToArray();
                }
                model.CaseNotificationActs = actIds.Select(x => new CaseNotificationAct
                {
                    CaseNotificationId = model.Id,
                    CaseSessionActId = x,
                    DateWrt = DateTime.Now,
                    UserId = userContext.UserId,
                    IsChecked = true
                }).ToList();
            }
        }
        public async Task CaseNotification_SaveData_NoTransaction(CaseNotification model, DeliveryLogVM logVM)
        {

            model.CasePersonId = model.CasePersonId.EmptyToNull();
            model.CasePersonLinkId = model.CasePersonLinkId.EmptyToNull();
            model.CasePersonAddressId = model.CasePersonAddressId.EmptyToNull();
            model.CaseLawUnitId = model.CaseLawUnitId.EmptyToNull();
            model.CaseLawUnitId = model.CaseLawUnitId.EmptyToNull();
            model.LawUnitAddressId = (model.LawUnitAddressId != null) ? ((model.LawUnitAddressId < 1) ? null : model.LawUnitAddressId) : model.LawUnitAddressId;
            model.CasePersonLinkId = model.CasePersonLinkId.EmptyToNull();
            model.CaseSessionActId = model.CaseSessionActId.EmptyToNull();
            model.CaseSessionActComplainId = model.CaseSessionActComplainId.EmptyToNull();
            model.NotificationIspnReasonId = model.NotificationIspnReasonId.EmptyToNull();
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
                casePersonLinks = FilterLinkOnSession(casePersonLinks, model.CaseSessionId, oldLinks);
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
                var caseLawUnitInfo = repo.AllReadonly<CaseLawUnit>()
                                      .Where(x => x.Id == model.CaseLawUnitId)
                                      .Select(x => new
                                      {
                                          LawUnitFullName = x.LawUnit.FullName,
                                          JudgeRoleLabel = x.JudgeRole.Label
                                      })
                                      .FirstOrDefault();

                model.NotificationPersonName = caseLawUnitInfo.LawUnitFullName;
                model.NotificationPersonDuty = caseLawUnitInfo.JudgeRoleLabel;

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
            if (!model.SkipSaveLists)
            {
                var htmlTemplate = repo.AllReadonly<HtmlTemplate>()
                                       .FirstOrDefault(x => x.Id == model.HtmlTemplateId);
                SetSessionActComplain(model, htmlTemplate);
                SetDocuments(model, htmlTemplate);
                SetMultiAct(model, htmlTemplate);
            }


            if (model.NotificationDeliveryGroupId != NomenclatureConstants.NotificationDeliveryGroup.WithSummons)
            {
                model.ToCourtId = null;
                model.LawUnitId = null;
            }

            if (model.Id > 0)
            {
                //Update
                var saved = repo.All<CaseNotification>()
                                .Include(x => x.CaseNotificationMLinks)
                                .Include(x => x.CaseNotificationComplains)
                                .Include(x => x.CaseNotificationDocuments)
                                .Include(x => x.CaseNotificationActs)
                                .Where(x => x.Id == model.Id)
                                .AsSplitQuery()
                                .FirstOrDefault();

                if (!model.SkipSaveLists)
                {
                    SaveCaseNotificationMLinks(saved, model.CaseNotificationMLinks);
                    SaveCaseNotificationComplains(saved, model);
                    SaveCaseNotificationDocuments(saved, model);
                    SaveCaseNotificationMultiActs(saved, model);
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
                if (NomenclatureConstants.NotificationDeliveryGroup.OnMoment(saved.NotificationDeliveryGroupId))
                {
                    saved.DeliveryDate = model.DeliveryDate;
                    saved.DeliveryInfo = model.DeliveryInfo;
                }
                if (NomenclatureConstants.NotificationDeliveryGroup.WithCourierLike(saved.NotificationDeliveryGroupId))
                {
                    if (NomenclatureConstants.NotificationState.NotificationDelivered().Contains(model.NotificationStateId))
                    {
                        saved.DeliveryDate = model.DeliveryDate;
                        saved.DeliveryInfo = model.DeliveryInfo;
                    }
                    else
                    {
                        saved.DeliveryDate = null;
                        saved.DeliveryInfo = String.Empty;
                    }
                }
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
                saved.InstitutionDocumentId = model.InstitutionDocumentId;
                saved.MoneyObligationId = model.MoneyObligationId;
                saved.NotificationIspnReasonId = model.NotificationIspnReasonId;
                if (model.NotificationStateId == NomenclatureConstants.NotificationState.UnDeliveredMail)
                {
                    await deliveryItemService.CreateDeliveryItem(saved, operIsChanged);
                    saved.NotificationDeliveryGroupId = NomenclatureConstants.NotificationDeliveryGroup.WithSummons;
                    saved.NotificationStateId = NomenclatureConstants.NotificationState.Ready;
                    saved.DateSend = null;
                    saved.IsFromEmail = true;
                }

                if (model.DatePrint != null)
                    saved.DatePrint = model.DatePrint;
                saved.DateWrt = DateTime.Now;
                saved.UserId = userContext.UserId;
                saved.EpepCasePersonId = model.EpepCasePersonId;
                CaseNotification_SetMLinkCaseId(saved);
                CreateHistory<CaseNotification, CaseNotificationH>(saved);
                //repo.Update(saved);
                repo.SaveChanges();
                var deliveryItem = await deliveryItemService.CreateDeliveryItem(saved, operIsChanged);
                try
                {
                    repo.SaveChanges();
                    deliveryItemService.CreateDeliveryItemOperLog(deliveryItem, null, logVM);
                    repo.SaveChanges();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Грешка при CreateDeliveryItemOperLog");
                }
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
                    if (model.NotificationStateId == NomenclatureConstants.NotificationState.UnDeliveredMail)
                    {
                        await deliveryItemService.CreateDeliveryItem(model, true);
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
                    var deliveryItem = await deliveryItemService.CreateDeliveryItem(model, true);

                    try
                    {
                        repo.SaveChanges();
                        deliveryItemService.CreateDeliveryItemOperLog(deliveryItem, null, logVM);
                        repo.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Грешка при CreateDeliveryItemOperLog");
                    }
                }
            }

            bool isFastProcess = await repo.GetPropByIdAsync<Case, bool>(x => x.Id == model.CaseId, x => x.IsFastProcess ?? false);

            if (isFastProcess)
            {
                await workNotificationService.TurnOfNotificationsForActionTakenCourtOfficerDeclatActFastProcess(model.Id);
                await workNotificationService.TurnOfNotificationsForN3(model.Id);
                await caseDeadlineService.StartUnreturnedMessageFastProcess(model.Id);
            }

            await repo.SaveChangesAsync();
        }
        public async Task<bool> CaseNotification_SaveData(CaseNotification model, DeliveryLogVM logVM)
        {
            try
            {
                if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.FearProtectsVineyard))
                {
                    try
                    {
                        ClearEntityTracker();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "ClearEntityTrackerError.CaseNotification_SaveData");
                    }
                }
                if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.ClearTrackedUsers))
                {
                    try
                    {
                        repo.StopTrackingApplicationUser();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "StopTrackingApplicationUser.CaseNotification_SaveData");
                    }
                }

                using (var ts = repo.BeginTransaction())
                {
                    await CaseNotification_SaveData_NoTransaction(model, logVM);
                    ts.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на уведомление Id={model.Id}");
            }
            return false;
        }
        public async Task<(bool, int)> DeliveryItemSaveReturn(DeliveryItemReturnVM model, ICollection<IFormFile> returnFiles, DeliveryLogVM logVM)
        {
            try
            {
                CaseNotification notification = null;
                if (model.Id > 0)
                {
                    //Update
                    var saved = await repo.GetByIdAsync<DeliveryItem>(model.Id);
                    saved.ReturnDate = model.ReturnDate;
                    if (saved.CaseNotificationId != null)
                        notification = await repo.GetByIdAsync<CaseNotification>(saved.CaseNotificationId); ;
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
                    //repo.Update(saved);
                    //if (notification != null)
                    //{
                    //    var workNotification = workNotificationService.NewWorkNotification(notification);
                    //    if (workNotification != null)
                    //        repo.Update(workNotification);


                    //    if (notification.NotificationStateId == NomenclatureConstants.NotificationState.Delivered)
                    //    {
                    //        var opers = await deliveryItemOperService.DeliveryItemOperSelect(saved.Id, true);
                    //        var oper = opers.Last();
                    //        await workNotificationService.SaveNotificationsForMessageDeliveredCaseFastProcess(notification.Id, oper.DateOper, false);
                    //        await workNotificationService.SaveNotificationsForLackSubmittedObjectionFastProcess(notification.Id, oper.DateOper, false);
                    //        await workNotificationService.SaveNotificationsForAppealActFastProcess(notification.Id, oper.DateOper, false);
                    //        await workNotificationService.SaveNotificationsForExpressingOpinionObjectionFastProcess(notification.Id, oper.DateOper, false);
                    //        await workNotificationService.SaveNotificationsForFilingClaimFastProcess(notification.Id, oper.DateOper, false);
                    //    }
                    //    await caseDeadlineService.CompleteExpiredUnreturnedMessageFastProcess(notification.Id, false);
                    //}
                    deliveryItemService.CreateDeliveryItemOperLog(saved, null, logVM);
                    await repo.SaveChangesAsync();

                    if (notification != null)
                    {
                        //Изпраща всички хартиени призовки в епеп след въвеждане на дата на връщане
                        var epepInfo = casePersonLinkService.GetEpepSummonInfo(notification, false);
                        if (epepInfo != null && epepInfo.CanSummonByEpep)
                        {
                            if (epepService.AppendCaseNotification(notification, epepInfo, EpepConstants.ServiceMethod.Add))
                            {
                                epepService.AppendCaseNotificationFile(notification.Id);
                                epepService.AppendCaseNotificationSummonReport(notification.Id);
                            }
                        }

                    }
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
                logger.LogError(ex, $"Грешка при запис на върнат отрязък DeliveryItemId={model.Id}");
                return (false, 0);
            }
        }
        public async Task<(bool, int)> DeliveryItemSaveReturnDocument(DeliveryItemReturnVM model, ICollection<IFormFile> returnFiles, DeliveryLogVM logVM)
        {
            try
            {
                DocumentNotification notification = null;
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<DeliveryItem>(model.Id);
                    saved.ReturnDate = model.ReturnDate;
                    if (saved.DocumentNotificationId != null)
                        notification = repo.GetById<DocumentNotification>(saved.DocumentNotificationId); ;
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
                    //if (notification != null)
                    //{
                    //    var workNotification = workNotificationService.NewWorkNotification(notification);
                    //    if (workNotification != null)
                    //        repo.Update(workNotification);
                    //}
                    deliveryItemService.CreateDeliveryItemOperLog(saved, null, logVM);
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
                logger.LogError(ex, $"Грешка при запис на върнат отрязък DeliveryItemId={model.Id}");
                return (false, 0);
            }
        }


        public async Task<(List<CaseSessionNotificationListVM>, int)> CaseSessionNotificationList_Select(int caseSessionId, int NotificationListTypeId, int start, int length, List<DataTablesSortColumnVM> sortedColumns, string search)
        {
            List<CaseSessionNotificationListVM> result = new List<CaseSessionNotificationListVM>();
            int caseId = await repo.AllReadonly<CaseSession>()
                             .Where(x => x.Id == caseSessionId)
                             .Select(x => x.CaseId)
                             .FirstOrDefaultAsync();

            Expression<Func<CaseSessionNotificationList, bool>> notificationListTypeExp =
                (NotificationListTypeId == SourceTypeSelectVM.CaseSessionNotificationList) ?
                x => x.NotificationListTypeId == NotificationListTypeId || x.NotificationListTypeId == null :
                x => x.NotificationListTypeId == NotificationListTypeId;
            var query = repo.AllReadonly<CaseSessionNotificationList>()
                             .Where(x => x.CaseSessionId == caseSessionId &&
                                         x.DateExpired == null)
                             .Where(notificationListTypeExp)
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
                                 IsDeleted = false,
                                 IsDeceased = false,
                                 CasePersonIdentificator = x.CasePerson.CasePersonIdentificator,
                                 DateTo = (x.CasePersonId != null) ? x.CasePerson.DateTo : x.CaseLawUnit.DateTo,
                                 DateExpired = (x.CasePersonId != null) ? x.CasePerson.DateExpired : x.DateExpired
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

            var caseSessionNotificationListVMs = query.ToList();
            Expression<Func<CaseNotification, bool>> notificationTypeExp =
                (NotificationListTypeId == SourceTypeSelectVM.CaseSessionNotificationList) ?
                x => x.NotificationTypeId == NomenclatureConstants.NotificationType.Subpoena || x.NotificationTypeId == null :
               ((NotificationListTypeId == SourceTypeSelectVM.CaseSessionNotificationListNotification) ?
                 x => x.NotificationTypeId == NomenclatureConstants.NotificationType.Notification :
                 x => x.NotificationTypeId == NomenclatureConstants.NotificationType.Message);
            var caseNotifications = await repo.AllReadonly<CaseNotification>()
                                         .Include(x => x.NotificationType)
                                         .Include(x => x.NotificationState)
                                         .Include(x => x.NotificationAddress)
                                         .Include(x => x.CaseNotificationMLinks)
                                         .Include(x => x.HtmlTemplate)
                                         .Where(x => x.CaseId == caseId &&
                                                     x.CaseSessionId == caseSessionId)
                                         .Where(notificationTypeExp)
                                         .Where(IsNotExpired())
                                         .ToListAsync();
            var dictMLink = new Dictionary<int, List<CaseNotificationMLink>>();
            var caseSessionDateFrom = await GetPropByIdAsync<CaseSession, DateTime>(caseSessionId, x => x.DateFrom);

            foreach (var item in caseSessionNotificationListVMs)
            {
                var notifications = caseNotifications.Where(x => (item.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson) ?
                                                                   x.CasePersonL1Id == item.PersonId && x.IsMultiLink != true :
                                                                   x.CaseLawUnitId == item.PersonId).ToList();
                var notificationsL = caseNotifications.Where(x => x.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson &&
                                                                  x.IsMultiLink == true &&
                                                                  x.CaseNotificationMLinks != null &&
                                                                  x.CaseNotificationMLinks.Any(m => m.IsActive && m.IsChecked && m.CasePersonSummonedId == item.PersonId)).ToList();
                notifications.AddRange(notificationsL);
                if (notifications.Any())
                {
                    foreach (var notification in notifications)
                    {
                        MakeNotificationListVMItem(result, NotificationListTypeId, dictMLink, item, notification);
                    }
                }
                else
                {
                    if ((item.DateTo != null && item.DateTo < caseSessionDateFrom) || item.DateExpired != null)
                        continue;

                    MakeNotificationListVMItem(result, NotificationListTypeId, dictMLink, item, null);
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
            var casePersonIdentificators = result.Select(x => x.CasePersonIdentificator).Distinct().ToList();
            var caseSessionPersons = await repo.AllReadonly<CasePerson>()
                                               .Where(x => x.CaseId == caseId &&
                                                           x.CaseSessionId == caseSessionId &&
                                                           casePersonIdentificators.Contains(x.CasePersonIdentificator))
                                               .ToListAsync();
            foreach (var item in result)
            {
                item.IsDeleted = caseSessionPersons.Any(p => p.CasePersonIdentificator == item.CasePersonIdentificator &&
                                                             p.DateExpired != null);
                item.IsDeceased = caseSessionPersons.Where(p => p.CasePersonIdentificator == item.CasePersonIdentificator &&
                                                                p.DateExpired != null)
                                                     .Select(p => p.IsDeceased).FirstOrDefault();
            }

            // Точен вид документ, името на бланката, изх.№ / дата, Начин на изпращане, имената на получател/адресат от регистратурата
            var caseNotificationIds = result.SelectMany(x => x.Notifications)
                                            .Select(n => (long)n.Id)
                                            .Where(x => x > 0)
                                            .ToArray();
            if (caseNotificationIds.Any())
            {
                var docTemplates = await repo.AllReadonly<DocumentTemplate>()
                                       .Include(x => x.Document)
                                       .ThenInclude(x => x.DeliveryGroup)
                                       .Include(x => x.Document)
                                       .ThenInclude(x => x.DocumentPersons)
                                       .Include(x => x.DocumentType)
                                       .Include(x => x.HtmlTemplate)
                                       .Where(x => x.SourceType == SourceTypeSelectVM.CaseNotification)
                                       .Where(x => caseNotificationIds.Contains(x.SourceId))
                                       .ToListAsync();
                foreach (var item in result)
                {
                    foreach (var nItem in item.Notifications)
                    {
                        nItem.Remark += ComposeRemarkDocumentTemplateAdd(nItem.Id, docTemplates);
                    }
                }
            }
            return (result, totalCount);
        }

        private void MakeNotificationListVMItem(List<CaseSessionNotificationListVM> items, int NotificationListTypeId,
                                                Dictionary<int, List<CaseNotificationMLink>> dictMLink,
                                                CaseSessionNotificationListVM item, CaseNotification notification)
        {
            var personName = item.PersonName;
            if (notification != null)
            {
                personName = notification.NotificationPersonName;
                if (notification.IsMultiLink == true)
                {
                    List<CaseNotificationMLink> links;
                    if (dictMLink.ContainsKey(notification.Id))
                    {
                        links = dictMLink[notification.Id];
                    }
                    else
                    {
                        links = CasePersonLinks(notification, false, NotificationListTypeId).Where(x => x.IsActive).ToList();
                        dictMLink.Add(notification.Id, links);
                    }
                    var link = links.Where(x => x.CasePersonSummonedId == item.PersonId).FirstOrDefault();
                    if (link != null)
                        personName = link.LinkLabel;
                }

            }
            var notificationListVM = items.Where(x => x.PersonId == item.PersonId &&
                                                      x.CaseSessionId == item.CaseSessionId &&
                                                      x.PersonName == personName)
                                          .FirstOrDefault();
            if (notificationListVM == null)
            {
                notificationListVM = new CaseSessionNotificationListVM();
                notificationListVM.Id = item.Id;
                notificationListVM.CaseSessionId = item.CaseSessionId;
                notificationListVM.PersonName = personName;
                notificationListVM.PersonRole = item.PersonRole;
                notificationListVM.PersonId = item.PersonId;
                notificationListVM.RowNumber = item.RowNumber;
                notificationListVM.NotificationPersonType = item.NotificationPersonType;
                notificationListVM.PersonType = item.PersonType;
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

        private string ComposeRemark(CaseNotification notification)
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
                case NomenclatureConstants.NotificationDeliveryGroup.ByRNFL:
                    result = "Уведомен чрез РНФЛ " + result;
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
        private string ComposeRemarkDocumentTemplateAdd(int id, List<DocumentTemplate> docTemplates)
        {
            var result = string.Empty;
            var docTemplateList = docTemplates.Where(x => x.SourceId == id)
                                              .ToList();
            foreach (var docTemplate in docTemplateList)
            {
                result += Environment.NewLine + docTemplate.DocumentType?.Label +
                                             (docTemplate.HtmlTemplate != null ? " " + docTemplate.HtmlTemplate.Label : string.Empty);
                if (docTemplate.Document != null)
                {
                    result += " Изпратен(а, о) на " + docTemplate.Document.DocumentDate.ToString(FormattingConstant.NormalDateFormat);
                    if (docTemplate.Document.DeliveryGroup != null)
                        result += " " + docTemplate.Document.DeliveryGroup.Label;
                    foreach (var person in docTemplate.Document.DocumentPersons)
                        result += " " + person.FullName;
                }
            }
            return result;
        }

        private async Task<IList<CheckListVM>> FillCheckListVMs_ForNotification(int caseId, int caseSessionId, int NotificationListTypeId, bool isCasePerson)
        {
            IList<CheckListVM> checkListVMs = new List<CheckListVM>();
            (var caseSessionNotificationListVMs, _) = await CaseSessionNotificationList_Select(caseSessionId, NotificationListTypeId, 0, -1, null, string.Empty);
            if (isCasePerson)
            {
                var casePersons = casePersonService.CasePersonFast_SelectForCasePreview(caseId, caseSessionId).ToList();

                foreach (var person in casePersons.OrderBy(x => x.RowNumber))
                    checkListVMs.Add(new CheckListVM()
                    {
                        Checked = (caseSessionNotificationListVMs.Where(x => x.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson).Any(x => x.PersonId == person.Id)),
                        Value = person.Id.ToString(),
                        Label = person.FullName + "(" + person.Uic + ") - " + person.RoleName,
                        Warrning = caseSessionNotificationListVMs.Where(x => x.Notifications.Any(n => !string.IsNullOrEmpty(n.Remark)))
                        //.Where(x => x.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson &&
                        //                                                     x.Notifications.Any(n => !string.IsNullOrEmpty(n.Remark)) &&
                        //                                                     x.PersonId == person.Id)
                                                                  .Any()
                                     ? "have_notification" : string.Empty
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
                        Label = caseLaw.LawUnitName + " - " + caseLaw.JudgeRoleLabel,
                        Warrning = caseSessionNotificationListVMs.Where(x => x.Notifications.Any(n => !string.IsNullOrEmpty(n.Remark)))
                        //.Where(x => x.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CaseLawUnit &&
                        //                                                     x.Notifications.Any(n => !string.IsNullOrEmpty(n.Remark)) &&
                        //                                                     x.PersonId == caseLaw.Id)
                                                                 .Any()
                                   ? "have_notification" : string.Empty
                    });
                return checkListVMs.OrderBy(x => x.Label).ToList();
            }
        }

        public async Task<CheckListViewVM> Person_SelectForCheck(int caseId, int caseSessionId, int NotificationListTypeId, bool isCasePerson)
        {

            CheckListViewVM checkListViewVM = new CheckListViewVM
            {
                CourtId = caseId,
                ObjectId = caseSessionId,
                OtherId = NotificationListTypeId,
                Label = "Изберете страни за списък за призоваване",
                ButtonLabel = "Потвърди",
                checkListVMs = await FillCheckListVMs_ForNotification(caseId, caseSessionId, NotificationListTypeId, isCasePerson)
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
                var caseSession = GetReadonly<CaseSession>(checkListViewVM.ObjectId);
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
                logger.LogError(ex, $"Грешка при запис на CaseSessionNotificationList CaseSessionId={checkListViewVM.ObjectId}");
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
                logger.LogError(ex, $"Грешка при запис на CaseSessionNotificationList CaseSessionId={checkListViewVM.ObjectId}");
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
                logger.LogError(ex, $"Грешка при запис на призован Id={model.Id}");
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

        public string GetFileNameNotification(CaseNotification notification)
        {
            var fileName = $"{notification.RegNumber}_{notification.RegDate:dd.MM.yyyy}.pdf";
            switch (notification.NotificationTypeId)
            {
                case NomenclatureConstants.NotificationType.Message:
                    fileName = "Съобщение_" + fileName;
                    break;
                case NomenclatureConstants.NotificationType.Notification:
                    fileName = "Уведомление_" + fileName;
                    break;
                case NomenclatureConstants.NotificationType.Subpoena:
                    fileName = "Призовка_" + fileName;
                    break;
                case NomenclatureConstants.NotificationType.GovernmentPaper:
                    fileName = "Призовка_" + fileName;
                    break;
                default:
                    break;
            }
            return fileName;
        }
        public async Task<bool> SavePrintedFile(int Id, byte[] pdfBytes)
        {
            var notification = repo.AllReadonly<CaseNotification>().Where(x => x.Id == Id).FirstOrDefault();
            var fileName = GetFileNameNotification(notification);
            var printRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.CaseNotificationPrint,
                SourceId = Id.ToString(),
                FileName = fileName,
                ContentType = "application/pdf",
                Title = notification.RegNumber,
                FileContentBase64 = Convert.ToBase64String(pdfBytes)
            };
            //scanRequest.FileId
            var result = await cdnService.MongoCdn_AppendUpdate(printRequest).ConfigureAwait(false);

            if (result && notification.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.ByEPEP)
            {
                //Призоката се изпраща към ЕПЕП само ако има валидна информация за връзките и наличен потребител с достъп до избраната страна
                var epepInfo = casePersonLinkService.GetEpepSummonInfo(notification);
                if (epepInfo != null && epepInfo.CanSummonByEpep)
                {
                    if (epepService.AppendCaseNotification(notification, epepInfo, EpepConstants.ServiceMethod.Add))
                    {
                        epepService.AppendCaseNotificationFile(Id);
                    }
                }
            }
            if (result && notification.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.ByRNFL)
            {
                (bool isRNFL, bool transferStarted) = await epepService.RNFL_CheckCase(notification.CaseId);
                if (isRNFL && transferStarted)
                {
                    await epepService.RNFL_SendSummon(notification.Id, EpepConstants.ServiceMethod.Add);
                }
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
        public List<CaseNotificationMLink> CasePersonLinksByNotificationId(int caseNotificationId, int casePersonId, bool filterPersonOnNotification, int notificationTypeId, int? caseSessionId)
        {
            var caseNotification = repo.AllReadonly<CaseNotification>()
                                       .Include(x => x.CaseNotificationMLinks)
                                       .Where(x => x.Id == caseNotificationId)
                                       .FirstOrDefault() ?? new CaseNotification() { CaseSessionId = caseSessionId };

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
            linksVm = FilterLinkOnSession(linksVm, caseNotification.CaseSessionId, oldLinks);
            foreach (var link in links)
                if (!linksVm.Any(x => x.Id == link.CasePersonLinkId))
                    link.IsActive = false;
            foreach (var linkVM in linksVm)
            {
                var link = links.FirstOrDefault(x => x.CasePersonLinkId == linkVM.Id);

                if (link == null)
                {
                    link = new CaseNotificationMLink()
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
                    link.CasePersonSummonedId = linkVM.isXFirst ? linkVM.PersonId : linkVM.PersonRelId;
                    link.CasePersonId = caseNotification.CasePersonId;
                    link.PersonSummonedName = linkVM.isXFirst ? linkVM.PersonName : linkVM.PersonRelName;
                    link.PersonSummonedRole = linkVM.isXFirst ? linkVM.PersonRole : linkVM.PersonRelRole;
                    link.LinkLabel = linkVM.Label;
                    link.IsActive = true;
                }
                if (linkVM.PersonSecondRelId == caseNotification.CasePersonId)
                {
                    link.PersonSummonedName = linkVM.LabelWithoutSecondRel;
                    link.PersonSummonedRole = string.Empty;
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

        public async Task<CaseNotification> ReadNotificationByIdAsync(int? id)
        {
            var result = await repo.AllReadonly<CaseNotification>()
                                   .Include(x => x.CaseNotificationActs)
                                   .Include(x => x.CaseNotificationDocuments)
                                   .Where(x => x.Id == id)
                                   .FirstOrDefaultAsync();
            if (result != null)
            {
                if (result.IsMultiLink == true)
                    result.CasePersonLinkId = -2;

                result.DeliveryDateCC = result.DeliveryDate;
                result.DeliveryInfoCC = result.DeliveryInfo;
                result.DocumentsVM = new string[0];
                if (result?.CaseNotificationDocuments?.Any() == true)
                {
                    result.DocumentsVM = result.CaseNotificationDocuments.Select(x => x.DocumentId.ToString()).ToArray();
                }
                result.MultiActIdVM = new string[0];
                if (result?.CaseNotificationActs?.Any() == true)
                {
                    result.MultiActIdVM = result.CaseNotificationActs.Where(x => x.IsChecked).Select(x => x.CaseSessionActId.ToString()).ToArray();
                }

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
        public async Task<List<int>> NotificationIdSelect(NotificationPrintFilterVM filter)
        {
            int notificationTypeId = NomenclatureConstants.NotificationType.FromListType(filter.NotificationListTypeId);
            var caseNotifications = repo.AllReadonly<CaseNotification>()
                                        .Where(x => x.CaseId == filter.CaseId &&
                                                    x.CaseSessionId == filter.CaseSessionId &&
                                                    x.DateExpired == null &&
                                                    (filter.CaseSessionActId == null || x.CaseSessionActId == filter.CaseSessionActId));
            List<int> result;
            if (filter.IsList || filter.FromRowNumber > 0 || filter.ToRowNumber > 0)
            {
                var caseSessionNotificationListVMs = repo.AllReadonly<CaseSessionNotificationList>()
                                                         .Where(x => x.CaseSessionId == filter.CaseSessionId &&
                                                                     x.DateExpired == null);
                if (filter.NotificationListTypeId != null)
                    caseSessionNotificationListVMs = caseSessionNotificationListVMs.Where(x => (x.NotificationListTypeId ?? SourceTypeSelectVM.CaseSessionNotificationList) == filter.NotificationListTypeId);

                var notifications = await caseNotifications.Where(x => caseSessionNotificationListVMs.Any(item => (item.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson) ?
                                                                                                             x.CasePersonId == item.CasePersonId && x.IsMultiLink != true :
                                                                                                             x.CaseLawUnitId == item.CaseLawUnitId) &&
                                                                 x.NotificationTypeId == notificationTypeId
                                                                ).ToListAsync();
                var notificationsL = await caseNotifications.Where(x => caseSessionNotificationListVMs.Any(item => x.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson &&
                                                                                                             x.IsMultiLink == true &&
                                                                                                             x.CaseNotificationMLinks != null &&
                                                                                                             x.CaseNotificationMLinks.Any(m => m.IsActive && m.IsChecked && m.CasePersonSummonedId == item.CasePersonId)) &&
                                                                  x.NotificationTypeId == notificationTypeId
                ).ToListAsync();
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
                    else
                    {
                        notification.NotificationNumber = caseSessionNotificationListVMs
                                                      .Where(item => notification.LawUnitId == item.CaseLawUnitId)
                                                      .Select(x => x.RowNumber)
                                                      .FirstOrDefault();
                    }
                }
                if (filter.FromRowNumber > 0 || filter.ToRowNumber > 0)
                {
                    result = notifications
                         .Where(x => x.NotificationNumber >= filter.FromRowNumber)
                         .Where(x => x.NotificationNumber <= filter.ToRowNumber)
                         .OrderBy(x => x.NotificationNumber)
                         .Select(x => x.Id)
                         .ToList();
                }
                else
                {
                    result = notifications
                       .OrderBy(x => x.NotificationNumber)
                       .Select(x => x.Id)
                       .ToList();

                }
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

        public async Task<bool> IsNotificationOnFastProcess(int notificationId)
        {
            return await repo.AllReadonly<CaseNotification>()
                             .Where(x => x.Id == notificationId)
                             .Select(x => x.Case.IsFastProcess)
                             .FirstOrDefaultAsync() ?? false;
        }

        public async Task<List<NotificationFileVM>> GetLinkDocument(int notificationId)
        {
            var result = new List<NotificationFileVM>();
            var docTemplates = repo.AllReadonly<DocumentTemplate>()
                                 .Include(x => x.Document)
                                 .Where(x => x.SourceType == SourceTypeSelectVM.CaseNotification &&
                                             x.SourceId == notificationId)
                                 .ToList();
            foreach (var docTemplate in docTemplates)
            {

                List<CdnItemVM> files = cdnService.Select(SourceTypeSelectVM.DocumentForNotification, docTemplate.DocumentId.ToString()).ToList();
                foreach (var file in files)
                {
                    var content = await cdnService.MongoCdn_Download(file, CdnFileSelect.PostProcess.Flatten).ConfigureAwait(false);
                    if (content?.FileContentBase64 != null)
                        result.Add(new NotificationFileVM
                        {
                            FileName = file.FileName,
                            IsPdf = file.FileName.EndsWith(".pdf"),
                            Content = Convert.FromBase64String(content.FileContentBase64)
                        });
                }
            }
            return result;
        }

        public async Task<List<NotificationFileVM>> GetCaseNotificationMongoFiles(int notificationId)
        {
            var result = new List<NotificationFileVM>();
            List<CdnItemVM> files = cdnService.Select(SourceTypeSelectVM.CaseNotificationDocument, notificationId.ToString()).Where(x => x.FileName.EndsWith(".pdf")).ToList();
            foreach (var file in files)
            {
                var content = await cdnService.MongoCdn_Download(file, CdnFileSelect.PostProcess.Flatten).ConfigureAwait(false);
                if (content?.FileContentBase64 != null)
                    result.Add(new NotificationFileVM
                    {
                        FileName = file.FileName,
                        IsPdf = file.FileName.EndsWith(".pdf"),
                        Content = Convert.FromBase64String(content.FileContentBase64)
                    });
            }
            return result;
        }

        public async Task<List<NotificationFileVM>> GetCaseNotificationDocuments(int notificationId)
        {
            var result = new List<NotificationFileVM>();
            var caseNotificationDocuments = repo.AllReadonly<CaseNotificationDocument>()
                                 .Where(x => x.CaseNotificationId == notificationId)
                                 .ToList();
            foreach (var caseNotificationDocument in caseNotificationDocuments)
            {
                List<CdnItemVM> files = cdnService.Select(SourceTypeSelectVM.DocumentForNotification, caseNotificationDocument.DocumentId.ToString()).ToList();
                foreach (var file in files)
                {
                    if (file.FileName.EndsWith(".cer"))
                    {
                        continue;
                    }
                    var content = await cdnService.MongoCdn_Download(file, CdnFileSelect.PostProcess.Flatten).ConfigureAwait(false);
                    if (content?.FileContentBase64 != null)
                        result.Add(new NotificationFileVM
                        {
                            FileName = file.FileName,
                            IsPdf = file.FileName.EndsWith(".pdf"),
                            Content = Convert.FromBase64String(content.FileContentBase64)
                        });
                }
            }
            return result;
        }


        public async Task<List<NotificationFileVM>> GetActAndComplainDocument(int notificationId)
        {
            var result = new List<NotificationFileVM>();
            var notification = repo.AllReadonly<CaseNotification>()
                                 .Include(x => x.CaseSessionActComplain)
                                 .Include(x => x.CaseNotificationComplains)
                                 .ThenInclude(x => x.CaseSessionActComplain)
                                 .Include(x => x.CaseNotificationActs)
                                 .Where(x => x.Id == notificationId)
                                 .First();

            List<CdnItemVM> files = new();
            if (notification.CaseSessionActId != null)
            {
                files.AddRange(cdnService.Select(SourceTypeSelectVM.CaseSessionActPdf, notification.CaseSessionActId.ToString()).ToList());
                files.AddRange(cdnService.Select(SourceTypeSelectVM.CaseSessionActManualUpload, notification.CaseSessionActId.ToString()).ToList());
            }
            if (notification.CaseNotificationActs != null)
            {
                foreach (var caseSessionAct in notification.CaseNotificationActs)
                {
                    files.AddRange(cdnService.Select(SourceTypeSelectVM.CaseSessionActPdf, caseSessionAct.CaseSessionActId.ToString()).ToList());
                    files.AddRange(cdnService.Select(SourceTypeSelectVM.CaseSessionActManualUpload, caseSessionAct.CaseSessionActId.ToString()).ToList());
                }
            }
            if (notification.CaseSessionActComplainId != null)
            {
                files.AddRange(cdnService.Select(SourceTypeSelectVM.CaseSessionActComplain, notification.CaseSessionActComplainId.ToString())
                                         .ToList());
                files.AddRange(cdnService.Select(SourceTypeSelectVM.Document, notification.CaseSessionActComplain.ComplainDocumentId.ToString())
                                      .ToList());
            }
            foreach (var item in notification.CaseNotificationComplains)
            {
                files.AddRange(cdnService.Select(SourceTypeSelectVM.CaseSessionActComplain, item.CaseSessionActComplainId.ToString())
                                         .ToList());
                files.AddRange(cdnService.Select(SourceTypeSelectVM.Document, item.CaseSessionActComplain.ComplainDocumentId.ToString())
                                         .ToList());
            }
            foreach (var file in files)
            {
                var content = await cdnService.MongoCdn_Download(file, CdnFileSelect.PostProcess.Flatten).ConfigureAwait(false);
                if (content?.FileContentBase64 != null)
                    result.Add(new NotificationFileVM
                    {
                        FileName = file.FileName,
                        IsPdf = file.FileName.EndsWith(".pdf"),
                        Content = Convert.FromBase64String(content.FileContentBase64)
                    });
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
            if (aCase?.IspnKind != NomenclatureConstants.IspnKinds.Rnfl)
            {
                deliveryGroup = deliveryGroup.Where(x => x.Id != NomenclatureConstants.NotificationDeliveryGroup.ByRNFL);
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

        public async Task<bool> IsNotificationDeliveryGroupByEpep(int caseId, int? caseSessionId, int casePersonId, string casePersonLinkIds)
        {
            int? casePersonLinkId = null;
            if (!string.IsNullOrEmpty(casePersonLinkIds))
            {
                casePersonLinkId = casePersonLinkIds.ToIntArray().FirstOrDefault();
            }
            var caseNotification = new CaseNotification()
            {
                CaseId = caseId,
                CaseSessionId = caseSessionId,
                CasePersonId = casePersonId,
                CasePersonLinkId = casePersonLinkId
            };

            var epepInfo = casePersonLinkService.GetEpepSummonInfo(caseNotification, true);

            return await Task.FromResult(epepInfo != null && epepInfo.CanSummonByEpep);


            //Сменя се - вече ще се гледа от EpepUser

            ////Първо проверявам за това дело дали има - ако има тогава взимам CasePersonIdentificator по casePersonId и после casePersonId от делото
            //var epepUsers = repo.AllReadonly<EpepUserAssignment>()
            //                  .Where(x => x.CaseId == caseId)
            //                  .Where(x => x.CanSummon ?? false == true)
            //                  .Where(x => x.DateExpired == null)
            //                  .ToList();
            //if (epepUsers.Count == 0) return false;

            //var personGuid = repo.AllReadonly<CasePerson>()
            //    .Where(x => x.CaseId == caseId)
            //    .Where(x => x.Id == casePersonId)
            //    .Select(x => x.CasePersonIdentificator)
            //    .FirstOrDefault();

            //var casePersonIdFromCase = repo.AllReadonly<CasePerson>()
            //    .Where(x => x.CaseId == caseId)
            //    .Where(x => x.CaseSessionId == null)
            //    .Where(x => x.DateExpired == null)
            //    .Where(x => x.CasePersonIdentificator == personGuid)
            //    .Select(x => x.Id)
            //    .FirstOrDefault();

            //return epepUsers.Where(x => x.CasePersonId == casePersonIdFromCase).Any();


            ////Ако няма пуснато заявление за това дело да не чете надолу излишно
            //var decisionCases = repo.AllReadonly<DocumentDecisionCase>()
            //            .Include(x => x.DocumentDecision)
            //            .Include(x => x.DocumentDecision.Document)
            //            .Include(x => x.DocumentDecision.Document.DocumentPersons)
            //            .Where(x => x.CaseId == caseId)
            //            .Where(x => x.DecisionRequestTypeId == NomenclatureConstants.DecisionRequestTypes.RequestNotification)
            //            .Where(x => x.DecisionTypeId == NomenclatureConstants.DecisionTypes.FullAccess)
            //            .ToList();

            //if (decisionCases.Count == 0) return false;

            //List<int> personIds = new List<int>();
            //personIds.Add(casePersonId);

            //if (string.IsNullOrEmpty(casePersonLinkIds) == false)
            //{
            //    int[] links = casePersonLinkIds.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToArray();
            //    var casePersonLinks = repo.AllReadonly<CasePersonLink>()
            //                 .Where(x => x.CaseId == caseId)
            //                 .Where(x => links.Contains(x.Id))
            //                 .ToList();

            //    personIds.AddRange(casePersonLinks.Where(x => x.CasePersonId > 0).Select(x => x.CasePersonId));
            //    personIds.AddRange(casePersonLinks.Where(x => x.CasePersonRelId > 0).Select(x => x.CasePersonRelId));
            //    personIds.AddRange(casePersonLinks.Where(x => (x.CasePersonSecondRelId ?? 0) > 0).Select(x => x.CasePersonSecondRelId ?? 0));
            //}
            //var personGuids = repo.AllReadonly<CasePerson>()
            //    .Where(x => x.CaseId == caseId)
            //    .Where(x => personIds.Contains(x.Id))
            //    .Select(x => x.CasePersonIdentificator);

            //var persons = repo.AllReadonly<CasePerson>()
            //    .Where(x => x.CaseId == caseId)
            //    .Where(x => x.DateExpired == null)
            //    .Where(x => personGuids.Contains(x.CasePersonIdentificator))
            //    .ToList();

            ////Или ЕГН/ЕИК или тип институция и SourceType/SourceId
            //return persons.Where(x => decisionCases.
            //            Where(a => a.DocumentDecision.Document.DocumentPersons
            //                    .Where(b => (b.UicTypeId == x.UicTypeId && b.Uic == x.Uic) ||
            //                        (b.Person_SourceType == SourceTypeSelectVM.Instutution &&
            //                         b.Person_SourceType == x.Person_SourceType &&
            //                         b.Person_SourceId == x.Person_SourceId))
            //                    .Any())
            //            .Any())
            //       .Any();
        }
        public List<SelectListItem> DocumentSenderPersonDDL(int caseId)
        {
            var docs = repo.AllReadonly<DocumentCaseInfo>()
                            .Where(x => x.CaseId == caseId &&
                                        x.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument &&
                                        x.Document.DateExpired == null)
                            .Select(x => x.Document);

            var result = repo.AllReadonly<DocumentPerson>()
                             .Where(x => docs.Any(d => x.DocumentId == d.Id))
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
        public List<SelectListItem> GetMoneyObligationDDL(int casePersonId, int caseLinkId, int caseSessionActId)
        {
            if (caseLinkId > 0)
            {
                var caseLink = repo.AllReadonly<CasePersonLink>()
                                   .Include(x => x.LinkDirection)
                                   .Where(x => x.Id == casePersonId)
                                   .FirstOrDefault();
                if (caseLink?.LinkDirection != null)
                {
                    int? posX = caseLink?.LinkDirection.LinkTemplate.IndexOf("{X}");
                    int? posY = caseLink?.LinkDirection.LinkTemplate.IndexOf("{Y}");
                    if (posY < posX)
                    {
                        casePersonId = caseLink.CasePersonRelId;
                    }
                    else
                    {
                        casePersonId = caseLink.CasePersonId;
                    }
                }
            }
            var casePersonIdentificator = repo.AllReadonly<CasePerson>()
                                              .Where(x => x.Id == casePersonId)
                                              .Select(x => x.CasePersonIdentificator)
                                              .FirstOrDefault();
            var casePersons = repo.AllReadonly<CasePerson>()
                                  .Where(x => x.CasePersonIdentificator == casePersonIdentificator);
            var result = repo.AllReadonly<Obligation>()
                             .Where(x => casePersons.Any(p => x.PersonId == p.PersonId) &&
                                         x.CaseSessionActId == caseSessionActId)
                             .Select(x => new SelectListItem()
                             {
                                 Text = $"{x.FullName} {x.Amount}",
                                 Value = x.Id.ToString()
                             }).ToList() ?? new List<SelectListItem>();

            result = result
                .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                .ToList();
            return result;
        }
        public List<HtmlTemplate> HtmlTemplateNotificationHave_Test()
        {
            var htmls = repo.AllReadonly<HtmlTemplate>().Where(x => x.HtmlTemplateTypeId == 11 || x.HtmlTemplateTypeId == 22 || x.HtmlTemplateTypeId == 23).ToList();
            var result = new List<HtmlTemplate>();
            foreach (var item in htmls)
            {
                string htmlText;
                Stream stream = new MemoryStream(item.Content);
                using (StreamReader sr = new StreamReader(stream))
                    htmlText = sr.ReadToEnd();
                if (string.IsNullOrEmpty(htmlText))
                    htmlText = "";

                if (htmlText.Contains("На основание чл.179 (3) НПК имате право да се явите с повереник"))
                    result.Add(item);
            }
            return result;
        }
        private EisppBaseCase[] InitCaseCause(int caseId, long documentId)
        {
            var cases = new List<EisppBaseCase>();
            {
                var documentCaseInfo = repo.AllReadonly<DocumentCaseInfo>()
                                           .Where(x => x.DocumentId == documentId)
                                           .Include(x => x.Court)
                                           .FirstOrDefault();
                if (documentCaseInfo != null)
                {
                    var caseCause = new EisppBaseCase();
                    caseCause.Year = documentCaseInfo.CaseYear ?? 0;
                    caseCause.ShortNumber = documentCaseInfo.CaseShortNumber;
                    var caseFrom = repo.AllReadonly<Case>()
                                       .Where(x => x.Id == documentCaseInfo.CaseId)
                                       .Include(x => x.Court)
                                       .Include(x => x.CaseType)
                                       .FirstOrDefault();
                    if (caseFrom != null)
                    {
                        caseCause.Year = caseFrom.RegDate.Year;
                        caseCause.ShortNumber = caseFrom.ShortNumber;
                        caseCause.ExactCaseType = caseFrom.CaseTypeId;
                        caseCause.CaseTypeId = caseFrom.CaseTypeId;
                        caseCause.CaseCodeId = caseFrom.CaseCodeId ?? 0;
                        caseCause.InstitutionTypeName = "Съдилища";
                        caseCause.InstitutionName = caseFrom.Court?.Label ?? "";
                        caseCause.InstitutionCaseTypeName = "Съдебно дело";
                        caseCause.CaseTypeName = caseFrom.CaseType?.Label;
                        caseCause.ConnectedCaseId = "C" + caseFrom.Id.ToString("000000000");
                    }
                    else
                    {
                        caseCause.InstitutionTypeName = "Съдилища";
                        caseCause.InstitutionCaseTypeName = "Съдебно дело";
                        caseCause.ConnectedCaseId = "D" + documentCaseInfo.Id.ToString("000000000");
                        var caseNumberDecoded = nomenclatureService.DecodeCaseRegNumber(documentCaseInfo.CaseRegNumber);
                        var documentCaseInfoCourt = repo.AllReadonly<Court>()
                                                        .Where(x => x.Id == caseNumberDecoded.CourtId)
                                                        .FirstOrDefault();
                        if (!caseNumberDecoded.IsValid || documentCaseInfoCourt == null)
                        {
                            logger.LogError($"Error DocumentCaseInfo.CaseRegNumber {documentCaseInfo.CaseRegNumber} e невалиден номер на дело", null);

                        }
                        else
                        {
                            caseCause.InstitutionName = documentCaseInfoCourt.Label ?? "";
                            caseCause.CaseCharacterId = caseNumberDecoded.CaseCharacterId;
                            var caseCharacter = repo.AllReadonly<CaseCharacter>()
                                                    .Where(x => x.Id == caseCause.CaseCharacterId)
                                                    .FirstOrDefault();
                            caseCause.CaseTypeName = caseCharacter?.Label;
                        }
                    }
                    cases.Add(caseCause);
                }
            }
            var institutionCases = repo.AllReadonly<DocumentInstitutionCaseInfo>()
                                       .Where(x => x.DocumentId == documentId)
                                       .Include(x => x.Institution)
                                       .ThenInclude(x => x.InstitutionType)
                                       .Include(x => x.InstitutionCaseType)
                                       .ToList();

            foreach (var institutionCase in institutionCases)
            {
                var caseCause = new EisppBaseCase();
                caseCause.Year = institutionCase.CaseYear;
                caseCause.ShortNumber = institutionCase.CaseNumber;
                caseCause.InstitutionCaseTypeName = institutionCase.InstitutionCaseType.Label;
                caseCause.InstitutionName = institutionCase.Institution.FullName;
                caseCause.InstitutionTypeName = institutionCase.Institution.InstitutionType.Label;
                caseCause.CaseTypeName = institutionCase.InstitutionCaseType?.Label;
                try
                {
                    caseCause.CaseType = institutionCase.InstitutionCaseTypeId ?? 0;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error InstitutionCase CaseType");
                    continue;
                }
                string institutionType = institutionCase.Institution?.InstitutionTypeId.ToString() ?? "";
                caseCause.ConnectedCaseId = "I" + institutionCase.Id.ToString("000000000");
                cases.Add(caseCause);
            }

            return cases.ToArray();
        }
        public List<SelectListItem> GetDDL_ConnectedCases(int caseId, bool addDefaultElement = true)
        {
            var aCase = repo.AllReadonly<Case>()
                            .Where(x => x.Id == caseId)
                            .FirstOrDefault();
            var connectedCases = InitCaseCause(caseId, aCase?.DocumentId ?? 0);
            var selectListItems = connectedCases.Select(x => new SelectListItem()
            {
                Text = $"{x.CaseTypeName} № {x.ShortNumber} от {x.Year}г. {x.InstitutionName}",
                Value = x.ConnectedCaseId
            }).ToList() ?? new List<SelectListItem>();
            foreach (var item in selectListItems)
            {
                if (!string.IsNullOrEmpty(item.Text))
                {
                    item.Text = item.Text.First().ToString().ToUpper() + item.Text.Substring(1);
                }
            }
            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }
            return selectListItems;
        }
        public List<SelectListItem> GetNotificationIspnReasonDDL(bool addDefaultElement = true)
        {
            var selectListItems = repo.AllReadonly<NotificationIspnReason>()
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.Accomply,
                                          Value = x.Id.ToString()
                                      })
                                      .ToList() ?? new List<SelectListItem>();
            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }
            return selectListItems;
        }




        public DateTime? GetDatePrevSession(int caseSessionId)
        {
            var caseSession = repo.AllReadonly<CaseSession>()
                                  .Where(x => x.Id == caseSessionId)
                                  .FirstOrDefault();
            if (caseSession == null)
                return null;
            var prevSession = repo.AllReadonly<CaseSession>()
                                  .Where(x => x.CaseId == caseSession.CaseId &&
                                              x.DateFrom < caseSession.DateFrom.Date)
                                  .OrderByDescending(x => x.DateFrom)
                                  .FirstOrDefault();
            return prevSession?.DateFrom;
        }
        public List<CaseNotificationLinkVM> FilterLinkOnSession(List<CaseNotificationLinkVM> links, int? caseSessionId, List<int> oldLinks)
        {
            return casePersonLinkService.FilterLinkOnSession(links, caseSessionId, oldLinks);
            //Логиката е пренесена в casePersonLinkService

            //if (caseSessionId == null || !links.Any())
            //    return links;
            //var caseSession = repo.AllReadonly<CaseSession>()
            //                      .Where(x => x.Id == caseSessionId)
            //                      .FirstOrDefault();
            //if (caseSession == null)
            //    return links;
            //var dateEnd = DateTime.Now.AddYears(100);
            //links = links.Where(x => (oldLinks != null && oldLinks.Any(o => x.Id == o)) ||
            //                         (x.DateTo ?? dateEnd) >= caseSession.DateFrom.Date)
            //             .ToList();
            //return links;
        }

        public CaseNotification GenerateFormMulti(NotificationGroupVM notificationGroup, NotificationItemVM notificationItem)
        {
            var notification = new CaseNotification();
            notification.IsMultiLink = false;
            notification.CourierTrackNum = "M";
            notification.CourtId = userContext.CourtId;
            notification.HtmlTemplateId = notificationGroup.HtmlTemplateId > 0 ? (int?)notificationGroup.HtmlTemplateId : null;
            notification.DatePrint = DateTime.Now;
            notification.MultiComplainIdResultVM = notificationGroup.MultiComplainIdResultVM;
            if (notificationItem.ToCourtId > 0)
            {
                notification.ToCourtId = notificationItem.ToCourtId.EmptyToNull();
                notification.DeliveryAreaId = notificationItem.DeliveryAreaId.EmptyToNull();
                notification.LawUnitId = notificationItem.LawUnitId.EmptyToNull();
            }
            else
            {
                if (notificationItem.AddressId > 0)
                {
                    DeliveryAreaFindVM deliveryAreaFind;
                    if (notificationItem.IsLawUnit)
                    {
                        deliveryAreaFind = deliveryAreaAddressService.DeliveryAreaAddressIdFind(notificationItem.AddressId ?? 0, notification.CourtId ?? 0);
                    }
                    else
                    {
                        deliveryAreaFind = deliveryAreaAddressService.DeliveryAreaCasePersonAddressIdFind((int)(notificationItem.AddressId ?? 0), notification.CourtId ?? 0);
                    }
                    notification.ToCourtId = deliveryAreaFind.ToCourtId;
                    notification.DeliveryAreaId = deliveryAreaFind.DeliveryAreaId;
                    notification.LawUnitId = deliveryAreaFind.LawUnitId;
                }
            }
            if (notificationItem.IsLawUnit)
            {
                notification.CaseLawUnitId = notificationItem.PersonId;
                notification.LawUnitAddressId = notificationItem.AddressId < 0 ? null : notificationItem.AddressId;
                notification.NotificationPersonType = NomenclatureConstants.NotificationPersonType.CaseLawUnit;
            }
            else
            {
                notification.NotificationPersonType = NomenclatureConstants.NotificationPersonType.CasePerson;
                notification.CasePersonId = notificationItem.PersonId;
                notification.CasePersonLinkId = notificationItem.LinkId;
                notification.CasePersonAddressId = (int?)(notificationItem.AddressId < 0 ? null : notificationItem.AddressId);

                notification.CasePersonL1Id = notification.CasePersonId;
                notification.CasePersonL2Id = null;
                notification.CasePersonL3Id = null;
                notification.LinkDirectionId = null;
                notification.LinkDirectionSecondId = null;

                CaseNotificationLinkVM casePersonLink = null;
                if (notification.IsMultiLink != true && notification.CasePersonLinkId > 0)
                {
                    var oldLinks = new List<int>() { notification.CasePersonLinkId ?? 0 };
                    var casePersonLinks = casePersonLinkService.GetLinkForPerson(notification.CasePersonId ?? 0, NomenclatureConstants.FilterPersonOnNotification, notification.NotificationTypeId ?? 0, oldLinks);
                    casePersonLinks = FilterLinkOnSession(casePersonLinks, notification.CaseSessionId, oldLinks);
                    casePersonLink = casePersonLinks.Where(x => x.Id == notification.CasePersonLinkId).FirstOrDefault();
                    if (casePersonLink != null)
                    {
                        notification.CasePersonL1Id = casePersonLink.PersonId;
                        notification.CasePersonL2Id = casePersonLink.PersonRelId;
                        if (!casePersonLink.isXFirst)
                        {
                            notification.CasePersonL1Id = casePersonLink.PersonRelId;
                            notification.CasePersonL2Id = casePersonLink.PersonId;
                        }
                        notification.LinkDirectionId = casePersonLink.LinkDirectionId;
                        notification.LinkDirectionSecondId = casePersonLink.LinkDirectionSecondId.EmptyToNull(0);
                        notification.CasePersonL3Id = casePersonLink.PersonSecondRelId.EmptyToNull(0);
                    }
                }
            }

            notification.CaseId = notificationGroup.CaseId;
            notification.CaseSessionId = notificationGroup.CaseSessionId;
            notification.CaseSessionActId = notificationGroup.CaseSessionActId.EmptyToNull();
            notification.CaseSessionActComplainId = notificationGroup.CaseSessionActComplainId.EmptyToNull();
            notification.NotificationDeliveryGroupId = notificationGroup.NotificationDeliveryGroupId.EmptyToNull();
            notification.NotificationTypeId = notificationGroup.NotificationTypeId;
            notification.DeliveryDate = notificationGroup.DeliveryDate;
            notification.NotificationStateId = notificationGroup.NotificationStateId;
            notification.NotificationIspnReasonId = notificationGroup.NotificationIspnReasonId;
            return notification;
        }
        public async Task SaveMultiNotification(NotificationGroupVM notificationGroup, DeliveryLogVM logVM)
        {
            using (var ts = repo.BeginTransaction())
            {
                foreach (var notificationItem in notificationGroup.NotificationItems)
                {
                    if (!notificationItem.IsChecked)
                        continue;
                    var notification = GenerateFormMulti(notificationGroup, notificationItem);

                    if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.ClearTrackedUsers))
                    {
                        try
                        {
                            repo.StopTrackingApplicationUser();
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "StopTrackingApplicationUser.CaseNotification_SaveData");
                        }
                    }

                    await CaseNotification_SaveData_NoTransaction(notification, logVM);
                }
                ts.Commit();
            }
        }

        public NotificationGroupVM GenerateNotificationGroup(int caseId, int caseSessionId, int notificationListTypeId)
        {
            var notificationTypeId = NomenclatureConstants.NotificationType.FromListType(notificationListTypeId);

            var notificationGroup = new NotificationGroupVM()
            {
                NotificationItems = new List<NotificationItemVM>(),
                NotificationTypeId = notificationTypeId,
                CaseSessionId = caseSessionId,
                NotificationListTypeId = notificationListTypeId,
                CaseId = caseId,
                NotificationStateId = NomenclatureConstants.NotificationState.Ready,
                NotificationDeliveryGroupId = NomenclatureConstants.NotificationDeliveryGroup.WithSummons,
            };

            return notificationGroup;
        }

        public async Task LoadNotificationGroupList(NotificationGroupVM notificationGroup)
        {
            var notificationTypeId = NomenclatureConstants.NotificationType.FromListType(notificationGroup.NotificationListTypeId);
            var notificationListsLawUnit = await repo.AllReadonly<CaseSessionNotificationList>()
                               .Where(x => x.CaseSessionId == notificationGroup.CaseSessionId &&
                                          (x.NotificationListTypeId == notificationGroup.NotificationListTypeId ||
                                           (x.NotificationListTypeId == null && notificationGroup.NotificationListTypeId == SourceTypeSelectVM.CaseSessionNotificationList)
                                          ) &&
                                          x.DateExpired == null
                                    )
                               .Where(x => x.CaseLawUnitId != null)
                               .Select(x => new NotificationItemVM()
                               {
                                   PersonId = x.CaseLawUnitId ?? 0,
                                   IsLawUnit = true,
                                   LinkId = x.CasePersonLinkId,
                                   AddressId = x.NotificationAddressId,
                                   IsChecked = true,
                                   PersonLabel = x.CaseLawUnit.LawUnit.FullName,
                                   RowNumber = x.RowNumber,
                               })
                               .ToListAsync();
            foreach (var notificationItem in notificationListsLawUnit)
            {
                notificationItem.AddressId_Ddl = LawUnitAddress_SelectDDL_ByCaseLawUnitId(notificationItem.PersonId);
            }
            var notificationLists = await repo.AllReadonly<CaseSessionNotificationList>()
                                        .Where(x => x.CaseSessionId == notificationGroup.CaseSessionId &&
                                                   (x.NotificationListTypeId == notificationGroup.NotificationListTypeId ||
                                                    (x.NotificationListTypeId == null && notificationGroup.NotificationListTypeId == SourceTypeSelectVM.CaseSessionNotificationList)
                                                   ) &&
                                                   x.DateExpired == null
                                             )
                                        .Where(x => x.CasePersonId != null)
                                        .Select(x => new NotificationItemVM()
                                        {
                                            PersonId = x.CasePersonId ?? 0,
                                            IsLawUnit = false,
                                            LinkId = x.CasePersonLinkId,
                                            AddressId = x.NotificationAddressId,
                                            IsChecked = true,
                                            PersonLabel = x.CasePerson.FullName,
                                            PersonRole = x.CasePerson.PersonRole.Label,
                                            RowNumber = x.RowNumber,
                                        })
                                        .ToListAsync();
            var casePersonIds = notificationLists.Select(x => x.PersonId).ToArray();
            var linkListAllVM = await casePersonLinkService.GetLinkForPersonList(casePersonIds, notificationGroup.CaseId, notificationGroup.CaseSessionId);

            bool addTel = notificationGroup.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.OnPhone;
            bool addMail = notificationGroup.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.OnEMail;

            var addresses = repo.AllReadonly<CasePersonAddress>()
                .Where(x => x.CasePerson.CaseId == notificationGroup.CaseId &&
                            x.CasePerson.CaseSessionId == notificationGroup.CaseSessionId)
                .Select(x => new AddressSelectItemVM()
                {
                    PersonId = x.CasePersonId,
                    Value = x.Id.ToString(),
                    Text = x.Address.FullAddressNotificationMailTel(addTel, addMail)
                })
                .ToList();
            foreach (var notificationItem in notificationLists)
            {
                var linkListVM = linkListAllVM.Where(x => x.PersonId == notificationItem.PersonId ||
                                                          x.PersonRelId == notificationItem.PersonId ||
                                                          x.PersonSecondRelId == notificationItem.PersonId)
                                              .ToList();
                linkListVM = FilterLinkOnSession(linkListVM, notificationGroup.CaseSessionId, null);
                notificationItem.LinkId_Ddl = casePersonLinkService.ListForPersonToDropDown(linkListVM, notificationItem.PersonId, true, false);
                notificationItem.AddressId_Ddl = GetAddrForPersonFromList(addresses, linkListVM, notificationItem.PersonId, notificationItem.LinkId ?? 0);

            }
            notificationLists.AddRange(notificationListsLawUnit);
            notificationLists = notificationLists.OrderBy(x => x.RowNumber).ToList();
            if (notificationLists.Count() > 500)
            {
                if (notificationGroup.ToRowNumber == 0)
                {
                    notificationGroup.FromRowNumber = 1;
                    notificationGroup.ToRowNumber = 500;
                    notificationLists.Clear();
                }
                else
                {
                    notificationLists = notificationLists.Skip(notificationGroup.FromRowNumber - 1)
                                                         .Take(notificationGroup.ToRowNumber - notificationGroup.FromRowNumber + 1)
                                                         .ToList();
                }
            }
            foreach (var notificationItem in notificationLists)
            {
                var hasPerson = notificationGroup.NotificationItems.Any(p => p.PersonId == notificationItem.PersonId);
                if (!hasPerson)
                {
                    notificationGroup.NotificationItems.Add(notificationItem);
                }
                else
                {
                    foreach (var savedItem in notificationGroup.NotificationItems)
                    {
                        if (savedItem.PersonId != notificationItem.PersonId)
                        {
                            continue;
                        }

                        savedItem.PersonLabel = notificationItem.PersonLabel;
                        savedItem.PersonRole = notificationItem.PersonRole;
                        savedItem.AddressId_Ddl = notificationItem.AddressId_Ddl;
                        savedItem.LinkId_Ddl = notificationItem.LinkId_Ddl;
                        savedItem.IsLawUnit = notificationItem.IsLawUnit;
                    }
                }
            }
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

        public List<SelectListItem> GetAddrForPersonFromList(List<AddressSelectItemVM> addrListAll, List<CaseNotificationLinkVM> linkListVM, int casePersonId, int casePersonLinkId)
        {
            int casePersonAddrId = casePersonId;
            if (casePersonLinkId > 0 && linkListVM.Any(x => x.Id == casePersonLinkId))
            {

                var casePersonLink = linkListVM.FirstOrDefault(x => x.Id == casePersonLinkId);
                if (casePersonLink != null)
                {
                    casePersonAddrId = (casePersonLink.PersonSecondRelId ?? 0) != 0 ? (casePersonLink.PersonSecondRelId ?? 0) :
                                       (casePersonLink.isXFirst ? casePersonLink.PersonRelId : casePersonLink.PersonId);
                }
            }
            var result = addrListAll.Where(x => x.PersonId == casePersonAddrId)
                                        .Select(x => new SelectListItem()
                                        {
                                            Value = x.Value,
                                            Text = x.Text
                                        })
                                        .ToList();

            if (result.Count == 0)
                result.Insert(0, new SelectListItem() { Text = "Няма данни", Value = "-1" });
            result = result.OrderBy(x => x.Text).ToList();
            return result;
        }
        public List<SelectListItem> LawUnitAddress_SelectDDL_ByCaseLawUnitId(int caseLawUnitId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var caseLawUnit = repo.AllReadonly<CaseLawUnit>()
                                  .Where(x => x.Id == caseLawUnitId)
                                  .FirstOrDefault();
            int lawUnitId = caseLawUnit?.LawUnitId ?? 0;
            var result = repo.AllReadonly<LawUnitAddress>()
                       .Include(x => x.Address)
                       .Where(x => x.LawUnitId == lawUnitId)
                       .Select(x => new SelectListItem()
                       {
                           Text = x.Address.FullAddressNotification(),
                           Value = x.AddressId.ToString()
                       }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
                result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });

            if (addAllElement)
                result.Insert(0, new SelectListItem() { Text = "Всички", Value = "-2" });

            return result;
        }

        public async Task SaveDatePrintMulti(int id)
        {
            var notification = await GetByIdAsync<CaseNotification>(id);
            notification.DatePrint = DateTime.Now;
            await repo.SaveChangesAsync();
        }

        public async Task<List<SelectListItem>> GetDocumentsDDL(int caseId)
        {
            return await repo.AllReadonly<DocumentCaseInfo>()
                             .Where(x => x.CaseId == caseId)
                             .Select(x => new SelectListItem
                             {
                                 Value = x.DocumentId.ToString(),
                                 Text = $"{x.Document.DocumentType.Label} № {x.Document.DocumentNumber} от {x.Document.ActualDocumentDate:dd.MM.yyyy} г."
                             })
                             .ToListAsync();

        }

    }

}
