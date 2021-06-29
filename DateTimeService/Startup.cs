using DateTimeService.Areas.Identity.Data;
using DateTimeService.Data;
using DateTimeService.DatabaseManagementUtils;
using DateTimeService.Logging;
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
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

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
            services.AddIdentity<DateTimeServiceUser, IdentityRole>()
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
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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

            //DatabaseList.CreateDatabases(Configuration.GetSection("OneSDatabases").Get<List<DatabaseConnectionParameter>>());            

            services.AddSingleton<IHostedService, ReloadDatabasesFromFileService>();

            services.AddSingleton<DatabaseManagement>();
            services.AddSingleton<IHostedService, DatabaseManagementService>();
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                           ForwardedHeaders.XForwardedProto
            });

            //app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowedToAllowWildcardSubdomains().WithOrigins("https://*.21vek.by", "https://localhost*", "https://*.swagger.io"));
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

            app.UseHttpsRedirection();

            app.UseRouting();



            app.UseAuthentication();
            app.UseAuthorization();

            //loggerFactory = LoggerFactory.Create(builder => builder.ClearProviders());

            loggerFactory.AddHttp(Configuration["loggerHost"], Configuration.GetValue<int>("loggerPortUdp"), Configuration.GetValue<int>("loggerPortHttp"));
            var logger = loggerFactory.CreateLogger("HttpLogger");


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
