using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplicationService.Services.MockUp
{
    public class UserContextMockUp : IUserContext
    {
        public int CourtId => throw new NotImplementedException();

        public int CourtTypeId => throw new NotImplementedException();

        public string CourtName => throw new NotImplementedException();

        public int[] CourtInstances => throw new NotImplementedException();

        public int LawUnitId => throw new NotImplementedException();

        public string UserId => throw new NotImplementedException();

        public string Email => throw new NotImplementedException();

        public string LogName => throw new NotImplementedException();

        public string FullName => throw new NotImplementedException();

        public int[] SubDocRegistry => throw new NotImplementedException();

        public int[] CourtOrganizations => throw new NotImplementedException();

        public string CertificateNumber => throw new NotImplementedException();

        public int LawUnitTypeId => throw new NotImplementedException();

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
            throw new NotImplementedException();
        }

        public bool IsUserInCourt(int courtId)
        {
            throw new NotImplementedException();
        }

        public bool IsUserInFeature(string feature)
        {
            throw new NotImplementedException();
        }

        public bool IsUserInRole(string role)
        {
            throw new NotImplementedException();
        }

        public Task<UserSettingsModel> Settings()
        {
            throw new NotImplementedException();
        }
    }
}
