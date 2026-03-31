// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DataTables.AspNet.AspNetCore;
using Elasticsearch.Net;
using IO.RegixClient;
using IO.SignTools.Extensions;
using IO.SignTools.Models;
using IOWebApplication.Components;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.ModelBinders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rotativa.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace IOWebApplication.Extensions
{
    public static class ProgramExtensions
    {
        public static string MachineName = "";

        public static void ConfigureBuilder(this WebApplicationBuilder builder)
        {
            builder.Configuration.AddJsonFile("hosting.json", true, true).AddEnvironmentVariables("ASPNETCORE_");
            builder.WebHost.ConfigureKestrel(kestrel =>
            {
                //kestrel.Limits.MaxRequestBodySize = null;
                //kestrel.Limits.MaxRequestLineSize = 100000;
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (environment == "LocalDebug" || environment == "Development")
                {
                    kestrel.Listen(IPAddress.Loopback, 8080, portOptions =>
                    {
                        //var certificate = new X509Certificate("Certificates/eiss.local.pfx", "123456");
                        try
                        {
                            //portOptions.UseHttps("Certificates/eiss.local.pfx", "123456");
                            // portOptions.UseHttps( certificate.GetRawCertDataString());
                            var certificate = new X509Certificate2("Certificates/eiss.local.pfx", "123456");
                            portOptions.UseHttps(new HttpsConnectionAdapterOptions
                            {
                                ServerCertificate = certificate
                            });
                        }
                        catch (Exception ex) { }
                    });
                }
            });
        }

        public static void ConfigureProgramServices(this IServiceCollection services, IConfiguration configuration)
        {
            //LogFactory = loggerFactory;
            string[] elasticSearchUris = configuration.GetValue<string>("ElasticSearchURIs")?.Split(',', StringSplitOptions.RemoveEmptyEntries);
            List<Uri> elasticSearchPoolUris = new List<Uri>();

            foreach (var uri in elasticSearchUris)
            {
                elasticSearchPoolUris.Add(new Uri(uri));
            }

            var pool = new StaticConnectionPool(elasticSearchPoolUris);

            Log.Logger = new LoggerConfiguration()
               .Enrich.FromLogContext()
               .Enrich.WithProperty("MachineName", MachineName)
               .MinimumLevel.Error()
               .WriteTo
               .Elasticsearch(new ElasticsearchSinkOptions(pool)
               {
                   MinimumLogEventLevel = LogEventLevel.Warning,
                   AutoRegisterTemplate = true,
                   NumberOfShards = 3,
                   NumberOfReplicas = 2,
                   IndexFormat = "eiss-log-net8-{0:dd.MM.yyyy}"
               })
            .CreateLogger();

            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));


            int cookieMaxAgeMinutes = configuration.GetValue<int>("Authentication:CookieMaxAgeMinutes");

            // За добавяне на контексти, използвайте extension метода!!!
            services.AddAppDbContext(configuration);

            services.AddDataProtection().PersistKeysToDbContext<ApplicationDbContext>();

            services.AddAuthentication(x =>
            {
                x.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(cookieMaxAgeMinutes);
            })
            .AddStampIT(options =>
            {
                options.AppId = configuration.GetValue<string>("Authentication:StampIT:AppId");
                options.AppSecret = configuration.GetValue<string>("Authentication:StampIT:AppSecret");
                options.Scope.Add("pid");
                options.Scope.Add("certificate");
                options.ClaimActions.DeleteClaim(ClaimTypes.NameIdentifier);
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "pid");
                options.ClaimActions.MapJsonKey(CustomClaimType.IdStampit.Certificate, "certificate");
                options.ClaimActions.MapJsonKey(CustomClaimType.IdStampit.CertificateNumber, "certno");
                options.AuthorizationEndpoint = configuration.GetValue<string>("Authentication:StampIT:AuthorizationEndpoint");
                options.TokenEndpoint = configuration.GetValue<string>("Authentication:StampIT:TokenEndpoint");
                options.UserInformationEndpoint = configuration.GetValue<string>("Authentication:StampIT:UserInformationEndpoint");
                options.Events = new OAuthEvents()
                {
                    OnRemoteFailure = context => HandleRemoteFailure(context)
                };
            });

            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
            })
             .AddSignInManager()
             .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, string, ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, ApplicationUserToken, ApplicationRoleClaim>>()
             //.AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, string, ApplicationUserRole, ApplicationRoleClaim>>()
             //.AddDefaultTokenProviders()
             .AddEntityFrameworkStores<ApplicationDbContext>();



            services.AddAuthorization(options =>
            {
                options.AddPolicy(AdminOnlyPolicyRequirement.Name, policy =>
                    policy.Requirements.Add(new AdminOnlyPolicyRequirement()));
            });



            services.ConfigureApplicationCookie(options =>
            {
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(cookieMaxAgeMinutes);
                options.Cookie.IsEssential = true;
                //options.Cookie.MaxAge = options.ExpireTimeSpan;
                //options.Events.OnSigningIn = ctx =>
                //{
                //    if (ctx.Properties.IsPersistent)
                //    {
                //        var issued = ctx.Properties.IssuedUtc ?? DateTimeOffset.UtcNow;
                //        ctx.Properties.ExpiresUtc = issued.AddMinutes(cookieMaxAgeMinutes);
                //    }
                //    return Task.FromResult(0);
                //};
            });


            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationClaimsPrincipalFactory>();

            // За добавяне на услуги, използвайте extension метода!!!
            services.AddApplicationServices(configuration);

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new CultureInfo[]
                                     {
                                         new CultureInfo("bg")
                                     };

                options.DefaultRequestCulture = new RequestCulture("bg");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });



            var loggerFactory = services.BuildServiceProvider()
                    .GetService<ILoggerFactory>();


            services.AddMvcCore()
               .AddMvcOptions(options =>
               {
                   options.MaxModelBindingCollectionSize = 20000;
                   options.ModelBinderProviders.Insert(0, new NomenclatureModelBinderProvider(loggerFactory));
                   options.ModelBinderProviders.Insert(1, new DecimalModelBinderProvider());
                   options.ModelBinderProviders.Insert(2, new DoubleModelBinderProvider());
                   options.ModelBinderProviders.Insert(3, new DateTimeModelBinderProvider(FormattingConstant.NormalDateFormat));
               })
               .AddViewLocalization(
              LanguageViewLocationExpanderFormat.Suffix,
              opts => opts.ResourcesPath = "Resources");

            services.RegisterDataTables();

            services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
            });

            TimestampClientOptions tsOptions = new TimestampClientOptions();

            if (!string.IsNullOrEmpty(configuration.GetValue<string>("Authentication:StampIT:Timestamp:Token")))
            {
                tsOptions = new TimestampClientOptions()
                {
                    Token = configuration.GetValue<string>("Authentication:StampIT:Timestamp:Token"),
                    TimestampEndpoint = configuration.GetValue<string>("Authentication:StampIT:Timestamp:TimestampEndpoint"),
                    ValidateEndpoint = configuration.GetValue<string>("Authentication:StampIT:Timestamp:ValidateEndpoint")
                };
            }

            VerificationServiceOptions vsOptions = new VerificationServiceOptions()
            {
                Token = configuration.GetValue<string>("Authentication:StampIT:VerificationService:Token"),
                VerificationServiceEndpoint = configuration.GetValue<string>("Authentication:StampIT:VerificationService:VerificationServiceEndpoint"),
                ClientId = configuration.GetValue<string>("Authentication:StampIT:VerificationService:ClientId")
            };

            services.AddIOSignTools(options =>
            {
                options.TempDir = configuration.GetValue<string>("TempPdfDir");
                options.HashAlgorithm = System.Security.Cryptography.HashAlgorithmName.SHA256.Name;
                options.TimestampOptions = tsOptions;
                options.VerificationServiceOptions = vsOptions;
            });

            services.AddIoRegixClient(options =>
            {
                options.CertificatePath = configuration.GetValue<string>("Regix:Certificate");
                options.Password = configuration.GetValue<string>("Regix:Password");
                options.ClientType = configuration.GetValue<bool>("Regix:IsInProduction") ? ClientType.Production : ClientType.Test;
                options.UseNewEndpoint = configuration.GetValue<bool>("Regix:UseNewEndpoint", true);
            });

            //services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

            services.AddHealthChecks();

            services.AddMvc();
        }

        public static void ConfigureApplication(this WebApplication app, IConfiguration configuration)
        {
            var env = app.Environment;

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            var ttt = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT ");

            if (env.IsDevelopment() || env.EnvironmentName == "Docker")
            {
                //app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.Use((authContext, next) =>
            {
                authContext.Request.Scheme = "https";
                if (next != null)
                {
                    return next();
                }
                return null;
            });

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            app.UseRouting();


            app.UseRequestLocalization();
            app.UseStaticFiles();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");


            app.UseHealthChecks("/health");
            RotativaConfiguration.Setup(env.WebRootPath, configuration.GetValue<string>("RotativaLibRelativePath"));
        }

        private static Task HandleRemoteFailure(RemoteFailureContext context, string certErrorPath = "/home/logincerterror?error=")
        {
            //context.Response.Redirect($"{certErrorPath}{context.Failure}");

            var message = Regex.Replace(context.Failure.Message, @"[^\u001F-\u007F]+", string.Empty);
            if (!string.IsNullOrEmpty(message))
            {
                message = HttpUtility.UrlEncode(message);
            }
            context.Response.Redirect("/Home/Error?message=" + message);

            context.HandleResponse();

            return Task.FromResult(0);
        }
    }
}
