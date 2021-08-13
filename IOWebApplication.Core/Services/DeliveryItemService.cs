using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using IOWebApplication.Infrastructure.Constants;
using Microsoft.AspNetCore.Mvc.Rendering;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Core.Helper;
using Newtonsoft.Json;
using Integration.Epep;
using IOWebApplication.Infrastructure.Data.Models.Documents;

namespace IOWebApplication.Core.Services
{
    public class DeliveryItemService : BaseService, IDeliveryItemService
    {
        private readonly INomenclatureService nomenclatureService;
        private readonly IWorkingDaysService workingDaysService;
        public DeliveryItemService(
            ILogger<DeliveryItemService> _logger,
            IRepository _repo,
            IUserContext _userContext,
            INomenclatureService _nomenclatureService,
            IWorkingDaysService _workingDaysService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            nomenclatureService = _nomenclatureService;
            workingDaysService = _workingDaysService;
        }
        private Expression<Func<DeliveryItem, bool>> IsNotExpired()
        {
            return x => x.DateExpired == null;
        }

        public IQueryable<DeliveryItemVM> DeliveryItemSelect(DeliveryItemFilterVM filter)
        {
            filter.CaseRegNumber = filter.CaseRegNumber.ToShortCaseNumber() ?? filter.CaseRegNumber;
            DateTime dNull = DateTime.Now;
            filter.ResetCourtByType(userContext.CourtId);
            var deliveryOpers = repo.AllReadonly<DeliveryItemOper>();
                                    
            return repo.AllReadonly<DeliveryItem>()
                .Where(x =>
                         (filter.NotificationStateId <= 0 || x.NotificationStateId == filter.NotificationStateId) &&
                         (filter.NotificationTypeId <= 0 || x.NotificationTypeId == filter.NotificationTypeId) &&
                         (filter.LawUnitId <= 0 || x.LawUnitId == filter.LawUnitId) &&
                         (filter.CourtId <= 0 || x.CourtId == filter.CourtId) &&
                         (filter.FromCourtId <= 0 || x.FromCourtId == filter.FromCourtId) &&
                         (filter.FilterType != NomenclatureConstants.DeliveryItemFilterType.FromOther || x.FromCourtId != filter.CourtId) &&
                         (filter.FilterType != NomenclatureConstants.DeliveryItemFilterType.ToOther || x.CourtId != filter.FromCourtId) &&
                         (filter.DateSendFrom == null || ((((x.DateSend ?? deliveryOpers.Where(o => o.DeliveryItemId == x.Id && o.DeliveryOperId == NomenclatureConstants.NotificationState.Send).Max(o => (DateTime?)o.DateOper)) ??
                                     deliveryOpers.Where(o => o.DeliveryItemId == x.Id && o.DeliveryOperId == NomenclatureConstants.NotificationState.Received).Max(o => (DateTime?)o.DateOper)) ??
                                     deliveryOpers.Where(o => o.DeliveryItemId == x.Id && o.DeliveryOperId == NomenclatureConstants.NotificationState.ForDelivery).Max(o => (DateTime?)o.DateOper)) ??
                                     deliveryOpers.Where(o => o.DeliveryItemId == x.Id &&
                                      (o.DeliveryOperId == NomenclatureConstants.DeliveryOper.Visit1 || o.DeliveryOperId == NomenclatureConstants.DeliveryOper.Visit2 || o.DeliveryOperId == NomenclatureConstants.DeliveryOper.Visit3)
                                     ).Min(o => (DateTime?)o.DateOper)) >= (filter.DateSendFrom ?? dNull).Date) &&
                         (filter.DateSendTo == null || (((((x.DateSend ?? deliveryOpers.Where(o => o.DeliveryItemId == x.Id && o.DeliveryOperId == NomenclatureConstants.NotificationState.Send).Max(o => (DateTime?)o.DateOper)) ??
                                     deliveryOpers.Where(o => o.DeliveryItemId == x.Id && o.DeliveryOperId == NomenclatureConstants.NotificationState.Received).Max(o => (DateTime?)o.DateOper)) ??
                                     deliveryOpers.Where(o => o.DeliveryItemId == x.Id && o.DeliveryOperId == NomenclatureConstants.NotificationState.ForDelivery).Max(o => (DateTime?)o.DateOper)) ??
                                     deliveryOpers.Where(o => o.DeliveryItemId == x.Id &&
                                      (o.DeliveryOperId == NomenclatureConstants.DeliveryOper.Visit1 || o.DeliveryOperId == NomenclatureConstants.DeliveryOper.Visit2 || o.DeliveryOperId == NomenclatureConstants.DeliveryOper.Visit3)
                                     ).Min(o => (DateTime?)o.DateOper)) ?? dNull).Date <= (filter.DateSendTo ?? dNull).Date) &&
                         (filter.DateAcceptedFrom == null || (((x.DateAccepted ?? deliveryOpers.Where(o => o.DeliveryItemId == x.Id && o.DeliveryOperId == NomenclatureConstants.NotificationState.Received).Max(o => (DateTime?)o.DateOper)) ??
                                     deliveryOpers.Where(o => o.DeliveryItemId == x.Id && o.DeliveryOperId == NomenclatureConstants.NotificationState.ForDelivery).Max(o => (DateTime?)o.DateOper)) ??
                                     deliveryOpers.Where(o => o.DeliveryItemId == x.Id &&
                                      (o.DeliveryOperId == NomenclatureConstants.DeliveryOper.Visit1 || o.DeliveryOperId == NomenclatureConstants.DeliveryOper.Visit2 || o.DeliveryOperId == NomenclatureConstants.DeliveryOper.Visit3)
                                     ).Min(o => (DateTime?)o.DateOper)) >= (filter.DateAcceptedFrom ?? dNull).Date) &&
                         (filter.DateAcceptedTo == null || ((((x.DateAccepted ?? deliveryOpers.Where(o => o.DeliveryItemId == x.Id && o.DeliveryOperId == NomenclatureConstants.NotificationState.Received).Max(o => (DateTime?)o.DateOper)) ??
                                     deliveryOpers.Where(o => o.DeliveryItemId == x.Id && o.DeliveryOperId == NomenclatureConstants.NotificationState.ForDelivery).Max(o => (DateTime?)o.DateOper)) ??
                                     deliveryOpers.Where(o => o.DeliveryItemId == x.Id &&
                                      (o.DeliveryOperId == NomenclatureConstants.DeliveryOper.Visit1 || o.DeliveryOperId == NomenclatureConstants.DeliveryOper.Visit2 || o.DeliveryOperId == NomenclatureConstants.DeliveryOper.Visit3)
                                     ).Min(o => (DateTime?)o.DateOper)) ?? dNull).Date <= (filter.DateAcceptedTo ?? dNull).Date) &&
                         (string.IsNullOrEmpty(filter.RegNumber) || EF.Functions.ILike(x.RegNumber,filter.RegNumber.ToPaternSearch())) &&
                         (string.IsNullOrEmpty(filter.CaseRegNumber) || 
                             (x.CaseNotificationId != null ?
                               EF.Functions.ILike(x.CaseNotification.Case.RegNumber ?? "", filter.CaseRegNumber.ToPaternSearch()) :
                               x.DocumentNotificationId == null && EF.Functions.ILike(x.CaseInfo ?? "", filter.CaseRegNumber.ToPaternSearch())
                             )
                         ) &&
                         (filter.NotificationDeliveryGroupId <= 0 ||
                          ((x.CaseNotification.NotificationDeliveryGroupId ?? 0) == filter.NotificationDeliveryGroupId) ||
                          ((x.DocumentNotification.NotificationDeliveryGroupId ?? 0) == filter.NotificationDeliveryGroupId)
                         )
                      )
                .Where(IsNotExpired())
                .Select(x => new DeliveryItemVM()
                {
                    Id = x.Id,
                    FromCourtName = x.FromCourt.Label,
                    CourtName = x.Court.Label,
                    LawUnitName = x.LawUnitId == null ? x.CaseNotification.GetNotificationDeliveryGroup.Label : x.LawUnit.FullName,
                    AreaName = x.DeliveryArea == null ? "" : x.DeliveryArea.Description,
                    PersonName = x.PersonName,
                    Address = x.Address == null ? "" : x.Address.FullAddressNotification(),
                    StateName = x.NotificationState == null ? "" : x.NotificationState.Label,
                    RegNumber = x.RegNumber,
                    DateSend = (((x.DateSend ?? deliveryOpers.Where(o => o.DeliveryItemId == x.Id && o.DeliveryOperId == NomenclatureConstants.NotificationState.Send).Max(o => (DateTime?)o.DateOper)) ??
                                     deliveryOpers.Where(o => o.DeliveryItemId == x.Id && o.DeliveryOperId == NomenclatureConstants.NotificationState.Received).Max(o => (DateTime?)o.DateOper)) ??
                                     deliveryOpers.Where(o => o.DeliveryItemId == x.Id && o.DeliveryOperId == NomenclatureConstants.NotificationState.ForDelivery).Max(o => (DateTime?)o.DateOper)) ??
                                     deliveryOpers.Where(o => o.DeliveryItemId == x.Id &&
                                      (o.DeliveryOperId == NomenclatureConstants.DeliveryOper.Visit1 || o.DeliveryOperId == NomenclatureConstants.DeliveryOper.Visit2 || o.DeliveryOperId == NomenclatureConstants.DeliveryOper.Visit3)
                                     ).Min(o => (DateTime?)o.DateOper),
                    DateAccepted = ((x.DateAccepted ?? deliveryOpers.Where(o => o.DeliveryItemId == x.Id && o.DeliveryOperId == NomenclatureConstants.NotificationState.Received).Max(o => (DateTime?)o.DateOper)) ??
                                     deliveryOpers.Where(o => o.DeliveryItemId == x.Id && o.DeliveryOperId == NomenclatureConstants.NotificationState.ForDelivery).Max(o => (DateTime?)o.DateOper))??
                                     deliveryOpers.Where(o => o.DeliveryItemId == x.Id && 
                                      (o.DeliveryOperId == NomenclatureConstants.DeliveryOper.Visit1 || o.DeliveryOperId == NomenclatureConstants.DeliveryOper.Visit2 || o.DeliveryOperId == NomenclatureConstants.DeliveryOper.Visit3)
                                     ).Min(o => (DateTime?)o.DateOper)
                                                       ,
                    // DeliveryDate = deliveryOpers.Where(o => o.DeliveryItemId == x.Id).Max(o => (DateTime?)o.DateOper),
                    DeliveryDate = deliveryOpers.Where(o => o.DeliveryItemId == x.Id).OrderByDescending(r => r.Id).Select(o => (DateTime?)o.DateOper).FirstOrDefault(),
                    CaseInfo = x.CaseInfo,
                    NotificationDeliveryGroupId = x.CaseNotification == null ? 0 : x.CaseNotification.NotificationDeliveryGroupId ?? 0,
                    NotificationType = x.NotificationType.Label ?? "",
                    CaseNotificationId = x.CaseNotificationId,
                    DocumentNotificationId = x.DocumentNotificationId
                })
                .AsQueryable();
        }
        public List<NotificationState> DeliveryItemTransNotificationState(int toNotificationStateId)
        {
            int[] states = new int[] { NomenclatureConstants.NotificationState.Ready, 0, 0 };
            if (toNotificationStateId == NomenclatureConstants.NotificationState.Received || 
                toNotificationStateId == NomenclatureConstants.NotificationState.ForDelivery
               )
                states[1] = NomenclatureConstants.NotificationState.Send;
            if (toNotificationStateId == NomenclatureConstants.NotificationState.ForDelivery)
                states[2] = NomenclatureConstants.NotificationState.Received;
            return repo.AllReadonly<NotificationState>()
                       .Where(x => x.IsActive && states.Contains(x.Id))
                       .ToList();
        }

        public IQueryable<DeliveryItemVM> DeliveryItemTransSelect(DeliveryItemTransFilterVM filter, bool allFor)
        {
            DateTime dNull = NullDateStart();
            DateTime dNullEnd = NullDateEnd();
            int courtId = 0;
            int fromCourtId = 0;
            int lawUnitId = 0;
            if (filter.ToNotificationStateId == NomenclatureConstants.NotificationState.Send)
            {
                fromCourtId = userContext.CourtId;
                if (!allFor)
                    courtId = filter.ForId;
            }
            if (filter.ToNotificationStateId == NomenclatureConstants.NotificationState.ForDelivery)
            {
                courtId = userContext.CourtId;
                if (!allFor)
                    lawUnitId = filter.ForId;
            }
            if (filter.ToNotificationStateId == NomenclatureConstants.NotificationState.Received)
            {
                courtId = userContext.CourtId;
                if (!allFor)
                    fromCourtId = filter.ForId;
            }

            var opers = repo.AllReadonly<DeliveryItemOper>()
                            .Where(x => x.NotificationStateId == filter.NotificationStateId)
                            .AsQueryable();
            var deliveryItems = repo.AllReadonly<DeliveryItem>();
            if (filter.NotificationStateId == NomenclatureConstants.NotificationState.NoDeliveryArea ||
                filter.NotificationStateId == NomenclatureConstants.NotificationState.AllForReceived)
            {
                deliveryItems = deliveryItems.Where(x => (x.NotificationStateId == NomenclatureConstants.NotificationState.Ready ||
                                                          x.NotificationStateId == NomenclatureConstants.NotificationState.Send) &&
                                                          (filter.NotificationStateId == NomenclatureConstants.NotificationState.AllForReceived ||
                                                          x.DeliveryAreaId == null));
            }
            else
            {
                deliveryItems = deliveryItems.Where(x => x.NotificationStateId == filter.NotificationStateId);
            }
            return deliveryItems.Where(x =>
                         (courtId <= 0 || x.CourtId == courtId) &&
                         (fromCourtId <= 0 || x.FromCourtId == fromCourtId) &&
                         (lawUnitId == -2 ? x.LawUnitId == null : (lawUnitId <= 0 || x.LawUnitId == lawUnitId)) &&
                         (filter.DateFrom == null || opers.Where(op => op.DeliveryItemId == x.Id && op.DateOper >= (filter.DateFrom ?? dNull).Date).Any()) &&
                         (filter.DateTo == null || opers.Where(op => op.DeliveryItemId == x.Id && op.DateOper.Date <= (filter.DateTo ?? dNullEnd).Date).Any()) &&
                         (filter.RegDateFrom == null || x.RegDate >= (filter.RegDateFrom ?? dNull).Date) &&
                         (filter.RegDateTo == null || x.RegDate <= (filter.RegDateTo ?? dNullEnd).Date) &&
                         (filter.NotificationTypeId <= 0 || 
                          x.CaseNotification.NotificationTypeId == filter.NotificationTypeId || 
                          x.DocumentNotification.NotificationTypeId == filter.NotificationTypeId)
                      )
                .Where(IsNotExpired())
                .Select(x => new DeliveryItemVM()
                {
                    Id = x.Id,
                    FromCourtName = x.FromCourt.Label,
                    CourtName = x.Court.Label,
                    LawUnitName = x.LawUnit == null ? "" : x.LawUnit.FullName,
                    AreaName = x.DeliveryArea == null ? "" : x.DeliveryArea.Description,
                    PersonName = x.PersonName,
                    Address = x.Address == null ? "" : x.Address.FullAddressNotification(),
                    StateName = x.NotificationState == null ? "" : x.NotificationState.Label,
                    RegNumber = x.RegNumber,
                    DateSend = x.DateSend,
                    DateAccepted = x.DateAccepted,
                    DeliveryDate = x.DeliveryDate,
                    CourtId = x.CourtId,
                    FromCourtId = x.FromCourtId,
                    LawUnitId = x.LawUnitId ?? 0,
                    DateReady = opers.Where(op => op.DeliveryItemId == x.Id).OrderBy(r => r.DateOper).Select(op => op.DateOper).LastOrDefault(),
                    CheckRow = false,
                    CaseInfo = x.CaseInfo,
                    DeliveryAreaId = x.DeliveryAreaId,
                    CheckRowOrder = "1Z" + x.RegNumber,
                    NotificationType = x.CaseNotification.NotificationType.Label ?? (x.DocumentNotification.NotificationType.Label ?? ""),
                    CaseNotificationId = x.CaseNotificationId,
                    DocumentNotificationId = x.DocumentNotificationId
                })
                .AsQueryable();
        }
        public IQueryable<DeliveryItemVM> DeliveryItemChangeLawUnitSelect(DeliveryItemChangeLawUnitVM filterData, int[] newLawUnitId)
        {
            int[] states = NotificationStateEnd();
            var deliveries = repo.AllReadonly<DeliveryItem>()
                .Where(IsNotExpired())
                .Where(x => x.CourtId == filterData.CourtId)
                .Where(x => filterData.NotificationTypeId <= 0 || x.NotificationTypeId == filterData.NotificationTypeId)
                .Where(x => x.CaseNotification == null || x.CaseNotification.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.WithSummons);
            if (filterData.NotificationStateId <= 0)
                deliveries = deliveries.Where(x => !states.Contains(x.NotificationStateId));
            else
                deliveries = deliveries.Where(x => x.NotificationStateId == filterData.NotificationStateId);
            switch (filterData.LawUnitId)
            {
                case -1: break;
                case -2:
                    deliveries = deliveries.Where(x => x.LawUnitId != null && !newLawUnitId.Contains(x.LawUnitId == null ? 0 : (x.LawUnitId ?? 0)));
                    break;
                case 0:
                    deliveries = deliveries.Where(x => x.LawUnitId == null);
                    break;
                default:
                    deliveries = deliveries.Where(x => filterData.LawUnitId == x.LawUnitId);
                    break;
            };
            switch (filterData.DeliveryAreaId)
            {
                case -1: break;
                case 0:
                    deliveries = deliveries.Where(x => x.DeliveryAreaId == null);
                    break;
                default:
                    deliveries = deliveries.Where(x => x.DeliveryAreaId == filterData.DeliveryAreaId);
                    break;
            };

            var result = deliveries.Select(x => new DeliveryItemVM()
            {
                Id = x.Id,
                FromCourtName = x.FromCourt.Label,
                CourtName = x.Court.Label,
                LawUnitName = x.LawUnit == null ? "" : x.LawUnit.FullName,
                AreaName = x.DeliveryArea == null ? "" : x.DeliveryArea.Description,
                PersonName = x.PersonName,
                Address = x.Address == null ? "" : x.Address.FullAddressNotification(),
                StateName = x.NotificationState == null ? "" : x.NotificationState.Label,
                RegNumber = x.RegNumber,
                DateSend = x.DateSend,
                DateAccepted = x.DateAccepted,
                DeliveryDate = x.DeliveryDate,
                CourtId = x.CourtId,
                FromCourtId = x.FromCourtId,
                LawUnitId = x.LawUnitId,
                DeliveryAreaId = x.DeliveryAreaId,
                DateReady = null,
                CheckRow = false,
                NotificationType = x.CaseNotification.NotificationType.Label ?? (x.DocumentNotification.NotificationType.Label ?? ""),
                CaseNotificationId = x.CaseNotificationId,
                DocumentNotificationId = x.DocumentNotificationId
            })
                .AsQueryable();
            return result;
        }
        public List<Select2ItemVM> DeliveryItemTransForIdDDL(DeliveryItemTransFilterVM filter)
        {
            var list = DeliveryItemTransSelect(filter, true).ToList();
            switch (filter.ToNotificationStateId)
            {
                case NomenclatureConstants.NotificationState.Send:
                    var toCourts = list
                              .GroupBy(x => x.CourtId)
                              .Select(gr => new Select2ItemVM()
                              {
                                  Text = gr.Max(x => x.CourtName) + "  " + gr.Count().ToString(),
                                  Id = gr.Max(x => x.CourtId)
                              })
                              .OrderBy(x => x.Text)
                              .ToList() ?? new List<Select2ItemVM>();
                    toCourts.Add(new Select2ItemVM()
                    {
                        Id = 0,
                        Text = $"Всички {list.Count}"
                    });
                    return toCourts;
                case NomenclatureConstants.NotificationState.Received:
                    var fromCourts = list
                              .GroupBy(x => x.FromCourtId)
                              .Select(gr => new Select2ItemVM()
                              {
                                  Text = gr.Max(x => x.FromCourtName) + "  " + gr.Count().ToString(),
                                  Id = gr.Max(x => x.FromCourtId)
                              })
                              .OrderBy(x => x.Text)
                              .ToList() ?? new List<Select2ItemVM>();
                    fromCourts.Add(new Select2ItemVM()
                    {
                        Id = 0,
                        Text = $"Всички {list.Count}"
                    });
                    return fromCourts;
                case NomenclatureConstants.NotificationState.ForDelivery:
                    var lawUnits = list
                              .GroupBy(x => x.LawUnitId)
                              .Select(gr => new Select2ItemVM()
                              {
                                  Text = (gr.First().LawUnitId > 0 ? gr.First().LawUnitName : " БЕЗ ИЗБРАН ПРИЗОВКАР") + "  " + gr.Count().ToString(),
                                  Id = (gr.First().LawUnitId >0 ? gr.First().LawUnitId : -2) ?? 0
                              })
                              .OrderBy(x => x.Text)
                              .ToList() ?? new List<Select2ItemVM>();
                    return lawUnits;
            }
            return new List<Select2ItemVM>();
        }
        public IQueryable<DeliveryItemRecieveVM> getRecived(int id)
        {
            return repo.AllReadonly<DeliveryItem>()
                .Where(x => x.Id == id)
                .Where(IsNotExpired())
                .Select(x => new DeliveryItemRecieveVM()
                {
                    Id = x.Id,
                    FromCourtName = x.FromCourt.Label,
                    LawUnitName = x.LawUnit == null ? "" : x.LawUnit.FullName,
                    AreaName = x.DeliveryArea == null ? "" : x.DeliveryArea.Description,
                    PersonName = x.PersonName,
                    Address = x.Address == null ? "" : x.Address.FullAddressNotification(),
                    RegNumber = x.RegNumber
                })
                .AsQueryable();
        }
        public DeliveryItem GetDeliveryItemByRegNumber(string regNum)
        {
            return repo.AllReadonly<DeliveryItem>()
                .Where(x => x.RegNumber == regNum)
                .FirstOrDefault();
        }
        public DeliveryItem GetDeliveryItemByCaseNotificationId(int notificationId)
        {
            return repo.AllReadonly<DeliveryItem>()
                .Where(x => x.CaseNotificationId == notificationId)
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();
        }
        public DeliveryItem GetDeliveryItemByDocumentNotificationId(int notificationId)
        {
            return repo.AllReadonly<DeliveryItem>()
                .Where(x => x.DocumentNotificationId == notificationId)
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();
        }
        public string GetNotificationInfo(int notificationId)
        {
            return repo.AllReadonly<CaseNotification>()
                .Where(x => x.Id == notificationId)
                .Select(x => x.RegNumber + " / " + x.RegDate.ToString(FormattingConstant.NormalDateFormat))
                .FirstOrDefault() ?? "";
        }
        public string GetNotificationInfoByDeliveryItemId(int deliveryItemId)
        {
            var deliveryItem = repo.AllReadonly<DeliveryItem>()
                                   .Where(x => x.Id == deliveryItemId)
                                   .FirstOrDefault();
            return deliveryItem?.CaseNotificationId != null ? GetNotificationInfo(deliveryItem.CaseNotificationId ?? 0) : "";
        }

        public DeliveryItemOper CreateDeliveryItemOper(DeliveryItem deliveryItem, int deliverOperId)
        {
            int? operId = repo.AllReadonly<DeliveryOper>()
                              .Where(x => x.Id == deliverOperId)
                              .Select(x => (int?)x.Id)
                              .FirstOrDefault();
            if (operId == null)
                return null;
            DeliveryItemOper oper = new DeliveryItemOper();
            oper.CourtId = userContext.CourtId;
            oper.DeliveryOperId = deliverOperId;
            oper.DateOper = DateTime.Now;
            oper.NotificationStateId = deliveryItem.NotificationStateId;
            oper.DeliveryAreaId = deliveryItem.DeliveryAreaId;
            oper.LawUnitId = deliveryItem.LawUnitId;
            oper.DateWrt = DateTime.Now;
            oper.UserId = userContext.UserId;
            oper.DeliveryItemId = deliveryItem.Id;
            oper.DeliveryInfo = deliveryItem.DeliveryInfo;
            repo.Add<DeliveryItemOper>(oper);
            deliveryItem.DeliveryItemOpers = deliveryItem.DeliveryItemOpers ?? new HashSet<DeliveryItemOper>();
            deliveryItem.DeliveryItemOpers.Add(oper);
            return oper;
        }

        public bool DeliveryItemSaveDataAddReceived(DeliveryItem model)
        {
            try
            {
                if (model.DeliveryAreaId <= 0)
                    model.DeliveryAreaId = null;
                if (model.LawUnitId <= 0)
                    model.LawUnitId = null;


                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<DeliveryItem>(model.Id);
                    saved.FromCourtId = model.FromCourtId;
                    saved.CourtId = userContext.CourtId;
                    saved.DateSend = model.DateSend;
                    saved.DateAccepted = model.DateAccepted;
                    saved.DeliveryDate = model.DeliveryDate;
                    saved.ReturnDate = model.ReturnDate;
                    saved.RegNumber = model.RegNumber;
                    saved.RegDate = model.RegDate;
                    saved.CaseNotificationId = model.CaseNotificationId;
                    saved.DeliveryAreaId = model.DeliveryAreaId;
                    saved.NotificationStateId = model.NotificationStateId;
                    saved.LawUnitId = model.LawUnitId;
                    saved.PersonName = model.PersonName;
                    saved.AddressId = model.AddressId;
                    saved.CaseInfo = model.CaseInfo;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    nomenclatureService.SetFullAddress(saved.Address);
                    CreateDeliveryItemOper(saved, model.NotificationStateId);
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    model.NotificationStateId = NomenclatureConstants.NotificationState.Received;
                    model.CourtId = userContext.CourtId;
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    nomenclatureService.SetFullAddress(model.Address);
                    CreateDeliveryItemOper(model, model.NotificationStateId);

                    repo.Add<DeliveryItem>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на DeliveryItem с Id={ model.Id } DeliveryItemSaveDataAddReceived");
                return false;
            }
        }
        public bool SaveTrans(int[] deliveryItemIds, int notificationStateId, int deliverOperId)
        {
            foreach (int id in deliveryItemIds)
            {
                var deliveryItem = repo.GetById<DeliveryItem>(id);
                if (deliveryItem == null)
                {
                    return false;
                }
                deliveryItem.NotificationStateId = notificationStateId;
                DeliveryItemOper oper = CreateDeliveryItemOper(deliveryItem, deliverOperId);
                UpdateOperToNotification(deliveryItem, oper);
                repo.Update(deliveryItem);
            }
            repo.SaveChanges();
            return true;
        }
        public bool SaveChangeLawUnit(int[] deliveryItemIds, DeliveryItemChangeLawUnitVM filterData)
        {
            foreach (int id in deliveryItemIds)
            {
                var deliveryItem = repo.GetById<DeliveryItem>(id);
                if (deliveryItem == null)
                {
                    return false;
                }
                deliveryItem.CourtId = filterData.NewCourtId;
                
                if (filterData.CourtId != filterData.NewCourtId || filterData.NewLawUnitId != -1)
                    deliveryItem.LawUnitId = filterData.NewLawUnitId;
                if (deliveryItem.LawUnitId == -1)
                    deliveryItem.LawUnitId = null;
                
                if (filterData.CourtId != filterData.NewCourtId || filterData.NewDeliveryAreaId != -1)
                    deliveryItem.DeliveryAreaId = filterData.NewDeliveryAreaId;
                if (deliveryItem.DeliveryAreaId == -1)
                    deliveryItem.DeliveryAreaId = null;

                if (deliveryItem.CaseNotification != null)
                {
                    deliveryItem.CaseNotification.LawUnitId = deliveryItem.LawUnitId;
                    deliveryItem.CaseNotification.DeliveryAreaId = deliveryItem.DeliveryAreaId;
                    deliveryItem.CaseNotification.ToCourtId = deliveryItem.CourtId;
                }
                repo.Update(deliveryItem);
            }
            repo.SaveChanges();
            return true;
        }

        public DeliveryItemRecieveVM SaveRecieved(string regNumber, bool saveIfErr, out string messageErr)
        {
            DeliveryItemRecieveVM deliveryVM = null;
            messageErr = "";
            var deliveryItem = repo.AllReadonly<DeliveryItem>()
                       .Where(x => (x.RegNumber == regNumber))
                       .Include(x => x.FromCourt)
                       .Include(x => x.Address)
                       .Include(x => x.DeliveryArea)
                       .FirstOrDefault();

            if (deliveryItem != null)
            {
                LawUnit lawUnit = null;
                if (deliveryItem.NotificationStateId != NomenclatureConstants.NotificationState.Send)
                    messageErr = $" Призовка с номер {regNumber} не със статус изпратена.";
                if (deliveryItem.CourtId != userContext.CourtId)
                    messageErr += $" Призовка с номер {regNumber} не изпратена за този съд.";

                if (deliveryItem.DeliveryArea != null)
                    lawUnit = repo.GetById<LawUnit>(deliveryItem.DeliveryArea.LawUnitId);
                deliveryVM = new DeliveryItemRecieveVM()
                {
                    FromCourtName = deliveryItem.FromCourt.Label,
                    RegNumber = deliveryItem.RegNumber,
                    AreaName = deliveryItem.DeliveryArea?.Description,
                    LawUnitName = lawUnit?.FullName ?? "",
                    PersonName = deliveryItem.PersonName,
                    Address = deliveryItem.Address?.FullAddress ?? ""
                };
                if (saveIfErr || string.IsNullOrEmpty(messageErr))
                {
                    deliveryItem.NotificationStateId = NomenclatureConstants.NotificationState.Received;
                    deliveryItem.DateAccepted = DateTime.Now;
                    deliveryItem.DateWrt = DateTime.Now;
                    deliveryItem.UserId = userContext.UserId;
                    repo.Update(deliveryItem);
                    repo.SaveChanges();
                }
            }
            else
            {
                messageErr = $"Няма призовка с номер {regNumber}";
            }
            return deliveryVM;
        }

        public IQueryable<DeliveryItem> GetReceivedForToday(string userId, DateTime forDate)
        {
            DateTime dNull = forDate.AddDays(-10);
            return repo.AllReadonly<DeliveryItem>()
                       .Where(x => x.UserId == userId && (x.DateAccepted ?? dNull).Date == forDate.Date && x.DateWrt.Date == forDate.Date)
                       .Where(IsNotExpired())
                       .Include(x => x.FromCourt)
                       .Include(x => x.Address)
                       .Include(x => x.DeliveryArea)
                       .ThenInclude(da => da.LawUnit)
                       .AsQueryable();
        }
        public IQueryable<DeliveryItemRecieveVM> GetCheckedForToday(string userId, DateTime forDate)
        {
            DateTime dNull = forDate.AddDays(-10);
            return repo.AllReadonly<DeliveryItem>()
                       .Where(x => x.UserId == userId && (x.DateAccepted ?? dNull).Date == forDate.Date && x.DateWrt.Date == forDate.Date)
                       .Where(IsNotExpired())
                       .Select(deliveryItem => new DeliveryItemRecieveVM()
                       {
                           FromCourtName = deliveryItem.FromCourt.Label,
                           RegNumber = deliveryItem.RegNumber,
                           AreaName = deliveryItem.DeliveryArea == null ? "" : deliveryItem.DeliveryArea.Description,
                           LawUnitName = deliveryItem.DeliveryArea == null ? "" : (deliveryItem.DeliveryArea.LawUnit == null ? "" : deliveryItem.DeliveryArea.LawUnit.FullName),
                           PersonName = deliveryItem.PersonName,
                           Address = deliveryItem.Address == null ? "" : deliveryItem.Address.FullAddress
                       });
        }
        public bool DeliveryItemSaveArea(int id, int courtId, int? deliveryAreaId, int? lawUnitId)
        {
            try
            {
                var saved = repo.GetById<DeliveryItem>(id);
                if (saved == null)
                    return false;
                saved.CourtId = courtId;
                if (deliveryAreaId < 0)
                    saved.DeliveryAreaId = null;
                else
                    saved.DeliveryAreaId = deliveryAreaId;
                if (lawUnitId < 0)
                    saved.LawUnitId = null;
                else
                    saved.LawUnitId = lawUnitId;

                repo.Update(saved);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на районииране DeliveryItemId={ id }");
                return false;
            }
        }
        public DeliveryItem getDeliveryItem(int id)
        {
            return repo.AllReadonly<DeliveryItem>()
                      .Where(x => x.Id == id)
                      .Include(x => x.FromCourt)
                      .Include(x => x.Address)
                      .FirstOrDefault();
        }
        public DeliveryItem getDeliveryItemWithNotification(int id)
        {
            return repo.AllReadonly<DeliveryItem>()
                      .Where(x => x.Id == id)
                      .Include(x => x.FromCourt)
                      .Include(x => x.Address)
                      .Include(x => x.CaseNotification)
                      .FirstOrDefault();
        }
        public bool DeliveryItemSaveOper(DeliveryItemOperVM model)
        {
            try
            {
                if (model.DeliveryItemId > 0)
                {
                    //Update
                    var saved = repo.GetById<DeliveryItem>(model.DeliveryItemId);
                    saved.NotificationStateId = model.NotificationStateId;
                    saved.DeliveryInfo = model.DeliveryInfo;
                    DeliveryItemOper oper = CreateDeliveryItemOper(saved, model.DeliveryOperId);
                    if (oper != null)
                    {
                        oper.DateOper = model.DateOper ?? DateTime.Now;
                        oper.DeliveryReasonId = model.DeliveryReasonId;
                    }
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    UpdateOperToNotification(saved, oper);
                    repo.Update(saved);
                    repo.SaveChanges();
                    model.Id = oper.Id;
                }
                else
                {
                    //Insert
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на операция за разнос към DeliveryItem Id={ model.DeliveryItemId }");
                return false;
            }
        }
        private bool UpdateOperToDocumentNotification(DeliveryItem deliveryItem, DeliveryItemOper deliveryItemOper)
        {
            var documentNotification = repo.GetById<DocumentNotification>(deliveryItem.DocumentNotificationId);
            documentNotification.NotificationStateId = deliveryItem.NotificationStateId;
            documentNotification.DeliveryReasonId = deliveryItemOper.DeliveryReasonId;
            if (deliveryItem.NotificationStateId == NomenclatureConstants.NotificationState.Received)
                documentNotification.DateAccepted = deliveryItemOper.DateOper;
            if (deliveryItem.NotificationStateId == NomenclatureConstants.NotificationState.Send)
                documentNotification.DateSend = deliveryItemOper.DateOper;

            if (
                (deliveryItem.NotificationStateId == NomenclatureConstants.NotificationState.Delivered) ||
                (deliveryItem.NotificationStateId == NomenclatureConstants.NotificationState.Delivered47) ||
                (deliveryItem.NotificationStateId == NomenclatureConstants.NotificationState.Delivered50) ||
                (deliveryItem.NotificationStateId == NomenclatureConstants.NotificationState.Delivered51)
               )
                documentNotification.DeliveryDate = deliveryItemOper.DateOper;
            var deliveryItemOperL = repo.AllReadonly<DeliveryItemOper>()
                                        .Where(x => x.DeliveryItemId == deliveryItem.Id &&
                                                    x.DeliveryOperId >= deliveryItemOper.DeliveryOperId)
                                        .OrderBy(x => x.DeliveryOperId)
                                        .ThenBy(x => x.Id)
                                        .LastOrDefault();
            if (deliveryItemOperL == null || deliveryItemOperL.DeliveryOperId == deliveryItemOper.DeliveryOperId)
            {
                documentNotification.DeliveryOperId = deliveryItemOper.DeliveryOperId;
                documentNotification.DeliveryInfo = deliveryItemOper.DeliveryInfo;
            }
            else
            {
                documentNotification.DeliveryOperId = deliveryItemOperL.DeliveryOperId;
                documentNotification.DeliveryInfo = deliveryItemOperL.DeliveryInfo;
            }
            return true;
        }
        private bool UpdateOperToNotification(DeliveryItem deliveryItem, DeliveryItemOper deliveryItemOper)
        {
            if (deliveryItem.DocumentNotificationId != null)
                return UpdateOperToDocumentNotification(deliveryItem, deliveryItemOper);
            if (deliveryItem.CaseNotificationId == null)
                return false;
            var caseNotification = repo.GetById<CaseNotification>(deliveryItem.CaseNotificationId);
            caseNotification.NotificationStateId = deliveryItem.NotificationStateId;
            caseNotification.DeliveryReasonId = deliveryItemOper.DeliveryReasonId;
            if (deliveryItem.NotificationStateId == NomenclatureConstants.NotificationState.Received)
                caseNotification.DateAccepted = deliveryItemOper.DateOper;
            if (deliveryItem.NotificationStateId == NomenclatureConstants.NotificationState.Send)
                caseNotification.DateSend = deliveryItemOper.DateOper;

            if (
                (deliveryItem.NotificationStateId == NomenclatureConstants.NotificationState.Delivered) ||
                (deliveryItem.NotificationStateId == NomenclatureConstants.NotificationState.Delivered47) ||
                (deliveryItem.NotificationStateId == NomenclatureConstants.NotificationState.Delivered50) ||
                (deliveryItem.NotificationStateId == NomenclatureConstants.NotificationState.Delivered51)
               )
                caseNotification.DeliveryDate = deliveryItemOper.DateOper;
            var deliveryItemOperL = repo.AllReadonly<DeliveryItemOper>()
                                        .Where(x => x.DeliveryItemId == deliveryItem.Id &&
                                                    x.DeliveryOperId >= deliveryItemOper.DeliveryOperId)
                                        .OrderBy(x => x.DeliveryOperId)
                                        .ThenBy(x => x.Id)
                                        .LastOrDefault();
            if (deliveryItemOperL == null || deliveryItemOperL.DeliveryOperId == deliveryItemOper.DeliveryOperId)
            {
                caseNotification.DeliveryOperId = deliveryItemOper.DeliveryOperId;
                caseNotification.DeliveryInfo = deliveryItemOper.DeliveryInfo;
            }
            else
            {
                caseNotification.DeliveryOperId = deliveryItemOperL.DeliveryOperId;
                caseNotification.DeliveryInfo = deliveryItemOperL.DeliveryInfo;
            }
            return true;
        }
        public IQueryable<DeliveryItemReportVM> GetDeliveryItemOutReport(DeliveryItemListVM filter, bool forCurrentCourt)
        {
            int[] states = NotificationStateEndAndVisited();
            return repo.AllReadonly<DeliveryItem>()
                       .Where(x => //states.Contains(x.NotificationStateId) &&
                                   (userContext.CourtId == x.CourtId) &&
                                   (filter.FromCourtId <= 0 || filter.FromCourtId == x.FromCourtId) &&
                                   (filter.LawUnitId <= 0 || filter.LawUnitId == x.LawUnitId) &&
                                   (filter.CaseGroupId <= 0 || filter.CaseGroupId == x.CaseGroupId) &&
                                   (filter.CaseTypeId <= 0 || filter.CaseTypeId == x.CaseTypeId) &&
                                   (filter.DateFrom == null || filter.DateFrom <= ((x.DateAccepted ?? x.DateSend) ?? x.RegDate)) &&
                                   (filter.DateTo == null || filter.DateTo >= ((x.DateAccepted ?? x.DateSend) ?? x.RegDate)) &&
                                   (forCurrentCourt || userContext.CourtId != x.FromCourtId)
                              )
                       .Where(IsNotExpired())
                       .Select(x => new DeliveryItemReportVM()
                       {
                           RegNumber = x.RegNumber,
                           NotificationTypeLabel = (x.NotificationType == null ? "" : x.NotificationType.Label) + " " +
                                                   (x.CaseNotification.HaveАppendix == true ? " +прил.": "") + " " +
                                                   (x.CaseNotification.CaseSession.DateTo != null ? x.CaseNotification.CaseSession.DateTo.Value.ToString(FormattingConstant.NormalDateFormat) : "")
                                                   //+" "+ ((x.DateAccepted ?? x.DateSend) ?? x.RegDate).Value.ToString(FormattingConstant.NormalDateFormat)
                                                   ,
                           CaseInfo = x.CaseInfo ?? "",
                           PersonName = x.PersonName,
                           Address = x.Address == null ? "" : x.Address.FullAddressNotification(),
                           DeliveryInfo = x.DeliveryInfo,
                           ReturnDate = x.ReturnDate,
                           NotificationState = states.Contains(x.NotificationStateId)? x.NotificationState.Label ?? "" :"",
                           DeliveryDate = x.DeliveryDate,
                           DeliveryItemOper = x.DeliveryItemOpers.OrderBy(o => o.DeliveryOperId).ThenBy(o => o.Id).LastOrDefault()
                       })
                       .AsQueryable();
        }
        public int[] NotificationStateEnd()
        {
            return new int[]
            {
               NomenclatureConstants.NotificationState.Delivered,
               NomenclatureConstants.NotificationState.Delivered47,
               NomenclatureConstants.NotificationState.Delivered50,
               NomenclatureConstants.NotificationState.Delivered51,
               NomenclatureConstants.NotificationState.UnDelivered
            };
        }
        public int[] NotificationStateEndAndVisited()
        {
            return new int[]
            {
               NomenclatureConstants.NotificationState.Delivered,
               NomenclatureConstants.NotificationState.Delivered47,
               NomenclatureConstants.NotificationState.Delivered50,
               NomenclatureConstants.NotificationState.Delivered51,
               NomenclatureConstants.NotificationState.UnDelivered,
               NomenclatureConstants.NotificationState.Visited
            };
        }
        public IQueryable<DeliveryItemReportResultVM> GetDeliveryItemReportResult(DeliveryItemListVM filter)
        {
            DateTime nullDate = new DateTime(2000, 1, 1);
            int[] states = NotificationStateEndAndVisited();
            return repo.AllReadonly<DeliveryItem>()
                       .Where(x => states.Contains(x.NotificationStateId) &&
                                   (userContext.CourtId == x.CourtId) &&
                                   (filter.FromCourtId <= 0 || filter.FromCourtId == x.FromCourtId) &&
                                   (filter.LawUnitId <= 0 || filter.LawUnitId == x.LawUnitId) &&
                                   (filter.CaseGroupId <= 0 || filter.CaseGroupId == x.CaseGroupId) &&
                                   (filter.CaseTypeId <= 0 || filter.CaseTypeId == x.CaseTypeId) &&
                                   (filter.DateFrom == null || filter.DateFrom <= ((x.DateAccepted ?? x.DateSend) ?? x.RegDate)) &&
                                   (filter.DateTo == null || filter.DateTo >= ((x.DateAccepted ?? x.DateSend) ?? x.RegDate)) &&
                                   (x.PersonName.Contains(filter.PersonName)) 
                              )
                       .Where(IsNotExpired())
                       .Select(x => new DeliveryItemReportResultVM()
                       {
                           CaseGroupLabel = x.CaseGroup.Label,
                           CaseInfo = x.CaseInfo ?? "",
                           FromCourtName = x.FromCourt.Label,
                           DateFrom = x.DateSend,
                           DateFromStr = ((x.DateAccepted ?? x.DateSend) ?? x.RegDate) != null ? ((x.DateAccepted ?? x.DateSend) ?? x.RegDate).Value.ToString(FormattingConstant.NormalDateFormatHHMM) : "",
                           LawUnitName = x.LawUnit.FullName,
                           DocumentType = (x.NotificationType == null ? "" : x.NotificationType.Label) + 
                                          (x.HtmlTemplate == null ? "" : ", " + x.HtmlTemplate.Label) + " " +
                                          ((x.CaseNotification.CaseSession.SessionType.Label ?? "") != "" ? " от " : " ")+
                                           (x.CaseNotification.CaseSession.SessionType.Label ?? "").Trim() + " " +
                                          (x.CaseNotification.CaseSession.DateFrom != null ? x.CaseNotification.CaseSession.DateFrom.ToString(FormattingConstant.NormalDateFormat): ""),
                           HtmlTemplateName = x.HtmlTemplate.Label,
                           StateName = x.NotificationState == null ? "" : x.NotificationState.Label,
                           DateResult = x.DeliveryDate,
                           DateResultStr = x.ReturnDate == null ? "" : x.ReturnDate.Value.ToString(FormattingConstant.NormalDateFormatHHMM),
                           ReasonReturn = x.NotificationStateId != NomenclatureConstants.NotificationState.Delivered ?
                                          (x.DeliveryItemOpers.OrderBy(o => o.DeliveryOperId).ThenBy(o => o.Id).LastOrDefault().DeliveryReason.Label ?? "")+" "+x.DeliveryInfo :
                                          "",
                           PersonName = x.PersonName,
                           Address = x.Address == null ? "" : x.Address.FullAddressNotification()
                       })
                       .AsQueryable();
        }
        public IQueryable<DeliveryItemReturnNewVM> GetDeliveryItemReportResultNew(DeliveryItemListVM filter)
        {
            DateTime nullDate = new DateTime(2000, 1, 1);
            int[] states = NotificationStateEndAndVisited();
            var deliveryItemOper = repo.AllReadonly<DeliveryItemOper>()
                                       .Where(x => x.DeliveryOperId == NomenclatureConstants.DeliveryOper.ToLawUnit);
            
            return repo.AllReadonly<DeliveryItem>()
                       .Include(x => x.NotificationState)
                       .Where(x => states.Contains(x.NotificationStateId) &&
                                   (userContext.CourtId == x.CourtId) &&
                                   (filter.FromCourtId <= 0 || filter.FromCourtId == x.FromCourtId) &&
                                   (filter.LawUnitId <= 0 || filter.LawUnitId == x.LawUnitId) &&
                                   (filter.CaseGroupId <= 0 || filter.CaseGroupId == x.CaseGroupId) &&
                                   (filter.CaseTypeId <= 0 || filter.CaseTypeId == x.CaseTypeId) &&
                                   (filter.DateFrom == null || filter.DateFrom <= ((x.DateAccepted ?? x.DateSend) ?? x.RegDate)) &&
                                   (filter.DateTo == null || filter.DateTo >= ((x.DateAccepted ?? x.DateSend) ?? x.RegDate)) &&
                                   (x.PersonName.Contains(filter.PersonName))
                              )
                       .Where(IsNotExpired())
                       .Select(x => new DeliveryItemReturnNewVM()
                       {
                           DateAccepted = (x.DateAccepted  ?? x.RegDate),
                           CaseRegNumber = x.CaseInfo,
                           LawUnitName = x.LawUnit.FullName,
                           PersonName = x.PersonName,
                           DateToLawUnit = deliveryItemOper.Where(o => o.DeliveryItemId == x.Id).Max(d => (DateTime?)d.DateOper) ,
                           NotificationState =  x.NotificationState.Label,
                           DeliveryInfo = x.DeliveryInfo,
                           DeliveryDate = x.DeliveryItemOpers.Max(op => (DateTime?)op.DateOper), //x.DeliveryDate,
                           ReturnReason = x.NotificationStateId != NomenclatureConstants.NotificationState.Delivered ?
                                          (x.DeliveryItemOpers.OrderBy(o => o.DeliveryOperId).ThenBy(o => o.Id).LastOrDefault().DeliveryReason.Label ?? "") :"",
                           DateReturn = x.ReturnDate,
                           DateSend = x.DateSend
                       });
        }

        public DeliveryItemReturnVM GetDeliveryItemReturnByNotification(int notificationId)
        {
            var dItem = repo.AllReadonly<DeliveryItem>().Where(x => (x.CaseNotificationId == notificationId)).FirstOrDefault();
            if (dItem == null)
                return null;
            else
                return GetDeliveryItemReturn(dItem.Id);
        }
        public DeliveryItemReturnVM GetDeliveryItemReturnByDocumentNotification(int notificationId)
        {
            var dItem = repo.AllReadonly<DeliveryItem>().Where(x => (x.DocumentNotificationId == notificationId)).FirstOrDefault();
            if (dItem == null)
                return null;
            else
                return GetDeliveryItemReturn(dItem.Id);
        }
        public DeliveryItemReturnVM GetDeliveryItemReturn(int id)
        {
            int[] states = NotificationStateEnd();
            var model = repo.AllReadonly<DeliveryItem>()
                .Where(x => (x.Id == id))
                .Select(x => new DeliveryItemReturnVM()
                {
                    Id = x.Id,
                    FromCourtName = x.FromCourt == null ? "" : x.FromCourt.Label,
                    RegNumber = x.RegNumber,
                    PersonName = x.PersonName,
                    Address = x.Address == null ? "" : x.Address.FullAddressNotification(),
                    AreaName = x.DeliveryArea == null ? "" : x.DeliveryArea.Description,
                    LawUnitName = x.DeliveryArea.LawUnit == null ? "" : x.DeliveryArea.LawUnit.FullName,
                    NotificationStateId = x.NotificationStateId,
                    NotificationState = x.NotificationState == null ? "" : x.NotificationState.Label,
                    IsForReturn = states.Contains(x.NotificationStateId),
                    CaseNotificationId = x.CaseNotificationId,
                    DocumentNotificationId = x.DocumentNotificationId,
                    ReturnDate = x.CaseNotification == null ? x.ReturnDate : x.CaseNotification.ReturnDate,
                    ReturnInfo = x.CaseNotification == null ? x.DeliveryInfo : x.CaseNotification.ReturnInfo,
                    NotificationDeliveryGroupId = x.CaseNotification == null ? null : x.CaseNotification.NotificationDeliveryGroupId
                })
                .FirstOrDefault();
             return model;
        }
        public List<DeliveryItemReportVM> FillDeliveryItemForCourierList(List<DeliveryItemReportVM> deliveries)
        {
            var opers = GetDeliveryOperMobile();
            var reasons = repo.AllReadonly<DeliveryReason>().ToList();


            foreach (var delivery in deliveries)
            {   
                string deliveryInfo = delivery.DeliveryInfo;
                delivery.DeliveryInfo = "";
                if (delivery.ReturnDate != null)
                    delivery.DeliveryInfo += "Дата на връщане " + delivery.ReturnDate?.ToString(FormattingConstant.NormalDateFormat) + " ";

                if (delivery.DeliveryItemOper != null)
                {
                    var oper = opers.Where(x => x.Id == delivery.DeliveryItemOper.DeliveryOperId).FirstOrDefault();
                    if (oper != null)
                    {
                        delivery.DeliveryInfo += oper.Label + " " +
                                                 delivery.DeliveryItemOper.DateOper.ToString(FormattingConstant.NormalDateFormat) + " " +
                                                 delivery.NotificationState + " ";
                        if (delivery.DeliveryItemOper?.DeliveryReasonId > 0)
                        {
                            var reason = reasons.FirstOrDefault(x => x.Id == delivery.DeliveryItemOper?.DeliveryReasonId);
                            if (reason != null)
                                delivery.DeliveryInfo += reason.Label + " ";
                        }
                    }
                }
                delivery.DeliveryInfo += deliveryInfo;

                delivery.Address = delivery.PersonName + " " + delivery.Address;
            }
            return deliveries;
        }
        public (byte[], string) GetDeliveryItemOutToExcel(DeliveryItemListVM filter)
        {
            var dateTimeNow = DateTime.Now;
            var dateTimeAddOneYear = DateTime.Now.AddYears(1);
            var htmlTemplate = repo.AllReadonly<HtmlTemplate>()
                                   .Where(x => x.Alias == "courier_list" &&
                                               (x.DateFrom <= dateTimeNow && dateTimeNow <= (x.DateTo ?? dateTimeAddOneYear)))
                                   .FirstOrDefault();
            NPoiExcelService excelService = new NPoiExcelService(htmlTemplate.Content, 0);
            int titleRow = (htmlTemplate.XlsTitleRow ?? 0) - 1;
            int dataRow = (htmlTemplate.XlsDataRow ?? 0) - 1;
            if (filter.FromCourtId <= 0)
            {
                filter.FromCourtId = userContext.CourtId;
                var deliveries = GetDeliveryItemOutReport(filter, true).OrderBy(x => x.RegNumber).ToList();
                filter.FromCourtId = 0;
                var deliveriesOther = GetDeliveryItemOutReport(filter, false).OrderBy(x => x.RegNumber).ToList();
                filter.FromCourtId = userContext.CourtId;
                GetDeliveryItemOutToExcelOne(deliveries, filter, excelService, true, titleRow, dataRow);
                excelService.colIndex = 0;
                if (deliveriesOther.Count > 0)
                {
                    excelService.SetRowBreak();
                    int dataRow2 = dataRow + deliveries.Count + 1;
                    int titleRow2 = dataRow + deliveries.Count;
                    excelService.rowIndex = titleRow2;
                    excelService.AddRow();
                    excelService.CopyRowStyle(dataRow);
                    filter.FromCourtId = 0;

                    GetDeliveryItemOutToExcelOne(deliveriesOther, filter, excelService, false, titleRow2, dataRow2);
                }
            }
            else
            {
                var deliveries = GetDeliveryItemOutReport(filter, true).OrderBy(x => x.RegNumber).ToList();
                GetDeliveryItemOutToExcelOne(deliveries, filter, excelService, true, titleRow, dataRow);
            }
            return (excelService.ToArray(), htmlTemplate.FileName);
        }

        public void GetDeliveryItemOutToExcelOne(List<DeliveryItemReportVM> deliveries, DeliveryItemListVM filter, NPoiExcelService excelService, bool printLawUnitName, int rowTitle, int rowData)
        {
            deliveries = FillDeliveryItemForCourierList(deliveries);
            excelService.rowIndex = rowTitle;
            string titleStr = "";
            if (printLawUnitName)
            {
                string LawUnitName = repo.AllReadonly<LawUnit>().Where(x => x.Id == filter.LawUnitId).Select(x => x.FullName).FirstOrDefault();
                if (!string.IsNullOrEmpty(LawUnitName))
                    titleStr += "Описна книга на призовкар: " + LawUnitName + Environment.NewLine;
            }
            string CourtName = repo.AllReadonly<Court>().Where(x => x.Id == filter.FromCourtId).Select(x => x.Label).FirstOrDefault();
            CourtName = "ПРИЗОВКИ / СЪОБЩЕНИЯ НА " + (CourtName ?? "ДРУГИ СЪДИЛИЩА");
            titleStr += CourtName + Environment.NewLine;

            if (filter.CaseGroupId > 0)
            {
                string caseGroup = repo.AllReadonly<CaseGroup>().Where(x => x.Id == filter.CaseGroupId).Select(x => x.Label).FirstOrDefault();
                titleStr += caseGroup + Environment.NewLine;
            }
            if (filter.CaseTypeId > 0)
            {
                string caseType = repo.AllReadonly<CaseType>().Where(x => x.Id == filter.CaseTypeId).Select(x => x.Label).FirstOrDefault();
                titleStr += caseType + Environment.NewLine;
            }
            if (filter.DateFrom != null || filter.DateTo != null)
            {
                excelService.InsertRowTitle(true);
                string dateLabel = "За ";
                bool isPeriod = !(filter.DateFrom?.Date == filter.DateTo?.Date);
                if (isPeriod)
                    dateLabel = "За периодa от: ";
                if (filter.DateFrom != null)
                    dateLabel += filter.DateFrom?.ToString(FormattingConstant.NormalDateFormat);
                if (isPeriod)
                    dateLabel += " до: ";
                if (filter.DateTo != null)
                    dateLabel += filter.DateTo?.ToString(FormattingConstant.NormalDateFormat);
                titleStr += dateLabel + Environment.NewLine;
            }

            excelService.AddRange(titleStr, 5, excelService.CreateTitleStyle());
            excelService.SetRowHeghtFromText(titleStr);
            excelService.rowIndex = rowData;
            excelService.InsertList(
                deliveries,
                new List<Expression<Func<DeliveryItemReportVM, object>>>()
                {
                    x => x.RegNumber,
                    x => x.NotificationTypeLabel,
                    x => x.CaseInfo,
                    x => x.Address,
                    x => x.DeliveryInfo
                }
            );
        }
       
        public (byte[], string) GetDeliveryItemReportResultToExcel(DeliveryItemListVM filter)
        {
            int colCnt = 10;
            var dateTimeNow = DateTime.Now;
            var dateTimeAddOneYear = DateTime.Now.AddYears(1);
            var htmlTemplate = repo.AllReadonly<HtmlTemplate>()
                                   .Where(x => x.Alias == "courier_list_return" &&
                                               (x.DateFrom <= dateTimeNow && dateTimeNow <= (x.DateTo ?? dateTimeAddOneYear)))
                                   .FirstOrDefault();
            NPoiExcelService excelService = new NPoiExcelService(htmlTemplate.Content, 0);
            int titleRow = (htmlTemplate.XlsTitleRow ?? 0) - 1;
            int dataRow = (htmlTemplate.XlsDataRow ?? 0) - 1;
            var deliveries = GetDeliveryItemReportResult(filter).OrderBy(x => x.CaseInfo).ToList();

            string titleStr = "";
            excelService.rowIndex = titleRow;
            string LawUnitName = repo.AllReadonly<LawUnit>().Where(x => x.Id == filter.LawUnitId).Select(x => x.FullName).FirstOrDefault();
            if (!string.IsNullOrEmpty(LawUnitName))
                titleStr += "Съдебен призовкар: " + LawUnitName + Environment.NewLine;

            if (filter.CaseGroupId > 0)
            {
                string caseGroup = repo.AllReadonly<CaseGroup>().Where(x => x.Id == filter.CaseGroupId).Select(x => x.Label).FirstOrDefault();
                titleStr += caseGroup + Environment.NewLine;
            }
            if (filter.CaseTypeId > 0)
            {
                string caseType = repo.AllReadonly<CaseType>().Where(x => x.Id == filter.CaseTypeId).Select(x => x.Label).FirstOrDefault();
                titleStr += caseType + Environment.NewLine;
            }
            if (filter.DateFrom != null || filter.DateTo != null)
            {
                string dateLabel = "За ";
                bool isPeriod = !(filter.DateFrom?.Date == filter.DateTo?.Date);
                if (isPeriod)
                    dateLabel = "За периодa от: ";
                if (filter.DateFrom != null)
                    dateLabel += filter.DateFrom?.ToString(FormattingConstant.NormalDateFormat);
                if (isPeriod)
                    dateLabel += " до: ";
                if (filter.DateTo != null)
                    dateLabel += filter.DateTo?.ToString(FormattingConstant.NormalDateFormat);
                titleStr += dateLabel + Environment.NewLine;
            }
            if (filter.FromCourtId > 0)
            {
                var court = repo.AllReadonly<Court>().Where(x => x.Id == filter.FromCourtId).FirstOrDefault();
                if (court != null)
                    titleStr += court.Label + Environment.NewLine;
            }
            excelService.AddRange(titleStr, colCnt, excelService.CreateTitleStyle());
            excelService.SetRowHeghtFromText(titleStr);

            excelService.rowIndex = dataRow;
            excelService.InsertList(
                deliveries,
                new List<Expression<Func<DeliveryItemReportResultVM, object>>>()
                {
                   // x => x.CaseTypeLabel,
                    x => x.CaseInfo,
                    x => x.FromCourtName,
                    x => x.DateFromStr,
                    x => x.LawUnitName,
                    x => x.DocumentType,
                    x => x.StateName,
                    x => x.DateResultStr,
                    x => x.ReasonReturn,
                    x => x.PersonName,
                    x => x.Address
                }
            );
            return (excelService.ToArray(), htmlTemplate.FileName);
        }
        public (byte[], string) GetDeliveryItemReportResultToExcelNew(DeliveryItemListVM filter)
        {
            int colCnt = 10;
            var dateTimeNow = DateTime.Now;
            var dateTimeAddOneYear = DateTime.Now.AddYears(1);
            var htmlTemplate = repo.AllReadonly<HtmlTemplate>()
                                   .Where(x => x.Alias == "courier_list_return_new" &&
                                               (x.DateFrom <= dateTimeNow && dateTimeNow <= (x.DateTo ?? dateTimeAddOneYear)))
                                   .FirstOrDefault();
            NPoiExcelService excelService = new NPoiExcelService(htmlTemplate.Content, 0);
            int titleRow = (htmlTemplate.XlsTitleRow ?? 0) - 1;
            int dataRow = (htmlTemplate.XlsDataRow ?? 0) - 1;

            string titleStr = "";
            excelService.rowIndex = titleRow;
            string LawUnitName = repo.AllReadonly<LawUnit>().Where(x => x.Id == filter.LawUnitId).Select(x => x.FullName).FirstOrDefault();
            if (!string.IsNullOrEmpty(LawUnitName))
                titleStr += "Съдебен призовкар: " + LawUnitName + Environment.NewLine;

            if (filter.CaseGroupId > 0)
            {
                string caseGroup = repo.AllReadonly<CaseGroup>().Where(x => x.Id == filter.CaseGroupId).Select(x => x.Label).FirstOrDefault();
                titleStr += caseGroup + Environment.NewLine;
            }
            if (filter.CaseTypeId > 0)
            {
                string caseType = repo.AllReadonly<CaseType>().Where(x => x.Id == filter.CaseTypeId).Select(x => x.Label).FirstOrDefault();
                titleStr += caseType + Environment.NewLine;
            }
            if (filter.DateFrom != null || filter.DateTo != null)
            {
                string dateLabel = "За ";
                bool isPeriod = !(filter.DateFrom?.Date == filter.DateTo?.Date);
                if (isPeriod)
                    dateLabel = "За периодa от: ";
                if (filter.DateFrom != null)
                    dateLabel += filter.DateFrom?.ToString(FormattingConstant.NormalDateFormat);
                if (isPeriod)
                    dateLabel += " до: ";
                if (filter.DateTo != null)
                    dateLabel += filter.DateTo?.ToString(FormattingConstant.NormalDateFormat);
                titleStr += dateLabel + Environment.NewLine;
            }
            if (filter.FromCourtId > 0)
            {
                var court = repo.AllReadonly<Court>().Where(x => x.Id == filter.FromCourtId).FirstOrDefault();
                if (court != null)
                    titleStr += court.Label + Environment.NewLine;
            }
            excelService.AddRange(titleStr, colCnt, excelService.CreateTitleStyle());
            excelService.SetRowHeghtFromText(titleStr);

            var deliveries = GetDeliveryItemReportResultNew(filter)
                                  .OrderBy(x => x.CaseRegNumber)
                                  .ToList();
            foreach (var item in deliveries)
            {
                item.DateAcceptedRep = item.DateAccepted?.ToString(FormattingConstant.NormalDateFormat) ?? "";
                if (item.CaseRegNumber?.IndexOf(" /") > 0)
                {
                    item.CaseRegNumber = item.CaseRegNumber.Substring(0, item.CaseRegNumber.IndexOf(" /"));
                }
                item.DateToLawUnitRep = item.DateToLawUnit?.ToString(FormattingConstant.NormalDateFormatHHMM) ?? "";
                item.DateReturnRep = item.DateReturn?.ToString(FormattingConstant.NormalDateFormatHHMM) ?? "";
                item.DateSendRep = item.DateSend?.ToString(FormattingConstant.NormalDateFormatHHMM) ?? "";
                item.DeliveryInfoRep = item.NotificationState + " " + item.DeliveryDate?.ToString(FormattingConstant.NormalDateFormatHHMM) + "  " +
                                       item.DeliveryInfo + " " + item.ReturnReason; 
                       
            }
            excelService.rowIndex = dataRow;
            excelService.InsertList(
                deliveries,
                new List<Expression<Func<DeliveryItemReturnNewVM, object>>>()
                {
                   // x => x.CaseTypeLabel,
                    x => x.DateAcceptedRep,
                    x => x.CaseRegNumber,
                    x => x.PersonName,
                    x => x.LawUnitName,
                    x => x.DateToLawUnitRep,
                    x => x.DeliveryInfoRep,
                    x => x.DateReturnRep,
                    x => x.DateSendRep
                }
            );
            return (excelService.ToArray(), htmlTemplate.FileName);
        }
        public List<MobileValueLabelVM> GetCourtsMobile()
        {
            return repo.AllReadonly<Court>()
                .Select(x => new MobileValueLabelVM()
                {
                    label = x.Label,
                    value = x.Id.ToString(),
                })
                .ToList();
        }
        public List<MobileValueLabelGroupVM> GetNotificationStateMobile()
        {
            var operState = repo.AllReadonly<DeliveryOperState>();
            int[] states = NotificationStateEnd();
            var result = repo.AllReadonly<NotificationState>()
                .Where(x => operState.Any(o => o.NotificationStateId == x.Id) || x.Id == NomenclatureConstants.NotificationState.ForDelivery)
                .Select(x => new MobileValueLabelGroupVM()
                {
                    label = x.Label,
                    value = x.Id.ToString(),
                    orderNumber = x.OrderNumber,
                })
                .ToList();
            foreach(var item in result)
            {
                item.group = states.Contains(int.Parse(item.value)) ? "1" : "0";
            }
            return result;
        }
        public List<DeliveryOper> GetDeliveryOperMobile()
        {
            var operState = repo.AllReadonly<DeliveryOperState>();
            return repo.AllReadonly<DeliveryOper>()
                .Where(x => operState.Any(o => o.DeliveryOperId == x.Id))
                .ToList();
        }
        public List<MobileValueLabelGroupVM> GetDeliveryReasonMobile()
        {
            return repo.AllReadonly<DeliveryStateReason>()
                .Include(x => x.DeliveryReason)
                .Select(x => new MobileValueLabelGroupVM()
                {
                    label = x.DeliveryReason.Label,
                    value = x.DeliveryReason.Id.ToString(),
                    group = x.NotificationStateId.ToString(),
                    orderNumber = x.DeliveryReason.OrderNumber,
                })
                .ToList();
        }
        public List<MobileValueLabelVM> GetNotificationTypeMobile()
        {
            return repo.AllReadonly<NotificationType>()
                .Select(x => new MobileValueLabelVM()
                {
                    label = x.Label,
                    value = x.Id.ToString(),
                    orderNumber = x.OrderNumber,
                })
                .ToList();
        }
        private DateTime NullDateStart()
        {
            return new DateTime(2000, 1, 1);
        }
        private DateTime NullDateEnd()
        {
            return new DateTime(2100, 1, 1);
        }
        public List<DeliveryItemMobileVM> GetDeliveryItemMobileVM(int courtId, int lawUnitId, DateTime? fromDate, DateTime? toDate)
        {
            int[] states = new int[]
            {
               NomenclatureConstants.NotificationState.Visited,
               NomenclatureConstants.NotificationState.ForDelivery,
            };
            DateTime dateWD = fromDate ?? DateTime.Now;
            dateWD = dateWD.AddDays(-365);
            DateTime dateWDTo = (toDate ?? DateTime.Now).Date;
            var dictWD = workingDaysService.GetWorkingDays(courtId ,dateWD, dateWDTo);
            var listWD = dictWD.Where(kv => kv.Value == CommonContants.WorkingDays.NotWorkDay).Select(kv => kv.Key).ToList();

            DateTime dStart = DateTime.Now;
            DateTime fromDateN = fromDate ?? NullDateStart();
            DateTime toDateN = toDate ?? NullDateStart();
            // var deliveryOpers = getDeliveryOperMobile();
            var deliveries = repo.AllReadonly<DeliveryItem>()
                .Where(x => x.LawUnitId == lawUnitId &&
                            x.CourtId == courtId &&
                            states.Contains(x.NotificationStateId) &&
                           (x.DateAccepted ?? dStart).Date >= fromDateN.Date &&
                           (x.DateAccepted ?? dStart).Date <= toDateN.Date)
                .Include(x => x.Address)
                .Include(x => x.DeliveryItemOpers)
                .ToList();
            var result = deliveries.Select(x => new DeliveryItemMobileVM()
                {
                    Id = x.Id,
                    CourtId = x.FromCourtId,
                    ItemDate = x.DateAccepted ?? dStart,
                    RegNumber = x.RegNumber,
                    PersonName = x.PersonName,
                    Address = x.Address == null ? "" : x.Address.FullAddressNotification(),
                    StateId = x.NotificationStateId,
                    CaseInfo = x.CaseInfo,
                    ReasonId = 0,
                    VisitCount = x.DeliveryItemOpers
                                  .Where(o => o.NotificationStateId == NomenclatureConstants.NotificationState.Visited)
                                  .GroupBy(gr => gr.DeliveryOperId)
                                  .Select(gr => gr.First())
                                  .Count(),//,Max(o => (int?)o.DeliveryOperId) ?? 0,
                    LastVisit = x.DeliveryItemOpers.Where(o => o.NotificationStateId == NomenclatureConstants.NotificationState.Visited).Max(o => (DateTime?)o.DateOper),
                    HaveHolidayVisit = listWD.Where(wd => x.DeliveryItemOpers.Where(o => 
                      o.NotificationStateId == NomenclatureConstants.NotificationState.Visited && 
                      o.DateOper.Date == wd).Any()).Any(),
                    NotificationTypeId = x.NotificationTypeId ?? 0
                })
                .ToList();
            return result;
        }
        public bool DeliveryItemSaveOperMobile(DeliveryItemVisitMobile model)
        {
            try
            {
                model.Id = 0;
                model.DateAPI = DateTime.Now;
                model.IsOK = false;
                repo.Add(model);
                repo.SaveChanges();
                if (model.NotificationStateId <= 0)
                    return true;
                var opers = repo.AllReadonly<DeliveryOperState>()
                                .GroupBy(gr => gr.DeliveryOperId) 
                                .Select(gr => gr.First().DeliveryOper)
                                .OrderBy(x => x.Id).ToList();
                var dOper = opers.Last();
                if (opers.Count > model.DeliveryOperId) 
                    dOper = opers[model.DeliveryOperId];
                
                var saved = repo.GetById<DeliveryItem>(model.DeliveryItemId);

                saved.NotificationStateId = model.NotificationStateId;
                DeliveryItemOper oper = CreateDeliveryItemOper(saved, dOper.Id);
                oper.DeliveryInfo = model.DeliveryInfo;
                oper.Lat = model.Lat;
                oper.Long = model.Long;
                oper.DeliveryReasonId = model.DeliveryReasonId > 0 ? (int?)model.DeliveryReasonId : null;
                oper.UserId = model.UserId;
                oper.DateWrt = model.DateOper;
                oper.DateOper = model.DateOper;

                saved.DeliveryReasonId = oper.DeliveryReasonId;
                saved.DeliveryInfo = "";
                saved.DateWrt = DateTime.Now;
                saved.UserId =  model.UserId;

                UpdateOperToNotification(saved, oper);
                repo.Update(saved);
                model.IsOK = true;
                repo.Update(model);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на посещение от мобилното устройство Id={ model.Id }" + JsonConvert.SerializeObject(model));
                return false;
            }
        }
        public Court GetCourtById(int courtId)
        {
            return repo.GetById<Court>(courtId);
        }
        public List<SelectListItem> LawUnitForCourt_SelectDdlAllInDeliveryItem(int forCourtId, List<SelectListItem> newLawUnits)
        {
            int[] states = NotificationStateEnd();
            var deliveries = repo.AllReadonly<DeliveryItem>()
                             .Include(x => x.LawUnit)
                             .Where(x => x.CourtId == forCourtId)
                             .Where(x => !states.Contains(x.NotificationStateId))
                             .Where(x => x.LawUnitId != null);
            var result = repo.AllReadonly<LawUnit>()
                              .Where(x => deliveries.Any(d => d.LawUnitId == x.Id))
                       .Select(x => new SelectListItem()
                       {
                           Text = x.FullName,
                           Value = x.Id.ToString()
                       }).ToList() ?? new List<SelectListItem>();
            foreach (var item in result)
            {
                if (!newLawUnits.Any(x => x.Value == item.Value))
                    item.Text += " *";
            }

            result.Insert(0, new SelectListItem() { Text = "Без избран призовкар", Value = "0" });
            result.Insert(0, new SelectListItem() { Text = "Без деистващ призовкар", Value = "-2" });
            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            foreach (var item in newLawUnits)
            {
                if (!result.Any(x => x.Value == item.Value))
                    result.Add(item);
            }

            return result;
        }
       
        public List<SelectListItem> SelectNewLawUnitType()
        {
            var result = new List<SelectListItem>();
            result.Insert(0, new SelectListItem() { Text = "Служители", Value = NomenclatureConstants.LawUnitTypes.OtherEmployee.ToString() });
            result.Insert(0, new SelectListItem() { Text = "Призовкари", Value = NomenclatureConstants.LawUnitTypes.MessageDeliverer.ToString() });
            return result;
        }
        public List<SelectListItem> NotificationDeliveryGroupSelect()
        {
            var result = repo.AllReadonly<NotificationDeliveryGroup>()
                              .Where(x => x.Id == NomenclatureConstants.NotificationDeliveryGroup.WithSummons ||
                                          x.Id == NomenclatureConstants.NotificationDeliveryGroup.WithCourier ||
                                          x.Id == NomenclatureConstants.NotificationDeliveryGroup.WithCityHall)
                       .Select(x => new SelectListItem()
                       {
                           Text = x.Label,
                           Value = x.Id.ToString()
                       }).ToList() ?? new List<SelectListItem>();
            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });

            return result;
        }
        public bool DeliveryItemSaveState(int deliveryItemId, int notificationStateId, DateTime? deliveryDate, string deliveryInfo)
        {
            try
            {
                var saved = repo.GetById<DeliveryItem>(deliveryItemId);
                saved.NotificationStateId = notificationStateId;
                int deliveryOperId = notificationStateId;
                if (!repo.AllReadonly<DeliveryOper>().Any(x => x.Id == deliveryOperId))
                {
                    deliveryOperId = NomenclatureConstants.DeliveryOper.Visit1;
                }
                
                DeliveryItemOper oper = CreateDeliveryItemOper(saved, deliveryOperId);
                if (oper != null)
                {
                    oper.DateOper = deliveryDate ?? DateTime.Now;
                }
                saved.DeliveryDate = deliveryDate ?? DateTime.Now;
                saved.DeliveryInfo = deliveryInfo;
                saved.DateWrt = DateTime.Now;
                saved.UserId = userContext.UserId;
                UpdateOperToNotification(saved, oper);
                repo.Update(saved);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на статус на призовка deliveryItemId={ deliveryItemId }");
                return false;
            }
        }
        public List<Select2ItemVM> GetCourtsSelect2(List<DeliveryArea> deliveryAreaList)
        {
            var result = repo.AllReadonly<Court>()
                        .Where(x => deliveryAreaList.Any(d => d.CourtId == x.Id) || !deliveryAreaList.Any())
                        .OrderBy(x => x.Label)
                        .Select(x => new Select2ItemVM()
                        {
                            Text = x.Label,
                            Id = x.Id
                        }).ToList() ?? new List<Select2ItemVM>();
            result.Insert(0, new Select2ItemVM() { Text = "Избери", Id = -1 });
            return result;
        }

    }
}
