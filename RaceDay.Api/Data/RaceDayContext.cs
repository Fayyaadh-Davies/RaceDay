using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
using Route = RaceDay.Api.Models.Route;
using Environment = RaceDay.Api.Models.Environment;

namespace RaceDay.Api.Data;

public class RaceDayContext : DbContext
{
    public RaceDayContext(DbContextOptions<RaceDayContext> options) : base(options)
    {
    }

    public DbSet<Organisation> Organisations => Set<Organisation>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Environment> Environments => Set<Environment>();
    public DbSet<Weather> WeatherRecords => Set<Weather>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organisation>(entity =>
        {
            entity.ToTable("Organisation");
            entity.HasKey(e => e.OrganisationID);
            entity.HasIndex(e => e.RegistrationNumber).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(e => e.UserID);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasOne(e => e.Organisation)
                .WithMany(o => o.Users)
                .HasForeignKey(e => e.OrganisationID)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.ToTable("Participant");
            entity.HasKey(e => e.ParticipantID);
            entity.HasIndex(e => e.UserID).IsUnique();
            entity.HasOne(e => e.User)
                .WithOne(u => u.Participant)
                .HasForeignKey<Participant>(e => e.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");
            entity.HasKey(e => e.CategoryID);
            entity.Property(e => e.DistanceKM).HasPrecision(6, 2);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Event");
            entity.HasKey(e => e.EventID);
            entity.Property(e => e.DistanceKM).HasPrecision(6, 2);
            entity.HasOne(e => e.Organisation)
                .WithMany(o => o.Events)
                .HasForeignKey(e => e.OrganisationID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EventCategory>(entity =>
        {
            entity.ToTable("EventCategory");
            entity.HasKey(e => e.EventCategoryID);
            entity.HasIndex(e => new { e.EventID, e.CategoryID }).IsUnique();
            entity.Property(e => e.EntryFee).HasPrecision(10, 2);
            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.EventCategories)
                .HasForeignKey(e => e.EventID)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Category)
                .WithMany(c => c.EventCategories)
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.ToTable("Enrolment");
            entity.HasKey(e => e.EnrolmentID);
            entity.HasIndex(e => new { e.ParticipantID, e.EventID }).IsUnique();
            entity.HasOne(e => e.Participant)
                .WithMany(p => p.Enrolments)
                .HasForeignKey(e => e.ParticipantID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.Enrolments)
                .HasForeignKey(e => e.EventID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.EventCategory)
                .WithMany(ec => ec.Enrolments)
                .HasForeignKey(e => e.EventCategoryID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.ToTable("Result");
            entity.HasKey(e => e.ResultID);
            entity.HasIndex(e => e.EnrolmentID).IsUnique();
            entity.HasOne(e => e.Enrolment)
                .WithOne(en => en.Result)
                .HasForeignKey<Result>(e => e.EnrolmentID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Route>(entity =>
        {
            entity.ToTable("Route");
            entity.HasKey(e => e.RouteID);
            entity.HasIndex(e => e.EventID).IsUnique();
            entity.Property(e => e.DistanceKM).HasPrecision(6, 2);
            entity.HasOne(e => e.Event)
                .WithOne(ev => ev.Route)
                .HasForeignKey<Route>(e => e.EventID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Environment>(entity =>
        {
            entity.ToTable("Environment");
            entity.HasKey(e => e.EnvironmentID);
            entity.HasIndex(e => e.EventID).IsUnique();
            entity.Property(e => e.TemperatureC).HasPrecision(5, 2);
            entity.Property(e => e.HumidityPercent).HasPrecision(5, 2);
            entity.Property(e => e.WindSpeedKMH).HasPrecision(6, 2);
            entity.HasOne(e => e.Event)
                .WithOne(ev => ev.Environment)
                .HasForeignKey<Environment>(e => e.EventID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Weather>(entity =>
        {
            entity.ToTable("Weather");
            entity.HasKey(e => e.WeatherID);
            entity.Property(e => e.TemperatureC).HasPrecision(5, 2);
            entity.Property(e => e.WindSpeedKMH).HasPrecision(6, 2);
            entity.Property(e => e.RainProbability).HasPrecision(5, 2);
            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.WeatherRecords)
                .HasForeignKey(e => e.EventID)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
