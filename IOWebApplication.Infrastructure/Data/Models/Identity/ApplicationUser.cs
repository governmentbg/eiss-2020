using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<ApplicationUserClaim> Claims { get; set; }
        public virtual ICollection<ApplicationUserLogin> Logins { get; set; }
        public virtual ICollection<ApplicationUserToken> Tokens { get; set; }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

        public int CourtId { get; set; }
        public int LawUnitId { get; set; }
        public bool MustChangePassword { get; set; }
        public bool IsActive { get; set; }

        public virtual Court Court { get; set; }
        public virtual LawUnit LawUnit { get; set; }

        public string EissId { get; set; }
        public string UserSettings { get; set; }

        public bool? WorkNotificationToMail { get; set; }

        [NotMapped]
        public bool PasswordLogin { get; set; }
    }
}
