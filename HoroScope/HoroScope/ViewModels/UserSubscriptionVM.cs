namespace HoroScope.ViewModels
{
    public class UserSubscriptionVM
    {
        public int SubId { get; set; }
        public string PlanName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
