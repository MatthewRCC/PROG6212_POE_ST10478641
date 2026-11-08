using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/weather")]
public class WeatherForecastsController(RaceDayDb db) : ControllerBase
{
    public record WeatherRequest(DateTime ForecastFor, decimal? TemperatureC, string? Summary);

    // Public weather forecast information for public events.
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetForecasts(int eventId)
    {
        var isPublic = await db.Events.AnyAsync(e => e.Id == eventId && e.IsPublic);
        if (!isPublic) return NotFound();

        var forecasts = await db.WeatherForecasts
            .Where(w => w.EventId == eventId)
            .OrderBy(w => w.ForecastFor)
            .Select(w => new
            {
                w.Id, w.EventId, w.ForecastFor, w.TemperatureC, w.Summary
            })
            .ToListAsync();

        return Ok(forecasts);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Organiser))]
    public async Task<IActionResult> Create(int eventId, WeatherRequest dto)
    {
        var raceEvent = await db.Events.FindAsync(eventId);
        if (raceEvent is null) return NotFound();

        if (!TryGetUserId(out var userId)) return Unauthorized();
        if (raceEvent.OrganiserId != userId) return Forbid();

        if (dto.ForecastFor == default)
            return BadRequest(new { message = "A forecast date and time are required." });

        var forecast = new WeatherForecast
        {
            EventId = eventId,
            ForecastFor = dto.ForecastFor,
            TemperatureC = dto.TemperatureC,
            Summary = dto.Summary?.Trim()
        };

        db.WeatherForecasts.Add(forecast);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetForecasts), new { eventId }, new
        {
            forecast.Id, forecast.EventId, forecast.ForecastFor,
            forecast.TemperatureC, forecast.Summary
        });
    }

    [HttpPut("{forecastId:int}")]
    [Authorize(Roles = nameof(UserRole.Organiser))]
    public async Task<IActionResult> Update(int eventId, int forecastId, WeatherRequest dto)
    {
        var forecast = await db.WeatherForecasts
            .Include(w => w.Event)
            .SingleOrDefaultAsync(w => w.Id == forecastId && w.EventId == eventId);

        if (forecast is null) return NotFound();
        if (!TryGetUserId(out var userId)) return Unauthorized();
        if (forecast.Event!.OrganiserId != userId) return Forbid();

        if (dto.ForecastFor == default)
            return BadRequest(new { message = "A forecast date and time are required." });

        forecast.ForecastFor = dto.ForecastFor;
        forecast.TemperatureC = dto.TemperatureC;
        forecast.Summary = dto.Summary?.Trim();

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{forecastId:int}")]
    [Authorize(Roles = nameof(UserRole.Organiser))]
    public async Task<IActionResult> Delete(int eventId, int forecastId)
    {
        var forecast = await db.WeatherForecasts
            .Include(w => w.Event)
            .SingleOrDefaultAsync(w => w.Id == forecastId && w.EventId == eventId);

        if (forecast is null) return NotFound();
        if (!TryGetUserId(out var userId)) return Unauthorized();
        if (forecast.Event!.OrganiserId != userId) return Forbid();

        db.WeatherForecasts.Remove(forecast);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private bool TryGetUserId(out int userId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
