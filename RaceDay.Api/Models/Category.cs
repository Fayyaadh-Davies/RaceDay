using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Category
{
    public int CategoryID { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Type { get; set; } = string.Empty;

    public decimal DistanceKM { get; set; }

    [MaxLength(50)]
    public string? AgeGroup { get; set; }

    public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
}
