using Audit.Core;
using Audit.PostgreSql.Configuration;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Services;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.UserContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IOWebApplicationApi.Services;
using IOWebApplicationApi.Contracts;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Описва услугите и контекстите на приложението
    /// </summary>
    public static class IOWebApiServiceCollectionExtension
    {
        /// <summary>
        /// Регистрира услугите на приложението в  IoC контейнера
        /// </summary>
        /// <param name="services">Регистрирани услуги</param>
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(x => {
                var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
                var factory = x.GetRequiredService<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            });
        }

        /// <summary>
        /// Регистрира контекстите на приложението в IoC контейнера
        /// </summary>
        /// <param name="services">Регистрирани услуги</param>
        /// <param name="Configuration">Настройки на приложението</param>
        public static void AddAppDbContext(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped(typeof(IRepository), typeof(Repository));

            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<INomenclatureService, NomenclatureService>();
            services.AddScoped<ICourtLawUnitService, CourtLawUnitService>();
            services.AddScoped<IDeliveryItemService, DeliveryItemService>();
            services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
            services.AddScoped<IRelationManyToManyDateService, RelationManyToManyDateService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IWorkingDaysService, WorkingDaysService>();
            services.AddScoped<IMobileFileService, MobileFileService>();
            // Настройки на Одит лога 
            //Audit.Core.Configuration.Setup()
            //.UsePostgreSql(config => config
            //    .ConnectionString(Configuration.GetConnectionString("DefaultConnection"))
            //    .Schema("audit")
            //    .TableName("audit_log")
            //    .IdColumnName("id")
            //    .DataColumn("data", DataType.JSONB)
            //    .LastUpdatedColumnName("updated_date"));
        }
    }
}

