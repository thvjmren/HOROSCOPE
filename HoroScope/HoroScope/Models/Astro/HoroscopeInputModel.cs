public class HoroscopeInputModel
{
    public string City { get; set; } = "";

    public int Day { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }

    public int Hour { get; set; }
    public int Minute { get; set; }

    public DateTime BirthDate => new DateTime(Year, Month, Day);
    public TimeSpan BirthTime => new TimeSpan(Hour, Minute, 0);
}
