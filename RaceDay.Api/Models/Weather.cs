using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

[Table("Weather")]
public class Weather
{
    public int WeatherID { get; set; }

    public int EventID { get; set; }
    public Event Event { get; set; } = null!;

    public DateTime RecordedAt { get; set; }

    public decimal? TemperatureC { get; set; }

    [MaxLength(100)]
    public string? WeatherCondition { get; set; }

    public decimal? WindSpeedKMH { get; set; }

    public decimal? RainProbability { get; set; }
}
