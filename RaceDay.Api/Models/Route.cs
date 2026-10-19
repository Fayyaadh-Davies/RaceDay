using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Route
{
    public int RouteID { get; set; }

    public int EventID { get; set; }
    public Event Event { get; set; } = null!;

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public decimal DistanceKM { get; set; }

    public int? ElevationM { get; set; }

    [MaxLength(500)]
    public string? MapURL { get; set; }
}
