using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Controllers;

namespace RaceDay.Api.Controllers;
[ApiController, Route("api/users"), Authorize]
public class UsersController(RaceDayDb db) : ControllerBase {
    public record UpdateDto(string FirstName, string LastName, string Email);
    /// <summary>View your own profile.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> Me() {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return Unauthorized();
        var u = await db.Users.FindAsync(id); return u is null ? NotFound() : Ok(AuthController.Safe(u));
    }
    /// <summary>Update your own profile.</summary>
    [HttpPut("me")]
    public async Task<IActionResult> Update(UpdateDto dto) {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return Unauthorized();
        var u = await db.Users.FindAsync(id); if (u is null) return NotFound();
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email && x.Id != id)) return Conflict(new { message = "Email already in use." });
        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName) || string.IsNullOrWhiteSpace(email)) return BadRequest();
        u.FirstName = dto.FirstName.Trim(); u.LastName = dto.LastName.Trim(); u.Email = email;
        await db.SaveChangesAsync(); return Ok(AuthController.Safe(u));
    }
}
