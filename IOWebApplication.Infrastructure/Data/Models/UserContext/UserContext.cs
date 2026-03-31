using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Data.Models.UserContext
{
    public class UserContext : IUserContext
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
        public UserContext(IHttpContextAccessor _ca)
        {
            context = _ca.HttpContext;
        }


        public string UserId
        {
            get
            {
                string userId = null;

                if (User != null && User.Claims != null && User.Claims.Count() > 0)
                {
                    var subClaim = User.Claims
                        .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                    if (subClaim != null)
                    {
                        userId = subClaim.Value;
                    }
                }

                return userId;
            }
        }
        public string Email
        {
            get
            {
                string email = string.Empty;

                if (User != null && User.Claims != null && User.Claims.Count() > 0)
                {

                    var claimEmail = User.Claims
                        .FirstOrDefault(c => c.Type == ClaimTypes.Name);

                    email = claimEmail?.Value;
                }

                return email;
            }
        }
        public string LogName
        {
            get
            {
                string logName = string.Empty;


                logName = string.Format("{0} ({1})", this.FullName, this.Email);

                return logName;
            }
        }
        public string FullName
        {
            get
            {
                string fullName = string.Empty;

                if (User != null && User.Claims != null && User.Claims.Count() > 0)
                {
                    var subClaim = User.Claims
                        .FirstOrDefault(c => c.Type == CustomClaimType.FullName);

                    if (subClaim != null)
                    {
                        fullName = subClaim.Value;
                    }
                }

                return fullName;
            }
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
        public string CourtName
        {

            get
            {
                string courtName = string.Empty;
                if (User != null && User.Claims != null && User.Claims.Count() > 0)
                {
                    var subClaim = User.Claims
                        .FirstOrDefault(c => c.Type == CustomClaimType.CourtName);

                    if (subClaim != null)
                    {
                        courtName = subClaim.Value;
                    }
                }

                return courtName;
            }

        }
        public int CourtTypeId
        {
            get
            {
                int result = 0;
                if (User != null && User.Claims != null && User.Claims.Count() > 0)
                {
                    var subClaim = User.Claims
                        .FirstOrDefault(c => c.Type == CustomClaimType.CourtTypeId);

                    if (subClaim != null)
                    {
                        result = int.Parse(subClaim.Value);
                    }
                }

                return result;
            }
        }
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

        public int LawUnitTypeId
        {
            get
            {
                //TODO
                int lawUnitTypeId = 0;
                if (User != null && User.Claims != null && User.Claims.Count() > 0)
                {
                    var subClaim = User.Claims
                        .FirstOrDefault(c => c.Type == CustomClaimType.LawUnitTypeId);

                    if (subClaim != null)
                    {
                        lawUnitTypeId = int.Parse(subClaim.Value);
                    }
                }


                return lawUnitTypeId;
            }

        }
        public int[] CourtInstances
        {
            get
            {

                List<int> instances = new List<int>();
                if (User != null && User.Claims != null && User.Claims.Count() > 0)
                {
                    var subClaim = User.Claims
                        .FirstOrDefault(c => c.Type == CustomClaimType.InstanceList);

                    if (subClaim != null)
                    {
                        foreach (var item in subClaim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        {
                            instances.Add(int.Parse(item));
                        }
                    }
                }
                if (instances.Count == 0)
                {
                    instances.Add(1);
                    instances.Add(2);
                    instances.Add(3);
                }

                return instances.ToArray();
            }
        }

        public int[] SubDocRegistry
        {
            get
            {
                List<int> result = new List<int>();
                if (User != null && User.Claims != null && User.Claims.Count() > 0)
                {
                    var subClaim = User.Claims
                        .FirstOrDefault(c => c.Type == CustomClaimType.SubDocRegistry);

                    if (subClaim != null)
                    {
                        foreach (var item in subClaim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        {
                            result.Add(int.Parse(item));
                        }
                    }
                }

                return result.ToArray();
            }
        }

        public int[] CourtOrganizations
        {
            get
            {
                List<int> result = new List<int>();
                if (User != null && User.Claims != null && User.Claims.Count() > 0)
                {
                    var subClaim = User.Claims
                        .FirstOrDefault(c => c.Type == CustomClaimType.OrganizationList);

                    if (subClaim != null)
                    {
                        foreach (var item in subClaim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        {
                            result.Add(int.Parse(item));
                        }
                    }
                }

                return result.ToArray();
            }
        }

        public bool IsUserInRole(string role)
        {
            bool result = User.IsInRole(role);
            if (!result)
            {
                return User.IsInRole(AccountConstants.Roles.GlobalAdministrator);
            }
            else
            {
                return result;
            }
        }

        public bool IsUserInFeature(string feature)
        {
            return AccountConstants.Features.IsInFeature(User, feature);
        }

        public bool IsUserInCourt(int courtId)
        {
            if (User != null && User.Claims != null && User.Claims.Count() > 0)
            {
                string courtList = User.Claims
                    .Where(c => c.Type == CustomClaimType.CourtList)
                    .Select(c => c.Value)
                    .FirstOrDefault();

                courtList = "," + courtList + ",";

                string courtCode = "," + courtId.ToString() + ",";

                return courtList.Contains(courtCode);
            }
            return false;
        }
        public string EnvironmentName
        {
            get
            {
                string result = null;
                if (User != null && User.Claims != null && User.Claims.Count() > 0)
                {
                    var subClaim = User.Claims
                        .FirstOrDefault(c => c.Type == CustomClaimType.EnvName);

                    if (subClaim != null)
                    {
                        result = subClaim.Value;
                    }
                }

                return result;
            }
        }


        public string ClaimValue(string claimType)
        {
            string result = string.Empty;

            if (User != null && User.Claims != null && User.Claims.Count() > 0)
            {

                var _claim = User.Claims
                    .FirstOrDefault(c => c.Type == claimType);

                result = _claim?.Value;
            }

            return result;
        }

        public bool IsSystemInFeature(string feature)
        {
            var claimValue = ClaimValue(CustomClaimType.SystemFeatures) ?? "";
            return claimValue.Contains("#" + feature + "$");
        }

        public string GenHash(object id, object parent = null)
        {
            var parentText = (parent != null) ? parent.ToString() : "n-a";
            var passwordAsBytes = Encoding.UTF8.GetBytes($"{DateTime.Now.Year}-{id}-{parentText}-{this.UserId}-{this.CourtTypeId}");
            var saltBytes = Encoding.UTF8.GetBytes($"{this.CourtId}-{this.LawUnitId}-{id}");
            List<byte> passwordWithSaltBytes = new List<byte>();
            passwordWithSaltBytes.AddRange(passwordAsBytes);
            passwordWithSaltBytes.AddRange(saltBytes);
            byte[] digestBytes = System.Security.Cryptography.SHA256.Create().ComputeHash(passwordWithSaltBytes.ToArray());
            return Convert.ToBase64String(digestBytes).ToLower();
        }

        public bool CheckHash(string hash, object id, object parent = null)
        {
            return hash == GenHash(id, parent);
        }

        public bool CheckHash(BlankEditVM blankModel)
        {
            return CheckHash(blankModel.SessionName, blankModel.SourceId, blankModel.SourceType);
        }

        public string CertificateNumber
        {
            get
            {
                string certNo = string.Empty;

                if (User != null && User.Claims != null && User.Claims.Count() > 0)
                {

                    var claimCertNo = User.Claims
                        .FirstOrDefault(c => c.Type == CustomClaimType.IdStampit.CertificateNumber);

                    certNo = claimCertNo?.Value;
                }

                return certNo;
            }
        }

        public bool IsInterimPeriodEuro
        {
            get
            {
                bool result = false;

                if (User != null && User.Claims != null && User.Claims.Count() > 0)
                {
                    DateTime now = DateTime.Now.Date;
                    DateTime dateStart = ParseDateEuroFromClaim(CustomClaimType.InterimPeriodEuroStart);
                    DateTime dateEnd = ParseDateEuroFromClaim(CustomClaimType.InterimPeriodEuroEnd);

                    result = dateStart <= now && dateEnd >= now;
                }

                return result;
            }
        }

        public bool IsPeriodEuro
        {
            get
            {
                bool result = false;

                if (User != null && User.Claims != null && User.Claims.Count() > 0)
                {
                    DateTime now = DateTime.Now.Date;
                    DateTime dateStart = ParseDateEuroFromClaim(CustomClaimType.InterimPeriodEuroStart);

                    result = dateStart <= now;
                }

                return result;
            }
        }

        public string CurrentCurrencyCode
        {
            get
            {
                if (IsPeriodEuro)
                    return NomenclatureConstants.CurrencyCode.EUR;
                else
                    return NomenclatureConstants.CurrencyCode.BGN;
            }
        }

        public decimal EuroExchangeRate
        {
            get
            {
                decimal result = 1;

                if (User != null && User.Claims != null && User.Claims.Count() > 0)
                {
                    var euroRateStr = ClaimValue(CustomClaimType.EuroExchangeRate);

                    result = NomenclatureExtensions.ParseDecimal(euroRateStr);
                }

                return result;
            }
        }

        private DateTime ParseDateEuroFromClaim(string claimType)
        {
            DateTime date = DateTime.Now.AddYears(1);

            try
            {
                var dateStr = ClaimValue(claimType);

                if (string.IsNullOrEmpty(dateStr) == false)
                {
                    DateTime.TryParseExact(dateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
                }
            }
            catch (Exception)
            {
                date = DateTime.Now.AddYears(1);
            }

            return date;
        }

    }

    public class DBUserContext : IDBUserContext
    {
        private IUserContext userContext;
        private IRepository repo;
        public DBUserContext(IUserContext userContext, IRepository repo)
        {
            this.userContext = userContext;
            this.repo = repo;
        }

        public async Task<UserSettingsModel> Settings()
        {

            var settings = await repo.AllReadonly<ApplicationUser>()
                                    .Where(x => x.Id == userContext.UserId)
                                    .Select(x => x.UserSettings)
                                    .FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(settings))
            {
                return JsonTextSerializer.Deserialize<UserSettingsModel>(settings);
            }
            else
            {
                return new UserSettingsModel();
            }
        }
    }
}

