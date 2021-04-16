using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IOWebApplication.Extensions
{
    public class ApplicationClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
    {
        private readonly IRepository repo;
        public ApplicationClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IOptions<IdentityOptions> options,
            IRepository _repo) : base(userManager, roleManager, options)
        {
            repo = _repo;
        }

        public async override Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);
            if (user.LawUnit == null)
            {
                user.LawUnit = repo.GetById<LawUnit>(user.LawUnitId);
                string[] courtListIds = repo.AllReadonly<CourtLawUnit>()
                                        .Where(x => x.LawUnitId == user.LawUnitId)
                                        .Where(x => NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(x.PeriodTypeId))
                                        .Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                        .Select(x => x.CourtId.ToString())
                                        .ToArray();

                string courtList = string.Join(',', courtListIds);
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.CourtList, courtList));
            }
            ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.FullName, user.LawUnit.FullName));
            ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.Uic, user.LawUnit.Uic ?? ""));
            ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.CourtId, user.CourtId.ToString()));
            ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.LawUnitId, user.LawUnitId.ToString()));
            var courtInfo = repo.AllReadonly<Court>()
                                    .Include(x => x.CourtType)
                                    .Where(x => x.Id == user.CourtId)
                                    .Select(x => new
                                    {
                                        Label = x.Label,
                                        CourtTypeId = x.CourtTypeId,
                                        InstanceList = x.CourtType.InstanceList
                                    })
                                    .FirstOrDefault();
            if (courtInfo != null)
            {
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.CourtName, courtInfo.Label));
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.CourtTypeId, courtInfo.CourtTypeId.ToString()));
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.InstanceList, courtInfo.InstanceList));
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.OrganizationList, userCourtOrganizations(user)));
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.SubDepartments, generateSubDepartmentList(user)));
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(CustomClaimType.SubDocRegistry, generateSubRegistryList(user)));

            }

            if (user.PasswordLogin)
            {
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim("PasswordLogin", "true"));
            }
            return principal;
        }

        private string userCourtOrganizations(ApplicationUser user)
        {
            var orgList = repo.AllReadonly<CourtLawUnit>()
                                               .Where(x => x.CourtId == user.CourtId
                                               && x.LawUnitId == user.LawUnitId
                                               && (NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(x.PeriodTypeId))
                                               && (x.DateTo ?? DateTime.Now) >= DateTime.Now.AddMinutes(-1))
                                               .Select(x => x.CourtOrganizationId ?? 0)
                                               .Where(x => x > 0)
                                               .ToList();
            return orgList.ConcatenateWithSeparator();

        }

        private string generateSubDepartmentList(ApplicationUser user)
        {

            var depId = repo.AllReadonly<CourtDepartmentLawUnit>()
                                    .Include(x => x.CourtDepartment)
                                    .Where(x => x.LawUnitId == user.LawUnitId && x.CourtDepartment.CourtId == user.CourtId)
                                    .Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                    .Select(x => x.CourtDepartmentId)
                                    .DefaultIfEmpty(0).FirstOrDefault();
            if (depId > 0)
            {
                List<int> depList = new List<int>(){
                    depId
                };
                appendSubDepartments(depList, depId);

                return depList.ConcatenateWithSeparator();
            }
            return string.Empty;
        }

        private void appendSubDepartments(List<int> depList, int depId)
        {
            var subDepartments = repo.AllReadonly<CourtDepartment>()
                                     .Where(x => x.ParentId == depId)
                                     .Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                     .Select(x => x.Id).ToList();
            if (subDepartments != null)
                foreach (var dep in subDepartments)
                {
                    depList.Add(dep);
                    appendSubDepartments(depList, dep);
                }
        }

        private string generateSubRegistryList(ApplicationUser user)
        {

            var orgInfo = repo.AllReadonly<CourtLawUnit>()
                                    .Include(x => x.CourtOrganization)
                                    .Where(x => x.LawUnitId == user.LawUnitId && x.CourtId == user.CourtId)
                                    .Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                    .Where(x => NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(x.PeriodTypeId))
                                    .Select(x => new
                                    {
                                        OrgId = x.CourtOrganizationId ?? 0,
                                        IsRegistry = (x.CourtOrganization != null) ? x.CourtOrganization.IsDocumentRegistry ?? false : false
                                    }).FirstOrDefault();
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


                appendSubRegistry(orgList, orgInfo.OrgId);

                return orgList.ConcatenateWithSeparator();
            }
            return string.Empty;
        }

        private void appendSubRegistry(List<int> orgList, int orgId)
        {
            var subOrganizations = repo.AllReadonly<CourtOrganization>()
                                     .Where(x => x.ParentId == orgId)
                                     .Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                     .ToList();
            if (subOrganizations != null)
                foreach (var org in subOrganizations)
                {
                    if (org.IsDocumentRegistry == true)
                    {
                        orgList.Add(org.Id);
                    }
                    appendSubRegistry(orgList, org.Id);
                }
        }
    }
}
