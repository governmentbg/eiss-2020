using IOWebApplication.Infrastructure.Data.Common;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Data.Models.Identity
{
    [Table("users")]
    public class ApplicationUserStore : UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, string, ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, ApplicationUserToken, ApplicationRoleClaim>
    {
        //private DbSet<ApplicationUserRole> UserRoles { get { return Context.Set<ApplicationUserRole>(); } }

        public ApplicationUserStore(ApplicationDbContext context) : base(context)
        {

        }

        ///// <summary>
        ///// Return a user role for the userId and roleId if it exists.
        ///// </summary>
        ///// <param name="userId">The user's id.</param>
        ///// <param name="roleId">The role's id.</param>
        ///// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        ///// <returns>The user role if it exists.</returns>
        //protected override Task<ApplicationUserRole> FindUserRoleAsync(string userId, string roleId, CancellationToken cancellationToken)
        //{
        //    return UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        //}
    }
}
