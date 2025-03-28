using AirQualityDashboard.Models;

public class SensorDetailsViewModel
{
    public Sensor Sensor { get; set; }
    public List<AQIData> PagedAQIData { get; set; }
    public List<AQIData> FullAQIData { get; set; }
    public string Filter { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public List<HourlyAverage> HourlyAverages { get; set; }
    public AQIData LatestReading { get; set; }
}