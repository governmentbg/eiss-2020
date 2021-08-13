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
using IOWebApplication.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;
using IOWebApplication.Infrastructure.Constants;
using System.Linq.Expressions;

namespace IOWebApplication.Core.Services
{
    public class DeliveryAreaAddressService : BaseService, IDeliveryAreaAddressService
    {
        public DeliveryAreaAddressService(
            ILogger<DeliveryAreaAddressService> _logger,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }
        private Expression<Func<EkEkatte, string>> EkatteCityName()
        {
            return c => (c.TVM ?? "") + " " + (c.Name ?? "");
        }
        public IQueryable<DeliveryAreaAddressVM> DeliveryAreaAddressSelect(DeliveryAreaAddressFilterVM filter)
        {
            var cities = repo.AllReadonly<EkEkatte>().AsQueryable();
            var streets = repo.AllReadonly<EkStreet>().AsQueryable();
            var numberTypes = repo.AllReadonly<DeliveryNumberType>().AsQueryable();
            var result = repo.AllReadonly<DeliveryAreaAddress>()
                .Where(x => x.DeliveryAreaId == filter.DeliveryAreaId && (
                              ((filter.DateFrom ?? DateTime.MinValue) <= (x.DateFrom ?? DateTime.MinValue) && (x.DateFrom ?? DateTime.MinValue) <= (filter.DateTo ?? DateTime.MaxValue)) ||
                              ((filter.DateFrom ?? DateTime.MinValue) <= (x.DateTo ?? DateTime.MaxValue) && (x.DateTo ?? DateTime.MaxValue) <= (filter.DateTo ?? DateTime.MaxValue)) ||
                              ((x.DateFrom ?? DateTime.MinValue) <= (filter.DateFrom ?? DateTime.MinValue) && (filter.DateFrom ?? DateTime.MinValue) <= (x.DateTo ?? DateTime.MaxValue)) ||
                              ((x.DateFrom ?? DateTime.MinValue) <= (filter.DateTo ?? DateTime.MaxValue) && (filter.DateTo ?? DateTime.MaxValue) <= (x.DateTo ?? DateTime.MaxValue))
                            ) &&
                            (filter.ExpiredType != 0 || (x.DateExpired != null && (filter.DateExpiredFrom ?? DateTime.MinValue) <= (x.DateExpired ?? DateTime.MinValue) && (x.DateExpired ?? DateTime.MinValue) <= (filter.DateExpiredTo ?? DateTime.MaxValue))) &&
                            (filter.ExpiredType != 1 || x.DateExpired == null)
                )
                .Select(x => new DeliveryAreaAddressVM()
                {
                    Id = x.Id,
                    City = cities.Where(c => c.Ekatte == x.CityCode).Select(EkatteCityName()).FirstOrDefault(),
                    Street = streets.Where(s => s.Code == x.StreetCode && s.Ekatte == x.CityCode).Select(c => c.Name).FirstOrDefault(),
                    ResidentionArea = streets.Where(s => s.Code == x.ResidentionAreaCode && s.Ekatte == x.CityCode).Select(c => c.Name).FirstOrDefault(),
                    NumberType = numberTypes.Where(n => n.Id == x.NumberType).Select(c => c.Label).FirstOrDefault(),
                    NumberFrom = x.NumberFrom,
                    NumberTo = x.NumberTo,
                    BlockName = x.BlockName,
                    DateFrom = x.DateFrom,
                    DateTo = x.DateTo,
                    DateExpired = x.DateExpired
                })
                .AsQueryable();
            return result;
        }
        public bool DeliveryAreaAddressSaveData(DeliveryAreaAddress model)
        {
            try
            {
                if (model.NumberType == DeliveryAddressNumberType.BlockName || model.NumberType == DeliveryAddressNumberType.NumberName)
                {
                    model.NumberTo = null;
                }
                else
                {
                    model.BlockName = string.Empty;
                }
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<DeliveryAreaAddress>(model.Id);
                    saved.DeliveryAreaId = model.DeliveryAreaId;
                    saved.ResidentionAreaCode = model.ResidentionAreaCode;
                    saved.StreetCode = model.StreetCode;
                    saved.CityCode = model.CityCode;
                    saved.NumberType = model.NumberType;
                    saved.NumberFrom = model.NumberFrom;
                    saved.NumberTo = model.NumberTo;
                    saved.BlockName = model.BlockName;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo;
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
                    repo.Add<DeliveryAreaAddress>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на адреси към райони за разнос към съд Id={ model.Id }");
                return false;
            }
        }
        public List<SelectListItem> GetEkatteByArea(int deliveryAreaId)
        {
            var CourtRegionId = repo.AllReadonly<DeliveryArea>()
                                   .Where(x => x.Id == deliveryAreaId)
                                   .Include(x => x.Court)
                                   .Select(x => x.Court.CourtRegionId)
                                   .FirstOrDefault();
            return GetEkatte(CourtRegionId);
        }
        public List<SelectListItem> GetEkatteByCourt(int courtId)
        {
            var CourtRegionId = repo.AllReadonly<Court>()
                                  .Where(x => x.Id == courtId)
                                   .Select(x => x.CourtRegionId)
                                  .FirstOrDefault();
            return GetEkatte(CourtRegionId);
        }

        private List<SelectListItem> GetEkatte(int? courtRegionId)
        {
            var regions = repo.AllReadonly<CourtRegionArea>()
                                     .Where(c => c.CourtRegionId == courtRegionId);
            var distrints = repo.AllReadonly<EkDistrict>()
                                     .Where(d => regions.Any(r => r.DistrictCode == d.Ekatte && String.IsNullOrEmpty(r.MunicipalityCode)));
            var municipalities = repo.AllReadonly<EkMunincipality>()
                                     .Where(m => regions.Any(r => r.MunicipalityCode == m.Municipality) ||
                                                 distrints.Any(d => d.DistrictId == m.DistrictId));
            var result = repo.All<EkEkatte>()
                .Include(e => e.Munincipality)
                .Include(e => e.District)
                .Join(municipalities, e => e.MunicipalId, m => m.MunicipalityId, (e, m) => new { e, m })
                .OrderBy(x => x.e.Kind).ThenBy(x => x.e.TVM).ThenBy(x => x.e.Name)
                .Select(x => new SelectListItem()
                {
                    Value = x.e.Ekatte.ToString(),
                    Text = (x.e.TVM ?? "") + " " + (x.e.Name ?? "") + " общ. " + (x.e.Munincipality.Name ?? "")
                }).ToList();
            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }

        private string VerifyNumberEvenOdd(int? Number, int? NumberType)
        {
            string result = "";
            if ((NumberType ?? 0) == DeliveryAddressNumberType.OddNumber || (NumberType ?? 0) == DeliveryAddressNumberType.BlockOdd)
                if (((Number ?? 0) % 2) == 0)
                    return $"{Number} трябва да е нечетно.";
            if ((NumberType ?? 0) == DeliveryAddressNumberType.EvenNumber || (NumberType ?? 0) == DeliveryAddressNumberType.BlockEven)
                if (((Number ?? 0) % 2) != 0)
                    return $"{Number} трябва да е четно.";
            return result;
        }
        public string VerifyNumberFrom(int? NumberFrom, int? NumberType, int? NumberTo)
        {
            if (NumberFrom == null)
                return "";
            string result = VerifyNumberEvenOdd(NumberFrom, NumberType);
            if (NumberTo != null)
                if ((NumberFrom ?? 0) > (NumberTo ?? 0))
                    return $"{NumberFrom} трябва да < от {NumberTo}.";
            return result;
        }
        public string VerifyNumberTo(int? NumberFrom, int? NumberType, int? NumberTo)
        {
            if (NumberTo == null)
                return "";
            string result = VerifyNumberEvenOdd(NumberTo, NumberType);
            if ((NumberFrom ?? 0) > (NumberTo ?? 0))
                return $"{NumberTo} трябва да > от {NumberFrom}.";
            return result;
        }

        private DeliveryAreaFindVM DeliveryAreaAddressOnlyOneFromList(List<DeliveryAreaAddress> deliveryAddressesList, int courtId, Address address)
        {
            List<DeliveryAreaAddress> deliveryAddressesListSpecial = null;
            int? courtDelivererId = null;
            if (!string.IsNullOrEmpty(address?.CityCode)) {
                var munincipality = repo.AllReadonly<EkEkatte>()
                               .Where(c => c.Ekatte == address.CityCode)
                               .Select(x => x.Munincipality.Municipality)
                               .FirstOrDefault();
                if (!string.IsNullOrEmpty(munincipality))
                {
                    var courtDeliverer = repo.AllReadonly<CourtDeliverer>()
                                               .Where(x => x.CourtId == courtId &&
                                                           x.Ekatte == munincipality)
                                               .FirstOrDefault();
                    courtDelivererId = courtDeliverer?.DeivererCourtId;
                    if (courtDelivererId != null)
                    {
                        deliveryAddressesListSpecial = deliveryAddressesList.Where(x => x.DeliveryArea.CourtId == courtDelivererId).ToList(); 
                    }
                }
            }
            //if (courtDelivererId != null)
            //    deliveryAddressesList = deliveryAddressesListSpecial;
            var deliveryAreaFindVM = new DeliveryAreaFindVM();

            deliveryAreaFindVM.DeliveryAreaAddressList = deliveryAddressesList;
            deliveryAreaFindVM.DeliveryAreaList = deliveryAddressesList
                                                      .GroupBy(x => x.DeliveryAreaId)
                                                      .Select(g => g.First().DeliveryArea)
                                                      .ToList();
            deliveryAreaFindVM.DeliveryAreaId = -1;
            deliveryAreaFindVM.LawUnitId = -1;
            deliveryAreaFindVM.ToCourtId = courtDelivererId ?? -1;
            if (courtDelivererId != null)
            {
                if (deliveryAddressesList.Count == 0)
                    return deliveryAreaFindVM;
                var deliveryAreaList = deliveryAddressesListSpecial
                                                  .GroupBy(x => x.DeliveryAreaId)
                                                  .Select(g => g.First().DeliveryArea)
                                                  .ToList();
                if (deliveryAreaList.Count == 0)
                {
                    deliveryAreaList = repo.AllReadonly<DeliveryArea>()
                                           .Where(x => x.CourtId == courtDelivererId)
                                           .ToList();
                    if (deliveryAddressesList.Count > 0)
                    {
                        deliveryAreaFindVM.DeliveryAreaList.AddRange(deliveryAreaList);
                    }
                }
                if (deliveryAreaList.Count == 1)
                {
                    var deliveryArea = deliveryAreaList.First();
                    deliveryAreaFindVM.DeliveryAreaId = deliveryArea.Id;
                    deliveryAreaFindVM.LawUnitId = deliveryArea.LawUnitId ?? -1;
                    deliveryAreaFindVM.ToCourtId = deliveryArea.CourtId;
                }
            }
            else
            {
                if (deliveryAreaFindVM.DeliveryAreaList.Count == 1)
                {
                    var deliveryArea = deliveryAreaFindVM.DeliveryAreaList.First();
                    deliveryAreaFindVM.DeliveryAreaId = deliveryArea.Id;
                    deliveryAreaFindVM.LawUnitId = deliveryArea.LawUnitId ?? -1;
                    deliveryAreaFindVM.ToCourtId = deliveryArea.CourtId;
                }
            }
            if (deliveryAreaFindVM.ToCourtId == -1)
            {
                var courts = deliveryAreaFindVM.DeliveryAreaList
                                    .GroupBy(x => x.CourtId)
                                    .Select(g => g.First().CourtId)
                                    .ToList();
                if (courts.Count == 1)
                    deliveryAreaFindVM.ToCourtId = courts.First();
            }
            if (deliveryAreaFindVM.ToCourtId == -1)
            {
                var court = repo.AllReadonly<Court>().Where(x => x.Id == userContext.CourtId).FirstOrDefault();
                var courtsAll = repo.AllReadonly<Court>();
                var courts = deliveryAreaFindVM.DeliveryAreaList
                                    .Where(x => courtsAll.Any(c => c.Id == x.CourtId && c.CourtTypeId == court.CourtTypeId))
                                    .GroupBy(x => x.CourtId)
                                    .Select(g => g.First().CourtId)
                                    .ToList();
                if (courts.Count == 1)
                    deliveryAreaFindVM.ToCourtId = courts.First();
            }
            return deliveryAreaFindVM;
        }

        private Expression<Func<DeliveryAreaAddress, bool>> isActiveNow()
        {
            return x => x.DateExpired == null &&
                       (x.DateFrom ?? DateTime.MinValue).Date <= DateTime.Now &&
                       (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now.Date &&
                       x.DeliveryArea.DateExpired == null &&
                       (x.DeliveryArea.DateFrom ?? DateTime.MinValue).Date <= DateTime.Now &&
                       (x.DeliveryArea.DateTo ?? DateTime.MaxValue) >= DateTime.Now.Date;
        }

        private DeliveryAreaFindVM DeliveryAreaAddressBlockNameFind(Address address, bool equalStreet,int courtId)
        {
            var deliveryAddresses = repo.AllReadonly<DeliveryAreaAddress>()
                                    .Include(x => x.DeliveryArea)
                                    .Where(isActiveNow())
                                    .Where(x => x.CityCode == address.CityCode &&
                                                x.NumberType == DeliveryAddressNumberType.BlockName &&
                                                CyrillicVisualName(x.BlockName) == CyrillicVisualName(address.SubBlock));
            if (!string.IsNullOrEmpty(address.ResidentionAreaCode))
                deliveryAddresses = deliveryAddresses.Where(x => x.ResidentionAreaCode == address.ResidentionAreaCode);
            if ((address.Block ?? 0) == 0)
            {
                deliveryAddresses = deliveryAddresses.Where(x => x.NumberFrom == 0 || x.NumberFrom == null);
            }
            else
            {
                deliveryAddresses = deliveryAddresses.Where(x => x.NumberFrom == address.Block);
            }
            if (equalStreet && !string.IsNullOrEmpty(address.StreetCode))
                deliveryAddresses = deliveryAddresses.Where(x => x.StreetCode == address.StreetCode);
            var deliveryAddressesList = deliveryAddresses.ToList();

            return DeliveryAreaAddressOnlyOneFromList(deliveryAddressesList, courtId, address);
        }

        private DeliveryAreaFindVM DeliveryAreaAddressNumberNameFind(Address address, bool equalResidentionArea, int courtId)
        {
            var deliveryAddresses = repo.AllReadonly<DeliveryAreaAddress>()
                                    .Include(x => x.DeliveryArea)
                                    .Where(isActiveNow())
                                    .Where(x => x.CityCode == address.CityCode &&
                                                x.NumberType == DeliveryAddressNumberType.NumberName &&
                                                x.NumberFrom == address.StreetNumber &&
                                                CyrillicVisualName(x.BlockName) == CyrillicVisualName(address.SubNumber));
            if (equalResidentionArea && !string.IsNullOrEmpty(address.ResidentionAreaCode))
                deliveryAddresses = deliveryAddresses.Where(x => x.ResidentionAreaCode == address.ResidentionAreaCode);
            if (!string.IsNullOrEmpty(address.StreetCode))
                deliveryAddresses = deliveryAddresses.Where(x => x.StreetCode == address.StreetCode);
            var deliveryAddressesList = deliveryAddresses.ToList();

            return DeliveryAreaAddressOnlyOneFromList(deliveryAddressesList, courtId, address);
        }

        private DeliveryAreaFindVM DeliveryAreaAddressResidentionAreaFind(Address address, bool equalStreet, int courtId)
        {
            int block = (address.Block ?? 0);
            if (block <= 0 && string.IsNullOrEmpty(address.SubBlock))
                return DeliveryAreaAddressOnlyOneFromList(new List<DeliveryAreaAddress>(), courtId, address);
            if (!string.IsNullOrEmpty(address.SubBlock))
            {
                var blockResult = DeliveryAreaAddressBlockNameFind(address, equalStreet, courtId);
                if (blockResult.DeliveryAreaAddressList.Any() || (address.Block ?? 0) == 0)
                    return blockResult;
            }
            int maxNum = 99999;
            int typeNum = 0;
            if (block % 2 == 0)
                typeNum = DeliveryAddressNumberType.BlockEven;
            else
                typeNum = DeliveryAddressNumberType.BlockOdd;

            var deliveryAddresses = repo.AllReadonly<DeliveryAreaAddress>()
                                    .Include(x => x.DeliveryArea)
                                    .Where(isActiveNow())
                                    .Where(x => x.CityCode == address.CityCode &&
                                                string.IsNullOrEmpty(x.BlockName) &&
                                                (x.NumberType == typeNum || x.NumberType == DeliveryAddressNumberType.Block) &&
                                                (x.NumberFrom ?? 0) <= block &&
                                                block <= (x.NumberTo ?? maxNum));
            if (!string.IsNullOrEmpty(address.ResidentionAreaCode))
                deliveryAddresses = deliveryAddresses.Where(x => x.ResidentionAreaCode == address.ResidentionAreaCode);

            if (equalStreet && !string.IsNullOrEmpty(address.StreetCode))
                deliveryAddresses = deliveryAddresses.Where(x => x.StreetCode == address.StreetCode);
            var deliveryAddressesList = deliveryAddresses
                                           .OrderBy(x => (x.NumberTo ?? maxNum) - (x.NumberFrom ?? 0))
                                           .ToList();
            return DeliveryAreaAddressOnlyOneFromList(deliveryAddressesList, courtId, address);
        }
        private DeliveryAreaFindVM DeliveryAreaAddressStreetFind(Address address, bool equalResidentionArea, int courtId)
        {
            int maxNum = 99999;
            int num = (address.StreetNumber ?? 0);
            int typeNum = 0;
            if (num > 0)
            {
                if (num % 2 == 0)
                    typeNum = DeliveryAddressNumberType.EvenNumber;
                else
                    typeNum = DeliveryAddressNumberType.OddNumber;
            }
            var deliveryAddresses = repo.AllReadonly<DeliveryAreaAddress>()
                                      .Include(x => x.DeliveryArea)
                                      .Where(isActiveNow())
                                      .Where(x => x.CityCode == address.CityCode &&
                                                  x.StreetCode == address.StreetCode &&
                                                  string.IsNullOrEmpty(x.BlockName) &&
                                                  (x.NumberType == typeNum || x.NumberType == DeliveryAddressNumberType.OddEvenNumber) &&
                                                  (x.NumberFrom ?? 0) <= num &&
                                                  (x.NumberTo == null || (x.NumberTo ?? 0) >= num)
                                             );
            if (equalResidentionArea)
                deliveryAddresses = deliveryAddresses.Where(x => x.ResidentionAreaCode == address.ResidentionAreaCode);
            var deliveryAddressesList = deliveryAddresses
                                           .OrderBy(x => (x.NumberTo ?? maxNum) - (x.NumberFrom ?? 0))
                                           .ToList();
            return DeliveryAreaAddressOnlyOneFromList(deliveryAddressesList, courtId, address);
        }
        private DeliveryAreaFindVM DeliveryAreaAddressCityFind(Address address, int courtId)
        {
            var deliveryAddressesList = repo.AllReadonly<DeliveryAreaAddress>()
                       .Include(x => x.DeliveryArea)
                       .Where(isActiveNow())
                       .Where(x => x.CityCode == address.CityCode &&
                                   string.IsNullOrEmpty(x.ResidentionAreaCode) &&
                                   string.IsNullOrEmpty(x.StreetCode))
                      .ToList();
            return DeliveryAreaAddressOnlyOneFromList(deliveryAddressesList, courtId, address);
        }
        public DeliveryAreaFindVM DeliveryAreaAddressFind(Address address, int courtId)
        {
            if (address == null)
                return null;
            // Търсене по квартал улица и блок
            if (!string.IsNullOrEmpty(address.StreetCode) || !string.IsNullOrEmpty(address.ResidentionAreaCode))
            {
                var deliveryAreaFind = DeliveryAreaAddressResidentionAreaFind(address, true, courtId);
                if (deliveryAreaFind.IsFoundArea())
                    return deliveryAreaFind;
            }
            // Търсене по квартал и блок
            if (!string.IsNullOrEmpty(address.ResidentionAreaCode))
            {
                var deliveryAreaFind = DeliveryAreaAddressResidentionAreaFind(address, false, courtId);
                if (deliveryAreaFind.IsFoundArea())
                    return deliveryAreaFind;
            }

            if (address.StreetNumber > 0 && !string.IsNullOrEmpty(address.SubNumber))
            {
                // Търсене по квартал улица номер и подномер
                var deliveryAreaFind = DeliveryAreaAddressNumberNameFind(address, true, courtId);
                if (deliveryAreaFind.IsFoundArea())
                    return deliveryAreaFind;
                // Търсене по улица номер и подномер
                deliveryAreaFind = DeliveryAreaAddressNumberNameFind(address, false, courtId);
                if (deliveryAreaFind.IsFoundArea())
                    return deliveryAreaFind;
            }

            if (!string.IsNullOrEmpty(address.StreetCode))
            {
                // Търсене по квартал улица и номер
                var deliveryAreaFind = DeliveryAreaAddressStreetFind(address, true, courtId);
                if (deliveryAreaFind.IsFoundArea())
                    return deliveryAreaFind;
                // Търсене по улица и номер
                deliveryAreaFind = DeliveryAreaAddressStreetFind(address, false, courtId);
                if (deliveryAreaFind.IsFoundArea())
                    return deliveryAreaFind;
            }

            return DeliveryAreaAddressCityFind(address, courtId);
        }
        public DeliveryAreaFindVM DeliveryAreaCasePersonAddressIdFind(int CasePersonAddressId, int courtId)
        {
            if (CasePersonAddressId <= 0)
                return DeliveryAreaAddressOnlyOneFromList(new List<DeliveryAreaAddress>(), courtId, null);
            Address address = repo.AllReadonly<CasePersonAddress>()
                                    .Where(x => x.Id == CasePersonAddressId)
                                    .Include(x => x.Address)
                                    .Select(x => x.Address)
                                    .FirstOrDefault();

            return DeliveryAreaAddressFind(address, courtId);
        }
        public DeliveryAreaFindVM DeliveryAreaAddressIdFind(int AddressId, int courtId)
        {
            if (AddressId <= 0)
                return null;
            Address address = repo.AllReadonly<Address>()
                                    .Where(x => x.Id == AddressId)
                                    .FirstOrDefault();
            return DeliveryAreaAddressFind(address, courtId);
        }


        public DeliveryAreaAddressTestVM DeliveryAreaAddressFindTest(DeliveryAreaAddressTestVM model, int courtId)
        {
            var deliveryAreaFindVM = DeliveryAreaAddressFind(model.Address, courtId);
            var deliveryAddr = deliveryAreaFindVM.DeliveryAreaAddressList.FirstOrDefault() ?? new DeliveryAreaAddress();
            model.AreaName = deliveryAddr.DeliveryArea?.Description;
            int? lawUnitId = deliveryAddr.DeliveryArea?.LawUnitId;
            model.LawUnitName = repo.AllReadonly<LawUnit>()
                                    .Where(x => x.Id == lawUnitId)
                                    .Select(x => x.FullName)
                                    .FirstOrDefault();
            model.City = repo.AllReadonly<EkEkatte>().Where(c => c.Ekatte == deliveryAddr.CityCode).Select(EkatteCityName()).FirstOrDefault();
            model.Street = repo.AllReadonly<EkStreet>().Where(s => s.Code == deliveryAddr.StreetCode && s.Ekatte == deliveryAddr.CityCode).Select(c => c.Name).FirstOrDefault();
            model.ResidentionArea = repo.AllReadonly<EkStreet>().Where(s => s.Code == deliveryAddr.ResidentionAreaCode && s.Ekatte == deliveryAddr.CityCode).Select(c => c.Name).FirstOrDefault();
            model.NumberType = repo.AllReadonly<DeliveryNumberType>().Where(n => n.Id == deliveryAddr.NumberType).Select(c => c.Label).FirstOrDefault();
            model.NumberFrom = deliveryAddr.NumberFrom?.ToString();
            model.NumberTo = deliveryAddr.NumberTo?.ToString();

            return model;
        }
        public IQueryable<DeliveryAreaAddressVM> DeliveryAreaAddressDuplication(int courtId)
        {
            var deliveryAddress = repo.AllReadonly<DeliveryAreaAddress>()
                                     .Include(x => x.DeliveryArea)
                                     .Where(isActiveNow())
                                     .Where(x => x.DeliveryArea.CourtId == courtId &&
                                                 (!string.IsNullOrEmpty(x.StreetCode) ||
                                                  (!string.IsNullOrEmpty(x.ResidentionAreaCode) &&
                                                    (x.NumberFrom != null || x.NumberTo != null || !string.IsNullOrEmpty(x.BlockName))
                                                  )
                                                 )
                                            )
                                     .ToList();
            var deliveryAddress2 = new List<DeliveryAreaAddress>();
            int maxNum = 9999;
            foreach (var deliveryAddr in deliveryAddress)
            {
                var deliveryAddressD = deliveryAddress
                    .Where(x => x.Id != deliveryAddr.Id &&
                                x.DeliveryAreaId != deliveryAddr.DeliveryAreaId &&
                                (x.NumberType == deliveryAddr.NumberType ||
                                  (x.NumberType == DeliveryAddressNumberType.EvenNumber && deliveryAddr.NumberType == DeliveryAddressNumberType.OddEvenNumber) ||
                                  (x.NumberType == DeliveryAddressNumberType.OddEvenNumber && deliveryAddr.NumberType == DeliveryAddressNumberType.EvenNumber) ||
                                  (x.NumberType == DeliveryAddressNumberType.BlockEven && deliveryAddr.NumberType == DeliveryAddressNumberType.Block) ||
                                  (x.NumberType == DeliveryAddressNumberType.Block && deliveryAddr.NumberType == DeliveryAddressNumberType.BlockEven)
                                ) &&
                                x.CityCode == deliveryAddr.CityCode &&
                                (
                                  deliveryAddr.NumberType == DeliveryAddressNumberType.Block ||
                                  deliveryAddr.NumberType == DeliveryAddressNumberType.BlockEven ||
                                  deliveryAddr.NumberType == DeliveryAddressNumberType.BlockOdd ||
                                  deliveryAddr.NumberType == DeliveryAddressNumberType.BlockName ||
                                  x.StreetCode == deliveryAddr.StreetCode
                                ) &&
                                (
                                  deliveryAddr.NumberType == DeliveryAddressNumberType.EvenNumber ||
                                  deliveryAddr.NumberType == DeliveryAddressNumberType.OddNumber ||
                                  deliveryAddr.NumberType == DeliveryAddressNumberType.OddEvenNumber ||
                                  x.ResidentionAreaCode == deliveryAddr.ResidentionAreaCode
                                ) &&
                                (
                                 ((x.NumberFrom ?? 0) <= (deliveryAddr.NumberFrom ?? 0) && (deliveryAddr.NumberFrom ?? 0) <= (x.NumberTo ?? maxNum)) ||
                                 ((x.NumberFrom ?? 0) <= (deliveryAddr.NumberTo ?? maxNum) && (deliveryAddr.NumberTo ?? maxNum) <= (x.NumberTo ?? maxNum))
                                ))
                    .ToList();
                var deliveryAddressBlock = deliveryAddress
                       .Where(x => x.Id != deliveryAddr.Id &&
                                   x.DeliveryAreaId != deliveryAddr.DeliveryAreaId &&
                                   x.NumberType == deliveryAddr.NumberType &&
                                   x.NumberType == DeliveryAddressNumberType.BlockName &&
                                   x.StreetCode == deliveryAddr.StreetCode &&
                                   x.ResidentionAreaCode == deliveryAddr.ResidentionAreaCode &&
                                   x.NumberFrom == deliveryAddr.NumberFrom &&
                                   CyrillicVisualName(x.BlockName) == CyrillicVisualName(deliveryAddr.BlockName))
                       .ToList();
                deliveryAddressD.AddRange(deliveryAddressBlock);
                if (deliveryAddressD.Any())
                    if (!deliveryAddress2.Any(x => x.Id == deliveryAddr.Id))
                        deliveryAddress2.Add(deliveryAddr);
                foreach (var deliveryAddrD in deliveryAddressD)
                {
                    if (!deliveryAddress2.Any(x => x.Id == deliveryAddrD.Id))
                        deliveryAddress2.Add(deliveryAddrD);
                }
            }
            deliveryAddress2 = deliveryAddress2.OrderBy(x => x.CityCode).ThenBy(x => x.ResidentionAreaCode ?? "").ThenBy(x => x.StreetCode ?? "").ThenBy(x => x.NumberFrom ?? 0).ToList();
            var cities = repo.AllReadonly<EkEkatte>().AsQueryable();
            var streets = repo.AllReadonly<EkStreet>().AsQueryable();
            var numberTypes = repo.AllReadonly<DeliveryNumberType>().AsQueryable();
            return deliveryAddress2.Select(x => new DeliveryAreaAddressVM()
            {
                Id = x.Id,
                AreaName = x.DeliveryArea.Description,
                City = cities.Where(c => c.Ekatte == x.CityCode).Select(EkatteCityName()).FirstOrDefault(),
                ResidentionArea = streets.Where(s => s.Code == x.ResidentionAreaCode && s.Ekatte == x.CityCode).Select(c => c.Name).FirstOrDefault(),
                Street = streets.Where(s => s.Code == x.StreetCode && s.Ekatte == x.CityCode).Select(c => c.Name).FirstOrDefault(),
                NumberType = numberTypes.Where(n => n.Id == x.NumberType).Select(c => c.Label).FirstOrDefault(),
                NumberFrom = x.NumberFrom,
                NumberTo = x.NumberTo,
                BlockName = x.BlockName,
                Remark = (x.NumberType == DeliveryAddressNumberType.EvenNumber || x.NumberType == DeliveryAddressNumberType.OddNumber || x.NumberType == DeliveryAddressNumberType.OddEvenNumber) &&
                         (x.NumberFrom > 0 || x.NumberTo > 0) &&
                         (string.IsNullOrEmpty(x.StreetCode)) ? "За да се районира от номер до номер улица трябва да е въведена улица" : ""
            }).AsQueryable();
        }

        /// <summary>
        /// Всички улици/квартали, които не са влезнали в район - да може да се изберат към район
        /// </summary>
        /// <param name="cityId"></param>
        /// <returns></returns>
        public IQueryable<MultiSelectTransferVM> EkStreetForSelect_Select(string cityId, int courtId)
        {
            return repo.AllReadonly<EkStreet>()
                           .Where(x => x.Ekatte == cityId)
                           .Where(x => repo.AllReadonly<DeliveryAreaAddress>()
                                            .Where(a => a.StreetCode == x.Code && a.CityCode == cityId &&
                                            a.DateExpired == null && a.DeliveryArea.CourtId == courtId).Any() == false
                                 )
                           .Select(x => new MultiSelectTransferVM()
                           {
                               Id = x.Id,
                               Text = $"{x.Name}"
                           }).AsQueryable();
        }

        public bool DeliveryAreaAddressSaveListData(int deliveryAreaId, List<int> streets, string cityId)
        {
            try
            {
                var ekStreets = repo.AllReadonly<EkStreet>()
                    .Where(x => x.Ekatte == cityId)
                    .ToList();
                foreach (var item in streets)
                {
                    var street = ekStreets.Where(a => a.Id == item).FirstOrDefault();

                    DeliveryAreaAddress saved = new DeliveryAreaAddress();
                    saved.DeliveryAreaId = deliveryAreaId;
                    if (street.StreetType == EkStreetTypes.Area)
                        saved.ResidentionAreaCode = street.Code;
                    else
                        saved.StreetCode = street.Code;
                    saved.CityCode = cityId;
                    saved.NumberType = DeliveryAddressNumberType.OddEvenNumber;
                    saved.NumberFrom = 0;
                    saved.NumberTo = 999;
                    saved.IsActive = true;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    repo.Add<DeliveryAreaAddress>(saved);
                }
                repo.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на улици към райони");
                return false;
            }
        }
        private string CyrillicVisualName(string blockName)
        {
            return string.Join("", blockName.ToUpper().ToCharArray().Select(x => VisualLetterEnBg.ContainsKey(x) ? VisualLetterEnBg[x] : x));
        }
        public List<SelectListItem> ExpiredTypeDDL()
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Text = "Всички", Value = "-1" });
            result.Add(new SelectListItem() { Text = "Анулирани", Value = "0" });
            result.Add(new SelectListItem() { Text = "Активни", Value = "1" });
            return result;
        }
    }
}
