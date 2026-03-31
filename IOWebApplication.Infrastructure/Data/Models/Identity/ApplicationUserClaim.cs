using Microsoft.AspNetCore.Identity;
using System;

namespace IOWebApplication.Infrastructure.Data.Models.Identity
{
    public class ApplicationUserClaim : IdentityUserClaim<string>
    {
        public virtual ApplicationUser User { get; set; }

        private DateTime now { get; set; }
        public ApplicationUserClaim()
        {
            now = DateTime.Now;
        }
    }
}
