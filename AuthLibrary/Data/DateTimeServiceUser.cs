using DateTimeService.Areas.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace AuthLibrary.Data
{
    // Add profile data for application users by adding properties to the DateTimeServiceUser class

    public class DateTimeServiceUser : IdentityUser
    {
        [JsonIgnore]
        public List<RefreshToken> RefreshTokens { get; set; }
    }
}
