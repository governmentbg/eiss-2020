using Integration.Eispp;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Http;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Services;
using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IOWebApplicationService.Infrastructure.Services;
using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Services;
using MongoDB.Driver;
using IO.SignTools.Contracts;
using IO.SignTools.Services;
using IEisppService = IOWebApplication.Core.Contracts.IEisppService;
using IOWebApplication.Infrastructure.Data.Models.UserContext;
using IntegrationService.Test.Mockups;
using IOWebApplication.Infrastructure.Models.ViewModels.Eispp;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Http;

namespace IntegrationService.Test
{
    class Program
    {
        private static IConfiguration _configuration;

        static void Main(string[] args)
        {
            var container = Initialize();
            // TestExperts(container);
             TestEisppIntegration(container);
            // TestEisppImport(container);
           // SaveTCard(container).Wait();
            // TestStatistics(container);
            // Да се внимава само продуктион TestIspnIntegration(container);
            //TestPrintService(container);

        }

        static void TestExperts(IServiceProvider container)
        {
            var service = container.GetService<IExpertsSearchService>();
            var experts = service.SearchExperts().Result;

            foreach (var item in experts)
            {
                Console.WriteLine("Вещо лице: {0}", item.FullName);
                Console.Write("Специалности: ");

                foreach (var competence in item.Competences)
                {
                    Console.WriteLine("{0} {1}", competence.Code, competence.Name);
                }

                Console.Write("Райони: ");

                foreach (var region in item.CourtRegions)
                {
                    Console.WriteLine(region);
                }

                Console.WriteLine();
            }
        }

        static void TestEisppIntegration(IServiceProvider container)
        {
            var service = container.GetService<IEisppCommunicationService>();
            service.PushMQWithFetch(100).GetAwaiter().GetResult();
        }
        static void TestIspnIntegration(IServiceProvider container)
        {
            var service = container.GetService<IISPNCaseService>();
            service.PushMQWithFetch(100).GetAwaiter().GetResult();
        }
        static void TestStatistics(IServiceProvider container)
        {
            var service = container.GetService<IStatisticsService>();
            
            service.StatisticsTest();
        }

        static async Task SaveTCard(IServiceProvider container)
        {
            var cdnService = container.GetService<ICdnService>();
            var eisppNumber = "АММ22000021ВЕШ";
            var xml = File.ReadAllText(@"d:\Documents\"+eisppNumber);
            CdnUploadRequest xmlRequest = new CdnUploadRequest()
            {
                ContentType = System.Net.Mime.MediaTypeNames.Text.Html,
                FileContentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml)),
                FileName = eisppNumber,
                SourceId = eisppNumber,
                SourceType = SourceTypeSelectVM.Integration_EISPP_CardTHN,
                Title = $"КАРТА ЗА СЪСТОЯНИЕ НА НП: {eisppNumber} към { DateTime.Today.ToString("dd.MM.yyyy") }"
            };
            await cdnService.MongoCdn_AppendUpdate(xmlRequest).ConfigureAwait(false);
        }
        static void TestEisppImport(IServiceProvider container)
        {
            var service = container.GetService<IEisppImportService>();
            // var content = File.ReadAllBytes(@"D:\Documents\Eispp20200930\EISPP_Standart_std8_ekatte_31032020.xlsx");
            //var ektteItems = service.GetEktteFromExcel(content, 5);
            // var content = File.ReadAllBytes(@"D:\Documents\Eispp20201222\EISPP_Standart_std3_nmk_nmkhka_18.12.2020.xlsx");
            //var content = File.ReadAllBytes(@"D:\Documents\Eispp20201222\EISPP_Standart_std4_nmk_logical_18.12.2020.xlsx");
            //var nomItems = service.GetNomenclatureFromExcel(content, 5);            
            // var newItems = service.GetNewNomenclature(nomItems);
            //var content = File.ReadAllBytes(@"D:\Documents\EISPP20210125\EISPP_Standart_std6_pnekcq_25012021.xlsx");
            //var nomItems = service.GetCrimeQualificationFromExcel(content, 7);
            //var newItems = service.GetNewCrimeQualificationElement(nomItems);
            //newItems = newItems.OrderBy(x => x.SystemCode).ToList();
            //  service.SavewCrimeQualificationElement(newItems);
            //var nomItems = service.GetStructureFromExcel(content, 5);
            //var content = File.ReadAllBytes(@"D:\Documents\Eispp20201222\EISPP_Standart_std7_strvid_18.12.2020.xlsx");
            //var nomItems = service.GetStructureFromExcel(content, 5);
            //nomItems = service.GetClosedStructure(nomItems);
            //service.SaveClosedStructure(nomItems);
            //service.ImportEktteRajon(ektteItems);
            var filter = new EisppReportFilterVM()
            {
                DateFrom = new DateTime(2021, 2, 5, 10, 0, 0, 0, 0),
                DateTo = new DateTime(2021, 2, 12, 10, 0, 0, 0, 0),
            };
            var xlsBytes = service.MakeFridayReport(filter);
            File.WriteAllBytes(@"d:\1\1.xlsx", xlsBytes);
        }
        static void TestPrintService(IServiceProvider container)
        {
            var service = container.GetService<IPrintDocumentService>();
            service.fillList_UpperCourt(106, null);
        }
        static IServiceProvider Initialize()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            return new ServiceCollection()
                .AddLogging()
                .AddSingleton<IConfiguration>(_configuration)
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"), 
                        m => m.MigrationsAssembly("IOWebApplication.Infrastructure")))
                .AddScoped(typeof(IRepository), typeof(Repository))
                .AddScoped<IIOSignToolsService, IOSignToolsService>()
                .AddScoped<ICdnService, CdnService>()
                .AddScoped<IEisppConnectionService, EisppConnectionService>()
                .AddScoped<IEisppCommunicationService, EisppCommunicationService>()
                .AddScoped<IExpertsSearchService, ExpertsSearchService>()
                .AddScoped<IHttpRequester, HttpRequester>()
                .AddScoped<ICdnService, CdnService>()
                .AddScoped<IEisppRulesService, EisppRulesService>()
                .AddScoped<IStatisticsService, StatisticsService>()
                .AddScoped<IStatisticsReportService, StatisticsReportService>()
                .AddScoped<IISPNCaseService, ISPNCaseService>()
                .AddScoped<IEisppImportService, EisppImportService>()
                .AddScoped<IEisppService, EisppService>()
                .AddScoped<IUserContext, UserContextMockup>()
                .AddScoped<IMQEpepService, MQEpepService>()
                .AddScoped<ICounterService, CounterService>()
                .AddScoped<INomenclatureService, NomenclatureService>()
                .AddScoped<IPrintDocumentService, PrintDocumentService>()
                .AddScoped<ICourtLawUnitService, CourtLawUnitService>()
                .AddScoped<ICaseLawUnitService, CaseLawUnitService>()
                .AddScoped<ICommonService, CommonService>()
                .AddScoped<ICaseMigrationService, CaseMigrationService>()
                .AddScoped<ICaseNotificationService, CaseNotificationService>()
                .AddScoped<ICaseFastProcessService, CaseFastProcessService>()
                .AddScoped<ICourtLoadPeriodService, CourtLoadPeriodService>()
                .AddScoped<IRelationManyToManyDateService, RelationManyToManyDateService>()
                .AddScoped<IDeliveryAreaService, DeliveryAreaService>()
                .AddScoped<IDeliveryItemService, DeliveryItemService>()
                .AddScoped<IWorkingDaysService, WorkingDaysService>()
                .AddScoped<ICasePersonService, CasePersonService>()
                .AddScoped<ICasePersonLinkService, CasePersonLinkService>()
                .AddScoped<IWorkNotificationService, WorkNotificationService>()
                .AddScoped<IPriceService, PriceService>()
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
                .AddAutoMapper(typeof(AutoMapperProfile).Assembly)
                .AddScoped<IUrlHelper>(x =>
                {
                    return null;   
                    //var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
                    //var factory = x.GetRequiredService<IUrlHelperFactory>();
                    //return factory.GetUrlHelper(actionContext);
                })
                .AddSingleton<IMongoClient>(s => new MongoClient(_configuration.GetConnectionString("MongoDbConnection")))
                .AddSingleton<IISPNHttpRequester>(x =>
                {
                    var model = new ISPNHttpRequester();
                    model.requester = new HttpRequester(
                        _configuration.GetValue<string>("ISPN:CertificatePath"),
                        _configuration.GetValue<string>("ISPN:CertificatePassword"),
                        true);
                    return model;
                })
                .BuildServiceProvider();
        }

    }
}
