using System.ComponentModel.DataAnnotations;

namespace DateTimeService.Areas.Identity.Models
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "User Name is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Old password is required")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "New password is required")]
        public string NewPassword { get; set; }
    }
}
