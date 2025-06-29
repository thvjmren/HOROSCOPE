using HoroScope.Models.Base;
using System.ComponentModel.DataAnnotations;


namespace HoroScope.Models
{
    public class Contact : BaseEntity
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Message { get; set; }
    }
}
