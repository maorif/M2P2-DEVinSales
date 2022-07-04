using System.ComponentModel.DataAnnotations;

namespace DevInSales.Application.Dtos
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "The field {0} is required.")]
        [EmailAddress(ErrorMessage = "The field {0} is invalid.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The field {0} is required.")]
        public string Password { get; set;}
    }
}