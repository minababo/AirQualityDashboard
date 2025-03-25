using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class AQIData
{
    [Key]
    public int AQIDataId { get; set; }

    [Required]
    public int SensorId { get; set; }

    [ForeignKey("SensorId")]
    public Sensor Sensor { get; set; } = null!;

    [Required]
    public int AQI { get; set; }

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.Now;
}