using System.Collections.Generic;
using AssetFox.Core.Common;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Hubs.Services;
using AssetFox.Core.Reporting;
using AssetFox.Core.Reporting.Interfaces;
using AssetFox.Core.Reporting.Logging;
using AssetFoxCore.Security;
using AssetFoxCore.Services.Aggregation;
using AssetFoxCore.StartupExtension;
using AssetFoxCore.GraphQL;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AssetFoxCore.Services;
using AssetFox.Core.Reporting.Concrete.GeneralSummary;
using AssetFox.Core.UniversalReporting.Components;
using AssetFox.Core.UniversalReporting.Charts;
using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Export;
using AssetFox.Core.UniversalReporting.Pipeline;
using AssetFox.Core.UniversalReporting.Query;
using AssetFox.Core.UniversalReporting.Transforms;
using AssetFox.Core.UniversalReporting.Viewer;
using AssetFoxCore.Interfaces;
using AssetFoxCore.Services.Reporting;
using static AssetFoxCore.Security.SecurityConstants;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace AssetFoxCore
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
            //services.Configure<ForwardedHeadersOptions>(options =>
            //{
            //    options.ForwardedHeaders =
            //        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            //});
            var urlsKeyValue = Configuration.GetSection("AllowedOrigins:urls").GetChildren();
            var urls = new List<string>();

            foreach (var item in urlsKeyValue)
            {
                urls.Add(item.Value);
            }

            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .WithOrigins(urls.ToArray());
            }));

            services.AddSecurityConfig(Configuration);
            services.AddSingleton<IClaimsTransformation, ClaimsTransformation>();
            var cache = new AggregatedSelectValuesResultDtoCache(2000000);
            services.AddSingleton<IAggregatedSelectValuesResultDtoCache>(cache);

            services.AddSingleton(Configuration);
            services.AddControllers().AddNewtonsoftJson();

            services.AddSimulationData();
            services.AddPagingData();
            services.AddDefaultData();

            services.AddSingleton<ILog, LogNLog>();

            services.AddSignalR();
            services.AddScoped<IHubService, HubService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, SecurityAuthorizationMiddlewareResultHandler>();
            var connectionString = Configuration.GetConnectionString("BridgeCareConnex");

            services.AddDbContext<IAMContext>(options => options.UseSqlServer(
                connectionString,
                sqlServerOptions => sqlServerOptions.CommandTimeout(1800))
                );

            services.AddGraphQLServer()
                .AddQueryType<QueryObjectType>()
                .AddFiltering()
                .AddSorting();
                //.AddAuthorization();

            SetupReporting(services);

            services.AddScoped<IReportGenerator, DictionaryBasedReportGenerator>();
            services.AddScoped<IAggregationService, AggregationService>();
            services.AddScoped<ICompleteSimulationCloningService, CompleteSimulationCloningService>();
            services.AddScoped<CriteriaSqlEmitter>();
            services.AddScoped<ChartTemplateValidator>();
            services.AddScoped<ReportCalculatedFieldSqlCompiler>();
            services.AddScoped<IViewerDataService, ViewerDataService>();
            services.AddScoped<IComponentDataSource, AssetDetailDataSource>();
            services.AddScoped<IComponentDataSource, TreatmentTimelineDataSource>();
            services.AddScoped<IComponentDataSource, ConditionTrackingDataSource>();
            services.AddScoped<IComponentDataSource, FundedTreatmentListDataSource>();
            services.AddScoped<IComponentDataSource, WorkSummaryDataSource>();
            services.AddScoped<IComponentDataSource, BudgetSummaryDataSource>();
            services.AddScoped<IComponentDataSource, TextBlockDataSource>();
            services.AddScoped<IComponentDataSource, BarChartDataSource>();
            services.AddScoped<IComponentDataSource, PieChartDataSource>();
            services.AddScoped<IComponentDataSource, LineChartDataSource>();
            services.AddScoped<ComponentDataSourceRegistry>();
            services.AddScoped<PagedQueryExecutor>();
            services.AddScoped<StreamingQueryExecutor>();
            services.AddScoped<IRowTransform, PercentOfTotalTransform>();
            services.AddScoped<IRowTransform, TotalsRowTransform>();
            services.AddScoped<TransformPipeline>();
            services.AddScoped<IUniversalQueryEngine, UniversalQueryEngine>();
            services.AddScoped<IUniversalReportExporter, UniversalReportExcelExporter>();
            services.AddScoped<UniversalReportRunner>();

            services.AddSingleton<IAnalysisEventLoggingService, AnalysisEventLoggingService>();
        }

        private void SetupReporting(IServiceCollection services)
        {
            var reportFactoryList = new List<IReportFactory>();
            //reportFactoryList.Add(new HelloWorldReportFactory());
            reportFactoryList.Add(new BAMSInventoryReportFactory());
            reportFactoryList.Add(new BAMSSummaryReportFactory());
            reportFactoryList.Add(new ScenarioOutputReportFactory());
            reportFactoryList.Add(new PAMSSummaryReportFactory());
            reportFactoryList.Add(new BAMSAuditReportFactory());
            reportFactoryList.Add(new PAMSAuditReportFactory());
            reportFactoryList.Add(new BAMSPBExportReportFactory());
            reportFactoryList.Add(new PAMSPBExportReportFactory());
            reportFactoryList.Add(new PAMSInventorySectionsReportFactory());
            reportFactoryList.Add(new PAMSInventorySegmentsReportFactory());
            reportFactoryList.Add(new NetworkExportReportFactory());
            reportFactoryList.Add(new RawDataNetworkExportReportFactory());
            reportFactoryList.Add(new PAMSDistressProgressionReportFactory());
            reportFactoryList.Add(new FlexibleAuditReportFactory());
            reportFactoryList.Add(new GeneralSummaryReportFactory());
            reportFactoryList.Add(new UserDefinedReportFactory());
            reportFactoryList.Add(new UniversalReportFactory());
            services.AddSingleton<IReportLookupLibrary>(service => new ReportLookupLibrary(reportFactoryList));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILog logger)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseForwardedHeaders();
            }

            app.ConfigureExceptionHandler(logger);
            //app.UseForwardedHeaders();

            app.UseCors("CorsPolicy");

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                var mapControllers = endpoints.MapControllers();
                var securityType = SecurityConfigurationReader.GetSecurityType(Configuration);
                if (securityType == SecurityTypes.LocalDebug)
                    mapControllers.AllowAnonymous();
                endpoints.MapHub<BridgeCareHub>("/bridgecarehub");
                endpoints.MapGraphQL();
            });
        }

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<IAMContext>();
            context.Database.Migrate();
        }
    }
}
