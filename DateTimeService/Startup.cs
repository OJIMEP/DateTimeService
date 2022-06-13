using DateTimeService.Areas.Identity.Data;
using DateTimeService.Data;
using DateTimeService.DatabaseManagementNewServices.Interfaces;
using DateTimeService.DatabaseManagementNewServices.Services;
using DateTimeService.DatabaseManagementUtils;
using DateTimeService.Logging;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace DateTimeService
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

            services.AddCors();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddControllers(options => options.Filters.Add(typeof(Filters.ConnectionResetExceptionFilter)));

            // For Entity Framework  
            services.AddDbContext<DateTimeServiceContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DateTimeServiceContextConnection")));

            // For Identity  
            services.AddDefaultIdentity<DateTimeServiceUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<DateTimeServiceContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // Default Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
            });

            // Adding Authentication  
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "JWT_OR_COOKIE";
                options.DefaultChallengeScheme = "JWT_OR_COOKIE";
                options.DefaultScheme = "JWT_OR_COOKIE";
            })
            // Adding Jwt Bearer  
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = Configuration["JWT:ValidAudience"],
                    ValidIssuer = Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"])),
                    ValidateLifetime = true
                };
            })
            .AddPolicyScheme("JWT_OR_COOKIE", "JWT_OR_COOKIE", options =>
            {
                // runs on each request
                options.ForwardDefaultSelector = context =>
                {
                    // filter by auth type
                    string authorization = context.Request.Headers[HeaderNames.Authorization];
                    if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                        return JwtBearerDefaults.AuthenticationScheme;

                    // otherwise always check for Identity cookie auth
                    return IdentityConstants.ApplicationScheme;
                };
            });
            

            

            services.AddAuthorization(builder =>
            {
                builder.AddPolicy("Hangfire", policy => policy.RequireRole(UserRoles.Admin));
            });

            services.AddScoped<IUserService, UserService>();

            services.AddScoped<ILoadBalancing, LoadBalancing>();

            services.AddHttpClient<IGeoZones, GeoZones>();

            services.AddScoped<IGeoZones, GeoZones>();

            services.AddSwaggerGen();

            services.AddHttpClient<DatabaseManagement>("elastic").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });

            services.AddHttpClient<ILogger, HttpLogger>();

            //new
            services.AddSingleton<IReadableDatabase, ReadableDatabasesService>();
            services.AddTransient<IReloadDatabasesService, DatabaseManagementNewServices.Services.ReloadDatabasesFromFileService>();
            services.AddTransient<IDatabaseCheck, DatabaseCheckService>();
            services.AddTransient<IDatabaseAvailabilityControl, DatabaseAvailabilityControlService>();

            //old
            /*services.AddSingleton<IHostedService, DatabaseManagementUtils.ReloadDatabasesFromFileService>();
            services.AddSingleton<DatabaseManagement>();
            services.AddSingleton<IHostedService, DatabaseManagementService>();*/


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

            app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowedToAllowWildcardSubdomains().WithOrigins(Configuration.GetSection("CorsOrigins").Get<List<string>>().ToArray()));

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

            if (Configuration.GetValue<bool>("onlyHttps"))
            {
                app.UseHttpsRedirection();
            }
            
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            loggerFactory.AddHttp(Configuration["loggerHost"], Configuration.GetValue<int>("loggerPortUdp"), Configuration.GetValue<int>("loggerPortHttp"), Configuration["loggerEnv"]);
            loggerFactory.CreateLogger("HttpLogger");

            app.UseHangfireDashboard();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapControllerRoute(
                //    name: "default",
                //    pattern: "{controller=Home}/{action=Index}/{id?}");
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
