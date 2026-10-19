using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Organisation
{
    public int OrganisationID { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string ContactEmail { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? ContactNumber { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
