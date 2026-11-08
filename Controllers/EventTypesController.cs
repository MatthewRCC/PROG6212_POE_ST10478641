using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.Api.Controllers;

// Public read-only list of supported race/event types.
[ApiController]
[Route("api/event-types")]
public class EventTypesController(RaceDayDb db) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var types = await db.EventTypes
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.Description })
            .ToListAsync();

        return Ok(types);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var type = await db.EventTypes
            .Where(x => x.Id == id)
            .Select(x => new { x.Id, x.Name, x.Description })
            .SingleOrDefaultAsync();

        return type is null ? NotFound() : Ok(type);
    }
}
