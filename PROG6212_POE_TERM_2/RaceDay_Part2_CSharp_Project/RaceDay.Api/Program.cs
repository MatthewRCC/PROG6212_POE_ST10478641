using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<RaceDayDb>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("RaceDayDb")));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o => {
    o.Cookie.Name = "RaceDay.Auth"; o.Cookie.HttpOnly = true; o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Events.OnRedirectToLogin = c => { c.Response.StatusCode = 401; return Task.CompletedTask; };
    o.Events.OnRedirectToAccessDenied = c => { c.Response.StatusCode = 403; return Task.CompletedTask; };
});
builder.Services.AddAuthorization();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection(); app.UseAuthentication(); app.UseAuthorization();
app.MapControllers();
app.Run();
public partial class Program { }

public enum UserRole { Participant = 1, Organiser = 2 }
public class AppUser {
    public int Id { get; set; }
    [Required, MaxLength(80)] public string FirstName { get; set; } = "";
    [Required, MaxLength(80)] public string LastName { get; set; } = "";
    [Required, EmailAddress, MaxLength(200)] public string Email { get; set; } = "";
    [Required] public string PasswordHash { get; set; } = "";
    public UserRole Role { get; set; } = UserRole.Participant;
    public ICollection<RaceEvent> OrganisedEvents { get; set; } = new List<RaceEvent>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
public class EventType { public int Id { get; set; } [Required] public string Name { get; set; } = ""; public string? Description { get; set; } public ICollection<RaceEvent> Events { get; set; } = new List<RaceEvent>(); }
public class RaceEvent {
    public int Id { get; set; } [Required, MaxLength(150)] public string Name { get; set; } = "";
    public string? Description { get; set; } [Required] public string Location { get; set; } = "";
    public DateTime EventDate { get; set; } public bool IsPublic { get; set; } = true;
    public int OrganiserId { get; set; } public AppUser? Organiser { get; set; }
    public int EventTypeId { get; set; } public EventType? EventType { get; set; }
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
public class Category { public int Id { get; set; } [Required, MaxLength(100)] public string Name { get; set; } = ""; public string? Description { get; set; } public int EventId { get; set; } public RaceEvent? Event { get; set; } public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>(); }
public class Enrollment { public int Id { get; set; } public int ParticipantId { get; set; } public AppUser? Participant { get; set; } public int EventId { get; set; } public RaceEvent? Event { get; set; } public int CategoryId { get; set; } public Category? Category { get; set; } public DateTime EnrolledAt { get; set; } = DateTime.UtcNow; public RaceResult? Result { get; set; } }
public class RaceResult { public int Id { get; set; } public int EnrollmentId { get; set; } public Enrollment? Enrollment { get; set; } public TimeSpan? FinishTime { get; set; } public int? Position { get; set; } public DateTime? RecordedAt { get; set; } }
public class RaceRoute { public int Id { get; set; } [Required] public string Name { get; set; } = ""; public string? Description { get; set; } public decimal DistanceKm { get; set; } public int EventId { get; set; } public RaceEvent? Event { get; set; } }
public class WeatherForecast { public int Id { get; set; } public int EventId { get; set; } public RaceEvent? Event { get; set; } public DateTime ForecastFor { get; set; } public decimal? TemperatureC { get; set; } public string? Summary { get; set; } }
public class RaceDayDb(DbContextOptions<RaceDayDb> options) : DbContext(options) {
    public DbSet<AppUser> Users => Set<AppUser>(); public DbSet<EventType> EventTypes => Set<EventType>();
    public DbSet<RaceEvent> Events => Set<RaceEvent>(); public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>(); public DbSet<RaceResult> Results => Set<RaceResult>();
    public DbSet<RaceRoute> Routes => Set<RaceRoute>(); public DbSet<WeatherForecast> WeatherForecasts => Set<WeatherForecast>();
    protected override void OnModelCreating(ModelBuilder b) {
        b.Entity<AppUser>().HasIndex(x => x.Email).IsUnique();
        b.Entity<AppUser>().Property(x => x.Role).HasConversion<string>();
        b.Entity<RaceEvent>().HasOne(x => x.Organiser).WithMany(x => x.OrganisedEvents).HasForeignKey(x => x.OrganiserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Enrollment>().HasIndex(x => new { x.ParticipantId, x.CategoryId }).IsUnique();
        b.Entity<RaceResult>().HasIndex(x => x.EnrollmentId).IsUnique();
        b.Entity<RaceResult>().HasOne(x => x.Enrollment).WithOne(x => x.Result).HasForeignKey<RaceResult>(x => x.EnrollmentId);
    }
}
