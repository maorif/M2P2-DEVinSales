using System.ComponentModel.DataAnnotations;

namespace DevInSales.Application.Dtos
{
    public class RegisterUserRequest
    {
        [Required(ErrorMessage = "The field {0} is required")]
        [EmailAddress(ErrorMessage = "The field {0} is invalid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(50, ErrorMessage = "The field {0} should contain between {2} and {1] caracters", MinimumLength = 6)]
        public string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Password should be the same")]
        public string ConfirmPassword { get; set; }
    }
}