using System.ComponentModel.DataAnnotations;

namespace HoroScope.ViewModels
{
    public class LoginVM
    {
        [MaxLength(256)]
        public string UsernameOrEmail { get; set; }
        [MinLength(8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool IsPersistent { get; set; }
    }
}
