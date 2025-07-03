namespace HoroScope.ViewModels
{
    public class UserProfileVM
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }

        public DateTime? BirthDate { get; set; }
        public string? BirthTime { get; set; }
        public string? BirthPlace { get; set; }

        public IFormFile? ProfilePhoto { get; set; }

        public string? SunSign { get; set; }
        public string? RisingSign { get; set; }
        public string? MoonSign { get; set; }
        private string? _profileImageUrl;
        public string? ProfileImageUrl
        {
            get => string.IsNullOrEmpty(_profileImageUrl) ? "/assets/images/defaulticon.jpg" : _profileImageUrl;
            set => _profileImageUrl = value;
        }

    }
}
