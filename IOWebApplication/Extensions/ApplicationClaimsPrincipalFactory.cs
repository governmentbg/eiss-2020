using AspNetCoreGeneratedDocument;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace IOWebApplication.Extensions
{
    public class ApplicationClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
    {
        private readonly IRepository repo;
        private readonly IConfiguration configuration;
        public ApplicationClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IOptions<IdentityOptions> options,
            IRepository _repo,
            IConfiguration _configuration) : base(userManager, roleManager, options)
        {
            repo = _repo;
            configuration = _configuration;
        }

        public async override Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            ClaimsIdentity claimsIdentity = new ClaimsIdentity("Identity.Application");
            //, Options.ClaimsIdentity.UserNameClaimType,
            //Options.ClaimsIdentity.RoleClaimType);
            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, user.Email));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
            claimsIdentity.AddClaim(new Claim(new ClaimsIdentityOptions().SecurityStampClaimType, user.SecurityStamp));
            if (user.LawUnit == null)
            {
                user.LawUnit = await repo.AllReadonly<LawUnit>()
                                         .Where(x => x.Id == user.LawUnitId)
                                         .FirstOrDefaultAsync();
                string[] courtListIds = await repo.AllReadonly<CourtLawUnit>()
                                        .Where(x => x.LawUnitId == user.LawUnitId && x.DateExpired == null)
                                        .Where(x => NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(x.PeriodTypeId))
                                        .Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                        .Select(x => x.CourtId.ToString())
                                        .ToArrayAsync();

                string courtList = string.Join(',', courtListIds);
                claimsIdentity.AddClaim(new Claim(CustomClaimType.CourtList, courtList));
            }
            claimsIdentity.AddClaim(new Claim(CustomClaimType.FullName, System.Web.HttpUtility.HtmlDecode(user.LawUnit.FullName)));
            claimsIdentity.AddClaim(new Claim(CustomClaimType.Uic, user.LawUnit.Uic ?? ""));
            claimsIdentity.AddClaim(new Claim(CustomClaimType.CourtId, user.CourtId.ToString()));
            claimsIdentity.AddClaim(new Claim(CustomClaimType.LawUnitId, user.LawUnitId.ToString()));
            claimsIdentity.AddClaim(new Claim(CustomClaimType.LawUnitTypeId, user.LawUnit.LawUnitTypeId.ToString()));
            var courtInfo = await repo.AllReadonly<Court>()
                                    .Include(x => x.CourtType)
                                    .Where(x => x.Id == user.CourtId)
                                    .Select(x => new
                                    {
                                        Label = x.Label,
                                        CourtTypeId = x.CourtTypeId,
                                        InstanceList = x.CourtType.InstanceList
                                    })
                                    .FirstOrDefaultAsync();
            if (courtInfo != null)
            {
                claimsIdentity.AddClaim(new Claim(CustomClaimType.CourtName, courtInfo.Label));
                claimsIdentity.AddClaim(new Claim(CustomClaimType.CourtTypeId, courtInfo.CourtTypeId.ToString()));
                claimsIdentity.AddClaim(new Claim(CustomClaimType.InstanceList, courtInfo.InstanceList));
                claimsIdentity.AddClaim(new Claim(CustomClaimType.OrganizationList, await userCourtOrganizations(user)));
                claimsIdentity.AddClaim(new Claim(CustomClaimType.SubDepartments, await generateSubDepartmentList(user)));
                claimsIdentity.AddClaim(new Claim(CustomClaimType.SubDocRegistry, await generateSubRegistryList(user)));
                claimsIdentity.AddClaim(new Claim(CustomClaimType.SystemFeatures, await systemFeatures()));
            }

            if (user.PasswordLogin)
            {
                claimsIdentity.AddClaim(new Claim("PasswordLogin", "true"));
            }

            var roles = await repo.AllReadonly<ApplicationUserRole>()
                                    .Where(x => x.UserId == user.Id)
                                    .Select(x => x.Role.Name)
                                    .ToArrayAsync();

            foreach (var role in roles)
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            var certNo = await repo.AllReadonly<ApplicationUserClaim>()
                                    .Where(x => x.UserId == user.Id)
                                    .Where(x => x.ClaimType == CustomClaimType.IdStampit.CertificateNumber)
                                    .Select(x => x.ClaimValue)
                                    .FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(certNo))
            {
                claimsIdentity.AddClaim(new Claim(CustomClaimType.IdStampit.CertificateNumber, certNo));
            }

            string[] euroDataParamNames = new string[] { NomenclatureConstants.SystemParamName.InterimPeriodEuroStart, NomenclatureConstants.SystemParamName.InterimPeriodEuroEnd,
                                                         NomenclatureConstants.SystemParamName.EuroExchangeRate };
            var euroParams = await repo.AllReadonly<SystemParam>()
                                       .Where(x => euroDataParamNames.Contains(x.ParamName))
                                       .ToListAsync();

            claimsIdentity.AddClaim(new Claim(CustomClaimType.InterimPeriodEuroStart, euroParams.Where(x => x.ParamName == NomenclatureConstants.SystemParamName.InterimPeriodEuroStart).Select(x => x.ParamValue).DefaultIfEmpty("").FirstOrDefault()));
            claimsIdentity.AddClaim(new Claim(CustomClaimType.InterimPeriodEuroEnd, euroParams.Where(x => x.ParamName == NomenclatureConstants.SystemParamName.InterimPeriodEuroEnd).Select(x => x.ParamValue).DefaultIfEmpty("").FirstOrDefault()));
            claimsIdentity.AddClaim(new Claim(CustomClaimType.EuroExchangeRate, euroParams.Where(x => x.ParamName == NomenclatureConstants.SystemParamName.EuroExchangeRate).Select(x => x.ParamValue).DefaultIfEmpty("").FirstOrDefault()));
            claimsIdentity.AddClaim(new Claim(CustomClaimType.EnvName, configuration.GetValue<string>("Environment:Name", NomenclatureConstants.Environments.Production)));

            GenericPrincipal genericPrincipal = new GenericPrincipal(claimsIdentity, null);
            var result = new ClaimsPrincipal(genericPrincipal);

            //var oldprincipal = await base.CreateAsync(user);

            return result;
        }

        /*
        public async Task<ClaimsPrincipal> CreateAsyncOld(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);
            if (user.LawUnit == null)
            {
                user.LawUnit = await repo.GetByIdAsync<LawUnit>(user.LawUnitId);
                string[] courtListIds = await repo.AllReadonly<CourtLawUnit>()
                                        .Where(x => x.LawUnitId == user.LawUnitId && x.DateExpired == null)
                                        .Where(x => NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(x.PeriodTypeId))
                                        .Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                        .Select(x => x.CourtId.ToString())
                                        .ToArrayAsync();

                string courtList = string.Join(',', courtListIds);
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.CourtList, courtList));
            }
            ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.FullName, System.Web.HttpUtility.HtmlDecode(user.LawUnit.FullName)));
            ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.Uic, user.LawUnit.Uic ?? ""));
            ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.CourtId, user.CourtId.ToString()));
            ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.LawUnitId, user.LawUnitId.ToString()));
            ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.LawUnitTypeId, user.LawUnit.LawUnitTypeId.ToString()));
            var courtInfo = await repo.AllReadonly<Court>()
                                    .Include(x => x.CourtType)
                                    .Where(x => x.Id == user.CourtId)
                                    .Select(x => new
                                    {
                                        Label = x.Label,
                                        CourtTypeId = x.CourtTypeId,
                                        InstanceList = x.CourtType.InstanceList
                                    })
                                    .FirstOrDefaultAsync();
            if (courtInfo != null)
            {
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.CourtName, courtInfo.Label));
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.CourtTypeId, courtInfo.CourtTypeId.ToString()));
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.InstanceList, courtInfo.InstanceList));
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.OrganizationList, await userCourtOrganizations(user)));
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.SubDepartments, await generateSubDepartmentList(user)));
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.SubDocRegistry, await generateSubRegistryList(user)));
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.SystemFeatures, await systemFeatures()));

            }

            if (user.PasswordLogin)
            {
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim("PasswordLogin", "true"));
            }
            return principal;
        }*/

        private async Task<string> systemFeatures()
        {
            return await repo.AllReadonly<SystemParam>()
                       .Where(x => x.ParamName == NomenclatureConstants.SystemParamName.SystemFeatures)
                       .Select(x => x.ParamValue)
                       .FirstOrValueAsync("");
        }

        private async Task<string> userCourtOrganizations(ApplicationUser user)
        {
            var orgList = await repo.AllReadonly<CourtLawUnit>()
                                               .Where(x => x.CourtId == user.CourtId && x.DateExpired == null
                                               && x.LawUnitId == user.LawUnitId
                                               && (NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(x.PeriodTypeId))
                                               && (x.DateTo ?? DateTime.Now) >= DateTime.Now.AddMinutes(-1))
                                               .Select(x => x.CourtOrganizationId ?? 0)
                                               .Where(x => x > 0)
                                               .ToListAsync();
            return orgList.ConcatenateWithSeparator();

        }

        private async Task<string> generateSubDepartmentList(ApplicationUser user)
        {

            var depId = await repo.AllReadonly<CourtDepartmentLawUnit>()
                                    .Include(x => x.CourtDepartment)
                                    .Where(x => x.LawUnitId == user.LawUnitId && x.CourtDepartment.CourtId == user.CourtId)
                                    .Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                    .Select(x => x.CourtDepartmentId)
                                    .FirstOrValueAsync(0);
            if (depId > 0)
            {
                List<int> depList = new List<int>(){
                    depId
                };
                await appendSubDepartments(depList, depId);

                return depList.ConcatenateWithSeparator();
            }
            return string.Empty;
        }

        private async Task appendSubDepartments(List<int> depList, int depId)
        {
            var subDepartments = await repo.AllReadonly<CourtDepartment>()
                                     .Where(x => x.ParentId == depId)
                                     .Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                     .Select(x => x.Id).ToListAsync();

            if (subDepartments != null)
                foreach (var dep in subDepartments)
                {
                    depList.Add(dep);
                    await appendSubDepartments(depList, dep);
                }
        }

        private async Task<string> generateSubRegistryList(ApplicationUser user)
        {

            var orgInfo = await repo.AllReadonly<CourtLawUnit>()
                                    .Include(x => x.CourtOrganization)
                                    .Where(x => x.LawUnitId == user.LawUnitId && x.CourtId == user.CourtId && x.DateExpired == null)
                                    .Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                    .Where(x => NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(x.PeriodTypeId))
                                    .Select(x => new
                                    {
                                        OrgId = x.CourtOrganizationId ?? 0,
                                        IsRegistry = (x.CourtOrganization != null) ? x.CourtOrganization.IsDocumentRegistry ?? false : false
                                    }).FirstOrDefaultAsync();
            if (orgInfo != null)
            {
                List<int> orgList = new List<int>();
                if (orgInfo.IsRegistry)
                {
                    //Ако е регистратура, се добавя като първи елемент - текущата организационна структура на служителя
                    orgList.Add(orgInfo.OrgId);
                }
                else
                {
                    //Ако не е регистратура, се добавя нулева стойност. Само POWER_USER може да вижда подчинените му регистратури
                    orgList.Add(NomenclatureConstants.NullVal);
                }


                await appendSubRegistry(orgList, orgInfo.OrgId);

                return orgList.ConcatenateWithSeparator();
            }
            return string.Empty;
        }

        private async Task appendSubRegistry(List<int> orgList, int orgId)
        {
            var subOrganizations = await repo.AllReadonly<CourtOrganization>()
                                     .Where(x => x.ParentId == orgId)
                                     .Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                     .Select(x => new
                                     {
                                         x.Id,
                                         x.IsDocumentRegistry
                                     })
                                     .ToListAsync();
            if (subOrganizations != null)
                foreach (var org in subOrganizations)
                {
                    if (org.IsDocumentRegistry == true)
                    {
                        orgList.Add(org.Id);
                    }
                    await appendSubRegistry(orgList, org.Id);
                }
        }
    }
}
