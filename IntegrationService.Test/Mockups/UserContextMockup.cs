using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using static IOWebApplication.Infrastructure.Constants.AccountConstants;

namespace IntegrationService.Test.Mockups
{
    /// <summary>
    /// User context Mockup for unit testing
    /// </summary>
    public class UserContextMockup : IUserContext
    {
        /// <summary>
        /// Mocked value (1)
        /// </summary>
        public int CourtId => 1;

        /// <summary>
        /// Mocked value (1)
        /// </summary>
        public int CourtTypeId => 1;

        /// <summary>
        /// Mocked value (Окръжен съд Враца)
        /// </summary>
        public string CourtName => "Окръжен съд Враца";

        /// <summary>
        /// Mocked value
        /// </summary>
        public int[] CourtInstances => new int[] { 1 };

        /// <summary>
        /// Mocked value (1)
        /// </summary>
        public int LawUnitId => 1;

        /// <summary>
        /// Mocked value (user-id-guid)
        /// </summary>
        public string UserId => "user-id-guid";

        /// <summary>
        /// Mocked value (courtuser@eiss.bg)
        /// </summary>
        public string Email => "courtuser@eiss.bg";

        /// <summary>
        /// Mocked value
        /// </summary>
        public string LogName => "TestUser";

        /// <summary>
        /// Mocked value (Тестови Потребител)
        /// </summary>
        public string FullName => "Тестови Потребител";

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

        /// <summary>
        /// Mocked value, true if courtId == 1
        /// </summary>
        public bool IsUserInCourt(int courtId)
        {
            return courtId == CourtId;
        }

        /// <summary>
        /// Mocked value true for (MODULE_DOCUMENTS or MODULE_CASE_DATA)
        /// </summary>
        public bool IsUserInFeature(string feature)
        {
            var features = new string[] 
            {
                Features.Modules.Documents,
                Features.Modules.CaseData
            };

            return features.Contains(feature);
        }

        /// <summary>
        /// Mocked value true for 
        /// (CASE_EDIT, DOC_EDIT, DOC_INIT)
        /// </summary>
        public bool IsUserInRole(string role)
        {
            var roles = new string[] 
            {
                Roles.CaseEdit,
                Roles.DocumentEdit,
                Roles.DocumentInit
            };

            return roles.Contains(role);
        }

        /// <summary>
        /// Mocked value
        /// </summary>
        public Task<UserSettingsModel> Settings()
        {
            var userSettings = new UserSettingsModel();

            return Task.FromResult(userSettings);
        }
    }
}
