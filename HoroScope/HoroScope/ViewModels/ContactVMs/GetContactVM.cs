using System.ComponentModel.DataAnnotations;

namespace HoroScope.ViewModels
{
    public class GetContactVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required(ErrorMessage = "Message is required.")]
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
