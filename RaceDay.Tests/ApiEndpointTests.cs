using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;


namespace RaceDay.Tests;

public class ApiEndpointTests : IClassFixture<RaceDayWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly RaceDayWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ApiEndpointTests(RaceDayWebApplicationFactory factory)
    {
        _factory = factory;
        _client = CreateClient();
    }

    [Fact]
    public async Task PublicEvents_ReturnsSeededEvents()
    {
        var response = await _client.GetAsync("/api/events");
        response.EnsureSuccessStatusCode();
        var events = await response.Content.ReadFromJsonAsync<List<EventDto>>(JsonOptions);
        Assert.NotNull(events);
        Assert.NotEmpty(events);
        Assert.Contains(events, e => e.Name == "Johannesburg City Run");
        Assert.All(events, e => Assert.False(string.IsNullOrWhiteSpace(e.EventType)));
    }

    [Fact]
    public async Task Profile_WithoutSession_Returns401()
    {
        var response = await _client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegisterAndLogin_Participant_CanViewProfile()
    {
        var email = $"new.runner.{Guid.NewGuid():N}@example.com";
        var register = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Sipho",
            lastName = "Khumalo",
            email,
            password = DataSeeder.DemoPassword,
            role = "Participant",
            dateOfBirth = "1999-04-01",
            gender = "Male",
            emergencyContact = "Lerato Khumalo",
            emergencyContactNumber = "0829990001"
        });
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        var loginClient = CreateClient();
        await Login(loginClient, email, DataSeeder.DemoPassword);

        var me = await loginClient.GetAsync("/api/users/me");
        me.EnsureSuccessStatusCode();
        var profile = await me.Content.ReadFromJsonAsync<UserProfileDto>(JsonOptions);
        Assert.Equal("Participant", profile!.Role);
        Assert.Equal(email, profile.Email);
    }

    [Fact]
    public async Task Participant_CannotCreateEvent()
    {
        var client = CreateClient();
        await Login(client, "thabo@example.com", DataSeeder.DemoPassword);

        var response = await client.PostAsJsonAsync("/api/events", SampleEvent("Illegal Event"));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanCreateAndDeleteEvent()
    {
        var client = CreateClient();
        await Login(client, "john@jrr.co.za", DataSeeder.DemoPassword);

        var create = await client.PostAsJsonAsync("/api/events", SampleEvent("Soweto Community Run"));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<EventDto>(JsonOptions);
        Assert.Equal("Running", created!.EventType);
        Assert.Equal(10, created.DistanceKM);

        var del = await client.DeleteAsync($"/api/events/{created.EventId}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);
    }

    [Fact]
    public async Task Participant_CanEnrolAndOrganiser_CanViewEnrolmentsAndCaptureResult()
    {
        var organiser = CreateClient();
        await Login(organiser, "fatima@dca.co.za", DataSeeder.DemoPassword);

        var createdEvent = await (await organiser.PostAsJsonAsync("/api/events", new
        {
            name = "Test Coastal 5K",
            description = "Unit test event",
            eventDate = "2026-12-01",
            startTime = "07:00:00",
            location = "Durban",
            distanceKM = 5.0,
            eventType = "Walking",
            status = "Open"
        })).Content.ReadFromJsonAsync<EventDto>(JsonOptions);

        var category = await (await organiser.PostAsJsonAsync("/api/categories", new
        {
            name = "Test 5KM Walk",
            type = "Walking",
            distanceKM = 5.0,
            ageGroup = "Open",
            eventId = createdEvent!.EventId,
            entryFee = 50,
            maximumParticipants = 100
        })).Content.ReadFromJsonAsync<CategoryDto>(JsonOptions);

        var participant = CreateClient();
        await Login(participant, "thabo@example.com", DataSeeder.DemoPassword);

        var enrol = await participant.PostAsJsonAsync("/api/enrolments", new
        {
            eventId = createdEvent.EventId,
            categoryId = category!.CategoryId
        });
        Assert.Equal(HttpStatusCode.Created, enrol.StatusCode);
        var enrolment = await enrol.Content.ReadFromJsonAsync<EnrolmentDto>(JsonOptions);
        Assert.Equal(createdEvent.EventId, enrolment!.EventId);

        var own = await participant.GetAsync("/api/enrolments/me");
        own.EnsureSuccessStatusCode();

        var eventEnrolments = await organiser.GetAsync($"/api/events/{createdEvent.EventId}/enrolments");
        eventEnrolments.EnsureSuccessStatusCode();

        var resultResponse = await organiser.PostAsJsonAsync("/api/results", new
        {
            enrolmentId = enrolment.EnrolmentId,
            finishTime = "00:32:10",
            position = 4,
            pace = "6:26 min/km",
            resultStatus = "Finished"
        });
        Assert.Equal(HttpStatusCode.Created, resultResponse.StatusCode);

        var history = await participant.GetAsync("/api/participants/me/results");
        history.EnsureSuccessStatusCode();
        var results = await history.Content.ReadFromJsonAsync<List<ResultDto>>(JsonOptions);
        Assert.Contains(results!, r => r.EnrolmentId == enrolment.EnrolmentId);
    }

    [Fact]
    public async Task Organiser_CannotEnrolInEvent()
    {
        var client = CreateClient();
        await Login(client, "john@jrr.co.za", DataSeeder.DemoPassword);

        var response = await client.PostAsJsonAsync("/api/enrolments", new { eventId = 1, categoryId = 1 });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Logout_ClearsSession()
    {
        var client = CreateClient();
        await Login(client, "thabo@example.com", DataSeeder.DemoPassword);

        var logout = await client.PostAsync("/api/auth/logout", null);
        logout.EnsureSuccessStatusCode();

        var me = await client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }

    [Fact]
    public async Task Swagger_IsAvailable()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("/api/auth/login", json);
        Assert.Contains("/api/events", json);
        Assert.Contains("/api/enrolments", json);
        Assert.Contains("/api/results", json);
    }

    [Fact]
    public async Task PublicRouteAndWeather_ReturnSeededData()
    {
        var route = await _client.GetAsync("/api/events/1/route");
        route.EnsureSuccessStatusCode();
        var weather = await _client.GetAsync("/api/events/1/weather");
        weather.EnsureSuccessStatusCode();
        var environment = await _client.GetAsync("/api/events/1/environment");
        environment.EnsureSuccessStatusCode();
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new() { HandleCookies = true, AllowAutoRedirect = false });
    }

    private static async Task Login(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
    }

    private static object SampleEvent(string name) => new
    {
        name,
        description = "Test event",
        eventDate = "2026-12-12",
        startTime = "06:30:00",
        location = "Johannesburg",
        distanceKM = 10.0,
        eventType = "Running",
        status = "Upcoming"
    };
}
 