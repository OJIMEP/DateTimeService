using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DateTimeService.Areas.Identity.Models
{
    public class RefreshTokenModel
    {
        [Required]
        public string refresh_token { get; set;}
    }
}
