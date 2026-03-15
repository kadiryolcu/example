using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    /// <summary>
    /// Keycloak ile kullanıcı girişi bilgilerini döndürür
    /// </summary>
    [HttpGet("userinfo")]
    [Authorize] // Gerçek authentication için authorize geri al
    public IActionResult GetUserInfo()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var name = User.FindFirst(ClaimTypes.Name)?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

        return Ok(new
        {
            UserId = userId,
            Email = email,
            Name = name,
            Roles = roles,
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false
        });
    }

    /// <summary>
    /// Kullanıcının giriş yapılıp yapılmadığını kontrol eder
    /// </summary>
    [HttpGet("check-auth")]
    [Authorize] // Gerçek authentication için authorize geri al
    public IActionResult CheckAuth()
    {
        return Ok(new { IsAuthenticated = true });
    }

    /// <summary>
    /// Çıkış yapar (client tarafında token temizlenmeli)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // Keycloak logout URL'ini döndür
        var logoutUrl = "http://localhost:8080/realms/master/protocol/openid-connect/logout";
        return Ok(new { LogoutUrl = logoutUrl });
    }
}
