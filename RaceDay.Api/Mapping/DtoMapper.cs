using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using Route = RaceDay.Api.Models.Route;
using Environment = RaceDay.Api.Models.Environment;

namespace RaceDay.Api.Mapping;

public static class DtoMapper
{
    public static UserProfileDto ToProfile(User user) => new()
    {
        UserId = user.UserID,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        Role = user.Role,
        OrganisationId = user.OrganisationID,
        OrganisationName = user.Organisation?.Name,
        ParticipantId = user.Participant?.ParticipantID,
        DateOfBirth = user.Participant?.DateOfBirth,
        Gender = user.Participant?.Gender,
        EmergencyContact = user.Participant?.EmergencyContact,
        EmergencyContactNumber = user.Participant?.EmergencyContactNumber
    };

    public static EventDto ToEvent(Event e) => new()
    {
        EventId = e.EventID,
        OrganisationId = e.OrganisationID,
        OrganisationName = e.Organisation?.Name ?? string.Empty,
        Name = e.Name,
        Description = e.Description,
        EventDate = e.EventDate,
        StartTime = e.StartTime,
        Location = e.Location,
        DistanceKM = e.DistanceKM,
        EventType = e.EventType,
        Status = e.Status
    };

    public static CategoryDto ToCategory(Category c) => new()
    {
        CategoryId = c.CategoryID,
        Name = c.Name,
        Type = c.Type,
        DistanceKM = c.DistanceKM,
        AgeGroup = c.AgeGroup
    };

    public static EnrolmentDto ToEnrolment(Enrolment e) => new()
    {
        EnrolmentId = e.EnrolmentID,
        ParticipantId = e.ParticipantID,
        ParticipantName = e.Participant?.User is null ? string.Empty : $"{e.Participant.User.FirstName} {e.Participant.User.LastName}",
        EventId = e.EventID,
        EventName = e.Event?.Name ?? string.Empty,
        EventCategoryId = e.EventCategoryID,
        CategoryId = e.EventCategory?.CategoryID ?? 0,
        CategoryName = e.EventCategory?.Category?.Name ?? string.Empty,
        EnrolmentDate = e.EnrolmentDate,
        Status = e.Status
    };

    public static ResultDto ToResult(Result r) => new()
    {
        ResultId = r.ResultID,
        EnrolmentId = r.EnrolmentID,
        EventId = r.Enrolment?.EventID ?? 0,
        EventName = r.Enrolment?.Event?.Name ?? string.Empty,
        ParticipantId = r.Enrolment?.ParticipantID ?? 0,
        ParticipantName = r.Enrolment?.Participant?.User is null ? string.Empty : $"{r.Enrolment.Participant.User.FirstName} {r.Enrolment.Participant.User.LastName}",
        FinishTime = r.FinishTime,
        Position = r.Position,
        Pace = r.Pace,
        ResultStatus = r.ResultStatus
    };

    public static RouteDto ToRoute(Route r) => new()
    {
        RouteId = r.RouteID,
        EventId = r.EventID,
        Name = r.Name,
        DistanceKM = r.DistanceKM,
        ElevationM = r.ElevationM,
        MapURL = r.MapURL
    };

    public static WeatherDto ToWeather(Weather w) => new()
    {
        WeatherId = w.WeatherID,
        EventId = w.EventID,
        RecordedAt = w.RecordedAt,
        TemperatureC = w.TemperatureC,
        WeatherCondition = w.WeatherCondition,
        WindSpeedKMH = w.WindSpeedKMH,
        RainProbability = w.RainProbability
    };

    public static EnvironmentDto ToEnvironment(Environment e) => new()
    {
        EnvironmentId = e.EnvironmentID,
        EventId = e.EventID,
        TemperatureC = e.TemperatureC,
        HumidityPercent = e.HumidityPercent,
        WindSpeedKMH = e.WindSpeedKMH,
        SurfaceCondition = e.SurfaceCondition
    };
}
