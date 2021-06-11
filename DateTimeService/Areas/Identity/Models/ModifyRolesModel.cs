using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DateTimeService.Areas.Identity.Models
{
    public class ModifyRolesModel
    {
        [Required(ErrorMessage = "User Name is required")]
        public string Username { get; set; }

        [JsonPropertyName("addRoles")]
        public string[] AddRoles { get; set; }

        [JsonPropertyName("deleteRoles")]
        public string[] DeleteRoles { get; set; }
    }
}
