namespace AirQualityDashboard.Models.ViewModels
{
    public class SensorMapViewModel
    {
        public int SensorId { get; set; }
        public string SensorName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public double? LatestPM25 { get; set; }
        public DateTime? LatestTimestamp { get; set; }
        public List<DateTime> TrendTimestamps { get; set; }
        public List<double> TrendPM25 { get; set; }
        public List<double> TrendPM10 { get; set; }
        public bool IsActive { get; set; }
    }
}
