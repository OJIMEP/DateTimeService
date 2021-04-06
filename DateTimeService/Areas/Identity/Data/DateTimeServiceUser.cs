using DateTimeService.Areas.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DateTimeService.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the DateTimeServiceUser class
    public class DateTimeServiceUser : IdentityUser
    {
        [JsonIgnore]
        public List<RefreshToken> RefreshTokens { get; set; }
    }
}
