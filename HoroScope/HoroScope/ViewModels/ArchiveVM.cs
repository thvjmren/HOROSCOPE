using System.Globalization;

namespace HoroScope.ViewModels
{
    public class ArchiveVM
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName => new CultureInfo("en-US").DateTimeFormat.GetMonthName(Month).ToUpperInvariant();
        public int Count { get; set; }
    }

}
