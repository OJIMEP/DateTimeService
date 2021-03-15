//using System;
//using DateTimeService.Areas.Identity.Data;
//using DateTimeService.Data;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.UI;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;

//[assembly: HostingStartup(typeof(DateTimeService.Areas.Identity.IdentityHostingStartup))]
//namespace DateTimeService.Areas.Identity
//{
//    public class IdentityHostingStartup : IHostingStartup
//    {
//        public void Configure(IWebHostBuilder builder)
//        {
//            builder.ConfigureServices((context, services) =>
//            {
//                services.AddDbContext<DateTimeServiceContext>(options =>
//                    options.UseSqlServer(
//                        context.Configuration.GetConnectionString("DateTimeServiceContextConnection")));

//                services.AddDefaultIdentity<DateTimeServiceUser>(options => options.SignIn.RequireConfirmedAccount = true)
//                    .AddEntityFrameworkStores<DateTimeServiceContext>();
//            });
//        }
//    }
//}