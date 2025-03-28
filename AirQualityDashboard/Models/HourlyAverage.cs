namespace AirQualityDashboard.Models
{
    public class HourlyAverage
    {
        public DateTime Timestamp { get; set; }
        public double PM25 { get; set; }
        public double PM10 { get; set; }
        public double CO2 { get; set; }
    }
}
