using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.Api.Controllers;
[ApiController, Route("api/events")]
public class EventsController(RaceDayDb db) : ControllerBase {
    public record EventDto(string Name, string? Description, string Location, DateTime EventDate, bool IsPublic, int EventTypeId);
    /// <summary>List public events.</summary>
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> List() => Ok(await db.Events.Where(e => e.IsPublic).Select(e => new { e.Id, e.Name, e.Description, e.Location, e.EventDate, e.EventTypeId }).ToListAsync());
    /// <summary>View a public event.</summary>
    [HttpGet("{id:int}"), AllowAnonymous]
    public async Task<IActionResult> Get(int id) {
        var e = await db.Events.Where(e => e.Id == id && e.IsPublic).Select(e => new { e.Id, e.Name, e.Description, e.Location, e.EventDate, e.EventTypeId, Categories = e.Categories.Select(c => new { c.Id, c.Name, c.Description }) }).SingleOrDefaultAsync();
        return e is null ? NotFound() : Ok(e);
    }
    /// <summary>Create an event as an organiser.</summary>
    [HttpPost, Authorize(Roles = nameof(UserRole.Organiser))]
    public async Task<IActionResult> Create(EventDto dto) {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid)) return Unauthorized();
        if (!await db.EventTypes.AnyAsync(x => x.Id == dto.EventTypeId)) return BadRequest(new { message = "Unknown event type." });
        var e = new RaceEvent { Name = dto.Name.Trim(), Description = dto.Description, Location = dto.Location.Trim(), EventDate = dto.EventDate, IsPublic = dto.IsPublic, EventTypeId = dto.EventTypeId, OrganiserId = uid };
        db.Events.Add(e); await db.SaveChangesAsync(); return CreatedAtAction(nameof(Get), new { id = e.Id }, new { e.Id, e.Name });
    }
    /// <summary>Update an event owned by the signed-in organiser.</summary>
    [HttpPut("{id:int}"), Authorize(Roles = nameof(UserRole.Organiser))]
    public async Task<IActionResult> Update(int id, EventDto dto) {
        var e = await db.Events.FindAsync(id); if (e is null) return NotFound();
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid)) return Unauthorized();
        if (e.OrganiserId != uid) return Forbid();
        if (!await db.EventTypes.AnyAsync(x => x.Id == dto.EventTypeId)) return BadRequest(new { message = "Unknown event type." });
        e.Name = dto.Name.Trim(); e.Description = dto.Description; e.Location = dto.Location.Trim(); e.EventDate = dto.EventDate; e.IsPublic = dto.IsPublic; e.EventTypeId = dto.EventTypeId;
        await db.SaveChangesAsync(); return NoContent();
    }
    /// <summary>Delete an event owned by the signed-in organiser.</summary>
    [HttpDelete("{id:int}"), Authorize(Roles = nameof(UserRole.Organiser))]
    public async Task<IActionResult> Delete(int id) {
        var e = await db.Events.FindAsync(id); if (e is null) return NotFound();
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid)) return Unauthorized();
        if (e.OrganiserId != uid) return Forbid();
        db.Events.Remove(e); await db.SaveChangesAsync(); return NoContent();
    }
}
