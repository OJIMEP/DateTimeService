using Microsoft.AspNetCore.Identity;
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
            if (await userManager.FindByNameAsync(adminEmail) == null)
            {
                DateTimeServiceUser admin = new DateTimeServiceUser { Email = adminEmail, UserName = adminEmail };
                IdentityResult result = await userManager.CreateAsync(admin, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "admin");
                }
            }
        }
    }
}
