using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Identity
{
    [Table("roles")]
    public class ApplicationRoleStore : RoleStore<ApplicationRole>
    {
        public ApplicationRoleStore(ApplicationDbContext context) : base(context)
        {
        }
    }
}
