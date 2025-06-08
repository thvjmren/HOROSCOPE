using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class Expert : BaseEntity
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Image { get; set; }
    }
}
