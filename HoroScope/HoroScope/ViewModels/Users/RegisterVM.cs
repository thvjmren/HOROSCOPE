using System.ComponentModel.DataAnnotations;

namespace HoroScope.ViewModels
{
    public class RegisterVM
    {
        [Required]
        [MinLength(3)]
        [MaxLength(50, ErrorMessage = "max 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "only letters")]
        public string Name { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "max 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "only letters")]
        public string Surname { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "max 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9._]+$", ErrorMessage = "only letters")]
        public string UserName { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
    }
}
