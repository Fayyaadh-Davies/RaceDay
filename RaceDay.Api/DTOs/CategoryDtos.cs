using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal DistanceKM { get; set; }
    public string? AgeGroup { get; set; }
}

public class CategoryRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    [Range(0.1, 1000)]
    public decimal DistanceKM { get; set; }

    public string? AgeGroup { get; set; }

    public int? EventId { get; set; }
    public decimal EntryFee { get; set; }
    public int? MaximumParticipants { get; set; }
}

public class EventCategoryDto
{
    public int EventCategoryId { get; set; }
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal DistanceKM { get; set; }
    public string? AgeGroup { get; set; }
    public decimal EntryFee { get; set; }
    public int? MaximumParticipants { get; set; }
}
