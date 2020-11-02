// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSwag;

namespace IOWebApplication.Cdn
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        private readonly string CorsPolicy = "cdnCorsPolicy";

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // За добавяне на контексти, използвайте extension метода!!!
            services.AddAppDbContext(Configuration);

            // За добавяне на услуги, използвайте extension метода!!!
            services.AddApplicationServices();

            services.AddMvc();
            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicy,
                builder =>
                {
                    builder.WithOrigins("*");
                });
            });

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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.Use((authContext, next) =>
                {
                    //app.UseHsts();
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

            app.UseAuthentication();

            app.UseMvc();
            app.UseCors(CorsPolicy);
        }
    }
}
