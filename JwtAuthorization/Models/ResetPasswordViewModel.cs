using System.ComponentModel.DataAnnotations;

namespace JwtAuthorization.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; }

        [Required, MinLength(6)]
        public string NewPassword { get; set; }
    }
}
