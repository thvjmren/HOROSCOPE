using System.ComponentModel.DataAnnotations;

namespace HoroScope.ViewModels
{
    public class GetServiceCategoryVM
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
