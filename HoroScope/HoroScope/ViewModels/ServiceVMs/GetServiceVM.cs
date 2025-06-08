using System.ComponentModel.DataAnnotations;

namespace HoroScope.ViewModels.ServiceVMs
{
    public class GetServiceVM
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public string CategoryName { get; set; }
    }
}
