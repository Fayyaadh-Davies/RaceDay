using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public class ResultRequest
{
    [Required]
    public int EnrolmentId { get; set; }

    public TimeOnly? FinishTime { get; set; }

    public int? Position { get; set; }

    public string? Pace { get; set; }

    public string ResultStatus { get; set; } = "Finished";
}

public class UpdateResultRequest
{
    public TimeOnly? FinishTime { get; set; }
    public int? Position { get; set; }
    public string? Pace { get; set; }
    public string ResultStatus { get; set; } = "Finished";
}

public class ResultDto
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public TimeOnly? FinishTime { get; set; }
    public int? Position { get; set; }
    public string? Pace { get; set; }
    public string ResultStatus { get; set; } = string.Empty;
}
