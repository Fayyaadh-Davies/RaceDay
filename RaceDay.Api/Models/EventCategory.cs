namespace RaceDay.Api.Models;

public class EventCategory
{
    public int EventCategoryID { get; set; }

    public int EventID { get; set; }
    public Event Event { get; set; } = null!;

    public int CategoryID { get; set; }
    public Category Category { get; set; } = null!;

    public decimal EntryFee { get; set; }

    public int? MaximumParticipants { get; set; }

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
