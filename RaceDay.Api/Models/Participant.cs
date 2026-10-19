using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Participant
{
    public int ParticipantID { get; set; }

    public int UserID { get; set; }
    public User User { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(20)]
    public string? Gender { get; set; }

    [MaxLength(100)]
    public string? EmergencyContact { get; set; }

    [MaxLength(30)]
    public string? EmergencyContactNumber { get; set; }

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
