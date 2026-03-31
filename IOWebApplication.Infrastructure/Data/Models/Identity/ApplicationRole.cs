using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace IOWebApplication.Infrastructure.Data.Models.Identity
{
    public class ApplicationRole : IdentityRole
    {
        public string Code { get; set; }

        public string Label { get; set; }

        public int? OrderNumber { get; set; }

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
        public virtual ICollection<ApplicationRoleClaim> RoleClaims { get; set; }
    }
}
