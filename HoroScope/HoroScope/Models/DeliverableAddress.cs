using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class DeliverableAddress : BaseEntity
    {
        public string PinCode { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
