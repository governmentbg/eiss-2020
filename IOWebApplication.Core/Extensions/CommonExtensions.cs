// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
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
        public static void CopyFrom(this NamesBase model, NamesBase source, bool copySourceData = true)
        {
            model.Uic = source.Uic;
            model.UicTypeId = source.UicTypeId;
            model.FirstName = source.FirstName;
            model.MiddleName = source.MiddleName;
            model.FamilyName = source.FamilyName;
            model.Family2Name = source.Family2Name;
            model.DepartmentName = source.DepartmentName;
            model.LatinName = source.LatinName;
            model.FullName = source.MakeFullName();

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
            if (model.Person_SourceType > 0 && model.Person_SourceType != SourceTypeSelectVM.EisppPerson)
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
    }
}
