using System.ComponentModel.DataAnnotations;

namespace DateTimeService.Areas.Identity.Models
{
    public class DeleteModel
    {
        [Required(ErrorMessage = "User Name is required")]
        public string Username { get; set; }
    }
}
