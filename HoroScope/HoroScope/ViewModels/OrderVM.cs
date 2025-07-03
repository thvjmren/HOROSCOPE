namespace HoroScope.ViewModels
{
    public class OrderVM
    {
        public string ZipCode { get; set; }
        public string PhoneNumber { get; set; }
        public List<BasketInOrderVM>? BasketInOrderVMs { get; set; }

    }
}
