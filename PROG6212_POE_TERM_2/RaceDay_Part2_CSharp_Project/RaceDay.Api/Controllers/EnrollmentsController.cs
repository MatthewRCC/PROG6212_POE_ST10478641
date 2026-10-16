using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.Api.Controllers;
[ApiController, Route("api")]
[Authorize]
public class EnrollmentsController(RaceDayDb db) : ControllerBase {
    public record EnrolDto(int CategoryId);
    public record ResultDto(TimeSpan? FinishTime, int? Position);
    /// <summary>Enrol yourself as a participant in a category.</summary>
    [HttpPost("events/{eventId:int}/enrollments"), Authorize(Roles = nameof(UserRole.Participant))]
    public async Task<IActionResult> Enrol(int eventId, EnrolDto dto) {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid)) return Unauthorized();
        var c = await db.Categories.Include(x => x.Event).SingleOrDefaultAsync(x => x.Id == dto.CategoryId && x.EventId == eventId);
        if (c?.Event is null || !c.Event.IsPublic) return NotFound();
        if (await db.Enrollments.AnyAsync(x => x.ParticipantId == uid && x.CategoryId == c.Id)) return Conflict(new { message = "Already enrolled." });
        var en = new Enrollment { ParticipantId = uid, EventId = eventId, CategoryId = c.Id };
        db.Enrollments.Add(en); await db.SaveChangesAsync(); return Created("/api/users/me/results", new { en.Id, en.EventId, en.CategoryId });
    }
    /// <summary>View enrolments for an event you organise.</summary>
    [HttpGet("organiser/events/{eventId:int}/enrollments"), Authorize(Roles = nameof(UserRole.Organiser))]
    public async Task<IActionResult> Enrolments(int eventId) {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid)) return Unauthorized();
        var e = await db.Events.FindAsync(eventId); if (e is null) return NotFound(); if (e.OrganiserId != uid) return Forbid();
        return Ok(await db.Enrollments.Where(x => x.EventId == eventId).Select(x => new { x.Id, x.CategoryId, Category = x.Category!.Name, ParticipantId = x.ParticipantId, Participant = x.Participant!.FirstName + " " + x.Participant.LastName, Email = x.Participant.Email, Result = x.Result == null ? null : new { x.Result.FinishTime, x.Result.Position } }).ToListAsync());
    }
    /// <summary>Record or update a participant's finish time and position for your event.</summary>
    [HttpPut("organiser/enrollments/{enrollmentId:int}/result"), Authorize(Roles = nameof(UserRole.Organiser))]
    public async Task<IActionResult> RecordResult(int enrollmentId, ResultDto dto) {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid)) return Unauthorized();
        if (dto.FinishTime < TimeSpan.Zero || dto.Position < 1) return BadRequest(new { message = "Finish time must be non-negative and position must be at least 1." });
        var en = await db.Enrollments.Include(x => x.Event).Include(x => x.Result).SingleOrDefaultAsync(x => x.Id == enrollmentId);
        if (en is null) return NotFound(); if (en.Event!.OrganiserId != uid) return Forbid();
        var r = en.Result ?? new RaceResult { EnrollmentId = enrollmentId };
        r.FinishTime = dto.FinishTime; r.Position = dto.Position; r.RecordedAt = DateTime.UtcNow;
        if (en.Result is null) db.Results.Add(r);
        await db.SaveChangesAsync(); return Ok(new { r.Id, r.EnrollmentId, r.FinishTime, r.Position, r.RecordedAt });
    }
    /// <summary>View only your own race results.</summary>
    [HttpGet("users/me/results"), Authorize(Roles = nameof(UserRole.Participant))]
    public async Task<IActionResult> MyResults() {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid)) return Unauthorized();
        return Ok(await db.Results.Where(x => x.Enrollment!.ParticipantId == uid).Select(x => new { x.Id, x.FinishTime, x.Position, x.RecordedAt, EventName = x.Enrollment!.Event!.Name, Category = x.Enrollment.Category!.Name }).ToListAsync());
    }
}
