using IntegrationService.Epep.Contracts;
using IO.SignTools.Contracts;
using IO.SignTools.Services;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Services;
using IOWebApplication.Core.Services.Tasks;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Services;
using IOWebApplicationService.Infrastructure.Contracts;
using IOWebApplicationService.Infrastructure.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace IntegrationService.Epep.Services
{
    public static class IOServiceAppServiceCollectionExtension
    {
        /// <summary>
        /// Регистрира услугите на приложението в IoC контейнера
        /// </summary>
        /// <param name="services">Регистрирани услуги</param>
        /// <param name="Configuration">Настройки на приложението</param>
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // services.AddLogging();
            services.AddHostedService<EpepTimedHostedService>();

            services.AddScoped<IConsoleTaskExecuteMessageService, ConsoleTaskExecuteMessageService>();
            services.AddScoped<IConsoleTaskRecieverService, ConsoleTaskRecieverService>();
            services.AddScoped<IIOSignToolsService, IOSignToolsService>();
            services.AddScoped<ICdnService, CdnService>();
            services.AddScoped<IEpepConnectionService, EpepConnectionService>();
            services.AddScoped<IEpepService, EpepService>();
            
            services.Configure<FormOptions>(options =>
                  {
                      options.ValueLengthLimit = int.MaxValue;
                      options.MultipartBodyLengthLimit = long.MaxValue; // <-- !!! long.MaxValue
                      options.MultipartBoundaryLengthLimit = int.MaxValue;
                      options.MultipartHeadersCountLimit = int.MaxValue;
                      options.MultipartHeadersLengthLimit = int.MaxValue;
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
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"), m => m.MigrationsAssembly("IOWebApplication.Infrastructure")));

            services.AddScoped(typeof(IRepository), typeof(Repository));

            services.AddSingleton<IMongoClient>(s => new MongoClient(Configuration.GetConnectionString("MongoDbConnection")));
        }
    }
}