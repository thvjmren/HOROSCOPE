using System.ComponentModel.DataAnnotations;

namespace HoroScope.Models
{
    public class GetInfoVM
    {
        [Phone(ErrorMessage = "The phone number is not valid.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "The email address is not valid.")]
        public string Email { get; set; }

        public string Address { get; set; }

        [Url(ErrorMessage = "The Facebook URL is not valid.")]
        public string FacebookUrl { get; set; }

        [Url(ErrorMessage = "The Twitter URL is not valid.")]
        public string TwitterUrl { get; set; }

        [Url(ErrorMessage = "The Instagram URL is not valid.")]
        public string InstagramUrl { get; set; }

        [Url(ErrorMessage = "The LinkedIn URL is not valid.")]
        public string LinkedinUrl { get; set; }
    }
}
