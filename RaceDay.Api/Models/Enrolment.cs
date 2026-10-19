using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Enrolment
{
    public int EnrolmentID { get; set; }

    public int ParticipantID { get; set; }
    public Participant Participant { get; set; } = null!;

    public int EventID { get; set; }
    public Event Event { get; set; } = null!;

    public int EventCategoryID { get; set; }
    public EventCategory EventCategory { get; set; } = null!;

    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(30)]
    public string Status { get; set; } = "Confirmed";

    public Result? Result { get; set; }
}
