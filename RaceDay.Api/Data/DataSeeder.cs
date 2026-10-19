using RaceDay.Api.Auth;
using RaceDay.Api.Models;
using Route = RaceDay.Api.Models.Route;
using Environment = RaceDay.Api.Models.Environment;
namespace RaceDay.Api.Data;

public static class DataSeeder
{
    public const string DemoPassword = "Password123!";

    public static void Seed(RaceDayContext db)
    {
        if (db.Users.Any())
        {
            return;
        }

        var hash = PasswordHasher.Hash(DemoPassword);

        var orgs = new List<Organisation>
        {
            new() { Name = "Johannesburg Road Runners", RegistrationNumber = "ORG001", ContactEmail = "info@jrr.co.za", ContactNumber = "0115551001", Address = "Johannesburg, Gauteng" },
            new() { Name = "Durban Coastal Athletics", RegistrationNumber = "ORG002", ContactEmail = "events@dca.co.za", ContactNumber = "0315551002", Address = "Durban, KwaZulu-Natal" },
            new() { Name = "Cape Active Events", RegistrationNumber = "ORG003", ContactEmail = "info@capeactive.co.za", ContactNumber = "0215551003", Address = "Cape Town, Western Cape" },
            new() { Name = "Gauteng Cycling Events", RegistrationNumber = "ORG004", ContactEmail = "events@gce.co.za", ContactNumber = "0115551004", Address = "Pretoria, Gauteng" }
        };
        db.Organisations.AddRange(orgs);
        db.SaveChanges();

        var users = new List<User>
        {
            new() { FirstName = "Admin", LastName = "RaceDay", Email = "admin@raceday.co.za", PasswordHash = hash, Role = Roles.Admin },
            new() { FirstName = "Thabo", LastName = "Mokoena", Email = "thabo@example.com", PasswordHash = hash, Role = Roles.Participant },
            new() { FirstName = "Ayesha", LastName = "Davids", Email = "ayesha@example.com", PasswordHash = hash, Role = Roles.Participant },
            new() { FirstName = "Liam", LastName = "Naidoo", Email = "liam@example.com", PasswordHash = hash, Role = Roles.Participant },
            new() { FirstName = "Mia", LastName = "Jacobs", Email = "mia@example.com", PasswordHash = hash, Role = Roles.Participant },
            new() { FirstName = "Daniel", LastName = "Peters", Email = "daniel@example.com", PasswordHash = hash, Role = Roles.Participant },
            new() { FirstName = "Zanele", LastName = "Dlamini", Email = "zanele@example.com", PasswordHash = hash, Role = Roles.Participant },
            new() { FirstName = "Ryan", LastName = "Smith", Email = "ryan@example.com", PasswordHash = hash, Role = Roles.Participant },
            new() { FirstName = "John", LastName = "Radebe", Email = "john@jrr.co.za", PasswordHash = hash, Role = Roles.Organiser, OrganisationID = orgs[0].OrganisationID },
            new() { FirstName = "Fatima", LastName = "Khan", Email = "fatima@dca.co.za", PasswordHash = hash, Role = Roles.Organiser, OrganisationID = orgs[1].OrganisationID },
            new() { FirstName = "Jason", LastName = "Williams", Email = "jason@capeactive.co.za", PasswordHash = hash, Role = Roles.Organiser, OrganisationID = orgs[2].OrganisationID },
            new() { FirstName = "Peter", LastName = "Molefe", Email = "peter@gce.co.za", PasswordHash = hash, Role = Roles.Organiser, OrganisationID = orgs[3].OrganisationID }
        };
        db.Users.AddRange(users);
        db.SaveChanges();

        var participantUsers = users.Where(u => u.Role == Roles.Participant).ToList();
        var dobs = new[]
        {
            new DateOnly(1998, 5, 12), new DateOnly(1999, 8, 21), new DateOnly(1997, 3, 17),
            new DateOnly(2000, 11, 5), new DateOnly(1996, 7, 9), new DateOnly(1998, 12, 15),
            new DateOnly(1995, 2, 28)
        };
        var genders = new[] { "Male", "Female", "Male", "Female", "Male", "Female", "Male" };
        var contacts = new[]
        {
            ("Sipho Mokoena", "0821111111"), ("Sara Davids", "0822222222"), ("Nadia Naidoo", "0823333333"),
            ("Paul Jacobs", "0824444444"), ("Linda Peters", "0825555555"), ("Thandi Dlamini", "0826666666"),
            ("Mark Smith", "0827777777")
        };

        for (var i = 0; i < participantUsers.Count; i++)
        {
            db.Participants.Add(new Participant
            {
                UserID = participantUsers[i].UserID,
                DateOfBirth = dobs[i],
                Gender = genders[i],
                EmergencyContact = contacts[i].Item1,
                EmergencyContactNumber = contacts[i].Item2
            });
        }
        db.SaveChanges();

        var categories = new List<Category>
        {
            new() { Name = "5KM Fun Run", Type = "Running", DistanceKM = 5.00m, AgeGroup = "Open" },
            new() { Name = "10KM Road Run", Type = "Running", DistanceKM = 10.00m, AgeGroup = "Senior" },
            new() { Name = "21KM Half Marathon", Type = "Running", DistanceKM = 21.10m, AgeGroup = "Open" },
            new() { Name = "5KM Walk", Type = "Walking", DistanceKM = 5.00m, AgeGroup = "Open" },
            new() { Name = "10KM Walk", Type = "Walking", DistanceKM = 10.00m, AgeGroup = "Open" },
            new() { Name = "20KM Cycle", Type = "Cycling", DistanceKM = 20.00m, AgeGroup = "Open" },
            new() { Name = "50KM Cycle", Type = "Cycling", DistanceKM = 50.00m, AgeGroup = "Open" },
            new() { Name = "Under 20 10KM", Type = "Running", DistanceKM = 10.00m, AgeGroup = "Under 20" }
        };
        db.Categories.AddRange(categories);
        db.SaveChanges();

        var events = new List<Event>
        {
            new() { OrganisationID = orgs[0].OrganisationID, Name = "Johannesburg City Run", Description = "Annual Johannesburg road running event", EventDate = new DateOnly(2026, 10, 10), StartTime = new TimeOnly(6, 30), Location = "Johannesburg", DistanceKM = 10.00m, EventType = "Running", Status = "Open" },
            new() { OrganisationID = orgs[1].OrganisationID, Name = "Durban Coastal Walk", Description = "Scenic coastal walking event", EventDate = new DateOnly(2026, 10, 18), StartTime = new TimeOnly(7, 0), Location = "Durban", DistanceKM = 10.00m, EventType = "Walking", Status = "Open" },
            new() { OrganisationID = orgs[2].OrganisationID, Name = "Cape Active Half Marathon", Description = "Half marathon through Cape Town", EventDate = new DateOnly(2026, 11, 1), StartTime = new TimeOnly(6, 0), Location = "Cape Town", DistanceKM = 21.10m, EventType = "Running", Status = "Upcoming" },
            new() { OrganisationID = orgs[3].OrganisationID, Name = "Gauteng Cycle Challenge", Description = "Road cycling challenge", EventDate = new DateOnly(2026, 11, 15), StartTime = new TimeOnly(5, 30), Location = "Pretoria", DistanceKM = 50.00m, EventType = "Cycling", Status = "Upcoming" }
        };
        db.Events.AddRange(events);
        db.SaveChanges();

        var eventCategories = new List<EventCategory>
        {
            new() { EventID = events[0].EventID, CategoryID = categories[0].CategoryID, EntryFee = 100.00m, MaximumParticipants = 1000 },
            new() { EventID = events[0].EventID, CategoryID = categories[1].CategoryID, EntryFee = 180.00m, MaximumParticipants = 1000 },
            new() { EventID = events[1].EventID, CategoryID = categories[3].CategoryID, EntryFee = 80.00m, MaximumParticipants = 500 },
            new() { EventID = events[1].EventID, CategoryID = categories[4].CategoryID, EntryFee = 120.00m, MaximumParticipants = 500 },
            new() { EventID = events[2].EventID, CategoryID = categories[1].CategoryID, EntryFee = 180.00m, MaximumParticipants = 1500 },
            new() { EventID = events[2].EventID, CategoryID = categories[2].CategoryID, EntryFee = 300.00m, MaximumParticipants = 1500 },
            new() { EventID = events[3].EventID, CategoryID = categories[5].CategoryID, EntryFee = 200.00m, MaximumParticipants = 800 },
            new() { EventID = events[3].EventID, CategoryID = categories[6].CategoryID, EntryFee = 350.00m, MaximumParticipants = 800 }
        };
        db.EventCategories.AddRange(eventCategories);
        db.SaveChanges();

        db.Routes.AddRange(
            new Route { EventID = events[0].EventID, Name = "Johannesburg City Route", DistanceKM = 10.00m, ElevationM = 180, MapURL = "https://example.com/routes/johannesburg" },
            new Route { EventID = events[1].EventID, Name = "Durban Coastal Route", DistanceKM = 10.00m, ElevationM = 65, MapURL = "https://example.com/routes/durban" },
            new Route { EventID = events[2].EventID, Name = "Cape Town Half Marathon Route", DistanceKM = 21.10m, ElevationM = 220, MapURL = "https://example.com/routes/capetown" },
            new Route { EventID = events[3].EventID, Name = "Gauteng Cycling Route", DistanceKM = 50.00m, ElevationM = 540, MapURL = "https://example.com/routes/gauteng" }
        );

        db.Environments.AddRange(
            new Environment { EventID = events[0].EventID, TemperatureC = 18.00m, HumidityPercent = 55.00m, WindSpeedKMH = 12.00m, SurfaceCondition = "Dry asphalt" },
            new Environment { EventID = events[1].EventID, TemperatureC = 22.00m, HumidityPercent = 70.00m, WindSpeedKMH = 18.00m, SurfaceCondition = "Dry coastal road" },
            new Environment { EventID = events[2].EventID, TemperatureC = 16.00m, HumidityPercent = 60.00m, WindSpeedKMH = 10.00m, SurfaceCondition = "Dry asphalt" },
            new Environment { EventID = events[3].EventID, TemperatureC = 15.00m, HumidityPercent = 50.00m, WindSpeedKMH = 15.00m, SurfaceCondition = "Dry road" }
        );

        db.WeatherRecords.AddRange(
            new Weather { EventID = events[0].EventID, RecordedAt = new DateTime(2026, 10, 10, 5, 30, 0), TemperatureC = 17.50m, WeatherCondition = "Partly Cloudy", WindSpeedKMH = 11.00m, RainProbability = 10.00m },
            new Weather { EventID = events[1].EventID, RecordedAt = new DateTime(2026, 10, 18, 6, 0, 0), TemperatureC = 21.00m, WeatherCondition = "Sunny", WindSpeedKMH = 16.00m, RainProbability = 5.00m },
            new Weather { EventID = events[2].EventID, RecordedAt = new DateTime(2026, 11, 1, 5, 0, 0), TemperatureC = 15.00m, WeatherCondition = "Clear", WindSpeedKMH = 9.00m, RainProbability = 5.00m },
            new Weather { EventID = events[3].EventID, RecordedAt = new DateTime(2026, 11, 15, 4, 30, 0), TemperatureC = 14.00m, WeatherCondition = "Clear", WindSpeedKMH = 13.00m, RainProbability = 8.00m }
        );
        db.SaveChanges();

        var participants = db.Participants.OrderBy(p => p.ParticipantID).ToList();
        var enrolments = new List<Enrolment>
        {
            new() { ParticipantID = participants[0].ParticipantID, EventID = events[0].EventID, EventCategoryID = eventCategories[0].EventCategoryID, Status = "Confirmed" },
            new() { ParticipantID = participants[1].ParticipantID, EventID = events[0].EventID, EventCategoryID = eventCategories[1].EventCategoryID, Status = "Confirmed" },
            new() { ParticipantID = participants[2].ParticipantID, EventID = events[0].EventID, EventCategoryID = eventCategories[0].EventCategoryID, Status = "Confirmed" },
            new() { ParticipantID = participants[3].ParticipantID, EventID = events[1].EventID, EventCategoryID = eventCategories[2].EventCategoryID, Status = "Confirmed" },
            new() { ParticipantID = participants[4].ParticipantID, EventID = events[1].EventID, EventCategoryID = eventCategories[3].EventCategoryID, Status = "Confirmed" },
            new() { ParticipantID = participants[5].ParticipantID, EventID = events[2].EventID, EventCategoryID = eventCategories[5].EventCategoryID, Status = "Confirmed" },
            new() { ParticipantID = participants[6].ParticipantID, EventID = events[3].EventID, EventCategoryID = eventCategories[7].EventCategoryID, Status = "Confirmed" }
        };
        db.Enrolments.AddRange(enrolments);
        db.SaveChanges();

        db.Results.AddRange(
            new Result { EnrolmentID = enrolments[0].EnrolmentID, FinishTime = new TimeOnly(0, 27, 45), Position = 15, Pace = "5:33 min/km", ResultStatus = "Finished" },
            new Result { EnrolmentID = enrolments[1].EnrolmentID, FinishTime = new TimeOnly(0, 52, 20), Position = 21, Pace = "5:14 min/km", ResultStatus = "Finished" },
            new Result { EnrolmentID = enrolments[2].EnrolmentID, FinishTime = new TimeOnly(0, 31, 10), Position = 38, Pace = "6:14 min/km", ResultStatus = "Finished" }
        );
        db.SaveChanges();
    }
}
