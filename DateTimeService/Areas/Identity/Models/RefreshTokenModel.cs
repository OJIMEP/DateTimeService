using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DateTimeService.Areas.Identity.Models
{
    public class RefreshTokenModel
    {
        [Required, JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set;}
    }
}
