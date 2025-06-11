using System.ComponentModel.DataAnnotations;

namespace Subscription_Manager.Dtos.Account
{
    public class RegisterDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
