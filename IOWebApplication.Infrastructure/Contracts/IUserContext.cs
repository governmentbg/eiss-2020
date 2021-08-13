using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Identity;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface IUserContext
    {
        int CourtId { get; }
        int CourtTypeId { get; }
        string CourtName { get; }
        int[] CourtInstances { get; }
        int[] CourtOrganizations { get; }
        int[] SubDocRegistry { get; }
        int LawUnitId { get; }
        int LawUnitTypeId { get; }
        string UserId { get; }
        string Email { get; }
        string LogName { get; }
        string FullName { get; }

        bool IsUserInRole(string role);
        bool IsUserInFeature(string feature);
        bool IsUserInCourt(int courtId);
        bool IsSystemInFeature(string feature);
        string CertificateNumber { get; }
        string ClaimValue(string claimType);

        Task<UserSettingsModel> Settings();

        string GenHash(object id, object parent = null);
        bool CheckHash(string hash, object id, object parent = null);
        bool CheckHash(BlankEditVM blankModel);
    }
}
