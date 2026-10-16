using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(RaceDayDb db, IPasswordHasher<AppUser> hasher) : ControllerBase {
    public record RegisterDto(string FirstName, string LastName, string Email, string Password, UserRole Role);
    public record LoginDto(string Email, string Password);
    public record ProfileDto(int Id, string FirstName, string LastName, string Email, string Role);

    /// <summary>Register an organiser or participant account. Password is securely hashed.</summary>
    [HttpPost("register"), AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDto dto) {
        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName) ||
            string.IsNullOrWhiteSpace(dto.Email) || dto.Password.Length < 8 || !Enum.IsDefined(dto.Role))
            return BadRequest(new { message = "Provide names, a valid email, a password of at least 8 characters, and a valid role." });
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email)) return Conflict(new { message = "Email already registered." });
        var user = new AppUser { FirstName = dto.FirstName.Trim(), LastName = dto.LastName.Trim(), Email = email, Role = dto.Role };
        user.PasswordHash = hasher.HashPassword(user, dto.Password);
        db.Users.Add(user); await db.SaveChangesAsync();
        return Created("/api/users/me", Safe(user));
    }

    /// <summary>Log in and issue an HttpOnly cookie.</summary>
    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto dto) {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == dto.Email.Trim().ToLowerInvariant());
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Email or password is incorrect." });
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.Email), new Claim(ClaimTypes.Role, user.Role.ToString()) };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });
        return Ok(Safe(user));
    }
    /// <summary>Log out the current user.</summary>
    [HttpPost("logout"), Authorize]
    public async Task<IActionResult> Logout() { await HttpContext.SignOutAsync(); return NoContent(); }
    public static ProfileDto Safe(AppUser u) => new(u.Id, u.FirstName, u.LastName, u.Email, u.Role.ToString());
}
