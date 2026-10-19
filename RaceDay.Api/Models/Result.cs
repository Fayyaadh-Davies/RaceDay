using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Result
{
    public int ResultID { get; set; }

    public int EnrolmentID { get; set; }
    public Enrolment Enrolment { get; set; } = null!;

    public TimeOnly? FinishTime { get; set; }

    public int? Position { get; set; }

    [MaxLength(30)]
    public string? Pace { get; set; }

    [Required, MaxLength(30)]
    public string ResultStatus { get; set; } = "Finished";
}
