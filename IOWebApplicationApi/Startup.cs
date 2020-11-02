// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IdentityServer4.AccessTokenValidation;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplicationApi.Helper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.AspNetCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace IOWebApplicationApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // За добавяне на контексти, използвайте extension метода!!!
            services.AddAppDbContext(Configuration);

            #region Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
                 { options.User.RequireUniqueEmail = false;}
               )
               .AddUserStore<ApplicationUserStore>()
               .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, string, ApplicationUserRole, ApplicationRoleClaim>>()
               .AddDefaultTokenProviders();

            // ===== Add Jwt Authentication ========
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); //  => remove default claims
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Configuration["JwtIssuer"],
                        ValidAudience = Configuration["JwtIssuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtKey"])),
                        ClockSkew = TimeSpan.Zero // remove delay of token when expire, 
                    };
                });

            string privateKey = Configuration["JwtMobileKey"];
            ECDsa eCDsa = EDCsaHelper.LoadPrivateKey(EDCsaHelper.FromHexString(privateKey));
            var key = new ECDsaSecurityKey(eCDsa);

            services
              .AddAuthentication(options => {
                  options.DefaultAuthenticateScheme = "MobileBearer"; //JwtBearerDefaults.AuthenticationScheme;
                  options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
              })
              .AddJwtBearer("MobileBearer", cfg =>
               {
                  cfg.RequireHttpsMetadata = false;
                  cfg.SaveToken = true;
                  cfg.IncludeErrorDetails = true;
                  cfg.TokenValidationParameters = new TokenValidationParameters
                  {
                      ValidIssuer = Configuration["JwtMobileIssuer"],
                      ValidAudience = Configuration["JwtMobileIssuer"],
                      IssuerSigningKey = key, // new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtMobileKey"])),
                      ClockSkew = TimeSpan.Zero // remove delay of token when expire
                  };
              });
            services.AddCors();
            #endregion Identity
            // За добавяне на услуги, използвайте extension метода!!!
            services.AddApplicationServices();

            services.AddMvc();
            services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
            });

            services.AddSwaggerDocument(conf => 
            {
                conf.PostProcess = document =>
                {
                    document.Info.Title = Configuration.GetValue<string>("SwaggerUI:Title");
                    document.Info.Description = Configuration.GetValue<string>("SwaggerUI:Description");
                    document.Info.Version = Configuration.GetValue<string>("SwaggerUI:Version");
                    document.Schemes = new List<SwaggerSchema>() { SwaggerSchema.Http, SwaggerSchema.Https };
                    document.SecurityDefinitions.Add("apikey", new SwaggerSecurityScheme
                    {
                        Type = SwaggerSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        In = SwaggerSecurityApiKeyLocation.Header
                    });
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.Use((authContext, next) =>
                {
                    authContext.Request.Scheme = "https";
                    return next();
                });
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseSwagger();
            app.UseSwaggerUi3();

            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
