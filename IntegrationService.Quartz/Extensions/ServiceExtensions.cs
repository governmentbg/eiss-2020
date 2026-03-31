// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IntegrationService.Quartz.QuartzServices;
using IO.SignTools.Contracts;
using IO.SignTools.Extensions;
using IO.SignTools.Models;
using IO.SignTools.Services;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Services;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.UserContext;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.Integrations.EpepRest;
using IOWebApplication.Infrastructure.Models.Integrations.RNFL;
using IOWebApplication.Infrastructure.Services;
using IOWebApplication.Infrastructure.Services.Mocks;
using IOWebApplicationService.Infrastructure.Contracts;
using IOWebApplicationService.Infrastructure.Data.Common;
using IOWebApplicationService.Infrastructure.Data.DW;
using IOWebApplicationService.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace IntegrationService.Quartz
{
    public static class ServiceExtensions
    {

        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUrlHelper, MockUrlHelper>();
            services.AddScoped<IHttpContextAccessor, MockHttpContextAccessor>();

            services.AddScoped<ICaisConnectionFactory, CaisConnectionFactory>();
            services.AddScoped<ICaisService, CaisService>();
            services.AddScoped<ICaseClassificationService, CaseClassificationService>();
            services.AddScoped<ICaseDeadlineService, CaseDeadlineService>();
            services.AddScoped<ICaseEvidenceService, CaseEvidenceService>();
            services.AddScoped<ICaseFastProcessService, CaseFastProcessService>();
            services.AddScoped<ICaseGroupService, CaseGroupService>();
            services.AddScoped<ICaseLawUnitService, CaseLawUnitService>();
            services.AddScoped<ICaseLifecycleService, CaseLifecycleService>();
            services.AddScoped<ICaseLoadCorrectionService, CaseLoadCorrectionService>();
            services.AddScoped<ICaseLoadIndexService, CaseLoadIndexService>();
            services.AddScoped<ICaseMigrationService, CaseMigrationService>();
            services.AddScoped<ICaseMoneyService, CaseMoneyService>();
            services.AddScoped<ICaseMovementService, CaseMovementService>();
            services.AddScoped<ICaseNotificationService, CaseNotificationService>();
            services.AddScoped<ICasePersonLinkService, CasePersonLinkService>();
            services.AddScoped<ICasePersonService, CasePersonService>();
            services.AddScoped<ICaseSelectionProtokolService, CaseSelectionProtokolService>();
            services.AddScoped<ICaseService, CaseService>();
            services.AddScoped<ICaseSessionActComplainService, CaseSessionActComplainService>();
            services.AddScoped<ICaseSessionActCoordinationService, CaseSessionActCoordinationService>();
            services.AddScoped<ICaseSessionActService, CaseSessionActService>();
            services.AddScoped<ICaseSessionDocService, CaseSessionDocService>();
            services.AddScoped<ICaseSessionFastDocumentService, CaseSessionFastDocumentService>();
            services.AddScoped<ICaseSessionMeetingService, CaseSessionMeetingService>();
            services.AddScoped<ICaseSessionService, CaseSessionService>();
            services.AddScoped<ICdnService, CdnService>();
            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<ICounterService, CounterService>();
            services.AddScoped<ICourtLawUnitService, CourtLawUnitService>();
            services.AddScoped<ICourtLoadPeriodService, CourtLoadPeriodService>();
            services.AddScoped<ICsrdService, CsrdService>();
            services.AddScoped<ICubipsaConnectionService, CubipsaConnectionService>();
            services.AddScoped<ICubipsaService, CubipsaService>();
            services.AddScoped<IDBUserContext, DBUserContext>();
            services.AddScoped<IDWCaseSelectionProtocolService, DWCaseSelectionProtocolService>();
            services.AddScoped<IDWCaseService, DWCaseService>();
            services.AddScoped<IDWDocumentService, DWDocumentService>();
            services.AddScoped<IDWErrorLogService, DWErrorLogService>();
            services.AddScoped<IDWService, DWService>();
            services.AddScoped<IDWSessionActService, DWSessionActService>();
            services.AddScoped<IDWSessionService, DWSessionService>();
            services.AddScoped<IDeliveryAreaAddressService, DeliveryAreaAddressService>();
            services.AddScoped<IDeliveryItemService, DeliveryItemService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IEesppService, EesppService>();
            services.AddScoped<IEisppCommunicationService, EisppCommunicationService>();
            services.AddScoped<IEisppConnectionService, EisppConnectionService>();
            services.AddScoped<IEisppRulesService, EisppRulesService>();
            services.AddScoped<IElasticIndexService, ElasticIndexService>();
            services.AddScoped<IElasticService, ElasticService>();
            services.AddScoped<IEpepCaseMigrationService, EpepCaseMigrationService>();
            services.AddScoped<IEpepConnectionService, EpepConnectionService>();
            services.AddScoped<IEpepDocumentService, EpepDocumentService>();
            services.AddScoped<IEpepRestClient, EpepRestClient>();
            services.AddScoped<IEproService, EproService>();
            services.AddScoped<IIOSignToolsService, IOSignToolsService>();
            services.AddScoped<IISPNCaseService, ISPNCaseService>();
            services.AddScoped<IISPNCaseService, ISPNCaseService>();
            services.AddScoped<IMQEpepService, MQEpepService>();
            services.AddLazybleService<IMoneyService, MoneyService>();
            services.AddScoped<INomenclatureService, NomenclatureService>();
            services.AddScoped<IPriceService, PriceService>();
            services.AddScoped<IProxyEissService, ProxyEissService>();
            services.AddScoped<IRelationManyToManyDateService, RelationManyToManyDateService>();
            services.AddScoped<ISismaService, SismaService>();
            services.AddScoped<IStatisticsReportService, StatisticsReportService>();
            services.AddScoped<IStatisticsService, StatisticsService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IUserContext, MockUserContext>();
            services.AddScoped<IVksNotificationService, VksNotificationService>();
            services.AddScoped<IWorkNotificationService, WorkNotificationService>();
            services.AddScoped<IWorkTaskService, WorkTaskService>();
            services.AddScoped<IWorkingDaysService, WorkingDaysService>();
            services.AddScoped<IFastProcessSelectionCourtService, FastProcessSelectionCourtService>();

            services.AddLazybleService<IDocumentRequestService, DocumentRequestService>();
            services.AddLazybleService<ICasePersonLinkService, CasePersonLinkService>();

            services.AddScoped<IPdfCreatorService, PdfCreatorService>();
            services.AddScoped<IBankFileService, BankFileService>();
            services.AddScoped<ICourtStampCertificateService, CourtStampCertificateService>();
            services.AddScoped<IDeliveryItemService, DeliveryItemService>();
            services.AddScoped<IDeliveryItemOperService, DeliveryItemOperService>();
            services.AddScoped<IEissProcessService, EissProcessService>();
            services.Configure<RnflRestConfigurationVM>(configuration.GetSection("RNFL"));
            services.AddScoped<IRnflRestClient, RnflRestClient>();
            services.AddScoped<IRnflRestService, RnflRestService>();
            services.AddScoped<IMqRecoverService, MqRecoverService>();

            services.Configure<CdnConfigVM>(configuration);


            services.AddSingleton<EproCryptoHelper, EproCryptoHelper>();


            var newEpep = configuration.GetValue<bool>("EPEP:RestActive", false);
            if (newEpep)
            {
                services.AddScoped<IEpepService, EpepRestService>();
            }
            else
            {
                services.AddScoped<IEpepService, EpepService>();
            }

            services.Configure<EpepRestConfigurationVM>(configuration.GetSection("EPEP"));

            //TimestampClientOptions tsOptions = new TimestampClientOptions();

            //if (!string.IsNullOrEmpty(configuration.GetValue<string>("Authentication:StampIT:Timestamp:Token")))
            //{
            //    tsOptions = new TimestampClientOptions();
            //    //{
            //    //    Token = configuration.GetValue<string>("Authentication:StampIT:Timestamp:Token"),
            //    //    TimestampEndpoint = configuration.GetValue<string>("Authentication:StampIT:Timestamp:TimestampEndpoint"),
            //    //    ValidateEndpoint = configuration.GetValue<string>("Authentication:StampIT:Timestamp:ValidateEndpoint")
            //    //};
            //}

            //VerificationServiceOptions vsOptions = new VerificationServiceOptions();
            ////{
            ////    Token = configuration.GetValue<string>("Authentication:StampIT:VerificationService:Token"),
            ////    VerificationServiceEndpoint = configuration.GetValue<string>("Authentication:StampIT:VerificationService:VerificationServiceEndpoint"),
            ////    ClientId = configuration.GetValue<string>("Authentication:StampIT:VerificationService:ClientId")
            ////};
            services.AddIOSignTools(options =>
            {
                options.TempDir = configuration.GetValue<string>("TempPdfDir");
                options.HashAlgorithm = System.Security.Cryptography.HashAlgorithmName.SHA256.Name;
                options.TimestampOptions = new TimestampClientOptions();
                options.VerificationServiceOptions = new VerificationServiceOptions();
            });

            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = long.MaxValue; // <-- !!! long.MaxValue
                options.MultipartBoundaryLengthLimit = int.MaxValue;
                options.MultipartHeadersCountLimit = int.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            services.AddMvcCore().AddRazorRuntimeCompilation();
            services.AddRazorTemplating();
            services.AddIOHtmlToPdf(options =>
            {
                options.PdfCreatorUrl = configuration.GetValue<string>("PdfCreator:Url");
                options.IgnoreSSLErrors = configuration.GetValue<bool>("PdfCreator:IgnoreSSLErrors", true);
                options.PdfOptions = new IO.HtmlToPdf.Models.PDFOptions() { Timeout = 0 };
                options.RequestTimeout = TimeSpan.FromMinutes(15);
            });
        }

        /// <summary>
        /// Регистрира контекстите на приложението в IoC контейнера
        /// </summary>
        /// <param name="services">Регистрирани услуги</param>
        /// <param name="Configuration">Настройки на приложението</param>
        public static void AddAppDbContext(this IServiceCollection services, IConfiguration Configuration)
        {
            string connString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connString, m => m.MigrationsAssembly("IOWebApplication.Infrastructure")));

            string readonlyDbConnString = Configuration.GetConnectionString("ReadonlyConnection") ?? connString;
            services.AddDbContext<ReadonlyDbContext>(options =>
               options.UseNpgsql(readonlyDbConnString, m => m.MigrationsAssembly(null))
           );

            services.AddDbContext<DWDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DWConnection"), m =>
                {
                    m.MigrationsAssembly("IOWebApplicationService.Infrastructure");
                    m.CommandTimeout(Configuration.GetValue<int>("DW:Timeout", 60));
                }));


            services.AddScoped(typeof(IRepository), typeof(Repository));
            services.AddScoped(typeof(IDWRepository), typeof(DWRepository));
            services.AddScoped(typeof(IReadonlyRepository), typeof(ReadonlyRepository));

            services.AddSingleton<IMongoClient>(s => new MongoClient(Configuration.GetConnectionString("MongoDbConnection")));

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        /// <summary>
        /// Регистрира HttpClient-ите и сертификатите към тях
        /// </summary>
        /// <param name="services">Регистрирани услуги</param>
        /// <param name="Configuration">Настройки на приложението</param>
        public static void ConfigureHttpClients(this IServiceCollection services, IConfiguration config)
        {
            var newEpep = config.GetValue<bool>("EPEP:RestActive", false);
            if (newEpep)
            {
                //EPEP Rest Client
                services.AddHttpClient(EpepRestClient.FactoryName, client =>
                {
                    client.Timeout = new TimeSpan(0, 2, 30);
                    //client.DefaultRequestHeaders.ExpectContinue = false;
                });
            }

            //RNFL Rest Client
            services.AddHttpClient(RnflRestClient.FactoryName, client =>
            {
                client.Timeout = new TimeSpan(0, 2, 0);
            });

            //CSRD Client
            services.AddHttpClient("csrdHttpClient", client =>
            {
                var endPoint = config.GetValue<string>("CSRD:Endpoint");
                var method = config.GetValue<string>("CSRD:Method");
                client.BaseAddress = new Uri(new Uri(endPoint), method);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                // return Program.HttpClientHandler_CSRD;
                var certificatePath = config.GetValue<string>("CSRD:CertificatePath");
                var certificatePassword = config.GetValue<string>("CSRD:CertificatePassword");
                HttpClientHandler result = new HttpClientHandler();
                if (!string.IsNullOrEmpty(certificatePath))
                {
                    var _cert = new X509Certificate2(certificatePath, certificatePassword);
                    result.ClientCertificates.Add(_cert);
                    result.ClientCertificateOptions = ClientCertificateOption.Manual;
                    result.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                }
                return result;
            });

            //ИСПН Client
            services.AddHttpClient("ispnHttpClient", client =>
            {
                client.BaseAddress = new Uri(config.GetValue<string>("ISPN:URI"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                //return Program.HttpClientHandler_ISPN;
                var certificatePath = config.GetValue<string>("ISPN:CertificatePath");
                var certificatePassword = config.GetValue<string>("ISPN:CertificatePassword");
                HttpClientHandler result = new HttpClientHandler();
                if (!string.IsNullOrEmpty(certificatePath))
                {
                    var _cert = new X509Certificate2(certificatePath, certificatePassword);
                    result.ClientCertificates.Add(_cert);
                    result.ClientCertificateOptions = ClientCertificateOption.Manual;
                    result.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                }
                return result;
            });

            //EPRO Client
            services.AddHttpClient("eproHttpClient", client =>
            {
                client.BaseAddress = new Uri(config.GetValue<string>("EPRO:URI"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = new TimeSpan(0, 1, 30);
            });

            //EESPP Client
            services.AddHttpClient("eesppHttpClient", client =>
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = new TimeSpan(0, 1, 30);
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                HttpClientHandler result = new HttpClientHandler();
                result.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                result.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                return result;
            });


            //SISMA Client
            services.AddHttpClient("sismaHttpClient", client =>
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = new TimeSpan(0, 1, 30);
            });

            //ЦАЙС Client
            services.AddHttpClient("caisHttpClient", client =>
            {
                client.BaseAddress = new Uri(config.GetValue<string>("CAIS:URI"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
                var soapAction = config.GetValue<string>("CAIS:SoapAction", "http://cs.mjs.bg/EISSServicesModel-v1.0/IEISSIntegrationService/SendBulletinsData");
                client.DefaultRequestHeaders.Add("SOAPAction", soapAction);
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                var certificatePath = config.GetValue<string>("CAIS:CertificatePath");
                var certificatePassword = config.GetValue<string>("CAIS:CertificatePassword");
                HttpClientHandler result = new HttpClientHandler();
                if (!string.IsNullOrEmpty(certificatePath))
                {
                    var _cert = new X509Certificate2(certificatePath, certificatePassword);
                    result.ClientCertificates.Add(_cert);
                    result.ClientCertificateOptions = ClientCertificateOption.Manual;
                    result.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                }
                return result;
            });
        }
    }
}
