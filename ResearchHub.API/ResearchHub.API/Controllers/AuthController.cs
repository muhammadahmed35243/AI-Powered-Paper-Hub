using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ResearchHub.API.Data;
using ResearchHub.API.Models;
using ResearchHub.API.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ResearchHub.API.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>()
                    {
                        _configuration["Google:ClientId"]
                            ?? _configuration["Authentication:Google:ClientId"]
                            ?? Environment.GetEnvironmentVariable("Google__ClientId")
                            ?? Environment.GetEnvironmentVariable("Authentication__Google__ClientId")
                            ?? string.Empty
                    }
                };

                // Validate the Google ID Token
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);

                // Check if user exists
                var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == payload.Subject || u.Email == payload.Email);

                if (user == null)
                {
                    user = new User
                    {
                        Email = payload.Email,
                        Name = payload.Name,
                        GoogleId = payload.Subject,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Users.Add(user);
                }
                else
                {
                    user.GoogleId ??= payload.Subject;
                    user.Name = payload.Name;
                }

                var adminEmails = _configuration.GetSection("Admin:Emails").Get<string[]>() ?? Array.Empty<string>();
                if (adminEmails.Contains(payload.Email, StringComparer.OrdinalIgnoreCase))
                    user.IsAdmin = true;

                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);

                return Ok(new AuthResponse
                {
                    Token = token,
                    Email = user.Email,
                    Name = user.Name,
                    UserId = user.UserId
                });
            }
            catch (InvalidJwtException)
            {
                return Unauthorized(new { message = "Invalid Google token." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred during authentication." });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout() => Ok(new { message = "Logged out. Discard the JWT on the client." });

        [Authorize]
        [HttpGet("user-profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .Include(u => u.Interests)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return NotFound();

            return Ok(new
            {
                user.UserId,
                user.Name,
                user.Email,
                user.ResearchInterests,
                Interests = user.Interests.Select(i => new { i.Topic, i.InterestLevel })
            });
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Name, user.Name),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            if (user.IsAdmin)
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims.ToArray(),
                expires: DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"] ?? "7")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
