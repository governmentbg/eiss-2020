using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;

namespace IOWebApplication.Infrastructure.Services.Mocks
{
    public class MockUserContext : IUserContext
    {
        public int CourtId => 0;

        public int CourtTypeId => throw new NotImplementedException();

        public string CourtName => throw new NotImplementedException();

        public int[] CourtInstances => throw new NotImplementedException();

        public int LawUnitId => 0;

        public string UserId => null;

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
