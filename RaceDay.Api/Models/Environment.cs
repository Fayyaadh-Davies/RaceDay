using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

[Table("Environment")]
public class Environment
{
    public int EnvironmentID { get; set; }

    public int EventID { get; set; }
    public Event Event { get; set; } = null!;

    public decimal? TemperatureC { get; set; }

    public decimal? HumidityPercent { get; set; }

    public decimal? WindSpeedKMH { get; set; }

    [MaxLength(100)]
    public string? SurfaceCondition { get; set; }
}
