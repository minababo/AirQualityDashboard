using AirQualityDashboard.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Sensor
{
    [Key]
    public int SensorId { get; set; }

    [Required]
    public string SensorName { get; set; } = string.Empty;

    [Required]
    public string LocationName { get; set; } = string.Empty;

    [Required]
    public double Latitude { get; set; }

    [Required]
    public double Longitude { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<AQIData> AQIDataRecords { get; set; } = new List<AQIData>();
}
