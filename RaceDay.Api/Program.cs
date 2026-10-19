using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RaceDay.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "RaceDay.Session";
    options.Cookie.SameSite = SameSiteMode.Lax;
});
// Program.cs (before AddDbContext)
Console.WriteLine("Effective DefaultConnection: " + builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Services.AddDbContext<RaceDayContext>(options =>
{
    if (builder.Environment.IsEnvironment("Testing") || builder.Configuration.GetValue<bool>("UseInMemoryDatabase"))
    {
        options.UseInMemoryDatabase(builder.Configuration["InMemoryDatabaseName"] ?? "RaceDayDb");
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = """
                      RESTful API for RaceDay, a South African road running, walking and cycling event platform.
                      Role-based session authentication is used: POST /api/auth/login issues a session cookie
                      (RaceDay.Session). Use Swagger's Try it out after logging in to call protected endpoints.
                      Roles: Organiser (manage events, categories, results) and Participant (enrol and view own results).
                      """
    });
    options.EnableAnnotations();
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RaceDayContext>();
    db.Database.EnsureCreated();
    DataSeeder.Seed(db);
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1");
    options.RoutePrefix = "swagger";
});

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseSession();
app.MapControllers();
app.Run();

public partial class Program;
