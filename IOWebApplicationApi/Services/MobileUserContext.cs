// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace IOWebApplicationApi.Services
{
    public class MobileUserContext : IUserContext
    {
        HttpContext context;
        private ClaimsPrincipal _user;
        private ClaimsPrincipal User
        {
            get
            {
                if (_user == null)
                {
                    _user = context.User;
                }
                return _user;
            }
        }
        public MobileUserContext(IHttpContextAccessor _ca)
        {
            context = _ca.HttpContext;
        }

        public int CourtId
        {

            get
            {
                int result = 0;
                if (User != null && User.Claims != null && User.Claims.Count() > 0)
                {
                    var subClaim = User.Claims
                        .FirstOrDefault(c => c.Type == CustomClaimType.CourtId);

                    if (subClaim != null)
                    {
                        result = int.Parse(subClaim.Value);
                    }
                }

                return result;
            }

        }

        public int CourtTypeId => throw new NotImplementedException();

        public string CourtName => throw new NotImplementedException();

        public int[] CourtInstances => throw new NotImplementedException();

        public int LawUnitId
        {

            get
            {
                //TODO
                int lawUnitId = 0;
                if (User != null && User.Claims != null && User.Claims.Count() > 0)
                {
                    var subClaim = User.Claims
                        .FirstOrDefault(c => c.Type == CustomClaimType.LawUnitId);

                    if (subClaim != null)
                    {
                        lawUnitId = int.Parse(subClaim.Value);
                    }
                }


                return lawUnitId;
            }

        }

        public string UserId
        {
            get
            {
                string userId = null;

                if (User != null && User.Claims != null && User.Claims.Count() > 0)
                {
                    var subClaim = User.Claims
                        .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

                    if (subClaim != null)
                    {
                        userId = subClaim.Value;
                    }
                }

                return userId;
            }
        }

        public string Email => "service agent";

        public string LogName => "service agent";

        public string FullName => "service agent";

        public int[] SubDocRegistry => throw new NotImplementedException();

        public int[] CourtOrganizations => throw new NotImplementedException();

        public string CertificateNumber => throw new NotImplementedException();

        public int LawUnitTypeId => throw new NotImplementedException();

        public bool IsInterimPeriodEuro => throw new NotImplementedException();

        public bool IsPeriodEuro => throw new NotImplementedException();

        public decimal EuroExchangeRate => throw new NotImplementedException();

        public string CurrentCurrencyCode => throw new NotImplementedException();

        public string EnvironmentName => throw new NotImplementedException();

        public bool CheckHash(string hash, object id, object parent = null)
        {
            throw new NotImplementedException();
        }

        public bool CheckHash(BlankEditVM blankModel)
        {
            throw new NotImplementedException();
        }

        public string ClaimValue(string claimType)
        {
            throw new NotImplementedException();
        }

        public string GenHash(object id, object parent = null)
        {
            throw new NotImplementedException();
        }

        public bool IsSystemInFeature(string feature)
        {
            return true;
            //throw new NotImplementedException();
        }

        public bool IsUserInCourt(int courtId)
        {
            return true;
            //throw new NotImplementedException();
        }

        public bool IsUserInFeature(string feature)
        {
            return true;
            //throw new NotImplementedException();
        }

        public bool IsUserInRole(string role)
        {
            return true;
            //throw new NotImplementedException();
        }
    }
}
