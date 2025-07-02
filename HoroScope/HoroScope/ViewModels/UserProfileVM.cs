namespace HoroScope.ViewModels
{
    public class UserProfileVM
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }

        public DateTime? BirthDate { get; set; }
        public string BirthTime { get; set; }
        public string BirthPlace { get; set; }

        public IFormFile? ProfilePhoto { get; set; }

        public string? SunSign { get; set; }
        public string? RisingSign { get; set; }
        public string? MoonSign { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
