using AuthLibrary.Data;
using DateTimeService.Configuration;
using DateTimeService.DatabaseManagementNewServices.Interfaces;
using DateTimeService.Logging;
using DateTimeService.Middlewares;
using FluentValidation;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace DateTimeService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConfiguration _configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCors();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddControllers(options => options.Filters.Add(typeof(Filters.ConnectionResetExceptionFilter)));

            // For Entity Framework  
            services.AddDbContext<DateTimeServiceContext>(options => options.UseSqlServer(_configuration.GetConnectionString("DateTimeServiceContextConnection")));

            services
                .AddAuthorizationAuthentication(_configuration)
                .AddFilters()
                .AddHttpClients()
                .AddDatabaseManagement()
                .AddDataServices()
                .AddHttpContextAccessor()
                .AddRedis(_configuration)
                .AddTransient<GlobalExceptionHandlingMiddleware>()
                .AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Transient);

            services.AddSwaggerGen();

            services.AddHangfire(x => x.UseMemoryStorage());
            
            services.AddHangfireServer(options =>
            {
                options.SchedulePollingInterval = TimeSpan.FromMilliseconds(5000);
            });
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                           ForwardedHeaders.XForwardedProto
            });

            app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowedToAllowWildcardSubdomains().WithOrigins(_configuration.GetSection("CorsOrigins").Get<List<string>>().ToArray()));

            app.UseStaticFiles();
            app.UseSwagger();

            if (env.IsDevelopment())
            {
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger.json", "DateTimeService v1"));
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/NetMS/swagger.json", "DateTimeService v1"));
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            if (_configuration.GetValue<bool>("onlyHttps"))
            {
                app.UseHttpsRedirection();
            }
            
            app.UseRouting();

            app.Use(async (context, next) =>
            {
                // создаем новый экземпляр ConcurrentDictionary для каждого запроса
                context.Items[typeof(ConcurrentDictionary<object, object>)] = new ConcurrentDictionary<object, object>();

                await next();
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

            loggerFactory.AddHttp(_configuration["loggerHost"], _configuration.GetValue<int>("loggerPortUdp"), _configuration.GetValue<int>("loggerPortHttp"), _configuration["loggerEnv"]);
            loggerFactory.CreateLogger("HttpLogger");

            app.UseHangfireDashboard();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapHangfireDashboardWithAuthorizationPolicy("Hangfire");
            });

            try
            {
                var reloadDatabasesService = serviceProvider.GetRequiredService<IReloadDatabasesService>();
                RecurringJob.AddOrUpdate("ReloadDatabasesFromFiles", () => reloadDatabasesService.ReloadAsync(CancellationToken.None), "*/10 * * * * *"); //every 10 seconds

                var checkStatusService = serviceProvider.GetRequiredService<IDatabaseAvailabilityControl>();
                RecurringJob.AddOrUpdate("CheckAndUpdateDatabasesStatus", () => checkStatusService.CheckAndUpdateDatabasesStatus(CancellationToken.None), Cron.Minutely());
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Startup>>();
                logger.LogError(ex, "An error occurred while starting recurring job.");
            }
        }
    }
}
