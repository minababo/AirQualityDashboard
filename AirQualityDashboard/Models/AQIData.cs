using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class AQIData
{
    [Key]
    public int AQIDataId { get; set; }

    public int SensorId { get; set; }

    [ForeignKey("SensorId")]
    public Sensor Sensor { get; set; }

    [Required]
    public double PM25 { get; set; }

    [Required]
    public double PM10 { get; set; }

    [Required]
    public double CO2 { get; set; }

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
