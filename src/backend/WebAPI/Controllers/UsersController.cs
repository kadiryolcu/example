using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    /// <summary>
    /// Mevcut kullanıcının bilgilerini Keycloak üzerinden alır
    /// </summary>
    [HttpGet("me")]
    [AllowAnonymous] // Keycloak çalışmadığında test edilebilmek için geçici olarakAllowAnonymous
    public IActionResult GetCurrentUser()
    {
        // Eğer kullanıcı authenticated değilse mock data döndür
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Ok(new
            {
                Id = "mock-user-id",
                Email = "mock@example.com",
                Name = "Mock User",
                FirstName = "Mock",
                LastName = "User",
                Roles = new[] { "User" },
                IsAuthenticated = false,
                Message = "Keycloak sunucusu çalışmıyor - mock veri döndürülüyor"
            });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var name = User.FindFirst(ClaimTypes.Name)?.Value;
        var firstName = User.FindFirst("given_name")?.Value;
        var lastName = User.FindFirst("family_name")?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

        return Ok(new
        {
            Id = userId,
            Email = email,
            Name = name,
            FirstName = firstName,
            LastName = lastName,
            Roles = roles,
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false
        });
    }

    /// <summary>
    /// Kullanıcının rollerini döndürür
    /// </summary>
    [HttpGet("roles")]
    [Authorize]
    public IActionResult GetUserRoles()
    {
        var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
        return Ok(roles);
    }

    /// <summary>
    /// Kullanıcının belirli bir role sahip olup olmadığını kontrol eder
    /// </summary>
    [HttpGet("has-role/{role}")]
    [Authorize]
    public IActionResult HasRole([FromRoute] string role)
    {
        var hasRole = User.IsInRole(role);
        return Ok(new { HasRole = hasRole });
    }
}
