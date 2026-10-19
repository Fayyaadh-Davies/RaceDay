using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

[Table("Event")]
public class Event
{
    public int EventID { get; set; }

    public int OrganisationID { get; set; }
    public Organisation Organisation { get; set; } = null!;

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateOnly EventDate { get; set; }

    public TimeOnly StartTime { get; set; }

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    public decimal DistanceKM { get; set; }

    [Required, MaxLength(20)]
    public string EventType { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string Status { get; set; } = "Upcoming";

    public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    public Route? Route { get; set; }
    public Environment? Environment { get; set; }
    public ICollection<Weather> WeatherRecords { get; set; } = new List<Weather>();
}
