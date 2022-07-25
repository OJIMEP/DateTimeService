using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DateTimeService.Areas.Identity.Models
{
    public class RefreshTokenModel
    {
        [Required, JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
    }
}
