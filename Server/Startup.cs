using AutoMapper;
using EzAspDotNet.Exception;
using EzAspDotNet.Services;
using EzAspDotNet.StartUp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Server.Services;
using System;
using System.Text;

namespace Server
{

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo.Console()
                .CreateLogger();

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            EzAspDotNet.Models.MapperUtil.Initialize(
                new MapperConfiguration(cfg =>
                {
                    cfg.AllowNullDestinationValues = true;

                    cfg.CreateMap<EzAspDotNet.Notification.Models.Notification, Protocols.Common.Notification>(MemberList.None);
                    cfg.CreateMap<Protocols.Common.Notification, EzAspDotNet.Notification.Models.Notification>(MemberList.None);

                    cfg.CreateMap<WebCrawler.Models.Source, Protocols.Common.Source>(MemberList.None);
                    cfg.CreateMap<Protocols.Common.Source, WebCrawler.Models.Source>(MemberList.None);

                    cfg.CreateMap<WebCrawler.Models.CrawlingData, Protocols.Common.CrawlingData>(MemberList.None);
                    cfg.CreateMap<Protocols.Common.CrawlingData, WebCrawler.Models.CrawlingData>(MemberList.None);
                })
            );

            services.CommonConfigureServices();

            services.AddHttpClient();

            services.AddControllers().AddNewtonsoftJson();

            services.AddCors(options => options.AddPolicy("AllowSpecificOrigin",
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });

            services.ConfigureSwaggerGen(options =>
            {
                options.CustomSchemaIds(x => x.FullName);
            });

            services.AddSingleton<IHostedService, CrawlingLoopingService>();
            services.AddSingleton<CrawlingService>();
            services.AddSingleton<SourceService>();
            services.AddSingleton<NotificationService>();
            services.AddSingleton<IHostedService, WebHookLoopingService>();
            services.AddSingleton<WebHookService>();

            Log.Logger.Information($"Local TimeZone:{TimeZoneInfo.Local}");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseHttpsRedirection();
            }

            app.UseCors("AllowSpecificOrigin");

            app.ConfigureExceptionHandler();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger(c =>
            {
                c.SerializeAsV2 = true;
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "My API V1");
            });
        }
    }
}
