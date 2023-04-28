using DateTimeService.Data;
using DateTimeService.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using DateTimeService.DatabaseManagementNewServices.Interfaces;
using DateTimeService.DatabaseManagementNewServices.Services;
using System.Net.Http;
using DateTimeService.Services;
using AuthLibrary.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace DateTimeService.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFilters(this IServiceCollection services)
        {
            services.AddScoped<LogActionFilter>();
            
            return services;
        }

        public static IServiceCollection AddHttpClients(this IServiceCollection services)
        {
            services.AddHttpClient<IGeoZones, GeoZones>();
            services.AddHttpClient<ILogger, HttpLogger>();

            services.AddHttpClient<DatabaseManagement>("elastic").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });

            return services;
        }

        public static IServiceCollection AddDatabaseManagement(this IServiceCollection services)
        {
            //new
            services.AddSingleton<IReadableDatabase, ReadableDatabasesService>();
            services.AddTransient<IReloadDatabasesService, DatabaseManagementNewServices.Services.ReloadDatabasesFromFileService>();
            services.AddTransient<IDatabaseCheck, DatabaseCheckService>();
            services.AddTransient<IDatabaseAvailabilityControl, DatabaseAvailabilityControlService>();

            //old
            /*services.AddSingleton<IHostedService, DatabaseManagementUtils.ReloadDatabasesFromFileService>();
            services.AddSingleton<DatabaseManagement>();
            services.AddSingleton<IHostedService, DatabaseManagementService>();*/

            return services;
        }

        public static IServiceCollection AddDataServices(this IServiceCollection services)
        {
            services.AddScoped<IGeoZones, GeoZones>();
            services.AddTransient<IDataService, DataService>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<ILoadBalancing, LoadBalancing>();

            return services;
        }

        public static IServiceCollection AddAuthorizationAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
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
                    ValidAudience = configuration["JWT:ValidAudience"],
                    ValidIssuer = configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"])),
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

            return services;
        }
    }
}
