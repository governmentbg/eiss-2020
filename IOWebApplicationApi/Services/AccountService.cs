// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Services;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplicationApi.Contracts;
using IOWebApplicationApi.Helper;
using IOWebApplicationApi.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Sec;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplicationApi.Services
{
    public class AccountService : BaseService, IAccountService
    {
        private readonly IConfiguration configuration;
        public AccountService(
            IConfiguration _configuration,
            ILogger<DeliveryAccount> _logger,
            IRepository _repo)
        {
            logger = _logger;
            repo = _repo;
            configuration = _configuration;
        }
   
        public async Task<string> GenerateJwtToken(DeliveryAccount account)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.MobileUserId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, account.Id),
                new Claim(CustomClaimType.CourtId, account.CourtId.ToString()),
                new Claim(CustomClaimType.LawUnitId, account.LawUnitId.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            //var expires = DateTime.Now.AddDays(Convert.ToDouble(configuration["JwtExpireDays"]));
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(configuration["JwtExpireMinutes"]));


            var token = new JwtSecurityToken(
                configuration["JwtIssuer"],
                configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
     
        private async Task<string> GenerateJwtMobileToken(DeliveryAccount account)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.MobileUserId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, account.Id),
                new Claim(CustomClaimType.CourtId, account.CourtId.ToString()),
                new Claim(CustomClaimType.LawUnitId, account.LawUnitId.ToString()),
            };

            // var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtMobileKey"]));
            //  var creds = new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256); // .EcdsaSha512); // HmacSha256);
            string privateKey = configuration["JwtMobileKey"];
            ECDsa eCDsa = EDCsaHelper.LoadPrivateKey(EDCsaHelper.FromHexString(privateKey));
            var key = new ECDsaSecurityKey(eCDsa);
            var creds = new SigningCredentials(key, SecurityAlgorithms.EcdsaSha512); // .EcdsaSha512); // HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(configuration["JwtMobileExpireDays"]));

            var token = new JwtSecurityToken(
                configuration["JwtMobileIssuer"],
                configuration["JwtMobileIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
     
        public async Task<DeliveryAccountVM> Register(string registerGuid)
        {
            
            var account = repo.All<DeliveryAccount>()
                              .Where(x => x.Id == registerGuid && 
                                          x.IsActive &&
                                          string.IsNullOrEmpty(x.MobileToken))
                              .FirstOrDefault();
            if (account == null)
                return null;
            var user = repo.All<ApplicationUser>()
                           .Include(x => x.LawUnit)
                           .Include(x => x.Court)
                           .Where(x => x.Id == account.MobileUserId)
                           .FirstOrDefault();
            if (user == null)
                return null;
            if (user.LawUnit == null)
                return null;
            if (user.Court == null)
                return null;
            account.MobileToken = await GenerateJwtMobileToken(account);
            repo.SaveChanges();
            return new DeliveryAccountVM() 
            {
                CourtId = account.CourtId,
                MobileUserId= account.MobileUserId,
                MobileToken = account.MobileToken,
                UserName = user.Email,
                FullName = user.LawUnit.FullName,
                CourtName = user.Court.Label,
                ApiAddress = configuration["MobileApiURI"],
                RegisterGuid = account.Id
            };
        }
      
       
        public bool SavePin(string registerGuid, string pin)
        {
            var account = repo.All<DeliveryAccount>()
                                    .Where(x => x.Id == registerGuid &&
                                                x.IsActive &&
                                                string.IsNullOrEmpty(x.PinHash))
                                    .FirstOrDefault();
            if (account == null)
                return false;
            account.PinHash = BCrypt.Net.BCrypt.HashPassword(pin);
            repo.SaveChanges();
            return true;
        }

        public async Task<string> LoginPin(string registerGuid, string pin)
        {
            var account = repo.All<DeliveryAccount>()
                                    .Where(x => x.Id == registerGuid &&
                                                x.DateExpired == null)
                                    .FirstOrDefault();
            if (account == null)
                return "";
            if (BCrypt.Net.BCrypt.Verify(pin, account.PinHash))
               return await GenerateJwtToken(account);
            return "";
        }
    }
}
