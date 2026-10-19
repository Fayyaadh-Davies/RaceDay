using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public class EventDto
{
    public int EventId { get; set; }
    public int OrganisationId { get; set; }
    public string OrganisationName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly EventDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal DistanceKM { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class EventRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateOnly EventDate { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [Range(0.1, 1000)]
    public decimal DistanceKM { get; set; }

    [Required]
    public string EventType { get; set; } = string.Empty;

    public string Status { get; set; } = "Upcoming";
}
