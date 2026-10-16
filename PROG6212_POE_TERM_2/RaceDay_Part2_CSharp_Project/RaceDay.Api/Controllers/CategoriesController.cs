using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.Api.Controllers;
[ApiController, Route("api/events/{eventId:int}/categories")]
public class CategoriesController(RaceDayDb db) : ControllerBase {
    public record CategoryDto(string Name, string? Description);
    /// <summary>List categories of a public event.</summary>
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> List(int eventId) {
        if (!await db.Events.AnyAsync(e => e.Id == eventId && e.IsPublic)) return NotFound();
        return Ok(await db.Categories.Where(c => c.EventId == eventId).Select(c => new { c.Id, c.Name, c.Description, c.EventId }).ToListAsync());
    }
    /// <summary>Create a category for an event owned by this organiser.</summary>
    [HttpPost, Authorize(Roles = nameof(UserRole.Organiser))]
    public async Task<IActionResult> Create(int eventId, CategoryDto dto) {
        var e = await db.Events.FindAsync(eventId); if (e is null) return NotFound();
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid)) return Unauthorized();
        if (e.OrganiserId != uid) return Forbid();
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest();
        var c = new Category { Name = dto.Name.Trim(), Description = dto.Description, EventId = eventId };
        db.Categories.Add(c); await db.SaveChangesAsync(); return CreatedAtAction(nameof(List), new { eventId }, new { c.Id, c.Name, c.Description });
    }
    /// <summary>Update a category belonging to an event owned by this organiser.</summary>
    [HttpPut("{categoryId:int}"), Authorize(Roles = nameof(UserRole.Organiser))]
    public async Task<IActionResult> Update(int eventId, int categoryId, CategoryDto dto) {
        var c = await db.Categories.Include(x => x.Event).SingleOrDefaultAsync(x => x.Id == categoryId && x.EventId == eventId);
        if (c is null) return NotFound();
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid)) return Unauthorized();
        if (c.Event!.OrganiserId != uid) return Forbid();
        c.Name = dto.Name.Trim(); c.Description = dto.Description; await db.SaveChangesAsync(); return NoContent();
    }
    /// <summary>Delete a category belonging to an event owned by this organiser.</summary>
    [HttpDelete("{categoryId:int}"), Authorize(Roles = nameof(UserRole.Organiser))]
    public async Task<IActionResult> Delete(int eventId, int categoryId) {
        var c = await db.Categories.Include(x => x.Event).SingleOrDefaultAsync(x => x.Id == categoryId && x.EventId == eventId);
        if (c is null) return NotFound();
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid)) return Unauthorized();
        if (c.Event!.OrganiserId != uid) return Forbid();
        db.Categories.Remove(c); await db.SaveChangesAsync(); return NoContent();
    }
}
