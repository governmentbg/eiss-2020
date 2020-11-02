// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Audit.Core;
using AutoMapper;
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
using IOWebApplication.Services;
using IOWebApplicationApi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rotativa.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IOWebApplication
{
    public class Startup
    {
        private ILoggerFactory LogFactory { get; }

        public Startup(Microsoft.Extensions.Configuration.IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            LogFactory = loggerFactory;
            string[] elasticSearchUris = Configuration.GetValue<string>("ElasticSearchURIs")?.Split(',', StringSplitOptions.RemoveEmptyEntries);
            List<Uri> elasticSearchPoolUris = new List<Uri>();

            foreach (var uri in elasticSearchUris)
            {
                elasticSearchPoolUris.Add(new Uri(uri));
            }

            var pool = new StaticConnectionPool(elasticSearchPoolUris);

            Log.Logger = new LoggerConfiguration()
               .Enrich.FromLogContext()
               .MinimumLevel.Error()
               .WriteTo
               .Elasticsearch(new ElasticsearchSinkOptions(pool)
               {
                   MinimumLogEventLevel = LogEventLevel.Warning,
                   AutoRegisterTemplate = true,
		           NumberOfShards = 3,
		           NumberOfReplicas = 2,
                   IndexFormat = "eiss-log-{0:dd.MM.yyyy}"
               })
               .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        private static Task HandleRemoteFailure(RemoteFailureContext context)
        {
            context.Response.Redirect($"/account/login_cert_error?error={context.Failure}");
            context.HandleResponse();
            return Task.FromResult(0);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            // За добавяне на контексти, използвайте extension метода!!!
            services.AddAppDbContext(Configuration);


            services.AddAuthentication(x =>
            {
                x.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

            })
            .AddCookie()
            .AddStampIT(options =>
            {
                options.AppId = Configuration.GetValue<string>("Authentication:StampIT:AppId");
                options.AppSecret = Configuration.GetValue<string>("Authentication:StampIT:AppSecret");
                options.Scope.Add("pid");
                options.ClaimActions.DeleteClaim(ClaimTypes.NameIdentifier);
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "pid");
                options.ClaimActions.MapJsonKey(CustomClaimType.IdStampit.CertificateNumber, "certno");
                options.AuthorizationEndpoint = Configuration.GetValue<string>("Authentication:StampIT:AuthorizationEndpoint");
                options.TokenEndpoint = Configuration.GetValue<string>("Authentication:StampIT:TokenEndpoint");
                options.UserInformationEndpoint = Configuration.GetValue<string>("Authentication:StampIT:UserInformationEndpoint");
                options.Events = new OAuthEvents()
                {
                    OnRemoteFailure = context => HandleRemoteFailure(context)
                };
            });

            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
             {
                 options.User.RequireUniqueEmail = false;

             })
             .AddUserStore<ApplicationUserStore>()
             .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, string, ApplicationUserRole, ApplicationRoleClaim>>()
             .AddDefaultTokenProviders();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AdminOnlyPolicyRequirement.Name, policy =>
                    policy.Requirements.Add(new AdminOnlyPolicyRequirement()));
            });

            int cookieMaxAgeMinutes = Configuration.GetValue<int>("Authentication:CookieMaxAgeMinutes");
            services.ConfigureApplicationCookie(options =>
            {
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(cookieMaxAgeMinutes);
            });

            // За добавяне на услуги, използвайте extension метода!!!
            services.AddApplicationServices();

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


            services.AddMvc(config =>
            {
                config.ModelBinderProviders.Insert(0, new NomenclatureModelBinderProvider(LogFactory));
                config.ModelBinderProviders.Insert(1, new DecimalModelBinderProvider(LogFactory));
                config.ModelBinderProviders.Insert(2, new DoubleModelBinderProvider(LogFactory));
                config.ModelBinderProviders.Insert(3, new DateTimeModelBinderProvider(FormattingConstant.NormalDateFormat, LogFactory));
            }).AddMvcLocalization(LanguageViewLocationExpanderFormat.Suffix);

            services.RegisterDataTables();

            services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
            });

            TimestampClientOptions tsOptions = null;

            if (!string.IsNullOrEmpty(Configuration.GetValue<string>("Authentication:StampIT:Timestamp:Token")))
            {
                tsOptions = new TimestampClientOptions()
                {
                    Token = Configuration.GetValue<string>("Authentication:StampIT:Timestamp:Token"),
                    TimestampEndpoint = Configuration.GetValue<string>("Authentication:StampIT:Timestamp:TimestampEndpoint"),
                    ValidateEndpoint = Configuration.GetValue<string>("Authentication:StampIT:Timestamp:ValidateEndpoint")
                };
            }

            VerificationServiceOptions vsOptions = new VerificationServiceOptions()
            {
                Token = Configuration.GetValue<string>("Authentication:StampIT:VerificationService:Token"),
                VerificationServiceEndpoint = Configuration.GetValue<string>("Authentication:StampIT:VerificationService:VerificationServiceEndpoint"),
                ClientId = Configuration.GetValue<string>("Authentication:StampIT:VerificationService:ClientId")
            };

            services.AddIOSignTools(options =>
            {
                options.TempDir = Configuration.GetValue<string>("TempPdfDir");
                options.HashAlgorithm = System.Security.Cryptography.HashAlgorithmName.SHA256.Name;
                options.TimestampOptions = tsOptions;
                options.VerificationServiceOptions = vsOptions;
            });

            services.AddIoRegixClient(options =>
            {
                options.CertificatePath = Configuration.GetValue<string>("Regix:Certificate");
                options.Password = Configuration.GetValue<string>("Regix:Password");
                options.ClientType = Configuration.GetValue<bool>("Regix:IsInProduction") ? ClientType.Production : ClientType.Test;
            });

            services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            var ttt = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT ");

            if (env.IsDevelopment() || env.EnvironmentName == "Docker")
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.Use((authContext, next) =>
            {
                authContext.Request.Scheme = "https";
                return next();
            });


            app.UseRequestLocalization();
            app.UseStaticFiles();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                  name: "areas",
                  template: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                );

                routes.MapRoute(
                  name: "default",
                  template: "{controller=Home}/{action=Index}/{id?}"
                );
            });

            RotativaConfiguration.Setup(env, Configuration.GetValue<string>("RotativaLibRelativePath"));
        }
    }
}
