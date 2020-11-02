// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using AutoMapper;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Services;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Http;
using IOWebApplication.Infrastructure.Services;
using IOWebApplication.Test.Mockups;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace IOWebApplication.Test.Extensions
{
    public static class TestServiceCollectionExtension
    {
        public static void AddTestServices(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUrlHelper, UrlHelperMock>();
            services.AddScoped<IRepository, RepositoryMock>();
            services.AddScoped<IUserContext, UserContextMockup>();
            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<INomenclatureService, NomenclatureService>();
            //services.AddScoped<ILogOperationService<ApplicationDbContext>, LogOperationService<ApplicationDbContext>>();
            services.AddScoped<ICdnService, CdnService>();
            services.AddScoped<IHttpRequester, HttpRequester>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IDocumentTemplateService, DocumentTemplateService>();
            services.AddScoped<ICounterService, CounterServiceMock>();
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
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddAutoMapper();
            services.AddLogging();
            services.TryAddSingleton(GetConfiguration());
        }

        private static IConfiguration GetConfiguration()
        {
            //var myConfiguration = new Dictionary<string, string>
            //{
            //    {"Key1", "Value1"},
            //    {"Nested:Key1", "NestedValue1"},
            //    {"Nested:Key2", "NestedValue2"}
            //};

            //return new ConfigurationBuilder()
            //    .AddInMemoryCollection(myConfiguration)
            //    .Build();
            return new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
        }
    }
}