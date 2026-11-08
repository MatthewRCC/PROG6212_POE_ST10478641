using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/routes")]
public class RoutesController(RaceDayDb db) : ControllerBase
{
    public record RouteRequest(string Name, string? Description, decimal DistanceKm);

    // Anyone can view routes for a public event.
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoutes(int eventId)
    {
        var isPublic = await db.Events.AnyAsync(e => e.Id == eventId && e.IsPublic);
        if (!isPublic) return NotFound();

        var routes = await db.Routes
            .Where(r => r.EventId == eventId)
            .OrderBy(r => r.Name)
            .Select(r => new { r.Id, r.Name, r.Description, r.DistanceKm, r.EventId })
            .ToListAsync();

        return Ok(routes);
    }

    // Only the organiser who owns the event can add a route.
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Organiser))]
    public async Task<IActionResult> Create(int eventId, RouteRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.DistanceKm <= 0)
            return BadRequest(new { message = "A route name and distance greater than zero are required." });

        var raceEvent = await db.Events.FindAsync(eventId);
        if (raceEvent is null) return NotFound();

        if (!TryGetUserId(out var userId)) return Unauthorized();
        if (raceEvent.OrganiserId != userId) return Forbid();

        var route = new RaceRoute
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            DistanceKm = dto.DistanceKm,
            EventId = eventId
        };

        db.Routes.Add(route);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRoutes), new { eventId }, new
        {
            route.Id, route.Name, route.Description, route.DistanceKm, route.EventId
        });
    }

    [HttpPut("{routeId:int}")]
    [Authorize(Roles = nameof(UserRole.Organiser))]
    public async Task<IActionResult> Update(int eventId, int routeId, RouteRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.DistanceKm <= 0)
            return BadRequest(new { message = "A route name and distance greater than zero are required." });

        var route = await db.Routes
            .Include(r => r.Event)
            .SingleOrDefaultAsync(r => r.Id == routeId && r.EventId == eventId);

        if (route is null) return NotFound();
        if (!TryGetUserId(out var userId)) return Unauthorized();
        if (route.Event!.OrganiserId != userId) return Forbid();

        route.Name = dto.Name.Trim();
        route.Description = dto.Description?.Trim();
        route.DistanceKm = dto.DistanceKm;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{routeId:int}")]
    [Authorize(Roles = nameof(UserRole.Organiser))]
    public async Task<IActionResult> Delete(int eventId, int routeId)
    {
        var route = await db.Routes
            .Include(r => r.Event)
            .SingleOrDefaultAsync(r => r.Id == routeId && r.EventId == eventId);

        if (route is null) return NotFound();
        if (!TryGetUserId(out var userId)) return Unauthorized();
        if (route.Event!.OrganiserId != userId) return Forbid();

        db.Routes.Remove(route);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private bool TryGetUserId(out int userId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
