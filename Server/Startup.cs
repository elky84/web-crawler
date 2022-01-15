using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using EzAspDotNet.Exception;
using EzAspDotNet.Services;
using MongoDbWebUtil.Services;
using Server.Services;
using System.Diagnostics;
using System;

namespace Server
{

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo.Console()
                .CreateLogger();

            Configuration = configuration;

            using (var log = new LoggerConfiguration().WriteTo.Console().CreateLogger())
            {
                log.Information($"Local TimeZone:{TimeZoneInfo.Local}");
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
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

            services.AddTransient<MongoDbService>();

            services.AddSingleton<IHostedService, WebCrawlingLoopingService>();
            services.AddSingleton<WebCrawlingService>();

            services.AddSingleton<IHostedService, FeedCrawlingLoopingService>();
            services.AddSingleton<FeedCrawlingService>();

            services.AddSingleton<SourceService>();

            services.AddSingleton<NotificationService>();

            services.AddSingleton<IHostedService, WebHookLoopingService>();
            services.AddSingleton<WebHookService>();

            services.AddSingleton<RssService>();
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
