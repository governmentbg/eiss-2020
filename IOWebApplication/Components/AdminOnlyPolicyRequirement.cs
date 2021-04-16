using IOWebApplication.Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Components
{
    /// <summary>
    /// Проверка за роли Администратор или Администратор на инфраструктурата
    /// </summary>
    public class AdminOnlyPolicyRequirement : AuthorizationHandler<AdminOnlyPolicyRequirement>, IAuthorizationRequirement
    {
        public const string Name = "AdminOnlyPolicy";

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminOnlyPolicyRequirement requirement)
        {
            if (context.User == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }
            bool isAdmin = context.User.IsInRole(AccountConstants.Roles.Administrator);
            bool isGlobalAdmin = context.User.IsInRole(AccountConstants.Roles.GlobalAdministrator);

            if (!isAdmin && !isGlobalAdmin)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
