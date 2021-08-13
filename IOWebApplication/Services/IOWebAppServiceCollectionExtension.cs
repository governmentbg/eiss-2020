using IO.LogOperation.Service;
using IO.SignTools.Contracts;
using IO.SignTools.Services;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Services;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.UserContext;
using IOWebApplication.Infrastructure.Http;
using IOWebApplication.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Описва услугите и контекстите на приложението
    /// </summary>
    public static class IOWebAppServiceCollectionExtension
    {
        /// <summary>
        /// Регистрира услугите на приложението в  IoC контейнера
        /// </summary>
        /// <param name="services">Регистрирани услуги</param>
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationClaimsPrincipalFactory>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddScoped<IUrlHelper>(x =>
            {
                var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
                var factory = x.GetRequiredService<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            });
            services.Configure<FormOptions>(options =>
            {
                options.ValueCountLimit = int.MaxValue;
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = long.MaxValue; // <-- !!! long.MaxValue
                options.MultipartBoundaryLengthLimit = int.MaxValue;
                options.MultipartHeadersCountLimit = int.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });
            services.AddScoped<IIOSignToolsService, IOSignToolsService>();
            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<INomenclatureService, NomenclatureService>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<ILogOperationService<ApplicationDbContext>, LogOperationService<ApplicationDbContext>>();
            services.AddScoped<ICdnService, CdnService>();
            services.AddScoped<IBaseCdnService, BaseCdnService>();
            services.AddScoped<IHttpRequester, HttpRequester>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IDocumentTemplateService, DocumentTemplateService>();
            services.AddScoped<ICounterService, CounterService>();
            services.AddScoped<ICourtLawUnitService, CourtLawUnitService>();
            services.AddScoped<ICourtDepartmentService, CourtDepartmentService>();
            services.AddScoped<ICourtOrganizationService, CourtOrganizationService>();
            services.AddScoped<ICourtGroupService, CourtGroupService>();
            services.AddScoped<ICourtGroupCodeService, CourtGroupCodeService>();
            services.AddScoped<IHtmlTemplate, HtmlTemplateService>();
            services.AddScoped<ICaseGroupService, CaseGroupService>();
            services.AddScoped<ICourtGroupLawUnitService, CourtGroupLawUnitService>();
            services.AddScoped<ICourtDutyService, CourtDutyService>();
            services.AddScoped<ICaseService, CaseService>();
            services.AddScoped<IRelationManyToManyDateService, RelationManyToManyDateService>();
            services.AddScoped<ICasePersonService, CasePersonService>();
            services.AddScoped<ICaseSessionService, CaseSessionService>();
            services.AddScoped<ICaseLawUnitService, CaseLawUnitService>();
            services.AddScoped<ICaseLifecycleService, CaseLifecycleService>();
            services.AddScoped<ICaseSessionFastDocumentService, CaseSessionFastDocumentService>();
            services.AddScoped<IWorkTaskService, WorkTaskService>();
            services.AddScoped<ICaseLoadIndexService, CaseLoadIndexService>();
            services.AddScoped<ICaseLawyerHelpService, CaseLawyerHelpService>();
            services.AddScoped<ICasePersonSentenceService, CasePersonSentenceService>();
            services.AddScoped<ICaseLoadCorrectionService, CaseLoadCorrectionService>();
            services.AddScoped<ICaseSessionActComplainService, CaseSessionActComplainService>();
            services.AddScoped<ICaseSessionActService, CaseSessionActService>();
            services.AddScoped<ICasePersonLinkService, CasePersonLinkService>();
            services.AddScoped<ICaseNotificationService, CaseNotificationService>();
            services.AddScoped<ICaseSessionActLawBaseService, CaseSessionActLawBaseService>();
            services.AddScoped<ICaseClassificationService, CaseClassificationService>();
            services.AddScoped<ICaseMoneyService, CaseMoneyService>();
            services.AddScoped<ICaseSessionDocService, CaseSessionDocService>();
            services.AddScoped<ICaseSelectionProtokolService, CaseSelectionProtokolService>();
            services.AddScoped<IPrintDocumentService, PrintDocumentService>();
            services.AddScoped<ICourtLoadPeriodService, CourtLoadPeriodService>();
            services.AddScoped<ICaseEvidenceService, CaseEvidenceService>();
            services.AddScoped<IDeliveryAreaService, DeliveryAreaService>();
            services.AddScoped<IDeliveryAreaAddressService, DeliveryAreaAddressService>();
            services.AddScoped<IDeliveryItemService, DeliveryItemService>();
            services.AddScoped<IDeliveryItemOperService, DeliveryItemOperService>();
            services.AddScoped<IEkMunicipalityService, EkMunicipalityService>();
            services.AddScoped<ICaseSessionActCoordinationService, CaseSessionActCoordinationService>();
            services.AddScoped<ILoadGroupService, LoadGroupService>();
            services.AddScoped<ICaseMovementService, CaseMovementService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IReportViewerService, ReportViewerService>();
            services.AddScoped<ICaseSessionMeetingService, CaseSessionMeetingService>();
            services.AddScoped<IMoneyService, MoneyService>();
            services.AddScoped<ICourtRegionService, CourtRegionService>();
            services.AddScoped<ICaseMigrationService, CaseMigrationService>();
            services.AddScoped<ICaseArchiveService, CaseArchiveService>();
            services.AddScoped<ICalendarService, CalendarService>();
            services.AddScoped<IRegixReportService, RegixReportService>();
            services.AddScoped<ICaseFastProcessService, CaseFastProcessService>();
            services.AddScoped<IEisppService, EisppService>();
            services.AddScoped<IEisppImportService, EisppImportService>();
            services.AddScoped<IEisppRulesService, EisppRulesService>();
            services.AddScoped<IWorkNotificationService, WorkNotificationService>();
            services.AddScoped<ICourtArchiveService, CourtArchiveService>();
            services.AddScoped<IPriceService, PriceService>();
            services.AddScoped<ICaseDeadlineService, CaseDeadlineService>();
            services.AddScoped<IMQEpepService, MQEpepService>();
            services.AddScoped<IDeliveryAccountService, DeliveryAccountService>();
            services.AddScoped<IWorkingDaysService, WorkingDaysService>();
            services.AddScoped<IEpepConnectionService, EpepConnectionService>();
            services.AddScoped<IExcelReportService, ExcelReportService>();
            services.AddScoped<IMigrationDataService, MigrationDataService>();
            //services.AddScoped<IOAuditLogDataProvider, IOAuditLogDataProvider>();
            services.AddScoped<ITempFileHandler, TempFileHandler>();
            services.AddScoped<ICaseLawUnitTaskChangeService, CaseLawUnitTaskChangeService>();
            services.AddScoped<IDocumentResolutionService, DocumentResolutionService>();
            services.AddScoped<INewsService, NewsService>();
            services.AddScoped<ICaseDeactivationService, CaseDeactivationService>();
            services.AddScoped<IDeactivateItemService, DeactivateItemService>();
            services.AddScoped<IStatisticsReportService, StatisticsReportService>();
            services.AddScoped<IVKSSelectionService, VKSSelectionService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IApiDocumentService, ApiDocumentService>();
            services.AddScoped<IDocumentPersonLinkService, DocumentPersonLinkService>();
            services.AddScoped<IDocumentNotificationService, DocumentNotificationService>();
            services.AddScoped<IVksNotificationService, VksNotificationService>();
            services.AddScoped<IElasticService, ElasticService>();
        }

        /// <summary>
        /// Регистрира контекстите на приложението в IoC контейнера
        /// </summary>
        /// <param name="services">Регистрирани услуги</param>
        /// <param name="Configuration">Настройки на приложението</param>
        public static void AddAppDbContext(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"), m => m.MigrationsAssembly("IOWebApplication.Infrastructure"))
            );

            services.AddScoped(typeof(IRepository), typeof(Repository));

            services.AddSingleton<IMongoClient>(s =>
                new MongoClient(Configuration.GetConnectionString("MongoDbConnection"))
            );

            //Настройки на Одит лога
            //Audit.Core.Configuration.Setup()
            //.UsePostgreSql(config => config
            //    .ConnectionString(Configuration.GetConnectionString("DefaultConnection"))
            //    .Schema("audit_log")
            //    .TableName("audit_log")
            //    .IdColumnName("id")
            //    .DataColumn("data", DataType.JSONB)
            //    .LastUpdatedColumnName("updated_date"));            
        }
    }
}
