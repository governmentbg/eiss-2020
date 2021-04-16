using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Services;
using IOWebApplication.Core.Services.Tasks;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Services;
using IOWebApplicationService.Infrastructure.Contracts;
using IOWebApplicationService.Infrastructure.Data.Common;
using IOWebApplicationService.Infrastructure.Data.DW;
using IOWebApplicationService.Infrastructure.Services;
using IOWebApplicationService.Services.MockUp;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IOWebApplicationService.Services
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
            services.AddLogging();

            services.AddHostedService<DWTimedHostedService>();


            services.AddScoped<IConsoleTaskExecuteMessageService, ConsoleTaskExecuteMessageService>();
            services.AddScoped<IConsoleTaskRecieverService, ConsoleTaskRecieverService>();
            services.AddScoped<IEpepConnectionService, EpepConnectionService>();
            services.AddScoped<IEpepService, EpepService>();
            services.AddScoped<IDWService, DWService>();
            services.AddScoped<IDWCaseService, DWCaseService>();
            services.AddScoped<ICasePersonLinkService, CasePersonLinkService>();
            services.AddScoped<IUserContext, UserContextMockUp>();
            services.AddScoped<IDWCaseSelectionProtocolService, DWCaseSelectionProtocolService>();


            services.AddScoped<IDWDocumentService, DWDocumentService>();
      services.AddScoped<IDWErrorLogService, DWErrorLogService>();
      services.AddScoped<IDWSessionService, DWSessionService>();
            services.AddScoped<IDWSessionActService, DWSessionActService>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IEMailMessageService, EMailMessageService>();
            services.Configure<FormOptions>(options =>
                        {
                            options.ValueLengthLimit = int.MaxValue;
                            options.MultipartBodyLengthLimit = long.MaxValue;
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
            services.AddDbContext<DWDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DWConnection"), m => m.MigrationsAssembly("IOWebApplicationService.Infrastructure")));

            services.AddScoped(typeof(IRepository), typeof(Repository));
            services.AddScoped(typeof(IDWRepository), typeof(DWRepository));
        }
    }
}