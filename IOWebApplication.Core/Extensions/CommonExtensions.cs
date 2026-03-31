using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Audit;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System;
using System.Linq.Expressions;

namespace IOWebApplication.Core.Extensions
{
    public static class CommonExtensions
    {


        /// <summary>
        /// Копира имената и идентификатора от друга инстанция
        /// </summary>
        /// <param name="model"></param>
        /// <param name="source"></param>
        /// <param name="copySourceData"></param>
        public static void CopyFrom(this NamesBase model, NamesBase source, bool copySourceData = true)
        {
            model.Uic = source.Uic?.Trim();
            model.UicTypeId = source.UicTypeId;
            model.FirstName = source.FirstName?.Trim();
            model.MiddleName = source.MiddleName?.Trim();
            model.FamilyName = source.FamilyName?.Trim();
            model.Family2Name = source.Family2Name?.Trim();
            model.DepartmentName = source.DepartmentName?.Trim();
            model.LatinName = source.LatinName;
            model.FullName = source.MakeFullName();
            model.IsDeceased = source.IsDeceased;
            model.DateDeceased = source.DateDeceased;
            model.GenderId = source.GenderId.NumberEmptyToNull();
            model.CitizenshipId = source.CitizenshipId.NumberEmptyToNull();

            if (copySourceData)
            {
                model.Person_SourceId = source.Person_SourceId;
                model.Person_SourceType = source.Person_SourceType;
                model.Person_SourceCode = source.Person_SourceCode;
            }
        }

        public static void CopyFrom(this Address model, Address source)
        {
            model.AddressTypeId = source.AddressTypeId;
            model.CountryCode = source.CountryCode;
            model.DistrictCode = source.DistrictCode;
            model.MunicipalityCode = source.MunicipalityCode;
            model.CityCode = source.CityCode;
            model.RegionCode = source.RegionCode;
            model.StreetCode = source.StreetCode;
            model.ForeignAddress = source.ForeignAddress;
            model.Block = source.Block;
            model.SubBlock = source.SubBlock;
            model.ResidentionAreaCode = source.ResidentionAreaCode;
            model.StreetNumber = source.StreetNumber;
            model.SubNumber = source.SubNumber;
            model.Entrance = source.Entrance;
            model.Floor = source.Floor;
            model.Appartment = source.Appartment;
            model.Phone = source.Phone;
            model.Fax = source.Fax;
            model.Email = source.Email;
            model.Description = source.Description;
            model.FullAddress = source.FullAddress;
        }

        public static string MakeFullName(this NamesBase model)
        {
            //Ако е избрана институция се връща директно пълното име
            if (!string.IsNullOrEmpty(model.FullName) && model.Person_SourceType > 0 && model.Person_SourceType != SourceTypeSelectVM.EisppPerson)
            {
                return model.FullName;
            }
            switch (model.UicTypeId)
            {
                case NomenclatureConstants.UicTypes.EIK:
                case NomenclatureConstants.UicTypes.Bulstat:
                    return model.FullName;
                default:
                    string result = model.FirstName;
                    if (!string.IsNullOrEmpty(model.MiddleName))
                    {
                        result += " " + model.MiddleName;
                    }
                    if (!string.IsNullOrEmpty(model.FamilyName))
                    {
                        result += " " + model.FamilyName;
                    }
                    if (!string.IsNullOrEmpty(model.Family2Name))
                    {
                        result += " " + model.Family2Name;
                    }
                    return result;
            }
        }

        public static string MakeFullNameEISPP(this NamesBase model)
        {
            switch (model.UicTypeId)
            {
                case NomenclatureConstants.UicTypes.EIK:
                case NomenclatureConstants.UicTypes.Bulstat:
                    return model.FullName ?? "";
                default:
                    string result = model.FirstName;
                    if (!string.IsNullOrEmpty(model.MiddleName))
                    {
                        result += " " + model.MiddleName;
                    }
                    if (!string.IsNullOrEmpty(model.FamilyName))
                    {
                        result += " " + model.FamilyName;
                    }
                    if (!string.IsNullOrEmpty(model.Family2Name))
                    {
                        result += " " + model.Family2Name;
                    }
                    return result ?? "";
            }
        }

        public static Expression<Func<T, bool>> PersonNamesBase_Where<T>(int uicTypeId, string uic, string fullName) where T : PersonNamesBase
        {
            Expression<Func<T, bool>> personWhere = x => true;
            if (!string.IsNullOrEmpty(uic))
                personWhere = x => x.Uic == uic && x.UicTypeId == uicTypeId;
            else
                personWhere = x => x.Uic == null && x.FullName.ToLower() == fullName.ToLower() && x.UicTypeId == uicTypeId;

            return personWhere;
        }

        public static Expression<Func<T, bool>> LawUnit_Where<T>(int uicTypeId, int? personId, string uic, string fullName) where T : ILawUnit
        {
            Expression<Func<T, bool>> personWhere = x => true;
            if (personId != null)
                personWhere = x => x.LawUnit.PersonId == personId;
            else if (!string.IsNullOrEmpty(uic))
                personWhere = x => x.LawUnit.Uic == uic && x.LawUnit.UicTypeId == uicTypeId;
            else
                personWhere = x => x.LawUnit.Uic == null && x.LawUnit.FullName.ToLower() == fullName.ToLower() && x.LawUnit.UicTypeId == uicTypeId;

            return personWhere;
        }
        public static string FullAddressNotification(this Address model)
        {
            string result = string.Empty;

            if (model != null)
            {
                result = model.FullAddress ?? "";

                if (!string.IsNullOrEmpty(model.Phone))
                {
                    result = result.Replace($",тел: {model.Phone}", "");
                }
                if (!string.IsNullOrEmpty(model.Fax))
                {
                    result = result.Replace($",факс: {model.Fax}", "");
                }
                if (!string.IsNullOrEmpty(model.Email))
                {
                    result = result.Replace($",e-mail: {model.Email}", "");
                }
            }

            return result;
        }
        public static string FullAddressNotificationMailTel(this Address model, bool addTel, bool addEmail)
        {
            string result = string.Empty;

            if (model != null)
            {
                result = FullAddressNotification(model);

                if (addTel && !string.IsNullOrEmpty(model.Phone))
                    result = result + $" ,тел: {model.Phone}";
                if (addEmail && !string.IsNullOrEmpty(model.Email))
                    result = result + $" ,e-mail: {model.Email}";
            }

            return result;
        }

        public static bool IsDataTableRequest(this ActionExecutedContext context)
        {

            if (context?.HttpContext?.Request?.HasFormContentType == true)
                if (context?.HttpContext?.Request?.Form?.ContainsKey("draw") == true)
                {
                    return true;
                }

            return false;
        }

        public static bool IsHiddenActionResult(this ActionExecutedContext context)
        {

            if (context?.Result is Microsoft.AspNetCore.Mvc.JsonResult
                 || context?.Result is Microsoft.AspNetCore.Mvc.PartialViewResult
                 || context?.Result is Microsoft.AspNetCore.Mvc.FileResult
                 || context?.Result is Microsoft.AspNetCore.Mvc.FileContentResult)
            {
                return true;
            }

            return false;
        }
    }
}
