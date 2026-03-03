using DeviceManagement.Api.AppSettings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DeviceManagement.Api.Controllers;

[ApiController]
[Route("api/Auth")]
[Tags("Auth")]
public class AuthController(IOptions<JwtSettings> jwtOptions, ILogger<AuthController> logger) : ControllerBase
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));

    public record UserLogin(string Username, string Password);

    /// <summary>
    /// Authenticates user and returns a JWT
    /// </summary>
    /// <param name="login">User credentials</param>
    /// <response code="200">JWT token generated successfully</response>
    /// <response code="400">Bad request, check error(s).</response>
    /// <response code="401">Invalid credentials.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Login([FromBody] UserLogin login)
    {
        if (string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
        {
            return BadRequest(new { Error = "Username and password are required" });
        }

        // ──────────────────────────────────────────────────────────────────────────────-──-
        // TODO: REPLACE WITH REAL AUTH LOGIC (database + password hash verification)
        // ────────────────────────────────────────────────────────────────────────────-──-─-
        var credentialsValid = login.Username == "test" && login.Password == "password01!";
        if (!credentialsValid)
        {
            logger.LogWarning("Failed login attempt for username: {Username}", login.Username);
            return Unauthorized();
        }

        // ────────────────────────────────────────────────
        //          JWT Configuration
        // ────────────────────────────────────────────────
        if (!_jwtSettings.ExpirationInMinutes.HasValue || _jwtSettings.ExpirationInMinutes <= 0)
        {
            _jwtSettings.ExpirationInMinutes = 30; // fallback
        }
        if (string.IsNullOrEmpty(_jwtSettings.Issuer) || string.IsNullOrEmpty(_jwtSettings.Audience) || string.IsNullOrEmpty(_jwtSettings.Secret))
        {
            logger.LogError("JWT configuration is missing required values");
            return StatusCode(500, new { Error = "Server configuration error" });
        }

        var secretBytes = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
        if (secretBytes.Length < 32)
        {
            logger.LogCritical("JWT secret is too weak (shorter than 256 bits)");
            return StatusCode(500, new { Error = "Server configuration error" });
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, login.Username),
            new Claim(JwtRegisteredClaimNames.UniqueName, login.Username),
            new Claim("email", "test@test.com"), // replace with real data
            new Claim(ClaimTypes.Role, "Admin"), // replace with real roles (from database for example)
            new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes!.Value),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(secretBytes), SecurityAlgorithms.HmacSha256)
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new
        {
            Token = jwt,
            ExpiresInMinutes = _jwtSettings.ExpirationInMinutes.Value,
            TokenType = JwtBearerDefaults.AuthenticationScheme
        });
    }
}