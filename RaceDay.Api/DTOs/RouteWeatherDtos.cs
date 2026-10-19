using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public class RouteDto
{
    public int RouteId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DistanceKM { get; set; }
    public int? ElevationM { get; set; }
    public string? MapURL { get; set; }
}

public class RouteRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Range(0.1, 1000)]
    public decimal DistanceKM { get; set; }

    public int? ElevationM { get; set; }

    [MaxLength(500)]
    public string? MapURL { get; set; }
}

public class WeatherDto
{
    public int WeatherId { get; set; }
    public int EventId { get; set; }
    public DateTime RecordedAt { get; set; }
    public decimal? TemperatureC { get; set; }
    public string? WeatherCondition { get; set; }
    public decimal? WindSpeedKMH { get; set; }
    public decimal? RainProbability { get; set; }
}

public class EnvironmentDto
{
    public int EnvironmentId { get; set; }
    public int EventId { get; set; }
    public decimal? TemperatureC { get; set; }
    public decimal? HumidityPercent { get; set; }
    public decimal? WindSpeedKMH { get; set; }
    public string? SurfaceCondition { get; set; }
}
