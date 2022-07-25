using AuthLibrary.Data;
using DateTimeService.Controllers;
using DateTimeService.Data;
using DateTimeService.DatabaseManagementNewServices.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DateTimeService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //CreateHostBuilder(args).Build().Run();
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var db = scope.ServiceProvider.GetRequiredService<DateTimeServiceContext>();
                    db.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<DateTimeController>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }


                try
                {
                    var userManager = services.GetRequiredService<UserManager<DateTimeServiceUser>>();
                    var rolesManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var configuration = services.GetRequiredService<IConfiguration>();
                    await RoleInitializer.InitializeAsync(userManager, rolesManager, configuration);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<DateTimeController>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }

                try
                {
                    var db = scope.ServiceProvider.GetRequiredService<DateTimeServiceContext>();
                    await RoleInitializer.CleanTokensAsync(db);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<DateTimeController>>();
                    logger.LogError(ex, "An error occurred while clearing the database.");
                }

                
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    //logging.ClearProviders();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
