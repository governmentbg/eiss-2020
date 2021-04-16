using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IO.RegixClient;

namespace IOWebApplication.Core.Extensions
{
    public static class ModelExtensions
    {
        public static Address ToEntity(this PermanentAddressResponseType model)
        {
            if (string.IsNullOrEmpty(model.SettlementCode))
            {
                return null;
            }

            var result = new Address();
            result.CountryCode = NomenclatureConstants.CountryBG;
            result.CityCode = model.SettlementCode;
            result.ResidentionAreaCode = model.LocationCode;
            result.SubBlock = model.BuildingNumber;
            result.Entrance = model.Entrance;
            result.Floor = model.Floor;
            result.Appartment = model.Apartment;
            result.AddressTypeId = NomenclatureConstants.AddressType.Permanent;
            return result;
        }

        public static Address ToEntity(this TemporaryAddressResponseType model)
        {
            if (string.IsNullOrEmpty(model.SettlementCode))
            {
                return null;
            }

            var result = new Address();
            result.CountryCode = NomenclatureConstants.CountryBG;
            result.CityCode = model.SettlementCode;
            result.ResidentionAreaCode = model.LocationCode;
            result.SubBlock = model.BuildingNumber;
            result.Entrance = model.Entrance;
            result.Floor = model.Floor;
            result.Appartment = model.Apartment;
            result.AddressTypeId = NomenclatureConstants.AddressType.Current;
            return result;
        }
    }
}
