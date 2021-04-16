using IOWebApplication.Core.Helper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IOWebApplication.Extensions
{
    public static class IOIdentityProviderConnectOptionsExtensions
    {
        /// <summary>
        /// Добавя общи параметри за настройка на Claims от и към IO Identity Server
        /// </summary>
        /// <param name="options"></param>
        /// <param name="Configuration">Достъп до конфигурационните файлове</param>
        public static void AddIOSettings(this OpenIdConnectOptions options, IConfiguration Configuration)
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.RequireHttpsMetadata = false;
            options.ResponseType = "code id_token";
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            //options.Scope.Add("email");
            //options.Scope.Add("roles");
            //options.Scope.Add("iodemoapi");
            //options.Scope.Add("tenant");
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.ClaimActions.MapJsonKey("tenant_id", "tenant_id");
            options.ClaimActions.MapJsonKey("full_name", "full_name");
            options.ClaimActions.MapJsonKey("tenant_name", "tenant_name");
            options.ClaimActions.MapJsonKey("branch_id", "branch_id");
            options.ClaimActions.MapJsonKey("branch_code", "branch_code");
            options.ClaimActions.Add(new ArrayToClaimsClaimAction("role", "role", "role"));
            options.Events = new OpenIdConnectEvents()
            {
                OnTokenValidated = tokenValidatedContext =>
                {
                    var identity = tokenValidatedContext.Principal.Identity as ClaimsIdentity;
                    var subjectClaim = identity.Claims.FirstOrDefault(c => c.Type == "sub");
                    var newClaimsIdentity = new ClaimsIdentity(
                        OpenIdConnectDefaults.AuthenticationScheme,
                        "full_name",
                        "role");

                    newClaimsIdentity.AddClaim(subjectClaim);

                    tokenValidatedContext.Principal = new ClaimsPrincipal(newClaimsIdentity);

                    return Task.FromResult(0);
                }
            };
        }
    }
}
