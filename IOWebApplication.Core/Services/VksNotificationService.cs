// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
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
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class VksNotificationService : BaseService, IVksNotificationService
    {
        private readonly ICasePersonLinkService casePersonLinkService;
        private readonly ICdnService cdnService;
        private readonly INomenclatureService nomService;
        public VksNotificationService(
            ILogger<VksNotificationService> _logger,
            IRepository _repo,
            IUserContext _userContext,
            ICasePersonLinkService _casePersonLinkService,
            ICdnService _cdnService,
            INomenclatureService _nomService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            casePersonLinkService = _casePersonLinkService;
            cdnService = _cdnService;
            nomService = _nomService;
        }
        public VksNotificationListVM GetNotificationItem(int caseSessionId)
        {
            var caseSession = repo.AllReadonly<CaseSession>()
                                  .Where(x => x.Id == caseSessionId)
                                  .FirstOrDefault();

            int vksMonth = caseSession.DateFrom.Year * 100 + caseSession.DateFrom.Month;
            var vksHeader = repo.AllReadonly<VksNotificationHeader>()
                                .Where(x => x.Month == vksMonth)
                                .FirstOrDefault();
            var notificationList = new VksNotificationListVM()
            {
                CaseId = caseSession.CaseId,
                CaseSessionId = caseSessionId,
                PaperEdition = vksHeader?.PaperEdition,
                CheckRow = repo.AllReadonly<VksNotificationPrintList>()
                               .Where(x => x.CaseSessionId == caseSessionId)
                               .Select(x => x.CheckRow)
                               .FirstOrDefault()
            };
            FillNotificationItem(notificationList);
            FillNotificationItemDDL(notificationList);
            return notificationList;
        }

        public void FillNotificationItem(VksNotificationListVM notificationList)
        {
            notificationList.VksNotificationItems = new List<VksNotificationItemVM>();
            var caseNotifications = repo.AllReadonly<CaseSessionNotificationList>()
                                        .Include(x => x.VksNotificationHeader)
                                        .Where(x => x.CaseId == notificationList.CaseId &&
                                                    x.CaseSessionId == notificationList.CaseSessionId &&
                                                    x.NotificationListTypeId == SourceTypeSelectVM.CaseSessionNotificationListNotificationDP &&
                                                    x.DateExpired == null)
                                        .ToList();
            var casePersons = repo.AllReadonly<CasePerson>()
                                  .Include(x => x.PersonRole)
                                  .Where(x => x.CaseId == notificationList.CaseId &&
                                              x.CaseSessionId == notificationList.CaseSessionId &&
                                              x.DateExpired == null)
                                  .ToList();
            foreach (var person in casePersons)
            {
                var notificationItem = new VksNotificationItemVM();
                var caseNotification = caseNotifications.FirstOrDefault(x => x.CasePersonId == person.Id);
                notificationItem.CasePersonId = person.Id;
                if (caseNotification != null)
                {
                    notificationItem.RowNumber = caseNotification.RowNumber;
                    notificationItem.CasePersonLinkId = caseNotification.CasePersonLinkId ?? 0;
                    notificationItem.VksNotificationStateId = caseNotification.VksNotificationStateId;
                    notificationItem.NotificationAddressId = caseNotification.NotificationAddressId;
                    notificationItem.NotificationListId = caseNotification.Id;
                }
                else
                {
                    var linkListVM = casePersonLinkService.GetLinkForPerson(person.Id,
                                                                      false,
                                                                      SourceTypeSelectVM.CaseSessionNotificationListNotificationDP,
                                                                      null);
                    linkListVM = linkListVM.Where(x => (x.isXFirst && x.PersonId == person.Id) ||
                                                       (!x.isXFirst && x.PersonRelId == person.Id))
                                           .ToList();
                    if (linkListVM.Count() == 1)
                    {
                        notificationItem.CasePersonLinkId = linkListVM.First().Id;
                    }
                    notificationItem.VksNotificationStateId = 2;
                }

                notificationItem.PersonName = GetCleanFullName(person.FullName);
                notificationItem.PaperEdition = notificationList.CheckRow ? caseNotification?.VksNotificationHeader?.PaperEdition : string.Empty;

                notificationList.VksNotificationItems.Add(notificationItem);
            }
        }
        public List<SelectListItem> GetAddrForPerson(List<CaseNotificationLinkVM> linkListVM, int casePersonId, int casePersonLinkId)
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
                addrList = GetDDL_CasePersonAddress(casePersonAddrId);
            }
            else
            {
                addrList = GetDDL_CasePersonAddress(casePersonId);
            }
            return addrList;
        }
        public void FillNotificationItemDDL(VksNotificationListVM notificationList)
        {
            foreach (var item in notificationList.VksNotificationItems)
            {
                var linkListVM = casePersonLinkService.GetLinkForPerson(item.CasePersonId ?? 0,
                                                                        false,
                                                                        SourceTypeSelectVM.CaseSessionNotificationListNotificationDP,
                                                                        null);
                linkListVM = linkListVM.Where(x => (x.isXFirst && x.PersonId == item.CasePersonId) ||
                                                   (!x.isXFirst && x.PersonRelId == item.CasePersonId))
                                       .ToList();
                foreach (var link in linkListVM)
                {
                    SetVksLabelFromTemplate(link);
                }

                item.CasePersonLinksDdl = casePersonLinkService.ListForPersonToDropDown(linkListVM, item.CasePersonId ?? 0)
                                                               .Where(x => x.Value != "-2")
                                                               .ToList();
                item.NotificationAddressesDdl = GetAddrForPerson(linkListVM, item.CasePersonId ?? 0, item.CasePersonLinkId ?? 0);
                if (item.NotificationAddressId == null && item.NotificationAddressesDdl.Count(x => x.Value != "-1") > 0)
                {
                    item.NotificationAddressId = long.Parse(item.NotificationAddressesDdl.First().Value);
                }
                item.VksNotificationStatesDdl = nomService.GetDropDownList<VksNotificationState>(false);
            }
        }
        public List<SelectListItem> GetVksPersonAdress(int casePersonId, int? casePersonLinkId)
        {
            var linkListVM = casePersonLinkService.GetLinkForPerson(casePersonId,
                                                                    false,
                                                                    SourceTypeSelectVM.CaseSessionNotificationListNotificationDP,
                                                                    null);
            return GetAddrForPerson(linkListVM, casePersonId, casePersonLinkId ?? 0);
        }
        public bool SaveData(VksNotificationListVM model)
        {
            try
            {
                foreach (var item in model.VksNotificationItems)
                {
                    if (item.NotificationListId >= 0)
                    {
                        var saved = GetById<CaseSessionNotificationList>(item.NotificationListId);
                        saved.CasePersonLinkId = item.CasePersonLinkId.EmptyToNull();
                        saved.NotificationAddressId = item.NotificationAddressId > 0 ? item.NotificationAddressId : null;
                        saved.VksNotificationStateId = item.VksNotificationStateId.EmptyToNull();
                        saved.DateWrt = DateTime.Now;
                        saved.UserId = userContext.UserId;
                    }
                    else
                    {
                        var notification = new CaseSessionNotificationList();
                        notification.NotificationListTypeId = SourceTypeSelectVM.CaseSessionNotificationListNotificationDP;
                        notification.CaseId = model.CaseId;
                        notification.CaseSessionId = model.CaseSessionId;
                        notification.CasePersonId = item.CasePersonId;
                        notification.CasePersonLinkId = item.CasePersonLinkId.EmptyToNull();
                        notification.NotificationAddressId = item.NotificationAddressId > 0 ? item.NotificationAddressId : null;
                        notification.VksNotificationStateId = item.VksNotificationStateId.EmptyToNull();
                        notification.DateWrt = DateTime.Now;
                        notification.UserId = userContext.UserId;
                        repo.Add(notification);
                    }
                }
                repo.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на списък за призоваване CaseSessionId={ model.CaseSessionId }");
                return false;
            }
            return true;
        }
        public IEnumerable<VksNotificationPrintListVM> FillVksNotificationPrintList(VksNotificationPrintFilter filter, int[] caseSessionIds)
        {
            if (filter != null)
            {
                var d = filter.DateFrom.Day;
                var m = filter.DateFrom.Month;
                var y = filter.DateFrom.Year;
                filter.DateFrom = new DateTime(y, m, d);
                filter.DateTo = new DateTime(y, m, DateTime.DaysInMonth(y, m));
            }
            var result = new List<VksNotificationPrintListVM>();
            var caseSessions = repo.AllReadonly<CaseSession>()
                                   .Include(x => x.Case)
                                   .ThenInclude(x => x.Otdelenie)
                                   .Where(x => x.CourtId == userContext.CourtId &&
                                               x.DateExpired == null &&
                                               (filter == null || x.DateFrom >= filter.DateFrom.Date) &&
                                               (filter == null || x.DateFrom.Date <= filter.DateTo.Value)
                                          ).ToList();
            foreach (var caseSession in caseSessions)
            {
                if (caseSessionIds?.Length > 0)
                {
                    if (!caseSessionIds.Contains(caseSession.Id))
                        continue;
                }
                var caseNotifications = repo.AllReadonly<CaseSessionNotificationList>()
                                            .Include(x => x.CasePerson)
                                            .ThenInclude(x => x.PersonRole)
                                            .Include(x => x.NotificationAddress)
                                            .Include(x => x.CasePersonLink)
                                            .ThenInclude(x => x.LinkDirection)
                                            .Where(x => x.CaseId == caseSession.CaseId &&
                                                        x.CaseSessionId == caseSession.Id &&
                                                        x.VksNotificationStateId == 2 &&
                                                        x.NotificationListTypeId == SourceTypeSelectVM.CaseSessionNotificationListNotificationDP &&
                                                        x.DateExpired == null)
                                            .ToList();
                var caseMigration = repo.AllReadonly<CaseMigration>()
                                        //.Where(x => x.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptCase_AfterComplain &&
                                        .Where(x => x.DateExpired == null &&
                                                    x.CaseId == caseSession.CaseId)
                                        .OrderByDescending(x => x.Id)
                                        .FirstOrDefault();
                var item = new VksNotificationPrintListVM();
                if (caseSessionIds?.Length > 0)
                {
                    item.CheckRow = true;
                }
                else
                {
                    item.CheckRow = repo.AllReadonly<VksNotificationPrintList>()
                                        .Where(x => x.CaseSessionId == caseSession.Id)
                                        .Select(x => x.CheckRow)
                                        .FirstOrDefault();
                }
                if (caseMigration != null)
                {
                    var caseFrom = repo.AllReadonly<Case>()
                                      .Include(x => x.CaseType)
                                      .Include(x => x.Court)
                                      .Where(x => x.Id == caseMigration.PriorCaseId)
                                      .FirstOrDefault();
                    item.CaseFromLabel = $"{caseFrom.CaseType.Label} {caseFrom.ShortNumber}/{caseFrom.RegDate.Year} по описа на {caseFrom.Court.Label}";
                }

                item.JudicalCompositionId = caseSession.Case.OtdelenieId ?? 0;
                item.JudicalCompositionLabel = caseSession.Case.Otdelenie?.Label;
                item.Id = caseSession.Id;
                item.CaseId = caseSession.CaseId;
                item.CaseSessionId = caseSession.Id;
                item.CaseLabel = $"{caseSession.Case.RegNumber}";
                var hour = caseSession.DateFrom.Hour.ToString();
                if (caseSession.DateFrom.Minute >= 30)
                    hour += ":30";
                item.SessionTimeLabel = $"На {caseSession.DateFrom.ToString("dd.MM.yyyy")} г. от {hour} ч.";
                if (item.JudicalCompositionId == 0)
                    continue;
                bool forAdd = false;
                foreach (var notification in caseNotifications)
                {
                    var linkLable = String.Empty;
                    if (notification.CasePersonLinkId > 0)
                    {
                        var linkListVM = casePersonLinkService.GetLinkForPerson(notification.CasePersonId ?? 0,
                                                                           false,
                                                                           SourceTypeSelectVM.CaseSessionNotificationListNotificationDP,
                                                                           null);
                        var link = linkListVM.Where(x => x.Id == notification.CasePersonLinkId).FirstOrDefault();
                        SetVksLabelFromTemplate(link);
                        linkLable = link.Label;
                    }
                    var sideName = (!string.IsNullOrEmpty(linkLable) ? linkLable : GetCleanFullName(notification.CasePerson.FullName));
                    if (!string.IsNullOrEmpty(sideName) && notification.NotificationAddress != null)
                        sideName += " ";
                    if (notification.NotificationAddress != null)
                        sideName += nomService.GetFullAddress(notification.NotificationAddress, false, true, true);
                    if (notification.CasePerson?.PersonRole?.RoleKindId == NomenclatureConstants.PersonKinds.LeftSide)
                    {
                        if (!string.IsNullOrEmpty(sideName))
                            item.LeftSide += (string.IsNullOrEmpty(item.LeftSide) ? string.Empty : "; ") + sideName;
                        forAdd = true;
                    }
                    if (notification.CasePerson?.PersonRole?.RoleKindId == NomenclatureConstants.PersonKinds.RightSide)
                    {
                        if (!string.IsNullOrEmpty(sideName))
                            item.RightSide += (string.IsNullOrEmpty(item.RightSide) ? string.Empty : "; ") + sideName;
                        forAdd = true;
                    }

                }
                if (forAdd)
                    result.Add(item);
            }
            return result;
        }
        public int SaveSelectedList(string caseSessionIdsJson, int vksMonth)
        {
            var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
            var caseSessionIds = JsonConvert.DeserializeObject<int[]>(caseSessionIdsJson, dateTimeConverter);
            var vksHeader = repo.AllReadonly<VksNotificationHeader>()
                                .Where(x => x.Month == vksMonth)
                                .FirstOrDefault();
            if (!string.IsNullOrEmpty(vksHeader?.PaperEdition))
            {
                throw new Exception($"За месец {vksMonth} списъкът е отбелязан като публикуван с {vksHeader.PaperEdition}");
            }
            if (vksHeader == null)
            {
                vksHeader = new VksNotificationHeader();
                vksHeader.Month = vksMonth;
                repo.Add(vksHeader);
                repo.SaveChanges();
            }
            var vksNotificationPrintLists = repo.All<VksNotificationPrintList>()
                                                .Where(x => x.VksNotificationHeaderId == vksHeader.Id)
                                                .ToList();
            foreach (var vksNotificationPrintList in vksNotificationPrintLists)
            {
                if (!caseSessionIds.Contains(vksNotificationPrintList.CaseSessionId))
                {
                    if (vksNotificationPrintList.CheckRow)
                    {
                        vksNotificationPrintList.CheckRow = false;
                        vksNotificationPrintList.DateWrt = DateTime.Now;
                        vksNotificationPrintList.UserId = userContext.UserId;
                    }
                }
            }
            foreach (var id in caseSessionIds)
            {
                var vksNotificationPrintList = vksNotificationPrintLists.FirstOrDefault(x => x.CaseSessionId == id);
                if (vksNotificationPrintList != null)
                {
                    vksNotificationPrintList.CheckRow = true;
                    vksNotificationPrintList.DateWrt = DateTime.Now;
                    vksNotificationPrintList.UserId = userContext.UserId;
                }
                else
                {
                    var caseSession = repo.AllReadonly<CaseSession>()
                                          .Where(x => x.Id == id)
                                          .FirstOrDefault();
                    var model = new VksNotificationPrintList();
                    model.CaseSessionId = id;
                    model.CaseId = caseSession.CaseId;
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    model.VksNotificationHeaderId = vksHeader.Id;
                    model.CheckRow = true;
                    repo.Add(model);

                    var notificationPersons = repo.AllReadonly<CaseSessionNotificationList>()
                                                .Where(x => x.CaseSessionId == id &&
                                                            x.VksNotificationStateId == 2 &&
                                                            x.NotificationListTypeId == SourceTypeSelectVM.CaseSessionNotificationListNotificationDP &&
                                                            x.DateExpired == null &&
                                                            x.VksNotificationHeaderId == null)
                                                .ToList();
                    foreach (var notificationPerson in notificationPersons)
                    {
                        notificationPerson.VksNotificationHeaderId = vksHeader.Id;
                        repo.Update(notificationPerson);
                    }
                }
            }
            repo.SaveChanges();
            return vksHeader.Id;
        }
        public List<SelectListItem> GetDDL_NotificationPrintList()
        {
            var result = repo.AllReadonly<VksNotificationHeader>()
                             .OrderByDescending(x => x.Id)
                             .Select(x => new SelectListItem()
                             {
                                 Value = x.Id.ToString(),
                                 Text = $"{x.Month % 100}.{x.Month / 100}"
                             }).ToList();

            result = result.Prepend(new SelectListItem() { Value = "-1", Text = "Изберете" }).ToList();
            return result;
        }
        public async Task<CdnDownloadResult> ReadPrintedFile(int Id)
        {
            CdnItemVM aFile = cdnService.Select(SourceTypeSelectVM.CaseSessionNotificationListNotificationDP, Id.ToString()).FirstOrDefault();
            if (aFile != null)
                return await cdnService.MongoCdn_Download(aFile).ConfigureAwait(false);
            return null;
        }

        public bool SaveVksNotificationPrint(VksNotificationPrintVM model)
        {
            var header = repo.All<VksNotificationHeader>()
                             .Where(x => x.Id == model.VksNotificationHeaderId)
                             .FirstOrDefault();
            header.PaperEdition = model.PaperEdition;
            repo.SaveChanges();
            return true;
        }

        public string GetCleanFullName(string fullName)
        {
            if (fullName.IndexOf("(") > 0)
            {
                return fullName.Substring(0, fullName.IndexOf("("));
            }
            return fullName;
        }
        /// <summary>
        /// Извличане на стринг за уведомления за Връзки по страни, може и втори представляващ
        /// </summary>
        /// <param name="model"></param>
        private void SetVksLabelFromTemplate(CaseNotificationLinkVM model)
        {
            string personX = GetCleanFullName(model.PersonName);
            string roleX = model.PersonRole?.ToLower();
            string personY = GetCleanFullName(model.PersonRelName);
            string roleY = model.PersonRelRole?.ToLower();
            int posX = model.LinkTemplateVks.IndexOf("{X}");
            int posY = model.LinkTemplateVks.IndexOf("{Y}");
            model.isXFirst = true;
            if (posY < posX)
            {
                personX = GetCleanFullName(model.PersonRelName);
                roleX = model.PersonRelRole?.ToLower();
                personY = GetCleanFullName(model.PersonName);
                roleY = model.PersonRole?.ToLower();
                model.isXFirst = false;
            }
            model.Label = model.LinkTemplateVks
                               .Replace("{X}", personX)
                               .Replace("{RoleX}", roleX)
                               .Replace("{Y}", personY)
                               .Replace("{RoleY}", roleY);
            model.LabelWithoutSecondRel = model.Label;
            model.LabelWithoutFirstPerson = model.LinkTemplateVks
                                                 .Replace("{X}", string.Empty)
                                                 .Replace("{RoleX}", string.Empty)
                                                 .Replace("{Y}", personY)
                                                 .Replace("{RoleY}", roleY);
            model.LabelWithoutFirstPerson = model.LabelWithoutFirstPerson.Replace("()", "<br>");

            if (model.LinkDirectionSecondId > 0)
                model.Label += model.SecondLinkTemplateVks
                               .Replace("{Z}", GetCleanFullName(model.PersonSecondRelName))
                               .Replace("{RoleZ}", model.PersonSecondRelRole);
        }
        public string GetPaperEdition(VksNotificationPrintFilter filter)
        {
            var vksMonth = filter.DateFrom.Month + filter.DateFrom.Year * 100;
            return repo.AllReadonly<VksNotificationHeader>()
                                .Where(x => x.Month == vksMonth)
                                .Select(x => x.PaperEdition)
                                .FirstOrDefault();
        }
        public bool IsCaseForCountryPaper(int caseId)
        {
            var caseCase = repo.GetById<Case>(caseId);
            return (caseCase?.CaseGroupId == NomenclatureConstants.CaseGroups.GrajdanskoDelo || caseCase?.CaseGroupId == NomenclatureConstants.CaseGroups.Trade);
        }
        public List<SelectListItem> GetDDL_CasePersonAddress(int casePersonId)
        {
            var addresses = repo.AllReadonly<CasePersonAddress>()
                 .Include(x => x.Address)
                 .Where(x => x.CasePersonId == casePersonId)
                 .ToList();
          var result = addresses.Select(x => new SelectListItem()
                {
                    Value = x.AddressId.ToString(),
                    Text = ((x.ForNotification ?? false) ? " " : "") + nomService.GetFullAddress(x.Address, false, true, true)
                }).ToList();

            if (result.Count == 0)
                result.Insert(0, new SelectListItem() { Text = "Няма данни", Value = "-1" });

            return result.OrderBy(x => x.Text).ToList();
        }
    }
}
