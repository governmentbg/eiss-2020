using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

//([a-zA-Z]+.DeliveryAreaSelectDDL)
//await $1
namespace IOWebApplication.Core.Services
{
    public class DeliveryItemService : BaseService, IDeliveryItemService
    {
        private readonly INomenclatureService nomenclatureService;
        private readonly IWorkingDaysService workingDaysService;
        private readonly IWorkNotificationService workNotificationService;
        private readonly ICaseDeadlineService caseDeadlineService;
        public DeliveryItemService(
            ILogger<DeliveryItemService> _logger,
            IRepository _repo,
            IUserContext _userContext,
            INomenclatureService _nomenclatureService,
            IWorkingDaysService _workingDaysService,
            IWorkNotificationService _workNotificationService,
            ICaseDeadlineService _caseDeadlineService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            nomenclatureService = _nomenclatureService;
            workingDaysService = _workingDaysService;
            workNotificationService = _workNotificationService;
            caseDeadlineService = _caseDeadlineService;
        }
        private Expression<Func<DeliveryItem, bool>> IsNotExpired()
        {
            return x => x.DateExpired == null;
        }

        private Expression<Func<DeliveryItem, bool>> GetNotificationStateIdWhere(int notificationStateId)
        {
            Expression<Func<DeliveryItem, bool>> notificationStateIdWhere = x => true;
            if (notificationStateId > 0)
                notificationStateIdWhere = x => x.NotificationStateId == notificationStateId;

            return notificationStateIdWhere;
        }

        private Expression<Func<DeliveryItem, bool>> GetNotificationTypeIdWhere(int notificationTypeId)
        {
            Expression<Func<DeliveryItem, bool>> notificationTypeIdWhere = x => true;
            if (notificationTypeId > 0)
                notificationTypeIdWhere = x => x.NotificationTypeId == notificationTypeId;

            return notificationTypeIdWhere;
        }

        private Expression<Func<DeliveryItem, bool>> GetLawUnitIdWhere(int lawUnitId)
        {
            Expression<Func<DeliveryItem, bool>> lawUnitIdWhere = x => true;
            if (lawUnitId > 0)
                lawUnitIdWhere = x => x.LawUnitId == lawUnitId;

            return lawUnitIdWhere;
        }

        private Expression<Func<DeliveryItem, bool>> GetCourtIdWhere(int courtId)
        {
            Expression<Func<DeliveryItem, bool>> courtIdWhere = x => true;
            if (courtId > 0)
                courtIdWhere = x => x.CourtId == courtId;

            return courtIdWhere;
        }

        private Expression<Func<DeliveryItem, bool>> GetFromCourtIdWhere(int fromCourtId)
        {
            Expression<Func<DeliveryItem, bool>> fromCourtIdWhere = x => true;
            if (fromCourtId > 0)
                fromCourtIdWhere = x => x.FromCourtId == fromCourtId;

            return fromCourtIdWhere;
        }

        private Expression<Func<DeliveryItem, bool>> GetFilterTypeWhere(int filterType, int courtId, int fromCourtId)
        {
            Expression<Func<DeliveryItem, bool>> filterTypeWhere = x => true;
            if (filterType > 0)
            {
                switch (filterType)
                {
                    case NomenclatureConstants.DeliveryItemFilterType.FromOther:
                        filterTypeWhere = x => x.FromCourtId != courtId &&
                                               x.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.WithSummons;
                        break;
                    case NomenclatureConstants.DeliveryItemFilterType.ToOther:
                        filterTypeWhere = x => x.CourtId != fromCourtId;
                        break;
                    default:
                        filterTypeWhere = x => true;
                        break;
                }
            }

            return filterTypeWhere;
        }

        private Expression<Func<DeliveryItem, bool>> GetDateSendFromWhere(DateTime? dateSendFrom)
        {
            Expression<Func<DeliveryItem, bool>> dateSendFromWhere = x => true;
            if (dateSendFrom != null)
            {
                dateSendFrom = dateSendFrom.ForceStartDate();
                dateSendFromWhere = x => x.DateSend >= dateSendFrom;
            }

            return dateSendFromWhere;
        }

        private Expression<Func<DeliveryItem, bool>> GetDateSendToWhere(DateTime? dateSendTo)
        {
            Expression<Func<DeliveryItem, bool>> dateSendToWhere = x => true;
            if (dateSendTo != null)
            {
                dateSendTo = dateSendTo.ForceEndDate();
                dateSendToWhere = x => x.DateSend <= dateSendTo;
            }

            return dateSendToWhere;
        }

        private Expression<Func<DeliveryItem, bool>> GetDateAcceptedFromWhere(DateTime? dateAcceptedFrom)
        {
            Expression<Func<DeliveryItem, bool>> dateAcceptedFromWhere = x => true;
            if (dateAcceptedFrom != null)
            {
                dateAcceptedFrom = dateAcceptedFrom.ForceStartDate();
                dateAcceptedFromWhere = x => x.DateAccepted >= dateAcceptedFrom;
            }

            return dateAcceptedFromWhere;
        }

        private Expression<Func<DeliveryItem, bool>> GetDateAcceptedToWhere(DateTime? dateAcceptedTo)
        {
            Expression<Func<DeliveryItem, bool>> dateAcceptedToWhere = x => true;
            if (dateAcceptedTo != null)
            {
                dateAcceptedTo = dateAcceptedTo.ForceEndDate();
                dateAcceptedToWhere = x => x.DateAccepted <= dateAcceptedTo;
            }

            return dateAcceptedToWhere;
        }

        private Expression<Func<DeliveryItem, bool>> GetRegNumberWhere(string regNumber)
        {
            Expression<Func<DeliveryItem, bool>> regNumberWhere = x => true;
            if (!string.IsNullOrEmpty(regNumber) && regNumber.Length < 12)
            {
                var regNumberPatern = regNumber.ToPaternSearch();
                regNumberWhere = x => EF.Functions.ILike(x.RegNumber, regNumberPatern);
            }
            if (!string.IsNullOrEmpty(regNumber) && regNumber.Length >= 12)
            {
                regNumberWhere = x => x.RegNumber == regNumber;
            }

            return regNumberWhere;
        }


        private Expression<Func<DeliveryItem, bool>> GetNotificationDeliveryGroupIdWhere(int notificationDeliveryGroupId)
        {
            Expression<Func<DeliveryItem, bool>> notificationDeliveryGroupIdWhere = x => true;
            if (notificationDeliveryGroupId > 0)
                notificationDeliveryGroupIdWhere = x => x.NotificationDeliveryGroupId == notificationDeliveryGroupId;

            return notificationDeliveryGroupIdWhere;
        }

        private Expression<Func<DeliveryItem, bool>> GetPreparedByIdWhere(int preparedById)
        {
            Expression<Func<DeliveryItem, bool>> preparedByIdWhere = x => true;
            if (preparedById > 0)
                preparedByIdWhere = x => x.PreparedById == preparedById;

            return preparedByIdWhere;
        }

        private Expression<Func<DeliveryItem, bool>> GetCourtDepartmentIdWhere(int courtDepartmentId)
        {
            Expression<Func<DeliveryItem, bool>> courtDepartmentIdWhere = x => true;
            if (courtDepartmentId > 0)
                courtDepartmentIdWhere = x => x.CaseSession.CaseLawUnits.Any(l => l.CourtDepartmentId == courtDepartmentId &&
                                                                                  l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            return courtDepartmentIdWhere;
        }

        public IQueryable<DeliveryItemVM> DeliveryItemSelect(DeliveryItemFilterVM filter)
        {
              DateTime dNull = DateTime.Now;

            filter.ResetCourtByType(userContext.CourtId);

            var notificationGroups = NotificationDeliveryGroup(filter.FilterType);

            Expression<Func<DeliveryItem, bool>> isGenerated = x => true;
            if (filter.IsGenerated == NomenclatureConstants.YesNo.Yes)
            {
                isGenerated = x => x.CaseNotificationId != null && x.CaseNotification.DatePrint != null;
            }
            if (filter.IsGenerated == NomenclatureConstants.YesNo.No)
            {
                isGenerated = x => x.CaseNotificationId == null || x.CaseNotification.DatePrint == null;
            }
            Expression<Func<DeliveryItem, bool>> isFastProcess = x => true;
            if (filter.IsFastProcess == NomenclatureConstants.YesNo.Yes)
            {
                isFastProcess = x => x.Case.IsFastProcess == true;
            }
            if (filter.IsFastProcess == NomenclatureConstants.YesNo.No)
            {
                isFastProcess = x => x.Case.IsFastProcess != true;
            }
            var query = repo.AllReadonly<DeliveryItem>()
                       .Where(GetNotificationStateIdWhere(filter.NotificationStateId))
                       .Where(GetNotificationTypeIdWhere(filter.NotificationTypeId))
                       .Where(GetLawUnitIdWhere(filter.LawUnitId))
                       .Where(GetCourtIdWhere(filter.CourtId))
                       .Where(GetFromCourtIdWhere(filter.FromCourtId))
                       .Where(GetFilterTypeWhere(filter.FilterType, filter.CourtId, filter.FromCourtId))
                       .Where(GetDateSendFromWhere(filter.DateSendFrom))
                       .Where(GetDateSendToWhere(filter.DateSendTo))
                       .Where(GetDateAcceptedFromWhere(filter.DateAcceptedFrom))
                       .Where(GetDateAcceptedToWhere(filter.DateAcceptedTo))
                       .Where(GetRegNumberWhere(filter.RegNumber))
                       .Where(isFastProcess)
                       .Where(GetNotificationDeliveryGroupIdWhere(filter.NotificationDeliveryGroupId))
                       .Where(GetPreparedByIdWhere(filter.PreparedById))
                       .Where(GetCourtDepartmentIdWhere(filter.CourtDepartmentId))
                       .Where(IsNotExpired())
                       .Where(isGenerated);
            if (!string.IsNullOrEmpty(filter.CaseRegNumber))
            {
                var caseRegNumber = (filter.CaseRegNumber.ToShortCaseNumber() ?? filter.CaseRegNumber).ToPaternSearch();
                var docRegNumber = filter.CaseRegNumber.ToPaternSearch();

                query = query.Where(x => x.CaseId != null && EF.Functions.ILike(x.Case.RegNumber, caseRegNumber))
                             .Union(query.Where(x => x.CaseId == null && x.DocumentNotificationId == null && EF.Functions.ILike(x.CaseInfo, docRegNumber)));
            }
            return query.Select(x => new DeliveryItemVM()
                       {
                           Id = x.Id,
                           FromCourtName = x.FromCourt.Label,
                           CourtName = x.Court.Label,
                           LawUnitName = x.LawUnitId != null && x.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.WithSummons ?
                                                      x.LawUnit.FullName:
                                                      x.NotificationDeliveryGroup.Label,
                           AreaName = x.DeliveryAreaId != null && x.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.WithSummons ?
                                                                  x.DeliveryArea.Description:
                                                                  string.Empty,
                           PersonName = x.PersonName,
                           FullAddress = x.Address.FullAddress,
                           AddressPhone = x.Address.Phone ,
                           AddressEmail = x.Address.Email ,
                           AddressFax = x.Address.Fax ,
                           StateName = x.NotificationState.Label,
                           RegNumber = x.RegNumber,
                           DateSend = x.DateSend,
                           DateAccepted = x.DateAccepted,
                           PreparedBy = x.PreparedBy.FullName,
                           DepartmentLabel = x.CaseSession
                                              .CaseLawUnits
                                              .OrderBy(l => l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                              .Select(l => l.CourtDepartment.Label)
                                              .FirstOrDefault(),
                           DeliveryDate = x.DeliveryDate ?? x.RegDate,
                           CaseInfo = x.DocumentNotificationId != null ? "Документ "+(x.CaseInfo ?? string.Empty)
                                                                        : x.CaseInfo,
                           NotificationDeliveryGroupId = x.NotificationDeliveryGroupId,
                           NotificationType = x.NotificationType.Label,
                           CaseNotificationId = x.CaseNotificationId,
                           DocumentNotificationId = x.DocumentNotificationId,
                           FastCaseInfo = x.Case.IsFastProcess == true ? NomenclatureConstants.DeliveryItemMessage.FastProcess: string.Empty,
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

        public IQueryable<DeliveryItem> DeliveryItemTransSelectQuery(DeliveryItemTransFilterVM filter, bool allCourt)
        {
            filter.DateFrom = filter.DateFrom?.Date;
            filter.DateTo = filter.DateTo?.Date;

            filter.RegDateFrom = filter.RegDateFrom?.Date;
            filter.RegDateTo = filter.RegDateTo?.Date;

            int courtId = 0;
            int fromCourtId = 0;
            int lawUnitId = 0;
            if (filter.ToNotificationStateId == NomenclatureConstants.NotificationState.Send)
            {
                fromCourtId = userContext.CourtId;
                if (!allCourt)
                    courtId = filter.ForId;
            }
            if (filter.ToNotificationStateId == NomenclatureConstants.NotificationState.ForDelivery)
            {
                courtId = userContext.CourtId;
                if (!allCourt)
                    lawUnitId = filter.ForId;
            }
            if (filter.ToNotificationStateId == NomenclatureConstants.NotificationState.Received)
            {
                courtId = userContext.CourtId;
                if (!allCourt)
                    fromCourtId = filter.ForId;
            }

            var opers = repo.AllReadonly<DeliveryItemOper>()
                            .Where(x => x.NotificationStateId == filter.NotificationStateId);

            var deliveryItems = repo.AllReadonly<DeliveryItem>();

            if (filter.NotificationStateId == NomenclatureConstants.NotificationState.NoDeliveryArea ||
                filter.NotificationStateId == NomenclatureConstants.NotificationState.AllForReceived)
            {
                var states = new int[]{ NomenclatureConstants.NotificationState.Ready, NomenclatureConstants.NotificationState.Send};
                deliveryItems = deliveryItems.Where(x => states.Contains(x.NotificationStateId));
                if (filter.NotificationStateId != NomenclatureConstants.NotificationState.AllForReceived) {
                    deliveryItems = deliveryItems.Where(x => x.DeliveryAreaId == null);
                }
                opers = repo.AllReadonly<DeliveryItemOper>();
            }
            else
            {
                deliveryItems = deliveryItems.Where(x => x.NotificationStateId == filter.NotificationStateId);
            }
            Expression<Func<DeliveryItem, bool>> isFastProcess = x => true;
            if (filter.IsFastProcess == NomenclatureConstants.YesNo.Yes)
            {
                isFastProcess = x => x.Case.IsFastProcess == true;
            }
            if (filter.IsFastProcess == NomenclatureConstants.YesNo.No)
            {
                isFastProcess = x => x.Case.IsFastProcess != true;
            }

            deliveryItems = deliveryItems.Where(isFastProcess)
                                         .Where(GetCourtIdWhere(courtId))
                                         .Where(GetFromCourtIdWhere(fromCourtId))
                                         .Where(IsNotExpired());
            //.Where(x => (lawUnitId == -2 ? x.LawUnitId == null : (lawUnitId <= 0 || x.LawUnitId == lawUnitId)) &&
            //         (filter.DateFrom == null || opers.Where(op => op.DeliveryItemId == x.Id && op.DateOper >= filter.DateFrom).Any()) &&
            //         (filter.DateTo == null || opers.Where(op => op.DeliveryItemId == x.Id && op.DateOper.Date <= filter.DateTo).Any()) &&
            //         (filter.RegDateFrom == null || x.RegDate >= filter.RegDateFrom) &&
            //         (filter.RegDateTo == null || x.RegDate <= filter.RegDateTo) &&
            //         (filter.NotificationTypeId <= 0 || x.NotificationTypeId == filter.NotificationTypeId) &&
            //         (filter.NotificationDeliveryGroupId <= 0 || x.NotificationDeliveryGroupId == filter.NotificationDeliveryGroupId)
            //      )

            //x => (lawUnitId == -2 ? x.LawUnitId == null : (lawUnitId <= 0 || x.LawUnitId == lawUnitId)
            if (lawUnitId == -2)
            {
                deliveryItems = deliveryItems.Where(x => x.LawUnitId == null);
            }
            if (lawUnitId > 0)
            {
                deliveryItems = deliveryItems.Where(x => x.LawUnitId == lawUnitId);
            }

            //      (filter.DateFrom == null || opers.Where(op => op.DeliveryItemId == x.Id && op.DateOper >= filter.DateFrom).Any()) &&
            if (filter.DateFrom != null)
            {
                deliveryItems = deliveryItems.Where(x => opers.Where(op => op.DeliveryItemId == x.Id && op.DateOper >= filter.DateFrom).Any());
            }
            // (filter.DateTo == null || opers.Where(op => op.DeliveryItemId == x.Id && op.DateOper.Date <= filter.DateTo).Any()) &&     
            if (filter.DateTo != null)
            {
                deliveryItems = deliveryItems.Where(x => opers.Where(op => op.DeliveryItemId == x.Id && op.DateOper.Date <= filter.DateTo).Any());
            }
            // (filter.RegDateFrom == null || x.RegDate >= filter.RegDateFrom)
            if (filter.RegDateFrom != null)
            {
                deliveryItems = deliveryItems.Where(x => x.RegDate >= filter.RegDateFrom);
            }
            // (filter.RegDateTo == null || x.RegDate <= filter.RegDateTo) 
            if (filter.RegDateTo != null)
            {
                deliveryItems = deliveryItems.Where(x => x.RegDate <= filter.RegDateTo);
            }
            //         (filter.NotificationTypeId <= 0 || x.NotificationTypeId == filter.NotificationTypeId) &&
            if (filter.NotificationTypeId > 0)
            {
                deliveryItems = deliveryItems.Where(x => x.NotificationTypeId == filter.NotificationTypeId);
            }
            //         (filter.NotificationDeliveryGroupId <= 0 || x.NotificationDeliveryGroupId == filter.NotificationDeliveryGroupId)
            if (filter.NotificationDeliveryGroupId > 0)
            {
                deliveryItems = deliveryItems.Where(x => x.NotificationDeliveryGroupId == filter.NotificationDeliveryGroupId);
            }
            if (filter.ToNotificationStateId == NomenclatureConstants.NotificationState.ForDelivery)
            {
                deliveryItems = deliveryItems.Where(x => x.CaseNotificationId == null || x.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.WithSummons);
            }
            // през епеп, ел поща, по телефон, в заседание да не се показват
            if (filter.NotificationDeliveryGroupId == 0 && filter.ToNotificationStateId != NomenclatureConstants.NotificationState.ForDelivery)
            {
                deliveryItems = deliveryItems.Where(x => x.CaseNotificationId == null || (
                          x.NotificationDeliveryGroupId != NomenclatureConstants.NotificationDeliveryGroup.ByEPEP &&
                          x.NotificationDeliveryGroupId != NomenclatureConstants.NotificationDeliveryGroup.OnPhone &&
                          x.NotificationDeliveryGroupId != NomenclatureConstants.NotificationDeliveryGroup.OnEMail &&
                          x.NotificationDeliveryGroupId != NomenclatureConstants.NotificationDeliveryGroup.OnSession
                       ));
            }
            return deliveryItems;
        }

        public IQueryable<DeliveryItemRecapTransVM> DeliveryItemTransSelectRecap(DeliveryItemTransFilterVM filter)
        {
            var deliveryItems = DeliveryItemTransSelectQuery(filter, true);
            
                if (filter.ToNotificationStateId == NomenclatureConstants.NotificationState.ForDelivery)
                {
                    return deliveryItems
                     .Select(x => new DeliveryItemRecapTransVM()
                     {
                         Id = x.Id,
                         FromCourtName = x.FromCourt.Label,
                         FromCourtId = x.FromCourtId,
                         CourtId = x.CourtId,
                         CourtName = x.Court.Label,
                         LawUnitName = x.LawUnit.FullName,
                         LawUnitId = x.LawUnitId,
                     });
                }
                else
                {
                    return deliveryItems
                     .Select(x => new DeliveryItemRecapTransVM()
                     {
                         Id = x.Id,
                         FromCourtName = x.FromCourt.Label,
                         FromCourtId = x.FromCourtId,
                         CourtId = x.CourtId,
                         CourtName = x.Court.Label,
                         LawUnitId = x.LawUnitId,
                     });
                }
        }
        public IQueryable<DeliveryItemVM> DeliveryItemTransSelect(DeliveryItemTransFilterVM filter)
        {
            var deliveryItems = DeliveryItemTransSelectQuery(filter, false);
            
                return deliveryItems
                  .Select(x => new DeliveryItemVM()
                  {
                      Id = x.Id,
                      FromCourtName = x.FromCourt.Label,
                      CourtName = x.Court.Label,
                      LawUnitName = x.LawUnit.FullName,
                      AreaName = x.DeliveryArea.Description,
                      PersonName = x.PersonName,
                      FullAddress = x.Address.FullAddress,
                      AddressPhone = x.Address.Phone,
                      AddressEmail = x.Address.Email,
                      AddressFax = x.Address.Fax,
                      StateName = x.NotificationState.Label,
                      RegNumber = x.RegNumber,
                      DateSend = x.DateSend,
                      DateAccepted = x.DateAccepted,
                      DeliveryDate = x.DeliveryDate,
                      CourtId = x.CourtId,
                      FromCourtId = x.FromCourtId,
                      LawUnitId = x.LawUnitId,
                      CheckRow = false,
                      CaseInfo = x.DocumentNotificationId != null ? $"Документ {x.CaseInfo}" : x.CaseInfo,
                      DeliveryAreaId = x.DeliveryAreaId,
                      CheckRowOrder = "1Z" + x.RegNumber,
                      NotificationType = x.NotificationType.Label,
                      CaseNotificationId = x.CaseNotificationId,
                      DocumentNotificationId = x.DocumentNotificationId,
                      FastCaseInfo = x.Case.IsFastProcess == true ? NomenclatureConstants.DeliveryItemMessage.FastProcess : string.Empty,
                  });
        }
        public IQueryable<DeliveryItemVM> DeliveryItemChangeLawUnitSelect(DeliveryItemChangeLawUnitVM filterData, int[] newLawUnitId)
        {
            int[] states = NomenclatureConstants.NotificationState.NotificationEndState();
            var deliveries = repo.AllReadonly<DeliveryItem>()
                .Where(IsNotExpired())
                .Where(x => x.CourtId == filterData.CourtId)
                .Where(x => filterData.NotificationTypeId <= 0 || x.NotificationTypeId == filterData.NotificationTypeId)
                .Where(x => x.CaseNotificationId == null || x.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.WithSummons);
            if (filterData.NotificationStateId <= 0)
            {
                deliveries = deliveries.Where(x => !states.Contains(x.NotificationStateId));
            }
            else
            {
                deliveries = deliveries.Where(x => x.NotificationStateId == filterData.NotificationStateId);
            }
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
                LawUnitName = x.LawUnit.FullName,
                AreaName =  x.DeliveryArea.Description,
                PersonName = x.PersonName,
                FullAddress = x.Address.FullAddress,
                AddressPhone = x.Address.Phone,
                AddressEmail = x.Address.Email,
                AddressFax = x.Address.Fax,
                StateName = x.NotificationState.Label,
                RegNumber = x.RegNumber,
                DateSend = x.DateSend,
                DateAccepted = x.DateAccepted,
                DeliveryDate = x.DeliveryDate,
                CourtId = x.CourtId,
                FromCourtId = x.FromCourtId,
                LawUnitId = x.LawUnitId,
                DeliveryAreaId = x.DeliveryAreaId,
                CheckRow = false,
                NotificationType = x.NotificationType.Label,
                CaseNotificationId = x.CaseNotificationId,
                DocumentNotificationId = x.DocumentNotificationId
            })
                .AsQueryable();
            return result;
        }
        public async Task<List<Select2ItemVM>> DeliveryItemTransForIdDDL(DeliveryItemTransFilterVM filter)
        {
            var list = DeliveryItemTransSelectRecap(filter);
            switch (filter.ToNotificationStateId)
            {
                case NomenclatureConstants.NotificationState.Send:
                    var recapListSend =await list
                              .GroupBy(x => new { x.CourtId, x.CourtName })
                              .Select(gr => new DeliveryItemRecapVM()
                              {
                                  Id = gr.Key.CourtId,
                                  Name = gr.Key.CourtName,
                                  Count = gr.Count()
                              }).ToListAsync();
                    return RecapToSelect2ItemList(recapListSend, true);
                case NomenclatureConstants.NotificationState.Received:
                    var recapListReceived = await list
                              .GroupBy(x => new { x.FromCourtId, x.FromCourtName })
                              .Select(gr => new DeliveryItemRecapVM()
                              {
                                  Id = gr.Key.FromCourtId,
                                  Name = gr.Key.FromCourtName,
                                  Count = gr.Count()
                              })
                              .ToListAsync();
                    return RecapToSelect2ItemList(recapListReceived, true);
                case NomenclatureConstants.NotificationState.ForDelivery:
                    var recapListForDelivery = await list
                              .GroupBy(x => new { x.LawUnitId, x.LawUnitName })
                              .Select(gr => new DeliveryItemRecapVM()
                              {
                                  Id = (gr.Key.LawUnitId > 0 ? gr.Key.LawUnitId : -2) ?? 0,
                                  Name = gr.Key.LawUnitId > 0 ? gr.Key.LawUnitName : " БЕЗ ИЗБРАН ПРИЗОВКАР",
                                  Count = gr.Count()
                              })
                              .ToListAsync();
                    return RecapToSelect2ItemList(recapListForDelivery, false);
            }
            return new List<Select2ItemVM>();
        }

        private static List<Select2ItemVM> RecapToSelect2ItemList(List<DeliveryItemRecapVM> recapList, bool addAll)
        {
            var result = recapList
                        .Select(r => new Select2ItemVM()
                        {
                            Text = $"{r.Name}  {r.Count}",
                            Id = r.Id
                        })
                      .OrderBy(x => x.Text)
                      .ToList();
            if (addAll)
            {
                result.Insert(0, new Select2ItemVM()
                {
                    Id = 0,
                    Text = $"Всички {recapList.Sum(x => (int?)x.Count) ?? 0}"
                });
            }
            return result;
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
                .Include(x => x.CaseNotification)
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

        public DeliveryItem GetDeliveryItemByMediationNotificationId(int notificationId)
        {
            return repo.AllReadonly<DeliveryItem>()
                .Where(x => x.MediationNotificationId == notificationId)
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
            if (deliverOperId == NomenclatureConstants.NotificationState.Delivered)
                deliverOperId = NomenclatureConstants.DeliveryOper.Visit1;
            int? operId = repo.AllReadonly<DeliveryOper>()
                              .Where(x => x.Id == deliverOperId)
                              .Select(x => (int?)x.Id)
                              .FirstOrDefault();
            if (operId == null)
                return null;
            DeliveryItemOper oper = new DeliveryItemOper();
            oper.CourtId = deliveryItem.CourtId;
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

        public async Task<bool> DeliveryItemSaveDataAddReceived(DeliveryItem model, DeliveryLogVM logVM)
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
                    saved.ReturnDate = model.ReturnDate;
                    saved.RegNumber = model.RegNumber;
                    saved.RegDate = model.RegDate;
                    saved.CaseNotificationId = model.CaseNotificationId;
                    saved.CaseId = model.CaseId;
                    saved.CaseSessionId = model.CaseSessionId;
                    saved.DeliveryAreaId = model.DeliveryAreaId;
                    saved.NotificationStateId = model.NotificationStateId;
                    saved.NotificationDeliveryGroupId = model.NotificationDeliveryGroupId ?? NomenclatureConstants.NotificationDeliveryGroup.WithSummons;
                    saved.LawUnitId = model.LawUnitId;
                    saved.PersonName = model.PersonName;
                    saved.AddressId = model.AddressId;
                    saved.CaseInfo = model.CaseInfo;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    nomenclatureService.SetFullAddress(saved.Address);
                    var oper = CreateDeliveryItemOper(saved, model.NotificationStateId);
                    await SetDeliveryItemDates(saved, oper);
                    repo.Update(saved);
                    repo.SaveChanges();
                    // DeliveryLog
                    CreateDeliveryItemOperLog(saved, oper, logVM);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    model.NotificationDeliveryGroupId = model.NotificationDeliveryGroupId ?? NomenclatureConstants.NotificationDeliveryGroup.WithSummons;
                    model.NotificationStateId = NomenclatureConstants.NotificationState.Received;
                    model.CourtId = userContext.CourtId;
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    nomenclatureService.SetFullAddress(model.Address);
                    var oper = CreateDeliveryItemOper(model, model.NotificationStateId);
                    await SetDeliveryItemDates(model, oper);
                    repo.Add<DeliveryItem>(model);
                    repo.SaveChanges();

                    // DeliveryLog
                    CreateDeliveryItemOperLog(model, oper, logVM);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на DeliveryItem с Id={model.Id} DeliveryItemSaveDataAddReceived");
                return false;
            }
        }
        public async Task<bool> SaveTrans(int[] deliveryItemIds, int notificationStateId, int deliverOperId, DeliveryLogVM logVM)
        {
            try
            {
                    foreach (int id in deliveryItemIds)
                    {
                        var deliveryItem = await repo.GetByIdAsync<DeliveryItem>(id);
                        if (deliveryItem == null)
                        {
                            return false;
                        }
                        deliveryItem.NotificationStateId = notificationStateId;
                        DeliveryItemOper oper = CreateDeliveryItemOper(deliveryItem, deliverOperId);
                        await UpdateOperToNotification(deliveryItem, oper);
                        await CreateDeliveryItemOperLogExAsync(deliveryItem, oper, logVM, userContext.CourtId, userContext.UserId);
                    }
                    await repo.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return false;
            }
        }
        private string ChangeLawUnitNewDataAuditInfo(DeliveryItemChangeLawUnitVM filterData)
        {
            var changeInfo = " сменено на ";
            var toCourt = repo.AllReadonly<Court>()
                                      .FirstOrDefault(x => x.Id == filterData.NewCourtId);
            if (toCourt != null)
            {
                changeInfo += " " + toCourt.Label;
            }
            if (filterData.NewLawUnitId != -1)
            {
                var delivererName = FromDelivererSpecialName(filterData.NewLawUnitId);
                if (delivererName == null)
                {
                    var lawUnit = repo.AllReadonly<LawUnit>()
                                      .Where(x => x.Id == filterData.NewLawUnitId)
                                      .FirstOrDefault();
                    delivererName = lawUnit?.FullName;
                }
                if (delivererName != null)
                    changeInfo += " " + delivererName;
            };
            if (filterData.NewDeliveryAreaId != -1)
            {
                var deliveryArea = repo.AllReadonly<DeliveryArea>()
                                       .Where(x => x.Id == filterData.NewDeliveryAreaId)
                                       .FirstOrDefault();
                if (deliveryArea != null)
                    changeInfo += " " + deliveryArea.Description;
            };
            return changeInfo;

        }
        public string ChangeLawUnitAuditInfo(DeliveryItemChangeLawUnitVM filterData)
        {
            var changeInfo = "От";

            var fromCourt = repo.AllReadonly<Court>()
                                .FirstOrDefault(x => x.Id == filterData.CourtId);
            if (fromCourt != null)
            {
                changeInfo += " " + fromCourt.Label;
            }

            if (filterData.LawUnitId != -1 && filterData.LawUnitId != null)
            {
                var delivererName = FromDelivererSpecialName(filterData.LawUnitId ?? 0);
                if (delivererName == null)
                {
                    var lawUnit = repo.AllReadonly<LawUnit>()
                                      .Where(x => x.Id == filterData.LawUnitId)
                                      .FirstOrDefault();
                    delivererName = lawUnit?.FullName;
                }
                if (delivererName != null)
                    changeInfo += " " + delivererName;
            };

            if (filterData.DeliveryAreaId != -1 && filterData.DeliveryAreaId != null)
            {
                var deliveryArea = repo.AllReadonly<DeliveryArea>()
                                       .Where(x => x.Id == filterData.DeliveryAreaId)
                                       .FirstOrDefault();
                if (deliveryArea != null)
                    changeInfo += " " + deliveryArea.Description;
            };

            changeInfo += ChangeLawUnitNewDataAuditInfo(filterData);
            return changeInfo;
        }

        public async Task<bool> SaveChangeLawUnit(int[] deliveryItemIds, DeliveryItemChangeLawUnitVM filterData, DeliveryLogVM logVM)
        {

            foreach (int id in deliveryItemIds)
            {
                var deliveryItem = await repo.GetByIdAsync<DeliveryItem>(id);
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
                await CreateDeliveryItemOperLogExAsync(deliveryItem, null, logVM, userContext.CourtId, userContext.UserId);
            }
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<(DeliveryItemRecieveVM, string)> SaveRecieved(string regNumber, bool saveIfErr, DeliveryLogVM logVM)
        {
            DeliveryItemRecieveVM deliveryVM = null;
            var messageErr = string.Empty;
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
                    deliveryItem.DateWrt = DateTime.Now;
                    deliveryItem.UserId = userContext.UserId;
                    var oper = CreateDeliveryItemOper(deliveryItem, deliveryItem.NotificationStateId);
                    await SetDeliveryItemDates(deliveryItem, oper);
                    repo.Update(deliveryItem);
                    CreateDeliveryItemOperLog(deliveryItem, null, logVM);
                    repo.SaveChanges();
                }
            }
            else
            {
                messageErr = $"Няма призовка с номер {regNumber}";
            }
            return (deliveryVM, messageErr);
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
        public bool DeliveryItemSaveArea(int id, int courtId, int? deliveryAreaId, int? lawUnitId, DeliveryLogVM logVM)
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
                CreateDeliveryItemOperLog(saved, null, logVM);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на райониране DeliveryItemId={id}");
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
        public async Task<bool> DeliveryItemSaveOper(DeliveryItemOperVM model, DeliveryLogVM logVM)
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
                    await UpdateOperToNotification(saved, oper);
                    repo.Update(saved);
                    repo.SaveChanges();

                    CreateDeliveryItemOperLog(saved, oper, logVM);
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
                logger.LogError(ex, $"Грешка при запис на операция за разнос към DeliveryItem Id={model.DeliveryItemId}");
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

            if (NomenclatureConstants.NotificationState.NotificationDelivered().Contains(deliveryItem.NotificationStateId))
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
        private async Task<bool> UpdateOperToNotification(DeliveryItem deliveryItem, DeliveryItemOper deliveryItemOper)
        {
            if (deliveryItem.DocumentNotificationId != null)
                return UpdateOperToDocumentNotification(deliveryItem, deliveryItemOper);
            var deliveryItemOperL = repo.AllReadonly<DeliveryItemOper>()
                                     .Where(x => x.DeliveryItemId == deliveryItem.Id &&
                                                 x.DeliveryOperId >= deliveryItemOper.DeliveryOperId &&
                                                 x.DeliveryOperId >= NomenclatureConstants.DeliveryOper.Visit1)
                                     .OrderBy(x => x.DeliveryOperId)
                                     .ThenBy(x => x.Id)
                                     .LastOrDefault();
            if (deliveryItemOperL == null || deliveryItemOperL.DeliveryOperId == deliveryItemOper.DeliveryOperId)
            {
                await SetDeliveryItemDates(deliveryItem, deliveryItemOper);
            }
            else
            {
                await SetDeliveryItemDates(deliveryItem, deliveryItemOperL);
            }
            if (deliveryItem.CaseNotificationId == null)
                return false;
            var caseNotification = repo.GetById<CaseNotification>(deliveryItem.CaseNotificationId);
            caseNotification.NotificationStateId = deliveryItem.NotificationStateId;
            caseNotification.DeliveryReasonId = deliveryItemOper.DeliveryReasonId;
            if (deliveryItem.NotificationStateId == NomenclatureConstants.NotificationState.Received)
                caseNotification.DateAccepted = deliveryItemOper.DateOper;
            if (deliveryItem.NotificationStateId == NomenclatureConstants.NotificationState.Send)
                caseNotification.DateSend = deliveryItemOper.DateOper;

            if (NomenclatureConstants.NotificationState.NotificationDelivered().Contains(deliveryItem.NotificationStateId))
                caseNotification.DeliveryDate = deliveryItemOper.DateOper;

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
            int[] states = NomenclatureConstants.NotificationState.NotificationStateEndAndVisited();
            int[] opers = GetDeliveryOperMobile().Select(x => x.Id).ToArray();
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
                                                   (x.CaseNotification.HaveАppendix == true ? " +прил." : "") + " " +
                                                   (x.CaseSession.DateTo != null ? x.CaseSession.DateTo.Value.ToString(FormattingConstant.NormalDateFormat) : "")
                                                   //+" "+ ((x.DateAccepted ?? x.DateSend) ?? x.RegDate).Value.ToString(FormattingConstant.NormalDateFormat)
                                                   ,
                           CaseInfo = x.CaseInfo ?? "",
                           PersonName = x.PersonName,
                           Address = x.Address == null ? "" : x.Address.FullAddressNotification(),
                           DeliveryInfo = x.DeliveryInfo,
                           ReturnDate = x.ReturnDate,
                           NotificationState = states.Contains(x.NotificationStateId) ? x.NotificationState.Label ?? "" : "",
                           DeliveryDate = x.DeliveryDate,
                           DeliveryItemOpers = x.DeliveryItemOpers.Where(x =>  opers.Contains(x.DeliveryOperId)).ToList()
                       })
                       .AsQueryable();

            //x.DeliveryItemOpers.OrderBy(o => o.DeliveryOperId).ThenBy(o => o.Id).LastOrDefault()
        }
      
        public IQueryable<DeliveryItemReportResultVM> GetDeliveryItemReportResult(DeliveryItemListVM filter)
        {
            DateTime nullDate = new DateTime(2000, 1, 1);
            int[] states = NomenclatureConstants.NotificationState.NotificationStateEndAndVisited();
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
                                          ((x.CaseSession.SessionType.Label ?? "") != "" ? " от " : " ") +
                                           (x.CaseSession.SessionType.Label ?? "").Trim() + " " +
                                          x.CaseSession.DateFrom.ToString(FormattingConstant.NormalDateFormat),
                           HtmlTemplateName = x.HtmlTemplate.Label,
                           StateName = x.NotificationState == null ? "" : x.NotificationState.Label,
                           DateResult = x.DeliveryDate,
                           DateResultStr = x.ReturnDate == null ? "" : x.ReturnDate.Value.ToString(FormattingConstant.NormalDateFormatHHMM),
                           ReasonReturn = x.NotificationStateId != NomenclatureConstants.NotificationState.Delivered ?
                                          (x.DeliveryItemOpers.OrderBy(o => o.DeliveryOperId).ThenBy(o => o.Id).LastOrDefault().DeliveryReason.Label ?? "") + " " + x.DeliveryInfo :
                                          "",
                           PersonName = x.PersonName,
                           Address = x.Address == null ? "" : x.Address.FullAddressNotification()
                       })
                       .AsQueryable();
        }
        public IQueryable<DeliveryItemReturnNewVM> GetDeliveryItemReportResultNew(DeliveryItemListVM filter)
        {
            DateTime nullDate = new DateTime(2000, 1, 1);
            int[] states = NomenclatureConstants.NotificationState.NotificationStateEndAndVisited();
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
                           DateAccepted = (x.DateAccepted ?? x.RegDate),
                           CaseRegNumber = x.CaseInfo,
                           LawUnitName = x.LawUnit.FullName,
                           PersonName = x.PersonName,
                           DateToLawUnit = deliveryItemOper.Where(o => o.DeliveryItemId == x.Id).Max(d => (DateTime?)d.DateOper),
                           NotificationState = x.NotificationState.Label,
                           DeliveryInfo = x.DeliveryInfo,
                           DeliveryDate = x.DeliveryItemOpers.Max(op => (DateTime?)op.DateOper), //x.DeliveryDate,
                           ReturnReason = x.NotificationStateId != NomenclatureConstants.NotificationState.Delivered ?
                                          (x.DeliveryItemOpers.OrderBy(o => o.DeliveryOperId).ThenBy(o => o.Id).LastOrDefault().DeliveryReason.Label ?? "") : "",
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
            int[] states = NomenclatureConstants.NotificationState.NotificationEndState475051();
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
                    NotificationDeliveryGroupId = x.NotificationDeliveryGroupId
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
                var deliveryItemOper = delivery.DeliveryItemOpers.OrderBy(o => o.DeliveryOperId).ThenBy(o => o.Id).LastOrDefault();
                if (deliveryItemOper != null)
                {
                    var oper = opers.Where(x => x.Id == deliveryItemOper.DeliveryOperId).FirstOrDefault();
                    if (oper != null)
                    {
                        delivery.DeliveryInfo += oper.Label + " " +
                                                 deliveryItemOper.DateOper.ToString(FormattingConstant.NormalDateFormat) + " " +
                                                 delivery.NotificationState + " ";
                        if (deliveryItemOper?.DeliveryReasonId > 0)
                        {
                            var reason = reasons.FirstOrDefault(x => x.Id == deliveryItemOper?.DeliveryReasonId);
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
            int[] states = NomenclatureConstants.NotificationState.NotificationEndState();
            var result = repo.AllReadonly<NotificationState>()
                .Where(x => operState.Any(o => o.NotificationStateId == x.Id) || x.Id == NomenclatureConstants.NotificationState.ForDelivery)
                .Select(x => new MobileValueLabelGroupVM()
                {
                    label = x.Label,
                    value = x.Id.ToString(),
                    orderNumber = x.OrderNumber,
                })
                .ToList();
            foreach (var item in result)
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
            int[] states = NomenclatureConstants.NotificationState.NotificationForDelivery();
            int[] statesVisit = NomenclatureConstants.NotificationState.NotificationForVisit();
            DateTime dateWD = fromDate ?? DateTime.Now;
            dateWD = dateWD.AddDays(-365);
            DateTime dateWDTo = (toDate ?? DateTime.Now).Date;
            var dictWD = workingDaysService.GetWorkingDays(courtId, dateWD, dateWDTo);
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
                                  .Where(o => o.DeliveryOperId > 20)
                                  //.GroupBy(gr => gr.DeliveryOperId)
                                  //.Select(gr => gr.Key)
                                  .Max(o => (int?)(o.DeliveryOperId - 20)) ?? 0,
                FirstVisit = x.DeliveryItemOpers.Where(o =>
                   statesVisit.Contains(o.NotificationStateId) &&
                   o.DeliveryOperId <= NomenclatureConstants.DeliveryOper.Visit2)
                   .Min(o => (DateTime?)o.DateOper),
                LastVisit = x.DeliveryItemOpers.Where(
                    o => statesVisit.Contains(o.NotificationStateId) &&
                    o.DeliveryOperId <= NomenclatureConstants.DeliveryOper.Visit2)
                .Max(o => (DateTime?)o.DateOper),
                HaveHolidayVisit = listWD.Where(wd => x.DeliveryItemOpers.Where(o =>
                  statesVisit.Contains(o.NotificationStateId) &&
                  o.DateOper.Date == wd).Any()).Any(),
                NotificationTypeId = x.NotificationTypeId ?? 0
            })
                .ToList();
            return result;
        }
        public async Task<bool> DeliveryItemSaveOperMobile(DeliveryItemVisitMobile model)
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
                var states = repo.AllReadonly<DeliveryOperState>();
                var opers = repo.AllReadonly<DeliveryOper>()
                                .Where(x => states.Any(s => s.DeliveryOperId == x.Id))
                                .OrderBy(x => x.Id)
                                .ToList();
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
                //oper.CourtId = model.CourtId;
                saved.DeliveryReasonId = oper.DeliveryReasonId;
                saved.DeliveryInfo = "";
                saved.DateWrt = DateTime.Now;
                saved.UserId = model.UserId;

                await UpdateOperToNotification(saved, oper);
                repo.Update(saved);
                model.IsOK = true;
                repo.Update(model);
                repo.SaveChanges();

                var logVM = new DeliveryLogVM()
                {
                    IsFromMobile = true,
                    Action = "Добавяне",
                    PageLabel = "Посещения въведени през мобилно устройство"
                };
                CreateDeliveryItemOperLogEx(saved, oper, logVM, oper.CourtId, oper.UserId);
                try
                {
                    repo.SaveChanges();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Грешка при запис на log от мобилното устройство Id={model.Id}" + JsonConvert.SerializeObject(model));
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на посещение от мобилното устройство Id={model.Id}" + JsonConvert.SerializeObject(model));
                return false;
            }
        }
        public Court GetCourtById(int courtId)
        {
            return repo.GetById<Court>(courtId);
        }

        private string FromDelivererSpecialName(int lawUnitId)
        {
            if (lawUnitId == 0)
                return "Без избран призовкар";
            if (lawUnitId == -2)
                return "Без деистващ призовкар";
            return null;
        }
        public List<SelectListItem> LawUnitForCourt_SelectDdlAllInDeliveryItem(int forCourtId, List<SelectListItem> newLawUnits)
        {
            int[] states = NomenclatureConstants.NotificationState.NotificationEndState();
            var deliveries = repo.AllReadonly<DeliveryItem>()
                             .Include(x => x.LawUnit)
                             .Where(x => x.CourtId == forCourtId)
                             .Where(x => !states.Contains(x.NotificationStateId))
                             .Where(x => x.LawUnitId != null)
                             .Where(x => x.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.WithSummons);
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

            result.Insert(0, new SelectListItem() { Text = FromDelivererSpecialName(0), Value = "0" });
            result.Insert(0, new SelectListItem() { Text = FromDelivererSpecialName(-2), Value = "-2" });
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
        private int[] NotificationDeliveryGroup(int filterType)
        {
            var result = new List<int>
            {
                NomenclatureConstants.NotificationDeliveryGroup.WithSummons,
                NomenclatureConstants.NotificationDeliveryGroup.WithCourier,
                NomenclatureConstants.NotificationDeliveryGroup.WithCityHall ,
                NomenclatureConstants.NotificationDeliveryGroup.WithRegistry,
                NomenclatureConstants.NotificationDeliveryGroup.WithSecurity
            };
            if (filterType == NomenclatureConstants.DeliveryItemFilterType.Inner)
            {
                result.Add(NomenclatureConstants.NotificationDeliveryGroup.ByEPEP);
                result.Add(NomenclatureConstants.NotificationDeliveryGroup.OnEMail);
                result.Add(NomenclatureConstants.NotificationDeliveryGroup.ByRNFL);
            }
            return result.ToArray();
        }
        public List<SelectListItem> NotificationDeliveryGroupSelect(int filterType)
        {
            var notificationGroups = NotificationDeliveryGroup(filterType);
            var result = repo.AllReadonly<NotificationDeliveryGroup>()
                             .Where(x => notificationGroups.Any(n => n == x.Id))
                       .Select(x => new SelectListItem()
                       {
                           Text = x.Label,
                           Value = x.Id.ToString()
                       }).ToList() ?? new List<SelectListItem>();
            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });

            return result;
        }
        public async Task<bool> DeliveryItemSaveState(int deliveryItemId, int notificationStateId, DateTime? deliveryDate, string deliveryInfo, DeliveryLogVM logVM)
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
                saved.DeliveryInfo = deliveryInfo;
                saved.DateWrt = DateTime.Now;
                saved.UserId = userContext.UserId;
                await UpdateOperToNotification(saved, oper);
                repo.Update(saved);
                repo.SaveChanges();

                CreateDeliveryItemOperLog(saved, oper, logVM);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на статус на призовка deliveryItemId={deliveryItemId}");
                return false;
            }
        }
        public List<Select2ItemVM> GetCourtsSelect2(List<DeliveryArea> deliveryAreaList)
        {
            var courtIds = deliveryAreaList.Select(x => x.CourtId).ToArray();
            var blankDeliveryAreaList = !deliveryAreaList.Any();
            var result = repo.AllReadonly<Court>()
                        .Where(x => courtIds.Contains(x.Id) || blankDeliveryAreaList)
                        .OrderBy(x => x.Label)
                        .Select(x => new Select2ItemVM()
                        {
                            Text = x.Label,
                            Id = x.Id
                        }).ToList() ?? new List<Select2ItemVM>();
            result.Insert(0, new Select2ItemVM() { Text = "Избери", Id = -1 });
            return result;
        }
        public void CreateDeliveryItemOperLog(
           DeliveryItem deliveryItem,
           DeliveryItemOper deliveryItemOper,
           DeliveryLogVM logVM)
        {
            CreateDeliveryItemOperLogEx(deliveryItem, deliveryItemOper, logVM, userContext.CourtId, userContext.UserId);
        }
        public void CreateDeliveryItemOperLogEx(
            DeliveryItem deliveryItem,
            DeliveryItemOper deliveryItemOper,
            DeliveryLogVM logVM,
            int courtId,
            string userId)
        {
            try
            {
                if (deliveryItem == null)
                    return;
                var operLog = new DeliveryItemOperLog();
                operLog.CourtWrtId = courtId;
                operLog.DateWrt = DateTime.Now;
                operLog.UserId = userId;
                if (deliveryItemOper != null)
                {
                    operLog.DeliveryItemOperId = deliveryItemOper.Id;
                    operLog.DeliveryOperId = deliveryItemOper.DeliveryOperId;
                    operLog.DateOper = deliveryItemOper.DateOper;
                    operLog.Long = deliveryItemOper.Long;
                    operLog.Lat = deliveryItemOper.Lat;
                    operLog.LawUnitId = deliveryItemOper.LawUnitId;
                    operLog.DeliveryAreaId = deliveryItemOper.DeliveryAreaId;
                    operLog.NotificationStateId = deliveryItemOper.NotificationStateId;
                }
                else
                {
                    operLog.DateOper = deliveryItem.DateWrt;
                    operLog.NotificationStateId = deliveryItem.NotificationStateId;
                    operLog.DeliveryAreaId = deliveryItem.DeliveryAreaId;
                    operLog.LawUnitId = deliveryItem.LawUnitId;
                }
                operLog.AddressStr = deliveryItem.Address?.FullAddress;
                if (string.IsNullOrEmpty(operLog.AddressStr) && deliveryItem.AddressId > 0)
                {
                    var address = repo.AllReadonly<Address>()
                                      .Where(x => x.Id == deliveryItem.AddressId)
                                      .FirstOrDefault();

                    operLog.AddressStr = address?.FullAddress;
                }
                operLog.PersonName = deliveryItem.PersonName;
                operLog.RegNumber = deliveryItem.RegNumber;
                operLog.RegDate = deliveryItem.RegDate;
                operLog.CaseInfo = deliveryItem.CaseInfo;
                operLog.CaseNotificationId = deliveryItem.CaseNotificationId;
                operLog.DocumentNotificationId = deliveryItem.DocumentNotificationId;
                operLog.DeliveryItemId = deliveryItem.Id;
                operLog.DeliveryInfo = deliveryItem.DeliveryInfo;
                operLog.FromCourtId = deliveryItem.FromCourtId;
                operLog.ToCourtId = deliveryItem.CourtId;
                operLog.Action = logVM.Action;
                operLog.PageUrl = logVM.PageUrl;
                operLog.PageLabel = logVM.PageLabel;
                operLog.IsFromMobile = logVM.IsFromMobile;
                operLog.NotificationDeliveryGroupId = deliveryItem.NotificationDeliveryGroupId;
                repo.Add(operLog);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при CreateDeliveryItemOperLog");
            }
        }

        public async Task CreateDeliveryItemOperLogExAsync(
            DeliveryItem deliveryItem,
            DeliveryItemOper deliveryItemOper,
            DeliveryLogVM logVM,
            int courtId,
            string userId)
        {
            try
            {
                if (deliveryItem == null)
                    return;
                var operLog = new DeliveryItemOperLog();
                operLog.CourtWrtId = courtId;
                operLog.DateWrt = DateTime.Now;
                operLog.UserId = userId;
                if (deliveryItemOper != null)
                {
                    operLog.DeliveryItemOper = deliveryItemOper;
                    operLog.DeliveryOperId = deliveryItemOper.DeliveryOperId;
                    operLog.DateOper = deliveryItemOper.DateOper;
                    operLog.Long = deliveryItemOper.Long;
                    operLog.Lat = deliveryItemOper.Lat;
                    operLog.LawUnitId = deliveryItemOper.LawUnitId;
                    operLog.DeliveryAreaId = deliveryItemOper.DeliveryAreaId;
                    operLog.NotificationStateId = deliveryItemOper.NotificationStateId;
                }
                else
                {
                    operLog.DateOper = deliveryItem.DateWrt;
                    operLog.NotificationStateId = deliveryItem.NotificationStateId;
                    operLog.DeliveryAreaId = deliveryItem.DeliveryAreaId;
                    operLog.LawUnitId = deliveryItem.LawUnitId;
                }
                operLog.AddressStr = deliveryItem.Address?.FullAddress;
                if (string.IsNullOrEmpty(operLog.AddressStr) && deliveryItem.AddressId > 0)
                {
                    var address = repo.AllReadonly<Address>()
                                      .Where(x => x.Id == deliveryItem.AddressId)
                                      .FirstOrDefault();

                    operLog.AddressStr = address?.FullAddress;
                }
                operLog.PersonName = deliveryItem.PersonName;
                operLog.RegNumber = deliveryItem.RegNumber;
                operLog.RegDate = deliveryItem.RegDate;
                operLog.CaseInfo = deliveryItem.CaseInfo;
                operLog.CaseNotificationId = deliveryItem.CaseNotificationId;
                operLog.DocumentNotificationId = deliveryItem.DocumentNotificationId;
                operLog.DeliveryItemId = deliveryItem.Id;
                operLog.DeliveryInfo = deliveryItem.DeliveryInfo;
                operLog.FromCourtId = deliveryItem.FromCourtId;
                operLog.ToCourtId = deliveryItem.CourtId;
                operLog.Action = logVM.Action;
                operLog.PageUrl = logVM.PageUrl;
                operLog.PageLabel = logVM.PageLabel;
                operLog.IsFromMobile = logVM.IsFromMobile;
                operLog.NotificationDeliveryGroupId = deliveryItem.NotificationDeliveryGroupId;
                await repo.AddAsync(operLog);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при CreateDeliveryItemOperLogAsync");
            }
        }

        public List<DeliveryItemOperLogVM> SelectLog(int deliveryItemId)
        {
            var operIds = NomenclatureConstants.DeliveryOper.Visits();
            var courts = repo.AllReadonly<Court>();
            var groups = repo.AllReadonly<NotificationDeliveryGroup>();
            var opers = repo.AllReadonly<DeliveryOper>()
                            .Where(x => operIds.Contains(x.Id));
            return repo.AllReadonly<DeliveryItemOperLog>()
                       .Where(x => x.DeliveryItemId == deliveryItemId)
                       .Select(x => new DeliveryItemOperLogVM()
                       {
                           DateWrt = x.DateWrt,
                           UserName = x.User.LawUnit.FullName + " " + (x.CourtWrtId == userContext.CourtId ? "" : x.CourtWrt.Label),
                           Action = x.Action,
                           PageLabel = x.PageLabel,
                           FromCourtName = courts.Where(c => c.Id == x.FromCourtId).Select(c => c.Label).FirstOrDefault(),
                           ToCourtName = courts.Where(c => c.Id == x.FromCourtId).Select(c => c.Label).FirstOrDefault(),
                           LawUnitName = x.LawUnitId == null ?
                                         groups.Where(g => g.Id == x.NotificationDeliveryGroupId).Select(g => g.Label).FirstOrDefault() :
                                         x.LawUnit.FullName,
                           AreaName = x.DeliveryArea == null ? "" : x.DeliveryArea.Description,
                           PersonName = x.PersonName,
                           Address = x.AddressStr,
                           StateName = x.NotificationState == null ? "" : x.NotificationState.Label,
                           RegNumber = x.RegNumber,
                           CaseInfo = x.CaseInfo,
                           DateOper = (operIds.Contains(x.DeliveryOperId) ? x.DateOper : null),
                           OperName = opers.Where(o => o.Id == x.DeliveryOperId).Select(o => o.Label).FirstOrDefault()
                       })
                       .OrderBy(x => x.DateWrt)
                       .ToList();
        }

        public async Task SetDeliveryItemDates(DeliveryItem deliveryItem, DeliveryItemOper oper)
        {
            if (oper == null || deliveryItem == null)
                return;
            if (deliveryItem.DateSend == null && deliveryItem.NotificationStateId != NomenclatureConstants.NotificationState.Ready)
            {
                deliveryItem.DateSend = oper.DateOper;
            }
            if (deliveryItem.DateAccepted == null &&
                deliveryItem.NotificationStateId != NomenclatureConstants.NotificationState.Ready &&
                deliveryItem.NotificationStateId != NomenclatureConstants.NotificationState.Send
                )
            {
                deliveryItem.DateAccepted = oper.DateOper;
                //if (deliveryItem.CaseNotificationId != null)
                //{
                //   await workNotificationService.SaveNotificationsForReceivedMessageDeliveryFastProcess(deliveryItem.CaseNotificationId ?? 0, oper.DateOper, false);
                //}
            }

            if (deliveryItem.NotificationStateId != NomenclatureConstants.NotificationState.Ready)
            {
                deliveryItem.DeliveryDate = oper.DateOper;
            }

            if (deliveryItem.PreparedById == null)
            {
                deliveryItem.PreparedById = repo.AllReadonly<ApplicationUser>().Where(x => x.Id == oper.UserId).FirstOrDefault()?.LawUnitId;
            }

            var notificationId = deliveryItem.CaseNotificationId ?? 0;

            bool isFastProcess = await repo.GetPropByIdAsync<Case, bool>(x => x.Id == deliveryItem.CaseId, x => x.IsFastProcess ?? false);

            if (isFastProcess)
            {
                if ((deliveryItem.CaseNotificationId ?? 0) > 0 && deliveryItem.NotificationStateId == NomenclatureConstants.NotificationState.Received)
                {
                    await workNotificationService.SaveNotificationsForNoticeServiceReceivedFastProcess(notificationId, oper.DateOper, false);
                }

                if ((deliveryItem.CaseNotificationId ?? 0) > 0 && deliveryItem.NotificationStateId == NomenclatureConstants.NotificationState.UnDelivered)
                {
                    await workNotificationService.SaveNotificationsForNotDeliveredMessageDeliveryFastProcess(deliveryItem.CaseNotificationId ?? 0, oper.DateOper, false);
                    await caseDeadlineService.CompleteExpiredUnreturnedMessageFastProcess(notificationId, false);
                }

                if (notificationId > 0 && deliveryItem.NotificationStateId == NomenclatureConstants.NotificationState.Delivered)
                {
                    await workNotificationService.SaveNotificationsForLackSubmittedObjectionFastProcess(notificationId, oper.DateOper, false);
                    await workNotificationService.EditDateEventNotificationsForLackSubmittedObjectionFastProcess(notificationId, oper.DateOper, false);

                    await workNotificationService.SaveNotificationsForAppealActFastProcess(notificationId, oper.DateOper, false);

                    await workNotificationService.SaveNotificationsForExpressingOpinionObjectionFastProcess(notificationId, oper.DateOper, false);
                    await workNotificationService.SaveNotificationsForFilingClaimFastProcess(notificationId, oper.DateOper, false);

                    await workNotificationService.SaveNotificationsForMessageDeliveredCaseFastProcess(notificationId, oper.DateOper, false);
                    await workNotificationService.EditDateEventNotificationsForMessageDeliveredCaseFastProcess(notificationId, oper.DateOper, false);

                    await workNotificationService.EditDateEventNotificationsForNoProceduralActionTakenFastProcess(notificationId, oper.DateOper, false);

                    await workNotificationService.EditDateEventNotificationsForExpressingOpinionObjectionFastProcess(notificationId, oper.DateOper, false);

                    await workNotificationService.EditDateEventNotificationsForFilingClaimFastProcess(notificationId, oper.DateOper, false);

                    await caseDeadlineService.CompleteExpiredUnreturnedMessageFastProcess(notificationId, false);
                }
            }

            if (notificationId > 0)
            {
                var workNotification = await workNotificationService.NewWorkNotification(notificationId, deliveryItem.NotificationStateId);
                if (workNotification != null)
                    repo.Update(workNotification);
            }
        }
        public async Task<DeliveryItem> CreateDeliveryItem(CaseNotification notification, bool operIsChanged)
        {
            if (notification.NotificationStateId == NomenclatureConstants.NotificationState.Proekt)
                return null;
            if (!NomenclatureConstants.NotificationDeliveryGroup.DeliveryGroupForDeliveryItem.Contains(notification.NotificationDeliveryGroupId ?? 0))
                return null;
            DeliveryItem deliveryItem = null;
            if (notification.Id > 0)
            {
                deliveryItem = repo.AllReadonly<DeliveryItem>()
                                   .Where(x => x.CaseNotificationId == notification.Id)
                                   .OrderByDescending(x => x.Id)
                                   .FirstOrDefault();
            }
            deliveryItem = deliveryItem ?? new DeliveryItem();
            bool stateIsChanged = (deliveryItem.NotificationStateId != notification.NotificationStateId);
            deliveryItem.FromCourtId = notification.CourtId ?? userContext.CourtId;
            deliveryItem.ReturnDate = notification.ReturnDate;
            deliveryItem.RegNumber = notification.RegNumber ?? "";
            deliveryItem.RegDate = notification.RegDate;
            deliveryItem.CaseNotificationId = notification.Id;
            deliveryItem.CaseId = notification.CaseId;
            deliveryItem.CaseSessionId = notification.CaseSessionId;
            deliveryItem.NotificationStateId = notification.NotificationStateId;
            deliveryItem.NotificationTypeId = notification.NotificationTypeId;
            deliveryItem.NotificationDeliveryGroupId = notification.NotificationDeliveryGroupId;
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
                var oper = CreateDeliveryItemOper(deliveryItem, notification.DeliveryOperId ?? notification.NotificationStateId);
                await SetDeliveryItemDates(deliveryItem, oper);
            }

            notification.DeliveryItems = notification.DeliveryItems ?? new HashSet<DeliveryItem>();
            notification.DeliveryItems.Add(deliveryItem);
            if (deliveryItem.Id > 0)
            {
                //KBorisov: Гърми при attach на deliveryItem.CaseNotification
                // deliveryItem.CaseNotification = null;
                repo.Update(deliveryItem);
            }
            else
                repo.Add(deliveryItem);
            return deliveryItem;
        }
    }
}
