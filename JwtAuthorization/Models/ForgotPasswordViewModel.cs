using System.ComponentModel.DataAnnotations;

namespace JwtAuthorization.Models
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
