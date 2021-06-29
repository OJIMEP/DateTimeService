using DateTimeService.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace DateTimeService.Areas.Identity.Data
{
    public class RoleInitializer
    {
        public static async Task InitializeAsync(UserManager<DateTimeServiceUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration _configuration)
        {
            string adminEmail = _configuration["Identity:adminEmail"];
            string password = _configuration["Identity:adminPass"];
            if (await roleManager.FindByNameAsync(UserRoles.Admin) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            }
            if (await roleManager.FindByNameAsync(UserRoles.MaxAvailableCount) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(UserRoles.MaxAvailableCount));
            }
            if (await roleManager.FindByNameAsync(UserRoles.AvailableDate) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(UserRoles.AvailableDate));
            }
            if (await roleManager.FindByNameAsync(UserRoles.IntervalList) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(UserRoles.IntervalList));
            }
            if (await roleManager.FindByNameAsync(UserRoles.User) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
            }
            if (await userManager.FindByNameAsync(adminEmail) == null)
            {
                DateTimeServiceUser admin = new() { Email = adminEmail, UserName = adminEmail };
                IdentityResult result = await userManager.CreateAsync(admin, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, UserRoles.Admin);
                }
            }
        }
        public static async Task CleanTokensAsync(DateTimeServiceContext db)
        {
            await db.Database.ExecuteSqlRawAsync(@"Delete
  FROM [dbo].[RefreshToken]
  Where[Created] < DATEADD(DAY, -2, GETUTCDATE())");
        }

    }
}
