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
using Microsoft.AspNetCore.Mvc.Rendering;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Constants;

namespace IOWebApplication.Core.Services
{
    public class DeliveryAreaService : BaseService, IDeliveryAreaService
    {
        public DeliveryAreaService(
            ILogger<DeliveryAreaService> _logger,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }

        public DeliveryArea GetById(int id)
        {
            var deliveryArea = repo.AllReadonly<DeliveryArea>()
                                   .Where(x => x.Id == id)
                                   .FirstOrDefault();
            if (deliveryArea != null)
                deliveryArea.CountAddress = repo.AllReadonly<DeliveryAreaAddress>()
                                                .Where(x => x.DeliveryAreaId == id && x.DateExpired == null)
                                                .Count();
            return deliveryArea;
        }

        public IQueryable<DeliveryAreaVM> DeliveryAreaSelect(DeliveryAreaFilterVM filter)
        {
            var lawUnit = repo.AllReadonly<LawUnit>();
            var addresses = repo.AllReadonly<DeliveryAreaAddress>();
            var deliveryAreas = repo.AllReadonly<DeliveryArea>()
                .Where(x => (filter.CourtId <= 0 || x.CourtId == filter.CourtId) &&
                            (
                              ((filter.DateFrom ?? DateTime.MinValue) <= (x.DateFrom ?? DateTime.MinValue) && (x.DateFrom ?? DateTime.MinValue) <= (filter.DateTo ?? DateTime.MaxValue)) ||
                              ((filter.DateFrom ?? DateTime.MinValue) <= (x.DateTo ?? DateTime.MaxValue) && (x.DateTo ?? DateTime.MaxValue) <= (filter.DateTo ?? DateTime.MaxValue)) ||
                              ((x.DateFrom ?? DateTime.MinValue) <= (filter.DateFrom ?? DateTime.MinValue) && (filter.DateFrom ?? DateTime.MinValue) <= (x.DateTo ?? DateTime.MaxValue)) ||
                              ((x.DateFrom ?? DateTime.MinValue) <= (filter.DateTo ?? DateTime.MaxValue) && (filter.DateTo ?? DateTime.MaxValue) <= (x.DateTo ?? DateTime.MaxValue))
                            ) &&
                            (filter.ExpiredType != 0 || (x.DateExpired != null && (filter.DateExpiredFrom ?? DateTime.MinValue) <= (x.DateExpired ?? DateTime.MinValue) && (x.DateExpired ?? DateTime.MinValue) <= (filter.DateExpiredTo ?? DateTime.MaxValue))) &&
                            (filter.ExpiredType != 1 || x.DateExpired == null)
                 );
            bool haveAddrFilter = false;
            if (!string.IsNullOrEmpty(filter.CityCode) && filter.CityCode != "-1")
            {
                haveAddrFilter = true;
                addresses = addresses.Where(a => a.CityCode == filter.CityCode &&
                                            (string.IsNullOrEmpty(filter.ResidentionAreaCode) || a.ResidentionAreaCode == filter.ResidentionAreaCode) &&
                                            (string.IsNullOrEmpty(filter.StreetCode) || a.StreetCode == filter.StreetCode) 
                                           );
            };
            if (filter.Number > 0 || filter.Block > 0)
            {
                int maxNum = 99999;
                haveAddrFilter = true;
                int block = 0;
                if (filter.Block > 0)
                    block = filter.Block??0;
                int typeNumBlock;
                if (block % 2 == 0)
                    typeNumBlock = NomenclatureConstants.DeliveryAddressNumberType.BlockEven;
                else
                    typeNumBlock = NomenclatureConstants.DeliveryAddressNumberType.BlockOdd;
                int streetNum = 0;
                if (filter.Number > 0)
                    streetNum = filter.Number ?? 0;
                int typeNumStreet;
                if (block % 2 == 0)
                    typeNumStreet = NomenclatureConstants.DeliveryAddressNumberType.EvenNumber;
                else
                    typeNumStreet = NomenclatureConstants.DeliveryAddressNumberType.OddNumber;

                addresses = addresses.Where(x => (block > 0 && (x.NumberType == typeNumBlock || x.NumberType == NomenclatureConstants.DeliveryAddressNumberType.Block) &&
                                                               (x.NumberFrom ?? 0) <= block && block <= (x.NumberTo ?? maxNum)) ||
                                                 (streetNum > 0 && (x.NumberType == typeNumStreet || x.NumberType == NomenclatureConstants.DeliveryAddressNumberType.OddEvenNumber) &&
                                                               (x.NumberFrom ?? 0) <= streetNum && streetNum <= (x.NumberTo ?? maxNum)));
            }
            if (haveAddrFilter)
            {
                deliveryAreas = deliveryAreas.Where(x => addresses.Any(a => a.DeliveryAreaId == x.Id && a.DateExpired == null));
            }
            return deliveryAreas
                .Select(x => new DeliveryAreaVM()
                {
                    Id = x.Id,
                    Description = x.Description,
                    Code = x.Code,
                    LawUnitName = lawUnit.Where(l => l.Id == x.LawUnitId).Select(d => d.FullName).FirstOrDefault(),
                    DateFrom = x.DateFrom,
                    DateTo = x.DateTo,
                    DateExpired = x.DateExpired
                });
        }
        public int? GetDeliveryAreaIdByLawUnitId(int courtId, int? lawUnitId)
        {
            return repo.AllReadonly<DeliveryArea>()
                .Where(x => x.CourtId == courtId &&
                            x.LawUnitId == lawUnitId &&
                            x.DateExpired == null)
                .Select(x => (int?)x.Id)
                .FirstOrDefault();
        }

        public List<SelectListItem> DeliveryAreaSelectDDL(int forCourtId, bool addNotSet)
        {
            var result = repo.AllReadonly<DeliveryArea>()
                .Where(x => (x.CourtId == forCourtId && x.DateExpired == null))
                       .Select(x => new SelectListItem()
                       {
                           Text = x.Description,
                           Value = x.Id.ToString()
                       }).ToList() ?? new List<SelectListItem>();
            if (addNotSet)
                result.Insert(0, new SelectListItem() { Text = "Без избран район", Value = "0" });
            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }
        public List<SelectListItem> DeliveryAreaListToDdl(List<DeliveryArea> deliveryAreaList)
        {
            var result = deliveryAreaList
                       .Select(x => new SelectListItem()
                       {
                           Text = x.Description,
                           Value = x.Id.ToString()
                       }).ToList() ?? new List<SelectListItem>();
            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }

        public List<SelectListItem> RemoveSelectAddNoChange(List<SelectListItem> fromList)
        {
            var result = fromList.Where(x => x.Value != "-1").ToList();
            result.Insert(0, new SelectListItem() { Text = "Без промяна", Value = "-1" });
            return result;
        }
        public List<Select2ItemVM> RemoveSelectAddNoChangeSelect2(List<Select2ItemVM> fromList, string newVal)
        {
            var result = fromList.Where(x => x.Id != -1).ToList();
            result.Insert(0, new Select2ItemVM() { Text = newVal, Id = -1 });
            return result;
        }

       
        public List<Select2ItemVM> DeliveryAreaDdlSelect2(int forCourtId)
        {
            var result = repo.AllReadonly<DeliveryArea>()
                .Where(x => (x.CourtId == forCourtId && x.DateExpired == null))
                       .Select(x => new Select2ItemVM()
                       {
                           Text = x.Description,
                           Id = x.Id
                       }).ToList() ?? new List<Select2ItemVM>();
            result.Insert(0, new Select2ItemVM() { Text = "Избери", Id = -1 });
            return result;
        }
        public List<Select2ItemVM> DeliveryAreaListToDdlSelect2(List<DeliveryArea> deliveryAreaList)
        {
            var result = deliveryAreaList
                       .Select(x => new Select2ItemVM()
                       {
                           Text = x.Description,
                           Id = x.Id
                       }).ToList() ?? new List<Select2ItemVM>();
            if (result.Count > 0)
                result.Insert(0, new Select2ItemVM() { Text = "Избери", Id = -1 });
            return result;
        }
        public bool DeliveryAreaSaveData(DeliveryArea model)
        {
            try
            {
                model.LawUnitId = model.LawUnitId.EmptyToNull(-1);
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<DeliveryArea>(model.Id);
                    saved.CourtId = model.CourtId;
                    saved.LawUnitId = model.LawUnitId;
                    saved.Description = model.Description;
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
                    repo.Add<DeliveryArea>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на райони за разнос към съд Id={ model.Id }");
                return false;
            }
        }
        public void insertCaseRegion()
        {
            var courts = repo.AllReadonly<Court>()
                             .Where(x => x.CourtTypeId == 2 || x.CourtTypeId == 8 || x.CourtTypeId == 10 || x.CourtTypeId == 11 || x.CourtTypeId == 9)
                             .OrderBy(x => x.CourtTypeId == 9)
                             .ThenBy(x => x.CourtTypeId)
                             .ThenBy(x => x.Id)
                             .ToList();
            foreach (var court in courts)
            {
                var courtRegion = new CourtRegion()
                {
                    Id = court.Id,
                    UserId = userContext.UserId,
                    DateWrt = DateTime.Now,
                    IsActive = court.IsActive,
                    Label = court.Label,
                    ParentId = null
                };
                int parentId = 0;
                int code = int.Parse(court.Code);
                switch (court.CourtTypeId)
                {
                    case 8:
                        parentId = 800;
                        break;
                    case 10:
                        parentId = (code / 100) * 100;
                        break;
                    case 11:
                        parentId = (code / 10) * 10;
                        break;
                    case 9:
                        parentId = 0;
                        break;
                }
                if (parentId > 0)
                    courtRegion.ParentId = courts.Where(x => x.Code == parentId.ToString()).Select(x => x.Id).FirstOrDefault();
                if (court.CourtTypeId != 9) {
                    repo.Add(courtRegion);
                    court.CourtRegionId = court.Id;
                    repo.Update(court);
                } else
                {
                    string cityName = court.Label.Replace("Административен съд –", "").Trim();
                    var courtTo = courts.Where(x => x.CourtTypeId == 10 && x.Label.EndsWith(cityName)).FirstOrDefault();
                    if (courtTo == null)
                    {
                        court.CourtRegionId = null;
                        if (cityName == "София-окръг")
                            court.CourtRegionId = 179;
                        if (cityName == "София-град")
                            court.CourtRegionId = 178;
                        if (court.CourtRegionId != null)
                            repo.Update(court);
                    }
                    else {
                        court.CourtRegionId = courtTo.Id;
                        repo.Update(court);
                    }
                }
                if (court.CourtTypeId == 11 || court.CourtTypeId == 10)
                {
                    string ektte = court.CityCode.Trim();
                    var city = repo.AllReadonly<EkEkatte>()
                                   .Where(x => x.Ekatte == ektte)
                                   .FirstOrDefault();
                    var municipality = repo.AllReadonly<EkMunincipality>()
                               .Where(x => x.MunicipalityId == city.MunicipalId)
                               .FirstOrDefault();
                    var district = repo.AllReadonly<EkDistrict>()
                               .Where(x => x.DistrictId == municipality.DistrictId)
                               .FirstOrDefault();
                    var courtRegionArea = new CourtRegionArea()
                    {
                        CourtRegionId = courtRegion.Id,
                        UserId = userContext.UserId,
                        DateWrt = DateTime.Now,
                        IsActive = court.IsActive,
                        DistrictCode = district.Ekatte,
                        MunicipalityCode = court.CourtTypeId == 11 ? municipality.Municipality : null
                    };
                    repo.Add(courtRegionArea);
                }
                repo.SaveChanges();
            }
        }

        public void updateCaseRegionParent()
        {
            var courts = repo.AllReadonly<Court>()
                             .Where(x => x.CourtTypeId == 2 || x.CourtTypeId == 8 || x.CourtTypeId == 10 || x.CourtTypeId == 11 || x.CourtTypeId == 9)
                             .OrderBy(x => x.CourtTypeId == 9)
                             .ThenBy(x => x.CourtTypeId)
                             .ThenBy(x => x.Id)
                             .ToList();
            foreach (var court in courts)
            {
                var courtRegion = repo.GetById<CourtRegion>(court.Id);

                int parentId = 0;
                int code = int.Parse(court.Code);
                switch (court.CourtTypeId)
                {
                    case 8:
                        parentId = 800;
                        break;
                    case 10:
                        parentId = (code / 100) * 100;
                        break;
                    case 11:
                        parentId = (code / 10) * 10;
                        break;
                    case 9:
                        parentId = 0;
                        break;
                }
                if (parentId > 0)
                {
                    courtRegion.ParentId = courts.Where(x => x.Code == parentId.ToString()).Select(x => x.Id).FirstOrDefault();
                    repo.SaveChanges();
                }
            }
        }
        public void saveHtmlTemplateLink() 
        {
            var htmlTemplates = repo.AllReadonly<HtmlTemplate>()
                                    .Where(x => x.Id == 132 || x.Id == 133)
                                    .ToList();
            var caseGroups = repo.AllReadonly<CaseGroup>()
                                    .Where(x => x.Id == 1 || x.Id == 3 || x.Id == 5)
                                    .ToList();
            var courtTypes = repo.AllReadonly<CourtType>()
                                    .Where(x => x.Id != 2)
                                    .ToList();
            foreach (var htmlTemplate in htmlTemplates)
            {
                foreach (var caseGroup in caseGroups)
                {
                    foreach(var courtType in courtTypes)
                    {
                        var htmlTemplateLink = repo.AllReadonly<HtmlTemplateLink>()
                                    .Where(x => x.HtmlTemplateId == htmlTemplate.Id &&
                                                x.CourtTypeId == courtType.Id &&
                                                x.CaseGroupId == caseGroup.Id)
                                    .FirstOrDefault();
                        if (htmlTemplateLink == null)
                        {
                            htmlTemplateLink = new HtmlTemplateLink()
                            {
                                HtmlTemplateId = htmlTemplate.Id,
                                CourtTypeId = courtType.Id,
                                CaseGroupId = caseGroup.Id
                            };
                            repo.Add(htmlTemplateLink);
                            repo.SaveChanges();
                        }
                    }
                }
            }

        }
        
    }
}
